// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;

public sealed record DesignConversionImprovementLoopRequest(
    string LoopId,
    DesignConversionInternalEvaluateRequest InitialEvaluationRequest,
    int MaxAttempts,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignConversionCandidateUpdateRequest(
    string LoopId,
    int CompletedAttemptNumber,
    DesignConversionInternalEvaluateRequest CurrentRequest,
    DesignConversionInternalEvaluationResult CurrentEvaluation,
    DesignConversionTierPolicyDecision TierPolicy,
    IReadOnlyList<DesignConversionImprovementLoopAttemptTelemetry> AttemptHistory);

public interface IDesignConversionImprovementCandidateStrategy
{
    Task<OperationResult<DesignConversionImprovementCandidate>> CreateNextCandidateAsync(
        DesignConversionCandidateUpdateRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record DesignConversionImprovementLoopAttemptTelemetry(
    int AttemptNumber,
    string CandidateRef,
    string JobRef,
    string RequestRef,
    string StopReason,
    bool ShouldContinue,
    double BestScore,
    double RequiredScore,
    decimal EstimatedCostUsd,
    decimal BudgetUsd,
    int PolicyAttemptCount,
    int MaxImprovementCandidates,
    IReadOnlyList<string> RenderResultRefs,
    IReadOnlyList<string> ComparisonArtifactRefs,
    IReadOnlyList<string> VisualJudgeResultRefs,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyDictionary<string, string> PublicMetadata)
{
    public bool VisualCostMeasurementComplete { get; init; } = false;
    public int VisualCostMissingMeasurementCount { get; init; } = 1;
    public string VisualCostSource { get; init; } = DesignConversionVisualCostAccounting.UnknownCostSource;
}

public sealed record DesignConversionImprovementLoopResult(
    string LoopId,
    string StopReason,
    bool Completed,
    bool QualityReached,
    bool ProviderRequiredOrBlocked,
    bool RenderManifestIncomplete,
    bool BudgetCapReached,
    bool MaxAttemptsReached,
    bool Cancelled,
    bool DeadlineExceeded,
    bool CandidateStrategyFailed,
    int AttemptCount,
    DesignConversionInternalEvaluationResult? FinalEvaluation,
    IReadOnlyList<DesignConversionImprovementLoopAttemptTelemetry> Attempts,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyDictionary<string, string> PublicMetadata,
    IReadOnlyList<string> Warnings);

public sealed class DesignConversionImprovementLoopCoordinator
{
    public const string CoordinatorMarker = "design_conversion_improvement_loop_coordinator";
    public const string RefsOnlyTelemetryMarker = "improvement_loop_refs_only_public_telemetry";
    public const string NoExternalProviderMarker = "improvement_loop_no_external_provider_invocation";
    public const string LocalArtifactDbAutowriteMarker = "improvement_loop_local_artifact_db_autowrite";
    public const string LocalTrainingStoreDpoSourceMarker = "improvement_loop_local_training_store_dpo_source";
    public const string LocalArtifactDbPersistenceSkippedDeadlineExceededMarker = "improvement_loop_local_artifact_db_persistence_skipped_deadline_exceeded";
    public const string LocalArtifactDbPersistenceFailedMarker = "improvement_loop_local_artifact_db_persistence_failed";
    public const string LocalArtifactDbPersistenceFailureBoundedMarker = "improvement_loop_local_artifact_db_persistence_failure_bounded";
    public const string MaxAttemptsReached = "max_attempts_reached";
    public const string CandidateStrategyFailed = "candidate_strategy_failed";
    public const string EvaluationFailed = "evaluation_failed_bounded";
    public const string NoProgressStop = "no_progress_stop";
    public const string Cancelled = "cancelled";
    public const string DeadlineExceeded = "deadline_exceeded";
    public const string DeadlineExceededMarker = "improvement_loop_deadline_exceeded";
    public const string CandidateRepeatedHistoryMarker = "IMPROVEMENT_CANDIDATE_REPEATED_HISTORY";
    public const string DeadlineExceededMetadataKey = "deadline_exceeded";
    public const string ConfiguredRunDeadlineMsMetadataKey = "configured_run_deadline_ms";
    public const string ScoreDirectionHigherIsBetter = "higher_is_better";
    public const int MaxAllowedAttempts = 50;
    public const int DefaultRunDeadlineSeconds = 120;
    public const int MinRunDeadlineMilliseconds = 1;
    public const int MaxRunDeadlineSeconds = 600;

    private const double DefaultQualityImprovementEpsilon = 0.0001;
    private const string MaxAttemptsExceededErrorCode = "IMPROVEMENT_LOOP_MAX_ATTEMPTS_EXCEEDED";
    private const string CandidateNoOpErrorCode = "IMPROVEMENT_CANDIDATE_NO_OP";
    private const string CandidateRepeatedHistoryEvidenceRef = "improvement_loop:candidate_repeated_history";

    private readonly IDesignConversionThinClientJobEvaluator _evaluationService;
    private readonly IDesignConversionImprovementCandidateStrategy _candidateStrategy;
    private readonly IDesignConversionLocalArtifactDb? _artifactDb;
    private readonly Func<DateTimeOffset> _utcNow;
    private readonly TimeSpan _runDeadline;

    private sealed record CandidateAcceptanceDecision(
        string Decision,
        bool AcceptedAsIncumbent,
        string RejectionReason,
        double IncumbentScoreBefore,
        double CandidateMeasuredScore,
        double MeasuredImprovement,
        double ImprovementEpsilon,
        bool QualityNonRegression,
        string QualityNonRegressionEvidenceMode,
        bool ViewportNonRegression,
        bool RequiredViewportCoverageComplete,
        int ConsecutiveNoImprovement,
        int MaxConsecutiveNoImprovement,
        decimal SpentBudgetUsd,
        string BestCandidateId);

    private sealed record QualityNonRegressionEvidence(
        bool NonRegression,
        string EvidenceMode,
        string RejectionReason,
        bool ViewportNonRegression);

    public DesignConversionImprovementLoopCoordinator(
        DesignConversionInternalService evaluationService,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        IDesignConversionLocalArtifactDb? artifactDb = null,
        Func<DateTimeOffset>? utcNow = null,
        TimeSpan? runDeadline = null)
        : this(
            new DesignConversionInternalServiceThinClientJobEvaluator(evaluationService),
            candidateStrategy,
            artifactDb,
            utcNow,
            runDeadline)
    {
    }

    public DesignConversionImprovementLoopCoordinator(
        IDesignConversionThinClientJobEvaluator evaluationService,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        IDesignConversionLocalArtifactDb? artifactDb = null,
        Func<DateTimeOffset>? utcNow = null,
        TimeSpan? runDeadline = null)
    {
        _evaluationService = evaluationService ?? throw new ArgumentNullException(nameof(evaluationService));
        _candidateStrategy = candidateStrategy ?? throw new ArgumentNullException(nameof(candidateStrategy));
        _artifactDb = artifactDb;
        _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
        _runDeadline = NormalizeRunDeadline(runDeadline);
    }

    public async Task<OperationResult<DesignConversionImprovementLoopResult>> RunAsync(
        DesignConversionImprovementLoopRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = Validate(request);
        if (!validation.IsSuccess)
            return validation.MapError<DesignConversionImprovementLoopResult>();

        var attempts = ImmutableList<DesignConversionImprovementLoopAttemptTelemetry>.Empty;
        var warnings = new List<string>();
        var evidenceRefs = new List<string>
        {
            CoordinatorMarker,
            RefsOnlyTelemetryMarker,
            NoExternalProviderMarker,
            DesignConversionVisualQaBoundaryPolicy.ImprovementLoopMarker,
            DesignConversionVisualQaBoundaryPolicy.ProductNotReadyMarker
        };
        var currentRequest = request.InitialEvaluationRequest;
        var rootJobId = currentRequest.JobId;
        var currentCandidateId = "initial";
        var bestCandidateId = currentCandidateId;
        var qualityImprovementEpsilon = ResolveQualityImprovementEpsilon(request);
        var maxConsecutiveNoImprovement = ResolveMaxConsecutiveNoImprovement(request);
        var consecutiveNoImprovement = 0;
        var spentBudgetUsd = 0m;
        var coverageRetryCount = 0;
        var evaluatedMarkupFingerprints = new HashSet<string>(StringComparer.Ordinal);
        DesignConversionInternalEvaluateRequest? bestRequest = null;
        DesignConversionInternalEvaluationResult? bestEvaluation = null;
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(_runDeadline);
        var runToken = deadline.Token;

        for (var attemptNumber = 1; attemptNumber <= request.MaxAttempts; attemptNumber++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return await CompleteAsync(
                    request,
                    Cancelled,
                    bestEvaluation,
                    attempts,
                    evidenceRefs,
                    warnings,
                    cancellationToken);
            }

            if (IsInternalDeadlineExceeded(deadline, cancellationToken))
            {
                return await CompleteDeadlineExceededAsync(
                    request,
                    bestEvaluation,
                    attempts,
                    evidenceRefs,
                    warnings,
                    "IMPROVEMENT_LOOP_DEADLINE_EXCEEDED_BEFORE_ATTEMPT",
                    cancellationToken);
            }

            var isCoverageRetryAttempt = coverageRetryCount > 0;
            OperationResult<DesignConversionInternalEvaluationResult> evaluation;
            try
            {
                evaluation = await _evaluationService.EvaluateAsync(currentRequest, runToken);
            }
            catch (OperationCanceledException) when (IsInternalDeadlineExceeded(deadline, cancellationToken))
            {
                return await CompleteDeadlineExceededAsync(
                    request,
                    bestEvaluation,
                    attempts,
                    evidenceRefs,
                    warnings,
                    "IMPROVEMENT_LOOP_DEADLINE_EXCEEDED_DURING_EVALUATION",
                    cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                warnings.Add("IMPROVEMENT_LOOP_CANCELLED_DURING_EVALUATION");
                return await CompleteAsync(
                    request,
                    Cancelled,
                    bestEvaluation,
                    attempts,
                    evidenceRefs,
                    warnings,
                    cancellationToken);
            }

            if (!evaluation.IsSuccess)
            {
                if (string.Equals(evaluation.ErrorCode, "CANCELLED", StringComparison.OrdinalIgnoreCase))
                {
                    if (IsInternalDeadlineExceeded(deadline, cancellationToken))
                    {
                        return await CompleteDeadlineExceededAsync(
                            request,
                            bestEvaluation,
                            attempts,
                            evidenceRefs,
                            warnings,
                            "IMPROVEMENT_LOOP_DEADLINE_EXCEEDED_DURING_EVALUATION",
                            cancellationToken);
                    }

                    warnings.Add("IMPROVEMENT_LOOP_CANCELLED_DURING_EVALUATION");
                    return await CompleteAsync(
                        request,
                        Cancelled,
                        bestEvaluation,
                        attempts,
                        evidenceRefs,
                        warnings,
                        cancellationToken);
                }

                warnings.Add($"IMPROVEMENT_LOOP_EVALUATION_FAILED:{evaluation.ErrorCode ?? "UNKNOWN"}");
                var failedAttempt = BuildFailedAttemptTelemetry(
                    attemptNumber,
                    currentCandidateId,
                    currentRequest,
                    bestEvaluation,
                    evaluation.ErrorCode ?? "UNKNOWN",
                    spentBudgetUsd,
                    consecutiveNoImprovement,
                    maxConsecutiveNoImprovement,
                    qualityImprovementEpsilon);
                attempts = attempts.Add(failedAttempt);
                evidenceRefs.AddRange(failedAttempt.EvidenceRefs);
                return await CompleteAsync(
                    request,
                    EvaluationFailed,
                    bestEvaluation,
                    attempts,
                    evidenceRefs,
                    warnings,
                    cancellationToken);
            }

            var candidateEvaluation = evaluation.Value!;
            evaluatedMarkupFingerprints.Add(BuildMarkupFingerprint(currentRequest.Html, currentRequest.Css));
            var incumbentScoreBefore = bestEvaluation is null
                ? GetMeasuredQualityScore(candidateEvaluation)
                : GetMeasuredQualityScore(bestEvaluation);
            var acceptance = AssessCandidate(
                attemptNumber,
                currentCandidateId,
                candidateEvaluation,
                bestEvaluation,
                consecutiveNoImprovement,
                maxConsecutiveNoImprovement,
                qualityImprovementEpsilon);

            if (acceptance.AcceptedAsIncumbent)
            {
                bestEvaluation = candidateEvaluation;
                bestRequest = currentRequest;
                bestCandidateId = currentCandidateId;
                consecutiveNoImprovement = 0;
            }
            else
            {
                consecutiveNoImprovement++;
                warnings.Add($"IMPROVEMENT_LOOP_CANDIDATE_REJECTED:{acceptance.RejectionReason}");
            }

            spentBudgetUsd += candidateEvaluation.VisualQaBoundaryTrace.TierPolicy.EstimatedCostUsd;
            var policy = candidateEvaluation.VisualQaBoundaryTrace.TierPolicy;
            var coverageRetryStopSource = IsRetrySameCandidateCoveragePolicy(policy)
                ? ResolveCoverageRetryStopSource(attemptNumber, request, spentBudgetUsd, policy)
                : "none";
            var attempt = BuildAttemptTelemetry(
                attemptNumber,
                currentCandidateId,
                currentRequest,
                candidateEvaluation,
                acceptance with
                {
                    IncumbentScoreBefore = incumbentScoreBefore,
                    ConsecutiveNoImprovement = consecutiveNoImprovement,
                    SpentBudgetUsd = spentBudgetUsd,
                    BestCandidateId = bestCandidateId
                },
                isCoverageRetryAttempt,
                coverageRetryCount,
                coverageRetryStopSource);
            attempts = attempts.Add(attempt);
            evidenceRefs.AddRange(attempt.EvidenceRefs);

            if (IsInternalDeadlineExceeded(deadline, cancellationToken))
            {
                return await CompleteDeadlineExceededAsync(
                    request,
                    bestEvaluation,
                    attempts,
                    evidenceRefs,
                    warnings,
                    "IMPROVEMENT_LOOP_DEADLINE_EXCEEDED_AFTER_EVALUATION",
                    cancellationToken);
            }

            if (IsRetrySameCandidateCoveragePolicy(policy))
            {
                if (!string.Equals(coverageRetryStopSource, "none", StringComparison.Ordinal))
                {
                    warnings.Add($"IMPROVEMENT_LOOP_COVERAGE_RETRY_STOPPED:{coverageRetryStopSource}");
                    evidenceRefs.Add(DesignConversionVisualQaBoundaryPolicy.CoverageRetryMarker);
                    evidenceRefs.Add($"improvement_loop:coverage_retry_stop_source={coverageRetryStopSource}");
                    var terminalEvaluation = acceptance.AcceptedAsIncumbent ? candidateEvaluation : bestEvaluation;
                    return await CompleteAsync(
                        request,
                        ResolveCoverageRetryStopReason(coverageRetryStopSource),
                        terminalEvaluation,
                        attempts,
                        evidenceRefs,
                        warnings,
                        cancellationToken);
                }

                coverageRetryCount++;
                warnings.Add("IMPROVEMENT_LOOP_COVERAGE_RETRY_SAME_CANDIDATE");
                evidenceRefs.Add(DesignConversionVisualQaBoundaryPolicy.CoverageRetryMarker);
                evidenceRefs.Add($"improvement_loop:coverage_retry_count={coverageRetryCount.ToString(CultureInfo.InvariantCulture)}");
                continue;
            }

            coverageRetryCount = 0;

            if (!policy.ShouldContinue)
            {
                var terminalEvaluation = string.Equals(policy.StopReason, DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete, StringComparison.Ordinal)
                    ? candidateEvaluation
                    : acceptance.AcceptedAsIncumbent ? candidateEvaluation : bestEvaluation;
                if (string.Equals(policy.StopReason, DesignConversionVisualQaBoundaryPolicy.QualityReached, StringComparison.Ordinal) &&
                    !acceptance.AcceptedAsIncumbent)
                {
                    warnings.Add("IMPROVEMENT_LOOP_POLICY_QUALITY_REACHED_REJECTED_BY_INCUMBENT_GUARD");
                    return await CompleteAsync(
                        request,
                        NoProgressStop,
                        terminalEvaluation,
                        attempts,
                        evidenceRefs,
                        warnings,
                        cancellationToken);
                }

                return await CompleteAsync(
                    request,
                    policy.StopReason,
                    terminalEvaluation,
                    attempts,
                    evidenceRefs,
                    warnings,
                    cancellationToken);
            }

            if (!acceptance.AcceptedAsIncumbent && consecutiveNoImprovement >= maxConsecutiveNoImprovement)
            {
                warnings.Add("IMPROVEMENT_LOOP_MAX_CONSECUTIVE_NO_IMPROVEMENT_REACHED");
                return await CompleteAsync(
                    request,
                    NoProgressStop,
                    bestEvaluation,
                    attempts,
                    evidenceRefs,
                    warnings,
                    cancellationToken);
            }

            var loopBudgetExhausted = IsLoopBudgetExhausted(spentBudgetUsd, policy);
            var nextEvaluationWouldExceedBudget = WouldNextEvaluationExceedRemainingBudget(spentBudgetUsd, policy);
            if (loopBudgetExhausted || nextEvaluationWouldExceedBudget)
            {
                warnings.Add(loopBudgetExhausted
                    ? "IMPROVEMENT_LOOP_SPENT_BUDGET_REACHED"
                    : "IMPROVEMENT_LOOP_NEXT_ESTIMATED_COST_EXCEEDS_REMAINING_BUDGET");
                if (nextEvaluationWouldExceedBudget)
                    evidenceRefs.Add("improvement_loop:next_estimated_cost_exceeds_remaining_budget");
                return await CompleteAsync(
                    request,
                    DesignConversionVisualQaBoundaryPolicy.BudgetCapReached,
                    bestEvaluation,
                    attempts,
                    evidenceRefs,
                    warnings,
                    cancellationToken);
            }

            if (attemptNumber >= request.MaxAttempts)
            {
                warnings.Add("IMPROVEMENT_LOOP_MAX_ATTEMPTS_REACHED_WITH_POLICY_STILL_CONTINUING");
                return await CompleteAsync(
                    request,
                    MaxAttemptsReached,
                    bestEvaluation,
                    attempts,
                    evidenceRefs,
                    warnings,
                    cancellationToken);
            }

            if (bestRequest is null || bestEvaluation is null)
            {
                warnings.Add("IMPROVEMENT_LOOP_INCUMBENT_MISSING_AFTER_EVALUATION");
                return await CompleteAsync(
                    request,
                    EvaluationFailed,
                    bestEvaluation,
                    attempts,
                    evidenceRefs,
                    warnings,
                    cancellationToken);
            }

            OperationResult<DesignConversionImprovementCandidate> candidateResult;
            try
            {
                candidateResult = await _candidateStrategy.CreateNextCandidateAsync(
                    new DesignConversionCandidateUpdateRequest(
                        LoopId: request.LoopId,
                        CompletedAttemptNumber: attemptNumber,
                        CurrentRequest: bestRequest,
                        CurrentEvaluation: bestEvaluation,
                        TierPolicy: bestEvaluation.VisualQaBoundaryTrace.TierPolicy,
                        AttemptHistory: attempts),
                    runToken);
            }
            catch (OperationCanceledException) when (IsInternalDeadlineExceeded(deadline, cancellationToken))
            {
                return await CompleteDeadlineExceededAsync(
                    request,
                    bestEvaluation,
                    attempts,
                    evidenceRefs,
                    warnings,
                    "IMPROVEMENT_LOOP_DEADLINE_EXCEEDED_DURING_CANDIDATE_STRATEGY",
                    cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                warnings.Add("IMPROVEMENT_LOOP_CANCELLED_DURING_CANDIDATE_STRATEGY");
                return await CompleteAsync(
                    request,
                    Cancelled,
                    bestEvaluation,
                    attempts,
                    evidenceRefs,
                    warnings,
                    cancellationToken);
            }

            if (!candidateResult.IsSuccess)
            {
                var strategyErrorCode = candidateResult.ErrorCode ?? "UNKNOWN";
                if (string.Equals(strategyErrorCode, "CANCELLED", StringComparison.OrdinalIgnoreCase))
                {
                    if (IsInternalDeadlineExceeded(deadline, cancellationToken))
                    {
                        return await CompleteDeadlineExceededAsync(
                            request,
                            bestEvaluation,
                            attempts,
                            evidenceRefs,
                            warnings,
                            "IMPROVEMENT_LOOP_DEADLINE_EXCEEDED_DURING_CANDIDATE_STRATEGY",
                            cancellationToken);
                    }

                    warnings.Add("IMPROVEMENT_LOOP_CANCELLED_DURING_CANDIDATE_STRATEGY");
                    return await CompleteAsync(
                        request,
                        Cancelled,
                        bestEvaluation,
                        attempts,
                        evidenceRefs,
                        warnings,
                        cancellationToken);
                }

                if (string.Equals(
                    strategyErrorCode,
                    DesignConversionRuntimeImprovementCandidateStrategy.CandidateUnchangedErrorCode,
                    StringComparison.Ordinal))
                {
                    warnings.Add($"IMPROVEMENT_LOOP_CANDIDATE_STRATEGY_NO_PROGRESS:{strategyErrorCode}");
                    return await CompleteAsync(
                        request,
                        NoProgressStop,
                        bestEvaluation,
                        attempts,
                        evidenceRefs,
                        warnings,
                        cancellationToken);
                }

                warnings.Add($"IMPROVEMENT_LOOP_CANDIDATE_STRATEGY_FAILED:{strategyErrorCode}");
                return await CompleteAsync(
                    request,
                    CandidateStrategyFailed,
                    bestEvaluation,
                    attempts,
                    evidenceRefs,
                    warnings,
                    cancellationToken);
            }

            var candidate = candidateResult.Value!;
            var candidateValidation = ValidateCandidate(candidate, bestRequest, currentRequest, evaluatedMarkupFingerprints);
            if (!candidateValidation.IsSuccess)
            {
                var validationErrorCode = candidateValidation.ErrorCode ?? "UNKNOWN";
                if (string.Equals(validationErrorCode, CandidateNoOpErrorCode, StringComparison.Ordinal) ||
                    string.Equals(validationErrorCode, CandidateRepeatedHistoryMarker, StringComparison.Ordinal))
                {
                    warnings.Add($"IMPROVEMENT_LOOP_CANDIDATE_VALIDATION_NO_PROGRESS:{validationErrorCode}");
                    if (string.Equals(validationErrorCode, CandidateRepeatedHistoryMarker, StringComparison.Ordinal))
                    {
                        warnings.Add(CandidateRepeatedHistoryMarker);
                        evidenceRefs.Add(CandidateRepeatedHistoryEvidenceRef);
                    }

                    return await CompleteAsync(
                        request,
                        NoProgressStop,
                        bestEvaluation,
                        attempts,
                        evidenceRefs,
                        warnings,
                        cancellationToken);
                }

                warnings.Add($"IMPROVEMENT_LOOP_CANDIDATE_STRATEGY_FAILED:{validationErrorCode}");
                return await CompleteAsync(
                    request,
                    CandidateStrategyFailed,
                    bestEvaluation,
                    attempts,
                    evidenceRefs,
                    warnings,
                    cancellationToken);
            }

            currentCandidateId = candidate.CandidateId;
            currentRequest = BuildNextRequest(bestRequest, rootJobId, candidate, attemptNumber + 1, bestEvaluation);
        }

        return await CompleteAsync(
            request,
            MaxAttemptsReached,
            bestEvaluation,
            attempts,
            evidenceRefs,
            warnings,
            cancellationToken);
    }

    private async Task<OperationResult<DesignConversionImprovementLoopResult>> CompleteAsync(
        DesignConversionImprovementLoopRequest request,
        string stopReason,
        DesignConversionInternalEvaluationResult? finalEvaluation,
        IReadOnlyList<DesignConversionImprovementLoopAttemptTelemetry> attempts,
        IReadOnlyList<string> evidenceRefs,
        IReadOnlyList<string> warnings,
        CancellationToken cancellationToken)
    {
        var result = BuildResult(request, stopReason, finalEvaluation, attempts, evidenceRefs, warnings, _runDeadline);
        if (_artifactDb is null || result.FinalEvaluation is null)
            return OperationResult<DesignConversionImprovementLoopResult>.Ok(result);

        var createdAtUtc = _utcNow();
        var dpoBuild = new DesignConversionDpoGenerator()
            .BuildRuntimeCreatedTrainingCandidates(result.FinalEvaluation, createdAtUtc);
        var dpoCandidates = dpoBuild.IsSuccess
            ? dpoBuild.Value!.AcceptedRecords
            : [];
        var record = DesignConversionLocalArtifactDbRecord.FromEvaluation(
            result.FinalEvaluation,
            dpoCandidates: dpoCandidates,
            createdAtUtc: createdAtUtc,
            metadata: BuildLocalArtifactDbMetadata(
                request,
                result,
                dpoCandidates.Count,
                dpoBuild.IsSuccess
                    ? dpoBuild.Value!.Warnings
                    : [dpoBuild.ErrorCode ?? "runtime_created_dpo_candidate_build_failed"]));
        var append = await _artifactDb.AppendRecordAsync(record, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        if (!append.IsSuccess)
        {
            return OperationResult<DesignConversionImprovementLoopResult>.Ok(
                MarkLocalArtifactDbPersistenceFailed(
                    result,
                    append.ErrorCode,
                    dpoCandidates.Count));
        }

        return OperationResult<DesignConversionImprovementLoopResult>.Ok(
            MarkLocalArtifactDbPersistence(result, append.Value!, dpoCandidates.Count));
    }

    private Task<OperationResult<DesignConversionImprovementLoopResult>> CompleteDeadlineExceededAsync(
        DesignConversionImprovementLoopRequest request,
        DesignConversionInternalEvaluationResult? finalEvaluation,
        IReadOnlyList<DesignConversionImprovementLoopAttemptTelemetry> attempts,
        List<string> evidenceRefs,
        List<string> warnings,
        string warning,
        CancellationToken cancellationToken)
    {
        warnings.Add(warning);
        evidenceRefs.Add(DeadlineExceededMarker);
        evidenceRefs.Add($"improvement_loop:{ConfiguredRunDeadlineMsMetadataKey}={FormatRunDeadlineMilliseconds(_runDeadline)}");
        var result = BuildResult(request, DeadlineExceeded, finalEvaluation, attempts, evidenceRefs, warnings, _runDeadline);
        return Task.FromResult(OperationResult<DesignConversionImprovementLoopResult>.Ok(
            MarkLocalArtifactDbPersistenceSkippedForDeadline(result)));
    }

    private static bool IsInternalDeadlineExceeded(
        CancellationTokenSource deadline,
        CancellationToken callerCancellationToken)
        => deadline.IsCancellationRequested && !callerCancellationToken.IsCancellationRequested;

    private static TimeSpan NormalizeRunDeadline(TimeSpan? runDeadline)
    {
        var value = runDeadline ?? TimeSpan.FromSeconds(DefaultRunDeadlineSeconds);
        var min = TimeSpan.FromMilliseconds(MinRunDeadlineMilliseconds);
        var max = TimeSpan.FromSeconds(MaxRunDeadlineSeconds);

        if (value <= TimeSpan.Zero || value < min)
            return min;

        return value > max ? max : value;
    }

    private static string FormatRunDeadlineMilliseconds(TimeSpan runDeadline)
        => Math.Ceiling(runDeadline.TotalMilliseconds).ToString(CultureInfo.InvariantCulture);

    private static OperationResult<bool> Validate(DesignConversionImprovementLoopRequest request)
    {
        if (request is null)
            return OperationResult<bool>.Fail("DesignConversion improvement loop request is required.", "IMPROVEMENT_LOOP_REQUEST_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.LoopId))
            return OperationResult<bool>.Fail("DesignConversion improvement loop id is required.", "IMPROVEMENT_LOOP_ID_REQUIRED");
        if (request.InitialEvaluationRequest is null)
            return OperationResult<bool>.Fail("DesignConversion improvement loop initial evaluation request is required.", "IMPROVEMENT_LOOP_INITIAL_REQUEST_REQUIRED");
        if (request.MaxAttempts <= 0)
            return OperationResult<bool>.Fail("DesignConversion improvement loop max attempts must be greater than zero.", "IMPROVEMENT_LOOP_MAX_ATTEMPTS_REQUIRED");
        if (request.MaxAttempts > MaxAllowedAttempts)
            return OperationResult<bool>.Fail(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "DesignConversion improvement loop max attempts must be less than or equal to {0}.",
                    MaxAllowedAttempts),
                MaxAttemptsExceededErrorCode);

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateCandidate(
        DesignConversionImprovementCandidate candidate,
        DesignConversionInternalEvaluateRequest strategyRequest,
        DesignConversionInternalEvaluateRequest lastEvaluatedRequest,
        IReadOnlySet<string> evaluatedMarkupFingerprints)
    {
        if (candidate is null)
            return OperationResult<bool>.Fail("DesignConversion improvement candidate is required.", "IMPROVEMENT_CANDIDATE_REQUIRED");
        if (string.IsNullOrWhiteSpace(candidate.CandidateId))
            return OperationResult<bool>.Fail("DesignConversion improvement candidate id is required.", "IMPROVEMENT_CANDIDATE_ID_REQUIRED");
        if (string.IsNullOrWhiteSpace(candidate.RequestId))
            return OperationResult<bool>.Fail("DesignConversion improvement candidate request id is required.", "IMPROVEMENT_CANDIDATE_REQUEST_ID_REQUIRED");
        if (string.IsNullOrWhiteSpace(candidate.Html))
            return OperationResult<bool>.Fail("DesignConversion improvement candidate HTML is required.", "IMPROVEMENT_CANDIDATE_HTML_REQUIRED");
        if (string.IsNullOrWhiteSpace(candidate.Css))
            return OperationResult<bool>.Fail("DesignConversion improvement candidate CSS is required.", "IMPROVEMENT_CANDIDATE_CSS_REQUIRED");
        if (MatchesRequestMarkup(candidate, strategyRequest) ||
            (!HasSameMarkup(strategyRequest, lastEvaluatedRequest) && MatchesRequestMarkup(candidate, lastEvaluatedRequest)))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion improvement candidate must change HTML or CSS.",
                CandidateNoOpErrorCode);
        }

        if (evaluatedMarkupFingerprints.Contains(BuildMarkupFingerprint(candidate.Html, candidate.Css)))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion improvement candidate repeats an already evaluated HTML/CSS pair.",
                CandidateRepeatedHistoryMarker);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool MatchesRequestMarkup(
        DesignConversionImprovementCandidate candidate,
        DesignConversionInternalEvaluateRequest request)
        => string.Equals(NormalizeMarkup(candidate.Html), NormalizeMarkup(request.Html), StringComparison.Ordinal) &&
           string.Equals(NormalizeMarkup(candidate.Css), NormalizeMarkup(request.Css), StringComparison.Ordinal);

    private static bool HasSameMarkup(
        DesignConversionInternalEvaluateRequest first,
        DesignConversionInternalEvaluateRequest second)
        => string.Equals(NormalizeMarkup(first.Html), NormalizeMarkup(second.Html), StringComparison.Ordinal) &&
           string.Equals(NormalizeMarkup(first.Css), NormalizeMarkup(second.Css), StringComparison.Ordinal);

    private static string NormalizeMarkup(string value)
        => string.Join("", (value ?? string.Empty).Where(c => !char.IsWhiteSpace(c)));

    private static string BuildMarkupFingerprint(string html, string css)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendExactUtf8Field(hash, "html", html);
        AppendExactUtf8Field(hash, "css", css);

        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static void AppendExactUtf8Field(IncrementalHash hash, string fieldName, string? value)
    {
        value ??= string.Empty;
        var byteCount = Encoding.UTF8.GetByteCount(value);

        AppendAscii(hash, fieldName);
        Span<byte> lengthBytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(lengthBytes, byteCount);
        hash.AppendData(lengthBytes);

        var buffer = ArrayPool<byte>.Shared.Rent(Math.Min(Math.Max(byteCount, 1), 4096));
        try
        {
            var encoder = Encoding.UTF8.GetEncoder();
            var remaining = value.AsSpan();
            while (!remaining.IsEmpty)
            {
                encoder.Convert(
                    remaining,
                    buffer,
                    flush: true,
                    out var charsUsed,
                    out var bytesUsed,
                    out _);
                if (bytesUsed > 0)
                    hash.AppendData(buffer.AsSpan(0, bytesUsed));

                remaining = remaining[charsUsed..];
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static void AppendAscii(IncrementalHash hash, string value)
    {
        Span<byte> fieldBytes = stackalloc byte[value.Length];
        for (var i = 0; i < value.Length; i++)
            fieldBytes[i] = (byte)value[i];

        hash.AppendData(fieldBytes);
    }

    private static CandidateAcceptanceDecision AssessCandidate(
        int attemptNumber,
        string candidateId,
        DesignConversionInternalEvaluationResult candidateEvaluation,
        DesignConversionInternalEvaluationResult? incumbentEvaluation,
        int consecutiveNoImprovement,
        int maxConsecutiveNoImprovement,
        double improvementEpsilon)
    {
        var candidateScore = GetMeasuredQualityScore(candidateEvaluation);
        var candidateCoverageComplete = candidateEvaluation.VisualQaBoundaryTrace.MultiResolutionRender.IsComplete;
        if (attemptNumber == 1 || incumbentEvaluation is null)
        {
            return new CandidateAcceptanceDecision(
                Decision: "initial_incumbent",
                AcceptedAsIncumbent: true,
                RejectionReason: "none",
                IncumbentScoreBefore: candidateScore,
                CandidateMeasuredScore: candidateScore,
                MeasuredImprovement: 0.0,
                ImprovementEpsilon: improvementEpsilon,
                QualityNonRegression: false,
                QualityNonRegressionEvidenceMode: "unavailable",
                ViewportNonRegression: false,
                RequiredViewportCoverageComplete: candidateCoverageComplete,
                ConsecutiveNoImprovement: consecutiveNoImprovement,
                MaxConsecutiveNoImprovement: maxConsecutiveNoImprovement,
                SpentBudgetUsd: 0m,
                BestCandidateId: candidateId);
        }

        var incumbentScore = GetMeasuredQualityScore(incumbentEvaluation);
        var measuredImprovement = candidateScore - incumbentScore;
        var qualityNonRegression = AssessQualityNonRegression(
            candidateEvaluation,
            incumbentEvaluation,
            improvementEpsilon);
        if (!incumbentEvaluation.VisualQaBoundaryTrace.MultiResolutionRender.IsComplete &&
            candidateCoverageComplete &&
            IsCoverageRecoveryNonRegression(qualityNonRegression))
        {
            return new CandidateAcceptanceDecision(
                Decision: "accepted_required_viewport_coverage_completed",
                AcceptedAsIncumbent: true,
                RejectionReason: "none",
                IncumbentScoreBefore: incumbentScore,
                CandidateMeasuredScore: candidateScore,
                MeasuredImprovement: measuredImprovement,
                ImprovementEpsilon: improvementEpsilon,
                QualityNonRegression: qualityNonRegression.NonRegression,
                QualityNonRegressionEvidenceMode: qualityNonRegression.EvidenceMode,
                ViewportNonRegression: qualityNonRegression.ViewportNonRegression,
                RequiredViewportCoverageComplete: true,
                ConsecutiveNoImprovement: consecutiveNoImprovement,
                MaxConsecutiveNoImprovement: maxConsecutiveNoImprovement,
                SpentBudgetUsd: 0m,
                BestCandidateId: candidateId);
        }

        var accepted = candidateCoverageComplete &&
            qualityNonRegression.NonRegression &&
            measuredImprovement >= improvementEpsilon;
        var rejectionReason = accepted
            ? "none"
            : ResolveCandidateRejectionReason(
                candidateCoverageComplete,
                qualityNonRegression,
                measuredImprovement,
                improvementEpsilon);

        return new CandidateAcceptanceDecision(
            Decision: accepted ? "accepted_measured_improvement" : "rejected_candidate",
            AcceptedAsIncumbent: accepted,
            RejectionReason: rejectionReason,
            IncumbentScoreBefore: incumbentScore,
            CandidateMeasuredScore: candidateScore,
            MeasuredImprovement: measuredImprovement,
            ImprovementEpsilon: improvementEpsilon,
            QualityNonRegression: qualityNonRegression.NonRegression,
            QualityNonRegressionEvidenceMode: qualityNonRegression.EvidenceMode,
            ViewportNonRegression: qualityNonRegression.ViewportNonRegression,
            RequiredViewportCoverageComplete: candidateCoverageComplete,
            ConsecutiveNoImprovement: consecutiveNoImprovement,
            MaxConsecutiveNoImprovement: maxConsecutiveNoImprovement,
            SpentBudgetUsd: 0m,
            BestCandidateId: candidateId);
    }

    private static bool IsCoverageRecoveryNonRegression(QualityNonRegressionEvidence qualityNonRegression)
        => qualityNonRegression.NonRegression &&
           (string.Equals(qualityNonRegression.EvidenceMode, "viewport_scoped", StringComparison.Ordinal) ||
            string.Equals(qualityNonRegression.EvidenceMode, "coverage_recovery_score_guarded", StringComparison.Ordinal));

    private static string ResolveCandidateRejectionReason(
        bool candidateCoverageComplete,
        QualityNonRegressionEvidence qualityNonRegression,
        double measuredImprovement,
        double improvementEpsilon)
    {
        if (!candidateCoverageComplete)
            return "required_viewport_coverage_incomplete";
        if (string.Equals(qualityNonRegression.RejectionReason, "quality_evidence_unavailable", StringComparison.Ordinal))
            return "quality_evidence_unavailable";
        if (string.Equals(qualityNonRegression.RejectionReason, "unscoped_qa_regression", StringComparison.Ordinal))
            return "unscoped_qa_regression";
        if (measuredImprovement < 0.0)
            return "quality_regression";
        if (!qualityNonRegression.NonRegression)
            return qualityNonRegression.RejectionReason;
        if (measuredImprovement < improvementEpsilon)
            return "no_measured_improvement";
        return "candidate_rejected";
    }

    private static QualityNonRegressionEvidence AssessQualityNonRegression(
        DesignConversionInternalEvaluationResult candidateEvaluation,
        DesignConversionInternalEvaluationResult incumbentEvaluation,
        double improvementEpsilon)
    {
        var candidateScores = GetViewportScores(candidateEvaluation);
        var incumbentScores = GetViewportScores(incumbentEvaluation);
        if (candidateScores.Count > 0 && incumbentScores.Count > 0)
        {
            var viewportNonRegression = HasViewportNonRegression(candidateScores, incumbentScores, improvementEpsilon);
            return new QualityNonRegressionEvidence(
                NonRegression: viewportNonRegression,
                EvidenceMode: "viewport_scoped",
                RejectionReason: viewportNonRegression ? "none" : "viewport_regression",
                ViewportNonRegression: viewportNonRegression);
        }

        if (candidateEvaluation.VisualQaBoundaryTrace.MultiResolutionRender.IsComplete &&
            !incumbentEvaluation.VisualQaBoundaryTrace.MultiResolutionRender.IsComplete &&
            HasFiniteMetadataOnlyQaEvidenceForObservedCoverage(candidateEvaluation) &&
            HasFiniteMetadataOnlyQaEvidenceForObservedCoverage(incumbentEvaluation))
        {
            var candidateMeasuredScore = GetMeasuredQualityScore(candidateEvaluation);
            var incumbentMeasuredScore = GetMeasuredQualityScore(incumbentEvaluation);
            var scoreGuardedNonRegression = candidateMeasuredScore + improvementEpsilon >= incumbentMeasuredScore;
            return new QualityNonRegressionEvidence(
                NonRegression: scoreGuardedNonRegression,
                EvidenceMode: "coverage_recovery_score_guarded",
                RejectionReason: scoreGuardedNonRegression ? "none" : "coverage_recovery_quality_regression",
                ViewportNonRegression: false);
        }

        if (candidateScores.Count != incumbentScores.Count)
            return QualityEvidenceUnavailable();

        var candidateDistribution = GetUnscopedQaScoreDistribution(candidateEvaluation);
        var incumbentDistribution = GetUnscopedQaScoreDistribution(incumbentEvaluation);
        if (candidateDistribution.Count == 0 ||
            incumbentDistribution.Count == 0 ||
            candidateDistribution.Count != incumbentDistribution.Count)
        {
            return QualityEvidenceUnavailable();
        }

        for (var index = 0; index < incumbentDistribution.Count; index++)
        {
            if (candidateDistribution[index] + improvementEpsilon < incumbentDistribution[index])
            {
                return new QualityNonRegressionEvidence(
                    NonRegression: false,
                    EvidenceMode: "unscoped_qa_distribution",
                    RejectionReason: "unscoped_qa_regression",
                    ViewportNonRegression: false);
            }
        }

        return new QualityNonRegressionEvidence(
            NonRegression: true,
            EvidenceMode: "unscoped_qa_distribution",
            RejectionReason: "none",
            ViewportNonRegression: false);
    }

    private static bool HasFiniteMetadataOnlyQaEvidenceForObservedCoverage(
        DesignConversionInternalEvaluationResult evaluation)
    {
        var observedViewportCount = evaluation.VisualQaBoundaryTrace.MultiResolutionRender.ObservedViewportIds
            .Where(viewportId => !string.IsNullOrWhiteSpace(viewportId))
            .Distinct(StringComparer.Ordinal)
            .Count();
        if (observedViewportCount == 0 || evaluation.QaResults.Count != observedViewportCount)
            return false;

        return evaluation.QaResults.All(result =>
            result is not null &&
            result.Actual is not null &&
            result.Actual.Resolution is null &&
            IsFiniteScore(result.Verdict.Score));
    }

    private static bool HasViewportNonRegression(
        IReadOnlyDictionary<string, double> candidateScores,
        IReadOnlyDictionary<string, double> incumbentScores,
        double improvementEpsilon)
    {
        foreach (var pair in incumbentScores)
        {
            if (!candidateScores.TryGetValue(pair.Key, out var candidateScore))
                return false;
            if (candidateScore + improvementEpsilon < pair.Value)
                return false;
        }

        return true;
    }

    private static QualityNonRegressionEvidence QualityEvidenceUnavailable()
        => new(
            NonRegression: false,
            EvidenceMode: "unavailable",
            RejectionReason: "quality_evidence_unavailable",
            ViewportNonRegression: false);

    private static IReadOnlyDictionary<string, double> GetViewportScores(
        DesignConversionInternalEvaluationResult evaluation)
        => evaluation.QaResults
            .Where(result => result?.Actual?.Resolution is not null && IsFiniteScore(result.Verdict.Score))
            .GroupBy(result => NormalizeResolutionId(result.Actual.Resolution), StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Min(result => result.Verdict.Score),
                StringComparer.Ordinal);

    private static IReadOnlyList<double> GetUnscopedQaScoreDistribution(
        DesignConversionInternalEvaluationResult evaluation)
        => evaluation.QaResults
            .Where(result => result is not null &&
                result.Actual?.Resolution is null &&
                IsFiniteScore(result.Verdict.Score))
            .Select(result => result.Verdict.Score)
            .OrderBy(score => score)
            .ToArray();

    private static double GetMeasuredQualityScore(DesignConversionInternalEvaluationResult evaluation)
    {
        var viewportScores = GetViewportScores(evaluation);
        if (viewportScores.Count > 0)
            return viewportScores.Values.Min();

        var verdictScores = evaluation.QaResults
            .Where(result => result is not null && IsFiniteScore(result.Verdict.Score))
            .Select(result => result.Verdict.Score);
        return verdictScores.DefaultIfEmpty(0.0).Min();
    }

    private static bool IsFiniteScore(double score)
        => !double.IsNaN(score) && !double.IsInfinity(score);

    private static string NormalizeResolutionId(RenderQaResolution resolution)
        => string.IsNullOrWhiteSpace(resolution.ResolutionId)
            ? $"{resolution.Width.ToString(CultureInfo.InvariantCulture)}x{resolution.Height.ToString(CultureInfo.InvariantCulture)}@{resolution.DeviceScaleFactor.ToString("R", CultureInfo.InvariantCulture)}"
            : resolution.ResolutionId;

    private static bool IsLoopBudgetExhausted(
        decimal spentBudgetUsd,
        DesignConversionTierPolicyDecision policy)
        => !string.Equals(policy.StopReason, DesignConversionVisualQaBoundaryPolicy.QualityReached, StringComparison.Ordinal) &&
           policy.BudgetUsd > 0m &&
           spentBudgetUsd >= policy.BudgetUsd;

    private static bool WouldNextEvaluationExceedRemainingBudget(
        decimal spentBudgetUsd,
        DesignConversionTierPolicyDecision policy)
        => !string.Equals(policy.StopReason, DesignConversionVisualQaBoundaryPolicy.QualityReached, StringComparison.Ordinal) &&
           policy.BudgetUsd > 0m &&
           policy.EstimatedCostUsd > 0m &&
           spentBudgetUsd < policy.BudgetUsd &&
           spentBudgetUsd + policy.EstimatedCostUsd > policy.BudgetUsd;

    private static bool IsRetrySameCandidateCoveragePolicy(DesignConversionTierPolicyDecision policy)
        => string.Equals(policy.RetryMode, DesignConversionVisualQaBoundaryPolicy.RetryModeRetrySameCandidate, StringComparison.Ordinal) &&
           string.Equals(policy.StopReason, DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete, StringComparison.Ordinal);

    private static string ResolveCoverageRetryStopSource(
        int completedAttemptNumber,
        DesignConversionImprovementLoopRequest request,
        decimal spentBudgetUsd,
        DesignConversionTierPolicyDecision policy)
    {
        var costBudgetReached = policy.BudgetUsd <= 0m ||
            IsLoopBudgetExhausted(spentBudgetUsd, policy) ||
            WouldNextEvaluationExceedRemainingBudget(spentBudgetUsd, policy);
        var tierIterationBudgetReached = policy.MaxImprovementCandidates <= 0 ||
            completedAttemptNumber >= policy.MaxImprovementCandidates;
        var configuredMaxAttemptsReached = completedAttemptNumber >= request.MaxAttempts;

        if (costBudgetReached && tierIterationBudgetReached)
            return "cost_and_iteration_budget";
        if (costBudgetReached)
            return "cost_budget";
        if (tierIterationBudgetReached)
            return "iteration_budget";
        if (configuredMaxAttemptsReached)
            return "configured_max_attempts";
        return "none";
    }

    private static string ResolveCoverageRetryStopReason(string coverageRetryStopSource)
        => string.Equals(coverageRetryStopSource, "configured_max_attempts", StringComparison.Ordinal)
            ? DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete
            : DesignConversionVisualQaBoundaryPolicy.BudgetCapReached;

    private static double ResolveQualityImprovementEpsilon(DesignConversionImprovementLoopRequest request)
        => TryReadDouble(request.Metadata, "quality_improvement_epsilon")
           ?? TryReadDouble(request.InitialEvaluationRequest.Budget.Metadata, "quality_improvement_epsilon")
           ?? DefaultQualityImprovementEpsilon;

    private static int ResolveMaxConsecutiveNoImprovement(DesignConversionImprovementLoopRequest request)
        => Math.Max(
            1,
            TryReadInt(request.Metadata, "max_consecutive_no_improvement")
            ?? TryReadInt(request.InitialEvaluationRequest.Budget.Metadata, "max_consecutive_no_improvement")
            ?? request.MaxAttempts);

    private static double? TryReadDouble(IReadOnlyDictionary<string, string>? metadata, string key)
        => metadata is not null &&
           metadata.TryGetValue(key, out var value) &&
           double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? Math.Max(0.0, parsed)
            : null;

    private static int? TryReadInt(IReadOnlyDictionary<string, string>? metadata, string key)
        => metadata is not null &&
           metadata.TryGetValue(key, out var value) &&
           int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;

    private static DesignConversionInternalEvaluateRequest BuildNextRequest(
        DesignConversionInternalEvaluateRequest currentRequest,
        string rootJobId,
        DesignConversionImprovementCandidate candidate,
        int nextAttemptNumber,
        DesignConversionInternalEvaluationResult previousEvaluation)
        => currentRequest with
        {
            JobId = $"{rootJobId}:loop-attempt:{nextAttemptNumber.ToString(CultureInfo.InvariantCulture)}",
            RequestId = candidate.RequestId,
            Html = candidate.Html,
            Css = candidate.Css,
            DeterministicEvidenceRefs = MergeStringDictionaries(
                currentRequest.DeterministicEvidenceRefs,
                new Dictionary<string, string>
                {
                    ["improvement_loop_candidate_ref"] = HashRef("dcm-loop-candidate", candidate.CandidateId),
                    ["previous_evaluation_job_ref"] = HashRef("dcm-loop-job", previousEvaluation.JobId)
                }),
            Provenance = MergeStringDictionaries(
                currentRequest.Provenance,
                new Dictionary<string, string>
                {
                    ["improvement_loop"] = CoordinatorMarker,
                    ["improvement_loop_candidate_ref"] = HashRef("dcm-loop-candidate", candidate.CandidateId),
                    ["improvement_loop_attempt"] = nextAttemptNumber.ToString(CultureInfo.InvariantCulture)
                })
        };

    private static DesignConversionImprovementLoopAttemptTelemetry BuildAttemptTelemetry(
        int attemptNumber,
        string candidateId,
        DesignConversionInternalEvaluateRequest request,
        DesignConversionInternalEvaluationResult evaluation,
        CandidateAcceptanceDecision acceptance,
        bool isCoverageRetryAttempt,
        int coverageRetryCount,
        string coverageRetryStopSource)
    {
        var policy = evaluation.VisualQaBoundaryTrace.TierPolicy;
        var visualDecision = evaluation.VisualQaBoundaryTrace.VisualDecision;
        var renderRefs = evaluation.RenderResults
            .Select(result => HashRef("dcm-loop-render", result.RenderResultId))
            .ToArray();
        var comparisonRefs = evaluation.ComparisonArtifacts
            .Select(result => HashRef("dcm-loop-comparison", result.ArtifactId))
            .ToArray();
        var visualJudgeRefs = evaluation.VisualJudgeResults
            .Select(result => HashRef("dcm-loop-visual-judge", result.JudgeResultId))
            .ToArray();
        var browserRenderResultCount = evaluation.RenderResults.Count(result =>
            IsTruthy(result.Image.Metadata, "browser_or_playwright_used"));
        var browserRenderExecuted = browserRenderResultCount > 0;
        var loopEvidenceRefs = new List<string>
        {
            CoordinatorMarker,
            RefsOnlyTelemetryMarker,
            DesignConversionVisualQaBoundaryPolicy.ImprovementLoopMarker,
            $"improvement_loop:attempt={attemptNumber.ToString(CultureInfo.InvariantCulture)}",
            $"improvement_loop:stop_reason={policy.StopReason}"
        };
        if (isCoverageRetryAttempt || IsRetrySameCandidateCoveragePolicy(policy))
        {
            loopEvidenceRefs.Add(DesignConversionVisualQaBoundaryPolicy.CoverageRetryMarker);
            loopEvidenceRefs.Add($"improvement_loop:coverage_retry_count={coverageRetryCount.ToString(CultureInfo.InvariantCulture)}");
            loopEvidenceRefs.Add($"improvement_loop:coverage_retry_stop_source={coverageRetryStopSource}");
        }

        var evidenceRefs = evaluation.EvidenceRefs
            .Concat(policy.EvidenceRefs)
            .Select(reference => HashRef("dcm-loop-evidence", reference))
            .Concat(loopEvidenceRefs)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(reference => reference, StringComparer.Ordinal)
            .ToArray();
        var publicMetadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["refs_or_hashes_only"] = bool.TrueString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["screenshot_bytes_included"] = bool.FalseString,
            ["loop_candidate_ref"] = HashRef("dcm-loop-candidate", candidateId),
            ["job_ref"] = HashRef("dcm-loop-job", evaluation.JobId),
            ["request_ref"] = HashRef("dcm-loop-request", request.RequestId),
            ["attempt_number"] = attemptNumber.ToString(CultureInfo.InvariantCulture),
            ["tier"] = policy.Tier,
            ["score_direction"] = ScoreDirectionHigherIsBetter,
            ["stop_reason"] = policy.StopReason,
            ["should_continue"] = policy.ShouldContinue.ToString(),
            ["retry_mode"] = policy.RetryMode,
            ["coverage_retry"] = isCoverageRetryAttempt.ToString(),
            ["coverage_retry_count"] = coverageRetryCount.ToString(CultureInfo.InvariantCulture),
            ["coverage_retry_stop_source"] = coverageRetryStopSource,
            ["coverage_retry_same_candidate"] = IsRetrySameCandidateCoveragePolicy(policy).ToString(),
            ["coverage_retry_css_mutated"] = bool.FalseString,
            ["best_score"] = policy.BestScore.ToString("0.####", CultureInfo.InvariantCulture),
            ["measured_quality_score"] = acceptance.CandidateMeasuredScore.ToString("0.####", CultureInfo.InvariantCulture),
            ["incumbent_score_before"] = acceptance.IncumbentScoreBefore.ToString("0.####", CultureInfo.InvariantCulture),
            ["measured_improvement"] = acceptance.MeasuredImprovement.ToString("0.####", CultureInfo.InvariantCulture),
            ["improvement_epsilon"] = acceptance.ImprovementEpsilon.ToString("0.####", CultureInfo.InvariantCulture),
            ["candidate_acceptance_decision"] = acceptance.Decision,
            ["candidate_accepted_as_incumbent"] = acceptance.AcceptedAsIncumbent.ToString(),
            ["candidate_rejection_reason"] = acceptance.RejectionReason,
            ["quality_non_regression"] = acceptance.QualityNonRegression.ToString(),
            ["quality_non_regression_evidence_mode"] = acceptance.QualityNonRegressionEvidenceMode,
            ["viewport_non_regression"] = acceptance.ViewportNonRegression.ToString(),
            ["required_viewport_coverage_complete"] = acceptance.RequiredViewportCoverageComplete.ToString(),
            ["consecutive_no_improvement"] = acceptance.ConsecutiveNoImprovement.ToString(CultureInfo.InvariantCulture),
            ["max_consecutive_no_improvement"] = acceptance.MaxConsecutiveNoImprovement.ToString(CultureInfo.InvariantCulture),
            ["loop_spent_budget_usd"] = acceptance.SpentBudgetUsd.ToString("0.####", CultureInfo.InvariantCulture),
            ["best_incumbent_candidate_ref"] = HashRef("dcm-loop-candidate", acceptance.BestCandidateId),
            ["cost_savings_accept_lower_quality"] = bool.FalseString,
            ["required_score"] = policy.RequiredScore.ToString("0.####", CultureInfo.InvariantCulture),
            ["estimated_cost_usd"] = policy.EstimatedCostUsd.ToString("0.####", CultureInfo.InvariantCulture),
            ["budget_usd"] = policy.BudgetUsd.ToString("0.####", CultureInfo.InvariantCulture),
            ["remaining_budget_usd"] = policy.RemainingBudgetUsd.ToString("0.####", CultureInfo.InvariantCulture),
            ["policy_attempt_count"] = policy.AttemptCount.ToString(CultureInfo.InvariantCulture),
            ["max_improvement_candidates"] = policy.MaxImprovementCandidates.ToString(CultureInfo.InvariantCulture),
            ["submitted_budget_usd"] = ReadPolicyMetadata(policy, "submitted_budget_usd", policy.BudgetUsd.ToString("0.####", CultureInfo.InvariantCulture)),
            ["submitted_max_improvement_candidates"] = ReadPolicyMetadata(policy, "submitted_max_improvement_candidates", policy.MaxImprovementCandidates.ToString(CultureInfo.InvariantCulture)),
            ["effective_budget_usd"] = ReadPolicyMetadata(policy, "effective_budget_usd", policy.BudgetUsd.ToString("0.####", CultureInfo.InvariantCulture)),
            ["effective_max_improvement_candidates"] = ReadPolicyMetadata(policy, "effective_max_improvement_candidates", policy.MaxImprovementCandidates.ToString(CultureInfo.InvariantCulture)),
            ["tier_budget_cap_usd"] = ReadPolicyMetadata(policy, "tier_budget_cap_usd", "unbounded"),
            ["tier_max_improvement_candidates"] = ReadPolicyMetadata(policy, "tier_max_improvement_candidates", "unbounded"),
            ["tier_policy_budget_cap_applied"] = ReadPolicyMetadata(policy, "tier_policy_budget_cap_applied", bool.FalseString),
            ["tier_policy_iteration_cap_applied"] = ReadPolicyMetadata(policy, "tier_policy_iteration_cap_applied", bool.FalseString),
            ["tier_policy_budget_entitlement_default_applied"] = ReadPolicyMetadata(policy, "tier_policy_budget_entitlement_default_applied", bool.FalseString),
            ["tier_policy_iteration_entitlement_default_applied"] = ReadPolicyMetadata(policy, "tier_policy_iteration_entitlement_default_applied", bool.FalseString),
            ["budget_stop_source"] = ReadPolicyMetadata(policy, "budget_stop_source", "none"),
            ["visual_decision"] = visualDecision.Decision,
            ["visual_decision_can_retry"] = visualDecision.CanRetry.ToString(),
            ["visual_decision_quality_threshold_met"] = visualDecision.QualityThresholdMet.ToString(),
            ["visual_decision_cost_budget_met"] = visualDecision.CostBudgetMet.ToString(),
            ["visual_decision_remaining_budget_usd"] = visualDecision.RemainingBudgetUsd.ToString("0.####", CultureInfo.InvariantCulture),
            ["visual_decision_correction_count"] = visualDecision.CorrectionFindings.Count.ToString(CultureInfo.InvariantCulture),
            ["visual_decision_correction_categories"] = string.Join(
                "|",
                visualDecision.CorrectionFindings
                    .Select(finding => finding.Category)
                    .Where(category => !string.IsNullOrWhiteSpace(category))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(category => category, StringComparer.Ordinal)),
            ["render_result_count"] = evaluation.RenderResults.Count.ToString(CultureInfo.InvariantCulture),
            ["browser_render_executed"] = browserRenderExecuted.ToString(),
            ["browser_render_not_executed"] = (!browserRenderExecuted).ToString(),
            ["browser_render_result_count"] = browserRenderResultCount.ToString(CultureInfo.InvariantCulture),
            ["comparison_artifact_count"] = evaluation.ComparisonArtifacts.Count.ToString(CultureInfo.InvariantCulture),
            ["visual_judge_result_count"] = evaluation.VisualJudgeResults.Count.ToString(CultureInfo.InvariantCulture),
            ["visual_judge_provider_boundary"] = evaluation.VisualQaBoundaryTrace.VisualJudgeProviderBoundary,
            ["external_ai_visual_judge_executed"] = evaluation.VisualQaBoundaryTrace.ExternalAiVisualJudgeExecuted.ToString(),
            ["product_ready"] = evaluation.VisualQaBoundaryTrace.ProductReady.ToString(),
            ["trained_feature_claimed"] = evaluation.VisualQaBoundaryTrace.TrainedFeatureClaimed.ToString()
        };
        foreach (var pair in BuildVisualProviderBoundaryMetadata(evaluation))
            publicMetadata[pair.Key] = pair.Value;

        return new DesignConversionImprovementLoopAttemptTelemetry(
            AttemptNumber: attemptNumber,
            CandidateRef: publicMetadata["loop_candidate_ref"],
            JobRef: publicMetadata["job_ref"],
            RequestRef: publicMetadata["request_ref"],
            StopReason: policy.StopReason,
            ShouldContinue: policy.ShouldContinue,
            BestScore: policy.BestScore,
            RequiredScore: policy.RequiredScore,
            EstimatedCostUsd: policy.EstimatedCostUsd,
            BudgetUsd: policy.BudgetUsd,
            PolicyAttemptCount: policy.AttemptCount,
            MaxImprovementCandidates: policy.MaxImprovementCandidates,
            RenderResultRefs: renderRefs,
            ComparisonArtifactRefs: comparisonRefs,
            VisualJudgeResultRefs: visualJudgeRefs,
            EvidenceRefs: evidenceRefs,
            PublicMetadata: publicMetadata)
        {
            VisualCostMeasurementComplete = policy.ExternalCostMeasurementComplete,
            VisualCostMissingMeasurementCount = policy.MissingExternalCostMeasurementCount,
            VisualCostSource = string.IsNullOrWhiteSpace(policy.CostSource)
                ? DesignConversionVisualCostAccounting.UnknownCostSource
                : policy.CostSource
        };
    }

    private static DesignConversionImprovementLoopAttemptTelemetry BuildFailedAttemptTelemetry(
        int attemptNumber,
        string candidateId,
        DesignConversionInternalEvaluateRequest request,
        DesignConversionInternalEvaluationResult? incumbentEvaluation,
        string errorCode,
        decimal spentBudgetUsd,
        int consecutiveNoImprovement,
        int maxConsecutiveNoImprovement,
        double improvementEpsilon)
    {
        var incumbentPolicy = incumbentEvaluation?.VisualQaBoundaryTrace.TierPolicy;
        var incumbentScore = incumbentEvaluation is null ? 0.0 : GetMeasuredQualityScore(incumbentEvaluation);
        var requiredScore = incumbentPolicy?.RequiredScore
            ?? TryReadDouble(request.Budget.Metadata, "quality_threshold")
            ?? 0.0;
        var budgetUsd = incumbentPolicy?.BudgetUsd ?? Math.Max(0m, request.Budget.BudgetUsd);
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["refs_or_hashes_only"] = bool.TrueString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["screenshot_bytes_included"] = bool.FalseString,
            ["loop_candidate_ref"] = HashRef("dcm-loop-candidate", candidateId),
            ["job_ref"] = HashRef("dcm-loop-job", request.JobId),
            ["request_ref"] = HashRef("dcm-loop-request", request.RequestId),
            ["attempt_number"] = attemptNumber.ToString(CultureInfo.InvariantCulture),
            ["tier"] = request.Budget.Tier,
            ["score_direction"] = ScoreDirectionHigherIsBetter,
            ["stop_reason"] = EvaluationFailed,
            ["should_continue"] = bool.FalseString,
            ["best_score"] = incumbentScore.ToString("0.####", CultureInfo.InvariantCulture),
            ["measured_quality_score"] = incumbentScore.ToString("0.####", CultureInfo.InvariantCulture),
            ["incumbent_score_before"] = incumbentScore.ToString("0.####", CultureInfo.InvariantCulture),
            ["measured_improvement"] = "0",
            ["improvement_epsilon"] = improvementEpsilon.ToString("0.####", CultureInfo.InvariantCulture),
            ["candidate_acceptance_decision"] = "evaluation_failed_incumbent_retained",
            ["candidate_accepted_as_incumbent"] = bool.FalseString,
            ["candidate_rejection_reason"] = "evaluation_failed",
            ["quality_non_regression"] = bool.FalseString,
            ["quality_non_regression_evidence_mode"] = "unavailable",
            ["viewport_non_regression"] = bool.FalseString,
            ["required_viewport_coverage_complete"] = bool.FalseString,
            ["consecutive_no_improvement"] = consecutiveNoImprovement.ToString(CultureInfo.InvariantCulture),
            ["max_consecutive_no_improvement"] = maxConsecutiveNoImprovement.ToString(CultureInfo.InvariantCulture),
            ["loop_spent_budget_usd"] = spentBudgetUsd.ToString("0.####", CultureInfo.InvariantCulture),
            ["best_incumbent_candidate_ref"] = HashRef("dcm-loop-candidate", "initial"),
            ["evaluation_error_code"] = DesignConversionDpoLeakageGuard.HashRef("dcm-loop-evaluation-error", errorCode),
            ["required_score"] = requiredScore.ToString("0.####", CultureInfo.InvariantCulture),
            ["estimated_cost_usd"] = "0",
            ["budget_usd"] = budgetUsd.ToString("0.####", CultureInfo.InvariantCulture),
            ["remaining_budget_usd"] = Math.Max(0m, budgetUsd - spentBudgetUsd).ToString("0.####", CultureInfo.InvariantCulture),
            ["policy_attempt_count"] = "0",
            ["max_improvement_candidates"] = request.Budget.MaxImprovementCandidates.ToString(CultureInfo.InvariantCulture),
            ["submitted_budget_usd"] = request.Budget.BudgetUsd.ToString("0.####", CultureInfo.InvariantCulture),
            ["submitted_max_improvement_candidates"] = request.Budget.MaxImprovementCandidates.ToString(CultureInfo.InvariantCulture),
            ["effective_budget_usd"] = budgetUsd.ToString("0.####", CultureInfo.InvariantCulture),
            ["effective_max_improvement_candidates"] = request.Budget.MaxImprovementCandidates.ToString(CultureInfo.InvariantCulture),
            ["tier_budget_cap_usd"] = "unbounded",
            ["tier_max_improvement_candidates"] = "unbounded",
            ["tier_policy_budget_cap_applied"] = bool.FalseString,
            ["tier_policy_iteration_cap_applied"] = bool.FalseString,
            ["tier_policy_budget_entitlement_default_applied"] = bool.FalseString,
            ["tier_policy_iteration_entitlement_default_applied"] = bool.FalseString,
            ["budget_stop_source"] = "none",
            ["visual_decision"] = "blocked",
            ["visual_decision_can_retry"] = bool.FalseString,
            ["visual_decision_quality_threshold_met"] = bool.FalseString,
            ["visual_decision_cost_budget_met"] = (spentBudgetUsd <= budgetUsd).ToString(),
            ["visual_decision_remaining_budget_usd"] = Math.Max(0m, budgetUsd - spentBudgetUsd).ToString("0.####", CultureInfo.InvariantCulture),
            ["visual_decision_correction_count"] = "0",
            ["visual_decision_correction_categories"] = "evaluation_failed",
            ["render_result_count"] = "0",
            ["browser_render_executed"] = bool.FalseString,
            ["browser_render_not_executed"] = bool.TrueString,
            ["browser_render_result_count"] = "0",
            ["comparison_artifact_count"] = "0",
            ["visual_judge_result_count"] = "0",
            ["visual_judge_provider_boundary"] = EvaluationFailed,
            ["external_ai_visual_judge_executed"] = bool.FalseString,
            ["product_ready"] = bool.FalseString,
            ["trained_feature_claimed"] = bool.FalseString
        };
        var evidenceRefs = new[]
        {
            CoordinatorMarker,
            RefsOnlyTelemetryMarker,
            DesignConversionVisualQaBoundaryPolicy.ImprovementLoopMarker,
            $"improvement_loop:attempt={attemptNumber.ToString(CultureInfo.InvariantCulture)}",
            $"improvement_loop:stop_reason={EvaluationFailed}",
            "improvement_loop:evaluation_failed_incumbent_retained",
            DesignConversionVisualQaBoundaryPolicy.ProductNotReadyMarker
        };

        return new DesignConversionImprovementLoopAttemptTelemetry(
            AttemptNumber: attemptNumber,
            CandidateRef: metadata["loop_candidate_ref"],
            JobRef: metadata["job_ref"],
            RequestRef: metadata["request_ref"],
            StopReason: EvaluationFailed,
            ShouldContinue: false,
            BestScore: incumbentScore,
            RequiredScore: requiredScore,
            EstimatedCostUsd: 0m,
            BudgetUsd: budgetUsd,
            PolicyAttemptCount: 0,
            MaxImprovementCandidates: request.Budget.MaxImprovementCandidates,
            RenderResultRefs: [],
            ComparisonArtifactRefs: [],
            VisualJudgeResultRefs: [],
            EvidenceRefs: evidenceRefs,
            PublicMetadata: metadata)
        {
            VisualCostMeasurementComplete = false,
            VisualCostMissingMeasurementCount = 1,
            VisualCostSource = DesignConversionVisualCostAccounting.UnknownCostSource
        };
    }

    private static DesignConversionImprovementLoopResult BuildResult(
        DesignConversionImprovementLoopRequest request,
        string stopReason,
        DesignConversionInternalEvaluationResult? finalEvaluation,
        IReadOnlyList<DesignConversionImprovementLoopAttemptTelemetry> attempts,
        IReadOnlyList<string> evidenceRefs,
        IReadOnlyList<string> warnings,
        TimeSpan configuredRunDeadline)
    {
        var qualityReached = string.Equals(stopReason, DesignConversionVisualQaBoundaryPolicy.QualityReached, StringComparison.Ordinal);
        var providerRequired = string.Equals(stopReason, DesignConversionVisualQaBoundaryPolicy.ProviderRequired, StringComparison.Ordinal);
        var renderManifestIncomplete = string.Equals(stopReason, DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete, StringComparison.Ordinal) ||
            IsTerminalCoverageRetryIncomplete(attempts) ||
            finalEvaluation?.VisualQaBoundaryTrace.MultiResolutionRender.IsComplete == false;
        var budgetCapReached = string.Equals(stopReason, DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, StringComparison.Ordinal);
        var maxAttemptsReached = string.Equals(stopReason, MaxAttemptsReached, StringComparison.Ordinal);
        var cancelled = string.Equals(stopReason, Cancelled, StringComparison.Ordinal);
        var deadlineExceeded = string.Equals(stopReason, DeadlineExceeded, StringComparison.Ordinal);
        var candidateStrategyFailed = string.Equals(stopReason, CandidateStrategyFailed, StringComparison.Ordinal);
        var evaluationFailed = string.Equals(stopReason, EvaluationFailed, StringComparison.Ordinal);
        var noProgressStop = string.Equals(stopReason, NoProgressStop, StringComparison.Ordinal);
        var completed = qualityReached && !cancelled && !deadlineExceeded && !candidateStrategyFailed;
        var lastAttempt = attempts.LastOrDefault();
        var finalPolicy = finalEvaluation?.VisualQaBoundaryTrace.TierPolicy;
        var finalMeasuredScore = finalEvaluation is null ? 0.0 : GetMeasuredQualityScore(finalEvaluation);
        var spentBudgetUsd = attempts.Select(attempt => attempt.EstimatedCostUsd).Sum();
        var coverageRetryAttemptCount = attempts.Count(attempt => IsTruthy(attempt.PublicMetadata, "coverage_retry"));
        var configuredRunDeadlineMs = FormatRunDeadlineMilliseconds(configuredRunDeadline);
        var resultEvidenceRefs = evidenceRefs
            .Concat(
            [
                $"improvement_loop:stop_reason={stopReason}",
                $"improvement_loop:{ConfiguredRunDeadlineMsMetadataKey}={configuredRunDeadlineMs}",
                qualityReached ? "improvement_loop:quality_reached" : "improvement_loop:not_quality_reached",
                completed ? "improvement_loop:completed_quality_reached" : "improvement_loop:not_completed",
                deadlineExceeded ? DeadlineExceededMarker : "improvement_loop:deadline_exceeded_false",
                evaluationFailed ? "improvement_loop:evaluation_failed_bounded" : "improvement_loop:evaluation_failed_false",
                noProgressStop ? "improvement_loop:no_progress_stop" : "improvement_loop:no_progress_stop_false",
                RefsOnlyTelemetryMarker,
                DesignConversionVisualQaBoundaryPolicy.ProductNotReadyMarker
            ])
            .Distinct(StringComparer.Ordinal)
            .OrderBy(reference => reference, StringComparer.Ordinal)
            .ToArray();
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["refs_or_hashes_only"] = bool.TrueString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["screenshot_bytes_included"] = bool.FalseString,
            ["loop_ref"] = HashRef("dcm-loop", request.LoopId),
            ["stop_reason"] = stopReason,
            ["completed"] = completed.ToString(),
            ["quality_reached"] = qualityReached.ToString(),
            ["provider_required_or_blocked"] = providerRequired.ToString(),
            ["render_manifest_incomplete"] = renderManifestIncomplete.ToString(),
            ["budget_cap_reached"] = budgetCapReached.ToString(),
            ["max_attempts_reached"] = maxAttemptsReached.ToString(),
            ["cancelled"] = cancelled.ToString(),
            [DeadlineExceededMetadataKey] = deadlineExceeded.ToString(),
            [ConfiguredRunDeadlineMsMetadataKey] = configuredRunDeadlineMs,
            ["candidate_strategy_failed"] = candidateStrategyFailed.ToString(),
            ["evaluation_failed"] = evaluationFailed.ToString(),
            ["no_progress_stop"] = noProgressStop.ToString(),
            ["attempt_count"] = attempts.Count.ToString(CultureInfo.InvariantCulture),
            ["configured_max_attempts"] = request.MaxAttempts.ToString(CultureInfo.InvariantCulture),
            ["coverage_retry"] = (coverageRetryAttemptCount > 0).ToString(),
            ["coverage_retry_count"] = coverageRetryAttemptCount.ToString(CultureInfo.InvariantCulture),
            ["coverage_retry_stop_source"] = ReadPublicMetadata(lastAttempt, "coverage_retry_stop_source", "none"),
            ["retry_mode"] = ReadPublicMetadata(lastAttempt, "retry_mode", DesignConversionVisualQaBoundaryPolicy.RetryModeNone),
            ["score_direction"] = ScoreDirectionHigherIsBetter,
            ["quality_non_regression"] = ReadPublicMetadata(lastAttempt, "quality_non_regression", bool.FalseString),
            ["quality_non_regression_evidence_mode"] = ReadPublicMetadata(lastAttempt, "quality_non_regression_evidence_mode", "unavailable"),
            ["improvement_epsilon"] = ResolveQualityImprovementEpsilon(request).ToString("0.####", CultureInfo.InvariantCulture),
            ["max_consecutive_no_improvement"] = ResolveMaxConsecutiveNoImprovement(request).ToString(CultureInfo.InvariantCulture),
            ["consecutive_no_improvement"] = ReadPublicMetadata(lastAttempt, "consecutive_no_improvement", "0"),
            ["spent_budget_usd"] = spentBudgetUsd.ToString("0.####", CultureInfo.InvariantCulture),
            ["best_incumbent_candidate_ref"] = ReadPublicMetadata(lastAttempt, "best_incumbent_candidate_ref", HashRef("dcm-loop-candidate", "initial")),
            ["cost_savings_accept_lower_quality"] = bool.FalseString,
            ["tier"] = ReadPublicMetadata(lastAttempt, "tier", "unspecified"),
            ["final_best_score"] = (finalPolicy?.BestScore ?? lastAttempt?.BestScore ?? 0.0).ToString("0.####", CultureInfo.InvariantCulture),
            ["final_measured_quality_score"] = finalMeasuredScore.ToString("0.####", CultureInfo.InvariantCulture),
            ["final_required_score"] = (finalPolicy?.RequiredScore ?? lastAttempt?.RequiredScore ?? 0.0).ToString("0.####", CultureInfo.InvariantCulture),
            ["final_estimated_cost_usd"] = (finalPolicy?.EstimatedCostUsd ?? lastAttempt?.EstimatedCostUsd ?? 0m).ToString("0.####", CultureInfo.InvariantCulture),
            ["final_budget_usd"] = ReadPublicMetadata(lastAttempt, "budget_usd", "0"),
            ["final_remaining_budget_usd"] = finalPolicy is null
                ? ReadPublicMetadata(lastAttempt, "remaining_budget_usd", "0")
                : Math.Max(0m, finalPolicy.BudgetUsd - spentBudgetUsd).ToString("0.####", CultureInfo.InvariantCulture),
            ["effective_budget_usd"] = ReadPublicMetadata(lastAttempt, "effective_budget_usd", "0"),
            ["effective_max_improvement_candidates"] = ReadPublicMetadata(lastAttempt, "effective_max_improvement_candidates", "0"),
            ["submitted_budget_usd"] = ReadPublicMetadata(lastAttempt, "submitted_budget_usd", "0"),
            ["submitted_max_improvement_candidates"] = ReadPublicMetadata(lastAttempt, "submitted_max_improvement_candidates", "0"),
            ["tier_budget_cap_usd"] = ReadPublicMetadata(lastAttempt, "tier_budget_cap_usd", "unbounded"),
            ["tier_max_improvement_candidates"] = ReadPublicMetadata(lastAttempt, "tier_max_improvement_candidates", "unbounded"),
            ["tier_policy_budget_cap_applied"] = ReadPublicMetadata(lastAttempt, "tier_policy_budget_cap_applied", bool.FalseString),
            ["tier_policy_iteration_cap_applied"] = ReadPublicMetadata(lastAttempt, "tier_policy_iteration_cap_applied", bool.FalseString),
            ["tier_policy_budget_entitlement_default_applied"] = ReadPublicMetadata(lastAttempt, "tier_policy_budget_entitlement_default_applied", bool.FalseString),
            ["tier_policy_iteration_entitlement_default_applied"] = ReadPublicMetadata(lastAttempt, "tier_policy_iteration_entitlement_default_applied", bool.FalseString),
            ["budget_stop_source"] = ReadPublicMetadata(lastAttempt, "budget_stop_source", "none"),
            ["render_result_count"] = ReadPublicMetadata(lastAttempt, "render_result_count", "0"),
            ["comparison_artifact_count"] = ReadPublicMetadata(lastAttempt, "comparison_artifact_count", "0"),
            ["visual_judge_result_count"] = ReadPublicMetadata(lastAttempt, "visual_judge_result_count", "0"),
            ["browser_render_executed"] = ReadPublicMetadata(lastAttempt, "browser_render_executed", bool.FalseString),
            ["browser_render_not_executed"] = ReadPublicMetadata(lastAttempt, "browser_render_not_executed", bool.TrueString),
            ["browser_render_result_count"] = ReadPublicMetadata(lastAttempt, "browser_render_result_count", "0"),
            ["external_ai_visual_judge_executed"] = (finalEvaluation?.VisualQaBoundaryTrace.ExternalAiVisualJudgeExecuted ?? false).ToString(),
            ["visual_provider_output_observed"] = ReadPublicMetadata(lastAttempt, "visual_provider_output_observed", bool.FalseString),
            ["external_visual_ai_configured"] = ReadPublicMetadata(lastAttempt, "external_visual_ai_configured", bool.FalseString),
            ["external_visual_ai_executed"] = ReadPublicMetadata(lastAttempt, "external_visual_ai_executed", bool.FalseString),
            ["visual_ai_provider_contract_executed"] = ReadPublicMetadata(lastAttempt, "visual_ai_provider_contract_executed", bool.FalseString),
            ["real_external_provider_executed"] = ReadPublicMetadata(lastAttempt, "real_external_provider_executed", bool.FalseString),
            ["network_call_executed"] = ReadPublicMetadata(lastAttempt, "network_call_executed", bool.FalseString),
            ["fake_client"] = ReadPublicMetadata(lastAttempt, "fake_client", bool.FalseString),
            ["external_visual_ai_blocked"] = ReadPublicMetadata(lastAttempt, "external_visual_ai_blocked", bool.FalseString),
            ["blocked_reason"] = ReadPublicMetadata(lastAttempt, "blocked_reason", "none"),
            ["provider_kind"] = ReadPublicMetadata(lastAttempt, "provider_kind", "none"),
            ["provider_name"] = ReadPublicMetadata(lastAttempt, "provider_name", "none"),
            ["provider_model"] = ReadPublicMetadata(lastAttempt, "provider_model", "none"),
            ["provider_endpoint_host"] = ReadPublicMetadata(lastAttempt, "provider_endpoint_host", "none"),
            ["image_payload_mode"] = ReadPublicMetadata(lastAttempt, "image_payload_mode", "refs_or_hashes_only"),
            ["visual_provider_estimated_cost_usd"] = ReadPublicMetadata(lastAttempt, "visual_provider_estimated_cost_usd", "0"),
            [DesignConversionVisualAiProviderSelectionPolicy.ProviderSelectionPolicyKey] = ReadPublicMetadata(lastAttempt, DesignConversionVisualAiProviderSelectionPolicy.ProviderSelectionPolicyKey, DesignConversionVisualAiProviderSelectionPolicy.PolicyName),
            [DesignConversionVisualAiProviderSelectionPolicy.CandidateProvidersKey] = ReadPublicMetadata(lastAttempt, DesignConversionVisualAiProviderSelectionPolicy.CandidateProvidersKey, DesignConversionVisualAiProviderSelectionPolicy.CandidateProviders),
            [DesignConversionVisualAiProviderSelectionPolicy.RequestedProviderKey] = ReadPublicMetadata(lastAttempt, DesignConversionVisualAiProviderSelectionPolicy.RequestedProviderKey, "auto"),
            [DesignConversionVisualAiProviderSelectionPolicy.RequestedProviderNameKey] = ReadPublicMetadata(lastAttempt, DesignConversionVisualAiProviderSelectionPolicy.RequestedProviderNameKey, "auto"),
            [DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey] = ReadPublicMetadata(lastAttempt, DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey, "none"),
            [DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderNameKey] = ReadPublicMetadata(lastAttempt, DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderNameKey, "none"),
            [DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey] = ReadPublicMetadata(lastAttempt, DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey, "not_selected"),
            [DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey] = ReadPublicMetadata(lastAttempt, DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey, "not_configured"),
            [DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackStatusKey] = ReadPublicMetadata(lastAttempt, DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackStatusKey, "fallback_unavailable"),
            [DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackReasonKey] = ReadPublicMetadata(lastAttempt, DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackReasonKey, "no_visual_provider_artifact"),
            [DesignConversionVisualAiProviderSelectionPolicy.UnavailableProviderCountKey] = ReadPublicMetadata(lastAttempt, DesignConversionVisualAiProviderSelectionPolicy.UnavailableProviderCountKey, "3"),
            [DesignConversionVisualAiProviderSelectionPolicy.UnavailableProvidersKey] = ReadPublicMetadata(lastAttempt, DesignConversionVisualAiProviderSelectionPolicy.UnavailableProvidersKey, DesignConversionVisualAiProviderSelectionPolicy.CandidateProviders),
            [DesignConversionVisualAiProviderSelectionPolicy.RealCloudProviderClaimedKey] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString,
            ["product_ready"] = bool.FalseString,
            ["trained_feature_claimed"] = bool.FalseString
        };

        return new DesignConversionImprovementLoopResult(
            LoopId: request.LoopId,
            StopReason: stopReason,
            Completed: completed,
            QualityReached: qualityReached,
            ProviderRequiredOrBlocked: providerRequired,
            RenderManifestIncomplete: renderManifestIncomplete,
            BudgetCapReached: budgetCapReached,
            MaxAttemptsReached: maxAttemptsReached,
            Cancelled: cancelled,
            DeadlineExceeded: deadlineExceeded,
            CandidateStrategyFailed: candidateStrategyFailed,
            AttemptCount: attempts.Count,
            FinalEvaluation: finalEvaluation,
            Attempts: attempts,
            EvidenceRefs: resultEvidenceRefs,
            PublicMetadata: metadata,
            Warnings: warnings
                .Concat(finalEvaluation?.Warnings ?? [])
                .Distinct(StringComparer.Ordinal)
                .OrderBy(warning => warning, StringComparer.Ordinal)
                .ToArray());
    }

    private static bool IsTerminalCoverageRetryIncomplete(
        IReadOnlyList<DesignConversionImprovementLoopAttemptTelemetry> attempts)
    {
        var terminalAttempt = attempts.LastOrDefault();
        return terminalAttempt is not null &&
            IsTruthy(terminalAttempt.PublicMetadata, "coverage_retry_same_candidate") &&
            !IsTruthy(terminalAttempt.PublicMetadata, "required_viewport_coverage_complete");
    }

    private static IReadOnlyDictionary<string, string> BuildLocalArtifactDbMetadata(
        DesignConversionImprovementLoopRequest request,
        DesignConversionImprovementLoopResult result,
        int dpoCandidateCount,
        IReadOnlyList<string> dpoBuildWarnings)
    {
        var finalEvaluation = result.FinalEvaluation!;
        var lastAttempt = result.Attempts.LastOrDefault();
        var finalCandidateFromLoop = lastAttempt is not null && lastAttempt.AttemptNumber > 1;
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["artifact_db_record_source"] = nameof(DesignConversionImprovementLoopCoordinator),
            [LocalArtifactDbAutowriteMarker] = bool.TrueString,
            [LocalTrainingStoreDpoSourceMarker] = bool.TrueString,
            ["improvement_loop_to_local_training_store"] = bool.TrueString,
            ["local_training_store_dpo_source"] = bool.TrueString,
            ["refs_or_hashes_only"] = bool.TrueString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["screenshot_bytes_included"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString,
            ["loop_ref"] = HashRef("dcm-loop", request.LoopId),
            ["original_request_ref"] = HashRef("dcm-loop-original-request", request.InitialEvaluationRequest.RequestId),
            ["current_request_ref"] = lastAttempt?.RequestRef ?? HashRef("dcm-loop-request", finalEvaluation.SubmittedRequest.RequestId),
            ["final_request_ref"] = HashRef("dcm-loop-final-request", finalEvaluation.SubmittedRequest.RequestId),
            ["final_job_ref"] = HashRef("dcm-loop-final-job", finalEvaluation.JobId),
            ["final_candidate_ref"] = lastAttempt?.CandidateRef ?? HashRef("dcm-loop-candidate", finalEvaluation.SubmittedRequest.RequestId),
            ["final_candidate_from_loop"] = finalCandidateFromLoop.ToString(),
            ["attempt_count"] = result.AttemptCount.ToString(CultureInfo.InvariantCulture),
            ["stop_reason"] = result.StopReason,
            ["quality_reached"] = result.QualityReached.ToString(),
            ["runtime_created_dpo_candidate_count"] = dpoCandidateCount.ToString(CultureInfo.InvariantCulture),
            ["runtime_created_dpo_candidate"] = (dpoCandidateCount > 0).ToString(),
            ["review_status"] = JsonlDesignConversionLocalArtifactDb.RuntimeCandidateReviewPendingStatus,
            ["approved_for_training"] = bool.FalseString,
            ["training_candidate_only"] = bool.TrueString,
            ["product_not_ready"] = bool.TrueString,
            ["production_dpo"] = bool.FalseString,
            ["production_db_ready"] = bool.FalseString,
            ["model_readiness_claimed"] = bool.FalseString
        };

        if (dpoBuildWarnings.Count > 0)
            metadata["runtime_created_dpo_candidate_warnings"] = string.Join("|", dpoBuildWarnings.Distinct(StringComparer.Ordinal));

        return metadata;
    }

    private static DesignConversionImprovementLoopResult MarkLocalArtifactDbPersistence(
        DesignConversionImprovementLoopResult result,
        DesignConversionLocalArtifactDbRecord record,
        int dpoCandidateCount)
    {
        var metadata = new Dictionary<string, string>(result.PublicMetadata, StringComparer.Ordinal)
        {
            [LocalArtifactDbAutowriteMarker] = bool.TrueString,
            [LocalTrainingStoreDpoSourceMarker] = bool.TrueString,
            ["local_artifact_db_record_ref"] = HashRef("dcm-local-artifact-db-record", record.RecordId),
            ["local_artifact_db_job_ref"] = HashRef("dcm-local-artifact-db-job", record.JobId),
            ["local_artifact_db_request_ref"] = HashRef("dcm-local-artifact-db-request", record.RequestId),
            ["runtime_created_dpo_candidate_count"] = dpoCandidateCount.ToString(CultureInfo.InvariantCulture),
            ["runtime_created_dpo_candidate"] = (dpoCandidateCount > 0).ToString(),
            ["training_candidate_only"] = bool.TrueString,
            ["product_not_ready"] = bool.TrueString,
            ["approved_for_training"] = bool.FalseString,
            ["production_dpo"] = bool.FalseString,
            ["production_db_ready"] = bool.FalseString
        };

        return result with
        {
            EvidenceRefs = result.EvidenceRefs
                .Append(LocalArtifactDbAutowriteMarker)
                .Append(LocalTrainingStoreDpoSourceMarker)
                .Append(JsonlDesignConversionLocalArtifactDb.StorageTrainingArtifactDbMarker)
                .Append(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(reference => reference, StringComparer.Ordinal)
                .ToArray(),
            PublicMetadata = metadata,
            Warnings = result.Warnings
                .Append("improvement_loop_local_artifact_db_autowrite=true")
                .Append("improvement_loop_local_training_store_dpo_source=true")
                .Distinct(StringComparer.Ordinal)
                .OrderBy(warning => warning, StringComparer.Ordinal)
                .ToArray()
        };
    }

    private static DesignConversionImprovementLoopResult MarkLocalArtifactDbPersistenceSkippedForDeadline(
        DesignConversionImprovementLoopResult result)
    {
        var metadata = new Dictionary<string, string>(result.PublicMetadata, StringComparer.Ordinal)
        {
            [LocalArtifactDbAutowriteMarker] = bool.FalseString,
            [LocalTrainingStoreDpoSourceMarker] = bool.FalseString,
            [LocalArtifactDbPersistenceSkippedDeadlineExceededMarker] = bool.TrueString,
            ["improvement_loop_to_local_training_store"] = bool.FalseString,
            ["local_training_store_dpo_source"] = bool.FalseString,
            ["runtime_created_dpo_candidate_count"] = "0",
            ["runtime_created_dpo_candidate"] = bool.FalseString
        };

        return result with
        {
            EvidenceRefs = result.EvidenceRefs
                .Append(LocalArtifactDbPersistenceSkippedDeadlineExceededMarker)
                .Append("improvement_loop:local_artifact_db_autowrite=false")
                .Append("improvement_loop:local_training_store_dpo_source=false")
                .Distinct(StringComparer.Ordinal)
                .OrderBy(reference => reference, StringComparer.Ordinal)
                .ToArray(),
            PublicMetadata = metadata,
            Warnings = result.Warnings
                .Append("improvement_loop_local_artifact_db_persistence_skipped_deadline_exceeded=true")
                .Append("improvement_loop_local_artifact_db_autowrite=false")
                .Append("improvement_loop_local_training_store_dpo_source=false")
                .Distinct(StringComparer.Ordinal)
                .OrderBy(warning => warning, StringComparer.Ordinal)
                .ToArray()
        };
    }

    private static DesignConversionImprovementLoopResult MarkLocalArtifactDbPersistenceFailed(
        DesignConversionImprovementLoopResult result,
        string? errorCode,
        int dpoCandidateCount)
    {
        var safeErrorCode = SafePersistenceErrorCode(errorCode);
        var metadata = new Dictionary<string, string>(result.PublicMetadata, StringComparer.Ordinal)
        {
            ["persistence_invoked"] = bool.TrueString,
            [LocalArtifactDbAutowriteMarker] = bool.FalseString,
            [LocalTrainingStoreDpoSourceMarker] = bool.FalseString,
            [LocalArtifactDbPersistenceFailedMarker] = bool.TrueString,
            [LocalArtifactDbPersistenceFailureBoundedMarker] = bool.TrueString,
            ["local_artifact_db_persistence_status"] = "non_durable",
            ["local_artifact_db_persistence_error_code"] = safeErrorCode,
            ["improvement_loop_to_local_training_store"] = bool.FalseString,
            ["local_training_store_dpo_source"] = bool.FalseString,
            ["runtime_created_dpo_candidate_count"] = dpoCandidateCount.ToString(CultureInfo.InvariantCulture),
            ["runtime_created_dpo_candidate"] = (dpoCandidateCount > 0).ToString(),
            ["production_db_ready"] = bool.FalseString
        };

        return result with
        {
            EvidenceRefs = result.EvidenceRefs
                .Append(LocalArtifactDbPersistenceFailedMarker)
                .Append(LocalArtifactDbPersistenceFailureBoundedMarker)
                .Append("improvement_loop:local_artifact_db_persistence_status=non_durable")
                .Append($"improvement_loop:local_artifact_db_persistence_error_code={safeErrorCode}")
                .Append("improvement_loop:local_artifact_db_autowrite=false")
                .Append("improvement_loop:local_training_store_dpo_source=false")
                .Distinct(StringComparer.Ordinal)
                .OrderBy(reference => reference, StringComparer.Ordinal)
                .ToArray(),
            PublicMetadata = metadata,
            Warnings = result.Warnings
                .Append("improvement_loop_local_artifact_db_persistence_status=non_durable")
                .Append($"improvement_loop_local_artifact_db_persistence_error_code={safeErrorCode}")
                .Append("improvement_loop_local_artifact_db_autowrite=false")
                .Append("improvement_loop_local_training_store_dpo_source=false")
                .Distinct(StringComparer.Ordinal)
                .OrderBy(warning => warning, StringComparer.Ordinal)
                .ToArray()
        };
    }

    private static string SafePersistenceErrorCode(string? errorCode)
    {
        var compact = new string((errorCode ?? string.Empty)
            .Where(ch => char.IsLetterOrDigit(ch) || ch is '_' or '-')
            .Take(96)
            .ToArray());

        return string.IsNullOrWhiteSpace(compact)
            ? "LOCAL_ARTIFACT_DB_APPEND_FAILED"
            : compact;
    }

    private static IReadOnlyDictionary<string, string> MergeStringDictionaries(
        IReadOnlyDictionary<string, string>? first,
        IReadOnlyDictionary<string, string> second)
        => (first ?? new Dictionary<string, string>())
            .Concat(second)
            .GroupBy(pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal);

    private static string HashRef(string prefix, string? value)
        => DesignConversionDpoLeakageGuard.HashRef(prefix, value);

    private static IReadOnlyDictionary<string, string> BuildVisualProviderBoundaryMetadata(
        DesignConversionInternalEvaluationResult evaluation)
    {
        var artifacts = evaluation.ComparisonArtifacts;
        var providerArtifacts = artifacts
            .Where(artifact => IsVisualProviderArtifact(artifact.Metadata))
            .ToArray();
        var externalReceiptsValid = DesignConversionExternalVisualAiReceiptValidator.HasValidOriginalExternalReceipts(
            evaluation.ComparisonArtifacts,
            evaluation.VisualJudgeResults,
            evaluation.JobId,
            evaluation.SubmittedRequest.RequestId);
        var untrustedEstimatedCost = providerArtifacts
            .Select(artifact => TryReadDecimal(artifact.Metadata, "estimated_cost_usd"))
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .Sum();
        var artifactClaimsExternalProviderCalled = providerArtifacts.Any(artifact =>
            IsTruthy(artifact.Metadata, "network_call_executed") ||
            IsTruthy(artifact.Metadata, "real_external_provider_executed") ||
            IsTruthy(artifact.Metadata, "external_visual_ai_executed"));
        var artifactClaimsResponseIdentityVerified = providerArtifacts.Any(
            artifact => IsTruthy(artifact.Metadata, "provider_response_identity_verified"));
        var artifactClaimsResponseSchemaVerified = providerArtifacts.Any(
            artifact => IsTruthy(artifact.Metadata, "provider_response_schema_verified"));
        var verifiedExternalVisualAiExecuted = externalReceiptsValid &&
                                               providerArtifacts.Any(artifact => IsTruthy(artifact.Metadata, "external_visual_ai_executed"));
        var verifiedExternalProviderCalled = externalReceiptsValid && artifactClaimsExternalProviderCalled;
        var verifiedRealExternalProviderExecuted = externalReceiptsValid &&
                                                   providerArtifacts.Any(artifact => IsTruthy(artifact.Metadata, "real_external_provider_executed"));
        var verifiedNetworkCallExecuted = externalReceiptsValid &&
                                          providerArtifacts.Any(artifact => IsTruthy(artifact.Metadata, "network_call_executed"));
        var verifiedProviderResponseIdentity = externalReceiptsValid && artifactClaimsResponseIdentityVerified;
        var verifiedProviderResponseSchema = externalReceiptsValid && artifactClaimsResponseSchemaVerified;
        var verifiedEstimatedCost = externalReceiptsValid ? untrustedEstimatedCost : 0m;

        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["visual_provider_output_observed"] = (providerArtifacts.Length > 0).ToString(),
            ["external_visual_ai_configured"] = providerArtifacts.Any(artifact => IsTruthy(artifact.Metadata, "external_visual_ai_configured")).ToString(),
            ["external_visual_ai_executed"] = verifiedExternalVisualAiExecuted.ToString(),
            ["external_provider_called"] = verifiedExternalProviderCalled.ToString(),
            ["visual_ai_provider_contract_executed"] = providerArtifacts.Any(artifact => IsTruthy(artifact.Metadata, "visual_ai_provider_contract_executed")).ToString(),
            ["real_external_provider_executed"] = verifiedRealExternalProviderExecuted.ToString(),
            ["network_call_executed"] = verifiedNetworkCallExecuted.ToString(),
            ["provider_response_identity_verified"] = verifiedProviderResponseIdentity.ToString(),
            ["provider_response_schema_verified"] = verifiedProviderResponseSchema.ToString(),
            ["fake_client"] = providerArtifacts.Any(artifact => IsTruthy(artifact.Metadata, "fake_client")).ToString(),
            ["external_visual_ai_blocked"] = providerArtifacts.Any(artifact => IsTruthy(artifact.Metadata, "external_visual_ai_blocked")).ToString(),
            ["blocked_reason"] = JoinedMetadataValues(providerArtifacts, "blocked_reason", excludeNone: true),
            ["provider_kind"] = JoinedMetadataValues(providerArtifacts, "provider_kind", excludeNone: false),
            ["provider_name"] = JoinedMetadataValues(providerArtifacts, "provider_name", excludeNone: false),
            ["provider_model"] = JoinedMetadataValues(providerArtifacts, "provider_model", excludeNone: false),
            ["provider_endpoint_host"] = JoinedMetadataValues(providerArtifacts, "provider_endpoint_host", excludeNone: false),
            ["image_payload_mode"] = JoinedMetadataValues(providerArtifacts, "image_payload_mode", excludeNone: false, fallback: "refs_or_hashes_only"),
            ["visual_provider_estimated_cost_usd"] = verifiedEstimatedCost.ToString("0.####", CultureInfo.InvariantCulture),
            [DesignConversionVisualAiProviderSelectionPolicy.ProviderSelectionPolicyKey] = JoinedMetadataValues(providerArtifacts, DesignConversionVisualAiProviderSelectionPolicy.ProviderSelectionPolicyKey, excludeNone: false, fallback: DesignConversionVisualAiProviderSelectionPolicy.PolicyName),
            [DesignConversionVisualAiProviderSelectionPolicy.CandidateProvidersKey] = JoinedMetadataValues(providerArtifacts, DesignConversionVisualAiProviderSelectionPolicy.CandidateProvidersKey, excludeNone: false, fallback: DesignConversionVisualAiProviderSelectionPolicy.CandidateProviders),
            [DesignConversionVisualAiProviderSelectionPolicy.RequestedProviderKey] = JoinedMetadataValues(providerArtifacts, DesignConversionVisualAiProviderSelectionPolicy.RequestedProviderKey, excludeNone: false, fallback: "auto"),
            [DesignConversionVisualAiProviderSelectionPolicy.RequestedProviderNameKey] = JoinedMetadataValues(providerArtifacts, DesignConversionVisualAiProviderSelectionPolicy.RequestedProviderNameKey, excludeNone: false, fallback: "auto"),
            [DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey] = JoinedMetadataValues(providerArtifacts, DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey, excludeNone: false, fallback: "none"),
            [DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderNameKey] = JoinedMetadataValues(providerArtifacts, DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderNameKey, excludeNone: false, fallback: "none"),
            [DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey] = JoinedMetadataValues(providerArtifacts, DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey, excludeNone: false, fallback: "not_selected"),
            [DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey] = JoinedMetadataValues(providerArtifacts, DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey, excludeNone: false, fallback: "not_configured"),
            [DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackStatusKey] = JoinedMetadataValues(providerArtifacts, DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackStatusKey, excludeNone: false, fallback: "fallback_unavailable"),
            [DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackReasonKey] = JoinedMetadataValues(providerArtifacts, DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackReasonKey, excludeNone: false, fallback: "no_visual_provider_artifact"),
            [DesignConversionVisualAiProviderSelectionPolicy.UnavailableProviderCountKey] = JoinedMetadataValues(providerArtifacts, DesignConversionVisualAiProviderSelectionPolicy.UnavailableProviderCountKey, excludeNone: false, fallback: "3"),
            [DesignConversionVisualAiProviderSelectionPolicy.UnavailableProvidersKey] = JoinedMetadataValues(providerArtifacts, DesignConversionVisualAiProviderSelectionPolicy.UnavailableProvidersKey, excludeNone: false, fallback: DesignConversionVisualAiProviderSelectionPolicy.CandidateProviders),
            [DesignConversionVisualAiProviderSelectionPolicy.RealCloudProviderClaimedKey] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString
        };
    }

    private static bool IsVisualProviderArtifact(IReadOnlyDictionary<string, string> metadata)
        => metadata.ContainsKey("provider_kind") ||
           metadata.ContainsKey("external_visual_ai_configured") ||
           metadata.ContainsKey("visual_ai_provider_contract_executed");

    private static string JoinedMetadataValues(
        IReadOnlyList<DesignConversionQaComparisonArtifact> artifacts,
        string key,
        bool excludeNone,
        string fallback = "none")
    {
        var redactions = new SortedSet<string>(StringComparer.Ordinal);
        var values = artifacts
            .Select(artifact => artifact.Metadata.TryGetValue(key, out var value) ? value : null)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Where(value => !excludeNone || !string.Equals(value, "none", StringComparison.Ordinal))
            .Select(value => DesignConversionDpoLeakageGuard.SanitizeScalar(
                value,
                $"improvement_loop_visual_provider.{key}",
                redactions))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
        return values.Length == 0 ? fallback : string.Join("|", values);
    }

    private static decimal? TryReadDecimal(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) &&
           decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;

    private static string ReadPublicMetadata(
        DesignConversionImprovementLoopAttemptTelemetry? attempt,
        string key,
        string fallback)
        => attempt is not null && attempt.PublicMetadata.TryGetValue(key, out var value)
            ? value
            : fallback;

    private static string ReadPolicyMetadata(
        DesignConversionTierPolicyDecision policy,
        string key,
        string fallback)
        => policy.Metadata.TryGetValue(key, out var value)
            ? value
            : fallback;

    private static bool IsTruthy(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) &&
           (string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "1", StringComparison.Ordinal) ||
            string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase));
}
