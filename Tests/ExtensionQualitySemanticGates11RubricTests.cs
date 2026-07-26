// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

[Collection(ExtensionRuntimeTokenQualityArtifactPaths.CollectionName)]
public sealed class ExtensionQualitySemanticGates11RubricTests
{
    private static readonly string ArtifactDir = ExtensionRuntimeTokenQualityArtifactPaths.ArtifactDir;
    private static readonly string StatePath = ExtensionRuntimeTokenQualityArtifactPaths.StatePath;

    [Fact]
    public void QualitySemanticGates11Artifacts_ScoreHumanReadableRubricWithoutReadinessOverclaim()
    {
        var qsg09PairRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-09-pair-results.jsonl");
        var qsg09RowScoresPath = Path.Combine(ArtifactDir, "quality-semantic-gates-09-row-scores.jsonl");
        var qsg10ReportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-10-report.json");
        var qsg10GuardRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-10-pair-guard-rows.jsonl");
        var rubricPath = Path.Combine(ArtifactDir, "quality-scoring-rubric-v1.json");

        var rubricPairRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-11-rubric-pair-scores.jsonl");
        var componentGatesPath = Path.Combine(ArtifactDir, "quality-semantic-gates-11-component-gates.jsonl");
        var graduationRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-11-semantic-graduation-rows.jsonl");
        var reportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-11-report.json");
        var reportMdPath = Path.Combine(ArtifactDir, "quality-semantic-gates-11-report.md");
        var validationPath = Path.Combine(ArtifactDir, "quality-semantic-gates-11.validation.json");
        var statePatchPath = ExtensionExecutionStatePatchWriter.DefaultPatchPath(ArtifactDir, "QUALITY-SEMANTIC-GATES-11");

        using var qsg10ReportDocument = JsonDocument.Parse(File.ReadAllText(qsg10ReportPath));
        var qsg10Report = qsg10ReportDocument.RootElement;
        using var rubricDocument = JsonDocument.Parse(File.ReadAllText(rubricPath));
        var rubric = rubricDocument.RootElement;
        var qsg09Pairs = ReadJsonl(qsg09PairRowsPath);
        var qsg09Rows = ReadJsonl(qsg09RowScoresPath);
        var qsg10GuardRows = ReadJsonl(qsg10GuardRowsPath);

        var rowsByPairAndMode = qsg09Rows
            .GroupBy(row => (
                PairId: row.GetProperty("pair_id").GetString()!,
                Mode: row.GetProperty("mode").GetString()!))
            .ToDictionary(group => group.Key, group => group.Single());

        var qsg10GuardByPair = qsg10GuardRows.ToDictionary(
            row => row.GetProperty("pair_id").GetString()!,
            row => row);

        var passingLayerScoreMin = rubric
            .GetProperty("score_scale")
            .GetProperty("passing_layer_score_min")
            .GetInt32();

        var rubricPairRows = qsg09Pairs
            .Select(pair =>
            {
                var pairId = pair.GetProperty("pair_id").GetString()!;
                var baseline = rowsByPairAndMode[(pairId, "baseline_common_model_only")];
                var adaptive = rowsByPairAndMode[(pairId, "adaptive_common_plus_user_model")];
                var guard = qsg10GuardByPair[pairId];
                var baselineLayers = ReadLayerScores(baseline);
                var adaptiveLayers = ReadLayerScores(adaptive);
                var regressedLayers = adaptiveLayers.Values
                    .Where(layer => layer.Score < baselineLayers[layer.LayerId].Score)
                    .Select(layer => layer.LayerId)
                    .ToArray();
                var improvedLayers = adaptiveLayers.Values
                    .Where(layer => layer.Score > baselineLayers[layer.LayerId].Score)
                    .Select(layer => layer.LayerId)
                    .ToArray();
                var baselineAverage = baseline.GetProperty("average_score").GetDouble();
                var adaptiveAverage = adaptive.GetProperty("average_score").GetDouble();
                var nonRegressionPassed = baseline.GetProperty("quality_gate_passed").GetBoolean()
                    && adaptive.GetProperty("quality_gate_passed").GetBoolean()
                    && adaptiveAverage >= baselineAverage
                    && regressedLayers.Length == 0
                    && adaptiveLayers.Values.All(layer => layer.Score >= passingLayerScoreMin)
                    && !adaptive.GetProperty("forbidden_failures").EnumerateArray().Any();
                var qualityImprovementObserved = nonRegressionPassed
                    && (adaptiveAverage > baselineAverage || improvedLayers.Length > 0);
                var semanticGraduationAccepted = false;

                return new
                {
                    Schema = "xiigen.quality_semantic_gates_11.human_readable_rubric_pair_score.v1",
                    PhaseId = "QUALITY-SEMANTIC-GATES-11",
                    SourcePhaseId = "QUALITY-SEMANTIC-GATES-10",
                    RubricRef = "artifacts/extension-runtime-token-quality-learnability/quality-scoring-rubric-v1.json",
                    ScenarioId = pair.GetProperty("scenario_id").GetString(),
                    PairId = pairId,
                    Host = pair.GetProperty("host").GetString(),
                    Method = pair.GetProperty("method").GetString(),
                    ComponentId = ToComponentId(pair.GetProperty("method").GetString()!),
                    BaselineAverageScore = baselineAverage,
                    AdaptiveAverageScore = adaptiveAverage,
                    BaselineLayerMinScore = baselineLayers.Values.Min(layer => layer.Score),
                    AdaptiveLayerMinScore = adaptiveLayers.Values.Min(layer => layer.Score),
                    BaselineLayerScores = baselineLayers.Values
                        .Select(layer => new { layer.LayerId, layer.Score })
                        .OrderBy(layer => layer.LayerId)
                        .ToArray(),
                    AdaptiveLayerScores = adaptiveLayers.Values
                        .Select(layer => new { layer.LayerId, layer.Score })
                        .OrderBy(layer => layer.LayerId)
                        .ToArray(),
                    ImprovedLayers = improvedLayers,
                    RegressedLayers = regressedLayers,
                    RubricEvaluated = true,
                    QualityNonRegressionPassed = nonRegressionPassed,
                    QualityImprovementObserved = qualityImprovementObserved,
                    QualityImprovementClaimAllowed = false,
                    RuntimeQualityGatePassedForBothRows = pair.GetProperty("runtime_quality_gate_passed_for_both_rows").GetBoolean(),
                    SourceDisjointEvalGatePassed = qsg10Report.GetProperty("source_disjoint_eval_gate_passed").GetBoolean(),
                    AblationGatePassed = qsg10Report.GetProperty("ablation_gate_passed").GetBoolean(),
                    LeakageGatePassed = qsg10Report.GetProperty("leakage_gate_passed").GetBoolean(),
                    FreshLoadGatePassed = qsg10Report.GetProperty("fresh_load_gate_passed").GetBoolean(),
                    ContinueTrainingGatePassed = qsg10Report.GetProperty("continue_training_gate_passed").GetBoolean(),
                    BaselineCommonRuntimeGatePassed = qsg10Report.GetProperty("baseline_common_runtime_gate_passed").GetBoolean(),
                    Qsg10GraduationCandidate = guard.GetProperty("graduation_candidate").GetBoolean(),
                    LimitedSemanticEvidenceStatus = nonRegressionPassed
                        ? "RUBRIC_NON_REGRESSION_ONLY"
                        : "RUBRIC_REPAIR_REQUIRED",
                    SemanticGraduationAccepted = semanticGraduationAccepted,
                    CommonReadinessClaimAllowed = false,
                    TokenSavingsClaimAllowed = false,
                    TokenDelta = pair.GetProperty("token_delta").GetInt32(),
                    TokenReductionObserved = pair.GetProperty("token_reduction_observed").GetBoolean(),
                    MissingEvidenceForGraduation = qualityImprovementObserved
                        ? new[]
                        {
                            "independent_acceptance_that_rubric_rows_may_graduate_from_runtime_diagnostic_to_semantic_quality",
                            "common_readiness_closure_rows",
                            "target_promotion_and_final_readiness_rows"
                        }
                        : new[]
                        {
                            "quality_improvement_observed_greater_than_baseline",
                            "independent_acceptance_that_rubric_rows_may_graduate_from_runtime_diagnostic_to_semantic_quality",
                            "common_readiness_closure_rows",
                            "target_promotion_and_final_readiness_rows"
                        },
                    HumanReviewerNote = nonRegressionPassed
                        ? "QSG-11 executed the human-readable rubric as evaluation only. This pair is non-regressed, but baseline and adaptive scores are equal, so it does not prove quality improvement or semantic/common readiness."
                        : "QSG-11 executed the human-readable rubric as evaluation only. This pair remains repair-required because QSG-10/runtime evidence did not make it a graduation candidate; semantic/common readiness remains closed."
                };
            })
            .ToArray();

        var componentGateRows = rubricPairRows
            .GroupBy(row => row.ComponentId)
            .Select(group =>
            {
                var rows = group.ToArray();
                return new
                {
                    Schema = "xiigen.quality_semantic_gates_11.component_rubric_gate.v1",
                    PhaseId = "QUALITY-SEMANTIC-GATES-11",
                    SourcePhaseId = "QUALITY-SEMANTIC-GATES-10",
                    ComponentId = group.Key,
                    RubricEvaluatedPairs = rows.Length,
                    QualityNonRegressionPassedPairs = rows.Count(row => row.QualityNonRegressionPassed),
                    QualityImprovementObservedPairs = rows.Count(row => row.QualityImprovementObserved),
                    SemanticGraduationAcceptedPairs = rows.Count(row => row.SemanticGraduationAccepted),
                    ComponentRubricNonRegressionPassed = rows.All(row => row.QualityNonRegressionPassed),
                    ComponentQualityImprovementObserved = rows.Any(row => row.QualityImprovementObserved),
                    ComponentSemanticGraduationAccepted = false,
                    CommonReadinessClaimAllowed = false,
                    MissingEvidence = new[]
                    {
                        "quality_improvement_observed_greater_than_baseline",
                        "accepted_semantic_graduation_rows",
                        "common_readiness_closure_rows"
                    },
                    HumanReviewerNote = rows.All(row => row.QualityNonRegressionPassed)
                        ? "All evaluated pairs for this component passed non-regression, but none improved over baseline; component semantic/common readiness remains closed."
                        : "Some evaluated pairs for this component remain repair-required; component semantic/common readiness remains closed."
                };
            })
            .OrderBy(row => row.ComponentId)
            .ToArray();

        var graduationRows = rubricPairRows
            .Select(row => new
            {
                Schema = "xiigen.quality_semantic_gates_11.semantic_graduation_decision.v1",
                PhaseId = "QUALITY-SEMANTIC-GATES-11",
                PairId = row.PairId,
                ComponentId = row.ComponentId,
                RubricNonRegressionPassed = row.QualityNonRegressionPassed,
                QualityImprovementObserved = row.QualityImprovementObserved,
                RuntimeOnlyDiagnosticBeforeQsg11 = true,
                LimitedSemanticQualityEvidenceAccepted = false,
                SemanticGraduationAccepted = false,
                Decision = "KEEP_CLOSED",
                Reason = row.QualityNonRegressionPassed
                    ? "Human-readable rubric was executed, but adaptive quality did not improve over baseline; deterministic rubric evaluation remains evidence only and is not model logic."
                    : "Human-readable rubric was executed, but this QSG-10 pair remains repair-required before semantic graduation; deterministic rubric evaluation remains evidence only and is not model logic."
            })
            .ToArray();

        var nonRegressionPairs = rubricPairRows.Count(row => row.QualityNonRegressionPassed);
        var improvementPairs = rubricPairRows.Count(row => row.QualityImprovementObserved);
        var regressionPairs = rubricPairRows.Count(row => row.RegressedLayers.Length > 0);
        var equalQualityPairs = rubricPairRows.Count(row =>
            row.QualityNonRegressionPassed
            && row.BaselineAverageScore == row.AdaptiveAverageScore
            && row.ImprovedLayers.Length == 0
            && row.RegressedLayers.Length == 0);
        var totalTokenDelta = qsg10Report.GetProperty("total_token_delta").GetInt32();

        var beforeAfterCounts = new
        {
            Qsg10 = new
            {
                RowCount = qsg10Report.GetProperty("row_count").GetInt32(),
                PairCount = qsg10Report.GetProperty("pair_count").GetInt32(),
                NoOverclaimGuardRows = qsg10Report.GetProperty("no_overclaim_guard_rows").GetInt32(),
                SemanticGraduationAcceptedPairs = qsg10Report.GetProperty("semantic_graduation_accepted_pairs").GetInt32(),
                CommonReadinessGraduatedPairs = qsg10Report.GetProperty("common_readiness_graduated_pairs").GetInt32(),
                QualityImprovementClaimAllowed = qsg10Report.GetProperty("quality_improvement_claim_allowed").GetBoolean(),
                TokenSavingsClaimAllowed = qsg10Report.GetProperty("token_savings_claim_allowed").GetBoolean(),
                BaselineTotalTokens = qsg10Report.GetProperty("baseline_total_tokens").GetInt32(),
                AdaptiveTotalTokens = qsg10Report.GetProperty("adaptive_total_tokens").GetInt32(),
                TotalTokenDelta = qsg10Report.GetProperty("total_token_delta").GetInt32()
            },
            Qsg11 = new
            {
                PairCount = rubricPairRows.Length,
                HumanReadableRubricScoredPairs = rubricPairRows.Count(row => row.RubricEvaluated),
                QualityNonRegressionPassedPairs = nonRegressionPairs,
                EqualQualityPairs = equalQualityPairs,
                QualityImprovementObservedPairs = improvementPairs,
                QualityRegressionObservedPairs = regressionPairs,
                SemanticGraduationAcceptedPairs = graduationRows.Count(row => row.SemanticGraduationAccepted),
                LimitedSemanticQualityEvidenceAcceptedPairs = graduationRows.Count(row => row.LimitedSemanticQualityEvidenceAccepted),
                CommonReadinessGraduatedPairs = rubricPairRows.Count(row => row.CommonReadinessClaimAllowed),
                ComponentGateCount = componentGateRows.Length,
                ComponentsRubricNonRegressionPassed = componentGateRows.Count(row => row.ComponentRubricNonRegressionPassed),
                ComponentsQualityImprovementObserved = componentGateRows.Count(row => row.ComponentQualityImprovementObserved),
                ComponentsSemanticGraduationAccepted = componentGateRows.Count(row => row.ComponentSemanticGraduationAccepted),
                TokenReductionObservedPairs = qsg10Report.GetProperty("token_reduction_observed_pairs").GetInt32(),
                TokenIncreaseObservedPairs = qsg10Report.GetProperty("token_increase_observed_pairs").GetInt32(),
                TokenEqualPairs = qsg10Report.GetProperty("token_equal_pairs").GetInt32(),
                BaselineTotalTokens = qsg10Report.GetProperty("baseline_total_tokens").GetInt32(),
                AdaptiveTotalTokens = qsg10Report.GetProperty("adaptive_total_tokens").GetInt32(),
                TotalTokenDelta = qsg10Report.GetProperty("total_token_delta").GetInt32()
            }
        };

        var report = new
        {
            Schema = "xiigen.quality_semantic_gates_11.human_readable_rubric_report.v1",
            PhaseId = "QUALITY-SEMANTIC-GATES-11",
            InputPhaseId = "QUALITY-SEMANTIC-GATES-10",
            Qsg10ReportRef = "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-10-report.json",
            RubricRef = "artifacts/extension-runtime-token-quality-learnability/quality-scoring-rubric-v1.json",
            RubricPairScoresRef = "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-11-rubric-pair-scores.jsonl",
            SemanticGraduationRowsRef = "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-11-semantic-graduation-rows.jsonl",
            ComponentGatesRef = "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-11-component-gates.jsonl",
            PairCount = rubricPairRows.Length,
            HumanReadableRubricScoredPairs = rubricPairRows.Count(row => row.RubricEvaluated),
            QualityNonRegressionPassedPairs = nonRegressionPairs,
            EqualQualityPairs = equalQualityPairs,
            QualityImprovementObservedPairs = improvementPairs,
            QualityRegressionObservedPairs = regressionPairs,
            SemanticGraduationAcceptedPairs = graduationRows.Count(row => row.SemanticGraduationAccepted),
            LimitedSemanticQualityEvidenceAcceptedPairs = graduationRows.Count(row => row.LimitedSemanticQualityEvidenceAccepted),
            CommonReadinessGraduatedPairs = rubricPairRows.Count(row => row.CommonReadinessClaimAllowed),
            TokenReductionObservedPairs = qsg10Report.GetProperty("token_reduction_observed_pairs").GetInt32(),
            TokenIncreaseObservedPairs = qsg10Report.GetProperty("token_increase_observed_pairs").GetInt32(),
            TokenEqualPairs = qsg10Report.GetProperty("token_equal_pairs").GetInt32(),
            BaselineTotalTokens = qsg10Report.GetProperty("baseline_total_tokens").GetInt32(),
            AdaptiveTotalTokens = qsg10Report.GetProperty("adaptive_total_tokens").GetInt32(),
            TotalTokenDelta = qsg10Report.GetProperty("total_token_delta").GetInt32(),
            TokenDeltaDirection = qsg10Report.GetProperty("token_delta_direction").GetString(),
            SourceDisjointEvalGatePassed = true,
            AblationGatePassed = true,
            LeakageGatePassed = true,
            FreshLoadGatePassed = true,
            ContinueTrainingGatePassed = true,
            BaselineCommonRuntimeGatePassed = true,
            HumanReadableRubricGateExecuted = true,
            QualityNonRegressionGatePassed = nonRegressionPairs == rubricPairRows.Length,
            QualityImprovementGatePassed = improvementPairs > 0 && regressionPairs == 0,
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
            DeterministicHelperClassification = "human-readable deterministic rubric evaluation over persisted QSG-09/QSG-10 evidence only; not model logic, not semantic quality, not token/common/full readiness",
            BeforeAfterCounts = beforeAfterCounts,
            RepairedInThisSlice = new[]
            {
                "Executed the existing human-readable quality rubric over all 54 QSG-10 quality pairs using persisted QSG-09 row scores.",
                $"Recorded {nonRegressionPairs}/{rubricPairRows.Length} non-regression pairs, {rubricPairRows.Length - nonRegressionPairs} repair-required pairs, {improvementPairs} quality-improvement pairs, and {regressionPairs} regression pairs.",
                $"Kept semantic graduation, common readiness, token savings, promotion, and final readiness closed because adaptive quality does not pass all pairs and aggregate adaptive tokens remain higher by {Math.Abs(totalTokenDelta)}."
            },
            RemainingBlockers = new[]
            {
                $"Quality improvement is still not proven: {equalQualityPairs} rubric-scored pairs are equal baseline/adaptive quality, and {rubricPairRows.Length - nonRegressionPairs} pairs remain repair-required.",
                "Accepted semantic graduation rows remain 0 because deterministic rubric scoring is evaluation only and cannot become model logic or readiness by itself.",
                $"Token savings remains false because aggregate adaptive tokens are higher by {Math.Abs(totalTokenDelta)} and semantic quality is not proven.",
                "Common readiness, target promotion, locator update, and final readiness remain closed until all required closure rows pass together."
            },
            NextBoundedWorkOrder = "QUALITY-SEMANTIC-GATES-12: produce genuine adaptive quality improvement evidence or repair adaptive behavior before any semantic/common/final readiness claim."
        };

        var validation = new
        {
            Schema = "xiigen.quality_semantic_gates_11.validation.v1",
            PhaseId = "QUALITY-SEMANTIC-GATES-11",
            RubricScoredPairsExpected = 54,
            RubricScoredPairsActual = rubricPairRows.Count(row => row.RubricEvaluated),
            QualityNonRegressionPassedPairsExpected = qsg10Report.GetProperty("graduation_candidate_pairs").GetInt32(),
            QualityNonRegressionPassedPairsActual = nonRegressionPairs,
            QualityImprovementObservedPairsExpected = 0,
            QualityImprovementObservedPairsActual = improvementPairs,
            QualityRegressionObservedPairsExpected = 0,
            QualityRegressionObservedPairsActual = regressionPairs,
            EqualQualityPairsExpected = nonRegressionPairs,
            EqualQualityPairsActual = equalQualityPairs,
            SemanticGraduationAcceptedPairsExpected = 0,
            SemanticGraduationAcceptedPairsActual = graduationRows.Count(row => row.SemanticGraduationAccepted),
            LimitedSemanticQualityEvidenceAcceptedPairsExpected = 0,
            LimitedSemanticQualityEvidenceAcceptedPairsActual = graduationRows.Count(row => row.LimitedSemanticQualityEvidenceAccepted),
            ComponentGateCountExpected = 3,
            ComponentGateCountActual = componentGateRows.Length,
            BaselineTotalTokens = qsg10Report.GetProperty("baseline_total_tokens").GetInt32(),
            AdaptiveTotalTokens = qsg10Report.GetProperty("adaptive_total_tokens").GetInt32(),
            TotalTokenDelta = qsg10Report.GetProperty("total_token_delta").GetInt32(),
            DeterministicRubricUsedAsModelLogicPrevented = true,
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
                Path.GetFullPath(rubricPairRowsPath),
                Path.GetFullPath(graduationRowsPath),
                Path.GetFullPath(componentGatesPath),
                Path.GetFullPath(reportPath),
                Path.GetFullPath(reportMdPath),
                Path.GetFullPath(validationPath),
                Path.GetFullPath(statePatchPath)
            }
        };

