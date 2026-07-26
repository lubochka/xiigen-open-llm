// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Globalization;

internal static class DesignConversionExternalVisualAiReceiptValidator
{
    public const string ValidatedClassification = "server_owned_external_visual_ai_receipt_validated";
    public const string ValidatedEvidenceRef = "SERVER_OWNED_EXTERNAL_VISUAL_AI_RECEIPT_VALIDATED";
    public const string InvalidEvidenceRef = "SERVER_OWNED_EXTERNAL_VISUAL_AI_RECEIPT_INVALID";

    private static readonly HashSet<string> AllowedSelectedProviders = new(StringComparer.Ordinal)
    {
        "claude",
        "openai",
        "gemini"
    };

    public static ExternalVisualAiReceiptValidation ValidateComparisonArtifact(
        DesignConversionQaComparisonRequest request,
        DesignConversionQaComparisonArtifact artifact)
    {
        if (request is null)
            return ExternalVisualAiReceiptValidation.Fail("comparison_request_required");
        if (artifact is null || artifact.Verdict is null)
            return ExternalVisualAiReceiptValidation.Fail("comparison_artifact_required");
        if (artifact.Metadata is null)
            return ExternalVisualAiReceiptValidation.Fail("comparison_metadata_required");
        if (!string.Equals(artifact.ComparisonRequestId, request.ComparisonRequestId, StringComparison.Ordinal))
            return ExternalVisualAiReceiptValidation.Fail("comparison_request_id_mismatch");
        if (!SameImageRef(artifact.Expected, request.Expected) || !SameImageRef(artifact.Actual, request.Actual))
            return ExternalVisualAiReceiptValidation.Fail("comparison_image_refs_mismatch");
        if (request.Expected is null || request.Actual is null)
            return ExternalVisualAiReceiptValidation.Fail("comparison_request_images_required");

        var metadata = artifact.Metadata;
        var expectedProviderRequestId = request.ComparisonRequestId;
        var expectedVisualResultId = ExternalHttpDesignConversionVisualJudge.BuildCanonicalVisualResultId(
            expectedProviderRequestId,
            request.ComparisonRequestId);
        var expectedArtifactId = ExternalHttpDesignConversionVisualJudge.BuildCanonicalComparisonArtifactId(
            expectedProviderRequestId,
            request.ComparisonRequestId);
        var expectedJobId = ResolveJobId(request);

        if (!string.Equals(artifact.ArtifactId, expectedArtifactId, StringComparison.Ordinal))
            return ExternalVisualAiReceiptValidation.Fail("comparison_artifact_id_mismatch");
        if (!IsSafeIdentity(expectedJobId) || !IsSafeIdentity(request.RequestId))
            return ExternalVisualAiReceiptValidation.Fail("server_request_identity_missing");
        if (!MetadataEquals(metadata, "status", ExternalHttpDesignConversionVisualJudge.StatusOk))
            return ExternalVisualAiReceiptValidation.Fail("external_visual_status_not_success");
        if (IsTruthy(metadata, "external_visual_ai_blocked") ||
            !MetadataEquals(metadata, "blocked_reason", "none"))
        {
            return ExternalVisualAiReceiptValidation.Fail("external_visual_result_blocked");
        }

        var providerName = ReadMetadata(metadata, "provider_name");
        var providerModel = ReadMetadata(metadata, "provider_model");
        var endpointHost = ReadMetadata(metadata, "provider_endpoint_host");
        if (!IsSafeIdentity(providerName) || !IsSafeIdentity(providerModel) || !IsSafeIdentity(endpointHost))
            return ExternalVisualAiReceiptValidation.Fail("provider_identity_missing");
        if (!ProviderIdentityIsAllowed(metadata, providerName))
            return ExternalVisualAiReceiptValidation.Fail("provider_identity_not_allowed");
        if (!HasExternalExecutionMarkers(metadata))
            return ExternalVisualAiReceiptValidation.Fail("external_execution_markers_invalid");
        if (!HasResponseVerificationMarkers(metadata))
            return ExternalVisualAiReceiptValidation.Fail("response_verification_markers_invalid");
        if (!HasNoRawPayloadExposure(metadata))
            return ExternalVisualAiReceiptValidation.Fail("raw_payload_exposure_detected");
        if (!HasValidComparisonEvidenceRefs(artifact))
            return ExternalVisualAiReceiptValidation.Fail("external_evidence_refs_invalid");
        if (!HasValidReceipt(
                metadata,
                expectedJobId,
                request.RequestId,
                expectedProviderRequestId,
                request.ComparisonRequestId,
                ReceiptFieldExpectation.RequiredAbsent,
                expectedVisualResultId,
                ReceiptFieldExpectation.ForValue(expectedArtifactId),
                providerName,
                providerModel,
                endpointHost))
        {
            return ExternalVisualAiReceiptValidation.Fail("server_call_receipt_invalid");
        }

        return ExternalVisualAiReceiptValidation.Ok();
    }

