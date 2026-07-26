// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Text.Json.Nodes;
using DesignConversionDomain;

public static class DesignConversionModelServerNetworkBoundary
{
    public const string ServerOwnedVisualProviderNetworkCallVerifiedKey =
        "server_owned_visual_provider_network_call_verified";
    public const string ServerOwnedVisualProviderNetworkCallVerificationKey =
        "server_owned_visual_provider_network_call_verification";

    private const string VerificationReason =
        "comparison_artifact_provider_identity_schema_and_network_evidence_verified";

    public static bool IsTrustedVisualProviderNetworkCall(
        DesignConversionQaComparisonArtifact artifact,
        string? currentRequestId)
        => IsTrustedVisualProviderNetworkCall(
            artifact,
            currentJobId: null,
            currentRootRequestId: currentRequestId,
            providerResponseIdentityVerified: IsTruthy(artifact.Metadata, "provider_response_identity_verified"),
            providerResponseSchemaVerified: IsTruthy(artifact.Metadata, "provider_response_schema_verified"));

    public static bool IsTrustedVisualProviderNetworkCall(
        DesignConversionQaComparisonArtifact artifact,
        string? currentJobId,
        string? currentRootRequestId)
        => IsTrustedVisualProviderNetworkCall(
            artifact,
            currentJobId,
            currentRootRequestId,
            providerResponseIdentityVerified: IsTruthy(artifact.Metadata, "provider_response_identity_verified"),
            providerResponseSchemaVerified: IsTruthy(artifact.Metadata, "provider_response_schema_verified"));

    public static bool IsTrustedVisualProviderNetworkCall(
        DesignConversionQaComparisonArtifact artifact,
        string? currentJobId,
        string? currentRootRequestId,
        bool providerResponseIdentityVerified,
        bool providerResponseSchemaVerified)
    {
        if (artifact is null ||
            string.IsNullOrWhiteSpace(artifact.ArtifactId) ||
            string.IsNullOrWhiteSpace(artifact.ComparisonRequestId) ||
            artifact.Expected is null ||
            artifact.Actual is null)
        {
            return false;
        }

        var metadata = artifact.Metadata;
        if (!IsTruthy(metadata, "network_call_executed") ||
            !IsTruthy(metadata, "real_external_provider_executed") ||
            !IsTruthy(metadata, "external_visual_ai_executed") ||
            !IsTruthy(metadata, "visual_ai_provider_contract_executed") ||
            !providerResponseIdentityVerified ||
            !providerResponseSchemaVerified ||
            IsTruthy(metadata, "fake_client") ||
            IsTruthy(metadata, DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker))
        {
            return false;
        }

        if (!MetadataEquals(metadata, DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey, "executed") ||
            !MetadataEquals(metadata, DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey, "external_executed"))
        {
            return false;
        }

        if (!HasSafeProviderIdentity(metadata, "provider_name") ||
            !HasSafeProviderIdentity(metadata, "provider_model"))
        {
            return false;
        }

        if (!EvidenceContains(artifact.EvidenceRefs, "network_call_executed:true") ||
            !EvidenceContains(artifact.EvidenceRefs, "real_external_provider_executed:true") ||
            !EvidenceContains(artifact.EvidenceRefs, "external_ai_visual_judge_executed:true") ||
            EvidenceContains(artifact.EvidenceRefs, DesignConversionExternalVisualAiComparisonProvider.FakeClientMarker) ||
            EvidenceContains(artifact.EvidenceRefs, DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker) ||
            EvidenceContains(artifact.EvidenceRefs, DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker))
        {
            return false;
        }

        var receiptProviderRequestId = TryGetMetadataValue(
            metadata,
            ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey);
        var receiptComparisonRequestId = TryGetMetadataValue(
            metadata,
            ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey);
        if (string.IsNullOrWhiteSpace(receiptProviderRequestId) ||
            !string.Equals(receiptComparisonRequestId, artifact.ComparisonRequestId, StringComparison.Ordinal))
        {
            return false;
        }

        var expectedVisualJudgeRequestId = receiptProviderRequestId.Contains(
            ":visual-judge-request:",
            StringComparison.Ordinal)
            ? receiptProviderRequestId
            : null;
        var expectedVisualResultId = ExternalHttpDesignConversionVisualJudge.BuildCanonicalVisualResultId(
            receiptProviderRequestId,
            artifact.ComparisonRequestId);
        var expectedComparisonArtifactId = ExternalHttpDesignConversionVisualJudge.BuildCanonicalComparisonArtifactId(
            receiptProviderRequestId,
            artifact.ComparisonRequestId);

        return CurrentEvaluationArtifactIdentityIsPresent(artifact) &&
               string.Equals(artifact.ArtifactId, expectedComparisonArtifactId, StringComparison.Ordinal) &&
               HasTrustedVisualProviderCallReceipt(
                   metadata,
                   currentJobId,
                   currentRootRequestId,
                   expectedProviderRequestId: receiptProviderRequestId,
                   expectedComparisonRequestId: artifact.ComparisonRequestId,
                   expectedVisualJudgeRequestId: expectedVisualJudgeRequestId,
                   expectedVisualResultId: expectedVisualResultId,
                   expectedComparisonArtifactId: expectedComparisonArtifactId,
                   expectedProviderName: metadata.TryGetValue("provider_name", out var artifactProviderName) ? artifactProviderName : string.Empty,
                   expectedProviderModel: metadata.TryGetValue("provider_model", out var artifactProviderModel) ? artifactProviderModel : string.Empty) &&
               (string.IsNullOrWhiteSpace(currentJobId) ||
                artifact.ComparisonRequestId.StartsWith(
                    $"{currentJobId}:comparison-request:",
                    StringComparison.Ordinal));
    }

