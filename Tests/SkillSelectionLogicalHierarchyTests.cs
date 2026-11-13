// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;
using SmartSkillsDomain;

public sealed class SkillSelectionLogicalHierarchyTests
{
    [Fact]
    public async Task SkillSelectionLogicalFlow_ExposesFlowAndUnitsWithoutSemanticQualityClaim()
    {
        var machine = new SkillSelectionMachine();
        var flow = new SkillSelectionLogicalFlow(machine);
        var request = new SkillSelectionRequest(
            Prompt: "Review a plan and select the planning skills.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["planning-session-startup", "plan-review"],
            HostFacts: new Dictionary<string, string> { ["host"] = "codex" },
            UserPromptSourceEntryId: "UPS-CM26D-001");

        var result = await flow.ExecuteAsync(new LogicalFlowRequest<SkillSelectionRequest>(
            RunId: "cm26d-flow",
            FlowId: SkillSelectionLogicalFlow.FlowId,
            ScenarioId: "cm26d-skill-selection",
            Input: request,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(SkillSelectionLogicalFlow.FlowId, result.Value!.FlowId);
        Assert.Equal("4", result.Value.Metrics["logical_unit_count"]);
        Assert.Equal("11", result.Value.Metrics["semantic_unit_count"]);
        Assert.Equal(bool.FalseString, result.Value.Metrics["semantic_quality_claimed"]);
        Assert.Equal(bool.FalseString, result.Value.Metrics["training_run"]);
        Assert.Equal(bool.FalseString, result.Value.Metrics["candidate_position_scaffold_final_authority"]);
        Assert.Contains("SKILL_SELECTION_LOGICAL_FLOW_SOURCE_BACKED", result.Value.EvidenceIds);
        Assert.Contains("SKILL_SELECTION_11_UNITS_CHECKPOINT_BOUNDARY", result.Value.EvidenceIds);
        Assert.Contains("SKILL_SELECTION_LOGICAL_UNIT_SOURCE_BACKED", result.Value.EvidenceIds);
        Assert.Contains("SS13_ASSET_LIFECYCLE_IS_NOT_FULL_SEMANTIC_QUALITY", result.Value.Warnings);
    }

    [Fact]
    public async Task SkillSelectionLogicalOrchestrator_RequiresTypedSkillSelectionRequest()
    {
        var orchestrator = new SkillSelectionLogicalOrchestrator(
            new SkillSelectionLogicalFlow(new SkillSelectionMachine()));

        var missing = await orchestrator.RunAsync(new LogicalOrchestratorRequest(
            RunId: "cm26d-missing",
            ScenarioId: "cm26d-skill-selection",
            UserRequest: "select skills",
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>()));
        var request = new SkillSelectionRequest(
            Prompt: "Select review skills for a bounded work order.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["planning-session-startup", "plan-review"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-CM26D-002");
        var ok = await orchestrator.RunAsync(new LogicalOrchestratorRequest(
            RunId: "cm26d-ok",
            ScenarioId: "cm26d-skill-selection",
            UserRequest: request.Prompt,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                [SkillSelectionLogicalOrchestrator.RequiredMetadataKey] = request
            }));

        Assert.False(missing.IsSuccess);
        Assert.Equal("SKILL_SELECTION_REQUEST_METADATA_REQUIRED", missing.ErrorCode);
        Assert.True(ok.IsSuccess, ok.ErrorMessage);
        Assert.Equal(SkillSelectionLogicalFlow.FlowId, ok.Value!.SelectedFlowId);
        Assert.False(ok.Value.FlowResult.Summary.Passed);
        Assert.Contains("SKILL_SELECTION_LOGICAL_FLOW_SOURCE_BACKED", ok.Value.FlowResult.EvidenceIds);
    }

    [Fact]
    public async Task SkillSelectionLogicalUnits_MapAllElevenSemanticUnitsAndKeepCandidateScaffoldNonFinal()
    {
        var machine = new SkillSelectionMachine();
        var flow = new SkillSelectionLogicalFlow(machine);
        var request = new SkillSelectionRequest(
            Prompt: "Select skills for architecture review.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["planning-session-startup", "plan-review"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-CM26D-003");

        var result = await flow.ExecuteAsync(new LogicalFlowRequest<SkillSelectionRequest>(
            RunId: "cm26d-units",
            FlowId: SkillSelectionLogicalFlow.FlowId,
            ScenarioId: "cm26d-skill-selection",
            Input: request,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        foreach (var unitId in SmartSkillsSemanticUnitCatalog.ExpectedDomainIds)
            Assert.Contains($"SKILL_SELECTION_SEMANTIC_UNIT:{unitId}", result.Value!.EvidenceIds);
        Assert.Contains("checkpoint_semantic_unit_count=11", result.Value!.EvidenceIds);
        Assert.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence, result.Value.EvidenceIds);
        Assert.Contains("CANDIDATE_POSITION_SCAFFOLD_CLASSIFIED_AS_NON_FINAL_AUTHORITY", result.Value.EvidenceIds);
        Assert.Contains("STATIC_VALIDATORS_AND_JSON_REPORTS_NOT_TRAINED_AUTHORITY", result.Value.EvidenceIds);
    }

    [Fact]
    public async Task SkillSelectionMachine_GetOrchestrator_ReturnsTrainedLogicalHierarchyAdapter()
    {
        var machine = new SkillSelectionMachine();
        var logical = await machine.GetOrchestratorAsync();
        var request = new SkillSelectionRequest(
            Prompt: "Select review skills for a trained logical hierarchy.",
            ProjectRoot: "C:/Projects/llm_mvp_core",
            AvailableSkillIds: ["planning-session-startup", "plan-review"],
            HostFacts: new Dictionary<string, string>(),
            UserPromptSourceEntryId: "UPS-CM26D-004");
        var run = await logical.Value!.RunAsync(new LogicalOrchestratorRequest(
            RunId: "cm26d-trained-logical",
            ScenarioId: "cm26d-skill-selection",
            UserRequest: request.Prompt,
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>
            {
                [SkillSelectionLogicalOrchestrator.RequiredMetadataKey] = request
            }));

        Assert.True(logical.IsSuccess, logical.ErrorMessage);
        Assert.IsType<SkillSelectionLogicalOrchestrator>(logical.Value);
        Assert.True(run.IsSuccess, run.ErrorMessage);
        Assert.Equal(SkillSelectionLogicalFlow.FlowId, run.Value!.SelectedFlowId);
    }
}
