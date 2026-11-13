// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Два незакрытых пункта приёмки карточки переноса для среза «доставить проверенное
// исправление существующей переводческей единице». Основание — карточка
// docs/plans/replanning-possible-diffs/fixes-based-on-experiments/
// 106-feedback-and-transition-memory-plan-2026-10-30.md, раздел R34-FB, пункты 1 и 4.
//
// Что здесь закрывается, по-человечески.
//
// Пункт 1 карточки. Плечи «до» и «после» обязаны получаться СОХРАНЕНИЕМ И ЗАГРУЗКОЙ
// слепка состояния, а не двумя независимо обученными холодными машинами. Карточка
// называет это дословно: «Создать A/B через SaveCheckpointAsync/LoadCheckpointAsync, а
// не метаданные ExportAsync/ImportAsync». Прежний опубликованный срез проверок этого не
// делал: он читал ответы до доставки и после доставки на ОДНОЙ живой машине, то есть
// плеча «до» как восстановленного состояния у него не было вовсе.
//
// Пункт 4 карточки. Слепок состояния ПОСЛЕ доставки обязан читаться в НОВОМ процессе,
// где обучения не было вовсе, и отдавать оба проверенных правильных перевода.
//
// Чем «плечо через загрузку» отличается от второй холодной машины, и как это видно
// проверке. Вторая холодная машина, обученная теми же восемью уроками, дала бы те же
// восемь правильных учебных ответов, и подмену было бы не отличить. Поэтому ниже
// проверяется не только то, ЧТО отвечает загруженное плечо, но и то, что оно отвечает
// РОВНО теми же текстами, что живая машина отдавала до доставки, знак в знак, на обеих
// исправляемых строках, и при этом удержание восьми усвоенных строк у него полное.
// Холодная необученная машина обоих этих условий сразу выполнить не может: без восьми
// уроков у неё не было бы удержания, а второе обучение не воспроизвело бы прежние
// неправильные ответы как следствие ЗАГРУЗКИ, а лишь как следствие повторного обучения.
//
// Чего эти проверки НЕ доказывают.
//   * Общий механизм обратной связи готовым не объявляется: доставляется одна пара одной
//     назначенной единице.
//   * Автоматическая связь производителя пары с доменным получателем здесь не создаётся.
//   * Обобщения на отложенных данных здесь нет: две исправленные строки после коррекции
//     становятся учебными, и их повторное чтение — это чтение усвоенного исправления.
//   * Идемпотентность повторной доставки одного опознавателя урока остаётся явно открытой
//     предпосылкой: договор получателя на это ещё не назван.
//
// Исторические наблюдения карточки. Карточка называет прежние неправильные ответы «Анна
// закрыла окно» и «Борис закрыл дверь». Сегодняшний продукт отвечает иначе: «Анна не
// открыла дверь» на обе исправляемые строки. Поэтому дословный текст прежнего ответа
// ожиданием НЕ делается; ожиданием делается равенство ответа загруженного плеча «до»
// фактическому ответу живой машины до доставки.

namespace LlmFromScratch.Tests;

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure.Logical;
using TranslationDomain;
using Xunit.Abstractions;

// R34FbSnapshotArmsFixture — общая оснастка обоих плеч: учебные данные, исправляемые
// строки, сборка уроков, чтение настоящим продуктовым читателем и пути слепков.
// Вынесена отдельно, потому что ею пользуются ДВА разных класса проверок, и класс
// свежей загрузки обязан оставаться без единого вызова обучения.
internal static class R34FbSnapshotArmsFixture
{
    // Topic — тема запроса. Тема входит в адрес точной памяти единицы, поэтому она одна и
    // та же в обучении и в чтении; новых тем этот срез не вводит.
    internal const string Topic = "general";

    // DesignatedDirection — направление назначенной обучаемой единицы: английский→русский.
    internal const TranslationDirection DesignatedDirection = TranslationDirection.EN_RU;

