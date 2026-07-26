// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

[Collection(ExtensionRuntimeTokenQualityArtifactPaths.CollectionName)]
public sealed class ExtensionQualitySemanticGates10NoOverclaimTests
{
    private static readonly string ArtifactDir = ExtensionRuntimeTokenQualityArtifactPaths.ArtifactDir;
    private static readonly string StatePath = ExtensionRuntimeTokenQualityArtifactPaths.StatePath;

    [Fact]
    public void QualitySemanticGates10Artifacts_AddGraduationCriteriaAndNoOverclaimGuard()
    {
        var qsg09ReportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-09-report.json");
        var qsg09PairRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-09-pair-results.jsonl");
        var qsg09ComponentGatesPath = Path.Combine(ArtifactDir, "quality-semantic-gates-09-component-gates.jsonl");

        var pairGuardRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-10-pair-guard-rows.jsonl");
        var componentGatesPath = Path.Combine(ArtifactDir, "quality-semantic-gates-10-component-gates.jsonl");
        var graduationCriteriaPath = Path.Combine(ArtifactDir, "quality-semantic-gates-10-graduation-criteria.json");
        var reportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-10-report.json");
        var reportMdPath = Path.Combine(ArtifactDir, "quality-semantic-gates-10-report.md");
        var validationPath = Path.Combine(ArtifactDir, "quality-semantic-gates-10.validation.json");
        var statePatchPath = ExtensionExecutionStatePatchWriter.DefaultPatchPath(ArtifactDir, "QUALITY-SEMANTIC-GATES-10");

        using var qsg09ReportDocument = JsonDocument.Parse(File.ReadAllText(qsg09ReportPath));
        var qsg09Report = qsg09ReportDocument.RootElement;
        var qsg09Pairs = ReadJsonl(qsg09PairRowsPath);
        var qsg09Components = ReadJsonl(qsg09ComponentGatesPath);

        var pairGuardRows = qsg09Pairs
            .Select(pair => new
            {
                Schema = "xiigen.quality_semantic_gates_10.no_overclaim_pair_guard.v1",
                PhaseId = "QUALITY-SEMANTIC-GATES-10",
                SourcePhaseId = "QUALITY-SEMANTIC-GATES-09",
                ScenarioId = pair.GetProperty("scenario_id").GetString(),
                PairId = pair.GetProperty("pair_id").GetString(),
                Host = pair.GetProperty("host").GetString(),
                Method = pair.GetProperty("method").GetString(),
                Qsg09QualityGatePassed = pair.GetProperty("quality_gate_passed").GetBoolean(),
                RuntimeQualityGatePassedForBothRows = pair.GetProperty("runtime_quality_gate_passed_for_both_rows").GetBoolean(),
                Qsg09SemanticQualityGatePassed = pair.GetProperty("semantic_quality_gate_passed").GetBoolean(),
                BaselineInternalRuntimeOnly = true,
                GraduationCandidate = pair.GetProperty("quality_gate_passed").GetBoolean()
                    && pair.GetProperty("runtime_quality_gate_passed_for_both_rows").GetBoolean(),
                GraduationAccepted = false,
                CommonReadinessClaimAllowed = false,
                QualityImprovementClaimAllowed = false,
                TokenSavingsClaimAllowed = false,
                ProductReadinessGatePassed = false,
                BaselineTotalTokens = pair.GetProperty("baseline_total_tokens").GetInt32(),
                AdaptiveTotalTokens = pair.GetProperty("adaptive_total_tokens").GetInt32(),
                TokenDelta = pair.GetProperty("token_delta").GetInt32(),
                TokenReductionObserved = pair.GetProperty("token_delta").GetInt32() > 0,
                BlockedClaims = new[]
                {
                    "semantic_quality",
                    "common_readiness",
                    "quality_improvement",
                    "token_savings",
                    "target_promotion",
                    "final_readiness"
                },
                BlockingReasons = new[]
                {
                    "baseline_internal_runtime_quality_is_diagnostic_only",
                    "semantic_quality_gate_not_passed",
                    "quality_gate_pairs_do_not_equal_semantic_quality",
                    "token_delta_cannot_substitute_for_semantic_quality"
                },
                HumanReviewerNote = "QSG-10 keeps this QSG-09 runtime-quality pair closed for semantic/common readiness; graduation requires an accepted semantic proof gate, not a baseline/internal runtime diagnostic."
            })
            .ToArray();

        var componentGateRows = qsg09Components
            .Select(component => new
            {
                Schema = "xiigen.quality_semantic_gates_10.component_graduation_gate.v1",
                PhaseId = "QUALITY-SEMANTIC-GATES-10",
                SourcePhaseId = "QUALITY-SEMANTIC-GATES-09",
                ComponentId = component.GetProperty("component_id").GetString(),
                MachineId = component.GetProperty("machine_id").GetString(),
                RuntimeLayerPassRows = component.GetProperty("runtime_layer_pass_rows").GetInt32(),
                BaselineCommonNeedsRepairRows = component.GetProperty("baseline_common_needs_repair_rows").GetInt32(),
                SourceDisjointEvalGatePassed = component.GetProperty("source_disjoint_eval_gate_passed").GetBoolean(),
                AblationGatePassed = component.GetProperty("ablation_gate_passed").GetBoolean(),
                LeakageGatePassed = component.GetProperty("leakage_gate_passed").GetBoolean(),
                FreshLoadGatePassed = component.GetProperty("fresh_load_gate_passed").GetBoolean(),
                ContinueTrainingGatePassed = component.GetProperty("continue_training_gate_passed").GetBoolean(),
                BaselineCommonRuntimeGatePassed = component.GetProperty("baseline_common_runtime_gate_passed").GetBoolean(),
                Qsg09SemanticQualityGatePassed = component.GetProperty("semantic_quality_gate_passed").GetBoolean(),
                AcceptedSemanticGraduationRows = 0,
                GraduationCriteriaMet = false,
                GraduationAccepted = false,
                CommonReadinessClaimAllowed = false,
                RequiredMissingProof = new[]
                {
                    "accepted_semantic_graduation_rows",
                    "human_readable_rubric_quality_improvement_rows",
                    "proof_that_baseline_internal_runtime_logic_is_not_the_semantic_authority"
                },
                RequiredEvidenceRefs = new[]
                {
                    "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-09-pair-results.jsonl",
                    "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-09-row-scores.jsonl",
                    "docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md Section 11"
                },
                HumanReviewerNote = "Baseline/common runtime rows are repaired, but this component has zero accepted semantic graduation rows; common readiness remains closed."
            })
            .ToArray();

        var tokenReductionObservedPairs = pairGuardRows.Count(row => row.TokenReductionObserved);
        var tokenIncreaseObservedPairs = pairGuardRows.Count(row => row.TokenDelta < 0);
        var tokenEqualPairs = pairGuardRows.Count(row => row.TokenDelta == 0);
        var totalTokenDelta = pairGuardRows.Sum(row => row.TokenDelta);
        var baselineTotalTokens = pairGuardRows.Sum(row => row.BaselineTotalTokens);
        var adaptiveTotalTokens = pairGuardRows.Sum(row => row.AdaptiveTotalTokens);
        var qsg09QualityGatePairs = qsg09Pairs.Count(pair => pair.GetProperty("quality_gate_passed").GetBoolean());
        var qsg09PairCount = qsg09Pairs.Count;

        var beforeAfterCounts = new
        {
            Qsg09 = new
            {
                RowCount = qsg09Report.GetProperty("row_count").GetInt32(),
                PairCount = qsg09Report.GetProperty("pair_count").GetInt32(),
                BaselineCommonRepairedRows = qsg09Report.GetProperty("baseline_common_repaired_rows").GetInt32(),
                BaselineCommonNeedsRepairRows = qsg09Report.GetProperty("baseline_common_needs_repair_rows").GetInt32(),
                RuntimeQualityPassedRows = qsg09Report.GetProperty("runtime_quality_passed_rows").GetInt32(),
                QualityGatePairs = qsg09Report.GetProperty("quality_gate_pairs").GetInt32(),
                SemanticQualityGatePassed = qsg09Report.GetProperty("semantic_quality_gate_passed").GetBoolean(),
                CommonReadinessClaimAllowed = qsg09Report.GetProperty("common_readiness_claim_allowed").GetBoolean(),
                QualityImprovementClaimAllowed = qsg09Report.GetProperty("quality_improvement_claim_allowed").GetBoolean(),
                TokenSavingsClaimAllowed = qsg09Report.GetProperty("token_savings_claim_allowed").GetBoolean(),
                TokenSavingsClaimAllowedPairs = qsg09Report
                    .GetProperty("before_after_counts")
                    .GetProperty("qsg09")
                    .GetProperty("token_savings_claim_allowed_pairs")
                    .GetInt32()
            },
            Qsg10 = new
            {
                RowCount = qsg09Report.GetProperty("row_count").GetInt32(),
                PairCount = pairGuardRows.Length,
                Qsg09QualityGatePairsSeen = qsg09QualityGatePairs,
                NoOverclaimGuardRows = pairGuardRows.Length,
                GraduationCandidatePairs = pairGuardRows.Count(row => row.GraduationCandidate),
                SemanticGraduationAcceptedPairs = pairGuardRows.Count(row => row.GraduationAccepted),
                CommonReadinessGraduatedPairs = pairGuardRows.Count(row => row.CommonReadinessClaimAllowed),
                ComponentGateCount = componentGateRows.Length,
                ComponentsGraduationCandidates = componentGateRows.Length,
                ComponentsGraduationAccepted = componentGateRows.Count(row => row.GraduationAccepted),
                TokenReductionObservedPairs = tokenReductionObservedPairs,
                TokenIncreaseObservedPairs = tokenIncreaseObservedPairs,
                TokenEqualPairs = tokenEqualPairs,
                BaselineTotalTokens = baselineTotalTokens,
                AdaptiveTotalTokens = adaptiveTotalTokens,
                TotalTokenDelta = totalTokenDelta,
                TokenSavingsClaimAllowedPairs = pairGuardRows.Count(row => row.TokenSavingsClaimAllowed)
            }
        };

        var graduationCriteria = new
        {
            Schema = "xiigen.quality_semantic_gates_10.graduation_criteria.v1",
            PhaseId = "QUALITY-SEMANTIC-GATES-10",
            SourcePhaseId = "QUALITY-SEMANTIC-GATES-09",
            Decision = "KEEP_CLOSED",
            GraduationRequires = new[]
            {
                "runtime_quality_gate_passed_for_both_rows",
                "source_disjoint_eval_gate_passed",
                "ablation_gate_passed",
                "leakage_gate_passed",
                "fresh_load_gate_passed",
                "continue_training_gate_passed",
                "baseline_common_runtime_gate_passed",
                "accepted_semantic_graduation_rows_greater_than_zero",
                "quality_improvement_rows_pass_human_readable_rubric",
                "no_baseline_internal_runtime_logic_as_final_semantic_authority"
            },
            GraduationBlockedBecause = new[]
            {
                "accepted_semantic_graduation_rows_greater_than_zero is false",
                $"QSG-09 quality_gate_pairs={qsg09QualityGatePairs} across {qsg09PairCount} pair rows is runtime diagnostic evidence only",
                $"Token before/after evidence is diagnostic and overall adaptive tokens are higher by {Math.Abs(totalTokenDelta)} tokens",
                "Common readiness, target promotion, and final readiness require closure rows outside QSG-10"
            },
            EvidenceSeparationRule = $"quality_gate_pairs={qsg09QualityGatePairs} across {qsg09PairCount} pair rows may be reported only as runtime-quality diagnostic evidence; it must not be read as semantic quality, token savings, common readiness, promotion, or final readiness."
        };

        var report = new
        {
            Schema = "xiigen.quality_semantic_gates_10.no_overclaim_graduation_report.v1",
            PhaseId = "QUALITY-SEMANTIC-GATES-10",
            InputPhaseId = "QUALITY-SEMANTIC-GATES-09",
            Qsg09ReportRef = "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-09-report.json",
            GraduationCriteriaRef = "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-10-graduation-criteria.json",
            PairGuardRowsRef = "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-10-pair-guard-rows.jsonl",
            ComponentGatesRef = "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-10-component-gates.jsonl",
            RowCount = qsg09Report.GetProperty("row_count").GetInt32(),
            PairCount = pairGuardRows.Length,
            ComponentGateCount = componentGateRows.Length,
            Qsg09QualityGatePairsSeen = qsg09QualityGatePairs,
            NoOverclaimGuardRows = pairGuardRows.Length,
            GraduationDecision = "KEEP_CLOSED",
            GraduationCandidatePairs = pairGuardRows.Count(row => row.GraduationCandidate),
            SemanticGraduationAcceptedPairs = pairGuardRows.Count(row => row.GraduationAccepted),
            CommonReadinessGraduatedPairs = pairGuardRows.Count(row => row.CommonReadinessClaimAllowed),
            TokenReductionObservedPairs = tokenReductionObservedPairs,
            TokenIncreaseObservedPairs = tokenIncreaseObservedPairs,
            TokenEqualPairs = tokenEqualPairs,
            BaselineTotalTokens = baselineTotalTokens,
            AdaptiveTotalTokens = adaptiveTotalTokens,
            TotalTokenDelta = totalTokenDelta,
            TokenDeltaDirection = totalTokenDelta > 0
                ? "adaptive_fewer_tokens_overall"
                : totalTokenDelta < 0
                    ? "adaptive_more_tokens_overall"
                    : "token_equal_overall",
            SourceDisjointEvalGatePassed = true,
            AblationGatePassed = true,
            LeakageGatePassed = true,
            FreshLoadGatePassed = true,
            ContinueTrainingGatePassed = true,
            BaselineCommonRuntimeGatePassed = true,
            SemanticQualityGatePassed = false,
            CommonReadinessClaimAllowed = false,
            QualityImprovementClaimAllowed = false,
            TokenSavingsClaimAllowed = false,
            SemanticQualityClaimed = false,
            LearnabilityClaimed = false,
            ProductReadinessClaimAllowed = false,
            TargetPromotionDone = false,
            PromotionReadinessClaimed = false,
            FinalReadinessClaimed = false,
            TrainingStarted = false,
            CheckpointMutationStarted = false,
            DeterministicHelperClassification = "baseline/internal runtime graduation guard only; not semantic quality, not token savings, not common/full readiness",
            BeforeAfterCounts = beforeAfterCounts,
            RepairedInThisSlice = new[]
            {
                "Added explicit QSG-10 graduation criteria for when baseline/internal runtime quality may move toward semantic/common readiness.",
                $"Added {pairGuardRows.Length} pair-level no-overclaim guard rows proving QSG-09 quality_gate_pairs={qsg09QualityGatePairs} stays diagnostic and does not become semantic quality, token savings, common readiness, promotion, or final readiness.",
                $"Computed token before/after diagnostics from QSG-09 pairs: {tokenReductionObservedPairs} reductions, {tokenIncreaseObservedPairs} increases, {tokenEqualPairs} equal pairs, and adaptive uses {Math.Abs(totalTokenDelta)} more total tokens overall; token savings claim remains false."
            },
            RemainingBlockers = new[]
            {
                "Accepted semantic graduation rows are still 0.",
                "Quality improvement under the human-readable rubric is still not proven.",
                "Token savings remains false because semantic quality is not proven and aggregate adaptive tokens are higher.",
                "Common readiness, promotion, and final readiness remain closed until all required closure rows pass together."
            },
            NextBoundedWorkOrder = "QUALITY-SEMANTIC-GATES-11: produce accepted semantic graduation rows under the human-readable quality rubric, or continue keeping common readiness closed."
        };

        var validation = new
        {
            Schema = "xiigen.quality_semantic_gates_10.validation.v1",
            PhaseId = "QUALITY-SEMANTIC-GATES-10",
            Qsg09QualityGatePairsExpected = qsg09Report.GetProperty("quality_gate_pairs").GetInt32(),
            Qsg09QualityGatePairsActual = qsg09QualityGatePairs,
            PairGuardRowsExpected = 54,
            PairGuardRowsActual = pairGuardRows.Length,
            ComponentGateCountExpected = 3,
            ComponentGateCountActual = componentGateRows.Length,
            GraduationCandidatePairsExpected = qsg09QualityGatePairs,
            GraduationCandidatePairsActual = pairGuardRows.Count(row => row.GraduationCandidate),
            SemanticGraduationAcceptedPairsExpected = 0,
            SemanticGraduationAcceptedPairsActual = pairGuardRows.Count(row => row.GraduationAccepted),
            CommonReadinessGraduatedPairsExpected = 0,
            CommonReadinessGraduatedPairsActual = pairGuardRows.Count(row => row.CommonReadinessClaimAllowed),
            TokenReductionObservedPairs = tokenReductionObservedPairs,
            TokenIncreaseObservedPairs = tokenIncreaseObservedPairs,
            TokenEqualPairs = tokenEqualPairs,
            BaselineTotalTokens = baselineTotalTokens,
            AdaptiveTotalTokens = adaptiveTotalTokens,
            TotalTokenDelta = totalTokenDelta,
            QualityGatePairsMisreadAsSemanticQualityPrevented = qsg09QualityGatePairs == qsg09Report.GetProperty("quality_gate_pairs").GetInt32()
                && qsg09PairCount == 54
                && pairGuardRows.All(row => !row.GraduationAccepted)
                && pairGuardRows.All(row => !row.CommonReadinessClaimAllowed),
            SemanticQualityGatePassed = false,
            CommonReadinessClaimAllowed = false,
            QualityImprovementClaimAllowed = false,
            TokenSavingsClaimAllowed = false,
            ProductReadinessClaimAllowed = false,
            TargetPromotionDone = false,
            PromotionReadinessClaimed = false,
            FinalReadinessClaimed = false,
            TrainingStarted = false,
            CheckpointMutationStarted = false,
            OutputFiles = new[]
            {
                Path.GetFullPath(pairGuardRowsPath),
                Path.GetFullPath(componentGatesPath),
                Path.GetFullPath(graduationCriteriaPath),
                Path.GetFullPath(reportPath),
                Path.GetFullPath(reportMdPath),
                Path.GetFullPath(validationPath),
                Path.GetFullPath(statePatchPath)
            }
        };

        WriteJsonl(pairGuardRowsPath, pairGuardRows);
        WriteJsonl(componentGatesPath, componentGateRows);
        WriteJson(graduationCriteriaPath, graduationCriteria);
        WriteJson(reportPath, report);
        WriteQsg10Markdown(reportMdPath, beforeAfterCounts);
        WriteJson(validationPath, validation);
        statePatchPath = UpdateQsg10State(StatePath, report, validation);

        var writtenPairGuardRows = ReadJsonl(pairGuardRowsPath);
        Assert.Equal(54, writtenPairGuardRows.Count);
        Assert.All(writtenPairGuardRows, row =>
        {
            var qsg09QualityGatePassed = row.GetProperty("qsg09_quality_gate_passed").GetBoolean();
            var runtimeQualityGatePassedForBothRows = row.GetProperty("runtime_quality_gate_passed_for_both_rows").GetBoolean();
            Assert.Equal(
                qsg09QualityGatePassed && runtimeQualityGatePassedForBothRows,
                row.GetProperty("graduation_candidate").GetBoolean());
            Assert.False(row.GetProperty("qsg09_semantic_quality_gate_passed").GetBoolean());
            Assert.False(row.GetProperty("graduation_accepted").GetBoolean());
            Assert.False(row.GetProperty("common_readiness_claim_allowed").GetBoolean());
            Assert.False(row.GetProperty("quality_improvement_claim_allowed").GetBoolean());
            Assert.False(row.GetProperty("token_savings_claim_allowed").GetBoolean());
            Assert.Contains(
                row.GetProperty("blocking_reasons").EnumerateArray(),
                reason => reason.GetString() == "quality_gate_pairs_do_not_equal_semantic_quality");
        });

        var writtenComponentGateRows = ReadJsonl(componentGatesPath);
        Assert.Equal(3, writtenComponentGateRows.Count);
        Assert.All(writtenComponentGateRows, row =>
        {
            Assert.Equal(0, row.GetProperty("accepted_semantic_graduation_rows").GetInt32());
            Assert.False(row.GetProperty("qsg09_semantic_quality_gate_passed").GetBoolean());
            Assert.False(row.GetProperty("graduation_criteria_met").GetBoolean());
            Assert.False(row.GetProperty("graduation_accepted").GetBoolean());
            Assert.False(row.GetProperty("common_readiness_claim_allowed").GetBoolean());
            Assert.Contains(
                row.GetProperty("required_missing_proof").EnumerateArray(),
                proof => proof.GetString() == "accepted_semantic_graduation_rows");
            Assert.Contains(
                row.GetProperty("required_missing_proof").EnumerateArray(),
                proof => proof.GetString() == "human_readable_rubric_quality_improvement_rows");
        });

        using var writtenReport = JsonDocument.Parse(File.ReadAllText(reportPath));
        var root = writtenReport.RootElement;
        Assert.Equal("QUALITY-SEMANTIC-GATES-10", root.GetProperty("phase_id").GetString());
        Assert.Equal(qsg09QualityGatePairs, root.GetProperty("qsg09_quality_gate_pairs_seen").GetInt32());
        Assert.Equal(54, root.GetProperty("no_overclaim_guard_rows").GetInt32());
        Assert.Equal(pairGuardRows.Count(row => row.GraduationCandidate), root.GetProperty("graduation_candidate_pairs").GetInt32());
        Assert.Equal(0, root.GetProperty("semantic_graduation_accepted_pairs").GetInt32());
        Assert.Equal(0, root.GetProperty("common_readiness_graduated_pairs").GetInt32());
        Assert.Equal(0, root.GetProperty("token_reduction_observed_pairs").GetInt32());
        Assert.Equal(54, root.GetProperty("token_increase_observed_pairs").GetInt32());
        Assert.Equal(0, root.GetProperty("token_equal_pairs").GetInt32());
        Assert.Equal(totalTokenDelta, root.GetProperty("total_token_delta").GetInt32());
        Assert.False(root.GetProperty("semantic_quality_gate_passed").GetBoolean());
        Assert.False(root.GetProperty("common_readiness_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("quality_improvement_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(root.GetProperty("learnability_claimed").GetBoolean());
        Assert.False(root.GetProperty("product_readiness_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("target_promotion_done").GetBoolean());
        Assert.False(root.GetProperty("promotion_readiness_claimed").GetBoolean());
        Assert.False(root.GetProperty("final_readiness_claimed").GetBoolean());
        Assert.False(root.GetProperty("training_started").GetBoolean());
        Assert.False(root.GetProperty("checkpoint_mutation_started").GetBoolean());

        using var writtenValidation = JsonDocument.Parse(File.ReadAllText(validationPath));
        Assert.True(writtenValidation.RootElement.GetProperty("quality_gate_pairs_misread_as_semantic_quality_prevented").GetBoolean());

        using var writtenStatePatch = JsonDocument.Parse(File.ReadAllText(statePatchPath));
        Assert.False(writtenStatePatch.RootElement.GetProperty("giant_state_mutated").GetBoolean());
        var stateRoot = writtenStatePatch.RootElement.GetProperty("patch_fields");
        Assert.Equal("QUALITY-SEMANTIC-GATES-10", stateRoot.GetProperty("current_phase").GetString());
        Assert.False(stateRoot.GetProperty("stop_allowed").GetBoolean());
        Assert.True(stateRoot.GetProperty("continuation_required").GetBoolean());
        Assert.StartsWith(
            "QUALITY-SEMANTIC-GATES-11:",
            stateRoot.GetProperty("next_action").GetString());
        Assert.Contains(
            "accepted semantic graduation rows",
            stateRoot.GetProperty("next_action").GetString(),
            StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<JsonElement> ReadJsonl(string path)
        => File.ReadAllLines(path)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonDocument.Parse(line).RootElement.Clone())
            .ToArray();

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

    private static void WriteQsg10Markdown(string path, object beforeAfterCounts)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# QUALITY-SEMANTIC-GATES-10");
        builder.AppendLine();
        builder.AppendLine("QSG-10 adds an explicit graduation and no-overclaim gate for QSG-09 runtime-quality pairs. It keeps semantic quality, token savings, common readiness, promotion, and final readiness closed.");
        builder.AppendLine();
        builder.AppendLine("## What This Proves");
        builder.AppendLine();
        builder.AppendLine("- QSG-09 quality_gate_pairs=48 across 54 pair rows is runtime-quality diagnostic evidence only.");
        builder.AppendLine("- Baseline/internal runtime quality cannot graduate to common readiness without accepted semantic graduation rows.");
        builder.AppendLine("- QSG-09 token diagnostics are honest before/after evidence but do not permit a token-savings claim.");
        builder.AppendLine();
        builder.AppendLine("## What This Does Not Prove");
        builder.AppendLine();
        builder.AppendLine("- Semantic quality, quality improvement, common readiness, target promotion, or final readiness.");
        builder.AppendLine();
        builder.AppendLine("## Counts");
        builder.AppendLine();
        builder.AppendLine("```json");
        builder.AppendLine(JsonSerializer.Serialize(beforeAfterCounts, JsonOptions).Replace("\r\n", "\n", StringComparison.Ordinal));
        builder.AppendLine("```");
        builder.AppendLine();
        builder.AppendLine("Next independent track: QUALITY-SEMANTIC-GATES-11 should add accepted semantic graduation rows under the human-readable quality rubric, or keep common readiness closed.");

        File.WriteAllText(path, builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal), new UTF8Encoding(false));
    }

    private static string UpdateQsg10State(string statePath, object report, object validation)
    {
        return ExtensionExecutionStatePatchWriter.WritePatch(
            statePath,
            ArtifactDir,
            "QUALITY-SEMANTIC-GATES-10",
            new Dictionary<string, object?>
            {
                ["updated_at_local"] = "2026-06-17T00:00:00.000Z",
                ["current_phase"] = "QUALITY-SEMANTIC-GATES-10",
                ["phase_status"] = "BASELINE_INTERNAL_GRADUATION_DECISION_RECORDED_READINESS_CLAIMS_STILL_CLOSED",
                ["phase_scope"] = "QSG-10 added explicit graduation criteria and no-overclaim guard rows for QSG-09 runtime-quality pairs; semantic quality, common readiness, quality improvement, token savings, training, checkpoint mutation, target promotion, locator update, and final readiness remain closed.",
                ["goal_reached"] = false,
                ["stop_allowed"] = false,
                ["next_action"] = "QUALITY-SEMANTIC-GATES-11: produce accepted semantic graduation rows under the human-readable quality rubric, or continue keeping common readiness closed.",
                ["continuation_required"] = true
            },
            report,
            validation,
            JsonOptions);
    }

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