    public static bool HasValidOriginalExternalReceipts(
        IReadOnlyList<DesignConversionQaComparisonArtifact> artifacts,
        IReadOnlyList<DesignConversionVisualJudgeResult> judgeResults,
        string finalJobId,
        string finalRequestId)
    {
        if (artifacts is null || judgeResults is null || artifacts.Count != judgeResults.Count)
            return false;
        if (!IsSafeIdentity(finalJobId) || !IsSafeIdentity(finalRequestId))
            return false;

        var matchedJudgeResultIndexes = new HashSet<int>();
        foreach (var artifact in artifacts)
        {
            var matchedIndex = -1;
            for (var index = 0; index < judgeResults.Count; index++)
            {
                if (!IsCorrespondingExternalJudgeResult(
                        artifact,
                        judgeResults[index],
                        finalJobId))
                {
                    continue;
                }

                if (matchedIndex >= 0)
                    return false;

                matchedIndex = index;
            }

            if (matchedIndex < 0 || !matchedJudgeResultIndexes.Add(matchedIndex))
                return false;

            var judgeResult = judgeResults[matchedIndex];
            if (!HasValidOriginalComparisonReceipt(
                    artifact,
                    judgeResult.Provider,
                    finalJobId,
                    finalRequestId) ||
                !HasValidOriginalJudgeReceipts(
                    judgeResult,
                    artifact.ComparisonRequestId,
                    finalJobId,
                    finalRequestId))
            {
                return false;
            }
        }

        return matchedJudgeResultIndexes.Count == judgeResults.Count;
    }

    private static bool IsCorrespondingExternalJudgeResult(
        DesignConversionQaComparisonArtifact artifact,
        DesignConversionVisualJudgeResult result,
        string finalJobId)
    {
        if (!TryBuildCanonicalJudgeRequestId(
                artifact.ComparisonRequestId,
                finalJobId,
                out var expectedJudgeRequestId))
        {
            return false;
        }

        return result.Provider.ExternalCallExecuted &&
               string.Equals(result.JudgeRequestId, expectedJudgeRequestId, StringComparison.Ordinal) &&
               MatchesReceiptField(
                   artifact.Metadata,
                   "provider_name",
                   ReceiptFieldExpectation.ForValue(result.Provider.ProviderName)) &&
               MatchesReceiptField(
                   artifact.Metadata,
                   "provider_model",
                   ReceiptFieldExpectation.ForValue(result.Provider.ModelId));
    }

    private static bool TryBuildCanonicalJudgeRequestId(
        string comparisonRequestId,
        string finalJobId,
        out string judgeRequestId)
    {
        var comparisonPrefix = $"{finalJobId}:comparison-request:";
        if (!comparisonRequestId.StartsWith(comparisonPrefix, StringComparison.Ordinal))
        {
            judgeRequestId = string.Empty;
            return false;
        }

        var viewportText = comparisonRequestId[comparisonPrefix.Length..];
        if (!int.TryParse(
                viewportText,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var viewport) ||
            viewport <= 0 ||
            !string.Equals(
                viewportText,
                viewport.ToString(CultureInfo.InvariantCulture),
                StringComparison.Ordinal))
        {
            judgeRequestId = string.Empty;
            return false;
        }

        judgeRequestId = $"{finalJobId}:visual-judge-request:{viewportText}";
        return true;
    }

