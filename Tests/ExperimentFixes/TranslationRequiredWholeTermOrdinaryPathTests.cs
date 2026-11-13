// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Назначенный остаток строки очереди переносов «обязательное слово проверяется
// целиком» (строка работ 24, устойчивое имя FIX-P56): закрыть ОБЫЧНЫЙ путь
// потребителя и путь ПОСЛЕ ЗАГРУЗКИ сохранённого состояния.
//
// Чем этот файл отличается от уже принятого соседа. Файл
// TranslationRequiredWholeTermLearningTests.cs проверяет то же правило, но зовёт
// проверяющего качества напрямую и собирает разбор запроса копией внутри теста:
// там подпись урока — рукотворная постоянная. Он сам честно называет этот предел
// в своей шапке. Соседний файл здесь НЕ дублируется и НЕ ослабляется: он
// остаётся проверкой прямого контроля и должен оставаться зелёным.
//
// Что проверяется здесь по-человечески. Переводчик выучил, что в переводе одной
// фразы обязано стоять слово «ready», а в переводе другой — два слова подряд
// «is ready». Прежде наличие обязательного слова искали куском текста, поэтому
// заведомо неверный перевод «already» проходил проверку: внутри него есть буквы
// «ready». Теперь требование ищется целыми словами. Этот файл проверяет, что так
// ведёт себя не отдельно взятая единица, а продукт целиком: обучение идёт
// публичным обучением машины перевода на размеченной партии, запрос идёт
// публичным оркестратором, разбор исходного текста считает сам продукт, и
// состояние переживает настоящую запись на диск и загрузку новой машиной.
//
// Почему решение принимает именно проверяющий, а не что-то ещё. В обычном
// адаптере направления кандидат принимается, когда его уверенность не ниже 0.50
// и проверяющий дал «прошло», ЛИБО когда кандидат пришёл из карты фраз и его
// уверенность не ниже 0.85. Учебные строки здесь намеренно не содержат ни карт
// фраз, ни карт слов, поэтому исполнитель отвечает из точной памяти источника:
// опознание «executor-ru_en-exact-source», уверенность 0.93. При таком
// опознании обход карты фраз невозможен по построению, и единственным способом
// принять кандидата остаётся «прошло» от проверяющего. Каждая проверка ниже
// отдельно утверждает, что опознание ответа НЕ заканчивается на «-phrase-map»:
// это и есть поле улик, доказывающее, что обход не срабатывал.
//
// Чего эти проверки НЕ доказывают. Качество обучения здесь не измеряется вообще.
// Правило целого слова не переносится на остальные ветви проверяющего: ветвь
// приметы источника и ветвь правил понятий по-прежнему пользуются поиском куска
// текста, это отдельные владельцы. Морфологии, нормализации Unicode и разбора
// отрицания здесь нет. Загрузка выполняется в том же процессе операционной
// системы, что и запись: отдельный холодный процесс в разрешённые команды этого
// наряда не входит, и байты проверяются только через настоящий файл на диске.
namespace LlmFromScratch.Tests;

using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure.Logical;
using TranslationDomain;

public sealed class TranslationRequiredWholeTermOrdinaryPathTests
{
    // Имя назначения и номер попытки: по ним складывается путь улик.
    private const string FixId = "FIX-P56";
    private const string AttemptId = "attempt-0001";

    // Источник корпуса учебных строк этой приёмки.
    private const string SourceId = "fix-p56-ordinary-path-v1";

    // Язык источника и язык цели обычного запроса: направление русский→английский.
    private const string SourceLanguage = "ru";
    private const string TargetLanguage = "en";

    // Две темы запроса. Адрес точной памяти исполнителя — пара «тема плюс
    // исходный текст», а требование проверяющего адресуется только подписью
    // разбора. Поэтому две темы позволяют выучить для одного исходного текста
    // законный ответ и ответ-ловушку, не создавая второго требования.
    private const string PassTopic = "general";
    private const string TrapTopic = "code";

    // Исходный текст первой группы: содержит имя из кода.
    private const string CodeIdentifierSource = "ReadyFlag готов";

    // Исходный текст второй группы: содержит пустое значение из кода.
    private const string NullLiteralSource = "null готов";

