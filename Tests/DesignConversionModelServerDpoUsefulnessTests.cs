// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ModelServer.DesignConversion;

[Collection(DesignConversionLocalHostTestCollectionNames.CollectionName)]
public sealed class DesignConversionModelServerDpoUsefulnessTests : IAsyncLifetime
{
    private const string OwnedTempPathPrefix = "xiigen-dpo-usefulness-";
    private const string RouteAdmissionCurrentToken = "dpo-usefulness-route-token-0123456789";
    private const string RouteAdmissionTenantId = "dpo-usefulness-tenant-alpha";
    private readonly HashSet<string> _ownedTempPaths = new(
        OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal);

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        List<Exception>? cleanupFailures = null;
        foreach (var path in _ownedTempPaths
                     .OrderByDescending(path => path.Length)
                     .ToArray())
        {
            try
            {
                DeleteOwnedTempPath(path);
            }
            catch (Exception exception)
            {
                cleanupFailures ??= [];
                cleanupFailures.Add(exception);
            }
        }

        if (cleanupFailures is not null)
        {
            throw new AggregateException(
                "One or more owned DPO-usefulness test paths could not be cleaned.",
                cleanupFailures);
        }

        return Task.CompletedTask;
    }

    [Fact]
    public void DeleteOwnedTempPath_RegisteredDirectChildRemovesDirectoryAndOwnership()
    {
        var ownedPath = CreateOwnedTempPath();
        Directory.CreateDirectory(ownedPath);
        File.WriteAllText(Path.Combine(ownedPath, "sentinel.txt"), "owned");

        DeleteOwnedTempPath(ownedPath);

        Assert.False(Directory.Exists(ownedPath));
        Assert.DoesNotContain(ownedPath, _ownedTempPaths);
    }

    [Fact]
    public void DeleteOwnedTempPath_UnregisteredTempRootAndPrefixedSiblingRemainIntact()
    {
        var tempRoot = NormalizedTempRoot();
        var unownedPath = Path.Combine(
            tempRoot,
            OwnedTempPathPrefix + Guid.NewGuid().ToString("N"));
        var sentinelPath = Path.Combine(unownedPath, "sentinel.txt");
        Directory.CreateDirectory(unownedPath);
        File.WriteAllText(sentinelPath, "must remain");

        try
        {
            DeleteOwnedTempPath(tempRoot);
            DeleteOwnedTempPath(unownedPath);

            Assert.True(Directory.Exists(tempRoot));
            Assert.True(Directory.Exists(unownedPath));
            Assert.True(File.Exists(sentinelPath));
        }
        finally
        {
            if (Directory.Exists(unownedPath))
                Directory.Delete(unownedPath, recursive: true);
        }
    }

    [Fact]
    public async Task DisposeAsync_InvalidOwnedShapeFailsBeforeMutationAndStillCleansValidOwnedPath()
    {
        var invalidPath = Path.Combine(
            NormalizedTempRoot(),
            OwnedTempPathPrefix + "invalid-" + Guid.NewGuid().ToString("N"));
        var invalidSentinelPath = Path.Combine(invalidPath, "sentinel.txt");
        Directory.CreateDirectory(invalidPath);
        File.WriteAllText(invalidSentinelPath, "must remain");
        Assert.True(_ownedTempPaths.Add(invalidPath));

        var validPath = CreateOwnedTempPath();
        Directory.CreateDirectory(validPath);

        try
        {
            var exception = await Assert.ThrowsAsync<AggregateException>(DisposeAsync);

            var cleanupFailure = Assert.Single(exception.InnerExceptions);
            Assert.IsType<InvalidOperationException>(cleanupFailure);
            Assert.True(Directory.Exists(invalidPath));
            Assert.True(File.Exists(invalidSentinelPath));
            Assert.Contains(invalidPath, _ownedTempPaths);
            Assert.False(Directory.Exists(validPath));
            Assert.DoesNotContain(validPath, _ownedTempPaths);
        }
        finally
        {
            _ownedTempPaths.Remove(invalidPath);
            if (Directory.Exists(invalidPath))
                Directory.Delete(invalidPath, recursive: true);
        }
    }

    [Fact]
    public async Task GetDpoUsefulness_RegistryRoute_DelegatesToDomainApiAndReturnsSanitizedRows()
    {
        const string privateMarker = "PRIVATE_PASS145_DPO_USEFULNESS_SECRET";
        var db = await CreateSeededDbAsync(privateMarker);
        var registry = CreateRegistry(db);
        var registered = registry.Resolve(
            DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointMethod,
            DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath);

        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointRoute, registered.Value!.BoundaryMetadata["endpoint_route"]);
        Assert.Equal(nameof(DesignConversionModelServerDpoUsefulnessHandler), registered.Value.BoundaryMetadata["delegates_to_handler"]);
        Assert.Equal(nameof(DesignConversionDpoUsefulnessApi), registered.Value.BoundaryMetadata["delegates_to_domain_api"]);
        Assert.Equal(bool.TrueString, registered.Value.BoundaryMetadata["dpo_usefulness_response_refs_only"]);
        Assert.Equal(bool.FalseString, registered.Value.BoundaryMetadata["product_ready"]);

        var response = await registry.InvokeAsync(new DesignConversionModelServerHttpRequest(
            Method: "GET",
            Path: DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath,
            BodyJson: null,
            ContentType: null,
            QueryString: "?includeBlocked=true&limit=10"));

        Assert.True(response.IsSuccess, response.ErrorMessage);
        AssertSanitizedDpoUsefulnessBody(response.Value!, privateMarker, expectedRowCount: 2);
    }

    [Theory]
    [InlineData(null, 401, "DESIGN_CONVERSION_ROUTE_ADMISSION_TOKEN_REQUIRED")]
    [InlineData("wrong-dpo-usefulness-token", 403, "DESIGN_CONVERSION_ROUTE_ADMISSION_TOKEN_INVALID")]
    public async Task GetDpoUsefulness_EnabledRouteAdmission_RejectsMissingOrInvalidAuthWithoutDomainApi(
        string? suppliedToken,
        int expectedStatusCode,
        string expectedErrorCode)
    {
        const string privateMarker = "PRIVATE_PASS145_DPO_USEFULNESS_ADMISSION_REJECTION_SECRET";
        var db = new CountingLocalArtifactDb(await CreateSeededDbAsync(privateMarker));
        var registry = CreateRegistry(db, CreateEnabledRouteAdmission());

        var response = await registry.InvokeAsync(new DesignConversionModelServerHttpRequest(
            Method: "GET",
            Path: DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath,
            BodyJson: null,
            ContentType: null,
            Headers: RouteAdmissionHeaders(suppliedToken),
            QueryString: "?includeBlocked=true&limit=10"));

        Assert.True(response.IsSuccess, response.ErrorMessage);
        Assert.Equal(0, db.QueryDisplayRowsCallCount);
        AssertRouteAdmissionRejectedDpoUsefulness(response.Value!, expectedStatusCode, expectedErrorCode, privateMarker, suppliedToken);
    }

    [Fact]
    public async Task GetDpoUsefulness_EnabledRouteAdmission_AcceptsValidTokenAndReturnsSanitizedRefsOnlyBody()
    {
        const string privateMarker = "PRIVATE_PASS145_DPO_USEFULNESS_ADMISSION_ACCEPTED_SECRET";
        var routeAdmission = CreateEnabledRouteAdmission();
        var db = new CountingLocalArtifactDb(await CreateSeededDbAsync(privateMarker, routeAdmission));
        var registry = CreateRegistry(db, routeAdmission);

        var response = await registry.InvokeAsync(new DesignConversionModelServerHttpRequest(
            Method: "GET",
            Path: DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath,
            BodyJson: null,
            ContentType: null,
            Headers: RouteAdmissionHeaders(RouteAdmissionCurrentToken),
            QueryString: "?includeBlocked=true&limit=10"));

        Assert.True(response.IsSuccess, response.ErrorMessage);
        Assert.Equal(1, db.QueryDisplayRowsCallCount);
        AssertSanitizedDpoUsefulnessBody(response.Value!, privateMarker, expectedRowCount: 2);

        Assert.Equal(
            bool.TrueString,
            response.Value!.Headers["x-design-conversion-dpo-usefulness-route-admission-checked"]);
        Assert.Equal(
            bool.TrueString,
            response.Value.Headers["x-design-conversion-route-admission-enabled"]);
        Assert.Equal(
            bool.TrueString,
            response.Value.Headers["x-design-conversion-route-admission-authorized"]);
        Assert.Equal(
            bool.TrueString,
            response.Value.Headers["x-design-conversion-route-admission-tenant-bound"]);
        Assert.DoesNotContain(RouteAdmissionCurrentToken, response.Value.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(RouteAdmissionTenantId, response.Value.BodyJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetDpoUsefulness_EnabledRouteAdmission_BindsRowsToAdmittedTenantAndIgnoresCallerTenantSelector()
    {
        const string privateMarker = "PRIVATE_W053_DPO_USEFULNESS_TENANT_SCOPE";
        var routeAdmission = CreateEnabledRouteAdmission();
        var siblingAdmission = CreateEnabledRouteAdmission("dpo-usefulness-tenant-sibling");
        var inner = await CreateSeededDbAsync(privateMarker, routeAdmission);
        await DesignConversionPostEvaluationEligibilityTestFixture.AppendEligibleRecordAsync(
            inner,
            BuildArtifactRecord(
                "w053-sibling-tenant-row",
                DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                [WithRouteAdmissionBinding(
                    BuildTrainingRecord("w053-sibling-tenant-row", DesignConversionDpoUnits.CriticUnit, 0.99),
                    siblingAdmission)],
                privateMarker));
        var db = new CountingLocalArtifactDb(inner);
        var registry = CreateRegistry(db, routeAdmission);
        var siblingTenantIdRef = Assert.IsType<string>(
            siblingAdmission.PublicMetadata()["routeAdmissionTenantIdRef"]);

        var response = await registry.InvokeAsync(new DesignConversionModelServerHttpRequest(
            Method: "GET",
            Path: DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath,
            BodyJson: null,
            ContentType: null,
            Headers: RouteAdmissionHeaders(RouteAdmissionCurrentToken),
            QueryString: "?includeBlocked=true&limit=10&tenantIdRef=" + Uri.EscapeDataString(siblingTenantIdRef)));

        Assert.True(response.IsSuccess, response.ErrorMessage);
        Assert.Equal(1, db.QueryDisplayRowsCallCount);
        Assert.Equal(200, response.Value!.StatusCode);
        using var document = JsonDocument.Parse(response.Value.BodyJson);
        var root = document.RootElement;
        Assert.Equal(2, root.GetProperty("rowCount").GetInt32());
        Assert.Equal(2, root.GetProperty("rows").GetArrayLength());
        Assert.Equal(
            bool.TrueString,
            root.GetProperty("boundaryMetadata").GetProperty("tenant_scope_applied").GetString());
        Assert.Equal(
            "server_admission_context",
            root.GetProperty("boundaryMetadata").GetProperty("tenant_scope_source").GetString());
        Assert.DoesNotContain(siblingTenantIdRef, response.Value.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(privateMarker, response.Value.BodyJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetDpoUsefulness_AspNetMapper_EnabledAdmissionRejectsMissingTokenBeforeBodyReadAndDomainApi()
    {
        const string privateMarker = "PRIVATE_PASS145_DPO_USEFULNESS_ASPNET_ADMISSION_SECRET";
        var db = new CountingLocalArtifactDb(await CreateSeededDbAsync(privateMarker));
        var registry = CreateRegistry(db, CreateEnabledRouteAdmission());

        await using var requestBody = new ThrowOnReadStream();
        await using var responseBody = new MemoryStream();
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath;
        context.Request.QueryString = new QueryString("?includeBlocked=true&limit=10");
        context.Request.Body = requestBody;
        context.Response.Body = responseBody;

        var method = typeof(DesignConversionModelServerAspNetEndpointMapper).GetMethod(
            "DispatchDpoUsefulnessAsync",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        var task = Assert.IsAssignableFrom<Task>(method.Invoke(null, [context, registry]));
        await task;

        Assert.Equal(401, context.Response.StatusCode);
        Assert.False(requestBody.ReadAttempted);
        Assert.Equal(0, db.QueryDisplayRowsCallCount);
        var bodyJson = Encoding.UTF8.GetString(responseBody.ToArray());
        Assert.Contains("DESIGN_CONVERSION_ROUTE_ADMISSION_TOKEN_REQUIRED", bodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(RouteAdmissionCurrentToken, bodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(RouteAdmissionTenantId, bodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(privateMarker, bodyJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetDpoUsefulness_RegistryRoute_SerializesUsefulnessMetricsForClientConsumption()
    {
        const string privateMarker = "PRIVATE_PASS145_DPO_USEFULNESS_METRICS_SECRET";
        var db = await CreateSeededDbAsync(privateMarker);
        var registry = CreateRegistry(db);

        var response = await registry.InvokeAsync(new DesignConversionModelServerHttpRequest(
            Method: "GET",
            Path: DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath,
            BodyJson: null,
            ContentType: null,
            QueryString: "?includeBlocked=true&limit=10"));

        Assert.True(response.IsSuccess, response.ErrorMessage);

        using var document = JsonDocument.Parse(response.Value!.BodyJson);
        var root = document.RootElement;
        Assert.False(root.GetProperty("productReady").GetBoolean());

        var row = Assert.Single(root.GetProperty("rows").EnumerateArray(), item =>
            string.Equals(item.GetProperty("reviewStatus").GetString(), "train-ready", StringComparison.Ordinal));
        var metrics = row.GetProperty("usefulnessMetrics");

        Assert.Equal(0.94, metrics.GetProperty("score").GetDouble(), precision: 6);
        Assert.Equal(0.57, metrics.GetProperty("qualityGain").GetDouble(), precision: 6);
        Assert.Equal(
            DesignConversionNegativeReasonTaxonomy.Spacing,
            metrics.GetProperty("negativePairUsefulness").GetString());
        Assert.Contains("source_kind=Figma", metrics.GetProperty("sourceCoverage").GetString(), StringComparison.Ordinal);
        Assert.Contains("source_ref=present", metrics.GetProperty("sourceCoverage").GetString(), StringComparison.Ordinal);
        Assert.Contains("source_disjoint=not_proven", metrics.GetProperty("sourceCoverage").GetString(), StringComparison.Ordinal);
        Assert.Equal("not_proven", metrics.GetProperty("sourceDisjointStatus").GetString());
        Assert.Equal("not_proven", metrics.GetProperty("heldOutEvaluationStatus").GetString());
        Assert.Equal(
            "retrain_intake_eligible;local_promotion_requires_training_run_measurement_gate_and_policy_checkpoint",
            metrics.GetProperty("promotionEligibility").GetString());
        Assert.Equal(
            "policy_checkpoint_evidence:not_present",
            Assert.Single(metrics.GetProperty("policyCheckpointEvidenceRefs").EnumerateArray()).GetString());
        Assert.False(metrics.GetProperty("productReady").GetBoolean());
        Assert.Equal("product_ready=false", metrics.GetProperty("productReadyBoundary").GetString());

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
    }

    [Fact]
    public async Task TryGetDpoUsefulnessAsync_LocalCaller_ReturnsStableRefsOnlyEnvelope()
    {
        const string privateMarker = "PRIVATE_PASS145_LOCAL_CALLER_SECRET";
        var db = await CreateSeededDbAsync(privateMarker);
        var adapter = new DesignConversionModelServerLocalCallerAdapter(CreateRegistry(db));

        var envelope = await adapter.TryGetDpoUsefulnessAsync(new DesignConversionDpoUsefulnessApiRequest(
            IncludeBlocked: true,
            Limit: 10));

        Assert.Equal(DesignConversionModelServerCallerContract.LocalCallerResponseEnvelopeSchemaVersion, envelope.EnvelopeSchemaVersion);
        Assert.Equal(DesignConversionModelServerCallerContract.LocalCallerResponseEnvelopeBoundary, envelope.EnvelopeBoundary);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionModelServerCallerContract.DpoUsefulnessRouteSchemaVersion, envelope.ContractSchemaVersion);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointMethod, envelope.EndpointMethod);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath, envelope.EndpointPath);
        Assert.Equal(200, envelope.StatusCode);
        Assert.True(envelope.RegistryInvokeSucceeded);
        Assert.True(envelope.RuntimeInvoked);
        Assert.True(envelope.ParsedResponseJsonSucceeded);
        Assert.Equal("dpo-usefulness-readback-completed", envelope.ParsedContractStatus);
        Assert.Null(envelope.ParsedErrorCode);
        Assert.Contains("includeBlocked", envelope.RequiredRequestFields);
        Assert.DoesNotContain(privateMarker, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.False(envelope.RawHtmlIncluded);
        Assert.False(envelope.RawCssIncluded);
        Assert.False(envelope.RawDesignTreeIncluded);
        Assert.False(envelope.ResponseContainsRawPrivateData);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["dpo_usefulness_response_refs_only"]);
        Assert.Equal(nameof(DesignConversionDpoUsefulnessApi), envelope.BoundaryMetadata["delegates_to_domain_api"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["public_http_endpoint_verified"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["deployment_verified"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["product_ready"]);
    }

    [Fact]
    public async Task TryGetDpoUsefulnessAsync_LocalCaller_ParsesTypedUsefulnessMetricsForClientConsumption()
    {
        const string privateMarker = "PRIVATE_PASS145_LOCAL_CALLER_TYPED_METRICS_SECRET";
        var db = await CreateSeededDbAsync(privateMarker);
        var adapter = new DesignConversionModelServerLocalCallerAdapter(CreateRegistry(db));

        var envelope = await adapter.TryGetDpoUsefulnessAsync(new DesignConversionDpoUsefulnessApiRequest(
            IncludeBlocked: true,
            Limit: 10));

        var typed = envelope.ParsedDpoUsefulnessResponse;
        Assert.NotNull(typed);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal("dpo-usefulness-readback-completed", typed.ContractStatus);
        Assert.False(typed.ProductReady);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["local_caller_dpo_usefulness_typed_response_parsed"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["product_ready"]);

        var row = Assert.Single(typed.Rows, item =>
            string.Equals(item.ReviewStatus, "train-ready", StringComparison.Ordinal));
        var metrics = row.UsefulnessMetrics;
        Assert.Equal(0.94, metrics.Score, precision: 6);
        Assert.Equal(0.57, metrics.QualityGain, precision: 6);
        Assert.Equal(DesignConversionNegativeReasonTaxonomy.Spacing, metrics.NegativePairUsefulness);
        Assert.Contains("source_kind=Figma", metrics.SourceCoverage, StringComparison.Ordinal);
        Assert.Contains("source_ref=present", metrics.SourceCoverage, StringComparison.Ordinal);
        Assert.Contains("source_disjoint=not_proven", metrics.SourceCoverage, StringComparison.Ordinal);
        Assert.Equal("not_proven", metrics.SourceDisjointStatus);
        Assert.Equal("not_proven", metrics.HeldOutEvaluationStatus);
        Assert.Equal(
            "retrain_intake_eligible;local_promotion_requires_training_run_measurement_gate_and_policy_checkpoint",
            metrics.PromotionEligibility);
        Assert.Equal(new[] { "policy_checkpoint_evidence:not_present" }, metrics.PolicyCheckpointEvidenceRefs);
        Assert.False(metrics.ProductReady);
        Assert.Equal("product_ready=false", metrics.ProductReadyBoundary);
    }

    [Fact]
    public async Task GetDpoUsefulness_AspNetMapper_MapsKestrelRouteAndAnnotatesLocalOnlyBoundary()
    {
        const string privateMarker = "PRIVATE_PASS145_ASPNET_SECRET";
        var db = await CreateSeededDbAsync(privateMarker);
        var registry = CreateRegistry(db);
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        var routeBuilder = app.MapDesignConversionModelServerDpoUsefulness(registry);
        Assert.NotNull(routeBuilder);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };

            using var response = await client.GetAsync(
                DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath + "?includeBlocked=true&limit=10");
            var bodyJson = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(IPAddress.Loopback.ToString(), baseAddress.Host);
            Assert.True(baseAddress.Port > 0);
            Assert.Equal(
                bool.TrueString,
                response.Headers.GetValues("x-design-conversion-asp-net-host-verified").Single());
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath,
                response.Headers.GetValues("x-design-conversion-asp-net-route").Single());
            Assert.Equal(
                bool.FalseString,
                response.Headers.GetValues("x-design-conversion-public-deployment-verified").Single());
            Assert.DoesNotContain(privateMarker, bodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain("<main", bodyJson, StringComparison.OrdinalIgnoreCase);

            using var document = JsonDocument.Parse(bodyJson);
            var root = document.RootElement;
            Assert.Equal(DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath, root.GetProperty("endpoint").GetString());
            Assert.Equal("GET", root.GetProperty("method").GetString());
            Assert.Equal(DesignConversionModelServerDpoUsefulnessHandler.ApiShape, root.GetProperty("apiShape").GetString());
            Assert.Equal(DesignConversionDpoUsefulnessApi.ApiShape, root.GetProperty("dpoUsefulnessApiShape").GetString());
            Assert.Equal(2, root.GetProperty("rows").GetArrayLength());
            Assert.Equal(2, root.GetProperty("rowCount").GetInt32());
            Assert.Equal(0.88, root.GetProperty("averageUsefulnessScore").GetDouble(), precision: 6);
            Assert.Equal(0.82, root.GetProperty("minimumUsefulnessScore").GetDouble(), precision: 6);
            Assert.Equal(0.94, root.GetProperty("maximumUsefulnessScore").GetDouble(), precision: 6);
            Assert.False(root.GetProperty("productReady").GetBoolean());

            var metadata = root.GetProperty("boundaryMetadata");
            Assert.Equal(nameof(DesignConversionModelServerAspNetEndpointMapper), metadata.GetProperty("asp_net_endpoint_mapper").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_host_verified").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_route_matched").GetString());
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath,
                metadata.GetProperty("asp_net_route_path").GetString());
            Assert.Equal(
                nameof(DesignConversionModelServerDpoUsefulnessHandler),
                metadata.GetProperty("asp_net_handler_dispatch_target").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("dpo_usefulness_response_refs_only").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("public_http_endpoint_verified").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("deployment_verified").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("response_contains_raw_private_data").GetString());
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Fact]
    public void DescribeDpoUsefulnessRoute_DocumentsLocalOnlyRefsOnlyContract()
    {
        var descriptor = DesignConversionModelServerCallerContract.DescribeDpoUsefulnessRoute();

        Assert.Equal(DesignConversionModelServerCallerContract.DpoUsefulnessRouteSchemaVersion, descriptor.SchemaVersion);
        Assert.Equal(DesignConversionModelServerCallerContract.DpoUsefulnessRouteBoundary, descriptor.Boundary);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointMethod, descriptor.EndpointMethod);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath, descriptor.EndpointPath);
        Assert.Contains(descriptor.RequestFields, field =>
            field is { JsonName: "includeBlocked", Required: false, MayCarryPrivateInput: false });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "rows", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "rows[].retrainDecisionStatus", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "rows[].retrainDecisionEvidenceRefs", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "rows[].usefulnessMetrics", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "rows[].usefulnessMetrics.score", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "rows[].usefulnessMetrics.acceptanceReason", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "rows[].usefulnessMetrics.qualityGain", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "rows[].usefulnessMetrics.negativePairUsefulness", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "rows[].usefulnessMetrics.sourceCoverage", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "rows[].usefulnessMetrics.sourceDisjointStatus", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "rows[].usefulnessMetrics.heldOutEvaluationStatus", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "rows[].usefulnessMetrics.promotionEligibility", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "rows[].usefulnessMetrics.policyCheckpointEvidenceRefs", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "rows[].usefulnessMetrics.productReady", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "rowCount", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "averageUsefulnessScore", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "minimumUsefulnessScore", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "maximumUsefulnessScore", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "boundaryMetadata.delegates_to_domain_api", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "boundaryMetadata.dpo_usefulness_row_count", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "boundaryMetadata.dpo_usefulness_average_usefulness_score", Required: true });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: "DPO_USEFULNESS_QUERY_INVALID", StatusCode: 400 });
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["dpo_usefulness_route_discoverable_by_contract"]);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["dpo_usefulness_response_refs_only"]);
        Assert.Equal(nameof(DesignConversionDpoUsefulnessApi), descriptor.BoundaryMetadata["delegates_to_domain_api"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["socket_opened"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["public_http_endpoint_verified"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["cross_repo_caller_verified"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["deployment_verified"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["product_ready"]);
    }

    private static void AssertSanitizedDpoUsefulnessBody(
        DesignConversionModelServerHttpResponse response,
        string privateMarker,
        int expectedRowCount)
    {
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.ContentType);
        Assert.Equal(DesignConversionModelServerDpoUsefulnessHandler.LocalDpoUsefulnessRouteBoundary, response.Headers["x-design-conversion-dpo-usefulness-route"]);
        Assert.Equal(bool.TrueString, response.Headers["x-design-conversion-dpo-usefulness-route-admission-checked"]);
        Assert.Equal(bool.FalseString, response.Headers["x-design-conversion-public-deployment-verified"]);
        Assert.DoesNotContain(privateMarker, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("<main", response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".private", response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/lubao", response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.fig", response.BodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("raw_provider_payload", response.BodyJson, StringComparison.OrdinalIgnoreCase);

        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath, root.GetProperty("endpoint").GetString());
        Assert.Equal("GET", root.GetProperty("method").GetString());
        Assert.Equal(DesignConversionModelServerHttpHandler.LocalHttpSeam, root.GetProperty("httpSeam").GetString());
        Assert.Equal(DesignConversionModelServerDpoUsefulnessHandler.ApiShape, root.GetProperty("apiShape").GetString());
        Assert.Equal(DesignConversionModelServerDpoUsefulnessHandler.RuntimeSurface, root.GetProperty("runtimeSurface").GetString());
        Assert.Equal("dpo-usefulness-readback-completed", root.GetProperty("contractStatus").GetString());
        Assert.Equal(DesignConversionDpoUsefulnessApi.ApiShape, root.GetProperty("dpoUsefulnessApiShape").GetString());
        Assert.Equal(expectedRowCount, root.GetProperty("rows").GetArrayLength());
        Assert.Equal(expectedRowCount, root.GetProperty("rowCount").GetInt32());
        Assert.Equal(1, root.GetProperty("trainReadyCount").GetInt32());
        Assert.Equal(0, root.GetProperty("heldOutCount").GetInt32());
        Assert.Equal(1, root.GetProperty("providerMissingCount").GetInt32());
        Assert.Equal(1, root.GetProperty("blockedCount").GetInt32());
        Assert.Equal(0.88, root.GetProperty("averageUsefulnessScore").GetDouble(), precision: 6);
        Assert.Equal(0.82, root.GetProperty("minimumUsefulnessScore").GetDouble(), precision: 6);
        Assert.Equal(0.94, root.GetProperty("maximumUsefulnessScore").GetDouble(), precision: 6);
        Assert.False(root.GetProperty("productReady").GetBoolean());
        Assert.Contains(root.GetProperty("warnings").EnumerateArray(), item =>
            string.Equals(item.GetString(), "raw_html_css_tree_private_provider_payloads_not_emitted", StringComparison.Ordinal));

        var rows = root.GetProperty("rows").EnumerateArray().ToArray();
        Assert.Contains(rows, row =>
            row.GetProperty("reviewStatus").GetString() == "train-ready" &&
            row.GetProperty("trainReady").GetBoolean());
        Assert.Contains(rows, row =>
            row.GetProperty("reviewStatus").GetString() == "provider-missing" &&
            row.GetProperty("providerMissing").GetBoolean());
        var trainReadyRow = Assert.Single(rows, row =>
            row.GetProperty("reviewStatus").GetString() == "train-ready");
        var usefulnessMetrics = trainReadyRow.GetProperty("usefulnessMetrics");
        Assert.Equal(0.94, usefulnessMetrics.GetProperty("score").GetDouble(), precision: 6);
        Assert.Equal("ready-for-retrain-intake", usefulnessMetrics.GetProperty("acceptanceReason").GetString());
        Assert.Equal(0.57, usefulnessMetrics.GetProperty("qualityGain").GetDouble(), precision: 6);
        Assert.Equal(DesignConversionNegativeReasonTaxonomy.Spacing, usefulnessMetrics.GetProperty("negativePairUsefulness").GetString());
        Assert.Contains("source_kind=Figma", usefulnessMetrics.GetProperty("sourceCoverage").GetString(), StringComparison.Ordinal);
        Assert.Contains("source_ref=present", usefulnessMetrics.GetProperty("sourceCoverage").GetString(), StringComparison.Ordinal);
        Assert.Contains("source_disjoint=not_proven", usefulnessMetrics.GetProperty("sourceCoverage").GetString(), StringComparison.Ordinal);
        Assert.Equal("not_proven", usefulnessMetrics.GetProperty("sourceDisjointStatus").GetString());
        Assert.Equal("not_proven", usefulnessMetrics.GetProperty("heldOutEvaluationStatus").GetString());
        Assert.Equal(
            "retrain_intake_eligible;local_promotion_requires_training_run_measurement_gate_and_policy_checkpoint",
            usefulnessMetrics.GetProperty("promotionEligibility").GetString());
        Assert.Equal(
            "policy_checkpoint_evidence:not_present",
            Assert.Single(usefulnessMetrics.GetProperty("policyCheckpointEvidenceRefs").EnumerateArray()).GetString());
        Assert.False(usefulnessMetrics.GetProperty("productReady").GetBoolean());
        Assert.Equal("product_ready=false", usefulnessMetrics.GetProperty("productReadyBoundary").GetString());

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointRoute, metadata.GetProperty("dpo_usefulness_endpoint_route").GetString());
        Assert.Equal(nameof(DesignConversionDpoUsefulnessApi), metadata.GetProperty("delegates_to_domain_api").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("dpo_usefulness_response_refs_only").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("dpo_usefulness_route_admission_checked").GetString());
        Assert.Equal(
            "admit_status_before_domain_api_when_route_matches",
            metadata.GetProperty("dpo_usefulness_route_admission_policy").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("dpo_usefulness_route_admission_raw_token_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("dpo_usefulness_route_admission_raw_tenant_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("route_admission_raw_tenant_id_exposed").GetString());
        Assert.False(metadata.TryGetProperty("route_admission_scope", out _));
        Assert.Equal(expectedRowCount.ToString(), metadata.GetProperty("dpo_usefulness_row_count").GetString());
        Assert.Equal("1", metadata.GetProperty("dpo_usefulness_train_ready_count").GetString());
        Assert.Equal("0", metadata.GetProperty("dpo_usefulness_held_out_count").GetString());
        Assert.Equal("1", metadata.GetProperty("dpo_usefulness_blocked_count").GetString());
        Assert.Equal("0.88", metadata.GetProperty("dpo_usefulness_average_usefulness_score").GetString());
        Assert.Equal("0.82", metadata.GetProperty("dpo_usefulness_minimum_usefulness_score").GetString());
        Assert.Equal("0.94", metadata.GetProperty("dpo_usefulness_maximum_usefulness_score").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("public_http_endpoint_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("deployment_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_user_private_data_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("provider_payload_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("response_contains_raw_private_data").GetString());
    }

    private static void AssertRouteAdmissionRejectedDpoUsefulness(
        DesignConversionModelServerHttpResponse response,
        int expectedStatusCode,
        string expectedErrorCode,
        string privateMarker,
        string? suppliedToken)
    {
        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.ContentType);
        Assert.Equal(bool.TrueString, response.Headers["x-design-conversion-dpo-usefulness-route-admission-checked"]);
        Assert.Equal(bool.TrueString, response.Headers["x-design-conversion-route-admission-enabled"]);
        Assert.Equal(bool.FalseString, response.Headers["x-design-conversion-route-admission-authorized"]);
        Assert.Equal(bool.TrueString, response.Headers["x-design-conversion-route-admission-tenant-bound"]);

        Assert.DoesNotContain(privateMarker, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(RouteAdmissionCurrentToken, response.BodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(RouteAdmissionTenantId, response.BodyJson, StringComparison.Ordinal);
        if (!string.IsNullOrWhiteSpace(suppliedToken))
            Assert.DoesNotContain(suppliedToken, response.BodyJson, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath, root.GetProperty("endpoint").GetString());
        Assert.Equal("GET", root.GetProperty("method").GetString());
        Assert.Equal(expectedStatusCode, root.GetProperty("statusCode").GetInt32());
        Assert.Equal(expectedErrorCode, root.GetProperty("errorCode").GetString());

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, metadata.GetProperty("dpo_usefulness_route_admission_checked").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("route_admission_enabled").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("route_admission_authorized").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("route_admission_tenant_bound").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("route_admission_raw_tenant_id_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("dpo_usefulness_route_admission_raw_token_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("dpo_usefulness_route_admission_raw_tenant_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("token_value_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("token_hash_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("token_fingerprint_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("response_contains_raw_private_data").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
        Assert.Equal(
            DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath,
            metadata.GetProperty("route_admission_path").GetString());
        Assert.False(metadata.TryGetProperty("route_admission_scope", out _));
    }

    private static DesignConversionModelServerEndpointRegistry CreateRegistry(
        IDesignConversionLocalArtifactDb db,
        DesignConversionModelServerRouteAdmission? routeAdmission = null)
    {
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionDpoUsefulness(
            new DesignConversionModelServerDpoUsefulnessHandler(
                new DesignConversionDpoUsefulnessApi(db),
                routeAdmission));
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        return registry;
    }

    private static DesignConversionModelServerRouteAdmission CreateEnabledRouteAdmission(
        string tenantId = RouteAdmissionTenantId)
        => DesignConversionModelServerRouteAdmission.Create(
            new DesignConversionModelServerRouteAdmissionOptions(
                enabled: true,
                currentToken: RouteAdmissionCurrentToken,
                tenantId: tenantId),
            () => DateTimeOffset.Parse("2026-07-11T12:00:00Z"));

    private static IReadOnlyDictionary<string, string>? RouteAdmissionHeaders(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [DesignConversionModelServerRouteAdmission.AuthorizationHeaderName] = "Bearer " + token.Trim()
        };
    }

    private async Task<JsonlDesignConversionLocalArtifactDb> CreateSeededDbAsync(
        string privateMarker,
        DesignConversionModelServerRouteAdmission? routeAdmission = null)
    {
        var root = CreateOwnedTempPath();
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        await DesignConversionPostEvaluationEligibilityTestFixture.AppendEligibleRecordAsync(
            db,
            BuildArtifactRecord(
                "pass145-train-ready",
                DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                [WithOptionalRouteAdmissionBinding(
                    BuildTrainingRecord("pass145-train-ready", DesignConversionDpoUnits.CriticUnit, 0.94),
                    routeAdmission)],
                privateMarker));
        await DesignConversionPostEvaluationEligibilityTestFixture.AppendEligibleRecordAsync(
            db,
            BuildArtifactRecord(
                "pass145-provider-missing",
                DesignConversionVisualQaBoundaryPolicy.ProviderRequired,
                [WithOptionalRouteAdmissionBinding(
                    BuildTrainingRecord("pass145-provider-missing", DesignConversionDpoUnits.CriticUnit, 0.82),
                    routeAdmission)],
                privateMarker));
        return db;
    }

    private static DesignConversionDpoTrainingRecord WithOptionalRouteAdmissionBinding(
        DesignConversionDpoTrainingRecord record,
        DesignConversionModelServerRouteAdmission? routeAdmission)
        => routeAdmission is null
            ? record
            : WithRouteAdmissionBinding(record, routeAdmission);

    private static DesignConversionDpoTrainingRecord WithRouteAdmissionBinding(
        DesignConversionDpoTrainingRecord record,
        DesignConversionModelServerRouteAdmission routeAdmission)
    {
        var publicMetadata = routeAdmission.PublicMetadata();
        var tenantIdRef = Assert.IsType<string>(publicMetadata["routeAdmissionTenantIdRef"]);
        var scopeRef = Assert.IsType<string>(publicMetadata["routeAdmissionScopeRef"]);
        var metadata = new Dictionary<string, string>(record.Provenance.Metadata, StringComparer.Ordinal)
        {
            [DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionTenantBoundMetadataKey] = bool.TrueString,
            [DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionTenantIdRefMetadataKey] = tenantIdRef,
            [DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionScopeRefMetadataKey] = scopeRef
        };
        return record with
        {
            Provenance = record.Provenance with { Metadata = metadata }
        };
    }

    private string CreateOwnedTempPath()
    {
        var path = Path.GetFullPath(
            Path.Combine(
                NormalizedTempRoot(),
                OwnedTempPathPrefix + Guid.NewGuid().ToString("N")));
        ValidateOwnedTempPath(path);
        if (!_ownedTempPaths.Add(path))
            throw new InvalidOperationException("Owned DPO-usefulness test path registration collided.");

        return path;
    }

    private void DeleteOwnedTempPath(string path)
    {
        var normalizedPath = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(path));
        if (!_ownedTempPaths.Contains(normalizedPath))
            return;

        ValidateOwnedTempPath(normalizedPath);
        if (Directory.Exists(normalizedPath))
            Directory.Delete(normalizedPath, recursive: true);

        _ownedTempPaths.Remove(normalizedPath);
    }

    private static void ValidateOwnedTempPath(string path)
    {
        var normalizedPath = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(path));
        var tempRoot = NormalizedTempRoot();
        var parent = Directory.GetParent(normalizedPath);
        var leafName = Path.GetFileName(normalizedPath);
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        var suffix = leafName.StartsWith(OwnedTempPathPrefix, StringComparison.Ordinal)
            ? leafName[OwnedTempPathPrefix.Length..]
            : string.Empty;
        if (parent is null ||
            !string.Equals(
                Path.TrimEndingDirectorySeparator(Path.GetFullPath(parent.FullName)),
                tempRoot,
                comparison) ||
            string.Equals(normalizedPath, tempRoot, comparison) ||
            !leafName.StartsWith(OwnedTempPathPrefix, StringComparison.Ordinal) ||
            !Guid.TryParseExact(suffix, "N", out _))
        {
            throw new InvalidOperationException(
                $"Owned DPO-usefulness test cleanup path must match %TEMP%/{OwnedTempPathPrefix}<N-format GUID>.");
        }
    }

    private static string NormalizedTempRoot()
        => Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(Path.GetTempPath()));

    private static DesignConversionDpoTrainingRecord BuildTrainingRecord(
        string id,
        string unitId,
        double usefulness)
    {
        var example = DesignConversionDpoGeneratorTests.CreateExample(id, unitId, usefulness) with
        {
            Provenance = new DesignConversionDpoProvenance(
                SourceRequestId: "pass145-design-request-redacted",
                SourceKind: DesignConversionSourceKind.Figma.ToString(),
                SourceManifestRef: "manifest://design-conversion/pass145-source-redacted",
                ReviewerRef: "teacher:pass145-redacted",
                CreatedAtUtc: DateTimeOffset.UnixEpoch,
                Metadata: new Dictionary<string, string>
                {
                    ["split"] = "train",
                    ["source_quality"] = "teacher-reviewed"
                })
        };
        var build = new DesignConversionDpoGenerator().BuildTrainReadyRecords([example]);
        Assert.True(build.IsSuccess, build.ErrorMessage);
        return build.Value!.AllRecords.Single();
    }

    private static DesignConversionLocalArtifactDbRecord BuildArtifactRecord(
        string recordId,
        string visualJudgeStatus,
        IReadOnlyList<DesignConversionDpoTrainingRecord> records,
        string privateMarker)
        => new(
            RecordId: recordId,
            DatabaseKind: JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind,
            JobId: $"{recordId}:job",
            RequestId: $"{recordId}:request",
            SourceKind: DesignConversionSourceKind.Figma.ToString(),
            Tier: "pro",
            SanitizedSourceRefs: new Dictionary<string, string>
            {
                ["source_manifest_ref"] = $"file:///C:/Users/lubao/Documents/private.fig?token={privateMarker}",
                ["source_kind"] = "figma"
            },
            ConversionResult: new DesignConversionConversionResultArtifact(
                ResultId: $"{recordId}:conversion-result",
                HtmlRef: $"<main data-secret=\"{privateMarker}\">exact_prompt private html</main>",
                HtmlSha256: string.Empty,
                CssRef: $".private{{background:url('C:/Users/lubao/{privateMarker}.png');}}",
                CssSha256: string.Empty,
                Metadata: new Dictionary<string, string>
                {
                    ["raw_provider_payload"] = privateMarker
                }),
            RenderArtifactRefs:
            [
                new DesignConversionRenderArtifactDbRef(
                    ViewportId: "desktop",
                    ArtifactId: $"{recordId}:render:desktop",
                    Kind: "browser_screenshot",
                    UriRef: $"file:///C:/Users/lubao/private/{privateMarker}/render-desktop.png",
                    Sha256: "sha256:render-desktop",
                    ByteLength: 1234,
                    Width: 1440,
                    Height: 900,
                    Metadata: new Dictionary<string, string>
                    {
                        ["private_marker"] = privateMarker
                    })
            ],
            VisualJudgeStatus: new DesignConversionVisualJudgeArtifactDbStatus(
                Status: visualJudgeStatus,
                ProviderBoundary: visualJudgeStatus == DesignConversionVisualQaBoundaryPolicy.ProviderRequired
                    ? DesignConversionVisualQaBoundaryPolicy.ProviderRequired
                    : DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                ExternalCallExecuted: false,
                EstimatedCostUsd: null,
                TierDecision: visualJudgeStatus == DesignConversionVisualQaBoundaryPolicy.ProviderRequired
                    ? DesignConversionVisualQaBoundaryPolicy.ProviderRequired
                    : DesignConversionVisualQaBoundaryPolicy.CostCapMarker,
                EvidenceRefs: [visualJudgeStatus, "visual_judge_status", "product_not_ready"],
                Metadata: new Dictionary<string, string>
                {
                    ["visual_judge_status"] = visualJudgeStatus,
                    ["raw_provider_payload"] = privateMarker
                }),
            DpoCandidates: records
                .Select(record => DesignConversionLocalArtifactDbDpoCandidate.FromTrainingRecord(record))
                .ToArray(),
            CreatedAtUtc: DateTimeOffset.UnixEpoch,
            EvidenceRefs:
            [
                "storage_training_artifact_db",
                "local_artifact_db",
                "conversion_result",
                "render_artifact_refs",
                "visual_judge_status",
                "dpo_candidate",
                "usefulness_score",
                "training_candidate_only",
                "product_not_ready"
            ],
            Metadata: new Dictionary<string, string>
            {
                ["private_marker"] = privateMarker
            });

    private sealed class CountingLocalArtifactDb : IDesignConversionLocalArtifactDb
    {
        private readonly IDesignConversionLocalArtifactDb _inner;
        private int _queryDisplayRowsCallCount;

        public CountingLocalArtifactDb(IDesignConversionLocalArtifactDb inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public int QueryDisplayRowsCallCount => Volatile.Read(ref _queryDisplayRowsCallCount);

        public Task<OperationResult<DesignConversionLocalArtifactDbRecord>> AppendRecordAsync(
            DesignConversionLocalArtifactDbRecord record,
            CancellationToken ct = default)
            => _inner.AppendRecordAsync(record, ct);

        public Task<OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>> ListRecordsAsync(
            DesignConversionLocalArtifactDbQuery? query = null,
            CancellationToken ct = default)
            => _inner.ListRecordsAsync(query, ct);

        public Task<OperationResult<IReadOnlyList<DesignConversionDpoTrainingRecord>>> QueryApprovedTrainingCandidatesAsync(
            DesignConversionLocalArtifactDbQuery? query = null,
            CancellationToken ct = default)
            => _inner.QueryApprovedTrainingCandidatesAsync(query, ct);

        public Task<OperationResult<DesignConversionApprovedDpoHandoffArtifact>> ExportApprovedRuntimeDpoCandidatesAsync(
            DesignConversionLocalArtifactDbQuery? query = null,
            DateTimeOffset? exportedAtUtc = null,
            CancellationToken ct = default)
            => _inner.ExportApprovedRuntimeDpoCandidatesAsync(query, exportedAtUtc, ct);

        public Task<OperationResult<DesignConversionTrainingIntakeRecord>> CreateTrainingIntakeAsync(
            DesignConversionApprovedDpoHandoffArtifact handoff,
            string requestedByRef,
            string reason,
            DateTimeOffset? requestedAtUtc = null,
            CancellationToken ct = default,
            IReadOnlyDictionary<string, string>? admissionBoundaryMetadata = null)
            => _inner.CreateTrainingIntakeAsync(handoff, requestedByRef, reason, requestedAtUtc, ct, admissionBoundaryMetadata);

        public Task<OperationResult<IReadOnlyList<DesignConversionTrainingIntakeRecord>>> ListTrainingIntakeRequestsAsync(
            CancellationToken ct = default)
            => _inner.ListTrainingIntakeRequestsAsync(ct);

        public Task<OperationResult<DesignConversionTrainingIntakePreflightResult>> RunTrainingIntakePreflightAsync(
            string intakeId,
            CancellationToken ct = default)
            => _inner.RunTrainingIntakePreflightAsync(intakeId, ct);

        public Task<OperationResult<DesignConversionTrainingIntakeRunResult>> RunTrainingIntakeAsync(
            string intakeId,
            DesignConversionTrainingPipelineOptions? options = null,
            CancellationToken ct = default,
            IReadOnlyDictionary<string, string>? admissionBoundaryMetadata = null)
            => _inner.RunTrainingIntakeAsync(intakeId, options, ct, admissionBoundaryMetadata);

        public Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>> RunTrainingIntakeAndPromoteLocalAsync(
            string intakeId,
            string promotionOutputDirectory,
            DesignConversionTrainingPipelineOptions? options = null,
            CancellationToken ct = default)
            => _inner.RunTrainingIntakeAndPromoteLocalAsync(intakeId, promotionOutputDirectory, options, ct);

        public Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackResult>> RollbackTrainingIntakeLocalPromotionAsync(
            string promotedLocalModelRef,
            string requestedByRef,
            string reason,
            DateTimeOffset? requestedAtUtc = null,
            CancellationToken ct = default)
            => _inner.RollbackTrainingIntakeLocalPromotionAsync(promotedLocalModelRef, requestedByRef, reason, requestedAtUtc, ct);

        public Task<OperationResult<DesignConversionDpoCandidateApprovalResult>> ApproveRuntimeCreatedDpoCandidateAsync(
            string localArtifactRecordId,
            string candidateOrTrainingRecordId,
            string reviewerRef,
            string reason,
            DateTimeOffset? approvedAtUtc = null,
            CancellationToken ct = default)
            => _inner.ApproveRuntimeCreatedDpoCandidateAsync(
                localArtifactRecordId,
                candidateOrTrainingRecordId,
                reviewerRef,
                reason,
                approvedAtUtc,
                ct);

        public Task<OperationResult<DesignConversionDpoDisplayResult>> QueryDisplayRowsAsync(
            DesignConversionDpoDisplayQuery? query = null,
            CancellationToken ct = default)
        {
            Interlocked.Increment(ref _queryDisplayRowsCallCount);
            return _inner.QueryDisplayRowsAsync(query, ct);
        }
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
            throw new InvalidOperationException("Request body should not be read before route admission rejects.");
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            ReadAttempted = true;
            throw new InvalidOperationException("Request body should not be read before route admission rejects.");
        }

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();
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
}