    // EvidenceCheckpointFileName — имя сохранённого слепка состояния ПОСЛЕ доставки,
    // который читает проверка свежей загрузки в новом процессе.
    internal const string EvidenceCheckpointFileName = "after-delivery.checkpoint.json";

    // TrainRows — учебный набор из восьми строк, дословно тот же, что в опыте P4201.
    internal static readonly (string Id, string SourceEn, string GoldRu)[] TrainRows =
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

    // ExamRows — две исправляемые строки опыта P4201 и их проверенные правильные ответы.
    // Правильный ответ хранится здесь, в учебной оснастке, а не в продуктовой функции
    // выбора ответа.
    internal static readonly (string Id, string SourceEn, string GoldRu)[] ExamRows =
    [
        ("e5", "Anna did not open the window", "Анна не открыла окно"),
        ("e7", "Boris did not open the door", "Борис не открыл дверь")
    ];

    // LessonJson — настройки записи учебных текстов: перечисления пишутся словами, как их
    // читает получатель обучения.
    private static readonly JsonSerializerOptions LessonJson = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    // WorkingCheckpointDirectory — рабочая папка слепков этого прогона. Она лежит ВНЕ
    // хранилища версий: прогон проверок не имеет права править отслеживаемые улики.
    internal static string WorkingCheckpointDirectory
        => Path.Combine(Path.GetTempPath(), "r34fb-attempt-0002");

    // EvidenceDirectory — папка улик этой попытки внутри рабочего каталога проекта.
    internal static string EvidenceDirectory
        => Path.Combine(FindRepositoryRoot(), "artifacts", "fbe", "R34-FB", "attempt-0002");

    // EvidenceCheckpointPath — полный путь сохранённого слепка состояния ПОСЛЕ доставки.
    internal static string EvidenceCheckpointPath
        => Path.Combine(EvidenceDirectory, EvidenceCheckpointFileName);

    // FindRepositoryRoot находит корень рабочего каталога проекта по двум приметам сразу:
    // файлу правил агентов и папке улик. Одной приметы мало: папка с похожим именем могла
    // бы встретиться выше по дереву.
    internal static string FindRepositoryRoot()
    {
        // Поиск начинается от папки собранных проверок и идёт вверх по дереву.
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            // Корень опознан, когда рядом есть и файл правил агентов, и папка улик.
            if (File.Exists(Path.Combine(current.FullName, "AGENTS.md"))
                && Directory.Exists(Path.Combine(current.FullName, "artifacts")))
            {
                return current.FullName;
            }

            // Иначе подъём на один уровень выше.
            current = current.Parent;
        }

