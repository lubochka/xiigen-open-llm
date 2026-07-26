// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Globalization;
using AgentInfrastructure;

public sealed class DesignConversionExternalVisualAiComparisonProvider : IDesignConversionComparisonProvider
{
    public const string ProviderKind = "external_visual_ai_comparison_provider_boundary";
    public const string ProviderContractExecutedMarker = "visual_ai_provider_contract_executed";
    public const string NoKeyBlockedMarker = "external_visual_ai_blocked:not_configured";
    public const string RefsOnlyPayloadMarker = "external_visual_ai_payload:refs_or_hashes_only";
    public const string FakeClientMarker = "fake_visual_ai_client:true";
    public const string NetworkCallNotExecutedMarker = "network_call_executed:false";
    public const string RealExternalProviderNotExecutedMarker = "real_external_provider_executed:false";
    public const string TerminalHttpNonSuccessReason = "terminal_http_non_success";
    public const string TransportFailureReason = "transport_failure";
    public const string ResponseReadFailureReason = "response_read_failure";
    public const string ResponsePolicyBlockReason = "response_policy_block";
    public const string ResponseValidationFailureReason = "response_validation_failure";

    private static readonly IReadOnlyList<string> ServerCallReceiptMetadataKeys =
    [
        ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey,
        ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVersionKey,
        ExternalHttpDesignConversionVisualJudge.ServerCallReceiptIdKey,
        ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJobIdKey,
        ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJudgeRequestIdKey,
        ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRequestIdKey,
        ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRootRequestIdKey,
        ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey,
        ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey,
        ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualJudgeRequestIdKey,
        ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualResultIdKey,
        ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey,
        ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderNameKey,
        ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderModelKey,
        ExternalHttpDesignConversionVisualJudge.ServerCallReceiptEndpointHostKey,
        ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey,
        ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey,
        ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey
    ];

    private static readonly IReadOnlyList<string> ServerOwnedExternalAttemptMetadataKeys =
    [
        ExternalHttpDesignConversionVisualJudge.NetworkCallAttemptedKey,
        ExternalHttpDesignConversionVisualJudge.NetworkCallExecutionVerifiedKey,
        ExternalHttpDesignConversionVisualJudge.ProviderResponseReceivedKey,
        ExternalHttpDesignConversionVisualJudge.TransportFailureCodeKey,
        ExternalHttpDesignConversionVisualJudge.ResponseReadFailureCodeKey,
        ExternalHttpDesignConversionVisualJudge.ResponsePolicyCodeKey,
        ExternalHttpDesignConversionVisualJudge.ResponseValidationCodeKey,
        ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey,
        ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey,
        ExternalHttpDesignConversionVisualJudge.ServerOwnedCostPerAttemptUsdKey,
        ExternalHttpDesignConversionVisualJudge.ServerOwnedTotalCostUsdKey,
        ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingKey,
        "cost_source",
        "server_owned_pricing_configured",
        "server_owned_pricing_valid",
        "server_owned_pricing_status"
    ];

    private readonly IDesignConversionVisualJudge _visualJudge;

    public DesignConversionExternalVisualAiComparisonProvider(IDesignConversionVisualJudge? visualJudge = null)
    {
        _visualJudge = visualJudge ?? new NoConfiguredVisualJudge();
    }

    internal bool OwnsServerOwnedExternalHttpVisualJudge
        => _visualJudge is ExternalHttpDesignConversionVisualJudge;

    public async Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
        DesignConversionQaComparisonRequest request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<DesignConversionQaComparisonArtifact>.Fail(
                "DesignConversion external visual AI comparison cancelled.",
                "CANCELLED");

        var validation = Validate(request);
        if (!validation.IsSuccess)
            return validation.MapError<DesignConversionQaComparisonArtifact>();

        var judgeRequest = new DesignConversionVisualJudgeRequest(
            JudgeRequestId: request.ComparisonRequestId,
            RequestId: request.RequestId,
            ExpectedScreenshot: request.Expected,
            RenderedImage: request.Actual,
            DiffArtifactUri: TryMetadata(request.Metadata, "diff_artifact_uri"),
            DiffArtifactSha256: TryMetadata(request.Metadata, "diff_artifact_sha256"),
            Metadata: BuildJudgeMetadata(request));

        var judge = await _visualJudge.JudgeAsync(judgeRequest, cancellationToken);
        if (!judge.IsSuccess)
            return judge.MapError<DesignConversionQaComparisonArtifact>();

