// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Buffers;
using System.Buffers.Binary;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;

public sealed record DesignConversionBrowserCaptureRequest(
    string CaptureRequestId,
    string RequestId,
    string DocumentHtml,
    RenderQaResolution Resolution,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignConversionBrowserCaptureResult(
    string ScreenshotUri,
    string ScreenshotSha256,
    long ElapsedMs,
    IReadOnlyDictionary<string, string> Metadata,
    IReadOnlyList<string> Warnings);

public sealed record DesignConversionBrowserRenderManifestRequest(
    string RenderRunId,
    string RequestId,
    string Html,
    string Css,
    IReadOnlyList<RenderQaResolution> RequiredResolutions,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignConversionBrowserPngEvidence(
    string LocalPath,
    long ByteLength,
    int PngWidth,
    int PngHeight,
    bool HasNonBlankPixels,
    int NonWhitePixelCount,
    int DistinctColorCount) : IDisposable
{
    public string SnapshotSha256 { get; init; } = string.Empty;

    private RemoteSnapshotOwnershipToken? OwnershipToken { get; init; }

    internal DesignConversionBrowserPngEvidence WithOwnedRemoteSnapshot()
        => this with { OwnershipToken = new RemoteSnapshotOwnershipToken(LocalPath) };

    public void Dispose()
        => OwnershipToken?.Dispose();

    private sealed class RemoteSnapshotOwnershipToken(string ownedLocalPath)
    {
        private string? _ownedLocalPath = ownedLocalPath;

        internal void Dispose()
        {
            var path = Interlocked.Exchange(ref _ownedLocalPath, null);
            if (path is not null)
                _ = DesignConversionBrowserPngEvidenceInspector.CleanupOwnedRemoteEvidenceTempFile(path);
        }
    }
}

internal sealed record DesignConversionDecodedPngComparisonEvidence(
    string LocalPath,
    long ByteLength,
    int PngWidth,
    int PngHeight,
    bool HasNonBlankPixels,
    int NonWhitePixelCount,
    int DistinctColorCount,
    string Sha256,
    byte[] RgbaPixels);

public static class DesignConversionBoundedContentReader
{
    private const int BufferSize = 81920;
    public const long HardMaxBytes = 64L * 1024 * 1024;
    public const string DefaultSizeErrorCode = "DESIGN_CONVERSION_BOUNDED_CONTENT_SIZE_INVALID";
    public const string HardLimitErrorCode = "DESIGN_CONVERSION_BOUNDED_CONTENT_LIMIT_TOO_LARGE";

    public static OperationResult<byte[]> ReadToEnd(
        Stream stream,
        long maxBytes,
        string description,
        string sizeErrorCode = DefaultSizeErrorCode,
        string changedErrorCode = DefaultSizeErrorCode)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));
        if (maxBytes > HardMaxBytes)
        {
            return OperationResult<byte[]>.Fail(
                "Bounded content byte limit exceeds the configured hard ceiling.",
                HardLimitErrorCode);
        }
        if (maxBytes <= 0)
        {
            return OperationResult<byte[]>.Fail(
                $"{description} byte limit must be positive.",
                sizeErrorCode);
        }

        long? expectedLength = null;
        if (stream.CanSeek)
        {
            expectedLength = stream.Length - stream.Position;
            if (expectedLength <= 0 || expectedLength > maxBytes)
            {
                return OperationResult<byte[]>.Fail(
                    $"{description} exceeded the bounded byte size.",
                    sizeErrorCode);
            }
        }

        using var bytes = expectedLength is > 0
            ? new MemoryStream(capacity: (int)Math.Min(expectedLength.Value, BufferSize))
            : new MemoryStream();
        var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
        try
        {
            long totalRead = 0;
            while (true)
            {
                var remaining = maxBytes - totalRead;
                var readLength = remaining >= buffer.Length
                    ? buffer.Length
                    : checked((int)remaining + 1);
                var read = stream.Read(buffer, 0, readLength);
                if (read == 0)
                    break;

                if (read > remaining)
                {
                    return OperationResult<byte[]>.Fail(
                        $"{description} exceeded the bounded byte size.",
                        sizeErrorCode);
                }

                bytes.Write(buffer, 0, read);
                totalRead += read;
            }

            if (totalRead <= 0)
            {
                return OperationResult<byte[]>.Fail(
                    $"{description} exceeded the bounded byte size.",
                    sizeErrorCode);
            }

            if (expectedLength.HasValue && totalRead != expectedLength.Value)
            {
                return OperationResult<byte[]>.Fail(
                    $"{description} changed while being read.",
                    changedErrorCode);
            }

            return OperationResult<byte[]>.Ok(bytes.ToArray());
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public static async Task<OperationResult<byte[]>> ReadToEndAsync(
        Stream stream,
        long maxBytes,
        string description,
        string sizeErrorCode = DefaultSizeErrorCode,
        string changedErrorCode = DefaultSizeErrorCode,
        CancellationToken cancellationToken = default)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));
        if (maxBytes > HardMaxBytes)
        {
            return OperationResult<byte[]>.Fail(
                "Bounded content byte limit exceeds the configured hard ceiling.",
                HardLimitErrorCode);
        }
        if (maxBytes <= 0)
        {
            return OperationResult<byte[]>.Fail(
                $"{description} byte limit must be positive.",
                sizeErrorCode);
        }

        long? expectedLength = null;
        if (stream.CanSeek)
        {
            expectedLength = stream.Length - stream.Position;
            if (expectedLength <= 0 || expectedLength > maxBytes)
            {
                return OperationResult<byte[]>.Fail(
                    $"{description} exceeded the bounded byte size.",
                    sizeErrorCode);
            }
        }

        using var bytes = expectedLength is > 0
            ? new MemoryStream(capacity: (int)Math.Min(expectedLength.Value, BufferSize))
            : new MemoryStream();
        var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
        try
        {
            long totalRead = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var remaining = maxBytes - totalRead;
                var readLength = remaining >= buffer.Length
                    ? buffer.Length
                    : checked((int)remaining + 1);
                var read = await stream.ReadAsync(buffer, 0, readLength, cancellationToken)
                    .ConfigureAwait(false);
                if (read == 0)
                    break;

                if (read > remaining)
                {
                    return OperationResult<byte[]>.Fail(
                        $"{description} exceeded the bounded byte size.",
                        sizeErrorCode);
                }

                bytes.Write(buffer, 0, read);
                totalRead += read;
            }

            if (totalRead <= 0)
            {
                return OperationResult<byte[]>.Fail(
                    $"{description} exceeded the bounded byte size.",
                    sizeErrorCode);
            }

            if (expectedLength.HasValue && totalRead != expectedLength.Value)
            {
                return OperationResult<byte[]>.Fail(
                    $"{description} changed while being read.",
                    changedErrorCode);
            }

            return OperationResult<byte[]>.Ok(bytes.ToArray());
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}

public sealed record DesignConversionRemotePngFetchPolicy(
    bool AllowHttp,
    bool AllowHttps,
    int MaxRedirects,
    long MaxBytes,
    TimeSpan Timeout);

public sealed record DesignConversionRemotePngHttpRequest(
    Uri Uri,
    IPAddress PinnedAddress,
    TimeSpan Timeout);

public sealed class DesignConversionRemotePngHttpResponse : IDisposable
{
    private readonly IDisposable? _owner;

    public DesignConversionRemotePngHttpResponse(
        int statusCode,
        string? contentType,
        long? contentLength,
        Uri? redirectLocation,
        Stream body,
        IReadOnlyList<string>? contentEncodings = null,
        IDisposable? owner = null)
    {
        StatusCode = statusCode;
        ContentType = contentType;
        ContentLength = contentLength;
        RedirectLocation = redirectLocation;
        Body = body ?? throw new ArgumentNullException(nameof(body));
        ContentEncodings = contentEncodings ?? Array.Empty<string>();
        _owner = owner;
    }

    public int StatusCode { get; }
    public string? ContentType { get; }
    public long? ContentLength { get; }
    public Uri? RedirectLocation { get; }
    public Stream Body { get; }
    public IReadOnlyList<string> ContentEncodings { get; }

    public void Dispose()
    {
        Body.Dispose();
        _owner?.Dispose();
    }
}

public interface IDesignConversionRemotePngDnsResolver
{
    Task<OperationResult<IReadOnlyList<IPAddress>>> ResolveAsync(
        string host,
        CancellationToken cancellationToken = default);
}

public interface IDesignConversionRemotePngHttpTransport
{
    Task<OperationResult<DesignConversionRemotePngHttpResponse>> SendAsync(
        DesignConversionRemotePngHttpRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record DesignConversionBrowserRenderManifestEntry(
    string ViewportId,
    int RequestedWidth,
    int RequestedHeight,
    double DeviceScaleFactor,
    string ScreenshotUri,
    string LocalPath,
    string Sha256,
    long ByteLength,
    int PngWidth,
    int PngHeight,
    bool HasNonBlankPixels,
    int NonWhitePixelCount,
    int DistinctColorCount,
    long ElapsedMs,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignConversionBrowserRenderManifestResult(
    string RenderRunId,
    IReadOnlyList<DesignConversionBrowserRenderManifestEntry> Entries,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> Warnings);

public interface IDesignConversionBrowserProcessAdapter
{
    Task<OperationResult<DesignConversionBrowserCaptureResult>> CaptureScreenshotAsync(
        DesignConversionBrowserCaptureRequest request,
        CancellationToken cancellationToken = default);
}

public interface IDesignConversionBatchBrowserProcessAdapter : IDesignConversionBrowserProcessAdapter
{
    Task<IReadOnlyList<OperationResult<DesignConversionBrowserCaptureResult>>> CaptureScreenshotsAsync(
        IReadOnlyList<DesignConversionBrowserCaptureRequest> requests,
        CancellationToken cancellationToken = default);
}

public sealed record DesignConversionBrowserPhysicalScreenshotResolution(
    int LogicalWidth,
    int LogicalHeight,
    double DeviceScaleFactor,
    int PhysicalWidth,
    int PhysicalHeight,
    long PhysicalPixelCount)
{
    public static OperationResult<DesignConversionBrowserPhysicalScreenshotResolution> Resolve(
        RenderQaResolution? resolution,
        string errorCode,
        string subject)
    {
        if (resolution is null ||
            resolution.Width <= 0 ||
            resolution.Height <= 0 ||
            double.IsNaN(resolution.DeviceScaleFactor) ||
            double.IsInfinity(resolution.DeviceScaleFactor) ||
            resolution.DeviceScaleFactor <= 0)
        {
            return Invalid(resolution, errorCode, subject);
        }

        var scaledWidth = resolution.Width * resolution.DeviceScaleFactor;
        var scaledHeight = resolution.Height * resolution.DeviceScaleFactor;
        if (!TryRoundPhysicalDimension(scaledWidth, out var physicalWidth) ||
            !TryRoundPhysicalDimension(scaledHeight, out var physicalHeight))
        {
            return Invalid(resolution, errorCode, subject);
        }

        long pixelCount;
        try
        {
            pixelCount = checked((long)physicalWidth * physicalHeight);
        }
        catch (OverflowException)
        {
            return Invalid(resolution, errorCode, subject);
        }

        if (pixelCount > DesignConversionBrowserPngEvidenceInspector.MaxPngScreenshotPixels)
            return Invalid(resolution, errorCode, subject);

        return OperationResult<DesignConversionBrowserPhysicalScreenshotResolution>.Ok(new(
            LogicalWidth: resolution.Width,
            LogicalHeight: resolution.Height,
            DeviceScaleFactor: resolution.DeviceScaleFactor,
            PhysicalWidth: physicalWidth,
            PhysicalHeight: physicalHeight,
            PhysicalPixelCount: pixelCount));
    }

    private static bool TryRoundPhysicalDimension(double scaledValue, out int physicalDimension)
    {
        physicalDimension = 0;
        if (double.IsNaN(scaledValue) || double.IsInfinity(scaledValue))
            return false;

        var rounded = Math.Round(scaledValue, MidpointRounding.AwayFromZero);
        if (double.IsNaN(rounded) || double.IsInfinity(rounded) || rounded < 1 || rounded > int.MaxValue)
            return false;

        physicalDimension = (int)rounded;
        return true;
    }

    private static OperationResult<DesignConversionBrowserPhysicalScreenshotResolution> Invalid(
        RenderQaResolution? resolution,
        string errorCode,
        string subject)
        => OperationResult<DesignConversionBrowserPhysicalScreenshotResolution>.Fail(
            string.Join(
                " ",
                $"{subject} resolution is invalid for Chromium screenshot capture.",
                "width=" + (resolution?.Width.ToString(CultureInfo.InvariantCulture) ?? "0"),
                "height=" + (resolution?.Height.ToString(CultureInfo.InvariantCulture) ?? "0"),
                "device_scale_factor=" + (resolution?.DeviceScaleFactor.ToString("R", CultureInfo.InvariantCulture) ?? "0"),
                "max_physical_pixels=" + DesignConversionBrowserPngEvidenceInspector.MaxPngScreenshotPixels.ToString(CultureInfo.InvariantCulture)),
            errorCode);
}

public sealed class BrowserBackedDesignConversionRenderProvider :
    IDesignConversionRenderProvider,
    IDesignConversionBoundedConcurrentRenderProvider,
    IDesignConversionBatchRenderProvider
{
    public const string ProviderKind = "browser_process_render_provider";
    public const string VisualFidelityClaimedMarker = "visual_fidelity_claimed:true";
    public const string BrowserUsedMarker = "browser_or_playwright_used:true";
    public const string ExternalAiUsedMarker = "external_ai_used:false";
    public const string BoundaryMarker = "hidden_determinism_boundary:browser_backed_render_evidence";
    public const string LocalOnlyCspMarker = "browser_document_csp_local_only";
    private const int MaxBrowserDocumentChars = 1_000_000;
    private const int MaxServerOwnedBrowserCaptureAttempts = 2;
    private const string BrowserScreenshotTimeoutCode = "BROWSER_SCREENSHOT_TIMEOUT";
    private const string BrowserCaptureBatchResultEnvelopeNullCode = "BROWSER_CAPTURE_BATCH_RESULT_ENVELOPE_NULL";
    private const string ServerOwnedTimeoutRetrySource = "server_owned_browser_screenshot_timeout_retry";
    private const string BrowserDocumentCspMeta = LocalChromiumBrowserProcessAdapter.StrictOfflineDocumentCspMeta;

    private readonly IDesignConversionBrowserProcessAdapter _browserAdapter;

    public BrowserBackedDesignConversionRenderProvider()
        : this(new LocalChromiumBrowserProcessAdapter())
    {
    }

    public BrowserBackedDesignConversionRenderProvider(IDesignConversionBrowserProcessAdapter browserAdapter)
        => _browserAdapter = browserAdapter ?? throw new ArgumentNullException(nameof(browserAdapter));

    public int MaxConcurrentRenderOperations => LocalChromiumBrowserProcessAdapter.DefaultMaxConcurrentBrowserCaptureProcesses;

    public async Task<IReadOnlyList<OperationResult<DesignConversionRenderResult>>> RenderBatchAsync(
        IReadOnlyList<DesignConversionRenderRequest> requests,
        CancellationToken cancellationToken = default)
    {
        if (requests is null || requests.Count == 0)
            return [];

        if (_browserAdapter is IDesignConversionBatchBrowserProcessAdapter batchAdapter)
            return await RenderBatchWithBatchAdapterAsync(batchAdapter, requests, cancellationToken);

        return await RenderBatchWithPerItemFallbackAsync(requests, cancellationToken);
    }

    private async Task<IReadOnlyList<OperationResult<DesignConversionRenderResult>>> RenderBatchWithPerItemFallbackAsync(
        IReadOnlyList<DesignConversionRenderRequest> requests,
        CancellationToken cancellationToken)
    {
        var results = new OperationResult<DesignConversionRenderResult>[requests.Count];
        var maxConcurrency = Math.Min(requests.Count, Math.Max(1, MaxConcurrentRenderOperations));
        var nextIndex = -1;
        var workers = Enumerable.Range(0, maxConcurrency)
            .Select(_ => RunBatchRenderWorkerAsync(
                requests,
                results,
                () => Interlocked.Increment(ref nextIndex),
                cancellationToken))
            .ToArray();

        try
        {
            await Task.WhenAll(workers);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            FillMissingBatchResults(
                results,
                "DesignConversion browser render batch cancelled.",
                "CANCELLED");
        }
        catch
        {
            FillMissingBatchResults(
                results,
                "DesignConversion browser render batch failed.",
                "EXCEPTION_THROWN");
        }

        return results;
    }

    private async Task<IReadOnlyList<OperationResult<DesignConversionRenderResult>>> RenderBatchWithBatchAdapterAsync(
        IDesignConversionBatchBrowserProcessAdapter batchAdapter,
        IReadOnlyList<DesignConversionRenderRequest> requests,
        CancellationToken cancellationToken)
    {
        var results = new OperationResult<DesignConversionRenderResult>[requests.Count];
        var prepared = new List<(int Index, PreparedBrowserRenderRequest Prepared)>(requests.Count);
        for (var index = 0; index < requests.Count; index++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                results[index] = BrowserRenderBatchCancelled();
                continue;
            }

            var item = PrepareBrowserRenderRequest(requests[index]);
            if (!item.IsSuccess)
            {
                results[index] = item.MapError<DesignConversionRenderResult>();
                continue;
            }

            prepared.Add((index, item.Value!));
        }

        if (prepared.Count == 0)
            return results;

        var captureRequests = prepared.Select(item => item.Prepared.CaptureRequest).ToArray();
        var captures = await CaptureBrowserScreenshotsWithTransientRetryAsync(
            batchAdapter,
            captureRequests,
            cancellationToken);

        for (var preparedIndex = 0; preparedIndex < prepared.Count; preparedIndex++)
        {
            var resultIndex = prepared[preparedIndex].Index;
            if (preparedIndex >= captures.Count)
            {
                results[resultIndex] = OperationResult<DesignConversionRenderResult>.Fail(
                    "DesignConversion browser render batch returned an invalid result envelope.",
                    "BROWSER_CAPTURE_BATCH_RESULT_COUNT_MISMATCH");
                continue;
            }

            var capture = captures[preparedIndex];
            if (!capture.IsSuccess)
            {
                results[resultIndex] = capture.MapError<DesignConversionRenderResult>();
                continue;
            }

            try
            {
                results[resultIndex] = await BuildBrowserRenderResultAsync(
                    prepared[preparedIndex].Prepared,
                    capture.Value!,
                    cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                results[resultIndex] = BrowserRenderBatchCancelled();
            }
            catch
            {
                results[resultIndex] = OperationResult<DesignConversionRenderResult>.Fail(
                    "DesignConversion browser render batch item failed.",
                    "EXCEPTION_THROWN");
            }
        }

        FillMissingBatchResults(
            results,
            "DesignConversion browser render batch failed.",
            "EXCEPTION_THROWN");
        return results;
    }

    public async Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
        DesignConversionRenderRequest request,
        CancellationToken cancellationToken = default)
    {
        var prepared = PrepareBrowserRenderRequest(request, cancellationToken);
        if (!prepared.IsSuccess)
            return prepared.MapError<DesignConversionRenderResult>();
        var capture = await CaptureBrowserScreenshotWithTransientRetryAsync(prepared.Value!.CaptureRequest, cancellationToken);

        if (!capture.IsSuccess)
            return capture.MapError<DesignConversionRenderResult>();

        return await BuildBrowserRenderResultAsync(prepared.Value!, capture.Value!, cancellationToken);
    }

    private static OperationResult<PreparedBrowserRenderRequest> PrepareBrowserRenderRequest(
        DesignConversionRenderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<PreparedBrowserRenderRequest>.Fail(
                "DesignConversion browser render cancelled.",
                "CANCELLED");
        if (request is null)
            return OperationResult<PreparedBrowserRenderRequest>.Fail(
                "DesignConversion render request is required.",
                "RENDER_REQUEST_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.RenderRequestId))
            return OperationResult<PreparedBrowserRenderRequest>.Fail(
                "DesignConversion render request id is required.",
                "RENDER_REQUEST_ID_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.RequestId))
            return OperationResult<PreparedBrowserRenderRequest>.Fail(
                "DesignConversion request id is required.",
                "REQUEST_ID_REQUIRED");
        if (request.Resolution is null)
            return OperationResult<PreparedBrowserRenderRequest>.Fail(
                "DesignConversion render resolution is required.",
                "RENDER_RESOLUTION_REQUIRED");

        var physicalResolution = DesignConversionBrowserPhysicalScreenshotResolution.Resolve(
            request.Resolution,
            "BROWSER_RENDER_RESOLUTION_INVALID",
            "DesignConversion browser render");
        if (!physicalResolution.IsSuccess)
            return physicalResolution.MapError<PreparedBrowserRenderRequest>();

        var documentHtml = BuildBrowserDocument(request.Html ?? string.Empty, request.Css ?? string.Empty);
        if (documentHtml.Length > MaxBrowserDocumentChars)
        {
            return OperationResult<PreparedBrowserRenderRequest>.Fail(
                $"DesignConversion browser render document exceeds the bounded local render limit of {MaxBrowserDocumentChars.ToString(CultureInfo.InvariantCulture)} characters.",
                "BROWSER_RENDER_DOCUMENT_TOO_LARGE");
        }

        return OperationResult<PreparedBrowserRenderRequest>.Ok(new PreparedBrowserRenderRequest(
            RenderRequest: request,
            CaptureRequest: new DesignConversionBrowserCaptureRequest(
                CaptureRequestId: request.RenderRequestId,
                RequestId: request.RequestId,
                DocumentHtml: documentHtml,
                Resolution: request.Resolution,
                Metadata: request.Metadata),
            PhysicalResolution: physicalResolution.Value!,
            HtmlSha256: DeterministicDesignConversionRenderProvider.Sha256Text(request.Html ?? string.Empty),
            CssSha256: DeterministicDesignConversionRenderProvider.Sha256Text(request.Css ?? string.Empty)));
    }

    private static async Task<OperationResult<DesignConversionRenderResult>> BuildBrowserRenderResultAsync(
        PreparedBrowserRenderRequest prepared,
        BrowserCaptureAttemptResult capture,
        CancellationToken cancellationToken)
    {
        var screenshotProof = await ValidateBrowserScreenshotProofAsync(
            capture.Capture,
            prepared.PhysicalResolution,
            cancellationToken);
        if (!screenshotProof.IsSuccess)
            return screenshotProof.MapError<DesignConversionRenderResult>();

        var request = prepared.RenderRequest;
        var proof = screenshotProof.Value!;
        var captureValue = capture.Capture;
        var captureMetadata = BuildServerOwnedCaptureMetadata(captureValue.Metadata, capture);
        var metadata = DeterministicDesignConversionRenderProvider.MergeMetadata(
            request.Metadata,
            captureMetadata
                .Concat(new Dictionary<string, string>
                {
                    ["provider_kind"] = ProviderKind,
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
                    ["browser_document_policy"] = LocalOnlyCspMarker,
                    ["browser_remote_network_allowed"] = bool.FalseString,
                    ["browser_screenshot_byte_length"] = proof.PngEvidence.ByteLength.ToString(CultureInfo.InvariantCulture),
                    ["browser_screenshot_png_width"] = proof.PngEvidence.PngWidth.ToString(CultureInfo.InvariantCulture),
                    ["browser_screenshot_png_height"] = proof.PngEvidence.PngHeight.ToString(CultureInfo.InvariantCulture),
                    ["media_type"] = "image/png",
                    ["content_type"] = "image/png",
                    ["png_width"] = proof.PngEvidence.PngWidth.ToString(CultureInfo.InvariantCulture),
                    ["png_height"] = proof.PngEvidence.PngHeight.ToString(CultureInfo.InvariantCulture),
                    ["browser_screenshot_non_white_pixel_count"] = proof.PngEvidence.NonWhitePixelCount.ToString(CultureInfo.InvariantCulture),
                    ["browser_screenshot_distinct_color_count"] = proof.PngEvidence.DistinctColorCount.ToString(CultureInfo.InvariantCulture),
                    ["product_ready"] = bool.FalseString,
                    ["html_sha256"] = prepared.HtmlSha256,
                    ["css_sha256"] = prepared.CssSha256,
                    ["viewport_id"] = request.Resolution.ResolutionId,
                    ["viewport_width"] = request.Resolution.Width.ToString(CultureInfo.InvariantCulture),
                    ["viewport_height"] = request.Resolution.Height.ToString(CultureInfo.InvariantCulture),
                    ["device_scale_factor"] = request.Resolution.DeviceScaleFactor.ToString("R", CultureInfo.InvariantCulture)
                })
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal));

        var image = new RenderQaImageRecord(
            ImageId: $"{request.RenderRequestId}:browser-image:{proof.Sha256[..12]}",
            Label: "browser-render-screenshot",
            Resolution: request.Resolution,
            Uri: captureValue.ScreenshotUri,
            Sha256: $"sha256:{proof.Sha256}",
            Metadata: metadata);
        var renderResultId = $"{request.RenderRequestId}:browser-render:{proof.Sha256[..12]}";
        var receipt = new DesignConversionBrowserRenderProofReceipt(
            IssuerKind: ProviderKind,
            RenderResultId: renderResultId,
            RenderRequestId: request.RenderRequestId,
            RequestId: request.RequestId,
            ImageId: image.ImageId,
            ScreenshotUri: captureValue.ScreenshotUri,
            NormalizedSha256: proof.Sha256,
            ByteLength: proof.PngEvidence.ByteLength,
            PngWidth: proof.PngEvidence.PngWidth,
            PngHeight: proof.PngEvidence.PngHeight,
            PngSignatureVerified: true,
            Sha256Verified: true,
            HasNonBlankPixels: proof.PngEvidence.HasNonBlankPixels,
            NonWhitePixelCount: proof.PngEvidence.NonWhitePixelCount,
            DistinctColorCount: proof.PngEvidence.DistinctColorCount,
            ResolutionId: request.Resolution.ResolutionId,
            ViewportWidth: request.Resolution.Width,
            ViewportHeight: request.Resolution.Height,
            DeviceScaleFactor: request.Resolution.DeviceScaleFactor)
        {
            InspectedLocalPath = proof.PngEvidence.LocalPath
        };

        return OperationResult<DesignConversionRenderResult>.Ok(new DesignConversionRenderResult(
            RenderResultId: renderResultId,
            RenderRequestId: request.RenderRequestId,
            Image: image,
            ElapsedMs: captureValue.ElapsedMs,
            Warnings: BuildServerOwnedCaptureWarnings(captureValue.Warnings, capture))
        {
            BrowserRenderProofReceipt = receipt
        });
    }

    private async Task RunBatchRenderWorkerAsync(
        IReadOnlyList<DesignConversionRenderRequest> requests,
        OperationResult<DesignConversionRenderResult>[] results,
        Func<int> claimNextIndex,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            var index = claimNextIndex();
            if (index >= requests.Count)
                return;

            results[index] = await RenderBatchItemAsync(requests[index], cancellationToken);
        }
    }

    private async Task<OperationResult<DesignConversionRenderResult>> RenderBatchItemAsync(
        DesignConversionRenderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return BrowserRenderBatchCancelled();

            return await RenderAsync(request, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return BrowserRenderBatchCancelled();
        }
        catch
        {
            return OperationResult<DesignConversionRenderResult>.Fail(
                "DesignConversion browser render batch item failed.",
                "EXCEPTION_THROWN");
        }
    }

    private static void FillMissingBatchResults(
        OperationResult<DesignConversionRenderResult>[] results,
        string message,
        string errorCode)
    {
        for (var index = 0; index < results.Length; index++)
            results[index] ??= OperationResult<DesignConversionRenderResult>.Fail(message, errorCode);
    }

    private static OperationResult<DesignConversionRenderResult> BrowserRenderBatchCancelled()
        => OperationResult<DesignConversionRenderResult>.Fail(
            "DesignConversion browser render batch cancelled.",
            "CANCELLED");

    private async Task<OperationResult<BrowserCaptureAttemptResult>> CaptureBrowserScreenshotWithTransientRetryAsync(
        DesignConversionBrowserCaptureRequest captureRequest,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxServerOwnedBrowserCaptureAttempts; attempt++)
        {
            if (cancellationToken.IsCancellationRequested)
                return BrowserCaptureCancelled();

            OperationResult<DesignConversionBrowserCaptureResult> capture;
            try
            {
                capture = await _browserAdapter.CaptureScreenshotAsync(captureRequest, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return BrowserCaptureCancelled();
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                capture = OperationResult<DesignConversionBrowserCaptureResult>.Fail(
                    "DesignConversion browser screenshot capture timed out.",
                    BrowserScreenshotTimeoutCode);
            }

            if (capture.IsSuccess)
                return OperationResult<BrowserCaptureAttemptResult>.Ok(new BrowserCaptureAttemptResult(
                    Capture: capture.Value!,
                    AttemptCount: attempt,
                    RetriedAfterTimeout: attempt > 1));

            var errorCode = NormalizeBrowserCaptureErrorCode(capture.ErrorCode);
            if (string.Equals(errorCode, "CANCELLED", StringComparison.Ordinal) ||
                cancellationToken.IsCancellationRequested)
            {
                return BrowserCaptureCancelled();
            }

            if (!string.Equals(errorCode, BrowserScreenshotTimeoutCode, StringComparison.Ordinal) ||
                attempt >= MaxServerOwnedBrowserCaptureAttempts)
            {
                return OperationResult<BrowserCaptureAttemptResult>.Fail(
                    BuildSanitizedBrowserCaptureFailureMessage(errorCode, attempt),
                    string.IsNullOrWhiteSpace(errorCode) ? "BROWSER_CAPTURE_FAILED" : errorCode);
            }
        }

        return OperationResult<BrowserCaptureAttemptResult>.Fail(
            "DesignConversion browser render unavailable after a bounded server-owned screenshot timeout retry.",
            BrowserScreenshotTimeoutCode);
    }

    private async Task<IReadOnlyList<OperationResult<BrowserCaptureAttemptResult>>> CaptureBrowserScreenshotsWithTransientRetryAsync(
        IDesignConversionBatchBrowserProcessAdapter batchAdapter,
        IReadOnlyList<DesignConversionBrowserCaptureRequest> captureRequests,
        CancellationToken cancellationToken)
    {
        var results = new OperationResult<BrowserCaptureAttemptResult>[captureRequests.Count];
        var pending = Enumerable.Range(0, captureRequests.Count).ToArray();

        for (var attempt = 1; attempt <= MaxServerOwnedBrowserCaptureAttempts && pending.Length > 0; attempt++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                FillBatchCaptureAttemptResults(
                    results,
                    pending,
                    BrowserCaptureCancelled());
                break;
            }

            IReadOnlyList<OperationResult<DesignConversionBrowserCaptureResult>>? captures;
            try
            {
                var pendingRequests = pending.Select(index => captureRequests[index]).ToArray();
                captures = await batchAdapter.CaptureScreenshotsAsync(pendingRequests, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                FillBatchCaptureAttemptResults(
                    results,
                    pending,
                    BrowserCaptureCancelled());
                break;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                if (attempt < MaxServerOwnedBrowserCaptureAttempts)
                    continue;

                FillBatchCaptureAttemptResults(
                    results,
                    pending,
                    OperationResult<BrowserCaptureAttemptResult>.Fail(
                        BuildSanitizedBrowserCaptureFailureMessage(BrowserScreenshotTimeoutCode, attempt),
                        BrowserScreenshotTimeoutCode));
                break;
            }
            catch
            {
                FillBatchCaptureAttemptResults(
                    results,
                    pending,
                    OperationResult<BrowserCaptureAttemptResult>.Fail(
                        "DesignConversion browser render unavailable.",
                        "BROWSER_CAPTURE_FAILED"));
                break;
            }

            if (captures is null)
            {
                FillBatchCaptureAttemptResults(
                    results,
                    pending,
                    OperationResult<BrowserCaptureAttemptResult>.Fail(
                        "DesignConversion browser render batch returned a null result envelope.",
                        BrowserCaptureBatchResultEnvelopeNullCode));
                break;
            }

            if (captures.Count != pending.Length)
            {
                FillBatchCaptureAttemptResults(
                    results,
                    pending,
                    OperationResult<BrowserCaptureAttemptResult>.Fail(
                        "DesignConversion browser render batch returned an invalid result envelope.",
                        "BROWSER_CAPTURE_BATCH_RESULT_COUNT_MISMATCH"));
                break;
            }

            var retry = new List<int>();
            for (var batchIndex = 0; batchIndex < captures.Count; batchIndex++)
            {
                var resultIndex = pending[batchIndex];
                var capture = captures[batchIndex];
                if (capture is null)
                {
                    results[resultIndex] = OperationResult<BrowserCaptureAttemptResult>.Fail(
                        "DesignConversion browser render batch returned a null result envelope.",
                        BrowserCaptureBatchResultEnvelopeNullCode);
                    continue;
                }

                if (capture.IsSuccess)
                {
                    results[resultIndex] = OperationResult<BrowserCaptureAttemptResult>.Ok(new BrowserCaptureAttemptResult(
                        Capture: capture.Value!,
                        AttemptCount: attempt,
                        RetriedAfterTimeout: attempt > 1));
                    continue;
                }

                var errorCode = NormalizeBrowserCaptureErrorCode(capture.ErrorCode);
                if (string.Equals(errorCode, "CANCELLED", StringComparison.Ordinal) ||
                    cancellationToken.IsCancellationRequested)
                {
                    results[resultIndex] = BrowserCaptureCancelled();
                    continue;
                }

                if (string.Equals(errorCode, BrowserScreenshotTimeoutCode, StringComparison.Ordinal) &&
                    attempt < MaxServerOwnedBrowserCaptureAttempts)
                {
                    retry.Add(resultIndex);
                    continue;
                }

                results[resultIndex] = OperationResult<BrowserCaptureAttemptResult>.Fail(
                    BuildSanitizedBrowserCaptureFailureMessage(errorCode, attempt),
                    string.IsNullOrWhiteSpace(errorCode) ? "BROWSER_CAPTURE_FAILED" : errorCode);
            }

            pending = retry.ToArray();
        }

        FillMissingCaptureAttemptResults(results);
        return results;
    }

    private static void FillBatchCaptureAttemptResults(
        OperationResult<BrowserCaptureAttemptResult>[] results,
        IReadOnlyList<int> pending,
        OperationResult<BrowserCaptureAttemptResult> result)
    {
        foreach (var index in pending)
            results[index] ??= result;
    }

    private static void FillMissingCaptureAttemptResults(
        OperationResult<BrowserCaptureAttemptResult>[] results)
    {
        for (var index = 0; index < results.Length; index++)
        {
            results[index] ??= OperationResult<BrowserCaptureAttemptResult>.Fail(
                "DesignConversion browser render unavailable after a bounded server-owned screenshot timeout retry.",
                BrowserScreenshotTimeoutCode);
        }
    }

    private static OperationResult<BrowserCaptureAttemptResult> BrowserCaptureCancelled()
        => OperationResult<BrowserCaptureAttemptResult>.Fail(
            "DesignConversion browser render capture cancelled.",
            "CANCELLED");

    private static IReadOnlyDictionary<string, string> BuildServerOwnedCaptureMetadata(
        IReadOnlyDictionary<string, string>? adapterMetadata,
        BrowserCaptureAttemptResult capture)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal);
        if (adapterMetadata is not null)
        {
            foreach (var pair in adapterMetadata)
                metadata[pair.Key] = pair.Value;
        }

        metadata["browser_capture_attempt_count"] = capture.AttemptCount.ToString(CultureInfo.InvariantCulture);
        metadata["browser_capture_max_attempts"] = MaxServerOwnedBrowserCaptureAttempts.ToString(CultureInfo.InvariantCulture);
        metadata["browser_capture_retry_applied"] = capture.RetriedAfterTimeout ? bool.TrueString : bool.FalseString;
        metadata["browser_capture_retry_source"] = capture.RetriedAfterTimeout ? ServerOwnedTimeoutRetrySource : "none";
        if (capture.RetriedAfterTimeout)
            metadata["browser_capture_retry_error_code"] = BrowserScreenshotTimeoutCode;
        return metadata;
    }

    private static IReadOnlyList<string> BuildServerOwnedCaptureWarnings(
        IReadOnlyList<string> adapterWarnings,
        BrowserCaptureAttemptResult capture)
    {
        if (!capture.RetriedAfterTimeout)
            return adapterWarnings;

        return (adapterWarnings ?? Array.Empty<string>())
            .Concat(
            [
                "browser_capture_retry_applied:true",
                "browser_capture_attempt_count=" + capture.AttemptCount.ToString(CultureInfo.InvariantCulture),
                "browser_capture_retry_source=" + ServerOwnedTimeoutRetrySource
            ])
            .ToArray();
    }

    private static string NormalizeBrowserCaptureErrorCode(string? errorCode)
        => string.IsNullOrWhiteSpace(errorCode)
            ? string.Empty
            : errorCode.Trim().ToUpperInvariant();

    private static string BuildSanitizedBrowserCaptureFailureMessage(string errorCode, int attemptCount)
    {
        if (string.Equals(errorCode, BrowserScreenshotTimeoutCode, StringComparison.Ordinal) &&
            attemptCount >= MaxServerOwnedBrowserCaptureAttempts)
        {
            return "DesignConversion browser render unavailable after a bounded server-owned screenshot timeout retry.";
        }

        return "DesignConversion browser render unavailable.";
    }

