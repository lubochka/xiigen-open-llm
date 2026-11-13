// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using ProjectWikiDomain;
using Xunit.Abstractions;

/// <summary>
/// FIX-RET. Перенос измеренного опыта «удержание прежнего парафраза» (RETENTION-M1-01)
/// в продукт: словарь входных признаков одного маршрута не должен затираться следующим
/// совместимым уроком того же маршрута.
///
/// Уроки и экзамен — дословные строки авторского опыта
/// (experiments-v2/RP01-MICRO/N61-MODE1/Program.cs, BuildLessons/BuildExam/Pair).
/// Писатель — настоящий продуктовый <see cref="OrchestrationPlanMachine.TrainOrchestrationBatchAsync"/>,
/// читатель — настоящий продуктовый <see cref="OrchestrationPlanMachine.GeneratePlanAsync"/>.
/// Полный ответ сверяется по режиму маршрута, ссылке маршрута в привязке, верхней
/// ссылке маршрута в тексте плана и телу плана после нормализации пробелов — одного
/// идентификатора недостаточно.
/// </summary>
public sealed class ExperimentBasedPromptFlowRetentionTests
{
    private readonly ITestOutputHelper _output;

    public ExperimentBasedPromptFlowRetentionTests(ITestOutputHelper output) => _output = output;

    // ── дословные данные карточки: три урока и четыре задания экзамена ────────────────

    private const string LabABody = "Validate approval, then persist the ledger.";
    private const string LabARejected = "Persist the ledger before validating approval.";
    private const string LabBBody = "Validate receipt, then email the receipt.";
    private const string LabBRejected = "Email the receipt before validating it.";

    private static OrchestrationTrainingPair LessonB()
        => Pair("00-b", "LAB-B", "validate payment receipt email", LabBBody, LabBRejected);

    private static OrchestrationTrainingPair LessonA1()
        => Pair("10-a1", "LAB-A", "validate payment approval ledger", LabABody, LabARejected);

    private static OrchestrationTrainingPair LessonA2()
        => Pair("20-a2", "LAB-A", "check invoice consent journal", LabABody, LabARejected);

    // Несовместимый урок: тот же маршрут и тот же запрос, что у совместимого парафраза,
    // но тело плана действительно другое. Повтор одинакового тела не обнаружил бы
    // ошибку «никогда не заменять», поэтому тело здесь отличается по существу.
    private static OrchestrationTrainingPair LessonA2WithChangedPlan()
        => Pair(
            "20-a2-changed",
            "LAB-A",
            "check invoice consent journal",
            "Persist the journal first, then collect the consent.",
            "Collect the consent only after the journal is closed.");

    // Урок того же маршрута и того же тела плана, но с собственным заголовком цели:
    // нужен, чтобы проверить, что слова заголовка не попадают во входные признаки
    // ни после одного урока, ни после накопления нескольких.
    private static OrchestrationTrainingPair PairWithTitle(
        string pairId,
        string flowRef,
        string prompt,
        string title,
        string chosenPlan,
        string rejected)
        => Pair(pairId, flowRef, prompt, chosenPlan, rejected) with
        {
            Chosen = new OrchestrationPlanChosen(
                FlowRef: flowRef,
                Title: title,
                SourcePath: $"simulation:RETENTION/{pairId}",
                PlanText: chosenPlan)
        };

    private static OrchestrationTrainingPair Pair(
        string pairId,
        string flowRef,
        string prompt,
        string chosenPlan,
        string rejected)
        => new(
            PairId: pairId,
            Mode: PairMode.Mode1LearnedFlowGrounding,
            Prompt: prompt,
            Chosen: new OrchestrationPlanChosen(
                FlowRef: flowRef,
                Title: flowRef == "LAB-A" ? "Approval ledger plan" : "Receipt plan",
                SourcePath: $"simulation:RETENTION/{pairId}",
                PlanText: chosenPlan),
            RejectedText: rejected,
            ExpectedRouteMode: GenerationRouteMode.LearnedFlowGrounding,
            ExpectedBoundarySource: BoundarySource.Classifier,
            ExpectedMlGenerated: true,
            ExpectedIntentKind: IntentKind.ImplementationExecutionRequest,
            FlowRef: flowRef,
            SourceBundleId: $"simulation:RETENTION/{pairId}",
            IsHeldOut: false);

