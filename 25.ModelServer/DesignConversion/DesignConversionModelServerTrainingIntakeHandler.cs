// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;

public sealed record DesignConversionModelServerTrainingIntakeRequest(
    string Action,
    string? IntakeId = null,
    string? TenantId = null,
    string? IdempotencyKey = null,
    DesignConversionApprovedDpoHandoffArtifact? Handoff = null,
    string? RequestedByRef = null,
    string? Reason = null,
    DateTimeOffset? RequestedAtUtc = null,
    string? PromotionOutputDirectory = null,
    DesignConversionTrainingPipelineOptions? Options = null,
    IReadOnlyDictionary<string, string>? Metadata = null,
    string? PromotedLocalModelRef = null,
    string? LocalArtifactRecordId = null,
    string? CandidateOrTrainingRecordId = null,
    DesignConversionLocalArtifactDbQuery? Query = null);

public sealed record DesignConversionModelServerRepositoryOwnedTrainingOptions(
    DesignConversionRepositoryOwnedTrainingRunContextSpec? RunContextSpec,
    string? ConfigurationErrorCode = null,
    string? ConfigurationErrorMessage = null)
{
    public const string ServerConfigurationRequiredErrorCode =
        "TRAINING_INTAKE_REPOSITORY_OWNED_CONTEXT_SERVER_CONFIGURATION_REQUIRED";
    public const string ServerConfigurationInvalidErrorCode =
        "TRAINING_INTAKE_REPOSITORY_OWNED_CONTEXT_SERVER_CONFIGURATION_INVALID";
    public const string ServerConfigurationDisabledIsNotBypassErrorCode =
        "TRAINING_INTAKE_REPOSITORY_OWNED_CONTEXT_DISABLED_IS_NOT_BYPASS";

    public static DesignConversionModelServerRepositoryOwnedTrainingOptions
        MissingServerConfiguration()
        => new(
            RunContextSpec: null,
            ConfigurationErrorCode: ServerConfigurationRequiredErrorCode,
            ConfigurationErrorMessage:
            "DesignConversion repository-owned training requires server-side repository training configuration.");

    public static DesignConversionModelServerRepositoryOwnedTrainingOptions
        InvalidServerConfiguration()
        => new(
            RunContextSpec: null,
            ConfigurationErrorCode: ServerConfigurationInvalidErrorCode,
            ConfigurationErrorMessage:
            "DesignConversion repository-owned training server configuration is invalid.");

    public static DesignConversionModelServerRepositoryOwnedTrainingOptions
        DisabledIsNotBypass()
        => new(
            RunContextSpec: null,
            ConfigurationErrorCode: ServerConfigurationDisabledIsNotBypassErrorCode,
            ConfigurationErrorMessage:
            "DesignConversion repository-owned training is disabled; legacy training fallback is not permitted for production training.");

    public override string ToString()
        => $"{nameof(DesignConversionModelServerRepositoryOwnedTrainingOptions)} {{ " +
           $"{nameof(RunContextSpec)}Configured = {(RunContextSpec is not null).ToString()}, " +
           $"{nameof(RunContextSpec)} = [REDACTED], " +
           $"{nameof(ConfigurationErrorCode)}Configured = {(!string.IsNullOrWhiteSpace(ConfigurationErrorCode)).ToString()}, " +
           $"{nameof(ConfigurationErrorMessage)} = [REDACTED] }}";
}

