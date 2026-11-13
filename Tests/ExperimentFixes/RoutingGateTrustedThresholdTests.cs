// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using AgentInfrastructure.Logical;
using ProjectWikiDomain;

// Что здесь проверяется, по-человечески.
//
// Это строка очереди про обычный путь маршрутизации, доверенный порог и обе дороги загрузки.
//
// Что было неправильно, по коду. Первое: заслон отложенной проверки сравнивал три наблюдения с
// порогами, ПРИЕХАВШИМИ ВМЕСТЕ СО СНИМКОМ, а доверенные пороги файла — 0.95, 1.0 и 0.85 — в
// сравнении не участвовали вовсе. Снимок со своими нулевыми порогами открывал маршрут при любых
// наблюдениях. Второе: конечность чисел у заслона не проверялась, поэтому плюс бесконечность
// была «больше любого порога» и проходила заслон. Третье: дорога контрольной точки копировала
// сохранённый заслон целиком, сбрасывая только признак заявленного смыслового качества, и делала
// это ДО всякой проверки — а читатели ниже смотрят ровно на признак пройденности: по нему
// подписываются ссылки на контрольную точку и паспорт как проверенные оценкой и выдаётся
// точность экспорта. То есть сохранённый флаг был властью.
//
// Что уже было закрыто до этой правки, и я этого не переписываю. Дорога передаточной записи уже
// имела свой валидатор происхождения: неизмеренная точность, неположительная точность, ссылки из
// непроверенной основы и отсутствие опознания оценки отвергались и раньше. Открытыми там были
// порядок изменения состояния и значение порога: любая положительная точность объявлялась
// пройденным порогом.
//
// Что стало. Решение принимает сама проверка: наблюдения обязаны быть измеренными числами,
// сохранённые пороги обязаны совпадать с действующей политикой, сравнение идёт с доверенными
// порогами, а сохранённый признак пройденности может только ЗАПРЕТИТЬ. Обе дороги загрузки
// проверяют свидетельство заслона одним валидатором и ДО изменения живого состояния.
//
// Проверка идёт обычным путём: через обычную единицу маршрутизации с входным снимком и через
// обычные члены плановой машины. Соседняя проверка конечности из другой публикации этот пункт не
// закрывает: у неё свой вход и своё доказательство.
//
// О красном прогоне на прежней логике. Красные по сути: снимок со своими нулевыми порогами и
// снимок с бесконечной точностью — прежде оба открывали маршрут; контрольная точка с заявленной
// пройденностью и точностью одна десятая — прежде становилась живым состоянием; передаточная
// запись с точностью одна десятая — прежде давала ПРОЙДЕННЫЙ порог. Положительные контроли
// зелёные и до, и после.
//
// Прогона не было: прямые команды сборки и прогона тестов запрещены политикой разрешений проекта,
// и обходить запрет оболочкой-посредником я не стал.
public sealed class RoutingGateTrustedThresholdTests
{
    // Общая примета отказа заслона, которую читают прежние читатели предупреждений.
    private const string AggregateGateWarning = "request_routing_missing_or_failed_eval_gate";

    // ── Обычный путь маршрутизации: доверенный порог ──────────────────────────

    [Fact]
    public async Task SnapshotCarryingItsOwnZeroThresholds_DoesNotOpenTheRoute()
    {
        // Снимок приносит СВОИ нулевые пороги и признак пройденности, а наблюдения слабые.
        // Прежняя проверка сравнивала бы наблюдения именно с этими нулями и открыла бы маршрут.
        var snapshot = SnapshotWithGate(Gate(
            routeAccuracy: 0.10,
            wrongAnnotationRate: 0.10,
            averageConfidence: 0.10,
            heldOutGatePassed: true,
            minimumRouteAccuracy: 0.0,
            minimumWrongAnnotationRate: 0.0,
            minimumAverageConfidence: 0.0));

        var decision = await RouteThroughOrdinaryUnitAsync(snapshot);

        // Маршрут не открыт: запускать целевой обработчик нельзя.
        Assert.False(decision.CanStartTargetRunner);
        Assert.Equal(ProjectWikiRequestRoutingDecisionStatus.NeedsSecondPass, decision.DecisionStatus);
        // Названо точное поле: порог снимка не совпадает с действующей политикой.
        Assert.Contains("request_routing_gate_route_accuracy_threshold_untrusted", decision.Warnings);
        // Общая примета сохранена для прежних читателей.
        Assert.Contains(AggregateGateWarning, decision.Warnings);
        // Наружная уверенность остаётся конечным числом.
        Assert.True(double.IsFinite(decision.Confidence));
    }

