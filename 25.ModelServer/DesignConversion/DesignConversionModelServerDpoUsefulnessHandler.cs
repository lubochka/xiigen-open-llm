// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Globalization;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;

public sealed record DesignConversionModelServerDpoUsefulnessResponse(
    string Endpoint,
    string Method,
    string HttpSeam,
    string ApiShape,
    string RuntimeSurface,
    int StatusCode,
    string ContractStatus,
    string DpoUsefulnessApiShape,
    IReadOnlyList<DesignConversionDpoUsefulnessApiRow> Rows,
    int RowCount,
    int TrainReadyCount,
    int QuarantinedCount,
    int ProviderMissingCount,
    int RejectedNegativeQualityCount,
    int HeldOutCount,
    int BlockedCount,
    double? AverageUsefulnessScore,
    double? MinimumUsefulnessScore,
    double? MaximumUsefulnessScore,
    bool ProductReady,
    IReadOnlyList<string> Warnings,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed class DesignConversionModelServerDpoUsefulnessHandler
{
    public const string EndpointPath = "/api/design-conversion/dpo-usefulness";
    public const string ApiShape = "design_conversion_modelserver_dpo_usefulness_route_v1";
    public const string RuntimeSurface = "modelserver_design_conversion_dpo_usefulness_route";
    public const string LocalDpoUsefulnessRouteBoundary =
        "local_dpo_usefulness_route_refs_only_no_socket_no_public_deployment";

    private readonly DesignConversionDpoUsefulnessApi _api;

    public DesignConversionModelServerDpoUsefulnessHandler(DesignConversionDpoUsefulnessApi api)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
    }

    public async Task<DesignConversionModelServerHttpResponse> HandleAsync(
        DesignConversionModelServerHttpRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            return Error("GET", EndpointPath, 400, "REQUEST_REQUIRED", "DesignConversion DPO usefulness request is required.");

        var method = string.IsNullOrWhiteSpace(request.Method) ? string.Empty : request.Method.Trim().ToUpperInvariant();
        var path = NormalizePath(request.Path);
        if (!string.Equals(path, EndpointPath, StringComparison.Ordinal))
            return Error(method, path, 404, "ROUTE_NOT_FOUND", $"DesignConversion DPO usefulness route is not registered: {path}");

        if (!string.Equals(method, "GET", StringComparison.Ordinal))
            return Error(method, path, 405, "METHOD_NOT_ALLOWED", "DesignConversion DPO usefulness endpoint requires GET.");

        var apiRequest = ToApiRequest(request);
        if (!apiRequest.IsSuccess)
        {
            return Error(
                method,
                path,
                400,
                apiRequest.ErrorCode ?? "DPO_USEFULNESS_QUERY_INVALID",
                apiRequest.ErrorMessage ?? "DesignConversion DPO usefulness query is invalid.");
        }

        var result = await _api.QueryAsync(apiRequest.Value!, cancellationToken);
        if (!result.IsSuccess)
        {
            return Error(
                method,
                path,
                400,
                result.ErrorCode ?? "DPO_USEFULNESS_QUERY_FAILED",
                result.ErrorMessage ?? "DesignConversion DPO usefulness API rejected the request.");
        }

        var value = result.Value!;
        var metadata = BuildBoundaryMetadata(method, path, 200, true, value.BoundaryMetadata, value);
        var body = new DesignConversionModelServerDpoUsefulnessResponse(
            Endpoint: EndpointPath,
            Method: method,
            HttpSeam: DesignConversionModelServerHttpHandler.LocalHttpSeam,
            ApiShape: ApiShape,
            RuntimeSurface: RuntimeSurface,
            StatusCode: 200,
            ContractStatus: "dpo-usefulness-readback-completed",
            DpoUsefulnessApiShape: value.ApiShape,
            Rows: value.Rows,
            RowCount: value.RowCount,
            TrainReadyCount: value.TrainReadyCount,
            QuarantinedCount: value.QuarantinedCount,
            ProviderMissingCount: value.ProviderMissingCount,
            RejectedNegativeQualityCount: value.RejectedNegativeQualityCount,
            HeldOutCount: value.HeldOutCount,
            BlockedCount: value.BlockedCount,
            AverageUsefulnessScore: value.AverageUsefulnessScore,
            MinimumUsefulnessScore: value.MinimumUsefulnessScore,
            MaximumUsefulnessScore: value.MaximumUsefulnessScore,
            ProductReady: false,
            Warnings: value.Warnings
                .Concat([
                    "dpo_usefulness_route_local_only=true",
                    "modelserver_route_delegates_to_domain_dpo_usefulness_api",
                    "public_http_endpoint_verified=false",
                    "deployment_verified=false",
                    "product_ready=false"
                ])
                .Distinct(StringComparer.Ordinal)
                .OrderBy(warning => warning, StringComparer.Ordinal)
                .ToArray(),
            BoundaryMetadata: metadata);

        return Json(200, method, path, body);
    }

    private static OperationResult<DesignConversionDpoUsefulnessApiRequest> ToApiRequest(
        DesignConversionModelServerHttpRequest request)
    {
        var query = ParseQuery(request);
        if (!query.IsSuccess)
            return query.MapError<DesignConversionDpoUsefulnessApiRequest>();

        var values = query.Value!;
        if (!TryReadBool(values, "includeBlocked", defaultValue: true, out var includeBlocked, out var boolError))
            return InvalidQuery(boolError);
        if (!TryReadDouble(values, "minimumUsefulnessScore", out var minimum, out var minimumError))
            return InvalidQuery(minimumError);
        if (!TryReadDouble(values, "maximumUsefulnessScore", out var maximum, out var maximumError))
            return InvalidQuery(maximumError);
        if (!TryReadInt(values, "limit", out var limit, out var limitError))
            return InvalidQuery(limitError);

        return OperationResult<DesignConversionDpoUsefulnessApiRequest>.Ok(new DesignConversionDpoUsefulnessApiRequest(
            UnitId: ReadOptional(values, "unitId"),
            IncludeBlocked: includeBlocked,
            MinimumUsefulnessScore: minimum,
            MaximumUsefulnessScore: maximum,
            Limit: limit));
    }

    private static OperationResult<Dictionary<string, string>> ParseQuery(
        DesignConversionModelServerHttpRequest request)
    {
        var query = request.QueryString;
        if (string.IsNullOrWhiteSpace(query))
        {
            var question = request.Path.IndexOf('?', StringComparison.Ordinal);
            if (question >= 0)
                query = request.Path[(question + 1)..];
        }

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(query))
            return OperationResult<Dictionary<string, string>>.Ok(values);

        foreach (var part in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = part.IndexOf('=', StringComparison.Ordinal);
            var rawKey = separator >= 0 ? part[..separator] : part;
            if (string.IsNullOrWhiteSpace(rawKey))
                continue;

            var rawValue = separator >= 0 ? part[(separator + 1)..] : string.Empty;
            var key = Uri.UnescapeDataString(rawKey.Replace("+", " ", StringComparison.Ordinal));
            var value = Uri.UnescapeDataString(rawValue.Replace("+", " ", StringComparison.Ordinal));
            values[key] = value;
        }

        return OperationResult<Dictionary<string, string>>.Ok(values);
    }

    private static string? ReadOptional(IReadOnlyDictionary<string, string> values, string key)
        => values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : null;

    private static bool TryReadBool(
        IReadOnlyDictionary<string, string> values,
        string key,
        bool defaultValue,
        out bool parsed,
        out string error)
    {
        error = string.Empty;
        parsed = defaultValue;
        if (!values.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            return true;

        if (bool.TryParse(value, out parsed))
            return true;

        error = $"{key} must be true or false.";
        return false;
    }

    private static bool TryReadDouble(
        IReadOnlyDictionary<string, string> values,
        string key,
        out double? parsed,
        out string error)
    {
        error = string.Empty;
        parsed = null;
        if (!values.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            return true;

        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
        {
            parsed = number;
            return true;
        }

        error = $"{key} must be a number.";
        return false;
    }

    private static bool TryReadInt(
        IReadOnlyDictionary<string, string> values,
        string key,
        out int? parsed,
        out string error)
    {
        error = string.Empty;
        parsed = null;
        if (!values.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            return true;

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
        {
            parsed = number;
            return true;
        }

        error = $"{key} must be an integer.";
        return false;
    }

    private static OperationResult<DesignConversionDpoUsefulnessApiRequest> InvalidQuery(string error)
        => OperationResult<DesignConversionDpoUsefulnessApiRequest>.Fail(
            string.IsNullOrWhiteSpace(error) ? "DesignConversion DPO usefulness query is invalid." : error,
            "DPO_USEFULNESS_QUERY_INVALID");

    private static DesignConversionModelServerHttpResponse Error(
        string method,
        string path,
        int statusCode,
        string errorCode,
        string errorMessage)
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
                apiMetadata: null,
                apiResult: null));

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
            ["x-design-conversion-dpo-usefulness-route"] = LocalDpoUsefulnessRouteBoundary,
            ["x-design-conversion-public-deployment-verified"] = bool.FalseString,
            ["x-design-conversion-cross-repo-caller-verified"] = bool.FalseString,
            ["x-design-conversion-runtime-surface"] = RuntimeSurface,
            ["x-design-conversion-method"] = string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method,
            ["x-design-conversion-path"] = NormalizePath(path),
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
        IReadOnlyDictionary<string, string>? apiMetadata,
        DesignConversionDpoUsefulnessApiResult? apiResult)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["http_endpoint_shape"] = "GET /api/design-conversion/dpo-usefulness",
            ["http_method"] = string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method,
            ["http_path"] = NormalizePath(path),
            ["http_status_code"] = statusCode.ToString(CultureInfo.InvariantCulture),
            ["http_content_type"] = "application/json; charset=utf-8",
            ["local_http_host_seam"] = DesignConversionModelServerHttpHandler.LocalHttpSeam,
            ["local_dpo_usefulness_route_boundary"] = LocalDpoUsefulnessRouteBoundary,
            ["local_http_host_seam_verified"] = bool.TrueString,
            ["runtime_invoked"] = runtimeInvoked.ToString(),
            ["runtime_surface"] = RuntimeSurface,
            ["api_shape"] = ApiShape,
            ["dpo_usefulness_api_shape"] = DesignConversionDpoUsefulnessApi.ApiShape,
            ["dpo_usefulness_runtime_surface"] = DesignConversionDpoUsefulnessApi.RuntimeSurface,
            ["delegates_to_domain_api"] = nameof(DesignConversionDpoUsefulnessApi),
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
            ["platform_integration_verified"] = bool.FalseString,
            ["cross_repo_caller_verified"] = bool.FalseString,
            ["deployment_verified"] = bool.FalseString,
            ["public_production_default_ready"] = bool.FalseString,
            ["product_ready"] = bool.FalseString,
            ["production_db_ready"] = bool.FalseString,
            ["trained_model_ready"] = bool.FalseString,
            ["model_readiness_claimed"] = bool.FalseString
        };

        foreach (var pair in DesignConversionModelServerCallerContract.DpoUsefulnessBoundaryMetadata())
            metadata[pair.Key] = pair.Value;

        foreach (var pair in apiMetadata ?? new Dictionary<string, string>())
            metadata[pair.Key] = pair.Value;

        if (apiResult is not null)
        {
            metadata["dpo_usefulness_row_count"] = apiResult.RowCount.ToString(CultureInfo.InvariantCulture);
            metadata["dpo_usefulness_train_ready_count"] = apiResult.TrainReadyCount.ToString(CultureInfo.InvariantCulture);
            metadata["dpo_usefulness_held_out_count"] = apiResult.HeldOutCount.ToString(CultureInfo.InvariantCulture);
            metadata["dpo_usefulness_blocked_count"] = apiResult.BlockedCount.ToString(CultureInfo.InvariantCulture);
            metadata["dpo_usefulness_provider_missing_count"] = apiResult.ProviderMissingCount.ToString(CultureInfo.InvariantCulture);
            metadata["dpo_usefulness_average_usefulness_score"] = FormatUsefulnessScore(apiResult.AverageUsefulnessScore);
            metadata["dpo_usefulness_minimum_usefulness_score"] = FormatUsefulnessScore(apiResult.MinimumUsefulnessScore);
            metadata["dpo_usefulness_maximum_usefulness_score"] = FormatUsefulnessScore(apiResult.MaximumUsefulnessScore);
        }

        metadata["runtime_invoked"] = runtimeInvoked.ToString();
        metadata["refs_or_hashes_only"] = bool.TrueString;
        metadata["refs_only_public_response"] = bool.TrueString;
        metadata["raw_html_included"] = bool.FalseString;
        metadata["raw_css_included"] = bool.FalseString;
        metadata["raw_design_tree_included"] = bool.FalseString;
        metadata["raw_user_private_data_included"] = bool.FalseString;
        metadata["response_contains_raw_private_data"] = bool.FalseString;
        metadata["provider_payload_included"] = bool.FalseString;
        metadata["external_provider_called"] = bool.FalseString;
        metadata["external_ai_visual_judge_executed"] = bool.FalseString;
        metadata["network_call_executed"] = bool.FalseString;
        metadata["asp_net_host_verified"] = bool.FalseString;
        metadata["socket_opened"] = bool.FalseString;
        metadata["public_http_endpoint_verified"] = bool.FalseString;
        metadata["platform_integration_verified"] = bool.FalseString;
        metadata["cross_repo_caller_verified"] = bool.FalseString;
        metadata["deployment_verified"] = bool.FalseString;
        metadata["public_production_default_ready"] = bool.FalseString;
        metadata["product_ready"] = bool.FalseString;
        return metadata;
    }

    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "/";

        var trimmed = path.Trim();
        var queryIndex = trimmed.IndexOf('?', StringComparison.Ordinal);
        return queryIndex >= 0 ? trimmed[..queryIndex] : trimmed;
    }

    private static string FormatUsefulnessScore(double? value)
        => value is null
            ? "none"
            : value.Value.ToString("0.######", CultureInfo.InvariantCulture);
}
