// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("OrchestratorModel")]
[assembly: InternalsVisibleTo("LlmFromScratch.Tests")]

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

// Owns one process-local, compare-and-swap topology timeline without persistence or execution authority.
public sealed class InMemoryDynamicGraphTopologyRuntime : IDynamicGraphTopologyRuntimeOwner
{
    private const string DependencyRequiredCode = "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_DEPENDENCY_REQUIRED";
    private const string RequestRequiredCode = "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_REQUEST_REQUIRED";
    private const string ExpectedRevisionInvalidCode = "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_EXPECTED_REVISION_INVALID";
    private const string SignalPreviousForbiddenCode = "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_SIGNAL_PREVIOUS_FORBIDDEN";
    private const string SignalRequiredCode = "DYNAMIC_GRAPH_TOPOLOGY_BUILD_SIGNAL_REQUIRED";
    private const string SignalInvalidCode = "DYNAMIC_GRAPH_TOPOLOGY_BUILD_SIGNAL_INVALID";
    private const string ScopeMismatchCode = "DYNAMIC_GRAPH_TOPOLOGY_BUILD_SCOPE_MISMATCH";
    private const string BudgetRequiredCode = "DYNAMIC_GRAPH_TOPOLOGY_BUILD_BUDGET_REQUIRED";
    private const string BudgetInvalidCode = "DYNAMIC_GRAPH_TOPOLOGY_BUILD_BUDGET_INVALID";
    private const string RevisionConflictCode = "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_REVISION_CONFLICT";
    private const string CurrentNotFoundCode = "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_CURRENT_NOT_FOUND";
    private const string BuilderResultRequiredCode = "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_BUILDER_RESULT_REQUIRED";
    private const string BuilderFailedCode = "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_BUILDER_FAILED";
    private const string BuilderResultInvalidCode = "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_BUILDER_RESULT_INVALID";
    private const string ProposedTopologyInvalidCode = "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_PROPOSED_TOPOLOGY_INVALID";
    private const string TopologyRequiredCode = "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_TOPOLOGY_REQUIRED";
    private const string ExceptionCode = "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_EXCEPTION";
    private const string BudgetExceededCode = "DYNAMIC_GRAPH_TOPOLOGY_BUILD_BUDGET_EXCEEDED";
    private const string RestoreStateRequiredCode =
        "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_RESTORE_STATE_REQUIRED";
    private const string RestoreStateInvalidCode =
        "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_RESTORE_STATE_INVALID";
    private const string RestoreAlreadyConsumedCode =
        "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_RESTORE_ALREADY_CONSUMED";
    private const int MaximumParallelBranches = 8;

    private static readonly string[] BudgetDimensions =
        ["nodes", "edges", "depth", "parallel_branches"];

    private readonly object _sync = new();
    private readonly IDynamicGraphTopologyBuilder? _builder;
    private readonly List<DynamicGraphTopologyRuntimeSnapshot> _history = [];
    private DynamicGraphTopologyRuntimeSnapshot? _current;
    private bool _restoreConsumed;

    public InMemoryDynamicGraphTopologyRuntime(IDynamicGraphTopologyBuilder builder)
        => _builder = builder;

    public OperationResult<DynamicGraphTopologyRuntimeSnapshot> ReadCurrent()
    {
        if (_builder is null)
            return DependencyFailure<DynamicGraphTopologyRuntimeSnapshot>();

        try
        {
            lock (_sync)
            {
                return _current is null
                    ? Failure<DynamicGraphTopologyRuntimeSnapshot>(
                        "No dynamic graph topology revision has been accepted.",
                        CurrentNotFoundCode)
                    : OperationResult<DynamicGraphTopologyRuntimeSnapshot>.Ok(CopySnapshot(_current));
            }
        }
        catch (Exception exception) when (IsNonFatalRuntimeException(exception))
        {
            return UnexpectedFailure<DynamicGraphTopologyRuntimeSnapshot>();
        }
    }

    public OperationResult<IReadOnlyList<DynamicGraphTopologyRuntimeSnapshot>> ListRevisions()
    {
        if (_builder is null)
            return DependencyFailure<IReadOnlyList<DynamicGraphTopologyRuntimeSnapshot>>();

        try
        {
            lock (_sync)
            {
                IReadOnlyList<DynamicGraphTopologyRuntimeSnapshot> snapshots = Array.AsReadOnly(
                    _history.Select(CopySnapshot).ToArray());
                return OperationResult<IReadOnlyList<DynamicGraphTopologyRuntimeSnapshot>>.Ok(snapshots);
            }
        }
        catch (Exception exception) when (IsNonFatalRuntimeException(exception))
        {
            return UnexpectedFailure<IReadOnlyList<DynamicGraphTopologyRuntimeSnapshot>>();
        }
    }

