// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Globalization;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;

public sealed record DesignConversionModelServerImprovementLoopRequest(
    string LoopId,
    DesignConversionModelServerLocalCallerPackage? InitialPackage,
    int MaxAttempts,
    IReadOnlyDictionary<string, string>? Metadata = null,
    DesignConversionExternalVisualProviderConfigInput? ExternalVisualProviderConfig = null);

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

public delegate OperationResult<DesignConversionImprovementLoopCoordinator>
    DesignConversionExternalVisualProviderCoordinatorResolver(
        DesignConversionExternalVisualProviderConfigInput config);

public sealed class DesignConversionModelServerImprovementLoopHandler
{
    public const string EndpointPath = "/api/design-conversion/improvement-loop";
    public const string LocalImprovementLoopRouteBoundary =
        "local_improvement_loop_route_refs_only_no_socket_no_public_deployment";
    public const string CoordinatorNotConfiguredErrorCode = "IMPROVEMENT_LOOP_COORDINATOR_NOT_CONFIGURED";
    public const string ExternalVisualProviderCoordinatorResolverNotConfiguredErrorCode =
        "IMPROVEMENT_LOOP_EXTERNAL_VISUAL_PROVIDER_COORDINATOR_RESOLVER_NOT_CONFIGURED";
    public const string ExternalVisualProviderConfigInvalidErrorCode =
        "IMPROVEMENT_LOOP_EXTERNAL_VISUAL_PROVIDER_CONFIG_INVALID";
    public const string ExternalVisualProviderCoordinatorResolveFailedErrorCode =
        "IMPROVEMENT_LOOP_EXTERNAL_VISUAL_PROVIDER_COORDINATOR_RESOLVE_FAILED";
    public const string MaxAttemptsExceededErrorCode = "IMPROVEMENT_LOOP_MAX_ATTEMPTS_EXCEEDED";
    public const string NegativeBudgetUsdErrorCode = DesignConversionInternalService.NegativeBudgetUsdErrorCode;
    private const string CanonicalQualityThresholdMetadataKey = "quality_threshold";
    private const int DefaultMaxRenderQaRuns = 2;

    private readonly DesignConversionImprovementLoopCoordinator? _coordinator;
    private readonly DesignConversionExternalVisualProviderCoordinatorResolver? _externalVisualProviderCoordinatorResolver;
    private readonly IReadOnlyDictionary<string, string> _routeBoundaryMetadata;

    public DesignConversionModelServerImprovementLoopHandler(
        DesignConversionImprovementLoopCoordinator? coordinator,
        IReadOnlyDictionary<string, string>? routeBoundaryMetadata = null,
        DesignConversionModelServerRouteAdmission? routeAdmission = null,
        DesignConversionExternalVisualProviderCoordinatorResolver? externalVisualProviderCoordinatorResolver = null)
    {
        _coordinator = coordinator;
        _externalVisualProviderCoordinatorResolver = externalVisualProviderCoordinatorResolver;
        _routeBoundaryMetadata = routeBoundaryMetadata is null
            ? new Dictionary<string, string>(StringComparer.Ordinal)
            : new Dictionary<string, string>(routeBoundaryMetadata, StringComparer.Ordinal);
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

        if (_coordinator is null && _externalVisualProviderCoordinatorResolver is null)
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
            ct => HandleAcceptedAsync(method, path, request.BodyJson, admission.BoundaryMetadata, ct),
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

        var externalArtifactProof = await ProveExternalVisualProviderArtifactsAsync(
            loopRequest,
            mappedRequest.Value!,
            cancellationToken);
        if (!externalArtifactProof.IsSuccess)
        {
            return Error(
                method,
                path,
                400,
                externalArtifactProof.ErrorCode ?? DesignConversionModelServerArtifactRefGuard.ErrorCode,
                externalArtifactProof.ErrorMessage ?? ExternalVisualProviderArtifactProofRequiredMessage,
                admissionMetadata);
        }

        var coordinator = ResolveCoordinator(loopRequest);
        if (!coordinator.IsSuccess)
        {
            return Error(
                method,
                path,
                CoordinatorErrorStatusCode(coordinator.ErrorCode),
                coordinator.ErrorCode ?? ExternalVisualProviderCoordinatorResolveFailedErrorCode,
                coordinator.ErrorMessage ?? "DesignConversion improvement-loop coordinator could not be resolved.",
                admissionMetadata);
        }

        var result = await coordinator.Value!.RunAsync(mappedRequest.Value!, cancellationToken);
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
        var loopBoundaryMetadata = BuildLoopBoundaryMetadata(value);
        var metadata = BuildBoundaryMetadata(
            method,
            path,
            200,
            runtimeInvoked: true,
            coordinatorConfigured: true,
            routeBoundaryMetadata: _routeBoundaryMetadata,
            admissionMetadata: admissionMetadata,
            requestBoundaryMetadata: mappedRequest.Value!.Metadata,
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
            RequestId: mappedRequest.Value!.InitialEvaluationRequest.RequestId,
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
            Attempts: value.Attempts
                .Select(ToResponseAttempt)
                .ToArray(),
            EvidenceRefs: value.EvidenceRefs,
            Warnings: value.Warnings,
            BoundaryMetadata: metadata);

        return Json(200, method, path, body);
    }

