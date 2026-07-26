// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;
using SmartSkillsDomain;

public sealed class ResultGapLogicalHierarchyTests
{
    [Fact]
    public async Task ResultGapLogicalHierarchy_AdapterExposesOrchestrator()
    {
        ILearningMachine<ResultGapDomain> adapter = new ResultGapLearningMachineAdapter();

        var result = await adapter.GetOrchestratorAsync();

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.IsType<ResultGapLogicalOrchestrator>(result.Value);
    }

    [Fact]
    public async Task ResultGapLogicalHierarchy_OrchestratorRunsFourSourceBackedUnits()
    {
        var adapter = new ResultGapLearningMachineAdapter();
        var orchestrator = (ResultGapLogicalOrchestrator)(await adapter.GetOrchestratorAsync()).Value!;
        var resultGapRequest = CreateRequest();
        var request = new LogicalOrchestratorRequest(
            ScenarioId: "scenario-result-gap-hierarchy",
            RunId: "run-result-gap-hierarchy",
            UserRequest: "Review the AI output for a missing skill pattern.",
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                [ResultGapLogicalOrchestrator.RequiredMetadataKey] = resultGapRequest
            });

        var result = await orchestrator.RunAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(ResultGapLogicalFlow.FlowId, result.Value!.SelectedFlowId);
        Assert.Equal("RESULT_GAP_DETECTED_BOOTSTRAP_FIXTURE_CONTRACT_SUPPORT", result.Value.OverallVerdict);
        Assert.Equal("plan-review", result.Value.FinalHumanResult);
        Assert.Equal("4", result.Value.FlowResult.Metrics["logical_unit_count"]);
        Assert.Equal(bool.FalseString, result.Value.FlowResult.Metrics["semantic_quality_claimed"]);
        Assert.Equal(bool.FalseString, result.Value.FlowResult.Metrics["training_run"]);
        Assert.Equal(bool.FalseString, result.Value.FlowResult.Metrics["bootstrap_fixture_final_authority"]);
        Assert.Equal(SkillSelectionMachine.ResultGapUnitId, result.Value.FlowResult.Metrics["skill_selection_result_gap_checkpoint_unit"]);
        Assert.Contains("RESULT_GAP_LOGICAL_FLOW_SOURCE_BACKED", result.Value.FlowResult.EvidenceIds);
        Assert.Contains("RESULT_GAP_UNIT_ROLE:bootstrap_fixture_gap_detection", result.Value.FlowResult.EvidenceIds);
        Assert.Contains("RESULT_GAP_UNIT_ROLE:checkpoint_backed_result_gap_unit_boundary", result.Value.FlowResult.EvidenceIds);
        Assert.Contains("RESULT_GAP_UNIT_ROLE:user_local_adaptive_signal_boundary", result.Value.FlowResult.EvidenceIds);
        Assert.Contains("RESULT_GAP_UNIT_ROLE:bootstrap_not_trained_authority_check", result.Value.FlowResult.EvidenceIds);
        Assert.Contains(SkillSelectionMachine.ResultGapCheckpointEvidence, result.Value.FlowResult.EvidenceIds);
        Assert.Contains("BOOTSTRAP_EXACT_EXPECTED_SKILL_COMPARISON_IS_NOT_FINAL_SEMANTIC_AUTHORITY", result.Value.FlowResult.EvidenceIds);
        Assert.Contains("NO_REGEX_BM25_STATIC_LOOKUP_ADDED_AS_RESULT_GAP_AUTHORITY", result.Value.FlowResult.EvidenceIds);
    }

    [Fact]
    public async Task ResultGapLogicalHierarchy_MissingMetadataFailsHonestly()
    {
        var orchestrator = new ResultGapLogicalOrchestrator(new ResultGapLogicalFlow(new ResultGapMachine(), new SkillSelectionMachine()));
        var request = new LogicalOrchestratorRequest(
            ScenarioId: "scenario-result-gap-missing",
            RunId: "run-result-gap-missing",
            UserRequest: "No metadata object.",
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>());

        var result = await orchestrator.RunAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("RESULT_GAP_REQUEST_METADATA_REQUIRED", result.ErrorCode);
    }

    [Fact]
    public async Task ResultGapLogicalHierarchy_TrainEvaluateAndExportStayHonest()
    {
        ILearningMachine<ResultGapDomain> adapter = new ResultGapLearningMachineAdapter();
        var batch = new LearningBatch<ResultGapDomain>(
            BatchId: "result-gap-hierarchy-batch",
            DomainTag: ResultGapLearningMachineAdapter.DomainTag,
            TrainingItems:
            [
                new LearningTrainingItem("row-001", "output-ref", "chosen-skill-ref", "rejected-skill-ref")
            ],
            SourceIds: ["source-ref"]);
        var eval = new LearningEvaluationSet<ResultGapDomain>(
            EvaluationId: "result-gap-hierarchy-eval",
            DomainTag: ResultGapLearningMachineAdapter.DomainTag,
            EvaluationItems:
            [
                new LearningEvaluationItem("eval-001", "output-ref", "expected-gap-ref")
            ]);

        var train = await adapter.TrainBatchAsync(batch);
        var evaluation = await adapter.EvaluateAsync(eval);
        var export = await adapter.ExportAsync();

        Assert.False(train.IsSuccess);
        Assert.Equal("RESULT_GAP_TRAINING_NOT_IMPLEMENTED_IN_LOGICAL_ADAPTER", train.ErrorCode);
        Assert.True(evaluation.IsSuccess, evaluation.ErrorMessage);
        Assert.False(evaluation.Value!.PassesGate);
        Assert.Equal(0.0, evaluation.Value.Accuracy);
        Assert.Contains("semantic quality remains unclaimed", evaluation.Value.GateNote);
        Assert.False(export.IsSuccess);
        Assert.Equal("RESULT_GAP_STANDALONE_EXPORT_NOT_IMPLEMENTED_IN_LOGICAL_ADAPTER", export.ErrorCode);
    }

    private static ResultGapRequest CreateRequest()
        => new(
            Prompt: "redacted prompt ref: result-gap-hierarchy",
            SelectedSkillIds: ["implementation"],
            AiOutput: "redacted output ref: missing plan review",
            ExpectedMissingSkillId: "plan-review",
            ObservedMissingSkillId: "",
            UserPromptSourceEntryId: "source-ledger:cm-26c");
}
