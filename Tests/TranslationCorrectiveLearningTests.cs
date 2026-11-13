// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Приёмка назначенного среза «доставить проверенное исправление существующей
// переводческой единице». Основание — карточка переноса
// docs/plans/replanning-possible-diffs/fixes-based-on-experiments/
// 106-feedback-and-transition-memory-plan-2026-10-30.md, раздел R34-FB.
//
// Что было сломано, по-человечески. Когда внешний ответ проверен арбитрами и признан
// годным, продукт собирает из него учебную пару: годная сторона и отвергнутая сторона.
// Эта пара ВОЗВРАЩАЕТСЯ вызывающему как данные — и на этом всё
// (22.AgentAlgorithms/Pipeline/InternalFirstAiSupport.cs, строка 232: «возвращаем
// приспособленный внешний ответ как готовый к обучению сигнал»). Назначенная обучаемая
// единица перевода пару не получает, то есть исправляющий ответ уроком не становится:
// модель не меняет своего ответа, сколько бы исправлений ей ни принесли.
//
// Что проверяется здесь. Доставка проверенной пары ОДНОЙ назначенной единице —
// исполнительной единице направления английский→русский — через её СОБСТВЕННЫЙ уже
// существующий приёмник пакетного обучения структурированных примеров
// (27.TranslationDomain/TranslationMachine.cs, строка 392), и измерение двух разных
// вещей: ответ назначенной единицы изменился, а чужая память не изменилась.
//
// Контроль чувствительности снимка чужой памяти (решающая 7, добавлена 9 ноября 2026).
// Равенство снимков «до» и «после» само по себе ещё не измерение: такое же равенство дал
// бы и снимок, который не меняется никогда. Поэтому та же доставка тем же переходником
// направляется СОСЕДНЕЙ единице русский→английский, и снимок чужой памяти обязан
// измениться, а снимок назначенной единицы — остаться тем же. Только после этого
// равенство снимков в решающей 5 читается как настоящая неизменность. Отдельно: ответ
// соседнего направления до доставки — начальный ответ необученной единицы с пометкой
// начального режима, поэтому контроль доказывает чувствительность снимка и ничего не
// говорит о качестве перевода соседнего направления.
//
// Чего эти проверки НЕ доказывают.
//   * Общий механизм обратной связи готовым не объявляется: здесь доставляется пара
//     одной назначенной единице, а не строится общий приёмник обратной связи.
//   * Автоматическая связь производителя пары (проект алгоритмов агента) с доменным
//     получателем здесь НЕ создаётся: карточка прямо запрещает глобальный маршрутизатор
//     и автоматического учителя, и эта связь принадлежит другому владельцу.
//   * Полная приёмка карточки плечами через сохранение и загрузку слепка состояния
//     (пункты 1 и 4 раздела) этим срезом не закрывается.
//   * Идемпотентность повторной доставки одного опознавателя урока остаётся ЯВНО
//     открытой предпосылкой: договор получателя на это ещё не назван, а молчаливое
//     слияние по тексту вместо опознавателя карточка запрещает.
//   * Две исправленные строки после коррекции становятся УЧЕБНЫМИ: это повторное
//     чтение усвоенного исправления, а не обобщение на отложенных данных.
//
// Находка при снятии красного, записана здесь, чтобы не потеряться. Карточка называет
// наблюдённые в опыте P4201 неправильные ответы — «Анна закрыла окно» и «Борис закрыл
// дверь». Сегодняшний продукт отвечает ИНАЧЕ: на строку «Anna did not open the window»
// он отдаёт «Анна не открыла дверь», то есть отрицание сохраняет, а предмет берёт
// чужой. Наблюдения карточки, таким образом, ИСТОРИЧЕСКИЕ. Поэтому проверки ниже
// опираются не на дословный текст прежнего неправильного ответа, а на то, что ответ не
// равен проверенному правильному; дословный текст прежнего ответа ожиданием не делается.
// Прямое следствие: проверка одного лишь признака отрицания сегодняшний неправильный
// ответ НЕ забраковала бы, поэтому названные смысловые признаки строки включают и
// предмет, а порог ставится «обязательны все названные» — иначе проверка по одному
// признаку выдала бы негодный ответ за годный.

namespace LlmFromScratch.Tests;

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using TranslationDomain;
using Xunit.Abstractions;

// Проект алгоритмов агента открывается ТОЛЬКО через псевдоним. В решении тринадцать
// отдельных объявлений конверта результата операции, по одному на сборку, и они не
// взаимозаменяемы: прямое «using» обеих сборок дало бы неоднозначность типа конверта,
// а не общий тип. Здесь конверт берётся из общей инфраструктуры, а типы производителя
// пары — по псевдониму.
using Aa = AgentAlgorithms;

// TranslationCorrectiveLearningTests — приёмка доставки проверенного исправления
// назначенной переводческой единице и измерения её ответа до и после доставки.
public sealed class TranslationCorrectiveLearningTests
{
    // _output печатает панель обучения: запрос, ответ до доставки, ответ после.
    private readonly ITestOutputHelper _output;

    // Конструктор получает приёмник вывода проверки от исполнителя проверок.
    public TranslationCorrectiveLearningTests(ITestOutputHelper output)
    {
        // Сохраняем приёмник вывода: панель должна попасть в журнал прогона.
        _output = output;
    }

    // Topic — тема запроса. Тема входит в адрес точной памяти единицы, поэтому она одна
    // и та же в обучении и в чтении; новых тем этот срез не вводит.
    private const string Topic = "general";

    // RequiredMarker — русский признак отрицания, которого требует настоящий проверяющий
    // ответа для обеих исправляемых строк.
    private const string RequiredMarker = "не";

    // DesignatedDirection — направление назначенной обучаемой единицы.
    private const TranslationDirection DesignatedDirection = TranslationDirection.EN_RU;

    // AdapterTypeName — полное имя предлагаемого карточкой переходника подтверждённой
    // коррекции. Тип ищется ПО ИМЕНИ, а не связывается при сборке: иначе этот файл не
    // собрался бы на неизменённом опубликованном продукте и красное пришло бы на
    // компиляции вместо значений.
    private const string AdapterTypeName = "TranslationDomain.TranslationCorrectionLearningAdapter";