        // Без корня пути улик не построить, и молча подставлять другую папку нельзя.
        throw new InvalidOperationException("Корень рабочего каталога проекта не найден.");
    }

    // ColdMachineWithEightLessonsAsync создаёт холодную машину перевода и обучает её
    // восемью общими уроками через уже существующий продуктовый вход обучения.
    internal static async Task<TranslationMachine> ColdMachineWithEightLessonsAsync()
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
            BatchId: "r34fb-a0002-base-eight",
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
    internal static TranslationLearningExample BuildSeedLesson(
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

    // ReadThroughProductAsync читает ответ НАСТОЯЩИМ продуктовым читателем: дирижёр машины
    // перевода, поток маршрутизации, обученный переходник направления и выдача
    // исполнительной единицы. Внутреннего словаря оснастка не читает.
    internal static async Task<string> ReadThroughProductAsync(
        TranslationMachine machine,
        string sourceText,
        string probeId)
    {
        // Дирижёр выдаётся самой машиной — другого входа у продукта нет.
        var orchestrator = await machine.GetOrchestratorAsync();
        Assert.True(orchestrator.IsSuccess, orchestrator.ErrorMessage);

        // Запрос пользователя со своим направлением и темой.
        var request = new LogicalOrchestratorRequest(
            RunId: $"r34fb-a0002-{probeId}",
            ScenarioId: TranslationRoutingFlow.ScenarioId,
            UserRequest: sourceText,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                ["source_language"] = "en",
                ["target_language"] = "ru",
                ["topic_domain"] = Topic
            });

        // Ответ берётся из человеческого поля результата дирижёра.
        var result = await orchestrator.Value!.RunAsync(request);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!.FinalHumanResult;
    }

    // CountCorrectTrainRowsAsync считает, сколько из восьми учебных строк читаются
    // правильно настоящим продуктовым читателем. Это мера удержания усвоенного.
    internal static async Task<int> CountCorrectTrainRowsAsync(
        TranslationMachine machine,
        string probePrefix)
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

    // DeliverVerifiedCorrectionAsync доставляет ОДНУ подтверждённую коррекцию назначенной
    // единице через продуктовый переходник подтверждённой коррекции. Типы берутся прямо,
    // без поиска по имени: на опубликованном продукте они уже существуют.
    internal static Task<AgentInfrastructure.OperationResult<LearningBatchResult>> DeliverVerifiedCorrectionAsync(
        TranslationMachine machine,
        string lessonId,
        string sourceText,
        string actualRejectedAnswer,
        string verifiedCorrectAnswer)
    {
        // Запись подтверждённой коррекции: происхождение проверки названо человеческими
        // словами, уверенность проверки не выдумывается и остаётся нулевой.
        var correction = new TranslationVerifiedCorrection(
            LessonId: lessonId,
            Direction: DesignatedDirection,
            TopicDomain: Topic,
            SourceText: sourceText,
            ActualRejectedAnswer: actualRejectedAnswer,
            VerifiedCorrectAnswer: verifiedCorrectAnswer,
            VerifiedAnswerConfidence: 0.0,
            VerificationProvenance: "InternalResultExaminer: missing predicate 'не'");

        // Доставка идёт через продуктовый переходник в существующий приёмник обучения.
        return new TranslationCorrectionLearningAdapter().DeliverAsync(machine, correction);
    }
}

// TranslationCorrectionSnapshotArmsTests — пункт 1 приёмки: плечо «до» получается
// ЗАГРУЗКОЙ сохранённого слепка состояния, а не вторым холодным обучением.
public sealed class TranslationCorrectionSnapshotArmsTests
{
    // _output печатает панель плеч: запрос, ответ до доставки, ответ после доставки и
    // ответ загруженного плеча «до».
    private readonly ITestOutputHelper _output;

    // Конструктор получает приёмник вывода проверки от исполнителя проверок.
    public TranslationCorrectionSnapshotArmsTests(ITestOutputHelper output)
    {
        // Сохраняем приёмник вывода: панель должна попасть в журнал прогона.
        _output = output;
    }

    // Пункт 1. Слепок состояния сохраняется ДО доставки; доставка выполняется; затем
    // сохранённый слепок «до» ЗАГРУЖАЕТСЯ, и оба запроса снова отвечают ровно теми
    // неправильными текстами, что были до доставки. Попутно сохраняется слепок состояния
    // ПОСЛЕ доставки — его читает проверка свежей загрузки в новом процессе.
    [Fact]
    public async Task Decisive07_BeforeArmComesFromLoadingTheSavedSnapshotNotFromSecondColdTraining()
    {
        // Рабочая папка слепков этого прогона лежит вне хранилища версий.
        Directory.CreateDirectory(R34FbSnapshotArmsFixture.WorkingCheckpointDirectory);
        var beforePath = Path.Combine(
            R34FbSnapshotArmsFixture.WorkingCheckpointDirectory, "before-delivery.checkpoint.json");
        var afterPath = Path.Combine(
            R34FbSnapshotArmsFixture.WorkingCheckpointDirectory, "after-delivery.checkpoint.json");

        // Живая машина с восемью общими уроками: это единственная обучаемая машина опыта.
        var live = await R34FbSnapshotArmsFixture.ColdMachineWithEightLessonsAsync();

        // Ответы ДО доставки читаются настоящим продуктовым читателем и запоминаются
        // дословно: именно с ними будет сверяться загруженное плечо «до».
        var liveBefore = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var row in R34FbSnapshotArmsFixture.ExamRows)
        {
            liveBefore[row.Id] = await R34FbSnapshotArmsFixture.ReadThroughProductAsync(
                live, row.SourceEn, $"decisive07-live-before-{row.Id}");
            // До доставки ответ не равен проверенному правильному — иначе исправлять нечего.
            Assert.NotEqual(row.GoldRu, liveBefore[row.Id]);
        }

