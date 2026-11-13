// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class SkillSelectionOrchestrator : ILogicalOrchestrator
{
    public Task<OperationResult<SkillSelectionDecision>> DecideAsync(
        SkillSelectionRequest request,
        CancellationToken ct = default)
    {
        var decision = new SkillSelectionDecision(
            SelectedSkillIds: [],
            RejectedSkillIds: request.AvailableSkillIds,
            EvidenceFeatures: ["S1_CONTRACT_ONLY", "NO_TRAINED_SEMANTIC_SELECTION"],
            Confidence: 0.0,
            RequiresSecondPass: true,
            DecisionText: "Phase39 S.1 contract only: no trained semantic skill selection has run.");

        return Task.FromResult(OperationResult<SkillSelectionDecision>.Ok(decision));
    }

    public async Task<OperationResult<LogicalOrchestratorResult>> RunAsync(
        LogicalOrchestratorRequest request,
        CancellationToken cancellationToken = default)
    {
        var skillRequest = new SkillSelectionRequest(
            Prompt: request.UserRequest,
            ProjectRoot: request.Metadata.TryGetValue("project_root", out var root) ? root?.ToString() ?? "" : "",
            AvailableSkillIds: [],
            HostFacts: request.Metadata.ToDictionary(
                item => item.Key,
                item => item.Value?.ToString() ?? string.Empty,
                StringComparer.Ordinal),
            UserPromptSourceEntryId: request.Metadata.TryGetValue("user_prompt_source_entry_id", out var source)
                ? source?.ToString() ?? ""
                : "");

        var decision = await DecideAsync(skillRequest, cancellationToken);
        if (!decision.IsSuccess)
        {
            return decision.MapError<LogicalOrchestratorResult>();
        }

        var flags = request.Flags ?? new LogicalExecutionFlags();
        var flowResult = new LogicalFlowResult(
            FlowId: "skill-selection-contract-only",
            ScenarioId: request.ScenarioId,
            OverallVerdict: "CONTRACT_ONLY",
            FinalHumanResult: decision.Value!.DecisionText,
            Summary: new LogicalFlowSummary(
                SummaryId: $"{request.RunId}-skill-selection-contract-summary",
                Tier: 0,
                BaselinePrimary: 0.0,
                EnhancedPrimary: 0.0,
                PrimaryMetricName: "semantic_selection_accuracy",
                LowerIsBetter: false,
                Passed: false,
                ShouldScaleUp: false,
                NextAction: "Run Phase39 trainable semantic selection gates.",
                ScalingSignal: 0.0,
                ScalingSignalNote: "S.1 contract scaffold only.",
                Notes: "No trained semantic skill-selection unit was invoked.",
                BaselineTimeMs: 0.0,
                EnhancedTimeMs: 0.0),
            Metrics: new Dictionary<string, string>
            {
                ["contract_only"] = bool.TrueString,
                ["requires_second_pass"] = decision.Value.RequiresSecondPass.ToString()
            },
            EvidenceIds: decision.Value.EvidenceFeatures.ToArray(),
            Warnings: ["S1_CONTRACT_ONLY_NOT_TRAINED"]);

        var result = new LogicalOrchestratorResult(
            ScenarioId: request.ScenarioId,
            SelectedFlowId: flowResult.FlowId,
            OverallVerdict: flowResult.OverallVerdict,
            FinalHumanResult: flowResult.FinalHumanResult,
            AppliedFlags: flags,
            FlowResult: flowResult)
        {
            Warnings = flowResult.Warnings
        };

        return OperationResult<LogicalOrchestratorResult>.Ok(result);
    }
}
