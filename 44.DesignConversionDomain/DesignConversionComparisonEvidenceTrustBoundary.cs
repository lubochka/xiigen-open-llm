// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Globalization;
using AgentInfrastructure;

internal static class DesignConversionComparisonEvidenceTrustBoundary
{
    public const string TrustBoundary = "server_owned_comparison_provider_boundary";
    public const string TrustBoundaryKey = "comparison_evidence_trust_boundary";
    public const string TrustClassificationKey = "comparison_evidence_trust_classification";
    public const string ServerOwnedLocalPngClassification = "server_owned_local_png_byte_inspected_comparison_evidence";
    public const string ServerOwnedExternalVisualAiReceiptValidatedClassification =
        DesignConversionExternalVisualAiReceiptValidator.ValidatedClassification;
    public const string DeterministicOrScaffoldClassification = "deterministic_or_scaffold_comparison_evidence";
    public const string UntrustedInjectedClassification = "untrusted_injected_comparison_provider_evidence";
    public const string ServerCanonicalizedKey = "comparison_evidence_server_canonicalized";

    public static OperationResult<DesignConversionQaComparisonArtifact> Canonicalize(
        IDesignConversionComparisonProvider comparisonProvider,
        DesignConversionQaComparisonRequest request,
        DesignConversionQaComparisonArtifact artifact)
    {
        if (comparisonProvider is null)
            return OperationResult<DesignConversionQaComparisonArtifact>.Fail(
                "DesignConversion comparison provider is required at the trust boundary.",
                "COMPARISON_PROVIDER_REQUIRED");
        if (request is null)
            return OperationResult<DesignConversionQaComparisonArtifact>.Fail(
                "DesignConversion comparison request is required at the trust boundary.",
                "COMPARISON_REQUEST_REQUIRED");
        if (artifact is null || artifact.Verdict is null)
            return OperationResult<DesignConversionQaComparisonArtifact>.Fail(
                "DesignConversion comparison artifact is required at the trust boundary.",
                "COMPARISON_ARTIFACT_REQUIRED");
        if (!string.Equals(artifact.ComparisonRequestId, request.ComparisonRequestId, StringComparison.Ordinal))
            return OperationResult<DesignConversionQaComparisonArtifact>.Fail(
                "DesignConversion comparison artifact did not preserve the server-owned comparison request id.",
                "COMPARISON_ARTIFACT_REQUEST_ID_MISMATCH");
        var providerKind = ResolveProviderKind(comparisonProvider);
        var metadata = new Dictionary<string, string>(artifact.Metadata ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        var imageRefsMatch = SameImageRef(artifact.Expected, request.Expected) &&
                             SameImageRef(artifact.Actual, request.Actual);
        var providerClaimedTrusted = IsTruthy(metadata, DesignConversionRenderEvidenceTrustBoundary.ProductionTrustedKey) ||
                                     IsTruthy(metadata, DesignConversionRenderEvidenceTrustBoundary.ApprovalEvidenceAllowedKey);
        if (!imageRefsMatch && comparisonProvider is ServerOwnedLocalPngComparisonProvider)
        {
            return OperationResult<DesignConversionQaComparisonArtifact>.Fail(
                "DesignConversion server-owned local PNG comparison artifact did not preserve the expected/rendered image refs.",
                "COMPARISON_ARTIFACT_IMAGE_REF_MISMATCH");
        }

        var trustedLocalPng = imageRefsMatch &&
                              comparisonProvider is ServerOwnedLocalPngComparisonProvider &&
                              IsValidServerOwnedLocalPngComparison(artifact, metadata);
        var externalReceiptValidation = comparisonProvider is DesignConversionExternalVisualAiComparisonProvider externalProvider
            ? externalProvider.OwnsServerOwnedExternalHttpVisualJudge
                ? DesignConversionExternalVisualAiReceiptValidator.ValidateComparisonArtifact(request, artifact)
                : ExternalVisualAiReceiptValidation.Fail("provider_does_not_own_external_http_visual_judge")
            : ExternalVisualAiReceiptValidation.Fail("not_external_visual_ai_provider");
        var trustedExternalVisualAi = externalReceiptValidation.IsValid;
        var classification = trustedLocalPng
            ? ServerOwnedLocalPngClassification
            : trustedExternalVisualAi
                ? ServerOwnedExternalVisualAiReceiptValidatedClassification
                : IsKnownDeterministicProvider(comparisonProvider)
                    ? DeterministicOrScaffoldClassification
                    : UntrustedInjectedClassification;

        var additions = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [TrustBoundaryKey] = TrustBoundary,
            [TrustClassificationKey] = classification,
            [ServerCanonicalizedKey] = bool.TrueString,
            ["server_bound_request_id"] = request.RequestId,
            ["server_bound_comparison_request_id"] = request.ComparisonRequestId,
            ["server_bound_comparison_provider_kind"] = providerKind,
            ["server_bound_viewport_id"] = request.Actual.Resolution.ResolutionId,
            ["server_bound_viewport_width"] = request.Actual.Resolution.Width.ToString(CultureInfo.InvariantCulture),
            ["server_bound_viewport_height"] = request.Actual.Resolution.Height.ToString(CultureInfo.InvariantCulture),
            ["server_bound_device_scale_factor"] = request.Actual.Resolution.DeviceScaleFactor.ToString("R", CultureInfo.InvariantCulture),
            ["server_bound_expected_image_id"] = request.Expected.ImageId,
            ["server_bound_rendered_image_id"] = request.Actual.ImageId,
            ["server_bound_expected_sha256"] = request.Expected.Sha256,
            ["server_bound_rendered_sha256"] = request.Actual.Sha256,
            ["comparison_image_refs_preserved"] = imageRefsMatch.ToString(),
            ["comparison_image_refs_rebound_by_server"] = (!imageRefsMatch).ToString(),
            ["provider_kind"] = providerKind,
            ["comparison_provider_kind"] = providerKind,
            ["product_ready"] = bool.FalseString,
            ["external_ai_used"] = bool.FalseString,
            ["model_weights_published"] = bool.FalseString,
            ["model_training_verified"] = bool.FalseString,
            ["training_authority_verified"] = bool.FalseString,
            ["dpo_eligible"] = bool.FalseString,
            ["comparison_provider_trust_claim_overridden_by_server"] = providerClaimedTrusted.ToString(),
            ["server_owned_external_visual_ai_receipt_validated"] = trustedExternalVisualAi.ToString(),
            ["server_owned_external_visual_ai_receipt_validation_reason"] = externalReceiptValidation.Reason
        };

        if (request.Metadata.TryGetValue("job_id", out var jobId) && !string.IsNullOrWhiteSpace(jobId))
            additions["server_bound_job_id"] = jobId;
        if (request.Metadata.TryGetValue("render_result_id", out var renderResultId) && !string.IsNullOrWhiteSpace(renderResultId))
            additions["server_bound_render_result_id"] = renderResultId;

        if (trustedLocalPng)
        {
            additions[DesignConversionRenderEvidenceTrustBoundary.ProductionTrustedKey] = bool.TrueString;
            additions[DesignConversionRenderEvidenceTrustBoundary.ApprovalEvidenceAllowedKey] = bool.TrueString;
            additions["comparison_artifact_kind"] = "server_owned_local_png_diff";
            additions["hidden_determinism_boundary"] = ServerOwnedLocalPngComparisonProvider.Boundary;
            additions["local_png_pixel_comparison"] = bool.TrueString;
            additions["visual_comparison_proven"] = bool.TrueString;
            additions["server_owned_local_png_comparison_trust_verified"] = bool.TrueString;
            additions["comparison_diff_readback_sha256_verified"] = bool.TrueString;
            additions["external_ai_visual_judge_executed"] = bool.FalseString;
            additions["network_call_executed"] = bool.FalseString;
            additions["real_external_provider_executed"] = bool.FalseString;
        }
        else if (trustedExternalVisualAi)
        {
            additions[DesignConversionRenderEvidenceTrustBoundary.ProductionTrustedKey] = bool.TrueString;
            additions[DesignConversionRenderEvidenceTrustBoundary.ApprovalEvidenceAllowedKey] = bool.TrueString;
            additions["comparison_artifact_kind"] = "server_owned_external_visual_ai_receipt";
            additions["hidden_determinism_boundary"] = ServerOwnedExternalVisualAiReceiptValidatedClassification;
            additions["local_png_pixel_comparison"] = bool.FalseString;
            additions["visual_comparison_proven"] = bool.TrueString;
            additions["server_owned_local_png_comparison_trust_verified"] = bool.FalseString;
            additions["server_owned_external_visual_ai_comparison_trust_verified"] = bool.TrueString;
            additions["comparison_diff_readback_sha256_verified"] = bool.FalseString;
            additions["external_ai_used"] = bool.TrueString;
            additions["external_ai_visual_judge_executed"] = bool.TrueString;
            additions["network_call_executed"] = bool.TrueString;
            additions["real_external_provider_executed"] = bool.TrueString;
        }
        else
        {
            additions[DesignConversionRenderEvidenceTrustBoundary.ProductionTrustedKey] = bool.FalseString;
            additions[DesignConversionRenderEvidenceTrustBoundary.ApprovalEvidenceAllowedKey] = bool.FalseString;
            additions["comparison_artifact_kind"] = ResolveUntrustedArtifactKind(comparisonProvider);
            additions["visual_comparison_proven"] = bool.FalseString;
            additions["server_owned_local_png_comparison_trust_verified"] = bool.FalseString;
            additions["server_owned_external_visual_ai_comparison_trust_verified"] = bool.FalseString;
            if (comparisonProvider is DesignConversionExternalVisualAiComparisonProvider)
            {
                additions["comparison_score_zeroed_by_trust_boundary"] = bool.TrueString;
                additions["comparison_pass_zeroed_by_trust_boundary"] = bool.TrueString;
            }
        }

        foreach (var pair in additions)
            metadata[pair.Key] = pair.Value;

        var canonicalizationEvidenceRefs = new List<string>
        {
            trustedLocalPng
                ? "SERVER_OWNED_LOCAL_PNG_COMPARISON_TRUST_CANONICALIZED"
                : trustedExternalVisualAi
                    ? DesignConversionExternalVisualAiReceiptValidator.ValidatedEvidenceRef
                    : "UNTRUSTED_COMPARISON_PROVIDER_TRUST_CANONICALIZED_FALSE"
        };
        if (!imageRefsMatch)
            canonicalizationEvidenceRefs.Add("UNTRUSTED_COMPARISON_PROVIDER_IMAGE_REFS_REBOUND_BY_SERVER");
        if (comparisonProvider is DesignConversionExternalVisualAiComparisonProvider && !trustedExternalVisualAi)
        {
            canonicalizationEvidenceRefs.Add(DesignConversionExternalVisualAiReceiptValidator.InvalidEvidenceRef);
            canonicalizationEvidenceRefs.Add($"server_owned_external_visual_ai_receipt_invalid:{externalReceiptValidation.Reason}");
            canonicalizationEvidenceRefs.Add("EXTERNAL_VISUAL_AI_COMPARISON_SCORE_ZEROED_BEFORE_DOWNSTREAM_JUDGE");
        }

        var evidenceRefs = artifact.EvidenceRefs
            .Concat(canonicalizationEvidenceRefs)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var verdictEvidenceRefs = artifact.Verdict.EvidenceRefs
            .Concat(canonicalizationEvidenceRefs)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var verdict = artifact.Verdict with { EvidenceRefs = verdictEvidenceRefs };
        var metrics = artifact.Metrics;
        if (comparisonProvider is DesignConversionExternalVisualAiComparisonProvider && !trustedExternalVisualAi)
        {
            verdict = verdict with { Score = 0.0, Passed = false };
            metrics = ZeroInvalidExternalComparisonMetrics(metrics);
        }
        else if (trustedExternalVisualAi)
        {
            metrics = AddTrustedExternalComparisonMetrics(metrics);
        }

        return OperationResult<DesignConversionQaComparisonArtifact>.Ok(artifact with
        {
            Expected = request.Expected,
            Actual = request.Actual,
            Metadata = metadata,
            Metrics = metrics,
            EvidenceRefs = evidenceRefs,
            Verdict = verdict
        });
    }