        var judgeResult = judge.Value!;
        var comparisonArtifact = judgeResult.ToComparisonArtifact(judgeRequest);
        var metadata = new Dictionary<string, string>(BuildBoundaryMetadata(judgeResult), StringComparer.Ordinal)
        {
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey] =
                SafeScalar(
                    comparisonArtifact.ArtifactId,
                    ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey)
        };
        var metrics = BuildMetrics(judgeResult);
        var evidenceRefs = BuildEvidenceRefs(judgeResult, metadata);

        return OperationResult<DesignConversionQaComparisonArtifact>.Ok(
            comparisonArtifact with
            {
                Metrics = metrics,
                EvidenceRefs = evidenceRefs,
                Metadata = metadata
            });
    }

    private static OperationResult<bool> Validate(DesignConversionQaComparisonRequest request)
    {
        if (request is null)
            return OperationResult<bool>.Fail("DesignConversion comparison request is required.", "COMPARISON_REQUEST_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.ComparisonRequestId))
            return OperationResult<bool>.Fail("DesignConversion comparison request id is required.", "COMPARISON_REQUEST_ID_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.RequestId))
            return OperationResult<bool>.Fail("DesignConversion request id is required.", "REQUEST_ID_REQUIRED");
        if (request.Expected is null || request.Actual is null)
            return OperationResult<bool>.Fail("DesignConversion expected and actual visual artifacts are required.", "COMPARISON_IMAGES_REQUIRED");

        return OperationResult<bool>.Ok(true);
    }

    private static IReadOnlyDictionary<string, string> BuildJudgeMetadata(DesignConversionQaComparisonRequest request)
    {
        var metadata = SanitizeMetadata(request.Metadata);
        metadata["comparison_request_id"] = SafeScalar(request.ComparisonRequestId, "comparison_request_id");
        metadata["comparison_adapter"] = ProviderKind;
        metadata["image_payload_mode"] = "refs_or_hashes_only";
        metadata["raw_html_included"] = bool.FalseString;
        metadata["raw_css_included"] = bool.FalseString;
        metadata["raw_design_tree_included"] = bool.FalseString;
        metadata["provider_payload_included"] = bool.FalseString;
        return metadata;
    }

    private static IReadOnlyDictionary<string, string> BuildBoundaryMetadata(DesignConversionVisualJudgeResult result)
    {
        var fakeClient = IsTruthy(result.Provider.Metadata, "fake_client") ||
                         IsTruthy(result.Provider.Metadata, DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker);
        var providerMetadataConfigured = IsTruthy(result.Provider.Metadata, "configured");
        var networkCallAttempted = IsTruthy(
            result.Provider.Metadata,
            ExternalHttpDesignConversionVisualJudge.NetworkCallAttemptedKey);
        var networkCallExecuted = IsTruthy(result.Provider.Metadata, "network_call_executed");
        var networkCallExecutionVerified = IsTruthy(
            result.Provider.Metadata,
            ExternalHttpDesignConversionVisualJudge.NetworkCallExecutionVerifiedKey);
        var terminalHttpExecutionVerifiedByReceipt =
            string.Equals(
                result.Status,
                ExternalHttpDesignConversionVisualJudge.StatusHttpBlocked,
                StringComparison.Ordinal) &&
            IsTruthy(result.Provider.Metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey) &&
            IsTruthy(
                result.Provider.Metadata,
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey);
        var groundedNetworkCallExecution = networkCallExecutionVerified || terminalHttpExecutionVerifiedByReceipt;
        var groundedBlockedStatus = TryResolveConfiguredBlockedStatus(
            result.Status,
            fakeClient,
            result.Provider.ExternalCallExecuted,
            providerMetadataConfigured,
            networkCallAttempted,
            networkCallExecuted,
            groundedNetworkCallExecution,
            out var blockedReason,
            out var configuredProviderMode,
            out var configuredProviderStatus);
        var configuredBlocked = result.IsBlocked && groundedBlockedStatus;
        var realExternalProviderExecuted = result.Provider.ExternalCallExecuted && !fakeClient && networkCallExecuted;
        var providerConfigured = configuredBlocked ||
                                 (!result.IsBlocked && !string.Equals(
                                     result.Status,
                                     DesignConversionVisualQaBoundaryPolicy.ProviderRequired,
                                     StringComparison.Ordinal));
        var providerContractExecuted = !result.IsBlocked ||
                                       (configuredBlocked && networkCallExecuted && groundedNetworkCallExecution);
        var finalBlockedReason = result.IsBlocked
            ? configuredBlocked ? blockedReason : "not_configured"
            : "none";
        var responseIdentityVerified = IsTruthy(result.Provider.Metadata, "provider_response_identity_verified") &&
                                       IsTruthy(result.Metadata, "provider_response_identity_verified");
        var responseSchemaVerified = IsTruthy(result.Provider.Metadata, "provider_response_schema_verified") &&
                                     IsTruthy(result.Metadata, "provider_response_schema_verified");
        var policyMetadata = configuredBlocked
            ? DesignConversionVisualAiProviderSelectionPolicy.DescribeExternalConfig(
                result.Provider.ProviderName,
                configComplete: true,
                completeProviderMode: configuredProviderMode,
                completeProviderStatus: configuredProviderStatus)
            : result.IsBlocked
                ? DesignConversionVisualAiProviderSelectionPolicy.DescribeNotConfigured(
                    result.Provider.ProviderName,
                    finalBlockedReason)
                : fakeClient
                    ? DesignConversionVisualAiProviderSelectionPolicy.DescribeLocalStub(
                        result.Provider.ProviderName,
                        "test_or_local_provider_no_network")
                    : DesignConversionVisualAiProviderSelectionPolicy.DescribeExternalConfig(
                        result.Provider.ProviderName,
                        configComplete: true,
                        completeProviderMode: realExternalProviderExecuted
                            ? "external_executed"
                            : "external_configured_not_called",
                        completeProviderStatus: realExternalProviderExecuted
                            ? "executed"
                            : "configured_not_called");

        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["provider_kind"] = ProviderKind,
            ["provider_name"] = SafeScalar(result.Provider.ProviderName, "provider_name"),
            ["provider_model"] = SafeScalar(result.Provider.ModelId, "provider_model"),
            ["provider_endpoint_host"] = SafeScalar(result.Provider.EndpointHost, "provider_endpoint_host"),
            ["external_visual_ai_configured"] = providerConfigured.ToString(),
            ["external_visual_ai_executed"] = realExternalProviderExecuted.ToString(),
            ["visual_ai_provider_contract_executed"] = providerContractExecuted.ToString(),
            ["real_external_provider_executed"] = realExternalProviderExecuted.ToString(),
            ["network_call_executed"] = networkCallExecuted.ToString(),
            ["fake_client"] = fakeClient.ToString(),
            ["external_visual_ai_blocked"] = result.IsBlocked.ToString(),
            ["blocked_reason"] = finalBlockedReason,
            ["provider_response_identity_verified"] = responseIdentityVerified.ToString(),
            ["provider_response_schema_verified"] = responseSchemaVerified.ToString(),
            ["estimated_cost_usd"] = (result.EstimatedCostUsd ?? 0m).ToString("0.####", CultureInfo.InvariantCulture),
            ["score"] = result.Verdict.Score.ToString("0.####", CultureInfo.InvariantCulture),
            ["defect_count"] = result.Verdict.Defects.Count.ToString(CultureInfo.InvariantCulture),
            ["image_payload_mode"] = "refs_or_hashes_only",
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString,
            ["status"] = SafeScalar(result.Status, "visual_ai_status")
        };
        DesignConversionVisualAiProviderSelectionPolicy.CopyPublicMetadata(metadata, policyMetadata);
        DesignConversionVisualAiProviderSelectionPolicy.CopyPublicMetadata(metadata, result.Provider.Metadata);
        DesignConversionVisualAiProviderSelectionPolicy.CopyPublicMetadata(metadata, result.Metadata);
        CopyServerCallReceiptMetadata(metadata, result.Provider.Metadata);
        CopyServerCallReceiptMetadata(metadata, result.Metadata);
        CopyServerOwnedExternalAttemptMetadata(metadata, result.Provider.Metadata);
        CopyServerOwnedExternalAttemptMetadata(metadata, result.Metadata);
        metadata[DesignConversionVisualAiProviderSelectionPolicy.RealCloudProviderClaimedKey] = bool.FalseString;
        return metadata;
    }

    private static IReadOnlyDictionary<string, double> BuildMetrics(DesignConversionVisualJudgeResult result)
    {
        var metadata = BuildBoundaryMetadata(result);
        var metrics = new Dictionary<string, double>(StringComparer.Ordinal)
        {
            ["visual_judge_score"] = result.Verdict.Score,
            ["external_visual_ai_configured"] = IsTruthy(metadata, "external_visual_ai_configured") ? 1.0 : 0.0,
            ["external_visual_ai_executed"] = IsTruthy(metadata, "external_visual_ai_executed") ? 1.0 : 0.0,
            ["visual_ai_provider_contract_executed"] = IsTruthy(metadata, "visual_ai_provider_contract_executed") ? 1.0 : 0.0,
            ["real_external_provider_executed"] = IsTruthy(metadata, "real_external_provider_executed") ? 1.0 : 0.0,
            ["network_call_executed"] = IsTruthy(metadata, "network_call_executed") ? 1.0 : 0.0,
            ["fake_client"] = IsTruthy(metadata, "fake_client") ? 1.0 : 0.0,
            ["external_visual_ai_blocked"] = IsTruthy(metadata, "external_visual_ai_blocked") ? 1.0 : 0.0,
            ["estimated_cost_usd"] = (double)(result.EstimatedCostUsd ?? 0m)
        };

        foreach (var pair in result.TokenMetadata)
            metrics[$"token_{pair.Key}"] = pair.Value;

        return metrics;
    }

    private static IReadOnlyList<string> BuildEvidenceRefs(
        DesignConversionVisualJudgeResult result,
        IReadOnlyDictionary<string, string> metadata)
    {
        var blockedReason = metadata.TryGetValue("blocked_reason", out var reason)
            ? reason
            : "none";
        var providerNotConfigured = IsTruthy(metadata, "external_visual_ai_blocked") &&
                                    string.Equals(blockedReason, "not_configured", StringComparison.Ordinal);
        var configuredExternalProviderBlocked = IsTruthy(metadata, "external_visual_ai_blocked") &&
                                                !providerNotConfigured;
        var refs = result.EvidenceRefs
            .Concat(result.Verdict.EvidenceRefs)
            .Append(ProviderKind)
            .Append(DesignConversionVisualQaBoundaryPolicy.VisualJudgeProviderBoundaryMarker)
            .Append(RefsOnlyPayloadMarker)
            .Append(IsTruthy(metadata, "visual_ai_provider_contract_executed")
                ? ProviderContractExecutedMarker
                : "visual_ai_provider_contract_executed:false")
            .Append(IsTruthy(metadata, "network_call_executed")
                ? "network_call_executed:true"
                : NetworkCallNotExecutedMarker)
            .Append(IsTruthy(metadata, "real_external_provider_executed")
                ? "real_external_provider_executed:true"
                : RealExternalProviderNotExecutedMarker)
            .Append(IsTruthy(metadata, "fake_client") ? FakeClientMarker : "fake_visual_ai_client:false")
            .Append($"provider_name:{metadata["provider_name"]}")
            .Append($"provider_model:{metadata["provider_model"]}")
            .Append($"visual_ai_selected_provider:{metadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey]}")
            .Append($"visual_ai_provider_mode:{metadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey]}")
            .Append($"visual_ai_provider_fallback_status:{metadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackStatusKey]}")
            .Append($"estimated_cost_usd:{metadata["estimated_cost_usd"]}");

        if (providerNotConfigured)
            refs = refs.Append(NoKeyBlockedMarker).Append("blocked_reason:not_configured");
        else if (configuredExternalProviderBlocked)
            refs = refs
                .Append(DesignConversionVisualQaBoundaryPolicy.ExternalProviderCallFailedMarker)
                .Append($"blocked_reason:{blockedReason}");
        if (IsTruthy(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey))
        {
            refs = refs
                .Append($"{ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey}:true")
                .Append($"visual_provider_call_receipt:{metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptIdKey]}");
        }

        return refs
            .Where(reference => !string.IsNullOrWhiteSpace(reference))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static bool TryResolveConfiguredBlockedStatus(
        string status,
        bool fakeClient,
        bool externalCallExecuted,
        bool providerMetadataConfigured,
        bool networkCallAttempted,
        bool networkCallExecuted,
        bool networkCallExecutionVerified,
        out string blockedReason,
        out string providerMode,
        out string providerStatus)
    {
        if (fakeClient || !externalCallExecuted || !providerMetadataConfigured)
            return NotConfigured(out blockedReason, out providerMode, out providerStatus);

        switch (status)
        {
            case ExternalHttpDesignConversionVisualJudge.StatusHttpBlocked
                when networkCallExecuted && networkCallExecutionVerified:
                blockedReason = TerminalHttpNonSuccessReason;
                providerMode = "external_executed";
                providerStatus = "terminal_http_status";
                return true;
            case ExternalHttpDesignConversionVisualJudge.StatusTransportBlocked
                when networkCallAttempted:
                blockedReason = TransportFailureReason;
                providerMode = "external_attempted";
                providerStatus = TransportFailureReason;
                return true;
            case ExternalHttpDesignConversionVisualJudge.StatusResponseReadBlocked
                when networkCallExecuted && networkCallExecutionVerified:
                blockedReason = ResponseReadFailureReason;
                providerMode = "external_response_received";
                providerStatus = ResponseReadFailureReason;
                return true;
            case ExternalHttpDesignConversionVisualJudge.StatusResponsePolicyBlocked
                when networkCallExecuted && networkCallExecutionVerified:
                blockedReason = ResponsePolicyBlockReason;
                providerMode = "external_response_received";
                providerStatus = "response_policy_blocked";
                return true;
            case ExternalHttpDesignConversionVisualJudge.StatusResponseValidationBlocked
                when networkCallExecuted && networkCallExecutionVerified:
                blockedReason = ResponseValidationFailureReason;
                providerMode = "external_response_received";
                providerStatus = ResponseValidationFailureReason;
                return true;
            default:
                return NotConfigured(out blockedReason, out providerMode, out providerStatus);
        }
    }

    private static bool NotConfigured(
        out string blockedReason,
        out string providerMode,
        out string providerStatus)
    {
        blockedReason = "not_configured";
        providerMode = "not_configured";
        providerStatus = "not_selected";
        return false;
    }

    private static void CopyServerCallReceiptMetadata(
        IDictionary<string, string> target,
        IReadOnlyDictionary<string, string>? source)
    {
        if (source is null)
            return;

        foreach (var key in ServerCallReceiptMetadataKeys)
        {
            if (source.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                target[key] = SafeScalar(value, key);
        }
    }

    private static void CopyServerOwnedExternalAttemptMetadata(
        IDictionary<string, string> target,
        IReadOnlyDictionary<string, string>? source)
    {
        if (source is null)
            return;

        foreach (var key in ServerOwnedExternalAttemptMetadataKeys)
        {
            if (source.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                target[key] = SafeScalar(value, key);
        }
    }

    private static Dictionary<string, string> SanitizeMetadata(IReadOnlyDictionary<string, string>? metadata)
    {
        var sanitized = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in metadata ?? new Dictionary<string, string>())
        {
            var key = SafeScalar(pair.Key, "visual_ai_metadata_key");
            if (string.IsNullOrWhiteSpace(key))
                continue;

            sanitized[key] = IsSensitiveKey(key) || LooksLikeRawPayloadOrLocalPath(pair.Value)
                ? DesignConversionDpoLeakageGuard.HashRef("dcm-visual-ai-redacted", pair.Value)
                : SafeScalar(pair.Value, $"visual_ai_metadata.{key}");
        }

        return sanitized;
    }

    private static string? TryMetadata(IReadOnlyDictionary<string, string>? metadata, string key)
        => metadata is not null && metadata.TryGetValue(key, out var value) && !LooksLikeRawPayloadOrLocalPath(value)
            ? SafeScalar(value, key)
            : null;

    private static bool IsSensitiveKey(string key)
    {
        var normalized = key.Trim().Replace("-", "_", StringComparison.Ordinal).ToLowerInvariant();
        return normalized.Contains("secret", StringComparison.Ordinal)
               || normalized.Contains("token", StringComparison.Ordinal)
               || normalized.Contains("api_key", StringComparison.Ordinal)
               || normalized.Contains("apikey", StringComparison.Ordinal)
               || normalized.Contains("password", StringComparison.Ordinal)
               || normalized.Contains("authorization", StringComparison.Ordinal)
               || normalized.Contains("private", StringComparison.Ordinal)
               || normalized.Contains("user_id", StringComparison.Ordinal)
               || normalized.Contains("path", StringComparison.Ordinal)
               || normalized is "html" or "css" or "raw_html" or "raw_css" or "tree" or "design_tree" or "original_tree";
    }

    private static bool LooksLikeRawPayloadOrLocalPath(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           (value.Length > 160 ||
            value.Contains("<html", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("<main", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("<div", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("{\"", StringComparison.Ordinal) ||
            value.Contains("data-secret", StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith("file:", StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith(@"\\", StringComparison.Ordinal) ||
            (value.Length >= 3 && char.IsLetter(value[0]) && value[1] == ':' && (value[2] == '\\' || value[2] == '/')));

    private static string SafeScalar(string? value, string scope)
    {
        var redactions = new SortedSet<string>(StringComparer.Ordinal);
        return DesignConversionDpoLeakageGuard.SanitizeScalar(value ?? string.Empty, scope, redactions);
    }

    private static bool IsTruthy(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) &&
           (string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "1", StringComparison.Ordinal) ||
            string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase));
}
