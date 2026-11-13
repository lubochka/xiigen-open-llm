// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

// Requests one compare-and-swap topology build against the current runtime revision.
public sealed record DynamicGraphTopologyApplyRequest(
    long ExpectedRevision,
    DynamicGraphBuildSignal Signal,
    TopologyBuildBudget Budget);

// Exposes one accepted, immutable topology revision together with its complete build proof.
public sealed record DynamicGraphTopologyRuntimeSnapshot(
    long Revision,
    DynamicGraphTopology Topology,
    string SignalId,
    DynamicGraphBuildSourceKind SourceKind,
    string SourceRef,
    string TraceRef,
    TopologyBudgetProof BudgetProof,
    TopologyMutationEvidence MutationEvidence);

// Reports the atomic revision outcome of one accepted, no-op, or budget-rejected build attempt.
public sealed record DynamicGraphTopologyApplyResult(
    long PreviousRevision,
    long CurrentRevision,
    bool RevisionAdvanced,
    BuildDynamicTopologyResult BuildResult,
    DynamicGraphTopologyRuntimeSnapshot? Current);

// Provides read-only access to the currently accepted topology revision.
public interface IDynamicGraphTopologySource
{
    OperationResult<DynamicGraphTopologyRuntimeSnapshot> ReadCurrent();
}

// Owns compare-and-swap topology mutation and immutable accepted-revision history.
public interface IDynamicGraphTopologyRuntimeOwner : IDynamicGraphTopologySource
{
    OperationResult<DynamicGraphTopologyApplyResult> ApplyBuildSignal(
        DynamicGraphTopologyApplyRequest request);

    OperationResult<IReadOnlyList<DynamicGraphTopologyRuntimeSnapshot>> ListRevisions();
}