    private const string ExternalVisualProviderArtifactProofRequiredMessage =
        "DesignConversion external visual provider screenshot/render refs must resolve to server-owned local nonblank PNG artifacts before request-scoped provider dispatch.";

    private static async Task<OperationResult<bool>> ProveExternalVisualProviderArtifactsAsync(
        DesignConversionModelServerImprovementLoopRequest? routeRequest,
        DesignConversionImprovementLoopRequest mappedRequest,
        CancellationToken cancellationToken)
    {
        var config = routeRequest?.ExternalVisualProviderConfig;
        if (config is null)
            return OperationResult<bool>.Ok(true);

        var configValidation = DesignConversionExternalVisualProviderConfigIntake.TryCreateOptions(config);
        if (!configValidation.IsSuccess)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion external visual provider config is invalid or incomplete.",
                ExternalVisualProviderConfigInvalidErrorCode);
        }

        IDesignConversionVisualJudgeImageContentResolver imageContentResolver;
        try
        {
            imageContentResolver = new ServerOwnedLocalPngComparisonProvider(
                config.ServerOwnedImageArtifactRoot!,
                maxArtifactBytes: ExternalHttpDesignConversionVisualJudge.MaxInlinePngImageBytes);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or IOException or UnauthorizedAccessException)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion external visual provider config is invalid or incomplete.",
                ExternalVisualProviderConfigInvalidErrorCode);
        }

        var proofImages = ExternalVisualProviderArtifactProofImages(mappedRequest.InitialEvaluationRequest);
        if (!proofImages.IsSuccess)
            return proofImages.MapError<bool>();

        foreach (var image in proofImages.Value!)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var proof = await imageContentResolver.ResolveAsync(
                image,
                image.Label,
                ExternalHttpDesignConversionVisualJudge.MaxInlinePngImageBytes,
                cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (!proof.IsSuccess)
                return ExternalVisualProviderArtifactProofRequired();
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<IReadOnlyList<RenderQaImageRecord>> ExternalVisualProviderArtifactProofImages(
        DesignConversionInternalEvaluateRequest request)
    {
        var images = new List<RenderQaImageRecord>();
        var index = 0;
        foreach (var artifact in request.Screenshots ?? [])
        {
            var image = ToExternalVisualProviderArtifactProofImage(artifact, "screenshot", ++index);
            if (!image.IsSuccess)
                return image.MapError<IReadOnlyList<RenderQaImageRecord>>();
            images.Add(image.Value!);
        }

        foreach (var artifact in request.DesignRenders ?? [])
        {
            var image = ToExternalVisualProviderArtifactProofImage(artifact, "design-render", ++index);
            if (!image.IsSuccess)
                return image.MapError<IReadOnlyList<RenderQaImageRecord>>();
            images.Add(image.Value!);
        }

        return OperationResult<IReadOnlyList<RenderQaImageRecord>>.Ok(images);
    }

    private static OperationResult<RenderQaImageRecord> ToExternalVisualProviderArtifactProofImage(
        DesignRenderArtifact artifact,
        string role,
        int index)
    {
        var resolution = DesignConversionRenderResolutionFactory.CreateGrouped([artifact]);
        if (!resolution.IsSuccess || resolution.Value!.Count == 0)
            return ExternalVisualProviderArtifactProofRequired().MapError<RenderQaImageRecord>();

        return OperationResult<RenderQaImageRecord>.Ok(new RenderQaImageRecord(
            ImageId: $"{artifact.ArtifactId}:external-provider-proof:{index.ToString(CultureInfo.InvariantCulture)}",
            Label: string.IsNullOrWhiteSpace(artifact.Kind) ? role : artifact.Kind,
            Resolution: resolution.Value[0],
            Uri: artifact.Uri,
            Sha256: artifact.Sha256,
            Metadata: artifact.Metadata ?? new Dictionary<string, string>()));
    }

    private static OperationResult<bool> ExternalVisualProviderArtifactProofRequired()
        => OperationResult<bool>.Fail(
            ExternalVisualProviderArtifactProofRequiredMessage,
            DesignConversionModelServerArtifactRefGuard.ErrorCode);

    private OperationResult<DesignConversionImprovementLoopCoordinator> ResolveCoordinator(
        DesignConversionModelServerImprovementLoopRequest? request)
    {
        var config = request?.ExternalVisualProviderConfig;
        if (config is null)
        {
            return _coordinator is null
                ? OperationResult<DesignConversionImprovementLoopCoordinator>.Fail(
                    "DesignConversion improvement-loop coordinator is not configured for this local ModelServer route.",
                    CoordinatorNotConfiguredErrorCode)
                : OperationResult<DesignConversionImprovementLoopCoordinator>.Ok(_coordinator);
        }

        var validation = DesignConversionExternalVisualProviderConfigIntake.TryCreateOptions(config);
        if (!validation.IsSuccess)
        {
            return OperationResult<DesignConversionImprovementLoopCoordinator>.Fail(
                "DesignConversion external visual provider config is invalid or incomplete.",
                ExternalVisualProviderConfigInvalidErrorCode);
        }

        if (_externalVisualProviderCoordinatorResolver is null)
        {
            return OperationResult<DesignConversionImprovementLoopCoordinator>.Fail(
                "DesignConversion external visual provider coordinator resolver is not configured for this local ModelServer route.",
                ExternalVisualProviderCoordinatorResolverNotConfiguredErrorCode);
        }

        try
        {
            var resolved = _externalVisualProviderCoordinatorResolver(config);
            if (!resolved.IsSuccess || resolved.Value is null)
            {
                return OperationResult<DesignConversionImprovementLoopCoordinator>.Fail(
                    "DesignConversion external visual provider coordinator resolver rejected the request.",
                    ExternalVisualProviderCoordinatorResolveFailedErrorCode);
            }

            return resolved;
        }
        catch (Exception)
        {
            return OperationResult<DesignConversionImprovementLoopCoordinator>.Fail(
                "DesignConversion external visual provider coordinator resolver failed.",
                ExternalVisualProviderCoordinatorResolveFailedErrorCode);
        }
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

        if (request.MaxAttempts <= 0)
            return OperationResult<DesignConversionImprovementLoopRequest>.Fail(
                "DesignConversion improvement-loop max attempts must be greater than zero.",
                "IMPROVEMENT_LOOP_MAX_ATTEMPTS_REQUIRED");

        if (request.MaxAttempts > DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts)
            return OperationResult<DesignConversionImprovementLoopRequest>.Fail(
                $"DesignConversion improvement-loop max attempts must be less than or equal to {DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts.ToString(CultureInfo.InvariantCulture)}.",
                MaxAttemptsExceededErrorCode);

        var payloadSize = DesignConversionModelServerPayloadSizeGuard.ValidateImprovementLoopRequest(
            request,
            "DesignConversion improvement-loop request body");
        if (!payloadSize.IsSuccess)
            return payloadSize.MapError<DesignConversionImprovementLoopRequest>();

        var initialRequest = ToInternalEvaluateRequest(request.LoopId, request.InitialPackage);
        if (!initialRequest.IsSuccess)
            return initialRequest.MapError<DesignConversionImprovementLoopRequest>();

        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["modelserver_improvement_loop_route"] = EndpointPath,
            ["route_boundary"] = LocalImprovementLoopRouteBoundary,
            ["refs_only_public_response"] = bool.TrueString,
            ["asp_net_host_verified"] = bool.FalseString,
            ["socket_opened"] = bool.FalseString,
            ["public_http_endpoint_verified"] = bool.FalseString,
            ["cross_repo_caller_verified"] = bool.FalseString,
            ["external_provider_called"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };
        foreach (var pair in request.Metadata ?? new Dictionary<string, string>())
            metadata[$"caller_metadata_ref:{pair.Key}"] = DesignConversionDpoLeakageGuard.HashRef($"loop-metadata:{pair.Key}", pair.Value);

        AddExternalVisualProviderRequestMetadata(metadata, request.ExternalVisualProviderConfig);

        return OperationResult<DesignConversionImprovementLoopRequest>.Ok(new DesignConversionImprovementLoopRequest(
            LoopId: request.LoopId.Trim(),
            InitialEvaluationRequest: initialRequest.Value!,
            MaxAttempts: request.MaxAttempts,
            Metadata: metadata));
    }

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
        var renderResolutions = DesignConversionRenderResolutionFactory.CreateModelServerMinimumResponsiveMatrix(
            (package.Screenshots ?? [])
                .Concat(package.DesignRenders ?? [])
                .Select(ToRenderResolutionArtifact));
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

    private static DesignConversionModelServerImprovementLoopAttemptResponse ToResponseAttempt(
        DesignConversionImprovementLoopAttemptTelemetry attempt)
        => new(
            AttemptNumber: attempt.AttemptNumber,
            CandidateRef: attempt.CandidateRef,
            JobRef: attempt.JobRef,
            RequestRef: attempt.RequestRef,
            StopReason: attempt.StopReason,
            ShouldContinue: attempt.ShouldContinue,
            BestScore: attempt.BestScore,
            RequiredScore: attempt.RequiredScore,
            EstimatedCostUsd: attempt.EstimatedCostUsd,
            BudgetUsd: attempt.BudgetUsd,
            PolicyAttemptCount: attempt.PolicyAttemptCount,
            MaxImprovementCandidates: attempt.MaxImprovementCandidates,
            RenderResultRefs: attempt.RenderResultRefs,
            ComparisonArtifactRefs: attempt.ComparisonArtifactRefs,
            VisualJudgeResultRefs: attempt.VisualJudgeResultRefs,
            EvidenceRefs: attempt.EvidenceRefs,
            PublicMetadata: attempt.PublicMetadata);

    private static IReadOnlyDictionary<string, string> BuildLoopBoundaryMetadata(
        DesignConversionImprovementLoopResult value)
    {
        var metadata = new Dictionary<string, string>(value.PublicMetadata, StringComparer.Ordinal);
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

        var budgetUsd = budget is null
            ? 0.50m
            : budget.BudgetUsd > 0m
                ? budget.BudgetUsd
                : DesignConversionVisualQaBoundaryPolicy.IsServerOwnedEntitlementTier(budget.Tier)
                    ? 0m
                    : 0.50m;

        return new DesignConversionBudgetMetadata(
            UserId: DesignConversionDpoLeakageGuard.HashRef("dcm-loop-user", budget?.UserId),
            Tier: string.IsNullOrWhiteSpace(budget?.Tier) ? "local-modelserver-improvement-loop" : budget!.Tier,
            MaxImprovementCandidates: budget?.MaxImprovementCandidates ?? 2,
            MaxRenderQaRuns: Math.Max(1, budget?.MaxRenderQaRuns ?? DefaultMaxRenderQaRuns),
            BudgetUsd: budgetUsd,
            Metadata: metadata);
    }

    private static IReadOnlyDictionary<string, string> BuildBudgetMetadata(DesignConversionModelServerBudget? budget)
    {
        var metadata = BuildSafeRefMetadata(budget?.Metadata, "budget").ToDictionary(
            pair => pair.Key,
            pair => pair.Value,
            StringComparer.Ordinal);

        if (TryResolveCanonicalQualityThreshold(budget, out var threshold))
            metadata[CanonicalQualityThresholdMetadataKey] = threshold.ToString("R", CultureInfo.InvariantCulture);

        return metadata;
    }

    private static bool TryResolveCanonicalQualityThreshold(
        DesignConversionModelServerBudget? budget,
        out double threshold)
    {
        threshold = 0.0;
        if (budget?.Metadata is null ||
            !budget.Metadata.TryGetValue(CanonicalQualityThresholdMetadataKey, out var configured) ||
            !double.TryParse(configured, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ||
            double.IsNaN(parsed) ||
            double.IsInfinity(parsed) ||
            parsed <= 0.0 ||
            parsed > 1.0 ||
            LowersServerOwnedTierThreshold(budget.Tier, parsed))
        {
            return false;
        }

        threshold = parsed;
        return true;
    }

    private static bool LowersServerOwnedTierThreshold(string? tier, double threshold)
    {
        var serverOwnedThreshold = (tier ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "free" => 0.80,
            "paid" => 0.86,
            "pro" => 0.90,
            _ => (double?)null
        };

        return serverOwnedThreshold.HasValue && threshold < serverOwnedThreshold.Value;
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

    private static int CoordinatorErrorStatusCode(string? errorCode)
        => string.Equals(errorCode, ExternalVisualProviderConfigInvalidErrorCode, StringComparison.Ordinal)
            ? 400
            : 503;

    private static bool IsCoordinatorConfiguredForError(string errorCode)
        => errorCode is not CoordinatorNotConfiguredErrorCode
            and not ExternalVisualProviderCoordinatorResolverNotConfiguredErrorCode
            and not ExternalVisualProviderCoordinatorResolveFailedErrorCode;

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

    private static void AddExternalVisualProviderRequestMetadata(
        IDictionary<string, string> metadata,
        DesignConversionExternalVisualProviderConfigInput? config)
    {
        if (config is null)
        {
            metadata["external_visual_provider_request_config_present"] = bool.FalseString;
            metadata["external_visual_provider_config_complete"] = bool.FalseString;
            metadata["external_visual_provider_factory_ready"] = bool.FalseString;
            metadata["real_external_provider_executable_when_network_authorized"] = bool.FalseString;
            metadata["real_external_provider_executed"] = bool.FalseString;
            metadata["network_call_executed"] = bool.FalseString;
            metadata["provider_payload_included"] = bool.FalseString;
            metadata["external_visual_provider_route_config_boundary"] = "request_config_intake_only_no_live_network";
            DesignConversionVisualAiProviderSelectionPolicy.CopyPublicMetadata(
                metadata,
                DesignConversionVisualAiProviderSelectionPolicy.DescribeNotConfigured(
                    fallbackReason: "request_config_not_present"));
            return;
        }

        var descriptor = DesignConversionExternalVisualProviderConfigIntake.Describe(config);
        metadata["external_visual_provider_config_intake"] = bool.TrueString;
        metadata["external_visual_provider_request_config_present"] = bool.TrueString;
        metadata["external_visual_provider_config_complete"] = descriptor.IsComplete.ToString();
        metadata["external_visual_provider_factory_ready"] = descriptor.FactoryReady.ToString();
        metadata["real_external_provider_executable_when_network_authorized"] =
            descriptor.RealExternalProviderExecutableWhenNetworkAuthorized.ToString();
        metadata["real_external_provider_executed"] = bool.FalseString;
        metadata["network_call_executed"] = bool.FalseString;
        metadata["provider_payload_included"] = bool.FalseString;
        metadata["secret_value_included"] = bool.FalseString;
        metadata["external_visual_provider_api_key_configured"] =
            descriptor.BoundaryMetadata["external_visual_provider_api_key_configured"];
        metadata["external_visual_provider_api_key_value_included"] = bool.FalseString;
        metadata["external_visual_provider_endpoint_host"] = descriptor.EndpointHost;
        metadata["external_visual_provider_endpoint_scheme"] = SafeEndpointScheme(config.Endpoint);
        metadata["external_visual_provider_name"] = descriptor.ProviderName;
        metadata["external_visual_provider_model"] = descriptor.ModelId;
        metadata["external_visual_provider_api_key_header_name"] = descriptor.ApiKeyHeaderName;
        metadata["external_visual_provider_api_key_scheme"] = descriptor.ApiKeyScheme;
        metadata["external_visual_provider_missing_setting_names"] =
            descriptor.MissingSettingNames.Count == 0 ? "none" : string.Join(",", descriptor.MissingSettingNames);
        metadata["external_visual_provider_invalid_setting_names"] =
            descriptor.InvalidSettingNames.Count == 0 ? "none" : string.Join(",", descriptor.InvalidSettingNames);
        metadata["external_visual_provider_config_descriptor_status"] = descriptor.Status;
        metadata["external_visual_provider_route_config_boundary"] = "request_config_intake_only_no_live_network";
        DesignConversionVisualAiProviderSelectionPolicy.CopyPublicMetadata(metadata, descriptor.BoundaryMetadata);
    }

    private static string SafeEndpointScheme(string? endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            return "missing";

        return Uri.TryCreate(endpoint.Trim(), UriKind.Absolute, out var uri) &&
            !string.IsNullOrWhiteSpace(uri.Scheme)
                ? uri.Scheme
                : "invalid";
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
            or "score_direction"
            or "improvement_epsilon"
            or "max_consecutive_no_improvement"
            or "consecutive_no_improvement"
            or "tier"
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
            or "external_visual_provider_config_intake"
            or "external_visual_provider_request_config_present"
            or "external_visual_provider_config_complete"
            or "external_visual_provider_factory_ready"
            or "external_visual_provider_request_config_supported"
            or "external_visual_provider_request_scoped_resolver_configured"
            or "external_visual_provider_shared_http_client_injected"
            or "external_visual_provider_http_client_owned_by_caller"
            or "external_visual_provider_http_client_disposed_by_registry"
            or "real_external_provider_executable_when_network_authorized"
            or "external_visual_provider_api_key_configured"
            or "external_visual_provider_api_key_value_included"
            or "external_visual_provider_endpoint_host"
            or "external_visual_provider_endpoint_scheme"
            or "external_visual_provider_name"
            or "external_visual_provider_model"
            or "external_visual_provider_api_key_header_name"
            or "external_visual_provider_api_key_scheme"
            or "external_visual_provider_missing_setting_names"
            or "external_visual_provider_invalid_setting_names"
            or "external_visual_provider_config_descriptor_status"
            or "external_visual_provider_route_config_boundary"
            or "visual_ai_provider_selection_policy"
            or "visual_ai_candidate_providers"
            or "visual_ai_requested_provider"
            or "visual_ai_requested_provider_name"
            or "visual_ai_selected_provider"
            or "visual_ai_selected_provider_name"
            or "visual_ai_selected_provider_status"
            or "visual_ai_provider_mode"
            or "visual_ai_provider_fallback_status"
            or "visual_ai_provider_fallback_reason"
            or "visual_ai_unavailable_provider_count"
            or "visual_ai_unavailable_providers"
            or "visual_ai_real_cloud_provider_claimed"
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
