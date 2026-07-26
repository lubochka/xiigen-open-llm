// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Collections.ObjectModel;
using AgentInfrastructure;
using Domain = SemanticCompiler29.Domain;
using SemanticCompilerAdapter;

// One deterministic candidate supplied to the learned L2 edge-authority unit.
public sealed record EdgeCandidate(
    string TargetSubPhaseId,
    string[] EvidencePredicates,
    double BaselineScore,
    string AuthorityRole);

// This provider proposes deterministic raw-score candidates without executing learned authority.
public interface IEdgeCandidateProvider
{
    OperationResult<EdgeCandidate[]> Propose(
        IDynamicGraphEdge edge,
        EdgeTraversalInput input);
}

// The inference-only input used to rank candidate targets for one dynamic-graph edge.
public sealed record EdgeAuthorityInput(
    string EdgeId,
    string[] SemanticTokens,
    string[] PredicateIds,
    ContextSignals Context,
    IReadOnlyList<EdgeCandidate> Candidates,
    string[] ExcludedLeakageSignals);

// This materializer creates a defensive, leakage-bounded snapshot for one edge-authority decision.
public interface IDynamicGraphEdgeAuthorityInputMaterializer
{
    OperationResult<EdgeAuthorityInput> Materialize(
        IDynamicGraphEdge edge,
        EdgeTraversalInput input,
        IReadOnlyList<EdgeCandidate> candidates);
}

// The advisory L2 decision returned without mutating graph topology or claiming final authority.
public sealed record EdgeAuthorityDecision(
    IReadOnlyList<(string TargetSubPhaseId, double Confidence)> OrderedTargets,
    int FanOutCount,
    bool KnowledgeEdge,
    string AuthoritySource,
    IReadOnlyList<string> AblationBaselineRank);

// This diagnostics-only input owns the normalized edge input and the separately executed canonical decision.
public sealed record DynamicGraphEdgeCandidateOrderProbeInput
{
    private readonly EdgeAuthorityInput _normalizedInput;
    private readonly EdgeAuthorityDecision _referenceDecision;

    public DynamicGraphEdgeCandidateOrderProbeInput(
        EdgeAuthorityInput NormalizedInput,
        EdgeAuthorityDecision ReferenceDecision)
    {
        ArgumentNullException.ThrowIfNull(NormalizedInput);
        ArgumentNullException.ThrowIfNull(ReferenceDecision);

        _normalizedInput = DynamicGraphEdgeAuthorityContractSnapshots.CopyInput(NormalizedInput);
        _referenceDecision = DynamicGraphEdgeAuthorityContractSnapshots.CopyDecision(ReferenceDecision);
    }

    public EdgeAuthorityInput NormalizedInput
        => DynamicGraphEdgeAuthorityContractSnapshots.CopyInput(_normalizedInput);

    public EdgeAuthorityDecision ReferenceDecision
        => DynamicGraphEdgeAuthorityContractSnapshots.CopyDecision(_referenceDecision);
}

// One diagnostics-only invocation records its stable probe identity, candidate order, and full learned decision.
public sealed record DynamicGraphEdgeCandidateOrderPermutationProbe
{
    private readonly EdgeCandidate[] _candidateOrder;
    private readonly EdgeAuthorityDecision _decision;

    public DynamicGraphEdgeCandidateOrderPermutationProbe(
        string ProbeId,
        IReadOnlyList<EdgeCandidate> CandidateOrder,
        EdgeAuthorityDecision Decision)
    {
        ArgumentNullException.ThrowIfNull(CandidateOrder);
        ArgumentNullException.ThrowIfNull(Decision);

        this.ProbeId = ProbeId;
        _candidateOrder = CandidateOrder
            .Select(DynamicGraphEdgeAuthorityContractSnapshots.CopyCandidate)
            .ToArray();
        _decision = DynamicGraphEdgeAuthorityContractSnapshots.CopyDecision(Decision);
    }

    public string ProbeId { get; }

