// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using AgentInfrastructure.Logical;
using SemanticCompilerAdapter;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;

// Перенос измеренного результата опыта сто пятнадцать: словесная статистика обученного
// корпуса считается один раз на версию корпуса, а не заново для каждого кандидата.
//
// Что было неправильно. Словесная часть оценки кандидатов вызывалась внутри цикла по
// кандидатам, а внутри неё весь корпус приводился к корням ДВАЖДЫ: ради средней длины
// документа и ради частоты документов на каждое слово запроса. Доказательство по коду:
// 18.CapabilityTraining/LlmIngredientLogicalUnits.cs, вызов словесной оценки стоял в цикле
// по кандидатам, а прежние строки 368 и 377 перебирали всю память примеров с приведением
// каждого текста к корням.
//
// Что измерил опыт на четырёх документах и четырёх запросах: 156 приведений текста к
// корням против 4, символов 1988 против 88, байт 159 344 против 28 336, времени 14,2
// против 1,0 миллисекунды, при совпадении оценок и порядка кандидатов бит в бит
// (наибольшее расхождение ровно ноль на 16 сравнениях).
//
// Что доказывают проверки ниже и чего они НЕ доказывают. Они доказывают: совпадение самих
// статистик с прежними выражениями, пересборку снимка при новом уроке, отказ по отпечатку
// содержимого при РАВНОМ числе документов, законную смену ответа обычного читателя и
// работу на корпусе из одного документа. Они НЕ перемеряют выигрыш в приведениях, символах,
// байтах и времени: это измерение опыта на его копии логики, а не утверждение этих проверок.
//
// Прогона не было: прямые команды сборки и прогона тестов запрещены политикой разрешений
// проекта, и обходить запрет оболочкой-посредником я не стал.
public sealed class LexicalCorpusStatisticsTests
{
    // Четыре факта проекта Cedar: тот же набор DOC-FACT, на котором считал опыт.
    private const string OwnerId = "DOC-01";
    private const string OwnerText = "Cedar owner";
    private const string StatusId = "DOC-02";
    private const string StatusText = "Cedar status";
    private const string ReleaseId = "DOC-03";
    private const string ReleaseText = "Cedar release";
    private const string LanguageId = "DOC-04";
    private const string LanguageText = "Cedar language";

    // Ответ урока о состоянии проекта до и после исправления.
    private const string StatusBeforeCorrection = "approved";
    private const string StatusAfterCorrection = "cancelled";

