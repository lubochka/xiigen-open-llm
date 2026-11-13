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
            // Project each QSG-09 component separately so its own provenance can keep readiness closed.
            .Select(component =>
            {
                // Preserve the component identity used to distinguish the blocked orchestration path.
                var componentId = component.GetProperty("component_id").GetString();
                // Carry the upstream runtime gate verbatim instead of manufacturing a QSG-10 success.
                var baselineCommonRuntimeGatePassed = component.GetProperty("baseline_common_runtime_gate_passed").GetBoolean();
                // Require extra provenance only for the component whose common checkpoint is still untrusted.
                var requiredMissingProof = componentId == "orchestration_reasoning"
                    // The orchestration component needs the stricter missing-proof inventory.
                    ? new[]
                    // These entries explain why runtime diagnostics cannot yet become readiness authority.
                    {
                        // No candidate may graduate until an independently accepted semantic row exists.
                        "accepted_semantic_graduation_rows",
                        // Human-readable quality improvement remains an explicit unresolved proof.
                        "human_readable_rubric_quality_improvement_rows",
                        // Orchestration needs rebuilt source-admission provenance for its common checkpoint.
                        "orchestration_reasoning_source_admission_provenance_rebuild",
                        // Runtime internals must not silently become the semantic decision authority.
                        "proof_that_baseline_internal_runtime_logic_is_not_the_semantic_authority"
                    // The orchestration-only list ends without weakening any shared closure condition.
                    }
                    // Other components retain the common proof set without claiming orchestration repair.
                    : new[]
                    // The shared list still blocks graduation until semantic evidence is independently accepted.
                    {
                        // Every component still lacks accepted semantic graduation rows.
                        "accepted_semantic_graduation_rows",
                        // Every component still lacks accepted human-rubric improvement evidence.
                        "human_readable_rubric_quality_improvement_rows",
                        // Every component must keep runtime implementation details non-authoritative.
                        "proof_that_baseline_internal_runtime_logic_is_not_the_semantic_authority"
                    // Bind the selected proof list to the emitted component record.
                    };

                // Emit a reviewable component record that preserves upstream facts and closure decisions together.
                return new
                {
                    // Pin the schema so downstream readers know this row is a graduation gate, not a success claim.
                    Schema = "xiigen.quality_semantic_gates_10.component_graduation_gate.v1",
                    // Identify this row as QSG-10 output for provenance audits.
                    PhaseId = "QUALITY-SEMANTIC-GATES-10",
                    // Retain QSG-09 as the sole source of the propagated runtime evidence.
                    SourcePhaseId = "QUALITY-SEMANTIC-GATES-09",
                    // Preserve which component the evidence and blockers describe.
                    ComponentId = componentId,
                    // Preserve the upstream machine identity instead of substituting a new authority.
                    MachineId = component.GetProperty("machine_id").GetString(),
                    // Carry the exact upstream runtime-pass count as diagnostic evidence only.
                    RuntimeLayerPassRows = component.GetProperty("runtime_layer_pass_rows").GetInt32(),
                    // Carry the exact unresolved baseline/common count for this component.
                    BaselineCommonNeedsRepairRows = component.GetProperty("baseline_common_needs_repair_rows").GetInt32(),
                    // Preserve the source-disjoint gate result without treating it as semantic graduation.
                    SourceDisjointEvalGatePassed = component.GetProperty("source_disjoint_eval_gate_passed").GetBoolean(),
                    // Preserve the ablation result as one bounded prerequisite rather than a readiness shortcut.
                    AblationGatePassed = component.GetProperty("ablation_gate_passed").GetBoolean(),
                    // Preserve the leakage result while keeping the semantic decision closed.
                    LeakageGatePassed = component.GetProperty("leakage_gate_passed").GetBoolean(),
                    // Preserve fresh-load evidence without converting it into common readiness.
                    FreshLoadGatePassed = component.GetProperty("fresh_load_gate_passed").GetBoolean(),
                    // Preserve continue-training evidence without authorizing promotion or graduation.
                    ContinueTrainingGatePassed = component.GetProperty("continue_training_gate_passed").GetBoolean(),
                    // Report the actual upstream runtime gate, including the orchestration false result.
                    BaselineCommonRuntimeGatePassed = baselineCommonRuntimeGatePassed,
                    // Carry the upstream semantic gate exactly so QSG-10 cannot invent quality evidence.
                    Qsg09SemanticQualityGatePassed = component.GetProperty("semantic_quality_gate_passed").GetBoolean(),
                    // Record that no independently accepted semantic rows exist for this component.
                    AcceptedSemanticGraduationRows = 0,
                    // Keep eligibility closed because the required proof set is incomplete.
                    GraduationCriteriaMet = false,
                    // Keep the graduation decision closed even when some runtime diagnostics pass.
                    GraduationAccepted = false,
                    // Prevent downstream readers from interpreting diagnostics as common readiness.
                    CommonReadinessClaimAllowed = false,
                    // Publish the component-specific reasons that continue to block graduation.
                    RequiredMissingProof = requiredMissingProof,
                    // Point reviewers to the exact upstream artifacts and governing plan section.
                    RequiredEvidenceRefs = new[]
                    // Keep the evidence bundle bounded to the two source artifacts and their governing contract.
                    {
                        // The QSG-09 pair results are the row-level runtime evidence source.
                        "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-09-pair-results.jsonl",
                        // The QSG-09 row scores provide the corresponding diagnostic measurements.
                        "artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-09-row-scores.jsonl",
                        // The plan section defines the boundary these diagnostics must not cross.
                        "docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md Section 11"
                    // Close the evidence list before recording the human interpretation.
                    },
                    // Explain the row differently depending on whether the upstream runtime gate itself passed.
                    HumanReviewerNote = baselineCommonRuntimeGatePassed
                        // A runtime pass still cannot replace the absent semantic graduation rows.
                        ? "This component baseline/common runtime gate passed, but it has zero accepted semantic graduation rows; common readiness remains closed."
                        // A failed runtime gate additionally names the source-admission provenance rebuild debt.
                        : "This component remains blocked by source-admission provenance rebuild and has zero accepted semantic graduation rows; common readiness remains closed."
                // Complete one bounded component record without opening any readiness state.
                };
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
        // Read the upstream common-runtime decision so QSG-10 reports the false gate instead of hard-coding success.
        var qsg09BaselineCommonRuntimeGatePassed = qsg09Report.GetProperty("baseline_common_runtime_gate_passed").GetBoolean();
        // Preserve the upstream wording that limits the legacy repaired-row count to its real scope.
        var qsg09BaselineCommonRepairedRowsScope = qsg09Report.GetProperty("baseline_common_repaired_rows_scope").GetString();
        // Count only components whose complete graduation criteria actually passed; diagnostics alone contribute zero.
        var componentGraduationCandidateCount = componentGateRows.Count(row => row.GraduationCriteriaMet);

        var beforeAfterCounts = new
        {
            Qsg09 = new
            {
                RowCount = qsg09Report.GetProperty("row_count").GetInt32(),
                PairCount = qsg09Report.GetProperty("pair_count").GetInt32(),
                BaselineCommonRepairedRows = qsg09Report.GetProperty("baseline_common_repaired_rows").GetInt32(),
                // Carry the qualifier that prevents the legacy count from being read as universal repair.
                BaselineCommonRepairedRowsScope = qsg09BaselineCommonRepairedRowsScope,
                BaselineCommonNeedsRepairRows = qsg09Report.GetProperty("baseline_common_needs_repair_rows").GetInt32(),
                // Preserve the PromptToTask unresolved-row count as a separate component fact.
                PromptToTaskBaselineCommonNeedsRepairRows = qsg09Report.GetProperty("prompt_to_task_baseline_common_needs_repair_rows").GetInt32(),
                // Preserve the orchestration unresolved-row count instead of merging it with other components.
                OrchestrationReasoningBaselineCommonNeedsRepairRows = qsg09Report.GetProperty("orchestration_reasoning_baseline_common_needs_repair_rows").GetInt32(),
                // Expose the exact number of orchestration rows blocked on provenance reconstruction.
                OrchestrationReasoningProvenanceRebuildBlockedRows = qsg09Report.GetProperty("orchestration_reasoning_provenance_rebuild_blocked_rows").GetInt32(),
                RuntimeQualityPassedRows = qsg09Report.GetProperty("runtime_quality_passed_rows").GetInt32(),
                QualityGatePairs = qsg09Report.GetProperty("quality_gate_pairs").GetInt32(),
                // Preserve the false upstream runtime gate in the before-state snapshot.
                BaselineCommonRuntimeGatePassed = qsg09BaselineCommonRuntimeGatePassed,
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
                // Report zero candidates because incomplete provenance keeps every component gate closed.
                ComponentsGraduationCandidates = componentGraduationCandidateCount,
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
            // Embed the upstream scope statement in the report so the legacy count cannot lose its qualifier.
            Qsg09BaselineCommonRepairedRowsScope = qsg09BaselineCommonRepairedRowsScope,
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
            // Keep the report aligned with QSG-09's actual false baseline/common runtime gate.
            BaselineCommonRuntimeGatePassed = qsg09BaselineCommonRuntimeGatePassed,
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
                // Describe the measured token direction without turning a larger adaptive total into a savings claim.
                $"Computed token before/after diagnostics from QSG-09 pairs: {tokenReductionObservedPairs} reductions, {tokenIncreaseObservedPairs} increases, {tokenEqualPairs} equal pairs, and adaptive uses {Math.Abs(totalTokenDelta)} more total tokens overall; token savings claim remains false.",
                // Tell human reviewers why the old repaired-row total has a deliberately narrow interpretation.
                "Carried QSG-09 baseline_common_repaired_rows_scope so the legacy count is not misread as universal OrchestrationReasoning repair."
            },
            RemainingBlockers = new[]
            {
                "Accepted semantic graduation rows are still 0.",
                // Keep the exact 54-row orchestration provenance debt visible in the bounded next-step list.
                "OrchestrationReasoning common checkpoint source-admission provenance rebuild is still blocked across 54 rows.",
                "Quality improvement under the human-readable rubric is still not proven.",
                "Token savings remains false because semantic quality is not proven and aggregate adaptive tokens are higher.",
                "Common readiness, promotion, and final readiness remain closed until all required closure rows pass together."
            },
            // Route subsequent work to provenance repair and human-rubric evidence while readiness remains closed.
            NextBoundedWorkOrder = "QUALITY-SEMANTIC-GATES-11: keep readiness claims closed until OrchestrationReasoning common checkpoint provenance is rebuilt and accepted semantic graduation rows pass the human-readable quality rubric."
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
            // The accepted QSG-09 facts allow no pair to enter graduation candidacy.
            GraduationCandidatePairsExpected = 0,
            GraduationCandidatePairsActual = pairGuardRows.Count(row => row.GraduationCandidate),
            // The component-level oracle likewise requires zero graduation candidates.
            ComponentsGraduationCandidatesExpected = 0,
            // Compare the emitted component count to the same criteria-derived value used by the report.
            ComponentsGraduationCandidatesActual = componentGraduationCandidateCount,
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
            // Pin the expected common-runtime gate closed for this provenance state.
            BaselineCommonRuntimeGatePassedExpected = false,
            // Record the actual propagated gate so divergence fails the validation artifact.
            BaselineCommonRuntimeGatePassedActual = qsg09BaselineCommonRuntimeGatePassed,
            // Retain the human-readable legacy-count qualifier in machine-readable validation evidence.
            Qsg09BaselineCommonRepairedRowsScope = qsg09BaselineCommonRepairedRowsScope,
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
            OutputFiles = ExtensionRuntimeTokenQualityArtifactPaths.ToCanonicalWindowsRepoRefs(
                "QUALITY-SEMANTIC-GATES-10 output artifact",
                pairGuardRowsPath,
                componentGatesPath,
                graduationCriteriaPath,
                reportPath,
                reportMdPath,
                validationPath,
                statePatchPath)
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
            // Prove the upstream quality diagnostic is false for every guarded pair.
            Assert.False(qsg09QualityGatePassed);
            // Prove neither side of the pair has the runtime evidence needed for candidacy.
            Assert.False(runtimeQualityGatePassedForBothRows);
            Assert.False(row.GetProperty("qsg09_semantic_quality_gate_passed").GetBoolean());
            // Prove the generated guard does not promote a diagnostic row into graduation candidacy.
            Assert.False(row.GetProperty("graduation_candidate").GetBoolean());
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
        // Isolate the orchestration component so its 54-row provenance debt is checked directly.
        var writtenOrchestrationComponentGate = Assert.Single(
            // Search only the generated component-gate rows written by this QSG-10 pass.
            writtenComponentGateRows,
            // Bind the strict oracle to OrchestrationReasoning rather than accepting any component.
            row => row.GetProperty("component_id").GetString() == "orchestration_reasoning");
        // Preserve the exact 54 upstream runtime rows in the orchestration projection.
        Assert.Equal(54, writtenOrchestrationComponentGate.GetProperty("runtime_layer_pass_rows").GetInt32());
        // Preserve the exact 54 rows whose common-runtime authority still needs repair.
        Assert.Equal(54, writtenOrchestrationComponentGate.GetProperty("baseline_common_needs_repair_rows").GetInt32());
        // Keep the orchestration common-runtime gate closed despite the diagnostic row count.
        Assert.False(writtenOrchestrationComponentGate.GetProperty("baseline_common_runtime_gate_passed").GetBoolean());
        // Require the generated row to name the missing provenance reconstruction explicitly.
        Assert.Contains(
            // Inspect the emitted proof-debt list rather than relying on a summary flag.
            writtenOrchestrationComponentGate.GetProperty("required_missing_proof").EnumerateArray(),
            // Match the exact source-admission provenance marker needed by the next repair.
            proof => proof.GetString() == "orchestration_reasoning_source_admission_provenance_rebuild");
        // Require the human review note to explain the same blocked provenance boundary.
        Assert.Contains(
            // The readable note must name the repair, not merely report a false boolean.
            "source-admission provenance rebuild",
            // Read the note from the generated orchestration component row.
            writtenOrchestrationComponentGate.GetProperty("human_reviewer_note").GetString(),
            // Use exact ordinal wording so the evidence remains deterministic across cultures.
            StringComparison.Ordinal);

        using var writtenReport = JsonDocument.Parse(File.ReadAllText(reportPath));
        var root = writtenReport.RootElement;
        Assert.Equal("QUALITY-SEMANTIC-GATES-10", root.GetProperty("phase_id").GetString());
        // Pin the source data to zero quality-gate pairs before reviewing the generated report.
        Assert.Equal(0, qsg09QualityGatePairs);
        Assert.Equal(qsg09QualityGatePairs, root.GetProperty("qsg09_quality_gate_pairs_seen").GetInt32());
        Assert.Equal(54, root.GetProperty("no_overclaim_guard_rows").GetInt32());
        Assert.Equal(pairGuardRows.Count(row => row.GraduationCandidate), root.GetProperty("graduation_candidate_pairs").GetInt32());
        // Prove the report exposes zero candidates rather than recycling the old quality count.
        Assert.Equal(0, root.GetProperty("graduation_candidate_pairs").GetInt32());
        Assert.Equal(0, root.GetProperty("semantic_graduation_accepted_pairs").GetInt32());
        Assert.Equal(0, root.GetProperty("common_readiness_graduated_pairs").GetInt32());
        // Preserve the exact count of pairs whose adaptive trace used fewer tokens.
        Assert.Equal(12, root.GetProperty("token_reduction_observed_pairs").GetInt32());
        // Preserve the exact count of pairs whose adaptive trace used more tokens.
        Assert.Equal(42, root.GetProperty("token_increase_observed_pairs").GetInt32());
        Assert.Equal(0, root.GetProperty("token_equal_pairs").GetInt32());
        // Pin the baseline aggregate so later reports cannot shift the comparison denominator.
        Assert.Equal(13887, root.GetProperty("baseline_total_tokens").GetInt32());
        // Pin the larger adaptive aggregate that prevents a token-savings claim.
        Assert.Equal(14343, root.GetProperty("adaptive_total_tokens").GetInt32());
        // Preserve the signed baseline-minus-adaptive delta showing 456 additional adaptive tokens.
        Assert.Equal(-456, root.GetProperty("total_token_delta").GetInt32());
        Assert.Equal(totalTokenDelta, root.GetProperty("total_token_delta").GetInt32());
        // Prove the generated report carries the false common-runtime gate.
        Assert.False(root.GetProperty("baseline_common_runtime_gate_passed").GetBoolean());
        // Compare the report's legacy-count qualifier to the exact upstream wording.
        Assert.Equal(
            // Use the source report's scope as the authority for the expected human interpretation.
            qsg09BaselineCommonRepairedRowsScope,
            // Read the propagated scope from the generated QSG-10 report.
            root.GetProperty("qsg09_baseline_common_repaired_rows_scope").GetString());
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
        // Hold a single validation root so every exact counter is checked against the same generated document.
        var validationRoot = writtenValidation.RootElement;
        // The validation contract must expect zero source quality-gate pairs.
        Assert.Equal(0, validationRoot.GetProperty("qsg09_quality_gate_pairs_expected").GetInt32());
        // The generated artifact must observe the same zero rather than preserve the stale count.
        Assert.Equal(0, validationRoot.GetProperty("qsg09_quality_gate_pairs_actual").GetInt32());
        // Validate the 12 token-reduction rows independently of report prose.
        Assert.Equal(12, validationRoot.GetProperty("token_reduction_observed_pairs").GetInt32());
        // Validate the 42 token-increase rows that keep savings unproven.
        Assert.Equal(42, validationRoot.GetProperty("token_increase_observed_pairs").GetInt32());
        // Confirm no pair has an equal token total, preserving the complete 54-row partition.
        Assert.Equal(0, validationRoot.GetProperty("token_equal_pairs").GetInt32());
        // Recheck the exact baseline aggregate in the machine-readable validation record.
        Assert.Equal(13887, validationRoot.GetProperty("baseline_total_tokens").GetInt32());
        // Recheck the exact adaptive aggregate in the machine-readable validation record.
        Assert.Equal(14343, validationRoot.GetProperty("adaptive_total_tokens").GetInt32());
        // Recheck the signed aggregate delta so the direction cannot be reversed silently.
        Assert.Equal(-456, validationRoot.GetProperty("total_token_delta").GetInt32());
        // Require validation to record the actual common-runtime gate as closed.
        Assert.False(validationRoot.GetProperty("baseline_common_runtime_gate_passed_actual").GetBoolean());
        // Require validation to preserve the same legacy-count scope as the source report.
        Assert.Equal(
            // Treat the upstream scope text as the expected provenance boundary.
            qsg09BaselineCommonRepairedRowsScope,
            // Read the scope copied into the validation artifact.
            validationRoot.GetProperty("qsg09_baseline_common_repaired_rows_scope").GetString());
        // Prove the validation explicitly guards against treating runtime quality as semantic authority.
        Assert.True(validationRoot.GetProperty("quality_gate_pairs_misread_as_semantic_quality_prevented").GetBoolean());

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

        // Read the human-facing report so its claims are held to the same closed-readiness facts.
        var writtenMarkdown = File.ReadAllText(reportMdPath);
        // Require the corrected zero-of-54 quality-pair statement in the human report.
        Assert.Contains("quality_gate_pairs=0 across 54", writtenMarkdown, StringComparison.Ordinal);
        // Require the human report to name the orchestration provenance rebuild debt.
        Assert.Contains("source-admission provenance rebuild", writtenMarkdown, StringComparison.Ordinal);
        // Reject the stale 48-pair statement that previously overstated the QSG-09 evidence.
        Assert.DoesNotContain("quality_gate_pairs=48", writtenMarkdown, StringComparison.Ordinal);
        // Reject prose that could broaden a legacy subset count into an all-baseline claim.
        Assert.DoesNotContain("all baseline", writtenMarkdown, StringComparison.OrdinalIgnoreCase);
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
        // Tell readers the exact zero-of-54 fact and keep runtime diagnostics explicitly non-semantic.
        builder.AppendLine("- QSG-09 quality_gate_pairs=0 across 54 pair rows; runtime-quality diagnostics remain non-semantic evidence only.");
        builder.AppendLine("- Baseline/internal runtime quality cannot graduate to common readiness without accepted semantic graduation rows.");
        // Explain that all 54 orchestration rows still need trusted source-admission provenance.
        builder.AppendLine("- OrchestrationReasoning has 54 runtime rows that still need source-admission provenance rebuild before the common checkpoint can pass.");
        // Present the exact token partition and larger adaptive total without claiming savings.
        builder.AppendLine("- QSG-09 token diagnostics are honest before/after evidence: 12 reductions, 42 increases, 0 equal pairs, and aggregate adaptive tokens are higher.");
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
        // Direct the next track to provenance and human-rubric proof while preserving the readiness closure.
        builder.AppendLine("Next independent track: QUALITY-SEMANTIC-GATES-11 must keep readiness claims closed until OrchestrationReasoning common checkpoint provenance is rebuilt and accepted semantic graduation rows pass the human-readable quality rubric.");

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
                // Keep the execution-state continuation aligned with the same provenance-first readiness boundary.
                ["next_action"] = "QUALITY-SEMANTIC-GATES-11: keep readiness claims closed until OrchestrationReasoning common checkpoint provenance is rebuilt and accepted semantic graduation rows pass the human-readable quality rubric.",
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