    public IReadOnlyList<EdgeCandidate> CandidateOrder
        => Array.AsReadOnly(
            _candidateOrder
                .Select(DynamicGraphEdgeAuthorityContractSnapshots.CopyCandidate)
                .ToArray());

    public EdgeAuthorityDecision Decision
        => DynamicGraphEdgeAuthorityContractSnapshots.CopyDecision(_decision);
}

// The report is evidence only: it cannot select targets, replace the canonical decision, or mutate topology.
public sealed record DynamicGraphEdgeCandidateOrderLeakageReport
{
    private readonly DynamicGraphEdgeCandidateOrderPermutationProbe[] _probes;
    private readonly string[] _excludedLeakageSignals;

    public DynamicGraphEdgeCandidateOrderLeakageReport(
        string EdgeId,
        string ResolvedAuthorityUnitId,
        bool IsOrderInvariant,
        IReadOnlyList<DynamicGraphEdgeCandidateOrderPermutationProbe> Probes,
        IReadOnlyList<string> ExcludedLeakageSignals)
    {
        ArgumentNullException.ThrowIfNull(Probes);
        ArgumentNullException.ThrowIfNull(ExcludedLeakageSignals);

        this.EdgeId = EdgeId;
        this.ResolvedAuthorityUnitId = ResolvedAuthorityUnitId;
        this.IsOrderInvariant = IsOrderInvariant;
        _probes = Probes
            .Select(DynamicGraphEdgeAuthorityContractSnapshots.CopyProbe)
            .ToArray();
        _excludedLeakageSignals = ExcludedLeakageSignals.ToArray();
    }

    public string EdgeId { get; }

    public string ResolvedAuthorityUnitId { get; }

    public bool IsOrderInvariant { get; }

    public IReadOnlyList<DynamicGraphEdgeCandidateOrderPermutationProbe> Probes
        => Array.AsReadOnly(
            _probes
                .Select(DynamicGraphEdgeAuthorityContractSnapshots.CopyProbe)
                .ToArray());

    public IReadOnlyList<string> ExcludedLeakageSignals
        => Array.AsReadOnly(_excludedLeakageSignals.ToArray());
}

// This diagnostics-only input owns the traversal, normalized input, and canonical decision used by identity controls.
public sealed record DynamicGraphEdgeIdentityProbeInput
{
    private readonly EdgeTraversalInput _traversalInput;
    private readonly EdgeAuthorityInput _normalizedInput;
    private readonly EdgeAuthorityDecision _referenceDecision;

    public DynamicGraphEdgeIdentityProbeInput(
        EdgeTraversalInput TraversalInput,
        EdgeAuthorityInput NormalizedInput,
        EdgeAuthorityDecision ReferenceDecision)
    {
        ArgumentNullException.ThrowIfNull(TraversalInput);
        ArgumentNullException.ThrowIfNull(NormalizedInput);
        ArgumentNullException.ThrowIfNull(ReferenceDecision);

        _traversalInput = DynamicGraphEdgeAuthorityContractSnapshots.CopyTraversalInput(TraversalInput);
        _normalizedInput = DynamicGraphEdgeAuthorityContractSnapshots.CopyInput(NormalizedInput);
        _referenceDecision = DynamicGraphEdgeAuthorityContractSnapshots.CopyDecision(ReferenceDecision);
    }

    public EdgeTraversalInput TraversalInput
        => DynamicGraphEdgeAuthorityContractSnapshots.CopyTraversalInput(_traversalInput);

    public EdgeAuthorityInput NormalizedInput
        => DynamicGraphEdgeAuthorityContractSnapshots.CopyInput(_normalizedInput);

    public EdgeAuthorityDecision ReferenceDecision
        => DynamicGraphEdgeAuthorityContractSnapshots.CopyDecision(_referenceDecision);
}

// One diagnostics-only control records the changed identity signal and the complete learned decision.
public sealed record DynamicGraphEdgeIdentityControlProbe
{
    private readonly EdgeAuthorityDecision _decision;

