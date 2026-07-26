// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using DesignConversionDomain;

public sealed record DesignConversionModelServerHttpRequest(
    string Method,
    string Path,
    string? BodyJson,
    string? ContentType = "application/json",
    IReadOnlyDictionary<string, string>? Headers = null,
    string? QueryString = null);

public sealed record DesignConversionModelServerHttpResponse(
    int StatusCode,
    string ContentType,
    string BodyJson,
    IReadOnlyDictionary<string, string> Headers);

public sealed record DesignConversionModelServerHttpEvaluateResponse(
    string Endpoint,
    string Method,
    string HttpSeam,
    string ApiShape,
    string RuntimeSurface,
    int StatusCode,
    string ContractStatus,
    string JobId,
    string RequestId,
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
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionModelServerHttpErrorResponse(
    string Endpoint,
    string Method,
    string HttpSeam,
    int StatusCode,
    string ErrorCode,
    string ErrorMessage,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed class DesignConversionModelServerHttpHandler
{
    public const string EndpointPath = "/api/design-conversion/evaluate";
    public const string InternalEndpointPath = "/internal/design-conversion/evaluate";
    public const string LocalHttpSeam = "local_http_host_seam_no_socket_no_public_deployment";

    public static JsonSerializerOptions JsonOptions { get; } = CreateJsonOptions();

    private readonly DesignConversionModelServerApi _api;

    public DesignConversionModelServerHttpHandler(
        DesignConversionModelServerApi api,
        DesignConversionModelServerRouteAdmission? routeAdmission = null)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
        RouteAdmission = routeAdmission ?? DesignConversionModelServerRouteAdmission.Create(null);
    }

    public DesignConversionModelServerRouteAdmission RouteAdmission { get; }

    public async Task<DesignConversionModelServerHttpResponse> HandleAsync(
        DesignConversionModelServerHttpRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            return Error("POST", EndpointPath, 400, "REQUEST_REQUIRED", "DesignConversion HTTP request is required.");

        var method = string.IsNullOrWhiteSpace(request.Method) ? string.Empty : request.Method.Trim().ToUpperInvariant();
        var path = NormalizePath(request.Path);
        if (!IsEvaluateEndpointPath(path))
            return Error(method, path, 404, "ROUTE_NOT_FOUND", $"DesignConversion HTTP route is not registered: {path}");

        if (!string.Equals(method, "POST", StringComparison.Ordinal))
            return Error(method, path, 405, "METHOD_NOT_ALLOWED", "DesignConversion evaluate endpoint requires POST.");

        var admission = RouteAdmission.AdmitEvaluate(request);
        if (!admission.IsAccepted)
        {
            return Error(
                method,
                path,
                admission.StatusCode,
                admission.ErrorCode,
                admission.ErrorMessage,
                admission.BoundaryMetadata);
        }

        if (!IsJsonContentType(request.ContentType))
        {
            return Error(
                method,
                path,
                415,
                "UNSUPPORTED_MEDIA_TYPE",
                "DesignConversion evaluate endpoint requires application/json.",
                admission.BoundaryMetadata);
        }

        if (string.IsNullOrWhiteSpace(request.BodyJson))
        {
            return Error(
                method,
                path,
                400,
                "REQUEST_BODY_REQUIRED",
                "DesignConversion evaluate endpoint requires a JSON request body.",
                admission.BoundaryMetadata);
        }

        return await RouteAdmission.ExecuteEvaluateOnceAsync(
            admission,
            request.BodyJson,
            ct => HandleAcceptedAsync(method, path, request.BodyJson, admission.BoundaryMetadata, admission.AdmissionContext, ct),
            conflictAdmission => Error(
                method,
                path,
                409,
                "DESIGN_CONVERSION_IDEMPOTENCY_KEY_CONFLICT",
                "DesignConversion evaluate idempotency key was already used with a different canonical payload.",
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
        DesignConversionModelServerRouteAdmissionContext admissionContext,
        CancellationToken cancellationToken)
    {
        DesignConversionModelServerApiRequest? apiRequest;
        try
        {
            using var document = JsonDocument.Parse(bodyJson);
            var sourceKind = ValidateSourceKindIfPresent(document.RootElement);
            if (!sourceKind.IsSuccess)
            {
                return Error(
                    method,
                    path,
                    400,
                    sourceKind.ErrorCode ?? "SOURCE_KIND_UNSUPPORTED",
                    sourceKind.ErrorMessage ?? "DesignConversion evaluate source kind is not supported.",
                    admissionMetadata);
            }

            apiRequest = document.RootElement.Deserialize<DesignConversionModelServerApiRequest>(JsonOptions);
        }
        catch (JsonException)
        {
            return Error(
                method,
                path,
                400,
                "REQUEST_JSON_INVALID",
                "DesignConversion evaluate endpoint received invalid JSON.",
                admissionMetadata);
        }

        if (apiRequest is null)
            return Error(
                method,
                path,
                400,
                "REQUEST_BODY_REQUIRED",
                "DesignConversion evaluate endpoint received an empty JSON request.",
                admissionMetadata);

        var result = await _api.SubmitAndEvaluateAsync(apiRequest, admissionContext, cancellationToken);
        if (!result.IsSuccess)
        {
            return Error(
                method,
                path,
                StatusCodeForApiError(result.ErrorCode),
                result.ErrorCode ?? "DESIGN_CONVERSION_HTTP_EVALUATE_FAILED",
                result.ErrorMessage ?? "DesignConversion evaluate endpoint rejected the request.",
                admissionMetadata);
        }

        var value = result.Value!;
        var metadata = BuildBoundaryMetadata(
            method,
            path,
            200,
            runtimeInvoked: true,
            MergeBoundaryMetadata(admissionMetadata, value.BoundaryMetadata));
        var body = new DesignConversionModelServerHttpEvaluateResponse(
            Endpoint: path,
            Method: method,
            HttpSeam: LocalHttpSeam,
            ApiShape: value.ApiShape,
            RuntimeSurface: value.RuntimeSurface,
            StatusCode: 200,
            ContractStatus: value.ContractStatus,
            JobId: value.JobId,
            RequestId: value.RequestId,
            LearnedInferenceApplied: value.LearnedInferenceApplied,
            TrainedModelAuthority: value.TrainedModelAuthority,
            NotTrainedModelAuthority: value.NotTrainedModelAuthority,
            DeterministicLogicClaimedAsLearning: value.DeterministicLogicClaimedAsLearning,
            ModelTrainingVerified: value.ModelTrainingVerified,
            ModelUploadReady: value.ModelUploadReady,
            DeploymentVerified: value.DeploymentVerified,
            PublicHttpEndpointVerified: value.PublicHttpEndpointVerified,
            PlatformIntegrationVerified: value.PlatformIntegrationVerified,
            ProductReady: value.ProductReady,
            RuntimeStatus: value.RuntimeStatus,
            TrainingReadiness: value.TrainingReadiness,
            EvidenceRefs: value.EvidenceRefs,
            Warnings: value.Warnings,
            BoundaryMetadata: metadata);

        return Json(200, method, path, body);
    }

    private static DesignConversionModelServerHttpResponse Error(
        string method,
        string path,
        int statusCode,
        string errorCode,
        string errorMessage,
        IReadOnlyDictionary<string, string>? boundaryMetadata = null)
    {
        var body = new DesignConversionModelServerHttpErrorResponse(
            Endpoint: path,
            Method: string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method,
            HttpSeam: LocalHttpSeam,
            StatusCode: statusCode,
            ErrorCode: errorCode,
            ErrorMessage: errorMessage,
            BoundaryMetadata: BuildBoundaryMetadata(
                method,
                path,
                statusCode,
                runtimeInvoked: false,
                apiMetadata: boundaryMetadata));

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
            ["x-design-conversion-endpoint"] = NormalizePath(path),
            ["x-design-conversion-http-seam"] = LocalHttpSeam,
            ["x-design-conversion-public-deployment-verified"] = bool.FalseString,
            ["x-design-conversion-cross-repo-caller-verified"] = bool.FalseString,
            ["x-design-conversion-runtime-surface"] = DesignConversionModelServerApi.RuntimeSurface,
            ["x-design-conversion-method"] = string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method,
            ["x-design-conversion-path"] = NormalizePath(path),
            ["x-design-conversion-status-code"] = statusCode.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };

        return new DesignConversionModelServerHttpResponse(
            StatusCode: statusCode,
            ContentType: "application/json; charset=utf-8",
            BodyJson: JsonSerializer.Serialize(body, JsonOptions),
            Headers: headers);
    }

    private static IReadOnlyDictionary<string, string> BuildBoundaryMetadata(
        string method,
        string path,
        int statusCode,
        bool runtimeInvoked,
        IReadOnlyDictionary<string, string>? apiMetadata)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["http_endpoint_shape"] = $"{(string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method)} {NormalizePath(path)}",
            ["http_method"] = string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method,
            ["http_path"] = NormalizePath(path),
            ["http_status_code"] = statusCode.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["http_content_type"] = "application/json; charset=utf-8",
            ["local_http_host_seam"] = LocalHttpSeam,
            ["local_http_host_seam_verified"] = bool.TrueString,
            ["socket_opened"] = bool.FalseString,
            ["public_http_endpoint_verified"] = bool.FalseString,
            ["cross_repo_caller_verified"] = bool.FalseString,
            ["deployment_verified"] = bool.FalseString,
            ["product_ready"] = bool.FalseString,
            ["runtime_invoked"] = runtimeInvoked.ToString(),
            [DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey] =
                DesignConversionModelAuthorityBoundary.TrainedModelAuthorityValue,
            [DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey] =
                DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityValue,
            ["runtime_surface"] = DesignConversionModelServerApi.RuntimeSurface,
            ["api_shape"] = DesignConversionModelServerApi.ApiShape,
            ["caller_contract_schema_version"] = DesignConversionModelServerCallerContract.SchemaVersion,
            ["caller_contract_boundary"] = DesignConversionModelServerCallerContract.Boundary,
            ["caller_contract_required_fields"] = string.Join(",", DesignConversionModelServerCallerContract.RequiredRequestFieldNames()),
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["raw_user_private_data_included"] = bool.FalseString
        };

        foreach (var pair in DesignConversionModelServerCallerContract.BoundaryMetadata())
            metadata[pair.Key] = pair.Value;

        foreach (var pair in apiMetadata ?? new Dictionary<string, string>())
            metadata[pair.Key] = pair.Value;

        metadata["caller_contract_schema_version"] = DesignConversionModelServerCallerContract.SchemaVersion;
        metadata["caller_contract_boundary"] = DesignConversionModelServerCallerContract.Boundary;
        metadata["caller_contract_required_fields"] = string.Join(",", DesignConversionModelServerCallerContract.RequiredRequestFieldNames());
        metadata["local_http_host_seam"] = LocalHttpSeam;
        metadata["local_http_host_seam_verified"] = bool.TrueString;
        metadata["socket_opened"] = bool.FalseString;
        metadata["public_http_endpoint_verified"] = bool.FalseString;
        metadata["cross_repo_caller_verified"] = bool.FalseString;
        metadata["deployment_verified"] = bool.FalseString;
        metadata["product_ready"] = bool.FalseString;
        metadata["runtime_invoked"] = runtimeInvoked.ToString();
        metadata[DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey] =
            DesignConversionModelAuthorityBoundary.TrainedModelAuthorityValue;
        metadata[DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey] =
            DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityValue;

        return metadata;
    }

    private static IReadOnlyDictionary<string, string> MergeBoundaryMetadata(
        IReadOnlyDictionary<string, string>? left,
        IReadOnlyDictionary<string, string>? right)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in left ?? new Dictionary<string, string>())
            metadata[pair.Key] = pair.Value;
        foreach (var pair in right ?? new Dictionary<string, string>())
            metadata[pair.Key] = pair.Value;

        return metadata;
    }

    private static string NormalizePath(string? path)
        => string.IsNullOrWhiteSpace(path) ? "/" : path.Trim();

    private static bool IsEvaluateEndpointPath(string path)
        => string.Equals(path, EndpointPath, StringComparison.Ordinal) ||
           string.Equals(path, InternalEndpointPath, StringComparison.Ordinal);

    private static bool IsJsonContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return true;

        return contentType.Split(';', 2)[0].Trim().Equals("application/json", StringComparison.OrdinalIgnoreCase);
    }

    private static OperationResult<bool> ValidateSourceKindIfPresent(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object ||
            !root.TryGetProperty("sourceKind", out var sourceKind))
        {
            return OperationResult<bool>.Ok(true);
        }

        if (sourceKind.ValueKind == JsonValueKind.String)
        {
            var value = sourceKind.GetString();
            return Enum.TryParse<DesignConversionSourceKind>(value, ignoreCase: true, out var parsed) &&
                   Enum.IsDefined(typeof(DesignConversionSourceKind), parsed)
                ? OperationResult<bool>.Ok(true)
                : UnsupportedSourceKind(value);
        }

        if (sourceKind.ValueKind == JsonValueKind.Number &&
            sourceKind.TryGetInt32(out var numericValue))
        {
            var parsed = (DesignConversionSourceKind)numericValue;
            return Enum.IsDefined(typeof(DesignConversionSourceKind), parsed)
                ? OperationResult<bool>.Ok(true)
                : UnsupportedSourceKind(numericValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        return UnsupportedSourceKind(sourceKind.ValueKind.ToString());
    }

    private static OperationResult<bool> UnsupportedSourceKind(string? value)
        => OperationResult<bool>.Fail(
            $"DesignConversion evaluate source kind is not supported: {value ?? "UNKNOWN"}",
            "SOURCE_KIND_UNSUPPORTED");

    private static int StatusCodeForApiError(string? errorCode)
        => errorCode switch
        {
            "DESIGN_CONVERSION_ACTIVE_MODEL_TENANT_CONTEXT_MISMATCH" => 403,
            "DESIGN_CONVERSION_ACTIVE_MODEL_TRUSTED_TENANT_CONTEXT_INVALID" => 403,
            _ => 400
        };

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = false
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
