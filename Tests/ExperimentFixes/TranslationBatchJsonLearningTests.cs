// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное покрытие переноса «нечитаемый JSON поздней строки не оставляет
// ранний урок в переводчике» (назначение «отказ партии» раздела 3 плана
// docs/plans/replanning-possible-diffs/fixes-based-on-experiments/
// 109-p53-p55-transfer-plan-2026-10-30.md; основание — опыт отказа партии
// P53-PRIMARY-01, подслучай P53.1).
//
// Что здесь проверяется по-человечески. Переводчик учится партиями: в одной
// партии приходит несколько учебных строк, каждая строка несёт вход и
// правильный ответ в виде текста JSON. Продуктовый цикл обучения шёл по
// строкам подряд и записывал урок каждой строки в память сразу. Если текст
// JSON второй строки нельзя прочитать, разборщик прерывал работу уже ПОСЛЕ
// того, как первая строка была записана. Партия считалась неприменённой, а
// память переводчика при этом уже помнила половину партии: три пары
// ассоциативной памяти, запись точного совпадения источника и опознание
// учебной строки. Это измеренный след дефекта, а не предположение.
//
// Исправление, выбранное планом: перед первым обновлением памяти пройти все
// потенциально обучаемые строки партии и проверить, что их тексты JSON вообще
// читаются. Нечитаемая строка даёт управляемый отказ с кодом
// BATCH_ROW_UNREADABLE и настоящим идентификатором строки, и ни одной записи
// состояния, версии, истории или опознаний при этом не происходит.
//
// Два плеча измерения:
//   плечо A (база) — на НЕИЗМЕНЁННОМ продуктовом коде: настройка одним уроком,
//                    затем исправная пара из двух уроков. Плечо A обязано быть
//                    зелёным и до правки, и после неё: это положительный
//                    контроль и основание сравнения цены;
//   плечо B (кандидат) — то же самое плюс негодная партия перед исправной,
//                    сохранение и свежая загрузка. Плечо B обязано падать на
//                    НЕИЗМЕНЁННОМ коде (на значениях, а не на сборке) и быть
//                    зелёным после правки.
//
// Показания каждого плеча пишутся в файл показаний ДО утверждений, в блоке
// finally, поэтому красный прогон плеча B всё равно оставляет измеренный след
// дефекта, а не обрывает измерение.
//
// Чего эти проверки НЕ доказывают. Они не обещают атомарность партии при любой
// ошибке времени выполнения, при отмене или при отказе публикации: закрыт
// только случай нечитаемого текста JSON. Текст, который синтаксически верен,
// но не соответствует типу или смыслу структуры данных, остаётся незакрытой
// границей. Повторная доставка одной и той же строки здесь тоже не
// рассматривается: отдельного сравнения по списку опознаний эта правка не
// добавляет. Свежая загрузка выполняется новым объектом переводчика в том же
// процессе операционной системы, а не отдельным холодным процессом.
namespace LlmFromScratch.Tests;

using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure.Logical;
using TranslationDomain;

public sealed class TranslationBatchJsonLearningTests
{
    // Имя назначения и номер попытки: по ним складывается путь показаний.
    private const string FixId = "FIX-P53-BATCH-JSON";
    private const string AttemptId = "attempt-0001";

    // Источник корпуса учебных строк этой приёмки задан планом дословно.
    private const string SourceId = "r53-native-acceptance-v1";

    // Тема, направление и языки приёмки заданы планом: тема general, EN_RU.
    private const string Topic = "general";
    private const string SourceLang = "en";
    private const string TargetLang = "ru";
    private static readonly TranslationDirection Direction = TranslationDirection.EN_RU;

    // Код управляемого отказа нечитаемой строки, заданный планом дословно.
    private const string UnreadableRowCode = "BATCH_ROW_UNREADABLE";

    // Заведомо нечитаемый текст правильного ответа, заданный планом дословно.
    private const string BrokenChosenJson = "{";

    // Идентификатор нечитаемой строки, заданный планом дословно.
    private const string InvalidExampleId = "R53-invalid";

    // Настроечный урок: единственная учебная пара начального состояния.
    private static readonly LessonPair SetupLesson = new("R53-t01", "keep", "сохрани");

    // Первая строка исправной пары — законный урок открытия файла.
    private static readonly LessonPair OpenLesson = new("R53-t02", "open file", "открой файл");

    // Вторая строка исправной пары — законный урок закрытия файла.
    private static readonly LessonPair CloseLesson = new("R53-t03", "close file", "закрой файл");

    // Панель независимого экзамена: три обращения к обычному потребителю.
    // Эталон панели задан заранее и в ответ не подставляется.
    private static readonly PanelRow[] Panel =
    [
        new("p1", "keep", "сохрани", "настроечный урок начального состояния"),
        new("p2", "open file", "открой файл", "первая строка исправной пары"),
        new("p3", "close file", "закрой файл", "вторая строка исправной пары")
    ];

