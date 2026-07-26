// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;
using SmartSkillsDomain;
using System.Text.Json;

public sealed class OrchestrationReasoningLogicalHierarchyTests
{
    [Fact]
    public async Task OrchestrationReasoningLogicalHierarchy_AdapterExposesOrchestrator()
    {
        ILearningMachine<OrchestrationReasoningDomain> adapter = new OrchestrationReasoningLearningMachineAdapter();

        var result = await adapter.GetOrchestratorAsync();

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.IsType<OrchestrationReasoningLogicalOrchestrator>(result.Value);
    }

    [Fact]
    public async Task OrchestrationReasoningLogicalHierarchy_OrchestratorRunsFourSourceBackedUnits()
    {
        var adapter = new OrchestrationReasoningLearningMachineAdapter();
        var orchestrator = (OrchestrationReasoningLogicalOrchestrator)(await adapter.GetOrchestratorAsync()).Value!;
        var reasoningRequest = CreateRequest();
        var request = new LogicalOrchestratorRequest(
            ScenarioId: "scenario-or-hierarchy",
            RunId: "run-or-hierarchy",
            UserRequest: "Plan a bounded implementation review.",
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                [OrchestrationReasoningLogicalOrchestrator.RequiredMetadataKey] = reasoningRequest
            });

        var result = await orchestrator.RunAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(OrchestrationReasoningLogicalFlow.FlowId, result.Value!.SelectedFlowId);
        Assert.Equal(OrchestrationReasoningMachine.NotTrainedStatus, result.Value.OverallVerdict);
        Assert.Equal(OrchestrationReasoningMachine.NoInferenceScenarioId, result.Value.FinalHumanResult);
        Assert.Equal("4", result.Value.FlowResult.Metrics["logical_unit_count"]);
        Assert.Equal(bool.FalseString, result.Value.FlowResult.Metrics["semantic_quality_claimed"]);
        Assert.Contains("ORCHESTRATION_REASONING_LOGICAL_FLOW_SOURCE_BACKED", result.Value.FlowResult.EvidenceIds);
        Assert.Contains("ORCHESTRATION_REASONING_UNIT_ROLE:scenario_planning", result.Value.FlowResult.EvidenceIds);
        Assert.Contains("ORCHESTRATION_REASONING_UNIT_ROLE:gate_family_planning", result.Value.FlowResult.EvidenceIds);
        Assert.Contains("ORCHESTRATION_REASONING_UNIT_ROLE:risk_policy_review", result.Value.FlowResult.EvidenceIds);
        Assert.Contains("ORCHESTRATION_REASONING_UNIT_ROLE:authority_boundary_check", result.Value.FlowResult.EvidenceIds);
        Assert.Contains("NO_RANKER_TEMPLATE_OR_FLOW_INDEX_USED", result.Value.FlowResult.EvidenceIds);
    }

    [Fact]
    public async Task OrchestrationReasoningLogicalHierarchy_MissingMetadataFailsHonestly()
    {
        var orchestrator = new OrchestrationReasoningLogicalOrchestrator(new OrchestrationReasoningLogicalFlow(new OrchestrationReasoningMachine()));
        var request = new LogicalOrchestratorRequest(
            ScenarioId: "scenario-or-missing",
            RunId: "run-or-missing",
            UserRequest: "No metadata object.",
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>());

        var result = await orchestrator.RunAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("ORCHESTRATION_REASONING_REQUEST_METADATA_REQUIRED", result.ErrorCode);
    }

    [Fact]
    public async Task OrchestrationReasoningLogicalHierarchy_TrainAndEvaluateStayHonest()
    {
        var pattern = CreateLearnedPattern();
        var request = CreateRequest();
        ILearningMachine<OrchestrationReasoningDomain> adapter = new OrchestrationReasoningLearningMachineAdapter();
        var batch = new LearningBatch<OrchestrationReasoningDomain>(
            BatchId: "or-hierarchy-batch",
            DomainTag: OrchestrationReasoningMachine.DomainTag,
            TrainingItems:
            [
                new LearningTrainingItem(
                    "row-001",
                    request.RequestId,
                    JsonSerializer.Serialize(pattern, JsonOptions),
                    "rejected-plan-ref")
            ],
            SourceIds: ["source-ref"]);
        var eval = new LearningEvaluationSet<OrchestrationReasoningDomain>(
            EvaluationId: "or-hierarchy-eval",
            DomainTag: OrchestrationReasoningMachine.DomainTag,
            EvaluationItems:
            [
                new LearningEvaluationItem(
                    "eval-001",
                    JsonSerializer.Serialize(request, JsonOptions),
                    pattern.ScenarioRef)
            ]);

        var train = await adapter.TrainBatchAsync(batch);
        var evaluation = await adapter.EvaluateAsync(eval);

        Assert.True(train.IsSuccess, train.ErrorMessage);
        Assert.Equal(1, train.Value!.RowsTrained);
        Assert.True(evaluation.IsSuccess, evaluation.ErrorMessage);
        Assert.True(evaluation.Value!.PassesGate);
        Assert.Equal(1.0, evaluation.Value.Accuracy);
        Assert.Contains("semantic quality remains unclaimed", evaluation.Value.GateNote);
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private static OrchestrationReasoningLearnedPattern CreateLearnedPattern()
        => new(
            PatternId: "or-hierarchy-learned-pattern-001",
            ScenarioRef: "or-hierarchy-learned-scenario-001",
            RequiredTaskTypes: ["bounded_work_order"],
            RequiredConstraintSignals: ["no broad tests"],
            RequiredRequirementSignals: ["repair hierarchy evidence"],
            RequiredDeliverableSignals: ["source adapter"],
            RequiredEvidenceSignals: ["OrchestrationReasoningLogicalHierarchyTests"],
            PhaseFamilies: ["trained-planner"],
            GateFamilies: ["focused-tests"],
            AssetFamilies: ["common-guide-state"],
            HostCheckSets: ["core-only"],
            RiskFlags: ["semantic_quality_claimed_false"],
            NextGranularizationActions: ["run heldout gate"],
            Confidence: 0.91);

    private static OrchestrationReasoningRequest CreateRequest()
        => new(
            RequestId: "or-hierarchy-request-001",
            Task: new StructuredTaskContract(
                PromptId: "prompt-or-hierarchy-001",
                RawPromptSha256: "sha256:or-hierarchy-001",
                UserLanguage: "en",
                TaskType: "bounded_work_order",
                ExplicitUserRequirements: ["repair hierarchy evidence", "do not claim semantic quality"],
                Constraints: ["no broad tests", "no host repos", "no training"],
                Deliverables: ["source adapter", "focused tests"],
                ForbiddenActions: ["FlowIndex", "PromptFlowIndex", "template plan generation"],
                RequiredHosts: ["claude", "cursor", "codex"],
                EvidenceNeeds: ["OrchestrationReasoningLogicalHierarchyTests"],
                SourceProvenance: new Dictionary<string, string>
                {
                    ["work_order"] = "CM-26B",
                    ["source_root_id"] = "llm_mvp_core"
                },
                SplitId: "split-or-hierarchy-001"),
            GuideDerivedStateRefs: [],
            CorpusBundleIds: ["corpus:orchestration-reasoning:cm26b"],
            AdaptiveContextRefs: new Dictionary<string, string>(),
            RuntimePolicyRefs: new Dictionary<string, string>
            {
                ["deterministic_shortcut"] = "forbidden"
            });
}