    public DynamicGraphEdgeIdentityControlProbe(
        string ProbeId,
        string ControlledSignal,
        string ControlValue,
        EdgeAuthorityDecision Decision)
    {
        ArgumentNullException.ThrowIfNull(Decision);

        this.ProbeId = ProbeId;
        this.ControlledSignal = ControlledSignal;
        this.ControlValue = ControlValue;
        _decision = DynamicGraphEdgeAuthorityContractSnapshots.CopyDecision(Decision);
    }

    public string ProbeId { get; }

    public string ControlledSignal { get; }

    public string ControlValue { get; }

    public EdgeAuthorityDecision Decision
        => DynamicGraphEdgeAuthorityContractSnapshots.CopyDecision(_decision);
}

// The report is diagnostics only and cannot replace the canonical learned edge-authority decision.
public sealed record DynamicGraphEdgeIdentityLeakageReport
{
    private readonly DynamicGraphEdgeIdentityControlProbe[] _probes;
    private readonly string[] _excludedLeakageSignals;

    public DynamicGraphEdgeIdentityLeakageReport(
        string EdgeId,
        string ResolvedAuthorityUnitId,
        bool IsIdentityInvariant,
        IReadOnlyList<DynamicGraphEdgeIdentityControlProbe> Probes,
        IReadOnlyList<string> ExcludedLeakageSignals)
    {
        ArgumentNullException.ThrowIfNull(Probes);
        ArgumentNullException.ThrowIfNull(ExcludedLeakageSignals);

        this.EdgeId = EdgeId;
        this.ResolvedAuthorityUnitId = ResolvedAuthorityUnitId;
        this.IsIdentityInvariant = IsIdentityInvariant;
        _probes = Probes
            .Select(DynamicGraphEdgeAuthorityContractSnapshots.CopyIdentityProbe)
            .ToArray();
        _excludedLeakageSignals = ExcludedLeakageSignals.ToArray();
    }

    public string EdgeId { get; }

    public string ResolvedAuthorityUnitId { get; }

    public bool IsIdentityInvariant { get; }

    public IReadOnlyList<DynamicGraphEdgeIdentityControlProbe> Probes
        => Array.AsReadOnly(
            _probes
                .Select(DynamicGraphEdgeAuthorityContractSnapshots.CopyIdentityProbe)
                .ToArray());

    public IReadOnlyList<string> ExcludedLeakageSignals
        => Array.AsReadOnly(_excludedLeakageSignals.ToArray());
}

// This ephemeral carrier binds a descriptor and typed executable from the same registry tuple.
public sealed record ResolvedDynamicGraphEdgeAuthority(
    LogicalUnitDescriptor Descriptor,
    ILogicalUnit<EdgeAuthorityInput, EdgeAuthorityDecision> Unit);

// This resolver admits one registry-backed L2 authority without executing or persisting it.
public interface IDynamicGraphEdgeAuthorityResolver
{
    OperationResult<ResolvedDynamicGraphEdgeAuthority> Resolve(
        IDynamicGraphEdge edge,
        string domainTag);
}

/// <summary>
/// Captures immutable structural edge-authority provenance only. It excludes combined quality,
/// accuracy, novelty, ablation measurement, trained state, lifecycle state, DPO state,
/// promotion state, and graph aggregates.
/// </summary>
public sealed record DynamicGraphEdgeAuthorityEvidence
{
    private readonly string[] _excludedLeakageSignals;

