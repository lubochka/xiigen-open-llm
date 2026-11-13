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
using AgentInfrastructure.Logical;
using ContinuousLearning;

public sealed record DesignConversionImprovementLoopRequest(
    string LoopId,
    DesignConversionInternalEvaluateRequest InitialEvaluationRequest,
    int MaxAttempts,
    IReadOnlyDictionary<string, string> Metadata)
{
    /// <summary>
    /// Untrusted caller admission/advisory measurement. This is never visual
    /// evaluation output and never child-DPO held-out accuracy authority.
    /// </summary>
    public DesignConversionMeasurementGateRequest? MeasurementGateRequest
    {
        get;
        init;
    }

    /// <summary>
    /// Carries the canonical repository-owned DPO dataset transaction context for
    /// this request so the coordinator can fail closed before any teacher call.
    /// </summary>
    public DesignConversionRepositoryOwnedDpoTransactionContext?
        RepositoryOwnedDpoTransactionContext
    {
        get;
        init;
    }
}

public sealed record DesignConversionCandidateUpdateRequest(
    string LoopId,
    int CompletedAttemptNumber,
    DesignConversionInternalEvaluateRequest CurrentRequest,
    DesignConversionInternalEvaluationResult CurrentEvaluation,
    DesignConversionTierPolicyDecision TierPolicy,
    IReadOnlyList<DesignConversionImprovementLoopAttemptTelemetry> AttemptHistory)
{
    /// <summary>
    /// Keeps repository-owned DPO authority attached to the candidate update so
    /// teacher authoring never has to infer it from ambient process state.
    /// </summary>
    public DesignConversionRepositoryOwnedDpoTransactionContext?
        RepositoryOwnedDpoTransactionContext
    {
        get;
        init;
    }
}

public interface IDesignConversionImprovementCandidateStrategy
{
    Task<OperationResult<DesignConversionImprovementCandidate>> CreateNextCandidateAsync(
        DesignConversionCandidateUpdateRequest request,
        CancellationToken cancellationToken = default);
}

internal sealed record DesignConversionTrainingIntakePreflightHandoffRequest(
    string LoopId,
    string SourceJobId,
    string SourceRequestId,
    string LocalArtifactRecordId,
    string StableIntakeId,
    string IdempotencyKeyRef,
    string RequestedByRef,
    string Reason,
    DateTimeOffset RequestedAtUtc,
    DesignConversionMeasurementGateRequest MeasurementGateRequest,
    int DpoStepCount,
    IReadOnlyList<string> SourceEvidenceRefs,
    IReadOnlyDictionary<string, string> AdmissionBoundaryMetadata);

