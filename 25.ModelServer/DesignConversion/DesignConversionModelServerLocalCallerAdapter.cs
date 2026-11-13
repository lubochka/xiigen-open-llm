// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Globalization;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;

public sealed record DesignConversionModelServerLocalCallerPackage(
    string RequestId,
    string SourceKind,
    DesignConversionModelServerPayloadRef? DesignTree,
    DesignConversionModelServerPayloadRef? OriginalTree,
    string? Html,
    string? Css,
    IReadOnlyList<DesignConversionModelServerArtifactRef>? Screenshots = null,
    IReadOnlyList<DesignConversionModelServerArtifactRef>? DesignRenders = null,
    DesignConversionModelServerBudget? Budget = null,
    IReadOnlyDictionary<string, string>? Metadata = null)
{
    /// <summary>
    /// Optional bounded caller admission/advisory measurement for the local
    /// improvement-loop path. It is never child-DPO evaluation authority.
    /// </summary>
    public DesignConversionMeasurementGateRequest? MeasurementGateRequest
    {
        get;
        init;
    }
}

public sealed record DesignConversionModelServerImprovementLoopLocalCallerPackage(
    string LoopId,
    DesignConversionModelServerLocalCallerPackage? InitialPackage,
    int MaxAttempts,
    IReadOnlyDictionary<string, string>? Metadata = null);

public sealed record DesignConversionModelServerLocalCallerAdapterResponse(
    string EnvelopeSchemaVersion,
    string EnvelopeBoundary,
    string EnvelopeKind,
    string AdapterBoundary,
    string CallerSourceKind,
    string ContractSchemaVersion,
    IReadOnlyList<string> RequiredRequestFields,
    string EndpointMethod,
    string EndpointPath,
    int StatusCode,
    bool RegistryInvokeSucceeded,
    bool RuntimeInvoked,
    bool ParsedResponseJsonSucceeded,
    string? ResponseJsonParseError,
    string? ParsedContractStatus,
    string? ParsedRequestId,
    string? ParsedJobId,
    string? ParsedErrorCode,
    string? ParsedErrorMessage,
    string ResponseBodyJson,
    bool RawHtmlIncluded,
    bool RawCssIncluded,
    bool RawDesignTreeIncluded,
    bool ResponseContainsRawPrivateData,
    bool RawPrivateResponseLeakageDetected,
    bool ExternalCallerRepoTouched,
    bool CrossRepoCallerVerified,
    bool PublicHttpEndpointVerified,
    IReadOnlyDictionary<string, string> BoundaryMetadata,
    DesignConversionModelServerDpoUsefulnessResponse? ParsedDpoUsefulnessResponse = null);

public sealed class DesignConversionModelServerLocalCallerAdapter
{
    public const string AdapterBoundary = "in_repo_local_contract_consumer_adapter_no_external_repo_wiring";
    public const string ParseFailureErrorCode = "ADAPTER_RESPONSE_JSON_PARSE_FAILED";

    private readonly DesignConversionModelServerEndpointRegistry _registry;

    public DesignConversionModelServerLocalCallerAdapter(DesignConversionModelServerEndpointRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public async Task<OperationResult<DesignConversionModelServerLocalCallerAdapterResponse>> SubmitAsync(
        DesignConversionModelServerLocalCallerPackage package,
        CancellationToken cancellationToken = default)
    {
        var envelope = await TrySubmitAsync(package, cancellationToken);
        if (IsSuccessfulEnvelope(envelope))
            return OperationResult<DesignConversionModelServerLocalCallerAdapterResponse>.Ok(envelope);

        return OperationResult<DesignConversionModelServerLocalCallerAdapterResponse>.Fail(
            envelope.ParsedErrorMessage ?? "DesignConversion local caller adapter returned an error envelope.",
            envelope.ParsedErrorCode ?? "LOCAL_CALLER_ADAPTER_ERROR");
    }

    public async Task<DesignConversionModelServerLocalCallerAdapterResponse> TrySubmitAsync(
        DesignConversionModelServerLocalCallerPackage? package,
        CancellationToken cancellationToken = default)
    {
        var descriptor = DesignConversionModelServerCallerContract.Describe();
        var routeDescriptor = EvaluateRouteDescriptor();
        var validatedSource = ValidatePackage(package, descriptor);
        if (!validatedSource.IsSuccess)
        {
            return BuildAdapterErrorResponse(
                routeDescriptor,
                package?.SourceKind ?? "UNKNOWN",
                statusCode: 400,
                errorCode: validatedSource.ErrorCode,
                errorMessage: validatedSource.ErrorMessage,
                registryInvokeSucceeded: false,
                runtimeInvoked: false,
                errorSource: "local_package_validation");
        }

        var apiRequest = ToApiRequest(package!, validatedSource.Value!.Value);
        var bodyJson = JsonSerializer.Serialize(apiRequest, DesignConversionModelServerHttpHandler.JsonOptions);
        var request = new DesignConversionModelServerHttpRequest(
            Method: DesignConversionModelServerEndpointRegistry.EndpointMethod,
            Path: DesignConversionModelServerEndpointRegistry.EndpointPath,
            BodyJson: bodyJson,
            ContentType: routeDescriptor.RequestContentType,
            Headers: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["x-design-conversion-local-caller-adapter"] = AdapterBoundary,
                ["x-design-conversion-caller-contract-schema"] = descriptor.SchemaVersion
            });

        return await TryInvokeHttpAsync(request, package!.SourceKind, routeDescriptor, cancellationToken);
    }

