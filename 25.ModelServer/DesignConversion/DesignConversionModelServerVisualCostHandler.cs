// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Globalization;
using System.Text.Json;
using DesignConversionDomain;

public sealed record DesignConversionModelServerVisualCostReadbackResponse(
    string Endpoint,
    string Method,
    string HttpSeam,
    string ApiShape,
    string RuntimeSurface,
    int StatusCode,
    DesignConversionModelServerVisualCostSummary Summary,
    IReadOnlyList<DesignConversionVisualCostLedgerRecord> Records,
    bool PublicHttpEndpointVerified,
    bool PlatformIntegrationVerified,
    bool DeploymentVerified,
    bool ProductReady,
    IReadOnlyList<string> Warnings,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed class DesignConversionModelServerVisualCostHandler
{
    public const string EndpointBasePath = "/api/design-conversion/jobs";
    public const string EndpointPathPrefix = EndpointBasePath + "/";
    public const string EndpointPathSuffix = "/visual-costs";
    public const string EndpointTemplatePath = EndpointPathPrefix + "{jobId}" + EndpointPathSuffix;
    public const string LocalVisualCostReadbackBoundary =
        "local_visual_cost_readback_route_privacy_safe_no_socket_no_public_deployment";
    private static readonly HashSet<string> SafeVisualCostMetadataKeys = new(StringComparer.Ordinal)
    {
        "route_admission_enabled",
        "route_admission_configuration_status",
        "route_admission_authorized",
        "route_admission_header_name",
        "route_admission_project_header_name",
        "route_admission_token_supplied",
        "route_admission_token_accepted",
        "route_admission_previous_token_configured",
        "route_admission_previous_token_active",
        "route_admission_accepted_key_count",
        "route_admission_rotation_state",
        "route_admission_error_ref",
        "route_admission_status_code",
        "route_admission_method",
        "idempotency_header_name",
        "idempotency_required",
        "idempotency_key_accepted",
        "idempotency_cache_capacity",
        "idempotency_cache_ttl_seconds",
        "idempotency_ttl_seconds",
        "idempotency_failure_replay_policy",
        "idempotency_durable_replay_enabled",
        "idempotency_durable_replay_store_state",
        "route_admission_max_concurrent_workloads",
        "idempotency_payload_digest_exposed",
        "runtime_visual_cost_records_readback_invoked",
        "runtime_surface",
        "api_shape",
        "visual_cost_record_count",
        "visual_cost_total_cost_usd",
        "visual_cost_measurement_complete",
        "visual_cost_missing_measurement_count",
        "visual_cost_evaluation_attempt_count",
        "visual_cost_source",
        "visual_cost_response_refs_only",
        "visual_cost_unknown_job_empty_success",
        "refs_or_hashes_only",
        "raw_route_job_id_included",
        "raw_job_id_included",
        "raw_request_id_included",
        "raw_html_included",
        "raw_css_included",
        "raw_design_tree_included",
        "provider_payload_included",
        "screenshot_bytes_included",
        "filesystem_root_included",
        "sensitive_principal_data_included",
        "response_contains_raw_private_data",
        "external_provider_called",
        "public_http_endpoint_verified",
        "platform_integration_verified",
        "cross_repo_caller_verified",
        "deployment_verified",
        "product_ready"
    };

    private readonly DesignConversionModelServerApi _api;

    public DesignConversionModelServerVisualCostHandler(
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
            return Error("GET", 400, "REQUEST_REQUIRED", "DesignConversion visual-cost readback request is required.");

        var method = string.IsNullOrWhiteSpace(request.Method) ? string.Empty : request.Method.Trim().ToUpperInvariant();
        var path = NormalizePath(request.Path);
        if (!IsVisualCostReadbackPath(path))
            return Error(method, 404, "ROUTE_NOT_FOUND", "DesignConversion visual-cost readback route is not registered.");

        if (!string.Equals(method, "GET", StringComparison.Ordinal))
            return Error(method, 405, "METHOD_NOT_ALLOWED", "DesignConversion visual-cost readback endpoint requires GET.");

        var admission = RouteAdmission.AdmitStatus(request);
        if (!admission.IsAccepted)
        {
            return Error(
                method,
                admission.StatusCode,
                admission.ErrorCode,
                admission.ErrorMessage,
                SanitizeVisualCostAdmissionMetadata(admission.BoundaryMetadata));
        }

        var jobId = ExtractJobId(path);
        if (!jobId.IsSuccess)
            return Error(
                method,
                400,
                jobId.ErrorCode,
                jobId.ErrorMessage,
                SanitizeVisualCostAdmissionMetadata(admission.BoundaryMetadata));

        if (!RouteAdmission.IsStatusJobAllowed(admission, jobId.Value!))
        {
            return Error(
                method,
                404,
                "JOB_NOT_FOUND",
                "DesignConversion visual-cost readback rejected the job id.",
                SanitizeVisualCostAdmissionMetadata(admission.BoundaryMetadata));
        }

        var result = await _api.GetVisualCostRecordsAsync(jobId.Value!, cancellationToken);
        if (!result.IsSuccess)
        {
            return Error(
                method,
                StatusCodeForApiError(result.ErrorCode),
                result.ErrorCode ?? "DESIGN_CONVERSION_VISUAL_COST_READBACK_FAILED",
                result.ErrorMessage ?? "DesignConversion visual-cost records could not be read for the requested job.",
                SanitizeVisualCostAdmissionMetadata(admission.BoundaryMetadata),
                runtimeReadbackInvoked: true);
        }

        var value = result.Value!;
        var metadata = BuildBoundaryMetadata(
            method,
            200,
            MergeBoundaryMetadata(
                SanitizeVisualCostAdmissionMetadata(admission.BoundaryMetadata),
                value.BoundaryMetadata),
            value.Summary,
            runtimeReadbackInvoked: true);
        var warnings = new[]
            {
                "visual_cost_readback_route_local_only=true",
                "runtime_visual_cost_records_readback_invoked=true",
                "public_http_endpoint_verified=false",
                "platform_integration_verified=false",
                "deployment_verified=false",
                "product_ready=false"
            }
            .Distinct(StringComparer.Ordinal)
            .OrderBy(warning => warning, StringComparer.Ordinal)
            .ToArray();
        var body = new DesignConversionModelServerVisualCostReadbackResponse(
            Endpoint: EndpointTemplatePath,
            Method: method,
            HttpSeam: DesignConversionModelServerHttpHandler.LocalHttpSeam,
            ApiShape: value.ApiShape,
            RuntimeSurface: value.RuntimeSurface,
            StatusCode: 200,
            Summary: value.Summary,
            Records: value.Records,
            PublicHttpEndpointVerified: false,
            PlatformIntegrationVerified: false,
            DeploymentVerified: false,
            ProductReady: false,
            Warnings: warnings,
            BoundaryMetadata: metadata);

        return Json(200, method, body);
    }

    public static bool IsVisualCostReadbackPath(string path)
    {
        var normalized = NormalizePath(path);
        if (!normalized.StartsWith(EndpointPathPrefix, StringComparison.Ordinal) ||
            !normalized.EndsWith(EndpointPathSuffix, StringComparison.Ordinal))
        {
            return false;
        }

        var segmentLength = normalized.Length - EndpointPathPrefix.Length - EndpointPathSuffix.Length;
        return segmentLength >= 0;
    }

    public static bool IsConcreteVisualCostReadbackPath(string path)
    {
        var normalized = NormalizePath(path);
        if (!IsVisualCostReadbackPath(normalized))
            return false;

        var segment = normalized.Substring(
            EndpointPathPrefix.Length,
            normalized.Length - EndpointPathPrefix.Length - EndpointPathSuffix.Length);
        return !string.IsNullOrWhiteSpace(segment);
    }

    private static DesignConversionJobIdParseResult ExtractJobId(string path)
    {
        var normalized = NormalizePath(path);
        if (!IsVisualCostReadbackPath(normalized))
            return DesignConversionJobIdParseResult.Fail(
                "JOB_ID_INVALID",
                "DesignConversion visual-cost readback route job id is malformed.");

        var segment = normalized.Substring(
            EndpointPathPrefix.Length,
            normalized.Length - EndpointPathPrefix.Length - EndpointPathSuffix.Length);
        if (segment.Length == 0)
            return DesignConversionJobIdParseResult.Fail(
                "JOB_ID_REQUIRED",
                "DesignConversion visual-cost readback route job id is required.");
        if (string.IsNullOrWhiteSpace(segment))
            return DesignConversionJobIdParseResult.Fail(
                "JOB_ID_INVALID",
                "DesignConversion visual-cost readback route job id is malformed.");

        string decoded;
        try
        {
            decoded = Uri.UnescapeDataString(segment);
        }
        catch (UriFormatException)
        {
            return DesignConversionJobIdParseResult.Fail(
                "JOB_ID_INVALID",
                "DesignConversion visual-cost readback route job id is malformed.");
        }

        if (string.IsNullOrWhiteSpace(decoded) ||
            decoded.Contains('/', StringComparison.Ordinal) ||
            decoded.Contains('\\', StringComparison.Ordinal) ||
            decoded.Contains('?', StringComparison.Ordinal) ||
            decoded.Contains('#', StringComparison.Ordinal))
        {
            return DesignConversionJobIdParseResult.Fail(
                "JOB_ID_INVALID",
                "DesignConversion visual-cost readback route job id is malformed.");
        }

        return DesignConversionJobIdParseResult.Ok(decoded.Trim());
    }

    private static DesignConversionModelServerHttpResponse Error(
        string method,
        int statusCode,
        string errorCode,
        string errorMessage,
        IReadOnlyDictionary<string, string>? boundaryMetadata = null,
        bool runtimeReadbackInvoked = false)
    {
        var body = new DesignConversionModelServerHttpErrorResponse(
            Endpoint: EndpointTemplatePath,
            Method: string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method,
            HttpSeam: DesignConversionModelServerHttpHandler.LocalHttpSeam,
            StatusCode: statusCode,
            ErrorCode: errorCode,
            ErrorMessage: errorMessage,
            BoundaryMetadata: BuildBoundaryMetadata(
                method,
                statusCode,
                boundaryMetadata,
                new DesignConversionModelServerVisualCostSummary(
                    RecordCount: 0,
                    TotalCostUsd: 0m,
                    MeasurementComplete: false,
                    MissingMeasurementCount: 0,
                    EvaluationAttemptCount: 0,
                    CostSource: DesignConversionVisualCostAccounting.UnknownCostSource),
                runtimeReadbackInvoked));

        return Json(statusCode, method, body);
    }

    private static DesignConversionModelServerHttpResponse Json<T>(
        int statusCode,
        string method,
        T body)
    {
        var headers = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Cache-Control"] = "no-store, no-cache, max-age=0",
            ["Pragma"] = "no-cache",
            ["Expires"] = "0",
            ["x-design-conversion-endpoint"] = EndpointTemplatePath,
            ["x-design-conversion-http-seam"] = DesignConversionModelServerHttpHandler.LocalHttpSeam,
            ["x-design-conversion-visual-cost-readback-boundary"] = LocalVisualCostReadbackBoundary,
            ["x-design-conversion-public-deployment-verified"] = bool.FalseString,
            ["x-design-conversion-cross-repo-caller-verified"] = bool.FalseString,
            ["x-design-conversion-runtime-surface"] = DesignConversionModelServerApi.RuntimeSurface,
            ["x-design-conversion-method"] = string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method,
            ["x-design-conversion-path"] = EndpointTemplatePath,
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
        int statusCode,
        IReadOnlyDictionary<string, string>? apiMetadata,
        DesignConversionModelServerVisualCostSummary summary,
        bool runtimeReadbackInvoked)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["http_endpoint_shape"] = "GET " + EndpointTemplatePath,
            ["http_method"] = string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method,
            ["http_path"] = EndpointTemplatePath,
            ["http_status_code"] = statusCode.ToString(CultureInfo.InvariantCulture),
            ["http_content_type"] = "application/json; charset=utf-8",
            ["cache_control"] = "no-store, no-cache, max-age=0",
            ["pragma"] = "no-cache",
            ["expires"] = "0",
            ["local_http_host_seam"] = DesignConversionModelServerHttpHandler.LocalHttpSeam,
            ["local_visual_cost_readback_boundary"] = LocalVisualCostReadbackBoundary,
            ["local_http_host_seam_verified"] = bool.TrueString,
            ["visual_cost_readback_route_local_only"] = bool.TrueString,
            ["runtime_visual_cost_records_readback_invoked"] = runtimeReadbackInvoked.ToString(),
            ["runtime_invoked"] = bool.FalseString,
            ["runtime_surface"] = DesignConversionModelServerApi.RuntimeSurface,
            ["api_shape"] = DesignConversionModelServerApi.ApiShape,
            ["visual_cost_response_refs_only"] = bool.TrueString,
            ["visual_cost_record_count"] = summary.RecordCount.ToString(CultureInfo.InvariantCulture),
            ["visual_cost_total_cost_usd"] = FormatVisualCostUsd(summary.TotalCostUsd),
            ["visual_cost_measurement_complete"] = summary.MeasurementComplete.ToString(),
            ["visual_cost_missing_measurement_count"] = summary.MissingMeasurementCount.ToString(CultureInfo.InvariantCulture),
            ["visual_cost_evaluation_attempt_count"] = summary.EvaluationAttemptCount.ToString(CultureInfo.InvariantCulture),
            ["visual_cost_source"] = summary.CostSource,
            ["visual_cost_unknown_job_empty_success"] = (statusCode == 200 && summary.RecordCount == 0).ToString(),
            ["refs_or_hashes_only"] = bool.TrueString,
            ["raw_route_job_id_included"] = bool.FalseString,
            ["raw_job_id_included"] = bool.FalseString,
            ["raw_request_id_included"] = bool.FalseString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString,
            ["screenshot_bytes_included"] = bool.FalseString,
            ["filesystem_root_included"] = bool.FalseString,
            ["sensitive_principal_data_included"] = bool.FalseString,
            ["response_contains_raw_private_data"] = bool.FalseString,
            ["external_provider_called"] = bool.FalseString,
            ["public_http_endpoint_verified"] = bool.FalseString,
            ["platform_integration_verified"] = bool.FalseString,
            ["cross_repo_caller_verified"] = bool.FalseString,
            ["deployment_verified"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };

        foreach (var pair in apiMetadata ?? new Dictionary<string, string>())
        {
            if (IsSafeVisualCostMetadataKey(pair.Key) && IsSafeVisualCostMetadataValue(pair.Value))
                metadata[pair.Key] = pair.Value;
        }

        metadata["http_path"] = EndpointTemplatePath;
        metadata["route_admission_path"] = EndpointTemplatePath;
        metadata["runtime_visual_cost_records_readback_invoked"] = runtimeReadbackInvoked.ToString();
        metadata["runtime_invoked"] = bool.FalseString;
        metadata["visual_cost_response_refs_only"] = bool.TrueString;
        metadata["visual_cost_record_count"] = summary.RecordCount.ToString(CultureInfo.InvariantCulture);
        metadata["visual_cost_total_cost_usd"] = FormatVisualCostUsd(summary.TotalCostUsd);
        metadata["visual_cost_measurement_complete"] = summary.MeasurementComplete.ToString();
        metadata["visual_cost_missing_measurement_count"] = summary.MissingMeasurementCount.ToString(CultureInfo.InvariantCulture);
        metadata["visual_cost_evaluation_attempt_count"] = summary.EvaluationAttemptCount.ToString(CultureInfo.InvariantCulture);
        metadata["visual_cost_source"] = summary.CostSource;
        metadata["visual_cost_unknown_job_empty_success"] = (statusCode == 200 && summary.RecordCount == 0).ToString();
        metadata["raw_route_job_id_included"] = bool.FalseString;
        metadata["raw_job_id_included"] = bool.FalseString;
        metadata["raw_request_id_included"] = bool.FalseString;
        metadata["raw_html_included"] = bool.FalseString;
        metadata["raw_css_included"] = bool.FalseString;
        metadata["raw_design_tree_included"] = bool.FalseString;
        metadata["provider_payload_included"] = bool.FalseString;
        metadata["filesystem_root_included"] = bool.FalseString;
        metadata["sensitive_principal_data_included"] = bool.FalseString;
        metadata["response_contains_raw_private_data"] = bool.FalseString;
        metadata["public_http_endpoint_verified"] = bool.FalseString;
        metadata["platform_integration_verified"] = bool.FalseString;
        metadata["cross_repo_caller_verified"] = bool.FalseString;
        metadata["deployment_verified"] = bool.FalseString;
        metadata["product_ready"] = bool.FalseString;
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

    private static IReadOnlyDictionary<string, string> SanitizeVisualCostAdmissionMetadata(
        IReadOnlyDictionary<string, string> metadata)
    {
        var sanitized = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["route_admission_path"] = EndpointTemplatePath
        };

        CopyIfPresent(metadata, sanitized, "route_admission_enabled");
        CopyIfPresent(metadata, sanitized, "route_admission_configuration_status");
        CopyIfPresent(metadata, sanitized, "route_admission_authorized");
        CopyIfPresent(metadata, sanitized, "route_admission_header_name");
        CopyIfPresent(metadata, sanitized, "route_admission_project_header_name");
        CopyIfPresent(metadata, sanitized, "route_admission_token_supplied");
        CopyIfPresent(metadata, sanitized, "route_admission_token_accepted");
        CopyIfPresent(metadata, sanitized, "route_admission_previous_token_configured");
        CopyIfPresent(metadata, sanitized, "route_admission_previous_token_active");
        CopyIfPresent(metadata, sanitized, "route_admission_accepted_key_count");
        CopyIfPresent(metadata, sanitized, "route_admission_rotation_state");
        CopyIfPresent(metadata, sanitized, "route_admission_error_ref");
        CopyIfPresent(metadata, sanitized, "route_admission_status_code");
        CopyIfPresent(metadata, sanitized, "route_admission_method");
        CopyIfPresent(metadata, sanitized, "idempotency_header_name");
        CopyIfPresent(metadata, sanitized, "idempotency_required");
        CopyIfPresent(metadata, sanitized, "idempotency_key_accepted");
        CopyIfPresent(metadata, sanitized, "idempotency_cache_capacity");
        CopyIfPresent(metadata, sanitized, "idempotency_cache_ttl_seconds");
        CopyIfPresent(metadata, sanitized, "idempotency_ttl_seconds");
        CopyIfPresent(metadata, sanitized, "idempotency_failure_replay_policy");
        CopyIfPresent(metadata, sanitized, "idempotency_durable_replay_enabled");
        CopyIfPresent(metadata, sanitized, "idempotency_durable_replay_store_state");
        CopyIfPresent(metadata, sanitized, "route_admission_max_concurrent_workloads");
        CopyIfPresent(metadata, sanitized, "idempotency_payload_digest_exposed");

        return sanitized;
    }

    private static void CopyIfPresent(
        IReadOnlyDictionary<string, string> source,
        IDictionary<string, string> target,
        string key)
    {
        if (source.TryGetValue(key, out var value) && IsSafeVisualCostMetadataValue(value))
            target[key] = value;
    }

    private static int StatusCodeForApiError(string? errorCode)
        => errorCode switch
        {
            "JOB_ID_REQUIRED" => 400,
            "CANCELLED" => 499,
            DesignConversionThinClientJobRuntime.VisualCostRecordsReadFailedErrorCode => 503,
            JsonlDesignConversionVisualCostLedger.ReadFailedErrorCode => 503,
            _ => 503
        };

    private static string FormatVisualCostUsd(decimal amount)
        => amount.ToString("G29", CultureInfo.InvariantCulture);

    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "/";

        var trimmed = path.Trim();
        var queryIndex = trimmed.IndexOf('?', StringComparison.Ordinal);
        return queryIndex >= 0 ? trimmed[..queryIndex] : trimmed;
    }

    private static bool IsSafeVisualCostMetadataKey(string key)
        => !string.IsNullOrWhiteSpace(key) && SafeVisualCostMetadataKeys.Contains(key);

    private static bool IsSafeVisualCostMetadataValue(string? value)
        => string.IsNullOrWhiteSpace(value) ||
           (value.Length <= 160 &&
            !value.Contains("<html", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("<main", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("<div", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("{\"", StringComparison.Ordinal) &&
            !value.Contains("PRIVATE", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("SECRET", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("Bearer ", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("data-secret", StringComparison.OrdinalIgnoreCase) &&
            !Path.IsPathFullyQualified(value));

    private sealed record DesignConversionJobIdParseResult(
        bool IsSuccess,
        string? Value,
        string ErrorCode,
        string ErrorMessage)
    {
        public static DesignConversionJobIdParseResult Ok(string value)
            => new(true, value, string.Empty, string.Empty);

        public static DesignConversionJobIdParseResult Fail(string errorCode, string errorMessage)
            => new(false, null, errorCode, errorMessage);
    }
}
