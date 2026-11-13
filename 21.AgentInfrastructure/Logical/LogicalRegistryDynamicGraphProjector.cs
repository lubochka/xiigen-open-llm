// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;

// Converts an explicitly selected logical-registry subgraph into a validated dynamic-graph build signal.
public sealed class LogicalRegistryDynamicGraphProjector : ILogicalRegistryDynamicGraphProjector
{
    private const string DependencyRequiredCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_DEPENDENCY_REQUIRED";
    private const string RequestRequiredCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_REQUEST_REQUIRED";
    private const string ScopeInvalidCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_SCOPE_INVALID";
    private const string SignalReferencesInvalidCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_SIGNAL_REFERENCES_INVALID";
    private const string NodeBindingsRequiredCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_NODE_BINDINGS_REQUIRED";
    private const string EdgeBindingsInvalidCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_EDGE_BINDINGS_INVALID";
    private const string DuplicateNodeBindingCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_DUPLICATE_NODE_BINDING";
    private const string SubPhaseNotFoundCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_SUB_PHASE_NOT_FOUND";
    private const string NodeMismatchCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_NODE_MISMATCH";
    private const string DuplicateEdgeBindingCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_DUPLICATE_EDGE_BINDING";
    private const string TransitionsRequiredCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_TRANSITIONS_REQUIRED";
    private const string NonProjectableTransitionCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_TRANSITION_NOT_PROJECTABLE";
    private const string DuplicateTransitionCoverageCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_DUPLICATE_TRANSITION_COVERAGE";
    private const string IncompleteTransitionCoverageCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_INCOMPLETE_TRANSITION_COVERAGE";
    private const string EdgeSourceMismatchCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_EDGE_SOURCE_MISMATCH";
    private const string ExceptionCode = "DYNAMIC_GRAPH_REGISTRY_PROJECTION_EXCEPTION";
    private const string SubPhaseTopologyKind = "sub-phase";

    private readonly ILogicalTopologyRegistry? _registry;

    public LogicalRegistryDynamicGraphProjector(ILogicalTopologyRegistry registry)
        => _registry = registry;

    public OperationResult<DynamicGraphBuildSignal> Project(
        DynamicGraphRegistryProjectionRequest request)
    {
        if (_registry is null)
            return Failure("Logical topology registry dependency is required.", DependencyRequiredCode);

        if (request is null)
            return Failure("Dynamic graph registry projection request is required.", RequestRequiredCode);

        if (!IsCanonicalSingleLineValue(request.TenantId)
            || !IsCanonicalSingleLineValue(request.DomainTag)
            || !IsCanonicalSingleLineValue(request.ContextId))
        {
            return Failure("Dynamic graph registry projection scope is invalid.", ScopeInvalidCode);
        }

        if (!IsCanonicalSingleLineValue(request.SignalId)
            || !IsCanonicalSingleLineValue(request.SourceRef)
            || !IsCanonicalSingleLineValue(request.TraceRef))
        {
            return Failure(
                "Dynamic graph registry projection signal references are invalid.",
                SignalReferencesInvalidCode);
        }

        if (request.NodeBindings is not { Length: > 0 })
        {
            return Failure(
                "At least one dynamic graph registry node binding is required.",
                NodeBindingsRequiredCode);
        }

        if (request.EdgeBindings is null)
        {
            return Failure(
                "Dynamic graph registry edge bindings are invalid.",
                EdgeBindingsInvalidCode);
        }

        try
        {
            var snapshot = SnapshotRequest(request);
            var bindingValidation = ValidateBindingSurface(snapshot);
            if (!bindingValidation.IsSuccess)
                return bindingValidation.MapError<DynamicGraphBuildSignal>();

            // Both registry surfaces are read exactly once so one projection observes one bounded snapshot pair.
            var subPhaseOperation = _registry.ListSubPhases();
            var transitionOperation = _registry.ListTransitions();

            if (subPhaseOperation is null || transitionOperation is null)
                return UnexpectedFailure();
            if (!subPhaseOperation.IsSuccess)
                return subPhaseOperation.MapError<DynamicGraphBuildSignal>();
            if (!transitionOperation.IsSuccess)
                return transitionOperation.MapError<DynamicGraphBuildSignal>();
            if (subPhaseOperation.Value is null || transitionOperation.Value is null)
                return UnexpectedFailure();

            var subPhases = SnapshotSubPhases(subPhaseOperation.Value);
            var transitions = SnapshotTransitions(transitionOperation.Value);
            var nodes = ResolveNodes(snapshot.NodeBindings, subPhases);
            if (!nodes.IsSuccess)
                return nodes.MapError<DynamicGraphBuildSignal>();

            var projectedEdges = ResolveEdges(snapshot.EdgeBindings, transitions, nodes.Value!);
            if (!projectedEdges.IsSuccess)
                return projectedEdges.MapError<DynamicGraphBuildSignal>();

            var order = CreateOrder(
                snapshot.TenantId,
                snapshot.DomainTag,
                snapshot.ContextId,
                nodes.Value!,
                projectedEdges.Value!);
            if (!order.IsSuccess)
                return order.MapError<DynamicGraphBuildSignal>();

            var nodesById = nodes.Value!.ToDictionary(node => node.NodeId, StringComparer.Ordinal);
            var orderedNodes = order.Value!
                .Select(entry => nodesById[entry.NodeId])
                .ToArray();
            var topology = new DynamicGraphTopology(
                snapshot.TenantId,
                snapshot.DomainTag,
                snapshot.ContextId,
                orderedNodes,
                projectedEdges.Value!.OrderBy(edge => edge.EdgeId, StringComparer.Ordinal).ToArray(),
                order.Value!);

            var validated = DynamicGraphValidation.Validate(topology);
            if (!validated.IsSuccess)
                return validated.MapError<DynamicGraphBuildSignal>();

            return OperationResult<DynamicGraphBuildSignal>.Ok(new DynamicGraphBuildSignal(
                SignalId: snapshot.SignalId,
                SourceKind: DynamicGraphBuildSourceKind.LogicalRegistryProjection,
                ProposedTopology: validated.Value!,
                PreviousTopology: null,
                SourceRef: snapshot.SourceRef,
                TraceRef: snapshot.TraceRef));
        }
        catch (Exception exception) when (IsNonFatalProjectionException(exception))
        {
            return UnexpectedFailure();
        }
    }