    [Fact]
    public async Task SnapshotWithNonFiniteRouteAccuracy_DoesNotOpenTheRoute()
    {
        // Пороги доверенные, признак пройденности стоит, но наблюдение не измерено.
        // Прежняя проверка пропустила бы его: бесконечность больше любого порога.
        var snapshot = SnapshotWithGate(Gate(
            routeAccuracy: double.PositiveInfinity,
            wrongAnnotationRate: 1.0,
            averageConfidence: 0.90,
            heldOutGatePassed: true));

        var decision = await RouteThroughOrdinaryUnitAsync(snapshot);

        // Маршрут не открыт, и названо точное поле с общей приметой «не измерено».
        Assert.False(decision.CanStartTargetRunner);
        Assert.Contains(
            "request_routing_gate_route_accuracy_" + ProjectWikiMeasuredNumberRule.NotMeasuredReasonSuffix,
            decision.Warnings);
        // Уверенность наружу остаётся конечной, а не бесконечной.
        Assert.True(double.IsFinite(decision.Confidence));
    }

    [Fact]
    public async Task SnapshotRecordingItsOwnFailedGate_DoesNotOpenTheRoute()
    {
        // Наблюдения и пороги в порядке, но сам снимок записал, что заслон не пройден.
        // Сохранённый признак может только запретить — здесь он и запрещает.
        var snapshot = SnapshotWithGate(Gate(
            routeAccuracy: 0.99,
            wrongAnnotationRate: 1.0,
            averageConfidence: 0.95,
            heldOutGatePassed: false));

        var decision = await RouteThroughOrdinaryUnitAsync(snapshot);

        Assert.False(decision.CanStartTargetRunner);
        Assert.Contains("request_routing_gate_not_passed_in_snapshot", decision.Warnings);
    }

    [Fact]
    public async Task HonestSnapshotWithTrustedThresholds_IsNotRefusedByTheGate()
    {
        // Положительный контроль: доверенные пороги, признак пройденности и наблюдения не ниже
        // порогов. Заслон не имеет права отказать. Зелёный и до, и после починки.
        var snapshot = SnapshotWithGate(Gate(
            routeAccuracy: 0.99,
            wrongAnnotationRate: 1.0,
            averageConfidence: 0.95,
            heldOutGatePassed: true));

        var decision = await RouteThroughOrdinaryUnitAsync(snapshot);

        // Ни одной приметы отказа заслона: ни общей, ни по точному полю.
        Assert.DoesNotContain(AggregateGateWarning, decision.Warnings);
        Assert.DoesNotContain(
            decision.Warnings,
            warning => warning.StartsWith("request_routing_gate_", StringComparison.Ordinal));
        // Уверенность наружу конечна. Сам выбор маршрута здесь не утверждается: за ним стоит
        // ещё порог уверенности, и эта проверка про заслон, а не про него.
        Assert.True(double.IsFinite(decision.Confidence));
    }

    // ── Дорога загрузки первая: контрольная точка ─────────────────────────────

    [Fact]
    public void CheckpointClaimingAPassedGateWithLowAccuracy_IsRefusedBeforeStateChanges()
    {
        var machine = new OrchestrationPlanMachine();
        // Исходное наблюдаемое состояние запоминается до попытки загрузки.
        var before = ExportSummary(machine);

        // Контрольная точка заявляет пройденный порог при точности одна десятая.
        var checkpoint = machine.ExportCheckpoint() with
        {
            LastEvaluationGate = new OrchestrationPlanEvaluationGate(
                "eval-low",
                4,
                0.10,
                true,
                "stored gate claiming passage",
                DateTimeOffset.UtcNow,
                false)
        };

        var imported = machine.ImportCheckpoint(checkpoint);

        // Отказ с точной причиной, а не живое состояние.
        Assert.False(imported.IsSuccess);
        Assert.Equal(
            OrchestrationPlanMachine.ImportedGateContradictsEvidenceErrorCode,
            imported.ErrorCode);
        // Причина называет именно «ниже действующего порога». Сравнение идёт по словам, а не по
        // записи числа: запись зависит от культуры среды, а слова — нет.
        Assert.Contains("is below the active threshold", imported.ErrorMessage);
        // Живое состояние не менялось: прежние ссылки и прежняя точность экспорта.
        Assert.Equal(before, ExportSummary(machine));
    }

