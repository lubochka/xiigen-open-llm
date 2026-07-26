// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Net;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ModelServer.DesignConversion;

public sealed class DesignConversionModelServerAspNetRequestBodyLimitTests
{
    private const int ExpectedJsonRequestBodyLimitBytes = 8 * 1024 * 1024;
    private const string ExpectedOversizeErrorCode = "REQUEST_BODY_TOO_LARGE";
    private const string ExpectedOversizeErrorMessage = "DesignConversion ASP.NET JSON request body is too large.";

    [Fact]
    public async Task OrdinaryJsonRoute_KnownOversizedContentLength_Returns413WithoutReadingBodyOrEchoingPrivateData()
    {
        const string privateMarker = "PRIVATE_ASPNET_JSON_KNOWN_LENGTH_OVERSIZE";
        var stream = new CountingReadStream(Encoding.UTF8.GetBytes("{\"private\":\"" + privateMarker + "\"}"));
        await using var app = CreateEvaluateMapperApp(CreateEvaluateRegistry());
        var endpoint = ResolveEndpoint(app, DesignConversionModelServerEndpointRegistry.EndpointPath);
        var context = CreateJsonPostContext(
            DesignConversionModelServerEndpointRegistry.EndpointPath,
            stream,
            contentLength: ExpectedJsonRequestBodyLimitBytes + 1);

        await endpoint.RequestDelegate!(context);
        var bodyJson = await ReadResponseBodyAsync(context);

        Assert.Equal(StatusCodes.Status413PayloadTooLarge, context.Response.StatusCode);
        Assert.Equal(0, stream.ReadCallCount);
        Assert.Equal(0, stream.BytesRead);
        AssertOversizeResponseContract(
            context,
            bodyJson,
            DesignConversionModelServerEndpointRegistry.EndpointPath,
            privateMarker);
    }

    [Fact]
    public async Task OrdinaryJsonRoute_UnknownLengthOversizedBody_Returns413AfterBoundedReadWithoutHandlerErrorOrPrivateEcho()
    {
        const string privateMarker = "PRIVATE_ASPNET_JSON_CHUNKED_OVERSIZE";
        var bodyBytes = Encoding.UTF8.GetBytes(
            "{\"padding\":\"" +
            new string('a', ExpectedJsonRequestBodyLimitBytes + 64 * 1024) +
            privateMarker +
            "\"}");
        var stream = new CountingReadStream(bodyBytes);
        await using var app = CreateEvaluateMapperApp(CreateEvaluateRegistry());
        var endpoint = ResolveEndpoint(app, DesignConversionModelServerEndpointRegistry.EndpointPath);
        var context = CreateJsonPostContext(
            DesignConversionModelServerEndpointRegistry.EndpointPath,
            stream,
            contentLength: null);

        await endpoint.RequestDelegate!(context);
        var bodyJson = await ReadResponseBodyAsync(context);

        Assert.Equal(StatusCodes.Status413PayloadTooLarge, context.Response.StatusCode);
        Assert.True(stream.ReadCallCount > 0);
        Assert.True(
            stream.BytesRead <= ExpectedJsonRequestBodyLimitBytes + 1,
            $"Expected bounded server reads <= limit + 1 byte, observed {stream.BytesRead}.");
        Assert.True(
            stream.BytesRead < bodyBytes.Length,
            $"Expected server to stop before buffering the entire oversized body, observed {stream.BytesRead} of {bodyBytes.Length}.");
        AssertOversizeResponseContract(
            context,
            bodyJson,
            DesignConversionModelServerEndpointRegistry.EndpointPath,
            privateMarker);
    }

