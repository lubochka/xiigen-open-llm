// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;
using DesignConversionDomain;

// Приёмочные случаи переноса по опыту сто двадцать три: путь, по которому геометрия сцены
// доходит до зрительного критика разметки.
//
// Что измерил опыт. Критик выдавал вердикт, НЕ имея геометрии на входе вовсе: с подменённым
// входом он отвечал одно и то же на всех четырёх сценах (ровно порог вырождения), а с
// фактическими относительными прямоугольниками различал их. Значит отсутствовал не разум
// критика, а мост между сценой и критиком.
//
// Чего эти случаи НЕ утверждают. Критик обученным не стал и здесь так не называется ни в
// каком виде: по измерению он честный необученный каркас, и правка добавляет только путь
// входа плюс честные предупреждения. Обученная память геометрию по-прежнему не потребляет —
// для этого нужен отдельный опыт на независимых семьях геометрии, которого ещё нет.
//
// Какие случаи красные на прежней логике, а какие — охранные:
//   красные: «геометрия видна в доказательствах», «непригодный ряд назван», «пустой список
//            назван пустым», «каркас назван каркасом при увиденной геометрии», «поток
//            доносит геометрию до критика» — все они на прежнем коде даже не выражались,
//            потому что поля геометрии не существовало;
//   охранные (зелёные и до, и после): «без геометрии состав ответа прежний» и «геометрия не
//            сдвигает оценку и решение» — они стерегут измеренные числа прошлых пакетов.
public sealed class VisualCriticSeesSceneGeometryTests
{
    // Случай 1 (воспроизведение прежнего правила как функции входа, не копия нового кода).
    //
    // Прежний состав доказательств зависел от набора ссылок проверки качества — это
    // доказывается вторым входом: другой набор даёт другой ответ. И он НЕ зависел от
    // геометрии: по геометрии прежнее поведение было постоянным, и это и есть дефект, а не
    // придуманный вход.
    [Fact]
    public void PriorRule_DependsOnQaRefsAndIsConstantWithRespectToGeometry()
    {
        var geometry = new[] { Row("header", 0.0, 0.0, 1.0, 0.1) };

        // Зависимость от первого довода: два разных входа — два разных ответа.
        var firstInput = DesignConversionRuntimeScaffoldCriticUnit.PriorEvidenceRefsWithoutGeometry(
            ["render-qa-result:a"],
            geometry);
        var secondInput = DesignConversionRuntimeScaffoldCriticUnit.PriorEvidenceRefsWithoutGeometry(
            ["render-qa-result:b"],
            geometry);
        Assert.Equal(["render-qa-result:a"], firstInput);
        Assert.Equal(["render-qa-result:b"], secondInput);
        Assert.NotEqual(firstInput, secondInput);

        // Постоянство по геометрии: что ни передай — ответ один и тот же.
        var withGeometry = DesignConversionRuntimeScaffoldCriticUnit.PriorEvidenceRefsWithoutGeometry(
            ["render-qa-result:a"],
            geometry);
        var withoutGeometry = DesignConversionRuntimeScaffoldCriticUnit.PriorEvidenceRefsWithoutGeometry(
            ["render-qa-result:a"],
            null);
        var withOtherGeometry = DesignConversionRuntimeScaffoldCriticUnit.PriorEvidenceRefsWithoutGeometry(
            ["render-qa-result:a"],
            [Row("footer", 0.0, 0.9, 1.0, 0.1)]);
        Assert.Equal(withGeometry, withoutGeometry);
        Assert.Equal(withGeometry, withOtherGeometry);
        Assert.DoesNotContain(withGeometry, reference => reference.StartsWith("scene-geometry", StringComparison.Ordinal));
    }