    public OperationResult<DynamicGraphTopologyApplyResult> ApplyBuildSignal(
        DynamicGraphTopologyApplyRequest request)
    {
        if (_builder is null)
            return DependencyFailure<DynamicGraphTopologyApplyResult>();

        if (request is null)
        {
            return Failure<DynamicGraphTopologyApplyResult>(
                "Dynamic graph topology apply request is required.",
                RequestRequiredCode);
        }

        if (request.ExpectedRevision < 0)
        {
            return Failure<DynamicGraphTopologyApplyResult>(
                "Expected dynamic graph topology revision must be non-negative.",
                ExpectedRevisionInvalidCode);
        }

        if (request.Signal is not null && request.Signal.PreviousTopology is not null)
        {
            return Failure<DynamicGraphTopologyApplyResult>(
                "Caller-supplied previous topology is forbidden.",
                SignalPreviousForbiddenCode);
        }

        if (request.Signal is null)
        {
            return Failure<DynamicGraphTopologyApplyResult>(
                "Dynamic graph topology build signal is required.",
                SignalRequiredCode);
        }

        if (!IsBuildSignalShapeValid(request.Signal))
        {
            return Failure<DynamicGraphTopologyApplyResult>(
                "Dynamic graph topology build signal is invalid.",
                SignalInvalidCode);
        }

        if (request.Budget is null)
        {
            return Failure<DynamicGraphTopologyApplyResult>(
                "Dynamic graph topology build budget is required.",
                BudgetRequiredCode);
        }

        if (!IsBuildBudgetValid(request.Budget))
        {
            return Failure<DynamicGraphTopologyApplyResult>(
                "Dynamic graph topology build budget is invalid.",
                BudgetInvalidCode);
        }

        try
        {
            lock (_sync)
            {
                var previousRevision = _current?.Revision ?? 0L;
                if (request.ExpectedRevision != previousRevision)
                {
                    return Failure<DynamicGraphTopologyApplyResult>(
                        "Expected dynamic graph topology revision does not match the current revision.",
                        RevisionConflictCode);
                }

                if (request.Signal.SourceKind == DynamicGraphBuildSourceKind.DeterministicBootstrap
                    && _current is not null)
                {
                    return Failure<DynamicGraphTopologyApplyResult>(
                        "Dynamic graph topology build signal is invalid.",
                        SignalInvalidCode);
                }

                var signalAuthority = NormalizeSignal(request.Signal, _current?.Topology)!;
                var proposedValidation = DynamicGraphValidation.Validate(signalAuthority.ProposedTopology);
                if (!proposedValidation.IsSuccess)
                {
                    return Failure<DynamicGraphTopologyApplyResult>(
                        "Dynamic graph topology proposal is invalid.",
                        string.IsNullOrWhiteSpace(proposedValidation.ErrorCode)
                            ? ProposedTopologyInvalidCode
                            : proposedValidation.ErrorCode);
                }

                if (_current is not null
                    && !HasMatchingScope(_current.Topology, signalAuthority.ProposedTopology))
                {
                    return Failure<DynamicGraphTopologyApplyResult>(
                        "Dynamic graph topology mutation scope does not match the previous topology.",
                        ScopeMismatchCode);
                }

                var builderSignal = CopySignal(signalAuthority);
                OperationResult<BuildDynamicTopologyResult>? builderOperation;
                try
                {
                    builderOperation = _builder.BuildDynamicTopology(
                        builderSignal,
                        request.Budget);
                }
                catch (Exception exception) when (IsNonFatalRuntimeException(exception))
                {
                    return UnexpectedFailure<DynamicGraphTopologyApplyResult>();
                }

                if (builderOperation is null
                    || (builderOperation.IsSuccess && builderOperation.Value is null))
                {
                    return Failure<DynamicGraphTopologyApplyResult>(
                        "Dynamic graph topology builder result is required.",
                        BuilderResultRequiredCode);
                }

                if (!builderOperation.IsSuccess)
                {
                    return Failure<DynamicGraphTopologyApplyResult>(
                        "Dynamic graph topology builder failed.",
                        BuilderFailedCode);
                }

                var validated = ValidateAndCopyBuildResult(
                    builderOperation.Value!,
                    signalAuthority,
                    request.Budget,
                    _current);
                if (!validated.IsSuccess)
                {
                    return Failure<DynamicGraphTopologyApplyResult>(
                        "Dynamic graph topology builder returned an invalid result.",
                        string.IsNullOrWhiteSpace(validated.ErrorCode)
                            ? BuilderResultInvalidCode
                            : validated.ErrorCode);
                }

                var buildResult = validated.Value!;
                if (StringComparer.Ordinal.Equals(buildResult.FailureReason, BudgetExceededCode))
                {
                    return OperationResult<DynamicGraphTopologyApplyResult>.Ok(
                        new DynamicGraphTopologyApplyResult(
                            PreviousRevision: previousRevision,
                            CurrentRevision: previousRevision,
                            RevisionAdvanced: false,
                            BuildResult: CopyBuildResult(buildResult),
                            Current: CopySnapshotOrNull(_current)));
                }

                if (!buildResult.MutationEvidence.MutationApplied)
                {
                    return OperationResult<DynamicGraphTopologyApplyResult>.Ok(
                        new DynamicGraphTopologyApplyResult(
                            PreviousRevision: previousRevision,
                            CurrentRevision: previousRevision,
                            RevisionAdvanced: false,
                            BuildResult: CopyBuildResult(buildResult),
                            Current: CopySnapshotOrNull(_current)));
                }

                var currentRevision = checked(previousRevision + 1L);
                var accepted = new DynamicGraphTopologyRuntimeSnapshot(
                    Revision: currentRevision,
                    Topology: CopyTopology(buildResult.Topology!),
                    SignalId: signalAuthority.SignalId,
                    SourceKind: signalAuthority.SourceKind,
                    SourceRef: signalAuthority.SourceRef,
                    TraceRef: signalAuthority.TraceRef,
                    BudgetProof: CopyBudgetProof(buildResult.BudgetProof),
                    MutationEvidence: CopyMutationEvidence(buildResult.MutationEvidence));

                var nextCurrent = CopySnapshot(accepted);
                var nextHistory = CopySnapshot(accepted);
                var operation = OperationResult<DynamicGraphTopologyApplyResult>.Ok(
                    new DynamicGraphTopologyApplyResult(
                        PreviousRevision: previousRevision,
                        CurrentRevision: currentRevision,
                        RevisionAdvanced: true,
                        BuildResult: CopyBuildResult(buildResult),
                        Current: CopySnapshot(accepted)));

                // All fallible copies are prepared before publication; the final reference assignment cannot fail.
                _history.Add(nextHistory);
                _current = nextCurrent;
                return operation;
            }
        }
        catch (Exception exception) when (IsNonFatalRuntimeException(exception))
        {
            return UnexpectedFailure<DynamicGraphTopologyApplyResult>();
        }
    }

