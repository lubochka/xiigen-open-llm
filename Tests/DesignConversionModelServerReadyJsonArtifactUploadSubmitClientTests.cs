// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Buffers.Binary;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;
using ModelServer.DesignConversion;

public sealed class DesignConversionModelServerReadyJsonArtifactUploadSubmitClientTests
{
    private const string BaseUrl = "http://127.0.0.1:54321";
    private const string RouteToken = "ready-json-upload-route-token-123";
    private const string CallerIdempotencyKey = "ready-json-upload-caller-submit-key";

    [Fact]
    public async Task ReadyJsonSubmitClient_UploadsLocalPngArtifactsBeforeEvaluate()
    {
        const string requestId = "ready-json-upload-before-evaluate";
        const string privateMarker = "PRIVATE_READY_JSON_UPLOAD_BEFORE_EVALUATE_SECRET";
        using var temp = TemporaryDirectory.Create();
        var screenshot = await WritePngFixtureAsync(temp.Path, "source-screenshot.png");
        var render = await WritePngFixtureAsync(temp.Path, "expected-render.png");
        var package = CreatePackage(requestId, privateMarker, screenshot, render);
        var readyJson = ReadyJson(uploadConfigured: true, uploadMaxBytes: TinyPngBytes.Length + 16);

        var first = await SubmitAsync(readyJson, package);
        var repeat = await SubmitAsync(readyJson, package);

        Assert.True(first.Result.IsSuccess, first.Result.ErrorMessage);
        Assert.True(repeat.Result.IsSuccess, repeat.Result.ErrorMessage);
        var submitted = first.Result.Value!;
        Assert.Equal("success", submitted.Envelope.EnvelopeKind);
        Assert.Equal(requestId, submitted.Envelope.ParsedRequestId);

        Assert.Equal(
            [
                DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath,
                DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath,
                DesignConversionModelServerEndpointRegistry.EndpointPath
            ],
            first.Handler.Paths);
        Assert.Equal(3, first.Handler.SendCount);

        var uploadRequests = first.Handler.UploadRequests;
        Assert.Equal(2, uploadRequests.Count);
        Assert.All(uploadRequests, request =>
        {
            Assert.Equal("POST", request.Method);
            Assert.Equal("image/png", request.ContentType);
            Assert.Equal(TinyPngBytes, request.BodyBytes);
            Assert.Equal(
                "Bearer " + RouteToken,
                AssertSingleHeader(request, DesignConversionModelServerRouteAdmission.AuthorizationHeaderName));
            var uploadIdempotencyKey = AssertSingleHeader(
                request,
                DesignConversionModelServerRouteAdmission.IdempotencyHeaderName);
            Assert.False(uploadIdempotencyKey.Contains(privateMarker, StringComparison.Ordinal));
            Assert.True(uploadIdempotencyKey.Length <= 128);
        });
        Assert.Equal(
            new[] { package.Screenshots![0].ArtifactId, package.DesignRenders![0].ArtifactId },
            uploadRequests.Select(request => request.Headers[DesignConversionModelServerArtifactUploadHttpHandler.ArtifactIdHeaderName]));
        Assert.Equal(
            new[] { "screenshot", "design-render" },
            uploadRequests.Select(request => request.Headers[DesignConversionModelServerArtifactUploadHttpHandler.ArtifactKindHeaderName]));
        Assert.Equal(
            new[] { screenshot.Sha256, render.Sha256 },
            uploadRequests.Select(request => request.Headers[DesignConversionModelServerArtifactUploadHttpHandler.ExpectedSha256HeaderName]));

        var uploadIdempotencyKeys = uploadRequests
            .Select(request => request.Headers[DesignConversionModelServerRouteAdmission.IdempotencyHeaderName])
            .ToArray();
        Assert.Equal(uploadIdempotencyKeys.Length, uploadIdempotencyKeys.Distinct(StringComparer.Ordinal).Count());
        Assert.DoesNotContain(CallerIdempotencyKey, uploadIdempotencyKeys);
        Assert.Equal(
            uploadIdempotencyKeys,
            repeat.Handler.UploadRequests
                .Select(request => request.Headers[DesignConversionModelServerRouteAdmission.IdempotencyHeaderName])
                .ToArray());

        var evaluate = first.Handler.EvaluateRequests.Single();
        Assert.Equal(
            "Bearer " + RouteToken,
            AssertSingleHeader(evaluate, DesignConversionModelServerRouteAdmission.AuthorizationHeaderName));
        Assert.Equal(
            CallerIdempotencyKey,
            AssertSingleHeader(evaluate, DesignConversionModelServerRouteAdmission.IdempotencyHeaderName));
        Assert.DoesNotContain(screenshot.Path, evaluate.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(render.Path, evaluate.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(new Uri(screenshot.Path).AbsoluteUri, evaluate.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(new Uri(render.Path).AbsoluteUri, evaluate.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Convert.ToBase64String(TinyPngBytes), evaluate.BodyJson, StringComparison.Ordinal);
        Assert.Contains(screenshot.Sha256, evaluate.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(render.Sha256, evaluate.BodyJson, StringComparison.OrdinalIgnoreCase);
        using var evaluateJson = JsonDocument.Parse(evaluate.BodyJson!);
        var evaluatedScreenshotUri = evaluateJson.RootElement
            .GetProperty("screenshots")[0]
            .GetProperty("uri")
            .GetString();
        var evaluatedRenderUri = evaluateJson.RootElement
            .GetProperty("designRenders")[0]
            .GetProperty("uri")
            .GetString();
        Assert.Equal(ServerOwnedArtifactUri(package.Screenshots![0].ArtifactId), evaluatedScreenshotUri);
        Assert.Equal(ServerOwnedArtifactUri(package.DesignRenders![0].ArtifactId), evaluatedRenderUri);
        Assert.NotEqual(screenshot.Sha256, evaluatedScreenshotUri);
        Assert.NotEqual(render.Sha256, evaluatedRenderUri);
        Assert.DoesNotContain(RouteToken, submitted.Envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(screenshot.Path, VisibleResultText(first.Result), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(render.Path, VisibleResultText(first.Result), StringComparison.OrdinalIgnoreCase);
    }

    public static IEnumerable<object[]> UnsafeCallerOutboundHeaderCases()
    {
        yield return ["token-crlf"];
        yield return ["token-control"];
        yield return ["token-over-max"];
        yield return ["token-internal-whitespace"];
        yield return ["token-interior-padding"];
        yield return ["idempotency-over-max"];
        yield return ["idempotency-crlf"];
    }

    [Theory]
    [MemberData(nameof(UnsafeCallerOutboundHeaderCases))]
    public async Task ReadyJsonSubmitClient_InvalidCallerOutboundHeaders_FailsBeforeArtifactUploadOrEvaluate(
        string caseName)
    {
        var requestId = "ready-json-upload-invalid-" + caseName;
        const string privateMarker = "PRIVATE_READY_JSON_UPLOAD_INVALID_CALLER_HEADERS_SECRET";
        var (routeAdmissionToken, callerIdempotencyKey, malformedFragment) =
            CreateUnsafeCallerOutboundHeaders(caseName);
        using var temp = TemporaryDirectory.Create();
        var screenshot = await WritePngFixtureAsync(temp.Path, "source-screenshot.png");
        var package = CreatePackage(requestId, privateMarker, screenshot, designRender: null);
        File.Delete(screenshot.Path);
        Assert.False(File.Exists(screenshot.Path));
        var handler = new ReadyJsonUploadSubmitHandler(requestId);
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);

        var result = await client.TrySubmitAsync(
            ReadyJson(uploadConfigured: true, uploadMaxBytes: TinyPngBytes.Length + 16),
            package,
            routeAdmissionToken: routeAdmissionToken,
            idempotencyKey: callerIdempotencyKey);

        Assert.False(result.IsSuccess);
        Assert.Equal("READY_JSON_CALLER_OUTBOUND_HEADER_INVALID", result.ErrorCode);
        Assert.Equal("DesignConversion live-local ready.json caller outbound headers are invalid.", result.ErrorMessage);
        Assert.Equal(0, handler.SendCount);
        Assert.Empty(handler.UploadRequests);
        Assert.Empty(handler.EvaluateRequests);

        var visible = VisibleResultText(result);
        var serialized = JsonSerializer.Serialize(result, DesignConversionModelServerHttpHandler.JsonOptions);
        Assert.DoesNotContain(malformedFragment, visible, StringComparison.Ordinal);
        Assert.DoesNotContain(malformedFragment, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(screenshot.Path, visible, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(new Uri(screenshot.Path).AbsoluteUri, visible, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Convert.ToBase64String(TinyPngBytes), visible, StringComparison.Ordinal);
        Assert.DoesNotContain(privateMarker, visible, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("max-token")]
    [InlineData("trailing-padding")]
    public async Task ReadyJsonSubmitClient_ValidCallerHeaders_AreTrimmedAndAppliedExactly(
        string caseName)
    {
        var expectedToken = caseName switch
        {
            "max-token" => new string('t', 4096),
            "trailing-padding" => "abcDEF012+/==",
            _ => throw new ArgumentOutOfRangeException(nameof(caseName))
        };
        var requestId = "ready-json-valid-caller-headers-" + caseName;
        var package = CreateRefsOnlyPackage(requestId, "PRIVATE_READY_JSON_VALID_CALLER_HEADERS_SECRET");
        var handler = new ReadyJsonUploadSubmitHandler(requestId);
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);

        var result = await client.TrySubmitAsync(
            ReadyJson(uploadConfigured: false, uploadMaxBytes: null),
            package,
            routeAdmissionToken: " \t" + expectedToken + "\r\n",
            idempotencyKey: " \t" + CallerIdempotencyKey + "\r\n");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("success", result.Value!.Envelope.EnvelopeKind);
        Assert.Equal(1, handler.SendCount);
        Assert.Empty(handler.UploadRequests);
        var evaluate = Assert.Single(handler.EvaluateRequests);
        Assert.Equal(
            "Bearer " + expectedToken,
            AssertSingleHeader(evaluate, DesignConversionModelServerRouteAdmission.AuthorizationHeaderName));
        Assert.Equal(
            CallerIdempotencyKey,
            AssertSingleHeader(evaluate, DesignConversionModelServerRouteAdmission.IdempotencyHeaderName));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(" \t\r\n", " \t\r\n")]
    public async Task ReadyJsonSubmitClient_OptionalCallerHeaders_AreOmitted(
        string? routeAdmissionToken,
        string? callerIdempotencyKey)
    {
        const string requestId = "ready-json-optional-caller-headers";
        var package = CreateRefsOnlyPackage(requestId, "PRIVATE_READY_JSON_OPTIONAL_CALLER_HEADERS_SECRET");
        var handler = new ReadyJsonUploadSubmitHandler(requestId);
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);

        var result = await client.TrySubmitAsync(
            ReadyJson(uploadConfigured: false, uploadMaxBytes: null),
            package,
            routeAdmissionToken: routeAdmissionToken,
            idempotencyKey: callerIdempotencyKey);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("success", result.Value!.Envelope.EnvelopeKind);
        Assert.Equal(1, handler.SendCount);
        var evaluate = Assert.Single(handler.EvaluateRequests);
        Assert.False(evaluate.HeaderValues.ContainsKey(
            DesignConversionModelServerRouteAdmission.AuthorizationHeaderName));
        Assert.False(evaluate.HeaderValues.ContainsKey(
            DesignConversionModelServerRouteAdmission.IdempotencyHeaderName));
    }

    [Fact]
    public async Task ReadyJsonSubmitClient_ReceiptArtifactRefUriForgedToSha256_FailsClosedBeforeEvaluate()
    {
        const string requestId = "ready-json-upload-forged-receipt-uri";
        const string privateMarker = "PRIVATE_READY_JSON_UPLOAD_FORGED_RECEIPT_URI_SECRET";
        using var temp = TemporaryDirectory.Create();
        var screenshot = await WritePngFixtureAsync(temp.Path, "source-screenshot.png");
        var package = CreatePackage(requestId, privateMarker, screenshot, designRender: null);

        var run = await SubmitAsync(
            ReadyJson(uploadConfigured: true, uploadMaxBytes: TinyPngBytes.Length + 16),
            package,
            UploadReceiptMutation.ArtifactRefUriUsesSha256);

        Assert.True(run.Result.IsSuccess, run.Result.ErrorMessage);
        var submitted = run.Result.Value!;
        Assert.Equal("error", submitted.Envelope.EnvelopeKind);
        Assert.Equal("READY_JSON_ARTIFACT_UPLOAD_RESPONSE_INVALID", submitted.Envelope.ParsedErrorCode);
        Assert.False(submitted.Envelope.RuntimeInvoked);
        Assert.Equal(1, run.Handler.SendCount);
        Assert.Equal([DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath], run.Handler.Paths);
        Assert.Single(run.Handler.UploadRequests);
        Assert.Empty(run.Handler.EvaluateRequests);
        AssertBoundedUploadFailurePrivacy(run.Result, screenshot, privateMarker);
    }

    [Fact]
    public async Task ReadyJsonSubmitClient_ReceiptViewportWidthContradictsRequestedCanonicalMetadata_FailsClosedBeforeEvaluate()
    {
        const string requestId = "ready-json-upload-contradictory-receipt-viewport";
        const string privateMarker = "PRIVATE_READY_JSON_UPLOAD_CONTRADICTORY_VIEWPORT_SECRET";
        const int viewportWidth = 1440;
        const int viewportHeight = 900;
        using var temp = TemporaryDirectory.Create();
        var viewportPng = BuildGrayscalePng(viewportWidth, viewportHeight);
        var screenshot = await WritePngFixtureAsync(temp.Path, "source-screenshot.png", viewportPng);
        var package = CreatePackage(
            requestId,
            privateMarker,
            screenshot,
            designRender: null,
            screenshotWidth: viewportWidth,
            screenshotHeight: viewportHeight,
            screenshotMetadata: new Dictionary<string, string>
            {
                [DesignConversionModelServerArtifactUploadStore.ViewportWidthMetadataKey] = viewportWidth.ToString(System.Globalization.CultureInfo.InvariantCulture),
                [DesignConversionModelServerArtifactUploadStore.ViewportHeightMetadataKey] = viewportHeight.ToString(System.Globalization.CultureInfo.InvariantCulture)
            });

        var run = await SubmitAsync(
            ReadyJson(uploadConfigured: true, uploadMaxBytes: viewportPng.Length + 16),
            package,
            UploadReceiptMutation.ViewportWidthIsOne);

        Assert.True(run.Result.IsSuccess, run.Result.ErrorMessage);
        var submitted = run.Result.Value!;
        Assert.Equal("error", submitted.Envelope.EnvelopeKind);
        Assert.Equal("READY_JSON_ARTIFACT_UPLOAD_RESPONSE_INVALID", submitted.Envelope.ParsedErrorCode);
        Assert.False(submitted.Envelope.RuntimeInvoked);
        Assert.Equal(1, run.Handler.SendCount);
        Assert.Equal([DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath], run.Handler.Paths);
        var upload = Assert.Single(run.Handler.UploadRequests);
        Assert.Equal(
            "1440",
            upload.Headers[DesignConversionModelServerArtifactUploadHttpHandler.ViewportWidthHeaderName]);
        Assert.Empty(run.Handler.EvaluateRequests);
        AssertBoundedUploadFailurePrivacy(run.Result, screenshot, privateMarker);
    }

    [Fact]
    public async Task ReadyJsonSubmitClient_LocalPngMissingUploadRoute_FailsClosedBeforeEvaluate()
    {
        const string requestId = "ready-json-upload-route-missing";
        const string privateMarker = "PRIVATE_READY_JSON_UPLOAD_ROUTE_MISSING_SECRET";
        using var temp = TemporaryDirectory.Create();
        var screenshot = await WritePngFixtureAsync(temp.Path, "source-screenshot.png");
        var package = CreatePackage(requestId, privateMarker, screenshot, designRender: null);
        var handler = new ReadyJsonUploadSubmitHandler(requestId);
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);

        var result = await client.TrySubmitAsync(
            ReadyJson(uploadConfigured: false, uploadMaxBytes: null),
            package,
            routeAdmissionToken: RouteToken,
            idempotencyKey: CallerIdempotencyKey);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(0, handler.SendCount);
        var submitted = result.Value!;
        Assert.Equal("error", submitted.Envelope.EnvelopeKind);
        Assert.Equal("READY_JSON_ARTIFACT_UPLOAD_NOT_READY", submitted.Envelope.ParsedErrorCode);
        Assert.Equal(400, submitted.Envelope.StatusCode);
        Assert.False(submitted.Envelope.RuntimeInvoked);
        Assert.DoesNotContain(screenshot.Path, VisibleResultText(result), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(privateMarker, VisibleResultText(result), StringComparison.Ordinal);
        Assert.DoesNotContain(RouteToken, VisibleResultText(result), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ReadyJsonSubmitClient_LocalPngHashMismatch_FailsClosedBeforeEvaluate()
    {
        const string requestId = "ready-json-upload-hash-mismatch";
        const string privateMarker = "PRIVATE_READY_JSON_UPLOAD_HASH_MISMATCH_SECRET";
        using var temp = TemporaryDirectory.Create();
        var screenshot = await WritePngFixtureAsync(temp.Path, "source-screenshot.png");
        var mismatched = screenshot with { Sha256 = "sha256:" + new string('0', 64) };
        var package = CreatePackage(requestId, privateMarker, mismatched, designRender: null);
        var handler = new ReadyJsonUploadSubmitHandler(requestId);
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);

        var result = await client.TrySubmitAsync(
            ReadyJson(uploadConfigured: true, uploadMaxBytes: TinyPngBytes.Length + 16),
            package,
            routeAdmissionToken: RouteToken,
            idempotencyKey: CallerIdempotencyKey);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(0, handler.SendCount);
        var submitted = result.Value!;
        Assert.Equal("error", submitted.Envelope.EnvelopeKind);
        Assert.Equal("READY_JSON_ARTIFACT_UPLOAD_SHA256_MISMATCH", submitted.Envelope.ParsedErrorCode);
        Assert.False(submitted.Envelope.RuntimeInvoked);
        Assert.DoesNotContain(screenshot.Path, VisibleResultText(result), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(privateMarker, VisibleResultText(result), StringComparison.Ordinal);
        Assert.DoesNotContain(RouteToken, VisibleResultText(result), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ReadyJsonSubmitClient_LocalPngReadyMaxByteOverflow_FailsClosedBeforeEvaluate()
    {
        const string requestId = "ready-json-upload-max-byte-overflow";
        const string privateMarker = "PRIVATE_READY_JSON_UPLOAD_MAX_BYTE_OVERFLOW_SECRET";
        using var temp = TemporaryDirectory.Create();
        var screenshot = await WritePngFixtureAsync(temp.Path, "source-screenshot.png");
        var package = CreatePackage(requestId, privateMarker, screenshot, designRender: null);
        var handler = new ReadyJsonUploadSubmitHandler(requestId);
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);

        var result = await client.TrySubmitAsync(
            ReadyJson(uploadConfigured: true, uploadMaxBytes: TinyPngBytes.Length - 1),
            package,
            routeAdmissionToken: RouteToken,
            idempotencyKey: CallerIdempotencyKey);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(0, handler.SendCount);
        var submitted = result.Value!;
        Assert.Equal("error", submitted.Envelope.EnvelopeKind);
        Assert.Equal("READY_JSON_ARTIFACT_UPLOAD_TOO_LARGE", submitted.Envelope.ParsedErrorCode);
        Assert.False(submitted.Envelope.RuntimeInvoked);
        Assert.DoesNotContain(screenshot.Path, VisibleResultText(result), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(privateMarker, VisibleResultText(result), StringComparison.Ordinal);
        Assert.DoesNotContain(RouteToken, VisibleResultText(result), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ReadyJsonSubmitClient_ServerOwnedRefsDoNotRequireUploadConfig()
    {
        const string requestId = "ready-json-upload-server-owned-compat";
        const string privateMarker = "PRIVATE_READY_JSON_UPLOAD_SERVER_OWNED_COMPAT_SECRET";
        var package = CreateRefsOnlyPackage(requestId, privateMarker);
        var handler = new ReadyJsonUploadSubmitHandler(requestId);
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);

        var result = await client.TrySubmitAsync(
            ReadyJson(uploadConfigured: false, uploadMaxBytes: null),
            package,
            routeAdmissionToken: RouteToken,
            idempotencyKey: CallerIdempotencyKey);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("success", result.Value!.Envelope.EnvelopeKind);
        Assert.Equal(1, handler.SendCount);
        Assert.Equal([DesignConversionModelServerEndpointRegistry.EndpointPath], handler.Paths);
        Assert.Empty(handler.UploadRequests);
        Assert.Single(handler.EvaluateRequests);
    }

    private static async Task<SubmitRun> SubmitAsync(
        string readyJson,
        DesignConversionModelServerLocalCallerPackage package,
        UploadReceiptMutation receiptMutation = UploadReceiptMutation.None)
    {
        var handler = new ReadyJsonUploadSubmitHandler(package.RequestId, receiptMutation);
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);

        var result = await client.TrySubmitAsync(
            readyJson,
            package,
            routeAdmissionToken: RouteToken,
            idempotencyKey: CallerIdempotencyKey);

        return new SubmitRun(result, handler);
    }

    private static (string RouteAdmissionToken, string CallerIdempotencyKey, string MalformedFragment)
        CreateUnsafeCallerOutboundHeaders(string caseName)
        => caseName switch
        {
            "token-crlf" =>
                ("route-token\r\nInjected: yes", CallerIdempotencyKey, "Injected: yes"),
            "token-control" =>
                ("route-token\u0001", CallerIdempotencyKey, "route-token"),
            "token-over-max" =>
                (new string('t', 4097), CallerIdempotencyKey, new string('t', 64)),
            "token-internal-whitespace" =>
                ("route token", CallerIdempotencyKey, "route token"),
            "token-interior-padding" =>
                ("route=token", CallerIdempotencyKey, "route=token"),
            "idempotency-over-max" =>
                (RouteToken, new string('a', 129), new string('a', 64)),
            "idempotency-crlf" =>
                (RouteToken, "caller-key\r\nInjected: yes", "Injected: yes"),
            _ => throw new ArgumentOutOfRangeException(nameof(caseName))
        };

    private static async Task<LocalPngFixture> WritePngFixtureAsync(
        string root,
        string fileName,
        byte[]? pngBytes = null)
    {
        var bytes = pngBytes ?? TinyPngBytes;
        var path = Path.Combine(root, fileName);
        await File.WriteAllBytesAsync(path, bytes);
        var sha = "sha256:" + Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        return new LocalPngFixture(path, sha);
    }

    private static byte[] BuildGrayscalePng(int width, int height)
    {
        var scanlines = new byte[checked(height * (width + 1))];
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Fastest, leaveOpen: true))
            zlib.Write(scanlines);

        var ihdr = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(ihdr.AsSpan(0, 4), width);
        BinaryPrimitives.WriteInt32BigEndian(ihdr.AsSpan(4, 4), height);
        ihdr[8] = 8;
        ihdr[9] = 0;

        using var png = new MemoryStream();
        png.Write([137, 80, 78, 71, 13, 10, 26, 10]);
        WritePngChunk(png, "IHDR", ihdr);
        WritePngChunk(png, "IDAT", compressed.ToArray());
        WritePngChunk(png, "IEND", []);
        return png.ToArray();
    }

    private static void WritePngChunk(Stream stream, string type, byte[] data)
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
        foreach (var value in bytes)
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++)
                crc = (crc & 1) == 1 ? (crc >> 1) ^ 0xedb88320u : crc >> 1;
        }

        return ~crc;
    }

    private static string ReadyJson(bool uploadConfigured, long? uploadMaxBytes)
        => JsonSerializer.Serialize(
            new Dictionary<string, object?>
            {
                ["baseUrl"] = BaseUrl,
                ["routePaths"] = uploadConfigured
                    ? new[]
                    {
                        DesignConversionModelServerEndpointRegistry.EndpointPath,
                        DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath
                    }
                    : new[] { DesignConversionModelServerEndpointRegistry.EndpointPath },
                ["artifactUploadRouteMapped"] = uploadConfigured,
                ["artifactUploadConfigured"] = uploadConfigured,
                ["artifactUploadEndpointPath"] = uploadConfigured
                    ? DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath
                    : null,
                ["artifactUploadMaxBytes"] = uploadMaxBytes
            },
            DesignConversionModelServerHttpHandler.JsonOptions);

    private static DesignConversionModelServerLocalCallerPackage CreatePackage(
        string requestId,
        string privateMarker,
        LocalPngFixture screenshot,
        LocalPngFixture? designRender,
        int screenshotWidth = 1,
        int screenshotHeight = 1,
        IReadOnlyDictionary<string, string>? screenshotMetadata = null)
    {
        var screenshots = new[]
        {
            new DesignConversionModelServerArtifactRef(
                ArtifactId: requestId + "-source-screenshot",
                Kind: "screenshot",
                Width: screenshotWidth,
                Height: screenshotHeight,
                Uri: new Uri(screenshot.Path).AbsoluteUri,
                Sha256: screenshot.Sha256,
                Metadata: screenshotMetadata)
        };
        var renders = designRender is null
            ? Array.Empty<DesignConversionModelServerArtifactRef>()
            : new[]
            {
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: requestId + "-design-render",
                    Kind: "design-render",
                    Width: 1,
                    Height: 1,
                    Uri: designRender.Path,
                    Sha256: designRender.Sha256)
            };

        return CreateBasePackage(requestId, privateMarker) with
        {
            Screenshots = screenshots,
            DesignRenders = renders
        };
    }

    private static DesignConversionModelServerLocalCallerPackage CreateRefsOnlyPackage(
        string requestId,
        string privateMarker)
        => CreateBasePackage(requestId, privateMarker) with
        {
            Screenshots =
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: requestId + "-source-screenshot",
                    Kind: "screenshot",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://ready-json-upload/source-screenshot.png",
                    Sha256: "sha256:" + new string('1', 64))
            ],
            DesignRenders =
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: requestId + "-design-render",
                    Kind: "design-render",
                    Width: 1440,
                    Height: 900,
                    Uri: "sha256:" + new string('2', 64),
                    Sha256: "sha256:" + new string('2', 64))
            ]
        };