    public static bool IsTrustedVisualJudgeNetworkCall(
        DesignConversionVisualJudgeResult result,
        string? currentRequestId)
        => IsTrustedVisualJudgeNetworkCall(result, currentJobId: null, currentRootRequestId: currentRequestId);

    public static bool IsTrustedVisualJudgeNetworkCall(
        DesignConversionVisualJudgeResult result,
        string? currentJobId,
        string? currentRootRequestId)
    {
        if (result is null ||
            string.IsNullOrWhiteSpace(result.JudgeRequestId) ||
            result.Provider is null ||
            string.IsNullOrWhiteSpace(result.Provider.ProviderName) ||
            string.IsNullOrWhiteSpace(result.Provider.ModelId) ||
            string.IsNullOrWhiteSpace(result.Provider.EndpointHost) ||
            result.IsBlocked ||
            !result.Provider.ExternalCallExecuted)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(currentJobId) &&
            !result.JudgeRequestId.StartsWith(
                $"{currentJobId}:visual-judge-request:",
                StringComparison.Ordinal))
        {
            return false;
        }

        var providerComparisonRequestId = TryGetMetadataValue(
            result.Provider.Metadata,
            ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey);
        var resultComparisonRequestId = TryGetMetadataValue(
            result.Metadata,
            ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey);
        var expectedProviderVisualResultId = ExternalHttpDesignConversionVisualJudge.BuildCanonicalVisualResultId(
            result.JudgeRequestId,
            providerComparisonRequestId);
        var expectedResultVisualResultId = ExternalHttpDesignConversionVisualJudge.BuildCanonicalVisualResultId(
            result.JudgeRequestId,
            resultComparisonRequestId);