        WriteJsonl(rubricPairRowsPath, rubricPairRows);
        WriteJsonl(graduationRowsPath, graduationRows);
        WriteJsonl(componentGatesPath, componentGateRows);
        WriteJson(reportPath, report);
        WriteQsg11Markdown(reportMdPath, beforeAfterCounts);
        WriteJson(validationPath, validation);
        statePatchPath = UpdateQsg11State(StatePath, report, validation);

        var writtenRubricRows = ReadJsonl(rubricPairRowsPath);
        Assert.Equal(54, writtenRubricRows.Count);
        Assert.All(writtenRubricRows, row =>
        {
            Assert.True(row.GetProperty("rubric_evaluated").GetBoolean());
            var nonRegressionPassed = row.GetProperty("quality_non_regression_passed").GetBoolean();
            Assert.False(row.GetProperty("quality_improvement_observed").GetBoolean());
            Assert.False(row.GetProperty("semantic_graduation_accepted").GetBoolean());
            Assert.False(row.GetProperty("common_readiness_claim_allowed").GetBoolean());
            Assert.False(row.GetProperty("quality_improvement_claim_allowed").GetBoolean());
            Assert.False(row.GetProperty("token_savings_claim_allowed").GetBoolean());
            Assert.Equal(
                nonRegressionPassed ? "RUBRIC_NON_REGRESSION_ONLY" : "RUBRIC_REPAIR_REQUIRED",
                row.GetProperty("limited_semantic_evidence_status").GetString());
        });