    [Fact]
    public void CheckpointClaimingAPassedGateWithNonFiniteAccuracy_IsRefusedBeforeStateChanges()
    {
        var machine = new OrchestrationPlanMachine();
        var before = ExportSummary(machine);

        // Та же заявка, но число вообще не измерено.
        var checkpoint = machine.ExportCheckpoint() with
        {
            LastEvaluationGate = new OrchestrationPlanEvaluationGate(
                "eval-nan",
                4,
                double.NaN,
                true,
                "stored gate claiming passage",
                DateTimeOffset.UtcNow,
                false)
        };

        var imported = machine.ImportCheckpoint(checkpoint);

        Assert.False(imported.IsSuccess);
        Assert.Equal(
            OrchestrationPlanMachine.ImportedGateContradictsEvidenceErrorCode,
            imported.ErrorCode);
        // Причина называет именно неизмеренность, а не «ниже порога».
        Assert.Contains("not a measured number", imported.ErrorMessage);
        Assert.Equal(before, ExportSummary(machine));
    }

    [Fact]
    public async Task CheckpointWithAnHonestPassedGate_IsAcceptedAndBecomesTheExportedAccuracy()
    {
        // Положительный контроль дороги контрольной точки: честная пройденная оценка.
        var machine = new OrchestrationPlanMachine();
        var checkpoint = machine.ExportCheckpoint() with
        {
            LastEvaluationGate = new OrchestrationPlanEvaluationGate(
                "eval-honest",
                8,
                0.80,
                true,
                "stored gate with honest passage",
                DateTimeOffset.UtcNow,
                false)
        };

        var imported = machine.ImportCheckpoint(checkpoint);

        Assert.True(imported.IsSuccess, imported.ErrorMessage);
        // Принятая оценка видна наружу обычным членом выгрузки.
        var export = await machine.ExportAsync();
        Assert.True(export.IsSuccess, export.ErrorMessage);
        Assert.Equal(0.80, export.Value!.AccuracyAtExport);
        // И ссылки подписаны опознанием оценки, а не непроверенной основой.
        Assert.Contains("eval/eval-honest", export.Value.ManifestRef);
    }

    // ── Дорога загрузки вторая: передаточная запись ───────────────────────────

    [Fact]
    public async Task ImportedExportWithLowAccuracy_DoesNotBecomeAPassedGate()
    {
        // Честная низкая точность: загрузка разрешена, но пройденным порогом она не становится.
        // Прежний код объявлял пройденным любой положительный результат.
        var machine = new OrchestrationPlanMachine();

        var imported = await machine.ImportAsync(ExportWithAccuracy(0.10, "eval-low"));

        // Загрузка прошла: честный низкий результат не запрещён.
        Assert.True(imported.IsSuccess, imported.ErrorMessage);
        var export = await machine.ExportAsync();
        Assert.True(export.IsSuccess, export.ErrorMessage);
        // Но пройденного заслона нет: точность экспорта ноль, ссылки из непроверенной основы.
        Assert.Equal(0.0, export.Value!.AccuracyAtExport);
        Assert.Contains("baseline_unverified", export.Value.ManifestRef);
    }

    [Fact]
    public async Task ImportedExportWithHonestAccuracy_BecomesAPassedGate()
    {
        // Положительный контроль дороги передаточной записи: честная точность выше порога.
        var machine = new OrchestrationPlanMachine();

        var imported = await machine.ImportAsync(ExportWithAccuracy(0.90, "eval-good"));

        Assert.True(imported.IsSuccess, imported.ErrorMessage);
        var export = await machine.ExportAsync();
        Assert.True(export.IsSuccess, export.ErrorMessage);
        // Пройденный заслон виден наружу: точность и подписанная опознанием ссылка.
        Assert.Equal(0.90, export.Value!.AccuracyAtExport);
        Assert.Contains("eval/eval-good", export.Value.ManifestRef);
    }

