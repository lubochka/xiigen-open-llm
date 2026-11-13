// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Collections.ObjectModel;
using AgentInfrastructure;
using Domain = SemanticCompiler29.Domain;
using SemanticCompilerAdapter;

// This pipeline executes one learned L2 authority while leaving L3 target execution and graph mutation outside the seam.
public sealed class DynamicGraphEdgeAuthorityExecutionPipeline
    : IDynamicGraphEdgeAuthorityExecutionPipeline
{
    private const string ExpectedAuthorityRole = "edge-authority";
    private const string ExpectedAuthorityLevel = "L2";
    private const string ExpectedAuthoritySource = RoutingAuthorityVocabulary.TrainedEdgeUnitRole;
    private const string ExpectedCandidateRole = RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole;
    private const string ExpectedUpstreamProjectionRule =
        "GraphNodeCandidate.Id -> UpstreamCandidateRefs (order-free set)";

    private const string CancelledCode = "CANCELLED";
    private const string CandidateProviderRequiredCode = "EDGE_AUTHORITY_CANDIDATE_PROVIDER_REQUIRED";
    private const string InputMaterializerRequiredCode = "EDGE_AUTHORITY_INPUT_MATERIALIZER_REQUIRED";
    private const string ResolverRequiredCode = "EDGE_AUTHORITY_RESOLVER_REQUIRED";
    private const string EdgeRequiredCode = "EDGE_AUTHORITY_EDGE_REQUIRED";
    private const string EdgeInvalidCode = "EDGE_AUTHORITY_EDGE_INVALID";
    private const string TraversalRequiredCode = "EDGE_AUTHORITY_TRAVERSAL_REQUIRED";
    private const string TraversalInvalidCode = "EDGE_AUTHORITY_TRAVERSAL_INVALID";
    private const string FlagsRequiredCode = "EDGE_AUTHORITY_FLAGS_REQUIRED";
    private const string RunIdInvalidCode = "EDGE_AUTHORITY_RUN_ID_INVALID";
    private const string DomainInvalidCode = "EDGE_AUTHORITY_DOMAIN_INVALID";
    private const string LearnedUnitDisabledCode = "EDGE_AUTHORITY_LEARNED_UNIT_DISABLED";
    private const string UnsupportedKindCode = "EDGE_AUTHORITY_EDGE_KIND_UNSUPPORTED";
    private const string CandidateProposalFailedCode = "EDGE_AUTHORITY_CANDIDATE_PROPOSAL_FAILED";
    private const string InputMaterializationFailedCode = "EDGE_AUTHORITY_INPUT_MATERIALIZATION_FAILED";
    private const string ResolutionFailedCode = "EDGE_AUTHORITY_RESOLUTION_FAILED";
    private const string UnitExecutionFailedCode = "EDGE_AUTHORITY_UNIT_EXECUTION_FAILED";
    private const string DecisionInvalidCode = "EDGE_AUTHORITY_DECISION_INVALID";
    private const string ResultRequiredCode = "EDGE_AUTHORITY_RESULT_REQUIRED";

    private readonly IEdgeCandidateProvider? _candidateProvider;
    private readonly IDynamicGraphEdgeAuthorityInputMaterializer? _inputMaterializer;
    private readonly IDynamicGraphEdgeAuthorityResolver? _resolver;

    public DynamicGraphEdgeAuthorityExecutionPipeline(
        IEdgeCandidateProvider? candidateProvider,
        IDynamicGraphEdgeAuthorityInputMaterializer? inputMaterializer,
        IDynamicGraphEdgeAuthorityResolver? resolver)
    {
        _candidateProvider = candidateProvider;
        _inputMaterializer = inputMaterializer;
        _resolver = resolver;
    }

    // ExecuteAsync keeps baseline generation advisory and makes the learned unit the sole decision authority.
    public async Task<OperationResult<EdgeAuthorityExecutionResult>> ExecuteAsync(
        IDynamicGraphEdge edge,
        EdgeTraversalInput input,
        string domainTag,
        LogicalExecutionFlags flags,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        if (edge is null)
            return Failure("Dynamic graph edge is required.", EdgeRequiredCode);

        if (input is null)
            return Failure("Edge traversal input is required.", TraversalRequiredCode);

        if (flags is null)
            return Failure("Logical execution flags are required.", FlagsRequiredCode);

        if (!IsCanonicalValue(domainTag))
            return Failure("Edge authority domain is invalid.", DomainInvalidCode);

        if (!flags.UseLearnedUnitWhenAvailable)
            return Failure("Learned edge authority must be enabled.", LearnedUnitDisabledCode);

        EdgeSnapshot snapshotEdge;
        EdgeTraversalInput snapshotInput;
        var snapshottingEdge = true;
        try
        {
            if (!TrySnapshotEdge(edge, out snapshotEdge))
                return Failure("Dynamic graph edge is invalid.", EdgeInvalidCode);

            snapshottingEdge = false;
            if (!TrySnapshotTraversalInput(input, out snapshotInput))
                return Failure("Edge traversal input is invalid.", TraversalInvalidCode);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return snapshottingEdge
                ? Failure("Dynamic graph edge is invalid.", EdgeInvalidCode)
                : Failure("Edge traversal input is invalid.", TraversalInvalidCode);
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        var runId = snapshotInput.RunId;
        if (!IsCanonicalValue(runId))
            return Failure("Edge authority RunId is invalid.", RunIdInvalidCode);

        if (snapshotEdge.Kind is not GraphEdgeKind.FanOut
            and not GraphEdgeKind.ManyToMany
            and not GraphEdgeKind.MultiOutput)
        {
            return Failure("Dynamic graph edge kind is unsupported by learned authority.", UnsupportedKindCode);
        }

        if (!StringComparer.Ordinal.Equals(snapshotEdge.EdgeId, snapshotInput.EdgeId))
            return Failure("Edge traversal input is invalid.", TraversalInvalidCode);

        if (!StringComparer.Ordinal.Equals(
                snapshotEdge.FromNodeId,
                snapshotInput.UpstreamNodeOutput.SourceNodeId))
        {
            return Failure("Edge traversal input is invalid.", TraversalInvalidCode);
        }

        if (!IsValidUpstreamProvenance(snapshotInput.UpstreamNodeOutput))
            return Failure("Edge traversal input is invalid.", TraversalInvalidCode);

        var candidateProvider = _candidateProvider;
        if (candidateProvider is null)
            return Failure("Edge candidate provider is required.", CandidateProviderRequiredCode);

        var inputMaterializer = _inputMaterializer;
        if (inputMaterializer is null)
            return Failure("Edge authority input materializer is required.", InputMaterializerRequiredCode);

        var resolver = _resolver;
        if (resolver is null)
            return Failure("Edge authority resolver is required.", ResolverRequiredCode);

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        OperationResult<EdgeCandidate[]>? proposed;
        try
        {
            if (!TrySnapshotTraversalInput(snapshotInput, out var providerInput))
                return CandidateProposalFailed();

            if (cancellationToken.IsCancellationRequested)
                return Cancelled();

            proposed = candidateProvider.Propose(snapshotEdge, providerInput);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return CandidateProposalFailed();
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        EdgeCandidate[] proposedSnapshot;
        try
        {
            if (proposed is null
                || !proposed.IsSuccess
                || proposed.Value is null
                || !TrySnapshotProviderCandidates(
                    proposed.Value,
                    snapshotEdge.ToNodeIds,
                    out proposedSnapshot))
            {
                return CandidateProposalFailed();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return CandidateProposalFailed();
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        OperationResult<EdgeAuthorityInput>? materialized;
        try
        {
            if (!TrySnapshotTraversalInput(snapshotInput, out var materializerInput))
                return InputMaterializationFailed();

            if (cancellationToken.IsCancellationRequested)
                return Cancelled();

            materialized = inputMaterializer.Materialize(
                snapshotEdge,
                materializerInput,
                Array.AsReadOnly(proposedSnapshot.Select(CopyCandidate).ToArray()));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InputMaterializationFailed();
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        EdgeAuthorityInput normalizedInput;
        try
        {
            if (materialized is null
                || !materialized.IsSuccess
                || materialized.Value is null
                || !TrySnapshotNormalizedInput(
                    materialized.Value,
                    snapshotEdge.EdgeId,
                    proposedSnapshot,
                    out normalizedInput))
            {
                return InputMaterializationFailed();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InputMaterializationFailed();
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        OperationResult<ResolvedDynamicGraphEdgeAuthority>? resolvedResult;
        try
        {
            resolvedResult = resolver.Resolve(snapshotEdge, domainTag);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return ResolutionFailed();
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        ResolvedDynamicGraphEdgeAuthority resolved;
        LogicalUnitDescriptor descriptor;
        ILogicalUnit<EdgeAuthorityInput, EdgeAuthorityDecision> unit;
        try
        {
            if (resolvedResult is null || !resolvedResult.IsSuccess || resolvedResult.Value is null)
                return ResolutionFailed();

            resolved = resolvedResult.Value;
            descriptor = resolved.Descriptor;
            unit = resolved.Unit;
            if (!IsValidResolvedAuthority(descriptor, unit, snapshotEdge.AuthorityUnitId, domainTag))
                return ResolutionFailed();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return ResolutionFailed();
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        LogicalUnitRequest<EdgeAuthorityInput> request;
        try
        {
            request = new LogicalUnitRequest<EdgeAuthorityInput>(
                runId,
                descriptor.UnitId,
                descriptor.UnitRole,
                descriptor.DomainTag,
                CopyInput(normalizedInput),
                flags with { },
                new ReadOnlyDictionary<string, object>(
                    new Dictionary<string, object>(StringComparer.Ordinal)));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return UnitExecutionFailed();
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        OperationResult<LogicalUnitResult<EdgeAuthorityDecision>>? executed;
        try
        {
            executed = await unit.ExecuteAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return UnitExecutionFailed();
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        if (executed is null || (executed.IsSuccess && executed.Value is null))
            return ResultRequired();

        if (!executed.IsSuccess)
            return UnitExecutionFailed();

        var unitResult = executed.Value!;
        if (unitResult.Output is null)
            return ResultRequired();

        double unitConfidence;
        string[] evidenceIds;
        string[] warnings;
        EdgeAuthorityDecision decisionSource;
        try
        {
            if (!TrySnapshotUnitEnvelope(
                    unitResult,
                    descriptor,
                    out unitConfidence,
                    out evidenceIds,
                    out warnings,
                    out decisionSource))
            {
                return UnitExecutionFailed();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return UnitExecutionFailed();
        }

        EdgeAuthorityDecision decision;
        try
        {
            if (!TrySnapshotDecision(
                    decisionSource,
                    snapshotEdge.ToNodeIds,
                    normalizedInput.Candidates,
                    out decision))
            {
                return DecisionInvalid();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return DecisionInvalid();
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        OperationResult<LogicalUnitResult<DynamicGraphEdgeCandidateOrderLeakageReport>>? guardResult;
        try
        {
            var guard = new DynamicGraphEdgeCandidateOrderLeakageGuardUnit(descriptor, unit);
            var guardRequest = new LogicalUnitRequest<DynamicGraphEdgeCandidateOrderProbeInput>(
                runId,
                DynamicGraphEdgeCandidateOrderLeakageGuardUnit.Id,
                DynamicGraphEdgeCandidateOrderLeakageGuardUnit.Role,
                domainTag,
                new DynamicGraphEdgeCandidateOrderProbeInput(
                    CopyInput(normalizedInput),
                    DynamicGraphEdgeAuthorityContractSnapshots.CopyDecision(decision)),
                flags with { },
                new ReadOnlyDictionary<string, object>(
                    new Dictionary<string, object>(StringComparer.Ordinal)));
            guardResult = await guard.ExecuteAsync(guardRequest, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return CandidateOrderGuardFailed();
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        if (guardResult is null || !guardResult.IsSuccess || guardResult.Value?.Output is null)
        {
            if (guardResult is { IsSuccess: false }
                && (StringComparer.Ordinal.Equals(
                        guardResult.ErrorCode,
                        DynamicGraphEdgeCandidateOrderLeakageGuardUnit.LeakageCode)
                    || StringComparer.Ordinal.Equals(
                        guardResult.ErrorCode,
                        DynamicGraphEdgeCandidateOrderLeakageGuardUnit.GuardFailedCode)))
            {
                return Failure(guardResult.ErrorMessage, guardResult.ErrorCode);
            }

            return CandidateOrderGuardFailed();
        }

        try
        {
            if (!TryValidateSuccessfulCandidateOrderGuardResult(
                    guardResult,
                    normalizedInput,
                    decision,
                    descriptor,
                    domainTag))
            {
                return CandidateOrderGuardFailed();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return CandidateOrderGuardFailed();
        }

        OperationResult<LogicalUnitResult<DynamicGraphEdgeIdentityLeakageReport>>? identityGuardResult;
        try
        {
            var identityGuard = new DynamicGraphEdgeIdentityLeakageGuardUnit(
                snapshotEdge,
                inputMaterializer,
                descriptor,
                unit);
            var identityGuardRequest = new LogicalUnitRequest<DynamicGraphEdgeIdentityProbeInput>(
                runId,
                DynamicGraphEdgeIdentityLeakageGuardUnit.Id,
                DynamicGraphEdgeIdentityLeakageGuardUnit.Role,
                domainTag,
                new DynamicGraphEdgeIdentityProbeInput(
                    snapshotInput,
                    CopyInput(normalizedInput),
                    DynamicGraphEdgeAuthorityContractSnapshots.CopyDecision(decision)),
                flags with { },
                new ReadOnlyDictionary<string, object>(
                    new Dictionary<string, object>(StringComparer.Ordinal)));
            identityGuardResult = await identityGuard
                .ExecuteAsync(identityGuardRequest, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return IdentityGuardFailed();
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        if (identityGuardResult is null
            || !identityGuardResult.IsSuccess
            || identityGuardResult.Value?.Output is null)
        {
            if (identityGuardResult is { IsSuccess: false }
                && (StringComparer.Ordinal.Equals(
                        identityGuardResult.ErrorCode,
                        DynamicGraphEdgeIdentityLeakageGuardUnit.LeakageCode)
                    || StringComparer.Ordinal.Equals(
                        identityGuardResult.ErrorCode,
                        DynamicGraphEdgeIdentityLeakageGuardUnit.GuardFailedCode)))
            {
                return Failure(identityGuardResult.ErrorMessage, identityGuardResult.ErrorCode);
            }

            return IdentityGuardFailed();
        }

        try
        {
            if (!TryValidateSuccessfulIdentityGuardResult(
                    identityGuardResult,
                    snapshotInput,
                    normalizedInput,
                    decision,
                    descriptor,
                    domainTag))
            {
                return IdentityGuardFailed();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return IdentityGuardFailed();
        }

        var authorityEvidence = new DynamicGraphEdgeAuthorityEvidence(
            snapshotEdge.EdgeId,
            snapshotEdge.AuthorityUnitId,
            snapshotEdge.AuthorityUnitLevel,
            snapshotEdge.DeterministicFinalAuthority,
            snapshotEdge.BaselineUnitId,
            snapshotEdge.BaselineRole,
            snapshotEdge.ExcludedLeakageSignals,
            snapshotEdge.SourceProof,
            snapshotEdge.CutoverProof,
            snapshotEdge.AblationProofRef);
        var normalizedCandidates = normalizedInput.Candidates.Select(CopyCandidate).ToArray();
        return OperationResult<EdgeAuthorityExecutionResult>.Ok(
            new EdgeAuthorityExecutionResult(
                CopyInput(normalizedInput),
                Array.AsReadOnly(normalizedCandidates),
                decision,
                authorityEvidence,
                guardResult.Value.Output,
                identityGuardResult.Value.Output,
                unitConfidence,
                Array.AsReadOnly(evidenceIds.ToArray()),
                Array.AsReadOnly(warnings.ToArray())));
    }

    private static bool TrySnapshotEdge(IDynamicGraphEdge edge, out EdgeSnapshot snapshot)
    {
        snapshot = null!;
        var edgeId = edge.EdgeId;
        var fromNodeId = edge.FromNodeId;
        var targetSource = edge.ToNodeIds;
        var kind = edge.Kind;
        var parallelEligible = edge.ParallelEligible;
        var authorityUnitId = edge.AuthorityUnitId;
        var authorityUnitLevel = edge.AuthorityUnitLevel;
        var deterministicFinalAuthority = edge.DeterministicFinalAuthority;
        var baselineUnitId = edge.BaselineUnitId;
        var baselineRole = edge.BaselineRole;
        var leakageSource = edge.ExcludedLeakageSignals;
        var sourceProof = edge.SourceProof;
        var cutoverProof = edge.CutoverProof;
        var ablationProofRef = edge.AblationProofRef;

        if (!IsCanonicalValue(edgeId)
            || !IsCanonicalValue(fromNodeId)
            || !IsCanonicalValue(authorityUnitId)
            || !StringComparer.Ordinal.Equals(authorityUnitLevel, ExpectedAuthorityLevel)
            || deterministicFinalAuthority
            || !IsCanonicalValue(baselineUnitId)
            || !IsCanonicalValue(baselineRole)
            || !IsCanonicalValue(sourceProof)
            || !IsCanonicalValue(cutoverProof)
            || !IsCanonicalValue(ablationProofRef)
            || targetSource is null
            || leakageSource is null)
        {
            return false;
        }

        var targets = targetSource.ToArray();
        var leakageSignals = leakageSource.ToArray();
        if (targets.Length == 0
            || targets.Any(target => !IsCanonicalValue(target))
            || targets.Distinct(StringComparer.Ordinal).Count() != targets.Length
            || targets.Contains(fromNodeId, StringComparer.Ordinal)
            || (kind == GraphEdgeKind.ManyToMany && targets.Length < 2)
            || !DynamicGraphEdgeAuthorityLeakageBoundary.IsExact(leakageSignals))
        {
            return false;
        }

        snapshot = new EdgeSnapshot(
            edgeId,
            fromNodeId,
            Array.AsReadOnly(targets),
            kind,
            parallelEligible,
            authorityUnitId,
            authorityUnitLevel,
            deterministicFinalAuthority,
            baselineUnitId!,
            baselineRole,
            Array.AsReadOnly(leakageSignals),
            sourceProof,
            cutoverProof,
            ablationProofRef);
        return true;
    }

    internal static bool TrySnapshotTraversalInput(
        EdgeTraversalInput input,
        out EdgeTraversalInput snapshot)
    {
        snapshot = null!;
        var runId = input.RunId;
        var edgeId = input.EdgeId;
        var source = input.Source;
        var context = input.Context;
        var upstream = input.UpstreamNodeOutput;

        if (source is null
            || context is null
            || upstream is null
            || !TrySnapshotSource(source, out var sourceSnapshot)
            || !TrySnapshotContext(context, out var contextSnapshot)
            || !TrySnapshotUpstream(upstream, out var upstreamSnapshot))
        {
            return false;
        }

        snapshot = new EdgeTraversalInput(
            runId,
            edgeId,
            sourceSnapshot,
            contextSnapshot,
            upstreamSnapshot);
        return true;
    }

    private static bool TrySnapshotSource(
        SubPhaseRouterSourceDocument source,
        out SubPhaseRouterSourceDocument snapshot)
    {
        snapshot = null!;
        var documentId = source.DocumentId;
        var sourcePath = source.SourcePath;
        var sourceTier = source.SourceTier;
        var licenseOrAuthority = source.LicenseOrAuthority;
        var documentType = source.DocumentType;
        var documentTitle = source.DocumentTitle;
        var compiledText = source.CompiledText;
        var predicateSource = source.CompiledPredicateIds;
        var sourceStatus = source.SourceStatus;
        var knownFailureMode = source.KnownFailureMode;
        var domainLabel = source.DomainLabel;
        var segmentSource = source.SegmentDocuments;

        if (predicateSource is null
            || !TrySnapshotTopLevelSegments(segmentSource, out var segments))
        {
            return false;
        }

        snapshot = new SubPhaseRouterSourceDocument(
            documentId,
            sourcePath,
            sourceTier,
            licenseOrAuthority,
            documentType,
            documentTitle,
            compiledText,
            predicateSource.ToArray(),
            sourceStatus,
            knownFailureMode,
            domainLabel,
            segments);
        return true;
    }

    private static bool TrySnapshotTopLevelSegments(
        SubPhaseRouterSourceDocument[]? source,
        out SubPhaseRouterSourceDocument[]? snapshot)
    {
        snapshot = null;
        if (source is null)
            return true;

        var topLevel = source.ToArray();
        var output = new SubPhaseRouterSourceDocument[topLevel.Length];
        for (var index = 0; index < topLevel.Length; index++)
        {
            var segment = topLevel[index];
            if (segment is null || !TrySnapshotShallowSegment(segment, out output[index]))
                return false;
        }

        snapshot = output;
        return true;
    }

    // This intentionally snapshots one top-level segment and never follows its potentially cyclic child graph.
    private static bool TrySnapshotShallowSegment(
        SubPhaseRouterSourceDocument source,
        out SubPhaseRouterSourceDocument snapshot)
    {
        snapshot = null!;
        var documentId = source.DocumentId;
        var sourcePath = source.SourcePath;
        var sourceTier = source.SourceTier;
        var licenseOrAuthority = source.LicenseOrAuthority;
        var documentType = source.DocumentType;
        var documentTitle = source.DocumentTitle;
        var compiledText = source.CompiledText;
        var predicateSource = source.CompiledPredicateIds;
        var sourceStatus = source.SourceStatus;
        var knownFailureMode = source.KnownFailureMode;
        var domainLabel = source.DomainLabel;

        if (predicateSource is null)
            return false;

        snapshot = new SubPhaseRouterSourceDocument(
            documentId,
            sourcePath,
            sourceTier,
            licenseOrAuthority,
            documentType,
            documentTitle,
            compiledText,
            predicateSource.ToArray(),
            sourceStatus,
            knownFailureMode,
            domainLabel,
            SegmentDocuments: null);
        return true;
    }

    private static bool IsValidUpstreamProvenance(NodeOutputToEdgeInputAdapter? source)
    {
        if (source is null
            || source.UpstreamCandidateRefs is not { Count: > 0 } references
            || source.UpstreamOutputHash is not { } outputHash
            || !IsCanonicalValue(outputHash)
            || outputHash.Any(char.IsControl)
            || !StringComparer.Ordinal.Equals(
                source.ProjectionRule,
                ExpectedUpstreamProjectionRule))
        {
            return false;
        }

        string? previous = null;
        foreach (var reference in references)
        {
            if (reference is null
                || !IsCanonicalValue(reference)
                || reference.Any(char.IsControl)
                || (previous is not null
                    && StringComparer.Ordinal.Compare(previous, reference) >= 0))
            {
                return false;
            }

            previous = reference;
        }

        return true;
    }

    private static bool TrySnapshotUpstream(
        NodeOutputToEdgeInputAdapter source,
        out NodeOutputToEdgeInputAdapter snapshot)
    {
        snapshot = null!;
        var sourceNodeId = source.SourceNodeId;
        var referenceSource = source.UpstreamCandidateRefs;
        var outputHash = source.UpstreamOutputHash;
        var projectionRule = source.ProjectionRule;
        if (referenceSource is null)
            return false;

        snapshot = new NodeOutputToEdgeInputAdapter(
            sourceNodeId,
            Array.AsReadOnly(referenceSource.ToArray()),
            outputHash,
            projectionRule);
        return true;
    }

    private static bool TrySnapshotProviderCandidates(
        IReadOnlyList<EdgeCandidate> candidates,
        IReadOnlyList<string> edgeTargets,
        out EdgeCandidate[] snapshots)
    {
        snapshots = [];
        var source = candidates.ToArray();
        if (source.Length == 0)
            return false;

        var allowedTargets = edgeTargets.ToHashSet(StringComparer.Ordinal);
        var seenTargets = new HashSet<string>(StringComparer.Ordinal);
        var output = new List<EdgeCandidate>(source.Length);
        foreach (var candidate in source)
        {
            if (candidate is null
                || !IsCanonicalValue(candidate.TargetSubPhaseId)
                || !allowedTargets.Contains(candidate.TargetSubPhaseId)
                || !seenTargets.Add(candidate.TargetSubPhaseId)
                || !double.IsFinite(candidate.BaselineScore)
                || candidate.BaselineScore < 0d
                || !StringComparer.Ordinal.Equals(candidate.AuthorityRole, ExpectedCandidateRole)
                || candidate.EvidencePredicates is null)
            {
                return false;
            }

            if (!TryNormalizeEvidence(
                    candidate.EvidencePredicates,
                    requireStoredCanonical: false,
                    out var evidence))
            {
                return false;
            }

            output.Add(new EdgeCandidate(
                candidate.TargetSubPhaseId,
                evidence,
                candidate.BaselineScore,
                ExpectedCandidateRole));
        }

        snapshots = output.ToArray();
        return true;
    }

    internal static bool TrySnapshotNormalizedInput(
        EdgeAuthorityInput source,
        string expectedEdgeId,
        IReadOnlyList<EdgeCandidate> proposedCandidates,
        out EdgeAuthorityInput snapshot)
    {
        snapshot = null!;
        var edgeId = source.EdgeId;
        var semanticSource = source.SemanticTokens;
        var predicateSource = source.PredicateIds;
        var contextSource = source.Context;
        var candidateSource = source.Candidates;
        var leakageSource = source.ExcludedLeakageSignals;
        if (!IsCanonicalValue(edgeId)
            || !StringComparer.Ordinal.Equals(edgeId, expectedEdgeId)
            || semanticSource is null
            || predicateSource is null
            || contextSource is null
            || candidateSource is null
            || leakageSource is null)
        {
            return false;
        }

        var leakageSignals = leakageSource.ToArray();
        if (!TrySnapshotNormalizedSemanticEvidence(
                semanticSource,
                predicateSource,
                out var semanticTokens,
                out var predicateIds)
            || !DynamicGraphEdgeAuthorityLeakageBoundary.IsExact(leakageSignals)
            || !TrySnapshotContext(contextSource, out var context))
        {
            return false;
        }

        var normalizedSource = candidateSource.ToArray();
        if (normalizedSource.Length != proposedCandidates.Count || normalizedSource.Length == 0)
            return false;

        var proposedByTarget = new Dictionary<string, EdgeCandidate>(StringComparer.Ordinal);
        foreach (var proposed in proposedCandidates)
        {
            if (!proposedByTarget.TryAdd(proposed.TargetSubPhaseId, proposed))
                return false;
        }

        var predicateSet = predicateIds.ToHashSet(StringComparer.Ordinal);
        var normalizedTargets = new HashSet<string>(StringComparer.Ordinal);
        var normalizedCandidates = new List<EdgeCandidate>(normalizedSource.Length);
        foreach (var candidate in normalizedSource)
        {
            if (candidate is null
                || !IsCanonicalValue(candidate.TargetSubPhaseId)
                || !normalizedTargets.Add(candidate.TargetSubPhaseId)
                || !proposedByTarget.TryGetValue(candidate.TargetSubPhaseId, out var proposed)
                || !double.IsFinite(candidate.BaselineScore)
                || candidate.BaselineScore < 0d
                || !candidate.BaselineScore.Equals(proposed.BaselineScore)
                || !StringComparer.Ordinal.Equals(candidate.AuthorityRole, ExpectedCandidateRole)
                || !StringComparer.Ordinal.Equals(candidate.AuthorityRole, proposed.AuthorityRole)
                || candidate.EvidencePredicates is null)
            {
                return false;
            }

            if (!TryNormalizeEvidence(
                    candidate.EvidencePredicates,
                    requireStoredCanonical: true,
                    out var evidence)
                || !TryNormalizeEvidence(
                    proposed.EvidencePredicates,
                    requireStoredCanonical: true,
                    out var proposedEvidence)
                || !evidence.SequenceEqual(proposedEvidence, StringComparer.Ordinal)
                || evidence.Any(value => !predicateSet.Contains(value)))
            {
                return false;
            }

            normalizedCandidates.Add(new EdgeCandidate(
                candidate.TargetSubPhaseId,
                evidence,
                candidate.BaselineScore,
                ExpectedCandidateRole));
        }

        if (normalizedTargets.Count != proposedByTarget.Count)
            return false;

        snapshot = new EdgeAuthorityInput(
            edgeId,
            semanticTokens,
            predicateIds,
            context,
            Array.AsReadOnly(normalizedCandidates.ToArray()),
            leakageSignals);
        return true;
    }

    internal static bool TrySnapshotNormalizedSemanticEvidence(
        IReadOnlyList<string>? semanticSource,
        IReadOnlyList<string>? predicateSource,
        out string[] semanticTokens,
        out string[] predicateIds)
    {
        semanticTokens = [];
        predicateIds = [];
        if (semanticSource is null
            || predicateSource is null
            || !TrySnapshotCanonicalArray(semanticSource, allowEmpty: true, out semanticTokens)
            || !TrySnapshotNormalizedPredicateIds(predicateSource, out predicateIds)
            || semanticTokens.Length + predicateIds.Length == 0)
        {
            semanticTokens = [];
            predicateIds = [];
            return false;
        }

        return true;
    }

    private static bool TrySnapshotNormalizedPredicateIds(
        IReadOnlyList<string> values,
        out string[] snapshot)
    {
        snapshot = values.ToArray();
        if (snapshot.Any(value => string.IsNullOrWhiteSpace(value)
                                  || ContainsLineBreak(value)
                                  || !StringComparer.Ordinal.Equals(
                                      value,
                                      value.Trim().ToUpperInvariant()))
            || snapshot.Distinct(StringComparer.Ordinal).Count() != snapshot.Length)
        {
            snapshot = [];
            return false;
        }

        return true;
    }

    private static bool TryNormalizeEvidence(
        IReadOnlyList<string> values,
        bool requireStoredCanonical,
        out string[] snapshot)
    {
        var normalized = new HashSet<string>(StringComparer.Ordinal);
        foreach (var value in values.ToArray())
        {
            if (string.IsNullOrWhiteSpace(value) || ContainsLineBreak(value))
            {
                snapshot = [];
                return false;
            }

            var canonical = value.Trim().ToUpperInvariant();
            if (requireStoredCanonical && !StringComparer.Ordinal.Equals(value, canonical))
            {
                snapshot = [];
                return false;
            }

            if (!normalized.Add(canonical) && requireStoredCanonical)
            {
                snapshot = [];
                return false;
            }
        }

        snapshot = normalized.OrderBy(value => value, StringComparer.Ordinal).ToArray();
        return true;
    }

    private static bool TrySnapshotCanonicalArray(
        IReadOnlyList<string> values,
        bool allowEmpty,
        out string[] snapshot)
    {
        snapshot = values.ToArray();
        if ((!allowEmpty && snapshot.Length == 0)
            || snapshot.Any(value => !IsCanonicalValue(value) || ContainsLineBreak(value))
            || snapshot.Distinct(StringComparer.Ordinal).Count() != snapshot.Length)
        {
            snapshot = [];
            return false;
        }

        return true;
    }

    private static bool TrySnapshotContext(ContextSignals source, out ContextSignals snapshot)
    {
        snapshot = null!;
        var priorDomainSource = source.PriorDomains;
        var completedSource = source.CompletedSubPhases;
        var activeEdgeSource = source.ActiveKnowledgeEdges;
        var confidenceSource = source.PriorConfidenceMap;
        if (priorDomainSource is null
            || completedSource is null
            || activeEdgeSource is null
            || confidenceSource is null)
        {
            return false;
        }

        var priorDomains = priorDomainSource.ToArray();
        if (priorDomains.Any(domain => !Enum.IsDefined(typeof(Domain), domain)))
            return false;

        priorDomains = priorDomains.Distinct().OrderBy(domain => (int)domain).ToArray();
        if (!TrySnapshotCanonicalArray(completedSource, allowEmpty: true, out var completed)
            || !TrySnapshotCanonicalArray(activeEdgeSource, allowEmpty: true, out var activeEdges))
        {
            return false;
        }

        var confidence = new SortedDictionary<Domain, double>();
        var expectedConfidenceCount = confidenceSource.Count;
        foreach (var pair in confidenceSource)
        {
            if (!Enum.IsDefined(typeof(Domain), pair.Key)
                || !double.IsFinite(pair.Value)
                || !confidence.TryAdd(pair.Key, pair.Value))
            {
                return false;
            }
        }

        if (confidence.Count != expectedConfidenceCount)
            return false;

        snapshot = new ContextSignals(
            priorDomains,
            completed,
            activeEdges,
            new ReadOnlyDictionary<Domain, double>(confidence));
        return true;
    }

    private static bool IsValidResolvedAuthority(
        LogicalUnitDescriptor? descriptor,
        ILogicalUnit<EdgeAuthorityInput, EdgeAuthorityDecision>? unit,
        string expectedUnitId,
        string expectedDomainTag)
    {
        if (descriptor is null
            || unit is null
            || !IsCanonicalValue(descriptor.UnitId)
            || !IsCanonicalValue(descriptor.UnitRole)
            || !IsCanonicalValue(descriptor.DomainTag)
            || !StringComparer.Ordinal.Equals(descriptor.UnitId, expectedUnitId)
            || !StringComparer.Ordinal.Equals(descriptor.UnitRole, ExpectedAuthorityRole)
            || !StringComparer.Ordinal.Equals(descriptor.DomainTag, expectedDomainTag)
            || !descriptor.IsLearned
            || descriptor.InputType != typeof(EdgeAuthorityInput)
            || descriptor.OutputType != typeof(EdgeAuthorityDecision)
            || !Enum.IsDefined(typeof(LogicalUnitTrustState), descriptor.TrustState)
            || descriptor.TrustState is LogicalUnitTrustState.Candidate or LogicalUnitTrustState.Rejected)
        {
            return false;
        }

        return true;
    }

    internal static bool TrySnapshotUnitEnvelope(
        LogicalUnitResult<EdgeAuthorityDecision> source,
        LogicalUnitDescriptor descriptor,
        out double unitConfidence,
        out string[] evidenceIds,
        out string[] warnings,
        out EdgeAuthorityDecision decision)
    {
        unitConfidence = 0d;
        evidenceIds = [];
        warnings = [];
        decision = null!;

        if (!StringComparer.Ordinal.Equals(source.UnitId, descriptor.UnitId)
            || !StringComparer.Ordinal.Equals(source.UnitRole, descriptor.UnitRole)
            || !StringComparer.Ordinal.Equals(source.DomainTag, descriptor.DomainTag)
            || !double.IsFinite(source.Confidence)
            || source.Confidence is < 0d or > 1d
            || source.PredicateIds is null
            || source.DomainTags is null
            || source.EvidenceIds is null
            || source.Warnings is null
            || source.Output is null
            || source.TransitionSignal is not null)
        {
            return false;
        }

        var predicateIds = source.PredicateIds.ToArray();
        var domainTags = source.DomainTags.ToArray();
        evidenceIds = source.EvidenceIds.ToArray();
        warnings = source.Warnings.ToArray();

        if (predicateIds.Any(value => value is null)
            || domainTags.Any(value => value is null)
            || evidenceIds.Any(value => value is null || ContainsLineBreak(value))
            || warnings.Any(value => value is null || ContainsLineBreak(value)))
        {
            evidenceIds = [];
            warnings = [];
            return false;
        }

        unitConfidence = source.Confidence;
        decision = source.Output;
        return true;
    }

    internal static bool TrySnapshotDecision(
        EdgeAuthorityDecision source,
        IReadOnlyList<string> edgeTargets,
        IReadOnlyList<EdgeCandidate> normalizedCandidates,
        out EdgeAuthorityDecision snapshot)
    {
        snapshot = null!;
        if (!StringComparer.Ordinal.Equals(source.AuthoritySource, ExpectedAuthoritySource)
            || source.OrderedTargets is null
            || source.AblationBaselineRank is null)
        {
            return false;
        }

        var orderedTargets = source.OrderedTargets.ToArray();
        var ablationRank = source.AblationBaselineRank.ToArray();
        var candidateSnapshot = normalizedCandidates.Select(CopyCandidate).ToArray();
        var edgeTargetSet = edgeTargets.ToHashSet(StringComparer.Ordinal);
        var candidateIds = candidateSnapshot
            .Select(candidate => candidate.TargetSubPhaseId)
            .ToHashSet(StringComparer.Ordinal);

        if (candidateIds.Count != candidateSnapshot.Length)
            return false;

        var selectedIds = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < orderedTargets.Length; index++)
        {
            var target = orderedTargets[index];
            if (!IsCanonicalValue(target.TargetSubPhaseId)
                || !selectedIds.Add(target.TargetSubPhaseId)
                || !edgeTargetSet.Contains(target.TargetSubPhaseId)
                || !candidateIds.Contains(target.TargetSubPhaseId)
                || !double.IsFinite(target.Confidence)
                || target.Confidence is < 0d or > 1d)
            {
                return false;
            }

            if (index == 0)
                continue;

            var previous = orderedTargets[index - 1];
            if (previous.Confidence < target.Confidence
                || (previous.Confidence.Equals(target.Confidence)
                    && StringComparer.Ordinal.Compare(
                        previous.TargetSubPhaseId,
                        target.TargetSubPhaseId) >= 0))
            {
                return false;
            }
        }

        if (source.FanOutCount != orderedTargets.Length
            || source.FanOutCount > candidateSnapshot.Length
            || source.FanOutCount > edgeTargetSet.Count
            || (source.KnowledgeEdge && (orderedTargets.Length != 0 || source.FanOutCount != 0))
            || (!source.KnowledgeEdge && orderedTargets.Length == 0))
        {
            return false;
        }

        var expectedBaselineRank = candidateSnapshot
            .OrderByDescending(candidate => candidate.BaselineScore)
            .ThenBy(candidate => candidate.TargetSubPhaseId, StringComparer.Ordinal)
            .Select(candidate => candidate.TargetSubPhaseId)
            .ToArray();

        if (ablationRank.Length != expectedBaselineRank.Length
            || ablationRank.Any(value => !IsCanonicalValue(value))
            || ablationRank.Distinct(StringComparer.Ordinal).Count() != ablationRank.Length)
        {
            return false;
        }

        for (var index = 0; index < expectedBaselineRank.Length; index++)
        {
            if (!StringComparer.Ordinal.Equals(ablationRank[index], expectedBaselineRank[index]))
                return false;
        }

        snapshot = new EdgeAuthorityDecision(
            Array.AsReadOnly(orderedTargets.ToArray()),
            source.FanOutCount,
            source.KnowledgeEdge,
            ExpectedAuthoritySource,
            Array.AsReadOnly(ablationRank.ToArray()));
        return true;
    }

    // This fail-closed seam verifies the complete diagnostics envelope without trusting guard-produced ordering.
    internal static bool TryValidateSuccessfulCandidateOrderGuardResult(
        OperationResult<LogicalUnitResult<DynamicGraphEdgeCandidateOrderLeakageReport>>? guardResult,
        EdgeAuthorityInput normalizedInput,
        EdgeAuthorityDecision canonicalDecision,
        LogicalUnitDescriptor descriptor,
        string expectedDomainTag)
    {
        if (guardResult is null
            || !guardResult.IsSuccess
            || guardResult.Value is not { } envelope
            || envelope.Output is not { } report
            || !StringComparer.Ordinal.Equals(
                envelope.UnitId,
                DynamicGraphEdgeCandidateOrderLeakageGuardUnit.Id)
            || !StringComparer.Ordinal.Equals(
                envelope.UnitRole,
                DynamicGraphEdgeCandidateOrderLeakageGuardUnit.Role)
            || !StringComparer.Ordinal.Equals(envelope.DomainTag, expectedDomainTag)
            || !envelope.Confidence.Equals(1d)
            || envelope.PredicateIds is null
            || !envelope.PredicateIds.SequenceEqual(normalizedInput.PredicateIds, StringComparer.Ordinal)
            || envelope.DomainTags is null
            || !envelope.DomainTags.SequenceEqual([expectedDomainTag], StringComparer.Ordinal)
            || envelope.EvidenceIds is null
            || !envelope.EvidenceIds.SequenceEqual(
                ["EDGE_AUTHORITY_CANDIDATE_ORDER_INVARIANT"],
                StringComparer.Ordinal)
            || envelope.Warnings is null
            || envelope.Warnings.Length != 0
            || envelope.TransitionSignal is not null
            || !report.IsOrderInvariant
            || !StringComparer.Ordinal.Equals(report.EdgeId, normalizedInput.EdgeId)
            || !StringComparer.Ordinal.Equals(report.ResolvedAuthorityUnitId, descriptor.UnitId)
            || !DynamicGraphEdgeAuthorityLeakageBoundary.IsExact(report.ExcludedLeakageSignals))
        {
            return false;
        }

        var probes = report.Probes;
        if (probes is null || probes.Count != 5)
            return false;

        for (var probeIndex = 0; probeIndex < probes.Count; probeIndex++)
        {
            var probe = probes[probeIndex];
            if (probe is null
                || !StringComparer.Ordinal.Equals(
                    probe.ProbeId,
                    DynamicGraphEdgeCandidateOrderLeakageGuardUnit.CreateProbeId(probeIndex)))
            {
                return false;
            }

            var expectedCandidates = DynamicGraphEdgeCandidateOrderLeakageGuardUnit
                .CreateExpectedPermutation(normalizedInput.Candidates, probeIndex);
            if (!CandidateOrdersEqual(probe.CandidateOrder, expectedCandidates)
                || !DynamicGraphEdgeCandidateOrderLeakageGuardUnit.DecisionsEqual(
                    canonicalDecision,
                    probe.Decision))
            {
                return false;
            }
        }

        return true;
    }

    // This fail-closed seam validates identity diagnostics independently from the guard implementation.
    internal static bool TryValidateSuccessfulIdentityGuardResult(
        OperationResult<LogicalUnitResult<DynamicGraphEdgeIdentityLeakageReport>>? guardResult,
        EdgeTraversalInput traversalInput,
        EdgeAuthorityInput normalizedInput,
        EdgeAuthorityDecision canonicalDecision,
        LogicalUnitDescriptor descriptor,
        string expectedDomainTag)
    {
        if (guardResult is null
            || !guardResult.IsSuccess
            || guardResult.Value is not { } envelope
            || envelope.Output is not { } report
            || !StringComparer.Ordinal.Equals(
                envelope.UnitId,
                DynamicGraphEdgeIdentityLeakageGuardUnit.Id)
            || !StringComparer.Ordinal.Equals(
                envelope.UnitRole,
                DynamicGraphEdgeIdentityLeakageGuardUnit.Role)
            || !StringComparer.Ordinal.Equals(envelope.DomainTag, expectedDomainTag)
            || !envelope.Confidence.Equals(1d)
            || envelope.PredicateIds is null
            || !envelope.PredicateIds.SequenceEqual(normalizedInput.PredicateIds, StringComparer.Ordinal)
            || envelope.DomainTags is null
            || !envelope.DomainTags.SequenceEqual([expectedDomainTag], StringComparer.Ordinal)
            || envelope.EvidenceIds is null
            || !envelope.EvidenceIds.SequenceEqual(
                ["EDGE_AUTHORITY_IDENTITY_INVARIANT"],
                StringComparer.Ordinal)
            || envelope.Warnings is null
            || envelope.Warnings.Length != 0
            || envelope.TransitionSignal is not null
            || !report.IsIdentityInvariant
            || !StringComparer.Ordinal.Equals(report.EdgeId, normalizedInput.EdgeId)
            || !StringComparer.Ordinal.Equals(report.ResolvedAuthorityUnitId, descriptor.UnitId)
            || !DynamicGraphEdgeAuthorityLeakageBoundary.IsExact(report.ExcludedLeakageSignals))
        {
            return false;
        }

        var probes = report.Probes;
        if (probes is null || probes.Count != 3)
            return false;

        for (var probeIndex = 0; probeIndex < probes.Count; probeIndex++)
        {
            var probe = probes[probeIndex];
            var expectedControlValue = DynamicGraphEdgeIdentityLeakageGuardUnit
                .CreateControlValue(probeIndex);
            if (probe is null
                || !StringComparer.Ordinal.Equals(
                    probe.ProbeId,
                    DynamicGraphEdgeIdentityLeakageGuardUnit.CreateProbeId(probeIndex))
                || !StringComparer.Ordinal.Equals(
                    probe.ControlledSignal,
                    DynamicGraphEdgeIdentityLeakageGuardUnit.CreateControlledSignal(probeIndex))
                || !StringComparer.Ordinal.Equals(probe.ControlValue, expectedControlValue)
                || !IsCanonicalValue(probe.ControlValue)
                || StringComparer.Ordinal.Equals(
                    probe.ControlValue,
                    DynamicGraphEdgeIdentityLeakageGuardUnit.GetOriginalValue(
                        traversalInput,
                        probeIndex))
                || !DynamicGraphEdgeCandidateOrderLeakageGuardUnit.DecisionsEqual(
                    canonicalDecision,
                    probe.Decision))
            {
                return false;
            }
        }

        return true;
    }

    private static bool CandidateOrdersEqual(
        IReadOnlyList<EdgeCandidate>? actual,
        IReadOnlyList<EdgeCandidate> expected)
    {
        if (actual is null || actual.Count != expected.Count)
            return false;

        for (var index = 0; index < expected.Count; index++)
        {
            var actualCandidate = actual[index];
            var expectedCandidate = expected[index];
            if (actualCandidate is null
                || !StringComparer.Ordinal.Equals(
                    actualCandidate.TargetSubPhaseId,
                    expectedCandidate.TargetSubPhaseId)
                || !actualCandidate.BaselineScore.Equals(expectedCandidate.BaselineScore)
                || !StringComparer.Ordinal.Equals(
                    actualCandidate.AuthorityRole,
                    expectedCandidate.AuthorityRole)
                || actualCandidate.EvidencePredicates is null
                || !actualCandidate.EvidencePredicates.SequenceEqual(
                    expectedCandidate.EvidencePredicates,
                    StringComparer.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static EdgeAuthorityInput CopyInput(EdgeAuthorityInput source)
    {
        return new EdgeAuthorityInput(
            source.EdgeId,
            source.SemanticTokens.ToArray(),
            source.PredicateIds.ToArray(),
            CopyContext(source.Context),
            Array.AsReadOnly(source.Candidates.Select(CopyCandidate).ToArray()),
            source.ExcludedLeakageSignals.ToArray());
    }

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

    private static EdgeCandidate CopyCandidate(EdgeCandidate source)
        => source with { EvidencePredicates = source.EvidencePredicates.ToArray() };

    private static bool IsCanonicalValue(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && StringComparer.Ordinal.Equals(value, value.Trim())
           && !ContainsLineBreak(value);

    private static bool ContainsLineBreak(string value)
        => value.IndexOfAny(['\r', '\n', '\u0085', '\u2028', '\u2029']) >= 0;

    private static OperationResult<EdgeAuthorityExecutionResult> Cancelled()
        => Failure("Cancelled.", CancelledCode);

    private static OperationResult<EdgeAuthorityExecutionResult> CandidateProposalFailed()
        => Failure("Edge authority candidate proposal failed.", CandidateProposalFailedCode);

    private static OperationResult<EdgeAuthorityExecutionResult> InputMaterializationFailed()
        => Failure("Edge authority input materialization failed.", InputMaterializationFailedCode);

    private static OperationResult<EdgeAuthorityExecutionResult> ResolutionFailed()
        => Failure("Edge authority resolution failed.", ResolutionFailedCode);

    private static OperationResult<EdgeAuthorityExecutionResult> UnitExecutionFailed()
        => Failure("Edge authority unit execution failed.", UnitExecutionFailedCode);

    private static OperationResult<EdgeAuthorityExecutionResult> DecisionInvalid()
        => Failure("Edge authority decision is invalid.", DecisionInvalidCode);

    private static OperationResult<EdgeAuthorityExecutionResult> ResultRequired()
        => Failure("Edge authority execution result is required.", ResultRequiredCode);

    private static OperationResult<EdgeAuthorityExecutionResult> CandidateOrderGuardFailed()
        => Failure(
            "Edge authority candidate-order guard failed.",
            DynamicGraphEdgeCandidateOrderLeakageGuardUnit.GuardFailedCode);

    private static OperationResult<EdgeAuthorityExecutionResult> IdentityGuardFailed()
        => Failure(
            "Edge authority identity guard failed.",
            DynamicGraphEdgeIdentityLeakageGuardUnit.GuardFailedCode);

    private static OperationResult<EdgeAuthorityExecutionResult> Failure(string message, string code)
        => OperationResult<EdgeAuthorityExecutionResult>.Fail(message, code);

    private sealed record EdgeSnapshot(
        string EdgeId,
        string FromNodeId,
        IReadOnlyList<string> ToNodeIds,
        GraphEdgeKind Kind,
        bool ParallelEligible,
        string AuthorityUnitId,
        string AuthorityUnitLevel,
        bool DeterministicFinalAuthority,
        string BaselineUnitId,
        string BaselineRole,
        IReadOnlyList<string> ExcludedLeakageSignals,
        string SourceProof,
        string CutoverProof,
        string AblationProofRef) : IDynamicGraphEdge;
}