    // Настройки записи учебных текстов JSON повторяют настройки опыта:
    // без отступов, с безопасным для кириллицы кодировщиком и строковыми
    // значениями перечислений.
    private static readonly JsonSerializerOptions UnitJson = new()
    {
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    // Настройки устойчивого отпечатка состояния: без отступов, чтобы отпечаток
    // не зависел от переносов строк.
    private static readonly JsonSerializerOptions StableJson = new()
    {
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    // Настройки записи файла показаний: с отступами, чтобы человек читал его
    // глазами без дополнительной обработки.
    private static readonly JsonSerializerOptions ReceiptJson = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    // ── Плечо A: база на неизменённом продуктовом коде ────────────────────────
    //
    // Назначенные дозы плеча A: 2 вызова продуктового обучения партии,
    // 3 назначенные строки, 3 настоящих успешных урока, 9 обычных чтений.
    [Fact]
    public void BaselineArm_SetupThenValidPair_TeachesThreeAnswers()
    {
        // Журнал показаний плеча заполняется по ходу и пишется в finally,
        // поэтому измерение сохраняется даже при падении утверждений.
        var meter = new ArmMeter("A", "база на текущем продуктовом коде до правки");

        try
        {
            // Подготовка: создание переводчика и построение учебных строк.
            var machine = meter.Measure(
                "подготовка: структуры данных и зависимости",
                () => new TranslationMachine());

            // Холодная панель: до любого обучения все три строки обязаны быть
            // в исходном отказе, то есть в обёртке заготовки.
            var cold = meter.Measure(
                "холодная панель (3 чтения)",
                () => ReadPanel(machine));
            meter.RecordPanel("холодная панель", cold);
            Assert.Equal(0, cold.Count(answer => answer.ExactCorrect));
            Assert.All(cold, answer => Assert.Contains("BOOTSTRAP", answer.Answer, StringComparison.Ordinal));

            // Настроечная партия: один законный урок «keep».
            var setup = meter.Measure(
                "обучение: настроечная партия (1 строка)",
                () => TrainBatch(machine, "r53-setup", [BuildExample(SetupLesson)]));
            meter.RecordBatch("настроечная партия", setup, assignedRows: 1);
            Assert.True(setup.IsSuccess, setup.ErrorMessage);
            Assert.Equal(1, setup.Value!.RowsTrained);

            // Панель после настройки: переведён только «keep».
            var afterSetup = meter.Measure(
                "панель после настройки (3 чтения)",
                () => ReadPanel(machine));
            meter.RecordPanel("панель после настройки", afterSetup);
            AssertOnlySetupLessonAnswered(afterSetup);

            // Исправная пара: два законных урока — открытие и закрытие файла.
            // Именно эта фаза служит основанием сравнения цены между плечами:
            // одинаковая настройка, одинаковые входы, одинаковые дозы.
            var valid = meter.Measure(
                "обучение: исправная пара (2 строки)",
                () => TrainBatch(machine, "r53-valid", [BuildExample(OpenLesson), BuildExample(CloseLesson)]));
            meter.RecordBatch("исправная пара", valid, assignedRows: 2);
            Assert.True(valid.IsSuccess, valid.ErrorMessage);
            Assert.Equal(2, valid.Value!.RowsTrained);

            // Панель после исправной пары: все три ответа обязаны быть верны.
            var afterValid = meter.Measure(
                "панель после исправной пары (3 чтения)",
                () => ReadPanel(machine));
            meter.RecordPanel("панель после исправной пары", afterValid);
            AssertWholePanelAnswered(afterValid);

            // Фактические дозы плеча сверяются с назначенными планом.
            meter.AssertAssignedDoses(batchCalls: 2, assignedRows: 3, successfulLessons: 3, ordinaryReads: 9);
        }
        finally
        {
            // Показания пишутся всегда: и при зелёном, и при красном прогоне.
            meter.Write();
        }
    }

    // ── Плечо B: кандидат после правки ────────────────────────────────────────
    //
    // Назначенные дозы плеча B: 3 вызова продуктового обучения партии,
    // 5 назначенных строк, 3 настоящих успешных урока, 15 обычных чтений.
    //
    // На неизменённом коде это плечо обязано падать на значениях: негодная
    // партия там прерывает работу разборщиком и оставляет урок первой строки.
    [Fact]
    public void CandidateArm_UnreadableRowLeavesNoLesson_ThenValidPairTeachesAndSurvivesFreshLoad()
    {
        // Журнал показаний плеча B.
        var meter = new ArmMeter("B", "кандидат: ранний отказ нечитаемой партии");

        // Путь контрольной точки принадлежит показаниям этого плеча.
        var checkpointPath = Path.Combine(ReceiptDirectory(), "candidate-checkpoint.json");

        try
        {
            // Подготовка: создание переводчика и построение учебных строк.
            var machine = meter.Measure(
                "подготовка: структуры данных и зависимости",
                () => new TranslationMachine());

            // Холодная панель: исходный отказ по всем трём строкам.
            var cold = meter.Measure(
                "холодная панель (3 чтения)",
                () => ReadPanel(machine));
            meter.RecordPanel("холодная панель", cold);
            Assert.Equal(0, cold.Count(answer => answer.ExactCorrect));

            // Настроечная партия: один законный урок «keep».
            var setup = meter.Measure(
                "обучение: настроечная партия (1 строка)",
                () => TrainBatch(machine, "r53-setup", [BuildExample(SetupLesson)]));
            meter.RecordBatch("настроечная партия", setup, assignedRows: 1);
            Assert.True(setup.IsSuccess, setup.ErrorMessage);

            // Панель после настройки: переведён только «keep».
            var afterSetup = meter.Measure(
                "панель после настройки (3 чтения)",
                () => ReadPanel(machine));
            meter.RecordPanel("панель после настройки", afterSetup);
            AssertOnlySetupLessonAnswered(afterSetup);

            // Срез состояния до негодной партии: с ним сравнивается состояние
            // сразу после отказа, до сохранения, загрузки и починки.
            var beforeInvalid = Snapshot(machine);

            // Негодная партия: первая строка законна, вторая несёт тот же вход,
            // но нечитаемый текст правильного ответа.
            var invalid = meter.Measure(
                "проверка отклонённой партии (2 строки, 0 уроков)",
                () => TrainBatch(
                    machine,
                    "r53-invalid",
                    [BuildExample(OpenLesson), BuildBrokenExample(OpenLesson)]));
            meter.RecordBatch("негодная партия", invalid, assignedRows: 2);

            // Срез состояния сразу после отказа — до любой починки.
            var afterInvalid = Snapshot(machine);
            meter.RecordRejectionFootprint(beforeInvalid, afterInvalid);

            // Панель сразу после отказа: переведён по-прежнему только «keep».
            var afterInvalidPanel = meter.Measure(
                "панель после отклонённой партии (3 чтения)",
                () => ReadPanel(machine));
            meter.RecordPanel("панель после отклонённой партии", afterInvalidPanel);

            // Отказ обязан быть управляемым: конверт результата, названный код
            // и настоящий идентификатор нечитаемой строки в сообщении.
            Assert.False(
                invalid.ThrewException,
                $"нечитаемая строка обязана давать управляемый отказ, а не исключение: {invalid.ExceptionText}");
            Assert.False(invalid.IsSuccess, "партия с нечитаемой строкой не может быть успешной");
            Assert.Equal(UnreadableRowCode, invalid.ErrorCode);
            Assert.Contains(InvalidExampleId, invalid.ErrorMessage, StringComparison.Ordinal);

            // Ни одной записи: ни памяти единиц, ни версии, ни истории,
            // ни опознаний учебных строк.
            Assert.Equal(beforeInvalid.StableSha, afterInvalid.StableSha);
            Assert.Equal(beforeInvalid.Version, afterInvalid.Version);
            Assert.Equal(beforeInvalid.HistoryRows, afterInvalid.HistoryRows);
            Assert.Equal(beforeInvalid.IdentityRows, afterInvalid.IdentityRows);
            Assert.Equal(beforeInvalid.KnnEntries, afterInvalid.KnnEntries);
            Assert.Equal(beforeInvalid.ExactSourceEntries, afterInvalid.ExactSourceEntries);

            // Ответы после отказа: переведён только настроечный урок.
            AssertOnlySetupLessonAnswered(afterInvalidPanel);

            // Исправная пара на том же состоянии действительно учит открытие и
            // закрытие, и настроечный урок при этом сохраняется.
            var valid = meter.Measure(
                "обучение: исправная пара (2 строки)",
                () => TrainBatch(machine, "r53-valid", [BuildExample(OpenLesson), BuildExample(CloseLesson)]));
            meter.RecordBatch("исправная пара", valid, assignedRows: 2);
            Assert.True(valid.IsSuccess, valid.ErrorMessage);
            Assert.Equal(2, valid.Value!.RowsTrained);

            // Панель после исправной пары: три из трёх и удержание «keep».
            var afterValid = meter.Measure(
                "панель после исправной пары (3 чтения)",
                () => ReadPanel(machine));
            meter.RecordPanel("панель после исправной пары", afterValid);
            AssertWholePanelAnswered(afterValid);

            // Одно сохранение штатной контрольной точки.
            var saved = meter.Measure(
                "сохранение контрольной точки",
                () => machine.SaveCheckpointAsync(checkpointPath).GetAwaiter().GetResult());
            Assert.True(saved.IsSuccess, saved.ErrorMessage);
            meter.RecordCheckpointBytes(checkpointPath);

            // Одна свежая загрузка: новый объект переводчика читает файл с диска
            // и не выполняет ни одного обучения. Это не отдельный холодный
            // процесс операционной системы — записано честно.
            var loaded = meter.Measure(
                "свежая загрузка контрольной точки новым объектом переводчика",
                () => TranslationMachine.LoadCheckpointAsync(checkpointPath).GetAwaiter().GetResult());
            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);

            // Панель после свежей загрузки: те же три из трёх без обучения.
            var afterLoad = meter.Measure(
                "панель после свежей загрузки (3 чтения)",
                () => ReadPanel(loaded.Value!.Machine));
            meter.RecordPanel("панель после свежей загрузки", afterLoad);
            AssertWholePanelAnswered(afterLoad);

            // Фактические дозы плеча сверяются с назначенными планом.
            meter.AssertAssignedDoses(batchCalls: 3, assignedRows: 5, successfulLessons: 3, ordinaryReads: 15);
        }
        finally
        {
            // Показания пишутся всегда: и при зелёном, и при красном прогоне.
            meter.Write();
        }
    }

    // ── Чистая проба чувствительности сравнения состояний ─────────────────────
    //
    // Назначенные дозы: 0 вызовов обучения, 0 обращений к потоку перевода.
    // Меняется только КОПИЯ фактического снимка; живая модель не трогается.
    [Fact]
    public void StateComparison_DetectsOneAddedIdentity_OnCopyOfActualSnapshot()
    {
        // Живая модель с одним настоящим уроком: снимок берётся с неё.
        var machine = new TranslationMachine();
        var setup = TrainBatch(machine, "r53-sensitivity-setup", [BuildExample(SetupLesson)]);
        Assert.True(setup.IsSuccess, setup.ErrorMessage);

        // Фактический снимок состояния и его отпечаток.
        var actual = machine.ExportCheckpoint();
        Assert.True(actual.IsSuccess, actual.ErrorMessage);
        var actualSha = StableSha(actual.Value!);

        // Копия снимка с одним добавленным опознанием учебной строки.
        var mutatedIdentities = actual.Value!.StructuredTrainingIdentities
            .Append(new TranslationStructuredTrainingIdentityCheckpoint(
                "r53-sensitivity-extra",
                SourceId,
                TranslationUnitIds.ExecutorFor(Direction),
                "r53-sensitivity-setup"))
            .ToList();
        var mutatedCopy = actual.Value! with { StructuredTrainingIdentities = mutatedIdentities };

        // Сравнение обязано обнаружить отличие на одном добавленном опознании.
        Assert.NotEqual(actualSha, StableSha(mutatedCopy));

        // Живая модель не изменилась: её отпечаток остался прежним.
        var liveAgain = machine.ExportCheckpoint();
        Assert.True(liveAgain.IsSuccess, liveAgain.ErrorMessage);
        Assert.Equal(actualSha, StableSha(liveAgain.Value!));
    }

    // ── Утверждения панели ────────────────────────────────────────────────────

    // Переведён только настроечный урок; две другие строки остаются в отказе.
    private static void AssertOnlySetupLessonAnswered(IReadOnlyList<PanelAnswer> answers)
    {
        // Настроечная строка обязана быть переведена точно.
        Assert.True(
            answers.Single(answer => answer.RowId == "p1").ExactCorrect,
            $"настроечный урок потерян: «{answers.Single(answer => answer.RowId == "p1").Answer}»");

        // Две строки исправной пары обязаны оставаться в исходном отказе.
        foreach (var rowId in new[] { "p2", "p3" })
        {
            var answer = answers.Single(candidate => candidate.RowId == rowId);
            Assert.False(answer.ExactCorrect, $"строка {rowId} не должна быть выучена: «{answer.Answer}»");
            Assert.Contains("BOOTSTRAP", answer.Answer, StringComparison.Ordinal);
        }
    }

    // Переведены все три строки панели: три из трёх с удержанием настройки.
    private static void AssertWholePanelAnswered(IReadOnlyList<PanelAnswer> answers)
    {
        // Каждая строка панели проверяется отдельно, чтобы сообщение о падении
        // называло конкретный фактический ответ.
        foreach (var row in Panel)
        {
            var answer = answers.Single(candidate => candidate.RowId == row.RowId);
            Assert.True(
                answer.ExactCorrect,
                $"строка {row.RowId} «{row.Source}»: ожидалось «{row.Gold}», получено «{answer.Answer}»");
        }

        // Итоговый счёт панели назван явно.
        Assert.Equal(Panel.Length, answers.Count(answer => answer.ExactCorrect));
    }

    // ── Учебные строки и партии ───────────────────────────────────────────────

    // Построение законной учебной строки. Структуры данных повторяют опыт:
    // вход с темой general и языком источника, правильный ответ с уверенностью
    // 0,95 и пустыми остальными полями, подпись настоящего токенизатора,
    // признак отложенности — ложь.
    private static TranslationLearningExample BuildExample(LessonPair pair)
    {
        // Подпись берётся у настоящего токенизатора, а не придумывается.
        var signature = new TranslationSemanticTokenizer().Tokenize(pair.Source, SourceLang).Signature;

        // Строка собирается ровно по форме учебной записи предметной области.
        return new TranslationLearningExample(
            ExampleId: pair.ExampleId,
            UnitId: TranslationUnitIds.ExecutorFor(Direction),
            FlowId: TranslationRoutingFlow.FlowId,
            InputJson: JsonSerializer.Serialize(new ExecutorInput(pair.Source, SourceLang, Topic), UnitJson),
            ChosenOutputJson: JsonSerializer.Serialize(new ExecutorOutput(pair.Target, 0.95, null, null), UnitJson),
            RejectedOutputJson: JsonSerializer.Serialize(new ExecutorOutput("[WRONG: missing translation]", 0.1, null), UnitJson),
            RejectedType: "synthetic_missing_translation",
            SemanticSignature: signature,
            SourceId: SourceId,
            HeldOut: false);
    }

    // Копия учебной строки с другим идентификатором и нечитаемым текстом
    // правильного ответа: вход остаётся тем же, портится только ответ.
    private static TranslationLearningExample BuildBrokenExample(LessonPair pair)
        => BuildExample(pair) with { ExampleId = InvalidExampleId, ChosenOutputJson = BrokenChosenJson };

    // Вызов продуктового обучения партии. Исключение не скрывается, а
    // записывается как наблюдаемый исход: на неизменённом коде именно оно и
    // составляет измеренный след дефекта.
    private static BatchOutcome TrainBatch(
        TranslationMachine machine,
        string batchId,
        IReadOnlyList<TranslationLearningExample> examples)
    {
        // Партия собирается из переданных строк с общим источником корпуса.
        var batch = new TranslationLearningBatch(
            BatchId: batchId,
            DomainTag: TranslationMachine.DomainTag,
            Examples: examples,
            SourceIds: [SourceId]);

        try
        {
            // Продуктовый путь обучения вызывается напрямую, без копий обучаемого.
            var result = machine.TrainStructuredBatchAsync(batch).GetAwaiter().GetResult();
            return new BatchOutcome(
                IsSuccess: result.IsSuccess,
                ErrorCode: result.ErrorCode,
                ErrorMessage: result.ErrorMessage,
                Value: result.Value,
                ThrewException: false,
                ExceptionText: "");
        }
        catch (Exception ex)
        {
            // Исходное поведение: разборщик прерывает партию исключением.
            return new BatchOutcome(
                IsSuccess: false,
                ErrorCode: "",
                ErrorMessage: "",
                Value: null,
                ThrewException: true,
                ExceptionText: $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    // ── Обычные чтения через продуктовый поток ────────────────────────────────

    // Чтение всей панели через обычного потребителя: оркестратор переводчика.
    private static IReadOnlyList<PanelAnswer> ReadPanel(TranslationMachine machine)
    {
        // Оркестратор берётся продуктовым способом, а не собирается в обход.
        var orchestratorResult = machine.GetOrchestratorAsync().GetAwaiter().GetResult();
        Assert.True(orchestratorResult.IsSuccess, orchestratorResult.ErrorMessage);
        var orchestrator = orchestratorResult.Value!;

        // Ответы собираются по порядку строк панели.
        var answers = new List<PanelAnswer>(Panel.Length);
        foreach (var row in Panel)
        {
            // Запрос строится ровно так, как его строит продуктовый вызов.
            var request = new LogicalOrchestratorRequest(
                RunId: $"r53-{row.RowId}-{Guid.NewGuid():N}",
                ScenarioId: TranslationRoutingFlow.ScenarioId,
                UserRequest: row.Source,
                Flags: new LogicalExecutionFlags(),
                Metadata: new Dictionary<string, object>
                {
                    ["source_language"] = SourceLang,
                    ["target_language"] = TargetLang,
                    ["topic_domain"] = Topic
                });

            var run = orchestrator.RunAsync(request).GetAwaiter().GetResult();
            Assert.True(run.IsSuccess, run.ErrorMessage);
            var value = run.Value!;

            // Служебные показания ответа читаются из метрик потока.
            var metrics = value.FlowResult?.Metrics ?? new Dictionary<string, string>(StringComparer.Ordinal);
            var adapterId = metrics.TryGetValue("adapter_id", out var adapter) ? adapter : "не сообщено";
            var confidence = metrics.TryGetValue("confidence", out var raw) &&
                double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                    ? parsed
                    : 0.0;

            // Эталон в ответ не подставляется: сравнивается фактический текст.
            var answerText = value.FinalHumanResult ?? string.Empty;
            answers.Add(new PanelAnswer(
                RowId: row.RowId,
                Source: row.Source,
                Gold: row.Gold,
                Answer: answerText,
                AdapterId: adapterId,
                Verdict: value.OverallVerdict,
                Confidence: confidence,
                ExactCorrect: string.Equals(answerText.Trim(), row.Gold, StringComparison.OrdinalIgnoreCase)));
        }

        return answers;
    }

    // ── Срез и сравнение состояний ────────────────────────────────────────────

    // Срез принятого обученного содержимого живой модели.
    private static StateSnapshot Snapshot(TranslationMachine machine)
    {
        // Снимок берётся продуктовым выгрузчиком состояния.
        var exported = machine.ExportCheckpoint();
        Assert.True(exported.IsSuccess, exported.ErrorMessage);
        var checkpoint = exported.Value!;

        // Исполнитель нужного направления разбирается отдельно: именно его
        // память правит обучение этой приёмки.
        var executor = checkpoint.Executors.FirstOrDefault(entry => entry.Direction == Direction);

        return new StateSnapshot(
            StableSha: StableSha(checkpoint),
            Version: checkpoint.Version,
            HistoryRows: checkpoint.TrainingHistory.Count,
            IdentityRows: checkpoint.StructuredTrainingIdentities?.Count ?? 0,
            KnnEntries: checkpoint.KnnStore.Entries.Count,
            ExactSourceEntries: executor?.ExactSourceMap.Count ?? 0,
            WordMapEntries: executor?.WordMap.Count ?? 0,
            PhraseMapEntries: executor?.PhraseMap.Count ?? 0);
    }

    // Устойчивый отпечаток принятого обученного содержимого.
    //
    // Исключённые поля названы явно: отметка времени выгрузки ExportedAtUtc и
    // временные поля истории обучения CpuMs, PeakRamMb, ElapsedMs. Без этого
    // отпечаток менялся бы без единой новой записи в память. Файловое
    // равенство всей контрольной точки этим сравнением не подменяется.
    private static string StableSha(TranslationMachineCheckpoint checkpoint)
    {
        // Проекция состояния: только принятое обученное содержимое.
        var projection = new
        {
            checkpoint.Version,
            checkpoint.AccuracyAfterLastBatch,
            checkpoint.HasStructuredTraining,
            checkpoint.KnnStore,
            checkpoint.SemanticTokenizer,
            checkpoint.Executors,
            checkpoint.QualityVerifier,
            История = checkpoint.TrainingHistory
                .Select(entry => new
                {
                    entry.BatchId,
                    entry.RowsTrained,
                    entry.AccuracyBefore,
                    entry.AccuracyAfter,
                    entry.RewardMargin,
                    entry.PromotedUnitIds
                })
                .ToArray(),
            Опознания = checkpoint.StructuredTrainingIdentities
        };

        // Отпечаток берётся от устойчивого текста проекции.
        var text = JsonSerializer.Serialize(projection, StableJson);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
    }

    // ── Путь показаний ────────────────────────────────────────────────────────

    // Каталог показаний этой попытки внутри каталога запускателей.
    private static string ReceiptDirectory() => AllowedReceiptDirectory.Value;

    // Корень хранилища: подъём от каталога сборки проверок до каталога с запускателями опытов.
    private static string? RepositoryRootOrNull()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "experiments-v2", "_runners")))
                return directory.FullName;

            directory = directory.Parent;
        }

