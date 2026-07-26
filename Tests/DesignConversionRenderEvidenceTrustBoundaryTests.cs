// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Reflection;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;

public sealed class DesignConversionRenderEvidenceTrustBoundaryTests
{
    private const string BrowserSha = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";
    private const string TrustClassificationKey = "render_evidence_trust_classification";
    private const string ProductionTrustedKey = "render_evidence_production_trusted";
    private const string ApprovalEvidenceAllowedKey = "approval_evidence_allowed";
    private const string SyntheticOrScaffoldKey = "synthetic_or_scaffold_render_result";
    private const string BrowserBackedClassification = "browser_backed_render_evidence";
    private const string UntrustedInjectedClassification = "untrusted_injected_render_provider_evidence";

    [Fact]
    public void Canonicalize_BrowserReceiptUsesMeasuredPngDimensionsNotLogicalRequest()
    {
        var provider = new ReceiptCarryingWrapperProvider();
        var resolution = new RenderQaResolution("mobile", 390, 844, 2.0);
        var request = CreateRequest(resolution);
        var result = AttachBrowserReceipt(
            request,
            CreateBrowserResult(request, BrowserProofMetadata(
                measuredWidth: "780",
                measuredHeight: "1688",
                forgedGenericWidth: "1",
                forgedGenericHeight: "2",
                proofMarkersVerified: false)),
            measuredWidth: 780,
            measuredHeight: 1688);

        var canonical = Canonicalize(provider, request, result);

        Assert.True(canonical.IsSuccess, canonical.ErrorMessage);
        var metadata = canonical.Value!.Image.Metadata;
        Assert.Equal("780", metadata["png_width"]);
        Assert.Equal("1688", metadata["png_height"]);
        Assert.Equal("390", metadata["server_bound_viewport_width"]);
        Assert.Equal("844", metadata["server_bound_viewport_height"]);
        Assert.Equal("780", metadata["browser_screenshot_png_width"]);
        Assert.Equal("1688", metadata["browser_screenshot_png_height"]);
        Assert.Equal("browser_screenshot_byte_inspection", metadata["png_dimensions_source"]);
        Assert.Equal(BrowserBackedDesignConversionRenderProvider.ProviderKind, metadata["browser_render_proof_issuer_kind"]);
        Assert.Equal(nameof(ReceiptCarryingWrapperProvider), metadata["server_bound_render_provider_kind"]);
        Assert.Equal(bool.TrueString, metadata["browser_screenshot_file_verified"]);
        Assert.Equal(bool.TrueString, metadata["browser_screenshot_png_signature_verified"]);
        Assert.Equal(bool.TrueString, metadata["browser_screenshot_sha256_verified"]);
        Assert.Equal(bool.TrueString, metadata["browser_screenshot_nonblank_pixels_verified"]);
        Assert.Equal(bool.TrueString, metadata["png_signature_verified"]);
    }

    [Theory]
    [InlineData(null, "1688")]
    [InlineData("0", "1688")]
    [InlineData("780", "not-an-integer")]
    public void Canonicalize_BrowserReceiptWithoutPositiveMeasuredPngDimensionsFailsClosed(
        string? measuredWidth,
        string? measuredHeight)
    {
        var provider = new ReceiptCarryingWrapperProvider();
        var resolution = new RenderQaResolution("mobile", 390, 844, 2.0);
        var request = CreateRequest(resolution);
        var result = AttachBrowserReceipt(
            request,
            CreateBrowserResult(request, BrowserProofMetadata(
                measuredWidth,
                measuredHeight,
                forgedGenericWidth: "390",
                forgedGenericHeight: "844")),
            measuredWidth: 780,
            measuredHeight: 1688);

        var canonical = Canonicalize(provider, request, result);

        Assert.False(canonical.IsSuccess);
        Assert.Equal("BROWSER_RENDER_PNG_DIMENSION_PROOF_REQUIRED", canonical.ErrorCode);
    }