    private static ProjectionSnapshot SnapshotRequest(
        DynamicGraphRegistryProjectionRequest request)
        => new(
            request.TenantId,
            request.DomainTag,
            request.ContextId,
            request.SignalId,
            request.SourceRef,
            request.TraceRef,
            request.NodeBindings.Select(SnapshotNodeBinding).ToArray(),
            request.EdgeBindings.Select(SnapshotEdgeBinding).ToArray());

    private static DynamicGraphRegistryNodeBinding SnapshotNodeBinding(
        DynamicGraphRegistryNodeBinding binding)
        => binding is null
            ? null!
            : new DynamicGraphRegistryNodeBinding(
                binding.LogicalSubPhaseId,
                SnapshotNode(binding.Node));

    private static DynamicGraphRegistryEdgeBinding SnapshotEdgeBinding(
        DynamicGraphRegistryEdgeBinding binding)
        => binding is null
            ? null!
            : new DynamicGraphRegistryEdgeBinding(
                binding.EdgeId,
                binding.LogicalTransitionIds?.ToArray()!,
                binding.Kind,
                binding.ParallelEligible,
                binding.AuthorityUnitId,
                binding.AuthorityUnitLevel,
                binding.DeterministicFinalAuthority,
                binding.BaselineUnitId,
                binding.BaselineRole,
                binding.ExcludedLeakageSignals?.ToArray()!,
                binding.SourceProof,
                binding.CutoverProof,
                binding.AblationProofRef);

    private static IDynamicGraphNode SnapshotNode(IDynamicGraphNode node)
        => node is null
            ? null!
            : new ProjectedNode(
                node.NodeId,
                node.Descriptor is null ? null! : node.Descriptor with { });

    private static OperationResult<bool> ValidateBindingSurface(ProjectionSnapshot snapshot)
    {
        var subPhaseIds = new HashSet<string>(StringComparer.Ordinal);
        var nodeIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var binding in snapshot.NodeBindings)
        {
            if (binding is null
                || !IsCanonicalSingleLineValue(binding.LogicalSubPhaseId)
                || binding.Node is null
                || !IsCanonicalSingleLineValue(binding.Node.NodeId))
            {
                return OperationResult<bool>.Fail(
                    "Dynamic graph registry node binding is invalid.",
                    NodeBindingsRequiredCode);
            }

            if (!subPhaseIds.Add(binding.LogicalSubPhaseId)
                || !nodeIds.Add(binding.Node.NodeId))
            {
                return OperationResult<bool>.Fail(
                    "Dynamic graph registry node binding is duplicated.",
                    DuplicateNodeBindingCode);
            }
        }

