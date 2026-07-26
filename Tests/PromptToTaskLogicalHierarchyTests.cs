// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;
using SmartSkillsDomain;

public sealed class PromptToTaskLogicalHierarchyTests
{
    [Fact]
    public async Task PromptToTaskLogicalHierarchy_AdapterExposesOrchestrator()
    {
        ILearningMachine<PromptToTaskDomain> adapter = new PromptToTaskLearningMachineAdapter();

        var result = await adapter.GetOrchestratorAsync();

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.IsType<PromptToTaskLogicalOrchestrator>(result.Value);
    }

    [Fact]
    public async Task PromptToTaskLogicalHierarchy_OrchestratorRunsFourSourceBackedUnits()
    {
        var adapter = new PromptToTaskLearningMachineAdapter();
        var orchestrator = (PromptToTaskLogicalOrchestrator)(await adapter.GetOrchestratorAsync()).Value!;
        var promptRequest = CreateRequest("ptt-hierarchy-001", "Create one bounded work order.");
        var request = new LogicalOrchestratorRequest(
            ScenarioId: "scenario-ptt-hierarchy",
            RunId: "run-ptt-hierarchy",
            UserRequest: promptRequest.RawPrompt,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                [PromptToTaskLogicalOrchestrator.RequiredMetadataKey] = promptRequest
            });

        var result = await orchestrator.RunAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(PromptToTaskLogicalFlow.FlowId, result.Value!.SelectedFlowId);
        Assert.Equal(PromptToTaskMachine.NotTrainedStatus, result.Value.OverallVerdict);
        Assert.Equal(PromptToTaskMachine.NotReadyTaskType, result.Value.FinalHumanResult);
        Assert.Equal("4", result.Value.FlowResult.Metrics["logical_unit_count"]);
        Assert.Equal(bool.FalseString, result.Value.FlowResult.Metrics["semantic_quality_claimed"]);
        Assert.Contains("PROMPT_TO_TASK_LOGICAL_FLOW_SOURCE_BACKED", result.Value.FlowResult.EvidenceIds);
        Assert.Contains("PROMPT_TO_TASK_UNIT_ROLE:intent_extraction", result.Value.FlowResult.EvidenceIds);
        Assert.Contains("PROMPT_TO_TASK_UNIT_ROLE:constraint_extraction", result.Value.FlowResult.EvidenceIds);
        Assert.Contains("PROMPT_TO_TASK_UNIT_ROLE:deliverable_extraction", result.Value.FlowResult.EvidenceIds);
        Assert.Contains("PROMPT_TO_TASK_UNIT_ROLE:authority_boundary_check", result.Value.FlowResult.EvidenceIds);
    }

    [Fact]
    public async Task PromptToTaskLogicalHierarchy_MissingMetadataFailsHonestly()
    {
        var orchestrator = new PromptToTaskLogicalOrchestrator(new PromptToTaskLogicalFlow(new PromptToTaskMachine()));
        var request = new LogicalOrchestratorRequest(
            ScenarioId: "scenario-ptt-missing",
            RunId: "run-ptt-missing",
            UserRequest: "No metadata object.",
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>());

        var result = await orchestrator.RunAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("PROMPT_TO_TASK_REQUEST_METADATA_REQUIRED", result.ErrorCode);
    }

    [Fact]
    public async Task PromptToTaskLogicalHierarchy_TrainAndEvaluateStayHonest()
    {
        ILearningMachine<PromptToTaskDomain> adapter = new PromptToTaskLearningMachineAdapter();
        var batch = new LearningBatch<PromptToTaskDomain>(
            BatchId: "ptt-hierarchy-batch",
            DomainTag: PromptToTaskMachine.DomainTag,
            TrainingItems:
            [
                new LearningTrainingItem("row-001", "prompt-ref", "chosen-ref", "rejected-ref")
            ],
            SourceIds: ["source-ref"]);
        var eval = new LearningEvaluationSet<PromptToTaskDomain>(
            EvaluationId: "ptt-hierarchy-eval",
            DomainTag: PromptToTaskMachine.DomainTag,
            EvaluationItems:
            [
                new LearningEvaluationItem("eval-001", "prompt-ref", "expected-ref")
            ]);

        var train = await adapter.TrainBatchAsync(batch);
        var evaluation = await adapter.EvaluateAsync(eval);

        Assert.False(train.IsSuccess);
        Assert.Equal("PROMPT_TO_TASK_TRAINING_NOT_IMPLEMENTED_IN_LOGICAL_ADAPTER", train.ErrorCode);
        Assert.True(evaluation.IsSuccess, evaluation.ErrorMessage);
        Assert.False(evaluation.Value!.PassesGate);
        Assert.Equal(0.0, evaluation.Value.Accuracy);
        Assert.Contains("semantic quality remains unclaimed", evaluation.Value.GateNote);
    }

    private static PromptToTaskRequest CreateRequest(string promptId, string rawPrompt)
        => new(
            PromptId: promptId,
            RawPrompt: rawPrompt,
            RawPromptSha256: $"sha256:{promptId}",
            PromptProvenance: "conversation:cm-26a1",
            UserLanguage: "en",
            ExplicitConstraints: ["<=15 minutes", "no broad tests", "no host repos"],
            ContextFields: new Dictionary<string, string>
            {
                ["work_order"] = "CM-26A1"
            });
}
