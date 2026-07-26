// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;
using SmartSkillsDomain;

public sealed class SkillSelectionP16CFocusedBoundaryTests
{
    private static readonly string ForbiddenSemanticQualityTrueEvidence = "SEMANTIC_QUALITY_CLAIMED_" + "TRUE";

    [Fact]
    public void CandidateOrderLeakageGuard_DetectsOrderSensitiveProbeButCannotSelectSkill()
    {
        var request = new SkillSelectionRequest(
            Prompt: "Select the review skill for a bounded phase.",
            ProjectRoot: "C:/sandbox/llm_mvp_core",
            AvailableSkillIds: ["plan-review", "image-generation", "documentation-sync"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "P16C-CANDIDATE-GUARD");
        var guard = new CandidateOrderLeakageGuardUnit(
            new CandidateOrderLeakageGuardState(
                UnitId: CandidateOrderLeakageGuardUnit.UnitId,
                SchemaVersion: 1,
                CandidatePositionWeights: new Dictionary<int, int> { [0] = 5, [1] = 1 },
                SemanticQualityClaimed: false),
            probeRequest => new SkillSelectionDecision(
                SelectedSkillIds: [probeRequest.AvailableSkillIds[0]],
                RejectedSkillIds: probeRequest.AvailableSkillIds.Skip(1).ToArray(),
                EvidenceFeatures: ["position_probe"],
                Confidence: 0.7,
                RequiresSecondPass: false,
                DecisionText: "Synthetic leaky probe selects the first candidate."));

        var report = guard.Evaluate(request);

        Assert.False(report.SemanticQualityClaimed);
        Assert.False(report.CanSelectSkill);
        Assert.True(report.OrderSensitivityDetected);
        Assert.False(report.PermutationSelectionInvariant);
        Assert.Equal(2, report.PermutationCount);
        Assert.Contains("candidate_list_order_skill_authority", report.BlockedFeatureIds);
        Assert.Contains(CandidateOrderLeakageGuardUnit.DiagnosticsEvidence, report.EvidenceFeatures);
        Assert.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence, report.EvidenceFeatures);
        Assert.Contains("candidate_order_guard_status=selection_authority_blocked", report.EvidenceFeatures);
    }

    [Fact]
    public async Task PromptUnderstanding_DoesNotExposeExactPromptSourceOrCandidatePositionAuthority()
    {
        var trainingRequest = new SkillSelectionRequest(
            Prompt: "Review plan coverage before accepting an execution phase.",
            ProjectRoot: "C:/sandbox/llm_mvp_core",
            AvailableSkillIds: ["planning-session-startup", "plan-review"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "P16C-PROMPT-TRAIN");
        var heldOutRequest = new SkillSelectionRequest(
            Prompt: "Review the source-ref title marker and plan coverage before the executor starts.",
            ProjectRoot: "C:/sandbox/llm_mvp_core",
            AvailableSkillIds: ["planning-session-startup", "plan-review"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "P16C-PROMPT-HELDOUT");
        var machine = new SkillSelectionMachine();
        var training = await machine.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "p16c-prompt-understanding-pair",
                    Request: trainingRequest,
                    Chosen: new SkillSelectionDecision(["plan-review"], ["planning-session-startup"], ["review_intent"], 1.0, false, "Plan review matches review intent."),
                    Rejected: new SkillSelectionDecision(["planning-session-startup"], ["plan-review"], ["startup_near_miss"], 0.1, true, "Startup is a near miss for session initialization."),
                    RejectedType: "adjacent_wrong_skill",
                    SourcePromptEntryId: "P16C-PROMPT-TRAIN",
                    TargetTrainingAdapter: SkillSelectionMachine.PromptUnderstandingTrainingAdapter)
            ]);