    [Fact]
    public async Task Canonicalize_BrowserProviderDecoratorRetainsReceiptAndBrowserTrust()
    {
        var fixtureRoot = Path.Combine(
            Path.GetTempPath(),
            $"n22-2-browser-receipt-tests-{Guid.NewGuid():N}");
        var resolution = new RenderQaResolution("fixture", 4, 4, 1.0);
        var request = CreateRequest(resolution);
        try
        {
            var inner = new BrowserBackedDesignConversionRenderProvider(new LocalPngBrowserAdapter(fixtureRoot));
            var provider = new WarningAddingRenderProvider(inner);

            var render = await provider.RenderAsync(request);
            Assert.True(render.IsSuccess, render.ErrorMessage);

            var canonical = Canonicalize(provider, request, render.Value!);

            Assert.True(canonical.IsSuccess, canonical.ErrorMessage);
            var metadata = canonical.Value!.Image.Metadata;
            Assert.Equal(BrowserBackedClassification, metadata[TrustClassificationKey]);
            Assert.Equal(bool.TrueString, metadata[ApprovalEvidenceAllowedKey]);
            Assert.Equal(bool.TrueString, metadata[ProductionTrustedKey]);
            Assert.Equal(nameof(WarningAddingRenderProvider), metadata["server_bound_render_provider_kind"]);
            Assert.Equal(BrowserBackedDesignConversionRenderProvider.ProviderKind, metadata["browser_render_proof_issuer_kind"]);
            Assert.Contains("WRAPPER_WARNING_ADDED", canonical.Value.Warnings);
        }
        finally
        {
            if (Directory.Exists(fixtureRoot))
                Directory.Delete(fixtureRoot, recursive: true);
        }

        Assert.False(Directory.Exists(fixtureRoot));
    }

    [Fact]
    public void Canonicalize_MarkerOnlyProviderWithForgedBrowserMetadataRemainsUntrusted()
    {
        var provider = new MarkerOnlyRenderProvider();
        var resolution = new RenderQaResolution("mobile", 390, 844, 2.0);
        var request = CreateRequest(resolution);
        var result = CreateBrowserResult(request, BrowserProofMetadata(
            measuredWidth: "780",
            measuredHeight: "1688",
            forgedGenericWidth: "780",
            forgedGenericHeight: "1688"));

        var canonical = Canonicalize(provider, request, result);

        Assert.True(canonical.IsSuccess, canonical.ErrorMessage);
        var metadata = canonical.Value!.Image.Metadata;
        Assert.Equal(UntrustedInjectedClassification, metadata[TrustClassificationKey]);
        Assert.Equal(bool.FalseString, metadata[ApprovalEvidenceAllowedKey]);
        Assert.Equal(bool.FalseString, metadata[ProductionTrustedKey]);
        Assert.Equal(bool.TrueString, metadata[SyntheticOrScaffoldKey]);
        Assert.Equal(bool.TrueString, metadata["browser_render_claim_overridden_by_server"]);
    }

    [Fact]
    public void Canonicalize_BrowserReceiptRenderResultIdMismatchFailsClosed()
    {
        var provider = new ReceiptCarryingWrapperProvider();
        var resolution = new RenderQaResolution("mobile", 390, 844, 2.0);
        var request = CreateRequest(resolution);
        var result = AttachBrowserReceipt(
            request,
            CreateBrowserResult(request, BrowserProofMetadata(
                measuredWidth: "780",
                measuredHeight: "1688",
                forgedGenericWidth: "780",
                forgedGenericHeight: "1688")),
            measuredWidth: 780,
            measuredHeight: 1688);
        result = result with { RenderResultId = "swapped-result-n22-2" };

        var canonical = Canonicalize(provider, request, result);

        Assert.False(canonical.IsSuccess);
        Assert.Equal("BROWSER_RENDER_PROOF_RECEIPT_BINDING_MISMATCH", canonical.ErrorCode);
    }

