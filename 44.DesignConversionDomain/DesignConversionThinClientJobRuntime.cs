// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LlmFromScratch.Tests")]

namespace DesignConversionDomain;

using System.Collections.Immutable;
using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public interface IDesignConversionThinClientJobEvaluator
{
    Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
        DesignConversionInternalEvaluateRequest request,
        CancellationToken cancellationToken = default);
}

internal interface IDesignConversionRepositoryOwnedDpoTransactionContextAwareEvaluator
{
    /// <summary>
    /// Accepts the request-owned DPO transaction context at the runtime bridge so
    /// the live submit path can enforce repository-owned reuse preflight before
    /// teacher inference without changing the older internal evaluation contract.
    /// </summary>
    Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
        DesignConversionInternalEvaluateRequest request,
        DesignConversionRepositoryOwnedDpoTransactionContext?
            repositoryOwnedDpoTransactionContext,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Opaque logical identity for a chosen runtime evaluation.
/// This boundary deliberately excludes the evaluation payload, render data, provider data, and provenance.
/// </summary>
public sealed record DesignConversionRuntimeRejectedEvaluationLookup(string ChosenJobId);

/// <summary>
/// Opaque reference to a separately measured rejected evaluation.
/// A public caller can create a reference, but only an internal trusted factory can hydrate one.
/// </summary>
public sealed class DesignConversionRuntimeRejectedEvaluationReference
{
    private readonly DesignConversionInternalEvaluationResult? _trustedHydratedEvaluation;
    private readonly string? _trustedChosenEvaluationJobId;
    private readonly DesignConversionInternalEvaluationResult? _trustedChosenEvaluation;
    private readonly DesignConversionDpoTeacherCandidateAuthoringEvidence?
        _trustedTeacherCandidateAuthoringEvidence;
    private readonly IReadOnlyList<DesignConversionTeacherAuthoredEvaluationPair>
        _trustedTeacherAuthoredEvaluationPairs;
    private readonly IReadOnlyList<DesignConversionIndependentPreferenceEvidence>
        _trustedCoordinatorConsumedPreferenceEvidence;
    private readonly IReadOnlyList<string> _trustedCoordinatorPreferenceWarnings;
    private readonly bool _coordinatorOwnedPreferenceEvaluationHandled;

    public DesignConversionRuntimeRejectedEvaluationReference(string evaluationRef)
    {
        EvaluationRef = evaluationRef ?? string.Empty;
        _trustedTeacherAuthoredEvaluationPairs = [];
        _trustedCoordinatorConsumedPreferenceEvidence = [];
        _trustedCoordinatorPreferenceWarnings = [];
    }

    private DesignConversionRuntimeRejectedEvaluationReference(
        string evaluationRef,
        DesignConversionInternalEvaluationResult trustedHydratedEvaluation,
        string? trustedChosenEvaluationJobId,
        DesignConversionInternalEvaluationResult? trustedChosenEvaluation,
        DesignConversionDpoTeacherCandidateAuthoringEvidence?
            trustedTeacherCandidateAuthoringEvidence,
        IReadOnlyList<DesignConversionTeacherAuthoredEvaluationPair>?
            trustedTeacherAuthoredEvaluationPairs,
        IReadOnlyList<DesignConversionIndependentPreferenceEvidence>?
            trustedCoordinatorConsumedPreferenceEvidence,
        IReadOnlyList<string>? trustedCoordinatorPreferenceWarnings)
        : this(evaluationRef)
    {
        _trustedHydratedEvaluation = trustedHydratedEvaluation ??
            throw new ArgumentNullException(nameof(trustedHydratedEvaluation));
        _trustedChosenEvaluationJobId = string.IsNullOrWhiteSpace(trustedChosenEvaluationJobId)
            ? null
            : trustedChosenEvaluationJobId;
        _trustedChosenEvaluation = trustedChosenEvaluation;
        _trustedTeacherCandidateAuthoringEvidence =
            trustedTeacherCandidateAuthoringEvidence;
        _trustedTeacherAuthoredEvaluationPairs =
            trustedTeacherAuthoredEvaluationPairs?.ToImmutableArray() ?? [];
        _coordinatorOwnedPreferenceEvaluationHandled =
            trustedCoordinatorConsumedPreferenceEvidence is not null;
        _trustedCoordinatorConsumedPreferenceEvidence =
            trustedCoordinatorConsumedPreferenceEvidence?.ToImmutableArray() ?? [];
        _trustedCoordinatorPreferenceWarnings =
            trustedCoordinatorPreferenceWarnings?.ToImmutableArray() ?? [];
    }

    public string EvaluationRef { get; }

    internal static DesignConversionRuntimeRejectedEvaluationReference CreateInternallyHydrated(
        string evaluationRef,
        DesignConversionInternalEvaluationResult trustedHydratedEvaluation,
        string? trustedChosenEvaluationJobId = null,
        DesignConversionInternalEvaluationResult? trustedChosenEvaluation = null,
        DesignConversionDpoTeacherCandidateAuthoringEvidence?
            trustedTeacherCandidateAuthoringEvidence = null,
        IReadOnlyList<DesignConversionTeacherAuthoredEvaluationPair>?
            trustedTeacherAuthoredEvaluationPairs = null,
        IReadOnlyList<DesignConversionIndependentPreferenceEvidence>?
            trustedCoordinatorConsumedPreferenceEvidence = null,
        IReadOnlyList<string>? trustedCoordinatorPreferenceWarnings = null)
        => string.IsNullOrWhiteSpace(evaluationRef)
            ? throw new ArgumentException("An opaque rejected-evaluation reference is required.", nameof(evaluationRef))
            : new DesignConversionRuntimeRejectedEvaluationReference(
                evaluationRef,
                trustedHydratedEvaluation,
                trustedChosenEvaluationJobId,
                trustedChosenEvaluation,
                trustedTeacherCandidateAuthoringEvidence,
                trustedTeacherAuthoredEvaluationPairs,
                trustedCoordinatorConsumedPreferenceEvidence,
                trustedCoordinatorPreferenceWarnings);

    internal bool TryGetTrustedHydratedEvaluation(
        out DesignConversionInternalEvaluationResult? trustedHydratedEvaluation)
    {
        trustedHydratedEvaluation = _trustedHydratedEvaluation;
        return !string.IsNullOrWhiteSpace(EvaluationRef) &&
               trustedHydratedEvaluation is not null &&
               !string.IsNullOrWhiteSpace(trustedHydratedEvaluation.JobId) &&
               string.Equals(
                   EvaluationRef,
                   trustedHydratedEvaluation.JobId,
                   StringComparison.Ordinal);
    }

    internal bool TryGetTrustedChosenEvaluationJobId(out string? trustedChosenEvaluationJobId)
    {
        trustedChosenEvaluationJobId = _trustedChosenEvaluationJobId;
        return !string.IsNullOrWhiteSpace(trustedChosenEvaluationJobId);
    }

    internal bool TryGetTrustedChosenEvaluation(
        out DesignConversionInternalEvaluationResult? trustedChosenEvaluation)
    {
        trustedChosenEvaluation = _trustedChosenEvaluation;
        return trustedChosenEvaluation is not null &&
               !string.IsNullOrWhiteSpace(trustedChosenEvaluation.JobId);
    }

    internal bool TryGetTrustedTeacherCandidateAuthoringEvidence(
        out DesignConversionDpoTeacherCandidateAuthoringEvidence?
            trustedTeacherCandidateAuthoringEvidence)
    {
        trustedTeacherCandidateAuthoringEvidence =
            _trustedTeacherCandidateAuthoringEvidence;
        return trustedTeacherCandidateAuthoringEvidence is not null;
    }

    internal bool TryGetTrustedTeacherAuthoredEvaluationPairs(
        out IReadOnlyList<DesignConversionTeacherAuthoredEvaluationPair>
            evaluationPairs)
    {
        evaluationPairs = _trustedTeacherAuthoredEvaluationPairs;
        return evaluationPairs.Count > 0;
    }

    internal bool TryGetTrustedCoordinatorConsumedPreferenceEvidence(
        out IReadOnlyList<DesignConversionIndependentPreferenceEvidence>
            preferenceEvidence)
    {
        preferenceEvidence = _trustedCoordinatorConsumedPreferenceEvidence;
        return _coordinatorOwnedPreferenceEvaluationHandled;
    }

    internal bool TryGetTrustedCoordinatorPreferenceWarnings(
        out IReadOnlyList<string> warnings)
    {
        warnings = _trustedCoordinatorPreferenceWarnings;
        return _coordinatorOwnedPreferenceEvaluationHandled;
    }
}

/// <summary>
/// Resolves an opaque reference from trusted internal evaluation history.
/// Implementations receive only the chosen logical identifier and must not receive raw evaluation payloads.
/// </summary>
public interface IDesignConversionRuntimeRejectedEvaluationSource
{
    Task<OperationResult<DesignConversionRuntimeRejectedEvaluationReference?>>
        ResolveMeasuredRejectedEvaluationAsync(
            DesignConversionRuntimeRejectedEvaluationLookup chosenEvaluation,
            CancellationToken cancellationToken = default);
}

/// <summary>
/// Internal-only recorder used by the coordinator-owned evaluation path to hand a
/// separately measured rejected result to the immediately following runtime
/// persistence step. The public source contract remains lookup-only and opaque.
/// </summary>
internal interface IDesignConversionRuntimeRejectedEvaluationHandoff :
    IDesignConversionRuntimeRejectedEvaluationSource
{
    bool TryRecord(
        DesignConversionInternalEvaluateRequest rootRequest,
        DesignConversionInternalEvaluationResult chosenEvaluation,
        DesignConversionInternalEvaluationResult rejectedEvaluation);

    void Discard(string rootJobId);

    int PendingCount { get; }
}

internal interface IDesignConversionTeacherCandidateAuthoringHandoff
{
    bool TryRecordTeacherAuthoredCandidate(
        DesignConversionInternalEvaluateRequest rootRequest,
        DesignConversionInternalEvaluationResult generatedEvaluation,
        DesignConversionInternalEvaluationResult incumbentEvaluation,
        DesignConversionDpoTeacherCandidateAuthoringEvidence authoringEvidence);

    bool TryRecordTeacherAuthoredEvaluationBatch(
        DesignConversionInternalEvaluateRequest rootRequest,
        DesignConversionInternalEvaluationResult chosenLookupEvaluation,
        IReadOnlyList<DesignConversionTeacherAuthoredEvaluationPair>
            evaluationPairs,
        IReadOnlyList<DesignConversionIndependentPreferenceEvidence>?
            coordinatorConsumedPreferenceEvidence = null,
        IReadOnlyList<string>? coordinatorPreferenceWarnings = null);
}

/// <summary>
/// Process-local, single-consumption handoff for a trusted measured rejected
/// evaluation. Entries are keyed by the root submit job, never by global recency.
/// </summary>
internal sealed class DesignConversionRuntimeRejectedEvaluationHandoff :
    IDesignConversionRuntimeRejectedEvaluationHandoff,
    IDesignConversionTeacherCandidateAuthoringHandoff
{
    internal const string RootOwnershipBindingProvenanceKey =
        "runtime_rejected_evaluation_root_ownership_binding";
    private const string SubmittedRequestJobIdProvenanceKey = "internal_service_job_id";

    private sealed record PendingRejectedEvaluation(
        string RootJobId,
        string ChosenJobId,
        string RootRequestId,
        DesignConversionInternalEvaluationResult ChosenEvaluation,
        DesignConversionInternalEvaluationResult RejectedEvaluation,
        DesignConversionDpoTeacherCandidateAuthoringEvidence?
            TeacherCandidateAuthoringEvidence,
        IReadOnlyList<DesignConversionTeacherAuthoredEvaluationPair>
            TeacherAuthoredEvaluationPairs,
        IReadOnlyList<DesignConversionIndependentPreferenceEvidence>?
            CoordinatorConsumedPreferenceEvidence,
        IReadOnlyList<string>? CoordinatorPreferenceWarnings);

    private readonly object _gate = new();
    private readonly Dictionary<string, PendingRejectedEvaluation> _pendingByRootJobId =
        new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _rootJobIdByChosenJobId =
        new(StringComparer.Ordinal);

    public int PendingCount
    {
        get
        {
            lock (_gate)
                return _pendingByRootJobId.Count;
        }
    }

    public bool TryRecord(
        DesignConversionInternalEvaluateRequest rootRequest,
        DesignConversionInternalEvaluationResult chosenEvaluation,
        DesignConversionInternalEvaluationResult rejectedEvaluation)
        => TryRecordCore(
            rootRequest,
            chosenEvaluation,
            rejectedEvaluation,
            candidateAuthoringEvidence: null,
            teacherAuthoredEvaluationPairs: [],
            coordinatorConsumedPreferenceEvidence: null,
            coordinatorPreferenceWarnings: null);

    public bool TryRecordTeacherAuthoredCandidate(
        DesignConversionInternalEvaluateRequest rootRequest,
        DesignConversionInternalEvaluationResult generatedEvaluation,
        DesignConversionInternalEvaluationResult incumbentEvaluation,
        DesignConversionDpoTeacherCandidateAuthoringEvidence authoringEvidence)
        => authoringEvidence is not null &&
           TryRecordCore(
               rootRequest,
               generatedEvaluation,
               incumbentEvaluation,
               authoringEvidence,
               [
                   new DesignConversionTeacherAuthoredEvaluationPair(
                       authoringEvidence.AttemptNumber,
                       generatedEvaluation,
                       incumbentEvaluation,
                       authoringEvidence,
                       authoringEvidence.GeneratedCandidateId)
               ],
               coordinatorConsumedPreferenceEvidence: null,
               coordinatorPreferenceWarnings: null);

    public bool TryRecordTeacherAuthoredEvaluationBatch(
        DesignConversionInternalEvaluateRequest rootRequest,
        DesignConversionInternalEvaluationResult chosenLookupEvaluation,
        IReadOnlyList<DesignConversionTeacherAuthoredEvaluationPair>
            evaluationPairs,
        IReadOnlyList<DesignConversionIndependentPreferenceEvidence>?
            coordinatorConsumedPreferenceEvidence = null,
        IReadOnlyList<string>? coordinatorPreferenceWarnings = null)
    {
        if (evaluationPairs is null || evaluationPairs.Count == 0 ||
            evaluationPairs.Count >
                ContinuousLearning.LegalTrainingTeacherAdmissionAuthority.MaxBatchRows ||
            evaluationPairs.Any(static pair => pair is null) ||
            coordinatorConsumedPreferenceEvidence?.Any(
                static evidence => evidence is null) == true ||
            coordinatorConsumedPreferenceEvidence?.Count > evaluationPairs.Count ||
            coordinatorPreferenceWarnings?.Any(
                static warning => string.IsNullOrWhiteSpace(warning)) == true)
            return false;

        var orderedPairs = evaluationPairs
            .OrderBy(static pair => pair.AttemptNumber)
            .ToImmutableArray();
        return TryRecordCore(
            rootRequest,
            chosenLookupEvaluation,
            orderedPairs[0].IncumbentEvaluation,
            candidateAuthoringEvidence: null,
            orderedPairs,
            coordinatorConsumedPreferenceEvidence,
            coordinatorPreferenceWarnings);
    }

    private bool TryRecordCore(
        DesignConversionInternalEvaluateRequest rootRequest,
        DesignConversionInternalEvaluationResult chosenEvaluation,
        DesignConversionInternalEvaluationResult rejectedEvaluation,
        DesignConversionDpoTeacherCandidateAuthoringEvidence?
            candidateAuthoringEvidence,
        IReadOnlyList<DesignConversionTeacherAuthoredEvaluationPair>
            teacherAuthoredEvaluationPairs,
        IReadOnlyList<DesignConversionIndependentPreferenceEvidence>?
            coordinatorConsumedPreferenceEvidence,
        IReadOnlyList<string>? coordinatorPreferenceWarnings)
    {
        if (!IsRecordOwnedByRootRequest(rootRequest, chosenEvaluation) ||
            !IsRecordOwnedByRootRequest(rootRequest, rejectedEvaluation) ||
            string.Equals(chosenEvaluation.JobId, rejectedEvaluation.JobId, StringComparison.Ordinal) ||
            teacherAuthoredEvaluationPairs.Count >
                ContinuousLearning.LegalTrainingTeacherAdmissionAuthority.MaxBatchRows ||
            teacherAuthoredEvaluationPairs.Any(pair =>
                pair is null ||
                !IsRecordOwnedByRootRequest(rootRequest, pair.GeneratedEvaluation) ||
                !IsRecordOwnedByRootRequest(rootRequest, pair.IncumbentEvaluation) ||
                string.Equals(
                    pair.GeneratedEvaluation.JobId,
                    pair.IncumbentEvaluation.JobId,
                    StringComparison.Ordinal)))
        {
            Discard(rootRequest?.JobId ?? string.Empty);
            return false;
        }

        lock (_gate)
        {
            if (_rootJobIdByChosenJobId.TryGetValue(
                    chosenEvaluation.JobId,
                    out var existingRootJobId) &&
                !string.Equals(existingRootJobId, rootRequest.JobId, StringComparison.Ordinal))
            {
                // The chosen identity belongs to another root. Clear only this
                // root's stale state and never disturb the other root's entry.
                RemoveByRootJobIdNoLock(rootRequest.JobId);
                return false;
            }

            // Re-running the same root replaces its stale pending selection.
            RemoveByRootJobIdNoLock(rootRequest.JobId);

            _pendingByRootJobId.Add(
                rootRequest.JobId,
                new PendingRejectedEvaluation(
                    rootRequest.JobId,
                    chosenEvaluation.JobId,
                    rootRequest.RequestId,
                    chosenEvaluation,
                    rejectedEvaluation,
                    candidateAuthoringEvidence,
                    teacherAuthoredEvaluationPairs.ToImmutableArray(),
                    coordinatorConsumedPreferenceEvidence?.ToImmutableArray(),
                    coordinatorPreferenceWarnings?.ToImmutableArray()));
            if (!string.Equals(rootRequest.JobId, chosenEvaluation.JobId, StringComparison.Ordinal))
                _rootJobIdByChosenJobId.Add(chosenEvaluation.JobId, rootRequest.JobId);
            return true;
        }
    }

    public Task<OperationResult<DesignConversionRuntimeRejectedEvaluationReference?>>
        ResolveMeasuredRejectedEvaluationAsync(
            DesignConversionRuntimeRejectedEvaluationLookup chosenEvaluation,
            CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            if (chosenEvaluation is not null &&
                !string.IsNullOrWhiteSpace(chosenEvaluation.ChosenJobId))
            {
                DiscardByChosenJobId(chosenEvaluation.ChosenJobId);
            }

            return Task.FromResult(
                OperationResult<DesignConversionRuntimeRejectedEvaluationReference?>.Fail(
                    "DesignConversion rejected-evaluation lookup cancelled.",
                    "CANCELLED"));
        }

        if (chosenEvaluation is null || string.IsNullOrWhiteSpace(chosenEvaluation.ChosenJobId))
        {
            return Task.FromResult(
                OperationResult<DesignConversionRuntimeRejectedEvaluationReference?>.Ok(null));
        }

        var pending = RemoveByChosenJobId(chosenEvaluation.ChosenJobId);
        if (pending is null)
        {
            return Task.FromResult(
                OperationResult<DesignConversionRuntimeRejectedEvaluationReference?>.Ok(null));
        }

        if (cancellationToken.IsCancellationRequested ||
            pending is null ||
            string.IsNullOrWhiteSpace(pending.RootRequestId) ||
            string.IsNullOrWhiteSpace(pending.RejectedEvaluation.JobId))
        {
            return Task.FromResult(
                OperationResult<DesignConversionRuntimeRejectedEvaluationReference?>.Ok(null));
        }

        return Task.FromResult(
            OperationResult<DesignConversionRuntimeRejectedEvaluationReference?>.Ok(
                DesignConversionRuntimeRejectedEvaluationReference.CreateInternallyHydrated(
                    pending.RejectedEvaluation.JobId,
                    pending.RejectedEvaluation,
                    pending.ChosenJobId,
                    pending.ChosenEvaluation,
                    pending.TeacherCandidateAuthoringEvidence,
                    pending.TeacherAuthoredEvaluationPairs,
                    pending.CoordinatorConsumedPreferenceEvidence,
                    pending.CoordinatorPreferenceWarnings)));
    }

    public void Discard(string rootJobId)
    {
        if (string.IsNullOrWhiteSpace(rootJobId))
            return;

        lock (_gate)
        {
            RemoveByRootJobIdNoLock(rootJobId);
        }
    }

    private PendingRejectedEvaluation? RemoveByChosenJobId(string chosenJobId)
    {
        lock (_gate)
        {
            var rootJobId = _pendingByRootJobId.ContainsKey(chosenJobId)
                ? chosenJobId
                : _rootJobIdByChosenJobId.TryGetValue(chosenJobId, out var mappedRootJobId)
                    ? mappedRootJobId
                    : string.Empty;
            if (string.IsNullOrWhiteSpace(rootJobId) ||
                !_pendingByRootJobId.Remove(rootJobId, out var pending))
            {
                return null;
            }

            if (!string.Equals(pending.ChosenJobId, pending.RootJobId, StringComparison.Ordinal))
                _rootJobIdByChosenJobId.Remove(pending.ChosenJobId);
            return pending;
        }
    }

    private void DiscardByChosenJobId(string chosenJobId)
        => RemoveByChosenJobId(chosenJobId);

    internal static DesignConversionInternalEvaluateRequest BindToRootOwnership(
        DesignConversionInternalEvaluateRequest rootRequest)
    {
        ArgumentNullException.ThrowIfNull(rootRequest);
        var binding = BuildRootOwnershipBinding(rootRequest.JobId, rootRequest.RequestId);
        return rootRequest with
        {
            Provenance = (rootRequest.Provenance ?? new Dictionary<string, string>())
                .Concat(new Dictionary<string, string>
                {
                    [RootOwnershipBindingProvenanceKey] = binding
                })
                .GroupBy(pair => pair.Key, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal)
        };
    }

    private static string BuildRootOwnershipBinding(string rootJobId, string rootRequestId)
        => DesignConversionDpoLeakageGuard.HashRef(
            "dcm-runtime-rejected-root-ownership",
            string.Concat(rootJobId, "|", rootRequestId));

    private void RemoveByRootJobIdNoLock(string rootJobId)
    {
        if (_pendingByRootJobId.Remove(rootJobId, out var pending) &&
            !string.Equals(pending.ChosenJobId, rootJobId, StringComparison.Ordinal))
        {
            _rootJobIdByChosenJobId.Remove(pending.ChosenJobId);
        }
    }

    private static bool IsRecordOwnedByRootRequest(
        DesignConversionInternalEvaluateRequest rootRequest,
        DesignConversionInternalEvaluationResult evaluation)
    {
        if (rootRequest is null ||
            evaluation is null ||
            string.IsNullOrWhiteSpace(rootRequest.JobId) ||
            string.IsNullOrWhiteSpace(rootRequest.RequestId) ||
            string.IsNullOrWhiteSpace(evaluation.JobId) ||
            string.IsNullOrWhiteSpace(evaluation.SubmittedRequest?.RequestId))
        {
            return false;
        }

        var expectedBinding = BuildRootOwnershipBinding(rootRequest.JobId, rootRequest.RequestId);
        return rootRequest.Provenance.TryGetValue(
                   RootOwnershipBindingProvenanceKey,
                   out var rootBinding) &&
               string.Equals(rootBinding, expectedBinding, StringComparison.Ordinal) &&
               evaluation.SubmittedRequest.Provenance.TryGetValue(
                   RootOwnershipBindingProvenanceKey,
                   out var evaluationBinding) &&
               string.Equals(evaluationBinding, expectedBinding, StringComparison.Ordinal) &&
               evaluation.SubmittedRequest.Provenance.TryGetValue(
                   SubmittedRequestJobIdProvenanceKey,
                   out var submittedJobId) &&
               string.Equals(submittedJobId, evaluation.JobId, StringComparison.Ordinal);
    }
}

public sealed class DesignConversionInternalServiceThinClientJobEvaluator(
    DesignConversionInternalService service) :
    IDesignConversionThinClientJobEvaluator,
    IDesignConversionDpoPolicyPairRuntimeEvaluator
{
    private readonly DesignConversionInternalService _service =
        service ?? throw new ArgumentNullException(nameof(service));

    public Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
        DesignConversionInternalEvaluateRequest request,
        CancellationToken cancellationToken = default)
        => _service.EvaluateAsync(request, cancellationToken);

    Task<OperationResult<DesignConversionInternalEvaluationResult>>
        IDesignConversionDpoPolicyPairRuntimeEvaluator
            .ApplyDpoPolicyRuntimePairAsync(
                DesignConversionInternalEvaluationResult evaluation,
                DesignConversionDpoPolicyRuntimePair pair,
                CancellationToken cancellationToken)
        => _service.ApplyDpoPolicyRuntimePairAsync(
            evaluation,
            pair,
            cancellationToken);
}