    public async Task<DesignConversionModelServerLocalCallerAdapterResponse> TryRunImprovementLoopAsync(
        DesignConversionModelServerImprovementLoopLocalCallerPackage? package,
        CancellationToken cancellationToken = default)
    {
        var contractDescriptor = DesignConversionModelServerCallerContract.Describe();
        var routeDescriptor = ImprovementLoopRouteDescriptor();
        var validatedSource = ValidateImprovementLoopPackage(package, contractDescriptor);
        if (!validatedSource.IsSuccess)
        {
            return BuildAdapterErrorResponse(
                routeDescriptor,
                package?.InitialPackage?.SourceKind ?? "UNKNOWN",
                statusCode: 400,
                errorCode: validatedSource.ErrorCode,
                errorMessage: validatedSource.ErrorMessage,
                registryInvokeSucceeded: false,
                runtimeInvoked: false,
                errorSource: "local_improvement_loop_package_validation");
        }

        DesignConversionMeasurementGateRequest? measurementSnapshot = null;
        if (package!.InitialPackage!.MeasurementGateRequest is not null)
        {
            var capturedMeasurement = DesignConversionMeasurementAdmission.Capture(
                package.InitialPackage.MeasurementGateRequest);
            if (!capturedMeasurement.IsSuccess || capturedMeasurement.Value is null)
            {
                return BuildAdapterErrorResponse(
                    routeDescriptor,
                    package.InitialPackage.SourceKind,
                    statusCode: 400,
                    errorCode: capturedMeasurement.ErrorCode,
                    errorMessage: capturedMeasurement.ErrorMessage,
                    registryInvokeSucceeded: false,
                    runtimeInvoked: false,
                    errorSource: "local_improvement_loop_measurement_validation");
            }
            measurementSnapshot = capturedMeasurement.Value;
        }

        var handlerRequest = new DesignConversionModelServerImprovementLoopRequest(
                LoopId: package.LoopId,
                InitialPackage: package.InitialPackage,
                MaxAttempts: package.MaxAttempts,
                Metadata: BuildImprovementLoopRequestMetadata(
                    package.Metadata,
                    package.InitialPackage.SourceKind))
            {
                MeasurementGateRequest = measurementSnapshot
            };
        var bodyJson = JsonSerializer.Serialize(handlerRequest, DesignConversionModelServerHttpHandler.JsonOptions);
        var request = new DesignConversionModelServerHttpRequest(
            Method: DesignConversionModelServerEndpointRegistry.EndpointMethod,
            Path: DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            BodyJson: bodyJson,
            ContentType: routeDescriptor.RequestContentType,
            Headers: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["x-design-conversion-local-caller-adapter"] = AdapterBoundary,
                ["x-design-conversion-caller-contract-schema"] = contractDescriptor.SchemaVersion,
                ["x-design-conversion-improvement-loop-route"] =
                    DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointRoute
            });