    private sealed record ExamTask(string Id, string Query, string? GoldFlow, string? GoldBody, string Meaning);

    private static ExamTask[] Exam() =>
    [
        new("RQ1", "validate payment approval ledger audit", "LAB-A", LabABody, "удержание старого навыка"),
        new("RQ2", "check invoice consent journal review", "LAB-A", LabABody, "приобретение нового парафраза"),
        new("RQ3", "validate payment receipt email summary", "LAB-B", LabBBody, "сохранение конкурирующего маршрута"),
        new("RQ4", "schedule backups policy cron", null, null, "отрицательный запрос: обобщение без выученного блока")
    ];

    // ── чтение полного ответа тем же правилом, что в опыте ───────────────────────────

    private sealed record Readout(
        string TaskId,
        bool Correct,
        string RouteMode,
        string? GroundedFlowRef,
        string? UpperFlowRef,
        string? Body,
        string Reason);

    private static string Normalize(string text)
        => string.Join(" ", text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static async Task<Dictionary<string, Readout>> ReadAllAsync(OrchestrationPlanMachine machine)
    {
        var readouts = new Dictionary<string, Readout>(StringComparer.Ordinal);
        foreach (var task in Exam())
        {
            var result = await machine.GeneratePlanAsync(task.Query);
            Assert.True(result.IsSuccess, $"{task.Id}: {result.ErrorCode} {result.ErrorMessage}");
            readouts[task.Id] = Grade(result.Value!, task);
        }

        return readouts;
    }

    private static Readout Grade(OrchestrationGenerationResult value, ExamTask task)
    {
        var lines = value.PlanText.Replace("\r\n", "\n").Split('\n');
        var flowRefIndex = Array.FindIndex(lines, line => line.StartsWith("FLOW_REF=", StringComparison.Ordinal));
        var upperFlowRef = flowRefIndex >= 0 ? lines[flowRefIndex]["FLOW_REF=".Length..].Trim() : null;
        var titleIndex = flowRefIndex >= 0
            ? Array.FindIndex(lines, flowRefIndex, line => line.StartsWith("TITLE=", StringComparison.Ordinal))
            : -1;
        var body = titleIndex >= 0 ? Normalize(string.Join("\n", lines.Skip(titleIndex + 1))) : null;

        if (task.GoldFlow is null)
        {
            var leaked = upperFlowRef is "LAB-A" or "LAB-B";
            var ok = value.RouteMode == GenerationRouteMode.Mode2Generalization
                     && value.FlowGrounding is null
                     && !leaked;
            return new Readout(
                task.Id,
                ok,
                value.RouteMode.ToString(),
                value.FlowGrounding?.FlowRef,
                upperFlowRef,
                body,
                ok ? "обобщение без выученного блока" : "на отрицательном запросе появился выученный блок или привязка");
        }

        var routeOk = value.RouteMode == GenerationRouteMode.LearnedFlowGrounding
                      && value.BoundarySource == BoundarySource.Classifier
                      && value.IntentKind == IntentKind.ImplementationExecutionRequest;
        var flowOk = value.FlowGrounding?.FlowRef == task.GoldFlow && upperFlowRef == task.GoldFlow;
        var bodyOk = body is not null && body == Normalize(task.GoldBody!);

        return new Readout(
            task.Id,
            routeOk && flowOk && bodyOk,
            value.RouteMode.ToString(),
            value.FlowGrounding?.FlowRef,
            upperFlowRef,
            body,
            routeOk && flowOk && bodyOk
                ? "маршрут, ссылка и всё тело выученного плана совпали"
                : $"routeOk={routeOk} flowOk={flowOk} bodyOk={bodyOk}");
    }

    private static async Task TrainAsync(OrchestrationPlanMachine machine, OrchestrationTrainingPair pair)
    {
        var trained = await machine.TrainOrchestrationBatchAsync(
            new OrchestrationTrainingBatch($"RETENTION/{pair.PairId}", [pair]));
        Assert.True(trained.IsSuccess, $"{pair.PairId}: {trained.ErrorCode} {trained.ErrorMessage}");
        Assert.Equal(1, trained.Value!.Mode1RowsTrained);
    }

    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, float>> InputFeatures(
        OrchestrationPlanMachine machine)
        => machine.ExportCheckpoint().FlowGroundingWeights
            .ToDictionary(route => route.Key, route => route.Value.InputFeatureWeights, StringComparer.Ordinal);

    private void FeaturePanel(string stage, OrchestrationPlanMachine machine)
    {
        var features = InputFeatures(machine);
        var total = features.Values.Sum(route => route.Count);
        _output.WriteLine($"ПРИЗНАКИ {stage}: всего входных признаков двух маршрутов = {total}");
        foreach (var route in features.OrderBy(route => route.Key, StringComparer.Ordinal))
            _output.WriteLine(
                $"  {route.Key}: {route.Value.Count} признаков -> " +
                string.Join(", ", route.Value.OrderBy(f => f.Key, StringComparer.Ordinal).Select(f => $"{f.Key}={f.Value}")));
    }

    private void Panel(string stage, Dictionary<string, Readout> readouts)
    {
        _output.WriteLine($"ПАНЕЛЬ {stage}");
        foreach (var task in Exam())
        {
            var r = readouts[task.Id];
            _output.WriteLine(
                $"  {r.TaskId} «{task.Query}» -> верно={r.Correct} маршрут={r.RouteMode} " +
                $"привязка={r.GroundedFlowRef ?? "нет"} верхняяСсылка={r.UpperFlowRef ?? "нет"} " +
                $"тело=«{r.Body ?? "нет"}» причина={r.Reason}");
        }
    }

    // ── решающая проверка переноса ───────────────────────────────────────────────────

    /// <summary>
    /// Прежний парафраз обязан сохранить свой ответ после единственного нового
    /// совместимого урока того же маршрута. RQ1/RQ2 читаются сразу после этого
    /// урока: старый урок не повторяется и сохранённое состояние поверх результата
    /// не загружается.
    /// </summary>
    [Fact]
    public async Task Retention_AfterCompatibleParaphrasePreservesOldAnswer()
    {
        var machine = new OrchestrationPlanMachine();
        await TrainAsync(machine, LessonB());
        await TrainAsync(machine, LessonA1());

        var before = await ReadAllAsync(machine);
        Panel("после двух общих уроков (00-b, 10-a1)", before);
        FeaturePanel("после двух общих уроков (00-b, 10-a1)", machine);
        Assert.True(before["RQ1"].Correct, $"RQ1 обязан быть верен до нового урока: {before["RQ1"]}");
        Assert.False(before["RQ2"].Correct, $"RQ2 обязан быть неверен до нового урока: {before["RQ2"]}");

        await TrainAsync(machine, LessonA2());

        var after = await ReadAllAsync(machine);
        Panel("сразу после единственного нового урока (20-a2)", after);
        FeaturePanel("сразу после единственного нового урока (20-a2)", machine);

        Assert.True(after["RQ1"].Correct, $"удержание: прежний парафраз потерял свой ответ: {after["RQ1"]}");
        Assert.True(after["RQ2"].Correct, $"приобретение: новый парафраз не выучен: {after["RQ2"]}");
        Assert.True(after["RQ3"].Correct, $"конкурирующий маршрут потерян: {after["RQ3"]}");
        Assert.True(after["RQ4"].Correct, $"отрицательный запрос испорчен: {after["RQ4"]}");
    }

    /// <summary>
    /// Накопление касается ровно одного словаря. Входные признаки совместимого
    /// парафраза объединяются по максимуму (повторяющийся токен получает максимум,
    /// а не сумму), а остальные семь полей записи весов берутся из свежего расчёта —
    /// это проверяется сравнением с моделью, обученной только новому уроку.
    /// Арифметика измеренного опыта: входных признаков двух маршрутов 8 -> 12.
    /// </summary>
    [Fact]
    public async Task Retention_MergeTouchesOnlyInputFeatureWeights()
    {
        var machine = new OrchestrationPlanMachine();
        await TrainAsync(machine, LessonB());
        await TrainAsync(machine, LessonA1());

        var beforeFeatures = InputFeatures(machine);
        FeaturePanel("два общих урока", machine);
        Assert.Equal(4, beforeFeatures["LAB-A"].Count);
        Assert.Equal(4, beforeFeatures["LAB-B"].Count);
        Assert.Equal(8, beforeFeatures.Values.Sum(route => route.Count));

        await TrainAsync(machine, LessonA2());

        var afterFeatures = InputFeatures(machine);
        FeaturePanel("после совместимого парафраза", machine);

        // Прежние ключи остались, новые добавились: 4 -> 8 у LAB-A, итого 8 -> 12.
        Assert.Equal(8, afterFeatures["LAB-A"].Count);
        Assert.Equal(4, afterFeatures["LAB-B"].Count);
        Assert.Equal(12, afterFeatures.Values.Sum(route => route.Count));
        foreach (var token in new[] { "validate", "payment", "approval", "ledger", "check", "invoice", "consent", "journal" })
            Assert.Contains(token, afterFeatures["LAB-A"].Keys, StringComparer.OrdinalIgnoreCase);

        // Общий для двух уроков токен получает максимум, а не сумму.
        Assert.Equal(1.0f, afterFeatures["LAB-A"]["validate"]);
        Assert.Equal(1.0f, afterFeatures["LAB-A"]["payment"]);

        // Остальные семь полей — из свежего расчёта по новому уроку.
        var freshOnly = new OrchestrationPlanMachine();
        await TrainAsync(freshOnly, LessonA2());
        var expected = freshOnly.ExportCheckpoint().FlowGroundingWeights["LAB-A"];
        var actual = machine.ExportCheckpoint().FlowGroundingWeights["LAB-A"];
        Assert.Equal(expected.AllowedScopeFeatures, actual.AllowedScopeFeatures);
        Assert.Equal(expected.LabelDescriptorFeatures, actual.LabelDescriptorFeatures);
        Assert.Equal(expected.EvidenceOnlyTargetFeatures, actual.EvidenceOnlyTargetFeatures);
        Assert.Equal(expected.PhaseWeights, actual.PhaseWeights);
        Assert.Equal(expected.ArtifactWeights, actual.ArtifactWeights);
        Assert.Equal(expected.GateWeights, actual.GateWeights);
        Assert.Equal(expected.StackWeights, actual.StackWeights);
    }

    /// <summary>
    /// Граница накопления против вечной утечки. Объединение начинается с ПОЛНОГО
    /// прежнего словаря, поэтому ключ, однажды попавший во входные признаки маршрута,
    /// оттуда больше не исчезает. Единственная дверь внутрь — токены текста запроса:
    /// заголовок цели уходит в описательные признаки, а маршруты цели — в целевые.
    /// Проверяется именно это: накопленный словарь равен ровно объединению токенов
    /// запросов двух уроков, и ни одного слова из заголовков в нём нет — ни после
    /// первого урока, ни после второго. Прежние сторожа утечки проверяли только
    /// последний урок, этот держит весь накопленный словарь.
    /// </summary>
    [Fact]
    public async Task Retention_MergeNeverCarriesTargetLabelTokensIntoInputFeatures()
    {
        const string FirstPrompt = "validate payment approval ledger";
        const string SecondPrompt = "check invoice consent journal";
        var forbidden = new[] { "unique", "descriptor", "alpha", "second", "marker", "beta", "approvalledger" };

        var machine = new OrchestrationPlanMachine();
        await TrainAsync(machine, PairWithTitle(
            "40-leak-a1", "LAB-A", FirstPrompt, "Unique Descriptor Alpha", LabABody, LabARejected));

        var afterFirst = machine.ExportCheckpoint().FlowGroundingWeights["LAB-A"];
        FeaturePanel("один урок с заголовком «Unique Descriptor Alpha»", machine);
        foreach (var token in forbidden)
            Assert.DoesNotContain(token, afterFirst.InputFeatureWeights.Keys, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("unique", afterFirst.LabelDescriptorFeatures.Keys, StringComparer.OrdinalIgnoreCase);

        await TrainAsync(machine, PairWithTitle(
            "41-leak-a2", "LAB-A", SecondPrompt, "Second Marker Beta", LabABody, LabARejected));

        var afterMerge = machine.ExportCheckpoint().FlowGroundingWeights["LAB-A"];
        FeaturePanel("после накопления второго урока с заголовком «Second Marker Beta»", machine);

        // Ни одно слово обоих заголовков не стало входным признаком.
        foreach (var token in forbidden)
            Assert.DoesNotContain(token, afterMerge.InputFeatureWeights.Keys, StringComparer.OrdinalIgnoreCase);

        // Накопленный словарь равен РОВНО объединению токенов двух запросов: ни одного
        // лишнего ключа сверх старых и свежих, и ни один старый не потерян.
        var expectedKeys = FirstPrompt.Split(' ').Concat(SecondPrompt.Split(' '))
            .OrderBy(token => token, StringComparer.Ordinal)
            .ToArray();
        var actualKeys = afterMerge.InputFeatureWeights.Keys
            .Select(key => key.ToLowerInvariant())
            .OrderBy(token => token, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(expectedKeys, actualKeys);

        // Описательные признаки остались свежими: это поле из объединения исключено.
        Assert.Contains("second", afterMerge.LabelDescriptorFeatures.Keys, StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain("unique", afterMerge.LabelDescriptorFeatures.Keys, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Границы накопления. Урок того же маршрута с действительно другим телом плана
    /// по-прежнему заменяет признаки, а не объединяет их: совместимость решается
    /// сравнением положительных целей, а не похожестью запросов.
    /// </summary>
    [Fact]
    public async Task Retention_ChangedPositivePlanKeepsReplacementSemantics()
    {
        var machine = new OrchestrationPlanMachine();
        await TrainAsync(machine, LessonB());
        await TrainAsync(machine, LessonA1());
        await TrainAsync(machine, LessonA2WithChangedPlan());

        var weights = machine.ExportCheckpoint().FlowGroundingWeights["LAB-A"].InputFeatureWeights;

        // Признаки прежнего урока обязаны уйти: это и есть прежняя замена.
        Assert.DoesNotContain("approval", weights.Keys, StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain("ledger", weights.Keys, StringComparer.OrdinalIgnoreCase);
        // Признаки свежего урока на месте.
        Assert.Contains("invoice", weights.Keys, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("journal", weights.Keys, StringComparer.OrdinalIgnoreCase);

        var readouts = await ReadAllAsync(machine);
        Panel("несовместимый урок: замена сохранена", readouts);

        // Новый урок отвечает своим телом, а не прежним.
        var rq2 = readouts["RQ2"];
        Assert.Equal("LAB-A", rq2.GroundedFlowRef);
        Assert.Equal(Normalize("Persist the journal first, then collect the consent."), rq2.Body);
        // Прежние пары соседей не удаляются.
        Assert.Equal(3, machine.Mode1FlowGroundingKnnSize);
    }

    /// <summary>
    /// Восстановление: объединённые признаки обязаны выжить после сохранения и
    /// свежей загрузки. Новый объект, загрузка состояния, прежние входы — без обучения.
    /// </summary>
    [Fact]
    public async Task Retention_MergedInputFeaturesSurviveFreshLoadWithoutTraining()
    {
        var machine = new OrchestrationPlanMachine();
        await TrainAsync(machine, LessonB());
        await TrainAsync(machine, LessonA1());
        await TrainAsync(machine, LessonA2());

        var root = Path.Combine(Path.GetTempPath(), $"fix-ret-restore-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "s1.checkpoint.json");
        try
        {
            var saved = await machine.SaveCheckpointAsync(path);
            Assert.True(saved.IsSuccess, $"{saved.ErrorCode} {saved.ErrorMessage}");

            var loaded = await OrchestrationPlanMachine.LoadCheckpointAsync(path);
            Assert.True(loaded.IsSuccess, $"{loaded.ErrorCode} {loaded.ErrorMessage}");
            var fresh = loaded.Value!.Machine;

            // Никакого обучения после загрузки: читаются прежние входы.
            var readouts = await ReadAllAsync(fresh);
            Panel("новый объект после загрузки состояния, без обучения", readouts);

            Assert.True(readouts["RQ1"].Correct, $"после загрузки потерян прежний парафраз: {readouts["RQ1"]}");
            Assert.True(readouts["RQ2"].Correct, $"после загрузки потерян новый парафраз: {readouts["RQ2"]}");
            Assert.True(readouts["RQ3"].Correct, $"после загрузки потерян конкурирующий маршрут: {readouts["RQ3"]}");
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>
    /// Продолжение обучения после восстановления. Допуск накопления смотрит на уроки
    /// из памяти соседей, а прежние веса берёт из словаря весов — это два разных
    /// хранилища, и слепок обязан нести оба: поля `FlowGroundingKnn` и
    /// `FlowGroundingWeights` восстанавливаются одним вызовом. Если бы уроки не
    /// восстанавливались, первый же совместимый парафраз после загрузки выполнил бы
    /// прежнюю полную замену и стёр только что восстановленный словарь. Проверяется
    /// числами: сколько ключей было после восстановления и сколько стало после урока.
    /// </summary>
    [Fact]
    public async Task Retention_ContinuationAfterRestoreKeepsAccumulating()
    {
        var machine = new OrchestrationPlanMachine();
        await TrainAsync(machine, LessonB());
        await TrainAsync(machine, LessonA1());
        await TrainAsync(machine, LessonA2());

        var root = Path.Combine(Path.GetTempPath(), $"fix-ret-continue-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "s1.checkpoint.json");
        try
        {
            var saved = await machine.SaveCheckpointAsync(path);
            Assert.True(saved.IsSuccess, $"{saved.ErrorCode} {saved.ErrorMessage}");

            var loaded = await OrchestrationPlanMachine.LoadCheckpointAsync(path);
            Assert.True(loaded.IsSuccess, $"{loaded.ErrorCode} {loaded.ErrorMessage}");
            var fresh = loaded.Value!.Machine;

            // Оба хранилища действительно восстановлены: уроки и словарь весов.
            Assert.Equal(3, fresh.Mode1FlowGroundingKnnSize);
            Assert.Equal(2, fresh.Mode1FlowGroundingWeightsSize);
            var afterRestore = InputFeatures(fresh)["LAB-A"].Count;
            FeaturePanel("после восстановления, до продолжения", fresh);
            Assert.Equal(8, afterRestore);

            // Ещё один совместимый парафраз того же маршрута, четыре новых токена.
            await TrainAsync(fresh, Pair(
                "30-a3", "LAB-A", "confirm audited reconciliation entry", LabABody, LabARejected));

            var afterContinuation = InputFeatures(fresh)["LAB-A"].Count;
            FeaturePanel("после продолжения обучения на восстановленном состоянии", fresh);

            // Накопление продолжилось, а не заменило восстановленный словарь.
            Assert.Equal(12, afterContinuation);
            Assert.True(
                afterContinuation > afterRestore,
                $"после восстановления признаки заменились вместо накопления: было {afterRestore}, стало {afterContinuation}");

            // И прежние ответы на месте: ни один из двух выученных парафразов не потерян.
            var readouts = await ReadAllAsync(fresh);
            Panel("после продолжения обучения на восстановленном состоянии", readouts);
            Assert.True(readouts["RQ1"].Correct, $"продолжение стёрло прежний парафраз: {readouts["RQ1"]}");
            Assert.True(readouts["RQ2"].Correct, $"продолжение стёрло новый парафраз: {readouts["RQ2"]}");
            Assert.True(readouts["RQ3"].Correct, $"продолжение стёрло конкурирующий маршрут: {readouts["RQ3"]}");
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    // ── опоры непустоты: проходят на обоих состояниях продукта ───────────────────────

    /// <summary>
    /// Опора непустоты. Два общих урока действительно записаны, и прежний запрос
    /// достижим через настоящего потребителя. Если эта проверка зелёная, красное
    /// в решающей проверке нельзя списать на отсутствие данных.
    /// </summary>
    [Fact]
    public async Task Retention_Anchor_TwoCommonLessonsMakeOldQueryAnswerable()
    {
        var machine = new OrchestrationPlanMachine();
        await TrainAsync(machine, LessonB());
        await TrainAsync(machine, LessonA1());

        Assert.Equal(2, machine.Mode1FlowGroundingKnnSize);
        Assert.Equal(2, machine.Mode1FlowGroundingWeightsSize);

        var readouts = await ReadAllAsync(machine);
        Panel("опора непустоты: состояние двух общих уроков", readouts);

        Assert.True(readouts["RQ1"].Correct, $"прежний урок недостижим: {readouts["RQ1"]}");
        Assert.True(readouts["RQ3"].Correct, $"конкурирующий урок недостижим: {readouts["RQ3"]}");
        Assert.Equal(LabABody, Normalize(readouts["RQ1"].Body!));
    }

    /// <summary>
    /// Опора непустоты. Третий урок действительно прошёл обучение в обоих состояниях
    /// продукта: новый парафраз выучен, конкурирующий маршрут и отрицательный запрос
    /// сохранены. Значит до правки терялся ровно прежний парафраз, а не обучение.
    /// </summary>
    [Fact]
    public async Task Retention_Anchor_NewParaphraseIsAcquiredAndCompetingRouteSurvives()
    {
        var machine = new OrchestrationPlanMachine();
        await TrainAsync(machine, LessonB());
        await TrainAsync(machine, LessonA1());
        await TrainAsync(machine, LessonA2());

        Assert.Equal(3, machine.Mode1FlowGroundingKnnSize);
        Assert.Equal(2, machine.Mode1FlowGroundingWeightsSize);

        var readouts = await ReadAllAsync(machine);
        Panel("опора непустоты: состояние трёх уроков", readouts);

        Assert.True(readouts["RQ2"].Correct, $"новый парафраз не выучен: {readouts["RQ2"]}");
        Assert.True(readouts["RQ3"].Correct, $"конкурирующий маршрут потерян: {readouts["RQ3"]}");
        Assert.True(readouts["RQ4"].Correct, $"отрицательный запрос испорчен: {readouts["RQ4"]}");
    }

    /// <summary>
    /// Панель обучения для отчёта: ответы на все четыре задания до обучения, после
    /// первого урока, после второго урока и после третьего. Утверждение — опора
    /// непустоты: без обучения выученного блока нет, после обучения он есть.
    /// Проходит на обоих состояниях продукта; содержание панели различается.
    /// </summary>
    [Fact]
    public async Task Retention_LearningPanel_ReadsFourQueriesAtEveryStage()
    {
        var machine = new OrchestrationPlanMachine();

        var untrained = await ReadAllAsync(machine);
        Panel("этап 0: до обучения", untrained);
        Assert.All(
            new[] { "RQ1", "RQ2", "RQ3" },
            id => Assert.NotEqual("LearnedFlowGrounding", untrained[id].RouteMode));

        await TrainAsync(machine, LessonB());
        Panel("этап 1: после первого урока (00-b, LAB-B)", await ReadAllAsync(machine));

        await TrainAsync(machine, LessonA1());
        Panel("этап 2: после второго урока (10-a1, LAB-A)", await ReadAllAsync(machine));

        await TrainAsync(machine, LessonA2());
        var stage3 = await ReadAllAsync(machine);
        Panel("этап 3: после третьего урока (20-a2, совместимый парафраз LAB-A)", stage3);

        Assert.Equal("LearnedFlowGrounding", stage3["RQ2"].RouteMode);
    }
}