    private static bool HasValidOriginalComparisonReceipt(
        DesignConversionQaComparisonArtifact artifact,
        DesignConversionVisualJudgeProviderMetadata expectedProvider,
        string finalJobId,
        string finalRequestId)
    {
        var metadata = artifact.Metadata;
        var providerRequestId = ReadMetadata(
            metadata,
            ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey);
        var expectedVisualResultId =
            ExternalHttpDesignConversionVisualJudge.BuildCanonicalVisualResultId(
                providerRequestId,
                artifact.ComparisonRequestId);
        var expectedArtifactId =
            ExternalHttpDesignConversionVisualJudge.BuildCanonicalComparisonArtifactId(
                providerRequestId,
                artifact.ComparisonRequestId);

        return artifact.Expected is not null &&
               artifact.Actual is not null &&
               IsSafeIdentity(artifact.Expected.ImageId) &&
               IsSafeIdentity(artifact.Actual.ImageId) &&
               IsSafeIdentity(artifact.Expected.Sha256) &&
               IsSafeIdentity(artifact.Actual.Sha256) &&
               artifact.ComparisonRequestId.StartsWith(
                   $"{finalJobId}:comparison-request:",
                   StringComparison.Ordinal) &&
               string.Equals(providerRequestId, artifact.ComparisonRequestId, StringComparison.Ordinal) &&
               string.Equals(artifact.ArtifactId, expectedArtifactId, StringComparison.Ordinal) &&
               HasExternalExecutionMarkers(metadata) &&
               HasResponseVerificationMarkers(metadata) &&
               HasProviderPolicyExecuted(metadata) &&
               IsSafeIdentity(expectedProvider.ProviderName) &&
               IsSafeIdentity(expectedProvider.ModelId) &&
               IsSafeIdentity(expectedProvider.EndpointHost) &&
               MatchesReceiptField(
                   metadata,
                   "provider_name",
                   ReceiptFieldExpectation.ForValue(expectedProvider.ProviderName)) &&
               MatchesReceiptField(
                   metadata,
                   "provider_model",
                   ReceiptFieldExpectation.ForValue(expectedProvider.ModelId)) &&
               MatchesReceiptField(
                   metadata,
                   "provider_endpoint_host",
                   ReceiptFieldExpectation.ForValue(expectedProvider.EndpointHost)) &&
               HasValidComparisonEvidenceRefs(artifact) &&
               HasValidReceipt(
                   metadata,
                   finalJobId,
                   finalRequestId,
                   providerRequestId,
                   artifact.ComparisonRequestId,
                   ReceiptFieldExpectation.RequiredAbsent,
                   expectedVisualResultId,
                   ReceiptFieldExpectation.ForValue(expectedArtifactId),
                   expectedProvider.ProviderName,
                   expectedProvider.ModelId,
                   expectedProvider.EndpointHost);
    }

    private static bool HasValidOriginalJudgeReceipts(
        DesignConversionVisualJudgeResult result,
        string expectedComparisonRequestId,
        string finalJobId,
        string finalRequestId)
    {
        var providerMetadata = result.Provider.Metadata;
        var resultMetadata = result.Metadata;
        var expectedVisualResultId =
            ExternalHttpDesignConversionVisualJudge.BuildCanonicalVisualResultId(
                result.JudgeRequestId,
                expectedComparisonRequestId);

        return result.Provider.ExternalCallExecuted &&
               !result.IsBlocked &&
               IsSafeIdentity(result.Provider.ProviderName) &&
               IsSafeIdentity(result.Provider.ModelId) &&
               IsSafeIdentity(result.Provider.EndpointHost) &&
               result.JudgeRequestId.StartsWith(
                   $"{finalJobId}:visual-judge-request:",
                   StringComparison.Ordinal) &&
               expectedComparisonRequestId.StartsWith(
                   $"{finalJobId}:comparison-request:",
                   StringComparison.Ordinal) &&
               string.Equals(result.JudgeResultId, expectedVisualResultId, StringComparison.Ordinal) &&
               HasValidJudgeTrustMarkers(providerMetadata) &&
               HasValidJudgeTrustMarkers(resultMetadata) &&
               HasProviderPolicyExecuted(providerMetadata) &&
               result.EvidenceRefs.Contains("external_ai_visual_judge_executed:true", StringComparer.Ordinal) &&
               HasValidReceipt(
                   providerMetadata,
                   finalJobId,
                   finalRequestId,
                   result.JudgeRequestId,
                   expectedComparisonRequestId,
                   ReceiptFieldExpectation.ForValue(result.JudgeRequestId),
                   expectedVisualResultId,
                   ReceiptFieldExpectation.RequiredAbsent,
                   result.Provider.ProviderName,
                   result.Provider.ModelId,
                   result.Provider.EndpointHost) &&
               HasValidReceipt(
                   resultMetadata,
                   finalJobId,
                   finalRequestId,
                   result.JudgeRequestId,
                   expectedComparisonRequestId,
                   ReceiptFieldExpectation.ForValue(result.JudgeRequestId),
                   expectedVisualResultId,
                   ReceiptFieldExpectation.RequiredAbsent,
                   result.Provider.ProviderName,
                   result.Provider.ModelId,
                   result.Provider.EndpointHost);
    }

