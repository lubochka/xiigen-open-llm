// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;
using ModelServer.DesignConversion;

public sealed class DesignConversionModelServerLocalHttpHostTests
{
    [Fact]
    public async Task PostImprovementLoop_LocalHttpHost_ReturnsRefsOnlyEnvelope()
    {
        const string privateMarker = "PRIVATE_PASS136_LOCAL_HTTP_SECRET";
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider);
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionImprovementLoopFromComposition(
            composition,
            new UnexpectedCandidateStrategy());
        Assert.True(registered.IsSuccess, registered.ErrorMessage);

        await using var host = await LoopbackRegistryHttpHost.StartAsync(
            new DesignConversionModelServerLocalHttpHostAdapter(registry));
        using var client = new HttpClient
        {
            BaseAddress = host.BaseAddress,
            Timeout = TimeSpan.FromSeconds(5)
        };

        var request = new DesignConversionModelServerImprovementLoopRequest(
            LoopId: "pass136-local-http-host-loop",
            InitialPackage: CreatePackage("pass136-local-http-host-request", privateMarker),
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["pass"] = "codex-pass136",
                ["caller_private_marker"] = privateMarker
            });
        using var content = new StringContent(
            JsonSerializer.Serialize(request, DesignConversionModelServerHttpHandler.JsonOptions),
            Encoding.UTF8,
            "application/json");

        using var response = await client.PostAsync(
            DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            content);
        var bodyJson = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(host.SocketOpened);
        Assert.Equal("127.0.0.1", host.LoopbackHost);
        Assert.True(host.Port > 0);
        Assert.Equal(
            DesignConversionModelServerLocalHttpHostAdapter.LocalLoopbackHostBoundary,
            response.Headers.GetValues("x-design-conversion-local-http-host-boundary").Single());
        Assert.Equal(
            bool.TrueString,
            response.Headers.GetValues("x-design-conversion-socket-opened").Single());
        Assert.DoesNotContain(privateMarker, bodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(request.InitialPackage!.Html!, bodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(request.InitialPackage.Css!, bodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(request.InitialPackage.DesignTree!.Payload!, bodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(request.InitialPackage.OriginalTree!.Payload!, bodyJson, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(bodyJson);
        var root = document.RootElement;
        Assert.Equal(
            DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            root.GetProperty("endpoint").GetString());
        Assert.Equal("POST", root.GetProperty("method").GetString());
        Assert.Equal(DesignConversionModelServerHttpHandler.LocalHttpSeam, root.GetProperty("httpSeam").GetString());
        Assert.Equal(DesignConversionModelServerApi.ApiShape, root.GetProperty("apiShape").GetString());
        Assert.Equal(DesignConversionModelServerApi.RuntimeSurface, root.GetProperty("runtimeSurface").GetString());
        Assert.Equal(200, root.GetProperty("statusCode").GetInt32());
        Assert.Equal("bounded_non_ready", root.GetProperty("contractStatus").GetString());
        Assert.Equal("pass136-local-http-host-request", root.GetProperty("requestId").GetString());
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, root.GetProperty("stopReason").GetString());
        Assert.False(root.GetProperty("completed").GetBoolean());
        Assert.Equal(1, root.GetProperty("attemptCount").GetInt32());
        Assert.Equal(2, root.GetProperty("attempts")[0].GetProperty("renderResultRefs").GetArrayLength());
        Assert.Equal(2, root.GetProperty("attempts")[0].GetProperty("comparisonArtifactRefs").GetArrayLength());

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(
            DesignConversionModelServerLocalHttpHostAdapter.LocalLoopbackHostBoundary,
            metadata.GetProperty("local_http_host_boundary").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("local_http_host_verified").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("local_loopback_host_verified").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("localhost_loopback_only").GetString());
        Assert.Equal("127.0.0.1", metadata.GetProperty("loopback_host").GetString());
        Assert.Equal("ephemeral", metadata.GetProperty("bound_port_mode").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("socket_opened").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("loopback_socket_opened").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("asp_net_host_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("public_http_endpoint_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("cross_repo_caller_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("public_production_default_ready").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("external_provider_called").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("network_call_executed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("external_network_call_executed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("refs_only_public_response").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());

        Assert.Equal(2, browserAdapter.Requests.Count);
        Assert.Equal(2, comparisonProvider.Requests.Count);
    }

    [Theory]
    [InlineData("true")]
    [InlineData("42")]
    [InlineData("[]")]
    public async Task PostEvaluate_LocalHttpHost_TopLevelJsonScalarOrArrayBody_ReturnsBadRequestEnvelopeWithoutRuntime(
        string bodyJson)
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
        var api = new DesignConversionModelServerApi(
            service,
            () => DateTimeOffset.UnixEpoch.AddMinutes(516));
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionEvaluate(new DesignConversionModelServerHttpHandler(api));
        Assert.True(registered.IsSuccess, registered.ErrorMessage);

        await using var host = await LoopbackRegistryHttpHost.StartAsync(
            new DesignConversionModelServerLocalHttpHostAdapter(registry));
        using var client = new HttpClient
        {
            BaseAddress = host.BaseAddress,
            Timeout = TimeSpan.FromSeconds(5)
        };
        using var content = new StringContent(bodyJson, Encoding.UTF8, "application/json");

        using var response = await client.PostAsync(
            DesignConversionModelServerEndpointRegistry.EndpointPath,
            content);
        var responseBodyJson = await response.Content.ReadAsStringAsync();

        AssertLocalHostInputValidationError(
            response,
            responseBodyJson,
            DesignConversionModelServerEndpointRegistry.EndpointPath,
            "REQUEST_JSON_INVALID");
        Assert.Contains("invalid JSON", responseBodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
    }

    [Theory]
    [InlineData("true")]
    [InlineData("42")]
    [InlineData("[]")]
    public async Task PostImprovementLoop_LocalHttpHost_TopLevelJsonScalarOrArrayBody_ReturnsBadRequestEnvelopeWithoutCoordinator(
        string bodyJson)
    {
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var candidateStrategy = new UnexpectedCandidateStrategy();
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider);
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionImprovementLoopFromComposition(
            composition,
            candidateStrategy);
        Assert.True(registered.IsSuccess, registered.ErrorMessage);

        await using var host = await LoopbackRegistryHttpHost.StartAsync(
            new DesignConversionModelServerLocalHttpHostAdapter(registry));
        using var client = new HttpClient
        {
            BaseAddress = host.BaseAddress,
            Timeout = TimeSpan.FromSeconds(5)
        };
        using var content = new StringContent(bodyJson, Encoding.UTF8, "application/json");

        using var response = await client.PostAsync(
            DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            content);
        var responseBodyJson = await response.Content.ReadAsStringAsync();

        AssertLocalHostInputValidationError(
            response,
            responseBodyJson,
            DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            "REQUEST_JSON_INVALID");
        Assert.Contains("invalid JSON", responseBodyJson, StringComparison.OrdinalIgnoreCase);

        using var document = JsonDocument.Parse(responseBodyJson);
        var metadata = document.RootElement.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, metadata.GetProperty("improvement_loop_coordinator_configured").GetString());

        Assert.Empty(browserAdapter.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(candidateStrategy.Requests);
    }

    [Theory]
    [InlineData(DesignConversionModelServerEndpointRegistry.EndpointPath, "{}", "REQUEST_ID_REQUIRED")]
    [InlineData(DesignConversionModelServerEndpointRegistry.EndpointPath, "{\"requestId\":\"pass137-missing-evaluate-fields\"}", "DESIGN_CONVERSION_SOURCE_TREE_REQUIRED")]
    [InlineData(DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath, "{\"loopId\":\"pass137-missing-loop-package\",\"maxAttempts\":1}", "IMPROVEMENT_LOOP_INITIAL_PACKAGE_REQUIRED")]
    public async Task RegisteredDesignConversionRoute_LocalHttpHost_ValidJsonObjectMissingRequiredFields_ReturnsBadRequestEnvelopeWithoutDownstreamInvocation(
        string endpointPath,
        string bodyJson,
        string expectedErrorCode)
    {
        var fixture = RegisteredLocalHostValidationFixture.Create();
        await using var host = await LoopbackRegistryHttpHost.StartAsync(
            new DesignConversionModelServerLocalHttpHostAdapter(fixture.Registry));
        using var client = new HttpClient
        {
            BaseAddress = host.BaseAddress,
            Timeout = TimeSpan.FromSeconds(5)
        };
        using var content = new StringContent(bodyJson, Encoding.UTF8, "application/json");

        using var response = await client.PostAsync(endpointPath, content);
        var responseBodyJson = await response.Content.ReadAsStringAsync();

        AssertLocalHostInputValidationError(
            response,
            responseBodyJson,
            endpointPath,
            expectedErrorCode);
        fixture.AssertNoDownstreamInvocation();
    }

    [Theory]
    [InlineData("evaluate_unsupported_source_kind", DesignConversionModelServerEndpointRegistry.EndpointPath, "SOURCE_KIND_UNSUPPORTED")]
    [InlineData("evaluate_missing_artifact_refs", DesignConversionModelServerEndpointRegistry.EndpointPath, "DESIGN_CONVERSION_SCREENSHOT_OR_RENDER_REQUIRED")]
    [InlineData("improvement_loop_unsupported_source_kind", DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath, "SOURCE_KIND_UNSUPPORTED")]
    [InlineData("improvement_loop_missing_artifact_refs", DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath, "DESIGN_CONVERSION_SCREENSHOT_OR_RENDER_REQUIRED")]
    public async Task RegisteredDesignConversionRoute_LocalHttpHost_ValidJsonObjectUnsupportedSourceKindOrMissingArtifactRefs_ReturnsBadRequestEnvelopeWithoutDownstreamInvocation(
        string scenario,
        string endpointPath,
        string expectedErrorCode)
    {
        var fixture = RegisteredLocalHostValidationFixture.Create();
        await using var host = await LoopbackRegistryHttpHost.StartAsync(
            new DesignConversionModelServerLocalHttpHostAdapter(fixture.Registry));
        using var client = new HttpClient
        {
            BaseAddress = host.BaseAddress,
            Timeout = TimeSpan.FromSeconds(5)
        };
        using var content = new StringContent(
            CreateValidObjectValidationBodyJson(scenario),
            Encoding.UTF8,
            "application/json");

        using var response = await client.PostAsync(endpointPath, content);
        var responseBodyJson = await response.Content.ReadAsStringAsync();

        AssertLocalHostInputValidationError(
            response,
            responseBodyJson,
            endpointPath,
            expectedErrorCode);
        fixture.AssertNoDownstreamInvocation();
    }

    [Theory]
    [InlineData(DesignConversionModelServerEndpointRegistry.EndpointPath)]
    [InlineData(DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath)]
    public async Task RegisteredDesignConversionRoute_LocalHttpHost_MalformedJsonBody_ReturnsBadRequestEnvelopeWithoutDownstreamInvocation(
        string endpointPath)
    {
        var fixture = RegisteredLocalHostValidationFixture.Create();
        await using var host = await LoopbackRegistryHttpHost.StartAsync(
            new DesignConversionModelServerLocalHttpHostAdapter(fixture.Registry));
        using var client = new HttpClient
        {
            BaseAddress = host.BaseAddress,
            Timeout = TimeSpan.FromSeconds(5)
        };
        using var content = new StringContent("{not-json", Encoding.UTF8, "application/json");

        using var response = await client.PostAsync(endpointPath, content);
        var responseBodyJson = await response.Content.ReadAsStringAsync();

        AssertLocalHostInputValidationError(
            response,
            responseBodyJson,
            endpointPath,
            "REQUEST_JSON_INVALID");
        Assert.Contains("invalid JSON", responseBodyJson, StringComparison.OrdinalIgnoreCase);
        fixture.AssertNoDownstreamInvocation();
    }

    [Theory]
    [InlineData(DesignConversionModelServerEndpointRegistry.EndpointPath)]
    [InlineData(DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath)]
    public async Task RegisteredDesignConversionRoute_LocalHttpHost_UnsupportedContentType_ReturnsUnsupportedMediaTypeEnvelopeWithoutDownstreamInvocation(
        string endpointPath)
    {
        var fixture = RegisteredLocalHostValidationFixture.Create();
        await using var host = await LoopbackRegistryHttpHost.StartAsync(
            new DesignConversionModelServerLocalHttpHostAdapter(fixture.Registry));
        using var client = new HttpClient
        {
            BaseAddress = host.BaseAddress,
            Timeout = TimeSpan.FromSeconds(5)
        };
        using var content = new StringContent("{}", Encoding.UTF8, "text/plain");

        using var response = await client.PostAsync(endpointPath, content);
        var responseBodyJson = await response.Content.ReadAsStringAsync();

        AssertLocalHostInputValidationError(
            response,
            responseBodyJson,
            endpointPath,
            "UNSUPPORTED_MEDIA_TYPE",
            HttpStatusCode.UnsupportedMediaType);
        fixture.AssertNoDownstreamInvocation();
    }

    [Theory]
    [InlineData(DesignConversionModelServerEndpointRegistry.EndpointPath, false)]
    [InlineData(DesignConversionModelServerEndpointRegistry.EndpointPath, true)]
    [InlineData(DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath, false)]
    [InlineData(DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath, true)]
    public async Task RegisteredDesignConversionRoute_LocalHttpHost_MissingOrEmptyBody_ReturnsBadRequestEnvelopeWithoutDownstreamInvocation(
        string endpointPath,
        bool includeEmptyJsonContent)
    {
        var fixture = RegisteredLocalHostValidationFixture.Create();
        await using var host = await LoopbackRegistryHttpHost.StartAsync(
            new DesignConversionModelServerLocalHttpHostAdapter(fixture.Registry));
        using var client = new HttpClient
        {
            BaseAddress = host.BaseAddress,
            Timeout = TimeSpan.FromSeconds(5)
        };
        using var request = new HttpRequestMessage(HttpMethod.Post, endpointPath)
        {
            Content = includeEmptyJsonContent
                ? new StringContent(string.Empty, Encoding.UTF8, "application/json")
                : null
        };

        using var response = await client.SendAsync(request);
        var responseBodyJson = await response.Content.ReadAsStringAsync();

        AssertLocalHostInputValidationError(
            response,
            responseBodyJson,
            endpointPath,
            "REQUEST_BODY_REQUIRED");
        fixture.AssertNoDownstreamInvocation();
    }

    [Theory]
    [InlineData(DesignConversionModelServerEndpointRegistry.EndpointPath)]
    [InlineData(DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath)]
    public async Task RegisteredDesignConversionRoute_LocalHttpHost_WhitespaceOnlyJsonBody_ReturnsBodyRequiredEnvelopeWithoutDownstreamInvocation(
        string endpointPath)
    {
        var fixture = RegisteredLocalHostValidationFixture.Create();
        await using var host = await LoopbackRegistryHttpHost.StartAsync(
            new DesignConversionModelServerLocalHttpHostAdapter(fixture.Registry));
        using var client = new HttpClient
        {
            BaseAddress = host.BaseAddress,
            Timeout = TimeSpan.FromSeconds(5)
        };
        using var content = new StringContent("   \r\n\t", Encoding.UTF8, "application/json");

        using var response = await client.PostAsync(endpointPath, content);
        var responseBodyJson = await response.Content.ReadAsStringAsync();

        AssertLocalHostInputValidationError(
            response,
            responseBodyJson,
            endpointPath,
            "REQUEST_BODY_REQUIRED");
        fixture.AssertNoDownstreamInvocation();
    }

    [Theory]
    [InlineData("GET", DesignConversionModelServerEndpointRegistry.EndpointPath)]
    [InlineData("PUT", DesignConversionModelServerEndpointRegistry.EndpointPath)]
    [InlineData("GET", DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath)]
    [InlineData("PUT", DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath)]
    public async Task RegisteredDesignConversionRoute_LocalHttpHost_WrongMethod_ReturnsMethodNotAllowedEnvelopeWithoutDownstreamInvocation(
        string method,
        string endpointPath)
    {
        var fixture = RegisteredLocalHostValidationFixture.Create();
        await using var host = await LoopbackRegistryHttpHost.StartAsync(
            new DesignConversionModelServerLocalHttpHostAdapter(fixture.Registry));
        using var client = new HttpClient
        {
            BaseAddress = host.BaseAddress,
            Timeout = TimeSpan.FromSeconds(5)
        };
        using var request = new HttpRequestMessage(new HttpMethod(method), endpointPath)
        {
            Content = method.Equals("GET", StringComparison.Ordinal)
                ? null
                : new StringContent("{}", Encoding.UTF8, "application/json")
        };

        using var response = await client.SendAsync(request);
        var responseBodyJson = await response.Content.ReadAsStringAsync();

        AssertLocalHostRegistryValidationError(
            response,
            responseBodyJson,
            HttpStatusCode.MethodNotAllowed,
            endpointPath,
            method,
            "DESIGN_CONVERSION_ENDPOINT_METHOD_NOT_ALLOWED");
        fixture.AssertNoDownstreamInvocation();
    }

    [Theory]
    [InlineData("/api/design-conversion/evaluate/not-registered")]
    [InlineData("/api/design-conversion/improvement-loop/not-registered")]
    public async Task UnknownDesignConversionRoute_LocalHttpHost_Post_ReturnsNotFoundEnvelopeWithoutDownstreamInvocation(
        string endpointPath)
    {
        var fixture = RegisteredLocalHostValidationFixture.Create();
        await using var host = await LoopbackRegistryHttpHost.StartAsync(
            new DesignConversionModelServerLocalHttpHostAdapter(fixture.Registry));
        using var client = new HttpClient
        {
            BaseAddress = host.BaseAddress,
            Timeout = TimeSpan.FromSeconds(5)
        };
        using var content = new StringContent("{}", Encoding.UTF8, "application/json");

        using var response = await client.PostAsync(endpointPath, content);
        var responseBodyJson = await response.Content.ReadAsStringAsync();

        AssertLocalHostRegistryValidationError(
            response,
            responseBodyJson,
            HttpStatusCode.NotFound,
            endpointPath,
            "POST",
            "DESIGN_CONVERSION_ENDPOINT_ROUTE_NOT_FOUND");
        fixture.AssertNoDownstreamInvocation();
    }

    private static DesignConversionModelServerLocalCallerPackage CreatePackage(
        string requestId,
        string privateMarker)
        => new(
            RequestId: requestId,
            SourceKind: nameof(DesignConversionSourceKind.Figma),
            DesignTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{requestId}:design-tree-ref",
                Kind: "figma-json",
                Sha256: $"sha256:{requestId}:design-tree",
                Payload: $"{{\"secret\":\"{privateMarker}\",\"kind\":\"generated\"}}",
                Format: "figma-json",
                Metadata: new Dictionary<string, string>
                {
                    ["private_marker"] = privateMarker
                }),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{requestId}:original-tree-ref",
                Kind: "figma-json",
                Sha256: $"sha256:{requestId}:original-tree",
                Payload: $"{{\"secret\":\"{privateMarker}\",\"kind\":\"original\"}}",
                Format: "figma-json",
                Metadata: new Dictionary<string, string>
                {
                    ["private_marker"] = privateMarker
                }),
            Html: $"<main data-secret=\"{privateMarker}\">Pass136</main>",
            Css: $".pass136{{background-image:url('{privateMarker}');}}",
            Screenshots:
            [
                CreateArtifact($"{requestId}:source-desktop", "screenshot", 800, 600),
                CreateArtifact($"{requestId}:source-mobile", "screenshot", 390, 844)
            ],
            DesignRenders:
            [
                CreateArtifact($"{requestId}:design-desktop", "design-render", 800, 600),
                CreateArtifact($"{requestId}:design-mobile", "design-render", 390, 844)
            ],
            Budget: new DesignConversionModelServerBudget(
                UserId: $"user-{privateMarker}",
                Tier: "pass136-local-http-host",
                MaxImprovementCandidates: 1,
                MaxRenderQaRuns: 2,
                BudgetUsd: 1.00m,
                Metadata: new Dictionary<string, string>
                {
                    ["quality_threshold"] = "0.90",
                    ["budget_private_marker"] = privateMarker
                }),
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

    private static string CreateValidObjectValidationBodyJson(string scenario)
    {
        const string privateMarker = "PRIVATE_PASS138_LOCAL_HTTP_VALIDATION_SECRET";
        var package = CreatePackage($"pass138-{scenario}", privateMarker);

        return scenario switch
        {
            "evaluate_unsupported_source_kind" => SerializeEvaluatePackage(package with
            {
                SourceKind = "Sketch"
            }),
            "evaluate_missing_artifact_refs" => SerializeEvaluatePackage(package with
            {
                Screenshots = [],
                DesignRenders = []
            }),
            "improvement_loop_unsupported_source_kind" => SerializeImprovementLoopRequest(package with
            {
                SourceKind = "Sketch"
            }),
            "improvement_loop_missing_artifact_refs" => SerializeImprovementLoopRequest(package with
            {
                Screenshots = [],
                DesignRenders = []
            }),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "Unknown local-host validation scenario.")
        };
    }

    private static string SerializeEvaluatePackage(DesignConversionModelServerLocalCallerPackage package)
        => JsonSerializer.Serialize(package, DesignConversionModelServerHttpHandler.JsonOptions);

    private static string SerializeImprovementLoopRequest(DesignConversionModelServerLocalCallerPackage package)
        => JsonSerializer.Serialize(
            new DesignConversionModelServerImprovementLoopRequest(
                LoopId: $"{package.RequestId}:loop",
                InitialPackage: package,
                MaxAttempts: 1,
                Metadata: new Dictionary<string, string>
                {
                    ["pass"] = "codex-pass138-local-http-validation"
                }),
            DesignConversionModelServerHttpHandler.JsonOptions);

    private static void AssertLocalHostInputValidationError(
        HttpResponseMessage response,
        string bodyJson,
        string endpointPath,
        string expectedErrorCode,
        HttpStatusCode expectedStatus = HttpStatusCode.BadRequest)
    {
        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains(expectedErrorCode, bodyJson, StringComparison.Ordinal);
        Assert.Equal(
            endpointPath,
            response.Headers.GetValues("x-design-conversion-endpoint").Single());
        Assert.Equal(
            DesignConversionModelServerHttpHandler.LocalHttpSeam,
            response.Headers.GetValues("x-design-conversion-http-seam").Single());
        Assert.Equal(
            DesignConversionModelServerLocalHttpHostAdapter.LocalLoopbackHostBoundary,
            response.Headers.GetValues("x-design-conversion-local-http-host-boundary").Single());
        Assert.Equal(
            bool.TrueString,
            response.Headers.GetValues("x-design-conversion-local-http-host-verified").Single());
        Assert.Equal(
            DesignConversionModelServerLocalHttpHostAdapter.LoopbackHost,
            response.Headers.GetValues("x-design-conversion-loopback-host").Single());
        Assert.Equal(
            bool.TrueString,
            response.Headers.GetValues("x-design-conversion-socket-opened").Single());
        Assert.Equal(
            ((int)expectedStatus).ToString(),
            response.Headers.GetValues("x-design-conversion-status-code").Single());

        using var document = JsonDocument.Parse(bodyJson);
        var root = document.RootElement;
        Assert.Equal(endpointPath, root.GetProperty("endpoint").GetString());
        Assert.Equal("POST", root.GetProperty("method").GetString());
        Assert.Equal(DesignConversionModelServerHttpHandler.LocalHttpSeam, root.GetProperty("httpSeam").GetString());
        Assert.Equal((int)expectedStatus, root.GetProperty("statusCode").GetInt32());
        Assert.Equal(expectedErrorCode, root.GetProperty("errorCode").GetString());
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("errorMessage").GetString()));

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(endpointPath, metadata.GetProperty("http_path").GetString());
        Assert.Equal(((int)expectedStatus).ToString(), metadata.GetProperty("http_status_code").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("local_http_host_verified").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("local_loopback_host_verified").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("localhost_loopback_only").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("socket_opened").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("loopback_socket_opened").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("external_provider_called").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("network_call_executed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
    }

    private static void AssertLocalHostRegistryValidationError(
        HttpResponseMessage response,
        string bodyJson,
        HttpStatusCode expectedStatus,
        string endpointPath,
        string expectedMethod,
        string expectedErrorCode)
    {
        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains(expectedErrorCode, bodyJson, StringComparison.Ordinal);
        Assert.Equal(
            endpointPath,
            response.Headers.GetValues("x-design-conversion-endpoint").Single());
        Assert.Equal(
            DesignConversionModelServerHttpHandler.LocalHttpSeam,
            response.Headers.GetValues("x-design-conversion-http-seam").Single());
        Assert.Equal(
            DesignConversionModelServerLocalHttpHostAdapter.LocalLoopbackHostBoundary,
            response.Headers.GetValues("x-design-conversion-local-http-host-boundary").Single());
        Assert.Equal(
            bool.TrueString,
            response.Headers.GetValues("x-design-conversion-local-http-host-verified").Single());
        Assert.Equal(
            DesignConversionModelServerLocalHttpHostAdapter.LoopbackHost,
            response.Headers.GetValues("x-design-conversion-loopback-host").Single());
        Assert.Equal(
            bool.TrueString,
            response.Headers.GetValues("x-design-conversion-socket-opened").Single());
        Assert.Equal(
            ((int)expectedStatus).ToString(),
            response.Headers.GetValues("x-design-conversion-status-code").Single());

        using var document = JsonDocument.Parse(bodyJson);
        var root = document.RootElement;
        Assert.Equal(endpointPath, root.GetProperty("endpoint").GetString());
        Assert.Equal(expectedMethod, root.GetProperty("method").GetString());
        Assert.Equal(DesignConversionModelServerHttpHandler.LocalHttpSeam, root.GetProperty("httpSeam").GetString());
        Assert.Equal((int)expectedStatus, root.GetProperty("statusCode").GetInt32());
        Assert.Equal(expectedErrorCode, root.GetProperty("errorCode").GetString());
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("errorMessage").GetString()));

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(endpointPath, metadata.GetProperty("http_path").GetString());
        Assert.Equal(expectedMethod, metadata.GetProperty("http_method").GetString());
        Assert.Equal(((int)expectedStatus).ToString(), metadata.GetProperty("http_status_code").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("local_http_host_verified").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("local_loopback_host_verified").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("localhost_loopback_only").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("socket_opened").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("loopback_socket_opened").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("external_provider_called").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("network_call_executed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("refs_only_public_response").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("response_contains_raw_private_data").GetString());
    }

    private static DesignConversionModelServerArtifactRef CreateArtifact(
        string id,
        string kind,
        int width,
        int height)
        => new(
            ArtifactId: id,
            Kind: kind,
            Width: width,
            Height: height,
            Uri: $"artifact://pass136/{id}.png",
            Sha256: $"sha256:{id}");

    private static string Sha256Text(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private sealed class RecordingRenderProvider : IDesignConversionRenderProvider
    {
        public List<DesignConversionRenderRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var image = new RenderQaImageRecord(
                ImageId: $"{request.RenderRequestId}:actual",
                Label: "actual-render",
                Resolution: request.Resolution,
                Uri: $"artifact://pass136/local-http-render/{Sha256Text(request.RenderRequestId)}.png",
                Sha256: $"sha256:{Sha256Text(request.RenderRequestId + ":actual")}",
                Metadata: request.Metadata);

            return Task.FromResult(OperationResult<DesignConversionRenderResult>.Ok(new DesignConversionRenderResult(
                RenderResultId: $"{request.RenderRequestId}:result",
                RenderRequestId: request.RenderRequestId,
                Image: image,
                ElapsedMs: 7,
                Warnings: ["pass136-local-http-recording-render-provider"])));
        }
    }

    private sealed class RecordingBrowserAdapter : IDesignConversionBrowserProcessAdapter
    {
        public List<DesignConversionBrowserCaptureRequest> Requests { get; } = [];

        public async Task<OperationResult<DesignConversionBrowserCaptureResult>> CaptureScreenshotAsync(
            DesignConversionBrowserCaptureRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);

            var render = await new DeterministicLocalDesignConversionPngRenderProvider(Path.Combine(
                    Path.GetTempPath(),
                    "llm_mvp_core",
                    "pass136-local-http-recording-browser",
                    Guid.NewGuid().ToString("N")))
                .RenderAsync(
                    new DesignConversionRenderRequest(
                        RenderRequestId: $"{request.CaptureRequestId}:pass136-recording-browser-png",
                        RequestId: request.RequestId,
                        Html: request.DocumentHtml,
                        Css: "body{margin:0}.pass136-recording-browser-proof{color:#123456;background:#f8fafc}",
                        Resolution: request.Resolution,
                        Metadata: request.Metadata),
                    cancellationToken);

            if (!render.IsSuccess)
                return render.MapError<DesignConversionBrowserCaptureResult>();

            var sha = render.Value!.Image.Sha256["sha256:".Length..];
            return OperationResult<DesignConversionBrowserCaptureResult>.Ok(
                new DesignConversionBrowserCaptureResult(
                    ScreenshotUri: render.Value.Image.Uri,
                    ScreenshotSha256: sha,
                    ElapsedMs: 3,
                    Metadata: new Dictionary<string, string>
                    {
                        ["browser_adapter_kind"] = "pass136-recording-browser-adapter",
                        ["browser_process_boundary"] = "adapter_supplied_inspectable_png_file"
                    },
                    Warnings: []));
        }
    }

    private sealed class RecordingComparisonProvider(double score) : IDesignConversionComparisonProvider
    {
        public List<DesignConversionQaComparisonRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var verdict = new VisualVerdict(
                VerdictId: $"{request.ComparisonRequestId}:pass136-verdict",
                Score: score,
                Defects: [],
                EvidenceRefs: [$"pass136-comparison:{request.ComparisonRequestId}"],
                Passed: true);

            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Ok(
                new DesignConversionQaComparisonArtifact(
                    ArtifactId: $"{request.ComparisonRequestId}:pass136-artifact",
                    ComparisonRequestId: request.ComparisonRequestId,
                    Expected: request.Expected,
                    Actual: request.Actual,
                    Verdict: verdict,
                    DiffImageUri: $"artifact://pass136/diff/{Sha256Text(request.ComparisonRequestId)}.json",
                    DiffImageSha256: $"sha256:{Sha256Text(request.ComparisonRequestId + ":diff")}",
                    Metrics: new Dictionary<string, double>
                    {
                        ["visual_distance"] = 1.0 - score
                    },
                    EvidenceRefs: ["pass136-comparison-provider"])
                {
                    Metadata = new Dictionary<string, string>
                    {
                        ["external_ai_visual_judge_executed"] = bool.FalseString,
                        ["visual_ai_executed"] = bool.FalseString,
                        ["raw_html_included"] = bool.FalseString,
                        ["raw_css_included"] = bool.FalseString,
                        ["raw_design_tree_included"] = bool.FalseString
                    }
                }));
        }
    }

    private sealed class UnexpectedCandidateStrategy : IDesignConversionImprovementCandidateStrategy
    {
        public List<DesignConversionCandidateUpdateRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionImprovementCandidate>> CreateNextCandidateAsync(
            DesignConversionCandidateUpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(OperationResult<DesignConversionImprovementCandidate>.Fail(
                "PASS136 expects quality reached before a candidate is requested.",
                "PASS136_UNEXPECTED_CANDIDATE_REQUEST"));
        }
    }

    private sealed class RegisteredLocalHostValidationFixture
    {
        private RegisteredLocalHostValidationFixture(
            DesignConversionModelServerEndpointRegistry registry,
            RecordingRenderProvider renderProvider,
            RecordingBrowserAdapter browserAdapter,
            RecordingComparisonProvider comparisonProvider,
            UnexpectedCandidateStrategy candidateStrategy)
        {
            Registry = registry;
            RenderProvider = renderProvider;
            BrowserAdapter = browserAdapter;
            ComparisonProvider = comparisonProvider;
            CandidateStrategy = candidateStrategy;
        }

        public DesignConversionModelServerEndpointRegistry Registry { get; }

        private RecordingRenderProvider RenderProvider { get; }

        private RecordingBrowserAdapter BrowserAdapter { get; }

        private RecordingComparisonProvider ComparisonProvider { get; }

        private UnexpectedCandidateStrategy CandidateStrategy { get; }

        public static RegisteredLocalHostValidationFixture Create()
        {
            var renderProvider = new RecordingRenderProvider();
            var browserAdapter = new RecordingBrowserAdapter();
            var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
            var candidateStrategy = new UnexpectedCandidateStrategy();
            var service = new DesignConversionInternalService(renderProvider, comparisonProvider, routingFlow: null);
            var api = new DesignConversionModelServerApi(
                service,
                () => DateTimeOffset.UnixEpoch.AddMinutes(517));
            var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
                browserAdapter,
                comparisonProvider);
            var registry = new DesignConversionModelServerEndpointRegistry();

            var evaluateRegistered = registry.RegisterDesignConversionEvaluate(
                new DesignConversionModelServerHttpHandler(api));
            Assert.True(evaluateRegistered.IsSuccess, evaluateRegistered.ErrorMessage);

            var improvementLoopRegistered = registry.RegisterDesignConversionImprovementLoopFromComposition(
                composition,
                candidateStrategy);
            Assert.True(improvementLoopRegistered.IsSuccess, improvementLoopRegistered.ErrorMessage);

            return new RegisteredLocalHostValidationFixture(
                registry,
                renderProvider,
                browserAdapter,
                comparisonProvider,
                candidateStrategy);
        }

        public void AssertNoDownstreamInvocation()
        {
            Assert.Empty(RenderProvider.Requests);
            Assert.Empty(BrowserAdapter.Requests);
            Assert.Empty(ComparisonProvider.Requests);
            Assert.Empty(CandidateStrategy.Requests);
        }
    }

    private sealed class LoopbackRegistryHttpHost : IAsyncDisposable
    {
        private readonly DesignConversionModelServerLocalHttpHostAdapter _adapter;
        private readonly TcpListener _listener;
        private readonly CancellationTokenSource _stop = new();
        private readonly List<Task> _connections = [];
        private readonly object _sync = new();
        private readonly Task _serverTask;
        private int _socketOpened;

        private LoopbackRegistryHttpHost(DesignConversionModelServerLocalHttpHostAdapter adapter)
        {
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            _listener = new TcpListener(IPAddress.Loopback, 0);
            _listener.Start();
            Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
            BaseAddress = new Uri($"http://127.0.0.1:{Port}");
            _serverTask = Task.Run(ServeAsync);
        }

        public Uri BaseAddress { get; }

        public int Port { get; }

        public string LoopbackHost => "127.0.0.1";

        public bool SocketOpened => Volatile.Read(ref _socketOpened) == 1;

        public static Task<LoopbackRegistryHttpHost> StartAsync(
            DesignConversionModelServerLocalHttpHostAdapter adapter)
            => Task.FromResult(new LoopbackRegistryHttpHost(adapter));

        public async ValueTask DisposeAsync()
        {
            _stop.Cancel();
            _listener.Stop();

            try
            {
                await _serverTask;
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }

            Task[] connections;
            lock (_sync)
                connections = [.. _connections];

            try
            {
                await Task.WhenAll(connections);
            }
            finally
            {
                _stop.Dispose();
            }
        }

        private async Task ServeAsync()
        {
            while (!_stop.IsCancellationRequested)
            {
                TcpClient client;
                try
                {
                    client = await _listener.AcceptTcpClientAsync(_stop.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (SocketException) when (_stop.IsCancellationRequested)
                {
                    break;
                }

                Interlocked.Exchange(ref _socketOpened, 1);
                var connection = HandleClientAsync(client, _stop.Token);
                lock (_sync)
                    _connections.Add(connection);
                _ = connection.ContinueWith(
                    completed =>
                    {
                        lock (_sync)
                            _connections.Remove(completed);
                    },
                    TaskScheduler.Default);
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            using (client)
            using (var stream = client.GetStream())
            {
                var request = await ReadHttpRequestAsync(stream, cancellationToken);
                var response = await _adapter.DispatchAsync(request, cancellationToken);
                await WriteHttpResponseAsync(stream, response, cancellationToken);
            }
        }

        private static async Task<DesignConversionModelServerHttpRequest> ReadHttpRequestAsync(
            NetworkStream stream,
            CancellationToken cancellationToken)
        {
            var received = new List<byte>();
            var buffer = new byte[1024];
            var headerEnd = -1;
            while (headerEnd < 0)
            {
                var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                if (read == 0)
                    throw new InvalidOperationException("Client closed the local loopback HTTP request before headers were complete.");

                for (var index = 0; index < read; index++)
                    received.Add(buffer[index]);

                headerEnd = FindHeaderEnd(received);
                if (received.Count > 65536)
                    throw new InvalidOperationException("Local loopback HTTP request headers exceeded the test limit.");
            }

            var headerText = Encoding.ASCII.GetString(received.Take(headerEnd).ToArray());
            var headerLines = headerText.Split("\r\n", StringSplitOptions.None);
            var requestLine = headerLines[0].Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
            var method = requestLine.Length > 0 ? requestLine[0] : "UNKNOWN";
            var path = requestLine.Length > 1 ? NormalizeRequestTarget(requestLine[1]) : "/";
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in headerLines.Skip(1))
            {
                var separator = line.IndexOf(':', StringComparison.Ordinal);
                if (separator <= 0)
                    continue;

                headers[line[..separator].Trim()] = line[(separator + 1)..].Trim();
            }

            var contentLength = headers.TryGetValue("Content-Length", out var contentLengthText) &&
                                int.TryParse(contentLengthText, out var parsed)
                ? parsed
                : 0;
            var bodyBytes = new byte[contentLength];
            var bodyStart = headerEnd + 4;
            var bufferedBodyLength = Math.Min(contentLength, received.Count - bodyStart);
            if (bufferedBodyLength > 0)
                received.CopyTo(bodyStart, bodyBytes, 0, bufferedBodyLength);

            var copied = bufferedBodyLength;
            while (copied < contentLength)
            {
                var read = await stream.ReadAsync(bodyBytes.AsMemory(copied, contentLength - copied), cancellationToken);
                if (read == 0)
                    throw new InvalidOperationException("Client closed the local loopback HTTP request before the body was complete.");
                copied += read;
            }

            headers.TryGetValue("Content-Type", out var contentType);
            return new DesignConversionModelServerHttpRequest(
                Method: method,
                Path: path,
                BodyJson: contentLength == 0 ? null : Encoding.UTF8.GetString(bodyBytes),
                ContentType: contentType,
                Headers: headers);
        }

        private static async Task WriteHttpResponseAsync(
            NetworkStream stream,
            DesignConversionModelServerHttpResponse response,
            CancellationToken cancellationToken)
        {
            var bodyBytes = Encoding.UTF8.GetBytes(response.BodyJson);
            var header = new StringBuilder()
                .Append("HTTP/1.1 ")
                .Append(response.StatusCode)
                .Append(' ')
                .Append(ReasonPhrase(response.StatusCode))
                .Append("\r\n")
                .Append("Content-Type: ")
                .Append(response.ContentType)
                .Append("\r\n")
                .Append("Content-Length: ")
                .Append(bodyBytes.Length)
                .Append("\r\n")
                .Append("Connection: close\r\n");

            foreach (var pair in response.Headers)
            {
                if (pair.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase) ||
                    pair.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase) ||
                    pair.Key.Contains('\r', StringComparison.Ordinal) ||
                    pair.Key.Contains('\n', StringComparison.Ordinal))
                {
                    continue;
                }

                header
                    .Append(pair.Key)
                    .Append(": ")
                    .Append(SanitizeHeaderValue(pair.Value))
                    .Append("\r\n");
            }

            header.Append("\r\n");
            await stream.WriteAsync(Encoding.ASCII.GetBytes(header.ToString()), cancellationToken);
            await stream.WriteAsync(bodyBytes, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }

        private static int FindHeaderEnd(IReadOnlyList<byte> bytes)
        {
            for (var index = 0; index <= bytes.Count - 4; index++)
            {
                if (bytes[index] == '\r' &&
                    bytes[index + 1] == '\n' &&
                    bytes[index + 2] == '\r' &&
                    bytes[index + 3] == '\n')
                {
                    return index;
                }
            }

            return -1;
        }

        private static string NormalizeRequestTarget(string target)
        {
            if (Uri.TryCreate(target, UriKind.Absolute, out var absolute))
                return absolute.AbsolutePath;

            var queryIndex = target.IndexOf('?', StringComparison.Ordinal);
            return queryIndex >= 0 ? target[..queryIndex] : target;
        }

        private static string ReasonPhrase(int statusCode)
            => statusCode switch
            {
                200 => "OK",
                400 => "Bad Request",
                404 => "Not Found",
                405 => "Method Not Allowed",
                415 => "Unsupported Media Type",
                502 => "Bad Gateway",
                503 => "Service Unavailable",
                _ => "DesignConversion Response"
            };

        private static string SanitizeHeaderValue(string value)
            => value
                .Replace("\r", string.Empty, StringComparison.Ordinal)
                .Replace("\n", string.Empty, StringComparison.Ordinal);
    }
}
