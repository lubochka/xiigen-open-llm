// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

// Binds one selected logical sub-phase to the explicitly supplied graph node that represents it.
public sealed record DynamicGraphRegistryNodeBinding(
    string LogicalSubPhaseId,
    IDynamicGraphNode Node);

// Binds one graph edge to the exact registry transitions and authority evidence that define it.
public sealed record DynamicGraphRegistryEdgeBinding(
    string EdgeId,
    string[] LogicalTransitionIds,
    GraphEdgeKind Kind,
    bool ParallelEligible,
    string AuthorityUnitId,
    string AuthorityUnitLevel,
    bool DeterministicFinalAuthority,
    string? BaselineUnitId,
    string BaselineRole,
    string[] ExcludedLeakageSignals,
    string SourceProof,
    string CutoverProof,
    string AblationProofRef);

// Requests one deterministic, strict induced-subgraph projection from the logical topology registry.
public sealed record DynamicGraphRegistryProjectionRequest(
    string TenantId,
    string DomainTag,
    string ContextId,
    string SignalId,
    string SourceRef,
    string TraceRef,
    DynamicGraphRegistryNodeBinding[] NodeBindings,
    DynamicGraphRegistryEdgeBinding[] EdgeBindings);

// Projects registry topology into a validated build signal without applying it to a runtime owner.
public interface ILogicalRegistryDynamicGraphProjector
{
    OperationResult<DynamicGraphBuildSignal> Project(
        DynamicGraphRegistryProjectionRequest request);
}