    // Случай 2 (красный на прежней логике). Переданная геометрия ВИДНА в доказательствах
    // вердикта и в доказательствах единицы, а прежнее правило на том же входе её не вернуло бы.
    [Fact]
    public async Task SuppliedGeometry_IsVisibleInVerdictEvidenceWherePriorRuleReturnedNone()
    {
        var geometry = new[]
        {
            Row("header", 0.0, 0.0, 1.0, 0.125),
            Row("card", 0.0625, 0.25, 0.5, 0.3333333)
        };

        var result = await RunCriticAsync(geometry);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var verdictRefs = result.Value!.Output.Verdict.EvidenceRefs;
        Assert.Contains("scene-geometry:header=0,0,1,0.125", verdictRefs);
        Assert.Contains("scene-geometry:card=0.0625,0.25,0.5,0.333333", verdictRefs);
        Assert.Contains(
            DesignConversionRuntimeScaffoldCriticUnit.SceneGeometrySuppliedEvidence,
            verdictRefs);
        Assert.Contains(
            $"{DesignConversionRuntimeScaffoldCriticUnit.SceneGeometryRowCountEvidencePrefix}2/2",
            verdictRefs);
        Assert.Contains(
            DesignConversionRuntimeScaffoldCriticUnit.SceneGeometrySuppliedEvidence,
            result.Value.EvidenceIds);

        // Та же разница, видимая на ходу: прежнее правило на этом же входе геометрию не даёт.
        var priorRefs = DesignConversionRuntimeScaffoldCriticUnit.PriorEvidenceRefsWithoutGeometry(
            result.Value.Output.QaEvidenceRefs,
            geometry);
        Assert.DoesNotContain(priorRefs, reference => reference.StartsWith("scene-geometry", StringComparison.Ordinal));
        Assert.Contains(verdictRefs, reference => reference.StartsWith("scene-geometry:", StringComparison.Ordinal));
    }

    // Случай 3 (охранный). Без геометрии состав ответа в точности прежний: ни одной ссылки
    // геометрии, предупреждения — ровно те две, что были. Так исторические пакеты и
    // измеренные числа опыта остаются нетронутыми.
    [Fact]
    public async Task AbsentGeometry_LeavesEvidenceAndWarningsExactlyAsBefore()
    {
        var result = await RunCriticAsync(null);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(
            [
                "RUNTIME_SCAFFOLD_NOT_LEARNED_CRITIC",
                DesignConversionRuntimeScaffoldCriticUnit.InputEvidenceOnlyWarning
            ],
            result.Value!.Warnings);
        Assert.DoesNotContain(
            result.Value.EvidenceIds,
            reference => reference.StartsWith("scene-geometry", StringComparison.Ordinal) ||
                         reference.Contains("SCENE_GEOMETRY", StringComparison.Ordinal));
        Assert.DoesNotContain(
            result.Value.Output.Verdict.EvidenceRefs,
            reference => reference.StartsWith("scene-geometry", StringComparison.Ordinal) ||
                         reference.Contains("SCENE_GEOMETRY", StringComparison.Ordinal));
    }

    // Случай 4 (красный на прежней логике). Непригодный ряд НАЗВАН с причиной и местом, а не
    // осуждён молча: пустое опознание, не-число и сторона без площади называются отдельно, и
    // пригодные ряды при этом остаются видны.
    [Fact]
    public async Task UnusableGeometryRows_AreNamedWithReasonInsteadOfSilentJudgement()
    {
        var geometry = new[]
        {
            Row("good", 0.1, 0.1, 0.2, 0.2),
            Row("   ", 0.0, 0.0, 0.5, 0.5),
            Row("broken-number", double.NaN, 0.0, 0.5, 0.5),
            Row("flat", 0.0, 0.0, 0.5, 0.0)
        };

        var result = await RunCriticAsync(geometry);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var verdictRefs = result.Value!.Output.Verdict.EvidenceRefs;
        Assert.Contains("scene-geometry:good=0.1,0.1,0.2,0.2", verdictRefs);
        Assert.Contains("scene-geometry-unusable:1:empty-element-id", verdictRefs);
        Assert.Contains("scene-geometry-unusable:2:non-finite-number", verdictRefs);
        Assert.Contains("scene-geometry-unusable:3:non-positive-size", verdictRefs);
        Assert.Contains(
            $"{DesignConversionRuntimeScaffoldCriticUnit.SceneGeometryRowCountEvidencePrefix}1/4",
            verdictRefs);
        Assert.Contains(
            DesignConversionRuntimeScaffoldCriticUnit.SceneGeometryUnusableRowWarning,
            result.Value.Warnings);
        Assert.DoesNotContain(
            DesignConversionRuntimeScaffoldCriticUnit.SceneGeometryNoUsableRowWarning,
            result.Value.Warnings);
    }

