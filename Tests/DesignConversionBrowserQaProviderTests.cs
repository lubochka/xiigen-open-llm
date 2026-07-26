// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using AgentInfrastructure;
using DesignConversionDomain;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

[Collection("ProcessSpawningTests")]
public sealed class DesignConversionBrowserQaProviderTests
{
    [Fact]
    public async Task RenderAsync_CreatesBrowserEvidenceForMultipleResolutions()
    {
        var adapter = new RecordingBrowserAdapter();
        var provider = new BrowserBackedDesignConversionRenderProvider(adapter);
        var desktop = new RenderQaResolution("desktop", 1440, 900, 1.0);
        var mobile = new RenderQaResolution("mobile", 390, 844, 3.0);

        var desktopResult = await provider.RenderAsync(CreateRenderRequest("render-desktop", desktop));
        var mobileResult = await provider.RenderAsync(CreateRenderRequest("render-mobile", mobile));

        Assert.True(desktopResult.IsSuccess, desktopResult.ErrorMessage);
        Assert.True(mobileResult.IsSuccess, mobileResult.ErrorMessage);
        Assert.Equal("browser-render-screenshot", desktopResult.Value!.Image.Label);
        Assert.Equal("browser_process_render_provider", desktopResult.Value.Image.Metadata["provider_kind"]);
        Assert.Equal(bool.TrueString, desktopResult.Value.Image.Metadata["visual_fidelity_claimed"]);
        Assert.Equal(bool.TrueString, desktopResult.Value.Image.Metadata["browser_or_playwright_used"]);
        Assert.Equal(bool.FalseString, desktopResult.Value.Image.Metadata["external_ai_used"]);
        Assert.Equal("browser_backed_render_evidence", desktopResult.Value.Image.Metadata["hidden_determinism_boundary"]);
        Assert.Equal(bool.TrueString, desktopResult.Value.Image.Metadata["browser_screenshot_file_verified"]);
        Assert.Equal(bool.TrueString, desktopResult.Value.Image.Metadata["browser_screenshot_png_signature_verified"]);
        Assert.Equal(bool.TrueString, desktopResult.Value.Image.Metadata["browser_screenshot_sha256_verified"]);
        Assert.Equal(bool.TrueString, desktopResult.Value.Image.Metadata["browser_screenshot_nonblank_pixels_verified"]);
        Assert.Equal(bool.TrueString, desktopResult.Value.Image.Metadata["png_signature_verified"]);
        Assert.StartsWith("file:///", desktopResult.Value.Image.Uri, StringComparison.Ordinal);
        Assert.True(File.Exists(new Uri(desktopResult.Value.Image.Uri).LocalPath));
        Assert.Equal("1440", desktopResult.Value.Image.Metadata["viewport_width"]);
        Assert.Equal("390", mobileResult.Value!.Image.Metadata["viewport_width"]);
        Assert.NotEqual(desktopResult.Value.Image.Sha256, mobileResult.Value.Image.Sha256);
        Assert.Equal(2, adapter.Requests.Count);
        Assert.Equal(1440, adapter.Requests[0].Resolution.Width);
        Assert.Equal(900, adapter.Requests[0].Resolution.Height);
        Assert.Equal(3.0, adapter.Requests[1].Resolution.DeviceScaleFactor);
        Assert.Contains("<style>.card", adapter.Requests[0].DocumentHtml, StringComparison.Ordinal);

        var capturedDocument = adapter.Requests[0].DocumentHtml;
        var (cspIndex, csp) = ExtractFirstCspContent(capturedDocument);
        var callerStyleIndex = capturedDocument.IndexOf(".card{display:grid", StringComparison.Ordinal);
        var callerContentIndex = capturedDocument.IndexOf("<main class=\"card\">", StringComparison.Ordinal);
        Assert.True(callerStyleIndex >= 0, "Captured browser document must contain the caller CSS.");
        Assert.True(callerContentIndex >= 0, "Captured browser document must contain the caller HTML.");
        Assert.True(cspIndex < callerStyleIndex, "Injected CSP must precede caller CSS.");
        Assert.True(cspIndex < callerContentIndex, "Injected CSP must precede caller HTML.");

        foreach (var directive in new[]
        {
            "default-src 'none'",
            "base-uri 'none'",
            "form-action 'none'",
            "frame-src 'none'",
            "child-src 'none'",
            "object-src 'none'",
            "script-src 'none'",
            "connect-src 'none'",
            "worker-src 'none'",
            "manifest-src 'none'",
            "prefetch-src 'none'",
            "img-src data: blob:"
        })
        {
            Assert.Contains(directive, csp, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("file:", csp, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("http:", csp, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("https:", csp, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(desktopResult.Value.Warnings);
    }

    [Fact]
    public async Task RenderAsync_AcceptsChromiumPhysicalScreenshotDimensionsForScaledViewport()
    {
        var adapter = new RecordingBrowserAdapter();
        var provider = new BrowserBackedDesignConversionRenderProvider(adapter);
        var resolution = new RenderQaResolution("scaled-100x50", 100, 50, 2.0);

        var result = await provider.RenderAsync(CreateRenderRequest("render-scaled-100x50", resolution));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var request = Assert.Single(adapter.Requests);
        Assert.Equal(100, request.Resolution.Width);
        Assert.Equal(50, request.Resolution.Height);
        Assert.Equal(2.0, request.Resolution.DeviceScaleFactor);
        Assert.Equal(100, result.Value!.Image.Resolution.Width);
        Assert.Equal(50, result.Value.Image.Resolution.Height);
        Assert.Equal(2.0, result.Value.Image.Resolution.DeviceScaleFactor);
        Assert.Equal("100", result.Value.Image.Metadata["viewport_width"]);
        Assert.Equal("50", result.Value.Image.Metadata["viewport_height"]);
        Assert.Equal("2", result.Value.Image.Metadata["device_scale_factor"]);
        Assert.Equal("image/png", result.Value.Image.Metadata["media_type"]);
        Assert.Equal("image/png", result.Value.Image.Metadata["content_type"]);
        Assert.Equal("200", result.Value.Image.Metadata["png_width"]);
        Assert.Equal("100", result.Value.Image.Metadata["png_height"]);
        Assert.Equal("200", result.Value.Image.Metadata["browser_screenshot_png_width"]);
        Assert.Equal("100", result.Value.Image.Metadata["browser_screenshot_png_height"]);

        var receipt = result.Value.BrowserRenderProofReceipt;
        Assert.NotNull(receipt);
        Assert.Equal(100, receipt!.ViewportWidth);
        Assert.Equal(50, receipt.ViewportHeight);
        Assert.Equal(2.0, receipt.DeviceScaleFactor);
        Assert.Equal(200, receipt.PngWidth);
        Assert.Equal(100, receipt.PngHeight);

        var png = DesignConversionBrowserPngEvidenceInspector.Inspect(result.Value.Image.Uri);
        Assert.True(png.IsSuccess, png.ErrorMessage);
        Assert.Equal(200, png.Value!.PngWidth);
        Assert.Equal(100, png.Value.PngHeight);
        Assert.Equal(png.Value.PngWidth.ToString(System.Globalization.CultureInfo.InvariantCulture), result.Value.Image.Metadata["png_width"]);
        Assert.Equal(png.Value.PngHeight.ToString(System.Globalization.CultureInfo.InvariantCulture), result.Value.Image.Metadata["png_height"]);
    }

    [Theory]
    [InlineData(1.25, 126, 64)]
    [InlineData(1.5, 152, 77)]
    public async Task RenderAsync_UsesCanonicalAwayFromZeroPhysicalDimensionsForFractionalScale(
        double deviceScaleFactor,
        int expectedPhysicalWidth,
        int expectedPhysicalHeight)
    {
        var logical = new RenderQaResolution($"fractional-{deviceScaleFactor:R}", 101, 51, deviceScaleFactor);
        var physical = DesignConversionBrowserPhysicalScreenshotResolution.Resolve(
            logical,
            "TEST_RESOLUTION_INVALID",
            "Test browser render");
        Assert.True(physical.IsSuccess, physical.ErrorMessage);
        Assert.Equal(expectedPhysicalWidth, physical.Value!.PhysicalWidth);
        Assert.Equal(expectedPhysicalHeight, physical.Value.PhysicalHeight);

        var adapter = new RecordingBrowserAdapter();
        var provider = new BrowserBackedDesignConversionRenderProvider(adapter);

        var result = await provider.RenderAsync(CreateRenderRequest(
            $"render-fractional-{deviceScaleFactor:R}",
            logical));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Single(adapter.Requests);
        Assert.Equal("101", result.Value!.Image.Metadata["viewport_width"]);
        Assert.Equal("51", result.Value.Image.Metadata["viewport_height"]);
        Assert.Equal(deviceScaleFactor.ToString("R", System.Globalization.CultureInfo.InvariantCulture), result.Value.Image.Metadata["device_scale_factor"]);
        Assert.Equal("image/png", result.Value.Image.Metadata["media_type"]);
        Assert.Equal("image/png", result.Value.Image.Metadata["content_type"]);
        Assert.Equal(expectedPhysicalWidth.ToString(System.Globalization.CultureInfo.InvariantCulture), result.Value.Image.Metadata["png_width"]);
        Assert.Equal(expectedPhysicalHeight.ToString(System.Globalization.CultureInfo.InvariantCulture), result.Value.Image.Metadata["png_height"]);
        Assert.Equal(expectedPhysicalWidth.ToString(System.Globalization.CultureInfo.InvariantCulture), result.Value.Image.Metadata["browser_screenshot_png_width"]);
        Assert.Equal(expectedPhysicalHeight.ToString(System.Globalization.CultureInfo.InvariantCulture), result.Value.Image.Metadata["browser_screenshot_png_height"]);
        Assert.Equal(expectedPhysicalWidth, result.Value.BrowserRenderProofReceipt!.PngWidth);
        Assert.Equal(expectedPhysicalHeight, result.Value.BrowserRenderProofReceipt.PngHeight);
        Assert.Equal(101, result.Value.BrowserRenderProofReceipt.ViewportWidth);
        Assert.Equal(51, result.Value.BrowserRenderProofReceipt.ViewportHeight);
    }

    [Fact]
    public async Task LocalPngComparisonProvider_AcceptsIdenticalServerOwnedPngRefsAndWritesVerifiedDiff()
    {
        var artifactRoot = NewQuotaTestRoot("design-conversion-local-png-comparison-identical");
        try
        {
            var resolution = new RenderQaResolution("comparison-identical", 24, 18, 1.0);
            var expected = WriteComparisonImage(
                artifactRoot,
                "expected",
                "expected-identical",
                resolution,
                SolidRgbaPng(24, 18, red: 40, green: 120, blue: 200, alpha: 255),
                useContentRef: true);
            var actual = WriteComparisonImage(
                artifactRoot,
                "actual",
                "actual-identical",
                resolution,
                SolidRgbaPng(24, 18, red: 40, green: 120, blue: 200, alpha: 255),
                useContentRef: false);
            var provider = new ServerOwnedLocalPngComparisonProvider(artifactRoot);

            var result = await provider.CompareAsync(new DesignConversionQaComparisonRequest(
                ComparisonRequestId: "comparison-identical-request",
                RequestId: "comparison-identical",
                Expected: expected,
                Actual: actual,
                Metadata: new Dictionary<string, string>()));

            Assert.True(result.IsSuccess, result.ErrorMessage);
            var artifact = result.Value!;
            Assert.Equal(ServerOwnedLocalPngComparisonProvider.ProviderKind, artifact.Metadata["provider_kind"]);
            Assert.Equal("server_owned_local_png_pixel_comparison", artifact.Metadata["hidden_determinism_boundary"]);
            Assert.Equal(bool.FalseString, artifact.Metadata["external_ai_visual_judge_executed"]);
            Assert.Equal("image/png", artifact.Metadata["media_type"]);
            Assert.Equal("24", artifact.Metadata["png_width"]);
            Assert.Equal("18", artifact.Metadata["png_height"]);
            Assert.StartsWith("local-png-ref:sha256:", artifact.DiffImageUri, StringComparison.Ordinal);
            Assert.Equal(1.0, artifact.Verdict.Score);
            Assert.True(artifact.Verdict.Passed);
            Assert.Equal(0.0, artifact.Metrics["visual_distance"]);
            Assert.DoesNotContain(DeterministicDesignConversionComparisonProvider.ProviderKind, artifact.EvidenceRefs);
            Assert.DoesNotContain(DeterministicDesignConversionComparisonProvider.VisualComparisonNotProvenMarker, artifact.EvidenceRefs);

            var diffPath = Assert.Single(
                Directory.EnumerateFiles(artifactRoot, "*.png", SearchOption.AllDirectories),
                path => path.Contains("comparison-diffs", StringComparison.OrdinalIgnoreCase));
            var diff = DesignConversionBrowserPngEvidenceInspector.Inspect(diffPath);
            Assert.True(diff.IsSuccess, diff.ErrorMessage);
            Assert.True(diff.Value!.HasNonBlankPixels);
            Assert.Equal(24, diff.Value.PngWidth);
            Assert.Equal(18, diff.Value.PngHeight);
            Assert.Equal("sha256:" + Sha256FileRaw(diffPath), artifact.DiffImageSha256);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public void BoundedContentReader_RejectsSeekableStreamThatReportsBoundedLengthThenEmitsPastLimitWithoutLeakage()
    {
        const int maxBytes = 32;
        var privateMarker = "PRIVATE_R3_SEEKABLE_OVERRUN_" + Guid.NewGuid().ToString("N");
        var payload = PayloadWithMarker(maxBytes, privateMarker);
        using var stream = new AdversarialReadStream(
            payload,
            canSeek: true,
            reportedLength: maxBytes);

        var result = DesignConversionBoundedContentReader.ReadToEnd(
            stream,
            maxBytes,
            "DesignConversion local PNG comparison expected image",
            "TEST_BOUNDED_CONTENT_SIZE_INVALID");

        Assert.False(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorCode));
        AssertNoPrivateContentLeak(result, privateMarker);
    }

    [Fact]
    public void BoundedContentReader_RejectsNonSeekableUnknownLengthStreamThatEmitsPastLimitWithoutLeakage()
    {
        const int maxBytes = 24;
        var privateMarker = "PRIVATE_R3_NON_SEEKABLE_OVERRUN_" + Guid.NewGuid().ToString("N");
        var payload = PayloadWithMarker(maxBytes, privateMarker);
        using var stream = new AdversarialReadStream(
            payload,
            canSeek: false,
            reportedLength: null);

        var result = DesignConversionBoundedContentReader.ReadToEnd(
            stream,
            maxBytes,
            "DesignConversion local PNG comparison rendered image",
            "TEST_BOUNDED_CONTENT_SIZE_INVALID");

        Assert.False(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorCode));
        AssertNoPrivateContentLeak(result, privateMarker);
    }

    [Fact]
    public void BoundedContentReader_AcceptsValidBoundedStreamWithExactBytes()
    {
        var expected = Encoding.ASCII.GetBytes("bounded-r3-image-content");
        using var stream = new AdversarialReadStream(
            expected,
            canSeek: true,
            reportedLength: expected.LongLength);

        var result = DesignConversionBoundedContentReader.ReadToEnd(
            stream,
            expected.LongLength,
            "DesignConversion local PNG comparison expected image");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public async Task BoundedContentReaderAsync_AcceptsValidBoundedStreamUsingReadAsyncOnly()
    {
        var expected = Encoding.ASCII.GetBytes("bounded-r3-async-image-content");
        using var stream = new AsyncOnlyReadStream(
            expected,
            canSeek: true,
            reportedLength: expected.LongLength);

        var result = await DesignConversionBoundedContentReader.ReadToEndAsync(
            stream,
            expected.LongLength,
            "DesignConversion local PNG comparison expected image");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(expected, result.Value);
        Assert.Equal(0, stream.SyncReadCallCount);
        Assert.True(stream.AsyncReadCallCount > 0);
    }

    [Fact]
    public async Task BoundedContentReaderAsync_RejectsSeekableStreamThatReportsBoundedLengthThenEmitsPastLimitWithoutLeakage()
    {
        const int maxBytes = 32;
        var privateMarker = "PRIVATE_R3_ASYNC_SEEKABLE_OVERRUN_" + Guid.NewGuid().ToString("N");
        var payload = PayloadWithMarker(maxBytes, privateMarker);
        using var stream = new AsyncOnlyReadStream(
            payload,
            canSeek: true,
            reportedLength: maxBytes);

        var result = await DesignConversionBoundedContentReader.ReadToEndAsync(
            stream,
            maxBytes,
            "DesignConversion local PNG comparison expected image",
            "TEST_BOUNDED_CONTENT_SIZE_INVALID");

        Assert.False(result.IsSuccess);
        Assert.Equal("TEST_BOUNDED_CONTENT_SIZE_INVALID", result.ErrorCode);
        Assert.Equal(0, stream.SyncReadCallCount);
        Assert.True(stream.AsyncReadCallCount > 0);
        AssertNoPrivateContentLeak(result, privateMarker);
    }

    [Fact]
    public async Task BoundedContentReaderAsync_PropagatesCancellationWithoutPrivateLeakageOrPartialSuccess()
    {
        const int maxBytes = 64;
        var privateMarker = "PRIVATE_R3_ASYNC_CANCEL_" + Guid.NewGuid().ToString("N");
        var payload = PayloadWithMarker(maxBytes, privateMarker);
        using var cancellation = new CancellationTokenSource();
        using var stream = new AsyncOnlyReadStream(
            payload,
            canSeek: false,
            reportedLength: null,
            maxChunkSize: 8,
            cancelBeforeReadNumber: 2,
            cancellationSource: cancellation);

        var ex = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => DesignConversionBoundedContentReader.ReadToEndAsync(
                stream,
                maxBytes,
                "DesignConversion local PNG comparison rendered image",
                "TEST_BOUNDED_CONTENT_SIZE_INVALID",
                cancellationToken: cancellation.Token));

        Assert.Equal(0, stream.SyncReadCallCount);
        Assert.Equal(2, stream.AsyncReadCallCount);
        Assert.DoesNotContain(privateMarker, ex.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(Convert.ToBase64String(Encoding.UTF8.GetBytes(privateMarker)), ex.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task BoundedContentReaderAsync_RejectsHardCapBeforeReading()
    {
        var privateMarker = "PRIVATE_R3_ASYNC_HARD_CAP_READ_" + Guid.NewGuid().ToString("N");
        using var stream = new AsyncOnlyReadStream(
            Encoding.UTF8.GetBytes(privateMarker),
            canSeek: false,
            reportedLength: null);

        var result = await DesignConversionBoundedContentReader.ReadToEndAsync(
            stream,
            DesignConversionBoundedContentReader.HardMaxBytes + 1,
            "DesignConversion bounded content");

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionBoundedContentReader.HardLimitErrorCode, result.ErrorCode);
        Assert.Equal(0, stream.SyncReadCallCount);
        Assert.Equal(0, stream.AsyncReadCallCount);
        AssertNoPrivateContentLeak(result, privateMarker);
    }

    [Fact]
    public void BoundedContentReader_RejectsLimitAboveHardCapBeforeReading()
    {
        var privateMarker = "PRIVATE_R3_HARD_CAP_READ_" + Guid.NewGuid().ToString("N");
        using var stream = new ThrowOnReadStream(privateMarker);

        var result = DesignConversionBoundedContentReader.ReadToEnd(
            stream,
            DesignConversionBoundedContentReader.HardMaxBytes + 1,
            "DesignConversion bounded content");

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionBoundedContentReader.HardLimitErrorCode, result.ErrorCode);
        Assert.Equal(0, stream.ReadCallCount);
        AssertNoPrivateContentLeak(result, privateMarker);
    }

    [Fact]
    public void LocalPngComparisonProvider_RejectsMaxArtifactBytesAboveScreenshotHardCapWithoutAllocating()
    {
        var artifactRoot = NewQuotaTestRoot("design-conversion-local-png-comparison-hard-cap");

        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => new ServerOwnedLocalPngComparisonProvider(
                artifactRoot,
                maxArtifactBytes: DesignConversionBrowserPngEvidenceInspector.MaxPngScreenshotBytes + 1));

        Assert.Equal("maxArtifactBytes", ex.ParamName);
    }

    [Fact]
    public async Task LocalPngComparisonProvider_ResolveAsyncAcceptsBoundedExternalImageContent()
    {
        var artifactRoot = NewQuotaTestRoot("design-conversion-external-image-content-valid");
        try
        {
            var resolution = new RenderQaResolution("external-image-valid", 16, 16, 1.0);
            var png = SolidRgbaPng(16, 16, red: 40, green: 120, blue: 200, alpha: 255);
            var image = WriteComparisonImage(
                artifactRoot,
                "expected",
                "external-image-valid",
                resolution,
                png,
                useContentRef: false);
            var provider = new ServerOwnedLocalPngComparisonProvider(artifactRoot);

            var result = await provider.ResolveAsync(image, "expected", maxBytes: png.Length);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal("image/png", result.Value!.MediaType);
            Assert.Equal(Sha256Bytes(png), result.Value.Sha256);
            Assert.Equal(png, result.Value.Bytes);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task LocalPngComparisonProvider_ComputesNonzeroDistanceForVisiblePngDifference()
    {
        var artifactRoot = NewQuotaTestRoot("design-conversion-local-png-comparison-different");
        try
        {
            var resolution = new RenderQaResolution("comparison-different", 20, 16, 1.0);
            var expected = WriteComparisonImage(
                artifactRoot,
                "expected",
                "expected-blue",
                resolution,
                SolidRgbaPng(20, 16, red: 20, green: 80, blue: 180, alpha: 255),
                useContentRef: true);
            var actual = WriteComparisonImage(
                artifactRoot,
                "actual",
                "actual-red",
                resolution,
                SolidRgbaPng(20, 16, red: 220, green: 40, blue: 40, alpha: 255),
                useContentRef: false);
            var provider = new ServerOwnedLocalPngComparisonProvider(artifactRoot);

            var result = await provider.CompareAsync(new DesignConversionQaComparisonRequest(
                ComparisonRequestId: "comparison-different-request",
                RequestId: "comparison-different",
                Expected: expected,
                Actual: actual,
                Metadata: new Dictionary<string, string>()));

            Assert.True(result.IsSuccess, result.ErrorMessage);
            var artifact = result.Value!;
            Assert.True(artifact.Metrics["visual_distance"] > 0.0);
            Assert.True(artifact.Metrics["different_pixel_count"] > 0.0);
            Assert.True(artifact.Verdict.Score < 1.0);
            Assert.Contains(artifact.Verdict.Defects, defect => defect.Category == "pixel-difference");
            var diffPath = Assert.Single(
                Directory.EnumerateFiles(artifactRoot, "*.png", SearchOption.AllDirectories),
                path => path.Contains("comparison-diffs", StringComparison.OrdinalIgnoreCase));
            var diff = DesignConversionBrowserPngEvidenceInspector.Inspect(diffPath);
            Assert.True(diff.IsSuccess, diff.ErrorMessage);
            Assert.True(diff.Value!.HasNonBlankPixels);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task LocalPngComparisonProvider_ComputesNonzeroDistanceForVisibleAlphaOnlyDifference()
    {
        var artifactRoot = NewQuotaTestRoot("design-conversion-local-png-comparison-alpha");
        try
        {
            var resolution = new RenderQaResolution("comparison-alpha", 18, 14, 1.0);
            var expected = WriteComparisonImage(
                artifactRoot,
                "expected",
                "expected-opaque-black",
                resolution,
                SolidRgbaPng(18, 14, red: 0, green: 0, blue: 0, alpha: 255),
                useContentRef: true);
            var actual = WriteComparisonImage(
                artifactRoot,
                "actual",
                "actual-translucent-black",
                resolution,
                SolidRgbaPng(18, 14, red: 0, green: 0, blue: 0, alpha: 128),
                useContentRef: false);
            var provider = new ServerOwnedLocalPngComparisonProvider(artifactRoot);

            var result = await provider.CompareAsync(new DesignConversionQaComparisonRequest(
                ComparisonRequestId: "comparison-alpha-request",
                RequestId: "comparison-alpha",
                Expected: expected,
                Actual: actual,
                Metadata: new Dictionary<string, string>()));

            Assert.True(result.IsSuccess, result.ErrorMessage);
            var artifact = result.Value!;
            Assert.True(artifact.Metrics["visual_distance"] > 0.0);
            Assert.True(artifact.Metrics["different_pixel_count"] > 0.0);
            Assert.True(artifact.Verdict.Score < 1.0);
            var diffPath = Assert.Single(
                Directory.EnumerateFiles(artifactRoot, "*.png", SearchOption.AllDirectories),
                path => path.Contains("comparison-diffs", StringComparison.OrdinalIgnoreCase));
            var diff = DesignConversionBrowserPngEvidenceInspector.Inspect(diffPath);
            Assert.True(diff.IsSuccess, diff.ErrorMessage);
            Assert.True(diff.Value!.HasNonBlankPixels);
            Assert.Equal(18, diff.Value.PngWidth);
            Assert.Equal(14, diff.Value.PngHeight);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task LocalPngComparisonProvider_AcceptsCanonicalPhysicalPngDimensionsForScaledResolution()
    {
        var artifactRoot = NewQuotaTestRoot("design-conversion-local-png-comparison-scaled-physical");
        try
        {
            var resolution = new RenderQaResolution("comparison-scaled-101x51-at-1-5", 101, 51, 1.5);
            var expected = WriteComparisonImage(
                artifactRoot,
                "expected",
                "expected-scaled-physical",
                resolution,
                SolidRgbaPng(152, 77, red: 40, green: 120, blue: 200, alpha: 255),
                useContentRef: true);
            var actual = WriteComparisonImage(
                artifactRoot,
                "actual",
                "actual-scaled-physical",
                resolution,
                SolidRgbaPng(152, 77, red: 40, green: 120, blue: 200, alpha: 255),
                useContentRef: false);
            var provider = new ServerOwnedLocalPngComparisonProvider(artifactRoot);

            var result = await provider.CompareAsync(new DesignConversionQaComparisonRequest(
                ComparisonRequestId: "comparison-scaled-physical-request",
                RequestId: "comparison-scaled-physical",
                Expected: expected,
                Actual: actual,
                Metadata: new Dictionary<string, string>()));

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal("152", result.Value!.Metadata["expected_png_width"]);
            Assert.Equal("77", result.Value.Metadata["expected_png_height"]);
            Assert.Equal("152", result.Value.Metadata["rendered_png_width"]);
            Assert.Equal("77", result.Value.Metadata["rendered_png_height"]);
            Assert.Equal("101", result.Value.Metadata["viewport_width"]);
            Assert.Equal("51", result.Value.Metadata["viewport_height"]);
            Assert.Equal("1.5", result.Value.Metadata["device_scale_factor"]);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task LocalPngComparisonProvider_RejectsLogicalSizedPngForScaledResolution()
    {
        var artifactRoot = NewQuotaTestRoot("design-conversion-local-png-comparison-scaled-logical");
        try
        {
            var resolution = new RenderQaResolution("comparison-scaled-101x51-at-1-5", 101, 51, 1.5);
            var expected = WriteComparisonImage(
                artifactRoot,
                "expected",
                "expected-scaled-logical",
                resolution,
                SolidRgbaPng(101, 51, red: 40, green: 120, blue: 200, alpha: 255),
                useContentRef: true);
            var actual = WriteComparisonImage(
                artifactRoot,
                "actual",
                "actual-scaled-physical",
                resolution,
                SolidRgbaPng(152, 77, red: 40, green: 120, blue: 200, alpha: 255),
                useContentRef: false);
            var provider = new ServerOwnedLocalPngComparisonProvider(artifactRoot);

            var result = await provider.CompareAsync(new DesignConversionQaComparisonRequest(
                ComparisonRequestId: "comparison-scaled-logical-request",
                RequestId: "comparison-scaled-logical",
                Expected: expected,
                Actual: actual,
                Metadata: new Dictionary<string, string>()));

            Assert.False(result.IsSuccess);
            Assert.Equal("LOCAL_PNG_COMPARISON_DIMENSION_MISMATCH", result.ErrorCode);
            Assert.DoesNotContain("LOCAL_PNG_COMPARISON_SHA", result.ErrorCode, StringComparison.Ordinal);
            Assert.DoesNotContain(artifactRoot, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Theory]
    [InlineData("sha")]
    [InlineData("missing")]
    [InlineData("dimension")]
    public async Task LocalPngComparisonProvider_FailsClosedForInvalidPngProof(string mode)
    {
        var artifactRoot = NewQuotaTestRoot($"design-conversion-local-png-comparison-{mode}");
        try
        {
            var resolution = new RenderQaResolution("comparison-invalid", 16, 12, 1.0);
            var expectedBytes = mode == "dimension"
                ? SolidRgbaPng(17, 12, red: 40, green: 120, blue: 200, alpha: 255)
                : SolidRgbaPng(16, 12, red: 40, green: 120, blue: 200, alpha: 255);
            var expected = WriteComparisonImage(
                artifactRoot,
                "expected",
                "expected-invalid",
                resolution,
                expectedBytes,
                useContentRef: true);
            var actual = WriteComparisonImage(
                artifactRoot,
                "actual",
                "actual-invalid",
                resolution,
                SolidRgbaPng(16, 12, red: 40, green: 120, blue: 200, alpha: 255),
                useContentRef: false);

            if (mode == "sha")
            {
                expected = expected with { Sha256 = "sha256:" + new string('a', 64) };
            }
            else if (mode == "missing")
            {
                expected = expected with
                {
                    Uri = "local-png-ref:sha256:" + new string('b', 64),
                    Sha256 = "sha256:" + new string('b', 64)
                };
            }

            var provider = new ServerOwnedLocalPngComparisonProvider(artifactRoot);

            var result = await provider.CompareAsync(new DesignConversionQaComparisonRequest(
                ComparisonRequestId: $"comparison-invalid-{mode}",
                RequestId: "comparison-invalid",
                Expected: expected,
                Actual: actual,
                Metadata: new Dictionary<string, string>()));

            Assert.False(result.IsSuccess);
            Assert.StartsWith("LOCAL_PNG_COMPARISON_", result.ErrorCode, StringComparison.Ordinal);
            Assert.DoesNotContain(artifactRoot, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task LocalPngComparisonProvider_ShortCircuitsHashLookupOnFirstExactShaBeforeLaterByteBudgetExhaustion()
    {
        var artifactRoot = NewQuotaTestRoot("design-conversion-local-png-comparison-short-circuit");
        try
        {
            var resolution = new RenderQaResolution("comparison-short-circuit", 8, 6, 1.0);
            Directory.CreateDirectory(artifactRoot);
            var matchPath = Path.Combine(artifactRoot, "a-exact-match.png");
            var actualPath = Path.Combine(artifactRoot, "m-actual-direct.png");
            var lateBudgetPath = Path.Combine(artifactRoot, "z-late-budget-exhaustion.png");
            var matchBytes = SolidRgbaPng(8, 6, red: 40, green: 120, blue: 200, alpha: 255);
            File.WriteAllBytes(matchPath, matchBytes);
            File.WriteAllBytes(actualPath, matchBytes);
            File.WriteAllBytes(lateBudgetPath, TwoColorRgbaPng(80, 80));
            Assert.True(new FileInfo(lateBudgetPath).Length > new FileInfo(matchPath).Length);

            var expected = ComparisonImageFromPath(
                "expected",
                "expected-short-circuit",
                resolution,
                matchPath,
                useContentRef: true);
            var actual = ComparisonImageFromPath(
                "actual",
                "actual-short-circuit",
                resolution,
                actualPath,
                useContentRef: false);
            var provider = new ServerOwnedLocalPngComparisonProvider(
                artifactRoot,
                maxHashLookupBytes: new FileInfo(matchPath).Length);

            var result = await provider.CompareAsync(new DesignConversionQaComparisonRequest(
                ComparisonRequestId: "comparison-short-circuit-request",
                RequestId: "comparison-short-circuit",
                Expected: expected,
                Actual: actual,
                Metadata: new Dictionary<string, string>()));

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(ServerOwnedLocalPngComparisonProvider.ProviderKind, result.Value!.Metadata["provider_kind"]);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task LocalPngComparisonProvider_FailsWithStableBudgetCodeWhenHashLookupByteBudgetExceededBeforeLateSha()
    {
        var artifactRoot = NewQuotaTestRoot("design-conversion-local-png-comparison-byte-budget");
        try
        {
            var resolution = new RenderQaResolution("comparison-byte-budget", 8, 6, 1.0);
            Directory.CreateDirectory(artifactRoot);
            var firstPath = Path.Combine(artifactRoot, "a-unmatched.png");
            var latePath = Path.Combine(artifactRoot, "z-late-target.png");
            File.WriteAllBytes(firstPath, SolidRgbaPng(8, 6, red: 20, green: 80, blue: 180, alpha: 255));
            File.WriteAllBytes(latePath, SolidRgbaPng(8, 6, red: 220, green: 40, blue: 40, alpha: 255));
            var lateSha = Sha256FileRaw(latePath);
            var expected = ComparisonImageFromPath(
                "expected",
                "expected-late-byte-budget",
                resolution,
                latePath,
                useContentRef: true);
            var actual = ComparisonImageFromPath(
                "actual",
                "actual-byte-budget",
                resolution,
                firstPath,
                useContentRef: false);
            var provider = new ServerOwnedLocalPngComparisonProvider(
                artifactRoot,
                maxHashLookupBytes: new FileInfo(firstPath).Length);

            var result = await provider.CompareAsync(new DesignConversionQaComparisonRequest(
                ComparisonRequestId: "comparison-byte-budget-request",
                RequestId: "comparison-byte-budget",
                Expected: expected with { Uri = $"local-png-ref:sha256:{lateSha}", Sha256 = $"sha256:{lateSha}" },
                Actual: actual,
                Metadata: new Dictionary<string, string>()));

            Assert.False(result.IsSuccess);
            Assert.Equal(ServerOwnedLocalPngComparisonProvider.HashLookupByteBudgetExceededCode, result.ErrorCode);
            Assert.DoesNotContain("LOCAL_PNG_COMPARISON_SHA_NOT_FOUND", result.ErrorCode, StringComparison.Ordinal);
            Assert.DoesNotContain(artifactRoot, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task LocalPngComparisonProvider_FailsWithStableBudgetCodeWhenHashLookupFileBudgetExceededBeforeLateSha()
    {
        var artifactRoot = NewQuotaTestRoot("design-conversion-local-png-comparison-file-budget");
        try
        {
            var resolution = new RenderQaResolution("comparison-file-budget", 8, 6, 1.0);
            Directory.CreateDirectory(artifactRoot);
            var firstPath = Path.Combine(artifactRoot, "a-unmatched.png");
            var latePath = Path.Combine(artifactRoot, "z-late-target.png");
            File.WriteAllBytes(firstPath, SolidRgbaPng(8, 6, red: 20, green: 80, blue: 180, alpha: 255));
            File.WriteAllBytes(latePath, SolidRgbaPng(8, 6, red: 220, green: 40, blue: 40, alpha: 255));
            var lateSha = Sha256FileRaw(latePath);
            var expected = ComparisonImageFromPath(
                "expected",
                "expected-late-file-budget",
                resolution,
                latePath,
                useContentRef: true);
            var actual = ComparisonImageFromPath(
                "actual",
                "actual-file-budget",
                resolution,
                firstPath,
                useContentRef: false);
            var provider = new ServerOwnedLocalPngComparisonProvider(
                artifactRoot,
                maxHashLookupFiles: 1);

            var result = await provider.CompareAsync(new DesignConversionQaComparisonRequest(
                ComparisonRequestId: "comparison-file-budget-request",
                RequestId: "comparison-file-budget",
                Expected: expected with { Uri = $"local-png-ref:sha256:{lateSha}", Sha256 = $"sha256:{lateSha}" },
                Actual: actual,
                Metadata: new Dictionary<string, string>()));

            Assert.False(result.IsSuccess);
            Assert.Equal(ServerOwnedLocalPngComparisonProvider.HashLookupFileBudgetExceededCode, result.ErrorCode);
            Assert.DoesNotContain("LOCAL_PNG_COMPARISON_SHA_NOT_FOUND", result.ErrorCode, StringComparison.Ordinal);
            Assert.DoesNotContain(artifactRoot, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task LocalPngComparisonProvider_FailsWithStableBudgetCodeWhenHashLookupDirectoryBudgetExceededBeforeLateSha()
    {
        var artifactRoot = NewQuotaTestRoot("design-conversion-local-png-comparison-directory-budget");
        try
        {
            var resolution = new RenderQaResolution("comparison-directory-budget", 8, 6, 1.0);
            Directory.CreateDirectory(artifactRoot);
            Directory.CreateDirectory(Path.Combine(artifactRoot, "a-before-budget"));
            var lateDirectory = Path.Combine(artifactRoot, "z-late-target");
            Directory.CreateDirectory(lateDirectory);
            var actualPath = Path.Combine(artifactRoot, "m-actual-direct.png");
            var latePath = Path.Combine(lateDirectory, "z-late-target.png");
            File.WriteAllBytes(actualPath, SolidRgbaPng(8, 6, red: 20, green: 80, blue: 180, alpha: 255));
            File.WriteAllBytes(latePath, SolidRgbaPng(8, 6, red: 220, green: 40, blue: 40, alpha: 255));
            var lateSha = Sha256FileRaw(latePath);
            var expected = ComparisonImageFromPath(
                "expected",
                "expected-late-directory-budget",
                resolution,
                latePath,
                useContentRef: true);
            var actual = ComparisonImageFromPath(
                "actual",
                "actual-directory-budget",
                resolution,
                actualPath,
                useContentRef: false);
            var provider = new ServerOwnedLocalPngComparisonProvider(
                artifactRoot,
                maxHashLookupDirectories: 2);

            var result = await provider.CompareAsync(new DesignConversionQaComparisonRequest(
                ComparisonRequestId: "comparison-directory-budget-request",
                RequestId: "comparison-directory-budget",
                Expected: expected with { Uri = $"local-png-ref:sha256:{lateSha}", Sha256 = $"sha256:{lateSha}" },
                Actual: actual,
                Metadata: new Dictionary<string, string>()));

            Assert.False(result.IsSuccess);
            Assert.Equal(ServerOwnedLocalPngComparisonProvider.HashLookupDirectoryBudgetExceededCode, result.ErrorCode);
            Assert.DoesNotContain("LOCAL_PNG_COMPARISON_SHA_NOT_FOUND", result.ErrorCode, StringComparison.Ordinal);
            Assert.DoesNotContain(artifactRoot, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Theory]
    [InlineData("directories")]
    [InlineData("files")]
    [InlineData("bytes")]
    public void LocalPngComparisonProvider_RejectsNonPositiveHashLookupBudgets(string budget)
    {
        var artifactRoot = NewQuotaTestRoot($"design-conversion-local-png-comparison-invalid-budget-{budget}");
        try
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _ = budget switch
                {
                    "directories" => new ServerOwnedLocalPngComparisonProvider(artifactRoot, maxHashLookupDirectories: 0),
                    "files" => new ServerOwnedLocalPngComparisonProvider(artifactRoot, maxHashLookupFiles: 0),
                    "bytes" => new ServerOwnedLocalPngComparisonProvider(artifactRoot, maxHashLookupBytes: 0),
                    _ => throw new InvalidOperationException("Unknown budget.")
                };
            });

            Assert.Contains("hash lookup", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task LocalPngComparisonProvider_PreservesRootAndReparseSafetyForHashLookup()
    {
        const string SecretMarker = "LOCAL_PNG_COMPARISON_REPARSE_TARGET_SECRET";
        var testRoot = NewQuotaTestRoot("design-conversion-local-png-comparison-reparse");
        var artifactRoot = Path.Combine(testRoot, "artifact-root");
        var outsideRoot = Path.Combine(testRoot, "outside-root");
        var outsidePath = Path.Combine(outsideRoot, "outside-target.png");
        var linkRoot = Path.Combine(artifactRoot, "m-linked-outside");
        var linkCreated = false;
        try
        {
            Directory.CreateDirectory(artifactRoot);
            Directory.CreateDirectory(outsideRoot);
            var resolution = new RenderQaResolution("comparison-reparse", 8, 6, 1.0);
            File.WriteAllText(Path.Combine(outsideRoot, "sentinel.txt"), SecretMarker, Encoding.UTF8);
            File.WriteAllBytes(outsidePath, SolidRgbaPng(8, 6, red: 40, green: 120, blue: 200, alpha: 255));
            var outsideDirect = ComparisonImageFromPath(
                "expected",
                "expected-outside-direct",
                resolution,
                outsidePath,
                useContentRef: false);
            var insidePath = Path.Combine(artifactRoot, "z-inside-actual.png");
            File.WriteAllBytes(insidePath, SolidRgbaPng(8, 6, red: 220, green: 40, blue: 40, alpha: 255));
            var inside = ComparisonImageFromPath(
                "actual",
                "actual-inside",
                resolution,
                insidePath,
                useContentRef: false);
            var provider = new ServerOwnedLocalPngComparisonProvider(artifactRoot);

            var escaped = await provider.CompareAsync(new DesignConversionQaComparisonRequest(
                ComparisonRequestId: "comparison-root-escaped-request",
                RequestId: "comparison-root-escaped",
                Expected: outsideDirect,
                Actual: inside,
                Metadata: new Dictionary<string, string>()));

            Assert.False(escaped.IsSuccess);
            Assert.Equal("LOCAL_PNG_COMPARISON_PATH_ESCAPED_ROOT", escaped.ErrorCode);
            Assert.DoesNotContain(testRoot, escaped.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(SecretMarker, escaped.ErrorMessage, StringComparison.Ordinal);

            try
            {
                Directory.CreateSymbolicLink(linkRoot, outsideRoot);
                linkCreated = true;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or PlatformNotSupportedException)
            {
                return;
            }

            var outsideSha = Sha256FileRaw(outsidePath);
            var reparseResult = await provider.CompareAsync(new DesignConversionQaComparisonRequest(
                ComparisonRequestId: "comparison-reparse-request",
                RequestId: "comparison-reparse",
                Expected: outsideDirect with
                {
                    Uri = $"local-png-ref:sha256:{outsideSha}",
                    Sha256 = $"sha256:{outsideSha}"
                },
                Actual: inside,
                Metadata: new Dictionary<string, string>()));

            Assert.False(reparseResult.IsSuccess);
            Assert.Equal("LOCAL_PNG_COMPARISON_SHA_NOT_FOUND", reparseResult.ErrorCode);
            Assert.DoesNotContain(testRoot, reparseResult.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(SecretMarker, reparseResult.ErrorMessage, StringComparison.Ordinal);
            Assert.True(File.Exists(outsidePath), outsidePath);
        }
        finally
        {
            if (linkCreated && Directory.Exists(linkRoot))
                Directory.Delete(linkRoot);
            DeleteDirectoryIfExists(testRoot);
        }
    }

    [Fact]
    public async Task RenderAsync_RejectsUnscaledPngForScaledViewport()
    {
        var adapter = new RecordingBrowserAdapter(useChromiumPhysicalDimensions: false);
        var provider = new BrowserBackedDesignConversionRenderProvider(adapter);

        var result = await provider.RenderAsync(CreateRenderRequest(
            "render-unscaled-png-at-1-5",
            new RenderQaResolution("scaled-mismatch", 101, 51, 1.5)));

        Assert.False(result.IsSuccess);
        Assert.Equal("BROWSER_SCREENSHOT_DIMENSION_MISMATCH", result.ErrorCode);
        Assert.Single(adapter.Requests);
        Assert.Contains("101x51", result.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains("152x77", result.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains("1.5", result.ErrorMessage, StringComparison.Ordinal);
    }

    public static TheoryData<RenderQaResolution> InvalidBrowserRenderResolutions =>
        new()
        {
            new RenderQaResolution("nan-dsf", 100, 50, double.NaN),
            new RenderQaResolution("infinite-dsf", 100, 50, double.PositiveInfinity),
            new RenderQaResolution("zero-width", 0, 50, 1.0),
            new RenderQaResolution("negative-height", 100, -50, 1.0),
            new RenderQaResolution("zero-dsf", 100, 50, 0.0),
            new RenderQaResolution("overflow-physical-width", int.MaxValue, 1, 2.0),
            new RenderQaResolution("pixel-cap-exceeded", 4097, 4097, 1.0)
        };

    [Theory]
    [MemberData(nameof(InvalidBrowserRenderResolutions))]
    public async Task RenderAsync_RejectsInvalidPhysicalResolutionsBeforeBrowserAdapter(RenderQaResolution resolution)
    {
        var adapter = new RecordingBrowserAdapter();
        var provider = new BrowserBackedDesignConversionRenderProvider(adapter);

        var result = await provider.RenderAsync(CreateRenderRequest(
            $"render-invalid-{resolution.ResolutionId}",
            resolution));

        Assert.False(result.IsSuccess);
        Assert.Equal("BROWSER_RENDER_RESOLUTION_INVALID", result.ErrorCode);
        Assert.Empty(adapter.Requests);
    }

    [Fact]
    public async Task LocalChromiumProcessAdapter_RejectsMalformedResolutionBeforeProcessSideEffects()
    {
        var artifactRoot = Path.GetFullPath(Path.Combine(
            "TestResults",
            "local-chromium-invalid-resolution",
            Guid.NewGuid().ToString("N")));
        var runner = new FakeLocalChromiumBrowserProcessRunner(WriteSuccessfulFakeBrowserScreenshotAsync);
        var adapter = CreateQuotaTestAdapter(
            artifactRoot,
            runner,
            LocalChromiumBrowserProcessAdapter.DefaultMaxRetainedCaptureBytes);

        try
        {
            var result = await adapter.CaptureScreenshotAsync(new DesignConversionBrowserCaptureRequest(
                CaptureRequestId: "capture-invalid-resolution",
                RequestId: "request-invalid-resolution",
                DocumentHtml: "<main>invalid resolution must fail before process</main>",
                Resolution: new RenderQaResolution("invalid-capture", 100, 50, double.PositiveInfinity),
                Metadata: new Dictionary<string, string>()));

            Assert.False(result.IsSuccess);
            Assert.Equal("BROWSER_CAPTURE_RESOLUTION_INVALID", result.ErrorCode);
            Assert.Empty(runner.Requests);
            Assert.False(Directory.Exists(artifactRoot));
        }
        finally
        {
            if (Directory.Exists(artifactRoot))
                Directory.Delete(artifactRoot, recursive: true);
        }
    }

    [Fact]
    public async Task RenderAsync_InsertsHeadAfterOpeningHtmlWhenDocumentHasNoHeadOrBody()
    {
        var adapter = new RecordingBrowserAdapter();
        var provider = new BrowserBackedDesignConversionRenderProvider(adapter);
        const string openingHtml = "<html lang=\"en\" data-template=\"n6-5\">";
        const string callerMain = "<main id=\"n6-edge-content\"><h1>Browser tolerated document</h1></main>";
        const string cssMarker = ".n6-edge-css-marker";
        const string cspMetaPrefix = "<meta http-equiv=\"Content-Security-Policy\" content=\"";
        var request = CreateRenderRequest(
            "render-html-without-head-or-body",
            new RenderQaResolution("edge", 640, 480, 1.0)) with
        {
            Html = $"<!doctype html>{openingHtml}{callerMain}",
            Css = $"{cssMarker}{{display:block;color:#102030;}}"
        };

        var result = await provider.RenderAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var capturedDocument = Assert.Single(adapter.Requests).DocumentHtml;
        Assert.Equal(1, CountOccurrences(capturedDocument, cssMarker, StringComparison.Ordinal));
        Assert.Equal(1, CountOccurrences(capturedDocument, cspMetaPrefix, StringComparison.Ordinal));
        Assert.Equal(1, CountOccurrences(capturedDocument, "default-src 'none'", StringComparison.Ordinal));
        Assert.Contains("<!doctype html>", capturedDocument, StringComparison.Ordinal);
        Assert.Contains(openingHtml, capturedDocument, StringComparison.Ordinal);
        Assert.Contains(callerMain, capturedDocument, StringComparison.Ordinal);

        var htmlIndex = capturedDocument.IndexOf(openingHtml, StringComparison.Ordinal);
        var headIndex = capturedDocument.IndexOf("<head>", StringComparison.Ordinal);
        var cspIndex = capturedDocument.IndexOf(cspMetaPrefix, StringComparison.Ordinal);
        var styleIndex = capturedDocument.IndexOf($"<style>{cssMarker}", StringComparison.Ordinal);
        var callerMainIndex = capturedDocument.IndexOf(callerMain, StringComparison.Ordinal);
        Assert.True(htmlIndex >= 0, "Captured document must preserve the caller html tag and attributes.");
        Assert.Equal(htmlIndex + openingHtml.Length, headIndex);
        Assert.True(cspIndex > headIndex, "Generated CSP must be inside the injected head.");
        Assert.True(styleIndex > cspIndex, "Caller CSS must follow the generated CSP.");
        Assert.True(callerMainIndex > styleIndex, "Injected head must precede caller content.");
    }

    [Fact]
    public async Task RenderAsync_ReturnsExplicitEnvironmentBlockerWhenBrowserAdapterUnavailable()
    {
        var provider = new BrowserBackedDesignConversionRenderProvider(new FailingBrowserAdapter(
            "No Chromium-family browser executable is available.",
            "BROWSER_RUNTIME_UNAVAILABLE"));

        var result = await provider.RenderAsync(CreateRenderRequest(
            "render-unavailable",
            new RenderQaResolution("desktop", 1440, 900, 1.0)));

        Assert.False(result.IsSuccess);
        Assert.Equal("BROWSER_RUNTIME_UNAVAILABLE", result.ErrorCode);
        Assert.Contains("browser render unavailable", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("deterministic-render-evidence", result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain("metadata_only_render_evidence", result.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RenderAsync_RetriesSingleScreenshotTimeoutThenSucceedsWithServerOwnedRetryEvidence()
    {
        const string privateMarker = "PRIVATE_BROWSER_TIMEOUT_RETRY_SECRET";
        const string privatePath = @"C:\private\browser\profile\screenshot-timeout.png";
        var adapter = new ScriptedBrowserAdapter(
            ScriptedBrowserAdapter.Timeout($"{privateMarker} at {privatePath}"),
            ScriptedBrowserAdapter.ValidLocalPng());
        var provider = new BrowserBackedDesignConversionRenderProvider(adapter);

        var result = await provider.RenderAsync(CreateRenderRequest(
            "render-timeout-then-success",
            new RenderQaResolution("timeout-retry", 32, 24, 1.0)));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, adapter.Requests.Count);
        Assert.Same(adapter.Requests[0], adapter.Requests[1]);
        Assert.Equal("render-timeout-then-success", adapter.Requests[0].CaptureRequestId);
        Assert.Equal(adapter.Requests[0].CaptureRequestId, adapter.Requests[1].CaptureRequestId);
        Assert.Equal("2", result.Value!.Image.Metadata["browser_capture_attempt_count"]);
        Assert.Equal("2", result.Value.Image.Metadata["browser_capture_max_attempts"]);
        Assert.Equal(bool.TrueString, result.Value.Image.Metadata["browser_capture_retry_applied"]);
        Assert.Equal(
            "server_owned_browser_screenshot_timeout_retry",
            result.Value.Image.Metadata["browser_capture_retry_source"]);
        Assert.Equal(
            "BROWSER_SCREENSHOT_TIMEOUT",
            result.Value.Image.Metadata["browser_capture_retry_error_code"]);
        Assert.Contains("browser_capture_retry_applied:true", result.Value.Warnings);
        Assert.Contains("browser_capture_attempt_count=2", result.Value.Warnings);
        Assert.Contains(
            "browser_capture_retry_source=server_owned_browser_screenshot_timeout_retry",
            result.Value.Warnings);
        AssertNoBrowserRetryPrivateLeak(privateMarker, privatePath, result);
    }

    [Fact]
    public async Task RenderAsync_RetriesThrownNonCallerOperationCanceledThenSucceedsWithServerOwnedRetryEvidence()
    {
        var adapter = new ScriptedBrowserAdapter(
            ScriptedBrowserAdapter.ThrowsNonCallerOperationCanceled(),
            ScriptedBrowserAdapter.ValidLocalPng());
        var provider = new BrowserBackedDesignConversionRenderProvider(adapter);

        var result = await provider.RenderAsync(CreateRenderRequest(
            "render-thrown-oce-then-success",
            new RenderQaResolution("thrown-oce-retry", 32, 24, 1.0)));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, adapter.Requests.Count);
        Assert.Same(adapter.Requests[0], adapter.Requests[1]);
        Assert.Equal("render-thrown-oce-then-success", adapter.Requests[0].CaptureRequestId);
        Assert.Equal(adapter.Requests[0].CaptureRequestId, adapter.Requests[1].CaptureRequestId);
        Assert.Equal("2", result.Value!.Image.Metadata["browser_capture_attempt_count"]);
        Assert.Equal("2", result.Value.Image.Metadata["browser_capture_max_attempts"]);
        Assert.Equal(bool.TrueString, result.Value.Image.Metadata["browser_capture_retry_applied"]);
        Assert.Equal(
            "server_owned_browser_screenshot_timeout_retry",
            result.Value.Image.Metadata["browser_capture_retry_source"]);
        Assert.Equal(
            "BROWSER_SCREENSHOT_TIMEOUT",
            result.Value.Image.Metadata["browser_capture_retry_error_code"]);
        Assert.Contains("browser_capture_retry_applied:true", result.Value.Warnings);
        Assert.Contains("browser_capture_attempt_count=2", result.Value.Warnings);
        Assert.Contains(
            "browser_capture_retry_source=server_owned_browser_screenshot_timeout_retry",
            result.Value.Warnings);
        AssertNoBrowserRetryExceptionLeak(result);
    }

    [Fact]
    public async Task RenderAsync_StopsAfterTwoScreenshotTimeoutsWithoutRawAdapterLeakOrSuccessArtifact()
    {
        const string firstPrivateMarker = "PRIVATE_BROWSER_TIMEOUT_FIRST";
        const string secondPrivateMarker = "PRIVATE_BROWSER_TIMEOUT_SECOND";
        const string privatePath = @"C:\private\browser\profile\second-timeout.png";
        var adapter = new ScriptedBrowserAdapter(
            ScriptedBrowserAdapter.Timeout($"{firstPrivateMarker} at C:\\private\\browser\\profile\\first-timeout.png"),
            ScriptedBrowserAdapter.Timeout($"{secondPrivateMarker} at {privatePath}"));
        var provider = new BrowserBackedDesignConversionRenderProvider(adapter);

        var result = await provider.RenderAsync(CreateRenderRequest(
            "render-timeout-exhausted",
            new RenderQaResolution("timeout-exhausted", 32, 24, 1.0)));

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal("BROWSER_SCREENSHOT_TIMEOUT", result.ErrorCode);
        Assert.Equal(2, adapter.Requests.Count);
        Assert.Same(adapter.Requests[0], adapter.Requests[1]);
        Assert.Equal(adapter.Requests[0].CaptureRequestId, adapter.Requests[1].CaptureRequestId);
        Assert.Contains("timeout retry", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("product_ready", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        AssertNoBrowserRetryPrivateLeak(firstPrivateMarker, privatePath, result);
        AssertNoBrowserRetryPrivateLeak(secondPrivateMarker, privatePath, result);
    }

    [Fact]
    public async Task RenderAsync_DoesNotRetryPermanentBrowserCaptureFailure()
    {
        const string privateMarker = "PRIVATE_BROWSER_PERMANENT_FAILURE";
        const string privatePath = @"C:\private\browser\profile\permanent-failure.png";
        var adapter = new ScriptedBrowserAdapter(
            ScriptedBrowserAdapter.Failure(
                $"Permanent browser failure {privateMarker} at {privatePath}.",
                LocalChromiumBrowserProcessAdapter.ScreenshotFailedCode),
            ScriptedBrowserAdapter.ValidLocalPng());
        var provider = new BrowserBackedDesignConversionRenderProvider(adapter);

        var result = await provider.RenderAsync(CreateRenderRequest(
            "render-permanent-browser-failure",
            new RenderQaResolution("permanent-failure", 32, 24, 1.0)));

        Assert.False(result.IsSuccess);
        Assert.Equal(LocalChromiumBrowserProcessAdapter.ScreenshotFailedCode, result.ErrorCode);
        Assert.Single(adapter.Requests);
        AssertNoBrowserRetryPrivateLeak(privateMarker, privatePath, result);
    }

    [Fact]
    public async Task RenderAsync_CancellationAfterFirstTimeoutDoesNotIssueRetry()
    {
        const string privateMarker = "PRIVATE_BROWSER_CANCELLED_TIMEOUT";
        using var cancellation = new CancellationTokenSource();
        var adapter = new ScriptedBrowserAdapter(
            (request, _) =>
            {
                cancellation.Cancel();
                return Task.FromResult(OperationResult<DesignConversionBrowserCaptureResult>.Fail(
                    $"{privateMarker} should not leak after cancellation.",
                    "BROWSER_SCREENSHOT_TIMEOUT"));
            },
            ScriptedBrowserAdapter.ValidLocalPng());
        var provider = new BrowserBackedDesignConversionRenderProvider(adapter);

        var result = await provider.RenderAsync(
            CreateRenderRequest(
                "render-cancelled-after-timeout",
                new RenderQaResolution("cancelled-timeout", 32, 24, 1.0)),
            cancellation.Token);

        Assert.False(result.IsSuccess);
        Assert.Equal("CANCELLED", result.ErrorCode);
        Assert.Single(adapter.Requests);
        AssertNoBrowserRetryPrivateLeak(privateMarker, result);
    }

    [Fact]
    public async Task RenderAsync_RejectsMetadataOnlyAdapterScreenshotAsVisualProof()
    {
        var provider = new BrowserBackedDesignConversionRenderProvider(new MetadataOnlyBrowserAdapter());

        var result = await provider.RenderAsync(CreateRenderRequest(
            "render-metadata-only",
            new RenderQaResolution("desktop", 800, 600, 1.0)));

        Assert.False(result.IsSuccess);
        Assert.Equal("BROWSER_SCREENSHOT_PROOF_UNAVAILABLE", result.ErrorCode);
        Assert.Contains("inspectable local PNG screenshot proof", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("must reference a local file path", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RenderAsync_RejectsRemoteAdapterScreenshotWithoutCreatingRemoteTempSnapshot()
    {
        var baseline = RemoteEvidenceTempFileCount();
        var provider = new BrowserBackedDesignConversionRenderProvider(
            new RemoteScreenshotBrowserAdapter("https://public.test/browser-screenshot.png"));

        var result = await provider.RenderAsync(CreateRenderRequest(
            "render-remote-adapter-screenshot",
            new RenderQaResolution("remote-adapter", 64, 32, 1.0)));

        Assert.False(result.IsSuccess);
        Assert.Equal("BROWSER_SCREENSHOT_REMOTE_URI_UNSUPPORTED", result.ErrorCode);
        Assert.DoesNotContain("public.test", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(baseline, RemoteEvidenceTempFileCount());
    }

    [Fact]
    public async Task BrowserRenderManifest_RejectsMetadataOnlyArtifactScreenshotProofWithoutEvidenceRows()
    {
        var provider = new DesignConversionBrowserRenderManifestProvider(
            new BrowserBackedDesignConversionRenderProvider(new MetadataOnlyBrowserAdapter()));

        var manifest = await provider.RenderManifestAsync(new DesignConversionBrowserRenderManifestRequest(
            RenderRunId: "manifest-metadata-only-artifact",
            RequestId: "request-metadata-only-artifact",
            Html: "<main>metadata-only artifact screenshot proof must not become manifest evidence</main>",
            Css: "main{min-height:100vh;background:#fff;color:#111}",
            RequiredResolutions:
            [
                new RenderQaResolution("fake-first", 800, 600, 1.0),
                new RenderQaResolution("unreached-second", 390, 844, 2.0)
            ],
            Metadata: new Dictionary<string, string>
            {
                ["qa_data_role"] = "metadata_only_artifact_manifest_regression_evidence_only"
            }));

        Assert.False(manifest.IsSuccess);
        Assert.Equal("BROWSER_SCREENSHOT_PROOF_UNAVAILABLE", manifest.ErrorCode);
        Assert.Contains("inspectable local PNG screenshot proof", manifest.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("must reference a local file path", manifest.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(DesignConversionBrowserRenderManifestProvider.BrowserRenderQaMarker, manifest.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(DesignConversionBrowserRenderManifestProvider.MultiResolutionRenderMarker, manifest.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(DesignConversionBrowserRenderManifestProvider.PngArtifactsMarker, manifest.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(DesignConversionBrowserRenderManifestProvider.NonBlankPixelsMarker, manifest.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("product_ready", manifest.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.True(manifest.Value is null, "Failed manifest render must not return accepted entries or evidence refs.");
    }

    [Fact]
    public async Task RenderAsync_RejectsMalformedLocalPngAdapterScreenshotAsVisualProof()
    {
        var adapter = new MalformedLocalPngBrowserAdapter();
        var provider = new BrowserBackedDesignConversionRenderProvider(adapter);

        try
        {
            var result = await provider.RenderAsync(CreateRenderRequest(
                "render-malformed-local-png",
                new RenderQaResolution("desktop", 1, 1, 1.0)));

            Assert.False(result.IsSuccess);
            Assert.Equal("BROWSER_SCREENSHOT_PROOF_UNAVAILABLE", result.ErrorCode);
            Assert.Contains("inspectable local PNG screenshot proof", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("PNG pixel data is larger than expected", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("product_ready", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.NotNull(adapter.ScreenshotPath);
            Assert.True(File.Exists(adapter.ScreenshotPath), adapter.ScreenshotPath);
        }
        finally
        {
            if (adapter.ScreenshotPath is { } path && File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public async Task RenderAsync_RejectsSinglePixelArtifactPngAsBlankProofWithoutPayloadLeak()
    {
        var adapter = new LocalPngBrowserAdapter(
            "single_pixel_artifact_local_png_browser_process",
            static (width, height) => SingleNonWhitePixelRgbaPng(width, height));
        var provider = new BrowserBackedDesignConversionRenderProvider(adapter);

        try
        {
            var result = await provider.RenderAsync(CreateRenderRequest(
                "render-single-pixel-artifact-local-png",
                new RenderQaResolution("single-pixel-artifact", 16, 16, 1.0)));

            Assert.False(result.IsSuccess);
            Assert.Equal("BROWSER_SCREENSHOT_BLANK_PNG", result.ErrorCode);
            Assert.Contains("blank", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("file://", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            if (adapter.ScreenshotPath is { } path)
                Assert.DoesNotContain(path, result.ErrorMessage, StringComparison.Ordinal);
        }
        finally
        {
            if (adapter.ScreenshotPath is { } path && File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public async Task RenderAsync_AcceptsFlatNonWhitePngScreenshotAsVisualProof()
    {
        var adapter = new LocalPngBrowserAdapter(
            "flat_nonwhite_local_png_browser_process",
            static (width, height) => SolidRgbaPng(width, height, 18, 52, 86, 255));
        var provider = new BrowserBackedDesignConversionRenderProvider(adapter);

        try
        {
            var result = await provider.RenderAsync(CreateRenderRequest(
                "render-flat-nonwhite-local-png",
                new RenderQaResolution("flat-nonwhite", 16, 16, 1.0)));

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(bool.TrueString, result.Value!.Image.Metadata["browser_screenshot_nonblank_pixels_verified"]);
            Assert.Equal("256", result.Value.Image.Metadata["browser_screenshot_non_white_pixel_count"]);
            Assert.Equal("1", result.Value.Image.Metadata["browser_screenshot_distinct_color_count"]);

            var png = DesignConversionBrowserPngEvidenceInspector.Inspect(result.Value.Image.Uri);
            Assert.True(png.IsSuccess, png.ErrorMessage);
            Assert.True(png.Value!.HasNonBlankPixels);
            Assert.Equal(256, png.Value.NonWhitePixelCount);
            Assert.Equal(1, png.Value.DistinctColorCount);
        }
        finally
        {
            if (adapter.ScreenshotPath is { } path && File.Exists(path))
                File.Delete(path);
        }
    }

    [Theory]
    [InlineData("", "BROWSER_SCREENSHOT_SHA_MISSING")]
    [InlineData("sha256:ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff", "BROWSER_SCREENSHOT_SHA_MISMATCH")]
    public async Task RenderAsync_PreservesDeclaredBrowserScreenshotShaValidationFailures(
        string declaredSha,
        string expectedErrorCode)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            "browser-screenshot-declared-sha-validation",
            Guid.NewGuid().ToString("N"));
        var provider = new BrowserBackedDesignConversionRenderProvider(
            new DeclaredShaBrowserAdapter(root, declaredSha));

        try
        {
            var result = await provider.RenderAsync(CreateRenderRequest(
                $"render-declared-sha-{expectedErrorCode.ToLowerInvariant()}",
                new RenderQaResolution("declared-sha", 3, 2, 1.0)));

            Assert.False(result.IsSuccess);
            Assert.Equal(expectedErrorCode, result.ErrorCode);
            Assert.DoesNotContain(root, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("file:", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("<main", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(".card", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task RenderAsync_AcceptsDeclaredBrowserScreenshotShaFromInspectedSnapshot()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            "browser-screenshot-declared-sha-success",
            Guid.NewGuid().ToString("N"));
        var declaredSha = Convert.ToHexString(SHA256.HashData(TwoColorRgbaPng(3, 2))).ToLowerInvariant();
        var provider = new BrowserBackedDesignConversionRenderProvider(
            new DeclaredShaBrowserAdapter(root, declaredSha));

        try
        {
            var result = await provider.RenderAsync(CreateRenderRequest(
                "render-declared-sha-success",
                new RenderQaResolution("declared-sha-success", 3, 2, 1.0)));

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal("sha256:" + declaredSha, result.Value!.Image.Sha256);
            Assert.Equal(bool.TrueString, result.Value.Image.Metadata["browser_screenshot_sha256_verified"]);
            Assert.Equal(bool.TrueString, result.Value.Image.Metadata["browser_screenshot_nonblank_pixels_verified"]);
            Assert.Equal("6", result.Value.Image.Metadata["browser_screenshot_non_white_pixel_count"]);
            Assert.Equal("2", result.Value.Image.Metadata["browser_screenshot_distinct_color_count"]);

            var png = DesignConversionBrowserPngEvidenceInspector.Inspect(result.Value.Image.Uri);
            Assert.True(png.IsSuccess, png.ErrorMessage);
            Assert.True(png.Value!.HasNonBlankPixels);
            Assert.Equal(6, png.Value.NonWhitePixelCount);
            Assert.Equal(2, png.Value.DistinctColorCount);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task BrowserRenderManifest_RejectsMalformedLocalPngScreenshotProofWithoutEvidenceRows()
    {
        var adapter = new MalformedLocalPngBrowserAdapter();
        var provider = new DesignConversionBrowserRenderManifestProvider(
            new BrowserBackedDesignConversionRenderProvider(adapter));

        try
        {
            var manifest = await provider.RenderManifestAsync(new DesignConversionBrowserRenderManifestRequest(
                RenderRunId: "manifest-malformed-local-png",
                RequestId: "request-malformed-local-png",
                Html: "<main>malformed screenshot proof must not become manifest evidence</main>",
                Css: "main{min-height:100vh;background:#fff;color:#111}",
                RequiredResolutions:
                [
                    new RenderQaResolution("broken-first", 1, 1, 1.0),
                    new RenderQaResolution("unreached-second", 2, 2, 1.0)
                ],
                Metadata: new Dictionary<string, string>
                {
                    ["qa_data_role"] = "malformed_png_manifest_propagation_evidence_only"
                }));

            Assert.False(manifest.IsSuccess);
            Assert.Equal("BROWSER_SCREENSHOT_PROOF_UNAVAILABLE", manifest.ErrorCode);
            Assert.Contains("inspectable local PNG screenshot proof", manifest.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("PNG pixel data is larger than expected", manifest.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(DesignConversionBrowserRenderManifestProvider.BrowserRenderQaMarker, manifest.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(DesignConversionBrowserRenderManifestProvider.MultiResolutionRenderMarker, manifest.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(DesignConversionBrowserRenderManifestProvider.PngArtifactsMarker, manifest.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(DesignConversionBrowserRenderManifestProvider.NonBlankPixelsMarker, manifest.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("product_ready", manifest.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.True(manifest.Value is null, "Failed manifest render must not return accepted entries or evidence refs.");
            Assert.NotNull(adapter.ScreenshotPath);
            Assert.True(File.Exists(adapter.ScreenshotPath), adapter.ScreenshotPath);
        }
        finally
        {
            if (adapter.ScreenshotPath is { } path && File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void PngEvidenceInspector_RejectsRemoteAndNetworkScreenshotUrisBeforeFileInspection()
    {
        var remote = DesignConversionBrowserPngEvidenceInspector.Inspect(
            "http://127.0.0.1:65535/private-screenshot.png");
        var artifact = DesignConversionBrowserPngEvidenceInspector.Inspect(
            "artifact://browser-screenshot/fake-proof.png");
        var networkFile = DesignConversionBrowserPngEvidenceInspector.Inspect(
            "file://127.0.0.1/C$/private-screenshot.png");
        var uncPath = DesignConversionBrowserPngEvidenceInspector.Inspect(
            @"\\127.0.0.1\C$\private-screenshot.png");

        Assert.False(remote.IsSuccess);
        Assert.Equal("PNG_SCREENSHOT_URI_NOT_LOCAL_FILE", remote.ErrorCode);
        Assert.Contains("remote screenshot URIs are not accepted", remote.ErrorMessage, StringComparison.OrdinalIgnoreCase);

        Assert.False(artifact.IsSuccess);
        Assert.Equal("PNG_SCREENSHOT_URI_NOT_LOCAL_FILE", artifact.ErrorCode);

        Assert.False(networkFile.IsSuccess);
        Assert.Equal("PNG_SCREENSHOT_URI_NETWORK_PATH", networkFile.ErrorCode);

        Assert.False(uncPath.IsSuccess);
        Assert.Equal("PNG_SCREENSHOT_URI_NETWORK_PATH", uncPath.ErrorCode);
    }

    [Theory]
    [InlineData("http://0.0.0.0/proof.png")]
    [InlineData("http://127.0.0.1/proof.png")]
    [InlineData("http://10.0.0.1/proof.png")]
    [InlineData("http://100.64.0.1/proof.png")]
    [InlineData("http://172.16.0.1/proof.png")]
    [InlineData("http://192.0.2.1/proof.png")]
    [InlineData("http://192.168.1.5/proof.png")]
    [InlineData("http://169.254.169.254/latest/meta-data")]
    [InlineData("http://198.18.0.1/proof.png")]
    [InlineData("http://198.51.100.1/proof.png")]
    [InlineData("http://203.0.113.1/proof.png")]
    [InlineData("http://224.0.0.1/proof.png")]
    [InlineData("http://240.0.0.1/proof.png")]
    [InlineData("http://[::1]/proof.png")]
    [InlineData("http://[::]/proof.png")]
    [InlineData("http://[fc00::1]/proof.png")]
    [InlineData("http://[fe80::1]/proof.png")]
    [InlineData("http://[::ffff:192.168.1.1]/proof.png")]
    [InlineData("http://[100::1]/proof.png")]
    [InlineData("http://[2001::1]/proof.png")]
    [InlineData("http://[2001:2::1]/proof.png")]
    [InlineData("http://[2001:db8::1]/proof.png")]
    [InlineData("http://[2002:0808:0808::1]/proof.png")]
    [InlineData("http://[64:ff9b::a9fe:a9fe]/proof.png")]
    public async Task PngEvidenceInspector_RemoteFetchRejectsBlockedLiteralDestinations(string uri)
    {
        var transport = new FakeRemotePngHttpTransport();

        var result = await DesignConversionBrowserPngEvidenceInspector.InspectAsync(
            uri,
            new FakeRemotePngDnsResolver(),
            transport,
            RemotePolicy());

        Assert.False(result.IsSuccess);
        Assert.Equal("PNG_REMOTE_HOST_BLOCKED", result.ErrorCode);
        Assert.Empty(transport.Requests);
    }

    [Theory]
    [InlineData("6to4.test", "2002:0808:0808::1")]
    [InlineData("teredo.test", "2001::1")]
    [InlineData("nat64-metadata.test", "64:ff9b::a9fe:a9fe")]
    [InlineData("benchmark.test", "2001:2::1")]
    public async Task PngEvidenceInspector_RemoteFetchRejectsBlockedResolvedIpv6Destinations(
        string host,
        string address)
    {
        var resolver = new FakeRemotePngDnsResolver(new Dictionary<string, IPAddress[]>
        {
            [host] = [IPAddress.Parse(address)]
        });
        var transport = new FakeRemotePngHttpTransport();

        var result = await DesignConversionBrowserPngEvidenceInspector.InspectAsync(
            $"https://{host}/proof.png",
            resolver,
            transport,
            RemotePolicy());

        Assert.False(result.IsSuccess);
        Assert.Equal("PNG_REMOTE_HOST_BLOCKED", result.ErrorCode);
        Assert.Collection(resolver.Queries, query => Assert.Equal(host, query));
        Assert.Empty(transport.Requests);
    }

    [Fact]
    public async Task PngEvidenceInspector_RemoteFetchRejectsCredentialsAndDisallowedHttpScheme()
    {
        var resolver = FakeRemotePngDnsResolver.PublicOnly("public.test");
        var transport = new FakeRemotePngHttpTransport();

        var credentials = await DesignConversionBrowserPngEvidenceInspector.InspectAsync(
            "https://user:secret@public.test/proof.png",
            resolver,
            transport,
            RemotePolicy());
        var httpDisallowed = await DesignConversionBrowserPngEvidenceInspector.InspectAsync(
            "http://public.test/proof.png",
            resolver,
            transport,
            RemotePolicy() with { AllowHttp = false });

        Assert.False(credentials.IsSuccess);
        Assert.Equal("PNG_REMOTE_URI_USERINFO_FORBIDDEN", credentials.ErrorCode);
        Assert.False(httpDisallowed.IsSuccess);
        Assert.Equal("PNG_REMOTE_URI_UNSUPPORTED_SCHEME", httpDisallowed.ErrorCode);
        Assert.Empty(transport.Requests);
    }

    [Fact]
    public async Task PngEvidenceInspector_RemoteFetchRejectsMixedDnsWhenAnyAddressIsPrivate()
    {
        var resolver = new FakeRemotePngDnsResolver(new Dictionary<string, IPAddress[]>
        {
            ["mixed.test"] =
            [
                FakeRemotePngDnsResolver.PublicAddress,
                IPAddress.Parse("10.0.0.25")
            ]
        });
        var transport = new FakeRemotePngHttpTransport();

        var result = await DesignConversionBrowserPngEvidenceInspector.InspectAsync(
            "https://mixed.test/proof.png",
            resolver,
            transport,
            RemotePolicy());

        Assert.False(result.IsSuccess);
        Assert.Equal("PNG_REMOTE_HOST_BLOCKED", result.ErrorCode);
        Assert.Empty(transport.Requests);
    }

    [Fact]
    public async Task PngEvidenceInspector_RemoteFetchRevalidatesRedirectTargetsAndRejectsPrivateMetadataRedirect()
    {
        var resolver = FakeRemotePngDnsResolver.PublicOnly("public.test");
        var transport = new FakeRemotePngHttpTransport(
            _ => FakeRemotePngHttpResponse(
                302,
                redirectLocation: "https://169.254.169.254/latest/meta-data"));

        var result = await DesignConversionBrowserPngEvidenceInspector.InspectAsync(
            "https://public.test/proof.png",
            resolver,
            transport,
            RemotePolicy());

        Assert.False(result.IsSuccess);
        Assert.Equal("PNG_REMOTE_HOST_BLOCKED", result.ErrorCode);
        Assert.Single(transport.Requests);
    }

    [Fact]
    public async Task PngEvidenceInspector_RemoteFetchRejectsRedirectLoopsAndRedirectLimits()
    {
        var resolver = FakeRemotePngDnsResolver.PublicOnly("public.test", "next.test");
        var loopTransport = new FakeRemotePngHttpTransport(
            _ => FakeRemotePngHttpResponse(302, redirectLocation: "https://public.test/proof.png"));
        var limitTransport = new FakeRemotePngHttpTransport(
            _ => FakeRemotePngHttpResponse(302, redirectLocation: "https://next.test/proof.png"),
            _ => FakeRemotePngHttpResponse(302, redirectLocation: "https://public.test/final.png"));

        var loop = await DesignConversionBrowserPngEvidenceInspector.InspectAsync(
            "https://public.test/proof.png",
            resolver,
            loopTransport,
            RemotePolicy(maxRedirects: 3));
        var limit = await DesignConversionBrowserPngEvidenceInspector.InspectAsync(
            "https://public.test/proof.png",
            resolver,
            limitTransport,
            RemotePolicy(maxRedirects: 1));

        Assert.False(loop.IsSuccess);
        Assert.Equal("PNG_REMOTE_REDIRECT_LOOP", loop.ErrorCode);
        Assert.False(limit.IsSuccess);
        Assert.Equal("PNG_REMOTE_REDIRECT_LIMIT", limit.ErrorCode);
    }

    [Fact]
    public async Task PngEvidenceInspector_RemoteFetchRejectsHttpsDowngradeRedirect()
    {
        var resolver = FakeRemotePngDnsResolver.PublicOnly("public.test");
        var transport = new FakeRemotePngHttpTransport(
            _ => FakeRemotePngHttpResponse(302, redirectLocation: "http://public.test/proof.png"));

        var result = await DesignConversionBrowserPngEvidenceInspector.InspectAsync(
            "https://public.test/proof.png",
            resolver,
            transport,
            RemotePolicy());

        Assert.False(result.IsSuccess);
        Assert.Equal("PNG_REMOTE_HTTPS_DOWNGRADE", result.ErrorCode);
    }

    [Fact]
    public async Task PngEvidenceInspector_RemoteFetchRejectsHttpsDowngradeBeforeResolvingRedirectHost()
    {
        var resolver = FakeRemotePngDnsResolver.PublicOnly("public.test");
        var transport = new FakeRemotePngHttpTransport(
            _ => FakeRemotePngHttpResponse(302, redirectLocation: "http://downgrade.test/proof.png"));

        var result = await DesignConversionBrowserPngEvidenceInspector.InspectAsync(
            "https://public.test/proof.png",
            resolver,
            transport,
            RemotePolicy());

        Assert.False(result.IsSuccess);
        Assert.Equal("PNG_REMOTE_HTTPS_DOWNGRADE", result.ErrorCode);
        Assert.Collection(resolver.Queries, query => Assert.Equal("public.test", query));
        Assert.Single(transport.Requests);
    }

    [Fact]
    public async Task PngEvidenceInspector_RemoteFetchCanonicalizesRedirectLoopsBeforeResolvingAgain()
    {
        var resolver = FakeRemotePngDnsResolver.PublicOnly("public.test");
        var transport = new FakeRemotePngHttpTransport(
            _ => FakeRemotePngHttpResponse(302, redirectLocation: "https://PUBLIC.TEST.:443/proof.png"));

        var result = await DesignConversionBrowserPngEvidenceInspector.InspectAsync(
            "https://public.test/proof.png",
            resolver,
            transport,
            RemotePolicy());

        Assert.False(result.IsSuccess);
        Assert.Equal("PNG_REMOTE_REDIRECT_LOOP", result.ErrorCode);
        Assert.Collection(resolver.Queries, query => Assert.Equal("public.test", query));
        Assert.Single(transport.Requests);
    }

    [Fact]
    public async Task PngEvidenceInspector_RemoteFetchRejectsOversizedHeaderAndChunkedStreamsBeforeDecode()
    {
        var resolver = FakeRemotePngDnsResolver.PublicOnly("public.test");
        var headerTransport = new FakeRemotePngHttpTransport(
            _ => FakeRemotePngHttpResponse(200, [], contentLength: 65));
        var chunkedTransport = new FakeRemotePngHttpTransport(
            _ => FakeRemotePngHttpResponse(
                200,
                Encoding.ASCII.GetBytes("123456789"),
                contentLength: null));

        var oversizedHeader = await DesignConversionBrowserPngEvidenceInspector.InspectAsync(
            "https://public.test/proof.png",
            resolver,
            headerTransport,
            RemotePolicy(maxBytes: 64));
        var oversizedChunked = await DesignConversionBrowserPngEvidenceInspector.InspectAsync(
            "https://public.test/proof.png",
            resolver,
            chunkedTransport,
            RemotePolicy(maxBytes: 8));

        Assert.False(oversizedHeader.IsSuccess);
        Assert.Equal("PNG_REMOTE_CONTENT_LENGTH_TOO_LARGE", oversizedHeader.ErrorCode);
        Assert.False(oversizedChunked.IsSuccess);
        Assert.Equal("PNG_REMOTE_BYTES_TOO_LARGE", oversizedChunked.ErrorCode);
    }

    [Fact]
    public async Task PngEvidenceInspector_RemoteFetchRejectsEncodedContentBeforeDecode()
    {
        var resolver = FakeRemotePngDnsResolver.PublicOnly("public.test");
        var encodedTransport = new FakeRemotePngHttpTransport(
            _ => FakeRemotePngHttpResponse(
                200,
                ValidPng(),
                contentEncodings: ["gzip"]));

        var encoded = await DesignConversionBrowserPngEvidenceInspector.InspectAsync(
            "https://public.test/proof.png",
            resolver,
            encodedTransport,
            RemotePolicy());

        Assert.False(encoded.IsSuccess);
        Assert.Equal("PNG_REMOTE_CONTENT_ENCODING_UNSUPPORTED", encoded.ErrorCode);
    }

    [Fact]
    public async Task PngEvidenceInspector_RemoteFetchRejectsTimeoutInvalidContentTypeNonPngAndBlankPng()
    {
        var resolver = FakeRemotePngDnsResolver.PublicOnly("public.test");
        var timeout = await DesignConversionBrowserPngEvidenceInspector.InspectAsync(
            "https://public.test/proof.png",
            resolver,
            new FakeRemotePngHttpTransport(throwTimeout: true),
            RemotePolicy());
        var invalidType = await DesignConversionBrowserPngEvidenceInspector.InspectAsync(
            "https://public.test/proof.png",
            resolver,
            new FakeRemotePngHttpTransport(_ => FakeRemotePngHttpResponse(200, ValidPng(), contentType: "text/plain")),
            RemotePolicy());
        var nonPng = await DesignConversionBrowserPngEvidenceInspector.InspectAsync(
            "https://public.test/proof.png",
            resolver,
            new FakeRemotePngHttpTransport(_ => FakeRemotePngHttpResponse(200, Encoding.ASCII.GetBytes("not-png"))),
            RemotePolicy());
        var blank = await DesignConversionBrowserPngEvidenceInspector.InspectAsync(
            "https://public.test/proof.png",
            resolver,
            new FakeRemotePngHttpTransport(_ => FakeRemotePngHttpResponse(200, BlankPng())),
            RemotePolicy());

        Assert.False(timeout.IsSuccess);
        Assert.Equal("PNG_REMOTE_FETCH_TIMEOUT", timeout.ErrorCode);
        Assert.False(invalidType.IsSuccess);
        Assert.Equal("PNG_REMOTE_CONTENT_TYPE_UNSUPPORTED", invalidType.ErrorCode);
        Assert.False(nonPng.IsSuccess);
        Assert.Equal("PNG_SIGNATURE_INVALID", nonPng.ErrorCode);
        Assert.False(blank.IsSuccess);
        Assert.Equal("PNG_REMOTE_BLANK", blank.ErrorCode);
    }

    [Fact]
    public async Task PngEvidenceInspector_PublicLocalEvidenceDisposeDoesNotDeleteCallerOwnedFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"caller-owned-local-png-{Guid.NewGuid():N}.png");
        try
        {
            await File.WriteAllBytesAsync(path, ValidPng());

            var result = await DesignConversionBrowserPngEvidenceInspector.InspectAsync(path);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            using var evidence = result.Value!;
            Assert.True(File.Exists(evidence.LocalPath), evidence.LocalPath);

            evidence.Dispose();

            Assert.True(File.Exists(path), path);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public async Task PngEvidenceInspector_LocalInspectAsyncReadsValidPng()
    {
        var path = Path.Combine(Path.GetTempPath(), $"async-local-png-{Guid.NewGuid():N}.png");
        var png = ValidPng();
        try
        {
            await File.WriteAllBytesAsync(path, png);

            var result = await DesignConversionBrowserPngEvidenceInspector.InspectAsync(path);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            using var evidence = result.Value!;
            Assert.Equal(path, evidence.LocalPath);
            Assert.Equal(png.Length, evidence.ByteLength);
            Assert.Equal(2, evidence.PngWidth);
            Assert.Equal(1, evidence.PngHeight);
            Assert.True(evidence.HasNonBlankPixels);
            Assert.True(evidence.NonWhitePixelCount > 0);
            Assert.Equal(Convert.ToHexString(SHA256.HashData(png)).ToLowerInvariant(), evidence.SnapshotSha256);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void PngEvidenceInspector_LocalInspectReadsValidPngWithExactEvidence()
    {
        var path = Path.Combine(Path.GetTempPath(), $"sync-local-png-{Guid.NewGuid():N}.png");
        var png = ValidPng();
        try
        {
            File.WriteAllBytes(path, png);

            var result = DesignConversionBrowserPngEvidenceInspector.Inspect(path);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            using var evidence = result.Value!;
            Assert.Equal(path, evidence.LocalPath);
            Assert.Equal(png.Length, evidence.ByteLength);
            Assert.Equal(2, evidence.PngWidth);
            Assert.Equal(1, evidence.PngHeight);
            Assert.True(evidence.HasNonBlankPixels);
            Assert.True(evidence.NonWhitePixelCount > 0);
            Assert.Equal(Convert.ToHexString(SHA256.HashData(png)).ToLowerInvariant(), evidence.SnapshotSha256);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public async Task PngEvidenceInspector_LocalInspectAsyncReturnsCancelledForAlreadyCancelledToken()
    {
        var path = Path.Combine(Path.GetTempPath(), $"async-local-cancelled-png-{Guid.NewGuid():N}.png");
        try
        {
            await File.WriteAllBytesAsync(path, ValidPng());
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            var result = await DesignConversionBrowserPngEvidenceInspector.InspectAsync(
                path,
                cancellationToken: cancellation.Token);

            Assert.False(result.IsSuccess);
            Assert.Equal("CANCELLED", result.ErrorCode);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public async Task PngEvidenceInspector_RemoteFetchOwnsSnapshotUntilDisposeAndPinsResolvedAddress()
    {
        var baseline = RemoteEvidenceTempFileCount();
        var resolver = FakeRemotePngDnsResolver.PublicOnly("public.test");
        var png = ValidPng();
        var transport = new FakeRemotePngHttpTransport(_ => FakeRemotePngHttpResponse(200, png));

        var result = await DesignConversionBrowserPngEvidenceInspector.InspectAsync(
            "https://public.test/proof.png",
            resolver,
            transport,
            RemotePolicy());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var localPath = result.Value!.LocalPath;
        {
            using var evidence = result.Value;
            using var clone = evidence with { };
            Assert.True(File.Exists(localPath), localPath);
            Assert.Equal(2, evidence.PngWidth);
            Assert.Equal(1, evidence.PngHeight);
            Assert.True(evidence.HasNonBlankPixels);
            Assert.Equal(png.Length, evidence.ByteLength);
            Assert.Equal(baseline + 1, RemoteEvidenceTempFileCount());
            var request = Assert.Single(transport.Requests);
            Assert.Equal(FakeRemotePngDnsResolver.PublicAddress, request.PinnedAddress);
            Assert.Equal("public.test", request.Uri.Host);

            evidence.Dispose();

            Assert.False(File.Exists(localPath), localPath);
            Assert.Equal(baseline, RemoteEvidenceTempFileCount());

            clone.Dispose();
            evidence.Dispose();
        }

        Assert.False(File.Exists(localPath), localPath);
        Assert.Equal(baseline, RemoteEvidenceTempFileCount());
    }

    [Fact]
    public async Task PngEvidenceInspector_RemoteFailureAndCancellationRestoreTempBaseline()
    {
        var baseline = RemoteEvidenceTempFileCount();
        var resolver = FakeRemotePngDnsResolver.PublicOnly("public.test");

        var invalidPng = await DesignConversionBrowserPngEvidenceInspector.InspectAsync(
            "https://public.test/proof.png",
            resolver,
            new FakeRemotePngHttpTransport(_ => FakeRemotePngHttpResponse(200, Encoding.ASCII.GetBytes("not-png"))),
            RemotePolicy());

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var cancelled = await DesignConversionBrowserPngEvidenceInspector.InspectAsync(
            "https://public.test/proof.png",
            resolver,
            new FakeRemotePngHttpTransport(_ => FakeRemotePngHttpResponse(200, ValidPng())),
            RemotePolicy(),
            cancellation.Token);

        Assert.False(invalidPng.IsSuccess);
        Assert.Equal("PNG_SIGNATURE_INVALID", invalidPng.ErrorCode);
        Assert.False(cancelled.IsSuccess);
        Assert.Equal("CANCELLED", cancelled.ErrorCode);
        Assert.Equal(baseline, RemoteEvidenceTempFileCount());
    }

    [Fact]
    public async Task PngEvidenceInspector_RemoteTempCleanupEnvelopeRestoresBaselineForSuccessFailuresAndCancellation()
    {
        var baseline = RemoteEvidenceTempFileCount();
        var resolver = FakeRemotePngDnsResolver.PublicOnly("public.test");
        var secretUrl = "https://public.test/private-proof.png?token=PRIVATE_N6_6B_URL";

        var successBody = new TrackingReadStream(ValidPng());
        var successOwner = new TrackingDisposable();
        var success = await DesignConversionBrowserPngEvidenceInspector.InspectAsync(
            secretUrl,
            resolver,
            new FakeRemotePngHttpTransport(_ => FakeRemotePngHttpResponse(200, successBody, owner: successOwner)),
            RemotePolicy());

        Assert.True(success.IsSuccess, success.ErrorMessage);
        var successPath = success.Value!.LocalPath;
        Assert.True(successBody.IsDisposed);
        Assert.True(successOwner.IsDisposed);
        Assert.Equal(baseline + 1, RemoteEvidenceTempFileCount());

        success.Value.Dispose();
        success.Value.Dispose();

        Assert.False(File.Exists(successPath), successPath);
        Assert.Equal(baseline, RemoteEvidenceTempFileCount());

        var invalidMarker = "PRIVATE_N6_6B_INVALID_BODY_" + Guid.NewGuid().ToString("N");
        var invalidUrl = "https://public.test/private-proof.png?token=" + invalidMarker;
        var invalidBody = new TrackingReadStream(Encoding.UTF8.GetBytes(invalidMarker));
        var invalidOwner = new TrackingDisposable();
        var invalid = await InspectRemoteFailureForCleanupEnvelopeAsync(
            invalidUrl,
            resolver,
            new FakeRemotePngHttpTransport(_ => FakeRemotePngHttpResponse(200, invalidBody, owner: invalidOwner)),
            "PNG_SIGNATURE_INVALID",
            baseline,
            privateValues: [invalidMarker, invalidUrl]);
        Assert.False(invalid.IsSuccess);
        Assert.True(invalidBody.IsDisposed);
        Assert.True(invalidOwner.IsDisposed);

        var headerMarker = "PRIVATE_N6_6B_HEADER_BODY";
        var headerUrl = "https://public.test/private-proof.png?token=" + headerMarker;
        var headerBody = new TrackingReadStream(Encoding.UTF8.GetBytes(headerMarker));
        var headerOwner = new TrackingDisposable();
        var oversizedHeader = await InspectRemoteFailureForCleanupEnvelopeAsync(
            headerUrl,
            resolver,
            new FakeRemotePngHttpTransport(_ => FakeRemotePngHttpResponse(200, headerBody, contentLength: 65, owner: headerOwner)),
            "PNG_REMOTE_CONTENT_LENGTH_TOO_LARGE",
            baseline,
            privateValues: [headerMarker, headerUrl],
            maxBytes: 64);
        Assert.False(oversizedHeader.IsSuccess);
        Assert.True(headerBody.IsDisposed);
        Assert.True(headerOwner.IsDisposed);

        var chunkedMarker = "PRIVATE_N6_6B_CHUNKED_BODY_" + Guid.NewGuid().ToString("N");
        var chunkedUrl = "https://public.test/private-proof.png?token=" + chunkedMarker;
        var chunkedBody = new TrackingReadStream(Encoding.UTF8.GetBytes(chunkedMarker));
        var chunkedOwner = new TrackingDisposable();
        var oversizedChunked = await InspectRemoteFailureForCleanupEnvelopeAsync(
            chunkedUrl,
            resolver,
            new FakeRemotePngHttpTransport(_ => FakeRemotePngHttpResponse(200, chunkedBody, contentLength: null, owner: chunkedOwner)),
            "PNG_REMOTE_BYTES_TOO_LARGE",
            baseline,
            privateValues: [chunkedMarker, chunkedUrl],
            maxBytes: 8);
        Assert.False(oversizedChunked.IsSuccess);
        Assert.True(chunkedBody.IsDisposed);
        Assert.True(chunkedOwner.IsDisposed);

        var redirectMarker = "PRIVATE_N6_6B_REDIRECT";
        var redirectUrl = "https://public.test/private-proof.png?token=" + redirectMarker;
        var redirectDowngradeUrl = "http://public.test/private-proof.png?token=" + redirectMarker;
        var redirectBody = new TrackingReadStream([]);
        var redirectOwner = new TrackingDisposable();
        var redirectFailure = await InspectRemoteFailureForCleanupEnvelopeAsync(
            redirectUrl,
            resolver,
            new FakeRemotePngHttpTransport(_ => FakeRemotePngHttpResponse(
                302,
                redirectBody,
                redirectLocation: redirectDowngradeUrl,
                owner: redirectOwner)),
            "PNG_REMOTE_HTTPS_DOWNGRADE",
            baseline,
            privateValues: [redirectMarker, redirectUrl, redirectDowngradeUrl]);
        Assert.False(redirectFailure.IsSuccess);
        Assert.True(redirectBody.IsDisposed);
        Assert.True(redirectOwner.IsDisposed);

        var networkMarker = "PRIVATE_N6_6B_NETWORK_" + Guid.NewGuid().ToString("N");
        var networkUrl = "https://public.test/private-proof.png?token=" + networkMarker;
        var networkFailure = await InspectRemoteFailureForCleanupEnvelopeAsync(
            networkUrl,
            resolver,
            new FakeRemotePngHttpTransport(_ => throw new System.Net.Http.HttpRequestException(networkMarker)),
            "PNG_REMOTE_FETCH_FAILED",
            baseline,
            privateValues: [networkMarker, networkUrl]);
        Assert.False(networkFailure.IsSuccess);

        var cancellationMarker = "PRIVATE_N6_6B_CANCELLED_" + Guid.NewGuid().ToString("N");
        var cancellationUrl = "https://public.test/private-proof.png?token=" + cancellationMarker;
        using var cancellation = new CancellationTokenSource();
        var cancellationBody = new CancelAfterEofReadStream(ValidPng(), cancellation);
        var cancellationOwner = new TrackingDisposable();
        var cancelled = await InspectRemoteFailureForCleanupEnvelopeAsync(
            cancellationUrl,
            resolver,
            new FakeRemotePngHttpTransport(_ => FakeRemotePngHttpResponse(200, cancellationBody, owner: cancellationOwner)),
            "CANCELLED",
            baseline,
            privateValues: [cancellationMarker, cancellationUrl],
            cancellationToken: cancellation.Token);
        Assert.False(cancelled.IsSuccess);
        Assert.True(cancellationBody.IsDisposed);
        Assert.True(cancellationOwner.IsDisposed);
    }

    [Fact]
    public void PngEvidenceInspector_RemoteTempCleanupFailureIsBoundedAndSanitized()
    {
        var baseline = RemoteEvidenceTempFileCount();
        var privateMarker = "PRIVATE_N6_6B_CLEANUP_" + Guid.NewGuid().ToString("N");
        var privatePath = Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            "design-conversion-remote-png-evidence",
            privateMarker + ".png");
        var privateUrl = "https://public.test/private-proof.png?token=" + privateMarker;

        var cleanup = DesignConversionBrowserPngEvidenceInspector.CleanupOwnedRemoteEvidenceTempFile(privatePath);

        Assert.False(cleanup.IsSuccess);
        Assert.Equal("PNG_REMOTE_TEMP_CLEANUP_FAILED", cleanup.ErrorCode);
        Assert.DoesNotContain(privateMarker, cleanup.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(privatePath, cleanup.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(privateUrl, cleanup.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(privateMarker, cleanup.ErrorCode, StringComparison.Ordinal);
        Assert.Equal(baseline, RemoteEvidenceTempFileCount());
    }

    [Fact]
    public void PngEvidenceInspector_RejectsOversizedScreenshotBytesBeforeDecode()
    {
        var path = Path.Combine(Path.GetTempPath(), $"oversized-png-bytes-{Guid.NewGuid():N}.png");
        try
        {
            using (var stream = File.Create(path))
            {
                var header = MinimalPngHeader(1, 1);
                stream.Write(header, 0, header.Length);
                stream.SetLength((64L * 1024 * 1024) + 1);
            }

            var result = DesignConversionBrowserPngEvidenceInspector.Inspect(path);

            Assert.False(result.IsSuccess);
            Assert.Equal("PNG_SCREENSHOT_BYTES_TOO_LARGE", result.ErrorCode);
            Assert.Contains("too large", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public async Task PngEvidenceInspector_LocalInspectAsyncRejectsOversizedScreenshotBytesBeforeDecode()
    {
        var path = Path.Combine(Path.GetTempPath(), $"async-oversized-png-bytes-{Guid.NewGuid():N}.png");
        try
        {
            using (var stream = File.Create(path))
            {
                var header = MinimalPngHeader(1, 1);
                stream.Write(header, 0, header.Length);
                stream.SetLength(DesignConversionBrowserPngEvidenceInspector.MaxPngScreenshotBytes + 1);
            }

            var result = await DesignConversionBrowserPngEvidenceInspector.InspectAsync(path);

            Assert.False(result.IsSuccess);
            Assert.Equal("PNG_SCREENSHOT_BYTES_TOO_LARGE", result.ErrorCode);
            Assert.Contains("too large", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void PngEvidenceInspector_RejectsOversizedScreenshotPixelsBeforeDecode()
    {
        var path = Path.Combine(Path.GetTempPath(), $"oversized-png-pixels-{Guid.NewGuid():N}.png");
        try
        {
            File.WriteAllBytes(path, MinimalPngHeader(20_000, 20_000));

            var result = DesignConversionBrowserPngEvidenceInspector.Inspect(path);

            Assert.False(result.IsSuccess);
            Assert.Equal("PNG_SCREENSHOT_PIXELS_TOO_LARGE", result.ErrorCode);
            Assert.Contains("dimensions are too large", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void PngEvidenceInspector_RejectsDecompressedPixelDataLargerThanDimensions()
    {
        var path = Path.Combine(Path.GetTempPath(), $"oversized-png-pixel-data-{Guid.NewGuid():N}.png");
        try
        {
            File.WriteAllBytes(path, PngWithExtraDecompressedPixelData());

            var result = DesignConversionBrowserPngEvidenceInspector.Inspect(path);

            Assert.False(result.IsSuccess);
            Assert.Equal("PNG_PIXEL_DATA_TOO_LARGE", result.ErrorCode);
            Assert.Contains("larger than expected", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void PngEvidenceInspector_DecodesValidRgbaPngFilters(int filter)
    {
        var path = Path.Combine(Path.GetTempPath(), $"filtered-png-{filter}-{Guid.NewGuid():N}.png");
        try
        {
            File.WriteAllBytes(path, FilteredRgbaPng((byte)filter));

            var result = DesignConversionBrowserPngEvidenceInspector.Inspect(path);

            Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
            Assert.Equal(3, result.Value!.PngWidth);
            Assert.Equal(2, result.Value.PngHeight);
            Assert.True(result.Value.HasNonBlankPixels);
            Assert.Equal(6, result.Value.NonWhitePixelCount);
            Assert.Equal(6, result.Value.DistinctColorCount);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Theory]
    [InlineData("rgba")]
    [InlineData("rgb")]
    [InlineData("grayscale")]
    [InlineData("grayscale-alpha")]
    public void PngEvidenceInspector_InspectMetricsAgreeWithComparisonRasterForSupportedColorTypes(string colorType)
    {
        var fixture = SupportedColorTypePng(colorType);
        var path = Path.Combine(Path.GetTempPath(), $"metrics-raster-{colorType}-{Guid.NewGuid():N}.png");
        try
        {
            File.WriteAllBytes(path, fixture.PngBytes);

            var inspected = DesignConversionBrowserPngEvidenceInspector.Inspect(path);
            var decoded = DesignConversionBrowserPngEvidenceInspector.DecodePngBytesForComparison(fixture.PngBytes, path);

            Assert.True(inspected.IsSuccess, $"{inspected.ErrorCode}: {inspected.ErrorMessage}");
            Assert.True(decoded.IsSuccess, $"{decoded.ErrorCode}: {decoded.ErrorMessage}");
            Assert.Equal(fixture.ExpectedRgbaPixels, decoded.Value!.RgbaPixels);

            var expectedMetrics = MeasureRgbaPixelsForTest(decoded.Value.RgbaPixels);
            Assert.Equal(decoded.Value.PngWidth, inspected.Value!.PngWidth);
            Assert.Equal(decoded.Value.PngHeight, inspected.Value.PngHeight);
            Assert.Equal(expectedMetrics.HasNonBlankPixels, inspected.Value.HasNonBlankPixels);
            Assert.Equal(expectedMetrics.NonWhitePixelCount, inspected.Value.NonWhitePixelCount);
            Assert.Equal(expectedMetrics.DistinctColorCount, inspected.Value.DistinctColorCount);
            Assert.Equal(expectedMetrics.HasNonBlankPixels, decoded.Value.HasNonBlankPixels);
            Assert.Equal(expectedMetrics.NonWhitePixelCount, decoded.Value.NonWhitePixelCount);
            Assert.Equal(expectedMetrics.DistinctColorCount, decoded.Value.DistinctColorCount);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void PngEvidenceInspector_ValidInvalidValidDecodeSequencePreservesFilterErrorAndExactRgba()
    {
        var first = SupportedColorTypePng("rgb");
        var invalid = Png(2, 1, colorType: 6, [5, 15, 118, 110, 255, 245, 158, 11, 255]);
        var second = SupportedColorTypePng("rgba");
        var firstPath = Path.Combine(Path.GetTempPath(), $"pooled-sequence-first-{Guid.NewGuid():N}.png");
        var invalidPath = Path.Combine(Path.GetTempPath(), $"pooled-sequence-invalid-{Guid.NewGuid():N}.png");
        var secondPath = Path.Combine(Path.GetTempPath(), $"pooled-sequence-second-{Guid.NewGuid():N}.png");
        try
        {
            File.WriteAllBytes(firstPath, first.PngBytes);
            File.WriteAllBytes(invalidPath, invalid);
            File.WriteAllBytes(secondPath, second.PngBytes);

            AssertPngFixtureMatchesInspectorAndComparison(first, firstPath);

            var invalidDecoded = DesignConversionBrowserPngEvidenceInspector.DecodePngBytesForComparison(invalid, invalidPath);
            Assert.False(invalidDecoded.IsSuccess);
            Assert.Equal("PNG_FILTER_INVALID", invalidDecoded.ErrorCode);
            Assert.Contains("filter is invalid", invalidDecoded.ErrorMessage, StringComparison.OrdinalIgnoreCase);

            AssertPngFixtureMatchesInspectorAndComparison(second, secondPath);
        }
        finally
        {
            if (File.Exists(firstPath))
                File.Delete(firstPath);
            if (File.Exists(invalidPath))
                File.Delete(invalidPath);
            if (File.Exists(secondPath))
                File.Delete(secondPath);
        }
    }

    [Fact]
    public async Task PngEvidenceInspector_DecodesSupportedColorTypesInParallelWithIndependentPooledOwnership()
    {
        string[] colorTypes = ["rgba", "rgb", "grayscale", "grayscale-alpha"];
        var tasks = colorTypes
            .SelectMany(colorType => Enumerable.Range(0, 8).Select(iteration => Task.Run(() =>
            {
                var fixture = SupportedColorTypePng(colorType);
                var path = Path.Combine(Path.GetTempPath(), $"pooled-parallel-{colorType}-{iteration}-{Guid.NewGuid():N}.png");
                try
                {
                    File.WriteAllBytes(path, fixture.PngBytes);
                    AssertPngFixtureMatchesInspectorAndComparison(fixture, path);
                }
                finally
                {
                    if (File.Exists(path))
                        File.Delete(path);
                }
            })))
            .ToArray();

        await Task.WhenAll(tasks);
    }

    [Fact]
    public void PngEvidenceInspector_ComparisonDecoderReturnsExactRgbaRasterForMultiRowMultiColorImage()
    {
        const int width = 3;
        const int height = 2;
        byte[] expectedRgbaPixels =
        [
            1, 2, 3, 255,
            250, 240, 230, 255,
            0, 0, 0, 0,
            128, 64, 32, 192,
            255, 255, 255, 255,
            9, 99, 199, 77
        ];
        var path = Path.Combine(Path.GetTempPath(), $"comparison-raster-exact-{Guid.NewGuid():N}.png");
        var png = RgbaPng(width, height, EncodeRgbaScanlines(width, height, expectedRgbaPixels, filter: 4));
        try
        {
            File.WriteAllBytes(path, png);

            var decoded = DesignConversionBrowserPngEvidenceInspector.DecodePngBytesForComparison(png, path);

            Assert.True(decoded.IsSuccess, $"{decoded.ErrorCode}: {decoded.ErrorMessage}");
            Assert.Equal(width, decoded.Value!.PngWidth);
            Assert.Equal(height, decoded.Value.PngHeight);
            Assert.Equal(checked(width * height * 4), decoded.Value.RgbaPixels.Length);
            Assert.Equal(expectedRgbaPixels, decoded.Value.RgbaPixels);
            Assert.Equal(Sha256Bytes(expectedRgbaPixels), Sha256Bytes(decoded.Value.RgbaPixels));
            Assert.Equal(Sha256Bytes(png), decoded.Value.Sha256);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void PngEvidenceInspector_DecodesAwkwardMultiIdatPngLikeSingleIdat()
    {
        var singlePath = Path.Combine(Path.GetTempPath(), $"single-idat-png-{Guid.NewGuid():N}.png");
        var multiPath = Path.Combine(Path.GetTempPath(), $"multi-idat-png-{Guid.NewGuid():N}.png");
        try
        {
            var scanlines = TwoColorRgbaScanlines(5, 4);
            var singleIdat = RgbaPng(5, 4, scanlines);
            var multiIdat = RgbaPngWithSplitIdat(5, 4, scanlines, insertAncillaryBetweenIdats: true, 1, 2, 5);
            File.WriteAllBytes(singlePath, singleIdat);
            File.WriteAllBytes(multiPath, multiIdat);

            var single = DesignConversionBrowserPngEvidenceInspector.Inspect(singlePath);
            var multi = DesignConversionBrowserPngEvidenceInspector.Inspect(multiPath);

            Assert.True(single.IsSuccess, $"{single.ErrorCode}: {single.ErrorMessage}");
            Assert.True(multi.IsSuccess, $"{multi.ErrorCode}: {multi.ErrorMessage}");
            Assert.Equal(single.Value!.PngWidth, multi.Value!.PngWidth);
            Assert.Equal(single.Value.PngHeight, multi.Value.PngHeight);
            Assert.Equal(single.Value.HasNonBlankPixels, multi.Value.HasNonBlankPixels);
            Assert.Equal(single.Value.NonWhitePixelCount, multi.Value.NonWhitePixelCount);
            Assert.Equal(single.Value.DistinctColorCount, multi.Value.DistinctColorCount);
            Assert.Equal(Sha256Bytes(singleIdat), single.Value.SnapshotSha256);
            Assert.Equal(Sha256Bytes(multiIdat), multi.Value.SnapshotSha256);
            Assert.NotEqual(single.Value.SnapshotSha256, multi.Value.SnapshotSha256);

            var singleDecoded = DesignConversionBrowserPngEvidenceInspector.DecodePngBytesForComparison(singleIdat, singlePath);
            var multiDecoded = DesignConversionBrowserPngEvidenceInspector.DecodePngBytesForComparison(multiIdat, multiPath);

            Assert.True(singleDecoded.IsSuccess, $"{singleDecoded.ErrorCode}: {singleDecoded.ErrorMessage}");
            Assert.True(multiDecoded.IsSuccess, $"{multiDecoded.ErrorCode}: {multiDecoded.ErrorMessage}");
            Assert.Equal(Sha256Bytes(singleDecoded.Value!.RgbaPixels), Sha256Bytes(multiDecoded.Value!.RgbaPixels));
        }
        finally
        {
            if (File.Exists(singlePath))
                File.Delete(singlePath);
            if (File.Exists(multiPath))
                File.Delete(multiPath);
        }
    }

    [Fact]
    public void PngEvidenceInspector_ReturnsOperationResultForTruncatedMultiIdatCompressedData()
    {
        var path = Path.Combine(Path.GetTempPath(), $"truncated-multi-idat-png-{Guid.NewGuid():N}.png");
        try
        {
            var compressed = CompressZlib(TwoColorRgbaScanlines(5, 4));
            var truncated = compressed.AsSpan(0, 3).ToArray();
            File.WriteAllBytes(path, RgbaPngWithCompressedIdat(5, 4, truncated, insertAncillaryBetweenIdats: true, 1, 2, 5));

            var result = DesignConversionBrowserPngEvidenceInspector.Inspect(path);

            Assert.False(result.IsSuccess);
            Assert.Contains(result.ErrorCode, new[] { "PNG_COMPRESSED_DATA_INVALID", "PNG_PIXEL_DATA_INCOMPLETE" });
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void PngEvidenceInspector_ReturnsOperationResultForEmptyIdatData()
    {
        var path = Path.Combine(Path.GetTempPath(), $"empty-idat-png-{Guid.NewGuid():N}.png");
        try
        {
            File.WriteAllBytes(path, PngWithEmptyIdat());

            var result = DesignConversionBrowserPngEvidenceInspector.Inspect(path);

            Assert.False(result.IsSuccess);
            Assert.Equal("PNG_IDAT_MISSING", result.ErrorCode);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void PngEvidenceInspector_ReturnsOperationResultForHugePositiveIdatChunkLength()
    {
        var path = Path.Combine(Path.GetTempPath(), $"huge-idat-length-png-{Guid.NewGuid():N}.png");
        try
        {
            File.WriteAllBytes(path, PngWithHugePositiveIdatLengthWithoutPayload());

            var result = DesignConversionBrowserPngEvidenceInspector.Inspect(path);

            Assert.False(result.IsSuccess);
            Assert.Equal("PNG_CHUNK_INVALID", result.ErrorCode);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void PngEvidenceInspector_DoesNotTreatSingleNonWhitePixelAsNonBlankProof()
    {
        var path = Path.Combine(Path.GetTempPath(), $"single-pixel-artifact-png-{Guid.NewGuid():N}.png");
        try
        {
            File.WriteAllBytes(path, SingleNonWhitePixelRgbaPng(16, 16));

            var result = DesignConversionBrowserPngEvidenceInspector.Inspect(path);

            Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
            Assert.False(result.Value!.HasNonBlankPixels);
            Assert.Equal(1, result.Value.NonWhitePixelCount);
            Assert.Equal(2, result.Value.DistinctColorCount);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void PngEvidenceInspector_AcceptsFlatNonWhitePngAsNonBlankProof()
    {
        var path = Path.Combine(Path.GetTempPath(), $"flat-nonwhite-png-{Guid.NewGuid():N}.png");
        try
        {
            File.WriteAllBytes(path, SolidRgbaPng(16, 16, 18, 52, 86, 255));

            var result = DesignConversionBrowserPngEvidenceInspector.Inspect(path);

            Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
            Assert.True(result.Value!.HasNonBlankPixels);
            Assert.Equal(256, result.Value.NonWhitePixelCount);
            Assert.Equal(1, result.Value.DistinctColorCount);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void PngEvidenceInspector_RejectsShortDecompressedPixelData()
    {
        var path = Path.Combine(Path.GetTempPath(), $"short-png-pixel-data-{Guid.NewGuid():N}.png");
        try
        {
            File.WriteAllBytes(path, RgbaPng(1, 1, [0, 255, 255, 255]));

            var result = DesignConversionBrowserPngEvidenceInspector.Inspect(path);

            Assert.False(result.IsSuccess);
            Assert.Equal("PNG_PIXEL_DATA_INCOMPLETE", result.ErrorCode);
            Assert.Contains("shorter than expected", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void PngEvidenceInspector_RejectsInvalidScanlineFilter()
    {
        var path = Path.Combine(Path.GetTempPath(), $"invalid-filter-png-{Guid.NewGuid():N}.png");
        try
        {
            File.WriteAllBytes(path, RgbaPng(1, 1, [5, 15, 118, 110, 255]));

            var result = DesignConversionBrowserPngEvidenceInspector.Inspect(path);

            Assert.False(result.IsSuccess);
            Assert.Equal("PNG_FILTER_INVALID", result.ErrorCode);
            Assert.Contains("filter is invalid", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public async Task LocalChromiumProcessAdapter_ReportsEnvironmentBlockerForMissingExecutable()
    {
        const string SecretMarker = "N13_MISSING_BROWSER_SECRET_MARKER";
        const string SecretUri = "file:///C:/n13-missing-browser-secret.png";
        var adapter = new LocalChromiumBrowserProcessAdapter(
            executablePath: @"C:\definitely-not-installed\missing-browser.exe",
            timeout: TimeSpan.FromSeconds(1));

        var result = await adapter.CaptureScreenshotAsync(new DesignConversionBrowserCaptureRequest(
            CaptureRequestId: "capture-missing-browser",
            RequestId: "request-001",
            DocumentHtml: $"<!doctype html><html><body><img src=\"{SecretUri}\" data-secret=\"{SecretMarker}\"></body></html>",
            Resolution: new RenderQaResolution("desktop", 800, 600, 1.0),
            Metadata: new Dictionary<string, string>()));

        Assert.False(result.IsSuccess);
        Assert.Equal("BROWSER_RUNTIME_UNAVAILABLE", result.ErrorCode);
        Assert.Contains("Configure DESIGN_CONVERSION_BROWSER_EXECUTABLE", result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain("BROWSER_SCREENSHOT_PROOF_UNAVAILABLE", result.ErrorCode, StringComparison.Ordinal);
        Assert.DoesNotContain("browser_screenshot_file_verified", result.ErrorMessage, StringComparison.Ordinal);
        AssertBrowserCaptureResultDoesNotLeakSecrets(result, SecretUri, SecretMarker);
    }

    [Fact]
    public async Task LocalChromiumProcessAdapter_MissingExecutablePrecedesStorageQuotaWithoutCreatingWorkspace()
    {
        var artifactRoot = NewQuotaTestRoot("design-conversion-browser-missing-runtime-before-quota");
        var missingExecutable = Path.Combine(artifactRoot, "definitely-missing-chromium.exe");
        try
        {
            var adapter = new LocalChromiumBrowserProcessAdapter(
                executablePath: missingExecutable,
                timeout: TimeSpan.FromSeconds(5),
                artifactRoot: artifactRoot,
                maxRetainedCaptureBytes: 0);

            var result = await adapter.CaptureScreenshotAsync(
                BrowserCaptureRequest("missing-runtime-before-zero-quota"));

            Assert.False(result.IsSuccess);
            Assert.Equal("BROWSER_RUNTIME_UNAVAILABLE", result.ErrorCode);
            Assert.Contains("Configure DESIGN_CONVERSION_BROWSER_EXECUTABLE", result.ErrorMessage, StringComparison.Ordinal);
            Assert.False(Directory.Exists(artifactRoot), artifactRoot);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task LocalChromiumProcessAdapter_RetainedGuidPngBytesBlockLaterCaptureBeforeWorkspaceOrProcessSideEffects()
    {
        var artifactRoot = NewQuotaTestRoot("design-conversion-browser-quota-retained-blocks");
        var nonGuidRoot = Path.Combine(artifactRoot, "not-a-guid-sibling");
        var nonGuidSentinel = Path.Combine(nonGuidRoot, "sentinel.txt");
        try
        {
            Directory.CreateDirectory(nonGuidRoot);
            File.WriteAllText(nonGuidSentinel, "keep-non-guid", Encoding.UTF8);
            CreateRetainedCaptureScreenshot(artifactRoot, DesignConversionBrowserPngEvidenceInspector.MaxPngScreenshotBytes);
            var beforeGuidRoots = DirectGuidCaptureDirectories(artifactRoot);
            var runner = new FakeLocalChromiumBrowserProcessRunner(WriteSuccessfulFakeBrowserScreenshotAsync);
            var adapter = CreateQuotaTestAdapter(
                artifactRoot,
                runner,
                maxRetainedCaptureBytes: checked((DesignConversionBrowserPngEvidenceInspector.MaxPngScreenshotBytes * 2) - 1));

            var result = await adapter.CaptureScreenshotAsync(BrowserCaptureRequest("quota-retained-blocked"));

            Assert.False(result.IsSuccess);
            Assert.Equal(LocalChromiumBrowserCaptureStorageBudget.StorageQuotaExceededCode, result.ErrorCode);
            Assert.Empty(runner.Requests);
            Assert.Equal(beforeGuidRoots, DirectGuidCaptureDirectories(artifactRoot));
            Assert.True(File.Exists(nonGuidSentinel), nonGuidSentinel);
            Assert.DoesNotContain(artifactRoot, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task LocalChromiumProcessAdapter_FreshInstanceObservesPreviouslyRetainedGuidPngBytes()
    {
        var artifactRoot = NewQuotaTestRoot("design-conversion-browser-quota-restart");
        try
        {
            var firstRunner = new FakeLocalChromiumBrowserProcessRunner(WriteSuccessfulFakeBrowserScreenshotAsync);
            var firstAdapter = CreateQuotaTestAdapter(
                artifactRoot,
                firstRunner,
                maxRetainedCaptureBytes: LocalChromiumBrowserProcessAdapter.DefaultMaxRetainedCaptureBytes);
            var first = await firstAdapter.CaptureScreenshotAsync(BrowserCaptureRequest("quota-restart-first"));
            Assert.True(first.IsSuccess, first.ErrorMessage);
            var retainedLength = new FileInfo(new Uri(first.Value!.ScreenshotUri).LocalPath).Length;
            Assert.True(retainedLength > 0);

            var freshRunner = new FakeLocalChromiumBrowserProcessRunner(WriteSuccessfulFakeBrowserScreenshotAsync);
            var freshAdapter = CreateQuotaTestAdapter(
                artifactRoot,
                freshRunner,
                maxRetainedCaptureBytes: checked(retainedLength + DesignConversionBrowserPngEvidenceInspector.MaxPngScreenshotBytes - 1));

            var blocked = await freshAdapter.CaptureScreenshotAsync(BrowserCaptureRequest("quota-restart-blocked"));

            Assert.False(blocked.IsSuccess);
            Assert.Equal(LocalChromiumBrowserCaptureStorageBudget.StorageQuotaExceededCode, blocked.ErrorCode);
            Assert.Empty(freshRunner.Requests);
            Assert.Single(DirectGuidCaptureDirectories(artifactRoot));
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task LocalChromiumProcessAdapter_ConcurrentReservationsCannotOverbookConfiguredQuota()
    {
        var artifactRoot = NewQuotaTestRoot("design-conversion-browser-quota-concurrent");
        var firstRunnerEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirstRunner = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        try
        {
            var runner = new FakeLocalChromiumBrowserProcessRunner(async (request, cancellationToken) =>
            {
                firstRunnerEntered.SetResult();
                await releaseFirstRunner.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
                return await WriteSuccessfulFakeBrowserScreenshotAsync(request, cancellationToken);
            });
            var adapter = CreateQuotaTestAdapter(
                artifactRoot,
                runner,
                maxRetainedCaptureBytes: DesignConversionBrowserPngEvidenceInspector.MaxPngScreenshotBytes,
                maxConcurrency: 2);

            var first = adapter.CaptureScreenshotAsync(BrowserCaptureRequest("quota-concurrent-first"));
            await firstRunnerEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            var second = await adapter.CaptureScreenshotAsync(BrowserCaptureRequest("quota-concurrent-second"));

            Assert.False(second.IsSuccess);
            Assert.Equal(LocalChromiumBrowserCaptureStorageBudget.StorageQuotaExceededCode, second.ErrorCode);
            Assert.Single(runner.Requests);

            releaseFirstRunner.SetResult();
            var firstResult = await first.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.True(firstResult.IsSuccess, firstResult.ErrorMessage);
        }
        finally
        {
            releaseFirstRunner.TrySetResult();
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task LocalChromiumProcessAdapter_ReleasesReservationAfterFailureSoLaterWorkCanProceed()
    {
        var artifactRoot = NewQuotaTestRoot("design-conversion-browser-quota-failure-release");
        try
        {
            var runner = new FakeLocalChromiumBrowserProcessRunner(
                (_, _) => Task.FromResult(OperationResult<LocalChromiumBrowserProcessRunResult>.Fail(
                    "Fake browser failure.",
                    "BROWSER_SCREENSHOT_FAILED")),
                WriteSuccessfulFakeBrowserScreenshotAsync);
            var adapter = CreateQuotaTestAdapter(
                artifactRoot,
                runner,
                maxRetainedCaptureBytes: DesignConversionBrowserPngEvidenceInspector.MaxPngScreenshotBytes);

            var failed = await adapter.CaptureScreenshotAsync(BrowserCaptureRequest("quota-failure-release-first"));
            var later = await adapter.CaptureScreenshotAsync(BrowserCaptureRequest("quota-failure-release-later"));

            Assert.False(failed.IsSuccess);
            Assert.Equal("BROWSER_SCREENSHOT_FAILED", failed.ErrorCode);
            Assert.True(later.IsSuccess, later.ErrorMessage);
            Assert.Equal(2, runner.Requests.Count);
            Assert.Single(DirectGuidCaptureDirectories(artifactRoot));
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task LocalChromiumProcessAdapter_ClassifiesRecognizedProcessCrashBeforeScreenshotAsRuntimeBlocker()
    {
        var artifactRoot = NewQuotaTestRoot("design-conversion-browser-runtime-crash-classification");
        const string SecretPath = @"C:\secret\chromium-profile\debug.log";
        try
        {
            var runner = new FakeLocalChromiumBrowserProcessRunner((_, _) =>
                Task.FromResult(LocalChromiumBrowserProcessAdapter.ClassifyCompletedBrowserProcessFailure(
                    unchecked((int)0x80000003),
                    screenshotExists: false,
                    stderr: $"Chromium startup crash wrote {SecretPath}",
                    stdout: string.Empty)));
            var adapter = CreateQuotaTestAdapter(
                artifactRoot,
                runner,
                maxRetainedCaptureBytes: DesignConversionBrowserPngEvidenceInspector.MaxPngScreenshotBytes);

            var result = await adapter.CaptureScreenshotAsync(BrowserCaptureRequest("runtime-crash-classified"));

            Assert.False(result.IsSuccess);
            Assert.Equal(LocalChromiumBrowserProcessAdapter.RuntimeCrashedBeforeScreenshotCode, result.ErrorCode);
            Assert.Contains("recognized process crash code", result.ErrorMessage, StringComparison.Ordinal);
            Assert.Contains("exit_code=-2147483645", result.ErrorMessage, StringComparison.Ordinal);
            Assert.Contains("stderr_sha256=", result.ErrorMessage, StringComparison.Ordinal);
            Assert.Contains("stderr_chars=", result.ErrorMessage, StringComparison.Ordinal);
            Assert.DoesNotContain(SecretPath, result.ErrorMessage, StringComparison.Ordinal);
            Assert.Single(runner.Requests);
            Assert.Empty(DirectGuidCaptureDirectories(artifactRoot));
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task LocalChromiumProcessAdapter_KeepsUnrelatedProcessFailureAsScreenshotFailure()
    {
        var artifactRoot = NewQuotaTestRoot("design-conversion-browser-unrelated-failure-classification");
        try
        {
            var runner = new FakeLocalChromiumBrowserProcessRunner((_, _) =>
                Task.FromResult(LocalChromiumBrowserProcessAdapter.ClassifyCompletedBrowserProcessFailure(
                    exitCode: 1,
                    screenshotExists: false,
                    stderr: "Ordinary page/render failure",
                    stdout: string.Empty)));
            var adapter = CreateQuotaTestAdapter(
                artifactRoot,
                runner,
                maxRetainedCaptureBytes: DesignConversionBrowserPngEvidenceInspector.MaxPngScreenshotBytes);

            var result = await adapter.CaptureScreenshotAsync(BrowserCaptureRequest("unrelated-failure-not-runtime"));

            Assert.False(result.IsSuccess);
            Assert.Equal(LocalChromiumBrowserProcessAdapter.ScreenshotFailedCode, result.ErrorCode);
            Assert.NotEqual(LocalChromiumBrowserProcessAdapter.RuntimeCrashedBeforeScreenshotCode, result.ErrorCode);
            Assert.Contains("exit code 1", result.ErrorMessage, StringComparison.Ordinal);
            Assert.DoesNotContain("Ordinary page/render failure", result.ErrorMessage, StringComparison.Ordinal);
            Assert.Single(runner.Requests);
            Assert.Empty(DirectGuidCaptureDirectories(artifactRoot));
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task LocalChromiumProcessAdapter_ReleasesReservationAfterCancellationSoLaterWorkCanProceed()
    {
        var artifactRoot = NewQuotaTestRoot("design-conversion-browser-quota-cancel-release");
        using var cancellation = new CancellationTokenSource();
        try
        {
            var runner = new FakeLocalChromiumBrowserProcessRunner(
                async (_, cancellationToken) =>
                {
                    cancellation.Cancel();
                    await Task.Yield();
                    cancellationToken.ThrowIfCancellationRequested();
                    return OperationResult<LocalChromiumBrowserProcessRunResult>.Ok(
                        new LocalChromiumBrowserProcessRunResult(ElapsedMs: 0, Stdout: "", Stderr: ""));
                },
                WriteSuccessfulFakeBrowserScreenshotAsync);
            var adapter = CreateQuotaTestAdapter(
                artifactRoot,
                runner,
                maxRetainedCaptureBytes: DesignConversionBrowserPngEvidenceInspector.MaxPngScreenshotBytes);

            var cancelled = await adapter.CaptureScreenshotAsync(
                BrowserCaptureRequest("quota-cancel-release-first"),
                cancellation.Token);
            var later = await adapter.CaptureScreenshotAsync(BrowserCaptureRequest("quota-cancel-release-later"));

            Assert.False(cancelled.IsSuccess);
            Assert.Equal("CANCELLED", cancelled.ErrorCode);
            Assert.True(later.IsSuccess, later.ErrorMessage);
            Assert.Equal(2, runner.Requests.Count);
            Assert.Single(DirectGuidCaptureDirectories(artifactRoot));
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task LocalChromiumProcessAdapter_MapsCancellationAfterScreenshotBeforeReadAndCleansWorkspace()
    {
        var artifactRoot = NewQuotaTestRoot("design-conversion-browser-cancel-after-screenshot");
        using var cancellation = new CancellationTokenSource();
        try
        {
            var privateMarker = "PRIVATE_R3_BROWSER_CAPTURE_CANCEL_" + Guid.NewGuid().ToString("N");
            var runner = new FakeLocalChromiumBrowserProcessRunner((request, _) =>
            {
                File.WriteAllBytes(request.ScreenshotPath, MinimalPngHeader(1, 1));
                cancellation.Cancel();
                return Task.FromResult(OperationResult<LocalChromiumBrowserProcessRunResult>.Ok(
                    new LocalChromiumBrowserProcessRunResult(ElapsedMs: 1, Stdout: privateMarker, Stderr: privateMarker)));
            });
            var adapter = CreateQuotaTestAdapter(
                artifactRoot,
                runner,
                maxRetainedCaptureBytes: DesignConversionBrowserPngEvidenceInspector.MaxPngScreenshotBytes);

            var result = await adapter.CaptureScreenshotAsync(
                BrowserCaptureRequest("cancel-after-screenshot-before-read"),
                cancellation.Token);

            Assert.False(result.IsSuccess);
            Assert.Equal("CANCELLED", result.ErrorCode);
            Assert.Single(runner.Requests);
            Assert.Empty(DirectGuidCaptureDirectories(artifactRoot));
            Assert.DoesNotContain(privateMarker, result.ErrorMessage, StringComparison.Ordinal);
            Assert.DoesNotContain(Convert.ToBase64String(Encoding.UTF8.GetBytes(privateMarker)), result.ErrorMessage, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task LocalChromiumProcessAdapter_IgnoresNonGuidSiblingsAndDoesNotDeleteThem()
    {
        var artifactRoot = NewQuotaTestRoot("design-conversion-browser-quota-non-guid");
        var nonGuidRoot = Path.Combine(artifactRoot, "non-guid-retained-workspace");
        var nonGuidScreenshot = Path.Combine(nonGuidRoot, "screenshot.png");
        var nonGuidSentinel = Path.Combine(nonGuidRoot, "sentinel.txt");
        try
        {
            Directory.CreateDirectory(nonGuidRoot);
            using (var stream = File.Create(nonGuidScreenshot))
                stream.SetLength(checked(DesignConversionBrowserPngEvidenceInspector.MaxPngScreenshotBytes * 4));
            File.WriteAllText(nonGuidSentinel, "keep", Encoding.UTF8);
            var runner = new FakeLocalChromiumBrowserProcessRunner(WriteSuccessfulFakeBrowserScreenshotAsync);
            var adapter = CreateQuotaTestAdapter(
                artifactRoot,
                runner,
                maxRetainedCaptureBytes: DesignConversionBrowserPngEvidenceInspector.MaxPngScreenshotBytes);

            var result = await adapter.CaptureScreenshotAsync(BrowserCaptureRequest("quota-non-guid-ignored"));

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.True(File.Exists(nonGuidSentinel), nonGuidSentinel);
            Assert.True(File.Exists(nonGuidScreenshot), nonGuidScreenshot);
            Assert.Single(DirectGuidCaptureDirectories(artifactRoot));
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task LocalChromiumProcessAdapter_GuidReparseStorageStateFailsClosedWithoutTraversingOrDeletingTarget()
    {
        const string SecretMarker = "BROWSER_QUOTA_REPARSE_TARGET_SECRET";
        var testRoot = NewQuotaTestRoot("design-conversion-browser-quota-reparse");
        var artifactRoot = Path.Combine(testRoot, "base");
        var targetRoot = Path.Combine(testRoot, "target-outside-base");
        var linkRoot = Path.Combine(artifactRoot, Guid.NewGuid().ToString("N"));
        var targetSentinel = Path.Combine(targetRoot, "sentinel.txt");
        var linkCreated = false;
        Exception? linkCreationFailure = null;

        try
        {
            Directory.CreateDirectory(artifactRoot);
            Directory.CreateDirectory(targetRoot);
            File.WriteAllText(targetSentinel, SecretMarker, Encoding.UTF8);

            try
            {
                Directory.CreateSymbolicLink(linkRoot, targetRoot);
                linkCreated = true;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or PlatformNotSupportedException)
            {
                linkCreationFailure = ex;
            }

            Assert.True(
                linkCreated,
                $"The GUID reparse proof requires a local directory symbolic link. " +
                $"Creation failed with {linkCreationFailure?.GetType().Name}: {linkCreationFailure?.Message}");

            var runner = new FakeLocalChromiumBrowserProcessRunner(WriteSuccessfulFakeBrowserScreenshotAsync);
            var adapter = CreateQuotaTestAdapter(
                artifactRoot,
                runner,
                maxRetainedCaptureBytes: DesignConversionBrowserPngEvidenceInspector.MaxPngScreenshotBytes);

            var result = await adapter.CaptureScreenshotAsync(BrowserCaptureRequest("quota-reparse-fails-closed"));

            Assert.False(result.IsSuccess);
            Assert.Equal(LocalChromiumBrowserCaptureStorageBudget.StorageQuotaUnsafeStateCode, result.ErrorCode);
            Assert.Empty(runner.Requests);
            Assert.True(File.Exists(targetSentinel), targetSentinel);
            Assert.True(Directory.Exists(targetRoot), targetRoot);
            Assert.DoesNotContain(testRoot, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(SecretMarker, result.ErrorMessage, StringComparison.Ordinal);
        }
        finally
        {
            if (linkCreated && Directory.Exists(linkRoot))
                Directory.Delete(linkRoot);
            DeleteDirectoryIfExists(testRoot);
        }
    }

    [Fact]
    public async Task LocalChromiumProcessAdapter_RejectsOversizeScreenshotByFileLengthBeforeReadingBytes()
    {
        var artifactRoot = NewQuotaTestRoot("design-conversion-browser-quota-oversize");
        try
        {
            var runner = new FakeLocalChromiumBrowserProcessRunner((request, _) =>
            {
                using (var stream = File.Create(request.ScreenshotPath))
                {
                    var header = MinimalPngHeader(1, 1);
                    stream.Write(header, 0, header.Length);
                    stream.SetLength(DesignConversionBrowserPngEvidenceInspector.MaxPngScreenshotBytes + 1);
                }

                return Task.FromResult(OperationResult<LocalChromiumBrowserProcessRunResult>.Ok(
                    new LocalChromiumBrowserProcessRunResult(ElapsedMs: 1, Stdout: "", Stderr: "")));
            });
            var adapter = CreateQuotaTestAdapter(
                artifactRoot,
                runner,
                maxRetainedCaptureBytes: LocalChromiumBrowserProcessAdapter.DefaultMaxRetainedCaptureBytes);

            var result = await adapter.CaptureScreenshotAsync(BrowserCaptureRequest("quota-oversize-before-read"));

            Assert.False(result.IsSuccess);
            Assert.Equal("BROWSER_SCREENSHOT_BYTES_TOO_LARGE", result.ErrorCode);
            Assert.Single(runner.Requests);
            Assert.Empty(DirectGuidCaptureDirectories(artifactRoot));
            Assert.DoesNotContain(artifactRoot, result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task LocalChromiumProcessAdapter_RetainsSuccessfulScreenshotAndCleansFailedWorkspaceWithStorageBudget()
    {
        var artifactRoot = NewQuotaTestRoot("design-conversion-browser-quota-retention-cleanup");
        try
        {
            var runner = new FakeLocalChromiumBrowserProcessRunner(
                WriteSuccessfulFakeBrowserScreenshotAsync,
                (_, _) => Task.FromResult(OperationResult<LocalChromiumBrowserProcessRunResult>.Fail(
                    "Fake browser failure.",
                    "BROWSER_SCREENSHOT_FAILED")));
            var adapter = CreateQuotaTestAdapter(
                artifactRoot,
                runner,
                maxRetainedCaptureBytes: LocalChromiumBrowserProcessAdapter.DefaultMaxRetainedCaptureBytes);

            var success = await adapter.CaptureScreenshotAsync(BrowserCaptureRequest("quota-retain-success"));
            var retainedRoot = Assert.Single(DirectGuidCaptureDirectories(artifactRoot));
            var retainedScreenshot = Path.Combine(retainedRoot, "screenshot.png");
            var failed = await adapter.CaptureScreenshotAsync(BrowserCaptureRequest("quota-clean-failed"));

            Assert.True(success.IsSuccess, success.ErrorMessage);
            Assert.True(File.Exists(retainedScreenshot), retainedScreenshot);
            Assert.False(File.Exists(Path.Combine(retainedRoot, "index.html")));
            Assert.False(Directory.Exists(Path.Combine(retainedRoot, "profile")));
            Assert.False(failed.IsSuccess);
            Assert.Equal("BROWSER_SCREENSHOT_FAILED", failed.ErrorCode);
            Assert.Equal([retainedRoot], DirectGuidCaptureDirectories(artifactRoot));
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public async Task LocalChromiumProcessAdapter_CleansSensitiveWorkspaceAcrossSuccessFailureTimeoutAndCancellation()
    {
        var artifactRoot = NewQuotaTestRoot("design-conversion-browser-n6-6a-capture-cleanup");
        var privateMarker = "PRIVATE_N6_6A_BROWSER_CAPTURE_" + Guid.NewGuid().ToString("N");
        using var cancellation = new CancellationTokenSource();
        try
        {
            var runner = new FakeLocalChromiumBrowserProcessRunner(
                async (request, cancellationToken) =>
                {
                    WriteFakeBrowserProfileCache(request.UserDataPath, privateMarker);
                    return await WriteSuccessfulFakeBrowserScreenshotAsync(request, cancellationToken);
                },
                (request, _) =>
                {
                    WriteFakeBrowserProfileCache(request.UserDataPath, privateMarker);
                    return Task.FromResult(OperationResult<LocalChromiumBrowserProcessRunResult>.Fail(
                        "Fake browser process failed after writing transient profile data.",
                        LocalChromiumBrowserProcessAdapter.ScreenshotFailedCode));
                },
                (request, _) =>
                {
                    WriteFakeBrowserProfileCache(request.UserDataPath, privateMarker);
                    throw new OperationCanceledException();
                },
                (request, cancellationToken) =>
                {
                    WriteFakeBrowserProfileCache(request.UserDataPath, privateMarker);
                    cancellation.Cancel();
                    cancellationToken.ThrowIfCancellationRequested();
                    return Task.FromResult(OperationResult<LocalChromiumBrowserProcessRunResult>.Ok(
                        new LocalChromiumBrowserProcessRunResult(ElapsedMs: 1, Stdout: privateMarker, Stderr: privateMarker)));
                });
            var adapter = CreateQuotaTestAdapter(
                artifactRoot,
                runner,
                maxRetainedCaptureBytes: LocalChromiumBrowserProcessAdapter.DefaultMaxRetainedCaptureBytes);

            var success = await adapter.CaptureScreenshotAsync(BrowserCaptureRequest("n6-6a-success"));
            Assert.True(success.IsSuccess, success.ErrorMessage);
            var retainedRoot = Assert.Single(DirectGuidCaptureDirectories(artifactRoot));
            var retainedScreenshot = Path.GetFullPath(new Uri(success.Value!.ScreenshotUri).LocalPath);
            var expectedRetainedScreenshot = Path.GetFullPath(Path.Combine(retainedRoot, "screenshot.png"));
            Assert.True(
                string.Equals(
                    expectedRetainedScreenshot,
                    retainedScreenshot,
                    OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal),
                $"Expected retained screenshot path {expectedRetainedScreenshot}, actual {retainedScreenshot}.");
            AssertCaptureRootContainsOnlyRetainedScreenshots(artifactRoot, expectedScreenshotCount: 1);
            AssertNoSensitiveBrowserWorkspaceArtifactsRemain(artifactRoot, privateMarker);

            var processFailure = await adapter.CaptureScreenshotAsync(BrowserCaptureRequest("n6-6a-process-failure"));
            Assert.False(processFailure.IsSuccess);
            Assert.Equal(LocalChromiumBrowserProcessAdapter.ScreenshotFailedCode, processFailure.ErrorCode);
            Assert.Equal([retainedRoot], DirectGuidCaptureDirectories(artifactRoot));
            AssertCaptureRootContainsOnlyRetainedScreenshots(artifactRoot, expectedScreenshotCount: 1);
            AssertNoSensitiveBrowserWorkspaceArtifactsRemain(artifactRoot, privateMarker);
            AssertBrowserCaptureResultDoesNotLeakSecrets(processFailure, artifactRoot, privateMarker);

            var timeout = await adapter.CaptureScreenshotAsync(BrowserCaptureRequest("n6-6a-timeout"));
            Assert.False(timeout.IsSuccess);
            Assert.Equal("BROWSER_SCREENSHOT_TIMEOUT", timeout.ErrorCode);
            Assert.Equal([retainedRoot], DirectGuidCaptureDirectories(artifactRoot));
            AssertCaptureRootContainsOnlyRetainedScreenshots(artifactRoot, expectedScreenshotCount: 1);
            AssertNoSensitiveBrowserWorkspaceArtifactsRemain(artifactRoot, privateMarker);
            AssertBrowserCaptureResultDoesNotLeakSecrets(timeout, artifactRoot, privateMarker);

            var cancelled = await adapter.CaptureScreenshotAsync(
                BrowserCaptureRequest("n6-6a-cancelled"),
                cancellation.Token);
            Assert.False(cancelled.IsSuccess);
            Assert.Equal("CANCELLED", cancelled.ErrorCode);
            Assert.Equal([retainedRoot], DirectGuidCaptureDirectories(artifactRoot));
            AssertCaptureRootContainsOnlyRetainedScreenshots(artifactRoot, expectedScreenshotCount: 1);
            AssertNoSensitiveBrowserWorkspaceArtifactsRemain(artifactRoot, privateMarker);
            AssertBrowserCaptureResultDoesNotLeakSecrets(cancelled, artifactRoot, privateMarker);
            Assert.Equal(4, runner.Requests.Count);

            foreach (var request in runner.Requests.Skip(1))
            {
                var captureRoot = Path.GetDirectoryName(request.ScreenshotPath);
                Assert.False(Directory.Exists(captureRoot), captureRoot);
                Assert.False(File.Exists(request.HtmlPath), request.HtmlPath);
                Assert.False(Directory.Exists(request.UserDataPath), request.UserDataPath);
            }
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }
    }

    [Fact]
    public void LocalChromiumProcessAdapter_ProcessCleanupTerminatesKnownDescendantAfterLauncherExited()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var testRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-browser-exited-launcher-" + Guid.NewGuid().ToString("N"));
        var parentScriptPath = Path.Combine(testRoot, "parent.ps1");
        var childScriptPath = Path.Combine(testRoot, "child.ps1");
        var childPidPath = Path.Combine(testRoot, "child.pid");
        var marker = "design-conversion-known-child-" + Guid.NewGuid().ToString("N");
        var powershellPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.System),
            "WindowsPowerShell",
            "v1.0",
            "powershell.exe");
        Process? launcher = null;
        Process? child = null;

        try
        {
            Directory.CreateDirectory(testRoot);
            File.WriteAllText(
                childScriptPath,
                """
                param([Parameter(Mandatory=$true)][string]$Marker)
                $null = $Marker
                Start-Sleep -Seconds 60
                """,
                Encoding.UTF8);
            File.WriteAllText(
                parentScriptPath,
                """
                param(
                    [Parameter(Mandatory=$true)][string]$PidFile,
                    [Parameter(Mandatory=$true)][string]$ChildScript,
                    [Parameter(Mandatory=$true)][string]$Marker)

                $startInfo = New-Object System.Diagnostics.ProcessStartInfo
                $startInfo.FileName = (Get-Process -Id $PID).Path
                $startInfo.Arguments = '-NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File "' + $ChildScript + '" -Marker "' + $Marker + '"'
                $startInfo.UseShellExecute = $false
                $startInfo.CreateNoWindow = $true
                $child = [System.Diagnostics.Process]::Start($startInfo)
                [System.IO.File]::WriteAllText(
                    $PidFile,
                    $child.Id.ToString([System.Globalization.CultureInfo]::InvariantCulture))
                """,
                Encoding.UTF8);
            Assert.True(File.Exists(powershellPath), "Windows PowerShell executable is required for this Windows-only process-lifetime test.");

            var startInfo = new ProcessStartInfo
            {
                FileName = powershellPath,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            startInfo.ArgumentList.Add("-NoLogo");
            startInfo.ArgumentList.Add("-NoProfile");
            startInfo.ArgumentList.Add("-NonInteractive");
            startInfo.ArgumentList.Add("-ExecutionPolicy");
            startInfo.ArgumentList.Add("Bypass");
            startInfo.ArgumentList.Add("-File");
            startInfo.ArgumentList.Add(parentScriptPath);
            startInfo.ArgumentList.Add("-PidFile");
            startInfo.ArgumentList.Add(childPidPath);
            startInfo.ArgumentList.Add("-ChildScript");
            startInfo.ArgumentList.Add(childScriptPath);
            startInfo.ArgumentList.Add("-Marker");
            startInfo.ArgumentList.Add(marker);

            launcher = Process.Start(startInfo);
            Assert.NotNull(launcher);
            Assert.True(launcher.WaitForExit(5000), "Owned launcher did not exit within the bounded test setup window.");
            Assert.True(launcher.HasExited);
            Assert.True(
                SpinWait.SpinUntil(() => File.Exists(childPidPath), TimeSpan.FromSeconds(2)),
                "Owned launcher did not publish its child PID.");

            var childPid = int.Parse(File.ReadAllText(childPidPath), System.Globalization.CultureInfo.InvariantCulture);
            child = Process.GetProcessById(childPid);
            Assert.False(child.HasExited);

            LocalChromiumBrowserProcessAdapter.TryKillBrowserProcess(launcher);

            Assert.True(
                child.WaitForExit(2000),
                "Production cleanup did not terminate the known descendant of an already-exited launcher.");
            Assert.True(child.HasExited);
        }
        finally
        {
            if (child is not null)
            {
                try
                {
                    if (!child.HasExited)
                    {
                        child.Kill(entireProcessTree: true);
                        child.WaitForExit(2000);
                    }
                }
                catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
                {
                }
                finally
                {
                    child.Dispose();
                }
            }

            if (launcher is not null)
            {
                try
                {
                    if (!launcher.HasExited)
                    {
                        launcher.Kill(entireProcessTree: true);
                        launcher.WaitForExit(2000);
                    }
                }
                catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
                {
                }
                finally
                {
                    launcher.Dispose();
                }
            }

            DeleteDirectoryIfExists(testRoot);
        }
    }

    [Fact]
    public void LocalChromiumProcessAdapter_SensitiveCleanupRequiresStableAbsenceAfterProfileReappears()
    {
        var testRoot = Path.Combine(Path.GetTempPath(), "design-conversion-cleanup-reappearance");
        var captureRoot = Path.Combine(testRoot, Guid.NewGuid().ToString("N"));
        var htmlPath = Path.Combine(captureRoot, "index.html");
        var profilePath = Path.Combine(captureRoot, "profile");
        var fileSystem = new ScriptedWorkspaceFileSystem(captureRoot, htmlPath, profilePath);
        var delays = new List<TimeSpan>();
        var cleaner = new LocalChromiumBrowserProcessAdapter.RetryingWorkspaceCleaner(
            fileSystem,
            delay =>
            {
                delays.Add(delay);
                if (delays.Count == 1)
                    fileSystem.ReappearProfile();
            });

        var cleanup = cleaner.CleanupSensitiveBrowserCaptureWorkspace(captureRoot, htmlPath, profilePath);

        Assert.True(cleanup.IsSuccess, cleanup.ErrorMessage);
        Assert.Equal(1, fileSystem.DeleteFileCallsFor(htmlPath));
        Assert.Equal(2, fileSystem.DeleteDirectoryCallsFor(profilePath));
        Assert.Equal(
            LocalChromiumBrowserProcessAdapter.RetryingWorkspaceCleaner.StableAbsenceObservationCount + 1,
            delays.Count);
        Assert.All(
            delays,
            delay => Assert.Equal(
                LocalChromiumBrowserProcessAdapter.RetryingWorkspaceCleaner.RetryDelay,
                delay));
        Assert.False(fileSystem.HtmlExists);
        Assert.False(fileSystem.ProfileExists);
    }

    [Fact]
    public void LocalChromiumProcessAdapter_SensitiveCleanupExhaustsSharedDelayBudgetWhenProfileKeepsReappearing()
    {
        const string PrivateMarker = "PRIVATE_REAPPEARING_PROFILE_CLEANUP";
        var testRoot = Path.Combine(Path.GetTempPath(), "design-conversion-cleanup-reappearance-bound");
        var captureRoot = Path.Combine(testRoot, Guid.NewGuid().ToString("N"));
        var htmlPath = Path.Combine(captureRoot, "index.html");
        var profilePath = Path.Combine(captureRoot, "profile");
        var fileSystem = new ScriptedWorkspaceFileSystem(
            captureRoot,
            htmlPath,
            profilePath,
            profileReappearsAfterDelete: true);
        var delays = new List<TimeSpan>();
        var cleaner = new LocalChromiumBrowserProcessAdapter.RetryingWorkspaceCleaner(
            fileSystem,
            delays.Add);

        var cleanup = cleaner.CleanupSensitiveBrowserCaptureWorkspace(captureRoot, htmlPath, profilePath);

        AssertSensitiveCleanupFailureDoesNotLeak(
            cleanup,
            testRoot,
            captureRoot,
            htmlPath,
            profilePath,
            PrivateMarker);
        Assert.Equal(
            LocalChromiumBrowserProcessAdapter.RetryingWorkspaceCleaner.MaxCleanupDrainDelaySteps + 1,
            fileSystem.DeleteDirectoryCallsFor(profilePath));
        Assert.Equal(
            LocalChromiumBrowserProcessAdapter.RetryingWorkspaceCleaner.MaxCleanupDrainDelaySteps,
            delays.Count);
        Assert.Equal(
            LocalChromiumBrowserProcessAdapter.RetryingWorkspaceCleaner.MaxCleanupDrainDelayMilliseconds,
            delays.Sum(delay => delay.TotalMilliseconds));
        Assert.True(fileSystem.ProfileExists);
    }

    [Fact]
    public void LocalChromiumProcessAdapter_CleanupValidationFailuresDoNotEnterRetryDrain()
    {
        var testRoot = Path.Combine(Path.GetTempPath(), "design-conversion-cleanup-validation");
        var baseRoot = Path.Combine(testRoot, "base");
        var captureRoot = Path.Combine(baseRoot, Guid.NewGuid().ToString("N"));
        var htmlPath = Path.Combine(captureRoot, "index.html");
        var profilePath = Path.Combine(captureRoot, "profile");

        var invalidFileSystem = new ScriptedWorkspaceFileSystem(captureRoot, htmlPath, profilePath);
        var invalidDelays = new List<TimeSpan>();
        var invalidCleaner = new LocalChromiumBrowserProcessAdapter.RetryingWorkspaceCleaner(
            invalidFileSystem,
            invalidDelays.Add);
        var invalid = invalidCleaner.CleanupSensitiveBrowserCaptureWorkspace(
            captureRoot,
            Path.Combine(captureRoot, "wrong.html"),
            profilePath);

        AssertSensitiveCleanupFailureDoesNotLeak(invalid, testRoot, captureRoot);
        Assert.Equal(0, invalidFileSystem.TotalFileSystemCalls);
        Assert.Empty(invalidDelays);

        var outsideRoot = Path.Combine(testRoot, "outside", Guid.NewGuid().ToString("N"));
        var outsideFileSystem = new ScriptedWorkspaceFileSystem(
            outsideRoot,
            Path.Combine(outsideRoot, "index.html"),
            Path.Combine(outsideRoot, "profile"));
        var outsideDelays = new List<TimeSpan>();
        var outsideCleaner = new LocalChromiumBrowserProcessAdapter.RetryingWorkspaceCleaner(
            outsideFileSystem,
            outsideDelays.Add);
        var outside = outsideCleaner.CleanupFailedBrowserCaptureWorkspace(baseRoot, outsideRoot);

        AssertSensitiveCleanupFailureDoesNotLeak(outside, testRoot, baseRoot, outsideRoot);
        Assert.Equal(0, outsideFileSystem.TotalFileSystemCalls);
        Assert.Empty(outsideDelays);

        var reparseFileSystem = new ScriptedWorkspaceFileSystem(
            captureRoot,
            htmlPath,
            profilePath,
            rootIsReparsePoint: true);
        var reparseDelays = new List<TimeSpan>();
        var reparseCleaner = new LocalChromiumBrowserProcessAdapter.RetryingWorkspaceCleaner(
            reparseFileSystem,
            reparseDelays.Add);
        var reparse = reparseCleaner.CleanupFailedBrowserCaptureWorkspace(baseRoot, captureRoot);

        AssertSensitiveCleanupFailureDoesNotLeak(reparse, testRoot, baseRoot, captureRoot);
        Assert.Equal(1, reparseFileSystem.DirectoryExistsCalls);
        Assert.Equal(1, reparseFileSystem.GetDirectoryAttributesCalls);
        Assert.Equal(0, reparseFileSystem.DeleteDirectoryCallsFor(captureRoot));
        Assert.Empty(reparseDelays);
    }

    [Fact]
    public void LocalChromiumProcessAdapter_SensitiveCleanupRetriesTransientProfileLockAndRetainsScreenshot()
    {
        const string PrivateMarker = "PRIVATE_TRANSIENT_PROFILE_LOCK_CLEANUP";
        const int FailuresBeforeSuccess = 9;
        var testRoot = NewQuotaTestRoot("design-conversion-browser-transient-profile-cleanup");
        var captureRoot = Path.Combine(testRoot, Guid.NewGuid().ToString("N"));
        var htmlPath = Path.Combine(captureRoot, "index.html");
        var profilePath = Path.Combine(captureRoot, "profile");
        var nestedProfilePath = Path.Combine(profilePath, "Default", "Cookies");
        var screenshotPath = Path.Combine(captureRoot, "screenshot.png");
        var retryDelays = new List<TimeSpan>();

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(nestedProfilePath)!);
            File.WriteAllText(htmlPath, $"<main>{PrivateMarker}</main>", Encoding.UTF8);
            File.WriteAllText(nestedProfilePath, PrivateMarker, Encoding.UTF8);
            File.WriteAllBytes(screenshotPath, SolidRgbaPng(4, 4, 32, 96, 160, 255));
            var fileSystem = new TransientDeleteDirectoryFailureFileSystem(
                profilePath,
                failuresBeforeSuccess: FailuresBeforeSuccess);
            var cleaner = new LocalChromiumBrowserProcessAdapter.RetryingWorkspaceCleaner(
                fileSystem,
                retryDelays.Add);

            var cleanup = cleaner.CleanupSensitiveBrowserCaptureWorkspace(captureRoot, htmlPath, profilePath);

            Assert.True(cleanup.IsSuccess, cleanup.ErrorMessage);
            Assert.Equal(FailuresBeforeSuccess + 1, fileSystem.DeleteDirectoryCallsFor(profilePath));
            Assert.Equal(
                FailuresBeforeSuccess +
                LocalChromiumBrowserProcessAdapter.RetryingWorkspaceCleaner.StableAbsenceObservationCount,
                retryDelays.Count);
            Assert.All(
                retryDelays,
                delay => Assert.Equal(
                    LocalChromiumBrowserProcessAdapter.RetryingWorkspaceCleaner.RetryDelay,
                    delay));
            Assert.False(File.Exists(htmlPath), htmlPath);
            Assert.False(Directory.Exists(profilePath), profilePath);
            Assert.True(File.Exists(screenshotPath), screenshotPath);
        }
        finally
        {
            DeleteDirectoryIfExists(testRoot);
        }
    }

    [Fact]
    public void LocalChromiumProcessAdapter_FailedWorkspaceCleanupExhaustsTransientRetriesWithSafeEnvelope()
    {
        const string PrivateMarker = "PRIVATE_EXHAUSTED_BROWSER_CLEANUP";
        var baseRoot = NewQuotaTestRoot("design-conversion-browser-exhausted-cleanup");
        var captureRoot = Path.Combine(baseRoot, Guid.NewGuid().ToString("N"));
        var htmlPath = Path.Combine(captureRoot, "index.html");
        var retryDelays = new List<TimeSpan>();

        try
        {
            Directory.CreateDirectory(captureRoot);
            File.WriteAllText(htmlPath, PrivateMarker, Encoding.UTF8);
            var fileSystem = new TransientDeleteDirectoryFailureFileSystem(
                captureRoot,
                failuresBeforeSuccess:
                    LocalChromiumBrowserProcessAdapter.RetryingWorkspaceCleaner.MaxCleanupDrainDelaySteps + 1);
            var cleaner = new LocalChromiumBrowserProcessAdapter.RetryingWorkspaceCleaner(
                fileSystem,
                retryDelays.Add);

            var cleanup = cleaner.CleanupFailedBrowserCaptureWorkspace(baseRoot, captureRoot);

            AssertSensitiveCleanupFailureDoesNotLeak(
                cleanup,
                baseRoot,
                captureRoot,
                htmlPath,
                PrivateMarker);
            Assert.Equal(
                LocalChromiumBrowserProcessAdapter.RetryingWorkspaceCleaner.MaxCleanupDrainDelaySteps + 1,
                fileSystem.DeleteDirectoryCallsFor(captureRoot));
            Assert.Equal(
                LocalChromiumBrowserProcessAdapter.RetryingWorkspaceCleaner.MaxCleanupDrainDelaySteps,
                retryDelays.Count);
            Assert.Equal(
                LocalChromiumBrowserProcessAdapter.RetryingWorkspaceCleaner.MaxCleanupDrainDelayMilliseconds,
                retryDelays.Sum(delay => delay.TotalMilliseconds));
            Assert.True(Directory.Exists(captureRoot), captureRoot);
        }
        finally
        {
            DeleteDirectoryIfExists(baseRoot);
        }
    }

    [Theory]
    [InlineData("failure", LocalChromiumBrowserProcessAdapter.ScreenshotFailedCode)]
    [InlineData("timeout", "BROWSER_SCREENSHOT_TIMEOUT")]
    [InlineData("cancel", "CANCELLED")]
    public async Task LocalChromiumProcessAdapter_CleanupFailureSupersedesBrowserResultWithoutLeakage(
        string mode,
        string originalCode)
    {
        var artifactRoot = NewQuotaTestRoot($"design-conversion-browser-cleanup-priority-{mode}");
        var privateMarker = "PRIVATE_CLEANUP_PRIORITY_" + Guid.NewGuid().ToString("N");
        using var cancellation = new CancellationTokenSource();

        try
        {
            var cleaner = new AlwaysFailingWorkspaceCleaner();
            var runner = new FakeLocalChromiumBrowserProcessRunner((request, cancellationToken) =>
            {
                WriteFakeBrowserProfileCache(request.UserDataPath, privateMarker);
                return mode switch
                {
                    "failure" => Task.FromResult(OperationResult<LocalChromiumBrowserProcessRunResult>.Fail(
                        $"Fake browser failure with {privateMarker}.",
                        LocalChromiumBrowserProcessAdapter.ScreenshotFailedCode)),
                    "timeout" => throw new OperationCanceledException($"Timeout carried {privateMarker}."),
                    "cancel" => CancelFromRunner(cancellation, cancellationToken),
                    _ => throw new InvalidOperationException("Unexpected cleanup-priority mode.")
                };
            });
            var adapter = CreateQuotaTestAdapter(
                artifactRoot,
                runner,
                maxRetainedCaptureBytes: LocalChromiumBrowserProcessAdapter.DefaultMaxRetainedCaptureBytes,
                workspaceCleaner: cleaner);

            var result = mode == "cancel"
                ? await adapter.CaptureScreenshotAsync(BrowserCaptureRequest($"cleanup-priority-{mode}"), cancellation.Token)
                : await adapter.CaptureScreenshotAsync(BrowserCaptureRequest($"cleanup-priority-{mode}"));

            Assert.False(result.IsSuccess);
            Assert.Equal(LocalChromiumBrowserProcessAdapter.SensitiveCleanupFailedCode, result.ErrorCode);
            Assert.NotEqual(originalCode, result.ErrorCode);
            Assert.Equal("Browser capture sensitive workspace cleanup failed.", result.ErrorMessage);
            Assert.Equal(1, cleaner.FailedWorkspaceCleanupCalls);
            AssertBrowserCaptureResultDoesNotLeakSecrets(
                result,
                artifactRoot,
                privateMarker,
                originalCode,
                "Fake browser failure",
                "Timeout carried");
        }
        finally
        {
            DeleteDirectoryIfExists(artifactRoot);
        }

        static Task<OperationResult<LocalChromiumBrowserProcessRunResult>> CancelFromRunner(
            CancellationTokenSource cancellation,
            CancellationToken cancellationToken)
        {
            cancellation.Cancel();
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(OperationResult<LocalChromiumBrowserProcessRunResult>.Ok(
                new LocalChromiumBrowserProcessRunResult(ElapsedMs: 0, Stdout: "", Stderr: "")));
        }
    }

    [Fact]
    public async Task LocalChromiumCaptureGate_DoesNotRunMoreThanConfiguredMaximumWorkBodiesConcurrently()
    {
        var gate = new LocalChromiumBrowserCaptureConcurrencyGate(
            LocalChromiumBrowserProcessAdapter.DefaultMaxConcurrentBrowserCaptureProcesses);
        Assert.Equal(2, gate.MaxConcurrency);
        var releaseWork = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var activeBodies = 0;
        var maxActiveBodies = 0;
        var enteredBodies = 0;

        Task<int> StartWorkAsync(int id)
            => gate.ExecuteAsync(
                async () =>
                {
                    var active = Interlocked.Increment(ref activeBodies);
                    TrackMax(ref maxActiveBodies, active);
                    Interlocked.Increment(ref enteredBodies);
                    try
                    {
                        await releaseWork.Task;
                        return id;
                    }
                    finally
                    {
                        Interlocked.Decrement(ref activeBodies);
                    }
                },
                () => -1);

        var tasks = Enumerable.Range(1, 4).Select(StartWorkAsync).ToArray();

        Assert.Equal(gate.MaxConcurrency, Volatile.Read(ref activeBodies));
        Assert.Equal(gate.MaxConcurrency, Volatile.Read(ref enteredBodies));
        Assert.Equal(gate.MaxConcurrency, Volatile.Read(ref maxActiveBodies));

        releaseWork.SetResult();
        var results = await Task.WhenAll(tasks);

        Assert.Equal([1, 2, 3, 4], results.Order().ToArray());
        Assert.Equal(4, Volatile.Read(ref enteredBodies));
        Assert.Equal(gate.MaxConcurrency, Volatile.Read(ref maxActiveBodies));

        static void TrackMax(ref int maximum, int value)
        {
            while (true)
            {
                var snapshot = Volatile.Read(ref maximum);
                if (value <= snapshot)
                    return;

                if (Interlocked.CompareExchange(ref maximum, value, snapshot) == snapshot)
                    return;
            }
        }
    }

    [Fact]
    public async Task LocalChromiumCaptureGate_CancelledQueuedOperationDoesNotEnterWorkBody()
    {
        var gate = new LocalChromiumBrowserCaptureConcurrencyGate(maxConcurrency: 1);
        var releaseOccupiedWork = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var occupiedBodyEntered = false;
        var occupied = gate.ExecuteAsync(
            async () =>
            {
                occupiedBodyEntered = true;
                await releaseOccupiedWork.Task;
                return "occupied";
            },
            () => "occupied-cancelled");
        Assert.True(occupiedBodyEntered);

        var queuedBodyEntered = false;
        using var queuedCancellation = new CancellationTokenSource();
        var queued = gate.ExecuteAsync(
            () =>
            {
                queuedBodyEntered = true;
                return Task.FromResult("queued");
            },
            () => "queued-cancelled",
            queuedCancellation.Token);
        Assert.False(queuedBodyEntered);
        Assert.False(queued.IsCompleted);

        queuedCancellation.Cancel();
        var queuedResult = await queued;

        Assert.Equal("queued-cancelled", queuedResult);
        Assert.False(queuedBodyEntered);

        releaseOccupiedWork.SetResult();
        Assert.Equal("occupied", await occupied);
    }

    [Fact]
    public async Task LocalChromiumCaptureGate_FailedOrThrowingWorkReleasesCapacityForLaterOperation()
    {
        var gate = new LocalChromiumBrowserCaptureConcurrencyGate(maxConcurrency: 1);
        var failed = await gate.ExecuteAsync(
            () => Task.FromResult(OperationResult<bool>.Fail("first body failed", "FIRST_BODY_FAILED")),
            () => OperationResult<bool>.Fail("queued body cancelled", "CANCELLED"));
        Assert.False(failed.IsSuccess);
        Assert.Equal("FIRST_BODY_FAILED", failed.ErrorCode);

        var afterFailureEntered = false;
        var afterFailure = await gate.ExecuteAsync(
            () =>
            {
                afterFailureEntered = true;
                return Task.FromResult("after-failure");
            },
            () => "cancelled");
        Assert.True(afterFailureEntered);
        Assert.Equal("after-failure", afterFailure);

        await Assert.ThrowsAsync<InvalidOperationException>(() => gate.ExecuteAsync(
            ThrowingWorkAsync,
            () => "cancelled"));

        var afterThrowEntered = false;
        var afterThrow = await gate.ExecuteAsync(
            () =>
            {
                afterThrowEntered = true;
                return Task.FromResult("after-throw");
            },
            () => "cancelled");
        Assert.True(afterThrowEntered);
        Assert.Equal("after-throw", afterThrow);

        static Task<string> ThrowingWorkAsync()
            => throw new InvalidOperationException("body boom");
    }

    [Fact]
    public void LocalChromiumProcessAdapter_SummarizesProcessTextWithoutLeakingRawStreams()
    {
        const string SecretPath = @"C:\outside-browser-artifacts\n13-process-secret.png";
        const string SecretUri = "file:///C:/outside-browser-artifacts/n13-process-secret.png?query=N13_QUERY_SECRET&token=N13_TOKEN_SECRET";
        const string StderrToken = "N13_STDERR_TOKEN_SECRET";
        const string StdoutToken = "N13_STDOUT_TOKEN_SECRET";
        var stderr = $"stderr path={SecretPath} uri={SecretUri} marker={StderrToken}";
        var stdout = $"stdout uri={SecretUri} path={SecretPath} marker={StdoutToken}";

        var method = typeof(LocalChromiumBrowserProcessAdapter).GetMethod(
            "SummarizeProcessText",
            BindingFlags.NonPublic | BindingFlags.Static,
            binder: null,
            types: [typeof(string), typeof(string)],
            modifiers: null);
        Assert.NotNull(method);

        var summary = Assert.IsType<string>(method.Invoke(null, [stderr, stdout]));
        var expected = string.Join(
            " ",
            "stderr_sha256=" + Sha256Text(stderr),
            "stderr_chars=" + stderr.Length.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "stdout_sha256=" + Sha256Text(stdout),
            "stdout_chars=" + stdout.Length.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Assert.Equal(expected, summary);

        foreach (var secret in new[]
        {
            stderr,
            stdout,
            SecretPath,
            SecretUri,
            "N13_QUERY_SECRET",
            "N13_TOKEN_SECRET",
            StderrToken,
            StdoutToken
        })
        {
            Assert.DoesNotContain(secret, summary, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void LocalChromiumProcessAdapter_SuccessCleanupRemovesSensitiveFilesAndPreservesScreenshot()
    {
        const string PrivateMarker = "PRIVATE_CAPTURE_HTML_MARKER_N6_6";
        const string PrivatePathSegment = "PRIVATE_CAPTURE_PATH_N6_6";
        var testRoot = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            "design-conversion-sensitive-browser-cleanup",
            Guid.NewGuid().ToString("N")));
        var captureRoot = Path.Combine(testRoot, Guid.NewGuid().ToString("N"));
        var htmlPath = Path.Combine(captureRoot, "index.html");
        var profilePath = Path.Combine(captureRoot, "profile");
        var screenshotPath = Path.Combine(captureRoot, "screenshot.png");
        var unrelatedSiblingPath = Path.Combine(captureRoot, "unrelated-sibling.txt");
        var nestedProfilePath = Path.Combine(profilePath, "Default", "Cache", "private-cache-entry");

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(nestedProfilePath)!);
            File.WriteAllText(htmlPath, $"<main data-private=\"{PrivateMarker}\">secret</main>", Encoding.UTF8);
            File.WriteAllText(nestedProfilePath, PrivateMarker, Encoding.UTF8);
            File.WriteAllBytes(screenshotPath, SolidRgbaPng(4, 4, 32, 96, 160, 255));
            File.WriteAllText(unrelatedSiblingPath, "keep", Encoding.UTF8);

            var cleanup = InvokeLocalChromiumSensitiveCleanup(captureRoot, htmlPath, profilePath);

            Assert.True(cleanup.IsSuccess, cleanup.ErrorMessage);
            Assert.False(File.Exists(htmlPath), htmlPath);
            Assert.False(Directory.Exists(profilePath), profilePath);
            Assert.True(File.Exists(screenshotPath), screenshotPath);
            Assert.True(File.Exists(unrelatedSiblingPath), unrelatedSiblingPath);

            var failureRoot = Path.Combine(testRoot, Guid.NewGuid().ToString("N"));
            var failureHtmlPath = Path.Combine(failureRoot, "index.html");
            var failureProfilePath = Path.Combine(failureRoot, "profile");
            var privateOutsideProfilePath = Path.Combine(testRoot, PrivatePathSegment, "profile");
            Directory.CreateDirectory(failureProfilePath);
            Directory.CreateDirectory(privateOutsideProfilePath);
            File.WriteAllText(failureHtmlPath, PrivateMarker, Encoding.UTF8);

            var failure = InvokeLocalChromiumSensitiveCleanup(
                failureRoot,
                failureHtmlPath,
                privateOutsideProfilePath);

            Assert.False(failure.IsSuccess);
            Assert.Equal("BROWSER_CAPTURE_SENSITIVE_CLEANUP_FAILED", failure.ErrorCode);
            AssertSensitiveCleanupFailureDoesNotLeak(
                failure,
                testRoot,
                failureHtmlPath,
                privateOutsideProfilePath,
                PrivateMarker,
                PrivatePathSegment);
            Assert.True(File.Exists(failureHtmlPath), failureHtmlPath);
            Assert.True(Directory.Exists(failureProfilePath), failureProfilePath);
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }

    [Fact]
    public void LocalChromiumProcessAdapter_FailureCleanupRemovesSafeDirectGuidWorkspace()
    {
        var baseRoot = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            "design-conversion-failed-browser-cleanup",
            Guid.NewGuid().ToString("N")));
        var captureRoot = Path.Combine(baseRoot, Guid.NewGuid().ToString("N"));
        var htmlPath = Path.Combine(captureRoot, "index.html");
        var profilePath = Path.Combine(captureRoot, "profile");
        var nestedProfilePath = Path.Combine(profilePath, "Default", "Cache", "entry");
        var screenshotPath = Path.Combine(captureRoot, "screenshot.png");
        var unexpectedSiblingPath = Path.Combine(captureRoot, "unexpected-sibling.bin");

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(nestedProfilePath)!);
            File.WriteAllText(htmlPath, "<main>partial private html</main>", Encoding.UTF8);
            File.WriteAllText(nestedProfilePath, "profile-cache", Encoding.UTF8);
            File.WriteAllBytes(screenshotPath, SolidRgbaPng(4, 4, 96, 48, 24, 255));
            File.WriteAllText(unexpectedSiblingPath, "unexpected", Encoding.UTF8);

            var cleanup = InvokeLocalChromiumFailedWorkspaceCleanup(baseRoot, captureRoot);

            Assert.True(cleanup.IsSuccess, cleanup.ErrorMessage);
            Assert.False(Directory.Exists(captureRoot), captureRoot);
            Assert.False(File.Exists(htmlPath), htmlPath);
            Assert.False(File.Exists(screenshotPath), screenshotPath);
            Assert.False(File.Exists(unexpectedSiblingPath), unexpectedSiblingPath);
        }
        finally
        {
            if (Directory.Exists(baseRoot))
                Directory.Delete(baseRoot, recursive: true);
        }
    }

    [Fact]
    public void LocalChromiumProcessAdapter_FailureCleanupRefusesOutsideAndNonGuidRootsWithoutDeletingSentinels()
    {
        const string SecretMarker = "N25_7_CLEANUP_SECRET_MARKER";
        var testRoot = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            "design-conversion-failed-browser-cleanup-refusal",
            Guid.NewGuid().ToString("N")));
        var baseRoot = Path.Combine(testRoot, "base");
        var outsideRoot = Path.Combine(testRoot, "outside", Guid.NewGuid().ToString("N"));
        var outsideSentinel = Path.Combine(outsideRoot, "sentinel.txt");
        var nonGuidRoot = Path.Combine(baseRoot, "not-a-guid-capture-root");
        var nonGuidSentinel = Path.Combine(nonGuidRoot, "sentinel.txt");

        try
        {
            Directory.CreateDirectory(outsideRoot);
            Directory.CreateDirectory(nonGuidRoot);
            File.WriteAllText(outsideSentinel, SecretMarker, Encoding.UTF8);
            File.WriteAllText(nonGuidSentinel, SecretMarker, Encoding.UTF8);

            var outside = InvokeLocalChromiumFailedWorkspaceCleanup(baseRoot, outsideRoot);
            var nonGuid = InvokeLocalChromiumFailedWorkspaceCleanup(baseRoot, nonGuidRoot);

            AssertSensitiveCleanupFailureDoesNotLeak(
                outside,
                testRoot,
                baseRoot,
                outsideRoot,
                outsideSentinel,
                SecretMarker);
            AssertSensitiveCleanupFailureDoesNotLeak(
                nonGuid,
                testRoot,
                baseRoot,
                nonGuidRoot,
                nonGuidSentinel,
                SecretMarker);
            Assert.True(File.Exists(outsideSentinel), outsideSentinel);
            Assert.True(File.Exists(nonGuidSentinel), nonGuidSentinel);
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }

    [Fact]
    public void LocalChromiumProcessAdapter_FailureCleanupRefusesReparseRootWithoutDeletingTargetWhenSupported()
    {
        const string SecretMarker = "N25_7_REPARSE_TARGET_SECRET";
        var testRoot = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            "design-conversion-failed-browser-cleanup-reparse",
            Guid.NewGuid().ToString("N")));
        var baseRoot = Path.Combine(testRoot, "base");
        var targetRoot = Path.Combine(testRoot, "target-outside-base");
        var linkRoot = Path.Combine(baseRoot, Guid.NewGuid().ToString("N"));
        var targetSentinel = Path.Combine(targetRoot, "sentinel.txt");
        var linkCreated = false;

        try
        {
            Directory.CreateDirectory(baseRoot);
            Directory.CreateDirectory(targetRoot);
            File.WriteAllText(targetSentinel, SecretMarker, Encoding.UTF8);

            try
            {
                Directory.CreateSymbolicLink(linkRoot, targetRoot);
                linkCreated = true;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or PlatformNotSupportedException)
            {
                return;
            }

            var cleanup = InvokeLocalChromiumFailedWorkspaceCleanup(baseRoot, linkRoot);

            AssertSensitiveCleanupFailureDoesNotLeak(
                cleanup,
                testRoot,
                baseRoot,
                targetRoot,
                linkRoot,
                targetSentinel,
                SecretMarker);
            Assert.True(File.Exists(targetSentinel), targetSentinel);
            Assert.True(Directory.Exists(targetRoot), targetRoot);
        }
        finally
        {
            if (linkCreated && Directory.Exists(linkRoot))
                Directory.Delete(linkRoot);
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }

    [Fact]
    public void LocalChromiumProcessAdapter_BuildStartInfoUsesFailClosedNetworkIsolationArguments()
    {
        var htmlPath = Path.Combine(Path.GetTempPath(), "design-conversion-launch-isolation", "index.html");
        var screenshotPath = Path.Combine(Path.GetTempPath(), "design-conversion-launch-isolation", "screenshot.png");
        var userDataPath = Path.Combine(Path.GetTempPath(), "design-conversion-launch-isolation", "profile");

        var startInfo = InvokeLocalChromiumBuildStartInfo(
            executablePath: @"C:\browser\chrome.exe",
            htmlPath: htmlPath,
            screenshotPath: screenshotPath,
            userDataPath: userDataPath,
            resolution: new RenderQaResolution("launch-isolation", 640, 480, 1.0));

        var args = startInfo.ArgumentList.ToArray();
        Assert.Contains("--host-resolver-rules=MAP * ~NOTFOUND", args);

        var resolverRules = args
            .Where(arg => arg.StartsWith("--host-resolver-rules=", StringComparison.Ordinal))
            .ToArray();
        var resolverRule = Assert.Single(resolverRules);
        Assert.DoesNotContain("EXCLUDE", resolverRule, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("localhost", resolverRule, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("127.0.0.1", resolverRule, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("::1", resolverRule, StringComparison.OrdinalIgnoreCase);

        Assert.DoesNotContain("--disable-web-security", args);
        Assert.Contains("--headless=new", args);
        Assert.Contains("--disable-gpu", args);
        Assert.Contains("--disable-dev-shm-usage", args);
        Assert.Contains("--disable-background-networking", args);
        Assert.Contains("--disable-component-update", args);
        Assert.Contains("--disable-extensions", args);
        Assert.Contains("--disable-sync", args);
        Assert.Contains("--metrics-recording-only", args);
        Assert.Contains("--no-pings", args);
        Assert.Contains("--safebrowsing-disable-auto-update", args);
        Assert.Contains("--disable-features=Translate,OptimizationHints,MediaRouter", args);
        Assert.Contains("--hide-scrollbars", args);
        Assert.Contains($"--window-size=640,480", args);
        Assert.Contains("--force-device-scale-factor=1", args);
        Assert.Contains($"--screenshot={screenshotPath}", args);
        Assert.Contains(new Uri(htmlPath).AbsoluteUri, args);
    }

    [Fact]
    public async Task LocalChromiumProcessAdapter_CapturesRealScreenshotWhenRuntimeIsAvailableOrReportsBlocker()
    {
        var executable = LocalChromiumBrowserProcessAdapter.TryResolveDefaultExecutablePath();
        if (string.IsNullOrWhiteSpace(executable))
        {
            var missing = await new LocalChromiumBrowserProcessAdapter(
                    executablePath: @"C:\definitely-not-installed\missing-browser.exe",
                    timeout: TimeSpan.FromSeconds(1))
                .CaptureScreenshotAsync(new DesignConversionBrowserCaptureRequest(
                    CaptureRequestId: "capture-runtime-blocker",
                    RequestId: "request-001",
                    DocumentHtml: "<!doctype html><html><body>hello</body></html>",
                    Resolution: new RenderQaResolution("desktop", 800, 600, 1.0),
                    Metadata: new Dictionary<string, string>()));

            Assert.False(missing.IsSuccess);
            Assert.Equal("BROWSER_RUNTIME_UNAVAILABLE", missing.ErrorCode);
            return;
        }

        var adapter = new LocalChromiumBrowserProcessAdapter(executable, TimeSpan.FromSeconds(20));
        var result = await adapter.CaptureScreenshotAsync(new DesignConversionBrowserCaptureRequest(
            CaptureRequestId: "capture-real-browser",
            RequestId: "request-001",
            DocumentHtml: "<!doctype html><html><head><style>body{margin:0}.card{width:200px;height:120px;background:#0a7;color:white}</style></head><body><main class=\"card\"><h1>Hello</h1></main></body></html>",
            Resolution: new RenderQaResolution("desktop", 800, 600, 1.0),
            Metadata: new Dictionary<string, string>()));

        if (!result.IsSuccess)
        {
            AssertBrowserRuntimeUnavailableOrSlowBlocker(result);
            return;
        }

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.StartsWith("file:///", result.Value!.ScreenshotUri, StringComparison.Ordinal);
        Assert.Equal(64, result.Value.ScreenshotSha256.Length);
        Assert.Equal("local_chromium_process", result.Value.Metadata["browser_adapter_kind"]);
        Assert.Equal(executable, result.Value.Metadata["browser_executable"]);
        Assert.Equal(bool.TrueString, result.Value.Metadata["browser_screenshot_png_retained"]);
        Assert.Equal(bool.TrueString, result.Value.Metadata["browser_sensitive_workspace_removed"]);

        var png = DesignConversionBrowserPngEvidenceInspector.Inspect(result.Value.ScreenshotUri);
        Assert.True(png.IsSuccess, png.ErrorMessage);
        Assert.True(File.Exists(png.Value!.LocalPath), png.Value.LocalPath);
        Assert.Equal(800, png.Value.PngWidth);
        Assert.Equal(600, png.Value.PngHeight);
        Assert.True(png.Value.ByteLength > 1000, png.Value.LocalPath);
        Assert.True(png.Value.HasNonBlankPixels, png.Value.LocalPath);
        Assert.True(png.Value.NonWhitePixelCount > 0, png.Value.LocalPath);
        Assert.True(png.Value.DistinctColorCount > 1, png.Value.LocalPath);
        Assert.Equal(
            result.Value.ScreenshotSha256,
            Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(png.Value.LocalPath))).ToLowerInvariant());
    }

    [Fact]
    public async Task LocalChromiumProcessAdapter_BlocksLoopbackSubresourceConnectionsWhenRuntimeIsAvailableOrReportsBlocker()
    {
        var executable = LocalChromiumBrowserProcessAdapter.TryResolveDefaultExecutablePath();
        if (string.IsNullOrWhiteSpace(executable))
        {
            var missing = await new LocalChromiumBrowserProcessAdapter(
                    executablePath: @"C:\definitely-not-installed\missing-browser.exe",
                    timeout: TimeSpan.FromSeconds(1))
                .CaptureScreenshotAsync(new DesignConversionBrowserCaptureRequest(
                    CaptureRequestId: "capture-loopback-runtime-blocker",
                    RequestId: "request-loopback-blocker",
                    DocumentHtml: "<!doctype html><html><body>browser unavailable</body></html>",
                    Resolution: new RenderQaResolution("loopback-blocker", 320, 240, 1.0),
                    Metadata: new Dictionary<string, string>()));

            Assert.False(missing.IsSuccess);
            Assert.Equal("BROWSER_RUNTIME_UNAVAILABLE", missing.ErrorCode);
            return;
        }

        using var listener = new TcpListener(IPAddress.Loopback, port: 0);
        listener.Start(backlog: 8);
        using var listenerTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(6));
        var connectionTask = AcceptLoopbackConnectionAsync(listener, listenerTimeout.Token);
        var endpoint = (IPEndPoint)listener.LocalEndpoint;
        var artifactRoot = Path.GetFullPath(Path.Combine(
            "TestResults",
            "codex-loopback-browser-isolation",
            Guid.NewGuid().ToString("N")));

        try
        {
            var adapter = new LocalChromiumBrowserProcessAdapter(
                executable,
                TimeSpan.FromSeconds(20),
                artifactRoot);
            var result = await adapter.CaptureScreenshotAsync(new DesignConversionBrowserCaptureRequest(
                CaptureRequestId: "capture-loopback-isolation",
                RequestId: "request-loopback-isolation",
                DocumentHtml: $$"""
                    <!doctype html>
                    <html>
                    <head>
                      <meta charset="utf-8">
                      <link rel="preload" as="image" href="http://127.0.0.1:{{endpoint.Port}}/preload-loopback-probe.png">
                      <style>
                        body{margin:0;background:#f8fafc;color:#102033;font-family:Arial,sans-serif}
                        main{min-height:240px;display:grid;place-items:center;background-image:url("http://127.0.0.1:{{endpoint.Port}}/css-loopback-probe.png")}
                        h1{font-size:28px}
                      </style>
                      <script>
                        window.addEventListener('load', function() {
                          var image = new Image();
                          image.src = 'http://127.0.0.1:{{endpoint.Port}}/script-loopback-probe.png';
                          document.body.appendChild(image);
                        });
                      </script>
                    </head>
                    <body>
                      <img src="http://127.0.0.1:{{endpoint.Port}}/image-loopback-probe.png" alt="" width="32" height="32">
                      <main><h1>Loopback isolation proof</h1></main>
                    </body>
                    </html>
                    """,
                Resolution: new RenderQaResolution("loopback-isolation", 320, 240, 1.0),
                Metadata: new Dictionary<string, string>()));

            var connectionReachedListener = await WaitForLoopbackConnectionProbeAsync(connectionTask, TimeSpan.FromMilliseconds(750));
            Assert.False(connectionReachedListener, "Chromium loaded a loopback img or CSS url subresource.");
            if (!result.IsSuccess)
            {
                Assert.Contains(
                    result.ErrorCode,
                    new[]
                    {
                        LocalChromiumBrowserProcessAdapter.RuntimeUnavailableCode,
                        LocalChromiumBrowserProcessAdapter.RuntimeCrashedBeforeScreenshotCode
                    });
                Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
                Assert.DoesNotContain(endpoint.Port.ToString(), result.ErrorMessage, StringComparison.Ordinal);
                return;
            }

            var png = DesignConversionBrowserPngEvidenceInspector.Inspect(result.Value!.ScreenshotUri);
            Assert.True(png.IsSuccess, png.ErrorMessage);
            Assert.True(File.Exists(png.Value!.LocalPath), png.Value.LocalPath);
            Assert.Equal(320, png.Value.PngWidth);
            Assert.Equal(240, png.Value.PngHeight);
            Assert.True(png.Value.ByteLength > 1000, png.Value.LocalPath);
            Assert.True(png.Value.HasNonBlankPixels, png.Value.LocalPath);
            Assert.Equal(
                result.Value.ScreenshotSha256,
                Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(png.Value.LocalPath))).ToLowerInvariant());
        }
        finally
        {
            listenerTimeout.Cancel();
            listener.Stop();
            try
            {
                await connectionTask;
            }
            catch (Exception ex) when (ex is OperationCanceledException or SocketException or ObjectDisposedException)
            {
            }
        }
    }

    [Fact]
    public async Task LocalChromiumProcessAdapter_BlocksFileSubresourceReadsWhenRuntimeIsAvailableOrReportsBlocker()
    {
        var executable = LocalChromiumBrowserProcessAdapter.TryResolveDefaultExecutablePath();
        if (string.IsNullOrWhiteSpace(executable))
        {
            var missing = await new LocalChromiumBrowserProcessAdapter(
                    executablePath: @"C:\definitely-not-installed\missing-browser.exe",
                    timeout: TimeSpan.FromSeconds(1))
                .CaptureScreenshotAsync(new DesignConversionBrowserCaptureRequest(
                    CaptureRequestId: "capture-file-subresource-runtime-blocker",
                    RequestId: "request-file-subresource-blocker",
                    DocumentHtml: "<!doctype html><html><body>browser unavailable</body></html>",
                    Resolution: new RenderQaResolution("file-subresource-blocker", 320, 240, 1.0),
                    Metadata: new Dictionary<string, string>()));

            Assert.False(missing.IsSuccess);
            Assert.Equal("BROWSER_RUNTIME_UNAVAILABLE", missing.ErrorCode);
            return;
        }

        var testRoot = Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            "design-conversion-file-subresource-lfi",
            Guid.NewGuid().ToString("N"));
        var artifactRoot = Path.Combine(testRoot, "browser-artifacts");
        var secretRoot = Path.Combine(testRoot, "secret-outside-artifacts");
        var secretPath = Path.Combine(secretRoot, "n13-secret-file-subresource.png");
        var secretUri = new Uri(secretPath).AbsoluteUri;
        const string SecretMarker = "N13_FILE_SUBRESOURCE_SECRET_MARKER";

        try
        {
            Directory.CreateDirectory(artifactRoot);
            Directory.CreateDirectory(secretRoot);
            File.WriteAllBytes(secretPath, SolidRgbaPng(64, 64, 12, 52, 210, 255));
            Assert.False(
                Path.GetFullPath(secretPath).StartsWith(Path.GetFullPath(artifactRoot), StringComparison.OrdinalIgnoreCase),
                "The secret PNG must live outside the browser artifact root.");

            var adapter = new LocalChromiumBrowserProcessAdapter(
                executable,
                TimeSpan.FromSeconds(20),
                artifactRoot);
            var documentHtml = $$"""
                <!doctype html>
                <html>
                <head>
                  <meta charset="utf-8">
                  <meta http-equiv="Content-Security-Policy" content="default-src * file: http: https: data: blob: 'unsafe-inline'; img-src file: data: blob:;">
                  <style>
                    html,body{margin:0;width:100%;height:100%;background:#f8fafc}
                    main{width:360px;height:260px;display:grid;place-items:center;background:#f8fafc}
                    .slot{width:320px;height:220px;background:#fde68a}
                    .slot img{display:block;width:320px;height:220px;object-fit:fill;background:#fde68a}
                  </style>
                </head>
                <body>
                  <main data-secret-marker="{{SecretMarker}}">
                    <div class="slot">
                      <img src="{{secretUri}}" width="320" height="220" alt="">
                    </div>
                  </main>
                </body>
                </html>
                """;
            AssertStrictOfflineCspPrecedesCallerContent(
                InvokeLocalChromiumBuildStrictOfflineCaptureDocument(documentHtml),
                SecretMarker);

            var withSecret = await adapter.CaptureScreenshotAsync(new DesignConversionBrowserCaptureRequest(
                CaptureRequestId: "capture-file-subresource-secret-present",
                RequestId: "request-file-subresource-secret-present",
                DocumentHtml: documentHtml,
                Resolution: new RenderQaResolution("file-subresource-present", 360, 260, 1.0),
                Metadata: new Dictionary<string, string>()));

            File.Delete(secretPath);

            var withoutSecret = await adapter.CaptureScreenshotAsync(new DesignConversionBrowserCaptureRequest(
                CaptureRequestId: "capture-file-subresource-secret-deleted",
                RequestId: "request-file-subresource-secret-deleted",
                DocumentHtml: documentHtml,
                Resolution: new RenderQaResolution("file-subresource-deleted", 360, 260, 1.0),
                Metadata: new Dictionary<string, string>()));

            if (!withSecret.IsSuccess)
            {
                AssertBrowserRuntimeUnavailableOrSlowBlocker(withSecret);
                return;
            }

            if (!withoutSecret.IsSuccess)
            {
                AssertBrowserRuntimeUnavailableOrSlowBlocker(withoutSecret);
                return;
            }

            AssertBrowserCaptureResultDoesNotLeakSecrets(withSecret, secretPath, secretUri, SecretMarker);
            AssertBrowserCaptureResultDoesNotLeakSecrets(withoutSecret, secretPath, secretUri, SecretMarker);

            var withSecretPng = DesignConversionBrowserPngEvidenceInspector.Inspect(withSecret.Value!.ScreenshotUri);
            var withoutSecretPng = DesignConversionBrowserPngEvidenceInspector.Inspect(withoutSecret.Value!.ScreenshotUri);
            Assert.True(withSecretPng.IsSuccess, withSecretPng.ErrorMessage);
            Assert.True(withoutSecretPng.IsSuccess, withoutSecretPng.ErrorMessage);
            Assert.Equal(360, withSecretPng.Value!.PngWidth);
            Assert.Equal(260, withSecretPng.Value.PngHeight);
            Assert.Equal(360, withoutSecretPng.Value!.PngWidth);
            Assert.Equal(260, withoutSecretPng.Value.PngHeight);
            Assert.True(withSecretPng.Value.HasNonBlankPixels, withSecretPng.Value.LocalPath);
            Assert.True(withoutSecretPng.Value.HasNonBlankPixels, withoutSecretPng.Value.LocalPath);

            Assert.True(
                string.Equals(withSecret.Value.ScreenshotSha256, withoutSecret.Value.ScreenshotSha256, StringComparison.Ordinal),
                $"File subresource LFI regression: screenshots differ when only the external secret PNG existence changed. present={withSecret.Value.ScreenshotSha256} deleted={withoutSecret.Value.ScreenshotSha256}");

            var persistedDocuments = Directory
                .EnumerateFiles(artifactRoot, "index.html", SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            var persistedProfiles = Directory
                .EnumerateDirectories(artifactRoot, "profile", SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            var persistedScreenshots = Directory
                .EnumerateFiles(artifactRoot, "screenshot.png", SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.Ordinal)
                .Select(Path.GetFullPath)
                .ToArray();
            Assert.Empty(persistedDocuments);
            Assert.Empty(persistedProfiles);
            Assert.Equal(2, persistedScreenshots.Length);
            Assert.Contains(persistedScreenshots, path => string.Equals(path, Path.GetFullPath(withSecretPng.Value.LocalPath), StringComparison.OrdinalIgnoreCase));
            Assert.Contains(persistedScreenshots, path => string.Equals(path, Path.GetFullPath(withoutSecretPng.Value.LocalPath), StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            if (File.Exists(secretPath))
                File.Delete(secretPath);
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }

    [Fact]
    public async Task BrowserRenderManifest_CapturesThreeRealNonBlankPngsWhenRuntimeIsAvailableOrReportsBlocker()
    {
        var executable = LocalChromiumBrowserProcessAdapter.TryResolveDefaultExecutablePath();
        if (string.IsNullOrWhiteSpace(executable))
        {
            var missing = await new LocalChromiumBrowserProcessAdapter(
                    executablePath: @"C:\definitely-not-installed\missing-browser.exe",
                    timeout: TimeSpan.FromSeconds(1))
                .CaptureScreenshotAsync(new DesignConversionBrowserCaptureRequest(
                    CaptureRequestId: "capture-runtime-blocker",
                    RequestId: "request-001",
                    DocumentHtml: "<!doctype html><html><body>hello</body></html>",
                    Resolution: new RenderQaResolution("desktop", 800, 600, 1.0),
                    Metadata: new Dictionary<string, string>()));

            Assert.False(missing.IsSuccess);
            Assert.Equal("BROWSER_RUNTIME_UNAVAILABLE", missing.ErrorCode);
            return;
        }

        var artifactRoot = Path.GetFullPath(Path.Combine(
            "TestResults",
            "codex-pass78-browser-render-qa",
            Guid.NewGuid().ToString("N")));
        var browserTimeout = TimeSpan.FromSeconds(3);
        var adapter = new LocalChromiumBrowserProcessAdapter(
            executable,
            browserTimeout,
            artifactRoot);
        var provider = new DesignConversionBrowserRenderManifestProvider(
            new BrowserBackedDesignConversionRenderProvider(adapter));
        var viewports = new[]
        {
            new RenderQaResolution("mobile", 360, 740, 1.0),
            new RenderQaResolution("tablet", 768, 1024, 1.0),
            new RenderQaResolution("desktop", 1440, 900, 1.0)
        };

        var manifest = await provider.RenderManifestAsync(new DesignConversionBrowserRenderManifestRequest(
            RenderRunId: "codex-pass78-browser-render-qa",
            RequestId: "request-001",
            Html: """
                <main class="shell">
                  <section class="hero">
                    <p class="eyebrow">browser_render_qa</p>
                    <h1>Responsive render proof</h1>
                    <p>HTML and CSS are loaded by a local Chromium-family browser.</p>
                  </section>
                  <section class="grid">
                    <article>mobile</article>
                    <article>tablet</article>
                    <article>desktop</article>
                  </section>
                </main>
                """,
            Css: """
                html,body{margin:0;min-height:100%;font-family:Arial,sans-serif;background:#f8fafc;color:#102033}
                .shell{min-height:100vh;display:grid;gap:24px;align-content:center;padding:32px;background:linear-gradient(135deg,#f8fafc 0%,#d8f3dc 45%,#ffd166 100%)}
                .hero{border:4px solid #118ab2;background:#ffffff;box-shadow:0 16px 40px rgba(16,32,51,.18);padding:28px}
                .eyebrow{margin:0 0 8px;color:#9b2226;font-weight:700;text-transform:uppercase}
                h1{margin:0;font-size:42px}
                .grid{display:grid;grid-template-columns:repeat(3,1fr);gap:12px}
                article{min-height:86px;display:grid;place-items:center;background:#073b4c;color:white;font-weight:700}
                @media (max-width: 700px){.shell{padding:18px}.grid{grid-template-columns:1fr}h1{font-size:30px}}
                """,
            RequiredResolutions: viewports,
            Metadata: new Dictionary<string, string>
            {
                ["pass"] = "codex-pass78",
                ["visual_ai_not_executed"] = bool.TrueString,
                ["qa_data_role"] = "browser_render_evidence_only"
            }));

        if (!manifest.IsSuccess)
        {
            AssertBrowserRuntimeUnavailableOrSlowBlocker(manifest);
            return;
        }

        Assert.Equal(3, manifest.Value!.Entries.Count);
        Assert.Contains(DesignConversionBrowserRenderManifestProvider.BrowserRenderQaMarker, manifest.Value.EvidenceRefs);
        Assert.Contains(DesignConversionBrowserRenderManifestProvider.MultiResolutionRenderMarker, manifest.Value.EvidenceRefs);
        Assert.Contains(DesignConversionBrowserRenderManifestProvider.PngArtifactsMarker, manifest.Value.EvidenceRefs);
        Assert.Contains(DesignConversionBrowserRenderManifestProvider.NonBlankPixelsMarker, manifest.Value.EvidenceRefs);
        Assert.All(manifest.Value.Entries, entry =>
        {
            Assert.True(File.Exists(entry.LocalPath), entry.LocalPath);
            Assert.Equal(entry.RequestedWidth, entry.PngWidth);
            Assert.Equal(entry.RequestedHeight, entry.PngHeight);
            Assert.StartsWith("sha256:", entry.Sha256, StringComparison.Ordinal);
            Assert.Equal(71, entry.Sha256.Length);
            Assert.True(entry.ByteLength > 1000, entry.LocalPath);
            Assert.True(entry.HasNonBlankPixels, entry.LocalPath);
            Assert.True(entry.NonWhitePixelCount > 0, entry.LocalPath);
            Assert.True(entry.DistinctColorCount > 1, entry.LocalPath);
            Assert.Equal("browser_process_render_provider", entry.Metadata["provider_kind"]);
            Assert.Equal(bool.TrueString, entry.Metadata["browser_or_playwright_used"]);
            Assert.Equal(bool.FalseString, entry.Metadata["external_ai_used"]);
        });

        var manifestPath = Path.Combine(artifactRoot, "codex-pass78-browser-render-manifest.json");
        var manifestJson = JsonSerializer.Serialize(
            manifest.Value,
            new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(manifestPath, manifestJson, Encoding.UTF8);
        Assert.True(File.Exists(manifestPath), manifestPath);
    }

    [Fact]
    public async Task DeterministicLocalPngRenderer_CreatesFourViewportArtifactsWithStableHashesAndSafeMetadata()
    {
        var artifactRoot = Path.GetFullPath(Path.Combine(
            "TestResults",
            "codex-pass159-local-render-qa",
            Guid.NewGuid().ToString("N")));
        var renderer = new DeterministicLocalDesignConversionPngRenderProvider(artifactRoot);
        var provider = new DesignConversionBrowserRenderManifestProvider(renderer);
        var viewports = Pass159Viewports();

        var first = await provider.RenderManifestAsync(new DesignConversionBrowserRenderManifestRequest(
            RenderRunId: "codex-pass159-run-a",
            RequestId: "pass159-request",
            Html: """
                <main class="private-screen" data-secret="PRIVATE_PASS159_SECRET">
                  <h1>Design QA</h1>
                  <p>Generated HTML body.</p>
                </main>
                """,
            Css: """
                .private-screen{min-height:100vh;display:grid;place-items:center;background:url('C:/Users/private-user/private.png')}
                h1{color:#123456}
                """,
            RequiredResolutions: viewports,
            Metadata: new Dictionary<string, string>
            {
                ["pass"] = "codex-pass159",
                ["qa_data_role"] = "deterministic_local_png_artifact_evidence_only",
                ["source_manifest_ref"] = "file:///C:/Users/private-user/private-pass159.fig"
            }));
        var second = await provider.RenderManifestAsync(new DesignConversionBrowserRenderManifestRequest(
            RenderRunId: "codex-pass159-run-b",
            RequestId: "pass159-request",
            Html: """
                <main class="private-screen" data-secret="PRIVATE_PASS159_SECRET">
                  <h1>Design QA</h1>
                  <p>Generated HTML body.</p>
                </main>
                """,
            Css: """
                .private-screen{min-height:100vh;display:grid;place-items:center;background:url('C:/Users/private-user/private.png')}
                h1{color:#123456}
                """,
            RequiredResolutions: viewports,
            Metadata: new Dictionary<string, string>
            {
                ["pass"] = "codex-pass159",
                ["qa_data_role"] = "deterministic_local_png_artifact_evidence_only",
                ["source_manifest_ref"] = "file:///C:/Users/private-user/private-pass159.fig"
            }));

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(4, first.Value!.Entries.Count);
        Assert.Equal(4, second.Value!.Entries.Count);
        Assert.Contains("browser_or_playwright_used:false", first.Value.EvidenceRefs);
        Assert.Contains("external_ai_used:false", first.Value.EvidenceRefs);
        Assert.Contains("BROWSER_RENDER_NOT_EXECUTED_LOCAL_ARTIFACT_RENDERER", first.Value.Warnings);
        Assert.Equal(
            first.Value.Entries.Select(entry => entry.Sha256).ToArray(),
            second.Value.Entries.Select(entry => entry.Sha256).ToArray());
        Assert.All(first.Value.Entries, entry =>
        {
            Assert.True(File.Exists(entry.LocalPath), entry.LocalPath);
            Assert.Equal(entry.RequestedWidth, entry.PngWidth);
            Assert.Equal(entry.RequestedHeight, entry.PngHeight);
            Assert.Equal(Sha256File(entry.LocalPath), entry.Sha256);
            Assert.StartsWith("file:///", entry.ScreenshotUri, StringComparison.Ordinal);
            Assert.StartsWith(artifactRoot, entry.LocalPath, StringComparison.OrdinalIgnoreCase);
            Assert.True(entry.ByteLength > 1000, entry.LocalPath);
            Assert.True(entry.HasNonBlankPixels, entry.LocalPath);
            Assert.True(entry.NonWhitePixelCount > 0, entry.LocalPath);
            Assert.True(entry.DistinctColorCount > 1, entry.LocalPath);
            Assert.Equal(DeterministicLocalDesignConversionPngRenderProvider.ProviderKind, entry.Metadata["provider_kind"]);
            Assert.Equal(bool.TrueString, entry.Metadata["actual_png_bytes_written"]);
            Assert.Equal(bool.FalseString, entry.Metadata["visual_fidelity_claimed"]);
            Assert.Equal(bool.FalseString, entry.Metadata["browser_or_playwright_used"]);
            Assert.Equal(bool.FalseString, entry.Metadata["external_ai_used"]);
            Assert.True(int.Parse(entry.Metadata["input_metadata_redaction_count"]) > 0);
            Assert.DoesNotContain("C:/Users/", entry.Metadata["source_manifest_ref"], StringComparison.OrdinalIgnoreCase);
            Assert.StartsWith("local-png://design-conversion/", entry.Metadata["artifact_path_ref"], StringComparison.Ordinal);
        });

        var serialized = JsonSerializer.Serialize(first.Value);
        Assert.DoesNotContain("PRIVATE_PASS159_SECRET", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private-screen", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("background:url", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BrowserRenderManifest_UsesReceiptSnapshotForRemoteScreenshotUriWithoutSecondInspect()
    {
        var baseline = RemoteEvidenceTempFileCount();
        var artifactRoot = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            "receipt-backed-browser-manifest",
            Guid.NewGuid().ToString("N")));
        try
        {
            var remoteUri = "https://127.0.0.1/receipt-backed-browser-screenshot.png";
            var renderer = new ReceiptBackedRemoteUriRenderProvider(artifactRoot, remoteUri);
            var provider = new DesignConversionBrowserRenderManifestProvider(renderer);
            var viewport = new RenderQaResolution("receipt-backed-desktop", 64, 32, 1.0);

            var manifest = await provider.RenderManifestAsync(new DesignConversionBrowserRenderManifestRequest(
                RenderRunId: "receipt-backed-browser-manifest",
                RequestId: "receipt-backed-request",
                Html: "<main>receipt-backed browser proof</main>",
                Css: "main{min-height:100vh;background:#0f766e;color:#fff}",
                RequiredResolutions: [viewport],
                Metadata: new Dictionary<string, string>
                {
                    ["qa_data_role"] = "receipt_backed_manifest_evidence_only"
                }));

            Assert.True(manifest.IsSuccess, manifest.ErrorMessage);
            var entry = Assert.Single(manifest.Value!.Entries);
            var receipt = renderer.LastReceipt;
            Assert.NotNull(receipt);
            Assert.Equal(remoteUri, entry.ScreenshotUri);
            Assert.Equal(renderer.SnapshotPath, entry.LocalPath);
            Assert.True(File.Exists(entry.LocalPath), entry.LocalPath);
            Assert.Equal($"sha256:{receipt!.NormalizedSha256}", entry.Sha256);
            Assert.Equal(receipt.ByteLength, entry.ByteLength);
            Assert.Equal(receipt.PngWidth, entry.PngWidth);
            Assert.Equal(receipt.PngHeight, entry.PngHeight);
            Assert.Equal(receipt.HasNonBlankPixels, entry.HasNonBlankPixels);
            Assert.Equal(receipt.NonWhitePixelCount, entry.NonWhitePixelCount);
            Assert.Equal(receipt.DistinctColorCount, entry.DistinctColorCount);
            Assert.Equal(BrowserBackedDesignConversionRenderProvider.ProviderKind, entry.Metadata["provider_kind"]);
            Assert.Equal(bool.TrueString, entry.Metadata["browser_render_proof_receipt_validated"]);
            Assert.Contains("browser_or_playwright_used:true", manifest.Value.EvidenceRefs);
            Assert.Contains("SERVER_OWNED_BROWSER_RENDER_PROVENANCE_CANONICALIZED", manifest.Value.Warnings);

            var serializedRenderResult = JsonSerializer.Serialize(renderer.LastRenderResult);
            Assert.DoesNotContain("InspectedLocalPath", serializedRenderResult, StringComparison.Ordinal);
            Assert.DoesNotContain(renderer.SnapshotPath!, serializedRenderResult, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(baseline, RemoteEvidenceTempFileCount());
        }
        finally
        {
            if (Directory.Exists(artifactRoot))
                Directory.Delete(artifactRoot, recursive: true);
        }
    }

    [Fact]
    public async Task BrowserRenderManifest_RejectsReceiptSnapshotMutationWithSanitizedMismatch()
    {
        var artifactRoot = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            "receipt-backed-browser-manifest-mutated",
            Guid.NewGuid().ToString("N")));
        try
        {
            var remoteUri = "https://127.0.0.1/receipt-backed-mutated-browser-screenshot.png";
            var renderer = new ReceiptBackedRemoteUriRenderProvider(
                artifactRoot,
                remoteUri,
                mutateSnapshotBeforeReturn: true);
            var provider = new DesignConversionBrowserRenderManifestProvider(renderer);

            var manifest = await provider.RenderManifestAsync(new DesignConversionBrowserRenderManifestRequest(
                RenderRunId: "receipt-backed-browser-manifest-mutated",
                RequestId: "receipt-backed-mutated-request",
                Html: "<main>mutated receipt-backed browser proof</main>",
                Css: "main{min-height:100vh;background:#7c2d12;color:#fff}",
                RequiredResolutions: [new RenderQaResolution("receipt-backed-mutated", 64, 32, 1.0)],
                Metadata: new Dictionary<string, string>
                {
                    ["qa_data_role"] = "receipt_backed_manifest_mutation_evidence_only"
                }));

            Assert.False(manifest.IsSuccess);
            Assert.Equal(DesignConversionBrowserRenderManifestProvider.BrowserRenderReceiptSnapshotMismatchCode, manifest.ErrorCode);
            Assert.Contains("snapshot changed", manifest.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(remoteUri, manifest.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(artifactRoot, manifest.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(renderer.SnapshotPath!, manifest.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.True(manifest.Value is null, "Failed manifest render must not return accepted receipt-backed entries.");
        }
        finally
        {
            if (Directory.Exists(artifactRoot))
                Directory.Delete(artifactRoot, recursive: true);
        }
    }

    [Fact]
    public async Task BrowserRenderManifest_DoesNotTrustBrowserMarkersWithoutInternalReceipt()
    {
        var baseline = RemoteEvidenceTempFileCount();
        var remoteUri = "https://127.0.0.1/marker-only-browser-screenshot.png";
        var provider = new DesignConversionBrowserRenderManifestProvider(
            new MarkerOnlyRemoteUriRenderProvider(remoteUri));

        var manifest = await provider.RenderManifestAsync(new DesignConversionBrowserRenderManifestRequest(
            RenderRunId: "marker-only-browser-manifest",
            RequestId: "marker-only-browser-request",
            Html: "<main>marker-only browser metadata is not a receipt</main>",
            Css: "main{min-height:100vh;background:#fff;color:#111}",
            RequiredResolutions: [new RenderQaResolution("marker-only", 64, 32, 1.0)],
            Metadata: new Dictionary<string, string>
            {
                ["qa_data_role"] = "marker_only_manifest_regression_evidence_only"
            }));

        Assert.False(manifest.IsSuccess);
        Assert.Equal("BROWSER_RENDER_REMOTE_SCREENSHOT_REQUIRES_RECEIPT", manifest.ErrorCode);
        Assert.DoesNotContain(remoteUri, manifest.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(DesignConversionBrowserRenderManifestProvider.BrowserRenderQaMarker, manifest.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.True(manifest.Value is null, "Marker-only metadata must not become accepted browser manifest evidence.");
        Assert.Equal(baseline, RemoteEvidenceTempFileCount());
    }

    [Fact]
    public async Task RuntimePath_WithRealBrowserAndUntrustedInjectedComparisonPersistsFourBrowserRenderRefsAndRetries()
    {
        var executable = LocalChromiumBrowserProcessAdapter.TryResolveDefaultExecutablePath();
        Assert.False(string.IsNullOrWhiteSpace(executable));
        Assert.True(File.Exists(executable), executable);

        var artifactRoot = Path.GetFullPath(Path.Combine(
            "TestResults",
            "codex-pass166-browser-backed-render-qa",
            Guid.NewGuid().ToString("N")));
        var dbRoot = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "pass166-browser-backed-runtime-artifact-db");
        var comparisonProvider = new Pass166LocalComparisonProvider(score: 0.97);
        var evaluator = new RecordingEvaluator(new DesignConversionInternalServiceThinClientJobEvaluator(
            new DesignConversionInternalService(
                new BrowserBackedDesignConversionRenderProvider(
                    new LocalChromiumBrowserProcessAdapter(executable, TimeSpan.FromSeconds(25), artifactRoot)),
                comparisonProvider)));
        var db = new JsonlDesignConversionLocalArtifactDb(dbRoot);
        var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock(), db);
        var submitted = runtime.Submit(CreatePass166SubmittedRequest());
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);
        var proofPath = Path.Combine(artifactRoot, "codex-pass166-browser-runtime-proof.json");

        if (!result.IsSuccess)
        {
            AssertBrowserRuntimeUnavailableOrSlowBlocker(result);
            Assert.False(File.Exists(proofPath), proofPath);
            return;
        }

        Assert.True(result.IsSuccess, result.ErrorMessage);
        if (string.Equals(result.Value!.ContractStatus, DesignConversionThinClientStatus.Failed, StringComparison.Ordinal))
        {
            AssertBrowserRuntimeBlockerStatus(result.Value);
            var runtimeErrorCode = result.Value.Job.Metadata["error_code"];
            Assert.Single(evaluator.Requests);
            var failedEvaluation = Assert.Single(evaluator.Responses);
            if (failedEvaluation.IsSuccess)
            {
                Assert.NotEmpty(failedEvaluation.Value!.ViewportFailures);
                Assert.All(
                    failedEvaluation.Value.ViewportFailures,
                    failure => Assert.Equal(runtimeErrorCode, failure.ErrorCode));
                Assert.Empty(failedEvaluation.Value.RenderResults);
            }
            else
            {
                Assert.Equal(runtimeErrorCode, failedEvaluation.ErrorCode);
            }

            Assert.Contains(runtimeErrorCode, BrowserRuntimeBlockerErrorCodes);
            Assert.False(File.Exists(proofPath), proofPath);

            var blockedListed = await db.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(JobId: submitted.Value.JobId));
            Assert.True(blockedListed.IsSuccess, blockedListed.ErrorMessage);
            Assert.Empty(blockedListed.Value!);
            return;
        }

        Assert.Equal(DesignConversionThinClientStatus.Completed, result.Value!.ContractStatus);
        Assert.Contains("local_artifact_db_persistence_verified=true", result.Value.Warnings);
        Assert.Single(evaluator.Requests);
        Assert.Equal(4, evaluator.Requests[0].RenderResolutions.Count);
        Assert.Equal(4, comparisonProvider.Requests.Count);

        var evaluation = Assert.Single(evaluator.Responses).Value!;
        Assert.Equal(4, evaluation.RenderResults.Count);
        Assert.Equal(4, evaluation.ComparisonArtifacts.Count);
        Assert.Equal(4, evaluation.VisualJudgeResults.Count);
        Assert.True(evaluation.VisualQaBoundaryTrace.MultiResolutionRender.IsComplete);
        Assert.Equal(4, evaluation.VisualQaBoundaryTrace.MultiResolutionRender.RequiredViewportIds.Count);
        Assert.Equal(4, evaluation.VisualQaBoundaryTrace.MultiResolutionRender.ObservedViewportIds.Count);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker, evaluation.VisualQaBoundaryTrace.VisualJudgeProviderBoundary);
        Assert.False(evaluation.VisualQaBoundaryTrace.ExternalAiVisualJudgeExecuted);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ContinueImprovement, evaluation.VisualQaBoundaryTrace.TierPolicy.StopReason);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionRetry, evaluation.VisualQaBoundaryTrace.VisualDecision.Decision);
        Assert.False(evaluation.VisualQaBoundaryTrace.VisualDecision.QualityThresholdMet);
        Assert.False(evaluation.VisualQaBoundaryTrace.ProductReady);
        Assert.All(evaluation.ComparisonArtifacts, artifact =>
        {
            Assert.Equal(
                bool.TrueString,
                artifact.Metadata[DesignConversionComparisonEvidenceTrustBoundary.ServerCanonicalizedKey]);
            Assert.Equal(
                DesignConversionComparisonEvidenceTrustBoundary.UntrustedInjectedClassification,
                artifact.Metadata[DesignConversionComparisonEvidenceTrustBoundary.TrustClassificationKey]);
            Assert.Equal(
                bool.FalseString,
                artifact.Metadata[DesignConversionRenderEvidenceTrustBoundary.ProductionTrustedKey]);
            Assert.Equal(
                bool.FalseString,
                artifact.Metadata[DesignConversionRenderEvidenceTrustBoundary.ApprovalEvidenceAllowedKey]);
            Assert.Equal(bool.FalseString, artifact.Metadata["visual_comparison_proven"]);
        });
        Assert.All(evaluation.VisualJudgeResults, judge =>
        {
            Assert.False(judge.Verdict.Passed);
            Assert.Equal(0.0, judge.Verdict.Score);
            Assert.Contains(judge.Verdict.Defects, defect =>
                string.Equals(defect.Category, "comparison_visual_comparison_not_proven", StringComparison.Ordinal) &&
                string.Equals(defect.Severity, "major", StringComparison.Ordinal));
        });
        Assert.All(evaluation.RenderResults, render =>
        {
            Assert.Equal(BrowserBackedDesignConversionRenderProvider.ProviderKind, render.Image.Metadata["provider_kind"]);
            Assert.Equal(bool.TrueString, render.Image.Metadata["browser_or_playwright_used"]);
            Assert.Equal(bool.TrueString, render.Image.Metadata["visual_fidelity_claimed"]);
            Assert.Equal(bool.FalseString, render.Image.Metadata["external_ai_used"]);
            Assert.Equal("local_chromium_process", render.Image.Metadata["browser_adapter_kind"]);
            Assert.Equal(executable, render.Image.Metadata["browser_executable"]);

            var png = DesignConversionBrowserPngEvidenceInspector.Inspect(render.Image.Uri);
            Assert.True(png.IsSuccess, png.ErrorMessage);
            Assert.True(File.Exists(png.Value!.LocalPath), png.Value.LocalPath);
            Assert.True(png.Value.ByteLength > 1000, png.Value.LocalPath);
            Assert.Equal(render.Image.Resolution.Width, png.Value.PngWidth);
            Assert.Equal(render.Image.Resolution.Height, png.Value.PngHeight);
            Assert.True(png.Value.HasNonBlankPixels, png.Value.LocalPath);
            Assert.True(png.Value.NonWhitePixelCount > 0, png.Value.LocalPath);
            Assert.True(png.Value.DistinctColorCount > 1, png.Value.LocalPath);
        });

        var listed = await db.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(JobId: submitted.Value.JobId));
        Assert.True(listed.IsSuccess, listed.ErrorMessage);
        var stored = Assert.Single(listed.Value!);
        Assert.Equal(4, stored.RenderArtifactRefs.Count);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker, stored.VisualJudgeStatus.ProviderBoundary);
        Assert.False(stored.VisualJudgeStatus.ExternalCallExecuted);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ContinueImprovement, stored.VisualJudgeStatus.TierDecision);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionRetry, stored.VisualJudgeStatus.Metadata["visual_decision"]);
        Assert.Equal("4", stored.VisualJudgeStatus.Metadata["visual_decision_correction_count"]);
        Assert.Equal(bool.TrueString, stored.VisualJudgeStatus.Metadata["product_not_ready"]);
        Assert.All(stored.RenderArtifactRefs, artifact =>
        {
            Assert.Equal("browser-render-screenshot", artifact.Kind);
            Assert.StartsWith("sha256:", artifact.Sha256, StringComparison.Ordinal);
            Assert.NotEmpty(artifact.UriRef);
            Assert.DoesNotContain("PRIVATE_PASS166_SECRET", artifact.UriRef, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(".pass166-private", artifact.UriRef, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(BrowserBackedDesignConversionRenderProvider.ProviderKind, artifact.Metadata["provider_kind"]);
            Assert.Equal(bool.TrueString, artifact.Metadata["browser_or_playwright_used"]);
            Assert.Equal(bool.FalseString, artifact.Metadata["external_ai_used"]);
            Assert.Contains(evaluation.RenderResults, render =>
                string.Equals(render.Image.Sha256, artifact.Sha256, StringComparison.Ordinal) &&
                render.Image.Resolution.Width == artifact.Width &&
                render.Image.Resolution.Height == artifact.Height);
        });

        var persistedText = await File.ReadAllTextAsync(Path.Combine(dbRoot, "conversion-results.jsonl"));
        Assert.Contains("render_artifact_refs", persistedText, StringComparison.Ordinal);
        Assert.Contains("visual_decision", persistedText, StringComparison.Ordinal);
        Assert.Contains(BrowserBackedDesignConversionRenderProvider.ProviderKind, persistedText, StringComparison.Ordinal);
        Assert.DoesNotContain("PRIVATE_PASS166_SECRET", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<main", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".pass166-private", persistedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("external-provider-payload", persistedText, StringComparison.OrdinalIgnoreCase);

        await File.WriteAllTextAsync(
            proofPath,
            JsonSerializer.Serialize(new
            {
                schema = "codex-pass166-browser-backed-untrusted-comparison-retry-proof-v1",
                browser_executable = executable,
                artifact_root = artifactRoot,
                local_artifact_db_root = dbRoot,
                viewport_count = evaluation.RenderResults.Count,
                browser_render_executed = true,
                external_ai_visual_judge_executed = false,
                visual_decision = evaluation.VisualQaBoundaryTrace.VisualDecision.Decision,
                tier_stop_reason = evaluation.VisualQaBoundaryTrace.TierPolicy.StopReason,
                comparison_trust_artifacts = evaluation.ComparisonArtifacts.Select(artifact => new
                {
                    artifact.ArtifactId,
                    server_canonicalized = artifact.Metadata[DesignConversionComparisonEvidenceTrustBoundary.ServerCanonicalizedKey],
                    trust_classification = artifact.Metadata[DesignConversionComparisonEvidenceTrustBoundary.TrustClassificationKey],
                    production_trusted = artifact.Metadata[DesignConversionRenderEvidenceTrustBoundary.ProductionTrustedKey],
                    approval_evidence_allowed = artifact.Metadata[DesignConversionRenderEvidenceTrustBoundary.ApprovalEvidenceAllowedKey],
                    visual_comparison_proven = artifact.Metadata["visual_comparison_proven"]
                }).ToArray(),
                render_artifacts = stored.RenderArtifactRefs.Select(artifact => new
                {
                    artifact.ViewportId,
                    artifact.Width,
                    artifact.Height,
                    artifact.UriRef,
                    artifact.Sha256,
                    provider_kind = artifact.Metadata["provider_kind"],
                    browser_or_playwright_used = artifact.Metadata["browser_or_playwright_used"]
                }).ToArray()
            }, new JsonSerializerOptions { WriteIndented = true }),
            Encoding.UTF8);
        Assert.True(File.Exists(proofPath), proofPath);
    }

    [Theory]
    [InlineData(LocalChromiumBrowserProcessAdapter.RuntimeUnavailableCode)]
    [InlineData(LocalChromiumBrowserProcessAdapter.RuntimeCrashedBeforeScreenshotCode)]
    public async Task RuntimePath_WithBrowserRuntimeBlockerEvaluatorFailurePreservesSanitizedRuntimeBoundaryCode(
        string browserErrorCode)
    {
        var evaluator = new FixedFailureEvaluator(
            "DesignConversion thin-client job failed after browser process failure.",
            browserErrorCode);
        var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock());
        var submitted = runtime.Submit(CreatePass166SubmittedRequest());
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var status = result.Value!;
        AssertBrowserRuntimeBlockerStatus(status);
        Assert.Equal(browserErrorCode, status.Job.Metadata["error_code"]);
        Assert.Equal(
            $"{DesignConversionThinClientJobRuntime.BrowserRuntimeBlockerSafeMessage} error_code={browserErrorCode}.",
            status.Message);
        Assert.Single(evaluator.Requests);
    }

    [Theory]
    [InlineData(LocalChromiumBrowserProcessAdapter.RuntimeUnavailableCode)]
    [InlineData(LocalChromiumBrowserProcessAdapter.RuntimeCrashedBeforeScreenshotCode)]
    public async Task RuntimePath_WithBrowserRenderViewportFailurePreservesSanitizedRuntimeBoundaryCode(
        string browserErrorCode)
    {
        const string secretMarker = "PRIVATE_BROWSER_VIEWPORT_SECRET";
        var testRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var dbRoot = Path.Combine(
            testRoot,
            "browser-viewport-blocker-runtime-artifact-db");
        try
        {
            var evaluator = new RecordingEvaluator(new DesignConversionInternalServiceThinClientJobEvaluator(
                new DesignConversionInternalService(
                    new BrowserBackedDesignConversionRenderProvider(new FailingBrowserAdapter(
                        $"Browser process failed before screenshot; stderr={secretMarker}; path=C:\\private\\browser\\profile.",
                        browserErrorCode)),
                    new Pass166LocalComparisonProvider(score: 0.97))));
            var db = new JsonlDesignConversionLocalArtifactDb(dbRoot);
            var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock(), db);
            var submitted = runtime.Submit(CreatePass166SubmittedRequest());
            Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

            var result = await runtime.RunAsync(submitted.Value!.JobId);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            var status = result.Value!;
            AssertBrowserRuntimeBlockerStatus(status);
            Assert.Equal(browserErrorCode, status.Job.Metadata["error_code"]);
            AssertThinClientStatusDoesNotContain(status, secretMarker, @"C:\private\browser\profile");
            Assert.Single(evaluator.Requests);
            var evaluation = Assert.Single(evaluator.Responses);
            Assert.True(evaluation.IsSuccess, evaluation.ErrorMessage);
            Assert.All(evaluation.Value!.ViewportFailures, failure => Assert.Equal(browserErrorCode, failure.ErrorCode));
            Assert.Empty(evaluation.Value.RenderResults);

            var listed = await db.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(JobId: submitted.Value.JobId));
            Assert.True(listed.IsSuccess, listed.ErrorMessage);
            Assert.Empty(listed.Value!);
        }
        finally
        {
            DeleteDirectoryIfExists(testRoot);
        }
    }

    [Fact]
    public async Task RuntimePath_WithUnrelatedEvaluatorFailureDoesNotRelabelAsBrowserBlocker()
    {
        const string unrelatedErrorCode = "COMPARISON_PROVIDER_UNAVAILABLE";
        const string unrelatedMessage = "Comparison provider failed before visual scoring.";
        var evaluator = new FixedFailureEvaluator(unrelatedMessage, unrelatedErrorCode);
        var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock());
        var submitted = runtime.Submit(CreatePass166SubmittedRequest());
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var status = result.Value!;
        Assert.Equal(DesignConversionThinClientStatus.Failed, status.ContractStatus);
        Assert.Equal(unrelatedMessage, status.Message);
        Assert.Equal(unrelatedErrorCode, status.Job.Metadata["error_code"]);
        Assert.Contains($"internal_evaluator_error_code={unrelatedErrorCode}", status.Warnings);
        Assert.DoesNotContain(
            $"{DesignConversionThinClientJobRuntime.BrowserRuntimeBlockerProvenanceKey}=true",
            status.Warnings);
        Assert.False(status.Provenance.ContainsKey(DesignConversionThinClientJobRuntime.BrowserRuntimeBlockerProvenanceKey));
        Assert.False(status.Provenance.ContainsKey(DesignConversionThinClientJobRuntime.BrowserRuntimeBlockerErrorCodeProvenanceKey));
        Assert.Single(evaluator.Requests);
    }

    [Fact]
    public async Task RuntimePath_WithBrowserRuntimeBlockerDoesNotLeakRawProcessText()
    {
        const string secretMarker = "PRIVATE_BROWSER_RUNTIME_SECRET";
        const string secretPath = @"C:\private\browser\profile\chrome.exe";
        const string rawHtmlMarker = "<main data-secret=\"PRIVATE_BROWSER_RUNTIME_SECRET\">";
        var evaluator = new FixedFailureEvaluator(
            $"Browser stderr contained {secretMarker} at {secretPath} and {rawHtmlMarker}.",
            LocalChromiumBrowserProcessAdapter.RuntimeCrashedBeforeScreenshotCode);
        var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock());
        var submitted = runtime.Submit(CreatePass166SubmittedRequest());
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var status = result.Value!;
        AssertBrowserRuntimeBlockerStatus(status);
        AssertThinClientStatusDoesNotContain(status, secretMarker, secretPath, rawHtmlMarker);
        Assert.Single(evaluator.Requests);
    }

    [Fact]
    public async Task RuntimePath_WithGenericBrowserScreenshotFailureSanitizesWithoutRuntimeBlockerProvenance()
    {
        const string secretMarker = "PRIVATE_BROWSER_SCREENSHOT_SECRET";
        const string secretPath = @"C:\private\browser\profile\screenshot.png";
        var evaluator = new FixedFailureEvaluator(
            $"Browser screenshot failed with {secretMarker} at {secretPath}.",
            LocalChromiumBrowserProcessAdapter.ScreenshotFailedCode);
        var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock());
        var submitted = runtime.Submit(CreatePass166SubmittedRequest());
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

        var result = await runtime.RunAsync(submitted.Value!.JobId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var status = result.Value!;
        AssertBrowserScreenshotFailureStatus(status);
        AssertThinClientStatusDoesNotContain(status, secretMarker, secretPath);
        Assert.Single(evaluator.Requests);
    }

    [Fact]
    public async Task RuntimePath_WithGenericBrowserScreenshotViewportFailureFailsWithoutPersistenceOrBlockerProvenance()
    {
        const string secretMarker = "PRIVATE_BROWSER_VIEWPORT_SCREENSHOT_SECRET";
        const string secretPath = @"C:\private\browser\profile\viewport.png";
        var testRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var dbRoot = Path.Combine(testRoot, "browser-viewport-screenshot-failure-artifact-db");
        try
        {
            var evaluator = new RecordingEvaluator(new DesignConversionInternalServiceThinClientJobEvaluator(
                new DesignConversionInternalService(
                    new BrowserBackedDesignConversionRenderProvider(new FailingBrowserAdapter(
                        $"Browser screenshot failed with {secretMarker} at {secretPath}.",
                        LocalChromiumBrowserProcessAdapter.ScreenshotFailedCode)),
                    new Pass166LocalComparisonProvider(score: 0.97))));
            var db = new JsonlDesignConversionLocalArtifactDb(dbRoot);
            var runtime = new DesignConversionThinClientJobRuntime(evaluator, FixedClock(), db);
            var submitted = runtime.Submit(CreatePass166SubmittedRequest());
            Assert.True(submitted.IsSuccess, submitted.ErrorMessage);

            var result = await runtime.RunAsync(submitted.Value!.JobId);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            var status = result.Value!;
            AssertBrowserScreenshotFailureStatus(status);
            AssertThinClientStatusDoesNotContain(status, secretMarker, secretPath);
            Assert.Single(evaluator.Requests);
            var evaluation = Assert.Single(evaluator.Responses);
            Assert.True(evaluation.IsSuccess, evaluation.ErrorMessage);
            Assert.NotEmpty(evaluation.Value!.ViewportFailures);
            Assert.All(
                evaluation.Value.ViewportFailures,
                failure => Assert.Equal(LocalChromiumBrowserProcessAdapter.ScreenshotFailedCode, failure.ErrorCode));
            Assert.Empty(evaluation.Value.RenderResults);

            var listed = await db.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(JobId: submitted.Value.JobId));
            Assert.True(listed.IsSuccess, listed.ErrorMessage);
            Assert.Empty(listed.Value!);
        }
        finally
        {
            DeleteDirectoryIfExists(testRoot);
        }
    }

    [Fact]
    public async Task DeterministicLocalPngRenderer_RejectsInvalidHtmlAndUnsafeArtifactRoot()
    {
        var viewport = new RenderQaResolution("mobile", 375, 812, 2.0);
        var unsafeRootRenderer = new DeterministicLocalDesignConversionPngRenderProvider(@"..\pass159-escape");
        var unsafeRoot = await unsafeRootRenderer.RenderAsync(CreateRenderRequest("pass159-unsafe-root", viewport));

        Assert.False(unsafeRoot.IsSuccess);
        Assert.Equal("LOCAL_RENDER_ARTIFACT_ROOT_UNSAFE", unsafeRoot.ErrorCode);

        var artifactRoot = Path.GetFullPath(Path.Combine(
            "TestResults",
            "codex-pass159-invalid-html",
            Guid.NewGuid().ToString("N")));
        var renderer = new DeterministicLocalDesignConversionPngRenderProvider(artifactRoot);
        var invalidHtml = await renderer.RenderAsync(CreateRenderRequest("pass159-invalid-html", viewport) with
        {
            Html = ""
        });

        Assert.False(invalidHtml.IsSuccess);
        Assert.Equal("LOCAL_RENDER_HTML_REQUIRED", invalidHtml.ErrorCode);
    }

    [Fact]
    public void BrowserRenderManifest_RejectsMissingRequiredViewport()
    {
        var required = new[]
        {
            new RenderQaResolution("mobile", 360, 740, 1.0),
            new RenderQaResolution("tablet", 768, 1024, 1.0),
            new RenderQaResolution("desktop", 1440, 900, 1.0)
        };
        var entries = new[]
        {
            ManifestEntry(required[0]),
            ManifestEntry(required[2])
        };

        var coverage = DesignConversionBrowserRenderManifestProvider.ValidateRequiredViewportCoverage(required, entries);

        Assert.False(coverage.IsSuccess);
        Assert.Equal("BROWSER_RENDER_VIEWPORT_MISSING", coverage.ErrorCode);
        Assert.Contains("tablet", coverage.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    private static string NewQuotaTestRoot(string leaf)
        => Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            leaf,
            Guid.NewGuid().ToString("N")));

    private static byte[] PayloadWithMarker(int safePrefixByteCount, string privateMarker)
    {
        var markerBytes = Encoding.UTF8.GetBytes(privateMarker);
        var payload = new byte[safePrefixByteCount + markerBytes.Length];
        Array.Fill(payload, (byte)'A', 0, safePrefixByteCount);
        Buffer.BlockCopy(markerBytes, 0, payload, safePrefixByteCount, markerBytes.Length);
        return payload;
    }

    private static void AssertNoPrivateContentLeak(OperationResult<byte[]> result, string privateMarker)
    {
        var serialized = JsonSerializer.Serialize(new
        {
            result.ErrorCode,
            result.ErrorMessage
        });
        Assert.DoesNotContain(privateMarker, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(Convert.ToBase64String(Encoding.UTF8.GetBytes(privateMarker)), serialized, StringComparison.Ordinal);
    }

    private sealed class AdversarialReadStream : Stream
    {
        private readonly byte[] _bytes;
        private readonly bool _canSeek;
        private readonly long? _reportedLength;
        private long _position;

        public AdversarialReadStream(byte[] bytes, bool canSeek, long? reportedLength)
        {
            _bytes = bytes;
            _canSeek = canSeek;
            _reportedLength = reportedLength;
        }

        public override bool CanRead => true;
        public override bool CanSeek => _canSeek;
        public override bool CanWrite => false;

        public override long Length
            => _canSeek && _reportedLength.HasValue
                ? _reportedLength.Value
                : throw new NotSupportedException();

        public override long Position
        {
            get => _position;
            set
            {
                if (!_canSeek)
                    throw new NotSupportedException();
                _position = value;
            }
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_position >= _bytes.LongLength)
                return 0;

            var read = Math.Min(count, checked((int)(_bytes.LongLength - _position)));
            Buffer.BlockCopy(_bytes, checked((int)_position), buffer, offset, read);
            _position += read;
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!_canSeek)
                throw new NotSupportedException();

            var next = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => _position + offset,
                SeekOrigin.End => Length + offset,
                _ => throw new ArgumentOutOfRangeException(nameof(origin))
            };
            if (next < 0)
                throw new IOException("Cannot seek before the beginning of the stream.");

            _position = next;
            return _position;
        }

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();
    }

    private sealed class ThrowOnReadStream : Stream
    {
        private readonly string _privateMarker;

        public ThrowOnReadStream(string privateMarker)
        {
            _privateMarker = privateMarker;
        }

        public int ReadCallCount { get; private set; }
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ReadCallCount++;
            throw new IOException(_privateMarker);
        }

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();
    }

    private sealed class AsyncOnlyReadStream : Stream
    {
        private readonly byte[] _bytes;
        private readonly bool _canSeek;
        private readonly long? _reportedLength;
        private readonly int _maxChunkSize;
        private readonly int? _cancelBeforeReadNumber;
        private readonly CancellationTokenSource? _cancellationSource;
        private long _position;

        public AsyncOnlyReadStream(
            byte[] bytes,
            bool canSeek,
            long? reportedLength,
            int maxChunkSize = int.MaxValue,
            int? cancelBeforeReadNumber = null,
            CancellationTokenSource? cancellationSource = null)
        {
            _bytes = bytes;
            _canSeek = canSeek;
            _reportedLength = reportedLength;
            _maxChunkSize = maxChunkSize;
            _cancelBeforeReadNumber = cancelBeforeReadNumber;
            _cancellationSource = cancellationSource;
        }

        public int SyncReadCallCount { get; private set; }
        public int AsyncReadCallCount { get; private set; }
        public override bool CanRead => true;
        public override bool CanSeek => _canSeek;
        public override bool CanWrite => false;

        public override long Length
            => _canSeek && _reportedLength.HasValue
                ? _reportedLength.Value
                : throw new NotSupportedException();

        public override long Position
        {
            get => _position;
            set
            {
                if (!_canSeek)
                    throw new NotSupportedException();
                _position = value;
            }
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            SyncReadCallCount++;
            throw new IOException("Synchronous reads are forbidden for this async reader test stream.");
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            AsyncReadCallCount++;
            if (_cancelBeforeReadNumber == AsyncReadCallCount)
            {
                _cancellationSource?.Cancel();
                return Task.FromCanceled<int>(cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<int>(cancellationToken);
            if (_position >= _bytes.LongLength)
                return Task.FromResult(0);

            var boundedCount = Math.Min(count, _maxChunkSize);
            var read = Math.Min(boundedCount, checked((int)(_bytes.LongLength - _position)));
            Buffer.BlockCopy(_bytes, checked((int)_position), buffer, offset, read);
            _position += read;
            return Task.FromResult(read);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!_canSeek)
                throw new NotSupportedException();

            var next = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => _position + offset,
                SeekOrigin.End => Length + offset,
                _ => throw new ArgumentOutOfRangeException(nameof(origin))
            };
            if (next < 0)
                throw new IOException("Cannot seek before the beginning of the stream.");

            _position = next;
            return _position;
        }

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();
    }

    private class TrackingReadStream(byte[] bytes) : MemoryStream(bytes, writable: false)
    {
        public bool IsDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }

    private sealed class CancelAfterEofReadStream(
        byte[] bytes,
        CancellationTokenSource cancellationSource) : TrackingReadStream(bytes)
    {
        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            var read = await base.ReadAsync(buffer, cancellationToken);
            if (read == 0)
                cancellationSource.Cancel();
            return read;
        }

        public override async Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken)
        {
            var read = await base.ReadAsync(buffer, offset, count, cancellationToken);
            if (read == 0)
                cancellationSource.Cancel();
            return read;
        }
    }

    private sealed class TrackingDisposable : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
            => IsDisposed = true;
    }

    private static string CreateRetainedCaptureScreenshot(string artifactRoot, long byteLength)
    {
        var captureRoot = Path.Combine(artifactRoot, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(captureRoot);
        using var stream = File.Create(Path.Combine(captureRoot, "screenshot.png"));
        stream.SetLength(byteLength);
        return captureRoot;
    }

    private static string[] DirectGuidCaptureDirectories(string artifactRoot)
    {
        if (!Directory.Exists(artifactRoot))
            return [];

        return Directory
            .EnumerateDirectories(artifactRoot, "*", SearchOption.TopDirectoryOnly)
            .Where(path => Guid.TryParseExact(Path.GetFileName(path), "N", out _))
            .Select(Path.GetFullPath)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
    }

    private static void DeleteDirectoryIfExists(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }

    private static int RemoteEvidenceTempFileCount()
    {
        var count = DesignConversionBrowserPngEvidenceInspector.CountRemoteEvidenceTempFilesForTesting();
        Assert.True(count.IsSuccess, count.ErrorMessage);
        return count.Value;
    }

    private static async Task<OperationResult<DesignConversionBrowserPngEvidence>> InspectRemoteFailureForCleanupEnvelopeAsync(
        string screenshotUri,
        FakeRemotePngDnsResolver resolver,
        FakeRemotePngHttpTransport transport,
        string expectedErrorCode,
        int expectedBaseline,
        IReadOnlyList<string>? privateValues = null,
        long maxBytes = 4096,
        CancellationToken cancellationToken = default)
    {
        Assert.Equal(expectedBaseline, RemoteEvidenceTempFileCount());

        var result = await DesignConversionBrowserPngEvidenceInspector.InspectAsync(
            screenshotUri,
            resolver,
            transport,
            RemotePolicy(maxBytes: maxBytes),
            cancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedErrorCode, result.ErrorCode);
        AssertPngInspectionResultDoesNotLeak(result, privateValues ?? Array.Empty<string>());
        Assert.Equal(expectedBaseline, RemoteEvidenceTempFileCount());
        return result;
    }

    private static void AssertPngInspectionResultDoesNotLeak(
        OperationResult<DesignConversionBrowserPngEvidence> result,
        IEnumerable<string> privateValues)
    {
        var haystacks = new List<string>
        {
            result.ErrorCode ?? string.Empty,
            result.ErrorMessage ?? string.Empty
        };

        if (result.Value is { } evidence)
        {
            haystacks.Add(evidence.LocalPath);
            haystacks.Add(evidence.SnapshotSha256);
        }

        foreach (var privateValue in privateValues.Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            foreach (var haystack in haystacks)
                Assert.DoesNotContain(privateValue, haystack, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static DesignConversionBrowserCaptureRequest BrowserCaptureRequest(string captureRequestId)
        => new(
            CaptureRequestId: captureRequestId,
            RequestId: $"request-{captureRequestId}",
            DocumentHtml: "<main>quota browser capture</main>",
            Resolution: new RenderQaResolution("quota-test", 16, 16, 1.0),
            Metadata: new Dictionary<string, string>());

    private static LocalChromiumBrowserProcessAdapter CreateQuotaTestAdapter(
        string artifactRoot,
        FakeLocalChromiumBrowserProcessRunner processRunner,
        long maxRetainedCaptureBytes,
        int maxConcurrency = 1,
        LocalChromiumBrowserProcessAdapter.IWorkspaceCleaner? workspaceCleaner = null)
    {
        var dummyExecutablePath = typeof(DesignConversionBrowserQaProviderTests).Assembly.Location;
        Assert.True(File.Exists(dummyExecutablePath), $"Test assembly must exist at {dummyExecutablePath}.");
        return new LocalChromiumBrowserProcessAdapter(
            executablePath: dummyExecutablePath,
            timeout: TimeSpan.FromSeconds(5),
            artifactRoot: artifactRoot,
            captureConcurrencyGate: new LocalChromiumBrowserCaptureConcurrencyGate(maxConcurrency),
            maxRetainedCaptureBytes: maxRetainedCaptureBytes,
            processRunner: processRunner,
            workspaceCleaner: workspaceCleaner);
    }

    private static Task<OperationResult<LocalChromiumBrowserProcessRunResult>> WriteSuccessfulFakeBrowserScreenshotAsync(
        FakeLocalChromiumBrowserProcessRunRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var physicalResolution = DesignConversionBrowserPhysicalScreenshotResolution.Resolve(
            request.Resolution,
            "FAKE_BROWSER_CAPTURE_RESOLUTION_INVALID",
            "Fake browser capture");
        if (!physicalResolution.IsSuccess)
            return Task.FromResult(physicalResolution.MapError<LocalChromiumBrowserProcessRunResult>());

        File.WriteAllBytes(
            request.ScreenshotPath,
            SolidRgbaPng(
                physicalResolution.Value!.PhysicalWidth,
                physicalResolution.Value.PhysicalHeight,
                red: 32,
                green: 96,
                blue: 160,
                alpha: 255));
        return Task.FromResult(OperationResult<LocalChromiumBrowserProcessRunResult>.Ok(
            new LocalChromiumBrowserProcessRunResult(ElapsedMs: 2, Stdout: "", Stderr: "")));
    }

    private static void WriteFakeBrowserProfileCache(string userDataPath, string privateMarker)
    {
        var cachePath = Path.Combine(userDataPath, "Default", "Cache", "private-cache-entry");
        Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);
        File.WriteAllText(cachePath, privateMarker, Encoding.UTF8);
    }

    private static void AssertCaptureRootContainsOnlyRetainedScreenshots(
        string artifactRoot,
        int expectedScreenshotCount)
    {
        var files = Directory.Exists(artifactRoot)
            ? Directory
                .EnumerateFiles(artifactRoot, "*", SearchOption.AllDirectories)
                .Select(Path.GetFullPath)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray()
            : [];
        var directories = Directory.Exists(artifactRoot)
            ? Directory
                .EnumerateDirectories(artifactRoot, "*", SearchOption.AllDirectories)
                .Select(Path.GetFullPath)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray()
            : [];

        Assert.Equal(expectedScreenshotCount, files.Length);
        Assert.All(files, path => Assert.Equal("screenshot.png", Path.GetFileName(path)));
        Assert.Equal(expectedScreenshotCount, files.Count(path =>
            string.Equals(Path.GetFileName(path), "screenshot.png", StringComparison.Ordinal)));
        Assert.Equal(expectedScreenshotCount, directories.Length);
        Assert.All(directories, path => Assert.True(
            Guid.TryParseExact(Path.GetFileName(path), "N", out _),
            $"Only retained GUID screenshot roots may remain, but found {path}."));
    }

    private static void AssertNoSensitiveBrowserWorkspaceArtifactsRemain(
        string artifactRoot,
        string privateMarker)
    {
        if (!Directory.Exists(artifactRoot))
            return;

        var persistedDocuments = Directory
            .EnumerateFiles(artifactRoot, "index.html", SearchOption.AllDirectories)
            .ToArray();
        var persistedProfiles = Directory
            .EnumerateDirectories(artifactRoot, "profile", SearchOption.AllDirectories)
            .ToArray();
        var fileSystemNames = Directory
            .EnumerateFileSystemEntries(artifactRoot, "*", SearchOption.AllDirectories)
            .ToArray();

        Assert.Empty(persistedDocuments);
        Assert.Empty(persistedProfiles);
        Assert.DoesNotContain(fileSystemNames, path => path.Contains(privateMarker, StringComparison.Ordinal));
    }

    private static DesignConversionRenderRequest CreateRenderRequest(string renderRequestId, RenderQaResolution resolution)
        => new(
            RenderRequestId: renderRequestId,
            RequestId: "request-001",
            Html: "<main class=\"card\"><h1>Hello</h1></main>",
            Css: ".card{display:grid;gap:12px;color:#111;}",
            Resolution: resolution,
            Metadata: new Dictionary<string, string>
            {
                ["source_screenshot_uri"] = "artifact://source-shot.png",
                ["source_screenshot_sha256"] = "sha256:source-shot",
                ["qa_data_role"] = "input_evidence_only"
            });

    private static void AssertNoBrowserRetryPrivateLeak<T>(
        string privateMarker,
        OperationResult<T> result)
        => AssertNoBrowserRetryPrivateLeak(privateMarker, string.Empty, result);

    private static void AssertNoBrowserRetryExceptionLeak<T>(OperationResult<T> result)
    {
        var serialized = JsonSerializer.Serialize(new
        {
            result.ErrorCode,
            result.ErrorMessage,
            result.Value
        });
        Assert.DoesNotContain(nameof(OperationCanceledException), serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("TaskCanceledException", serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("The operation was canceled", serialized, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertNoBrowserRetryPrivateLeak<T>(
        string privateMarker,
        string privatePath,
        OperationResult<T> result)
    {
        var serialized = JsonSerializer.Serialize(new
        {
            result.ErrorCode,
            result.ErrorMessage,
            result.Value
        });
        Assert.DoesNotContain(privateMarker, serialized, StringComparison.Ordinal);
        if (!string.IsNullOrWhiteSpace(privatePath))
            Assert.DoesNotContain(privatePath, serialized, StringComparison.OrdinalIgnoreCase);
    }

    private static DesignConversionBrowserRenderManifestEntry ManifestEntry(RenderQaResolution resolution)
        => new(
            ViewportId: resolution.ResolutionId,
            RequestedWidth: resolution.Width,
            RequestedHeight: resolution.Height,
            DeviceScaleFactor: resolution.DeviceScaleFactor,
            ScreenshotUri: $"file:///C:/tmp/{resolution.ResolutionId}.png",
            LocalPath: $@"C:\tmp\{resolution.ResolutionId}.png",
            Sha256: "sha256:" + new string('a', 64),
            ByteLength: 2048,
            PngWidth: resolution.Width,
            PngHeight: resolution.Height,
            HasNonBlankPixels: true,
            NonWhitePixelCount: 10,
            DistinctColorCount: 2,
            ElapsedMs: 5,
            Metadata: new Dictionary<string, string>());

    private static DesignConversionRemotePngFetchPolicy RemotePolicy(long maxBytes = 4096, int maxRedirects = 5)
        => DesignConversionBrowserPngEvidenceInspector.DefaultRemoteFetchPolicy with
        {
            MaxBytes = maxBytes,
            MaxRedirects = maxRedirects,
            Timeout = TimeSpan.FromSeconds(1)
        };

    private static DesignConversionRemotePngHttpResponse FakeRemotePngHttpResponse(
        int statusCode,
        byte[]? body = null,
        string contentType = "image/png",
        long? contentLength = null,
        string? redirectLocation = null,
        string[]? contentEncodings = null)
    {
        var bytes = body ?? [];
        return new DesignConversionRemotePngHttpResponse(
            statusCode,
            contentType,
            contentLength,
            redirectLocation is null ? null : new Uri(redirectLocation, UriKind.RelativeOrAbsolute),
            new MemoryStream(bytes, writable: false),
            contentEncodings);
    }

    private static DesignConversionRemotePngHttpResponse FakeRemotePngHttpResponse(
        int statusCode,
        Stream body,
        string contentType = "image/png",
        long? contentLength = null,
        string? redirectLocation = null,
        string[]? contentEncodings = null,
        IDisposable? owner = null)
        => new(
            statusCode,
            contentType,
            contentLength,
            redirectLocation is null ? null : new Uri(redirectLocation, UriKind.RelativeOrAbsolute),
            body,
            contentEncodings,
            owner);

    private static TestPngFixture SupportedColorTypePng(string colorType)
        => colorType switch
        {
            "rgba" => new TestPngFixture(
                PngWithNativePixels(
                    width: 3,
                    height: 2,
                    colorType: 6,
                    bytesPerPixel: 4,
                    [
                        0, 0, 0, 255,
                        255, 255, 255, 255,
                        10, 20, 30, 0,
                        128, 128, 128, 255,
                        255, 255, 255, 0,
                        10, 20, 30, 128
                    ]),
                [
                    0, 0, 0, 255,
                    255, 255, 255, 255,
                    10, 20, 30, 0,
                    128, 128, 128, 255,
                    255, 255, 255, 0,
                    10, 20, 30, 128
                ]),
            "rgb" => new TestPngFixture(
                PngWithNativePixels(
                    width: 3,
                    height: 2,
                    colorType: 2,
                    bytesPerPixel: 3,
                    [
                        0, 0, 0,
                        255, 255, 255,
                        10, 20, 30,
                        128, 128, 128,
                        255, 0, 0,
                        0, 255, 0
                    ]),
                [
                    0, 0, 0, 255,
                    255, 255, 255, 255,
                    10, 20, 30, 255,
                    128, 128, 128, 255,
                    255, 0, 0, 255,
                    0, 255, 0, 255
                ]),
            "grayscale" => new TestPngFixture(
                PngWithNativePixels(
                    width: 3,
                    height: 2,
                    colorType: 0,
                    bytesPerPixel: 1,
                    [0, 255, 128, 64, 10, 200]),
                [
                    0, 0, 0, 255,
                    255, 255, 255, 255,
                    128, 128, 128, 255,
                    64, 64, 64, 255,
                    10, 10, 10, 255,
                    200, 200, 200, 255
                ]),
            "grayscale-alpha" => new TestPngFixture(
                PngWithNativePixels(
                    width: 3,
                    height: 2,
                    colorType: 4,
                    bytesPerPixel: 2,
                    [0, 255, 255, 255, 128, 0, 64, 128, 255, 0, 10, 255]),
                [
                    0, 0, 0, 255,
                    255, 255, 255, 255,
                    128, 128, 128, 0,
                    64, 64, 64, 128,
                    255, 255, 255, 0,
                    10, 10, 10, 255
                ]),
            _ => throw new ArgumentOutOfRangeException(nameof(colorType), colorType, "Unsupported PNG color type fixture.")
        };

    private static void AssertPngFixtureMatchesInspectorAndComparison(TestPngFixture fixture, string path)
    {
        var inspected = DesignConversionBrowserPngEvidenceInspector.Inspect(path);
        var decoded = DesignConversionBrowserPngEvidenceInspector.DecodePngBytesForComparison(fixture.PngBytes, path);

        Assert.True(inspected.IsSuccess, $"{inspected.ErrorCode}: {inspected.ErrorMessage}");
        Assert.True(decoded.IsSuccess, $"{decoded.ErrorCode}: {decoded.ErrorMessage}");
        Assert.Equal(fixture.ExpectedRgbaPixels, decoded.Value!.RgbaPixels);

        var expectedMetrics = MeasureRgbaPixelsForTest(fixture.ExpectedRgbaPixels);
        Assert.Equal(decoded.Value.PngWidth, inspected.Value!.PngWidth);
        Assert.Equal(decoded.Value.PngHeight, inspected.Value.PngHeight);
        Assert.Equal(expectedMetrics.HasNonBlankPixels, inspected.Value.HasNonBlankPixels);
        Assert.Equal(expectedMetrics.NonWhitePixelCount, inspected.Value.NonWhitePixelCount);
        Assert.Equal(expectedMetrics.DistinctColorCount, inspected.Value.DistinctColorCount);
        Assert.Equal(expectedMetrics.HasNonBlankPixels, decoded.Value.HasNonBlankPixels);
        Assert.Equal(expectedMetrics.NonWhitePixelCount, decoded.Value.NonWhitePixelCount);
        Assert.Equal(expectedMetrics.DistinctColorCount, decoded.Value.DistinctColorCount);
    }

    private static byte[] PngWithNativePixels(
        int width,
        int height,
        byte colorType,
        int bytesPerPixel,
        byte[] nativePixels)
    {
        var stride = checked(width * bytesPerPixel);
        if (nativePixels.Length != checked(stride * height))
            throw new ArgumentException("Native PNG pixels do not match dimensions.", nameof(nativePixels));

        var scanlines = new byte[checked(height * (1 + stride))];
        for (var y = 0; y < height; y++)
        {
            var sourceRow = checked(y * stride);
            var outputRow = checked(y * (stride + 1));
            scanlines[outputRow] = 0;
            nativePixels.AsSpan(sourceRow, stride).CopyTo(scanlines.AsSpan(outputRow + 1, stride));
        }

        return Png(width, height, colorType, scanlines);
    }

    private static RgbaMetricsForTest MeasureRgbaPixelsForTest(ReadOnlySpan<byte> rgbaPixels)
    {
        if (rgbaPixels.Length % 4 != 0)
            throw new ArgumentException("RGBA pixels must be a multiple of four bytes.", nameof(rgbaPixels));

        var nonWhite = 0;
        var colors = new HashSet<int>();
        var totalPixels = rgbaPixels.Length / 4;
        for (var offset = 0; offset < rgbaPixels.Length; offset += 4)
        {
            var r = rgbaPixels[offset];
            var g = rgbaPixels[offset + 1];
            var b = rgbaPixels[offset + 2];
            var a = rgbaPixels[offset + 3];
            if (a > 0 && (r != 255 || g != 255 || b != 255))
                nonWhite++;
            if (colors.Count < 1024)
                colors.Add((a << 24) | (r << 16) | (g << 8) | b);
        }

        var hasNonBlankPixels = nonWhite > 0 && (
            nonWhite == totalPixels ||
            nonWhite >= Math.Max(2, (totalPixels + 1023) / 1024));
        return new RgbaMetricsForTest(
            HasNonBlankPixels: hasNonBlankPixels,
            NonWhitePixelCount: nonWhite,
            DistinctColorCount: colors.Count);
    }

    private static byte[] ValidPng()
        => RgbaPng(2, 1, [0, 0, 255, 255, 255, 0, 128, 0, 255]);

    private static byte[] BlankPng()
        => RgbaPng(2, 1, [0, 255, 255, 255, 255, 255, 255, 255, 255]);

    private static byte[] SingleNonWhitePixelRgbaPng(int width, int height)
    {
        var scanlines = new byte[checked(height * (1 + width * 4))];
        var offset = 0;
        for (var y = 0; y < height; y++)
        {
            scanlines[offset++] = 0;
            for (var x = 0; x < width; x++)
            {
                scanlines[offset++] = 255;
                scanlines[offset++] = 255;
                scanlines[offset++] = 255;
                scanlines[offset++] = 255;
            }
        }

        var firstPixel = 1;
        scanlines[firstPixel] = 0;
        scanlines[firstPixel + 1] = 0;
        scanlines[firstPixel + 2] = 0;
        scanlines[firstPixel + 3] = 255;
        return RgbaPng(width, height, scanlines);
    }

    private static byte[] SolidRgbaPng(int width, int height, byte red, byte green, byte blue, byte alpha)
    {
        var scanlines = new byte[checked(height * (1 + width * 4))];
        var offset = 0;
        for (var y = 0; y < height; y++)
        {
            scanlines[offset++] = 0;
            for (var x = 0; x < width; x++)
            {
                scanlines[offset++] = red;
                scanlines[offset++] = green;
                scanlines[offset++] = blue;
                scanlines[offset++] = alpha;
            }
        }

        return RgbaPng(width, height, scanlines);
    }

    private static byte[] FilteredRgbaPng(byte filter)
    {
        const int width = 3;
        const int height = 2;
        byte[] pixels =
        [
            10, 20, 30, 255,
            40, 50, 60, 255,
            70, 80, 90, 255,
            90, 80, 70, 255,
            60, 50, 40, 255,
            30, 20, 10, 255
        ];

        return RgbaPng(width, height, EncodeRgbaScanlines(width, height, pixels, filter));
    }

    private static byte[] EncodeRgbaScanlines(int width, int height, ReadOnlySpan<byte> pixels, byte filter)
    {
        var stride = checked(width * 4);
        var scanlines = new byte[checked(height * (1 + stride))];
        for (var y = 0; y < height; y++)
        {
            var sourceRow = checked(y * stride);
            var outputRow = checked(y * (stride + 1));
            scanlines[outputRow] = filter;
            for (var x = 0; x < stride; x++)
            {
                var actual = pixels[sourceRow + x];
                var left = x >= 4 ? pixels[sourceRow + x - 4] : (byte)0;
                var up = y > 0 ? pixels[sourceRow - stride + x] : (byte)0;
                var upLeft = y > 0 && x >= 4 ? pixels[sourceRow - stride + x - 4] : (byte)0;
                var predict = filter switch
                {
                    0 => 0,
                    1 => left,
                    2 => up,
                    3 => (left + up) / 2,
                    4 => PaethPredictor(left, up, upLeft),
                    _ => 0
                };

                scanlines[outputRow + 1 + x] = unchecked((byte)(actual - predict));
            }
        }

        return scanlines;
    }

    private static int PaethPredictor(int left, int up, int upLeft)
    {
        var estimate = left + up - upLeft;
        var leftDistance = Math.Abs(estimate - left);
        var upDistance = Math.Abs(estimate - up);
        var upLeftDistance = Math.Abs(estimate - upLeft);
        if (leftDistance <= upDistance && leftDistance <= upLeftDistance)
            return left;
        return upDistance <= upLeftDistance ? up : upLeft;
    }

    private static byte[] TwoColorRgbaPng(int width, int height)
        => RgbaPng(width, height, TwoColorRgbaScanlines(width, height));

    private static byte[] TwoColorRgbaScanlines(int width, int height)
    {
        var scanlines = new byte[checked(height * (1 + width * 4))];
        var offset = 0;
        for (var y = 0; y < height; y++)
        {
            scanlines[offset++] = 0;
            for (var x = 0; x < width; x++)
            {
                var alternate = ((x + y) & 1) == 1;
                scanlines[offset++] = alternate ? (byte)245 : (byte)15;
                scanlines[offset++] = alternate ? (byte)158 : (byte)118;
                scanlines[offset++] = alternate ? (byte)11 : (byte)110;
                scanlines[offset++] = 255;
            }
        }

        return scanlines;
    }

    private static byte[] RgbaPng(int width, int height, byte[] filteredScanlines)
        => Png(width, height, colorType: 6, filteredScanlines);

    private static byte[] Png(int width, int height, byte colorType, byte[] filteredScanlines)
    {
        using var png = new MemoryStream();
        var signature = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
        png.Write(signature, 0, signature.Length);
        WritePngChunk(png, "IHDR", PngIhdr(width, height, colorType));
        WritePngChunk(png, "IDAT", CompressZlib(filteredScanlines));
        WritePngChunk(png, "IEND", Array.Empty<byte>());
        return png.ToArray();
    }

    private static byte[] RgbaPngWithSplitIdat(
        int width,
        int height,
        byte[] filteredScanlines,
        bool insertAncillaryBetweenIdats,
        params int[] splitLengths)
        => RgbaPngWithCompressedIdat(
            width,
            height,
            CompressZlib(filteredScanlines),
            insertAncillaryBetweenIdats,
            splitLengths);

    private static byte[] RgbaPngWithCompressedIdat(
        int width,
        int height,
        byte[] compressed,
        bool insertAncillaryBetweenIdats,
        params int[] splitLengths)
    {
        using var png = new MemoryStream();
        var signature = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
        png.Write(signature, 0, signature.Length);
        WritePngChunk(png, "IHDR", PngIhdr(width, height));

        var cursor = 0;
        var chunkIndex = 0;
        foreach (var requestedLength in splitLengths)
        {
            if (cursor >= compressed.Length)
                break;

            var length = Math.Min(requestedLength, compressed.Length - cursor);
            WritePngChunk(png, "IDAT", compressed.AsSpan(cursor, length).ToArray());
            cursor += length;
            chunkIndex++;
            if (insertAncillaryBetweenIdats && chunkIndex == 2 && cursor < compressed.Length)
                WritePngChunk(png, "tEXt", Encoding.ASCII.GetBytes("proof\0split-idat"));
        }

        if (cursor < compressed.Length)
            WritePngChunk(png, "IDAT", compressed.AsSpan(cursor).ToArray());

        WritePngChunk(png, "IEND", Array.Empty<byte>());
        return png.ToArray();
    }

    private static RenderQaResolution[] Pass159Viewports()
        =>
        [
            new("mobile-375x812@2x", 375, 812, 2.0),
            new("tablet-768x1024@2x", 768, 1024, 2.0),
            new("desktop-1440x900@1x", 1440, 900, 1.0),
            new("wide-1920x1080@1x", 1920, 1080, 1.0)
        ];

    private static string Sha256File(string path)
        => "sha256:" + Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    private static string Sha256FileRaw(string path)
        => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    private static string Sha256Bytes(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static RenderQaImageRecord WriteComparisonImage(
        string artifactRoot,
        string role,
        string imageId,
        RenderQaResolution resolution,
        byte[] pngBytes,
        bool useContentRef)
    {
        var directory = Path.Combine(artifactRoot, "comparison-inputs", role);
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, $"{imageId}.png");
        File.WriteAllBytes(path, pngBytes);
        var sha = Sha256FileRaw(path);
        var evidence = DesignConversionBrowserPngEvidenceInspector.Inspect(path);
        Assert.True(evidence.IsSuccess, evidence.ErrorMessage);
        return new RenderQaImageRecord(
            ImageId: imageId,
            Label: role == "expected" ? "expected-design-render" : "browser-render-screenshot",
            Resolution: resolution,
            Uri: useContentRef ? $"local-png-ref:sha256:{sha}" : new Uri(path).AbsoluteUri,
            Sha256: $"sha256:{sha}",
            Metadata: new Dictionary<string, string>
            {
                ["media_type"] = "image/png",
                ["content_type"] = "image/png",
                ["png_width"] = evidence.Value!.PngWidth.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["png_height"] = evidence.Value.PngHeight.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["viewport_width"] = resolution.Width.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["viewport_height"] = resolution.Height.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["device_scale_factor"] = resolution.DeviceScaleFactor.ToString("R", System.Globalization.CultureInfo.InvariantCulture)
            });
    }

    private static RenderQaImageRecord ComparisonImageFromPath(
        string role,
        string imageId,
        RenderQaResolution resolution,
        string path,
        bool useContentRef)
    {
        var sha = Sha256FileRaw(path);
        var evidence = DesignConversionBrowserPngEvidenceInspector.Inspect(path);
        Assert.True(evidence.IsSuccess, evidence.ErrorMessage);
        return new RenderQaImageRecord(
            ImageId: imageId,
            Label: role == "expected" ? "expected-design-render" : "browser-render-screenshot",
            Resolution: resolution,
            Uri: useContentRef ? $"local-png-ref:sha256:{sha}" : new Uri(path).AbsoluteUri,
            Sha256: $"sha256:{sha}",
            Metadata: new Dictionary<string, string>
            {
                ["media_type"] = "image/png",
                ["content_type"] = "image/png",
                ["png_width"] = evidence.Value!.PngWidth.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["png_height"] = evidence.Value.PngHeight.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["viewport_width"] = resolution.Width.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["viewport_height"] = resolution.Height.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["device_scale_factor"] = resolution.DeviceScaleFactor.ToString("R", System.Globalization.CultureInfo.InvariantCulture)
            });
    }

    private static byte[] MinimalPngHeader(int width, int height)
    {
        var header = new byte[33];
        var signature = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
        signature.CopyTo(header, 0);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(8, 4), 13);
        Encoding.ASCII.GetBytes("IHDR").CopyTo(header, 12);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(16, 4), width);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(20, 4), height);
        header[24] = 8;
        header[25] = 6;
        return header;
    }

    private static byte[] PngWithExtraDecompressedPixelData()
    {
        using var png = new MemoryStream();
        var signature = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
        png.Write(signature, 0, signature.Length);

        WritePngChunk(png, "IHDR", PngIhdr(1, 1));
        WritePngChunk(png, "IDAT", CompressZlib(new byte[] { 0, 255, 255, 255, 255, 0 }));
        WritePngChunk(png, "IEND", Array.Empty<byte>());
        return png.ToArray();
    }

    private static byte[] PngWithEmptyIdat()
    {
        using var png = new MemoryStream();
        var signature = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
        png.Write(signature, 0, signature.Length);

        WritePngChunk(png, "IHDR", PngIhdr(1, 1));
        WritePngChunk(png, "IDAT", Array.Empty<byte>());
        WritePngChunk(png, "IEND", Array.Empty<byte>());
        return png.ToArray();
    }

    private static byte[] PngWithHugePositiveIdatLengthWithoutPayload()
    {
        using var png = new MemoryStream();
        var signature = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
        png.Write(signature, 0, signature.Length);

        WritePngChunk(png, "IHDR", PngIhdr(1, 1));

        Span<byte> lengthBytes = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(lengthBytes, 0x7ffffff0);
        png.Write(lengthBytes);
        var typeBytes = Encoding.ASCII.GetBytes("IDAT");
        png.Write(typeBytes, 0, typeBytes.Length);
        png.Write(new byte[4], 0, 4);
        return png.ToArray();
    }

    private static byte[] PngIhdr(int width, int height)
        => PngIhdr(width, height, colorType: 6);

    private static byte[] PngIhdr(int width, int height, byte colorType)
    {
        var data = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(0, 4), width);
        BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(4, 4), height);
        data[8] = 8;
        data[9] = colorType;
        return data;
    }

    private sealed record TestPngFixture(byte[] PngBytes, byte[] ExpectedRgbaPixels);

    private sealed record RgbaMetricsForTest(
        bool HasNonBlankPixels,
        int NonWhitePixelCount,
        int DistinctColorCount);

    private static byte[] CompressZlib(byte[] bytes)
    {
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.SmallestSize, leaveOpen: true))
            zlib.Write(bytes, 0, bytes.Length);
        return compressed.ToArray();
    }

    private static void WritePngChunk(Stream stream, string type, byte[] data)
    {
        Span<byte> lengthBytes = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(lengthBytes, data.Length);
        stream.Write(lengthBytes);

        var typeBytes = Encoding.ASCII.GetBytes(type);
        stream.Write(typeBytes, 0, typeBytes.Length);
        stream.Write(data, 0, data.Length);

        var crcInput = new byte[typeBytes.Length + data.Length];
        typeBytes.CopyTo(crcInput, 0);
        data.CopyTo(crcInput, typeBytes.Length);
        Span<byte> crcBytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crcBytes, Crc32(crcInput));
        stream.Write(crcBytes);
    }

    private static uint Crc32(ReadOnlySpan<byte> bytes)
    {
        var crc = 0xffffffffu;
        foreach (var value in bytes)
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++)
                crc = (crc & 1) == 1 ? 0xedb88320u ^ (crc >> 1) : crc >> 1;
        }

        return ~crc;
    }

    private static SubmittedDesignConversionRequest CreatePass166SubmittedRequest()
    {
        var renders = new[]
        {
            Pass166ExpectedRender("pass166-mobile", 390, 844),
            Pass166ExpectedRender("pass166-tablet", 768, 1024),
            Pass166ExpectedRender("pass166-desktop", 1440, 900),
            Pass166ExpectedRender("pass166-wide", 1920, 1080)
        };

        return new SubmittedDesignConversionRequest(
            RequestId: "codex-pass166-browser-backed-runtime-request",
            SourceKind: DesignConversionSourceKind.Figma,
            Html: """
                <main class="pass166-private" data-secret="PRIVATE_PASS166_SECRET">
                  <section class="hero">
                    <p class="eyebrow">Pass166</p>
                    <h1>Browser backed runtime proof</h1>
                    <p>Generated HTML and CSS render through the local browser adapter.</p>
                  </section>
                  <section class="grid">
                    <article>mobile</article>
                    <article>tablet</article>
                    <article>desktop</article>
                    <article>wide</article>
                  </section>
                </main>
                """,
            Css: """
                html,body{margin:0;min-height:100%;font-family:Arial,sans-serif;background:#f8fafc;color:#102033}
                .pass166-private{min-height:100vh;display:grid;align-content:center;gap:24px;padding:32px;background:linear-gradient(135deg,#f8fafc 0%,#d9f99d 44%,#bae6fd 100%)}
                .hero{border:4px solid #0f766e;background:#ffffff;box-shadow:0 16px 40px rgba(16,32,51,.18);padding:28px}
                .eyebrow{margin:0 0 8px;color:#be123c;font-weight:700;text-transform:uppercase}
                h1{margin:0;font-size:42px}
                p{font-size:20px;line-height:1.45}
                .grid{display:grid;grid-template-columns:repeat(4,1fr);gap:12px}
                article{min-height:86px;display:grid;place-items:center;background:#1e3a8a;color:white;font-weight:700}
                @media (max-width: 700px){.pass166-private{padding:18px}.grid{grid-template-columns:1fr}h1{font-size:30px}p{font-size:17px}}
                """,
            DesignTreePayload: new DesignTreePayload(
                PayloadId: "pass166-design-tree",
                Format: "figma-json",
                Content: "{\"private\":\"PRIVATE_PASS166_SECRET\",\"kind\":\"generated\"}",
                Sha256: $"sha256:{Sha256Text("pass166-design-tree")}",
                Metadata: new Dictionary<string, string>
                {
                    ["private_marker"] = "PRIVATE_PASS166_SECRET"
                }),
            OriginalTreePayload: new DesignTreePayload(
                PayloadId: "pass166-original-tree",
                Format: "figma-json",
                Content: "{\"private\":\"PRIVATE_PASS166_SECRET\",\"kind\":\"original\"}",
                Sha256: $"sha256:{Sha256Text("pass166-original-tree")}",
                Metadata: new Dictionary<string, string>
                {
                    ["private_marker"] = "PRIVATE_PASS166_SECRET"
                }),
            Screenshots: [],
            DesignRenders: renders,
            Budget: new DesignConversionBudgetMetadata(
                UserId: "pass166-local-user-ref",
                Tier: "pass166-browser-local-runtime",
                MaxImprovementCandidates: 4,
                MaxRenderQaRuns: 4,
                BudgetUsd: 1.00m,
                Metadata: new Dictionary<string, string>
                {
                    ["quality_threshold"] = "0.90",
                    ["external_provider_payload"] = "external-provider-payload"
                }),
            Provenance: new Dictionary<string, string>
            {
                ["boundary"] = "codex-pass166-browser-backed-runtime-path",
                ["source_manifest_ref"] = "artifact://pass166/source-manifest",
                ["external_provider_payload"] = "external-provider-payload"
            });
    }

    private static DesignRenderArtifact Pass166ExpectedRender(string id, int width, int height)
        => new(
            ArtifactId: id,
            Kind: "design-render",
            Width: width,
            Height: height,
            Uri: $"artifact://pass166/expected/{id}.png",
            Sha256: $"sha256:{Sha256Text(id)}",
            Metadata: new Dictionary<string, string>
            {
                ["expected_design_export_ref"] = bool.TrueString,
                ["refs_only"] = bool.TrueString
            });

    private static string Sha256Text(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static Func<DateTimeOffset> FixedClock()
    {
        var ticks = 0;
        return () => DateTimeOffset.UnixEpoch.AddSeconds(ticks++);
    }

    private static void AssertBrowserRuntimeUnavailableOrSlowBlocker<T>(OperationResult<T> result)
    {
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ErrorCode, BrowserRuntimeBlockerErrorCodes);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    private static void AssertBrowserRuntimeBlockerStatus(DesignConversionThinClientStatusResult status)
    {
        Assert.Equal(DesignConversionThinClientStatus.Failed, status.ContractStatus);
        Assert.Contains("browser", status.Message, StringComparison.OrdinalIgnoreCase);
        var errorCodeWarning = Assert.Single(
            status.Warnings,
            warning => warning.StartsWith("internal_evaluator_error_code=", StringComparison.Ordinal));
        var errorCode = errorCodeWarning["internal_evaluator_error_code=".Length..];
        Assert.Contains(errorCode, BrowserRuntimeBlockerErrorCodes);
        Assert.True(status.Provenance.TryGetValue("internal_evaluator_error_code", out var provenanceErrorCode));
        Assert.Equal(errorCode, provenanceErrorCode);
        Assert.Contains($"{DesignConversionThinClientJobRuntime.BrowserRuntimeBlockerProvenanceKey}=true", status.Warnings);
        Assert.Contains(
            $"{DesignConversionThinClientJobRuntime.BrowserRuntimeBlockerErrorCodeProvenanceKey}={errorCode}",
            status.Warnings);
        Assert.True(status.Provenance.TryGetValue(DesignConversionThinClientJobRuntime.BrowserRuntimeBlockerProvenanceKey, out var browserBlocker));
        Assert.Equal(bool.TrueString, browserBlocker);
        Assert.True(status.Provenance.TryGetValue(DesignConversionThinClientJobRuntime.BrowserRuntimeBlockerErrorCodeProvenanceKey, out var browserErrorCode));
        Assert.Equal(errorCode, browserErrorCode);
        Assert.Equal(
            $"{DesignConversionThinClientJobRuntime.BrowserRuntimeBlockerSafeMessage} error_code={errorCode}.",
            status.Message);
        Assert.DoesNotContain("local_artifact_db_persistence_verified=true", status.Warnings);
    }

    private static void AssertBrowserScreenshotFailureStatus(DesignConversionThinClientStatusResult status)
    {
        Assert.Equal(DesignConversionThinClientStatus.Failed, status.ContractStatus);
        Assert.Equal(LocalChromiumBrowserProcessAdapter.ScreenshotFailedCode, status.Job.Metadata["error_code"]);
        Assert.Equal(
            $"{DesignConversionThinClientJobRuntime.BrowserScreenshotFailureSafeMessage} " +
            $"error_code={LocalChromiumBrowserProcessAdapter.ScreenshotFailedCode}.",
            status.Message);
        Assert.Contains(
            $"internal_evaluator_error_code={LocalChromiumBrowserProcessAdapter.ScreenshotFailedCode}",
            status.Warnings);
        Assert.Equal(
            LocalChromiumBrowserProcessAdapter.ScreenshotFailedCode,
            status.Provenance["internal_evaluator_error_code"]);
        Assert.DoesNotContain(
            $"{DesignConversionThinClientJobRuntime.BrowserRuntimeBlockerProvenanceKey}=true",
            status.Warnings);
        Assert.DoesNotContain(
            status.Warnings,
            warning => warning.StartsWith(
                $"{DesignConversionThinClientJobRuntime.BrowserRuntimeBlockerErrorCodeProvenanceKey}=",
                StringComparison.Ordinal));
        Assert.False(status.Provenance.ContainsKey(DesignConversionThinClientJobRuntime.BrowserRuntimeBlockerProvenanceKey));
        Assert.False(status.Provenance.ContainsKey(DesignConversionThinClientJobRuntime.BrowserRuntimeBlockerErrorCodeProvenanceKey));
        Assert.DoesNotContain("local_artifact_db_persistence_verified=true", status.Warnings);
    }

    private static void AssertThinClientStatusDoesNotContain(
        DesignConversionThinClientStatusResult status,
        params string[] forbiddenValues)
    {
        var haystacks = new List<string>
        {
            status.Message
        };
        haystacks.AddRange(status.Warnings);
        haystacks.AddRange(status.Provenance.Keys);
        haystacks.AddRange(status.Provenance.Values);
        haystacks.AddRange(status.Job.Metadata.Keys);
        haystacks.AddRange(status.Job.Metadata.Values);
        haystacks.AddRange(status.Artifacts.Select(artifact => artifact.Uri));
        haystacks.AddRange(status.Artifacts.SelectMany(artifact => artifact.Metadata.Keys));
        haystacks.AddRange(status.Artifacts.SelectMany(artifact => artifact.Metadata.Values));

        foreach (var forbiddenValue in forbiddenValues.Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            foreach (var haystack in haystacks)
            {
                Assert.DoesNotContain(forbiddenValue, haystack, StringComparison.Ordinal);
            }
        }
    }

    private static void AssertBrowserCaptureResultDoesNotLeakSecrets(
        OperationResult<DesignConversionBrowserCaptureResult> result,
        params string[] secrets)
    {
        var haystacks = new List<string>
        {
            result.ErrorCode ?? string.Empty,
            result.ErrorMessage ?? string.Empty
        };

        if (result.Value is { } capture)
        {
            haystacks.Add(capture.ScreenshotUri);
            haystacks.Add(capture.ScreenshotSha256);
            haystacks.AddRange(capture.Warnings);
            haystacks.AddRange(capture.Metadata.Keys);
            haystacks.AddRange(capture.Metadata.Values);
        }

        foreach (var secret in secrets.Where(secret => !string.IsNullOrWhiteSpace(secret)))
        {
            foreach (var haystack in haystacks)
                Assert.DoesNotContain(secret, haystack, StringComparison.Ordinal);
        }
    }

    private static (int Index, string Content) ExtractFirstCspContent(string documentHtml)
    {
        const string CspMetaPrefix = "<meta http-equiv=\"Content-Security-Policy\" content=\"";
        var cspIndex = documentHtml.IndexOf(CspMetaPrefix, StringComparison.Ordinal);
        Assert.True(cspIndex >= 0, "Captured browser document must contain a CSP meta tag.");

        var contentStart = cspIndex + CspMetaPrefix.Length;
        var contentEnd = documentHtml.IndexOf('"', contentStart);
        Assert.True(contentEnd > contentStart, "Captured CSP meta tag must have a bounded content value.");
        return (cspIndex, documentHtml[contentStart..contentEnd]);
    }

    private static int CountOccurrences(string value, string marker, StringComparison comparison)
    {
        var count = 0;
        var searchIndex = 0;
        while (searchIndex < value.Length)
        {
            var index = value.IndexOf(marker, searchIndex, comparison);
            if (index < 0)
                return count;

            count++;
            searchIndex = index + marker.Length;
        }

        return count;
    }

    private static void AssertStrictOfflineCspPrecedesCallerContent(string documentHtml, string callerMarker)
    {
        const string CspMetaPrefix = "<meta http-equiv=\"Content-Security-Policy\" content=\"";
        var cspIndex = documentHtml.IndexOf(CspMetaPrefix, StringComparison.Ordinal);
        var callerIndex = documentHtml.IndexOf(callerMarker, StringComparison.Ordinal);
        Assert.True(cspIndex >= 0, "Persisted browser document must contain a CSP meta tag.");
        Assert.True(callerIndex >= 0, "Persisted browser document must contain the caller marker after the injected policy.");
        Assert.True(cspIndex < callerIndex, "Injected CSP must precede caller-controlled content.");

        var contentStart = cspIndex + CspMetaPrefix.Length;
        var contentEnd = documentHtml.IndexOf('"', contentStart);
        Assert.True(contentEnd > contentStart, "Persisted CSP meta tag must have a bounded content value.");
        var csp = documentHtml[contentStart..contentEnd];

        foreach (var directive in new[]
        {
            "default-src 'none'",
            "base-uri 'none'",
            "form-action 'none'",
            "frame-src 'none'",
            "child-src 'none'",
            "object-src 'none'",
            "script-src 'none'",
            "connect-src 'none'",
            "worker-src 'none'",
            "manifest-src 'none'",
            "prefetch-src 'none'",
            "style-src 'unsafe-inline'",
            "style-src-elem 'unsafe-inline'",
            "style-src-attr 'unsafe-inline'",
            "img-src data: blob:",
            "font-src data:",
            "media-src data: blob:"
        })
        {
            Assert.Contains(directive, csp, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("file:", csp, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("http:", csp, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("https:", csp, StringComparison.OrdinalIgnoreCase);

        var callerSuppliedMetaIndex = documentHtml.IndexOf("default-src * file:", StringComparison.Ordinal);
        Assert.True(callerSuppliedMetaIndex > cspIndex, "Caller-supplied CSP must not precede the injected restrictive CSP.");
    }

    private static ProcessStartInfo InvokeLocalChromiumBuildStartInfo(
        string executablePath,
        string htmlPath,
        string screenshotPath,
        string userDataPath,
        RenderQaResolution resolution)
    {
        var method = typeof(LocalChromiumBrowserProcessAdapter).GetMethod(
            "BuildStartInfo",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var startInfo = method.Invoke(
            null,
            [executablePath, htmlPath, screenshotPath, userDataPath, resolution]);
        return Assert.IsType<ProcessStartInfo>(startInfo);
    }

    private static string InvokeLocalChromiumBuildStrictOfflineCaptureDocument(string documentHtml)
    {
        var method = typeof(LocalChromiumBrowserProcessAdapter).GetMethod(
            "BuildStrictOfflineCaptureDocument",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var document = method.Invoke(null, [documentHtml]);
        return Assert.IsType<string>(document);
    }

    private static OperationResult<bool> InvokeLocalChromiumSensitiveCleanup(
        string captureRoot,
        string htmlPath,
        string userDataPath)
    {
        var method = typeof(LocalChromiumBrowserProcessAdapter).GetMethod(
            "CleanupSensitiveBrowserCaptureWorkspace",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var cleanup = method.Invoke(null, [captureRoot, htmlPath, userDataPath]);
        return Assert.IsType<OperationResult<bool>>(cleanup);
    }

    private static OperationResult<bool> InvokeLocalChromiumFailedWorkspaceCleanup(
        string baseRoot,
        string captureRoot)
    {
        var method = typeof(LocalChromiumBrowserProcessAdapter).GetMethod(
            "CleanupFailedBrowserCaptureWorkspace",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var cleanup = method.Invoke(null, [baseRoot, captureRoot]);
        return Assert.IsType<OperationResult<bool>>(cleanup);
    }

    private static void AssertSensitiveCleanupFailureDoesNotLeak(
        OperationResult<bool> failure,
        params string[] secrets)
    {
        Assert.False(failure.IsSuccess);
        Assert.Equal("BROWSER_CAPTURE_SENSITIVE_CLEANUP_FAILED", failure.ErrorCode);

        foreach (var secret in secrets.Where(secret => !string.IsNullOrWhiteSpace(secret)))
        {
            Assert.DoesNotContain(secret, failure.ErrorMessage, StringComparison.Ordinal);
            Assert.DoesNotContain(secret, failure.ErrorCode, StringComparison.Ordinal);
        }
    }

    private static async Task<bool> AcceptLoopbackConnectionAsync(
        TcpListener listener,
        CancellationToken cancellationToken)
    {
        try
        {
            using var client = await listener.AcceptTcpClientAsync(cancellationToken);
            return true;
        }
        catch (Exception ex) when (ex is OperationCanceledException or SocketException or ObjectDisposedException)
        {
            return false;
        }
    }

    private static async Task<bool> WaitForLoopbackConnectionProbeAsync(
        Task<bool> connectionTask,
        TimeSpan timeout)
    {
        var completed = await Task.WhenAny(connectionTask, Task.Delay(timeout));
        return completed == connectionTask && await connectionTask;
    }

    private static readonly string[] BrowserRuntimeBlockerErrorCodes =
    [
        "BROWSER_RUNTIME_UNAVAILABLE",
        LocalChromiumBrowserProcessAdapter.RuntimeCrashedBeforeScreenshotCode,
        "BROWSER_SCREENSHOT_TIMEOUT"
    ];

    private sealed record FakeLocalChromiumBrowserProcessRunRequest(
        string ExecutablePath,
        string HtmlPath,
        string ScreenshotPath,
        string UserDataPath,
        RenderQaResolution Resolution,
        TimeSpan Timeout);

    private sealed class FakeLocalChromiumBrowserProcessRunner : ILocalChromiumBrowserProcessRunner
    {
        private readonly object _sync = new();
        private readonly Queue<Func<
            FakeLocalChromiumBrowserProcessRunRequest,
            CancellationToken,
            Task<OperationResult<LocalChromiumBrowserProcessRunResult>>>> _handlers;
        private readonly List<FakeLocalChromiumBrowserProcessRunRequest> _requests = [];

        public FakeLocalChromiumBrowserProcessRunner(
            params Func<
                FakeLocalChromiumBrowserProcessRunRequest,
                CancellationToken,
                Task<OperationResult<LocalChromiumBrowserProcessRunResult>>>[] handlers)
        {
            _handlers = new Queue<Func<
                FakeLocalChromiumBrowserProcessRunRequest,
                CancellationToken,
                Task<OperationResult<LocalChromiumBrowserProcessRunResult>>>>(handlers);
        }

        public IReadOnlyList<FakeLocalChromiumBrowserProcessRunRequest> Requests
        {
            get
            {
                lock (_sync)
                    return _requests.ToArray();
            }
        }

        public Task<OperationResult<LocalChromiumBrowserProcessRunResult>> CaptureAsync(
            string executablePath,
            string htmlPath,
            string screenshotPath,
            string userDataPath,
            RenderQaResolution resolution,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            var request = new FakeLocalChromiumBrowserProcessRunRequest(
                executablePath,
                htmlPath,
                screenshotPath,
                userDataPath,
                resolution,
                timeout);
            Func<
                FakeLocalChromiumBrowserProcessRunRequest,
                CancellationToken,
                Task<OperationResult<LocalChromiumBrowserProcessRunResult>>> handler;

            lock (_sync)
            {
                _requests.Add(request);
                if (_handlers.Count == 0)
                {
                    return Task.FromResult(OperationResult<LocalChromiumBrowserProcessRunResult>.Fail(
                        "Fake browser process handler was not configured.",
                        "FAKE_BROWSER_PROCESS_HANDLER_MISSING"));
                }

                handler = _handlers.Dequeue();
            }

            return handler(request, cancellationToken);
        }
    }

    private sealed class AlwaysFailingWorkspaceCleaner : LocalChromiumBrowserProcessAdapter.IWorkspaceCleaner
    {
        public int FailedWorkspaceCleanupCalls { get; private set; }

        public OperationResult<bool> CleanupSensitiveBrowserCaptureWorkspace(
            string captureRoot,
            string htmlPath,
            string userDataPath)
            => SensitiveCleanupFailureForTest();

        public OperationResult<bool> CleanupFailedBrowserCaptureWorkspace(
            string baseRoot,
            string captureRoot)
        {
            FailedWorkspaceCleanupCalls++;
            return SensitiveCleanupFailureForTest();
        }
    }

    private sealed class ScriptedWorkspaceFileSystem(
        string captureRoot,
        string htmlPath,
        string profilePath,
        bool rootIsReparsePoint = false,
        bool profileReappearsAfterDelete = false)
        : LocalChromiumBrowserProcessAdapter.IWorkspaceFileSystem
    {
        private readonly string _captureRoot = NormalizeTestPath(captureRoot);
        private readonly string _htmlPath = NormalizeTestPath(htmlPath);
        private readonly string _profilePath = NormalizeTestPath(profilePath);
        private readonly Dictionary<string, int> _deleteFileCalls = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> _deleteDirectoryCalls = new(StringComparer.OrdinalIgnoreCase);

        public bool HtmlExists { get; private set; } = true;

        public bool ProfileExists { get; private set; } = true;

        public bool RootExists { get; private set; } = true;

        public int FileExistsCalls { get; private set; }

        public int DirectoryExistsCalls { get; private set; }

        public int GetDirectoryAttributesCalls { get; private set; }

        public int TotalFileSystemCalls =>
            FileExistsCalls +
            DirectoryExistsCalls +
            GetDirectoryAttributesCalls +
            _deleteFileCalls.Values.Sum() +
            _deleteDirectoryCalls.Values.Sum();

        public bool FileExists(string path)
        {
            FileExistsCalls++;
            return SameTestPath(path, _htmlPath) && HtmlExists;
        }

        public bool DirectoryExists(string path)
        {
            DirectoryExistsCalls++;
            if (SameTestPath(path, _profilePath))
                return ProfileExists;
            if (SameTestPath(path, _captureRoot))
                return RootExists;

            return false;
        }

        public FileAttributes GetDirectoryAttributes(string path)
        {
            GetDirectoryAttributesCalls++;
            Assert.True(SameTestPath(path, _captureRoot), path);
            return rootIsReparsePoint
                ? FileAttributes.Directory | FileAttributes.ReparsePoint
                : FileAttributes.Directory;
        }

        public void DeleteFile(string path)
        {
            Increment(_deleteFileCalls, path);
            if (SameTestPath(path, _htmlPath))
                HtmlExists = false;
        }

        public void DeleteDirectory(string path, bool recursive)
        {
            Assert.True(recursive);
            Increment(_deleteDirectoryCalls, path);
            if (SameTestPath(path, _profilePath))
            {
                ProfileExists = profileReappearsAfterDelete;
                return;
            }

            if (SameTestPath(path, _captureRoot))
            {
                RootExists = profileReappearsAfterDelete;
                ProfileExists = profileReappearsAfterDelete;
                HtmlExists = false;
            }
        }

        public void ReappearProfile()
        {
            RootExists = true;
            ProfileExists = true;
        }

        public int DeleteFileCallsFor(string path)
            => CallsFor(_deleteFileCalls, path);

        public int DeleteDirectoryCallsFor(string path)
            => CallsFor(_deleteDirectoryCalls, path);

        private static void Increment(IDictionary<string, int> callsByPath, string path)
        {
            var normalized = NormalizeTestPath(path);
            callsByPath.TryGetValue(normalized, out var calls);
            callsByPath[normalized] = calls + 1;
        }

        private static int CallsFor(IReadOnlyDictionary<string, int> callsByPath, string path)
        {
            callsByPath.TryGetValue(NormalizeTestPath(path), out var calls);
            return calls;
        }
    }

    private sealed class TransientDeleteDirectoryFailureFileSystem(string transientFailurePath, int failuresBeforeSuccess)
        : LocalChromiumBrowserProcessAdapter.IWorkspaceFileSystem
    {
        private readonly string _transientFailurePath = NormalizeTestPath(transientFailurePath);
        private readonly Dictionary<string, int> _deleteDirectoryCalls = new(StringComparer.OrdinalIgnoreCase);

        public bool FileExists(string path)
            => File.Exists(path);

        public bool DirectoryExists(string path)
            => Directory.Exists(path);

        public FileAttributes GetDirectoryAttributes(string path)
        {
            var directory = new DirectoryInfo(path);
            directory.Refresh();
            return directory.Attributes;
        }

        public void DeleteFile(string path)
            => File.Delete(path);

        public void DeleteDirectory(string path, bool recursive)
        {
            var normalized = NormalizeTestPath(path);
            _deleteDirectoryCalls.TryGetValue(normalized, out var calls);
            calls++;
            _deleteDirectoryCalls[normalized] = calls;

            if (SameTestPath(normalized, _transientFailurePath) && calls <= failuresBeforeSuccess)
            {
                if (calls % 2 == 0)
                    throw new UnauthorizedAccessException("Simulated transient browser profile lock.");
                throw new IOException("Simulated transient browser profile lock.");
            }

            Directory.Delete(path, recursive);
        }

        public int DeleteDirectoryCallsFor(string path)
        {
            _deleteDirectoryCalls.TryGetValue(NormalizeTestPath(path), out var calls);
            return calls;
        }
    }

    private static OperationResult<bool> SensitiveCleanupFailureForTest()
        => OperationResult<bool>.Fail(
            "Browser capture sensitive workspace cleanup failed.",
            LocalChromiumBrowserProcessAdapter.SensitiveCleanupFailedCode);

    private static string NormalizeTestPath(string path)
        => Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));

    private static bool SameTestPath(string left, string right)
        => string.Equals(
            NormalizeTestPath(left),
            NormalizeTestPath(right),
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    private sealed class FakeRemotePngDnsResolver : IDesignConversionRemotePngDnsResolver
    {
        private readonly Dictionary<string, IPAddress[]> _addresses;

        public FakeRemotePngDnsResolver()
            : this(new Dictionary<string, IPAddress[]>())
        {
        }

        public FakeRemotePngDnsResolver(Dictionary<string, IPAddress[]> addresses)
            => _addresses = new Dictionary<string, IPAddress[]>(addresses, StringComparer.OrdinalIgnoreCase);

        public static IPAddress PublicAddress { get; } = IPAddress.Parse("93.184.216.34");

        public static FakeRemotePngDnsResolver PublicOnly(params string[] hosts)
            => new(hosts.ToDictionary(
                host => host,
                _ => new[] { PublicAddress },
                StringComparer.OrdinalIgnoreCase));

        public List<string> Queries { get; } = [];

        public Task<OperationResult<IReadOnlyList<IPAddress>>> ResolveAsync(
            string host,
            CancellationToken cancellationToken = default)
        {
            Queries.Add(host);
            if (_addresses.TryGetValue(host, out var addresses))
                return Task.FromResult(OperationResult<IReadOnlyList<IPAddress>>.Ok(addresses));

            return Task.FromResult(OperationResult<IReadOnlyList<IPAddress>>.Fail(
                "Fake DNS host not configured.",
                "FAKE_DNS_HOST_MISSING"));
        }
    }

    private sealed class FakeRemotePngHttpTransport : IDesignConversionRemotePngHttpTransport
    {
        private readonly Queue<Func<DesignConversionRemotePngHttpRequest, DesignConversionRemotePngHttpResponse>> _responses;
        private readonly bool _throwTimeout;

        public FakeRemotePngHttpTransport(
            params Func<DesignConversionRemotePngHttpRequest, DesignConversionRemotePngHttpResponse>[] responses)
        {
            _responses = new Queue<Func<DesignConversionRemotePngHttpRequest, DesignConversionRemotePngHttpResponse>>(responses);
        }

        public FakeRemotePngHttpTransport(
            bool throwTimeout,
            params Func<DesignConversionRemotePngHttpRequest, DesignConversionRemotePngHttpResponse>[] responses)
            : this(responses)
        {
            _throwTimeout = throwTimeout;
        }

        public List<DesignConversionRemotePngHttpRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionRemotePngHttpResponse>> SendAsync(
            DesignConversionRemotePngHttpRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Requests.Add(request);
            if (_throwTimeout)
                throw new OperationCanceledException();
            if (_responses.Count == 0)
                return Task.FromResult(OperationResult<DesignConversionRemotePngHttpResponse>.Fail(
                    "Fake HTTP response not configured.",
                    "FAKE_HTTP_RESPONSE_MISSING"));

            return Task.FromResult(OperationResult<DesignConversionRemotePngHttpResponse>.Ok(
                _responses.Dequeue()(request)));
        }
    }

    private sealed class RecordingBrowserAdapter(bool useChromiumPhysicalDimensions = true) : IDesignConversionBrowserProcessAdapter
    {
        public List<DesignConversionBrowserCaptureRequest> Requests { get; } = [];

        public async Task<OperationResult<DesignConversionBrowserCaptureResult>> CaptureScreenshotAsync(
            DesignConversionBrowserCaptureRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var pngResolution = request.Resolution;
            if (useChromiumPhysicalDimensions)
            {
                var physicalResolution = DesignConversionBrowserPhysicalScreenshotResolution.Resolve(
                    request.Resolution,
                    "FAKE_BROWSER_CAPTURE_RESOLUTION_INVALID",
                    "Fake browser capture");
                if (!physicalResolution.IsSuccess)
                    return physicalResolution.MapError<DesignConversionBrowserCaptureResult>();

                pngResolution = new RenderQaResolution(
                    $"{request.Resolution.ResolutionId}:physical-png",
                    physicalResolution.Value!.PhysicalWidth,
                    physicalResolution.Value.PhysicalHeight,
                    1.0);
            }

            var render = await new DeterministicLocalDesignConversionPngRenderProvider(Path.Combine(
                    Path.GetTempPath(),
                    "llm_mvp_core",
                    "design-conversion-recording-browser-adapter",
                    Guid.NewGuid().ToString("N")))
                .RenderAsync(
                    new DesignConversionRenderRequest(
                        RenderRequestId: $"{request.CaptureRequestId}:recording-browser-png",
                        RequestId: request.RequestId,
                        Html: request.DocumentHtml,
                        Css: "body{margin:0}.recording-browser-proof{color:#123456;background:#f8fafc}",
                        Resolution: pngResolution,
                        Metadata: request.Metadata),
                    cancellationToken);

            if (!render.IsSuccess)
                return render.MapError<DesignConversionBrowserCaptureResult>();

            var sha = render.Value!.Image.Sha256["sha256:".Length..];
            return OperationResult<DesignConversionBrowserCaptureResult>.Ok(
                new DesignConversionBrowserCaptureResult(
                    ScreenshotUri: render.Value.Image.Uri,
                    ScreenshotSha256: sha,
                    ElapsedMs: 7,
                    Metadata: new Dictionary<string, string>
                    {
                        ["browser_adapter_kind"] = "recording_browser_process",
                        ["browser_process_boundary"] = "adapter_supplied_inspectable_png_file"
                    },
                    Warnings: []));
        }
    }

    private sealed class ScriptedBrowserAdapter(
        params Func<DesignConversionBrowserCaptureRequest, CancellationToken, Task<OperationResult<DesignConversionBrowserCaptureResult>>>[] steps)
        : IDesignConversionBrowserProcessAdapter
    {
        private readonly Queue<Func<DesignConversionBrowserCaptureRequest, CancellationToken, Task<OperationResult<DesignConversionBrowserCaptureResult>>>> _steps =
            new(steps);

        public List<DesignConversionBrowserCaptureRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionBrowserCaptureResult>> CaptureScreenshotAsync(
            DesignConversionBrowserCaptureRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return _steps.Count == 0
                ? Task.FromResult(OperationResult<DesignConversionBrowserCaptureResult>.Fail(
                    "Fake browser capture sequence exhausted.",
                    "FAKE_BROWSER_CAPTURE_SEQUENCE_EXHAUSTED"))
                : _steps.Dequeue()(request, cancellationToken);
        }

        public static Func<DesignConversionBrowserCaptureRequest, CancellationToken, Task<OperationResult<DesignConversionBrowserCaptureResult>>> Timeout(
            string message)
            => Failure(message, "BROWSER_SCREENSHOT_TIMEOUT");

        public static Func<DesignConversionBrowserCaptureRequest, CancellationToken, Task<OperationResult<DesignConversionBrowserCaptureResult>>> Failure(
            string message,
            string code)
            => (_, _) => Task.FromResult(OperationResult<DesignConversionBrowserCaptureResult>.Fail(message, code));

        public static Func<DesignConversionBrowserCaptureRequest, CancellationToken, Task<OperationResult<DesignConversionBrowserCaptureResult>>> ThrowsNonCallerOperationCanceled()
            => (_, _) => throw new OperationCanceledException();

        public static Func<DesignConversionBrowserCaptureRequest, CancellationToken, Task<OperationResult<DesignConversionBrowserCaptureResult>>> ValidLocalPng()
            => (request, cancellationToken) => new RecordingBrowserAdapter()
                .CaptureScreenshotAsync(request, cancellationToken);
    }

    private sealed class DeclaredShaBrowserAdapter(string artifactRoot, string declaredSha)
        : IDesignConversionBrowserProcessAdapter
    {
        public async Task<OperationResult<DesignConversionBrowserCaptureResult>> CaptureScreenshotAsync(
            DesignConversionBrowserCaptureRequest request,
            CancellationToken cancellationToken = default)
        {
            var physicalResolution = DesignConversionBrowserPhysicalScreenshotResolution.Resolve(
                request.Resolution,
                "FAKE_BROWSER_CAPTURE_RESOLUTION_INVALID",
                "Fake browser capture");
            if (!physicalResolution.IsSuccess)
                return physicalResolution.MapError<DesignConversionBrowserCaptureResult>();

            var captureRoot = Path.Combine(artifactRoot, Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(captureRoot);
            var screenshotPath = Path.Combine(captureRoot, "screenshot.png");
            var bytes = TwoColorRgbaPng(
                physicalResolution.Value!.PhysicalWidth,
                physicalResolution.Value.PhysicalHeight);
            await File.WriteAllBytesAsync(screenshotPath, bytes, cancellationToken);

            return OperationResult<DesignConversionBrowserCaptureResult>.Ok(
                new DesignConversionBrowserCaptureResult(
                    ScreenshotUri: new Uri(screenshotPath).AbsoluteUri,
                    ScreenshotSha256: declaredSha,
                    ElapsedMs: 5,
                    Metadata: new Dictionary<string, string>
                    {
                        ["browser_adapter_kind"] = "declared_sha_fake_browser_process",
                        ["browser_process_boundary"] = "adapter_supplied_declared_sha_local_png_file"
                    },
                    Warnings: []));
        }
    }

    private sealed class ReceiptBackedRemoteUriRenderProvider(
        string artifactRoot,
        string screenshotUri,
        bool mutateSnapshotBeforeReturn = false) : IDesignConversionRenderProvider
    {
        public string? SnapshotPath { get; private set; }
        public DesignConversionBrowserRenderProofReceipt? LastReceipt { get; private set; }
        public DesignConversionRenderResult? LastRenderResult { get; private set; }

        public Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
        {
            Directory.CreateDirectory(artifactRoot);
            SnapshotPath = Path.GetFullPath(Path.Combine(
                artifactRoot,
                $"{Guid.NewGuid():N}.png"));
            var bytes = TwoColorRgbaPng(
                request.Resolution.Width,
                request.Resolution.Height);
            File.WriteAllBytes(SnapshotPath, bytes);

            var evidence = DesignConversionBrowserPngEvidenceInspector.Inspect(SnapshotPath);
            if (!evidence.IsSuccess)
                return Task.FromResult(evidence.MapError<DesignConversionRenderResult>());

            var pngEvidence = evidence.Value!;
            var sha = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
            var imageId = $"{request.RenderRequestId}:receipt-backed-image:{sha[..12]}";
            var renderResultId = $"{request.RenderRequestId}:receipt-backed-render:{sha[..12]}";
            var metadata = BrowserReceiptMetadata(request, pngEvidence);
            var image = new RenderQaImageRecord(
                ImageId: imageId,
                Label: "browser-render-screenshot",
                Resolution: request.Resolution,
                Uri: screenshotUri,
                Sha256: $"sha256:{sha}",
                Metadata: metadata);

            LastReceipt = new DesignConversionBrowserRenderProofReceipt(
                IssuerKind: BrowserBackedDesignConversionRenderProvider.ProviderKind,
                RenderResultId: renderResultId,
                RenderRequestId: request.RenderRequestId,
                RequestId: request.RequestId,
                ImageId: imageId,
                ScreenshotUri: screenshotUri,
                NormalizedSha256: sha,
                ByteLength: pngEvidence.ByteLength,
                PngWidth: pngEvidence.PngWidth,
                PngHeight: pngEvidence.PngHeight,
                PngSignatureVerified: true,
                Sha256Verified: true,
                HasNonBlankPixels: pngEvidence.HasNonBlankPixels,
                NonWhitePixelCount: pngEvidence.NonWhitePixelCount,
                DistinctColorCount: pngEvidence.DistinctColorCount,
                ResolutionId: request.Resolution.ResolutionId,
                ViewportWidth: request.Resolution.Width,
                ViewportHeight: request.Resolution.Height,
                DeviceScaleFactor: request.Resolution.DeviceScaleFactor)
            {
                InspectedLocalPath = pngEvidence.LocalPath
            };

            LastRenderResult = new DesignConversionRenderResult(
                RenderResultId: renderResultId,
                RenderRequestId: request.RenderRequestId,
                Image: image,
                ElapsedMs: 11,
                Warnings: [])
            {
                BrowserRenderProofReceipt = LastReceipt
            };

            if (mutateSnapshotBeforeReturn)
                File.WriteAllBytes(SnapshotPath, [137, 80, 78, 71]);

            return Task.FromResult(OperationResult<DesignConversionRenderResult>.Ok(LastRenderResult));
        }
    }

    private sealed class MarkerOnlyRemoteUriRenderProvider(string screenshotUri) : IDesignConversionRenderProvider
    {
        public Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
        {
            var sha = Sha256Text($"{request.RenderRequestId}|{request.Resolution.ResolutionId}|marker-only");
            var image = new RenderQaImageRecord(
                ImageId: $"{request.RenderRequestId}:marker-only-image:{sha[..12]}",
                Label: "browser-render-screenshot",
                Resolution: request.Resolution,
                Uri: screenshotUri,
                Sha256: $"sha256:{sha}",
                Metadata: BrowserReceiptMetadata(
                    request,
                    new DesignConversionBrowserPngEvidence(
                        LocalPath: string.Empty,
                        ByteLength: 2048,
                        PngWidth: request.Resolution.Width,
                        PngHeight: request.Resolution.Height,
                        HasNonBlankPixels: true,
                        NonWhitePixelCount: 10,
                        DistinctColorCount: 2)));
            return Task.FromResult(OperationResult<DesignConversionRenderResult>.Ok(new DesignConversionRenderResult(
                RenderResultId: $"{request.RenderRequestId}:marker-only-render:{sha[..12]}",
                RenderRequestId: request.RenderRequestId,
                Image: image,
                ElapsedMs: 3,
                Warnings: [])));
        }
    }

    private static Dictionary<string, string> BrowserReceiptMetadata(
        DesignConversionRenderRequest request,
        DesignConversionBrowserPngEvidence pngEvidence)
        => new(StringComparer.Ordinal)
        {
            ["provider_kind"] = BrowserBackedDesignConversionRenderProvider.ProviderKind,
            ["render_artifact_kind"] = "browser_screenshot",
            ["visual_fidelity_claimed"] = bool.TrueString,
            ["browser_or_playwright_used"] = bool.TrueString,
            ["external_ai_used"] = bool.FalseString,
            ["hidden_determinism_boundary"] = "browser_backed_render_evidence",
            ["browser_screenshot_file_verified"] = bool.TrueString,
            ["browser_screenshot_png_signature_verified"] = bool.TrueString,
            ["browser_screenshot_sha256_verified"] = bool.TrueString,
            ["browser_screenshot_nonblank_pixels_verified"] = bool.TrueString,
            ["png_signature_verified"] = bool.TrueString,
            ["browser_screenshot_byte_length"] = pngEvidence.ByteLength.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["browser_screenshot_png_width"] = pngEvidence.PngWidth.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["browser_screenshot_png_height"] = pngEvidence.PngHeight.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["browser_screenshot_non_white_pixel_count"] = pngEvidence.NonWhitePixelCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["browser_screenshot_distinct_color_count"] = pngEvidence.DistinctColorCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["viewport_id"] = request.Resolution.ResolutionId,
            ["viewport_width"] = request.Resolution.Width.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["viewport_height"] = request.Resolution.Height.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["device_scale_factor"] = request.Resolution.DeviceScaleFactor.ToString("R", System.Globalization.CultureInfo.InvariantCulture)
        };

    private sealed class MetadataOnlyBrowserAdapter : IDesignConversionBrowserProcessAdapter
    {
        public Task<OperationResult<DesignConversionBrowserCaptureResult>> CaptureScreenshotAsync(
            DesignConversionBrowserCaptureRequest request,
            CancellationToken cancellationToken = default)
        {
            var sha = Sha256Text(string.Join(
                "|",
                request.CaptureRequestId,
                request.DocumentHtml,
                request.Resolution.ResolutionId,
                request.Resolution.Width,
                request.Resolution.Height,
                request.Resolution.DeviceScaleFactor));

            return Task.FromResult(OperationResult<DesignConversionBrowserCaptureResult>.Ok(
                new DesignConversionBrowserCaptureResult(
                    ScreenshotUri: $"artifact://browser-screenshot/{sha}.png",
                    ScreenshotSha256: sha,
                    ElapsedMs: 1,
                    Metadata: new Dictionary<string, string>
                    {
                        ["browser_adapter_kind"] = "metadata_only_fake_browser_process",
                        ["fake_browser_boundary"] = "adapter_contract_only_no_png_file"
                    },
                    Warnings: [])));
        }
    }

    private sealed class RemoteScreenshotBrowserAdapter(string screenshotUri) : IDesignConversionBrowserProcessAdapter
    {
        public Task<OperationResult<DesignConversionBrowserCaptureResult>> CaptureScreenshotAsync(
            DesignConversionBrowserCaptureRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(OperationResult<DesignConversionBrowserCaptureResult>.Ok(
                new DesignConversionBrowserCaptureResult(
                    ScreenshotUri: screenshotUri,
                    ScreenshotSha256: Sha256Text($"{request.CaptureRequestId}|remote-screenshot"),
                    ElapsedMs: 1,
                    Metadata: new Dictionary<string, string>
                    {
                        ["browser_adapter_kind"] = "remote_screenshot_browser_process",
                        ["browser_process_boundary"] = "adapter_supplied_remote_png_uri"
                    },
                    Warnings: [])));
    }

    private sealed class MalformedLocalPngBrowserAdapter : IDesignConversionBrowserProcessAdapter
    {
        public string? ScreenshotPath { get; private set; }

        public Task<OperationResult<DesignConversionBrowserCaptureResult>> CaptureScreenshotAsync(
            DesignConversionBrowserCaptureRequest request,
            CancellationToken cancellationToken = default)
        {
            ScreenshotPath = Path.Combine(
                Path.GetTempPath(),
                $"malformed-browser-screenshot-{Guid.NewGuid():N}.png");
            var bytes = PngWithExtraDecompressedPixelData();
            File.WriteAllBytes(ScreenshotPath, bytes);
            var sha = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

            return Task.FromResult(OperationResult<DesignConversionBrowserCaptureResult>.Ok(
                new DesignConversionBrowserCaptureResult(
                    ScreenshotUri: new Uri(ScreenshotPath).AbsoluteUri,
                    ScreenshotSha256: sha,
                    ElapsedMs: 1,
                    Metadata: new Dictionary<string, string>
                    {
                        ["browser_adapter_kind"] = "malformed_local_png_browser_process",
                        ["browser_process_boundary"] = "adapter_supplied_malformed_local_png_file"
                    },
                    Warnings: [])));
        }
    }

    private sealed class LocalPngBrowserAdapter(
        string adapterKind,
        Func<int, int, byte[]> createPng) : IDesignConversionBrowserProcessAdapter
    {
        public string? ScreenshotPath { get; private set; }

        public Task<OperationResult<DesignConversionBrowserCaptureResult>> CaptureScreenshotAsync(
            DesignConversionBrowserCaptureRequest request,
            CancellationToken cancellationToken = default)
        {
            var physicalResolution = DesignConversionBrowserPhysicalScreenshotResolution.Resolve(
                request.Resolution,
                "FAKE_BROWSER_CAPTURE_RESOLUTION_INVALID",
                "Fake browser capture");
            if (!physicalResolution.IsSuccess)
                return Task.FromResult(physicalResolution.MapError<DesignConversionBrowserCaptureResult>());

            ScreenshotPath = Path.Combine(
                Path.GetTempPath(),
                $"{adapterKind}-{Guid.NewGuid():N}.png");
            var bytes = createPng(
                physicalResolution.Value!.PhysicalWidth,
                physicalResolution.Value.PhysicalHeight);
            File.WriteAllBytes(ScreenshotPath, bytes);
            var sha = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

            return Task.FromResult(OperationResult<DesignConversionBrowserCaptureResult>.Ok(
                new DesignConversionBrowserCaptureResult(
                    ScreenshotUri: new Uri(ScreenshotPath).AbsoluteUri,
                    ScreenshotSha256: sha,
                    ElapsedMs: 1,
                    Metadata: new Dictionary<string, string>
                    {
                        ["browser_adapter_kind"] = adapterKind,
                        ["browser_process_boundary"] = "adapter_supplied_local_png_file"
                    },
                    Warnings: [])));
        }
    }

    private sealed class FailingBrowserAdapter(string message, string code) : IDesignConversionBrowserProcessAdapter
    {
        public Task<OperationResult<DesignConversionBrowserCaptureResult>> CaptureScreenshotAsync(
            DesignConversionBrowserCaptureRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(OperationResult<DesignConversionBrowserCaptureResult>.Fail(message, code));
    }

    private sealed class RecordingEvaluator(IDesignConversionThinClientJobEvaluator inner) : IDesignConversionThinClientJobEvaluator
    {
        public List<DesignConversionInternalEvaluateRequest> Requests { get; } = [];
        public List<OperationResult<DesignConversionInternalEvaluationResult>> Responses { get; } = [];

        public async Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var response = await inner.EvaluateAsync(request, cancellationToken);
            Responses.Add(response);
            return response;
        }
    }

    private sealed class FixedFailureEvaluator(string message, string errorCode) : IDesignConversionThinClientJobEvaluator
    {
        public List<DesignConversionInternalEvaluateRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(OperationResult<DesignConversionInternalEvaluationResult>.Fail(message, errorCode));
        }
    }

    private sealed class Pass166LocalComparisonProvider(double score) : IDesignConversionComparisonProvider
    {
        public List<DesignConversionQaComparisonRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var diffSha = Sha256Text(string.Join(
                "|",
                request.ComparisonRequestId,
                request.Expected.Sha256,
                request.Actual.Sha256,
                request.Expected.Resolution.Width,
                request.Expected.Resolution.Height,
                request.Actual.Resolution.Width,
                request.Actual.Resolution.Height,
                score));
            var verdict = new VisualVerdict(
                VerdictId: $"{request.ComparisonRequestId}:pass166-local-verdict",
                Score: score,
                Defects: [],
                EvidenceRefs: [$"pass166-local-comparison:{request.ComparisonRequestId}"],
                Passed: true);

            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Ok(
                new DesignConversionQaComparisonArtifact(
                    ArtifactId: $"{request.ComparisonRequestId}:pass166-local-comparison-artifact",
                    ComparisonRequestId: request.ComparisonRequestId,
                    Expected: request.Expected,
                    Actual: request.Actual,
                    Verdict: verdict,
                    DiffImageUri: $"artifact://pass166/local-comparison/{diffSha}.json",
                    DiffImageSha256: $"sha256:{diffSha}",
                    Metrics: new Dictionary<string, double>
                    {
                        ["visual_distance"] = Math.Max(0.0, 1.0 - score)
                    },
                    EvidenceRefs: ["pass166-local-comparison-provider-no-external-ai"])
                {
                    Metadata = new Dictionary<string, string>
                    {
                        ["comparison_verdict_score"] = score.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture),
                        ["comparison_verdict_passed"] = bool.TrueString,
                        ["quality_threshold"] = "0.90",
                        ["comparison_provider"] = "pass166_local_visual_labeler_wrapper",
                        ["external_ai_visual_judge_executed"] = bool.FalseString,
                        ["visual_ai_executed"] = bool.FalseString,
                        ["real_external_provider_executed"] = bool.FalseString,
                        ["network_call_executed"] = bool.FalseString,
                        ["raw_html_included"] = bool.FalseString,
                        ["raw_css_included"] = bool.FalseString,
                        ["raw_design_tree_included"] = bool.FalseString,
                        ["provider_payload_included"] = bool.FalseString
                    }
                }));
        }
    }
}
