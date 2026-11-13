// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

// Builds one deterministic, budgeted topology snapshot without traversal or semantic selection authority.
public sealed class DeterministicDynamicGraphTopologyBuilder : IDynamicGraphTopologyBuilder
{
    private const string SignalRequiredCode = "DYNAMIC_GRAPH_TOPOLOGY_BUILD_SIGNAL_REQUIRED";
    private const string SignalInvalidCode = "DYNAMIC_GRAPH_TOPOLOGY_BUILD_SIGNAL_INVALID";
    private const string BudgetRequiredCode = "DYNAMIC_GRAPH_TOPOLOGY_BUILD_BUDGET_REQUIRED";
    private const string BudgetInvalidCode = "DYNAMIC_GRAPH_TOPOLOGY_BUILD_BUDGET_INVALID";
    private const string ScopeMismatchCode = "DYNAMIC_GRAPH_TOPOLOGY_BUILD_SCOPE_MISMATCH";
    private const string BudgetExceededCode = "DYNAMIC_GRAPH_TOPOLOGY_BUILD_BUDGET_EXCEEDED";
    private const string ExceptionCode = "DYNAMIC_GRAPH_TOPOLOGY_BUILD_EXCEPTION";
    private const int MaximumParallelBranches = 8;

    public OperationResult<BuildDynamicTopologyResult> BuildDynamicTopology(
        DynamicGraphBuildSignal signal,
        TopologyBuildBudget budget)
    {
        try
        {
            if (signal is null)
                return Failure("Dynamic graph topology build signal is required.", SignalRequiredCode);

            if (!IsSignalValid(signal))
                return Failure("Dynamic graph topology build signal is invalid.", SignalInvalidCode);

            if (budget is null)
                return Failure("Dynamic graph topology build budget is required.", BudgetRequiredCode);

            if (!IsBudgetValid(budget))
                return Failure("Dynamic graph topology build budget is invalid.", BudgetInvalidCode);

            if (signal.PreviousTopology is not null)
            {
                var previousValidation = DynamicGraphValidation.Validate(signal.PreviousTopology);
                if (!previousValidation.IsSuccess)
                    return previousValidation.MapError<BuildDynamicTopologyResult>();
            }

            var proposedValidation = DynamicGraphValidation.Validate(signal.ProposedTopology);
            if (!proposedValidation.IsSuccess)
                return proposedValidation.MapError<BuildDynamicTopologyResult>();

            if (signal.PreviousTopology is not null
                && !HasMatchingScope(signal.PreviousTopology, signal.ProposedTopology))
            {
                return Failure(
                    "Dynamic graph topology mutation scope does not match the previous topology.",
                    ScopeMismatchCode);
            }

            var proof = CreateBudgetProof(signal.ProposedTopology, budget);
            var evidence = CreateMutationEvidence(signal);

            if (!proof.WithinBudget)
            {
                return OperationResult<BuildDynamicTopologyResult>.Ok(
                    new BuildDynamicTopologyResult(
                        Topology: CopyTopology(signal.PreviousTopology),
                        BudgetProof: proof,
                        FailureReason: BudgetExceededCode,
                        MutationEvidence: evidence));
            }

            var applied = HasMutation(evidence);
            var acceptedEvidence = evidence with { MutationApplied = applied };
            var acceptedTopology = signal.PreviousTopology is not null && !applied
                ? signal.PreviousTopology
                : signal.ProposedTopology;
            return OperationResult<BuildDynamicTopologyResult>.Ok(
                new BuildDynamicTopologyResult(
                    Topology: CopyTopology(acceptedTopology),
                    BudgetProof: proof,
                    FailureReason: null,
                    MutationEvidence: acceptedEvidence));
        }
        catch (Exception exception) when (IsNonFatalBuildException(exception))
        {
            return Failure(
                "Dynamic graph topology build failed unexpectedly.",
                ExceptionCode);
        }
    }

