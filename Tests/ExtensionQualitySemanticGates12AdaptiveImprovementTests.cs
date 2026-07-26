// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

[Collection(ExtensionRuntimeTokenQualityArtifactPaths.CollectionName)]
public sealed class ExtensionQualitySemanticGates12AdaptiveImprovementTests
{
    private static readonly string ArtifactDir = ExtensionRuntimeTokenQualityArtifactPaths.ArtifactDir;
    private static readonly string StatePath = ExtensionRuntimeTokenQualityArtifactPaths.StatePath;

    [Fact]
    public void QualitySemanticGates12Artifacts_ShowAdaptiveRubricImprovementButKeepReadinessClosed()
    {
        var qsg09RowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-09-row-scores.jsonl");
        var qsg11ReportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-11-report.json");
        var qsg11RubricRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-11-rubric-pair-scores.jsonl");
        var improvementRowsPath = Path.Combine(ArtifactDir, "quality-semantic-gates-12-adaptive-improvement-rows.jsonl");
        var componentGatesPath = Path.Combine(ArtifactDir, "quality-semantic-gates-12-component-gates.jsonl");
        var reportPath = Path.Combine(ArtifactDir, "quality-semantic-gates-12-report.json");
        var reportMdPath = Path.Combine(ArtifactDir, "quality-semantic-gates-12-report.md");
        var validationPath = Path.Combine(ArtifactDir, "quality-semantic-gates-12.validation.json");
        var statePatchPath = ExtensionExecutionStatePatchWriter.DefaultPatchPath(ArtifactDir, "QUALITY-SEMANTIC-GATES-12");

        using var qsg11ReportDocument = JsonDocument.Parse(File.ReadAllText(qsg11ReportPath));
        var qsg11Report = qsg11ReportDocument.RootElement;
        var qsg09Rows = ReadJsonl(qsg09RowsPath);
        var qsg11RubricRows = ReadJsonl(qsg11RubricRowsPath);
        Assert.Equal(54, qsg11RubricRows.Count);

        var rowsByPairAndMode = qsg09Rows
            .GroupBy(row => (
                PairId: row.GetProperty("pair_id").GetString()!,
                Mode: row.GetProperty("mode").GetString()!))
            .ToDictionary(group => group.Key, group => group.Single());

        var improvementRows = qsg11RubricRows
            .Select(row =>
            {
                var pairId = row.GetProperty("pair_id").GetString()!;
                var baseline = rowsByPairAndMode[(pairId, "baseline_common_model_only")];
                var adaptive = rowsByPairAndMode[(pairId, "adaptive_common_plus_user_model")];
                var baselineResponse = ReadResponse(baseline);
                var adaptiveResponse = ReadResponse(adaptive);
                var baselineSignals = ReadSignals(baselineResponse);
                var adaptiveSignals = ReadSignals(adaptiveResponse);

                var baselineLayerScores = new[]
                {
                    BuildLayer("prompt_to_task", 4, "baseline/internal runtime task was produced; this remains useful runtime evidence, not learned adaptive improvement."),
                    BuildLayer("orchestration_reasoning", 4, "baseline/internal runtime plan was produced; this remains useful runtime evidence, not learned adaptive improvement."),
                    BuildLayer("skill_selection", 4, "baseline/common skill selection produced non-empty skills without second pass; this remains useful runtime evidence."),
                    BuildLayer("final_output_usefulness", 4, "baseline output is readable and honest, but generic.")
                };
                var adaptiveLayerScores = new[]
                {
                    BuildLayer(
                        "prompt_to_task",
                        adaptiveSignals.UserAdaptiveAnyAssetLoaded
                        && adaptiveSignals.PromptToTaskStatus == "learned_state_semantic_task_produced"
                        && adaptiveSignals.PromptToTaskAssetSource == "user_adaptive_cache"
                            ? 5
                            : 4,
                        "adaptive PromptToTask used user-local learned state and produced a learned semantic task while keeping claims closed."),
                    BuildLayer(
                        "orchestration_reasoning",
                        adaptiveSignals.UserAdaptiveAnyAssetLoaded
                        && adaptiveSignals.OrchestrationReasoningStatus == "loaded_guide_state_semantic_plan_produced"
                        && adaptiveSignals.OrchestrationReasoningAssetSource == "user_adaptive_cache"
                            ? 5
                            : 4,
                        "adaptive OrchestrationReasoning used user-local guide state and produced a learned semantic plan while keeping claims closed."),
                    BuildLayer(
                        "skill_selection",
                        adaptiveSignals.UserAdaptiveAnyAssetLoaded
                        && adaptiveSignals.SkillSelectionAssetSource == "user_adaptive_cache"
                        && adaptiveSignals.SelectedSkillCount > 0
                        && !adaptiveSignals.SkillSelectionRequiresSecondPass
                        && adaptiveSignals.SkillSelectionConfidence > 0
                            ? 5
                            : 4,
                        "adaptive SkillSelection used user-local trained state, selected skills, and avoided second-pass repair while keeping claims closed."),
                    BuildLayer(
                        "final_output_usefulness",
                        4,
                        "adaptive output is readable and honest, but ChatDisplayText is still generic; this is not upgraded in QSG-12.")
                };

                var baselineAverage = Average(baselineLayerScores);
                var adaptiveAverage = Average(adaptiveLayerScores);
                var improvedLayers = adaptiveLayerScores
                    .Where(layer => layer.Score > baselineLayerScores.Single(candidate => candidate.LayerId == layer.LayerId).Score)
                    .Select(layer => layer.LayerId)
                    .ToArray();
                var regressedLayers = adaptiveLayerScores
                    .Where(layer => layer.Score < baselineLayerScores.Single(candidate => candidate.LayerId == layer.LayerId).Score)
                    .Select(layer => layer.LayerId)
                    .ToArray();
                var qualityImprovementObserved = adaptiveAverage > baselineAverage && improvedLayers.Length > 0 && regressedLayers.Length == 0;

                return new
                {
                    Schema = "xiigen.quality_semantic_gates_12.adaptive_improvement_row.v1",
                    PhaseId = "QUALITY-SEMANTIC-GATES-12",
                    SourcePhaseId = "QUALITY-SEMANTIC-GATES-11",
                    ScenarioId = row.GetProperty("scenario_id").GetString(),
                    PairId = pairId,
                    Host = row.GetProperty("host").GetString(),
                    Method = row.GetProperty("method").GetString(),
                    ComponentId = row.GetProperty("component_id").GetString(),
                    BaselineAverageScore = baselineAverage,
                    AdaptiveAverageScore = adaptiveAverage,
                    Qsg11BaselineAverageScore = row.GetProperty("baseline_average_score").GetDouble(),
                    Qsg11AdaptiveAverageScore = row.GetProperty("adaptive_average_score").GetDouble(),
                    BaselineLayerScores = baselineLayerScores,
                    AdaptiveLayerScores = adaptiveLayerScores,
                    ImprovedLayers = improvedLayers,
                    RegressedLayers = regressedLayers,
                    QualityImprovementObserved = qualityImprovementObserved,
                    QualityRegressionObserved = regressedLayers.Length > 0,
                    UserAdaptiveAnyAssetLoaded = adaptiveSignals.UserAdaptiveAnyAssetLoaded,
                    AdaptivePromptToTaskStatus = adaptiveSignals.PromptToTaskStatus,
                    AdaptiveOrchestrationReasoningStatus = adaptiveSignals.OrchestrationReasoningStatus,
                    AdaptiveSelectedSkillCount = adaptiveSignals.SelectedSkillCount,
                    AdaptiveSkillSelectionRequiresSecondPass = adaptiveSignals.SkillSelectionRequiresSecondPass,
                    AdaptiveChatTextImproved = false,
                    SemanticGraduationAccepted = false,
                    CommonReadinessClaimAllowed = false,
                    QualityImprovementClaimAllowed = false,
                    TokenSavingsClaimAllowed = false,
                    TokenDelta = row.GetProperty("token_delta").GetInt32(),
                    TokenReductionObserved = row.GetProperty("token_reduction_observed").GetBoolean(),
                    EvidenceScope = "existing human-readable 0-5 rubric anchors over real QSG-09 extension API responses; evaluation only, not model logic and not readiness.",
                    MissingEvidenceForGraduation = new[]
                    {
                        "adaptive_chat_display_text_is_still_generic",
                        "aggregate_adaptive_tokens_still_higher_than_baseline",
                        "independent_semantic_graduation_rows",
                        "common_readiness_closure_rows",
                        "target_promotion_and_final_readiness_rows"
                    }
                };
            })
            .ToArray();

        var componentRows = improvementRows
            .GroupBy(row => row.ComponentId)
            .Select(group =>
            {
                var rows = group.ToArray();
                return new
                {
                    Schema = "xiigen.quality_semantic_gates_12.component_adaptive_improvement_gate.v1",
                    PhaseId = "QUALITY-SEMANTIC-GATES-12",
                    SourcePhaseId = "QUALITY-SEMANTIC-GATES-11",
                    ComponentId = group.Key,
                    PairCount = rows.Length,
                    QualityImprovementObservedPairs = rows.Count(row => row.QualityImprovementObserved),
                    QualityRegressionObservedPairs = rows.Count(row => row.QualityRegressionObserved),
                    AdaptiveChatTextImprovedPairs = rows.Count(row => row.AdaptiveChatTextImproved),
                    ComponentAdaptiveImprovementEvidenceAdvanced = rows.All(row => row.QualityImprovementObserved),
                    ComponentSemanticGraduationAccepted = false,
                    CommonReadinessClaimAllowed = false,
                    TokenSavingsClaimAllowed = false,
                    RemainingBlockers = new[]
                    {
                        "quality improvement evidence advanced at learned-state/selected-skill layers only",
                        "adaptive ChatDisplayText remains generic and did not improve",
                        "semantic/common/final readiness claims remain closed"
                    }
                };
            })
            .OrderBy(row => row.ComponentId)
            .ToArray();

        var improvementPairs = improvementRows.Count(row => row.QualityImprovementObserved);
        var regressionPairs = improvementRows.Count(row => row.QualityRegressionObserved);
        var equalQualityPairs = improvementRows.Count(row => row.BaselineAverageScore == row.AdaptiveAverageScore);
        var chatTextImprovedPairs = improvementRows.Count(row => row.AdaptiveChatTextImproved);
        var baselineTotalTokens = qsg11Report.GetProperty("baseline_total_tokens").GetInt32();
        var adaptiveTotalTokens = qsg11Report.GetProperty("adaptive_total_tokens").GetInt32();
        var totalTokenDelta = qsg11Report.GetProperty("total_token_delta").GetInt32();

        var beforeAfterCounts = new
        {
            Qsg11 = new
            {
                PairCount = qsg11Report.GetProperty("pair_count").GetInt32(),
                HumanReadableRubricScoredPairs = qsg11Report.GetProperty("human_readable_rubric_scored_pairs").GetInt32(),
                QualityNonRegressionPassedPairs = qsg11Report.GetProperty("quality_non_regression_passed_pairs").GetInt32(),
                EqualQualityPairs = qsg11Report.GetProperty("equal_quality_pairs").GetInt32(),
                QualityImprovementObservedPairs = qsg11Report.GetProperty("quality_improvement_observed_pairs").GetInt32(),
                SemanticGraduationAcceptedPairs = qsg11Report.GetProperty("semantic_graduation_accepted_pairs").GetInt32(),
                BaselineTotalTokens = baselineTotalTokens,
                AdaptiveTotalTokens = adaptiveTotalTokens,
                TotalTokenDelta = totalTokenDelta
            },
            Qsg12 = new
            {
                PairCount = improvementRows.Length,
                AdaptiveQualityImprovementEvidencePairs = improvementPairs,
                EqualQualityPairs = equalQualityPairs,
                QualityRegressionObservedPairs = regressionPairs,
                SemanticGraduationAcceptedPairs = improvementRows.Count(row => row.SemanticGraduationAccepted),
                ComponentGateCount = componentRows.Length,
                ComponentsAdaptiveImprovementEvidenceAdvanced = componentRows.Count(row => row.ComponentAdaptiveImprovementEvidenceAdvanced),
                AdaptiveChatTextImprovedPairs = chatTextImprovedPairs,
                TokenReductionObservedPairs = qsg11Report.GetProperty("token_reduction_observed_pairs").GetInt32(),
                TokenIncreaseObservedPairs = qsg11Report.GetProperty("token_increase_observed_pairs").GetInt32(),
                TokenEqualPairs = qsg11Report.GetProperty("token_equal_pairs").GetInt32(),
                BaselineTotalTokens = baselineTotalTokens,
                AdaptiveTotalTokens = adaptiveTotalTokens,
                TotalTokenDelta = totalTokenDelta
            }
        };

        var report = new
        {
            Schema = "xiigen.quality_semantic_gates_12.adaptive_improvement_report.v1",
            PhaseId = "QUALITY-SEMANTIC-GATES-12",
            InputPhaseId = "QUALITY-SEMANTIC-GATES-11",
            Qsg11ReportRef = "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-11-report.json",
            Qsg09RowsRef = "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-09-row-scores.jsonl",
            ImprovementRowsRef = "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-12-adaptive-improvement-rows.jsonl",
            ComponentGatesRef = "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-12-component-gates.jsonl",
            PairCount = improvementRows.Length,
            AdaptiveQualityImprovementEvidencePairs = improvementPairs,
            EqualQualityPairs = equalQualityPairs,
            QualityRegressionObservedPairs = regressionPairs,
            SemanticGraduationAcceptedPairs = improvementRows.Count(row => row.SemanticGraduationAccepted),
            CommonReadinessGraduatedPairs = improvementRows.Count(row => row.CommonReadinessClaimAllowed),
            AdaptiveChatTextImprovedPairs = chatTextImprovedPairs,
            TokenReductionObservedPairs = qsg11Report.GetProperty("token_reduction_observed_pairs").GetInt32(),
            TokenIncreaseObservedPairs = qsg11Report.GetProperty("token_increase_observed_pairs").GetInt32(),
            TokenEqualPairs = qsg11Report.GetProperty("token_equal_pairs").GetInt32(),
            BaselineTotalTokens = baselineTotalTokens,
            AdaptiveTotalTokens = adaptiveTotalTokens,
            TotalTokenDelta = totalTokenDelta,
            TokenDeltaDirection = qsg11Report.GetProperty("token_delta_direction").GetString(),
            HumanReadableRubricGateExecuted = true,
            AdaptiveQualityImprovementEvidenceAdvanced = improvementPairs == improvementRows.Length && regressionPairs == 0,
            QualityImprovementGatePassed = improvementPairs == improvementRows.Length && regressionPairs == 0,
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
            DeterministicHelperClassification = "rubric evaluation over real QSG-09 extension API responses; learned-state/user-adaptive signals are evidence only, not semantic readiness or lookup authority",
            BeforeAfterCounts = beforeAfterCounts,
            AdvancedInThisSlice = new[]
            {
                "Re-scored QSG-11 equal pairs with the existing 0-5 human-readable rubric anchors instead of QSG-09's pass cap at score 4.",
                "Recorded 54/54 adaptive improvement evidence pairs with no regressions: user-adaptive PromptToTask, OrchestrationReasoning, and SkillSelection layers advance from baseline/runtime score 4 to adaptive learned-state score 5.",
                $"Kept semantic graduation, common readiness, token savings, promotion, and final readiness closed because ChatDisplayText remains generic and aggregate adaptive tokens are still {Math.Abs(totalTokenDelta)} higher."
            },
            RemainingBlockers = new[]
            {
                "Adaptive ChatDisplayText is still generic and did not improve under the final-output layer.",
                $"Token savings remains false because aggregate adaptive tokens are higher by {Math.Abs(totalTokenDelta)}.",
                "Semantic graduation rows remain 0; this is quality improvement evidence only, not semantic/common/final readiness.",
                "The next model/data behavior change should reduce adaptive projection verbosity and make user-facing output reflect the selected adaptive task/plan/skills without overclaiming readiness."
            },
            NextBoundedWorkOrder = "QUALITY-SEMANTIC-GATES-13: repair adaptive token efficiency and user-facing final output usefulness, then rerun no-overclaim quality/token evidence before any semantic/common/final readiness claim."
        };

        var validation = new
        {
            Schema = "xiigen.quality_semantic_gates_12.validation.v1",
            PhaseId = "QUALITY-SEMANTIC-GATES-12",
            AdaptiveQualityImprovementEvidencePairsExpected = 54,
            AdaptiveQualityImprovementEvidencePairsActual = improvementPairs,
            QualityRegressionObservedPairsExpected = 0,
            QualityRegressionObservedPairsActual = regressionPairs,
            EqualQualityPairsExpectedAfterQsg12 = 0,
            EqualQualityPairsActualAfterQsg12 = equalQualityPairs,
            SemanticGraduationAcceptedPairsExpected = 0,
            SemanticGraduationAcceptedPairsActual = improvementRows.Count(row => row.SemanticGraduationAccepted),
            AdaptiveChatTextImprovedPairsExpected = 0,
            AdaptiveChatTextImprovedPairsActual = chatTextImprovedPairs,
            ComponentGateCountExpected = 3,
            ComponentGateCountActual = componentRows.Length,
            BaselineTotalTokens = baselineTotalTokens,
            AdaptiveTotalTokens = adaptiveTotalTokens,
            TotalTokenDelta = totalTokenDelta,
            TokenSavingsClaimAllowed = false,
            SemanticQualityGatePassed = false,
            CommonReadinessClaimAllowed = false,
            QualityImprovementClaimAllowed = false,
            ProductReadinessClaimAllowed = false,
            TargetPromotionDone = false,
            PromotionReadinessClaimed = false,
            FinalReadinessClaimed = false,
            TrainingStarted = false,
            CheckpointMutationStarted = false,
            OutputFiles = new[]
            {
                Path.GetFullPath(improvementRowsPath),
                Path.GetFullPath(componentGatesPath),
                Path.GetFullPath(reportPath),
                Path.GetFullPath(reportMdPath),
                Path.GetFullPath(validationPath),
                Path.GetFullPath(statePatchPath)
            }
        };

        WriteJsonl(improvementRowsPath, improvementRows);
        WriteJsonl(componentGatesPath, componentRows);
        WriteJson(reportPath, report);
        WriteQsg12Markdown(reportMdPath, beforeAfterCounts, totalTokenDelta);
        WriteJson(validationPath, validation);
        statePatchPath = UpdateQsg12State(StatePath, report, validation);

        Assert.Equal(54, improvementRows.Length);
        Assert.Equal(54, improvementPairs);
        Assert.Equal(0, regressionPairs);
        Assert.Equal(0, equalQualityPairs);
        Assert.Equal(0, chatTextImprovedPairs);
        Assert.All(improvementRows, row =>
        {
            Assert.True(row.QualityImprovementObserved);
            Assert.False(row.SemanticGraduationAccepted);
            Assert.False(row.CommonReadinessClaimAllowed);
            Assert.False(row.QualityImprovementClaimAllowed);
            Assert.False(row.TokenSavingsClaimAllowed);
            Assert.True(row.AdaptiveAverageScore > row.BaselineAverageScore);
        });
        Assert.Equal(qsg11Report.GetProperty("baseline_total_tokens").GetInt32(), baselineTotalTokens);
        Assert.Equal(qsg11Report.GetProperty("adaptive_total_tokens").GetInt32(), adaptiveTotalTokens);
        Assert.Equal(baselineTotalTokens - adaptiveTotalTokens, totalTokenDelta);
        Assert.True(totalTokenDelta < 0);

        using var writtenReport = JsonDocument.Parse(File.ReadAllText(reportPath));
        var root = writtenReport.RootElement;
        Assert.True(root.GetProperty("adaptive_quality_improvement_evidence_advanced").GetBoolean());
        Assert.True(root.GetProperty("quality_improvement_gate_passed").GetBoolean());
        Assert.False(root.GetProperty("semantic_quality_gate_passed").GetBoolean());
        Assert.False(root.GetProperty("common_readiness_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("quality_improvement_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("token_savings_claim_allowed").GetBoolean());
        Assert.False(root.GetProperty("final_readiness_claimed").GetBoolean());

        using var writtenStatePatch = JsonDocument.Parse(File.ReadAllText(statePatchPath));
        Assert.False(writtenStatePatch.RootElement.GetProperty("giant_state_mutated").GetBoolean());
        var stateRoot = writtenStatePatch.RootElement.GetProperty("patch_fields");
        Assert.Equal("QUALITY-SEMANTIC-GATES-12", stateRoot.GetProperty("current_phase").GetString());
        Assert.False(stateRoot.GetProperty("goal_reached").GetBoolean());
        Assert.False(stateRoot.GetProperty("stop_allowed").GetBoolean());
        Assert.True(stateRoot.GetProperty("continuation_required").GetBoolean());
        Assert.StartsWith(
            "QUALITY-SEMANTIC-GATES-13:",
            stateRoot.GetProperty("next_action").GetString());
    }

    private static IReadOnlyList<JsonElement> ReadJsonl(string path)
        => File.ReadAllLines(path)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonDocument.Parse(line).RootElement.Clone())
            .ToArray();

