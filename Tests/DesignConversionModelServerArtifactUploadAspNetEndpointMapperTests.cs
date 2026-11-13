// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Buffers.Binary;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelServer.DesignConversion;

[Collection(DesignConversionLocalHostTestCollectionNames.CollectionName)]
public sealed class DesignConversionModelServerArtifactUploadAspNetEndpointMapperTests
{
    private static readonly DateTimeOffset FixedNow = DateTimeOffset.Parse("2026-07-11T12:00:00Z");
    private const string CurrentToken = "current-upload-aspnet-route-token-0123456789";
    private const string TenantId = "tenant-upload-aspnet-mapper-alpha";
    private const string UploadRouteId = "design_conversion_artifact_upload";

    [Fact]
    public async Task PostArtifactUpload_AspNetMapper_StoresRawPngAndAnnotatesMappedLoopbackBoundary()
    {
        var root = CreateTempRoot("success");
        try
        {
            await using var app = await StartUploadOnlyAppAsync(root);
            try
            {
                var png = BuildRgbaPng(6, 4, blank: false);
                var baseAddress = ResolveStartedAddress(app);
                using var client = CreateClient(baseAddress);
                using var content = new ByteArrayContent(png);
                content.Headers.ContentType = new MediaTypeHeaderValue("image/png");

                using var response = await SendUploadAsync(
                    client,
                    content,
                    png,
                    artifactId: "aspnet-valid-upload",
                    width: 6,
                    height: 4,
                    idempotencyKey: "aspnet-valid-upload-key",
                    unrelatedHeaderValue: new string('u', 8192));
                var bodyJson = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                AssertUploadMapperHeaders(response, expectedVerifiedSocket: true);
                using var document = JsonDocument.Parse(bodyJson);
                var body = document.RootElement;
                Assert.Equal(DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath, body.GetProperty("endpoint").GetString());
                Assert.Equal("POST", body.GetProperty("method").GetString());
                Assert.True(body.GetProperty("liveHttpRouteRegistered").GetBoolean());
                Assert.True(body.GetProperty("aspNetEndpointMapped").GetBoolean());
                Assert.False(body.GetProperty("deploymentVerified").GetBoolean());
                Assert.False(body.GetProperty("publicHttpEndpointVerified").GetBoolean());
                Assert.False(body.GetProperty("productReady").GetBoolean());
                Assert.False(body.GetProperty("rawBytesIncluded").GetBoolean());
                Assert.Equal(png.Length, body.GetProperty("byteCount").GetInt64());
                Assert.Equal("image/png", body.GetProperty("mediaType").GetString());
                Assert.Equal(Sha256(png), body.GetProperty("sha256").GetString());
                Assert.Equal("aspnet-valid-upload", body.GetProperty("artifactId").GetString());
                Assert.Equal("screenshot", body.GetProperty("kind").GetString());
                Assert.Equal(6, body.GetProperty("width").GetInt32());
                Assert.Equal(4, body.GetProperty("height").GetInt32());

                var artifactRef = JsonSerializer.Deserialize<DesignConversionModelServerArtifactRef>(
                    body.GetProperty("artifactRef").GetRawText(),
                    DesignConversionModelServerHttpHandler.JsonOptions);
                Assert.NotNull(artifactRef);
                var resolver = new DesignConversionServerOwnedLocalArtifactRefResolver(root);
                var resolved = resolver.Validate(artifactRef!, "ASP.NET upload mapper success");
                Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
                Assert.Single(PublishedPngFiles(root));
                Assert.Empty(TempFiles(root));

                var metadata = body.GetProperty("boundaryMetadata");
                AssertUploadMapperMetadata(metadata, expectedRouteVerified: true);
                Assert.Equal(bool.TrueString, metadata.GetProperty("upload_store_invoked").GetString());
                Assert.Equal(bool.FalseString, metadata.GetProperty("request_body_buffered").GetString());
                Assert.Equal(bool.FalseString, metadata.GetProperty("request_body_read_to_end").GetString());
                Assert.Equal(bool.FalseString, metadata.GetProperty("request_body_base64_encoded").GetString());
                Assert.Equal(bool.FalseString, metadata.GetProperty("raw_bytes_included").GetString());
                Assert.Equal(bool.FalseString, metadata.GetProperty("content_length_semantic_idempotency_included").GetString());
            }
            finally
            {
                await app.StopAsync();
            }
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Theory]
    [InlineData(DesignConversionModelServerRouteAdmission.AuthorizationHeaderName, 4089)]
    [InlineData(DesignConversionModelServerRouteAdmission.ProjectAuthorizationHeaderName, 4096)]
    public async Task PostArtifactUpload_AspNetMapper_DoesNotTransformSuffixedInvalidTokenIntoValidToken(
        string authHeaderName,
        int configuredTokenLength)
    {
        var useAuthorizationHeader = string.Equals(
            authHeaderName,
            DesignConversionModelServerRouteAdmission.AuthorizationHeaderName,
            StringComparison.Ordinal);
        var root = CreateTempRoot(useAuthorizationHeader ? "max-bearer-suffix" : "max-project-suffix");
        var configuredToken = new string('t', configuredTokenLength);
        const string attackerSuffix = "-attacker-suffix";
        try
        {
            Assert.InRange(configuredToken.Length, 16, 4096);
            await using var app = await StartUploadOnlyAppAsync(root, configuredToken);
            try
            {
                var png = BuildRgbaPng(4, 3, blank: false);
                var baseAddress = ResolveStartedAddress(app);
                using var client = CreateClient(baseAddress);
                using var content = new ByteArrayContent(png);
                content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath)
                {
                    Content = content
                };

                foreach (var pair in UploadHeaders(
                             png,
                             "aspnet-max-token-suffix",
                             width: 4,
                             height: 3,
                             idempotencyKey: "aspnet-max-token-suffix-key",
                             token: null,
                             includeAuthorization: false))
                {
                    Assert.True(request.Headers.TryAddWithoutValidation(pair.Key, pair.Value));
                }

                var validAuthValue = useAuthorizationHeader
                    ? "Bearer " + configuredToken
                    : configuredToken;
                var suppliedAuthValue = validAuthValue + attackerSuffix;
                Assert.Equal(4096, validAuthValue.Length);
                Assert.True(suppliedAuthValue.Length > 4096);
                Assert.True(request.Headers.TryAddWithoutValidation(authHeaderName, suppliedAuthValue));
                Assert.True(request.Headers.TryAddWithoutValidation("x-unrelated-oversized", new string('u', 8192)));

                using var response = await client.SendAsync(request);
                var bodyJson = await response.Content.ReadAsStringAsync();

                Assert.True(
                    response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden,
                    $"Expected bounded auth rejection, received {(int)response.StatusCode}.");
                Assert.Empty(PublishedPngFiles(root));
                Assert.Empty(TempFiles(root));
                Assert.DoesNotContain(configuredToken, bodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(attackerSuffix, bodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(root, bodyJson, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(Convert.ToBase64String(png), bodyJson, StringComparison.Ordinal);
                AssertUploadMapperHeaders(response, expectedVerifiedSocket: true);

                using var document = JsonDocument.Parse(bodyJson);
                var body = document.RootElement;
                var metadata = body.GetProperty("boundaryMetadata");
                AssertUploadMapperMetadata(metadata, expectedRouteVerified: false);
                Assert.Equal(bool.TrueString, metadata.GetProperty("live_http_route_registered").GetString());
                Assert.Equal(bool.TrueString, metadata.GetProperty("aspnet_endpoint_mapped").GetString());
                Assert.Equal(bool.FalseString, metadata.GetProperty("upload_store_invoked").GetString());
                Assert.Equal(bool.FalseString, metadata.GetProperty("token_value_exposed").GetString());
                Assert.Equal(bool.FalseString, metadata.GetProperty("token_hash_exposed").GetString());
            }
            finally
            {
                await app.StopAsync();
            }
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Theory]
    [InlineData("missing-auth", null, HttpStatusCode.Unauthorized)]
    [InlineData("invalid-auth", "wrong-upload-token", HttpStatusCode.Forbidden)]
    public async Task PostArtifactUpload_AspNetMapper_AuthFailuresRemainMappedAndLeakNoPrivateData(
        string caseName,
        string? token,
        HttpStatusCode expectedStatus)
    {
        var root = CreateTempRoot(caseName);
        try
        {
            await using var app = await StartUploadOnlyAppAsync(root);
            try
            {
                var png = BuildRgbaPng(4, 3, blank: false);
                var baseAddress = ResolveStartedAddress(app);
                using var client = CreateClient(baseAddress);
                using var content = new ByteArrayContent(png);
                content.Headers.ContentType = new MediaTypeHeaderValue("image/png");

                using var response = await SendUploadAsync(
                    client,
                    content,
                    png,
                    artifactId: "aspnet-auth-fail",
                    width: 4,
                    height: 3,
                    idempotencyKey: "aspnet-auth-fail-" + caseName,
                    token: token,
                    includeAuthorization: token is not null);
                var bodyJson = await response.Content.ReadAsStringAsync();

                Assert.Equal(expectedStatus, response.StatusCode);
                Assert.Empty(PublishedPngFiles(root));
                Assert.Empty(TempFiles(root));
                Assert.DoesNotContain(CurrentToken, bodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain("wrong-upload-token", bodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(root, bodyJson, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(Convert.ToBase64String(png), bodyJson, StringComparison.Ordinal);
                AssertUploadMapperHeaders(response, expectedVerifiedSocket: true);

                using var document = JsonDocument.Parse(bodyJson);
                var body = document.RootElement;
                Assert.Equal(DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath, body.GetProperty("endpoint").GetString());
                Assert.Equal((int)expectedStatus, body.GetProperty("statusCode").GetInt32());
                var metadata = body.GetProperty("boundaryMetadata");
                AssertUploadMapperMetadata(metadata, expectedRouteVerified: false);
                Assert.Equal(bool.TrueString, metadata.GetProperty("live_http_route_registered").GetString());
                Assert.Equal(bool.TrueString, metadata.GetProperty("aspnet_endpoint_mapped").GetString());
                Assert.Equal(bool.FalseString, metadata.GetProperty("upload_store_invoked").GetString());
                Assert.Equal(bool.FalseString, metadata.GetProperty("token_value_exposed").GetString());
                Assert.Equal(bool.FalseString, metadata.GetProperty("token_hash_exposed").GetString());
            }
            finally
            {
                await app.StopAsync();
            }
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task PostArtifactUpload_AspNetMapper_ChunkedUnknownLengthSucceedsWithoutRequiringBuffering()
    {
        var root = CreateTempRoot("chunked");
        try
        {
            await using var app = await StartUploadOnlyAppAsync(root);
            try
            {
                var png = BuildRgbaPng(5, 3, blank: false);
                var baseAddress = ResolveStartedAddress(app);
                using var client = CreateClient(baseAddress);
                using var content = new UnknownLengthByteArrayContent(png);
                content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                Assert.Null(content.Headers.ContentLength);

                using var response = await SendUploadAsync(
                    client,
                    content,
                    png,
                    artifactId: "aspnet-chunked-upload",
                    width: 5,
                    height: 3,
                    idempotencyKey: "aspnet-chunked-upload-key");
                var bodyJson = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                using var document = JsonDocument.Parse(bodyJson);
                var body = document.RootElement;
                Assert.True(body.GetProperty("liveHttpRouteRegistered").GetBoolean());
                Assert.True(body.GetProperty("aspNetEndpointMapped").GetBoolean());
                Assert.Equal(png.Length, body.GetProperty("byteCount").GetInt64());
                Assert.Single(PublishedPngFiles(root));
                Assert.Empty(TempFiles(root));

                var metadata = body.GetProperty("boundaryMetadata");
                AssertUploadMapperMetadata(metadata, expectedRouteVerified: true);
                Assert.Equal(bool.FalseString, metadata.GetProperty("request_body_buffered").GetString());
                Assert.Equal(bool.FalseString, metadata.GetProperty("request_body_read_to_end").GetString());
                Assert.Equal(bool.FalseString, metadata.GetProperty("request_body_base64_encoded").GetString());
            }
            finally
            {
                await app.StopAsync();
            }
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public void UploadDispatcher_SourceUsesRequestBodyDirectlyAndAvoidsJsonBodyAdapter()
    {
        var source = File.ReadAllText(MapperPath());
        var dispatch = ExtractSourceSlice(
            source,
            "private static async Task DispatchArtifactUploadAsync",
            "private static async Task WriteResponseAsync");

        Assert.Contains("Body: context.Request.Body", dispatch, StringComparison.Ordinal);
        Assert.Contains("handler.HandleAsync(request, context.RequestAborted)", dispatch, StringComparison.Ordinal);
        Assert.DoesNotContain("ToModelServerRequestAsync", dispatch, StringComparison.Ordinal);
        Assert.DoesNotContain("StreamReader", dispatch, StringComparison.Ordinal);
        Assert.DoesNotContain("ReadToEnd", dispatch, StringComparison.Ordinal);
        Assert.DoesNotContain("MemoryStream", dispatch, StringComparison.Ordinal);
        Assert.DoesNotContain("ToArray", dispatch, StringComparison.Ordinal);
        Assert.DoesNotContain("base64", dispatch, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".Read(", dispatch, StringComparison.Ordinal);

        var uploadHeaderCopy = ExtractSourceSlice(
            source,
            "private static IReadOnlyDictionary<string, string> ToArtifactUploadHeaderDictionary",
            "private static DesignConversionModelServerHttpResponse AnnotateAspNetHost");
        Assert.Contains("result[pair.Key] = pair.Value.ToString();", uploadHeaderCopy, StringComparison.Ordinal);
        Assert.DoesNotContain("..", uploadHeaderCopy, StringComparison.Ordinal);

        var mapping = ExtractSourceSlice(
            source,
            "public static IEndpointConventionBuilder MapDesignConversionModelServerArtifactUpload",
            "private static IEndpointConventionBuilder MapDesignConversionModelServerHumanApprovalEndpoint");
        Assert.Contains("endpoints.MapPost(", mapping, StringComparison.Ordinal);
        Assert.Contains("DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath", mapping, StringComparison.Ordinal);
    }

    private static async Task<WebApplication> StartUploadOnlyAppAsync(
        string root,
        string currentToken = CurrentToken)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        var app = builder.Build();
        app.MapDesignConversionModelServerArtifactUpload(CreateHandler(root, currentToken));
        await app.StartAsync();
        return app;
    }

    private static DesignConversionModelServerArtifactUploadHttpHandler CreateHandler(
        string root,
        string currentToken)
        => new(
            new DesignConversionModelServerArtifactUploadStore(root, maxBytes: 1024 * 1024),
            DesignConversionModelServerRouteAdmission.Create(
                new DesignConversionModelServerRouteAdmissionOptions(
                    enabled: true,
                    currentToken: currentToken,
                    tenantId: TenantId),
                () => FixedNow));

    private static HttpClient CreateClient(Uri baseAddress)
        => new()
        {
            BaseAddress = baseAddress,
            Timeout = TimeSpan.FromSeconds(10)
        };

    private static async Task<HttpResponseMessage> SendUploadAsync(
        HttpClient client,
        HttpContent content,
        byte[] png,
        string artifactId,
        int width,
        int height,
        string idempotencyKey,
        string? token = CurrentToken,
        bool includeAuthorization = true,
        string? unrelatedHeaderValue = null)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath)
        {
            Content = content
        };

        if (unrelatedHeaderValue is not null)
            Assert.True(request.Headers.TryAddWithoutValidation("x-unrelated-oversized", unrelatedHeaderValue));

        foreach (var pair in UploadHeaders(png, artifactId, width, height, idempotencyKey, token, includeAuthorization))
            request.Headers.TryAddWithoutValidation(pair.Key, pair.Value);

        return await client.SendAsync(request);
    }

    private static Dictionary<string, string> UploadHeaders(
        byte[] bytes,
        string artifactId,
        int width,
        int height,
        string idempotencyKey,
        string? token,
        bool includeAuthorization)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [DesignConversionModelServerArtifactUploadHttpHandler.ArtifactIdHeaderName] = artifactId,
            [DesignConversionModelServerArtifactUploadHttpHandler.ArtifactKindHeaderName] = "screenshot",
            [DesignConversionModelServerArtifactUploadHttpHandler.ExpectedSha256HeaderName] = Sha256(bytes),
            [DesignConversionModelServerArtifactUploadHttpHandler.WidthHeaderName] = width.ToString(System.Globalization.CultureInfo.InvariantCulture),
            [DesignConversionModelServerArtifactUploadHttpHandler.HeightHeaderName] = height.ToString(System.Globalization.CultureInfo.InvariantCulture),
            [DesignConversionModelServerRouteAdmission.IdempotencyHeaderName] = idempotencyKey
        };

        if (includeAuthorization)
            headers[DesignConversionModelServerRouteAdmission.AuthorizationHeaderName] = "Bearer " + token;

        return headers;
    }

    private static void AssertUploadMapperHeaders(
        HttpResponseMessage response,
        bool expectedVerifiedSocket)
    {
        Assert.Equal(
            nameof(DesignConversionModelServerAspNetEndpointMapper),
            response.Headers.GetValues("x-design-conversion-asp-net-endpoint-mapper").Single());
        Assert.Equal(
            DesignConversionModelServerAspNetEndpointMapper.AspNetEndpointMapperBoundary,
            response.Headers.GetValues("x-design-conversion-asp-net-boundary").Single());
        Assert.Equal(
            bool.TrueString,
            response.Headers.GetValues("x-design-conversion-asp-net-host-verified").Single());
        Assert.Equal(
            DesignConversionModelServerAspNetEndpointMapper.AspNetHostKind,
            response.Headers.GetValues("x-design-conversion-asp-net-host-kind").Single());
        Assert.Equal(
            DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath,
            response.Headers.GetValues("x-design-conversion-asp-net-route").Single());
        Assert.Equal(
            bool.TrueString,
            response.Headers.GetValues("x-design-conversion-live-route-registered").Single());
        Assert.Equal(
            bool.TrueString,
            response.Headers.GetValues("x-design-conversion-aspnet-endpoint-mapped").Single());
        Assert.Equal(
            UploadRouteId,
            response.Headers.GetValues("x-design-conversion-asp-net-route-id").Single());
        Assert.Equal(
            nameof(DesignConversionModelServerArtifactUploadHttpHandler),
            response.Headers.GetValues("x-design-conversion-asp-net-handler-dispatch-target").Single());
        Assert.Equal(
            bool.FalseString,
            response.Headers.GetValues("x-design-conversion-public-deployment-verified").Single());
        Assert.Equal(
            bool.FalseString,
            response.Headers.GetValues("x-design-conversion-product-ready").Single());
        Assert.Equal(
            expectedVerifiedSocket.ToString(),
            response.Headers.GetValues("x-design-conversion-socket-opened").Single());
    }

    private static void AssertUploadMapperMetadata(
        JsonElement metadata,
        bool expectedRouteVerified)
    {
        Assert.Equal(
            nameof(DesignConversionModelServerAspNetEndpointMapper),
            metadata.GetProperty("asp_net_endpoint_mapper").GetString());
        Assert.Equal(
            DesignConversionModelServerAspNetEndpointMapper.AspNetEndpointMapperBoundary,
            metadata.GetProperty("asp_net_endpoint_mapper_boundary").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_host_verified").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_minimal_api_pipeline_verified").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_route_matched").GetString());
        Assert.Equal(DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath, metadata.GetProperty("asp_net_route_path").GetString());
        Assert.Equal(UploadRouteId, metadata.GetProperty("asp_net_route_id").GetString());
        Assert.Equal(expectedRouteVerified.ToString(), metadata.GetProperty("asp_net_route_verified").GetString());
        Assert.Equal(
            expectedRouteVerified
                ? DesignConversionModelServerAspNetEndpointMapper.AspNetRouteVerifiedState
                : "not_verified_server_owned_loopback_route_dispatch",
            metadata.GetProperty("asp_net_route_verification_state").GetString());
        Assert.Equal(UploadRouteId, metadata.GetProperty("asp_net_route_verification_route_id").GetString());
        Assert.Equal(
            DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath,
            metadata.GetProperty("asp_net_route_verification_route_path").GetString());
        Assert.Equal(
            expectedRouteVerified.ToString(),
            metadata.GetProperty("asp_net_route_verification_successful_handler_dispatch").GetString());
        Assert.Equal(
            nameof(DesignConversionModelServerArtifactUploadHttpHandler),
            metadata.GetProperty("asp_net_handler_dispatch_target").GetString());
        Assert.Equal(DesignConversionModelServerAspNetEndpointMapper.AspNetHostKind, metadata.GetProperty("asp_net_host_kind").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("live_http_route_registered").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("aspnet_endpoint_mapped").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("localhost_loopback_only").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("socket_opened").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_kestrel_socket_opened").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("local_loopback_http_call_executed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("public_http_endpoint_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("cross_repo_caller_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("deployment_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("public_production_default_ready").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
    }

    private static Uri ResolveStartedAddress(WebApplication app)
    {
        var addresses = app.Services
            .GetRequiredService<IServer>()
            .Features
            .Get<IServerAddressesFeature>()?
            .Addresses
            .ToArray() ?? [];
        var address = Assert.Single(addresses);
        return new Uri(address);
    }

    private static string CreateTempRoot(string name)
    {
        var safeName = new string(name.Select(ch => char.IsAsciiLetterOrDigit(ch) || ch is '-' or '_' ? ch : '-').ToArray());
        var root = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "design-conversion-upload-aspnet-mapper-tests",
            $"{safeName}-{Guid.NewGuid():N}"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static void DeleteTempRoot(string root)
    {
        if (!Directory.Exists(root))
            return;

        var fullRoot = Path.GetFullPath(root);
        var allowedRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "design-conversion-upload-aspnet-mapper-tests"));
        if (!fullRoot.StartsWith(allowedRoot, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            return;

        Directory.Delete(fullRoot, recursive: true);
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

    private static string MapperPath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(
                directory.FullName,
                "25.ModelServer",
                "DesignConversion",
                "DesignConversionModelServerAspNetEndpointMapper.cs");
            if (File.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate DesignConversionModelServerAspNetEndpointMapper.cs from the test output directory.");
    }

    private static string ExtractSourceSlice(string source, string startMarker, string endMarker)
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        Assert.True(start >= 0, "Expected source start marker was not found: " + startMarker);
        var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
        Assert.True(end > start, "Expected source end marker was not found: " + endMarker);
        return source[start..end];
    }

    private sealed class UnknownLengthByteArrayContent(byte[] bytes) : HttpContent
    {
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
            => stream.WriteAsync(bytes, 0, bytes.Length);

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }
}