        var edgeIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var binding in snapshot.EdgeBindings)
        {
            if (binding is null || !IsCanonicalSingleLineValue(binding.EdgeId))
            {
                return OperationResult<bool>.Fail(
                    "Dynamic graph registry edge binding is invalid.",
                    EdgeBindingsInvalidCode);
            }

            if (!edgeIds.Add(binding.EdgeId))
            {
                return OperationResult<bool>.Fail(
                    "Dynamic graph registry edge binding is duplicated.",
                    DuplicateEdgeBindingCode);
            }

            if (binding.LogicalTransitionIds is not { Length: > 0 }
                || binding.LogicalTransitionIds.Any(id => !IsCanonicalSingleLineValue(id)))
            {
                return OperationResult<bool>.Fail(
                    "At least one logical transition id is required for each graph edge binding.",
                    TransitionsRequiredCode);
            }
        }

        return OperationResult<bool>.Ok(true);
    }

    private static IReadOnlyDictionary<string, LogicalSubPhase> SnapshotSubPhases(
        IReadOnlyList<LogicalSubPhaseEntry> entries)
    {
        var snapshots = new Dictionary<string, LogicalSubPhase>(StringComparer.Ordinal);
        foreach (var entry in entries)
        {
            var subPhase = entry.SubPhase;
            snapshots.Add(
                subPhase.SubPhaseId,
                subPhase with
                {
                    DomainIds = subPhase.DomainIds?.ToArray()!,
                    PredicateIds = subPhase.PredicateIds?.ToArray()!,
                    SourceIds = subPhase.SourceIds?.ToArray()!
                });
        }

        return snapshots;
    }

    private static IReadOnlyDictionary<string, LogicalTransition> SnapshotTransitions(
        IReadOnlyList<LogicalTransitionEntry> entries)
    {
        var snapshots = new Dictionary<string, LogicalTransition>(StringComparer.Ordinal);
        foreach (var entry in entries)
        {
            var transition = entry.Transition;
            snapshots.Add(
                transition.TransitionId,
                transition with
                {
                    PredicateIds = transition.PredicateIds?.ToArray()!,
                    ConstraintIds = transition.ConstraintIds?.ToArray()!,
                    SourceIds = transition.SourceIds?.ToArray()!
                });
        }

        return snapshots;
    }

    private static OperationResult<IReadOnlyList<IDynamicGraphNode>> ResolveNodes(
        IReadOnlyList<DynamicGraphRegistryNodeBinding> bindings,
        IReadOnlyDictionary<string, LogicalSubPhase> subPhases)
    {
        var nodes = new List<IDynamicGraphNode>(bindings.Count);
        foreach (var binding in bindings.OrderBy(value => value.LogicalSubPhaseId, StringComparer.Ordinal))
        {
            if (!subPhases.ContainsKey(binding.LogicalSubPhaseId))
            {
                return OperationResult<IReadOnlyList<IDynamicGraphNode>>.Fail(
                    "A selected logical sub-phase was not found in the registry.",
                    SubPhaseNotFoundCode);
            }

            if (!StringComparer.Ordinal.Equals(binding.Node.NodeId, binding.LogicalSubPhaseId))
            {
                return OperationResult<IReadOnlyList<IDynamicGraphNode>>.Fail(
                    "Dynamic graph node identity does not match its logical sub-phase binding.",
                    NodeMismatchCode);
            }

            nodes.Add(SnapshotNode(binding.Node));
        }

        return OperationResult<IReadOnlyList<IDynamicGraphNode>>.Ok(nodes.ToArray());
    }

    private static OperationResult<IReadOnlyList<IDynamicGraphEdge>> ResolveEdges(
        IReadOnlyList<DynamicGraphRegistryEdgeBinding> bindings,
        IReadOnlyDictionary<string, LogicalTransition> transitions,
        IReadOnlyList<IDynamicGraphNode> nodes)
    {
        var selectedNodeIds = nodes.Select(node => node.NodeId).ToHashSet(StringComparer.Ordinal);
        var inducedTransitionIds = transitions.Values
            .Where(transition => IsInducedTransition(transition, selectedNodeIds))
            .Select(transition => transition.TransitionId)
            .ToHashSet(StringComparer.Ordinal);
        var coveredTransitionIds = new HashSet<string>(StringComparer.Ordinal);
        var edges = new List<IDynamicGraphEdge>(bindings.Count);

        foreach (var binding in bindings.OrderBy(value => value.EdgeId, StringComparer.Ordinal))
        {
            var boundTransitions = new List<LogicalTransition>(binding.LogicalTransitionIds.Length);
            foreach (var transitionId in binding.LogicalTransitionIds)
            {
                if (!transitions.TryGetValue(transitionId, out var transition)
                    || !inducedTransitionIds.Contains(transitionId))
                {
                    return OperationResult<IReadOnlyList<IDynamicGraphEdge>>.Fail(
                        "A bound logical transition is not projectable into the selected induced subgraph.",
                        NonProjectableTransitionCode);
                }

                if (!coveredTransitionIds.Add(transitionId))
                {
                    return OperationResult<IReadOnlyList<IDynamicGraphEdge>>.Fail(
                        "A logical transition is covered by more than one graph edge binding.",
                        DuplicateTransitionCoverageCode);
                }

                boundTransitions.Add(transition);
            }

            var sourceNodeId = boundTransitions[0].FromTopologyId;
            if (boundTransitions.Any(transition =>
                    !StringComparer.Ordinal.Equals(transition.FromTopologyId, sourceNodeId)))
            {
                return OperationResult<IReadOnlyList<IDynamicGraphEdge>>.Fail(
                    "All logical transitions in one graph edge binding must share one source.",
                    EdgeSourceMismatchCode);
            }

            // Targets are deliberately not deduplicated: duplicate registry targets remain visible to
            // DynamicGraphValidation and produce its canonical DYNAMIC_GRAPH_DUPLICATE_EDGE_TARGET code.
            var targetNodeIds = boundTransitions
                .Select(transition => transition.ToTopologyId)
                .OrderBy(target => target, StringComparer.Ordinal)
                .ToArray();
            edges.Add(new ProjectedEdge(
                binding.EdgeId,
                sourceNodeId,
                Array.AsReadOnly(targetNodeIds),
                binding.Kind,
                binding.ParallelEligible,
                binding.AuthorityUnitId,
                binding.AuthorityUnitLevel,
                binding.DeterministicFinalAuthority,
                binding.BaselineUnitId,
                binding.BaselineRole,
                binding.ExcludedLeakageSignals is null
                    ? null!
                    : Array.AsReadOnly(binding.ExcludedLeakageSignals.ToArray()),
                binding.SourceProof,
                binding.CutoverProof,
                binding.AblationProofRef));
        }

        if (!coveredTransitionIds.SetEquals(inducedTransitionIds))
        {
            return OperationResult<IReadOnlyList<IDynamicGraphEdge>>.Fail(
                "Every transition in the selected induced subgraph must be covered exactly once.",
                IncompleteTransitionCoverageCode);
        }

        return OperationResult<IReadOnlyList<IDynamicGraphEdge>>.Ok(edges.ToArray());
    }

    private static bool IsInducedTransition(
        LogicalTransition transition,
        IReadOnlySet<string> selectedNodeIds)
        => StringComparer.Ordinal.Equals(transition.FromTopologyKind, SubPhaseTopologyKind)
           && StringComparer.Ordinal.Equals(transition.ToTopologyKind, SubPhaseTopologyKind)
           && selectedNodeIds.Contains(transition.FromTopologyId)
           && selectedNodeIds.Contains(transition.ToTopologyId);

    private static OperationResult<IReadOnlyList<DynamicGraphNodeOrder>> CreateOrder(
        string tenantId,
        string domainTag,
        string contextId,
        IReadOnlyList<IDynamicGraphNode> nodes,
        IReadOnlyList<IDynamicGraphEdge> edges)
    {
        var nodeIds = nodes.Select(node => node.NodeId).OrderBy(nodeId => nodeId, StringComparer.Ordinal).ToArray();
        var adjacency = nodeIds.ToDictionary(nodeId => nodeId, _ => new List<string>(), StringComparer.Ordinal);
        var indegree = nodeIds.ToDictionary(nodeId => nodeId, _ => 0, StringComparer.Ordinal);

        // Kahn traversal would otherwise classify an ordinary self-transition as a generic cycle.
        // Preserve the canonical validator contract before any indegree analysis can mask it.
        var selfEdge = edges
            .OrderBy(edge => edge.EdgeId, StringComparer.Ordinal)
            .FirstOrDefault(edge => edge.ToNodeIds.Contains(edge.FromNodeId, StringComparer.Ordinal));
        if (selfEdge is not null)
        {
            return OperationResult<IReadOnlyList<DynamicGraphNodeOrder>>.Fail(
                $"Dynamic graph edge '{selfEdge.EdgeId}' cannot target its own source node.",
                "DYNAMIC_GRAPH_SELF_EDGE");
        }

        foreach (var edge in edges.Where(edge => edge.Kind != GraphEdgeKind.ReturnLoop))
        {
            foreach (var targetNodeId in edge.ToNodeIds)
            {
                adjacency[edge.FromNodeId].Add(targetNodeId);
                indegree[targetNodeId]++;
            }
        }

        var ready = new SortedSet<string>(
            indegree.Where(pair => pair.Value == 0).Select(pair => pair.Key),
            StringComparer.Ordinal);
        var orderedNodeIds = new List<string>(nodeIds.Length);
        while (ready.Count > 0)
        {
            var nodeId = ready.Min!;
            ready.Remove(nodeId);
            orderedNodeIds.Add(nodeId);

            foreach (var targetNodeId in adjacency[nodeId].OrderBy(value => value, StringComparer.Ordinal))
            {
                indegree[targetNodeId]--;
                if (indegree[targetNodeId] == 0)
                    ready.Add(targetNodeId);
            }
        }

        if (orderedNodeIds.Count != nodeIds.Length)
        {
            return OperationResult<IReadOnlyList<DynamicGraphNodeOrder>>.Fail(
                "Dynamic graph topology contains an ordinary cycle.",
                "DYNAMIC_GRAPH_CYCLE_DETECTED");
        }

        IReadOnlyList<DynamicGraphNodeOrder> order = Array.AsReadOnly(
            orderedNodeIds.Select((nodeId, position) => new DynamicGraphNodeOrder(
                OrderId: CreateOrderId(tenantId, domainTag, contextId, nodeId),
                NodeId: nodeId,
                TenantId: tenantId,
                DomainTag: domainTag,
                ContextId: contextId,
                Position: position)).ToArray());
        return OperationResult<IReadOnlyList<DynamicGraphNodeOrder>>.Ok(order);
    }

    private static string CreateOrderId(
        string tenantId,
        string domainTag,
        string contextId,
        string nodeId)
    {
        var signature = string.Join(
            '\n',
            "registry-order-v1",
            LengthPrefixed("tenant", tenantId),
            LengthPrefixed("domain", domainTag),
            LengthPrefixed("context", contextId),
            LengthPrefixed("node", nodeId));
        return $"registry-order-{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(signature))).ToLowerInvariant()}";
    }

    private static string LengthPrefixed(string name, string value)
        => $"{name}:{Encoding.UTF8.GetByteCount(value)}:{value}";

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

    private static bool IsNonFatalProjectionException(Exception exception)
        => exception is not OutOfMemoryException
           && exception is not StackOverflowException
           && exception is not AccessViolationException
           && exception is not AppDomainUnloadedException
           && exception is not BadImageFormatException
           && !StringComparer.Ordinal.Equals(
               exception.GetType().FullName,
               "System.Threading.ThreadAbortException");

    private static OperationResult<DynamicGraphBuildSignal> UnexpectedFailure()
        => Failure(
            "Dynamic graph registry projection failed unexpectedly.",
            ExceptionCode);

    private static OperationResult<DynamicGraphBuildSignal> Failure(string message, string code)
        => OperationResult<DynamicGraphBuildSignal>.Fail(message, code);

    private sealed record ProjectionSnapshot(
        string TenantId,
        string DomainTag,
        string ContextId,
        string SignalId,
        string SourceRef,
        string TraceRef,
        DynamicGraphRegistryNodeBinding[] NodeBindings,
        DynamicGraphRegistryEdgeBinding[] EdgeBindings);

    private sealed record ProjectedNode(
        string NodeId,
        DynamicGraphNodeDescriptor Descriptor) : IDynamicGraphNode;

    private sealed record ProjectedEdge(
        string EdgeId,
        string FromNodeId,
        IReadOnlyList<string> ToNodeIds,
        GraphEdgeKind Kind,
        bool ParallelEligible,
        string AuthorityUnitId,
        string AuthorityUnitLevel,
        bool DeterministicFinalAuthority,
        string? BaselineUnitId,
        string BaselineRole,
        IReadOnlyList<string> ExcludedLeakageSignals,
        string SourceProof,
        string CutoverProof,
        string AblationProofRef) : IDynamicGraphEdge;
}