public sealed record DesignConversionModelServerTrainingIntakeRecordRef(
    string IntakeId,
    string IntakeRef,
    string SourceHandoffArtifactId,
    string SourceHandoffArtifactRef,
    int ApprovedCandidateCount,
    int AcceptedCandidateCount,
    IReadOnlyList<string> CandidateIds,
    IReadOnlyList<string> TrainingRecordIds,
    bool TrainingIntakeOnly,
    bool TrainingNotStarted,
    bool NotTrained,
    bool WeightsNotProduced,
    bool CheckpointNotMutated,
    bool ModelUploadNotReady,
    bool ProductNotReady,
    bool HeldOutNotProven,
    bool SourceDisjointNotProven,
    IReadOnlyList<string> Warnings,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionModelServerTrainingIntakePreflightRef(
    string PreflightId,
    string TrainingIntakeId,
    int AcceptedCandidateCount,
    bool MayStartTraining,
    string SourceManifestStatus,
    string SourceDisjointStatus,
    bool TrainingStarted,
    bool CheckpointMutated,
    bool WeightsProduced,
    bool ModelUploadReady,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> BlockReasons,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionModelServerApprovedRuntimeDpoPreflightRef(
    string LocalArtifactRecordId,
    string CandidateId,
    string TrainingRecordId,
    string UnitId,
    string SourceHandoffArtifactRef,
    string IntakeId,
    string IntakeRef,
    string StableIntakeIdRef,
    string IdempotencyKeyRef,
    bool IdempotencyReplayHit,
    bool IdempotencyCreatedNewSideEffect,
    string PreflightId,
    string PreflightStatus,
    bool MayStartTraining,
    int AcceptedCandidateCount,
    int DpoStepCount,
    string MeasurementGateId,
    string MeasurementGateUnitId,
    string MeasurementGateContractRef,
    DateTimeOffset PersistedRequestedAtUtc,
    bool TrainingRunnerInvoked,
    bool TrainingStarted,
    bool CheckpointMutated,
    bool WeightsProduced,
    bool ModelUploadReady,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> Warnings,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionModelServerTrainingIntakeRunnerEvidenceRef(
    string BoundaryDecisionKind,
    bool DatasetCountGatePassed,
    int RequiredTrainRecords,
    int RequiredHeldOutRecords,
    int ActualTrainRecords,
    int ActualHeldOutRecords,
    bool SourceDisjoint,
    string SourceManifestStatus,
    string SourceDisjointStatus,
    bool MachineLifecycleHookAttempted,
    bool TrainingStarted,
    string TrainingExecutionStatus,
    string RunnerBlockedReason,
    string CheckpointCandidateStatus,
    bool CheckpointCandidateCreated,
    string CheckpointMutationStatus,
    bool CheckpointMutationEvidenceProven,
    bool CheckpointContainsRealTrainedWeights,
    string WeightProofStatus,
    string FreshLoadStatus,
    string ContinueTrainingStatus,
    string MeasurementGateStatus,
    string PromotionStatus,
    bool PromotionUploadAllowed,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> FailureReasons,
    IReadOnlyList<string> Warnings);

public sealed record DesignConversionModelServerTrainingIntakeRunRef(
    string RunId,
    string TrainingIntakeId,
    string PreflightId,
    int AcceptedCandidateCount,
    int ResolvedTrainingRecordCount,
    bool MayStartTraining,
    string SourceManifestStatus,
    string SourceDisjointStatus,
    bool TrainingRunnerInvoked,
    bool LocalLifecycleHookAttempted,
    bool TrainingStarted,
    bool CheckpointMutated,
    bool CheckpointCandidateCreated,
    bool WeightsProduced,
    bool ModelUploadReady,
    string TrainingExecutionBoundary,
    string TrainingExecutionStatus,
    string RunnerBlockedReason,
    string CheckpointStatus,
    string CheckpointMutationStatus,
    string WeightProofStatus,
    string MeasurementGateStatus,
    string PromotionGateStatus,
    string UploadReadinessStatus,
    IReadOnlyList<string> PromotionBlockedReasons,
    DesignConversionModelServerTrainingIntakeRunnerEvidenceRef? RunnerEvidence,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> BlockReasons,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionModelServerTrainingIntakeLocalPromotionRef(
    DesignConversionModelServerTrainingIntakeRunRef TrainingRun,
    bool LocalPromotionReady,
    bool ExternalCloudUploadAttempted,
    string PromotionOutputDirectoryRef,
    string PromotedCheckpointRef,
    string? PromotionUploadRef,
    string? PromotedLocalModelId,
    string? PromotedLocalModelVersion,
    string? PromotedCheckpointSha256,
    bool? CheckpointFreshLoadVerified,
    bool? CheckpointContainsRealTrainedWeights,
    bool? PromotionManifestClaimed,
    bool? LocalOnly,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> BlockReasons,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionModelServerTrainingIntakeLifecycleStatusRef(
    string CommandId,
    string RequestedByRef,
    string RequestReasonRef,
    string RequestedOperation,
    bool ContinueTrainingRequested,
    string ContinueTrainingStatus,
    bool LocalContinueTrainingVerified,
    bool UploadRequested,
    string UploadStatus,
    string CloudUploadStatus,
    string LocalPublishPackageRef,
    string LocalPublishManifestRef,
    string LocalPublishBackupRef,
    string RollbackBoundaryRef,
    string ContinuedFromCheckpointRef,
    bool UploadAttempted,
    bool DeployRequested,
    string DeploymentStatus,
    bool DeploymentAttempted,
    bool DeploymentVerified,
    bool ProductReady,
    string TrainingIntakeId,
    string TrainingRunId,
    string CheckpointStatus,
    string CheckpointMutationStatus,
    string PromotedCheckpointId,
    string PromotedCheckpointPathRef,
    string? PromotionUploadRef,
    string? PromotedRuntimeModelId,
    string? PromotedLocalModelVersion,
    bool LocalPromotionReady,
    bool LocalOnly,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> BlockReasons,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionModelServerTrainingIntakeLocalPromotionRollbackRef(
    string RollbackId,
    bool RollbackApplied,
    bool PriorStateRestored,
    bool NoPriorStateRecorded,
    string RolledBackLocalModelRef,
    string PreviousActiveLocalModelRef,
    string RestoredActiveLocalModelRef,
    string RestoredActiveCheckpointSha256,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> BlockReasons,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionModelServerDpoCandidateApprovalRef(
    string LocalArtifactRecordId,
    string CandidateId,
    string TrainingRecordId,
    string UnitId,
    double UsefulnessScore,
    string ApprovalStatus,
    string ReviewStatus,
    bool ApprovedForTraining,
    string ApprovedByRef,
    string ApprovalReasonRef,
    DateTimeOffset ApprovedAtUtc,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionModelServerTrainingIntakeResponse(
    string Endpoint,
    string Method,
    string HttpSeam,
    string ApiShape,
    string RuntimeSurface,
    int StatusCode,
    string ContractStatus,
    string Action,
    string? IntakeId,
    int IntakeCount,
    IReadOnlyList<DesignConversionModelServerTrainingIntakeRecordRef> Intakes,
    DesignConversionModelServerTrainingIntakeRecordRef? Intake,
    DesignConversionModelServerTrainingIntakePreflightRef? Preflight,
    DesignConversionModelServerApprovedRuntimeDpoPreflightRef? ApprovedRuntimeDpoPreflight,
    DesignConversionModelServerTrainingIntakeRunRef? TrainingRun,
    DesignConversionModelServerTrainingIntakeLocalPromotionRef? LocalPromotion,
    DesignConversionModelServerTrainingIntakeLocalPromotionRollbackRef? LocalPromotionRollback,
    DesignConversionModelServerTrainingIntakeLifecycleStatusRef? LifecycleStatus,
    DesignConversionModelServerDpoCandidateApprovalRef? CandidateApproval,
    DesignConversionApprovedDpoHandoffArtifact? ApprovedHandoff,
    bool ProductReady,
    bool DeploymentVerified,
    bool PublicHttpEndpointVerified,
    IReadOnlyList<string> Warnings,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed class DesignConversionModelServerTrainingIntakeHandler
{
    public const string EndpointPath = "/api/design-conversion/training-intake";
    public const string ApiShape = "design_conversion_modelserver_training_intake_route_v1";
    public const string RuntimeSurface = "modelserver_design_conversion_training_intake_route";
    public const string LocalTrainingIntakeRouteBoundary =
        "local_training_intake_route_refs_only_no_socket_no_public_deployment";

    public const string ActionList = "list";
    public const string ActionCreate = "create";
    public const string ActionPreflight = "preflight";
    public const string ActionRun = "run";
    public const string ActionPromoteLocal = "promote-local";
    public const string ActionContinueTrainingUploadStatus = "continue-training-upload-status";
    public const string ActionRollbackLocalPromotion = "rollback-local-promotion";
    public const string ActionApproveRuntimeCandidate = "approve-runtime-candidate";
    public const string ActionExportApprovedRuntimeCandidates = "export-approved-runtime-candidates";
    public const string ActionPostApprovalPreflight = "post-approval-preflight";
    public const string TrainingIntakeAuthTokenHeaderName = "X-Xiigen-Training-Intake-Token";
    private const string MeasurementGateThresholdsInvalidCode =
        "TRAINING_INTAKE_MEASUREMENT_GATE_THRESHOLDS_INVALID";
    private const string RepositoryOwnedContextRequiresServerConfigurationCode =
        "TRAINING_INTAKE_REPOSITORY_OWNED_CONTEXT_REQUIRES_SERVER_CONFIGURATION";
    private const string RepositoryOwnedContextOptionsRequiredCode =
        "TRAINING_INTAKE_REPOSITORY_OWNED_CONTEXT_OPTIONS_REQUIRED";

    private readonly DesignConversionDpoUsefulnessApi _api;
    private readonly string? _expectedTrainingIntakeAuthToken;
    private readonly DesignConversionModelServerModelLifecycleTokenKeyRing?
        _trainingIntakeAuthTokenAuthority;
    private readonly string? _expectedTrainingIntakeTenantId;
    private readonly string? _expectedTrainingIntakeTenantIdRef;
    private readonly bool _remotePromotionConfigured;
    private readonly bool _enforceServerOwnedMeasurementGateThresholds;
    private readonly DesignConversionModelServerMeasurementGateThresholdConfiguration?
        _measurementGateThresholdConfiguration;
    private readonly DesignConversionModelServerRepositoryOwnedTrainingOptions?
        _repositoryOwnedTrainingOptions;

    public DesignConversionModelServerTrainingIntakeHandler(
        DesignConversionDpoUsefulnessApi api,
        string? expectedTrainingIntakeAuthToken = null)
        : this(
            api,
            expectedTrainingIntakeAuthToken,
            trainingIntakeAuthTokenAuthority: null,
            null,
            false,
            enforceServerOwnedMeasurementGateThresholds: false,
            measurementGateThresholdConfiguration: null,
            repositoryOwnedTrainingOptions: null)
    {
    }

    internal static DesignConversionModelServerTrainingIntakeHandler CreateForProductionHost(
        DesignConversionDpoUsefulnessApi api,
        DesignConversionModelServerMeasurementGateThresholdConfiguration?
            measurementGateThresholdConfiguration,
        DesignConversionModelServerModelLifecycleTokenKeyRing?
            trainingIntakeAuthTokenAuthority = null,
        DesignConversionModelServerRepositoryOwnedTrainingOptions?
            repositoryOwnedTrainingOptions = null)
        => new(
            api,
            null,
            trainingIntakeAuthTokenAuthority,
            null,
            remotePromotionConfigured: false,
            enforceServerOwnedMeasurementGateThresholds: true,
            measurementGateThresholdConfiguration,
            repositoryOwnedTrainingOptions);

    internal static DesignConversionModelServerTrainingIntakeHandler CreateForValidatedPromotionHost(
        DesignConversionDpoUsefulnessApi api,
        string? expectedTrainingIntakeTenantId,
        bool remotePromotionConfigured,
        DesignConversionModelServerMeasurementGateThresholdConfiguration?
            measurementGateThresholdConfiguration,
        DesignConversionModelServerModelLifecycleTokenKeyRing?
            trainingIntakeAuthTokenAuthority = null,
        DesignConversionModelServerRepositoryOwnedTrainingOptions?
            repositoryOwnedTrainingOptions = null)
        => new(
            api,
            null,
            trainingIntakeAuthTokenAuthority,
            expectedTrainingIntakeTenantId,
            remotePromotionConfigured,
            enforceServerOwnedMeasurementGateThresholds: true,
            measurementGateThresholdConfiguration,
            repositoryOwnedTrainingOptions);

    private DesignConversionModelServerTrainingIntakeHandler(
        DesignConversionDpoUsefulnessApi api,
        string? expectedTrainingIntakeAuthToken,
        DesignConversionModelServerModelLifecycleTokenKeyRing?
            trainingIntakeAuthTokenAuthority,
        string? expectedTrainingIntakeTenantId,
        bool remotePromotionConfigured,
        bool enforceServerOwnedMeasurementGateThresholds,
        DesignConversionModelServerMeasurementGateThresholdConfiguration?
            measurementGateThresholdConfiguration,
        DesignConversionModelServerRepositoryOwnedTrainingOptions?
            repositoryOwnedTrainingOptions)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
        _expectedTrainingIntakeAuthToken = string.IsNullOrWhiteSpace(expectedTrainingIntakeAuthToken)
            ? null
            : expectedTrainingIntakeAuthToken.Trim();
        _trainingIntakeAuthTokenAuthority = trainingIntakeAuthTokenAuthority;
        if (!string.IsNullOrWhiteSpace(expectedTrainingIntakeTenantId))
        {
            var tenant = DesignConversionModelActivationRegistry.TryBuildTenantIdentity(expectedTrainingIntakeTenantId);
            if (!tenant.IsSuccess)
                throw new ArgumentException("DesignConversion training intake expected tenant id is invalid.", nameof(expectedTrainingIntakeTenantId));

            _expectedTrainingIntakeTenantId = tenant.Value!.TenantId;
            _expectedTrainingIntakeTenantIdRef = tenant.Value.TenantIdRef;
        }

        _remotePromotionConfigured = remotePromotionConfigured;
        _enforceServerOwnedMeasurementGateThresholds =
            enforceServerOwnedMeasurementGateThresholds;
        _measurementGateThresholdConfiguration =
            measurementGateThresholdConfiguration;
        _repositoryOwnedTrainingOptions =
            SnapshotRepositoryOwnedTrainingOptions(
                repositoryOwnedTrainingOptions);
    }

    public async Task<DesignConversionModelServerHttpResponse> HandleAsync(
        DesignConversionModelServerHttpRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            return Error("POST", EndpointPath, 400, "REQUEST_REQUIRED", "DesignConversion training intake request is required.");

        var method = string.IsNullOrWhiteSpace(request.Method) ? string.Empty : request.Method.Trim().ToUpperInvariant();
        var path = NormalizePath(request.Path);
        if (!string.Equals(path, EndpointPath, StringComparison.Ordinal))
            return Error(method, path, 404, "ROUTE_NOT_FOUND", $"DesignConversion training intake route is not registered: {path}");

        if (!string.Equals(method, "POST", StringComparison.Ordinal))
            return Error(method, path, 405, "METHOD_NOT_ALLOWED", "DesignConversion training intake endpoint requires POST.");

        if (!IsJsonContentType(request.ContentType))
            return Error(method, path, 415, "UNSUPPORTED_MEDIA_TYPE", "DesignConversion training intake endpoint requires application/json.");

        var parsed = ParseBody(request.BodyJson);
        if (!parsed.IsSuccess)
        {
            return Error(
                method,
                path,
                400,
                parsed.ErrorCode ?? "TRAINING_INTAKE_REQUEST_INVALID",
                parsed.ErrorMessage ?? "DesignConversion training intake request is invalid.");
        }

        var routeRequest = parsed.Value!;
        var action = NormalizeAction(routeRequest.Action);
        if (string.IsNullOrWhiteSpace(action))
            return Error(method, path, 400, "TRAINING_INTAKE_ACTION_REQUIRED", "DesignConversion training intake action is required.");

        var admission = ValidateAdmissionBoundary(
            routeRequest,
            action,
            request.Headers,
            _expectedTrainingIntakeAuthToken,
            _trainingIntakeAuthTokenAuthority,
            _expectedTrainingIntakeTenantId,
            _expectedTrainingIntakeTenantIdRef,
            _remotePromotionConfigured);
        if (!admission.IsSuccess)
            return Error(method, path, admission.StatusCode, admission.ErrorCode!, admission.ErrorMessage!, admission.BoundaryMetadata);

        return action switch
        {
            ActionList => await HandleListAsync(method, path, action, admission.BoundaryMetadata, cancellationToken),
            ActionCreate => await HandleCreateAsync(method, path, routeRequest, admission.BoundaryMetadata, cancellationToken),
            ActionApproveRuntimeCandidate => await HandleApproveRuntimeCandidateAsync(method, path, routeRequest, admission.BoundaryMetadata, cancellationToken),
            ActionExportApprovedRuntimeCandidates => await HandleExportApprovedRuntimeCandidatesAsync(method, path, routeRequest, admission.BoundaryMetadata, cancellationToken),
            ActionPostApprovalPreflight => await HandlePostApprovalPreflightAsync(method, path, routeRequest, admission.BoundaryMetadata, cancellationToken),
            ActionPreflight => await HandlePreflightAsync(method, path, routeRequest, admission.BoundaryMetadata, cancellationToken),
            ActionRun => await HandleRunAsync(method, path, routeRequest, admission.BoundaryMetadata, cancellationToken),
            ActionPromoteLocal => await HandlePromoteLocalAsync(method, path, routeRequest, admission.BoundaryMetadata, cancellationToken),
            ActionContinueTrainingUploadStatus => await HandleContinueTrainingUploadStatusAsync(method, path, routeRequest, admission.BoundaryMetadata, cancellationToken),
            ActionRollbackLocalPromotion => await HandleRollbackLocalPromotionAsync(method, path, routeRequest, admission.BoundaryMetadata, cancellationToken),
            _ => Error(method, path, 400, "TRAINING_INTAKE_ACTION_UNSUPPORTED", $"DesignConversion training intake action is not supported: {SafeScalar(action, "training-intake-action")}")
        };
    }

    private async Task<DesignConversionModelServerHttpResponse> HandleListAsync(
        string method,
        string path,
        string action,
        IReadOnlyDictionary<string, string> admissionMetadata,
        CancellationToken cancellationToken)
    {
        var listed = await _api.ListTrainingIntakeRequestsAsync(cancellationToken);
        if (!listed.IsSuccess)
            return DomainError(method, path, action, listed.ErrorCode, listed.ErrorMessage);

        var intakes = listed.Value!.Select(ToRecordRef).ToArray();
        return Success(
            method,
            path,
            action,
            intakeId: null,
            intakes,
            intake: null,
            preflight: null,
            trainingRun: null,
            localPromotion: null,
            localPromotionRollback: null,
            lifecycleStatus: null,
            admissionMetadata,
            domainMetadata: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["training_intake_list_count"] = intakes.Length.ToString(CultureInfo.InvariantCulture)
            });
    }

    private async Task<DesignConversionModelServerHttpResponse> HandleCreateAsync(
        string method,
        string path,
        DesignConversionModelServerTrainingIntakeRequest request,
        IReadOnlyDictionary<string, string> admissionMetadata,
        CancellationToken cancellationToken)
    {
        if (request.Handoff is null)
            return Error(method, path, 400, "TRAINING_INTAKE_HANDOFF_REQUIRED", "DesignConversion training intake create action requires handoff.");
        if (string.IsNullOrWhiteSpace(request.RequestedByRef))
            return Error(method, path, 400, "TRAINING_INTAKE_REQUESTED_BY_REQUIRED", "DesignConversion training intake create action requires requestedByRef.");
        if (string.IsNullOrWhiteSpace(request.Reason))
            return Error(method, path, 400, "TRAINING_INTAKE_REASON_REQUIRED", "DesignConversion training intake create action requires reason.");

        var created = await _api.CreateTrainingIntakeAsync(
            request.Handoff,
            request.RequestedByRef,
            request.Reason,
            request.RequestedAtUtc,
            cancellationToken,
            admissionMetadata);
        if (!created.IsSuccess)
            return DomainError(method, path, ActionCreate, created.ErrorCode, created.ErrorMessage);

        var responseAdmissionMetadata = MergeMetadata(admissionMetadata, created.Value!.BoundaryMetadata);
        var intake = ToRecordRef(created.Value!);
        return Success(
            method,
            path,
            ActionCreate,
            intake.IntakeId,
            [intake],
            intake,
            preflight: null,
            trainingRun: null,
            localPromotion: null,
            localPromotionRollback: null,
            lifecycleStatus: null,
            responseAdmissionMetadata,
            domainMetadata: created.Value!.BoundaryMetadata);
    }

    private async Task<DesignConversionModelServerHttpResponse> HandleApproveRuntimeCandidateAsync(
        string method,
        string path,
        DesignConversionModelServerTrainingIntakeRequest request,
        IReadOnlyDictionary<string, string> admissionMetadata,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.LocalArtifactRecordId))
            return Error(method, path, 400, "TRAINING_INTAKE_LOCAL_ARTIFACT_RECORD_ID_REQUIRED", "DesignConversion runtime DPO candidate approval requires localArtifactRecordId.", admissionMetadata);
        if (string.IsNullOrWhiteSpace(request.CandidateOrTrainingRecordId))
            return Error(method, path, 400, "TRAINING_INTAKE_DPO_CANDIDATE_ID_REQUIRED", "DesignConversion runtime DPO candidate approval requires candidateOrTrainingRecordId.", admissionMetadata);
        if (string.IsNullOrWhiteSpace(request.RequestedByRef))
            return Error(method, path, 400, "TRAINING_INTAKE_APPROVAL_REVIEWER_REQUIRED", "DesignConversion runtime DPO candidate approval requires requestedByRef reviewer.", admissionMetadata);
        if (string.IsNullOrWhiteSpace(request.Reason))
            return Error(method, path, 400, "TRAINING_INTAKE_APPROVAL_REASON_REQUIRED", "DesignConversion runtime DPO candidate approval requires reason.", admissionMetadata);

        var approved = await _api.ApproveRuntimeCreatedDpoCandidateAsync(
            request.LocalArtifactRecordId.Trim(),
            request.CandidateOrTrainingRecordId.Trim(),
            request.RequestedByRef.Trim(),
            request.Reason.Trim(),
            request.RequestedAtUtc,
            cancellationToken);
        if (!approved.IsSuccess)
            return DomainError(method, path, ActionApproveRuntimeCandidate, approved.ErrorCode, approved.ErrorMessage, admissionMetadata);

        var candidateApproval = ToCandidateApprovalRef(approved.Value!);
        var responseAdmissionMetadata = MergeMetadata(admissionMetadata, candidateApproval.BoundaryMetadata);
        return Success(
            method,
            path,
            ActionApproveRuntimeCandidate,
            intakeId: null,
            [],
            intake: null,
            preflight: null,
            trainingRun: null,
            localPromotion: null,
            localPromotionRollback: null,
            lifecycleStatus: null,
            responseAdmissionMetadata,
            domainMetadata: candidateApproval.BoundaryMetadata,
            candidateApproval: candidateApproval,
            approvedHandoff: null);
    }

    private async Task<DesignConversionModelServerHttpResponse> HandleExportApprovedRuntimeCandidatesAsync(
        string method,
        string path,
        DesignConversionModelServerTrainingIntakeRequest request,
        IReadOnlyDictionary<string, string> admissionMetadata,
        CancellationToken cancellationToken)
    {
        var exported = await _api.ExportApprovedRuntimeDpoCandidatesAsync(
            request.Query,
            request.RequestedAtUtc,
            cancellationToken);
        if (!exported.IsSuccess)
            return DomainError(method, path, ActionExportApprovedRuntimeCandidates, exported.ErrorCode, exported.ErrorMessage, admissionMetadata);

        var approvedHandoff = ToPublicApprovedHandoff(exported.Value!);
        var responseAdmissionMetadata = MergeMetadata(admissionMetadata, approvedHandoff.BoundaryMetadata);
        return Success(
            method,
            path,
            ActionExportApprovedRuntimeCandidates,
            intakeId: null,
            [],
            intake: null,
            preflight: null,
            trainingRun: null,
            localPromotion: null,
            localPromotionRollback: null,
            lifecycleStatus: null,
            responseAdmissionMetadata,
            domainMetadata: approvedHandoff.BoundaryMetadata,
            candidateApproval: null,
            approvedHandoff: approvedHandoff);
    }

    private async Task<DesignConversionModelServerHttpResponse> HandlePostApprovalPreflightAsync(
        string method,
        string path,
        DesignConversionModelServerTrainingIntakeRequest request,
        IReadOnlyDictionary<string, string> admissionMetadata,
        CancellationToken cancellationToken)
    {
        var tenantId = NormalizeRequiredExactScalar(
            request.TenantId,
            "TRAINING_INTAKE_TENANT_ID_REQUIRED",
            "DesignConversion approved runtime DPO preflight requires canonical tenantId.");
        if (!tenantId.IsSuccess)
            return Error(method, path, 400, tenantId.ErrorCode!, tenantId.ErrorMessage!, admissionMetadata);

        var idempotencyKey = NormalizeRequiredExactScalar(
            request.IdempotencyKey,
            "TRAINING_INTAKE_IDEMPOTENCY_KEY_REQUIRED",
            "DesignConversion approved runtime DPO preflight requires canonical idempotencyKey.");
        if (!idempotencyKey.IsSuccess)
            return Error(method, path, 400, idempotencyKey.ErrorCode!, idempotencyKey.ErrorMessage!, admissionMetadata);

        var localArtifactRecordId = NormalizeRequiredExactScalar(
            request.LocalArtifactRecordId,
            "TRAINING_INTAKE_LOCAL_ARTIFACT_RECORD_ID_REQUIRED",
            "DesignConversion approved runtime DPO preflight requires canonical localArtifactRecordId.");
        if (!localArtifactRecordId.IsSuccess)
            return Error(method, path, 400, localArtifactRecordId.ErrorCode!, localArtifactRecordId.ErrorMessage!, admissionMetadata);

        var candidateOrTrainingRecordId = NormalizeRequiredExactScalar(
            request.CandidateOrTrainingRecordId,
            "TRAINING_INTAKE_DPO_CANDIDATE_ID_REQUIRED",
            "DesignConversion approved runtime DPO preflight requires canonical candidateOrTrainingRecordId.");
        if (!candidateOrTrainingRecordId.IsSuccess)
            return Error(method, path, 400, candidateOrTrainingRecordId.ErrorCode!, candidateOrTrainingRecordId.ErrorMessage!, admissionMetadata);

        var requestedByRef = NormalizeRequiredExactScalar(
            request.RequestedByRef,
            "TRAINING_INTAKE_APPROVED_RUNTIME_DPO_PREFLIGHT_REQUESTER_REQUIRED",
            "DesignConversion approved runtime DPO preflight requires canonical requestedByRef.");
        if (!requestedByRef.IsSuccess)
            return Error(method, path, 400, requestedByRef.ErrorCode!, requestedByRef.ErrorMessage!, admissionMetadata);

        var reason = NormalizeRequiredExactScalar(
            request.Reason,
            "TRAINING_INTAKE_APPROVED_RUNTIME_DPO_PREFLIGHT_REASON_REQUIRED",
            "DesignConversion approved runtime DPO preflight requires canonical reason.");
        if (!reason.IsSuccess)
            return Error(method, path, 400, reason.ErrorCode!, reason.ErrorMessage!, admissionMetadata);

        var preflight = await _api.RunApprovedRuntimeDpoCandidateTrainingIntakePreflightAsync(
            localArtifactRecordId.Value!,
            candidateOrTrainingRecordId.Value!,
            requestedByRef.Value!,
            reason.Value!,
            request.RequestedAtUtc,
            cancellationToken);
        if (!preflight.IsSuccess)
            return DomainError(method, path, ActionPostApprovalPreflight, preflight.ErrorCode, preflight.ErrorMessage, admissionMetadata);

        var preflightRef = ToApprovedRuntimeDpoPreflightRef(preflight.Value!);
        var responseAdmissionMetadata = MergeMetadata(admissionMetadata, preflightRef.BoundaryMetadata);
        return Success(
            method,
            path,
            ActionPostApprovalPreflight,
            preflightRef.IntakeId,
            [],
            intake: null,
            preflight: null,
            trainingRun: null,
            localPromotion: null,
            localPromotionRollback: null,
            lifecycleStatus: null,
            responseAdmissionMetadata,
            domainMetadata: preflightRef.BoundaryMetadata,
            candidateApproval: null,
            approvedHandoff: null,
            approvedRuntimeDpoPreflight: preflightRef);
    }

    private async Task<DesignConversionModelServerHttpResponse> HandlePreflightAsync(
        string method,
        string path,
        DesignConversionModelServerTrainingIntakeRequest request,
        IReadOnlyDictionary<string, string> admissionMetadata,
        CancellationToken cancellationToken)
    {
        var intakeId = NormalizeRequiredIntakeId(request.IntakeId);
        if (!intakeId.IsSuccess)
            return Error(method, path, 400, intakeId.ErrorCode ?? "TRAINING_INTAKE_ID_REQUIRED", intakeId.ErrorMessage ?? "DesignConversion training intake id is required.");

        var preflight = await _api.RunTrainingIntakePreflightAsync(intakeId.Value!, cancellationToken);
        if (!preflight.IsSuccess)
            return DomainError(method, path, ActionPreflight, preflight.ErrorCode, preflight.ErrorMessage);

        var preflightRef = ToPreflightRef(preflight.Value!);
        return Success(
            method,
            path,
            ActionPreflight,
            preflightRef.TrainingIntakeId,
            [],
            intake: null,
            preflightRef,
            trainingRun: null,
            localPromotion: null,
            localPromotionRollback: null,
            lifecycleStatus: null,
            admissionMetadata,
            domainMetadata: preflight.Value!.BoundaryMetadata);
    }

    private async Task<DesignConversionModelServerHttpResponse> HandleRunAsync(
        string method,
        string path,
        DesignConversionModelServerTrainingIntakeRequest request,
        IReadOnlyDictionary<string, string> admissionMetadata,
        CancellationToken cancellationToken)
    {
        var intakeId = NormalizeRequiredIntakeId(request.IntakeId);
        if (!intakeId.IsSuccess)
            return Error(method, path, 400, intakeId.ErrorCode ?? "TRAINING_INTAKE_ID_REQUIRED", intakeId.ErrorMessage ?? "DesignConversion training intake id is required.");
        var trainingOptionsGate = ApplyProductionTrainingOptionPolicies(
            method,
            path,
            request.Options,
            out var effectiveOptions);
        if (trainingOptionsGate is not null)
            return trainingOptionsGate;

        var run = await _api.RunTrainingIntakeAsync(
            intakeId.Value!,
            effectiveOptions,
            cancellationToken,
            admissionMetadata);
        if (!run.IsSuccess)
            return DomainError(method, path, ActionRun, run.ErrorCode, run.ErrorMessage);

        var trainingRun = run.Value!;
        var runRef = ToRunRef(trainingRun);
        var responseAdmissionMetadata = MergeMetadata(admissionMetadata, trainingRun.BoundaryMetadata);
        return Success(
            method,
            path,
            ActionRun,
            runRef.TrainingIntakeId,
            [],
            intake: null,
            preflight: null,
            runRef,
            localPromotion: null,
            localPromotionRollback: null,
            lifecycleStatus: null,
            responseAdmissionMetadata,
            domainMetadata: run.Value!.BoundaryMetadata);
    }

    private async Task<DesignConversionModelServerHttpResponse> HandlePromoteLocalAsync(
        string method,
        string path,
        DesignConversionModelServerTrainingIntakeRequest request,
        IReadOnlyDictionary<string, string> admissionMetadata,
        CancellationToken cancellationToken)
    {
        var intakeId = NormalizeRequiredIntakeId(request.IntakeId);
        if (!intakeId.IsSuccess)
            return Error(method, path, 400, intakeId.ErrorCode ?? "TRAINING_INTAKE_ID_REQUIRED", intakeId.ErrorMessage ?? "DesignConversion training intake id is required.");
        if (string.IsNullOrWhiteSpace(request.PromotionOutputDirectory))
            return Error(method, path, 400, "TRAINING_INTAKE_PROMOTION_OUTPUT_DIRECTORY_REQUIRED", "DesignConversion training intake local promotion requires promotionOutputDirectory.");
        var trainingOptionsGate = ApplyProductionTrainingOptionPolicies(
            method,
            path,
            request.Options,
            out var effectiveOptions);
        if (trainingOptionsGate is not null)
            return trainingOptionsGate;

        var promoted = await DesignConversionTrainingIntakeRouteAdmissionContext.RunAsync(
            admissionMetadata,
            () => _api.RunTrainingIntakeAndPromoteLocalAsync(
                intakeId.Value!,
                request.PromotionOutputDirectory,
                effectiveOptions,
                cancellationToken));
        if (!promoted.IsSuccess)
            return DomainError(method, path, ActionPromoteLocal, promoted.ErrorCode, promoted.ErrorMessage, admissionMetadata);

        var promotionRef = ToLocalPromotionRef(promoted.Value!);
        var responseAdmissionMetadata = MergeMetadata(admissionMetadata, promoted.Value!.BoundaryMetadata);
        return Success(
            method,
            path,
            ActionPromoteLocal,
            promotionRef.TrainingRun.TrainingIntakeId,
            [],
            intake: null,
            preflight: null,
            trainingRun: promotionRef.TrainingRun,
            promotionRef,
            localPromotionRollback: null,
            lifecycleStatus: null,
            responseAdmissionMetadata,
            domainMetadata: promoted.Value!.BoundaryMetadata);
    }

    private async Task<DesignConversionModelServerHttpResponse> HandleContinueTrainingUploadStatusAsync(
        string method,
        string path,
        DesignConversionModelServerTrainingIntakeRequest request,
        IReadOnlyDictionary<string, string> admissionMetadata,
        CancellationToken cancellationToken)
    {
        var intakeId = NormalizeRequiredIntakeId(request.IntakeId);
        if (!intakeId.IsSuccess)
            return Error(method, path, 400, intakeId.ErrorCode ?? "TRAINING_INTAKE_ID_REQUIRED", intakeId.ErrorMessage ?? "DesignConversion training intake id is required.");
        if (string.IsNullOrWhiteSpace(request.RequestedByRef))
            return Error(method, path, 400, "TRAINING_INTAKE_LIFECYCLE_REQUESTED_BY_REQUIRED", "DesignConversion lifecycle command requires requestedByRef.");
        if (string.IsNullOrWhiteSpace(request.Reason))
            return Error(method, path, 400, "TRAINING_INTAKE_LIFECYCLE_REASON_REQUIRED", "DesignConversion lifecycle command requires reason.");
        if (string.IsNullOrWhiteSpace(request.PromotionOutputDirectory))
            return Error(method, path, 400, "TRAINING_INTAKE_PROMOTION_OUTPUT_DIRECTORY_REQUIRED", "DesignConversion lifecycle command requires promotionOutputDirectory for local promotion status.");
        var trainingOptionsGate = ApplyProductionTrainingOptionPolicies(
            method,
            path,
            request.Options,
            out var effectiveOptions);
        if (trainingOptionsGate is not null)
            return trainingOptionsGate;

        var promoted = await DesignConversionTrainingIntakeRouteAdmissionContext.RunAsync(
            admissionMetadata,
            () => _api.RunTrainingIntakeAndPromoteLocalAsync(
                intakeId.Value!,
                request.PromotionOutputDirectory,
                effectiveOptions,
                cancellationToken));
        if (!promoted.IsSuccess)
            return DomainError(method, path, ActionContinueTrainingUploadStatus, promoted.ErrorCode, promoted.ErrorMessage, admissionMetadata);

        var promotionRef = ToLocalPromotionRef(promoted.Value!);
        var lifecycleStatus = ToLifecycleStatusRef(promoted.Value!, promotionRef, request);
        var responseAdmissionMetadata = MergeMetadata(admissionMetadata, lifecycleStatus.BoundaryMetadata);
        return Success(
            method,
            path,
            ActionContinueTrainingUploadStatus,
            promotionRef.TrainingRun.TrainingIntakeId,
            [],
            intake: null,
            preflight: null,
            trainingRun: promotionRef.TrainingRun,
            promotionRef,
            localPromotionRollback: null,
            lifecycleStatus,
            responseAdmissionMetadata,
            domainMetadata: lifecycleStatus.BoundaryMetadata);
    }

    private async Task<DesignConversionModelServerHttpResponse> HandleRollbackLocalPromotionAsync(
        string method,
        string path,
        DesignConversionModelServerTrainingIntakeRequest request,
        IReadOnlyDictionary<string, string> admissionMetadata,
        CancellationToken cancellationToken)
    {
        var promotedLocalModelRef = NormalizeRequiredPromotedLocalModelRef(request);
        if (!promotedLocalModelRef.IsSuccess)
            return Error(method, path, 400, promotedLocalModelRef.ErrorCode ?? "TRAINING_INTAKE_LOCAL_PROMOTION_ROLLBACK_REF_REQUIRED", promotedLocalModelRef.ErrorMessage ?? "DesignConversion local promotion rollback requires promotedLocalModelRef.");
        if (string.IsNullOrWhiteSpace(request.RequestedByRef))
            return Error(method, path, 400, "TRAINING_INTAKE_LOCAL_PROMOTION_ROLLBACK_REQUESTED_BY_REQUIRED", "DesignConversion local promotion rollback requires requestedByRef.");
        if (string.IsNullOrWhiteSpace(request.Reason))
            return Error(method, path, 400, "TRAINING_INTAKE_LOCAL_PROMOTION_ROLLBACK_REASON_REQUIRED", "DesignConversion local promotion rollback requires reason.");

        var rollback = await DesignConversionTrainingIntakeRouteAdmissionContext.RunAsync(
            admissionMetadata,
            () => _api.RollbackTrainingIntakeLocalPromotionAsync(
                promotedLocalModelRef.Value!,
                request.RequestedByRef,
                request.Reason,
                request.RequestedAtUtc,
                cancellationToken));
        if (!rollback.IsSuccess)
            return DomainError(method, path, ActionRollbackLocalPromotion, rollback.ErrorCode, rollback.ErrorMessage, admissionMetadata);

        var rollbackRef = ToRollbackRef(rollback.Value!);
        var responseAdmissionMetadata = MergeMetadata(admissionMetadata, rollback.Value!.BoundaryMetadata);
        return Success(
            method,
            path,
            ActionRollbackLocalPromotion,
            intakeId: null,
            [],
            intake: null,
            preflight: null,
            trainingRun: null,
            localPromotion: null,
            localPromotionRollback: rollbackRef,
            lifecycleStatus: null,
            responseAdmissionMetadata,
            domainMetadata: rollback.Value!.BoundaryMetadata);
    }

    private DesignConversionModelServerHttpResponse? ApplyProductionTrainingOptionPolicies(
        string method,
        string path,
        DesignConversionTrainingPipelineOptions? options,
        out DesignConversionTrainingPipelineOptions? effectiveOptions)
    {
        var repositoryOwnedGate = ApplyServerOwnedRepositoryTrainingContexts(
            method,
            path,
            options,
            out var repositoryOwnedOptions);
        if (repositoryOwnedGate is not null)
        {
            effectiveOptions = repositoryOwnedOptions;
            return repositoryOwnedGate;
        }

        var thresholdGate = ApplyProductionMeasurementGateThresholds(
            method,
            path,
            repositoryOwnedOptions,
            out var thresholdedOptions);
        if (thresholdGate is not null)
        {
            effectiveOptions = thresholdedOptions;
            return thresholdGate;
        }

        effectiveOptions = thresholdedOptions;
        return null;
    }

    private DesignConversionModelServerHttpResponse? ApplyProductionMeasurementGateThresholds(
        string method,
        string path,
        DesignConversionTrainingPipelineOptions? options,
        out DesignConversionTrainingPipelineOptions? effectiveOptions)
    {
        effectiveOptions = options;
        if (!_enforceServerOwnedMeasurementGateThresholds)
            return null;

        if (_measurementGateThresholdConfiguration is null)
        {
            return Error(
                method,
                path,
                503,
                "TRAINING_INTAKE_MEASUREMENT_GATE_THRESHOLDS_NOT_CONFIGURED",
                "DesignConversion training intake measurement gate thresholds are not configured.");
        }

        var validatedConfiguration =
            _measurementGateThresholdConfiguration.ValidateForDomainBounds();
        if (!validatedConfiguration.IsSuccess)
        {
            return Error(
                method,
                path,
                503,
                MeasurementGateThresholdsInvalidCode,
                "DesignConversion training intake measurement gate thresholds are invalid.");
        }

        if (options?.MeasurementGateRequest is null)
        {
            return Error(
                method,
                path,
                400,
                "TRAINING_INTAKE_MEASUREMENT_GATE_REQUEST_REQUIRED",
                "DesignConversion training intake requires a caller-supplied measurement gate request.");
        }

        effectiveOptions = ApplyServerOwnedMeasurementGateThresholds(
            options,
            validatedConfiguration.Value!);
        return null;
    }

    private DesignConversionModelServerHttpResponse? ApplyServerOwnedRepositoryTrainingContexts(
        string method,
        string path,
        DesignConversionTrainingPipelineOptions? options,
        out DesignConversionTrainingPipelineOptions? effectiveOptions)
    {
        effectiveOptions = options;
        if (_repositoryOwnedTrainingOptions is null)
        {
            if (!HasCallerSuppliedRepositoryOwnedTrainingContext(options))
                return null;

            return Error(
                method,
                path,
                400,
                RepositoryOwnedContextRequiresServerConfigurationCode,
                "DesignConversion training intake repository-owned contexts must come from server configuration, not from the caller request.");
        }

        if (_repositoryOwnedTrainingOptions.RunContextSpec is null)
        {
            return Error(
                method,
                path,
                503,
                _repositoryOwnedTrainingOptions.ConfigurationErrorCode ??
                    DesignConversionModelServerRepositoryOwnedTrainingOptions
                        .ServerConfigurationInvalidErrorCode,
                _repositoryOwnedTrainingOptions.ConfigurationErrorMessage ??
                    "DesignConversion repository-owned training server configuration is invalid.");
        }

        if (options is null)
        {
            return Error(
                method,
                path,
                400,
                RepositoryOwnedContextOptionsRequiredCode,
                "DesignConversion training intake options are required when server-owned repository training context is configured.");
        }

        var applied = ApplyServerOwnedRepositoryTrainingContexts(
            options,
            _repositoryOwnedTrainingOptions);
        if (!applied.IsSuccess || applied.Value is null)
        {
            return Error(
                method,
                path,
                409,
                applied.ErrorCode ??
                    DesignConversionRepositoryOwnedTrainingRunContextFactory
                        .UnavailableErrorCode,
                applied.ErrorMessage ??
                    "DesignConversion repository-owned training context could not be created.");
        }

        effectiveOptions = applied.Value;
        return null;
    }

    internal static DesignConversionTrainingPipelineOptions ApplyServerOwnedMeasurementGateThresholds(
        DesignConversionTrainingPipelineOptions options,
        DesignConversionModelServerMeasurementGateThresholdConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(options.MeasurementGateRequest);
        var validatedConfiguration = configuration.ValidateForDomainBounds();
        if (!validatedConfiguration.IsSuccess)
            throw new ArgumentException(
                "DesignConversion training intake measurement gate thresholds are invalid.",
                nameof(configuration));

        return options with
        {
            MeasurementGateRequest = options.MeasurementGateRequest with
            {
                Thresholds = validatedConfiguration.Value!.ToDomainThresholds()
            }
        };
    }

    internal static OperationResult<DesignConversionTrainingPipelineOptions>
        ApplyServerOwnedRepositoryTrainingContexts(
            DesignConversionTrainingPipelineOptions options,
            DesignConversionModelServerRepositoryOwnedTrainingOptions
                repositoryOwnedTrainingOptions)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(repositoryOwnedTrainingOptions);

        if (repositoryOwnedTrainingOptions.RunContextSpec is null)
        {
            return OperationResult<DesignConversionTrainingPipelineOptions>.Fail(
                repositoryOwnedTrainingOptions.ConfigurationErrorMessage ??
                "DesignConversion repository-owned training server configuration is invalid.",
                repositoryOwnedTrainingOptions.ConfigurationErrorCode ??
                DesignConversionModelServerRepositoryOwnedTrainingOptions
                    .ServerConfigurationInvalidErrorCode);
        }

        var contexts = DesignConversionRepositoryOwnedTrainingRunContextFactory
            .Create(repositoryOwnedTrainingOptions.RunContextSpec);
        if (!contexts.IsSuccess || contexts.Value is null)
            return contexts.MapError<DesignConversionTrainingPipelineOptions>();

        // The request may carry stale or hostile repository contexts because the
        // public JSON shape includes training options. Production wiring ignores
        // those caller-owned values and replaces both halves with the server's
        // freshly opened sealed-DPO/model-checkpoint contexts.
        return OperationResult<DesignConversionTrainingPipelineOptions>.Ok(
            options with
            {
                RepositoryOwnedDpoTransactionContext =
                    contexts.Value.DpoTransactionContext,
                RepositoryOwnedModelCheckpointContext =
                    contexts.Value.ModelCheckpointContext
            });
    }

    internal static bool HasCallerSuppliedRepositoryOwnedTrainingContext(
        DesignConversionTrainingPipelineOptions? options)
        => options?.RepositoryOwnedDpoTransactionContext is not null ||
           options?.RepositoryOwnedModelCheckpointContext is not null;

    private static DesignConversionModelServerRepositoryOwnedTrainingOptions?
        SnapshotRepositoryOwnedTrainingOptions(
            DesignConversionModelServerRepositoryOwnedTrainingOptions?
                options)
    {
        if (options is null)
            return null;

        if (options.RunContextSpec is null)
            return new DesignConversionModelServerRepositoryOwnedTrainingOptions(
                RunContextSpec: null,
                ConfigurationErrorCode: options.ConfigurationErrorCode,
                ConfigurationErrorMessage: options.ConfigurationErrorMessage);

        return new DesignConversionModelServerRepositoryOwnedTrainingOptions(
            options.RunContextSpec with
            {
                ProvenanceRefs =
                    options.RunContextSpec.ProvenanceRefs?.ToArray() ?? null!
            },
            options.ConfigurationErrorCode,
            options.ConfigurationErrorMessage);
    }

    private DesignConversionModelServerHttpResponse Success(
        string method,
        string path,
        string action,
        string? intakeId,
        IReadOnlyList<DesignConversionModelServerTrainingIntakeRecordRef> intakes,
        DesignConversionModelServerTrainingIntakeRecordRef? intake,
        DesignConversionModelServerTrainingIntakePreflightRef? preflight,
        DesignConversionModelServerTrainingIntakeRunRef? trainingRun,
        DesignConversionModelServerTrainingIntakeLocalPromotionRef? localPromotion,
        DesignConversionModelServerTrainingIntakeLocalPromotionRollbackRef? localPromotionRollback,
        DesignConversionModelServerTrainingIntakeLifecycleStatusRef? lifecycleStatus,
        IReadOnlyDictionary<string, string> admissionMetadata,
        IReadOnlyDictionary<string, string>? domainMetadata,
        DesignConversionModelServerDpoCandidateApprovalRef? candidateApproval = null,
        DesignConversionApprovedDpoHandoffArtifact? approvedHandoff = null,
        DesignConversionModelServerApprovedRuntimeDpoPreflightRef? approvedRuntimeDpoPreflight = null)
    {
        var metadata = BuildBoundaryMetadata(
            method,
            path,
            200,
            runtimeInvoked: true,
            action,
            _remotePromotionConfigured,
            MergeMetadata(domainMetadata, admissionMetadata));
        var warnings = BuildWarnings(action, _remotePromotionConfigured)
            .Concat(intake?.Warnings ?? [])
            .Concat(preflight?.Warnings ?? [])
            .Concat(approvedRuntimeDpoPreflight?.Warnings ?? [])
            .Concat(trainingRun?.Warnings ?? [])
            .Concat(localPromotion?.Warnings ?? [])
            .Concat(localPromotionRollback?.Warnings ?? [])
            .Concat(lifecycleStatus?.Warnings ?? [])
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var body = new DesignConversionModelServerTrainingIntakeResponse(
            Endpoint: EndpointPath,
            Method: method,
            HttpSeam: DesignConversionModelServerHttpHandler.LocalHttpSeam,
            ApiShape: ApiShape,
            RuntimeSurface: RuntimeSurface,
            StatusCode: 200,
            ContractStatus: "training-intake-action-completed",
            Action: action,
            IntakeId: intakeId,
            IntakeCount: intakes.Count,
            Intakes: intakes,
            Intake: intake,
            Preflight: preflight,
            TrainingRun: trainingRun,
            LocalPromotion: localPromotion,
            LocalPromotionRollback: localPromotionRollback,
            LifecycleStatus: lifecycleStatus,
            ApprovedRuntimeDpoPreflight: approvedRuntimeDpoPreflight,
            CandidateApproval: candidateApproval,
            ApprovedHandoff: approvedHandoff,
            ProductReady: false,
            DeploymentVerified: false,
            PublicHttpEndpointVerified: false,
            Warnings: warnings,
            BoundaryMetadata: metadata);

        return Json(200, method, path, body);
    }

    private static OperationResult<DesignConversionModelServerTrainingIntakeRequest> ParseBody(string? bodyJson)
    {
        if (string.IsNullOrWhiteSpace(bodyJson))
            return OperationResult<DesignConversionModelServerTrainingIntakeRequest>.Fail(
                "DesignConversion training intake request body is required.",
                "REQUEST_BODY_REQUIRED");

        try
        {
            var request = JsonSerializer.Deserialize<DesignConversionModelServerTrainingIntakeRequest>(
                bodyJson,
                DesignConversionModelServerHttpHandler.JsonOptions);
            return request is null
                ? OperationResult<DesignConversionModelServerTrainingIntakeRequest>.Fail(
                    "DesignConversion training intake request body deserialized to null.",
                    "TRAINING_INTAKE_REQUEST_REQUIRED")
                : OperationResult<DesignConversionModelServerTrainingIntakeRequest>.Ok(request);
        }
        catch (JsonException ex)
        {
            return OperationResult<DesignConversionModelServerTrainingIntakeRequest>.Fail(
                $"DesignConversion training intake request JSON is invalid: {ex.GetType().Name}",
                "REQUEST_JSON_INVALID");
        }
        catch (NotSupportedException ex)
        {
            // WHY: repository-owned contexts are server-owned route policy, not
            // client DTOs. A caller that serializes internal address objects must
            // receive a controlled fail-closed response instead of an unhandled
            // JSON-shape exception before training-intake admission can answer.
            return OperationResult<DesignConversionModelServerTrainingIntakeRequest>.Fail(
                $"DesignConversion training intake request JSON shape is not supported: {ex.GetType().Name}",
                "TRAINING_INTAKE_REQUEST_UNSUPPORTED_JSON_SHAPE");
        }
    }

    private static OperationResult<string> NormalizeRequiredIntakeId(string? intakeId)
    {
        if (string.IsNullOrWhiteSpace(intakeId))
            return OperationResult<string>.Fail(
                "DesignConversion training intake id is required for this action.",
                "TRAINING_INTAKE_ID_REQUIRED");

        return OperationResult<string>.Ok(intakeId.Trim());
    }

    private static OperationResult<string> NormalizeRequiredExactScalar(
        string? value,
        string errorCode,
        string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            !string.Equals(value, value.Trim(), StringComparison.Ordinal))
        {
            return OperationResult<string>.Fail(errorMessage, errorCode);
        }

        return OperationResult<string>.Ok(value);
    }

    private static string NormalizeAction(string? action)
    {
        if (string.IsNullOrWhiteSpace(action))
            return string.Empty;

        return action.Trim().ToLowerInvariant() switch
        {
            "list" or "list-intakes" or "list-training-intakes" => ActionList,
            "create" or "create-intake" or "create-training-intake" => ActionCreate,
            "preflight" or "run-preflight" or "run-training-intake-preflight" => ActionPreflight,
            "run" or "train" or "run-training-intake" => ActionRun,
            "promote" or "promote-local" or "run-and-promote-local" or "run-training-intake-and-promote-local" => ActionPromoteLocal,
            "continue-training" or "continue-train" or "continue-training-upload-status" or
                "continue-training-upload-deploy-status" or "upload-deploy-status" or
                "continue-training-and-upload-status" => ActionContinueTrainingUploadStatus,
            "rollback" or "rollback-local-promotion" or "rollback-training-intake-local-promotion" or
                "rollback-promoted-local-model" => ActionRollbackLocalPromotion,
            "approve-runtime-candidate" or "approve-runtime-dpo-candidate" or
                "approve-runtime-created-dpo-candidate" => ActionApproveRuntimeCandidate,
            "export-approved-runtime-candidates" or "export-approved-runtime-dpo-candidates" or
                "export-approved-runtime-created-dpo-candidates" => ActionExportApprovedRuntimeCandidates,
            "post-approval-preflight" or "approved-runtime-dpo-preflight" or
                "run-approved-runtime-dpo-candidate-training-intake-preflight" =>
                ActionPostApprovalPreflight,
            var normalized => normalized
        };
    }

    private static OperationResult<string> NormalizeRequiredPromotedLocalModelRef(
        DesignConversionModelServerTrainingIntakeRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.PromotedLocalModelRef))
            return OperationResult<string>.Ok(request.PromotedLocalModelRef.Trim());

        foreach (var key in new[]
        {
            "promotedLocalModelRef",
            "targetPromotedLocalModelRef",
            "localPromotionActiveLocalModelRef",
            "local_promotion_active_local_model_ref",
            "promotedLocalModelId",
            "localPromotedModelId",
            "local_promoted_model_id",
            "promotionUploadRef",
            "promotion_upload_ref",
            "localPromotedCheckpointSha256",
            "local_promoted_checkpoint_sha256"
        })
        {
            if (request.Metadata is not null &&
                request.Metadata.TryGetValue(key, out var metadataRef) &&
                !string.IsNullOrWhiteSpace(metadataRef))
            {
                return OperationResult<string>.Ok(metadataRef.Trim());
            }
        }

        return OperationResult<string>.Fail(
            "DesignConversion local promotion rollback requires promotedLocalModelRef or matching rollback metadata ref.",
            "TRAINING_INTAKE_LOCAL_PROMOTION_ROLLBACK_REF_REQUIRED");
    }

    private static AdmissionBoundaryValidation ValidateAdmissionBoundary(
        DesignConversionModelServerTrainingIntakeRequest request,
        string action,
        IReadOnlyDictionary<string, string>? headers,
        string? expectedTrainingIntakeAuthToken,
        DesignConversionModelServerModelLifecycleTokenKeyRing?
            trainingIntakeAuthTokenAuthority,
        string? expectedTrainingIntakeTenantId,
        string? expectedTrainingIntakeTenantIdRef,
        bool remotePromotionConfigured)
    {
        var mutating = IsMutatingAdmissionAction(action);
        var configuredAuthRequired = RequiresConfiguredLocalAuthToken(action);
        var legacyAuthConfigured = !string.IsNullOrWhiteSpace(expectedTrainingIntakeAuthToken);
        var lifecycleAuthConfigured = trainingIntakeAuthTokenAuthority is not null;
        var authConfigured = legacyAuthConfigured || lifecycleAuthConfigured;
        var authRequired = mutating && (legacyAuthConfigured || configuredAuthRequired);
        var hostTenantConfigured = !string.IsNullOrWhiteSpace(expectedTrainingIntakeTenantId);
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["training_intake_admission_boundary_version"] = "tenant_id_idempotency_key_optional_local_header_token_v2",
            ["training_intake_action"] = action,
            ["training_intake_mutating_admission_boundary_required"] = mutating.ToString(),
            ["training_intake_tenant_id_required"] = mutating.ToString(),
            ["training_intake_idempotency_key_required"] = mutating.ToString(),
            ["training_intake_admission_boundary_satisfied"] = (!mutating).ToString(),
            ["training_intake_auth_token_configured"] = authConfigured.ToString(),
            ["training_intake_auth_token_configuration_required"] = configuredAuthRequired.ToString(),
            ["training_intake_auth_token_required"] = authRequired.ToString(),
            ["training_intake_auth_token_enforced"] = authRequired.ToString(),
            ["training_intake_auth_token_header_name"] = TrainingIntakeAuthTokenHeaderName,
            ["training_intake_auth_token_verified"] = bool.FalseString,
            ["training_intake_auth_authority"] = lifecycleAuthConfigured
                ? "model_lifecycle_token_key_ring"
                : legacyAuthConfigured
                    ? "legacy_training_intake_token"
                    : "not_configured",
            ["training_intake_auth_admission_scope"] = lifecycleAuthConfigured
                ? "model_lifecycle_token_key_ring_required_for_runtime_candidate_approval_export"
                : legacyAuthConfigured
                    ? "legacy_local_header_token_required_for_mutating_training_intake_actions"
                    : configuredAuthRequired
                        ? "local_configured_header_token_required_for_runtime_candidate_approval_export"
                        : "auth_token_not_configured_local_behavior_preserved",
            ["training_intake_remote_promotion_configured"] = remotePromotionConfigured.ToString(),
            ["training_intake_route_local_only"] = (!remotePromotionConfigured).ToString(),
            ["training_intake_host_tenant_configured"] = hostTenantConfigured.ToString(),
            ["training_intake_host_tenant_verified"] = bool.FalseString,
            ["training_intake_host_tenant_ref"] = expectedTrainingIntakeTenantIdRef ?? string.Empty,
            ["training_intake_replay_cache_checked"] = bool.FalseString,
            ["training_intake_full_tenant_isolation_verified"] = bool.FalseString
        };

        if (!mutating)
            return AdmissionBoundaryValidation.Success(metadata);

        if (configuredAuthRequired && !authConfigured)
        {
            return AdmissionBoundaryValidation.Fail(
                "TRAINING_INTAKE_AUTH_TOKEN_NOT_CONFIGURED",
                $"DesignConversion training intake {action} action requires configured local auth token before runtime DPO candidate mutation.",
                metadata,
                statusCode: 503);
        }

        if (authRequired)
        {
            var suppliedToken = ReadHeader(headers, TrainingIntakeAuthTokenHeaderName);
            if (string.IsNullOrWhiteSpace(suppliedToken))
            {
                return AdmissionBoundaryValidation.Fail(
                    "TRAINING_INTAKE_AUTH_TOKEN_REQUIRED",
                    $"DesignConversion training intake {action} action requires local auth token header {TrainingIntakeAuthTokenHeaderName}.",
                    metadata,
                    statusCode: 401);
            }

            var tokenAccepted = lifecycleAuthConfigured
                ? trainingIntakeAuthTokenAuthority!.IsAccepted(suppliedToken)
                : ConstantTimeEquals(suppliedToken.Trim(), expectedTrainingIntakeAuthToken!);
            if (!tokenAccepted)
            {
                return AdmissionBoundaryValidation.Fail(
                    "TRAINING_INTAKE_AUTH_TOKEN_INVALID",
                    $"DesignConversion training intake {action} action rejected by local auth token admission boundary.",
                    metadata,
                    statusCode: 401);
            }

            metadata["training_intake_auth_token_verified"] = bool.TrueString;
        }

        if (string.IsNullOrWhiteSpace(request.TenantId))
            return AdmissionBoundaryValidation.Fail(
                "TRAINING_INTAKE_TENANT_ID_REQUIRED",
                $"DesignConversion training intake {action} action requires tenantId.",
                metadata);

        if (hostTenantConfigured)
        {
            var suppliedTenant = DesignConversionModelActivationRegistry.TryBuildTenantIdentity(request.TenantId);
            if (!suppliedTenant.IsSuccess)
            {
                return AdmissionBoundaryValidation.Fail(
                    "TRAINING_INTAKE_TENANT_ID_INVALID",
                    $"DesignConversion training intake {action} action received invalid tenantId.",
                    metadata,
                    statusCode: 403);
            }

            metadata["training_intake_tenant_id_ref"] = suppliedTenant.Value!.TenantIdRef;
            if (!string.Equals(suppliedTenant.Value.TenantId, expectedTrainingIntakeTenantId, StringComparison.Ordinal))
            {
                return AdmissionBoundaryValidation.Fail(
                    "TRAINING_INTAKE_TENANT_SCOPE_MISMATCH",
                    $"DesignConversion training intake {action} action rejected by host tenant boundary.",
                    metadata,
                    statusCode: 403);
            }

            metadata["training_intake_host_tenant_verified"] = bool.TrueString;
        }

        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
            return AdmissionBoundaryValidation.Fail(
                "TRAINING_INTAKE_IDEMPOTENCY_KEY_REQUIRED",
                $"DesignConversion training intake {action} action requires idempotencyKey.",
                metadata);

        metadata["training_intake_admission_boundary_satisfied"] = bool.TrueString;
        if (!hostTenantConfigured)
            metadata["training_intake_tenant_id_ref"] = ToHashRef(request.TenantId, "training-intake-tenant-id");
        metadata["training_intake_idempotency_key_ref"] = ToHashRef(request.IdempotencyKey, "training-intake-idempotency-key");
        metadata["training_intake_admission_boundary_scope"] = authRequired
            ? "route_admission_local_auth_token_tenant_id_idempotency_key_no_replay_cache"
            : "route_admission_tenant_id_idempotency_key_no_auth_token_configured_no_replay_cache";
        return AdmissionBoundaryValidation.Success(metadata);
    }

    private static string? ReadHeader(
        IReadOnlyDictionary<string, string>? headers,
        string headerName)
    {
        if (headers is null || headers.Count == 0)
            return null;

        if (headers.TryGetValue(headerName, out var value))
            return value;

        foreach (var pair in headers)
        {
            if (string.Equals(pair.Key, headerName, StringComparison.OrdinalIgnoreCase))
                return pair.Value;
        }

        return null;
    }

    private static bool ConstantTimeEquals(
        string actual,
        string expected)
    {
        var actualBytes = Encoding.UTF8.GetBytes(actual);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        return actualBytes.Length == expectedBytes.Length &&
               CryptographicOperations.FixedTimeEquals(actualBytes, expectedBytes);
    }

    private static bool IsMutatingAdmissionAction(string action)
        => string.Equals(action, ActionCreate, StringComparison.Ordinal) ||
           string.Equals(action, ActionRun, StringComparison.Ordinal) ||
           string.Equals(action, ActionPromoteLocal, StringComparison.Ordinal) ||
           string.Equals(action, ActionContinueTrainingUploadStatus, StringComparison.Ordinal) ||
           string.Equals(action, ActionRollbackLocalPromotion, StringComparison.Ordinal) ||
           string.Equals(action, ActionApproveRuntimeCandidate, StringComparison.Ordinal) ||
           string.Equals(action, ActionExportApprovedRuntimeCandidates, StringComparison.Ordinal) ||
           string.Equals(action, ActionPostApprovalPreflight, StringComparison.Ordinal);

    private static bool RequiresConfiguredLocalAuthToken(string action)
        => string.Equals(action, ActionApproveRuntimeCandidate, StringComparison.Ordinal) ||
           string.Equals(action, ActionExportApprovedRuntimeCandidates, StringComparison.Ordinal) ||
           string.Equals(action, ActionPostApprovalPreflight, StringComparison.Ordinal);

    private sealed record AdmissionBoundaryValidation(
        string? ErrorCode,
        string? ErrorMessage,
        IReadOnlyDictionary<string, string> BoundaryMetadata,
        int StatusCode = 400)
    {
        public bool IsSuccess => string.IsNullOrWhiteSpace(ErrorCode);

        public static AdmissionBoundaryValidation Success(IReadOnlyDictionary<string, string> metadata)
            => new(null, null, metadata);

        public static AdmissionBoundaryValidation Fail(
            string errorCode,
            string errorMessage,
            IReadOnlyDictionary<string, string> metadata,
            int statusCode = 400)
            => new(errorCode, errorMessage, metadata, statusCode);
    }

    private static DesignConversionModelServerTrainingIntakeRecordRef ToRecordRef(
        DesignConversionTrainingIntakeRecord record)
        => new(
            IntakeId: SafeScalar(record.IntakeId, "training-intake-id"),
            IntakeRef: SafeScalar(record.IntakeRef, "training-intake-ref"),
            SourceHandoffArtifactId: SafeScalar(record.SourceHandoffArtifactId, "training-handoff-artifact-id"),
            SourceHandoffArtifactRef: SafeScalar(record.SourceHandoffArtifactRef, "training-handoff-artifact-ref"),
            ApprovedCandidateCount: record.ApprovedCandidateCount,
            AcceptedCandidateCount: record.AcceptedCandidateCount,
            CandidateIds: record.CandidateRefs.Select(candidate => SafeScalar(candidate.CandidateId, "dpo-candidate-id")).ToArray(),
            TrainingRecordIds: record.CandidateRefs.Select(candidate => SafeScalar(candidate.TrainingRecordId, "dpo-training-record-id")).ToArray(),
            TrainingIntakeOnly: record.TrainingIntakeOnly,
            TrainingNotStarted: record.TrainingNotStarted,
            NotTrained: record.NotTrained,
            WeightsNotProduced: record.WeightsNotProduced,
            CheckpointNotMutated: record.CheckpointNotMutated,
            ModelUploadNotReady: record.ModelUploadNotReady,
            ProductNotReady: record.ProductNotReady,
            HeldOutNotProven: record.HeldOutNotProven,
            SourceDisjointNotProven: record.SourceDisjointNotProven,
            Warnings: SafeList(record.Warnings, "training-intake-warning"),
            BoundaryMetadata: SafeMetadata(record.BoundaryMetadata));

    private static DesignConversionModelServerDpoCandidateApprovalRef ToCandidateApprovalRef(
        DesignConversionDpoCandidateApprovalResult approval)
    {
        var approvedByRef = ToHashRef(approval.ApprovedByRef, "dpo-candidate-approval-reviewer");
        var approvalReasonRef = ToHashRef(approval.ApprovalReason, "dpo-candidate-approval-reason");
        var metadata = new Dictionary<string, string>(SafeMetadata(approval.BoundaryMetadata), StringComparer.Ordinal)
        {
            ["approved_by_ref"] = approvedByRef,
            ["approval_reason"] = approvalReasonRef
        };

        return new(
            LocalArtifactRecordId: SafeScalar(approval.LocalArtifactRecordId, "local-artifact-record-id"),
            CandidateId: SafeScalar(approval.CandidateId, "dpo-candidate-id"),
            TrainingRecordId: SafeScalar(approval.TrainingRecordId, "dpo-training-record-id"),
            UnitId: SafeScalar(approval.UnitId, "dpo-unit-id"),
            UsefulnessScore: approval.UsefulnessScore,
            ApprovalStatus: SafeScalar(approval.ApprovalStatus, "dpo-approval-status"),
            ReviewStatus: SafeScalar(approval.ReviewStatus, "dpo-review-status"),
            ApprovedForTraining: approval.ApprovedForTraining,
            ApprovedByRef: approvedByRef,
            ApprovalReasonRef: approvalReasonRef,
            ApprovedAtUtc: approval.ApprovedAtUtc,
            EvidenceRefs: SafeList(approval.EvidenceRefs, "dpo-approval-evidence-ref"),
            BoundaryMetadata: metadata);
    }

    private static DesignConversionApprovedDpoHandoffArtifact ToPublicApprovedHandoff(
        DesignConversionApprovedDpoHandoffArtifact handoff)
        => new(
            ArtifactId: SafeScalar(handoff.ArtifactId, "approved-dpo-handoff-artifact-id"),
            ArtifactRef: SafeScalar(handoff.ArtifactRef, "approved-dpo-handoff-artifact-ref"),
            SchemaVersion: SafeScalar(handoff.SchemaVersion, "approved-dpo-handoff-schema-version"),
            ExportedAtUtc: handoff.ExportedAtUtc,
            ApprovedCandidateCount: handoff.ApprovedCandidateCount,
            Rows: (handoff.Rows ?? []).Select(ToPublicApprovedHandoffRow).ToArray(),
            Warnings: SafeList(handoff.Warnings, "approved-dpo-handoff-warning"),
            BoundaryMetadata: SafePublicHandoffMetadata(handoff.BoundaryMetadata));

    private static DesignConversionApprovedDpoHandoffRow ToPublicApprovedHandoffRow(
        DesignConversionApprovedDpoHandoffRow row)
    {
        var approvedByRef = ToHashRef(row.ApprovedByRef, "approved-dpo-handoff-reviewer");
        var approvalReasonRef = ToHashRef(row.ApprovalReason, "approved-dpo-handoff-reason");
        var metadata = new Dictionary<string, string>(SafePublicHandoffMetadata(row.BoundaryMetadata), StringComparer.Ordinal)
        {
            ["approved_by_ref"] = approvedByRef,
            ["approval_reason"] = approvalReasonRef
        };

        return new DesignConversionApprovedDpoHandoffRow(
            LocalArtifactRecordId: SafeScalar(row.LocalArtifactRecordId, "approved-dpo-handoff-local-artifact-record-id"),
            CandidateId: SafeScalar(row.CandidateId, "approved-dpo-handoff-candidate-id"),
            TrainingRecordId: SafeScalar(row.TrainingRecordId, "approved-dpo-handoff-training-record-id"),
            UnitId: SafeScalar(row.UnitId, "approved-dpo-handoff-unit-id"),
            UsefulnessScore: row.UsefulnessScore,
            ApprovalStatus: SafeScalar(row.ApprovalStatus, "approved-dpo-handoff-approval-status"),
            ReviewStatus: SafeScalar(row.ReviewStatus, "approved-dpo-handoff-review-status"),
            ApprovedForTraining: row.ApprovedForTraining,
            TrainingHandoffOnly: row.TrainingHandoffOnly,
            ApprovedByRef: approvedByRef,
            ApprovalReason: approvalReasonRef,
            ApprovedAtUtc: SafeScalar(row.ApprovedAtUtc, "approved-dpo-handoff-approved-at-utc"),
            SourceManifestRef: SafeScalar(row.SourceManifestRef, "approved-dpo-handoff-source-manifest-ref"),
            RecordHash: SafeScalar(row.RecordHash, "approved-dpo-handoff-record-hash"),
            InternalUnitStateHash: SafeScalar(row.InternalUnitStateHash, "approved-dpo-handoff-internal-unit-state-hash"),
            SourceStructureHash: SafeScalar(row.SourceStructureHash, "approved-dpo-handoff-source-structure-hash"),
            ChosenPayloadHash: SafeScalar(row.ChosenPayloadHash, "approved-dpo-handoff-chosen-payload-hash"),
            RejectedPayloadHash: SafeScalar(row.RejectedPayloadHash, "approved-dpo-handoff-rejected-payload-hash"),
            TieBreakerHash: SafeScalar(row.TieBreakerHash, "approved-dpo-handoff-tie-breaker-hash"),
            ChosenHtmlRef: SafeScalar(row.ChosenHtmlRef, "approved-dpo-handoff-chosen-html-ref"),
            ChosenCssRef: SafeScalar(row.ChosenCssRef, "approved-dpo-handoff-chosen-css-ref"),
            RejectedHtmlRef: SafeScalar(row.RejectedHtmlRef, "approved-dpo-handoff-rejected-html-ref"),
            RejectedCssRef: SafeScalar(row.RejectedCssRef, "approved-dpo-handoff-rejected-css-ref"),
            DesignTreeRef: SafeScalar(row.DesignTreeRef, "approved-dpo-handoff-design-tree-ref"),
            OriginalTreeRef: SafeScalar(row.OriginalTreeRef, "approved-dpo-handoff-original-tree-ref"),
            SourceArtifactRefs: SafeList(row.SourceArtifactRefs, "approved-dpo-handoff-source-artifact-ref"),
            RenderArtifactRefs: SafeList(row.RenderArtifactRefs, "approved-dpo-handoff-render-artifact-ref"),
            ComparisonArtifactRefs: SafeList(row.ComparisonArtifactRefs, "approved-dpo-handoff-comparison-artifact-ref"),
            EvidenceRefs: SafeList(row.EvidenceRefs, "approved-dpo-handoff-evidence-ref"),
            BoundaryMetadata: metadata);
    }

    private static IReadOnlyDictionary<string, string> SafePublicHandoffMetadata(
        IReadOnlyDictionary<string, string>? metadata)
    {
        if (metadata is null || metadata.Count == 0)
            return new Dictionary<string, string>(StringComparer.Ordinal);

        var safe = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in metadata)
        {
            var trimmedKey = pair.Key?.Trim() ?? string.Empty;
            var key = IsPublicHandoffMetadataKey(trimmedKey)
                ? trimmedKey
                : SafeScalar(trimmedKey, "approved-dpo-handoff-metadata-key");
            safe[key] = SafeScalar(pair.Value, key);
        }

        return safe;
    }

    private static bool IsPublicHandoffMetadataKey(string value)
        => value.Length is > 0 and <= 128
           && value.All(character =>
               character is >= 'a' and <= 'z'
               || character is >= '0' and <= '9'
               || character == '_');

    private static DesignConversionModelServerTrainingIntakePreflightRef ToPreflightRef(
        DesignConversionTrainingIntakePreflightResult preflight)
        => new(
            PreflightId: SafeScalar(preflight.PreflightId, "training-intake-preflight-id"),
            TrainingIntakeId: SafeScalar(preflight.TrainingIntakeId, "training-intake-id"),
            AcceptedCandidateCount: preflight.AcceptedCandidateCount,
            MayStartTraining: preflight.MayStartTraining,
            SourceManifestStatus: SafeScalar(preflight.SourceManifestStatus, "source-manifest-status"),
            SourceDisjointStatus: SafeScalar(preflight.SourceDisjointStatus, "source-disjoint-status"),
            TrainingStarted: preflight.TrainingStarted,
            CheckpointMutated: preflight.CheckpointMutated,
            WeightsProduced: preflight.WeightsProduced,
            ModelUploadReady: preflight.ModelUploadReady,
            Warnings: SafeList(preflight.Warnings, "training-intake-preflight-warning"),
            BlockReasons: SafeList(preflight.BlockReasons, "training-intake-preflight-block"),
            BoundaryMetadata: SafeMetadata(preflight.BoundaryMetadata));

    private static DesignConversionModelServerApprovedRuntimeDpoPreflightRef ToApprovedRuntimeDpoPreflightRef(
        DesignConversionApprovedRuntimeDpoCandidateTrainingIntakePreflightResult preflight)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["approved_runtime_dpo_preflight_route_supported"] = bool.TrueString,
            ["approved_runtime_dpo_preflight_only"] = bool.TrueString,
            ["approved_runtime_dpo_preflight_local_artifact_record_id"] =
                SafeScalar(preflight.LocalArtifactRecordId, "approved-runtime-dpo-preflight-local-artifact-record-id"),
            ["approved_runtime_dpo_preflight_candidate_id"] =
                SafeScalar(preflight.CandidateId, "approved-runtime-dpo-preflight-candidate-id"),
            ["approved_runtime_dpo_preflight_training_record_id"] =
                SafeScalar(preflight.TrainingRecordId, "approved-runtime-dpo-preflight-training-record-id"),
            ["approved_runtime_dpo_preflight_unit_id"] =
                SafeScalar(preflight.UnitId, "approved-runtime-dpo-preflight-unit-id"),
            ["approved_runtime_dpo_preflight_source_handoff_artifact_ref"] =
                SafeScalar(preflight.SourceHandoffArtifactRef, "approved-runtime-dpo-preflight-handoff-ref"),
            ["approved_runtime_dpo_preflight_intake_ref"] =
                SafeScalar(preflight.IntakeRef, "approved-runtime-dpo-preflight-intake-ref"),
            ["approved_runtime_dpo_preflight_stable_intake_id_ref"] =
                SafeScalar(preflight.StableIntakeIdRef, "approved-runtime-dpo-preflight-stable-intake-ref"),
            ["approved_runtime_dpo_preflight_idempotency_key_ref"] =
                SafeScalar(preflight.IdempotencyKeyRef, "approved-runtime-dpo-preflight-idempotency-ref"),
            ["training_intake_replay_cache_checked"] = bool.TrueString,
            ["training_intake_idempotency_replay_hit"] = preflight.IdempotencyReplayHit.ToString(),
            ["training_intake_idempotency_created_new_side_effect"] =
                preflight.IdempotencyCreatedNewSideEffect.ToString(),
            ["training_intake_idempotent_replay_result"] = preflight.IdempotencyReplayHit.ToString(),
            ["preflight_id"] = SafeScalar(preflight.PreflightId, "approved-runtime-dpo-preflight-id"),
            ["preflight_status"] = SafeScalar(preflight.PreflightStatus, "approved-runtime-dpo-preflight-status"),
            ["may_start_training"] = preflight.MayStartTraining.ToString(),
            ["accepted_candidate_count"] = preflight.AcceptedCandidateCount.ToString(CultureInfo.InvariantCulture),
            ["coordinator_dpo_step_count"] = preflight.DpoStepCount.ToString(CultureInfo.InvariantCulture),
            ["coordinator_measurement_gate_id"] =
                SafeScalar(preflight.MeasurementGateId, "approved-runtime-dpo-measurement-gate-id"),
            ["coordinator_measurement_gate_unit_id"] =
                SafeScalar(preflight.MeasurementGateUnitId, "approved-runtime-dpo-measurement-gate-unit-id"),
            ["coordinator_measurement_gate_contract_ref"] =
                SafeScalar(preflight.MeasurementGateContractRef, "approved-runtime-dpo-measurement-gate-contract-ref"),
            ["training_runner_invoked"] = bool.FalseString,
            ["training_started"] = bool.FalseString,
            ["checkpoint_mutated"] = bool.FalseString,
            ["weights_produced"] = bool.FalseString,
            ["model_upload_ready"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };

        var evidenceRefs = BuildApprovedRuntimeDpoPreflightEvidenceRefs(preflight);
        var warnings = BuildApprovedRuntimeDpoPreflightWarnings();

        return new(
            LocalArtifactRecordId: SafeScalar(preflight.LocalArtifactRecordId, "approved-runtime-dpo-preflight-local-artifact-record-id"),
            CandidateId: SafeScalar(preflight.CandidateId, "approved-runtime-dpo-preflight-candidate-id"),
            TrainingRecordId: SafeScalar(preflight.TrainingRecordId, "approved-runtime-dpo-preflight-training-record-id"),
            UnitId: SafeScalar(preflight.UnitId, "approved-runtime-dpo-preflight-unit-id"),
            SourceHandoffArtifactRef: SafeScalar(preflight.SourceHandoffArtifactRef, "approved-runtime-dpo-preflight-handoff-ref"),
            IntakeId: SafeScalar(preflight.IntakeId, "training-intake-id"),
            IntakeRef: SafeScalar(preflight.IntakeRef, "approved-runtime-dpo-preflight-intake-ref"),
            StableIntakeIdRef: SafeScalar(preflight.StableIntakeIdRef, "approved-runtime-dpo-preflight-stable-intake-ref"),
            IdempotencyKeyRef: SafeScalar(preflight.IdempotencyKeyRef, "approved-runtime-dpo-preflight-idempotency-ref"),
            IdempotencyReplayHit: preflight.IdempotencyReplayHit,
            IdempotencyCreatedNewSideEffect: preflight.IdempotencyCreatedNewSideEffect,
            PreflightId: SafeScalar(preflight.PreflightId, "approved-runtime-dpo-preflight-id"),
            PreflightStatus: SafeScalar(preflight.PreflightStatus, "approved-runtime-dpo-preflight-status"),
            MayStartTraining: preflight.MayStartTraining,
            AcceptedCandidateCount: preflight.AcceptedCandidateCount,
            DpoStepCount: preflight.DpoStepCount,
            MeasurementGateId: SafeScalar(preflight.MeasurementGateId, "approved-runtime-dpo-measurement-gate-id"),
            MeasurementGateUnitId: SafeScalar(preflight.MeasurementGateUnitId, "approved-runtime-dpo-measurement-gate-unit-id"),
            MeasurementGateContractRef: SafeScalar(preflight.MeasurementGateContractRef, "approved-runtime-dpo-measurement-gate-contract-ref"),
            PersistedRequestedAtUtc: preflight.PersistedRequestedAtUtc,
            TrainingRunnerInvoked: false,
            TrainingStarted: false,
            CheckpointMutated: false,
            WeightsProduced: false,
            ModelUploadReady: false,
            EvidenceRefs: evidenceRefs,
            Warnings: warnings,
            BoundaryMetadata: metadata);
    }

    private static IReadOnlyList<string> BuildApprovedRuntimeDpoPreflightEvidenceRefs(
        DesignConversionApprovedRuntimeDpoCandidateTrainingIntakePreflightResult preflight)
    {
        var evidenceRefs = new SortedSet<string>(StringComparer.Ordinal)
        {
            "approved_runtime_dpo_preflight_route_refs_only",
            "approved_runtime_dpo_preflight_domain_delegation_completed",
            "approved_runtime_dpo_preflight_only",
            "approved_runtime_dpo_preflight_raw_domain_evidence_refs_not_forwarded",
            "training_runner_invoked=false",
            "training_started=false",
            "checkpoint_mutated=false",
            "weights_produced=false",
            "model_upload_ready=false"
        };

        if (preflight.IdempotencyReplayHit)
            evidenceRefs.Add("training_intake_idempotency_replay_hit=true");
        else if (preflight.IdempotencyCreatedNewSideEffect)
            evidenceRefs.Add("training_intake_idempotency_created_new_side_effect=true");

        if (!string.IsNullOrWhiteSpace(preflight.SourceHandoffArtifactRef))
            evidenceRefs.Add(SafeScalar(preflight.SourceHandoffArtifactRef, "approved-runtime-dpo-preflight-handoff-ref"));
        if (!string.IsNullOrWhiteSpace(preflight.IntakeRef))
            evidenceRefs.Add(SafeScalar(preflight.IntakeRef, "approved-runtime-dpo-preflight-intake-ref"));
        if (!string.IsNullOrWhiteSpace(preflight.StableIntakeIdRef))
            evidenceRefs.Add(SafeScalar(preflight.StableIntakeIdRef, "approved-runtime-dpo-preflight-stable-intake-ref"));
        if (!string.IsNullOrWhiteSpace(preflight.IdempotencyKeyRef))
            evidenceRefs.Add(SafeScalar(preflight.IdempotencyKeyRef, "approved-runtime-dpo-preflight-idempotency-ref"));
        if (!string.IsNullOrWhiteSpace(preflight.PreflightId))
            evidenceRefs.Add(SafeScalar(preflight.PreflightId, "approved-runtime-dpo-preflight-id"));
        if (!string.IsNullOrWhiteSpace(preflight.MeasurementGateContractRef))
            evidenceRefs.Add(SafeScalar(preflight.MeasurementGateContractRef, "approved-runtime-dpo-measurement-gate-contract-ref"));

        return evidenceRefs.ToArray();
    }

    private static IReadOnlyList<string> BuildApprovedRuntimeDpoPreflightWarnings()
        =>
        [
            "post_approval_preflight_only",
            "training_runner_invoked=false",
            "training_started=false",
            "checkpoint_mutated=false",
            "weights_produced=false",
            "model_upload_ready=false",
            "domain_evidence_refs_and_warnings_not_forwarded",
            "product_ready=false"
        ];

    private static DesignConversionModelServerTrainingIntakeRunRef ToRunRef(
        DesignConversionTrainingIntakeRunResult run)
        => new(
            RunId: SafeScalar(run.RunId, "training-intake-run-id"),
            TrainingIntakeId: SafeScalar(run.TrainingIntakeId, "training-intake-id"),
            PreflightId: SafeScalar(run.PreflightId, "training-intake-preflight-id"),
            AcceptedCandidateCount: run.AcceptedCandidateCount,
            ResolvedTrainingRecordCount: run.ResolvedTrainingRecordCount,
            MayStartTraining: run.MayStartTraining,
            SourceManifestStatus: SafeScalar(run.SourceManifestStatus, "source-manifest-status"),
            SourceDisjointStatus: SafeScalar(run.SourceDisjointStatus, "source-disjoint-status"),
            TrainingRunnerInvoked: run.TrainingRunnerInvoked,
            LocalLifecycleHookAttempted: run.LocalLifecycleHookAttempted,
            TrainingStarted: run.TrainingStarted,
            CheckpointMutated: run.CheckpointMutated,
            CheckpointCandidateCreated: run.CheckpointCandidateCreated,
            WeightsProduced: run.WeightsProduced,
            ModelUploadReady: run.ModelUploadReady,
            TrainingExecutionBoundary: SafeScalar(run.TrainingExecutionBoundary, "training-execution-boundary"),
            TrainingExecutionStatus: SafeScalar(run.TrainingExecutionStatus, "training-execution-status"),
            RunnerBlockedReason: SafeScalar(run.RunnerBlockedReason, "training-runner-block"),
            CheckpointStatus: SafeScalar(run.CheckpointStatus, "checkpoint-status"),
            CheckpointMutationStatus: SafeScalar(run.CheckpointMutationStatus, "checkpoint-mutation-status"),
            WeightProofStatus: SafeScalar(run.WeightProofStatus, "weight-proof-status"),
            MeasurementGateStatus: SafeScalar(run.MeasurementGateStatus, "measurement-gate-status"),
            PromotionGateStatus: SafeScalar(run.PromotionGateStatus, "promotion-gate-status"),
            UploadReadinessStatus: SafeScalar(run.UploadReadinessStatus, "upload-readiness-status"),
            PromotionBlockedReasons: SafeList(run.PromotionBlockedReasons, "promotion-block"),
            RunnerEvidence: run.RunnerEvidence is null ? null : ToRunnerEvidenceRef(run.RunnerEvidence),
            Warnings: SafeList(run.Warnings, "training-intake-run-warning"),
            BlockReasons: SafeList(run.BlockReasons, "training-intake-run-block"),
            BoundaryMetadata: SafeMetadata(run.BoundaryMetadata));

    private static DesignConversionModelServerTrainingIntakeRunnerEvidenceRef ToRunnerEvidenceRef(
        DesignConversionTrainingIntakeRunnerEvidence evidence)
        => new(
            BoundaryDecisionKind: SafeScalar(evidence.BoundaryDecisionKind, "training-boundary-decision"),
            DatasetCountGatePassed: evidence.DatasetCountGatePassed,
            RequiredTrainRecords: evidence.RequiredTrainRecords,
            RequiredHeldOutRecords: evidence.RequiredHeldOutRecords,
            ActualTrainRecords: evidence.ActualTrainRecords,
            ActualHeldOutRecords: evidence.ActualHeldOutRecords,
            SourceDisjoint: evidence.SourceDisjoint,
            SourceManifestStatus: SafeScalar(evidence.SourceManifestStatus, "source-manifest-status"),
            SourceDisjointStatus: SafeScalar(evidence.SourceDisjointStatus, "source-disjoint-status"),
            MachineLifecycleHookAttempted: evidence.MachineLifecycleHookAttempted,
            TrainingStarted: evidence.TrainingStarted,
            TrainingExecutionStatus: SafeScalar(evidence.TrainingExecutionStatus, "training-execution-status"),
            RunnerBlockedReason: SafeScalar(evidence.RunnerBlockedReason, "training-runner-block"),
            CheckpointCandidateStatus: SafeScalar(evidence.CheckpointCandidateStatus, "checkpoint-candidate-status"),
            CheckpointCandidateCreated: evidence.CheckpointCandidateCreated,
            CheckpointMutationStatus: SafeScalar(evidence.CheckpointMutationStatus, "checkpoint-mutation-status"),
            CheckpointMutationEvidenceProven: evidence.CheckpointMutationEvidenceProven,
            CheckpointContainsRealTrainedWeights: evidence.CheckpointContainsRealTrainedWeights,
            WeightProofStatus: SafeScalar(evidence.WeightProofStatus, "weight-proof-status"),
            FreshLoadStatus: SafeScalar(evidence.FreshLoadStatus, "fresh-load-status"),
            ContinueTrainingStatus: SafeScalar(evidence.ContinueTrainingStatus, "continue-training-status"),
            MeasurementGateStatus: SafeScalar(evidence.MeasurementGateStatus, "measurement-gate-status"),
            PromotionStatus: SafeScalar(evidence.PromotionStatus, "promotion-status"),
            PromotionUploadAllowed: evidence.PromotionUploadAllowed,
            EvidenceRefs: SafeList(evidence.EvidenceRefs, "runner-evidence-ref"),
            FailureReasons: SafeList(evidence.FailureReasons, "runner-failure"),
            Warnings: SafeList(evidence.Warnings, "runner-warning"));

    private static DesignConversionModelServerTrainingIntakeLocalPromotionRef ToLocalPromotionRef(
        DesignConversionTrainingIntakeLocalPromotionResult promotion)
        => new(
            TrainingRun: ToRunRef(promotion.TrainingRun),
            LocalPromotionReady: promotion.LocalPromotionReady,
            ExternalCloudUploadAttempted: promotion.ExternalCloudUploadAttempted,
            PromotionOutputDirectoryRef: ToHashRef(promotion.PromotionOutputDirectory, "local-promotion-output-directory"),
            PromotedCheckpointRef: ToHashRef(promotion.PromotedCheckpointPath, "local-promoted-checkpoint-path"),
            PromotionUploadRef: SafeOptionalScalar(promotion.Publication?.Manifest.PromotionUploadRef, "local-promotion-upload-ref"),
            PromotedLocalModelId: SafeOptionalScalar(promotion.PromotedLocalModel?.ModelId, "local-promoted-model-id"),
            PromotedLocalModelVersion: SafeOptionalScalar(promotion.PromotedLocalModel?.Version, "local-promoted-model-version"),
            PromotedCheckpointSha256: SafeOptionalScalar(promotion.PromotedLocalModel?.CheckpointSha256, "local-promoted-checkpoint-sha256"),
            CheckpointFreshLoadVerified: promotion.PromotedLocalModel?.CheckpointFreshLoadVerified,
            CheckpointContainsRealTrainedWeights: promotion.PromotedLocalModel?.CheckpointContainsRealTrainedWeights,
            PromotionManifestClaimed: promotion.PromotedLocalModel?.PromotionManifestClaimed,
            LocalOnly: promotion.PromotedLocalModel?.LocalOnly,
            EvidenceRefs: SafeList(promotion.EvidenceRefs, "local-promotion-evidence-ref"),
            Warnings: SafeList(promotion.Warnings, "local-promotion-warning"),
            BlockReasons: SafeList(promotion.BlockReasons, "local-promotion-block"),
            BoundaryMetadata: SafeMetadata(promotion.BoundaryMetadata));

    private static DesignConversionModelServerTrainingIntakeLocalPromotionRollbackRef ToRollbackRef(
        DesignConversionTrainingIntakeLocalPromotionRollbackResult rollback)
        => new(
            RollbackId: SafeScalar(rollback.RollbackId, "local-promotion-rollback-id"),
            RollbackApplied: rollback.RollbackApplied,
            PriorStateRestored: rollback.PriorStateRestored,
            NoPriorStateRecorded: rollback.NoPriorStateRecorded,
            RolledBackLocalModelRef: SafeScalar(rollback.RolledBackLocalModelRef, "local-promotion-rollback-target-ref"),
            PreviousActiveLocalModelRef: SafeScalar(rollback.PreviousActiveLocalModelRef, "local-promotion-rollback-previous-active-ref"),
            RestoredActiveLocalModelRef: SafeScalar(rollback.RestoredActiveLocalModelRef, "local-promotion-rollback-restored-active-ref"),
            RestoredActiveCheckpointSha256: SafeScalar(rollback.RestoredActiveCheckpointSha256, "local-promotion-rollback-restored-checkpoint-sha256"),
            EvidenceRefs: SafeList(rollback.EvidenceRefs, "local-promotion-rollback-evidence-ref"),
            Warnings: SafeList(rollback.Warnings, "local-promotion-rollback-warning"),
            BlockReasons: SafeList(rollback.BlockReasons, "local-promotion-rollback-block"),
            BoundaryMetadata: SafeMetadata(rollback.BoundaryMetadata));

    private static DesignConversionModelServerTrainingIntakeLifecycleStatusRef ToLifecycleStatusRef(
        DesignConversionTrainingIntakeLocalPromotionResult promotion,
        DesignConversionModelServerTrainingIntakeLocalPromotionRef promotionRef,
        DesignConversionModelServerTrainingIntakeRequest request)
    {
        var continueTrainingStatus = SafeScalar(
            promotion.TrainingRun.RunnerEvidence?.ContinueTrainingStatus ??
            ReadMetadata(promotion.TrainingRun.BoundaryMetadata, "continue_training_status") ??
            "not_proven",
            "continue-training-status");
        var localContinueTrainingVerified = string.Equals(
            continueTrainingStatus,
            DesignConversionLocalContinueTrainingProbe.VerifiedStatus,
            StringComparison.Ordinal);
        var model = promotion.PromotedLocalModel;
        var promotedCheckpointId = string.IsNullOrWhiteSpace(model?.CheckpointSha256)
            ? string.Empty
            : $"checkpoint-sha256:{model.CheckpointSha256[..16]}";
        var publication = promotion.Publication;
        var receipt = publication?.UploadReceipt;
        var modelServerUploadAttempted = publication?.Manifest.ModelServerUploadAttempted == true;
        var modelServerPromotionClaimed = publication?.Manifest.ModelServerPromotionClaimed == true;
        var remoteUploadVerified = publication?.PublicationKind == DesignConversionPromotionPublicationKind.VerifiedModelServerUpload
                                   && string.Equals(
                                       publication.PublicationStatus,
                                       DesignConversionPromotionPublicationStatus.UploadedVerified,
                                       StringComparison.Ordinal)
                                   && modelServerUploadAttempted
                                   && modelServerPromotionClaimed
                                   && receipt?.UploadAttempted == true
                                   && receipt.UploadSucceeded
                                   && receipt.VerificationSucceeded
                                   && string.Equals(
                                       receipt.PublicationStatus,
                                       DesignConversionPromotionPublicationStatus.UploadedVerified,
                                       StringComparison.Ordinal);
        var externalCloudUploadAttempted = promotion.ExternalCloudUploadAttempted
                                           || receipt?.ExternalCloudUploadAttempted == true;
        var uploadAttempted = remoteUploadVerified;
        var uploadStatus = remoteUploadVerified
            ? SafeScalar(
                publication?.Manifest.PromotionReceiptKind ?? "model_server_upload_verified",
                "promotion-upload-status")
            : promotion.LocalPromotionReady
                ? "blocked_local_only_no_external_upload_after_local_promotion"
                : $"blocked_local_only:{SafeScalar(promotion.TrainingRun.UploadReadinessStatus, "upload-readiness-status")}";
        var cloudUploadStatus = remoteUploadVerified
            ? externalCloudUploadAttempted
                ? SafeScalar(
                    publication?.PublicationStatus ?? receipt?.PublicationStatus ?? DesignConversionPromotionPublicationStatus.UploadedVerified,
                    "cloud-upload-status")
                : "not_external_cloud_repository_upload_verified"
            : DesignConversionPromotionPublicationStatus.NotUploaded;
        var deploymentStatus = remoteUploadVerified
            ? "blocked_no_deployment_proof_after_verified_upload"
            : "blocked_local_only_no_deploy";
        var localOnlyLifecycleStatus = !remoteUploadVerified;
        var remoteUploadEvidencePresent = remoteUploadVerified
                                          && (receipt?.EvidenceRefs?.Count > 0
                                              || promotionRef.EvidenceRefs.Any(reference =>
                                                  reference.StartsWith("UPLOAD_RECEIPT_EVIDENCE_SHA256:", StringComparison.Ordinal)));
        var localPublishPackageRef = SafeOptionalScalar(
            ReadMetadata(promotion.BoundaryMetadata, "promotion_upload_ref") ?? promotionRef.PromotionUploadRef,
            "local-publish-package-ref") ?? string.Empty;
        var localPublishManifestRef = SafeOptionalScalar(
            model?.ManifestRef,
            "local-publish-manifest-ref") ?? string.Empty;
        var localPublishBackupRef = SafeOptionalScalar(
            ReadMetadata(promotion.BoundaryMetadata, "local_promotion_prior_active_local_model_ref"),
            "local-publish-backup-ref") ?? "none";
        var rollbackBoundaryRef = SafeScalar(
            ReadMetadata(promotion.BoundaryMetadata, "local_promotion_rollback_boundary"),
            "local-promotion-rollback-boundary");
        var continuedFromCheckpointRef = SafeOptionalScalar(
            model?.DpoPolicyCheckpointSha256Ref,
            "continued-from-checkpoint-ref") ?? string.Empty;
        var commandId = ToHashRef(
            string.Join("|",
            [
                ActionContinueTrainingUploadStatus,
                promotion.TrainingRun.TrainingIntakeId,
                promotion.TrainingRun.RunId,
                request.RequestedByRef ?? string.Empty,
                request.Reason ?? string.Empty
            ]),
            "local-lifecycle-command");
        var metadata = new Dictionary<string, string>(SafeMetadata(promotion.BoundaryMetadata), StringComparer.Ordinal)
        {
            ["lifecycle_command_surface"] = "continue_training_upload_deploy_status",
            ["lifecycle_command_id"] = commandId,
            ["lifecycle_requested_by_ref"] = SafeScalar(request.RequestedByRef, "lifecycle-requester"),
            ["lifecycle_request_reason_ref"] = ToHashRef(request.Reason, "lifecycle-request-reason"),
            ["continue_training_requested"] = bool.TrueString,
            ["continue_training_status"] = continueTrainingStatus,
            ["local_continue_training_verified"] = localContinueTrainingVerified.ToString(),
            ["upload_requested"] = bool.TrueString,
            ["upload_status"] = uploadStatus,
            ["cloud_upload_status"] = cloudUploadStatus,
            ["local_publish_package_ref"] = localPublishPackageRef,
            ["local_publish_manifest_ref"] = localPublishManifestRef,
            ["local_publish_backup_ref"] = localPublishBackupRef,
            ["local_publish_rollback_boundary_ref"] = rollbackBoundaryRef,
            ["continued_from_checkpoint_ref"] = continuedFromCheckpointRef,
            ["upload_attempted"] = uploadAttempted.ToString(),
            ["model_server_upload_attempted"] = modelServerUploadAttempted.ToString(),
            ["model_server_upload_verified"] = remoteUploadVerified.ToString(),
            ["model_server_promotion_claimed"] = modelServerPromotionClaimed.ToString(),
            ["promotion_publication_status"] = SafeScalar(
                publication?.PublicationStatus ?? DesignConversionPromotionPublicationStatus.NotUploaded,
                "promotion-publication-status"),
            ["promotion_receipt_kind"] = SafeScalar(
                publication?.Manifest.PromotionReceiptKind ?? "none",
                "promotion-receipt-kind"),
            ["remote_upload_evidence_present"] = remoteUploadEvidencePresent.ToString(),
            ["deploy_requested"] = bool.TrueString,
            ["deployment_status"] = deploymentStatus,
            ["deployment_attempted"] = bool.FalseString,
            ["deployment_verified"] = bool.FalseString,
            ["product_ready"] = bool.FalseString,
            ["promoted_checkpoint_id"] = promotedCheckpointId,
            ["promoted_checkpoint_path_ref"] = promotionRef.PromotedCheckpointRef,
            ["promoted_runtime_model_id"] = promotionRef.PromotedLocalModelId ?? string.Empty,
            ["external_cloud_upload_attempted"] = externalCloudUploadAttempted.ToString(),
            ["external_deploy_attempted"] = bool.FalseString,
            ["local_only_lifecycle_status"] = localOnlyLifecycleStatus.ToString()
        };

        return new DesignConversionModelServerTrainingIntakeLifecycleStatusRef(
            CommandId: commandId,
            RequestedByRef: SafeScalar(request.RequestedByRef, "lifecycle-requester"),
            RequestReasonRef: ToHashRef(request.Reason, "lifecycle-request-reason"),
            RequestedOperation: "continue-training-and-upload-deploy-status",
            ContinueTrainingRequested: true,
            ContinueTrainingStatus: continueTrainingStatus,
            LocalContinueTrainingVerified: localContinueTrainingVerified,
            UploadRequested: true,
            UploadStatus: uploadStatus,
            CloudUploadStatus: cloudUploadStatus,
            LocalPublishPackageRef: localPublishPackageRef,
            LocalPublishManifestRef: localPublishManifestRef,
            LocalPublishBackupRef: localPublishBackupRef,
            RollbackBoundaryRef: rollbackBoundaryRef,
            ContinuedFromCheckpointRef: continuedFromCheckpointRef,
            UploadAttempted: uploadAttempted,
            DeployRequested: true,
            DeploymentStatus: deploymentStatus,
            DeploymentAttempted: false,
            DeploymentVerified: false,
            ProductReady: false,
            TrainingIntakeId: promotionRef.TrainingRun.TrainingIntakeId,
            TrainingRunId: promotionRef.TrainingRun.RunId,
            CheckpointStatus: promotionRef.TrainingRun.CheckpointStatus,
            CheckpointMutationStatus: promotionRef.TrainingRun.CheckpointMutationStatus,
            PromotedCheckpointId: promotedCheckpointId,
            PromotedCheckpointPathRef: promotionRef.PromotedCheckpointRef,
            PromotionUploadRef: promotionRef.PromotionUploadRef,
            PromotedRuntimeModelId: promotionRef.PromotedLocalModelId,
            PromotedLocalModelVersion: promotionRef.PromotedLocalModelVersion,
            LocalPromotionReady: promotionRef.LocalPromotionReady,
            LocalOnly: promotionRef.LocalOnly == true,
            EvidenceRefs: promotionRef.EvidenceRefs
                .Append(remoteUploadVerified
                    ? "DESIGN_CONVERSION_MODEL_LIFECYCLE_COMMAND_SURFACE_REMOTE_UPLOAD_VERIFIED"
                    : "DESIGN_CONVERSION_MODEL_LIFECYCLE_COMMAND_SURFACE_LOCAL_ONLY")
                .Append("DESIGN_CONVERSION_CONTINUE_TRAINING_UPLOAD_STATUS_PRODUCT_READY_FALSE")
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            Warnings: promotionRef.Warnings
                .Append(remoteUploadVerified
                    ? "continue_training_upload_status_remote_upload_verified_non_deployed"
                    : "continue_training_upload_status_local_only")
                .Concat(remoteUploadVerified
                    ? Array.Empty<string>()
                    : new[] { "upload_deploy_blocked_local_only" })
                .Append("product_ready=false")
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            BlockReasons: promotionRef.BlockReasons
                .Append(remoteUploadVerified
                    ? "external_deployment_not_proven"
                    : "external_upload_deploy_not_proven")
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            BoundaryMetadata: metadata);
    }

    private static IReadOnlyList<string> BuildWarnings(
        string action,
        bool remotePromotionConfigured)
    {
        var localityWarning = remotePromotionConfigured
            ? "training_intake_route_remote_promotion_configured=true"
            : "training_intake_route_local_only=true";

        return
        [
            localityWarning,
            "modelserver_route_delegates_to_domain_dpo_usefulness_api",
            $"training_intake_action={action}",
            "response_refs_or_hashes_only=true",
            "raw_html_css_design_tree_provider_payloads_not_emitted",
            "private_file_paths_not_emitted",
            "public_http_endpoint_verified=false",
            "deployment_verified=false",
            "product_ready=false"
        ];
    }

    private DesignConversionModelServerHttpResponse DomainError(
        string method,
        string path,
        string action,
        string? errorCode,
        string? errorMessage,
        IReadOnlyDictionary<string, string>? boundaryMetadata = null)
        => Error(
            method,
            path,
            400,
            string.IsNullOrWhiteSpace(errorCode) ? "TRAINING_INTAKE_DOMAIN_API_FAILED" : errorCode,
            string.IsNullOrWhiteSpace(errorMessage)
                ? $"DesignConversion training intake action failed: {action}"
                : errorMessage,
            boundaryMetadata);

    private DesignConversionModelServerHttpResponse Error(
        string method,
        string path,
        int statusCode,
        string errorCode,
        string errorMessage,
        IReadOnlyDictionary<string, string>? boundaryMetadata = null)
    {
        var body = new DesignConversionModelServerHttpErrorResponse(
            Endpoint: EndpointPath,
            Method: string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method,
            HttpSeam: DesignConversionModelServerHttpHandler.LocalHttpSeam,
            StatusCode: statusCode,
            ErrorCode: SafeScalar(errorCode, "training-intake-error-code"),
            ErrorMessage: SafeScalar(errorMessage, "training-intake-error-message"),
            BoundaryMetadata: BuildBoundaryMetadata(
                method,
                path,
                statusCode,
                runtimeInvoked: false,
                action: "error",
                _remotePromotionConfigured,
                domainMetadata: boundaryMetadata));

        return Json(statusCode, method, path, body);
    }

    private static DesignConversionModelServerHttpResponse Json<T>(
        int statusCode,
        string method,
        string path,
        T body)
    {
        var headers = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["x-design-conversion-endpoint"] = EndpointPath,
            ["x-design-conversion-http-seam"] = DesignConversionModelServerHttpHandler.LocalHttpSeam,
            ["x-design-conversion-training-intake-route"] = LocalTrainingIntakeRouteBoundary,
            ["x-design-conversion-public-deployment-verified"] = bool.FalseString,
            ["x-design-conversion-cross-repo-caller-verified"] = bool.FalseString,
            ["x-design-conversion-runtime-surface"] = RuntimeSurface,
            ["x-design-conversion-method"] = string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method,
            ["x-design-conversion-path"] = SafeHttpPath(path),
            ["x-design-conversion-status-code"] = statusCode.ToString(CultureInfo.InvariantCulture)
        };

        return new DesignConversionModelServerHttpResponse(
            StatusCode: statusCode,
            ContentType: "application/json; charset=utf-8",
            BodyJson: JsonSerializer.Serialize(body, DesignConversionModelServerHttpHandler.JsonOptions),
            Headers: headers);
    }

    private static IReadOnlyDictionary<string, string> BuildBoundaryMetadata(
        string method,
        string path,
        int statusCode,
        bool runtimeInvoked,
        string action,
        bool remotePromotionConfigured,
        IReadOnlyDictionary<string, string>? domainMetadata)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["http_endpoint_shape"] = "POST /api/design-conversion/training-intake",
            ["http_method"] = string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method,
            ["http_path"] = SafeHttpPath(path),
            ["http_status_code"] = statusCode.ToString(CultureInfo.InvariantCulture),
            ["http_content_type"] = "application/json; charset=utf-8",
            ["local_http_host_seam"] = DesignConversionModelServerHttpHandler.LocalHttpSeam,
            ["local_training_intake_route_boundary"] = LocalTrainingIntakeRouteBoundary,
            ["local_http_host_seam_verified"] = bool.TrueString,
            ["runtime_invoked"] = runtimeInvoked.ToString(),
            ["runtime_surface"] = RuntimeSurface,
            ["api_shape"] = ApiShape,
            ["training_intake_action"] = action,
            ["delegates_to_domain_api"] = nameof(DesignConversionDpoUsefulnessApi),
            ["training_intake_domain_api_methods"] =
                "CreateTrainingIntakeAsync,ListTrainingIntakeRequestsAsync,RunTrainingIntakePreflightAsync,RunTrainingIntakeAsync,RunTrainingIntakeAndPromoteLocalAsync,RollbackTrainingIntakeLocalPromotionAsync,ApproveRuntimeCreatedDpoCandidateAsync,ExportApprovedRuntimeDpoCandidatesAsync,RunApprovedRuntimeDpoCandidateTrainingIntakePreflightAsync",
            ["training_intake_continue_training_upload_status_supported"] = bool.TrueString,
            ["training_intake_local_promotion_rollback_supported"] = bool.TrueString,
            ["training_intake_runtime_candidate_approval_supported"] = bool.TrueString,
            ["training_intake_approved_runtime_export_supported"] = bool.TrueString,
            ["training_intake_approved_runtime_dpo_preflight_supported"] = bool.TrueString,
            ["training_intake_approved_runtime_dpo_preflight_requires_local_auth_token"] = bool.TrueString,
            ["refs_or_hashes_only"] = bool.TrueString,
            ["refs_only_public_response"] = bool.TrueString,
            ["private_file_paths_included"] = bool.FalseString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["raw_user_private_data_included"] = bool.FalseString,
            ["response_contains_raw_private_data"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString,
            ["screenshot_bytes_included"] = bool.FalseString,
            ["external_provider_called"] = bool.FalseString,
            ["external_ai_visual_judge_executed"] = bool.FalseString,
            ["network_call_executed"] = bool.FalseString,
            ["asp_net_host_verified"] = bool.FalseString,
            ["socket_opened"] = bool.FalseString,
            ["public_http_endpoint_verified"] = bool.FalseString,
            ["platform_integration_verified"] = bool.FalseString,
            ["cross_repo_caller_verified"] = bool.FalseString,
            ["deployment_verified"] = bool.FalseString,
            ["public_production_default_ready"] = bool.FalseString,
            ["product_ready"] = bool.FalseString,
            ["production_db_ready"] = bool.FalseString,
            ["trained_model_ready"] = bool.FalseString,
            ["model_readiness_claimed"] = bool.FalseString
        };

        foreach (var pair in DesignConversionModelServerCallerContract.TrainingIntakeBoundaryMetadata())
            metadata[pair.Key] = pair.Value;

        metadata["training_intake_supported_actions"] = string.Join(
            ",",
            [
                ActionList,
                ActionCreate,
                ActionPreflight,
                ActionRun,
                ActionPromoteLocal,
                ActionContinueTrainingUploadStatus,
                ActionRollbackLocalPromotion,
                ActionApproveRuntimeCandidate,
                ActionExportApprovedRuntimeCandidates,
                ActionPostApprovalPreflight
            ]);
        metadata["training_intake_domain_api_methods"] =
            "CreateTrainingIntakeAsync,ListTrainingIntakeRequestsAsync,RunTrainingIntakePreflightAsync,RunTrainingIntakeAsync,RunTrainingIntakeAndPromoteLocalAsync,RollbackTrainingIntakeLocalPromotionAsync,ApproveRuntimeCreatedDpoCandidateAsync,ExportApprovedRuntimeDpoCandidatesAsync,RunApprovedRuntimeDpoCandidateTrainingIntakePreflightAsync";
        metadata["training_intake_local_promotion_rollback_supported"] = bool.TrueString;
        metadata["training_intake_runtime_candidate_approval_supported"] = bool.TrueString;
        metadata["training_intake_approved_runtime_export_supported"] = bool.TrueString;
        metadata["training_intake_approved_runtime_dpo_preflight_supported"] = bool.TrueString;
        metadata["training_intake_approved_runtime_dpo_preflight_requires_local_auth_token"] = bool.TrueString;

        foreach (var pair in SafeMetadata(domainMetadata))
            metadata[pair.Key] = pair.Value;

        metadata["training_intake_remote_promotion_configured"] = remotePromotionConfigured.ToString();
        metadata["training_intake_route_local_only"] = (!remotePromotionConfigured).ToString();
        metadata["training_intake_continue_training_upload_status_local_only"] = (!remotePromotionConfigured).ToString();

        metadata["runtime_invoked"] = runtimeInvoked.ToString();
        metadata["training_intake_action"] = string.Equals(action, "error", StringComparison.Ordinal) && metadata.TryGetValue("training_intake_action", out var metadataAction) && !string.IsNullOrWhiteSpace(metadataAction) ? SafeScalar(metadataAction, "training-intake-action") : SafeScalar(action, "training-intake-action");
        metadata["refs_or_hashes_only"] = bool.TrueString;
        metadata["refs_only_public_response"] = bool.TrueString;
        metadata["private_file_paths_included"] = bool.FalseString;
        metadata["raw_html_included"] = bool.FalseString;
        metadata["raw_css_included"] = bool.FalseString;
        metadata["raw_design_tree_included"] = bool.FalseString;
        metadata["raw_user_private_data_included"] = bool.FalseString;
        metadata["response_contains_raw_private_data"] = bool.FalseString;
        metadata["provider_payload_included"] = bool.FalseString;
        metadata["external_provider_called"] = bool.FalseString;
        metadata["external_ai_visual_judge_executed"] = bool.FalseString;
        metadata["network_call_executed"] = bool.FalseString;
        metadata["public_http_endpoint_verified"] = bool.FalseString;
        metadata["cross_repo_caller_verified"] = bool.FalseString;
        metadata["deployment_verified"] = bool.FalseString;
        metadata["public_production_default_ready"] = bool.FalseString;
        metadata["product_ready"] = bool.FalseString;
        return metadata;
    }

    private static IReadOnlyDictionary<string, string> SafeMetadata(
        IReadOnlyDictionary<string, string>? metadata)
    {
        if (metadata is null || metadata.Count == 0)
            return new Dictionary<string, string>(StringComparer.Ordinal);

        var safe = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in metadata)
        {
            var safeKey = SafeScalar(pair.Key, "metadata-key");
            safe[safeKey] = SafeScalar(pair.Value, safeKey);
        }

        return safe;
    }

    private static IReadOnlyDictionary<string, string> MergeMetadata(
        IReadOnlyDictionary<string, string>? first,
        IReadOnlyDictionary<string, string>? second)
    {
        var merged = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in SafeMetadata(first))
            merged[pair.Key] = pair.Value;
        foreach (var pair in SafeMetadata(second))
            merged[pair.Key] = pair.Value;

        return merged;
    }

    private static string? ReadMetadata(IReadOnlyDictionary<string, string>? metadata, string key)
        => metadata is not null && metadata.TryGetValue(key, out var value)
            ? value
            : null;

    private static IReadOnlyList<string> SafeList(
        IReadOnlyList<string>? values,
        string kind)
        => (values ?? [])
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => SafeScalar(value, kind))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static string? SafeOptionalScalar(string? value, string kind)
        => string.IsNullOrWhiteSpace(value) ? null : SafeScalar(value, kind);

    private static string SafeHttpPath(string? value)
    {
        var normalized = NormalizePath(value);
        return LooksSensitive(normalized) ? ToHashRef(normalized, "http-path") : normalized;
    }

    private static string SafeScalar(string? value, string kind)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var trimmed = value.Trim();
        // Публичный training-intake показывает смысловые метки и hash-ref, но не
        // домашние/tmp/root пути, user-имена или JSON-escaped формы таких путей.
        return LooksSensitive(trimmed) ? ToHashRef(trimmed, kind) : trimmed;
    }

    private static string ToHashRef(string? value, string kind)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
        return $"{SafeHashRefKind(kind)}:sha256:{hash[..16]}";
    }

    private static string SafeHashRefKind(string? kind)
    {
        if (string.IsNullOrWhiteSpace(kind) || LooksSensitive(kind))
            return "sensitive-metadata";

        var safe = new string(kind
            .Trim()
            .Select(character =>
                char.IsLetterOrDigit(character) ||
                character is '-' or '_' or '.'
                    ? char.ToLowerInvariant(character)
                    : '-')
            .Take(96)
            .ToArray())
            .Trim('-', '_', '.');
        return string.IsNullOrWhiteSpace(safe) ? "sensitive-metadata" : safe;
    }

    private static bool LooksSensitive(string value)
    {
        var normalized = NormalizeEscapedPathMarkers(value);
        return LooksSensitiveWithoutEscapes(value) ||
               (!string.Equals(normalized, value, StringComparison.Ordinal) &&
                LooksSensitiveWithoutEscapes(normalized));
    }

    private static bool LooksSensitiveWithoutEscapes(string value)
        => value.Contains("<", StringComparison.Ordinal) ||
           value.Contains("{", StringComparison.Ordinal) ||
           value.Contains("file://", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("private", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("data-secret", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("exact_prompt", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("raw_provider_payload", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("provider_payload", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("user-luba", StringComparison.OrdinalIgnoreCase) ||
           LooksLikeLocalPath(value);

    private static string NormalizeEscapedPathMarkers(string value)
        => value
            .Replace(@"\/", "/", StringComparison.Ordinal)
            .Replace(@"\u002f", "/", StringComparison.OrdinalIgnoreCase)
            .Replace(@"\u005c", @"\", StringComparison.OrdinalIgnoreCase);

    private static bool LooksLikeLocalPath(string value)
    {
        if (value.StartsWith("file:", StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith(@"\\", StringComparison.Ordinal) ||
            ContainsWindowsDriveRoot(value) ||
            ContainsLocalPosixPath(value))
        {
            return true;
        }

        var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return !string.IsNullOrWhiteSpace(profile) &&
               value.Contains(profile, StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsLocalPosixPath(string value)
    {
        var normalized = value.Replace('\\', '/');
        for (var index = 0; index < normalized.Length - 1; index++)
        {
            if (normalized[index] != '/' || !IsPathBoundaryBefore(normalized, index))
                continue;

            if (StartsWithPosixLocalRoot(normalized, index))
                return true;
        }

        return false;
    }

    private static bool StartsWithPosixLocalRoot(string value, int slashIndex)
        => StartsWithPathSegment(value, slashIndex, "/tmp") ||
           StartsWithPathSegment(value, slashIndex, "/var/tmp") ||
           StartsWithPathSegment(value, slashIndex, "/root") ||
           StartsWithPathPrefix(value, slashIndex, "/home/") ||
           StartsWithPathPrefix(value, slashIndex, "/users/") ||
           StartsWithPathPrefix(value, slashIndex, "/mnt/") ||
           StartsWithPathPrefix(value, slashIndex, "/workspaces/") ||
           StartsWithPathPrefix(value, slashIndex, "/workspace/");

    private static bool StartsWithPathSegment(string value, int index, string segment)
        => value.Length >= index + segment.Length &&
           string.Compare(value, index, segment, 0, segment.Length, StringComparison.OrdinalIgnoreCase) == 0 &&
           (value.Length == index + segment.Length ||
            value[index + segment.Length] is '/' or '\\' or '"' or '\'' or '?' or '#' or ':' or ';' or ',' or ')' or ']' or '}');

    private static bool StartsWithPathPrefix(string value, int index, string prefix)
        => value.Length >= index + prefix.Length &&
           string.Compare(value, index, prefix, 0, prefix.Length, StringComparison.OrdinalIgnoreCase) == 0;

    private static bool IsPathBoundaryBefore(string value, int slashIndex)
        => slashIndex == 0 ||
           value[slashIndex - 1] is ' ' or '\t' or '\r' or '\n' or '"' or '\'' or '(' or '[' or '{' or '=' or ':' or ';' or ',';

    private static bool ContainsWindowsDriveRoot(string value)
    {
        for (var index = 0; index < value.Length - 2; index++)
        {
            if (((value[index] is >= 'a' and <= 'z') || (value[index] is >= 'A' and <= 'Z')) &&
                (index == 0 || !char.IsLetterOrDigit(value[index - 1])) &&
                value[index + 1] == ':' &&
                value[index + 2] is '\\' or '/')
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsJsonContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return true;

        return contentType.Split(';', 2)[0].Trim().Equals("application/json", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "/";

        var trimmed = path.Trim();
        var queryIndex = trimmed.IndexOf('?', StringComparison.Ordinal);
        return queryIndex >= 0 ? trimmed[..queryIndex] : trimmed;
    }
}