    private static bool IsSignalValid(DynamicGraphBuildSignal signal)
        => IsCanonicalSingleLineValue(signal.SignalId)
           && Enum.IsDefined(typeof(DynamicGraphBuildSourceKind), signal.SourceKind)
           && signal.ProposedTopology is not null
           && IsCanonicalSingleLineValue(signal.SourceRef)
           && IsCanonicalSingleLineValue(signal.TraceRef)
           && (signal.SourceKind != DynamicGraphBuildSourceKind.DeterministicBootstrap
               || signal.PreviousTopology is null);

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

    private static bool IsBudgetValid(TopologyBuildBudget budget)
        => budget.MaxNodes >= 1
           && budget.MaxEdges >= 0
           && budget.MaxDepth >= 0
           && budget.MaxParallelBranches >= 1
           && budget.MaxParallelBranches <= MaximumParallelBranches;

    private static bool HasMatchingScope(
        DynamicGraphTopology previous,
        DynamicGraphTopology proposed)
        => StringComparer.Ordinal.Equals(previous.TenantId, proposed.TenantId)
           && StringComparer.Ordinal.Equals(previous.DomainTag, proposed.DomainTag)
           && StringComparer.Ordinal.Equals(previous.ContextId, proposed.ContextId);

    private static TopologyBudgetProof CreateBudgetProof(
        DynamicGraphTopology proposed,
        TopologyBuildBudget budget)
    {
        var actualNodes = proposed.Nodes.Count;
        var actualEdges = proposed.Edges.Count;
        var actualDepth = CalculateOrdinaryDepth(proposed);
        var actualParallelBranches = CalculateParallelBranchWidth(proposed);
        var exceeded = new List<string>(4);

        if (actualNodes > budget.MaxNodes)
            exceeded.Add("nodes");
        if (actualEdges > budget.MaxEdges)
            exceeded.Add("edges");
        if (actualDepth > budget.MaxDepth)
            exceeded.Add("depth");
        if (actualParallelBranches > budget.MaxParallelBranches)
            exceeded.Add("parallel_branches");

        return new TopologyBudgetProof(
            MaxNodes: budget.MaxNodes,
            ActualNodes: actualNodes,
            MaxEdges: budget.MaxEdges,
            ActualEdges: actualEdges,
            MaxDepth: budget.MaxDepth,
            ActualDepth: actualDepth,
            MaxParallelBranches: budget.MaxParallelBranches,
            ActualParallelBranches: actualParallelBranches,
            WithinBudget: exceeded.Count == 0,
            ExceededDimensions: Array.AsReadOnly(exceeded.ToArray()));
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

            var positionComparison = positionByNodeId[left].CompareTo(positionByNodeId[right]);
            return positionComparison != 0
                ? positionComparison
                : StringComparer.Ordinal.Compare(left, right);
        });
        var ready = new SortedSet<string>(readyComparer);
        var depthByNodeId = topology.Nodes.ToDictionary(
            node => node.NodeId,
            _ => 0,
            StringComparer.Ordinal);

        foreach (var nodeId in indegree
                     .Where(pair => pair.Value == 0)
                     .Select(pair => pair.Key))
        {
            ready.Add(nodeId);
        }

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
        var parallelTargetsBySource = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        foreach (var edge in topology.Edges.Where(
                     edge => edge.Kind != GraphEdgeKind.ReturnLoop && edge.ParallelEligible))
        {
            if (!parallelTargetsBySource.TryGetValue(edge.FromNodeId, out var targets))
            {
                targets = new HashSet<string>(StringComparer.Ordinal);
                parallelTargetsBySource.Add(edge.FromNodeId, targets);
            }

            foreach (var targetNodeId in edge.ToNodeIds)
                targets.Add(targetNodeId);
        }

        return Math.Max(
            1,
            parallelTargetsBySource.Count == 0
                ? 0
                : parallelTargetsBySource.Values.Max(targets => targets.Count));
    }

    private static TopologyMutationEvidence CreateMutationEvidence(
        DynamicGraphBuildSignal signal)
    {
        var previousNodes = signal.PreviousTopology?.Nodes.ToDictionary(
                                node => node.NodeId,
                                StringComparer.Ordinal)
                            ?? new Dictionary<string, IDynamicGraphNode>(StringComparer.Ordinal);
        var proposedNodes = signal.ProposedTopology.Nodes.ToDictionary(
            node => node.NodeId,
            StringComparer.Ordinal);
        var previousEdges = signal.PreviousTopology?.Edges.ToDictionary(
                                edge => edge.EdgeId,
                                StringComparer.Ordinal)
                            ?? new Dictionary<string, IDynamicGraphEdge>(StringComparer.Ordinal);
        var proposedEdges = signal.ProposedTopology.Edges.ToDictionary(
            edge => edge.EdgeId,
            StringComparer.Ordinal);
        var previousOrder = signal.PreviousTopology?.Order.ToDictionary(
                                entry => entry.NodeId,
                                StringComparer.Ordinal)
                            ?? new Dictionary<string, DynamicGraphNodeOrder>(StringComparer.Ordinal);
        var proposedOrder = signal.ProposedTopology.Order.ToDictionary(
            entry => entry.NodeId,
            StringComparer.Ordinal);

        return new TopologyMutationEvidence(
            SignalId: signal.SignalId,
            SourceKind: signal.SourceKind,
            SourceRef: signal.SourceRef,
            TraceRef: signal.TraceRef,
            ProposedAddedNodeIds: Freeze(proposedNodes.Keys.Except(previousNodes.Keys, StringComparer.Ordinal)),
            ProposedRemovedNodeIds: Freeze(previousNodes.Keys.Except(proposedNodes.Keys, StringComparer.Ordinal)),
            ProposedUpdatedNodeIds: Freeze(proposedNodes.Keys
                .Intersect(previousNodes.Keys, StringComparer.Ordinal)
                .Where(nodeId => !NodeEquals(previousNodes[nodeId], proposedNodes[nodeId]))),
            ProposedAddedEdgeIds: Freeze(proposedEdges.Keys.Except(previousEdges.Keys, StringComparer.Ordinal)),
            ProposedRemovedEdgeIds: Freeze(previousEdges.Keys.Except(proposedEdges.Keys, StringComparer.Ordinal)),
            ProposedUpdatedEdgeIds: Freeze(proposedEdges.Keys
                .Intersect(previousEdges.Keys, StringComparer.Ordinal)
                .Where(edgeId => !EdgeEquals(previousEdges[edgeId], proposedEdges[edgeId]))),
            ProposedReorderedNodeIds: Freeze(proposedOrder.Keys
                .Intersect(previousOrder.Keys, StringComparer.Ordinal)
                .Where(nodeId => !OrderEquals(previousOrder[nodeId], proposedOrder[nodeId]))),
            MutationApplied: false);
    }

    private static bool NodeEquals(IDynamicGraphNode left, IDynamicGraphNode right)
    {
        var leftDescriptor = left.Descriptor;
        var rightDescriptor = right.Descriptor;
        return StringComparer.Ordinal.Equals(left.NodeId, right.NodeId)
               && StringComparer.Ordinal.Equals(leftDescriptor.NodeId, rightDescriptor.NodeId)
               && StringComparer.Ordinal.Equals(leftDescriptor.DomainTag, rightDescriptor.DomainTag)
               && StringComparer.Ordinal.Equals(leftDescriptor.TInputTypeName, rightDescriptor.TInputTypeName)
               && StringComparer.Ordinal.Equals(leftDescriptor.TOutputTypeName, rightDescriptor.TOutputTypeName)
               && leftDescriptor.IsTrained == rightDescriptor.IsTrained
               && leftDescriptor.DeterministicFinalAuthority == rightDescriptor.DeterministicFinalAuthority
               && leftDescriptor.RuntimeSemanticQualityClaimed == rightDescriptor.RuntimeSemanticQualityClaimed
               && StringComparer.Ordinal.Equals(leftDescriptor.AuthorityRole, rightDescriptor.AuthorityRole)
               && StringComparer.Ordinal.Equals(leftDescriptor.CheckpointRef, rightDescriptor.CheckpointRef)
               && StringComparer.Ordinal.Equals(leftDescriptor.ManifestRef, rightDescriptor.ManifestRef);
    }

    private static bool EdgeEquals(IDynamicGraphEdge left, IDynamicGraphEdge right)
        => StringComparer.Ordinal.Equals(left.EdgeId, right.EdgeId)
           && StringComparer.Ordinal.Equals(left.FromNodeId, right.FromNodeId)
           && SetLikeSequenceEquals(left.ToNodeIds, right.ToNodeIds)
           && left.Kind == right.Kind
           && left.ParallelEligible == right.ParallelEligible
           && StringComparer.Ordinal.Equals(left.AuthorityUnitId, right.AuthorityUnitId)
           && StringComparer.Ordinal.Equals(left.AuthorityUnitLevel, right.AuthorityUnitLevel)
           && left.DeterministicFinalAuthority == right.DeterministicFinalAuthority
           && StringComparer.Ordinal.Equals(left.BaselineUnitId, right.BaselineUnitId)
           && StringComparer.Ordinal.Equals(left.BaselineRole, right.BaselineRole)
           && left.ExcludedLeakageSignals.SequenceEqual(
               right.ExcludedLeakageSignals,
               StringComparer.Ordinal)
           && StringComparer.Ordinal.Equals(left.SourceProof, right.SourceProof)
           && StringComparer.Ordinal.Equals(left.CutoverProof, right.CutoverProof)
           && StringComparer.Ordinal.Equals(left.AblationProofRef, right.AblationProofRef);

    // Target carriers are declarative sets; sorting preserves exact multiplicity without order authority.
    // Leakage exclusions are an ordered authority vocabulary and are compared positionally in EdgeEquals.
    private static bool SetLikeSequenceEquals(
        IReadOnlyList<string> left,
        IReadOnlyList<string> right)
        => left.Count == right.Count
           && left.OrderBy(value => value, StringComparer.Ordinal)
               .SequenceEqual(
                   right.OrderBy(value => value, StringComparer.Ordinal),
                   StringComparer.Ordinal);

    // Reorder evidence is emitted for a retained node whenever any field of its order contract changes.
    private static bool OrderEquals(DynamicGraphNodeOrder left, DynamicGraphNodeOrder right)
        => StringComparer.Ordinal.Equals(left.OrderId, right.OrderId)
           && StringComparer.Ordinal.Equals(left.NodeId, right.NodeId)
           && StringComparer.Ordinal.Equals(left.TenantId, right.TenantId)
           && StringComparer.Ordinal.Equals(left.DomainTag, right.DomainTag)
           && StringComparer.Ordinal.Equals(left.ContextId, right.ContextId)
           && left.Position == right.Position;

    private static bool HasMutation(TopologyMutationEvidence evidence)
        => evidence.ProposedAddedNodeIds.Count > 0
           || evidence.ProposedRemovedNodeIds.Count > 0
           || evidence.ProposedUpdatedNodeIds.Count > 0
           || evidence.ProposedAddedEdgeIds.Count > 0
           || evidence.ProposedRemovedEdgeIds.Count > 0
           || evidence.ProposedUpdatedEdgeIds.Count > 0
           || evidence.ProposedReorderedNodeIds.Count > 0;

    private static IReadOnlyList<string> Freeze(IEnumerable<string> values)
        => Array.AsReadOnly(values.OrderBy(value => value, StringComparer.Ordinal).ToArray());

    private static DynamicGraphTopology? CopyTopology(DynamicGraphTopology? topology)
        => topology is null
            ? null
            : new DynamicGraphTopology(
                topology.TenantId,
                topology.DomainTag,
                topology.ContextId,
                topology.Nodes,
                topology.Edges,
                topology.Order);

    private static OperationResult<BuildDynamicTopologyResult> Failure(
        string message,
        string code)
        => OperationResult<BuildDynamicTopologyResult>.Fail(message, code);

    private static bool IsNonFatalBuildException(Exception exception)
        => exception is not OutOfMemoryException
           && exception is not StackOverflowException
           && exception is not AccessViolationException
           && exception is not AppDomainUnloadedException
           && exception is not BadImageFormatException
           && !StringComparer.Ordinal.Equals(
               exception.GetType().FullName,
               "System.Threading.ThreadAbortException");
}
