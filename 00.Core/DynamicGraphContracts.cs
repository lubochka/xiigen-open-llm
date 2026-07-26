// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Collections.ObjectModel;
using AgentInfrastructure;

// This interface is the non-generic identity and descriptor boundary stored by graph topology snapshots.
public interface IDynamicGraphNode
{
    // NodeId is the stable topology identity and must equal Descriptor.NodeId.
    string NodeId { get; }

    // Descriptor carries the declared domain and input/output shape without exposing training operations.
    DynamicGraphNodeDescriptor Descriptor { get; }
}

// This interface is the typed, inference-only node boundary layered on the existing logical-unit level.
public interface IDynamicGraphNode<TInput, TOutput> : IDynamicGraphNode
{
    // ExecuteAsync preserves the canonical queryable many-to-many candidate surface.
    Task<OperationResult<LogicalUnitResult<IQueryable<GraphNodeCandidate>>>> ExecuteAsync(
        LogicalUnitRequest<TInput> request,
        CancellationToken cancellationToken = default);
}

// This record is the canonical ten-field passport for a dynamic-graph node.
public sealed record DynamicGraphNodeDescriptor(
    string NodeId,                         // Stable topology identity.
    string DomainTag,                      // Declared home domain or explicit wildcard.
    string TInputTypeName,                 // Declared input shape.
    string TOutputTypeName,                // Declared requested-output shape.
    bool IsTrained,                        // True only for a validated published checkpoint.
    bool DeterministicFinalAuthority,      // Must remain false on accepted graph nodes.
    bool RuntimeSemanticQualityClaimed,    // Must remain false without trained evidence.
    string AuthorityRole,                  // Trained, baseline-candidate, or diagnostics-only role.
    string? CheckpointRef,                 // Published checkpoint identity when trained.
    string? ManifestRef);                  // Published checkpoint manifest identity when trained.

// This record is one canonical candidate on the node's IQueryable many-to-many output boundary.
public sealed record GraphNodeCandidate(
    string CandidateId,                    // Stable candidate identity.
    string UnitId,                         // Primary producing logical-unit identity.
    string[] UnitIds,                      // All producing logical-unit identities.
    string LogicalUnitRef,                 // Stable producer reference.
    string[] OutputRefs,                   // Materialized requested-type payload references.
    string OutputType,                     // Requested payload type name.
    double Confidence,                     // Candidate confidence supplied by the node.
    double? Score,                         // Optional raw candidate score.
    string[] ProvenanceRefs,               // Provenance envelope references.
    string PathOrderRef,                   // Graph-order evidence reference.
    string LegalStatus,                    // Legal-admission status.
    string Status,                         // Candidate lifecycle status.
    string[] ErrorCodes,                   // Candidate-specific failure codes.
    string[] SourceIds,                    // Source-disjoint evidence identities.
    string TraceRef,                       // Primary trace identity.
    string[] TraceRefs,                    // Supporting trace identities.
    string[] EvidenceIds,                  // Supporting evidence identities.
    string B1TeacherJudgeRef,              // Teacher/judge gate handoff reference.
    string B7GraphOrderRef,                // Graph-order resolver handoff reference.
    string B11AsyncBatchRef,               // Async candidate-batch handoff reference.
    string B12WinnerSelectionRef,          // Winner-selection handoff reference.
    string B6UnitDpoBuilderRef,            // Unit-level DPO-builder handoff reference.
    bool SemanticQualityClaimed);          // Honest per-candidate semantic-quality claim.

// This enum is the closed canonical set of dynamic-graph relation kinds.
public enum GraphEdgeKind
{
    // FanOut sends one source result to one or more target nodes.
    FanOut,

    // FanIn identifies a relation participating in an N-to-one merge.
    FanIn,

    // ManyToMany identifies a relation participating in an N-to-M traversal.
    ManyToMany,

    // MultiOutput sends multiple typed outputs across the relation boundary.
    MultiOutput,

    // ReturnLoop identifies a correction signal whose bounded execution belongs to the L3/L4 runtime layer.
    ReturnLoop
}

// This interface is the canonical typed source-to-target relation stored in a topology snapshot.
public interface IDynamicGraphEdge
{
    // EdgeId is the stable relation identity within the topology.
    string EdgeId { get; }