    // ── Оснастка ──────────────────────────────────────────────────────────────

    // Свидетельство заслона обычной формы. По умолчанию пороги доверенные — те же, что в
    // действующей политике модели маршрутизации.
    private static ProjectWikiRequestRoutingGateEvidence Gate(
        double routeAccuracy,
        double wrongAnnotationRate,
        double averageConfidence,
        bool heldOutGatePassed,
        double? minimumRouteAccuracy = null,
        double? minimumWrongAnnotationRate = null,
        double? minimumAverageConfidence = null)
        => new(
            "eval-routing",
            8,
            4,
            routeAccuracy,
            wrongAnnotationRate,
            averageConfidence,
            false,
            heldOutGatePassed,
            true,
            minimumRouteAccuracy ?? ProjectWikiRequestRoutingTrainableModel.MinimumHeldOutRouteAccuracy,
            minimumWrongAnnotationRate ?? ProjectWikiRequestRoutingTrainableModel.MinimumWrongAnnotationRate,
            minimumAverageConfidence ?? ProjectWikiRequestRoutingTrainableModel.MinimumAverageConfidence,
            ProjectWikiRequestRoutingTrainableModel.MinimumTopTwoMargin,
            ProjectWikiRequestRoutingTrainableModel.MinimumSelectionConfidence,
            []);

    // Снимок с явными смещениями и пустыми весами слов — ровно та оснастка, которую называет
    // паспорт: решение определяется смещениями, поэтому запас между двумя лучшими заведомо выше
    // порога запаса и проверка доходит до самого заслона.
    private static ProjectWikiRequestRoutingModelSnapshot SnapshotWithGate(
        ProjectWikiRequestRoutingGateEvidence gateEvidence)
    {
        // Длина вектора смещений берётся у самой модели, а не выдумывается числом.
        var baseline = ProjectWikiRequestRoutingTrainableModel.Train([]).ToSnapshot(gateEvidence);
        var biases = new double[baseline.Biases.Length];
        // Явный победитель и явный второй: запас между ними велик.
        biases[0] = 0.50;
        biases[1] = 0.10;
        return baseline with { Biases = biases };
    }

    // Обычный путь: обычная единица маршрутизации с входным снимком и обычным конвертом запроса.
    private static async Task<ProjectWikiRequestRoutingDecision> RouteThroughOrdinaryUnitAsync(
        ProjectWikiRequestRoutingModelSnapshot snapshot)
    {
        var unit = new ProjectWikiRequestRoutingUnit();
        var answer = await unit.ExecuteAsync(new LogicalUnitRequest<ProjectWikiRequestRoutingInput>(
            "run-routing-gate",
            ProjectWikiRequestRoutingUnit.UnitId,
            ProjectWikiRequestRoutingUnit.UnitRole,
            ProjectWikiRequestRoutingUnit.DomainTag,
            new ProjectWikiRequestRoutingInput("Собери документацию по проекту", snapshot),
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));
        Assert.True(answer.IsSuccess, answer.ErrorMessage);
        return answer.Value!.Output;
    }

    // Передаточная запись обычной формы с подписанной опознанием оценки ссылкой.
    private static LearningMachineExport ExportWithAccuracy(double accuracy, string evaluationId)
        => new(
            OrchestrationPlanMachine.MachineId,
            OrchestrationPlanMachine.DomainTag,
            "0.2.0",
            $"{OrchestrationPlanMachine.MachineId}/0.2.0/eval/{evaluationId}/checkpoint",
            $"{OrchestrationPlanMachine.MachineId}/0.2.0/eval/{evaluationId}/manifest.json",
            accuracy,
            DateTimeOffset.UtcNow);

    // Наблюдаемое снаружи состояние машины: версия, обе ссылки и точность экспорта. Этого
    // достаточно, чтобы увидеть, менялось ли живое состояние после отказа загрузки.
    private static string ExportSummary(OrchestrationPlanMachine machine)
    {
        var export = machine.ExportAsync().GetAwaiter().GetResult();
        Assert.True(export.IsSuccess, export.ErrorMessage);
        var value = export.Value!;
        return string.Join(
            "|",
            value.Version,
            value.CheckpointRef,
            value.ManifestRef,
            value.AccuracyAtExport.ToString("F4", System.Globalization.CultureInfo.InvariantCulture));
    }
}
