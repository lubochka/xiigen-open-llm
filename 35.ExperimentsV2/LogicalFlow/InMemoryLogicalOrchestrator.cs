// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;

namespace ExperimentsV2.LogicalFlow;

// This orchestrator is the first assistant-level runner for logical flows.
public sealed class InMemoryLogicalOrchestrator : ILogicalOrchestrator
{
    // The map keys are scenario ids so Phase 28 can select one flow without introducing a registry.
    private readonly IReadOnlyDictionary<string, LogicalFlowBinding> _flowsByScenarioId;

    // Create an orchestrator from a caller-provided scenario-to-flow map.
    public InMemoryLogicalOrchestrator(IReadOnlyDictionary<string, LogicalFlowBinding> flowsByScenarioId)
    {
        // Store the flow map for later selection.
        _flowsByScenarioId = flowsByScenarioId;
    }

    // Run one logical flow selected by scenario id.
    public async Task<AgentInfrastructure.OperationResult<LogicalOrchestratorResult>> RunAsync(LogicalOrchestratorRequest request, CancellationToken cancellationToken = default)
    {
        // Return cancellation through the shared operation envelope.
        if (cancellationToken.IsCancellationRequested)
            return AgentInfrastructure.OperationResult<LogicalOrchestratorResult>.Fail("Cancelled");

        // Reject empty user requests before flow selection.
        if (string.IsNullOrWhiteSpace(request.UserRequest))
            return AgentInfrastructure.OperationResult<LogicalOrchestratorResult>.Fail("User request cannot be empty.");

        // Find the flow for the requested scenario.
        if (!_flowsByScenarioId.TryGetValue(request.ScenarioId, out var binding))
            return AgentInfrastructure.OperationResult<LogicalOrchestratorResult>.Fail($"No logical flow registered for scenario '{request.ScenarioId}'.");

        // Apply default flags when the caller did not provide custom flags.
        var flags = request.Flags ?? new LogicalExecutionFlags();

        // Build the flow-level request using the selected flow id from the T34 adapter for this phase.
        var flowRequest = new LogicalFlowRequest<string>(
            request.RunId,
            binding.FlowId,
            request.ScenarioId,
            request.UserRequest,
            flags,
            request.Metadata);

        // Execute the selected flow.
        var flowResult = await binding.Flow.ExecuteAsync(flowRequest, cancellationToken);

        // Return flow failures through the assistant-level envelope.
        if (!flowResult.IsSuccess)
            return AgentInfrastructure.OperationResult<LogicalOrchestratorResult>.Fail(flowResult.ErrorMessage);

        // Build the assistant-level result from the selected flow result.
        var result = new LogicalOrchestratorResult(
            request.ScenarioId,
            flowResult.Value!.FlowId,
            flowResult.Value.OverallVerdict,
            flowResult.Value.FinalHumanResult,
            flags,
            flowResult.Value);

        // Return the assistant-level result.
        return AgentInfrastructure.OperationResult<LogicalOrchestratorResult>.Ok(result);
    }
}
