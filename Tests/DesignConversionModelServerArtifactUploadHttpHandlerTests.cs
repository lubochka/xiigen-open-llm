// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ModelServer.DesignConversion;

public sealed class DesignConversionModelServerArtifactUploadHttpHandlerTests
{
    private static readonly DateTimeOffset FixedNow = DateTimeOffset.Parse("2026-07-11T12:00:00Z");
    private const string CurrentToken = "current-upload-route-token-0123456789";
    private const string TenantId = "tenant-upload-handler-alpha";

    [Fact]
    public async Task HandleAsync_ValidPngSuccess_ReturnsResolverValidRefsOnlyReceipt()
    {
        var root = CreateTempRoot("valid");
        try
        {
            var png = BuildRgbaPng(6, 4, blank: false);
            var stream = new CountingReadStream(png);
            var handler = CreateHandler(root, maxBytes: 1024 * 1024);

            var response = await handler.HandleAsync(Request(
                stream,
                Headers(png, width: 6, height: 4, idempotencyKey: "valid-success"),
                contentLength: png.Length));

            Assert.Equal(201, response.StatusCode);
            Assert.True(stream.BytesRead > 0);
            using var document = JsonDocument.Parse(response.BodyJson);
            var body = document.RootElement;
            Assert.Equal(DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath, body.GetProperty("endpoint").GetString());
            Assert.Equal("refs_only_png_upload_receipt_transport_neutral_no_live_route", body.GetProperty("contractStatus").GetString());
            Assert.Equal(png.Length, body.GetProperty("byteCount").GetInt64());
            Assert.Equal("image/png", body.GetProperty("mediaType").GetString());
            Assert.Equal(Sha256(png), body.GetProperty("sha256").GetString());
            Assert.Equal("valid-upload", body.GetProperty("artifactId").GetString());
            Assert.Equal("screenshot", body.GetProperty("kind").GetString());
            Assert.Equal(6, body.GetProperty("width").GetInt32());
            Assert.Equal(4, body.GetProperty("height").GetInt32());
            Assert.False(body.GetProperty("liveHttpRouteRegistered").GetBoolean());
            Assert.False(body.GetProperty("aspNetEndpointMapped").GetBoolean());
            Assert.False(body.GetProperty("publicHttpEndpointVerified").GetBoolean());
            Assert.False(body.GetProperty("productReady").GetBoolean());
            Assert.False(body.GetProperty("rawBytesIncluded").GetBoolean());

            var artifactRef = JsonSerializer.Deserialize<DesignConversionModelServerArtifactRef>(
                body.GetProperty("artifactRef").GetRawText(),
                DesignConversionModelServerHttpHandler.JsonOptions);
            Assert.NotNull(artifactRef);
            var resolver = new DesignConversionServerOwnedLocalArtifactRefResolver(root);
            var resolved = resolver.Validate(artifactRef!, "artifact upload handler success test");
            Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
            Assert.Single(PublishedPngFiles(root));
            Assert.Empty(TempFiles(root));
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task HandleAsync_AdmissionFailures_DoNotReadBody()
    {
        var cases = new[]
        {
            new AdmissionCase(
                "disabled",
                DesignConversionModelServerRouteAdmission.Create(null, () => FixedNow),
                Headers(idempotencyKey: "disabled"),
                503),
            new AdmissionCase(
                "not-ready",
                DesignConversionModelServerRouteAdmission.Create(
                    new DesignConversionModelServerRouteAdmissionOptions(enabled: true, currentToken: null, tenantId: TenantId),
                    () => FixedNow),
                Headers(idempotencyKey: "not-ready"),
                503),
            new AdmissionCase(
                "missing-token",
                CreateAdmission(),
                Headers(includeAuthorization: false, idempotencyKey: "missing-token"),
                401),
            new AdmissionCase(
                "invalid-token",
                CreateAdmission(),
                Headers(token: "wrong-token", idempotencyKey: "invalid-token"),
                403),
            new AdmissionCase(
                "missing-idempotency-key",
                CreateAdmission(),
                Headers(includeIdempotency: false),
                400)
        };

        foreach (var item in cases)
        {
            var root = CreateTempRoot($"admission-{item.Name}");
            try
            {
                var body = new ThrowOnReadStream();
                var handler = CreateHandler(root, routeAdmission: item.Admission);

                var response = await handler.HandleAsync(Request(body, item.Headers, contentLength: 16));

                Assert.Equal(item.ExpectedStatusCode, response.StatusCode);
                Assert.False(body.ReadAttempted);
                Assert.Empty(PublishedPngFiles(root));
                Assert.Empty(TempFiles(root));
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }
    }

    [Fact]
    public async Task HandleAsync_PreBodyValidationFailures_DoNotReadBody()
    {
        var png = BuildRgbaPng(4, 3, blank: false);
        var duplicateHeaders = Headers(png, idempotencyKey: "duplicate-header");
        duplicateHeaders = new Dictionary<string, string>(duplicateHeaders, StringComparer.Ordinal)
        {
            [DesignConversionModelServerArtifactUploadHttpHandler.ArtifactIdHeaderName.ToUpperInvariant()] = "other-id"
        };

        var cases = new[]
        {
            new PreBodyCase("wrong-method", "GET", DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath, "image/png", Headers(png, idempotencyKey: "wrong-method"), png.Length, 405),
            new PreBodyCase("wrong-path", "POST", "/api/design-conversion/other", "image/png", Headers(png, idempotencyKey: "wrong-path"), png.Length, 404),
            new PreBodyCase("wrong-content-type", "POST", DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath, "application/octet-stream", Headers(png, idempotencyKey: "wrong-content-type"), png.Length, 415),
            new PreBodyCase("missing-metadata", "POST", DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath, "image/png", RemoveHeader(Headers(png, idempotencyKey: "missing-metadata"), DesignConversionModelServerArtifactUploadHttpHandler.ArtifactIdHeaderName), png.Length, 400),
            new PreBodyCase("duplicate-metadata", "POST", DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath, "image/png", duplicateHeaders, png.Length, 400),
            new PreBodyCase("comma-joined-metadata", "POST", DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath, "image/png", WithHeader(Headers(png, idempotencyKey: "comma-joined"), DesignConversionModelServerArtifactUploadHttpHandler.ArtifactIdHeaderName, "one,two"), png.Length, 400),
            new PreBodyCase("unsafe-metadata", "POST", DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath, "image/png", WithHeader(Headers(png, idempotencyKey: "unsafe"), DesignConversionModelServerArtifactUploadHttpHandler.ArtifactIdHeaderName, "../bad"), png.Length, 400),
            new PreBodyCase("too-long-metadata", "POST", DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath, "image/png", WithHeader(Headers(png, idempotencyKey: "too-long"), DesignConversionModelServerArtifactUploadHttpHandler.ArtifactKindHeaderName, new string('a', 97)), png.Length, 400),
            new PreBodyCase("noncanonical-sha", "POST", DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath, "image/png", WithHeader(Headers(png, idempotencyKey: "noncanonical-sha"), DesignConversionModelServerArtifactUploadHttpHandler.ExpectedSha256HeaderName, "SHA256:" + new string('0', 64)), png.Length, 400),
            new PreBodyCase("zero-width", "POST", DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath, "image/png", WithHeader(Headers(png, idempotencyKey: "zero-width"), DesignConversionModelServerArtifactUploadHttpHandler.WidthHeaderName, "0"), png.Length, 400),
            new PreBodyCase("overflow-height", "POST", DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath, "image/png", WithHeader(Headers(png, idempotencyKey: "overflow-height"), DesignConversionModelServerArtifactUploadHttpHandler.HeightHeaderName, "2147483648"), png.Length, 400),
            new PreBodyCase("zero-content-length", "POST", DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath, "image/png", Headers(png, idempotencyKey: "zero-content-length"), 0, 400),
            new PreBodyCase("negative-content-length", "POST", DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath, "image/png", Headers(png, idempotencyKey: "negative-content-length"), -1, 400),
            new PreBodyCase("known-oversize-content-length", "POST", DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath, "image/png", Headers(png, idempotencyKey: "known-oversize"), 257, 413)
        };

        foreach (var item in cases)
        {
            var root = CreateTempRoot($"prebody-{item.Name}");
            try
            {
                var body = new ThrowOnReadStream();
                var handler = CreateHandler(root, maxBytes: 256);

                var response = await handler.HandleAsync(new DesignConversionModelServerArtifactUploadHttpRequest(
                    item.Method,
                    item.Path,
                    body,
                    item.ContentType,
                    item.ContentLength,
                    item.Headers));

                Assert.Equal(item.ExpectedStatusCode, response.StatusCode);
                Assert.False(body.ReadAttempted);
                Assert.Empty(PublishedPngFiles(root));
                Assert.Empty(TempFiles(root));
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }
    }

    [Fact]
    public async Task HandleAsync_UnknownLengthOversize_IsStoppedByStreamingStore()
    {
        var root = CreateTempRoot("unknown-length-oversize");
        try
        {
            var bytes = Enumerable.Repeat((byte)0x41, 512 * 1024).ToArray();
            var stream = new CountingReadStream(bytes);
            var handler = CreateHandler(root, maxBytes: 128);

            var response = await handler.HandleAsync(Request(
                stream,
                Headers(
                    artifactId: "unknown-oversize",
                    expectedSha256: "sha256:" + new string('0', 64),
                    width: 1,
                    height: 1,
                    idempotencyKey: "unknown-oversize"),
                contentLength: null));

            Assert.Equal(413, response.StatusCode);
            AssertErrorCode(response, DesignConversionModelServerArtifactUploadStore.OversizeErrorCode);
            Assert.Equal(129, stream.BytesRead);
            AssertCleanFailureRoot(root);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task HandleAsync_StoreValidationFailures_MapStatusesAndCleanFiles()
    {
        var valid = BuildRgbaPng(4, 3, blank: false);
        var blank = BuildRgbaPng(4, 3, blank: true);
        var invalid = Encoding.ASCII.GetBytes("not-a-png");
        var cases = new[]
        {
            new StoreFailureCase("wrong-sha", valid, "sha256:" + new string('0', 64), 4, 3, DesignConversionModelServerArtifactUploadStore.HashMismatchErrorCode),
            new StoreFailureCase("invalid-png", invalid, Sha256(invalid), 1, 1, DesignConversionModelServerArtifactUploadStore.PngValidationErrorCode),
            new StoreFailureCase("blank-png", blank, Sha256(blank), 4, 3, DesignConversionModelServerArtifactUploadStore.PngValidationErrorCode),
            new StoreFailureCase("dimension-mismatch", valid, Sha256(valid), 5, 3, DesignConversionModelServerArtifactUploadStore.PngValidationErrorCode)
        };

        foreach (var item in cases)
        {
            var root = CreateTempRoot($"store-failure-{item.Name}");
            try
            {
                var handler = CreateHandler(root, maxBytes: 1024 * 1024);

                var response = await handler.HandleAsync(Request(
                    new CountingReadStream(item.Bytes),
                    Headers(
                        item.Bytes,
                        artifactId: item.Name,
                        expectedSha256: item.ExpectedSha256,
                        width: item.Width,
                        height: item.Height,
                        idempotencyKey: item.Name),
                    contentLength: item.Bytes.Length));

                Assert.Equal(422, response.StatusCode);
                AssertErrorCode(response, item.ExpectedErrorCode);
                AssertCleanFailureRoot(root);
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }
    }

    [Fact]
    public async Task HandleAsync_SameKeyMetadataSequentialAndConcurrentReplay_StoresOnceAndDoesNotReadReplayBody()
    {
        await AssertSequentialReplayAsync();
        await AssertConcurrentReplayAsync();
    }

    [Fact]
    public async Task HandleAsync_SameKeyDifferentMetadata_ConflictsWithoutReadingConflictBody()
    {
        var root = CreateTempRoot("same-key-conflict");
        try
        {
            var png = BuildRgbaPng(4, 3, blank: false);
            var handler = CreateHandler(root, maxBytes: 1024 * 1024);
            var first = await handler.HandleAsync(Request(
                new CountingReadStream(png),
                Headers(png, idempotencyKey: "conflict-key"),
                contentLength: png.Length));
            Assert.Equal(201, first.StatusCode);

            var conflictBody = new ThrowOnReadStream();
            var conflict = await handler.HandleAsync(Request(
                conflictBody,
                Headers(png, artifactId: "different-artifact", idempotencyKey: "conflict-key"),
                contentLength: png.Length));

            Assert.Equal(409, conflict.StatusCode);
            Assert.False(conflictBody.ReadAttempted);
            Assert.Single(PublishedPngFiles(root));
            Assert.Empty(TempFiles(root));
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task HandleAsync_ErrorResponses_DoNotExposeRootTokenTenantOrRawBytes()
    {
        const string privateMarker = "PRIVATE_UPLOAD_HTTP_ROOT_MARKER";
        const string rawMarker = "PRIVATE_RAW_BYTES_MARKER";
        var root = CreateTempRoot(privateMarker);
        try
        {
            var bytes = Encoding.ASCII.GetBytes(rawMarker);
            var handler = CreateHandler(root, maxBytes: 1024 * 1024);

            var response = await handler.HandleAsync(Request(
                new CountingReadStream(bytes),
                Headers(
                    bytes,
                    artifactId: "leak-check",
                    expectedSha256: Sha256(bytes),
                    width: 1,
                    height: 1,
                    idempotencyKey: "leak-check"),
                contentLength: bytes.Length));

            Assert.Equal(422, response.StatusCode);
            Assert.DoesNotContain(root, response.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(privateMarker, response.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(CurrentToken, response.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(TenantId, response.BodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(rawMarker, response.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain("sha256:", response.BodyJson, StringComparison.OrdinalIgnoreCase);
            AssertCleanFailureRoot(root);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task HandleAsync_SuccessMetadata_StatesNoLiveRouteNoPublicDeploymentAndNotProductReady()
    {
        var root = CreateTempRoot("metadata-honesty");
        try
        {
            var png = BuildRgbaPng(3, 2, blank: false);
            var handler = CreateHandler(root, maxBytes: 1024 * 1024);

            var response = await handler.HandleAsync(Request(
                new CountingReadStream(png),
                Headers(png, width: 3, height: 2, idempotencyKey: "metadata-honesty"),
                contentLength: png.Length));

            Assert.Equal(201, response.StatusCode);
            using var document = JsonDocument.Parse(response.BodyJson);
            var body = document.RootElement;
            var metadata = body.GetProperty("boundaryMetadata");
            Assert.False(body.GetProperty("liveHttpRouteRegistered").GetBoolean());
            Assert.False(body.GetProperty("aspNetEndpointMapped").GetBoolean());
            Assert.False(body.GetProperty("deploymentVerified").GetBoolean());
            Assert.False(body.GetProperty("publicHttpEndpointVerified").GetBoolean());
            Assert.False(body.GetProperty("productReady").GetBoolean());
            Assert.False(body.GetProperty("rawBytesIncluded").GetBoolean());
            Assert.Equal(bool.FalseString, metadata.GetProperty("live_http_route_registered").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("aspnet_endpoint_mapped").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("public_http_endpoint_verified").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("deployment_verified").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("raw_bytes_included").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("request_body_buffered").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("content_length_semantic_idempotency_included").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("artifact_sha256_digest_exposed_in_metadata").GetString());
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    private static async Task AssertSequentialReplayAsync()
    {
        var root = CreateTempRoot("sequential-replay");
        try
        {
            var png = BuildRgbaPng(4, 3, blank: false);
            var handler = CreateHandler(root, maxBytes: 1024 * 1024);
            var firstStream = new CountingReadStream(png);
            var first = await handler.HandleAsync(Request(
                firstStream,
                Headers(png, idempotencyKey: "same-key"),
                contentLength: png.Length));
            Assert.Equal(201, first.StatusCode);
            Assert.True(firstStream.BytesRead > 0);

            var replayStream = new ThrowOnReadStream();
            var replay = await handler.HandleAsync(Request(
                replayStream,
                Headers(png, idempotencyKey: "same-key"),
                contentLength: null));

            Assert.Equal(201, replay.StatusCode);
            Assert.Equal(first.BodyJson, replay.BodyJson);
            Assert.False(replayStream.ReadAttempted);
            Assert.Single(PublishedPngFiles(root));
            Assert.Empty(TempFiles(root));
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    private static async Task AssertConcurrentReplayAsync()
    {
        var root = CreateTempRoot("concurrent-replay");
        try
        {
            var png = BuildRgbaPng(5, 3, blank: false);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var firstStream = new GatedReadStream(png, release);
            var handler = CreateHandler(root, maxBytes: 1024 * 1024);
            var first = handler.HandleAsync(Request(
                firstStream,
                Headers(png, width: 5, height: 3, idempotencyKey: "concurrent-key"),
                contentLength: png.Length));

            await firstStream.FirstReadStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
            var replayStream = new ThrowOnReadStream();
            var replay = handler.HandleAsync(Request(
                replayStream,
                Headers(png, width: 5, height: 3, idempotencyKey: "concurrent-key"),
                contentLength: png.Length));
            Assert.False(replayStream.ReadAttempted);

            release.TrySetResult(true);
            var responses = await Task.WhenAll(first, replay).WaitAsync(TimeSpan.FromSeconds(5));

            Assert.All(responses, item => Assert.Equal(201, item.StatusCode));
            Assert.Equal(responses[0].BodyJson, responses[1].BodyJson);
            Assert.False(replayStream.ReadAttempted);
            Assert.Single(PublishedPngFiles(root));
            Assert.Empty(TempFiles(root));
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    private static DesignConversionModelServerArtifactUploadHttpHandler CreateHandler(
        string root,
        long maxBytes = 1024 * 1024,
        DesignConversionModelServerRouteAdmission? routeAdmission = null)
        => new(
            new DesignConversionModelServerArtifactUploadStore(root, maxBytes),
            routeAdmission ?? CreateAdmission());

    private static DesignConversionModelServerRouteAdmission CreateAdmission()
        => DesignConversionModelServerRouteAdmission.Create(
            new DesignConversionModelServerRouteAdmissionOptions(
                enabled: true,
                currentToken: CurrentToken,
                tenantId: TenantId),
            () => FixedNow);

    private static DesignConversionModelServerArtifactUploadHttpRequest Request(
        Stream body,
        IReadOnlyDictionary<string, string> headers,
        long? contentLength,
        string method = "POST",
        string path = DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath,
        string? contentType = "image/png")
        => new(method, path, body, contentType, contentLength, headers);

    private static Dictionary<string, string> Headers(
        byte[]? bytes = null,
        string artifactId = "valid-upload",
        string kind = "screenshot",
        string? expectedSha256 = null,
        int width = 4,
        int height = 3,
        string token = CurrentToken,
        string idempotencyKey = "upload-key",
        bool includeAuthorization = true,
        bool includeIdempotency = true)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [DesignConversionModelServerArtifactUploadHttpHandler.ArtifactIdHeaderName] = artifactId,
            [DesignConversionModelServerArtifactUploadHttpHandler.ArtifactKindHeaderName] = kind,
            [DesignConversionModelServerArtifactUploadHttpHandler.ExpectedSha256HeaderName] =
                expectedSha256 ?? (bytes is null ? "sha256:" + new string('0', 64) : Sha256(bytes)),
            [DesignConversionModelServerArtifactUploadHttpHandler.WidthHeaderName] = width.ToString(System.Globalization.CultureInfo.InvariantCulture),
            [DesignConversionModelServerArtifactUploadHttpHandler.HeightHeaderName] = height.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };

        if (includeAuthorization)
            headers[DesignConversionModelServerRouteAdmission.AuthorizationHeaderName] = "Bearer " + token;
        if (includeIdempotency)
            headers[DesignConversionModelServerRouteAdmission.IdempotencyHeaderName] = idempotencyKey;

        return headers;
    }

    private static Dictionary<string, string> WithHeader(
        IReadOnlyDictionary<string, string> source,
        string name,
        string value)
    {
        var copy = new Dictionary<string, string>(source, StringComparer.OrdinalIgnoreCase);
        copy[name] = value;
        return copy;
    }

    private static Dictionary<string, string> RemoveHeader(
        IReadOnlyDictionary<string, string> source,
        string name)
    {
        var copy = new Dictionary<string, string>(source, StringComparer.OrdinalIgnoreCase);
        copy.Remove(name);
        return copy;
    }

    private static void AssertErrorCode(
        DesignConversionModelServerHttpResponse response,
        string expectedErrorCode)
    {
        using var document = JsonDocument.Parse(response.BodyJson);
        Assert.Equal(expectedErrorCode, document.RootElement.GetProperty("errorCode").GetString());
    }

    private static string CreateTempRoot(string name)
    {
        var safeName = new string(name.Select(ch => char.IsAsciiLetterOrDigit(ch) || ch is '-' or '_' ? ch : '-').ToArray());
        var root = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "design-conversion-upload-handler-tests",
            $"{safeName}-{Guid.NewGuid():N}"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static void DeleteTempRoot(string root)
    {
        if (!Directory.Exists(root))
            return;

        var fullRoot = Path.GetFullPath(root);
        var allowedRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "design-conversion-upload-handler-tests"));
        if (!fullRoot.StartsWith(allowedRoot, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            return;

        Directory.Delete(fullRoot, recursive: true);
    }

    private static void AssertCleanFailureRoot(string root)
    {
        Assert.Empty(PublishedPngFiles(root));
        Assert.Empty(TempFiles(root));
    }

    private static string[] PublishedPngFiles(string root)
    {
        var ingress = Path.Combine(root, "ingress");
        return Directory.Exists(ingress)
            ? Directory.GetFiles(ingress, "*.png", SearchOption.AllDirectories)
            : [];
    }

    private static string[] TempFiles(string root)
        => Directory.Exists(root)
            ? Directory.GetFiles(root, "*.tmp", SearchOption.AllDirectories)
            : [];

    private static byte[] BuildRgbaPng(int width, int height, bool blank)
    {
        var raw = new byte[checked(height * ((width * 4) + 1))];
        for (var y = 0; y < height; y++)
        {
            var row = y * ((width * 4) + 1);
            raw[row] = 0;
            for (var x = 0; x < width; x++)
            {
                var offset = row + 1 + (x * 4);
                if (blank)
                {
                    raw[offset] = 255;
                    raw[offset + 1] = 255;
                    raw[offset + 2] = 255;
                    raw[offset + 3] = 255;
                    continue;
                }

                raw[offset] = (byte)(20 + ((x * 31) % 180));
                raw[offset + 1] = (byte)(40 + ((y * 47) % 160));
                raw[offset + 2] = (byte)(90 + (((x + y) * 29) % 150));
                raw[offset + 3] = 255;
            }
        }

        using var png = new MemoryStream();
        png.Write([137, 80, 78, 71, 13, 10, 26, 10]);
        WritePngChunk(png, "IHDR", BuildPngIhdr(width, height));
        WritePngChunk(png, "IDAT", CompressPngScanlines(raw));
        WritePngChunk(png, "IEND", []);
        return png.ToArray();
    }

    private static byte[] BuildPngIhdr(int width, int height)
    {
        var data = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(0, 4), width);
        BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(4, 4), height);
        data[8] = 8;
        data[9] = 6;
        return data;
    }

    private static byte[] CompressPngScanlines(byte[] raw)
    {
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Fastest, leaveOpen: true))
            zlib.Write(raw);
        return compressed.ToArray();
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
        foreach (var b in bytes)
        {
            crc ^= b;
            for (var bit = 0; bit < 8; bit++)
                crc = (crc & 1) == 1 ? (crc >> 1) ^ 0xedb88320u : crc >> 1;
        }

        return ~crc;
    }

    private static string Sha256(byte[] bytes)
        => "sha256:" + Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private sealed record AdmissionCase(
        string Name,
        DesignConversionModelServerRouteAdmission Admission,
        IReadOnlyDictionary<string, string> Headers,
        int ExpectedStatusCode);

    private sealed record PreBodyCase(
        string Name,
        string Method,
        string Path,
        string? ContentType,
        IReadOnlyDictionary<string, string> Headers,
        long? ContentLength,
        int ExpectedStatusCode);

    private sealed record StoreFailureCase(
        string Name,
        byte[] Bytes,
        string ExpectedSha256,
        int Width,
        int Height,
        string ExpectedErrorCode);

    private sealed class CountingReadStream(byte[] bytes) : Stream
    {
        private int _offset;

        public long BytesRead { get; private set; }
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => bytes.Length;
        public override long Position
        {
            get => _offset;
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = Math.Min(count, bytes.Length - _offset);
            if (read <= 0)
                return 0;

            Array.Copy(bytes, _offset, buffer, offset, read);
            _offset += read;
            BytesRead += read;
            return read;
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return ValueTask.FromCanceled<int>(cancellationToken);

            var read = Math.Min(buffer.Length, bytes.Length - _offset);
            if (read <= 0)
                return ValueTask.FromResult(0);

            bytes.AsSpan(_offset, read).CopyTo(buffer.Span);
            _offset += read;
            BytesRead += read;
            return ValueTask.FromResult(read);
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

    private sealed class GatedReadStream(byte[] bytes, TaskCompletionSource<bool> release) : Stream
    {
        private int _offset;
        private bool _released;

        public TaskCompletionSource<bool> FirstReadStarted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => bytes.Length;
        public override long Position
        {
            get => _offset;
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (!_released)
            {
                FirstReadStarted.TrySetResult(true);
                await release.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
                _released = true;
            }

            var read = Math.Min(buffer.Length, bytes.Length - _offset);
            if (read <= 0)
                return 0;

            bytes.AsSpan(_offset, read).CopyTo(buffer.Span);
            _offset += read;
            return read;
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

    private sealed class ThrowOnReadStream : Stream
    {
        public bool ReadAttempted { get; private set; }
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
            ReadAttempted = true;
            throw new InvalidOperationException("The request body must not be read for this handler path.");
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            ReadAttempted = true;
            throw new InvalidOperationException("The request body must not be read for this handler path.");
        }

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();
    }
}
