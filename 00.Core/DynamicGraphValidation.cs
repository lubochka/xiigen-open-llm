// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

// This validator applies deterministic structural and authority guards to one immutable topology snapshot.
public static class DynamicGraphValidation
{
    // DomainAgnosticTag is the only descriptor-domain value allowed to span a topology's declared domain.
    public const string DomainAgnosticTag = "*";

    // These are the only owner-defined semantic roles accepted on a node passport.
    private static readonly string[] NodeAuthorityRoles =
        ["trained_node", RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole, "diagnostics_only"];

    // These are the only owner-defined non-authoritative roles accepted for a baseline unit.
    private static readonly string[] BaselineRoles =
        ["candidate_generator", "ablation_baseline"];

    // Validate returns the accepted snapshot unchanged or the first deterministic structural failure.
    public static OperationResult<DynamicGraphTopology> Validate(DynamicGraphTopology? topology)
    {
        // A missing topology cannot carry any of the required graph identity or scope data.
        if (topology is null)
            return Failure("Dynamic graph topology is required.", "DYNAMIC_GRAPH_TOPOLOGY_REQUIRED");

        // Tenant isolation requires a non-blank topology tenant carrier.
        if (!IsCanonicalSingleLineValue(topology.TenantId))
            return Failure("Dynamic graph topology tenant id is required.", "DYNAMIC_GRAPH_TENANT_REQUIRED");

        // Domain isolation requires a non-blank topology domain carrier.
        if (!IsCanonicalSingleLineValue(topology.DomainTag))
            return Failure("Dynamic graph topology domain tag is required.", "DYNAMIC_GRAPH_DOMAIN_REQUIRED");

        // Context isolation requires a non-blank topology context carrier.
        if (!IsCanonicalSingleLineValue(topology.ContextId))
            return Failure("Dynamic graph topology context id is required.", "DYNAMIC_GRAPH_CONTEXT_REQUIRED");

        // A graph snapshot must contain at least one executable node.
        if (topology.Nodes.Count == 0)
            return Failure("Dynamic graph topology must contain at least one node.", "DYNAMIC_GRAPH_NODES_REQUIRED");

        // Build stable node lookup state while validating node passports and unique identities.
        var nodesById = new Dictionary<string, IDynamicGraphNode>(StringComparer.Ordinal);

        // Validate nodes in snapshot order so the first failure remains deterministic.
        foreach (var node in topology.Nodes)
        {
            // Null node entries remain validator-visible even though the topology owns its carriers.
            if (node is null)
                return Failure("Dynamic graph topology contains a null node.", "DYNAMIC_GRAPH_NODE_REQUIRED");

            // Every node needs a stable, non-blank topology identity.
            if (!IsCanonicalSingleLineValue(node.NodeId))
                return Failure("Dynamic graph node id is required.", "DYNAMIC_GRAPH_NODE_ID_REQUIRED");

            // Node identities are unique within a topology snapshot.
            if (!nodesById.TryAdd(node.NodeId, node))
                return Failure($"Duplicate dynamic graph node id '{node.NodeId}'.", "DYNAMIC_GRAPH_DUPLICATE_NODE_ID");

            // Every node needs the canonical descriptor passport.
            var descriptor = node.Descriptor;
            if (descriptor is null)
                return Failure($"Dynamic graph node '{node.NodeId}' descriptor is required.", "DYNAMIC_GRAPH_NODE_DESCRIPTOR_REQUIRED");

            // The non-generic node identity and descriptor identity must never drift apart.
            if (!StringComparer.Ordinal.Equals(node.NodeId, descriptor.NodeId))
                return Failure($"Dynamic graph node '{node.NodeId}' descriptor id does not match.", "DYNAMIC_GRAPH_NODE_DESCRIPTOR_ID_MISMATCH");

            // The declared input type is a required half of the node's input/output passport.
            if (!IsCanonicalSingleLineValue(descriptor.TInputTypeName))
                return Failure($"Dynamic graph node '{node.NodeId}' input type name is required.", "DYNAMIC_GRAPH_NODE_INPUT_TYPE_REQUIRED");

            // The declared output type is a required half of the node's input/output passport.
            if (!IsCanonicalSingleLineValue(descriptor.TOutputTypeName))
                return Failure($"Dynamic graph node '{node.NodeId}' output type name is required.", "DYNAMIC_GRAPH_NODE_OUTPUT_TYPE_REQUIRED");

            // A node must declare either this topology's domain or the explicit domain-agnostic wildcard.
            if (IsBlank(descriptor.DomainTag))
                return Failure($"Dynamic graph node '{node.NodeId}' domain tag is required.", "DYNAMIC_GRAPH_NODE_DOMAIN_REQUIRED");

            // A foreign node domain cannot silently enter a topology owned by another domain.
            if (!StringComparer.Ordinal.Equals(descriptor.DomainTag, topology.DomainTag)
                && !StringComparer.Ordinal.Equals(descriptor.DomainTag, DomainAgnosticTag))
            {
                return Failure($"Dynamic graph node '{node.NodeId}' domain does not match the topology domain.", "DYNAMIC_GRAPH_DOMAIN_MISMATCH");
            }

            // Blank and arbitrary authority roles cannot create an unowned authority path.
            if (IsBlank(descriptor.AuthorityRole))
                return Failure($"Dynamic graph node '{node.NodeId}' authority role is required.", "DYNAMIC_GRAPH_NODE_AUTHORITY_ROLE_REQUIRED");
            if (!NodeAuthorityRoles.Contains(descriptor.AuthorityRole, StringComparer.Ordinal))
                return Failure($"Dynamic graph node '{node.NodeId}' authority role is invalid.", "DYNAMIC_GRAPH_NODE_AUTHORITY_ROLE_INVALID");

            // A deterministic helper cannot claim final semantic authority through a graph node passport.
            if (descriptor.DeterministicFinalAuthority)
                return Failure($"Dynamic graph node '{node.NodeId}' cannot use deterministic final authority.", "DYNAMIC_GRAPH_NODE_DETERMINISTIC_AUTHORITY_FORBIDDEN");

            // An untrained node cannot claim runtime semantic quality.
            if (descriptor.RuntimeSemanticQualityClaimed && !descriptor.IsTrained)
                return Failure($"Dynamic graph node '{node.NodeId}' cannot claim semantic quality while untrained.", "DYNAMIC_GRAPH_NODE_UNTRAINED_SEMANTIC_CLAIM");

            // A runtime semantic-quality claim also requires the exact trained-node authority role.
            if (descriptor.RuntimeSemanticQualityClaimed
                && !StringComparer.Ordinal.Equals(descriptor.AuthorityRole, "trained_node"))
            {
                return Failure($"Dynamic graph node '{node.NodeId}' semantic-quality claim requires the trained-node role.", "DYNAMIC_GRAPH_NODE_SEMANTIC_CLAIM_ROLE_REQUIRED");
            }

            // The trained-node role and trained state are equivalent; baseline and diagnostics roles remain untrained.
            var hasTrainedRole = StringComparer.Ordinal.Equals(descriptor.AuthorityRole, "trained_node");
            if (descriptor.IsTrained != hasTrainedRole)
                return Failure($"Dynamic graph node '{node.NodeId}' trained state does not match its authority role.", "DYNAMIC_GRAPH_NODE_AUTHORITY_STATE_MISMATCH");

            // A trained passport must identify the checkpoint that made the claim possible.
            if (descriptor.IsTrained && !IsCanonicalSingleLineValue(descriptor.CheckpointRef))
                return Failure($"Dynamic graph node '{node.NodeId}' trained checkpoint reference is required.", "DYNAMIC_GRAPH_NODE_CHECKPOINT_REQUIRED");

            // A trained passport must identify the manifest that describes the checkpoint.
            if (descriptor.IsTrained && !IsCanonicalSingleLineValue(descriptor.ManifestRef))
                return Failure($"Dynamic graph node '{node.NodeId}' trained manifest reference is required.", "DYNAMIC_GRAPH_NODE_MANIFEST_REQUIRED");
        }

        // Every node needs exactly one scoped order entry so graph identity is explicit.
        if (topology.Order.Count != topology.Nodes.Count)
            return Failure("Dynamic graph topology order must contain exactly one entry per node.", "DYNAMIC_GRAPH_ORDER_INCOMPLETE");

        // Track stable order identities, node assignments, and positions independently.
        var orderIds = new HashSet<string>(StringComparer.Ordinal);
        var orderedNodeIds = new HashSet<string>(StringComparer.Ordinal);
        var positions = new HashSet<int>();
        var positionByNodeId = new Dictionary<string, int>(StringComparer.Ordinal);

        // Validate order entries in snapshot order before edge analysis.
        foreach (var entry in topology.Order)
        {
            // Null order entries are invalid required carriers.
            if (entry is null)
                return Failure("Dynamic graph topology contains a null order entry.", "DYNAMIC_GRAPH_ORDER_ENTRY_REQUIRED");

            // Each order entry needs its own stable identity for traces and later snapshots.
            if (!IsCanonicalSingleLineValue(entry.OrderId))
                return Failure("Dynamic graph order id is required.", "DYNAMIC_GRAPH_ORDER_ID_REQUIRED");

            // Stable order identities cannot be reused within the same topology snapshot.
            if (!orderIds.Add(entry.OrderId))
                return Failure($"Duplicate dynamic graph order id '{entry.OrderId}'.", "DYNAMIC_GRAPH_DUPLICATE_ORDER_ID");

            // Each order entry must bind a concrete topology node.
            if (!IsCanonicalSingleLineValue(entry.NodeId))
                return Failure("Dynamic graph order node id is required.", "DYNAMIC_GRAPH_ORDER_NODE_ID_REQUIRED");

            // A node cannot receive two different positions in one topology snapshot.
            if (!orderedNodeIds.Add(entry.NodeId))
                return Failure($"Dynamic graph node '{entry.NodeId}' has duplicate order entries.", "DYNAMIC_GRAPH_DUPLICATE_ORDER_NODE");

            // The order cannot refer to a node outside the topology snapshot.
            if (!nodesById.ContainsKey(entry.NodeId))
                return Failure($"Dynamic graph order refers to missing node '{entry.NodeId}'.", "DYNAMIC_GRAPH_DANGLING_ORDER_NODE");

            // Positions are zero-based and cannot be negative.
            if (entry.Position < 0)
                return Failure($"Dynamic graph node '{entry.NodeId}' order position cannot be negative.", "DYNAMIC_GRAPH_ORDER_POSITION_INVALID");

            // Two nodes cannot own the same deterministic position.
            if (!positions.Add(entry.Position))
                return Failure($"Duplicate dynamic graph order position '{entry.Position}'.", "DYNAMIC_GRAPH_DUPLICATE_ORDER_POSITION");

            // Order entries cannot silently cross the topology tenant boundary.
            if (!StringComparer.Ordinal.Equals(entry.TenantId, topology.TenantId))
                return Failure($"Dynamic graph node '{entry.NodeId}' tenant does not match the topology tenant.", "DYNAMIC_GRAPH_TENANT_MISMATCH");

            // Order entries cannot silently cross the topology domain boundary.
            if (!StringComparer.Ordinal.Equals(entry.DomainTag, topology.DomainTag))
                return Failure($"Dynamic graph node '{entry.NodeId}' order domain does not match the topology domain.", "DYNAMIC_GRAPH_DOMAIN_MISMATCH");

            // Order entries must belong to the same topology build/execution context.
            if (!StringComparer.Ordinal.Equals(entry.ContextId, topology.ContextId))
                return Failure($"Dynamic graph node '{entry.NodeId}' context does not match the topology context.", "DYNAMIC_GRAPH_CONTEXT_MISMATCH");

            // Retain the position for ReturnLoop direction and deterministic DAG queue ordering.
            positionByNodeId.Add(entry.NodeId, entry.Position);
        }

        // Positions must be exactly the contiguous zero-based set 0..Nodes.Count-1.
        for (var expectedPosition = 0; expectedPosition < topology.Nodes.Count; expectedPosition++)
        {
            if (!positions.Contains(expectedPosition))
                return Failure("Dynamic graph topology order positions must be contiguous and zero-based.", "DYNAMIC_GRAPH_ORDER_NOT_CONTIGUOUS");
        }

        // Track edge identities and grouped cardinality evidence independently.
        var edgeIds = new HashSet<string>(StringComparer.Ordinal);
        var fanInSourcesByTarget = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        var manyToManyGroups = new List<EndpointGroup>();

        // Only ordinary forward relations enter Kahn adjacency; declarative ReturnLoop arcs do not.
        var adjacency = nodesById.Keys.ToDictionary(
            nodeId => nodeId,
            _ => new List<string>(),
            StringComparer.Ordinal);
        var indegree = nodesById.Keys.ToDictionary(
            nodeId => nodeId,
            _ => 0,
            StringComparer.Ordinal);

        // Validate every relation before the ordinary DAG detector consumes it.
        foreach (var edge in topology.Edges)
        {
            // Null edge entries are invalid required carriers.
            if (edge is null)
                return Failure("Dynamic graph topology contains a null edge.", "DYNAMIC_GRAPH_EDGE_REQUIRED");

            // Every relation needs a stable identity.
            if (!IsCanonicalSingleLineValue(edge.EdgeId))
                return Failure("Dynamic graph edge id is required.", "DYNAMIC_GRAPH_EDGE_ID_REQUIRED");

            // Relation identities are unique within one topology snapshot.
            if (!edgeIds.Add(edge.EdgeId))
                return Failure($"Duplicate dynamic graph edge id '{edge.EdgeId}'.", "DYNAMIC_GRAPH_DUPLICATE_EDGE_ID");

            // Enum values outside the closed five-kind set are rejected even when produced by an unsafe cast.
            if (!Enum.IsDefined(typeof(GraphEdgeKind), edge.Kind))
                return Failure($"Dynamic graph edge '{edge.EdgeId}' kind is invalid.", "DYNAMIC_GRAPH_EDGE_KIND_INVALID");

            // Every relation needs a named source endpoint that belongs to this snapshot.
            if (!IsCanonicalSingleLineValue(edge.FromNodeId))
                return Failure($"Dynamic graph edge '{edge.EdgeId}' source node id is required.", "DYNAMIC_GRAPH_EDGE_SOURCE_REQUIRED");
            if (!nodesById.ContainsKey(edge.FromNodeId))
                return Failure($"Dynamic graph edge '{edge.EdgeId}' refers to missing source node '{edge.FromNodeId}'.", "DYNAMIC_GRAPH_DANGLING_SOURCE_NODE");

            // Every relation kind carries at least one explicit target endpoint.
            if (edge.ToNodeIds is null || edge.ToNodeIds.Count == 0)
                return Failure($"Dynamic graph edge '{edge.EdgeId}' must contain at least one target node id.", "DYNAMIC_GRAPH_EDGE_TARGETS_REQUIRED");

            // Grouped and correction cardinality rules pin the owner meanings represented by this edge model.
            if (edge.Kind == GraphEdgeKind.FanIn && edge.ToNodeIds.Count != 1)
                return Failure($"Dynamic graph FanIn edge '{edge.EdgeId}' must target exactly one node.", "DYNAMIC_GRAPH_FAN_IN_CARDINALITY_INVALID");
            if (edge.Kind == GraphEdgeKind.ManyToMany && edge.ToNodeIds.Count < 2)
                return Failure($"Dynamic graph ManyToMany edge '{edge.EdgeId}' must target at least two nodes.", "DYNAMIC_GRAPH_MANY_TO_MANY_CARDINALITY_INVALID");
            if (edge.Kind == GraphEdgeKind.ReturnLoop && edge.ToNodeIds.Count != 1)
                return Failure($"Dynamic graph ReturnLoop edge '{edge.EdgeId}' must target exactly one node.", "DYNAMIC_GRAPH_RETURN_LOOP_CARDINALITY_INVALID");
            if (edge.Kind == GraphEdgeKind.ReturnLoop && edge.ParallelEligible)
                return Failure($"Dynamic graph ReturnLoop edge '{edge.EdgeId}' cannot be parallel eligible.", "DYNAMIC_GRAPH_RETURN_LOOP_PARALLEL_FORBIDDEN");

            // Learned edge authority is always referenced by a non-blank registry identity at exact owner level L2.
            if (!IsCanonicalSingleLineValue(edge.AuthorityUnitId))
                return Failure($"Dynamic graph edge '{edge.EdgeId}' authority unit id is required.", "DYNAMIC_GRAPH_EDGE_AUTHORITY_REQUIRED");
            if (IsBlank(edge.AuthorityUnitLevel))
                return Failure($"Dynamic graph edge '{edge.EdgeId}' authority unit level is required.", "DYNAMIC_GRAPH_EDGE_AUTHORITY_LEVEL_REQUIRED");
            if (!StringComparer.Ordinal.Equals(edge.AuthorityUnitLevel, "L2"))
                return Failure($"Dynamic graph edge '{edge.EdgeId}' authority unit level must be L2.", "DYNAMIC_GRAPH_EDGE_AUTHORITY_LEVEL_INVALID");

            // A deterministic helper cannot bypass the learned authority boundary on an accepted edge.
            if (edge.DeterministicFinalAuthority)
                return Failure($"Dynamic graph edge '{edge.EdgeId}' cannot use deterministic final authority.", "DYNAMIC_GRAPH_EDGE_DETERMINISTIC_AUTHORITY_FORBIDDEN");

            // A baseline identity and an exact non-authoritative role must remain coherent.
            if (!IsCanonicalSingleLineValue(edge.BaselineUnitId))
                return Failure($"Dynamic graph edge '{edge.EdgeId}' baseline unit id is required.", "DYNAMIC_GRAPH_EDGE_BASELINE_UNIT_REQUIRED");
            if (IsBlank(edge.BaselineRole))
                return Failure($"Dynamic graph edge '{edge.EdgeId}' baseline role is required.", "DYNAMIC_GRAPH_EDGE_BASELINE_ROLE_REQUIRED");
            if (!BaselineRoles.Contains(edge.BaselineRole, StringComparer.Ordinal))
                return Failure($"Dynamic graph edge '{edge.EdgeId}' baseline role is invalid.", "DYNAMIC_GRAPH_EDGE_BASELINE_ROLE_INVALID");

            // Structural admission requires the same exact ordered vocabulary consumed by runtime authority guards.
            if (edge.ExcludedLeakageSignals is null)
                return Failure($"Dynamic graph edge '{edge.EdgeId}' leakage exclusion list is required.", "DYNAMIC_GRAPH_EDGE_LEAKAGE_EXCLUSIONS_REQUIRED");
            if (!DynamicGraphEdgeAuthorityLeakageSignalVocabulary.IsExact(edge.ExcludedLeakageSignals))
                return Failure($"Dynamic graph edge '{edge.EdgeId}' leakage exclusions must match the exact ordered vocabulary.", "DYNAMIC_GRAPH_EDGE_LEAKAGE_EXCLUSION_REQUIRED");

            // Relation proof carriers keep topology construction and authority cutover auditable.
            if (IsBlank(edge.SourceProof))
                return Failure($"Dynamic graph edge '{edge.EdgeId}' source proof is required.", "DYNAMIC_GRAPH_EDGE_SOURCE_PROOF_REQUIRED");
            if (IsBlank(edge.CutoverProof))
                return Failure($"Dynamic graph edge '{edge.EdgeId}' cutover proof is required.", "DYNAMIC_GRAPH_EDGE_CUTOVER_PROOF_REQUIRED");
            if (!IsCanonicalSingleLineValue(edge.AblationProofRef))
                return Failure($"Dynamic graph edge '{edge.EdgeId}' ablation proof reference is required.", "DYNAMIC_GRAPH_EDGE_ABLATION_PROOF_REQUIRED");

            // A relation cannot repeat, omit, dangle, or self-target an endpoint.
            var targetIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var targetNodeId in edge.ToNodeIds)
            {
                if (!IsCanonicalSingleLineValue(targetNodeId))
                    return Failure($"Dynamic graph edge '{edge.EdgeId}' contains a blank target node id.", "DYNAMIC_GRAPH_EDGE_TARGET_REQUIRED");
                if (!targetIds.Add(targetNodeId))
                    return Failure($"Dynamic graph edge '{edge.EdgeId}' repeats target node '{targetNodeId}'.", "DYNAMIC_GRAPH_DUPLICATE_EDGE_TARGET");
                if (!nodesById.ContainsKey(targetNodeId))
                    return Failure($"Dynamic graph edge '{edge.EdgeId}' refers to missing target node '{targetNodeId}'.", "DYNAMIC_GRAPH_DANGLING_TARGET_NODE");
                if (StringComparer.Ordinal.Equals(edge.FromNodeId, targetNodeId))
                    return Failure($"Dynamic graph edge '{edge.EdgeId}' cannot target its own source node.", "DYNAMIC_GRAPH_SELF_EDGE");
            }

            // ReturnLoop is a declarative correction back-edge: L3 initiates it and L4 may execute it later.
            if (edge.Kind == GraphEdgeKind.ReturnLoop)
            {
                var returnTarget = edge.ToNodeIds[0];
                if (positionByNodeId[returnTarget] >= positionByNodeId[edge.FromNodeId])
                    return Failure($"Dynamic graph ReturnLoop edge '{edge.EdgeId}' must target an earlier ordered node.", "DYNAMIC_GRAPH_RETURN_LOOP_DIRECTION_INVALID");

                // Deliberately exclude a valid correction arc from the ordinary acyclic execution plan.
                continue;
            }

            // Ordinary relations participate in structural DAG validation.
            foreach (var targetNodeId in edge.ToNodeIds)
            {
                adjacency[edge.FromNodeId].Add(targetNodeId);
                indegree[targetNodeId]++;
            }

            // FanIn cardinality is expressed by a stable target group across several one-source edges.
            if (edge.Kind == GraphEdgeKind.FanIn)
            {
                var targetNodeId = edge.ToNodeIds[0];
                if (!fanInSourcesByTarget.TryGetValue(targetNodeId, out var sources))
                {
                    sources = new HashSet<string>(StringComparer.Ordinal);
                    fanInSourcesByTarget.Add(targetNodeId, sources);
                }

                sources.Add(edge.FromNodeId);
            }

            // ManyToMany cardinality is expressed by distinct sources sharing one stable multi-target set.
            if (edge.Kind == GraphEdgeKind.ManyToMany)
            {
                var group = manyToManyGroups.FirstOrDefault(candidate => candidate.TargetNodeIds.SetEquals(targetIds));
                if (group is null)
                {
                    group = new EndpointGroup(targetIds);
                    manyToManyGroups.Add(group);
                }

                group.SourceNodeIds.Add(edge.FromNodeId);
            }
        }