    internal static string BuildBrowserDocument(string html, string css)
    {
        var headMarkup = $"{BrowserDocumentCspMeta}<style>{css}</style>";
        var htmlTagStart = FindOpeningHtmlTagStart(html);
        if (htmlTagStart >= 0)
        {
            var htmlTagEnd = html.IndexOf('>', htmlTagStart);
            if (htmlTagEnd < 0)
                return WrapBrowserDocument(html, headMarkup);

            if (html.Contains("</head>", StringComparison.OrdinalIgnoreCase))
                return html.Replace("</head>", $"{headMarkup}</head>", StringComparison.OrdinalIgnoreCase);

            if (html.Contains("<body", StringComparison.OrdinalIgnoreCase))
                return html.Replace("<body", $"<head>{headMarkup}</head><body", StringComparison.OrdinalIgnoreCase);

            return string.Concat(
                html[..(htmlTagEnd + 1)],
                "<head>",
                headMarkup,
                "</head>",
                html[(htmlTagEnd + 1)..]);
        }

        return WrapBrowserDocument(html, headMarkup);
    }

    private static string WrapBrowserDocument(string html, string headMarkup)
    {
        return string.Concat(
            "<!doctype html><html><head><meta charset=\"utf-8\">",
            "<meta name=\"viewport\" content=\"width=device-width,initial-scale=1\">",
            headMarkup,
            "</head><body>",
            html,
            "</body></html>");
    }

    private static int FindOpeningHtmlTagStart(string html)
    {
        var searchIndex = 0;
        while (searchIndex < html.Length)
        {
            var index = html.IndexOf("<html", searchIndex, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                return -1;

            var boundaryIndex = index + "<html".Length;
            if (boundaryIndex == html.Length || IsHtmlTagNameBoundary(html[boundaryIndex]))
                return index;

            searchIndex = boundaryIndex;
        }

        return -1;
    }

    private static bool IsHtmlTagNameBoundary(char value)
        => char.IsWhiteSpace(value) || value is '>' or '/';

    private static async Task<OperationResult<BrowserScreenshotProof>> ValidateBrowserScreenshotProofAsync(
        DesignConversionBrowserCaptureResult capture,
        DesignConversionBrowserPhysicalScreenshotResolution expectedResolution,
        CancellationToken cancellationToken)
    {
        if (DesignConversionBrowserPngEvidenceInspector.IsRemoteHttpOrHttpsScreenshotUri(capture.ScreenshotUri))
        {
            return OperationResult<BrowserScreenshotProof>.Fail(
                "DesignConversion browser render screenshot proof must be a retained local PNG artifact.",
                "BROWSER_SCREENSHOT_REMOTE_URI_UNSUPPORTED");
        }

        var pngEvidence = await DesignConversionBrowserPngEvidenceInspector.InspectAsync(
            capture.ScreenshotUri,
            cancellationToken: cancellationToken);
        if (!pngEvidence.IsSuccess)
        {
            return OperationResult<BrowserScreenshotProof>.Fail(
                $"DesignConversion browser render did not produce inspectable local PNG screenshot proof: {pngEvidence.ErrorMessage}",
                "BROWSER_SCREENSHOT_PROOF_UNAVAILABLE");
        }

        using var inspectedPng = pngEvidence.Value!;
        if (inspectedPng.PngWidth != expectedResolution.PhysicalWidth ||
            inspectedPng.PngHeight != expectedResolution.PhysicalHeight)
        {
            return OperationResult<BrowserScreenshotProof>.Fail(
                string.Join(
                    " ",
                    $"DesignConversion browser screenshot dimensions {inspectedPng.PngWidth.ToString(CultureInfo.InvariantCulture)}x{inspectedPng.PngHeight.ToString(CultureInfo.InvariantCulture)}",
                    $"do not match expected physical screenshot {expectedResolution.PhysicalWidth.ToString(CultureInfo.InvariantCulture)}x{expectedResolution.PhysicalHeight.ToString(CultureInfo.InvariantCulture)}",
                    $"for viewport {expectedResolution.LogicalWidth.ToString(CultureInfo.InvariantCulture)}x{expectedResolution.LogicalHeight.ToString(CultureInfo.InvariantCulture)}",
                    $"at device_scale_factor {expectedResolution.DeviceScaleFactor.ToString("R", CultureInfo.InvariantCulture)}."),
                "BROWSER_SCREENSHOT_DIMENSION_MISMATCH");
        }

        if (!inspectedPng.HasNonBlankPixels)
        {
            return OperationResult<BrowserScreenshotProof>.Fail(
                "DesignConversion browser screenshot proof is blank; visual QA is not proven.",
                "BROWSER_SCREENSHOT_BLANK_PNG");
        }

        var declaredSha = NormalizeSha256(capture.ScreenshotSha256);
        if (string.IsNullOrWhiteSpace(declaredSha))
        {
            return OperationResult<BrowserScreenshotProof>.Fail(
                "DesignConversion browser screenshot proof is missing a sha256 value.",
                "BROWSER_SCREENSHOT_SHA_MISSING");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return OperationResult<BrowserScreenshotProof>.Fail(
                "DesignConversion browser screenshot sha256 validation was cancelled.",
                "CANCELLED");
        }

        if (string.IsNullOrWhiteSpace(inspectedPng.SnapshotSha256) ||
            !string.Equals(declaredSha, inspectedPng.SnapshotSha256, StringComparison.OrdinalIgnoreCase))
        {
            return OperationResult<BrowserScreenshotProof>.Fail(
                "DesignConversion browser screenshot proof sha256 does not match the local PNG bytes.",
                "BROWSER_SCREENSHOT_SHA_MISMATCH");
        }

        return OperationResult<BrowserScreenshotProof>.Ok(new BrowserScreenshotProof(
            Sha256: inspectedPng.SnapshotSha256,
            PngEvidence: inspectedPng));
    }

    private static string NormalizeSha256(string? sha256)
    {
        if (string.IsNullOrWhiteSpace(sha256))
            return string.Empty;

        var value = sha256.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase)
            ? sha256["sha256:".Length..]
            : sha256;
        return value.Length == 64 && value.All(Uri.IsHexDigit)
            ? value.ToLowerInvariant()
            : string.Empty;
    }

    private sealed record BrowserScreenshotProof(
        string Sha256,
        DesignConversionBrowserPngEvidence PngEvidence);

    private sealed record PreparedBrowserRenderRequest(
        DesignConversionRenderRequest RenderRequest,
        DesignConversionBrowserCaptureRequest CaptureRequest,
        DesignConversionBrowserPhysicalScreenshotResolution PhysicalResolution,
        string HtmlSha256,
        string CssSha256);

    private sealed record BrowserCaptureAttemptResult(
        DesignConversionBrowserCaptureResult Capture,
        int AttemptCount,
        bool RetriedAfterTimeout);
}

public sealed class ServerOwnedLocalPngComparisonProvider :
    IDesignConversionComparisonProvider,
    IDesignConversionVisualJudgeImageContentResolver
{
    public const string ProviderKind = "server_owned_local_png_pixel_comparison_provider";
    public const string Boundary = "server_owned_local_png_pixel_comparison";
    public const double PassThreshold = 0.85;
    public const string HashLookupDirectoryBudgetExceededCode = "LOCAL_PNG_COMPARISON_HASH_LOOKUP_DIRECTORY_BUDGET_EXCEEDED";
    public const string HashLookupFileBudgetExceededCode = "LOCAL_PNG_COMPARISON_HASH_LOOKUP_FILE_BUDGET_EXCEEDED";
    public const string HashLookupByteBudgetExceededCode = "LOCAL_PNG_COMPARISON_HASH_LOOKUP_BYTE_BUDGET_EXCEEDED";
    public const int DefaultMaxHashLookupDirectories = 256;
    public const int DefaultMaxHashLookupFiles = 2048;
    public const long DefaultMaxHashLookupBytes = 256L * 1024L * 1024L;

    private readonly string _artifactRoot;
    private readonly string _artifactRootWithSeparator;
    private readonly StringComparison _pathComparison;
    private readonly long _maxArtifactBytes;
    private readonly int _maxHashLookupDirectories;
    private readonly int _maxHashLookupFiles;
    private readonly long _maxHashLookupBytes;

    public ServerOwnedLocalPngComparisonProvider(
        string artifactRoot,
        long maxArtifactBytes = DesignConversionBrowserPngEvidenceInspector.MaxPngScreenshotBytes,
        int maxHashLookupDirectories = DefaultMaxHashLookupDirectories,
        int maxHashLookupFiles = DefaultMaxHashLookupFiles,
        long maxHashLookupBytes = DefaultMaxHashLookupBytes)
    {
        if (string.IsNullOrWhiteSpace(artifactRoot))
            throw new ArgumentException("DesignConversion local PNG comparison artifact root is required.", nameof(artifactRoot));
        if (maxArtifactBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxArtifactBytes), "DesignConversion local PNG comparison max bytes must be positive.");
        if (maxArtifactBytes > DesignConversionBrowserPngEvidenceInspector.MaxPngScreenshotBytes)
            throw new ArgumentOutOfRangeException(nameof(maxArtifactBytes), "DesignConversion local PNG comparison max bytes must not exceed the PNG screenshot hard cap.");
        if (maxHashLookupDirectories <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxHashLookupDirectories), "DesignConversion local PNG comparison hash lookup directory budget must be positive.");
        if (maxHashLookupFiles <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxHashLookupFiles), "DesignConversion local PNG comparison hash lookup file budget must be positive.");
        if (maxHashLookupBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxHashLookupBytes), "DesignConversion local PNG comparison hash lookup byte budget must be positive.");

        _artifactRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(artifactRoot));
        _artifactRootWithSeparator = _artifactRoot + Path.DirectorySeparatorChar;
        _pathComparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        _maxArtifactBytes = maxArtifactBytes;
        _maxHashLookupDirectories = maxHashLookupDirectories;
        _maxHashLookupFiles = maxHashLookupFiles;
        _maxHashLookupBytes = maxHashLookupBytes;
    }

    public Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
        DesignConversionQaComparisonRequest request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Fail(
                "DesignConversion local PNG comparison cancelled.",
                "CANCELLED"));
        if (request is null)
            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Fail(
                "DesignConversion local PNG comparison request is required.",
                "LOCAL_PNG_COMPARISON_REQUEST_REQUIRED"));
        if (string.IsNullOrWhiteSpace(request.ComparisonRequestId))
            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Fail(
                "DesignConversion local PNG comparison request id is required.",
                "LOCAL_PNG_COMPARISON_REQUEST_ID_REQUIRED"));
        if (request.Expected is null || request.Actual is null)
            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Fail(
                "DesignConversion local PNG comparison requires expected and rendered image evidence.",
                "LOCAL_PNG_COMPARISON_IMAGES_REQUIRED"));
        if (!SameLogicalResolution(request.Expected.Resolution, request.Actual.Resolution))
            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Fail(
                "DesignConversion local PNG comparison requires matching logical expected/rendered resolutions.",
                "LOCAL_PNG_COMPARISON_RESOLUTION_MISMATCH"));

        var expected = ResolveImage(request.Expected, "expected");
        if (!expected.IsSuccess)
            return Task.FromResult(expected.MapError<DesignConversionQaComparisonArtifact>());
        var actual = ResolveImage(request.Actual, "rendered");
        if (!actual.IsSuccess)
            return Task.FromResult(actual.MapError<DesignConversionQaComparisonArtifact>());

        var expectedDimension = ValidatePngDimensions(request.Expected, expected.Value!.Png, "expected");
        if (!expectedDimension.IsSuccess)
            return Task.FromResult(expectedDimension.MapError<DesignConversionQaComparisonArtifact>());
        var actualDimension = ValidatePngDimensions(request.Actual, actual.Value!.Png, "rendered");
        if (!actualDimension.IsSuccess)
            return Task.FromResult(actualDimension.MapError<DesignConversionQaComparisonArtifact>());

        var resolution = request.Actual.Resolution;
        var diffResolution = DesignConversionBrowserPhysicalScreenshotResolution.Resolve(
            resolution,
            "LOCAL_PNG_COMPARISON_DIFF_RESOLUTION_INVALID",
            "DesignConversion local PNG comparison diff");
        if (!diffResolution.IsSuccess)
            return Task.FromResult(diffResolution.MapError<DesignConversionQaComparisonArtifact>());

        var comparison = ComparePixels(
            expected.Value.Png,
            actual.Value.Png,
            diffResolution.Value!.PhysicalWidth,
            diffResolution.Value.PhysicalHeight);
        var diffBytes = BuildDiffPng(
            comparison.DiffRgbaPixels,
            diffResolution.Value.PhysicalWidth,
            diffResolution.Value.PhysicalHeight);
        var diffSha = Sha256Bytes(diffBytes);
        var diffPath = WriteDiffArtifact(request, diffBytes, diffSha);
        if (!diffPath.IsSuccess)
            return Task.FromResult(diffPath.MapError<DesignConversionQaComparisonArtifact>());

        var diffReadback = ReadBoundedRegularFile(diffPath.Value!, "diff");
        if (!diffReadback.IsSuccess)
            return Task.FromResult(diffReadback.MapError<DesignConversionQaComparisonArtifact>());
        if (!string.Equals(Sha256Bytes(diffReadback.Value!), diffSha, StringComparison.Ordinal))
        {
            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Fail(
                "DesignConversion local PNG comparison diff artifact SHA-256 verification failed.",
                "LOCAL_PNG_COMPARISON_DIFF_SHA_MISMATCH"));
        }

        var diffEvidence = DesignConversionBrowserPngEvidenceInspector.DecodePngBytesForComparison(
            diffReadback.Value!,
            diffPath.Value!);
        if (!diffEvidence.IsSuccess)
            return Task.FromResult(diffEvidence.MapError<DesignConversionQaComparisonArtifact>());
        if (!diffEvidence.Value!.HasNonBlankPixels)
        {
            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Fail(
                "DesignConversion local PNG comparison diff artifact must contain nonblank PNG evidence.",
                "LOCAL_PNG_COMPARISON_DIFF_BLANK"));
        }

        var score = Math.Max(0.0, Math.Round(1.0 - comparison.VisualDistance, 6, MidpointRounding.AwayFromZero));
        var passed = score >= PassThreshold;
        var evidenceRefs = new[]
        {
            $"local-png-comparison:{request.ComparisonRequestId}",
            $"expected-sha256:sha256:{expected.Value.Png.Sha256}",
            $"rendered-sha256:sha256:{actual.Value.Png.Sha256}",
            $"diff-sha256:sha256:{diffSha}",
            ProviderKind,
            Boundary,
            "external_ai_visual_judge_executed:false",
            "network_call_executed:false",
            "real_external_provider_executed:false"
        };
        var defects = comparison.DifferentPixelCount == 0
            ? Array.Empty<VisualDefect>()
            : [CreatePixelDifferenceDefect(request, comparison.DifferentPixelCount)];
        var verdict = new VisualVerdict(
            VerdictId: $"{request.ComparisonRequestId}:local-png-verdict:{diffSha[..12]}",
            Score: score,
            Defects: defects,
            EvidenceRefs: evidenceRefs,
            Passed: passed);

        var artifact = new DesignConversionQaComparisonArtifact(
            ArtifactId: $"{request.ComparisonRequestId}:local-png-comparison:{diffSha[..12]}",
            ComparisonRequestId: request.ComparisonRequestId,
            Expected: request.Expected,
            Actual: request.Actual,
            Verdict: verdict,
            DiffImageUri: $"local-png-ref:sha256:{diffSha}",
            DiffImageSha256: $"sha256:{diffSha}",
            Metrics: new Dictionary<string, double>
            {
                ["visual_distance"] = comparison.VisualDistance,
                ["visual_judge_score"] = score,
                ["pixel_difference_ratio"] = comparison.PixelDifferenceRatio,
                ["different_pixel_count"] = comparison.DifferentPixelCount,
                ["total_compared_pixel_count"] = comparison.TotalPixelCount,
                ["visual_comparison_proven"] = 1.0,
                ["external_ai_visual_judge_executed"] = 0.0,
                ["network_call_executed"] = 0.0
            },
            EvidenceRefs: evidenceRefs)
        {
            Metadata = new Dictionary<string, string>
            {
                ["status"] = "local_png_comparison_completed",
                ["provider_kind"] = ProviderKind,
                ["comparison_provider_kind"] = ProviderKind,
                ["comparison_boundary"] = Boundary,
                ["hidden_determinism_boundary"] = "server_owned_local_png_pixel_comparison",
                ["local_png_pixel_comparison"] = bool.TrueString,
                ["visual_comparison_proven"] = bool.TrueString,
                ["external_ai_used"] = bool.FalseString,
                ["external_ai_visual_judge_executed"] = bool.FalseString,
                ["real_external_provider_executed"] = bool.FalseString,
                ["network_call_executed"] = bool.FalseString,
                ["ai_visual_judge_claimed"] = bool.FalseString,
                ["product_ready"] = bool.FalseString,
                ["media_type"] = "image/png",
                ["content_type"] = "image/png",
                ["png_width"] = diffEvidence.Value.PngWidth.ToString(CultureInfo.InvariantCulture),
                ["png_height"] = diffEvidence.Value.PngHeight.ToString(CultureInfo.InvariantCulture),
                ["viewport_width"] = resolution.Width.ToString(CultureInfo.InvariantCulture),
                ["viewport_height"] = resolution.Height.ToString(CultureInfo.InvariantCulture),
                ["device_scale_factor"] = resolution.DeviceScaleFactor.ToString("R", CultureInfo.InvariantCulture),
                ["expected_png_width"] = expected.Value.Png.PngWidth.ToString(CultureInfo.InvariantCulture),
                ["expected_png_height"] = expected.Value.Png.PngHeight.ToString(CultureInfo.InvariantCulture),
                ["rendered_png_width"] = actual.Value.Png.PngWidth.ToString(CultureInfo.InvariantCulture),
                ["rendered_png_height"] = actual.Value.Png.PngHeight.ToString(CultureInfo.InvariantCulture),
                ["expected_sha256_verified"] = bool.TrueString,
                ["rendered_sha256_verified"] = bool.TrueString,
                ["diff_sha256_verified"] = bool.TrueString,
                ["diff_png_signature_verified"] = bool.TrueString,
                ["diff_png_nonblank_pixels_verified"] = bool.TrueString,
                ["comparison_diff_readback_sha256_verified"] = bool.TrueString,
                ["diff_image_sha256"] = $"sha256:{diffSha}",
                ["visual_distance"] = comparison.VisualDistance.ToString("R", CultureInfo.InvariantCulture),
                ["pixel_difference_ratio"] = comparison.PixelDifferenceRatio.ToString("R", CultureInfo.InvariantCulture),
                ["comparison_verdict_score"] = score.ToString("R", CultureInfo.InvariantCulture),
                ["comparison_verdict_passed"] = passed.ToString(),
                [DesignConversionComparisonEvidenceTrustBoundary.TrustBoundaryKey] = DesignConversionComparisonEvidenceTrustBoundary.TrustBoundary,
                [DesignConversionComparisonEvidenceTrustBoundary.TrustClassificationKey] = DesignConversionComparisonEvidenceTrustBoundary.ServerOwnedLocalPngClassification,
                [DesignConversionComparisonEvidenceTrustBoundary.ServerCanonicalizedKey] = bool.TrueString,
                [DesignConversionRenderEvidenceTrustBoundary.ProductionTrustedKey] = bool.TrueString,
                [DesignConversionRenderEvidenceTrustBoundary.ApprovalEvidenceAllowedKey] = bool.TrueString,
                ["server_owned_local_png_comparison_trust_verified"] = bool.TrueString,
                ["model_weights_published"] = bool.FalseString,
                ["model_training_verified"] = bool.FalseString,
                ["training_authority_verified"] = bool.FalseString,
                ["dpo_eligible"] = bool.FalseString
            }
        };

        return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Ok(artifact));
    }

    public Task<OperationResult<DesignConversionVisualJudgeImageContent>> ResolveAsync(
        RenderQaImageRecord image,
        string role,
        long maxBytes,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(OperationResult<DesignConversionVisualJudgeImageContent>.Fail(
                "External visual judge image content resolution cancelled.",
                "CANCELLED"));
        }
        if (image is null)
        {
            return Task.FromResult(OperationResult<DesignConversionVisualJudgeImageContent>.Fail(
                $"External visual judge {role} image is required.",
                "EXTERNAL_VISUAL_JUDGE_IMAGE_REQUIRED"));
        }
        if (maxBytes <= 0)
        {
            return Task.FromResult(OperationResult<DesignConversionVisualJudgeImageContent>.Fail(
                "External visual judge image content byte limit must be positive.",
                "EXTERNAL_VISUAL_JUDGE_IMAGE_BYTES_OUT_OF_BOUNDS"));
        }
        if (!TryNormalizeSha256(image.Sha256, out var declaredSha))
        {
            return Task.FromResult(OperationResult<DesignConversionVisualJudgeImageContent>.Fail(
                $"External visual judge {role} image must include a sha256-prefixed 64-hex digest.",
                "EXTERNAL_VISUAL_JUDGE_IMAGE_SHA_REQUIRED"));
        }

        var path = ResolveImagePath(image.Uri, declaredSha, role);
        if (!path.IsSuccess)
            return Task.FromResult(MapExternalImageContentError(path.ErrorMessage, path.ErrorCode));

        var bytes = ReadBoundedRegularFile(path.Value!, role, Math.Min(_maxArtifactBytes, maxBytes));
        if (!bytes.IsSuccess)
            return Task.FromResult(MapExternalImageContentError(bytes.ErrorMessage, bytes.ErrorCode));

        var actualSha = Sha256Bytes(bytes.Value!);
        if (!string.Equals(actualSha, declaredSha, StringComparison.Ordinal))
        {
            return Task.FromResult(OperationResult<DesignConversionVisualJudgeImageContent>.Fail(
                $"External visual judge {role} image SHA-256 did not match the resolved PNG bytes.",
                "EXTERNAL_VISUAL_JUDGE_IMAGE_SHA_MISMATCH"));
        }

        var png = DesignConversionBrowserPngEvidenceInspector.DecodePngBytesForComparison(bytes.Value!, path.Value!);
        if (!png.IsSuccess)
            return Task.FromResult(MapExternalImageContentError(png.ErrorMessage, png.ErrorCode));
        if (!png.Value!.HasNonBlankPixels)
        {
            return Task.FromResult(OperationResult<DesignConversionVisualJudgeImageContent>.Fail(
                $"External visual judge {role} image must contain nonblank PNG pixels.",
                "EXTERNAL_VISUAL_JUDGE_IMAGE_PNG_INVALID"));
        }

        var dimension = ValidatePngDimensions(image, png.Value, role);
        if (!dimension.IsSuccess)
            return Task.FromResult(MapExternalImageContentError(dimension.ErrorMessage, dimension.ErrorCode));

        return Task.FromResult(OperationResult<DesignConversionVisualJudgeImageContent>.Ok(
            new DesignConversionVisualJudgeImageContent(
                MediaType: "image/png",
                Sha256: actualSha,
                Bytes: bytes.Value!)));
    }

    private OperationResult<LocalComparisonImage> ResolveImage(RenderQaImageRecord image, string role)
    {
        if (!TryNormalizeSha256(image.Sha256, out var declaredSha))
        {
            return OperationResult<LocalComparisonImage>.Fail(
                $"DesignConversion local PNG comparison {role} image must include a sha256-prefixed 64-hex digest.",
                "LOCAL_PNG_COMPARISON_SHA_REQUIRED");
        }

        var path = ResolveImagePath(image.Uri, declaredSha, role);
        if (!path.IsSuccess)
            return path.MapError<LocalComparisonImage>();

        var bytes = ReadBoundedRegularFile(path.Value!, role);
        if (!bytes.IsSuccess)
            return bytes.MapError<LocalComparisonImage>();

        var actualSha = Sha256Bytes(bytes.Value!);
        if (!string.Equals(actualSha, declaredSha, StringComparison.Ordinal))
        {
            return OperationResult<LocalComparisonImage>.Fail(
                $"DesignConversion local PNG comparison {role} image SHA-256 did not match the file bytes.",
                "LOCAL_PNG_COMPARISON_SHA_MISMATCH");
        }

        var png = DesignConversionBrowserPngEvidenceInspector.DecodePngBytesForComparison(bytes.Value!, path.Value!);
        if (!png.IsSuccess)
            return png.MapError<LocalComparisonImage>();
        if (!png.Value!.HasNonBlankPixels)
        {
            return OperationResult<LocalComparisonImage>.Fail(
                $"DesignConversion local PNG comparison {role} image must contain nonblank PNG pixels.",
                "LOCAL_PNG_COMPARISON_BLANK");
        }

        return OperationResult<LocalComparisonImage>.Ok(new LocalComparisonImage(path.Value!, png.Value));
    }

    private OperationResult<string> ResolveImagePath(string uri, string declaredSha, string role)
    {
        if (TryExtractShaRef(uri, out var uriSha))
        {
            if (!string.Equals(uriSha, declaredSha, StringComparison.Ordinal))
            {
                return OperationResult<string>.Fail(
                    $"DesignConversion local PNG comparison {role} image URI hash did not match declared SHA-256.",
                    "LOCAL_PNG_COMPARISON_URI_SHA_MISMATCH");
            }

            return FindArtifactBySha(declaredSha, role);
        }

        var localPath = ResolveLocalPath(uri);
        if (!localPath.IsSuccess)
            return localPath;
        if (!IsUnderRoot(localPath.Value!))
        {
            return OperationResult<string>.Fail(
                $"DesignConversion local PNG comparison {role} image path escaped the configured artifact root.",
                "LOCAL_PNG_COMPARISON_PATH_ESCAPED_ROOT");
        }

        return OperationResult<string>.Ok(localPath.Value!);
    }

    private OperationResult<string> FindArtifactBySha(string sha, string role)
    {
        var root = ValidateRoot();
        if (!root.IsSuccess)
            return root;

        var found = FindFirstArtifactBySha(root.Value!, sha, role);
        if (!found.IsSuccess)
            return found.MapError<string>();

        return string.IsNullOrWhiteSpace(found.Value)
            ? OperationResult<string>.Fail(
                $"DesignConversion local PNG comparison could not resolve {role} image content hash inside the configured artifact root.",
                "LOCAL_PNG_COMPARISON_SHA_NOT_FOUND")
            : OperationResult<string>.Ok(found.Value);
    }

    private OperationResult<string?> FindFirstArtifactBySha(
        string root,
        string sha,
        string role)
    {
        var pending = new Queue<string>();
        var budget = new HashLookupBudgetState();
        pending.Enqueue(root);
        while (pending.Count > 0)
        {
            var directory = pending.Dequeue();
            var directoryBudget = ClaimHashLookupDirectoryBudget(budget, role);
            if (!directoryBudget.IsSuccess)
                return directoryBudget.MapError<string?>();

            if (!IsSafeHashLookupDirectory(directory))
                continue;

            var files = EnumerateOrderedPngFiles(directory);
            if (!files.IsSuccess)
                continue;

            foreach (var path in files.Value!)
            {
                var fileBudget = ClaimHashLookupFileBudget(budget, role);
                if (!fileBudget.IsSuccess)
                    return fileBudget.MapError<string?>();

                var byteBudget = ClaimHashLookupByteBudget(path, budget, role);
                if (!byteBudget.IsSuccess)
                    return byteBudget.MapError<string?>();

                var bytes = ReadBoundedRegularFile(path, role);
                if (!bytes.IsSuccess)
                    continue;
                if (string.Equals(Sha256Bytes(bytes.Value!), sha, StringComparison.Ordinal))
                    return OperationResult<string?>.Ok(path);
            }

            var children = EnumerateOrderedChildDirectories(directory);
            if (!children.IsSuccess)
                continue;

            foreach (var child in children.Value!)
                pending.Enqueue(child);
        }

        return OperationResult<string?>.Ok(null);
    }

    private OperationResult<bool> ClaimHashLookupDirectoryBudget(
        HashLookupBudgetState budget,
        string role)
    {
        if (budget.DirectoriesVisited >= _maxHashLookupDirectories)
            return HashLookupBudgetExceeded(role, "directory", HashLookupDirectoryBudgetExceededCode);

        budget.DirectoriesVisited++;
        return OperationResult<bool>.Ok(true);
    }

    private OperationResult<bool> ClaimHashLookupFileBudget(
        HashLookupBudgetState budget,
        string role)
    {
        if (budget.FilesVisited >= _maxHashLookupFiles)
            return HashLookupBudgetExceeded(role, "file", HashLookupFileBudgetExceededCode);

        budget.FilesVisited++;
        return OperationResult<bool>.Ok(true);
    }

    private OperationResult<bool> ClaimHashLookupByteBudget(
        string path,
        HashLookupBudgetState budget,
        string role)
    {
        try
        {
            var fullPath = Path.GetFullPath(path);
            if (!IsUnderRoot(fullPath))
                return OperationResult<bool>.Ok(true);

            var fileInfo = new FileInfo(fullPath);
            fileInfo.Refresh();
            if (!fileInfo.Exists ||
                (fileInfo.Attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0 ||
                fileInfo.Length <= 0 ||
                fileInfo.Length > _maxArtifactBytes)
            {
                return OperationResult<bool>.Ok(true);
            }

            if (fileInfo.Length > _maxHashLookupBytes - budget.BytesVisited)
                return HashLookupBudgetExceeded(role, "byte", HashLookupByteBudgetExceededCode);

            budget.BytesVisited += fileInfo.Length;
            return OperationResult<bool>.Ok(true);
        }
        catch (Exception ex) when (IsExpectedFileSystemException(ex))
        {
            return OperationResult<bool>.Ok(true);
        }
    }

    private OperationResult<IReadOnlyList<string>> EnumerateOrderedPngFiles(string directory)
    {
        try
        {
            return OperationResult<IReadOnlyList<string>>.Ok(
                Directory.EnumerateFiles(directory, "*.png", SearchOption.TopDirectoryOnly)
                    .Select(Path.GetFullPath)
                    .Where(IsUnderRoot)
                    .Where(IsSafeHashLookupFile)
                    .Order(StringComparer.Ordinal)
                    .ToArray());
        }
        catch (Exception ex) when (IsExpectedFileSystemException(ex))
        {
            return OperationResult<IReadOnlyList<string>>.Fail(
                "DesignConversion local PNG comparison hash lookup could not enumerate PNG files.",
                "LOCAL_PNG_COMPARISON_HASH_LOOKUP_ENUMERATION_UNAVAILABLE");
        }
    }

    private OperationResult<IReadOnlyList<string>> EnumerateOrderedChildDirectories(string directory)
    {
        try
        {
            return OperationResult<IReadOnlyList<string>>.Ok(
                Directory.EnumerateDirectories(directory, "*", SearchOption.TopDirectoryOnly)
                    .Select(Path.GetFullPath)
                    .Where(IsUnderRoot)
                    .Where(IsSafeHashLookupDirectory)
                    .Order(StringComparer.Ordinal)
                    .ToArray());
        }
        catch (Exception ex) when (IsExpectedFileSystemException(ex))
        {
            return OperationResult<IReadOnlyList<string>>.Fail(
                "DesignConversion local PNG comparison hash lookup could not enumerate child directories.",
                "LOCAL_PNG_COMPARISON_HASH_LOOKUP_ENUMERATION_UNAVAILABLE");
        }
    }

    private bool IsSafeHashLookupDirectory(string directory)
    {
        try
        {
            var fullPath = Path.GetFullPath(directory);
            if (!IsUnderRoot(fullPath))
                return false;

            var directoryInfo = new DirectoryInfo(fullPath);
            directoryInfo.Refresh();
            return directoryInfo.Exists &&
                   string.IsNullOrWhiteSpace(directoryInfo.LinkTarget) &&
                   (directoryInfo.Attributes & FileAttributes.ReparsePoint) == 0;
        }
        catch (Exception ex) when (IsExpectedFileSystemException(ex))
        {
            return false;
        }
    }

    private bool IsSafeHashLookupFile(string path)
    {
        try
        {
            var fullPath = Path.GetFullPath(path);
            if (!IsUnderRoot(fullPath))
                return false;

            var fileInfo = new FileInfo(fullPath);
            fileInfo.Refresh();
            return fileInfo.Exists &&
                   string.IsNullOrWhiteSpace(fileInfo.LinkTarget) &&
                   (fileInfo.Attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) == 0;
        }
        catch (Exception ex) when (IsExpectedFileSystemException(ex))
        {
            return false;
        }
    }

    private OperationResult<byte[]> ReadBoundedRegularFile(string path, string role)
        => ReadBoundedRegularFile(path, role, _maxArtifactBytes);

    private OperationResult<byte[]> ReadBoundedRegularFile(
        string path,
        string role,
        long maxBytes)
    {
        try
        {
            var fullPath = Path.GetFullPath(path);
            if (!IsUnderRoot(fullPath))
            {
                return OperationResult<byte[]>.Fail(
                    $"DesignConversion local PNG comparison {role} image path escaped the configured artifact root.",
                    "LOCAL_PNG_COMPARISON_PATH_ESCAPED_ROOT");
            }

            var ancestorSafety = ValidateNoReparseAncestor(fullPath, role);
            if (!ancestorSafety.IsSuccess)
                return ancestorSafety.MapError<byte[]>();

            var fileInfo = new FileInfo(fullPath);
            fileInfo.Refresh();
            if (!fileInfo.Exists)
                return OperationResult<byte[]>.Fail($"DesignConversion local PNG comparison {role} image was not found.", "LOCAL_PNG_COMPARISON_FILE_MISSING");
            if (!string.IsNullOrWhiteSpace(fileInfo.LinkTarget) ||
                (fileInfo.Attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0)
            {
                return OperationResult<byte[]>.Fail($"DesignConversion local PNG comparison {role} image must be a regular file.", "LOCAL_PNG_COMPARISON_FILE_INVALID");
            }
            if (fileInfo.Length <= 0 || fileInfo.Length > maxBytes)
                return OperationResult<byte[]>.Fail($"DesignConversion local PNG comparison {role} image exceeded the bounded file size.", "LOCAL_PNG_COMPARISON_FILE_SIZE_INVALID");

            using var stream = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81920,
                options: FileOptions.SequentialScan);
            var expectedLength = stream.Length;
            if (expectedLength <= 0 ||
                expectedLength > maxBytes ||
                expectedLength != fileInfo.Length)
            {
                return OperationResult<byte[]>.Fail(
                    $"DesignConversion local PNG comparison {role} image exceeded the bounded file size.",
                    "LOCAL_PNG_COMPARISON_FILE_SIZE_INVALID");
            }

            var bytes = DesignConversionBoundedContentReader.ReadToEnd(
                stream,
                maxBytes,
                $"DesignConversion local PNG comparison {role} image",
                "LOCAL_PNG_COMPARISON_FILE_SIZE_INVALID",
                "LOCAL_PNG_COMPARISON_FILE_SIZE_INVALID");
            if (!bytes.IsSuccess)
                return bytes;

            if (stream.Length != expectedLength)
            {
                return OperationResult<byte[]>.Fail(
                    $"DesignConversion local PNG comparison {role} image changed while being read.",
                    "LOCAL_PNG_COMPARISON_FILE_SIZE_INVALID");
            }

            return bytes;
        }
        catch (Exception ex) when (IsExpectedFileSystemException(ex))
        {
            return OperationResult<byte[]>.Fail(
                $"DesignConversion local PNG comparison {role} image could not be read.",
                "LOCAL_PNG_COMPARISON_FILE_UNREADABLE");
        }
    }

    private OperationResult<bool> ValidateNoReparseAncestor(string fullPath, string role)
    {
        try
        {
            var root = ValidateRoot();
            if (!root.IsSuccess)
                return root.MapError<bool>();

            var directory = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrWhiteSpace(directory))
                return OperationResult<bool>.Fail($"DesignConversion local PNG comparison {role} image path is invalid.", "LOCAL_PNG_COMPARISON_FILE_INVALID");

            var current = Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory));
            while (!string.Equals(current, _artifactRoot, _pathComparison))
            {
                if (!IsUnderRoot(current))
                {
                    return OperationResult<bool>.Fail(
                        $"DesignConversion local PNG comparison {role} image path escaped the configured artifact root.",
                        "LOCAL_PNG_COMPARISON_PATH_ESCAPED_ROOT");
                }

                var directoryInfo = new DirectoryInfo(current);
                directoryInfo.Refresh();
                if (!directoryInfo.Exists)
                    return OperationResult<bool>.Fail($"DesignConversion local PNG comparison {role} image was not found.", "LOCAL_PNG_COMPARISON_FILE_MISSING");
                if (!string.IsNullOrWhiteSpace(directoryInfo.LinkTarget) ||
                    (directoryInfo.Attributes & FileAttributes.ReparsePoint) != 0)
                {
                    return OperationResult<bool>.Fail(
                        $"DesignConversion local PNG comparison {role} image path contains a reparse-point ancestor.",
                        "LOCAL_PNG_COMPARISON_PATH_REPARSE_ANCESTOR");
                }

                var parent = Directory.GetParent(current)?.FullName;
                if (string.IsNullOrWhiteSpace(parent))
                    return OperationResult<bool>.Fail($"DesignConversion local PNG comparison {role} image path is invalid.", "LOCAL_PNG_COMPARISON_FILE_INVALID");
                current = Path.TrimEndingDirectorySeparator(Path.GetFullPath(parent));
            }

            return OperationResult<bool>.Ok(true);
        }
        catch (Exception ex) when (IsExpectedFileSystemException(ex))
        {
            return OperationResult<bool>.Fail(
                $"DesignConversion local PNG comparison {role} image path could not be validated.",
                "LOCAL_PNG_COMPARISON_FILE_UNREADABLE");
        }
    }

    private static OperationResult<DesignConversionVisualJudgeImageContent> MapExternalImageContentError(
        string? errorMessage,
        string? errorCode)
    {
        var code = errorCode switch
        {
            "LOCAL_PNG_COMPARISON_SHA_REQUIRED" => "EXTERNAL_VISUAL_JUDGE_IMAGE_SHA_REQUIRED",
            "LOCAL_PNG_COMPARISON_URI_SHA_MISMATCH" => "EXTERNAL_VISUAL_JUDGE_IMAGE_SHA_MISMATCH",
            "LOCAL_PNG_COMPARISON_SHA_MISMATCH" => "EXTERNAL_VISUAL_JUDGE_IMAGE_SHA_MISMATCH",
            "LOCAL_PNG_COMPARISON_FILE_SIZE_INVALID" => "EXTERNAL_VISUAL_JUDGE_IMAGE_BYTES_OUT_OF_BOUNDS",
            "LOCAL_PNG_COMPARISON_FILE_MISSING" => "EXTERNAL_VISUAL_JUDGE_IMAGE_UNRESOLVABLE",
            "LOCAL_PNG_COMPARISON_SHA_NOT_FOUND" => "EXTERNAL_VISUAL_JUDGE_IMAGE_UNRESOLVABLE",
            "LOCAL_PNG_COMPARISON_PATH_ESCAPED_ROOT" => "EXTERNAL_VISUAL_JUDGE_IMAGE_UNRESOLVABLE",
            "LOCAL_PNG_COMPARISON_PATH_REPARSE_ANCESTOR" => "EXTERNAL_VISUAL_JUDGE_IMAGE_UNRESOLVABLE",
            "LOCAL_PNG_COMPARISON_NETWORK_PATH" => "EXTERNAL_VISUAL_JUDGE_IMAGE_UNRESOLVABLE",
            "LOCAL_PNG_COMPARISON_URI_UNSUPPORTED" => "EXTERNAL_VISUAL_JUDGE_IMAGE_UNRESOLVABLE",
            "LOCAL_PNG_COMPARISON_FILE_INVALID" => "EXTERNAL_VISUAL_JUDGE_IMAGE_UNRESOLVABLE",
            "LOCAL_PNG_COMPARISON_FILE_UNREADABLE" => "EXTERNAL_VISUAL_JUDGE_IMAGE_UNRESOLVABLE",
            "COMPARISON_PNG_SIGNATURE_INVALID" => "EXTERNAL_VISUAL_JUDGE_IMAGE_PNG_INVALID",
            "PNG_SIGNATURE_INVALID" => "EXTERNAL_VISUAL_JUDGE_IMAGE_PNG_INVALID",
            "PNG_DIMENSIONS_MISSING" => "EXTERNAL_VISUAL_JUDGE_IMAGE_PNG_INVALID",
            "PNG_COLOR_TYPE_UNSUPPORTED" => "EXTERNAL_VISUAL_JUDGE_IMAGE_PNG_INVALID",
            "PNG_BIT_DEPTH_UNSUPPORTED" => "EXTERNAL_VISUAL_JUDGE_IMAGE_PNG_INVALID",
            "PNG_INTERLACE_UNSUPPORTED" => "EXTERNAL_VISUAL_JUDGE_IMAGE_PNG_INVALID",
            "PNG_PIXEL_DATA_INCOMPLETE" => "EXTERNAL_VISUAL_JUDGE_IMAGE_PNG_INVALID",
            "PNG_PIXEL_DATA_TOO_LARGE" => "EXTERNAL_VISUAL_JUDGE_IMAGE_PNG_INVALID",
            "PNG_FILTER_INVALID" => "EXTERNAL_VISUAL_JUDGE_IMAGE_PNG_INVALID",
            "LOCAL_PNG_COMPARISON_DIMENSION_MISMATCH" => "EXTERNAL_VISUAL_JUDGE_IMAGE_PNG_INVALID",
            "LOCAL_PNG_COMPARISON_RESOLUTION_INVALID" => "EXTERNAL_VISUAL_JUDGE_IMAGE_PNG_INVALID",
            _ => "EXTERNAL_VISUAL_JUDGE_IMAGE_UNRESOLVABLE"
        };

        return OperationResult<DesignConversionVisualJudgeImageContent>.Fail(
            errorMessage ?? "External visual judge image content could not be resolved.",
            code);
    }

    private OperationResult<bool> ValidatePngDimensions(
        RenderQaImageRecord image,
        DesignConversionDecodedPngComparisonEvidence png,
        string role)
    {
        var physical = DesignConversionBrowserPhysicalScreenshotResolution.Resolve(
            image.Resolution,
            "LOCAL_PNG_COMPARISON_RESOLUTION_INVALID",
            $"DesignConversion local PNG comparison {role}");
        if (!physical.IsSuccess)
            return physical.MapError<bool>();

        if (png.PngWidth == physical.Value!.PhysicalWidth &&
            png.PngHeight == physical.Value.PhysicalHeight)
        {
            return OperationResult<bool>.Ok(true);
        }

        return OperationResult<bool>.Fail(
            $"DesignConversion local PNG comparison {role} image PNG dimensions did not match device-scaled physical resolution metadata.",
            "LOCAL_PNG_COMPARISON_DIMENSION_MISMATCH");
    }

    private OperationResult<string> WriteDiffArtifact(
        DesignConversionQaComparisonRequest request,
        byte[] diffBytes,
        string diffSha)
    {
        try
        {
            var root = ValidateRoot();
            if (!root.IsSuccess)
                return root;

            var directory = Path.Combine(
                root.Value!,
                "comparison-diffs",
                SafePathSegment(request.RequestId),
                SafePathSegment(request.ComparisonRequestId));
            var path = Path.Combine(directory, $"{diffSha[..16]}.png");
            var fullPath = Path.GetFullPath(path);
            if (!IsUnderRoot(fullPath))
            {
                return OperationResult<string>.Fail(
                    "DesignConversion local PNG comparison diff artifact path escaped the configured artifact root.",
                    "LOCAL_PNG_COMPARISON_DIFF_PATH_INVALID");
            }

            Directory.CreateDirectory(directory);
            File.WriteAllBytes(fullPath, diffBytes);
            return OperationResult<string>.Ok(fullPath);
        }
        catch (Exception ex) when (IsExpectedFileSystemException(ex))
        {
            return OperationResult<string>.Fail(
                "DesignConversion local PNG comparison diff artifact could not be written.",
                "LOCAL_PNG_COMPARISON_DIFF_WRITE_FAILED");
        }
    }

    private OperationResult<string> ValidateRoot()
    {
        try
        {
            if (IsNetworkRoot(_artifactRoot))
            {
                return OperationResult<string>.Fail(
                    "DesignConversion local PNG comparison artifact root must be a local directory.",
                    "LOCAL_PNG_COMPARISON_ROOT_INVALID");
            }

            Directory.CreateDirectory(_artifactRoot);
            var root = new DirectoryInfo(_artifactRoot);
            root.Refresh();
            if (!root.Exists ||
                !string.IsNullOrWhiteSpace(root.LinkTarget) ||
                (root.Attributes & FileAttributes.ReparsePoint) != 0)
            {
                return OperationResult<string>.Fail(
                    "DesignConversion local PNG comparison artifact root is invalid.",
                    "LOCAL_PNG_COMPARISON_ROOT_INVALID");
            }

            return OperationResult<string>.Ok(_artifactRoot);
        }
        catch (Exception ex) when (IsExpectedFileSystemException(ex))
        {
            return OperationResult<string>.Fail(
                "DesignConversion local PNG comparison artifact root is unavailable.",
                "LOCAL_PNG_COMPARISON_ROOT_UNAVAILABLE");
        }
    }

    private bool IsUnderRoot(string path)
    {
        var fullPath = Path.GetFullPath(path);
        return string.Equals(fullPath, _artifactRoot, _pathComparison) ||
               fullPath.StartsWith(_artifactRootWithSeparator, _pathComparison);
    }

    private static bool IsNetworkRoot(string path)
        => path.StartsWith(@"\\", StringComparison.Ordinal) ||
           path.StartsWith("//", StringComparison.Ordinal);

    private static OperationResult<string> ResolveLocalPath(string uri)
    {
        if (string.IsNullOrWhiteSpace(uri))
            return OperationResult<string>.Fail("DesignConversion local PNG comparison URI is required.", "LOCAL_PNG_COMPARISON_URI_REQUIRED");

        try
        {
            var text = uri.Trim();
            if (text.StartsWith(@"\\", StringComparison.Ordinal) || text.StartsWith("//", StringComparison.Ordinal))
                return OperationResult<string>.Fail("DesignConversion local PNG comparison URI must not reference a network path.", "LOCAL_PNG_COMPARISON_NETWORK_PATH");
            if (Path.IsPathFullyQualified(text))
                return OperationResult<string>.Ok(Path.GetFullPath(text));
            if (Uri.TryCreate(text, UriKind.Absolute, out var parsed) && parsed.IsFile && !parsed.IsUnc && string.IsNullOrWhiteSpace(parsed.Host))
                return OperationResult<string>.Ok(Path.GetFullPath(parsed.LocalPath));

            return OperationResult<string>.Fail("DesignConversion local PNG comparison URI must be a local file or content hash ref.", "LOCAL_PNG_COMPARISON_URI_UNSUPPORTED");
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException or UriFormatException)
        {
            return OperationResult<string>.Fail("DesignConversion local PNG comparison URI was invalid.", "LOCAL_PNG_COMPARISON_URI_INVALID");
        }
    }

    private static LocalPngComparisonResult ComparePixels(
        DesignConversionDecodedPngComparisonEvidence expected,
        DesignConversionDecodedPngComparisonEvidence actual,
        int logicalWidth,
        int logicalHeight)
    {
        var diff = new byte[checked(logicalWidth * logicalHeight * 4)];
        long totalDelta = 0;
        var different = 0;
        var total = logicalWidth * logicalHeight;
        for (var y = 0; y < logicalHeight; y++)
        {
            for (var x = 0; x < logicalWidth; x++)
            {
                var expectedPixel = Sample(expected, x, y, logicalWidth, logicalHeight);
                var actualPixel = Sample(actual, x, y, logicalWidth, logicalHeight);
                var expectedRendered = CompositeOnWhite(expectedPixel);
                var actualRendered = CompositeOnWhite(actualPixel);
                var delta = Math.Abs(expectedRendered.R - actualRendered.R) +
                            Math.Abs(expectedRendered.G - actualRendered.G) +
                            Math.Abs(expectedRendered.B - actualRendered.B);
                totalDelta += delta;
                if (delta > 0)
                    different++;

                var offset = checked(((y * logicalWidth) + x) * 4);
                if (delta == 0)
                {
                    diff[offset] = 255;
                    diff[offset + 1] = 255;
                    diff[offset + 2] = 255;
                    diff[offset + 3] = 255;
                }
                else
                {
                    diff[offset] = 255;
                    diff[offset + 1] = (byte)Math.Max(0, 255 - Math.Min(255, delta / 2));
                    diff[offset + 2] = (byte)Math.Max(0, 255 - Math.Min(255, delta / 2));
                    diff[offset + 3] = 255;
                }
            }
        }

        var evidenceMarkerPixelCount = Math.Min(
            total,
            DesignConversionBrowserPngEvidenceInspector.MinimumMeaningfulNonBlankPixels(total));
        for (var pixel = 0; pixel < evidenceMarkerPixelCount; pixel++)
        {
            var offset = pixel * 4;
            diff[offset] = 0;
            diff[offset + 1] = 160;
            diff[offset + 2] = 80;
            diff[offset + 3] = 255;
        }

        var visualDistance = total <= 0
            ? 1.0
            : Math.Round(totalDelta / (double)(total * 765), 6, MidpointRounding.AwayFromZero);
        var ratio = total <= 0
            ? 1.0
            : Math.Round(different / (double)total, 6, MidpointRounding.AwayFromZero);
        return new LocalPngComparisonResult(
            VisualDistance: visualDistance,
            PixelDifferenceRatio: ratio,
            DifferentPixelCount: different,
            TotalPixelCount: total,
            DiffRgbaPixels: diff);
    }

    private static (int R, int G, int B, int A) Sample(
        DesignConversionDecodedPngComparisonEvidence image,
        int logicalX,
        int logicalY,
        int logicalWidth,
        int logicalHeight)
    {
        var sourceX = Math.Clamp((int)Math.Floor(((logicalX + 0.5) * image.PngWidth) / logicalWidth), 0, image.PngWidth - 1);
        var sourceY = Math.Clamp((int)Math.Floor(((logicalY + 0.5) * image.PngHeight) / logicalHeight), 0, image.PngHeight - 1);
        var offset = checked(((sourceY * image.PngWidth) + sourceX) * 4);
        return (
            image.RgbaPixels[offset],
            image.RgbaPixels[offset + 1],
            image.RgbaPixels[offset + 2],
            image.RgbaPixels[offset + 3]);
    }

    private static (int R, int G, int B) CompositeOnWhite((int R, int G, int B, int A) pixel)
    {
        var alpha = pixel.A / 255d;
        return (
            (int)Math.Round((pixel.R * alpha) + (255 * (1d - alpha)), MidpointRounding.AwayFromZero),
            (int)Math.Round((pixel.G * alpha) + (255 * (1d - alpha)), MidpointRounding.AwayFromZero),
            (int)Math.Round((pixel.B * alpha) + (255 * (1d - alpha)), MidpointRounding.AwayFromZero));
    }

    private static byte[] BuildDiffPng(byte[] rgbaPixels, int width, int height)
    {
        var stride = checked(width * 4);
        var scanlines = new byte[checked(height * (stride + 1))];
        for (var y = 0; y < height; y++)
        {
            scanlines[y * (stride + 1)] = 0;
            Buffer.BlockCopy(rgbaPixels, y * stride, scanlines, (y * (stride + 1)) + 1, stride);
        }

        using var png = new MemoryStream();
        png.Write([137, 80, 78, 71, 13, 10, 26, 10]);
        WritePngChunk(png, "IHDR", PngIhdr(width, height));
        WritePngChunk(png, "IDAT", CompressZlib(scanlines));
        WritePngChunk(png, "IEND", []);
        return png.ToArray();
    }

    private static byte[] PngIhdr(int width, int height)
    {
        var ihdr = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(ihdr.AsSpan(0, 4), width);
        BinaryPrimitives.WriteInt32BigEndian(ihdr.AsSpan(4, 4), height);
        ihdr[8] = 8;
        ihdr[9] = 6;
        return ihdr;
    }

    private static byte[] CompressZlib(byte[] value)
    {
        using var output = new MemoryStream();
        using (var zlib = new ZLibStream(output, CompressionLevel.Optimal, leaveOpen: true))
            zlib.Write(value, 0, value.Length);
        return output.ToArray();
    }

    private static void WritePngChunk(Stream stream, string type, byte[] data)
    {
        Span<byte> length = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(length, data.Length);
        stream.Write(length);
        var typeBytes = Encoding.ASCII.GetBytes(type);
        stream.Write(typeBytes);
        stream.Write(data);
        Span<byte> crc = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crc, Crc32(typeBytes, data));
        stream.Write(crc);
    }

    private static uint Crc32(byte[] typeBytes, byte[] data)
    {
        var crc = 0xffffffffu;
        foreach (var value in typeBytes.Concat(data))
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++)
                crc = (crc & 1) == 1 ? (crc >> 1) ^ 0xedb88320u : crc >> 1;
        }

        return ~crc;
    }

    private static VisualDefect CreatePixelDifferenceDefect(
        DesignConversionQaComparisonRequest request,
        int differentPixelCount)
    {
        var width = Math.Max(1, request.Actual.Resolution.Width);
        var height = Math.Max(1, request.Actual.Resolution.Height);
        return new VisualDefect(
            DefectId: $"{request.ComparisonRequestId}:local-png-defect:pixels-differ",
            Category: "pixel-difference",
            Severity: "minor",
            Description: $"Local server-owned PNG comparison found {differentPixelCount.ToString(CultureInfo.InvariantCulture)} differing rendered pixels.",
            X: 0,
            Y: 0,
            Width: width,
            Height: Math.Max(1, Math.Round(height * 0.05, 3, MidpointRounding.AwayFromZero)),
            EvidenceImageIds: [request.Expected.ImageId, request.Actual.ImageId]);
    }

    private static bool SameLogicalResolution(RenderQaResolution expected, RenderQaResolution actual)
        => expected is not null &&
           actual is not null &&
           string.Equals(expected.ResolutionId, actual.ResolutionId, StringComparison.Ordinal) &&
           expected.Width == actual.Width &&
           expected.Height == actual.Height &&
           Math.Abs(expected.DeviceScaleFactor - actual.DeviceScaleFactor) < 0.000001;

    private static bool TryExtractShaRef(string? value, out string sha)
    {
        sha = string.Empty;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var text = value.Trim();
        if (text.StartsWith("local-png-ref:", StringComparison.OrdinalIgnoreCase))
            text = text["local-png-ref:".Length..];

        return TryNormalizeSha256(text, out sha);
    }

    private static bool TryNormalizeSha256(string? value, out string sha)
    {
        sha = string.Empty;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var text = value.Trim();
        if (text.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase))
            text = text["sha256:".Length..];
        if (text.Length != 64 || !text.All(Uri.IsHexDigit))
            return false;

        sha = text.ToLowerInvariant();
        return true;
    }

    private static string Sha256Bytes(byte[] value)
        => Convert.ToHexString(SHA256.HashData(value)).ToLowerInvariant();

    private static string SafePathSegment(string value)
    {
        var cleaned = new string((value ?? string.Empty)
            .Select(static c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' ? c : '-')
            .ToArray())
            .Trim('-');
        return string.IsNullOrWhiteSpace(cleaned) ? "comparison" : cleaned[..Math.Min(cleaned.Length, 80)];
    }

    private static bool IsExpectedFileSystemException(Exception exception)
        => exception is IOException
            or UnauthorizedAccessException
            or ArgumentException
            or NotSupportedException
            or PathTooLongException
            or System.Security.SecurityException;

    private static OperationResult<bool> HashLookupBudgetExceeded(
        string role,
        string budgetKind,
        string errorCode)
        => OperationResult<bool>.Fail(
            $"DesignConversion local PNG comparison {role} image content-hash lookup exceeded the configured {budgetKind} budget.",
            errorCode);

    private sealed record LocalComparisonImage(
        string LocalPath,
        DesignConversionDecodedPngComparisonEvidence Png);

    private sealed class HashLookupBudgetState
    {
        public int DirectoriesVisited { get; set; }
        public int FilesVisited { get; set; }
        public long BytesVisited { get; set; }
    }

    private sealed record LocalPngComparisonResult(
        double VisualDistance,
        double PixelDifferenceRatio,
        int DifferentPixelCount,
        int TotalPixelCount,
        byte[] DiffRgbaPixels);
}