    private static bool HasExternalExecutionMarkers(IReadOnlyDictionary<string, string> metadata)
        => MatchesReceiptField(metadata, "network_call_executed", ReceiptFieldExpectation.ForValue(bool.TrueString)) &&
           MatchesReceiptField(metadata, "real_external_provider_executed", ReceiptFieldExpectation.ForValue(bool.TrueString)) &&
           MatchesReceiptField(metadata, "external_visual_ai_executed", ReceiptFieldExpectation.ForValue(bool.TrueString)) &&
           MatchesReceiptField(metadata, "visual_ai_provider_contract_executed", ReceiptFieldExpectation.ForValue(bool.TrueString)) &&
           !IsTruthy(metadata, "fake_client") &&
           !IsTruthy(metadata, DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker);

    private static bool HasResponseVerificationMarkers(IReadOnlyDictionary<string, string> metadata)
        => MatchesReceiptField(metadata, "provider_response_identity_verified", ReceiptFieldExpectation.ForValue(bool.TrueString)) &&
           MatchesReceiptField(metadata, "provider_response_schema_verified", ReceiptFieldExpectation.ForValue(bool.TrueString));

    private static bool HasProviderPolicyExecuted(IReadOnlyDictionary<string, string> metadata)
        => MatchesReceiptField(
               metadata,
               DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey,
               ReceiptFieldExpectation.ForValue("executed")) &&
           MatchesReceiptField(
               metadata,
               DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey,
               ReceiptFieldExpectation.ForValue("external_executed"));

    private static bool HasValidJudgeTrustMarkers(IReadOnlyDictionary<string, string> metadata)
        => MatchesReceiptField(metadata, "network_call_executed", ReceiptFieldExpectation.ForValue(bool.TrueString)) &&
           HasResponseVerificationMarkers(metadata) &&
           !IsTruthy(metadata, "fake_client") &&
           MatchesReceiptField(
               metadata,
               DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
               ReceiptFieldExpectation.ForValue(bool.FalseString));

    private static bool HasValidComparisonEvidenceRefs(DesignConversionQaComparisonArtifact artifact)
        => artifact.EvidenceRefs.Contains("network_call_executed:true", StringComparer.Ordinal) &&
           artifact.EvidenceRefs.Contains("real_external_provider_executed:true", StringComparer.Ordinal) &&
           artifact.EvidenceRefs.Contains("external_ai_visual_judge_executed:true", StringComparer.Ordinal) &&
           !artifact.EvidenceRefs.Contains(DesignConversionExternalVisualAiComparisonProvider.FakeClientMarker, StringComparer.Ordinal) &&
           !artifact.EvidenceRefs.Contains(DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker, StringComparer.Ordinal) &&
           !artifact.EvidenceRefs.Contains(DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker, StringComparer.Ordinal);

