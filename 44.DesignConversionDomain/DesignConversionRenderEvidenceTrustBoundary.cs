// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Globalization;
using AgentInfrastructure;

internal static class DesignConversionRenderEvidenceTrustBoundary
{
    public const string TrustBoundary = "server_owned_render_provider_boundary";
    public const string TrustBoundaryKey = "render_evidence_trust_boundary";
    public const string TrustClassificationKey = "render_evidence_trust_classification";
    public const string ProductionTrustedKey = "render_evidence_production_trusted";
    public const string ApprovalEvidenceAllowedKey = "approval_evidence_allowed";
    public const string SyntheticOrScaffoldKey = "synthetic_or_scaffold_render_result";
    public const string ScaffoldOrSyntheticRenderKey = "scaffold_or_synthetic_render";
    public const string BrowserBackedClassification = "browser_backed_render_evidence";
    public const string DeterministicOrScaffoldClassification = "deterministic_or_scaffold_render_evidence";
    public const string UntrustedInjectedClassification = "untrusted_injected_render_provider_evidence";

    public static OperationResult<DesignConversionRenderResult> Canonicalize(
        IDesignConversionRenderProvider renderProvider,
        DesignConversionRenderRequest request,
        DesignConversionRenderResult result)
    {
        if (renderProvider is null)
            return OperationResult<DesignConversionRenderResult>.Fail(
                "DesignConversion render provider is required at the trust boundary.",
                "RENDER_PROVIDER_REQUIRED");
        if (request is null)
            return OperationResult<DesignConversionRenderResult>.Fail(
                "DesignConversion render request is required at the trust boundary.",
                "RENDER_REQUEST_REQUIRED");
        if (result is null || result.Image is null)
            return OperationResult<DesignConversionRenderResult>.Fail(
                "DesignConversion render result is required at the trust boundary.",
                "RENDER_RESULT_REQUIRED");
        if (!string.Equals(result.RenderRequestId, request.RenderRequestId, StringComparison.Ordinal))
            return OperationResult<DesignConversionRenderResult>.Fail(
                "DesignConversion render result did not preserve the server-owned render request id.",
                "RENDER_RESULT_REQUEST_ID_MISMATCH");
        if (!SameResolution(result.Image.Resolution, request.Resolution))
            return OperationResult<DesignConversionRenderResult>.Fail(
                "DesignConversion render result did not preserve the server-owned render resolution.",
                "RENDER_RESULT_RESOLUTION_MISMATCH");

        var metadata = new Dictionary<string, string>(result.Image.Metadata ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        var providerKind = ResolveProviderKind(renderProvider);
        var receiptValidation = ValidateBrowserRenderProofReceipt(request, result, metadata);
        if (!receiptValidation.IsSuccess &&
            (renderProvider is BrowserBackedDesignConversionRenderProvider ||
             result.BrowserRenderProofReceipt is not null))
        {
            return receiptValidation.MapError<DesignConversionRenderResult>();
        }

        var validatedReceipt = receiptValidation.IsSuccess
            ? receiptValidation.Value!
            : null;
        var isTrustedBrowserRender = validatedReceipt is not null;
        var providerClaimedBrowser = IsTruthy(metadata, "browser_or_playwright_used") ||
                                     MetadataEquals(metadata, "provider_kind", BrowserBackedDesignConversionRenderProvider.ProviderKind) ||
                                     MetadataEquals(metadata, "hidden_determinism_boundary", "browser_backed_render_evidence");
        var classification = isTrustedBrowserRender
            ? BrowserBackedClassification
            : IsKnownDeterministicProvider(renderProvider)
                ? DeterministicOrScaffoldClassification
                : UntrustedInjectedClassification;

        var additions = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [TrustBoundaryKey] = TrustBoundary,
            [TrustClassificationKey] = classification,
            ["server_bound_request_id"] = request.RequestId,
            ["server_bound_render_request_id"] = request.RenderRequestId,
            ["server_bound_render_provider_kind"] = providerKind,
            ["server_bound_viewport_id"] = request.Resolution.ResolutionId,
            ["server_bound_viewport_width"] = request.Resolution.Width.ToString(CultureInfo.InvariantCulture),
            ["server_bound_viewport_height"] = request.Resolution.Height.ToString(CultureInfo.InvariantCulture),
            ["server_bound_device_scale_factor"] = request.Resolution.DeviceScaleFactor.ToString("R", CultureInfo.InvariantCulture),
            ["product_ready"] = bool.FalseString,
            ["promotion_evidence"] = bool.FalseString,
            ["learned_evidence"] = bool.FalseString,
            ["model_training_verified"] = bool.FalseString
        };
        if (request.Metadata.TryGetValue("job_id", out var jobId) && !string.IsNullOrWhiteSpace(jobId))
            additions["server_bound_job_id"] = jobId;

        if (isTrustedBrowserRender)
        {
            additions["provider_kind"] = BrowserBackedDesignConversionRenderProvider.ProviderKind;
            additions["render_artifact_kind"] = "browser_screenshot";
            additions["visual_fidelity_claimed"] = bool.TrueString;
            additions["browser_or_playwright_used"] = bool.TrueString;
            additions["external_ai_used"] = bool.FalseString;
            additions["hidden_determinism_boundary"] = "browser_backed_render_evidence";
            additions["browser_render_proof_issuer_kind"] = validatedReceipt!.IssuerKind;
            additions["browser_render_proof_receipt_validated"] = bool.TrueString;
            additions["browser_screenshot_file_verified"] = bool.TrueString;
            additions["browser_screenshot_png_signature_verified"] = bool.TrueString;
            additions["browser_screenshot_sha256_verified"] = bool.TrueString;
            additions["browser_screenshot_nonblank_pixels_verified"] = bool.TrueString;
            additions["png_signature_verified"] = bool.TrueString;
            additions[ProductionTrustedKey] = bool.TrueString;
            additions[ApprovalEvidenceAllowedKey] = bool.TrueString;
            additions[SyntheticOrScaffoldKey] = bool.FalseString;
            additions[ScaffoldOrSyntheticRenderKey] = bool.FalseString;
            additions["media_type"] = "image/png";
            additions["content_type"] = "image/png";
            additions["png_width"] = validatedReceipt.PngWidth.ToString(CultureInfo.InvariantCulture);
            additions["png_height"] = validatedReceipt.PngHeight.ToString(CultureInfo.InvariantCulture);
            additions["png_dimensions_source"] = "browser_screenshot_byte_inspection";
            additions["browser_screenshot_byte_length"] = validatedReceipt.ByteLength.ToString(CultureInfo.InvariantCulture);
            additions["browser_screenshot_png_width"] = validatedReceipt.PngWidth.ToString(CultureInfo.InvariantCulture);
            additions["browser_screenshot_png_height"] = validatedReceipt.PngHeight.ToString(CultureInfo.InvariantCulture);
            additions["browser_screenshot_non_white_pixel_count"] = validatedReceipt.NonWhitePixelCount.ToString(CultureInfo.InvariantCulture);
            additions["browser_screenshot_distinct_color_count"] = validatedReceipt.DistinctColorCount.ToString(CultureInfo.InvariantCulture);
            additions["browser_render_evidence_server_canonicalized"] = bool.TrueString;
        }
        else
        {
            additions["provider_kind"] = providerKind;
            additions["render_artifact_kind"] = ResolveNonBrowserArtifactKind(renderProvider);
            additions["visual_fidelity_claimed"] = bool.FalseString;
            additions["browser_or_playwright_used"] = bool.FalseString;
            additions["external_ai_used"] = bool.FalseString;
            additions["hidden_determinism_boundary"] = ResolveNonBrowserBoundary(renderProvider);
            additions[ProductionTrustedKey] = bool.FalseString;
            additions[ApprovalEvidenceAllowedKey] = bool.FalseString;
            additions[SyntheticOrScaffoldKey] = bool.TrueString;
            additions[ScaffoldOrSyntheticRenderKey] = bool.TrueString;
            additions["browser_render_claim_overridden_by_server"] = providerClaimedBrowser.ToString();
            additions["scaffold_render_evidence_server_canonicalized"] = bool.TrueString;
        }

        foreach (var pair in additions)
            metadata[pair.Key] = pair.Value;

        var warnings = result.Warnings
            .Concat(isTrustedBrowserRender
                ? ["SERVER_OWNED_BROWSER_RENDER_PROVENANCE_CANONICALIZED"]
                : ["SERVER_OWNED_SCAFFOLD_RENDER_PROVENANCE_CANONICALIZED"])
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return OperationResult<DesignConversionRenderResult>.Ok(result with
        {
            Image = result.Image with { Metadata = metadata },
            Warnings = warnings
        });
    }