public sealed class DeterministicLocalDesignConversionPngRenderProvider : IDesignConversionRenderProvider
{
    public const string ProviderKind = "deterministic_local_png_artifact_renderer";
    public const string VisualFidelityClaimedMarker = "visual_fidelity_claimed:false";
    public const string BrowserUsedMarker = "browser_or_playwright_used:false";
    public const string ExternalAiUsedMarker = "external_ai_used:false";
    public const string BoundaryMarker = "hidden_determinism_boundary:deterministic_local_png_artifact_evidence";
    public const string BytesWrittenMarker = "actual_png_bytes_written:true";

    private static readonly byte[] PngSignature = [137, 80, 78, 71, 13, 10, 26, 10];

    private readonly string _artifactRoot;

    public DeterministicLocalDesignConversionPngRenderProvider(string artifactRoot)
        => _artifactRoot = artifactRoot;

    public async Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
        DesignConversionRenderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<DesignConversionRenderResult>.Fail(
                "DesignConversion deterministic local PNG render cancelled.",
                "CANCELLED");
        if (request is null)
            return OperationResult<DesignConversionRenderResult>.Fail(
                "DesignConversion render request is required.",
                "RENDER_REQUEST_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.RenderRequestId))
            return OperationResult<DesignConversionRenderResult>.Fail(
                "DesignConversion render request id is required.",
                "RENDER_REQUEST_ID_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.RequestId))
            return OperationResult<DesignConversionRenderResult>.Fail(
                "DesignConversion request id is required.",
                "REQUEST_ID_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.Html))
            return OperationResult<DesignConversionRenderResult>.Fail(
                "DesignConversion submitted HTML is required for local PNG render artifacts.",
                "LOCAL_RENDER_HTML_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.Css))
            return OperationResult<DesignConversionRenderResult>.Fail(
                "DesignConversion submitted CSS is required for local PNG render artifacts.",
                "LOCAL_RENDER_CSS_REQUIRED");
        if (request.Resolution is null || request.Resolution.Width <= 0 || request.Resolution.Height <= 0 || request.Resolution.DeviceScaleFactor <= 0)
            return OperationResult<DesignConversionRenderResult>.Fail(
                "DesignConversion local PNG render resolution must have positive dimensions and scale.",
                "LOCAL_RENDER_RESOLUTION_INVALID");

        var root = ResolveSafeArtifactRoot(_artifactRoot);
        if (!root.IsSuccess)
            return root.MapError<DesignConversionRenderResult>();

        var htmlSha = DeterministicDesignConversionRenderProvider.Sha256Text(request.Html);
        var cssSha = DeterministicDesignConversionRenderProvider.Sha256Text(request.Css);
        var contentSeed = DeterministicDesignConversionRenderProvider.Sha256Text(string.Join(
            "|",
            ProviderKind,
            request.RequestId,
            request.Resolution.ResolutionId,
            request.Resolution.Width.ToString(CultureInfo.InvariantCulture),
            request.Resolution.Height.ToString(CultureInfo.InvariantCulture),
            request.Resolution.DeviceScaleFactor.ToString("R", CultureInfo.InvariantCulture),
            htmlSha,
            cssSha));
        var bytes = BuildDeterministicPng(
            request.Resolution.Width,
            request.Resolution.Height,
            contentSeed);
        var pngSha = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        var safeRunId = SafePathSegment($"{request.RequestId}-{request.RenderRequestId}");
        var safeViewport = SafePathSegment(request.Resolution.ResolutionId);
        var runRoot = Path.Combine(root.Value!, safeRunId);
        var screenshotPath = Path.Combine(runRoot, $"{safeViewport}-{pngSha[..16]}.png");
        var safety = EnsurePathUnderRoot(root.Value!, screenshotPath);
        if (!safety.IsSuccess)
            return safety.MapError<DesignConversionRenderResult>();

        Directory.CreateDirectory(runRoot);
        await File.WriteAllBytesAsync(screenshotPath, bytes, cancellationToken);

        var metadataRedactions = new SortedSet<string>(StringComparer.Ordinal);
        var safeInputMetadata = DesignConversionDpoLeakageGuard.SanitizeMetadata(
            request.Metadata,
            "deterministic_local_png_render_metadata",
            metadataRedactions);
        var metadata = DeterministicDesignConversionRenderProvider.MergeMetadata(
            safeInputMetadata,
            new Dictionary<string, string>
            {
                ["provider_kind"] = ProviderKind,
                ["render_artifact_kind"] = "deterministic_local_png_artifact",
                ["visual_fidelity_claimed"] = bool.FalseString,
                ["browser_or_playwright_used"] = bool.FalseString,
                ["external_ai_used"] = bool.FalseString,
                ["actual_png_bytes_written"] = bool.TrueString,
                ["hidden_determinism_boundary"] = "deterministic_local_png_artifact_evidence",
                ["html_sha256"] = htmlSha,
                ["css_sha256"] = cssSha,
                ["viewport_id"] = request.Resolution.ResolutionId,
                ["viewport_width"] = request.Resolution.Width.ToString(CultureInfo.InvariantCulture),
                ["viewport_height"] = request.Resolution.Height.ToString(CultureInfo.InvariantCulture),
                ["device_scale_factor"] = request.Resolution.DeviceScaleFactor.ToString("R", CultureInfo.InvariantCulture),
                ["artifact_file_name"] = Path.GetFileName(screenshotPath),
                ["artifact_path_ref"] = $"local-png://design-conversion/{safeRunId}/{safeViewport}/{pngSha[..16]}",
                ["input_metadata_redaction_count"] = metadataRedactions.Count.ToString(CultureInfo.InvariantCulture)
            });

        var image = new RenderQaImageRecord(
            ImageId: $"{request.RenderRequestId}:deterministic-local-png:{pngSha[..12]}",
            Label: "deterministic-local-png-render-artifact",
            Resolution: request.Resolution,
            Uri: new Uri(screenshotPath).AbsoluteUri,
            Sha256: $"sha256:{pngSha}",
            Metadata: metadata);

        return OperationResult<DesignConversionRenderResult>.Ok(new DesignConversionRenderResult(
            RenderResultId: $"{request.RenderRequestId}:deterministic-local-png-render:{pngSha[..12]}",
            RenderRequestId: request.RenderRequestId,
            Image: image,
            ElapsedMs: 0,
            Warnings:
            [
                "DETERMINISTIC_LOCAL_RENDER_ARTIFACT_NOT_BROWSER_SCREENSHOT",
                "DETERMINISTIC_LOCAL_RENDER_ARTIFACT_DOES_NOT_CLAIM_VISUAL_FIDELITY",
                "VISUAL_AI_NOT_EXECUTED_LOCAL_RENDER_ARTIFACT"
            ]));
    }

    private static OperationResult<string> ResolveSafeArtifactRoot(string artifactRoot)
    {
        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<string>.Fail(
                "DesignConversion local PNG artifact root is required.",
                "LOCAL_RENDER_ARTIFACT_ROOT_REQUIRED");
        if (artifactRoot.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            return OperationResult<string>.Fail(
                "DesignConversion local PNG artifact root contains invalid path characters.",
                "LOCAL_RENDER_ARTIFACT_ROOT_UNSAFE");
        if (!Path.IsPathFullyQualified(artifactRoot))
            return OperationResult<string>.Fail(
                "DesignConversion local PNG artifact root must be an absolute local path.",
                "LOCAL_RENDER_ARTIFACT_ROOT_UNSAFE");

        var fullPath = Path.GetFullPath(artifactRoot);
        var lastSegment = Path.GetFileName(fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        if (lastSegment is "." or ".." || fullPath.Contains($"{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
        {
            return OperationResult<string>.Fail(
                "DesignConversion local PNG artifact root must not contain traversal segments.",
                "LOCAL_RENDER_ARTIFACT_ROOT_UNSAFE");
        }

        return OperationResult<string>.Ok(fullPath);
    }

    private static OperationResult<bool> EnsurePathUnderRoot(string root, string candidatePath)
    {
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var fullCandidate = Path.GetFullPath(candidatePath);
        if (!fullCandidate.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion local PNG artifact path escaped the configured root.",
                "LOCAL_RENDER_ARTIFACT_PATH_UNSAFE");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static string SafePathSegment(string? value)
    {
        var source = string.IsNullOrWhiteSpace(value) ? "viewport" : value;
        var builder = new StringBuilder(source.Length);
        foreach (var ch in source)
            builder.Append(char.IsLetterOrDigit(ch) || ch is '-' or '_' ? ch : '-');

        var segment = builder.ToString().Trim('-');
        return string.IsNullOrWhiteSpace(segment)
            ? DeterministicDesignConversionRenderProvider.Sha256Text(source)[..16]
            : segment.Length <= 80
                ? segment
                : $"{segment[..48]}-{DeterministicDesignConversionRenderProvider.Sha256Text(segment)[..16]}";
    }

    private static byte[] BuildDeterministicPng(int width, int height, string seedHex)
    {
        var rgba = new byte[checked(width * height * 4)];
        var seedA = Convert.ToInt32(seedHex[..2], 16);
        var seedB = Convert.ToInt32(seedHex.Substring(2, 2), 16);
        var seedC = Convert.ToInt32(seedHex.Substring(4, 2), 16);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var offset = ((y * width) + x) * 4;
                var stripe = ((x / Math.Max(1, width / 12)) + (y / Math.Max(1, height / 10))) % 2;
                rgba[offset] = (byte)((seedA + x + (stripe * 60)) % 256);
                rgba[offset + 1] = (byte)((seedB + y + (stripe * 35)) % 256);
                rgba[offset + 2] = (byte)((seedC + x + y + 96) % 256);
                rgba[offset + 3] = 255;
            }
        }

        using var png = new MemoryStream();
        png.Write(PngSignature);
        WriteChunk(png, "IHDR", BuildIhdr(width, height));
        WriteChunk(png, "IDAT", CompressScanlines(rgba, width, height));
        WriteChunk(png, "IEND", []);
        return png.ToArray();
    }

    private static byte[] BuildIhdr(int width, int height)
    {
        var data = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(0, 4), width);
        BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(4, 4), height);
        data[8] = 8;
        data[9] = 6;
        data[10] = 0;
        data[11] = 0;
        data[12] = 0;
        return data;
    }

    private static byte[] CompressScanlines(byte[] rgba, int width, int height)
    {
        var stride = width * 4;
        using var raw = new MemoryStream();
        for (var y = 0; y < height; y++)
        {
            raw.WriteByte(0);
            raw.Write(rgba, y * stride, stride);
        }

        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Fastest, leaveOpen: true))
            zlib.Write(raw.ToArray());
        return compressed.ToArray();
    }

    private static void WriteChunk(Stream stream, string type, byte[] data)
    {
        var typeBytes = Encoding.ASCII.GetBytes(type);
        Span<byte> lengthBytes = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(lengthBytes, data.Length);
        stream.Write(lengthBytes);
        stream.Write(typeBytes);
        stream.Write(data);
        var crcInput = new byte[typeBytes.Length + data.Length];
        Array.Copy(typeBytes, 0, crcInput, 0, typeBytes.Length);
        Array.Copy(data, 0, crcInput, typeBytes.Length, data.Length);
        Span<byte> crcBytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crcBytes, Crc32(crcInput));
        stream.Write(crcBytes);
    }

    private static uint Crc32(ReadOnlySpan<byte> bytes)
    {
        var crc = 0xffffffffu;
        foreach (var b in bytes)
        {
            crc ^= b;
            for (var bit = 0; bit < 8; bit++)
                crc = (crc & 1) == 1 ? (crc >> 1) ^ 0xedb88320u : crc >> 1;
        }

        return ~crc;
    }
}

public sealed class DesignConversionBrowserRenderManifestProvider
{
    public const string BrowserRenderQaMarker = "browser_render_qa";
    public const string MultiResolutionRenderMarker = "multi_resolution_render";
    public const string PngArtifactsMarker = "png_artifacts";
    public const string NonBlankPixelsMarker = "nonblank_pixels";
    public const string BrowserRenderReceiptSnapshotMismatchCode = "BROWSER_RENDER_RECEIPT_SNAPSHOT_MISMATCH";

    private readonly IDesignConversionRenderProvider _renderProvider;

    public DesignConversionBrowserRenderManifestProvider(IDesignConversionRenderProvider renderProvider)
        => _renderProvider = renderProvider ?? throw new ArgumentNullException(nameof(renderProvider));

    public async Task<OperationResult<DesignConversionBrowserRenderManifestResult>> RenderManifestAsync(
        DesignConversionBrowserRenderManifestRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateRequest(request);
        if (!validation.IsSuccess)
            return validation.MapError<DesignConversionBrowserRenderManifestResult>();

        var entries = new List<DesignConversionBrowserRenderManifestEntry>();
        var warnings = new List<string>();
        var renderRequests = BuildManifestRenderRequests(request);
        var renderResults = await RenderManifestViewportsAsync(renderRequests, cancellationToken);
        if (!renderResults.IsSuccess)
            return renderResults.MapError<DesignConversionBrowserRenderManifestResult>();

        for (var index = 0; index < renderRequests.Length; index++)
        {
            var renderRequest = renderRequests[index];
            var resolution = renderRequest.Resolution;
            var render = renderResults.Value![index];

            if (!render.IsSuccess)
                return render.MapError<DesignConversionBrowserRenderManifestResult>();

            var entry = await TryCreateManifestEntryAsync(
                resolution,
                renderRequest,
                render.Value!,
                cancellationToken);
            if (!entry.IsSuccess)
                return entry.MapError<DesignConversionBrowserRenderManifestResult>();

            entries.Add(entry.Value!.Entry);
            warnings.AddRange(entry.Value.Warnings);
        }

        var coverage = ValidateRequiredViewportCoverage(request.RequiredResolutions, entries);
        if (!coverage.IsSuccess)
            return coverage.MapError<DesignConversionBrowserRenderManifestResult>();

        var browserExecuted = entries.Any(entry => IsTruthy(entry.Metadata, "browser_or_playwright_used"));
        if (!browserExecuted)
            warnings.Add("BROWSER_RENDER_NOT_EXECUTED_LOCAL_ARTIFACT_RENDERER");

        return OperationResult<DesignConversionBrowserRenderManifestResult>.Ok(
            new DesignConversionBrowserRenderManifestResult(
                RenderRunId: request.RenderRunId,
                Entries: entries,
                EvidenceRefs:
                [
                    BrowserRenderQaMarker,
                    MultiResolutionRenderMarker,
                    PngArtifactsMarker,
                    NonBlankPixelsMarker,
                    browserExecuted ? "browser_or_playwright_used:true" : "browser_or_playwright_used:false",
                    "external_ai_used:false"
                ],
                Warnings: warnings.Distinct(StringComparer.Ordinal).ToArray()));
    }

    private async Task<OperationResult<IReadOnlyList<OperationResult<DesignConversionRenderResult>>>> RenderManifestViewportsAsync(
        IReadOnlyList<DesignConversionRenderRequest> renderRequests,
        CancellationToken cancellationToken)
    {
        if (renderRequests.Count > 1 &&
            _renderProvider is IDesignConversionBatchRenderProvider batchRenderProvider)
        {
            return await RenderManifestBatchAsync(batchRenderProvider, renderRequests, cancellationToken);
        }

        var results = new OperationResult<DesignConversionRenderResult>[renderRequests.Count];
        for (var index = 0; index < renderRequests.Count; index++)
            results[index] = await _renderProvider.RenderAsync(renderRequests[index], cancellationToken);

        return OperationResult<IReadOnlyList<OperationResult<DesignConversionRenderResult>>>.Ok(results);
    }

