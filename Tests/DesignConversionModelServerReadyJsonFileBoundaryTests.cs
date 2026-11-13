// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Net;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;
using ModelServer.DesignConversion;

public sealed class DesignConversionModelServerReadyJsonFileBoundaryTests
{
    private const string FileTooLargeErrorCode = "READY_JSON_FILE_TOO_LARGE";
    private const string FileTooLargeMessage =
        "DesignConversion live-local ready.json file is too large to read.";
    private const string ReadFailedErrorCode = "READY_JSON_READ_FAILED";
    private const string ReadFailedMessage =
        "DesignConversion live-local ready.json could not be read.";

    [Fact]
    public async Task TrySubmitReadyFileAsync_OversizedKnownLengthFile_ReturnsBoundedNonReadyWithoutHttpOrPrivateMetadata()
    {
        const string privateMarker = "PRIVATE_READY_JSON_FILE_OVERSIZE_SECRET";
        using var temp = TemporaryDirectory.Create();
        var readyPath = System.IO.Path.Combine(temp.Path, privateMarker + ".ready.json");
        await File.WriteAllTextAsync(
            readyPath,
            new string(' ', CalculatedMaxReadyJsonFileBytes + 1),
            Encoding.UTF8);
        var handler = new CapturingSubmitHandler(SuccessResponseJson("oversized-ready-json-file"));
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);
        var package = CreatePackage("oversized-ready-json-file", privateMarker);