    private static bool IsValidServerOwnedLocalPngComparison(
        DesignConversionQaComparisonArtifact artifact,
        IReadOnlyDictionary<string, string> metadata)
        => MetadataEquals(metadata, "provider_kind", ServerOwnedLocalPngComparisonProvider.ProviderKind) &&
           MetadataEquals(metadata, "comparison_provider_kind", ServerOwnedLocalPngComparisonProvider.ProviderKind) &&
           MetadataEquals(metadata, "comparison_boundary", ServerOwnedLocalPngComparisonProvider.Boundary) &&
           MetadataEquals(metadata, "hidden_determinism_boundary", ServerOwnedLocalPngComparisonProvider.Boundary) &&
           IsTruthy(metadata, "local_png_pixel_comparison") &&
           IsTruthy(metadata, "visual_comparison_proven") &&
           IsTruthy(metadata, "expected_sha256_verified") &&
           IsTruthy(metadata, "rendered_sha256_verified") &&
           IsTruthy(metadata, "diff_sha256_verified") &&
           IsTruthy(metadata, "diff_png_signature_verified") &&
           IsTruthy(metadata, "diff_png_nonblank_pixels_verified") &&
           IsTruthy(metadata, "comparison_diff_readback_sha256_verified") &&
           ShaRefsMatch(artifact.DiffImageUri, artifact.DiffImageSha256) &&
           MetadataEquals(metadata, "diff_image_sha256", artifact.DiffImageSha256) &&
           EvidenceContains(artifact.EvidenceRefs, ServerOwnedLocalPngComparisonProvider.ProviderKind) &&
           EvidenceContains(artifact.EvidenceRefs, ServerOwnedLocalPngComparisonProvider.Boundary) &&
           EvidenceContains(artifact.Verdict.EvidenceRefs, ServerOwnedLocalPngComparisonProvider.ProviderKind) &&
           EvidenceContains(artifact.Verdict.EvidenceRefs, ServerOwnedLocalPngComparisonProvider.Boundary) &&
           MetricGreaterThanZero(artifact.Metrics, "visual_comparison_proven");

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