internal sealed record DesignConversionTrainingIntakePreflightHandoffResult(
    DesignConversionTrainingIntakeRecord? Intake,
    DesignConversionTrainingIntakePreflightResult? Preflight,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> Warnings,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

internal sealed record DesignConversionTrainingIntakeRunAndPromoteLocalRequest(
    string IntakeId,
    DesignConversionTrainingPipelineOptions TrainingOptions,
    IReadOnlyDictionary<string, string> AdmissionBoundaryMetadata);

internal interface IDesignConversionTrainingIntakePreflightHandoffGateway
{
    Task<OperationResult<DesignConversionTrainingIntakePreflightHandoffResult>> SubmitPreflightAsync(
        DesignConversionTrainingIntakePreflightHandoffRequest request,
        CancellationToken cancellationToken = default);

    Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>> RunTrainingIntakeAndPromoteLocalAsync(
        DesignConversionTrainingIntakeRunAndPromoteLocalRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class DesignConversionTrainingIntakePreflightCoordinatorOptions
{
    public static DesignConversionTrainingIntakePreflightCoordinatorOptions Disabled { get; } = new();

    public DesignConversionTrainingIntakePreflightCoordinatorOptions(
        bool enabled = false,
        int dpoStepCount =
            DesignConversionDpoPolicyCheckpointBridgeOptions.DefaultDpoStepCount)
        : this(
            enabled,
            dpoStepCount,
            runTrainingIntakeAndPromoteLocal: false)
    {
    }

    public DesignConversionTrainingIntakePreflightCoordinatorOptions(
        bool enabled,
        int dpoStepCount,
        bool runTrainingIntakeAndPromoteLocal)
    {
        Enabled = enabled;
        DpoStepCount = Math.Clamp(
            dpoStepCount,
            LogicalDpoTrainingUnit.MinimumSupportedMaxStepCount,
            DesignConversionDpoPolicyCheckpointBridgeOptions.MaxAllowedDpoStepCount);
        RunTrainingIntakeAndPromoteLocal = runTrainingIntakeAndPromoteLocal;
    }

    public bool Enabled { get; }

    public int DpoStepCount { get; }

    public bool RunTrainingIntakeAndPromoteLocal { get; }
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
    public const string LocalArtifactDbDpoSelectedRejectedAttemptMarker = "improvement_loop_local_artifact_db_dpo_selected_rejected_attempt";
    public const string LocalArtifactDbDpoNoEligibleRejectedAttemptMarker = "improvement_loop_local_artifact_db_dpo_no_eligible_rejected_attempt";
    public const string LocalArtifactDbPersistenceSkippedDeadlineExceededMarker = "improvement_loop_local_artifact_db_persistence_skipped_deadline_exceeded";
    public const string LocalArtifactDbPersistenceFailedMarker = "improvement_loop_local_artifact_db_persistence_failed";
    public const string LocalArtifactDbPersistenceFailureBoundedMarker = "improvement_loop_local_artifact_db_persistence_failure_bounded";
    public const string ImprovementLoopHistoryStatusMetadataKey =
        "improvement_loop_history_status";
    public const string ImprovementLoopHistoryAttemptCountMetadataKey =
        "improvement_loop_history_attempt_count";
    public const string ImprovementLoopHistorySha256MetadataKey =
        "improvement_loop_history_sha256";
    public const string ImprovementLoopHistoryOnlyMetadataKey =
        "improvement_loop_history_only";
    public const string ImprovementLoopHistoryOnlyMarker =
        "improvement_loop_history_only";
    public const string TrainingIntakePreflightConfiguredDpoStepCountMetadataKey = "training_intake_preflight_configured_dpo_step_count";
    public const string TrainingIntakePreflightHandoffStatusMetadataKey = "training_intake_preflight_handoff_status";
    public const string TrainingIntakePreflightHandoffAdmittedMetadataKey = "training_intake_preflight_handoff_admitted";
    public const string TrainingIntakePreflightHandoffSkippedNotEligible = "not_eligible";
    public const string TrainingIntakePreflightHandoffMeasurementBlocked = "measurement_admission_blocked";
    public const string TrainingIntakePreflightHandoffGatewayFailed = "gateway_failed";
    public const string TrainingIntakePreflightHandoffGatewayEvidenceMissing = "gateway_evidence_missing";
    public const string TrainingIntakePreflightHandoffPreflightAdmitted = "preflight_admitted";
    public const string TrainingIntakePreflightHandoffPreflightBlocked = "preflight_blocked";
    public const string MaxAttemptsReached = "max_attempts_reached";
    public const string MaxTeacherCallsReached = "max_teacher_calls_reached";
    public const string CandidateStrategyFailed = "candidate_strategy_failed";
    public const string EvaluationFailed = "evaluation_failed_bounded";
    public const string EvaluatorBoundaryExceptionCode =
        "IMPROVEMENT_LOOP_EVALUATOR_BOUNDARY_EXCEPTION";
    public const string CandidateStrategyBoundaryExceptionCode =
        "IMPROVEMENT_LOOP_CANDIDATE_STRATEGY_BOUNDARY_EXCEPTION";
    public const string NoProgressStop = "no_progress_stop";
    public const string Cancelled = "cancelled";
    public const string DeadlineExceeded = "deadline_exceeded";
    public const string DeadlineExceededMarker = "improvement_loop_deadline_exceeded";
    public const string CandidateRepeatedHistoryMarker = "IMPROVEMENT_CANDIDATE_REPEATED_HISTORY";
    public const string DeadlineExceededMetadataKey = "deadline_exceeded";
    public const string ConfiguredRunDeadlineMsMetadataKey = "configured_run_deadline_ms";
    public const string MaxTeacherCallsContractSuppliedMetadataKey = "max_teacher_calls_contract_supplied";
    public const string MaxTeacherCallsContractValueMetadataKey = "max_teacher_calls_contract_value";
    public const string MaxTeacherCallsContractBoundaryMetadataKey = "max_teacher_calls_contract_boundary";
    public const string MaxTeacherCallsRuntimeEnforcementMetadataKey = "max_teacher_calls_runtime_enforcement";
    public const string MaxTeacherCallsCallCapEnforcedMetadataKey = "max_teacher_calls_call_cap_enforced";
    public const string MaxTeacherCallsObservedCallCountMetadataKey = "max_teacher_calls_observed_teacher_call_count";
    public const string MaxTeacherCallsCapReachedMetadataKey = "max_teacher_calls_cap_reached";
    public const string RepositoryOwnedDpoTransactionPreflightMarker =
        "improvement_loop_repository_owned_dpo_transaction_context_preflight";
    public const string RepositoryOwnedDpoTransactionPreflightFailedWarning =
        "IMPROVEMENT_LOOP_REPOSITORY_OWNED_DPO_CONTEXT_INVALID";
    public const string RepositoryOwnedDpoDatasetExistingReusedStop =
        "repository_owned_dpo_dataset_existing_reused";
    public const string RepositoryOwnedDpoDatasetResolutionMarker =
        "improvement_loop_repository_owned_dpo_dataset_resolution";
    public const string RepositoryOwnedDpoDatasetExistingReusedMarker =
        "improvement_loop_repository_owned_dpo_dataset_existing_sealed_reused";
    public const string RepositoryOwnedDpoDatasetCleanMissGenerationAuthorizedMarker =
        "improvement_loop_repository_owned_dpo_dataset_clean_miss_generation_authorized";
    public const string RepositoryOwnedDpoDatasetResolutionFailedWarning =
        "IMPROVEMENT_LOOP_REPOSITORY_OWNED_DPO_DATASET_UNAVAILABLE";
    public const string RepositoryOwnedDpoDatasetPairProjectionFailedErrorCode =
        "IMPROVEMENT_LOOP_REPOSITORY_OWNED_DPO_PAIR_PROJECTION_FAILED";
    public const string RepositoryOwnedDpoDatasetResolutionSourceMetadataKey =
        "repository_owned_dpo_dataset_resolution_source";
    public const string RepositoryOwnedDpoDatasetPairCountMetadataKey =
        "repository_owned_dpo_dataset_pair_count";
    public const string RepositoryOwnedDpoDatasetManifestSha256MetadataKey =
        "repository_owned_dpo_dataset_manifest_sha256";
    public const string RepositoryOwnedDpoDatasetLocalCandidateCreatedMetadataKey =
        "repository_owned_dpo_dataset_local_candidate_created";
    public const string ScoreDirectionHigherIsBetter = "higher_is_better";
    public const int MaxAllowedAttempts = 50;
    public const int DefaultRunDeadlineSeconds = 120;
    public const int MinRunDeadlineMilliseconds = 1;
    public const int MaxRunDeadlineSeconds = 600;

    private const double DefaultQualityImprovementEpsilon = 0.0001;
    private const string MaxAttemptsExceededErrorCode = "IMPROVEMENT_LOOP_MAX_ATTEMPTS_EXCEEDED";
    private const string CandidateNoOpErrorCode = "IMPROVEMENT_CANDIDATE_NO_OP";
    private const string CandidateRepeatedHistoryEvidenceRef = "improvement_loop:candidate_repeated_history";
    internal const string ObservedTeacherCallEvidenceRefPrefix =
        "improvement_loop:max_teacher_call_observed=";
    internal const string TeacherAuthoringFallbackMarker =
        "runtime_teacher_authoring_deterministic_fallback";
    internal const string TeacherAuthoringErrorWarningPrefix =
        "runtime_teacher_authoring_error_code=";
    public const string PostEvaluationStateStoreUnavailableErrorCode =
        "IMPROVEMENT_LOOP_POST_EVALUATION_STATE_STORE_UNAVAILABLE";
    public const string PostEvaluationStateWriteFailedErrorCode =
        "IMPROVEMENT_LOOP_POST_EVALUATION_STATE_WRITE_FAILED";
    public const string PostEvaluationStateCommitFailedErrorCode =
        "IMPROVEMENT_LOOP_POST_EVALUATION_STATE_COMMIT_FAILED";
    public const string PostEvaluationStateQuarantineFailedErrorCode =
        "IMPROVEMENT_LOOP_POST_EVALUATION_STATE_QUARANTINE_FAILED";
    public const string LocalArtifactDbAppendFailedErrorCode =
        "IMPROVEMENT_LOOP_LOCAL_ARTIFACT_DB_APPEND_FAILED";
    public const string LocalArtifactDbReadbackFailedErrorCode =
        "IMPROVEMENT_LOOP_LOCAL_ARTIFACT_DB_READBACK_FAILED";
    public const string LocalArtifactDbReadbackMismatchErrorCode =
        "IMPROVEMENT_LOOP_LOCAL_ARTIFACT_DB_READBACK_MISMATCH";
    public const string AttemptHistoryInvalidErrorCode =
        "IMPROVEMENT_LOOP_ATTEMPT_HISTORY_INVALID";
    public const string TrainingIntakeRunAndPromoteLocalFailedErrorCode =
        "DESIGN_CONVERSION_TRAINING_INTAKE_RUN_AND_PROMOTE_LOCAL_FAILED";
    private const string PersistenceFailureSafeMessage =
        "Improvement-loop persistence did not complete durably.";

    private readonly IDesignConversionThinClientJobEvaluator _evaluationService;
    private readonly IDesignConversionImprovementCandidateStrategy _candidateStrategy;
    private readonly IDesignConversionLocalArtifactDb? _artifactDb;
    private readonly IDesignConversionLocalArtifactDb?
        _independentPreferenceArtifactDb;
    private readonly IDesignConversionPostEvaluationStateStore?
        _postEvaluationStateStore;
    private readonly IDesignConversionRuntimeRejectedEvaluationHandoff?
        _rejectedEvaluationHandoff;
    private readonly IDesignConversionTrainingIntakePreflightHandoffGateway?
        _trainingIntakePreflightGateway;
    private readonly DesignConversionTrainingIntakePreflightCoordinatorOptions
        _trainingIntakePreflightOptions;
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

    private sealed record MeasuredEvaluationAttempt(
        int AttemptNumber,
        string CandidateId,
        DesignConversionInternalEvaluationResult Evaluation,
        CandidateAcceptanceDecision Acceptance,
        TeacherAuthoredCandidateContext? TeacherAuthoring,
        DesignConversionTeacherAuthoredEvaluationPair? TeacherAuthoredPair,
        DesignConversionIndependentPreferenceEvidence?
            IndependentPreferenceEvidence);

    private sealed record DpoPolicyPairRankingAttempt(
        DesignConversionInternalEvaluationResult Evaluation,
        DesignConversionTeacherAuthoredEvaluationPair? TeacherAuthoredPair,
        DesignConversionIndependentPreferenceEvidence?
            IndependentPreferenceEvidence);

    private sealed record TeacherAuthoredCandidateContext(
        string CandidateId,
        DesignConversionInternalEvaluationResult IncumbentEvaluation,
        DesignConversionDpoTeacherCandidateAuthoringEvidence AuthoringEvidence);

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
        TimeSpan? runDeadline = null,
        IDesignConversionPostEvaluationStateStore?
            postEvaluationStateStore = null,
        DesignConversionTrainingIntakePreflightCoordinatorOptions?
            trainingIntakePreflightOptions = null)
        : this(
            new DesignConversionInternalServiceThinClientJobEvaluator(evaluationService),
            candidateStrategy,
            rejectedEvaluationHandoff: null,
            artifactDb,
            utcNow,
            runDeadline,
            postEvaluationStateStore,
            trainingIntakePreflightOptions,
            trainingIntakePreflightGateway: null,
            independentPreferenceArtifactDb: artifactDb)
    {
    }

    public DesignConversionImprovementLoopCoordinator(
        IDesignConversionThinClientJobEvaluator evaluationService,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        IDesignConversionLocalArtifactDb? artifactDb = null,
        Func<DateTimeOffset>? utcNow = null,
        TimeSpan? runDeadline = null,
        IDesignConversionPostEvaluationStateStore?
            postEvaluationStateStore = null,
        DesignConversionTrainingIntakePreflightCoordinatorOptions?
            trainingIntakePreflightOptions = null)
        : this(
            evaluationService,
            candidateStrategy,
            rejectedEvaluationHandoff: null,
            artifactDb,
            utcNow,
            runDeadline,
            postEvaluationStateStore,
            trainingIntakePreflightOptions,
            trainingIntakePreflightGateway: null,
            independentPreferenceArtifactDb: artifactDb)
    {
    }

    internal DesignConversionImprovementLoopCoordinator(
        IDesignConversionThinClientJobEvaluator evaluationService,
        IDesignConversionImprovementCandidateStrategy candidateStrategy,
        IDesignConversionRuntimeRejectedEvaluationHandoff?
            rejectedEvaluationHandoff,
        IDesignConversionLocalArtifactDb? artifactDb = null,
        Func<DateTimeOffset>? utcNow = null,
        TimeSpan? runDeadline = null,
        IDesignConversionPostEvaluationStateStore?
            postEvaluationStateStore = null,
        DesignConversionTrainingIntakePreflightCoordinatorOptions?
            trainingIntakePreflightOptions = null,
        IDesignConversionTrainingIntakePreflightHandoffGateway?
            trainingIntakePreflightGateway = null,
        IDesignConversionLocalArtifactDb?
            independentPreferenceArtifactDb = null)
    {
        _evaluationService = evaluationService ?? throw new ArgumentNullException(nameof(evaluationService));
        ArgumentNullException.ThrowIfNull(candidateStrategy);
        _candidateStrategy = artifactDb is null ||
            candidateStrategy is IDesignConversionTeacherAuthoringCandidateStrategy
            ? candidateStrategy
            : new DesignConversionDpoTeacherAuthoringCandidateStrategy(
                candidateStrategy,
                artifactDb);
        _artifactDb = artifactDb;
        _independentPreferenceArtifactDb =
            independentPreferenceArtifactDb ?? artifactDb;
        _trainingIntakePreflightGateway =
            trainingIntakePreflightGateway ??
            (artifactDb is null
                ? null
                : new LocalArtifactDbTrainingIntakePreflightHandoffGateway(
                    artifactDb));
        _trainingIntakePreflightOptions =
            trainingIntakePreflightOptions ??
            DesignConversionTrainingIntakePreflightCoordinatorOptions.Disabled;
        _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
        _runDeadline = NormalizeRunDeadline(runDeadline);
        _postEvaluationStateStore =
            postEvaluationStateStore ??
            ResolvePostEvaluationStateStore(artifactDb);
        _rejectedEvaluationHandoff = rejectedEvaluationHandoff;
    }

    public async Task<OperationResult<DesignConversionImprovementLoopResult>> RunAsync(
        DesignConversionImprovementLoopRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = Validate(request);
        if (!validation.IsSuccess)
            return validation.MapError<DesignConversionImprovementLoopResult>();

        // Capture caller-owned mutable collections before the first await. A
        // malformed JSON/object graph is reduced to a closed admission state;
        // visual-loop evaluation may still finish, but no training handoff can
        // treat it as measured-training authority.
        var measurementSnapshot = DesignConversionMeasurementAdmission.Capture(
            request.MeasurementGateRequest);
        request = request with
        {
            MeasurementGateRequest = measurementSnapshot.IsSuccess
                ? measurementSnapshot.Value
                : null,

            // A null carrier is intentionally normalized to disabled legacy mode
            // so every later branch has an explicit route state instead of
            // guessing whether repository-owned reuse was requested.
            RepositoryOwnedDpoTransactionContext =
                request.RepositoryOwnedDpoTransactionContext ??
                DesignConversionRepositoryOwnedDpoTransactionContext.Disabled
        };

        // Stamp the root ownership binding before any initial or candidate
        // request is evaluated. BuildNextRequest preserves this provenance for
        // every later loop attempt.
        request = request with
        {
            InitialEvaluationRequest =
                DesignConversionRuntimeRejectedEvaluationHandoff.BindToRootOwnership(
                    request.InitialEvaluationRequest)
        };

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
        var measuredAttempts = ImmutableList<MeasuredEvaluationAttempt>.Empty;
        TeacherAuthoredCandidateContext? pendingTeacherAuthoredCandidate = null;
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
                    cancellationToken,
                    measuredAttempts);
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
                throw;
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    warnings.Add("IMPROVEMENT_LOOP_CANCELLED_DURING_EVALUATION");
                    return await CompleteAsync(
                        request,
                        Cancelled,
                        bestEvaluation,
                        attempts,
                        evidenceRefs,
                        warnings,
                        cancellationToken,
                        measuredAttempts);
                }

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

                warnings.Add(
                    $"IMPROVEMENT_LOOP_EVALUATION_FAILED:{EvaluatorBoundaryExceptionCode}");
                var failedAttempt = BuildFailedAttemptTelemetry(
                    attemptNumber,
                    currentCandidateId,
                    currentRequest,
                    bestEvaluation,
                    EvaluatorBoundaryExceptionCode,
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
                    cancellationToken,
                    measuredAttempts);
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
                        cancellationToken,
                        measuredAttempts);
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
                    cancellationToken,
                    measuredAttempts);
            }

            var candidateEvaluation = evaluation.Value!;
            var measuredTeacherAuthoring = pendingTeacherAuthoredCandidate is not null &&
                                           string.Equals(
                                               pendingTeacherAuthoredCandidate.CandidateId,
                                               currentCandidateId,
                                               StringComparison.Ordinal)
                ? pendingTeacherAuthoredCandidate
                : null;
            var pairRanking = await TryApplyDpoPolicyRuntimePairAsync(
                request.InitialEvaluationRequest,
                attemptNumber,
                currentCandidateId,
                candidateEvaluation,
                measuredTeacherAuthoring,
                warnings,
                evidenceRefs,
                runToken).ConfigureAwait(false);
            candidateEvaluation = pairRanking.Evaluation;
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
            var recordedAcceptance = acceptance with
            {
                IncumbentScoreBefore = incumbentScoreBefore,
                ConsecutiveNoImprovement = consecutiveNoImprovement,
                SpentBudgetUsd = spentBudgetUsd,
                BestCandidateId = bestCandidateId
            };
            var attempt = BuildAttemptTelemetry(
                attemptNumber,
                currentCandidateId,
                currentRequest,
                candidateEvaluation,
                recordedAcceptance,
                isCoverageRetryAttempt,
                coverageRetryCount,
                coverageRetryStopSource);
            attempts = attempts.Add(attempt);
            measuredAttempts = measuredAttempts.Add(new MeasuredEvaluationAttempt(
                attemptNumber,
                currentCandidateId,
                candidateEvaluation,
                recordedAcceptance,
                measuredTeacherAuthoring,
                pairRanking.TeacherAuthoredPair,
                pairRanking.IndependentPreferenceEvidence));
            if (measuredTeacherAuthoring is not null)
                pendingTeacherAuthoredCandidate = null;
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
                        cancellationToken,
                        measuredAttempts);
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
                        cancellationToken,
                        measuredAttempts);
                }

                return await CompleteAsync(
                    request,
                    policy.StopReason,
                    terminalEvaluation,
                    attempts,
                    evidenceRefs,
                    warnings,
                    cancellationToken,
                    measuredAttempts);
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
                    cancellationToken,
                    measuredAttempts);
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
                    cancellationToken,
                    measuredAttempts);
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
                    cancellationToken,
                    measuredAttempts);
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
                    cancellationToken,
                    measuredAttempts);
            }

            var candidateUpdateRequest = new DesignConversionCandidateUpdateRequest(
                LoopId: request.LoopId,
                CompletedAttemptNumber: attemptNumber,
                CurrentRequest: bestRequest,
                CurrentEvaluation: bestEvaluation,
                TierPolicy: bestEvaluation.VisualQaBoundaryTrace.TierPolicy,
                AttemptHistory: attempts)
            {
                // The same request-level context is carried into teacher
                // authoring so the preflight uses the caller's canonical DPO
                // identity, not data reconstructed from a candidate.
                RepositoryOwnedDpoTransactionContext =
                    request.RepositoryOwnedDpoTransactionContext
            };
            OperationResult<DesignConversionImprovementCandidate> candidateResult;
            DesignConversionDpoTeacherCandidateAuthoringEvidence?
                candidateAuthoringEvidence = null;
            try
            {
                if (_candidateStrategy is IDesignConversionTeacherAuthoringCandidateStrategy
                        teacherAuthoringStrategy)
                {
                    var observedTeacherCalls = TryCountObservedTeacherCalls(
                        evidenceRefs,
                        out var trustedObservedTeacherCalls)
                        ? trustedObservedTeacherCalls
                        : -1;
                    var reservation = DesignConversionDpoTeacherCallReservation.Create(
                        request.InitialEvaluationRequest.Budget?.MaxTeacherCalls,
                        observedTeacherCalls);
                    if (reservation.IsSuccess && reservation.Value is not null)
                    {
                        var repositoryOwnedDpoPreflight =
                            ValidateRepositoryOwnedDpoTransactionContext(
                                candidateUpdateRequest
                                    .RepositoryOwnedDpoTransactionContext);
                        if (!repositoryOwnedDpoPreflight.IsSuccess)
                        {
                            // Enabled repository-owned reuse fails closed before
                            // the teacher strategy can issue its first inference,
                            // and before deterministic fallback could mask the
                            // missing or noncanonical transaction authority.
                            warnings.Add(
                                $"{RepositoryOwnedDpoTransactionPreflightFailedWarning}:{repositoryOwnedDpoPreflight.ErrorCode}");
                            evidenceRefs.Add(
                                RepositoryOwnedDpoTransactionPreflightMarker);
                            evidenceRefs.Add(
                                $"improvement_loop:{DesignConversionRepositoryOwnedDpoTransactionContext.ValidationMetadataKey}={bool.FalseString}");
                            return await CompleteAsync(
                                request,
                                CandidateStrategyFailed,
                                bestEvaluation,
                                attempts,
                                evidenceRefs,
                                warnings,
                                cancellationToken,
                                measuredAttempts);
                        }

                        var repositoryOwnedDpoResolution =
                            await ResolveExistingRepositoryOwnedDpoDatasetAsync(
                                candidateUpdateRequest
                                    .RepositoryOwnedDpoTransactionContext,
                                runToken);
                        if (repositoryOwnedDpoResolution is not null)
                        {
                            if (IsCleanMissingRepositoryOwnedDpoDataset(
                                    repositoryOwnedDpoResolution) &&
                                _artifactDb is not null)
                            {
                                // WHY: a clean miss is the only repository-owned
                                // case allowed to reach the teacher. The artifact
                                // DB path below is where rich records are built and
                                // then sealed into the canonical repository
                                // dataset before any local DPO copy can be saved.
                                evidenceRefs.Add(
                                    RepositoryOwnedDpoDatasetResolutionMarker);
                                evidenceRefs.Add(
                                    RepositoryOwnedDpoDatasetCleanMissGenerationAuthorizedMarker);
                                evidenceRefs.Add(
                                    $"improvement_loop:{RepositoryOwnedDpoDatasetResolutionSourceMetadataKey}=clean_missing_generation_authorized");
                                evidenceRefs.Add(
                                    $"improvement_loop:{RepositoryOwnedDpoDatasetLocalCandidateCreatedMetadataKey}={bool.FalseString}");
                            }
                            else
                            {
                                AddRepositoryOwnedDpoDatasetResolutionEvidence(
                                    repositoryOwnedDpoResolution,
                                    evidenceRefs,
                                    warnings);
                                if (!repositoryOwnedDpoResolution.IsSuccess ||
                                    repositoryOwnedDpoResolution.Value is null)
                                {
                                    return CompleteWithoutLocalDpoBuild(
                                        request,
                                        CandidateStrategyFailed,
                                        bestEvaluation,
                                        attempts,
                                        evidenceRefs,
                                        warnings,
                                        repositoryOwnedDpoResolution);
                                }

                                return CompleteWithoutLocalDpoBuild(
                                    request,
                                    RepositoryOwnedDpoDatasetExistingReusedStop,
                                    bestEvaluation,
                                    attempts,
                                    evidenceRefs,
                                    warnings,
                                    repositoryOwnedDpoResolution);
                            }
                        }

                        OperationResult<DesignConversionTeacherAuthoredCandidateResult>
                            authored;
                        try
                        {
                            authored = await teacherAuthoringStrategy
                                .AuthorCandidateAsync(
                                    candidateUpdateRequest,
                                    reservation.Value,
                                    runToken);
                        }
                        finally
                        {
                            if (reservation.Value.CallIssued)
                            {
                                evidenceRefs.RemoveAll(reference => string.Equals(
                                    reference,
                                    NoExternalProviderMarker,
                                    StringComparison.Ordinal));
                                evidenceRefs.Add(
                                    ObservedTeacherCallEvidenceRefPrefix +
                                    reservation.Value.ActualObservedCallCount.ToString(
                                        CultureInfo.InvariantCulture));
                            }
                        }

                        if (authored.IsSuccess && authored.Value is not null)
                        {
                            candidateResult =
                                OperationResult<DesignConversionImprovementCandidate>.Ok(
                                    authored.Value.Candidate);
                            candidateAuthoringEvidence =
                                authored.Value.AuthoringEvidence;
                            if (candidateAuthoringEvidence is not null)
                            {
                                evidenceRefs.Add(
                                    DesignConversionDpoTeacherAuthoringCandidateStrategy
                                        .CandidateAuthoringMarker);
                                evidenceRefs.Add(
                                    $"teacher-authoring:incumbent_ref={candidateAuthoringEvidence.IncumbentRef}");
                                evidenceRefs.Add(
                                    $"teacher-authoring:generated_ref={candidateAuthoringEvidence.GeneratedRef}");
                            }
                        }
                        else
                        {
                            if (!string.Equals(
                                    authored.ErrorCode,
                                    DesignConversionDpoTeacherAuthoringCandidateStrategy
                                        .FallbackRequiredErrorCode,
                                    StringComparison.Ordinal))
                            {
                                AddTeacherAuthoringFallbackEvidence(
                                    warnings,
                                    evidenceRefs,
                                    authored.ErrorCode);
                            }
                            candidateResult = await _candidateStrategy
                                .CreateNextCandidateAsync(
                                    candidateUpdateRequest,
                                    runToken);
                        }
                    }
                    else
                    {
                        AddTeacherAuthoringFallbackEvidence(
                            warnings,
                            evidenceRefs,
                            reservation.ErrorCode);
                        candidateResult = await _candidateStrategy
                            .CreateNextCandidateAsync(candidateUpdateRequest, runToken);
                    }
                }
                else
                {
                    candidateResult = await _candidateStrategy
                        .CreateNextCandidateAsync(candidateUpdateRequest, runToken);
                }
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
                throw;
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    warnings.Add("IMPROVEMENT_LOOP_CANCELLED_DURING_CANDIDATE_STRATEGY");
                    return await CompleteAsync(
                        request,
                        Cancelled,
                        bestEvaluation,
                        attempts,
                        evidenceRefs,
                        warnings,
                        cancellationToken,
                        measuredAttempts);
                }

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

                warnings.Add(
                    $"IMPROVEMENT_LOOP_CANDIDATE_STRATEGY_FAILED:{CandidateStrategyBoundaryExceptionCode}");
                return await CompleteAsync(
                    request,
                    CandidateStrategyFailed,
                    bestEvaluation,
                    attempts,
                    evidenceRefs,
                    warnings,
                    cancellationToken,
                    measuredAttempts);
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
                        cancellationToken,
                        measuredAttempts);
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
                        cancellationToken,
                        measuredAttempts);
                }

                warnings.Add($"IMPROVEMENT_LOOP_CANDIDATE_STRATEGY_FAILED:{strategyErrorCode}");
                return await CompleteAsync(
                    request,
                    CandidateStrategyFailed,
                    bestEvaluation,
                    attempts,
                    evidenceRefs,
                    warnings,
                    cancellationToken,
                    measuredAttempts);
            }

            var candidate = candidateResult.Value!;
            if (candidateAuthoringEvidence is not null &&
                (!string.Equals(
                     candidateAuthoringEvidence.IncumbentBinding,
                     DesignConversionDpoTeacherAttestationRuntime
                         .BuildIncumbentBinding(bestRequest),
                     StringComparison.Ordinal) ||
                 !string.Equals(
                     candidateAuthoringEvidence.GeneratedBinding,
                     DesignConversionDpoTeacherAttestationRuntime
                         .BuildGeneratedBinding(candidate.Html, candidate.Css),
                     StringComparison.Ordinal)))
            {
                warnings.Add(
                    "IMPROVEMENT_LOOP_CANDIDATE_STRATEGY_FAILED:DESIGN_CONVERSION_DPO_TEACHER_AUTHORING_BINDING_MISMATCH");
                return await CompleteAsync(
                    request,
                    CandidateStrategyFailed,
                    bestEvaluation,
                    attempts,
                    evidenceRefs,
                    warnings,
                    cancellationToken,
                    measuredAttempts);
            }
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
                        cancellationToken,
                        measuredAttempts);
                }

                warnings.Add($"IMPROVEMENT_LOOP_CANDIDATE_STRATEGY_FAILED:{validationErrorCode}");
                return await CompleteAsync(
                    request,
                    CandidateStrategyFailed,
                    bestEvaluation,
                    attempts,
                    evidenceRefs,
                    warnings,
                    cancellationToken,
                    measuredAttempts);
            }

            currentCandidateId = candidate.CandidateId;
            if (candidateAuthoringEvidence is not null)
            {
                pendingTeacherAuthoredCandidate = new TeacherAuthoredCandidateContext(
                    candidate.CandidateId,
                    bestEvaluation,
                    candidateAuthoringEvidence);
            }
            currentRequest = BuildNextRequest(bestRequest, rootJobId, candidate, attemptNumber + 1, bestEvaluation);
        }

        return await CompleteAsync(
            request,
            MaxAttemptsReached,
            bestEvaluation,
            attempts,
            evidenceRefs,
            warnings,
            cancellationToken,
            measuredAttempts);
    }

    private async Task<DpoPolicyPairRankingAttempt>
        TryApplyDpoPolicyRuntimePairAsync(
            DesignConversionInternalEvaluateRequest rootRequest,
            int attemptNumber,
            string candidateId,
            DesignConversionInternalEvaluationResult candidateEvaluation,
            TeacherAuthoredCandidateContext? teacherAuthoring,
            ICollection<string> warnings,
            ICollection<string> evidenceRefs,
            CancellationToken cancellationToken)
    {
        if (teacherAuthoring is null)
            return new DpoPolicyPairRankingAttempt(candidateEvaluation, null, null);

        var evaluationPair = new DesignConversionTeacherAuthoredEvaluationPair(
            attemptNumber,
            candidateEvaluation,
            teacherAuthoring.IncumbentEvaluation,
            teacherAuthoring.AuthoringEvidence,
            candidateId);
        if (_independentPreferenceArtifactDb is null)
            return new DpoPolicyPairRankingAttempt(
                candidateEvaluation,
                evaluationPair,
                null);

        var evidenceSource = DesignConversionIndependentPreferenceEvidenceAssociation
            .Resolve(_independentPreferenceArtifactDb);
        if (evidenceSource is null)
            return new DpoPolicyPairRankingAttempt(
                candidateEvaluation,
                evaluationPair,
                null);

        var observedAtUtc = _utcNow();
        try
        {
            if (evidenceSource.CanIssueEvidence)
            {
                var issued = await evidenceSource.IssueEvidenceAsync(
                    evaluationPair,
                    observedAtUtc,
                    cancellationToken).ConfigureAwait(false);
                if (!issued.IsSuccess)
                {
                    warnings.Add(
                        "LOCAL_INDEPENDENT_PREFERENCE_EVALUATION_FAILED:" +
                        SafePersistenceErrorCode(
                            issued.ErrorCode ??
                            DesignConversionLocalIndependentPreferenceEvaluator
                                .EvaluationUnavailableErrorCode));
                }
            }

            if (!evidenceSource.TryConsumeEvidence(
                    evaluationPair,
                    observedAtUtc,
                    out var receipt) ||
                receipt is null)
            {
                return new DpoPolicyPairRankingAttempt(
                    candidateEvaluation,
                    evaluationPair,
                    null);
            }

            if (_evaluationService is not
                IDesignConversionDpoPolicyPairRuntimeEvaluator runtimeEvaluator)
            {
                return new DpoPolicyPairRankingAttempt(
                    candidateEvaluation,
                    evaluationPair,
                    receipt);
            }

            var runtimePair = DesignConversionDpoPolicyRuntimePairFactory.Create(
                rootRequest,
                evaluationPair,
                receipt,
                evidenceSource.ArtifactRootBinding,
                observedAtUtc);
            if (!runtimePair.IsSuccess || runtimePair.Value is null)
            {
                warnings.Add(
                    "DPO_POLICY_RUNTIME_PAIR_UNAVAILABLE:" +
                    SafePersistenceErrorCode(runtimePair.ErrorCode ??
                        DesignConversionMachine
                            .DpoPolicyRuntimePairContentUnavailableCode));
                return new DpoPolicyPairRankingAttempt(
                    candidateEvaluation,
                    evaluationPair,
                    null);
            }

            var ranked = await runtimeEvaluator.ApplyDpoPolicyRuntimePairAsync(
                candidateEvaluation,
                runtimePair.Value,
                cancellationToken).ConfigureAwait(false);
            if (!ranked.IsSuccess || ranked.Value is null)
            {
                warnings.Add(
                    "DPO_POLICY_RUNTIME_PAIR_UNAVAILABLE:" +
                    SafePersistenceErrorCode(ranked.ErrorCode ??
                        DesignConversionMachine
                            .DpoPolicyRuntimePairContentUnavailableCode));
                return new DpoPolicyPairRankingAttempt(
                    candidateEvaluation,
                    evaluationPair,
                    receipt);
            }

            evidenceRefs.Add(
                $"dpo-policy-runtime-pair:{runtimePair.Value.PairBindingSha256}");

            return new DpoPolicyPairRankingAttempt(
                ranked.Value,
                evaluationPair,
                receipt);
        }
        catch (OperationCanceledException)
        {
            warnings.Add("DPO_POLICY_RUNTIME_PAIR_UNAVAILABLE:CANCELLED");
            return new DpoPolicyPairRankingAttempt(
                candidateEvaluation,
                evaluationPair,
                null);
        }
        catch (Exception)
        {
            warnings.Add(
                "DPO_POLICY_RUNTIME_PAIR_UNAVAILABLE:" +
                DesignConversionMachine.DpoPolicyRuntimePairContentUnavailableCode);
            return new DpoPolicyPairRankingAttempt(
                candidateEvaluation,
                evaluationPair,
                null);
        }
    }

    private async Task<OperationResult<DesignConversionImprovementLoopResult>> CompleteAsync(
        DesignConversionImprovementLoopRequest request,
        string stopReason,
        DesignConversionInternalEvaluationResult? finalEvaluation,
        IReadOnlyList<DesignConversionImprovementLoopAttemptTelemetry> attempts,
        IReadOnlyList<string> evidenceRefs,
        IReadOnlyList<string> warnings,
        CancellationToken cancellationToken,
        IReadOnlyList<MeasuredEvaluationAttempt>? measuredAttempts = null)
    {
        var updatedEvidenceRefs = evidenceRefs.ToList();
        var updatedWarnings = warnings.ToList();
        var teacherAuthoredPairs = SelectTeacherAuthoredEvaluationPairs(
            measuredAttempts ?? []);
        var coordinatorConsumedPreferenceEvidence =
            SelectCoordinatorConsumedIndependentPreferenceEvidence(
                measuredAttempts ?? []);
        var rejectedSelection = finalEvaluation is null
            ? null
            : SelectRuntimeRejectedEvaluation(finalEvaluation, measuredAttempts ?? []);
        var result = BuildResult(
            request,
            stopReason,
            finalEvaluation,
            attempts,
            updatedEvidenceRefs,
            updatedWarnings,
            _runDeadline);
        if (_artifactDb is null)
        {
            if (teacherAuthoredPairs.Count > 0 &&
                !cancellationToken.IsCancellationRequested &&
                IsRuntimeRejectedEvaluationHandoffEligibleStop(stopReason))
            {
                var recorded = _rejectedEvaluationHandoff is
                        IDesignConversionTeacherCandidateAuthoringHandoff teacherHandoff &&
                    teacherHandoff.TryRecordTeacherAuthoredEvaluationBatch(
                        request.InitialEvaluationRequest,
                        finalEvaluation ?? teacherAuthoredPairs[0].GeneratedEvaluation,
                        teacherAuthoredPairs,
                        _independentPreferenceArtifactDb is null
                            ? null
                            : coordinatorConsumedPreferenceEvidence,
                        _independentPreferenceArtifactDb is null
                            ? null
                            : updatedWarnings
                                .Where(warning => warning.StartsWith(
                                    "LOCAL_INDEPENDENT_PREFERENCE_EVALUATION_FAILED:",
                                    StringComparison.Ordinal) ||
                                    warning.StartsWith(
                                        "DPO_POLICY_RUNTIME_PAIR_UNAVAILABLE:",
                                        StringComparison.Ordinal))
                                .ToArray());
                if (!recorded)
                    _rejectedEvaluationHandoff?.Discard(request.InitialEvaluationRequest.JobId);
            }
            else if (finalEvaluation is not null &&
                     rejectedSelection is not null &&
                     !cancellationToken.IsCancellationRequested &&
                     IsRuntimeRejectedEvaluationHandoffEligibleStop(stopReason))
            {
                // Legacy deterministic selection remains refs-only telemetry and
                // cannot authorize the teacher-authored generator overload.
                var recorded = _rejectedEvaluationHandoff?.TryRecord(
                    request.InitialEvaluationRequest,
                    finalEvaluation,
                    rejectedSelection.Evaluation) ?? false;
                if (!recorded)
                    _rejectedEvaluationHandoff?.Discard(request.InitialEvaluationRequest.JobId);
            }
            else
            {
                _rejectedEvaluationHandoff?.Discard(request.InitialEvaluationRequest.JobId);
            }

            return OperationResult<DesignConversionImprovementLoopResult>.Ok(result);
        }

        _rejectedEvaluationHandoff?.Discard(request.InitialEvaluationRequest.JobId);
        if (cancellationToken.IsCancellationRequested ||
            string.Equals(stopReason, Cancelled, StringComparison.Ordinal) ||
            attempts.Count == 0)
        {
            return OperationResult<DesignConversionImprovementLoopResult>.Ok(
                result);
        }

        var loopRef = HashRef("dcm-loop", request.LoopId);
        if (!DesignConversionLocalArtifactDbRecord
                .TryBuildImprovementLoopAttemptHistory(
                    loopRef,
                    result.Attempts,
                    out var attemptHistory,
                    out var attemptHistoryJson) ||
            attemptHistory is null ||
            attemptHistoryJson is null)
        {
            return PersistenceFailure(AttemptHistoryInvalidErrorCode);
        }

        if (finalEvaluation is null)
        {
            if (string.Equals(stopReason, EvaluationFailed, StringComparison.Ordinal) &&
                attempts.Count == 1)
            {
                return await PersistHistoryOnlyAsync(
                    request,
                    result,
                    attemptHistory,
                    attemptHistoryJson,
                    cancellationToken);
            }

            return OperationResult<DesignConversionImprovementLoopResult>.Ok(result);
        }

        var createdAtUtc = _utcNow();
        var generator = new DesignConversionDpoGenerator();
        var independentPreferenceWarnings = updatedWarnings
            .Where(warning => warning.StartsWith(
                "LOCAL_INDEPENDENT_PREFERENCE_EVALUATION_FAILED:",
                StringComparison.Ordinal))
            .ToList();
        if (IsEnabledRepositoryOwnedDpoTransaction(
                request.RepositoryOwnedDpoTransactionContext) &&
            teacherAuthoredPairs.Count == 0)
        {
            // WHY: an enabled repository-owned DPO request may finish the visual
            // loop before candidate authoring is reached, especially on a repeat
            // call after the first run already sealed the dataset. In that case
            // the disk dataset is still the authority: reopen it before any local
            // DPO candidate is built, and fail closed if it is missing, corrupt,
            // incomplete or bound to a different request identity.
            var repositoryOwnedDpoResolution =
                await ResolveExistingRepositoryOwnedDpoDatasetAsync(
                        request.RepositoryOwnedDpoTransactionContext,
                        cancellationToken)
                    .ConfigureAwait(false);
            if (repositoryOwnedDpoResolution is not null)
            {
                AddRepositoryOwnedDpoDatasetResolutionEvidence(
                    repositoryOwnedDpoResolution,
                    updatedEvidenceRefs,
                    updatedWarnings);
                if (!repositoryOwnedDpoResolution.IsSuccess ||
                    repositoryOwnedDpoResolution.Value is null)
                {
                    return CompleteWithoutLocalDpoBuild(
                        request,
                        CandidateStrategyFailed,
                        finalEvaluation,
                        attempts,
                        updatedEvidenceRefs,
                        updatedWarnings,
                        repositoryOwnedDpoResolution);
                }

                return CompleteWithoutLocalDpoBuild(
                    request,
                    RepositoryOwnedDpoDatasetExistingReusedStop,
                    finalEvaluation,
                    attempts,
                    updatedEvidenceRefs,
                    updatedWarnings,
                    repositoryOwnedDpoResolution);
            }
        }

        OperationResult<DesignConversionDpoBuildResult> dpoBuild;
        if (teacherAuthoredPairs.Count > 0)
        {
            var evidenceSource = DesignConversionIndependentPreferenceEvidenceAssociation
                .Resolve(_artifactDb);
            if (evidenceSource is null)
            {
                dpoBuild = generator.BuildRuntimeTeacherAuthoredTrainingCandidates(
                    teacherAuthoredPairs,
                    createdAtUtc);
            }
            else
            {
                dpoBuild = generator
                    .BuildRuntimeTeacherAuthoredTrainingCandidates(
                        teacherAuthoredPairs,
                        coordinatorConsumedPreferenceEvidence,
                        evidenceSource.ArtifactRootBinding,
                        createdAtUtc);
            }
        }
        else if (rejectedSelection is null)
        {
            dpoBuild = generator.BuildRuntimeCreatedTrainingCandidates(
                finalEvaluation,
                createdAtUtc);
        }
        else
        {
            dpoBuild = generator.BuildRuntimeCreatedTrainingCandidates(
                finalEvaluation,
                rejectedSelection.Evaluation,
                createdAtUtc);
        }
        IReadOnlyList<DesignConversionDpoTrainingRecord> dpoCandidates = dpoBuild.IsSuccess
            ? dpoBuild.Value!.AcceptedRecords
            : [];
        var dpoWarnings = (dpoBuild.IsSuccess
                ? dpoBuild.Value!.Warnings
                : [dpoBuild.ErrorCode ?? "runtime_created_dpo_candidate_build_failed"])
            .Concat(independentPreferenceWarnings)
            .Append(dpoCandidates.Count > 0
                ? "training_candidate_only=true"
                : "training_candidate_only=false")
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (dpoCandidates.Count > 0 && teacherAuthoredPairs.Count > 0)
        {
            dpoWarnings = dpoWarnings
                .Append(
                    DesignConversionDpoTeacherAuthoringCandidateStrategy
                        .CandidateAuthoringMarker)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
        }
        else if (dpoCandidates.Count > 0 && rejectedSelection is not null)
        {
            dpoWarnings = dpoWarnings
                .Append(LocalArtifactDbDpoSelectedRejectedAttemptMarker)
                .Append($"runtime_rejected_attempt_number={rejectedSelection.AttemptNumber.ToString(CultureInfo.InvariantCulture)}")
                .Append($"runtime_rejected_candidate_ref={HashRef("dcm-loop-candidate", rejectedSelection.CandidateId)}")
                .Append($"runtime_rejected_measured_score={rejectedSelection.MeasuredScore.ToString("0.####", CultureInfo.InvariantCulture)}")
                .Distinct(StringComparer.Ordinal)
                .ToArray();
        }
        else if (dpoCandidates.Count == 0)
        {
            dpoWarnings = dpoWarnings
                .Append(LocalArtifactDbDpoNoEligibleRejectedAttemptMarker)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
        }

        if (dpoBuild.IsSuccess)
        {
            dpoBuild = OperationResult<DesignConversionDpoBuildResult>.Ok(
                dpoBuild.Value! with
                {
                    Warnings = dpoWarnings
                });
        }

        result = DesignConversionTeacherCallUsageReceiptAssociation.CarryForward(
            result,
            result with
            {
                Warnings = result.Warnings
                    .Concat(dpoWarnings)
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray()
            });

        if (IsEnabledRepositoryOwnedDpoTransaction(
                request.RepositoryOwnedDpoTransactionContext))
        {
            var repositoryOwnedDpoResolution =
                await SealRepositoryOwnedDpoDatasetFromAcceptedRecordsAsync(
                    request.RepositoryOwnedDpoTransactionContext!,
                    teacherAuthoredPairs,
                    dpoBuild,
                    cancellationToken);
            AddRepositoryOwnedDpoDatasetResolutionEvidence(
                repositoryOwnedDpoResolution,
                updatedEvidenceRefs,
                updatedWarnings);
            result = DesignConversionTeacherCallUsageReceiptAssociation
                .CarryForward(
                    result,
                    result with
                    {
                        EvidenceRefs = updatedEvidenceRefs
                            .Distinct(StringComparer.Ordinal)
                            .Order(StringComparer.Ordinal)
                            .ToArray(),
                        Warnings = result.Warnings
                            .Concat(updatedWarnings)
                            .Distinct(StringComparer.Ordinal)
                            .Order(StringComparer.Ordinal)
                            .ToArray()
                    });
            if (!repositoryOwnedDpoResolution.IsSuccess ||
                repositoryOwnedDpoResolution.Value is null)
            {
                return CompleteWithoutLocalDpoBuild(
                    request,
                    CandidateStrategyFailed,
                    finalEvaluation,
                    attempts,
                    result.EvidenceRefs,
                    result.Warnings,
                    repositoryOwnedDpoResolution);
            }

            result = MergeRepositoryOwnedDpoDatasetResolutionMetadata(
                result,
                repositoryOwnedDpoResolution);

            // WHY: once the reopened repository-owned dataset is the authority,
            // the local artifact record may keep the evaluation/history audit,
            // but it must not become a second DPO dataset under another root.
            dpoCandidates = [];
        }

        var evaluationForPersistence = finalEvaluation with
        {
            EvidenceRefs = finalEvaluation.EvidenceRefs
                .Concat(result.EvidenceRefs.Where(IsTeacherCallBudgetEvidenceRef))
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray()
        };
        var record = DesignConversionLocalArtifactDbRecord.FromEvaluation(
            evaluationForPersistence,
            dpoCandidates: dpoCandidates,
            createdAtUtc: createdAtUtc,
            metadata: BuildLocalArtifactDbMetadata(
                request,
                result,
                dpoCandidates.Count,
                dpoBuild.IsSuccess
                    ? dpoBuild.Value!.Warnings
                    : [dpoBuild.ErrorCode ?? "runtime_created_dpo_candidate_build_failed"],
                attemptHistory.LoopRef,
                attemptHistoryJson));
        DesignConversionPostEvaluationState? postEvaluationState = null;
        if (dpoCandidates.Count > 0)
        {
            if (_postEvaluationStateStore is null)
            {
                return PersistenceFailure(
                    PostEvaluationStateStoreUnavailableErrorCode);
            }

            OperationResult<DesignConversionPostEvaluationState> preparedState;
            try
            {
                preparedState =
                    await PreparePostEvaluationStateAsync(
                        request,
                        record,
                        createdAtUtc,
                        cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                return PersistenceFailure(
                    PostEvaluationStateWriteFailedErrorCode);
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (!preparedState.IsSuccess)
            {
                return PersistenceFailure(
                    PostEvaluationStateWriteFailedErrorCode);
            }

            postEvaluationState = preparedState.Value!;
            record = MarkPostEvaluationState(record, postEvaluationState.StateRef);
        }

        OperationResult<DesignConversionLocalArtifactDbRecord> append;
        try
        {
            append = await _artifactDb.AppendRecordAsync(
                record,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (DesignConversionCandidatePayloadCustodyCrashException)
        {
            throw;
        }
        catch (Exception)
        {
            append =
                OperationResult<DesignConversionLocalArtifactDbRecord>.Fail(
                    PersistenceFailureSafeMessage,
                    LocalArtifactDbAppendFailedErrorCode);
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (!append.IsSuccess)
        {
            if (postEvaluationState is not null)
            {
                var quarantine = await QuarantinePostEvaluationStateAsync(
                    postEvaluationState,
                    artifactRecordId: string.Empty,
                    append.ErrorCode ??
                    LocalArtifactDbAppendFailedErrorCode);
                if (!quarantine.IsSuccess)
                {
                    return PersistenceFailure(
                        PostEvaluationStateQuarantineFailedErrorCode);
                }
            }

            return PersistenceFailure(
                LocalArtifactDbAppendFailedErrorCode);
        }

        OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>
            readback;
        try
        {
            readback = await _artifactDb.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery(
                    JobId: append.Value!.JobId,
                    RequestId: append.Value.RequestId),
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            readback =
                OperationResult<
                    IReadOnlyList<DesignConversionLocalArtifactDbRecord>>.Fail(
                    PersistenceFailureSafeMessage,
                    LocalArtifactDbReadbackFailedErrorCode);
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (!readback.IsSuccess)
        {
            if (postEvaluationState is not null)
            {
                var quarantine = await QuarantinePostEvaluationStateAsync(
                    postEvaluationState,
                    append.Value!.RecordId,
                    LocalArtifactDbReadbackFailedErrorCode);
                if (!quarantine.IsSuccess)
                {
                    return PersistenceFailure(
                        PostEvaluationStateQuarantineFailedErrorCode);
                }
            }

            return PersistenceFailure(LocalArtifactDbReadbackFailedErrorCode);
        }

        var matchingRecords = (readback.Value ?? [])
            .Where(candidate =>
                IsMatchingDurableArtifactRecord(
                    append.Value!,
                    candidate))
            .ToArray();
        if (matchingRecords.Length != 1 ||
            !HasExactDurableAttemptHistory(
                matchingRecords.SingleOrDefault(),
                attemptHistory,
                attemptHistoryJson))
        {
            if (postEvaluationState is not null)
            {
                var quarantine = await QuarantinePostEvaluationStateAsync(
                    postEvaluationState,
                    append.Value!.RecordId,
                    LocalArtifactDbReadbackMismatchErrorCode);
                if (!quarantine.IsSuccess)
                {
                    return PersistenceFailure(
                        PostEvaluationStateQuarantineFailedErrorCode);
                }
            }

            return PersistenceFailure(LocalArtifactDbReadbackMismatchErrorCode);
        }

        var durableRecord = matchingRecords[0];
        if (postEvaluationState is not null)
        {
            var committedState =
                await CommitPostEvaluationStateAsync(
                    postEvaluationState,
                    durableRecord,
                    cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (!committedState.IsSuccess)
            {
                var quarantine = await QuarantinePostEvaluationStateAsync(
                    postEvaluationState,
                    durableRecord.RecordId,
                    committedState.ErrorCode ??
                    PostEvaluationStateCommitFailedErrorCode);
                return !quarantine.IsSuccess
                    ? PersistenceFailure(
                        PostEvaluationStateQuarantineFailedErrorCode)
                    : PersistenceFailure(
                        PostEvaluationStateCommitFailedErrorCode);
            }
        }

        var persistedResult =
            MarkLocalArtifactDbPersistence(
                result,
                durableRecord,
                dpoCandidates.Count);

        var preflightHandoff =
            await TryCompleteTrainingIntakePreflightHandoffAsync(
                request,
                persistedResult,
                durableRecord,
                dpoCandidates.Count,
                cancellationToken);
        return preflightHandoff;
    }

    private OperationResult<DesignConversionImprovementLoopResult>
        CompleteWithoutLocalDpoBuild(
            DesignConversionImprovementLoopRequest request,
            string stopReason,
            DesignConversionInternalEvaluationResult? finalEvaluation,
            IReadOnlyList<DesignConversionImprovementLoopAttemptTelemetry> attempts,
            IReadOnlyList<string> evidenceRefs,
            IReadOnlyList<string> warnings,
            OperationResult<RepositoryOwnedDpoDatasetResolution>?
                repositoryOwnedDpoResolution)
    {
        // WHY: when the exact repository-owned DPO dataset is already sealed on
        // disk, the loop must not synthesize another local training candidate in
        // the artifact DB. The saved DPO bytes, not a reconstructed candidate,
        // are the authority for the next training slice.
        _rejectedEvaluationHandoff?.Discard(
            request.InitialEvaluationRequest.JobId);
        var result = BuildResult(
            request,
            stopReason,
            finalEvaluation,
            attempts,
            evidenceRefs,
            warnings,
            _runDeadline);
        return OperationResult<DesignConversionImprovementLoopResult>.Ok(
            MergeRepositoryOwnedDpoDatasetResolutionMetadata(
                result,
                repositoryOwnedDpoResolution));
    }

    private async Task<OperationResult<DesignConversionImprovementLoopResult>>
        PersistHistoryOnlyAsync(
            DesignConversionImprovementLoopRequest request,
            DesignConversionImprovementLoopResult result,
            DesignConversionImprovementLoopAttemptHistorySnapshot attemptHistory,
            string attemptHistoryJson,
            CancellationToken cancellationToken)
    {
        var initialRequest = request.InitialEvaluationRequest;
        var createdAtUtc = _utcNow();
        var record = new DesignConversionLocalArtifactDbRecord(
            RecordId:
                $"{initialRequest.JobId}:improvement_loop_history:" +
                HashRef("dcm-loop-history", request.LoopId),
            DatabaseKind: JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind,
            JobId: initialRequest.JobId,
            RequestId: initialRequest.RequestId,
            SourceKind: initialRequest.SourceKind.ToString(),
            Tier: initialRequest.Budget.Tier,
            SanitizedSourceRefs: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["design_tree_payload_ref"] = initialRequest.DesignTreePayload.Sha256,
                ["original_tree_payload_ref"] = initialRequest.OriginalTreePayload.Sha256,
                ["source_manifest_ref"] = initialRequest.Provenance.TryGetValue(
                    "source_manifest_ref",
                    out var sourceManifestRef)
                    ? sourceManifestRef
                    : initialRequest.DesignTreePayload.PayloadId
            },
            ConversionResult: new DesignConversionConversionResultArtifact(
                ResultId: "improvement-loop-history-only",
                HtmlRef: string.Empty,
                HtmlSha256: string.Empty,
                CssRef: string.Empty,
                CssSha256: string.Empty,
                Metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [ImprovementLoopHistoryOnlyMetadataKey] = bool.TrueString,
                    ["refs_or_hashes_only"] = bool.TrueString,
                    ["product_not_ready"] = bool.TrueString
                }),
            RenderArtifactRefs: [],
            VisualJudgeStatus: new DesignConversionVisualJudgeArtifactDbStatus(
                Status: EvaluationFailed,
                ProviderBoundary: "not_recorded",
                ExternalCallExecuted: false,
                EstimatedCostUsd: null,
                TierDecision: EvaluationFailed,
                EvidenceRefs: [ImprovementLoopHistoryOnlyMarker],
                Metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [ImprovementLoopHistoryOnlyMetadataKey] = bool.TrueString,
                    ["external_call_executed"] = bool.FalseString,
                    ["product_not_ready"] = bool.TrueString
                }),
            DpoCandidates: [],
            CreatedAtUtc: createdAtUtc,
            EvidenceRefs: result.EvidenceRefs
                .Append(ImprovementLoopHistoryOnlyMarker)
                .Append("training_grade=false")
                .Append("not_training_grade=true")
                .Append("approved_for_training=false")
                .Append("product_not_ready")
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            Metadata: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["artifact_db_record_source"] =
                    nameof(DesignConversionImprovementLoopCoordinator),
                [LocalArtifactDbAutowriteMarker] = bool.TrueString,
                [ImprovementLoopHistoryOnlyMetadataKey] = bool.TrueString,
                ["refs_or_hashes_only"] = bool.TrueString,
                ["training_candidate_only"] = bool.FalseString,
                ["training_grade"] = bool.FalseString,
                ["not_training_grade"] = bool.TrueString,
                ["approved_for_training"] = bool.FalseString,
                ["review_required"] = bool.FalseString,
                ["approval_required"] = bool.FalseString,
                ["production_dpo"] = bool.FalseString,
                ["product_not_ready"] = bool.TrueString,
                ["model_readiness_claimed"] = bool.FalseString,
                [DesignConversionLocalArtifactDbRecord
                    .ImprovementLoopRefMetadataKey] = attemptHistory.LoopRef,
                [DesignConversionLocalArtifactDbRecord
                    .ImprovementLoopAttemptHistoryMetadataKey] = attemptHistoryJson
            });

        OperationResult<DesignConversionLocalArtifactDbRecord> append;
        try
        {
            append = await _artifactDb!.AppendRecordAsync(
                record,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return PersistenceFailure(LocalArtifactDbAppendFailedErrorCode);
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (!append.IsSuccess)
            return PersistenceFailure(LocalArtifactDbAppendFailedErrorCode);

        OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>
            readback;
        try
        {
            readback = await _artifactDb.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery(
                    JobId: append.Value!.JobId,
                    RequestId: append.Value.RequestId),
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return PersistenceFailure(LocalArtifactDbReadbackFailedErrorCode);
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (!readback.IsSuccess)
            return PersistenceFailure(LocalArtifactDbReadbackFailedErrorCode);

        var matchingRecords = (readback.Value ?? [])
            .Where(candidate =>
                IsMatchingDurableArtifactRecord(append.Value!, candidate))
            .ToArray();
        if (matchingRecords.Length != 1 ||
            !HasExactDurableAttemptHistory(
                matchingRecords.SingleOrDefault(),
                attemptHistory,
                attemptHistoryJson) ||
            !IsHistoryOnlyNonAuthorityRecord(matchingRecords[0]))
        {
            return PersistenceFailure(LocalArtifactDbReadbackMismatchErrorCode);
        }

        return OperationResult<DesignConversionImprovementLoopResult>.Ok(
            MarkLocalArtifactDbPersistence(
                result,
                matchingRecords[0],
                dpoCandidateCount: 0));
    }

    private async Task<OperationResult<DesignConversionImprovementLoopResult>>
        TryCompleteTrainingIntakePreflightHandoffAsync(
            DesignConversionImprovementLoopRequest request,
            DesignConversionImprovementLoopResult result,
            DesignConversionLocalArtifactDbRecord record,
            int dpoCandidateCount,
            CancellationToken cancellationToken)
    {
        if (!_trainingIntakePreflightOptions.Enabled)
            return OperationResult<DesignConversionImprovementLoopResult>.Ok(result);

        var callerAdmissionMeasurement =
            new DesignConversionMeasurementGateEvaluator().Evaluate(
                request.MeasurementGateRequest);
        if (!callerAdmissionMeasurement.Passed)
        {
            return OperationResult<DesignConversionImprovementLoopResult>.Ok(
                MarkTrainingIntakePreflightHandoff(
                    result,
                    TrainingIntakePreflightHandoffMeasurementBlocked,
                    admitted: false,
                    intake: null,
                    preflight: null,
                    dpoStepCount: _trainingIntakePreflightOptions.DpoStepCount,
                    measurementGate: request.MeasurementGateRequest,
                    stableIntakeId: string.Empty,
                    idempotencyKeyRef: string.Empty,
                    gatewayEvidenceRefs: callerAdmissionMeasurement.EvidenceRefs,
                    gatewayWarnings: callerAdmissionMeasurement.Reasons
                        .Append("caller_measurement_admission_advisory_only")
                        .ToArray(),
                    gatewayMetadata: new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["caller_measurement_authority"] = "admission_advisory_only",
                        ["training_handoff_invoked"] = bool.FalseString,
                        ["training_runner_invoked"] = bool.FalseString
                    }));
        }

        if (dpoCandidateCount <= 0 ||
            record.DpoCandidates.Count <= 0 ||
            result.FinalEvaluation is null)
        {
            return OperationResult<DesignConversionImprovementLoopResult>.Ok(
                MarkTrainingIntakePreflightHandoff(
                    result,
                    TrainingIntakePreflightHandoffSkippedNotEligible,
                    admitted: false,
                    intake: null,
                    preflight: null,
                    dpoStepCount: _trainingIntakePreflightOptions.DpoStepCount,
                    measurementGate: null,
                    stableIntakeId: string.Empty,
                    idempotencyKeyRef: string.Empty,
                    gatewayEvidenceRefs: [],
                    gatewayWarnings: ["training_intake_preflight_handoff_not_eligible"],
                    gatewayMetadata: new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["eligible_dpo_candidate_count"] = dpoCandidateCount.ToString(CultureInfo.InvariantCulture)
                    }));
        }

        var handoffRequest =
            BuildTrainingIntakePreflightHandoffRequest(
                request,
                result,
                record);
        OperationResult<DesignConversionTrainingIntakePreflightHandoffResult>
            handoff;
        try
        {
            handoff = await _trainingIntakePreflightGateway!
                .SubmitPreflightAsync(handoffRequest, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            handoff =
                OperationResult<
                    DesignConversionTrainingIntakePreflightHandoffResult>.Fail(
                    "DesignConversion training-intake preflight handoff failed closed.",
                    "DESIGN_CONVERSION_TRAINING_INTAKE_PREFLIGHT_HANDOFF_FAILED");
        }

        if (!handoff.IsSuccess || handoff.Value is null)
        {
            return OperationResult<DesignConversionImprovementLoopResult>.Ok(
                MarkTrainingIntakePreflightHandoff(
                    result,
                    TrainingIntakePreflightHandoffGatewayFailed,
                    admitted: false,
                    intake: null,
                    preflight: null,
                    dpoStepCount: handoffRequest.DpoStepCount,
                    measurementGate: handoffRequest.MeasurementGateRequest,
                    stableIntakeId: handoffRequest.StableIntakeId,
                    idempotencyKeyRef: handoffRequest.IdempotencyKeyRef,
                    gatewayEvidenceRefs: [],
                    gatewayWarnings:
                    [
                        "training_intake_preflight_handoff_failed",
                        handoff.ErrorCode ?? "DESIGN_CONVERSION_TRAINING_INTAKE_PREFLIGHT_HANDOFF_FAILED"
                    ],
                    gatewayMetadata: new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["gateway_error_code"] = handoff.ErrorCode ?? "UNKNOWN"
                    }));
        }

        var gatewayResult = handoff.Value;
        var evidenceBacked =
            gatewayResult.EvidenceRefs.Count > 0 &&
            gatewayResult.Preflight is not null &&
            gatewayResult.Intake is not null &&
            string.Equals(
                gatewayResult.Intake.IntakeId,
                gatewayResult.Preflight.TrainingIntakeId,
                StringComparison.Ordinal);
        var status = !evidenceBacked
            ? TrainingIntakePreflightHandoffGatewayEvidenceMissing
            : gatewayResult.Preflight!.MayStartTraining
                ? TrainingIntakePreflightHandoffPreflightAdmitted
                : TrainingIntakePreflightHandoffPreflightBlocked;
        var admitted = evidenceBacked && gatewayResult.Preflight!.MayStartTraining;

        DesignConversionTrainingIntakeLocalPromotionResult? runAndPromotion = null;
        if (admitted &&
            _trainingIntakePreflightOptions.RunTrainingIntakeAndPromoteLocal)
        {
            OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>
                atomicResult;
            try
            {
                atomicResult = await _trainingIntakePreflightGateway!
                    .RunTrainingIntakeAndPromoteLocalAsync(
                        new DesignConversionTrainingIntakeRunAndPromoteLocalRequest(
                            IntakeId: gatewayResult.Intake!.IntakeId,
                            TrainingOptions:
                                BuildTrainingIntakeRunAndPromoteLocalOptions(
                                    handoffRequest,
                                    gatewayResult.Intake.IntakeId),
                            AdmissionBoundaryMetadata:
                                handoffRequest.AdmissionBoundaryMetadata),
                        cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                atomicResult = OperationResult<
                    DesignConversionTrainingIntakeLocalPromotionResult>.Fail(
                        "DesignConversion training-intake atomic local run and promotion failed closed.",
                        TrainingIntakeRunAndPromoteLocalFailedErrorCode);
            }
            if (!atomicResult.IsSuccess || atomicResult.Value is null)
            {
                return atomicResult
                    .MapError<DesignConversionImprovementLoopResult>();
            }

            runAndPromotion = atomicResult.Value;
        }

        return OperationResult<DesignConversionImprovementLoopResult>.Ok(
            MarkTrainingIntakePreflightHandoff(
                result,
                status,
                admitted,
                gatewayResult.Intake,
                gatewayResult.Preflight,
                handoffRequest.DpoStepCount,
                handoffRequest.MeasurementGateRequest,
                handoffRequest.StableIntakeId,
                handoffRequest.IdempotencyKeyRef,
                gatewayResult.EvidenceRefs,
                gatewayResult.Warnings,
                gatewayResult.BoundaryMetadata,
                runAndPromotion));
    }

    private static DesignConversionTrainingPipelineOptions
        BuildTrainingIntakeRunAndPromoteLocalOptions(
            DesignConversionTrainingIntakePreflightHandoffRequest request,
            string intakeId)
        => new(
            RunId: HashRef(
                "dcm-training-intake-run",
                $"{request.StableIntakeId}|{intakeId}"),
            BatchSize: 1,
            MeasurementGateRequest: request.MeasurementGateRequest,
            DpoPolicyBridgeOptions:
                new DesignConversionDpoPolicyCheckpointBridgeOptions(
                    DpoStepCount: request.DpoStepCount));

    private DesignConversionTrainingIntakePreflightHandoffRequest
        BuildTrainingIntakePreflightHandoffRequest(
            DesignConversionImprovementLoopRequest request,
            DesignConversionImprovementLoopResult result,
            DesignConversionLocalArtifactDbRecord record)
    {
        var dpoStepCount = _trainingIntakePreflightOptions.DpoStepCount;
        var gate = request.MeasurementGateRequest!;
        var measurementContractRef =
            BuildTrainingIntakePreflightMeasurementGateContractRef(gate);
        var stableIntakeId = BuildTrainingIntakePreflightStableIntakeId(
            request,
            record,
            dpoStepCount,
            measurementContractRef);
        var idempotencyKeyRef = HashRef(
            "dcm-training-intake-preflight-idempotency",
            string.Join(
                "|",
                [
                    request.LoopId,
                    record.RecordId,
                    record.JobId,
                    record.RequestId,
                    dpoStepCount.ToString(CultureInfo.InvariantCulture),
                    measurementContractRef
                ]));
        var admissionMetadata =
            BuildTrainingIntakePreflightAdmissionMetadata(
                request,
                record,
                stableIntakeId,
                idempotencyKeyRef,
                gate,
                dpoStepCount);

        return new DesignConversionTrainingIntakePreflightHandoffRequest(
            LoopId: request.LoopId,
            SourceJobId: record.JobId,
            SourceRequestId: record.RequestId,
            LocalArtifactRecordId: record.RecordId,
            StableIntakeId: stableIntakeId,
            IdempotencyKeyRef: idempotencyKeyRef,
            RequestedByRef: HashRef(
                "dcm-training-intake-preflight-requester",
                request.InitialEvaluationRequest.RequestId),
            Reason: HashRef(
                "dcm-training-intake-preflight-reason",
                $"coordinator-preflight:{request.LoopId}:{record.RecordId}"),
            RequestedAtUtc: record.CreatedAtUtc,
            MeasurementGateRequest: gate,
            DpoStepCount: dpoStepCount,
            SourceEvidenceRefs: BuildTrainingIntakePreflightSourceEvidenceRefs(
                result,
                record,
                gate),
            AdmissionBoundaryMetadata: admissionMetadata);
    }

    private static string BuildTrainingIntakePreflightStableIntakeId(
        DesignConversionImprovementLoopRequest request,
        DesignConversionLocalArtifactDbRecord record,
        int dpoStepCount,
        string measurementContractRef)
        => HashRef(
            "dcm-training-intake-preflight-stable-intake",
            string.Join(
                "|",
                [
                    "design-conversion-improvement-loop-training-intake-preflight-v1",
                    request.LoopId,
                    record.RecordId,
                    record.JobId,
                    record.RequestId,
                    dpoStepCount.ToString(CultureInfo.InvariantCulture),
                    measurementContractRef
                ]));

    private static IReadOnlyDictionary<string, string>
        BuildTrainingIntakePreflightAdmissionMetadata(
            DesignConversionImprovementLoopRequest request,
            DesignConversionLocalArtifactDbRecord record,
            string stableIntakeId,
            string idempotencyKeyRef,
            DesignConversionMeasurementGateRequest gate,
            int dpoStepCount)
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["training_intake_tenant_id_ref"] = HashRef(
                "dcm-training-intake-preflight-tenant",
                request.InitialEvaluationRequest.JobId),
            ["training_intake_idempotency_key_ref"] = idempotencyKeyRef,
            ["coordinator_training_intake_preflight_handoff"] = bool.TrueString,
            ["coordinator_loop_ref"] = HashRef("dcm-loop", request.LoopId),
            ["coordinator_local_artifact_record_ref"] = HashRef(
                "dcm-local-artifact-db-record",
                record.RecordId),
            ["coordinator_stable_intake_id"] = stableIntakeId,
            ["coordinator_dpo_step_count"] =
                dpoStepCount.ToString(CultureInfo.InvariantCulture),
            ["coordinator_measurement_gate_id"] =
                DesignConversionMeasurementAdmission.ProjectIdentifierRef(
                    "gate-id",
                    gate.GateId),
            ["coordinator_measurement_gate_unit_id"] =
                DesignConversionMeasurementAdmission.ProjectIdentifierRef(
                    "unit-id",
                    gate.UnitId),
            ["coordinator_measurement_gate_contract_ref"] =
                BuildTrainingIntakePreflightMeasurementGateContractRef(gate),
            ["caller_measurement_authority"] = "admission_advisory_only",
            ["caller_measurement_is_child_dpo_accuracy"] = bool.FalseString,
            ["training_started"] = bool.FalseString,
            ["training_runner_invoked"] = bool.FalseString,
            ["checkpoint_mutated"] = bool.FalseString,
            ["weights_produced"] = bool.FalseString,
            ["model_upload_ready"] = bool.FalseString
        };

    internal static string BuildTrainingIntakePreflightMeasurementGateContractRef(
        DesignConversionMeasurementGateRequest gate)
    {
        var canonical = DesignConversionMeasurementAdmission.BuildCanonicalSha256(
            gate);
        return canonical.IsSuccess && canonical.Value is not null
            ? canonical.Value
            : throw new ArgumentException(
                "A valid caller admission measurement is required.",
                nameof(gate));
    }

    private static IReadOnlyList<string>
        BuildTrainingIntakePreflightSourceEvidenceRefs(
            DesignConversionImprovementLoopResult result,
            DesignConversionLocalArtifactDbRecord record,
            DesignConversionMeasurementGateRequest gate)
        => record.EvidenceRefs
            .Concat(result.EvidenceRefs)
            .Concat(gate.HeldOutMetrics?.EvidenceRefs ?? [])
            .Concat(gate.IntegrityProof?.EvidenceRefs ?? [])
            .Append("coordinator_training_intake_preflight_source_evidence")
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static DesignConversionImprovementLoopResult
        MarkTrainingIntakePreflightHandoff(
            DesignConversionImprovementLoopResult result,
            string status,
            bool admitted,
            DesignConversionTrainingIntakeRecord? intake,
            DesignConversionTrainingIntakePreflightResult? preflight,
            int dpoStepCount,
            DesignConversionMeasurementGateRequest? measurementGate,
            string stableIntakeId,
            string idempotencyKeyRef,
            IReadOnlyList<string> gatewayEvidenceRefs,
            IReadOnlyList<string> gatewayWarnings,
            IReadOnlyDictionary<string, string> gatewayMetadata,
            DesignConversionTrainingIntakeLocalPromotionResult?
                runAndPromotion = null)
    {
        var evidenceRefs = result.EvidenceRefs
            .Concat(gatewayEvidenceRefs)
            .Concat(runAndPromotion?.EvidenceRefs ?? [])
            .Append("training_intake_preflight_handoff")
            .Append($"training_intake_preflight_handoff_status={status}")
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var metadata = new Dictionary<string, string>(
            result.PublicMetadata,
            StringComparer.Ordinal)
        {
            [TrainingIntakePreflightHandoffStatusMetadataKey] = status,
            [TrainingIntakePreflightHandoffAdmittedMetadataKey] =
                admitted.ToString(),
            [TrainingIntakePreflightConfiguredDpoStepCountMetadataKey] =
                dpoStepCount.ToString(CultureInfo.InvariantCulture),
            ["training_intake_preflight_stable_intake_id"] = stableIntakeId,
            ["training_intake_preflight_idempotency_key_ref"] =
                idempotencyKeyRef,
            ["training_intake_preflight_actual_intake_id"] =
                intake?.IntakeId ?? string.Empty,
            ["training_intake_preflight_id"] =
                preflight?.PreflightId ?? string.Empty,
            ["training_intake_preflight_may_start_training"] =
                preflight?.MayStartTraining.ToString() ?? bool.FalseString,
            ["training_intake_preflight_accepted_candidate_count"] =
                preflight?.AcceptedCandidateCount.ToString(
                    CultureInfo.InvariantCulture) ?? "0",
            ["training_intake_preflight_measurement_gate_id"] =
                measurementGate is null
                    ? string.Empty
                    : DesignConversionMeasurementAdmission.ProjectIdentifierRef(
                        "gate-id",
                        measurementGate.GateId),
            ["training_intake_preflight_measurement_gate_unit_id"] =
                measurementGate is null
                    ? string.Empty
                    : DesignConversionMeasurementAdmission.ProjectIdentifierRef(
                        "unit-id",
                        measurementGate.UnitId),
            ["training_runner_invoked"] = bool.FalseString,
            ["training_started"] = bool.FalseString,
            ["checkpoint_mutated"] = bool.FalseString,
            ["weights_produced"] = bool.FalseString,
            ["model_upload_ready"] = bool.FalseString
        };
        foreach (var pair in gatewayMetadata)
        {
            if (!string.IsNullOrWhiteSpace(pair.Key))
                metadata[$"training_intake_preflight_gateway_{pair.Key}"] =
                    pair.Value ?? string.Empty;
        }
        if (runAndPromotion is not null)
        {
            metadata["training_intake_atomic_run_promote_invoked"] =
                bool.TrueString;
            metadata["training_runner_invoked"] =
                runAndPromotion.TrainingRun.TrainingRunnerInvoked.ToString();
            metadata["training_started"] =
                runAndPromotion.TrainingRun.TrainingStarted.ToString();
            metadata["checkpoint_mutated"] =
                runAndPromotion.TrainingRun.CheckpointMutated.ToString();
            metadata["weights_produced"] =
                runAndPromotion.TrainingRun.WeightsProduced.ToString();
            metadata["model_upload_ready"] =
                runAndPromotion.TrainingRun.ModelUploadReady.ToString();
            metadata["training_intake_run_id"] =
                runAndPromotion.TrainingRun.RunId;
            metadata["training_intake_run_status"] =
                runAndPromotion.TrainingRun.TrainingExecutionStatus;
            metadata["training_intake_local_promotion_ready"] =
                runAndPromotion.LocalPromotionReady.ToString();
            metadata["training_intake_external_cloud_upload_attempted"] =
                runAndPromotion.ExternalCloudUploadAttempted.ToString();
        }

        return DesignConversionTeacherCallUsageReceiptAssociation.CarryForward(
            result,
            result with
            {
                EvidenceRefs = evidenceRefs,
                PublicMetadata = metadata,
                Warnings = result.Warnings
                    .Concat(gatewayWarnings)
                    .Concat(runAndPromotion?.Warnings ?? [])
                    .Concat(runAndPromotion?.BlockReasons ?? [])
                    .Append($"training_intake_preflight_handoff_status:{status}")
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray()
            });
    }

    private static IDesignConversionPostEvaluationStateStore?
        ResolvePostEvaluationStateStore(
            IDesignConversionLocalArtifactDb? artifactDb)
        => artifactDb is IDesignConversionPostEvaluationStateRootProvider
            rootProvider
            ? new JsonDesignConversionPostEvaluationStateStore(
                Path.GetFullPath(
                    rootProvider.PostEvaluationStateRootDirectory))
            : null;

    private async Task<OperationResult<DesignConversionPostEvaluationState>>
        PreparePostEvaluationStateAsync(
            DesignConversionImprovementLoopRequest request,
            DesignConversionLocalArtifactDbRecord record,
            DateTimeOffset createdAtUtc,
            CancellationToken cancellationToken)
    {
        var stateRef =
            JsonDesignConversionPostEvaluationStateStore.BuildStateRef(
                string.Join(
                    "|",
                    [
                        "improvement-loop",
                        request.LoopId,
                        record.RecordId
                    ]));
        var state = new DesignConversionPostEvaluationState(
            StateRef: stateRef,
            ReservationRef: stateRef,
            UserRef:
                JsonDesignConversionPostEvaluationStateStore
                    .NoReservationUserRef,
            PeriodStartUtc:
                JsonDesignConversionPostEvaluationStateStore
                    .NoReservationPeriodStartUtc,
            PeriodEndExclusiveUtc:
                JsonDesignConversionPostEvaluationStateStore
                    .NoReservationPeriodEndExclusiveUtc,
            ChargedCostUsd: 0m,
            AccountingStatus:
                JsonDesignConversionPostEvaluationStateStore
                    .AccountingSettled,
            AccountingDesiredAction:
                JsonDesignConversionPostEvaluationStateStore.DesiredNone,
            SettlementAttempted: false,
            SettlementDefinitivelyNotApplied: false,
            ReleaseAttempted: false,
            PersistenceStatus:
                JsonDesignConversionPostEvaluationStateStore
                    .PersistenceProvisional,
            LedgerRecordId: string.Empty,
            ArtifactRecordId: string.Empty,
            CompletionBindingKind: string.Empty,
            CompletionBindingRef: string.Empty,
            LastErrorCode: string.Empty,
            UpdatedAtUtc: createdAtUtc,
            Version: 1);
        var write = await _postEvaluationStateStore!.WriteAsync(
            state,
            cancellationToken);
        return write.IsSuccess
            ? write
            : OperationResult<DesignConversionPostEvaluationState>.Fail(
                "Improvement-loop post-evaluation state could not be durably prepared.",
                PostEvaluationStateWriteFailedErrorCode);
    }

    private async Task<OperationResult<DesignConversionPostEvaluationState>>
        CommitPostEvaluationStateAsync(
            DesignConversionPostEvaluationState state,
            DesignConversionLocalArtifactDbRecord record,
            CancellationToken cancellationToken)
    {
        var denyBeforeCommit =
            await _postEvaluationStateStore!
                .DenyTrainingEligibilityAsync(
                    state.StateRef,
                    record.RecordId,
                    PostEvaluationStateCommitFailedErrorCode,
                    cancellationToken);
        if (!denyBeforeCommit.IsSuccess)
        {
            return OperationResult<DesignConversionPostEvaluationState>.Fail(
                PersistenceFailureSafeMessage,
                PostEvaluationStateCommitFailedErrorCode);
        }

        OperationResult<DesignConversionPostEvaluationState> write;
        try
        {
            write = await _postEvaluationStateStore!.WriteAsync(
                state with
                {
                    PersistenceStatus =
                        JsonDesignConversionPostEvaluationStateStore
                            .PersistenceCommitted,
                    LedgerRecordId = string.Empty,
                    ArtifactRecordId = record.RecordId,
                    CompletionBindingKind =
                        JsonDesignConversionPostEvaluationStateStore
                            .CompletionBindingLocalArtifactDbRecord,
                    CompletionBindingRef = record.RecordId,
                    LastErrorCode = string.Empty,
                    UpdatedAtUtc = _utcNow(),
                    Version = state.Version
                },
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return OperationResult<DesignConversionPostEvaluationState>.Fail(
                PersistenceFailureSafeMessage,
                PostEvaluationStateCommitFailedErrorCode);
        }

        if (!write.IsSuccess)
        {
            return OperationResult<DesignConversionPostEvaluationState>.Fail(
                PersistenceFailureSafeMessage,
                PostEvaluationStateCommitFailedErrorCode);
        }

        OperationResult<DesignConversionPostEvaluationState?> readback;
        try
        {
            readback = await _postEvaluationStateStore.ReadAsync(
                state.StateRef,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return OperationResult<DesignConversionPostEvaluationState>.Fail(
                PersistenceFailureSafeMessage,
                PostEvaluationStateCommitFailedErrorCode);
        }

        var committed = readback.Value;
        if (!readback.IsSuccess ||
            committed is null ||
            !string.Equals(
                committed.PersistenceStatus,
                JsonDesignConversionPostEvaluationStateStore
                    .PersistenceCommitted,
                StringComparison.Ordinal) ||
            !string.Equals(
                committed.ArtifactRecordId,
                record.RecordId,
                StringComparison.Ordinal) ||
            !string.Equals(
                committed.CompletionBindingKind,
                JsonDesignConversionPostEvaluationStateStore
                    .CompletionBindingLocalArtifactDbRecord,
                StringComparison.Ordinal) ||
            !string.Equals(
                committed.CompletionBindingRef,
                record.RecordId,
                StringComparison.Ordinal))
        {
            return OperationResult<DesignConversionPostEvaluationState>.Fail(
                PersistenceFailureSafeMessage,
                PostEvaluationStateCommitFailedErrorCode);
        }

        var release =
            await _postEvaluationStateStore
                .ReleaseTrainingEligibilityDenialAsync(
                    state.StateRef,
                    record.RecordId,
                    cancellationToken);
        return release.IsSuccess
            ? OperationResult<DesignConversionPostEvaluationState>.Ok(
                committed)
            : OperationResult<DesignConversionPostEvaluationState>.Fail(
                PersistenceFailureSafeMessage,
                PostEvaluationStateCommitFailedErrorCode);
    }

    private async Task<OperationResult<DesignConversionPostEvaluationState>>
        QuarantinePostEvaluationStateAsync(
        DesignConversionPostEvaluationState state,
        string artifactRecordId,
        string errorCode)
    {
        if (_postEvaluationStateStore is null)
        {
            return OperationResult<DesignConversionPostEvaluationState>.Fail(
                PersistenceFailureSafeMessage,
                PostEvaluationStateStoreUnavailableErrorCode);
        }

        try
        {
            var read = await _postEvaluationStateStore.ReadAsync(
                state.StateRef,
                CancellationToken.None);
            if (!read.IsSuccess || read.Value is null)
            {
                return OperationResult<DesignConversionPostEvaluationState>.Fail(
                    PersistenceFailureSafeMessage,
                    PostEvaluationStateQuarantineFailedErrorCode);
            }

            var latest = read.Value;
            if (string.Equals(
                    latest.PersistenceStatus,
                    JsonDesignConversionPostEvaluationStateStore
                        .PersistenceCommitted,
                    StringComparison.Ordinal))
            {
                return await _postEvaluationStateStore
                    .DenyTrainingEligibilityAsync(
                        latest.StateRef,
                        artifactRecordId,
                        SafePersistenceErrorCode(errorCode),
                        CancellationToken.None);
            }

            var write = await _postEvaluationStateStore.WriteAsync(
                latest with
                {
                    PersistenceStatus =
                        JsonDesignConversionPostEvaluationStateStore
                            .PersistenceQuarantined,
                    ArtifactRecordId = artifactRecordId,
                    CompletionBindingKind =
                        string.IsNullOrWhiteSpace(artifactRecordId)
                            ? string.Empty
                            : JsonDesignConversionPostEvaluationStateStore
                                .CompletionBindingLocalArtifactDbRecord,
                    CompletionBindingRef = artifactRecordId,
                    LastErrorCode = SafePersistenceErrorCode(errorCode),
                    UpdatedAtUtc = _utcNow(),
                    Version = latest.Version
                },
                CancellationToken.None);
            return write.IsSuccess
                ? write
                : OperationResult<
                    DesignConversionPostEvaluationState>.Fail(
                    PersistenceFailureSafeMessage,
                    PostEvaluationStateQuarantineFailedErrorCode);
        }
        catch (Exception)
        {
            return OperationResult<DesignConversionPostEvaluationState>.Fail(
                PersistenceFailureSafeMessage,
                PostEvaluationStateQuarantineFailedErrorCode);
        }
    }

    private static bool IsMatchingDurableArtifactRecord(
        DesignConversionLocalArtifactDbRecord expected,
        DesignConversionLocalArtifactDbRecord candidate)
        =>
            string.Equals(
                candidate.DatabaseKind,
                JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind,
                StringComparison.Ordinal) &&
            string.Equals(
                candidate.RecordId,
                expected.RecordId,
                StringComparison.Ordinal) &&
            string.Equals(
                candidate.JobId,
                expected.JobId,
                StringComparison.Ordinal) &&
            string.Equals(
                candidate.RequestId,
                expected.RequestId,
                StringComparison.Ordinal);

    private static bool HasExactDurableAttemptHistory(
        DesignConversionLocalArtifactDbRecord? record,
        DesignConversionImprovementLoopAttemptHistorySnapshot expectedHistory,
        string expectedCanonicalJson)
    {
        if (record?.Metadata is null ||
            !record.Metadata.TryGetValue(
                DesignConversionLocalArtifactDbRecord
                    .ImprovementLoopAttemptHistoryMetadataKey,
                out var durableJson) ||
            !record.Metadata.TryGetValue(
                DesignConversionLocalArtifactDbRecord
                    .ImprovementLoopRefMetadataKey,
                out var durableOuterLoopRef) ||
            !string.Equals(
                durableJson,
                expectedCanonicalJson,
                StringComparison.Ordinal) ||
            !record.TryGetImprovementLoopAttemptHistory(out var durableHistory) ||
            durableHistory is null)
        {
            return false;
        }

        return durableHistory.AttemptCount == expectedHistory.AttemptCount &&
               string.Equals(
                   durableOuterLoopRef,
                   expectedHistory.LoopRef,
                   StringComparison.Ordinal) &&
               string.Equals(
                   durableHistory.LoopRef,
                   expectedHistory.LoopRef,
                   StringComparison.Ordinal) &&
               string.Equals(
                   durableHistory.HistorySha256,
                   expectedHistory.HistorySha256,
                   StringComparison.Ordinal);
    }

    private static bool IsHistoryOnlyNonAuthorityRecord(
        DesignConversionLocalArtifactDbRecord record)
        => record.DpoCandidates.Count == 0 &&
           record.Metadata.TryGetValue(
               ImprovementLoopHistoryOnlyMetadataKey,
               out var historyOnly) &&
           string.Equals(
               historyOnly,
               bool.TrueString,
               StringComparison.OrdinalIgnoreCase) &&
           !IsTruthy(record.Metadata, "training_grade") &&
           !IsTruthy(record.Metadata, "approved_for_training") &&
           IsTruthy(record.Metadata, "product_not_ready");

    private static OperationResult<DesignConversionImprovementLoopResult>
        PersistenceFailure(string errorCode)
        => OperationResult<DesignConversionImprovementLoopResult>.Fail(
            PersistenceFailureSafeMessage,
            SafePersistenceErrorCode(errorCode));

    private static DesignConversionLocalArtifactDbRecord
        MarkPostEvaluationState(
            DesignConversionLocalArtifactDbRecord record,
            string stateRef)
        => record with
        {
            Metadata = new Dictionary<string, string>(
                record.Metadata,
                StringComparer.Ordinal)
            {
                [JsonDesignConversionPostEvaluationStateStore
                    .StateRefMetadataKey] = stateRef,
                [JsonDesignConversionPostEvaluationStateStore
                    .PersistenceStatusMetadataKey] =
                    JsonDesignConversionPostEvaluationStateStore
                        .PersistenceProvisional,
                [JsonDesignConversionPostEvaluationStateStore
                    .TrainingEligibleMetadataKey] = bool.FalseString
            }
        };

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
        _rejectedEvaluationHandoff?.Discard(request.InitialEvaluationRequest.JobId);
        var result = BuildResult(request, DeadlineExceeded, finalEvaluation, attempts, evidenceRefs, warnings, _runDeadline);
        return Task.FromResult(OperationResult<DesignConversionImprovementLoopResult>.Ok(
            MarkLocalArtifactDbPersistenceSkippedForDeadline(result)));
    }

    private static bool IsInternalDeadlineExceeded(
        CancellationTokenSource deadline,
        CancellationToken callerCancellationToken)
        => deadline.IsCancellationRequested && !callerCancellationToken.IsCancellationRequested;

    private static bool IsRuntimeRejectedEvaluationHandoffEligibleStop(string stopReason)
        => !string.Equals(stopReason, Cancelled, StringComparison.Ordinal) &&
           !string.Equals(stopReason, DeadlineExceeded, StringComparison.Ordinal) &&
           !string.Equals(stopReason, EvaluationFailed, StringComparison.Ordinal) &&
           !string.Equals(stopReason, CandidateStrategyFailed, StringComparison.Ordinal);

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

    private static OperationResult<bool> ValidateRepositoryOwnedDpoTransactionContext(
        DesignConversionRepositoryOwnedDpoTransactionContext? context)
    {
        // Disabled or absent context is the documented legacy route, so only an
        // explicitly enabled repository-owned route is required to prove a
        // canonical address and request identity before teacher work starts.
        if (context is null || !context.Enabled)
            return OperationResult<bool>.Ok(true);

        // Validation delegates to the repository-owned layout and manifest
        // authorities so the coordinator does not duplicate address or identity
        // rules outside their owning primitives.
        return context.Validate();
    }

    private static async Task<OperationResult<RepositoryOwnedDpoDatasetResolution>?>
        ResolveExistingRepositoryOwnedDpoDatasetAsync(
            DesignConversionRepositoryOwnedDpoTransactionContext? context,
            CancellationToken cancellationToken)
    {
        var normalized =
            context ?? DesignConversionRepositoryOwnedDpoTransactionContext
                .Disabled;
        if (!normalized.Enabled)
            return null;

        var validation = normalized.Validate();
        if (!validation.IsSuccess)
            return validation.MapError<RepositoryOwnedDpoDatasetResolution>();

        // WHY: the first probe is deliberately disk-first and creator-free.
        // An exact sealed hit must return before any teacher call. A clean
        // missing result is only an authorization signal for the later
        // teacher-once branch; every other miss, mismatch, partial write, or
        // corruption stays fail-closed before teacher inference.
        var resolution =
            await RepositoryOwnedDpoDatasetResolver
                .OpenExistingOrCreateAndSealAsync(
                    normalized.Address!,
                    normalized.RequestIdentity!,
                    createPairsAsync: null,
                    cancellationToken)
                .ConfigureAwait(false);
        if (!resolution.IsSuccess || resolution.Value is null)
            return resolution;

        if (resolution.Value.Source ==
            RepositoryOwnedDpoDatasetResolutionSource
                .ExistingSealedRepositoryDataset)
        {
            return resolution;
        }

        return OperationResult<RepositoryOwnedDpoDatasetResolution>.Fail(
            "Repository-owned DPO dataset must already be sealed before DesignConversion teacher inference.",
            RepositoryOwnedDpoDatasetResolver.InvalidErrorCode);
    }

    private static bool IsCleanMissingRepositoryOwnedDpoDataset(
        OperationResult<RepositoryOwnedDpoDatasetResolution>? resolution)
        => resolution is { IsSuccess: false } &&
           string.Equals(
               resolution.ErrorCode,
               RepositoryOwnedDpoDatasetResolver.MissingErrorCode,
               StringComparison.Ordinal);

    private static bool IsEnabledRepositoryOwnedDpoTransaction(
        DesignConversionRepositoryOwnedDpoTransactionContext? context)
        => context is { Enabled: true };

    private static async Task<OperationResult<RepositoryOwnedDpoDatasetResolution>>
        SealRepositoryOwnedDpoDatasetFromAcceptedRecordsAsync(
            DesignConversionRepositoryOwnedDpoTransactionContext context,
            IReadOnlyList<DesignConversionTeacherAuthoredEvaluationPair>
                teacherAuthoredPairs,
            OperationResult<DesignConversionDpoBuildResult> dpoBuild,
            CancellationToken cancellationToken)
    {
        var validation = context.Validate();
        if (!validation.IsSuccess)
            return validation.MapError<RepositoryOwnedDpoDatasetResolution>();
        if (context.Address is null || context.RequestIdentity is null)
        {
            return OperationResult<RepositoryOwnedDpoDatasetResolution>.Fail(
                DesignConversionRepositoryOwnedDpoTransactionContext
                    .InvalidSafeMessage,
                DesignConversionRepositoryOwnedDpoTransactionContext
                    .InvalidErrorCode);
        }

        if (teacherAuthoredPairs.Count == 0)
        {
            return OperationResult<RepositoryOwnedDpoDatasetResolution>.Fail(
                "Repository-owned DPO creation requires a teacher-authored pair from this improvement loop.",
                RepositoryOwnedDpoDatasetPairProjectionFailedErrorCode);
        }

        if (!dpoBuild.IsSuccess || dpoBuild.Value is null)
            return dpoBuild.MapError<RepositoryOwnedDpoDatasetResolution>();

        var acceptedRecords = dpoBuild.Value.AcceptedRecords;
        if (acceptedRecords.Count == 0)
        {
            return OperationResult<RepositoryOwnedDpoDatasetResolution>.Fail(
                "Repository-owned DPO creation produced no trainable records to seal.",
                RepositoryOwnedDpoDatasetPairProjectionFailedErrorCode);
        }

        var pairs = new List<DpoPair>(acceptedRecords.Count);
        for (var index = 0; index < acceptedRecords.Count; index++)
        {
            var record = acceptedRecords[index];
            var repositoryRecord =
                BindRepositoryOwnedDpoChildTeacherRequestId(
                    context,
                    record,
                    index);
            if (!repositoryRecord.IsSuccess ||
                repositoryRecord.Value is null)
            {
                return OperationResult<RepositoryOwnedDpoDatasetResolution>.Fail(
                    repositoryRecord.ErrorMessage,
                    RepositoryOwnedDpoDatasetPairProjectionFailedErrorCode);
            }

            // WHY: the sealed repository dataset is addressed by a public
            // domain slug such as figma-html-css. The pair must carry that same
            // domain so a later fresh open proves these exact rows belong to
            // this exact dataset instead of to the legacy local machine tag.
            var projected =
                DesignConversionTrainingIntakeDpoPairAuthority.Project(
                    repositoryRecord.Value,
                    context.Address.DomainSlug);
            if (!projected.IsSuccess || projected.Value is null)
            {
                return OperationResult<RepositoryOwnedDpoDatasetResolution>.Fail(
                    projected.ErrorMessage,
                    RepositoryOwnedDpoDatasetPairProjectionFailedErrorCode);
            }

            pairs.Add(projected.Value);
        }

        // WHY: the clean-missing path may call the open teacher once, but the
        // resulting DPO bytes must be sealed in the canonical repository
        // dataset before the loop writes any local candidate copy. A later run
        // reopens these bytes from disk and skips the teacher.
        return await RepositoryOwnedDpoDatasetResolver
            .OpenExistingOrCreateAndSealAsync(
                context.Address,
                context.RequestIdentity,
                createPairsAsync: (_, _) => Task.FromResult(
                    OperationResult<IReadOnlyList<DpoPair>>.Ok(
                        pairs.ToArray())),
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static OperationResult<DesignConversionDpoTrainingRecord>
        BindRepositoryOwnedDpoChildTeacherRequestId(
            DesignConversionRepositoryOwnedDpoTransactionContext context,
            DesignConversionDpoTrainingRecord? record,
            int ordinal)
    {
        if (record?.TeacherProvenance is not { } teacher ||
            context.Address is null ||
            context.RequestIdentity is null ||
            ordinal < 0 ||
            string.IsNullOrWhiteSpace(teacher.RequestId))
        {
            return OperationResult<DesignConversionDpoTrainingRecord>.Fail(
                "Repository-owned DPO child records require a complete teacher passport before durable pair projection.",
                RepositoryOwnedDpoDatasetPairProjectionFailedErrorCode);
        }

        // WHY: one teacher authoring call produces the Critic and Improver
        // records together, so the original teacher RequestId is intentionally
        // shared. The durable DPO store, however, tracks consumed teacher
        // requests per pair and must never see two pair rows with the same id.
        // This child id keeps the real teacher passport and derives a stable
        // per-row request identity from the sealed dataset address and request
        // fingerprint; for example, critic[0] and improver[1] from the same
        // teacher call become two deterministic repository-owned children.
        var childTeacher = teacher with
        {
            RequestId = BuildRepositoryOwnedDpoChildTeacherRequestId(
                context,
                record,
                ordinal)
        };
        return OperationResult<DesignConversionDpoTrainingRecord>.Ok(
            record with
            {
                TeacherProvenance = childTeacher
            });
    }

    private static string BuildRepositoryOwnedDpoChildTeacherRequestId(
        DesignConversionRepositoryOwnedDpoTransactionContext context,
        DesignConversionDpoTrainingRecord record,
        int ordinal)
        => string.Concat(
            "repository-owned-dpo-child:",
            DesignConversionDpoGenerator.Sha256Ref(string.Join(
                "\n",
                [
                    context.Address!.ManifestRelativePath,
                    context.RequestIdentity!.RequestIdentitySha256,
                    record.TeacherProvenance!.RequestId,
                    record.RecordId,
                    record.UnitId,
                    ordinal.ToString(CultureInfo.InvariantCulture)
                ])));

    private static void AddRepositoryOwnedDpoDatasetResolutionEvidence(
        OperationResult<RepositoryOwnedDpoDatasetResolution> resolution,
        ICollection<string> evidenceRefs,
        ICollection<string> warnings)
    {
        evidenceRefs.Add(RepositoryOwnedDpoDatasetResolutionMarker);
        if (!resolution.IsSuccess || resolution.Value is null)
        {
            var errorCode = SanitizeRepositoryOwnedDpoDatasetErrorCode(
                resolution.ErrorCode);
            warnings.Add(
                $"{RepositoryOwnedDpoDatasetResolutionFailedWarning}:{errorCode}");
            evidenceRefs.Add(
                $"improvement_loop:{RepositoryOwnedDpoDatasetResolutionSourceMetadataKey}=unavailable");
            evidenceRefs.Add(
                $"improvement_loop:{RepositoryOwnedDpoDatasetLocalCandidateCreatedMetadataKey}={bool.FalseString}");
            return;
        }

        var sourceText = resolution.Value.Source.ToString();
        var manifest = resolution.Value.Dataset.Manifest;
        evidenceRefs.Add(
            $"improvement_loop:{RepositoryOwnedDpoDatasetResolutionSourceMetadataKey}={sourceText}");
        evidenceRefs.Add(
            $"improvement_loop:{RepositoryOwnedDpoDatasetPairCountMetadataKey}={manifest.PairCount.ToString(CultureInfo.InvariantCulture)}");
        evidenceRefs.Add(
            $"improvement_loop:{RepositoryOwnedDpoDatasetManifestSha256MetadataKey}={manifest.ManifestSha256}");
        evidenceRefs.Add(
            $"improvement_loop:{RepositoryOwnedDpoDatasetLocalCandidateCreatedMetadataKey}={bool.FalseString}");
        if (resolution.Value.Source ==
            RepositoryOwnedDpoDatasetResolutionSource
                .ExistingSealedRepositoryDataset)
        {
            evidenceRefs.Add(RepositoryOwnedDpoDatasetExistingReusedMarker);
        }
    }

    private static DesignConversionImprovementLoopResult
        MergeRepositoryOwnedDpoDatasetResolutionMetadata(
            DesignConversionImprovementLoopResult result,
            OperationResult<RepositoryOwnedDpoDatasetResolution>?
                repositoryOwnedDpoResolution)
    {
        if (repositoryOwnedDpoResolution is null)
            return result;

        var metadata = new Dictionary<string, string>(
            result.PublicMetadata,
            StringComparer.Ordinal);
        metadata[RepositoryOwnedDpoDatasetLocalCandidateCreatedMetadataKey] =
            bool.FalseString;
        if (!repositoryOwnedDpoResolution.IsSuccess ||
            repositoryOwnedDpoResolution.Value is null)
        {
            metadata[RepositoryOwnedDpoDatasetResolutionSourceMetadataKey] =
                "unavailable";
            metadata[RepositoryOwnedDpoDatasetPairCountMetadataKey] = "0";
            metadata[RepositoryOwnedDpoDatasetManifestSha256MetadataKey] =
                "unavailable";
            return result with
            {
                PublicMetadata = metadata
            };
        }

        var dataset = repositoryOwnedDpoResolution.Value.Dataset;
        metadata[RepositoryOwnedDpoDatasetResolutionSourceMetadataKey] =
            repositoryOwnedDpoResolution.Value.Source.ToString();
        metadata[RepositoryOwnedDpoDatasetPairCountMetadataKey] =
            dataset.Manifest.PairCount.ToString(CultureInfo.InvariantCulture);
        metadata[RepositoryOwnedDpoDatasetManifestSha256MetadataKey] =
            dataset.Manifest.ManifestSha256;
        return result with
        {
            PublicMetadata = metadata
        };
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
        var resolvedRequiredScore = DesignConversionVisualQaBoundaryPolicy.ResolveQualityThreshold(request.Budget);
        var requiredScore = incumbentPolicy?.RequiredScore ?? resolvedRequiredScore;
        var resolvedQualityThresholdSource = DesignConversionVisualQaBoundaryPolicy.ResolveQualityThresholdSource(request.Budget);
        var qualityThresholdSource = incumbentPolicy is null
            ? resolvedQualityThresholdSource
            : ReadPolicyMetadata(incumbentPolicy, "quality_threshold_source", resolvedQualityThresholdSource);
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
            ["quality_threshold_source"] = qualityThresholdSource,
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
        var configuredMaxTeacherCalls = request.InitialEvaluationRequest.Budget?.MaxTeacherCalls;
        var observedTeacherCallStateValid = TryCountObservedTeacherCalls(
            evidenceRefs,
            out var observedTeacherCallCount);
        if (!observedTeacherCallStateValid)
            observedTeacherCallCount = 0;
        var maxTeacherCallsReached =
            configuredMaxTeacherCalls.HasValue &&
            observedTeacherCallStateValid &&
            observedTeacherCallCount >= configuredMaxTeacherCalls.Value;
        var effectiveStopReason = ResolveMaxTeacherCallsEffectiveStopReason(
            stopReason,
            maxTeacherCallsReached);
        var qualityReached = string.Equals(effectiveStopReason, DesignConversionVisualQaBoundaryPolicy.QualityReached, StringComparison.Ordinal);
        var providerRequired = string.Equals(effectiveStopReason, DesignConversionVisualQaBoundaryPolicy.ProviderRequired, StringComparison.Ordinal);
        var renderManifestIncomplete = string.Equals(effectiveStopReason, DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete, StringComparison.Ordinal) ||
            IsTerminalCoverageRetryIncomplete(attempts) ||
            finalEvaluation?.VisualQaBoundaryTrace.MultiResolutionRender.IsComplete == false;
        var budgetCapReached = string.Equals(effectiveStopReason, DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, StringComparison.Ordinal);
        var maxAttemptsReached = string.Equals(effectiveStopReason, MaxAttemptsReached, StringComparison.Ordinal);
        var cancelled = string.Equals(effectiveStopReason, Cancelled, StringComparison.Ordinal);
        var deadlineExceeded = string.Equals(effectiveStopReason, DeadlineExceeded, StringComparison.Ordinal);
        var candidateStrategyFailed = string.Equals(effectiveStopReason, CandidateStrategyFailed, StringComparison.Ordinal);
        var evaluationFailed = string.Equals(effectiveStopReason, EvaluationFailed, StringComparison.Ordinal);
        var noProgressStop = string.Equals(effectiveStopReason, NoProgressStop, StringComparison.Ordinal);
        var completed = qualityReached && !cancelled && !deadlineExceeded && !candidateStrategyFailed;
        var lastAttempt = attempts.LastOrDefault();
        var finalPolicy = finalEvaluation?.VisualQaBoundaryTrace.TierPolicy;
        var finalMeasuredScore = finalEvaluation is null ? 0.0 : GetMeasuredQualityScore(finalEvaluation);
        var spentBudgetUsd = attempts.Select(attempt => attempt.EstimatedCostUsd).Sum();
        var coverageRetryAttemptCount = attempts.Count(attempt => IsTruthy(attempt.PublicMetadata, "coverage_retry"));
        var configuredRunDeadlineMs = FormatRunDeadlineMilliseconds(configuredRunDeadline);
        var maxTeacherCallsContractSupplied = configuredMaxTeacherCalls.HasValue;
        var repositoryOwnedDpoTransactionMetadata =
            (request.RepositoryOwnedDpoTransactionContext ??
             DesignConversionRepositoryOwnedDpoTransactionContext.Disabled)
            .ToBoundaryMetadata();
        var resultEvidenceRefs = evidenceRefs
            // Repository-owned DPO context state is emitted as public evidence so
            // the model-server boundary can prove whether this was enabled,
            // valid, or still on the explicit disabled legacy route.
            .Concat(repositoryOwnedDpoTransactionMetadata.Select(pair =>
                $"improvement_loop:{pair.Key}={pair.Value}"))
            .Concat(
            [
                $"improvement_loop:stop_reason={effectiveStopReason}",
                $"improvement_loop:{ConfiguredRunDeadlineMsMetadataKey}={configuredRunDeadlineMs}",
                $"improvement_loop:{MaxTeacherCallsContractSuppliedMetadataKey}={(maxTeacherCallsContractSupplied ? bool.TrueString : bool.FalseString)}",
                $"improvement_loop:{MaxTeacherCallsContractValueMetadataKey}={(configuredMaxTeacherCalls.HasValue ? configuredMaxTeacherCalls.Value.ToString(CultureInfo.InvariantCulture) : "not_supplied")}",
                $"improvement_loop:{MaxTeacherCallsContractBoundaryMetadataKey}={DesignConversionVisualQaBoundaryPolicy.MaxTeacherCallsContractBoundary}",
                $"improvement_loop:{MaxTeacherCallsRuntimeEnforcementMetadataKey}={(maxTeacherCallsContractSupplied ? bool.TrueString : bool.FalseString)}",
                $"improvement_loop:{MaxTeacherCallsCallCapEnforcedMetadataKey}={(maxTeacherCallsContractSupplied ? bool.TrueString : bool.FalseString)}",
                $"improvement_loop:{MaxTeacherCallsObservedCallCountMetadataKey}={observedTeacherCallCount.ToString(CultureInfo.InvariantCulture)}",
                $"improvement_loop:{MaxTeacherCallsCapReachedMetadataKey}={(maxTeacherCallsReached ? bool.TrueString : bool.FalseString)}",
                qualityReached ? "improvement_loop:quality_reached" : "improvement_loop:not_quality_reached",
                completed ? "improvement_loop:completed_quality_reached" : "improvement_loop:not_completed",
                deadlineExceeded ? DeadlineExceededMarker : "improvement_loop:deadline_exceeded_false",
                evaluationFailed ? "improvement_loop:evaluation_failed_bounded" : "improvement_loop:evaluation_failed_false",
                noProgressStop ? "improvement_loop:no_progress_stop" : "improvement_loop:no_progress_stop_false",
                maxTeacherCallsReached ? "improvement_loop:max_teacher_calls_reached" : "improvement_loop:max_teacher_calls_reached_false",
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
            ["stop_reason"] = effectiveStopReason,
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
            [MaxTeacherCallsContractSuppliedMetadataKey] = maxTeacherCallsContractSupplied
                ? bool.TrueString
                : bool.FalseString,
            [MaxTeacherCallsContractValueMetadataKey] = configuredMaxTeacherCalls.HasValue
                ? configuredMaxTeacherCalls.Value.ToString(CultureInfo.InvariantCulture)
                : "not_supplied",
            [MaxTeacherCallsContractBoundaryMetadataKey] =
                DesignConversionVisualQaBoundaryPolicy.MaxTeacherCallsContractBoundary,
            [MaxTeacherCallsRuntimeEnforcementMetadataKey] = maxTeacherCallsContractSupplied
                ? bool.TrueString
                : bool.FalseString,
            [MaxTeacherCallsCallCapEnforcedMetadataKey] = maxTeacherCallsContractSupplied
                ? bool.TrueString
                : bool.FalseString,
            [MaxTeacherCallsObservedCallCountMetadataKey] =
                observedTeacherCallCount.ToString(CultureInfo.InvariantCulture),
            [MaxTeacherCallsCapReachedMetadataKey] = maxTeacherCallsReached
                ? bool.TrueString
                : bool.FalseString,
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
        foreach (var pair in repositoryOwnedDpoTransactionMetadata)
        {
            // Loop metadata carries the same DPO context facts as evidence refs
            // because UI/status consumers read metadata without parsing evidence
            // ref strings.
            metadata[pair.Key] = pair.Value;
        }

        var result = new DesignConversionImprovementLoopResult(
            LoopId: request.LoopId,
            StopReason: effectiveStopReason,
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
                .Concat(string.Equals(effectiveStopReason, MaxTeacherCallsReached, StringComparison.Ordinal)
                    ? ["IMPROVEMENT_LOOP_MAX_TEACHER_CALLS_REACHED"]
                    : [])
                .Concat(finalEvaluation?.Warnings ?? [])
                .Distinct(StringComparer.Ordinal)
                .OrderBy(warning => warning, StringComparer.Ordinal)
                .ToArray());
        DesignConversionTeacherCallUsageReceiptAssociation.Associate(
            result,
            DesignConversionTeacherCallUsageReceipt.Create(
                request.InitialEvaluationRequest.JobId,
                request.InitialEvaluationRequest.RequestId,
                configuredMaxTeacherCalls,
                observedTeacherCallCount,
                observedTeacherCallStateValid));
        return result;
    }

    private static string ResolveMaxTeacherCallsEffectiveStopReason(
        string stopReason,
        bool maxTeacherCallsReached)
        => maxTeacherCallsReached &&
           string.Equals(stopReason, NoProgressStop, StringComparison.Ordinal)
            ? MaxTeacherCallsReached
            : stopReason;

    private static bool IsTerminalCoverageRetryIncomplete(
        IReadOnlyList<DesignConversionImprovementLoopAttemptTelemetry> attempts)
    {
        var terminalAttempt = attempts.LastOrDefault();
        return terminalAttempt is not null &&
            IsTruthy(terminalAttempt.PublicMetadata, "coverage_retry_same_candidate") &&
            !IsTruthy(terminalAttempt.PublicMetadata, "required_viewport_coverage_complete");
    }

    private sealed record RuntimeRejectedEvaluationSelection(
        int AttemptNumber,
        string CandidateId,
        DesignConversionInternalEvaluationResult Evaluation,
        double MeasuredScore);

    private static IReadOnlyList<DesignConversionTeacherAuthoredEvaluationPair>
        SelectTeacherAuthoredEvaluationPairs(
        IReadOnlyList<MeasuredEvaluationAttempt> measuredAttempts)
    {
        var selected = new List<DesignConversionTeacherAuthoredEvaluationPair>();
        foreach (var attempt in measuredAttempts.OrderBy(value => value.AttemptNumber))
        {
            if (attempt.TeacherAuthoredPair is not null)
            {
                selected.Add(attempt.TeacherAuthoredPair);
                continue;
            }

            var context = attempt.TeacherAuthoring;
            if (context is null)
                continue;

            selected.Add(new DesignConversionTeacherAuthoredEvaluationPair(
                attempt.AttemptNumber,
                attempt.Evaluation,
                context.IncumbentEvaluation,
                context.AuthoringEvidence,
                context.CandidateId));
        }

        return selected.ToArray();
    }

    private static IReadOnlyList<
        DesignConversionIndependentPreferenceEvidence>
        SelectCoordinatorConsumedIndependentPreferenceEvidence(
            IReadOnlyList<MeasuredEvaluationAttempt> measuredAttempts)
        => measuredAttempts
            .OrderBy(value => value.AttemptNumber)
            .Select(value => value.IndependentPreferenceEvidence)
            .Where(static value => value is not null)
            .Cast<DesignConversionIndependentPreferenceEvidence>()
            .GroupBy(value => value.ReceiptSha256, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToArray();

    private static bool EvidenceMatchesPair(
        DesignConversionIndependentPreferenceEvidence evidence,
        DesignConversionTeacherAuthoredEvaluationPair pair)
        => evidence.AttemptNumber == pair.AttemptNumber &&
           string.Equals(evidence.GeneratedRef,
               pair.AuthoringEvidence.GeneratedRef, StringComparison.Ordinal) &&
           string.Equals(evidence.IncumbentRef,
               pair.AuthoringEvidence.IncumbentRef, StringComparison.Ordinal) &&
           string.Equals(evidence.GeneratedEvaluationIdentity,
               DesignConversionDpoGenerator
                   .BuildIndependentPreferenceEvaluationIdentity(
                       pair.GeneratedEvaluation),
               StringComparison.Ordinal) &&
           string.Equals(evidence.IncumbentEvaluationIdentity,
               DesignConversionDpoGenerator
                   .BuildIndependentPreferenceEvaluationIdentity(
                       pair.IncumbentEvaluation),
               StringComparison.Ordinal);

    private static RuntimeRejectedEvaluationSelection? SelectRuntimeRejectedEvaluation(
        DesignConversionInternalEvaluationResult finalEvaluation,
        IReadOnlyList<MeasuredEvaluationAttempt> measuredAttempts)
    {
        if (measuredAttempts.Count == 0 ||
            finalEvaluation.RuntimeFlowResult?.Output is null)
        {
            return null;
        }

        var finalAttempt = measuredAttempts
            .Where(attempt => IsSameEvaluationIdentity(attempt.Evaluation, finalEvaluation))
            .OrderByDescending(attempt => attempt.AttemptNumber)
            .FirstOrDefault();
        if (finalAttempt is null)
            return null;

        var finalScore = GetMeasuredQualityScore(finalEvaluation);
        if (!IsFiniteScore(finalScore))
            return null;

        return measuredAttempts
            .Where(attempt => attempt.AttemptNumber < finalAttempt.AttemptNumber)
            .Select(attempt => new RuntimeRejectedEvaluationSelection(
                attempt.AttemptNumber,
                attempt.CandidateId,
                attempt.Evaluation,
                GetMeasuredQualityScore(attempt.Evaluation)))
            .Where(selection =>
                IsFiniteScore(selection.MeasuredScore) &&
                selection.MeasuredScore < finalScore &&
                !string.IsNullOrWhiteSpace(selection.Evaluation.JobId) &&
                !string.IsNullOrWhiteSpace(finalEvaluation.JobId) &&
                !string.Equals(
                    selection.Evaluation.JobId,
                    finalEvaluation.JobId,
                    StringComparison.Ordinal) &&
                !IsSameEvaluationIdentity(selection.Evaluation, finalEvaluation) &&
                HasDistinctRuntimeOutputIdentity(finalEvaluation, selection.Evaluation))
            .OrderByDescending(selection => selection.MeasuredScore)
            .ThenByDescending(selection => selection.AttemptNumber)
            .FirstOrDefault();
    }

    private static bool IsSameEvaluationIdentity(
        DesignConversionInternalEvaluationResult left,
        DesignConversionInternalEvaluationResult right)
    {
        var leftOutput = left.RuntimeFlowResult?.Output;
        var rightOutput = right.RuntimeFlowResult?.Output;
        if (leftOutput is not null && rightOutput is not null)
        {
            return string.Equals(leftOutput.OutputId, rightOutput.OutputId, StringComparison.Ordinal) ||
                string.Equals(leftOutput.CandidateId, rightOutput.CandidateId, StringComparison.Ordinal) ||
                string.Equals(leftOutput.Verdict.VerdictId, rightOutput.Verdict.VerdictId, StringComparison.Ordinal);
        }

        return string.Equals(left.JobId, right.JobId, StringComparison.Ordinal) ||
            string.Equals(left.SubmittedRequest.RequestId, right.SubmittedRequest.RequestId, StringComparison.Ordinal);
    }

    private static bool HasDistinctRuntimeOutputIdentity(
        DesignConversionInternalEvaluationResult chosen,
        DesignConversionInternalEvaluationResult rejected)
    {
        var chosenOutput = chosen.RuntimeFlowResult?.Output;
        var rejectedOutput = rejected.RuntimeFlowResult?.Output;
        return chosenOutput is not null &&
            rejectedOutput is not null &&
            !string.IsNullOrWhiteSpace(chosenOutput.OutputId) &&
            !string.IsNullOrWhiteSpace(rejectedOutput.OutputId) &&
            !string.Equals(chosenOutput.OutputId, rejectedOutput.OutputId, StringComparison.Ordinal) &&
            !string.IsNullOrWhiteSpace(chosenOutput.CandidateId) &&
            !string.IsNullOrWhiteSpace(rejectedOutput.CandidateId) &&
            !string.Equals(chosenOutput.CandidateId, rejectedOutput.CandidateId, StringComparison.Ordinal) &&
            !string.IsNullOrWhiteSpace(chosenOutput.Verdict.VerdictId) &&
            !string.IsNullOrWhiteSpace(rejectedOutput.Verdict.VerdictId) &&
            !string.Equals(chosenOutput.Verdict.VerdictId, rejectedOutput.Verdict.VerdictId, StringComparison.Ordinal);
    }

    private static IReadOnlyDictionary<string, string> BuildLocalArtifactDbMetadata(
        DesignConversionImprovementLoopRequest request,
        DesignConversionImprovementLoopResult result,
        int dpoCandidateCount,
        IReadOnlyList<string> dpoBuildWarnings,
        string attemptHistoryLoopRef,
        string canonicalAttemptHistoryJson)
    {
        var finalEvaluation = result.FinalEvaluation!;
        var lastAttempt = result.Attempts.LastOrDefault();
        var finalCandidateFromLoop = lastAttempt is not null && lastAttempt.AttemptNumber > 1;
        var highQualityNegativePairProven = dpoBuildWarnings.Contains(
            "high_quality_negative_pair_proven",
            StringComparer.Ordinal);
        var chosenRejectedPairProven = dpoBuildWarnings.Contains(
            "chosen_rejected_pair_proven",
            StringComparer.Ordinal);
        var runtimeRejectedIndependentlyEvaluated = dpoBuildWarnings.Contains(
            "runtime_rejected_independently_evaluated=true",
            StringComparer.Ordinal);
        var teacherAttestationVerified = dpoBuildWarnings.Contains(
            "runtime_teacher_attestation_verified=true",
            StringComparer.Ordinal);
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
            [DesignConversionLocalArtifactDbRecord
                .ImprovementLoopRefMetadataKey] = attemptHistoryLoopRef,
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
            ["chosen_rejected_pair_proven"] = chosenRejectedPairProven.ToString(),
            ["chosen_rejected_pair_not_proven"] = (!chosenRejectedPairProven).ToString(),
            ["high_quality_negative_pair_proven"] = highQualityNegativePairProven.ToString(),
            ["high_quality_negative_pair_not_proven"] = (!highQualityNegativePairProven).ToString(),
            ["runtime_rejected_independently_evaluated"] =
                runtimeRejectedIndependentlyEvaluated.ToString(),
            ["runtime_teacher_attestation_verified"] = teacherAttestationVerified.ToString(),
            ["review_status"] = dpoCandidateCount > 0
                ? JsonlDesignConversionLocalArtifactDb.RuntimeCandidateReviewPendingStatus
                : "not-created",
            ["review_required"] = (dpoCandidateCount > 0).ToString(),
            ["approval_required"] = (dpoCandidateCount > 0).ToString(),
            ["approval_status"] = dpoCandidateCount > 0 ? "pending" : "not-required",
            ["approved_for_training"] = bool.FalseString,
            ["training_candidate_only"] = (dpoCandidateCount > 0).ToString(),
            ["product_not_ready"] = bool.TrueString,
            ["production_dpo"] = bool.FalseString,
            ["production_db_ready"] = bool.FalseString,
            ["model_readiness_claimed"] = bool.FalseString,
            [DesignConversionLocalArtifactDbRecord
                .ImprovementLoopAttemptHistoryMetadataKey] =
                canonicalAttemptHistoryJson
        };

        if (dpoCandidateCount > 0 &&
            dpoBuildWarnings.Contains(
                LocalArtifactDbDpoSelectedRejectedAttemptMarker,
                StringComparer.Ordinal))
        {
            metadata[LocalArtifactDbDpoSelectedRejectedAttemptMarker] = bool.TrueString;
            CopyWarningValueToMetadata(
                dpoBuildWarnings,
                metadata,
                "runtime_rejected_attempt_number");
            CopyWarningValueToMetadata(
                dpoBuildWarnings,
                metadata,
                "runtime_rejected_candidate_ref");
            CopyWarningValueToMetadata(
                dpoBuildWarnings,
                metadata,
                "runtime_rejected_measured_score");
        }
        else if (dpoCandidateCount == 0)
        {
            metadata[LocalArtifactDbDpoNoEligibleRejectedAttemptMarker] = bool.TrueString;
        }

        if (dpoBuildWarnings.Count > 0)
            metadata["runtime_created_dpo_candidate_warnings"] = string.Join("|", dpoBuildWarnings.Distinct(StringComparer.Ordinal));

        foreach (var key in MaxTeacherCallsMetadataKeys())
        {
            if (result.PublicMetadata.TryGetValue(key, out var value))
                metadata[key] = value;
        }

        foreach (var key in RepositoryOwnedDpoTransactionMetadataKeys())
        {
            if (result.PublicMetadata.TryGetValue(key, out var value))
                metadata[key] = value;
        }

        foreach (var key in RepositoryOwnedDpoDatasetResolutionMetadataKeys())
        {
            if (result.PublicMetadata.TryGetValue(key, out var value))
                metadata[key] = value;
        }

        return metadata;
    }

    private static IEnumerable<string> RepositoryOwnedDpoDatasetResolutionMetadataKeys()
    {
        yield return RepositoryOwnedDpoDatasetResolutionSourceMetadataKey;
        yield return RepositoryOwnedDpoDatasetPairCountMetadataKey;
        yield return RepositoryOwnedDpoDatasetManifestSha256MetadataKey;
        yield return RepositoryOwnedDpoDatasetLocalCandidateCreatedMetadataKey;
    }

    private static IEnumerable<string> RepositoryOwnedDpoTransactionMetadataKeys()
    {
        yield return DesignConversionRepositoryOwnedDpoTransactionContext
            .EnabledMetadataKey;
        yield return DesignConversionRepositoryOwnedDpoTransactionContext
            .ValidationMetadataKey;
        yield return DesignConversionRepositoryOwnedDpoTransactionContext
            .ManifestRelativePathMetadataKey;
        yield return DesignConversionRepositoryOwnedDpoTransactionContext
            .RequestIdentitySha256MetadataKey;
    }

    private static IEnumerable<string> MaxTeacherCallsMetadataKeys()
    {
        yield return MaxTeacherCallsContractSuppliedMetadataKey;
        yield return MaxTeacherCallsContractValueMetadataKey;
        yield return MaxTeacherCallsContractBoundaryMetadataKey;
        yield return MaxTeacherCallsRuntimeEnforcementMetadataKey;
        yield return MaxTeacherCallsCallCapEnforcedMetadataKey;
        yield return MaxTeacherCallsObservedCallCountMetadataKey;
        yield return MaxTeacherCallsCapReachedMetadataKey;
    }

    private static bool IsTeacherCallBudgetEvidenceRef(string evidenceRef)
        => !string.IsNullOrWhiteSpace(evidenceRef) &&
           (evidenceRef.StartsWith(
                ObservedTeacherCallEvidenceRefPrefix,
                StringComparison.Ordinal) ||
            evidenceRef.StartsWith(
                "improvement_loop:max_teacher_calls_",
                StringComparison.Ordinal));

    private static void CopyWarningValueToMetadata(
        IReadOnlyList<string> warnings,
        IDictionary<string, string> metadata,
        string key)
    {
        var prefix = key + "=";
        var warning = warnings.FirstOrDefault(value =>
            value.StartsWith(prefix, StringComparison.Ordinal));
        if (!string.IsNullOrWhiteSpace(warning))
            metadata[key] = warning[prefix.Length..];
    }

    private static DesignConversionImprovementLoopResult MarkLocalArtifactDbPersistence(
        DesignConversionImprovementLoopResult result,
        DesignConversionLocalArtifactDbRecord record,
        int dpoCandidateCount)
    {
        var historyOnly = IsTruthy(
            record.Metadata,
            ImprovementLoopHistoryOnlyMetadataKey);
        if (!record.TryGetImprovementLoopAttemptHistory(out var attemptHistory) ||
            attemptHistory is null)
        {
            throw new InvalidOperationException(
                "Durable improvement-loop attempt history is required before public persistence markers are emitted.");
        }

        var metadata = new Dictionary<string, string>(result.PublicMetadata, StringComparer.Ordinal)
        {
            [LocalArtifactDbAutowriteMarker] = bool.TrueString,
            ["local_artifact_db_record_ref"] = HashRef("dcm-local-artifact-db-record", record.RecordId),
            ["local_artifact_db_job_ref"] = HashRef("dcm-local-artifact-db-job", record.JobId),
            ["local_artifact_db_request_ref"] = HashRef("dcm-local-artifact-db-request", record.RequestId),
            ["runtime_created_dpo_candidate_count"] = dpoCandidateCount.ToString(CultureInfo.InvariantCulture),
            ["runtime_created_dpo_candidate"] = (dpoCandidateCount > 0).ToString(),
            ["training_candidate_only"] = (dpoCandidateCount > 0).ToString(),
            ["product_not_ready"] = bool.TrueString,
            ["approved_for_training"] = bool.FalseString,
            ["production_dpo"] = bool.FalseString,
            ["production_db_ready"] = bool.FalseString,
            [ImprovementLoopHistoryStatusMetadataKey] =
                DesignConversionLocalArtifactDbRecord
                    .ImprovementLoopAttemptHistoryRecordedStatus,
            [ImprovementLoopHistoryAttemptCountMetadataKey] =
                attemptHistory.AttemptCount.ToString(CultureInfo.InvariantCulture),
            [ImprovementLoopHistorySha256MetadataKey] =
                attemptHistory.HistorySha256,
            [ImprovementLoopHistoryOnlyMetadataKey] = historyOnly.ToString()
        };
        if (!historyOnly)
            metadata[LocalTrainingStoreDpoSourceMarker] = bool.TrueString;

        var persistenceEvidence = result.EvidenceRefs
            .Append(LocalArtifactDbAutowriteMarker)
            .Append(JsonlDesignConversionLocalArtifactDb.StorageTrainingArtifactDbMarker)
            .Append(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind)
            .Append($"{ImprovementLoopHistoryStatusMetadataKey}=" +
                    DesignConversionLocalArtifactDbRecord
                        .ImprovementLoopAttemptHistoryRecordedStatus)
            .Append($"{ImprovementLoopHistoryAttemptCountMetadataKey}=" +
                    attemptHistory.AttemptCount.ToString(CultureInfo.InvariantCulture))
            .Append($"{ImprovementLoopHistorySha256MetadataKey}=" +
                    attemptHistory.HistorySha256);
        var persistenceWarnings = result.Warnings
            .Append("improvement_loop_local_artifact_db_autowrite=true")
            .Append(dpoCandidateCount > 0
                ? "training_candidate_only=true"
                : "training_candidate_only=false");
        if (historyOnly)
        {
            persistenceEvidence = persistenceEvidence
                .Append(ImprovementLoopHistoryOnlyMarker);
            persistenceWarnings = persistenceWarnings
                .Append("improvement_loop_history_only=true");
        }
        else
        {
            persistenceEvidence = persistenceEvidence
                .Append(LocalTrainingStoreDpoSourceMarker);
            persistenceWarnings = persistenceWarnings
                .Append("improvement_loop_local_training_store_dpo_source=true");
        }

        return DesignConversionTeacherCallUsageReceiptAssociation.CarryForward(
            result,
            result with
            {
                EvidenceRefs = persistenceEvidence
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(reference => reference, StringComparer.Ordinal)
                    .ToArray(),
                PublicMetadata = metadata,
                Warnings = persistenceWarnings
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(warning => warning, StringComparer.Ordinal)
                    .ToArray()
            });
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
            ["runtime_created_dpo_candidate"] = bool.FalseString,
            ["training_candidate_only"] = bool.FalseString
        };

        return DesignConversionTeacherCallUsageReceiptAssociation.CarryForward(
            result,
            result with
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
                    .Append("training_candidate_only=false")
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(warning => warning, StringComparer.Ordinal)
                    .ToArray()
            });
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
            ["training_candidate_only"] = (dpoCandidateCount > 0).ToString(),
            ["production_db_ready"] = bool.FalseString
        };

        return DesignConversionTeacherCallUsageReceiptAssociation.CarryForward(
            result,
            result with
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
                    .Append(dpoCandidateCount > 0
                        ? "training_candidate_only=true"
                        : "training_candidate_only=false")
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(warning => warning, StringComparer.Ordinal)
                    .ToArray()
            });
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

    private sealed class LocalArtifactDbTrainingIntakePreflightHandoffGateway(
        IDesignConversionLocalArtifactDb artifactDb)
        : IDesignConversionTrainingIntakePreflightHandoffGateway
    {
        private const string CoordinatorLocalPromotionDirectoryName =
            "coordinator-training-intake-local-promotions";

        public async Task<OperationResult<DesignConversionTrainingIntakePreflightHandoffResult>> SubmitPreflightAsync(
            DesignConversionTrainingIntakePreflightHandoffRequest request,
            CancellationToken cancellationToken = default)
        {
            var handoff = await artifactDb.ExportApprovedRuntimeDpoCandidatesAsync(
                new DesignConversionLocalArtifactDbQuery(
                    JobId: request.SourceJobId,
                    RequestId: request.SourceRequestId,
                    ApprovedTrainingOnly: true),
                request.RequestedAtUtc,
                cancellationToken);
            if (!handoff.IsSuccess)
                return handoff.MapError<DesignConversionTrainingIntakePreflightHandoffResult>();
            var handoffValue = handoff.Value!;

            var intake = await artifactDb.CreateTrainingIntakeAsync(
                handoffValue,
                request.RequestedByRef,
                request.Reason,
                request.RequestedAtUtc,
                cancellationToken,
                request.AdmissionBoundaryMetadata);
            if (!intake.IsSuccess)
                return intake.MapError<DesignConversionTrainingIntakePreflightHandoffResult>();
            var intakeValue = intake.Value!;
            var persistedContract =
                ReadPersistedPreflightContract(request, intakeValue);
            if (!persistedContract.IsSuccess)
            {
                return persistedContract
                    .MapError<
                        DesignConversionTrainingIntakePreflightHandoffResult>();
            }
            var persistedContractValue = persistedContract.Value!;

            var preflight = await artifactDb.RunTrainingIntakePreflightAsync(
                intakeValue.IntakeId,
                cancellationToken);
            if (!preflight.IsSuccess)
                return preflight.MapError<DesignConversionTrainingIntakePreflightHandoffResult>();
            var preflightValue = preflight.Value!;

            return OperationResult<DesignConversionTrainingIntakePreflightHandoffResult>.Ok(
                new DesignConversionTrainingIntakePreflightHandoffResult(
                    Intake: intakeValue,
                    Preflight: preflightValue,
                    EvidenceRefs: BuildGatewayEvidenceRefs(
                        request,
                        handoffValue,
                        intakeValue,
                        preflightValue,
                        persistedContractValue),
                    Warnings: handoffValue.Warnings
                        .Concat(intakeValue.Warnings)
                        .Concat(preflightValue.Warnings)
                        .Append("coordinator_training_intake_preflight_gateway_completed")
                        .Distinct(StringComparer.Ordinal)
                        .Order(StringComparer.Ordinal)
                        .ToArray(),
                    BoundaryMetadata: BuildGatewayBoundaryMetadata(
                        intakeValue,
                        preflightValue,
                        persistedContractValue)));
        }

        public Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>> RunTrainingIntakeAndPromoteLocalAsync(
            DesignConversionTrainingIntakeRunAndPromoteLocalRequest request,
            CancellationToken cancellationToken = default)
        {
            if (artifactDb is not IDesignConversionLocalPromotionActivationAuthority
                activationAuthority)
            {
                return Task.FromResult(
                    OperationResult<
                        DesignConversionTrainingIntakeLocalPromotionResult>.Fail(
                        "DesignConversion local promotion authority is required.",
                        "DESIGN_CONVERSION_LOCAL_PROMOTION_AUTHORITY_REQUIRED"));
            }

            var outputDirectory = Path.Combine(
                activationAuthority.AllowedArtifactRootDirectory,
                CoordinatorLocalPromotionDirectoryName);
            return DesignConversionTrainingIntakeRouteAdmissionContext.RunAsync(
                request.AdmissionBoundaryMetadata,
                () => artifactDb.RunTrainingIntakeAndPromoteLocalAsync(
                    request.IntakeId,
                    outputDirectory,
                    request.TrainingOptions,
                    cancellationToken));
        }

        private static IReadOnlyList<string> BuildGatewayEvidenceRefs(
            DesignConversionTrainingIntakePreflightHandoffRequest request,
            DesignConversionApprovedDpoHandoffArtifact handoff,
            DesignConversionTrainingIntakeRecord intake,
            DesignConversionTrainingIntakePreflightResult preflight,
            PersistedPreflightContractSnapshot persistedContract)
            => request.SourceEvidenceRefs
                .Concat(handoff.Rows.SelectMany(row => row.EvidenceRefs))
                .Append(handoff.ArtifactRef)
                .Append(intake.IntakeRef)
                .Append(preflight.PreflightId)
                .Append(persistedContract.MeasurementGateContractRef)
                .Append($"training_intake_preflight_dpo_step_count={persistedContract.DpoStepCountText}")
                .Append("coordinator_training_intake_preflight_gateway_evidence")
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();

        private static IReadOnlyDictionary<string, string> BuildGatewayBoundaryMetadata(
            DesignConversionTrainingIntakeRecord intake,
            DesignConversionTrainingIntakePreflightResult preflight,
            PersistedPreflightContractSnapshot persistedContract)
            => new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["gateway"] = nameof(LocalArtifactDbTrainingIntakePreflightHandoffGateway),
                ["stable_intake_id"] = persistedContract.StableIntakeId,
                ["actual_intake_id"] = intake.IntakeId,
                ["idempotency_key_ref"] = persistedContract.IdempotencyKeyRef,
                ["preflight_id"] = preflight.PreflightId,
                ["measurement_gate_id"] = persistedContract.MeasurementGateId,
                ["measurement_gate_unit_id"] = persistedContract.MeasurementGateUnitId,
                ["measurement_gate_contract_ref"] = persistedContract.MeasurementGateContractRef,
                ["dpo_step_count"] = persistedContract.DpoStepCountText
            };

        private static OperationResult<PersistedPreflightContractSnapshot>
            ReadPersistedPreflightContract(
            DesignConversionTrainingIntakePreflightHandoffRequest request,
            DesignConversionTrainingIntakeRecord intake)
        {
            if (!IsAllowedDpoStepCount(request.DpoStepCount))
            {
                return PersistedPreflightContractNotPreserved();
            }

            var metadata = intake.BoundaryMetadata;
            var expectedContractRef =
                BuildTrainingIntakePreflightMeasurementGateContractRef(
                    request.MeasurementGateRequest);
            if (!TryReadRequiredPersistedMetadata(
                    metadata,
                    "coordinator_stable_intake_id",
                    request.StableIntakeId,
                    out var stableIntakeId) ||
                !TryReadRequiredPersistedMetadata(
                    metadata,
                    "training_intake_idempotency_key_ref",
                    request.IdempotencyKeyRef,
                    out var idempotencyKeyRef) ||
                !TryReadRequiredPersistedMetadata(
                    metadata,
                    "coordinator_measurement_gate_id",
                    DesignConversionMeasurementAdmission.ProjectIdentifierRef(
                        "gate-id",
                        request.MeasurementGateRequest.GateId),
                    out var gateId) ||
                !TryReadRequiredPersistedMetadata(
                    metadata,
                    "coordinator_measurement_gate_unit_id",
                    DesignConversionMeasurementAdmission.ProjectIdentifierRef(
                        "unit-id",
                        request.MeasurementGateRequest.UnitId),
                    out var gateUnitId) ||
                !TryReadRequiredPersistedMetadata(
                    metadata,
                    "coordinator_measurement_gate_contract_ref",
                    expectedContractRef,
                    out var gateContractRef) ||
                !TryReadRequiredPersistedMetadata(
                    metadata,
                    "coordinator_dpo_step_count",
                    request.DpoStepCount.ToString(CultureInfo.InvariantCulture),
                    out var dpoStepCountText) ||
                !int.TryParse(
                    dpoStepCountText,
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out var persistedDpoStepCount) ||
                !IsAllowedDpoStepCount(persistedDpoStepCount) ||
                persistedDpoStepCount != request.DpoStepCount)
            {
                return PersistedPreflightContractNotPreserved();
            }

            return OperationResult<PersistedPreflightContractSnapshot>.Ok(
                new PersistedPreflightContractSnapshot(
                    stableIntakeId,
                    idempotencyKeyRef,
                    gateId,
                    gateUnitId,
                    gateContractRef,
                    dpoStepCountText));
        }

        private static bool TryReadRequiredPersistedMetadata(
            IReadOnlyDictionary<string, string> metadata,
            string key,
            string expectedValue,
            out string value)
        {
            value = string.Empty;
            if (string.IsNullOrWhiteSpace(expectedValue) ||
                !metadata.TryGetValue(key, out var actualValue) ||
                string.IsNullOrWhiteSpace(actualValue) ||
                !string.Equals(actualValue, expectedValue, StringComparison.Ordinal))
            {
                return false;
            }

            value = actualValue;
            return true;
        }

        private static bool IsAllowedDpoStepCount(int dpoStepCount)
            => dpoStepCount >=
                   LogicalDpoTrainingUnit.MinimumSupportedMaxStepCount &&
               dpoStepCount <=
                   DesignConversionDpoPolicyCheckpointBridgeOptions
                       .MaxAllowedDpoStepCount;

        private static OperationResult<PersistedPreflightContractSnapshot>
            PersistedPreflightContractNotPreserved()
            => OperationResult<PersistedPreflightContractSnapshot>.Fail(
                "DesignConversion training-intake preflight contract was not durably preserved.",
                "DESIGN_CONVERSION_TRAINING_INTAKE_PREFLIGHT_CONTRACT_NOT_PERSISTED");

        private sealed record PersistedPreflightContractSnapshot(
            string StableIntakeId,
            string IdempotencyKeyRef,
            string MeasurementGateId,
            string MeasurementGateUnitId,
            string MeasurementGateContractRef,
            string DpoStepCountText);
    }

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

    private static bool TryCountObservedTeacherCalls(
        IEnumerable<string> evidenceRefs,
        out int observedTeacherCallCount)
    {
        observedTeacherCallCount = 0;
        var ordinals = new List<int>();
        foreach (var evidenceRef in evidenceRefs)
        {
            if (string.IsNullOrWhiteSpace(evidenceRef) ||
                !evidenceRef.StartsWith(
                    ObservedTeacherCallEvidenceRefPrefix,
                    StringComparison.Ordinal))
            {
                continue;
            }

            var ordinal = ReadObservedTeacherCallOrdinal(evidenceRef);
            if (ordinal <= 0)
                return false;
            ordinals.Add(ordinal);
        }

        ordinals.Sort();
        for (var index = 0; index < ordinals.Count; index++)
        {
            if (ordinals[index] != index + 1)
                return false;
        }

        observedTeacherCallCount = ordinals.Count;
        return true;
    }

    private static void AddTeacherAuthoringFallbackEvidence(
        ICollection<string> warnings,
        ICollection<string> evidenceRefs,
        string? errorCode)
    {
        var sanitizedErrorCode = SanitizeTeacherAuthoringErrorCode(errorCode);
        warnings.Add(TeacherAuthoringErrorWarningPrefix + sanitizedErrorCode);
        warnings.Add(TeacherAuthoringFallbackMarker);
        evidenceRefs.Add(TeacherAuthoringFallbackMarker);
        evidenceRefs.Add(
            "teacher-authoring:fallback_error_code=" + sanitizedErrorCode);
    }

    private static string SanitizeTeacherAuthoringErrorCode(string? errorCode)
    {
        if (string.IsNullOrWhiteSpace(errorCode) ||
            errorCode.Length > 128 ||
            errorCode.Any(value => value is not (>= 'A' and <= 'Z')
                and not (>= '0' and <= '9') and not '_'))
        {
            return DesignConversionDpoTeacherAttestationRuntime
                .RuntimeUnavailableErrorCode;
        }

        return errorCode;
    }

    private static string SanitizeRepositoryOwnedDpoDatasetErrorCode(
        string? errorCode)
    {
        if (string.IsNullOrWhiteSpace(errorCode) ||
            errorCode.Length > 128 ||
            errorCode.Any(value => value is not (>= 'A' and <= 'Z')
                and not (>= '0' and <= '9') and not '_'))
        {
            return RepositoryOwnedDpoDatasetResolver.UnavailableErrorCode;
        }

        return errorCode;
    }

    private static int ReadObservedTeacherCallOrdinal(string evidenceRef)
    {
        if (string.IsNullOrWhiteSpace(evidenceRef) ||
            !evidenceRef.StartsWith(ObservedTeacherCallEvidenceRefPrefix, StringComparison.Ordinal))
        {
            return 0;
        }

        return int.TryParse(
            evidenceRef[ObservedTeacherCallEvidenceRefPrefix.Length..],
            NumberStyles.None,
            CultureInfo.InvariantCulture,
            out var ordinal)
            ? Math.Max(0, ordinal)
            : 0;
    }

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
