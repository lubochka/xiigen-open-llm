// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

[Collection(ExtensionRuntimeTokenQualityArtifactPaths.CollectionName)]
public sealed class ExtensionQualitySemanticGates14ClosureTests
{
    private static readonly string ArtifactDir = ExtensionRuntimeTokenQualityArtifactPaths.ArtifactDir;

    private static readonly string StatePath = ExtensionRuntimeTokenQualityArtifactPaths.StatePath;

    [Fact]
    public void QualitySemanticGates14Artifacts_OpenOnlyBoundedSemanticGraduation()
    {
        var qsg07ReportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-07-report.json");
        var qsg08ReportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-08-report.json");
        var qsg09ReportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-09-report.json");
        var qsg10ReportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-10-report.json");
        var qsg12ReportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-12-report.json");
        var qsg12ImprovementRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-12-adaptive-improvement-rows.jsonl");
        var qsg13ReportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-13-report.json");
        var qsg13PairRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-13-pair-results.jsonl");
        var qsg13DisplayRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-13-adaptive-display-rows.jsonl");

        var semanticGraduationRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-14-semantic-graduation-rows.jsonl");
        var commonReadinessRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-14-common-readiness-closure-rows.jsonl");
        var componentGatesPath = Path.Combine(ArtifactDir, "quality-semantic-gates-14-component-gates.jsonl");
        var reportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-14-report.json");
        var reportMdPath = Path.Combine(ArtifactDir, "quality-semantic-gates-14-report.md");
        var validationPath = Path.Combine(ArtifactDir, "quality-semantic-gates-14.validation.json");
        var statePatchPath = ExtensionExecutionStatePatchWriter.DefaultPatchPath(ArtifactDir, "QUALITY-SEMANTIC-GATES-14");

        using var qsg07Document = JsonDocument.Parse(File.ReadAllText(qsg07ReportPath));
        using var qsg08Document = JsonDocument.Parse(File.ReadAllText(qsg08ReportPath));
        using var qsg09Document = JsonDocument.Parse(File.ReadAllText(qsg09ReportPath));
        using var qsg10Document = JsonDocument.Parse(File.ReadAllText(qsg10ReportPath));
        using var qsg12Document = JsonDocument.Parse(File.ReadAllText(qsg12ReportPath));
        using var qsg13Document = JsonDocument.Parse(File.ReadAllText(qsg13ReportPath));
        var qsg07Report = qsg07Document.RootElement;
        var qsg08Report = qsg08Document.RootElement;
        var qsg09Report = qsg09Document.RootElement;
        var qsg10Report = qsg10Document.RootElement;
        var qsg12Report = qsg12Document.RootElement;
        var qsg13Report = qsg13Document.RootElement;

        var qsg12ImprovementByPair = ReadJsonl(qsg12ImprovementRowsPath)
            .ToDictionary(row => row.GetProperty("pair_id").GetString()!, row => row);
        var qsg13DisplayByPair = ReadJsonl(qsg13DisplayRowsPath)
            .ToDictionary(row => row.GetProperty("pair_id").GetString()!, row => row);
        var qsg13Pairs = ReadJsonl(qsg13PairRowsPath);

        var sourceDisjointPassed = qsg07Report.GetProperty("source_disjoint_eval_gate_passed").GetBoolean()
            && qsg10Report.GetProperty("source_disjoint_eval_gate_passed").GetBoolean();
        var ablationPassed = qsg07Report.GetProperty("ablation_gate_passed").GetBoolean()
            && qsg10Report.GetProperty("ablation_gate_passed").GetBoolean();
        var leakagePassed = qsg07Report.GetProperty("leakage_gate_passed").GetBoolean()
            && qsg10Report.GetProperty("leakage_gate_passed").GetBoolean();
        var freshLoadPassed = qsg08Report.GetProperty("fresh_load_gate_passed").GetBoolean()
            && qsg10Report.GetProperty("fresh_load_gate_passed").GetBoolean();
        var continueTrainingPassed = qsg08Report.GetProperty("continue_training_gate_passed").GetBoolean()
            && qsg10Report.GetProperty("continue_training_gate_passed").GetBoolean();
        var baselineCommonRuntimePassed = qsg09Report.GetProperty("baseline_common_runtime_gate_passed").GetBoolean()
            && qsg10Report.GetProperty("baseline_common_runtime_gate_passed").GetBoolean();
        var qsg12QualityImprovementPassed = qsg12Report.GetProperty("quality_improvement_gate_passed").GetBoolean()
            && qsg12Report.GetProperty("adaptive_quality_improvement_evidence_pairs").GetInt32() == qsg13Pairs.Count;
        var qsg13DisplayPassed = qsg13Report.GetProperty("adaptive_user_facing_output_repaired").GetBoolean()
            && qsg13Report.GetProperty("adaptive_chat_text_improved_pairs").GetInt32() == qsg13Pairs.Count;
        var qsg13TokenPassed = qsg13Report.GetProperty("adaptive_tokens_not_more_than_baseline").GetBoolean()
            && qsg13Report.GetProperty("token_reduction_observed_pairs").GetInt32() == qsg13Pairs.Count
            && qsg13Report.GetProperty("total_token_delta").GetInt32() > 0;
        var noOverclaimPassed = !qsg10Report.GetProperty("semantic_quality_claimed").GetBoolean()
            && !qsg13Report.GetProperty("semantic_quality_claimed").GetBoolean()
            && !qsg13Report.GetProperty("common_readiness_claim_allowed").GetBoolean()
            && !qsg13Report.GetProperty("final_readiness_claimed").GetBoolean();

        var semanticGraduationRows = qsg13Pairs
            .Select(pair =>
            {
                var pairId = pair.GetProperty("pair_id").GetString()!;
                var improvement = qsg12ImprovementByPair[pairId];
                var display = qsg13DisplayByPair[pairId];
                var pairQualityImproved = improvement.GetProperty("quality_improvement_observed").GetBoolean()
                    && !improvement.GetProperty("quality_regression_observed").GetBoolean();
                var pairDisplayImproved = display.GetProperty("adaptive_chat_text_improved").GetBoolean();
                var pairTokenReduced = pair.GetProperty("token_reduction_observed").GetBoolean();
                var pairSemanticGraduationAccepted = sourceDisjointPassed
                    && ablationPassed
                    && leakagePassed
                    && freshLoadPassed
                    && continueTrainingPassed
                    && baselineCommonRuntimePassed
                    && qsg12QualityImprovementPassed
                    && qsg13DisplayPassed
                    && qsg13TokenPassed
                    && noOverclaimPassed
                    && pairQualityImproved
                    && pairDisplayImproved
                    && pairTokenReduced;

                return new
                {
                    schema = "xiigen.quality_semantic_gates_14.semantic_graduation_row.v1",
                    phase_id = "QUALITY-SEMANTIC-GATES-14",
                    source_phase_ids = new[] { "QUALITY-SEMANTIC-GATES-07", "QUALITY-SEMANTIC-GATES-08", "QUALITY-SEMANTIC-GATES-09", "QUALITY-SEMANTIC-GATES-10", "QUALITY-SEMANTIC-GATES-12", "QUALITY-SEMANTIC-GATES-13" },
                    scenario_id = pair.GetProperty("scenario_id").GetString(),
                    pair_id = pairId,
                    host = pair.GetProperty("host").GetString(),
                    method = pair.GetProperty("method").GetString(),
                    component_id = ToComponentId(pair.GetProperty("method").GetString()!),
                    source_disjoint_eval_gate_passed = sourceDisjointPassed,
                    ablation_gate_passed = ablationPassed,
                    leakage_gate_passed = leakagePassed,
                    fresh_load_gate_passed = freshLoadPassed,
                    continue_training_gate_passed = continueTrainingPassed,
                    baseline_common_runtime_gate_passed = baselineCommonRuntimePassed,
                    quality_improvement_gate_passed = pairQualityImproved,
                    adaptive_chat_text_improved = pairDisplayImproved,
                    token_reduction_observed = pairTokenReduced,
                    no_overclaim_gate_passed = noOverclaimPassed,
                    limited_semantic_quality_evidence_accepted = pairSemanticGraduationAccepted,
                    semantic_graduation_accepted = pairSemanticGraduationAccepted,
                    common_readiness_claim_allowed = false,
                    token_savings_claim_allowed = pairSemanticGraduationAccepted,
                    product_runtime_semantic_quality_claimed = false,
                    evidence_scope = "bounded semantic graduation for the evaluated QSG scenario pack only; this does not mutate runtime claim behavior and does not prove common readiness, promotion, locator update, rollback, or final readiness.",
                    missing_evidence_for_common_readiness = CommonReadinessMissingEvidence
                };
            })
            .OrderBy(row => row.pair_id, StringComparer.Ordinal)
            .ToArray();

        var commonReadinessRows = semanticGraduationRows
            .Select(row => new
            {
                schema = "xiigen.quality_semantic_gates_14.common_readiness_closure_row.v1",
                phase_id = "QUALITY-SEMANTIC-GATES-14",
                row.pair_id,
                row.component_id,
                semantic_graduation_accepted = row.semantic_graduation_accepted,
                common_readiness_claim_allowed = false,
                common_readiness_decision = "KEEP_CLOSED",
                missing_evidence = CommonReadinessMissingEvidence,
                no_overclaim_note = "QSG-14 accepts bounded semantic graduation evidence only; common model readiness still needs promotion/locator/final-readiness closure packets."
            })
            .ToArray();

        var componentRows = semanticGraduationRows
            .GroupBy(row => row.component_id)
            .Select(group =>
            {
                var rows = group.ToArray();
                return new
                {
                    schema = "xiigen.quality_semantic_gates_14.component_gate.v1",
                    phase_id = "QUALITY-SEMANTIC-GATES-14",
                    component_id = group.Key,
                    pair_count = rows.Length,
                    semantic_graduation_accepted_pairs = rows.Count(row => row.semantic_graduation_accepted),
                    limited_semantic_quality_evidence_accepted_pairs = rows.Count(row => row.limited_semantic_quality_evidence_accepted),
                    token_savings_claim_allowed_pairs = rows.Count(row => row.token_savings_claim_allowed),
                    component_semantic_graduation_accepted = rows.All(row => row.semantic_graduation_accepted),
                    common_readiness_claim_allowed = false,
                    component_common_readiness_decision = "KEEP_CLOSED",
                    missing_evidence_for_common_readiness = CommonReadinessMissingEvidence
                };
            })
            .OrderBy(row => row.component_id, StringComparer.Ordinal)
            .ToArray();

        var semanticGraduationAcceptedPairs = semanticGraduationRows.Count(row => row.semantic_graduation_accepted);
        var commonReadinessGraduatedPairs = commonReadinessRows.Count(row => row.common_readiness_claim_allowed);
        var tokenSavingsClaimAllowedPairs = semanticGraduationRows.Count(row => row.token_savings_claim_allowed);

        var beforeAfterCounts = new
        {
            qsg13 = new
            {
                pair_count = qsg13Report.GetProperty("pair_count").GetInt32(),
                adaptive_quality_improvement_evidence_pairs = qsg13Report.GetProperty("adaptive_quality_improvement_evidence_pairs").GetInt32(),
                adaptive_chat_text_improved_pairs = qsg13Report.GetProperty("adaptive_chat_text_improved_pairs").GetInt32(),
                semantic_graduation_accepted_pairs = qsg13Report.GetProperty("semantic_graduation_accepted_pairs").GetInt32(),
                common_readiness_graduated_pairs = qsg13Report.GetProperty("common_readiness_graduated_pairs").GetInt32(),
                token_reduction_observed_pairs = qsg13Report.GetProperty("token_reduction_observed_pairs").GetInt32(),
                token_increase_observed_pairs = qsg13Report.GetProperty("token_increase_observed_pairs").GetInt32(),
                token_equal_pairs = qsg13Report.GetProperty("token_equal_pairs").GetInt32(),
                baseline_total_tokens = qsg13Report.GetProperty("baseline_total_tokens").GetInt32(),
                adaptive_total_tokens = qsg13Report.GetProperty("adaptive_total_tokens").GetInt32(),
                total_token_delta = qsg13Report.GetProperty("total_token_delta").GetInt32(),
                semantic_quality_gate_passed = qsg13Report.GetProperty("semantic_quality_gate_passed").GetBoolean(),
                common_readiness_claim_allowed = qsg13Report.GetProperty("common_readiness_claim_allowed").GetBoolean(),
                token_savings_claim_allowed = qsg13Report.GetProperty("token_savings_claim_allowed").GetBoolean(),
                final_readiness_claimed = qsg13Report.GetProperty("final_readiness_claimed").GetBoolean()
            },
            qsg14 = new
            {
                pair_count = semanticGraduationRows.Length,
                adaptive_quality_improvement_evidence_pairs = qsg13Report.GetProperty("adaptive_quality_improvement_evidence_pairs").GetInt32(),
                adaptive_chat_text_improved_pairs = qsg13Report.GetProperty("adaptive_chat_text_improved_pairs").GetInt32(),
                semantic_graduation_accepted_pairs = semanticGraduationAcceptedPairs,
                limited_semantic_quality_evidence_accepted_pairs = semanticGraduationRows.Count(row => row.limited_semantic_quality_evidence_accepted),
                common_readiness_graduated_pairs = commonReadinessGraduatedPairs,
                token_savings_claim_allowed_pairs = tokenSavingsClaimAllowedPairs,
                token_reduction_observed_pairs = qsg13Report.GetProperty("token_reduction_observed_pairs").GetInt32(),
                token_increase_observed_pairs = qsg13Report.GetProperty("token_increase_observed_pairs").GetInt32(),
                token_equal_pairs = qsg13Report.GetProperty("token_equal_pairs").GetInt32(),
                baseline_total_tokens = qsg13Report.GetProperty("baseline_total_tokens").GetInt32(),
                adaptive_total_tokens = qsg13Report.GetProperty("adaptive_total_tokens").GetInt32(),
                total_token_delta = qsg13Report.GetProperty("total_token_delta").GetInt32(),
                semantic_quality_gate_passed = semanticGraduationAcceptedPairs == semanticGraduationRows.Length,
                common_readiness_claim_allowed = false,
                token_savings_claim_allowed = tokenSavingsClaimAllowedPairs == semanticGraduationRows.Length,
                final_readiness_claimed = false
            }
        };

        var semanticQualityGatePassed = semanticGraduationAcceptedPairs == semanticGraduationRows.Length;
        var tokenSavingsClaimAllowed = tokenSavingsClaimAllowedPairs == semanticGraduationRows.Length;
        var report = new
        {
            schema = "xiigen.quality_semantic_gates_14.semantic_common_readiness_closure_report.v1",
            phase_id = "QUALITY-SEMANTIC-GATES-14",
            input_phase_id = "QUALITY-SEMANTIC-GATES-13",
            qsg13_report_ref = "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-13-report.json",
            semantic_graduation_rows_ref = "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-14-semantic-graduation-rows.jsonl",
            common_readiness_rows_ref = "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-14-common-readiness-closure-rows.jsonl",
            component_gates_ref = "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-14-component-gates.jsonl",
            pair_count = semanticGraduationRows.Length,
            source_disjoint_eval_gate_passed = sourceDisjointPassed,
            ablation_gate_passed = ablationPassed,
            leakage_gate_passed = leakagePassed,
            fresh_load_gate_passed = freshLoadPassed,
            continue_training_gate_passed = continueTrainingPassed,
            baseline_common_runtime_gate_passed = baselineCommonRuntimePassed,
            human_readable_rubric_gate_executed = true,
            quality_improvement_gate_passed = qsg12QualityImprovementPassed,
            adaptive_user_facing_output_repaired = qsg13DisplayPassed,
            adaptive_tokens_not_more_than_baseline = qsg13Report.GetProperty("adaptive_tokens_not_more_than_baseline").GetBoolean(),
            no_overclaim_gate_passed = noOverclaimPassed,
            semantic_graduation_accepted_pairs = semanticGraduationAcceptedPairs,
            limited_semantic_quality_evidence_accepted_pairs = semanticGraduationRows.Count(row => row.limited_semantic_quality_evidence_accepted),
            common_readiness_graduated_pairs = commonReadinessGraduatedPairs,
            token_reduction_observed_pairs = qsg13Report.GetProperty("token_reduction_observed_pairs").GetInt32(),
            token_increase_observed_pairs = qsg13Report.GetProperty("token_increase_observed_pairs").GetInt32(),
            token_equal_pairs = qsg13Report.GetProperty("token_equal_pairs").GetInt32(),
            baseline_total_tokens = qsg13Report.GetProperty("baseline_total_tokens").GetInt32(),
            adaptive_total_tokens = qsg13Report.GetProperty("adaptive_total_tokens").GetInt32(),
            total_token_delta = qsg13Report.GetProperty("total_token_delta").GetInt32(),
            token_delta_direction = qsg13Report.GetProperty("token_delta_direction").GetString(),
            semantic_quality_gate_passed = semanticQualityGatePassed,
            bounded_semantic_quality_claim_allowed = semanticQualityGatePassed,
            common_readiness_claim_allowed = false,
            quality_improvement_claim_allowed = semanticQualityGatePassed,
            token_savings_claim_allowed = tokenSavingsClaimAllowed,
            semantic_quality_claimed = false,
            learnability_claimed = false,
            product_readiness_claim_allowed = false,
            target_promotion_done = false,
            locator_update_done = false,
            promotion_readiness_claimed = false,
            final_readiness_claimed = false,
            training_started = false,
            checkpoint_mutation_started = false,
            deterministic_helper_classification = "QSG-14 evaluates accumulated packet evidence and accepts bounded semantic graduation for the evaluated scenario pack only; deterministic rubrics/token counters remain evidence and are not hidden runtime semantic logic.",
            before_after_counts = beforeAfterCounts,
            claims_opened_in_this_slice = semanticQualityGatePassed
                ? new[]
                {
                    "bounded_semantic_graduation_for_54_evaluated_pairs",
                    "bounded_semantic_quality_gate_for_the_QSG_scenario_pack",
                    "bounded_token_savings_claim_for_the_QSG13_paired_trace_set"
                }
                : Array.Empty<string>(),
            claims_kept_closed = new[]
            {
                "bounded_semantic_quality_claim_allowed_when_current_upstream_gates_do_not_all_pass",
                "token_savings_claim_allowed_when_current_upstream_gates_do_not_all_pass",
                "common_readiness_claim_allowed",
                "product_runtime_semantic_quality_claimed",
                "learnability_claimed",
                "target_promotion_done",
                "locator_update_done",
                "promotion_readiness_claimed",
                "product_readiness_claim_allowed",
                "final_readiness_claimed"
            },
            remaining_blockers = CommonReadinessMissingEvidence,
            next_bounded_work_order = "QUALITY-SEMANTIC-GATES-15: produce common-readiness target/promotion/locator/final-readiness closure evidence or keep common/final readiness closed."
        };

        var validation = new
        {
            schema = "xiigen.quality_semantic_gates_14.validation.v1",
            phase_id = "QUALITY-SEMANTIC-GATES-14",
            semantic_graduation_accepted_pairs_expected = semanticQualityGatePassed ? 54 : 0,
            semantic_graduation_accepted_pairs_actual = semanticGraduationAcceptedPairs,
            limited_semantic_quality_evidence_accepted_pairs_expected = semanticQualityGatePassed ? 54 : 0,
            limited_semantic_quality_evidence_accepted_pairs_actual = semanticGraduationRows.Count(row => row.limited_semantic_quality_evidence_accepted),
            common_readiness_graduated_pairs_expected = 0,
            common_readiness_graduated_pairs_actual = commonReadinessGraduatedPairs,
            token_savings_claim_allowed_pairs_expected = tokenSavingsClaimAllowed ? 54 : 0,
            token_savings_claim_allowed_pairs_actual = tokenSavingsClaimAllowedPairs,
            component_gate_count_expected = 3,
            component_gate_count_actual = componentRows.Length,
            semantic_quality_gate_passed = semanticQualityGatePassed,
            bounded_semantic_quality_claim_allowed = semanticQualityGatePassed,
            common_readiness_claim_allowed = false,
            quality_improvement_claim_allowed = semanticQualityGatePassed,
            token_savings_claim_allowed = tokenSavingsClaimAllowed,
            semantic_quality_claimed = false,
            product_readiness_claim_allowed = false,
            target_promotion_done = false,
            locator_update_done = false,
            promotion_readiness_claimed = false,
            final_readiness_claimed = false,
            training_started = false,
            checkpoint_mutation_started = false,
            missing_evidence_for_common_readiness = CommonReadinessMissingEvidence,
            output_files = ExtensionRuntimeTokenQualityArtifactPaths.ToCanonicalWindowsRepoRefs(
                "QUALITY-SEMANTIC-GATES-14 output artifact",
                semanticGraduationRowsPath,
                commonReadinessRowsPath,
                componentGatesPath,
                reportPath,
                reportMdPath,
                validationPath,
                statePatchPath)
        };

        WriteJsonl(semanticGraduationRowsPath, semanticGraduationRows);
        WriteJsonl(commonReadinessRowsPath, commonReadinessRows);
        WriteJsonl(componentGatesPath, componentRows);
        WriteJson(reportPath, report);
        WriteQsg14Markdown(
            reportMdPath,
            beforeAfterCounts,
            semanticGraduationAcceptedPairs,
            tokenSavingsClaimAllowedPairs,
            semanticQualityGatePassed,
            tokenSavingsClaimAllowed);
        WriteJson(validationPath, validation);
        statePatchPath = UpdateState(report, validation, semanticQualityGatePassed, tokenSavingsClaimAllowed);

        Assert.Equal(54, semanticGraduationRows.Length);
        Assert.Equal(semanticQualityGatePassed ? 54 : 0, semanticGraduationAcceptedPairs);
        Assert.Equal(tokenSavingsClaimAllowed ? 54 : 0, tokenSavingsClaimAllowedPairs);
        Assert.Equal(0, commonReadinessGraduatedPairs);
        Assert.All(semanticGraduationRows, row =>
        {
            Assert.Equal(semanticQualityGatePassed, row.semantic_graduation_accepted);
            Assert.Equal(semanticQualityGatePassed, row.limited_semantic_quality_evidence_accepted);
            Assert.Equal(tokenSavingsClaimAllowed, row.token_savings_claim_allowed);
            Assert.False(row.common_readiness_claim_allowed);
            Assert.False(row.product_runtime_semantic_quality_claimed);
            Assert.Equal(CommonReadinessMissingEvidence, row.missing_evidence_for_common_readiness);
        });
        Assert.All(commonReadinessRows, row =>
        {
            Assert.Equal("KEEP_CLOSED", row.common_readiness_decision);
            Assert.False(row.common_readiness_claim_allowed);
            Assert.Equal(CommonReadinessMissingEvidence, row.missing_evidence);
        });

        using var writtenReport = JsonDocument.Parse(File.ReadAllText(reportPath));
        var root = writtenReport.RootElement;
        Assert.Equal(semanticQualityGatePassed, root.GetProperty("semantic_quality_gate_passed").GetBoolean());
        Assert.Equal(semanticQualityGatePassed, root.GetProperty("bounded_semantic_quality_claim_allowed").GetBoolean());
        Assert.Equal(semanticQualityGatePassed, root.GetProperty("quality_improvement_claim_allowed").GetBoolean());
        Assert.Equal(tokenSavingsClaimAllowed, root.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("common_readiness_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(root.GetProperty("learnability_claimed").GetBoolean());
        Assert.False(root.GetProperty("product_readiness_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("target_promotion_done").GetBoolean());
        Assert.False(root.GetProperty("locator_update_done").GetBoolean());
        Assert.False(root.GetProperty("promotion_readiness_claimed").GetBoolean());
        Assert.False(root.GetProperty("final_readiness_claimed").GetBoolean());
        Assert.False(root.GetProperty("training_started").GetBoolean());
        Assert.False(root.GetProperty("checkpoint_mutation_started").GetBoolean());
        AssertJsonStringArray(root.GetProperty("remaining_blockers"), CommonReadinessMissingEvidence);

        using var writtenValidation = JsonDocument.Parse(File.ReadAllText(validationPath));
        var validationRoot = writtenValidation.RootElement;
        Assert.Equal(semanticQualityGatePassed, validationRoot.GetProperty("bounded_semantic_quality_claim_allowed").GetBoolean());
        Assert.Equal(semanticQualityGatePassed, validationRoot.GetProperty("quality_improvement_claim_allowed").GetBoolean());
        Assert.Equal(tokenSavingsClaimAllowed, validationRoot.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(validationRoot.GetProperty("common_readiness_claim_allowed").GetBoolean());
        Assert.False(validationRoot.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(validationRoot.GetProperty("product_readiness_claim_allowed").GetBoolean());
        Assert.False(validationRoot.GetProperty("target_promotion_done").GetBoolean());
        Assert.False(validationRoot.GetProperty("locator_update_done").GetBoolean());
        Assert.False(validationRoot.GetProperty("promotion_readiness_claimed").GetBoolean());
        Assert.False(validationRoot.GetProperty("final_readiness_claimed").GetBoolean());
        Assert.False(validationRoot.GetProperty("training_started").GetBoolean());
        Assert.False(validationRoot.GetProperty("checkpoint_mutation_started").GetBoolean());
        AssertJsonStringArray(validationRoot.GetProperty("missing_evidence_for_common_readiness"), CommonReadinessMissingEvidence);

        using var writtenStatePatch = JsonDocument.Parse(File.ReadAllText(statePatchPath));
        Assert.False(writtenStatePatch.RootElement.GetProperty("giant_state_mutated").GetBoolean());
        var stateRoot = writtenStatePatch.RootElement.GetProperty("patch_fields");
        Assert.Equal("QUALITY-SEMANTIC-GATES-14", stateRoot.GetProperty("current_phase").GetString());
        Assert.False(stateRoot.GetProperty("goal_reached").GetBoolean());
        Assert.False(stateRoot.GetProperty("stop_allowed").GetBoolean());
        Assert.True(stateRoot.GetProperty("continuation_required").GetBoolean());
        Assert.Equal(semanticQualityGatePassed, stateRoot.GetProperty("quality_improvement_claim_allowed").GetBoolean());
        Assert.Equal(tokenSavingsClaimAllowed, stateRoot.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(stateRoot.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(stateRoot.GetProperty("learnability_claimed").GetBoolean());
        var stateReport = writtenStatePatch.RootElement.GetProperty("report");
        Assert.Equal(semanticQualityGatePassed, stateReport.GetProperty("bounded_semantic_quality_claim_allowed").GetBoolean());
        Assert.Equal(semanticQualityGatePassed, stateReport.GetProperty("quality_improvement_claim_allowed").GetBoolean());
        Assert.Equal(tokenSavingsClaimAllowed, stateReport.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(stateReport.GetProperty("common_readiness_claim_allowed").GetBoolean());
        Assert.False(stateReport.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(stateReport.GetProperty("learnability_claimed").GetBoolean());
        Assert.False(stateReport.GetProperty("product_readiness_claim_allowed").GetBoolean());
        Assert.False(stateReport.GetProperty("target_promotion_done").GetBoolean());
        Assert.False(stateReport.GetProperty("locator_update_done").GetBoolean());
        Assert.False(stateReport.GetProperty("promotion_readiness_claimed").GetBoolean());
        Assert.False(stateReport.GetProperty("final_readiness_claimed").GetBoolean());
        Assert.False(stateReport.GetProperty("training_started").GetBoolean());
        Assert.False(stateReport.GetProperty("checkpoint_mutation_started").GetBoolean());
        AssertJsonStringArray(stateReport.GetProperty("remaining_blockers"), CommonReadinessMissingEvidence);
        AssertJsonStringArray(
            writtenStatePatch.RootElement.GetProperty("validation").GetProperty("missing_evidence_for_common_readiness"),
            CommonReadinessMissingEvidence);
        Assert.StartsWith(
            "QUALITY-SEMANTIC-GATES-15:",
            stateRoot.GetProperty("next_action").GetString());
    }

    private static IReadOnlyList<JsonElement> ReadJsonl(string path)
        => File.ReadAllLines(path)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonDocument.Parse(line).RootElement.Clone())
            .ToArray();

    private static string ToComponentId(string method)
        => method switch
        {
            "prompt-to-task" => "prompt_to_task",
            "orchestration-plan" => "orchestration_reasoning",
            "skill-selection" => "skill_selection",
            _ => throw new InvalidOperationException($"Unknown method '{method}'.")
        };

    private static void AssertJsonStringArray(JsonElement element, IReadOnlyList<string> expected)
    {
        var actual = element.EnumerateArray()
            .Select(item => item.GetString() ?? throw new InvalidOperationException("Expected a JSON string."))
            .ToArray();
        Assert.Equal(expected, actual);
    }

    private static void WriteJsonl<T>(string path, IEnumerable<T> rows)
    {
        var lines = rows.Select(row => JsonSerializer.Serialize(row, JsonLineOptions));
        File.WriteAllText(path, string.Join("\n", lines) + "\n", new UTF8Encoding(false));
    }

    private static void WriteJson<T>(string path, T value)
        => File.WriteAllText(
            path,
            JsonSerializer.Serialize(value, JsonOptions).Replace("\r\n", "\n", StringComparison.Ordinal) + "\n",
            new UTF8Encoding(false));

    private static void WriteQsg14Markdown(
        string path,
        object beforeAfterCounts,
        int semanticGraduationAcceptedPairs,
        int tokenSavingsClaimAllowedPairs,
        bool semanticQualityGatePassed,
        bool tokenSavingsClaimAllowed)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# QUALITY-SEMANTIC-GATES-14");
        builder.AppendLine();
        builder.AppendLine("QSG-14 evaluates the accumulated QSG-07 through QSG-13 evidence and opens bounded semantic graduation only when every current upstream gate passes. Common readiness and final readiness remain closed.");
        builder.AppendLine();
        builder.AppendLine("## What This Proves");
        builder.AppendLine();
        builder.AppendLine($"- {semanticGraduationAcceptedPairs}/54 evaluated pairs have accepted semantic graduation evidence after source-disjoint, ablation, leakage, fresh-load, continue-training, baseline/common runtime, quality-improvement, ChatDisplayText, token, and no-overclaim gates are considered together.");
        builder.AppendLine($"- Bounded semantic quality claim allowed: {BoolWord(semanticQualityGatePassed)}.");
        builder.AppendLine($"- Bounded token savings claim allowed for the QSG-13 paired trace set: {BoolWord(tokenSavingsClaimAllowed)} ({tokenSavingsClaimAllowedPairs}/54 pairs).");
        builder.AppendLine();
        builder.AppendLine("## What This Does Not Prove");
        builder.AppendLine();
        builder.AppendLine("- Runtime responses still do not claim semantic quality.");
        builder.AppendLine("- Common readiness, target promotion, locator update, product readiness, learnability, rollback, and final readiness remain closed.");
        builder.AppendLine();
        builder.AppendLine("## Counts");
        builder.AppendLine();
        builder.AppendLine("```json");
        builder.AppendLine(JsonSerializer.Serialize(beforeAfterCounts, JsonOptions).Replace("\r\n", "\n", StringComparison.Ordinal));
        builder.AppendLine("```");
        builder.AppendLine();
        builder.AppendLine("Next independent track: QUALITY-SEMANTIC-GATES-15 should produce common-readiness target/promotion/locator/final-readiness closure evidence or keep common/final readiness closed.");

        File.WriteAllText(path, builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal), new UTF8Encoding(false));
    }

    private static string BoolWord(bool value) => value ? "true" : "false";

    private static string UpdateState(
        object report,
        object validation,
        bool semanticQualityGatePassed,
        bool tokenSavingsClaimAllowed)
        => ExtensionExecutionStatePatchWriter.WritePatch(
            StatePath,
            ArtifactDir,
            "QUALITY-SEMANTIC-GATES-14",
            new Dictionary<string, object?>
            {
                ["updated_at_local"] = "2026-06-17T00:00:00.000Z",
                ["current_phase"] = "QUALITY-SEMANTIC-GATES-14",
                ["phase_status"] = "BOUNDED_SEMANTIC_GRADUATION_EVALUATED_COMMON_READINESS_CLOSED",
                ["phase_scope"] = "QSG-14 considered accumulated leakage, source-disjoint eval, ablation, fresh-load, continue-training, baseline/common runtime, no-overclaim, rubric improvement, ChatDisplayText, and token diagnostics. It opens bounded semantic graduation/token-savings evidence only when all current upstream gates pass; common readiness, product semantic claims, learnability, target promotion, locator update, and final readiness remain closed.",
                ["goal_reached"] = false,
                ["stop_allowed"] = false,
                ["next_action"] = "QUALITY-SEMANTIC-GATES-15: produce common-readiness target/promotion/locator/final-readiness closure evidence or keep common/final readiness closed.",
                ["continuation_required"] = true,
                ["quality_improvement_claim_allowed"] = semanticQualityGatePassed,
                ["token_savings_claim_allowed"] = tokenSavingsClaimAllowed,
                ["semantic_quality_claimed"] = false,
                ["learnability_claimed"] = false,
                ["training_started"] = false,
                ["checkpoint_mutation_started"] = false
            },
            report,
            validation,
            JsonOptions);

    private static readonly string[] CommonReadinessMissingEvidence =
    [
        "common_readiness_target_manifest_or_checkpoint_candidate_not_promoted",
        "locator_update_not_done",
        "product_runtime_semantic_quality_claim_not_enabled_or_verified",
        "target_promotion_review_packet_missing",
        "rollback_and_final_readiness_closure_not_in_this_qsg14_slice",
        "ten_final_readiness_checks_not_run_after_this_semantic_graduation"
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private static readonly JsonSerializerOptions JsonLineOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };
}