    // FromNodeId identifies the single source endpoint.
    string FromNodeId { get; }

    // ToNodeIds identifies the non-empty target endpoint set used by fan-out and multi-output relations.
    IReadOnlyList<string> ToNodeIds { get; }

    // Kind classifies the relation without adding an async or sequential pseudo-kind.
    GraphEdgeKind Kind { get; }

    // ParallelEligible is an execution-mode hint rather than a sixth edge kind.
    bool ParallelEligible { get; }

    // AuthorityUnitId is the registry identity of the learned L2 authority and never a direct unit object.
    string AuthorityUnitId { get; }

    // AuthorityUnitLevel names the existing owner level, expected to be the L2 logical-unit level.
    string AuthorityUnitLevel { get; }

    // DeterministicFinalAuthority must remain false on accepted topology edges.
    bool DeterministicFinalAuthority { get; }

    // BaselineUnitId optionally identifies the deterministic candidate generator used for comparison.
    string? BaselineUnitId { get; }

    // BaselineRole states the non-authoritative role of the baseline component.
    string BaselineRole { get; }

    // ExcludedLeakageSignals names signals that are forbidden from learned edge authority.
    IReadOnlyList<string> ExcludedLeakageSignals { get; }

    // SourceProof identifies the provenance evidence from which the relation was constructed.
    string SourceProof { get; }

    // CutoverProof identifies the evidence for any baseline-to-learned authority cutover.
    string CutoverProof { get; }

    // AblationProofRef identifies the comparison evidence that the learned authority contributes.
    string AblationProofRef { get; }
}

// This immutable selection identifies one learned-authority target in canonical handoff order.
public sealed record DynamicGraphEdgeTargetSelection(
    string TargetNodeId,
    double Confidence,
    int Ordinal);

// This inference-only carrier hands learned edge targets to L4 without executing them or mutating topology.
// Every collection is privately owned and every getter returns a fresh read-only snapshot.
public sealed record DynamicGraphEdgeTargetHandoff
{
    private readonly string _runId;
    private readonly string _edgeId;
    private readonly string _fromNodeId;
    private readonly string _domainTag;
    private readonly GraphEdgeKind _kind;
    private readonly bool _parallelEligible;
    private readonly DynamicGraphEdgeTargetSelection[] _orderedTargets;
    private readonly bool _knowledgeEdge;
    private readonly string _authoritySource;
    private readonly string[] _upstreamCandidateRefs;
    private readonly string _upstreamOutputHash;
    private readonly string _projectionRule;

    public DynamicGraphEdgeTargetHandoff(
        string RunId,
        string EdgeId,
        string FromNodeId,
        string DomainTag,
        GraphEdgeKind Kind,
        bool ParallelEligible,
        IReadOnlyList<DynamicGraphEdgeTargetSelection> OrderedTargets,
        bool KnowledgeEdge,
        string AuthoritySource,
        IReadOnlyList<string> UpstreamCandidateRefs,
        string UpstreamOutputHash,
        string ProjectionRule)
    {
        ArgumentNullException.ThrowIfNull(OrderedTargets);
        ArgumentNullException.ThrowIfNull(UpstreamCandidateRefs);

        _runId = RunId;
        _edgeId = EdgeId;
        _fromNodeId = FromNodeId;
        _domainTag = DomainTag;
        _kind = Kind;
        _parallelEligible = ParallelEligible;
        _orderedTargets = OrderedTargets.Select(CopySelection).ToArray();
        _knowledgeEdge = KnowledgeEdge;
        _authoritySource = AuthoritySource;
        _upstreamCandidateRefs = UpstreamCandidateRefs.ToArray();
        _upstreamOutputHash = UpstreamOutputHash;
        _projectionRule = ProjectionRule;
    }

    public string RunId => _runId;

    public string EdgeId => _edgeId;

    public string FromNodeId => _fromNodeId;

    public string DomainTag => _domainTag;

    public GraphEdgeKind Kind => _kind;

    public bool ParallelEligible => _parallelEligible;

    public IReadOnlyList<DynamicGraphEdgeTargetSelection> OrderedTargets
        => Array.AsReadOnly(_orderedTargets.Select(CopySelection).ToArray());

    public bool KnowledgeEdge => _knowledgeEdge;