        var decision = machine.DecideFromTrainedState(heldOutRequest);

        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.True(decision.IsSuccess, decision.ErrorMessage);
        Assert.Equal(["plan-review"], decision.Value!.SelectedSkillIds);
        Assert.Contains(SkillSelectionMachine.PromptUnderstandingCheckpointEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(PromptUnderstandingSkillUnit.UnitExecutedEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(PromptUnderstandingSkillUnit.BaselineEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains("prompt_understanding:semantic_quality_claimed=false", decision.Value.EvidenceFeatures);
        Assert.DoesNotContain(SkillSelectionMachine.CandidatePositionScaffoldEvidence, decision.Value.EvidenceFeatures);
        var runtimeEvidence = string.Join("\n", decision.Value.EvidenceFeatures);
        Assert.DoesNotContain("P16C-PROMPT", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("source-ref", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("title", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain(trainingRequest.Prompt, runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("candidate_position", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain(ForbiddenSemanticQualityTrueEvidence, runtimeEvidence, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TaskUnderstanding_AblationFallsBackToNotReadyInsteadOfCandidatePosition()
    {
        var request = new SkillSelectionRequest(
            Prompt: "Classify the task boundary and select the plan review skill.",
            ProjectRoot: "C:/sandbox/llm_mvp_core",
            AvailableSkillIds: ["image-generation", "plan-review"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "P16C-TASK-HELDOUT");
        var trained = new SkillSelectionMachine();
        await trained.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "p16c-task-understanding-pair",
                    Request: request with { Prompt = "Classify review task boundaries before accepting the work." },
                    Chosen: new SkillSelectionDecision(["plan-review"], ["image-generation"], ["review_task_boundary"], 1.0, false, "Plan review matches the task boundary."),
                    Rejected: new SkillSelectionDecision(["image-generation"], ["plan-review"], ["visual_near_miss"], 0.1, true, "Image generation is a visual near miss."),
                    RejectedType: "adjacent_wrong_task_skill",
                    SourcePromptEntryId: "P16C-TASK-TRAIN",
                    TargetTrainingAdapter: SkillSelectionMachine.TaskUnderstandingTrainingAdapter)
            ]);
        var empty = new SkillSelectionMachine();

        var trainedDecision = trained.DecideFromTrainedState(request);
        var emptyDecision = empty.DecideFromTrainedState(request);

        Assert.True(trainedDecision.IsSuccess, trainedDecision.ErrorMessage);
        Assert.True(emptyDecision.IsSuccess, emptyDecision.ErrorMessage);
        Assert.Equal(["plan-review"], trainedDecision.Value!.SelectedSkillIds);
        Assert.Empty(emptyDecision.Value!.SelectedSkillIds);
        Assert.True(trainedDecision.Value.Confidence > emptyDecision.Value.Confidence);
        Assert.Contains(SkillSelectionMachine.TaskUnderstandingCheckpointEvidence, trainedDecision.Value.EvidenceFeatures);
        Assert.Contains(TaskUnderstandingSkillUnit.UnitExecutedEvidence, trainedDecision.Value.EvidenceFeatures);
        Assert.Contains("task_understanding:semantic_quality_claimed=false", trainedDecision.Value.EvidenceFeatures);
        Assert.Contains("NO_TRAINED_STATE_AVAILABLE", emptyDecision.Value.EvidenceFeatures);
        Assert.DoesNotContain(SkillSelectionMachine.CandidatePositionScaffoldEvidence, trainedDecision.Value.EvidenceFeatures);
        Assert.DoesNotContain(SkillSelectionMachine.CandidatePositionScaffoldEvidence, emptyDecision.Value.EvidenceFeatures);
    }

    [Fact]
    public async Task PromptContext_BaselineEvidenceIsLabelledAndContextAblationDegradesConfidence()
    {
        var trainingRequest = new SkillSelectionRequest(
            Prompt: "Use redacted host facts to choose the context audit skill.",
            ProjectRoot: "C:/sandbox/llm_mvp_core",
            AvailableSkillIds: ["context-audit", "image-generation"],
            HostFacts: new Dictionary<string, string>
            {
                ["host_type"] = "code_agent",
                ["privacy_scope"] = "redacted"
            },
            UserPromptSourceEntryId: "P16C-CONTEXT-TRAIN");
        var heldOutRequest = new SkillSelectionRequest(
            Prompt: "Choose the context audit skill using redacted host facts for this session.",
            ProjectRoot: "C:/sandbox/llm_mvp_core",
            AvailableSkillIds: ["image-generation", "context-audit"],
            HostFacts: new Dictionary<string, string>
            {
                ["host_type"] = "code_agent",
                ["privacy_scope"] = "redacted"
            },
            UserPromptSourceEntryId: "P16C-CONTEXT-HELDOUT");
        var machine = new SkillSelectionMachine();
        await machine.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "p16c-prompt-context-pair",
                    Request: trainingRequest,
                    Chosen: new SkillSelectionDecision(["context-audit"], ["image-generation"], ["redacted_host_context"], 1.0, false, "Context audit matches redacted host facts."),
                    Rejected: new SkillSelectionDecision(["image-generation"], ["context-audit"], ["visual_near_miss"], 0.1, true, "Visual generation is adjacent only."),
                    RejectedType: "adjacent_wrong_context_skill",
                    SourcePromptEntryId: "P16C-CONTEXT-TRAIN",
                    TargetTrainingAdapter: SkillSelectionMachine.PromptContextTrainingAdapter)
            ]);

        var decision = machine.DecideFromTrainedState(heldOutRequest);
        var stripped = machine.DecideFromTrainedState(heldOutRequest with
        {
            ProjectRoot = "",
            HostFacts = new Dictionary<string, string>(),
            UserPromptSourceEntryId = ""
        });

        Assert.True(decision.IsSuccess, decision.ErrorMessage);
        Assert.True(stripped.IsSuccess, stripped.ErrorMessage);
        Assert.Equal(["context-audit"], decision.Value!.SelectedSkillIds);
        Assert.Contains(SkillSelectionMachine.PromptContextCheckpointEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(PromptContextSkillSelectionUnit.UnitExecutedEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(PromptContextSkillSelectionUnit.BaselineEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains("prompt_context:semantic_quality_claimed=false", decision.Value.EvidenceFeatures);
        Assert.True(decision.Value.Confidence > stripped.Value!.Confidence);
        Assert.DoesNotContain(SkillSelectionMachine.CandidatePositionScaffoldEvidence, decision.Value.EvidenceFeatures);
    }

    [Fact]
    public async Task IntermediateChecks_KnnEvidenceStaysSupportOnlyAndRuntimeEvidenceHidesSourceMarkers()
    {
        var trainingRequest = new SkillSelectionRequest(
            Prompt: "Run an intermediate check before continuing so the next step stays aligned with evidence.",
            ProjectRoot: "C:/sandbox/llm_mvp_core",
            AvailableSkillIds: ["intermediate-checks", "image-generation"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "P16C-INTERMEDIATE-TRAIN");
        var heldOutRequest = new SkillSelectionRequest(
            Prompt: "Pause for an evidence checkpoint before continuing to the next step.",
            ProjectRoot: "C:/sandbox/llm_mvp_core",
            AvailableSkillIds: ["image-generation", "intermediate-checks"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "P16C-INTERMEDIATE-HELDOUT");
        var machine = new SkillSelectionMachine();
        await machine.TrainMachineBatchAsync(
            [
                new SkillSelectionMachinePair(
                    PairId: "p16c-intermediate-checks-pair",
                    Request: trainingRequest,
                    Chosen: new SkillSelectionDecision(["intermediate-checks"], ["image-generation"], ["mid_step_evidence_check"], 1.0, false, "Intermediate checks keep execution aligned."),
                    Rejected: new SkillSelectionDecision(["image-generation"], ["intermediate-checks"], ["visual_near_miss"], 0.1, true, "Visual generation is adjacent only."),
                    RejectedType: "adjacent_wrong_intermediate_check_skill",
                    SourcePromptEntryId: "P16C-INTERMEDIATE-TRAIN",
                    TargetTrainingAdapter: SkillSelectionMachine.IntermediateChecksTrainingAdapter)
            ]);

        var unitState = machine.ExportIntermediateChecksSkillUnitState();
        var unit = new IntermediateChecksSkillUnit();
        var features = SkillSelectionMachine.ExtractPromptUnderstandingSignals(heldOutRequest.Prompt);
        var nearest = unit.FindNearestCheckExamples(features, unitState, 3);
        var support = unit.ComputeCheckSupport(features, "intermediate-checks", unitState, 3);
        var decision = machine.DecideFromTrainedState(heldOutRequest);

        Assert.False(unitState.SemanticQualityClaimed);
        Assert.NotEmpty(nearest);
        Assert.True(support.SupportScore > 0.0);
        Assert.True(support.MatchedFeatureCount > 0);
        Assert.True(decision.IsSuccess, decision.ErrorMessage);
        Assert.Equal(["intermediate-checks"], decision.Value!.SelectedSkillIds);
        Assert.Contains(SkillSelectionMachine.IntermediateChecksCheckpointEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(IntermediateChecksSkillUnit.UnitExecutedEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains(IntermediateChecksSkillUnit.BaselineEvidence, decision.Value.EvidenceFeatures);
        Assert.Contains("intermediate_checks:knn_role=support_evidence_only", decision.Value.EvidenceFeatures);
        Assert.Contains("intermediate_checks:semantic_quality_claimed=false", decision.Value.EvidenceFeatures);
        var runtimeEvidence = string.Join("\n", decision.Value.EvidenceFeatures);
        Assert.DoesNotContain("P16C-INTERMEDIATE", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("p16c-intermediate-checks-pair", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain(trainingRequest.Prompt, runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain("candidate_position", runtimeEvidence, StringComparison.Ordinal);
        Assert.DoesNotContain(ForbiddenSemanticQualityTrueEvidence, runtimeEvidence, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LogicalFlow_KeepsRuntimeChainBaselineNoClaimAndListsExpectedSemanticUnits()
    {
        var flow = new SkillSelectionLogicalFlow(new SkillSelectionMachine());
        var request = new SkillSelectionRequest(
            Prompt: "Select review skills for a bounded work order.",
            ProjectRoot: "C:/sandbox/llm_mvp_core",
            AvailableSkillIds: ["planning-session-startup", "plan-review"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "P16C-FLOW");

        var result = await flow.ExecuteAsync(new LogicalFlowRequest<SkillSelectionRequest>(
            RunId: "p16c-flow",
            FlowId: SkillSelectionLogicalFlow.FlowId,
            ScenarioId: "p16c-skill-selection-boundary",
            Input: request,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("11", result.Value!.Metrics["semantic_unit_count"]);
        Assert.Equal(bool.FalseString, result.Value.Metrics["semantic_quality_claimed"]);
        Assert.Equal(bool.FalseString, result.Value.Metrics["candidate_position_scaffold_final_authority"]);
        Assert.Contains("SKILL_SELECTION_11_UNITS_CHECKPOINT_BOUNDARY", result.Value.EvidenceIds);
        Assert.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence, result.Value.EvidenceIds);
        Assert.Contains("CANDIDATE_POSITION_SCAFFOLD_CLASSIFIED_AS_NON_FINAL_AUTHORITY", result.Value.EvidenceIds);
        Assert.Contains("SS13_ASSET_LIFECYCLE_IS_NOT_FULL_SEMANTIC_QUALITY", result.Value.Warnings);
    }
}