        return null;
    }

    // Куда этому прогону разрешено писать показания. Решение принимает общий приёмник
    // доказательств ОДИН раз за прогон, по одному факту: отслеживает ли хранилище файлы
    // внутри назначенного каталога показаний.
    //
    // Почему это нужно здесь. Назначенный каталог показаний этой попытки уже опубликован:
    // его файлы отслеживаются хранилищем. Значит каждый следующий прогон перезаписывал
    // опубликованные показания. Пока каталог не опубликован, прогон пишет в него и остаётся
    // в хранилище — это запись опыта. После публикации тот же прогон становится проверочным
    // и заметно уходит в игнорируемый каталог результатов испытаний.
    private static readonly Lazy<string> AllowedReceiptDirectory = new(() =>
    {
        var root = RepositoryRootOrNull();

        // Корень не найден: показания пишутся рядом со сборкой, и это видно по пути.
        if (root is null)
        {
            var fallback = Path.Combine(AppContext.BaseDirectory, $"{FixId}-{AttemptId}", "receipts");
            Directory.CreateDirectory(fallback);
            return fallback;
        }

        // Назначенный карточкой каталог показаний этой попытки.
        var declared = Path.Combine(root, "experiments-v2", "_runners", $"{FixId}-{AttemptId}", "receipts");

        // Разрешённый каталог выдаёт приёмник, он же сообщает род прогона и точный путь.
        return ExperimentFixes.ExperimentEvidenceSink
            .ForDeclaredDirectory(root, declared)
            .EnsureWriteRootDirectory();
    });

    // ── Измеритель плеча ──────────────────────────────────────────────────────

    // ArmMeter — измеритель одного плеча. Каждая стадия измеряется отдельной
    // строкой; неизвестное значение остаётся пустым и не превращается в ноль.
    private sealed class ArmMeter
    {
        private readonly string _armId;
        private readonly string _description;
        private readonly List<PhaseMeasurement> _phases = [];
        private readonly List<object> _panels = [];
        private readonly List<object> _batches = [];
        private readonly Process _process = Process.GetCurrentProcess();
        private object? _rejectionFootprint;
        private long? _checkpointBytes;
        private int _batchCalls;
        private int _assignedRows;
        private int _successfulLessons;
        private int _ordinaryReads;

        // Плечо создаётся с человеческим описанием своего назначения.
        public ArmMeter(string armId, string description)
        {
            _armId = armId;
            _description = description;
        }

        // Measure выполняет одну стадию и записывает её показания отдельной строкой.
        public T Measure<T>(string phaseName, Func<T> action)
        {
            // Начальные показания окна измерения.
            var allocatedStart = GC.GetAllocatedBytesForCurrentThread();
            var processAllocatedStart = GC.GetTotalAllocatedBytes(precise: false);
            var cpuStart = ReadProcessorTime();
            var watch = Stopwatch.StartNew();

            // Сама стадия выполняется синхронно в том же потоке, иначе
            // потоковые выделения нельзя было бы приписать этой стадии.
            var value = action();

            // Конечные показания окна измерения.
            watch.Stop();
            var cpuEnd = ReadProcessorTime();

            _phases.Add(new PhaseMeasurement(
                Стадия: phaseName,
                ВремяПоСтеннымЧасамМс: Math.Round(watch.Elapsed.TotalMilliseconds, 3),
                ПроцессорноеВремяПроцессаМс: cpuStart is null || cpuEnd is null
                    ? null
                    : Math.Round((cpuEnd.Value - cpuStart.Value).TotalMilliseconds, 3),
                ВыделеноПотокомБайт: GC.GetAllocatedBytesForCurrentThread() - allocatedStart,
                ВыделеноПроцессомБайт: GC.GetTotalAllocatedBytes(precise: false) - processAllocatedStart));

            return value;
        }

        // RecordPanel записывает фактические ответы панели одной стадии.
        public void RecordPanel(string phaseName, IReadOnlyList<PanelAnswer> answers)
        {
            // Каждое чтение панели учитывается в фактическом бюджете чтений.
            _ordinaryReads += answers.Count;
            _panels.Add(new
            {
                Стадия = phaseName,
                ВерныхОтветов = answers.Count(answer => answer.ExactCorrect),
                ВсегоЧтений = answers.Count,
                Ответы = answers
            });
        }

        // RecordBatch записывает фактический исход одного вызова обучения партии.
        public void RecordBatch(string phaseName, BatchOutcome outcome, int assignedRows)
        {
            // Фактические дозы считаются по наблюдаемому исходу, а не по плану.
            _batchCalls += 1;
            _assignedRows += assignedRows;
            _successfulLessons += outcome.Value?.RowsTrained ?? 0;
            _batches.Add(new
            {
                Стадия = phaseName,
                НазначеноСтрок = assignedRows,
                Успех = outcome.IsSuccess,
                КодОтказа = outcome.ErrorCode,
                СообщениеОтказа = outcome.ErrorMessage,
                ВыброшеноИсключение = outcome.ThrewException,
                ТекстИсключения = outcome.ExceptionText,
                ОбученоСтрокПоОтчёту = outcome.Value?.RowsTrained
            });
        }

        // RecordRejectionFootprint записывает след отклонённой партии в памяти.
        public void RecordRejectionFootprint(StateSnapshot before, StateSnapshot after)
        {
            // Сравнение показывает, что именно отказ оставил в живом состоянии.
            _rejectionFootprint = new
            {
                ОтпечатокСовпал = string.Equals(before.StableSha, after.StableSha, StringComparison.Ordinal),
                ВерсияДо = before.Version,
                ВерсияПосле = after.Version,
                СтрокИсторииДо = before.HistoryRows,
                СтрокИсторииПосле = after.HistoryRows,
                ОпознанийДо = before.IdentityRows,
                ОпознанийПосле = after.IdentityRows,
                ПарПамятиДо = before.KnnEntries,
                ПарПамятиПосле = after.KnnEntries,
                ЗаписейТочногоИсточникаДо = before.ExactSourceEntries,
                ЗаписейТочногоИсточникаПосле = after.ExactSourceEntries,
                ЗаписейСловаряДо = before.WordMapEntries,
                ЗаписейСловаряПосле = after.WordMapEntries
            };
        }

        // RecordCheckpointBytes записывает фактический размер контрольной точки.
        public void RecordCheckpointBytes(string checkpointPath)
        {
            // Размер читается с диска: это наблюдаемое значение, а не оценка.
            _checkpointBytes = File.Exists(checkpointPath)
                ? new FileInfo(checkpointPath).Length
                : null;
        }

        // AssertAssignedDoses сверяет фактические дозы плеча с назначенными.
        public void AssertAssignedDoses(int batchCalls, int assignedRows, int successfulLessons, int ordinaryReads)
        {
            // Расхождение дозы — это нарушение бюджета, а не мелочь оформления.
            Assert.Equal(batchCalls, _batchCalls);
            Assert.Equal(assignedRows, _assignedRows);
            Assert.Equal(successfulLessons, _successfulLessons);
            Assert.Equal(ordinaryReads, _ordinaryReads);
        }

        // Write пишет файл показаний плеча. Вызывается всегда, в том числе при
        // падении утверждений, чтобы красный прогон сохранял измерение.
        public void Write()
        {
            // Пик рабочего набора относится ко всему процессу и не делится по стадиям.
            long? peakWorkingSet;
            try
            {
                _process.Refresh();
                peakWorkingSet = _process.PeakWorkingSet64;
            }
            catch (PlatformNotSupportedException)
            {
                peakWorkingSet = null;
            }

            var payload = new
            {
                Назначение = FixId,
                Попытка = AttemptId,
                Плечо = _armId,
                ЧтоЭтоЗаПлечо = _description,
                ВремяЗаписиUtc = DateTimeOffset.UtcNow,
                ЧестноеОграничениеИзмерений = new[]
                {
                    "процессорное время относится ко всему процессу, не только к стадии",
                    "пик рабочего набора относится ко всему процессу и по стадиям не делится",
                    "выделения потоком относятся к стадии, выделения процессом включают посторонние потоки",
                    "свежая загрузка выполняется новым объектом переводчика в том же процессе операционной системы",
                    "неизвестные значения записаны пустыми и не превращены в ноль"
                },
                ФактическиеДозы = new
                {
                    ВызововОбученияПартии = _batchCalls,
                    НазначенныхСтрок = _assignedRows,
                    УспешныхУроковПоОтчёту = _successfulLessons,
                    ОбычныхЧтений = _ordinaryReads
                },
                ПикРабочегоНабораБайт = peakWorkingSet,
                РазмерКонтрольнойТочкиБайт = _checkpointBytes,
                СтадииИзмерения = _phases,
                ВызовыОбучения = _batches,
                СледОтклонённойПартии = _rejectionFootprint,
                Панели = _panels
            };

            // Запись показаний: перевод строк Unix, чтобы проверка пробелов
            // перед коммитом не ругалась на переводы строк Windows.
            var path = Path.Combine(ReceiptDirectory(), $"arm-{_armId}-receipt.json");
            var json = JsonSerializer.Serialize(payload, ReceiptJson)
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace("\r", "\n", StringComparison.Ordinal);
            if (!json.EndsWith('\n')) json += "\n";
            File.WriteAllText(path, json, new UTF8Encoding(false));
        }

        // ReadProcessorTime читает процессорное время процесса; на площадке без
        // такой возможности возвращает пустое значение вместо нуля.
        private TimeSpan? ReadProcessorTime()
        {
            try
            {
                _process.Refresh();
                return _process.TotalProcessorTime;
            }
            catch (PlatformNotSupportedException)
            {
                return null;
            }
        }
    }

    // ── Записи данных ────────────────────────────────────────────────────────

    // Учебная пара: идентификатор строки, источник и правильный перевод.
    private sealed record LessonPair(string ExampleId, string Source, string Target);

    // Строка панели: идентификатор, источник, эталон и человеческое пояснение.
    private sealed record PanelRow(string RowId, string Source, string Gold, string Meaning);

    // Фактический ответ панели: текст, маршрут, вердикт, уверенность, верность.
    private sealed record PanelAnswer(
        string RowId,
        string Source,
        string Gold,
        string Answer,
        string AdapterId,
        string Verdict,
        double Confidence,
        bool ExactCorrect);

    // Наблюдаемый исход одного вызова обучения партии.
    private sealed record BatchOutcome(
        bool IsSuccess,
        string ErrorCode,
        string ErrorMessage,
        LearningBatchResult? Value,
        bool ThrewException,
        string ExceptionText);

    // Срез принятого обученного содержимого и его счётчики.
    private sealed record StateSnapshot(
        string StableSha,
        string Version,
        int HistoryRows,
        int IdentityRows,
        int KnnEntries,
        int ExactSourceEntries,
        int WordMapEntries,
        int PhraseMapEntries);

    // Показания одной стадии измерения.
    private sealed record PhaseMeasurement(
        string Стадия,
        double ВремяПоСтеннымЧасамМс,
        double? ПроцессорноеВремяПроцессаМс,
        long ВыделеноПотокомБайт,
        long ВыделеноПроцессомБайт);
}
