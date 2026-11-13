// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace OrchestratorModel.Logical;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

/// <summary>
/// Composes one factory-aware exact-target executor with the one-shot edge-authority admission
/// boundary. The composition owns no registry, topology, retry, fallback, or mutation behavior.
/// </summary>
public sealed class DynamicGraphFactorySelectedTargetExecutionComposition
{
    private readonly DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe _probe;

    private DynamicGraphFactorySelectedTargetExecutionComposition(
        DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe probe)
    {
        _probe = probe;
    }

    public static OperationResult<DynamicGraphFactorySelectedTargetExecutionComposition> Create(
        InMemoryLogicalFlowRegistry? flowRegistry,
        InMemoryLogicalUnitRegistry? unitRegistry,
        ILogicalTopologyRegistry? topologyRegistry,
        LogicalOrchestratorRequest? executionContext,
        LogicalUnitTrustState minimumTrustState = LogicalUnitTrustState.Trusted)
    {
        if (flowRegistry is null)
        {
            return OperationResult<DynamicGraphFactorySelectedTargetExecutionComposition>.Fail(
                "Dynamic graph factory selected-target flow registry is required.",
                "DYNAMIC_GRAPH_FACTORY_SELECTED_TARGET_FLOW_REGISTRY_REQUIRED");
        }

        if (unitRegistry is null)
        {
            return OperationResult<DynamicGraphFactorySelectedTargetExecutionComposition>.Fail(
                "Dynamic graph factory selected-target unit registry is required.",
                "DYNAMIC_GRAPH_FACTORY_SELECTED_TARGET_UNIT_REGISTRY_REQUIRED");
        }

        if (topologyRegistry is null)
        {
            return OperationResult<DynamicGraphFactorySelectedTargetExecutionComposition>.Fail(
                "Dynamic graph factory selected-target topology registry is required.",
                "DYNAMIC_GRAPH_FACTORY_SELECTED_TARGET_TOPOLOGY_REGISTRY_REQUIRED");
        }

        if (executionContext is null)
        {
            return OperationResult<DynamicGraphFactorySelectedTargetExecutionComposition>.Fail(
                "Dynamic graph factory selected-target execution context is required.",
                "DYNAMIC_GRAPH_FACTORY_SELECTED_TARGET_EXECUTION_CONTEXT_REQUIRED");
        }

        var factoryRequest = new LogicalFlowFactoryRequest(
            executionContext.RunId,
            executionContext.Flags!,
            unitRegistry,
            executionContext.Metadata,
            topologyRegistry)
        {
            MinimumTrustState = minimumTrustState
        };
        var executor = DynamicGraphRegistryL3SelectedTargetExecutor.CreateFactoryAware(
            flowRegistry,
            executionContext.ScenarioId,
            executionContext.UserRequest,
            factoryRequest);
        if (!executor.IsSuccess)
        {
            return executor.MapError<DynamicGraphFactorySelectedTargetExecutionComposition>();
        }

        var probe = DynamicGraphEdgeAuthoritySelectionTargetExecutionProbe.Create(executor.Value!);
        if (!probe.IsSuccess)
        {
            return probe.MapError<DynamicGraphFactorySelectedTargetExecutionComposition>();
        }

        return OperationResult<DynamicGraphFactorySelectedTargetExecutionComposition>.Ok(
            new DynamicGraphFactorySelectedTargetExecutionComposition(probe.Value!));
    }

    public Task<OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>
        RunAsync(
            DynamicGraphMeasurementTarget? target,
            EdgeAuthorityExecutionResult? authorityResult,
            DynamicGraphEdgeAuthorityNoveltyEvaluationRequest? noveltyRequest,
            CancellationToken cancellationToken = default)
        => _probe.RunAsync(target, authorityResult, noveltyRequest, cancellationToken);

    /// <summary>
    /// Executes trusted in-process association evidence that was already admitted. This path does
    /// not establish provenance, re-measure evidence, or expose an HTTP/client, aggregate-quality,
    /// DPO, training, promotion, or lifecycle boundary.
    /// </summary>
    public Task<OperationResult<DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt>>
        RunAdmittedAsync(
            DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement? association,
            CancellationToken cancellationToken = default)
        => _probe.RunAdmittedAsync(association, cancellationToken);
}