    // Случай 5 (красный на прежней логике). Геометрию видно — и ровно поэтому сказано прямо,
    // что критик остался необученным каркасом: предупреждение каркаса никуда не делось,
    // добавилось честное «видит, но не обучен», и притязание на обучение по-прежнему false.
    [Fact]
    public async Task SeenGeometry_DoesNotTurnTheScaffoldIntoATrainedCritic()
    {
        var result = await RunCriticAsync([Row("header", 0.0, 0.0, 1.0, 0.125)]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(
            [
                "RUNTIME_SCAFFOLD_NOT_LEARNED_CRITIC",
                DesignConversionRuntimeScaffoldCriticUnit.InputEvidenceOnlyWarning,
                DesignConversionRuntimeScaffoldCriticUnit.SceneGeometrySeenButStillScaffoldWarning
            ],
            result.Value!.Warnings);
        Assert.False(result.Value.Output.DeterministicLogicClaimedAsLearning);
        Assert.Contains("DESIGN_CONVERSION_CRITIC_RUNTIME_SCAFFOLD", result.Value.EvidenceIds);
        Assert.Contains("DETERMINISTIC_LOGIC_CLAIMED_AS_LEARNING_FALSE", result.Value.EvidenceIds);
        Assert.Equal(
            DesignConversionRuntimeScaffoldCriticUnit.Role,
            result.Value.UnitRole);
    }

    // Случай 6 (охранный). Геометрия НЕ сдвигает ни оценку, ни решение «прошло или не
    // прошло», ни перечень дефектов: их по-прежнему даёт только свод итогов проверки
    // качества. Это и защищает измеренные числа от задней правки.
    [Fact]
    public async Task Geometry_DoesNotMoveScoreOrPassedOrDefects()
    {
        var withoutGeometry = await RunCriticAsync(null);
        var withGeometry = await RunCriticAsync(
        [
            Row("header", 0.0, 0.0, 1.0, 0.125),
            Row("broken", double.PositiveInfinity, 0.0, 0.5, 0.5)
        ]);

        Assert.True(withoutGeometry.IsSuccess, withoutGeometry.ErrorMessage);
        Assert.True(withGeometry.IsSuccess, withGeometry.ErrorMessage);
        Assert.Equal(withoutGeometry.Value!.Output.Verdict.Score, withGeometry.Value!.Output.Verdict.Score);
        Assert.Equal(withoutGeometry.Value.Output.Verdict.Passed, withGeometry.Value.Output.Verdict.Passed);
        Assert.Equal(withoutGeometry.Value.Confidence, withGeometry.Value.Confidence);
        Assert.Equal(
            withoutGeometry.Value.Output.Verdict.Defects.Select(defect => defect.DefectId),
            withGeometry.Value.Output.Verdict.Defects.Select(defect => defect.DefectId));
    }

    // Случай 7 (красный на прежней логике). Пустой переданный список НЕ выдаётся за увиденную
    // геометрию: он назван пустым отдельной отметкой и отдельным предупреждением, и отметки
    // «геометрия дошла» при этом нет.
    [Fact]
    public async Task EmptyGeometryList_IsNamedEmptyAndNotCountedAsSeenGeometry()
    {
        var result = await RunCriticAsync([]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Contains(
            DesignConversionRuntimeScaffoldCriticUnit.SceneGeometrySuppliedWithoutRowsEvidence,
            result.Value!.Output.Verdict.EvidenceRefs);
        Assert.DoesNotContain(
            DesignConversionRuntimeScaffoldCriticUnit.SceneGeometrySuppliedEvidence,
            result.Value.Output.Verdict.EvidenceRefs);
        Assert.Contains(
            DesignConversionRuntimeScaffoldCriticUnit.SceneGeometrySuppliedWithoutRowsWarning,
            result.Value.Warnings);
        Assert.DoesNotContain(
            DesignConversionRuntimeScaffoldCriticUnit.SceneGeometrySeenButStillScaffoldWarning,
            result.Value.Warnings);
    }

    // Случай 8 (красный на прежней логике). Путь целиком: геометрия, переданная во вход
    // потока, доходит до критика через настоящий производственный путь, а не только через
    // прямой вызов единицы в проверке.
    [Fact]
    public async Task RoutingFlow_CarriesSceneGeometryThroughToTheCritic()
    {
        var flow = new DesignConversionRoutingFlow();
        var flowInput = new DesignConversionRuntimeFlowInput(
            Request: CreateRequest(),
            QaResults: [CreateQaResult()],
            ComparisonArtifacts: [],
            AiTeacherEvidenceRefs: [],
            DeterministicEvidenceRefs: new Dictionary<string, string> { ["fixture"] = "scene-geometry" },
            SceneGeometry: [Row("hero", 0.25, 0.5, 0.5, 0.25)]);

        var result = await flow.RunAsync(
            flowInput,
            runId: "run-scene-geometry",
            flags: new LogicalExecutionFlags(),
            metadata: new Dictionary<string, object>());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Contains(
            "scene-geometry:hero=0.25,0.5,0.5,0.25",
            result.Value!.CriticOutput.Verdict.EvidenceRefs);
        Assert.Contains(
            DesignConversionRuntimeScaffoldCriticUnit.SceneGeometrySuppliedEvidence,
            result.Value.CriticOutput.Verdict.EvidenceRefs);
        Assert.False(result.Value.CriticOutput.DeterministicLogicClaimedAsLearning);
    }

    private static DesignConversionRelativeGeometry Row(
        string elementId,
        double left,
        double top,
        double width,
        double height)
        => new(elementId, left, top, width, height);

    private static async Task<AgentInfrastructure.OperationResult<LogicalUnitResult<DesignConversionCriticUnitOutput>>> RunCriticAsync(
        IReadOnlyList<DesignConversionRelativeGeometry>? sceneGeometry)
    {
        var unit = new DesignConversionRuntimeScaffoldCriticUnit();
        var input = new DesignConversionCriticUnitInput(
            Request: CreateRequest(),
            QaResults: [CreateQaResult()],
            ComparisonArtifacts: [],
            AiTeacherEvidenceRefs: [],
            DeterministicEvidenceRefs: new Dictionary<string, string> { ["fixture"] = "scene-geometry" },
            SceneGeometry: sceneGeometry);

        return await unit.ExecuteAsync(new LogicalUnitRequest<DesignConversionCriticUnitInput>(
            RunId: "run-scene-geometry",
            UnitId: DesignConversionRuntimeScaffoldCriticUnit.UnitId,
            UnitRole: DesignConversionRuntimeScaffoldCriticUnit.Role,
            DomainTag: DesignConversionRuntimeScaffoldCriticUnit.DomainTag,
            Input: input,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>()));
    }

    private static RenderQaResultRecord CreateQaResult()
    {
        var expected = CreateImage("expected", 1440, 900);
        var actual = CreateImage("actual", 1440, 900);
        return new RenderQaResultRecord(
            ResultId: "qa-result-geometry-001",
            RequestId: "design-request-geometry-001",
            Expected: expected,
            Actual: actual,
            Verdict: new VisualVerdict(
                VerdictId: "qa-verdict-geometry",
                Score: 0.73,
                Defects:
                [
                    new VisualDefect(
                        DefectId: "qa-verdict-geometry:spacing",
                        Category: "spacing",
                        Severity: "major",
                        Description: "Spacing evidence supplied by render QA input.",
                        X: 24,
                        Y: 40,
                        Width: 120,
                        Height: 32,
                        EvidenceImageIds: ["expected", "actual"])
                ],
                EvidenceRefs: ["verdict-evidence:qa-verdict-geometry"],
                Passed: false),
            Metrics: new Dictionary<string, double> { ["visual_distance"] = 0.21 },
            Warnings: []);
    }

    private static RenderQaImageRecord CreateImage(string id, int width, int height)
        => new(
            ImageId: id,
            Label: id,
            Resolution: new RenderQaResolution($"res-{width}x{height}", width, height, 1.0),
            Uri: $"artifact://{id}.png",
            Sha256: $"sha256:{id}",
            Metadata: new Dictionary<string, string>());

    private static SubmittedDesignConversionRequest CreateRequest()
        => new(
            RequestId: "design-request-geometry-001",
            SourceKind: DesignConversionSourceKind.Figma,
            Html: "<main class=\"card\">Hello</main>",
            Css: ".card{display:grid;}",
            DesignTreePayload: new DesignTreePayload(
                PayloadId: "tree-current",
                Format: "figma-json",
                Content: "{\"type\":\"FRAME\"}",
                Sha256: "sha256:tree-current",
                Metadata: new Dictionary<string, string>()),
            OriginalTreePayload: new DesignTreePayload(
                PayloadId: "tree-original",
                Format: "original-json",
                Content: "{\"type\":\"FRAME\"}",
                Sha256: "sha256:tree-original",
                Metadata: new Dictionary<string, string>()),
            Screenshots: [new DesignRenderArtifact("shot-001", "screenshot", 1440, 900, "artifact://shot.png", "sha256:shot", new Dictionary<string, string>())],
            DesignRenders: [new DesignRenderArtifact("render-001", "design-render", 1440, 900, "artifact://render.png", "sha256:render", new Dictionary<string, string>())],
            Budget: new DesignConversionBudgetMetadata(
                UserId: "user-redacted",
                Tier: "pro",
                MaxImprovementCandidates: 3,
                MaxRenderQaRuns: 4,
                BudgetUsd: 1.50m,
                Metadata: new Dictionary<string, string>()),
            Provenance: new Dictionary<string, string> { ["source"] = "scene-geometry-acceptance" });
}