    public string AuthoritySource => _authoritySource;

    public IReadOnlyList<string> UpstreamCandidateRefs
        => Array.AsReadOnly(_upstreamCandidateRefs.ToArray());

    public string UpstreamOutputHash => _upstreamOutputHash;

    public string ProjectionRule => _projectionRule;

    public bool TargetsExecuted => false;

    public bool TopologyMutated => false;

    private static DynamicGraphEdgeTargetSelection CopySelection(
        DynamicGraphEdgeTargetSelection source)
        => source with { };
}

// This enum makes the v1 fan-in merger's deterministic, non-authoritative role machine-checkable.
public enum FanInMergeRole
{
    // DeterministicFanInBaseline performs structural worst-of aggregation and never selects a winner.
    DeterministicFanInBaseline
}

// This record carries one typed async branch and the owner-required merge, path, and evidence metadata.
public sealed record GraphAsyncBranchResult(
    string CombinationId,                 // Async combination that produced the branch.
    string BranchId,                      // Stable branch identity within the fan-in set.
    string OverallVerdict,                // Branch verdict used by the worst-of rule.
    double Confidence,                    // Diagnostic branch confidence; never winner authority.
    string[] SourceHashes,                // Source provenance hashes without source-path authority.
    LogicalFlowResult Inner,              // Existing normalized logical-flow result.
    string MergeKey,                      // Canonical merge-group key.
    string PathOrderRef,                  // Canonical path-order reference used with MergeKey for ordering.
    string[] ProvenanceRefs,              // Per-branch provenance evidence references.
    string[] TraceRefs,                   // Per-branch traversal trace references.
    string[] ErrorCodes);                 // Per-branch failure evidence preserved by fan-in.

// Plan-06 names this typed carrier but omits its field schema. This minimal map preserves every
// branch-to-value fact in stable branch-id order without choosing a winner or claiming resolution.
// The merger derives Metrics.<key> fields and encodes presence as P:<length>:<value> or M:0:.
public sealed record GraphFanInConflict(
    string FieldName,
    IReadOnlyDictionary<string, string> ValuesByBranchId);

// This record is the current Plan-06 input carrier for deterministic N-to-one fan-in aggregation.
public sealed record GraphFanInMergeInput(
    string RunId,
    GraphAsyncBranchResult[] Branches,
    string MergeKey,
    string PathOrderRef,
    string LegalScope,
    string[] SourceRefs,
    string[] ProvenanceRefs,
    string[] TraceRefs,
    GraphAsyncBranchResult[] RejectedAlternatives,
    GraphFanInConflict[] Conflicts,
    string[] FailureReasonCodes,
    bool UseContextCarryForward);

// This record is the typed B-16 fan-in result and the machine-checkable order-authority witness.
// Its getters return defensive snapshots because the owner-required LogicalFlowResult and branch
// carriers still contain arrays; callers can mutate a retrieved copy without mutating this result.
public sealed record GraphFanInMergeResult
{
    private readonly LogicalFlowResult _merged;
    private readonly LogicalContextPacket _context;
    private readonly FanInMergeRole _mergeRole;
    private readonly bool _mergeOrderIsAuthority;
    private readonly int _mergedBranchCount;
    private readonly GraphAsyncBranchResult[] _rejectedAlternatives;
    private readonly GraphFanInConflict[] _conflicts;
    private readonly string[] _provenanceRefs;
    private readonly string[] _traceRefs;
    private readonly string[] _failureReasonCodes;

    // The constructor snapshots every nested carrier before storing the result state.
    public GraphFanInMergeResult(
        LogicalFlowResult Merged,
        LogicalContextPacket Context,
        FanInMergeRole MergeRole,
        bool MergeOrderIsAuthority,
        int MergedBranchCount,
        IReadOnlyList<GraphAsyncBranchResult> RejectedAlternatives,
        IReadOnlyList<GraphFanInConflict> Conflicts,
        IReadOnlyList<string> ProvenanceRefs,
        IReadOnlyList<string> TraceRefs,
        IReadOnlyList<string> FailureReasonCodes)
    {
        _merged = CopyFlowResult(Merged);
        _context = CopyContext(Context);
        _mergeRole = MergeRole;
        _mergeOrderIsAuthority = MergeOrderIsAuthority;
        _mergedBranchCount = MergedBranchCount;
        _rejectedAlternatives = RejectedAlternatives.Select(CopyBranch).ToArray();
        _conflicts = Conflicts.Select(CopyConflict).ToArray();
        _provenanceRefs = ProvenanceRefs.ToArray();
        _traceRefs = TraceRefs.ToArray();
        _failureReasonCodes = FailureReasonCodes.ToArray();
    }

