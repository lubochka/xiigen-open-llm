// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentInfrastructure;
using DesignConversionDomain;

public sealed record DesignConversionModelServerStatusArtifactRef(
    string ArtifactId,
    string Kind,
    string Uri,
    string Sha256,
    string MediaType,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignConversionModelServerStatusReadbackResponse(
    string Endpoint,
    string Method,
    string HttpSeam,
    string ApiShape,
    string RuntimeSurface,
    int StatusCode,
    string JobId,
    string RequestId,
    string ContractStatus,
    string JobStatus,
    string StatusUri,
    bool LearnedInferenceApplied,
    bool TrainedModelAuthority,
    bool NotTrainedModelAuthority,
    bool DeterministicLogicClaimedAsLearning,
    bool PublicHttpEndpointVerified,
    bool PlatformIntegrationVerified,
    bool DeploymentVerified,
    bool ProductReady,
    IReadOnlyList<DesignConversionModelServerStatusArtifactRef> Artifacts,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> Warnings,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed class DesignConversionModelServerStatusHandler
{
    public const string EndpointBasePath = "/api/design-conversion/status";
    public const string EndpointPathPrefix = EndpointBasePath + "/";
    public const string EndpointTemplatePath = EndpointPathPrefix + "{jobId}";
    public const string LocalStatusReadbackBoundary =
        "local_status_readback_route_refs_only_no_socket_no_public_deployment";

    private readonly DesignConversionModelServerApi _api;

    public DesignConversionModelServerStatusHandler(
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
            return Error("GET", EndpointTemplatePath, 400, "REQUEST_REQUIRED", "DesignConversion status/readback request is required.");

        var method = string.IsNullOrWhiteSpace(request.Method) ? string.Empty : request.Method.Trim().ToUpperInvariant();
        var path = NormalizePath(request.Path);
        if (!IsStatusReadbackPath(path))
            return Error(method, path, 404, "ROUTE_NOT_FOUND", $"DesignConversion status/readback route is not registered: {path}");

        if (!string.Equals(method, "GET", StringComparison.Ordinal))
            return Error(method, path, 405, "METHOD_NOT_ALLOWED", "DesignConversion status/readback endpoint requires GET.");

        var admission = RouteAdmission.AdmitStatus(request);
        if (!admission.IsAccepted)
        {
            return Error(
                method,
                EndpointTemplatePath,
                admission.StatusCode,
                admission.ErrorCode,
                admission.ErrorMessage,
                SanitizeStatusDenialAdmissionMetadata(admission.BoundaryMetadata));
        }

        var jobId = ExtractJobId(path);
        if (!RouteAdmission.IsStatusJobAllowed(admission, jobId))
        {
            return Error(
                method,
                EndpointTemplatePath,
                404,
                "JOB_NOT_FOUND",
                "DesignConversion status/readback rejected the job id.",
                SanitizeStatusDenialAdmissionMetadata(admission.BoundaryMetadata));
        }

        var result = await _api.GetJobResultAsync(jobId, cancellationToken);
        if (!result.IsSuccess)
        {
            return Error(
                method,
                path,
                StatusCodeForApiError(result.ErrorCode),
                result.ErrorCode ?? "DESIGN_CONVERSION_STATUS_READBACK_FAILED",
                result.ErrorMessage ?? "DesignConversion status/readback rejected the job id.",
                SanitizeStatusDenialAdmissionMetadata(admission.BoundaryMetadata));
        }

        var value = result.Value!;
        var runtimeStatus = value.RuntimeStatus;
        var metadata = BuildBoundaryMetadata(
            method,
            path,
            200,
            MergeBoundaryMetadata(admission.BoundaryMetadata, value.BoundaryMetadata),
            value.EvidenceRefs,
            runtimeStatus.Artifacts.Count);
        var warnings = runtimeStatus.Warnings
            .Concat([
                "status_readback_route_local_only=true",
                "job_rerun_invoked=false",
                "runtime_status_readback_invoked=true",
                "public_http_endpoint_verified=false",
                "platform_integration_verified=false",
                "deployment_verified=false",
                "product_ready=false"
            ])
            .Distinct(StringComparer.Ordinal)
            .OrderBy(warning => warning, StringComparer.Ordinal)
            .ToArray();
        var body = new DesignConversionModelServerStatusReadbackResponse(
            Endpoint: EndpointTemplatePath,
            Method: method,
            HttpSeam: DesignConversionModelServerHttpHandler.LocalHttpSeam,
            ApiShape: value.ApiShape,
            RuntimeSurface: DesignConversionModelServerApi.RuntimeSurface,
            StatusCode: 200,
            JobId: value.JobId,
            RequestId: runtimeStatus.Job.RequestId,
            ContractStatus: value.ContractStatus,
            JobStatus: runtimeStatus.Job.Status,
            StatusUri: runtimeStatus.Job.StatusUri,
            LearnedInferenceApplied: value.LearnedInferenceApplied,
            TrainedModelAuthority: value.TrainedModelAuthority,
            NotTrainedModelAuthority: value.NotTrainedModelAuthority,
            DeterministicLogicClaimedAsLearning: value.DeterministicLogicClaimedAsLearning,
            PublicHttpEndpointVerified: false,
            PlatformIntegrationVerified: false,
            DeploymentVerified: false,
            ProductReady: false,
            Artifacts: runtimeStatus.Artifacts.Select(ToResponseArtifact).ToArray(),
            EvidenceRefs: value.EvidenceRefs,
            Warnings: warnings,
            BoundaryMetadata: metadata);

        return Json(200, method, path, body);
    }

    private static DesignConversionModelServerStatusArtifactRef ToResponseArtifact(
        DesignConversionThinClientArtifactRef artifact)
        => new(
            ArtifactId: artifact.ArtifactId,
            Kind: artifact.Kind,
            Uri: artifact.Uri,
            Sha256: artifact.Sha256,
            MediaType: artifact.MediaType,
            Metadata: FilterArtifactMetadata(artifact.Metadata));

    private static IReadOnlyDictionary<string, string> FilterArtifactMetadata(IReadOnlyDictionary<string, string>? metadata)
    {
        var filtered = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in metadata ?? new Dictionary<string, string>())
        {
            if (IsSafeStatusMetadataKey(pair.Key) && IsSafeStatusMetadataValue(pair.Key, pair.Value))
                filtered[pair.Key] = pair.Value;
        }

        return filtered;
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
            Endpoint: EndpointTemplatePath,
            Method: string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method,
            HttpSeam: DesignConversionModelServerHttpHandler.LocalHttpSeam,
            StatusCode: statusCode,
            ErrorCode: errorCode,
            ErrorMessage: errorMessage,
            BoundaryMetadata: BuildBoundaryMetadata(
                method,
                path,
                statusCode,
                apiMetadata: boundaryMetadata,
                evidenceRefs: [],
                artifactCount: 0));

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
            ["x-design-conversion-endpoint"] = EndpointTemplatePath,
            ["x-design-conversion-http-seam"] = DesignConversionModelServerHttpHandler.LocalHttpSeam,
            ["x-design-conversion-status-readback-boundary"] = LocalStatusReadbackBoundary,
            ["x-design-conversion-public-deployment-verified"] = bool.FalseString,
            ["x-design-conversion-cross-repo-caller-verified"] = bool.FalseString,
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
        IReadOnlyDictionary<string, string>? apiMetadata,
        IReadOnlyList<string> evidenceRefs,
        int artifactCount)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["http_endpoint_shape"] = "GET /api/design-conversion/status/{jobId}",
            ["http_method"] = string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method,
            ["http_path"] = NormalizePath(path),
            ["http_status_code"] = statusCode.ToString(CultureInfo.InvariantCulture),
            ["http_content_type"] = "application/json; charset=utf-8",
            ["local_http_host_seam"] = DesignConversionModelServerHttpHandler.LocalHttpSeam,
            ["local_status_readback_boundary"] = LocalStatusReadbackBoundary,
            ["local_http_host_seam_verified"] = bool.TrueString,
            ["status_readback_route_local_only"] = bool.TrueString,
            ["runtime_status_readback_invoked"] = (statusCode == 200).ToString(),
            ["job_rerun_invoked"] = bool.FalseString,
            ["runtime_invoked"] = bool.FalseString,
            [DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey] =
                DesignConversionModelAuthorityBoundary.TrainedModelAuthorityValue,
            [DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey] =
                DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityValue,
            ["runtime_surface"] = DesignConversionModelServerApi.RuntimeSurface,
            ["api_shape"] = DesignConversionModelServerApi.ApiShape,
            ["status_readback_response_refs_only"] = bool.TrueString,
            ["status_readback_artifact_ref_count"] = artifactCount.ToString(CultureInfo.InvariantCulture),
            ["status_readback_evidence_ref_count"] = evidenceRefs.Count.ToString(CultureInfo.InvariantCulture),
            ["refs_or_hashes_only"] = bool.TrueString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["raw_user_private_data_included"] = bool.FalseString,
            ["response_contains_raw_private_data"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString,
            ["screenshot_bytes_included"] = bool.FalseString,
            ["external_provider_called"] = bool.FalseString,
            ["external_ai_visual_judge_executed"] = bool.FalseString,
            ["asp_net_host_verified"] = bool.FalseString,
            ["socket_opened"] = bool.FalseString,
            ["public_http_endpoint_verified"] = bool.FalseString,
            ["platform_integration_verified"] = bool.FalseString,
            ["cross_repo_caller_verified"] = bool.FalseString,
            ["deployment_verified"] = bool.FalseString,
            ["product_ready"] = bool.FalseString
        };

        foreach (var pair in DesignConversionModelServerCallerContract.BoundaryMetadata())
            metadata[pair.Key] = pair.Value;

        foreach (var pair in DesignConversionModelServerCallerContract.StatusReadbackBoundaryMetadata())
            metadata[pair.Key] = pair.Value;

        foreach (var pair in apiMetadata ?? new Dictionary<string, string>())
        {
            if (IsSafeStatusMetadataKey(pair.Key) && IsSafeStatusMetadataValue(pair.Key, pair.Value))
                metadata[pair.Key] = pair.Value;
        }

        metadata["runtime_status_readback_invoked"] = (statusCode == 200).ToString();
        metadata["job_rerun_invoked"] = bool.FalseString;
        metadata["runtime_invoked"] = bool.FalseString;
        metadata[DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey] =
            DesignConversionModelAuthorityBoundary.TrainedModelAuthorityValue;
        metadata[DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityKey] =
            DesignConversionModelAuthorityBoundary.NotTrainedModelAuthorityValue;
        metadata["refs_or_hashes_only"] = bool.TrueString;
        metadata["raw_html_included"] = bool.FalseString;
        metadata["raw_css_included"] = bool.FalseString;
        metadata["raw_design_tree_included"] = bool.FalseString;
        metadata["raw_user_private_data_included"] = bool.FalseString;
        metadata["response_contains_raw_private_data"] = bool.FalseString;
        metadata["external_provider_called"] =
            IsTruthyBoundary(metadata, DesignConversionModelServerNetworkBoundary.ServerOwnedVisualProviderNetworkCallVerifiedKey)
                .ToString();
        metadata["asp_net_host_verified"] = bool.FalseString;
        metadata["socket_opened"] = bool.FalseString;
        metadata["public_http_endpoint_verified"] = bool.FalseString;
        metadata["platform_integration_verified"] = bool.FalseString;
        metadata["cross_repo_caller_verified"] = bool.FalseString;
        metadata["deployment_verified"] = bool.FalseString;
        PreserveTrustedNetworkBoundaryMetadata(metadata);
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

    private static IReadOnlyDictionary<string, string> SanitizeStatusDenialAdmissionMetadata(
        IReadOnlyDictionary<string, string> metadata)
    {
        var sanitized = new Dictionary<string, string>(metadata, StringComparer.Ordinal)
        {
            ["route_admission_path"] = EndpointTemplatePath
        };

        return sanitized;
    }

    private static void PreserveTrustedNetworkBoundaryMetadata(IDictionary<string, string> metadata)
    {
        var jsonMetadata = new JsonObject();
        foreach (var pair in metadata)
            jsonMetadata[pair.Key] = pair.Value;

        DesignConversionModelServerNetworkBoundary.PreserveTrustedNetworkBoundaryMetadata(jsonMetadata);
        CopyJsonMetadataValue(jsonMetadata, metadata, DesignConversionModelServerNetworkBoundary.ServerOwnedVisualProviderNetworkCallVerifiedKey);
        CopyJsonMetadataValue(jsonMetadata, metadata, DesignConversionModelServerNetworkBoundary.ServerOwnedVisualProviderNetworkCallVerificationKey);
        CopyJsonMetadataValue(jsonMetadata, metadata, "network_call_executed");
        CopyJsonMetadataValue(jsonMetadata, metadata, "external_network_call_executed");
        CopyJsonMetadataValue(jsonMetadata, metadata, "external_provider_called");
        CopyJsonMetadataValue(jsonMetadata, metadata, "real_external_provider_executed");
    }

    private static void CopyJsonMetadataValue(
        JsonObject source,
        IDictionary<string, string> target,
        string key)
    {
        if (source.TryGetPropertyValue(key, out var value))
            target[key] = value?.GetValue<string>() ?? string.Empty;
    }

    private static int StatusCodeForApiError(string? errorCode)
        => errorCode switch
        {
            "JOB_ID_REQUIRED" => 400,
            "JOB_NOT_FOUND" => 404,
            _ => 400
        };

    private static string ExtractJobId(string path)
    {
        var normalized = NormalizePath(path);
        if (string.Equals(normalized, EndpointBasePath, StringComparison.Ordinal))
            return string.Empty;
        if (!normalized.StartsWith(EndpointPathPrefix, StringComparison.Ordinal))
            return string.Empty;

        var segment = normalized[EndpointPathPrefix.Length..];
        return Uri.UnescapeDataString(segment);
    }

    private static bool IsStatusReadbackPath(string path)
        => string.Equals(path, EndpointBasePath, StringComparison.Ordinal) ||
           path.StartsWith(EndpointPathPrefix, StringComparison.Ordinal);

    private static string NormalizePath(string? path)
        => string.IsNullOrWhiteSpace(path) ? "/" : path.Trim();

    private static bool IsSafeStatusMetadataKey(string key)
        => !string.IsNullOrWhiteSpace(key) &&
           (string.Equals(key, "idempotency_payload_digest_exposed", StringComparison.Ordinal) ||
            (!key.Contains("html", StringComparison.OrdinalIgnoreCase) &&
             !key.Contains("css", StringComparison.OrdinalIgnoreCase) &&
             !key.Contains("tree", StringComparison.OrdinalIgnoreCase) &&
             !key.Contains("payload", StringComparison.OrdinalIgnoreCase) &&
             !key.Contains("secret", StringComparison.OrdinalIgnoreCase) &&
             !key.Contains("private", StringComparison.OrdinalIgnoreCase)));

    private static bool IsSafeStatusMetadataValue(string key, string? value)
        => IsServerOwnedVisualProviderReceiptMetadataKey(key)
            ? IsSafeServerOwnedVisualProviderReceiptValue(value)
            : IsSafeStatusMetadataValue(value);

    private static bool IsSafeStatusMetadataValue(string? value)
        => string.IsNullOrWhiteSpace(value) ||
           (value.Length <= 160 &&
            !value.Contains("<html", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("<main", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("<div", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("{\"", StringComparison.Ordinal) &&
            !value.Contains("PRIVATE", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("SECRET", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("data-secret", StringComparison.OrdinalIgnoreCase));

    private static bool IsSafeServerOwnedVisualProviderReceiptValue(string? value)
        => string.IsNullOrWhiteSpace(value) ||
           (value.Length <= 512 &&
            !value.Contains("<html", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("<main", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("<div", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("{\"", StringComparison.Ordinal) &&
            !value.Contains("PRIVATE", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("SECRET", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("data-secret", StringComparison.OrdinalIgnoreCase));

    private static bool IsServerOwnedVisualProviderReceiptMetadataKey(string key)
        => string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVersionKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptIdKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJobIdKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJudgeRequestIdKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRequestIdKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRootRequestIdKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualJudgeRequestIdKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualResultIdKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderNameKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderModelKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptEndpointHostKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey, StringComparison.Ordinal) ||
           string.Equals(key, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey, StringComparison.Ordinal);

    private static bool IsTruthyBoundary(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) &&
           (string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "1", StringComparison.Ordinal) ||
            string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase));
}