    private static async Task<OperationResult<IReadOnlyList<OperationResult<DesignConversionRenderResult>>>> RenderManifestBatchAsync(
        IDesignConversionBatchRenderProvider batchRenderProvider,
        IReadOnlyList<DesignConversionRenderRequest> renderRequests,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<OperationResult<DesignConversionRenderResult>>? batchResults;
        try
        {
            batchResults = await batchRenderProvider.RenderBatchAsync(renderRequests, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            return OperationResult<IReadOnlyList<OperationResult<DesignConversionRenderResult>>>.Fail(
                "DesignConversion browser render manifest batch render was cancelled.",
                "CANCELLED");
        }
        catch
        {
            return OperationResult<IReadOnlyList<OperationResult<DesignConversionRenderResult>>>.Fail(
                "DesignConversion browser render manifest batch render provider failed.",
                "BROWSER_RENDER_BATCH_PROVIDER_EXCEPTION");
        }

        if (batchResults is null)
        {
            return OperationResult<IReadOnlyList<OperationResult<DesignConversionRenderResult>>>.Fail(
                "DesignConversion browser render manifest batch provider returned a null result envelope.",
                "BROWSER_RENDER_BATCH_RESULT_ENVELOPE_NULL");
        }

        if (batchResults.Count != renderRequests.Count)
        {
            return OperationResult<IReadOnlyList<OperationResult<DesignConversionRenderResult>>>.Fail(
                "DesignConversion browser render manifest batch provider returned an invalid result count.",
                "BROWSER_RENDER_BATCH_RESULT_COUNT_MISMATCH");
        }

        for (var index = 0; index < batchResults.Count; index++)
        {
            if (batchResults[index] is null)
            {
                return OperationResult<IReadOnlyList<OperationResult<DesignConversionRenderResult>>>.Fail(
                    "DesignConversion browser render manifest batch provider returned a missing viewport result.",
                    "BROWSER_RENDER_BATCH_RESULT_REQUIRED");
            }
        }

        return OperationResult<IReadOnlyList<OperationResult<DesignConversionRenderResult>>>.Ok(batchResults);
    }

    private static DesignConversionRenderRequest[] BuildManifestRenderRequests(
        DesignConversionBrowserRenderManifestRequest request)
    {
        var renderRequests = new DesignConversionRenderRequest[request.RequiredResolutions.Count];
        for (var index = 0; index < request.RequiredResolutions.Count; index++)
        {
            renderRequests[index] = new DesignConversionRenderRequest(
                RenderRequestId: $"{request.RenderRunId}:render:{index + 1}",
                RequestId: request.RequestId,
                Html: request.Html,
                Css: request.Css,
                Resolution: request.RequiredResolutions[index],
                Metadata: request.Metadata);
        }

        return renderRequests;
    }

    private async Task<OperationResult<ProcessedBrowserRenderManifestEntry>> TryCreateManifestEntryAsync(
        RenderQaResolution resolution,
        DesignConversionRenderRequest renderRequest,
        DesignConversionRenderResult render,
        CancellationToken cancellationToken)
    {
        if (render.BrowserRenderProofReceipt is not null)
        {
            var canonicalRender = DesignConversionRenderEvidenceTrustBoundary.Canonicalize(
                _renderProvider,
                renderRequest,
                render);
            if (!canonicalRender.IsSuccess)
                return canonicalRender.MapError<ProcessedBrowserRenderManifestEntry>();

            var receiptEntry = await TryCreateReceiptBackedManifestEntryAsync(
                resolution,
                canonicalRender.Value!,
                cancellationToken);
            if (!receiptEntry.IsSuccess)
                return receiptEntry.MapError<ProcessedBrowserRenderManifestEntry>();

            return OperationResult<ProcessedBrowserRenderManifestEntry>.Ok(
                new ProcessedBrowserRenderManifestEntry(
                    receiptEntry.Value!,
                    canonicalRender.Value!.Warnings));
        }

        if (DesignConversionBrowserPngEvidenceInspector.IsRemoteHttpOrHttpsScreenshotUri(render.Image.Uri))
        {
            return OperationResult<ProcessedBrowserRenderManifestEntry>.Fail(
                "Browser render manifest remote screenshot URI requires an internal receipt-backed local snapshot.",
                "BROWSER_RENDER_REMOTE_SCREENSHOT_REQUIRES_RECEIPT");
        }

        var pngEvidence = await DesignConversionBrowserPngEvidenceInspector.InspectAsync(
            render.Image.Uri,
            cancellationToken: cancellationToken);
        if (!pngEvidence.IsSuccess)
            return pngEvidence.MapError<ProcessedBrowserRenderManifestEntry>();

        using var inspectedPng = pngEvidence.Value!;
        if (!inspectedPng.HasNonBlankPixels)
        {
            return OperationResult<ProcessedBrowserRenderManifestEntry>.Fail(
                $"Browser render produced a blank PNG for viewport '{resolution.ResolutionId}'.",
                "BROWSER_RENDER_BLANK_PNG");
        }

        return OperationResult<ProcessedBrowserRenderManifestEntry>.Ok(
            new ProcessedBrowserRenderManifestEntry(
                new DesignConversionBrowserRenderManifestEntry(
                    ViewportId: resolution.ResolutionId,
                    RequestedWidth: resolution.Width,
                    RequestedHeight: resolution.Height,
                    DeviceScaleFactor: resolution.DeviceScaleFactor,
                    ScreenshotUri: render.Image.Uri,
                    LocalPath: inspectedPng.LocalPath,
                    Sha256: render.Image.Sha256,
                    ByteLength: inspectedPng.ByteLength,
                    PngWidth: inspectedPng.PngWidth,
                    PngHeight: inspectedPng.PngHeight,
                    HasNonBlankPixels: inspectedPng.HasNonBlankPixels,
                    NonWhitePixelCount: inspectedPng.NonWhitePixelCount,
                    DistinctColorCount: inspectedPng.DistinctColorCount,
                    ElapsedMs: render.ElapsedMs,
                    Metadata: render.Image.Metadata),
                render.Warnings));
    }

    private sealed record ProcessedBrowserRenderManifestEntry(
        DesignConversionBrowserRenderManifestEntry Entry,
        IReadOnlyList<string> Warnings);

    public static OperationResult<bool> ValidateRequiredViewportCoverage(
        IReadOnlyList<RenderQaResolution> requiredResolutions,
        IReadOnlyList<DesignConversionBrowserRenderManifestEntry> entries)
    {
        if (requiredResolutions is null || requiredResolutions.Count == 0)
            return OperationResult<bool>.Fail(
                "Browser render QA requires at least one viewport.",
                "BROWSER_RENDER_VIEWPORTS_REQUIRED");
        if (entries is null)
            return OperationResult<bool>.Fail(
                "Browser render QA manifest entries are required.",
                "BROWSER_RENDER_MANIFEST_REQUIRED");

        var observed = entries
            .Select(entry => entry.ViewportId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missing = requiredResolutions
            .Select(resolution => resolution.ResolutionId)
            .Where(id => !observed.Contains(id))
            .ToArray();
        if (missing.Length > 0)
        {
            return OperationResult<bool>.Fail(
                $"Browser render QA missing required viewport(s): {string.Join(", ", missing)}.",
                "BROWSER_RENDER_VIEWPORT_MISSING");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static async Task<OperationResult<DesignConversionBrowserRenderManifestEntry>> TryCreateReceiptBackedManifestEntryAsync(
        RenderQaResolution resolution,
        DesignConversionRenderResult render,
        CancellationToken cancellationToken)
    {
        var receipt = render.BrowserRenderProofReceipt;
        if (receipt is null)
            return OperationResult<DesignConversionBrowserRenderManifestEntry>.Fail(
                "DesignConversion browser render proof receipt is required for receipt-backed manifest construction.",
                "BROWSER_RENDER_PROOF_RECEIPT_REQUIRED");

        var snapshot = await ValidateReceiptSnapshotAsync(receipt, cancellationToken);
        if (!snapshot.IsSuccess)
            return snapshot.MapError<DesignConversionBrowserRenderManifestEntry>();

        return OperationResult<DesignConversionBrowserRenderManifestEntry>.Ok(new DesignConversionBrowserRenderManifestEntry(
            ViewportId: resolution.ResolutionId,
            RequestedWidth: resolution.Width,
            RequestedHeight: resolution.Height,
            DeviceScaleFactor: resolution.DeviceScaleFactor,
            ScreenshotUri: render.Image.Uri,
            LocalPath: snapshot.Value!.LocalPath,
            Sha256: $"sha256:{receipt.NormalizedSha256}",
            ByteLength: receipt.ByteLength,
            PngWidth: receipt.PngWidth,
            PngHeight: receipt.PngHeight,
            HasNonBlankPixels: receipt.HasNonBlankPixels,
            NonWhitePixelCount: receipt.NonWhitePixelCount,
            DistinctColorCount: receipt.DistinctColorCount,
            ElapsedMs: render.ElapsedMs,
            Metadata: render.Image.Metadata));
    }

    private static async Task<OperationResult<DesignConversionBrowserPngEvidence>> ValidateReceiptSnapshotAsync(
        DesignConversionBrowserRenderProofReceipt receipt,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(receipt.InspectedLocalPath))
            return ReceiptSnapshotUnavailable();

        var path = CanonicalizeReceiptSnapshotPath(receipt.InspectedLocalPath);
        if (!path.IsSuccess)
            return path.MapError<DesignConversionBrowserPngEvidence>();

        FileInfo fileInfo;
        try
        {
            fileInfo = new FileInfo(path.Value!);
            if (!fileInfo.Exists)
                return ReceiptSnapshotUnavailable();

            fileInfo.Refresh();
            if ((fileInfo.Attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0)
                return ReceiptSnapshotInvalid();
            if (fileInfo.Length <= 0 ||
                fileInfo.Length > DesignConversionBrowserPngEvidenceInspector.MaxPngScreenshotBytes ||
                fileInfo.Length != receipt.ByteLength)
            {
                return ReceiptSnapshotMismatch();
            }
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or NotSupportedException or UnauthorizedAccessException)
        {
            return ReceiptSnapshotInvalid();
        }

        var sha = await ComputeSha256BoundedAsync(fileInfo.FullName, receipt.ByteLength, cancellationToken);
        if (!sha.IsSuccess)
            return sha.MapError<DesignConversionBrowserPngEvidence>();
        if (!string.Equals(sha.Value, receipt.NormalizedSha256, StringComparison.Ordinal))
            return ReceiptSnapshotMismatch();

        return OperationResult<DesignConversionBrowserPngEvidence>.Ok(new DesignConversionBrowserPngEvidence(
            LocalPath: fileInfo.FullName,
            ByteLength: receipt.ByteLength,
            PngWidth: receipt.PngWidth,
            PngHeight: receipt.PngHeight,
            HasNonBlankPixels: receipt.HasNonBlankPixels,
            NonWhitePixelCount: receipt.NonWhitePixelCount,
            DistinctColorCount: receipt.DistinctColorCount)
        {
            SnapshotSha256 = sha.Value!
        });
    }

    private static OperationResult<string> CanonicalizeReceiptSnapshotPath(string localPath)
    {
        try
        {
            if (localPath.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase) ||
                localPath.StartsWith(@"\\", StringComparison.Ordinal))
            {
                return ReceiptSnapshotInvalid().MapError<string>();
            }

            if (!Path.IsPathFullyQualified(localPath))
            {
                if (!Uri.TryCreate(localPath, UriKind.Absolute, out var uri) ||
                    !uri.IsFile ||
                    uri.IsUnc ||
                    !string.IsNullOrWhiteSpace(uri.Host))
                {
                    return ReceiptSnapshotInvalid().MapError<string>();
                }

                localPath = uri.LocalPath;
            }

            var fullPath = Path.GetFullPath(localPath);
            if (!Path.IsPathFullyQualified(fullPath))
                return ReceiptSnapshotInvalid().MapError<string>();

            return OperationResult<string>.Ok(fullPath);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException or UriFormatException)
        {
            return ReceiptSnapshotInvalid().MapError<string>();
        }
    }

    private static async Task<OperationResult<string>> ComputeSha256BoundedAsync(
        string path,
        long expectedByteLength,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81920,
                options: FileOptions.Asynchronous | FileOptions.SequentialScan);
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            var buffer = new byte[81920];
            long totalRead = 0;
            while (true)
            {
                var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                if (read == 0)
                    break;

                totalRead += read;
                if (totalRead > expectedByteLength ||
                    totalRead > DesignConversionBrowserPngEvidenceInspector.MaxPngScreenshotBytes)
                {
                    return ReceiptSnapshotMismatch().MapError<string>();
                }

                hash.AppendData(buffer, 0, read);
            }

            if (totalRead != expectedByteLength)
                return ReceiptSnapshotMismatch().MapError<string>();

            return OperationResult<string>.Ok(Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant());
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return OperationResult<string>.Fail(
                "DesignConversion browser render manifest construction was cancelled.",
                "CANCELLED");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return ReceiptSnapshotUnavailable().MapError<string>();
        }
    }

    private static OperationResult<DesignConversionBrowserPngEvidence> ReceiptSnapshotUnavailable()
        => OperationResult<DesignConversionBrowserPngEvidence>.Fail(
            "DesignConversion browser render proof receipt snapshot is unavailable.",
            "BROWSER_RENDER_RECEIPT_SNAPSHOT_UNAVAILABLE");

    private static OperationResult<DesignConversionBrowserPngEvidence> ReceiptSnapshotInvalid()
        => OperationResult<DesignConversionBrowserPngEvidence>.Fail(
            "DesignConversion browser render proof receipt snapshot is not a regular local file.",
            "BROWSER_RENDER_RECEIPT_SNAPSHOT_INVALID");

    private static OperationResult<DesignConversionBrowserPngEvidence> ReceiptSnapshotMismatch()
        => OperationResult<DesignConversionBrowserPngEvidence>.Fail(
            "DesignConversion browser render proof receipt snapshot changed before manifest construction.",
            BrowserRenderReceiptSnapshotMismatchCode);

    private static bool IsTruthy(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) &&
           (string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "1", StringComparison.Ordinal) ||
            string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase));

    private static OperationResult<bool> ValidateRequest(DesignConversionBrowserRenderManifestRequest request)
    {
        if (request is null)
            return OperationResult<bool>.Fail("Browser render manifest request is required.", "BROWSER_RENDER_MANIFEST_REQUEST_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.RenderRunId))
            return OperationResult<bool>.Fail("Browser render manifest run id is required.", "BROWSER_RENDER_RUN_ID_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.RequestId))
            return OperationResult<bool>.Fail("Browser render manifest request id is required.", "BROWSER_RENDER_REQUEST_ID_REQUIRED");
        if (request.RequiredResolutions is null || request.RequiredResolutions.Count == 0)
            return OperationResult<bool>.Fail("Browser render QA requires viewports.", "BROWSER_RENDER_VIEWPORTS_REQUIRED");

        return OperationResult<bool>.Ok(true);
    }
}

public static class DesignConversionBrowserPngEvidenceInspector
{
    private static readonly byte[] PngSignature = [137, 80, 78, 71, 13, 10, 26, 10];
    private static readonly uint[] PngCrc32Table = BuildPngCrc32Table();
    internal const long MaxPngScreenshotBytes = DesignConversionBoundedContentReader.HardMaxBytes;
    internal const long MaxPngScreenshotPixels = 16_777_216;
    private const int MinimumSparseNonWhitePixels = 2;
    private const int SparseNonWhitePixelRatioDivisor = 1024;
    private const int PngIhdrHeaderLength = 33;
    internal const string RemoteFetchRateLimitedCode = "PNG_REMOTE_RATE_LIMITED";
    internal const int DefaultRemoteFetchOperationLimitPerWindow = 120;
    internal static readonly TimeSpan DefaultRemoteFetchOperationRateLimitWindow = TimeSpan.FromSeconds(60);
    private static readonly IDesignConversionRemotePngDnsResolver DefaultDnsResolver = new SystemDesignConversionRemotePngDnsResolver();
    private static readonly IDesignConversionRemotePngHttpTransport DefaultHttpTransport = new PinnedAddressDesignConversionRemotePngHttpTransport();
    private static readonly DesignConversionBrowserQaOperationRateLimiter DefaultRemoteFetchOperationRateLimiter = new(
        DefaultRemoteFetchOperationLimitPerWindow,
        DefaultRemoteFetchOperationRateLimitWindow);

    public static DesignConversionRemotePngFetchPolicy DefaultRemoteFetchPolicy { get; } = new(
        AllowHttp: true,
        AllowHttps: true,
        MaxRedirects: 5,
        MaxBytes: MaxPngScreenshotBytes,
        Timeout: TimeSpan.FromSeconds(10));

    internal static bool IsRemoteHttpOrHttpsScreenshotUri(string screenshotUri)
        => Uri.TryCreate(screenshotUri, UriKind.Absolute, out var uri) &&
           IsHttpOrHttpsScheme(uri.Scheme);

    public static async Task<OperationResult<DesignConversionBrowserPngEvidence>> InspectAsync(
        string screenshotUri,
        IDesignConversionRemotePngDnsResolver? dnsResolver = null,
        IDesignConversionRemotePngHttpTransport? httpTransport = null,
        DesignConversionRemotePngFetchPolicy? fetchPolicy = null,
        CancellationToken cancellationToken = default)
        => await InspectCoreAsync(
            screenshotUri,
            dnsResolver,
            httpTransport,
            fetchPolicy,
            DefaultRemoteFetchOperationRateLimiter,
            cancellationToken);

    internal static async Task<OperationResult<DesignConversionBrowserPngEvidence>> InspectWithRemoteRateLimitForTestingAsync(
        string screenshotUri,
        IDesignConversionRemotePngDnsResolver? dnsResolver,
        IDesignConversionRemotePngHttpTransport? httpTransport,
        DesignConversionRemotePngFetchPolicy? fetchPolicy,
        DesignConversionBrowserQaOperationRateLimiter remoteOperationRateLimiter,
        CancellationToken cancellationToken = default)
    {
        if (remoteOperationRateLimiter is null)
            throw new ArgumentNullException(nameof(remoteOperationRateLimiter));

        return await InspectCoreAsync(
            screenshotUri,
            dnsResolver,
            httpTransport,
            fetchPolicy,
            remoteOperationRateLimiter,
            cancellationToken);
    }

    private static async Task<OperationResult<DesignConversionBrowserPngEvidence>> InspectCoreAsync(
        string screenshotUri,
        IDesignConversionRemotePngDnsResolver? dnsResolver,
        IDesignConversionRemotePngHttpTransport? httpTransport,
        DesignConversionRemotePngFetchPolicy? fetchPolicy,
        DesignConversionBrowserQaOperationRateLimiter remoteOperationRateLimiter,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(screenshotUri))
            return OperationResult<DesignConversionBrowserPngEvidence>.Fail(
                "PNG screenshot uri is required.",
                "PNG_SCREENSHOT_URI_REQUIRED");

        if (Uri.TryCreate(screenshotUri, UriKind.Absolute, out var uri) &&
            IsHttpOrHttpsScheme(uri.Scheme))
        {
            return await InspectRemoteAsync(
                uri,
                dnsResolver ?? DefaultDnsResolver,
                httpTransport ?? DefaultHttpTransport,
                fetchPolicy ?? DefaultRemoteFetchPolicy,
                remoteOperationRateLimiter,
                cancellationToken);
        }

        return await InspectLocalAsync(screenshotUri, cancellationToken);
    }

    public static OperationResult<DesignConversionBrowserPngEvidence> Inspect(string screenshotUri)
    {
        if (string.IsNullOrWhiteSpace(screenshotUri))
            return OperationResult<DesignConversionBrowserPngEvidence>.Fail(
                "PNG screenshot uri is required.",
                "PNG_SCREENSHOT_URI_REQUIRED");

        var path = ResolveLocalPath(screenshotUri);
        if (!path.IsSuccess)
            return path.MapError<DesignConversionBrowserPngEvidence>();

        if (!File.Exists(path.Value!))
            return OperationResult<DesignConversionBrowserPngEvidence>.Fail(
                $"PNG screenshot file was not found for uri '{screenshotUri}'.",
                "PNG_SCREENSHOT_FILE_MISSING");

        try
        {
            var fileInfo = new FileInfo(path.Value!);
            fileInfo.Refresh();
            if (fileInfo.Length > MaxPngScreenshotBytes)
                return OperationResult<DesignConversionBrowserPngEvidence>.Fail(
                    $"PNG screenshot is too large to inspect safely: {fileInfo.Length.ToString(CultureInfo.InvariantCulture)} bytes exceeds {MaxPngScreenshotBytes.ToString(CultureInfo.InvariantCulture)} bytes.",
                    "PNG_SCREENSHOT_BYTES_TOO_LARGE");

            var bytes = ReadLocalPngBytes(
                path.Value!,
                fileInfo.Length,
                fileInfo.CreationTimeUtc,
                fileInfo.LastWriteTimeUtc);
            if (!bytes.IsSuccess)
                return bytes.MapError<DesignConversionBrowserPngEvidence>();

            return InspectPngBytes(bytes.Value!, path.Value!);
        }
        catch (Exception exception) when (IsExpectedLocalPngFileSystemException(exception))
        {
            return LocalPngReadFailed();
        }
    }

    private static async Task<OperationResult<DesignConversionBrowserPngEvidence>> InspectLocalAsync(
        string screenshotUri,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return LocalPngInspectionCancelled();

        var path = ResolveLocalPath(screenshotUri);
        if (!path.IsSuccess)
            return path.MapError<DesignConversionBrowserPngEvidence>();

        try
        {
            if (!File.Exists(path.Value!))
                return OperationResult<DesignConversionBrowserPngEvidence>.Fail(
                    $"PNG screenshot file was not found for uri '{screenshotUri}'.",
                    "PNG_SCREENSHOT_FILE_MISSING");

            var bytes = await ReadLocalPngBytesAsync(path.Value!, cancellationToken);
            if (!bytes.IsSuccess)
                return bytes.MapError<DesignConversionBrowserPngEvidence>();

            return InspectPngBytes(bytes.Value!, path.Value!);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return LocalPngInspectionCancelled();
        }
        catch (Exception exception) when (IsExpectedLocalPngFileSystemException(exception))
        {
            return LocalPngReadFailed();
        }
    }

    private static async Task<OperationResult<byte[]>> ReadLocalPngBytesAsync(
        string path,
        CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);

        var expectedLength = stream.Length;
        if (expectedLength > MaxPngScreenshotBytes)
            return OperationResult<byte[]>.Fail(
                $"PNG screenshot is too large to inspect safely: {expectedLength.ToString(CultureInfo.InvariantCulture)} bytes exceeds {MaxPngScreenshotBytes.ToString(CultureInfo.InvariantCulture)} bytes.",
                "PNG_SCREENSHOT_BYTES_TOO_LARGE");

        using var bytes = new MemoryStream(capacity: (int)Math.Min(Math.Max(expectedLength, 0), 81920));
        var buffer = new byte[81920];
        long totalRead = 0;
        while (true)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
            if (read == 0)
                break;

            totalRead += read;
            if (totalRead > MaxPngScreenshotBytes)
                return OperationResult<byte[]>.Fail(
                    "PNG screenshot grew beyond the configured byte limit while being inspected.",
                    "PNG_SCREENSHOT_BYTES_TOO_LARGE");
            if (totalRead > expectedLength)
                return LocalPngChangedDuringRead().MapError<byte[]>();

            bytes.Write(buffer, 0, read);
        }

        if (totalRead != expectedLength || stream.Length != expectedLength)
            return LocalPngChangedDuringRead().MapError<byte[]>();

