// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Collections.ObjectModel;
using AgentInfrastructure;
using Domain = SemanticCompiler29.Domain;
using SemanticCompilerAdapter;

// This diagnostics-only unit proves that source and correlation identities cannot influence learned edge authority.
public sealed class DynamicGraphEdgeIdentityLeakageGuardUnit
    : ILogicalUnit<DynamicGraphEdgeIdentityProbeInput, DynamicGraphEdgeIdentityLeakageReport>
{
    public const string Id = "dynamic-graph-edge-identity-leakage-guard";
    public const string Role = "edge-authority-identity-diagnostics";
    public const string LeakageCode = "EDGE_AUTHORITY_IDENTITY_LEAKAGE";
    public const string GuardFailedCode = "EDGE_AUTHORITY_IDENTITY_GUARD_FAILED";
    public const string CancelledCode = "CANCELLED";

    private const int ProbeCount = 3;
    private const string ExpectedAuthorityRole = "edge-authority";
    private const string SourceIdControlValue = "edge-authority-source-id-control";
    private const string SourcePathControlValue = "identity-control://edge-authority-source-path";
    private const string CorrelationIdControlValue = "edge-authority-correlation-id-control";

    private static readonly string[] ProbeIds =
        ["source-id-control-01", "source-path-control-02", "correlation-id-control-03"];

    private static readonly string[] ControlledSignals = ["source_id", "source_path", "correlation_id"];

    private static readonly string[] ControlValues =
        [SourceIdControlValue, SourcePathControlValue, CorrelationIdControlValue];

    private readonly IDynamicGraphEdge? _edge;
    private readonly IDynamicGraphEdgeAuthorityInputMaterializer? _materializer;
    private readonly LogicalUnitDescriptor? _descriptor;
    private readonly ILogicalUnit<EdgeAuthorityInput, EdgeAuthorityDecision>? _resolvedAuthority;

    public DynamicGraphEdgeIdentityLeakageGuardUnit(
        IDynamicGraphEdge? edge,
        IDynamicGraphEdgeAuthorityInputMaterializer? materializer,
        LogicalUnitDescriptor? descriptor,
        ILogicalUnit<EdgeAuthorityInput, EdgeAuthorityDecision>? resolvedAuthority)
    {
        _edge = edge;
        _materializer = materializer;
        _descriptor = descriptor;
        _resolvedAuthority = resolvedAuthority;
    }

    public async Task<OperationResult<LogicalUnitResult<DynamicGraphEdgeIdentityLeakageReport>>> ExecuteAsync(
        LogicalUnitRequest<DynamicGraphEdgeIdentityProbeInput> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        EdgeTraversalInput traversalInput;
        EdgeAuthorityInput normalizedInput;
        EdgeAuthorityDecision referenceDecision;
        LogicalUnitDescriptor descriptor;
        ILogicalUnit<EdgeAuthorityInput, EdgeAuthorityDecision> resolvedAuthority;
        IDynamicGraphEdgeAuthorityInputMaterializer materializer;
        EdgeSnapshot edge;
        try
        {
            if (request is null
                || request.Input is null
                || request.Flags is null
                || request.Metadata is null
                || !IsCanonicalValue(request.RunId)
                || !StringComparer.Ordinal.Equals(request.UnitId, Id)
                || !StringComparer.Ordinal.Equals(request.UnitRole, Role)
                || !IsCanonicalValue(request.DomainTag))
            {
                return GuardFailed("Edge authority identity guard request validation failed.");
            }

            if (!TrySnapshotDescriptor(_descriptor, request.DomainTag, out descriptor))
                return GuardFailed("Edge authority identity guard descriptor validation failed.");

            if (_resolvedAuthority is not { } authority || _materializer is not { } inputMaterializer)
                return GuardFailed("Edge authority identity guard dependencies are required.");

            if (!TrySnapshotProbeInput(
                    request.Input,
                    request.RunId,
                    out traversalInput,
                    out normalizedInput,
                    out referenceDecision))
            {
                return GuardFailed("Edge authority identity guard probe input validation failed.");
            }

            if (!TrySnapshotEdge(
                    _edge,
                    normalizedInput.EdgeId,
                    descriptor.UnitId,
                    out edge))
            {
                return GuardFailed("Edge authority identity guard edge validation failed.");
            }

            if (!ControlValuesAreDistinctFromOriginals(traversalInput))
                return GuardFailed("Edge authority identity guard controls collide with source identities.");

            resolvedAuthority = authority;
            materializer = inputMaterializer;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return GuardFailed("Edge authority identity guard input snapshot failed.");
        }

        var probes = new List<DynamicGraphEdgeIdentityControlProbe>(ProbeCount);
        for (var probeIndex = 0; probeIndex < ProbeCount; probeIndex++)
        {
            if (cancellationToken.IsCancellationRequested)
                return Cancelled();

            try
            {
                var controlTraversal = CreateControlTraversal(traversalInput, probeIndex);
                var materialized = materializer.Materialize(
                    edge,
                    DynamicGraphEdgeAuthorityContractSnapshots.CopyTraversalInput(controlTraversal),
                    Array.AsReadOnly(
                        normalizedInput.Candidates
                            .Select(DynamicGraphEdgeAuthorityContractSnapshots.CopyCandidate)
                            .ToArray()));

                if (cancellationToken.IsCancellationRequested)
                    return Cancelled();

                if (materialized is null
                    || !materialized.IsSuccess
                    || materialized.Value is null
                    || !DynamicGraphEdgeAuthorityExecutionPipeline.TrySnapshotNormalizedInput(
                        materialized.Value,
                        normalizedInput.EdgeId,
                        normalizedInput.Candidates,
                        out var controlInput))
                {
                    return GuardFailed("Edge authority identity guard rematerialization failed.");
                }

                if (!InputsEqual(normalizedInput, controlInput))
                    return LeakageDetected();

                var authorityRequest = new LogicalUnitRequest<EdgeAuthorityInput>(
                    controlTraversal.RunId,
                    descriptor.UnitId,
                    descriptor.UnitRole,
                    descriptor.DomainTag,
                    DynamicGraphEdgeAuthorityContractSnapshots.CopyInput(controlInput),
                    request.Flags with { },
                    new ReadOnlyDictionary<string, object>(
                        new Dictionary<string, object>(StringComparer.Ordinal)));
                var executed = await resolvedAuthority
                    .ExecuteAsync(authorityRequest, cancellationToken)
                    .ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested)
                    return Cancelled();

                if (!TrySnapshotProbeDecision(executed, descriptor, controlInput.Candidates, out var decision))
                    return GuardFailed("Edge authority identity guard authority result validation failed.");

                if (!DynamicGraphEdgeCandidateOrderLeakageGuardUnit.DecisionsEqual(referenceDecision, decision))
                    return LeakageDetected();

                probes.Add(new DynamicGraphEdgeIdentityControlProbe(
                    CreateProbeId(probeIndex),
                    CreateControlledSignal(probeIndex),
                    CreateControlValue(probeIndex),
                    decision));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return Cancelled();
            }
            catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return GuardFailed("Edge authority identity guard probe execution failed.");
            }
        }

        try
        {
            var report = new DynamicGraphEdgeIdentityLeakageReport(
                normalizedInput.EdgeId,
                descriptor.UnitId,
                IsIdentityInvariant: true,
                Array.AsReadOnly(probes.ToArray()),
                DynamicGraphEdgeAuthorityLeakageBoundary.CreateSnapshot());
            var result = new LogicalUnitResult<DynamicGraphEdgeIdentityLeakageReport>(
                Id,
                Role,
                request.DomainTag,
                report,
                1d,
                normalizedInput.PredicateIds.ToArray(),
                [request.DomainTag],
                ["EDGE_AUTHORITY_IDENTITY_INVARIANT"],
                []);
            return OperationResult<LogicalUnitResult<DynamicGraphEdgeIdentityLeakageReport>>.Ok(result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return GuardFailed("Edge authority identity guard report construction failed.");
        }
    }

    internal static string CreateProbeId(int probeIndex)
        => GetProbeValue(ProbeIds, probeIndex);

    internal static string CreateControlledSignal(int probeIndex)
        => GetProbeValue(ControlledSignals, probeIndex);

    internal static string CreateControlValue(int probeIndex)
        => GetProbeValue(ControlValues, probeIndex);

    internal static string GetOriginalValue(EdgeTraversalInput traversalInput, int probeIndex)
    {
        ArgumentNullException.ThrowIfNull(traversalInput);
        return probeIndex switch
        {
            0 => traversalInput.Source.DocumentId,
            1 => traversalInput.Source.SourcePath,
            2 => traversalInput.RunId,
            _ => throw new ArgumentOutOfRangeException(nameof(probeIndex))
        };
    }

    internal static bool InputsEqual(EdgeAuthorityInput expected, EdgeAuthorityInput actual)
    {
        if (!StringComparer.Ordinal.Equals(expected.EdgeId, actual.EdgeId)
            || !expected.SemanticTokens.SequenceEqual(actual.SemanticTokens, StringComparer.Ordinal)
            || !expected.PredicateIds.SequenceEqual(actual.PredicateIds, StringComparer.Ordinal)
            || !expected.ExcludedLeakageSignals.SequenceEqual(
                actual.ExcludedLeakageSignals,
                StringComparer.Ordinal)
            || !ContextsEqual(expected.Context, actual.Context)
            || expected.Candidates.Count != actual.Candidates.Count)
        {
            return false;
        }

        for (var index = 0; index < expected.Candidates.Count; index++)
        {
            var expectedCandidate = expected.Candidates[index];
            var actualCandidate = actual.Candidates[index];
            if (!StringComparer.Ordinal.Equals(
                    expectedCandidate.TargetSubPhaseId,
                    actualCandidate.TargetSubPhaseId)
                || !expectedCandidate.BaselineScore.Equals(actualCandidate.BaselineScore)
                || !StringComparer.Ordinal.Equals(
                    expectedCandidate.AuthorityRole,
                    actualCandidate.AuthorityRole)
                || !expectedCandidate.EvidencePredicates.SequenceEqual(
                    actualCandidate.EvidencePredicates,
                    StringComparer.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static bool TrySnapshotProbeInput(
        DynamicGraphEdgeIdentityProbeInput source,
        string expectedRunId,
        out EdgeTraversalInput traversalInput,
        out EdgeAuthorityInput normalizedInput,
        out EdgeAuthorityDecision referenceDecision)
    {
        traversalInput = null!;
        normalizedInput = null!;
        referenceDecision = null!;

        if (!DynamicGraphEdgeAuthorityExecutionPipeline.TrySnapshotTraversalInput(
                source.TraversalInput,
                out traversalInput)
            || !IsCanonicalValue(traversalInput.RunId)
            || !StringComparer.Ordinal.Equals(traversalInput.RunId, expectedRunId)
            || !IsCanonicalValue(traversalInput.EdgeId))
        {
            return false;
        }

        var input = source.NormalizedInput;
        if (!DynamicGraphEdgeAuthorityExecutionPipeline.TrySnapshotNormalizedInput(
                input,
                traversalInput.EdgeId,
                input.Candidates,
                out normalizedInput))
        {
            return false;
        }

        var candidateTargets = normalizedInput.Candidates
            .Select(candidate => candidate.TargetSubPhaseId)
            .ToArray();
        return DynamicGraphEdgeAuthorityExecutionPipeline.TrySnapshotDecision(
            source.ReferenceDecision,
            candidateTargets,
            normalizedInput.Candidates,
            out referenceDecision);
    }

    private static bool TrySnapshotDescriptor(
        LogicalUnitDescriptor? source,
        string expectedDomainTag,
        out LogicalUnitDescriptor descriptor)
    {
        descriptor = null!;
        if (source is null
            || !IsCanonicalValue(source.UnitId)
            || !StringComparer.Ordinal.Equals(source.UnitRole, ExpectedAuthorityRole)
            || !StringComparer.Ordinal.Equals(source.DomainTag, expectedDomainTag)
            || source.InputType != typeof(EdgeAuthorityInput)
            || source.OutputType != typeof(EdgeAuthorityDecision)
            || !source.IsLearned
            || !Enum.IsDefined(source.TrustState)
            || source.TrustState is LogicalUnitTrustState.Candidate or LogicalUnitTrustState.Rejected)
        {
            return false;
        }

        descriptor = source with { };
        return true;
    }

    private static bool TrySnapshotEdge(
        IDynamicGraphEdge? source,
        string expectedEdgeId,
        string expectedAuthorityUnitId,
        out EdgeSnapshot snapshot)
    {
        snapshot = null!;
        if (source is null)
            return false;

        var edgeId = source.EdgeId;
        var fromNodeId = source.FromNodeId;
        var targets = source.ToNodeIds?.ToArray();
        var leakageSignals = source.ExcludedLeakageSignals?.ToArray();
        if (!IsCanonicalValue(edgeId)
            || !StringComparer.Ordinal.Equals(edgeId, expectedEdgeId)
            || !IsCanonicalValue(fromNodeId)
            || targets is null
            || targets.Length == 0
            || targets.Any(target => !IsCanonicalValue(target))
            || targets.Distinct(StringComparer.Ordinal).Count() != targets.Length
            || !IsCanonicalValue(source.AuthorityUnitId)
            || !StringComparer.Ordinal.Equals(source.AuthorityUnitId, expectedAuthorityUnitId)
            || !StringComparer.Ordinal.Equals(source.AuthorityUnitLevel, "L2")
            || source.DeterministicFinalAuthority
            || !IsCanonicalValue(source.BaselineRole)
            || leakageSignals is null
            || !DynamicGraphEdgeAuthorityLeakageBoundary.IsExact(leakageSignals)
            || !IsCanonicalValue(source.SourceProof)
            || !IsCanonicalValue(source.CutoverProof)
            || !IsCanonicalValue(source.AblationProofRef))
        {
            return false;
        }

        var baselineUnitId = source.BaselineUnitId;
        if (baselineUnitId is not null && !IsCanonicalValue(baselineUnitId))
            return false;

        snapshot = new EdgeSnapshot(
            edgeId,
            fromNodeId,
            Array.AsReadOnly(targets),
            source.Kind,
            source.ParallelEligible,
            source.AuthorityUnitId,
            source.AuthorityUnitLevel,
            source.DeterministicFinalAuthority,
            baselineUnitId,
            source.BaselineRole,
            Array.AsReadOnly(leakageSignals),
            source.SourceProof,
            source.CutoverProof,
            source.AblationProofRef);
        return true;
    }

    private static bool TrySnapshotProbeDecision(
        OperationResult<LogicalUnitResult<EdgeAuthorityDecision>>? executed,
        LogicalUnitDescriptor descriptor,
        IReadOnlyList<EdgeCandidate> candidates,
        out EdgeAuthorityDecision decision)
    {
        decision = null!;
        if (executed is null
            || !executed.IsSuccess
            || executed.Value is null
            || !DynamicGraphEdgeAuthorityExecutionPipeline.TrySnapshotUnitEnvelope(
                executed.Value,
                descriptor,
                out _,
                out _,
                out _,
                out var decisionSource))
        {
            return false;
        }

        var targetIds = candidates.Select(candidate => candidate.TargetSubPhaseId).ToArray();
        return DynamicGraphEdgeAuthorityExecutionPipeline.TrySnapshotDecision(
            decisionSource,
            targetIds,
            candidates,
            out decision);
    }

    private static EdgeTraversalInput CreateControlTraversal(EdgeTraversalInput source, int probeIndex)
    {
        var snapshot = DynamicGraphEdgeAuthorityContractSnapshots.CopyTraversalInput(source);
        var controlledSource = probeIndex switch
        {
            0 => CopySource(snapshot.Source, documentId: SourceIdControlValue, sourcePath: snapshot.Source.SourcePath),
            1 => CopySource(snapshot.Source, documentId: snapshot.Source.DocumentId, sourcePath: SourcePathControlValue),
            2 => CopySource(snapshot.Source, snapshot.Source.DocumentId, snapshot.Source.SourcePath),
            _ => throw new ArgumentOutOfRangeException(nameof(probeIndex))
        };

        return new EdgeTraversalInput(
            probeIndex == 2 ? CorrelationIdControlValue : snapshot.RunId,
            snapshot.EdgeId,
            controlledSource,
            snapshot.Context,
            snapshot.UpstreamNodeOutput);
    }

    private static SubPhaseRouterSourceDocument CopySource(
        SubPhaseRouterSourceDocument source,
        string documentId,
        string sourcePath)
        => new(
            documentId,
            sourcePath,
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
                .Select(segment => new SubPhaseRouterSourceDocument(
                    segment.DocumentId,
                    segment.SourcePath,
                    segment.SourceTier,
                    segment.LicenseOrAuthority,
                    segment.DocumentType,
                    segment.DocumentTitle,
                    segment.CompiledText,
                    segment.CompiledPredicateIds.ToArray(),
                    segment.SourceStatus,
                    segment.KnownFailureMode,
                    segment.DomainLabel,
                    SegmentDocuments: null))
                .ToArray());

    private static bool ControlValuesAreDistinctFromOriginals(EdgeTraversalInput traversalInput)
        => Enumerable.Range(0, ProbeCount)
            .All(index => !StringComparer.Ordinal.Equals(
                GetOriginalValue(traversalInput, index),
                CreateControlValue(index)));

    private static bool ContextsEqual(ContextSignals expected, ContextSignals actual)
    {
        if (!expected.PriorDomains.SequenceEqual(actual.PriorDomains)
            || !expected.CompletedSubPhases.SequenceEqual(actual.CompletedSubPhases, StringComparer.Ordinal)
            || !expected.ActiveKnowledgeEdges.SequenceEqual(actual.ActiveKnowledgeEdges, StringComparer.Ordinal)
            || expected.PriorConfidenceMap.Count != actual.PriorConfidenceMap.Count)
        {
            return false;
        }

        foreach (var pair in expected.PriorConfidenceMap)
        {
            if (!actual.PriorConfidenceMap.TryGetValue(pair.Key, out var value)
                || !pair.Value.Equals(value))
            {
                return false;
            }
        }

        return true;
    }

    private static string GetProbeValue(IReadOnlyList<string> values, int probeIndex)
    {
        if (probeIndex is < 0 or >= ProbeCount)
            throw new ArgumentOutOfRangeException(nameof(probeIndex));

        return values[probeIndex];
    }

    private static bool IsCanonicalValue(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && StringComparer.Ordinal.Equals(value, value.Trim())
           && value.IndexOfAny(['\r', '\n', '\u0085', '\u2028', '\u2029']) < 0;

    private static OperationResult<LogicalUnitResult<DynamicGraphEdgeIdentityLeakageReport>> Cancelled()
        => Failure("Cancelled.", CancelledCode);

    private static OperationResult<LogicalUnitResult<DynamicGraphEdgeIdentityLeakageReport>> GuardFailed(
        string message)
        => Failure(message, GuardFailedCode);

    private static OperationResult<LogicalUnitResult<DynamicGraphEdgeIdentityLeakageReport>> LeakageDetected()
        => Failure("Edge authority identity leakage detected.", LeakageCode);

    private static OperationResult<LogicalUnitResult<DynamicGraphEdgeIdentityLeakageReport>> Failure(
        string message,
        string code)
        => OperationResult<LogicalUnitResult<DynamicGraphEdgeIdentityLeakageReport>>.Fail(message, code);

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