    private static bool ShaRefsMatch(string uri, string sha256)
    {
        var uriSha = ExtractSha(uri);
        var artifactSha = ExtractSha(sha256);
        return !string.IsNullOrWhiteSpace(uriSha) &&
               string.Equals(uriSha, artifactSha, StringComparison.Ordinal);
    }

    private static string ExtractSha(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var text = value.Trim();
        if (text.StartsWith("local-png-ref:", StringComparison.OrdinalIgnoreCase))
            text = text["local-png-ref:".Length..];
        if (text.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase))
            text = text["sha256:".Length..];

        return text.Length == 64 && text.All(Uri.IsHexDigit)
            ? text.ToLowerInvariant()
            : string.Empty;
    }

    private static string ResolveProviderKind(IDesignConversionComparisonProvider provider)
        => provider switch
        {
            ServerOwnedLocalPngComparisonProvider => ServerOwnedLocalPngComparisonProvider.ProviderKind,
            DeterministicDesignConversionComparisonProvider => DeterministicDesignConversionComparisonProvider.ProviderKind,
            DesignConversionExternalVisualAiComparisonProvider => DesignConversionExternalVisualAiComparisonProvider.ProviderKind,
            _ => provider.GetType().Name
        };

    private static bool IsKnownDeterministicProvider(IDesignConversionComparisonProvider provider)
        => provider is DeterministicDesignConversionComparisonProvider;

    private static string ResolveUntrustedArtifactKind(IDesignConversionComparisonProvider provider)
        => provider switch
        {
            DeterministicDesignConversionComparisonProvider => "metadata_only_comparison",
            DesignConversionExternalVisualAiComparisonProvider => "external_visual_ai_comparison_receipt_invalid",
            _ => "untrusted_injected_comparison_artifact"
        };

    private static IReadOnlyDictionary<string, double> ZeroInvalidExternalComparisonMetrics(
        IReadOnlyDictionary<string, double>? metrics)
    {
        var rewritten = new Dictionary<string, double>(metrics ?? new Dictionary<string, double>(), StringComparer.Ordinal)
        {
            ["visual_judge_score"] = 0.0,
            ["comparison_score"] = 0.0,
            ["visual_comparison_proven"] = 0.0,
            ["comparison_score_zeroed_by_trust_boundary"] = 1.0,
            ["comparison_pass_zeroed_by_trust_boundary"] = 1.0
        };
        return rewritten;
    }

    private static IReadOnlyDictionary<string, double> AddTrustedExternalComparisonMetrics(
        IReadOnlyDictionary<string, double>? metrics)
    {
        var rewritten = new Dictionary<string, double>(metrics ?? new Dictionary<string, double>(), StringComparer.Ordinal)
        {
            ["visual_comparison_proven"] = 1.0,
            ["server_owned_external_visual_ai_receipt_validated"] = 1.0
        };
        return rewritten;
    }

    private static bool EvidenceContains(IReadOnlyList<string>? evidenceRefs, string expected)
        => evidenceRefs is not null && evidenceRefs.Contains(expected, StringComparer.Ordinal);

    private static bool MetricGreaterThanZero(IReadOnlyDictionary<string, double>? metrics, string key)
        => metrics is not null && metrics.TryGetValue(key, out var value) && value > 0.0;

    private static bool IsTruthy(IReadOnlyDictionary<string, string>? metadata, string key)
        => metadata is not null &&
           metadata.TryGetValue(key, out var value) &&
           (string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "1", StringComparison.Ordinal) ||
            string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase));

    private static bool MetadataEquals(IReadOnlyDictionary<string, string>? metadata, string key, string expected)
        => metadata is not null &&
           metadata.TryGetValue(key, out var value) &&
           string.Equals(value, expected, StringComparison.Ordinal);
}