    private static bool HasValidReceipt(
        IReadOnlyDictionary<string, string> metadata,
        string finalJobId,
        string finalRequestId,
        string expectedProviderRequestId,
        string expectedComparisonRequestId,
        ReceiptFieldExpectation expectedVisualJudgeRequestId,
        string expectedVisualResultId,
        ReceiptFieldExpectation expectedComparisonArtifactId,
        string expectedProviderName,
        string expectedProviderModel,
        string expectedEndpointHost)
        => MatchesReceiptField(
               metadata,
               ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey,
               ReceiptFieldExpectation.ForValue(bool.TrueString)) &&
           MatchesReceiptField(
               metadata,
               ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVersionKey,
               ReceiptFieldExpectation.ForValue(ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVersion)) &&
           MatchesReceiptField(
               metadata,
               ExternalHttpDesignConversionVisualJudge.ServerCallReceiptIdKey,
               ReceiptFieldExpectation.ForValue($"{expectedProviderRequestId}:server-visual-provider-call-receipt")) &&
           MatchesReceiptField(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJobIdKey, ReceiptFieldExpectation.ForValue(finalJobId)) &&
           MatchesReceiptField(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRequestIdKey, ReceiptFieldExpectation.ForValue(finalRequestId)) &&
           MatchesReceiptField(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRootRequestIdKey, ReceiptFieldExpectation.ForValue(finalRequestId)) &&
           MatchesReceiptField(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJudgeRequestIdKey, ReceiptFieldExpectation.ForValue(expectedProviderRequestId)) &&
           MatchesReceiptField(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey, ReceiptFieldExpectation.ForValue(expectedProviderRequestId)) &&
           MatchesReceiptField(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey, ReceiptFieldExpectation.ForValue(expectedComparisonRequestId)) &&
           MatchesReceiptField(
               metadata,
               ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualJudgeRequestIdKey,
               expectedVisualJudgeRequestId) &&
           MatchesReceiptField(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualResultIdKey, ReceiptFieldExpectation.ForValue(expectedVisualResultId)) &&
           MatchesReceiptField(
               metadata,
               ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey,
               expectedComparisonArtifactId) &&
           MatchesReceiptField(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderNameKey, ReceiptFieldExpectation.ForValue(expectedProviderName)) &&
           MatchesReceiptField(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderModelKey, ReceiptFieldExpectation.ForValue(expectedProviderModel)) &&
           MatchesReceiptField(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptEndpointHostKey, ReceiptFieldExpectation.ForValue(expectedEndpointHost)) &&
           MatchesReceiptField(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey, ReceiptFieldExpectation.ForValue(bool.TrueString)) &&
           MatchesReceiptField(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey, ReceiptFieldExpectation.ForValue(bool.TrueString)) &&
           MatchesReceiptField(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey, ReceiptFieldExpectation.ForValue(bool.TrueString));

    private static bool ProviderIdentityIsAllowed(
        IReadOnlyDictionary<string, string> metadata,
        string providerName)
    {
        if (!metadata.TryGetValue(DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey, out var selectedProvider) ||
            !AllowedSelectedProviders.Contains(selectedProvider))
        {
            return false;
        }

        if (!HasProviderPolicyExecuted(metadata))
            return false;

        return selectedProvider switch
        {
            "claude" => ProviderNameMatches(providerName, "claude") ||
                        ProviderNameMatches(providerName, "anthropic"),
            "openai" => ProviderNameMatches(providerName, "openai") ||
                        ProviderNameMatches(providerName, "gpt"),
            "gemini" => ProviderNameMatches(providerName, "gemini") ||
                        ProviderNameMatches(providerName, "google"),
            _ => false
        };
    }

    private static bool ProviderNameMatches(string providerName, string alias)
    {
        var normalized = providerName.Trim().ToLowerInvariant();
        if (string.Equals(normalized, alias, StringComparison.Ordinal))
            return true;

        return normalized.Length > alias.Length + 1 &&
               normalized.StartsWith(alias, StringComparison.Ordinal) &&
               normalized[alias.Length] is '-' or '_' or '.' or ':';
    }

    private static bool HasNoRawPayloadExposure(IReadOnlyDictionary<string, string> metadata)
    {
        foreach (var key in new[]
                 {
                     "raw_html_included",
                     "raw_css_included",
                     "raw_design_tree_included",
                     "raw_user_private_data_included",
                     "provider_payload_included",
                     "secret_value_included",
                     "external_visual_provider_api_key_value_included"
                 })
        {
            if (metadata.TryGetValue(key, out var value) && !IsFalseyOrEmpty(value))
                return false;
        }

        foreach (var pair in metadata)
        {
            var key = pair.Key.Trim().Replace("-", "_", StringComparison.Ordinal).ToLowerInvariant();
            if ((key is "html" or "css" or "raw_html" or "raw_css" or "tree" or "design_tree" or "original_tree" or "provider_payload") &&
                LooksLikeRawPayload(pair.Value))
            {
                return false;
            }
        }

        return true;
    }