        return OperationResult<byte[]>.Ok(bytes.ToArray());
    }

    private static OperationResult<byte[]> ReadLocalPngBytes(
        string path,
        long precheckedLength,
        DateTime precheckedCreationTimeUtc,
        DateTime precheckedLastWriteTimeUtc)
    {
        using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            options: FileOptions.SequentialScan);

        var openedLength = stream.Length;
        if (openedLength != precheckedLength)
            return LocalPngChangedDuringRead().MapError<byte[]>();
        if (openedLength > MaxPngScreenshotBytes)
            return OperationResult<byte[]>.Fail(
                $"PNG screenshot is too large to inspect safely: {openedLength.ToString(CultureInfo.InvariantCulture)} bytes exceeds {MaxPngScreenshotBytes.ToString(CultureInfo.InvariantCulture)} bytes.",
                "PNG_SCREENSHOT_BYTES_TOO_LARGE");

        OperationResult<byte[]> bytes;
        if (openedLength == 0)
        {
            bytes = OperationResult<byte[]>.Ok([]);
        }
        else
        {
            bytes = DesignConversionBoundedContentReader.ReadToEnd(
                stream,
                MaxPngScreenshotBytes,
                "PNG screenshot",
                "PNG_SCREENSHOT_BYTES_TOO_LARGE",
                "PNG_SCREENSHOT_FILE_CHANGED");
            if (!bytes.IsSuccess)
                return bytes;
        }

        if (stream.Length != openedLength)
            return LocalPngChangedDuringRead().MapError<byte[]>();

        var finalInfo = new FileInfo(path);
        finalInfo.Refresh();
        if (!finalInfo.Exists ||
            finalInfo.Length != precheckedLength ||
            finalInfo.CreationTimeUtc != precheckedCreationTimeUtc ||
            finalInfo.LastWriteTimeUtc != precheckedLastWriteTimeUtc)
        {
            return LocalPngChangedDuringRead().MapError<byte[]>();
        }

        return bytes;
    }

    private static OperationResult<DesignConversionBrowserPngEvidence> LocalPngInspectionCancelled()
        => OperationResult<DesignConversionBrowserPngEvidence>.Fail(
            "PNG screenshot inspection cancelled.",
            "CANCELLED");

    private static OperationResult<DesignConversionBrowserPngEvidence> LocalPngReadFailed()
        => OperationResult<DesignConversionBrowserPngEvidence>.Fail(
            "PNG screenshot file could not be read.",
            "PNG_SCREENSHOT_FILE_READ_FAILED");

    private static OperationResult<DesignConversionBrowserPngEvidence> LocalPngChangedDuringRead()
        => OperationResult<DesignConversionBrowserPngEvidence>.Fail(
            "PNG screenshot file changed while it was being inspected.",
            "PNG_SCREENSHOT_FILE_CHANGED");

    private static bool IsExpectedLocalPngFileSystemException(Exception exception)
        => exception is IOException
            or UnauthorizedAccessException
            or ArgumentException
            or NotSupportedException
            or System.Security.SecurityException;

    internal static OperationResult<DesignConversionDecodedPngComparisonEvidence> DecodePngBytesForComparison(
        byte[] bytes,
        string localPath)
    {
        if (bytes.Length < PngSignature.Length || !bytes.AsSpan(0, PngSignature.Length).SequenceEqual(PngSignature))
            return OperationResult<DesignConversionDecodedPngComparisonEvidence>.Fail(
                "Comparison PNG is not a PNG file.",
                "COMPARISON_PNG_SIGNATURE_INVALID");

        var header = ReadPngHeader(bytes);
        if (!header.IsSuccess)
            return header.MapError<DesignConversionDecodedPngComparisonEvidence>();
        if (header.Value!.PixelCount > MaxPngScreenshotPixels)
            return OperationResult<DesignConversionDecodedPngComparisonEvidence>.Fail(
                $"Comparison PNG dimensions are too large to inspect safely: {header.Value.Width.ToString(CultureInfo.InvariantCulture)}x{header.Value.Height.ToString(CultureInfo.InvariantCulture)} exceeds {MaxPngScreenshotPixels.ToString(CultureInfo.InvariantCulture)} pixels.",
                "COMPARISON_PNG_PIXELS_TOO_LARGE");

        var decode = DecodePngForComparison(bytes);
        if (!decode.IsSuccess)
            return decode.MapError<DesignConversionDecodedPngComparisonEvidence>();

        var decoded = decode.Value!;
        var metrics = decoded.Metrics;
        var sha = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        return OperationResult<DesignConversionDecodedPngComparisonEvidence>.Ok(
            new DesignConversionDecodedPngComparisonEvidence(
                LocalPath: localPath,
                ByteLength: bytes.LongLength,
                PngWidth: metrics.Width,
                PngHeight: metrics.Height,
                HasNonBlankPixels: metrics.HasNonBlankPixels,
                NonWhitePixelCount: metrics.NonWhitePixelCount,
                DistinctColorCount: metrics.DistinctColorCount,
                Sha256: sha,
                RgbaPixels: decoded.RgbaPixels));
    }

    private static async Task<OperationResult<DesignConversionBrowserPngEvidence>> InspectRemoteAsync(
        Uri initialUri,
        IDesignConversionRemotePngDnsResolver dnsResolver,
        IDesignConversionRemotePngHttpTransport httpTransport,
        DesignConversionRemotePngFetchPolicy policy,
        DesignConversionBrowserQaOperationRateLimiter remoteOperationRateLimiter,
        CancellationToken cancellationToken)
    {
        var policyValidation = ValidateRemoteFetchPolicy(policy);
        if (!policyValidation.IsSuccess)
            return policyValidation.MapError<DesignConversionBrowserPngEvidence>();

        var initialUriValidation = ValidateRemoteUri(initialUri, policy);
        if (!initialUriValidation.IsSuccess)
            return initialUriValidation.MapError<DesignConversionBrowserPngEvidence>();
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<DesignConversionBrowserPngEvidence>.Fail("Remote PNG fetch cancelled.", "CANCELLED");
        if (!remoteOperationRateLimiter.TryAcquire())
            return RemotePngFetchRateLimited();

        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var current = initialUri;
        string? localPath = null;
        var committedToEvidence = false;

        try
        {
            for (var redirect = 0; redirect <= policy.MaxRedirects; redirect++)
            {
                if (!visited.Add(CanonicalRemoteUriKey(current)))
                    return OperationResult<DesignConversionBrowserPngEvidence>.Fail(
                        "Remote PNG fetch rejected a redirect loop.",
                        "PNG_REMOTE_REDIRECT_LOOP");

                var pinnedAddress = await ValidateAndResolveRemoteTargetAsync(
                    current,
                    dnsResolver,
                    policy,
                    cancellationToken);
                if (!pinnedAddress.IsSuccess)
                    return pinnedAddress.MapError<DesignConversionBrowserPngEvidence>();

                var responseResult = await httpTransport.SendAsync(
                    new DesignConversionRemotePngHttpRequest(current, pinnedAddress.Value!, policy.Timeout),
                    cancellationToken);
                if (!responseResult.IsSuccess)
                    return responseResult.MapError<DesignConversionBrowserPngEvidence>();

                using var response = responseResult.Value!;
                if (IsRedirectStatus(response.StatusCode))
                {
                    if (redirect == policy.MaxRedirects)
                        return OperationResult<DesignConversionBrowserPngEvidence>.Fail(
                            "Remote PNG fetch exceeded the redirect limit.",
                            "PNG_REMOTE_REDIRECT_LIMIT");

                    var next = ResolveRedirect(current, response.RedirectLocation);
                    if (!next.IsSuccess)
                        return next.MapError<DesignConversionBrowserPngEvidence>();
                    var redirectValidation = ValidateRemoteRedirectTarget(current, next.Value!, policy);
                    if (!redirectValidation.IsSuccess)
                        return redirectValidation.MapError<DesignConversionBrowserPngEvidence>();
                    if (visited.Contains(CanonicalRemoteUriKey(next.Value!)))
                    {
                        return OperationResult<DesignConversionBrowserPngEvidence>.Fail(
                            "Remote PNG fetch rejected a redirect loop.",
                            "PNG_REMOTE_REDIRECT_LOOP");
                    }

                    var redirectTarget = await ValidateAndResolveRemoteTargetAsync(
                        next.Value!,
                        dnsResolver,
                        policy,
                        cancellationToken);
                    if (!redirectTarget.IsSuccess)
                        return redirectTarget.MapError<DesignConversionBrowserPngEvidence>();

                    current = next.Value!;
                    continue;
                }

                if (response.StatusCode != 200)
                    return OperationResult<DesignConversionBrowserPngEvidence>.Fail(
                        "Remote PNG fetch returned an unsupported HTTP status.",
                        "PNG_REMOTE_STATUS_UNSUPPORTED");

                if (!IsAllowedPngContentType(response.ContentType))
                    return OperationResult<DesignConversionBrowserPngEvidence>.Fail(
                        "Remote PNG fetch returned an unsupported content type.",
                        "PNG_REMOTE_CONTENT_TYPE_UNSUPPORTED");

                if (response.ContentEncodings.Count > 0)
                    return OperationResult<DesignConversionBrowserPngEvidence>.Fail(
                        "Remote PNG fetch returned an unsupported content encoding.",
                        "PNG_REMOTE_CONTENT_ENCODING_UNSUPPORTED");

                if (response.ContentLength.HasValue &&
                    (response.ContentLength.Value > MaxPngScreenshotBytes || response.ContentLength.Value > policy.MaxBytes))
                    return OperationResult<DesignConversionBrowserPngEvidence>.Fail(
                        "Remote PNG content length exceeds the configured byte limit.",
                        "PNG_REMOTE_CONTENT_LENGTH_TOO_LARGE");

                var bytes = await ReadRemoteBytesAsync(response.Body, policy.MaxBytes, cancellationToken);
                if (!bytes.IsSuccess)
                    return bytes.MapError<DesignConversionBrowserPngEvidence>();

                var tempPath = CreateRemoteEvidenceTempPath();
                if (!tempPath.IsSuccess)
                    return tempPath.MapError<DesignConversionBrowserPngEvidence>();

                localPath = tempPath.Value;
                var evidence = InspectPngBytes(bytes.Value!, localPath!);
                if (!evidence.IsSuccess)
                    return evidence;
                if (!evidence.Value!.HasNonBlankPixels)
                    return OperationResult<DesignConversionBrowserPngEvidence>.Fail(
                        "Remote PNG screenshot proof is blank.",
                        "PNG_REMOTE_BLANK");

                await File.WriteAllBytesAsync(localPath!, bytes.Value!, cancellationToken);
                var ownedEvidence = evidence.Value.WithOwnedRemoteSnapshot();
                committedToEvidence = true;
                return OperationResult<DesignConversionBrowserPngEvidence>.Ok(ownedEvidence);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return OperationResult<DesignConversionBrowserPngEvidence>.Fail(
                "Remote PNG fetch cancelled.",
                "CANCELLED");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return OperationResult<DesignConversionBrowserPngEvidence>.Fail(
                "Remote PNG fetch timed out.",
                "PNG_REMOTE_FETCH_TIMEOUT");
        }
        catch (IOException)
        {
            return OperationResult<DesignConversionBrowserPngEvidence>.Fail(
                "Remote PNG fetch failed.",
                "PNG_REMOTE_FETCH_FAILED");
        }
        catch (HttpRequestException)
        {
            return OperationResult<DesignConversionBrowserPngEvidence>.Fail(
                "Remote PNG fetch failed.",
                "PNG_REMOTE_FETCH_FAILED");
        }
        catch (SocketException)
        {
            return OperationResult<DesignConversionBrowserPngEvidence>.Fail(
                "Remote PNG fetch failed.",
                "PNG_REMOTE_FETCH_FAILED");
        }
        finally
        {
            if (!committedToEvidence && localPath is not null)
                _ = CleanupOwnedRemoteEvidenceTempFile(localPath);
        }

        return OperationResult<DesignConversionBrowserPngEvidence>.Fail(
            "Remote PNG fetch exceeded the redirect limit.",
            "PNG_REMOTE_REDIRECT_LIMIT");
    }

    private static OperationResult<DesignConversionBrowserPngEvidence> RemotePngFetchRateLimited()
        => OperationResult<DesignConversionBrowserPngEvidence>.Fail(
            "Remote PNG fetch rate limit exceeded.",
            RemoteFetchRateLimitedCode);

    private static OperationResult<DesignConversionRemotePngFetchPolicy> ValidateRemoteFetchPolicy(
        DesignConversionRemotePngFetchPolicy policy)
    {
        if (!policy.AllowHttp && !policy.AllowHttps)
            return OperationResult<DesignConversionRemotePngFetchPolicy>.Fail(
                "Remote PNG fetch policy does not allow HTTP or HTTPS.",
                "PNG_REMOTE_POLICY_NO_SCHEME");
        if (policy.MaxRedirects < 0)
            return OperationResult<DesignConversionRemotePngFetchPolicy>.Fail(
                "Remote PNG fetch policy redirect limit is invalid.",
                "PNG_REMOTE_POLICY_INVALID_REDIRECT_LIMIT");
        if (policy.MaxBytes <= 0 || policy.MaxBytes > MaxPngScreenshotBytes)
            return OperationResult<DesignConversionRemotePngFetchPolicy>.Fail(
                "Remote PNG fetch policy byte limit is invalid.",
                "PNG_REMOTE_POLICY_INVALID_BYTE_LIMIT");
        if (policy.Timeout <= TimeSpan.Zero)
            return OperationResult<DesignConversionRemotePngFetchPolicy>.Fail(
                "Remote PNG fetch policy timeout is invalid.",
                "PNG_REMOTE_POLICY_INVALID_TIMEOUT");

        return OperationResult<DesignConversionRemotePngFetchPolicy>.Ok(policy);
    }

    private static async Task<OperationResult<IPAddress>> ValidateAndResolveRemoteTargetAsync(
        Uri uri,
        IDesignConversionRemotePngDnsResolver dnsResolver,
        DesignConversionRemotePngFetchPolicy policy,
        CancellationToken cancellationToken)
    {
        var uriValidation = ValidateRemoteUri(uri, policy);
        if (!uriValidation.IsSuccess)
            return uriValidation.MapError<IPAddress>();

        IReadOnlyList<IPAddress> addresses;
        if (IPAddress.TryParse(uri.Host, out var literalAddress))
        {
            addresses = [literalAddress];
        }
        else
        {
            var resolved = await dnsResolver.ResolveAsync(uri.DnsSafeHost, cancellationToken);
            if (!resolved.IsSuccess)
                return OperationResult<IPAddress>.Fail(
                    "Remote PNG host could not be resolved.",
                    "PNG_REMOTE_DNS_RESOLUTION_FAILED");
            if (resolved.Value is null || resolved.Value.Count == 0)
                return OperationResult<IPAddress>.Fail(
                    "Remote PNG host did not resolve to any addresses.",
                    "PNG_REMOTE_DNS_EMPTY");

            addresses = resolved.Value;
        }

        if (addresses.Any(IsBlockedDestinationAddress))
            return OperationResult<IPAddress>.Fail(
                "Remote PNG host resolved to a blocked destination address.",
                "PNG_REMOTE_HOST_BLOCKED");

        return OperationResult<IPAddress>.Ok(addresses[0]);
    }

    private static OperationResult<bool> ValidateRemoteUri(Uri uri, DesignConversionRemotePngFetchPolicy policy)
    {
        if (!IsHttpOrHttpsScheme(uri.Scheme) ||
            (string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) && !policy.AllowHttp) ||
            (string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) && !policy.AllowHttps))
        {
            return OperationResult<bool>.Fail(
                "Remote PNG fetch only accepts configured HTTP and HTTPS schemes.",
                "PNG_REMOTE_URI_UNSUPPORTED_SCHEME");
        }

        if (!string.IsNullOrEmpty(uri.UserInfo))
            return OperationResult<bool>.Fail(
                "Remote PNG fetch does not accept credentials in the URI.",
                "PNG_REMOTE_URI_USERINFO_FORBIDDEN");
        if (string.IsNullOrWhiteSpace(uri.Host))
            return OperationResult<bool>.Fail(
                "Remote PNG fetch requires a host.",
                "PNG_REMOTE_HOST_REQUIRED");

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateRemoteRedirectTarget(
        Uri current,
        Uri next,
        DesignConversionRemotePngFetchPolicy policy)
    {
        var uriValidation = ValidateRemoteUri(next, policy);
        if (!uriValidation.IsSuccess)
            return uriValidation;

        if (string.Equals(current.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(next.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
        {
            return OperationResult<bool>.Fail(
                "Remote PNG fetch rejected an HTTPS to HTTP redirect.",
                "PNG_REMOTE_HTTPS_DOWNGRADE");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<Uri> ResolveRedirect(Uri current, Uri? location)
    {
        if (location is null)
            return OperationResult<Uri>.Fail(
                "Remote PNG redirect did not include a valid Location header.",
                "PNG_REMOTE_REDIRECT_INVALID");

        var next = location.IsAbsoluteUri ? location : new Uri(current, location);
        return OperationResult<Uri>.Ok(next);
    }

    private static string CanonicalRemoteUriKey(Uri uri)
    {
        var host = string.IsNullOrWhiteSpace(uri.IdnHost) ? uri.Host : uri.IdnHost;
        var builder = new UriBuilder(uri)
        {
            Scheme = uri.Scheme.ToLowerInvariant(),
            Host = host.TrimEnd('.').ToLowerInvariant(),
            Port = uri.IsDefaultPort ? -1 : uri.Port
        };

        return builder.Uri.AbsoluteUri;
    }

    private static async Task<OperationResult<byte[]>> ReadRemoteBytesAsync(
        Stream stream,
        long maxBytes,
        CancellationToken cancellationToken)
    {
        using var bytes = new MemoryStream(capacity: (int)Math.Min(maxBytes, 81920));
        var buffer = new byte[81920];
        while (true)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
            if (read == 0)
                return OperationResult<byte[]>.Ok(bytes.ToArray());

            if (bytes.Length + read > maxBytes)
                return OperationResult<byte[]>.Fail(
                    "Remote PNG stream exceeds the configured byte limit.",
                    "PNG_REMOTE_BYTES_TOO_LARGE");

            bytes.Write(buffer, 0, read);
        }
    }

    private static OperationResult<DesignConversionBrowserPngEvidence> InspectPngBytes(
        byte[] bytes,
        string localPath)
    {
        if (bytes.Length < PngSignature.Length || !bytes.AsSpan(0, PngSignature.Length).SequenceEqual(PngSignature))
            return OperationResult<DesignConversionBrowserPngEvidence>.Fail(
                "Browser screenshot is not a PNG file.",
                "PNG_SIGNATURE_INVALID");

        var header = ReadPngHeader(bytes);
        if (!header.IsSuccess)
            return header.MapError<DesignConversionBrowserPngEvidence>();
        if (header.Value!.PixelCount > MaxPngScreenshotPixels)
            return OperationResult<DesignConversionBrowserPngEvidence>.Fail(
                $"PNG screenshot dimensions are too large to inspect safely: {header.Value.Width.ToString(CultureInfo.InvariantCulture)}x{header.Value.Height.ToString(CultureInfo.InvariantCulture)} exceeds {MaxPngScreenshotPixels.ToString(CultureInfo.InvariantCulture)} pixels.",
                "PNG_SCREENSHOT_PIXELS_TOO_LARGE");

        var decode = DecodePngMetrics(bytes);
        if (!decode.IsSuccess)
            return decode.MapError<DesignConversionBrowserPngEvidence>();

        var metrics = decode.Value!;
        return OperationResult<DesignConversionBrowserPngEvidence>.Ok(new DesignConversionBrowserPngEvidence(
            LocalPath: localPath,
            ByteLength: bytes.LongLength,
            PngWidth: metrics.Width,
            PngHeight: metrics.Height,
            HasNonBlankPixels: metrics.HasNonBlankPixels,
            NonWhitePixelCount: metrics.NonWhitePixelCount,
            DistinctColorCount: metrics.DistinctColorCount)
        {
            SnapshotSha256 = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant()
        });
    }

    private static bool IsHttpOrHttpsScheme(string scheme)
        => string.Equals(scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
           string.Equals(scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);

    private static bool IsRedirectStatus(int statusCode)
        => statusCode is 301 or 302 or 303 or 307 or 308;

    private static bool IsAllowedPngContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return false;

        var mediaType = contentType.Split(';', 2)[0].Trim();
        return string.Equals(mediaType, "image/png", StringComparison.OrdinalIgnoreCase);
    }

    internal static OperationResult<int> CountRemoteEvidenceTempFilesForTesting()
    {
        try
        {
            var root = CanonicalizeRemoteEvidenceTempRoot(requireExistingSafeDirectory: false);
            if (!root.IsSuccess)
                return root.MapError<int>();
            if (!Directory.Exists(root.Value!))
                return OperationResult<int>.Ok(0);

            var rootInfo = new DirectoryInfo(root.Value!);
            rootInfo.Refresh();
            if ((rootInfo.Attributes & FileAttributes.ReparsePoint) != 0)
                return RemoteEvidenceTempCleanupFailure().MapError<int>();

            var count = Directory
                .EnumerateFiles(root.Value!, "*.png", SearchOption.TopDirectoryOnly)
                .Count(path => ValidateRemoteEvidenceTempPath(path, requireRootExists: true).IsSuccess);
            return OperationResult<int>.Ok(count);
        }
        catch (Exception exception) when (IsExpectedRemoteEvidenceTempFileSystemException(exception))
        {
            return RemoteEvidenceTempCleanupFailure().MapError<int>();
        }
    }

    internal static OperationResult<bool> CleanupOwnedRemoteEvidenceTempFile(string localPath)
    {
        var path = ValidateRemoteEvidenceTempPath(localPath, requireRootExists: true);
        if (!path.IsSuccess)
            return path.MapError<bool>();

        try
        {
            var fileInfo = new FileInfo(path.Value!);
            fileInfo.Refresh();
            if (!fileInfo.Exists)
                return OperationResult<bool>.Ok(true);
            if ((fileInfo.Attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0)
                return RemoteEvidenceTempCleanupFailure();

            File.Delete(fileInfo.FullName);
            return File.Exists(fileInfo.FullName)
                ? RemoteEvidenceTempCleanupFailure()
                : OperationResult<bool>.Ok(true);
        }
        catch (Exception exception) when (IsExpectedRemoteEvidenceTempFileSystemException(exception))
        {
            return RemoteEvidenceTempCleanupFailure();
        }
    }

    private static OperationResult<string> CreateRemoteEvidenceTempPath()
    {
        try
        {
            var root = CanonicalizeRemoteEvidenceTempRoot(requireExistingSafeDirectory: false);
            if (!root.IsSuccess)
                return root;

            Directory.CreateDirectory(root.Value!);
            var rootInfo = new DirectoryInfo(root.Value!);
            rootInfo.Refresh();
            if ((rootInfo.Attributes & FileAttributes.ReparsePoint) != 0)
                return RemoteEvidenceTempCleanupFailure().MapError<string>();

            var path = Path.Combine(root.Value!, $"{Guid.NewGuid():N}.png");
            return ValidateRemoteEvidenceTempPath(path, requireRootExists: true);
        }
        catch (Exception exception) when (IsExpectedRemoteEvidenceTempFileSystemException(exception))
        {
            return RemoteEvidenceTempCleanupFailure().MapError<string>();
        }
    }

    private static OperationResult<string> ValidateRemoteEvidenceTempPath(
        string localPath,
        bool requireRootExists)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(localPath) ||
                localPath.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase) ||
                localPath.StartsWith(@"\\", StringComparison.Ordinal))
            {
                return RemoteEvidenceTempCleanupFailure().MapError<string>();
            }

            var root = CanonicalizeRemoteEvidenceTempRoot(requireRootExists);
            if (!root.IsSuccess)
                return root;

            var fullPath = Path.GetFullPath(localPath);
            var parent = Path.GetDirectoryName(fullPath);
            var fileName = Path.GetFileName(fullPath);
            if (string.IsNullOrWhiteSpace(parent) ||
                !string.Equals(
                    Path.TrimEndingDirectorySeparator(Path.GetFullPath(parent)),
                    root.Value,
                    OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) ||
                !string.Equals(Path.GetExtension(fileName), ".png", StringComparison.OrdinalIgnoreCase) ||
                !Guid.TryParseExact(Path.GetFileNameWithoutExtension(fileName), "N", out _))
            {
                return RemoteEvidenceTempCleanupFailure().MapError<string>();
            }

            return OperationResult<string>.Ok(fullPath);
        }
        catch (Exception exception) when (IsExpectedRemoteEvidenceTempFileSystemException(exception))
        {
            return RemoteEvidenceTempCleanupFailure().MapError<string>();
        }
    }

    private static OperationResult<string> CanonicalizeRemoteEvidenceTempRoot(bool requireExistingSafeDirectory)
    {
        try
        {
            var root = Path.GetFullPath(Path.Combine(
                Path.GetTempPath(),
                "llm_mvp_core",
                "design-conversion-remote-png-evidence"));

            if (requireExistingSafeDirectory)
            {
                var rootInfo = new DirectoryInfo(root);
                rootInfo.Refresh();
                if (!rootInfo.Exists ||
                    (rootInfo.Attributes & FileAttributes.ReparsePoint) != 0)
                {
                    return RemoteEvidenceTempCleanupFailure().MapError<string>();
                }
            }

            return OperationResult<string>.Ok(Path.TrimEndingDirectorySeparator(root));
        }
        catch (Exception exception) when (IsExpectedRemoteEvidenceTempFileSystemException(exception))
        {
            return RemoteEvidenceTempCleanupFailure().MapError<string>();
        }
    }

    private static OperationResult<bool> RemoteEvidenceTempCleanupFailure()
        => OperationResult<bool>.Fail(
            "Remote PNG evidence temp cleanup failed.",
            "PNG_REMOTE_TEMP_CLEANUP_FAILED");

    private static bool IsExpectedRemoteEvidenceTempFileSystemException(Exception exception)
        => exception is IOException
            or UnauthorizedAccessException
            or ArgumentException
            or NotSupportedException
            or System.Security.SecurityException;

    internal static bool IsBlockedDestinationAddress(IPAddress address)
    {
        if (IPAddress.IsLoopback(address) ||
            address.Equals(IPAddress.Any) ||
            address.Equals(IPAddress.IPv6Any) ||
            address.IsIPv4MappedToIPv6)
        {
            return true;
        }

        return address.AddressFamily switch
        {
            AddressFamily.InterNetwork => IsBlockedIpv4(address.GetAddressBytes()),
            AddressFamily.InterNetworkV6 => IsBlockedIpv6(address.GetAddressBytes()),
            _ => true
        };
    }

    private static bool IsBlockedIpv4(byte[] bytes)
    {
        var first = bytes[0];
        var second = bytes[1];
        var third = bytes[2];

        return first == 0 ||
               first == 10 ||
               first == 127 ||
               (first == 100 && second is >= 64 and <= 127) ||
               (first == 169 && second == 254) ||
               (first == 172 && second is >= 16 and <= 31) ||
               (first == 192 && second == 168) ||
               (first == 192 && second == 0) ||
               (first == 192 && second == 88 && third == 99) ||
               (first == 198 && second is 18 or 19) ||
               (first == 192 && second == 0 && third == 2) ||
               (first == 198 && second == 51 && third == 100) ||
               (first == 203 && second == 0 && third == 113) ||
               first >= 224;
    }

    private static bool IsBlockedIpv6(byte[] bytes)
    {
        if ((bytes[0] & 0xfe) == 0xfc ||
            (bytes[0] == 0xfe && (bytes[1] & 0xc0) == 0x80) ||
            bytes[0] == 0xff ||
            HasIpv6Prefix(bytes, [0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00], prefixLength: 64) ||
            HasIpv6Prefix(bytes, [0x20, 0x01, 0x00, 0x00], prefixLength: 32) ||
            HasIpv6Prefix(bytes, [0x20, 0x01, 0x00, 0x02, 0x00, 0x00], prefixLength: 48) ||
            HasIpv6Prefix(bytes, [0x20, 0x01, 0x0d, 0xb8], prefixLength: 32) ||
            HasIpv6Prefix(bytes, [0x20, 0x02], prefixLength: 16) ||
            HasIpv6Prefix(bytes, [0x00, 0x64, 0xff, 0x9b, 0x00, 0x01], prefixLength: 48))
        {
            return true;
        }

        if (HasIpv6Prefix(bytes, [0x00, 0x64, 0xff, 0x9b, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00], prefixLength: 96))
            return IsBlockedIpv4(bytes[^4..]);

        return false;
    }

    private static bool HasIpv6Prefix(byte[] bytes, ReadOnlySpan<byte> prefix, int prefixLength)
    {
        var fullBytes = prefixLength / 8;
        if (!bytes.AsSpan(0, fullBytes).SequenceEqual(prefix[..fullBytes]))
            return false;

        var remainingBits = prefixLength % 8;
        if (remainingBits == 0)
            return true;

        var mask = (byte)(0xff << (8 - remainingBits));
        return (bytes[fullBytes] & mask) == (prefix[fullBytes] & mask);
    }

    private static OperationResult<string> ResolveLocalPath(string screenshotUri)
    {
        if (IsNetworkPath(screenshotUri))
            return OperationResult<string>.Fail(
                "PNG screenshot proof must not reference a network file path.",
                "PNG_SCREENSHOT_URI_NETWORK_PATH");

        if (Path.IsPathFullyQualified(screenshotUri))
            return OperationResult<string>.Ok(screenshotUri);

        if (Uri.TryCreate(screenshotUri, UriKind.Absolute, out var uri) && uri.IsFile)
        {
            if (uri.IsUnc || !string.IsNullOrWhiteSpace(uri.Host))
                return OperationResult<string>.Fail(
                    "PNG screenshot proof must not reference a network file path.",
                    "PNG_SCREENSHOT_URI_NETWORK_PATH");

            return OperationResult<string>.Ok(uri.LocalPath);
        }

        return OperationResult<string>.Fail(
            "PNG screenshot proof must reference a local file path; remote screenshot URIs are not accepted.",
            "PNG_SCREENSHOT_URI_NOT_LOCAL_FILE");
    }

    private static bool IsNetworkPath(string path)
    {
        if (path.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase))
            return true;
        if (path.StartsWith(@"\\?\", StringComparison.OrdinalIgnoreCase))
            return false;
        return path.StartsWith(@"\\", StringComparison.Ordinal) ||
               path.StartsWith("//", StringComparison.Ordinal);
    }

    private static OperationResult<PngHeader> ReadPngHeader(string path)
    {
        var header = new byte[PngIhdrHeaderLength];
        using (var stream = File.OpenRead(path))
        {
            var read = stream.Read(header, 0, header.Length);
            if (read < PngSignature.Length || !header.AsSpan(0, PngSignature.Length).SequenceEqual(PngSignature))
                return OperationResult<PngHeader>.Fail(
                    "Browser screenshot is not a PNG file.",
                    "PNG_SIGNATURE_INVALID");
            if (read < PngIhdrHeaderLength)
                return OperationResult<PngHeader>.Fail(
                    "PNG dimensions are missing.",
                    "PNG_DIMENSIONS_MISSING");
        }

        return ReadPngHeader(header);
    }

    private static OperationResult<PngHeader> ReadPngHeader(byte[] bytes)
    {
        if (bytes.Length < PngSignature.Length || !bytes.AsSpan(0, PngSignature.Length).SequenceEqual(PngSignature))
            return OperationResult<PngHeader>.Fail(
                "Browser screenshot is not a PNG file.",
                "PNG_SIGNATURE_INVALID");
        if (bytes.Length < PngIhdrHeaderLength)
            return OperationResult<PngHeader>.Fail(
                "PNG dimensions are missing.",
                "PNG_DIMENSIONS_MISSING");

        return ReadPngHeader(bytes.AsSpan(0, PngIhdrHeaderLength));
    }

    private static OperationResult<PngHeader> ReadPngHeader(ReadOnlySpan<byte> header)
    {
        var ihdrLength = BinaryPrimitives.ReadInt32BigEndian(header.Slice(PngSignature.Length, 4));
        var ihdrType = Encoding.ASCII.GetString(header.Slice(PngSignature.Length + 4, 4));
        if (ihdrLength != 13 || !string.Equals(ihdrType, "IHDR", StringComparison.Ordinal))
            return OperationResult<PngHeader>.Fail(
                "PNG dimensions are missing.",
                "PNG_DIMENSIONS_MISSING");

        var width = BinaryPrimitives.ReadInt32BigEndian(header.Slice(PngSignature.Length + 8, 4));
        var height = BinaryPrimitives.ReadInt32BigEndian(header.Slice(PngSignature.Length + 12, 4));
        if (width <= 0 || height <= 0)
            return OperationResult<PngHeader>.Fail(
                "PNG dimensions are missing.",
                "PNG_DIMENSIONS_MISSING");

        return OperationResult<PngHeader>.Ok(new PngHeader(width, height, (long)width * height));
    }

    private static OperationResult<DecodedPngMetrics> DecodePngMetrics(byte[] bytes)
    {
        var scanlines = DecodePngScanlines(bytes);
        if (!scanlines.IsSuccess)
            return scanlines.MapError<DecodedPngMetrics>();

        using var decoded = scanlines.Value!;
        return OperationResult<DecodedPngMetrics>.Ok(MeasurePixels(
            decoded.Pixels,
            decoded.Width,
            decoded.Height,
            decoded.BytesPerPixel,
            decoded.ColorType));
    }

    private static OperationResult<DecodedPngRaster> DecodePngForComparison(byte[] bytes)
    {
        var scanlines = DecodePngScanlines(bytes);
        if (!scanlines.IsSuccess)
            return scanlines.MapError<DecodedPngRaster>();

        using var decoded = scanlines.Value!;
        return OperationResult<DecodedPngRaster>.Ok(MaterializeComparisonRaster(
            decoded.Pixels,
            decoded.Width,
            decoded.Height,
            decoded.BytesPerPixel,
            decoded.ColorType));
    }

    private static OperationResult<DecodedPngScanlines> DecodePngScanlines(byte[] bytes)
    {
        var offset = PngSignature.Length;
        var width = 0;
        var height = 0;
        var bitDepth = 0;
        var colorType = 0;
        var interlace = 0;
        var idatSegments = new List<PngIdatSegment>();
        var idatLength = 0L;
        var seenIhdr = false;
        var seenIend = false;

        while ((long)offset + 12L <= bytes.Length)
        {
            var length = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(offset, 4));
            var type = Encoding.ASCII.GetString(bytes, offset + 4, 4);
            var dataStart = offset + 8;
            var chunkEnd = (long)dataStart + length + 4L;
            if (length < 0 || chunkEnd > bytes.Length)
                return OperationResult<DecodedPngScanlines>.Fail("PNG chunk length is invalid.", "PNG_CHUNK_INVALID");

            var storedCrc = BinaryPrimitives.ReadUInt32BigEndian(
                bytes.AsSpan(dataStart + length, 4));
            var computedCrc = ComputePngChunkCrc(
                bytes.AsSpan(offset + 4, 4),
                bytes.AsSpan(dataStart, length));
            if (storedCrc != computedCrc)
                return InvalidPngChunkCrc();

            if (type == "IHDR")
            {
                if (seenIhdr || offset != PngSignature.Length || length != 13)
                    return InvalidPngChunkSequence();

                seenIhdr = true;
                width = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(dataStart, 4));
                height = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(dataStart + 4, 4));
                bitDepth = bytes[dataStart + 8];
                colorType = bytes[dataStart + 9];
                interlace = bytes[dataStart + 12];
            }
            else if (type == "IDAT")
            {
                if (!seenIhdr)
                    return InvalidPngChunkSequence();

                if (length > 0)
                {
                    idatSegments.Add(new PngIdatSegment(dataStart, length));
                    idatLength = checked(idatLength + length);
                }
            }
            else if (type == "IEND")
            {
                if (!seenIhdr || seenIend || length != 0 || chunkEnd != bytes.Length)
                    return InvalidPngChunkSequence();

                seenIend = true;
                offset = (int)chunkEnd;
                break;
            }
            else if (!seenIhdr)
            {
                return InvalidPngChunkSequence();
            }

            offset = (int)chunkEnd;
        }

        if (!seenIend || offset != bytes.Length)
            return InvalidPngChunkSequence();

        if (width <= 0 || height <= 0)
            return OperationResult<DecodedPngScanlines>.Fail("PNG dimensions are missing.", "PNG_DIMENSIONS_MISSING");
        if (bitDepth != 8)
            return OperationResult<DecodedPngScanlines>.Fail("Only 8-bit PNG evidence is supported.", "PNG_BIT_DEPTH_UNSUPPORTED");
        if (interlace != 0)
            return OperationResult<DecodedPngScanlines>.Fail("Interlaced PNG evidence is not supported.", "PNG_INTERLACE_UNSUPPORTED");

        var bytesPerPixel = colorType switch
        {
            0 => 1,
            2 => 3,
            4 => 2,
            6 => 4,
            _ => 0
        };
        if (bytesPerPixel == 0)
            return OperationResult<DecodedPngScanlines>.Fail($"PNG color type {colorType} is not supported.", "PNG_COLOR_TYPE_UNSUPPORTED");
        if (idatLength <= 0)
            return OperationResult<DecodedPngScanlines>.Fail("PNG compressed pixel data is missing.", "PNG_IDAT_MISSING");

        PooledPngScanlineBuffer? pixelData = null;
        int stride;
        try
        {
            stride = checked(width * bytesPerPixel);
            var expectedLength = checked((stride + 1) * height);
            using var idat = new SegmentedReadOnlyByteStream(bytes, idatSegments);
            using var zlib = new ZLibStream(idat, CompressionMode.Decompress);
            var decompressedResult = ReadDecompressedPixelData(zlib, expectedLength);
            if (!decompressedResult.IsSuccess)
                return decompressedResult.MapError<DecodedPngScanlines>();
            pixelData = decompressedResult.Value!;

            var reconstructed = ReconstructScanlinesInPlace(pixelData.ActiveSpan, height, bytesPerPixel, stride);
            if (!reconstructed.IsSuccess)
                return reconstructed.MapError<DecodedPngScanlines>();

            var compactedLength = checked(stride * height);
            var decoded = new DecodedPngScanlines(
                Width: width,
                Height: height,
                BytesPerPixel: bytesPerPixel,
                ColorType: colorType,
                PixelData: pixelData,
                CompactedLength: compactedLength);
            pixelData = null;
            return OperationResult<DecodedPngScanlines>.Ok(decoded);
        }
        catch (InvalidDataException)
        {
            return OperationResult<DecodedPngScanlines>.Fail(
                "PNG compressed pixel data is invalid.",
                "PNG_COMPRESSED_DATA_INVALID");
        }
        catch (IOException)
        {
            return OperationResult<DecodedPngScanlines>.Fail(
                "PNG compressed pixel data is invalid.",
                "PNG_COMPRESSED_DATA_INVALID");
        }
        catch (OverflowException)
        {
            return OperationResult<DecodedPngScanlines>.Fail(
                "PNG pixel data is too large to inspect safely.",
                "PNG_PIXEL_DATA_TOO_LARGE");
        }
        finally
        {
            pixelData?.Dispose();
        }
    }

    private static OperationResult<DecodedPngScanlines> InvalidPngChunkCrc()
        => OperationResult<DecodedPngScanlines>.Fail(
            "PNG chunk CRC is invalid.",
            "PNG_CHUNK_CRC_INVALID");

    private static OperationResult<DecodedPngScanlines> InvalidPngChunkSequence()
        => OperationResult<DecodedPngScanlines>.Fail(
            "PNG chunk sequence is invalid.",
            "PNG_CHUNK_SEQUENCE_INVALID");

    private static uint ComputePngChunkCrc(
        ReadOnlySpan<byte> typeBytes,
        ReadOnlySpan<byte> data)
    {
        var crc = uint.MaxValue;
        crc = UpdatePngCrc32(crc, typeBytes);
        crc = UpdatePngCrc32(crc, data);
        return ~crc;
    }

    private static uint UpdatePngCrc32(uint crc, ReadOnlySpan<byte> bytes)
    {
        foreach (var value in bytes)
            crc = PngCrc32Table[(int)((crc ^ value) & 0xff)] ^ (crc >> 8);

        return crc;
    }

    private static uint[] BuildPngCrc32Table()
    {
        var table = new uint[256];
        for (var index = 0; index < table.Length; index++)
        {
            var value = (uint)index;
            for (var bit = 0; bit < 8; bit++)
                value = (value & 1) == 1 ? 0xedb88320u ^ (value >> 1) : value >> 1;

            table[index] = value;
        }

        return table;
    }

    private static OperationResult<PooledPngScanlineBuffer> ReadDecompressedPixelData(Stream zlib, int expectedLength)
    {
        PooledPngScanlineBuffer? pixelData = PooledPngScanlineBuffer.Rent(expectedLength);
        var totalRead = 0;
        try
        {
            var active = pixelData.ActiveSpan;
            while (totalRead < active.Length)
            {
                var read = zlib.Read(active.Slice(totalRead));
                if (read == 0)
                    return OperationResult<PooledPngScanlineBuffer>.Fail(
                        "PNG pixel data is shorter than expected.",
                        "PNG_PIXEL_DATA_INCOMPLETE");

                totalRead += read;
            }

            if (zlib.ReadByte() >= 0)
                return OperationResult<PooledPngScanlineBuffer>.Fail(
                    "PNG pixel data is larger than expected for its dimensions.",
                    "PNG_PIXEL_DATA_TOO_LARGE");

            var rented = pixelData;
            pixelData = null;
            return OperationResult<PooledPngScanlineBuffer>.Ok(rented);
        }
        finally
        {
            pixelData?.Dispose();
        }
    }

    private static OperationResult<bool> ReconstructScanlinesInPlace(Span<byte> pixelData, int height, int bytesPerPixel, int stride)
    {
        for (var y = 0; y < height; y++)
        {
            var sourceRow = checked(y * (stride + 1));
            var row = checked(y * stride);
            var filter = pixelData[sourceRow];
            if (filter > 4)
                return OperationResult<bool>.Fail("PNG scanline filter is invalid.", "PNG_FILTER_INVALID");

            for (var x = 0; x < stride; x++)
            {
                var raw = pixelData[sourceRow + 1 + x];
                var left = x >= bytesPerPixel ? pixelData[row + x - bytesPerPixel] : (byte)0;
                var up = y > 0 ? pixelData[row - stride + x] : (byte)0;
                var upLeft = y > 0 && x >= bytesPerPixel ? pixelData[row - stride + x - bytesPerPixel] : (byte)0;
                var predict = filter switch
                {
                    0 => 0,
                    1 => left,
                    2 => up,
                    3 => (left + up) / 2,
                    4 => Paeth(left, up, upLeft),
                    _ => 0
                };

                pixelData[row + x] = unchecked((byte)(raw + predict));
            }
        }

        return OperationResult<bool>.Ok(true);
    }

    private sealed class SegmentedReadOnlyByteStream : Stream
    {
        private readonly byte[] _source;
        private readonly IReadOnlyList<PngIdatSegment> _segments;
        private int _segmentIndex;
        private int _segmentOffset;

        internal SegmentedReadOnlyByteStream(byte[] source, IReadOnlyList<PngIdatSegment> segments)
        {
            _source = source;
            _segments = segments;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            ArgumentOutOfRangeException.ThrowIfNegative(offset);
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (buffer.Length - offset < count)
                throw new ArgumentException("The offset and count exceed the buffer bounds.", nameof(count));

            return Read(buffer.AsSpan(offset, count));
        }

        public override int Read(Span<byte> buffer)
        {
            var totalRead = 0;
            while (buffer.Length > 0 && _segmentIndex < _segments.Count)
            {
                var segment = _segments[_segmentIndex];
                var available = segment.Length - _segmentOffset;
                if (available <= 0)
                {
                    _segmentIndex++;
                    _segmentOffset = 0;
                    continue;
                }

                var copy = Math.Min(buffer.Length, available);
                _source.AsSpan(segment.Offset + _segmentOffset, copy).CopyTo(buffer);
                _segmentOffset += copy;
                totalRead += copy;
                buffer = buffer.Slice(copy);
                if (_segmentOffset == segment.Length)
                {
                    _segmentIndex++;
                    _segmentOffset = 0;
                }
            }

            return totalRead;
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();
    }

    private static DecodedPngMetrics MeasurePixels(ReadOnlySpan<byte> pixels, int width, int height, int bytesPerPixel, int colorType)
    {
        var nonWhite = 0;
        var colors = new HashSet<int>();
        var totalPixels = width * height;
        for (var pixel = 0; pixel < totalPixels; pixel++)
        {
            var offset = pixel * bytesPerPixel;
            var (r, g, b, a) = Rgba(pixels, offset, colorType);
            AccumulatePixelMetrics(r, g, b, a, colors, ref nonWhite);
        }

        return new DecodedPngMetrics(
            Width: width,
            Height: height,
            HasNonBlankPixels: HasMeaningfulNonBlankPixels(nonWhite, totalPixels),
            NonWhitePixelCount: nonWhite,
            DistinctColorCount: colors.Count);
    }

    private static DecodedPngRaster MaterializeComparisonRaster(
        ReadOnlySpan<byte> pixels,
        int width,
        int height,
        int bytesPerPixel,
        int colorType)
    {
        var nonWhite = 0;
        var colors = new HashSet<int>();
        var totalPixels = width * height;
        var rgbaPixels = new byte[checked(totalPixels * 4)];
        for (var pixel = 0; pixel < totalPixels; pixel++)
        {
            var offset = pixel * bytesPerPixel;
            var (r, g, b, a) = Rgba(pixels, offset, colorType);
            var rgbaOffset = pixel * 4;
            rgbaPixels[rgbaOffset] = (byte)r;
            rgbaPixels[rgbaOffset + 1] = (byte)g;
            rgbaPixels[rgbaOffset + 2] = (byte)b;
            rgbaPixels[rgbaOffset + 3] = (byte)a;
            AccumulatePixelMetrics(r, g, b, a, colors, ref nonWhite);
        }

        return new DecodedPngRaster(
            Metrics: new DecodedPngMetrics(
                Width: width,
                Height: height,
                HasNonBlankPixels: HasMeaningfulNonBlankPixels(nonWhite, totalPixels),
                NonWhitePixelCount: nonWhite,
                DistinctColorCount: colors.Count),
            RgbaPixels: rgbaPixels);
    }

    private static void AccumulatePixelMetrics(int r, int g, int b, int a, HashSet<int> colors, ref int nonWhite)
    {
        if (a > 0 && (r != 255 || g != 255 || b != 255))
            nonWhite++;

        if (colors.Count < 1024)
            colors.Add((a << 24) | (r << 16) | (g << 8) | b);
    }

    internal static int MinimumMeaningfulNonBlankPixels(int totalPixels)
        => totalPixels <= 0
            ? 0
            : Math.Min(
                totalPixels,
                Math.Max(
                    MinimumSparseNonWhitePixels,
                    (totalPixels + SparseNonWhitePixelRatioDivisor - 1) / SparseNonWhitePixelRatioDivisor));

    private static bool HasMeaningfulNonBlankPixels(int nonWhitePixels, int totalPixels)
    {
        if (nonWhitePixels <= 0 || totalPixels <= 0)
            return false;
        if (nonWhitePixels == totalPixels)
            return true;

        return nonWhitePixels >= MinimumMeaningfulNonBlankPixels(totalPixels);
    }

    private static (int R, int G, int B, int A) Rgba(ReadOnlySpan<byte> pixels, int offset, int colorType)
        => colorType switch
        {
            0 => (pixels[offset], pixels[offset], pixels[offset], 255),
            2 => (pixels[offset], pixels[offset + 1], pixels[offset + 2], 255),
            4 => (pixels[offset], pixels[offset], pixels[offset], pixels[offset + 1]),
            6 => (pixels[offset], pixels[offset + 1], pixels[offset + 2], pixels[offset + 3]),
            _ => (255, 255, 255, 0)
        };

    private static int Paeth(int left, int up, int upLeft)
    {
        var estimate = left + up - upLeft;
        var leftDistance = Math.Abs(estimate - left);
        var upDistance = Math.Abs(estimate - up);
        var upLeftDistance = Math.Abs(estimate - upLeft);
        if (leftDistance <= upDistance && leftDistance <= upLeftDistance)
            return left;
        return upDistance <= upLeftDistance ? up : upLeft;
    }

    private sealed class SystemDesignConversionRemotePngDnsResolver : IDesignConversionRemotePngDnsResolver
    {
        public async Task<OperationResult<IReadOnlyList<IPAddress>>> ResolveAsync(
            string host,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var addresses = await Dns.GetHostAddressesAsync(host, cancellationToken);
                return OperationResult<IReadOnlyList<IPAddress>>.Ok(addresses);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return OperationResult<IReadOnlyList<IPAddress>>.Fail(
                    "Remote PNG DNS resolution timed out.",
                    "PNG_REMOTE_DNS_TIMEOUT");
            }
            catch (SocketException)
            {
                return OperationResult<IReadOnlyList<IPAddress>>.Fail(
                    "Remote PNG DNS resolution failed.",
                    "PNG_REMOTE_DNS_RESOLUTION_FAILED");
            }
        }
    }

    private sealed class PinnedAddressDesignConversionRemotePngHttpTransport : IDesignConversionRemotePngHttpTransport
    {
        public async Task<OperationResult<DesignConversionRemotePngHttpResponse>> SendAsync(
            DesignConversionRemotePngHttpRequest request,
            CancellationToken cancellationToken = default)
        {
            var handler = new SocketsHttpHandler
            {
                AllowAutoRedirect = false,
                ConnectTimeout = request.Timeout,
                PooledConnectionLifetime = TimeSpan.Zero,
                PooledConnectionIdleTimeout = TimeSpan.Zero,
                AutomaticDecompression = DecompressionMethods.None,
                UseProxy = false,
                Proxy = null
            };
            handler.ConnectCallback = async (context, token) =>
            {
                var socket = new Socket(request.PinnedAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
                {
                    NoDelay = true
                };

                try
                {
                    await socket.ConnectAsync(
                        new IPEndPoint(request.PinnedAddress, context.DnsEndPoint.Port),
                        token);
                    return new NetworkStream(socket, ownsSocket: true);
                }
                catch
                {
                    socket.Dispose();
                    throw;
                }
            };

            var client = new HttpClient(handler, disposeHandler: true)
            {
                Timeout = Timeout.InfiniteTimeSpan
            };

            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(request.Timeout);

                using var message = new HttpRequestMessage(HttpMethod.Get, request.Uri);
                message.Headers.Accept.ParseAdd("image/png");
                var response = await client.SendAsync(
                    message,
                    HttpCompletionOption.ResponseHeadersRead,
                    timeout.Token);
                var stream = await response.Content.ReadAsStreamAsync(timeout.Token);
                var owner = new HttpResponseOwner(response, client);
                return OperationResult<DesignConversionRemotePngHttpResponse>.Ok(
                    new DesignConversionRemotePngHttpResponse(
                        statusCode: (int)response.StatusCode,
                        contentType: response.Content.Headers.ContentType?.ToString(),
                        contentLength: response.Content.Headers.ContentLength,
                        redirectLocation: response.Headers.Location,
                        body: stream,
                        contentEncodings: response.Content.Headers.ContentEncoding.ToArray(),
                        owner: owner));
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                client.Dispose();
                return OperationResult<DesignConversionRemotePngHttpResponse>.Fail(
                    "Remote PNG fetch timed out.",
                    "PNG_REMOTE_FETCH_TIMEOUT");
            }
            catch (Exception) when (!cancellationToken.IsCancellationRequested)
            {
                client.Dispose();
                return OperationResult<DesignConversionRemotePngHttpResponse>.Fail(
                    "Remote PNG fetch failed.",
                    "PNG_REMOTE_FETCH_FAILED");
            }
        }
    }

    private sealed class HttpResponseOwner(HttpResponseMessage response, HttpClient client) : IDisposable
    {
        public void Dispose()
        {
            response.Dispose();
            client.Dispose();
        }
    }

    private sealed record DecodedPngMetrics(
        int Width,
        int Height,
        bool HasNonBlankPixels,
        int NonWhitePixelCount,
        int DistinctColorCount);

    private sealed record DecodedPngRaster(
        DecodedPngMetrics Metrics,
        byte[] RgbaPixels);

    private sealed record DecodedPngScanlines(
        int Width,
        int Height,
        int BytesPerPixel,
        int ColorType,
        PooledPngScanlineBuffer PixelData,
        int CompactedLength) : IDisposable
    {
        public ReadOnlySpan<byte> Pixels => PixelData.ActiveSpan.Slice(0, CompactedLength);

        public void Dispose()
            => PixelData.Dispose();
    }

    private sealed class PooledPngScanlineBuffer : IDisposable
    {
        private byte[]? _buffer;

        private PooledPngScanlineBuffer(byte[] buffer, int activeLength)
        {
            _buffer = buffer;
            ActiveLength = activeLength;
        }

        internal int ActiveLength { get; }

        internal Span<byte> ActiveSpan
        {
            get
            {
                var buffer = _buffer ?? throw new ObjectDisposedException(nameof(PooledPngScanlineBuffer));
                return buffer.AsSpan(0, ActiveLength);
            }
        }

        internal static PooledPngScanlineBuffer Rent(int activeLength)
            => new(ArrayPool<byte>.Shared.Rent(activeLength), activeLength);

        public void Dispose()
        {
            var buffer = _buffer;
            if (buffer is null)
                return;

            _buffer = null;
            ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
        }
    }

    private sealed record PngHeader(int Width, int Height, long PixelCount);

    private sealed record PngIdatSegment(int Offset, int Length);
}