    // CorrectionTypeName — полное имя записи подтверждённой коррекции, которую принимает
    // переходник. Ищется по имени по той же причине.
    private const string CorrectionTypeName = "TranslationDomain.TranslationVerifiedCorrection";

    // TrainRows — учебный набор из восьми строк, дословно тот же, что в опыте P4201.
    private static readonly (string Id, string SourceEn, string GoldRu)[] TrainRows =
    [
        ("t1", "Anna opened the door", "Анна открыла дверь"),
        ("t2", "Anna closed the window", "Анна закрыла окно"),
        ("t3", "Boris opened the window", "Борис открыл окно"),
        ("t4", "Boris closed the door", "Борис закрыл дверь"),
        ("t5", "Anna did not open the door", "Анна не открыла дверь"),
        ("t6", "Anna did not close the window", "Анна не закрыла окно"),
        ("t7", "Boris did not open the window", "Борис не открыл окно"),
        ("t8", "Boris did not close the door", "Борис не закрыл дверь")
    ];

    // ExamRows — две исправляемые строки опыта P4201. Правильный ответ хранится здесь, в
    // учебной оснастке, а не в продуктовой функции выбора ответа. Required — названные
    // смысловые признаки правильного ответа: отрицание И предмет. Одного отрицания
    // недостаточно, потому что сегодняшний неправильный ответ отрицание сохраняет.
    private static readonly (string Id, string SourceEn, string GoldRu, string[] Required)[] ExamRows =
    [
        ("e5", "Anna did not open the window", "Анна не открыла окно", [RequiredMarker, "окно"]),
        ("e7", "Boris did not open the door", "Борис не открыл дверь", [RequiredMarker, "дверь"])
    ];

    // LessonJson — настройки записи учебных текстов: перечисления пишутся словами, как их
    // читает получатель обучения.
    private static readonly JsonSerializerOptions LessonJson = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    // ── Опоры непустоты: проходят на ОБОИХ состояниях продукта ────────────────────

    // Опора 1. Проверенная пара действительно существует и действительно проверена
    // настоящим продуктовым проверяющим: ответ модели забракован по отсутствию
    // названного признака, а производитель пары сохраняет обе её стороны раздельно.
    [Fact]
    public async Task Anchor01_VerifiedPairExistsAndKeepsBothSidesSeparately()
    {
        // Берём настоящий ответ продукта на первую исправляемую строку.
        var machine = await ColdMachineWithEightLessonsAsync();
        var row = ExamRows[0];
        var issuedAnswer = await ReadThroughProductAsync(machine, row.SourceEn, "anchor01");

        // Настоящий ответ не пуст: проверять нечего, если ответа нет вовсе.
        Assert.False(string.IsNullOrWhiteSpace(issuedAnswer));
        // Хотя бы одного названного смыслового признака в выданном ответе нет — именно
        // поэтому ответ и подлежит исправлению. Дословный текст прежнего неправильного
        // ответа ожиданием не делается: он исторический.
        Assert.Contains(row.Required, marker => !issuedAnswer.Contains(marker, StringComparison.OrdinalIgnoreCase));

        // Настоящий проверяющий ответа из проекта алгоритмов агента судит фактический
        // ответ, без подстановки требуемых признаков из запроса.
        var examiner = new Aa.InternalResultExaminer();
        var examination = examiner.Examine(
            originalRequest: row.SourceEn,
            internalResponse: issuedAnswer,
            requiredPredicates: row.Required,
            arbiterConstraints: [],
            // Порог «обязательны ВСЕ названные признаки». Это собственный довод
            // продуктового проверяющего, а не правка его устройства: при пороге по
            // умолчанию ответ с половиной найденных признаков прошёл бы как годный.
            confidenceThreshold: 1.0);
        Assert.True(examination.IsSuccess);
        // Отказ проверяющего — это именно потребность во внешней помощи при названном
        // отсутствующем признаке.
        Assert.True(examination.Value!.NeedsExternalAi);
        Assert.NotEmpty(examination.Value.MissingPredicates);
        // Названные отсутствующими признаки берутся только из требуемого набора строки.
        Assert.All(examination.Value.MissingPredicates, missing => Assert.Contains(missing, row.Required));

        // Производитель учебной пары из того же проекта: внешний ответ проходит арбитров
        // и сохраняется вместе с фактически выданным отвергнутым ответом.
        var package = new Aa.ExternalAiRequestPackage(
            row.SourceEn,
            "TRANSLATE_OP",
            issuedAnswer,
            examination.Value.Confidence,
            examination.Value.MissingCompetence,
            "Russian sentence that keeps the negation of the English source.",
            []);
        var producer = new Aa.ExternalAiResultAdapter();
        var produced = producer.Adapt(package, row.GoldRu, "TranslationDomain");

        // Пара собрана, арбитры её не отвергли.
        Assert.True(produced.IsSuccess);
        Assert.Equal("external_answer_adapted_for_dpo", produced.Value!.SignalType);
        // Отвергнутая сторона — ФАКТИЧЕСКИ выданный ответ, а не непереведённый источник.
        Assert.Equal(issuedAnswer, produced.Value.InternalAttempt);
        Assert.NotEqual(row.SourceEn, produced.Value.InternalAttempt);
        // Годная сторона — проверенный правильный ответ.
        Assert.Equal(row.GoldRu, produced.Value.ExternalAnswer);
    }