    public LogicalFlowResult Merged => CopyFlowResult(_merged);

    public LogicalContextPacket Context => CopyContext(_context);

    public FanInMergeRole MergeRole => _mergeRole;

    public bool MergeOrderIsAuthority => _mergeOrderIsAuthority;

    public int MergedBranchCount => _mergedBranchCount;

    public IReadOnlyList<GraphAsyncBranchResult> RejectedAlternatives
        => Array.AsReadOnly(_rejectedAlternatives.Select(CopyBranch).ToArray());

    public IReadOnlyList<GraphFanInConflict> Conflicts
        => Array.AsReadOnly(_conflicts.Select(CopyConflict).ToArray());

    public IReadOnlyList<string> ProvenanceRefs
        => Array.AsReadOnly(_provenanceRefs.ToArray());

    public IReadOnlyList<string> TraceRefs
        => Array.AsReadOnly(_traceRefs.ToArray());

    public IReadOnlyList<string> FailureReasonCodes
        => Array.AsReadOnly(_failureReasonCodes.ToArray());

    private static GraphAsyncBranchResult CopyBranch(GraphAsyncBranchResult source)
        => new(
            CombinationId: source.CombinationId,
            BranchId: source.BranchId,
            OverallVerdict: source.OverallVerdict,
            Confidence: source.Confidence,
            SourceHashes: source.SourceHashes.ToArray(),
            Inner: CopyFlowResult(source.Inner),
            MergeKey: source.MergeKey,
            PathOrderRef: source.PathOrderRef,
            ProvenanceRefs: source.ProvenanceRefs.ToArray(),
            TraceRefs: source.TraceRefs.ToArray(),
            ErrorCodes: source.ErrorCodes.ToArray());

    private static GraphFanInConflict CopyConflict(GraphFanInConflict source)
        => new(source.FieldName, CopyDictionary(source.ValuesByBranchId));

    private static LogicalFlowResult CopyFlowResult(LogicalFlowResult source)
        => new(
            FlowId: source.FlowId,
            ScenarioId: source.ScenarioId,
            OverallVerdict: source.OverallVerdict,
            FinalHumanResult: source.FinalHumanResult,
            Summary: source.Summary with { },
            Metrics: CopyDictionary(source.Metrics),
            EvidenceIds: source.EvidenceIds.ToArray(),
            Warnings: source.Warnings.ToArray())
        {
            TransitionSignals = source.TransitionSignals.Select(CopyTransitionSignal).ToArray(),
            EdgeTargetHandoff = CopyTargetHandoff(source.EdgeTargetHandoff)
        };

    private static DynamicGraphEdgeTargetHandoff? CopyTargetHandoff(
        DynamicGraphEdgeTargetHandoff? source)
        => source is null
            ? null
            : new DynamicGraphEdgeTargetHandoff(
                source.RunId,
                source.EdgeId,
                source.FromNodeId,
                source.DomainTag,
                source.Kind,
                source.ParallelEligible,
                source.OrderedTargets,
                source.KnowledgeEdge,
                source.AuthoritySource,
                source.UpstreamCandidateRefs,
                source.UpstreamOutputHash,
                source.ProjectionRule);

    private static LogicalContextPacket CopyContext(LogicalContextPacket source)
        => new(
            RunId: source.RunId,
            SourceFlowIds: source.SourceFlowIds.ToArray(),
            SourceScenarioIds: source.SourceScenarioIds.ToArray(),
            Fragments: source.Fragments.Select(CopyContextFragment).ToArray(),
            CreatedAtUtc: source.CreatedAtUtc);

