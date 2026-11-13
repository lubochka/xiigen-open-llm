// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace OrchestratorModel.Logical;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

// Identifies one caller-selected composition session without exposing graph internals.
public sealed record DynamicGraphLogicalOrchestratorSessionDescriptor(
    string ProfileId,
    string TenantId,
    string DomainTag,
    string ContextId);

// Publishes bounded current-topology evidence without mutable topology or provenance carriers.
public sealed record DynamicGraphLogicalOrchestratorSessionResult(
    DynamicGraphLogicalOrchestratorSessionDescriptor Descriptor,
    long Revision,
    string SignalId,
    DynamicGraphBuildSourceKind SourceKind,
    int NodeCount,
    int EdgeCount,
    bool InitialRevisionAdvanced);

// Carries one bounded assistant request without exposing scenario, topology, or learning controls.
public sealed record DynamicGraphLogicalOrchestratorRunRequest(string UserRequest);

// Returns only the completed logical-assistant result and its bounded evidence projection.
public sealed record DynamicGraphLogicalOrchestratorRunResult(
    string ScenarioId,
    string SelectedFlowId,
    string OverallVerdict,
    string FinalHumanResult,
    IReadOnlyDictionary<string, string> Metrics,
    string[] EvidenceIds);

// Returns one completed run together with evidence from the exact topology snapshot used by it.
public sealed record DynamicGraphLogicalOrchestratorTopologyRunResult(
    long TopologyRevision,
    string TopologySignalId,
    DynamicGraphBuildSourceKind TopologySourceKind,
    DynamicGraphLogicalOrchestratorRunResult Run);

// Provides the only host-neutral read boundary retained after successful composition.
public interface IDynamicGraphLogicalOrchestratorCompositionSession
{
    DynamicGraphLogicalOrchestratorSessionDescriptor Descriptor { get; }

    OperationResult<DynamicGraphLogicalOrchestratorSessionResult> ReadCurrent();
}

// Applies an explicit registry projection without widening the read-only composition session.
public interface IDynamicGraphLogicalOrchestratorTopologyMutationSession
{
    OperationResult<DynamicGraphTopologyApplyResult> ApplyRegistryProjection(
        long expectedRevision,
        DynamicGraphRegistryProjectionRequest request,
        TopologyBuildBudget budget);
}

// Executes an already-composed assistant without adding execution authority to the read-only session contract.
public interface IDynamicGraphLogicalOrchestratorRunSession
{
    Task<OperationResult<DynamicGraphLogicalOrchestratorRunResult>> RunAsync(
        DynamicGraphLogicalOrchestratorRunRequest request,
        CancellationToken cancellationToken = default);
}

// Executes against one retained current-topology snapshot without widening read or mutation authority.
public interface IDynamicGraphLogicalOrchestratorTopologyRunSession
{
    Task<OperationResult<DynamicGraphLogicalOrchestratorTopologyRunResult>> RunOnCurrentTopologyAsync(
        DynamicGraphLogicalOrchestratorRunRequest request,
        CancellationToken cancellationToken = default);
}

// Composes an explicit bootstrap profile without requiring host or storage types at the boundary.
public interface IDynamicGraphLogicalOrchestratorCompositionExecutor
{
    OperationResult<IDynamicGraphLogicalOrchestratorCompositionSession> Compose(
        DynamicGraphLogicalOrchestratorBootstrapProfile profile);
}