    // Обязательное слово первой группы: одно целое слово.
    private const string SingleWordRequirement = "ready";

    // Обязательное требование второй группы: два целых слова подряд.
    private const string MultiWordRequirement = "is ready";

    // Опознание обычного запасного адаптера-основы: его выдаёт ответ, когда
    // проверяющий отказал и обычный путь не принял выученного кандидата.
    private const string BootstrapAdapterId = "bootstrap-ru_en";

    // Опознание ответа из точной памяти исполнителя: его выдаёт ответ, когда
    // проверяющий дал «прошло» и выученный перевод принят.
    private const string ExactSourceAdapterId = "executor-ru_en-exact-source";

    // Человеческое имя холодной стадии: по нему показания отличают стадию до
    // обучения, где правильным исходом для всех строк является ответ основы.
    private const string ColdPhaseName = "до обучения";

    // Настройки записи учебных текстов: без отступов, с безопасным для кириллицы
    // кодировщиком и строковыми значениями перечислений.
    private static readonly JsonSerializerOptions UnitJson = new()
    {
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    // Настройки записи файла улик: с отступами, чтобы человек читал его глазами.
    private static readonly JsonSerializerOptions ReceiptJson = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    // ── Проверка 1: предусловие приёмки — родные подписи пригодны ─────────────
    //
    // Эта проверка не обучает ничего и не переводит ничего. Она измеряет, какую
    // подпись разбора продукт считает сам для двух исходных текстов, и
    // утверждает два условия пригодности оснастки:
    //   1) в подписи есть настоящее известное смысловое понятие, иначе обучение
    //      проверяющего вообще не запишет требование;
    //   2) подписи двух групп различаются, иначе оба требования лягут под один
    //      адрес и законный ответ одной группы станет отказом из-за требования
    //      другой.
    // Если условие нарушено, оснастка не достигнута, и подгонять её под факт
    // нельзя: измеренные подписи остаются в уликах как находка.
    [Fact]
    public void FixturePrecondition_NativeSignaturesAreUsableAndDistinct()
    {
        // Разбор считается тем же продуктовым разборщиком, который работает на
        // обычном пути, а не копией внутри теста.
        var tokenizer = new SemanticTokenizerUnit();

        // Подпись первой группы: исходный текст с именем из кода.
        var codeSignature = tokenizer.Tokenize(CodeIdentifierSource, SourceLanguage).Signature;

        // Подпись второй группы: исходный текст с пустым значением из кода.
        var nullSignature = tokenizer.Tokenize(NullLiteralSource, SourceLanguage).Signature;

        // Измеренные подписи и понятия пишутся в улики до утверждений.
        WriteReceipt("native-signature-precondition.json", new
        {
            Назначение = FixId,
            Попытка = AttemptId,
            ЧтоЭтоЗаПроверка = "измерение родных подписей разбора до обучения: пригодность оснастки",
            ВремяЗаписиUtc = DateTimeOffset.UtcNow,
            ПервойГруппы = new
            {
                ИсходныйТекст = CodeIdentifierSource,
                РоднаяПодпись = codeSignature,
                ПонятияТокенов = DescribeConcepts(tokenizer, CodeIdentifierSource),
                ОбязательноеТребование = SingleWordRequirement
            },
            ВторойГруппы = new
            {
                ИсходныйТекст = NullLiteralSource,
                РоднаяПодпись = nullSignature,
                ПонятияТокенов = DescribeConcepts(tokenizer, NullLiteralSource),
                ОбязательноеТребование = MultiWordRequirement
            },
            ЧестноеОграничение = new[]
            {
                "здесь не выполняется ни одного урока и ни одного перевода",
                "подписи измерены продуктовым разборщиком, а не собраны копией внутри теста"
            }
        });

        // Подпись обязана быть непустой: пустой адрес требование не примет.
        Assert.False(
            string.IsNullOrWhiteSpace(codeSignature),
            $"родная подпись для «{CodeIdentifierSource}» пуста: оснастка не достигнута");
        Assert.False(
            string.IsNullOrWhiteSpace(nullSignature),
            $"родная подпись для «{NullLiteralSource}» пуста: оснастка не достигнута");

        // Подписи групп обязаны различаться, иначе два требования сойдутся в один
        // адрес и законный ответ одной группы будет отвергнут требованием другой.
        Assert.False(
            string.Equals(codeSignature, nullSignature, StringComparison.Ordinal),
            $"подписи двух групп совпали («{codeSignature}»): требования сойдутся в один адрес, оснастка не достигнута");

        // В подписи обязано быть настоящее известное понятие: без него обучение
        // проверяющего требование по подписи не записывает вообще.
        Assert.True(
            SignatureCarriesKnownConcept(codeSignature),
            $"в подписи «{codeSignature}» нет известного смыслового понятия: обучение проверяющего требование не запишет");
        Assert.True(
            SignatureCarriesKnownConcept(nullSignature),
            $"в подписи «{nullSignature}» нет известного смыслового понятия: обучение проверяющего требование не запишет");
    }

    // ── Проверка 2: обычный путь потребителя до и после обучения ──────────────
    //
    // Это ядро назначенного остатка. Весь путь идёт публичными входами продукта:
    // обучение — публичным обучением машины на размеченной партии, запрос —
    // публичным оркестратором. Разбор исходного текста продукт считает сам.
    [Fact]
    public async Task OrdinaryConsumerPath_RequiredTermIsCheckedAsWholeWords()
    {
        // Журнал улик заполняется по ходу и пишется в блоке finally, поэтому
        // красный прогон тоже оставляет измеренный след.
        var trace = new OrdinaryPathTrace("обычный путь потребителя: до обучения и после обучения");

        try
        {
            // Машина перевода создаётся обычным публичным конструктором: его
            // готовая оснастка подключает к направлению русский→английский
            // обученный адаптер, а значит разбор, исполнителя и проверяющего.
            var machine = new TranslationMachine();

            // Холодный прогон: ни одного урока ещё не было. Каждый из четырёх
            // запросов обязан обслужить запасной адаптер-основа, потому что без
            // выученной памяти уверенность сборного кандидата ниже порога приёма.
            // Это изоляция: любой последующий отказ может прийти только из
            // выученного состояния.
            foreach (var row in OrdinaryRows())
            {
                // Запрос идёт обычным публичным путём потребителя.
                var cold = await AskOrdinaryConsumerAsync(machine, row);
                // Фактический исход записывается в улики целиком.
                trace.Record(ColdPhaseName, row, cold);
                // Выученного ответа ещё нет, поэтому отвечает основа.
                Assert.Equal(BootstrapAdapterId, cold.AdapterId);
                // Обход карты фраз не участвует ни в одном состоянии продукта.
                Assert.False(cold.CameFromPhraseMap, $"строка {row.RowId}: до обучения ответ пришёл из карты фраз");
            }

            // Настоящее обучение: одна партия, шесть учебных строк продукта.
            // Четыре строки учат исполнителя точным ответам, две — проверяющего
            // обязательным требованиям. Карт слов и карт фраз нет ни в одной.
            var batch = BuildAssignedBatch();
            // Обучение вызывается публичным методом машины.
            var trained = await machine.TrainStructuredBatchAsync(batch);
            // Отказ обучения — это отказ приёмки, а не мелочь: дальше измерять нечего.
            Assert.True(
                trained.IsSuccess,
                $"обучение партии не прошло: {trained.ErrorCode} {trained.ErrorMessage}");
            // Фактическое число принятых учебных строк записывается в улики.
            trace.RecordLesson(batch.Examples.Count, trained.Value!.RowsTrained);

            // Прогон после обучения: ядро доказательства. Законный ответ обязан
            // стать получаемым, ответ-ловушка обязан получить отказ.
            foreach (var row in OrdinaryRows())
            {
                // Тот же обычный публичный путь, что и в холодном прогоне.
                var after = await AskOrdinaryConsumerAsync(machine, row);
                // Фактический исход записывается в улики до утверждений.
                trace.Record("после обучения", row, after);
                // Ожидание задано заранее и под факт не подставляется.
                AssertOrdinaryOutcome(after, row, "после обучения");
            }

            // Фактическое принятое состояние пишется на диск публичным
            // сохранением: дальше оно читается обратно настоящей загрузкой.
            var checkpointPath = Path.Combine(ReceiptDirectory(), "checkpoint-after-training.json");
            // Сохранение — публичный метод машины, возвращающий ссылку на файл.
            var saved = await machine.SaveCheckpointAsync(checkpointPath);
            // Без удачной записи проверять загрузку нечем.
            Assert.True(saved.IsSuccess, $"запись состояния не прошла: {saved.ErrorCode} {saved.ErrorMessage}");
            // Файл обязан существовать: наличие байтов на диске — часть утверждения.
            Assert.True(File.Exists(checkpointPath), $"файл состояния не найден по пути {checkpointPath}");

            // Загрузка — публичный статический метод: он возвращает НОВУЮ машину,
            // восстановленную только из прочитанных байтов, без единого урока.
            var loaded = await TranslationMachine.LoadCheckpointAsync(checkpointPath);
            // Без удачной загрузки путь после загрузки не доказан.
            Assert.True(loaded.IsSuccess, $"загрузка состояния не прошла: {loaded.ErrorCode} {loaded.ErrorMessage}");
            // Восстановленная машина берётся из результата загрузки.
            var reloaded = loaded.Value!.Machine;
            // Размер прочитанного файла записывается в улики как признак настоящих байтов.
            trace.RecordCheckpoint(checkpointPath, new FileInfo(checkpointPath).Length);

            // Прогон после загрузки: те же четыре исхода, ни одного нового урока.
            foreach (var row in OrdinaryRows())
            {
                // Запрос идёт обычным публичным путём уже у восстановленной машины.
                var afterLoad = await AskOrdinaryConsumerAsync(reloaded, row);
                // Фактический исход записывается в улики до утверждений.
                trace.Record("после загрузки", row, afterLoad);
                // Ожидание то же самое: загрузка не имеет права менять поведение.
                AssertOrdinaryOutcome(afterLoad, row, "после загрузки состояния");
            }

            // Заведомо неверные выученные тексты не имеют права появиться в
            // ответе ни разу за весь прогон. Это человеческий смысл исправления.
            Assert.DoesNotContain(
                trace.AnsweredTexts,
                text => text.Contains("already", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            // Улики пишутся всегда: и при зелёном, и при красном прогоне.
            trace.Write();
        }
    }

    // ── Утверждение одного исхода обычного пути ───────────────────────────────

    // Один исход обычного пути сверяется с заранее заданным ожиданием.
    private static void AssertOrdinaryOutcome(OrdinaryAnswer answer, OrdinaryRow row, string phase)
    {
        // Обход карты фраз обязан молчать: иначе решение принял не проверяющий,
        // и доказательство обычного пути недействительно.
        Assert.False(
            answer.CameFromPhraseMap,
            $"{phase}, строка {row.RowId}: ответ пришёл из карты фраз, решение принял обход, а не проверяющий");

        if (row.ExpectedAccepted)
        {
            // Законный ответ обязан прийти из точной памяти исполнителя: это и
            // значит, что проверяющий дал «прошло» и выученный перевод принят.
            Assert.Equal(ExactSourceAdapterId, answer.AdapterId);
            // Текст ответа обязан быть ровно выученным переводом.
            Assert.Equal(row.TrainedTarget, answer.AnsweredText);
        }
        else
        {
            // Ответ-ловушка обязан получить отказ проверяющего, поэтому обычный
            // путь уходит в основу и выученный неверный текст не выдаётся.
            Assert.Equal(BootstrapAdapterId, answer.AdapterId);
            // Текст основы сохраняет исходный запрос и выученного текста не несёт.
            Assert.Equal($"[RU_EN:BOOTSTRAP] {row.SourceText}", answer.AnsweredText);
            // Отдельно утверждается, что заведомо неверный выученный текст не выдан.
            Assert.NotEqual(row.TrainedTarget, answer.AnsweredText);
        }
    }

    // ── Обычный публичный путь потребителя ────────────────────────────────────

    // Один запрос обычного потребителя: публичный оркестратор машины перевода.
    private static async Task<OrdinaryAnswer> AskOrdinaryConsumerAsync(
        TranslationMachine machine,
        OrdinaryRow row)
    {
        // Оркестратор берётся публичным методом машины: это обычный вход.
        var orchestrator = await machine.GetOrchestratorAsync();
        // Без оркестратора обычного пути нет, и проверять нечего.
        Assert.True(orchestrator.IsSuccess, $"оркестратор не выдан: {orchestrator.ErrorCode} {orchestrator.ErrorMessage}");

        // Тема и языки передаются теми же показаниями запроса, что у потребителя.
        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["source_language"] = SourceLanguage,
            ["target_language"] = TargetLanguage,
            ["topic_domain"] = row.TopicDomain
        };

        // Запрос собирается обычной записью запроса оркестратора.
        var request = new LogicalOrchestratorRequest(
            RunId: $"fix-p56-{row.RowId}",
            ScenarioId: "fix-p56-ordinary-path",
            UserRequest: row.SourceText,
            Flags: null,
            Metadata: metadata);

        // Запрос выполняется обычным публичным путём.
        var result = await orchestrator.Value!.RunAsync(request);
        // Отказ самого пути означает, что измерять исход нечем.
        Assert.True(result.IsSuccess, $"строка {row.RowId}: обычный путь отказал: {result.ErrorCode} {result.ErrorMessage}");

        // Опознание пути ответа читается из показаний потока: именно это поле
        // доказывает, кто выдал ответ и не срабатывал ли обход карты фраз.
        var metrics = result.Value!.FlowResult.Metrics;
        // Опознание обязано присутствовать: без него исход не доказан.
        Assert.True(metrics.TryGetValue("adapter_id", out var adapterId), $"строка {row.RowId}: показание adapter_id отсутствует");

        return new OrdinaryAnswer(
            RowId: row.RowId,
            AdapterId: adapterId!,
            AnsweredText: result.Value!.FinalHumanResult,
            CameFromPhraseMap: adapterId!.EndsWith("-phrase-map", StringComparison.Ordinal));
    }

    // ── Назначенная учебная партия ────────────────────────────────────────────

    // Четыре строки панели обычного пути: по две на каждую группу требований.
    private static OrdinaryRow[] OrdinaryRows() =>
    [
        new("S1", CodeIdentifierSource, PassTopic, "ReadyFlag ready", true,
            "положительный случай: обязательное слово стоит целым словом"),
        new("S2", CodeIdentifierSource, TrapTopic, "ReadyFlag already", false,
            "КОНТРПРИМЕР: обязательное слово стоит куском внутри другого слова"),
        new("S3", NullLiteralSource, PassTopic, "null is ready", true,
            "многословное требование: два обязательных слова стоят рядом целыми словами"),
        new("S4", NullLiteralSource, TrapTopic, "null is already", false,
            "КОНТРПРИМЕР многословного требования: второе слово стоит куском внутри другого слова")
    ];

    // Одна учебная партия продукта: четыре урока исполнителю и два проверяющему.
    private static TranslationLearningBatch BuildAssignedBatch()
    {
        // Разбор считается тем же продуктовым разборщиком, что работает в пути.
        var tokenizer = new SemanticTokenizerUnit();

        // Учебные строки собираются по порядку панели.
        var examples = new List<TranslationLearningExample>();

        // Исполнитель учится точному ответу для каждой пары «тема плюс текст».
        foreach (var row in OrdinaryRows())
            examples.Add(BuildExecutorLesson(tokenizer, row));

        // Проверяющий учится обязательному требованию для каждой группы подписей.
        examples.Add(BuildVerifierLesson(tokenizer, CodeIdentifierSource, SingleWordRequirement, "V1"));
        examples.Add(BuildVerifierLesson(tokenizer, NullLiteralSource, MultiWordRequirement, "V2"));

        // Партия называет своё имя, предметную область и источник корпуса.
        return new TranslationLearningBatch(
            BatchId: "fix-p56-ordinary-path-batch-1",
            DomainTag: "TranslationDomain",
            Examples: examples,
            SourceIds: [SourceId]);
    }

    // Один урок исполнителя: текст источника, тема и точный выученный ответ.
    private static TranslationLearningExample BuildExecutorLesson(
        SemanticTokenizerUnit tokenizer,
        OrdinaryRow row)
    {
        // Правильный ответ урока — ровно тот текст, который исполнитель обязан
        // вернуть. Карт слов и карт фраз в уроке нет намеренно: иначе кандидат
        // пришёл бы из карты фраз и включил бы обход решения проверяющего.
        var chosen = new ExecutorOutput(
            Text: row.TrainedTarget,
            Confidence: 0.9,
            WordMappings: null,
            PhraseMappings: null);

        // Отвергнутый вариант несёт пустой ответ: он нужен форме учебной записи.
        var rejected = new ExecutorOutput(
            Text: string.Empty,
            Confidence: 0.1,
            WordMappings: null,
            PhraseMappings: null);

        // Подпись урока берётся родным разборщиком продукта, а не из руки.
        var signature = tokenizer.Tokenize(row.SourceText, SourceLanguage).Signature;

        // Строка собирается по форме учебной записи предметной области.
        return new TranslationLearningExample(
            ExampleId: $"fix-p56-exec-{row.RowId}",
            UnitId: TranslationUnitIds.ExecutorRuEn,
            FlowId: TranslationRoutingFlow.FlowId,
            InputJson: JsonSerializer.Serialize(
                new ExecutorInput(row.SourceText, SourceLanguage, row.TopicDomain), UnitJson),
            ChosenOutputJson: JsonSerializer.Serialize(chosen, UnitJson),
            RejectedOutputJson: JsonSerializer.Serialize(rejected, UnitJson),
            RejectedType: "empty_translation",
            SemanticSignature: signature,
            SourceId: SourceId,
            HeldOut: false);
    }

    // Один урок проверяющего: обязательное требование для подписи этой группы.
    private static TranslationLearningExample BuildVerifierLesson(
        SemanticTokenizerUnit tokenizer,
        string sourceText,
        string requirement,
        string rowId)
    {
        // Правильный ответ урока — отказ с названным обязательным требованием.
        // Из текста этого замечания обучение и достаёт требование.
        var chosen = new VerifierOutput(
            Score: 0.42,
            Issues: [$"expected '{requirement}'"],
            Pass: false,
            CorrectionHint: requirement);

        // Отвергнутый вариант несёт сам дефект: неверный кандидат принят.
        var rejected = new VerifierOutput(
            Score: 0.95,
            Issues: [],
            Pass: true,
            CorrectionHint: null);

        // Подпись урока берётся родным разборщиком продукта, а не из руки.
        var signature = tokenizer.Tokenize(sourceText, SourceLanguage).Signature;

        // Кандидат урока — ответ-ловушка: он показывает, что именно было неверно.
        var lessonCandidate = requirement == MultiWordRequirement ? "is already" : "already";

        // Строка собирается по форме учебной записи предметной области.
        return new TranslationLearningExample(
            ExampleId: $"fix-p56-verifier-{rowId}",
            UnitId: TranslationUnitIds.QualityVerifier,
            FlowId: TranslationRoutingFlow.FlowId,
            InputJson: JsonSerializer.Serialize(
                new VerifierInput(sourceText, lessonCandidate), UnitJson),
            ChosenOutputJson: JsonSerializer.Serialize(chosen, UnitJson),
            RejectedOutputJson: JsonSerializer.Serialize(rejected, UnitJson),
            RejectedType: "required_term_substring_accepted",
            SemanticSignature: signature,
            SourceId: SourceId,
            HeldOut: false);
    }

    // ── Вспомогательные измерения ─────────────────────────────────────────────

    // Человеческое описание понятий токенов одного исходного текста.
    private static string[] DescribeConcepts(SemanticTokenizerUnit tokenizer, string sourceText)
        => tokenizer.Tokenize(sourceText, SourceLanguage).Tokens
            .Select(token => $"{token.Word}: понятие {token.Concept}, часть речи {token.Pos}")
            .ToArray();

    // Несёт ли подпись настоящее известное смысловое понятие. Правило повторяет
    // продуктовое условие записи требования: понятие ищется среди составляющих
    // подписи после первой, потому что первая составляющая — образец предложения.
    private static bool SignatureCarriesKnownConcept(string signature)
        => (signature ?? string.Empty).Split('|')
            .Skip(1)
            .Any(component => Enum.GetNames<SemanticConcept>()
                .Any(name => string.Equals(component, name, StringComparison.Ordinal)));

    // ── Путь улик ─────────────────────────────────────────────────────────────

    // Каталог улик этого назначения внутри рабочего дерева.
    private static string ReceiptDirectory() => AllowedReceiptDirectory.Value;

    // Корень рабочего дерева ищется подъёмом от каталога сборки проверок до
    // каталога, содержащего файл решения.
    private static string? RepositoryRootOrNull()
    {
        // Подъём начинается от каталога, из которого запущены проверки.
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            // Файл решения однозначно отмечает корень рабочего дерева.
            if (File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
                return directory.FullName;

            // Иначе поиск продолжается на уровень выше.
            directory = directory.Parent;
        }

        // Корень не найден: это честный случай, он обрабатывается ниже.
        return null;
    }

    // Назначенный нарядом каталог улик этой попытки, внутри которого каждый
    // прогон получает СВОЙ подкаталог. Так повторный прогон больше не затирает
    // показания уже принятой работы: прежде все прогоны писали в один и тот же
    // файл, и размер файла состояния, названный в принятых уликах, разошёлся с
    // размером, который оставил следующий прогон.
    //
    // Имя подкаталога складывается из времени начала прогона по всемирному
    // времени и номера процесса, поэтому два прогона не могут столкнуться даже
    // в одну секунду. Решение принимается один раз за прогон; если корень
    // рабочего дерева не найден, улики пишутся рядом со сборкой, и это видно по
    // самому пути.
    private static readonly Lazy<string> AllowedReceiptDirectory = new(() =>
    {
        // Корень рабочего дерева определяет назначенный путь улик.
        var root = RepositoryRootOrNull();

        // Каталог попытки: назначенный нарядом либо запасной рядом со сборкой.
        var attemptDirectory = root is null
            ? Path.Combine(AppContext.BaseDirectory, FixId, AttemptId)
            : Path.Combine(root, "artifacts", "fbe", FixId, AttemptId);

        // Имя прогона: время начала по всемирному времени плюс номер процесса.
        var runName = string.Concat(
            DateTimeOffset.UtcNow.ToString("yyyyMMddTHHmmssfffZ"),
            "-pid",
            Environment.ProcessId.ToString(System.Globalization.CultureInfo.InvariantCulture));

        // Подкаталог этого прогона внутри каталога попытки.
        var directory = Path.Combine(attemptDirectory, "runs", runName);

        // Каталог создаётся, если его ещё нет.
        Directory.CreateDirectory(directory);
        return directory;
    });

    // Запись одного файла улик: UTF-8 без метки порядка байтов и переводы строк
    // Unix, иначе проверка пробелов перед коммитом ругается на возврат каретки.
    private static void WriteReceipt(string fileName, object payload)
    {
        // Путь складывается из каталога улик этой попытки и имени файла.
        var path = Path.Combine(ReceiptDirectory(), fileName);

        // Переводы строк Windows заменяются на перевод строки Unix.
        var json = JsonSerializer.Serialize(payload, ReceiptJson)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal);
        // Файл обязан заканчиваться переводом строки.
        if (!json.EndsWith('\n')) json += "\n";
        File.WriteAllText(path, json, new UTF8Encoding(false));
    }

    // ── Журнал улик обычного пути ─────────────────────────────────────────────

    // OrdinaryPathTrace собирает фактические исходы обычного пути по стадиям.
    private sealed class OrdinaryPathTrace
    {
        private readonly string _description;
        private readonly List<object> _rows = [];
        private readonly List<string> _answeredTexts = [];
        private int _lessonRowsSubmitted;
        private int _lessonRowsTrained;
        private string? _checkpointPath;
        private long? _checkpointBytes;

        // Журнал создаётся с человеческим описанием своего назначения.
        public OrdinaryPathTrace(string description) => _description = description;

        // Все выданные тексты ответов: по ним проверяется, что заведомо неверный
        // выученный перевод не появился ни разу.
        public IReadOnlyList<string> AnsweredTexts => _answeredTexts;

        // Record записывает фактический исход одного запроса одной стадии.
        public void Record(string phase, OrdinaryRow row, OrdinaryAnswer answer)
        {
            // Текст ответа запоминается отдельно для сквозной проверки.
            _answeredTexts.Add(answer.AnsweredText);
            _rows.Add(new
            {
                Стадия = phase,
                Код = row.RowId,
                ИсходныйТекст = row.SourceText,
                ТемаЗапроса = row.TopicDomain,
                ВыученныйОтвет = row.TrainedTarget,
                // Ожидание зависит от СТАДИИ, а не только от строки панели. До
                // обучения выученного ответа ещё нет ни у одной строки, поэтому
                // правильно отвечает заготовка-основа; прежде в это поле писался
                // признак строки, и показания до обучения читались как будто
                // выученный ответ уже должен быть принят.
                ОжидалсяИсход = DescribeExpectedOutcome(phase, row),
                ФактическоеОпознаниеПути = answer.AdapterId,
                ФактическийТекстОтвета = answer.AnsweredText,
                ОтветИзКартыФраз = answer.CameFromPhraseMap,
                Смысл = row.Meaning
            });
        }

        // DescribeExpectedOutcome называет ожидаемый исход для СТАДИИ и строки.
        private static string DescribeExpectedOutcome(string phase, OrdinaryRow row)
        {
            // До обучения выученной памяти нет ни у одной строки панели, поэтому
            // правильным исходом для всех четырёх строк является ответ основы.
            if (string.Equals(phase, ColdPhaseName, StringComparison.Ordinal))
                return "ответ заготовки-основы: выученной памяти ещё нет";

            // После обучения и после загрузки исход задаётся самой строкой.
            return row.ExpectedAccepted ? "выученный ответ принят" : "отказ проверяющего";
        }

        // RecordLesson записывает поданное и фактически обученное число строк.
        public void RecordLesson(int submitted, int trained)
        {
            // Поданное и обученное число считаются отдельно: равенство не предполагается.
            _lessonRowsSubmitted = submitted;
            _lessonRowsTrained = trained;
        }

        // RecordCheckpoint записывает путь и размер прочитанного файла состояния.
        public void RecordCheckpoint(string path, long bytes)
        {
            // Путь и размер — признак того, что байты действительно легли на диск.
            _checkpointPath = path;
            _checkpointBytes = bytes;
        }

        // Write пишет файл улик. Вызывается всегда, в том числе при падении.
        public void Write()
        {
            var payload = new
            {
                Назначение = FixId,
                Попытка = AttemptId,
                ЧтоЭтоЗаПроверка = _description,
                ВремяЗаписиUtc = DateTimeOffset.UtcNow,
                ПубличныеВходыПродукта = new[]
                {
                    "обучение: TranslationMachine.TrainStructuredBatchAsync",
                    "запрос: TranslationMachine.GetOrchestratorAsync затем RunAsync",
                    "сохранение: TranslationMachine.SaveCheckpointAsync",
                    "загрузка: TranslationMachine.LoadCheckpointAsync"
                },
                ДоказательствоЧтоРешилПроверяющий = new[]
                {
                    "опознание ответа читается из показания adapter_id потока",
                    "ни один ответ не имеет опознания, заканчивающегося на -phrase-map",
                    "значит обход карты фраз не срабатывал и приём кандидата решал проверяющий"
                },
                УчебныеСтроки = new
                {
                    Подано = _lessonRowsSubmitted,
                    ФактическиОбучено = _lessonRowsTrained
                },
                ФайлСостояния = new
                {
                    Путь = _checkpointPath,
                    РазмерБайт = _checkpointBytes
                },
                ЧестноеОграничение = new[]
                {
                    "качество обучения здесь не измеряется вообще",
                    "правило целого слова проверено только в ветви требований по подписи",
                    "загрузка выполняется в том же процессе операционной системы, что и запись",
                    "неизвестные значения оставлены пустыми и не превращены в ноль"
                },
                Исходы = _rows
            };

            // Запись улик обычного пути отдельным файлом.
            WriteReceipt("ordinary-path-receipt.json", payload);
        }
    }

    // ── Записи данных ─────────────────────────────────────────────────────────

    // Строка панели обычного пути: запрос, выученный ответ и ожидаемый исход.
    private sealed record OrdinaryRow(
        string RowId,
        string SourceText,
        string TopicDomain,
        string TrainedTarget,
        bool ExpectedAccepted,
        string Meaning);

    // Фактический исход одного запроса обычного пути.
    private sealed record OrdinaryAnswer(
        string RowId,
        string AdapterId,
        string AnsweredText,
        bool CameFromPhraseMap);
}