internal sealed record LocalChromiumBrowserProcessRunResult(
    long ElapsedMs,
    string Stdout,
    string Stderr);

internal sealed record LocalChromiumBrowserProcessBatchCaptureRequest(
    string HtmlPath,
    string ScreenshotPath,
    RenderQaResolution Resolution,
    int Ordinal);

internal sealed record LocalChromiumBrowserProcessBatchCaptureResult(
    long ElapsedMs,
    string Stdout,
    string Stderr,
    int ObservedBrowserProcessId,
    string BrowserSessionId,
    int ProcessStartCount,
    int BatchCaptureCount,
    int BatchOrdinal);

internal interface ILocalChromiumBrowserProcessRunner
{
    Task<OperationResult<LocalChromiumBrowserProcessRunResult>> CaptureAsync(
        string executablePath,
        string htmlPath,
        string screenshotPath,
        string userDataPath,
        RenderQaResolution resolution,
        TimeSpan timeout,
        LocalChromiumBrowserRssPolicy rssPolicy,
        CancellationToken cancellationToken);
}

internal interface ILocalChromiumBrowserBatchProcessRunner : ILocalChromiumBrowserProcessRunner
{
    Task<IReadOnlyList<OperationResult<LocalChromiumBrowserProcessBatchCaptureResult>>> CaptureBatchAsync(
        string executablePath,
        string userDataPath,
        IReadOnlyList<LocalChromiumBrowserProcessBatchCaptureRequest> requests,
        TimeSpan timeout,
        LocalChromiumBrowserRssPolicy rssPolicy,
        CancellationToken cancellationToken);
}