    // Restores one authenticated retained revision without widening the public runtime-owner contract.
    // The operation is one-shot and failure-atomic: every validation and allocation completes before
    // the current/history references and the one-shot marker are published.
    internal OperationResult<DynamicGraphTopologyRuntimeSnapshot> RestoreCheckpoint(
        InMemoryDynamicGraphTopologyRestoreState state)
    {
        if (_builder is null)
            return DependencyFailure<DynamicGraphTopologyRuntimeSnapshot>();
        if (state is null || state.Current is null)
        {
            return Failure<DynamicGraphTopologyRuntimeSnapshot>(
                "Dynamic graph topology restore state is required.",
                RestoreStateRequiredCode);
        }

        try
        {
            lock (_sync)
            {
                if (_restoreConsumed)
                {
                    return Failure<DynamicGraphTopologyRuntimeSnapshot>(
                        "Dynamic graph topology restore authority was already consumed.",
                        RestoreAlreadyConsumedCode);
                }

                if (_current is null
                    || _history.Count != 1
                    || _current.Revision != 1
                    || !SnapshotEquals(_current, _history[0])
                    || !state.InitialRevisionAdvanced
                    || !IsRestoreSnapshotValid(state.Current, _current))
                {
                    return Failure<DynamicGraphTopologyRuntimeSnapshot>(
                        "Dynamic graph topology restore state is invalid.",
                        RestoreStateInvalidCode);
                }

                var nextCurrent = CopySnapshot(state.Current);
                var nextHistory = CopySnapshot(state.Current);
                var readback = CopySnapshot(state.Current);
                var operation =
                    OperationResult<DynamicGraphTopologyRuntimeSnapshot>.Ok(readback);

                // Clear retains the already-allocated single-entry capacity, so publication below is
                // non-throwing after all independently owned copies have been allocated.
                _history.Clear();
                _history.Add(nextHistory);
                _current = nextCurrent;
                _restoreConsumed = true;
                return operation;
            }
        }
        catch (Exception exception) when (IsNonFatalRuntimeException(exception))
        {
            return UnexpectedFailure<DynamicGraphTopologyRuntimeSnapshot>();
        }
    }

    private static bool IsRestoreSnapshotValid(
        DynamicGraphTopologyRuntimeSnapshot restored,
        DynamicGraphTopologyRuntimeSnapshot bootstrap)
    {
        if (restored.Revision is < 1 or long.MaxValue
            || restored.Topology is null
            || restored.BudgetProof is null
            || restored.MutationEvidence is null
            || restored.SourceKind != DynamicGraphBuildSourceKind.LogicalRegistryProjection
            || restored.MutationEvidence.SourceKind
                != DynamicGraphBuildSourceKind.LogicalRegistryProjection
            || !IsCanonicalSingleLineValue(restored.SignalId)
            || !IsCanonicalSingleLineValue(restored.SourceRef)
            || !IsCanonicalSingleLineValue(restored.TraceRef)
            || !StringComparer.Ordinal.Equals(
                restored.SignalId,
                restored.MutationEvidence.SignalId)
            || !StringComparer.Ordinal.Equals(
                restored.SourceRef,
                restored.MutationEvidence.SourceRef)
            || !StringComparer.Ordinal.Equals(
                restored.TraceRef,
                restored.MutationEvidence.TraceRef)
            || !HasMatchingScope(bootstrap.Topology, restored.Topology)
            || !TopologyEquals(bootstrap.Topology, restored.Topology)
            || !HasRequiredEvidenceLists(restored.MutationEvidence)
            || !HasCanonicalEvidenceLists(restored.MutationEvidence))
        {
            return false;
        }

        var budget = new TopologyBuildBudget(
            restored.BudgetProof.MaxNodes,
            restored.BudgetProof.MaxEdges,
            restored.BudgetProof.MaxDepth,
            restored.BudgetProof.MaxParallelBranches);
        if (!IsBuildBudgetValid(budget)
            || !IsBudgetProofCoherent(restored.BudgetProof, budget, restored.Topology)
            || !restored.BudgetProof.WithinBudget
            || restored.BudgetProof.ExceededDimensions.Count != 0)
        {
            return false;
        }

        return restored.Revision == 1
            ? restored.MutationEvidence.MutationApplied
              && EvidenceDiffMatches(
                  restored.MutationEvidence,
                  previous: null,
                  restored.Topology)
            : IsExplicitRestoreMutationEvidenceValid(
                restored.MutationEvidence,
                restored.Topology);
    }

    private static bool IsExplicitRestoreMutationEvidenceValid(
        TopologyMutationEvidence evidence,
        DynamicGraphTopology topology)
    {
        var nodeIds = topology.Nodes
            .Select(node => node.NodeId)
            .ToHashSet(StringComparer.Ordinal);
        var edgeIds = topology.Edges
            .Select(edge => edge.EdgeId)
            .ToHashSet(StringComparer.Ordinal);

        var addedNodes = evidence.ProposedAddedNodeIds.ToHashSet(StringComparer.Ordinal);
        var removedNodes = evidence.ProposedRemovedNodeIds.ToHashSet(StringComparer.Ordinal);
        var updatedNodes = evidence.ProposedUpdatedNodeIds.ToHashSet(StringComparer.Ordinal);
        var reorderedNodes = evidence.ProposedReorderedNodeIds.ToHashSet(StringComparer.Ordinal);
        var addedEdges = evidence.ProposedAddedEdgeIds.ToHashSet(StringComparer.Ordinal);
        var removedEdges = evidence.ProposedRemovedEdgeIds.ToHashSet(StringComparer.Ordinal);
        var updatedEdges = evidence.ProposedUpdatedEdgeIds.ToHashSet(StringComparer.Ordinal);

        if (!addedNodes.IsSubsetOf(nodeIds)
            || !updatedNodes.IsSubsetOf(nodeIds)
            || !reorderedNodes.IsSubsetOf(nodeIds)
            || removedNodes.Overlaps(nodeIds)
            || addedNodes.Overlaps(removedNodes)
            || addedNodes.Overlaps(updatedNodes)
            || addedNodes.Overlaps(reorderedNodes)
            || removedNodes.Overlaps(updatedNodes)
            || removedNodes.Overlaps(reorderedNodes)
            || !addedEdges.IsSubsetOf(edgeIds)
            || !updatedEdges.IsSubsetOf(edgeIds)
            || removedEdges.Overlaps(edgeIds)
            || addedEdges.Overlaps(removedEdges)
            || addedEdges.Overlaps(updatedEdges)
            || removedEdges.Overlaps(updatedEdges))
        {
            return false;
        }

        var hasMutation = HasMutationEvidence(evidence);
        return hasMutation && evidence.MutationApplied == hasMutation;
    }

