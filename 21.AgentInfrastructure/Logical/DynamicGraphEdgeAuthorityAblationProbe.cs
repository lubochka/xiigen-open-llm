// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Collections.ObjectModel;
using AgentInfrastructure;
using Domain = SemanticCompiler29.Domain;
using SemanticCompilerAdapter;

public interface IDynamicGraphEdgeAuthorityAblationProbe
{
    Task<OperationResult<DynamicGraphEdgeAuthorityAblationProbeResult>> RunAsync(
        IDynamicGraphEdge? edge,
        EdgeTraversalInput? input,
        string? domainTag,
        LogicalExecutionFlags? flags,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Owns paired learned-subject and independently materialized deterministic-baseline evidence.
/// This carrier proves only the authority-selection boundary. It does not prove held-out accuracy
/// drop, target execution, full physical disablement, novelty, aggregate quality, trained state,
/// lifecycle, promotion, graph mutation, DPO, teacher behavior, or training.
/// </summary>
public sealed record DynamicGraphEdgeAuthorityAblationProbeResult
{
    private readonly EdgeAuthorityExecutionResult _subject;
    private readonly EdgeAuthorityInput _baselineInput;
    private readonly string[] _orderedBaselineTargetIds;
    private readonly string _authorityUnitExcludedFromDirectBaselineBranchId;

    public DynamicGraphEdgeAuthorityAblationProbeResult(
        EdgeAuthorityExecutionResult Subject,
        EdgeAuthorityInput BaselineInput,
        IReadOnlyList<string> OrderedBaselineTargetIds,
        string AuthorityUnitExcludedFromDirectBaselineBranchId)
    {
        ArgumentNullException.ThrowIfNull(Subject);
        ArgumentNullException.ThrowIfNull(BaselineInput);
        ArgumentNullException.ThrowIfNull(OrderedBaselineTargetIds);
        ArgumentNullException.ThrowIfNull(AuthorityUnitExcludedFromDirectBaselineBranchId);

        _subject = CopySubject(Subject);
        _baselineInput = CopyInput(BaselineInput);
        _orderedBaselineTargetIds = OrderedBaselineTargetIds.ToArray();
        _authorityUnitExcludedFromDirectBaselineBranchId =
            AuthorityUnitExcludedFromDirectBaselineBranchId;
    }

    public EdgeAuthorityExecutionResult Subject => CopySubject(_subject);

    public EdgeAuthorityInput BaselineInput => CopyInput(_baselineInput);

    public IReadOnlyList<string> OrderedBaselineTargetIds
        => Array.AsReadOnly(_orderedBaselineTargetIds.ToArray());

    public string AuthorityUnitExcludedFromDirectBaselineBranchId
        => _authorityUnitExcludedFromDirectBaselineBranchId;

    // This records only calls made directly by this probe. It does not attest to provider or
    // materializer internals, their dependencies, or any transitive provenance.
    public bool ProbeDirectlyInvokedLearnedAuthorityForBaseline => false;

    private static EdgeAuthorityExecutionResult CopySubject(EdgeAuthorityExecutionResult source)
        => new(
            CopyInput(source.NormalizedInput),
            source.ProposedCandidates.Select(CopyCandidate).ToArray(),
            DynamicGraphEdgeAuthorityContractSnapshots.CopyDecision(source.Decision),
            DynamicGraphEdgeAuthorityContractSnapshots.CopyAuthorityEvidence(source.AuthorityEvidence),
            DynamicGraphEdgeAuthorityContractSnapshots.CopyReport(source.CandidateOrderLeakageReport),
            DynamicGraphEdgeAuthorityContractSnapshots.CopyIdentityReport(source.IdentityLeakageReport),
            source.UnitConfidence,
            source.EvidenceIds.ToArray(),
            source.Warnings.ToArray());

    private static EdgeAuthorityInput CopyInput(EdgeAuthorityInput source)
        => DynamicGraphEdgeAuthorityContractSnapshots.CopyInput(source);

    private static EdgeCandidate CopyCandidate(EdgeCandidate source)
        => DynamicGraphEdgeAuthorityContractSnapshots.CopyCandidate(source);
}

/// <summary>
/// Executes a learned authority subject once, then directly invokes only the configured candidate
/// provider and materializer for the deterministic-baseline branch. This does not attest to either
/// dependency's internals or transitive provenance. The result is boundary evidence only: no target
/// is executed, no graph is mutated, and no held-out numeric quality claim is made.
/// </summary>
public sealed class DynamicGraphEdgeAuthorityAblationProbe
    : IDynamicGraphEdgeAuthorityAblationProbe
{
    private const string ExpectedAuthorityLevel = "L2";
    private const string ExpectedCandidateRole =
        RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole;
    private const string ExpectedAuthoritySource = RoutingAuthorityVocabulary.TrainedEdgeUnitRole;
    private const string ExpectedUpstreamProjectionRule =
        "GraphNodeCandidate.Id -> UpstreamCandidateRefs (order-free set)";

    private const string CancelledCode = "CANCELLED";
    private const string SubjectPipelineRequiredCode =
        "EDGE_AUTHORITY_ABLATION_SUBJECT_PIPELINE_REQUIRED";
    private const string BaselineProviderRequiredCode =
        "EDGE_AUTHORITY_ABLATION_BASELINE_PROVIDER_REQUIRED";
    private const string BaselineMaterializerRequiredCode =
        "EDGE_AUTHORITY_ABLATION_BASELINE_MATERIALIZER_REQUIRED";
    private const string EdgeRequiredCode = "EDGE_AUTHORITY_ABLATION_EDGE_REQUIRED";
    private const string EdgeInvalidCode = "EDGE_AUTHORITY_ABLATION_EDGE_INVALID";
    private const string TraversalRequiredCode = "EDGE_AUTHORITY_ABLATION_TRAVERSAL_REQUIRED";
    private const string TraversalInvalidCode = "EDGE_AUTHORITY_ABLATION_TRAVERSAL_INVALID";
    private const string FlagsRequiredCode = "EDGE_AUTHORITY_ABLATION_FLAGS_REQUIRED";
    private const string DomainInvalidCode = "EDGE_AUTHORITY_ABLATION_DOMAIN_INVALID";
    private const string LearnedUnitDisabledCode = "EDGE_AUTHORITY_ABLATION_LEARNED_UNIT_DISABLED";
    private const string SubjectExecutionFailedCode =
        "EDGE_AUTHORITY_ABLATION_SUBJECT_EXECUTION_FAILED";
    private const string SubjectResultInvalidCode =
        "EDGE_AUTHORITY_ABLATION_SUBJECT_RESULT_INVALID";
    private const string BaselineCandidateFailedCode =
        "EDGE_AUTHORITY_ABLATION_BASELINE_CANDIDATE_FAILED";
    private const string BaselineMaterializationFailedCode =
        "EDGE_AUTHORITY_ABLATION_BASELINE_MATERIALIZATION_FAILED";
    private const string InputMismatchCode =
        "EDGE_AUTHORITY_ABLATION_SUBJECT_BASELINE_INPUT_MISMATCH";
    private const string BaselineRankMismatchCode =
        "EDGE_AUTHORITY_ABLATION_BASELINE_RANK_MISMATCH";

    private readonly IDynamicGraphEdgeAuthorityExecutionPipeline? _subjectPipeline;
    private readonly IEdgeCandidateProvider? _baselineCandidateProvider;
    private readonly IDynamicGraphEdgeAuthorityInputMaterializer? _baselineInputMaterializer;

    public DynamicGraphEdgeAuthorityAblationProbe(
        IDynamicGraphEdgeAuthorityExecutionPipeline? subjectPipeline,
        IEdgeCandidateProvider? baselineCandidateProvider,
        IDynamicGraphEdgeAuthorityInputMaterializer? baselineInputMaterializer)
    {
        _subjectPipeline = subjectPipeline;
        _baselineCandidateProvider = baselineCandidateProvider;
        _baselineInputMaterializer = baselineInputMaterializer;
    }

    /// <summary>
    /// Produces paired evidence without this probe directly executing a baseline resolver, learned
    /// unit, selected target, graph mutation, training operation, DPO operation, or numeric held-out
    /// scorer. No claim is made about provider or materializer internals or transitive provenance.
    /// </summary>
    public async Task<OperationResult<DynamicGraphEdgeAuthorityAblationProbeResult>> RunAsync(
        IDynamicGraphEdge? edge,
        EdgeTraversalInput? input,
        string? domainTag,
        LogicalExecutionFlags? flags,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        var subjectPipeline = _subjectPipeline;
        if (subjectPipeline is null)
        {
            return Failure(
                "Learned edge authority subject pipeline is required.",
                SubjectPipelineRequiredCode);
        }

        var baselineCandidateProvider = _baselineCandidateProvider;
        if (baselineCandidateProvider is null)
        {
            return Failure(
                "Independent baseline candidate provider is required.",
                BaselineProviderRequiredCode);
        }

        var baselineInputMaterializer = _baselineInputMaterializer;
        if (baselineInputMaterializer is null)
        {
            return Failure(
                "Independent baseline input materializer is required.",
                BaselineMaterializerRequiredCode);
        }

        if (edge is null)
            return Failure("Dynamic graph edge is required.", EdgeRequiredCode);

        if (input is null)
            return Failure("Edge traversal input is required.", TraversalRequiredCode);

        if (flags is null)
            return Failure("Logical execution flags are required.", FlagsRequiredCode);

        if (!IsCanonicalValue(domainTag))
            return Failure("Edge authority ablation domain is invalid.", DomainInvalidCode);

        if (!flags.UseLearnedUnitWhenAvailable)
            return Failure("Learned edge authority subject must be enabled.", LearnedUnitDisabledCode);

        EdgeSnapshot stableEdge;
        EdgeTraversalInput stableInput;
        LogicalExecutionFlags stableFlags;
        var snapshottingEdge = true;
        try
        {
            if (!TrySnapshotEdge(edge, out stableEdge))
                return Failure("Dynamic graph edge is invalid.", EdgeInvalidCode);

            snapshottingEdge = false;
            if (!DynamicGraphEdgeAuthorityExecutionPipeline.TrySnapshotTraversalInput(
                    input,
                    out stableInput))
            {
                return Failure("Edge traversal input is invalid.", TraversalInvalidCode);
            }

            stableFlags = flags with { };
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

        if (!IsValidTraversalCorrelation(stableEdge, stableInput))
            return Failure("Edge traversal input is invalid.", TraversalInvalidCode);

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        OperationResult<EdgeAuthorityExecutionResult>? subjectExecution;
        try
        {
            subjectExecution = await subjectPipeline.ExecuteAsync(
                    stableEdge.Copy(),
                    CopyTraversalInput(stableInput),
                    domainTag!,
                    stableFlags with { },
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Failure(
                "Learned edge authority subject execution failed.",
                SubjectExecutionFailedCode);
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        if (subjectExecution is null)
        {
            return Failure(
                "Learned edge authority subject result is invalid.",
                SubjectResultInvalidCode);
        }

        if (!subjectExecution.IsSuccess)
            return subjectExecution.MapError<DynamicGraphEdgeAuthorityAblationProbeResult>();

        EdgeAuthorityExecutionResult subject;
        try
        {
            if (subjectExecution.Value is null
                || !TrySnapshotSubject(
                    subjectExecution.Value,
                    stableEdge,
                    stableInput,
                    out subject))
            {
                return Failure(
                    "Learned edge authority subject result is invalid.",
                    SubjectResultInvalidCode);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Failure(
                "Learned edge authority subject result is invalid.",
                SubjectResultInvalidCode);
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        OperationResult<EdgeCandidate[]>? baselineProposal;
        try
        {
            baselineProposal = baselineCandidateProvider.Propose(
                stableEdge.Copy(),
                CopyTraversalInput(stableInput));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return BaselineCandidateFailed();
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        EdgeCandidate[] baselineCandidates;
        try
        {
            if (baselineProposal is null
                || !baselineProposal.IsSuccess
                || baselineProposal.Value is null
                || !TrySnapshotCandidates(
                    baselineProposal.Value,
                    stableEdge.ToNodeIds,
                    out baselineCandidates))
            {
                return BaselineCandidateFailed();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return BaselineCandidateFailed();
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        OperationResult<EdgeAuthorityInput>? baselineMaterialization;
        try
        {
            baselineMaterialization = baselineInputMaterializer.Materialize(
                stableEdge.Copy(),
                CopyTraversalInput(stableInput),
                Array.AsReadOnly(baselineCandidates.Select(CopyCandidate).ToArray()));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return BaselineMaterializationFailed();
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        EdgeAuthorityInput baselineInput;
        try
        {
            if (baselineMaterialization is null
                || !baselineMaterialization.IsSuccess
                || baselineMaterialization.Value is null
                || !TrySnapshotBaselineInput(
                    baselineMaterialization.Value,
                    baselineCandidates,
                    out baselineInput))
            {
                return BaselineMaterializationFailed();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return BaselineMaterializationFailed();
        }

        var subjectInput = subject.NormalizedInput;
        if (!InputsEqual(subjectInput, baselineInput))
        {
            return Failure(
                "Learned subject and independent baseline inputs do not match.",
                InputMismatchCode);
        }

        var orderedBaselineTargetIds = baselineInput.Candidates
            .OrderByDescending(candidate => candidate.BaselineScore)
            .ThenBy(candidate => candidate.TargetSubPhaseId, StringComparer.Ordinal)
            .Select(candidate => candidate.TargetSubPhaseId)
            .ToArray();

        if (!subject.Decision.AblationBaselineRank.SequenceEqual(
                orderedBaselineTargetIds,
                StringComparer.Ordinal))
        {
            return Failure(
                "Learned subject ablation baseline rank does not match the independent baseline.",
                BaselineRankMismatchCode);
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        var producedResult = new DynamicGraphEdgeAuthorityAblationProbeResult(
            subject,
            baselineInput,
            Array.AsReadOnly(orderedBaselineTargetIds.ToArray()),
            stableEdge.AuthorityUnitId);
        if (!TrySnapshotValidatedResult(
                producedResult,
                stableEdge,
                stableInput,
                out var validatedResult))
        {
            return Failure(
                "Learned edge authority subject result is invalid.",
                SubjectResultInvalidCode);
        }

        return OperationResult<DynamicGraphEdgeAuthorityAblationProbeResult>.Ok(validatedResult);
    }

    /// <summary>
    /// The single owned 20AC result boundary. Both the concrete probe and consumers of the probe
    /// validate and snapshot through this method, so a consumer cannot accept a weaker carrier.
    /// It validates only the existing probe/materializer contract and adds no semantic derivation.
    /// </summary>
    internal static bool TrySnapshotValidatedResult(
        DynamicGraphEdgeAuthorityAblationProbeResult? source,
        IDynamicGraphEdge? edge,
        EdgeTraversalInput? traversal,
        out DynamicGraphEdgeAuthorityAblationProbeResult snapshot)
    {
        snapshot = null!;
        if (source is null
            || edge is null
            || traversal is null
            || !TrySnapshotEdge(edge, out var stableEdge)
            || !DynamicGraphEdgeAuthorityExecutionPipeline.TrySnapshotTraversalInput(
                traversal,
                out var stableTraversal)
            || !IsValidTraversalCorrelation(stableEdge, stableTraversal))
        {
            return false;
        }

        var subjectSource = source.Subject;
        var baselineSource = source.BaselineInput;
        var baselineOrderSource = source.OrderedBaselineTargetIds;
        if (!TrySnapshotSubject(
                subjectSource,
                stableEdge,
                stableTraversal,
                out var subject)
            || !TrySnapshotBaselineInput(
                baselineSource,
                baselineSource.Candidates,
                out var baselineInput)
            || !InputsEqual(subject.NormalizedInput, baselineInput)
            || baselineOrderSource is null
            || !StringComparer.Ordinal.Equals(
                source.AuthorityUnitExcludedFromDirectBaselineBranchId,
                stableEdge.AuthorityUnitId)
            || source.ProbeDirectlyInvokedLearnedAuthorityForBaseline)
        {
            return false;
        }

        var expectedBaselineOrder = baselineInput.Candidates
            .OrderByDescending(candidate => candidate.BaselineScore)
            .ThenBy(candidate => candidate.TargetSubPhaseId, StringComparer.Ordinal)
            .Select(candidate => candidate.TargetSubPhaseId)
            .ToArray();
        var suppliedBaselineOrder = baselineOrderSource.ToArray();
        if (!suppliedBaselineOrder.SequenceEqual(expectedBaselineOrder, StringComparer.Ordinal)
            || !subject.Decision.AblationBaselineRank.SequenceEqual(
                expectedBaselineOrder,
                StringComparer.Ordinal))
        {
            return false;
        }

        snapshot = new DynamicGraphEdgeAuthorityAblationProbeResult(
            subject,
            baselineInput,
            Array.AsReadOnly(expectedBaselineOrder.ToArray()),
            stableEdge.AuthorityUnitId);
        return true;
    }

    private static bool TrySnapshotSubject(
        EdgeAuthorityExecutionResult source,
        EdgeSnapshot edge,
        EdgeTraversalInput stableTraversal,
        out EdgeAuthorityExecutionResult snapshot)
    {
        snapshot = null!;
        var proposedSource = source.ProposedCandidates;
        var normalizedSource = source.NormalizedInput;
        var decisionSource = source.Decision;
        var authorityEvidence = source.AuthorityEvidence;
        var orderReport = source.CandidateOrderLeakageReport;
        var identityReport = source.IdentityLeakageReport;
        var unitConfidence = source.UnitConfidence;
        var evidenceSource = source.EvidenceIds;
        var warningSource = source.Warnings;

        if (proposedSource is null
            || normalizedSource is null
            || decisionSource is null
            || authorityEvidence is null
            || orderReport is null
            || identityReport is null
            || evidenceSource is null
            || warningSource is null
            || !double.IsFinite(unitConfidence)
            || unitConfidence is < 0d or > 1d
            || !TrySnapshotCandidates(proposedSource, edge.ToNodeIds, out var proposed)
            || !DynamicGraphEdgeAuthorityExecutionPipeline.TrySnapshotNormalizedInput(
                normalizedSource,
                edge.EdgeId,
                proposed,
                out var normalized)
            || !CandidateListsEqual(proposed, normalized.Candidates)
            || !TrySnapshotSubjectDecision(
                decisionSource,
                edge.ToNodeIds,
                normalized.Candidates,
                out var decision)
            || !AuthorityEvidenceMatches(authorityEvidence, edge)
            || !StringComparer.Ordinal.Equals(orderReport.EdgeId, edge.EdgeId)
            || !StringComparer.Ordinal.Equals(identityReport.EdgeId, edge.EdgeId)
            || !StringComparer.Ordinal.Equals(orderReport.ResolvedAuthorityUnitId, edge.AuthorityUnitId)
            || !StringComparer.Ordinal.Equals(identityReport.ResolvedAuthorityUnitId, edge.AuthorityUnitId)
            || !ReportsMatch(
                orderReport,
                identityReport,
                normalized,
                stableTraversal,
                decision))
        {
            return false;
        }

        var evidenceIds = evidenceSource.ToArray();
        var warnings = warningSource.ToArray();
        if (evidenceIds.Any(value => value is null || ContainsLineBreak(value))
            || warnings.Any(value => value is null || ContainsLineBreak(value)))
        {
            return false;
        }

        snapshot = new EdgeAuthorityExecutionResult(
            normalized,
            Array.AsReadOnly(proposed.Select(CopyCandidate).ToArray()),
            decision,
            DynamicGraphEdgeAuthorityContractSnapshots.CopyAuthorityEvidence(authorityEvidence),
            DynamicGraphEdgeAuthorityContractSnapshots.CopyReport(orderReport),
            DynamicGraphEdgeAuthorityContractSnapshots.CopyIdentityReport(identityReport),
            unitConfidence,
            Array.AsReadOnly(evidenceIds.ToArray()),
            Array.AsReadOnly(warnings.ToArray()));
        return true;
    }

    private static bool TrySnapshotSubjectDecision(
        EdgeAuthorityDecision source,
        IReadOnlyList<string> edgeTargets,
        IReadOnlyList<EdgeCandidate> candidates,
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
        var baselineRank = source.AblationBaselineRank.ToArray();
        var edgeTargetSet = edgeTargets.ToHashSet(StringComparer.Ordinal);
        var candidateIds = candidates
            .Select(candidate => candidate.TargetSubPhaseId)
            .ToHashSet(StringComparer.Ordinal);
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

            if (index > 0)
            {
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
        }

        if (source.FanOutCount != orderedTargets.Length
            || source.FanOutCount > candidates.Count
            || source.FanOutCount > edgeTargetSet.Count
            || (source.KnowledgeEdge && orderedTargets.Length != 0)
            || (!source.KnowledgeEdge && orderedTargets.Length == 0)
            || baselineRank.Length != candidates.Count
            || baselineRank.Any(value => !IsCanonicalValue(value))
            || baselineRank.Distinct(StringComparer.Ordinal).Count() != baselineRank.Length
            || !baselineRank.ToHashSet(StringComparer.Ordinal).SetEquals(candidateIds))
        {
            return false;
        }

        snapshot = new EdgeAuthorityDecision(
            Array.AsReadOnly(orderedTargets.ToArray()),
            source.FanOutCount,
            source.KnowledgeEdge,
            ExpectedAuthoritySource,
            Array.AsReadOnly(baselineRank.ToArray()));
        return true;
    }

    private static bool ReportsMatch(
        DynamicGraphEdgeCandidateOrderLeakageReport orderReport,
        DynamicGraphEdgeIdentityLeakageReport identityReport,
        EdgeAuthorityInput normalizedInput,
        EdgeTraversalInput stableTraversal,
        EdgeAuthorityDecision decision)
    {
        if (!orderReport.IsOrderInvariant
            || !identityReport.IsIdentityInvariant
            || !DynamicGraphEdgeAuthorityLeakageBoundary.IsExact(
                orderReport.ExcludedLeakageSignals)
            || !DynamicGraphEdgeAuthorityLeakageBoundary.IsExact(
                identityReport.ExcludedLeakageSignals)
            || orderReport.Probes.Count != 5
            || identityReport.Probes.Count != 3)
        {
            return false;
        }

        for (var index = 0; index < orderReport.Probes.Count; index++)
        {
            var probe = orderReport.Probes[index];
            var expectedOrder = DynamicGraphEdgeCandidateOrderLeakageGuardUnit
                .CreateExpectedPermutation(normalizedInput.Candidates, index);
            if (probe is null
                || !StringComparer.Ordinal.Equals(
                    probe.ProbeId,
                    DynamicGraphEdgeCandidateOrderLeakageGuardUnit.CreateProbeId(index))
                || !CandidateListsEqual(probe.CandidateOrder, expectedOrder)
                || !DynamicGraphEdgeCandidateOrderLeakageGuardUnit.DecisionsEqual(
                    decision,
                    probe.Decision))
            {
                return false;
            }
        }

        for (var index = 0; index < identityReport.Probes.Count; index++)
        {
            var probe = identityReport.Probes[index];
            var expectedControlValue =
                DynamicGraphEdgeIdentityLeakageGuardUnit.CreateControlValue(index);
            var originalValue =
                DynamicGraphEdgeIdentityLeakageGuardUnit.GetOriginalValue(stableTraversal, index);
            if (probe is null
                || !StringComparer.Ordinal.Equals(
                    probe.ProbeId,
                    DynamicGraphEdgeIdentityLeakageGuardUnit.CreateProbeId(index))
                || !StringComparer.Ordinal.Equals(
                    probe.ControlledSignal,
                    DynamicGraphEdgeIdentityLeakageGuardUnit.CreateControlledSignal(index))
                || !StringComparer.Ordinal.Equals(
                    probe.ControlValue,
                    expectedControlValue)
                || !IsCanonicalValue(probe.ControlValue)
                || StringComparer.Ordinal.Equals(probe.ControlValue, originalValue)
                || !DynamicGraphEdgeCandidateOrderLeakageGuardUnit.DecisionsEqual(
                    decision,
                    probe.Decision))
            {
                return false;
            }
        }

        return true;
    }

    private static bool TrySnapshotBaselineInput(
        EdgeAuthorityInput source,
        IReadOnlyList<EdgeCandidate> proposedCandidates,
        out EdgeAuthorityInput snapshot)
    {
        snapshot = null!;
        var raw = CopyInput(source);
        if (!DynamicGraphEdgeAuthorityExecutionPipeline.TrySnapshotNormalizedInput(
                raw,
                raw.EdgeId,
                proposedCandidates,
                out var normalized)
            || !InputsEqual(raw, normalized)
            || !CandidateListsEqual(proposedCandidates, normalized.Candidates))
        {
            return false;
        }

        snapshot = normalized;
        return true;
    }

    private static bool TrySnapshotCandidates(
        IReadOnlyList<EdgeCandidate> source,
        IReadOnlyList<string> allowedTargetSource,
        out EdgeCandidate[] snapshot)
    {
        snapshot = [];
        var candidates = source.ToArray();
        if (candidates.Length == 0)
            return false;

        var allowedTargets = allowedTargetSource.ToHashSet(StringComparer.Ordinal);
        var seenTargets = new HashSet<string>(StringComparer.Ordinal);
        var output = new EdgeCandidate[candidates.Length];
        for (var index = 0; index < candidates.Length; index++)
        {
            var candidate = candidates[index];
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

            var evidence = candidate.EvidencePredicates.ToArray();
            if (evidence.Any(value => string.IsNullOrWhiteSpace(value)
                                      || ContainsLineBreak(value)
                                      || !StringComparer.Ordinal.Equals(
                                          value,
                                          value.Trim().ToUpperInvariant()))
                || evidence.Distinct(StringComparer.Ordinal).Count() != evidence.Length
                || !evidence.SequenceEqual(
                    evidence.OrderBy(value => value, StringComparer.Ordinal),
                    StringComparer.Ordinal))
            {
                return false;
            }

            output[index] = new EdgeCandidate(
                candidate.TargetSubPhaseId,
                evidence,
                candidate.BaselineScore,
                ExpectedCandidateRole);
        }

        snapshot = output;
        return true;
    }

    private static bool InputsEqual(EdgeAuthorityInput left, EdgeAuthorityInput right)
        => StringComparer.Ordinal.Equals(left.EdgeId, right.EdgeId)
           && left.SemanticTokens.SequenceEqual(right.SemanticTokens, StringComparer.Ordinal)
           && left.PredicateIds.SequenceEqual(right.PredicateIds, StringComparer.Ordinal)
           && ContextsEqual(left.Context, right.Context)
           && CandidateListsEqual(left.Candidates, right.Candidates)
           && left.ExcludedLeakageSignals.SequenceEqual(
               right.ExcludedLeakageSignals,
               StringComparer.Ordinal);

    private static bool ContextsEqual(ContextSignals left, ContextSignals right)
    {
        if (!left.PriorDomains.SequenceEqual(right.PriorDomains)
            || !left.CompletedSubPhases.SequenceEqual(
                right.CompletedSubPhases,
                StringComparer.Ordinal)
            || !left.ActiveKnowledgeEdges.SequenceEqual(
                right.ActiveKnowledgeEdges,
                StringComparer.Ordinal)
            || left.PriorConfidenceMap.Count != right.PriorConfidenceMap.Count)
        {
            return false;
        }

        foreach (var pair in left.PriorConfidenceMap)
        {
            if (!right.PriorConfidenceMap.TryGetValue(pair.Key, out var rightValue)
                || !pair.Value.Equals(rightValue))
            {
                return false;
            }
        }

        return true;
    }

    private static bool CandidateListsEqual(
        IReadOnlyList<EdgeCandidate> left,
        IReadOnlyList<EdgeCandidate> right)
    {
        if (left.Count != right.Count)
            return false;

        for (var index = 0; index < left.Count; index++)
        {
            var leftCandidate = left[index];
            var rightCandidate = right[index];
            if (leftCandidate is null
                || rightCandidate is null
                || !StringComparer.Ordinal.Equals(
                    leftCandidate.TargetSubPhaseId,
                    rightCandidate.TargetSubPhaseId)
                || !leftCandidate.BaselineScore.Equals(rightCandidate.BaselineScore)
                || !StringComparer.Ordinal.Equals(
                    leftCandidate.AuthorityRole,
                    rightCandidate.AuthorityRole)
                || !leftCandidate.EvidencePredicates.SequenceEqual(
                    rightCandidate.EvidencePredicates,
                    StringComparer.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static bool AuthorityEvidenceMatches(
        DynamicGraphEdgeAuthorityEvidence evidence,
        EdgeSnapshot edge)
        => StringComparer.Ordinal.Equals(evidence.EdgeId, edge.EdgeId)
           && StringComparer.Ordinal.Equals(evidence.AuthorityUnitId, edge.AuthorityUnitId)
           && StringComparer.Ordinal.Equals(evidence.AuthorityUnitLevel, edge.AuthorityUnitLevel)
           && evidence.DeterministicFinalAuthority == edge.DeterministicFinalAuthority
           && StringComparer.Ordinal.Equals(evidence.BaselineUnitId, edge.BaselineUnitId)
           && StringComparer.Ordinal.Equals(evidence.BaselineRole, edge.BaselineRole)
           && evidence.ExcludedLeakageSignals.SequenceEqual(
               edge.ExcludedLeakageSignals,
               StringComparer.Ordinal)
           && StringComparer.Ordinal.Equals(evidence.SourceProof, edge.SourceProof)
           && StringComparer.Ordinal.Equals(evidence.CutoverProof, edge.CutoverProof)
           && StringComparer.Ordinal.Equals(evidence.AblationProofRef, edge.AblationProofRef);

    private static bool TrySnapshotEdge(IDynamicGraphEdge source, out EdgeSnapshot snapshot)
    {
        snapshot = null!;
        var edgeId = source.EdgeId;
        var fromNodeId = source.FromNodeId;
        var targetSource = source.ToNodeIds;
        var kind = source.Kind;
        var parallelEligible = source.ParallelEligible;
        var authorityUnitId = source.AuthorityUnitId;
        var authorityUnitLevel = source.AuthorityUnitLevel;
        var deterministicFinalAuthority = source.DeterministicFinalAuthority;
        var baselineUnitId = source.BaselineUnitId;
        var baselineRole = source.BaselineRole;
        var excludedSource = source.ExcludedLeakageSignals;
        var sourceProof = source.SourceProof;
        var cutoverProof = source.CutoverProof;
        var ablationProofRef = source.AblationProofRef;

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
            || excludedSource is null)
        {
            return false;
        }

        var targets = targetSource.ToArray();
        var excluded = excludedSource.ToArray();
        if (targets.Length == 0
            || targets.Any(target => !IsCanonicalValue(target))
            || targets.Distinct(StringComparer.Ordinal).Count() != targets.Length
            || targets.Contains(fromNodeId, StringComparer.Ordinal)
            || (kind == GraphEdgeKind.ManyToMany && targets.Length < 2)
            || kind is not GraphEdgeKind.FanOut
                and not GraphEdgeKind.ManyToMany
                and not GraphEdgeKind.MultiOutput
            || !DynamicGraphEdgeAuthorityLeakageBoundary.IsExact(excluded))
        {
            return false;
        }

        snapshot = new EdgeSnapshot(
            edgeId!,
            fromNodeId!,
            targets,
            kind,
            parallelEligible,
            authorityUnitId!,
            authorityUnitLevel,
            deterministicFinalAuthority,
            baselineUnitId!,
            baselineRole!,
            excluded,
            sourceProof!,
            cutoverProof!,
            ablationProofRef!);
        return true;
    }

    private static bool IsValidTraversalCorrelation(
        EdgeSnapshot edge,
        EdgeTraversalInput input)
        => IsCanonicalValue(input.RunId)
           && StringComparer.Ordinal.Equals(edge.EdgeId, input.EdgeId)
           && StringComparer.Ordinal.Equals(edge.FromNodeId, input.UpstreamNodeOutput.SourceNodeId)
           && IsValidUpstreamProvenance(input.UpstreamNodeOutput);

    private static bool IsValidUpstreamProvenance(NodeOutputToEdgeInputAdapter source)
    {
        if (source.UpstreamCandidateRefs is not { Count: > 0 } references
            || !IsCanonicalValue(source.UpstreamOutputHash)
            || source.UpstreamOutputHash.Any(char.IsControl)
            || !StringComparer.Ordinal.Equals(
                source.ProjectionRule,
                ExpectedUpstreamProjectionRule))
        {
            return false;
        }

        string? previous = null;
        foreach (var reference in references)
        {
            if (!IsCanonicalValue(reference)
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

    private static EdgeTraversalInput CopyTraversalInput(EdgeTraversalInput source)
        => DynamicGraphEdgeAuthorityContractSnapshots.CopyTraversalInput(source);

    private static EdgeAuthorityInput CopyInput(EdgeAuthorityInput source)
        => DynamicGraphEdgeAuthorityContractSnapshots.CopyInput(source);

    private static EdgeCandidate CopyCandidate(EdgeCandidate source)
        => DynamicGraphEdgeAuthorityContractSnapshots.CopyCandidate(source);

    private static bool IsCanonicalValue(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && StringComparer.Ordinal.Equals(value, value.Trim())
           && !ContainsLineBreak(value);

    private static bool ContainsLineBreak(string value)
        => value.IndexOfAny(['\r', '\n', '\u0085', '\u2028', '\u2029']) >= 0;

    private static OperationResult<DynamicGraphEdgeAuthorityAblationProbeResult> Cancelled()
        => Failure("Cancelled.", CancelledCode);

    private static OperationResult<DynamicGraphEdgeAuthorityAblationProbeResult>
        BaselineCandidateFailed()
        => Failure(
            "Independent baseline candidate proposal failed.",
            BaselineCandidateFailedCode);

    private static OperationResult<DynamicGraphEdgeAuthorityAblationProbeResult>
        BaselineMaterializationFailed()
        => Failure(
            "Independent baseline input materialization failed.",
            BaselineMaterializationFailedCode);

    private static OperationResult<DynamicGraphEdgeAuthorityAblationProbeResult> Failure(
        string message,
        string code)
        => OperationResult<DynamicGraphEdgeAuthorityAblationProbeResult>.Fail(message, code);

    private sealed class EdgeSnapshot : IDynamicGraphEdge
    {
        private readonly string[] _toNodeIds;
        private readonly string[] _excludedLeakageSignals;

        internal EdgeSnapshot(
            string edgeId,
            string fromNodeId,
            IReadOnlyList<string> toNodeIds,
            GraphEdgeKind kind,
            bool parallelEligible,
            string authorityUnitId,
            string authorityUnitLevel,
            bool deterministicFinalAuthority,
            string baselineUnitId,
            string baselineRole,
            IReadOnlyList<string> excludedLeakageSignals,
            string sourceProof,
            string cutoverProof,
            string ablationProofRef)
        {
            EdgeId = edgeId;
            FromNodeId = fromNodeId;
            _toNodeIds = toNodeIds.ToArray();
            Kind = kind;
            ParallelEligible = parallelEligible;
            AuthorityUnitId = authorityUnitId;
            AuthorityUnitLevel = authorityUnitLevel;
            DeterministicFinalAuthority = deterministicFinalAuthority;
            BaselineUnitId = baselineUnitId;
            BaselineRole = baselineRole;
            _excludedLeakageSignals = excludedLeakageSignals.ToArray();
            SourceProof = sourceProof;
            CutoverProof = cutoverProof;
            AblationProofRef = ablationProofRef;
        }

        public string EdgeId { get; }

        public string FromNodeId { get; }

        public IReadOnlyList<string> ToNodeIds => Array.AsReadOnly(_toNodeIds.ToArray());

        public GraphEdgeKind Kind { get; }

        public bool ParallelEligible { get; }

        public string AuthorityUnitId { get; }

        public string AuthorityUnitLevel { get; }

        public bool DeterministicFinalAuthority { get; }

        public string? BaselineUnitId { get; }

        public string BaselineRole { get; }

        public IReadOnlyList<string> ExcludedLeakageSignals
            => Array.AsReadOnly(_excludedLeakageSignals.ToArray());

        public string SourceProof { get; }

        public string CutoverProof { get; }

        public string AblationProofRef { get; }

        internal EdgeSnapshot Copy()
            => new(
                EdgeId,
                FromNodeId,
                _toNodeIds,
                Kind,
                ParallelEligible,
                AuthorityUnitId,
                AuthorityUnitLevel,
                DeterministicFinalAuthority,
                BaselineUnitId!,
                BaselineRole,
                _excludedLeakageSignals,
                SourceProof,
                CutoverProof,
                AblationProofRef);
    }
}