    [Fact]
    public async Task OrdinaryJsonRoute_ExactLimitValidJsonObject_ReachesRegisteredHandlerAndIsNotTransportRejected()
    {
        var bodyJson = CreateExactLimitJsonObject();
        var bodyBytes = Encoding.UTF8.GetBytes(bodyJson);
        var stream = new CountingReadStream(bodyBytes);
        await using var app = CreateEvaluateMapperApp(CreateEvaluateRegistry());
        var endpoint = ResolveEndpoint(app, DesignConversionModelServerEndpointRegistry.EndpointPath);
        var context = CreateJsonPostContext(
            DesignConversionModelServerEndpointRegistry.EndpointPath,
            stream,
            contentLength: bodyBytes.Length);

        await endpoint.RequestDelegate!(context);
        var responseBodyJson = await ReadResponseBodyAsync(context);

        Assert.Equal(ExpectedJsonRequestBodyLimitBytes, bodyBytes.Length);
        Assert.NotEqual(StatusCodes.Status413PayloadTooLarge, context.Response.StatusCode);
        Assert.Equal(bodyBytes.Length, stream.BytesRead);
        using var document = JsonDocument.Parse(responseBodyJson);
        var root = document.RootElement;
        Assert.Equal("REQUEST_ID_REQUIRED", root.GetProperty("errorCode").GetString());
        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_registry_dispatch_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
    }

    [Fact]
    public async Task OrdinaryJsonRoute_UnknownLengthOversizedBody_CarriesRefsOnlyReadinessFalseBoundary()
    {
        const string privateMarker = "PRIVATE_ASPNET_JSON_PRIVACY_OVERSIZE";
        var bodyBytes = Encoding.UTF8.GetBytes(
            "{\"token\":\"secret-token-value\",\"padding\":\"" +
            privateMarker +
            new string('z', ExpectedJsonRequestBodyLimitBytes + 16) +
            "\"}");
        var stream = new CountingReadStream(bodyBytes);
        await using var app = CreateEvaluateMapperApp(CreateEvaluateRegistry());
        var endpoint = ResolveEndpoint(app, DesignConversionModelServerEndpointRegistry.EndpointPath);
        var context = CreateJsonPostContext(
            DesignConversionModelServerEndpointRegistry.EndpointPath,
            stream,
            contentLength: null);

        await endpoint.RequestDelegate!(context);
        var bodyJson = await ReadResponseBodyAsync(context);

        AssertOversizeResponseContract(
            context,
            bodyJson,
            DesignConversionModelServerEndpointRegistry.EndpointPath,
            privateMarker,
            "secret-token-value");
    }