    private static DynamicGraphBuildSignal? NormalizeSignal(
        DynamicGraphBuildSignal? signal,
        DynamicGraphTopology? current)
        => signal is null
            ? null
            : signal with
            {
                ProposedTopology = CopyTopologyOrNull(signal.ProposedTopology)!,
                PreviousTopology = CopyTopologyOrNull(current)
            };

    private static DynamicGraphBuildSignal CopySignal(DynamicGraphBuildSignal signal)
        => signal with
        {
            ProposedTopology = CopyTopology(signal.ProposedTopology),
            PreviousTopology = CopyTopologyOrNull(signal.PreviousTopology)
        };

    private static OperationResult<BuildDynamicTopologyResult> ValidateAndCopyBuildResult(
        BuildDynamicTopologyResult result,
        DynamicGraphBuildSignal? signal,
        TopologyBuildBudget? budget,
        DynamicGraphTopologyRuntimeSnapshot? current)
    {
        if (result.BudgetProof is null
            || result.MutationEvidence is null
            || result.BudgetProof.ExceededDimensions is null
            || !HasRequiredEvidenceLists(result.MutationEvidence)
            || signal is null
            || signal.ProposedTopology is null
            || budget is null)
        {
            return InvalidBuilderResult();
        }

        if (!IsSuccessfulInputShapeValid(signal, budget, current))
            return InvalidBuilderResult();

        var proposedValidation = DynamicGraphValidation.Validate(signal.ProposedTopology);
        if (!proposedValidation.IsSuccess)
            return proposedValidation.MapError<BuildDynamicTopologyResult>();

        if (current is not null
            && !HasMatchingScope(current.Topology, signal.ProposedTopology))
        {
            return InvalidBuilderResult();
        }

        if (!EvidenceMatchesSignal(result.MutationEvidence, signal)
            || !IsBudgetProofCoherent(result.BudgetProof, budget, signal.ProposedTopology)
            || !HasCanonicalEvidenceLists(result.MutationEvidence)
            || !EvidenceDiffMatches(
                result.MutationEvidence,
                current?.Topology,
                signal.ProposedTopology))
        {
            return InvalidBuilderResult();
        }

        var isBudgetExceeded = StringComparer.Ordinal.Equals(
            result.FailureReason,
            BudgetExceededCode);
        if (result.FailureReason is not null && !isBudgetExceeded)
            return InvalidBuilderResult();

        if (isBudgetExceeded)
        {
            if (result.BudgetProof.WithinBudget
                || result.MutationEvidence.MutationApplied
                || result.BudgetProof.ExceededDimensions.Count == 0)
            {
                return InvalidBuilderResult();
            }

            if (current is null)
            {
                if (result.Topology is not null)
                    return InvalidBuilderResult();
            }
            else if (result.Topology is null)
            {
                return InvalidBuilderResult();
            }
            else
            {
                var retainedValidation = DynamicGraphValidation.Validate(result.Topology);
                if (!retainedValidation.IsSuccess)
                    return retainedValidation.MapError<BuildDynamicTopologyResult>();
                if (!TopologyEquals(result.Topology, current.Topology))
                    return InvalidBuilderResult();
            }

            return OperationResult<BuildDynamicTopologyResult>.Ok(CopyBuildResult(result));
        }

        if (!result.BudgetProof.WithinBudget
            || result.BudgetProof.ExceededDimensions.Count != 0)
        {
            return InvalidBuilderResult();
        }

        if (result.Topology is null)
        {
            return Failure<BuildDynamicTopologyResult>(
                "Accepted dynamic graph topology is required.",
                TopologyRequiredCode);
        }

        var topologyValidation = DynamicGraphValidation.Validate(result.Topology);
        if (!topologyValidation.IsSuccess)
            return topologyValidation.MapError<BuildDynamicTopologyResult>();
        if (!TopologyEquals(result.Topology, signal.ProposedTopology))
            return InvalidBuilderResult();

        var hasEvidenceMutation = HasMutationEvidence(result.MutationEvidence);
        if (result.MutationEvidence.MutationApplied != hasEvidenceMutation)
            return InvalidBuilderResult();

        if (current is null)
        {
            if (!result.MutationEvidence.MutationApplied)
                return InvalidBuilderResult();
        }
        else
        {
            var topologyChanged = !TopologyEquals(result.Topology, current.Topology);
            if (result.MutationEvidence.MutationApplied != topologyChanged)
                return InvalidBuilderResult();
        }

        return OperationResult<BuildDynamicTopologyResult>.Ok(CopyBuildResult(result));
    }

    private static bool EvidenceMatchesSignal(
        TopologyMutationEvidence evidence,
        DynamicGraphBuildSignal signal)
        => StringComparer.Ordinal.Equals(evidence.SignalId, signal.SignalId)
           && evidence.SourceKind == signal.SourceKind
           && StringComparer.Ordinal.Equals(evidence.SourceRef, signal.SourceRef)
           && StringComparer.Ordinal.Equals(evidence.TraceRef, signal.TraceRef);

    private static bool HasMatchingScope(
        DynamicGraphTopology current,
        DynamicGraphTopology proposed)
        => StringComparer.Ordinal.Equals(current.TenantId, proposed.TenantId)
           && StringComparer.Ordinal.Equals(current.DomainTag, proposed.DomainTag)
           && StringComparer.Ordinal.Equals(current.ContextId, proposed.ContextId);

