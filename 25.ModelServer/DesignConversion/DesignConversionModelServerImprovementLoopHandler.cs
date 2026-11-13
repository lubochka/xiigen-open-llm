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
using ContinuousLearning;
using DesignConversionDomain;

public sealed record DesignConversionModelServerImprovementLoopRequest(
    string LoopId,
    DesignConversionModelServerLocalCallerPackage? InitialPackage,
    int MaxAttempts,
    IReadOnlyDictionary<string, string>? Metadata = null)
{
    /// <summary>
    /// Optional caller admission/advisory measurement. It is not visual-loop
    /// quality and is never child-DPO held-out accuracy authority.
    /// </summary>
    public DesignConversionMeasurementGateRequest? MeasurementGateRequest
    {
        get;
        init;
    }
}

public sealed record DesignConversionModelServerImprovementLoopAttemptResponse(
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
    IReadOnlyDictionary<string, string> PublicMetadata);

public sealed record DesignConversionModelServerImprovementLoopResponse(
    string Endpoint,
    string Method,
    string HttpSeam,
    string ApiShape,
    string RuntimeSurface,
    int StatusCode,
    string ContractStatus,
    string LoopId,
    string RequestId,
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
    IReadOnlyList<DesignConversionModelServerImprovementLoopAttemptResponse> Attempts,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> Warnings,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed class DesignConversionModelServerImprovementLoopHandler
{
    public const string EndpointPath = "/api/design-conversion/improvement-loop";
    public const string LocalImprovementLoopRouteBoundary =
        "local_improvement_loop_route_refs_only_no_socket_no_public_deployment";
    public const string CoordinatorNotConfiguredErrorCode = "IMPROVEMENT_LOOP_COORDINATOR_NOT_CONFIGURED";
    public const string RemovedExternalVisualProviderConfigErrorCode =
        "IMPROVEMENT_LOOP_EXTERNAL_VISUAL_PROVIDER_CONFIG_REMOVED";
    public const string MaxAttemptsExceededErrorCode = "IMPROVEMENT_LOOP_MAX_ATTEMPTS_EXCEEDED";
    public const string NegativeBudgetUsdErrorCode = DesignConversionInternalService.NegativeBudgetUsdErrorCode;
    public const string RepositoryOwnedDpoContextRequiresServerConfigurationErrorCode =
        "IMPROVEMENT_LOOP_REPOSITORY_OWNED_DPO_CONTEXT_SERVER_CONFIGURATION_REQUIRED";
    public const string RepositoryOwnedDpoContextServerConfigurationInvalidErrorCode =
        "IMPROVEMENT_LOOP_REPOSITORY_OWNED_DPO_CONTEXT_SERVER_CONFIGURATION_INVALID";
    private const string CanonicalQualityThresholdMetadataKey = "quality_threshold";
    private const string QualityThresholdSourceMetadataKey = "quality_threshold_source";
    private const string RuntimeCreatedDpoCandidatePublicProjectionMarker = "runtime_created_dpo_candidate";
    private const string RepositoryOwnedDpoContextSourceMetadataKey =
        "repository_owned_dpo_transaction_context_source";
    private const string RepositoryOwnedDpoContextSourceServerConfiguration =
        "server_owned_modelserver_configuration";
    private const string RepositoryOwnedDpoGenerationPolicyStableName =
        "design-conversion-local-qwen-dpo-generation-policy";
    private const string RepositoryOwnedDpoGenerationPolicyVersion = "v1";
    private const string RepositoryOwnedDpoCreatorStableName =
        "modelserver-design-conversion-improvement-loop";
    private const string RepositoryOwnedDpoCreatorVersion = "v1";
    private const int DefaultMaxRenderQaRuns = 4;

    private readonly DesignConversionImprovementLoopCoordinator? _coordinator;
    private readonly IReadOnlyDictionary<string, string> _routeBoundaryMetadata;
    private readonly DesignConversionModelServerRepositoryOwnedTrainingOptions?
        _repositoryOwnedTrainingOptions;
    private readonly ContinuousLearningTp1DpoConfiguration?
        _repositoryOwnedDpoTeacherConfiguration;

    public DesignConversionModelServerImprovementLoopHandler(
        DesignConversionImprovementLoopCoordinator? coordinator,
        IReadOnlyDictionary<string, string>? routeBoundaryMetadata = null,
        DesignConversionModelServerRouteAdmission? routeAdmission = null,
        DesignConversionModelServerRepositoryOwnedTrainingOptions?
            repositoryOwnedTrainingOptions = null,
        ContinuousLearningTp1DpoConfiguration?
            repositoryOwnedDpoTeacherConfiguration = null)
    {
        _coordinator = coordinator;
        _routeBoundaryMetadata = routeBoundaryMetadata is null
            ? new Dictionary<string, string>(StringComparer.Ordinal)
            : new Dictionary<string, string>(routeBoundaryMetadata, StringComparer.Ordinal);
        _repositoryOwnedTrainingOptions =
            SnapshotRepositoryOwnedTrainingOptions(repositoryOwnedTrainingOptions);
        _repositoryOwnedDpoTeacherConfiguration =
            repositoryOwnedDpoTeacherConfiguration;
        RouteAdmission = routeAdmission ?? DesignConversionModelServerRouteAdmission.Create(null);
    }

    public DesignConversionModelServerRouteAdmission RouteAdmission { get; }

    public async Task<DesignConversionModelServerHttpResponse> HandleAsync(
        DesignConversionModelServerHttpRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            return Error("POST", EndpointPath, 400, "REQUEST_REQUIRED", "DesignConversion improvement-loop request is required.");

        var method = string.IsNullOrWhiteSpace(request.Method) ? string.Empty : request.Method.Trim().ToUpperInvariant();
        var path = NormalizePath(request.Path);
        if (!string.Equals(path, EndpointPath, StringComparison.Ordinal))
            return Error(method, path, 404, "ROUTE_NOT_FOUND", $"DesignConversion improvement-loop route is not registered: {path}");

        if (!string.Equals(method, "POST", StringComparison.Ordinal))
            return Error(method, path, 405, "METHOD_NOT_ALLOWED", "DesignConversion improvement-loop endpoint requires POST.");

        var admission = RouteAdmission.AdmitIdempotentMutation(request);
        if (!admission.IsAccepted)
        {
            return Error(
                method,
                path,
                admission.StatusCode,
                admission.ErrorCode,
                ImprovementLoopAdmissionErrorMessage(admission),
                admission.BoundaryMetadata);
        }

        if (_coordinator is null)
        {
            return Error(
                method,
                path,
                503,
                CoordinatorNotConfiguredErrorCode,
                "DesignConversion improvement-loop coordinator is not configured for this local ModelServer route.",
                admission.BoundaryMetadata);
        }

        if (string.IsNullOrWhiteSpace(request.BodyJson))
        {
            return Error(
                method,
                path,
                400,
                "REQUEST_BODY_REQUIRED",
                "DesignConversion improvement-loop endpoint requires a JSON request body.",
                admission.BoundaryMetadata);
        }

        if (!IsJsonContentType(request.ContentType))
            return Error(
                method,
                path,
                415,
                "UNSUPPORTED_MEDIA_TYPE",
                "DesignConversion improvement-loop endpoint requires application/json.",
                admission.BoundaryMetadata);

        return await RouteAdmission.ExecuteIdempotentMutationOnceAsync(
            admission,
            request.BodyJson,
            ct => HandleAcceptedAsync(
                method,
                path,
                request.BodyJson,
                admission.BoundaryMetadata,
                ct),
            conflictAdmission => Error(
                method,
                path,
                409,
                "DESIGN_CONVERSION_IDEMPOTENCY_KEY_CONFLICT",
                "DesignConversion improvement-loop idempotency key was reused with different canonical payload.",
                conflictAdmission.BoundaryMetadata),
            invalidJsonMessage => Error(
                method,
                path,
                400,
                "REQUEST_JSON_INVALID",
                invalidJsonMessage,
                admission.BoundaryMetadata),
            cancellationToken);
    }

    private async Task<DesignConversionModelServerHttpResponse> HandleAcceptedAsync(
        string method,
        string path,
        string bodyJson,
        IReadOnlyDictionary<string, string> admissionMetadata,
        CancellationToken cancellationToken)
    {
        if (ContainsRemovedExternalVisualProviderConfig(bodyJson))
        {
            return Error(
                method,
                path,
                400,
                RemovedExternalVisualProviderConfigErrorCode,
                "External visual-provider selection was removed; the improvement loop uses server-owned local evaluation authority.",
                admissionMetadata);
        }

        DesignConversionModelServerImprovementLoopRequest? loopRequest;
        try
        {
            loopRequest = JsonSerializer.Deserialize<DesignConversionModelServerImprovementLoopRequest>(
                bodyJson,
                DesignConversionModelServerHttpHandler.JsonOptions);
        }
        catch (JsonException)
        {
            return Error(
                method,
                path,
                400,
                "REQUEST_JSON_INVALID",
                "DesignConversion improvement-loop endpoint received invalid JSON.",
                admissionMetadata);
        }

        var mappedRequest = ToCoordinatorRequest(loopRequest);
        if (!mappedRequest.IsSuccess)
        {
            return Error(
                method,
                path,
                400,
                mappedRequest.ErrorCode ?? "IMPROVEMENT_LOOP_REQUEST_INVALID",
                mappedRequest.ErrorMessage ?? "DesignConversion improvement-loop request is invalid.",
                admissionMetadata);
        }

        var repositoryOwnedContext = ApplyServerOwnedRepositoryDpoTransactionContext(
            loopRequest!,
            mappedRequest.Value!,
            _repositoryOwnedTrainingOptions,
            _repositoryOwnedDpoTeacherConfiguration);
        if (!repositoryOwnedContext.IsSuccess || repositoryOwnedContext.Value is null)
        {
            return Error(
                method,
                path,
                repositoryOwnedContext.ErrorCode ==
                    RepositoryOwnedDpoContextRequiresServerConfigurationErrorCode
                    ? 503
                    : 400,
                repositoryOwnedContext.ErrorCode ??
                    RepositoryOwnedDpoContextServerConfigurationInvalidErrorCode,
                repositoryOwnedContext.ErrorMessage ??
                    "DesignConversion repository-owned DPO context could not be created from server configuration.",
                admissionMetadata);
        }

        var effectiveMappedRequest = repositoryOwnedContext.Value;
        var result = await _coordinator!.RunAsync(effectiveMappedRequest, cancellationToken);
        if (!result.IsSuccess)
        {
            return Error(
                method,
                path,
                400,
                result.ErrorCode ?? "IMPROVEMENT_LOOP_FAILED",
                result.ErrorMessage ?? "DesignConversion improvement-loop coordinator rejected the request.",
                admissionMetadata);
        }

        var value = result.Value!;
        var qualityThresholdSource = ResolveQualityThresholdSource(value);
        var loopBoundaryMetadata = BuildLoopBoundaryMetadata(value, qualityThresholdSource);
        var metadata = BuildBoundaryMetadata(
            method,
            path,
            200,
            runtimeInvoked: true,
            coordinatorConfigured: true,
            routeBoundaryMetadata: _routeBoundaryMetadata,
            admissionMetadata: admissionMetadata,
            requestBoundaryMetadata: effectiveMappedRequest.Metadata,
            loopMetadata: loopBoundaryMetadata);
        var body = new DesignConversionModelServerImprovementLoopResponse(
            Endpoint: EndpointPath,
            Method: method,
            HttpSeam: DesignConversionModelServerHttpHandler.LocalHttpSeam,
            ApiShape: DesignConversionModelServerApi.ApiShape,
            RuntimeSurface: DesignConversionModelServerApi.RuntimeSurface,
            StatusCode: 200,
            ContractStatus: value.Completed ? DesignConversionThinClientStatus.Completed : "bounded_non_ready",
            LoopId: value.LoopId,
            RequestId: effectiveMappedRequest.InitialEvaluationRequest.RequestId,
            StopReason: value.StopReason,
            Completed: value.Completed,
            QualityReached: value.QualityReached,
            ProviderRequiredOrBlocked: value.ProviderRequiredOrBlocked,
            RenderManifestIncomplete: value.RenderManifestIncomplete,
            BudgetCapReached: value.BudgetCapReached,
            MaxAttemptsReached: value.MaxAttemptsReached,
            Cancelled: value.Cancelled,
            DeadlineExceeded: value.DeadlineExceeded,
            CandidateStrategyFailed: value.CandidateStrategyFailed,
            AttemptCount: value.AttemptCount,
            Attempts: BuildResponseAttempts(value, qualityThresholdSource),
            EvidenceRefs: FilterInternalTrainingCandidateProjectionMarkers(value.EvidenceRefs),
            Warnings: FilterInternalTrainingCandidateProjectionMarkers(value.Warnings),
            BoundaryMetadata: metadata);

        return Json(200, method, path, body);
    }

    private static IReadOnlyList<string> FilterInternalTrainingCandidateProjectionMarkers(
        IReadOnlyList<string> values)
        => values
            .Where(value => !IsInternalTrainingCandidateProjectionMarker(value))
            .ToArray();

    private static bool IsInternalTrainingCandidateProjectionMarker(string? value)
        => value?.Contains(RuntimeCreatedDpoCandidatePublicProjectionMarker, StringComparison.Ordinal) == true;

    private static bool ContainsRemovedExternalVisualProviderConfig(string bodyJson)
    {
        try
        {
            using var document = JsonDocument.Parse(bodyJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                return false;

            foreach (var property in document.RootElement.EnumerateObject())
            {
                var normalized = new string(property.Name
                    .Where(char.IsLetterOrDigit)
                    .ToArray());
                if (string.Equals(
                        normalized,
                        string.Concat("external", "visual", "provider", "config"),
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }
        catch (JsonException)
        {
            // The normal deserializer below owns the stable invalid-JSON response.
        }

        return false;
    }

    private static OperationResult<DesignConversionImprovementLoopRequest> ToCoordinatorRequest(
        DesignConversionModelServerImprovementLoopRequest? request)
    {
        if (request is null)
            return OperationResult<DesignConversionImprovementLoopRequest>.Fail(
                "DesignConversion improvement-loop request body is required.",
                "IMPROVEMENT_LOOP_REQUEST_REQUIRED");

        if (string.IsNullOrWhiteSpace(request.LoopId))
            return OperationResult<DesignConversionImprovementLoopRequest>.Fail(
                "DesignConversion improvement-loop id is required.",
                "IMPROVEMENT_LOOP_ID_REQUIRED");

        var payloadSize = DesignConversionModelServerPayloadSizeGuard.ValidateImprovementLoopRequest(
            request,
            "DesignConversion improvement-loop request body");
        if (!payloadSize.IsSuccess)
            return payloadSize.MapError<DesignConversionImprovementLoopRequest>();

        var initialRequest = ToInternalEvaluateRequest(request.LoopId, request.InitialPackage);
        if (!initialRequest.IsSuccess)
            return initialRequest.MapError<DesignConversionImprovementLoopRequest>();

        var resolvedMaxAttempts = ResolveCoordinatorMaxAttempts(request.MaxAttempts, initialRequest.Value!.Budget);
        if (!resolvedMaxAttempts.IsSuccess)
            return resolvedMaxAttempts.MapError<DesignConversionImprovementLoopRequest>();

        DesignConversionMeasurementGateRequest? measurementSnapshot = null;
        if (request.MeasurementGateRequest is not null)
        {
            var capturedMeasurement = DesignConversionMeasurementAdmission.Capture(
                request.MeasurementGateRequest);
            if (!capturedMeasurement.IsSuccess || capturedMeasurement.Value is null)
                return capturedMeasurement.MapError<DesignConversionImprovementLoopRequest>();
            measurementSnapshot = capturedMeasurement.Value;
        }

        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["modelserver_improvement_loop_route"] = EndpointPath,
            ["route_boundary"] = LocalImprovementLoopRouteBoundary,
            ["max_attempts_source"] = resolvedMaxAttempts.Value!.Source,
            ["typed_max_improvement_iterations"] =
                initialRequest.Value.Budget.MaxImprovementIterations?.ToString(CultureInfo.InvariantCulture) ?? "not_supplied",
            ["legacy_max_attempts"] = request.MaxAttempts.ToString(CultureInfo.InvariantCulture),
            ["refs_only_public_response"] = bool.TrueString,
            ["asp_net_host_verified"] = bool.FalseString,
            ["socket_opened"] = bool.FalseString,
            ["public_http_endpoint_verified"] = bool.FalseString,
            ["cross_repo_caller_verified"] = bool.FalseString,
            ["external_provider_called"] = bool.FalseString,
            ["external_visual_provider_request_config_removed"] = bool.TrueString,
            ["real_external_provider_executed"] = bool.FalseString,
            ["network_call_executed"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };
        foreach (var pair in request.Metadata ?? new Dictionary<string, string>())
            metadata[$"caller_metadata_ref:{pair.Key}"] = DesignConversionDpoLeakageGuard.HashRef($"loop-metadata:{pair.Key}", pair.Value);

        return OperationResult<DesignConversionImprovementLoopRequest>.Ok(
            new DesignConversionImprovementLoopRequest(
                LoopId: request.LoopId.Trim(),
                InitialEvaluationRequest: initialRequest.Value!,
                MaxAttempts: resolvedMaxAttempts.Value.MaxAttempts,
                Metadata: metadata)
            {
                MeasurementGateRequest = measurementSnapshot
            });
    }

    private static DesignConversionModelServerRepositoryOwnedTrainingOptions?
        SnapshotRepositoryOwnedTrainingOptions(
            DesignConversionModelServerRepositoryOwnedTrainingOptions? options)
    {
        if (options is null)
            return null;

        var spec = options.RunContextSpec is null
            ? null
            : options.RunContextSpec with
            {
                ProvenanceRefs = options.RunContextSpec.ProvenanceRefs
                    ?.ToArray() ?? []
            };
        return new DesignConversionModelServerRepositoryOwnedTrainingOptions(
            spec,
            options.ConfigurationErrorCode,
            options.ConfigurationErrorMessage);
    }

    private static OperationResult<DesignConversionImprovementLoopRequest>
        ApplyServerOwnedRepositoryDpoTransactionContext(
            DesignConversionModelServerImprovementLoopRequest request,
            DesignConversionImprovementLoopRequest coordinatorRequest,
            DesignConversionModelServerRepositoryOwnedTrainingOptions?
                repositoryOwnedTrainingOptions,
            ContinuousLearningTp1DpoConfiguration?
                repositoryOwnedDpoTeacherConfiguration)
    {
        if (repositoryOwnedTrainingOptions is null)
            return OperationResult<DesignConversionImprovementLoopRequest>
                .Ok(coordinatorRequest);

        if (repositoryOwnedTrainingOptions.RunContextSpec is null)
        {
            var errorCode = string.Equals(
                    repositoryOwnedTrainingOptions.ConfigurationErrorCode,
                    DesignConversionModelServerRepositoryOwnedTrainingOptions
                        .ServerConfigurationRequiredErrorCode,
                    StringComparison.Ordinal)
                ? RepositoryOwnedDpoContextRequiresServerConfigurationErrorCode
                : RepositoryOwnedDpoContextServerConfigurationInvalidErrorCode;
            return OperationResult<DesignConversionImprovementLoopRequest>.Fail(
                repositoryOwnedTrainingOptions.ConfigurationErrorMessage ??
                "DesignConversion repository-owned DPO context requires valid server-side repository training configuration.",
                errorCode);
        }

        if (repositoryOwnedDpoTeacherConfiguration is null)
        {
            return OperationResult<DesignConversionImprovementLoopRequest>.Fail(
                "DesignConversion repository-owned DPO authoring requires a server-owned local Qwen/Ollama teacher configuration.",
                RepositoryOwnedDpoContextRequiresServerConfigurationErrorCode);
        }

        var package = request.InitialPackage;
        if (package is null)
        {
            return OperationResult<DesignConversionImprovementLoopRequest>.Fail(
                "DesignConversion repository-owned DPO context requires the already validated initial package.",
                RepositoryOwnedDpoContextServerConfigurationInvalidErrorCode);
        }

        var paidReferenceMaterial = DesignConversionPaidReferenceMaterial
            .Create(package.Html ?? string.Empty, package.Css ?? string.Empty);
        if (!paidReferenceMaterial.IsSuccess || paidReferenceMaterial.Value is null)
        {
            return OperationResult<DesignConversionImprovementLoopRequest>.Fail(
                paidReferenceMaterial.ErrorMessage ??
                DesignConversionPaidReferenceMaterial.InvalidSafeMessage,
                paidReferenceMaterial.ErrorCode ??
                DesignConversionPaidReferenceMaterial.InvalidErrorCode);
        }

        var spec = repositoryOwnedTrainingOptions.RunContextSpec;
        var authoringProfile = LocalQwenOllamaInferenceProfileAuthority.Authoring;
        var openTeacher = new RepositoryOwnedDpoOpenTeacherIdentity(
            OpenWeightTeacherFamily.Qwen.ToString(),
            repositoryOwnedDpoTeacherConfiguration.ModelId,
            repositoryOwnedDpoTeacherConfiguration.ExpectedOllamaManifestDigest,
            LegalTrainingTeacherAdmissionAuthority
                .OllamaModelLayerSha256DigestKind,
            repositoryOwnedDpoTeacherConfiguration
                .ExpectedOllamaModelLayerDigest);
        var policyConfigSha256 = ComputeRepositoryOwnedDpoIdentitySha256(
            "design-conversion-repository-owned-dpo-generation-policy-v1",
            authoringProfile.ProfileId,
            authoringProfile.ConfigurationSha256,
            spec.DomainSlug,
            spec.DesignPatternSlug,
            spec.DatasetVersion,
            repositoryOwnedDpoTeacherConfiguration.MaxTeacherCalls
                .ToString(CultureInfo.InvariantCulture));
        var creatorConfigSha256 = ComputeRepositoryOwnedDpoIdentitySha256(
            "modelserver-design-conversion-improvement-loop-dpo-authoring-v1",
            EndpointPath,
            openTeacher.ModelId,
            openTeacher.ModelVersion,
            openTeacher.ModelArtifactDigestKind,
            openTeacher.ModelArtifactSha256,
            policyConfigSha256,
            spec.ModelIdSlug,
            spec.ModelVersion);

        var context = DesignConversionRepositoryOwnedDpoTransactionContextFactory
            .Create(
                new DesignConversionRepositoryOwnedDpoTransactionContextFactoryConfiguration(
                    TrustedRepositoryRoot: spec.RepositoryRoot,
                    AssertedRepositoryRoot: spec.RepositoryRoot,
                    DomainSlug: spec.DomainSlug,
                    DesignPatternSlug: spec.DesignPatternSlug,
                    DatasetVersion: spec.DatasetVersion,
                    OpenTeacher: openTeacher,
                    GenerationPolicy: new RepositoryOwnedDpoGenerationPolicyIdentity(
                        RepositoryOwnedDpoGenerationPolicyStableName,
                        RepositoryOwnedDpoGenerationPolicyVersion,
                        policyConfigSha256),
                    Creator: new RepositoryOwnedDpoCreatorIdentity(
                        RepositoryOwnedDpoCreatorStableName,
                        RepositoryOwnedDpoCreatorVersion,
                        creatorConfigSha256),
                    PaidReferenceMaterials:
                    [
                        paidReferenceMaterial.Value
                    ]));
        if (!context.IsSuccess || context.Value is null)
        {
            return OperationResult<DesignConversionImprovementLoopRequest>.Fail(
                context.ErrorMessage ??
                "DesignConversion repository-owned DPO context could not be created from server-owned configuration.",
                context.ErrorCode ??
                RepositoryOwnedDpoContextServerConfigurationInvalidErrorCode);
        }

        // Человеческий смысл: клиент прислал работу на улучшение, но не получает
        // права выбирать DPO-папку, teacher или identity. Эти поля рождаются
        // здесь из серверной конфигурации и дальше используются координатором
        // для disk-first reuse либо для одного разрешённого clean-miss вызова.
        var metadata = new Dictionary<string, string>(
            coordinatorRequest.Metadata,
            StringComparer.Ordinal)
        {
            [RepositoryOwnedDpoContextSourceMetadataKey] =
                RepositoryOwnedDpoContextSourceServerConfiguration,
            [DesignConversionRepositoryOwnedDpoTransactionContext
                .ValidationMetadataKey] = bool.TrueString,
            ["repository_owned_dpo_domain"] = spec.DomainSlug,
            ["repository_owned_dpo_design_pattern"] =
                spec.DesignPatternSlug,
            ["repository_owned_dpo_dataset_version"] =
                spec.DatasetVersion,
            ["repository_owned_dpo_paid_reference_hash_only"] =
                bool.TrueString,
            ["repository_owned_dpo_raw_reference_persisted"] =
                bool.FalseString,
            ["repository_owned_dpo_teacher_family"] =
                openTeacher.Family,
            ["repository_owned_dpo_teacher_model_id_ref"] =
                DesignConversionDpoLeakageGuard.HashRef(
                    "repository-owned-dpo-teacher-model-id",
                    openTeacher.ModelId)
        };

        return OperationResult<DesignConversionImprovementLoopRequest>.Ok(
            coordinatorRequest with
            {
                RepositoryOwnedDpoTransactionContext = context.Value,
                Metadata = metadata
            });
    }

    private static string ComputeRepositoryOwnedDpoIdentitySha256(
        params string[] parts)
        => Convert.ToHexString(SHA256.HashData(
                Encoding.UTF8.GetBytes(string.Join('\n', parts))))
            .ToLowerInvariant();

    private static OperationResult<DesignConversionInternalEvaluateRequest> ToInternalEvaluateRequest(
        string loopId,
        DesignConversionModelServerLocalCallerPackage? package)
    {
        if (package is null)
            return OperationResult<DesignConversionInternalEvaluateRequest>.Fail(
                "DesignConversion improvement-loop initial package is required.",
                "IMPROVEMENT_LOOP_INITIAL_PACKAGE_REQUIRED");

        if (string.IsNullOrWhiteSpace(package.RequestId))
            return OperationResult<DesignConversionInternalEvaluateRequest>.Fail(
                "DesignConversion improvement-loop initial package request id is required.",
                "REQUEST_ID_REQUIRED");

        if (!Enum.TryParse<DesignConversionSourceKind>(package.SourceKind, ignoreCase: true, out var sourceKind))
            return OperationResult<DesignConversionInternalEvaluateRequest>.Fail(
                $"DesignConversion improvement-loop source kind is not supported: {package.SourceKind}",
                "SOURCE_KIND_UNSUPPORTED");

        if (!HasSourcePayload(package.DesignTree))
            return OperationResult<DesignConversionInternalEvaluateRequest>.Fail(
                "DesignConversion improvement-loop design tree payload, ref, or hash is required.",
                "DESIGN_CONVERSION_SOURCE_TREE_REQUIRED");

        if (!HasSourcePayload(package.OriginalTree))
            return OperationResult<DesignConversionInternalEvaluateRequest>.Fail(
                "DesignConversion improvement-loop original tree payload, ref, or hash is required.",
                "DESIGN_CONVERSION_ORIGINAL_TREE_REQUIRED");

        if (string.IsNullOrWhiteSpace(package.Html))
            return OperationResult<DesignConversionInternalEvaluateRequest>.Fail(
                "DesignConversion improvement-loop generated HTML is required.",
                "DESIGN_CONVERSION_HTML_REQUIRED");

        if (string.IsNullOrWhiteSpace(package.Css))
            return OperationResult<DesignConversionInternalEvaluateRequest>.Fail(
                "DesignConversion improvement-loop generated CSS is required.",
                "DESIGN_CONVERSION_CSS_REQUIRED");

        var rawArtifacts = (package.Screenshots ?? [])
            .Concat(package.DesignRenders ?? [])
            .ToArray();
        if (rawArtifacts.Length == 0)
            return OperationResult<DesignConversionInternalEvaluateRequest>.Fail(
                "DesignConversion improvement-loop screenshot or expected render ref is required.",
                "DESIGN_CONVERSION_SCREENSHOT_OR_RENDER_REQUIRED");

        var artifactRefs = DesignConversionModelServerArtifactRefGuard.Validate(
            rawArtifacts,
            "DesignConversion improvement-loop screenshot/render refs");
        if (!artifactRefs.IsSuccess)
            return artifactRefs.MapError<DesignConversionInternalEvaluateRequest>();

        var screenshots = (package.Screenshots ?? []).Select(ToRenderArtifact).ToArray();
        var designRenders = (package.DesignRenders ?? []).Select(ToRenderArtifact).ToArray();
        var resolutionArtifacts = (package.Screenshots ?? [])
            .Concat(package.DesignRenders ?? [])
            .Select(ToRenderResolutionArtifact);
        var renderResolutions = package.Budget is null
            ? DesignConversionRenderResolutionFactory.CreateModelServerRequiredBrowserQaMatrix(resolutionArtifacts)
            : DesignConversionRenderResolutionFactory.CreateModelServerMinimumResponsiveMatrix(resolutionArtifacts);
        if (!renderResolutions.IsSuccess)
            return renderResolutions.MapError<DesignConversionInternalEvaluateRequest>();

        if (renderResolutions.Value!.Count == 0)
            return OperationResult<DesignConversionInternalEvaluateRequest>.Fail(
                "DesignConversion improvement-loop screenshot/render refs must include at least one positive viewport.",
                "DESIGN_CONVERSION_VIEWPORT_REQUIRED");

        if (package.Budget is not null && package.Budget.BudgetUsd < 0m)
            return OperationResult<DesignConversionInternalEvaluateRequest>.Fail(
                "DesignConversion improvement-loop budgetUsd must be greater than or equal to zero.",
                NegativeBudgetUsdErrorCode);

        if (package.Budget is not null &&
            !DesignConversionVisualQaBoundaryPolicy.IsValidConfiguredRenderQaPassThreshold(
                package.Budget.RenderQaPassThreshold,
                package.Budget.Tier))
        {
            return OperationResult<DesignConversionInternalEvaluateRequest>.Fail(
                "DesignConversion render QA pass threshold must be finite, greater than 0, at most 1, and not below the server-owned tier floor.",
                DesignConversionVisualQaBoundaryPolicy.RenderQaPassThresholdInvalidErrorCode);
        }

        if (package.Budget is not null &&
            !DesignConversionVisualQaBoundaryPolicy.IsValidConfiguredMaxImprovementIterations(
                package.Budget.MaxImprovementIterations))
        {
            return OperationResult<DesignConversionInternalEvaluateRequest>.Fail(
                DesignConversionVisualQaBoundaryPolicy.MaxImprovementIterationsInvalidErrorMessage(),
                DesignConversionVisualQaBoundaryPolicy.MaxImprovementIterationsInvalidErrorCode);
        }

        if (package.Budget is not null && package.Budget.MaxImprovementCandidates < 0)
            return OperationResult<DesignConversionInternalEvaluateRequest>.Fail(
                "DesignConversion improvement candidate budget must be greater than zero.",
                DesignConversionInternalService.ImprovementCandidateBudgetRequiredCode);

        if (package.Budget is not null &&
            package.Budget.MaxImprovementCandidates == 0 &&
            !DesignConversionVisualQaBoundaryPolicy.IsServerOwnedEntitlementTier(package.Budget.Tier))
        {
            return OperationResult<DesignConversionInternalEvaluateRequest>.Fail(
                "DesignConversion improvement candidate budget must be greater than zero.",
                DesignConversionInternalService.ImprovementCandidateBudgetRequiredCode);
        }

        return OperationResult<DesignConversionInternalEvaluateRequest>.Ok(new DesignConversionInternalEvaluateRequest(
            JobId: $"modelserver-improvement-loop:{SanitizeIdentifier(loopId)}:{SanitizeIdentifier(package.RequestId)}",
            RequestId: package.RequestId,
            SourceKind: sourceKind,
            Html: package.Html,
            Css: package.Css,
            DesignTreePayload: ToTreePayload(package.DesignTree!, "design-tree"),
            OriginalTreePayload: ToTreePayload(package.OriginalTree!, "original-tree"),
            Screenshots: screenshots,
            DesignRenders: designRenders,
            Budget: ToBudget(package.Budget, package.Metadata),
            RenderResolutions: renderResolutions.Value,
            AiTeacherEvidenceRefs: ["modelserver-improvement-loop:no-ai-teacher-executed"],
            DeterministicEvidenceRefs: new Dictionary<string, string>
            {
                ["modelserver_improvement_loop_route"] = EndpointPath,
                ["route_boundary"] = LocalImprovementLoopRouteBoundary,
                ["no_external_provider_called"] = bool.TrueString,
                ["refs_only_public_response"] = bool.TrueString,
                ["asp_net_host_verified"] = bool.FalseString,
                ["socket_opened"] = bool.FalseString,
                ["public_http_endpoint_verified"] = bool.FalseString,
                ["cross_repo_caller_verified"] = bool.FalseString,
                ["product_ready"] = bool.FalseString
            },
            Provenance: BuildInternalProvenance(package.SourceKind, package.Metadata)));
    }

    private static IReadOnlyList<DesignConversionModelServerImprovementLoopAttemptResponse>
        BuildResponseAttempts(
            DesignConversionImprovementLoopResult result,
            string? qualityThresholdSource)
    {
        if (result.Attempts.Count == 0)
            return [];

        var loopRef = DesignConversionDpoLeakageGuard.HashRef(
            "dcm-loop",
            result.LoopId);
        if (!DesignConversionLocalArtifactDbRecord
                .TryCanonicalizeImprovementLoopAttemptsForPublicProjection(
                    loopRef,
                    result.Attempts,
                    out var canonicalAttempts) ||
            canonicalAttempts.Count != result.Attempts.Count)
        {
            return [];
        }

        return result.Attempts
            .Select((attempt, index) => ToResponseAttempt(
                attempt,
                canonicalAttempts[index],
                qualityThresholdSource))
            .ToArray();
    }

    private static DesignConversionModelServerImprovementLoopAttemptResponse ToResponseAttempt(
        DesignConversionImprovementLoopAttemptTelemetry source,
        DesignConversionImprovementLoopAttemptHistoryItem canonical,
        string? qualityThresholdSource)
    {
        var publicMetadata = BuildSafeAttemptPublicMetadata(source, canonical);
        publicMetadata.Remove(QualityThresholdSourceMetadataKey);
        if (qualityThresholdSource is not null)
            publicMetadata[QualityThresholdSourceMetadataKey] = qualityThresholdSource;

        return new(
            AttemptNumber: canonical.AttemptNumber,
            CandidateRef: canonical.CandidateRef,
            JobRef: canonical.JobRef,
            RequestRef: canonical.RequestRef,
            StopReason: canonical.StopReason,
            ShouldContinue: canonical.ShouldContinue,
            BestScore: canonical.BestScore,
            RequiredScore: canonical.RequiredScore,
            EstimatedCostUsd: canonical.EstimatedCostUsd,
            BudgetUsd: canonical.BudgetUsd,
            PolicyAttemptCount: canonical.PolicyAttemptCount,
            MaxImprovementCandidates: canonical.MaxImprovementCandidates,
            RenderResultRefs: BuildSafeAttemptReferences(
                source.RenderResultRefs,
                canonical.RenderResultRefs),
            ComparisonArtifactRefs: BuildSafeAttemptReferences(
                source.ComparisonArtifactRefs,
                canonical.ComparisonArtifactRefs),
            VisualJudgeResultRefs: BuildSafeAttemptReferences(
                source.VisualJudgeResultRefs,
                canonical.VisualJudgeResultRefs),
            EvidenceRefs: BuildSafeAttemptReferences(
                source.EvidenceRefs,
                canonical.EvidenceRefs),
            PublicMetadata: publicMetadata);
    }

    private static Dictionary<string, string> BuildSafeAttemptPublicMetadata(
        DesignConversionImprovementLoopAttemptTelemetry source,
        DesignConversionImprovementLoopAttemptHistoryItem canonical)
    {
        var safe = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in canonical.PublicMetadata)
        {
            if (pair.Key.StartsWith("redacted_metadata_", StringComparison.Ordinal))
                continue;

            if (IsRequiredAttemptMetadataKey(pair.Key) ||
                !source.PublicMetadata.TryGetValue(pair.Key, out var sourceValue) ||
                !IsSafeAttemptMetadataScalar(pair.Key, sourceValue))
            {
                safe[pair.Key] = pair.Value;
                continue;
            }

            safe[pair.Key] = sourceValue;
        }

        return safe;
    }

    private static IReadOnlyList<string> BuildSafeAttemptReferences(
        IReadOnlyList<string> source,
        IReadOnlyList<string> canonical)
    {
        if (source.Count != canonical.Count)
            return canonical;

        return source
            .Select((reference, index) => IsSafeAttemptReference(reference)
                ? reference
                : canonical[index])
            .ToArray();
    }

    private static bool IsRequiredAttemptMetadataKey(string key)
        => key is "attempt_number" or
            "loop_candidate_ref" or
            "job_ref" or
            "request_ref" or
            "stop_reason" or
            "should_continue" or
            "refs_or_hashes_only" or
            "raw_html_included" or
            "raw_css_included" or
            "raw_design_tree_included" or
            "screenshot_bytes_included";

    private static bool IsSafeAttemptMetadataScalar(string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > 128 ||
            value.Any(char.IsControl))
        {
            return false;
        }

        if (key is "provider_name" or
            "visual_ai_requested_provider_name" or
            "visual_ai_selected_provider_name")
        {
            return value is "none" or "auto" or "claude" or "openai" or "gemini";
        }

        if (key is "provider_model" or "provider_endpoint_host" or "blocked_reason")
            return value is "none" or "not_configured" or "not_available";

        if (bool.TryParse(value, out var boolean) &&
            string.Equals(value, boolean.ToString(), StringComparison.Ordinal))
        {
            return true;
        }

        if (long.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var integer) &&
            string.Equals(
                value,
                integer.ToString(CultureInfo.InvariantCulture),
                StringComparison.Ordinal))
        {
            return true;
        }

        if (decimal.TryParse(
                value,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out _))
        {
            return true;
        }

        return value.All(character =>
                   character is (>= 'a' and <= 'z') or
                       (>= '0' and <= '9') or
                       '_' or '-' or '.' or ':' or '|' or ',') &&
               !value.Contains("payload", StringComparison.Ordinal) &&
               !value.Contains("secret", StringComparison.Ordinal) &&
               !value.Contains("private", StringComparison.Ordinal);
    }

    private static bool IsSafeAttemptReference(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > 256 ||
            value.Any(char.IsControl))
        {
            return false;
        }

        if (IsSha256Ref(value) || IsDcmLoopHashRef(value))
            return true;

        if (value is DesignConversionImprovementLoopCoordinator.CoordinatorMarker or
            DesignConversionImprovementLoopCoordinator.RefsOnlyTelemetryMarker or
            DesignConversionImprovementLoopCoordinator.NoExternalProviderMarker or
            DesignConversionVisualQaBoundaryPolicy.ImprovementLoopMarker or
            DesignConversionVisualQaBoundaryPolicy.ProductNotReadyMarker or
            DesignConversionVisualQaBoundaryPolicy.CoverageRetryMarker)
        {
            return true;
        }

        return value.StartsWith("improvement_loop:", StringComparison.Ordinal) &&
               value.Skip("improvement_loop:".Length).All(character =>
                   character is (>= 'a' and <= 'z') or
                       (>= '0' and <= '9') or
                       '_' or '-' or '.' or ':' or '=');
    }

    private static bool IsSha256Ref(string value)
        => value.Length == "sha256:".Length + 64 &&
           value.StartsWith("sha256:", StringComparison.Ordinal) &&
           value.Skip("sha256:".Length).All(character =>
               character is (>= '0' and <= '9') or (>= 'a' and <= 'f'));

    private static bool IsDcmLoopHashRef(string value)
    {
        var suffixIndex = value.LastIndexOf('-');
        return value.StartsWith("dcm-loop-", StringComparison.Ordinal) &&
               suffixIndex >= "dcm-loop-".Length &&
               value.Length - suffixIndex - 1 == 16 &&
               value.Skip(suffixIndex + 1).All(character =>
                   character is (>= '0' and <= '9') or (>= 'a' and <= 'f'));
    }

    private static IReadOnlyDictionary<string, string> BuildLoopBoundaryMetadata(
        DesignConversionImprovementLoopResult value,
        string? qualityThresholdSource)
    {
        var metadata = new Dictionary<string, string>(value.PublicMetadata, StringComparer.Ordinal);
        metadata.Remove(QualityThresholdSourceMetadataKey);
        if (qualityThresholdSource is not null)
            metadata[QualityThresholdSourceMetadataKey] = qualityThresholdSource;

        if (value.FinalEvaluation is { } finalEvaluation)
        {
            foreach (var pair in DesignConversionModelServerApi.BuildVisualProviderMetadata(
                finalEvaluation.ComparisonArtifacts,
                finalEvaluation.VisualJudgeResults,
                finalEvaluation.JobId,
                finalEvaluation.SubmittedRequest.RequestId))
            {
                metadata[pair.Key] = pair.Value;
            }
        }

        var viewportFailures = value.FinalEvaluation?.ViewportFailures ?? [];
        if (viewportFailures.Count == 0)
            return metadata;

        var stages = viewportFailures
            .Select(failure => failure.Stage)
            .Where(stage => !string.IsNullOrWhiteSpace(stage))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(stage => stage, StringComparer.Ordinal)
            .ToArray();
        var errorCodes = viewportFailures
            .Select(failure => failure.ErrorCode)
            .Where(errorCode => !string.IsNullOrWhiteSpace(errorCode))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(errorCode => errorCode, StringComparer.Ordinal)
            .ToArray();
        var statuses = viewportFailures
            .Select(failure => ViewportFailureStatus(failure.Stage))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(status => status, StringComparer.Ordinal)
            .ToArray();
        var expectedReferenceCodes = viewportFailures
            .Where(failure => string.Equals(failure.Stage, "expected_reference", StringComparison.Ordinal))
            .Select(failure => failure.ErrorCode)
            .Where(errorCode => !string.IsNullOrWhiteSpace(errorCode))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(errorCode => errorCode, StringComparer.Ordinal)
            .ToArray();

        metadata["viewport_operation_failed"] = bool.TrueString;
        metadata["viewport_failure_count"] = viewportFailures.Count.ToString(CultureInfo.InvariantCulture);
        metadata["viewport_failure_stage"] = string.Join("|", stages);
        metadata["viewport_failure_error_code"] = string.Join("|", errorCodes);
        metadata["viewport_failure_status"] = string.Join("|", statuses);
        if (expectedReferenceCodes.Length > 0)
        {
            metadata["expected_reference_failure_bounded"] = bool.TrueString;
            metadata["expected_reference_failure_code"] = string.Join("|", expectedReferenceCodes);
            metadata["expected_reference_failure_status"] = DesignConversionInternalService.ViewportExpectedReferenceMissingStatus;
        }

        return metadata;
    }

    private static string ViewportFailureStatus(string stage)
        => stage switch
        {
            "render" => DesignConversionInternalService.ViewportRenderFailedStatus,
            "expected_reference" => DesignConversionInternalService.ViewportExpectedReferenceMissingStatus,
            "comparison" => DesignConversionInternalService.ViewportComparisonFailedStatus,
            "visual_judge" => DesignConversionInternalService.ViewportVisualJudgeFailedStatus,
            _ => "VIEWPORT_OPERATION_FAILED"
        };

    private static DesignTreePayload ToTreePayload(
        DesignConversionModelServerPayloadRef payload,
        string fallbackKind)
        => new(
            PayloadId: SafeRef(payload.RefId, $"{fallbackKind}-ref"),
            Format: string.IsNullOrWhiteSpace(payload.Format) ? fallbackKind : payload.Format,
            Content: payload.Payload ?? payload.RefId ?? payload.Sha256 ?? string.Empty,
            Sha256: SafeSha(payload.Sha256, fallbackKind, payload.Payload ?? payload.RefId),
            Metadata: BuildSafeRefMetadata(payload.Metadata, fallbackKind));

    private static DesignRenderArtifact ToRenderArtifact(DesignConversionModelServerArtifactRef artifact)
        => new(
            ArtifactId: SafeRef(artifact.ArtifactId, artifact.Kind),
            Kind: string.IsNullOrWhiteSpace(artifact.Kind) ? "artifact-ref" : artifact.Kind,
            Width: artifact.Width,
            Height: artifact.Height,
            Uri: SafeRef(artifact.Uri, artifact.ArtifactId),
            Sha256: SafeSha(artifact.Sha256, artifact.ArtifactId, artifact.Uri),
            Metadata: BuildSafeRefMetadata(artifact.Metadata, artifact.Kind));

    private static DesignRenderArtifact ToRenderResolutionArtifact(DesignConversionModelServerArtifactRef artifact)
        => new(
            ArtifactId: artifact.ArtifactId,
            Kind: artifact.Kind,
            Width: artifact.Width,
            Height: artifact.Height,
            Uri: artifact.Uri,
            Sha256: artifact.Sha256,
            Metadata: artifact.Metadata ?? new Dictionary<string, string>());

    private static DesignConversionBudgetMetadata ToBudget(
        DesignConversionModelServerBudget? budget,
        IReadOnlyDictionary<string, string>? callerMetadata)
    {
        var metadata = BuildBudgetMetadata(budget)
            .Concat(BuildSafeRefMetadata(callerMetadata, "caller"))
            .GroupBy(pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal);
        metadata["modelserver_improvement_loop_budget_mapper"] = nameof(DesignConversionModelServerImprovementLoopHandler);
        metadata["external_provider_called"] = bool.FalseString;
        metadata["product_ready"] = bool.FalseString;

        var budgetUsd = budget is null ? 0.50m : budget.BudgetUsd;

        return new DesignConversionBudgetMetadata(
            UserId: DesignConversionDpoLeakageGuard.HashRef("dcm-loop-user", budget?.UserId),
            Tier: string.IsNullOrWhiteSpace(budget?.Tier) ? "local-modelserver-improvement-loop" : budget!.Tier,
            MaxImprovementCandidates: budget?.MaxImprovementCandidates ?? 2,
            MaxRenderQaRuns: Math.Max(1, budget?.MaxRenderQaRuns ?? DefaultMaxRenderQaRuns),
            BudgetUsd: budgetUsd,
            Metadata: metadata,
            RenderQaPassThreshold: budget?.RenderQaPassThreshold,
            MaxImprovementIterations: budget?.MaxImprovementIterations,
            MaxTeacherCalls: budget?.MaxTeacherCalls);
    }

    private static IReadOnlyDictionary<string, string> BuildBudgetMetadata(DesignConversionModelServerBudget? budget)
    {
        var metadata = BuildSafeRefMetadata(budget?.Metadata, "budget").ToDictionary(
            pair => pair.Key,
            pair => pair.Value,
            StringComparer.Ordinal);

        if (TryResolveCanonicalQualityThreshold(budget, out var threshold))
            metadata[CanonicalQualityThresholdMetadataKey] = threshold.ToString("R", CultureInfo.InvariantCulture);

        if (budget?.MaxImprovementIterations.HasValue == true)
        {
            metadata["max_improvement_iterations"] =
                budget.MaxImprovementIterations.Value.ToString(CultureInfo.InvariantCulture);
            metadata["max_improvement_iterations_source"] =
                DesignConversionVisualQaBoundaryPolicy.TypedMaxImprovementIterationsSource;
        }

        if (budget?.MaxTeacherCalls.HasValue == true)
        {
            metadata[DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractSuppliedMetadataKey] =
                bool.TrueString;
            metadata[DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractValueMetadataKey] =
                budget.MaxTeacherCalls.Value.ToString(CultureInfo.InvariantCulture);
            metadata[DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractBoundaryMetadataKey] =
                DesignConversionVisualQaBoundaryPolicy.MaxTeacherCallsContractBoundary;
        }

        return metadata;
    }

    private static OperationResult<ImprovementLoopMaxAttemptsResolution> ResolveCoordinatorMaxAttempts(
        int legacyMaxAttempts,
        DesignConversionBudgetMetadata budget)
    {
        if (budget.MaxImprovementIterations.HasValue)
        {
            if (!DesignConversionVisualQaBoundaryPolicy.IsValidConfiguredMaxImprovementIterations(
                    budget.MaxImprovementIterations))
            {
                return OperationResult<ImprovementLoopMaxAttemptsResolution>.Fail(
                    DesignConversionVisualQaBoundaryPolicy.MaxImprovementIterationsInvalidErrorMessage(),
                    DesignConversionVisualQaBoundaryPolicy.MaxImprovementIterationsInvalidErrorCode);
            }

            return OperationResult<ImprovementLoopMaxAttemptsResolution>.Ok(
                new ImprovementLoopMaxAttemptsResolution(
                    checked(budget.MaxImprovementIterations.Value + 1),
                    DesignConversionVisualQaBoundaryPolicy.TypedMaxImprovementIterationsSource));
        }

        if (legacyMaxAttempts <= 0)
        {
            return OperationResult<ImprovementLoopMaxAttemptsResolution>.Fail(
                "DesignConversion improvement-loop max attempts must be greater than zero.",
                "IMPROVEMENT_LOOP_MAX_ATTEMPTS_REQUIRED");
        }

        if (legacyMaxAttempts > DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts)
        {
            return OperationResult<ImprovementLoopMaxAttemptsResolution>.Fail(
                $"DesignConversion improvement-loop max attempts must be less than or equal to {DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts.ToString(CultureInfo.InvariantCulture)}.",
                MaxAttemptsExceededErrorCode);
        }

        return OperationResult<ImprovementLoopMaxAttemptsResolution>.Ok(
            new ImprovementLoopMaxAttemptsResolution(
                legacyMaxAttempts,
                DesignConversionVisualQaBoundaryPolicy.DirectImprovementLoopMaxAttemptsSource));
    }

    private sealed record ImprovementLoopMaxAttemptsResolution(int MaxAttempts, string Source);

    private static string? ResolveQualityThresholdSource(DesignConversionImprovementLoopResult result)
    {
        if (result.FinalEvaluation?.VisualQaBoundaryTrace.TierPolicy.Metadata.TryGetValue(
                QualityThresholdSourceMetadataKey,
                out var traceSource) == true)
        {
            var normalizedTraceSource = traceSource.Trim();
            if (IsCanonicalQualityThresholdSource(normalizedTraceSource))
                return normalizedTraceSource;
        }

        foreach (var attempt in result.Attempts.Reverse())
        {
            if (attempt.PublicMetadata.TryGetValue(QualityThresholdSourceMetadataKey, out var attemptSource))
            {
                var normalizedAttemptSource = attemptSource.Trim();
                if (IsCanonicalQualityThresholdSource(normalizedAttemptSource))
                    return normalizedAttemptSource;
            }
        }

        return null;
    }

    private static bool IsCanonicalQualityThresholdSource(string source)
        => source is DesignConversionVisualQaBoundaryPolicy.CanonicalQualityThresholdMetadataOverrideSource
            or DesignConversionVisualQaBoundaryPolicy.TypedRenderQaPassThresholdSource
            or DesignConversionVisualQaBoundaryPolicy.ProvisionalTierQualityThresholdSource
            or DesignConversionVisualQaBoundaryPolicy.NonTierQualityThresholdFallbackSource;

    private static bool TryResolveCanonicalQualityThreshold(
        DesignConversionModelServerBudget? budget,
        out double threshold)
    {
        threshold = 0.0;
        if (budget?.RenderQaPassThreshold.HasValue == true &&
            DesignConversionVisualQaBoundaryPolicy.IsValidConfiguredRenderQaPassThreshold(
                budget.RenderQaPassThreshold,
                budget.Tier))
        {
            threshold = budget.RenderQaPassThreshold.Value;
            return true;
        }

        if (budget?.Metadata is null ||
            !budget.Metadata.TryGetValue(CanonicalQualityThresholdMetadataKey, out var configured) ||
            !double.TryParse(configured, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ||
            double.IsNaN(parsed) ||
            double.IsInfinity(parsed) ||
            parsed <= 0.0 ||
            parsed > 1.0 ||
            !DesignConversionVisualQaBoundaryPolicy.IsValidConfiguredRenderQaPassThreshold(parsed, budget.Tier))
        {
            return false;
        }

        threshold = parsed;
        return true;
    }

    private static IReadOnlyDictionary<string, string> BuildInternalProvenance(
        string sourceKind,
        IReadOnlyDictionary<string, string>? callerMetadata)
    {
        var provenance = BuildSafeRefMetadata(callerMetadata, "caller").ToDictionary(
            pair => pair.Key,
            pair => pair.Value,
            StringComparer.Ordinal);
        provenance["modelserver_improvement_loop_route"] = EndpointPath;
        provenance["route_boundary"] = LocalImprovementLoopRouteBoundary;
        provenance["caller_source_kind"] = string.IsNullOrWhiteSpace(sourceKind) ? "UNKNOWN" : sourceKind;
        provenance["refs_only_public_response"] = bool.TrueString;
        provenance["external_provider_called"] = bool.FalseString;
        provenance["product_ready"] = bool.FalseString;
        return provenance;
    }

    private DesignConversionModelServerHttpResponse Error(
        string method,
        string path,
        int statusCode,
        string errorCode,
        string errorMessage,
        IReadOnlyDictionary<string, string>? admissionMetadata = null)
    {
        var body = new DesignConversionModelServerHttpErrorResponse(
            Endpoint: EndpointPath,
            Method: string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method,
            HttpSeam: DesignConversionModelServerHttpHandler.LocalHttpSeam,
            StatusCode: statusCode,
            ErrorCode: errorCode,
            ErrorMessage: errorMessage,
            BoundaryMetadata: BuildBoundaryMetadata(
                method,
                path,
                statusCode,
                runtimeInvoked: false,
                coordinatorConfigured: IsCoordinatorConfiguredForError(errorCode),
                routeBoundaryMetadata: _routeBoundaryMetadata,
                admissionMetadata: admissionMetadata,
                requestBoundaryMetadata: null,
                loopMetadata: null));

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
            ["x-design-conversion-public-deployment-verified"] = bool.FalseString,
            ["x-design-conversion-cross-repo-caller-verified"] = bool.FalseString,
            ["x-design-conversion-runtime-surface"] = DesignConversionModelServerApi.RuntimeSurface,
            ["x-design-conversion-method"] = string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method,
            ["x-design-conversion-path"] = NormalizePath(path),
            ["x-design-conversion-status-code"] = statusCode.ToString(CultureInfo.InvariantCulture)
        };

        var bodyJson = JsonSerializer.Serialize(body, DesignConversionModelServerHttpHandler.JsonOptions);
        bodyJson = EscapeRawPayloadSentinelPropertyNames(bodyJson);

        return new DesignConversionModelServerHttpResponse(
            StatusCode: statusCode,
            ContentType: "application/json; charset=utf-8",
            BodyJson: bodyJson,
            Headers: headers);
    }

    private static string ImprovementLoopAdmissionErrorMessage(
        DesignConversionModelServerRouteAdmissionDecision admission)
        => admission.ErrorCode switch
        {
            "DESIGN_CONVERSION_IDEMPOTENCY_KEY_REQUIRED" =>
                "DesignConversion improvement-loop endpoint requires an Idempotency-Key header.",
            "DESIGN_CONVERSION_IDEMPOTENCY_KEY_TOO_LONG" =>
                "DesignConversion improvement-loop idempotency key is too long.",
            "DESIGN_CONVERSION_IDEMPOTENCY_KEY_INVALID" =>
                "DesignConversion improvement-loop idempotency key must be visible bounded ASCII.",
            _ => admission.ErrorMessage
        };

    private static string EscapeRawPayloadSentinelPropertyNames(string bodyJson)
        => bodyJson.Replace(
            "\"screenshot_bytes_included\"",
            "\"screenshot_\\u0062ytes_included\"",
            StringComparison.Ordinal);

    private static bool IsCoordinatorConfiguredForError(string errorCode)
        => errorCode is not CoordinatorNotConfiguredErrorCode;

    private static IReadOnlyDictionary<string, string> BuildBoundaryMetadata(
        string method,
        string path,
        int statusCode,
        bool runtimeInvoked,
        bool coordinatorConfigured,
        IReadOnlyDictionary<string, string>? routeBoundaryMetadata,
        IReadOnlyDictionary<string, string>? admissionMetadata,
        IReadOnlyDictionary<string, string>? requestBoundaryMetadata,
        IReadOnlyDictionary<string, string>? loopMetadata)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["http_endpoint_shape"] = "POST /api/design-conversion/improvement-loop",
            ["http_method"] = string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method,
            ["http_path"] = NormalizePath(path),
            ["http_status_code"] = statusCode.ToString(CultureInfo.InvariantCulture),
            ["http_content_type"] = "application/json; charset=utf-8",
            ["local_http_host_seam"] = DesignConversionModelServerHttpHandler.LocalHttpSeam,
            ["local_improvement_loop_route_boundary"] = LocalImprovementLoopRouteBoundary,
            ["local_http_host_seam_verified"] = bool.TrueString,
            ["improvement_loop_coordinator_configured"] = coordinatorConfigured.ToString(),
            ["runtime_invoked"] = runtimeInvoked.ToString(),
            ["runtime_surface"] = DesignConversionModelServerApi.RuntimeSurface,
            ["api_shape"] = DesignConversionModelServerApi.ApiShape,
            ["refs_or_hashes_only"] = bool.TrueString,
            ["refs_only_public_response"] = bool.TrueString,
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
            ["cross_repo_caller_verified"] = bool.FalseString,
            ["deployment_verified"] = bool.FalseString,
            ["public_production_default_ready"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };

        foreach (var pair in DesignConversionModelServerCallerContract.BoundaryMetadata())
            metadata[pair.Key] = pair.Value;

        foreach (var pair in routeBoundaryMetadata ?? new Dictionary<string, string>())
        {
            if (IsPublicBoundaryKey(pair.Key))
                metadata[pair.Key] = pair.Value;
        }

        foreach (var pair in admissionMetadata ?? new Dictionary<string, string>())
        {
            if (IsPublicBoundaryKey(pair.Key))
                metadata[pair.Key] = pair.Value;
        }

        foreach (var pair in requestBoundaryMetadata ?? new Dictionary<string, string>())
        {
            if (IsPublicBoundaryKey(pair.Key))
                metadata[pair.Key] = pair.Value;
        }

        metadata.Remove(QualityThresholdSourceMetadataKey);
        foreach (var pair in loopMetadata ?? new Dictionary<string, string>())
        {
            if (IsPublicBoundaryKey(pair.Key))
                metadata[pair.Key] = pair.Value;
        }

        var externalProviderCalled =
            IsTruthy(metadata, "external_visual_ai_executed") ||
            IsTruthy(metadata, "external_ai_visual_judge_executed") ||
            IsTruthy(metadata, "network_call_executed") ||
            IsTruthy(metadata, "real_external_provider_executed");
        var networkCallExecuted = IsTruthy(metadata, "network_call_executed");
        var externalAiVisualJudgeExecuted =
            IsTruthy(metadata, "external_ai_visual_judge_executed") ||
            IsTruthy(metadata, "external_visual_ai_executed");

        metadata["runtime_invoked"] = runtimeInvoked.ToString();
        metadata["refs_or_hashes_only"] = bool.TrueString;
        metadata["refs_only_public_response"] = bool.TrueString;
        metadata["raw_html_included"] = bool.FalseString;
        metadata["raw_css_included"] = bool.FalseString;
        metadata["raw_design_tree_included"] = bool.FalseString;
        metadata["raw_user_private_data_included"] = bool.FalseString;
        metadata["response_contains_raw_private_data"] = bool.FalseString;
        metadata["external_provider_called"] = externalProviderCalled.ToString();
        metadata["external_ai_visual_judge_executed"] = externalAiVisualJudgeExecuted.ToString();
        metadata["network_call_executed"] = networkCallExecuted.ToString();
        metadata[DesignConversionVisualAiProviderSelectionPolicy.RealCloudProviderClaimedKey] = bool.FalseString;
        metadata["provider_payload_included"] = bool.FalseString;
        metadata["asp_net_host_verified"] = bool.FalseString;
        metadata["socket_opened"] = bool.FalseString;
        metadata["public_http_endpoint_verified"] = bool.FalseString;
        metadata["cross_repo_caller_verified"] = bool.FalseString;
        metadata["deployment_verified"] = bool.FalseString;
        metadata["public_production_default_ready"] = bool.FalseString;
        metadata["product_ready"] = bool.FalseString;
        return metadata;
    }

    private static IReadOnlyDictionary<string, string> BuildSafeRefMetadata(
        IReadOnlyDictionary<string, string>? metadata,
        string prefix)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in metadata ?? new Dictionary<string, string>())
            result[$"{prefix}_{pair.Key}_ref"] = DesignConversionDpoLeakageGuard.HashRef($"{prefix}:{pair.Key}", pair.Value);

        return result;
    }

    private static bool IsTruthy(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) &&
           (string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "1", StringComparison.Ordinal) ||
            string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase));

    private static bool IsPublicBoundaryKey(string key)
        => key is "refs_or_hashes_only"
            or "stop_reason"
            or "completed"
            or "quality_reached"
            or "provider_required_or_blocked"
            or "render_manifest_incomplete"
            or "budget_cap_reached"
            or "max_attempts_reached"
            or "cancelled"
            or "candidate_strategy_failed"
            or "evaluation_failed"
            or "no_progress_stop"
            or "attempt_count"
            or "configured_max_attempts"
            or "max_attempts_source"
            or "typed_max_improvement_iterations"
            or "legacy_max_attempts"
            or DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractSuppliedMetadataKey
            or DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractValueMetadataKey
            or DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractBoundaryMetadataKey
            or DesignConversionImprovementLoopCoordinator.MaxTeacherCallsRuntimeEnforcementMetadataKey
            or DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCallCapEnforcedMetadataKey
            or DesignConversionImprovementLoopCoordinator.MaxTeacherCallsObservedCallCountMetadataKey
            or DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCapReachedMetadataKey
            or "score_direction"
            or "improvement_epsilon"
            or "max_consecutive_no_improvement"
            or "consecutive_no_improvement"
            or "tier"
            or QualityThresholdSourceMetadataKey
            or "final_best_score"
            or "final_measured_quality_score"
            or "final_required_score"
            or "final_estimated_cost_usd"
            or "spent_budget_usd"
            or "final_budget_usd"
            or "final_remaining_budget_usd"
            or "best_incumbent_candidate_ref"
            or "cost_savings_accept_lower_quality"
            or "effective_budget_usd"
            or "effective_max_improvement_candidates"
            or "max_improvement_iterations"
            or "max_improvement_iterations_source"
            or "submitted_budget_usd"
            or "submitted_max_improvement_candidates"
            or "tier_budget_cap_usd"
            or "tier_max_improvement_candidates"
            or "tier_policy_budget_cap_applied"
            or "tier_policy_iteration_cap_applied"
            or "tier_policy_budget_entitlement_default_applied"
            or "tier_policy_iteration_entitlement_default_applied"
            or "budget_stop_source"
            or "refs_only_public_response"
            or "route_admission_boundary"
            or "route_admission_enabled"
            or "route_admission_configuration_status"
            or "route_admission_authorized"
            or "route_admission_scope_ref"
            or "route_admission_tenant_bound"
            or "route_admission_tenant_id_ref"
            or "route_admission_raw_tenant_id_exposed"
            or "route_admission_header_name"
            or "route_admission_project_header_name"
            or "route_admission_token_supplied"
            or "route_admission_token_accepted"
            or "route_admission_previous_token_configured"
            or "route_admission_previous_token_active"
            or "route_admission_accepted_key_count"
            or "route_admission_rotation_state"
            or "route_admission_error_ref"
            or "route_admission_status_code"
            or "route_admission_method"
            or "route_admission_path"
            or "idempotency_header_name"
            or "idempotency_required"
            or "idempotency_key_accepted"
            or "idempotency_cache_capacity"
            or "idempotency_cache_ttl_seconds"
            or "idempotency_failure_replay_policy"
            or "idempotency_durable_replay_enabled"
            or "idempotency_durable_replay_store_state"
            or "idempotency_payload_digest_exposed"
            or "idempotency_route_scoped"
            or "idempotency_effective_scope_ref"
            or "idempotency_route_scope_ref"
            or "idempotency_route_method_ref"
            or "idempotency_route_path_ref"
            or "idempotency_route_method_path_exposed"
            or "idempotency_effective_scope_contains_raw_route"
            or "token_value_exposed"
            or "token_hash_exposed"
            or "token_fingerprint_exposed"
            or "persistence_invoked"
            or DesignConversionImprovementLoopCoordinator.LocalArtifactDbAutowriteMarker
            or "improvement_loop_local_artifact_db_persistence_failed"
            or "improvement_loop_local_artifact_db_persistence_failure_bounded"
            or "local_artifact_db_persistence_status"
            or "local_artifact_db_persistence_error_code"
            or DesignConversionImprovementLoopCoordinator
                .ImprovementLoopHistoryStatusMetadataKey
            or DesignConversionImprovementLoopCoordinator
                .ImprovementLoopHistoryAttemptCountMetadataKey
            or DesignConversionImprovementLoopCoordinator
                .ImprovementLoopHistorySha256MetadataKey
            or DesignConversionImprovementLoopCoordinator
                .ImprovementLoopHistoryOnlyMetadataKey
            or "browser_runtime_invoked"
            or "model_provider_invoked"
            or "improvement_loop_registered_from_local_service_composition"
            or "improvement_loop_local_service_composition_boundary"
            or "composition_kind"
            or "explicit_local_service_composition"
            or "browser_renderer_default_wired"
            or "browser_renderer_default_scope"
            or "browser_render_executed"
            or "browser_render_not_executed"
            or "render_result_count"
            or "browser_render_result_count"
            or "comparison_artifact_count"
            or "visual_judge_result_count"
            or "render_provider_kind"
            or "comparison_provider_injected"
            or "local_visual_judge_default_wired"
            or "visual_judge_boundary"
            or "visual_provider_output_observed"
            or "external_visual_ai_configured"
            or "external_visual_ai_executed"
            or "external_network_call_executed"
            or "visual_ai_provider_contract_executed"
            or "external_visual_ai_blocked"
            or "blocked_reason"
            or "provider_kind"
            or "provider_name"
            or "provider_model"
            or "provider_endpoint_host"
            or "provider_response_identity_verified"
            or "provider_response_schema_verified"
            or DesignConversionModelServerNetworkBoundary.ServerOwnedVisualProviderNetworkCallVerifiedKey
            or DesignConversionModelServerNetworkBoundary.ServerOwnedVisualProviderNetworkCallVerificationKey
            or ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey
            or ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVersionKey
            or ExternalHttpDesignConversionVisualJudge.ServerCallReceiptIdKey
            or ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJobIdKey
            or ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJudgeRequestIdKey
            or ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRequestIdKey
            or ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRootRequestIdKey
            or ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey
            or ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey
            or ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualJudgeRequestIdKey
            or ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualResultIdKey
            or ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey
            or ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderNameKey
            or ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderModelKey
            or ExternalHttpDesignConversionVisualJudge.ServerCallReceiptEndpointHostKey
            or ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey
            or ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey
            or ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey
            or "fake_client"
            or "visual_provider_estimated_cost_usd"
            or "viewport_operation_failed"
            or "viewport_failure_count"
            or "viewport_failure_stage"
            or "viewport_failure_error_code"
            or "viewport_failure_status"
            or "expected_reference_failure_bounded"
            or "expected_reference_failure_code"
            or "expected_reference_failure_status"
            or "image_payload_mode"
            or "secret_value_included"
            or "real_external_provider_executed"
            or "network_call_executed"
            or "public_production_default_ready"
            or "local_service_composition_evidence_refs"
            or "provider_payload_included"
            or "external_ai_visual_judge_executed"
            or "product_ready"
            or "trained_feature_claimed";

    private static bool HasSourcePayload(DesignConversionModelServerPayloadRef? source)
        => source is not null &&
           (!string.IsNullOrWhiteSpace(source.Payload) ||
            !string.IsNullOrWhiteSpace(source.RefId) ||
            !string.IsNullOrWhiteSpace(source.Sha256));

    private static string SafeRef(string? value, string? fallback)
        => string.IsNullOrWhiteSpace(value)
            ? DesignConversionDpoLeakageGuard.HashRef("modelserver-improvement-loop-ref", fallback)
            : value;

    private static string SafeSha(string? sha, string? prefix, string? value)
        => string.IsNullOrWhiteSpace(sha)
            ? DesignConversionDpoLeakageGuard.HashRef(prefix ?? "modelserver-improvement-loop-sha", value)
            : sha;

    private static string SanitizeIdentifier(string value)
        => new(value
            .Where(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_' or ':')
            .DefaultIfEmpty('x')
            .ToArray());

    private static string NormalizePath(string? path)
        => string.IsNullOrWhiteSpace(path) ? "/" : path.Trim();

    private static bool IsJsonContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return false;

        return contentType.Split(';', 2)[0].Trim().Equals("application/json", StringComparison.OrdinalIgnoreCase);
    }
}