    private static bool LooksLikeRawPayload(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           (value.Contains("<html", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("<main", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("<div", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("{\"", StringComparison.Ordinal) ||
            value.Contains("data-secret", StringComparison.OrdinalIgnoreCase));

    private static bool IsFalseyOrEmpty(string? value)
        => string.IsNullOrWhiteSpace(value) ||
           string.Equals(value, bool.FalseString, StringComparison.OrdinalIgnoreCase) ||
           string.Equals(value, "0", StringComparison.Ordinal) ||
           string.Equals(value, "no", StringComparison.OrdinalIgnoreCase);

    private static string ResolveJobId(DesignConversionQaComparisonRequest request)
    {
        if (request.Metadata is not null &&
            request.Metadata.TryGetValue("job_id", out var jobId) &&
            !string.IsNullOrWhiteSpace(jobId))
        {
            return jobId;
        }

        var markerIndex = request.ComparisonRequestId.IndexOf(":comparison-request:", StringComparison.Ordinal);
        return markerIndex > 0 ? request.ComparisonRequestId[..markerIndex] : string.Empty;
    }

    private static bool SameImageRef(RenderQaImageRecord left, RenderQaImageRecord right)
        => left is not null &&
           right is not null &&
           string.Equals(left.ImageId, right.ImageId, StringComparison.Ordinal) &&
           string.Equals(left.Uri, right.Uri, StringComparison.Ordinal) &&
           string.Equals(left.Sha256, right.Sha256, StringComparison.Ordinal) &&
           SameResolution(left.Resolution, right.Resolution);

    private static bool SameResolution(RenderQaResolution left, RenderQaResolution right)
        => left is not null &&
           right is not null &&
           string.Equals(left.ResolutionId, right.ResolutionId, StringComparison.Ordinal) &&
           left.Width == right.Width &&
           left.Height == right.Height &&
           Math.Abs(left.DeviceScaleFactor - right.DeviceScaleFactor) < 0.000001;

    private static bool MatchesReceiptField(
        IReadOnlyDictionary<string, string> metadata,
        string key,
        ReceiptFieldExpectation expectation)
        => expectation.Kind switch
        {
            ReceiptFieldExpectationKind.RequiredValue =>
                metadata.TryGetValue(key, out var value) &&
                string.Equals(value, expectation.Value, StringComparison.Ordinal),
            ReceiptFieldExpectationKind.RequiredEmpty =>
                metadata.TryGetValue(key, out var value) && value.Length == 0,
            ReceiptFieldExpectationKind.RequiredAbsent => !metadata.ContainsKey(key),
            _ => false
        };

    private static string ReadMetadata(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) ? value : string.Empty;

    private static bool IsTruthy(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) &&
           (string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "1", StringComparison.Ordinal) ||
            string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase));

    private static bool MetadataEquals(IReadOnlyDictionary<string, string> metadata, string key, string expected)
        => metadata.TryGetValue(key, out var value) &&
           string.Equals(value, expected, StringComparison.Ordinal);

    private static bool IsSafeIdentity(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           !string.Equals(value, "none", StringComparison.OrdinalIgnoreCase) &&
           !string.Equals(value, "missing", StringComparison.OrdinalIgnoreCase) &&
           !string.Equals(value, "unconfigured", StringComparison.OrdinalIgnoreCase);

    private enum ReceiptFieldExpectationKind
    {
        RequiredValue,
        RequiredEmpty,
        RequiredAbsent
    }

    private readonly record struct ReceiptFieldExpectation(
        ReceiptFieldExpectationKind Kind,
        string? Value)
    {
        public static ReceiptFieldExpectation ForValue(string value)
            => new(ReceiptFieldExpectationKind.RequiredValue, value);

        public static ReceiptFieldExpectation RequiredEmpty { get; } =
            new(ReceiptFieldExpectationKind.RequiredEmpty, null);

        public static ReceiptFieldExpectation RequiredAbsent { get; } =
            new(ReceiptFieldExpectationKind.RequiredAbsent, null);
    }
}

internal sealed record ExternalVisualAiReceiptValidation(bool IsValid, string Reason)
{
    public static ExternalVisualAiReceiptValidation Ok()
        => new(true, "valid");

    public static ExternalVisualAiReceiptValidation Fail(string reason)
        => new(false, reason);
}