    private static bool IsSuccessfulInputShapeValid(
        DynamicGraphBuildSignal signal,
        TopologyBuildBudget budget,
        DynamicGraphTopologyRuntimeSnapshot? current)
        => IsBuildSignalShapeValid(signal)
           && IsBuildBudgetValid(budget)
           && (signal.SourceKind != DynamicGraphBuildSourceKind.DeterministicBootstrap
               || current is null);

    private static bool IsBuildSignalShapeValid(DynamicGraphBuildSignal signal)
        => IsCanonicalSingleLineValue(signal.SignalId)
           && Enum.IsDefined(typeof(DynamicGraphBuildSourceKind), signal.SourceKind)
           && signal.ProposedTopology is not null
           && IsCanonicalSingleLineValue(signal.SourceRef)
           && IsCanonicalSingleLineValue(signal.TraceRef);

    private static bool IsCanonicalSingleLineValue(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && string.Equals(value, value.Trim(), StringComparison.Ordinal)
           && value.All(character =>
               !char.IsControl(character)
               && character is not '\r'
               && character is not '\n'
               && character is not '\u0085'
               && character is not '\u2028'
               && character is not '\u2029');

    private static bool IsBuildBudgetValid(TopologyBuildBudget budget)
        => budget.MaxNodes >= 1
           && budget.MaxEdges >= 0
           && budget.MaxDepth >= 0
           && budget.MaxParallelBranches is >= 1 and <= MaximumParallelBranches;

    private static bool IsBudgetProofCoherent(
        TopologyBudgetProof proof,
        TopologyBuildBudget budget,
        DynamicGraphTopology proposed)
    {
        if (proof.MaxNodes != budget.MaxNodes
            || proof.MaxEdges != budget.MaxEdges
            || proof.MaxDepth != budget.MaxDepth
            || proof.MaxParallelBranches != budget.MaxParallelBranches
            || proof.ActualNodes != proposed.Nodes.Count
            || proof.ActualEdges != proposed.Edges.Count
            || proof.ActualDepth != CalculateOrdinaryDepth(proposed)
            || proof.ActualParallelBranches != CalculateParallelBranchWidth(proposed)
            || proof.ActualNodes < 0
            || proof.ActualEdges < 0
            || proof.ActualDepth < 0
            || proof.ActualParallelBranches < 0)
        {
            return false;
        }

        var expectedExceeded = new List<string>(4);
        if (proof.ActualNodes > proof.MaxNodes)
            expectedExceeded.Add(BudgetDimensions[0]);
        if (proof.ActualEdges > proof.MaxEdges)
            expectedExceeded.Add(BudgetDimensions[1]);
        if (proof.ActualDepth > proof.MaxDepth)
            expectedExceeded.Add(BudgetDimensions[2]);
        if (proof.ActualParallelBranches > proof.MaxParallelBranches)
            expectedExceeded.Add(BudgetDimensions[3]);

        return proof.WithinBudget == (expectedExceeded.Count == 0)
               && expectedExceeded.SequenceEqual(
                   proof.ExceededDimensions,
                   StringComparer.Ordinal);
    }

    private static int CalculateOrdinaryDepth(DynamicGraphTopology topology)
    {
        var positionByNodeId = topology.Order.ToDictionary(
            entry => entry.NodeId,
            entry => entry.Position,
            StringComparer.Ordinal);
        var adjacency = topology.Nodes.ToDictionary(
            node => node.NodeId,
            _ => new List<string>(),
            StringComparer.Ordinal);
        var indegree = topology.Nodes.ToDictionary(
            node => node.NodeId,
            _ => 0,
            StringComparer.Ordinal);
        var depthByNodeId = topology.Nodes.ToDictionary(
            node => node.NodeId,
            _ => 0,
            StringComparer.Ordinal);

        foreach (var edge in topology.Edges.Where(edge => edge.Kind != GraphEdgeKind.ReturnLoop))
        {
            foreach (var targetNodeId in edge.ToNodeIds)
            {
                adjacency[edge.FromNodeId].Add(targetNodeId);
                indegree[targetNodeId]++;
            }
        }

        var readyComparer = Comparer<string>.Create((left, right) =>
        {
            if (StringComparer.Ordinal.Equals(left, right))
                return 0;
            var byPosition = positionByNodeId[left].CompareTo(positionByNodeId[right]);
            return byPosition != 0 ? byPosition : StringComparer.Ordinal.Compare(left, right);
        });
        var ready = new SortedSet<string>(readyComparer);
        foreach (var nodeId in indegree.Where(pair => pair.Value == 0).Select(pair => pair.Key))
            ready.Add(nodeId);

        while (ready.Count > 0)
        {
            var nodeId = ready.Min!;
            ready.Remove(nodeId);
            foreach (var targetNodeId in adjacency[nodeId]
                         .OrderBy(target => positionByNodeId[target])
                         .ThenBy(target => target, StringComparer.Ordinal))
            {
                depthByNodeId[targetNodeId] = Math.Max(
                    depthByNodeId[targetNodeId],
                    depthByNodeId[nodeId] + 1);
                indegree[targetNodeId]--;
                if (indegree[targetNodeId] == 0)
                    ready.Add(targetNodeId);
            }
        }

        return depthByNodeId.Values.Max();
    }

    private static int CalculateParallelBranchWidth(DynamicGraphTopology topology)
    {
        var targetsBySource = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        foreach (var edge in topology.Edges.Where(
                     edge => edge.Kind != GraphEdgeKind.ReturnLoop && edge.ParallelEligible))
        {
            if (!targetsBySource.TryGetValue(edge.FromNodeId, out var targets))
            {
                targets = new HashSet<string>(StringComparer.Ordinal);
                targetsBySource.Add(edge.FromNodeId, targets);
            }

            foreach (var target in edge.ToNodeIds)
                targets.Add(target);
        }

        return Math.Max(
            1,
            targetsBySource.Count == 0
                ? 0
                : targetsBySource.Values.Max(targets => targets.Count));
    }

    private static bool EvidenceDiffMatches(
        TopologyMutationEvidence evidence,
        DynamicGraphTopology? previous,
        DynamicGraphTopology proposed)
    {
        var previousNodes = previous?.Nodes.ToDictionary(node => node.NodeId, StringComparer.Ordinal)
                            ?? new Dictionary<string, IDynamicGraphNode>(StringComparer.Ordinal);
        var proposedNodes = proposed.Nodes.ToDictionary(node => node.NodeId, StringComparer.Ordinal);
        var previousEdges = previous?.Edges.ToDictionary(edge => edge.EdgeId, StringComparer.Ordinal)
                            ?? new Dictionary<string, IDynamicGraphEdge>(StringComparer.Ordinal);
        var proposedEdges = proposed.Edges.ToDictionary(edge => edge.EdgeId, StringComparer.Ordinal);
        var previousOrder = previous?.Order.ToDictionary(entry => entry.NodeId, StringComparer.Ordinal)
                            ?? new Dictionary<string, DynamicGraphNodeOrder>(StringComparer.Ordinal);
        var proposedOrder = proposed.Order.ToDictionary(entry => entry.NodeId, StringComparer.Ordinal);

        return SequenceMatches(
                   evidence.ProposedAddedNodeIds,
                   proposedNodes.Keys.Except(previousNodes.Keys, StringComparer.Ordinal))
               && SequenceMatches(
                   evidence.ProposedRemovedNodeIds,
                   previousNodes.Keys.Except(proposedNodes.Keys, StringComparer.Ordinal))
               && SequenceMatches(
                   evidence.ProposedUpdatedNodeIds,
                   proposedNodes.Keys.Intersect(previousNodes.Keys, StringComparer.Ordinal)
                       .Where(nodeId => !NodeComparer.Instance.Equals(
                           previousNodes[nodeId],
                           proposedNodes[nodeId])))
               && SequenceMatches(
                   evidence.ProposedAddedEdgeIds,
                   proposedEdges.Keys.Except(previousEdges.Keys, StringComparer.Ordinal))
               && SequenceMatches(
                   evidence.ProposedRemovedEdgeIds,
                   previousEdges.Keys.Except(proposedEdges.Keys, StringComparer.Ordinal))
               && SequenceMatches(
                   evidence.ProposedUpdatedEdgeIds,
                   proposedEdges.Keys.Intersect(previousEdges.Keys, StringComparer.Ordinal)
                       .Where(edgeId => !EdgeComparer.Instance.Equals(
                           previousEdges[edgeId],
                           proposedEdges[edgeId])))
               && SequenceMatches(
                   evidence.ProposedReorderedNodeIds,
                   proposedOrder.Keys.Intersect(previousOrder.Keys, StringComparer.Ordinal)
                       .Where(nodeId => !NodeOrderComparer.Instance.Equals(
                           previousOrder[nodeId],
                           proposedOrder[nodeId])));
    }

    private static bool SequenceMatches(IReadOnlyList<string> actual, IEnumerable<string> expected)
        => actual.SequenceEqual(
            expected.OrderBy(value => value, StringComparer.Ordinal),
            StringComparer.Ordinal);

    private static bool HasRequiredEvidenceLists(TopologyMutationEvidence evidence)
        => evidence.ProposedAddedNodeIds is not null
           && evidence.ProposedRemovedNodeIds is not null
           && evidence.ProposedUpdatedNodeIds is not null
           && evidence.ProposedAddedEdgeIds is not null
           && evidence.ProposedRemovedEdgeIds is not null
           && evidence.ProposedUpdatedEdgeIds is not null
           && evidence.ProposedReorderedNodeIds is not null;

    private static bool HasCanonicalEvidenceLists(TopologyMutationEvidence evidence)
        => IsCanonical(evidence.ProposedAddedNodeIds)
           && IsCanonical(evidence.ProposedRemovedNodeIds)
           && IsCanonical(evidence.ProposedUpdatedNodeIds)
           && IsCanonical(evidence.ProposedAddedEdgeIds)
           && IsCanonical(evidence.ProposedRemovedEdgeIds)
           && IsCanonical(evidence.ProposedUpdatedEdgeIds)
           && IsCanonical(evidence.ProposedReorderedNodeIds);

    private static bool IsCanonical(IReadOnlyList<string> values)
        => values.All(value => !string.IsNullOrWhiteSpace(value))
           && values.SequenceEqual(
               values.OrderBy(value => value, StringComparer.Ordinal),
               StringComparer.Ordinal)
           && values.Distinct(StringComparer.Ordinal).Count() == values.Count;

    private static bool HasMutationEvidence(TopologyMutationEvidence evidence)
        => evidence.ProposedAddedNodeIds.Count > 0
           || evidence.ProposedRemovedNodeIds.Count > 0
           || evidence.ProposedUpdatedNodeIds.Count > 0
           || evidence.ProposedAddedEdgeIds.Count > 0
           || evidence.ProposedRemovedEdgeIds.Count > 0
           || evidence.ProposedUpdatedEdgeIds.Count > 0
           || evidence.ProposedReorderedNodeIds.Count > 0;

    private static bool TopologyEquals(DynamicGraphTopology left, DynamicGraphTopology right)
        => StringComparer.Ordinal.Equals(left.TenantId, right.TenantId)
           && StringComparer.Ordinal.Equals(left.DomainTag, right.DomainTag)
           && StringComparer.Ordinal.Equals(left.ContextId, right.ContextId)
           && SortedNodes(left).SequenceEqual(SortedNodes(right), NodeComparer.Instance)
           && SortedEdges(left).SequenceEqual(SortedEdges(right), EdgeComparer.Instance)
           && left.Order.OrderBy(entry => entry.NodeId, StringComparer.Ordinal)
               .SequenceEqual(
                   right.Order.OrderBy(entry => entry.NodeId, StringComparer.Ordinal),
                   NodeOrderComparer.Instance);

    private static IEnumerable<IDynamicGraphNode> SortedNodes(DynamicGraphTopology topology)
        => topology.Nodes.OrderBy(node => node.NodeId, StringComparer.Ordinal);

    private static IEnumerable<IDynamicGraphEdge> SortedEdges(DynamicGraphTopology topology)
        => topology.Edges.OrderBy(edge => edge.EdgeId, StringComparer.Ordinal);

    private static DynamicGraphTopology CopyTopology(DynamicGraphTopology topology)
        => new(
            topology.TenantId,
            topology.DomainTag,
            topology.ContextId,
            topology.Nodes,
            topology.Edges,
            topology.Order);

    private static DynamicGraphTopology? CopyTopologyOrNull(DynamicGraphTopology? topology)
        => topology is null ? null : CopyTopology(topology);

    private static TopologyBudgetProof CopyBudgetProof(TopologyBudgetProof proof)
        => proof with
        {
            ExceededDimensions = CopyStringList(proof.ExceededDimensions)
        };

    private static TopologyMutationEvidence CopyMutationEvidence(TopologyMutationEvidence evidence)
        => evidence with
        {
            ProposedAddedNodeIds = Freeze(evidence.ProposedAddedNodeIds),
            ProposedRemovedNodeIds = Freeze(evidence.ProposedRemovedNodeIds),
            ProposedUpdatedNodeIds = Freeze(evidence.ProposedUpdatedNodeIds),
            ProposedAddedEdgeIds = Freeze(evidence.ProposedAddedEdgeIds),
            ProposedRemovedEdgeIds = Freeze(evidence.ProposedRemovedEdgeIds),
            ProposedUpdatedEdgeIds = Freeze(evidence.ProposedUpdatedEdgeIds),
            ProposedReorderedNodeIds = Freeze(evidence.ProposedReorderedNodeIds)
        };

    private static IReadOnlyList<string> Freeze(IReadOnlyList<string> values)
        => CopyStringList(values);

    private static IReadOnlyList<string> CopyStringList(IReadOnlyList<string> values)
    {
        var copy = new string[values.Count];
        for (var index = 0; index < values.Count; index++)
            copy[index] = values[index];
        return new System.Collections.ObjectModel.ReadOnlyCollection<string>(copy);
    }

    private static BuildDynamicTopologyResult CopyBuildResult(BuildDynamicTopologyResult result)
        => new(
            Topology: CopyTopologyOrNull(result.Topology),
            BudgetProof: CopyBudgetProof(result.BudgetProof),
            FailureReason: result.FailureReason,
            MutationEvidence: CopyMutationEvidence(result.MutationEvidence));

    private static DynamicGraphTopologyRuntimeSnapshot CopySnapshot(
        DynamicGraphTopologyRuntimeSnapshot snapshot)
        => new(
            Revision: snapshot.Revision,
            Topology: CopyTopology(snapshot.Topology),
            SignalId: snapshot.SignalId,
            SourceKind: snapshot.SourceKind,
            SourceRef: snapshot.SourceRef,
            TraceRef: snapshot.TraceRef,
            BudgetProof: CopyBudgetProof(snapshot.BudgetProof),
            MutationEvidence: CopyMutationEvidence(snapshot.MutationEvidence));

    private static DynamicGraphTopologyRuntimeSnapshot? CopySnapshotOrNull(
        DynamicGraphTopologyRuntimeSnapshot? snapshot)
        => snapshot is null ? null : CopySnapshot(snapshot);

    private static bool SnapshotEquals(
        DynamicGraphTopologyRuntimeSnapshot left,
        DynamicGraphTopologyRuntimeSnapshot right)
        => left.Revision == right.Revision
           && TopologyEquals(left.Topology, right.Topology)
           && StringComparer.Ordinal.Equals(left.SignalId, right.SignalId)
           && left.SourceKind == right.SourceKind
           && StringComparer.Ordinal.Equals(left.SourceRef, right.SourceRef)
           && StringComparer.Ordinal.Equals(left.TraceRef, right.TraceRef)
           && left.BudgetProof.MaxNodes == right.BudgetProof.MaxNodes
           && left.BudgetProof.ActualNodes == right.BudgetProof.ActualNodes
           && left.BudgetProof.MaxEdges == right.BudgetProof.MaxEdges
           && left.BudgetProof.ActualEdges == right.BudgetProof.ActualEdges
           && left.BudgetProof.MaxDepth == right.BudgetProof.MaxDepth
           && left.BudgetProof.ActualDepth == right.BudgetProof.ActualDepth
           && left.BudgetProof.MaxParallelBranches
               == right.BudgetProof.MaxParallelBranches
           && left.BudgetProof.ActualParallelBranches
               == right.BudgetProof.ActualParallelBranches
           && left.BudgetProof.WithinBudget == right.BudgetProof.WithinBudget
           && left.BudgetProof.ExceededDimensions.SequenceEqual(
               right.BudgetProof.ExceededDimensions,
               StringComparer.Ordinal)
           && StringComparer.Ordinal.Equals(
               left.MutationEvidence.SignalId,
               right.MutationEvidence.SignalId)
           && left.MutationEvidence.SourceKind == right.MutationEvidence.SourceKind
           && StringComparer.Ordinal.Equals(
               left.MutationEvidence.SourceRef,
               right.MutationEvidence.SourceRef)
           && StringComparer.Ordinal.Equals(
               left.MutationEvidence.TraceRef,
               right.MutationEvidence.TraceRef)
           && left.MutationEvidence.MutationApplied
               == right.MutationEvidence.MutationApplied
           && left.MutationEvidence.ProposedAddedNodeIds.SequenceEqual(
               right.MutationEvidence.ProposedAddedNodeIds,
               StringComparer.Ordinal)
           && left.MutationEvidence.ProposedRemovedNodeIds.SequenceEqual(
               right.MutationEvidence.ProposedRemovedNodeIds,
               StringComparer.Ordinal)
           && left.MutationEvidence.ProposedUpdatedNodeIds.SequenceEqual(
               right.MutationEvidence.ProposedUpdatedNodeIds,
               StringComparer.Ordinal)
           && left.MutationEvidence.ProposedAddedEdgeIds.SequenceEqual(
               right.MutationEvidence.ProposedAddedEdgeIds,
               StringComparer.Ordinal)
           && left.MutationEvidence.ProposedRemovedEdgeIds.SequenceEqual(
               right.MutationEvidence.ProposedRemovedEdgeIds,
               StringComparer.Ordinal)
           && left.MutationEvidence.ProposedUpdatedEdgeIds.SequenceEqual(
               right.MutationEvidence.ProposedUpdatedEdgeIds,
               StringComparer.Ordinal)
           && left.MutationEvidence.ProposedReorderedNodeIds.SequenceEqual(
               right.MutationEvidence.ProposedReorderedNodeIds,
               StringComparer.Ordinal);

    private static OperationResult<BuildDynamicTopologyResult> InvalidBuilderResult()
        => Failure<BuildDynamicTopologyResult>(
            "Dynamic graph topology builder returned an invalid result.",
            BuilderResultInvalidCode);

    private static OperationResult<T> DependencyFailure<T>()
        => Failure<T>(
            "Dynamic graph topology builder dependency is required.",
            DependencyRequiredCode);

    private static OperationResult<T> UnexpectedFailure<T>()
        => Failure<T>(
            "Dynamic graph topology runtime failed unexpectedly.",
            ExceptionCode);

    private static bool IsNonFatalRuntimeException(Exception exception)
    {
        var pending = new Stack<Exception>();
        var visited = new HashSet<Exception>(ReferenceEqualityComparer.Instance);
        pending.Push(exception);

        while (pending.Count > 0)
        {
            var current = pending.Pop();
            if (!visited.Add(current))
                continue;

            if (current is OperationCanceledException
                || current is OutOfMemoryException
                || current is StackOverflowException
                || current is AccessViolationException
                || current is AppDomainUnloadedException
                || current is BadImageFormatException
                || StringComparer.Ordinal.Equals(
                    current.GetType().FullName,
                    "System.Threading.ThreadAbortException"))
            {
                return false;
            }

            if (current is AggregateException aggregate)
            {
                foreach (var inner in aggregate.InnerExceptions)
                    pending.Push(inner);
            }
            else if (current.InnerException is not null)
            {
                pending.Push(current.InnerException);
            }
        }

        return true;
    }

    private static OperationResult<T> Failure<T>(string message, string code)
        => OperationResult<T>.Fail(message, code);

    private sealed class NodeComparer : IEqualityComparer<IDynamicGraphNode>
    {
        public static NodeComparer Instance { get; } = new();

        public bool Equals(IDynamicGraphNode? left, IDynamicGraphNode? right)
        {
            if (left is null || right is null)
                return left is null && right is null;

            var leftDescriptor = left.Descriptor;
            var rightDescriptor = right.Descriptor;
            if (leftDescriptor is null || rightDescriptor is null)
                return leftDescriptor is null && rightDescriptor is null;

            return StringComparer.Ordinal.Equals(left.NodeId, right.NodeId)
                   && leftDescriptor == rightDescriptor;
        }

        public int GetHashCode(IDynamicGraphNode value) => 0;
    }

    private sealed class EdgeComparer : IEqualityComparer<IDynamicGraphEdge>
    {
        public static EdgeComparer Instance { get; } = new();

        public bool Equals(IDynamicGraphEdge? left, IDynamicGraphEdge? right)
        {
            if (left is null || right is null)
                return left is null && right is null;

            return StringComparer.Ordinal.Equals(left.EdgeId, right.EdgeId)
                   && StringComparer.Ordinal.Equals(left.FromNodeId, right.FromNodeId)
                   && SetLikeEquals(left.ToNodeIds, right.ToNodeIds)
                   && left.Kind == right.Kind
                   && left.ParallelEligible == right.ParallelEligible
                   && StringComparer.Ordinal.Equals(left.AuthorityUnitId, right.AuthorityUnitId)
                   && StringComparer.Ordinal.Equals(left.AuthorityUnitLevel, right.AuthorityUnitLevel)
                   && left.DeterministicFinalAuthority == right.DeterministicFinalAuthority
                   && StringComparer.Ordinal.Equals(left.BaselineUnitId, right.BaselineUnitId)
                   && StringComparer.Ordinal.Equals(left.BaselineRole, right.BaselineRole)
                   && SetLikeEquals(left.ExcludedLeakageSignals, right.ExcludedLeakageSignals)
                   && StringComparer.Ordinal.Equals(left.SourceProof, right.SourceProof)
                   && StringComparer.Ordinal.Equals(left.CutoverProof, right.CutoverProof)
                   && StringComparer.Ordinal.Equals(left.AblationProofRef, right.AblationProofRef);
        }

        public int GetHashCode(IDynamicGraphEdge value) => 0;

        private static bool SetLikeEquals(IReadOnlyList<string> left, IReadOnlyList<string> right)
            => left.Count == right.Count
               && left.OrderBy(value => value, StringComparer.Ordinal)
                   .SequenceEqual(
                       right.OrderBy(value => value, StringComparer.Ordinal),
                       StringComparer.Ordinal);
    }

    private sealed class NodeOrderComparer : IEqualityComparer<DynamicGraphNodeOrder>
    {
        public static NodeOrderComparer Instance { get; } = new();

        public bool Equals(DynamicGraphNodeOrder? left, DynamicGraphNodeOrder? right)
            => left == right;

        public int GetHashCode(DynamicGraphNodeOrder value) => 0;
    }
}

// Internal complete restore carrier shared only with the orchestrator facade and tests.
internal sealed record InMemoryDynamicGraphTopologyRestoreState(
    DynamicGraphTopologyRuntimeSnapshot Current,
    bool InitialRevisionAdvanced);
