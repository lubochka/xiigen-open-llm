// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Globalization;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DesignConversionDomain;

public sealed record DesignConversionModelServerPayloadRef(
    string RefId,
    string Kind,
    string? Sha256 = null,
    string? Payload = null,
    string? Format = null,
    IReadOnlyDictionary<string, string>? Metadata = null);

public sealed record DesignConversionModelServerArtifactRef(
    string ArtifactId,
    string Kind,
    int Width,
    int Height,
    string Uri,
    string Sha256,
    IReadOnlyDictionary<string, string>? Metadata = null);

public sealed record DesignConversionModelServerBudget(
    string UserId,
    string Tier,
    int MaxImprovementCandidates,
    int MaxRenderQaRuns,
    decimal BudgetUsd,
    IReadOnlyDictionary<string, string>? Metadata = null,
    double? RenderQaPassThreshold = null,
    int? MaxImprovementIterations = null,
    int? MaxTeacherCalls = null);

public sealed record DesignConversionModelServerTrustedVisualCostEntitlement(
    string UserIdRef,
    string TenantIdRef,
    string DesignTier,
    decimal MonthlyVisualCostLimitUsd,
    string MonthlyVisualCostLimitUsdText,
    string DesignTierAuthority,
    string MonthlyVisualCostLimitAuthority,
    string UserIdentityAuthority,
    string PeriodKind);

public sealed record DesignConversionModelServerApiRequest(
    string RequestId,
    DesignConversionSourceKind SourceKind,
    DesignConversionModelServerPayloadRef DesignTree,
    DesignConversionModelServerPayloadRef OriginalTree,
    string Html,
    string Css,
    IReadOnlyList<DesignConversionModelServerArtifactRef>? Screenshots,
    IReadOnlyList<DesignConversionModelServerArtifactRef>? DesignRenders,
    DesignConversionModelServerBudget? Budget,
    IReadOnlyDictionary<string, string>? Metadata = null)
{
    /// <summary>
    /// Carries the typed repository-owned DPO transaction identity through the
    /// public model-server request so production composition cannot silently
    /// drop the pre-inference fail-closed authority.
    /// </summary>
    public DesignConversionRepositoryOwnedDpoTransactionContext?
        RepositoryOwnedDpoTransactionContext
    {
        get;
        init;
    }
}

public sealed record DesignConversionModelServerTrainingReadiness(
    string Status,
    int RuntimeCreatedRecordCount,
    int DpoCandidateCount,
    string SourceDisjointStatus,
    string MeasurementStatus,
    string PromotionStatus,
    bool ModelTrainingVerified,
    bool ModelUploadReady,
    bool DeploymentVerified,
    bool ProductReady,
    IReadOnlyList<string> Warnings,
    [property: JsonIgnore]
    DesignConversionPromotedLocalModel? PromotedLocalModel = null);

public sealed record DesignConversionModelServerApiResult(
    string ApiShape,
    string RuntimeSurface,
    string JobId,
    string RequestId,
    string ContractStatus,
    bool LearnedInferenceApplied,
    bool TrainedModelAuthority,
    bool NotTrainedModelAuthority,
    bool DeterministicLogicClaimedAsLearning,
    bool ModelTrainingVerified,
    bool ModelUploadReady,
    bool DeploymentVerified,
    bool PublicHttpEndpointVerified,
    bool PlatformIntegrationVerified,
    bool ProductReady,
    DesignConversionThinClientStatusResult RuntimeStatus,
    DesignConversionModelServerTrainingReadiness TrainingReadiness,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> Warnings,
    IReadOnlyDictionary<string, string> BoundaryMetadata,
    [property: JsonIgnore]
    DesignConversionPromotedLocalModel? PromotedLocalModel = null);

public sealed record DesignConversionModelServerStatusResponse(
    string ApiShape,
    string JobId,
    string ContractStatus,
    bool LearnedInferenceApplied,
    bool TrainedModelAuthority,
    bool NotTrainedModelAuthority,
    bool DeterministicLogicClaimedAsLearning,
    bool PublicHttpEndpointVerified,
    bool DeploymentVerified,
    bool ProductReady,
    DesignConversionThinClientStatusResult RuntimeStatus,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyDictionary<string, string> BoundaryMetadata,
    [property: JsonIgnore]
    DesignConversionPromotedLocalModel? PromotedLocalModel = null);

public sealed record DesignConversionModelServerVisualCostSummary(
    int RecordCount,
    decimal TotalCostUsd,
    bool MeasurementComplete,
    int MissingMeasurementCount,
    int EvaluationAttemptCount,
    string CostSource);