public sealed class DesignConversionImprovementLoopThinClientJobEvaluator :
    IDesignConversionThinClientJobEvaluator,
    IDesignConversionRepositoryOwnedDpoTransactionContextAwareEvaluator
{
    public const string SubmitPathLoopExecutedMarker = "DESIGN_CONVERSION_SUBMIT_PATH_IMPROVEMENT_LOOP_EXECUTED";
    public const string SubmitPathLoopFinalEvaluationMarker = "DESIGN_CONVERSION_SUBMIT_PATH_IMPROVEMENT_LOOP_FINAL_EVALUATION_RETURNED";
    public const string SubmitPathLoopMaxAttemptsMarker = "DESIGN_CONVERSION_SUBMIT_PATH_IMPROVEMENT_LOOP_MAX_ATTEMPTS_FROM_BUDGET";
    public const string MaxAttemptsExceededErrorCode = "IMPROVEMENT_LOOP_MAX_ATTEMPTS_EXCEEDED";

    private readonly IDesignConversionThinClientJobEvaluator _inner;
    private readonly IDesignConversionImprovementCandidateStrategy _candidateStrategy;
    private readonly IDesignConversionRuntimeRejectedEvaluationHandoff? _rejectedEvaluationHandoff;
    private readonly IDesignConversionLocalArtifactDb?
        _independentPreferenceArtifactDb;
    private readonly Func<DateTimeOffset>? _utcNow;

    public DesignConversionImprovementLoopThinClientJobEvaluator(
        IDesignConversionThinClientJobEvaluator inner,
        IDesignConversionImprovementCandidateStrategy? candidateStrategy = null)
        : this(
            inner,
            candidateStrategy,
            rejectedEvaluationHandoff: null,
            independentPreferenceArtifactDb: null,
            utcNow: null)
    {
    }

    internal DesignConversionImprovementLoopThinClientJobEvaluator(
        IDesignConversionThinClientJobEvaluator inner,
        IDesignConversionImprovementCandidateStrategy? candidateStrategy,
        IDesignConversionRuntimeRejectedEvaluationHandoff? rejectedEvaluationHandoff,
        IDesignConversionLocalArtifactDb?
            independentPreferenceArtifactDb = null,
        Func<DateTimeOffset>? utcNow = null)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _candidateStrategy = candidateStrategy ?? new DesignConversionRuntimeImprovementCandidateStrategy();
        _rejectedEvaluationHandoff = rejectedEvaluationHandoff;
        _independentPreferenceArtifactDb = independentPreferenceArtifactDb;
        _utcNow = utcNow;
    }

    public async Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
        DesignConversionInternalEvaluateRequest request,
        CancellationToken cancellationToken = default)
        // The legacy interface remains supported by treating repository-owned
        // DPO reuse as explicitly disabled when no caller supplied the new
        // transaction context.
        => await EvaluateAsync(
            request,
            repositoryOwnedDpoTransactionContext: null,
            cancellationToken);

    public async Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
        DesignConversionInternalEvaluateRequest request,
        DesignConversionRepositoryOwnedDpoTransactionContext?
            repositoryOwnedDpoTransactionContext,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                "DesignConversion internal evaluation request is required.",
                "REQUEST_REQUIRED");
        }

        // A new invocation owns the root identity. Clear any previous pending
        // rejected selection before validation so invalid, single-attempt,
        // cancelled, and failed paths cannot consume stale state later.
        _rejectedEvaluationHandoff?.Discard(request.JobId);

        if (request.Budget is not null && request.Budget.BudgetUsd < 0m)
        {
            return OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                "DesignConversion improvement-loop budgetUsd must be greater than or equal to zero.",
                DesignConversionInternalService.NegativeBudgetUsdErrorCode);
        }

        if (request.Budget is not null &&
            (request.Budget.MaxImprovementCandidates < 0 ||
             (request.Budget.MaxImprovementCandidates == 0 &&
              !DesignConversionVisualQaBoundaryPolicy.IsServerOwnedEntitlementTier(request.Budget.Tier))))
        {
            return OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                "DesignConversion improvement candidate budget must be greater than zero.",
                DesignConversionInternalService.ImprovementCandidateBudgetRequiredCode);
        }

        if (request.Budget is not null &&
            request.Budget.MaxImprovementCandidates > DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts)
        {
            return OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                $"DesignConversion improvement-loop max attempts must be less than or equal to {DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts}.",
                MaxAttemptsExceededErrorCode);
        }

        var resolvedMaxAttempts = ResolveSubmitPathMaxAttempts(request.Budget);
        if (!resolvedMaxAttempts.IsSuccess)
            return resolvedMaxAttempts.MapError<DesignConversionInternalEvaluationResult>();

        var maxAttempts = resolvedMaxAttempts.Value!.MaxAttempts;
        if (maxAttempts <= 1)
            return await _inner.EvaluateAsync(request, cancellationToken);

        OperationResult<DesignConversionImprovementLoopResult> loop;
        try
        {
            var coordinator = new DesignConversionImprovementLoopCoordinator(
                _inner,
                _candidateStrategy,
                _rejectedEvaluationHandoff,
                utcNow: _utcNow,
                independentPreferenceArtifactDb:
                    _independentPreferenceArtifactDb);
            loop = await coordinator.RunAsync(
                new DesignConversionImprovementLoopRequest(
                    LoopId: $"{request.JobId}:submit-path-loop",
                    InitialEvaluationRequest: request,
                    MaxAttempts: maxAttempts,
                    Metadata: new Dictionary<string, string>
                    {
                        ["submit_path_improvement_loop"] = bool.TrueString,
                        ["max_attempts_source"] = resolvedMaxAttempts.Value.Source,
                        ["typed_max_improvement_iterations"] =
                            request.Budget?.MaxImprovementIterations?.ToString(CultureInfo.InvariantCulture) ?? "not_supplied",
                        ["legacy_max_improvement_candidates"] =
                            request.Budget?.MaxImprovementCandidates.ToString(CultureInfo.InvariantCulture) ?? "not_supplied",
                        ["product_ready"] = bool.FalseString
                    })
                {
                    // This attaches the public request's repository-owned DPO
                    // transaction context to the production submit-path loop
                    // before the coordinator decides whether teacher authoring
                    // may issue its first inference call.
                    RepositoryOwnedDpoTransactionContext =
                        repositoryOwnedDpoTransactionContext ??
                        DesignConversionRepositoryOwnedDpoTransactionContext
                            .Disabled
                },
                cancellationToken);
        }
        catch
        {
            _rejectedEvaluationHandoff?.Discard(request.JobId);
            throw;
        }

        if (!loop.IsSuccess)
        {
            _rejectedEvaluationHandoff?.Discard(request.JobId);
            return loop.MapError<DesignConversionInternalEvaluationResult>();
        }

        var result = loop.Value!;
        if (result.FinalEvaluation is null)
        {
            _rejectedEvaluationHandoff?.Discard(request.JobId);
            return OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                "DesignConversion improvement loop finished without a final evaluation.",
                "IMPROVEMENT_LOOP_FINAL_EVALUATION_MISSING");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            _rejectedEvaluationHandoff?.Discard(request.JobId);
            throw new OperationCanceledException(cancellationToken);
        }

        var projectedEvaluation = ProjectFinalEvaluation(
            result,
            request,
            maxAttempts,
            resolvedMaxAttempts.Value.Source);

        return OperationResult<DesignConversionInternalEvaluationResult>.Ok(
            projectedEvaluation);
    }

    internal static DesignConversionInternalEvaluationResult ProjectFinalEvaluation(
        DesignConversionImprovementLoopResult result,
        DesignConversionInternalEvaluateRequest request,
        int maxAttempts,
        string maxAttemptsSource)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(maxAttemptsSource);
        if (result.FinalEvaluation is null)
        {
            throw new ArgumentException(
                "DesignConversion improvement loop must have a final evaluation before projection.",
                nameof(result));
        }

        var projectedEvaluation = AddLoopBoundaryEvidence(
            result.FinalEvaluation,
            result,
            request,
            maxAttempts,
            maxAttemptsSource);
        if (DesignConversionTeacherCallUsageReceiptAssociation.TryResolve(
                result,
                out var teacherCallReceipt) &&
            teacherCallReceipt is not null)
        {
            DesignConversionTeacherCallUsageReceiptAssociation.Associate(
                projectedEvaluation,
                teacherCallReceipt);
        }

        return projectedEvaluation;
    }

    private static OperationResult<SubmitPathMaxAttemptsResolution> ResolveSubmitPathMaxAttempts(
        DesignConversionBudgetMetadata? budget)
    {
        if (budget is null)
        {
            return OperationResult<SubmitPathMaxAttemptsResolution>.Ok(
                new SubmitPathMaxAttemptsResolution(1, "submit_path_default_single_evaluation"));
        }

        if (DesignConversionVisualQaBoundaryPolicy.ResolveEffectiveTierBudgetUsd(
                budget.BudgetUsd,
                budget.Tier) <= 0m)
        {
            return OperationResult<SubmitPathMaxAttemptsResolution>.Ok(
                new SubmitPathMaxAttemptsResolution(1, "submit_path_zero_usd_baseline_only"));
        }

        if (budget.MaxImprovementIterations.HasValue)
        {
            if (!DesignConversionVisualQaBoundaryPolicy.IsValidConfiguredMaxImprovementIterations(
                    budget.MaxImprovementIterations))
            {
                return OperationResult<SubmitPathMaxAttemptsResolution>.Fail(
                    DesignConversionVisualQaBoundaryPolicy.MaxImprovementIterationsInvalidErrorMessage(),
                    DesignConversionVisualQaBoundaryPolicy.MaxImprovementIterationsInvalidErrorCode);
            }

            return OperationResult<SubmitPathMaxAttemptsResolution>.Ok(
                new SubmitPathMaxAttemptsResolution(
                    checked(budget.MaxImprovementIterations.Value + 1),
                    DesignConversionVisualQaBoundaryPolicy.TypedMaxImprovementIterationsSource));
        }

        return OperationResult<SubmitPathMaxAttemptsResolution>.Ok(
            new SubmitPathMaxAttemptsResolution(
                DesignConversionVisualQaBoundaryPolicy.ResolveEffectiveTierMaxImprovementCandidates(
                    budget.MaxImprovementCandidates,
                    budget.Tier),
                DesignConversionVisualQaBoundaryPolicy.LegacyMaxImprovementCandidatesIterationSource));
    }

    private static DesignConversionInternalEvaluationResult AddLoopBoundaryEvidence(
        DesignConversionInternalEvaluationResult evaluation,
        DesignConversionImprovementLoopResult loop,
        DesignConversionInternalEvaluateRequest rootRequest,
        int maxAttempts,
        string maxAttemptsSource)
    {
        var finalEvaluationJobId = evaluation.JobId;
        var finalEvaluationRequestId = evaluation.SubmittedRequest.RequestId;
        var finalEvaluationJobRef = DesignConversionDpoLeakageGuard.HashRef(
            "dcm-submit-path-final-job",
            finalEvaluationJobId);
        var finalEvaluationRequestRef = DesignConversionDpoLeakageGuard.HashRef(
            "dcm-submit-path-final-request",
            finalEvaluationRequestId);
        var replacements = FinalLoopIdentityReplacements.Create(
            rootRequest.JobId,
            rootRequest.RequestId,
            finalEvaluationJobId,
            finalEvaluationRequestId,
            evaluation);
        var sanitizedEvaluation = RewriteFinalLoopIdentityReferences(evaluation, replacements);
        var sanitizedInheritedEvidenceRefs = sanitizedEvaluation.EvidenceRefs
            .Concat(loop.EvidenceRefs)
            .Concat(loop.Attempts.SelectMany(attempt => attempt.EvidenceRefs))
            .Select(replacements.Rewrite);
        var loopVisualCostAccounting = BuildLoopVisualCostAccounting(loop.Attempts);
        var maxTeacherCallsRuntimeMetadata = ExtractMaxTeacherCallsRuntimeMetadata(loop.PublicMetadata);

        return sanitizedEvaluation with
        {
            JobId = rootRequest.JobId,
            SubmittedRequest = sanitizedEvaluation.SubmittedRequest with
            {
                RequestId = rootRequest.RequestId,
                Provenance = MergeMetadata(
                    sanitizedEvaluation.SubmittedRequest.Provenance,
                    maxTeacherCallsRuntimeMetadata)
            },
            VisualCostAccounting = loopVisualCostAccounting,
            EvidenceRefs = sanitizedInheritedEvidenceRefs
                .Append(SubmitPathLoopExecutedMarker)
                .Append(SubmitPathLoopFinalEvaluationMarker)
                .Append(SubmitPathLoopMaxAttemptsMarker)
                .Append($"submit_path_improvement_loop_max_attempts_source={maxAttemptsSource}")
                .Append($"submit_path_improvement_loop_final_evaluation_job_ref={finalEvaluationJobRef}")
                .Append($"submit_path_improvement_loop_final_evaluation_request_ref={finalEvaluationRequestRef}")
                .Distinct(StringComparer.Ordinal)
                .OrderBy(reference => reference, StringComparer.Ordinal)
                .ToArray(),
            Warnings = evaluation.Warnings
                .Where(warning => !IsSubmitPathImprovementLoopStopReasonWarning(warning))
                .Concat(loop.Warnings.Where(warning => !IsSubmitPathImprovementLoopStopReasonWarning(warning)))
                .Concat([
                    "submit_path_improvement_loop=true",
                    $"submit_path_improvement_loop_attempt_count={loop.AttemptCount}",
                    $"submit_path_improvement_loop_stop_reason={loop.StopReason}",
                    $"submit_path_improvement_loop_quality_reached={loop.QualityReached}",
                    $"submit_path_improvement_loop_completed={loop.Completed}",
                    $"submit_path_improvement_loop_max_attempts={maxAttempts}",
                    $"submit_path_improvement_loop_max_attempts_source={maxAttemptsSource}",
                    $"{DesignConversionImprovementLoopCoordinator.MaxTeacherCallsRuntimeEnforcementMetadataKey}={ReadMetadata(maxTeacherCallsRuntimeMetadata, DesignConversionImprovementLoopCoordinator.MaxTeacherCallsRuntimeEnforcementMetadataKey, bool.FalseString)}",
                    $"{DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCallCapEnforcedMetadataKey}={ReadMetadata(maxTeacherCallsRuntimeMetadata, DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCallCapEnforcedMetadataKey, bool.FalseString)}",
                    $"{DesignConversionImprovementLoopCoordinator.MaxTeacherCallsObservedCallCountMetadataKey}={ReadMetadata(maxTeacherCallsRuntimeMetadata, DesignConversionImprovementLoopCoordinator.MaxTeacherCallsObservedCallCountMetadataKey, "0")}",
                    $"{DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCapReachedMetadataKey}={ReadMetadata(maxTeacherCallsRuntimeMetadata, DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCapReachedMetadataKey, bool.FalseString)}",
                    "product_ready=false"
                ])
                .Distinct(StringComparer.Ordinal)
                .OrderBy(warning => warning, StringComparer.Ordinal)
                .ToArray()
        };
    }

    private static bool IsSubmitPathImprovementLoopStopReasonWarning(string warning)
        => warning.StartsWith("submit_path_improvement_loop_stop_reason=", StringComparison.Ordinal);

    private static IReadOnlyDictionary<string, string> ExtractMaxTeacherCallsRuntimeMetadata(
        IReadOnlyDictionary<string, string> metadata)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var key in MaxTeacherCallsMetadataKeys())
        {
            if (metadata.TryGetValue(key, out var value))
                result[key] = value;
        }

        return result;
    }

    private static IEnumerable<string> MaxTeacherCallsMetadataKeys()
    {
        yield return DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractSuppliedMetadataKey;
        yield return DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractValueMetadataKey;
        yield return DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractBoundaryMetadataKey;
        yield return DesignConversionImprovementLoopCoordinator.MaxTeacherCallsRuntimeEnforcementMetadataKey;
        yield return DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCallCapEnforcedMetadataKey;
        yield return DesignConversionImprovementLoopCoordinator.MaxTeacherCallsObservedCallCountMetadataKey;
        yield return DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCapReachedMetadataKey;
    }

    private static IReadOnlyDictionary<string, string> MergeMetadata(
        IReadOnlyDictionary<string, string>? first,
        IReadOnlyDictionary<string, string> second)
        => (first ?? new Dictionary<string, string>())
            .Concat(second)
            .GroupBy(pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal);

    private static string ReadMetadata(
        IReadOnlyDictionary<string, string> metadata,
        string key,
        string fallback)
        => metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;

    private sealed record SubmitPathMaxAttemptsResolution(int MaxAttempts, string Source);

    private static DesignConversionVisualCostAccounting BuildLoopVisualCostAccounting(
        IReadOnlyList<DesignConversionImprovementLoopAttemptTelemetry> attempts)
    {
        var sources = attempts
            .Select(attempt => attempt.VisualCostSource)
            .Where(source => !string.IsNullOrWhiteSpace(source))
            .Select(source => source.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var costSource = sources.Length switch
        {
            0 => DesignConversionVisualCostAccounting.UnknownCostSource,
            1 => sources[0],
            _ => DesignConversionVisualCostAccounting.MixedCostSource
        };

        return new DesignConversionVisualCostAccounting(
            TotalCostUsd: attempts.Select(attempt => attempt.EstimatedCostUsd).Sum(),
            MeasurementComplete: attempts.Count > 0 &&
                attempts.All(attempt => attempt.VisualCostMeasurementComplete),
            MissingMeasurementCount: attempts.Select(attempt => attempt.VisualCostMissingMeasurementCount).Sum(),
            EvaluationAttemptCount: attempts.Count,
            CostSource: costSource);
    }

    private static DesignConversionInternalEvaluationResult RewriteFinalLoopIdentityReferences(
        DesignConversionInternalEvaluationResult evaluation,
        FinalLoopIdentityReplacements r)
        => evaluation with
        {
            SubmittedRequest = Rewrite(evaluation.SubmittedRequest, r),
            RenderResults = evaluation.RenderResults.Select(value => Rewrite(value, r)).ToArray(),
            ComparisonArtifacts = evaluation.ComparisonArtifacts.Select(value => Rewrite(value, r)).ToArray(),
            VisualJudgeResults = evaluation.VisualJudgeResults.Select(value => Rewrite(value, r)).ToArray(),
            QaResults = evaluation.QaResults.Select(value => Rewrite(value, r)).ToArray(),
            RuntimeFlowResult = Rewrite(evaluation.RuntimeFlowResult, r),
            Verdict = Rewrite(evaluation.Verdict, r),
            EvidenceRefs = Rewrite(evaluation.EvidenceRefs, r),
            ScaffoldMarkers = RewriteMetadataValues(evaluation.ScaffoldMarkers, r),
            VisualQaBoundaryTrace = Rewrite(evaluation.VisualQaBoundaryTrace, r),
            ViewportFailures = evaluation.ViewportFailures.Select(failure => failure with
            {
                ViewportId = r.Rewrite(failure.ViewportId),
                EvidenceRefs = Rewrite(failure.EvidenceRefs, r)
            }).ToArray()
        };

    private static DesignConversionRuntimeFlowResult Rewrite(DesignConversionRuntimeFlowResult result, FinalLoopIdentityReplacements r)
        => result with { Request = Rewrite(result.Request, r), CriticOutput = result.CriticOutput with { OutputId = r.Rewrite(result.CriticOutput.OutputId), Verdict = Rewrite(result.CriticOutput.Verdict, r), QaEvidenceRefs = Rewrite(result.CriticOutput.QaEvidenceRefs, r), AiTeacherEvidenceRefs = Rewrite(result.CriticOutput.AiTeacherEvidenceRefs, r) }, ImproverOutput = result.ImproverOutput with { OutputId = r.Rewrite(result.ImproverOutput.OutputId), Candidate = Rewrite(result.ImproverOutput.Candidate, r), Output = Rewrite(result.ImproverOutput.Output, r), QaEvidenceRefs = Rewrite(result.ImproverOutput.QaEvidenceRefs, r), AiTeacherEvidenceRefs = Rewrite(result.ImproverOutput.AiTeacherEvidenceRefs, r) }, Candidate = Rewrite(result.Candidate, r), Output = Rewrite(result.Output, r), EvidenceRefs = Rewrite(result.EvidenceRefs, r) };

    private static SubmittedDesignConversionRequest Rewrite(SubmittedDesignConversionRequest request, FinalLoopIdentityReplacements r)
        => request with { DesignTreePayload = Rewrite(request.DesignTreePayload, r), OriginalTreePayload = Rewrite(request.OriginalTreePayload, r), Screenshots = request.Screenshots.Select(artifact => Rewrite(artifact, r)).ToArray(), DesignRenders = request.DesignRenders.Select(artifact => Rewrite(artifact, r)).ToArray(), Provenance = RewriteMetadataValues(request.Provenance, r) };

    private static DesignTreePayload Rewrite(DesignTreePayload payload, FinalLoopIdentityReplacements r)
        => payload with { PayloadId = r.Rewrite(payload.PayloadId), Sha256 = r.Rewrite(payload.Sha256), Metadata = RewriteMetadataValues(payload.Metadata, r) };

    private static DesignRenderArtifact Rewrite(DesignRenderArtifact artifact, FinalLoopIdentityReplacements r)
        => artifact with { ArtifactId = r.Rewrite(artifact.ArtifactId), Uri = r.Rewrite(artifact.Uri), Sha256 = r.Rewrite(artifact.Sha256), Metadata = RewriteMetadataValues(artifact.Metadata, r) };

    private static DesignConversionRenderResult Rewrite(DesignConversionRenderResult result, FinalLoopIdentityReplacements r)
        => result with { RenderResultId = r.Rewrite(result.RenderResultId), RenderRequestId = r.Rewrite(result.RenderRequestId), Image = Rewrite(result.Image, r), BrowserRenderProofReceipt = result.BrowserRenderProofReceipt is null ? null : result.BrowserRenderProofReceipt with { RenderResultId = r.Rewrite(result.BrowserRenderProofReceipt.RenderResultId), RenderRequestId = r.Rewrite(result.BrowserRenderProofReceipt.RenderRequestId), RequestId = r.Rewrite(result.BrowserRenderProofReceipt.RequestId), ImageId = r.Rewrite(result.BrowserRenderProofReceipt.ImageId), ScreenshotUri = r.Rewrite(result.BrowserRenderProofReceipt.ScreenshotUri), NormalizedSha256 = r.Rewrite(result.BrowserRenderProofReceipt.NormalizedSha256), ResolutionId = r.Rewrite(result.BrowserRenderProofReceipt.ResolutionId) } };

    private static DesignConversionQaComparisonArtifact Rewrite(DesignConversionQaComparisonArtifact artifact, FinalLoopIdentityReplacements r)
        => artifact with { ArtifactId = r.Rewrite(artifact.ArtifactId), ComparisonRequestId = r.Rewrite(artifact.ComparisonRequestId), Expected = Rewrite(artifact.Expected, r), Actual = Rewrite(artifact.Actual, r), Verdict = Rewrite(artifact.Verdict, r), DiffImageUri = r.Rewrite(artifact.DiffImageUri), DiffImageSha256 = r.Rewrite(artifact.DiffImageSha256), Metrics = r.RewriteExternalTrustMetrics(artifact.Metrics), EvidenceRefs = Rewrite(artifact.EvidenceRefs, r), Metadata = RewriteMetadataValues(artifact.Metadata, r) };

    private static DesignConversionVisualJudgeResult Rewrite(DesignConversionVisualJudgeResult result, FinalLoopIdentityReplacements r)
        => result with { JudgeResultId = r.Rewrite(result.JudgeResultId), JudgeRequestId = r.Rewrite(result.JudgeRequestId), Verdict = Rewrite(result.Verdict, r), Provider = result.Provider with { EndpointHost = r.Rewrite(result.Provider.EndpointHost), ModelId = r.Rewrite(result.Provider.ModelId), Metadata = RewriteMetadataValues(result.Provider.Metadata, r) }, EvidenceRefs = Rewrite(result.EvidenceRefs, r), Metadata = RewriteMetadataValues(result.Metadata, r) };

    private static DesignConversionImprovementCandidate Rewrite(DesignConversionImprovementCandidate candidate, FinalLoopIdentityReplacements r)
        => candidate with { CandidateId = r.Rewrite(candidate.CandidateId), RequestId = r.Rewrite(candidate.RequestId), EvidenceRefs = Rewrite(candidate.EvidenceRefs, r), Metadata = RewriteMetadataValues(candidate.Metadata, r) };

    private static DesignConversionImprovementOutput Rewrite(DesignConversionImprovementOutput output, FinalLoopIdentityReplacements r)
        => output with { OutputId = r.Rewrite(output.OutputId), CandidateId = r.Rewrite(output.CandidateId), Verdict = Rewrite(output.Verdict, r), QaResults = output.QaResults.Select(result => Rewrite(result, r)).ToArray(), EvidenceRefs = Rewrite(output.EvidenceRefs, r) };

    private static RenderQaResultRecord Rewrite(RenderQaResultRecord result, FinalLoopIdentityReplacements r)
        => result with { ResultId = r.Rewrite(result.ResultId), RequestId = r.Rewrite(result.RequestId), Expected = Rewrite(result.Expected, r), Actual = Rewrite(result.Actual, r), Verdict = Rewrite(result.Verdict, r) };

    private static RenderQaImageRecord Rewrite(RenderQaImageRecord image, FinalLoopIdentityReplacements r)
        => image with { ImageId = r.Rewrite(image.ImageId), Resolution = image.Resolution with { ResolutionId = r.Rewrite(image.Resolution.ResolutionId) }, Uri = r.Rewrite(image.Uri), Sha256 = r.Rewrite(image.Sha256), Metadata = RewriteMetadataValues(image.Metadata, r) };

    private static VisualVerdict Rewrite(VisualVerdict verdict, FinalLoopIdentityReplacements r)
        => verdict with { VerdictId = r.Rewrite(verdict.VerdictId), Defects = verdict.Defects.Select(defect => defect with { DefectId = r.Rewrite(defect.DefectId), EvidenceImageIds = Rewrite(defect.EvidenceImageIds, r) }).ToArray(), EvidenceRefs = Rewrite(verdict.EvidenceRefs, r) };

    private static DesignConversionVisualQaBoundaryTrace Rewrite(DesignConversionVisualQaBoundaryTrace trace, FinalLoopIdentityReplacements r)
        => trace with { MultiResolutionRender = trace.MultiResolutionRender with { RequiredViewportIds = Rewrite(trace.MultiResolutionRender.RequiredViewportIds, r), ObservedViewportIds = Rewrite(trace.MultiResolutionRender.ObservedViewportIds, r), MissingViewportIds = Rewrite(trace.MultiResolutionRender.MissingViewportIds, r), EvidenceRefs = Rewrite(trace.MultiResolutionRender.EvidenceRefs, r) }, TierPolicy = trace.TierPolicy with { EvidenceRefs = Rewrite(trace.TierPolicy.EvidenceRefs, r), Metadata = RewriteMetadataValues(trace.TierPolicy.Metadata, r) }, VisualDecision = trace.VisualDecision with { CorrectionFindings = trace.VisualDecision.CorrectionFindings.Select(finding => finding with { FindingId = r.Rewrite(finding.FindingId), EvidenceImageIds = Rewrite(finding.EvidenceImageIds, r) }).ToArray(), EvidenceRefs = Rewrite(trace.VisualDecision.EvidenceRefs, r), Metadata = RewriteMetadataValues(trace.VisualDecision.Metadata, r) }, EvidenceRefs = Rewrite(trace.EvidenceRefs, r) };

    private static IReadOnlyList<string> Rewrite(IEnumerable<string> values, FinalLoopIdentityReplacements r)
        => values.Select(r.RewriteEvidenceReference).ToArray();

    private static IReadOnlyDictionary<string, string> RewriteMetadataValues(IReadOnlyDictionary<string, string> metadata, FinalLoopIdentityReplacements r)
    {
        var rewritten = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in metadata)
            rewritten[pair.Key] = r.RewriteMetadataValue(pair.Key, pair.Value);

        return rewritten;
    }

    private sealed class FinalLoopIdentityReplacements
    {
        private static readonly HashSet<string> UntrustedExternalBooleanMetadataKeys = new(StringComparer.Ordinal)
        {
            "network_call_executed",
            "external_network_call_executed",
            "external_provider_called",
            "real_external_provider_executed",
            "external_visual_ai_executed",
            "external_ai_visual_judge_executed",
            "visual_ai_executed",
            "visual_ai_provider_contract_executed",
            "provider_response_identity_verified",
            "provider_response_schema_verified",
            "provider_response_verified",
            "visual_fidelity_verified",
            "visual_fidelity_provider_receipt_verified",
            "visual_fidelity_response_identity_verified",
            "visual_fidelity_response_schema_verified",
            ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey,
            ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey,
            ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey,
            ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey
        };

        private static readonly HashSet<string> UntrustedExternalMetricKeys = new(StringComparer.Ordinal)
        {
            "network_call_executed",
            "real_external_provider_executed",
            "external_visual_ai_executed",
            "external_ai_visual_judge_executed"
        };

        private readonly (string Raw, string Replacement)[] _items;

        private FinalLoopIdentityReplacements(
            IEnumerable<(string Raw, string Replacement)> items,
            bool downgradeExternalReceipts)
        {
            DowngradeExternalReceipts = downgradeExternalReceipts;
            _items = items
                .OrderByDescending(item => item.Raw.Length)
                .ThenBy(item => item.Raw, StringComparer.Ordinal)
                .ToArray();
        }

        private bool DowngradeExternalReceipts { get; }

        public static FinalLoopIdentityReplacements Create(
            string rootJobId,
            string rootRequestId,
            string finalJobId,
            string finalRequestId,
            DesignConversionInternalEvaluationResult evaluation)
        {
            var externalArtifacts = evaluation.ComparisonArtifacts
                .Where(IsExternalComparisonReceipt)
                .ToArray();
            var externalJudgeResults = evaluation.VisualJudgeResults
                .Where(IsExternalJudgeReceipt)
                .ToArray();
            var hasExternalReceipts = externalArtifacts.Length > 0 || externalJudgeResults.Length > 0;
            var externalReceiptsValid = HasValidOriginalExternalReceipts(
                externalArtifacts,
                externalJudgeResults,
                finalJobId,
                finalRequestId);
            var downgradeExternalReceipts = hasExternalReceipts && !externalReceiptsValid;
            var items = new List<(string Raw, string Replacement)>();
            Add(
                items,
                finalJobId,
                downgradeExternalReceipts
                    ? DesignConversionDpoLeakageGuard.HashRef("dcm-untrusted-final-loop-job", finalJobId)
                    : rootJobId);
            Add(
                items,
                finalRequestId,
                downgradeExternalReceipts
                    ? DesignConversionDpoLeakageGuard.HashRef("dcm-untrusted-final-loop-request", finalRequestId)
                    : rootRequestId);

            if (hasExternalReceipts && externalReceiptsValid)
            {
                var rootReplacements = new FinalLoopIdentityReplacements(
                    items,
                    downgradeExternalReceipts: false);
                AddCanonicalComparisonResultReplacements(items, rootReplacements, externalArtifacts);
                AddCanonicalVisualJudgeResultReplacements(items, rootReplacements, externalJudgeResults);
            }

            return new FinalLoopIdentityReplacements(items, downgradeExternalReceipts);
        }

        private static void AddCanonicalComparisonResultReplacements(
            List<(string Raw, string Replacement)> items,
            FinalLoopIdentityReplacements rootReplacements,
            IEnumerable<DesignConversionQaComparisonArtifact> artifacts)
        {
            foreach (var artifact in artifacts)
            {
                var providerRequestId = ReadMetadata(
                    artifact.Metadata,
                    ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey);
                var comparisonRequestId = artifact.ComparisonRequestId;
                var comparisonResultId = ReadMetadata(
                    artifact.Metadata,
                    ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualResultIdKey);
                var comparisonArtifactId = ReadMetadata(
                    artifact.Metadata,
                    ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey);
                var normalizedProviderRequestId = rootReplacements.Rewrite(providerRequestId);
                var normalizedComparisonRequestId = rootReplacements.Rewrite(comparisonRequestId);
                var normalizedComparisonResultId =
                    ExternalHttpDesignConversionVisualJudge.BuildCanonicalVisualResultId(
                        normalizedProviderRequestId,
                        normalizedComparisonRequestId);
                var normalizedComparisonArtifactId =
                    ExternalHttpDesignConversionVisualJudge.BuildCanonicalComparisonArtifactId(
                        normalizedProviderRequestId,
                        normalizedComparisonRequestId);

                Add(items, comparisonResultId, normalizedComparisonResultId);
                Add(items, comparisonArtifactId, normalizedComparisonArtifactId);

                var originalDefaultDiffUri = BuildDefaultComparisonDiffUri(comparisonResultId);
                if (string.Equals(artifact.DiffImageUri, originalDefaultDiffUri, StringComparison.Ordinal))
                {
                    Add(
                        items,
                        originalDefaultDiffUri,
                        BuildDefaultComparisonDiffUri(normalizedComparisonResultId));
                }
            }
        }

        private static void AddCanonicalVisualJudgeResultReplacements(
            List<(string Raw, string Replacement)> items,
            FinalLoopIdentityReplacements rootReplacements,
            IEnumerable<DesignConversionVisualJudgeResult> results)
        {
            foreach (var result in results)
            {
                var resultComparisonRequestId = ReadMetadata(
                    result.Metadata,
                    ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey);
                var normalizedJudgeRequestId = rootReplacements.Rewrite(result.JudgeRequestId);
                var normalizedComparisonRequestId = rootReplacements.Rewrite(resultComparisonRequestId);
                var normalizedJudgeResultId =
                    ExternalHttpDesignConversionVisualJudge.BuildCanonicalVisualResultId(
                        normalizedJudgeRequestId,
                        normalizedComparisonRequestId);
                Add(items, result.JudgeResultId, normalizedJudgeResultId);
            }
        }

        private static bool HasValidOriginalExternalReceipts(
            IReadOnlyList<DesignConversionQaComparisonArtifact> artifacts,
            IReadOnlyList<DesignConversionVisualJudgeResult> judgeResults,
            string finalJobId,
            string finalRequestId)
            => DesignConversionExternalVisualAiReceiptValidator.HasValidOriginalExternalReceipts(
                artifacts,
                judgeResults,
                finalJobId,
                finalRequestId);

        private static bool IsExternalComparisonReceipt(DesignConversionQaComparisonArtifact artifact)
            => IsTruthy(artifact.Metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey) ||
               IsTruthy(artifact.Metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey) ||
               IsTruthy(artifact.Metadata, "network_call_executed") ||
               IsTruthy(artifact.Metadata, "external_visual_ai_executed");

        private static bool IsExternalJudgeReceipt(DesignConversionVisualJudgeResult result)
            => result.Provider.ExternalCallExecuted ||
               IsTruthy(result.Provider.Metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey) ||
               IsTruthy(result.Metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey);

        private static bool IsTruthy(IReadOnlyDictionary<string, string> metadata, string key)
            => metadata.TryGetValue(key, out var value) &&
               (string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "1", StringComparison.Ordinal) ||
                string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase));

        private static bool IsSafeIdentity(string? value)
            => !string.IsNullOrWhiteSpace(value) &&
               !string.Equals(value, "none", StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(value, "missing", StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(value, "unconfigured", StringComparison.OrdinalIgnoreCase);

        public string RewriteMetadataValue(string key, string value)
        {
            var rewritten = Rewrite(value);
            return DowngradeExternalReceipts && UntrustedExternalBooleanMetadataKeys.Contains(key)
                ? bool.FalseString
                : rewritten;
        }

        public string RewriteEvidenceReference(string value)
        {
            var rewritten = Rewrite(value);
            if (!DowngradeExternalReceipts)
                return rewritten;

            if (string.Equals(rewritten, "network_call_executed:true", StringComparison.Ordinal))
                return "network_call_executed:false";
            if (string.Equals(rewritten, "real_external_provider_executed:true", StringComparison.Ordinal))
                return "real_external_provider_executed:false";
            if (string.Equals(rewritten, "external_visual_ai_executed:true", StringComparison.Ordinal))
                return "external_visual_ai_executed:false";
            if (string.Equals(rewritten, "external_ai_visual_judge_executed:true", StringComparison.Ordinal) ||
                string.Equals(rewritten, "visual_ai_executed_true", StringComparison.Ordinal))
            {
                return DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker;
            }

            return string.Equals(
                rewritten,
                $"{ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey}:true",
                StringComparison.Ordinal)
                ? $"{ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey}:false"
                : rewritten;
        }

        public IReadOnlyDictionary<string, double> RewriteExternalTrustMetrics(
            IReadOnlyDictionary<string, double> metrics)
        {
            if (!DowngradeExternalReceipts)
                return metrics;

            var rewritten = new Dictionary<string, double>(StringComparer.Ordinal);
            foreach (var pair in metrics)
            {
                rewritten[pair.Key] = UntrustedExternalMetricKeys.Contains(pair.Key)
                    ? 0.0
                    : pair.Value;
            }

            return rewritten;
        }

        private static string ReadMetadata(
            IReadOnlyDictionary<string, string> metadata,
            string key)
            => metadata.TryGetValue(key, out var value) ? value : string.Empty;

        private static string BuildDefaultComparisonDiffUri(string comparisonResultId)
            => $"artifact://design-conversion/visual-judge/{Uri.EscapeDataString(comparisonResultId)}";

        public string Rewrite(string value)
        {
            var input = value ?? string.Empty;
            if (input.Length == 0 || _items.Length == 0)
                return input;

            var rewritten = new StringBuilder(input.Length);
            for (var index = 0; index < input.Length;)
            {
                var matched = false;
                foreach (var item in _items)
                {
                    if (!input.AsSpan(index).StartsWith(item.Raw, StringComparison.Ordinal))
                        continue;

                    rewritten.Append(item.Replacement);
                    index += item.Raw.Length;
                    matched = true;
                    break;
                }

                if (!matched)
                    rewritten.Append(input[index++]);
            }

            return rewritten.ToString();
        }

        private static void Add(List<(string Raw, string Replacement)> items, string raw, string root)
        {
            if (string.IsNullOrWhiteSpace(raw) || string.Equals(raw, root, StringComparison.Ordinal))
                return;

            if (items.Any(item => string.Equals(item.Raw, raw, StringComparison.Ordinal)))
                return;

            items.Add((raw, root));
        }
    }
}

/// <summary>
/// Public composition boundary for the submit-path improvement evaluator and
/// its opaque rejected-evaluation lookup source. It intentionally exposes no
/// recorder or raw evaluation history.
/// </summary>
public sealed record DesignConversionImprovementLoopRuntimeComposition(
    IDesignConversionThinClientJobEvaluator Evaluator,
    IDesignConversionRuntimeRejectedEvaluationSource RejectedEvaluationSource);

public static class DesignConversionImprovementLoopRuntimeCompositionFactory
{
    public static DesignConversionImprovementLoopRuntimeComposition Create(
        IDesignConversionThinClientJobEvaluator evaluator,
        IDesignConversionImprovementCandidateStrategy? candidateStrategy = null,
        IDesignConversionLocalArtifactDb?
            independentPreferenceArtifactDb = null,
        Func<DateTimeOffset>? utcNow = null)
    {
        ArgumentNullException.ThrowIfNull(evaluator);
        var handoff = new DesignConversionRuntimeRejectedEvaluationHandoff();
        return new DesignConversionImprovementLoopRuntimeComposition(
            new DesignConversionImprovementLoopThinClientJobEvaluator(
                evaluator,
                candidateStrategy,
                handoff,
                independentPreferenceArtifactDb,
                utcNow),
            handoff);
    }
}

public sealed record DesignConversionAccountingReconciliationBatchResult(
    bool Completed,
    int ExaminedCount,
    int ReconciledCount,
    int FailedCount,
    int RemainingPendingCount,
    string LastErrorCode,
    string TerminalErrorCode)
{
    public bool IsSuccess =>
        Completed && string.IsNullOrEmpty(TerminalErrorCode);

    public string ErrorCode => TerminalErrorCode;

    public string ErrorMessage => IsSuccess
        ? string.Empty
        : DesignConversionThinClientJobRuntime.AccountingReconciliationRequiredSafeMessage;
}