        return string.Equals(result.JudgeResultId, expectedResultVisualResultId, StringComparison.Ordinal) &&
               IsTruthy(result.Provider.Metadata, "network_call_executed") &&
               IsTruthy(result.Provider.Metadata, "provider_response_identity_verified") &&
               IsTruthy(result.Provider.Metadata, "provider_response_schema_verified") &&
               IsTruthy(result.Metadata, "network_call_executed") &&
               IsTruthy(result.Metadata, "provider_response_identity_verified") &&
               IsTruthy(result.Metadata, "provider_response_schema_verified") &&
               MetadataEquals(
                   result.Provider.Metadata,
                   DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey,
                   "executed") &&
               MetadataEquals(
                   result.Provider.Metadata,
                   DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey,
                   "external_executed") &&
               HasTrustedVisualProviderCallReceipt(
                   result.Provider.Metadata,
                    currentJobId,
                    currentRootRequestId,
                    expectedProviderRequestId: result.JudgeRequestId,
                    expectedComparisonRequestId: providerComparisonRequestId,
                    expectedVisualJudgeRequestId: result.JudgeRequestId,
                    expectedVisualResultId: expectedProviderVisualResultId,
                    expectedComparisonArtifactId: null,
                    expectedProviderName: result.Provider.ProviderName,
                    expectedProviderModel: result.Provider.ModelId) &&
               HasTrustedVisualProviderCallReceipt(
                   result.Metadata,
                    currentJobId,
                    currentRootRequestId,
                    expectedProviderRequestId: result.JudgeRequestId,
                    expectedComparisonRequestId: resultComparisonRequestId,
                    expectedVisualJudgeRequestId: result.JudgeRequestId,
                    expectedVisualResultId: expectedResultVisualResultId,
                    expectedComparisonArtifactId: null,
                    expectedProviderName: result.Provider.ProviderName,
                    expectedProviderModel: result.Provider.ModelId) &&
               !IsTruthy(result.Provider.Metadata, "fake_client") &&
               !IsTruthy(result.Metadata, "fake_client") &&
               !IsTruthy(result.Provider.Metadata, DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker) &&
               !IsTruthy(result.Metadata, DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker) &&
               EvidenceContains(result.EvidenceRefs, "external_ai_visual_judge_executed:true");
    }

    public static bool MatchesTrustedVisualJudge(
        DesignConversionQaComparisonArtifact artifact,
        DesignConversionVisualJudgeResult result)
        => MetadataEquals(artifact.Metadata, "provider_name", result.Provider.ProviderName) &&
           MetadataEquals(artifact.Metadata, "provider_model", result.Provider.ModelId);

    public static void ApplyTrustedNetworkBoundaryMetadata(
        IDictionary<string, string> metadata,
        bool trustedNetworkCall)
    {
        metadata[ServerOwnedVisualProviderNetworkCallVerifiedKey] = trustedNetworkCall.ToString();
        metadata[ServerOwnedVisualProviderNetworkCallVerificationKey] =
            trustedNetworkCall ? VerificationReason : "not_verified";
        metadata["network_call_executed"] = trustedNetworkCall.ToString();
        metadata["external_network_call_executed"] = trustedNetworkCall.ToString();
        metadata["external_provider_called"] = trustedNetworkCall.ToString();
        metadata["real_external_provider_executed"] = trustedNetworkCall.ToString();
        metadata["external_visual_ai_executed"] = trustedNetworkCall.ToString();
        metadata["external_ai_visual_judge_executed"] = trustedNetworkCall.ToString();
    }

    public static void PreserveTrustedNetworkBoundaryMetadata(JsonObject metadata)
    {
        var trustedNetworkCall = HasTrustedStatusVisualProviderCallReceipt(metadata);

        metadata[ServerOwnedVisualProviderNetworkCallVerifiedKey] = trustedNetworkCall.ToString();
        metadata[ServerOwnedVisualProviderNetworkCallVerificationKey] =
            trustedNetworkCall ? VerificationReason : "not_verified";
        metadata["network_call_executed"] = trustedNetworkCall.ToString();
        metadata["external_network_call_executed"] = trustedNetworkCall.ToString();
        metadata["external_provider_called"] = trustedNetworkCall.ToString();
        metadata["real_external_provider_executed"] = trustedNetworkCall.ToString();
        metadata["external_visual_ai_executed"] = trustedNetworkCall.ToString();
        metadata["external_ai_visual_judge_executed"] = trustedNetworkCall.ToString();
    }

    private static bool HasTrustedStatusVisualProviderCallReceipt(JsonObject metadata)
        => !IsTruthy(metadata, "fake_client") &&
           !IsTruthy(metadata, DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker) &&
           MetadataEquals(metadata, DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey, "executed") &&
           MetadataEquals(metadata, DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey, "external_executed") &&
           HasSafeProviderIdentity(metadata, "provider_name") &&
           HasSafeProviderIdentity(metadata, "provider_model") &&
           HasTrustedVisualProviderCallReceipt(metadata);

    private static bool CurrentEvaluationArtifactIdentityIsPresent(DesignConversionQaComparisonArtifact artifact)
        => !string.IsNullOrWhiteSpace(artifact.ComparisonRequestId) &&
           !string.IsNullOrWhiteSpace(artifact.Expected.ImageId) &&
           !string.IsNullOrWhiteSpace(artifact.Actual.ImageId) &&
           !string.IsNullOrWhiteSpace(artifact.Expected.Sha256) &&
           !string.IsNullOrWhiteSpace(artifact.Actual.Sha256);

    private static bool HasTrustedVisualProviderCallReceipt(
        IReadOnlyDictionary<string, string> metadata,
        string? currentJobId,
        string? currentRootRequestId,
        string expectedProviderRequestId,
        string? expectedComparisonRequestId,
        string? expectedVisualJudgeRequestId,
        string? expectedVisualResultId,
        string? expectedComparisonArtifactId,
        string expectedProviderName,
        string expectedProviderModel)
    {
        if (!IsTruthy(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey) ||
            !MetadataEquals(
                metadata,
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVersionKey,
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVersion) ||
            !MetadataEquals(
                metadata,
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptIdKey,
                $"{expectedProviderRequestId}:server-visual-provider-call-receipt") ||
            !MetadataEquals(
                metadata,
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJudgeRequestIdKey,
                expectedProviderRequestId) ||
            !MetadataEquals(
                metadata,
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey,
                expectedProviderRequestId) ||
            !MetadataEquals(
                metadata,
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderNameKey,
                expectedProviderName) ||
            !MetadataEquals(
                metadata,
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderModelKey,
                expectedProviderModel) ||
            !IsTruthy(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey) ||
            !IsTruthy(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey) ||
            !IsTruthy(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey) ||
            !HasSafeProviderIdentity(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRequestIdKey) ||
            !HasSafeProviderIdentity(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRootRequestIdKey) ||
            !HasSafeProviderIdentity(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey) ||
            !HasSafeProviderIdentity(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptEndpointHostKey))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(expectedComparisonRequestId) &&
            !MetadataEquals(
                metadata,
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey,
                expectedComparisonRequestId))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(expectedVisualJudgeRequestId) &&
            !MetadataEquals(
                metadata,
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualJudgeRequestIdKey,
                expectedVisualJudgeRequestId))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(expectedVisualResultId) &&
            !MetadataEquals(
                metadata,
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualResultIdKey,
                expectedVisualResultId))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(expectedComparisonArtifactId) &&
            !MetadataEquals(
                metadata,
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey,
                expectedComparisonArtifactId))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(currentRootRequestId) &&
            (!MetadataEquals(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRootRequestIdKey, currentRootRequestId) ||
             !MetadataEquals(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRequestIdKey, currentRootRequestId)))
        {
            return false;
        }

        return string.IsNullOrWhiteSpace(currentJobId) ||
               MetadataEquals(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJobIdKey, currentJobId);
    }

    private static bool HasTrustedVisualProviderCallReceipt(JsonObject metadata)
    {
        if (!IsTruthy(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey) ||
            !MetadataEquals(
                metadata,
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVersionKey,
                ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVersion) ||
            !IsTruthy(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey) ||
            !IsTruthy(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey) ||
            !IsTruthy(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey) ||
            !HasJsonValue(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptIdKey) ||
            !HasJsonValue(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJobIdKey) ||
            !HasJsonValue(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJudgeRequestIdKey) ||
            !HasJsonValue(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRequestIdKey) ||
            !HasJsonValue(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRootRequestIdKey) ||
            !HasJsonValue(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey) ||
            !HasJsonValue(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualResultIdKey) ||
            !HasJsonValue(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey) ||
            !HasJsonValue(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderNameKey) ||
            !HasJsonValue(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderModelKey) ||
            !HasJsonValue(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptEndpointHostKey))
        {
            return false;
        }

        return JsonValuesMatchWhenPresent(
                   metadata,
                   ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderNameKey,
                   "provider_name") &&
               JsonValuesMatchWhenPresent(
                   metadata,
                   ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderModelKey,
                   "provider_model") &&
               JsonValueContains(
                   metadata,
                   ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualResultIdKey,
                   $":{ExternalHttpDesignConversionVisualJudge.ServerOwnedVisualResultSegment}") &&
               JsonComparisonArtifactLooksCanonical(metadata) &&
               JsonReceiptIdentityLooksCanonical(metadata);
    }

    private static bool HasSafeProviderIdentity(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) &&
           !string.IsNullOrWhiteSpace(value) &&
           !string.Equals(value, "none", StringComparison.OrdinalIgnoreCase) &&
           !string.Equals(value, "missing", StringComparison.OrdinalIgnoreCase) &&
           !string.Equals(value, "unconfigured", StringComparison.OrdinalIgnoreCase);

    private static bool HasSafeProviderIdentity(JsonObject metadata, string key)
        => TryGetJsonValue(metadata, key) is { } value &&
           !string.IsNullOrWhiteSpace(value) &&
           !string.Equals(value, "none", StringComparison.OrdinalIgnoreCase) &&
           !string.Equals(value, "missing", StringComparison.OrdinalIgnoreCase) &&
           !string.Equals(value, "unconfigured", StringComparison.OrdinalIgnoreCase);

    private static bool IsTruthy(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) &&
           (string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "1", StringComparison.Ordinal) ||
            string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase));

    private static bool IsTruthy(JsonObject metadata, string key)
        => metadata.TryGetPropertyValue(key, out var node) &&
           (string.Equals(node?.GetValue<string>(), bool.TrueString, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(node?.GetValue<string>(), "1", StringComparison.Ordinal) ||
            string.Equals(node?.GetValue<string>(), "yes", StringComparison.OrdinalIgnoreCase));

    private static bool MetadataEquals(
        IReadOnlyDictionary<string, string> metadata,
        string key,
        string expected)
        => metadata.TryGetValue(key, out var value) &&
           string.Equals(value, expected, StringComparison.Ordinal);

    private static string? TryGetMetadataValue(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : null;

    private static bool MetadataEquals(JsonObject metadata, string key, string expected)
        => metadata.TryGetPropertyValue(key, out var node) &&
           string.Equals(node?.GetValue<string>(), expected, StringComparison.Ordinal);

    private static bool HasJsonValue(JsonObject metadata, string key)
        => !string.IsNullOrWhiteSpace(TryGetJsonValue(metadata, key));

    private static bool JsonValuesMatchWhenPresent(JsonObject metadata, string leftKey, string rightKey)
    {
        if (!metadata.TryGetPropertyValue(leftKey, out var left) ||
            !metadata.TryGetPropertyValue(rightKey, out var right) ||
            string.IsNullOrWhiteSpace(right?.GetValue<string>()))
        {
            return true;
        }

        return string.Equals(left?.GetValue<string>(), right.GetValue<string>(), StringComparison.Ordinal);
    }

    private static bool JsonValueContains(JsonObject metadata, string key, string expected)
        => metadata.TryGetPropertyValue(key, out var node) &&
           node?.GetValue<string>()?.Contains(expected, StringComparison.Ordinal) == true;

    private static bool JsonComparisonArtifactLooksCanonical(JsonObject metadata)
    {
        var artifactId = TryGetJsonValue(
            metadata,
            ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey);
        return !string.IsNullOrWhiteSpace(artifactId) &&
               artifactId.Contains(
                   $":{ExternalHttpDesignConversionVisualJudge.ServerOwnedVisualResultSegment}",
                   StringComparison.Ordinal) &&
               artifactId.EndsWith(":comparison-artifact", StringComparison.Ordinal);
    }

    private static bool JsonReceiptIdentityLooksCanonical(JsonObject metadata)
    {
        if (!TryGetSingleJsonScalar(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJobIdKey, out var jobId) ||
            !TryGetSingleJsonScalar(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRequestIdKey, out var requestId) ||
            !TryGetSingleJsonScalar(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRootRequestIdKey, out var rootRequestId) ||
            !string.Equals(requestId, rootRequestId, StringComparison.Ordinal) ||
            !TryGetJsonScalarSet(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptIdKey, out var receiptIds) ||
            !TryGetJsonScalarSet(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJudgeRequestIdKey, out var judgeRequestIds) ||
            !TryGetJsonScalarSet(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey, out var providerRequestIds) ||
            !TryGetJsonScalarSet(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey, out var comparisonRequestIds) ||
            !TryGetJsonScalarSet(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualJudgeRequestIdKey, out var visualJudgeRequestIds) ||
            !TryGetJsonScalarSet(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualResultIdKey, out var visualResultIds) ||
            !TryGetJsonScalarSet(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey, out var comparisonArtifactIds) ||
            comparisonRequestIds.Length == 0 ||
            !SetEquals(judgeRequestIds, comparisonRequestIds))
        {
            return false;
        }

        var comparisonRequestIdsByViewport = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var comparisonRequestId in comparisonRequestIds)
        {
            if (!TryGetComparisonViewportSuffix(comparisonRequestId, jobId, out var viewportSuffix) ||
                !comparisonRequestIdsByViewport.TryAdd(viewportSuffix, comparisonRequestId))
            {
                return false;
            }
        }

        var expectedVisualJudgeRequestIds = comparisonRequestIdsByViewport
            .Keys
            .Select(viewportSuffix => $"{jobId}:visual-judge-request:{viewportSuffix}")
            .ToArray();
        var expectedProviderRequestIds = comparisonRequestIds.Concat(expectedVisualJudgeRequestIds).ToArray();
        var expectedReceiptIds = comparisonRequestIds
            .Select(comparisonRequestId => $"{comparisonRequestId}:server-visual-provider-call-receipt")
            .ToArray();
        var expectedVisualResultIds = comparisonRequestIds
            .Select(comparisonRequestId => ExternalHttpDesignConversionVisualJudge.BuildCanonicalVisualResultId(
                comparisonRequestId,
                comparisonRequestId))
            .ToArray();
        var expectedComparisonArtifactIds = comparisonRequestIds
            .Select(comparisonRequestId => ExternalHttpDesignConversionVisualJudge.BuildCanonicalComparisonArtifactId(
                comparisonRequestId,
                comparisonRequestId))
            .ToArray();

        return SetEquals(receiptIds, expectedReceiptIds) &&
               SetEquals(providerRequestIds, expectedProviderRequestIds) &&
               SetEquals(visualJudgeRequestIds, expectedVisualJudgeRequestIds) &&
               SetEquals(visualResultIds, expectedVisualResultIds) &&
               SetEquals(comparisonArtifactIds, expectedComparisonArtifactIds);
    }

    private static bool TryGetSingleJsonScalar(JsonObject metadata, string key, out string value)
    {
        if (TryGetJsonScalarSet(metadata, key, out var values) &&
            values.Length == 1)
        {
            value = values[0];
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static bool TryGetJsonScalarSet(JsonObject metadata, string key, out string[] values)
    {
        values = Array.Empty<string>();
        var raw = TryGetJsonValue(metadata, key);
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        var parsed = raw
            .Split('|')
            .Select(value => value.Trim())
            .ToArray();
        if (parsed.Length == 0 ||
            parsed.Any(value => !IsSafeReceiptScalar(value)) ||
            parsed.Distinct(StringComparer.Ordinal).Count() != parsed.Length)
        {
            return false;
        }

        values = parsed;
        return true;
    }

    private static bool TryGetComparisonViewportSuffix(
        string comparisonRequestId,
        string jobId,
        out string viewportSuffix)
    {
        var prefix = $"{jobId}:comparison-request:";
        if (comparisonRequestId.StartsWith(prefix, StringComparison.Ordinal) &&
            comparisonRequestId.Length > prefix.Length)
        {
            viewportSuffix = comparisonRequestId[prefix.Length..];
            return IsSafeReceiptScalar(viewportSuffix);
        }

        viewportSuffix = string.Empty;
        return false;
    }

    private static bool SetEquals(IReadOnlyList<string> actual, IReadOnlyList<string> expected)
        => actual.Count == expected.Count &&
           new HashSet<string>(actual, StringComparer.Ordinal).SetEquals(expected);

    private static bool IsSafeReceiptScalar(string value)
        => !string.IsNullOrWhiteSpace(value) &&
           !string.Equals(value, "none", StringComparison.OrdinalIgnoreCase) &&
           !string.Equals(value, "missing", StringComparison.OrdinalIgnoreCase) &&
           !string.Equals(value, "unconfigured", StringComparison.OrdinalIgnoreCase);

    private static string? TryGetJsonValue(JsonObject metadata, string key)
        => metadata.TryGetPropertyValue(key, out var node) ? node?.GetValue<string>() : null;

    private static bool EvidenceContains(IReadOnlyList<string>? evidenceRefs, string expected)
        => evidenceRefs is not null && evidenceRefs.Contains(expected, StringComparer.Ordinal);
}