public sealed class LocalChromiumBrowserProcessAdapter :
    IDesignConversionBrowserProcessAdapter,
    IDesignConversionBatchBrowserProcessAdapter
{
    internal const string SensitiveCleanupFailedCode = "BROWSER_CAPTURE_SENSITIVE_CLEANUP_FAILED";
    internal const string RuntimeUnavailableCode = "BROWSER_RUNTIME_UNAVAILABLE";
    internal const string RuntimeCrashedBeforeScreenshotCode = "BROWSER_RUNTIME_CRASHED_BEFORE_SCREENSHOT";
    internal const string ScreenshotFailedCode = "BROWSER_SCREENSHOT_FAILED";
    internal const string DevToolsEndpointOwnershipUnverifiedCode = "BROWSER_DEVTOOLS_ENDPOINT_OWNERSHIP_UNVERIFIED";
    internal const string BatchProcessRunnerRequiredCode = "BROWSER_BATCH_PROCESS_RUNNER_REQUIRED";
    internal const string BatchResultEnvelopeNullCode = "BROWSER_CAPTURE_BATCH_RESULT_ENVELOPE_NULL";
    internal const string CaptureRateLimitedCode = "BROWSER_CAPTURE_RATE_LIMITED";
    internal const long DefaultMaxRetainedCaptureBytes = 1L * 1024 * 1024 * 1024;
    internal const int DefaultMaxConcurrentBrowserCaptureProcesses = 2;
    internal const int DefaultBrowserCaptureOperationLimitPerWindow = 30;
    internal static readonly TimeSpan DefaultBrowserCaptureOperationRateLimitWindow = TimeSpan.FromSeconds(60);
    internal const string StrictOfflineDocumentCspContent =
        "default-src 'none'; base-uri 'none'; form-action 'none'; frame-src 'none'; child-src 'none'; object-src 'none'; script-src 'none'; connect-src 'none'; worker-src 'none'; manifest-src 'none'; prefetch-src 'none'; style-src 'unsafe-inline'; style-src-elem 'unsafe-inline'; style-src-attr 'unsafe-inline'; img-src data: blob:; font-src data:; media-src data: blob:";
    internal const string StrictOfflineDocumentCspMeta =
        "<meta http-equiv=\"Content-Security-Policy\" content=\"" + StrictOfflineDocumentCspContent + "\">";

    private static readonly LocalChromiumBrowserCaptureConcurrencyGate SharedCaptureConcurrencyGate =
        new(DefaultMaxConcurrentBrowserCaptureProcesses);

    private static readonly string[] DefaultExecutableCandidates =
    [
        "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
        "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe",
        "C:\\Program Files\\Microsoft\\Edge\\Application\\msedge.exe",
        "C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe",
        "/usr/bin/google-chrome",
        "/usr/bin/google-chrome-stable",
        "/usr/bin/chromium",
        "/usr/bin/chromium-browser",
        "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
        "/Applications/Microsoft Edge.app/Contents/MacOS/Microsoft Edge"
    ];

    private readonly string? _executablePath;
    private readonly TimeSpan _timeout;
    private readonly string _baseRoot;
    private readonly LocalChromiumBrowserCaptureConcurrencyGate _captureConcurrencyGate;
    private readonly LocalChromiumBrowserCaptureStorageBudget _storageBudget;
    private readonly ILocalChromiumBrowserProcessRunner _processRunner;
    private readonly IWorkspaceCleaner _workspaceCleaner;
    private readonly DesignConversionBrowserQaOperationRateLimiter _operationRateLimiter;
    private readonly LocalChromiumBrowserRssPolicy _rssPolicy;

    public LocalChromiumBrowserProcessAdapter(
        string? executablePath = null,
        TimeSpan? timeout = null,
        string? artifactRoot = null,
        long? maxRetainedCaptureBytes = null,
        long? maxBrowserProcessRssBytes = null)
        : this(
            executablePath,
            timeout,
            artifactRoot,
            SharedCaptureConcurrencyGate,
            maxRetainedCaptureBytes,
            processRunner: null,
            workspaceCleaner: null,
            operationRateLimiter: null,
            maxBrowserProcessRssBytes: maxBrowserProcessRssBytes,
            resolveDefaultExecutableWhenMissing: true,
            includeEnvironmentExecutableCandidates: true)
    {
    }

    internal LocalChromiumBrowserProcessAdapter(
        string? executablePath,
        TimeSpan? timeout,
        string? artifactRoot,
        LocalChromiumBrowserCaptureConcurrencyGate captureConcurrencyGate,
        long? maxRetainedCaptureBytes = null,
        ILocalChromiumBrowserProcessRunner? processRunner = null,
        IWorkspaceCleaner? workspaceCleaner = null,
        DesignConversionBrowserQaOperationRateLimiter? operationRateLimiter = null,
        long? maxBrowserProcessRssBytes = null)
        : this(
            executablePath,
            timeout,
            artifactRoot,
            captureConcurrencyGate,
            maxRetainedCaptureBytes,
            processRunner,
            workspaceCleaner,
            operationRateLimiter,
            maxBrowserProcessRssBytes,
            resolveDefaultExecutableWhenMissing: true,
            includeEnvironmentExecutableCandidates: true)
    {
    }

    private LocalChromiumBrowserProcessAdapter(
        string? executablePath,
        TimeSpan? timeout,
        string? artifactRoot,
        LocalChromiumBrowserCaptureConcurrencyGate captureConcurrencyGate,
        long? maxRetainedCaptureBytes,
        ILocalChromiumBrowserProcessRunner? processRunner,
        IWorkspaceCleaner? workspaceCleaner,
        DesignConversionBrowserQaOperationRateLimiter? operationRateLimiter,
        long? maxBrowserProcessRssBytes,
        bool resolveDefaultExecutableWhenMissing,
        bool includeEnvironmentExecutableCandidates)
    {
        _rssPolicy = LocalChromiumBrowserRssPolicy.FromMaxBytes(maxBrowserProcessRssBytes);
        _executablePath = string.IsNullOrWhiteSpace(executablePath)
            ? resolveDefaultExecutableWhenMissing
                ? TryResolveDefaultExecutablePath(includeEnvironmentExecutableCandidates)
                : null
            : executablePath;
        _timeout = timeout ?? TimeSpan.FromSeconds(15);
        _baseRoot = NormalizeBrowserCapturePath(string.IsNullOrWhiteSpace(artifactRoot)
            ? Path.Combine(Path.GetTempPath(), "llm_mvp_core", "design-conversion-browser-qa")
            : artifactRoot);
        _captureConcurrencyGate = captureConcurrencyGate ?? throw new ArgumentNullException(nameof(captureConcurrencyGate));
        _storageBudget = new LocalChromiumBrowserCaptureStorageBudget(
            _baseRoot,
            maxRetainedCaptureBytes ?? DefaultMaxRetainedCaptureBytes,
            DesignConversionBrowserPngEvidenceInspector.MaxPngScreenshotBytes);
        _processRunner = processRunner ?? DefaultLocalChromiumBrowserProcessRunner.Instance;
        _workspaceCleaner = workspaceCleaner ?? RetryingWorkspaceCleaner.Instance;
        _operationRateLimiter = operationRateLimiter ?? new DesignConversionBrowserQaOperationRateLimiter(
            DefaultBrowserCaptureOperationLimitPerWindow,
            DefaultBrowserCaptureOperationRateLimitWindow);
    }

    public static LocalChromiumBrowserProcessAdapter CreateServerOwnedDefault(
        TimeSpan? timeout = null,
        string? artifactRoot = null,
        long? maxRetainedCaptureBytes = null,
        long? maxBrowserProcessRssBytes = null)
    {
        _ = LocalChromiumBrowserRssPolicy.FromMaxBytes(maxBrowserProcessRssBytes);
        return new(
            executablePath: TryResolveServerOwnedDefaultExecutablePath(),
            timeout: timeout,
            artifactRoot: artifactRoot,
            captureConcurrencyGate: SharedCaptureConcurrencyGate,
            maxRetainedCaptureBytes: maxRetainedCaptureBytes,
            processRunner: null,
            workspaceCleaner: null,
            operationRateLimiter: null,
            maxBrowserProcessRssBytes: maxBrowserProcessRssBytes,
            resolveDefaultExecutableWhenMissing: false,
            includeEnvironmentExecutableCandidates: false);
    }

    public static string? TryResolveDefaultExecutablePath()
        => TryResolveDefaultExecutablePath(includeEnvironmentConfiguredCandidates: true);

    public static string? TryResolveServerOwnedDefaultExecutablePath()
        => TryResolveDefaultExecutablePath(includeEnvironmentConfiguredCandidates: false);

    private static string? TryResolveDefaultExecutablePath(bool includeEnvironmentConfiguredCandidates)
    {
        var configuredCandidates = new[]
        {
            Environment.GetEnvironmentVariable("DESIGN_CONVERSION_BROWSER_EXECUTABLE"),
            Environment.GetEnvironmentVariable("PLAYWRIGHT_CHROMIUM_EXECUTABLE_PATH"),
            Environment.GetEnvironmentVariable("CHROME_PATH"),
            Environment.GetEnvironmentVariable("EDGE_PATH")
        };

        var candidates = includeEnvironmentConfiguredCandidates
            ? configuredCandidates.Concat(DefaultExecutableCandidates)
            : DefaultExecutableCandidates;

        return candidates
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .FirstOrDefault(File.Exists);
    }

    public async Task<OperationResult<DesignConversionBrowserCaptureResult>> CaptureScreenshotAsync(
        DesignConversionBrowserCaptureRequest request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<DesignConversionBrowserCaptureResult>.Fail("Browser capture cancelled.", "CANCELLED");
        if (request is null)
            return OperationResult<DesignConversionBrowserCaptureResult>.Fail(
                "Browser capture request is required.",
                "BROWSER_CAPTURE_REQUEST_REQUIRED");
        if (request.Resolution is null)
            return OperationResult<DesignConversionBrowserCaptureResult>.Fail(
                "Browser capture resolution is required.",
                "BROWSER_CAPTURE_RESOLUTION_REQUIRED");
        var physicalResolution = DesignConversionBrowserPhysicalScreenshotResolution.Resolve(
            request.Resolution,
            "BROWSER_CAPTURE_RESOLUTION_INVALID",
            "Browser capture");
        if (!physicalResolution.IsSuccess)
            return physicalResolution.MapError<DesignConversionBrowserCaptureResult>();
        if (string.IsNullOrWhiteSpace(_executablePath) || !File.Exists(_executablePath))
            return OperationResult<DesignConversionBrowserCaptureResult>.Fail(
                "No Chromium-family browser executable is available. Configure DESIGN_CONVERSION_BROWSER_EXECUTABLE, PLAYWRIGHT_CHROMIUM_EXECUTABLE_PATH, CHROME_PATH, or EDGE_PATH.",
                RuntimeUnavailableCode);
        if (!_operationRateLimiter.TryAcquire())
            return BrowserCaptureRateLimited();

        return await _captureConcurrencyGate.ExecuteAsync(
            () => CaptureScreenshotWithBrowserProcessAsync(request, cancellationToken),
            () => OperationResult<DesignConversionBrowserCaptureResult>.Fail("Browser capture cancelled.", "CANCELLED"),
            cancellationToken);
    }

    public async Task<IReadOnlyList<OperationResult<DesignConversionBrowserCaptureResult>>> CaptureScreenshotsAsync(
        IReadOnlyList<DesignConversionBrowserCaptureRequest> requests,
        CancellationToken cancellationToken = default)
    {
        if (requests is null || requests.Count == 0)
            return [];

        var results = new OperationResult<DesignConversionBrowserCaptureResult>[requests.Count];
        if (cancellationToken.IsCancellationRequested)
        {
            FillBrowserCaptureResults(
                results,
                OperationResult<DesignConversionBrowserCaptureResult>.Fail("Browser capture cancelled.", "CANCELLED"));
            return results;
        }

        var prepared = new List<(int Index, LocalChromiumPreparedBatchCapture Capture)>(requests.Count);
        for (var index = 0; index < requests.Count; index++)
        {
            var capture = PrepareLocalChromiumBatchCapture(requests[index], index);
            if (!capture.IsSuccess)
            {
                results[index] = capture.MapError<DesignConversionBrowserCaptureResult>();
                continue;
            }

            prepared.Add((index, capture.Value!));
        }

        if (prepared.Count == 0)
            return results;

        if (string.IsNullOrWhiteSpace(_executablePath) || !File.Exists(_executablePath))
        {
            var unavailable = OperationResult<DesignConversionBrowserCaptureResult>.Fail(
                "No Chromium-family browser executable is available. Configure DESIGN_CONVERSION_BROWSER_EXECUTABLE, PLAYWRIGHT_CHROMIUM_EXECUTABLE_PATH, CHROME_PATH, or EDGE_PATH.",
                RuntimeUnavailableCode);
            foreach (var item in prepared)
                results[item.Index] = unavailable;
            return results;
        }

        if (!_operationRateLimiter.TryAcquire())
        {
            var rateLimited = BrowserCaptureRateLimited();
            foreach (var item in prepared)
                results[item.Index] = rateLimited;
            return results;
        }

        var captured = await _captureConcurrencyGate.ExecuteAsync(
            () => CaptureScreenshotsWithBrowserProcessAsync(prepared.Select(item => item.Capture).ToArray(), cancellationToken),
            () => CreateBrowserCaptureBatchFailureResults(prepared.Count, "Browser capture cancelled.", "CANCELLED"),
            cancellationToken);

        for (var index = 0; index < prepared.Count; index++)
            results[prepared[index].Index] = captured[index];

        FillBrowserCaptureResults(
            results,
            OperationResult<DesignConversionBrowserCaptureResult>.Fail("Browser capture failed before producing a result.", ScreenshotFailedCode));
        return results;
    }

    private static OperationResult<LocalChromiumPreparedBatchCapture> PrepareLocalChromiumBatchCapture(
        DesignConversionBrowserCaptureRequest request,
        int ordinal)
    {
        if (request is null)
            return OperationResult<LocalChromiumPreparedBatchCapture>.Fail(
                "Browser capture request is required.",
                "BROWSER_CAPTURE_REQUEST_REQUIRED");
        if (request.Resolution is null)
            return OperationResult<LocalChromiumPreparedBatchCapture>.Fail(
                "Browser capture resolution is required.",
                "BROWSER_CAPTURE_RESOLUTION_REQUIRED");

        var physicalResolution = DesignConversionBrowserPhysicalScreenshotResolution.Resolve(
            request.Resolution,
            "BROWSER_CAPTURE_RESOLUTION_INVALID",
            "Browser capture");
        if (!physicalResolution.IsSuccess)
            return physicalResolution.MapError<LocalChromiumPreparedBatchCapture>();

        return OperationResult<LocalChromiumPreparedBatchCapture>.Ok(new LocalChromiumPreparedBatchCapture(
            Request: request,
            PhysicalResolution: physicalResolution.Value!,
            Ordinal: ordinal));
    }

    private async Task<IReadOnlyList<OperationResult<DesignConversionBrowserCaptureResult>>> CaptureScreenshotsWithBrowserProcessAsync(
        IReadOnlyList<LocalChromiumPreparedBatchCapture> captures,
        CancellationToken cancellationToken)
    {
        var results = new OperationResult<DesignConversionBrowserCaptureResult>[captures.Count];
        var storageReservations = new List<LocalChromiumBrowserCaptureStorageBudget.LocalChromiumBrowserCaptureStorageReservation>(captures.Count);
        string? root = null;
        string? userDataPath = null;
        List<string>? htmlPaths = null;
        List<LocalChromiumBrowserProcessBatchCaptureRequest>? batchRequests = null;
        OperationResult<bool>? finalCleanup = null;
        if (_processRunner is not ILocalChromiumBrowserBatchProcessRunner batchRunner)
        {
            FillBrowserCaptureResults(
                results,
                OperationResult<DesignConversionBrowserCaptureResult>.Fail(
                    "Browser batch process runner is required for LocalChromium batch capture.",
                    BatchProcessRunnerRequiredCode));
            return results;
        }

        try
        {
            for (var index = 0; index < captures.Count; index++)
            {
                var reservation = _storageBudget.TryReserve();
                if (!reservation.IsSuccess)
                {
                    ReplaceBrowserCaptureResults(
                        results,
                        OperationResult<DesignConversionBrowserCaptureResult>.Fail(
                            "Browser capture retained PNG storage quota unavailable for the batch.",
                            LocalChromiumBrowserCaptureStorageBudget.StorageQuotaExceededCode));
                    return results;
                }

                storageReservations.Add(reservation.Value!);
            }

            root = Path.Combine(_baseRoot, Guid.NewGuid().ToString("N"));
            userDataPath = Path.Combine(root, "profile");
            htmlPaths = new List<string>(captures.Count);
            batchRequests = new List<LocalChromiumBrowserProcessBatchCaptureRequest>(captures.Count);

            Directory.CreateDirectory(root);
            Directory.CreateDirectory(userDataPath);
            for (var index = 0; index < captures.Count; index++)
            {
                var htmlPath = Path.Combine(root, index == 0 ? "index.html" : $"index-{index.ToString("D4", CultureInfo.InvariantCulture)}.html");
                var screenshotPath = Path.Combine(root, $"screenshot-{index.ToString("D4", CultureInfo.InvariantCulture)}.png");
                htmlPaths.Add(htmlPath);
                await File.WriteAllTextAsync(
                    htmlPath,
                    BuildStrictOfflineCaptureDocument(captures[index].Request.DocumentHtml),
                    Encoding.UTF8,
                    cancellationToken);
                batchRequests.Add(new LocalChromiumBrowserProcessBatchCaptureRequest(
                    HtmlPath: htmlPath,
                    ScreenshotPath: screenshotPath,
                    Resolution: captures[index].Request.Resolution,
                    Ordinal: captures[index].Ordinal));
            }

            IReadOnlyList<OperationResult<LocalChromiumBrowserProcessBatchCaptureResult>>? runResults;
            try
            {
                runResults = await batchRunner.CaptureBatchAsync(
                    _executablePath!,
                    userDataPath,
                    batchRequests,
                    _timeout,
                    _rssPolicy,
                    cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                FillBrowserCaptureResults(
                    results,
                    OperationResult<DesignConversionBrowserCaptureResult>.Fail("Browser capture cancelled.", "CANCELLED"));
                runResults = [];
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                FillBrowserCaptureResults(
                    results,
                    OperationResult<DesignConversionBrowserCaptureResult>.Fail(
                        $"Browser screenshot process timed out after {_timeout.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture)} seconds.",
                        "BROWSER_SCREENSHOT_TIMEOUT"));
                runResults = [];
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                FillBrowserCaptureResults(
                    results,
                    OperationResult<DesignConversionBrowserCaptureResult>.Fail(
                        $"Browser screenshot process failed with sanitized exception type {ex.GetType().Name}.",
                        ScreenshotFailedCode));
                runResults = [];
            }

            if (runResults is null)
            {
                ReplaceBrowserCaptureResults(
                    results,
                    OperationResult<DesignConversionBrowserCaptureResult>.Fail(
                        "Browser screenshot batch returned a null result envelope.",
                        BatchResultEnvelopeNullCode));
            }
            else if (runResults.Count == 0 && results.All(result => result is not null))
            {
            }
            else if (runResults.Count != captures.Count)
            {
                FillBrowserCaptureResults(
                    results,
                    OperationResult<DesignConversionBrowserCaptureResult>.Fail(
                        "Browser screenshot batch returned an invalid result envelope.",
                        "BROWSER_CAPTURE_BATCH_RESULT_COUNT_MISMATCH"));
            }
            else if (runResults.Any(run => run is null))
            {
                ReplaceBrowserCaptureResults(
                    results,
                    OperationResult<DesignConversionBrowserCaptureResult>.Fail(
                        "Browser screenshot batch returned a null result envelope.",
                        BatchResultEnvelopeNullCode));
            }
            else
            {
                for (var index = 0; index < captures.Count; index++)
                {
                    var run = runResults[index]!;
                    if (!run.IsSuccess)
                    {
                        results[index] = SanitizeLocalChromiumBatchRunFailure(run);
                        continue;
                    }

                    var screenshotPath = batchRequests[index].ScreenshotPath;
                    var screenshotLength = ValidateCapturedScreenshotFileLengthBeforeRead(screenshotPath);
                    if (!screenshotLength.IsSuccess)
                    {
                        results[index] = screenshotLength.MapError<DesignConversionBrowserCaptureResult>();
                        continue;
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    var screenshotBytes = await ReadCapturedScreenshotBytesAsync(
                        screenshotPath,
                        screenshotLength.Value,
                        cancellationToken);
                    if (!screenshotBytes.IsSuccess)
                    {
                        results[index] = screenshotBytes.MapError<DesignConversionBrowserCaptureResult>();
                        continue;
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    var sha = Convert.ToHexString(SHA256.HashData(screenshotBytes.Value!)).ToLowerInvariant();
                    var metadata = new Dictionary<string, string>
                    {
                        ["browser_adapter_kind"] = "local_chromium_process",
                        ["browser_capture_mode"] = "local_chromium_batch",
                        ["browser_screenshot_png_retained"] = bool.TrueString,
                        ["browser_capture_storage_quota_bytes"] = _storageBudget.MaxRetainedCaptureBytes.ToString(CultureInfo.InvariantCulture),
                        ["browser_capture_storage_reservation_bytes"] = _storageBudget.ReservationBytes.ToString(CultureInfo.InvariantCulture),
                        ["browser_capture_screenshot_byte_length"] = screenshotLength.Value.ToString(CultureInfo.InvariantCulture),
                        ["browser_stdout_sha256"] = DeterministicDesignConversionRenderProvider.Sha256Text(run.Value!.Stdout),
                        ["browser_stderr_sha256"] = DeterministicDesignConversionRenderProvider.Sha256Text(run.Value.Stderr),
                        ["browser_session_id"] = run.Value.BrowserSessionId,
                        ["browser_observed_process_id"] = run.Value.ObservedBrowserProcessId.ToString(CultureInfo.InvariantCulture),
                        ["browser_process_start_count"] = run.Value.ProcessStartCount.ToString(CultureInfo.InvariantCulture),
                        ["browser_batch_capture_count"] = run.Value.BatchCaptureCount.ToString(CultureInfo.InvariantCulture),
                        ["browser_batch_ordinal"] = run.Value.BatchOrdinal.ToString(CultureInfo.InvariantCulture),
                        ["browser_session_reused"] = (run.Value.BatchOrdinal > 0).ToString()
                    };
                    results[index] = OperationResult<DesignConversionBrowserCaptureResult>.Ok(
                        new DesignConversionBrowserCaptureResult(
                            ScreenshotUri: new Uri(screenshotPath).AbsoluteUri,
                            ScreenshotSha256: sha,
                            ElapsedMs: run.Value.ElapsedMs,
                            Metadata: metadata,
                            Warnings: []));
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            FillBrowserCaptureResults(
                results,
                OperationResult<DesignConversionBrowserCaptureResult>.Fail("Browser capture cancelled.", "CANCELLED"));
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            FillBrowserCaptureResults(
                results,
                OperationResult<DesignConversionBrowserCaptureResult>.Fail(
                    $"Browser screenshot process timed out after {_timeout.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture)} seconds.",
                    "BROWSER_SCREENSHOT_TIMEOUT"));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            FillBrowserCaptureResults(
                results,
                OperationResult<DesignConversionBrowserCaptureResult>.Fail(
                    $"Browser screenshot process failed with sanitized exception type {ex.GetType().Name}.",
                    ScreenshotFailedCode));
        }
        finally
        {
            if (root is not null && userDataPath is not null)
            {
                var retainedScreenshotPaths = CollectRetainedBatchScreenshotPaths(results, batchRequests);
                if (retainedScreenshotPaths.Count > 0 && htmlPaths is not null)
                {
                    finalCleanup = CleanupUnreferencedBatchScreenshots(_baseRoot, root, retainedScreenshotPaths);
                    if (finalCleanup.IsSuccess)
                        finalCleanup = _workspaceCleaner.CleanupSensitiveBrowserCaptureWorkspace(root, htmlPaths, userDataPath);

                    if (!finalCleanup.IsSuccess)
                    {
                        try
                        {
                            _ = _workspaceCleaner.CleanupFailedBrowserCaptureWorkspace(_baseRoot, root);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                else
                {
                    finalCleanup = _workspaceCleaner.CleanupFailedBrowserCaptureWorkspace(_baseRoot, root);
                }
            }

            foreach (var reservation in storageReservations)
                reservation.Dispose();
        }

        if (finalCleanup is { IsSuccess: false })
        {
            ReplaceBrowserCaptureResults(
                results,
                finalCleanup.MapError<DesignConversionBrowserCaptureResult>());
        }
        else if (finalCleanup is { IsSuccess: true })
        {
            MarkBatchSensitiveWorkspaceRemoved(results);
        }

        FillBrowserCaptureResults(
            results,
            OperationResult<DesignConversionBrowserCaptureResult>.Fail(
                "Browser screenshot process failed before producing a result.",
                ScreenshotFailedCode));
        return results;
    }

    private static OperationResult<DesignConversionBrowserCaptureResult> SanitizeLocalChromiumBatchRunFailure(
        OperationResult<LocalChromiumBrowserProcessBatchCaptureResult> run)
    {
        var errorCode = string.IsNullOrWhiteSpace(run.ErrorCode)
            ? ScreenshotFailedCode
            : run.ErrorCode.Trim().ToUpperInvariant();
        return errorCode switch
        {
            "CANCELLED" => OperationResult<DesignConversionBrowserCaptureResult>.Fail(
                "Browser capture cancelled.",
                "CANCELLED"),
            "BROWSER_SCREENSHOT_TIMEOUT" => OperationResult<DesignConversionBrowserCaptureResult>.Fail(
                "Browser screenshot process timed out.",
                "BROWSER_SCREENSHOT_TIMEOUT"),
            RuntimeUnavailableCode => OperationResult<DesignConversionBrowserCaptureResult>.Fail(
                "No Chromium-family browser executable is available.",
                RuntimeUnavailableCode),
            RuntimeCrashedBeforeScreenshotCode => OperationResult<DesignConversionBrowserCaptureResult>.Fail(
                "Chromium runtime exited before producing a screenshot.",
                RuntimeCrashedBeforeScreenshotCode),
            LocalChromiumBrowserRssGuard.LimitExceededCode => OperationResult<DesignConversionBrowserCaptureResult>.Fail(
                LocalChromiumBrowserRssGuard.LimitExceededMessage,
                LocalChromiumBrowserRssGuard.LimitExceededCode),
            LocalChromiumBrowserRssGuard.InspectionFailedCode => OperationResult<DesignConversionBrowserCaptureResult>.Fail(
                LocalChromiumBrowserRssGuard.InspectionFailedMessage,
                LocalChromiumBrowserRssGuard.InspectionFailedCode),
            _ => OperationResult<DesignConversionBrowserCaptureResult>.Fail(
                "Browser screenshot process failed.",
                errorCode)
        };
    }

    private static OperationResult<DesignConversionBrowserCaptureResult>[] CreateBrowserCaptureBatchFailureResults(
        int count,
        string message,
        string errorCode)
    {
        var results = new OperationResult<DesignConversionBrowserCaptureResult>[count];
        FillBrowserCaptureResults(
            results,
            OperationResult<DesignConversionBrowserCaptureResult>.Fail(message, errorCode));
        return results;
    }

    private static OperationResult<DesignConversionBrowserCaptureResult> BrowserCaptureRateLimited()
        => OperationResult<DesignConversionBrowserCaptureResult>.Fail(
            "Browser capture rate limit exceeded.",
            CaptureRateLimitedCode);

    private static void FillBrowserCaptureResults(
        OperationResult<DesignConversionBrowserCaptureResult>[] results,
        OperationResult<DesignConversionBrowserCaptureResult> result)
        => FillBrowserCaptureResultsFrom(results, 0, result);

    private static void ReplaceBrowserCaptureResults(
        OperationResult<DesignConversionBrowserCaptureResult>[] results,
        OperationResult<DesignConversionBrowserCaptureResult> result)
    {
        for (var index = 0; index < results.Length; index++)
            results[index] = result;
    }

    private static void FillBrowserCaptureResultsFrom(
        OperationResult<DesignConversionBrowserCaptureResult>[] results,
        int startIndex,
        OperationResult<DesignConversionBrowserCaptureResult> result)
    {
        for (var index = Math.Max(0, startIndex); index < results.Length; index++)
            results[index] ??= result;
    }

    private static List<string> CollectRetainedBatchScreenshotPaths(
        OperationResult<DesignConversionBrowserCaptureResult>[] results,
        IReadOnlyList<LocalChromiumBrowserProcessBatchCaptureRequest>? batchRequests)
    {
        var retained = new List<string>();
        if (batchRequests is null)
            return retained;

        for (var index = 0; index < results.Length && index < batchRequests.Count; index++)
        {
            if (results[index] is { IsSuccess: true })
                retained.Add(batchRequests[index].ScreenshotPath);
        }

        return retained;
    }

    private static OperationResult<bool> CleanupUnreferencedBatchScreenshots(
        string baseRoot,
        string captureRoot,
        IReadOnlyCollection<string> retainedScreenshotPaths)
    {
        try
        {
            var root = ValidateBrowserCaptureWorkspaceRoot(baseRoot, captureRoot);
            if (!root.IsSuccess)
                return root.MapError<bool>();

            var pathComparison = OperatingSystem.IsWindows()
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal;
            var retained = new HashSet<string>(pathComparison);
            foreach (var retainedPath in retainedScreenshotPaths)
            {
                var normalizedRetained = NormalizeBrowserCapturePath(retainedPath);
                if (!IsBatchScreenshotPath(root.Value!, normalizedRetained))
                    return SensitiveCleanupFailure();

                retained.Add(normalizedRetained);
            }

            var rootInfo = new DirectoryInfo(root.Value!);
            rootInfo.Refresh();
            if (!rootInfo.Exists)
                return OperationResult<bool>.Ok(true);
            if ((rootInfo.Attributes & FileAttributes.ReparsePoint) != 0)
                return SensitiveCleanupFailure();

            var pngFiles = rootInfo
                .EnumerateFiles("*", SearchOption.TopDirectoryOnly)
                .Where(file => string.Equals(file.Extension, ".png", StringComparison.OrdinalIgnoreCase))
                .ToArray();
            foreach (var screenshot in pngFiles)
            {
                screenshot.Refresh();
                if (!screenshot.Exists)
                    continue;
                if ((screenshot.Attributes & FileAttributes.ReparsePoint) != 0)
                    return SensitiveCleanupFailure();

                var screenshotPath = NormalizeBrowserCapturePath(screenshot.FullName);
                var screenshotParent = Path.GetDirectoryName(screenshotPath);
                if (string.IsNullOrWhiteSpace(screenshotParent) ||
                    !SameBrowserCapturePath(screenshotParent, root.Value!))
                {
                    return SensitiveCleanupFailure();
                }

                if (retained.Contains(screenshotPath))
                    continue;

                screenshot.Delete();
                screenshot.Refresh();
                if (screenshot.Exists)
                    return SensitiveCleanupFailure();
            }

            foreach (var retainedPath in retained)
            {
                var retainedInfo = new FileInfo(retainedPath);
                retainedInfo.Refresh();
                if (!retainedInfo.Exists ||
                    (retainedInfo.Attributes & FileAttributes.ReparsePoint) != 0)
                {
                    return SensitiveCleanupFailure();
                }
            }

            return OperationResult<bool>.Ok(true);
        }
        catch (Exception)
        {
            return SensitiveCleanupFailure();
        }
    }

    private static bool IsBatchScreenshotPath(string root, string screenshotPath)
    {
        var parent = Path.GetDirectoryName(screenshotPath);
        if (string.IsNullOrWhiteSpace(parent) || !SameBrowserCapturePath(parent, root))
            return false;

        var fileName = Path.GetFileName(screenshotPath);
        if (!fileName.StartsWith("screenshot-", StringComparison.Ordinal) ||
            !fileName.EndsWith(".png", StringComparison.Ordinal))
        {
            return false;
        }

        var ordinal = fileName.Substring(
            "screenshot-".Length,
            fileName.Length - "screenshot-".Length - ".png".Length);
        return ordinal.Length == 4 && ordinal.All(char.IsDigit);
    }

    private static void MarkBatchSensitiveWorkspaceRemoved(
        OperationResult<DesignConversionBrowserCaptureResult>[] results)
    {
        for (var index = 0; index < results.Length; index++)
        {
            if (results[index] is not { IsSuccess: true, Value: { } value })
                continue;

            var metadata = new Dictionary<string, string>(value.Metadata, StringComparer.Ordinal)
            {
                ["browser_sensitive_workspace_removed"] = bool.TrueString
            };
            results[index] = OperationResult<DesignConversionBrowserCaptureResult>.Ok(value with
            {
                Metadata = metadata
            });
        }
    }

    private async Task<OperationResult<DesignConversionBrowserCaptureResult>> CaptureScreenshotWithBrowserProcessAsync(
        DesignConversionBrowserCaptureRequest request,
        CancellationToken cancellationToken)
    {
        LocalChromiumBrowserCaptureStorageBudget.LocalChromiumBrowserCaptureStorageReservation? storageReservation = null;
        string? root = null;
        string? htmlPath = null;
        string? screenshotPath = null;
        string? userDataPath = null;
        var captureCommittedForDownstream = false;
        var sensitiveWorkspaceCleanupProven = false;
        OperationResult<DesignConversionBrowserCaptureResult>? result = null;
        OperationResult<bool>? finalCleanup = null;
        try
        {
            var reservation = _storageBudget.TryReserve();
            if (!reservation.IsSuccess)
                return reservation.MapError<DesignConversionBrowserCaptureResult>();
            storageReservation = reservation.Value;

            root = Path.Combine(_baseRoot, Guid.NewGuid().ToString("N"));
            htmlPath = Path.Combine(root, "index.html");
            screenshotPath = Path.Combine(root, "screenshot.png");
            userDataPath = Path.Combine(root, "profile");

            Directory.CreateDirectory(root);
            Directory.CreateDirectory(userDataPath);
            await File.WriteAllTextAsync(
                htmlPath,
                BuildStrictOfflineCaptureDocument(request.DocumentHtml),
                Encoding.UTF8,
                cancellationToken);

            var run = await _processRunner.CaptureAsync(
                _executablePath!,
                htmlPath,
                screenshotPath,
                userDataPath,
                request.Resolution,
                _timeout,
                _rssPolicy,
                cancellationToken);
            if (!run.IsSuccess)
            {
                result = run.MapError<DesignConversionBrowserCaptureResult>();
            }
            else
            {
                var screenshotLength = ValidateCapturedScreenshotFileLengthBeforeRead(screenshotPath);
                if (!screenshotLength.IsSuccess)
                {
                    result = screenshotLength.MapError<DesignConversionBrowserCaptureResult>();
                }
                else
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var screenshotBytes = await ReadCapturedScreenshotBytesAsync(
                        screenshotPath,
                        screenshotLength.Value,
                        cancellationToken);
                    if (!screenshotBytes.IsSuccess)
                    {
                        result = screenshotBytes.MapError<DesignConversionBrowserCaptureResult>();
                    }
                    else
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var sha = Convert.ToHexString(SHA256.HashData(screenshotBytes.Value!)).ToLowerInvariant();
                        var cleanup = _workspaceCleaner.CleanupSensitiveBrowserCaptureWorkspace(root, htmlPath, userDataPath);
                        if (!cleanup.IsSuccess)
                        {
                            result = cleanup.MapError<DesignConversionBrowserCaptureResult>();
                        }
                        else
                        {
                            sensitiveWorkspaceCleanupProven = true;
                            var captureResult = new DesignConversionBrowserCaptureResult(
                                ScreenshotUri: new Uri(screenshotPath).AbsoluteUri,
                                ScreenshotSha256: sha,
                                ElapsedMs: run.Value!.ElapsedMs,
                                Metadata: new Dictionary<string, string>
                                {
                                    ["browser_adapter_kind"] = "local_chromium_process",
                                    ["browser_executable"] = _executablePath!,
                                    ["browser_screenshot_path"] = screenshotPath,
                                    ["browser_screenshot_png_retained"] = bool.TrueString,
                                    ["browser_sensitive_workspace_removed"] = bool.TrueString,
                                    ["browser_capture_storage_quota_bytes"] = _storageBudget.MaxRetainedCaptureBytes.ToString(CultureInfo.InvariantCulture),
                                    ["browser_capture_storage_reservation_bytes"] = _storageBudget.ReservationBytes.ToString(CultureInfo.InvariantCulture),
                                    ["browser_capture_screenshot_byte_length"] = screenshotLength.Value.ToString(CultureInfo.InvariantCulture),
                                    ["browser_stdout_sha256"] = DeterministicDesignConversionRenderProvider.Sha256Text(run.Value!.Stdout),
                                    ["browser_stderr_sha256"] = DeterministicDesignConversionRenderProvider.Sha256Text(run.Value.Stderr)
                                },
                                Warnings: []);
                            captureCommittedForDownstream = true;
                            result = OperationResult<DesignConversionBrowserCaptureResult>.Ok(captureResult);
                        }
                    }
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            result = OperationResult<DesignConversionBrowserCaptureResult>.Fail(
                "Browser capture cancelled.",
                "CANCELLED");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            result = OperationResult<DesignConversionBrowserCaptureResult>.Fail(
                $"Browser screenshot process timed out after {_timeout.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture)} seconds.",
                "BROWSER_SCREENSHOT_TIMEOUT");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            result = OperationResult<DesignConversionBrowserCaptureResult>.Fail(
                $"Browser screenshot process failed with sanitized exception type {ex.GetType().Name}.",
                ScreenshotFailedCode);
        }
        finally
        {
            if (root is not null && htmlPath is not null && userDataPath is not null)
            {
                finalCleanup = captureCommittedForDownstream && sensitiveWorkspaceCleanupProven
                    ? OperationResult<bool>.Ok(true)
                    : captureCommittedForDownstream
                        ? _workspaceCleaner.CleanupSensitiveBrowserCaptureWorkspace(root, htmlPath, userDataPath)
                        : _workspaceCleaner.CleanupFailedBrowserCaptureWorkspace(_baseRoot, root);
            }

            storageReservation?.Dispose();
        }

        if (finalCleanup is { IsSuccess: false })
            return finalCleanup.MapError<DesignConversionBrowserCaptureResult>();

        return result ?? OperationResult<DesignConversionBrowserCaptureResult>.Fail(
            "Browser screenshot process failed before producing a result.",
            ScreenshotFailedCode);
    }

    internal static OperationResult<long> ValidateCapturedScreenshotFileLengthBeforeRead(string screenshotPath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(screenshotPath))
                return OperationResult<long>.Fail(
                    "Browser screenshot file length could not be inspected safely.",
                    "BROWSER_SCREENSHOT_FILE_INSPECTION_FAILED");

            var fileInfo = new FileInfo(screenshotPath);
            fileInfo.Refresh();
            if (!fileInfo.Exists)
                return OperationResult<long>.Fail(
                    "Browser screenshot file was not produced.",
                    "BROWSER_SCREENSHOT_FAILED");
            if ((fileInfo.Attributes & FileAttributes.ReparsePoint) != 0)
                return OperationResult<long>.Fail(
                    "Browser screenshot file state is unsafe.",
                    "BROWSER_SCREENSHOT_FILE_UNSAFE");
            if (fileInfo.Length > DesignConversionBrowserPngEvidenceInspector.MaxPngScreenshotBytes)
                return OperationResult<long>.Fail(
                    $"Browser screenshot PNG exceeds the configured byte limit of {DesignConversionBrowserPngEvidenceInspector.MaxPngScreenshotBytes.ToString(CultureInfo.InvariantCulture)} bytes.",
                    "BROWSER_SCREENSHOT_BYTES_TOO_LARGE");

            return OperationResult<long>.Ok(fileInfo.Length);
        }
        catch (Exception)
        {
            return OperationResult<long>.Fail(
                "Browser screenshot file length could not be inspected safely.",
                "BROWSER_SCREENSHOT_FILE_INSPECTION_FAILED");
        }
    }

    private static async Task<OperationResult<byte[]>> ReadCapturedScreenshotBytesAsync(
        string screenshotPath,
        long precheckedLength,
        CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            screenshotPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);

        var openedLength = stream.Length;
        if (openedLength != precheckedLength)
            return OperationResult<byte[]>.Fail(
                "Browser screenshot file changed while it was being read.",
                "BROWSER_SCREENSHOT_FILE_INSPECTION_FAILED");

        var bytes = await DesignConversionBoundedContentReader.ReadToEndAsync(
            stream,
            DesignConversionBrowserPngEvidenceInspector.MaxPngScreenshotBytes,
            "Browser screenshot PNG",
            "BROWSER_SCREENSHOT_BYTES_TOO_LARGE",
            "BROWSER_SCREENSHOT_FILE_INSPECTION_FAILED",
            cancellationToken);
        if (!bytes.IsSuccess)
            return bytes;

        if (stream.Length != openedLength)
            return OperationResult<byte[]>.Fail(
                "Browser screenshot file changed while it was being read.",
                "BROWSER_SCREENSHOT_FILE_INSPECTION_FAILED");

        return bytes;
    }

    private sealed record LocalChromiumPreparedBatchCapture(
        DesignConversionBrowserCaptureRequest Request,
        DesignConversionBrowserPhysicalScreenshotResolution PhysicalResolution,
        int Ordinal);

    private sealed class DefaultLocalChromiumBrowserProcessRunner : ILocalChromiumBrowserBatchProcessRunner
    {
        public static readonly DefaultLocalChromiumBrowserProcessRunner Instance = new();

        private DefaultLocalChromiumBrowserProcessRunner()
        {
        }

        public async Task<OperationResult<LocalChromiumBrowserProcessRunResult>> CaptureAsync(
            string executablePath,
            string htmlPath,
            string screenshotPath,
            string userDataPath,
            RenderQaResolution resolution,
            TimeSpan timeout,
            LocalChromiumBrowserRssPolicy rssPolicy,
            CancellationToken cancellationToken)
        {
            Process? process = null;
            LocalChromiumBrowserRssMonitor? rssMonitor = null;
            OperationResult<bool>? rssFailure = null;
            try
            {
                var stopwatch = Stopwatch.StartNew();
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(timeout);

                process = new Process
                {
                    StartInfo = BuildStartInfo(executablePath, htmlPath, screenshotPath, userDataPath, resolution)
                };
                if (!TryStartBrowserProcess(process, out _))
                    return OperationResult<LocalChromiumBrowserProcessRunResult>.Fail(
                        "Browser screenshot process could not start.",
                        RuntimeUnavailableCode);

                rssMonitor = StartBrowserRssMonitor(process, rssPolicy, timeoutCts.Token);
                var initialRssFailure = await TryGetRssRunFailureAsync(drain: false).ConfigureAwait(false);
                if (initialRssFailure is not null)
                    return initialRssFailure;

                var stdoutTask = process.StandardOutput.ReadToEndAsync(timeoutCts.Token);
                var stderrTask = process.StandardError.ReadToEndAsync(timeoutCts.Token);
                await process.WaitForExitAsync(timeoutCts.Token);
                stopwatch.Stop();

                var completedRssFailure = await TryGetRssRunFailureAsync(drain: true).ConfigureAwait(false);
                if (completedRssFailure is not null)
                    return completedRssFailure;

                var stdout = await stdoutTask;
                var stderr = await stderrTask;
                var screenshotExists = File.Exists(screenshotPath);
                if (process.ExitCode != 0 || !screenshotExists)
                    return ClassifyCompletedBrowserProcessFailure(process.ExitCode, screenshotExists, stderr, stdout);

                return OperationResult<LocalChromiumBrowserProcessRunResult>.Ok(
                    new LocalChromiumBrowserProcessRunResult(
                        ElapsedMs: stopwatch.ElapsedMilliseconds,
                        Stdout: stdout,
                        Stderr: stderr));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                var rssResult = await TryGetRssRunFailureAsync(drain: true).ConfigureAwait(false);
                if (rssResult is not null)
                    return rssResult;

                throw;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                var rssResult = await TryGetRssRunFailureAsync(drain: true).ConfigureAwait(false);
                if (rssResult is not null)
                    return rssResult;

                TryKillBrowserProcess(process);
                return OperationResult<LocalChromiumBrowserProcessRunResult>.Fail(
                    $"Browser screenshot process timed out after {timeout.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture)} seconds.",
                    "BROWSER_SCREENSHOT_TIMEOUT");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var rssResult = await TryGetRssRunFailureAsync(drain: true).ConfigureAwait(false);
                if (rssResult is not null)
                    return rssResult;

                return OperationResult<LocalChromiumBrowserProcessRunResult>.Fail(
                    $"Browser screenshot process failed with sanitized exception type {ex.GetType().Name}.",
                    ScreenshotFailedCode);
            }
            finally
            {
                if (rssMonitor is not null)
                    rssFailure ??= await rssMonitor.StopAndDrainAsync().ConfigureAwait(false);
                TryKillBrowserProcess(process);
                process?.Dispose();
            }

            async Task<OperationResult<LocalChromiumBrowserProcessRunResult>?> TryGetRssRunFailureAsync(bool drain)
            {
                if (rssMonitor is null)
                    return null;

                if (drain)
                    rssFailure ??= await rssMonitor.StopAndDrainAsync().ConfigureAwait(false);
                else if (rssMonitor.Completion.IsCompleted)
                    rssFailure ??= await rssMonitor.Completion.ConfigureAwait(false);

                return rssFailure?.MapError<LocalChromiumBrowserProcessRunResult>();
            }
        }

        public async Task<IReadOnlyList<OperationResult<LocalChromiumBrowserProcessBatchCaptureResult>>> CaptureBatchAsync(
            string executablePath,
            string userDataPath,
            IReadOnlyList<LocalChromiumBrowserProcessBatchCaptureRequest> requests,
            TimeSpan timeout,
            LocalChromiumBrowserRssPolicy rssPolicy,
            CancellationToken cancellationToken)
        {
            if (requests is null || requests.Count == 0)
                return [];

            var results = new OperationResult<LocalChromiumBrowserProcessBatchCaptureResult>[requests.Count];
            Process? process = null;
            LocalChromiumBrowserRssMonitor? rssMonitor = null;
            OperationResult<bool>? rssFailure = null;
            var stdout = string.Empty;
            var stderr = string.Empty;
            var processId = -1;
            var sessionId = string.Empty;
            var stopwatch = Stopwatch.StartNew();
            try
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(timeout);
                PrepareDevToolsActivePortForLaunch(userDataPath);
                process = new Process
                {
                    StartInfo = BuildBatchStartInfo(executablePath, userDataPath)
                };
                if (!TryStartBrowserProcess(process, out _))
                {
                    FillBatchProcessResults(
                        results,
                        OperationResult<LocalChromiumBrowserProcessBatchCaptureResult>.Fail(
                            "Browser screenshot process could not start.",
                            RuntimeUnavailableCode));
                    return results;
                }

                rssMonitor = StartBrowserRssMonitor(process, rssPolicy, timeoutCts.Token);
                var initialRssFailure = await TryGetRssBatchFailureAsync(drain: false).ConfigureAwait(false);
                if (initialRssFailure is not null)
                {
                    FillBatchProcessResults(results, initialRssFailure);
                    return results;
                }

                processId = process.Id;
                var stdoutTask = process.StandardOutput.ReadToEndAsync(timeoutCts.Token);
                var stderrTask = process.StandardError.ReadToEndAsync(timeoutCts.Token);
                await using var client = await ChromiumDevToolsPageClient.ConnectAsync(
                    userDataPath,
                    processId,
                    () => HasBrowserProcessExited(process),
                    timeout,
                    timeoutCts.Token);
                sessionId = client.SessionId;

                for (var index = 0; index < requests.Count; index++)
                {
                    var captureStopwatch = Stopwatch.StartNew();
                    try
                    {
                        await client.CaptureScreenshotAsync(
                            requests[index].HtmlPath,
                            requests[index].ScreenshotPath,
                            requests[index].Resolution,
                            timeoutCts.Token);
                        captureStopwatch.Stop();
                        results[index] = OperationResult<LocalChromiumBrowserProcessBatchCaptureResult>.Ok(
                            new LocalChromiumBrowserProcessBatchCaptureResult(
                                ElapsedMs: captureStopwatch.ElapsedMilliseconds,
                                Stdout: string.Empty,
                                Stderr: string.Empty,
                                ObservedBrowserProcessId: processId,
                                BrowserSessionId: sessionId,
                                ProcessStartCount: 1,
                                BatchCaptureCount: requests.Count,
                                BatchOrdinal: requests[index].Ordinal));
                        var rssResult = await TryGetRssBatchFailureAsync(drain: false).ConfigureAwait(false);
                        if (rssResult is not null)
                        {
                            ReplaceBatchProcessResultsFrom(results, index, rssResult);
                            break;
                        }
                    }
                    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                    {
                        var rssResult = await TryGetRssBatchFailureAsync(drain: true).ConfigureAwait(false);
                        if (rssResult is not null)
                        {
                            FillBatchProcessResultsFrom(results, index, rssResult);
                            break;
                        }

                        FillBatchProcessResultsFrom(
                            results,
                            index,
                            OperationResult<LocalChromiumBrowserProcessBatchCaptureResult>.Fail(
                                $"Browser screenshot process timed out after {timeout.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture)} seconds.",
                                "BROWSER_SCREENSHOT_TIMEOUT"));
                        break;
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        var rssResult = await TryGetRssBatchFailureAsync(drain: true).ConfigureAwait(false);
                        if (rssResult is not null)
                        {
                            FillBatchProcessResultsFrom(results, index, rssResult);
                            break;
                        }

                        FillBatchProcessResultsFrom(results, index, BuildBatchProcessFailure(ex, process));
                        break;
                    }
                }

                stopwatch.Stop();
                var completedRssFailure = await TryGetRssBatchFailureAsync(drain: true).ConfigureAwait(false);
                if (completedRssFailure is not null)
                    ReplaceBatchProcessResultsFrom(results, 0, completedRssFailure);

                TryKillBrowserProcess(process);
                stdout = await ReadProcessTextOrEmptyAsync(stdoutTask);
                stderr = await ReadProcessTextOrEmptyAsync(stderrTask);
                for (var index = 0; index < results.Length; index++)
                {
                    if (!results[index]?.IsSuccess ?? true)
                        continue;

                    var value = results[index]!.Value!;
                    results[index] = OperationResult<LocalChromiumBrowserProcessBatchCaptureResult>.Ok(value with
                    {
                        ElapsedMs = value.ElapsedMs <= 0 ? Math.Max(0, stopwatch.ElapsedMilliseconds) : value.ElapsedMs,
                        Stdout = stdout,
                        Stderr = stderr
                    });
                }

                FillBatchProcessResults(
                    results,
                    OperationResult<LocalChromiumBrowserProcessBatchCaptureResult>.Fail(
                        "Browser screenshot process failed before producing a result.",
                        ScreenshotFailedCode));
                return results;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                var rssResult = await TryGetRssBatchFailureAsync(drain: true).ConfigureAwait(false);
                if (rssResult is not null)
                {
                    FillBatchProcessResults(results, rssResult);
                    return results;
                }

                FillBatchProcessResults(
                    results,
                    OperationResult<LocalChromiumBrowserProcessBatchCaptureResult>.Fail(
                        "Browser capture cancelled.",
                        "CANCELLED"));
                return results;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                var rssResult = await TryGetRssBatchFailureAsync(drain: true).ConfigureAwait(false);
                if (rssResult is not null)
                {
                    FillBatchProcessResults(results, rssResult);
                    return results;
                }

                FillBatchProcessResults(
                    results,
                    OperationResult<LocalChromiumBrowserProcessBatchCaptureResult>.Fail(
                        $"Browser screenshot process timed out after {timeout.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture)} seconds.",
                        "BROWSER_SCREENSHOT_TIMEOUT"));
                return results;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var rssResult = await TryGetRssBatchFailureAsync(drain: true).ConfigureAwait(false);
                if (rssResult is not null)
                {
                    FillBatchProcessResults(results, rssResult);
                    return results;
                }

                FillBatchProcessResults(results, BuildBatchProcessFailure(ex, process));
                return results;
            }
            finally
            {
                if (rssMonitor is not null)
                    rssFailure ??= await rssMonitor.StopAndDrainAsync().ConfigureAwait(false);
                TryKillBrowserProcess(process);
                process?.Dispose();
            }

            async Task<OperationResult<LocalChromiumBrowserProcessBatchCaptureResult>?> TryGetRssBatchFailureAsync(bool drain)
            {
                if (rssMonitor is null)
                    return null;

                if (drain)
                    rssFailure ??= await rssMonitor.StopAndDrainAsync().ConfigureAwait(false);
                else if (rssMonitor.Completion.IsCompleted)
                    rssFailure ??= await rssMonitor.Completion.ConfigureAwait(false);

                return rssFailure?.MapError<LocalChromiumBrowserProcessBatchCaptureResult>();
            }
        }

        private static async Task<string> ReadProcessTextOrEmptyAsync(Task<string> readTask)
        {
            try
            {
                return await readTask.ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is OperationCanceledException or ObjectDisposedException or InvalidOperationException or IOException)
            {
                return string.Empty;
            }
        }

        private static OperationResult<LocalChromiumBrowserProcessBatchCaptureResult> BuildBatchProcessFailure(
            Exception exception,
            Process? process)
        {
            if (process is { HasExited: true } && IsRecognizedBrowserRuntimeCrashExitCode(process.ExitCode))
            {
                return OperationResult<LocalChromiumBrowserProcessBatchCaptureResult>.Fail(
                    "Chromium runtime exited before producing a screenshot with a recognized process crash code.",
                    RuntimeCrashedBeforeScreenshotCode);
            }

            if (exception is ChromiumDevToolsEndpointOwnershipException ownership)
            {
                return OperationResult<LocalChromiumBrowserProcessBatchCaptureResult>.Fail(
                    "Chromium DevTools endpoint ownership could not be verified.",
                    ownership.ErrorCode);
            }

            return OperationResult<LocalChromiumBrowserProcessBatchCaptureResult>.Fail(
                $"Browser screenshot process failed with sanitized exception type {exception.GetType().Name}.",
                ScreenshotFailedCode);
        }
    }

    private static void PrepareDevToolsActivePortForLaunch(string userDataPath)
    {
        var userDataFullPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(userDataPath));
        if (Directory.Exists(userDataFullPath) &&
            File.GetAttributes(userDataFullPath).HasFlag(FileAttributes.ReparsePoint))
        {
            throw new ChromiumDevToolsEndpointOwnershipException();
        }

        var activePortPath = BuildDevToolsActivePortPath(userDataFullPath);
        if (!File.Exists(activePortPath))
            return;

        var attributes = File.GetAttributes(activePortPath);
        if (attributes.HasFlag(FileAttributes.ReparsePoint) ||
            attributes.HasFlag(FileAttributes.Directory))
        {
            throw new ChromiumDevToolsEndpointOwnershipException();
        }

        File.Delete(activePortPath);
    }

    private static string BuildDevToolsActivePortPath(string userDataPath)
    {
        var userDataFullPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(userDataPath));
        var activePortPath = Path.GetFullPath(Path.Combine(userDataFullPath, "DevToolsActivePort"));
        if (!IsPathUnderDirectory(activePortPath, userDataFullPath))
            throw new ChromiumDevToolsEndpointOwnershipException();

        return activePortPath;
    }

    private static bool IsPathUnderDirectory(string path, string directory)
    {
        var directoryWithSeparator = Path.TrimEndingDirectorySeparator(directory) + Path.DirectorySeparatorChar;
        return path.StartsWith(directoryWithSeparator, StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasBrowserProcessExited(Process process)
    {
        try
        {
            process.Refresh();
            return process.HasExited;
        }
        catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException)
        {
            return true;
        }
    }

    internal sealed class ChromiumDevToolsEndpointOwnershipException : InvalidOperationException
    {
        internal ChromiumDevToolsEndpointOwnershipException()
            : base("Chromium DevTools endpoint ownership could not be verified.")
        {
        }

        internal string ErrorCode => DevToolsEndpointOwnershipUnverifiedCode;
    }

    private static void FillBatchProcessResults(
        OperationResult<LocalChromiumBrowserProcessBatchCaptureResult>[] results,
        OperationResult<LocalChromiumBrowserProcessBatchCaptureResult> result)
        => FillBatchProcessResultsFrom(results, 0, result);

    private static void FillBatchProcessResultsFrom(
        OperationResult<LocalChromiumBrowserProcessBatchCaptureResult>[] results,
        int startIndex,
        OperationResult<LocalChromiumBrowserProcessBatchCaptureResult> result)
    {
        for (var index = Math.Max(0, startIndex); index < results.Length; index++)
            results[index] ??= result;
    }

    private static void ReplaceBatchProcessResultsFrom(
        OperationResult<LocalChromiumBrowserProcessBatchCaptureResult>[] results,
        int startIndex,
        OperationResult<LocalChromiumBrowserProcessBatchCaptureResult> result)
    {
        for (var index = Math.Max(0, startIndex); index < results.Length; index++)
            results[index] = result;
    }

    internal interface IChromiumDevToolsSocketTransport : IDisposable
    {
        WebSocketState State { get; }

        Task ConnectAsync(Uri uri, CancellationToken cancellationToken);

        Task SendAsync(
            ArraySegment<byte> buffer,
            WebSocketMessageType messageType,
            bool endOfMessage,
            CancellationToken cancellationToken);

        Task<WebSocketReceiveResult> ReceiveAsync(
            ArraySegment<byte> buffer,
            CancellationToken cancellationToken);

        Task CloseAsync(
            WebSocketCloseStatus closeStatus,
            string? statusDescription,
            CancellationToken cancellationToken);

        void Abort();
    }

    private sealed class ClientWebSocketTransport : IChromiumDevToolsSocketTransport
    {
        private readonly ClientWebSocket _socket = new();

        public WebSocketState State => _socket.State;

        public Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
            => _socket.ConnectAsync(uri, cancellationToken);

        public Task SendAsync(
            ArraySegment<byte> buffer,
            WebSocketMessageType messageType,
            bool endOfMessage,
            CancellationToken cancellationToken)
            => _socket.SendAsync(buffer, messageType, endOfMessage, cancellationToken);

        public Task<WebSocketReceiveResult> ReceiveAsync(
            ArraySegment<byte> buffer,
            CancellationToken cancellationToken)
            => _socket.ReceiveAsync(buffer, cancellationToken);

        public Task CloseAsync(
            WebSocketCloseStatus closeStatus,
            string? statusDescription,
            CancellationToken cancellationToken)
            => _socket.CloseAsync(closeStatus, statusDescription, cancellationToken);

        public void Abort()
            => _socket.Abort();

        public void Dispose()
            => _socket.Dispose();
    }

    internal sealed class ChromiumDevToolsPageClient : IAsyncDisposable
    {
        private const int ReceiveBufferSize = 16384;
        private const int MaxDevToolsActivePortBytes = 512;
        private const int MaxDevToolsJsonBytes = 64 * 1024;
        private const string DevToolsBrowserPathPrefix = "/devtools/browser/";
        private const string DevToolsPagePathPrefix = "/devtools/page/";
        private static readonly UTF8Encoding StrictDevToolsActivePortUtf8 = new(
            encoderShouldEmitUTF8Identifier: false,
            throwOnInvalidBytes: true);
        private static readonly TimeSpan CloseHandshakeTimeout = TimeSpan.FromMilliseconds(250);
        private static readonly TimeSpan ReceiveDrainTimeout = TimeSpan.FromMilliseconds(250);
        private static readonly TimeSpan DefaultCommandResponseTimeout = TimeSpan.FromSeconds(10);
        private static readonly int MaxDevToolsMessageBytes = checked((int)Math.Min(
            DesignConversionBrowserPngEvidenceInspector.MaxPngScreenshotBytes * 2,
            128L * 1024L * 1024L));

        private readonly IChromiumDevToolsSocketTransport _socket;
        private readonly CancellationTokenSource _disposeCts = new();
        private readonly TimeSpan _commandResponseTimeout;
        private readonly object _disposeSync = new();
        private readonly object _sync = new();
        private readonly Dictionary<int, TaskCompletionSource<JsonElement>> _pending = [];
        private readonly Task _receiveTask;
        private Task? _disposeTask;
        private TaskCompletionSource<bool>? _loadEvent;
        private bool _acceptingCommands = true;
        private int _nextCommandId;
        private int _abortRequested;

        private ChromiumDevToolsPageClient(
            IChromiumDevToolsSocketTransport socket,
            string sessionId,
            TimeSpan commandResponseTimeout)
        {
            _socket = socket;
            SessionId = sessionId;
            _commandResponseTimeout = commandResponseTimeout <= TimeSpan.Zero
                ? DefaultCommandResponseTimeout
                : commandResponseTimeout;
            _receiveTask = Task.Run(ReceiveLoopAsync);
        }

        public string SessionId { get; }

        internal int PendingCommandCountForTesting
        {
            get
            {
                lock (_sync)
                    return _pending.Count;
            }
        }

        internal bool HasLoadWaiterForTesting
        {
            get
            {
                lock (_sync)
                    return _loadEvent is not null;
            }
        }

        internal Task<JsonElement> SendCommandForTestingAsync(
            string method,
            CancellationToken cancellationToken = default)
            => SendCommandAsync(method, null, cancellationToken);

        internal TaskCompletionSource<bool> PrepareNextLoadWaiterForTesting()
            => PrepareNextLoadWaiter();

        public static async Task<ChromiumDevToolsPageClient> ConnectAsync(
            string userDataPath,
            int processId,
            Func<bool> hasProcessExited,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            var pageSocketUri = await ResolvePageWebSocketUriAsync(
                    userDataPath,
                    hasProcessExited,
                    timeout,
                    cancellationToken)
                .ConfigureAwait(false);
            return await ConnectCoreAsync(
                pageSocketUri,
                processId,
                static () => new ClientWebSocketTransport(),
                cancellationToken).ConfigureAwait(false);
        }

        internal static Task<ChromiumDevToolsPageClient> ConnectForTestingAsync(
            Uri pageSocketUri,
            int processId,
            Func<IChromiumDevToolsSocketTransport> transportFactory,
            CancellationToken cancellationToken = default,
            TimeSpan? commandResponseTimeout = null)
            => ConnectCoreAsync(
                pageSocketUri,
                processId,
                transportFactory,
                cancellationToken,
                commandResponseTimeout);

        internal static Task<Uri> ResolvePageWebSocketUriForTestingAsync(
            string userDataPath,
            TimeSpan timeout,
            CancellationToken cancellationToken = default)
            => ResolvePageWebSocketUriAsync(
                userDataPath,
                static () => false,
                timeout,
                cancellationToken);

        private static async Task<ChromiumDevToolsPageClient> ConnectCoreAsync(
            Uri pageSocketUri,
            int processId,
            Func<IChromiumDevToolsSocketTransport> transportFactory,
            CancellationToken cancellationToken,
            TimeSpan? commandResponseTimeout = null)
        {
            var sessionId = "cdp-" + DeterministicDesignConversionRenderProvider
                .Sha256Text($"{processId.ToString(CultureInfo.InvariantCulture)}|{pageSocketUri}")[..16];
            IChromiumDevToolsSocketTransport? socket = null;
            ChromiumDevToolsPageClient? client = null;
            try
            {
                socket = transportFactory();
                await socket.ConnectAsync(pageSocketUri, cancellationToken).ConfigureAwait(false);
                client = new ChromiumDevToolsPageClient(
                    socket,
                    sessionId,
                    commandResponseTimeout ?? DefaultCommandResponseTimeout);
                socket = null;

                await client.SendCommandAsync("Page.enable", null, cancellationToken).ConfigureAwait(false);
                await client.SendCommandAsync("Runtime.enable", null, cancellationToken).ConfigureAwait(false);

                var initializedClient = client;
                client = null;
                return initializedClient;
            }
            catch
            {
                if (client is not null)
                {
                    try
                    {
                        await client.DisposeAsync().ConfigureAwait(false);
                    }
                    catch (Exception)
                    {
                    }
                }
                else if (socket is not null)
                {
                    try
                    {
                        socket.Dispose();
                    }
                    catch (Exception)
                    {
                    }
                }

                throw;
            }
        }

        public async Task CaptureScreenshotAsync(
            string htmlPath,
            string screenshotPath,
            RenderQaResolution resolution,
            CancellationToken cancellationToken)
        {
            await SendCommandAsync(
                "Emulation.setDeviceMetricsOverride",
                new Dictionary<string, object?>
                {
                    ["width"] = resolution.Width,
                    ["height"] = resolution.Height,
                    ["deviceScaleFactor"] = resolution.DeviceScaleFactor,
                    ["mobile"] = false,
                    ["screenWidth"] = resolution.Width,
                    ["screenHeight"] = resolution.Height
                },
                cancellationToken).ConfigureAwait(false);

            var loadEvent = PrepareNextLoadWaiter();
            await SendCommandAsync(
                "Page.navigate",
                new Dictionary<string, object?>
                {
                    ["url"] = new Uri(htmlPath).AbsoluteUri
                },
                cancellationToken).ConfigureAwait(false);
            await loadEvent.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken).ConfigureAwait(false);
            await WaitForDocumentReadinessAsync(cancellationToken).ConfigureAwait(false);
            var capture = await SendCommandAsync(
                "Page.captureScreenshot",
                new Dictionary<string, object?>
                {
                    ["format"] = "png",
                    ["fromSurface"] = true,
                    ["captureBeyondViewport"] = false
                },
                cancellationToken).ConfigureAwait(false);
            if (!capture.TryGetProperty("data", out var dataElement) ||
                dataElement.GetString() is not { Length: > 0 } base64)
            {
                throw new InvalidOperationException("CDP screenshot response omitted PNG bytes.");
            }

            var maxBase64Chars = checked((int)Math.Min(
                DesignConversionBrowserPngEvidenceInspector.MaxPngScreenshotBytes * 2,
                int.MaxValue));
            if (base64.Length > maxBase64Chars)
                throw new InvalidOperationException("CDP screenshot response exceeded the bounded PNG byte envelope.");

            var bytes = Convert.FromBase64String(base64);
            if (bytes.LongLength > DesignConversionBrowserPngEvidenceInspector.MaxPngScreenshotBytes)
                throw new InvalidOperationException("CDP screenshot PNG exceeded the bounded byte envelope.");

            await File.WriteAllBytesAsync(screenshotPath, bytes, cancellationToken).ConfigureAwait(false);
        }

        public ValueTask DisposeAsync()
        {
            Task disposeTask;
            lock (_disposeSync)
                disposeTask = _disposeTask ??= DisposeCoreAsync();
            return new ValueTask(disposeTask);
        }

        private async Task DisposeCoreAsync()
        {
            StopAcceptingAndCancelPendingOperations();
            var closeSucceeded = await TryCloseSocketAsync().ConfigureAwait(false);

            try
            {
                _disposeCts.Cancel();
            }
            catch (Exception)
            {
            }

            if (!closeSucceeded)
                TryAbortSocket();

            var receiveCompleted = await TryDrainTaskAsync(_receiveTask, ReceiveDrainTimeout).ConfigureAwait(false);
            if (!receiveCompleted)
                TryAbortSocket();

            try
            {
                _socket.Dispose();
            }
            catch (Exception)
            {
            }

            if (!receiveCompleted)
                _ = await TryDrainTaskAsync(_receiveTask, ReceiveDrainTimeout).ConfigureAwait(false);

            try
            {
                _disposeCts.Dispose();
            }
            catch (Exception)
            {
            }
        }

        private async Task<bool> TryCloseSocketAsync()
        {
            try
            {
                if (_socket.State != WebSocketState.Open)
                    return true;

                using var closeCts = new CancellationTokenSource(CloseHandshakeTimeout);
                var closeTask = _socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "done",
                    closeCts.Token);
                await closeTask.WaitAsync(CloseHandshakeTimeout).ConfigureAwait(false);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static async Task<bool> TryDrainTaskAsync(Task task, TimeSpan timeout)
        {
            try
            {
                await task.WaitAsync(timeout).ConfigureAwait(false);
                return true;
            }
            catch (Exception)
            {
                return task.IsCompleted;
            }
        }

        private void TryAbortSocket()
        {
            if (Interlocked.Exchange(ref _abortRequested, 1) != 0)
                return;

            try
            {
                _socket.Abort();
            }
            catch (Exception)
            {
            }
        }

        private void StopAcceptingAndCancelPendingOperations()
        {
            var (pending, loadEvent) = StopAcceptingAndDetachPendingOperations();
            foreach (var completion in pending)
                completion.TrySetCanceled();
            loadEvent?.TrySetCanceled();
        }

        private (TaskCompletionSource<JsonElement>[] Pending, TaskCompletionSource<bool>? LoadEvent)
            StopAcceptingAndDetachPendingOperations()
        {
            lock (_sync)
            {
                _acceptingCommands = false;
                var pending = _pending.Values.ToArray();
                _pending.Clear();
                var loadEvent = _loadEvent;
                _loadEvent = null;
                return (pending, loadEvent);
            }
        }

        private async Task WaitForDocumentReadinessAsync(CancellationToken cancellationToken)
        {
            const string expression = """
                new Promise(resolve => {
                  const settle = () => requestAnimationFrame(() => requestAnimationFrame(() => resolve(true)));
                  const images = Array.from(document.images || []).map(img => img.complete ? true : new Promise(done => {
                    img.addEventListener('load', done, { once: true });
                    img.addEventListener('error', done, { once: true });
                  }));
                  const fonts = document.fonts && document.fonts.ready ? document.fonts.ready.catch(() => true) : Promise.resolve(true);
                  Promise.race([
                    Promise.all([Promise.all(images), fonts]).then(settle, settle),
                    new Promise(done => setTimeout(() => { settle(); done(true); }, 1000))
                  ]).then(() => true, () => true);
                })
                """;
            await SendCommandAsync(
                "Runtime.evaluate",
                new Dictionary<string, object?>
                {
                    ["expression"] = expression,
                    ["awaitPromise"] = true,
                    ["returnByValue"] = true
                },
                cancellationToken).ConfigureAwait(false);
        }

        private TaskCompletionSource<bool> PrepareNextLoadWaiter()
        {
            var waiter = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            lock (_sync)
            {
                if (!_acceptingCommands)
                    throw CreateClientShutdownException();

                _loadEvent = waiter;
            }

            return waiter;
        }

        private async Task<JsonElement> SendCommandAsync(
            string method,
            IReadOnlyDictionary<string, object?>? parameters,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var id = Interlocked.Increment(ref _nextCommandId);
            var completion = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var commandCancellation = CreateCommandCancellationSource(cancellationToken);
            RegisterPendingCommand(id, completion);

            var payload = parameters is null
                ? JsonSerializer.Serialize(new Dictionary<string, object?>
                {
                    ["id"] = id,
                    ["method"] = method
                })
                : JsonSerializer.Serialize(new Dictionary<string, object?>
                {
                    ["id"] = id,
                    ["method"] = method,
                    ["params"] = parameters
                });
            var bytes = Encoding.UTF8.GetBytes(payload);
            try
            {
                await _socket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    endOfMessage: true,
                    commandCancellation.Token).ConfigureAwait(false);
                var response = await completion.Task.WaitAsync(_commandResponseTimeout, commandCancellation.Token)
                    .ConfigureAwait(false);
                if (response.TryGetProperty("error", out _))
                    throw new InvalidOperationException("CDP command failed.");

                return response.TryGetProperty("result", out var result)
                    ? result.Clone()
                    : JsonDocument.Parse("{}").RootElement.Clone();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(cancellationToken);
            }
            finally
            {
                UnregisterPendingCommand(id, completion);
            }
        }

        private CancellationTokenSource CreateCommandCancellationSource(CancellationToken cancellationToken)
        {
            try
            {
                return CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _disposeCts.Token);
            }
            catch (ObjectDisposedException)
            {
                throw CreateClientShutdownException();
            }
        }

        private void RegisterPendingCommand(int id, TaskCompletionSource<JsonElement> completion)
        {
            lock (_sync)
            {
                if (!_acceptingCommands)
                    throw CreateClientShutdownException();

                _pending[id] = completion;
            }
        }

        private void UnregisterPendingCommand(int id, TaskCompletionSource<JsonElement> completion)
        {
            lock (_sync)
            {
                if (_pending.TryGetValue(id, out var current) && ReferenceEquals(current, completion))
                    _pending.Remove(id);
            }
        }

        private static ObjectDisposedException CreateClientShutdownException()
            => new(
                nameof(ChromiumDevToolsPageClient),
                "CDP page client is no longer accepting commands.");

        private async Task ReceiveLoopAsync()
        {
            var buffer = new byte[ReceiveBufferSize];
            try
            {
                while (!_disposeCts.IsCancellationRequested && _socket.State is WebSocketState.Open or WebSocketState.CloseSent)
                {
                    using var message = new MemoryStream();
                    WebSocketReceiveResult received;
                    do
                    {
                        received = await _socket.ReceiveAsync(
                            new ArraySegment<byte>(buffer),
                            _disposeCts.Token).ConfigureAwait(false);
                        if (received.MessageType == WebSocketMessageType.Close)
                            return;
                        message.Write(buffer, 0, received.Count);
                        if (message.Length > MaxDevToolsMessageBytes)
                            throw new InvalidOperationException("CDP message exceeded the bounded JSON envelope.");
                    }
                    while (!received.EndOfMessage);

                    using var document = JsonDocument.Parse(message.ToArray());
                    var root = document.RootElement.Clone();
                    if (root.TryGetProperty("id", out var idElement) &&
                        idElement.TryGetInt32(out var id))
                    {
                        TaskCompletionSource<JsonElement>? completion;
                        lock (_sync)
                        {
                            _pending.TryGetValue(id, out completion);
                            _pending.Remove(id);
                        }

                        completion?.TrySetResult(root);
                        continue;
                    }

                    if (root.TryGetProperty("method", out var methodElement) &&
                        string.Equals(methodElement.GetString(), "Page.loadEventFired", StringComparison.Ordinal))
                    {
                        TaskCompletionSource<bool>? loadEvent;
                        lock (_sync)
                        {
                            loadEvent = _loadEvent;
                            _loadEvent = null;
                        }

                        loadEvent?.TrySetResult(true);
                    }
                }
            }
            catch (OperationCanceledException) when (_disposeCts.IsCancellationRequested)
            {
            }
            finally
            {
                StopAcceptingAndCancelPendingOperations();
            }
        }

        private static async Task<Uri> ResolvePageWebSocketUriAsync(
            string userDataPath,
            Func<bool> hasProcessExited,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            if (hasProcessExited is null)
                throw new ArgumentNullException(nameof(hasProcessExited));

            using var handler = new SocketsHttpHandler
            {
                UseProxy = false
            };
            using var http = new HttpClient(handler)
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
            var deadline = Stopwatch.StartNew();
            Exception? lastException = null;
            while (deadline.Elapsed < timeout)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    if (hasProcessExited())
                        throw new ChromiumDevToolsEndpointOwnershipException();

                    var identity = await TryReadDevToolsActivePortAsync(userDataPath, cancellationToken)
                        .ConfigureAwait(false);
                    if (identity is null)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    if (hasProcessExited())
                        throw new ChromiumDevToolsEndpointOwnershipException();

                    await VerifyBrowserEndpointOwnershipAsync(http, identity, cancellationToken)
                        .ConfigureAwait(false);

                    if (hasProcessExited())
                        throw new ChromiumDevToolsEndpointOwnershipException();

                    var created = await TryResolveCreatedPageWebSocketUriAsync(http, identity.Port, cancellationToken)
                        .ConfigureAwait(false);
                    if (created is not null)
                        return created;

                    var listed = await TryResolveListedPageWebSocketUriAsync(http, identity.Port, cancellationToken)
                        .ConfigureAwait(false);
                    if (listed is not null)
                        return listed;
                }
                catch (ChromiumDevToolsEndpointOwnershipException)
                {
                    throw;
                }
                catch (Exception ex) when (ex is HttpRequestException or JsonException or InvalidOperationException or TaskCanceledException)
                {
                    lastException = ex;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken).ConfigureAwait(false);
            }

            throw new InvalidOperationException(
                lastException is null
                    ? "Chromium DevTools endpoint did not become available."
                    : "Chromium DevTools endpoint did not become available after transient failures.");
        }

        private static async Task<DevToolsActivePortIdentity?> TryReadDevToolsActivePortAsync(
            string userDataPath,
            CancellationToken cancellationToken)
        {
            var activePortPath = BuildDevToolsActivePortPath(userDataPath);
            FileInfo fileInfo;
            try
            {
                fileInfo = new FileInfo(activePortPath);
                if (!fileInfo.Exists)
                    return null;
                if (fileInfo.Attributes.HasFlag(FileAttributes.ReparsePoint) ||
                    fileInfo.Attributes.HasFlag(FileAttributes.Directory))
                {
                    throw new ChromiumDevToolsEndpointOwnershipException();
                }
                if (fileInfo.Length == 0)
                    return null;
                if (fileInfo.Length > MaxDevToolsActivePortBytes)
                    throw new ChromiumDevToolsEndpointOwnershipException();
            }
            catch (Exception ex) when (ex is DirectoryNotFoundException or FileNotFoundException)
            {
                return null;
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
            {
                return null;
            }

            var length = checked((int)fileInfo.Length);
            var buffer = new byte[length];
            try
            {
                await using var stream = new FileStream(
                    activePortPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete,
                    bufferSize: length,
                    FileOptions.Asynchronous | FileOptions.SequentialScan);
                var offset = 0;
                while (offset < buffer.Length)
                {
                    var read = await stream.ReadAsync(buffer.AsMemory(offset), cancellationToken).ConfigureAwait(false);
                    if (read == 0)
                        break;
                    offset += read;
                }

                if (offset == 0)
                    return null;
                if (offset != buffer.Length)
                    Array.Resize(ref buffer, offset);
            }
            catch (Exception ex) when (ex is DirectoryNotFoundException or FileNotFoundException)
            {
                return null;
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
            {
                return null;
            }

            return ParseDevToolsActivePort(buffer);
        }

        private static DevToolsActivePortIdentity? ParseDevToolsActivePort(byte[] bytes)
        {
            if (bytes.Length == 0)
                return null;
            if (bytes.Length > MaxDevToolsActivePortBytes)
                throw new ChromiumDevToolsEndpointOwnershipException();
            if (Array.IndexOf(bytes, (byte)0) >= 0)
                throw new ChromiumDevToolsEndpointOwnershipException();

            var text = DecodeDevToolsActivePortText(bytes);
            if (text.Length == 0)
                return null;
            if (text[0] == '\uFEFF')
                text = text[1..];

            text = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
            var lines = text.Split('\n');
            if (lines.Length < 2 || string.IsNullOrEmpty(lines[0]) || string.IsNullOrEmpty(lines[1]))
                return null;

            for (var index = 2; index < lines.Length; index++)
            {
                if (lines[index].Length != 0)
                    throw new ChromiumDevToolsEndpointOwnershipException();
            }

            if (!int.TryParse(lines[0], NumberStyles.None, CultureInfo.InvariantCulture, out var port) ||
                port <= 0 ||
                port > 65535)
            {
                throw new ChromiumDevToolsEndpointOwnershipException();
            }

            var browserPath = lines[1];
            if (!IsStrictDevToolsPath(browserPath, DevToolsBrowserPathPrefix))
                throw new ChromiumDevToolsEndpointOwnershipException();

            return new DevToolsActivePortIdentity(port, browserPath);
        }

        private static string DecodeDevToolsActivePortText(byte[] bytes)
        {
            try
            {
                return StrictDevToolsActivePortUtf8.GetString(bytes);
            }
            catch (DecoderFallbackException)
            {
                throw new ChromiumDevToolsEndpointOwnershipException();
            }
        }

        private static async Task VerifyBrowserEndpointOwnershipAsync(
            HttpClient http,
            DevToolsActivePortIdentity identity,
            CancellationToken cancellationToken)
        {
            var versionUri = $"http://127.0.0.1:{identity.Port.ToString(CultureInfo.InvariantCulture)}/json/version";
            using var response = await http.GetAsync(versionUri, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw new ChromiumDevToolsEndpointOwnershipException();

            var body = await ReadBoundedHttpContentAsync(response.Content, cancellationToken).ConfigureAwait(false);
            using var document = JsonDocument.Parse(body);
            if (!document.RootElement.TryGetProperty("webSocketDebuggerUrl", out var browserSocketElement) ||
                browserSocketElement.GetString() is not { Length: > 0 } browserSocket ||
                !IsExactBrowserWebSocket(browserSocket, identity))
            {
                throw new ChromiumDevToolsEndpointOwnershipException();
            }
        }

        private static async Task<Uri?> TryResolveCreatedPageWebSocketUriAsync(
            HttpClient http,
            int port,
            CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Put,
                $"http://127.0.0.1:{port.ToString(CultureInfo.InvariantCulture)}/json/new?about:blank");
            using var response = await http.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return null;

            var body = await ReadBoundedHttpContentAsync(response.Content, cancellationToken).ConfigureAwait(false);
            using var document = JsonDocument.Parse(body);
            return ExtractPageWebSocketUri(document.RootElement, port);
        }

        private static async Task<Uri?> TryResolveListedPageWebSocketUriAsync(
            HttpClient http,
            int port,
            CancellationToken cancellationToken)
        {
            using var response = await http.GetAsync(
                $"http://127.0.0.1:{port.ToString(CultureInfo.InvariantCulture)}/json/list",
                cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return null;

            var body = await ReadBoundedHttpContentAsync(response.Content, cancellationToken).ConfigureAwait(false);
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
                return null;

            foreach (var item in document.RootElement.EnumerateArray())
            {
                if (item.TryGetProperty("type", out var type) &&
                    !string.Equals(type.GetString(), "page", StringComparison.Ordinal))
                {
                    continue;
                }

                var uri = ExtractPageWebSocketUri(item, port);
                if (uri is not null)
                    return uri;
            }

            return null;
        }

        private static async Task<string> ReadBoundedHttpContentAsync(
            HttpContent content,
            CancellationToken cancellationToken)
        {
            await using var stream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var memory = new MemoryStream();
            var buffer = new byte[4096];
            while (true)
            {
                var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false);
                if (read == 0)
                    break;
                if (memory.Length + read > MaxDevToolsJsonBytes)
                    throw new ChromiumDevToolsEndpointOwnershipException();
                memory.Write(buffer, 0, read);
            }

            return Encoding.UTF8.GetString(memory.ToArray());
        }

        private static bool IsExactBrowserWebSocket(string raw, DevToolsActivePortIdentity identity)
            => Uri.TryCreate(raw, UriKind.Absolute, out var uri) &&
                uri.Scheme.Equals("ws", StringComparison.Ordinal) &&
                uri.Host.Equals("127.0.0.1", StringComparison.Ordinal) &&
                uri.Port == identity.Port &&
                string.Equals(uri.AbsolutePath, identity.BrowserPath, StringComparison.Ordinal) &&
                string.IsNullOrEmpty(uri.Query) &&
                string.IsNullOrEmpty(uri.Fragment) &&
                string.Equals(
                    raw,
                    $"ws://127.0.0.1:{identity.Port.ToString(CultureInfo.InvariantCulture)}{identity.BrowserPath}",
                    StringComparison.Ordinal);

        private static Uri? ExtractPageWebSocketUri(JsonElement element, int expectedPort)
            => element.TryGetProperty("webSocketDebuggerUrl", out var webSocketElement) &&
                webSocketElement.GetString() is { Length: > 0 } raw &&
                Uri.TryCreate(raw, UriKind.Absolute, out var uri) &&
                uri.Scheme.Equals("ws", StringComparison.Ordinal) &&
                uri.Host.Equals("127.0.0.1", StringComparison.Ordinal) &&
                uri.Port == expectedPort &&
                IsStrictDevToolsPath(uri.AbsolutePath, DevToolsPagePathPrefix) &&
                string.IsNullOrEmpty(uri.Query) &&
                string.IsNullOrEmpty(uri.Fragment)
                ? uri
                : null;

        private static bool IsStrictDevToolsPath(string path, string prefix)
        {
            if (!path.StartsWith(prefix, StringComparison.Ordinal) || path.Length == prefix.Length)
                return false;

            for (var index = prefix.Length; index < path.Length; index++)
            {
                var ch = path[index];
                if (!(IsAsciiLetterOrDigit(ch) || ch is '-' or '_' or '.'))
                    return false;
            }

            return true;
        }

        private static bool IsAsciiLetterOrDigit(char ch)
            => ch is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9';

        private sealed record DevToolsActivePortIdentity(int Port, string BrowserPath);
    }

    internal static OperationResult<LocalChromiumBrowserProcessRunResult> ClassifyCompletedBrowserProcessFailure(
        int exitCode,
        bool screenshotExists,
        string stderr,
        string stdout)
    {
        var processSummary = SummarizeProcessText(stderr ?? string.Empty, stdout ?? string.Empty);
        if (!screenshotExists && IsRecognizedBrowserRuntimeCrashExitCode(exitCode))
        {
            return OperationResult<LocalChromiumBrowserProcessRunResult>.Fail(
                $"Chromium runtime exited before producing a screenshot with a recognized process crash code. exit_code={exitCode.ToString(CultureInfo.InvariantCulture)}. {processSummary}",
                RuntimeCrashedBeforeScreenshotCode);
        }

        return OperationResult<LocalChromiumBrowserProcessRunResult>.Fail(
            $"Browser screenshot process failed with exit code {exitCode.ToString(CultureInfo.InvariantCulture)}. {processSummary}",
            ScreenshotFailedCode);
    }

    private static bool IsRecognizedBrowserRuntimeCrashExitCode(int exitCode)
        => exitCode is
            unchecked((int)0x80000003) or // STATUS_BREAKPOINT, observed when local Chromium is installed but cannot launch in this runtime.
            unchecked((int)0xC0000005) or // STATUS_ACCESS_VIOLATION
            unchecked((int)0xC000001D) or // STATUS_ILLEGAL_INSTRUCTION
            unchecked((int)0xC00000FD) or // STATUS_STACK_OVERFLOW
            unchecked((int)0xC0000135) or // DLL/dependency load failure
            unchecked((int)0xC0000142) or // DLL initialization failure
            unchecked((int)0xC0000409);   // stack buffer overrun / fail-fast

    private static string BuildStrictOfflineCaptureDocument(string? documentHtml)
        => string.Concat(
            "<!doctype html><html><head><meta charset=\"utf-8\">",
            StrictOfflineDocumentCspMeta,
            "<meta name=\"viewport\" content=\"width=device-width,initial-scale=1\">",
            "</head><body>",
            documentHtml ?? string.Empty,
            "</body></html>");

    private static bool TryStartBrowserProcess(Process process, out string failureMessage)
    {
        try
        {
            if (process.Start())
            {
                failureMessage = string.Empty;
                return true;
            }

            failureMessage = "Process.Start returned false.";
            return false;
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
        {
            failureMessage = ex.Message;
            return false;
        }
    }

    private static LocalChromiumBrowserRssMonitor StartBrowserRssMonitor(
        Process process,
        LocalChromiumBrowserRssPolicy rssPolicy,
        CancellationToken cancellationToken)
    {
        var guard = new LocalChromiumBrowserRssGuard(
            rssPolicy,
            sampleRss: () =>
            {
                process.Refresh();
                return process.WorkingSet64;
            },
            hasExited: () => process.HasExited,
            delayAsync: static (delay, token) => Task.Delay(delay, token),
            killProcessTree: () => TryKillBrowserProcess(process));

        return guard.Start(cancellationToken);
    }

    internal static void TryKillBrowserProcess(Process? process)
    {
        if (process is null)
            return;

        try
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit(2000);
        }
        catch (Exception ex) when (ex is InvalidOperationException or Win32Exception or NotSupportedException)
        {
        }
    }

    internal interface IWorkspaceCleaner
    {
        OperationResult<bool> CleanupSensitiveBrowserCaptureWorkspace(
            string captureRoot,
            string htmlPath,
            string userDataPath);

        OperationResult<bool> CleanupSensitiveBrowserCaptureWorkspace(
            string captureRoot,
            IReadOnlyList<string> htmlPaths,
            string userDataPath);

        OperationResult<bool> CleanupFailedBrowserCaptureWorkspace(
            string baseRoot,
            string captureRoot);
    }

    internal interface IWorkspaceFileSystem
    {
        bool FileExists(string path);

        bool DirectoryExists(string path);

        FileAttributes GetDirectoryAttributes(string path);

        void DeleteFile(string path);

        void DeleteDirectory(string path, bool recursive);
    }

    internal sealed class RetryingWorkspaceCleaner : IWorkspaceCleaner
    {
        internal const int StableAbsenceObservationCount = 3;
        internal const int RetryDelayMilliseconds = 50;
        internal const int MaxCleanupDrainDelayMilliseconds = 5000;
        internal const int MaxCleanupDrainDelaySteps =
            MaxCleanupDrainDelayMilliseconds / RetryDelayMilliseconds;
        internal static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(RetryDelayMilliseconds);

        internal static readonly RetryingWorkspaceCleaner Instance = new(
            DefaultWorkspaceFileSystem.Instance,
            static delay => Thread.Sleep(delay));

        private readonly IWorkspaceFileSystem _fileSystem;
        private readonly Action<TimeSpan> _delay;

        internal RetryingWorkspaceCleaner(
            IWorkspaceFileSystem fileSystem,
            Action<TimeSpan>? delay = null)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _delay = delay ?? (_ => { });
        }

        public OperationResult<bool> CleanupSensitiveBrowserCaptureWorkspace(
            string captureRoot,
            string htmlPath,
            string userDataPath)
            => CleanupSensitiveBrowserCaptureWorkspace(
                captureRoot,
                [htmlPath],
                userDataPath);

        public OperationResult<bool> CleanupSensitiveBrowserCaptureWorkspace(
            string captureRoot,
            IReadOnlyList<string> htmlPaths,
            string userDataPath)
        {
            string root;
            List<string> htmls;
            string profile;
            try
            {
                root = NormalizeBrowserCapturePath(captureRoot);
                htmls = (htmlPaths ?? Array.Empty<string>())
                    .Select(NormalizeBrowserCapturePath)
                    .ToList();
                profile = NormalizeBrowserCapturePath(userDataPath);
            }
            catch (Exception)
            {
                return SensitiveCleanupFailure();
            }

            if (!IsGuidCaptureRoot(root) ||
                htmls.Count == 0 ||
                htmls.Any(html => !IsBatchHtmlPath(root, html)) ||
                !SameBrowserCapturePath(profile, Path.Combine(root, "profile")))
            {
                return SensitiveCleanupFailure();
            }

            return ExecuteWithTransientCleanupRetry(
                cleanup: () =>
                {
                    foreach (var html in htmls)
                    {
                        if (_fileSystem.FileExists(html))
                            _fileSystem.DeleteFile(html);
                    }

                    if (_fileSystem.DirectoryExists(profile))
                        _fileSystem.DeleteDirectory(profile, recursive: true);

                    return InspectSensitiveWorkspace(htmls, profile);
                },
                observe: () => InspectSensitiveWorkspace(htmls, profile));
        }

        public OperationResult<bool> CleanupFailedBrowserCaptureWorkspace(
            string baseRoot,
            string captureRoot)
        {
            var root = ValidateBrowserCaptureWorkspaceRoot(baseRoot, captureRoot);
            if (!root.IsSuccess)
                return root.MapError<bool>();

            return ExecuteWithTransientCleanupRetry(
                cleanup: () =>
                {
                    var state = InspectFailedWorkspaceRoot(root.Value!);
                    if (state != CleanupObservation.Present)
                        return state;

                    _fileSystem.DeleteDirectory(root.Value!, recursive: true);
                    return InspectFailedWorkspaceRoot(root.Value!);
                },
                observe: () => InspectFailedWorkspaceRoot(root.Value!));
        }

        private CleanupObservation InspectSensitiveWorkspace(IReadOnlyList<string> htmlPaths, string profilePath)
            => htmlPaths.Any(_fileSystem.FileExists) || _fileSystem.DirectoryExists(profilePath)
                ? CleanupObservation.Present
                : CleanupObservation.Absent;

        private static bool IsBatchHtmlPath(string root, string htmlPath)
        {
            var parent = Path.GetDirectoryName(htmlPath);
            if (string.IsNullOrWhiteSpace(parent) || !SameBrowserCapturePath(parent, root))
                return false;

            var fileName = Path.GetFileName(htmlPath);
            if (string.Equals(fileName, "index.html", StringComparison.Ordinal))
                return true;
            if (!fileName.StartsWith("index-", StringComparison.Ordinal) ||
                !fileName.EndsWith(".html", StringComparison.Ordinal))
            {
                return false;
            }

            var ordinal = fileName.Substring(
                "index-".Length,
                fileName.Length - "index-".Length - ".html".Length);
            return ordinal.Length == 4 && ordinal.All(char.IsDigit);
        }

        private CleanupObservation InspectFailedWorkspaceRoot(string captureRoot)
        {
            if (!_fileSystem.DirectoryExists(captureRoot))
                return CleanupObservation.Absent;

            return (_fileSystem.GetDirectoryAttributes(captureRoot) & FileAttributes.ReparsePoint) != 0
                ? CleanupObservation.PermanentFailure
                : CleanupObservation.Present;
        }

        private OperationResult<bool> ExecuteWithTransientCleanupRetry(
            Func<CleanupObservation> cleanup,
            Func<CleanupObservation> observe)
        {
            var remainingDelaySteps = MaxCleanupDrainDelaySteps;

            while (true)
            {
                CleanupObservation cleanupState;
                try
                {
                    cleanupState = cleanup();
                }
                catch (Exception ex) when (IsTransientCleanupException(ex))
                {
                    if (!TryDelay(ref remainingDelaySteps))
                        return SensitiveCleanupFailure();

                    continue;
                }
                catch (Exception)
                {
                    return SensitiveCleanupFailure();
                }

                if (cleanupState == CleanupObservation.PermanentFailure)
                    return SensitiveCleanupFailure();

                if (cleanupState == CleanupObservation.Present)
                {
                    if (!TryDelay(ref remainingDelaySteps))
                        return SensitiveCleanupFailure();

                    continue;
                }

                var stableAbsence = ObserveStableAbsence(observe, ref remainingDelaySteps);
                if (stableAbsence == CleanupObservation.Absent)
                    return OperationResult<bool>.Ok(true);
                if (stableAbsence == CleanupObservation.PermanentFailure)
                    return SensitiveCleanupFailure();
            }
        }

        private CleanupObservation ObserveStableAbsence(
            Func<CleanupObservation> observe,
            ref int remainingDelaySteps)
        {
            for (var observation = 0; observation < StableAbsenceObservationCount; observation++)
            {
                if (!TryDelay(ref remainingDelaySteps))
                    return CleanupObservation.PermanentFailure;

                try
                {
                    var state = observe();
                    if (state != CleanupObservation.Absent)
                        return state;
                }
                catch (Exception ex) when (IsTransientCleanupException(ex))
                {
                    return CleanupObservation.Present;
                }
                catch (Exception)
                {
                    return CleanupObservation.PermanentFailure;
                }
            }

            return CleanupObservation.Absent;
        }

        private bool TryDelay(ref int remainingDelaySteps)
        {
            if (remainingDelaySteps <= 0)
                return false;

            remainingDelaySteps--;

            try
            {
                _delay(RetryDelay);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool IsTransientCleanupException(Exception ex)
            => ex is IOException or UnauthorizedAccessException;

        private enum CleanupObservation
        {
            Absent,
            Present,
            PermanentFailure
        }
    }

    private sealed class DefaultWorkspaceFileSystem : IWorkspaceFileSystem
    {
        internal static readonly DefaultWorkspaceFileSystem Instance = new();

        private DefaultWorkspaceFileSystem()
        {
        }

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
            => Directory.Delete(path, recursive);
    }

    private static OperationResult<bool> CleanupSensitiveBrowserCaptureWorkspace(
        string captureRoot,
        string htmlPath,
        string userDataPath)
        => RetryingWorkspaceCleaner.Instance.CleanupSensitiveBrowserCaptureWorkspace(
            captureRoot,
            htmlPath,
            userDataPath);

    private static OperationResult<bool> CleanupFailedBrowserCaptureWorkspace(
        string baseRoot,
        string captureRoot)
        => RetryingWorkspaceCleaner.Instance.CleanupFailedBrowserCaptureWorkspace(
            baseRoot,
            captureRoot);

    private static OperationResult<string> ValidateBrowserCaptureWorkspaceRoot(
        string baseRoot,
        string captureRoot)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(baseRoot) || string.IsNullOrWhiteSpace(captureRoot))
                return SensitiveCleanupFailureForPath();

            var normalizedBaseRoot = NormalizeBrowserCapturePath(baseRoot);
            var normalizedCaptureRoot = NormalizeBrowserCapturePath(captureRoot);
            var parent = Path.GetDirectoryName(normalizedCaptureRoot);
            if (string.IsNullOrWhiteSpace(parent) ||
                !SameBrowserCapturePath(parent, normalizedBaseRoot) ||
                !IsGuidCaptureRoot(normalizedCaptureRoot))
            {
                return SensitiveCleanupFailureForPath();
            }

            return OperationResult<string>.Ok(normalizedCaptureRoot);
        }
        catch (Exception)
        {
            return SensitiveCleanupFailureForPath();
        }
    }

    private static OperationResult<bool> SensitiveCleanupFailure()
        => OperationResult<bool>.Fail(
            "Browser capture sensitive workspace cleanup failed.",
            SensitiveCleanupFailedCode);

    private static OperationResult<string> SensitiveCleanupFailureForPath()
        => OperationResult<string>.Fail(
            "Browser capture sensitive workspace cleanup failed.",
            SensitiveCleanupFailedCode);

    private static bool IsGuidCaptureRoot(string captureRoot)
    {
        var directoryName = Path.GetFileName(captureRoot);
        return directoryName.Length == 32 && Guid.TryParseExact(directoryName, "N", out _);
    }

    private static string NormalizeBrowserCapturePath(string path)
        => Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));

    private static bool SameBrowserCapturePath(string left, string right)
        => string.Equals(
            NormalizeBrowserCapturePath(left),
            NormalizeBrowserCapturePath(right),
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    private static ProcessStartInfo BuildStartInfo(
        string executablePath,
        string htmlPath,
        string screenshotPath,
        string userDataPath,
        RenderQaResolution resolution)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add("--headless=new");
        startInfo.ArgumentList.Add("--disable-gpu");
        startInfo.ArgumentList.Add("--disable-dev-shm-usage");
        AddLinuxRootChromiumSandboxFlag(startInfo);
        startInfo.ArgumentList.Add("--disable-background-networking");
        startInfo.ArgumentList.Add("--disable-component-update");
        startInfo.ArgumentList.Add("--disable-extensions");
        startInfo.ArgumentList.Add("--disable-sync");
        startInfo.ArgumentList.Add("--metrics-recording-only");
        startInfo.ArgumentList.Add("--no-pings");
        startInfo.ArgumentList.Add("--safebrowsing-disable-auto-update");
        startInfo.ArgumentList.Add("--disable-features=Translate,OptimizationHints,MediaRouter");
        startInfo.ArgumentList.Add("--host-resolver-rules=MAP * ~NOTFOUND");
        startInfo.ArgumentList.Add("--no-first-run");
        startInfo.ArgumentList.Add("--no-default-browser-check");
        startInfo.ArgumentList.Add("--hide-scrollbars");
        startInfo.ArgumentList.Add($"--user-data-dir={userDataPath}");
        startInfo.ArgumentList.Add($"--window-size={resolution.Width},{resolution.Height}");
        startInfo.ArgumentList.Add($"--force-device-scale-factor={resolution.DeviceScaleFactor.ToString("R", CultureInfo.InvariantCulture)}");
        startInfo.ArgumentList.Add($"--screenshot={screenshotPath}");
        startInfo.ArgumentList.Add(new Uri(htmlPath).AbsoluteUri);
        return startInfo;
    }

    private static ProcessStartInfo BuildBatchStartInfo(
        string executablePath,
        string userDataPath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add("--headless=new");
        startInfo.ArgumentList.Add("--disable-gpu");
        startInfo.ArgumentList.Add("--disable-dev-shm-usage");
        AddLinuxRootChromiumSandboxFlag(startInfo);
        startInfo.ArgumentList.Add("--disable-background-networking");
        startInfo.ArgumentList.Add("--disable-component-update");
        startInfo.ArgumentList.Add("--disable-extensions");
        startInfo.ArgumentList.Add("--disable-sync");
        startInfo.ArgumentList.Add("--metrics-recording-only");
        startInfo.ArgumentList.Add("--no-pings");
        startInfo.ArgumentList.Add("--safebrowsing-disable-auto-update");
        startInfo.ArgumentList.Add("--disable-features=Translate,OptimizationHints,MediaRouter");
        startInfo.ArgumentList.Add("--host-resolver-rules=MAP * ~NOTFOUND");
        startInfo.ArgumentList.Add("--no-first-run");
        startInfo.ArgumentList.Add("--no-default-browser-check");
        startInfo.ArgumentList.Add("--hide-scrollbars");
        startInfo.ArgumentList.Add($"--user-data-dir={userDataPath}");
        startInfo.ArgumentList.Add("--remote-debugging-address=127.0.0.1");
        startInfo.ArgumentList.Add("--remote-debugging-port=0");
        startInfo.ArgumentList.Add("about:blank");
        return startInfo;
    }

    private static void AddLinuxRootChromiumSandboxFlag(ProcessStartInfo startInfo)
    {
        // WSL/контейнерный root-runner иначе падает до PNG: Chromium сам требует
        // этот флаг, а локальная сеть всё равно закрыта CSP и host-resolver.
        if (IsLinuxRootUser())
            startInfo.ArgumentList.Add("--no-sandbox");
    }

    private static bool IsLinuxRootUser()
        => OperatingSystem.IsLinux() &&
           string.Equals(Environment.UserName, "root", StringComparison.Ordinal);

    private static string SummarizeProcessText(string stderr, string stdout)
        => string.Join(
            " ",
            "stderr_sha256=" + DeterministicDesignConversionRenderProvider.Sha256Text(stderr),
            "stderr_chars=" + stderr.Length.ToString(CultureInfo.InvariantCulture),
            "stdout_sha256=" + DeterministicDesignConversionRenderProvider.Sha256Text(stdout),
            "stdout_chars=" + stdout.Length.ToString(CultureInfo.InvariantCulture));
}