public sealed record DesignConversionModelServerVisualCostRecordsResponse(
    string ApiShape,
    string RuntimeSurface,
    DesignConversionModelServerVisualCostSummary Summary,
    IReadOnlyList<DesignConversionVisualCostLedgerRecord> Records,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionModelServerPromotedLocalApiFactoryResult(
    DesignConversionModelServerApi Api,
    DesignConversionPromotedLocalModelLoadResult PromotedLoad,
    IReadOnlyList<string> EvidenceRefs);

public sealed class DesignConversionModelServerApi
{
    public const string ApiShape = "design_conversion_model_server_api_v1";
    public const string RuntimeSurface = "in_process_model_server_design_conversion_api";
    public const string HttpWiringBoundary = "no_asp_net_or_minimal_api_host_in_25_modelserver";
    public const string ServerOwnedRouteAdmissionTenantBoundProvenanceKey = "server_owned_route_admission_tenant_bound";
    public const string ServerOwnedRouteAdmissionTenantIdProvenanceKey = "server_owned_route_admission_tenant_id";
    public const string ServerOwnedRouteAdmissionTenantIdRefProvenanceKey = "server_owned_route_admission_tenant_id_ref";
    public const string ServerOwnedRouteAdmissionScopeRefProvenanceKey = "server_owned_route_admission_scope_ref";
    public const string CallerTenantMetadataAuthoritativeProvenanceKey = "caller_tenant_metadata_authoritative";
    public const string ServerAssignedDesignTierMetadataKey = "server_assigned_design_tier";
    public const string ServerAssignedMonthlyVisualCostLimitUsdMetadataKey = "server_assigned_monthly_visual_cost_limit_usd";
    public const string DesignTierAuthorityMetadataKey = "design_tier_authority";
    public const string MonthlyVisualCostLimitAuthorityMetadataKey = "monthly_visual_cost_limit_authority";
    public const string UserIdentityAuthorityMetadataKey = "user_identity_authority";
    public const string PlatformTenantConfigurationAuthority = "platform_tenant_configuration";
    public const string VerifiedInboundPrincipalAuthority = "verified_inbound_principal";
    public const string TrustedVisualCostEntitlementRequiredErrorCode =
        "DESIGN_CONVERSION_TRUSTED_VISUAL_COST_ENTITLEMENT_REQUIRED";
    public const string TrustedVisualCostEntitlementInvalidErrorCode =
        "DESIGN_CONVERSION_TRUSTED_VISUAL_COST_ENTITLEMENT_INVALID";
    public const string TrustedVisualCostEntitlementMismatchErrorCode =
        "DESIGN_CONVERSION_TRUSTED_VISUAL_COST_ENTITLEMENT_MISMATCH";
    public const string UnexpectedRequestFailureErrorCode =
        "DESIGN_CONVERSION_MODEL_SERVER_API_UNEXPECTED";
    public const string UnexpectedRequestFailureErrorMessage =
        "DesignConversion model-server API request failed with an internal error.";
    public const string UnexpectedStatusFailureErrorCode =
        "DESIGN_CONVERSION_MODEL_SERVER_STATUS_UNEXPECTED";
    public const string UnexpectedStatusFailureErrorMessage =
        "DesignConversion status/readback request failed with an internal error.";
    private const decimal MaximumTrustedMonthlyVisualCostLimitUsd = 1_000_000m;
    private const int DefaultApiMaxRenderQaRuns = 4;
    private const int MaxVisualFidelityIssueCount = 1000;
    private const string CancelledErrorCode = "CANCELLED";
    private const string SubmitCancelledErrorMessage = "DesignConversion model-server API request cancelled.";
    private const string StatusCancelledErrorMessage = "DesignConversion status/readback request cancelled.";
    private const string ThinClientRuntimeExceptionErrorCode = "THIN_CLIENT_JOB_RUNTIME_EXCEPTION";
    private const string LocalArtifactDbStatusRestoreExceptionErrorCode = "LOCAL_ARTIFACT_DB_STATUS_RESTORE_EXCEPTION";
    private const string VisualDecisionMetadataStatusSource = "visual_decision_metadata";
    private const string FailClosedVisualDecisionFallbackStatusSource = "fail_closed_visual_decision_fallback";
    private const string MissingVisualDecisionRestoreToken = "missing";
    private const string BlankVisualDecisionRestoreToken = "blank";
    private const string UnknownVisualDecisionRestoreToken = "unknown";
    private static readonly IReadOnlyList<string> ServerComputedReadinessMarkers =
    [
        "browser_e2e_verified",
        "persistence_verified",
        "visual_fidelity_verified"
    ];
    private static readonly IReadOnlySet<string> ServerOwnedNonAuthorityProvenanceFacts =
        new HashSet<string>(StringComparer.Ordinal)
        {
            ServerOwnedRouteAdmissionTenantBoundProvenanceKey,
            DesignConversionThinClientJobRuntime.ServerOwnedRequiredBrowserQaMatrixProvenanceKey,
            DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractSuppliedMetadataKey,
            DesignConversionImprovementLoopCoordinator.MaxTeacherCallsRuntimeEnforcementMetadataKey,
            DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCallCapEnforcedMetadataKey,
            DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCapReachedMetadataKey,
            "trusted_visual_cost_entitlement_bound",
            "visual_cost_ledger_measurement_complete",
            "visual_cost_ledger_persistence_verified",
            "local_artifact_db_status_restored",
            "local_artifact_db_status_restore_visual_decision_metadata_accepted",
            "local_artifact_db_status_restore_fail_closed_visual_decision_fallback",
            "local_artifact_db_status_restore_legacy_completed_fallback",
            "status_restore_visual_decision_metadata_accepted",
            "status_restore_fail_closed_visual_decision_fallback",
            "status_restore_legacy_completed_fallback",
            "local_artifact_db_persistence_verified",
            "storage_training_artifact_db",
            "local_artifact_db",
            "external_ai_visual_judge_executed",
            "runtime_created_dpo_candidate",
            "runtime_created_dpo_candidate_review_pending",
            "independent_preference_proven",
            "review_required",
            "approval_required",
            "training_candidate_only",
            "not_training_grade",
            "high_quality_negative_pair_not_proven",
            "held_out_not_proven",
            "source_disjoint_not_proven",
            "product_not_ready",
            "post_evaluation_finalization_bounded",
            "post_evaluation_finalization_dependency_may_complete_late",
            // Серверный факт без права на власть: гейт проверки репозиторного
            // договора обучающих пар относится только к ВКЛЮЧЁННЫМ маршрутам, и
            // у отключённого договора отклонять нечего. Режим несёт парная
            // метка "включён", она правдиво False. Заявкой об обученном выводе
            // эта метка не является, поэтому детектор подделки её не судит.
            DesignConversionRepositoryOwnedDpoTransactionContext
                .ValidationMetadataKey,
            DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey
        };

    private readonly DesignConversionThinClientJobRuntime _runtime;
    private readonly IDesignConversionLocalArtifactDb? _artifactDb;
    private readonly CapturingThinClientJobEvaluator? _capture;
    private readonly RecordingRuntimeRejectedEvaluationSource? _rejectedEvaluationResolutionCapture;
    private readonly DesignConversionDpoUsefulnessApi? _trainingReadbackApi;
    private readonly DesignConversionPromotedLocalModel? _promotedLocalModel;
    private readonly IDesignConversionModelServerArtifactRefResolver? _artifactRefResolver;
    private readonly IDesignConversionActivatedModelRuntimeReadback? _activatedModelRuntimeReadback;
    private readonly IDesignConversionVerifiedActivatedModelRuntimeReadback? _verifiedActivatedModelRuntimeReadback;
    private readonly PromotedLocalModelVerificationProof? _promotedLocalModelVerificationProof;
    private readonly DesignConversionHumanApprovalService _humanApprovalService;
    private readonly IDesignConversionVisualCostBudgetStore? _visualCostBudgetStore;

    public DesignConversionThinClientJobRuntime AccountingReconciliationRuntime =>
        _runtime;

    public DesignConversionModelServerApi(
        DesignConversionInternalService service,
        Func<DateTimeOffset>? utcNow = null,
        DesignConversionDpoUsefulnessApi? trainingReadbackApi = null,
        DesignConversionPromotedLocalModel? promotedLocalModel = null,
        IDesignConversionModelServerArtifactRefResolver? artifactRefResolver = null,
        IDesignConversionActivatedModelRuntimeReadback? activatedModelRuntimeReadback = null,
        IDesignConversionHumanApprovalStore? humanApprovalStore = null,
        IDesignConversionVisualCostLedger? visualCostLedger = null,
        IDesignConversionVisualCostBudgetStore? visualCostBudgetStore = null,
        IDesignConversionPostEvaluationStateStore? postEvaluationStateStore = null)
        : this(
            new DesignConversionInternalServiceThinClientJobEvaluator(service),
            utcNow,
            trainingReadbackApi,
            promotedLocalModel,
            artifactRefResolver,
            activatedModelRuntimeReadback,
            humanApprovalStore,
            visualCostLedger,
            visualCostBudgetStore,
            postEvaluationStateStore: postEvaluationStateStore)
    {
    }

    public DesignConversionModelServerApi(
        IDesignConversionThinClientJobEvaluator evaluator,
        Func<DateTimeOffset>? utcNow = null,
        DesignConversionDpoUsefulnessApi? trainingReadbackApi = null,
        DesignConversionPromotedLocalModel? promotedLocalModel = null,
        IDesignConversionModelServerArtifactRefResolver? artifactRefResolver = null,
        IDesignConversionActivatedModelRuntimeReadback? activatedModelRuntimeReadback = null,
        IDesignConversionHumanApprovalStore? humanApprovalStore = null,
        IDesignConversionVisualCostLedger? visualCostLedger = null,
        IDesignConversionVisualCostBudgetStore? visualCostBudgetStore = null,
        IDesignConversionImprovementCandidateStrategy? improvementCandidateStrategy = null,
        IDesignConversionPostEvaluationStateStore? postEvaluationStateStore = null)
        : this(
            evaluator,
            utcNow,
            artifactDb: null,
            trainingReadbackApi,
            promotedLocalModel,
            artifactRefResolver,
            activatedModelRuntimeReadback,
            humanApprovalStore,
            visualCostLedger,
            visualCostBudgetStore,
            improvementCandidateStrategy,
            postEvaluationStateStore: postEvaluationStateStore)
    {
    }

    public DesignConversionModelServerApi(
        IDesignConversionThinClientJobEvaluator evaluator,
        Func<DateTimeOffset>? utcNow,
        IDesignConversionLocalArtifactDb? artifactDb,
        DesignConversionDpoUsefulnessApi? trainingReadbackApi = null,
        DesignConversionPromotedLocalModel? promotedLocalModel = null,
        IDesignConversionModelServerArtifactRefResolver? artifactRefResolver = null,
        IDesignConversionActivatedModelRuntimeReadback? activatedModelRuntimeReadback = null,
        IDesignConversionHumanApprovalStore? humanApprovalStore = null,
        IDesignConversionVisualCostLedger? visualCostLedger = null,
        IDesignConversionVisualCostBudgetStore? visualCostBudgetStore = null,
        IDesignConversionImprovementCandidateStrategy? improvementCandidateStrategy = null,
        IDesignConversionPostEvaluationStateStore? postEvaluationStateStore = null)
        : this(
            evaluator,
            utcNow,
            artifactDb,
            trainingReadbackApi,
            promotedLocalModel,
            artifactRefResolver,
            activatedModelRuntimeReadback,
            humanApprovalStore,
            visualCostLedger,
            visualCostBudgetStore,
            improvementCandidateStrategy,
            promotedLocalModelVerificationProof: null,
            verifiedActivatedModelRuntimeReadback: null,
            postEvaluationStateStore: postEvaluationStateStore)
    {
    }

    private DesignConversionModelServerApi(
        IDesignConversionThinClientJobEvaluator evaluator,
        Func<DateTimeOffset>? utcNow,
        IDesignConversionLocalArtifactDb? artifactDb,
        DesignConversionDpoUsefulnessApi? trainingReadbackApi,
        DesignConversionPromotedLocalModel? promotedLocalModel,
        IDesignConversionModelServerArtifactRefResolver? artifactRefResolver,
        IDesignConversionActivatedModelRuntimeReadback? activatedModelRuntimeReadback,
        IDesignConversionHumanApprovalStore? humanApprovalStore,
        IDesignConversionVisualCostLedger? visualCostLedger,
        IDesignConversionVisualCostBudgetStore? visualCostBudgetStore,
        IDesignConversionImprovementCandidateStrategy? improvementCandidateStrategy,
        PromotedLocalModelVerificationProof? promotedLocalModelVerificationProof,
        IDesignConversionVerifiedActivatedModelRuntimeReadback? verifiedActivatedModelRuntimeReadback,
        IDesignConversionPostEvaluationStateStore? postEvaluationStateStore = null)
    {
        var composedCandidateStrategy = artifactDb is null
            ? improvementCandidateStrategy
            : new DesignConversionDpoTeacherAuthoringCandidateStrategy(
                improvementCandidateStrategy ??
                    new DesignConversionRuntimeImprovementCandidateStrategy(),
                artifactDb);
        var loopComposition = DesignConversionImprovementLoopRuntimeCompositionFactory.Create(
            evaluator,
            composedCandidateStrategy,
            artifactDb,
            utcNow);
        _rejectedEvaluationResolutionCapture = new RecordingRuntimeRejectedEvaluationSource(
            loopComposition.RejectedEvaluationSource);
        _capture = new CapturingThinClientJobEvaluator(
            loopComposition.Evaluator);
        _visualCostBudgetStore = visualCostBudgetStore;
        _runtime = new DesignConversionThinClientJobRuntime(
            _capture,
            utcNow,
            artifactDb,
            visualCostLedger,
            visualCostBudgetStore,
            postEvaluationStateStore: postEvaluationStateStore,
            rejectedEvaluationSource: _rejectedEvaluationResolutionCapture);
        _artifactDb = artifactDb;
        _trainingReadbackApi = trainingReadbackApi;
        _promotedLocalModel = promotedLocalModel;
        _artifactRefResolver = artifactRefResolver;
        _activatedModelRuntimeReadback = activatedModelRuntimeReadback;
        _verifiedActivatedModelRuntimeReadback = verifiedActivatedModelRuntimeReadback;
        _promotedLocalModelVerificationProof = promotedLocalModelVerificationProof;
        _humanApprovalService = new DesignConversionHumanApprovalService(humanApprovalStore, utcNow);
    }

    public DesignConversionModelServerApi(
        DesignConversionThinClientJobRuntime runtime,
        DesignConversionDpoUsefulnessApi? trainingReadbackApi = null,
        DesignConversionPromotedLocalModel? promotedLocalModel = null,
        IDesignConversionModelServerArtifactRefResolver? artifactRefResolver = null,
        IDesignConversionActivatedModelRuntimeReadback? activatedModelRuntimeReadback = null,
        IDesignConversionHumanApprovalStore? humanApprovalStore = null,
        Func<DateTimeOffset>? utcNow = null)
    {
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        _artifactDb = runtime.LocalArtifactDb;
        _rejectedEvaluationResolutionCapture = null;
        _visualCostBudgetStore = runtime.VisualCostBudgetStore;
        _trainingReadbackApi = trainingReadbackApi;
        _promotedLocalModel = promotedLocalModel;
        _artifactRefResolver = artifactRefResolver;
        _activatedModelRuntimeReadback = activatedModelRuntimeReadback;
        _verifiedActivatedModelRuntimeReadback = null;
        _promotedLocalModelVerificationProof = null;
        _humanApprovalService = new DesignConversionHumanApprovalService(humanApprovalStore, utcNow);
    }

    public static DesignConversionModelServerApi CreateMachineBackedLocal(
        DesignConversionMachine machine,
        IDesignConversionRenderProvider renderProvider,
        IDesignConversionComparisonProvider comparisonProvider,
        Func<DateTimeOffset>? utcNow = null,
        IDesignConversionVisualJudge? visualJudge = null,
        DesignConversionDpoUsefulnessApi? trainingReadbackApi = null,
        DesignConversionPromotedLocalModel? promotedLocalModel = null,
        IDesignConversionModelServerArtifactRefResolver? artifactRefResolver = null,
        IDesignConversionActivatedModelRuntimeReadback? activatedModelRuntimeReadback = null,
        IDesignConversionHumanApprovalStore? humanApprovalStore = null,
        IDesignConversionVisualCostLedger? visualCostLedger = null,
        IDesignConversionVisualCostBudgetStore? visualCostBudgetStore = null,
        IDesignConversionPostEvaluationStateStore? postEvaluationStateStore = null)
    {
        if (machine is null)
            throw new ArgumentNullException(nameof(machine));
        if (renderProvider is null)
            throw new ArgumentNullException(nameof(renderProvider));
        if (comparisonProvider is null)
            throw new ArgumentNullException(nameof(comparisonProvider));

        var service = new DesignConversionInternalService(
            renderProvider,
            comparisonProvider,
            visualJudge,
            machine);

        return new DesignConversionModelServerApi(
            service,
            utcNow,
            trainingReadbackApi,
            promotedLocalModel,
            artifactRefResolver,
            activatedModelRuntimeReadback,
            humanApprovalStore,
            visualCostLedger,
            visualCostBudgetStore,
            postEvaluationStateStore);
    }

    internal static DesignConversionModelServerApi CreateForProductionActivatedRuntime(
        DesignConversionActivatedModelRuntimeEvaluator activatedEvaluator,
        Func<DateTimeOffset>? utcNow,
        IDesignConversionLocalArtifactDb? artifactDb,
        DesignConversionDpoUsefulnessApi? trainingReadbackApi,
        DesignConversionPromotedLocalModel? promotedLocalModel,
        IDesignConversionModelServerArtifactRefResolver? artifactRefResolver,
        IDesignConversionHumanApprovalStore? humanApprovalStore,
        IDesignConversionVisualCostLedger? visualCostLedger,
        IDesignConversionVisualCostBudgetStore? visualCostBudgetStore)
    {
        ArgumentNullException.ThrowIfNull(activatedEvaluator);

        return new DesignConversionModelServerApi(
            activatedEvaluator,
            utcNow,
            artifactDb,
            trainingReadbackApi,
            promotedLocalModel,
            artifactRefResolver,
            activatedEvaluator,
            humanApprovalStore,
            visualCostLedger,
            visualCostBudgetStore,
            improvementCandidateStrategy: null,
            promotedLocalModelVerificationProof: null,
            verifiedActivatedModelRuntimeReadback: activatedEvaluator);
    }

    public static async Task<OperationResult<DesignConversionModelServerPromotedLocalApiFactoryResult>>
        CreateFromTrainingIntakeLocalPromotionAsync(
            DesignConversionTrainingIntakeLocalPromotionResult promotion,
            IDesignConversionRenderProvider renderProvider,
            IDesignConversionComparisonProvider comparisonProvider,
            Func<DateTimeOffset>? utcNow = null,
            IDesignConversionVisualJudge? visualJudge = null,
            DesignConversionDpoUsefulnessApi? trainingReadbackApi = null,
            IDesignConversionLocalArtifactDb? artifactDb = null,
            CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<DesignConversionModelServerPromotedLocalApiFactoryResult>.Fail(
                "DesignConversion model-server promoted local API factory cancelled.",
                "CANCELLED");

        if (promotion is null)
            return OperationResult<DesignConversionModelServerPromotedLocalApiFactoryResult>.Fail(
                "DesignConversion training-intake local promotion result is required.",
                "DESIGN_CONVERSION_LOCAL_PROMOTION_RESULT_REQUIRED");
        if (renderProvider is null)
            return OperationResult<DesignConversionModelServerPromotedLocalApiFactoryResult>.Fail(
                "DesignConversion promoted local API requires a render provider.",
                "DESIGN_CONVERSION_RENDER_PROVIDER_REQUIRED");
        if (comparisonProvider is null)
            return OperationResult<DesignConversionModelServerPromotedLocalApiFactoryResult>.Fail(
                "DesignConversion promoted local API requires a comparison provider.",
                "DESIGN_CONVERSION_COMPARISON_PROVIDER_REQUIRED");
        if (!promotion.LocalPromotionReady)
            return OperationResult<DesignConversionModelServerPromotedLocalApiFactoryResult>.Fail(
                "DesignConversion promoted local API requires a local promotion-ready training intake result.",
                "DESIGN_CONVERSION_LOCAL_PROMOTION_READY_REQUIRED");
        if (promotion.Publication is null)
            return OperationResult<DesignConversionModelServerPromotedLocalApiFactoryResult>.Fail(
                "DesignConversion promoted local API requires a local publication receipt.",
                "DESIGN_CONVERSION_LOCAL_PROMOTION_PUBLICATION_REQUIRED");
        if (promotion.PromotedLocalModel is null)
            return OperationResult<DesignConversionModelServerPromotedLocalApiFactoryResult>.Fail(
                "DesignConversion promoted local API requires promoted local model metadata.",
                "DESIGN_CONVERSION_LOCAL_PROMOTED_MODEL_REQUIRED");
        if (string.IsNullOrWhiteSpace(promotion.PromotedCheckpointPath))
            return OperationResult<DesignConversionModelServerPromotedLocalApiFactoryResult>.Fail(
                "DesignConversion promoted local API requires a promoted checkpoint path.",
                "DESIGN_CONVERSION_LOCAL_PROMOTED_CHECKPOINT_PATH_REQUIRED");

        var load = await DesignConversionPromotedLocalModelLoader.LoadAsync(
            promotion.Publication,
            promotion.PromotedCheckpointPath,
            ct);
        if (!load.IsSuccess)
            return load.MapError<DesignConversionModelServerPromotedLocalApiFactoryResult>();

        var promotedLoad = load.Value!;
        var service = DesignConversionPromotedLocalModelLoader.CreateInternalService(
            promotedLoad,
            renderProvider,
            comparisonProvider,
            visualJudge);
        var api = new DesignConversionModelServerApi(
            new DesignConversionInternalServiceThinClientJobEvaluator(service),
            utcNow,
            artifactDb,
            trainingReadbackApi,
            promotedLoad.Model,
            artifactRefResolver: null,
            activatedModelRuntimeReadback: null,
            humanApprovalStore: null,
            visualCostLedger: null,
            visualCostBudgetStore: null,
            improvementCandidateStrategy: null,
            promotedLocalModelVerificationProof: PromotedLocalModelVerificationProof.FromFreshPromotionLoad(
                promotedLoad.Model),
            verifiedActivatedModelRuntimeReadback: null);
        var evidenceRefs = BuildPromotedLocalApiFactoryEvidenceRefs(promotion, promotedLoad);
        return OperationResult<DesignConversionModelServerPromotedLocalApiFactoryResult>.Ok(
            new DesignConversionModelServerPromotedLocalApiFactoryResult(api, promotedLoad, evidenceRefs));
    }

    public async Task<OperationResult<DesignConversionModelServerApiResult>> SubmitAndEvaluateAsync(
        DesignConversionModelServerApiRequest request,
        CancellationToken ct = default)
        => await SubmitAndEvaluateAsync(request, routeAdmissionContext: null, ct);

    public async Task<OperationResult<DesignConversionModelServerApiResult>> SubmitAndEvaluateAsync(
        DesignConversionModelServerApiRequest request,
        DesignConversionModelServerRouteAdmissionContext? routeAdmissionContext,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<DesignConversionModelServerApiResult>.Fail(
                SubmitCancelledErrorMessage,
                CancelledErrorCode);

        try
        {
            var validation = Validate(request, _artifactRefResolver);
            if (!validation.IsSuccess)
                return OperationResult<DesignConversionModelServerApiResult>.Fail(
                    validation.ErrorMessage,
                    validation.ErrorCode);

            var tenantContextValidation = ValidateActivatedRuntimeTenantContext(request, routeAdmissionContext);
            if (!tenantContextValidation.IsSuccess)
            {
                return OperationResult<DesignConversionModelServerApiResult>.Fail(
                    tenantContextValidation.ErrorMessage,
                    tenantContextValidation.ErrorCode);
            }

            var visualCostEntitlement = ValidateTrustedVisualCostEntitlement(request, routeAdmissionContext);
            if (!visualCostEntitlement.IsSuccess)
            {
                return OperationResult<DesignConversionModelServerApiResult>.Fail(
                    visualCostEntitlement.ErrorMessage,
                    visualCostEntitlement.ErrorCode);
            }

            var submitted = ToSubmittedRequest(
                request,
                routeAdmissionContext,
                visualCostEntitlement.Value!.Entitlement,
                trustedVisualCostReservationStoreWired: _visualCostBudgetStore is not null,
                serverOwnedArtifactValidationSucceeded: _artifactRefResolver is not null);
            var submit = _runtime.Submit(submitted);
            if (!submit.IsSuccess)
                return submit.MapError<DesignConversionModelServerApiResult>();

            var run = await _runtime.RunAsync(submit.Value!.JobId, ct);
            if (!run.IsSuccess)
                return run.MapError<DesignConversionModelServerApiResult>();

            var status = run.Value!;
            DesignConversionInternalEvaluationResult? evaluation = null;
            _capture?.TryGetEvaluation(status.Job.JobId, out evaluation);
            if (RequiresDpoPersistenceButLocalArtifactDbMissing(status, evaluation))
            {
                return OperationResult<DesignConversionModelServerApiResult>.Fail(
                    DesignConversionThinClientJobRuntime.LocalArtifactDbDpoPersistenceSafeMessage,
                    DesignConversionThinClientJobRuntime.LocalArtifactDbDpoPersistenceRequiredErrorCode);
            }

            if (IsFailedRuntimeStatus(status.ContractStatus))
            {
                var errorCode = status.Provenance.TryGetValue("internal_evaluator_error_code", out var runtimeError)
                    ? runtimeError
                    : "DESIGN_CONVERSION_MODEL_SERVER_EVALUATION_FAILED";
                if (IsCancellationErrorCode(errorCode))
                    return OperationResult<DesignConversionModelServerApiResult>.Fail(
                        SubmitCancelledErrorMessage,
                        CancelledErrorCode);
                if (IsUnexpectedRuntimeFailureCode(errorCode))
                    return OperationResult<DesignConversionModelServerApiResult>.Fail(
                        UnexpectedRequestFailureErrorMessage,
                        UnexpectedRequestFailureErrorCode);

                return OperationResult<DesignConversionModelServerApiResult>.Fail(status.Message, errorCode);
            }

            var promotedLocalModel = ResolvePromotedLocalModel(status.Job.JobId);
            var training = await BuildTrainingReadinessAsync(
                status,
                request.RequestId,
                promotedLocalModel,
                ct);
            var persistence = await VerifyLocalArtifactPersistenceAsync(status, evaluation, ct);
            return OperationResult<DesignConversionModelServerApiResult>.Ok(
                BuildResult(status, evaluation, training, promotedLocalModel, persistence));
        }
        catch (OperationCanceledException ex) when (IsCancellationFromSuppliedToken(ex, ct))
        {
            return OperationResult<DesignConversionModelServerApiResult>.Fail(
                SubmitCancelledErrorMessage,
                CancelledErrorCode);
        }
        catch (Exception ex) when (SnapshotExceptionGraphClassifier.IsNonFatal(ex))
        {
            return OperationResult<DesignConversionModelServerApiResult>.Fail(
                UnexpectedRequestFailureErrorMessage,
                UnexpectedRequestFailureErrorCode);
        }
    }

    private static bool IsFailedRuntimeStatus(string status)
        => string.Equals(status, DesignConversionThinClientStatus.Failed, StringComparison.Ordinal) ||
           string.Equals(status, DesignConversionThinClientStatus.Cancelled, StringComparison.Ordinal);

    private static bool IsCancellationErrorCode(string? errorCode)
        => string.Equals(errorCode, CancelledErrorCode, StringComparison.Ordinal);

    private static bool IsUnexpectedRuntimeFailureCode(string? errorCode)
        => string.Equals(errorCode, ThinClientRuntimeExceptionErrorCode, StringComparison.Ordinal);

    private static bool IsUnexpectedStatusRestoreFailureCode(string? errorCode)
        => string.Equals(errorCode, LocalArtifactDbStatusRestoreExceptionErrorCode, StringComparison.Ordinal) ||
           string.Equals(errorCode, UnexpectedStatusFailureErrorCode, StringComparison.Ordinal);

    private bool RequiresDpoPersistenceButLocalArtifactDbMissing(
        DesignConversionThinClientStatusResult status,
        DesignConversionInternalEvaluationResult? evaluation)
    {
        if (_artifactDb is not null || evaluation is null)
            return false;

        var warnings = status.Warnings
            .Concat(evaluation.Warnings)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var trustedRejectedEvaluationResolved =
            _rejectedEvaluationResolutionCapture?.HasResolved(status.Job.JobId) == true;
        return evaluation.EvidenceRefs.Contains(
                   DesignConversionImprovementLoopThinClientJobEvaluator.SubmitPathLoopExecutedMarker,
                   StringComparer.Ordinal) &&
               trustedRejectedEvaluationResolved &&
               evaluation.VisualQaBoundaryTrace.VisualDecision.CanPersistTrainingCandidate &&
               warnings.Contains(
                   $"runtime_teacher_attestation_error_code={DesignConversionDpoTeacherAttestationRuntime.RuntimeUnavailableErrorCode}",
                   StringComparer.Ordinal);
    }

    private static bool IsCancellationFromSuppliedToken(
        OperationCanceledException exception,
        CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
            return false;

        return !exception.CancellationToken.CanBeCanceled ||
               exception.CancellationToken.Equals(cancellationToken);
    }

    private static IReadOnlyList<string> BuildPromotedLocalApiFactoryEvidenceRefs(
        DesignConversionTrainingIntakeLocalPromotionResult promotion,
        DesignConversionPromotedLocalModelLoadResult promotedLoad)
    {
        var publicationDecisionEvidence = promotion.Publication?.Decision.EvidenceRefs ?? Array.Empty<string>();
        var publicationReceiptEvidence = promotion.Publication?.UploadReceipt?.EvidenceRefs ?? Array.Empty<string>();

        return promotion.EvidenceRefs
            .Concat(publicationDecisionEvidence)
            .Concat(publicationReceiptEvidence)
            .Concat(promotedLoad.Model.EvidenceRefs)
            .Concat([
                "DESIGN_CONVERSION_MODEL_SERVER_API_CREATED_FROM_TRAINING_INTAKE_LOCAL_PROMOTION",
                "DESIGN_CONVERSION_MODEL_SERVER_API_FRESH_LOADED_PROMOTED_LOCAL_CHECKPOINT",
                "DESIGN_CONVERSION_MODEL_SERVER_API_LOCAL_PROMOTION_RUNTIME_PRODUCT_READY_FALSE",
                "MODEL_SERVER_UPLOAD_NOT_ATTEMPTED_LOCAL_RECEIPT",
                "MODEL_SERVER_PROMOTION_NOT_CLAIMED_LOCAL_RECEIPT"
            ])
            .Where(static evidenceRef => !string.IsNullOrWhiteSpace(evidenceRef))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    public OperationResult<DesignConversionModelServerStatusResponse> GetJobResult(string jobId)
        => GetJobResultAsync(jobId).GetAwaiter().GetResult();

    public async Task<OperationResult<DesignConversionModelServerStatusResponse>> GetJobResultAsync(
        string jobId,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<DesignConversionModelServerStatusResponse>.Fail(
                StatusCancelledErrorMessage,
                CancelledErrorCode);

        try
        {
            var status = _runtime.GetStatus(jobId);
            if (!status.IsSuccess)
            {
                var restored = await TryRestoreStatusFromLocalArtifactDbAsync(jobId, ct);
                if (!restored.IsSuccess)
                {
                    if (IsCancellationErrorCode(restored.ErrorCode))
                        return OperationResult<DesignConversionModelServerStatusResponse>.Fail(
                            StatusCancelledErrorMessage,
                            CancelledErrorCode);
                    if (IsUnexpectedStatusRestoreFailureCode(restored.ErrorCode))
                        return OperationResult<DesignConversionModelServerStatusResponse>.Fail(
                            UnexpectedStatusFailureErrorMessage,
                            UnexpectedStatusFailureErrorCode);

                    return status.MapError<DesignConversionModelServerStatusResponse>();
                }

                status = restored;
            }

            var runtimeStatus = status.Value!;
            DesignConversionInternalEvaluationResult? evaluation = null;
            _capture?.TryGetEvaluation(runtimeStatus.Job.JobId, out evaluation);
            var promotedLocalModel = ResolvePromotedLocalModel(runtimeStatus.Job.JobId);
            var training = BuildTrainingReadinessNotConfigured(runtimeStatus, promotedLocalModel);
            var rawEvidenceRefs = BuildEvidenceRefs(runtimeStatus, evaluation, training.PromotedLocalModel)
                .Append(training.PromotedLocalModel is null
                    ? "DESIGN_CONVERSION_MODEL_SERVER_STATUS_NO_PROMOTED_LOCAL_MODEL_CONFIGURED"
                    : "DESIGN_CONVERSION_MODEL_SERVER_STATUS_PROMOTED_LOCAL_MODEL_IDENTIFIED")
                .Distinct(StringComparer.Ordinal)
                .OrderBy(reference => reference, StringComparer.Ordinal)
                .ToArray();
            var learnedApplied = LearnedInferenceApplied(evaluation);
            var deterministicClaim = evaluation?.DeterministicLogicClaimedAsLearning
                ?? IsTruthy(runtimeStatus.Provenance, "deterministic_logic_claimed_as_learning");
            var persistence = await VerifyLocalArtifactPersistenceAsync(runtimeStatus, evaluation, ct);
            var boundaryMetadata = BuildBoundaryMetadata(
                runtimeStatus,
                evaluation,
                learnedApplied,
                deterministicClaim,
                training,
                promotedLocalModel,
                rawEvidenceRefs,
                persistence);
            var modelAuthority = DesignConversionModelAuthorityBoundary.FromTrustedBoundaryMetadata(boundaryMetadata);
            var evidenceRefs = ReconcileBoundaryMarkers(rawEvidenceRefs, boundaryMetadata, sort: true);
            var publicRuntimeStatus = ReconcileRuntimeStatusBoundaryWarnings(runtimeStatus, boundaryMetadata);

            return OperationResult<DesignConversionModelServerStatusResponse>.Ok(new DesignConversionModelServerStatusResponse(
                ApiShape: ApiShape,
                JobId: runtimeStatus.Job.JobId,
                ContractStatus: runtimeStatus.ContractStatus,
                LearnedInferenceApplied: learnedApplied,
                TrainedModelAuthority: modelAuthority.TrainedModelAuthority,
                NotTrainedModelAuthority: modelAuthority.NotTrainedModelAuthority,
                DeterministicLogicClaimedAsLearning: deterministicClaim,
                PublicHttpEndpointVerified: false,
                DeploymentVerified: false,
                ProductReady: false,
                RuntimeStatus: publicRuntimeStatus,
                EvidenceRefs: evidenceRefs,
                BoundaryMetadata: boundaryMetadata,
                PromotedLocalModel: promotedLocalModel.Model));
        }
        catch (OperationCanceledException ex) when (IsCancellationFromSuppliedToken(ex, ct))
        {
            return OperationResult<DesignConversionModelServerStatusResponse>.Fail(
                StatusCancelledErrorMessage,
                CancelledErrorCode);
        }
        catch (Exception ex) when (SnapshotExceptionGraphClassifier.IsNonFatal(ex))
        {
            return OperationResult<DesignConversionModelServerStatusResponse>.Fail(
                UnexpectedStatusFailureErrorMessage,
                UnexpectedStatusFailureErrorCode);
        }
    }

    public async Task<OperationResult<DesignConversionModelServerVisualCostRecordsResponse>> GetVisualCostRecordsAsync(
        string jobId,
        CancellationToken ct = default)
    {
        var records = await _runtime.GetVisualCostRecordsAsync(jobId, ct);
        if (!records.IsSuccess)
            return records.MapError<DesignConversionModelServerVisualCostRecordsResponse>();

        var safeRecords = records.Value ?? [];
        var summary = BuildVisualCostSummary(safeRecords);
        var metadata = BuildVisualCostBoundaryMetadata(summary);
        return OperationResult<DesignConversionModelServerVisualCostRecordsResponse>.Ok(
            new DesignConversionModelServerVisualCostRecordsResponse(
                ApiShape: ApiShape,
                RuntimeSurface: RuntimeSurface,
                Summary: summary,
                Records: safeRecords,
                BoundaryMetadata: metadata));
    }

    private static DesignConversionModelServerVisualCostSummary BuildVisualCostSummary(
        IReadOnlyList<DesignConversionVisualCostLedgerRecord> records)
    {
        var costSources = records
            .Select(record => string.IsNullOrWhiteSpace(record.CostSource)
                ? DesignConversionVisualCostAccounting.UnknownCostSource
                : record.CostSource.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var costSource = costSources.Length switch
        {
            0 => DesignConversionVisualCostAccounting.UnknownCostSource,
            1 => costSources[0],
            _ => DesignConversionVisualCostAccounting.MixedCostSource
        };

        return new DesignConversionModelServerVisualCostSummary(
            RecordCount: records.Count,
            TotalCostUsd: records.Sum(record => record.TotalCostUsd),
            MeasurementComplete: records.Count > 0 && records.All(record => record.MeasurementComplete),
            MissingMeasurementCount: records.Sum(record => Math.Max(0, record.MissingMeasurementCount)),
            EvaluationAttemptCount: records.Sum(record => Math.Max(0, record.EvaluationAttemptCount)),
            CostSource: costSource);
    }

    private static IReadOnlyDictionary<string, string> BuildVisualCostBoundaryMetadata(
        DesignConversionModelServerVisualCostSummary summary)
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["runtime_visual_cost_records_readback_invoked"] = bool.TrueString,
            ["runtime_surface"] = RuntimeSurface,
            ["api_shape"] = ApiShape,
            ["visual_cost_record_count"] = summary.RecordCount.ToString(CultureInfo.InvariantCulture),
            ["visual_cost_total_cost_usd"] = summary.TotalCostUsd.ToString("G29", CultureInfo.InvariantCulture),
            ["visual_cost_measurement_complete"] = summary.MeasurementComplete.ToString(),
            ["visual_cost_missing_measurement_count"] = summary.MissingMeasurementCount.ToString(CultureInfo.InvariantCulture),
            ["visual_cost_evaluation_attempt_count"] = summary.EvaluationAttemptCount.ToString(CultureInfo.InvariantCulture),
            ["visual_cost_source"] = summary.CostSource,
            ["visual_cost_response_refs_only"] = bool.TrueString,
            ["visual_cost_unknown_job_empty_success"] = (summary.RecordCount == 0).ToString(),
            ["raw_job_id_included"] = bool.FalseString,
            ["raw_request_id_included"] = bool.FalseString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString,
            ["filesystem_root_included"] = bool.FalseString,
            ["sensitive_principal_data_included"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };

    private async Task<OperationResult<DesignConversionThinClientStatusResult>> TryRestoreStatusFromLocalArtifactDbAsync(
        string jobId,
        CancellationToken ct)
    {
        if (_artifactDb is null)
        {
            return OperationResult<DesignConversionThinClientStatusResult>.Fail(
                $"DesignConversion thin-client job '{jobId}' was not found.",
                "JOB_NOT_FOUND");
        }

        OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>> readback;
        try
        {
            readback = await _artifactDb.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery(JobId: jobId),
                ct);
        }
        catch (OperationCanceledException ex) when (IsCancellationFromSuppliedToken(ex, ct))
        {
            return OperationResult<DesignConversionThinClientStatusResult>.Fail(
                "DesignConversion status/readback local artifact DB restore was cancelled.",
                CancelledErrorCode);
        }
        catch (Exception ex) when (SnapshotExceptionGraphClassifier.IsNonFatal(ex))
        {
            return OperationResult<DesignConversionThinClientStatusResult>.Fail(
                UnexpectedStatusFailureErrorMessage,
                LocalArtifactDbStatusRestoreExceptionErrorCode);
        }

        if (!readback.IsSuccess)
        {
            return OperationResult<DesignConversionThinClientStatusResult>.Fail(
                readback.ErrorMessage ?? "DesignConversion status/readback local artifact DB restore failed.",
                readback.ErrorCode ?? "LOCAL_ARTIFACT_DB_STATUS_RESTORE_FAILED");
        }

        var record = (readback.Value ?? [])
            .Where(record => string.Equals(record.JobId, jobId, StringComparison.Ordinal))
            .OrderByDescending(record => record.CreatedAtUtc)
            .ThenBy(record => record.RecordId, StringComparer.Ordinal)
            .FirstOrDefault();
        if (record is null)
        {
            return OperationResult<DesignConversionThinClientStatusResult>.Fail(
                $"DesignConversion thin-client job '{jobId}' was not found.",
                "JOB_NOT_FOUND");
        }

        return OperationResult<DesignConversionThinClientStatusResult>.Ok(
            BuildStatusFromLocalArtifactDbRecord(record));
    }

    private static DesignConversionThinClientStatusResult BuildStatusFromLocalArtifactDbRecord(
        DesignConversionLocalArtifactDbRecord record)
    {
        var hasCompletedStatusSnapshot = record.TryGetCompletedStatusSnapshot(
            out var completedStatusSnapshot);
        var restoredStatus = ResolveLocalArtifactDbRestoredStatus(record);
        var metadata = new Dictionary<string, string>(record.Metadata ?? new Dictionary<string, string>(), StringComparer.Ordinal)
        {
            ["status"] = restoredStatus.Status,
            ["source_kind"] = record.SourceKind,
            ["budget_tier"] = record.Tier,
            ["local_artifact_db_status_restored"] = bool.TrueString,
            ["local_artifact_db_status_restore_boundary"] = "server_owned_local_artifact_db_restart_readback",
            ["local_artifact_db_status_restore_visual_decision"] = restoredStatus.VisualDecision,
            ["local_artifact_db_status_restore_status_source"] = restoredStatus.Source,
            ["local_artifact_db_status_restore_visual_decision_metadata_accepted"] =
                restoredStatus.UsedVisualDecisionMetadata.ToString(CultureInfo.InvariantCulture),
            ["local_artifact_db_status_restore_fail_closed_visual_decision_fallback"] =
                restoredStatus.UsedFailClosedVisualDecisionFallback.ToString(CultureInfo.InvariantCulture),
            ["local_artifact_db_status_restore_legacy_completed_fallback"] =
                bool.FalseString,
            ["local_artifact_db_persistence_verified"] = bool.TrueString,
            ["storage_training_artifact_db"] = bool.TrueString,
            ["local_artifact_db"] = bool.TrueString,
            ["local_artifact_db_completed_status_snapshot_restored"] =
                hasCompletedStatusSnapshot.ToString(CultureInfo.InvariantCulture),
            ["public_api_verified"] = bool.FalseString,
            ["persistence_verified"] = bool.FalseString,
            ["product_ready_verified"] = bool.FalseString,
            ["runtime_ready_verified"] = bool.FalseString,
            ["trained_feature_verified"] = bool.FalseString
        };
        var job = new DesignConversionThinClientJob(
            JobId: record.JobId,
            RequestId: record.RequestId,
            Status: restoredStatus.Status,
            StatusUri: $"memory://design-conversion/jobs/{Uri.EscapeDataString(record.JobId)}/status",
            SubmittedAtUtc: record.CreatedAtUtc,
            UpdatedAtUtc: record.CreatedAtUtc,
            Metadata: metadata);
        var provenance = new Dictionary<string, string>(metadata, StringComparer.Ordinal)
        {
            ["submitted_request_contract"] = nameof(SubmittedDesignConversionRequest),
            ["job_contract"] = nameof(DesignConversionThinClientJob),
            ["status_contract"] = nameof(DesignConversionThinClientStatusResult),
            ["runtime_adapter"] = nameof(DesignConversionModelServerApi),
            ["runtime_boundary"] = DesignConversionThinClientJobRuntime.RuntimeBoundary,
            ["storage_boundary"] = "server_owned_local_artifact_db_restart_readback",
            ["status_restore_source"] = JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind,
            ["status_restore_record_id"] = record.RecordId,
            ["status_restore_executed_runtime"] = bool.FalseString,
            ["status_restore_contract_status"] = restoredStatus.Status,
            ["status_restore_visual_decision"] = restoredStatus.VisualDecision,
            ["status_restore_status_source"] = restoredStatus.Source,
            ["status_restore_visual_decision_metadata_accepted"] =
                restoredStatus.UsedVisualDecisionMetadata.ToString(CultureInfo.InvariantCulture),
            ["status_restore_fail_closed_visual_decision_fallback"] =
                restoredStatus.UsedFailClosedVisualDecisionFallback.ToString(CultureInfo.InvariantCulture),
            ["status_restore_legacy_completed_fallback"] =
                bool.FalseString,
            ["status_restore_completed_status_snapshot"] = hasCompletedStatusSnapshot
                ? "validated_domain_snapshot"
                : "unavailable_fallback"
        };
        var warnings = SanitizeStatusBoundaryValues(record.EvidenceRefs, "restored_status_warning")
            .Concat([
                "local_artifact_db_status_restored=true",
                "local_artifact_db_status_restore_boundary=server_owned_local_artifact_db_restart_readback",
                "runtime_reexecution_invoked=false",
                $"local_artifact_db_status_restore_contract_status={restoredStatus.Status}",
                $"local_artifact_db_status_restore_visual_decision={restoredStatus.VisualDecision}",
                $"local_artifact_db_status_restore_status_source={restoredStatus.Source}",
                $"local_artifact_db_status_restore_visual_decision_metadata_accepted={restoredStatus.UsedVisualDecisionMetadata.ToString(CultureInfo.InvariantCulture)}",
                $"local_artifact_db_status_restore_fail_closed_visual_decision_fallback={restoredStatus.UsedFailClosedVisualDecisionFallback.ToString(CultureInfo.InvariantCulture)}",
                $"local_artifact_db_status_restore_legacy_completed_fallback={bool.FalseString}",
                hasCompletedStatusSnapshot
                    ? "local_artifact_db_completed_status_snapshot_restored=true"
                    : "local_artifact_db_completed_status_snapshot_restored=false",
                "product_ready=false"
            ])
            .Distinct(StringComparer.Ordinal)
            .OrderBy(warning => warning, StringComparer.Ordinal)
            .ToArray();

        return new DesignConversionThinClientStatusResult(
            Job: job,
            ContractStatus: restoredStatus.Status,
            Message: "DesignConversion thin-client job status restored from the server-owned local artifact DB after process restart.",
            Artifacts: BuildStatusArtifactsFromLocalArtifactDbRecord(record),
            Warnings: warnings,
            Provenance: provenance)
        {
            QualityMetrics = completedStatusSnapshot?.QualityMetrics ??
                DesignConversionThinClientStatusQualityMetrics.Unavailable(
                    source: "not_persisted_after_restart",
                    unavailableReason: "quality_metrics_not_persisted_after_restart"),
            Defects = completedStatusSnapshot?.Defects ?? []
        };
    }

    private static LocalArtifactDbRestoredStatus ResolveLocalArtifactDbRestoredStatus(
        DesignConversionLocalArtifactDbRecord record)
    {
        if (record.VisualJudgeStatus?.Metadata is not { } visualJudgeMetadata ||
            !visualJudgeMetadata.TryGetValue("visual_decision", out var rawVisualDecision))
        {
            return LocalArtifactDbRestoredStatus.FailClosedVisualDecisionFallback(
                MissingVisualDecisionRestoreToken);
        }

        if (string.IsNullOrWhiteSpace(rawVisualDecision))
        {
            return LocalArtifactDbRestoredStatus.FailClosedVisualDecisionFallback(
                BlankVisualDecisionRestoreToken);
        }

        var visualDecision = rawVisualDecision.Trim();
        return DesignConversionThinClientStatus.TryFromKnownVisualQaDecision(visualDecision, out var status)
            ? LocalArtifactDbRestoredStatus.FromVisualDecision(status, visualDecision)
            : LocalArtifactDbRestoredStatus.FailClosedVisualDecisionFallback(
                UnknownVisualDecisionRestoreToken);
    }

    private sealed record LocalArtifactDbRestoredStatus(
        string Status,
        string VisualDecision,
        string Source,
        bool UsedVisualDecisionMetadata,
        bool UsedFailClosedVisualDecisionFallback)
    {
        public static LocalArtifactDbRestoredStatus FromVisualDecision(
            string status,
            string visualDecision)
            => new(
                Status: status,
                VisualDecision: visualDecision,
                Source: VisualDecisionMetadataStatusSource,
                UsedVisualDecisionMetadata: true,
                UsedFailClosedVisualDecisionFallback: false);

        public static LocalArtifactDbRestoredStatus FailClosedVisualDecisionFallback(string visualDecision)
            => new(
                Status: DesignConversionThinClientStatus.NotReady,
                VisualDecision: visualDecision,
                Source: FailClosedVisualDecisionFallbackStatusSource,
                UsedVisualDecisionMetadata: false,
                UsedFailClosedVisualDecisionFallback: true);
    }

    private static IReadOnlyList<DesignConversionThinClientArtifactRef> BuildStatusArtifactsFromLocalArtifactDbRecord(
        DesignConversionLocalArtifactDbRecord record)
    {
        var artifacts = new List<DesignConversionThinClientArtifactRef>();
        if (!string.IsNullOrWhiteSpace(record.ConversionResult.HtmlRef) &&
            TryNormalizeSha256Ref(record.ConversionResult.HtmlSha256, out var htmlSha256))
        {
            artifacts.Add(new DesignConversionThinClientArtifactRef(
                ArtifactId: $"{record.JobId}:improved-html",
                Kind: "improved-html",
                Uri: record.ConversionResult.HtmlRef,
                Sha256: htmlSha256,
                MediaType: "text/html",
                Metadata: record.ConversionResult.Metadata));
        }

        if (!string.IsNullOrWhiteSpace(record.ConversionResult.CssRef) &&
            TryNormalizeSha256Ref(record.ConversionResult.CssSha256, out var cssSha256))
        {
            artifacts.Add(new DesignConversionThinClientArtifactRef(
                ArtifactId: $"{record.JobId}:improved-css",
                Kind: "improved-css",
                Uri: record.ConversionResult.CssRef,
                Sha256: cssSha256,
                MediaType: "text/css",
                Metadata: record.ConversionResult.Metadata));
        }

        foreach (var render in record.RenderArtifactRefs)
        {
            if (!TryNormalizeSha256Ref(render.Sha256, out var renderSha256))
                continue;

            artifacts.Add(new DesignConversionThinClientArtifactRef(
                ArtifactId: render.ArtifactId,
                Kind: render.Kind,
                Uri: render.UriRef,
                Sha256: renderSha256,
                MediaType: "image/png",
                Metadata: render.Metadata));
        }

        return artifacts;
    }

    private static bool TryNormalizeSha256Ref(string? value, out string sha256)
    {
        sha256 = string.Empty;
        if (!IsValidSha256(value))
            return false;

        var trimmed = value!.Trim();
        sha256 = trimmed.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase)
            ? "sha256:" + trimmed["sha256:".Length..].ToLowerInvariant()
            : "sha256:" + trimmed.ToLowerInvariant();
        return true;
    }

    public async Task<OperationResult<DesignConversionHumanApprovalChallenge>> IssueHumanApprovalChallengeAsync(
        DesignConversionHumanApprovalChallengeIssueRequest request,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<DesignConversionHumanApprovalChallenge>.Fail(
                "DesignConversion human approval challenge request cancelled.",
                "CANCELLED");
        if (request is null)
            return OperationResult<DesignConversionHumanApprovalChallenge>.Fail(
                "DesignConversion human approval challenge request is required.",
                "REQUEST_REQUIRED");

        var status = _runtime.GetStatus(request.JobId);
        if (!status.IsSuccess)
            return status.MapError<DesignConversionHumanApprovalChallenge>();

        DesignConversionInternalEvaluationResult? evaluation = null;
        _capture?.TryGetEvaluation(status.Value!.Job.JobId, out evaluation);
        return await _humanApprovalService.IssueChallengeAsync(request, status.Value!, evaluation, ct);
    }

    public async Task<OperationResult<DesignConversionHumanApprovalResult>> ApproveHumanConversionAsync(
        DesignConversionHumanApprovalDecisionRequest request,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<DesignConversionHumanApprovalResult>.Fail(
                "DesignConversion human approval request cancelled.",
                "CANCELLED");
        if (request is null)
            return OperationResult<DesignConversionHumanApprovalResult>.Fail(
                "DesignConversion human approval request is required.",
                "REQUEST_REQUIRED");

        var status = _runtime.GetStatus(request.JobId);
        if (!status.IsSuccess)
            return status.MapError<DesignConversionHumanApprovalResult>();

        DesignConversionInternalEvaluationResult? evaluation = null;
        _capture?.TryGetEvaluation(status.Value!.Job.JobId, out evaluation);
        return await _humanApprovalService.ApproveAsync(request, status.Value!, evaluation, ct);
    }

    public async Task<OperationResult<IReadOnlyList<DesignConversionHumanApprovalRecord>>> ListHumanApprovalRecordsAsync(
        CancellationToken ct = default)
        => await _humanApprovalService.ListApprovalRecordsAsync(ct);

    private static OperationResult<bool> Validate(
        DesignConversionModelServerApiRequest request,
        IDesignConversionModelServerArtifactRefResolver? artifactRefResolver)
    {
        if (request is null)
            return OperationResult<bool>.Fail("DesignConversion model-server API request is required.", "REQUEST_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.RequestId))
            return OperationResult<bool>.Fail("DesignConversion model-server request id is required.", "REQUEST_ID_REQUIRED");
        if (!HasSourcePayload(request.DesignTree))
            return OperationResult<bool>.Fail("DesignConversion source tree payload, ref, or hash is required.", "DESIGN_CONVERSION_SOURCE_TREE_REQUIRED");
        if (!HasSourcePayload(request.OriginalTree))
            return OperationResult<bool>.Fail("DesignConversion original tree payload, ref, or hash is required.", "DESIGN_CONVERSION_ORIGINAL_TREE_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.Html))
            return OperationResult<bool>.Fail("DesignConversion generated HTML is required.", "DESIGN_CONVERSION_HTML_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.Css))
            return OperationResult<bool>.Fail("DesignConversion generated CSS is required.", "DESIGN_CONVERSION_CSS_REQUIRED");

        var artifacts = (request.Screenshots ?? [])
            .Concat(request.DesignRenders ?? [])
            .ToArray();
        if (artifacts.Length == 0)
            return OperationResult<bool>.Fail("DesignConversion screenshot or expected render ref is required.", "DESIGN_CONVERSION_SCREENSHOT_OR_RENDER_REQUIRED");
        if (!artifacts.Any(artifact => artifact.Width > 0 && artifact.Height > 0))
            return OperationResult<bool>.Fail("DesignConversion screenshot/render refs must include at least one positive viewport.", "DESIGN_CONVERSION_VIEWPORT_REQUIRED");

        if (request.Budget is not null)
        {
            if (request.Budget.BudgetUsd < 0m)
                return OperationResult<bool>.Fail(
                    "DesignConversion improvement-loop budgetUsd must be greater than or equal to zero.",
                    DesignConversionInternalService.NegativeBudgetUsdErrorCode);

            if (!DesignConversionVisualQaBoundaryPolicy.IsValidConfiguredRenderQaPassThreshold(
                    request.Budget.RenderQaPassThreshold,
                    request.Budget.Tier))
            {
                return OperationResult<bool>.Fail(
                    "DesignConversion render QA pass threshold must be finite, greater than 0, at most 1, and not below the server-owned tier floor.",
                    DesignConversionVisualQaBoundaryPolicy.RenderQaPassThresholdInvalidErrorCode);
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
                    $"DesignConversion improvement-loop max attempts must be less than or equal to {DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts.ToString(CultureInfo.InvariantCulture)}.",
                    DesignConversionModelServerImprovementLoopHandler.MaxAttemptsExceededErrorCode);
            }
        }

        var artifactRefs = DesignConversionModelServerArtifactRefGuard.Validate(
            artifacts,
            "DesignConversion screenshot/render refs",
            artifactRefResolver);
        if (!artifactRefs.IsSuccess)
            return artifactRefs;

        var payloadSize = DesignConversionModelServerPayloadSizeGuard.ValidateApiRequest(
            request,
            "DesignConversion model-server API request");
        if (!payloadSize.IsSuccess)
            return payloadSize;

        return OperationResult<bool>.Ok(true);
    }

    private static bool HasSourcePayload(DesignConversionModelServerPayloadRef? source)
        => source is not null &&
           (!string.IsNullOrWhiteSpace(source.Payload) ||
            !string.IsNullOrWhiteSpace(source.RefId) ||
            !string.IsNullOrWhiteSpace(source.Sha256));

    private static SubmittedDesignConversionRequest ToSubmittedRequest(
        DesignConversionModelServerApiRequest request,
        DesignConversionModelServerRouteAdmissionContext? routeAdmissionContext,
        DesignConversionModelServerTrustedVisualCostEntitlement? visualCostEntitlement,
        bool trustedVisualCostReservationStoreWired,
        bool serverOwnedArtifactValidationSucceeded)
    {
        var redactions = new SortedSet<string>(StringComparer.Ordinal);
        var budget = request.Budget ?? new DesignConversionModelServerBudget(
            UserId: "local-user-redacted",
            Tier: "local",
            MaxImprovementCandidates: 1,
            MaxRenderQaRuns: DefaultApiMaxRenderQaRuns,
            BudgetUsd: 0m);
        var budgetMetadata = visualCostEntitlement is null
            ? SanitizeMetadata(budget.Metadata, "budget_metadata", redactions)
            : BuildTrustedVisualCostBudgetMetadata(
                budget.Metadata,
                visualCostEntitlement,
                trustedVisualCostReservationStoreWired,
                redactions);

        return new SubmittedDesignConversionRequest(
            RequestId: SanitizeScalar(request.RequestId, "request_id", redactions, fallback: "design-conversion-request"),
            SourceKind: request.SourceKind,
            Html: request.Html,
            Css: request.Css,
            DesignTreePayload: ToTreePayload(request.DesignTree, "design_tree", redactions),
            OriginalTreePayload: ToTreePayload(request.OriginalTree, "original_tree", redactions),
            Screenshots: (request.Screenshots ?? [])
                .Select(artifact => ToRenderArtifact(artifact, redactions, serverOwnedArtifactValidationSucceeded))
                .ToArray(),
            DesignRenders: (request.DesignRenders ?? [])
                .Select(artifact => ToRenderArtifact(artifact, redactions, serverOwnedArtifactValidationSucceeded))
                .ToArray(),
            Budget: new DesignConversionBudgetMetadata(
                UserId: SanitizeScalar(budget.UserId, "budget_user_id", redactions, fallback: "user-redacted"),
                Tier: SanitizeScalar(budget.Tier, "budget_tier", redactions, fallback: "local"),
                MaxImprovementCandidates: budget.MaxImprovementCandidates,
                MaxRenderQaRuns: Math.Max(1, budget.MaxRenderQaRuns),
                BudgetUsd: Math.Max(0m, budget.BudgetUsd),
                Metadata: budgetMetadata,
                RenderQaPassThreshold: budget.RenderQaPassThreshold,
                MaxImprovementIterations: budget.MaxImprovementIterations,
                MaxTeacherCalls: budget.MaxTeacherCalls),
            Provenance: BuildRequestProvenance(
                request,
                redactions,
                routeAdmissionContext,
                visualCostEntitlement,
                trustedVisualCostReservationStoreWired))
        {
            // WHY: this typed authority is not provenance text; the runtime must
            // receive the exact carrier so enabled repository-owned DPO reuse can
            // fail closed before any teacher inference call is issued.
            RepositoryOwnedDpoTransactionContext =
                request.RepositoryOwnedDpoTransactionContext
        };
    }

    private static DesignTreePayload ToTreePayload(
        DesignConversionModelServerPayloadRef source,
        string scope,
        SortedSet<string> redactions)
    {
        var payload = source.Payload ?? source.RefId ?? source.Sha256 ?? string.Empty;
        var sha = NormalizeSha256(source.Sha256, payload);
        return new DesignTreePayload(
            PayloadId: SanitizeScalar(source.RefId, $"{scope}_ref", redactions, fallback: $"{scope}:{sha}"),
            Format: SanitizeScalar(source.Format, $"{scope}_format", redactions, fallback: source.Kind),
            Content: payload,
            Sha256: sha,
            Metadata: SanitizeMetadata(source.Metadata, $"{scope}_metadata", redactions));
    }

    private static DesignRenderArtifact ToRenderArtifact(
        DesignConversionModelServerArtifactRef artifact,
        SortedSet<string> redactions,
        bool serverOwnedArtifactValidationSucceeded)
        => new(
            ArtifactId: SanitizeScalar(artifact.ArtifactId, "artifact_id", redactions, fallback: "artifact"),
            Kind: SanitizeScalar(artifact.Kind, "artifact_kind", redactions, fallback: "screenshot"),
            Width: artifact.Width,
            Height: artifact.Height,
            Uri: SanitizeArtifactUri(
                artifact.Uri,
                artifact.Sha256,
                redactions,
                serverOwnedArtifactValidationSucceeded),
            Sha256: NormalizeSha256(artifact.Sha256, artifact.Uri),
            Metadata: SanitizeArtifactMetadata(artifact.Metadata, "artifact_metadata", redactions));

    private static string SanitizeArtifactUri(
        string? uri,
        string? sha256,
        SortedSet<string> redactions,
        bool serverOwnedArtifactValidationSucceeded)
    {
        if (serverOwnedArtifactValidationSucceeded &&
            IsLocalFileArtifactUri(uri) &&
            TryNormalizeSha256Ref(sha256, out var normalizedSha256))
        {
            redactions.Add("artifact_uri:server_owned_local_file_uri_mapped_to_content_sha256_ref");
            return "local-png-ref:" + normalizedSha256;
        }

        return SanitizeScalar(
            uri,
            "artifact_uri",
            redactions,
            fallback: NormalizeSha256(sha256, uri));
    }

    private static bool IsLocalFileArtifactUri(string? uri)
    {
        if (string.IsNullOrWhiteSpace(uri))
            return false;

        var trimmed = uri.Trim();
        if (Path.IsPathFullyQualified(trimmed))
            return true;

        return Uri.TryCreate(trimmed, UriKind.Absolute, out var parsed) &&
               parsed.IsFile &&
               string.IsNullOrWhiteSpace(parsed.Host) &&
               !parsed.IsUnc;
    }

    private static IReadOnlyDictionary<string, string> SanitizeArtifactMetadata(
        IReadOnlyDictionary<string, string>? metadata,
        string scope,
        SortedSet<string> redactions)
    {
        var sanitized = SanitizeMetadata(metadata, scope, redactions);
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        var quarantined = new SortedSet<string>(StringComparer.Ordinal);

        foreach (var pair in sanitized)
        {
            if (IsCallerSuppliedTrustMetadataKey(pair.Key))
            {
                quarantined.Add(pair.Key);
                result[$"caller_artifact_metadata_ref:{pair.Key}"] =
                    DesignConversionDpoLeakageGuard.HashRef($"{scope}:{pair.Key}", pair.Value);
                continue;
            }

            result[pair.Key] = pair.Value;
        }

        if (quarantined.Count > 0)
        {
            result["caller_artifact_trust_metadata_quarantined"] = bool.TrueString;
            result["caller_artifact_trust_metadata_quarantined_keys"] = string.Join(",", quarantined);
        }

        return result;
    }

    private static bool IsCallerSuppliedTrustMetadataKey(string key)
        => key is "provider_kind"
            or "render_artifact_kind"
            or "visual_fidelity_claimed"
            or "browser_or_playwright_used"
            or "external_ai_used"
            or "hidden_determinism_boundary"
            or "browser_screenshot_file_verified"
            or "browser_screenshot_sha256_verified"
            or "browser_screenshot_nonblank_pixels_verified"
            or "actual_png_bytes_written"
            or "product_ready"
            or "promotion_ready"
            or "local_promotion_ready"
            or "model_training_verified"
            or "ModelUploadReady"
            or "learned_inference_applied"
            or "LearnedInferenceApplied"
            or "server_trained";

    private static IReadOnlyDictionary<string, string> BuildRequestProvenance(
        DesignConversionModelServerApiRequest request,
        SortedSet<string> redactions,
        DesignConversionModelServerRouteAdmissionContext? routeAdmissionContext,
        DesignConversionModelServerTrustedVisualCostEntitlement? visualCostEntitlement,
        bool trustedVisualCostReservationStoreWired)
    {
        var provenance = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["api_shape"] = ApiShape,
            ["runtime_surface"] = RuntimeSurface,
            ["submission_mapper"] = nameof(DesignConversionModelServerApi),
            ["design_conversion_runtime"] = nameof(DesignConversionThinClientJobRuntime),
            ["design_conversion_internal_service"] = nameof(DesignConversionInternalService),
            ["http_wiring_boundary"] = HttpWiringBoundary,
            ["public_http_endpoint_verified"] = bool.FalseString,
            ["platform_integration_verified"] = bool.FalseString,
            ["deployment_verified"] = bool.FalseString,
            ["product_ready"] = bool.FalseString,
            [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractSuppliedMetadataKey] = request.Budget?.MaxTeacherCalls.HasValue == true
                ? bool.TrueString
                : bool.FalseString,
            [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractValueMetadataKey] = request.Budget?.MaxTeacherCalls.HasValue == true
                ? request.Budget.MaxTeacherCalls.Value.ToString(CultureInfo.InvariantCulture)
                : "not_supplied",
            [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractBoundaryMetadataKey] =
                DesignConversionVisualQaBoundaryPolicy.MaxTeacherCallsContractBoundary,
            [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsRuntimeEnforcementMetadataKey] = bool.FalseString,
            [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCallCapEnforcedMetadataKey] = bool.FalseString,
            [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsObservedCallCountMetadataKey] = "0",
            [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCapReachedMetadataKey] = bool.FalseString,
            ["raw_html_included_in_response"] = bool.FalseString,
            ["raw_css_included_in_response"] = bool.FalseString,
            ["raw_design_tree_included_in_response"] = bool.FalseString,
            [CallerTenantMetadataAuthoritativeProvenanceKey] = bool.FalseString,
            [DesignConversionThinClientJobRuntime.ServerOwnedRequiredBrowserQaMatrixProvenanceKey] =
                (request.Budget is null).ToString()
        };

        var admittedTenant = routeAdmissionContext is { TenantBound: true }
            ? DesignConversionModelActivationRegistry.TryBuildTenantIdentity(routeAdmissionContext.TenantId)
            : null;
        if (admittedTenant?.IsSuccess == true &&
            string.Equals(admittedTenant.Value!.TenantIdRef, routeAdmissionContext!.TenantIdRef, StringComparison.Ordinal))
        {
            provenance[ServerOwnedRouteAdmissionTenantBoundProvenanceKey] = bool.TrueString;
            provenance[ServerOwnedRouteAdmissionTenantIdProvenanceKey] = admittedTenant.Value.TenantId;
            provenance[ServerOwnedRouteAdmissionTenantIdRefProvenanceKey] = admittedTenant.Value.TenantIdRef;
            provenance[ServerOwnedRouteAdmissionScopeRefProvenanceKey] = routeAdmissionContext!.ScopeRef;
            provenance["active_model_runtime_tenant_context_source"] = "server_owned_route_admission";
        }
        else
        {
            provenance[ServerOwnedRouteAdmissionTenantBoundProvenanceKey] = bool.FalseString;
            provenance[ServerOwnedRouteAdmissionTenantIdRefProvenanceKey] = string.Empty;
            provenance[ServerOwnedRouteAdmissionScopeRefProvenanceKey] = string.Empty;
            provenance["active_model_runtime_tenant_context_source"] = "missing_or_disabled_route_admission";
        }

        if (visualCostEntitlement is not null)
        {
            provenance["trusted_visual_cost_entitlement_bound"] = bool.TrueString;
            provenance["trusted_visual_cost_entitlement_authority"] = PlatformTenantConfigurationAuthority;
            provenance["trusted_visual_cost_entitlement_user_identity_authority"] =
                visualCostEntitlement.UserIdentityAuthority;
            provenance["trusted_visual_cost_entitlement_user_ref"] = visualCostEntitlement.UserIdRef;
            provenance["trusted_visual_cost_entitlement_tenant_id_ref"] = visualCostEntitlement.TenantIdRef;
            provenance["trusted_visual_cost_entitlement_design_tier"] = visualCostEntitlement.DesignTier;
            provenance["trusted_visual_cost_entitlement_monthly_limit_usd"] =
                visualCostEntitlement.MonthlyVisualCostLimitUsdText;
            provenance["trusted_visual_cost_entitlement_period_kind"] = visualCostEntitlement.PeriodKind;
            provenance["trusted_visual_cost_entitlement_raw_user_exposed"] = bool.FalseString;
            provenance["trusted_visual_cost_reservation_store_wired"] =
                trustedVisualCostReservationStoreWired.ToString();
        }
        else
        {
            provenance["trusted_visual_cost_entitlement_bound"] = bool.FalseString;
            provenance["trusted_visual_cost_reservation_store_wired"] = bool.FalseString;
        }

        foreach (var pair in SanitizeMetadata(request.Metadata, "request_metadata", redactions))
            provenance[$"request_metadata:{pair.Key}"] = pair.Value;
        foreach (var pair in SanitizeMetadata(request.Budget?.Metadata, "budget_metadata", redactions))
            provenance[$"budget_metadata:{pair.Key}"] = pair.Value;
        if (redactions.Count > 0)
            provenance["request_metadata_redactions"] = string.Join("|", redactions);

        return provenance;
    }

    private static OperationResult<bool> ValidateActivatedRuntimeTenantContext(
        DesignConversionModelServerApiRequest request,
        DesignConversionModelServerRouteAdmissionContext? routeAdmissionContext)
    {
        var callerTenantClaim = ResolveCallerTenantIdClaim(request);
        if (!callerTenantClaim.IsSuccess)
            return callerTenantClaim.MapError<bool>();

        if (routeAdmissionContext is not { TenantBound: true })
            return OperationResult<bool>.Ok(true);

        var admittedTenant = DesignConversionModelActivationRegistry.TryBuildTenantIdentity(routeAdmissionContext.TenantId);
        if (!admittedTenant.IsSuccess ||
            string.IsNullOrWhiteSpace(routeAdmissionContext.TenantIdRef) ||
            !string.Equals(admittedTenant.Value!.TenantIdRef, routeAdmissionContext.TenantIdRef, StringComparison.Ordinal))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion active model trusted tenant context is invalid.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_TRUSTED_TENANT_CONTEXT_INVALID");
        }

        if (string.IsNullOrWhiteSpace(callerTenantClaim.Value))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion active model tenant claim is required for admitted route context.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_TRUSTED_TENANT_CONTEXT_INVALID");
        }

        if (!string.IsNullOrWhiteSpace(callerTenantClaim.Value) &&
            !string.Equals(callerTenantClaim.Value, admittedTenant.Value.TenantId, StringComparison.Ordinal))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion active model tenant claim does not match the admitted tenant context.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_TENANT_CONTEXT_MISMATCH");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<string> ResolveCallerTenantIdClaim(DesignConversionModelServerApiRequest request)
    {
        var claims = EnumerateCallerTenantIdClaims(request)
            .ToArray();
        if (claims.Length == 0)
            return OperationResult<string>.Ok(string.Empty);

        string? canonicalTenantId = null;
        foreach (var claim in claims)
        {
            var tenant = DesignConversionModelActivationRegistry.TryCanonicalizeTenantId(claim.Value);
            if (!tenant.IsSuccess)
            {
                return OperationResult<string>.Fail(
                    "DesignConversion active model tenant claim is invalid.",
                    "DESIGN_CONVERSION_ACTIVE_MODEL_TRUSTED_TENANT_CONTEXT_INVALID");
            }

            canonicalTenantId = tenant.Value!;
        }

        if (claims.Length > 1)
        {
            return OperationResult<string>.Fail(
                "DesignConversion active model tenant claim is ambiguous.",
                "DESIGN_CONVERSION_ACTIVE_MODEL_TRUSTED_TENANT_CONTEXT_INVALID");
        }

        return OperationResult<string>.Ok(canonicalTenantId!);
    }

    private static IEnumerable<TrustedMetadataOccurrence> EnumerateCallerTenantIdClaims(
        DesignConversionModelServerApiRequest request)
    {
        foreach (var claim in EnumerateCallerTenantIdClaims(request.Metadata))
            yield return claim;
        foreach (var claim in EnumerateCallerTenantIdClaims(request.Budget?.Metadata))
            yield return claim;
    }

    private static IEnumerable<TrustedMetadataOccurrence> EnumerateCallerTenantIdClaims(
        IReadOnlyDictionary<string, string>? metadata)
    {
        foreach (var pair in metadata ?? new Dictionary<string, string>())
        {
            if (IsCallerTenantClaimKey(pair.Key))
                yield return new TrustedMetadataOccurrence(pair.Key, pair.Value);
        }
    }

    private static bool IsCallerTenantClaimKey(string key)
        => string.Equals(key, "tenant_id", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(key, "tenantId", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(key, "tenant", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(key, "x-tenant-id", StringComparison.OrdinalIgnoreCase);

    private static OperationResult<TrustedVisualCostEntitlementValidation> ValidateTrustedVisualCostEntitlement(
        DesignConversionModelServerApiRequest request,
        DesignConversionModelServerRouteAdmissionContext? routeAdmissionContext)
    {
        if (routeAdmissionContext is not { TenantBound: true })
        {
            if (ContainsTrustedVisualCostCallerContractMetadata(request))
            {
                return OperationResult<TrustedVisualCostEntitlementValidation>.Fail(
                    "DesignConversion trusted visual-cost entitlement metadata requires tenant-bound route admission.",
                    TrustedVisualCostEntitlementInvalidErrorCode);
            }

            return OperationResult<TrustedVisualCostEntitlementValidation>.Ok(new TrustedVisualCostEntitlementValidation(null));
        }

        if (request.Budget is null)
        {
            return OperationResult<TrustedVisualCostEntitlementValidation>.Fail(
                "DesignConversion trusted visual-cost entitlement requires authenticated route budget metadata.",
                TrustedVisualCostEntitlementRequiredErrorCode);
        }

        var admittedTenant = DesignConversionModelActivationRegistry.TryBuildTenantIdentity(routeAdmissionContext.TenantId);
        if (!admittedTenant.IsSuccess ||
            string.IsNullOrWhiteSpace(routeAdmissionContext.TenantIdRef) ||
            !string.Equals(admittedTenant.Value!.TenantIdRef, routeAdmissionContext.TenantIdRef, StringComparison.Ordinal))
        {
            return OperationResult<TrustedVisualCostEntitlementValidation>.Fail(
                "DesignConversion trusted visual-cost entitlement tenant context is invalid.",
                TrustedVisualCostEntitlementInvalidErrorCode);
        }

        var userId = ValidateTrustedVisualCostUserId(request.Budget.UserId);
        if (!userId.IsSuccess)
            return userId.MapError<TrustedVisualCostEntitlementValidation>();

        var tier = ReadSingleTrustedMetadataValue(request, ServerAssignedDesignTierMetadataKey);
        if (!tier.IsSuccess)
            return tier.MapError<TrustedVisualCostEntitlementValidation>();
        if (!IsCanonicalTrustedDesignTier(tier.Value!.Value))
        {
            return OperationResult<TrustedVisualCostEntitlementValidation>.Fail(
                "DesignConversion trusted visual-cost entitlement tier is invalid.",
                TrustedVisualCostEntitlementInvalidErrorCode);
        }

        var cap = ReadSingleTrustedMetadataValue(request, ServerAssignedMonthlyVisualCostLimitUsdMetadataKey);
        if (!cap.IsSuccess)
            return cap.MapError<TrustedVisualCostEntitlementValidation>();
        if (!TryParseCanonicalTrustedMonthlyVisualCostLimitUsd(cap.Value!.Value, out var monthlyLimitUsd))
        {
            return OperationResult<TrustedVisualCostEntitlementValidation>.Fail(
                "DesignConversion trusted visual-cost entitlement monthly limit is invalid.",
                TrustedVisualCostEntitlementInvalidErrorCode);
        }

        var tierAuthority = ReadSingleTrustedMetadataValue(request, DesignTierAuthorityMetadataKey);
        if (!tierAuthority.IsSuccess)
            return tierAuthority.MapError<TrustedVisualCostEntitlementValidation>();
        if (!string.Equals(tierAuthority.Value!.Value, PlatformTenantConfigurationAuthority, StringComparison.Ordinal))
        {
            return OperationResult<TrustedVisualCostEntitlementValidation>.Fail(
                "DesignConversion trusted visual-cost entitlement tier authority is invalid.",
                TrustedVisualCostEntitlementInvalidErrorCode);
        }

        var capAuthority = ReadSingleTrustedMetadataValue(request, MonthlyVisualCostLimitAuthorityMetadataKey);
        if (!capAuthority.IsSuccess)
            return capAuthority.MapError<TrustedVisualCostEntitlementValidation>();
        if (!string.Equals(capAuthority.Value!.Value, PlatformTenantConfigurationAuthority, StringComparison.Ordinal))
        {
            return OperationResult<TrustedVisualCostEntitlementValidation>.Fail(
                "DesignConversion trusted visual-cost entitlement monthly limit authority is invalid.",
                TrustedVisualCostEntitlementInvalidErrorCode);
        }

        var userAuthority = ReadSingleTrustedMetadataValue(request, UserIdentityAuthorityMetadataKey);
        if (!userAuthority.IsSuccess)
            return userAuthority.MapError<TrustedVisualCostEntitlementValidation>();
        if (!string.Equals(userAuthority.Value!.Value, VerifiedInboundPrincipalAuthority, StringComparison.Ordinal))
        {
            return OperationResult<TrustedVisualCostEntitlementValidation>.Fail(
                "DesignConversion trusted visual-cost entitlement user authority is invalid.",
                TrustedVisualCostEntitlementInvalidErrorCode);
        }

        if (!string.Equals(request.Budget.Tier, tier.Value.Value, StringComparison.Ordinal))
        {
            return OperationResult<TrustedVisualCostEntitlementValidation>.Fail(
                "DesignConversion trusted visual-cost entitlement tier does not match the submitted budget tier.",
                TrustedVisualCostEntitlementMismatchErrorCode);
        }

        var budgetMetadata = ValidateTrustedBudgetMetadataEchoes(
            request.Budget.Metadata,
            tier.Value.Value,
            cap.Value.Value);
        if (!budgetMetadata.IsSuccess)
            return budgetMetadata.MapError<TrustedVisualCostEntitlementValidation>();

        var entitlement = new DesignConversionModelServerTrustedVisualCostEntitlement(
            UserIdRef: DesignConversionDpoLeakageGuard.HashRef("design-conversion-visual-cost-user", userId.Value!),
            TenantIdRef: admittedTenant.Value.TenantIdRef,
            DesignTier: tier.Value.Value,
            MonthlyVisualCostLimitUsd: monthlyLimitUsd,
            MonthlyVisualCostLimitUsdText: cap.Value.Value,
            DesignTierAuthority: PlatformTenantConfigurationAuthority,
            MonthlyVisualCostLimitAuthority: PlatformTenantConfigurationAuthority,
            UserIdentityAuthority: VerifiedInboundPrincipalAuthority,
            PeriodKind: "utc_calendar_month");
        return OperationResult<TrustedVisualCostEntitlementValidation>.Ok(
            new TrustedVisualCostEntitlementValidation(entitlement));
    }

    private static OperationResult<string> ValidateTrustedVisualCostUserId(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return OperationResult<string>.Fail(
                "DesignConversion trusted visual-cost entitlement user identity is required.",
                TrustedVisualCostEntitlementRequiredErrorCode);
        }

        if (!string.Equals(userId, userId.Trim(), StringComparison.Ordinal) ||
            userId.Length > 256 ||
            userId.Any(char.IsControl) ||
            LooksLikeRawPayloadOrPrivateToken(userId) ||
            userId.Contains("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return OperationResult<string>.Fail(
                "DesignConversion trusted visual-cost entitlement user identity is invalid.",
                TrustedVisualCostEntitlementInvalidErrorCode);
        }

        return OperationResult<string>.Ok(userId);
    }

    private static OperationResult<TrustedMetadataValue> ReadSingleTrustedMetadataValue(
        DesignConversionModelServerApiRequest request,
        string key)
    {
        var requestMatches = EnumerateTrustedMetadataOccurrences(request.Metadata, key).ToArray();
        var budgetMatches = EnumerateTrustedMetadataOccurrences(request.Budget?.Metadata, key).ToArray();
        if (budgetMatches.Length > 0)
        {
            return OperationResult<TrustedMetadataValue>.Fail(
                "DesignConversion trusted visual-cost entitlement metadata contains caller budget reserved fields.",
                TrustedVisualCostEntitlementInvalidErrorCode);
        }

        if (requestMatches.Length == 0)
        {
            return OperationResult<TrustedMetadataValue>.Fail(
                "DesignConversion trusted visual-cost entitlement metadata is required.",
                TrustedVisualCostEntitlementRequiredErrorCode);
        }

        if (requestMatches.Length > 1)
        {
            return OperationResult<TrustedMetadataValue>.Fail(
                "DesignConversion trusted visual-cost entitlement metadata contains duplicate reserved fields.",
                TrustedVisualCostEntitlementInvalidErrorCode);
        }

        var match = requestMatches[0];
        if (!string.Equals(match.Key, key, StringComparison.Ordinal))
        {
            return OperationResult<TrustedMetadataValue>.Fail(
                "DesignConversion trusted visual-cost entitlement metadata key casing is invalid.",
                TrustedVisualCostEntitlementInvalidErrorCode);
        }

        var value = match.Value;
        if (string.IsNullOrEmpty(value) ||
            string.IsNullOrWhiteSpace(value) ||
            !string.Equals(value, value.Trim(), StringComparison.Ordinal))
        {
            return OperationResult<TrustedMetadataValue>.Fail(
                "DesignConversion trusted visual-cost entitlement metadata value is invalid.",
                TrustedVisualCostEntitlementInvalidErrorCode);
        }

        return OperationResult<TrustedMetadataValue>.Ok(new TrustedMetadataValue(value));
    }

    private static IEnumerable<TrustedMetadataOccurrence> EnumerateTrustedMetadataOccurrences(
        IReadOnlyDictionary<string, string>? metadata,
        string key)
    {
        foreach (var pair in metadata ?? new Dictionary<string, string>())
        {
            if (string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
                yield return new TrustedMetadataOccurrence(pair.Key, pair.Value);
        }
    }

    private static bool IsCanonicalTrustedDesignTier(string value)
        => value is "free" or "paid" or "pro" &&
           DesignConversionVisualQaBoundaryPolicy.IsServerOwnedEntitlementTier(value);

    private static bool TryParseCanonicalTrustedMonthlyVisualCostLimitUsd(string value, out decimal amount)
    {
        amount = 0m;
        if (string.IsNullOrEmpty(value) ||
            !string.Equals(value, value.Trim(), StringComparison.Ordinal) ||
            value.Contains("e", StringComparison.OrdinalIgnoreCase) ||
            value.Contains('+') ||
            value.Contains('-'))
        {
            return false;
        }

        var dotIndex = value.IndexOf('.', StringComparison.Ordinal);
        if (dotIndex != value.LastIndexOf('.'))
            return false;

        var integerPart = dotIndex < 0 ? value : value[..dotIndex];
        var fractionalPart = dotIndex < 0 ? string.Empty : value[(dotIndex + 1)..];
        if (integerPart.Length == 0 ||
            integerPart.Any(ch => ch is < '0' or > '9') ||
            (integerPart.Length > 1 && integerPart[0] == '0') ||
            fractionalPart.Length > 4 ||
            fractionalPart.Any(ch => ch is < '0' or > '9') ||
            (dotIndex >= 0 && fractionalPart.Length == 0))
        {
            return false;
        }

        if (!decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out amount))
            return false;

        return amount > 0m &&
               amount <= MaximumTrustedMonthlyVisualCostLimitUsd &&
               string.Equals(FormatTrustedVisualCostUsd(amount), value, StringComparison.Ordinal);
    }

    private static OperationResult<bool> ValidateTrustedBudgetMetadataEchoes(
        IReadOnlyDictionary<string, string>? metadata,
        string trustedTier,
        string trustedMonthlyLimitUsd)
    {
        foreach (var pair in metadata ?? new Dictionary<string, string>())
        {
            if (IsTrustedBudgetMonthlyCapMetadataKey(pair.Key) &&
                !string.Equals(pair.Value, trustedMonthlyLimitUsd, StringComparison.Ordinal))
            {
                return OperationResult<bool>.Fail(
                    "DesignConversion trusted visual-cost budget monthly limit metadata does not match the server-owned contract.",
                    TrustedVisualCostEntitlementMismatchErrorCode);
            }

            if (IsTrustedBudgetTierMetadataKey(pair.Key) &&
                !string.Equals(pair.Value, trustedTier, StringComparison.Ordinal))
            {
                return OperationResult<bool>.Fail(
                    "DesignConversion trusted visual-cost budget tier metadata does not match the server-owned contract.",
                    TrustedVisualCostEntitlementMismatchErrorCode);
            }
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool ContainsTrustedVisualCostCallerContractMetadata(DesignConversionModelServerApiRequest request)
        => ContainsTrustedVisualCostCallerContractMetadata(request.Metadata) ||
           ContainsTrustedVisualCostCallerContractMetadata(request.Budget?.Metadata);

    private static bool ContainsTrustedVisualCostCallerContractMetadata(IReadOnlyDictionary<string, string>? metadata)
    {
        foreach (var pair in metadata ?? new Dictionary<string, string>())
        {
            if (IsTrustedVisualCostCallerContractMetadataKey(pair.Key))
                return true;
        }

        return false;
    }

    private static bool IsTrustedVisualCostCallerContractMetadataKey(string key)
        => IsTrustedVisualCostReservedMetadataKey(key) ||
           IsTrustedBudgetMonthlyCapMetadataKey(key) ||
           IsTrustedBudgetTierMetadataKey(key) ||
           key.StartsWith("trusted_visual_cost_", StringComparison.OrdinalIgnoreCase);

    private static IReadOnlyDictionary<string, string> BuildTrustedVisualCostBudgetMetadata(
        IReadOnlyDictionary<string, string>? callerMetadata,
        DesignConversionModelServerTrustedVisualCostEntitlement entitlement,
        bool trustedVisualCostReservationStoreWired,
        SortedSet<string> redactions)
    {
        var sanitized = SanitizeMetadata(callerMetadata, "budget_metadata", redactions);
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal);
        var filteredKeys = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in sanitized)
        {
            if (IsTrustedVisualCostReservedMetadataKey(pair.Key) ||
                IsTrustedBudgetMonthlyCapMetadataKey(pair.Key) ||
                IsTrustedBudgetTierMetadataKey(pair.Key) ||
                IsRawUserMetadataKey(pair.Key))
            {
                filteredKeys.Add(pair.Key);
                continue;
            }

            metadata[pair.Key] = pair.Value;
        }

        metadata["trusted_visual_cost_entitlement_bound"] = bool.TrueString;
        metadata["trusted_visual_cost_entitlement_authority"] = PlatformTenantConfigurationAuthority;
        metadata["trusted_visual_cost_user_identity_authority"] = entitlement.UserIdentityAuthority;
        metadata["trusted_visual_cost_user_ref"] = entitlement.UserIdRef;
        metadata["trusted_visual_cost_tenant_id_ref"] = entitlement.TenantIdRef;
        metadata[ServerAssignedDesignTierMetadataKey] = entitlement.DesignTier;
        metadata[ServerAssignedMonthlyVisualCostLimitUsdMetadataKey] = entitlement.MonthlyVisualCostLimitUsdText;
        metadata[DesignTierAuthorityMetadataKey] = entitlement.DesignTierAuthority;
        metadata[MonthlyVisualCostLimitAuthorityMetadataKey] = entitlement.MonthlyVisualCostLimitAuthority;
        metadata[UserIdentityAuthorityMetadataKey] = entitlement.UserIdentityAuthority;
        metadata["trusted_visual_cost_period_kind"] = entitlement.PeriodKind;
        metadata["trusted_visual_cost_raw_user_exposed"] = bool.FalseString;
        metadata["trusted_visual_cost_reservation_store_wired"] =
            trustedVisualCostReservationStoreWired.ToString();

        if (filteredKeys.Count > 0)
        {
            metadata["caller_visual_cost_reserved_metadata_filtered"] = bool.TrueString;
            metadata["caller_visual_cost_reserved_metadata_filtered_keys"] = string.Join(",", filteredKeys);
        }

        return metadata;
    }

    private static bool IsTrustedVisualCostReservedMetadataKey(string key)
        => string.Equals(key, ServerAssignedDesignTierMetadataKey, StringComparison.OrdinalIgnoreCase) ||
           string.Equals(key, ServerAssignedMonthlyVisualCostLimitUsdMetadataKey, StringComparison.OrdinalIgnoreCase) ||
           string.Equals(key, DesignTierAuthorityMetadataKey, StringComparison.OrdinalIgnoreCase) ||
           string.Equals(key, MonthlyVisualCostLimitAuthorityMetadataKey, StringComparison.OrdinalIgnoreCase) ||
           string.Equals(key, UserIdentityAuthorityMetadataKey, StringComparison.OrdinalIgnoreCase);

    private static bool IsTrustedBudgetMonthlyCapMetadataKey(string key)
        => string.Equals(key, ServerAssignedMonthlyVisualCostLimitUsdMetadataKey, StringComparison.OrdinalIgnoreCase) ||
           string.Equals(key, "monthly_visual_cost_limit_usd", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(key, "monthly_visual_cost_cap_usd", StringComparison.OrdinalIgnoreCase);

    private static bool IsTrustedBudgetTierMetadataKey(string key)
        => string.Equals(key, ServerAssignedDesignTierMetadataKey, StringComparison.OrdinalIgnoreCase) ||
           string.Equals(key, "design_tier", StringComparison.OrdinalIgnoreCase);

    private static bool IsRawUserMetadataKey(string key)
        => string.Equals(key, "user_id", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(key, "userId", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(key, "user", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(key, "email", StringComparison.OrdinalIgnoreCase);

    private static string FormatTrustedVisualCostUsd(decimal value)
        => value.ToString("0.####", CultureInfo.InvariantCulture);

    private async Task<DesignConversionModelServerTrainingReadiness> BuildTrainingReadinessAsync(
        DesignConversionThinClientStatusResult status,
        string requestId,
        PromotedLocalModelResolution promotedLocalModel,
        CancellationToken ct)
    {
        if (_trainingReadbackApi is null)
            return BuildTrainingReadinessNotConfigured(status, promotedLocalModel);

        var readback = await _trainingReadbackApi.QueryLocalArtifactReadbackAsync(
            new DesignConversionLocalArtifactReadbackApiRequest(
                JobId: status.Job.JobId,
                RequestId: requestId,
                SourceKind: null,
                Limit: 1),
            ct);
        if (!readback.IsSuccess)
        {
            return ApplyPromotedLocalModelReadiness(new DesignConversionModelServerTrainingReadiness(
                Status: "local_artifact_readback_failed",
                RuntimeCreatedRecordCount: 0,
                DpoCandidateCount: 0,
                SourceDisjointStatus: "not_proven",
                MeasurementStatus: MeasurementStatus(status),
                PromotionStatus: "promotion_not_run",
                ModelTrainingVerified: false,
                ModelUploadReady: false,
                DeploymentVerified: false,
                ProductReady: false,
                Warnings:
                [
                    readback.ErrorCode ?? "LOCAL_ARTIFACT_READBACK_FAILED",
                    "model_training_verified=false",
                    "ModelUploadReady=false",
                    "deployment_verified=false"
                ]), promotedLocalModel);
        }

        var value = readback.Value!;
        return ApplyPromotedLocalModelReadiness(new DesignConversionModelServerTrainingReadiness(
            Status: "local_artifact_readback_available",
            RuntimeCreatedRecordCount: value.RuntimeCreatedRecordCount,
            DpoCandidateCount: value.DpoCandidateCount,
            SourceDisjointStatus: IsTruthy(value.BoundaryMetadata, "source_disjoint_not_proven")
                ? "not_proven"
                : "not_applicable_or_no_runtime_candidates",
            MeasurementStatus: MeasurementStatus(status),
            PromotionStatus: "promotion_not_run",
            ModelTrainingVerified: false,
            ModelUploadReady: false,
            DeploymentVerified: false,
            ProductReady: value.ProductReady,
            Warnings: value.Warnings
                .Concat([
                    "model_training_verified=false",
                    "ModelUploadReady=false",
                    "deployment_verified=false",
                    "promotion_not_run"
                ])
                .Distinct(StringComparer.Ordinal)
                .OrderBy(warning => warning, StringComparer.Ordinal)
                .ToArray()), promotedLocalModel);
    }

    private PromotedLocalModelResolution ResolvePromotedLocalModel(string jobId)
    {
        if (_verifiedActivatedModelRuntimeReadback is not null &&
            _verifiedActivatedModelRuntimeReadback.TryGetVerifiedPromotedLocalModel(
                jobId,
                out var verifiedActivatedModel))
        {
            return new PromotedLocalModelResolution(
                verifiedActivatedModel,
                PromotedLocalModelVerificationAuthority.ProductionActivatedRuntime);
        }

        if (_activatedModelRuntimeReadback is not null &&
            _activatedModelRuntimeReadback.TryGetPromotedLocalModel(jobId, out var activatedModel))
        {
            return new PromotedLocalModelResolution(
                activatedModel,
                PromotedLocalModelVerificationAuthority.None);
        }

        var authority =
            _promotedLocalModelVerificationProof is not null &&
            ReferenceEquals(_promotedLocalModelVerificationProof.Model, _promotedLocalModel)
                ? PromotedLocalModelVerificationAuthority.FreshPromotionLoad
                : PromotedLocalModelVerificationAuthority.None;
        return new PromotedLocalModelResolution(_promotedLocalModel, authority);
    }

    private static DesignConversionModelServerTrainingReadiness BuildTrainingReadinessNotConfigured(
        DesignConversionThinClientStatusResult status,
        PromotedLocalModelResolution promotedLocalModel)
        => ApplyPromotedLocalModelReadiness(new DesignConversionModelServerTrainingReadiness(
            Status: "local_artifact_readback_not_configured",
            RuntimeCreatedRecordCount: 0,
            DpoCandidateCount: 0,
            SourceDisjointStatus: "not_proven",
            MeasurementStatus: MeasurementStatus(status),
            PromotionStatus: "promotion_not_run",
            ModelTrainingVerified: false,
            ModelUploadReady: false,
            DeploymentVerified: false,
            ProductReady: false,
            Warnings:
            [
                "training_artifact_readback_not_configured",
                "model_training_verified=false",
                "ModelUploadReady=false",
                "deployment_verified=false",
                "promotion_not_run"
            ]), promotedLocalModel);

    private static DesignConversionModelServerTrainingReadiness ApplyPromotedLocalModelReadiness(
        DesignConversionModelServerTrainingReadiness readiness,
        PromotedLocalModelResolution promotedLocalModelResolution)
    {
        var promotedLocalModel = promotedLocalModelResolution.Model;
        if (promotedLocalModel is null)
            return readiness;

        var modelTrainingVerified =
            promotedLocalModelResolution.VerificationAuthorized &&
            promotedLocalModel.CheckpointFreshLoadVerified &&
            promotedLocalModel.CheckpointContainsRealTrainedWeights &&
            promotedLocalModel.PromotionManifestClaimed &&
            promotedLocalModel.LocalOnly &&
            promotedLocalModel.ReadinessEvaluable;
        var readinessBlockedBySourceDisjointHeldOut =
            promotedLocalModel.RequiresSourceDisjointHeldOutReadinessGate &&
            !promotedLocalModel.SourceDisjointHeldOutReadinessGatePassed;
        var sourceDisjointStatus = promotedLocalModel.SourceDisjointHeldOutReadinessGatePassed
            ? promotedLocalModel.SourceDisjointHeldOutReadinessStatus
            : readinessBlockedBySourceDisjointHeldOut
                ? promotedLocalModel.SourceDisjointHeldOutReadinessStatus
                : readiness.SourceDisjointStatus;

        return readiness with
        {
            Status = !promotedLocalModelResolution.VerificationAuthorized
                ? "promoted_local_model_informational_verification_not_proven"
                : readinessBlockedBySourceDisjointHeldOut
                ? "promoted_local_checkpoint_loaded_source_disjoint_heldout_not_proven"
                : "promoted_local_checkpoint_loaded",
            SourceDisjointStatus = sourceDisjointStatus,
            MeasurementStatus = readiness.MeasurementStatus,
            PromotionStatus = !promotedLocalModelResolution.VerificationAuthorized
                ? "local_promoted_model_informational_verification_not_proven"
                : readinessBlockedBySourceDisjointHeldOut
                ? "local_promoted_checkpoint_loaded_readiness_blocked_source_disjoint_heldout_not_proven"
                : "local_promoted_checkpoint_loaded",
            ModelTrainingVerified = modelTrainingVerified,
            ModelUploadReady = false,
            DeploymentVerified = false,
            ProductReady = false,
            Warnings = readiness.Warnings
                .Where(warning => !string.Equals(warning, "model_training_verified=false", StringComparison.Ordinal))
                .Concat([
                    $"model_training_verified={modelTrainingVerified}",
                    "ModelUploadReady=false",
                    "deployment_verified=false",
                    "product_ready=false",
                    promotedLocalModelResolution.VerificationAuthorized
                        ? "local_promoted_checkpoint_loaded"
                        : "local_promoted_checkpoint_load_not_verified",
                    $"local_model_promotion_verified={promotedLocalModelResolution.VerificationAuthorized}",
                    promotedLocalModelResolution.AuthorityMarker,
                    "cloud_model_upload_not_attempted",
                    $"source_disjoint_heldout_readiness_gate={promotedLocalModel.SourceDisjointHeldOutReadinessStatus}"
                ])
                .Concat(readinessBlockedBySourceDisjointHeldOut
                    ? [DesignConversionTrainingPipeline.SourceDisjointHeldOutReadinessGateNotProvenReason]
                    : [])
                .Distinct(StringComparer.Ordinal)
                .OrderBy(warning => warning, StringComparer.Ordinal)
                .ToArray(),
            PromotedLocalModel = promotedLocalModel
        };
    }

    private static DesignConversionModelServerApiResult BuildResult(
        DesignConversionThinClientStatusResult status,
        DesignConversionInternalEvaluationResult? evaluation,
        DesignConversionModelServerTrainingReadiness training,
        PromotedLocalModelResolution promotedLocalModel,
        LocalArtifactPersistenceReadback persistence)
    {
        var rawEvidenceRefs = BuildEvidenceRefs(status, evaluation, training.PromotedLocalModel)
            .Append(training.PromotedLocalModel is null
                ? "DESIGN_CONVERSION_MODEL_SERVER_NO_PROMOTED_LOCAL_MODEL_CONFIGURED"
                : "DESIGN_CONVERSION_MODEL_SERVER_PROMOTED_LOCAL_MODEL_IDENTIFIED")
            .Distinct(StringComparer.Ordinal)
            .OrderBy(reference => reference, StringComparer.Ordinal)
            .ToArray();
        var learnedApplied = LearnedInferenceApplied(evaluation);
        var deterministicClaim = evaluation?.DeterministicLogicClaimedAsLearning
            ?? IsTruthy(status.Provenance, "deterministic_logic_claimed_as_learning");
        var metadata = BuildBoundaryMetadata(
            status,
            evaluation,
            learnedApplied,
            deterministicClaim,
            training,
            promotedLocalModel,
            rawEvidenceRefs,
            persistence);
        var modelAuthority = DesignConversionModelAuthorityBoundary.FromTrustedBoundaryMetadata(metadata);
        var evidenceRefs = ReconcileBoundaryMarkers(rawEvidenceRefs, metadata, sort: true);
        var publicRuntimeStatus = ReconcileRuntimeStatusBoundaryWarnings(status, metadata);
        var warningsSource = publicRuntimeStatus.Warnings
            .Concat(training.Warnings)
            .Concat([
                HttpWiringBoundary,
                "public_http_endpoint_verified=false",
                "platform_integration_verified=false",
                $"model_training_verified={training.ModelTrainingVerified}",
                "ModelUploadReady=false",
                "deployment_verified=false",
                "product_ready=false"
            ]);
        if (training.ModelTrainingVerified)
        {
            warningsSource = warningsSource
                .Where(warning => !string.Equals(warning, "model_training_verified=false", StringComparison.Ordinal));
        }

        var warnings = ReconcileBoundaryMarkers(warningsSource, metadata, sort: true);

        return new DesignConversionModelServerApiResult(
            ApiShape: ApiShape,
            RuntimeSurface: RuntimeSurface,
            JobId: status.Job.JobId,
            RequestId: status.Job.RequestId,
            ContractStatus: status.ContractStatus,
            LearnedInferenceApplied: learnedApplied,
            TrainedModelAuthority: modelAuthority.TrainedModelAuthority,
            NotTrainedModelAuthority: modelAuthority.NotTrainedModelAuthority,
            DeterministicLogicClaimedAsLearning: deterministicClaim,
            ModelTrainingVerified: training.ModelTrainingVerified,
            ModelUploadReady: training.ModelUploadReady,
            DeploymentVerified: false,
            PublicHttpEndpointVerified: false,
            PlatformIntegrationVerified: false,
            ProductReady: false,
            RuntimeStatus: publicRuntimeStatus,
            TrainingReadiness: training,
            EvidenceRefs: evidenceRefs,
            Warnings: warnings,
            BoundaryMetadata: metadata,
            PromotedLocalModel: training.PromotedLocalModel);
    }

    private static IReadOnlyList<string> BuildEvidenceRefs(
        DesignConversionThinClientStatusResult status,
        DesignConversionInternalEvaluationResult? evaluation,
        DesignConversionPromotedLocalModel? promotedLocalModel = null)
    {
        var redactions = new SortedSet<string>(StringComparer.Ordinal);
        var evidenceRefs = (evaluation?.EvidenceRefs ?? [])
            .Concat(evaluation?.RuntimeFlowResult.EvidenceRefs ?? [])
            .Concat(evaluation?.RuntimeFlowResult.CriticOutput.QaEvidenceRefs ?? [])
            .Concat(evaluation?.RuntimeFlowResult.ImproverOutput.QaEvidenceRefs ?? [])
            .Concat(evaluation?.RuntimeFlowResult.Candidate.EvidenceRefs ?? [])
            .Concat(evaluation?.RuntimeFlowResult.Output.EvidenceRefs ?? [])
            .Concat(evaluation?.Verdict.EvidenceRefs ?? [])
            .Concat(promotedLocalModel?.EvidenceRefs ?? [])
            .Concat(status.Artifacts.Select(artifact => $"runtime-artifact:{artifact.Kind}:{artifact.Sha256}"))
            .Concat(status.Warnings.Where(IsBoundaryEvidence));
        return ReconcilePublicTeacherCallCapEvidenceRefs(evidenceRefs, status.Warnings)
            .Append(ApiShape)
            .Append(RuntimeSurface)
            .Append(HttpWiringBoundary)
            .Select(reference => SanitizeScalar(reference, "evidence_ref", redactions, fallback: "evidence"))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(reference => reference, StringComparer.Ordinal)
            .ToArray();
    }

    private static IEnumerable<string> ReconcilePublicTeacherCallCapEvidenceRefs(
        IEnumerable<string> evidenceRefs,
        IReadOnlyList<string> statusWarnings)
    {
        var reconciledTeacherRefs = statusWarnings
            .Where(IsTeacherCallCapBoundaryEvidence)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (reconciledTeacherRefs.Length == 0)
            return evidenceRefs;

        return evidenceRefs
            .Where(reference => !IsAnyTeacherCallCapEvidenceRef(reference))
            .Concat(reconciledTeacherRefs);
    }

    private static bool IsTeacherCallCapBoundaryEvidence(string value)
        => value.StartsWith("improvement_loop:max_teacher_call_observed=", StringComparison.Ordinal) ||
           value.StartsWith("improvement_loop:max_teacher_calls_cap_reached=", StringComparison.Ordinal) ||
           string.Equals(value, "improvement_loop:max_teacher_calls_reached", StringComparison.Ordinal) ||
           string.Equals(value, "improvement_loop:max_teacher_calls_reached_false", StringComparison.Ordinal);

    private static bool IsAnyTeacherCallCapEvidenceRef(string value)
        => string.Equals(
               value,
               DesignConversionImprovementLoopCoordinator.NoExternalProviderMarker,
               StringComparison.Ordinal) ||
           value.StartsWith("improvement_loop:max_teacher_call_observed=", StringComparison.Ordinal) ||
           value.StartsWith("improvement_loop:max_teacher_calls_", StringComparison.Ordinal);

    private static IReadOnlyDictionary<string, string> BuildBoundaryMetadata(
        DesignConversionThinClientStatusResult status,
        DesignConversionInternalEvaluationResult? evaluation,
        bool learnedApplied,
        bool deterministicClaim,
        DesignConversionModelServerTrainingReadiness training,
        PromotedLocalModelResolution promotedLocalModel,
        IReadOnlyList<string> evidenceRefs,
        LocalArtifactPersistenceReadback persistence)
    {
        var browserRenderExecuted = evaluation?.RenderResults.Any(result =>
            IsTruthy(result.Image.Metadata, "browser_or_playwright_used")) ?? false;
        var visualAiExecuted = evaluation?.VisualQaBoundaryTrace.ExternalAiVisualJudgeExecuted ?? false;
        var visualJudgeStatus = evaluation?.VisualQaBoundaryTrace.VisualJudgeStatus
            ?? DesignConversionVisualQaBoundaryPolicy.ProviderRequired;
        var visualJudgeProviderBoundary = evaluation?.VisualQaBoundaryTrace.VisualJudgeProviderBoundary
            ?? DesignConversionVisualQaBoundaryPolicy.ProviderRequired;
        var visualJudgeProviderRequired =
            string.Equals(visualJudgeProviderBoundary, DesignConversionVisualQaBoundaryPolicy.ProviderRequired, StringComparison.Ordinal) ||
            string.Equals(visualJudgeProviderBoundary, DesignConversionVisualQaBoundaryPolicy.ExternalProviderNotConfiguredMarker, StringComparison.Ordinal);
        var localVisualLabeler =
            string.Equals(visualJudgeProviderBoundary, DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker, StringComparison.Ordinal);
        var externalVisualAiConfigured = evaluation?.VisualQaBoundaryTrace is not null &&
                                         !visualJudgeProviderRequired &&
                                         !localVisualLabeler;
        var estimatedVisualJudgeCost = evaluation?.VisualQaBoundaryTrace.TierPolicy.EstimatedCostUsd ?? 0m;
        var requiredViewportCount = evaluation?.VisualQaBoundaryTrace.MultiResolutionRender.RequiredViewportIds.Count ?? 0;
        var observedViewportCount = evaluation?.VisualQaBoundaryTrace.MultiResolutionRender.ObservedViewportIds.Count ?? 0;
        var renderQaResultCount = evaluation?.QaResults.Count ?? 0;
        var comparisonArtifactCount = evaluation?.ComparisonArtifacts.Count ?? 0;
        var browserE2e = EvaluateBrowserE2eReadiness(status, evaluation);
        var visualProviderMetadata = BuildVisualProviderMetadata(
            evaluation?.ComparisonArtifacts ?? [],
            evaluation?.VisualJudgeResults ?? [],
            evaluation?.JobId,
            status.Job.RequestId);
        var visualFidelity = EvaluateVisualFidelityReadiness(status, evaluation, browserE2e);
        var hasUnverifiedLearnedStatusProvenanceClaim =
            HasUnverifiedLearnedStatusProvenanceClaim(status.Provenance);
        var modelAuthority = DesignConversionModelAuthorityBoundary.FromTrustedDirectDpoRuntimeEvidence(
            evaluation?.RuntimeFlowResult.EvidenceRefs);
        var qualityMetrics = status.QualityMetrics ??
            DesignConversionThinClientStatusQualityMetrics.Unavailable(
                source: "status_quality_metrics_missing",
                unavailableReason: "quality_metrics_not_available_for_status");
        var statusDefects = status.Defects ?? [];
        var localModelVerificationAuthority = promotedLocalModel.AuthorityValue;
        var localModelVerificationAuthorized = promotedLocalModel.VerificationAuthorized;

        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["api_shape"] = ApiShape,
            ["runtime_surface"] = RuntimeSurface,
            ["design_conversion_runtime"] = nameof(DesignConversionThinClientJobRuntime),
            ["design_conversion_internal_service"] = nameof(DesignConversionInternalService),
            ["http_wiring_boundary"] = HttpWiringBoundary,
            ["local_callable_api_surface_verified"] = bool.TrueString,
            ["public_http_endpoint_verified"] = bool.FalseString,
            ["asp_net_route_verified"] = bool.FalseString,
            ["platform_integration_verified"] = bool.FalseString,
            ["learned_inference_applied"] = learnedApplied.ToString(),
            ["LearnedInferenceApplied"] = learnedApplied.ToString(),
            ["learned_inference_provenance_boundary"] = "server_owned_runtime_evidence_refs_only_status_provenance_rejected",
            ["learned_inference_server_owned_evidence_verified"] = learnedApplied.ToString(),
            ["learned_inference_status_provenance_claim_present"] =
                hasUnverifiedLearnedStatusProvenanceClaim.ToString(),
            ["learned_inference_status_provenance_claim_accepted"] = bool.FalseString,
            ["learned_inference_unverified_status_provenance_downgraded"] =
                (hasUnverifiedLearnedStatusProvenanceClaim && !learnedApplied).ToString(),
            [DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey] =
                DesignConversionModelAuthorityBoundary.TrainedModelAuthorityValue,
            [DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey] =
                DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityValue,
            ["learned_evidence_ref_count"] = evidenceRefs.Count(reference =>
                reference.Contains("LEARNED_INFERENCE", StringComparison.OrdinalIgnoreCase) ||
                reference.Contains("learned-inference", StringComparison.OrdinalIgnoreCase))
                .ToString(CultureInfo.InvariantCulture),
            ["deterministic_logic_claimed_as_learning"] = deterministicClaim.ToString(),
            ["model_training_verified"] = training.ModelTrainingVerified.ToString(),
            ["model_upload_ready"] = training.ModelUploadReady.ToString(),
            ["ModelUploadReady"] = training.ModelUploadReady.ToString(),
            ["deployment_verified"] = bool.FalseString,
            ["product_ready"] = bool.FalseString,
            ["local_model_verification_authority"] = localModelVerificationAuthority,
            ["local_model_promotion_verified"] =
                (localModelVerificationAuthorized && training.PromotedLocalModel is not null).ToString(),
            ["local_promoted_checkpoint_loaded"] =
                (localModelVerificationAuthorized &&
                 training.PromotedLocalModel?.CheckpointFreshLoadVerified == true).ToString(),
            ["local_promoted_checkpoint_real_weights"] =
                (localModelVerificationAuthorized &&
                 training.PromotedLocalModel?.CheckpointContainsRealTrainedWeights == true).ToString(),
            ["cloud_model_upload_attempted"] = bool.FalseString,
            ["source_disjoint_status"] = training.SourceDisjointStatus,
            ["measurement_status"] = training.MeasurementStatus,
            ["promotion_status"] = training.PromotionStatus,
            ["training_readiness_status"] = training.Status,
            ["runtime_contract_status"] = status.ContractStatus,
            ["quality_metrics_available"] = qualityMetrics.Available.ToString(),
            ["quality_metrics_source"] = qualityMetrics.Source,
            ["quality_metrics_unavailable_reason"] = qualityMetrics.UnavailableReason ?? string.Empty,
            ["status_defect_count"] = statusDefects.Count.ToString(CultureInfo.InvariantCulture),
            ["status_defects_refs_only"] = bool.TrueString,
            ["trusted_visual_cost_entitlement_bound"] =
                ReadMetadata(status.Provenance, "trusted_visual_cost_entitlement_bound"),
            ["trusted_visual_cost_entitlement_authority"] =
                ReadMetadata(status.Provenance, "trusted_visual_cost_entitlement_authority"),
            ["trusted_visual_cost_entitlement_user_identity_authority"] =
                ReadMetadata(status.Provenance, "trusted_visual_cost_entitlement_user_identity_authority"),
            ["trusted_visual_cost_entitlement_user_ref"] =
                ReadMetadata(status.Provenance, "trusted_visual_cost_entitlement_user_ref"),
            ["trusted_visual_cost_entitlement_tenant_id_ref"] =
                ReadMetadata(status.Provenance, "trusted_visual_cost_entitlement_tenant_id_ref"),
            ["trusted_visual_cost_entitlement_design_tier"] =
                ReadMetadata(status.Provenance, "trusted_visual_cost_entitlement_design_tier"),
            ["trusted_visual_cost_entitlement_monthly_limit_usd"] =
                ReadMetadata(status.Provenance, "trusted_visual_cost_entitlement_monthly_limit_usd"),
            ["trusted_visual_cost_entitlement_period_kind"] =
                ReadMetadata(status.Provenance, "trusted_visual_cost_entitlement_period_kind"),
            ["trusted_visual_cost_entitlement_raw_user_exposed"] =
                ReadMetadata(status.Provenance, "trusted_visual_cost_entitlement_raw_user_exposed"),
            ["trusted_visual_cost_reservation_store_wired"] =
                ReadMetadata(status.Provenance, "trusted_visual_cost_reservation_store_wired"),
            ["render_qa_result_count"] = renderQaResultCount.ToString(CultureInfo.InvariantCulture),
            ["comparison_artifact_count"] = comparisonArtifactCount.ToString(CultureInfo.InvariantCulture),
            ["multi_viewport_required_count"] = requiredViewportCount.ToString(CultureInfo.InvariantCulture),
            ["multi_viewport_observed_count"] = observedViewportCount.ToString(CultureInfo.InvariantCulture),
            ["multi_viewport_complete"] = (evaluation?.VisualQaBoundaryTrace.MultiResolutionRender.IsComplete ?? false).ToString(),
            ["browser_render_executed"] = browserRenderExecuted.ToString(),
            ["browser_render_not_executed"] = (!browserRenderExecuted).ToString(),
            ["browser_e2e_verified"] = browserE2e.Verified.ToString(),
            ["browser_e2e_verification_authority"] = browserE2e.Authority,
            ["browser_e2e_verification_state"] = browserE2e.State,
            ["browser_e2e_failure_reason"] = browserE2e.FailureReason,
            ["browser_e2e_required_viewport_count"] = browserE2e.RequiredViewportCount.ToString(CultureInfo.InvariantCulture),
            ["browser_e2e_observed_viewport_count"] = browserE2e.ObservedViewportCount.ToString(CultureInfo.InvariantCulture),
            ["browser_e2e_verified_render_artifact_count"] = browserE2e.VerifiedRenderArtifactCount.ToString(CultureInfo.InvariantCulture),
            ["browser_e2e_png_signature_verified_count"] = browserE2e.PngSignatureVerifiedCount.ToString(CultureInfo.InvariantCulture),
            ["browser_e2e_sha256_verified_count"] = browserE2e.Sha256VerifiedCount.ToString(CultureInfo.InvariantCulture),
            ["browser_e2e_nonblank_verified_count"] = browserE2e.NonBlankVerifiedCount.ToString(CultureInfo.InvariantCulture),
            ["browser_e2e_dimension_verified_count"] = browserE2e.DimensionVerifiedCount.ToString(CultureInfo.InvariantCulture),
            ["browser_e2e_browser_provenance_count"] = browserE2e.BrowserProvenanceCount.ToString(CultureInfo.InvariantCulture),
            ["browser_e2e_provider_kind"] = browserE2e.ProviderKind,
            ["browser_e2e_artifact_kind"] = browserE2e.ArtifactKind,
            ["runtime_ready_verified"] = bool.FalseString,
            ["host_readiness_verified"] = bool.FalseString,
            ["visual_fidelity_verified"] = visualFidelity.Verified.ToString(),
            ["visual_fidelity_verification_authority"] = visualFidelity.Authority,
            ["visual_fidelity_verification_state"] = visualFidelity.State,
            ["visual_fidelity_failure_reason"] = visualFidelity.FailureReason,
            ["visual_fidelity_score"] = visualFidelity.Score,
            ["visual_fidelity_acceptance_threshold"] = visualFidelity.AcceptanceThreshold,
            ["visual_fidelity_threshold_source"] = "VisualQaBoundaryTrace.VisualDecision.RequiredThreshold",
            ["visual_fidelity_confidence"] = visualFidelity.Confidence,
            ["visual_fidelity_issue_count"] = visualFidelity.IssueCount.ToString(CultureInfo.InvariantCulture),
            ["visual_fidelity_blocking_issue_count"] = visualFidelity.BlockingIssueCount.ToString(CultureInfo.InvariantCulture),
            ["visual_fidelity_canonical_result_count"] = visualFidelity.CanonicalResultCount.ToString(CultureInfo.InvariantCulture),
            ["visual_fidelity_bound_artifact_count"] = visualFidelity.BoundArtifactCount.ToString(CultureInfo.InvariantCulture),
            ["visual_fidelity_score_threshold_met"] = visualFidelity.ScoreThresholdMet.ToString(),
            ["visual_fidelity_passed"] = visualFidelity.Passed.ToString(),
            ["visual_fidelity_provider_receipt_verified"] = visualFidelity.ProviderReceiptVerified.ToString(),
            ["visual_fidelity_response_identity_verified"] = visualFidelity.ResponseIdentityVerified.ToString(),
            ["visual_fidelity_response_schema_verified"] = visualFidelity.ResponseSchemaVerified.ToString(),
            ["persistence_verified"] = persistence.Verified.ToString(),
            ["persistence_verification_authority"] = persistence.Authority,
            ["persistence_verification_state"] = persistence.State,
            ["persistence_failure_reason"] = persistence.FailureReason,
            ["persistence_server_append_reported"] = persistence.AppendReported.ToString(),
            ["persistence_fresh_readback_success"] = persistence.FreshReadbackSuccess.ToString(),
            ["persistence_fresh_readback_query_job_id"] = persistence.QueryJobId,
            ["persistence_fresh_readback_query_request_id"] = persistence.QueryRequestId,
            ["persistence_fresh_readback_record_count"] = persistence.FreshReadbackRecordCount.ToString(CultureInfo.InvariantCulture),
            ["persistence_server_owned_record_count"] = persistence.ServerOwnedRecordCount.ToString(CultureInfo.InvariantCulture),
            ["persistence_record_id"] = persistence.RecordId,
            ["persistence_record_source"] = persistence.RecordSource,
            ["persistence_record_job_bound"] = persistence.RecordJobBound.ToString(),
            ["persistence_record_request_bound"] = persistence.RecordRequestBound.ToString(),
            ["persistence_record_refs_only"] = persistence.RecordRefsOnly.ToString(),
            ["persistence_raw_content_free"] = persistence.RawContentFree.ToString(),
            ["persistence_no_local_path_public"] = persistence.NoLocalPathPublic.ToString(),
            ["persistence_expected_render_ref_count"] = persistence.ExpectedRenderRefCount.ToString(CultureInfo.InvariantCulture),
            ["persistence_record_render_ref_count"] = persistence.RecordRenderRefCount.ToString(CultureInfo.InvariantCulture),
            ["persistence_bound_render_ref_count"] = persistence.BoundRenderRefCount.ToString(CultureInfo.InvariantCulture),
            ["persistence_conversion_result_bound"] = persistence.ConversionResultBound.ToString(),
            ["persistence_record_index_consistent"] = persistence.RecordIndexConsistent.ToString(),
            ["persistence_caller_claim_accepted"] = bool.FalseString,
            ["persistence_status_spoof_rejected"] = bool.TrueString,
            ["persistence_public_refs_only"] = bool.TrueString,
            [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractSuppliedMetadataKey] =
                ReadMetadata(status.Provenance, DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractSuppliedMetadataKey),
            [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractValueMetadataKey] =
                ReadMetadata(status.Provenance, DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractValueMetadataKey),
            [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractBoundaryMetadataKey] =
                ReadMetadata(status.Provenance, DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractBoundaryMetadataKey),
            [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsRuntimeEnforcementMetadataKey] =
                ReadMetadata(status.Provenance, DesignConversionImprovementLoopCoordinator.MaxTeacherCallsRuntimeEnforcementMetadataKey),
            [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCallCapEnforcedMetadataKey] =
                ReadMetadata(status.Provenance, DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCallCapEnforcedMetadataKey),
            [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsObservedCallCountMetadataKey] =
                ReadMetadata(status.Provenance, DesignConversionImprovementLoopCoordinator.MaxTeacherCallsObservedCallCountMetadataKey),
            [DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCapReachedMetadataKey] =
                ReadMetadata(status.Provenance, DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCapReachedMetadataKey),
            ["local_artifact_db_status_restored"] = IsTruthy(status.Provenance, "local_artifact_db_status_restored").ToString(),
            ["local_artifact_db_status_restore_boundary"] = ReadMetadata(status.Provenance, "local_artifact_db_status_restore_boundary"),
            ["local_artifact_db_status_restore_visual_decision"] =
                ReadMetadata(status.Provenance, "local_artifact_db_status_restore_visual_decision"),
            ["local_artifact_db_status_restore_status_source"] =
                ReadMetadata(status.Provenance, "local_artifact_db_status_restore_status_source"),
            ["local_artifact_db_status_restore_visual_decision_metadata_accepted"] =
                ReadMetadata(status.Provenance, "local_artifact_db_status_restore_visual_decision_metadata_accepted"),
            ["local_artifact_db_status_restore_fail_closed_visual_decision_fallback"] =
                ReadMetadata(status.Provenance, "local_artifact_db_status_restore_fail_closed_visual_decision_fallback"),
            ["local_artifact_db_status_restore_legacy_completed_fallback"] =
                ReadMetadata(status.Provenance, "local_artifact_db_status_restore_legacy_completed_fallback"),
            ["status_restore_source"] = ReadMetadata(status.Provenance, "status_restore_source"),
            ["status_restore_executed_runtime"] = ReadMetadata(status.Provenance, "status_restore_executed_runtime"),
            ["status_restore_contract_status"] = ReadMetadata(status.Provenance, "status_restore_contract_status"),
            ["status_restore_visual_decision"] = ReadMetadata(status.Provenance, "status_restore_visual_decision"),
            ["status_restore_status_source"] = ReadMetadata(status.Provenance, "status_restore_status_source"),
            ["status_restore_visual_decision_metadata_accepted"] =
                ReadMetadata(status.Provenance, "status_restore_visual_decision_metadata_accepted"),
            ["status_restore_fail_closed_visual_decision_fallback"] =
                ReadMetadata(status.Provenance, "status_restore_fail_closed_visual_decision_fallback"),
            ["status_restore_legacy_completed_fallback"] =
                ReadMetadata(status.Provenance, "status_restore_legacy_completed_fallback"),
            ["browser_renderer_explicit_local_injection_proven"] = ReadScaffoldMarker(
                evaluation,
                "browser_renderer_explicit_local_injection_proven"),
            ["browser_renderer_default_production_wired"] = ReadScaffoldMarker(
                evaluation,
                "browser_renderer_default_production_wired"),
            ["browser_renderer_default_scope"] = ReadScaffoldMarker(
                evaluation,
                "browser_renderer_default_scope"),
            ["browser_renderer_injected_provider_kind"] = ReadScaffoldMarker(
                evaluation,
                "browser_renderer_injected_provider_kind"),
            ["service_scaffold_browser_renderer_explicit_local_injection_proven"] = ReadScaffoldMarker(
                evaluation,
                "browser_renderer_explicit_local_injection_proven"),
            ["service_scaffold_browser_renderer_default_production_wired"] = ReadScaffoldMarker(
                evaluation,
                "browser_renderer_default_production_wired"),
            ["service_scaffold_browser_renderer_default_scope"] = ReadScaffoldMarker(
                evaluation,
                "browser_renderer_default_scope"),
            ["service_scaffold_browser_renderer_provider_kind"] = ReadScaffoldMarker(
                evaluation,
                "browser_renderer_injected_provider_kind"),
            ["visual_ai_executed"] = visualAiExecuted.ToString(),
            ["visual_ai_not_executed"] = (!visualAiExecuted).ToString(),
            ["visual_judge_status"] = visualJudgeStatus,
            ["visual_judge_provider_boundary"] = visualJudgeProviderBoundary,
            ["external_visual_ai_configured"] = externalVisualAiConfigured.ToString(),
            ["external_visual_ai_executed"] = visualAiExecuted.ToString(),
            ["external_visual_ai_blocked"] = visualJudgeProviderRequired.ToString(),
            ["blocked_reason"] = visualJudgeProviderRequired ? "not_configured" : "none",
            ["estimated_visual_judge_cost_usd"] = estimatedVisualJudgeCost.ToString("0.####", CultureInfo.InvariantCulture),
            ["render_artifact_refs"] = "refs_or_hashes_only",
            ["comparison_artifact_refs"] = "refs_or_hashes_only",
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["raw_user_private_data_included"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString
        };

        if (training.PromotedLocalModel is { } model)
        {
            metadata["local_promoted_model_id"] = model.ModelId;
            metadata["local_promoted_manifest_id"] = model.ManifestId;
            metadata["local_promoted_checkpoint_sha256"] = model.CheckpointSha256;
            metadata["local_promoted_model_version"] = model.Version;
            metadata["local_promoted_model_machine_id"] = model.MachineId;
            metadata["local_promoted_model_domain_tag"] = model.DomainTag;
            metadata["local_promoted_learned_weight_artifact_count"] =
                model.LearnedWeightArtifactCount.ToString(CultureInfo.InvariantCulture);
            metadata["local_promoted_learned_weight_observation_count"] =
                model.LearnedWeightObservationCount.ToString(CultureInfo.InvariantCulture);
            metadata["local_promoted_model_boundary"] = DesignConversionPromotedLocalModelLoader.LocalPromotionBoundary;
            metadata["local_promoted_model_local_only"] = model.LocalOnly.ToString();
            metadata["local_promoted_checkpoint_weights_source"] = model.LocalCheckpointWeightsSource;
            metadata["local_promoted_source_provenance_ref"] = model.SourceProvenanceRef;
            metadata["local_promoted_dpo_trainer_policy_checkpoint_converted"] =
                model.DpoTrainerPolicyCheckpointConverted.ToString();
            metadata["local_promoted_dpo_trainer_policy_checkpoint_fresh_loaded"] =
                model.DpoTrainerPolicyCheckpointFreshLoaded.ToString();
            metadata["local_promoted_dpo_policy_checkpoint_sha256_ref"] = model.DpoPolicyCheckpointSha256Ref;
            metadata["local_promoted_dpo_policy_checkpoint_file_ref"] = model.DpoPolicyCheckpointFileRef;
            metadata["local_promoted_source_disjoint_heldout_readiness_status"] =
                model.SourceDisjointHeldOutReadinessStatus;
            metadata["local_promoted_source_disjoint_heldout_readiness_gate_passed"] =
                model.SourceDisjointHeldOutReadinessGatePassed.ToString();
        }
        foreach (var pair in BuildRuntimeFlowMetricBoundaryMetadata(evaluation))
            metadata[pair.Key] = pair.Value;
        foreach (var pair in visualProviderMetadata)
            metadata[pair.Key] = pair.Value;
        DesignConversionModelAuthorityBoundary.ApplyTrustedSnapshot(metadata, modelAuthority);

        return metadata;
    }

    private static IReadOnlyDictionary<string, string> BuildRuntimeFlowMetricBoundaryMetadata(
        DesignConversionInternalEvaluationResult? evaluation)
    {
        var metrics = evaluation?.RuntimeFlowResult.Candidate.Metadata;
        if (metrics is null || metrics.Count == 0)
            return new Dictionary<string, string>();

        var redactions = new SortedSet<string>(StringComparer.Ordinal);
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var key in new[]
                 {
                     "LocalCheckpointFeatureAdjustmentApplied",
                     "LocalCheckpointFeatureContributionCount",
                     "local_checkpoint_weights_configured",
                     "local_checkpoint_weights_source",
                     "local_checkpoint_feature_adjustment_applied",
                     "dpo_trainer_policy_checkpoint_converted",
                      "dpo_trainer_policy_checkpoint_fresh_loaded",
                      "direct_dpo_policy_inference_applied",
                      "direct_dpo_policy_score_authority",
                      "direct_dpo_policy_acceptance_authority",
                      "baseline_visual_verdict_acceptance_authority",
                      "candidate_html_changed",
                      "candidate_css_changed",
                     "local_improver_selected_rule_ids",
                     "logical_flow_primary_metric_name",
                     "logical_flow_enhanced_primary",
                     "logical_flow_overall_verdict"
                 })
        {
            if (metrics.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                metadata[key] = SanitizeScalar(value, $"runtime_flow_metric.{key}", redactions, fallback: string.Empty);
        }

        return metadata;
    }

    private async Task<LocalArtifactPersistenceReadback> VerifyLocalArtifactPersistenceAsync(
        DesignConversionThinClientStatusResult status,
        DesignConversionInternalEvaluationResult? evaluation,
        CancellationToken ct)
    {
        const string authority = "server_owned_local_artifact_db_append_fresh_readback";
        var queryJobId = status.Job.JobId;
        var queryRequestId = status.Job.RequestId;
        var appendReported =
            IsTruthy(status.Job.Metadata, "local_artifact_db_persistence_verified") &&
            IsTruthy(status.Provenance, "local_artifact_db_persistence_verified");
        var appendRecordId = ReadMetadata(status.Job.Metadata, "local_artifact_db_record_id");

        if (_artifactDb is null)
        {
            return LocalArtifactPersistenceReadback.NotVerified(
                authority,
                "local_artifact_db_not_configured",
                appendReported,
                queryJobId,
                queryRequestId);
        }

        OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>> readback;
        try
        {
            readback = await _artifactDb.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery(
                    JobId: queryJobId,
                    RequestId: queryRequestId),
                ct);
        }
        catch (OperationCanceledException ex) when (IsCancellationFromSuppliedToken(ex, ct))
        {
            return LocalArtifactPersistenceReadback.NotVerified(
                authority,
                "fresh_readback_cancelled",
                appendReported,
                queryJobId,
                queryRequestId);
        }
        catch (Exception ex) when (SnapshotExceptionGraphClassifier.IsNonFatal(ex))
        {
            return LocalArtifactPersistenceReadback.NotVerified(
                authority,
                $"fresh_readback_exception_{SafeFailureToken(ex.GetType().Name)}",
                appendReported,
                queryJobId,
                queryRequestId);
        }

        if (!readback.IsSuccess)
        {
            return LocalArtifactPersistenceReadback.NotVerified(
                authority,
                string.IsNullOrWhiteSpace(readback.ErrorCode)
                    ? "fresh_readback_failed"
                    : $"fresh_readback_failed_{SafeFailureToken(readback.ErrorCode!)}",
                appendReported,
                queryJobId,
                queryRequestId,
                freshReadbackSuccess: false);
        }

        var records = readback.Value ?? [];
        var serverOwnedRecords = records
            .Where(record => IsServerOwnedRuntimePersistenceRecord(record, status, appendRecordId))
            .ToArray();
        var record = serverOwnedRecords.Length == 1 ? serverOwnedRecords[0] : null;
        var binding = BuildPersistenceBinding(record, evaluation);
        var recordRefsOnly = record is not null && IsPersistenceRecordRefsOnly(record);
        var rawContentFree = record is not null && IsPersistenceRecordRawContentFree(record);
        var recordIndexConsistent = record is not null && IsPersistenceRecordIndexConsistent(record);

        var failureReason = "none";
        if (!appendReported)
            failureReason = "runtime_append_success_not_reported";
        else if (records.Count == 0)
            failureReason = "fresh_readback_empty";
        else if (serverOwnedRecords.Length != 1)
            failureReason = "server_owned_record_count_not_one";
        else if (!recordRefsOnly)
            failureReason = "record_refs_only_flags_missing";
        else if (!rawContentFree)
            failureReason = "record_contains_raw_or_local_ref";
        else if (evaluation is null)
            failureReason = "internal_evaluation_receipt_missing";
        else if (!binding.ConversionResultBound)
            failureReason = "conversion_result_not_bound";
        else if (!binding.RenderRefsBound)
            failureReason = "render_refs_not_bound";
        else if (!recordIndexConsistent)
            failureReason = "record_index_not_consistent";

        var verified = string.Equals(failureReason, "none", StringComparison.Ordinal);
        return new LocalArtifactPersistenceReadback(
            Verified: verified,
            Authority: authority,
            State: verified
                ? "verified_by_server_append_and_fresh_job_request_readback"
                : "not_verified_by_server_append_and_fresh_job_request_readback",
            FailureReason: failureReason,
            AppendReported: appendReported,
            FreshReadbackSuccess: true,
            QueryJobId: queryJobId,
            QueryRequestId: queryRequestId,
            FreshReadbackRecordCount: records.Count,
            ServerOwnedRecordCount: serverOwnedRecords.Length,
            RecordId: record?.RecordId ?? string.Empty,
            RecordSource: record is null ? string.Empty : ReadMetadata(record.Metadata, "artifact_db_record_source"),
            RecordJobBound: binding.RecordJobBound,
            RecordRequestBound: binding.RecordRequestBound,
            RecordRefsOnly: recordRefsOnly,
            RawContentFree: rawContentFree,
            NoLocalPathPublic: rawContentFree,
            ExpectedRenderRefCount: binding.ExpectedRenderRefCount,
            RecordRenderRefCount: binding.RecordRenderRefCount,
            BoundRenderRefCount: binding.BoundRenderRefCount,
            ConversionResultBound: binding.ConversionResultBound,
            RecordIndexConsistent: recordIndexConsistent);
    }

    private static bool IsServerOwnedRuntimePersistenceRecord(
        DesignConversionLocalArtifactDbRecord record,
        DesignConversionThinClientStatusResult status,
        string appendRecordId)
        => record is not null &&
           string.Equals(record.JobId, status.Job.JobId, StringComparison.Ordinal) &&
           string.Equals(record.RequestId, status.Job.RequestId, StringComparison.Ordinal) &&
           string.Equals(record.DatabaseKind, JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind, StringComparison.Ordinal) &&
           (string.IsNullOrWhiteSpace(appendRecordId) ||
            string.Equals(record.RecordId, appendRecordId, StringComparison.Ordinal)) &&
           MetadataEquals(record.Metadata, "artifact_db_record_source", nameof(DesignConversionThinClientJobRuntime)) &&
           IsTruthy(record.Metadata, "local_artifact_db_persistence_verified") &&
           IsTruthy(record.Metadata, "local_artifact_db") &&
           IsTruthy(record.Metadata, "storage_training_artifact_db");

    private static PersistenceRecordBinding BuildPersistenceBinding(
        DesignConversionLocalArtifactDbRecord? record,
        DesignConversionInternalEvaluationResult? evaluation)
    {
        var expectedRenderResults = evaluation?.RenderResults ?? [];
        if (record is null)
        {
            return new PersistenceRecordBinding(
                RecordJobBound: false,
                RecordRequestBound: false,
                ConversionResultBound: false,
                ExpectedRenderRefCount: expectedRenderResults.Count,
                RecordRenderRefCount: 0,
                BoundRenderRefCount: 0,
                RenderRefsBound: false);
        }

        var boundRenderCount = expectedRenderResults.Count(result =>
            record.RenderArtifactRefs.Any(render => MatchesPersistenceRenderRef(render, result)));
        var conversionBound = evaluation is not null &&
                              string.Equals(
                                  record.ConversionResult.ResultId,
                                  evaluation.RuntimeFlowResult.Output.OutputId,
                                  StringComparison.Ordinal) &&
                              !string.IsNullOrWhiteSpace(record.ConversionResult.HtmlRef) &&
                              !string.IsNullOrWhiteSpace(record.ConversionResult.CssRef) &&
                              IsValidSha256(record.ConversionResult.HtmlSha256) &&
                              IsValidSha256(record.ConversionResult.CssSha256) &&
                              !LooksLikeUnsafePersistenceValue(record.ConversionResult.HtmlRef) &&
                              !LooksLikeUnsafePersistenceValue(record.ConversionResult.CssRef);

        return new PersistenceRecordBinding(
            RecordJobBound: evaluation is not null &&
                            string.Equals(record.JobId, evaluation.JobId, StringComparison.Ordinal),
            RecordRequestBound: evaluation is not null &&
                                string.Equals(record.RequestId, evaluation.SubmittedRequest.RequestId, StringComparison.Ordinal),
            ConversionResultBound: conversionBound,
            ExpectedRenderRefCount: expectedRenderResults.Count,
            RecordRenderRefCount: record.RenderArtifactRefs.Count,
            BoundRenderRefCount: boundRenderCount,
            RenderRefsBound: expectedRenderResults.Count > 0 &&
                             record.RenderArtifactRefs.Count == expectedRenderResults.Count &&
                             boundRenderCount == expectedRenderResults.Count);
    }

    private static bool MatchesPersistenceRenderRef(
        DesignConversionRenderArtifactDbRef recordRef,
        DesignConversionRenderResult result)
        => string.Equals(recordRef.ArtifactId, result.RenderResultId, StringComparison.Ordinal) &&
           string.Equals(recordRef.ViewportId, result.Image.Resolution.ResolutionId, StringComparison.Ordinal) &&
           string.Equals(recordRef.Sha256, result.Image.Sha256, StringComparison.Ordinal) &&
           recordRef.Width == result.Image.Resolution.Width &&
           recordRef.Height == result.Image.Resolution.Height &&
           !LooksLikeUnsafePersistenceValue(recordRef.UriRef);

    private static bool IsPersistenceRecordRefsOnly(DesignConversionLocalArtifactDbRecord record)
        => MetadataEquals(record.Metadata, "conversion_result", "refs_or_hashes_only") &&
           MetadataEquals(record.ConversionResult.Metadata, "conversion_result", "html_css_refs_or_hashes_only") &&
           MetadataEquals(record.ConversionResult.Metadata, "raw_html_included", bool.FalseString) &&
           MetadataEquals(record.ConversionResult.Metadata, "raw_css_included", bool.FalseString) &&
           IsTruthy(record.Metadata, "training_database_refs_only_trace") &&
           IsTruthy(record.Metadata, "raw_payload_redacted");

    private static bool IsPersistenceRecordRawContentFree(DesignConversionLocalArtifactDbRecord record)
        => IsCompletedStatusSnapshotMetadataSafe(record.VisualJudgeStatus) &&
           PersistenceRecordPublicValues(record).All(value => !LooksLikeUnsafePersistenceValue(value));

    private static bool IsPersistenceRecordIndexConsistent(DesignConversionLocalArtifactDbRecord record)
        => record.EvidenceRefs.Contains(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind, StringComparer.Ordinal) &&
           record.EvidenceRefs.Contains(JsonlDesignConversionLocalArtifactDb.StorageTrainingArtifactDbMarker, StringComparer.Ordinal) &&
           MetadataIntAtLeast(record.Metadata, "training_database_record_count", 1);

    private static IEnumerable<string> PersistenceRecordPublicValues(DesignConversionLocalArtifactDbRecord record)
    {
        yield return record.RecordId;
        yield return record.JobId;
        yield return record.RequestId;
        yield return record.SourceKind;
        yield return record.Tier;
        yield return record.ConversionResult.ResultId;
        yield return record.ConversionResult.HtmlRef;
        yield return record.ConversionResult.CssRef;
        yield return record.ConversionResult.HtmlSha256;
        yield return record.ConversionResult.CssSha256;
        foreach (var value in record.SanitizedSourceRefs.Values)
            yield return value;
        foreach (var value in record.ConversionResult.Metadata.Values)
            yield return value;
        foreach (var render in record.RenderArtifactRefs)
        {
            yield return render.ViewportId;
            yield return render.ArtifactId;
            yield return render.Kind;
            yield return render.UriRef;
            yield return render.Sha256;
            foreach (var value in render.Metadata.Values)
                yield return value;
        }
        yield return record.VisualJudgeStatus.Status;
        yield return record.VisualJudgeStatus.ProviderBoundary;
        yield return record.VisualJudgeStatus.TierDecision;
        foreach (var value in record.VisualJudgeStatus.EvidenceRefs)
            yield return value;
        foreach (var (key, value) in record.VisualJudgeStatus.Metadata)
        {
            if (string.Equals(
                    key,
                    DesignConversionLocalArtifactDbRecord.CompletedStatusSnapshotMetadataKey,
                    StringComparison.Ordinal))
            {
                continue;
            }

            yield return value;
        }
        foreach (var value in record.EvidenceRefs)
            yield return value;
        foreach (var value in record.Metadata.Values)
            yield return value;
    }

    private static bool IsCompletedStatusSnapshotMetadataSafe(DesignConversionVisualJudgeArtifactDbStatus visualJudgeStatus)
        => !visualJudgeStatus.Metadata.ContainsKey(DesignConversionLocalArtifactDbRecord.CompletedStatusSnapshotMetadataKey) ||
           DesignConversionLocalArtifactDbRecord.TryReadCompletedStatusSnapshot(visualJudgeStatus, out _);

    private static bool LooksLikeUnsafePersistenceValue(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           (LooksLikeLocalPathOrFileUri(value) ||
            value.Contains("PRIVATE", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("SECRET", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("<html", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("<main", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("<div", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("{\"", StringComparison.Ordinal) ||
            value.Contains("data-secret", StringComparison.OrdinalIgnoreCase));

    private static bool LooksLikeLocalPathOrFileUri(string value)
        => value.StartsWith("file:", StringComparison.OrdinalIgnoreCase) ||
           value.StartsWith(@"\\", StringComparison.Ordinal) ||
           (value.Length >= 3 && char.IsLetter(value[0]) && value[1] == ':' && (value[2] == '\\' || value[2] == '/')) ||
           value.StartsWith("/Users/", StringComparison.OrdinalIgnoreCase) ||
           value.StartsWith("/home/", StringComparison.OrdinalIgnoreCase);

    private static bool MetadataIntAtLeast(IReadOnlyDictionary<string, string> metadata, string key, int threshold)
        => metadata.TryGetValue(key, out var value) &&
           int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) &&
           parsed >= threshold;

    private static string SafeFailureToken(string value)
    {
        var token = new string((value ?? string.Empty)
            .Select(ch => char.IsLetterOrDigit(ch) ? char.ToLowerInvariant(ch) : '_')
            .ToArray()).Trim('_');
        return string.IsNullOrWhiteSpace(token) ? "unknown" : token;
    }

    private static VisualFidelityReadiness EvaluateVisualFidelityReadiness(
        DesignConversionThinClientStatusResult status,
        DesignConversionInternalEvaluationResult? evaluation,
        BrowserE2eReadiness browserE2e)
    {
        const string authority = "server_owned_browser_render_and_visual_provider_receipts";
        if (evaluation is null)
        {
            return VisualFidelityReadiness.NotVerified(
                authority,
                "not_evaluated_no_internal_evaluation_receipt",
                score: "none",
                acceptanceThreshold: "none",
                confidence: "none");
        }

        var trace = evaluation.VisualQaBoundaryTrace;
        var requiredViewportCount = trace.MultiResolutionRender.RequiredViewportIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .Count();
        var threshold = trace.VisualDecision.RequiredThreshold;
        var thresholdValid = IsFiniteNumber(threshold) && threshold >= 0.0 && threshold <= 1.0;
        var trustEvidence = BuildVisualProviderTrustEvidence(
            evaluation.ComparisonArtifacts,
            evaluation.VisualJudgeResults,
            evaluation.JobId,
            status.Job.RequestId);
        var canonicalResults = SelectTrustedCanonicalVisualJudgeResults(
            trustEvidence.TrustedProviderArtifacts,
            trustEvidence.MetadataVisualJudgeResults);
        var canonicalResult = canonicalResults.Length == 1 ? canonicalResults[0] : null;
        var boundArtifactCount = canonicalResult is null
            ? 0
            : trustEvidence.TrustedProviderArtifacts.Count(artifact =>
                MatchesTrustedVisualJudgeArtifactIdentity(artifact, canonicalResult));
        var providerReceiptVerified = canonicalResult is not null &&
                                      boundArtifactCount == 1 &&
                                      trustEvidence.TrustedProviderArtifacts.Any(artifact =>
                                          DesignConversionModelServerNetworkBoundary.IsTrustedVisualProviderNetworkCall(
                                              artifact,
                                              evaluation.JobId,
                                              status.Job.RequestId,
                                              providerResponseIdentityVerified: true,
                                              providerResponseSchemaVerified: true)) &&
                                      DesignConversionModelServerNetworkBoundary.IsTrustedVisualJudgeNetworkCall(
                                          canonicalResult,
                                          evaluation.JobId,
                                          status.Job.RequestId);
        var responseIdentityVerified = canonicalResult is not null &&
                                       IsTruthy(canonicalResult.Metadata, "provider_response_identity_verified") &&
                                       IsTruthy(canonicalResult.Provider.Metadata, "provider_response_identity_verified");
        var responseSchemaVerified = canonicalResult is not null &&
                                     IsTruthy(canonicalResult.Metadata, "provider_response_schema_verified") &&
                                     IsTruthy(canonicalResult.Provider.Metadata, "provider_response_schema_verified");
        var score = canonicalResult?.Verdict.Score ?? double.NaN;
        var scoreValid = canonicalResult is not null &&
                         IsFiniteNumber(score) &&
                         score >= 0.0 &&
                         score <= 1.0;
        var scoreThresholdMet = scoreValid && thresholdValid && score >= threshold;
        var passed = canonicalResult?.Verdict.Passed == true;
        var issueCount = canonicalResult?.Verdict.Defects.Count ?? 0;
        var issueCountBounded = canonicalResult is not null &&
                                issueCount >= 0 &&
                                issueCount <= MaxVisualFidelityIssueCount;
        var blockingIssueCount = canonicalResult?.Verdict.Defects.Count(defect =>
            IsBlockingVisualIssueSeverity(defect.Severity)) ?? 0;
        var confidence = canonicalResult is not null &&
                         TryFormatTrustedVisualJudgeConfidence(canonicalResult, out var formattedConfidence)
            ? formattedConfidence
            : "none";
        var confidenceValid = !string.Equals(confidence, "none", StringComparison.Ordinal);

        var failureReason = "none";
        if (!string.Equals(status.ContractStatus, DesignConversionThinClientStatus.Completed, StringComparison.Ordinal))
            failureReason = "runtime_not_completed";
        else if (requiredViewportCount == 0 || !trace.MultiResolutionRender.IsComplete)
            failureReason = "required_viewport_coverage_incomplete";
        else if (!browserE2e.Verified)
            failureReason = "browser_e2e_not_verified";
        else if (!trustEvidence.TrustedNetworkCall)
            failureReason = "trusted_external_provider_network_receipt_missing";
        else if (canonicalResults.Length != 1)
            failureReason = "canonical_visual_result_count_not_one";
        else if (boundArtifactCount != 1)
            failureReason = "canonical_visual_result_artifact_binding_not_one";
        else if (!providerReceiptVerified)
            failureReason = "trusted_external_provider_receipt_not_verified";
        else if (!responseIdentityVerified)
            failureReason = "provider_response_identity_not_verified";
        else if (!responseSchemaVerified)
            failureReason = "provider_response_schema_not_verified";
        else if (!scoreValid)
            failureReason = "canonical_visual_result_score_invalid";
        else if (!thresholdValid)
            failureReason = "canonical_visual_threshold_invalid";
        else if (!confidenceValid)
            failureReason = "canonical_visual_result_confidence_invalid";
        else if (!issueCountBounded)
            failureReason = "canonical_visual_result_issue_count_unbounded";
        else if (!scoreThresholdMet)
            failureReason = "canonical_visual_result_score_below_threshold";
        else if (blockingIssueCount > 0)
            failureReason = "canonical_visual_result_blocking_issue_present";

        var verified = string.Equals(failureReason, "none", StringComparison.Ordinal);
        return new VisualFidelityReadiness(
            Verified: verified,
            Authority: authority,
            State: verified
                ? "verified_by_server_owned_browser_and_visual_provider_receipts"
                : "not_verified_by_server_owned_browser_and_visual_provider_receipts",
            FailureReason: failureReason,
            Score: scoreValid ? score.ToString("0.####", CultureInfo.InvariantCulture) : "none",
            AcceptanceThreshold: thresholdValid ? threshold.ToString("0.####", CultureInfo.InvariantCulture) : "none",
            Confidence: confidence,
            IssueCount: issueCount,
            BlockingIssueCount: blockingIssueCount,
            CanonicalResultCount: canonicalResults.Length,
            BoundArtifactCount: boundArtifactCount,
            Passed: passed,
            ScoreThresholdMet: scoreThresholdMet,
            ProviderReceiptVerified: providerReceiptVerified,
            ResponseIdentityVerified: responseIdentityVerified,
            ResponseSchemaVerified: responseSchemaVerified);
    }

    private static BrowserE2eReadiness EvaluateBrowserE2eReadiness(
        DesignConversionThinClientStatusResult status,
        DesignConversionInternalEvaluationResult? evaluation)
    {
        const string authority = "server_owned_model_server_browser_render_receipts";
        if (evaluation is null)
        {
            return BrowserE2eReadiness.NotVerified(
                authority,
                "not_evaluated_no_internal_evaluation_receipt",
                "none",
                "none",
                requiredViewportCount: 0,
                observedViewportCount: 0);
        }

        var coverage = evaluation.VisualQaBoundaryTrace.MultiResolutionRender;
        var requiredViewportIds = coverage.RequiredViewportIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var observedViewportCount = coverage.ObservedViewportIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .Count();
        var renderResults = evaluation.RenderResults ?? [];
        var duplicateRequiredRenderReceipt = requiredViewportIds.Any(id =>
            renderResults.Count(result => string.Equals(
                result.Image.Resolution.ResolutionId,
                id,
                StringComparison.Ordinal)) > 1);
        var verifiedRequiredResults = requiredViewportIds
            .Select(id => renderResults.FirstOrDefault(result =>
                string.Equals(result.Image.Resolution.ResolutionId, id, StringComparison.Ordinal)))
            .Where(result => result is not null &&
                             IsServerOwnedBrowserE2eRenderResult(
                                 result,
                                 evaluation.JobId,
                                 evaluation.SubmittedRequest.RequestId))
            .Cast<DesignConversionRenderResult>()
            .ToArray();

        var verifiedRenderCount = verifiedRequiredResults.Length;
        var pngSignatureCount = verifiedRequiredResults.Count(result =>
            IsTruthy(result.Image.Metadata, "browser_screenshot_png_signature_verified") &&
            IsTruthy(result.Image.Metadata, "png_signature_verified"));
        var shaCount = verifiedRequiredResults.Count(result =>
            IsTruthy(result.Image.Metadata, "browser_screenshot_sha256_verified") &&
            IsValidSha256(result.Image.Sha256));
        var nonBlankCount = verifiedRequiredResults.Count(result =>
            IsTruthy(result.Image.Metadata, "browser_screenshot_nonblank_pixels_verified") &&
            MetadataIntGreaterThan(result.Image.Metadata, "browser_screenshot_non_white_pixel_count", 0) &&
            MetadataIntGreaterThan(result.Image.Metadata, "browser_screenshot_distinct_color_count", 1));
        var dimensionCount = verifiedRequiredResults.Count(HasVerifiedBrowserScreenshotDimensions);
        var browserProvenanceCount = verifiedRequiredResults.Count(result =>
            IsTruthy(result.Image.Metadata, "browser_or_playwright_used") &&
            !string.IsNullOrWhiteSpace(ReadMetadata(result.Image.Metadata, "browser_adapter_kind")));
        var providerKind = JoinedMetadataValue(verifiedRequiredResults, "provider_kind", "none");
        var artifactKind = JoinedMetadataValue(verifiedRequiredResults, "render_artifact_kind", "none");

        string failureReason;
        if (!HasCompletedServerOwnedBrowserRuntime(status, evaluation))
            failureReason = "runtime_not_completed";
        else if (requiredViewportIds.Length == 0)
            failureReason = "required_viewports_not_declared";
        else if (!coverage.IsComplete)
            failureReason = "required_viewport_coverage_incomplete";
        else if (duplicateRequiredRenderReceipt)
            failureReason = "server_owned_browser_render_artifact_receipts_not_unique";
        else if (verifiedRenderCount != requiredViewportIds.Length)
            failureReason = "server_owned_browser_render_artifact_receipts_incomplete";
        else if (pngSignatureCount != requiredViewportIds.Length)
            failureReason = "png_signature_receipts_incomplete";
        else if (shaCount != requiredViewportIds.Length)
            failureReason = "sha256_receipts_incomplete";
        else if (nonBlankCount != requiredViewportIds.Length)
            failureReason = "nonblank_pixel_receipts_incomplete";
        else if (dimensionCount != requiredViewportIds.Length)
            failureReason = "dimension_receipts_incomplete";
        else if (browserProvenanceCount != requiredViewportIds.Length)
            failureReason = "browser_process_provenance_incomplete";
        else
            failureReason = "none";

        var verified = string.Equals(failureReason, "none", StringComparison.Ordinal);
        return new BrowserE2eReadiness(
            Verified: verified,
            Authority: authority,
            State: verified
                ? "verified_by_server_owned_browser_render_receipts"
                : "not_verified_by_server_owned_browser_render_receipts",
            FailureReason: failureReason,
            RequiredViewportCount: requiredViewportIds.Length,
            ObservedViewportCount: observedViewportCount,
            VerifiedRenderArtifactCount: verifiedRenderCount,
            PngSignatureVerifiedCount: pngSignatureCount,
            Sha256VerifiedCount: shaCount,
            NonBlankVerifiedCount: nonBlankCount,
            DimensionVerifiedCount: dimensionCount,
            BrowserProvenanceCount: browserProvenanceCount,
            ProviderKind: providerKind,
            ArtifactKind: artifactKind);
    }

    internal static IReadOnlyDictionary<string, string> EvaluateBrowserE2eReadinessForTesting(
        DesignConversionThinClientStatusResult status,
        DesignConversionInternalEvaluationResult? evaluation)
    {
        var readiness = EvaluateBrowserE2eReadiness(status, evaluation);
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["verified"] = readiness.Verified.ToString(),
            ["state"] = readiness.State,
            ["failure_reason"] = readiness.FailureReason,
            ["authority"] = readiness.Authority
        };
    }

    private static bool HasCompletedServerOwnedBrowserRuntime(
        DesignConversionThinClientStatusResult status,
        DesignConversionInternalEvaluationResult evaluation)
        => string.Equals(status.ContractStatus, DesignConversionThinClientStatus.Completed, StringComparison.Ordinal) &&
           string.Equals(status.Job.Status, DesignConversionThinClientStatus.Completed, StringComparison.Ordinal) &&
           string.Equals(status.Job.JobId, evaluation.JobId, StringComparison.Ordinal) &&
           string.Equals(status.Job.RequestId, evaluation.SubmittedRequest.RequestId, StringComparison.Ordinal) &&
           MetadataEquals(status.Provenance, "internal_evaluation_job_id", evaluation.JobId) &&
           MetadataEquals(
               status.Provenance,
               "thin_client_terminal_status",
               DesignConversionThinClientStatus.Completed) &&
           MetadataEquals(
               evaluation.SubmittedRequest.Provenance,
               "internal_service_job_id",
               evaluation.JobId) &&
           string.Equals(
               evaluation.VisualQaBoundaryTrace.TierPolicy.StopReason,
               DesignConversionVisualQaBoundaryPolicy.QualityReached,
               StringComparison.Ordinal) &&
           string.Equals(
               evaluation.VisualQaBoundaryTrace.VisualDecision.Decision,
               DesignConversionVisualQaBoundaryPolicy.DecisionAccepted,
               StringComparison.Ordinal);

    private static bool IsServerOwnedBrowserE2eRenderResult(
        DesignConversionRenderResult result,
        string expectedJobId,
        string expectedRequestId)
    {
        var image = result.Image;
        var metadata = image.Metadata;
        var receipt = result.BrowserRenderProofReceipt;
        return receipt is not null &&
               image.Resolution is not null &&
               string.Equals(receipt.IssuerKind, BrowserBackedDesignConversionRenderProvider.ProviderKind, StringComparison.Ordinal) &&
               string.Equals(receipt.RenderResultId, result.RenderResultId, StringComparison.Ordinal) &&
               string.Equals(receipt.RenderRequestId, result.RenderRequestId, StringComparison.Ordinal) &&
               string.Equals(receipt.RequestId, expectedRequestId, StringComparison.Ordinal) &&
               string.Equals(receipt.ImageId, image.ImageId, StringComparison.Ordinal) &&
               string.Equals(receipt.ScreenshotUri, image.Uri, StringComparison.Ordinal) &&
               string.Equals(receipt.NormalizedSha256, NormalizeSha256(image.Sha256), StringComparison.Ordinal) &&
               string.Equals(receipt.ResolutionId, image.Resolution.ResolutionId, StringComparison.Ordinal) &&
               receipt.ViewportWidth == image.Resolution.Width &&
               receipt.ViewportHeight == image.Resolution.Height &&
               Math.Abs(receipt.DeviceScaleFactor - image.Resolution.DeviceScaleFactor) < 0.000001 &&
               receipt.PngSignatureVerified &&
               receipt.Sha256Verified &&
               receipt.HasNonBlankPixels &&
               receipt.ByteLength > 0 &&
               receipt.NonWhitePixelCount > 0 &&
               receipt.DistinctColorCount > 1 &&
               IsLocalFileUri(image.Uri) &&
               IsValidSha256(image.Sha256) &&
               MetadataEquals(metadata, "server_bound_job_id", expectedJobId) &&
               MetadataEquals(metadata, "server_bound_request_id", expectedRequestId) &&
               MetadataEquals(metadata, "server_bound_render_request_id", result.RenderRequestId) &&
               MetadataEquals(metadata, "browser_render_proof_issuer_kind", receipt.IssuerKind) &&
               IsTruthy(metadata, "browser_render_proof_receipt_validated") &&
               MetadataEquals(metadata, "provider_kind", BrowserBackedDesignConversionRenderProvider.ProviderKind) &&
               MetadataEquals(metadata, "server_bound_render_provider_kind", BrowserBackedDesignConversionRenderProvider.ProviderKind) &&
               MetadataEquals(metadata, "render_artifact_kind", "browser_screenshot") &&
               MetadataEquals(metadata, "render_evidence_trust_boundary", "server_owned_render_provider_boundary") &&
               MetadataEquals(metadata, "render_evidence_trust_classification", "browser_backed_render_evidence") &&
               IsTruthy(metadata, "render_evidence_production_trusted") &&
               IsTruthy(metadata, "approval_evidence_allowed") &&
               IsTruthy(metadata, "visual_fidelity_claimed") &&
               IsTruthy(metadata, "browser_or_playwright_used") &&
               MetadataEquals(metadata, "external_ai_used", bool.FalseString) &&
               MetadataEquals(metadata, "hidden_determinism_boundary", "browser_backed_render_evidence") &&
               IsTruthy(metadata, "browser_render_evidence_server_canonicalized") &&
               IsTruthy(metadata, "browser_screenshot_file_verified") &&
               IsTruthy(metadata, "browser_screenshot_png_signature_verified") &&
               IsTruthy(metadata, "png_signature_verified") &&
               IsTruthy(metadata, "browser_screenshot_sha256_verified") &&
               IsTruthy(metadata, "browser_screenshot_nonblank_pixels_verified") &&
               MetadataEquals(metadata, "synthetic_or_scaffold_render_result", bool.FalseString) &&
               MetadataEquals(metadata, "scaffold_or_synthetic_render", bool.FalseString) &&
               HasVerifiedBrowserScreenshotDimensions(result) &&
               MetadataLongGreaterThan(metadata, "browser_screenshot_byte_length", 0) &&
               MetadataIntGreaterThan(metadata, "browser_screenshot_non_white_pixel_count", 0) &&
               MetadataIntGreaterThan(metadata, "browser_screenshot_distinct_color_count", 1) &&
               !string.IsNullOrWhiteSpace(ReadMetadata(metadata, "browser_adapter_kind"));
    }

    private static string NormalizeSha256(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase)
                ? value["sha256:".Length..].ToLowerInvariant()
                : value.ToLowerInvariant();

    private static bool HasVerifiedBrowserScreenshotDimensions(DesignConversionRenderResult result)
    {
        var resolution = result.Image.Resolution;
        var metadata = result.Image.Metadata;
        return MetadataIntEquals(metadata, "browser_screenshot_png_width", resolution.Width) &&
               MetadataIntEquals(metadata, "browser_screenshot_png_height", resolution.Height) &&
               MetadataIntEquals(metadata, "png_width", resolution.Width) &&
               MetadataIntEquals(metadata, "png_height", resolution.Height) &&
               MetadataIntEquals(metadata, "viewport_width", resolution.Width) &&
               MetadataIntEquals(metadata, "viewport_height", resolution.Height);
    }

    private static bool IsLocalFileUri(string? uri)
        => Uri.TryCreate(uri, UriKind.Absolute, out var parsed) &&
           parsed.IsFile &&
           string.IsNullOrWhiteSpace(parsed.Host);

    private static bool IsValidSha256(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var hex = value.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase)
            ? value["sha256:".Length..]
            : value;
        return hex.Length == 64 && hex.All(Uri.IsHexDigit);
    }

    private static bool MetadataIntEquals(IReadOnlyDictionary<string, string> metadata, string key, int expected)
        => metadata.TryGetValue(key, out var value) &&
           int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) &&
           parsed == expected;

    private static bool MetadataIntGreaterThan(IReadOnlyDictionary<string, string> metadata, string key, int threshold)
        => metadata.TryGetValue(key, out var value) &&
           int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) &&
           parsed > threshold;

    private static bool MetadataLongGreaterThan(IReadOnlyDictionary<string, string> metadata, string key, long threshold)
        => metadata.TryGetValue(key, out var value) &&
           long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) &&
           parsed > threshold;

    private static string ReadMetadata(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) ? value : string.Empty;

    private static string JoinedMetadataValue(
        IReadOnlyList<DesignConversionRenderResult> results,
        string key,
        string fallback)
    {
        var values = results
            .Select(result => ReadMetadata(result.Image.Metadata, key))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
        return values.Length == 0 ? fallback : string.Join("|", values);
    }

    internal static IReadOnlyDictionary<string, string> BuildVisualProviderMetadata(
        IReadOnlyList<DesignConversionQaComparisonArtifact> artifacts,
        IReadOnlyList<DesignConversionVisualJudgeResult> visualJudgeResults,
        string? currentJobId,
        string? currentRootRequestId)
    {
        var trustEvidence = BuildVisualProviderTrustEvidence(
            artifacts,
            visualJudgeResults,
            currentJobId,
            currentRootRequestId);
        var trustedReceiptPairs = SelectTrustedReceiptPairs(
            trustEvidence.TrustedProviderArtifacts,
            trustEvidence.MetadataVisualJudgeResults);
        var canonicalReceiptPair = trustedReceiptPairs.FirstOrDefault();
        var trustedCanonicalVisualJudgeResult = canonicalReceiptPair?.VisualJudgeResult;
        var trustedNetworkCall = trustedReceiptPairs.Length > 0;
        var trustedReceiptArtifacts = trustedNetworkCall
            ? SelectTrustedReceiptArtifacts(trustedReceiptPairs)
            : Array.Empty<DesignConversionQaComparisonArtifact>();
        IReadOnlyList<DesignConversionQaComparisonArtifact> metadataProviderArtifacts = trustedNetworkCall
            ? trustedReceiptArtifacts
            : trustEvidence.ProviderArtifacts;
        IReadOnlyList<DesignConversionQaComparisonArtifact> receiptMetadataArtifacts = trustedReceiptArtifacts;
        var providerSelectionPolicy = trustedNetworkCall
            ? DesignConversionVisualAiProviderSelectionPolicy.VerifiedCloudExecutionPolicyName
            : JoinedNoCloudVisualProviderSelectionPolicy(metadataProviderArtifacts);

        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["visual_provider_output_observed"] = (trustEvidence.ProviderArtifacts.Count > 0).ToString(),
            ["external_visual_ai_executed"] = trustedNetworkCall.ToString(),
            ["external_ai_visual_judge_executed"] = trustedNetworkCall.ToString(),
            ["external_provider_called"] = trustedNetworkCall.ToString(),
            ["external_network_call_executed"] = trustedNetworkCall.ToString(),
            ["network_call_executed"] = trustedNetworkCall.ToString(),
            ["real_external_provider_executed"] = trustedNetworkCall.ToString(),
            [DesignConversionModelServerNetworkBoundary.ServerOwnedVisualProviderNetworkCallVerifiedKey] =
                trustedNetworkCall.ToString(),
            [DesignConversionModelServerNetworkBoundary.ServerOwnedVisualProviderNetworkCallVerificationKey] =
                trustedNetworkCall
                    ? "comparison_artifact_provider_identity_schema_and_network_evidence_verified"
                    : "not_verified",
            ["provider_response_identity_verified"] = trustedNetworkCall.ToString(),
            ["provider_response_schema_verified"] = trustedNetworkCall.ToString(),
            ["provider_kind"] = JoinedVisualProviderMetadata(metadataProviderArtifacts, "provider_kind"),
            ["provider_name"] = JoinedVisualProviderMetadata(metadataProviderArtifacts, "provider_name"),
            ["provider_model"] = JoinedVisualProviderMetadata(metadataProviderArtifacts, "provider_model"),
            ["provider_endpoint_host"] = JoinedVisualProviderMetadata(metadataProviderArtifacts, "provider_endpoint_host"),
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey] = trustedNetworkCall.ToString(),
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVersionKey] =
                JoinedVisualProviderMetadata(receiptMetadataArtifacts, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVersionKey),
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptIdKey] =
                JoinedVisualProviderMetadata(receiptMetadataArtifacts, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptIdKey),
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJobIdKey] =
                JoinedVisualProviderMetadata(receiptMetadataArtifacts, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJobIdKey),
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJudgeRequestIdKey] =
                JoinedVisualProviderMetadata(receiptMetadataArtifacts, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJudgeRequestIdKey),
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRequestIdKey] =
                JoinedVisualProviderMetadata(receiptMetadataArtifacts, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRequestIdKey),
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRootRequestIdKey] =
                JoinedVisualProviderMetadata(receiptMetadataArtifacts, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRootRequestIdKey),
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey] =
                JoinedTrustedVisualProviderPairCallMetadata(
                    trustedReceiptPairs,
                    ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey),
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey] =
                JoinedTrustedVisualProviderPairCallMetadata(
                    trustedReceiptPairs,
                    ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey),
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualJudgeRequestIdKey] =
                JoinedTrustedVisualProviderPairCallMetadata(
                    trustedReceiptPairs,
                    ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualJudgeRequestIdKey),
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualResultIdKey] =
                JoinedVisualProviderMetadata(receiptMetadataArtifacts, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualResultIdKey),
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey] =
                JoinedVisualProviderMetadata(receiptMetadataArtifacts, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey),
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderNameKey] =
                JoinedVisualProviderMetadata(receiptMetadataArtifacts, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderNameKey),
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderModelKey] =
                JoinedVisualProviderMetadata(receiptMetadataArtifacts, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderModelKey),
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptEndpointHostKey] =
                JoinedVisualProviderMetadata(receiptMetadataArtifacts, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptEndpointHostKey),
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey] = trustedNetworkCall.ToString(),
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey] = trustedNetworkCall.ToString(),
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey] = trustedNetworkCall.ToString(),
            ["image_payload_mode"] = JoinedVisualProviderMetadata(metadataProviderArtifacts, "image_payload_mode", "refs_or_hashes_only"),
            [DesignConversionVisualAiProviderSelectionPolicy.ProviderSelectionPolicyKey] = providerSelectionPolicy,
            [DesignConversionVisualAiProviderSelectionPolicy.CandidateProvidersKey] = JoinedVisualProviderMetadata(metadataProviderArtifacts, DesignConversionVisualAiProviderSelectionPolicy.CandidateProvidersKey, DesignConversionVisualAiProviderSelectionPolicy.CandidateProviders),
            [DesignConversionVisualAiProviderSelectionPolicy.RequestedProviderKey] = JoinedVisualProviderMetadata(metadataProviderArtifacts, DesignConversionVisualAiProviderSelectionPolicy.RequestedProviderKey, "auto"),
            [DesignConversionVisualAiProviderSelectionPolicy.RequestedProviderNameKey] = JoinedVisualProviderMetadata(metadataProviderArtifacts, DesignConversionVisualAiProviderSelectionPolicy.RequestedProviderNameKey, "auto"),
            [DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey] = JoinedVisualProviderMetadata(metadataProviderArtifacts, DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey, "none"),
            [DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderNameKey] = JoinedVisualProviderMetadata(metadataProviderArtifacts, DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderNameKey, "none"),
            [DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey] = JoinedVisualProviderMetadata(metadataProviderArtifacts, DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey, "not_selected"),
            [DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey] = JoinedVisualProviderMetadata(metadataProviderArtifacts, DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey, "not_configured"),
            [DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackStatusKey] = JoinedVisualProviderMetadata(metadataProviderArtifacts, DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackStatusKey, "fallback_unavailable"),
            [DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackReasonKey] = JoinedVisualProviderMetadata(metadataProviderArtifacts, DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackReasonKey, "no_visual_provider_artifact"),
            [DesignConversionVisualAiProviderSelectionPolicy.UnavailableProviderCountKey] = JoinedVisualProviderMetadata(metadataProviderArtifacts, DesignConversionVisualAiProviderSelectionPolicy.UnavailableProviderCountKey, "3"),
            [DesignConversionVisualAiProviderSelectionPolicy.UnavailableProvidersKey] = JoinedVisualProviderMetadata(metadataProviderArtifacts, DesignConversionVisualAiProviderSelectionPolicy.UnavailableProvidersKey, DesignConversionVisualAiProviderSelectionPolicy.CandidateProviders),
            [DesignConversionVisualAiProviderSelectionPolicy.RealCloudProviderClaimedKey] = trustedNetworkCall.ToString()
        };

        if (trustedCanonicalVisualJudgeResult is not null)
        {
            metadata["visual_provider_visual_judge_score"] =
                trustedCanonicalVisualJudgeResult.Verdict.Score.ToString("0.####", CultureInfo.InvariantCulture);
            metadata["visual_provider_visual_judge_passed"] =
                trustedCanonicalVisualJudgeResult.Verdict.Passed.ToString();
            metadata["visual_provider_visual_judge_issue_count"] =
                trustedCanonicalVisualJudgeResult.Verdict.Defects.Count.ToString(CultureInfo.InvariantCulture);
            if (TryFormatTrustedVisualJudgeConfidence(trustedCanonicalVisualJudgeResult, out var confidence))
                metadata["visual_provider_visual_judge_confidence"] = confidence;
            if (trustedCanonicalVisualJudgeResult.Metadata.TryGetValue("provider_reported_passed", out var providerReportedPassed))
                metadata["visual_provider_reported_passed"] = SanitizeScalar(
                    providerReportedPassed,
                    "visual_provider_reported_passed",
                    new SortedSet<string>(StringComparer.Ordinal),
                    fallback: string.Empty);
            if (trustedCanonicalVisualJudgeResult.Metadata.TryGetValue("provider_server_passed_mismatch", out var providerServerPassedMismatch))
                metadata["visual_provider_server_passed_mismatch"] = SanitizeScalar(
                    providerServerPassedMismatch,
                    "visual_provider_server_passed_mismatch",
                    new SortedSet<string>(StringComparer.Ordinal),
                    fallback: string.Empty);
        }

        return metadata;
    }

    private static VisualProviderTrustEvidence BuildVisualProviderTrustEvidence(
        IReadOnlyList<DesignConversionQaComparisonArtifact> artifacts,
        IReadOnlyList<DesignConversionVisualJudgeResult> visualJudgeResults,
        string? currentJobId,
        string? currentRootRequestId)
    {
        var providerArtifacts = artifacts
            .Where(artifact => artifact.Metadata.ContainsKey("provider_kind") ||
                               artifact.Metadata.ContainsKey("external_visual_ai_configured") ||
                               artifact.Metadata.ContainsKey(DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey))
            .ToArray();
        var trustedVisualJudgeResults = visualJudgeResults
            .Where(result => DesignConversionModelServerNetworkBoundary.IsTrustedVisualJudgeNetworkCall(
                result,
                currentJobId,
                currentRootRequestId))
            .ToArray();
        var trustedProviderArtifacts = providerArtifacts
            .Where(artifact => trustedVisualJudgeResults.Any(result =>
                DesignConversionModelServerNetworkBoundary.MatchesTrustedVisualJudge(artifact, result) &&
                DesignConversionModelServerNetworkBoundary.IsTrustedVisualProviderNetworkCall(
                    artifact,
                    currentJobId,
                    currentRootRequestId,
                    providerResponseIdentityVerified: true,
                    providerResponseSchemaVerified: true)))
            .ToArray();
        var trustedNetworkCall = trustedProviderArtifacts.Length > 0;
        var metadataVisualJudgeResults = trustedNetworkCall
            ? trustedVisualJudgeResults
                .Where(result => trustedProviderArtifacts.Any(artifact =>
                    MatchesTrustedVisualJudgeArtifactIdentity(artifact, result)))
                .ToArray()
            : visualJudgeResults;

        return new VisualProviderTrustEvidence(
            ProviderArtifacts: providerArtifacts,
            TrustedVisualJudgeResults: trustedVisualJudgeResults,
            TrustedProviderArtifacts: trustedProviderArtifacts,
            MetadataVisualJudgeResults: metadataVisualJudgeResults,
            TrustedNetworkCall: trustedNetworkCall);
    }

    private static TrustedVisualProviderReceiptPair[] SelectTrustedReceiptPairs(
        IReadOnlyList<DesignConversionQaComparisonArtifact> trustedProviderArtifacts,
        IReadOnlyList<DesignConversionVisualJudgeResult> metadataVisualJudgeResults)
    {
        var pairs = trustedProviderArtifacts
            .OrderBy(artifact => artifact.ArtifactId, StringComparer.Ordinal)
            .ThenBy(artifact => artifact.ComparisonRequestId, StringComparer.Ordinal)
            .SelectMany(artifact => metadataVisualJudgeResults
                .Where(result => MatchesTrustedVisualJudgeArtifactIdentity(artifact, result))
                .OrderBy(result => result.JudgeResultId, StringComparer.Ordinal)
                .ThenBy(result => result.JudgeRequestId, StringComparer.Ordinal)
                .Select(result => new TrustedVisualProviderReceiptPair(artifact, result)))
            .GroupBy(TrustedVisualProviderReceiptPairIdentity, StringComparer.Ordinal)
            .Select(group => group.Last())
            .OrderBy(pair => pair.Artifact.ArtifactId, StringComparer.Ordinal)
            .ThenBy(pair => pair.Artifact.ComparisonRequestId, StringComparer.Ordinal)
            .ThenBy(pair => pair.VisualJudgeResult.JudgeResultId, StringComparer.Ordinal)
            .ThenBy(pair => pair.VisualJudgeResult.JudgeRequestId, StringComparer.Ordinal)
            .ToArray();

        return pairs;
    }

    private static DesignConversionQaComparisonArtifact[] SelectTrustedReceiptArtifacts(
        IReadOnlyList<TrustedVisualProviderReceiptPair> pairs)
        => pairs
            .GroupBy(pair => TrustedVisualProviderArtifactIdentity(pair.Artifact), StringComparer.Ordinal)
            .Select(group => group.First().Artifact)
            .OrderBy(artifact => artifact.ArtifactId, StringComparer.Ordinal)
            .ThenBy(artifact => artifact.ComparisonRequestId, StringComparer.Ordinal)
            .ToArray();

    private static string TrustedVisualProviderReceiptPairIdentity(TrustedVisualProviderReceiptPair pair)
        => string.Join(
            "\u001F",
            pair.Artifact.ArtifactId,
            pair.Artifact.ComparisonRequestId,
            pair.VisualJudgeResult.JudgeResultId,
            pair.VisualJudgeResult.JudgeRequestId);

    private static string TrustedVisualProviderArtifactIdentity(DesignConversionQaComparisonArtifact artifact)
        => string.Join("\u001F", artifact.ArtifactId, artifact.ComparisonRequestId);

    private static DesignConversionVisualJudgeResult[] SelectTrustedCanonicalVisualJudgeResults(
        IReadOnlyList<DesignConversionQaComparisonArtifact> trustedProviderArtifacts,
        IReadOnlyList<DesignConversionVisualJudgeResult> metadataVisualJudgeResults)
        => metadataVisualJudgeResults
            .Where(result => trustedProviderArtifacts.Any(artifact =>
                MatchesTrustedVisualJudgeArtifactIdentity(artifact, result)))
            .GroupBy(result => result.JudgeResultId, StringComparer.Ordinal)
            .Select(group => group.Last())
            .ToArray();

    private static bool MatchesTrustedVisualJudgeArtifactIdentity(
        DesignConversionQaComparisonArtifact artifact,
        DesignConversionVisualJudgeResult result)
        => DesignConversionModelServerNetworkBoundary.MatchesTrustedVisualJudge(artifact, result) &&
           MetadataEquals(
               artifact.Metadata,
               ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey,
               artifact.ArtifactId) &&
           MetadataEquals(
               result.Metadata,
               ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualResultIdKey,
               result.JudgeResultId) &&
           MetadataEquals(
               result.Provider.Metadata,
               ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualResultIdKey,
               result.JudgeResultId) &&
           MetadataValuesEqual(
               artifact.Metadata,
               result.Metadata,
               ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJobIdKey) &&
           MetadataValuesEqual(
               artifact.Metadata,
               result.Metadata,
               ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRequestIdKey) &&
           MetadataValuesEqual(
               artifact.Metadata,
               result.Metadata,
               ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRootRequestIdKey) &&
           MetadataEquals(
               result.Metadata,
               ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey,
               artifact.ComparisonRequestId) &&
           MetadataEquals(
               result.Provider.Metadata,
               ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey,
               artifact.ComparisonRequestId);

    private static bool TryFormatTrustedVisualJudgeConfidence(
        DesignConversionVisualJudgeResult result,
        out string confidence)
    {
        if (result.Metadata.TryGetValue("confidence", out var rawConfidence) &&
            double.TryParse(
                rawConfidence,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var parsed) &&
            parsed >= 0.0 &&
            parsed <= 1.0)
        {
            confidence = parsed.ToString("0.####", CultureInfo.InvariantCulture);
            return true;
        }

        confidence = string.Empty;
        return false;
    }

    private static bool MetadataEquals(
        IReadOnlyDictionary<string, string> metadata,
        string key,
        string expected)
        => metadata.TryGetValue(key, out var value) &&
           string.Equals(value, expected, StringComparison.Ordinal);

    private static bool MetadataValuesEqual(
        IReadOnlyDictionary<string, string> left,
        IReadOnlyDictionary<string, string> right,
        string key)
        => left.TryGetValue(key, out var leftValue) &&
           right.TryGetValue(key, out var rightValue) &&
           string.Equals(leftValue, rightValue, StringComparison.Ordinal);

    private static bool IsFiniteNumber(double value)
        => !double.IsNaN(value) && !double.IsInfinity(value);

    private static bool IsBlockingVisualIssueSeverity(string severity)
        => string.Equals(severity, "blocker", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(severity, "major", StringComparison.OrdinalIgnoreCase);

    private static string JoinedVisualProviderMetadata(
        IReadOnlyList<DesignConversionQaComparisonArtifact> artifacts,
        string key,
        string fallback = "none")
    {
        var redactions = new SortedSet<string>(StringComparer.Ordinal);
        var values = artifacts
            .Select(artifact => artifact.Metadata.TryGetValue(key, out var value) ? value : null)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => DesignConversionDpoLeakageGuard.SanitizeScalar(value, $"modelserver_status_visual_provider.{key}", redactions))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

        return values.Length == 0 ? fallback : string.Join("|", values);
    }

    private static string JoinedNoCloudVisualProviderSelectionPolicy(
        IReadOnlyList<DesignConversionQaComparisonArtifact> artifacts)
    {
        var policy = JoinedVisualProviderMetadata(
            artifacts,
            DesignConversionVisualAiProviderSelectionPolicy.ProviderSelectionPolicyKey,
            DesignConversionVisualAiProviderSelectionPolicy.PolicyName);
        var safePolicies = policy
            .Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => value.Trim())
            .Where(value => !string.Equals(
                value,
                DesignConversionVisualAiProviderSelectionPolicy.VerifiedCloudExecutionPolicyName,
                StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return safePolicies.Length == 0
            ? DesignConversionVisualAiProviderSelectionPolicy.PolicyName
            : string.Join("|", safePolicies);
    }

    private static string JoinedTrustedVisualProviderPairCallMetadata(
        IReadOnlyList<TrustedVisualProviderReceiptPair> pairs,
        string key,
        string fallback = "none")
    {
        var rawValues = new List<string?>(capacity: pairs.Count * 3);
        foreach (var pair in pairs)
        {
            if (string.Equals(
                    key,
                    ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey,
                    StringComparison.Ordinal))
            {
                rawValues.Add(pair.Artifact.ComparisonRequestId);
            }
            else if (string.Equals(
                         key,
                         ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualJudgeRequestIdKey,
                         StringComparison.Ordinal))
            {
                rawValues.Add(pair.VisualJudgeResult.JudgeRequestId);
            }
            rawValues.Add(pair.Artifact.Metadata.TryGetValue(key, out var artifactValue) ? artifactValue : null);
            rawValues.Add(pair.VisualJudgeResult.Metadata.TryGetValue(key, out var resultValue) ? resultValue : null);
            rawValues.Add(pair.VisualJudgeResult.Provider.Metadata.TryGetValue(key, out var providerValue) ? providerValue : null);
        }

        var redactions = new SortedSet<string>(StringComparer.Ordinal);
        var values = rawValues
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => DesignConversionDpoLeakageGuard.SanitizeScalar(value, $"modelserver_status_visual_provider.{key}", redactions))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

        return values.Length == 0 ? fallback : string.Join("|", values);
    }

    private static string JoinedVisualJudgeResultMetadata(
        IReadOnlyList<DesignConversionVisualJudgeResult> visualJudgeResults,
        string key,
        string fallback = "none")
    {
        var redactions = new SortedSet<string>(StringComparer.Ordinal);
        var values = visualJudgeResults
            .Select(result => result.Metadata.TryGetValue(key, out var value) ? value : null)
            .Concat(visualJudgeResults.Select(result => result.Provider.Metadata.TryGetValue(key, out var value) ? value : null))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => DesignConversionDpoLeakageGuard.SanitizeScalar(value, $"modelserver_status_visual_provider.{key}", redactions))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

        return values.Length == 0 ? fallback : string.Join("|", values);
    }

    private static string ReadScaffoldMarker(
        DesignConversionInternalEvaluationResult? evaluation,
        string key)
        => evaluation?.ScaffoldMarkers.TryGetValue(key, out var value) == true
            ? value
            : string.Empty;

    private static bool LearnedInferenceApplied(DesignConversionInternalEvaluationResult? evaluation)
        => HasServerOwnedLearnedInferenceEvidence(evaluation);

    private static bool HasServerOwnedLearnedInferenceEvidence(DesignConversionInternalEvaluationResult? evaluation)
        => HasAcceptedRuntimeLearnedEvidence(evaluation?.RuntimeFlowResult.EvidenceRefs);

    private static bool HasAcceptedRuntimeLearnedEvidence(IReadOnlyList<string>? evidenceRefs)
        => (evidenceRefs ?? []).Any(IsAcceptedRuntimeLearnedEvidence);

    private static bool IsAcceptedRuntimeLearnedEvidence(string evidenceRef)
        => string.Equals(evidenceRef, DesignConversionMachine.LearnedInferenceAppliedEvidence, StringComparison.Ordinal) ||
           string.Equals(evidenceRef, DesignConversionMachine.DpoTrainerPolicyCheckpointRuntimeScoringEvidence, StringComparison.Ordinal) ||
           string.Equals(evidenceRef, DesignConversionMachine.DpoPolicyDirectInferenceScoreEvidence, StringComparison.Ordinal);

    private static bool HasUnverifiedLearnedStatusProvenanceClaim(IReadOnlyDictionary<string, string> provenance)
        => provenance.Any(pair =>
            IsTruthyValue(pair.Value) &&
            !ServerOwnedNonAuthorityProvenanceFacts.Contains(pair.Key));

    private static string MeasurementStatus(DesignConversionThinClientStatusResult status)
        => string.Equals(status.ContractStatus, DesignConversionThinClientStatus.Completed, StringComparison.Ordinal)
            ? "runtime_evaluation_completed_local_only"
            : "not_proven";

    private static bool IsBoundaryEvidence(string value)
        => value.Contains("verified=false", StringComparison.Ordinal) ||
           value.Contains("ModelUploadReady=false", StringComparison.Ordinal) ||
           value.Contains("deterministic_logic_claimed_as_learning", StringComparison.Ordinal) ||
           value.Contains("runtime_created_dpo_candidate", StringComparison.Ordinal) ||
           value.Contains("local_artifact_db", StringComparison.Ordinal) ||
           IsTeacherCallCapBoundaryEvidence(value) ||
           value.Contains("product_not_ready", StringComparison.Ordinal);

    private static DesignConversionThinClientStatusResult ReconcileRuntimeStatusBoundaryWarnings(
        DesignConversionThinClientStatusResult status,
        IReadOnlyDictionary<string, string> boundaryMetadata)
        => status with
        {
            Job = status.Job with
            {
                Metadata = SanitizePublicRuntimeMetadata(status.Job.Metadata)
            },
            Warnings = ReconcileBoundaryMarkers(
                    SanitizeStatusBoundaryValues(status.Warnings, "runtime_status_warning"),
                    boundaryMetadata,
                    sort: false)
                .ToArray(),
            Provenance = SanitizePublicRuntimeMetadata(status.Provenance)
        };

    private static IReadOnlyList<string> SanitizeStatusBoundaryValues(
        IEnumerable<string> values,
        string scope)
    {
        var redactions = new SortedSet<string>(StringComparer.Ordinal);
        return (values ?? [])
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => SanitizeScalar(value, scope, redactions, fallback: "status-boundary-ref"))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyDictionary<string, string> SanitizePublicRuntimeMetadata(
        IReadOnlyDictionary<string, string>? metadata)
    {
        var sanitized = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in metadata ?? new Dictionary<string, string>())
        {
            if (IsCallerOriginRuntimeMetadata(pair.Key))
                continue;

            if (IsPublicRawTenantMetadata(pair.Key, pair.Value))
                continue;

            sanitized[pair.Key] = pair.Value;
        }

        return sanitized;
    }

    private static bool IsCallerOriginRuntimeMetadata(string key)
        => key.StartsWith("request_metadata:", StringComparison.OrdinalIgnoreCase) ||
           key.StartsWith("budget_metadata:", StringComparison.OrdinalIgnoreCase);

    private static bool IsPublicRawTenantMetadata(string key, string? value)
    {
        if (string.Equals(key, ServerOwnedRouteAdmissionTenantIdRefProvenanceKey, StringComparison.Ordinal))
            return !DesignConversionThinClientJobRuntime.IsCanonicalRouteAdmissionTenantIdRef(value);

        if (string.Equals(key, ServerOwnedRouteAdmissionTenantIdProvenanceKey, StringComparison.Ordinal))
            return true;

        if (IsCallerTenantMetadataKey(key))
            return true;

        if (key.Contains("tenant", StringComparison.OrdinalIgnoreCase) &&
            !key.EndsWith("_ref", StringComparison.OrdinalIgnoreCase) &&
            !key.EndsWith("_bound", StringComparison.OrdinalIgnoreCase) &&
            !key.EndsWith("_exposed", StringComparison.OrdinalIgnoreCase) &&
            !key.EndsWith("_authoritative", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return LooksLikeRawTenantValue(value);
    }

    private static bool IsCallerTenantMetadataKey(string key)
        => string.Equals(key, "request_metadata:tenant_id", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(key, "request_metadata:tenantId", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(key, "request_metadata:tenant", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(key, "request_metadata:x-tenant-id", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(key, "budget_metadata:tenant_id", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(key, "budget_metadata:tenantId", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(key, "budget_metadata:tenant", StringComparison.OrdinalIgnoreCase);

    private static bool LooksLikeRawTenantValue(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Contains("tenant:", StringComparison.OrdinalIgnoreCase);

    private static IReadOnlyList<string> ReconcileBoundaryMarkers(
        IEnumerable<string> values,
        IReadOnlyDictionary<string, string> boundaryMetadata,
        bool sort)
    {
        var reconciled = values
            .Where(value => !IsStaleFalseServerComputedReadinessMarker(value, boundaryMetadata))
            .Distinct(StringComparer.Ordinal);

        return sort
            ? reconciled.OrderBy(value => value, StringComparer.Ordinal).ToArray()
            : reconciled.ToArray();
    }

    private static bool IsStaleFalseServerComputedReadinessMarker(
        string value,
        IReadOnlyDictionary<string, string> boundaryMetadata)
        => ServerComputedReadinessMarkers.Any(marker =>
            string.Equals(value, $"{marker}=false", StringComparison.Ordinal) &&
            IsTruthy(boundaryMetadata, marker));

    private static bool IsTruthy(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) &&
           IsTruthyValue(value);

    private static bool IsTruthyValue(string? value)
        => bool.TryParse(value, out var parsed) && parsed;

    private sealed record LocalArtifactPersistenceReadback(
        bool Verified,
        string Authority,
        string State,
        string FailureReason,
        bool AppendReported,
        bool FreshReadbackSuccess,
        string QueryJobId,
        string QueryRequestId,
        int FreshReadbackRecordCount,
        int ServerOwnedRecordCount,
        string RecordId,
        string RecordSource,
        bool RecordJobBound,
        bool RecordRequestBound,
        bool RecordRefsOnly,
        bool RawContentFree,
        bool NoLocalPathPublic,
        int ExpectedRenderRefCount,
        int RecordRenderRefCount,
        int BoundRenderRefCount,
        bool ConversionResultBound,
        bool RecordIndexConsistent)
    {
        public static LocalArtifactPersistenceReadback NotVerified(
            string authority,
            string failureReason,
            bool appendReported,
            string queryJobId,
            string queryRequestId,
            bool freshReadbackSuccess = false)
            => new(
                Verified: false,
                Authority: authority,
                State: "not_verified_by_server_append_and_fresh_job_request_readback",
                FailureReason: failureReason,
                AppendReported: appendReported,
                FreshReadbackSuccess: freshReadbackSuccess,
                QueryJobId: queryJobId,
                QueryRequestId: queryRequestId,
                FreshReadbackRecordCount: 0,
                ServerOwnedRecordCount: 0,
                RecordId: string.Empty,
                RecordSource: string.Empty,
                RecordJobBound: false,
                RecordRequestBound: false,
                RecordRefsOnly: false,
                RawContentFree: false,
                NoLocalPathPublic: false,
                ExpectedRenderRefCount: 0,
                RecordRenderRefCount: 0,
                BoundRenderRefCount: 0,
                ConversionResultBound: false,
                RecordIndexConsistent: false);
    }

    private sealed record PersistenceRecordBinding(
        bool RecordJobBound,
        bool RecordRequestBound,
        bool ConversionResultBound,
        int ExpectedRenderRefCount,
        int RecordRenderRefCount,
        int BoundRenderRefCount,
        bool RenderRefsBound);

    private sealed record TrustedVisualCostEntitlementValidation(
        DesignConversionModelServerTrustedVisualCostEntitlement? Entitlement);

    private sealed record TrustedMetadataValue(string Value);

    private sealed record TrustedMetadataOccurrence(
        string Key,
        string Value);

    private sealed record BrowserE2eReadiness(
        bool Verified,
        string Authority,
        string State,
        string FailureReason,
        int RequiredViewportCount,
        int ObservedViewportCount,
        int VerifiedRenderArtifactCount,
        int PngSignatureVerifiedCount,
        int Sha256VerifiedCount,
        int NonBlankVerifiedCount,
        int DimensionVerifiedCount,
        int BrowserProvenanceCount,
        string ProviderKind,
        string ArtifactKind)
    {
        public static BrowserE2eReadiness NotVerified(
            string authority,
            string failureReason,
            string providerKind,
            string artifactKind,
            int requiredViewportCount,
            int observedViewportCount)
            => new(
                Verified: false,
                Authority: authority,
                State: "not_verified_by_server_owned_browser_render_receipts",
                FailureReason: failureReason,
                RequiredViewportCount: requiredViewportCount,
                ObservedViewportCount: observedViewportCount,
                VerifiedRenderArtifactCount: 0,
                PngSignatureVerifiedCount: 0,
                Sha256VerifiedCount: 0,
                NonBlankVerifiedCount: 0,
                DimensionVerifiedCount: 0,
                BrowserProvenanceCount: 0,
                ProviderKind: providerKind,
                ArtifactKind: artifactKind);
    }

    private sealed record VisualFidelityReadiness(
        bool Verified,
        string Authority,
        string State,
        string FailureReason,
        string Score,
        string AcceptanceThreshold,
        string Confidence,
        int IssueCount,
        int BlockingIssueCount,
        int CanonicalResultCount,
        int BoundArtifactCount,
        bool Passed,
        bool ScoreThresholdMet,
        bool ProviderReceiptVerified,
        bool ResponseIdentityVerified,
        bool ResponseSchemaVerified)
    {
        public static VisualFidelityReadiness NotVerified(
            string authority,
            string failureReason,
            string score,
            string acceptanceThreshold,
            string confidence)
            => new(
                Verified: false,
                Authority: authority,
                State: "not_verified_by_server_owned_browser_and_visual_provider_receipts",
                FailureReason: failureReason,
                Score: score,
                AcceptanceThreshold: acceptanceThreshold,
                Confidence: confidence,
                IssueCount: 0,
                BlockingIssueCount: 0,
                CanonicalResultCount: 0,
                BoundArtifactCount: 0,
                Passed: false,
                ScoreThresholdMet: false,
                ProviderReceiptVerified: false,
                ResponseIdentityVerified: false,
                ResponseSchemaVerified: false);
    }

    private sealed record VisualProviderTrustEvidence(
        IReadOnlyList<DesignConversionQaComparisonArtifact> ProviderArtifacts,
        IReadOnlyList<DesignConversionVisualJudgeResult> TrustedVisualJudgeResults,
        IReadOnlyList<DesignConversionQaComparisonArtifact> TrustedProviderArtifacts,
        IReadOnlyList<DesignConversionVisualJudgeResult> MetadataVisualJudgeResults,
        bool TrustedNetworkCall);

    private sealed record TrustedVisualProviderReceiptPair(
        DesignConversionQaComparisonArtifact Artifact,
        DesignConversionVisualJudgeResult VisualJudgeResult);

    private static IReadOnlyDictionary<string, string> SanitizeMetadata(
        IReadOnlyDictionary<string, string>? metadata,
        string scope,
        SortedSet<string> redactions)
    {
        var sanitized = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in metadata ?? new Dictionary<string, string>())
        {
            var key = SanitizeScalar(pair.Key, $"{scope}_key", redactions, fallback: "metadata_key");
            if (string.IsNullOrWhiteSpace(key))
                continue;
            sanitized[key] = SanitizeScalar(pair.Value, $"{scope}.{key}", redactions, fallback: string.Empty);
        }

        return sanitized;
    }

    private static string SanitizeScalar(
        string? value,
        string scope,
        SortedSet<string> redactions,
        string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;

        var sanitized = DesignConversionDpoLeakageGuard.SanitizeScalar(value, scope, redactions);
        if (LooksLikeRawPayloadOrPrivateToken(sanitized))
        {
            redactions.Add($"{scope}:hashed_private_or_raw_payload");
            return DesignConversionDpoLeakageGuard.HashRef("redacted-api-scalar", value);
        }

        return sanitized;
    }

    private static bool LooksLikeRawPayloadOrPrivateToken(string value)
        => value.Length > 160 ||
           value.Contains("PRIVATE", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("SECRET", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("<html", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("<main", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("<div", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("{\"", StringComparison.Ordinal) ||
           value.Contains("data-secret", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeSha256(string? sha256, string? payload)
    {
        if (!string.IsNullOrWhiteSpace(sha256) && !LooksLikeRawPayloadOrPrivateToken(sha256))
            return sha256;

        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(payload ?? string.Empty))).ToLowerInvariant();
        return $"sha256:{hash}";
    }

    private enum PromotedLocalModelVerificationAuthority
    {
        None,
        FreshPromotionLoad,
        ProductionActivatedRuntime
    }

    private sealed class PromotedLocalModelVerificationProof
    {
        private PromotedLocalModelVerificationProof(DesignConversionPromotedLocalModel model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public DesignConversionPromotedLocalModel Model { get; }

        public static PromotedLocalModelVerificationProof FromFreshPromotionLoad(
            DesignConversionPromotedLocalModel model)
            => new(model);
    }

    private sealed record PromotedLocalModelResolution(
        DesignConversionPromotedLocalModel? Model,
        PromotedLocalModelVerificationAuthority Authority)
    {
        public const string AuthorityMarkerPrefix = "local_model_verification_authority=";
        public const string NoAuthorityValue = "not_proven";

        public bool VerificationAuthorized
            => Model is not null && Authority != PromotedLocalModelVerificationAuthority.None;

        public string AuthorityValue
            => Authority switch
            {
                PromotedLocalModelVerificationAuthority.FreshPromotionLoad =>
                    "fresh_promotion_loader_result",
                PromotedLocalModelVerificationAuthority.ProductionActivatedRuntime =>
                    "production_activated_runtime_registry_checkpoint_load",
                _ => NoAuthorityValue
            };

        public string AuthorityMarker
            => AuthorityMarkerPrefix + AuthorityValue;
    }

    private sealed class CapturingThinClientJobEvaluator(
        IDesignConversionThinClientJobEvaluator inner) :
        IDesignConversionThinClientJobEvaluator,
        IDesignConversionRepositoryOwnedDpoTransactionContextAwareEvaluator
    {
        private readonly IDesignConversionThinClientJobEvaluator _inner =
            inner ?? throw new ArgumentNullException(nameof(inner));
        private readonly Dictionary<string, DesignConversionInternalEvaluationResult> _evaluations = new(StringComparer.Ordinal);

        public async Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
            => await EvaluateAndCaptureAsync(
                request,
                repositoryOwnedDpoTransactionContext: null,
                cancellationToken);

        public async Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            DesignConversionRepositoryOwnedDpoTransactionContext?
                repositoryOwnedDpoTransactionContext,
            CancellationToken cancellationToken = default)
            => await EvaluateAndCaptureAsync(
                request,
                repositoryOwnedDpoTransactionContext,
                cancellationToken);

        private async Task<OperationResult<DesignConversionInternalEvaluationResult>>
            EvaluateAndCaptureAsync(
                DesignConversionInternalEvaluateRequest request,
                DesignConversionRepositoryOwnedDpoTransactionContext?
                    repositoryOwnedDpoTransactionContext,
                CancellationToken cancellationToken)
        {
            var result = _inner is
                IDesignConversionRepositoryOwnedDpoTransactionContextAwareEvaluator
                    repositoryOwnedDpoEvaluator
                ? await repositoryOwnedDpoEvaluator.EvaluateAsync(
                    request,
                    repositoryOwnedDpoTransactionContext,
                    cancellationToken)
                : await _inner.EvaluateAsync(request, cancellationToken);
            if (result.IsSuccess && result.Value is not null)
            {
                lock (_evaluations)
                {
                    _evaluations[request.JobId] = result.Value;
                    _evaluations[result.Value.JobId] = result.Value;
                }
            }

            return result;
        }

        public bool TryGetEvaluation(string jobId, out DesignConversionInternalEvaluationResult? evaluation)
        {
            lock (_evaluations)
                return _evaluations.TryGetValue(jobId, out evaluation);
        }
    }

    private sealed class RecordingRuntimeRejectedEvaluationSource(
        IDesignConversionRuntimeRejectedEvaluationSource inner)
        : IDesignConversionRuntimeRejectedEvaluationSource
    {
        private readonly IDesignConversionRuntimeRejectedEvaluationSource _inner =
            inner ?? throw new ArgumentNullException(nameof(inner));
        private readonly object _gate = new();
        private readonly HashSet<string> _resolvedChosenJobIds = new(StringComparer.Ordinal);

        public async Task<OperationResult<DesignConversionRuntimeRejectedEvaluationReference?>>
            ResolveMeasuredRejectedEvaluationAsync(
                DesignConversionRuntimeRejectedEvaluationLookup chosenEvaluation,
                CancellationToken cancellationToken = default)
        {
            var result = await _inner.ResolveMeasuredRejectedEvaluationAsync(
                chosenEvaluation,
                cancellationToken);
            if (result.IsSuccess &&
                result.Value is not null &&
                !string.IsNullOrWhiteSpace(chosenEvaluation?.ChosenJobId))
            {
                lock (_gate)
                    _resolvedChosenJobIds.Add(chosenEvaluation.ChosenJobId);
            }

            return result;
        }

        public bool HasResolved(string chosenJobId)
        {
            if (string.IsNullOrWhiteSpace(chosenJobId))
                return false;

            lock (_gate)
                return _resolvedChosenJobIds.Contains(chosenJobId);
        }
    }
}