    public DynamicGraphEdgeAuthorityEvidence(
        string EdgeId,
        string AuthorityUnitId,
        string AuthorityUnitLevel,
        bool DeterministicFinalAuthority,
        string BaselineUnitId,
        string BaselineRole,
        IReadOnlyList<string> ExcludedLeakageSignals,
        string SourceProof,
        string CutoverProof,
        string AblationProofRef)
    {
        ArgumentNullException.ThrowIfNull(EdgeId);
        ArgumentNullException.ThrowIfNull(AuthorityUnitId);
        ArgumentNullException.ThrowIfNull(AuthorityUnitLevel);
        ArgumentNullException.ThrowIfNull(BaselineUnitId);
        ArgumentNullException.ThrowIfNull(BaselineRole);
        ArgumentNullException.ThrowIfNull(ExcludedLeakageSignals);
        ArgumentNullException.ThrowIfNull(SourceProof);
        ArgumentNullException.ThrowIfNull(CutoverProof);
        ArgumentNullException.ThrowIfNull(AblationProofRef);

        this.EdgeId = EdgeId;
        this.AuthorityUnitId = AuthorityUnitId;
        this.AuthorityUnitLevel = AuthorityUnitLevel;
        this.DeterministicFinalAuthority = DeterministicFinalAuthority;
        this.BaselineUnitId = BaselineUnitId;
        this.BaselineRole = BaselineRole;
        _excludedLeakageSignals = ExcludedLeakageSignals.ToArray();
        this.SourceProof = SourceProof;
        this.CutoverProof = CutoverProof;
        this.AblationProofRef = AblationProofRef;
    }

    public string EdgeId { get; }

    public string AuthorityUnitId { get; }

    public string AuthorityUnitLevel { get; }

    public bool DeterministicFinalAuthority { get; }

    public string BaselineUnitId { get; }

    public string BaselineRole { get; }

    public IReadOnlyList<string> ExcludedLeakageSignals
        => Array.AsReadOnly(_excludedLeakageSignals.ToArray());

    public string SourceProof { get; }

    public string CutoverProof { get; }

    public string AblationProofRef { get; }
}

// This result owns deep snapshots of the normalized learned input, decision, structural evidence, and diagnostics.
public sealed record EdgeAuthorityExecutionResult
{
    private readonly EdgeAuthorityInput _normalizedInput;
    private readonly EdgeCandidate[] _proposedCandidates;
    private readonly EdgeAuthorityDecision _decision;
    private readonly DynamicGraphEdgeAuthorityEvidence _authorityEvidence;
    private readonly DynamicGraphEdgeCandidateOrderLeakageReport _candidateOrderLeakageReport;
    private readonly DynamicGraphEdgeIdentityLeakageReport _identityLeakageReport;
    private readonly double _unitConfidence;
    private readonly string[] _evidenceIds;
    private readonly string[] _warnings;

    public EdgeAuthorityExecutionResult(
        EdgeAuthorityInput NormalizedInput,
        IReadOnlyList<EdgeCandidate> ProposedCandidates,
        EdgeAuthorityDecision Decision,
        DynamicGraphEdgeAuthorityEvidence AuthorityEvidence,
        DynamicGraphEdgeCandidateOrderLeakageReport CandidateOrderLeakageReport,
        DynamicGraphEdgeIdentityLeakageReport IdentityLeakageReport,
        double UnitConfidence,
        IReadOnlyList<string> EvidenceIds,
        IReadOnlyList<string> Warnings)
    {
        ArgumentNullException.ThrowIfNull(NormalizedInput);
        ArgumentNullException.ThrowIfNull(ProposedCandidates);
        ArgumentNullException.ThrowIfNull(Decision);
        ArgumentNullException.ThrowIfNull(AuthorityEvidence);
        ArgumentNullException.ThrowIfNull(CandidateOrderLeakageReport);
        ArgumentNullException.ThrowIfNull(IdentityLeakageReport);
        ArgumentNullException.ThrowIfNull(EvidenceIds);
        ArgumentNullException.ThrowIfNull(Warnings);

        _normalizedInput = CopyInput(NormalizedInput);
        _proposedCandidates = ProposedCandidates.Select(CopyCandidate).ToArray();
        _decision = CopyDecision(Decision);
        _authorityEvidence = DynamicGraphEdgeAuthorityContractSnapshots.CopyAuthorityEvidence(
            AuthorityEvidence);
        _candidateOrderLeakageReport = DynamicGraphEdgeAuthorityContractSnapshots.CopyReport(
            CandidateOrderLeakageReport);
        _identityLeakageReport = DynamicGraphEdgeAuthorityContractSnapshots.CopyIdentityReport(
            IdentityLeakageReport);
        _unitConfidence = UnitConfidence;
        _evidenceIds = EvidenceIds.ToArray();
        _warnings = Warnings.ToArray();
    }