        var result = await client.TrySubmitReadyFileAsync(readyPath, package);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var submitted = result.Value!;
        Assert.Equal(0, handler.SendCount);
        Assert.Null(submitted.BaseUrl);
        Assert.Null(submitted.RequestUri);
        Assert.False(submitted.ReadyJsonBaseUrlConfigured);
        Assert.False(submitted.ProductReady);
        Assert.Equal("error", submitted.Envelope.EnvelopeKind);
        Assert.Equal(400, submitted.Envelope.StatusCode);
        Assert.False(submitted.Envelope.RegistryInvokeSucceeded);
        Assert.False(submitted.Envelope.RuntimeInvoked);
        Assert.Equal(FileTooLargeErrorCode, submitted.Envelope.ParsedErrorCode);
        Assert.Equal(FileTooLargeMessage, submitted.Envelope.ParsedErrorMessage);
        Assert.Equal(FileTooLargeErrorCode, submitted.Envelope.BoundaryMetadata["ready_json_error_code"]);
        Assert.Equal(FileTooLargeMessage, submitted.Envelope.BoundaryMetadata["ready_json_error_message"]);
        Assert.Equal(bool.FalseString, submitted.Envelope.BoundaryMetadata["ready_json_submit_http_request_sent"]);
        Assert.Equal(bool.TrueString, submitted.Envelope.BoundaryMetadata["ready_json_submit_skipped"]);
        AssertNoPrivateLeak(result, privateMarker, temp.Path, readyPath);
        Assert.Equal(CalculatedMaxReadyJsonFileBytes, ReadPublicMaxReadyJsonFileBytes());
    }

    [Fact]
    public async Task TrySubmitReadyFileAsync_DirectoryPathWithPrivateMarker_ReturnsStableReadFailedWithoutPathLeak()
    {
        const string privateMarker = "PRIVATE_READY_JSON_DIRECTORY_READ_FAILED_SECRET";
        using var temp = TemporaryDirectory.Create();
        var directoryPath = System.IO.Path.Combine(temp.Path, privateMarker);
        Directory.CreateDirectory(directoryPath);
        var handler = new CapturingSubmitHandler(SuccessResponseJson("directory-ready-json-file"));
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);
        var package = CreatePackage("directory-ready-json-file", privateMarker);

        var result = await client.TrySubmitReadyFileAsync(directoryPath, package);

        Assert.False(result.IsSuccess);
        Assert.Equal(ReadFailedErrorCode, result.ErrorCode);
        Assert.Equal(ReadFailedMessage, result.ErrorMessage);
        Assert.Equal(0, handler.SendCount);
        AssertNoPrivateLeak(result, privateMarker, temp.Path, directoryPath);
    }

    [Fact]
    public async Task TrySubmitReadyFileAsync_AlreadyCancelledToken_ReturnsCancelledWithoutHttpOrException()
    {
        const string privateMarker = "PRIVATE_READY_JSON_CANCELLED_FILE_SECRET";
        using var temp = TemporaryDirectory.Create();
        var readyPath = await WriteReadyJsonAsync(temp.Path, privateMarker, BuildReadyJson("http://127.0.0.1:54321"));
        var handler = new CapturingSubmitHandler(SuccessResponseJson("cancelled-ready-json-file"));
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);
        var package = CreatePackage("cancelled-ready-json-file", privateMarker);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = await client.TrySubmitReadyFileAsync(
            readyPath,
            package,
            cancellationToken: cancellation.Token);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.SubmitCancelledErrorCode, result.ErrorCode);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.SubmitCancelledMessage, result.ErrorMessage);
        Assert.Equal(0, handler.SendCount);
        AssertNoPrivateLeak(result, privateMarker, temp.Path, readyPath);
    }

    [Fact]
    public async Task TrySubmitReadyFileAsync_SendTaskCanceledWithoutCallerCancellation_ReturnsTimeoutWithoutPrivateMetadata()
    {
        const string privateMarker = "PRIVATE_READY_JSON_SUBMIT_TIMEOUT_SECRET";
        using var temp = TemporaryDirectory.Create();
        var readyPath = await WriteReadyJsonAsync(temp.Path, privateMarker, BuildReadyJson("http://127.0.0.1:54321"));
        var handler = new ThrowingSubmitHandler(
            _ => new TaskCanceledException(privateMarker + " " + temp.Path + " " + readyPath + " http://127.0.0.1:54321"));
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);
        var package = CreatePackage("timeout-ready-json-file", privateMarker);

        var result = await client.TrySubmitReadyFileAsync(readyPath, package);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.SubmitTimeoutErrorCode, result.ErrorCode);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.SubmitTimeoutMessage, result.ErrorMessage);
        Assert.Equal(1, handler.SendCount);
        AssertNoPrivateLeak(result, privateMarker, temp.Path, readyPath, "127.0.0.1:54321");
    }

    [Fact]
    public async Task TrySubmitReadyFileAsync_SendOperationCanceledWithoutCallerCancellation_ReturnsTimeoutWithoutPrivateMetadata()
    {
        const string privateMarker = "PRIVATE_READY_JSON_SUBMIT_OPERATION_CANCELLED_SECRET";
        using var temp = TemporaryDirectory.Create();
        var readyPath = await WriteReadyJsonAsync(temp.Path, privateMarker, BuildReadyJson("http://127.0.0.1:54321"));
        var handler = new ThrowingSubmitHandler(
            _ => new OperationCanceledException(privateMarker + " " + temp.Path + " " + readyPath + " http://127.0.0.1:54321"));
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);
        var package = CreatePackage("operation-cancelled-ready-json-file", privateMarker);

        var result = await client.TrySubmitReadyFileAsync(readyPath, package);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.SubmitTimeoutErrorCode, result.ErrorCode);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.SubmitTimeoutMessage, result.ErrorMessage);
        Assert.Equal(1, handler.SendCount);
        AssertNoPrivateLeak(result, privateMarker, temp.Path, readyPath, "127.0.0.1:54321");
    }

    [Fact]
    public async Task TrySubmitReadyFileAsync_SendTaskCanceledAfterCallerCancellation_ReturnsCancelled()
    {
        const string privateMarker = "PRIVATE_READY_JSON_SUBMIT_CALLER_CANCELLED_SECRET";
        using var temp = TemporaryDirectory.Create();
        var readyPath = await WriteReadyJsonAsync(temp.Path, privateMarker, BuildReadyJson("http://127.0.0.1:54321"));
        using var cancellation = new CancellationTokenSource();
        var handler = new ThrowingSubmitHandler(
            _ => new TaskCanceledException(privateMarker + " " + temp.Path + " " + readyPath),
            cancellation.Cancel);
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);
        var package = CreatePackage("caller-cancelled-ready-json-file", privateMarker);

        var result = await client.TrySubmitReadyFileAsync(
            readyPath,
            package,
            cancellationToken: cancellation.Token);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.SubmitCancelledErrorCode, result.ErrorCode);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.SubmitCancelledMessage, result.ErrorMessage);
        Assert.Equal(1, handler.SendCount);
        AssertNoPrivateLeak(result, privateMarker, temp.Path, readyPath);
    }

    [Fact]
    public async Task TrySubmitReadyFileAsync_SendHttpRequestException_ReturnsGenericHttpFailedWithoutPrivateMetadata()
    {
        const string privateMarker = "PRIVATE_READY_JSON_SUBMIT_HTTP_FAILED_SECRET";
        using var temp = TemporaryDirectory.Create();
        var readyPath = await WriteReadyJsonAsync(temp.Path, privateMarker, BuildReadyJson("http://127.0.0.1:54321"));
        var handler = new ThrowingSubmitHandler(
            _ => new HttpRequestException(privateMarker + " " + temp.Path + " " + readyPath + " http://127.0.0.1:54321"));
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);
        var package = CreatePackage("http-failed-ready-json-file", privateMarker);

        var result = await client.TrySubmitReadyFileAsync(readyPath, package);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.SubmitHttpFailedErrorCode, result.ErrorCode);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.SubmitHttpFailedMessage, result.ErrorMessage);
        Assert.Equal(1, handler.SendCount);
        AssertNoPrivateLeak(result, privateMarker, temp.Path, readyPath, "127.0.0.1:54321");
    }

    [Fact]
    public async Task TrySubmitReadyFileAsync_SendInvalidOperationException_ReturnsGenericHttpFailedWithoutPrivateMetadata()
    {
        const string privateMarker = "PRIVATE_READY_JSON_SUBMIT_INVALID_OPERATION_SECRET";
        using var temp = TemporaryDirectory.Create();
        var readyPath = await WriteReadyJsonAsync(temp.Path, privateMarker, BuildReadyJson("http://127.0.0.1:54321"));
        var handler = new ThrowingSubmitHandler(
            _ => new InvalidOperationException(privateMarker + " " + temp.Path + " " + readyPath + " http://127.0.0.1:54321"));
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);
        var package = CreatePackage("invalid-operation-ready-json-file", privateMarker);

        var result = await client.TrySubmitReadyFileAsync(readyPath, package);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.SubmitHttpFailedErrorCode, result.ErrorCode);
        Assert.Equal(DesignConversionModelServerReadyJsonSubmitClient.SubmitHttpFailedMessage, result.ErrorMessage);
        Assert.Equal(1, handler.SendCount);
        AssertNoPrivateLeak(result, privateMarker, temp.Path, readyPath, "127.0.0.1:54321");
    }

    [Fact]
    public async Task TrySubmitReadyFileAsync_ExactlyMaxReadyJsonCharsValidJson_ReachesHttpSubmission()
    {
        const string privateMarker = "PRIVATE_READY_JSON_EXACT_MAX_FILE_SECRET";
        const string requestId = "exact-max-ready-json-file";
        using var temp = TemporaryDirectory.Create();
        var readyJson = BuildExactMaxReadyJson();
        Assert.Equal(DesignConversionModelServerPayloadSizeGuard.MaxReadyJsonChars, readyJson.Length);
        var readyPath = await WriteReadyJsonAsync(temp.Path, privateMarker, readyJson);
        var handler = new CapturingSubmitHandler(SuccessResponseJson(requestId));
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);
        var package = CreatePackage(requestId, privateMarker);

        var result = await client.TrySubmitReadyFileAsync(readyPath, package);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var submitted = result.Value!;
        Assert.Equal(1, handler.SendCount);
        Assert.Equal(HttpMethod.Post.Method, handler.CapturedMethod);
        Assert.Equal(
            new Uri("http://127.0.0.1:54321" + DesignConversionModelServerEndpointRegistry.EndpointPath),
            handler.CapturedRequestUri);
        Assert.True(submitted.ReadyJsonBaseUrlConfigured);
        Assert.True(submitted.ReadyJsonBaseUrlLoopback);
        Assert.True(submitted.ReadyJsonAdvertisedSubmitRoute);
        Assert.False(submitted.ProductReady);
        Assert.Equal(requestId, submitted.Envelope.ParsedRequestId);
        using var capturedBody = JsonDocument.Parse(handler.CapturedBodyJson!);
        Assert.Equal(requestId, capturedBody.RootElement.GetProperty("requestId").GetString());
    }

    [Fact]
    public async Task TrySubmitReadyFileAsync_MalformedUtf8InsideIgnoredString_ReturnsReadFailedWithoutHttpOrByteEcho()
    {
        const string privateMarker = "PRIVATE_READY_JSON_MALFORMED_UTF8_SECRET";
        using var temp = TemporaryDirectory.Create();
        var readyPath = System.IO.Path.Combine(temp.Path, privateMarker + ".ready.json");
        var prefix = Encoding.UTF8.GetBytes(
            "{ \"baseUrl\": \"http://127.0.0.1:54321\", \"ignored\": \"safe-before ");
        var suffix = Encoding.UTF8.GetBytes(" safe-after\" }");
        await File.WriteAllBytesAsync(readyPath, [.. prefix, 0xC3, 0x28, .. suffix]);
        var handler = new CapturingSubmitHandler(SuccessResponseJson("malformed-utf8-ready-json-file"));
        using var httpClient = new HttpClient(handler);
        var client = new DesignConversionModelServerReadyJsonSubmitClient(httpClient);
        var package = CreatePackage("malformed-utf8-ready-json-file", privateMarker);

        var result = await client.TrySubmitReadyFileAsync(readyPath, package);

        Assert.False(result.IsSuccess);
        Assert.Equal(ReadFailedErrorCode, result.ErrorCode);
        Assert.Equal(ReadFailedMessage, result.ErrorMessage);
        Assert.Equal(0, handler.SendCount);
        AssertNoPrivateLeak(
            result,
            privateMarker,
            temp.Path,
            readyPath,
            "C3",
            "0xC3",
            nameof(DecoderFallbackException),
            "Unable to translate bytes");
    }

    private static int CalculatedMaxReadyJsonFileBytes
        => (DesignConversionModelServerPayloadSizeGuard.MaxReadyJsonChars * 4) + 3;

    private static int ReadPublicMaxReadyJsonFileBytes()
    {
        var field = typeof(DesignConversionModelServerReadyJsonSubmitClient)
            .GetField("MaxReadyJsonFileBytes");
        Assert.NotNull(field);
        return (int)field!.GetRawConstantValue()!;
    }

    private static async Task<string> WriteReadyJsonAsync(
        string root,
        string privateMarker,
        string readyJson)
    {
        var readyPath = System.IO.Path.Combine(root, privateMarker + ".ready.json");
        await File.WriteAllTextAsync(readyPath, readyJson, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        return readyPath;
    }

    private static string BuildExactMaxReadyJson()
    {
        var baseJson = BuildReadyJson("http://127.0.0.1:54321");
        Assert.True(baseJson.Length <= DesignConversionModelServerPayloadSizeGuard.MaxReadyJsonChars);
        return baseJson + new string(
            ' ',
            DesignConversionModelServerPayloadSizeGuard.MaxReadyJsonChars - baseJson.Length);
    }

    private static string BuildReadyJson(string baseUrl)
        => "{ \"baseUrl\": \"" + baseUrl + "\" }";

    private static DesignConversionModelServerLocalCallerPackage CreatePackage(
        string requestId,
        string privateMarker)
        => new(
            RequestId: requestId,
            SourceKind: nameof(DesignConversionSourceKind.Figma),
            DesignTree: new DesignConversionModelServerPayloadRef(
                RefId: "ready-json-file-boundary-design-tree",
                Kind: "figma-json",
                Sha256: "sha256:ready-json-file-boundary-design-tree",
                Payload: "{\"kind\":\"design\"}",
                Format: "figma-json"),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: "ready-json-file-boundary-original-tree",
                Kind: "figma-json",
                Sha256: "sha256:ready-json-file-boundary-original-tree",
                Payload: "{\"kind\":\"original\"}",
                Format: "figma-json"),
            Html: "<main data-private=\"" + privateMarker + "\">ready file boundary</main>",
            Css: ".ready-file-boundary{display:block;}",
            Screenshots:
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: "ready-json-file-boundary-screenshot",
                    Kind: "screenshot",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://ready-json-file-boundary/screenshot.png",
                    Sha256: "sha256:ready-json-file-boundary-screenshot")
            ],
            DesignRenders:
            [
                new DesignConversionModelServerArtifactRef(
                    ArtifactId: "ready-json-file-boundary-render",
                    Kind: "design-render",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://ready-json-file-boundary/render.png",
                    Sha256: "sha256:ready-json-file-boundary-render")
            ]);

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

    private static void AssertNoPrivateLeak(
        OperationResult<DesignConversionModelServerReadyJsonSubmitResult> result,
        params string[] privateValues)
    {
        var visible = VisibleResultText(result);
        foreach (var privateValue in privateValues.Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            Assert.DoesNotContain(privateValue, visible, StringComparison.Ordinal);
        }
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

    private sealed class ThrowingSubmitHandler(
        Func<CancellationToken, Exception> createException,
        Action? beforeThrow = null) : HttpMessageHandler
    {
        public int SendCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            SendCount++;
            beforeThrow?.Invoke();
            throw createException(cancellationToken);
        }
    }

    private sealed class CapturingSubmitHandler(string responseJson) : HttpMessageHandler
    {
        public int SendCount { get; private set; }
        public string? CapturedMethod { get; private set; }
        public Uri? CapturedRequestUri { get; private set; }
        public string? CapturedBodyJson { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            SendCount++;
            CapturedMethod = request.Method.Method;
            CapturedRequestUri = request.RequestUri;
            CapturedBodyJson = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };
        }
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
                "DesignConversionModelServerReadyJsonFileBoundaryTests",
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
                // Best-effort cleanup for temp files left open by failed assertions.
            }
        }
    }
}