        // Удержание усвоенного до доставки полное: все восемь учебных строк верны.
        Assert.Equal(
            R34FbSnapshotArmsFixture.TrainRows.Length,
            await R34FbSnapshotArmsFixture.CountCorrectTrainRowsAsync(live, "decisive07-live-before"));

        // СОХРАНЕНИЕ слепка состояния ДО доставки штатным продуктовым путём сохранения.
        var savedBefore = await live.SaveCheckpointAsync(beforePath);
        Assert.True(savedBefore.IsSuccess, savedBefore.ErrorMessage);
        Assert.True(File.Exists(beforePath));

        // Доставка двух подтверждённых коррекций назначенной единице живой машины.
        foreach (var row in R34FbSnapshotArmsFixture.ExamRows)
        {
            var delivered = await R34FbSnapshotArmsFixture.DeliverVerifiedCorrectionAsync(
                live,
                lessonId: $"r34fb-a0002-correction-{row.Id}",
                sourceText: row.SourceEn,
                actualRejectedAnswer: liveBefore[row.Id],
                verifiedCorrectAnswer: row.GoldRu);
            Assert.True(delivered.IsSuccess, delivered.ErrorMessage);
            // Урок принят ИМЕННО назначенной единицей, а не молча пропущен.
            Assert.Contains(
                TranslationUnitIds.ExecutorFor(R34FbSnapshotArmsFixture.DesignatedDirection),
                delivered.Value!.PromotedUnitIds);
        }