        // Every FanIn target must merge at least two distinct source branches.
        foreach (var group in fanInSourcesByTarget.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            if (group.Value.Count < 2)
                return Failure($"Dynamic graph FanIn target '{group.Key}' must have at least two distinct sources.", "DYNAMIC_GRAPH_FAN_IN_CARDINALITY_INVALID");
        }

        // Every ManyToMany endpoint set must be supplied by at least two distinct sources.
        foreach (var group in manyToManyGroups.OrderBy(candidate => candidate.StableTargetLabel, StringComparer.Ordinal))
        {
            if (group.SourceNodeIds.Count < 2)
                return Failure($"Dynamic graph ManyToMany target set '{group.StableTargetLabel}' must have at least two distinct sources.", "DYNAMIC_GRAPH_MANY_TO_MANY_CARDINALITY_INVALID");
        }

        // Compare ready nodes by stable position first and stable node identity second.
        var readyComparer = Comparer<string>.Create((left, right) =>
        {
            if (StringComparer.Ordinal.Equals(left, right))
                return 0;

            var positionComparison = positionByNodeId[left].CompareTo(positionByNodeId[right]);
            return positionComparison != 0
                ? positionComparison
                : StringComparer.Ordinal.Compare(left, right);
        });

        // Seed Kahn traversal with every node that has no incoming ordinary relation.
        var ready = new SortedSet<string>(readyComparer);
        foreach (var pair in indegree)
        {
            if (pair.Value == 0)
                ready.Add(pair.Key);
        }

