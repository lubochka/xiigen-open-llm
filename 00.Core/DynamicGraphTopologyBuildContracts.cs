// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

// Identifies the deterministic source class that proposed one topology snapshot.
public enum DynamicGraphBuildSourceKind
{
    DeterministicBootstrap,
    LogicalRegistryProjection,
    MachineTopologyProposal
}

// Carries one proposed topology mutation and its complete auditable source identity.
public sealed record DynamicGraphBuildSignal(
    string SignalId,
    DynamicGraphBuildSourceKind SourceKind,
    DynamicGraphTopology ProposedTopology,
    DynamicGraphTopology? PreviousTopology,
    string SourceRef,
    string TraceRef);

// Bounds structural topology size and breadth without introducing execution or learning policy.
public sealed record TopologyBuildBudget(
    int MaxNodes,
    int MaxEdges,
    int MaxDepth,
    int MaxParallelBranches);

// Records the proposed topology's measured dimensions and the fixed-order exceeded dimension set.
public sealed record TopologyBudgetProof(
    int MaxNodes,
    int ActualNodes,
    int MaxEdges,
    int ActualEdges,
    int MaxDepth,
    int ActualDepth,
    int MaxParallelBranches,
    int ActualParallelBranches,
    bool WithinBudget,
    IReadOnlyList<string> ExceededDimensions);

// Describes the complete ID-based difference between the previous and proposed topology snapshots.
public sealed record TopologyMutationEvidence(
    string SignalId,
    DynamicGraphBuildSourceKind SourceKind,
    string SourceRef,
    string TraceRef,
    IReadOnlyList<string> ProposedAddedNodeIds,
    IReadOnlyList<string> ProposedRemovedNodeIds,
    IReadOnlyList<string> ProposedUpdatedNodeIds,
    IReadOnlyList<string> ProposedAddedEdgeIds,
    IReadOnlyList<string> ProposedRemovedEdgeIds,
    IReadOnlyList<string> ProposedUpdatedEdgeIds,
    IReadOnlyList<string> ProposedReorderedNodeIds,
    bool MutationApplied);

// Returns either the accepted snapshot or the retained previous snapshot together with auditable proof.
public sealed record BuildDynamicTopologyResult(
    DynamicGraphTopology? Topology,
    TopologyBudgetProof BudgetProof,
    string? FailureReason,
    TopologyMutationEvidence MutationEvidence);

// Defines the synchronous deterministic topology-construction boundary.
public interface IDynamicGraphTopologyBuilder
{
    OperationResult<BuildDynamicTopologyResult> BuildDynamicTopology(
        DynamicGraphBuildSignal signal,
        TopologyBuildBudget budget);
}