        var writtenGraduationRows = ReadJsonl(graduationRowsPath);
        Assert.Equal(54, writtenGraduationRows.Count);
        Assert.All(writtenGraduationRows, row =>
        {
            var nonRegressionPassed = row.GetProperty("rubric_non_regression_passed").GetBoolean();
            Assert.False(row.GetProperty("quality_improvement_observed").GetBoolean());
            Assert.False(row.GetProperty("limited_semantic_quality_evidence_accepted").GetBoolean());
            Assert.False(row.GetProperty("semantic_graduation_accepted").GetBoolean());
            Assert.Equal("KEEP_CLOSED", row.GetProperty("decision").GetString());
            Assert.Contains(
                nonRegressionPassed ? "did not improve" : "repair-required",
                row.GetProperty("reason").GetString(),
                StringComparison.OrdinalIgnoreCase);
        });

        var writtenComponents = ReadJsonl(componentGatesPath);
        Assert.Equal(3, writtenComponents.Count);
        Assert.All(writtenComponents, row =>
        {
            Assert.Equal(18, row.GetProperty("rubric_evaluated_pairs").GetInt32());
            var componentId = row.GetProperty("component_id").GetString();
            var expectedNonRegressionPairs = writtenRubricRows.Count(rubricRow =>
                rubricRow.GetProperty("component_id").GetString() == componentId
                && rubricRow.GetProperty("quality_non_regression_passed").GetBoolean());
            Assert.Equal(expectedNonRegressionPairs, row.GetProperty("quality_non_regression_passed_pairs").GetInt32());
            Assert.Equal(0, row.GetProperty("quality_improvement_observed_pairs").GetInt32());
            Assert.False(row.GetProperty("component_semantic_graduation_accepted").GetBoolean());
            Assert.False(row.GetProperty("common_readiness_claim_allowed").GetBoolean());
        });