        // Плечо «после» — это та же живая машина после доставки: оба ответа стали
        // проверенными правильными.
        var liveAfter = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var row in R34FbSnapshotArmsFixture.ExamRows)
        {
            liveAfter[row.Id] = await R34FbSnapshotArmsFixture.ReadThroughProductAsync(
                live, row.SourceEn, $"decisive07-live-after-{row.Id}");
            Assert.Equal(row.GoldRu, liveAfter[row.Id]);
        }

        // СОХРАНЕНИЕ слепка состояния ПОСЛЕ доставки: его читает проверка свежей загрузки.
        var savedAfter = await live.SaveCheckpointAsync(afterPath);
        Assert.True(savedAfter.IsSuccess, savedAfter.ErrorMessage);
        Assert.True(File.Exists(afterPath));

        // ЗАГРУЗКА сохранённого слепка «до» штатным продуктовым путём загрузки. Это и есть
        // плечо «до»: состояние восстановлено из слепка, второго обучения здесь нет.
        var loadedBeforeArm = await TranslationMachine.LoadCheckpointAsync(beforePath);
        Assert.True(loadedBeforeArm.IsSuccess, loadedBeforeArm.ErrorMessage);
        var beforeArm = loadedBeforeArm.Value!.Machine;
        // Загруженное плечо — ДРУГОЙ объект машины, а не та же живая ссылка.
        Assert.NotSame(live, beforeArm);

        // Оба запроса на загруженном плече «до» отвечают ровно теми текстами, что живая
        // машина отдавала ДО доставки, знак в знак.
        _output.WriteLine("Панель плеч через сохранение и загрузку слепка состояния:");
        foreach (var row in R34FbSnapshotArmsFixture.ExamRows)
        {
            var answerOnBeforeArm = await R34FbSnapshotArmsFixture.ReadThroughProductAsync(
                beforeArm, row.SourceEn, $"decisive07-loaded-before-{row.Id}");

            _output.WriteLine($"  запрос:                      {row.SourceEn}");
            _output.WriteLine($"  живой ответ до доставки:     {liveBefore[row.Id]}");
            _output.WriteLine($"  живой ответ после доставки:  {liveAfter[row.Id]}");
            _output.WriteLine($"  ответ плеча «до» из слепка:  {answerOnBeforeArm}");
            _output.WriteLine($"  проверенный правильный:      {row.GoldRu}");

            // Плечо «до» вернуло прежний неправильный ответ дословно.
            Assert.Equal(liveBefore[row.Id], answerOnBeforeArm);
            // И он по-прежнему не равен проверенному правильному.
            Assert.NotEqual(row.GoldRu, answerOnBeforeArm);
            // И он не равен ответу плеча «после»: плечи действительно разные.
            Assert.NotEqual(liveAfter[row.Id], answerOnBeforeArm);
        }

        // Загруженное плечо «до» — это ВОССТАНОВЛЕННОЕ обученное состояние, а не холодная
        // машина: удержание восьми усвоенных строк у него полное. Холодная машина без
        // восьми уроков этого условия выполнить не может.
        Assert.Equal(
            R34FbSnapshotArmsFixture.TrainRows.Length,
            await R34FbSnapshotArmsFixture.CountCorrectTrainRowsAsync(beforeArm, "decisive07-loaded-before"));

        // Плечо «после», загруженное из своего слепка, отдаёт проверенные правильные
        // ответы: сохранение и загрузка переносят именно доставленный урок.
        var loadedAfterArm = await TranslationMachine.LoadCheckpointAsync(afterPath);
        Assert.True(loadedAfterArm.IsSuccess, loadedAfterArm.ErrorMessage);
        foreach (var row in R34FbSnapshotArmsFixture.ExamRows)
        {
            var answerOnAfterArm = await R34FbSnapshotArmsFixture.ReadThroughProductAsync(
                loadedAfterArm.Value!.Machine, row.SourceEn, $"decisive07-loaded-after-{row.Id}");
            Assert.Equal(row.GoldRu, answerOnAfterArm);
        }

        // Живая машина после всех загрузок по-прежнему в состоянии «после»: загрузка в
        // другие машины её состояния не тронула.
        foreach (var row in R34FbSnapshotArmsFixture.ExamRows)
        {
            var stillAfter = await R34FbSnapshotArmsFixture.ReadThroughProductAsync(
                live, row.SourceEn, $"decisive07-live-still-after-{row.Id}");
            Assert.Equal(row.GoldRu, stillAfter);
        }

        // Пути сохранённых слепков печатаются: по ним снимаются улики прогона.
        _output.WriteLine($"слепок «до»:    {Path.GetFullPath(beforePath)}");
        _output.WriteLine($"слепок «после»: {Path.GetFullPath(afterPath)}");
    }
}

// TranslationCorrectionFreshLoadTests — пункт 4 приёмки: слепок состояния ПОСЛЕ доставки
// читается в НОВОМ процессе, где обучения не было вовсе.
//
// Почему этот класс отдельный и почему он читает сохранённую улику. Проверка обязана
// доказать, что обученного состояния в её процессе никто не создавал. Поэтому класс
// вынесен отдельным — его можно отобрать узким фильтром в собственный процесс прогона, —
// и читает он слепок, записанный РАНЬШЕ и ДРУГИМ процессом: файл улики
// artifacts/fbe/R34-FB/attempt-0002/after-delivery.checkpoint.json. Ни одного вызова
// обучения в этом классе нет, и это видно по его исходнику целиком.
public sealed class TranslationCorrectionFreshLoadTests
{
    // _output печатает панель свежей загрузки: номер процесса, время записи слепка,
    // время начала процесса и оба прочитанных ответа.
    private readonly ITestOutputHelper _output;

    // Конструктор получает приёмник вывода проверки от исполнителя проверок.
    public TranslationCorrectionFreshLoadTests(ITestOutputHelper output)
    {
        // Сохраняем приёмник вывода: панель должна попасть в журнал прогона.
        _output = output;
    }

