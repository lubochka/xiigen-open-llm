// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DesignConversionDomain;

public sealed class DesignConversionRuntimeImproverNonCopyTests
{
    [Fact]
    public async Task ExecuteAsync_DefectSignalsProduceChangedHtmlAndCssCandidate()
    {
        var unit = new DesignConversionRuntimeScaffoldImproverUnit();
        var request = CreateRequest();
        var candidate = CreateCandidate(request, html: "<main class=\"card\">Hello</main>", css: ".card{display:grid;}");
        var verdict = CreateVerdict(
            passed: false,
            defects:
            [
                CreateDefect("low-contrast", "contrast", "major", "Text has low foreground/background contrast."),
                CreateDefect("crowded-spacing", "spacing", "minor", "Card content is crowded and needs padding.")
            ]);

        var result = await unit.ExecuteAsync(new LogicalUnitRequest<DesignConversionImproverUnitInput>(
            RunId: "pass151-non-copy",
            UnitId: DesignConversionRuntimeScaffoldImproverUnit.UnitId,
            UnitRole: DesignConversionRuntimeScaffoldImproverUnit.Role,
            DomainTag: DesignConversionRuntimeScaffoldImproverUnit.DomainTag,
            Input: new DesignConversionImproverUnitInput(
                Request: request,
                CriticVerdict: verdict,
                Candidate: candidate,
                QaEvidenceRefs: ["qa:defect-signal"],
                AiTeacherEvidenceRefs: ["teacher:redacted-correction-hint"]),
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var output = result.Value!.Output;
        Assert.NotEqual(candidate.Html, output.Candidate.Html);
        Assert.NotEqual(candidate.Css, output.Candidate.Css);
        Assert.Equal(output.Candidate.Html, output.Output.Html);
        Assert.Equal(output.Candidate.Css, output.Output.Css);
        Assert.Contains("dcm-local-improved", output.Candidate.Html);
        Assert.Contains("color:#111827", output.Candidate.Css);
        Assert.Contains("background-color:#ffffff", output.Candidate.Css);
        Assert.Contains("gap:12px", output.Candidate.Css);
        Assert.Contains("padding:16px", output.Candidate.Css);
        Assert.Contains("DESIGN_CONVERSION_IMPROVER_LOCAL_NON_COPY", result.Value.EvidenceIds);
        Assert.Contains("local-improver-rule:contrast", output.Candidate.EvidenceRefs);
        Assert.Contains("local-improver-rule:spacing", output.Candidate.EvidenceRefs);
        Assert.Equal(bool.TrueString, output.Candidate.Metadata["candidate_html_changed"]);
        Assert.Equal(bool.TrueString, output.Candidate.Metadata["candidate_css_changed"]);
        Assert.Equal("defect_driven_visible_html_css_candidate", output.Candidate.Metadata["local_improver_behavior"]);
        Assert.Equal("remaining:learned_weights_not_used_for_local_rule_choice", output.Candidate.Metadata["learned_integration_status"]);
        Assert.False(output.DeterministicLogicClaimedAsLearning);
    }

    [Fact]
    public async Task ExecuteAsync_NoDefectsKeepsCandidateUnchangedAndLabelsNoLocalChange()
    {
        var unit = new DesignConversionRuntimeScaffoldImproverUnit();
        var request = CreateRequest();
        var candidate = CreateCandidate(request, html: "<main class=\"card\">Hello</main>", css: ".card{display:grid;}");
        var verdict = CreateVerdict(passed: true, defects: []);

        var result = await unit.ExecuteAsync(new LogicalUnitRequest<DesignConversionImproverUnitInput>(
            RunId: "pass151-no-defects",
            UnitId: DesignConversionRuntimeScaffoldImproverUnit.UnitId,
            UnitRole: DesignConversionRuntimeScaffoldImproverUnit.Role,
            DomainTag: DesignConversionRuntimeScaffoldImproverUnit.DomainTag,
            Input: new DesignConversionImproverUnitInput(
                Request: request,
                CriticVerdict: verdict,
                Candidate: candidate,
                QaEvidenceRefs: ["qa:clean"],
                AiTeacherEvidenceRefs: []),
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var output = result.Value!.Output;
        Assert.Equal(candidate.Html, output.Candidate.Html);
        Assert.Equal(candidate.Css, output.Candidate.Css);
        Assert.DoesNotContain("DESIGN_CONVERSION_IMPROVER_LOCAL_NON_COPY", result.Value.EvidenceIds);
        Assert.Contains("DESIGN_CONVERSION_IMPROVER_NO_DEFECTS_NO_LOCAL_CHANGE", result.Value.EvidenceIds);
        Assert.Equal(bool.FalseString, output.Candidate.Metadata["candidate_html_changed"]);
        Assert.Equal(bool.FalseString, output.Candidate.Metadata["candidate_css_changed"]);
        Assert.Equal("no_defects_no_candidate_change", output.Candidate.Metadata["local_improver_behavior"]);
    }

    [Fact]
    public async Task RuntimeFlow_LearnedCorrectionWeightsChooseDifferentVisibleRulesForSameDefectContext()
    {
        var input = CreateRuntimeInputWithContrastAndSpacingDefects();

        var deterministic = await new DesignConversionRoutingFlow().RunAsync(
            input,
            "pass165-deterministic-fallback");
        Assert.True(deterministic.IsSuccess, deterministic.ErrorMessage);
        Assert.Equal(bool.FalseString, deterministic.Value!.Candidate.Metadata["learned_correction_choice_applied"]);
        Assert.Equal(
            "contrast|spacing",
            deterministic.Value.Candidate.Metadata["local_improver_selected_rule_ids"]);
        Assert.Equal(
            "remaining:learned_weights_not_used_for_local_rule_choice",
            deterministic.Value.Candidate.Metadata["learned_integration_status"]);
        Assert.Contains("color:#111827", deterministic.Value.Output.Css);
        Assert.Contains("gap:12px", deterministic.Value.Output.Css);
        Assert.Contains("DETERMINISTIC_LOGIC_CLAIMED_AS_LEARNING_FALSE", deterministic.Value.EvidenceRefs);

        var spacingMachine = await TrainFreshLoadedCorrectionChoiceMachineAsync(
            "spacing-preferred",
            chosenCategory: "spacing",
            rejectedCategory: "contrast");
        var contrastMachine = await TrainFreshLoadedCorrectionChoiceMachineAsync(
            "contrast-preferred",
            chosenCategory: "contrast",
            rejectedCategory: "spacing");

        var spacingResult = await spacingMachine.Machine.CreateRuntimeRoutingFlow().RunAsync(
            input,
            "pass165-spacing-learned-choice");
        var contrastResult = await contrastMachine.Machine.CreateRuntimeRoutingFlow().RunAsync(
            input,
            "pass165-contrast-learned-choice");

        Assert.True(spacingResult.IsSuccess, spacingResult.ErrorMessage);
        Assert.True(contrastResult.IsSuccess, contrastResult.ErrorMessage);
        Assert.True(spacingMachine.PreferredWeight > 0);
        Assert.True(spacingMachine.RejectedWeight < 0);
        Assert.True(contrastMachine.PreferredWeight > 0);
        Assert.True(contrastMachine.RejectedWeight < 0);

        Assert.Equal(bool.FalseString, spacingResult.Value!.Candidate.Metadata["TrainedModelAuthority"]);
        Assert.Equal(bool.FalseString, spacingResult.Value.Candidate.Metadata["trained_model_authority"]);
        Assert.Equal(bool.TrueString, spacingResult.Value.Candidate.Metadata["not_trained_model_authority"]);
        Assert.Equal("spacing", spacingResult.Value!.Candidate.Metadata["local_improver_selected_rule_ids"]);
        Assert.Equal("contrast", contrastResult.Value!.Candidate.Metadata["local_improver_selected_rule_ids"]);
        Assert.Equal("correction_category:spacing", spacingResult.Value.Candidate.Metadata["learned_correction_feature_key"]);
        Assert.Equal("correction_category:contrast", contrastResult.Value.Candidate.Metadata["learned_correction_feature_key"]);
        Assert.Equal(
            "bounded:learned_weights_ranked_local_correction_choice",
            spacingResult.Value.Candidate.Metadata["learned_integration_status"]);
        Assert.Equal(
            "bounded:learned_weights_ranked_local_correction_choice",
            contrastResult.Value.Candidate.Metadata["learned_integration_status"]);

        Assert.Contains("gap:12px", spacingResult.Value.Output.Css);
        Assert.Contains("padding:16px", spacingResult.Value.Output.Css);
        Assert.DoesNotContain("color:#111827", spacingResult.Value.Output.Css);
        Assert.Contains("color:#111827", contrastResult.Value.Output.Css);
        Assert.Contains("background-color:#ffffff", contrastResult.Value.Output.Css);
        Assert.DoesNotContain("gap:12px", contrastResult.Value.Output.Css);
        Assert.NotEqual(spacingResult.Value.Output.Css, contrastResult.Value.Output.Css);
        Assert.Equal(spacingResult.Value.Candidate.Css, spacingResult.Value.Output.Css);
        Assert.Equal(contrastResult.Value.Candidate.Css, contrastResult.Value.Output.Css);

        Assert.Contains(
            DesignConversionRuntimeScaffoldImproverUnit.LearnedCorrectionChoiceAppliedEvidence,
            spacingResult.Value.EvidenceRefs);
        Assert.Contains(
            DesignConversionRuntimeScaffoldImproverUnit.LearnedCorrectionChoiceAppliedEvidence,
            contrastResult.Value.EvidenceRefs);
        Assert.Contains(DesignConversionMachine.LearnedInferenceAppliedEvidence, spacingResult.Value.EvidenceRefs);
        Assert.Contains(DesignConversionMachine.LearnedInferenceAppliedEvidence, contrastResult.Value.EvidenceRefs);
    }

    private static SubmittedDesignConversionRequest CreateRequest()
        => new(
            RequestId: "pass151-request",
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
                Format: "figma-json",
                Content: "{\"type\":\"FRAME\",\"version\":\"original\"}",
                Sha256: "sha256:tree-original",
                Metadata: new Dictionary<string, string>()),
            Screenshots: [new DesignRenderArtifact("shot", "screenshot", 1440, 900, "artifact://shot.png", "sha256:shot", new Dictionary<string, string>())],
            DesignRenders: [new DesignRenderArtifact("render", "design-render", 1440, 900, "artifact://render.png", "sha256:render", new Dictionary<string, string>())],
            Budget: new DesignConversionBudgetMetadata(
                UserId: "user-redacted",
                Tier: "pro",
                MaxImprovementCandidates: 3,
                MaxRenderQaRuns: 3,
                BudgetUsd: 1.00m,
                Metadata: new Dictionary<string, string>()),
            Provenance: new Dictionary<string, string> { ["source"] = "pass151-test" });

    private static DesignConversionImprovementCandidate CreateCandidate(
        SubmittedDesignConversionRequest request,
        string html,
        string css)
        => new(
            CandidateId: $"{request.RequestId}:candidate",
            RequestId: request.RequestId,
            Html: html,
            Css: css,
            ChangeReasons: ["initial runtime candidate"],
            EvidenceRefs: ["candidate:initial"],
            Metadata: new Dictionary<string, string>());

    private static VisualVerdict CreateVerdict(bool passed, IReadOnlyList<VisualDefect> defects)
        => new(
            VerdictId: passed ? "pass151-clean-verdict" : "pass151-defect-verdict",
            Score: passed ? 0.96 : 0.42,
            Defects: defects,
            EvidenceRefs: ["verdict:pass151"],
            Passed: passed);

    private static VisualDefect CreateDefect(
        string defectId,
        string category,
        string severity,
        string description)
        => new(
            DefectId: defectId,
            Category: category,
            Severity: severity,
            Description: description,
            X: 10,
            Y: 20,
            Width: 100,
            Height: 40,
            EvidenceImageIds: ["shot", "render"]);

    private static DesignConversionRuntimeFlowInput CreateRuntimeInputWithContrastAndSpacingDefects()
    {
        var request = CreateRequest() with { RequestId = "pass165-shared-defect-request" };
        var expected = CreateImage("pass165-expected", 1440, 900);
        var actual = CreateImage("pass165-actual", 1440, 900);
        var verdict = CreateVerdict(
            passed: false,
            defects:
            [
                CreateDefect("pass165-low-contrast", "contrast", "major", "Foreground/background contrast is low."),
                CreateDefect("pass165-crowded-spacing", "spacing", "major", "Card content is crowded and needs padding.")
            ]);
        var qaResult = new RenderQaResultRecord(
            ResultId: "pass165-render-qa",
            RequestId: request.RequestId,
            Expected: expected,
            Actual: actual,
            Verdict: verdict,
            Metrics: new Dictionary<string, double> { ["visual_distance"] = 0.58 },
            Warnings: []);

        return new DesignConversionRuntimeFlowInput(
            Request: request,
            QaResults: [qaResult],
            ComparisonArtifacts: [],
            AiTeacherEvidenceRefs: ["teacher:pass165-redacted-correction-signal"],
            DeterministicEvidenceRefs: new Dictionary<string, string>
            {
                ["pass"] = "pass165",
                ["defect_context"] = "same_contrast_and_spacing"
            });
    }

    private static RenderQaImageRecord CreateImage(string id, int width, int height)
        => new(
            ImageId: id,
            Label: id,
            Resolution: new RenderQaResolution($"res-{width}x{height}", width, height, 1.0),
            Uri: $"artifact://{id}.png",
            Sha256: $"sha256:{id}",
            Metadata: new Dictionary<string, string>());

    private static async Task<CorrectionChoiceMachineProbe> TrainFreshLoadedCorrectionChoiceMachineAsync(
        string caseId,
        string chosenCategory,
        string rejectedCategory)
    {
        var chosenRuleId = DesignConversionRuntimeScaffoldImproverUnit.NormalizeCorrectionRuleId(chosenCategory);
        var rejectedRuleId = DesignConversionRuntimeScaffoldImproverUnit.NormalizeCorrectionRuleId(rejectedCategory);
        var source = new DesignConversionMachine();
        var train = await source.TrainBatchAsync(BuildCorrectionChoiceBatch(caseId, chosenCategory, rejectedCategory));
        Assert.True(train.IsSuccess, train.ErrorMessage);
        Assert.True(train.Value!.RewardMargin > 0);

        var checkpoint = source.ExportCheckpoint();
        Assert.True(checkpoint.IsSuccess, checkpoint.ErrorMessage);
        var weights = checkpoint.Value!.ImproverUnit.LearnedWeights ?? [];
        var preferred = Assert.Single(
            weights,
            weight => string.Equals(weight.FeatureKey, $"correction_category:{chosenRuleId}", StringComparison.Ordinal));
        var rejected = Assert.Single(
            weights,
            weight => string.Equals(weight.FeatureKey, $"correction_category:{rejectedRuleId}", StringComparison.Ordinal));
        Assert.Equal("chosen_correction_category_preference", preferred.Derivation);
        Assert.Equal("rejected_correction_category_penalty", rejected.Derivation);

        var freshLoaded = new DesignConversionMachine();
        var import = freshLoaded.ImportCheckpoint(checkpoint.Value);
        Assert.True(import.IsSuccess, import.ErrorMessage);

        return new CorrectionChoiceMachineProbe(
            Machine: freshLoaded,
            PreferredWeight: preferred.Weight,
            RejectedWeight: rejected.Weight,
            CheckpointVersion: checkpoint.Value.Version);
    }

    private static LearningBatch<DesignConversionDomain.DesignConversionDomain> BuildCorrectionChoiceBatch(
        string caseId,
        string chosenCategory,
        string rejectedCategory)
        => new(
            BatchId: $"pass165-{caseId}-batch",
            DomainTag: DesignConversionMachine.DomainTag,
            TrainingItems:
            [
                new LearningTrainingItem(
                    ItemId: $"pass165-{caseId}:improver",
                    Prompt: $$"""
                    {"unit_id":"{{DesignConversionDpoUnits.ImproverUnit}}","source_kind":"figma","pair_family":"pass165-correction-choice"}
                    """,
                    ChosenResponse: $$"""
                    {"visual_score":0.92,"accepted":true,"correction_category":"{{chosenCategory}}"}
                    """,
                    RejectedResponse: $$"""
                    {"visual_score":0.12,"accepted":false,"correction_category":"{{rejectedCategory}}"}
                    """)
            ],
            SourceIds: [$"pass165-{caseId}-source"]);

    private sealed record CorrectionChoiceMachineProbe(
        DesignConversionMachine Machine,
        double PreferredWeight,
        double RejectedWeight,
        string CheckpointVersion);
}