internal sealed class LocalChromiumBrowserCaptureStorageBudget
{
    internal const string StorageQuotaExceededCode = "BROWSER_CAPTURE_STORAGE_QUOTA_EXCEEDED";
    internal const string StorageQuotaUnsafeStateCode = "BROWSER_CAPTURE_STORAGE_UNSAFE_STATE";
    internal const string StorageQuotaInspectionFailedCode = "BROWSER_CAPTURE_STORAGE_INSPECTION_FAILED";

    private readonly object _sync = new();
    private readonly string _baseRoot;
    private long _activeReservedBytes;

    public LocalChromiumBrowserCaptureStorageBudget(
        string baseRoot,
        long maxRetainedCaptureBytes,
        long reservationBytes)
    {
        if (string.IsNullOrWhiteSpace(baseRoot))
            throw new ArgumentException("Browser capture storage root is required.", nameof(baseRoot));
        if (maxRetainedCaptureBytes < 0)
            throw new ArgumentOutOfRangeException(nameof(maxRetainedCaptureBytes), "Browser capture storage quota cannot be negative.");
        if (reservationBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(reservationBytes), "Browser capture reservation bytes must be positive.");

        _baseRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(baseRoot));
        MaxRetainedCaptureBytes = maxRetainedCaptureBytes;
        ReservationBytes = reservationBytes;
    }

    public long MaxRetainedCaptureBytes { get; }
    public long ReservationBytes { get; }

    public OperationResult<LocalChromiumBrowserCaptureStorageReservation> TryReserve()
    {
        lock (_sync)
        {
            var retained = MeasureRetainedCaptureBytes();
            if (!retained.IsSuccess)
                return retained.MapError<LocalChromiumBrowserCaptureStorageReservation>();

            var consumed = SaturatingAdd(retained.Value, _activeReservedBytes);
            var projected = SaturatingAdd(consumed, ReservationBytes);
            if (projected > MaxRetainedCaptureBytes)
            {
                return OperationResult<LocalChromiumBrowserCaptureStorageReservation>.Fail(
                    string.Join(
                        " ",
                        "Browser capture retained PNG storage quota exceeded.",
                        "retained_bytes=" + retained.Value.ToString(CultureInfo.InvariantCulture),
                        "active_reserved_bytes=" + _activeReservedBytes.ToString(CultureInfo.InvariantCulture),
                        "reservation_bytes=" + ReservationBytes.ToString(CultureInfo.InvariantCulture),
                        "quota_bytes=" + MaxRetainedCaptureBytes.ToString(CultureInfo.InvariantCulture)),
                    StorageQuotaExceededCode);
            }

            _activeReservedBytes = SaturatingAdd(_activeReservedBytes, ReservationBytes);
            return OperationResult<LocalChromiumBrowserCaptureStorageReservation>.Ok(
                new LocalChromiumBrowserCaptureStorageReservation(this, ReservationBytes));
        }
    }

    internal OperationResult<long> MeasureRetainedCaptureBytesForTesting()
        => MeasureRetainedCaptureBytes();

    private void Release(long reservationBytes)
    {
        lock (_sync)
        {
            _activeReservedBytes = reservationBytes >= _activeReservedBytes
                ? 0
                : _activeReservedBytes - reservationBytes;
        }
    }

    private OperationResult<long> MeasureRetainedCaptureBytes()
    {
        try
        {
            if (!Directory.Exists(_baseRoot))
                return OperationResult<long>.Ok(0);

            var baseInfo = new DirectoryInfo(_baseRoot);
            baseInfo.Refresh();
            if ((baseInfo.Attributes & FileAttributes.ReparsePoint) != 0)
                return UnsafeStorageState();

            var retainedBytes = 0L;
            foreach (var child in baseInfo.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
            {
                if (!IsGuidCaptureDirectoryName(child.Name))
                    continue;

                child.Refresh();
                if ((child.Attributes & FileAttributes.ReparsePoint) != 0)
                    return UnsafeStorageState();

                var pngFiles = child
                    .EnumerateFiles("*", SearchOption.TopDirectoryOnly)
                    .Where(file => string.Equals(file.Extension, ".png", StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                foreach (var screenshot in pngFiles)
                {
                    screenshot.Refresh();
                    if (!screenshot.Exists)
                        continue;
                    if ((screenshot.Attributes & FileAttributes.ReparsePoint) != 0)
                        return UnsafeStorageState();

                    retainedBytes = SaturatingAdd(retainedBytes, Math.Max(0, screenshot.Length));
                }
            }

            return OperationResult<long>.Ok(retainedBytes);
        }
        catch (Exception)
        {
            return OperationResult<long>.Fail(
                "Browser capture retained PNG storage could not be inspected safely.",
                StorageQuotaInspectionFailedCode);
        }
    }

    private static OperationResult<long> UnsafeStorageState()
        => OperationResult<long>.Fail(
            "Browser capture retained PNG storage contains unsafe state.",
            StorageQuotaUnsafeStateCode);

    private static bool IsGuidCaptureDirectoryName(string name)
        => name.Length == 32 && Guid.TryParseExact(name, "N", out _);

    private static long SaturatingAdd(long left, long right)
    {
        if (left < 0 || right < 0)
            return long.MaxValue;
        return left > long.MaxValue - right
            ? long.MaxValue
            : left + right;
    }

    internal sealed class LocalChromiumBrowserCaptureStorageReservation : IDisposable
    {
        private LocalChromiumBrowserCaptureStorageBudget? _owner;

        internal LocalChromiumBrowserCaptureStorageReservation(
            LocalChromiumBrowserCaptureStorageBudget owner,
            long reservedBytes)
        {
            _owner = owner;
            ReservedBytes = reservedBytes;
        }

        public long ReservedBytes { get; }

        public void Dispose()
        {
            var owner = Interlocked.Exchange(ref _owner, null);
            owner?.Release(ReservedBytes);
        }
    }
}

internal sealed class LocalChromiumBrowserCaptureConcurrencyGate
{
    private readonly SemaphoreSlim _slots;

    public LocalChromiumBrowserCaptureConcurrencyGate(int maxConcurrency)
    {
        if (maxConcurrency < 1)
            throw new ArgumentOutOfRangeException(nameof(maxConcurrency), "Browser capture concurrency must be at least one.");

        MaxConcurrency = maxConcurrency;
        _slots = new SemaphoreSlim(maxConcurrency, maxConcurrency);
    }

    public int MaxConcurrency { get; }

    public async Task<T> ExecuteAsync<T>(
        Func<Task<T>> work,
        Func<T> waitCanceledResult,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(work);
        ArgumentNullException.ThrowIfNull(waitCanceledResult);

        try
        {
            await _slots.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return waitCanceledResult();
        }

        try
        {
            if (cancellationToken.IsCancellationRequested)
                return waitCanceledResult();

            return await work();
        }
        finally
        {
            _slots.Release();
        }
    }
}