    private static WebApplication CreateEvaluateMapperApp(DesignConversionModelServerEndpointRegistry registry)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = [] });
        var app = builder.Build();
        app.MapDesignConversionModelServerEvaluate(registry);
        return app;
    }

    private static DesignConversionModelServerEndpointRegistry CreateEvaluateRegistry()
    {
        var service = new DesignConversionInternalService(
            new ThrowingRenderProvider(),
            new ThrowingComparisonProvider(),
            routingFlow: null);
        var api = new DesignConversionModelServerApi(
            service,
            () => DateTimeOffset.UnixEpoch.AddMinutes(913));
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionEvaluate(
            new DesignConversionModelServerHttpHandler(api));
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        return registry;
    }

    private static RouteEndpoint ResolveEndpoint(WebApplication app, string endpointPath)
        => ((IEndpointRouteBuilder)app)
            .DataSources
            .SelectMany(source => source.Endpoints)
            .Concat(app.Services
                .GetRequiredService<EndpointDataSource>()
                .Endpoints)
            .OfType<RouteEndpoint>()
            .Single(endpoint => string.Equals(
                "/" + (endpoint.RoutePattern.RawText ?? string.Empty).TrimStart('/'),
                endpointPath,
                StringComparison.Ordinal));

    private static DefaultHttpContext CreateJsonPostContext(
        string path,
        Stream body,
        long? contentLength)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = path;
        context.Request.Scheme = Uri.UriSchemeHttp;
        context.Request.ContentType = "application/json";
        context.Request.ContentLength = contentLength;
        context.Request.Body = body;
        context.Response.Body = new MemoryStream();
        context.Connection.LocalIpAddress = IPAddress.Loopback;
        context.Connection.RemoteIpAddress = IPAddress.Loopback;
        context.Connection.LocalPort = 52137;
        return context;
    }

    private static string CreateExactLimitJsonObject()
    {
        const string prefix = "{\"padding\":\"";
        const string suffix = "\"}";
        var paddingBytes = ExpectedJsonRequestBodyLimitBytes - Encoding.UTF8.GetByteCount(prefix + suffix);
        Assert.True(paddingBytes >= 0);
        var bodyJson = prefix + new string('a', paddingBytes) + suffix;
        Assert.Equal(ExpectedJsonRequestBodyLimitBytes, Encoding.UTF8.GetByteCount(bodyJson));
        return bodyJson;
    }

    private static async Task<string> ReadResponseBodyAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }

    private static void AssertOversizeResponseContract(
        HttpContext context,
        string bodyJson,
        string expectedEndpoint,
        params string[] privateMarkers)
    {
        Assert.Equal(
            ExpectedJsonRequestBodyLimitBytes,
            DesignConversionModelServerAspNetEndpointMapper.MaximumJsonRequestBodyBytes);
        Assert.Equal(StatusCodes.Status413PayloadTooLarge, context.Response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);
        Assert.Contains(
            nameof(DesignConversionModelServerAspNetEndpointMapper),
            context.Response.Headers["x-design-conversion-asp-net-endpoint-mapper"].ToString(),
            StringComparison.Ordinal);
        Assert.Equal(
            bool.FalseString,
            context.Response.Headers["x-design-conversion-product-ready"].ToString());
        Assert.Equal(
            bool.FalseString,
            context.Response.Headers["x-design-conversion-public-production-default-ready"].ToString());

        foreach (var marker in privateMarkers)
            Assert.DoesNotContain(marker, bodyJson, StringComparison.Ordinal);

        Assert.DoesNotContain("MemoryStream", bodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("C:\\", bodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/Users/", bodyJson, StringComparison.OrdinalIgnoreCase);

        using var document = JsonDocument.Parse(bodyJson);
        var root = document.RootElement;
        Assert.Equal(expectedEndpoint, root.GetProperty("endpoint").GetString());
        Assert.Equal("POST", root.GetProperty("method").GetString());
        Assert.Equal(StatusCodes.Status413PayloadTooLarge, root.GetProperty("statusCode").GetInt32());
        Assert.Equal(ExpectedOversizeErrorCode, root.GetProperty("errorCode").GetString());
        Assert.Equal(ExpectedOversizeErrorMessage, root.GetProperty("errorMessage").GetString());

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("refs_only_public_response").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("response_contains_raw_private_data").GetString());
        Assert.Equal(expectedEndpoint, metadata.GetProperty("asp_net_route_path").GetString());
        Assert.Equal(
            bool.FalseString,
            metadata.GetProperty("asp_net_registry_dispatch_verified").GetString());
        Assert.Equal(
            ExpectedJsonRequestBodyLimitBytes.ToString(),
            metadata.GetProperty("asp_net_json_request_body_limit_bytes").GetString());
        Assert.Equal(
            bool.TrueString,
            metadata.GetProperty("asp_net_request_body_rejected_before_registry").GetString());
        Assert.Equal(
            bool.FalseString,
            metadata.GetProperty("asp_net_route_verification_successful_handler_dispatch").GetString());
    }

    private sealed class CountingReadStream(byte[] bytes) : Stream
    {
        private readonly MemoryStream _inner = new(bytes, writable: false);

        public int ReadCallCount { get; private set; }
        public long BytesRead { get; private set; }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _inner.Length;

        public override long Position
        {
            get => _inner.Position;
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ReadCallCount++;
            var read = _inner.Read(buffer, offset, count);
            BytesRead += read;
            return read;
        }

        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            ReadCallCount++;
            var read = await _inner.ReadAsync(buffer, cancellationToken);
            BytesRead += read;
            return read;
        }

        public override Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken)
            => ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();
    }

    private sealed class ThrowingRenderProvider : IDesignConversionRenderProvider
    {
        public Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Render provider must not be invoked by request-body limit tests.");
    }

    private sealed class ThrowingComparisonProvider : IDesignConversionComparisonProvider
    {
        public Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Comparison provider must not be invoked by request-body limit tests.");
    }
}