    // Опора 2. Назначенный получатель обучения уже существует в продукте и достижим:
    // его собственный приёмник пакетного обучения принимает обычный урок и называет
    // единицу в числе продвинутых.
    [Fact]
    public async Task Anchor02_DesignatedReceiverAlreadyExistsAndIsReachable()
    {
        // Холодная машина перевода без обучения.
        var machine = new TranslationMachine();
        var tokenizer = new TranslationSemanticTokenizer();
        var unitId = TranslationUnitIds.ExecutorFor(DesignatedDirection);

        // Один обычный урок через уже существующий вход обучения.
        var batch = new TranslationLearningBatch(
            BatchId: "r34fb-anchor02",
            DomainTag: TranslationMachine.DomainTag,
            Examples: [BuildSeedLesson("r34fb-anchor02-t1", TrainRows[0].SourceEn, TrainRows[0].GoldRu, tokenizer)],
            SourceIds: ["T-ENRU"]);
        var result = await machine.TrainStructuredBatchAsync(batch);

        // Урок принят, строка учтена, получатель назван поимённо.
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, result.Value!.RowsTrained);
        Assert.Contains(unitId, result.Value.PromotedUnitIds);
    }

    // Опора 3. Сам путь обучения ЖИВОЙ и данных достаточно: урок с проверенным
    // правильным ответом, поданный руками через уже существующий продуктовый вход,
    // меняет ответ назначенной единицы. Без этой опоры красное можно было бы списать на
    // отсутствие данных или на мёртвый путь обучения.
    [Fact]
    public async Task Anchor03_ExistingTrainingEntryAlreadyChangesTheAnswer()
    {
        // Машина с восемью общими уроками.
        var machine = await ColdMachineWithEightLessonsAsync();
        var row = ExamRows[0];
        var tokenizer = new TranslationSemanticTokenizer();

        // До подачи урока ответ не равен проверенному правильному.
        var before = await ReadThroughProductAsync(machine, row.SourceEn, "anchor03-before");
        Assert.NotEqual(row.GoldRu, before);

        // Урок подаётся руками через тот же существующий вход обучения, без нового кода.
        var batch = new TranslationLearningBatch(
            BatchId: "r34fb-anchor03",
            DomainTag: TranslationMachine.DomainTag,
            Examples: [BuildSeedLesson("r34fb-anchor03-e5", row.SourceEn, row.GoldRu, tokenizer)],
            SourceIds: ["T-ENRU"]);
        var trained = await machine.TrainStructuredBatchAsync(batch);
        Assert.True(trained.IsSuccess, trained.ErrorMessage);

        // После подачи урока продуктовый читатель отдаёт проверенный правильный ответ.
        var after = await ReadThroughProductAsync(machine, row.SourceEn, "anchor03-after");
        Assert.Equal(row.GoldRu, after);
    }

    // Опора 4. Проба осмысленна: на восьми общих уроках обе исправляемые строки
    // отвечают НЕ тем, что требуется, а все восемь учебных строк читаются правильно.
    [Fact]
    public async Task Anchor04_ProbeIsMeaningfulAndRetentionIsFullBeforeDelivery()
    {
        // Машина с восемью общими уроками.
        var machine = await ColdMachineWithEightLessonsAsync();

        // Обе исправляемые строки отвечают непусто и неправильно.
        foreach (var row in ExamRows)
        {
            var answer = await ReadThroughProductAsync(machine, row.SourceEn, $"anchor04-{row.Id}");
            Assert.False(string.IsNullOrWhiteSpace(answer));
            Assert.NotEqual(row.GoldRu, answer);
        }

        // Удержание усвоенного: все восемь учебных строк читаются правильно.
        Assert.Equal(TrainRows.Length, await CountCorrectTrainRowsAsync(machine, "anchor04"));
    }

    // Опора 5. Молчаливое ничегонеделание существует и сегодня: партия, чьи строки не
    // нашли ни одного получателя обучения, объявляется УСПЕШНОЙ и называет ноль
    // продвинутых единиц. Это и есть причина, по которой переходнику нужна собственная
    // проверка известности получателя ДО записи, а не после. Поведение самого приёмника
    // этот срез не меняет: опора проходит на обоих состояниях продукта.
    [Fact]
    public async Task Anchor05_BatchThatReachedNoReceiverIsStillReportedSuccessfulToday()
    {
        // Машина пути внедрения потока: типизированных единиц и обучателей у неё нет.
        var machineWithoutUnits = new TranslationMachine(new TranslationRoutingFlow());
        var tokenizer = new TranslationSemanticTokenizer();
        var row = ExamRows[0];

        // Полностью законный урок для назначенной единицы.
        var batch = new TranslationLearningBatch(
            BatchId: "r34fb-anchor05",
            DomainTag: TranslationMachine.DomainTag,
            Examples: [BuildSeedLesson("r34fb-anchor05-e5", row.SourceEn, row.GoldRu, tokenizer)],
            SourceIds: ["T-ENRU"]);
        var result = await machineWithoutUnits.TrainStructuredBatchAsync(batch);

        // Партия объявлена принятой и строка посчитана обученной...
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, result.Value!.RowsTrained);
        // ...но ни одна единица не продвинута: урок не дошёл ни до кого.
        Assert.Empty(result.Value.PromotedUnitIds);
    }

    // ── Решающие проверки: красные на неизменённом опубликованном продукте ────────

    // Решающая 1. Переходник подтверждённой коррекции существует в доменном проекте
    // перевода и несёт обе названные карточкой точки: сборку урока без записи и
    // доставку в существующий приёмник обучения.
    [Fact]
    public void Decisive01_CorrectionDeliveryAdapterExistsInTranslationDomain()
    {
        // Тип переходника ищется по имени в сборке доменного проекта перевода.
        var adapterType = FindProductType(AdapterTypeName);
        Assert.NotNull(adapterType);

        // Запись подтверждённой коррекции — вход переходника.
        var correctionType = FindProductType(CorrectionTypeName);
        Assert.NotNull(correctionType);

        // Сборка урока без записи: на ней стоит допуск до любой записи в память.
        Assert.NotNull(adapterType!.GetMethod("BuildLesson", BindingFlags.Public | BindingFlags.Instance));
        // Доставка в существующий приёмник обучения назначенной единицы.
        Assert.NotNull(adapterType.GetMethod("DeliverAsync", BindingFlags.Public | BindingFlags.Instance));
    }

    // Решающая 2. Доставленная проверенная пара МЕНЯЕТ ответ назначенной единицы,
    // прочитанный настоящим продуктовым читателем, и не ломает удержание восьми
    // усвоенных строк. Это и есть панель обучения: запрос, ответ до, ответ после.
    [Fact]
    public async Task Decisive02_DeliveredVerifiedCorrectionChangesTheAnswerOfTheDesignatedUnit()
    {
        // Машина с восемью общими уроками.
        var machine = await ColdMachineWithEightLessonsAsync();

        // Ответы ДО доставки читаются настоящим продуктовым читателем.
        var before = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var row in ExamRows)
            before[row.Id] = await ReadThroughProductAsync(machine, row.SourceEn, $"decisive02-before-{row.Id}");

        // Доставка двух подтверждённых коррекций назначенной единице.
        foreach (var row in ExamRows)
        {
            var delivered = await DeliverAsync(
                machine,
                lessonId: $"r34fb-correction-{row.Id}",
                sourceText: row.SourceEn,
                actualRejectedAnswer: before[row.Id],
                verifiedCorrectAnswer: row.GoldRu,
                verificationProvenance: $"InternalResultExaminer: missing predicate '{RequiredMarker}'");
            Assert.True(delivered.IsSuccess, delivered.ErrorMessage);
            // Урок принят ИМЕННО назначенной единицей, а не молча пропущен.
            Assert.Contains(TranslationUnitIds.ExecutorFor(DesignatedDirection), delivered.Value!.PromotedUnitIds);
        }

        // Ответы ПОСЛЕ доставки читаются тем же настоящим продуктовым читателем.
        _output.WriteLine("Панель обучения назначенной единицы (направление английский→русский):");
        foreach (var row in ExamRows)
        {
            var after = await ReadThroughProductAsync(machine, row.SourceEn, $"decisive02-after-{row.Id}");
            _output.WriteLine($"  запрос:        {row.SourceEn}");
            _output.WriteLine($"  ответ до:      {before[row.Id]}");
            _output.WriteLine($"  ответ после:   {after}");
            _output.WriteLine($"  проверенный:   {row.GoldRu}");

            // Ответ изменился и стал проверенным правильным.
            Assert.NotEqual(before[row.Id], after);
            Assert.Equal(row.GoldRu, after);
        }

        // Удержание усвоенного не пострадало: все восемь учебных строк по-прежнему верны.
        Assert.Equal(TrainRows.Length, await CountCorrectTrainRowsAsync(machine, "decisive02"));
    }

    // Решающая 3. Урок сохраняет ДЕЙСТВИТЕЛЬНО выданный отвергнутый ответ, адресуется
    // назначенной единице и несёт семантическую подпись существующего разборщика.
    [Fact]
    public async Task Decisive03_BuiltLessonKeepsTheActuallyIssuedRejectedAnswer()
    {
        // Настоящий выданный ответ берётся у продукта, а не придумывается.
        var machine = await ColdMachineWithEightLessonsAsync();
        var row = ExamRows[0];
        var issuedAnswer = await ReadThroughProductAsync(machine, row.SourceEn, "decisive03");

        // Сборка урока без записи в память.
        var built = BuildLesson(
            lessonId: $"r34fb-correction-{row.Id}",
            sourceText: row.SourceEn,
            actualRejectedAnswer: issuedAnswer,
            verifiedCorrectAnswer: row.GoldRu,
            verificationProvenance: $"InternalResultExaminer: missing predicate '{RequiredMarker}'");
        Assert.True(built.IsSuccess, built.ErrorMessage);

        var lesson = built.Value!;
        // Урок адресован назначенной единице и назначенному потоку.
        Assert.Equal(TranslationUnitIds.ExecutorFor(DesignatedDirection), lesson.UnitId);
        Assert.Equal(TranslationRoutingFlow.FlowId, lesson.FlowId);
        Assert.False(lesson.HeldOut);

        // Годная сторона — проверенный правильный ответ.
        var chosen = JsonSerializer.Deserialize<ExecutorOutput>(lesson.ChosenOutputJson, LessonJson);
        Assert.NotNull(chosen);
        Assert.Equal(row.GoldRu, chosen!.Text);

        // Отвергнутая сторона — фактически выданный ответ, а НЕ непереведённый источник.
        var rejected = JsonSerializer.Deserialize<ExecutorOutput>(lesson.RejectedOutputJson, LessonJson);
        Assert.NotNull(rejected);
        Assert.Equal(issuedAnswer, rejected!.Text);
        Assert.NotEqual(row.SourceEn, rejected.Text);

        // Подпись урока — подпись существующего продуктового разборщика того же текста.
        var expectedSignature = new TranslationSemanticTokenizer().Tokenize(row.SourceEn, "en").Signature;
        Assert.Equal(expectedSignature, lesson.SemanticSignature);
    }

    // Решающая 4. Один отказ проверяющего БЕЗ проверенного правильного текста урока не
    // создаёт, и ответ назначенной единицы от такой попытки не меняется.
    [Fact]
    public async Task Decisive04_RejectionWithoutVerifiedTextCreatesNoLesson()
    {
        // Машина с восемью общими уроками.
        var machine = await ColdMachineWithEightLessonsAsync();
        var row = ExamRows[0];
        var issuedAnswer = await ReadThroughProductAsync(machine, row.SourceEn, "decisive04-before");
        var versionBefore = machine.CurrentStateVersion;

        // Сборка урока без проверенного правильного текста отвергается ДО записи.
        var built = BuildLesson(
            lessonId: $"r34fb-correction-{row.Id}",
            sourceText: row.SourceEn,
            actualRejectedAnswer: issuedAnswer,
            verifiedCorrectAnswer: "   ",
            verificationProvenance: "InternalResultExaminer: unresolved rejection");
        Assert.False(built.IsSuccess);
        Assert.Equal("CORRECTION_WITHOUT_VERIFIED_ANSWER", built.ErrorCode);

        // Доставка такой незавершённой коррекции тоже отвергается.
        var delivered = await DeliverAsync(
            machine,
            lessonId: $"r34fb-correction-{row.Id}",
            sourceText: row.SourceEn,
            actualRejectedAnswer: issuedAnswer,
            verifiedCorrectAnswer: "   ",
            verificationProvenance: "InternalResultExaminer: unresolved rejection");
        Assert.False(delivered.IsSuccess);
        Assert.Equal("CORRECTION_WITHOUT_VERIFIED_ANSWER", delivered.ErrorCode);

        // Состояние не публиковалось и ответ не изменился.
        Assert.Equal(versionBefore, machine.CurrentStateVersion);
        Assert.Equal(issuedAnswer, await ReadThroughProductAsync(machine, row.SourceEn, "decisive04-after"));
    }

    // Решающая 5. Чужая память не изменилась: посторонние единицы байт в байт те же, а
    // соседнее направление отвечает на свой запрос ровно как прежде.
    [Fact]
    public async Task Decisive05_NeighbourMemoryAndNeighbourAnswerStayUnchanged()
    {
        // Машина с восемью общими уроками.
        var machine = await ColdMachineWithEightLessonsAsync();
        var row = ExamRows[0];
        var issuedAnswer = await ReadThroughProductAsync(machine, row.SourceEn, "decisive05-before");

        // Слепок памяти ДО доставки: соседние единицы и общая память ближайших соседей.
        var neighboursBefore = SnapshotNeighbourMemory(machine);
        var designatedBefore = SnapshotDesignatedMemory(machine);
        var sharedBefore = SnapshotSharedPairs(machine);
        // Ответ соседнего направления русский→английский на свой собственный запрос.
        const string neighbourProbe = "Анна открыла дверь";
        var neighbourAnswerBefore = await ReadThroughProductAsync(
            machine, neighbourProbe, "decisive05-neighbour-before", "ru", "en");

        // Доставка одной подтверждённой коррекции назначенной единице.
        var delivered = await DeliverAsync(
            machine,
            lessonId: $"r34fb-correction-{row.Id}",
            sourceText: row.SourceEn,
            actualRejectedAnswer: issuedAnswer,
            verifiedCorrectAnswer: row.GoldRu,
            verificationProvenance: $"InternalResultExaminer: missing predicate '{RequiredMarker}'");
        Assert.True(delivered.IsSuccess, delivered.ErrorMessage);

        // Слепок памяти ПОСЛЕ доставки.
        var neighboursAfter = SnapshotNeighbourMemory(machine);
        var designatedAfter = SnapshotDesignatedMemory(machine);
        var sharedAfter = SnapshotSharedPairs(machine);

        // Память назначенной единицы изменилась — иначе доставки не было.
        Assert.NotEqual(designatedBefore, designatedAfter);
        // Память ВСЕХ прочих единиц — разборщика, проверяющего и пяти других
        // исполнительных единиц — осталась ровно той же.
        Assert.Equal(neighboursBefore, neighboursAfter);

        // Общая память ближайших соседей — назначенный проектом мост между обучением и
        // чтением, поэтому она пополняется. Проверяется, что в неё вошёл ТОЛЬКО
        // проверенный правильный ответ и ничего больше.
        Assert.True(sharedAfter.Count > sharedBefore.Count);
        var addedTargets = Subtract(sharedAfter, sharedBefore);
        Assert.NotEmpty(addedTargets);
        foreach (var target in addedTargets)
            Assert.Equal(row.GoldRu, target);

        // Ответ соседнего направления не изменился ни на знак.
        var neighbourAnswerAfter = await ReadThroughProductAsync(
            machine, neighbourProbe, "decisive05-neighbour-after", "ru", "en");
        Assert.Equal(neighbourAnswerBefore, neighbourAnswerAfter);

        // Панель неизменности печатается в журнал прогона, чтобы сравнение было видно
        // человеку числами, а не только засчитано утверждением. Ожиданий она не меняет.
        _output.WriteLine(
            "чужая память, длина слепка до и после: " +
            $"{neighboursBefore.Length} и {neighboursAfter.Length}; изменилась: " +
            $"{!string.Equals(neighboursBefore, neighboursAfter, StringComparison.Ordinal)}");
        _output.WriteLine(
            "память назначенной единицы, длина слепка до и после: " +
            $"{designatedBefore.Length} и {designatedAfter.Length}; изменилась: " +
            $"{!string.Equals(designatedBefore, designatedAfter, StringComparison.Ordinal)}");
        _output.WriteLine(
            $"общая память ближайших соседей: было {sharedBefore.Count}, стало {sharedAfter.Count}; " +
            $"добавлено только проверенных правильных ответов: {addedTargets.Count}");
        _output.WriteLine(
            $"ответ соседнего направления до и после: '{neighbourAnswerBefore}' и '{neighbourAnswerAfter}'");
    }

    // Решающая 6. Подмена отвергнутого текста и неизвестный получатель обнаруживаются ДО
    // любой записи в память.
    [Fact]
    public async Task Decisive06_SubstitutedRejectedTextAndUnknownReceiverAreRefusedBeforeAnyWrite()
    {
        // Машина с восемью общими уроками.
        var machine = await ColdMachineWithEightLessonsAsync();
        var row = ExamRows[0];
        var versionBefore = machine.CurrentStateVersion;

        // Подмена первого рода: вместо выданного ответа подставлен непереведённый источник.
        var sourceAsRejected = BuildLesson(
            lessonId: $"r34fb-correction-{row.Id}",
            sourceText: row.SourceEn,
            actualRejectedAnswer: row.SourceEn,
            verifiedCorrectAnswer: row.GoldRu,
            verificationProvenance: "InternalResultExaminer: missing predicate");
        Assert.False(sourceAsRejected.IsSuccess);
        Assert.Equal("REJECTED_ANSWER_SUBSTITUTED_BY_SOURCE", sourceAsRejected.ErrorCode);

        // Подмена второго рода: отвергнутая сторона равна проверенной правильной, то
        // есть исправлять нечего и урока из этого нет.
        var sameAsVerified = BuildLesson(
            lessonId: $"r34fb-correction-{row.Id}",
            sourceText: row.SourceEn,
            actualRejectedAnswer: row.GoldRu,
            verifiedCorrectAnswer: row.GoldRu,
            verificationProvenance: "InternalResultExaminer: missing predicate");
        Assert.False(sameAsVerified.IsSuccess);
        Assert.Equal("REJECTED_EQUALS_VERIFIED_ANSWER", sameAsVerified.ErrorCode);

        // Ни одна из отвергнутых попыток состояния не тронула.
        Assert.Equal(versionBefore, machine.CurrentStateVersion);

        // Неизвестный получатель: машина, собранная на пути внедрения потока, не имеет
        // ни одного обучателя единиц. Прежде такая доставка прошла бы «успешно» и не
        // обучила ничего; теперь она называется отказом ДО записи.
        var machineWithoutUnits = new TranslationMachine(new TranslationRoutingFlow());
        var unknownReceiver = await DeliverAsync(
            machineWithoutUnits,
            lessonId: $"r34fb-correction-{row.Id}",
            sourceText: row.SourceEn,
            actualRejectedAnswer: "Анна закрыла окно",
            verifiedCorrectAnswer: row.GoldRu,
            verificationProvenance: "InternalResultExaminer: missing predicate");
        Assert.False(unknownReceiver.IsSuccess);
        Assert.Equal("UNKNOWN_LEARNING_RECEIVER", unknownReceiver.ErrorCode);
    }

    // Решающая 7. Контроль чувствительности снимка чужой памяти. Та же доставка тем же
    // переходником, но получатель — СОСЕДНЯЯ единица: снимок чужой памяти обязан
    // измениться. Без этого контроля равенство снимков в решающей 5 мог бы дать и
    // снимок, который не меняется никогда, то есть «чужая память не изменилась»
    // осталось бы утверждением, а не измерением.
    [Fact]
    public async Task Decisive07_NeighbourMemorySnapshotChangesWhenTheNeighbourIsTheReceiver()
    {
        // Та же холодная машина с восемью общими уроками назначенного направления.
        var machine = await ColdMachineWithEightLessonsAsync();

        // Снимки ДО доставки: отдельно чужая память, отдельно память назначенной единицы.
        var neighboursBefore = SnapshotNeighbourMemory(machine);
        var designatedBefore = SnapshotDesignatedMemory(machine);

        // Запрос соседнего направления русский→английский и его проверенный правильный
        // ответ. Правильный ответ хранится здесь, в учебной оснастке, а не в продуктовой
        // функции выбора ответа.
        const string neighbourSource = "Анна открыла дверь";
        const string neighbourVerified = "Anna opened the door";

        // Фактически выданный соседней единицей ответ читается настоящим продуктовым
        // читателем: отвергнутая сторона урока обязана быть тем, что продукт ответил.
        var neighbourIssued = await ReadThroughProductAsync(
            machine, neighbourSource, "decisive07-neighbour-before", "ru", "en");

        // Предпосылка контроля: исправлять действительно есть что. Если бы соседняя
        // единица уже отвечала правильно, доставка была бы законно отвергнута, и
        // контроль чувствительности ничего бы не измерил.
        Assert.NotEqual(neighbourVerified, neighbourIssued);

        // Доставка одной подтверждённой коррекции СОСЕДНЕЙ единице тем же переходником.
        var delivered = await DeliverAsync(
            machine,
            direction: TranslationDirection.RU_EN,
            lessonId: "r34fb-sensitivity-ruen",
            sourceText: neighbourSource,
            actualRejectedAnswer: neighbourIssued,
            verifiedCorrectAnswer: neighbourVerified,
            verificationProvenance: "InternalResultExaminer: wrong target subject");
        Assert.True(delivered.IsSuccess, delivered.ErrorMessage);

        // Снимки ПОСЛЕ доставки.
        var neighboursAfter = SnapshotNeighbourMemory(machine);
        var designatedAfter = SnapshotDesignatedMemory(machine);

        // Главное измерение контроля: снимок чужой памяти ИЗМЕНИЛСЯ. Значит равенство
        // снимков в решающей 5 — настоящее измерение неизменности, а не слепая проверка.
        Assert.NotEqual(neighboursBefore, neighboursAfter);

        // Обратная сторона того же инварианта: при доставке соседу память назначенной
        // единицы осталась ровно той же.
        Assert.Equal(designatedBefore, designatedAfter);

        // Панель контроля печатается в журнал прогона, чтобы сравнение было видно
        // человеку, а не только засчитано утверждением.
        _output.WriteLine($"сосед, ответ до доставки: {neighbourIssued}");
        _output.WriteLine($"сосед, проверенный правильный ответ: {neighbourVerified}");
        _output.WriteLine(
            "снимок чужой памяти изменился: " +
            $"{!string.Equals(neighboursBefore, neighboursAfter, StringComparison.Ordinal)}");
        _output.WriteLine(
            "снимок назначенной единицы изменился: " +
            $"{!string.Equals(designatedBefore, designatedAfter, StringComparison.Ordinal)}");
    }

    // ── Общая оснастка ────────────────────────────────────────────────────────────

    // ColdMachineWithEightLessonsAsync создаёт холодную машину перевода и обучает её
    // восемью общими уроками через уже существующий продуктовый вход обучения.
    private static async Task<TranslationMachine> ColdMachineWithEightLessonsAsync()
    {
        // Холодная машина: никакого состояния с прошлых проверок.
        var machine = new TranslationMachine();
        // Разборщик берётся продуктовый — тот же, чью подпись читает получатель обучения.
        var tokenizer = new TranslationSemanticTokenizer();
        // Восемь уроков собираются в одну партию.
        var examples = TrainRows
            .Select(r => BuildSeedLesson($"t-enru-{r.Id}", r.SourceEn, r.GoldRu, tokenizer))
            .ToList();
        var batch = new TranslationLearningBatch(
            BatchId: "r34fb-base-eight",
            DomainTag: TranslationMachine.DomainTag,
            Examples: examples,
            SourceIds: ["T-ENRU"]);
        // Партия подаётся в существующий приёмник пакетного обучения.
        var result = await machine.TrainStructuredBatchAsync(batch);
        // Без принятой партии дальнейшее измерение бессмысленно.
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return machine;
    }

    // BuildSeedLesson собирает ОДИН начальный урок исполнительной единицы ровно в той
    // форме, которую читает получатель обучения. Отвергнутая сторона здесь —
    // непереведённый источник, потому что у начального урока выданного ответа ещё не
    // существовало; в продуктовой коррекции там стоит фактически выданный ответ.
    private static TranslationLearningExample BuildSeedLesson(
        string lessonId,
        string sourceEn,
        string goldRu,
        TranslationSemanticTokenizer tokenizer)
    {
        // Структурная подпись берётся у продуктового семантического разборщика.
        var sequence = tokenizer.Tokenize(sourceEn, "en");
        // Вход урока: исходный текст, язык источника и тема запроса.
        var input = new ExecutorInput(sourceEn, "en", Topic);
        // Годная сторона: проверенный правильный перевод без выдуманных соответствий.
        var chosen = new ExecutorOutput(goldRu, 0.93, null, null);
        // Отвергнутая сторона начального урока: непереведённый источник.
        var rejected = new ExecutorOutput(sourceEn, 0.31, null, null);

        return new TranslationLearningExample(
            ExampleId: lessonId,
            UnitId: TranslationUnitIds.ExecutorFor(DesignatedDirection),
            FlowId: TranslationRoutingFlow.FlowId,
            InputJson: JsonSerializer.Serialize(input, LessonJson),
            ChosenOutputJson: JsonSerializer.Serialize(chosen, LessonJson),
            RejectedOutputJson: JsonSerializer.Serialize(rejected, LessonJson),
            RejectedType: "t_enru_untranslated_source",
            SemanticSignature: sequence.Signature,
            SourceId: "T-ENRU",
            HeldOut: false);
    }

    // ReadThroughProductAsync читает ответ НАСТОЯЩИМ продуктовым читателем: дирижёр
    // машины перевода, поток маршрутизации, обученный переходник направления и выдача
    // исполнительной единицы. Внутреннего словаря оснастка не читает.
    private static async Task<string> ReadThroughProductAsync(
        TranslationMachine machine,
        string sourceText,
        string probeId,
        string sourceLanguage = "en",
        string targetLanguage = "ru")
    {
        // Дирижёр выдаётся самой машиной — другого входа у продукта нет.
        var orchestrator = await machine.GetOrchestratorAsync();
        Assert.True(orchestrator.IsSuccess, orchestrator.ErrorMessage);

        // Запрос пользователя со своим направлением и темой.
        var request = new LogicalOrchestratorRequest(
            RunId: $"r34fb-{probeId}",
            ScenarioId: TranslationRoutingFlow.ScenarioId,
            UserRequest: sourceText,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                ["source_language"] = sourceLanguage,
                ["target_language"] = targetLanguage,
                ["topic_domain"] = Topic
            });

        // Ответ берётся из человеческого поля результата дирижёра.
        var result = await orchestrator.Value!.RunAsync(request);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!.FinalHumanResult;
    }

    // CountCorrectTrainRowsAsync считает, сколько из восьми учебных строк читаются
    // правильно настоящим продуктовым читателем. Это мера удержания усвоенного.
    private static async Task<int> CountCorrectTrainRowsAsync(TranslationMachine machine, string probePrefix)
    {
        // Счётчик правильных ответов на учебных строках.
        var correct = 0;
        foreach (var row in TrainRows)
        {
            var answer = await ReadThroughProductAsync(machine, row.SourceEn, $"{probePrefix}-{row.Id}");
            if (string.Equals(answer, row.GoldRu, StringComparison.Ordinal))
                correct++;
        }

        return correct;
    }

    // SnapshotNeighbourMemory снимает состояние ВСЕХ единиц, кроме назначенной:
    // разборщика, проверяющего и пяти других исполнительных единиц.
    private static string SnapshotNeighbourMemory(TranslationMachine machine)
    {
        // Слепок состояния берётся продуктовой выгрузкой, а не чтением полей.
        var export = machine.ExportCheckpoint();
        Assert.True(export.IsSuccess, export.ErrorMessage);
        var checkpoint = export.Value!;

        // Собираются только посторонние части состояния.
        var neighbours = new
        {
            tokenizer = checkpoint.SemanticTokenizer,
            verifier = checkpoint.QualityVerifier,
            executors = checkpoint.Executors
                .Where(executor => executor.Direction != DesignatedDirection)
                .OrderBy(executor => executor.Direction.ToString(), StringComparer.Ordinal)
                .ToList()
        };

        return JsonSerializer.Serialize(neighbours, TranslationCheckpointJsonOptions.Standard);
    }

    // SnapshotDesignatedMemory снимает состояние ТОЛЬКО назначенной единицы.
    private static string SnapshotDesignatedMemory(TranslationMachine machine)
    {
        // Тот же продуктовый слепок, другая его часть.
        var export = machine.ExportCheckpoint();
        Assert.True(export.IsSuccess, export.ErrorMessage);
        var designated = export.Value!.Executors.Single(executor => executor.Direction == DesignatedDirection);
        return JsonSerializer.Serialize(designated, TranslationCheckpointJsonOptions.Standard);
    }

    // SnapshotSharedPairs снимает переводы общей памяти ближайших соседей. Эта память
    // по устройству продукта ОДНА на все направления и является назначенным мостом
    // между обучением и чтением, поэтому её рост — ожидаемое следствие доставки.
    private static List<string> SnapshotSharedPairs(TranslationMachine machine)
    {
        // Тот же продуктовый слепок, часть общей памяти.
        var export = machine.ExportCheckpoint();
        Assert.True(export.IsSuccess, export.ErrorMessage);
        return export.Value!.KnnStore.Entries.Select(entry => entry.Target).OrderBy(t => t, StringComparer.Ordinal).ToList();
    }

    // Subtract возвращает переводы, которых в прежнем слепке общей памяти не было.
    private static IReadOnlyList<string> Subtract(List<string> after, List<string> before)
    {
        // Копия прежнего списка: из неё вычёркиваются совпадения по одному.
        var remaining = new List<string>(before);
        var added = new List<string>();
        foreach (var target in after)
        {
            // Совпавший перевод вычёркивается, несовпавший считается добавленным.
            if (!remaining.Remove(target))
                added.Add(target);
        }

        return added;
    }

    // ── Доступ к предлагаемому карточкой переходнику по имени ─────────────────────

    // FindProductType ищет продуктовый тип по имени в сборке доменного проекта перевода.
    private static Type? FindProductType(string fullName)
        => typeof(TranslationMachine).Assembly.GetType(fullName, throwOnError: false);

    // CreateAdapterAndCorrection создаёт переходник и запись подтверждённой коррекции.
    private static (object Adapter, object Correction, Type AdapterType) CreateAdapterAndCorrection(
        string lessonId,
        string sourceText,
        string actualRejectedAnswer,
        string verifiedCorrectAnswer,
        string verificationProvenance)
    {
        // Тип переходника обязан существовать в продукте.
        var adapterType = FindProductType(AdapterTypeName);
        Assert.NotNull(adapterType);
        // Тип записи подтверждённой коррекции обязан существовать в продукте.
        var correctionType = FindProductType(CorrectionTypeName);
        Assert.NotNull(correctionType);

        // Переходник создаётся без доводов: своего состояния у него нет.
        var adapter = Activator.CreateInstance(adapterType!);
        Assert.NotNull(adapter);

        // Запись коррекции собирается по объявленному порядку полей.
        var correction = Activator.CreateInstance(
            correctionType!,
            lessonId,
            DesignatedDirection,
            Topic,
            sourceText,
            actualRejectedAnswer,
            verifiedCorrectAnswer,
            0.0,
            verificationProvenance);
        Assert.NotNull(correction);

        return (adapter!, correction!, adapterType!);
    }

    // BuildLesson вызывает сборку урока без записи в память.
    private static OperationResult<TranslationLearningExample> BuildLesson(
        string lessonId,
        string sourceText,
        string actualRejectedAnswer,
        string verifiedCorrectAnswer,
        string verificationProvenance)
    {
        // Переходник и запись коррекции создаются общей оснасткой.
        var (adapter, correction, adapterType) = CreateAdapterAndCorrection(
            lessonId, sourceText, actualRejectedAnswer, verifiedCorrectAnswer, verificationProvenance);

        // Сборка урока обязана быть открытым членом переходника.
        var method = adapterType.GetMethod("BuildLesson", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(method);

        // Результат сборки — обычный конверт результата операции общей инфраструктуры.
        var invoked = method!.Invoke(adapter, [correction]);
        return Assert.IsAssignableFrom<OperationResult<TranslationLearningExample>>(invoked);
    }

    // DeliverAsync вызывает доставку подтверждённой коррекции в существующий приёмник
    // обучения назначенной единицы.
    private static async Task<OperationResult<LearningBatchResult>> DeliverAsync(
        TranslationMachine machine,
        string lessonId,
        string sourceText,
        string actualRejectedAnswer,
        string verifiedCorrectAnswer,
        string verificationProvenance)
    {
        // Переходник и запись коррекции создаются общей оснасткой.
        var (adapter, correction, adapterType) = CreateAdapterAndCorrection(
            lessonId, sourceText, actualRejectedAnswer, verifiedCorrectAnswer, verificationProvenance);

        // Доставка обязана быть открытым членом переходника.
        var method = adapterType.GetMethod("DeliverAsync", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(method);

        // Доставка возвращает задачу с обычным конвертом результата партии обучения.
        var invoked = method!.Invoke(adapter, [machine, correction, CancellationToken.None]);
        var task = Assert.IsAssignableFrom<Task<OperationResult<LearningBatchResult>>>(invoked);
        return await task;
    }

    // CreateAdapterAndCorrection с ЯВНО названным направлением получателя. Прежний вызов
    // без направления сохраняет своё поведение и обучает назначенную единицу; эта
    // перегрузка нужна контролю чувствительности, где получатель — СОСЕДНЯЯ единица.
    private static (object Adapter, object Correction, Type AdapterType) CreateAdapterAndCorrection(
        TranslationDirection direction,
        string lessonId,
        string sourceText,
        string actualRejectedAnswer,
        string verifiedCorrectAnswer,
        string verificationProvenance)
    {
        // Тип переходника обязан существовать в продукте.
        var adapterType = FindProductType(AdapterTypeName);
        Assert.NotNull(adapterType);
        // Тип записи подтверждённой коррекции обязан существовать в продукте.
        var correctionType = FindProductType(CorrectionTypeName);
        Assert.NotNull(correctionType);

        // Переходник создаётся без доводов: своего состояния у него нет.
        var adapter = Activator.CreateInstance(adapterType!);
        Assert.NotNull(adapter);

        // Запись коррекции собирается по объявленному порядку полей, с названным
        // направлением получателя вместо постоянного назначенного.
        var correction = Activator.CreateInstance(
            correctionType!,
            lessonId,
            direction,
            Topic,
            sourceText,
            actualRejectedAnswer,
            verifiedCorrectAnswer,
            0.0,
            verificationProvenance);
        Assert.NotNull(correction);

        return (adapter!, correction!, adapterType!);
    }

    // DeliverAsync с ЯВНО названным направлением получателя: тот же продуктовый
    // переходник и тот же приёмник обучения, другая обучаемая единица.
    private static async Task<OperationResult<LearningBatchResult>> DeliverAsync(
        TranslationMachine machine,
        TranslationDirection direction,
        string lessonId,
        string sourceText,
        string actualRejectedAnswer,
        string verifiedCorrectAnswer,
        string verificationProvenance)
    {
        // Переходник и запись коррекции создаются перегрузкой с направлением.
        var (adapter, correction, adapterType) = CreateAdapterAndCorrection(
            direction, lessonId, sourceText, actualRejectedAnswer, verifiedCorrectAnswer, verificationProvenance);

        // Доставка обязана быть открытым членом переходника.
        var method = adapterType.GetMethod("DeliverAsync", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(method);

        // Доставка возвращает задачу с обычным конвертом результата партии обучения.
        var invoked = method!.Invoke(adapter, [machine, correction, CancellationToken.None]);
        var task = Assert.IsAssignableFrom<Task<OperationResult<LearningBatchResult>>>(invoked);
        return await task;
    }
}