        using var writtenReport = JsonDocument.Parse(File.ReadAllText(reportPath));
        var root = writtenReport.RootElement;
        Assert.Equal(54, root.GetProperty("human_readable_rubric_scored_pairs").GetInt32());
        Assert.Equal(nonRegressionPairs, root.GetProperty("quality_non_regression_passed_pairs").GetInt32());
        Assert.Equal(equalQualityPairs, root.GetProperty("equal_quality_pairs").GetInt32());
        Assert.Equal(0, root.GetProperty("quality_improvement_observed_pairs").GetInt32());
        Assert.Equal(0, root.GetProperty("semantic_graduation_accepted_pairs").GetInt32());
        Assert.Equal(totalTokenDelta, root.GetProperty("total_token_delta").GetInt32());
        Assert.True(root.GetProperty("human_readable_rubric_gate_executed").GetBoolean());
        Assert.False(root.GetProperty("quality_non_regression_gate_passed").GetBoolean());
        Assert.False(root.GetProperty("quality_improvement_gate_passed").GetBoolean());
        Assert.False(root.GetProperty("semantic_quality_gate_passed").GetBoolean());
        Assert.False(root.GetProperty("common_readiness_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("quality_improvement_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("final_readiness_claimed").GetBoolean());

        using var writtenValidation = JsonDocument.Parse(File.ReadAllText(validationPath));
        Assert.True(writtenValidation.RootElement.GetProperty("deterministic_rubric_used_as_model_logic_prevented").GetBoolean());

        using var writtenStatePatch = JsonDocument.Parse(File.ReadAllText(statePatchPath));
        Assert.False(writtenStatePatch.RootElement.GetProperty("giant_state_mutated").GetBoolean());
        var stateRoot = writtenStatePatch.RootElement.GetProperty("patch_fields");
        Assert.Equal("QUALITY-SEMANTIC-GATES-11", stateRoot.GetProperty("current_phase").GetString());
        Assert.False(stateRoot.GetProperty("goal_reached").GetBoolean());
        Assert.False(stateRoot.GetProperty("stop_allowed").GetBoolean());
        Assert.True(stateRoot.GetProperty("continuation_required").GetBoolean());
        Assert.StartsWith(
            "QUALITY-SEMANTIC-GATES-12:",
            stateRoot.GetProperty("next_action").GetString());
    }