    private static DesignConversionModelServerLocalCallerPackage CreateBasePackage(
        string requestId,
        string privateMarker)
        => new(
            RequestId: requestId,
            SourceKind: nameof(DesignConversionSourceKind.Figma),
            DesignTree: new DesignConversionModelServerPayloadRef(
                RefId: requestId + "-design-tree",
                Kind: "figma-json",
                Sha256: "sha256:" + new string('3', 64),
                Payload: "{\"kind\":\"design\",\"private\":\"" + privateMarker + "\"}",
                Format: "figma-json"),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: requestId + "-original-tree",
                Kind: "figma-json",
                Sha256: "sha256:" + new string('4', 64),
                Payload: "{\"kind\":\"original\",\"private\":\"" + privateMarker + "\"}",
                Format: "figma-json"),
            Html: "<main data-private=\"" + privateMarker + "\">ready-json upload</main>",
            Css: ".ready-json-upload{display:block;}",
            Screenshots: [],
            DesignRenders: []);

    private static string SuccessResponseJson(string requestId)
        => JsonSerializer.Serialize(new
        {
            contractStatus = DesignConversionThinClientStatus.Completed,
            requestId,
            boundaryMetadata = new Dictionary<string, string>
            {
                ["runtime_invoked"] = bool.TrueString
            }
        }, DesignConversionModelServerHttpHandler.JsonOptions);

    private static string ServerOwnedArtifactUri(string artifactId)
        => new Uri(Path.Combine(
            Path.GetTempPath(),
            "ready-json-server-owned-artifacts",
            artifactId + ".png")).AbsoluteUri;

    private static string AssertSingleHeader(CapturedRequest request, string headerName)
    {
        var values = request.HeaderValues[headerName];
        var value = Assert.Single(values);
        Assert.Equal(value, request.Headers[headerName]);
        return value;
    }

    private static string VisibleResultText(
        OperationResult<DesignConversionModelServerReadyJsonSubmitResult> result)
    {
        var parts = new List<string?>
        {
            result.ErrorCode,
            result.ErrorMessage
        };

        if (result.Value is { } submitted)
        {
            parts.Add(submitted.Envelope.ParsedErrorCode);
            parts.Add(submitted.Envelope.ParsedErrorMessage);
            parts.Add(submitted.Envelope.ResponseBodyJson);
            parts.AddRange(submitted.Envelope.BoundaryMetadata.Keys);
            parts.AddRange(submitted.Envelope.BoundaryMetadata.Values);
        }

        return string.Join("\n", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
    }

    private static void AssertBoundedUploadFailurePrivacy(
        OperationResult<DesignConversionModelServerReadyJsonSubmitResult> result,
        LocalPngFixture screenshot,
        string privateMarker)
    {
        var visible = VisibleResultText(result);
        var pngBase64 = Convert.ToBase64String(File.ReadAllBytes(screenshot.Path));
        Assert.DoesNotContain(screenshot.Path, visible, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(new Uri(screenshot.Path).AbsoluteUri, visible, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(pngBase64, visible, StringComparison.Ordinal);
        Assert.DoesNotContain("data:image/png;base64", visible, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(RouteToken, visible, StringComparison.Ordinal);
        Assert.DoesNotContain(privateMarker, visible, StringComparison.Ordinal);
    }

    private static readonly byte[] TinyPngBytes = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGOSHzRgQAAAABJRU5ErkJggg==");

    private sealed record SubmitRun(
        OperationResult<DesignConversionModelServerReadyJsonSubmitResult> Result,
        ReadyJsonUploadSubmitHandler Handler);

    private sealed record LocalPngFixture(string Path, string Sha256);

    private sealed record CapturedRequest(
        string Method,
        string Path,
        string? ContentType,
        string? BodyJson,
        byte[] BodyBytes,
        IReadOnlyDictionary<string, string> Headers,
        IReadOnlyDictionary<string, string[]> HeaderValues);

    private enum UploadReceiptMutation
    {
        None,
        ArtifactRefUriUsesSha256,
        ViewportWidthIsOne
    }

    private sealed class ReadyJsonUploadSubmitHandler(
        string requestId,
        UploadReceiptMutation receiptMutation = UploadReceiptMutation.None) : HttpMessageHandler
    {
        public int SendCount { get; private set; }

        public List<string> Paths { get; } = [];

        public List<CapturedRequest> UploadRequests { get; } = [];

        public List<CapturedRequest> EvaluateRequests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            SendCount++;
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            Paths.Add(path);
            var captured = await CaptureAsync(request, path, cancellationToken);

            if (string.Equals(path, DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath, StringComparison.Ordinal))
            {
                UploadRequests.Add(captured);
                return UploadResponse(captured, receiptMutation);
            }

            if (string.Equals(path, DesignConversionModelServerEndpointRegistry.EndpointPath, StringComparison.Ordinal))
            {
                EvaluateRequests.Add(captured);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(SuccessResponseJson(requestId), Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };
        }

        private static async Task<CapturedRequest> CaptureAsync(
            HttpRequestMessage request,
            string path,
            CancellationToken cancellationToken)
        {
            var bodyBytes = request.Content is null
                ? []
                : await request.Content.ReadAsByteArrayAsync(cancellationToken);
            var bodyJson = IsJsonContent(request)
                ? Encoding.UTF8.GetString(bodyBytes)
                : null;
            var headers = new Dictionary<string, string>(StringComparer.Ordinal);
            var headerValues = new Dictionary<string, string[]>(StringComparer.Ordinal);
            foreach (var header in request.Headers)
            {
                headers[header.Key] = string.Join(",", header.Value);
                headerValues[header.Key] = header.Value.ToArray();
            }
            if (request.Content is not null)
            {
                foreach (var header in request.Content.Headers)
                {
                    headers[header.Key] = string.Join(",", header.Value);
                    headerValues[header.Key] = header.Value.ToArray();
                }
            }

            return new CapturedRequest(
                request.Method.Method,
                path,
                request.Content?.Headers.ContentType?.MediaType,
                bodyJson,
                bodyBytes,
                headers,
                headerValues);
        }

        private static HttpResponseMessage UploadResponse(
            CapturedRequest request,
            UploadReceiptMutation receiptMutation)
        {
            var artifactId = request.Headers[DesignConversionModelServerArtifactUploadHttpHandler.ArtifactIdHeaderName];
            var kind = request.Headers[DesignConversionModelServerArtifactUploadHttpHandler.ArtifactKindHeaderName];
            var sha256 = request.Headers[DesignConversionModelServerArtifactUploadHttpHandler.ExpectedSha256HeaderName];
            var width = int.Parse(request.Headers[DesignConversionModelServerArtifactUploadHttpHandler.WidthHeaderName], System.Globalization.CultureInfo.InvariantCulture);
            var height = int.Parse(request.Headers[DesignConversionModelServerArtifactUploadHttpHandler.HeightHeaderName], System.Globalization.CultureInfo.InvariantCulture);
            var receiptMetadata = new Dictionary<string, string>
            {
                ["media_type"] = "image/png",
                ["ingress_boundary"] = DesignConversionServerOwnedLocalArtifactRefResolver.Boundary
            };
            if (receiptMutation is UploadReceiptMutation.ViewportWidthIsOne)
            {
                receiptMetadata[DesignConversionModelServerArtifactUploadStore.ViewportWidthMetadataKey] = "1";
            }

            var artifactRef = new DesignConversionModelServerArtifactRef(
                ArtifactId: artifactId,
                Kind: kind,
                Width: width,
                Height: height,
                Uri: receiptMutation is UploadReceiptMutation.ArtifactRefUriUsesSha256
                    ? sha256
                    : ServerOwnedArtifactUri(artifactId),
                Sha256: sha256,
                Metadata: receiptMetadata);
            var response = new DesignConversionModelServerArtifactUploadHttpSuccessResponse(
                Endpoint: DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath,
                Method: "POST",
                HttpSeam: DesignConversionModelServerArtifactUploadHttpHandler.LocalHttpSeam,
                StatusCode: 201,
                ContractStatus: "refs_only_png_upload_receipt_transport_neutral_no_live_route",
                ArtifactRef: artifactRef,
                ByteCount: request.BodyBytes.LongLength,
                MediaType: "image/png",
                Sha256: sha256,
                ArtifactId: artifactId,
                Kind: kind,
                Width: width,
                Height: height,
                LiveHttpRouteRegistered: true,
                AspNetEndpointMapped: true,
                DeploymentVerified: false,
                PublicHttpEndpointVerified: false,
                ProductReady: false,
                RawBytesIncluded: false,
                BoundaryMetadata: new Dictionary<string, string>
                {
                    ["upload_store_invoked"] = bool.TrueString,
                    ["raw_bytes_included"] = bool.FalseString,
                    ["product_ready"] = bool.FalseString
                });

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(response, DesignConversionModelServerHttpHandler.JsonOptions),
                    Encoding.UTF8,
                    "application/json")
            };
        }

        private static bool IsJsonContent(HttpRequestMessage request)
            => string.Equals(
                request.Content?.Headers.ContentType?.MediaType,
                "application/json",
                StringComparison.OrdinalIgnoreCase);
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        private TemporaryDirectory(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public static TemporaryDirectory Create()
        {
            var path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                nameof(DesignConversionModelServerReadyJsonArtifactUploadSubmitClientTests),
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return new TemporaryDirectory(path);
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(Path, recursive: true);
            }
            catch
            {
                // Best-effort cleanup for temp files held by failed assertions.
            }
        }
    }
}