    public EdgeAuthorityInput NormalizedInput => CopyInput(_normalizedInput);

    public IReadOnlyList<EdgeCandidate> ProposedCandidates
        => Array.AsReadOnly(_proposedCandidates.Select(CopyCandidate).ToArray());

    public EdgeAuthorityDecision Decision => CopyDecision(_decision);

    public DynamicGraphEdgeAuthorityEvidence AuthorityEvidence
        => DynamicGraphEdgeAuthorityContractSnapshots.CopyAuthorityEvidence(_authorityEvidence);

    public DynamicGraphEdgeCandidateOrderLeakageReport CandidateOrderLeakageReport
        => DynamicGraphEdgeAuthorityContractSnapshots.CopyReport(_candidateOrderLeakageReport);

    public DynamicGraphEdgeIdentityLeakageReport IdentityLeakageReport
        => DynamicGraphEdgeAuthorityContractSnapshots.CopyIdentityReport(_identityLeakageReport);

    public double UnitConfidence => _unitConfidence;

    public IReadOnlyList<string> EvidenceIds => Array.AsReadOnly(_evidenceIds.ToArray());

    public IReadOnlyList<string> Warnings => Array.AsReadOnly(_warnings.ToArray());

    private static EdgeAuthorityInput CopyInput(EdgeAuthorityInput source)
        => new(
            source.EdgeId,
            source.SemanticTokens.ToArray(),
            source.PredicateIds.ToArray(),
            CopyContext(source.Context),
            Array.AsReadOnly(source.Candidates.Select(CopyCandidate).ToArray()),
            source.ExcludedLeakageSignals.ToArray());

    private static EdgeCandidate CopyCandidate(EdgeCandidate source)
        => source with { EvidencePredicates = source.EvidencePredicates.ToArray() };

    private static ContextSignals CopyContext(ContextSignals source)
    {
        var confidence = new SortedDictionary<Domain, double>();
        foreach (var pair in source.PriorConfidenceMap.OrderBy(pair => (int)pair.Key))
            confidence.Add(pair.Key, pair.Value);

        return new ContextSignals(
            source.PriorDomains.ToArray(),
            source.CompletedSubPhases.ToArray(),
            source.ActiveKnowledgeEdges.ToArray(),
            new ReadOnlyDictionary<Domain, double>(confidence));
    }

    private static EdgeAuthorityDecision CopyDecision(EdgeAuthorityDecision source)
        => new(
            Array.AsReadOnly(source.OrderedTargets.ToArray()),
            source.FanOutCount,
            source.KnowledgeEdge,
            source.AuthoritySource,
            Array.AsReadOnly(source.AblationBaselineRank.ToArray()));
}

// This L3 seam runs one learned edge-authority decision without executing the selected targets.
public interface IDynamicGraphEdgeAuthorityExecutionPipeline
{
    Task<OperationResult<EdgeAuthorityExecutionResult>> ExecuteAsync(
        IDynamicGraphEdge edge,
        EdgeTraversalInput input,
        string domainTag,
        LogicalExecutionFlags flags,
        CancellationToken cancellationToken = default);
}

internal static class DynamicGraphEdgeAuthorityContractSnapshots
{
    internal static EdgeTraversalInput CopyTraversalInput(EdgeTraversalInput source)
        => new(
            source.RunId,
            source.EdgeId,
            CopySourceDocument(source.Source),
            CopyContext(source.Context),
            new NodeOutputToEdgeInputAdapter(
                source.UpstreamNodeOutput.SourceNodeId,
                Array.AsReadOnly(source.UpstreamNodeOutput.UpstreamCandidateRefs.ToArray()),
                source.UpstreamNodeOutput.UpstreamOutputHash,
                source.UpstreamNodeOutput.ProjectionRule));

    internal static EdgeAuthorityInput CopyInput(EdgeAuthorityInput source)
        => new(
            source.EdgeId,
            source.SemanticTokens.ToArray(),
            source.PredicateIds.ToArray(),
            CopyContext(source.Context),
            Array.AsReadOnly(source.Candidates.Select(CopyCandidate).ToArray()),
            source.ExcludedLeakageSignals.ToArray());

