// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;

namespace ExperimentsV2.LogicalFlow;

// This adapter exposes the proven T34 experiment as an executable logical flow.
public sealed class T34LogicalFlowAdapter : ILogicalFlow<string>
{
    // This flow id matches the T34 experiment id so evidence remains traceable.
    public const string FlowId = T34PhaseTransitionProof.Id;

    // This scenario id matches the only scenario T34 currently proves.
    public const string ScenarioId = T34PhaseTransitionProof.ScenarioId;

    // Execute the T34 flow through the logical-flow interface.
    public Task<AgentInfrastructure.OperationResult<LogicalFlowResult>> ExecuteAsync(LogicalFlowRequest<string> request, CancellationToken cancellationToken = default)
    {
        // Return cancellation through the shared operation envelope.
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail("Cancelled"));

        // Refuse requests for a different flow id so the adapter cannot be misbound.
        if (!string.Equals(request.FlowId, FlowId, StringComparison.Ordinal))
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail($"Unsupported flow id '{request.FlowId}'."));

        // Refuse requests for a different scenario id so the adapter stays scoped to the accepted T34 oracle.
        if (!string.Equals(request.ScenarioId, ScenarioId, StringComparison.Ordinal))
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail($"Unsupported scenario id '{request.ScenarioId}'."));

        // Build the deterministic T34 evidence through the unchanged experiment.
        var evidence = new T34PhaseTransitionProof().BuildEvidence();

        // Return T34 failures through the logical-flow envelope.
        if (!evidence.IsSuccess)
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(evidence.ErrorMessage));

        // Read the experiment summary so it can be copied into the runtime-neutral flow summary.
        var experimentSummary = evidence.Value!.Summary;

        // Convert the experiment-specific summary into the runtime-owned logical-flow summary shape.
        var flowSummary = new LogicalFlowSummary(
            experimentSummary.ExperimentId,
            experimentSummary.Tier,
            experimentSummary.BaselinePrimary,
            experimentSummary.EnhancedPrimary,
            experimentSummary.PrimaryMetricName,
            experimentSummary.LowerIsBetter,
            experimentSummary.Passed,
            experimentSummary.ShouldScaleUp,
            experimentSummary.NextAction,
            experimentSummary.ScalingSignal,
            experimentSummary.ScalingSignalNote,
            experimentSummary.Notes,
            experimentSummary.BaselineTimeMs,
            experimentSummary.EnhancedTimeMs);

        // Normalize the T34 metrics into a simple string dictionary for flow-level comparison.
        var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["transition_success"] = evidence.Value.Metrics.TransitionSuccess.ToString(),
            ["concern_count"] = evidence.Value.Metrics.ConcernCount.ToString(),
            ["block_count"] = evidence.Value.Metrics.BlockCount.ToString(),
            ["pass_count"] = evidence.Value.Metrics.PassCount.ToString(),
            ["domain_packet_count"] = evidence.Value.Metrics.DomainPacketCount.ToString(),
            ["formal_translation_count"] = evidence.Value.Metrics.FormalTranslationCount.ToString(),
            ["scenario_count"] = evidence.Value.Metrics.ScenarioCount.ToString(),
            ["human_result_sections_present"] = evidence.Value.Metrics.HumanResultSectionsPresent
        };

        // Build the normalized flow result from the T34 evidence.
        var result = new LogicalFlowResult(
            FlowId,
            ScenarioId,
            evidence.Value.OverallVerdict,
            evidence.Value.FinalHumanResult,
            flowSummary,
            metrics,
            [evidence.Value.RunId],
            []);

        // Return the normalized logical-flow result.
        return Task.FromResult(AgentInfrastructure.OperationResult<LogicalFlowResult>.Ok(result));
    }
}