    // Один обученный словарь на весь класс: обучение памяти единицы его не меняет.
    private static readonly Lazy<BpeTokenizer> SharedTokenizer = new(() =>
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train("Cedar owner status release language approved cancelled Dana Friday project fact question", 320);
        return tokenizer;
    });

    // Одна крошечная модель на весь класс: обучение единицы её не меняет, по ней только
    // считаются векторы.
    private static readonly Lazy<LlmModel> SharedModel = new(() => new LlmModel(new ModelConfig
    {
        VocabSize = 512,
        ContextLen = 64,
        HiddenDim = 16,
        NumLayers = 1,
        NumHeads = 4,
        NumKvHeads = 2,
        FfnHiddenDim = 32
    }));

    // Проверка 1, положительный контроль: статистика снимка совпадает с тем, что давали
    // прежние выражения на том же наборе. Зелёная и до переноса, и после: прежний код
    // считал ровно эти же величины, только заново на каждого кандидата.
    [Fact]
    public void SnapshotStatistics_MatchThePreviousExpressions_OnTheSameCorpus()
    {
        // Тот же корпус из четырёх документов.
        var documents = CedarCorpus(StatusBeforeCorrection);
        var built = LexicalCorpusStatistics.Build(documents);
        Assert.True(built.IsSuccess, built.ErrorMessage);
        var statistics = built.Value!;

        // Прежнее выражение средней длины документа, переписанное в проверку дословно:
        // сумма частот слов каждого документа без нижней границы, затем среднее, затем
        // нижняя граница в единицу.
        var previousAverageLength = Math.Max(
            1.0,
            documents.Select(document => PreviousTokenize(document.Text).Values.Sum()).DefaultIfEmpty(1).Average());
        // Среднее совпадает точно, а не приблизительно.
        Assert.Equal(previousAverageLength, statistics.AverageLength);
        // Число документов то же.
        Assert.Equal(documents.Length, statistics.DocumentCount);

        foreach (var document in documents)
        {
            // Документ лежит в снимке под своим опознанием.
            Assert.True(statistics.TryGetDocument(document.ExampleId, out var stored));
            // Прежние слова документа и их частоты.
            var previousTokens = PreviousTokenize(document.Text);
            // Длина документа совпадает с прежней, включая нижнюю границу в единицу.
            Assert.Equal(Math.Max(1, previousTokens.Values.Sum()), stored.Length);
            // Слова и частоты совпадают поимённо, а не только числом.
            Assert.Equal(previousTokens.Count, stored.Tokens.Count);
            foreach (var pair in previousTokens)
            {
                Assert.True(stored.Tokens.TryGetValue(pair.Key, out var frequency));
                Assert.Equal(pair.Value, frequency);
            }
        }

        // Частота документов совпадает с прежним пересчётом по всему корпусу для каждого
        // слова, которое в корпусе есть.
        foreach (var token in documents.SelectMany(document => PreviousTokenize(document.Text).Keys).Distinct())
        {
            var previousDocumentFrequency = documents.Count(document => PreviousTokenize(document.Text).ContainsKey(token));
            Assert.Equal(previousDocumentFrequency, statistics.GetDocumentFrequency(token));
        }

        // Слово, которого в корпусе нет, даёт ноль документов, ровно как прежний пересчёт.
        Assert.Equal(0, statistics.GetDocumentFrequency("sequoia"));
    }

    // Проверка 2: новый урок пересобирает снимок. Исправление поменяло только ответ, поэтому
    // средняя длина и частота документов честно остались прежними, и единственное поле,
    // отличившее версии, — отпечаток содержимого. Число документов при этом РАВНОЕ.
    [Fact]
    public void NewLesson_RebuildsTheSnapshot_AndOnlyTheContentFingerprintSeparatesTheVersions()
    {
        // Версия корпуса до исправления и после него: тексты те же, ответ урока другой.
        var before = BuiltStatistics(CedarCorpus(StatusBeforeCorrection));
        var after = BuiltStatistics(CedarCorpus(StatusAfterCorrection));

        // Число документов не изменилось: именно поэтому счётчик документов негодный судья.
        Assert.Equal(before.DocumentCount, after.DocumentCount);
        // Средняя длина не изменилась: исправление не трогало тексты. Это честный результат
        // опыта, а не выдуманный контраст.
        Assert.Equal(before.AverageLength, after.AverageLength);
        // Частота документов тоже не изменилась ни по одному слову корпуса.
        foreach (var token in CedarCorpus(StatusBeforeCorrection).SelectMany(document => PreviousTokenize(document.Text).Keys).Distinct())
        {
            Assert.Equal(before.GetDocumentFrequency(token), after.GetDocumentFrequency(token));
        }

        // Правило приведения текста не менялось, поэтому его проба совпала.
        Assert.Equal(before.NormalizationRuleProbeDigest, after.NormalizationRuleProbeDigest);
        // А отпечаток содержимого различается: новый урок виден именно здесь.
        Assert.NotEqual(before.CorpusContentHash, after.CorpusContentHash);
        // Значит и полное опознание версии корпуса другое.
        Assert.NotEqual(before.CorpusVersionKey, after.CorpusVersionKey);
    }

    // Проверка 3: после обучения новым уроком обычный читатель отвечает по-новому.
    // Корпус из одного документа выбран затем, чтобы победитель был определён однозначно:
    // проверка про законную смену ответа, а не про порядок кандидатов.
    [Fact]
    public async Task AfterTrainingTheCorrectedLesson_TheOrdinaryReaderAnswersTheNewOutput()
    {
        // Единица, обученная уроку до исправления.
        var beforeUnit = TrainedUnit("lexical-statistics-before", (StatusId, StatusText, StatusBeforeCorrection));
        // Обычный читатель отвечает прежним ответом урока.
        Assert.Equal(StatusBeforeCorrection, await AnswerAsync(beforeUnit, StatusText));

        // Единица, обученная тому же тексту с исправленным ответом.
        var afterUnit = TrainedUnit("lexical-statistics-after", (StatusId, StatusText, StatusAfterCorrection));
        // Ответ сменился законно: это новый урок, а не переиспользованный прежний снимок.
        Assert.Equal(StatusAfterCorrection, await AnswerAsync(afterUnit, StatusText));
    }

    // Проверка 4: снимок с прежней статистикой, предъявленный как новая версия корпуса,
    // отвергается по отпечатку содержимого — при РАВНОМ числе документов и без единого
    // обращения к модели. Отсутствие обращения к модели здесь не обещание, а свойство
    // подписи: проверке происхождения на вход передаются только сами документы.
    [Fact]
    public void StaleSnapshotDeclaredAsTheNewCorpusVersion_IsRefusedByTheContentFingerprint()
    {
        // Статистика старой версии корпуса и документы новой версии.
        var staleStatistics = BuiltStatistics(CedarCorpus(StatusBeforeCorrection));
        var newDocuments = CedarCorpus(StatusAfterCorrection);

        // Число документов у версий равное, поэтому отказать по счётчику невозможно.
        Assert.Equal(staleStatistics.DocumentCount, newDocuments.Length);

        // Прежний снимок, объявленный принадлежащим новому корпусу, отвергнут.
        var refused = staleStatistics.RequireBelongsTo(newDocuments);
        Assert.False(refused.IsSuccess);
        // Отказ пришёл именно по отпечатку содержимого.
        Assert.Equal(LexicalCorpusStatistics.ContentMismatchErrorCode, refused.ErrorCode);
        // Негодного снимка наружу не выдано.
        Assert.Null(refused.Value);

        // Честная пара принимается: тот же снимок со своим собственным корпусом.
        var accepted = staleStatistics.RequireBelongsTo(CedarCorpus(StatusBeforeCorrection));
        Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
        Assert.Same(staleStatistics, accepted.Value);

        // Продуктовый путь переиспользования снимка проходит ту же проверку: подключение
        // каталога правил ранжирования переиспользует и память, и её снимок.
        var unit = TrainedUnit("lexical-statistics-lineage", (StatusId, StatusText, StatusBeforeCorrection));
        var catalog = KnnCandidatePrincipleCatalog.CreateEmpty("documentation");
        Assert.True(catalog.IsSuccess, catalog.ErrorMessage);
        var reused = unit.WithCandidatePrinciples(catalog.Value!);
        Assert.True(reused.IsSuccess, reused.ErrorMessage);
    }

    // Проверка 5, крайний случай: корпус из одного документа и одного кандидата работает,
    // и выигрыша этот перенос на нём не обещает. Разовая цена подготовки равна одному
    // приведению документа к корням, то есть ровно той работе, которую тёплый запрос
    // экономит на одном кандидате.
    [Fact]
    public async Task SingleDocumentCorpus_Answers_AndPromisesNoSaving()
    {
        // Снимок корпуса из одного документа.
        var statistics = BuiltStatistics([Document(StatusId, StatusText, StatusAfterCorrection)]);

        // Один документ, одно приведение документа к корням за всю жизнь версии корпуса.
        Assert.Equal(1, statistics.DocumentCount);
        Assert.Equal(1, statistics.DocumentNormalizationCallsOnBuild);
        // Проба правила приведения текста — три постоянные строки, названные отдельной ценой.
        Assert.Equal(3, statistics.ProbeNormalizationCallsOnBuild);

        // Обычный читатель на таком корпусе отвечает единственным уроком, без отказа.
        var unit = TrainedUnit("lexical-statistics-single", (StatusId, StatusText, StatusAfterCorrection));
        Assert.Equal(StatusAfterCorrection, await AnswerAsync(unit, StatusText));
    }

    // Один документ корпуса для этих проверок. Описание ответа урока берётся тем же
    // единственным описателем, которым его берёт продукт: тесту незачем знать устройство
    // описания, ему нужен ровно тот же материал. Раньше третьим полем записи документа
    // стояла голая строка, и из-за этого отсутствующий ответ был неотличим от явно
    // заданного пустого; смысл этих проверок правка не меняет.
    private static LexicalCorpusDocument Document(string id, string text, string? output)
        => new(id, text, LexicalCorpusStatistics.DescribeOutput(output));

    // Четыре факта проекта Cedar как корпус словесной статистики. Ответ урока о состоянии
    // задаётся вызывающим, чтобы получить две версии одного корпуса.
    private static LexicalCorpusDocument[] CedarCorpus(string statusOutput)
        =>
        [
            Document(OwnerId, OwnerText, "Dana"),
            Document(StatusId, StatusText, statusOutput),
            Document(ReleaseId, ReleaseText, "Friday"),
            Document(LanguageId, LanguageText, "C#")
        ];

    // Собранный снимок или падение с человеческим сообщением отказа.
    private static LexicalCorpusStatistics BuiltStatistics(LexicalCorpusDocument[] documents)
    {
        var built = LexicalCorpusStatistics.Build(documents);
        Assert.True(built.IsSuccess, built.ErrorMessage);
        return built.Value!;
    }

    // Прежнее правило приведения текста к корням, переписанное в проверку дословно:
    // приведение к корням и отбор по длине больше единицы. Копия нужна затем, чтобы
    // сравнивать новый снимок с ПРЕЖНИМ выражением, а не с самим собой.
    private static Dictionary<string, int> PreviousTokenize(string text)
    {
        var tokens = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var token in TrainingVectorBuilder.NormalizeRoots(text).Where(token => token.Length > 1))
        {
            tokens[token] = tokens.TryGetValue(token, out var count) ? count + 1 : 1;
        }

        return tokens;
    }

    // ── Четвёртый раздел ремонта: однозначное кодирование полей перед хешированием ──
    //
    // Что было неправильно. Отпечаток содержимого корпуса собирался в одну строку через
    // постоянные разделители — управляющие знаки U+001F между полями и U+001E между
    // документами, — и разделитель НЕ экранировался. Поэтому тот же знак внутри текста
    // документа сдвигал границы полей, и два РАЗНЫХ корпуса давали один поток байтов и один
    // отпечаток. Вторая половина дефекта: отсутствующее значение приводилось к пустой строке,
    // то есть «поля нет» и «поле пустое» становились одним материалом.
    //
    // Прежнее правило воспроизведено ниже отдельной функцией ОТ ВХОДА
    // (PriorSeparatorContentDigest): оно принимает тот же корпус и считает прежний ответ. Каждая
    // красная проверка требует двух вещей разом: прежнее правило на том же входе даёт НЕВЕРНЫЙ
    // ответ (разный материал — один отпечаток), а действующее правило — верный.
    //
    // Чего эти проверки НЕ доказывают, и я этого не заявляю. Выбор длины в БАЙТАХ, а не в
    // знаках, ни одной из них не отличается: для счёта отпечатка однозначны оба счёта, потому
    // что сами байты значения всё равно лежат в потоке. Байтовая длина выбрана ради будущего
    // ЧИТАТЕЛЯ потока, который должен уметь его разобрать, и это свойство проверено чтением
    // самого кодировщика, а не этими проверками. Прогона не было: прямые команды сборки и
    // прогона тестов запрещены политикой разрешений проекта, зелёными они не называются.

    [Fact]
    public void FieldSeparatorInsideText_FooledThePriorDigest_AndIsSeparatedNow()
    {
        // Два РАЗНЫХ корпуса: в первом знак-разделитель полей лежит внутри текста документа,
        // во втором ровно те же куски разложены по другим полям.
        var textCarriesTheSeparator = new[]
        {
            Document(OwnerId, "Cedar" + FieldSeparator + "owner", "approved")
        };
        var outputCarriesTheSeparator = new[]
        {
            Document(OwnerId, "Cedar", "owner" + FieldSeparator + "approved")
        };

        // Прежнее правило на том же входе отвечает НЕВЕРНО: материал разный, а отпечаток один.
        Assert.Equal(
            PriorSeparatorContentDigest(textCarriesTheSeparator),
            PriorSeparatorContentDigest(outputCarriesTheSeparator));

        // Действующее правило отвечает верно: отпечатки разные, потому что длина каждого поля
        // объявлена до его байтов и разделитель внутри значения границей не становится.
        var first = BuiltStatistics(textCarriesTheSeparator);
        var second = BuiltStatistics(outputCarriesTheSeparator);
        Assert.NotEqual(first.CorpusContentHash, second.CorpusContentHash);
        Assert.NotEqual(first.CorpusVersionKey, second.CorpusVersionKey);

        // И прежнее правило действительно зависит от входа, а не отвечает одно и то же: на
        // входе без знака-разделителя его ответ другой.
        Assert.NotEqual(
            PriorSeparatorContentDigest(textCarriesTheSeparator),
            PriorSeparatorContentDigest([Document(OwnerId, "Cedar owner", "approved")]));
    }

    [Fact]
    public void DocumentSeparatorInsideOutput_MergedTwoDocumentsForThePriorDigest_AndIsSeparatedNow()
    {
        // Корпус из ДВУХ документов.
        var twoDocuments = new[]
        {
            Document(OwnerId, "owner", string.Empty),
            Document(StatusId, "status", string.Empty)
        };

        // Корпус из ОДНОГО документа, в ответе которого лежит знак-разделитель документов и
        // куски второго документа. Для прежнего правила это тот же поток байтов.
        var oneDocumentPretendingToBeTwo = new[]
        {
            Document(
                OwnerId,
                "owner",
                DocumentSeparator + StatusId + FieldSeparator + "status" + FieldSeparator)
        };

        // Прежнее правило на том же входе отвечает НЕВЕРНО: один документ и два документа
        // дают один отпечаток, потому что число документов в поток не входило вовсе.
        Assert.Equal(
            PriorSeparatorContentDigest(twoDocuments),
            PriorSeparatorContentDigest(oneDocumentPretendingToBeTwo));

        // Действующее правило отвечает верно: число документов записано в поток, и отпечатки
        // разные.
        var fromTwo = BuiltStatistics(twoDocuments);
        var fromOne = BuiltStatistics(oneDocumentPretendingToBeTwo);
        Assert.NotEqual(fromTwo.CorpusContentHash, fromOne.CorpusContentHash);
        // Число документов снимка тоже названо честно, а не выведено из потока.
        Assert.Equal(2, fromTwo.DocumentCount);
        Assert.Equal(1, fromOne.DocumentCount);
    }

    [Fact]
    public void AbsentFieldAndEmptyField_WereOneMaterialForThePriorDigest_AndAreSeparatedNow()
    {
        // Корпус, у которого текста НЕТ вовсе.
        var absentText = new[] { Document(OwnerId, null!, "approved") };
        // Корпус, у которого текст ЕСТЬ и он пустой.
        var emptyText = new[] { Document(OwnerId, string.Empty, "approved") };

        // Прежнее правило на том же входе отвечает НЕВЕРНО: отсутствие приводилось к пустой
        // строке, поэтому оба корпуса давали один отпечаток.
        Assert.Equal(
            PriorSeparatorContentDigest(absentText),
            PriorSeparatorContentDigest(emptyText));

        // Действующее правило отвечает верно: у отсутствующего значения своя метка.
        Assert.NotEqual(
            BuiltStatistics(absentText).CorpusContentHash,
            BuiltStatistics(emptyText).CorpusContentHash);
    }

    [Fact]
    public void UnicodeTextChange_ChangesTheFingerprint_AndTheSameBytesKeepIt()
    {
        // Текст вне основной латиницы: у таких знаков байтов больше одного.
        var cyrillic = new[] { Document(OwnerId, "владелец кедра", "одобрено") };
        // Тот же текст с одной изменённой буквой.
        var cyrillicChanged = new[] { Document(OwnerId, "владелец кедры", "одобрено") };

        // Изменение текста меняет отпечаток.
        Assert.NotEqual(
            BuiltStatistics(cyrillic).CorpusContentHash,
            BuiltStatistics(cyrillicChanged).CorpusContentHash);

        // Положительный контроль, зелёный и до правки, и после: копия с теми же байтами
        // сохраняет опознание, и снимок признаёт её своей.
        var snapshot = BuiltStatistics(cyrillic);
        var sameBytesCopy = new[] { Document(OwnerId, "владелец кедра", "одобрено") };
        Assert.Equal(snapshot.CorpusContentHash, BuiltStatistics(sameBytesCopy).CorpusContentHash);
        var belongs = snapshot.RequireBelongsTo(sameBytesCopy);
        Assert.True(belongs.IsSuccess, belongs.ErrorMessage);
    }

    [Fact]
    public void SameIdWithAnotherOutput_IsDetected_AndInputOrderDoesNotChangeTheFingerprint()
    {
        // Положительный контроль: иной ответ при том же опознании и том же тексте замечается.
        // Зелёный и до правки, и после: ответ входил в поток и прежде.
        Assert.NotEqual(
            BuiltStatistics([Document(OwnerId, OwnerText, "approved")]).CorpusContentHash,
            BuiltStatistics([Document(OwnerId, OwnerText, "cancelled")]).CorpusContentHash);

        // Порядок входа отпечаток не меняет: это ЯВНО выбранный договор — поток собирается в
        // порядке сравнения опознаний, а не в порядке перебора памяти.
        var straight = new[]
        {
            Document(OwnerId, OwnerText, "approved"),
            Document(StatusId, StatusText, "approved")
        };
        var reversed = new[]
        {
            Document(StatusId, StatusText, "approved"),
            Document(OwnerId, OwnerText, "approved")
        };
        Assert.Equal(
            BuiltStatistics(straight).CorpusContentHash,
            BuiltStatistics(reversed).CorpusContentHash);

        // А два документа с ОДНИМ опознанием отвергаются своим названным кодом: двусмысленный
        // корпус не получает отпечатка вовсе. Это тоже явно выбранный договор, а не случайность
        // перебора словаря.
        var duplicate = LexicalCorpusStatistics.Build(
        [
            Document(OwnerId, OwnerText, "approved"),
            Document(OwnerId, StatusText, "cancelled")
        ]);
        Assert.False(duplicate.IsSuccess);
        Assert.Equal(LexicalCorpusStatistics.DuplicateDocumentErrorCode, duplicate.ErrorCode);
    }

    // ── Прежнее правило, воспроизведённое для доказательства красноты ───────────

    // Знак, который прежде служил разделителем полей внутри документа.
    private const string FieldSeparator = "\u001f";

    // Знак, который прежде служил разделителем документов.
    private const string DocumentSeparator = "\u001e";

    /// <summary>
    /// ПРЕЖНЕЕ правило отпечатка содержимого корпуса: поля склеивались через постоянные
    /// разделители без экранирования, а отсутствующее значение приводилось к пустой строке.
    /// Это настоящая функция от входа: её ответ считается из предъявленного корпуса.
    /// Воспроизведено здесь только для сравнения.
    /// </summary>
    private static string PriorSeparatorContentDigest(IReadOnlyList<LexicalCorpusDocument> documents)
    {
        // Материал собирается в одну строку в том же постоянном порядке опознаний.
        var builder = new System.Text.StringBuilder();
        foreach (var document in documents.OrderBy(document => document?.ExampleId ?? string.Empty, StringComparer.Ordinal))
        {
            // Ровно прежние три поля и прежние разделители, включая приведение отсутствия к пустоте.
            builder.Append(document?.ExampleId ?? string.Empty).Append(FieldSeparator)
                   .Append(document?.Text ?? string.Empty).Append(FieldSeparator)
                   .Append(document?.OutputDescription.TextForm ?? string.Empty).Append(DocumentSeparator);
        }

        // Отпечаток считался по байтам собранной строки тем же примитивом.
        return Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(builder.ToString()))).ToLowerInvariant();
    }

    // ── Вторая половина четвёртого раздела: раздельный учёт стоимости разбора ──────
    //
    // Что это чинит. Словесная и структурная части оценки кандидатов стоят по-разному и
    // по-разному растут. Словесная читает неизменяемый снимок корпуса и потому от числа
    // кандидатов не зависит; структурная разбирает образец КАЖДОГО кандидата и потому
    // зависит. Пока обе цены складывались в одно число, этого различия не было видно, и
    // улучшение одной половины могло скрыть ухудшение другой.
    //
    // Что здесь доказывается. Обычный продуктовый путь оценки вызывается через обычный
    // конверт запроса (ExecuteAsync), на корпусе из ЧЕТЫРЁХ уроков, то есть на всех
    // кандидатах, а не через вспомогательную сборку снимка. После вызова читается раздельная
    // стоимость: приведений документов корпуса внутри оценки ноль, словесное приведение
    // ровно одно на запрос, структурных разборов — запрос плюс каждый кандидат.
    //
    // Чего здесь НЕ заявляется. Структурная цена этим слайсом НЕ уменьшена: она только
    // названа отдельным числом. Время не измеряется вовсе: числа счётные и воспроизводимые.
    // Прогона не было: прямые команды сборки и прогона тестов запрещены политикой разрешений
    // проекта, зелёными они не называются.

    /// <summary>
    /// ПРЕЖНЕЕ правило словесной части: статистика корпуса пересчитывалась ВНУТРИ оценки
    /// каждого кандидата, поэтому приведений документов корпуса к корням было столько, сколько
    /// документов, умноженное на число кандидатов. Настоящая функция от входа: её ответ
    /// меняется и от числа документов, и от числа кандидатов.
    /// Воспроизведено здесь только для сравнения.
    /// </summary>
    private static int PriorLexicalCorpusNormalizationCalls(int corpusDocuments, int candidates)
        => corpusDocuments * candidates; // Корпус приводился к корням заново на каждого кандидата.

    [Fact]
    public async Task OrdinaryScoringOnAllCandidates_CountsLexicalAndStructuralCostSeparately()
    {
        // Корпус из четырёх уроков: у запроса будет несколько кандидатов, а не один.
        var unit = TrainedUnitWithCedarCorpus("lexical-cost-separation");

        // Обычный продуктовый путь: тот же конверт запроса, которым пользуется продукт.
        var answer = await AnswerAsync(unit, StatusText);
        Assert.False(string.IsNullOrEmpty(answer));

        // Раздельная стоимость последнего обычного вызова оценки.
        var cost = unit.LastCandidateScoringCost;

        // Оценивались все кандидаты, а не один: иначе эта проверка ничего бы не показала.
        Assert.True(cost.CandidatesScored > 1);

        // Словесная часть: ровно одно приведение к корням на весь вызов — сам запрос.
        Assert.Equal(1, cost.LexicalNormalizationCalls);
        Assert.Equal(StatusText.Length, cost.LexicalCharactersNormalized);

        // Главное число: приведений документов КОРПУСА внутри оценки ноль. Прежнее правило на
        // том же входе отвечает иначе, и его ответ растёт с числом кандидатов.
        Assert.Equal(0, cost.LexicalCorpusNormalizationCalls);
        Assert.NotEqual(
            PriorLexicalCorpusNormalizationCalls(4, cost.CandidatesScored),
            cost.LexicalCorpusNormalizationCalls);

        // Прежнее правило действительно зависит от входа, а не отвечает одно и то же.
        Assert.NotEqual(
            PriorLexicalCorpusNormalizationCalls(4, cost.CandidatesScored),
            PriorLexicalCorpusNormalizationCalls(4, cost.CandidatesScored + 1));
        Assert.NotEqual(
            PriorLexicalCorpusNormalizationCalls(4, cost.CandidatesScored),
            PriorLexicalCorpusNormalizationCalls(5, cost.CandidatesScored));

        // Структурная часть считается ОТДЕЛЬНЫМ числом и честно равна «запрос плюс каждый
        // кандидат». Слайс её измеряет, а не уменьшает.
        Assert.Equal(cost.CandidatesScored + 1, cost.StructuralClassificationCalls);
        Assert.True(cost.StructuralCharactersClassified >= StatusText.Length);

        // Две цены не сложены в одну: словесное число не равно структурному на этом входе.
        Assert.NotEqual(cost.LexicalNormalizationCalls, cost.StructuralClassificationCalls);
    }

    [Fact]
    public async Task LexicalCostStaysFlatWhileStructuralCostGrowsWithCandidates()
    {
        // Два корпуса разного размера: один урок и четыре урока.
        var oneLesson = TrainedUnit("lexical-cost-one", (StatusId, StatusText, StatusAfterCorrection));
        var fourLessons = TrainedUnitWithCedarCorpus("lexical-cost-four");

        // Один и тот же обычный запрос обоим.
        Assert.False(string.IsNullOrEmpty(await AnswerAsync(oneLesson, StatusText)));
        Assert.False(string.IsNullOrEmpty(await AnswerAsync(fourLessons, StatusText)));

        var small = oneLesson.LastCandidateScoringCost;
        var large = fourLessons.LastCandidateScoringCost;

        // Словесная цена ПЛОСКАЯ: она не зависит ни от числа кандидатов, ни от размера корпуса.
        Assert.Equal(small.LexicalNormalizationCalls, large.LexicalNormalizationCalls);
        Assert.Equal(0, small.LexicalCorpusNormalizationCalls);
        Assert.Equal(0, large.LexicalCorpusNormalizationCalls);

        // Структурная цена РАСТЁТ вместе с числом оценённых кандидатов, и это видно числом.
        Assert.True(large.CandidatesScored > small.CandidatesScored);
        Assert.True(large.StructuralClassificationCalls > small.StructuralClassificationCalls);

        // Прежнее правило на том же входе дало бы рост и словесной цены тоже.
        Assert.True(
            PriorLexicalCorpusNormalizationCalls(4, large.CandidatesScored) >
            PriorLexicalCorpusNormalizationCalls(1, small.CandidatesScored));
    }

    // Единица, обученная ЧЕТЫРЁМ урокам: у обычного запроса будет несколько кандидатов,
    // поэтому раздельную стоимость можно измерить на всех кандидатах, а не на одном.
    private static TransformerEmbeddingKnnLogicalUnit<string> TrainedUnitWithCedarCorpus(string unitId)
    {
        var trained = TransformerEmbeddingKnnLogicalUnit<string>.Train(
            unitId,
            "generated-capability",
            "documentation",
            SharedModel.Value,
            SharedTokenizer.Value,
            [
                new TransformerKnnTrainingExample<string>(OwnerId, OwnerText, "Dana", ["PRED-DOC-FACT"], ["documentation"]),
                new TransformerKnnTrainingExample<string>(StatusId, StatusText, StatusAfterCorrection, ["PRED-DOC-FACT"], ["documentation"]),
                new TransformerKnnTrainingExample<string>(ReleaseId, ReleaseText, "Friday", ["PRED-DOC-FACT"], ["documentation"]),
                new TransformerKnnTrainingExample<string>(LanguageId, LanguageText, "C#", ["PRED-DOC-FACT"], ["documentation"])
            ]);
        Assert.True(trained.IsSuccess, trained.ErrorMessage);
        return trained.Value!;
    }

    // Единица, обученная одному уроку: обычное обучение, без переобучения и восстановления.
    private static TransformerEmbeddingKnnLogicalUnit<string> TrainedUnit(
        string unitId,
        (string Id, string Text, string Output) lesson)
    {
        var trained = TransformerEmbeddingKnnLogicalUnit<string>.Train(
            unitId,
            "generated-capability",
            "documentation",
            SharedModel.Value,
            SharedTokenizer.Value,
            [new TransformerKnnTrainingExample<string>(
                lesson.Id,
                lesson.Text,
                lesson.Output,
                ["PRED-DOC-FACT"],
                ["documentation"])]);
        Assert.True(trained.IsSuccess, trained.ErrorMessage);
        return trained.Value!;
    }

    // Один обычный вопрос к единице через обычный конверт запроса.
    private static async Task<string> AnswerAsync(TransformerEmbeddingKnnLogicalUnit<string> unit, string question)
    {
        var answer = await unit.ExecuteAsync(new LogicalUnitRequest<string>(
            "run-lexical-corpus-statistics",
            "lexical-corpus-statistics",
            "generated-capability",
            "documentation",
            question,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));
        Assert.True(answer.IsSuccess, answer.ErrorMessage);
        return answer.Value!.Output;
    }
}