    private static LogicalContextFragment CopyContextFragment(LogicalContextFragment source)
        => new(
            FlowId: source.FlowId,
            ScenarioId: source.ScenarioId,
            OverallVerdict: source.OverallVerdict,
            FinalHumanResult: source.FinalHumanResult,
            PhaseId: source.PhaseId,
            SubPhaseId: source.SubPhaseId,
            Metrics: CopyDictionary(source.Metrics),
            EvidenceIds: source.EvidenceIds.ToArray(),
            Warnings: source.Warnings.ToArray());

    private static PhaseTransitionSignal CopyTransitionSignal(PhaseTransitionSignal source)
        => source with { EvidenceIds = source.EvidenceIds.ToArray() };

    private static IReadOnlyDictionary<string, string> CopyDictionary(
        IReadOnlyDictionary<string, string> source)
    {
        var snapshot = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in source.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            snapshot.Add(pair.Key, pair.Value);

        return new ReadOnlyDictionary<string, string>(snapshot);
    }
}

// This interface is the exact Plan-06 deterministic fan-in boundary consumed by the future L3 edge flow.
public interface IEdgeFanInMerger
{
    // Merge returns evidence-preserving structural aggregation and never performs winner selection.
    OperationResult<GraphFanInMergeResult> Merge(GraphFanInMergeInput input);
}

// This record binds one stable node identity to its tenant/domain/context scope and deterministic position.
public sealed record DynamicGraphNodeOrder(
    string OrderId,                         // Stable order-entry identity.
    string NodeId,                          // Node identity assigned to this entry.
    string TenantId,                        // Tenant boundary of the ordered node.
    string DomainTag,                       // Domain boundary of the ordered node.
    string ContextId,                       // Build/execution context of the ordered node.
    int Position);                          // Unique zero-based deterministic position.

// This class is an immutable snapshot of the graph identity, scope, order, nodes, and relations.
public sealed class DynamicGraphTopology
{
    // The constructor snapshots every supplied carrier so caller mutations cannot change the topology later.
    public DynamicGraphTopology(
        string tenantId,                                    // Required topology tenant identity.
        string domainTag,                                   // Required topology domain identity.
        string contextId,                                   // Required topology context identity.
        IReadOnlyList<IDynamicGraphNode>? nodes,             // Materialized heterogeneous node source.
        IReadOnlyList<IDynamicGraphEdge>? edges,             // Materialized canonical relation source.
        IReadOnlyList<DynamicGraphNodeOrder>? order)         // Materialized scoped order source.
    {
        // TenantId records the owning tenant boundary for every node in this snapshot.
        TenantId = tenantId;

        // DomainTag records the declared graph domain for every non-wildcard node in this snapshot.
        DomainTag = domainTag;

        // ContextId records the stable execution/build context shared by the ordered nodes.
        ContextId = contextId;

        // Nodes owns copied identities and descriptors rather than caller node objects.
        Nodes = Array.AsReadOnly(nodes?.Select(SnapshotNode).ToArray() ?? []);

        // Edges owns copied scalars and nested lists rather than caller edge objects or list views.
        Edges = Array.AsReadOnly(edges?.Select(SnapshotEdge).ToArray() ?? []);

        // Order owns copied immutable records containing stable order identities and positions.
        Order = Array.AsReadOnly(order?.Select(SnapshotOrder).ToArray() ?? []);
    }

    // TenantId is the required tenant carrier for topology isolation.
    public string TenantId { get; }

    // DomainTag is the required domain carrier for topology isolation.
    public string DomainTag { get; }

    // ContextId is the required context carrier for the topology snapshot.
    public string ContextId { get; }

    // Nodes exposes the heterogeneous node boundary through a read-only collection.
    public IReadOnlyList<IDynamicGraphNode> Nodes { get; }

    // Edges exposes canonical relations through a read-only collection.
    public IReadOnlyList<IDynamicGraphEdge> Edges { get; }

    // ListEdgesFrom returns a fresh, source-scoped view in immutable snapshot enumeration order.
    public IDynamicGraphEdge[] ListEdgesFrom(string nodeId)
        => Edges
            .Where(edge => edge is not null && StringComparer.Ordinal.Equals(edge.FromNodeId, nodeId))
            .ToArray();

    // Order exposes stable, scoped node positions through a read-only collection.
    public IReadOnlyList<DynamicGraphNodeOrder> Order { get; }