    [Fact]
    public void DesignConversionRenderResultJson_DoesNotExposeInternalBrowserReceipt()
    {
        var resolution = new RenderQaResolution("json", 10, 10, 1.0);
        var request = CreateRequest(resolution);
        var result = AttachBrowserReceipt(
            request,
            CreateBrowserResult(request, new Dictionary<string, string>()),
            measuredWidth: 10,
            measuredHeight: 10);

        var json = JsonSerializer.Serialize(result);

        Assert.DoesNotContain("BrowserRenderProofReceipt", json, StringComparison.Ordinal);
        Assert.DoesNotContain("browserRenderProofReceipt", json, StringComparison.Ordinal);
        Assert.DoesNotContain("IssuerKind", json, StringComparison.Ordinal);
        Assert.DoesNotContain("issuerKind", json, StringComparison.Ordinal);
    }

    private static OperationResult<DesignConversionRenderResult> Canonicalize(
        IDesignConversionRenderProvider provider,
        DesignConversionRenderRequest request,
        DesignConversionRenderResult result)
    {
        var boundaryType = typeof(BrowserBackedDesignConversionRenderProvider)
            .Assembly
            .GetType("DesignConversionDomain.DesignConversionRenderEvidenceTrustBoundary", throwOnError: true)!;
        var canonicalize = boundaryType.GetMethod(
            "Canonicalize",
            BindingFlags.Public | BindingFlags.Static)!;

        return Assert.IsType<OperationResult<DesignConversionRenderResult>>(
            canonicalize.Invoke(null, [provider, request, result]));
    }

    private static DesignConversionRenderRequest CreateRequest(RenderQaResolution resolution)
        => new(
            RenderRequestId: "render-n22-2",
            RequestId: "request-n22-2",
            Html: "<main>n22-2</main>",
            Css: "main{min-height:100vh}",
            Resolution: resolution,
            Metadata: new Dictionary<string, string>
            {
                ["job_id"] = "job-n22-2"
            });

    private static DesignConversionRenderResult CreateBrowserResult(
        DesignConversionRenderRequest request,
        IReadOnlyDictionary<string, string> metadata)
    {
        var image = new RenderQaImageRecord(
            ImageId: "image-n22-2",
            Label: "browser-render-screenshot",
            Resolution: request.Resolution,
            Uri: "file:///tmp/n22-2.png",
            Sha256: $"sha256:{BrowserSha}",
            Metadata: metadata);

        return new DesignConversionRenderResult(
            RenderResultId: "result-n22-2",
            RenderRequestId: request.RenderRequestId,
            Image: image,
            ElapsedMs: 1,
            Warnings: []);
    }