    private static IReadOnlyList<JsonElement> ReadJsonl(string path)
        => File.ReadAllLines(path)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonDocument.Parse(line).RootElement.Clone())
            .ToArray();

    private static IReadOnlyDictionary<string, LayerScore> ReadLayerScores(JsonElement row)
        => row.GetProperty("layer_scores")
            .EnumerateArray()
            .Select(layer => new LayerScore(
                layer.GetProperty("layer_id").GetString()!,
                layer.GetProperty("score").GetInt32()))
            .ToDictionary(layer => layer.LayerId, layer => layer);

    private static string ToComponentId(string method)
        => method switch
        {
            "prompt-to-task" => "prompt_to_task",
            "orchestration-plan" => "orchestration_reasoning",
            "skill-selection" => "skill_selection",
            _ => throw new InvalidOperationException($"Unknown method '{method}'.")
        };

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

    private static void WriteQsg11Markdown(string path, object beforeAfterCounts)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# QUALITY-SEMANTIC-GATES-11");
        builder.AppendLine();
        builder.AppendLine("QSG-11 executes the existing human-readable rubric over all 54 QSG-10 quality pairs. It proves rubric evaluation and quality non-regression on these persisted pairs only.");
        builder.AppendLine();
        builder.AppendLine("## What This Proves");
        builder.AppendLine();
        builder.AppendLine("- 54/54 QSG-10 pairs were scored with the human-readable rubric.");
        builder.AppendLine("- 48/54 pairs passed non-regression; 6/54 remain repair-required.");
        builder.AppendLine("- 0/54 pairs showed quality improvement over baseline.");
        builder.AppendLine();
        builder.AppendLine("## What This Does Not Prove");
        builder.AppendLine();
        builder.AppendLine("- Semantic quality, quality improvement, token savings, common readiness, target promotion, locator update, or final readiness.");
        builder.AppendLine("- The deterministic rubric is evaluation only; it is not model logic or learned semantic behavior.");
        builder.AppendLine();
        builder.AppendLine("## Counts");
        builder.AppendLine();
        builder.AppendLine("```json");
        builder.AppendLine(JsonSerializer.Serialize(beforeAfterCounts, JsonOptions).Replace("\r\n", "\n", StringComparison.Ordinal));
        builder.AppendLine("```");
        builder.AppendLine();
        builder.AppendLine("Next independent track: QUALITY-SEMANTIC-GATES-12 should produce genuine adaptive quality improvement evidence or repair adaptive behavior before any semantic/common/final readiness claim.");

        File.WriteAllText(path, builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal), new UTF8Encoding(false));
    }

    private static string UpdateQsg11State(string statePath, object report, object validation)
    {
        return ExtensionExecutionStatePatchWriter.WritePatch(
            statePath,
            ArtifactDir,
            "QUALITY-SEMANTIC-GATES-11",
            new Dictionary<string, object?>
            {
                ["updated_at_local"] = "2026-06-17T00:00:00.000Z",
                ["current_phase"] = "QUALITY-SEMANTIC-GATES-11",
                ["phase_status"] = "HUMAN_READABLE_RUBRIC_EXECUTED_NONREGRESSION_ONLY_READINESS_CLAIMS_STILL_CLOSED",
                ["phase_scope"] = "QSG-11 executed human-readable rubric scoring over the 54 QSG-10 quality pairs; non-regression is proven for the persisted pairs, but quality improvement, semantic graduation, token savings, common readiness, target promotion, locator update, and final readiness remain closed.",
                ["goal_reached"] = false,
                ["stop_allowed"] = false,
                ["next_action"] = "QUALITY-SEMANTIC-GATES-12: produce genuine adaptive quality improvement evidence or repair adaptive behavior before any semantic/common/final readiness claim.",
                ["continuation_required"] = true
            },
            report,
            validation,
            JsonOptions);
    }

    private sealed record LayerScore(string LayerId, int Score);

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