    // Пункт 4. Сохранённый слепок состояния ПОСЛЕ доставки загружается в процессе, который
    // обучения не выполнял, и оба запроса читаются настоящим продуктовым читателем: оба
    // ответа обязаны быть проверенными правильными, а восемь усвоенных строк — удержанными.
    [Fact]
    public async Task Decisive08_FreshLoadWithoutAnyTrainingInThisProcessReturnsBothVerifiedTranslations()
    {
        // Путь сохранённой улики слепка состояния ПОСЛЕ доставки.
        var checkpointPath = R34FbSnapshotArmsFixture.EvidenceCheckpointPath;

        // Без файла слепка доказывать нечего, и подменять его обучением в этом процессе
        // запрещено: тогда проверка перестала бы быть проверкой свежей загрузки.
        Assert.True(
            File.Exists(checkpointPath),
            $"Слепок состояния после доставки не найден: {checkpointPath}. " +
            "Его записывает проверка плеч " +
            $"{nameof(TranslationCorrectionSnapshotArmsTests)}." +
            $"{nameof(TranslationCorrectionSnapshotArmsTests.Decisive07_BeforeArmComesFromLoadingTheSavedSnapshotNotFromSecondColdTraining)}, " +
            "после чего файл сохраняется в папку улик этой попытки.");

        // Время записи слепка и время начала ЭТОГО процесса читаются как доказательство
        // того, что состояние создал другой, более ранний процесс.
        var checkpointWrittenUtc = File.GetLastWriteTimeUtc(checkpointPath);
        using var thisProcess = Process.GetCurrentProcess();
        var processStartedUtc = thisProcess.StartTime.ToUniversalTime();

        _output.WriteLine("Панель свежей загрузки слепка состояния в новом процессе:");
        _output.WriteLine($"  номер этого процесса:        {thisProcess.Id}");
        _output.WriteLine($"  процесс начался (UTC):       {processStartedUtc:O}");
        _output.WriteLine($"  слепок записан (UTC):        {checkpointWrittenUtc:O}");
        _output.WriteLine($"  файл слепка:                 {checkpointPath}");

        // Слепок записан СТРОГО раньше начала этого процесса: значит обученное состояние
        // создано не здесь. Это и есть машинно проверяемый признак «нового процесса».
        Assert.True(
            checkpointWrittenUtc < processStartedUtc,
            $"Слепок записан в {checkpointWrittenUtc:O}, а процесс начался в {processStartedUtc:O}: " +
            "файл не старше процесса, поэтому утверждать, что состояние пришло из другого " +
            "процесса, нельзя.");

        // ЗАГРУЗКА слепка штатным продуктовым путём загрузки. Повторов обучения этот путь
        // не делает — ни одного вызова обучения в этом классе нет.
        var loaded = await TranslationMachine.LoadCheckpointAsync(checkpointPath);
        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        var restored = loaded.Value!.Machine;

        // Оба исправленных запроса читаются настоящим продуктовым читателем.
        foreach (var row in R34FbSnapshotArmsFixture.ExamRows)
        {
            var answer = await R34FbSnapshotArmsFixture.ReadThroughProductAsync(
                restored, row.SourceEn, $"decisive08-fresh-load-{row.Id}");

            _output.WriteLine($"  запрос:                      {row.SourceEn}");
            _output.WriteLine($"  ответ после свежей загрузки: {answer}");
            _output.WriteLine($"  проверенный правильный:      {row.GoldRu}");

            // Оба перевода обязаны быть проверенными правильными.
            Assert.Equal(row.GoldRu, answer);
        }

        // Удержание усвоенного тоже переносится загрузкой: все восемь учебных строк верны.
        var retained = await R34FbSnapshotArmsFixture.CountCorrectTrainRowsAsync(
            restored, "decisive08-fresh-load");
        _output.WriteLine($"  удержано учебных строк:      {retained} из {R34FbSnapshotArmsFixture.TrainRows.Length}");
        Assert.Equal(R34FbSnapshotArmsFixture.TrainRows.Length, retained);
    }
}