        return await TryInvokeHttpAsync(request, package.InitialPackage!.SourceKind, routeDescriptor, cancellationToken);
    }

    public async Task<DesignConversionModelServerLocalCallerAdapterResponse> TryGetJobStatusAsync(
        string? jobId,
        string callerSourceKind = "UNKNOWN",
        CancellationToken cancellationToken = default)
    {
        var routeDescriptor = StatusReadbackRouteDescriptor();
        var path = string.IsNullOrWhiteSpace(jobId)
            ? DesignConversionModelServerStatusHandler.EndpointPathPrefix
            : DesignConversionModelServerStatusHandler.EndpointPathPrefix + Uri.EscapeDataString(jobId.Trim());
        var request = new DesignConversionModelServerHttpRequest(
            Method: DesignConversionModelServerEndpointRegistry.StatusReadbackEndpointMethod,
            Path: path,
            BodyJson: null,
            ContentType: null,
            Headers: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["x-design-conversion-local-caller-adapter"] = AdapterBoundary,
                ["x-design-conversion-caller-contract-schema"] =
                    DesignConversionModelServerCallerContract.StatusReadbackRouteSchemaVersion,
                ["x-design-conversion-status-readback-route"] =
                    DesignConversionModelServerEndpointRegistry.StatusReadbackEndpointRoute
            });

        return await TryInvokeHttpAsync(request, callerSourceKind, routeDescriptor, cancellationToken);
    }

    public async Task<DesignConversionModelServerLocalCallerAdapterResponse> TryGetDpoUsefulnessAsync(
        DesignConversionDpoUsefulnessApiRequest? request = null,
        string callerSourceKind = "UNKNOWN",
        CancellationToken cancellationToken = default)
    {
        var routeDescriptor = DpoUsefulnessRouteDescriptor();
        var queryString = ToDpoUsefulnessQueryString(request);
        var endpointPath = DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath;
        var httpRequest = new DesignConversionModelServerHttpRequest(
            Method: DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointMethod,
            Path: endpointPath,
            BodyJson: null,
            ContentType: null,
            Headers: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["x-design-conversion-local-caller-adapter"] = AdapterBoundary,
                ["x-design-conversion-caller-contract-schema"] =
                    DesignConversionModelServerCallerContract.DpoUsefulnessRouteSchemaVersion,
                ["x-design-conversion-dpo-usefulness-route"] =
                    DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointRoute
            },
            QueryString: queryString);

        return await TryInvokeHttpAsync(httpRequest, callerSourceKind, routeDescriptor, cancellationToken);
    }

    public async Task<DesignConversionModelServerLocalCallerAdapterResponse> TryRunTrainingIntakeAsync(
        DesignConversionModelServerTrainingIntakeRequest request,
        string callerSourceKind = "UNKNOWN",
        CancellationToken cancellationToken = default)
    {
        var routeDescriptor = TrainingIntakeRouteDescriptor();
        var bodyJson = JsonSerializer.Serialize(request, DesignConversionModelServerHttpHandler.JsonOptions);
        var httpRequest = new DesignConversionModelServerHttpRequest(
            Method: DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointMethod,
            Path: DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
            BodyJson: bodyJson,
            ContentType: routeDescriptor.RequestContentType,
            Headers: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["x-design-conversion-local-caller-adapter"] = AdapterBoundary,
                ["x-design-conversion-caller-contract-schema"] =
                    DesignConversionModelServerCallerContract.TrainingIntakeRouteSchemaVersion,
                ["x-design-conversion-training-intake-route"] =
                    DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointRoute
            });

        return await TryInvokeHttpAsync(httpRequest, callerSourceKind, routeDescriptor, cancellationToken);
    }

    public async Task<DesignConversionModelServerLocalCallerAdapterResponse> TryInvokeHttpAsync(
        DesignConversionModelServerHttpRequest? request,
        string callerSourceKind,
        CancellationToken cancellationToken = default)
        => await TryInvokeHttpAsync(request, callerSourceKind, EvaluateRouteDescriptor(), cancellationToken);

    public async Task<DesignConversionModelServerLocalCallerAdapterResponse> TryInvokeImprovementLoopHttpAsync(
        DesignConversionModelServerHttpRequest? request,
        string callerSourceKind,
        CancellationToken cancellationToken = default)
        => await TryInvokeHttpAsync(request, callerSourceKind, ImprovementLoopRouteDescriptor(), cancellationToken);

    private async Task<DesignConversionModelServerLocalCallerAdapterResponse> TryInvokeHttpAsync(
        DesignConversionModelServerHttpRequest? request,
        string callerSourceKind,
        AdapterRouteDescriptor descriptor,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return BuildAdapterErrorResponse(
                descriptor,
                callerSourceKind,
                statusCode: 400,
                errorCode: "DESIGN_CONVERSION_ENDPOINT_REQUEST_REQUIRED",
                errorMessage: "DesignConversion endpoint request is required.",
                registryInvokeSucceeded: false,
                runtimeInvoked: false,
                errorSource: "endpoint_registry");
        }

        var invoked = await _registry.InvokeAsync(request, cancellationToken);
        if (!invoked.IsSuccess)
        {
            return BuildAdapterErrorResponse(
                descriptor,
                callerSourceKind,
                StatusCodeForRegistryError(invoked.ErrorCode),
                invoked.ErrorCode,
                invoked.ErrorMessage,
                registryInvokeSucceeded: false,
                runtimeInvoked: false,
                errorSource: "endpoint_registry");
        }

        return CreateEnvelopeFromHttpResponse(
            invoked.Value!,
            callerSourceKind,
            registryInvokeSucceeded: true,
            descriptor);
    }

    public static DesignConversionModelServerLocalCallerAdapterResponse CreateEnvelopeFromHttpResponse(
        DesignConversionModelServerHttpResponse? response,
        string callerSourceKind,
        bool registryInvokeSucceeded = true)
        => CreateEnvelopeFromHttpResponse(
            response,
            callerSourceKind,
            registryInvokeSucceeded,
            EvaluateRouteDescriptor());

    private static DesignConversionModelServerLocalCallerAdapterResponse CreateEnvelopeFromHttpResponse(
        DesignConversionModelServerHttpResponse? response,
        string callerSourceKind,
        bool registryInvokeSucceeded,
        AdapterRouteDescriptor descriptor)
    {
        if (response is null)
        {
            return BuildAdapterErrorResponse(
                descriptor,
                callerSourceKind,
                statusCode: 502,
                errorCode: "ADAPTER_RESPONSE_REQUIRED",
                errorMessage: "DesignConversion local caller adapter response is required.",
                registryInvokeSucceeded,
                runtimeInvoked: false,
                errorSource: "adapter_response_boundary");
        }

        return BuildResponse(descriptor, callerSourceKind, response, registryInvokeSucceeded);
    }

    private static AdapterRouteDescriptor EvaluateRouteDescriptor()
    {
        var descriptor = DesignConversionModelServerCallerContract.Describe();
        return new AdapterRouteDescriptor(
            SchemaVersion: descriptor.SchemaVersion,
            Boundary: descriptor.Boundary,
            EndpointMethod: descriptor.EndpointMethod,
            EndpointPath: descriptor.EndpointPath,
            RequestContentType: descriptor.RequestContentType,
            ResponseContentType: descriptor.ResponseContentType,
            RequiredRequestFields: descriptor.RequestFields
                .Where(field => field.Required)
                .Select(field => field.JsonName)
                .ToArray());
    }

    private static AdapterRouteDescriptor ImprovementLoopRouteDescriptor()
    {
        var descriptor = DesignConversionModelServerCallerContract.DescribeImprovementLoopRoute();
        return new AdapterRouteDescriptor(
            SchemaVersion: descriptor.SchemaVersion,
            Boundary: descriptor.Boundary,
            EndpointMethod: descriptor.EndpointMethod,
            EndpointPath: descriptor.EndpointPath,
            RequestContentType: descriptor.RequestContentType,
            ResponseContentType: descriptor.ResponseContentType,
            RequiredRequestFields: descriptor.RequestFields
                .Where(field => field.Required)
                .Select(field => field.JsonName)
                .ToArray());
    }

    private static AdapterRouteDescriptor StatusReadbackRouteDescriptor()
    {
        var descriptor = DesignConversionModelServerCallerContract.DescribeStatusReadbackRoute();
        return new AdapterRouteDescriptor(
            SchemaVersion: descriptor.SchemaVersion,
            Boundary: descriptor.Boundary,
            EndpointMethod: descriptor.EndpointMethod,
            EndpointPath: descriptor.EndpointPath,
            RequestContentType: descriptor.RequestContentType,
            ResponseContentType: descriptor.ResponseContentType,
            RequiredRequestFields: descriptor.RequestFields
                .Where(field => field.Required)
                .Select(field => field.JsonName)
                .ToArray());
    }

    private static AdapterRouteDescriptor DpoUsefulnessRouteDescriptor()
    {
        var descriptor = DesignConversionModelServerCallerContract.DescribeDpoUsefulnessRoute();
        return new AdapterRouteDescriptor(
            SchemaVersion: descriptor.SchemaVersion,
            Boundary: descriptor.Boundary,
            EndpointMethod: descriptor.EndpointMethod,
            EndpointPath: descriptor.EndpointPath,
            RequestContentType: descriptor.RequestContentType,
            ResponseContentType: descriptor.ResponseContentType,
            RequiredRequestFields: DesignConversionModelServerCallerContract.DpoUsefulnessOptionalQueryFieldNames());
    }

    private static AdapterRouteDescriptor TrainingIntakeRouteDescriptor()
    {
        var descriptor = DesignConversionModelServerCallerContract.DescribeTrainingIntakeRoute();
        return new AdapterRouteDescriptor(
            SchemaVersion: descriptor.SchemaVersion,
            Boundary: descriptor.Boundary,
            EndpointMethod: descriptor.EndpointMethod,
            EndpointPath: descriptor.EndpointPath,
            RequestContentType: descriptor.RequestContentType,
            ResponseContentType: descriptor.ResponseContentType,
            RequiredRequestFields: descriptor.RequestFields
                .Select(field => field.JsonName)
                .ToArray());
    }

    private static string ToDpoUsefulnessQueryString(DesignConversionDpoUsefulnessApiRequest? request)
    {
        if (request is null)
            return "?includeBlocked=true";

        var parts = new List<string>
        {
            "includeBlocked=" + request.IncludeBlocked.ToString().ToLowerInvariant()
        };
        if (!string.IsNullOrWhiteSpace(request.UnitId))
            parts.Add("unitId=" + Uri.EscapeDataString(request.UnitId));
        if (request.MinimumUsefulnessScore is { } minimum)
            parts.Add("minimumUsefulnessScore=" + minimum.ToString(CultureInfo.InvariantCulture));
        if (request.MaximumUsefulnessScore is { } maximum)
            parts.Add("maximumUsefulnessScore=" + maximum.ToString(CultureInfo.InvariantCulture));
        if (request.Limit is { } limit)
            parts.Add("limit=" + limit.ToString(CultureInfo.InvariantCulture));

        return "?" + string.Join("&", parts);
    }

    private static OperationResult<DesignConversionSourceKind?> ValidateImprovementLoopPackage(
        DesignConversionModelServerImprovementLoopLocalCallerPackage? package,
        DesignConversionModelServerCallerContractDescriptor descriptor)
    {
        if (package is null)
            return OperationResult<DesignConversionSourceKind?>.Fail(
                "DesignConversion improvement-loop local caller package is required.",
                "IMPROVEMENT_LOOP_LOCAL_PACKAGE_REQUIRED");

        if (string.IsNullOrWhiteSpace(package.LoopId))
            return OperationResult<DesignConversionSourceKind?>.Fail(
                "DesignConversion improvement-loop local caller package loop id is required.",
                "IMPROVEMENT_LOOP_ID_REQUIRED");

        if (package.InitialPackage is null)
            return OperationResult<DesignConversionSourceKind?>.Fail(
                "DesignConversion improvement-loop local caller package initial package is required.",
                "IMPROVEMENT_LOOP_INITIAL_PACKAGE_REQUIRED");

        var typedMaxImprovementIterations = package.InitialPackage.Budget?.MaxImprovementIterations;
        if (typedMaxImprovementIterations.HasValue)
        {
            if (!DesignConversionVisualQaBoundaryPolicy.IsValidConfiguredMaxImprovementIterations(
                    typedMaxImprovementIterations))
            {
                return OperationResult<DesignConversionSourceKind?>.Fail(
                    DesignConversionVisualQaBoundaryPolicy.MaxImprovementIterationsInvalidErrorMessage(),
                    DesignConversionVisualQaBoundaryPolicy.MaxImprovementIterationsInvalidErrorCode);
            }
        }
        else
        {
            if (package.MaxAttempts <= 0)
                return OperationResult<DesignConversionSourceKind?>.Fail(
                    "DesignConversion improvement-loop local caller package max attempts must be greater than zero.",
                    "IMPROVEMENT_LOOP_MAX_ATTEMPTS_REQUIRED");

            if (package.MaxAttempts > DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts)
                return OperationResult<DesignConversionSourceKind?>.Fail(
                    $"DesignConversion improvement-loop max attempts must be less than or equal to {DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts.ToString(CultureInfo.InvariantCulture)}.",
                    DesignConversionModelServerImprovementLoopHandler.MaxAttemptsExceededErrorCode);
        }

        var payloadSize = DesignConversionModelServerPayloadSizeGuard.ValidateImprovementLoopPackage(
            package,
            "DesignConversion improvement-loop local caller package");
        if (!payloadSize.IsSuccess)
            return payloadSize.MapError<DesignConversionSourceKind?>();

        return ValidatePackage(package.InitialPackage, descriptor);
    }

    private static OperationResult<DesignConversionSourceKind?> ValidatePackage(
        DesignConversionModelServerLocalCallerPackage? package,
        DesignConversionModelServerCallerContractDescriptor descriptor)
    {
        if (package is null)
            return OperationResult<DesignConversionSourceKind?>.Fail(
                "DesignConversion local caller package is required.",
                "LOCAL_CALLER_PACKAGE_REQUIRED");

        var required = DesignConversionModelServerCallerContract.RequiredRequestFieldNames();
        if (required.Contains("requestId", StringComparer.Ordinal) && string.IsNullOrWhiteSpace(package.RequestId))
            return OperationResult<DesignConversionSourceKind?>.Fail(
                "DesignConversion local caller package request id is required.",
                "REQUEST_ID_REQUIRED");

        if (required.Contains("sourceKind", StringComparer.Ordinal) &&
            string.IsNullOrWhiteSpace(package.SourceKind))
        {
            return OperationResult<DesignConversionSourceKind?>.Fail(
                "DesignConversion local caller package source kind is required.",
                "SOURCE_KIND_REQUIRED");
        }

        if (!Enum.TryParse<DesignConversionSourceKind>(
                package.SourceKind,
                ignoreCase: true,
                out var sourceKind) ||
            !descriptor.SupportedSourceKinds.Contains(sourceKind.ToString(), StringComparer.Ordinal))
        {
            return OperationResult<DesignConversionSourceKind?>.Fail(
                $"DesignConversion local caller package source kind is not supported: {package.SourceKind}",
                "SOURCE_KIND_UNSUPPORTED");
        }

        if (required.Contains("designTree", StringComparer.Ordinal) && !HasSourcePayload(package.DesignTree))
            return OperationResult<DesignConversionSourceKind?>.Fail(
                "DesignConversion local caller package design tree payload, ref, or hash is required.",
                "DESIGN_CONVERSION_SOURCE_TREE_REQUIRED");

        if (required.Contains("originalTree", StringComparer.Ordinal) && !HasSourcePayload(package.OriginalTree))
            return OperationResult<DesignConversionSourceKind?>.Fail(
                "DesignConversion local caller package original tree payload, ref, or hash is required.",
                "DESIGN_CONVERSION_ORIGINAL_TREE_REQUIRED");

        if (required.Contains("html", StringComparer.Ordinal) && string.IsNullOrWhiteSpace(package.Html))
            return OperationResult<DesignConversionSourceKind?>.Fail(
                "DesignConversion local caller package generated HTML is required.",
                "DESIGN_CONVERSION_HTML_REQUIRED");

        if (required.Contains("css", StringComparer.Ordinal) && string.IsNullOrWhiteSpace(package.Css))
            return OperationResult<DesignConversionSourceKind?>.Fail(
                "DesignConversion local caller package generated CSS is required.",
                "DESIGN_CONVERSION_CSS_REQUIRED");

        var payloadSize = DesignConversionModelServerPayloadSizeGuard.ValidatePackage(
            package,
            "DesignConversion local caller package");
        if (!payloadSize.IsSuccess)
            return payloadSize.MapError<DesignConversionSourceKind?>();

        var artifacts = (package.Screenshots ?? [])
            .Concat(package.DesignRenders ?? [])
            .ToArray();
        if (required.Contains("screenshots|designRenders", StringComparer.Ordinal) && artifacts.Length == 0)
            return OperationResult<DesignConversionSourceKind?>.Fail(
                "DesignConversion local caller package screenshot or expected render ref is required.",
                "DESIGN_CONVERSION_SCREENSHOT_OR_RENDER_REQUIRED");

        if (artifacts.Length > 0 && !artifacts.Any(artifact => artifact.Width > 0 && artifact.Height > 0))
            return OperationResult<DesignConversionSourceKind?>.Fail(
                "DesignConversion local caller package screenshot/render refs must include at least one positive viewport.",
                "DESIGN_CONVERSION_VIEWPORT_REQUIRED");

        var artifactRefs = DesignConversionModelServerArtifactRefGuard.Validate(
            artifacts,
            "DesignConversion local caller package screenshot/render refs");
        if (!artifactRefs.IsSuccess)
            return artifactRefs.MapError<DesignConversionSourceKind?>();

        if (package.Budget is not null && package.Budget.BudgetUsd < 0m)
            return OperationResult<DesignConversionSourceKind?>.Fail(
                "DesignConversion improvement-loop budgetUsd must be greater than or equal to zero.",
                DesignConversionInternalService.NegativeBudgetUsdErrorCode);

        if (package.Budget is not null &&
            !DesignConversionVisualQaBoundaryPolicy.IsValidConfiguredRenderQaPassThreshold(
                package.Budget.RenderQaPassThreshold,
                package.Budget.Tier))
        {
            return OperationResult<DesignConversionSourceKind?>.Fail(
                "DesignConversion render QA pass threshold must be finite, greater than 0, at most 1, and not below the server-owned tier floor.",
                DesignConversionVisualQaBoundaryPolicy.RenderQaPassThresholdInvalidErrorCode);
        }

        if (package.Budget is not null &&
            !DesignConversionVisualQaBoundaryPolicy.IsValidConfiguredMaxImprovementIterations(
                package.Budget.MaxImprovementIterations))
        {
            return OperationResult<DesignConversionSourceKind?>.Fail(
                DesignConversionVisualQaBoundaryPolicy.MaxImprovementIterationsInvalidErrorMessage(),
                DesignConversionVisualQaBoundaryPolicy.MaxImprovementIterationsInvalidErrorCode);
        }

        if (package.Budget is not null && package.Budget.MaxImprovementCandidates < 0)
            return OperationResult<DesignConversionSourceKind?>.Fail(
                "DesignConversion improvement candidate budget must be greater than zero.",
                DesignConversionInternalService.ImprovementCandidateBudgetRequiredCode);

        if (package.Budget is not null &&
            package.Budget.MaxImprovementCandidates == 0 &&
            !DesignConversionVisualQaBoundaryPolicy.IsServerOwnedEntitlementTier(package.Budget.Tier))
        {
            return OperationResult<DesignConversionSourceKind?>.Fail(
                "DesignConversion improvement candidate budget must be greater than zero.",
                DesignConversionInternalService.ImprovementCandidateBudgetRequiredCode);
        }

        return OperationResult<DesignConversionSourceKind?>.Ok(sourceKind);
    }

    private static DesignConversionModelServerApiRequest ToApiRequest(
        DesignConversionModelServerLocalCallerPackage package,
        DesignConversionSourceKind sourceKind)
        => new(
            RequestId: package.RequestId,
            SourceKind: sourceKind,
            DesignTree: package.DesignTree!,
            OriginalTree: package.OriginalTree!,
            Html: package.Html!,
            Css: package.Css!,
            Screenshots: package.Screenshots,
            DesignRenders: package.DesignRenders,
            Budget: package.Budget,
            Metadata: BuildRequestMetadata(package.Metadata, package.SourceKind));

    private static IReadOnlyDictionary<string, string> BuildRequestMetadata(
        IReadOnlyDictionary<string, string>? callerMetadata,
        string sourceKind)
    {
        var metadata = new Dictionary<string, string>(callerMetadata ?? new Dictionary<string, string>(), StringComparer.Ordinal)
        {
            ["local_caller_adapter_boundary"] = AdapterBoundary,
            ["caller_contract_schema_version"] = DesignConversionModelServerCallerContract.SchemaVersion,
            ["caller_source_kind"] = sourceKind,
            ["cross_repo_caller_verified"] = bool.FalseString,
            ["external_caller_repo_touched"] = bool.FalseString
        };
        return metadata;
    }

    private static IReadOnlyDictionary<string, string> BuildImprovementLoopRequestMetadata(
        IReadOnlyDictionary<string, string>? callerMetadata,
        string sourceKind)
    {
        var metadata = BuildRequestMetadata(callerMetadata, sourceKind).ToDictionary(
            pair => pair.Key,
            pair => pair.Value,
            StringComparer.Ordinal);
        metadata["local_improvement_loop_adapter_route"] = DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointRoute;
        metadata["refs_only_public_response"] = bool.TrueString;
        metadata["asp_net_host_verified"] = bool.FalseString;
        metadata["socket_opened"] = bool.FalseString;
        metadata["public_http_endpoint_verified"] = bool.FalseString;
        metadata["cross_repo_caller_verified"] = bool.FalseString;
        metadata["external_provider_called"] = bool.FalseString;
        metadata["product_ready"] = bool.FalseString;
        return metadata;
    }

    private static DesignConversionModelServerLocalCallerAdapterResponse BuildResponse(
        AdapterRouteDescriptor descriptor,
        string callerSourceKind,
        DesignConversionModelServerHttpResponse response,
        bool registryInvokeSucceeded)
    {
        var parsed = ParseResponse(response.BodyJson);
        var metadata = BuildEnvelopeBoundaryMetadata(
            descriptor,
            callerSourceKind,
            response.StatusCode,
            registryInvokeSucceeded,
            parsed.BoundaryMetadata);
        var rawHtmlIncluded = IsTruthy(metadata, "raw_html_included") || LooksLikeRawHtml(response.BodyJson);
        var rawCssIncluded = IsTruthy(metadata, "raw_css_included");
        var rawDesignTreeIncluded = IsTruthy(metadata, "raw_design_tree_included");
        var responseContainsRawPrivateData = IsTruthy(metadata, "response_contains_raw_private_data") ||
            IsTruthy(metadata, "raw_user_private_data_included") ||
            LooksLikeRawPrivatePayload(response.BodyJson);
        var rawPrivateLeakageDetected = rawHtmlIncluded ||
            rawCssIncluded ||
            rawDesignTreeIncluded ||
            responseContainsRawPrivateData;
        metadata["response_contains_raw_private_data"] = responseContainsRawPrivateData.ToString();
        metadata["raw_private_response_leakage_detected"] = rawPrivateLeakageDetected.ToString();

        var parsedErrorCode = parsed.ParseSucceeded
            ? parsed.ErrorCode
            : ParseFailureErrorCode;
        var parsedErrorMessage = parsed.ParseSucceeded
            ? parsed.ErrorMessage
            : "DesignConversion local caller adapter could not parse response JSON.";
        var parsedDpoUsefulnessResponse = ParseDpoUsefulnessResponse(descriptor, response, parsed);
        if (parsedDpoUsefulnessResponse.Attempted)
        {
            metadata["local_caller_dpo_usefulness_typed_response_parsed"] =
                parsedDpoUsefulnessResponse.ParseSucceeded.ToString();
            if (!parsedDpoUsefulnessResponse.ParseSucceeded)
                metadata["local_caller_dpo_usefulness_typed_response_parse_error"] =
                    parsedDpoUsefulnessResponse.ParseError ?? "unknown";
        }

        if (parsed.ParseSucceeded &&
            string.IsNullOrWhiteSpace(parsedErrorCode) &&
            parsedDpoUsefulnessResponse.Attempted &&
            !parsedDpoUsefulnessResponse.ParseSucceeded)
        {
            parsedErrorCode = parsedDpoUsefulnessResponse.ErrorCode;
            parsedErrorMessage = parsedDpoUsefulnessResponse.ErrorMessage;
        }

        var envelopeKind = response.StatusCode is >= 200 and <= 299 &&
            parsed.ParseSucceeded &&
            string.IsNullOrWhiteSpace(parsedErrorCode)
                ? "success"
                : "error";

        return new DesignConversionModelServerLocalCallerAdapterResponse(
            EnvelopeSchemaVersion: DesignConversionModelServerCallerContract.LocalCallerResponseEnvelopeSchemaVersion,
            EnvelopeBoundary: DesignConversionModelServerCallerContract.LocalCallerResponseEnvelopeBoundary,
            EnvelopeKind: envelopeKind,
            AdapterBoundary: AdapterBoundary,
            CallerSourceKind: callerSourceKind,
            ContractSchemaVersion: descriptor.SchemaVersion,
            RequiredRequestFields: descriptor.RequiredRequestFields,
            EndpointMethod: descriptor.EndpointMethod,
            EndpointPath: descriptor.EndpointPath,
            StatusCode: response.StatusCode,
            RegistryInvokeSucceeded: registryInvokeSucceeded,
            RuntimeInvoked: IsTruthy(metadata, "runtime_invoked"),
            ParsedResponseJsonSucceeded: parsed.ParseSucceeded,
            ResponseJsonParseError: parsed.ParseError,
            ParsedContractStatus: parsed.ContractStatus,
            ParsedRequestId: parsed.RequestId,
            ParsedJobId: parsed.JobId,
            ParsedErrorCode: parsedErrorCode,
            ParsedErrorMessage: parsedErrorMessage,
            ResponseBodyJson: response.BodyJson,
            RawHtmlIncluded: rawHtmlIncluded,
            RawCssIncluded: rawCssIncluded,
            RawDesignTreeIncluded: rawDesignTreeIncluded,
            ResponseContainsRawPrivateData: responseContainsRawPrivateData,
            RawPrivateResponseLeakageDetected: rawPrivateLeakageDetected,
            ExternalCallerRepoTouched: IsTruthy(metadata, "external_caller_repo_touched"),
            CrossRepoCallerVerified: IsTruthy(metadata, "cross_repo_caller_verified"),
            PublicHttpEndpointVerified: IsTruthy(metadata, "public_http_endpoint_verified"),
            BoundaryMetadata: metadata,
            ParsedDpoUsefulnessResponse: parsedDpoUsefulnessResponse.Response);
    }

    private static DesignConversionModelServerLocalCallerAdapterResponse BuildAdapterErrorResponse(
        AdapterRouteDescriptor descriptor,
        string callerSourceKind,
        int statusCode,
        string? errorCode,
        string? errorMessage,
        bool registryInvokeSucceeded,
        bool runtimeInvoked,
        string errorSource)
    {
        var metadata = BuildEnvelopeBoundaryMetadata(
            descriptor,
            callerSourceKind,
            statusCode,
            registryInvokeSucceeded,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["runtime_invoked"] = runtimeInvoked.ToString(),
                ["adapter_error_source"] = errorSource
            });
        var body = new DesignConversionModelServerHttpErrorResponse(
            Endpoint: descriptor.EndpointPath,
            Method: descriptor.EndpointMethod,
            HttpSeam: AdapterBoundary,
            StatusCode: statusCode,
            ErrorCode: string.IsNullOrWhiteSpace(errorCode) ? "LOCAL_CALLER_ADAPTER_ERROR" : errorCode,
            ErrorMessage: string.IsNullOrWhiteSpace(errorMessage)
                ? "DesignConversion local caller adapter returned an error envelope."
                : errorMessage,
            BoundaryMetadata: metadata);
        var response = new DesignConversionModelServerHttpResponse(
            StatusCode: statusCode,
            ContentType: descriptor.ResponseContentType,
            BodyJson: JsonSerializer.Serialize(body, DesignConversionModelServerHttpHandler.JsonOptions),
            Headers: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["x-design-conversion-local-caller-adapter"] = AdapterBoundary,
                ["x-design-conversion-local-caller-envelope-schema"] =
                    DesignConversionModelServerCallerContract.LocalCallerResponseEnvelopeSchemaVersion,
                ["x-design-conversion-status-code"] = statusCode.ToString(CultureInfo.InvariantCulture)
            });

        return BuildResponse(descriptor, callerSourceKind, response, registryInvokeSucceeded);
    }

    private static Dictionary<string, string> BuildEnvelopeBoundaryMetadata(
        AdapterRouteDescriptor descriptor,
        string callerSourceKind,
        int statusCode,
        bool registryInvokeSucceeded,
        IReadOnlyDictionary<string, string> parsedMetadata)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in DesignConversionModelServerCallerContract.BoundaryMetadata())
            metadata[pair.Key] = pair.Value;
        foreach (var pair in parsedMetadata)
            metadata[pair.Key] = pair.Value;

        metadata["local_caller_adapter_boundary"] = AdapterBoundary;
        metadata["local_caller_source_kind"] = string.IsNullOrWhiteSpace(callerSourceKind) ? "UNKNOWN" : callerSourceKind;
        metadata["caller_contract_schema_version"] = descriptor.SchemaVersion;
        metadata["caller_contract_boundary"] = descriptor.Boundary;
        metadata["local_caller_endpoint_method"] = descriptor.EndpointMethod;
        metadata["local_caller_endpoint_path"] = descriptor.EndpointPath;
        metadata["local_caller_response_envelope_schema_version"] =
            DesignConversionModelServerCallerContract.LocalCallerResponseEnvelopeSchemaVersion;
        metadata["local_caller_response_envelope_boundary"] =
            DesignConversionModelServerCallerContract.LocalCallerResponseEnvelopeBoundary;
        metadata["local_caller_success_error_envelope_stable"] = bool.TrueString;
        metadata["local_caller_response_status_code"] = statusCode.ToString(CultureInfo.InvariantCulture);
        metadata["registry_invoke_succeeded"] = registryInvokeSucceeded.ToString();
        metadata["runtime_invoked"] = IsTruthy(metadata, "runtime_invoked").ToString();
        metadata["external_caller_repo_touched"] = bool.FalseString;
        metadata["cross_repo_caller_verified"] = bool.FalseString;
        metadata["public_http_endpoint_verified"] = bool.FalseString;
        metadata["socket_opened"] = bool.FalseString;
        metadata["deployment_verified"] = bool.FalseString;
        metadata["product_ready"] = bool.FalseString;
        metadata["raw_html_included"] = IsTruthy(metadata, "raw_html_included").ToString();
        metadata["raw_css_included"] = IsTruthy(metadata, "raw_css_included").ToString();
        metadata["raw_design_tree_included"] = IsTruthy(metadata, "raw_design_tree_included").ToString();
        metadata["raw_user_private_data_included"] = IsTruthy(metadata, "raw_user_private_data_included").ToString();
        if (!metadata.ContainsKey("response_contains_raw_private_data"))
            metadata["response_contains_raw_private_data"] = bool.FalseString;

        return metadata;
    }

    private static ParsedResponse ParseResponse(string bodyJson)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal);
        string? contractStatus = null;
        string? requestId = null;
        string? jobId = null;
        string? errorCode = null;
        string? errorMessage = null;

        try
        {
            using var document = JsonDocument.Parse(bodyJson);
            var root = document.RootElement;
            contractStatus = ReadString(root, "contractStatus");
            requestId = ReadString(root, "requestId");
            jobId = ReadString(root, "jobId");
            errorCode = ReadString(root, "errorCode");
            errorMessage = ReadString(root, "errorMessage");

            if (root.TryGetProperty("boundaryMetadata", out var boundaryMetadata) &&
                boundaryMetadata.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in boundaryMetadata.EnumerateObject())
                    metadata[property.Name] = property.Value.ValueKind == JsonValueKind.String
                        ? property.Value.GetString() ?? string.Empty
                        : property.Value.ToString();
            }

            metadata["adapter_response_json_parse_succeeded"] = bool.TrueString;
            return new ParsedResponse(contractStatus, requestId, jobId, errorCode, errorMessage, true, null, metadata);
        }
        catch (JsonException ex)
        {
            metadata["adapter_response_json_parse_error"] = ex.GetType().Name;
            metadata["adapter_response_json_parse_succeeded"] = bool.FalseString;
        }

        metadata["local_caller_adapter_boundary"] = AdapterBoundary;
        metadata["caller_contract_schema_version"] = DesignConversionModelServerCallerContract.SchemaVersion;
        metadata["cross_repo_caller_verified"] = bool.FalseString;
        metadata["external_caller_repo_touched"] = bool.FalseString;

        return new ParsedResponse(
            contractStatus,
            requestId,
            jobId,
            errorCode,
            errorMessage,
            false,
            metadata["adapter_response_json_parse_error"],
            metadata);
    }

    private static ParsedDpoUsefulnessResponse ParseDpoUsefulnessResponse(
        AdapterRouteDescriptor descriptor,
        DesignConversionModelServerHttpResponse response,
        ParsedResponse parsed)
    {
        if (!string.Equals(
                descriptor.SchemaVersion,
                DesignConversionModelServerCallerContract.DpoUsefulnessRouteSchemaVersion,
                StringComparison.Ordinal) ||
            response.StatusCode is < 200 or > 299 ||
            !parsed.ParseSucceeded ||
            !string.IsNullOrWhiteSpace(parsed.ErrorCode))
        {
            return ParsedDpoUsefulnessResponse.NotAttempted;
        }

        var parsedTyped = DesignConversionModelServerDpoUsefulnessResponseParser.Parse(response.BodyJson);
        return parsedTyped.ParseSucceeded
            ? ParsedDpoUsefulnessResponse.Succeeded(parsedTyped.Response!)
            : ParsedDpoUsefulnessResponse.Failed(
                parsedTyped.ErrorCode ?? DesignConversionModelServerDpoUsefulnessResponseParser.ParseFailedErrorCode,
                parsedTyped.ErrorMessage ?? "DesignConversion local caller adapter could not parse the DPO usefulness response as a typed DTO.",
                parsedTyped.ParseError ?? "unknown");
    }

    private static string? ReadString(JsonElement root, string propertyName)
        => root.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static bool HasSourcePayload(DesignConversionModelServerPayloadRef? source)
        => source is not null &&
           (!string.IsNullOrWhiteSpace(source.Payload) ||
            !string.IsNullOrWhiteSpace(source.RefId) ||
            !string.IsNullOrWhiteSpace(source.Sha256));

    private static bool IsTruthy(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) &&
           bool.TryParse(value, out var parsed) &&
           parsed;

    private static bool IsSuccessfulEnvelope(DesignConversionModelServerLocalCallerAdapterResponse envelope)
        => envelope.StatusCode is >= 200 and <= 299 &&
           string.Equals(envelope.EnvelopeKind, "success", StringComparison.Ordinal) &&
           envelope.ParsedResponseJsonSucceeded &&
           string.IsNullOrWhiteSpace(envelope.ParsedErrorCode);

    private static int StatusCodeForRegistryError(string? errorCode)
        => errorCode switch
        {
            "DESIGN_CONVERSION_ENDPOINT_REQUEST_REQUIRED" => 400,
            "DESIGN_CONVERSION_ENDPOINT_METHOD_REQUIRED" => 400,
            "DESIGN_CONVERSION_ENDPOINT_METHOD_NOT_ALLOWED" => 405,
            "DESIGN_CONVERSION_ENDPOINT_ROUTE_NOT_FOUND" => 404,
            _ => 502
        };

    private static bool LooksLikeRawHtml(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           (value.Contains("<html", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("<main", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("<div", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("data-secret", StringComparison.OrdinalIgnoreCase));

    private static bool LooksLikeRawPrivatePayload(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (LooksLikeRawHtml(value))
            return true;

        try
        {
            using var document = JsonDocument.Parse(value);
            return ContainsSecretProperty(document.RootElement);
        }
        catch (JsonException)
        {
            return LooksLikeSecretPropertyName(value);
        }
    }

    private static bool ContainsSecretProperty(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    if (HasSensitiveSecretPropertyToken(property.Name) ||
                        ContainsSecretProperty(property.Value))
                    {
                        return true;
                    }
                }

                return false;

            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    if (ContainsSecretProperty(item))
                        return true;
                }

                return false;

            default:
                return false;
        }
    }

    private static bool LooksLikeSecretPropertyName(string value)
    {
        for (var index = 0; index < value.Length; index++)
        {
            if (value[index] != '"')
                continue;

            var nameStart = index + 1;
            var nameEnd = value.IndexOf('"', nameStart);
            if (nameEnd < 0)
                return false;

            if (!HasSensitiveSecretPropertyToken(value.Substring(nameStart, nameEnd - nameStart)))
                continue;

            var colonIndex = nameEnd + 1;
            while (colonIndex < value.Length && char.IsWhiteSpace(value[colonIndex]))
                colonIndex++;

            if (colonIndex < value.Length && value[colonIndex] == ':')
                return true;
        }

        return false;
    }

    private static bool HasSensitiveSecretPropertyToken(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            return false;

        var tokenStart = -1;
        var previous = '\0';

        for (var index = 0; index < propertyName.Length; index++)
        {
            var current = propertyName[index];
            if (!char.IsLetterOrDigit(current))
            {
                if (IsSecretToken(propertyName, tokenStart, index))
                    return true;

                tokenStart = -1;
                previous = '\0';
                continue;
            }

            var next = index + 1 < propertyName.Length ? propertyName[index + 1] : '\0';
            if (tokenStart >= 0 &&
                ((char.IsLower(previous) && char.IsUpper(current)) ||
                 (char.IsUpper(previous) && char.IsUpper(current) && char.IsLower(next))))
            {
                if (IsSecretToken(propertyName, tokenStart, index))
                    return true;

                tokenStart = index;
            }
            else if (tokenStart < 0)
            {
                tokenStart = index;
            }

            previous = current;
        }

        return IsSecretToken(propertyName, tokenStart, propertyName.Length);
    }

    private static bool IsSecretToken(string propertyName, int tokenStart, int tokenEnd)
    {
        const string secretToken = "secret";
        var tokenLength = tokenEnd - tokenStart;

        return tokenStart >= 0 &&
               tokenLength == secretToken.Length &&
               string.Compare(
                   propertyName,
                   tokenStart,
                   secretToken,
                   0,
                   secretToken.Length,
                   StringComparison.OrdinalIgnoreCase) == 0;
    }

    private sealed record ParsedResponse(
        string? ContractStatus,
        string? RequestId,
        string? JobId,
        string? ErrorCode,
        string? ErrorMessage,
        bool ParseSucceeded,
        string? ParseError,
        IReadOnlyDictionary<string, string> BoundaryMetadata);

    private sealed record ParsedDpoUsefulnessResponse(
        bool Attempted,
        bool ParseSucceeded,
        DesignConversionModelServerDpoUsefulnessResponse? Response,
        string? ErrorCode,
        string? ErrorMessage,
        string? ParseError)
    {
        public static ParsedDpoUsefulnessResponse NotAttempted { get; } = new(
            Attempted: false,
            ParseSucceeded: false,
            Response: null,
            ErrorCode: null,
            ErrorMessage: null,
            ParseError: null);

        public static ParsedDpoUsefulnessResponse Succeeded(DesignConversionModelServerDpoUsefulnessResponse response)
            => new(
                Attempted: true,
                ParseSucceeded: true,
                Response: response,
                ErrorCode: null,
                ErrorMessage: null,
                ParseError: null);

        public static ParsedDpoUsefulnessResponse Failed(string errorCode, string errorMessage, string parseError)
            => new(
                Attempted: true,
                ParseSucceeded: false,
                Response: null,
                ErrorCode: errorCode,
                ErrorMessage: errorMessage,
                ParseError: parseError);
    }

    private sealed record AdapterRouteDescriptor(
        string SchemaVersion,
        string Boundary,
        string EndpointMethod,
        string EndpointPath,
        string RequestContentType,
        string ResponseContentType,
        IReadOnlyList<string> RequiredRequestFields);
}