    private static OperationResult<DesignConversionBrowserRenderProofReceipt> ValidateBrowserRenderProofReceipt(
        DesignConversionRenderRequest request,
        DesignConversionRenderResult result,
        IReadOnlyDictionary<string, string> metadata)
    {
        var receipt = result.BrowserRenderProofReceipt;
        if (receipt is null)
        {
            return OperationResult<DesignConversionBrowserRenderProofReceipt>.Fail(
                "DesignConversion browser-backed render result requires an internal browser render proof receipt.",
                "BROWSER_RENDER_PROOF_RECEIPT_REQUIRED");
        }

        if (!string.Equals(receipt.IssuerKind, BrowserBackedDesignConversionRenderProvider.ProviderKind, StringComparison.Ordinal) ||
            !receipt.PngSignatureVerified ||
            !receipt.Sha256Verified ||
            !receipt.HasNonBlankPixels ||
            receipt.ByteLength <= 0 ||
            receipt.PngWidth <= 0 ||
            receipt.PngHeight <= 0 ||
            receipt.NonWhitePixelCount <= 0 ||
            receipt.DistinctColorCount <= 0 ||
            !IsNormalizedSha256(receipt.NormalizedSha256))
        {
            return OperationResult<DesignConversionBrowserRenderProofReceipt>.Fail(
                "DesignConversion browser render proof receipt is not a valid built-in byte-inspection receipt.",
                "BROWSER_RENDER_PROOF_RECEIPT_INVALID");
        }

        if (!TryReadMeasuredBrowserPngDimensions(metadata, out var measuredPngWidth, out var measuredPngHeight))
        {
            return OperationResult<DesignConversionBrowserRenderProofReceipt>.Fail(
                "DesignConversion browser-backed render result requires positive measured PNG width and height from browser screenshot byte inspection.",
                "BROWSER_RENDER_PNG_DIMENSION_PROOF_REQUIRED");
        }

        if (!TryReadPositiveInvariantLong(metadata, "browser_screenshot_byte_length", out var measuredByteLength) ||
            !TryReadPositiveInvariantInt(metadata, "browser_screenshot_non_white_pixel_count", out var measuredNonWhitePixels) ||
            !TryReadPositiveInvariantInt(metadata, "browser_screenshot_distinct_color_count", out var measuredDistinctColors))
        {
            return OperationResult<DesignConversionBrowserRenderProofReceipt>.Fail(
                "DesignConversion browser render proof receipt requires matching measured PNG byte and nonblank metadata.",
                "BROWSER_RENDER_PROOF_RECEIPT_BINDING_MISMATCH");
        }

        var imageSha = NormalizeSha256(result.Image.Sha256);
        if (!string.Equals(receipt.RenderResultId, result.RenderResultId, StringComparison.Ordinal) ||
            !string.Equals(receipt.RenderRequestId, request.RenderRequestId, StringComparison.Ordinal) ||
            !string.Equals(receipt.RequestId, request.RequestId, StringComparison.Ordinal) ||
            !string.Equals(receipt.ImageId, result.Image.ImageId, StringComparison.Ordinal) ||
            !string.Equals(receipt.ScreenshotUri, result.Image.Uri, StringComparison.Ordinal) ||
            !string.Equals(receipt.NormalizedSha256, imageSha, StringComparison.Ordinal) ||
            receipt.PngWidth != measuredPngWidth ||
            receipt.PngHeight != measuredPngHeight ||
            receipt.ByteLength != measuredByteLength ||
            receipt.NonWhitePixelCount != measuredNonWhitePixels ||
            receipt.DistinctColorCount != measuredDistinctColors ||
            !string.Equals(receipt.ResolutionId, request.Resolution.ResolutionId, StringComparison.Ordinal) ||
            receipt.ViewportWidth != request.Resolution.Width ||
            receipt.ViewportHeight != request.Resolution.Height ||
            Math.Abs(receipt.DeviceScaleFactor - request.Resolution.DeviceScaleFactor) >= 0.000001)
        {
            return OperationResult<DesignConversionBrowserRenderProofReceipt>.Fail(
                "DesignConversion browser render proof receipt does not match the server request, result image, or measured PNG metadata.",
                "BROWSER_RENDER_PROOF_RECEIPT_BINDING_MISMATCH");
        }

        return OperationResult<DesignConversionBrowserRenderProofReceipt>.Ok(receipt);
    }