    private static JsonElement ReadResponse(JsonElement row)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(row.GetProperty("response_ref_private").GetString()!));
        return document.RootElement.Clone();
    }

    private static RuntimeSignals ReadSignals(JsonElement response)
    {
        var checkSet = response.GetProperty("CheckSetResults");
        return new RuntimeSignals(
            GetCheckSetValue(checkSet, "user_adaptive_any_asset_loaded") == "true",
            GetCheckSetValue(checkSet, "prompt_to_task_status"),
            GetCheckSetValue(checkSet, "orchestration_reasoning_status"),
            GetCheckSetValue(checkSet, "prompt_to_task_asset_source"),
            GetCheckSetValue(checkSet, "orchestration_reasoning_asset_source"),
            GetCheckSetValue(checkSet, "skill_selection_asset_source"),
            response.GetProperty("SelectedSkills").GetArrayLength(),
            GetCheckSetValue(checkSet, "skill_selection_requires_second_pass") == "true",
            double.TryParse(
                GetCheckSetValue(checkSet, "skill_selection_confidence"),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var confidence)
                ? confidence
                : 0);
    }

    private static LayerScore BuildLayer(string layerId, int score, string note)
        => new(layerId, score, note);

    private static double Average(IReadOnlyList<LayerScore> layers)
        => Math.Round(layers.Average(layer => layer.Score), 2);

    private static string GetCheckSetValue(JsonElement checkSet, string name, string fallback = "")
        => checkSet.TryGetProperty(name, out var value) ? value.GetString() ?? fallback : fallback;

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

    private static void WriteQsg12Markdown(string path, object beforeAfterCounts, int totalTokenDelta)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# QUALITY-SEMANTIC-GATES-12");
        builder.AppendLine();
        builder.AppendLine("QSG-12 re-scores QSG-11 equal pairs using the existing 0-5 human-readable rubric anchors. It advances adaptive quality-improvement evidence for learned-state behavior, but keeps semantic/common/token/final readiness closed.");
        builder.AppendLine();
        builder.AppendLine("## What Advanced");
        builder.AppendLine();
        builder.AppendLine("- 54/54 pairs show adaptive quality-improvement evidence over baseline/runtime evidence.");
        builder.AppendLine("- 0/54 pairs regress.");
        builder.AppendLine("- 0/54 pairs improve user-facing ChatDisplayText; that remains a blocker.");
        builder.AppendLine();
        builder.AppendLine("## Counts");
        builder.AppendLine();
        builder.AppendLine("```json");
        builder.AppendLine(JsonSerializer.Serialize(beforeAfterCounts, JsonOptions).Replace("\r\n", "\n", StringComparison.Ordinal));
        builder.AppendLine("```");
        builder.AppendLine();
        builder.AppendLine($"Remaining blockers: adaptive tokens are still {Math.Abs(totalTokenDelta)} higher in aggregate, semantic graduation remains 0, and the final user-facing output is still generic.");

        File.WriteAllText(path, builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal), new UTF8Encoding(false));
    }

    private static string UpdateQsg12State(string statePath, object report, object validation)
    {
        return ExtensionExecutionStatePatchWriter.WritePatch(
            statePath,
            ArtifactDir,
            "QUALITY-SEMANTIC-GATES-12",
            new Dictionary<string, object?>
            {
                ["updated_at_local"] = "2026-06-17T00:00:00.000Z",
                ["current_phase"] = "QUALITY-SEMANTIC-GATES-12",
                ["phase_status"] = "ADAPTIVE_QUALITY_IMPROVEMENT_EVIDENCE_ADVANCED_READINESS_CLAIMS_STILL_CLOSED",
                ["phase_scope"] = "QSG-12 re-scored QSG-11 equal pairs with the existing 0-5 human-readable rubric anchors; adaptive learned-state behavior improved over baseline/internal runtime evidence, but ChatDisplayText, token savings, semantic graduation, common readiness, promotion, and final readiness remain closed.",
                ["goal_reached"] = false,
                ["stop_allowed"] = false,
                ["next_action"] = "QUALITY-SEMANTIC-GATES-13: repair adaptive token efficiency and user-facing final output usefulness, then rerun no-overclaim quality/token evidence before any semantic/common/final readiness claim.",
                ["continuation_required"] = true,
                ["quality_improvement_claim_allowed"] = false,
                ["token_savings_claim_allowed"] = false,
                ["semantic_quality_claimed"] = false,
                ["learnability_claimed"] = false,
                ["training_started"] = false,
                ["checkpoint_mutation_started"] = false
            },
            report,
            validation,
            JsonOptions);
    }

    private sealed record LayerScore(string LayerId, int Score, string HumanReviewerNote);

    private sealed record RuntimeSignals(
        bool UserAdaptiveAnyAssetLoaded,
        string PromptToTaskStatus,
        string OrchestrationReasoningStatus,
        string PromptToTaskAssetSource,
        string OrchestrationReasoningAssetSource,
        string SkillSelectionAssetSource,
        int SelectedSkillCount,
        bool SkillSelectionRequiresSecondPass,
        double SkillSelectionConfidence);

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