public sealed class DesignConversionThinClientJobRuntime
{
    public const string RuntimeBoundary = "in_process_thin_client_job_runtime_only";
    public const string StorageBoundary = "in_memory_only";
    public const string DeterministicExecutionBoundary = "deterministic_internal_service_path_only";
    public const string Flow47VerificationAuthority = "host_boundary";
    public const string Flow47VerificationState = "not_evaluated_in_thin_runtime";
    public const string VisualCostRecordsReadFailedErrorCode = "DESIGN_CONVERSION_THIN_CLIENT_VISUAL_COST_RECORDS_READ_FAILED";
    public const string VisualCostRecordsReadFailedSafeMessage =
        "DesignConversion visual-cost records could not be read for the requested job.";
    public const string VisualCostRecordsCancelledSafeMessage =
        "DesignConversion visual-cost record readback cancelled.";
    public const string BrowserRuntimeBlockerSafeMessage =
        "DesignConversion browser runtime blocker at thin-client boundary.";
    public const string BrowserScreenshotFailureSafeMessage =
        "DesignConversion browser screenshot failed at thin-client boundary.";
    public const string LocalArtifactDbDpoPersistenceRequiredErrorCode =
        "DESIGN_CONVERSION_DPO_ARTIFACT_DB_REQUIRED";
    public const string LocalArtifactDbDpoPersistenceFailedErrorCode =
        "DESIGN_CONVERSION_DPO_ARTIFACT_DB_PERSISTENCE_FAILED";
    public const string LocalArtifactDbDpoReadbackFailedErrorCode =
        "DESIGN_CONVERSION_DPO_ARTIFACT_DB_READBACK_FAILED";
    public const string LocalArtifactDbDpoReadbackMismatchErrorCode =
        "DESIGN_CONVERSION_DPO_ARTIFACT_DB_READBACK_MISMATCH";
    public const string LocalArtifactDbDpoPersistenceSafeMessage =
        "DesignConversion runtime-created DPO candidate persistence could not be durably verified.";
    public const string BrowserRuntimeBlockerProvenanceKey = "browser_runtime_blocker";
    public const string BrowserRuntimeBlockerErrorCodeProvenanceKey = "browser_runtime_blocker_error_code";
    public const string TrustedVisualCostBudgetStoreRequiredErrorCode =
        "DESIGN_CONVERSION_TRUSTED_VISUAL_COST_BUDGET_STORE_REQUIRED";
    public const string TrustedVisualCostBudgetContractInvalidErrorCode =
        "DESIGN_CONVERSION_TRUSTED_VISUAL_COST_BUDGET_CONTRACT_INVALID";
    public const string TrustedVisualCostBudgetReservationDeniedErrorCode =
        "DESIGN_CONVERSION_TRUSTED_VISUAL_COST_BUDGET_RESERVATION_DENIED";
    public const string TrustedVisualCostBudgetReservationFailedErrorCode =
        "DESIGN_CONVERSION_TRUSTED_VISUAL_COST_BUDGET_RESERVATION_FAILED";
    public const string TrustedVisualCostBudgetSettlementFailedErrorCode =
        "DESIGN_CONVERSION_TRUSTED_VISUAL_COST_BUDGET_SETTLEMENT_FAILED";
    public const string TrustedVisualCostAccountingIncompleteErrorCode =
        "DESIGN_CONVERSION_TRUSTED_VISUAL_COST_ACCOUNTING_INCOMPLETE";
    public const string TrustedVisualCostActualExceedsReservationErrorCode =
        "DESIGN_CONVERSION_TRUSTED_VISUAL_COST_ACTUAL_EXCEEDS_RESERVATION";
    public const string StaleRunningTimeoutErrorCode = "THIN_CLIENT_JOB_RUNTIME_STALE_RUNNING_TIMEOUT";
    public const string StaleRunningTimeoutSafeMessage =
        "DesignConversion thin-client job failed because its running marker expired before terminal runtime output was produced.";
    public static readonly TimeSpan DefaultStaleRunningTimeout = TimeSpan.FromMinutes(30);
    public const string PostEvaluationFinalizationTimeoutErrorCode =
        "THIN_CLIENT_JOB_RUNTIME_POST_EVALUATION_FINALIZATION_TIMEOUT";
    public const string PostEvaluationFinalizationCancelledErrorCode =
        "THIN_CLIENT_JOB_RUNTIME_POST_EVALUATION_FINALIZATION_CANCELLED";
    public const string PostEvaluationFinalizationFailedErrorCode =
        "THIN_CLIENT_JOB_RUNTIME_POST_EVALUATION_FINALIZATION_FAILED";
    public const string PostEvaluationFinalizationTimeoutSafeMessage =
        "DesignConversion thin-client job failed because post-evaluation finalization exceeded its bounded deadline.";
    public const string PostEvaluationFinalizationCancelledSafeMessage =
        "DesignConversion thin-client job post-evaluation finalization was cancelled and failed closed.";
    public const string PostEvaluationFinalizationFailedSafeMessage =
        "DesignConversion thin-client job post-evaluation finalization failed closed.";
    public const string AccountingReconciliationRequiredErrorCode =
        "DESIGN_CONVERSION_ACCOUNTING_RECONCILIATION_REQUIRED";
    public const string AccountingReconciliationRequiredSafeMessage =
        "DesignConversion visual-cost accounting requires durable reconciliation.";
    public const string AccountingReconciliationBatchSizeInvalidErrorCode =
        "DESIGN_CONVERSION_ACCOUNTING_RECONCILIATION_BATCH_SIZE_INVALID";
    public const string AccountingReconciliationBudgetStoreRequiredErrorCode =
        "DESIGN_CONVERSION_ACCOUNTING_RECONCILIATION_BUDGET_STORE_REQUIRED";
    public const string AccountingReconciliationItemFailedErrorCode =
        "DESIGN_CONVERSION_ACCOUNTING_RECONCILIATION_ITEM_FAILED";
    public const string AccountingReconciliationReservationIdentityInvalidErrorCode =
        "DESIGN_CONVERSION_ACCOUNTING_RECONCILIATION_RESERVATION_IDENTITY_INVALID";
    public const string AccountingReconciliationLateWorkDrainTimeoutErrorCode =
        "DESIGN_CONVERSION_ACCOUNTING_RECONCILIATION_LATE_WORK_DRAIN_TIMEOUT";
    public const string AccountingReconciliationShutdownStartedErrorCode =
        "DESIGN_CONVERSION_ACCOUNTING_RECONCILIATION_SHUTDOWN_STARTED";
    public const int AccountingReconciliationDefaultMaximumItems = 64;
    public const int AccountingReconciliationMaximumAllowedItems = 1024;
    public static readonly TimeSpan DefaultPostEvaluationFinalizationTimeout = TimeSpan.FromMinutes(2);
    public const string ServerOwnedRouteAdmissionTenantBoundMetadataKey =
        "server_owned_route_admission_tenant_bound";
    public const string ServerOwnedRouteAdmissionTenantIdRefMetadataKey =
        "server_owned_route_admission_tenant_id_ref";
    public const string ServerOwnedRouteAdmissionScopeRefMetadataKey =
        "server_owned_route_admission_scope_ref";
    public const string ServerOwnedRequiredBrowserQaMatrixProvenanceKey =
        "server_owned_required_browser_qa_matrix";

    private const string TrustedEntitlementBoundKey = "trusted_visual_cost_entitlement_bound";
    private const string TrustedEntitlementAuthorityKey = "trusted_visual_cost_entitlement_authority";
    private const string TrustedUserRefKey = "trusted_visual_cost_user_ref";
    private const string TrustedTenantIdRefKey = "trusted_visual_cost_tenant_id_ref";
    private const string TrustedUserIdentityAuthorityKey = "trusted_visual_cost_user_identity_authority";
    private const string ServerAssignedDesignTierKey = "server_assigned_design_tier";
    private const string ServerAssignedMonthlyLimitUsdKey = "server_assigned_monthly_visual_cost_limit_usd";
    private const string DesignTierAuthorityKey = "design_tier_authority";
    private const string MonthlyVisualCostLimitAuthorityKey = "monthly_visual_cost_limit_authority";
    private const string UserIdentityAuthorityKey = "user_identity_authority";
    private const string TrustedPeriodKindKey = "trusted_visual_cost_period_kind";
    private const string TrustedReservationStoreWiredKey = "trusted_visual_cost_reservation_store_wired";
    private const string PlatformTenantConfigurationAuthority = "platform_tenant_configuration";
    private const string VerifiedInboundPrincipalAuthority = "verified_inbound_principal";
    private const string RouteAdmissionTenantRefScope = "design-conversion-model-activation-tenant";
    private const string RouteAdmissionScopeRefScope = "design-conversion-route-admission-scope";

    private static readonly IReadOnlyList<string> BoundaryWarnings =
    [
        "broad_unfiltered_tests_project_verified=false",
        "public_api_verified=false",
        "flow47_verification_authority=" + Flow47VerificationAuthority,
        "flow47_verification_state=" + Flow47VerificationState,
        "platform_integration_verified=false",
        "persistence_verified=false",
        "product_ready_verified=false",
        "runtime_ready_verified=false",
        "trained_feature_verified=false",
        "model_training_verified=false",
        "browser_e2e_verified=false",
        "visual_fidelity_verified=false",
        "deployment_verified=false",
        "host_readiness_verified=false"
    ];

    private readonly object _gate = new();
    private readonly IDesignConversionThinClientJobEvaluator _evaluator;
    private readonly IDesignConversionLocalArtifactDb? _artifactDb;
    private readonly IDesignConversionVisualCostLedger _visualCostLedger;
    private readonly IDesignConversionVisualCostBudgetStore? _visualCostBudgetStore;
    private readonly IDesignConversionRuntimeRejectedEvaluationSource? _rejectedEvaluationSource;
    private readonly Func<DateTimeOffset> _utcNow;
    private readonly TimeSpan _staleRunningTimeout;
    private readonly Func<Task>? _beforePostEvaluationOwnershipTransition;
    private readonly Action? _beforeReservationReleaseStatusPublication;
    private readonly TimeSpan _postEvaluationFinalizationTimeout;
    private readonly Func<TimeSpan, CancellationToken, Task> _postEvaluationFinalizationDelay;
    private readonly IDesignConversionPostEvaluationStateStore _postEvaluationStateStore;
    private readonly Action<DesignConversionPostEvaluationState, int>?
        _afterAccountingReconciliationItemForTesting;
    private readonly Action<int>?
        _afterAccountingReconciliationDrainSnapshotForTesting;
    private readonly Dictionary<string, SubmittedDesignConversionRequest> _requests = new(StringComparer.Ordinal);
    private readonly Dictionary<string, DesignConversionThinClientJob> _jobs = new(StringComparer.Ordinal);
    private readonly Dictionary<string, DesignConversionThinClientStatusResult> _statuses = new(StringComparer.Ordinal);
    private readonly Dictionary<string, JobRunLease> _runLeases = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<long, Task> _accountingReconciliationLateTasks = new();
    private readonly object _accountingReconciliationLateWorkGate = new();
    private long _nextAccountingReconciliationLateTaskId;
    private bool _accountingReconciliationLateWorkDrainStarted;

    public DesignConversionThinClientJobRuntime(
        DesignConversionInternalService service,
        Func<DateTimeOffset>? utcNow = null,
        IDesignConversionLocalArtifactDb? artifactDb = null,
        IDesignConversionVisualCostLedger? visualCostLedger = null,
        IDesignConversionVisualCostBudgetStore? visualCostBudgetStore = null,
        TimeSpan? staleRunningTimeout = null,
        TimeSpan? postEvaluationFinalizationTimeout = null,
        IDesignConversionPostEvaluationStateStore? postEvaluationStateStore = null,
        IDesignConversionRuntimeRejectedEvaluationSource? rejectedEvaluationSource = null)
        : this(
            new DesignConversionInternalServiceThinClientJobEvaluator(service),
            utcNow,
            artifactDb,
            visualCostLedger,
            visualCostBudgetStore,
            staleRunningTimeout,
            postEvaluationFinalizationTimeout,
            postEvaluationStateStore,
            rejectedEvaluationSource)
    {
    }

    public DesignConversionThinClientJobRuntime(
        IDesignConversionThinClientJobEvaluator evaluator,
        Func<DateTimeOffset>? utcNow = null,
        IDesignConversionLocalArtifactDb? artifactDb = null,
        IDesignConversionVisualCostLedger? visualCostLedger = null,
        IDesignConversionVisualCostBudgetStore? visualCostBudgetStore = null,
        TimeSpan? staleRunningTimeout = null,
        TimeSpan? postEvaluationFinalizationTimeout = null,
        IDesignConversionPostEvaluationStateStore? postEvaluationStateStore = null,
        IDesignConversionRuntimeRejectedEvaluationSource? rejectedEvaluationSource = null)
        : this(
            evaluator,
            utcNow,
            artifactDb,
            visualCostLedger,
            visualCostBudgetStore,
            staleRunningTimeout,
            beforePostEvaluationOwnershipTransition: null,
            postEvaluationFinalizationTimeout,
            postEvaluationStateStore: postEvaluationStateStore,
            rejectedEvaluationSource: rejectedEvaluationSource)
    {
    }

    internal DesignConversionThinClientJobRuntime(
        IDesignConversionThinClientJobEvaluator evaluator,
        Func<DateTimeOffset>? utcNow,
        IDesignConversionLocalArtifactDb? artifactDb,
        IDesignConversionVisualCostLedger? visualCostLedger,
        IDesignConversionVisualCostBudgetStore? visualCostBudgetStore,
        TimeSpan? staleRunningTimeout,
        Func<Task>? beforePostEvaluationOwnershipTransition,
        TimeSpan? postEvaluationFinalizationTimeout = null,
        Func<TimeSpan, CancellationToken, Task>? postEvaluationFinalizationDelay = null,
        IDesignConversionPostEvaluationStateStore? postEvaluationStateStore = null,
        Action? beforeReservationReleaseStatusPublication = null,
        Action<DesignConversionPostEvaluationState, int>?
            afterAccountingReconciliationItemForTesting = null,
        Action<int>?
            afterAccountingReconciliationDrainSnapshotForTesting = null,
        IDesignConversionRuntimeRejectedEvaluationSource? rejectedEvaluationSource = null)
    {
        _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
        _staleRunningTimeout = staleRunningTimeout ?? DefaultStaleRunningTimeout;
        if (_staleRunningTimeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(
                nameof(staleRunningTimeout),
                "DesignConversion thin-client stale-running timeout must be positive.");
        _postEvaluationFinalizationTimeout =
            postEvaluationFinalizationTimeout ?? DefaultPostEvaluationFinalizationTimeout;
        if (_postEvaluationFinalizationTimeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(
                nameof(postEvaluationFinalizationTimeout),
                "DesignConversion thin-client post-evaluation finalization timeout must be positive.");

        _artifactDb = artifactDb;
        _visualCostLedger = visualCostLedger ?? JsonlDesignConversionVisualCostLedger.CreateDefault();
        _visualCostBudgetStore = visualCostBudgetStore;
        _rejectedEvaluationSource = rejectedEvaluationSource;
        _beforePostEvaluationOwnershipTransition = beforePostEvaluationOwnershipTransition;
        _beforeReservationReleaseStatusPublication =
            beforeReservationReleaseStatusPublication;
        _postEvaluationFinalizationDelay =
            postEvaluationFinalizationDelay ?? ((timeout, token) => Task.Delay(timeout, token));
        _postEvaluationStateStore = postEvaluationStateStore ??
            new JsonDesignConversionPostEvaluationStateStore(
                ResolvePostEvaluationStateRoot(artifactDb, visualCostBudgetStore));
        _afterAccountingReconciliationItemForTesting =
            afterAccountingReconciliationItemForTesting;
        _afterAccountingReconciliationDrainSnapshotForTesting =
            afterAccountingReconciliationDrainSnapshotForTesting;
    }

    public IDesignConversionLocalArtifactDb? LocalArtifactDb => _artifactDb;

    public IDesignConversionVisualCostLedger VisualCostLedger => _visualCostLedger;

    public IDesignConversionVisualCostBudgetStore? VisualCostBudgetStore => _visualCostBudgetStore;

    public IDesignConversionPostEvaluationStateStore PostEvaluationStateStore =>
        _postEvaluationStateStore;

    internal int LiveRunLeaseCount
    {
        get
        {
            lock (_gate)
                return _runLeases.Count;
        }
    }

    private static string ResolvePostEvaluationStateRoot(
        IDesignConversionLocalArtifactDb? artifactDb,
        IDesignConversionVisualCostBudgetStore? visualCostBudgetStore)
    {
        if (artifactDb is IDesignConversionPostEvaluationStateRootProvider artifactRootProvider)
            return Path.GetFullPath(artifactRootProvider.PostEvaluationStateRootDirectory);

        if (artifactDb is JsonlDesignConversionLocalArtifactDb jsonArtifactDb)
        {
            return Path.Combine(
                jsonArtifactDb.AllowedArtifactRootDirectory,
                JsonDesignConversionPostEvaluationStateStore.DirectoryName);
        }

        if (visualCostBudgetStore is IDesignConversionPostEvaluationStateRootProvider budgetRootProvider)
            return Path.GetFullPath(budgetRootProvider.PostEvaluationStateRootDirectory);

        if (visualCostBudgetStore is JsonlDesignConversionVisualCostBudgetStore jsonBudgetStore)
        {
            return Path.Combine(
                jsonBudgetStore.RootDirectory,
                JsonDesignConversionPostEvaluationStateStore.DirectoryName);
        }

        var localApplicationData = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localApplicationData) ||
            !Path.IsPathFullyQualified(localApplicationData))
        {
            throw new InvalidOperationException(
                "DesignConversion post-evaluation state requires a fully qualified durable root.");
        }

        return Path.Combine(
            localApplicationData,
            "llm_mvp_core",
            "design-conversion",
            JsonDesignConversionPostEvaluationStateStore.DirectoryName);
    }

    public async Task<OperationResult<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>> GetVisualCostRecordsAsync(
        string jobId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            return OperationResult<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>.Fail(
                "DesignConversion thin-client job id is required.",
                "JOB_ID_REQUIRED");

        if (cancellationToken.IsCancellationRequested)
            return VisualCostRecordsCancelled<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>();

        var evaluationJobRef = JsonlDesignConversionVisualCostLedger.BuildCanonicalEvaluationJobRef(jobId);
        OperationResult<IReadOnlyList<DesignConversionVisualCostLedgerRecord>> records;
        try
        {
            records = await _visualCostLedger.ListRecordsAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return VisualCostRecordsCancelled<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>();
        }
        catch (Exception)
        {
            return VisualCostRecordsReadFailed<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>();
        }

        if (!records.IsSuccess)
        {
            if (string.Equals(records.ErrorCode, "CANCELLED", StringComparison.Ordinal))
                return VisualCostRecordsCancelled<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>();

            return VisualCostRecordsReadFailed<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>();
        }

        // Unknown job ids intentionally return an empty set, avoiding a job-existence oracle.
        var matches = (records.Value ?? [])
            .Where(record => string.Equals(record.EvaluationJobRef, evaluationJobRef, StringComparison.Ordinal))
            .OrderBy(record => record.RecordedAtUtc)
            .ThenBy(record => record.RecordId, StringComparer.Ordinal)
            .ToArray();

        return OperationResult<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>.Ok(matches);
    }

    public async Task<OperationResult<int>> ReconcilePendingAccountingAsync(
        CancellationToken cancellationToken = default)
    {
        var batch = await ReconcilePendingAccountingBatchAsync(
            AccountingReconciliationDefaultMaximumItems,
            cancellationToken);
        return batch.IsSuccess
            ? OperationResult<int>.Ok(batch.ReconciledCount)
            : OperationResult<int>.Fail(
                batch.ErrorMessage,
                batch.ErrorCode);
    }

    public async Task<DesignConversionAccountingReconciliationBatchResult>
        ReconcilePendingAccountingBatchAsync(
            int maximumItems,
            CancellationToken cancellationToken = default)
    {
        if (maximumItems is <= 0 or > AccountingReconciliationMaximumAllowedItems)
        {
            return IncompleteAccountingReconciliationBatch(
                examinedCount: 0,
                reconciledCount: 0,
                failedCount: 0,
                remainingPendingCount: 0,
                lastErrorCode: AccountingReconciliationBatchSizeInvalidErrorCode,
                terminalErrorCode: AccountingReconciliationBatchSizeInvalidErrorCode);
        }

        if (IsAccountingReconciliationShutdownStarted())
        {
            return IncompleteAccountingReconciliationBatch(
                examinedCount: 0,
                reconciledCount: 0,
                failedCount: 0,
                remainingPendingCount: 0,
                lastErrorCode: AccountingReconciliationShutdownStartedErrorCode,
                terminalErrorCode: AccountingReconciliationShutdownStartedErrorCode);
        }

        if (_visualCostBudgetStore is null)
        {
            return IncompleteAccountingReconciliationBatch(
                examinedCount: 0,
                reconciledCount: 0,
                failedCount: 0,
                remainingPendingCount: 0,
                lastErrorCode: AccountingReconciliationBudgetStoreRequiredErrorCode,
                terminalErrorCode: AccountingReconciliationBudgetStoreRequiredErrorCode);
        }

        OperationResult<IReadOnlyList<DesignConversionPostEvaluationState>> listed;
        try
        {
            listed = await _postEvaluationStateStore.ListAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return IncompleteAccountingReconciliationBatch(
                examinedCount: 0,
                reconciledCount: 0,
                failedCount: 0,
                remainingPendingCount: 0,
                lastErrorCode: "CANCELLED",
                terminalErrorCode: "CANCELLED");
        }
        catch (Exception)
        {
            return IncompleteAccountingReconciliationBatch(
                examinedCount: 0,
                reconciledCount: 0,
                failedCount: 0,
                remainingPendingCount: 0,
                lastErrorCode:
                    JsonDesignConversionPostEvaluationStateStore.ReadFailedErrorCode,
                terminalErrorCode:
                    JsonDesignConversionPostEvaluationStateStore.ReadFailedErrorCode);
        }

        if (!listed.IsSuccess)
        {
            var listErrorCode = StableReconciliationItemError(listed.ErrorCode);
            return IncompleteAccountingReconciliationBatch(
                examinedCount: 0,
                reconciledCount: 0,
                failedCount: 0,
                remainingPendingCount: 0,
                lastErrorCode: listErrorCode,
                terminalErrorCode: listErrorCode);
        }

        var pending = listed.Value!
            .Where(state => string.Equals(
                state.AccountingStatus,
                JsonDesignConversionPostEvaluationStateStore.AccountingReconciliationRequired,
                StringComparison.Ordinal))
            .OrderBy(state => state.StateRef, StringComparer.Ordinal)
            .ToArray();
        var selected = pending.Take(maximumItems).ToArray();
        var examinedCount = 0;
        var reconciledCount = 0;
        var failedCount = 0;
        var lastErrorCode = string.Empty;
        foreach (var state in selected)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return IncompleteAccountingReconciliationBatch(
                    examinedCount,
                    reconciledCount,
                    failedCount,
                    pending.Length - reconciledCount,
                    lastErrorCode,
                    "CANCELLED");
            }

            examinedCount++;
            var reservation = BuildReconciliationReservationContext(state);
            var before = await ReadReservationSnapshotBoundedAsync(
                reservation,
                cancellationToken);
            if (IsCancelled(before, cancellationToken))
            {
                return IncompleteAccountingReconciliationBatch(
                    examinedCount,
                    reconciledCount,
                    failedCount,
                    pending.Length - reconciledCount,
                    lastErrorCode,
                    "CANCELLED");
            }

            if (!before.IsSuccess)
            {
                lastErrorCode = StableReconciliationItemError(before.ErrorCode);
                failedCount++;
                if (IsFatalAccountingReconciliationError(lastErrorCode))
                {
                    return IncompleteAccountingReconciliationBatch(
                        examinedCount,
                        reconciledCount,
                        failedCount,
                        pending.Length - reconciledCount,
                        lastErrorCode,
                        lastErrorCode);
                }

                var marker = await PreservePendingReconciliationFailureAsync(
                    state,
                    lastErrorCode,
                    cancellationToken);
                if (!marker.IsSuccess)
                {
                    var markerErrorCode =
                        StableReconciliationItemError(marker.ErrorCode);
                    return IncompleteAccountingReconciliationBatch(
                        examinedCount,
                        reconciledCount,
                        failedCount,
                        pending.Length - reconciledCount,
                        lastErrorCode,
                        markerErrorCode);
                }

                continue;
            }

            var durableStatus = ClassifyFinalAccountingStatus(before);
            if (durableStatus is null &&
                before.Value is not null &&
                string.Equals(
                    before.Value.Status,
                    JsonlDesignConversionVisualCostBudgetStore.StatusReserved,
                    StringComparison.Ordinal))
            {
                if (string.Equals(
                        state.AccountingDesiredAction,
                        JsonDesignConversionPostEvaluationStateStore.DesiredSettle,
                        StringComparison.Ordinal))
                {
                    var settle = await TrySettleReconciliationBoundedAsync(
                        state,
                        cancellationToken);
                    if (!settle.IsSuccess)
                    {
                        if (IsCancelled(settle, cancellationToken))
                        {
                            return IncompleteAccountingReconciliationBatch(
                                examinedCount,
                                reconciledCount,
                                failedCount,
                                pending.Length - reconciledCount,
                                lastErrorCode,
                                "CANCELLED");
                        }

                        lastErrorCode = StableReconciliationItemError(settle.ErrorCode);
                        failedCount++;
                        if (IsFatalAccountingReconciliationError(lastErrorCode))
                        {
                            return IncompleteAccountingReconciliationBatch(
                                examinedCount,
                                reconciledCount,
                                failedCount,
                                pending.Length - reconciledCount,
                                lastErrorCode,
                                lastErrorCode);
                        }

                        var marker = await PreservePendingReconciliationFailureAsync(
                            state,
                            lastErrorCode,
                            cancellationToken);
                        if (!marker.IsSuccess)
                        {
                            var markerErrorCode =
                                StableReconciliationItemError(marker.ErrorCode);
                            return IncompleteAccountingReconciliationBatch(
                                examinedCount,
                                reconciledCount,
                                failedCount,
                                pending.Length - reconciledCount,
                                lastErrorCode,
                                markerErrorCode);
                        }

                        continue;
                    }
                }
                else if (string.Equals(
                             state.AccountingDesiredAction,
                             JsonDesignConversionPostEvaluationStateStore.DesiredRelease,
                             StringComparison.Ordinal) &&
                         state.SettlementDefinitivelyNotApplied)
                {
                    var release = await ReleaseReservationBoundedAsync(
                        reservation,
                        cancellationToken);
                    if (!release.IsSuccess)
                    {
                        if (IsCancelled(release, cancellationToken))
                        {
                            return IncompleteAccountingReconciliationBatch(
                                examinedCount,
                                reconciledCount,
                                failedCount,
                                pending.Length - reconciledCount,
                                lastErrorCode,
                                "CANCELLED");
                        }

                        lastErrorCode = StableReconciliationItemError(release.ErrorCode);
                        failedCount++;
                        if (IsFatalAccountingReconciliationError(lastErrorCode))
                        {
                            return IncompleteAccountingReconciliationBatch(
                                examinedCount,
                                reconciledCount,
                                failedCount,
                                pending.Length - reconciledCount,
                                lastErrorCode,
                                lastErrorCode);
                        }

                        var marker = await PreservePendingReconciliationFailureAsync(
                            state,
                            lastErrorCode,
                            cancellationToken);
                        if (!marker.IsSuccess)
                        {
                            var markerErrorCode =
                                StableReconciliationItemError(marker.ErrorCode);
                            return IncompleteAccountingReconciliationBatch(
                                examinedCount,
                                reconciledCount,
                                failedCount,
                                pending.Length - reconciledCount,
                                lastErrorCode,
                                markerErrorCode);
                        }

                        continue;
                    }
                }
                else
                {
                    failedCount++;
                    lastErrorCode =
                        AccountingReconciliationReservationIdentityInvalidErrorCode;
                    return IncompleteAccountingReconciliationBatch(
                        examinedCount,
                        reconciledCount,
                        failedCount,
                        pending.Length - reconciledCount,
                        lastErrorCode,
                        lastErrorCode);
                }

                var after = await ReadReservationSnapshotBoundedAsync(
                    reservation,
                    cancellationToken);
                if (IsCancelled(after, cancellationToken))
                {
                    return IncompleteAccountingReconciliationBatch(
                        examinedCount,
                        reconciledCount,
                        failedCount,
                        pending.Length - reconciledCount,
                        lastErrorCode,
                        "CANCELLED");
                }

                if (!after.IsSuccess)
                {
                    lastErrorCode =
                        StableReconciliationItemError(after.ErrorCode);
                    failedCount++;
                    if (IsFatalAccountingReconciliationError(lastErrorCode))
                    {
                        return IncompleteAccountingReconciliationBatch(
                            examinedCount,
                            reconciledCount,
                            failedCount,
                            pending.Length - reconciledCount,
                            lastErrorCode,
                            lastErrorCode);
                    }

                    var marker = await PreservePendingReconciliationFailureAsync(
                        state,
                        lastErrorCode,
                        cancellationToken);
                    if (!marker.IsSuccess)
                    {
                        var markerErrorCode =
                            StableReconciliationItemError(marker.ErrorCode);
                        return IncompleteAccountingReconciliationBatch(
                            examinedCount,
                            reconciledCount,
                            failedCount,
                            pending.Length - reconciledCount,
                            lastErrorCode,
                            markerErrorCode);
                    }

                    continue;
                }

                durableStatus = ClassifyFinalAccountingStatus(after);
            }

            if (durableStatus is null)
            {
                failedCount++;
                lastErrorCode =
                    AccountingReconciliationReservationIdentityInvalidErrorCode;
                return IncompleteAccountingReconciliationBatch(
                    examinedCount,
                    reconciledCount,
                    failedCount,
                    pending.Length - reconciledCount,
                    lastErrorCode,
                    lastErrorCode);
            }

