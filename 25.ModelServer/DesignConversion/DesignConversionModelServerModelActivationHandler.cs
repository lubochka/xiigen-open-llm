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

// Публичный selector намеренно содержит только безопасные ссылки и хэши:
// вызывающий выбирает уже опубликованную локальную запись, но не приносит
// filesystem paths, Publication/Candidate или другой private payload.
public sealed record DesignConversionModelServerLocalPromotionActivationSelector(
    string PromotionUploadRef,
    string PromotedModelId,
    string Version,
    string CheckpointSha256);

public sealed record DesignConversionModelServerModelActivationRequest(
    string Action,
    string? TenantId = null,
    string? RequestedByRef = null,
    string? Reason = null,
    string? IdempotencyKey = null,
    long? ExpectedGeneration = null,
    DesignConversionPromotedModelActivationRecord? Candidate = null,
    string? ExpectedModelVersion = null,
    DateTimeOffset? RequestedAtUtc = null,
    DesignConversionModelServerLocalPromotionActivationSelector? LocalPromotionSelector = null);

public sealed record DesignConversionModelServerModelActivationVersionRef(
    string ActivationRef,
    string ModelId,
    string ManifestId,
    string ManifestRef,
    string PromotionUploadRef,
    string Version,
    string MachineId,
    string DomainTag,
    string CheckpointPathRef,
    string CheckpointSha256,
    string LocalCheckpointWeightsSource,
    bool CheckpointFreshLoadVerified,
    bool CheckpointContainsRealTrainedWeights,
    bool LocalOnly,
    IReadOnlyList<string> EvidenceRefs);