    // SnapshotNode preserves null carriers for validator diagnostics and otherwise severs caller object identity.
    private static IDynamicGraphNode SnapshotNode(IDynamicGraphNode? node)
        => node is null
            ? null!
            : new NodeSnapshot(
                node.NodeId,
                node.Descriptor is null ? null! : node.Descriptor with { });

    // SnapshotEdge preserves null carriers for validator diagnostics and deep-copies both nested list carriers.
    private static IDynamicGraphEdge SnapshotEdge(IDynamicGraphEdge? edge)
        => edge is null
            ? null!
            : new EdgeSnapshot(
                edge.EdgeId,
                edge.FromNodeId,
                edge.ToNodeIds is null ? null! : Array.AsReadOnly(edge.ToNodeIds.ToArray()),
                edge.Kind,
                edge.ParallelEligible,
                edge.AuthorityUnitId,
                edge.AuthorityUnitLevel,
                edge.DeterministicFinalAuthority,
                edge.BaselineUnitId,
                edge.BaselineRole,
                edge.ExcludedLeakageSignals is null
                    ? null!
                    : Array.AsReadOnly(edge.ExcludedLeakageSignals.ToArray()),
                edge.SourceProof,
                edge.CutoverProof,
                edge.AblationProofRef);

    // SnapshotOrder preserves null carriers for validator diagnostics and copies every positional value.
    private static DynamicGraphNodeOrder SnapshotOrder(DynamicGraphNodeOrder? entry)
        => entry is null ? null! : entry with { };

    // NodeSnapshot is a private immutable carrier so topology never retains a caller node implementation.
    private sealed record NodeSnapshot(
        string NodeId,
        DynamicGraphNodeDescriptor Descriptor) : IDynamicGraphNode;

    // EdgeSnapshot is a private immutable carrier so topology never retains a caller edge implementation.
    private sealed record EdgeSnapshot(
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

// This owner-defined request carries one input and the requested async combination cardinality.
public sealed record GraphCandidateRunRequest(
    string ScenarioId,
    string Query,
    string[] InputGraphRefs,
    string[] InputUnitRefs,
    AsyncCombinationBudget Budget,
    int CombinationCount,
    CancellationToken Cancellation,
    string LegalScope,
    string[] ProvenanceRefs,
    string TraceRef);

// This owner-defined budget separates plan count, branch concurrency, and cross-plan concurrency.
public sealed record AsyncCombinationBudget(
    int MaxCombinations,
    TimeSpan? Timeout,
    int MaxParallelBranches,
    int MaxParallelCombinationRuns = 1);

// This typed materialized output represents one combination run, including its monotonic wall-clock duration,
// without winner-selection authority.
public sealed record GraphCandidateOutput(
    string CandidateId,
    IReadOnlyList<GraphNodeCandidate> OutputRefs,
    string UnitId,
    string[] UnitIds,
    double Confidence,
    double? Score,
    string CombinationId,
    string[] ProvenanceRefs,
    string PathTraceRef,
    string TraceRef,
    string[] ErrorCodes,
    long ElapsedMs);

// This batch carries the requested-versus-actual combination result, whole-orchestration wall-clock duration,
// and an optional typed stream.
public sealed record AsyncCandidateBatch(
    string BatchId,
    GraphCandidateRunRequest Request,
    IReadOnlyList<GraphCandidateOutput> Candidates,
    IAsyncEnumerable<GraphCandidateOutput>? CandidateStream,
    int RequestedCombinationCount,
    int ActualCombinationCount,
    string Status,
    string[] ErrorCodes,
    long ElapsedMs);

// This plan is the owner-defined declarative identity of one combination traversal.
public sealed record CombinationPlan(
    int Index,
    string CombinationId,
    string[] NodeSetIds,
    string EdgeOrderKey,
    LogicalExecutionFlags Flags);

// This strategy deterministically enumerates bounded, declarative combination plans without executing nodes.
public interface IGraphCombinationStrategy
{
    OperationResult<IReadOnlyList<CombinationPlan>> Enumerate(
        GraphCandidateRunRequest request);
}

// This interface is the async-only boundary; concrete execution and scheduling remain outside this slice.
public interface IGraphCandidateRunner
{
    Task<OperationResult<AsyncCandidateBatch>> RunCandidatesAsync(
        GraphCandidateRunRequest request,
        CancellationToken cancellationToken = default);
}