    private static bool TryReadMeasuredBrowserPngDimensions(
        IReadOnlyDictionary<string, string>? metadata,
        out int width,
        out int height)
    {
        width = 0;
        height = 0;
        return TryReadPositiveInvariantInt(metadata, "browser_screenshot_png_width", out width) &&
               TryReadPositiveInvariantInt(metadata, "browser_screenshot_png_height", out height);
    }

    private static bool TryReadPositiveInvariantInt(
        IReadOnlyDictionary<string, string>? metadata,
        string key,
        out int value)
    {
        value = 0;
        return metadata is not null &&
               metadata.TryGetValue(key, out var raw) &&
               int.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out value) &&
               value > 0;
    }

    private static bool TryReadPositiveInvariantLong(
        IReadOnlyDictionary<string, string>? metadata,
        string key,
        out long value)
    {
        value = 0;
        return metadata is not null &&
               metadata.TryGetValue(key, out var raw) &&
               long.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out value) &&
               value > 0;
    }

    private static string NormalizeSha256(string? sha256)
    {
        if (string.IsNullOrWhiteSpace(sha256))
            return string.Empty;

        var value = sha256.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase)
            ? sha256["sha256:".Length..]
            : sha256;
        return IsSha256Hex(value)
            ? value.ToLowerInvariant()
            : string.Empty;
    }

    private static bool IsNormalizedSha256(string? sha256)
        => sha256 is not null &&
           string.Equals(sha256, sha256.ToLowerInvariant(), StringComparison.Ordinal) &&
           IsSha256Hex(sha256);

    private static bool IsSha256Hex(string value)
        => value.Length == 64 && value.All(Uri.IsHexDigit);

    private static bool SameResolution(RenderQaResolution left, RenderQaResolution right)
        => left is not null &&
           right is not null &&
           string.Equals(left.ResolutionId, right.ResolutionId, StringComparison.Ordinal) &&
           left.Width == right.Width &&
           left.Height == right.Height &&
           Math.Abs(left.DeviceScaleFactor - right.DeviceScaleFactor) < 0.000001;

    private static string ResolveProviderKind(IDesignConversionRenderProvider renderProvider)
        => renderProvider switch
        {
            BrowserBackedDesignConversionRenderProvider => BrowserBackedDesignConversionRenderProvider.ProviderKind,
            DeterministicLocalDesignConversionPngRenderProvider => DeterministicLocalDesignConversionPngRenderProvider.ProviderKind,
            DeterministicDesignConversionRenderProvider => DeterministicDesignConversionRenderProvider.ProviderKind,
            _ => renderProvider.GetType().Name
        };

    private static bool IsKnownDeterministicProvider(IDesignConversionRenderProvider renderProvider)
        => renderProvider is DeterministicDesignConversionRenderProvider or DeterministicLocalDesignConversionPngRenderProvider;

    private static string ResolveNonBrowserArtifactKind(IDesignConversionRenderProvider renderProvider)
        => renderProvider switch
        {
            DeterministicLocalDesignConversionPngRenderProvider => "deterministic_local_png_artifact",
            DeterministicDesignConversionRenderProvider => "deterministic_render_evidence_record",
            _ => "untrusted_injected_render_artifact"
        };

    private static string ResolveNonBrowserBoundary(IDesignConversionRenderProvider renderProvider)
        => renderProvider switch
        {
            DeterministicLocalDesignConversionPngRenderProvider => "deterministic_local_png_artifact_evidence",
            DeterministicDesignConversionRenderProvider => "metadata_only_render_evidence",
            _ => "untrusted_injected_provider_render_evidence"
        };

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