public sealed record DesignConversionModelServerModelActivationResponse(
    string Endpoint,
    string Method,
    string HttpSeam,
    string ApiShape,
    string RuntimeSurface,
    int StatusCode,
    string ContractStatus,
    string Action,
    string OperationKind,
    bool Applied,
    bool Replay,
    long Generation,
    string TenantIdRef,
    bool ProductReady,
    DesignConversionModelServerModelActivationVersionRef ActiveVersion,
    DesignConversionModelServerModelActivationVersionRef? PreviousVersion,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> Warnings,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed class DesignConversionModelServerModelActivationHandler
{
    public const string EndpointPath = "/api/design-conversion/model-activation";
    public const string ApiShape = "design_conversion_modelserver_model_activation_route_v1";
    public const string RuntimeSurface = "modelserver_design_conversion_model_activation_route";
    public const string LocalModelActivationRouteBoundary =
        "local_model_activation_route_internal_token_required_refs_only_no_public_anonymous_control";

    public const string ActionActivate = "activate";
    public const string ActionActivateLocalPromotion = "activate-local-promotion";
    public const string ActionRollback = "rollback";
    public const string ActionReadActive = "read-active";

    private readonly DesignConversionModelServerModelActivationApi? _api;
    private readonly string? _notReadyErrorCode;
    private readonly string? _notReadyReason;

    public DesignConversionModelServerModelActivationHandler(
        DesignConversionModelServerModelActivationApi api)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
    }

    private DesignConversionModelServerModelActivationHandler(
        string notReadyErrorCode,
        string notReadyReason)
    {
        _notReadyErrorCode = string.IsNullOrWhiteSpace(notReadyErrorCode)
            ? "DESIGN_CONVERSION_MODEL_ACTIVATION_ROUTE_NOT_READY"
            : notReadyErrorCode.Trim();
        _notReadyReason = SafeScalar(notReadyReason, "activation-route-not-ready-reason");
    }

    public static DesignConversionModelServerModelActivationHandler CreateNotReady(
        string errorCode,
        string reason)
        => new(errorCode, reason);

    public async Task<DesignConversionModelServerHttpResponse> HandleAsync(
        DesignConversionModelServerHttpRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            return Error("POST", EndpointPath, 400, "REQUEST_REQUIRED", "DesignConversion model activation request is required.");

        var method = string.IsNullOrWhiteSpace(request.Method) ? string.Empty : request.Method.Trim().ToUpperInvariant();
        var path = NormalizePath(request.Path);
        if (!string.Equals(path, EndpointPath, StringComparison.Ordinal))
            return Error(method, path, 404, "ROUTE_NOT_FOUND", "DesignConversion model activation route is not registered.");

        if (!string.Equals(method, "POST", StringComparison.Ordinal))
            return Error(method, path, 405, "METHOD_NOT_ALLOWED", "DesignConversion model activation endpoint requires POST.");

        if (!IsJsonContentType(request.ContentType))
            return Error(method, path, 415, "UNSUPPORTED_MEDIA_TYPE", "DesignConversion model activation endpoint requires application/json.");

        if (_api is null)
        {
            return Error(
                method,
                path,
                503,
                _notReadyErrorCode ?? "DESIGN_CONVERSION_MODEL_ACTIVATION_ROUTE_NOT_READY",
                "DesignConversion model activation route is not ready; configuration is incomplete or invalid.",
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["model_activation_route_startup_configuration_status"] = "bounded_non_ready",
                    ["model_activation_route_not_ready_reason_ref"] = _notReadyReason ?? string.Empty,
                    ["runtime_invoked"] = bool.FalseString
                });
        }

        var parsed = ParseBody(request.BodyJson);
        if (!parsed.IsSuccess)
        {
            return Error(
                method,
                path,
                400,
                parsed.ErrorCode ?? "MODEL_ACTIVATION_REQUEST_INVALID",
                parsed.ErrorMessage ?? "DesignConversion model activation request is invalid.");
        }

        var routeRequest = parsed.Value!;
        var action = NormalizeAction(routeRequest.Action);
        if (string.IsNullOrWhiteSpace(action))
            return Error(method, path, 400, "MODEL_ACTIVATION_ACTION_REQUIRED", "DesignConversion model activation action is required.");

        var token = ReadHeader(request.Headers, DesignConversionModelServerModelActivationApi.LocalAuthorizationHeaderName);
        OperationResult<DesignConversionModelActivationResult> result;
        switch (action)
        {
            case ActionActivate:
                var activationAuthorization = BuildAuthorization(routeRequest, token);
                if (!activationAuthorization.IsSuccess)
                    return Error(method, path, 400, activationAuthorization.ErrorCode!, activationAuthorization.ErrorMessage!);
                if (routeRequest.Candidate is null)
                    return Error(method, path, 400, "MODEL_ACTIVATION_CANDIDATE_REQUIRED", "DesignConversion model activation requires a promoted model activation record.");

                result = await _api.ActivatePublishedModelAsync(
                    activationAuthorization.Value!,
                    routeRequest.Candidate,
                    routeRequest.ExpectedModelVersion,
                    routeRequest.RequestedAtUtc,
                    cancellationToken);
                break;

            case ActionActivateLocalPromotion:
                var localPromotionAuthorization = BuildAuthorization(routeRequest, token);
                if (!localPromotionAuthorization.IsSuccess)
                    return Error(method, path, 400, localPromotionAuthorization.ErrorCode!, localPromotionAuthorization.ErrorMessage!);
                if (routeRequest.Candidate is not null)
                    return Error(method, path, 400, "MODEL_ACTIVATION_LOCAL_PROMOTION_CANDIDATE_FORBIDDEN", "DesignConversion local-promotion activation accepts only refs-only selector, not caller-supplied candidate records.");
                if (routeRequest.LocalPromotionSelector is null)
                    return Error(method, path, 400, "MODEL_ACTIVATION_LOCAL_PROMOTION_SELECTOR_REQUIRED", "DesignConversion local-promotion activation requires a refs-only local promotion selector.");

                result = await _api.ActivateLocalPromotionAsync(
                    localPromotionAuthorization.Value!,
                    routeRequest.LocalPromotionSelector,
                    routeRequest.ExpectedModelVersion,
                    routeRequest.RequestedAtUtc,
                    cancellationToken);
                break;

            case ActionRollback:
                var rollbackAuthorization = BuildAuthorization(routeRequest, token);
                if (!rollbackAuthorization.IsSuccess)
                    return Error(method, path, 400, rollbackAuthorization.ErrorCode!, rollbackAuthorization.ErrorMessage!);

                result = await _api.RollbackActiveModelAsync(
                    rollbackAuthorization.Value!,
                    routeRequest.RequestedAtUtc,
                    cancellationToken);
                break;

            case ActionReadActive:
                if (string.IsNullOrWhiteSpace(routeRequest.TenantId))
                    return Error(method, path, 400, "MODEL_ACTIVATION_TENANT_ID_REQUIRED", "DesignConversion model activation read requires tenantId.");

                result = await _api.ReadActiveModelAsync(
                    new DesignConversionModelServerModelActivationReadAuthorization(
                        routeRequest.TenantId.Trim(),
                        token),
                    cancellationToken);
                break;

            default:
                return Error(method, path, 400, "MODEL_ACTIVATION_ACTION_UNSUPPORTED", "DesignConversion model activation action is not supported.");
        }

        if (!result.IsSuccess)
        {
            return Error(
                method,
                path,
                StatusCodeForApiError(action, result.ErrorCode),
                result.ErrorCode ?? "MODEL_ACTIVATION_DOMAIN_API_FAILED",
                SafeDomainFailureMessage(action),
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["model_activation_action"] = action,
                    ["model_activation_domain_error_code"] = result.ErrorCode ?? string.Empty,
                    ["runtime_invoked"] = bool.FalseString
                });
        }

        return Success(method, path, action, result.Value!);
    }

    private static OperationResult<DesignConversionModelServerModelActivationAuthorization> BuildAuthorization(
        DesignConversionModelServerModelActivationRequest request,
        string? localAuthorizationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TenantId))
            return OperationResult<DesignConversionModelServerModelActivationAuthorization>.Fail(
                "DesignConversion model activation requires tenantId.",
                "MODEL_ACTIVATION_TENANT_ID_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.RequestedByRef))
            return OperationResult<DesignConversionModelServerModelActivationAuthorization>.Fail(
                "DesignConversion model activation requires requestedByRef.",
                "MODEL_ACTIVATION_REQUESTED_BY_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.Reason))
            return OperationResult<DesignConversionModelServerModelActivationAuthorization>.Fail(
                "DesignConversion model activation requires reason.",
                "MODEL_ACTIVATION_REASON_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
            return OperationResult<DesignConversionModelServerModelActivationAuthorization>.Fail(
                "DesignConversion model activation requires idempotencyKey.",
                "MODEL_ACTIVATION_IDEMPOTENCY_KEY_REQUIRED");
        if (request.ExpectedGeneration is null || request.ExpectedGeneration < 0)
            return OperationResult<DesignConversionModelServerModelActivationAuthorization>.Fail(
                "DesignConversion model activation requires a non-negative expectedGeneration.",
                "MODEL_ACTIVATION_EXPECTED_GENERATION_REQUIRED");

        return OperationResult<DesignConversionModelServerModelActivationAuthorization>.Ok(
            new DesignConversionModelServerModelActivationAuthorization(
                request.TenantId.Trim(),
                SafeScalar(request.RequestedByRef, "activation-requested-by"),
                request.Reason.Trim(),
                request.IdempotencyKey.Trim(),
                request.ExpectedGeneration.Value,
                localAuthorizationToken));
    }

    private static OperationResult<DesignConversionModelServerModelActivationRequest> ParseBody(string? bodyJson)
    {
        if (string.IsNullOrWhiteSpace(bodyJson))
            return OperationResult<DesignConversionModelServerModelActivationRequest>.Fail(
                "DesignConversion model activation endpoint requires a JSON request body.",
                "REQUEST_BODY_REQUIRED");

        try
        {
            using var document = JsonDocument.Parse(bodyJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return OperationResult<DesignConversionModelServerModelActivationRequest>.Fail(
                    "DesignConversion model activation request body must be a JSON object.",
                    "REQUEST_JSON_INVALID");
            }

            var request = document.RootElement.Deserialize<DesignConversionModelServerModelActivationRequest>(
                DesignConversionModelServerHttpHandler.JsonOptions);
            return request is null
                ? OperationResult<DesignConversionModelServerModelActivationRequest>.Fail(
                    "DesignConversion model activation request body is empty.",
                    "REQUEST_BODY_REQUIRED")
                : OperationResult<DesignConversionModelServerModelActivationRequest>.Ok(request);
        }
        catch (JsonException)
        {
            return OperationResult<DesignConversionModelServerModelActivationRequest>.Fail(
                "DesignConversion model activation endpoint received invalid JSON.",
                "REQUEST_JSON_INVALID");
        }
    }

    private static DesignConversionModelServerHttpResponse Success(
        string method,
        string path,
        string action,
        DesignConversionModelActivationResult result)
    {
        var metadata = BuildBoundaryMetadata(
            method,
            path,
            200,
            runtimeInvoked: true,
            domainMetadata: result.BoundaryMetadata);
        metadata["model_activation_action"] = action;
        metadata["model_activation_applied"] = result.Applied.ToString();
        metadata["model_activation_replay"] = result.Replay.ToString();
        metadata["model_activation_generation"] = result.Generation.ToString(CultureInfo.InvariantCulture);

        var body = new DesignConversionModelServerModelActivationResponse(
            Endpoint: EndpointPath,
            Method: method,
            HttpSeam: DesignConversionModelServerHttpHandler.LocalHttpSeam,
            ApiShape: ApiShape,
            RuntimeSurface: RuntimeSurface,
            StatusCode: 200,
            ContractStatus: "model-activation-completed",
            Action: action,
            OperationKind: result.OperationKind,
            Applied: result.Applied,
            Replay: result.Replay,
            Generation: result.Generation,
            TenantIdRef: result.TenantIdRef,
            ProductReady: false,
            ActiveVersion: ToVersionRef(result.ActiveVersion),
            PreviousVersion: result.PreviousVersion is null ? null : ToVersionRef(result.PreviousVersion),
            EvidenceRefs: SafeList(result.EvidenceRefs, "model-activation-evidence"),
            Warnings: SafeList(result.Warnings, "model-activation-warning")
                .Append("model_activation_route_internal_token_required")
                .Append("product_ready=false")
                .Distinct(StringComparer.Ordinal)
                .OrderBy(warning => warning, StringComparer.Ordinal)
                .ToArray(),
            BoundaryMetadata: metadata);

        return Json(200, method, path, body);
    }

    private static DesignConversionModelServerModelActivationVersionRef ToVersionRef(
        DesignConversionActivatedModelVersion version)
        => new(
            ActivationRef: SafeScalar(version.ActivationRef, "activation-ref"),
            ModelId: SafeScalar(version.ModelId, "model-id"),
            ManifestId: SafeScalar(version.ManifestId, "manifest-id"),
            ManifestRef: SafeScalar(version.ManifestRef, "manifest-ref"),
            PromotionUploadRef: SafeScalar(version.PromotionUploadRef, "promotion-upload-ref"),
            Version: SafeScalar(version.Version, "model-version"),
            MachineId: SafeScalar(version.MachineId, "machine-id"),
            DomainTag: SafeScalar(version.DomainTag, "domain-tag"),
            CheckpointPathRef: SafeScalar(version.CheckpointPathRef, "checkpoint-path-ref"),
            CheckpointSha256: SafeScalar(version.CheckpointSha256, "checkpoint-sha256"),
            LocalCheckpointWeightsSource: SafeScalar(version.LocalCheckpointWeightsSource, "local-checkpoint-weights-source"),
            CheckpointFreshLoadVerified: version.CheckpointFreshLoadVerified,
            CheckpointContainsRealTrainedWeights: version.CheckpointContainsRealTrainedWeights,
            LocalOnly: version.LocalOnly,
            EvidenceRefs: SafeList(version.EvidenceRefs, "model-activation-version-evidence"));

    private static DesignConversionModelServerHttpResponse Error(
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
            ErrorCode: SafeErrorCode(errorCode),
            ErrorMessage: SafeScalar(errorMessage, "model-activation-error-message"),
            BoundaryMetadata: BuildBoundaryMetadata(
                method,
                path,
                statusCode,
                runtimeInvoked: false,
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
            ["x-design-conversion-model-activation-route"] = LocalModelActivationRouteBoundary,
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

    private static Dictionary<string, string> BuildBoundaryMetadata(
        string method,
        string path,
        int statusCode,
        bool runtimeInvoked,
        IReadOnlyDictionary<string, string>? domainMetadata)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["http_endpoint_shape"] = "POST /api/design-conversion/model-activation",
            ["http_method"] = string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method,
            ["http_path"] = NormalizePath(path),
            ["http_status_code"] = statusCode.ToString(CultureInfo.InvariantCulture),
            ["http_content_type"] = "application/json; charset=utf-8",
            ["local_http_host_seam"] = DesignConversionModelServerHttpHandler.LocalHttpSeam,
            ["local_model_activation_route_boundary"] = LocalModelActivationRouteBoundary,
            ["model_activation_contract_schema_version"] = ApiShape,
            ["model_activation_route_local_only"] = bool.TrueString,
            ["model_activation_internal_token_required"] = bool.TrueString,
            ["model_activation_public_anonymous_control"] = bool.FalseString,
            ["model_activation_paths_returned_as_hash_refs"] = bool.TrueString,
            ["model_activation_checkpoints_deleted"] = bool.FalseString,
            ["runtime_invoked"] = runtimeInvoked.ToString(),
            ["runtime_surface"] = RuntimeSurface,
            ["api_shape"] = ApiShape,
            ["refs_or_hashes_only"] = bool.TrueString,
            ["refs_only_public_response"] = bool.TrueString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["raw_user_private_data_included"] = bool.FalseString,
            ["response_contains_raw_private_data"] = bool.FalseString,
            ["private_file_paths_included"] = bool.FalseString,
            ["secret_values_included"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString,
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
            ["product_ready"] = bool.FalseString
        };

        foreach (var pair in domainMetadata ?? new Dictionary<string, string>())
        {
            var key = SafeMetadataKey(pair.Key);
            metadata[key] = key.Contains("error_code", StringComparison.OrdinalIgnoreCase)
                ? SafeErrorCode(pair.Value)
                : SafeScalar(pair.Value, pair.Key);
        }

        metadata["runtime_invoked"] = runtimeInvoked.ToString();
        metadata["refs_or_hashes_only"] = bool.TrueString;
        metadata["refs_only_public_response"] = bool.TrueString;
        metadata["private_file_paths_included"] = bool.FalseString;
        metadata["secret_values_included"] = bool.FalseString;
        metadata["response_contains_raw_private_data"] = bool.FalseString;
        metadata["public_http_endpoint_verified"] = bool.FalseString;
        metadata["cross_repo_caller_verified"] = bool.FalseString;
        metadata["deployment_verified"] = bool.FalseString;
        metadata["product_ready"] = bool.FalseString;
        return metadata;
    }

    private static string? ReadHeader(IReadOnlyDictionary<string, string>? headers, string name)
    {
        if (headers is null)
            return null;

        foreach (var pair in headers)
        {
            if (string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase))
                return pair.Value;
        }

        return null;
    }

    private static int StatusCodeForApiError(string action, string? errorCode)
    {
        if (string.Equals(action, ActionActivateLocalPromotion, StringComparison.Ordinal))
        {
            return errorCode switch
            {
                "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_RECORD_NOT_FOUND" => 404,
                "DESIGN_CONVERSION_MODEL_ACTIVATION_LOCAL_PROMOTION_SELECTOR_AMBIGUOUS" => 409,
                "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_AUTHORITY_STATE_INVALID" => 409,
                "DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_NOT_CURRENT_ACTIVE" => 409,
                "DESIGN_CONVERSION_MODEL_ACTIVATION_VERSION_SCOPE_MISMATCH" => 409,
                _ => StatusCodeForApiError(errorCode)
            };
        }

        return StatusCodeForApiError(errorCode);
    }

    private static int StatusCodeForApiError(string? errorCode)
        => errorCode switch
        {
            "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_NOT_CONFIGURED" => 503,
            "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_REQUIRED" => 401,
            "DESIGN_CONVERSION_MODEL_ACTIVATION_AUTH_TOKEN_INVALID" => 403,
            "DESIGN_CONVERSION_MODEL_ACTIVATION_GENERATION_CONFLICT" => 409,
            "DESIGN_CONVERSION_MODEL_ROLLBACK_GENERATION_CONFLICT" => 409,
            "DESIGN_CONVERSION_MODEL_ACTIVATION_IDEMPOTENCY_KEY_CONFLICT" => 409,
            "DESIGN_CONVERSION_MODEL_ACTIVATION_TENANT_SCOPE_MISMATCH" => 409,
            "DESIGN_CONVERSION_MODEL_ACTIVATION_ACTIVE_VERSION_NOT_FOUND" => 404,
            _ => 400
        };

    private static string SafeDomainFailureMessage(string action)
        => action switch
        {
            ActionActivate => "DesignConversion model activation failed; see errorCode and refs-only boundary metadata.",
            ActionActivateLocalPromotion => "DesignConversion refs-only local promotion activation failed; see errorCode and refs-only boundary metadata.",
            ActionRollback => "DesignConversion model rollback failed; see errorCode and refs-only boundary metadata.",
            ActionReadActive => "DesignConversion active model read failed; see errorCode and refs-only boundary metadata.",
            _ => "DesignConversion model activation action failed; see errorCode and refs-only boundary metadata."
        };

    private static IReadOnlyList<string> SafeList(IReadOnlyList<string>? values, string kind)
        => (values ?? [])
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => SafeScalar(value, kind))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

    private static string SafeScalar(string? value, string kind)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var trimmed = value.Trim();
        return LooksSensitive(trimmed) ? ToHashRef(trimmed, kind) : trimmed;
    }

    private static string SafeErrorCode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = new string(value.Trim()
            .Select(character => char.IsAsciiLetterOrDigit(character) || character == '_' || character == '-'
                ? character
                : '_')
            .ToArray());
        return string.IsNullOrWhiteSpace(normalized)
            ? "MODEL_ACTIVATION_ERROR"
            : normalized;
    }

    private static string SafeMetadataKey(string? value)
        => SafeErrorCode(value);

    private static string ToHashRef(string value, string kind)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
        return $"{kind}:sha256:{hash[..16]}";
    }

    private static bool LooksSensitive(string value)
        => value.Contains("<", StringComparison.Ordinal) ||
           value.Contains("{", StringComparison.Ordinal) ||
           value.Contains("C:/", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("C:\\", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("file://", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("private", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("token", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("checkpoint_path:", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("data-secret", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("raw_provider_payload", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("provider_payload", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeAction(string? action)
        => string.IsNullOrWhiteSpace(action) ? string.Empty : action.Trim().ToLowerInvariant();

    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "/";

        var trimmed = path.Trim();
        var queryIndex = trimmed.IndexOf('?', StringComparison.Ordinal);
        return queryIndex >= 0 ? trimmed[..queryIndex] : trimmed;
    }

    private static bool IsJsonContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return true;

        return contentType.Split(';', 2)[0].Trim().Equals("application/json", StringComparison.OrdinalIgnoreCase);
    }
}