        // Consume the ordinary DAG in deterministic order.
        var visitedNodeCount = 0;
        while (ready.Count > 0)
        {
            var nodeId = ready.Min!;
            ready.Remove(nodeId);
            visitedNodeCount++;

            foreach (var targetNodeId in adjacency[nodeId]
                         .OrderBy(targetId => positionByNodeId[targetId])
                         .ThenBy(targetId => targetId, StringComparer.Ordinal))
            {
                indegree[targetNodeId]--;
                if (indegree[targetNodeId] == 0)
                    ready.Add(targetNodeId);
            }
        }

        // Any unvisited node belongs to an ordinary structural cycle.
        if (visitedNodeCount != nodesById.Count)
            return Failure("Dynamic graph topology contains an ordinary cycle.", "DYNAMIC_GRAPH_CYCLE_DETECTED");

        // Return the same immutable snapshot after every structural and authority invariant passes.
        return OperationResult<DynamicGraphTopology>.Ok(topology);
    }

    // IsBlank centralizes required string-carrier validation without normalizing identities.
    private static bool IsBlank(string? value)
        => string.IsNullOrWhiteSpace(value);

    // Scope identities must already be canonical single-line values; they are never normalized here.
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

    // Failure keeps every validation exit on the shared OperationResult contract.
    private static OperationResult<DynamicGraphTopology> Failure(string message, string code)
        => OperationResult<DynamicGraphTopology>.Fail(message, code);

    // EndpointGroup retains structural set identity without delimiter-based string-key collisions.
    private sealed class EndpointGroup
    {
        public EndpointGroup(IEnumerable<string> targetNodeIds)
        {
            TargetNodeIds = new HashSet<string>(targetNodeIds, StringComparer.Ordinal);
            SourceNodeIds = new HashSet<string>(StringComparer.Ordinal);
            StableTargetLabel = string.Join(",", TargetNodeIds.OrderBy(nodeId => nodeId, StringComparer.Ordinal));
        }

        public HashSet<string> TargetNodeIds { get; }

        public HashSet<string> SourceNodeIds { get; }

        public string StableTargetLabel { get; }
    }
}