            var write = await WritePostEvaluationStateAsync(
                state with
                {
                    AccountingStatus = durableStatus,
                    AccountingDesiredAction =
                        JsonDesignConversionPostEvaluationStateStore.DesiredNone,
                    SettlementDefinitivelyNotApplied =
                        durableStatus ==
                        JsonDesignConversionPostEvaluationStateStore.AccountingReleased ||
                        state.SettlementDefinitivelyNotApplied,
                    ReleaseAttempted =
                        durableStatus ==
                        JsonDesignConversionPostEvaluationStateStore.AccountingReleased ||
                        state.ReleaseAttempted,
                     LastErrorCode = string.Empty,
                     UpdatedAtUtc = DateTimeOffset.UtcNow
                 },
                cancellationToken);
            if (write.IsSuccess)
            {
                reconciledCount++;
                _afterAccountingReconciliationItemForTesting?.Invoke(
                    write.Value!,
                    reconciledCount);
            }
            else
            {
                if (IsCancelled(write, cancellationToken))
                {
                    return IncompleteAccountingReconciliationBatch(
                        examinedCount,
                        reconciledCount,
                        failedCount,
                        pending.Length - reconciledCount,
                        lastErrorCode,
                        "CANCELLED");
                }

                lastErrorCode = StableReconciliationItemError(write.ErrorCode);
                failedCount++;
                return IncompleteAccountingReconciliationBatch(
                    examinedCount,
                    reconciledCount,
                    failedCount,
                    pending.Length - reconciledCount,
                    lastErrorCode,
                    lastErrorCode);
            }
        }

        return new DesignConversionAccountingReconciliationBatchResult(
            Completed: true,
            examinedCount,
            reconciledCount,
            failedCount,
            Math.Max(0, pending.Length - reconciledCount),
            lastErrorCode,
            TerminalErrorCode: string.Empty);
    }

    private async Task<OperationResult<DesignConversionVisualCostSettlementResult>>
        TrySettleReconciliationBoundedAsync(
            DesignConversionPostEvaluationState state,
            CancellationToken callerCancellationToken = default)
    {
        if (_visualCostBudgetStore is null)
        {
            return OperationResult<DesignConversionVisualCostSettlementResult>.Fail(
                AccountingReconciliationRequiredSafeMessage,
                AccountingReconciliationBudgetStoreRequiredErrorCode);
        }

        var bounded = await AwaitAccountingReconciliationStepAsync(
            token => _visualCostBudgetStore.SettleAsync(
                new DesignConversionVisualCostSettlementRequest(
                    state.ReservationRef,
                    state.UserRef,
                    state.PeriodStartUtc,
                    state.PeriodEndExclusiveUtc,
                    state.ChargedCostUsd,
                    DateTimeOffset.UtcNow),
                token),
            callerCancellationToken);
        return bounded.IsSuccess
            ? bounded.Value!
            : OperationResult<DesignConversionVisualCostSettlementResult>.Fail(
                AccountingReconciliationRequiredSafeMessage,
                bounded.ErrorCode ?? AccountingReconciliationItemFailedErrorCode);
    }

    private async Task<OperationResult<T>>
        AwaitAccountingReconciliationStepAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            CancellationToken callerCancellationToken)
    {
        if (callerCancellationToken.IsCancellationRequested)
        {
            return OperationResult<T>.Fail(
                AccountingReconciliationRequiredSafeMessage,
                "CANCELLED");
        }

        using var operationCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(callerCancellationToken);
        using var timeoutCancellation = new CancellationTokenSource();
        var timeoutTask = Task.Delay(
            _postEvaluationFinalizationTimeout,
            timeoutCancellation.Token);
        Task<T> operationTask;
        lock (_accountingReconciliationLateWorkGate)
        {
            if (_accountingReconciliationLateWorkDrainStarted)
            {
                timeoutCancellation.Cancel();
                return OperationResult<T>.Fail(
                    AccountingReconciliationRequiredSafeMessage,
                    AccountingReconciliationShutdownStartedErrorCode);
            }

            try
            {
                operationTask = operation(operationCancellation.Token);
            }
            catch (OperationCanceledException)
            {
                timeoutCancellation.Cancel();
                return OperationResult<T>.Fail(
                    AccountingReconciliationRequiredSafeMessage,
                    callerCancellationToken.IsCancellationRequested
                        ? "CANCELLED"
                        : PostEvaluationFinalizationCancelledErrorCode);
            }
            catch (Exception)
            {
                timeoutCancellation.Cancel();
                return OperationResult<T>.Fail(
                    AccountingReconciliationRequiredSafeMessage,
                    PostEvaluationFinalizationFailedErrorCode);
            }

            TrackAccountingReconciliationLateTaskUnderGate(operationTask);
        }

        Task completed;
        if (callerCancellationToken.CanBeCanceled)
        {
            var callerCancellationTask = Task.Delay(
                Timeout.InfiniteTimeSpan,
                callerCancellationToken);
            completed = await Task.WhenAny(
                operationTask,
                timeoutTask,
                callerCancellationTask);
        }
        else
        {
            completed = await Task.WhenAny(operationTask, timeoutTask);
        }

        if (ReferenceEquals(completed, operationTask))
        {
            timeoutCancellation.Cancel();
            try
            {
                return OperationResult<T>.Ok(
                    await operationTask.ConfigureAwait(false));
            }
            catch (OperationCanceledException)
            {
                return OperationResult<T>.Fail(
                    AccountingReconciliationRequiredSafeMessage,
                    callerCancellationToken.IsCancellationRequested
                        ? "CANCELLED"
                        : PostEvaluationFinalizationCancelledErrorCode);
            }
            catch (Exception)
            {
                return OperationResult<T>.Fail(
                    AccountingReconciliationRequiredSafeMessage,
                    PostEvaluationFinalizationFailedErrorCode);
            }
        }

        operationCancellation.Cancel();
        if (callerCancellationToken.IsCancellationRequested)
        {
            return OperationResult<T>.Fail(
                AccountingReconciliationRequiredSafeMessage,
                "CANCELLED");
        }

        return OperationResult<T>.Fail(
            AccountingReconciliationRequiredSafeMessage,
            PostEvaluationFinalizationTimeoutErrorCode);
    }

    private void TrackAccountingReconciliationLateTaskUnderGate(Task task)
    {
        var id = Interlocked.Increment(
            ref _nextAccountingReconciliationLateTaskId);
        _accountingReconciliationLateTasks[id] = task;
        _ = task.ContinueWith(
            completed =>
            {
                try
                {
                    _ = completed.Exception;
                }
                finally
                {
                    _accountingReconciliationLateTasks.TryRemove(id, out _);
                }
            },
            TaskContinuationOptions.ExecuteSynchronously);
    }

    public async Task<OperationResult<int>>
        DrainAccountingReconciliationLateWorkAsync(
            TimeSpan maximumWait,
            CancellationToken cancellationToken = default)
    {
        if (maximumWait <= TimeSpan.Zero)
        {
            return OperationResult<int>.Fail(
                AccountingReconciliationRequiredSafeMessage,
                AccountingReconciliationLateWorkDrainTimeoutErrorCode);
        }

        Task[] snapshot;
        lock (_accountingReconciliationLateWorkGate)
        {
            _accountingReconciliationLateWorkDrainStarted = true;
            RemoveCompletedAccountingReconciliationTasks();
            snapshot = _accountingReconciliationLateTasks.Values.ToArray();
        }
        _afterAccountingReconciliationDrainSnapshotForTesting?.Invoke(
            snapshot.Length);

        if (cancellationToken.IsCancellationRequested)
        {
            return OperationResult<int>.Fail(
                AccountingReconciliationRequiredSafeMessage,
                "CANCELLED");
        }

        if (snapshot.Length == 0)
            return OperationResult<int>.Ok(0);

        var all = ObserveAccountingReconciliationTasksAsync(snapshot);
        try
        {
            await all.WaitAsync(maximumWait, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return OperationResult<int>.Fail(
                AccountingReconciliationRequiredSafeMessage,
                "CANCELLED");
        }
        catch (TimeoutException)
        {
            return OperationResult<int>.Fail(
                AccountingReconciliationRequiredSafeMessage,
                AccountingReconciliationLateWorkDrainTimeoutErrorCode);
        }

        lock (_accountingReconciliationLateWorkGate)
        {
            RemoveCompletedAccountingReconciliationTasks();
            return _accountingReconciliationLateTasks.Count == 0
                ? OperationResult<int>.Ok(0)
                : OperationResult<int>.Fail(
                    AccountingReconciliationRequiredSafeMessage,
                    AccountingReconciliationLateWorkDrainTimeoutErrorCode);
        }
    }

    private bool IsAccountingReconciliationShutdownStarted()
    {
        lock (_accountingReconciliationLateWorkGate)
            return _accountingReconciliationLateWorkDrainStarted;
    }

    private void RemoveCompletedAccountingReconciliationTasks()
    {
        foreach (var pair in _accountingReconciliationLateTasks)
        {
            if (!pair.Value.IsCompleted)
                continue;

            _ = pair.Value.Exception;
            _accountingReconciliationLateTasks.TryRemove(pair.Key, out _);
        }
    }

    internal int AccountingReconciliationTrackedWorkCount =>
        _accountingReconciliationLateTasks.Count;

    internal bool TryTrackAccountingReconciliationWorkForTesting(Task task)
    {
        ArgumentNullException.ThrowIfNull(task);
        lock (_accountingReconciliationLateWorkGate)
        {
            if (_accountingReconciliationLateWorkDrainStarted)
                return false;

            TrackAccountingReconciliationLateTaskUnderGate(task);
            return true;
        }
    }

    private static async Task ObserveAccountingReconciliationTasksAsync(
        IReadOnlyList<Task> tasks)
    {
        foreach (var task in tasks)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch
            {
                _ = task.Exception;
            }
        }
    }

    private async Task<OperationResult<DesignConversionPostEvaluationState>>
        PreservePendingReconciliationFailureAsync(
        DesignConversionPostEvaluationState state,
        string errorCode,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return OperationResult<DesignConversionPostEvaluationState>.Fail(
                AccountingReconciliationRequiredSafeMessage,
                "CANCELLED");
        }

        return await WritePostEvaluationStateAsync(
            state with
            {
                LastErrorCode = errorCode,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            },
            cancellationToken);
    }

    private static string StableReconciliationItemError(string? errorCode)
        => string.IsNullOrWhiteSpace(errorCode)
            ? AccountingReconciliationItemFailedErrorCode
            : errorCode.Trim();

    private static bool IsFatalAccountingReconciliationError(string errorCode)
        => errorCode is
            JsonDesignConversionPostEvaluationStateStore.ReadFailedErrorCode or
            JsonDesignConversionPostEvaluationStateStore.TrustBoundaryErrorCode or
            AccountingReconciliationShutdownStartedErrorCode or
            JsonDesignConversionPostEvaluationStateStore.ConflictErrorCode or
            JsonDesignConversionPostEvaluationStateStore.PublicationCleanupRequiredErrorCode or
            JsonDesignConversionPostEvaluationStateStore.LegacyCompletionBindingUnverifiableErrorCode or
            JsonlDesignConversionVisualCostBudgetStore.InvalidRequestErrorCode or
            JsonlDesignConversionVisualCostBudgetStore.IdempotencyConflictErrorCode or
            JsonlDesignConversionVisualCostBudgetStore.PeriodAuthorityConflictErrorCode or
            JsonlDesignConversionVisualCostBudgetStore.ReservationNotFoundErrorCode or
            JsonlDesignConversionVisualCostBudgetStore.InvalidTransitionErrorCode or
            JsonlDesignConversionVisualCostBudgetStore.SettlementConflictErrorCode or
            JsonlDesignConversionVisualCostBudgetStore.SettlementExceedsReservationErrorCode or
            JsonlDesignConversionVisualCostBudgetStore.ReadFailedErrorCode or
            AccountingReconciliationReservationIdentityInvalidErrorCode;

    private static DesignConversionAccountingReconciliationBatchResult
        IncompleteAccountingReconciliationBatch(
            int examinedCount,
            int reconciledCount,
            int failedCount,
            int remainingPendingCount,
            string lastErrorCode,
            string terminalErrorCode)
        => new(
            Completed: false,
            examinedCount,
            reconciledCount,
            failedCount,
            Math.Max(0, remainingPendingCount),
            lastErrorCode,
            terminalErrorCode);

    private static bool IsCancelled<T>(
        OperationResult<T> result,
        CancellationToken cancellationToken)
        => cancellationToken.IsCancellationRequested ||
           string.Equals(result.ErrorCode, "CANCELLED", StringComparison.Ordinal) ||
           string.Equals(
               result.ErrorCode,
               PostEvaluationFinalizationCancelledErrorCode,
               StringComparison.Ordinal);

    private static string? ClassifyFinalAccountingStatus(
        OperationResult<DesignConversionVisualCostReservationSnapshot?> query)
    {
        if (!query.IsSuccess || query.Value is null)
            return null;
        if (string.Equals(
                query.Value.Status,
                JsonlDesignConversionVisualCostBudgetStore.StatusSettled,
                StringComparison.Ordinal))
        {
            return JsonDesignConversionPostEvaluationStateStore.AccountingSettled;
        }

        if (string.Equals(
                query.Value.Status,
                JsonlDesignConversionVisualCostBudgetStore.StatusReleased,
                StringComparison.Ordinal))
        {
            return JsonDesignConversionPostEvaluationStateStore.AccountingReleased;
        }

        return null;
    }

    private static VisualCostBudgetReservationContext BuildReconciliationReservationContext(
        DesignConversionPostEvaluationState state)
        => new(
            new DesignConversionVisualCostReservationRequest(
                state.ReservationRef,
                state.UserRef,
                SubmittedTier: "reconciliation",
                state.PeriodStartUtc,
                state.PeriodEndExclusiveUtc,
                MonthlyLimitUsd: Math.Max(state.ChargedCostUsd, 0.0001m),
                RequestedReserveUsd: Math.Max(state.ChargedCostUsd, 0.0001m),
                state.UpdatedAtUtc),
            Reservation: null);

    public OperationResult<DesignConversionThinClientJob> Submit(SubmittedDesignConversionRequest request)
    {
        var validation = ValidateSubmission(request);
        if (!validation.IsSuccess)
            return validation.MapError<DesignConversionThinClientJob>();

        var now = _utcNow();
        var jobId = BuildJobId(request.RequestId);
        var job = new DesignConversionThinClientJob(
            JobId: jobId,
            RequestId: request.RequestId,
            Status: DesignConversionThinClientStatus.Submitted,
            StatusUri: BuildStatusUri(jobId),
            SubmittedAtUtc: now,
            UpdatedAtUtc: null,
            Metadata: BuildJobMetadata(request, status: DesignConversionThinClientStatus.Submitted, errorCode: null));
        var status = BuildStatusResult(
            job,
            DesignConversionThinClientStatus.Submitted,
            "In-process thin-client job submitted; explicit RunAsync is required before internal execution.",
            artifacts: [],
            extraWarnings: [],
            BuildStatusProvenance(request));

        lock (_gate)
        {
            if (_jobs.ContainsKey(jobId))
                return OperationResult<DesignConversionThinClientJob>.Fail(
                    $"DesignConversion thin-client job already exists for request '{request.RequestId}'.",
                    "JOB_ALREADY_EXISTS");

            _requests[jobId] = request;
            _jobs[jobId] = job;
            _statuses[jobId] = status;
        }

        return OperationResult<DesignConversionThinClientJob>.Ok(job);
    }

    public OperationResult<DesignConversionThinClientStatusResult> GetStatus(string jobId)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            return OperationResult<DesignConversionThinClientStatusResult>.Fail(
                "DesignConversion thin-client job id is required.",
                "JOB_ID_REQUIRED");

        ReservationReleaseWork? releaseWork = null;
        OperationResult<DesignConversionThinClientStatusResult> result;
        lock (_gate)
        {
            if (_statuses.TryGetValue(jobId, out var status))
            {
                result = TryFailClosedStaleRunningNoLock(jobId, out var staleStatus, out releaseWork)
                    ? OperationResult<DesignConversionThinClientStatusResult>.Ok(staleStatus)
                    : OperationResult<DesignConversionThinClientStatusResult>.Ok(status);
            }
            else
            {
                result = OperationResult<DesignConversionThinClientStatusResult>.Fail(
                    $"DesignConversion thin-client job '{jobId}' was not found.",
                    "JOB_NOT_FOUND");
            }
        }

        if (releaseWork is not null)
            StartReservationRelease(releaseWork);

        return result;
    }

    public async Task<OperationResult<DesignConversionThinClientStatusResult>> RunAsync(
        string jobId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            return OperationResult<DesignConversionThinClientStatusResult>.Fail(
                "DesignConversion thin-client job id is required.",
                "JOB_ID_REQUIRED");

        SubmittedDesignConversionRequest request = null!;
        DesignConversionThinClientJob runningJob = null!;
        var attemptId = string.Empty;
        ReservationReleaseWork? staleReleaseWork = null;
        OperationResult<DesignConversionThinClientStatusResult>? staleResult = null;
        lock (_gate)
        {
            if (!_jobs.TryGetValue(jobId, out var currentJob) || !_requests.TryGetValue(jobId, out var storedRequest))
            {
                return OperationResult<DesignConversionThinClientStatusResult>.Fail(
                    $"DesignConversion thin-client job '{jobId}' was not found.",
                    "JOB_NOT_FOUND");
            }

            request = storedRequest;

            if (TryFailClosedStaleRunningNoLock(jobId, out var staleStatus, out staleReleaseWork))
            {
                staleResult = OperationResult<DesignConversionThinClientStatusResult>.Ok(staleStatus);
            }
            else if (string.Equals(currentJob.Status, DesignConversionThinClientStatus.Running, StringComparison.Ordinal))
            {
                return OperationResult<DesignConversionThinClientStatusResult>.Fail(
                    $"DesignConversion thin-client job '{jobId}' is already running.",
                    "JOB_ALREADY_RUNNING");
            }
            else if (IsTerminalStatus(currentJob.Status))
            {
                return OperationResult<DesignConversionThinClientStatusResult>.Ok(_statuses[jobId]);
            }
            else
            {
                attemptId = Guid.NewGuid().ToString("N");
                runningJob = UpdateJob(currentJob, DesignConversionThinClientStatus.Running, errorCode: null);
                _jobs[jobId] = runningJob;
                _statuses[jobId] = BuildStatusResult(
                    runningJob,
                    DesignConversionThinClientStatus.Running,
                    "In-process thin-client job is running through the injected internal evaluator.",
                    artifacts: [],
                    extraWarnings: [],
                    BuildStatusProvenance(request));
                _runLeases[jobId] = new JobRunLease(attemptId);
            }
        }

        if (staleReleaseWork is not null)
            StartReservationRelease(staleReleaseWork);
        if (staleResult is not null)
            return staleResult;

        var internalRequest = BuildInternalEvaluateRequest(request, runningJob);
        OperationResult<DesignConversionInternalEvaluationResult> evaluation;
        VisualCostBudgetReservationContext? visualCostReservation = null;
        OperationResult<VisualCostBudgetSettlementContext>? visualCostSettlement = null;
        var evaluatorInvoked = false;
        if (!internalRequest.IsSuccess)
        {
            evaluation = internalRequest.MapError<DesignConversionInternalEvaluationResult>();
        }
        else
        {
            var reservation = await TryReserveVisualCostBudgetAsync(request, runningJob, cancellationToken);
            if (!reservation.IsSuccess)
            {
                evaluation = reservation.MapError<DesignConversionInternalEvaluationResult>();
            }
            else
            {
                visualCostReservation = reservation.Value;
                if (visualCostReservation is not null &&
                    !TryAttachReservationToLease(
                        jobId,
                        attemptId,
                        visualCostReservation,
                        out var lostOwnershipRelease,
                        out var lostOwnershipReleaseCompletion))
                {
                    return await ReturnTerminalAfterOwnershipLostAsync(
                        jobId,
                        lostOwnershipRelease,
                        lostOwnershipReleaseCompletion);
                }

                var preEvaluationState = visualCostReservation is null
                    ? null
                    : await PreparePostEvaluationStateAsync(
                        jobId,
                        attemptId,
                        visualCostReservation,
                        OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                            "Evaluation has not started.",
                            "EVALUATION_NOT_STARTED"),
                        CancellationToken.None);
                if (preEvaluationState is { IsSuccess: false })
                {
                    evaluation = OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                        PostEvaluationFinalizationFailedSafeMessage,
                        SafePersistenceErrorCode(preEvaluationState.ErrorCode));
                }
                else
                {
                    try
                    {
                        evaluatorInvoked = true;
                        // Production submit requests carry repository-owned DPO
                        // transaction authority outside the legacy internal DTO,
                        // so context-aware evaluators receive it explicitly while
                        // older evaluators continue to run in disabled mode.
                        evaluation = _evaluator is
                            IDesignConversionRepositoryOwnedDpoTransactionContextAwareEvaluator
                                repositoryOwnedDpoEvaluator
                            ? await repositoryOwnedDpoEvaluator.EvaluateAsync(
                                internalRequest.Value!,
                                request.RepositoryOwnedDpoTransactionContext,
                                cancellationToken)
                            : await _evaluator.EvaluateAsync(
                                internalRequest.Value!,
                                cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        evaluation = OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                            "DesignConversion thin-client job run cancelled.",
                            "CANCELLED");
                    }
                    catch (Exception ex)
                    {
                        evaluation = OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                            ex.Message,
                            "THIN_CLIENT_JOB_RUNTIME_EXCEPTION");
                    }
                }
            }
        }

        if (_beforePostEvaluationOwnershipTransition is not null)
            await _beforePostEvaluationOwnershipTransition();

        if (!TryAcquirePostEvaluationOwnership(
                jobId,
                attemptId,
                out var postEvaluationRelease,
                out var postEvaluationReleaseCompletion))
        {
            return await ReturnTerminalAfterOwnershipLostAsync(
                jobId,
                postEvaluationRelease,
                postEvaluationReleaseCompletion);
        }

        // Once the evaluator has been invoked, accounting finalization owns its
        // own bounded lifetime. Caller cancellation must not strand a reserved
        // budget; the explicit deadline below remains the cancellation authority.
        using var finalizationCancellation = new CancellationTokenSource();
        var finalizationDeadline = CreatePostEvaluationFinalizationDeadline();
        var preparedState = await PreparePostEvaluationStateAsync(
            jobId,
            attemptId,
            visualCostReservation,
            evaluation,
            CancellationToken.None);
        if (!preparedState.IsSuccess)
        {
            return CompletePostEvaluationTerminalNoRelease(
                jobId,
                attemptId,
                runningJob,
                request,
                visualCostReservation,
                PostEvaluationFinalizationFailedSafeMessage,
                SafePersistenceErrorCode(preparedState.ErrorCode),
                stateRef: null,
                accountingReconciliationRequired: visualCostReservation is not null);
        }

        var postEvaluationState = preparedState.Value!;

        if (visualCostReservation is not null && !evaluatorInvoked)
        {
            return await ReconcileAndCompletePostEvaluationAccountingFailureAsync(
                jobId,
                attemptId,
                runningJob,
                request,
                visualCostReservation,
                postEvaluationState,
                settlementDefinitivelyNotApplied: true,
                evaluation.ErrorMessage ?? PostEvaluationFinalizationFailedSafeMessage,
                evaluation.ErrorCode ?? PostEvaluationFinalizationFailedErrorCode,
                finalizationDeadline,
                finalizationCancellation);
        }

        if (visualCostReservation is not null && evaluatorInvoked)
        {
            var attemptedState = await WritePostEvaluationStateAsync(
                postEvaluationState with
                {
                    AccountingDesiredAction =
                        JsonDesignConversionPostEvaluationStateStore.DesiredSettle,
                    SettlementAttempted = true,
                    UpdatedAtUtc = DateTimeOffset.UtcNow
                },
                CancellationToken.None);
            if (attemptedState.IsSuccess)
                postEvaluationState = attemptedState.Value!;

            var boundedSettlement = await AwaitPostEvaluationFinalizationStepAsync(
                token => SettleVisualCostBudgetAsync(visualCostReservation, evaluation, token),
                finalizationDeadline,
                finalizationCancellation,
                CancellationToken.None);
            if (!boundedSettlement.IsSuccess)
            {
                return await ReconcileAndCompletePostEvaluationAccountingFailureAsync(
                    jobId,
                    attemptId,
                    runningJob,
                    request,
                    visualCostReservation,
                    postEvaluationState,
                    settlementDefinitivelyNotApplied: false,
                    boundedSettlement.ErrorMessage ?? PostEvaluationFinalizationFailedSafeMessage,
                    boundedSettlement.ErrorCode ?? PostEvaluationFinalizationFailedErrorCode,
                    finalizationDeadline,
                    finalizationCancellation);
            }

            visualCostSettlement = boundedSettlement.Value!;
            if (!visualCostSettlement.IsSuccess)
            {
                postEvaluationState = (await WritePostEvaluationStateAsync(
                    postEvaluationState with
                    {
                        AccountingDesiredAction =
                            JsonDesignConversionPostEvaluationStateStore.DesiredRelease,
                        SettlementDefinitivelyNotApplied = true,
                        LastErrorCode = SafeVisualCostBudgetErrorCode(
                            visualCostSettlement.ErrorCode,
                            TrustedVisualCostBudgetSettlementFailedErrorCode),
                        UpdatedAtUtc = DateTimeOffset.UtcNow
                    },
                    CancellationToken.None)).Value ?? postEvaluationState;
                return await ReconcileAndCompletePostEvaluationAccountingFailureAsync(
                    jobId,
                    attemptId,
                    runningJob,
                    request,
                    visualCostReservation,
                    postEvaluationState,
                    settlementDefinitivelyNotApplied: true,
                    visualCostSettlement.ErrorMessage ?? PostEvaluationFinalizationFailedSafeMessage,
                    visualCostSettlement.ErrorCode ?? PostEvaluationFinalizationFailedErrorCode,
                    finalizationDeadline,
                    finalizationCancellation);
            }

            var settledState = await WritePostEvaluationStateAsync(
                postEvaluationState with
                {
                    AccountingStatus =
                        JsonDesignConversionPostEvaluationStateStore.AccountingSettled,
                    AccountingDesiredAction =
                        JsonDesignConversionPostEvaluationStateStore.DesiredNone,
                    LastErrorCode = string.Empty,
                    UpdatedAtUtc = DateTimeOffset.UtcNow
                },
                CancellationToken.None);
            if (!settledState.IsSuccess)
            {
                return await ReconcileAndCompletePostEvaluationAccountingFailureAsync(
                    jobId,
                    attemptId,
                    runningJob,
                    request,
                    visualCostReservation,
                    postEvaluationState,
                    settlementDefinitivelyNotApplied: false,
                    PostEvaluationFinalizationFailedSafeMessage,
                    SafePersistenceErrorCode(settledState.ErrorCode),
                    finalizationDeadline,
                    finalizationCancellation);
            }

            postEvaluationState = settledState.Value!;
            if (!TryMarkReservationSettled(jobId, attemptId))
            {
                return await ReturnTerminalAfterOwnershipLostAsync(
                    jobId,
                    releaseWork: null,
                    releaseCompletion: null);
            }
        }

        DesignConversionThinClientStatusResult terminalStatus;
        if (evaluation.IsSuccess)
        {
            if (visualCostSettlement?.Value?.FailJob == true)
            {
                terminalStatus = BuildFailedStatus(
                    runningJob,
                    request,
                    visualCostSettlement.Value.FailureMessage,
                    visualCostSettlement.Value.FailureCode);
            }
            else if (TryGetKnownBrowserFailureFromEvaluation(evaluation.Value!, out var browserFailureCode))
            {
                terminalStatus = BuildFailedStatus(
                    runningJob,
                    request,
                    string.Empty,
                    browserFailureCode);
            }
            else
            {
                var completedStatus = BuildCompletedStatus(runningJob, request, evaluation.Value!);
                var carriedTeacherCallState = ResolveCarriedTeacherCallState(
                    request.Budget,
                    evaluation.Value!);
                var teacherCallUsage = new DesignConversionTeacherCallUsageAccumulator(
                    request.Budget.MaxTeacherCalls,
                    carriedTeacherCallState.PersistedObservedCallCount,
                    carriedTeacherCallState.IsValid);
                var boundedPersistence = await AwaitPostEvaluationFinalizationStepAsync(
                    token => PersistCompletedEvaluationAsync(
                        runningJob,
                        request,
                        completedStatus,
                        evaluation.Value!,
                        postEvaluationState,
                        finalizationDeadline,
                        teacherCallUsage,
                        token),
                    finalizationDeadline,
                    finalizationCancellation,
                    cancellationToken);
                if (boundedPersistence.IsSuccess)
                {
                    terminalStatus = boundedPersistence.Value!;
                }
                else
                {
                    var durablePersistence = await _postEvaluationStateStore.ReadAsync(
                        postEvaluationState.StateRef,
                        CancellationToken.None);
                    terminalStatus = durablePersistence.IsSuccess &&
                                     durablePersistence.Value is not null &&
                                     string.Equals(
                                         durablePersistence.Value.PersistenceStatus,
                                         JsonDesignConversionPostEvaluationStateStore.PersistenceCommitted,
                                         StringComparison.Ordinal)
                        ? completedStatus
                        : MarkPostEvaluationFinalizationFailure(
                            BuildFailedStatus(
                                runningJob,
                                request,
                                boundedPersistence.ErrorMessage ?? PostEvaluationFinalizationFailedSafeMessage,
                                boundedPersistence.ErrorCode ?? PostEvaluationFinalizationFailedErrorCode),
                            boundedPersistence.ErrorCode ?? PostEvaluationFinalizationFailedErrorCode);
                }

                terminalStatus = ApplyRuntimeTeacherCallBudgetMetadata(
                    terminalStatus,
                    request.Budget,
                    teacherCallUsage.ActualObservedCallCount);
            }
        }
        else
        {
            terminalStatus = BuildFailedStatus(runningJob, request, evaluation.ErrorMessage, evaluation.ErrorCode);
        }

        terminalStatus = MarkVisualCostBudgetReservation(terminalStatus, visualCostReservation, visualCostSettlement?.Value);
        DiscardPendingRejectedEvaluation(jobId);

        lock (_gate)
        {
            if (!_runLeases.TryGetValue(jobId, out var lease) ||
                !string.Equals(lease.AttemptId, attemptId, StringComparison.Ordinal) ||
                lease.Phase != JobRunLeasePhase.PostEvaluationOwned)
            {
                return _statuses.TryGetValue(jobId, out var existingStatus)
                    ? OperationResult<DesignConversionThinClientStatusResult>.Ok(existingStatus)
                    : OperationResult<DesignConversionThinClientStatusResult>.Fail(
                        $"DesignConversion thin-client job '{jobId}' lost its terminal status.",
                        "JOB_STATUS_LOST");
            }

            _jobs[jobId] = terminalStatus.Job;
            _statuses[jobId] = terminalStatus;
            lease.Phase = JobRunLeasePhase.Finished;
            _runLeases.Remove(jobId);
        }

        return OperationResult<DesignConversionThinClientStatusResult>.Ok(terminalStatus);
    }

    private Task CreatePostEvaluationFinalizationDeadline()
    {
        try
        {
            return _postEvaluationFinalizationDelay(
                       _postEvaluationFinalizationTimeout,
                       CancellationToken.None)
                   ?? Task.FromException(
                       new InvalidOperationException(
                           "DesignConversion post-evaluation finalization deadline factory returned no task."));
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }

    private static async Task<OperationResult<T>> AwaitPostEvaluationFinalizationStepAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        Task deadline,
        CancellationTokenSource finalizationCancellation,
        CancellationToken callerCancellationToken)
    {
        if (callerCancellationToken.IsCancellationRequested)
        {
            finalizationCancellation.Cancel();
            return OperationResult<T>.Fail(
                PostEvaluationFinalizationCancelledSafeMessage,
                PostEvaluationFinalizationCancelledErrorCode);
        }

        if (deadline.IsCompleted)
        {
            finalizationCancellation.Cancel();
            if (deadline.IsFaulted)
            {
                _ = deadline.Exception;
                return OperationResult<T>.Fail(
                    PostEvaluationFinalizationFailedSafeMessage,
                    PostEvaluationFinalizationFailedErrorCode);
            }

            return OperationResult<T>.Fail(
                PostEvaluationFinalizationTimeoutSafeMessage,
                PostEvaluationFinalizationTimeoutErrorCode);
        }

        Task<T> operationTask;
        try
        {
            operationTask = operation(finalizationCancellation.Token);
        }
        catch (OperationCanceledException)
        {
            return OperationResult<T>.Fail(
                PostEvaluationFinalizationCancelledSafeMessage,
                PostEvaluationFinalizationCancelledErrorCode);
        }
        catch (Exception)
        {
            return OperationResult<T>.Fail(
                PostEvaluationFinalizationFailedSafeMessage,
                PostEvaluationFinalizationFailedErrorCode);
        }

        var callerCancellation = callerCancellationToken.CanBeCanceled
            ? Task.Delay(Timeout.InfiniteTimeSpan, callerCancellationToken)
            : Task.Delay(Timeout.InfiniteTimeSpan);
        var completed = await Task.WhenAny(operationTask, deadline, callerCancellation);
        if (ReferenceEquals(completed, operationTask))
        {
            try
            {
                return OperationResult<T>.Ok(await operationTask);
            }
            catch (OperationCanceledException)
            {
                return OperationResult<T>.Fail(
                    PostEvaluationFinalizationCancelledSafeMessage,
                    PostEvaluationFinalizationCancelledErrorCode);
            }
            catch (Exception)
            {
                return OperationResult<T>.Fail(
                    PostEvaluationFinalizationFailedSafeMessage,
                    PostEvaluationFinalizationFailedErrorCode);
            }
        }

        finalizationCancellation.Cancel();
        ObserveLateFinalizationTask(operationTask);
        if (ReferenceEquals(completed, callerCancellation))
        {
            return OperationResult<T>.Fail(
                PostEvaluationFinalizationCancelledSafeMessage,
                PostEvaluationFinalizationCancelledErrorCode);
        }

        if (deadline.IsFaulted)
        {
            _ = deadline.Exception;
            return OperationResult<T>.Fail(
                PostEvaluationFinalizationFailedSafeMessage,
                PostEvaluationFinalizationFailedErrorCode);
        }

        return OperationResult<T>.Fail(
            PostEvaluationFinalizationTimeoutSafeMessage,
            PostEvaluationFinalizationTimeoutErrorCode);
    }

    private static void ObserveLateFinalizationTask(Task task)
        => _ = task.ContinueWith(
            completed => _ = completed.Exception,
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);

    private bool TryMarkReservationSettled(string jobId, string attemptId)
    {
        lock (_gate)
        {
            if (!_runLeases.TryGetValue(jobId, out var lease) ||
                !string.Equals(lease.AttemptId, attemptId, StringComparison.Ordinal) ||
                lease.Phase != JobRunLeasePhase.PostEvaluationOwned ||
                lease.Reservation is null)
            {
                return false;
            }

            lease.ReservationFinalized = true;
            return true;
        }
    }

    private async Task<OperationResult<DesignConversionPostEvaluationState>> PreparePostEvaluationStateAsync(
        string jobId,
        string attemptId,
        VisualCostBudgetReservationContext? reservation,
        OperationResult<DesignConversionInternalEvaluationResult> evaluation,
        CancellationToken cancellationToken)
    {
        // A reservation is the durable accounting idempotency identity. Runs
        // without a reservation have no accounting replay to reconcile and
        // therefore use the attempt identity; reusing jobId would alias
        // unrelated persistence attempts across process lifetimes.
        var idempotencySource = reservation?.Request.ReservationRef ??
                                attemptId;
        var stateRef = JsonDesignConversionPostEvaluationStateStore.BuildStateRef(idempotencySource);
        var plan = reservation is null
            ? new VisualCostBudgetSettlementPlan(
                0m,
                ActualCostKnown: false,
                FailJob: false,
                string.Empty,
                string.Empty)
            : BuildVisualCostBudgetSettlementPlan(reservation, evaluation);
        var state = new DesignConversionPostEvaluationState(
            StateRef: stateRef,
            ReservationRef: reservation?.Request.ReservationRef ?? stateRef,
            UserRef: reservation?.Request.UserRef ??
                     JsonDesignConversionPostEvaluationStateStore.NoReservationUserRef,
            PeriodStartUtc: reservation?.Request.PeriodStartUtc ??
                            JsonDesignConversionPostEvaluationStateStore
                                .NoReservationPeriodStartUtc,
            PeriodEndExclusiveUtc: reservation?.Request.PeriodEndExclusiveUtc ??
                                   JsonDesignConversionPostEvaluationStateStore
                                       .NoReservationPeriodEndExclusiveUtc,
            ChargedCostUsd: plan.ChargedCostUsd,
            AccountingStatus: reservation is null
                ? JsonDesignConversionPostEvaluationStateStore.AccountingSettled
                : JsonDesignConversionPostEvaluationStateStore.AccountingPending,
            AccountingDesiredAction: reservation is null
                ? JsonDesignConversionPostEvaluationStateStore.DesiredNone
                : JsonDesignConversionPostEvaluationStateStore.DesiredSettle,
            SettlementAttempted: false,
            SettlementDefinitivelyNotApplied: false,
            ReleaseAttempted: false,
            PersistenceStatus: JsonDesignConversionPostEvaluationStateStore.PersistenceProvisional,
            LedgerRecordId: string.Empty,
            ArtifactRecordId: string.Empty,
            CompletionBindingKind: string.Empty,
            CompletionBindingRef: string.Empty,
            LastErrorCode: string.Empty,
            UpdatedAtUtc: DateTimeOffset.UtcNow,
            Version: 1L);
        return await WritePostEvaluationStateAsync(state, cancellationToken);
    }

    private async Task<OperationResult<DesignConversionPostEvaluationState>> WritePostEvaluationStateAsync(
        DesignConversionPostEvaluationState state,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _postEvaluationStateStore.WriteAsync(state, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return OperationResult<DesignConversionPostEvaluationState>.Fail(
                PostEvaluationFinalizationCancelledSafeMessage,
                PostEvaluationFinalizationCancelledErrorCode);
        }
        catch (Exception)
        {
            return OperationResult<DesignConversionPostEvaluationState>.Fail(
                PostEvaluationFinalizationFailedSafeMessage,
                JsonDesignConversionPostEvaluationStateStore.WriteFailedErrorCode);
        }
    }

    private async Task<OperationResult<DesignConversionPostEvaluationState>> CommitPostEvaluationPersistenceAsync(
        DesignConversionPostEvaluationState state,
        string ledgerRecordId,
        string artifactRecordId,
        Task finalizationDeadline,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested || finalizationDeadline.IsCompleted)
        {
            return OperationResult<DesignConversionPostEvaluationState>.Fail(
                PostEvaluationFinalizationTimeoutSafeMessage,
                PostEvaluationFinalizationTimeoutErrorCode);
        }

        return await WritePostEvaluationStateAsync(
            state with
            {
                PersistenceStatus =
                    JsonDesignConversionPostEvaluationStateStore.PersistenceCommitted,
                LedgerRecordId = ledgerRecordId,
                ArtifactRecordId = artifactRecordId,
                CompletionBindingKind =
                    string.IsNullOrWhiteSpace(artifactRecordId)
                        ? JsonDesignConversionPostEvaluationStateStore
                            .CompletionBindingVisualCostLedgerRecord
                        : JsonDesignConversionPostEvaluationStateStore
                            .CompletionBindingLocalArtifactDbRecord,
                CompletionBindingRef =
                    string.IsNullOrWhiteSpace(artifactRecordId)
                        ? ledgerRecordId
                        : artifactRecordId,
                LastErrorCode = string.Empty,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            },
            cancellationToken);
    }

    private async Task QuarantinePostEvaluationPersistenceAsync(
        DesignConversionPostEvaluationState state,
        string ledgerRecordId,
        string artifactRecordId,
        string errorCode)
    {
        _ = await WritePostEvaluationStateAsync(
            state with
            {
                PersistenceStatus =
                    JsonDesignConversionPostEvaluationStateStore.PersistenceQuarantined,
                LedgerRecordId = ledgerRecordId,
                ArtifactRecordId = artifactRecordId,
                CompletionBindingKind =
                    string.IsNullOrWhiteSpace(artifactRecordId)
                        ? string.IsNullOrWhiteSpace(ledgerRecordId)
                            ? string.Empty
                            : JsonDesignConversionPostEvaluationStateStore
                                .CompletionBindingVisualCostLedgerRecord
                        : JsonDesignConversionPostEvaluationStateStore
                            .CompletionBindingLocalArtifactDbRecord,
                CompletionBindingRef =
                    string.IsNullOrWhiteSpace(artifactRecordId)
                        ? ledgerRecordId
                        : artifactRecordId,
                LastErrorCode = SafePersistenceErrorCode(errorCode),
                UpdatedAtUtc = DateTimeOffset.UtcNow
            },
            CancellationToken.None);
    }

    private async Task<OperationResult<DesignConversionThinClientStatusResult>>
        ReconcileAndCompletePostEvaluationAccountingFailureAsync(
            string jobId,
            string attemptId,
            DesignConversionThinClientJob runningJob,
            SubmittedDesignConversionRequest request,
            VisualCostBudgetReservationContext reservation,
            DesignConversionPostEvaluationState state,
            bool settlementDefinitivelyNotApplied,
            string safeMessage,
            string errorCode,
            Task finalizationDeadline,
            CancellationTokenSource finalizationCancellation)
    {
        var accountingStatus =
            JsonDesignConversionPostEvaluationStateStore.AccountingReconciliationRequired;
        var reconciliationRequired = true;
        var currentState = state;
        var query = await ReadReservationSnapshotWithinPostEvaluationFinalizationAsync(
            reservation,
            finalizationDeadline,
            finalizationCancellation);
        if (query.IsSuccess && query.Value is not null)
        {
            var snapshot = query.Value;
            if (string.Equals(
                    snapshot.Status,
                    JsonlDesignConversionVisualCostBudgetStore.StatusSettled,
                    StringComparison.Ordinal))
            {
                accountingStatus = JsonDesignConversionPostEvaluationStateStore.AccountingSettled;
                reconciliationRequired = false;
            }
            else if (string.Equals(
                         snapshot.Status,
                         JsonlDesignConversionVisualCostBudgetStore.StatusReleased,
                         StringComparison.Ordinal))
            {
                accountingStatus = JsonDesignConversionPostEvaluationStateStore.AccountingReleased;
                reconciliationRequired = false;
            }
            else if (string.Equals(
                         snapshot.Status,
                         JsonlDesignConversionVisualCostBudgetStore.StatusReserved,
                         StringComparison.Ordinal) &&
                     settlementDefinitivelyNotApplied)
            {
                var releaseAttemptState = await WritePostEvaluationStateAsync(
                    currentState with
                    {
                        AccountingStatus =
                            JsonDesignConversionPostEvaluationStateStore.AccountingReconciliationRequired,
                        AccountingDesiredAction =
                            JsonDesignConversionPostEvaluationStateStore.DesiredRelease,
                        SettlementDefinitivelyNotApplied = true,
                        ReleaseAttempted = true,
                        LastErrorCode = SafeVisualCostBudgetErrorCode(
                            errorCode,
                            TrustedVisualCostBudgetSettlementFailedErrorCode),
                        UpdatedAtUtc = DateTimeOffset.UtcNow
                    },
                    CancellationToken.None);
                if (releaseAttemptState.IsSuccess)
                    currentState = releaseAttemptState.Value!;

                var release =
                    await ReleaseReservationWithinPostEvaluationFinalizationAsync(
                        reservation,
                        finalizationDeadline,
                        finalizationCancellation);
                if (release.IsSuccess && release.Value!.Release.Released)
                {
                    accountingStatus = JsonDesignConversionPostEvaluationStateStore.AccountingReleased;
                    reconciliationRequired = false;
                }
                else
                {
                    var afterRelease =
                        await ReadReservationSnapshotWithinPostEvaluationFinalizationAsync(
                            reservation,
                            finalizationDeadline,
                            finalizationCancellation);
                    if (afterRelease.IsSuccess && afterRelease.Value is not null)
                    {
                        if (string.Equals(
                                afterRelease.Value.Status,
                                JsonlDesignConversionVisualCostBudgetStore.StatusSettled,
                                StringComparison.Ordinal))
                        {
                            accountingStatus =
                                JsonDesignConversionPostEvaluationStateStore.AccountingSettled;
                            reconciliationRequired = false;
                        }
                        else if (string.Equals(
                                     afterRelease.Value.Status,
                                     JsonlDesignConversionVisualCostBudgetStore.StatusReleased,
                                     StringComparison.Ordinal))
                        {
                            accountingStatus =
                                JsonDesignConversionPostEvaluationStateStore.AccountingReleased;
                            reconciliationRequired = false;
                        }
                    }
                }
            }
        }

        var desiredAction = reconciliationRequired
            ? settlementDefinitivelyNotApplied
                ? JsonDesignConversionPostEvaluationStateStore.DesiredRelease
                : JsonDesignConversionPostEvaluationStateStore.DesiredSettle
            : JsonDesignConversionPostEvaluationStateStore.DesiredNone;
        var durableState = await WritePostEvaluationStateAsync(
            currentState with
            {
                AccountingStatus = accountingStatus,
                AccountingDesiredAction = desiredAction,
                SettlementDefinitivelyNotApplied =
                    accountingStatus ==
                    JsonDesignConversionPostEvaluationStateStore.AccountingReleased ||
                    settlementDefinitivelyNotApplied,
                ReleaseAttempted =
                    accountingStatus ==
                    JsonDesignConversionPostEvaluationStateStore.AccountingReleased ||
                    currentState.ReleaseAttempted,
                LastErrorCode = reconciliationRequired
                    ? AccountingReconciliationRequiredErrorCode
                    : string.Empty,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            },
            CancellationToken.None);
        var effectiveState = durableState.IsSuccess ? durableState.Value! : currentState;

        return CompletePostEvaluationTerminalNoRelease(
            jobId,
            attemptId,
            runningJob,
            request,
            reservation,
            safeMessage,
            errorCode,
            effectiveState.StateRef,
            reconciliationRequired);
    }

    private async Task<OperationResult<DesignConversionVisualCostReservationSnapshot?>>
        ReadReservationSnapshotWithinPostEvaluationFinalizationAsync(
            VisualCostBudgetReservationContext reservation,
            Task finalizationDeadline,
            CancellationTokenSource finalizationCancellation)
    {
        if (_visualCostBudgetStore is null)
        {
            return OperationResult<DesignConversionVisualCostReservationSnapshot?>.Fail(
                AccountingReconciliationRequiredSafeMessage,
                AccountingReconciliationBudgetStoreRequiredErrorCode);
        }

        var bounded = await AwaitPostEvaluationFinalizationStepAsync(
            token => _visualCostBudgetStore.ReadPeriodAsync(
                new DesignConversionVisualCostBudgetPeriodQuery(
                    reservation.Request.UserRef,
                    reservation.Request.PeriodStartUtc,
                    reservation.Request.PeriodEndExclusiveUtc),
                token),
            finalizationDeadline,
            finalizationCancellation,
            CancellationToken.None);
        if (!bounded.IsSuccess)
        {
            return OperationResult<DesignConversionVisualCostReservationSnapshot?>.Fail(
                AccountingReconciliationRequiredSafeMessage,
                bounded.ErrorCode);
        }

        return SelectReservationSnapshot(
            reservation,
            bounded.Value!);
    }

    private async Task<OperationResult<VisualCostBudgetReleaseContext>>
        ReleaseReservationWithinPostEvaluationFinalizationAsync(
            VisualCostBudgetReservationContext reservation,
            Task finalizationDeadline,
            CancellationTokenSource finalizationCancellation)
    {
        var bounded = await AwaitPostEvaluationFinalizationStepAsync(
            token => ReleaseVisualCostBudgetReservationAsync(reservation, token),
            finalizationDeadline,
            finalizationCancellation,
            CancellationToken.None);
        return bounded.IsSuccess
            ? bounded.Value!
            : OperationResult<VisualCostBudgetReleaseContext>.Fail(
                PostEvaluationFinalizationFailedSafeMessage,
                bounded.ErrorCode ?? PostEvaluationFinalizationFailedErrorCode);
    }

    private async Task<OperationResult<DesignConversionVisualCostReservationSnapshot?>>
        ReadReservationSnapshotBoundedAsync(
            VisualCostBudgetReservationContext reservation,
            CancellationToken callerCancellationToken = default)
    {
        if (_visualCostBudgetStore is null)
        {
            return OperationResult<DesignConversionVisualCostReservationSnapshot?>.Fail(
                AccountingReconciliationRequiredSafeMessage,
                AccountingReconciliationBudgetStoreRequiredErrorCode);
        }

        var bounded = await AwaitAccountingReconciliationStepAsync(
            token => _visualCostBudgetStore.ReadPeriodAsync(
                new DesignConversionVisualCostBudgetPeriodQuery(
                    reservation.Request.UserRef,
                    reservation.Request.PeriodStartUtc,
                    reservation.Request.PeriodEndExclusiveUtc),
                token),
            callerCancellationToken);
        if (!bounded.IsSuccess)
        {
            return OperationResult<DesignConversionVisualCostReservationSnapshot?>.Fail(
                AccountingReconciliationRequiredSafeMessage,
                bounded.ErrorCode);
        }

        return SelectReservationSnapshot(
            reservation,
            bounded.Value!);
    }

    private static OperationResult<DesignConversionVisualCostReservationSnapshot?>
        SelectReservationSnapshot(
            VisualCostBudgetReservationContext reservation,
            OperationResult<DesignConversionVisualCostBudgetPeriodState> read)
    {
        if (!read.IsSuccess)
        {
            return OperationResult<DesignConversionVisualCostReservationSnapshot?>.Fail(
                AccountingReconciliationRequiredSafeMessage,
                StableReconciliationItemError(read.ErrorCode));
        }

        var matches = read.Value!.Reservations
            .Where(candidate => string.Equals(
                candidate.ReservationRef,
                reservation.Request.ReservationRef,
                StringComparison.Ordinal))
            .ToArray();
        return matches.Length == 1
            ? OperationResult<DesignConversionVisualCostReservationSnapshot?>.Ok(matches[0])
            : OperationResult<DesignConversionVisualCostReservationSnapshot?>.Fail(
                AccountingReconciliationRequiredSafeMessage,
                AccountingReconciliationReservationIdentityInvalidErrorCode);
    }

    private async Task<OperationResult<VisualCostBudgetReleaseContext>> ReleaseReservationBoundedAsync(
        VisualCostBudgetReservationContext reservation,
        CancellationToken callerCancellationToken = default)
    {
        var bounded = await AwaitAccountingReconciliationStepAsync(
            token => ReleaseVisualCostBudgetReservationAsync(reservation, token),
            callerCancellationToken);
        return bounded.IsSuccess
            ? bounded.Value!
            : OperationResult<VisualCostBudgetReleaseContext>.Fail(
                PostEvaluationFinalizationFailedSafeMessage,
                bounded.ErrorCode ?? PostEvaluationFinalizationFailedErrorCode);
    }

    private OperationResult<DesignConversionThinClientStatusResult>
        CompletePostEvaluationTerminalNoRelease(
            string jobId,
            string attemptId,
            DesignConversionThinClientJob runningJob,
            SubmittedDesignConversionRequest request,
            VisualCostBudgetReservationContext? reservation,
            string safeMessage,
            string errorCode,
            string? stateRef,
            bool accountingReconciliationRequired)
    {
        var failedStatus = MarkPostEvaluationAccountingState(
            MarkPostEvaluationFinalizationFailure(
                BuildFailedStatus(runningJob, request, safeMessage, errorCode),
                errorCode),
            stateRef,
            accountingReconciliationRequired);
        DiscardPendingRejectedEvaluation(jobId);
        lock (_gate)
        {
            if (!_runLeases.TryGetValue(jobId, out var lease) ||
                !string.Equals(lease.AttemptId, attemptId, StringComparison.Ordinal) ||
                lease.Phase != JobRunLeasePhase.PostEvaluationOwned)
            {
                return _statuses.TryGetValue(jobId, out var existingStatus)
                    ? OperationResult<DesignConversionThinClientStatusResult>.Ok(existingStatus)
                    : OperationResult<DesignConversionThinClientStatusResult>.Fail(
                        $"DesignConversion thin-client job '{jobId}' lost its terminal status.",
                        "JOB_STATUS_LOST");
            }

            _jobs[jobId] = failedStatus.Job;
            _statuses[jobId] = failedStatus;
            lease.ReservationFinalized = reservation is null ||
                                         !accountingReconciliationRequired;
            lease.Phase = JobRunLeasePhase.Finished;
            _runLeases.Remove(jobId);
        }

        return OperationResult<DesignConversionThinClientStatusResult>.Ok(failedStatus);
    }

    private bool TryAttachReservationToLease(
        string jobId,
        string attemptId,
        VisualCostBudgetReservationContext reservation,
        out ReservationReleaseWork? releaseWork,
        out Task? releaseCompletion)
    {
        releaseWork = null;
        releaseCompletion = null;
        lock (_gate)
        {
            if (!_runLeases.TryGetValue(jobId, out var lease) ||
                !string.Equals(lease.AttemptId, attemptId, StringComparison.Ordinal))
            {
                var orphanCompletion = new TaskCompletionSource<bool>(
                    TaskCreationOptions.RunContinuationsAsynchronously);
                releaseWork = new ReservationReleaseWork(
                    jobId,
                    attemptId,
                    reservation,
                    orphanCompletion);
                releaseCompletion = orphanCompletion.Task;
                return false;
            }

            lease.Reservation = reservation;
            if (lease.Phase == JobRunLeasePhase.Evaluating &&
                _jobs.TryGetValue(jobId, out var storedJob) &&
                string.Equals(storedJob.Status, DesignConversionThinClientStatus.Running, StringComparison.Ordinal))
            {
                return true;
            }

            releaseWork = TryClaimReservationReleaseNoLock(jobId, lease);
            releaseCompletion = lease.ReleaseClaimed ? lease.ReleaseCompletion.Task : null;
            return false;
        }
    }

    private bool TryAcquirePostEvaluationOwnership(
        string jobId,
        string attemptId,
        out ReservationReleaseWork? releaseWork,
        out Task? releaseCompletion)
    {
        releaseWork = null;
        releaseCompletion = null;
        lock (_gate)
        {
            if (_runLeases.TryGetValue(jobId, out var lease) &&
                string.Equals(lease.AttemptId, attemptId, StringComparison.Ordinal) &&
                lease.Phase == JobRunLeasePhase.Evaluating &&
                _jobs.TryGetValue(jobId, out var storedJob) &&
                string.Equals(storedJob.Status, DesignConversionThinClientStatus.Running, StringComparison.Ordinal))
            {
                lease.Phase = JobRunLeasePhase.PostEvaluationOwned;
                return true;
            }

            if (_runLeases.TryGetValue(jobId, out lease) &&
                string.Equals(lease.AttemptId, attemptId, StringComparison.Ordinal))
            {
                releaseWork = TryClaimReservationReleaseNoLock(jobId, lease);
                releaseCompletion = lease.ReleaseClaimed ? lease.ReleaseCompletion.Task : null;
            }

            return false;
        }
    }

    private async Task<OperationResult<DesignConversionThinClientStatusResult>> ReturnTerminalAfterOwnershipLostAsync(
        string jobId,
        ReservationReleaseWork? releaseWork,
        Task? releaseCompletion)
    {
        if (releaseWork is not null)
            StartReservationRelease(releaseWork);
        if (releaseCompletion is not null)
            await releaseCompletion;

        DiscardPendingRejectedEvaluation(jobId);

        lock (_gate)
        {
            _runLeases.Remove(jobId);
            var result = _statuses.TryGetValue(jobId, out var terminalStatus)
                ? OperationResult<DesignConversionThinClientStatusResult>.Ok(terminalStatus)
                : OperationResult<DesignConversionThinClientStatusResult>.Fail(
                    $"DesignConversion thin-client job '{jobId}' lost its terminal status.",
                    "JOB_STATUS_LOST");
            return result;
        }
    }

    private bool TryFailClosedStaleRunningNoLock(
        string jobId,
        out DesignConversionThinClientStatusResult staleStatus,
        out ReservationReleaseWork? releaseWork)
    {
        staleStatus = null!;
        releaseWork = null;
        if (!_jobs.TryGetValue(jobId, out var job) ||
            !_requests.TryGetValue(jobId, out var request) ||
            !string.Equals(job.Status, DesignConversionThinClientStatus.Running, StringComparison.Ordinal))
        {
            return false;
        }

        if (_runLeases.TryGetValue(jobId, out var activeLease) &&
            activeLease.Phase == JobRunLeasePhase.PostEvaluationOwned)
        {
            return false;
        }

        var now = _utcNow();
        var runningStartedAt = job.UpdatedAtUtc ?? job.SubmittedAtUtc;
        if (now - runningStartedAt < _staleRunningTimeout)
            return false;

        staleStatus = BuildStaleRunningFailedStatus(job, request, runningStartedAt, now);
        _jobs[jobId] = staleStatus.Job;
        _statuses[jobId] = staleStatus;
        if (_runLeases.TryGetValue(jobId, out activeLease) &&
            activeLease.Phase == JobRunLeasePhase.Evaluating)
        {
            activeLease.Phase = JobRunLeasePhase.Revoked;
            releaseWork = TryClaimReservationReleaseNoLock(jobId, activeLease);
            // Keep the revoked lease until its claimed release publishes the
            // terminal accounting metadata. The in-flight RunAsync can then
            // observe and await ReleaseCompletion instead of returning the
            // pre-release stale snapshot.
            if (releaseWork is null)
                _runLeases.Remove(jobId);
        }

        return true;
    }

    private ReservationReleaseWork? TryClaimReservationReleaseNoLock(
        string jobId,
        JobRunLease lease,
        string releaseEnforcement = "release_after_stale_terminal_cleanup_before_settlement")
    {
        if (lease.Reservation is null ||
            lease.ReservationFinalized ||
            lease.ReleaseClaimed ||
            lease.Phase is JobRunLeasePhase.PostEvaluationOwned or JobRunLeasePhase.Finished)
        {
            return null;
        }

        lease.ReleaseClaimed = true;
        return new ReservationReleaseWork(
            jobId,
            lease.AttemptId,
            lease.Reservation,
            lease.ReleaseCompletion,
            releaseEnforcement);
    }

    private void StartReservationRelease(ReservationReleaseWork releaseWork)
        => ObserveLateFinalizationTask(CompleteReservationReleaseAsync(releaseWork));

    private async Task CompleteReservationReleaseAsync(ReservationReleaseWork releaseWork)
    {
        var stateRef = JsonDesignConversionPostEvaluationStateStore.BuildStateRef(
            releaseWork.Reservation.Request.ReservationRef);
        OperationResult<VisualCostBudgetReleaseContext>? release = null;
        string? finalAccountingStatus = null;
        var durableStateFailure = false;
        try
        {
            var stateRead = await _postEvaluationStateStore.ReadAsync(
                stateRef,
                CancellationToken.None);
            durableStateFailure = !stateRead.IsSuccess;
            if (stateRead.IsSuccess && stateRead.Value is null)
            {
                var prepared = await PreparePostEvaluationStateAsync(
                    releaseWork.JobId,
                    releaseWork.AttemptId,
                    releaseWork.Reservation,
                    OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                        "Evaluation did not reach accounting settlement.",
                        "EVALUATION_NOT_SETTLED"),
                    CancellationToken.None);
                if (prepared.IsSuccess)
                    stateRead = OperationResult<DesignConversionPostEvaluationState?>.Ok(prepared.Value);
                else
                    durableStateFailure = true;
            }

            if (stateRead.IsSuccess && stateRead.Value is not null)
            {
                var releaseIntent = await WritePostEvaluationStateAsync(
                    stateRead.Value with
                    {
                        AccountingStatus =
                            JsonDesignConversionPostEvaluationStateStore.AccountingReconciliationRequired,
                        AccountingDesiredAction =
                            JsonDesignConversionPostEvaluationStateStore.DesiredRelease,
                        SettlementDefinitivelyNotApplied = true,
                        ReleaseAttempted = true,
                        UpdatedAtUtc = DateTimeOffset.UtcNow
                    },
                    CancellationToken.None);
                if (releaseIntent.IsSuccess)
                {
                    stateRead =
                        OperationResult<DesignConversionPostEvaluationState?>.Ok(
                            releaseIntent.Value);
                }
                else
                {
                    durableStateFailure = true;
                }
            }

            release = await ReleaseReservationBoundedAsync(releaseWork.Reservation);
            finalAccountingStatus = release.IsSuccess
                ? JsonDesignConversionPostEvaluationStateStore.AccountingReleased
                : ClassifyFinalAccountingStatus(
                    await ReadReservationSnapshotBoundedAsync(releaseWork.Reservation));
            if (stateRead.IsSuccess && stateRead.Value is not null)
            {
                var finalStateWrite = await WritePostEvaluationStateAsync(
                    stateRead.Value with
                    {
                        AccountingStatus = finalAccountingStatus ??
                            JsonDesignConversionPostEvaluationStateStore.AccountingReconciliationRequired,
                        AccountingDesiredAction = finalAccountingStatus is null
                            ? JsonDesignConversionPostEvaluationStateStore.DesiredRelease
                            : JsonDesignConversionPostEvaluationStateStore.DesiredNone,
                        SettlementDefinitivelyNotApplied = true,
                        ReleaseAttempted = true,
                        LastErrorCode = finalAccountingStatus is null
                            ? AccountingReconciliationRequiredErrorCode
                            : string.Empty,
                        UpdatedAtUtc = DateTimeOffset.UtcNow
                    },
                    CancellationToken.None);
                if (!finalStateWrite.IsSuccess)
                    durableStateFailure = true;
            }

            _beforeReservationReleaseStatusPublication?.Invoke();
            PublishReservationReleaseTerminal(
                releaseWork,
                release,
                finalAccountingStatus,
                stateRef,
                unexpectedFailure: durableStateFailure);
        }
        catch (Exception)
        {
            // Stale cleanup is a fire-and-observe boundary. Unexpected
            // dependency failures must never escape it or signal completion
            // before a fail-closed terminal status has replaced the live lease.
            PublishReservationReleaseTerminal(
                releaseWork,
                release,
                finalAccountingStatus,
                stateRef,
                unexpectedFailure: true);
        }
        finally
        {
            releaseWork.Completion.TrySetResult(true);
        }
    }

    private void PublishReservationReleaseTerminal(
        ReservationReleaseWork releaseWork,
        OperationResult<VisualCostBudgetReleaseContext>? release,
        string? finalAccountingStatus,
        string stateRef,
        bool unexpectedFailure)
    {
        lock (_gate)
        {
            if (!_statuses.TryGetValue(releaseWork.JobId, out var existingTerminalStatus))
            {
                if (!_jobs.TryGetValue(releaseWork.JobId, out var existingJob) ||
                    !_requests.TryGetValue(releaseWork.JobId, out var existingRequest))
                {
                    RemoveMatchingReleaseLeaseNoLock(releaseWork);
                    return;
                }

                existingTerminalStatus = BuildFailedStatus(
                    existingJob,
                    existingRequest,
                    AccountingReconciliationRequiredSafeMessage,
                    AccountingReconciliationRequiredErrorCode);
            }

            var releaseConfirmed =
                release?.IsSuccess == true &&
                string.Equals(
                    finalAccountingStatus,
                    JsonDesignConversionPostEvaluationStateStore.AccountingReleased,
                    StringComparison.Ordinal);
            var markedStatus = releaseConfirmed
                ? MarkVisualCostBudgetReservationRelease(
                    existingTerminalStatus,
                    releaseWork.Reservation,
                    release!.Value!,
                    releaseWork.ReleaseEnforcement)
                : MarkVisualCostBudgetReservationReleaseFailed(
                    existingTerminalStatus,
                    releaseWork.Reservation,
                    unexpectedFailure
                        ? AccountingReconciliationRequiredErrorCode
                        : SafeVisualCostBudgetErrorCode(
                            release?.ErrorCode,
                            TrustedVisualCostBudgetSettlementFailedErrorCode),
                    releaseWork.ReleaseEnforcement);
            var reconciliationRequired =
                unexpectedFailure ||
                finalAccountingStatus is null;
            markedStatus = MarkPostEvaluationAccountingState(
                markedStatus,
                stateRef,
                reconciliationRequired);
            if (unexpectedFailure)
            {
                markedStatus = MarkPostEvaluationFinalizationFailure(
                    markedStatus,
                    AccountingReconciliationRequiredErrorCode);
            }

            _jobs[releaseWork.JobId] = markedStatus.Job;
            _statuses[releaseWork.JobId] = markedStatus;
            RemoveMatchingReleaseLeaseNoLock(releaseWork);
        }
    }

    private void RemoveMatchingReleaseLeaseNoLock(ReservationReleaseWork releaseWork)
    {
        if (_runLeases.TryGetValue(releaseWork.JobId, out var lease) &&
            string.Equals(lease.AttemptId, releaseWork.AttemptId, StringComparison.Ordinal))
        {
            _runLeases.Remove(releaseWork.JobId);
        }
    }

    private DesignConversionThinClientStatusResult BuildStaleRunningFailedStatus(
        DesignConversionThinClientJob runningJob,
        SubmittedDesignConversionRequest request,
        DateTimeOffset runningStartedAt,
        DateTimeOffset observedAt)
    {
        var failedJob = UpdateJob(runningJob, DesignConversionThinClientStatus.Failed, StaleRunningTimeoutErrorCode);
        var staleMetadata = new Dictionary<string, string>
        {
            ["stale_running_cleanup"] = bool.TrueString,
            ["stale_running_timeout_seconds"] = _staleRunningTimeout.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture),
            ["stale_running_started_at_utc"] = runningStartedAt.ToString("O", CultureInfo.InvariantCulture),
            ["stale_running_observed_at_utc"] = observedAt.ToString("O", CultureInfo.InvariantCulture),
            ["runtime_output_produced"] = bool.FalseString,
            ["evaluator_reinvoked_after_stale_cleanup"] = bool.FalseString
        };
        failedJob = failedJob with
        {
            Metadata = MergeDictionaries(failedJob.Metadata, staleMetadata)
        };
        var staleProvenance = MergeDictionaries(
            BuildMaxTeacherCallsContractMetadata(request.Budget),
            new Dictionary<string, string>
            {
                ["internal_evaluator_error_code"] = StaleRunningTimeoutErrorCode,
                ["runtime_boundary"] = RuntimeBoundary,
                ["stale_running_cleanup"] = bool.TrueString,
                ["stale_running_timeout_seconds"] = staleMetadata["stale_running_timeout_seconds"],
                ["stale_running_started_at_utc"] = staleMetadata["stale_running_started_at_utc"],
                ["stale_running_observed_at_utc"] = staleMetadata["stale_running_observed_at_utc"],
                ["runtime_output_produced"] = bool.FalseString,
                ["evaluator_reinvoked_after_stale_cleanup"] = bool.FalseString,
                ["thin_client_terminal_status"] = DesignConversionThinClientStatus.Failed
            });

        return BuildStatusResult(
            failedJob,
            DesignConversionThinClientStatus.Failed,
            StaleRunningTimeoutSafeMessage,
            artifacts: [],
            extraWarnings:
            [
                $"internal_evaluator_error_code={StaleRunningTimeoutErrorCode}",
                "stale_running_cleanup=true",
                $"stale_running_timeout_seconds={staleMetadata["stale_running_timeout_seconds"]}",
                "runtime_output_produced=false",
                "evaluator_reinvoked_after_stale_cleanup=false",
                $"thin_client_terminal_status={DesignConversionThinClientStatus.Failed}"
            ],
            staleProvenance,
            DesignConversionThinClientStatusQualityMetrics.Unavailable(
                source: "stale_running_cleanup",
                unavailableReason: "runtime_output_not_produced_stale_running_timeout"));
    }

    private async Task<DesignConversionThinClientStatusResult> PersistCompletedEvaluationAsync(
        DesignConversionThinClientJob runningJob,
        SubmittedDesignConversionRequest request,
        DesignConversionThinClientStatusResult completedStatus,
        DesignConversionInternalEvaluationResult evaluation,
        DesignConversionPostEvaluationState postEvaluationState,
        Task finalizationDeadline,
        DesignConversionTeacherCallUsageAccumulator teacherCallUsage,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var createdAtUtc = _utcNow();
        var rejectedEvaluation = await ResolveMeasuredRejectedEvaluationAsync(
            evaluation,
            cancellationToken);
        var measuredChosenEvaluation = DesignConversionDpoGenerator
            .NormalizeMeasuredVisualScoreForDpo(
            rejectedEvaluation.TrustedChosenEvaluation ?? evaluation);
        if (!string.IsNullOrWhiteSpace(rejectedEvaluation.TrustedChosenEvaluationJobId))
        {
            measuredChosenEvaluation = measuredChosenEvaluation with
            {
                // Preserve the raw selected-loop identity only for the internal
                // DPO build. Public completion/status results retain the root id.
                JobId = rejectedEvaluation.TrustedChosenEvaluationJobId
            };
        }
        var measuredRejectedEvaluation = rejectedEvaluation.Value is null
            ? null
            : DesignConversionDpoGenerator.NormalizeMeasuredVisualScoreForDpo(
                rejectedEvaluation.Value);
        var actualObservedTeacherCallCount =
            teacherCallUsage.ActualObservedCallCount;
        var generator = new DesignConversionDpoGenerator();
        OperationResult<DesignConversionDpoBuildResult> dpoBuild;
        var independentPreferenceWarnings = new List<string>();
        if (rejectedEvaluation.TeacherAuthoredEvaluationPairs.Count > 0)
        {
            var evidenceSource = _artifactDb is null
                ? null
                : DesignConversionIndependentPreferenceEvidenceAssociation
                    .Resolve(_artifactDb);
            if (rejectedEvaluation
                .CoordinatorOwnedPreferenceEvaluationHandled)
            {
                dpoBuild = generator.BuildRuntimeTeacherAuthoredTrainingCandidates(
                    rejectedEvaluation.TeacherAuthoredEvaluationPairs,
                    rejectedEvaluation.CoordinatorConsumedPreferenceEvidence,
                    DesignConversionIndependentPreferenceEvidenceAssociation
                        .BuildArtifactRootBinding(_artifactDb!),
                    createdAtUtc);
            }
            else if (evidenceSource is null)
            {
                dpoBuild = generator.BuildRuntimeTeacherAuthoredTrainingCandidates(
                    rejectedEvaluation.TeacherAuthoredEvaluationPairs,
                    createdAtUtc);
            }
            else
            {
                var receipts = new List<
                    DesignConversionIndependentPreferenceEvidence>();
                foreach (var pair in
                         rejectedEvaluation.TeacherAuthoredEvaluationPairs)
                {
                    if (evidenceSource.CanIssueEvidence)
                    {
                        var issued = await evidenceSource.IssueEvidenceAsync(
                            pair,
                            createdAtUtc,
                            cancellationToken).ConfigureAwait(false);
                        if (!issued.IsSuccess)
                        {
                            independentPreferenceWarnings.Add(
                                "LOCAL_INDEPENDENT_PREFERENCE_EVALUATION_FAILED:" +
                                SafeVisualCostBudgetErrorCode(
                                    issued.ErrorCode,
                                    DesignConversionLocalIndependentPreferenceEvaluator
                                        .EvaluationUnavailableErrorCode));
                        }
                    }

                    if (evidenceSource.TryConsumeEvidence(
                            pair,
                            createdAtUtc,
                            out var evidence) &&
                        evidence is not null)
                    {
                        receipts.Add(evidence);
                    }
                }

                dpoBuild = generator.BuildRuntimeTeacherAuthoredTrainingCandidates(
                    rejectedEvaluation.TeacherAuthoredEvaluationPairs,
                    receipts,
                    evidenceSource.ArtifactRootBinding,
                    createdAtUtc);
            }
        }
        else if (measuredRejectedEvaluation is not null &&
            rejectedEvaluation.TeacherCandidateAuthoringEvidence is not null)
        {
            dpoBuild = generator.BuildRuntimeTeacherAuthoredTrainingCandidates(
                measuredChosenEvaluation,
                measuredRejectedEvaluation,
                rejectedEvaluation.TeacherCandidateAuthoringEvidence,
                createdAtUtc);
        }
        else if (measuredRejectedEvaluation is null)
        {
            dpoBuild = generator.BuildRuntimeCreatedTrainingCandidates(
                measuredChosenEvaluation,
                createdAtUtc);
        }
        else
        {
            dpoBuild = generator.BuildRuntimeCreatedTrainingCandidates(
                measuredChosenEvaluation,
                measuredRejectedEvaluation,
                createdAtUtc);
        }
        var dpoBuildWarnings = (dpoBuild.IsSuccess
                ? dpoBuild.Value!.Warnings
                : [dpoBuild.ErrorCode ?? "runtime_created_dpo_candidate_build_failed"])
            .Concat(rejectedEvaluation.Warnings)
            .Concat(independentPreferenceWarnings)
            .Append(dpoBuild.IsSuccess && dpoBuild.Value!.AcceptedRecords.Count > 0
                ? "training_candidate_only=true"
                : "training_candidate_only=false")
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var dpoCandidates = dpoBuild.IsSuccess
            ? dpoBuild.Value!.AcceptedRecords
            : [];
        var dpoPersistenceRequired = dpoCandidates.Count > 0;
        var teacherCallMetadata = BuildRuntimeTeacherCallBudgetMetadata(
            request.Budget,
            actualObservedTeacherCallCount);
        var teacherCallReconciliation = ReconcileSubmitPathTeacherCallCapAfterAttestation(
            completedStatus,
            evaluation,
            request.Budget,
            teacherCallIssued: false,
            actualObservedTeacherCallCount,
            teacherCallMetadata);
        completedStatus = teacherCallReconciliation.CompletedStatus with
        {
            Job = teacherCallReconciliation.CompletedStatus.Job with
            {
                Metadata = MergeDictionaries(
                    teacherCallReconciliation.CompletedStatus.Job.Metadata,
                    teacherCallMetadata)
            },
            Provenance = MergeDictionaries(
                teacherCallReconciliation.CompletedStatus.Provenance,
                teacherCallMetadata)
        };
        var evaluationForPersistence = teacherCallReconciliation.Evaluation;
        var visualCostRecord = BuildVisualCostLedgerRecord(
            evaluation,
            request.Budget,
            createdAtUtc,
            postEvaluationState.StateRef);
        OperationResult<DesignConversionVisualCostLedgerRecord> visualCostAppend;
        try
        {
            visualCostAppend = await _visualCostLedger.AppendRecordAsync(visualCostRecord, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            visualCostAppend = OperationResult<DesignConversionVisualCostLedgerRecord>.Fail(
                ex.Message,
                "VISUAL_COST_LEDGER_PERSISTENCE_EXCEPTION");
        }

        if (!visualCostAppend.IsSuccess)
        {
            if (string.Equals(visualCostAppend.ErrorCode, "CANCELLED", StringComparison.Ordinal))
                throw new OperationCanceledException(cancellationToken);

            return MarkVisualCostLedgerPersistenceFailed(
                BuildFailedStatus(
                    runningJob,
                    request,
                    "DesignConversion thin-client job failed after evaluation because visual-cost accounting could not be durably recorded.",
                    SafePersistenceErrorCode(visualCostAppend.ErrorCode)));
        }

        completedStatus = MarkVisualCostLedgerPersistence(completedStatus, visualCostAppend.Value!);

        if (_artifactDb is null)
        {
            if (dpoPersistenceRequired)
            {
                await QuarantinePostEvaluationPersistenceAsync(
                    postEvaluationState,
                    visualCostAppend.Value!.RecordId,
                    artifactRecordId: string.Empty,
                    LocalArtifactDbDpoPersistenceRequiredErrorCode);
                return BuildRequiredLocalArtifactDbPersistenceFailure(
                    runningJob,
                    request,
                    visualCostAppend.Value!,
                    LocalArtifactDbDpoPersistenceRequiredErrorCode);
            }

            var commitWithoutArtifact = await CommitPostEvaluationPersistenceAsync(
                postEvaluationState,
                visualCostAppend.Value!.RecordId,
                artifactRecordId: string.Empty,
                finalizationDeadline,
                cancellationToken);
            return commitWithoutArtifact.IsSuccess
                ? completedStatus
                : MarkPostEvaluationFinalizationFailure(
                    BuildFailedStatus(
                        runningJob,
                        request,
                        PostEvaluationFinalizationFailedSafeMessage,
                        SafePersistenceErrorCode(commitWithoutArtifact.ErrorCode)),
                    SafePersistenceErrorCode(commitWithoutArtifact.ErrorCode));
        }

        var record = DesignConversionLocalArtifactDbRecord.FromEvaluation(
            evaluationForPersistence,
            dpoCandidates: dpoCandidates,
            createdAtUtc: createdAtUtc,
            metadata: BuildLocalArtifactDbMetadata(
                evaluationForPersistence,
                dpoCandidates.Count,
                dpoBuildWarnings,
                postEvaluationState.StateRef,
                teacherCallMetadata),
            completedStatusQualityMetrics: completedStatus.QualityMetrics,
            completedStatusDefects: completedStatus.Defects);
        OperationResult<DesignConversionLocalArtifactDbRecord> append;
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            append = await _artifactDb.AppendRecordAsync(record, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            append = OperationResult<DesignConversionLocalArtifactDbRecord>.Fail(
                ex.Message,
                "LOCAL_ARTIFACT_DB_PERSISTENCE_EXCEPTION");
        }

        if (!append.IsSuccess)
        {
            await QuarantinePostEvaluationPersistenceAsync(
                postEvaluationState,
                visualCostAppend.Value!.RecordId,
                artifactRecordId: string.Empty,
                append.ErrorCode ?? LocalArtifactDbDpoPersistenceFailedErrorCode);
            if (string.Equals(append.ErrorCode, "CANCELLED", StringComparison.Ordinal))
                throw new OperationCanceledException(cancellationToken);

            if (dpoPersistenceRequired)
            {
                return BuildRequiredLocalArtifactDbPersistenceFailure(
                    runningJob,
                    request,
                    visualCostAppend.Value!,
                    LocalArtifactDbDpoPersistenceFailedErrorCode);
            }

            return MarkOptionalLocalArtifactDbPersistenceFailed(
                completedStatus,
                append.ErrorCode ?? "LOCAL_ARTIFACT_DB_PERSISTENCE_FAILED");
        }

        if (!dpoPersistenceRequired)
        {
            var commit = await CommitPostEvaluationPersistenceAsync(
                postEvaluationState,
                visualCostAppend.Value!.RecordId,
                append.Value!.RecordId,
                finalizationDeadline,
                cancellationToken);
            return commit.IsSuccess
                ? MarkLocalArtifactDbPersistence(completedStatus, append.Value!)
                : MarkPostEvaluationFinalizationFailure(
                    BuildFailedStatus(
                        runningJob,
                        request,
                        PostEvaluationFinalizationFailedSafeMessage,
                        SafePersistenceErrorCode(commit.ErrorCode)),
                    SafePersistenceErrorCode(commit.ErrorCode));
        }

        OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>> readback;
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            readback = await _artifactDb.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery(
                    JobId: append.Value!.JobId,
                    RequestId: append.Value.RequestId),
                cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return BuildRequiredLocalArtifactDbPersistenceFailure(
                runningJob,
                request,
                visualCostAppend.Value!,
                LocalArtifactDbDpoReadbackFailedErrorCode);
        }

        if (!readback.IsSuccess)
        {
            if (string.Equals(readback.ErrorCode, "CANCELLED", StringComparison.Ordinal))
                throw new OperationCanceledException(cancellationToken);

            return BuildRequiredLocalArtifactDbPersistenceFailure(
                runningJob,
                request,
                visualCostAppend.Value!,
                LocalArtifactDbDpoReadbackFailedErrorCode);
        }

        var matchingRecords = (readback.Value ?? [])
            .Where(candidate => IsMatchingDurableArtifactRecord(append.Value!, candidate))
            .ToArray();
        if (matchingRecords.Length != 1)
        {
            return BuildRequiredLocalArtifactDbPersistenceFailure(
                runningJob,
                request,
                visualCostAppend.Value!,
                LocalArtifactDbDpoReadbackMismatchErrorCode);
        }

        var committed = await CommitPostEvaluationPersistenceAsync(
            postEvaluationState,
            visualCostAppend.Value!.RecordId,
            matchingRecords[0].RecordId,
            finalizationDeadline,
            cancellationToken);
        if (!committed.IsSuccess)
        {
            await QuarantinePostEvaluationPersistenceAsync(
                postEvaluationState,
                visualCostAppend.Value!.RecordId,
                matchingRecords[0].RecordId,
                SafePersistenceErrorCode(committed.ErrorCode));
            return MarkPostEvaluationFinalizationFailure(
                BuildFailedStatus(
                    runningJob,
                    request,
                    PostEvaluationFinalizationFailedSafeMessage,
                    SafePersistenceErrorCode(committed.ErrorCode)),
                SafePersistenceErrorCode(committed.ErrorCode));
        }

        return MarkLocalArtifactDbPersistence(completedStatus, matchingRecords[0]);
    }

    private async Task<RejectedEvaluationResolution> ResolveMeasuredRejectedEvaluationAsync(
        DesignConversionInternalEvaluationResult chosenEvaluation,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_rejectedEvaluationSource is null)
            return RejectedEvaluationResolution.Unavailable;

        try
        {
            var resolved = await _rejectedEvaluationSource.ResolveMeasuredRejectedEvaluationAsync(
                new DesignConversionRuntimeRejectedEvaluationLookup(chosenEvaluation.JobId),
                cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (resolved is null ||
                !resolved.IsSuccess ||
                resolved.Value is null ||
                !resolved.Value.TryGetTrustedHydratedEvaluation(out var trustedRejectedEvaluation) ||
                trustedRejectedEvaluation is null)
            {
                if (resolved is not null &&
                    string.Equals(resolved.ErrorCode, "CANCELLED", StringComparison.Ordinal) &&
                    cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException(cancellationToken);
                }

                return RejectedEvaluationResolution.Unavailable;
            }

            var trustedChosenEvaluationJobId = resolved.Value
                .TryGetTrustedChosenEvaluationJobId(out var trustedChosenJobId)
                ? trustedChosenJobId
                : chosenEvaluation.JobId;
            var hasTrustedChosenEvaluation = resolved.Value.TryGetTrustedChosenEvaluation(
                out var trustedChosenEvaluation);
            var hasTeacherCandidateAuthoringEvidence = resolved.Value
                .TryGetTrustedTeacherCandidateAuthoringEvidence(
                    out var trustedTeacherCandidateAuthoringEvidence);
            var hasTeacherAuthoredEvaluationPairs = resolved.Value
                .TryGetTrustedTeacherAuthoredEvaluationPairs(
                    out var trustedTeacherAuthoredEvaluationPairs);
            var coordinatorOwnedPreferenceEvaluationHandled = resolved.Value
                .TryGetTrustedCoordinatorConsumedPreferenceEvidence(
                    out var coordinatorConsumedPreferenceEvidence);
            resolved.Value.TryGetTrustedCoordinatorPreferenceWarnings(
                out var coordinatorPreferenceWarnings);
            if (hasTrustedChosenEvaluation &&
                !string.Equals(
                    trustedChosenEvaluation!.JobId,
                    trustedChosenEvaluationJobId,
                    StringComparison.Ordinal))
            {
                return RejectedEvaluationResolution.Unavailable;
            }

            if (string.Equals(
                    trustedChosenEvaluationJobId,
                    trustedRejectedEvaluation.JobId,
                    StringComparison.Ordinal))
            {
                return RejectedEvaluationResolution.Unavailable;
            }

            return new RejectedEvaluationResolution(
                trustedRejectedEvaluation,
                trustedChosenEvaluationJobId,
                hasTrustedChosenEvaluation ? trustedChosenEvaluation : null,
                hasTeacherCandidateAuthoringEvidence
                    ? trustedTeacherCandidateAuthoringEvidence
                    : null,
                hasTeacherAuthoredEvaluationPairs
                    ? trustedTeacherAuthoredEvaluationPairs
                    : [],
                coordinatorOwnedPreferenceEvaluationHandled,
                coordinatorConsumedPreferenceEvidence,
                coordinatorPreferenceWarnings);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            cancellationToken.ThrowIfCancellationRequested();
            return RejectedEvaluationResolution.Unavailable;
        }
    }

    private void DiscardPendingRejectedEvaluation(string rootJobId)
        => (_rejectedEvaluationSource as IDesignConversionRuntimeRejectedEvaluationHandoff)
            ?.Discard(rootJobId);

    private sealed record RejectedEvaluationResolution(
        DesignConversionInternalEvaluationResult? Value,
        string? TrustedChosenEvaluationJobId,
        DesignConversionInternalEvaluationResult? TrustedChosenEvaluation,
        DesignConversionDpoTeacherCandidateAuthoringEvidence?
            TeacherCandidateAuthoringEvidence,
        IReadOnlyList<DesignConversionTeacherAuthoredEvaluationPair>
            TeacherAuthoredEvaluationPairs,
        bool CoordinatorOwnedPreferenceEvaluationHandled,
        IReadOnlyList<DesignConversionIndependentPreferenceEvidence>
            CoordinatorConsumedPreferenceEvidence,
        IReadOnlyList<string> Warnings)
    {
        public static RejectedEvaluationResolution Unavailable { get; } = new(
            null,
            null,
            null,
            null,
            [],
            false,
            [],
            ["runtime_rejected_evaluation_source_unavailable"]);
    }

    private DesignConversionThinClientStatusResult BuildRequiredLocalArtifactDbPersistenceFailure(
        DesignConversionThinClientJob runningJob,
        SubmittedDesignConversionRequest request,
        DesignConversionVisualCostLedgerRecord visualCostRecord,
        string errorCode)
        => MarkLocalArtifactDbPersistenceFailedClosed(
            MarkVisualCostLedgerPersistence(
                BuildFailedStatus(
                    runningJob,
                    request,
                    LocalArtifactDbDpoPersistenceSafeMessage,
                    errorCode),
                visualCostRecord),
            errorCode);

    private static bool IsMatchingDurableArtifactRecord(
        DesignConversionLocalArtifactDbRecord expected,
        DesignConversionLocalArtifactDbRecord actual)
    {
        if (!string.Equals(expected.RecordId, actual.RecordId, StringComparison.Ordinal)
            || !string.Equals(expected.DatabaseKind, actual.DatabaseKind, StringComparison.Ordinal)
            || !string.Equals(expected.JobId, actual.JobId, StringComparison.Ordinal)
            || !string.Equals(expected.RequestId, actual.RequestId, StringComparison.Ordinal)
            || !string.Equals(expected.SourceKind, actual.SourceKind, StringComparison.Ordinal)
            || !string.Equals(expected.Tier, actual.Tier, StringComparison.Ordinal)
            || !expected.CreatedAtUtc.Equals(actual.CreatedAtUtc)
            || !string.Equals(
                expected.ConversionResult.ResultId,
                actual.ConversionResult.ResultId,
                StringComparison.Ordinal)
            || !expected.Metadata.TryGetValue(
                JsonlDesignConversionLocalArtifactDb.ImmutableAppendFingerprintMetadataKey,
                out var expectedFingerprint)
            || !actual.Metadata.TryGetValue(
                JsonlDesignConversionLocalArtifactDb.ImmutableAppendFingerprintMetadataKey,
                out var actualFingerprint)
            || !string.Equals(expectedFingerprint, actualFingerprint, StringComparison.Ordinal))
        {
            return false;
        }

        return expected.DpoCandidates
            .Select(candidate => (candidate.CandidateId, candidate.RecordId, candidate.UnitId))
            .SequenceEqual(actual.DpoCandidates.Select(
                candidate => (candidate.CandidateId, candidate.RecordId, candidate.UnitId)));
    }

    private static DesignConversionVisualCostLedgerRecord BuildVisualCostLedgerRecord(
        DesignConversionInternalEvaluationResult evaluation,
        DesignConversionBudgetMetadata budget,
        DateTimeOffset createdAtUtc,
        string postEvaluationStateRef)
    {
        var recordedAtUtc = createdAtUtc.ToUniversalTime();
        var accounting = evaluation.VisualCostAccounting ?? DesignConversionVisualCostAccounting.Unavailable;
        var userRef = JsonlDesignConversionVisualCostLedger.BuildPseudonymousUserRef(budget.UserId);
        if (!userRef.IsSuccess)
            throw new InvalidOperationException("DesignConversion visual-cost ledger user ref source is required.");
        var period = JsonlDesignConversionVisualCostLedger.BuildUtcCalendarMonthPeriod(recordedAtUtc);
        var evaluationJobRef = JsonlDesignConversionVisualCostLedger.BuildCanonicalEvaluationJobRef(
            evaluation.JobId);
        var submittedRequestRef = JsonlDesignConversionVisualCostLedger.BuildCanonicalSubmittedRequestRef(
            evaluation.SubmittedRequest.RequestId);
        var recordId = JsonlDesignConversionVisualCostLedger.BuildRecordId(
            evaluationJobRef,
            submittedRequestRef,
            accounting);
        return new DesignConversionVisualCostLedgerRecord(
            RecordId: recordId,
            LedgerKind: JsonlDesignConversionVisualCostLedger.LedgerKind,
            SchemaVersion: JsonlDesignConversionVisualCostLedger.SchemaVersion,
            EvaluationJobRef: evaluationJobRef,
            SubmittedRequestRef: submittedRequestRef,
            TotalCostUsd: accounting.TotalCostUsd,
            MeasurementComplete: accounting.MeasurementComplete,
            MissingMeasurementCount: accounting.MissingMeasurementCount,
            EvaluationAttemptCount: accounting.EvaluationAttemptCount,
            CostSource: string.IsNullOrWhiteSpace(accounting.CostSource)
                ? DesignConversionVisualCostAccounting.UnknownCostSource
                : accounting.CostSource,
            RecordedAtUtc: recordedAtUtc,
            EvidenceRefs: [],
            Metadata: new Dictionary<string, string>
            {
                [JsonDesignConversionPostEvaluationStateStore.StateRefMetadataKey] =
                    postEvaluationStateRef,
                [JsonDesignConversionPostEvaluationStateStore.PersistenceStatusMetadataKey] =
                    JsonDesignConversionPostEvaluationStateStore.PersistenceProvisional,
                [JsonDesignConversionPostEvaluationStateStore.TrainingEligibleMetadataKey] =
                    bool.FalseString
            }) with
            {
                UserRef = userRef.Value!,
                SubmittedTier = JsonlDesignConversionVisualCostLedger.NormalizeSubmittedTier(budget.Tier),
                PeriodKind = JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind,
                PeriodStartUtc = period.StartUtc,
                PeriodEndExclusiveUtc = period.EndExclusiveUtc
            };
    }

    private async Task<OperationResult<VisualCostBudgetReservationContext?>> TryReserveVisualCostBudgetAsync(
        SubmittedDesignConversionRequest request,
        DesignConversionThinClientJob job,
        CancellationToken cancellationToken)
    {
        if (!IsTruthy(request.Provenance, TrustedEntitlementBoundKey))
            return OperationResult<VisualCostBudgetReservationContext?>.Ok(null);

        if (_visualCostBudgetStore is null)
        {
            return OperationResult<VisualCostBudgetReservationContext?>.Fail(
                "DesignConversion trusted visual-cost budget store is required before evaluation.",
                TrustedVisualCostBudgetStoreRequiredErrorCode);
        }

        var budgetRequest = BuildVisualCostBudgetReservationRequest(request, job, _utcNow());
        if (!budgetRequest.IsSuccess)
            return budgetRequest.MapError<VisualCostBudgetReservationContext?>();

        OperationResult<DesignConversionVisualCostReservationResult> reservation;
        try
        {
            reservation = await _visualCostBudgetStore.TryReserveAsync(budgetRequest.Value!.Request, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return OperationResult<VisualCostBudgetReservationContext?>.Fail(
                "DesignConversion trusted visual-cost budget reservation cancelled.",
                "CANCELLED");
        }
        catch (Exception)
        {
            return OperationResult<VisualCostBudgetReservationContext?>.Fail(
                "DesignConversion trusted visual-cost budget reservation failed closed.",
                TrustedVisualCostBudgetReservationFailedErrorCode);
        }

        if (!reservation.IsSuccess)
        {
            return OperationResult<VisualCostBudgetReservationContext?>.Fail(
                "DesignConversion trusted visual-cost budget reservation failed closed.",
                TrustedVisualCostBudgetReservationFailedErrorCode);
        }

        if (!reservation.Value!.Reserved)
        {
            return OperationResult<VisualCostBudgetReservationContext?>.Fail(
                "DesignConversion trusted visual-cost budget reservation was denied.",
                ResolveTrustedVisualCostReservationDenialErrorCode(reservation.Value.DenialReasonCode));
        }

        return OperationResult<VisualCostBudgetReservationContext?>.Ok(
            budgetRequest.Value with
            {
                Reservation = reservation.Value
            });
    }

    private static OperationResult<VisualCostBudgetReservationContext> BuildVisualCostBudgetReservationRequest(
        SubmittedDesignConversionRequest request,
        DesignConversionThinClientJob job,
        DateTimeOffset requestedAtUtc)
    {
        var metadata = request.Budget.Metadata ?? new Dictionary<string, string>();
        var provenance = request.Provenance ?? new Dictionary<string, string>();

        if (!IsTruthy(provenance, TrustedEntitlementBoundKey) ||
            !IsTruthy(provenance, TrustedReservationStoreWiredKey) ||
            !IsTruthy(metadata, TrustedEntitlementBoundKey) ||
            !IsTruthy(metadata, TrustedReservationStoreWiredKey))
        {
            return InvalidTrustedVisualCostContract();
        }

        if (!HasExact(metadata, TrustedEntitlementAuthorityKey, PlatformTenantConfigurationAuthority) ||
            !HasExact(metadata, DesignTierAuthorityKey, PlatformTenantConfigurationAuthority) ||
            !HasExact(metadata, MonthlyVisualCostLimitAuthorityKey, PlatformTenantConfigurationAuthority) ||
            !HasExact(metadata, TrustedUserIdentityAuthorityKey, VerifiedInboundPrincipalAuthority) ||
            !HasExact(metadata, UserIdentityAuthorityKey, VerifiedInboundPrincipalAuthority) ||
            !HasExact(metadata, TrustedPeriodKindKey, JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind))
        {
            return InvalidTrustedVisualCostContract();
        }

        if (!TryReadRequired(metadata, TrustedUserRefKey, out var trustedUserRef) ||
            !TryReadRequired(metadata, TrustedTenantIdRefKey, out var trustedTenantIdRef) ||
            !TryReadRequired(metadata, ServerAssignedDesignTierKey, out var submittedTier) ||
            !TryReadRequired(metadata, ServerAssignedMonthlyLimitUsdKey, out var monthlyLimitText) ||
            !TryParseCanonicalUsd(monthlyLimitText, out var monthlyLimitUsd))
        {
            return InvalidTrustedVisualCostContract();
        }

        if (!string.Equals(request.Budget.Tier, submittedTier, StringComparison.Ordinal) ||
            !HasExact(provenance, "trusted_visual_cost_entitlement_user_ref", trustedUserRef) ||
            !HasExact(provenance, "trusted_visual_cost_entitlement_tenant_id_ref", trustedTenantIdRef) ||
            !HasExact(provenance, "trusted_visual_cost_entitlement_design_tier", submittedTier) ||
            !HasExact(provenance, "trusted_visual_cost_entitlement_monthly_limit_usd", monthlyLimitText) ||
            !HasExact(provenance, "trusted_visual_cost_entitlement_period_kind", JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind))
        {
            return InvalidTrustedVisualCostContract();
        }

        var requestedReserveUsd = DesignConversionVisualQaBoundaryPolicy.ResolveEffectiveTierBudgetUsd(
            request.Budget.BudgetUsd,
            submittedTier);
        if (requestedReserveUsd <= 0m)
            return InvalidTrustedVisualCostContract();

        var compositeIdentity = string.Join("|", "tenant", trustedTenantIdRef, "user", trustedUserRef);
        var storeUserRef = JsonlDesignConversionVisualCostLedger.BuildPseudonymousUserRef(compositeIdentity);
        if (!storeUserRef.IsSuccess)
            return InvalidTrustedVisualCostContract();

        var period = JsonlDesignConversionVisualCostLedger.BuildUtcCalendarMonthPeriod(requestedAtUtc.ToUniversalTime());
        var reservationRef = JsonlDesignConversionVisualCostBudgetStore.BuildCanonicalReservationRef(string.Join(
            "|",
            job.JobId,
            request.RequestId,
            storeUserRef.Value!,
            trustedTenantIdRef,
            trustedUserRef,
            submittedTier,
            period.StartUtc.ToString("O", CultureInfo.InvariantCulture),
            requestedReserveUsd.ToString("0.####", CultureInfo.InvariantCulture),
            monthlyLimitText));
        var reservationRequest = new DesignConversionVisualCostReservationRequest(
            ReservationRef: reservationRef,
            UserRef: storeUserRef.Value!,
            SubmittedTier: submittedTier,
            PeriodStartUtc: period.StartUtc,
            PeriodEndExclusiveUtc: period.EndExclusiveUtc,
            MonthlyLimitUsd: monthlyLimitUsd,
            RequestedReserveUsd: requestedReserveUsd,
            RequestedAtUtc: requestedAtUtc.ToUniversalTime());

        return OperationResult<VisualCostBudgetReservationContext>.Ok(new VisualCostBudgetReservationContext(
            Request: reservationRequest,
            Reservation: null));
    }

    private async Task<OperationResult<VisualCostBudgetSettlementContext>> SettleVisualCostBudgetAsync(
        VisualCostBudgetReservationContext reservation,
        OperationResult<DesignConversionInternalEvaluationResult> evaluation,
        CancellationToken cancellationToken)
    {
        if (_visualCostBudgetStore is null)
        {
            return OperationResult<VisualCostBudgetSettlementContext>.Fail(
                "DesignConversion trusted visual-cost budget store is required for settlement.",
                TrustedVisualCostBudgetSettlementFailedErrorCode);
        }

        var plan = BuildVisualCostBudgetSettlementPlan(reservation, evaluation);

        OperationResult<DesignConversionVisualCostSettlementResult> settlement;
        try
        {
            settlement = await _visualCostBudgetStore.SettleAsync(
                new DesignConversionVisualCostSettlementRequest(
                    ReservationRef: reservation.Request.ReservationRef,
                    UserRef: reservation.Request.UserRef,
                    PeriodStartUtc: reservation.Request.PeriodStartUtc,
                    PeriodEndExclusiveUtc: reservation.Request.PeriodEndExclusiveUtc,
                    ActualCostUsd: plan.ChargedCostUsd,
                    SettledAtUtc: _utcNow().ToUniversalTime()),
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            throw;
        }

        if (!settlement.IsSuccess)
        {
            return OperationResult<VisualCostBudgetSettlementContext>.Fail(
                "DesignConversion trusted visual-cost budget settlement failed closed.",
                TrustedVisualCostBudgetSettlementFailedErrorCode);
        }

        return OperationResult<VisualCostBudgetSettlementContext>.Ok(new VisualCostBudgetSettlementContext(
            Settlement: settlement.Value!,
            ChargedCostUsd: plan.ChargedCostUsd,
            ActualCostKnown: plan.ActualCostKnown,
            FailJob: plan.FailJob,
            FailureCode: plan.FailureCode,
            FailureMessage: plan.FailureMessage));
    }

    private static VisualCostBudgetSettlementPlan BuildVisualCostBudgetSettlementPlan(
        VisualCostBudgetReservationContext reservation,
        OperationResult<DesignConversionInternalEvaluationResult> evaluation)
    {
        var accounting = evaluation.IsSuccess
            ? evaluation.Value!.VisualCostAccounting ?? DesignConversionVisualCostAccounting.Unavailable
            : DesignConversionVisualCostAccounting.Unavailable;
        var actualCostKnown =
            evaluation.IsSuccess &&
            accounting.MeasurementComplete &&
            accounting.MissingMeasurementCount == 0 &&
            accounting.EvaluationAttemptCount > 0 &&
            accounting.TotalCostUsd >= 0m &&
            accounting.TotalCostUsd <= reservation.Request.RequestedReserveUsd;
        var chargedCostUsd = actualCostKnown
            ? accounting.TotalCostUsd
            : reservation.Request.RequestedReserveUsd;
        var failJob = evaluation.IsSuccess && !actualCostKnown;
        var failureCode = accounting.TotalCostUsd > reservation.Request.RequestedReserveUsd
            ? TrustedVisualCostActualExceedsReservationErrorCode
            : TrustedVisualCostAccountingIncompleteErrorCode;
        var failureMessage = accounting.TotalCostUsd > reservation.Request.RequestedReserveUsd
            ? "DesignConversion trusted visual-cost accounting exceeded the reserved amount."
            : "DesignConversion trusted visual-cost accounting was incomplete and failed closed.";
        return new VisualCostBudgetSettlementPlan(
            chargedCostUsd,
            actualCostKnown,
            failJob,
            failureCode,
            failureMessage);
    }

    private async Task<OperationResult<VisualCostBudgetReleaseContext>> ReleaseVisualCostBudgetReservationAsync(
        VisualCostBudgetReservationContext reservation,
        CancellationToken cancellationToken = default)
    {
        if (_visualCostBudgetStore is null)
        {
            return OperationResult<VisualCostBudgetReleaseContext>.Fail(
                "DesignConversion trusted visual-cost budget store is required for release.",
                TrustedVisualCostBudgetSettlementFailedErrorCode);
        }

        OperationResult<DesignConversionVisualCostReleaseResult> release;
        try
        {
            release = await _visualCostBudgetStore.ReleaseAsync(
                new DesignConversionVisualCostReleaseRequest(
                    ReservationRef: reservation.Request.ReservationRef,
                    UserRef: reservation.Request.UserRef,
                    PeriodStartUtc: reservation.Request.PeriodStartUtc,
                    PeriodEndExclusiveUtc: reservation.Request.PeriodEndExclusiveUtc,
                    ReleasedAtUtc: DateTimeOffset.UtcNow),
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return OperationResult<VisualCostBudgetReleaseContext>.Fail(
                "DesignConversion trusted visual-cost budget release cancelled.",
                "CANCELLED");
        }
        catch (Exception)
        {
            return OperationResult<VisualCostBudgetReleaseContext>.Fail(
                "DesignConversion trusted visual-cost budget release failed closed.",
                TrustedVisualCostBudgetSettlementFailedErrorCode);
        }

        if (!release.IsSuccess)
        {
            return OperationResult<VisualCostBudgetReleaseContext>.Fail(
                "DesignConversion trusted visual-cost budget release failed closed.",
                TrustedVisualCostBudgetSettlementFailedErrorCode);
        }

        return OperationResult<VisualCostBudgetReleaseContext>.Ok(new VisualCostBudgetReleaseContext(release.Value!));
    }

    private static DesignConversionThinClientStatusResult MarkVisualCostBudgetReservationRelease(
        DesignConversionThinClientStatusResult status,
        VisualCostBudgetReservationContext reservation,
        VisualCostBudgetReleaseContext release,
        string releaseEnforcement)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [TrustedReservationStoreWiredKey] = bool.TrueString,
            ["trusted_visual_cost_budget_reserved"] = bool.TrueString,
            ["trusted_visual_cost_budget_reservation_ref"] = reservation.Request.ReservationRef,
            ["trusted_visual_cost_budget_user_ref"] = reservation.Request.UserRef,
            ["trusted_visual_cost_budget_submitted_tier"] = reservation.Request.SubmittedTier,
            ["trusted_visual_cost_budget_period_kind"] = JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind,
            ["trusted_visual_cost_budget_period_start_utc"] =
                reservation.Request.PeriodStartUtc.ToString("O", CultureInfo.InvariantCulture),
            ["trusted_visual_cost_budget_period_end_exclusive_utc"] =
                reservation.Request.PeriodEndExclusiveUtc.ToString("O", CultureInfo.InvariantCulture),
            ["trusted_visual_cost_budget_requested_reserve_usd"] =
                reservation.Request.RequestedReserveUsd.ToString("0.####", CultureInfo.InvariantCulture),
            ["trusted_visual_cost_budget_settled"] = bool.FalseString,
            ["trusted_visual_cost_budget_release_used"] = bool.TrueString,
            ["trusted_visual_cost_budget_release_status"] = release.Release.Status,
            ["trusted_visual_cost_budget_release_replayed"] =
                release.Release.Replayed.ToString(CultureInfo.InvariantCulture),
            ["trusted_visual_cost_budget_released_reserve_usd"] =
                release.Release.ReleasedReserveUsd.ToString("0.####", CultureInfo.InvariantCulture),
            ["trusted_visual_cost_budget_active_reserved_usd_after_release"] =
                release.Release.ActiveReservedUsd.ToString("0.####", CultureInfo.InvariantCulture),
            ["trusted_visual_cost_budget_enforcement"] = releaseEnforcement
        };

        return status with
        {
            Job = status.Job with
            {
                Metadata = MergeDictionaries(status.Job.Metadata, metadata)
            },
            Warnings = status.Warnings
                .Concat([
                    "trusted_visual_cost_budget_settled=false",
                    "trusted_visual_cost_budget_release_used=true",
                    $"trusted_visual_cost_budget_enforcement={releaseEnforcement}"
                ])
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Provenance = MergeDictionaries(status.Provenance, metadata)
        };
    }

    private static DesignConversionThinClientStatusResult MarkVisualCostBudgetReservationReleaseFailed(
        DesignConversionThinClientStatusResult status,
        VisualCostBudgetReservationContext reservation,
        string releaseFailureCode,
        string releaseEnforcement)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [TrustedReservationStoreWiredKey] = bool.TrueString,
            ["trusted_visual_cost_budget_reserved"] = bool.TrueString,
            ["trusted_visual_cost_budget_reservation_ref"] = reservation.Request.ReservationRef,
            ["trusted_visual_cost_budget_user_ref"] = reservation.Request.UserRef,
            ["trusted_visual_cost_budget_release_used"] = bool.FalseString,
            ["trusted_visual_cost_budget_release_failed"] = bool.TrueString,
            ["trusted_visual_cost_budget_release_failure_code"] = releaseFailureCode,
            ["trusted_visual_cost_budget_settled"] = bool.FalseString,
            ["trusted_visual_cost_budget_enforcement"] = releaseEnforcement + "_release_failed"
        };

        return status with
        {
            Job = status.Job with
            {
                Metadata = MergeDictionaries(status.Job.Metadata, metadata)
            },
            Warnings = status.Warnings
                .Concat([
                    "trusted_visual_cost_budget_settled=false",
                    "trusted_visual_cost_budget_release_used=false",
                    "trusted_visual_cost_budget_release_failed=true",
                    $"trusted_visual_cost_budget_release_failure_code={releaseFailureCode}",
                    $"trusted_visual_cost_budget_enforcement={releaseEnforcement}_release_failed"
                ])
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Provenance = MergeDictionaries(status.Provenance, metadata)
        };
    }

    private static DesignConversionThinClientStatusResult MarkVisualCostBudgetReservation(
        DesignConversionThinClientStatusResult status,
        VisualCostBudgetReservationContext? reservation,
        VisualCostBudgetSettlementContext? settlement)
    {
        if (reservation is null)
            return status;

        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [TrustedReservationStoreWiredKey] = bool.TrueString,
            ["trusted_visual_cost_budget_reserved"] = bool.TrueString,
            ["trusted_visual_cost_budget_reservation_ref"] = reservation.Request.ReservationRef,
            ["trusted_visual_cost_budget_user_ref"] = reservation.Request.UserRef,
            ["trusted_visual_cost_budget_submitted_tier"] = reservation.Request.SubmittedTier,
            ["trusted_visual_cost_budget_period_kind"] = JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind,
            ["trusted_visual_cost_budget_period_start_utc"] =
                reservation.Request.PeriodStartUtc.ToString("O", CultureInfo.InvariantCulture),
            ["trusted_visual_cost_budget_period_end_exclusive_utc"] =
                reservation.Request.PeriodEndExclusiveUtc.ToString("O", CultureInfo.InvariantCulture),
            ["trusted_visual_cost_budget_monthly_limit_usd"] =
                reservation.Request.MonthlyLimitUsd.ToString("0.####", CultureInfo.InvariantCulture),
            ["trusted_visual_cost_budget_requested_reserve_usd"] =
                reservation.Request.RequestedReserveUsd.ToString("0.####", CultureInfo.InvariantCulture),
            ["trusted_visual_cost_budget_reservation_status"] =
                reservation.Reservation?.Status ?? JsonlDesignConversionVisualCostBudgetStore.StatusReserved,
            ["trusted_visual_cost_budget_reservation_replayed"] =
                (reservation.Reservation?.Replayed ?? false).ToString(CultureInfo.InvariantCulture),
            ["trusted_visual_cost_budget_settled"] = (settlement is not null).ToString(CultureInfo.InvariantCulture),
            ["trusted_visual_cost_budget_settlement_status"] = settlement?.Settlement.Status ?? "not-settled",
            ["trusted_visual_cost_budget_settlement_replayed"] =
                (settlement?.Settlement.Replayed ?? false).ToString(CultureInfo.InvariantCulture),
            ["trusted_visual_cost_budget_charged_cost_usd"] =
                (settlement?.ChargedCostUsd ?? 0m).ToString("0.####", CultureInfo.InvariantCulture),
            ["trusted_visual_cost_budget_actual_cost_known"] =
                (settlement?.ActualCostKnown ?? false).ToString(CultureInfo.InvariantCulture),
            ["trusted_visual_cost_budget_release_used"] = bool.FalseString,
            ["trusted_visual_cost_budget_enforcement"] = "reserve_before_evaluator_settle_after_invocation"
        };

        return status with
        {
            Job = status.Job with
            {
                Metadata = MergeDictionaries(status.Job.Metadata, metadata)
            },
            Warnings = status.Warnings
                .Concat([
                    "trusted_visual_cost_reservation_store_wired=true",
                    "trusted_visual_cost_budget_reserved=true",
                    settlement is null
                        ? "trusted_visual_cost_budget_settled=false"
                        : "trusted_visual_cost_budget_settled=true",
                    settlement?.ActualCostKnown == true
                        ? "trusted_visual_cost_budget_actual_cost_known=true"
                        : "trusted_visual_cost_budget_actual_cost_known=false",
                    "trusted_visual_cost_budget_release_used=false"
                ])
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Provenance = MergeDictionaries(status.Provenance, metadata)
        };
    }

    private static DesignConversionThinClientStatusResult MarkPostEvaluationFinalizationFailure(
        DesignConversionThinClientStatusResult status,
        string errorCode)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["post_evaluation_finalization_bounded"] = bool.TrueString,
            ["post_evaluation_finalization_completed"] = bool.FalseString,
            ["post_evaluation_finalization_error_code"] = errorCode,
            ["post_evaluation_finalization_dependency_may_complete_late"] = bool.TrueString,
            ["post_evaluation_finalization_late_status_overwrite_allowed"] = bool.FalseString
        };

        return status with
        {
            Job = status.Job with
            {
                Metadata = MergeDictionaries(status.Job.Metadata, metadata)
            },
            Warnings = status.Warnings
                .Concat([
                    "post_evaluation_finalization_bounded=true",
                    "post_evaluation_finalization_completed=false",
                    $"post_evaluation_finalization_error_code={errorCode}",
                    "post_evaluation_finalization_dependency_may_complete_late=true",
                    "post_evaluation_finalization_late_status_overwrite_allowed=false"
                ])
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Provenance = MergeDictionaries(status.Provenance, metadata)
        };
    }

    private static DesignConversionThinClientStatusResult MarkPostEvaluationAccountingState(
        DesignConversionThinClientStatusResult status,
        string? stateRef,
        bool reconciliationRequired)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["accounting_reconciliation_required"] =
                reconciliationRequired.ToString(CultureInfo.InvariantCulture),
            ["accounting_reconciliation_state"] = reconciliationRequired
                ? JsonDesignConversionPostEvaluationStateStore.AccountingReconciliationRequired
                : "durably_finalized"
        };
        if (!string.IsNullOrWhiteSpace(stateRef))
        {
            metadata[JsonDesignConversionPostEvaluationStateStore.StateRefMetadataKey] =
                stateRef;
        }

        return status with
        {
            Job = status.Job with
            {
                Metadata = MergeDictionaries(status.Job.Metadata, metadata)
            },
            Warnings = status.Warnings
                .Concat([
                    $"accounting_reconciliation_required={reconciliationRequired.ToString(CultureInfo.InvariantCulture)}",
                    $"accounting_reconciliation_state={metadata["accounting_reconciliation_state"]}"
                ])
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Provenance = MergeDictionaries(status.Provenance, metadata)
        };
    }

    private static OperationResult<VisualCostBudgetReservationContext> InvalidTrustedVisualCostContract()
        => OperationResult<VisualCostBudgetReservationContext>.Fail(
            "DesignConversion trusted visual-cost budget contract is invalid.",
            TrustedVisualCostBudgetContractInvalidErrorCode);

    private static bool TryReadRequired(
        IReadOnlyDictionary<string, string> metadata,
        string key,
        out string value)
    {
        value = string.Empty;
        if (!metadata.TryGetValue(key, out var raw) ||
            string.IsNullOrWhiteSpace(raw) ||
            !string.Equals(raw, raw.Trim(), StringComparison.Ordinal))
        {
            return false;
        }

        value = raw;
        return true;
    }

    private static bool HasExact(
        IReadOnlyDictionary<string, string> metadata,
        string key,
        string expected)
        => metadata.TryGetValue(key, out var value) &&
           string.Equals(value, expected, StringComparison.Ordinal);

    private static bool IsTruthy(IReadOnlyDictionary<string, string>? metadata, string key)
        => metadata is not null &&
           metadata.TryGetValue(key, out var value) &&
           bool.TryParse(value, out var parsed) &&
           parsed;

    private static bool TryParseCanonicalUsd(string value, out decimal amount)
    {
        amount = 0m;
        if (string.IsNullOrWhiteSpace(value) ||
            !string.Equals(value, value.Trim(), StringComparison.Ordinal) ||
            value.Contains("e", StringComparison.OrdinalIgnoreCase) ||
            value.Contains('+') ||
            value.Contains('-'))
        {
            return false;
        }

        if (!decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out amount))
            return false;

        return amount > 0m &&
               string.Equals(amount.ToString("0.####", CultureInfo.InvariantCulture), value, StringComparison.Ordinal);
    }

    private static string SafeVisualCostBudgetErrorCode(string? errorCode, string fallback)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
            return fallback;

        var safe = new string(errorCode
            .Where(ch => char.IsLetterOrDigit(ch) || ch is '_' or '-')
            .ToArray());
        return string.IsNullOrWhiteSpace(safe) ? fallback : safe;
    }

    private static string ResolveTrustedVisualCostReservationDenialErrorCode(string? denialReasonCode)
        => string.Equals(
            denialReasonCode,
            JsonlDesignConversionVisualCostBudgetStore.DenialReasonMonthlyLimitExceeded,
            StringComparison.Ordinal)
            ? JsonlDesignConversionVisualCostBudgetStore.DenialReasonMonthlyLimitExceeded
            : TrustedVisualCostBudgetReservationDeniedErrorCode;

    private static OperationResult<bool> ValidateSubmission(SubmittedDesignConversionRequest request)
    {
        if (request is null)
            return OperationResult<bool>.Fail("DesignConversion thin-client submission is required.", "REQUEST_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.RequestId))
            return OperationResult<bool>.Fail("DesignConversion request id is required.", "REQUEST_ID_REQUIRED");
        var treeContent = DesignConversionInternalService.ValidateTreeContent(
            request.DesignTreePayload,
            request.OriginalTreePayload);
        if (!treeContent.IsSuccess)
            return treeContent;
        if (request.Budget is null)
            return OperationResult<bool>.Fail("DesignConversion budget metadata is required.", "BUDGET_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.Budget.UserId))
            return OperationResult<bool>.Fail("DesignConversion budget user id is required.", "USER_ID_REQUIRED");
        if (request.Budget.BudgetUsd < 0m)
            return OperationResult<bool>.Fail(
                "DesignConversion improvement-loop budgetUsd must be greater than or equal to zero.",
                DesignConversionInternalService.NegativeBudgetUsdErrorCode);
        if (request.Budget.MaxImprovementCandidates < 0 ||
            (request.Budget.MaxImprovementCandidates == 0 &&
             !DesignConversionVisualQaBoundaryPolicy.IsServerOwnedEntitlementTier(request.Budget.Tier)))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion improvement candidate budget must be greater than zero.",
                DesignConversionInternalService.ImprovementCandidateBudgetRequiredCode);
        }
        if (request.Budget.MaxImprovementCandidates > DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts)
        {
            return OperationResult<bool>.Fail(
                $"DesignConversion improvement-loop max attempts must be less than or equal to {DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts}.",
                DesignConversionImprovementLoopThinClientJobEvaluator.MaxAttemptsExceededErrorCode);
        }
        if (!DesignConversionVisualQaBoundaryPolicy.IsValidConfiguredMaxImprovementIterations(
                request.Budget.MaxImprovementIterations))
        {
            return OperationResult<bool>.Fail(
                DesignConversionVisualQaBoundaryPolicy.MaxImprovementIterationsInvalidErrorMessage(),
                DesignConversionVisualQaBoundaryPolicy.MaxImprovementIterationsInvalidErrorCode);
        }
        if (!DesignConversionVisualQaBoundaryPolicy.IsValidConfiguredMaxTeacherCalls(
                request.Budget.MaxTeacherCalls))
        {
            return OperationResult<bool>.Fail(
                DesignConversionVisualQaBoundaryPolicy.MaxTeacherCallsInvalidErrorMessage(),
                DesignConversionVisualQaBoundaryPolicy.MaxTeacherCallsInvalidErrorCode);
        }
        if (request.Budget.MaxRenderQaRuns <= 0)
            return OperationResult<bool>.Fail("DesignConversion render QA budget must be greater than zero.", "RENDER_QA_BUDGET_REQUIRED");
        if ((request.DesignRenders is null || request.DesignRenders.Count == 0) &&
            (request.Screenshots is null || request.Screenshots.Count == 0))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion expected design render or screenshot is required.",
                "EXPECTED_RENDER_REQUIRED");
        }

        var renderResolutions = BuildRenderResolutions(request);
        if (!renderResolutions.IsSuccess)
            return renderResolutions.MapError<bool>();

        if (renderResolutions.Value!.Count == 0)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion render artifacts must include positive width and height.",
                "RENDER_RESOLUTION_REQUIRED");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<DesignConversionInternalEvaluateRequest> BuildInternalEvaluateRequest(
        SubmittedDesignConversionRequest request,
        DesignConversionThinClientJob job)
    {
        var renderResolutions = BuildRenderResolutions(request);
        if (!renderResolutions.IsSuccess)
            return renderResolutions.MapError<DesignConversionInternalEvaluateRequest>();

        if (renderResolutions.Value!.Count == 0)
        {
            return OperationResult<DesignConversionInternalEvaluateRequest>.Fail(
                "DesignConversion render artifacts must include positive width and height.",
                "RENDER_RESOLUTION_REQUIRED");
        }

        return OperationResult<DesignConversionInternalEvaluateRequest>.Ok(new DesignConversionInternalEvaluateRequest(
            JobId: job.JobId,
            RequestId: request.RequestId,
            SourceKind: request.SourceKind,
            Html: request.Html,
            Css: request.Css,
            DesignTreePayload: request.DesignTreePayload,
            OriginalTreePayload: request.OriginalTreePayload,
            Screenshots: request.Screenshots?.ToArray() ?? [],
            DesignRenders: request.DesignRenders?.ToArray() ?? [],
            Budget: request.Budget,
            RenderResolutions: renderResolutions.Value,
            AiTeacherEvidenceRefs: ["thin-client-runtime:no-ai-teacher-executed"],
            DeterministicEvidenceRefs: new Dictionary<string, string>
            {
                ["thin_client_job_runtime"] = RuntimeBoundary,
                ["status_store"] = StorageBoundary,
                ["execution_path"] = DeterministicExecutionBoundary,
                ["public_api_verified"] = "false",
                ["persistence_verified"] = "false",
                ["product_ready_verified"] = "false",
                ["runtime_ready_verified"] = "false",
                ["trained_feature_verified"] = "false"
            },
            Provenance: MergeDictionaries(
                BuildStatusProvenance(request),
                new Dictionary<string, string>
                {
                    ["thin_client_job_id"] = job.JobId,
                    ["thin_client_status_uri"] = job.StatusUri,
                    ["submission_mapper"] = nameof(DesignConversionThinClientJobRuntime),
                    ["runtime_boundary"] = RuntimeBoundary,
                    ["storage_boundary"] = StorageBoundary
                })));
    }

    private static OperationResult<IReadOnlyList<RenderQaResolution>> BuildRenderResolutions(
        SubmittedDesignConversionRequest request)
    {
        var artifacts = (request.DesignRenders ?? []).Concat(request.Screenshots ?? []);
        return IsExactTrue(request.Provenance, ServerOwnedRequiredBrowserQaMatrixProvenanceKey)
            ? DesignConversionRenderResolutionFactory.CreateModelServerRequiredBrowserQaMatrix(artifacts)
            : DesignConversionRenderResolutionFactory.CreateModelServerMinimumResponsiveMatrix(artifacts);
    }

    private DesignConversionThinClientStatusResult BuildCompletedStatus(
        DesignConversionThinClientJob runningJob,
        SubmittedDesignConversionRequest request,
        DesignConversionInternalEvaluationResult evaluation)
    {
        var visualDecision = evaluation.VisualQaBoundaryTrace.VisualDecision;
        var terminalStatus = DesignConversionThinClientStatus.FromVisualQaDecision(visualDecision.Decision);
        var maxTeacherCallsStatusMetadata = BuildMaxTeacherCallsStatusMetadata(request.Budget, evaluation);
        var updatedJob = UpdateJob(runningJob, terminalStatus, errorCode: null);
        var completedJob = updatedJob with
        {
            Metadata = MergeDictionaries(
                updatedJob.Metadata,
                maxTeacherCallsStatusMetadata)
        };
        var artifacts = new[]
        {
            BuildArtifact(completedJob, "improved-html", "text/html", evaluation.ImprovedHtml),
            BuildArtifact(completedJob, "improved-css", "text/css", evaluation.ImprovedCss),
            BuildArtifact(completedJob, "runtime-flow-output", "application/json", evaluation.RuntimeFlowResult.Output.OutputId)
        };
        var warnings = evaluation.Warnings
            .Concat(evaluation.DeterministicLogicClaimedAsLearning
                ? ["deterministic_logic_claimed_as_learning=true"]
                : ["deterministic_logic_claimed_as_learning=false"])
            .Concat([
                $"final_visual_decision={visualDecision.Decision}",
                $"tier_policy_stop_reason={evaluation.VisualQaBoundaryTrace.TierPolicy.StopReason}",
                $"thin_client_terminal_status={terminalStatus}"
            ])
            .ToArray();
        var correctionFindings = visualDecision.CorrectionFindings ?? [];

        return BuildStatusResult(
            completedJob,
            terminalStatus,
            BuildTerminalStatusMessage(terminalStatus),
            artifacts,
            warnings,
            MergeDictionaries(
                BuildStatusProvenance(request),
                new Dictionary<string, string>
                {
                    ["internal_evaluation_job_id"] = evaluation.JobId,
                    ["runtime_flow_unit_order"] = string.Join(">", evaluation.RuntimeFlowResult.UnitExecutionOrder),
                    ["deterministic_logic_claimed_as_learning"] = evaluation.DeterministicLogicClaimedAsLearning ? "true" : "false",
                    ["final_visual_decision"] = visualDecision.Decision,
                    ["tier_policy_stop_reason"] = evaluation.VisualQaBoundaryTrace.TierPolicy.StopReason,
                    ["thin_client_terminal_status"] = terminalStatus
                }
                .Concat(maxTeacherCallsStatusMetadata)
                .GroupBy(pair => pair.Key, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal)),
            BuildStatusQualityMetrics(visualDecision, correctionFindings.Count),
            BuildStatusDefects(correctionFindings));
    }

    private static bool IsTerminalStatus(string status)
        => string.Equals(status, DesignConversionThinClientStatus.Completed, StringComparison.Ordinal) ||
           string.Equals(status, DesignConversionThinClientStatus.Quarantined, StringComparison.Ordinal) ||
           string.Equals(status, DesignConversionThinClientStatus.NotReady, StringComparison.Ordinal) ||
           string.Equals(status, DesignConversionThinClientStatus.Failed, StringComparison.Ordinal) ||
           string.Equals(status, DesignConversionThinClientStatus.Cancelled, StringComparison.Ordinal);

    private static string BuildTerminalStatusMessage(string terminalStatus)
        => terminalStatus switch
        {
            DesignConversionThinClientStatus.Completed =>
                "In-process thin-client job completed through the injected internal evaluator.",
            DesignConversionThinClientStatus.Quarantined =>
                "In-process thin-client job is quarantined by the final visual QA boundary decision.",
            _ =>
                "In-process thin-client job is not ready after the final visual QA boundary decision."
        };

    private static DesignConversionThinClientStatusQualityMetrics BuildStatusQualityMetrics(
        DesignConversionVisualComparisonDecision visualDecision,
        int correctionFindingCount)
    {
        var score = visualDecision.QualityScore;
        var requiredThreshold = visualDecision.RequiredThreshold;
        if (!IsFiniteUnitInterval(score) || !IsFiniteUnitInterval(requiredThreshold))
        {
            return DesignConversionThinClientStatusQualityMetrics.Unavailable(
                source: "final_visual_verdict",
                unavailableReason: "invalid_final_visual_quality_metrics") with
            {
                DefectCount = correctionFindingCount
            };
        }

        return new DesignConversionThinClientStatusQualityMetrics(
            Available: true,
            Score: score,
            RequiredThreshold: requiredThreshold,
            Passed: string.Equals(
                visualDecision.Decision,
                DesignConversionVisualQaBoundaryPolicy.DecisionAccepted,
                StringComparison.Ordinal),
            ThresholdMet: visualDecision.QualityThresholdMet,
            DefectCount: correctionFindingCount,
            Source: "final_visual_verdict",
            ThresholdSource: "VisualQaBoundaryTrace.VisualDecision.RequiredThreshold",
            UnavailableReason: null);
    }

    private static IReadOnlyList<DesignConversionThinClientStatusDefect> BuildStatusDefects(
        IReadOnlyList<DesignConversionVisualCorrectionFinding> correctionFindings)
        => correctionFindings
            .Select((finding, index) =>
            {
                var category = NormalizeStatusDefectCategory(finding.Category);
                var severity = NormalizeStatusDefectSeverity(finding.Severity);
                var defectRef = BuildStatusHashRef(
                    string.IsNullOrWhiteSpace(finding.FindingId)
                        ? $"visual-correction-finding:{index + 1}"
                        : finding.FindingId);
                var evidenceRefs = (finding.EvidenceImageIds ?? [])
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Select(BuildStatusHashRef)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
                if (evidenceRefs.Length == 0)
                    evidenceRefs = [defectRef];

                return new DesignConversionThinClientStatusDefect(
                    DefectRef: defectRef,
                    Category: category,
                    Severity: severity,
                    Message: BuildStatusDefectMessage(severity, category),
                    Ordinal: index + 1,
                    EvidenceRefs: evidenceRefs);
            })
            .ToArray();

    private static bool IsFiniteUnitInterval(double value)
        => double.IsFinite(value) && value >= 0.0 && value <= 1.0;

    private static string NormalizeStatusDefectCategory(string? value)
    {
        var normalized = NormalizeStatusLabel(value);
        return normalized is
            "alignment" or
            "artifact" or
            "color" or
            "content" or
            "layout" or
            "quality_threshold_not_met" or
            "responsive" or
            "size" or
            "spacing" or
            "typography"
                ? normalized
                : "other";
    }

    private static string NormalizeStatusDefectSeverity(string? value)
    {
        var normalized = NormalizeStatusLabel(value);
        return normalized is "blocker" or "critical" or "major" or "minor" or "info"
            ? normalized
            : "other";
    }

    private static string NormalizeStatusLabel(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? "other"
            : value.Trim().ToLowerInvariant().Replace('-', '_').Replace(' ', '_');

    private static string BuildStatusDefectMessage(string severity, string category)
        => $"{ToStatusDisplayLabel(severity)} {category.Replace('_', ' ')} issue reported by visual QA.";

    private static string ToStatusDisplayLabel(string value)
        => string.IsNullOrEmpty(value)
            ? "Other"
            : char.ToUpperInvariant(value[0]) + value[1..];

    private static string BuildStatusHashRef(string value)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();
        return $"sha256:{hash}";
    }

    private DesignConversionThinClientStatusResult BuildFailedStatus(
        DesignConversionThinClientJob runningJob,
        SubmittedDesignConversionRequest request,
        string message,
        string errorCode)
    {
        var status = string.Equals(errorCode, "CANCELLED", StringComparison.Ordinal)
            ? DesignConversionThinClientStatus.Cancelled
            : DesignConversionThinClientStatus.Failed;
        var failedJob = UpdateJob(runningJob, status, errorCode);
        var isBrowserRuntimeBlocker = IsBrowserRuntimeBlockerErrorCode(errorCode);
        var safeMessage = ResolveFailureSafeMessage(message, errorCode, isBrowserRuntimeBlocker);
        var extraWarnings = isBrowserRuntimeBlocker
            ? new[]
            {
                $"internal_evaluator_error_code={errorCode}",
                $"{BrowserRuntimeBlockerProvenanceKey}=true",
                $"{BrowserRuntimeBlockerErrorCodeProvenanceKey}={errorCode}"
            }
            : [$"internal_evaluator_error_code={errorCode}"];
        var failureProvenance = new Dictionary<string, string>
        {
            ["internal_evaluator_error_code"] = errorCode,
            ["runtime_boundary"] = RuntimeBoundary
        };
        if (isBrowserRuntimeBlocker)
        {
            failureProvenance[BrowserRuntimeBlockerProvenanceKey] = bool.TrueString;
            failureProvenance[BrowserRuntimeBlockerErrorCodeProvenanceKey] = errorCode;
        }

        return BuildStatusResult(
            failedJob,
            status,
            safeMessage,
            artifacts: [],
            extraWarnings,
            MergeDictionaries(request.Provenance, failureProvenance));
    }

    private static string BuildBrowserRuntimeBlockerSafeMessage(string errorCode)
        => $"{BrowserRuntimeBlockerSafeMessage} error_code={errorCode}.";

    private static string BuildBrowserScreenshotFailureSafeMessage(string errorCode)
        => $"{BrowserScreenshotFailureSafeMessage} error_code={errorCode}.";

    private static string ResolveFailureSafeMessage(
        string message,
        string errorCode,
        bool isBrowserRuntimeBlocker)
    {
        if (isBrowserRuntimeBlocker)
            return BuildBrowserRuntimeBlockerSafeMessage(errorCode);

        if (string.Equals(errorCode, LocalChromiumBrowserProcessAdapter.ScreenshotFailedCode, StringComparison.Ordinal))
            return BuildBrowserScreenshotFailureSafeMessage(errorCode);

        return string.IsNullOrWhiteSpace(message)
            ? "DesignConversion thin-client job failed."
            : message;
    }

    private static bool IsBrowserRuntimeBlockerErrorCode(string errorCode)
        => string.Equals(errorCode, LocalChromiumBrowserProcessAdapter.RuntimeUnavailableCode, StringComparison.Ordinal) ||
           string.Equals(errorCode, LocalChromiumBrowserProcessAdapter.RuntimeCrashedBeforeScreenshotCode, StringComparison.Ordinal) ||
           string.Equals(errorCode, "BROWSER_SCREENSHOT_TIMEOUT", StringComparison.Ordinal);

    private static bool IsKnownBrowserFailureErrorCode(string errorCode)
        => IsBrowserRuntimeBlockerErrorCode(errorCode) ||
           string.Equals(errorCode, LocalChromiumBrowserProcessAdapter.ScreenshotFailedCode, StringComparison.Ordinal);

    private static bool TryGetKnownBrowserFailureFromEvaluation(
        DesignConversionInternalEvaluationResult evaluation,
        out string errorCode)
    {
        foreach (var failure in evaluation.ViewportFailures)
        {
            if (IsKnownBrowserFailureErrorCode(failure.ErrorCode))
            {
                errorCode = failure.ErrorCode;
                return true;
            }
        }

        errorCode = string.Empty;
        return false;
    }

    private static DesignConversionThinClientStatusResult MarkLocalArtifactDbPersistence(
        DesignConversionThinClientStatusResult completedStatus,
        DesignConversionLocalArtifactDbRecord record)
    {
        var metadata = BuildLocalArtifactDbStatusMetadata(record);
        var highQualityNegativePairProven = IsExactTrue(
            metadata,
            "high_quality_negative_pair_proven");
        return completedStatus with
        {
            Job = completedStatus.Job with
            {
                Metadata = MergeDictionaries(completedStatus.Job.Metadata, metadata)
            },
            Warnings = completedStatus.Warnings
                .Concat([
                    "local_artifact_db_persistence_verified=true",
                    $"runtime_created_dpo_candidate_count={record.DpoCandidates.Count}",
                    record.DpoCandidates.Count > 0
                        ? "runtime_created_dpo_candidate=true"
                        : "runtime_created_dpo_candidate=false",
                    record.DpoCandidates.Count > 0
                        ? "runtime_created_dpo_candidate_review_pending=true"
                        : "runtime_created_dpo_candidate_review_pending=false",
                    record.DpoCandidates.Count > 0
                        ? "review_required=true"
                        : "review_required=false",
                    record.DpoCandidates.Count > 0
                        ? "approval_status=pending"
                        : "approval_status=not-required",
                    "approved_for_training=false",
                    record.DpoCandidates.Count > 0
                        ? "training_candidate_only=true"
                        : "training_candidate_only=false",
                    "production_dpo=false",
                    "product_not_ready=true",
                    "model_readiness_claimed=false",
                    highQualityNegativePairProven
                        ? "high_quality_negative_pair_proven=true"
                        : "high_quality_negative_pair_not_proven=true",
                    "held_out_not_proven=true",
                    "source_disjoint_not_proven=true",
                    "production_db_ready=false",
                    "trained_model_ready=false"
                ])
                .Concat(ReadRuntimeCreatedDpoCandidateWarnings(record))
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Provenance = MergeDictionaries(completedStatus.Provenance, metadata)
        };
    }

    private static DesignConversionThinClientStatusResult MarkOptionalLocalArtifactDbPersistenceFailed(
        DesignConversionThinClientStatusResult completedStatus,
        string errorCode)
    {
        var safeErrorCode = SafePersistenceErrorCode(errorCode);
        const string nonDurableStatus = DesignConversionThinClientStatus.NotReady;
        var metadata = new Dictionary<string, string>
        {
            ["status"] = nonDurableStatus,
            ["thin_client_terminal_status"] = nonDurableStatus,
            ["local_artifact_db_persistence_verified"] = bool.FalseString,
            ["local_artifact_db_persistence_failed"] = bool.TrueString,
            ["local_artifact_db_persistence_status"] = "non_durable_optional",
            ["local_artifact_db_persistence_error_code"] = safeErrorCode,
            ["local_artifact_db_autowrite"] = bool.FalseString,
            ["local_artifact_db_persistence_required"] = bool.FalseString,
            ["storage_training_artifact_db"] = bool.FalseString,
            ["local_artifact_db"] = bool.FalseString,
            ["production_db_ready"] = bool.FalseString,
            ["product_not_ready"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["training_candidate_only"] = bool.FalseString
        };

        return completedStatus with
        {
            ContractStatus = nonDurableStatus,
            Message = "In-process thin-client job is not ready because local artifact DB persistence was not durable.",
            Job = completedStatus.Job with
            {
                Status = nonDurableStatus,
                Metadata = MergeDictionaries(completedStatus.Job.Metadata, metadata)
            },
            Warnings = completedStatus.Warnings
                .Where(warning => !warning.StartsWith("thin_client_terminal_status=", StringComparison.Ordinal))
                .Concat([
                    "local_artifact_db_persistence_verified=false",
                    "local_artifact_db_persistence_failed=true",
                    "local_artifact_db_persistence_status=non_durable_optional",
                    $"local_artifact_db_persistence_error_code={safeErrorCode}",
                    "local_artifact_db_autowrite=false",
                    "local_artifact_db_persistence_required=false",
                    "production_db_ready=false",
                    "product_not_ready=true",
                    "training_candidate_only=false",
                    $"thin_client_terminal_status={nonDurableStatus}"
                ])
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Provenance = MergeDictionaries(completedStatus.Provenance, metadata)
        };
    }

    private static DesignConversionThinClientStatusResult MarkLocalArtifactDbPersistenceFailedClosed(
        DesignConversionThinClientStatusResult failedStatus,
        string errorCode)
    {
        var metadata = new Dictionary<string, string>
        {
            ["local_artifact_db_persistence_verified"] = bool.FalseString,
            ["local_artifact_db_persistence_failed"] = bool.TrueString,
            ["local_artifact_db_persistence_status"] = "failed_closed",
            ["local_artifact_db_persistence_error_code"] = errorCode,
            ["local_artifact_db_persistence_required"] = bool.TrueString,
            ["runtime_created_dpo_candidate"] = bool.TrueString,
            ["training_candidate_only"] = bool.TrueString,
            ["storage_training_artifact_db"] = bool.FalseString,
            ["local_artifact_db"] = bool.FalseString,
            ["production_db_ready"] = bool.FalseString,
            ["product_not_ready"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString
        };

        return failedStatus with
        {
            Job = failedStatus.Job with
            {
                Metadata = MergeDictionaries(failedStatus.Job.Metadata, metadata)
            },
            Warnings = failedStatus.Warnings
                .Concat([
                    "local_artifact_db_persistence_verified=false",
                    "local_artifact_db_persistence_failed=true",
                    "local_artifact_db_persistence_status=failed_closed",
                    $"local_artifact_db_persistence_error_code={errorCode}",
                    "local_artifact_db_persistence_required=true",
                    "runtime_created_dpo_candidate=true",
                    "training_candidate_only=true",
                    "production_db_ready=false",
                    "product_not_ready=true"
                ])
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Provenance = MergeDictionaries(failedStatus.Provenance, metadata)
        };
    }

    private static DesignConversionThinClientStatusResult MarkVisualCostLedgerPersistence(
        DesignConversionThinClientStatusResult completedStatus,
        DesignConversionVisualCostLedgerRecord record)
    {
        var metadata = BuildVisualCostLedgerStatusMetadata(record);
        return completedStatus with
        {
            Job = completedStatus.Job with
            {
                Metadata = MergeDictionaries(completedStatus.Job.Metadata, metadata)
            },
            Warnings = completedStatus.Warnings
                .Concat([
                    "visual_cost_ledger_persistence_verified=true",
                    "visual_cost_ledger_recorded=true",
                    "visual_cost_ledger_training_artifact_db=false",
                    "visual_cost_ledger_quota_enforcement=false"
                ])
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Provenance = MergeDictionaries(completedStatus.Provenance, metadata)
        };
    }

    private static DesignConversionThinClientStatusResult MarkVisualCostLedgerPersistenceFailed(
        DesignConversionThinClientStatusResult failedStatus)
    {
        var metadata = new Dictionary<string, string>
        {
            ["visual_cost_ledger_persistence_verified"] = bool.FalseString,
            ["visual_cost_ledger_persistence_failed"] = bool.TrueString,
            ["visual_cost_ledger_persistence_status"] = "failed_closed",
            ["visual_cost_ledger_training_artifact_db"] = bool.FalseString,
            ["visual_cost_ledger_quota_enforcement"] = bool.FalseString,
            ["completed_without_visual_cost_accounting"] = bool.FalseString
        };

        return failedStatus with
        {
            Job = failedStatus.Job with
            {
                Metadata = MergeDictionaries(failedStatus.Job.Metadata, metadata)
            },
            Warnings = failedStatus.Warnings
                .Concat([
                    "visual_cost_ledger_persistence_verified=false",
                    "visual_cost_ledger_persistence_failed=true",
                    "visual_cost_ledger_persistence_status=failed_closed",
                    "completed_without_visual_cost_accounting=false"
                ])
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Provenance = MergeDictionaries(failedStatus.Provenance, metadata)
        };
    }

    private static IReadOnlyDictionary<string, string> BuildLocalArtifactDbMetadata(
        DesignConversionInternalEvaluationResult evaluation,
        int runtimeCreatedDpoCandidateCount,
        IReadOnlyList<string> dpoCandidateWarnings,
        string postEvaluationStateRef,
        IReadOnlyDictionary<string, string> teacherCallMetadata)
    {
        var highQualityNegativePairProven = dpoCandidateWarnings.Contains(
            "high_quality_negative_pair_proven",
            StringComparer.Ordinal);
        var teacherAttestationVerified = dpoCandidateWarnings.Contains(
            "runtime_teacher_attestation_verified=true",
            StringComparer.Ordinal);
        var metadata = new Dictionary<string, string>
        {
            ["artifact_db_record_source"] = nameof(DesignConversionThinClientJobRuntime),
            ["artifact_db_single_writer_owner"] = nameof(DesignConversionThinClientJobRuntime),
            ["artifact_db_record_job_binding"] = "root_submit_job_final_evaluation_result",
            ["final_candidate_from_loop"] = IsSubmitPathLoopEvaluation(evaluation).ToString(),
            ["final_visual_score"] = evaluation.VisualQaBoundaryTrace.TierPolicy.AcceptanceScore
                .ToString("0.####", CultureInfo.InvariantCulture),
            ["attempt_count"] = ExtractWarningValue(
                evaluation.Warnings,
                "submit_path_improvement_loop_attempt_count=",
                "1"),
            ["final_loop_stop_reason"] = ExtractWarningValue(
                evaluation.Warnings,
                "submit_path_improvement_loop_stop_reason=",
                "not-run"),
            ["runtime_boundary"] = RuntimeBoundary,
            ["local_artifact_db_persistence_verified"] = bool.TrueString,
            ["storage_training_artifact_db"] = bool.TrueString,
            ["local_artifact_db"] = bool.TrueString,
            ["conversion_result"] = "refs_or_hashes_only",
            ["render_artifact_refs"] = "refs_or_hashes_only",
            ["visual_judge_status"] = evaluation.VisualQaBoundaryTrace.VisualJudgeStatus,
            ["visual_judge_provider_boundary"] = evaluation.VisualQaBoundaryTrace.VisualJudgeProviderBoundary,
            ["external_ai_visual_judge_executed"] = evaluation.VisualQaBoundaryTrace.ExternalAiVisualJudgeExecuted.ToString(),
            ["tier_policy_stop_reason"] = evaluation.VisualQaBoundaryTrace.TierPolicy.StopReason,
            ["training_candidate_only_boundary"] = runtimeCreatedDpoCandidateCount > 0
                ? "runtime_created_dpo_candidate_training_candidate_only_review_pending"
                : "runtime_created_dpo_candidate_not_created",
            ["runtime_created_dpo_candidate"] = (runtimeCreatedDpoCandidateCount > 0).ToString(),
            ["runtime_created_dpo_candidate_count"] = runtimeCreatedDpoCandidateCount.ToString(),
            ["runtime_created_dpo_candidate_warnings"] = string.Join("|", dpoCandidateWarnings ?? []),
            ["runtime_created_dpo_candidate_review_status"] = runtimeCreatedDpoCandidateCount > 0
                ? "runtime-candidate-review-pending"
                : "not-created",
            ["review_required"] = (runtimeCreatedDpoCandidateCount > 0).ToString(),
            ["approval_required"] = (runtimeCreatedDpoCandidateCount > 0).ToString(),
            ["approval_status"] = runtimeCreatedDpoCandidateCount > 0 ? "pending" : "not-required",
            ["approved_for_training"] = bool.FalseString,
            ["training_candidate_only"] = (runtimeCreatedDpoCandidateCount > 0).ToString(),
            ["production_dpo"] = bool.FalseString,
            ["high_quality_negative_pair_proven"] = highQualityNegativePairProven.ToString(),
            ["high_quality_negative_pair_not_proven"] = (!highQualityNegativePairProven).ToString(),
            ["runtime_teacher_attestation_verified"] = teacherAttestationVerified.ToString(),
            ["held_out_not_proven"] = bool.TrueString,
            ["source_disjoint_not_proven"] = bool.TrueString,
            ["product_not_ready"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["public_api_verified"] = bool.FalseString,
            ["production_db_ready"] = bool.FalseString,
            ["trained_model_ready"] = bool.FalseString,
            ["trained_feature_verified"] = bool.FalseString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_tree_payload_included"] = bool.FalseString,
            ["prompt_payload_included"] = bool.FalseString,
            ["external_provider_payload_included"] = bool.FalseString
        };

        metadata[JsonDesignConversionPostEvaluationStateStore.StateRefMetadataKey] =
            postEvaluationStateRef;
        metadata[JsonDesignConversionPostEvaluationStateStore.PersistenceStatusMetadataKey] =
            JsonDesignConversionPostEvaluationStateStore.PersistenceProvisional;
        metadata[JsonDesignConversionPostEvaluationStateStore.TrainingEligibleMetadataKey] =
            bool.FalseString;

        foreach (var pair in teacherCallMetadata)
            metadata[pair.Key] = pair.Value;

        AddRouteAdmissionBindingMetadata(metadata, evaluation.SubmittedRequest.Provenance);
        return metadata;
    }

    private static void AddRouteAdmissionBindingMetadata(
        IDictionary<string, string> metadata,
        IReadOnlyDictionary<string, string>? provenance)
    {
        if (!IsExactTrue(provenance, ServerOwnedRouteAdmissionTenantBoundMetadataKey))
            return;
        if (!TryReadCanonicalRouteAdmissionTenantIdRef(provenance, out var tenantIdRef))
            return;
        if (!TryReadCanonicalRouteAdmissionScopeRef(provenance, out var scopeRef))
            return;

        metadata[ServerOwnedRouteAdmissionTenantBoundMetadataKey] = bool.TrueString;
        metadata[ServerOwnedRouteAdmissionTenantIdRefMetadataKey] = tenantIdRef;
        metadata[ServerOwnedRouteAdmissionScopeRefMetadataKey] = scopeRef;
    }

    public static bool IsCanonicalRouteAdmissionTenantIdRef(string? value)
        => IsCanonicalScopedSha256Ref(value, RouteAdmissionTenantRefScope);

    public static bool IsCanonicalRouteAdmissionScopeRef(string? value)
        => IsCanonicalScopedSha256Ref(value, RouteAdmissionScopeRefScope);

    private static bool TryReadCanonicalRouteAdmissionTenantIdRef(
        IReadOnlyDictionary<string, string>? metadata,
        out string value)
        => TryReadCanonicalScopedSha256Ref(
            metadata,
            ServerOwnedRouteAdmissionTenantIdRefMetadataKey,
            IsCanonicalRouteAdmissionTenantIdRef,
            out value);

    private static bool IsExactTrue(
        IReadOnlyDictionary<string, string>? metadata,
        string key)
        => metadata is not null &&
           metadata.TryGetValue(key, out var value) &&
           string.Equals(value, bool.TrueString, StringComparison.Ordinal);

    private static bool TryReadCanonicalRouteAdmissionScopeRef(
        IReadOnlyDictionary<string, string>? metadata,
        out string value)
        => TryReadCanonicalScopedSha256Ref(
            metadata,
            ServerOwnedRouteAdmissionScopeRefMetadataKey,
            IsCanonicalRouteAdmissionScopeRef,
            out value);

    private static bool TryReadCanonicalScopedSha256Ref(
        IReadOnlyDictionary<string, string>? metadata,
        string key,
        Func<string?, bool> validator,
        out string value)
    {
        value = string.Empty;
        if (metadata is null ||
            !metadata.TryGetValue(key, out var candidate) ||
            !validator(candidate))
        {
            return false;
        }

        value = candidate;
        return true;
    }

    private static bool IsCanonicalScopedSha256Ref(string? value, string scope)
    {
        if (string.IsNullOrEmpty(value) ||
            !string.Equals(value, value.Trim(), StringComparison.Ordinal))
        {
            return false;
        }

        var prefix = $"sha256:{scope}:";
        if (!value.StartsWith(prefix, StringComparison.Ordinal) ||
            value.Length != prefix.Length + 64)
        {
            return false;
        }

        for (var index = prefix.Length; index < value.Length; index++)
        {
            var ch = value[index];
            if (ch is not (>= '0' and <= '9') &&
                ch is not (>= 'a' and <= 'f'))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsSubmitPathLoopEvaluation(DesignConversionInternalEvaluationResult evaluation)
        => evaluation.EvidenceRefs.Contains(
            DesignConversionImprovementLoopThinClientJobEvaluator.SubmitPathLoopExecutedMarker);

    private static string ExtractWarningValue(
        IReadOnlyList<string> warnings,
        string prefix,
        string fallback)
    {
        var value = warnings.FirstOrDefault(warning => warning.StartsWith(prefix, StringComparison.Ordinal));
        return string.IsNullOrWhiteSpace(value) ? fallback : value[prefix.Length..];
    }

    private static string SafePersistenceErrorCode(string? errorCode)
    {
        var value = string.IsNullOrWhiteSpace(errorCode)
            ? "LOCAL_ARTIFACT_DB_PERSISTENCE_FAILED"
            : errorCode;
        var safe = new string(value
            .Select(ch => char.IsLetterOrDigit(ch) || ch is '_' or '-' ? char.ToLowerInvariant(ch) : '_')
            .ToArray());
        return string.IsNullOrWhiteSpace(safe) ? "local_artifact_db_persistence_failed" : safe;
    }

    private static OperationResult<T> VisualCostRecordsReadFailed<T>()
        => OperationResult<T>.Fail(
            VisualCostRecordsReadFailedSafeMessage,
            VisualCostRecordsReadFailedErrorCode);

    private static OperationResult<T> VisualCostRecordsCancelled<T>()
        => OperationResult<T>.Fail(VisualCostRecordsCancelledSafeMessage, "CANCELLED");

    private static IReadOnlyDictionary<string, string> BuildLocalArtifactDbStatusMetadata(
        DesignConversionLocalArtifactDbRecord record)
    {
        var highQualityNegativePairProven = IsExactTrue(
            record.Metadata,
            "high_quality_negative_pair_proven") ||
            record.DpoCandidates.Count > 0;
        var metadata = new Dictionary<string, string>
        {
            ["local_artifact_db_persistence_verified"] = "true",
            ["local_artifact_db_record_id"] = record.RecordId,
            ["storage_training_artifact_db"] = "true",
            ["local_artifact_db"] = "true",
            ["conversion_result"] = "refs_or_hashes_only",
            ["render_artifact_refs"] = "refs_or_hashes_only",
            ["visual_judge_status"] = record.VisualJudgeStatus.Status,
            ["runtime_created_dpo_candidate"] = (record.DpoCandidates.Count > 0).ToString(),
            ["runtime_created_dpo_candidate_count"] = record.DpoCandidates.Count.ToString(),
            ["runtime_created_dpo_candidate_review_status"] = record.DpoCandidates.Count > 0
                ? "runtime-candidate-review-pending"
                : "not-created",
            ["review_required"] = (record.DpoCandidates.Count > 0).ToString(),
            ["approval_required"] = (record.DpoCandidates.Count > 0).ToString(),
            ["approval_status"] = record.DpoCandidates.Count > 0 ? "pending" : "not-required",
            ["approved_for_training"] = "false",
            ["training_candidate_only"] = record.DpoCandidates.Any(candidate => candidate.TrainingCandidateOnly).ToString(),
            ["production_dpo"] = "false",
            ["high_quality_negative_pair_proven"] = highQualityNegativePairProven.ToString(),
            ["high_quality_negative_pair_not_proven"] = (!highQualityNegativePairProven).ToString(),
            ["runtime_teacher_attestation_verified"] = IsExactTrue(
                record.Metadata,
                "runtime_teacher_attestation_verified").ToString(),
            ["held_out_not_proven"] = "true",
            ["source_disjoint_not_proven"] = "true",
            ["product_not_ready"] = "true",
            ["model_readiness_claimed"] = "false",
            ["public_api_verified"] = "false",
            ["production_db_ready"] = "false",
            ["trained_model_ready"] = "false"
        };

        if (record.Metadata.TryGetValue("runtime_created_dpo_candidate_warnings", out var warnings) &&
            !string.IsNullOrWhiteSpace(warnings))
        {
            metadata["runtime_created_dpo_candidate_warnings"] = warnings;
        }

        return metadata;
    }

    private static IReadOnlyList<string> ReadRuntimeCreatedDpoCandidateWarnings(
        DesignConversionLocalArtifactDbRecord record)
    {
        if (!record.Metadata.TryGetValue("runtime_created_dpo_candidate_warnings", out var warnings) ||
            string.IsNullOrWhiteSpace(warnings))
        {
            return [];
        }

        return warnings
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(IsRuntimeCreatedDpoCandidateStatusWarning)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static bool IsRuntimeCreatedDpoCandidateStatusWarning(string warning)
        => warning.StartsWith("runtime_created_dpo_candidate", StringComparison.Ordinal) ||
           warning.StartsWith("runtime_rejected_candidate_", StringComparison.Ordinal) ||
           warning.StartsWith("runtime_rejected_", StringComparison.Ordinal) ||
           warning.StartsWith("runtime_teacher_attestation_", StringComparison.Ordinal) ||
           warning.StartsWith("approved_for_training=", StringComparison.Ordinal) ||
           warning.StartsWith("synthetic_negative=", StringComparison.Ordinal) ||
           warning.StartsWith("scaffold_negative=", StringComparison.Ordinal) ||
           warning.StartsWith("training_grade=", StringComparison.Ordinal) ||
           warning.StartsWith("not_training_grade=", StringComparison.Ordinal) ||
           warning.StartsWith("production_dpo=", StringComparison.Ordinal) ||
           warning.StartsWith("model_readiness_claimed=", StringComparison.Ordinal) ||
           warning.StartsWith("training_candidate_only=", StringComparison.Ordinal) ||
           string.Equals(warning, "product_not_ready", StringComparison.Ordinal) ||
           string.Equals(warning, "chosen_rejected_pair_proven", StringComparison.Ordinal) ||
           string.Equals(warning, "high_quality_negative_pair_proven", StringComparison.Ordinal) ||
           string.Equals(warning, "held_out_not_proven", StringComparison.Ordinal) ||
           string.Equals(warning, "source_disjoint_not_proven", StringComparison.Ordinal);

    private static IReadOnlyDictionary<string, string> BuildVisualCostLedgerStatusMetadata(
        DesignConversionVisualCostLedgerRecord record)
        => new Dictionary<string, string>
        {
            ["visual_cost_ledger_persistence_verified"] = bool.TrueString,
            ["visual_cost_ledger_record_id"] = record.RecordId,
            ["visual_cost_ledger_kind"] = record.LedgerKind,
            ["visual_cost_ledger_schema_version"] = record.SchemaVersion,
            ["visual_cost_ledger_total_cost_usd"] = record.TotalCostUsd.ToString("G29", CultureInfo.InvariantCulture),
            ["visual_cost_ledger_measurement_complete"] = record.MeasurementComplete.ToString(CultureInfo.InvariantCulture),
            ["visual_cost_ledger_missing_measurement_count"] = record.MissingMeasurementCount.ToString(CultureInfo.InvariantCulture),
            ["visual_cost_ledger_evaluation_attempt_count"] = record.EvaluationAttemptCount.ToString(CultureInfo.InvariantCulture),
            ["visual_cost_ledger_cost_source"] = record.CostSource,
            ["visual_cost_ledger_user_ref"] = record.UserRef,
            ["visual_cost_ledger_user_ref_semantics"] = "pseudonymous_technical_scoped_sha256_not_anonymous_not_secret",
            ["visual_cost_ledger_submitted_tier"] = record.SubmittedTier,
            ["visual_cost_ledger_tier_authority"] = "submitted_context_only",
            ["visual_cost_ledger_period_kind"] = record.PeriodKind,
            ["visual_cost_ledger_period_start_utc"] = record.PeriodStartUtc.ToString("O", CultureInfo.InvariantCulture),
            ["visual_cost_ledger_period_end_exclusive_utc"] = record.PeriodEndExclusiveUtc.ToString("O", CultureInfo.InvariantCulture),
            ["visual_cost_ledger_training_artifact_db"] = bool.FalseString,
            ["visual_cost_ledger_quota_enforcement"] = bool.FalseString,
            ["visual_cost_ledger_monthly_cap_enforcement"] = bool.FalseString,
            ["visual_cost_ledger_scope"] = "record_and_aggregate_only"
        };

    private static DesignConversionThinClientStatusResult BuildStatusResult(
        DesignConversionThinClientJob job,
        string contractStatus,
        string message,
        IReadOnlyList<DesignConversionThinClientArtifactRef> artifacts,
        IReadOnlyList<string> extraWarnings,
        IReadOnlyDictionary<string, string>? provenance,
        DesignConversionThinClientStatusQualityMetrics? qualityMetrics = null,
        IReadOnlyList<DesignConversionThinClientStatusDefect>? defects = null)
        => new DesignConversionThinClientStatusResult(
            Job: job,
            ContractStatus: contractStatus,
            Message: message,
            Artifacts: artifacts,
            Warnings: BoundaryWarnings
                .Concat(extraWarnings)
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Provenance: MergeDictionaries(
                provenance,
                new Dictionary<string, string>
                {
                    ["submitted_request_contract"] = nameof(SubmittedDesignConversionRequest),
                    ["job_contract"] = nameof(DesignConversionThinClientJob),
                    ["status_contract"] = nameof(DesignConversionThinClientStatusResult),
                    ["runtime_adapter"] = nameof(DesignConversionThinClientJobRuntime),
                    ["runtime_boundary"] = RuntimeBoundary,
                    ["storage_boundary"] = StorageBoundary,
                    ["public_api_verified"] = "false",
                    ["flow47_verification_authority"] = Flow47VerificationAuthority,
                    ["flow47_verification_state"] = Flow47VerificationState,
                    ["persistence_verified"] = "false",
                    ["product_ready_verified"] = "false",
                    ["runtime_ready_verified"] = "false",
                    ["host_readiness_verified"] = "false"
                }))
        {
            QualityMetrics = qualityMetrics ??
                DesignConversionThinClientStatusQualityMetrics.Unavailable(
                    source: $"status_{contractStatus}",
                    unavailableReason: "quality_metrics_not_available_for_status"),
            Defects = defects ?? []
        };

    private DesignConversionThinClientJob UpdateJob(
        DesignConversionThinClientJob job,
        string status,
        string? errorCode)
        => job with
        {
            Status = status,
            UpdatedAtUtc = _utcNow(),
            Metadata = BuildJobMetadata(
                _requests.TryGetValue(job.JobId, out var request) ? request : null,
                status,
                errorCode)
        };

    private static IReadOnlyDictionary<string, string> BuildJobMetadata(
        SubmittedDesignConversionRequest? request,
        string status,
        string? errorCode)
    {
        var metadata = new Dictionary<string, string>
        {
            ["status"] = status,
            ["runtime_boundary"] = RuntimeBoundary,
            ["storage_boundary"] = StorageBoundary,
            ["public_api_verified"] = "false",
            ["flow47_verification_authority"] = Flow47VerificationAuthority,
            ["flow47_verification_state"] = Flow47VerificationState,
            ["platform_integration_verified"] = "false",
            ["persistence_verified"] = "false",
            ["product_ready_verified"] = "false",
            ["runtime_ready_verified"] = "false",
            ["trained_feature_verified"] = "false"
        };

        if (request is not null)
        {
            metadata["source_kind"] = request.SourceKind.ToString();
            metadata["budget_tier"] = request.Budget.Tier;
            foreach (var pair in BuildMaxTeacherCallsContractMetadata(request.Budget))
                metadata[pair.Key] = pair.Value;
        }

        if (!string.IsNullOrWhiteSpace(errorCode))
            metadata["error_code"] = errorCode;

        return metadata;
    }

    private static IReadOnlyDictionary<string, string> BuildStatusProvenance(
        SubmittedDesignConversionRequest request)
        => MergeDictionaries(
            MergeDictionaries(
                request.Provenance,
                BuildMaxTeacherCallsContractMetadata(request.Budget)),
            BuildRepositoryOwnedDpoTransactionMetadata(
                request.RepositoryOwnedDpoTransactionContext));

    private static IReadOnlyDictionary<string, string>
        BuildRepositoryOwnedDpoTransactionMetadata(
            DesignConversionRepositoryOwnedDpoTransactionContext? context)
        // Status provenance names disabled legacy mode explicitly so downstream
        // callers can distinguish "no repository-owned route requested" from a
        // valid or invalid enabled transaction context.
        => (context ?? DesignConversionRepositoryOwnedDpoTransactionContext
                .Disabled)
            .ToBoundaryMetadata();

    private static IReadOnlyDictionary<string, string> BuildMaxTeacherCallsStatusMetadata(
        DesignConversionBudgetMetadata budget,
        DesignConversionInternalEvaluationResult evaluation)
    {
        var metadata = new Dictionary<string, string>(
            BuildMaxTeacherCallsContractMetadata(budget),
            StringComparer.Ordinal);
        foreach (var key in MaxTeacherCallsMetadataKeys())
        {
            if (evaluation.SubmittedRequest.Provenance.TryGetValue(key, out var value))
                metadata[key] = value;
        }

        return metadata;
    }

    private static CarriedTeacherCallState ResolveCarriedTeacherCallState(
        DesignConversionBudgetMetadata budget,
        DesignConversionInternalEvaluationResult evaluation)
    {
        var claimsLoopState = IsSubmitPathLoopEvaluation(evaluation);
        var hasReceipt = DesignConversionTeacherCallUsageReceiptAssociation.TryResolve(
            evaluation,
            out var receipt) &&
            receipt is not null;
        if (!claimsLoopState)
            return hasReceipt
                ? new CarriedTeacherCallState(false, 0)
                : new CarriedTeacherCallState(true, 0);
        if (!hasReceipt)
            return new CarriedTeacherCallState(false, 0);

        var valid =
            receipt!.IsValid &&
            receipt.ConfiguredMaximumCalls == budget.MaxTeacherCalls &&
            string.Equals(receipt.RootJobId, evaluation.JobId, StringComparison.Ordinal) &&
            string.Equals(
                receipt.RootRequestId,
                evaluation.SubmittedRequest.RequestId,
                StringComparison.Ordinal) &&
            receipt.ObservedCallCount >= 0 &&
            (!budget.MaxTeacherCalls.HasValue ||
             receipt.ObservedCallCount <= budget.MaxTeacherCalls.Value);
        return new CarriedTeacherCallState(
            valid,
            valid ? receipt.ObservedCallCount : 0);
    }

    private static IReadOnlyDictionary<string, string>
        BuildRuntimeTeacherCallBudgetMetadata(
            DesignConversionBudgetMetadata budget,
            int observedCallCount)
    {
        var maxTeacherCalls = budget.MaxTeacherCalls;
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractSuppliedMetadataKey] =
                maxTeacherCalls.HasValue ? bool.TrueString : bool.FalseString,
            [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractValueMetadataKey] =
                maxTeacherCalls.HasValue
                    ? maxTeacherCalls.Value.ToString(CultureInfo.InvariantCulture)
                    : "not_supplied",
            [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractBoundaryMetadataKey] =
                DesignConversionVisualQaBoundaryPolicy.MaxTeacherCallsContractBoundary,
            [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsRuntimeEnforcementMetadataKey] =
                maxTeacherCalls.HasValue ? bool.TrueString : bool.FalseString,
            [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCallCapEnforcedMetadataKey] =
                maxTeacherCalls.HasValue ? bool.TrueString : bool.FalseString,
            [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsObservedCallCountMetadataKey] =
                observedCallCount.ToString(CultureInfo.InvariantCulture),
            [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCapReachedMetadataKey] =
                (maxTeacherCalls.HasValue && observedCallCount >= maxTeacherCalls.Value)
                    ? bool.TrueString
                    : bool.FalseString
        };
    }

    private static SubmitPathTeacherCallCapReconciliation
        ReconcileSubmitPathTeacherCallCapAfterAttestation(
            DesignConversionThinClientStatusResult completedStatus,
            DesignConversionInternalEvaluationResult evaluation,
            DesignConversionBudgetMetadata budget,
            bool teacherCallIssued,
            int observedCallCount,
            IReadOnlyDictionary<string, string> teacherCallMetadata)
    {
        if (!teacherCallIssued)
        {
            return new SubmitPathTeacherCallCapReconciliation(
                completedStatus,
                evaluation);
        }

        if (!IsSubmitPathLoopEvaluation(evaluation))
        {
            return new SubmitPathTeacherCallCapReconciliation(
                completedStatus,
                evaluation with
                {
                    EvidenceRefs = AppendIssuedTeacherCallEvidenceRef(
                        evaluation.EvidenceRefs,
                        observedCallCount)
                });
        }

        var capReached = budget.MaxTeacherCalls.HasValue &&
                         observedCallCount >= budget.MaxTeacherCalls.Value;
        var previousStopReason = ExtractWarningValue(
            evaluation.Warnings,
            "submit_path_improvement_loop_stop_reason=",
            string.Empty);
        var reconcileStopReason = capReached &&
                                  string.Equals(
                                      previousStopReason,
                                      DesignConversionImprovementLoopCoordinator.NoProgressStop,
                                      StringComparison.Ordinal);
        var reconciledStopReason = reconcileStopReason
            ? DesignConversionImprovementLoopCoordinator.MaxTeacherCallsReached
            : null;
        var reconciledStatus = completedStatus with
        {
            Warnings = ReconcileSubmitPathTeacherCallCapWarnings(
                completedStatus.Warnings,
                observedCallCount,
                capReached,
                teacherCallMetadata,
                reconciledStopReason)
        };
        var reconciledEvaluation = evaluation with
        {
            EvidenceRefs = ReconcileSubmitPathTeacherCallCapEvidenceRefs(
                evaluation.EvidenceRefs,
                observedCallCount,
                capReached,
                reconciledStopReason),
            Warnings = ReconcileSubmitPathTeacherCallCapWarnings(
                evaluation.Warnings,
                observedCallCount,
                capReached,
                teacherCallMetadata,
                reconciledStopReason)
        };
        return new SubmitPathTeacherCallCapReconciliation(
            reconciledStatus,
            reconciledEvaluation);
    }

    private static IReadOnlyList<string> ReconcileSubmitPathTeacherCallCapWarnings(
        IReadOnlyList<string> warnings,
        int observedCallCount,
        bool capReached,
        IReadOnlyDictionary<string, string> teacherCallMetadata,
        string? reconciledStopReason)
    {
        var observedCallCountText = observedCallCount.ToString(CultureInfo.InvariantCulture);
        var capReachedText = capReached ? bool.TrueString : bool.FalseString;
        var values = warnings
            .Where(warning => !IsSubmitPathTeacherCallCapWarningToReplace(
                warning,
                reconciledStopReason is not null))
            .Concat([
                $"{DesignConversionImprovementLoopCoordinator.MaxTeacherCallsRuntimeEnforcementMetadataKey}={ReadTeacherCallMetadata(teacherCallMetadata, DesignConversionImprovementLoopCoordinator.MaxTeacherCallsRuntimeEnforcementMetadataKey, bool.FalseString)}",
                $"{DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCallCapEnforcedMetadataKey}={ReadTeacherCallMetadata(teacherCallMetadata, DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCallCapEnforcedMetadataKey, bool.FalseString)}",
                $"{DesignConversionImprovementLoopCoordinator.MaxTeacherCallsObservedCallCountMetadataKey}={observedCallCountText}",
                $"{DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCapReachedMetadataKey}={capReachedText}",
                DesignConversionImprovementLoopCoordinator.ObservedTeacherCallEvidenceRefPrefix + observedCallCountText,
                $"improvement_loop:{DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCapReachedMetadataKey}={capReachedText}",
                capReached
                    ? "improvement_loop:max_teacher_calls_reached"
                    : "improvement_loop:max_teacher_calls_reached_false"
            ]);
        if (reconciledStopReason is not null)
        {
            values = values.Concat([
                $"submit_path_improvement_loop_stop_reason={reconciledStopReason}",
                "IMPROVEMENT_LOOP_MAX_TEACHER_CALLS_REACHED"
            ]);
        }

        return values
            .Distinct(StringComparer.Ordinal)
            .OrderBy(warning => warning, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<string> AppendIssuedTeacherCallEvidenceRef(
        IReadOnlyList<string> evidenceRefs,
        int observedCallCount)
        => evidenceRefs
            .Where(reference => !string.Equals(
                reference,
                DesignConversionImprovementLoopCoordinator.NoExternalProviderMarker,
                StringComparison.Ordinal))
            .Append(
                DesignConversionImprovementLoopCoordinator.ObservedTeacherCallEvidenceRefPrefix +
                observedCallCount.ToString(CultureInfo.InvariantCulture))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(reference => reference, StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyList<string> ReconcileSubmitPathTeacherCallCapEvidenceRefs(
        IReadOnlyList<string> evidenceRefs,
        int observedCallCount,
        bool capReached,
        string? reconciledStopReason)
    {
        var observedCallCountText = observedCallCount.ToString(CultureInfo.InvariantCulture);
        var capReachedText = capReached ? bool.TrueString : bool.FalseString;
        var values = evidenceRefs
            .Where(reference => !IsSubmitPathTeacherCallCapEvidenceRefToReplace(
                reference,
                reconciledStopReason is not null))
            .Concat([
                DesignConversionImprovementLoopCoordinator.ObservedTeacherCallEvidenceRefPrefix + observedCallCountText,
                $"improvement_loop:{DesignConversionImprovementLoopCoordinator.MaxTeacherCallsObservedCallCountMetadataKey}={observedCallCountText}",
                $"improvement_loop:{DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCapReachedMetadataKey}={capReachedText}",
                capReached
                    ? "improvement_loop:max_teacher_calls_reached"
                    : "improvement_loop:max_teacher_calls_reached_false"
            ]);
        if (reconciledStopReason is not null)
        {
            values = values.Concat([
                $"improvement_loop:stop_reason={reconciledStopReason}",
                "improvement_loop:no_progress_stop_false"
            ]);
        }

        return values
            .Distinct(StringComparer.Ordinal)
            .OrderBy(reference => reference, StringComparer.Ordinal)
            .ToArray();
    }

    private static bool IsSubmitPathTeacherCallCapWarningToReplace(
        string warning,
        bool replaceStopReason)
        => warning.StartsWith(
               $"{DesignConversionImprovementLoopCoordinator.MaxTeacherCallsRuntimeEnforcementMetadataKey}=",
               StringComparison.Ordinal) ||
           warning.StartsWith(
               $"{DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCallCapEnforcedMetadataKey}=",
               StringComparison.Ordinal) ||
           warning.StartsWith(
               $"{DesignConversionImprovementLoopCoordinator.MaxTeacherCallsObservedCallCountMetadataKey}=",
               StringComparison.Ordinal) ||
           warning.StartsWith(
               $"{DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCapReachedMetadataKey}=",
               StringComparison.Ordinal) ||
           warning.StartsWith(
               DesignConversionImprovementLoopCoordinator.ObservedTeacherCallEvidenceRefPrefix,
               StringComparison.Ordinal) ||
           warning.StartsWith(
               $"improvement_loop:{DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCapReachedMetadataKey}=",
               StringComparison.Ordinal) ||
           string.Equals(warning, "improvement_loop:max_teacher_calls_reached", StringComparison.Ordinal) ||
           string.Equals(warning, "improvement_loop:max_teacher_calls_reached_false", StringComparison.Ordinal) ||
           (replaceStopReason &&
            warning.StartsWith("submit_path_improvement_loop_stop_reason=", StringComparison.Ordinal));

    private static bool IsSubmitPathTeacherCallCapEvidenceRefToReplace(
        string evidenceRef,
        bool replaceStopReason)
        => string.Equals(
               evidenceRef,
               DesignConversionImprovementLoopCoordinator.NoExternalProviderMarker,
               StringComparison.Ordinal) ||
           evidenceRef.StartsWith(
               DesignConversionImprovementLoopCoordinator.ObservedTeacherCallEvidenceRefPrefix,
               StringComparison.Ordinal) ||
           evidenceRef.StartsWith(
               $"improvement_loop:{DesignConversionImprovementLoopCoordinator.MaxTeacherCallsObservedCallCountMetadataKey}=",
               StringComparison.Ordinal) ||
           evidenceRef.StartsWith(
               $"improvement_loop:{DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCapReachedMetadataKey}=",
               StringComparison.Ordinal) ||
           string.Equals(evidenceRef, "improvement_loop:max_teacher_calls_reached", StringComparison.Ordinal) ||
           string.Equals(evidenceRef, "improvement_loop:max_teacher_calls_reached_false", StringComparison.Ordinal) ||
           (replaceStopReason &&
            (evidenceRef.StartsWith("improvement_loop:stop_reason=", StringComparison.Ordinal) ||
             string.Equals(evidenceRef, "improvement_loop:no_progress_stop", StringComparison.Ordinal) ||
             string.Equals(evidenceRef, "improvement_loop:no_progress_stop_false", StringComparison.Ordinal)));

    private static string ReadTeacherCallMetadata(
        IReadOnlyDictionary<string, string> metadata,
        string key,
        string fallback)
        => metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;

    private sealed record SubmitPathTeacherCallCapReconciliation(
        DesignConversionThinClientStatusResult CompletedStatus,
        DesignConversionInternalEvaluationResult Evaluation);

    private static DesignConversionThinClientStatusResult
        ApplyRuntimeTeacherCallBudgetMetadata(
            DesignConversionThinClientStatusResult status,
            DesignConversionBudgetMetadata budget,
            int observedCallCount)
    {
        var metadata = BuildRuntimeTeacherCallBudgetMetadata(
            budget,
            observedCallCount);
        return status with
        {
            Job = status.Job with
            {
                Metadata = MergeDictionaries(status.Job.Metadata, metadata)
            },
            Provenance = MergeDictionaries(status.Provenance, metadata)
        };
    }

    private static IEnumerable<string> MaxTeacherCallsMetadataKeys()
    {
        yield return DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractSuppliedMetadataKey;
        yield return DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractValueMetadataKey;
        yield return DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractBoundaryMetadataKey;
        yield return DesignConversionImprovementLoopCoordinator.MaxTeacherCallsRuntimeEnforcementMetadataKey;
        yield return DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCallCapEnforcedMetadataKey;
        yield return DesignConversionImprovementLoopCoordinator.MaxTeacherCallsObservedCallCountMetadataKey;
        yield return DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCapReachedMetadataKey;
    }

    private static IReadOnlyDictionary<string, string> BuildMaxTeacherCallsContractMetadata(
        DesignConversionBudgetMetadata budget)
    {
        var maxTeacherCalls = budget.MaxTeacherCalls;
        return new Dictionary<string, string>
        {
            [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractSuppliedMetadataKey] = maxTeacherCalls.HasValue
                ? bool.TrueString
                : bool.FalseString,
            [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractValueMetadataKey] = maxTeacherCalls.HasValue
                ? maxTeacherCalls.Value.ToString(CultureInfo.InvariantCulture)
                : "not_supplied",
            [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractBoundaryMetadataKey] =
                DesignConversionVisualQaBoundaryPolicy.MaxTeacherCallsContractBoundary,
            [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsRuntimeEnforcementMetadataKey] = bool.FalseString,
            [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCallCapEnforcedMetadataKey] = bool.FalseString,
            [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsObservedCallCountMetadataKey] = "0",
            [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCapReachedMetadataKey] = bool.FalseString
        };
    }

    private sealed record CarriedTeacherCallState(
        bool IsValid,
        int PersistedObservedCallCount);

    private static DesignConversionThinClientArtifactRef BuildArtifact(
        DesignConversionThinClientJob job,
        string kind,
        string mediaType,
        string content)
    {
        var sha = Sha256Text(content ?? string.Empty);
        return new DesignConversionThinClientArtifactRef(
            ArtifactId: $"{job.JobId}:{kind}",
            Kind: kind,
            Uri: $"memory://design-conversion/jobs/{Uri.EscapeDataString(job.JobId)}/{kind}",
            Sha256: $"sha256:{sha}",
            MediaType: mediaType,
            Metadata: new Dictionary<string, string>
            {
                ["request_id"] = job.RequestId,
                ["job_id"] = job.JobId,
                ["storage_boundary"] = StorageBoundary
            });
    }

    private static string BuildJobId(string requestId)
    {
        var sanitized = new string(requestId
            .Select(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_' ? ch : '-')
            .ToArray());
        return $"thin-job-{sanitized}";
    }

    private static string BuildStatusUri(string jobId)
        => $"memory://design-conversion/jobs/{Uri.EscapeDataString(jobId)}/status";

    private static IReadOnlyDictionary<string, string> MergeDictionaries(
        IReadOnlyDictionary<string, string>? first,
        IReadOnlyDictionary<string, string> second)
        => (first ?? new Dictionary<string, string>())
            .Concat(second)
            .GroupBy(pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal);

    private static string Sha256Text(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private sealed record VisualCostBudgetReservationContext(
        DesignConversionVisualCostReservationRequest Request,
        DesignConversionVisualCostReservationResult? Reservation);

    private sealed record VisualCostBudgetSettlementContext(
        DesignConversionVisualCostSettlementResult Settlement,
        decimal ChargedCostUsd,
        bool ActualCostKnown,
        bool FailJob,
        string FailureCode,
        string FailureMessage);

    private sealed record VisualCostBudgetSettlementPlan(
        decimal ChargedCostUsd,
        bool ActualCostKnown,
        bool FailJob,
        string FailureCode,
        string FailureMessage);

    private sealed record VisualCostBudgetReleaseContext(
        DesignConversionVisualCostReleaseResult Release);

    private enum JobRunLeasePhase
    {
        Evaluating,
        PostEvaluationOwned,
        Revoked,
        Finished
    }

    private sealed class JobRunLease(string attemptId)
    {
        public string AttemptId { get; } = attemptId;

        public JobRunLeasePhase Phase { get; set; } = JobRunLeasePhase.Evaluating;

        public VisualCostBudgetReservationContext? Reservation { get; set; }

        public bool ReservationFinalized { get; set; }

        public bool ReleaseClaimed { get; set; }

        public TaskCompletionSource<bool> ReleaseCompletion { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private sealed record ReservationReleaseWork(
        string JobId,
        string AttemptId,
        VisualCostBudgetReservationContext Reservation,
        TaskCompletionSource<bool> Completion,
        string ReleaseEnforcement = "release_after_stale_terminal_cleanup_before_settlement");
}