    private static DesignConversionRenderResult AttachBrowserReceipt(
        DesignConversionRenderRequest request,
        DesignConversionRenderResult result,
        int measuredWidth,
        int measuredHeight)
    {
        const long byteLength = 1234;
        const int nonWhitePixelCount = 17;
        const int distinctColorCount = 3;
        var receiptType = typeof(BrowserBackedDesignConversionRenderProvider)
            .Assembly
            .GetType("DesignConversionDomain.DesignConversionBrowserRenderProofReceipt", throwOnError: true)!;
        var receipt = Activator.CreateInstance(
            receiptType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            args:
            [
                BrowserBackedDesignConversionRenderProvider.ProviderKind,
                result.RenderResultId,
                request.RenderRequestId,
                request.RequestId,
                result.Image.ImageId,
                result.Image.Uri,
                NormalizeSha256(result.Image.Sha256),
                byteLength,
                measuredWidth,
                measuredHeight,
                true,
                true,
                true,
                nonWhitePixelCount,
                distinctColorCount,
                request.Resolution.ResolutionId,
                request.Resolution.Width,
                request.Resolution.Height,
                request.Resolution.DeviceScaleFactor
            ],
            culture: null)!;

        var receiptProperty = typeof(DesignConversionRenderResult).GetProperty(
            "BrowserRenderProofReceipt",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        receiptProperty.SetValue(result, receipt);
        return result;
    }

    private static IReadOnlyDictionary<string, string> BrowserProofMetadata(
        string? measuredWidth,
        string? measuredHeight,
        string forgedGenericWidth,
        string forgedGenericHeight,
        bool proofMarkersVerified = true)
    {
        var proofMarker = proofMarkersVerified.ToString();
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["provider_kind"] = BrowserBackedDesignConversionRenderProvider.ProviderKind,
            ["render_artifact_kind"] = "browser_screenshot",
            ["visual_fidelity_claimed"] = bool.TrueString,
            ["browser_or_playwright_used"] = bool.TrueString,
            ["external_ai_used"] = bool.FalseString,
            ["hidden_determinism_boundary"] = "browser_backed_render_evidence",
            ["browser_screenshot_file_verified"] = proofMarker,
            ["browser_screenshot_png_signature_verified"] = proofMarker,
            ["browser_screenshot_sha256_verified"] = proofMarker,
            ["browser_screenshot_nonblank_pixels_verified"] = proofMarker,
            ["png_signature_verified"] = proofMarker,
            ["browser_screenshot_byte_length"] = "1234",
            ["browser_screenshot_non_white_pixel_count"] = "17",
            ["browser_screenshot_distinct_color_count"] = "3",
            ["png_width"] = forgedGenericWidth,
            ["png_height"] = forgedGenericHeight
        };

        if (measuredWidth is not null)
            metadata["browser_screenshot_png_width"] = measuredWidth;
        if (measuredHeight is not null)
            metadata["browser_screenshot_png_height"] = measuredHeight;

        return metadata;
    }

    private static string NormalizeSha256(string? sha256)
    {
        if (string.IsNullOrWhiteSpace(sha256))
            return string.Empty;

        var value = sha256.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase)
            ? sha256["sha256:".Length..]
            : sha256;
        return value.ToLowerInvariant();
    }

    private sealed class ReceiptCarryingWrapperProvider : IDesignConversionRenderProvider
    {
        public Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Canonicalize tests must not call this provider.");
    }

    private sealed class MarkerOnlyRenderProvider : IDesignConversionRenderProvider
    {
        public Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Canonicalize tests must not call this provider.");
    }

    private sealed class WarningAddingRenderProvider(IDesignConversionRenderProvider inner) : IDesignConversionRenderProvider
    {
        public async Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
        {
            var rendered = await inner.RenderAsync(request, cancellationToken);
            if (!rendered.IsSuccess)
                return rendered;

            return OperationResult<DesignConversionRenderResult>.Ok(rendered.Value! with
            {
                Warnings = rendered.Value.Warnings
                    .Concat(["WRAPPER_WARNING_ADDED"])
                    .ToArray()
            });
        }
    }

    private sealed class LocalPngBrowserAdapter(string artifactRoot) : IDesignConversionBrowserProcessAdapter
    {
        public async Task<OperationResult<DesignConversionBrowserCaptureResult>> CaptureScreenshotAsync(
            DesignConversionBrowserCaptureRequest request,
            CancellationToken cancellationToken = default)
        {
            var render = await new DeterministicLocalDesignConversionPngRenderProvider(artifactRoot)
                .RenderAsync(
                    new DesignConversionRenderRequest(
                        RenderRequestId: $"{request.CaptureRequestId}:local-png-proof",
                        RequestId: request.RequestId,
                        Html: request.DocumentHtml,
                        Css: "body{margin:0}",
                        Resolution: request.Resolution,
                        Metadata: request.Metadata),
                    cancellationToken);
            if (!render.IsSuccess)
                return render.MapError<DesignConversionBrowserCaptureResult>();

            return OperationResult<DesignConversionBrowserCaptureResult>.Ok(
                new DesignConversionBrowserCaptureResult(
                    ScreenshotUri: render.Value!.Image.Uri,
                    ScreenshotSha256: render.Value.Image.Sha256,
                    ElapsedMs: 1,
                    Metadata: new Dictionary<string, string>
                    {
                        ["browser_adapter_kind"] = "local_png_fixture"
                    },
                    Warnings: []));
        }
    }
}