    internal static EdgeCandidate CopyCandidate(EdgeCandidate source)
        => source with { EvidencePredicates = source.EvidencePredicates.ToArray() };

    internal static EdgeAuthorityDecision CopyDecision(EdgeAuthorityDecision source)
        => new(
            Array.AsReadOnly(source.OrderedTargets.ToArray()),
            source.FanOutCount,
            source.KnowledgeEdge,
            source.AuthoritySource,
            Array.AsReadOnly(source.AblationBaselineRank.ToArray()));

    internal static DynamicGraphEdgeAuthorityEvidence CopyAuthorityEvidence(
        DynamicGraphEdgeAuthorityEvidence source)
        => new(
            source.EdgeId,
            source.AuthorityUnitId,
            source.AuthorityUnitLevel,
            source.DeterministicFinalAuthority,
            source.BaselineUnitId,
            source.BaselineRole,
            source.ExcludedLeakageSignals.ToArray(),
            source.SourceProof,
            source.CutoverProof,
            source.AblationProofRef);

    internal static DynamicGraphEdgeCandidateOrderPermutationProbe CopyProbe(
        DynamicGraphEdgeCandidateOrderPermutationProbe source)
        => new(source.ProbeId, source.CandidateOrder, source.Decision);

    internal static DynamicGraphEdgeCandidateOrderLeakageReport CopyReport(
        DynamicGraphEdgeCandidateOrderLeakageReport source)
        => new(
            source.EdgeId,
            source.ResolvedAuthorityUnitId,
            source.IsOrderInvariant,
            source.Probes.Select(CopyProbe).ToArray(),
            source.ExcludedLeakageSignals.ToArray());

    internal static DynamicGraphEdgeIdentityControlProbe CopyIdentityProbe(
        DynamicGraphEdgeIdentityControlProbe source)
        => new(source.ProbeId, source.ControlledSignal, source.ControlValue, source.Decision);

    internal static DynamicGraphEdgeIdentityLeakageReport CopyIdentityReport(
        DynamicGraphEdgeIdentityLeakageReport source)
        => new(
            source.EdgeId,
            source.ResolvedAuthorityUnitId,
            source.IsIdentityInvariant,
            source.Probes.Select(CopyIdentityProbe).ToArray(),
            source.ExcludedLeakageSignals.ToArray());

    private static SubPhaseRouterSourceDocument CopySourceDocument(SubPhaseRouterSourceDocument source)
        => new(
            source.DocumentId,
            source.SourcePath,
            source.SourceTier,
            source.LicenseOrAuthority,
            source.DocumentType,
            source.DocumentTitle,
            source.CompiledText,
            source.CompiledPredicateIds.ToArray(),
            source.SourceStatus,
            source.KnownFailureMode,
            source.DomainLabel,
            source.SegmentDocuments?
                .Select(CopyShallowSourceDocument)
                .ToArray());

    private static SubPhaseRouterSourceDocument CopyShallowSourceDocument(SubPhaseRouterSourceDocument source)
        => new(
            source.DocumentId,
            source.SourcePath,
            source.SourceTier,
            source.LicenseOrAuthority,
            source.DocumentType,
            source.DocumentTitle,
            source.CompiledText,
            source.CompiledPredicateIds.ToArray(),
            source.SourceStatus,
            source.KnownFailureMode,
            source.DomainLabel,
            SegmentDocuments: null);

    private static ContextSignals CopyContext(ContextSignals source)
    {
        var confidence = new SortedDictionary<Domain, double>();
        foreach (var pair in source.PriorConfidenceMap.OrderBy(pair => (int)pair.Key))
            confidence.Add(pair.Key, pair.Value);

        return new ContextSignals(
            source.PriorDomains.ToArray(),
            source.CompletedSubPhases.ToArray(),
            source.ActiveKnowledgeEdges.ToArray(),
            new ReadOnlyDictionary<Domain, double>(confidence));
    }
}
