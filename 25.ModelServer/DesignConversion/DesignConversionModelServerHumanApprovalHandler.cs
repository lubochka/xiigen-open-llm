// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Globalization;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;

public sealed record DesignConversionModelServerHumanApprovalChallengeHttpRequest(
    string JobId,
    DesignConversionHumanApprovalActor Actor,
    int? TimeToLiveSeconds = null);

public sealed record DesignConversionModelServerHumanApprovalApproveHttpRequest(
    string ChallengeId,
    string JobId,
    DesignConversionHumanApprovalActor Actor,
    IReadOnlyList<string> AcknowledgedViewportIds,
    string OperationKey,
    string Reason);

public sealed record DesignConversionModelServerHumanApprovalChallengeResponse(
    string Endpoint,
    string Method,
    string HttpSeam,
    int StatusCode,
    string ChallengeId,
    string JobId,
    string RequestId,
    string TenantId,
    string SnapshotDigest,
    DateTimeOffset ExpiresAtUtc,
    IReadOnlyList<string> RequiredViewportIds,
    IReadOnlyList<DesignConversionHumanApprovalSnapshotRef> TargetScreenshotRefs,
    IReadOnlyList<DesignConversionHumanApprovalSnapshotRef> RenderedScreenshotRefs,
    IReadOnlyList<DesignConversionHumanApprovalSnapshotRef> ComparisonRefs,
    IReadOnlyList<DesignConversionHumanApprovalSnapshotRef> VisualJudgeRefs,
    bool ProductReady,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionModelServerHumanApprovalApproveResponse(
    string Endpoint,
    string Method,
    string HttpSeam,
    int StatusCode,
    string ApprovalId,
    string ChallengeId,
    string JobId,
    string RequestId,
    string TenantId,
    string ActorId,
    string SnapshotDigest,
    bool AcceptedForUser,
    bool RuntimeStateChanged,
    bool ProductReady,
    bool TrainedOrLearnedProvenance,
    bool DpoEligible,
    bool ModelWeightsPublished,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed class DesignConversionModelServerHumanApprovalHandler
{
    public const string ChallengeEndpointPath = "/api/design-conversion/human-approval/challenge";
    public const string ApproveEndpointPath = "/api/design-conversion/human-approval/approve";
    public const string HumanApprovalBoundary =
        "internal_modelserver_non_dpo_human_approval_no_anonymous_public_authority";

    private readonly DesignConversionModelServerApi? _api;
    private readonly string? _notReadyErrorCode;
    private readonly string? _notReadyReason;

    public DesignConversionModelServerHumanApprovalHandler(DesignConversionModelServerApi api)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
    }

    private DesignConversionModelServerHumanApprovalHandler(
        string notReadyErrorCode,
        string notReadyReason)
    {
        _notReadyErrorCode = SafeErrorCode(notReadyErrorCode);
        _notReadyReason = SafeScalar(notReadyReason, "human-approval-route-not-ready-reason");
    }

    public static DesignConversionModelServerHumanApprovalHandler CreateNotReady(
        string errorCode,
        string reason)
        => new(errorCode, reason);

    public async Task<DesignConversionModelServerHttpResponse> HandleAsync(
        DesignConversionModelServerHttpRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            return Error("POST", ChallengeEndpointPath, 400, "REQUEST_REQUIRED", "DesignConversion human approval request is required.");

        var method = string.IsNullOrWhiteSpace(request.Method) ? string.Empty : request.Method.Trim().ToUpperInvariant();
        var path = NormalizePath(request.Path);
        if (!IsHumanApprovalPath(path))
            return Error(method, path, 404, "ROUTE_NOT_FOUND", $"DesignConversion human approval route is not registered: {path}");
        if (!string.Equals(method, "POST", StringComparison.Ordinal))
            return Error(method, path, 405, "METHOD_NOT_ALLOWED", "DesignConversion human approval endpoints require POST.");
        if (!IsJsonContentType(request.ContentType))
            return Error(method, path, 415, "UNSUPPORTED_MEDIA_TYPE", "DesignConversion human approval endpoints require application/json.");
        if (_api is null)
        {
            return Error(
                method,
                path,
                503,
                _notReadyErrorCode ?? "DESIGN_CONVERSION_HUMAN_APPROVAL_ROUTE_NOT_READY",
                "DesignConversion human approval route is not ready; configuration is incomplete or invalid.",
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["human_approval_route_startup_configuration_status"] = "bounded_non_ready",
                    ["human_approval_route_not_ready_reason_ref"] = _notReadyReason ?? string.Empty,
                    ["human_approval_durable_store_configured"] = bool.FalseString,
                    ["runtime_invoked"] = bool.FalseString
                });
        }

        if (string.IsNullOrWhiteSpace(request.BodyJson))
            return Error(method, path, 400, "REQUEST_BODY_REQUIRED", "DesignConversion human approval endpoint requires a JSON request body.");

        var api = _api;
        return string.Equals(path, ChallengeEndpointPath, StringComparison.Ordinal)
            ? await HandleChallengeAsync(api, method, path, request.BodyJson!, cancellationToken)
            : await HandleApproveAsync(api, method, path, request.BodyJson!, cancellationToken);
    }

    private async Task<DesignConversionModelServerHttpResponse> HandleChallengeAsync(
        DesignConversionModelServerApi api,
        string method,
        string path,
        string bodyJson,
        CancellationToken cancellationToken)
    {
        DesignConversionModelServerHumanApprovalChallengeHttpRequest? body;
        try
        {
            body = JsonSerializer.Deserialize<DesignConversionModelServerHumanApprovalChallengeHttpRequest>(
                bodyJson,
                DesignConversionModelServerHttpHandler.JsonOptions);
        }
        catch (JsonException)
        {
            return Error(method, path, 400, "REQUEST_JSON_INVALID", "DesignConversion human approval challenge received invalid JSON.");
        }

        if (body is null)
            return Error(method, path, 400, "REQUEST_BODY_REQUIRED", "DesignConversion human approval challenge received an empty JSON body.");

        var ttl = body.TimeToLiveSeconds is > 0
            ? TimeSpan.FromSeconds(body.TimeToLiveSeconds.Value)
            : (TimeSpan?)null;
        var result = await api.IssueHumanApprovalChallengeAsync(
            new DesignConversionHumanApprovalChallengeIssueRequest(
                body.JobId,
                body.Actor,
                ttl),
            cancellationToken);
        if (!result.IsSuccess)
            return Error(method, path, StatusCodeForApiError(result.ErrorCode), result.ErrorCode, result.ErrorMessage);

        var challenge = result.Value!;
        var metadata = BuildBoundaryMetadata(method, path, 200, challenge.BoundaryMetadata);
        var response = new DesignConversionModelServerHumanApprovalChallengeResponse(
            Endpoint: ChallengeEndpointPath,
            Method: method,
            HttpSeam: DesignConversionModelServerHttpHandler.LocalHttpSeam,
            StatusCode: 200,
            ChallengeId: challenge.ChallengeId,
            JobId: challenge.JobId,
            RequestId: challenge.RequestId,
            TenantId: challenge.TenantId,
            SnapshotDigest: challenge.SnapshotDigest,
            ExpiresAtUtc: challenge.ExpiresAtUtc,
            RequiredViewportIds: challenge.RequiredViewportIds,
            TargetScreenshotRefs: challenge.TargetScreenshotRefs,
            RenderedScreenshotRefs: challenge.RenderedScreenshotRefs,
            ComparisonRefs: challenge.ComparisonRefs,
            VisualJudgeRefs: challenge.VisualJudgeRefs,
            ProductReady: false,
            BoundaryMetadata: metadata);

        return Json(200, method, path, response);
    }

    private async Task<DesignConversionModelServerHttpResponse> HandleApproveAsync(
        DesignConversionModelServerApi api,
        string method,
        string path,
        string bodyJson,
        CancellationToken cancellationToken)
    {
        DesignConversionModelServerHumanApprovalApproveHttpRequest? body;
        try
        {
            body = JsonSerializer.Deserialize<DesignConversionModelServerHumanApprovalApproveHttpRequest>(
                bodyJson,
                DesignConversionModelServerHttpHandler.JsonOptions);
        }
        catch (JsonException)
        {
            return Error(method, path, 400, "REQUEST_JSON_INVALID", "DesignConversion human approval approve received invalid JSON.");
        }

        if (body is null)
            return Error(method, path, 400, "REQUEST_BODY_REQUIRED", "DesignConversion human approval approve received an empty JSON body.");

        var result = await api.ApproveHumanConversionAsync(
            new DesignConversionHumanApprovalDecisionRequest(
                body.ChallengeId,
                body.JobId,
                body.Actor,
                body.AcknowledgedViewportIds,
                body.OperationKey,
                body.Reason),
            cancellationToken);
        if (!result.IsSuccess)
            return Error(method, path, StatusCodeForApiError(result.ErrorCode), result.ErrorCode, result.ErrorMessage);

        var value = result.Value!;
        var record = value.Record;
        var metadata = BuildBoundaryMetadata(method, path, 200, value.BoundaryMetadata);
        var response = new DesignConversionModelServerHumanApprovalApproveResponse(
            Endpoint: ApproveEndpointPath,
            Method: method,
            HttpSeam: DesignConversionModelServerHttpHandler.LocalHttpSeam,
            StatusCode: 200,
            ApprovalId: record.ApprovalId,
            ChallengeId: record.ChallengeId,
            JobId: record.JobId,
            RequestId: record.RequestId,
            TenantId: record.TenantId,
            ActorId: record.ActorId,
            SnapshotDigest: record.SnapshotDigest,
            AcceptedForUser: record.AcceptedForUser,
            RuntimeStateChanged: value.RuntimeStateChanged,
            ProductReady: record.ProductReady,
            TrainedOrLearnedProvenance: record.TrainedOrLearnedProvenance,
            DpoEligible: record.DpoEligible,
            ModelWeightsPublished: record.ModelWeightsPublished,
            EvidenceRefs: value.EvidenceRefs,
            BoundaryMetadata: metadata);

        return Json(200, method, path, response);
    }

    public static int StatusCodeForApiError(string? errorCode)
        => errorCode switch
        {
            "JOB_NOT_FOUND" or "DESIGN_CONVERSION_APPROVAL_CHALLENGE_NOT_FOUND" => 404,
            "DESIGN_CONVERSION_APPROVAL_AUTHORIZATION_DENIED" => 403,
            "DESIGN_CONVERSION_APPROVAL_CHALLENGE_EXPIRED" => 410,
            "DESIGN_CONVERSION_APPROVAL_STORE_UNAVAILABLE" => 503,
            "DESIGN_CONVERSION_APPROVAL_OPERATION_REPLAYED" or
                "DESIGN_CONVERSION_APPROVAL_CHALLENGE_REPLAYED" or
                "DESIGN_CONVERSION_APPROVAL_SNAPSHOT_STALE" => 409,
            "CANCELLED" => 499,
            _ => 400
        };

    private static DesignConversionModelServerHttpResponse Error(
        string method,
        string path,
        int statusCode,
        string? errorCode,
        string? errorMessage,
        IReadOnlyDictionary<string, string>? boundaryMetadata = null)
    {
        var body = new DesignConversionModelServerHttpErrorResponse(
            Endpoint: IsHumanApprovalPath(path) ? path : ChallengeEndpointPath,
            Method: string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method,
            HttpSeam: DesignConversionModelServerHttpHandler.LocalHttpSeam,
            StatusCode: statusCode,
            ErrorCode: SafeErrorCode(string.IsNullOrWhiteSpace(errorCode) ? "DESIGN_CONVERSION_HUMAN_APPROVAL_FAILED" : errorCode),
            ErrorMessage: SafeScalar(
                string.IsNullOrWhiteSpace(errorMessage) ? "DesignConversion human approval request failed." : errorMessage,
                "human-approval-error-message"),
            BoundaryMetadata: BuildBoundaryMetadata(method, path, statusCode, boundaryMetadata));

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
            ["x-design-conversion-http-seam"] = DesignConversionModelServerHttpHandler.LocalHttpSeam,
            ["x-design-conversion-human-approval-boundary"] = HumanApprovalBoundary,
            ["x-design-conversion-public-deployment-verified"] = bool.FalseString,
            ["x-design-conversion-cross-repo-caller-verified"] = bool.FalseString,
            ["x-design-conversion-anonymous-public-authority"] = bool.FalseString,
            ["x-design-conversion-runtime-surface"] = DesignConversionModelServerApi.RuntimeSurface,
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
        IReadOnlyDictionary<string, string>? apiMetadata)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["http_endpoint_shape"] = "POST /api/design-conversion/human-approval/{challenge|approve}",
            ["http_method"] = string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method,
            ["http_path"] = NormalizePath(path),
            ["http_status_code"] = statusCode.ToString(CultureInfo.InvariantCulture),
            ["http_content_type"] = "application/json; charset=utf-8",
            ["local_http_host_seam"] = DesignConversionModelServerHttpHandler.LocalHttpSeam,
            ["human_approval_boundary"] = HumanApprovalBoundary,
            ["typed_tenant_actor_authorization_required"] = bool.TrueString,
            ["human_approval_internal_route_only"] = bool.TrueString,
            ["anonymous_public_authority"] = bool.FalseString,
            ["external_platform_auth_boundary"] = "external_to_modelserver_actor_context_required",
            ["runtime_invoked"] = bool.FalseString,
            ["refs_only_response_required"] = bool.TrueString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["screenshot_bytes_included"] = bool.FalseString,
            ["token_or_secret_included"] = bool.FalseString,
            [DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey] =
                DesignConversionModelAuthorityBoundary.TrainedModelAuthorityValue,
            [DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey] =
                DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityValue,
            ["dpo_eligible"] = bool.FalseString,
            ["model_weights_published"] = bool.FalseString,
            ["public_http_endpoint_verified"] = bool.FalseString,
            ["platform_integration_verified"] = bool.FalseString,
            ["cross_repo_caller_verified"] = bool.FalseString,
            ["deployment_verified"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };

        foreach (var pair in apiMetadata ?? new Dictionary<string, string>())
            metadata[pair.Key] = pair.Value;

        metadata["anonymous_public_authority"] = bool.FalseString;
        metadata["product_ready"] = bool.FalseString;
        metadata[DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey] =
            DesignConversionModelAuthorityBoundary.TrainedModelAuthorityValue;
        metadata[DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey] =
            DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityValue;
        metadata["dpo_eligible"] = bool.FalseString;
        metadata["model_weights_published"] = bool.FalseString;
        return metadata;
    }

    private static string SafeErrorCode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "DESIGN_CONVERSION_HUMAN_APPROVAL_FAILED";

        var normalized = new string(value.Trim()
            .Select(character => char.IsAsciiLetterOrDigit(character) || character == '_' || character == '-'
                ? character
                : '_')
            .ToArray());
        return string.IsNullOrWhiteSpace(normalized)
            ? "DESIGN_CONVERSION_HUMAN_APPROVAL_FAILED"
            : normalized;
    }

    private static string SafeScalar(string? value, string kind)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var trimmed = value.Trim();
        if (trimmed.Contains("<", StringComparison.Ordinal) ||
            trimmed.Contains("{", StringComparison.Ordinal) ||
            trimmed.Contains("C:/", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Contains("C:\\", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Contains("file://", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Contains("private", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Contains("token", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Contains("raw_provider_payload", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Contains("provider_payload", StringComparison.OrdinalIgnoreCase))
        {
            return $"{kind}:{StableShortHash(trimmed)}";
        }

        return trimmed;
    }

    private static string StableShortHash(string value)
        => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant()[..16];

    private static bool IsHumanApprovalPath(string path)
        => string.Equals(NormalizePath(path), ChallengeEndpointPath, StringComparison.Ordinal) ||
           string.Equals(NormalizePath(path), ApproveEndpointPath, StringComparison.Ordinal);

    private static string NormalizePath(string? path)
        => string.IsNullOrWhiteSpace(path) ? "/" : path.Trim();

    private static bool IsJsonContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return true;

        return contentType.Split(';', 2)[0].Trim().Equals("application/json", StringComparison.OrdinalIgnoreCase);
    }
}
