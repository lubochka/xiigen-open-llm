// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Buffers.Binary;
using System.IO.Compression;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DesignConversionDomain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelServer.DesignConversion;
using DesignDomain = DesignConversionDomain.DesignConversionDomain;

[Collection(DesignConversionLocalHostTestCollectionNames.CollectionName)]
public sealed class DesignConversionModelServerAspNetHostTests
{
    private const string Flow47VerifiedKey = "flow47_verified";

    [Fact]
    public async Task NonTrainingStartup_MapsExactlyImprovementAndFlow38WithExactStartupMetadata()
    {
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            new RecordingBrowserAdapter(),
            new RecordingComparisonProvider(score: 0.96));
        var registry = DesignConversionModelServerAspNetHostStartup.CreateNonTrainingLocalRouteRegistry(
            composition,
            new UnexpectedCandidateStrategy(),
            new UnexpectedFlow38QuarantineStore(),
            "non-training-flow38-test-token-0123456789");
        var expectedRoutes = DesignConversionModelServerAspNetHostStartup.NonTrainingLocalRouteSet();

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = [] });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
        await using var app = builder.Build();
        app.ConfigureNonTrainingDesignConversionModelServer(registry);

        await app.StartAsync();
        try
        {
            var mappedEndpoints = app.Services
                .GetRequiredService<EndpointDataSource>()
                .Endpoints
                .OfType<RouteEndpoint>()
                .Where(endpoint => endpoint.Metadata
                    .GetMetadata<DesignConversionModelServerAspNetHostStartupMetadata>() is not null)
                .ToArray();
            Assert.Equal(2, mappedEndpoints.Length);
            Assert.Equal(
                expectedRoutes.Order(StringComparer.Ordinal),
                mappedEndpoints
                    .Select(endpoint => '/' + (endpoint.RoutePattern.RawText ?? string.Empty).TrimStart('/'))
                    .Order(StringComparer.Ordinal));
            Assert.All(mappedEndpoints, endpoint =>
            {
                var metadata = endpoint.Metadata
                    .GetMetadata<DesignConversionModelServerAspNetHostStartupMetadata>();
                Assert.NotNull(metadata);
                Assert.Equal(expectedRoutes, metadata!.MappedRoutes);
                Assert.Contains(metadata.MappedRoute, expectedRoutes);
                Assert.Equal(
                    [HttpMethod.Post.Method],
                    endpoint.Metadata.GetMetadata<HttpMethodMetadata>()!.HttpMethods);
            });

            using var client = new HttpClient
            {
                BaseAddress = ResolveStartedAddress(app),
                Timeout = TimeSpan.FromSeconds(5)
            };
            var forbiddenRoutes = new (HttpMethod Method, string Path)[]
            {
                (HttpMethod.Post, DesignConversionModelServerEndpointRegistry.EndpointPath),
                (HttpMethod.Get, DesignConversionModelServerStatusHandler.EndpointPathPrefix + "non-training-forbidden-job"),
                (HttpMethod.Get, DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath),
                (HttpMethod.Post, DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath),
                (HttpMethod.Post, DesignConversionModelServerEndpointRegistry.ModelActivationEndpointPath),
                (HttpMethod.Post, DesignConversionModelServerEndpointRegistry.HumanApprovalChallengeEndpointPath),
                (HttpMethod.Post, DesignConversionModelServerEndpointRegistry.HumanApprovalApproveEndpointPath)
            };

            foreach (var forbidden in forbiddenRoutes)
            {
                using var request = new HttpRequestMessage(forbidden.Method, forbidden.Path);
                if (forbidden.Method == HttpMethod.Post)
                    request.Content = new StringContent("{}", Encoding.UTF8, "application/json");
                using var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                Assert.Empty(responseBody);
                Assert.False(response.Headers.Contains("x-design-conversion-endpoint"));
                Assert.DoesNotContain("ENDPOINT_ROUTE_NOT_FOUND", responseBody, StringComparison.Ordinal);
                Assert.DoesNotContain("FLOW38_QUARANTINE_INTAKE_NOT_CONFIGURED", responseBody, StringComparison.Ordinal);
            }

            using (var improvementRequest = new HttpRequestMessage(
                       HttpMethod.Post,
                       DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath)
                   {
                       Content = new StringContent("{}", Encoding.UTF8, "application/json")
                   })
            using (var improvementResponse = await client.SendAsync(improvementRequest))
            {
                Assert.NotEqual(HttpStatusCode.NotFound, improvementResponse.StatusCode);
                Assert.Equal(
                    DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
                    improvementResponse.Headers.GetValues("x-design-conversion-endpoint").Single());
            }

            using (var flow38Request = new HttpRequestMessage(
                       HttpMethod.Post,
                       DesignConversionModelServerEndpointRegistry.Flow38QuarantineEndpointPath)
                   {
                       Content = new StringContent("{}", Encoding.UTF8, "application/json")
                   })
            using (var flow38Response = await client.SendAsync(flow38Request))
            {
                Assert.Equal(HttpStatusCode.Unauthorized, flow38Response.StatusCode);
                Assert.Equal(
                    DesignConversionModelServerEndpointRegistry.Flow38QuarantineEndpointPath,
                    flow38Response.Headers.GetValues("x-design-conversion-endpoint").Single());
            }
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Fact]
    public async Task PostTrainingIntake_ProductionHost_ReusesLifecycleKeyRingIndependentlyOfActivationStorage()
    {
        const string currentToken = "training-intake-current-key-ring-token-7F31";
        const string previousToken = "training-intake-previous-key-ring-token-8C42";
        const string wrongToken = "training-intake-wrong-key-ring-token-9D53";
        const string tenantId = "tenant:training-intake-lifecycle-key-ring";
        const string privateMarker = "training-intake-key-ring-private-marker-AE64";
        var now = DateTimeOffset.UnixEpoch.AddHours(900);
        var previousExpiresAtUtc = now.AddMinutes(10);
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "training-intake-production-lifecycle-key-ring");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            Path.Combine(tempRoot, "artifact-db"));
        var seededHandoff = await DesignConversionPostEvaluationEligibilityTestFixture
            .SeedApprovedHandoffAsync(artifactDb, "training-intake-production-key-ring");
        Assert.Single(seededHandoff.Rows);
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            new RecordingBrowserAdapter(),
            new RecordingComparisonProvider(score: 0.96));
        var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            composition,
            new UnexpectedCandidateStrategy(),
            artifactDb,
            () => now,
            modelLifecycleTokenKeyRing: new DesignConversionModelServerModelLifecycleTokenKeyRingOptions(
                currentToken,
                previousToken,
                previousExpiresAtUtc));

        async Task<int> CountTrainingIntakesAsync()
        {
            var intakes = await artifactDb.ListTrainingIntakeRequestsAsync();
            Assert.True(intakes.IsSuccess, intakes.ErrorMessage);
            return intakes.Value!.Count;
        }

        Assert.Equal(0, await CountTrainingIntakesAsync());
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = [] });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(registry);

        await app.StartAsync();
        try
        {
            using var client = new HttpClient
            {
                BaseAddress = ResolveStartedAddress(app),
                Timeout = TimeSpan.FromSeconds(5)
            };

            using (var activationResponse = await client.PostAsync(
                       DesignConversionModelServerEndpointRegistry.ModelActivationEndpointPath,
                       new StringContent("{}", Encoding.UTF8, "application/json")))
            {
                var activationBody = await activationResponse.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.ServiceUnavailable, activationResponse.StatusCode);
                Assert.Contains(
                    "DESIGN_CONVERSION_MODEL_ACTIVATION_STATE_ROOT_NOT_CONFIGURED",
                    activationBody,
                    StringComparison.Ordinal);
                Assert.DoesNotContain(currentToken, activationBody, StringComparison.Ordinal);
                Assert.DoesNotContain(previousToken, activationBody, StringComparison.Ordinal);
            }

            using (var listResponse = await client.PostAsync(
                       DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                       new StringContent(
                           JsonSerializer.Serialize(
                               new
                               {
                                   action = "list",
                                   metadata = new Dictionary<string, string>
                                   {
                                       ["private_marker"] = privateMarker
                                   }
                               },
                               DesignConversionModelServerHttpHandler.JsonOptions),
                           Encoding.UTF8,
                           "application/json")))
            {
                var listBody = await listResponse.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
                Assert.DoesNotContain(currentToken, listBody, StringComparison.Ordinal);
                Assert.DoesNotContain(previousToken, listBody, StringComparison.Ordinal);
                Assert.DoesNotContain(privateMarker, listBody, StringComparison.Ordinal);
                using var listDocument = JsonDocument.Parse(listBody);
                var listMetadata = listDocument.RootElement.GetProperty("boundaryMetadata");
                Assert.Equal(bool.FalseString, listMetadata.GetProperty("training_intake_mutating_admission_boundary_required").GetString());
                Assert.Equal(bool.TrueString, listMetadata.GetProperty("training_intake_auth_token_configured").GetString());
                Assert.Equal(bool.FalseString, listMetadata.GetProperty("training_intake_auth_token_configuration_required").GetString());
                Assert.Equal(bool.FalseString, listMetadata.GetProperty("training_intake_auth_token_required").GetString());
                Assert.Equal(bool.FalseString, listMetadata.GetProperty("training_intake_auth_token_enforced").GetString());
                Assert.Equal(bool.FalseString, listMetadata.GetProperty("training_intake_auth_token_verified").GetString());
                Assert.Equal("model_lifecycle_token_key_ring", listMetadata.GetProperty("training_intake_auth_authority").GetString());
                Assert.Equal(
                    "model_lifecycle_token_key_ring_required_for_runtime_candidate_approval_export",
                    listMetadata.GetProperty("training_intake_auth_admission_scope").GetString());
            }

            async Task<(HttpStatusCode StatusCode, string Body)> ExportAsync(
                string idempotencyKey,
                string? token)
            {
                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath)
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(
                            new
                            {
                                action = DesignConversionModelServerTrainingIntakeHandler.ActionExportApprovedRuntimeCandidates,
                                tenantId,
                                idempotencyKey,
                                requestedAtUtc = now,
                                metadata = new Dictionary<string, string>
                                {
                                    ["private_marker"] = privateMarker
                                }
                            },
                            DesignConversionModelServerHttpHandler.JsonOptions),
                        Encoding.UTF8,
                        "application/json")
                };
                if (token is not null)
                {
                    request.Headers.TryAddWithoutValidation(
                        DesignConversionModelServerTrainingIntakeHandler.TrainingIntakeAuthTokenHeaderName,
                        token);
                }

                using var response = await client.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();
                Assert.DoesNotContain(currentToken, body, StringComparison.Ordinal);
                Assert.DoesNotContain(previousToken, body, StringComparison.Ordinal);
                Assert.DoesNotContain(wrongToken, body, StringComparison.Ordinal);
                Assert.DoesNotContain(privateMarker, body, StringComparison.Ordinal);
                return (response.StatusCode, body);
            }

            var currentAccepted = await ExportAsync(
                "export-current-key-ring",
                currentToken);
            Assert.Equal(HttpStatusCode.OK, currentAccepted.StatusCode);
            using (var currentDocument = JsonDocument.Parse(currentAccepted.Body))
            {
                var currentRoot = currentDocument.RootElement;
                Assert.Equal(
                    1,
                    currentRoot.GetProperty("approvedHandoff").GetProperty("approvedCandidateCount").GetInt32());
                var metadata = currentRoot.GetProperty("boundaryMetadata");
                Assert.Equal(bool.TrueString, metadata.GetProperty("training_intake_mutating_admission_boundary_required").GetString());
                Assert.Equal(bool.TrueString, metadata.GetProperty("training_intake_auth_token_configured").GetString());
                Assert.Equal(bool.TrueString, metadata.GetProperty("training_intake_auth_token_configuration_required").GetString());
                Assert.Equal(bool.TrueString, metadata.GetProperty("training_intake_auth_token_required").GetString());
                Assert.Equal(bool.TrueString, metadata.GetProperty("training_intake_auth_token_enforced").GetString());
                Assert.Equal(bool.TrueString, metadata.GetProperty("training_intake_auth_token_verified").GetString());
                Assert.Equal("model_lifecycle_token_key_ring", metadata.GetProperty("training_intake_auth_authority").GetString());
            }
            var currentExport = JsonSerializer.Deserialize<DesignConversionModelServerTrainingIntakeResponse>(
                currentAccepted.Body,
                DesignConversionModelServerHttpHandler.JsonOptions);
            Assert.NotNull(currentExport?.ApprovedHandoff);
            Assert.Equal(0, await CountTrainingIntakesAsync());

            var previousAccepted = await ExportAsync(
                "export-previous-key-ring",
                previousToken);
            Assert.Equal(HttpStatusCode.OK, previousAccepted.StatusCode);
            Assert.Equal(0, await CountTrainingIntakesAsync());

            var missingRejected = await ExportAsync(
                "export-missing-key-ring",
                token: null);
            Assert.Equal(HttpStatusCode.Unauthorized, missingRejected.StatusCode);
            Assert.Contains("TRAINING_INTAKE_AUTH_TOKEN_REQUIRED", missingRejected.Body, StringComparison.Ordinal);
            Assert.Equal(0, await CountTrainingIntakesAsync());

            var wrongRejected = await ExportAsync(
                "export-wrong-key-ring",
                wrongToken);
            Assert.Equal(HttpStatusCode.Unauthorized, wrongRejected.StatusCode);
            Assert.Contains("TRAINING_INTAKE_AUTH_TOKEN_INVALID", wrongRejected.Body, StringComparison.Ordinal);
            Assert.Equal(0, await CountTrainingIntakesAsync());

            now = previousExpiresAtUtc.AddTicks(1);
            var expiredPreviousRejected = await ExportAsync(
                "export-expired-previous-key-ring",
                previousToken);
            Assert.Equal(HttpStatusCode.Unauthorized, expiredPreviousRejected.StatusCode);
            Assert.Contains("TRAINING_INTAKE_AUTH_TOKEN_INVALID", expiredPreviousRejected.Body, StringComparison.Ordinal);
            Assert.Equal(0, await CountTrainingIntakesAsync());

            using var createResponse = await client.PostAsync(
                DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                new StringContent(
                    JsonSerializer.Serialize(
                        new
                        {
                            action = DesignConversionModelServerTrainingIntakeHandler.ActionCreate,
                            tenantId,
                            idempotencyKey = "create-from-current-export-without-auth",
                            handoff = currentExport!.ApprovedHandoff,
                            requestedByRef = "architect:production-lifecycle-key-ring",
                            reason = "create intake from authenticated approved runtime export",
                            requestedAtUtc = now
                        },
                        DesignConversionModelServerHttpHandler.JsonOptions),
                    Encoding.UTF8,
                    "application/json"));
            var createBody = await createResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            Assert.DoesNotContain(currentToken, createBody, StringComparison.Ordinal);
            Assert.DoesNotContain(previousToken, createBody, StringComparison.Ordinal);
            using (var createDocument = JsonDocument.Parse(createBody))
            {
                var metadata = createDocument.RootElement.GetProperty("boundaryMetadata");
                Assert.Equal(bool.TrueString, metadata.GetProperty("training_intake_mutating_admission_boundary_required").GetString());
                Assert.Equal(bool.TrueString, metadata.GetProperty("training_intake_auth_token_configured").GetString());
                Assert.Equal(bool.FalseString, metadata.GetProperty("training_intake_auth_token_configuration_required").GetString());
                Assert.Equal(bool.FalseString, metadata.GetProperty("training_intake_auth_token_required").GetString());
                Assert.Equal(bool.FalseString, metadata.GetProperty("training_intake_auth_token_enforced").GetString());
                Assert.Equal(bool.FalseString, metadata.GetProperty("training_intake_auth_token_verified").GetString());
                Assert.Equal("model_lifecycle_token_key_ring", metadata.GetProperty("training_intake_auth_authority").GetString());
            }
            Assert.Equal(1, await CountTrainingIntakesAsync());
        }
        finally
        {
            await app.StopAsync();
        }

        var noAuthorityRegistry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            composition,
            new UnexpectedCandidateStrategy(),
            artifactDb,
            () => now);
        var noAuthorityBuilder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = [] });
        noAuthorityBuilder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
        await using var noAuthorityApp = noAuthorityBuilder.Build();
        noAuthorityApp.ConfigureDesignConversionModelServer(noAuthorityRegistry);
        await noAuthorityApp.StartAsync();
        try
        {
            using var client = new HttpClient
            {
                BaseAddress = ResolveStartedAddress(noAuthorityApp),
                Timeout = TimeSpan.FromSeconds(5)
            };
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(
                        new
                        {
                            action = DesignConversionModelServerTrainingIntakeHandler.ActionExportApprovedRuntimeCandidates,
                            tenantId,
                            idempotencyKey = "export-no-authority",
                            requestedAtUtc = now
                        },
                        DesignConversionModelServerHttpHandler.JsonOptions),
                    Encoding.UTF8,
                    "application/json")
            };
            request.Headers.TryAddWithoutValidation(
                DesignConversionModelServerTrainingIntakeHandler.TrainingIntakeAuthTokenHeaderName,
                currentToken);
            using var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            Assert.Contains("TRAINING_INTAKE_AUTH_TOKEN_NOT_CONFIGURED", body, StringComparison.Ordinal);
            Assert.DoesNotContain(currentToken, body, StringComparison.Ordinal);
            using (var document = JsonDocument.Parse(body))
            {
                var metadata = document.RootElement.GetProperty("boundaryMetadata");
                Assert.Equal(bool.FalseString, metadata.GetProperty("training_intake_auth_token_configured").GetString());
                Assert.Equal("not_configured", metadata.GetProperty("training_intake_auth_authority").GetString());
            }
            Assert.Equal(1, await CountTrainingIntakesAsync());
        }
        finally
        {
            await noAuthorityApp.StopAsync();
        }

        var invalidAuthorityRegistry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            composition,
            new UnexpectedCandidateStrategy(),
            artifactDb,
            () => now,
            modelLifecycleTokenKeyRing: new DesignConversionModelServerModelLifecycleTokenKeyRingOptions(
                "too-short"));
        var invalidAuthorityResult = await invalidAuthorityRegistry.InvokeAsync(
            new DesignConversionModelServerHttpRequest(
                "POST",
                DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                JsonSerializer.Serialize(
                    new
                    {
                        action = DesignConversionModelServerTrainingIntakeHandler.ActionExportApprovedRuntimeCandidates,
                        tenantId,
                        idempotencyKey = "export-invalid-authority",
                        requestedAtUtc = now
                    },
                    DesignConversionModelServerHttpHandler.JsonOptions),
                "application/json",
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [DesignConversionModelServerTrainingIntakeHandler.TrainingIntakeAuthTokenHeaderName] = currentToken
                }));
        Assert.True(invalidAuthorityResult.IsSuccess, invalidAuthorityResult.ErrorMessage);
        Assert.Equal(503, invalidAuthorityResult.Value!.StatusCode);
        Assert.Contains(
            "TRAINING_INTAKE_AUTH_TOKEN_NOT_CONFIGURED",
            invalidAuthorityResult.Value.BodyJson,
            StringComparison.Ordinal);
        Assert.DoesNotContain(currentToken, invalidAuthorityResult.Value.BodyJson, StringComparison.Ordinal);
        Assert.Equal(1, await CountTrainingIntakesAsync());

        DeleteDirectoryIfExists(tempRoot);
    }

    [Fact]
    public async Task NonDurableStartup_MapsExactlyNineRoutesAndExcludesDpoAndTrainingIntake()
    {
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            new RecordingBrowserAdapter(),
            new RecordingComparisonProvider(score: 0.96));
        var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            composition,
            new UnexpectedCandidateStrategy(),
            artifactDb: null,
            flow38QuarantineStore: new UnexpectedFlow38QuarantineStore(),
            flow38QuarantineBearerToken: "non-durable-flow38-test-token-0123456789");
        var expectedRoutes = DesignConversionModelServerAspNetHostStartup.NonDurableLocalRouteSet();

        Assert.Equal(9, expectedRoutes.Count);
        Assert.Contains(DesignConversionModelServerEndpointRegistry.Flow38QuarantineEndpointPath, expectedRoutes);
        Assert.DoesNotContain(DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath, expectedRoutes);
        Assert.DoesNotContain(DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath, expectedRoutes);

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = [] });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
        await using var app = builder.Build();
        app.ConfigureNonDurableDesignConversionModelServer(registry);

        await app.StartAsync();
        try
        {
            var mappedEndpoints = app.Services
                .GetRequiredService<EndpointDataSource>()
                .Endpoints
                .OfType<RouteEndpoint>()
                .Select(endpoint => new
                {
                    Endpoint = endpoint,
                    Metadata = endpoint.Metadata.GetMetadata<DesignConversionModelServerAspNetHostStartupMetadata>()
                })
                .Where(item => item.Metadata is not null)
                .ToArray();

            Assert.Equal(9, mappedEndpoints.Length);
            Assert.Equal(
                expectedRoutes.Order(StringComparer.Ordinal),
                mappedEndpoints.Select(item => item.Metadata!.MappedRoute).Order(StringComparer.Ordinal));
            Assert.All(mappedEndpoints, item =>
            {
                Assert.Equal(expectedRoutes, item.Metadata!.MappedRoutes);
                Assert.Contains(item.Metadata.MappedRoute, expectedRoutes);
                Assert.True(item.Metadata.LocalOnly);
                Assert.False(item.Metadata.ExternalProviderConfigured);
            });

            using var client = new HttpClient
            {
                BaseAddress = ResolveStartedAddress(app),
                Timeout = TimeSpan.FromSeconds(5)
            };
            using var dpoResponse = await client.GetAsync(
                DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath);
            Assert.Equal(HttpStatusCode.NotFound, dpoResponse.StatusCode);
            using var trainingContent = new StringContent("{}", Encoding.UTF8, "application/json");
            using var trainingResponse = await client.PostAsync(
                DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                trainingContent);
            Assert.Equal(HttpStatusCode.NotFound, trainingResponse.StatusCode);
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Fact]
    public void Flow38PublicBoundarySnapshot_MissingMalformedAndUnexpectedMetadataFailsClosed()
    {
        var absent = DesignConversionFlow38PublicBoundarySnapshot.FromRegistry(
            new DesignConversionModelServerEndpointRegistry());
        Assert.False(absent.RouteMapped);
        Assert.False(absent.MetadataValid);
        Assert.False(absent.Configured);
        Assert.Equal(
            DesignConversionFlow38PublicBoundarySnapshot.RouteNotRegisteredStatus,
            absent.MetadataStatus);

        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionFlow38Quarantine(
            new DesignConversionModelServerFlow38QuarantineHandler(
                new UnexpectedFlow38QuarantineStore(),
                "flow38-snapshot-test-token-0123456789"));
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        var registration = registered.Value!;

        var valid = DesignConversionFlow38PublicBoundarySnapshot.FromRegistration(registration);
        Assert.True(valid.MetadataValid);
        Assert.Equal(DesignConversionFlow38PublicBoundarySnapshot.ValidStatus, valid.MetadataStatus);
        Assert.True(valid.Configured);
        Assert.True(valid.QuarantineOnly);
        Assert.True(valid.StoreConfigured);
        Assert.False(valid.Persisted);
        Assert.Equal(DesignConversionModelServerFlow38QuarantineHandler.QuarantineVerdict, valid.LegalVerdict);
        Assert.Equal(DesignConversionModelServerFlow38QuarantineHandler.PersonalAdaptiveUse, valid.AllowedUse);
        Assert.Equal(DesignConversionModelServerFlow38QuarantineHandler.AdaptiveOrQuarantineRoute, valid.Route);
        Assert.False(valid.DpoTrainReady);
        Assert.False(valid.DpoPairCreated);
        Assert.False(valid.TrainingTriggered);
        Assert.False(valid.CommonTrainingAllowed);
        Assert.False(valid.RawOutputAccepted);
        Assert.True(valid.RefsOnlyPublicResponse);
        Assert.False(valid.ProductReady);

        var missingMetadata = new Dictionary<string, string>(registration.BoundaryMetadata, StringComparer.Ordinal);
        missingMetadata.Remove("legal_verdict");
        var missing = DesignConversionFlow38PublicBoundarySnapshot.FromRegistration(
            registration with { BoundaryMetadata = missingMetadata });
        Assert.False(missing.MetadataValid);
        Assert.False(missing.Configured);
        Assert.Equal(DesignConversionFlow38PublicBoundarySnapshot.MissingValue, missing.LegalVerdict);
        Assert.Contains("missing:legal_verdict", missing.MetadataIssues);

        var malformedMetadata = new Dictionary<string, string>(registration.BoundaryMetadata, StringComparer.Ordinal)
        {
            ["dpo_train_ready"] = "not-a-boolean"
        };
        var malformed = DesignConversionFlow38PublicBoundarySnapshot.FromRegistration(
            registration with { BoundaryMetadata = malformedMetadata });
        Assert.False(malformed.MetadataValid);
        Assert.False(malformed.Configured);
        Assert.False(malformed.DpoTrainReady);
        Assert.Contains("malformed:dpo_train_ready", malformed.MetadataIssues);

        var nonCanonicalBooleanCases = new (string Key, string Value)[]
        {
            ("flow38_quarantine_store_configured", "true"),
            ("dpo_train_ready", "false"),
            ("training_triggered", " false "),
            ("flow38_quarantine_only", " True"),
            ("product_ready", "False "),
            ("refs_only_public_response", "true")
        };
        foreach (var nonCanonicalCase in nonCanonicalBooleanCases)
        {
            var nonCanonicalMetadata = new Dictionary<string, string>(
                registration.BoundaryMetadata,
                StringComparer.Ordinal)
            {
                [nonCanonicalCase.Key] = nonCanonicalCase.Value
            };
            var nonCanonical = DesignConversionFlow38PublicBoundarySnapshot.FromRegistration(
                registration with { BoundaryMetadata = nonCanonicalMetadata });
            var nonCanonicalPublicFields = nonCanonical.ToPublicFields();

            Assert.False(nonCanonical.MetadataValid);
            Assert.False(nonCanonical.Configured);
            Assert.Contains("malformed:" + nonCanonicalCase.Key, nonCanonical.MetadataIssues);
            Assert.False(nonCanonical.SemanticQualityClaimed);
            Assert.False(nonCanonical.DpoTrainReady);
            Assert.False(nonCanonical.DpoPairCreated);
            Assert.False(nonCanonical.TrainingTriggered);
            Assert.False(nonCanonical.CommonTrainingAllowed);
            Assert.False(nonCanonical.RawOutputAccepted);
            Assert.False(nonCanonical.ProductReady);
            Assert.False((bool)nonCanonicalPublicFields["flow38QuarantineConfigured"]!);
            Assert.False((bool)nonCanonicalPublicFields["flow38QuarantineStoreConfigured"]!);
            Assert.False((bool)nonCanonicalPublicFields["flow38DpoTrainReady"]!);
            Assert.False((bool)nonCanonicalPublicFields["flow38DpoPairCreated"]!);
            Assert.False((bool)nonCanonicalPublicFields["flow38TrainingTriggered"]!);
            Assert.False((bool)nonCanonicalPublicFields["flow38CommonTrainingAllowed"]!);
            Assert.False((bool)nonCanonicalPublicFields["flow38RawOutputAccepted"]!);
            Assert.False((bool)nonCanonicalPublicFields["flow38ProductReady"]!);
        }

        var unexpectedMetadata = new Dictionary<string, string>(registration.BoundaryMetadata, StringComparer.Ordinal)
        {
            ["legal_verdict"] = "ALLOW",
            ["dpo_train_ready"] = bool.TrueString,
            ["dpo_pair_created"] = bool.TrueString,
            ["training_triggered"] = bool.TrueString,
            ["common_training_allowed"] = bool.TrueString,
            ["raw_output_accepted"] = bool.TrueString,
            ["product_ready"] = bool.TrueString
        };
        var unexpected = DesignConversionFlow38PublicBoundarySnapshot.FromRegistration(
            registration with { BoundaryMetadata = unexpectedMetadata });
        var publicFields = unexpected.ToPublicFields();
        Assert.False(unexpected.MetadataValid);
        Assert.False(unexpected.Configured);
        Assert.Equal("ALLOW", unexpected.LegalVerdict);
        Assert.False(unexpected.DpoTrainReady);
        Assert.False(unexpected.DpoPairCreated);
        Assert.False(unexpected.TrainingTriggered);
        Assert.False(unexpected.CommonTrainingAllowed);
        Assert.False(unexpected.RawOutputAccepted);
        Assert.False(unexpected.ProductReady);
        Assert.False((bool)publicFields["flow38QuarantineConfigured"]!);
        Assert.False((bool)publicFields["flow38QuarantineStoreConfigured"]!);
        Assert.True((bool)publicFields["flow38QuarantineStoreConfiguredObserved"]!);
        Assert.False((bool)publicFields["flow38DpoTrainReady"]!);
        Assert.False((bool)publicFields["flow38DpoPairCreated"]!);
        Assert.False((bool)publicFields["flow38TrainingTriggered"]!);
        Assert.False((bool)publicFields["flow38CommonTrainingAllowed"]!);
        Assert.False((bool)publicFields["flow38RawOutputAccepted"]!);
        Assert.False((bool)publicFields["flow38ProductReady"]!);
        Assert.Contains("unexpected:legal_verdict", unexpected.MetadataIssues);
        Assert.Contains("unexpected:dpo_train_ready", unexpected.MetadataIssues);
    }

    [Fact]
    public void StartupMetadataFactory_HasNoPublicDefaultOrSingleRoutePath()
    {
        var publicMetadataFactories = typeof(DesignConversionModelServerAspNetHostStartup)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(method => string.Equals(
                method.Name,
                "CreateStartupMetadata",
                StringComparison.Ordinal))
            .ToArray();

        Assert.Empty(publicMetadataFactories);
    }

    [Fact]
    public async Task PostImprovementLoop_AspNetHostStartup_ReceivesFigmaAndCanvaCallerShapedPayloads()
    {
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var visualJudge = new RecordingTestOnlyVisualJudge(score: 0.96);
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider,
            visualJudge);
        var registry = DesignConversionModelServerAspNetHostStartup.CreateImprovementLoopRegistry(
            composition,
            new UnexpectedCandidateStrategy());

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(registry);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };

            var fixtures = new[]
            {
                CreateFigmaCallerShapedRequest(),
                CreateCanvaCallerShapedRequest()
            };

            foreach (var fixture in fixtures)
            {
                var visualJudgeRequestCountBefore = visualJudge.Requests.Count;
                using var content = new StringContent(
                    fixture.BodyJson,
                    Encoding.UTF8,
                    "application/json");

                using var response = await client.PostAsync(
                    DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
                    content);
                var bodyJson = await response.Content.ReadAsStringAsync();

                AssertCallerShapedResponse(response, bodyJson, fixture, baseAddress);
                Assert.Equal(2, visualJudge.Requests.Count - visualJudgeRequestCountBefore);
            }

            Assert.Equal(4, browserAdapter.Requests.Count);
            Assert.Equal(4, comparisonProvider.Requests.Count);
            Assert.Equal(4, visualJudge.Requests.Count);
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Fact]
    public async Task PostImprovementLoop_AspNetHostStartup_ReturnsRefsOnlyEnvelope()
    {
        const string privateMarker = "PRIVATE_PASS139_ASPNET_HOST_STARTUP_SECRET";
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var visualJudge = new RecordingTestOnlyVisualJudge(score: 0.96);
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider,
            visualJudge);
        var registry = DesignConversionModelServerAspNetHostStartup.CreateImprovementLoopRegistry(
            composition,
            new UnexpectedCandidateStrategy());

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        var startupBuilder = app.ConfigureDesignConversionModelServer(registry);
        Assert.NotNull(startupBuilder);

        await app.StartAsync();
        try
        {
            var startupMetadata = app.Services
                .GetRequiredService<EndpointDataSource>()
                .Endpoints
                .Select(endpoint => endpoint.Metadata.GetMetadata<DesignConversionModelServerAspNetHostStartupMetadata>())
                .Where(metadata => metadata is not null)
                .Single(metadata => string.Equals(
                    metadata!.MappedRoute,
                    DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
                    StringComparison.Ordinal))!;
            Assert.Equal(
                DesignConversionModelServerAspNetHostStartup.HostStartupSurface,
                startupMetadata.StartupSurface);
            Assert.Equal(
                DesignConversionModelServerAspNetHostStartup.HostStartupBoundary,
                startupMetadata.Boundary);
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
                startupMetadata.MappedRoute);
            Assert.Equal(
                DesignConversionModelServerAspNetHostStartup.FullLocalRouteSet(),
                startupMetadata.MappedRoutes);
            Assert.Equal(
                DesignConversionModelServerAspNetHostStartup.FullLocalRouteSet().Count,
                startupMetadata.MappedRoutes.Count);
            Assert.Contains(
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                startupMetadata.MappedRoutes);
            Assert.True(startupMetadata.LocalOnly);
            Assert.True(startupMetadata.RefsOnly);
            Assert.False(startupMetadata.PublicDeploymentVerified);
            Assert.False(startupMetadata.CrossRepoCallerVerified);
            Assert.False(startupMetadata.ExternalProviderConfigured);
            Assert.False(startupMetadata.ProductReady);

            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };

            var request = new DesignConversionModelServerImprovementLoopRequest(
                LoopId: "pass139-aspnet-host-startup-loop",
                InitialPackage: CreatePackage("pass139-aspnet-host-startup-request", privateMarker),
                MaxAttempts: 2,
                Metadata: new Dictionary<string, string>
                {
                    ["pass"] = "codex-pass139",
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
            Assert.Equal(IPAddress.Loopback.ToString(), baseAddress.Host);
            Assert.True(baseAddress.Port > 0);
            Assert.Equal(
                bool.TrueString,
                response.Headers.GetValues("x-design-conversion-asp-net-host-verified").Single());
            Assert.Equal(
                nameof(DesignConversionModelServerAspNetEndpointMapper),
                response.Headers.GetValues("x-design-conversion-asp-net-endpoint-mapper").Single());
            Assert.Equal(
                bool.FalseString,
                response.Headers.GetValues("x-design-conversion-public-deployment-verified").Single());
            Assert.Equal(
                bool.FalseString,
                response.Headers.GetValues("x-design-conversion-public-production-default-ready").Single());
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
            Assert.Equal(DesignConversionThinClientStatus.Completed, root.GetProperty("contractStatus").GetString());
            Assert.Equal("pass139-aspnet-host-startup-request", root.GetProperty("requestId").GetString());
            Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, root.GetProperty("stopReason").GetString());
            Assert.True(root.GetProperty("completed").GetBoolean());
            Assert.Equal(1, root.GetProperty("attemptCount").GetInt32());
            Assert.Equal(2, root.GetProperty("attempts")[0].GetProperty("renderResultRefs").GetArrayLength());
            Assert.Equal(2, root.GetProperty("attempts")[0].GetProperty("comparisonArtifactRefs").GetArrayLength());
            AssertTestOnlyVisualJudgeBoundary(root);

            var metadata = root.GetProperty("boundaryMetadata");
            Assert.Equal(
                nameof(DesignConversionModelServerAspNetEndpointMapper),
                metadata.GetProperty("asp_net_endpoint_mapper").GetString());
            Assert.Equal(
                DesignConversionModelServerAspNetEndpointMapper.AspNetEndpointMapperBoundary,
                metadata.GetProperty("asp_net_endpoint_mapper_boundary").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_host_verified").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_minimal_api_pipeline_verified").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_route_matched").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_registry_dispatch_verified").GetString());
            Assert.Equal(
                nameof(DesignConversionModelServerImprovementLoopHandler),
                metadata.GetProperty("asp_net_handler_dispatch_target").GetString());
            Assert.Equal(
                DesignConversionModelServerAspNetEndpointMapper.AspNetHostKind,
                metadata.GetProperty("asp_net_host_kind").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("localhost_loopback_only").GetString());
            Assert.Equal("127.0.0.1", metadata.GetProperty("loopback_host").GetString());
            Assert.Equal("asp_net_runtime_bound", metadata.GetProperty("bound_port_mode").GetString());
            Assert.True(int.Parse(metadata.GetProperty("asp_net_local_port").GetString()!) > 0);
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_kestrel_socket_opened").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("local_loopback_http_call_executed").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("public_http_endpoint_verified").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("cross_repo_caller_verified").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("deployment_verified").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("public_production_default_ready").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("external_provider_called").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("network_call_executed").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("external_network_call_executed").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("external_ai_visual_judge_executed").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("external_visual_ai_executed").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("real_external_provider_executed").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty(DesignConversionVisualAiProviderSelectionPolicy.RealCloudProviderClaimedKey).GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("refs_only_public_response").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());

            Assert.Equal(2, browserAdapter.Requests.Count);
            Assert.Equal(2, comparisonProvider.Requests.Count);
            Assert.Equal(2, visualJudge.Requests.Count);
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Fact]
    public async Task PostEvaluate_AspNetHostStartup_ReturnsRefsOnlyCompletedEnvelope()
    {
        const string privateMarker = "PRIVATE_PASS_EVALUATE_ASPNET_HOST_SECRET";
        const string routeAdmissionToken = "refs-only-aspnet-host-route-token-0123456789";
        const string routeAdmissionTenantId = "tenant-refs-only-aspnet-host-alpha";
        const string modelLifecycleAuthToken = "refs-only-aspnet-host-model-token-0123456789";
        const string trustedVisualCostUserId = "refs-only-aspnet-host-user";
        const string trustedVisualCostTier = "paid";
        const string trustedVisualCostMonthlyCap = "125.75";
        var artifactDbRoot = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "design-conversion-evaluate-aspnet-host");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
        var activationStateRoot = Path.Combine(artifactDbRoot, "activation-state");
        var serverOwnedArtifactRoot = Path.Combine(artifactDbRoot, "server-owned-artifacts");
        var visualCostBudgetStoreRoot = Path.Combine(artifactDbRoot, "visual-cost-budget-store");
        var ledgerRoot = Path.Combine(artifactDbRoot, "visual-cost-ledger");
        using var ledgerRootOverride = UseVisualCostLedgerRoot(ledgerRoot);
        var visualCostBudgetStore = new JsonlDesignConversionVisualCostBudgetStore(visualCostBudgetStoreRoot);
        var activationRecord = await BuildAspNetHostActivationRecordAsync(
            artifactDbRoot,
            routeAdmissionTenantId,
            "refs-only-aspnet-host-active-model");
        var activationRegistry = new DesignConversionModelActivationRegistry(
            activationStateRoot,
            () => DateTimeOffset.UnixEpoch.AddMinutes(400),
            allowedArtifactRootDirectory: artifactDbRoot);
        var activated = await activationRegistry.ActivateAsync(
            new DesignConversionModelActivationCommand(
                routeAdmissionTenantId,
                RequestedByRef: "aspnet-refs-only-host-test",
                Reason: "refs-only ASP.NET evaluate fixture activation",
                IdempotencyKey: "activate:refs-only-aspnet-host",
                ExpectedGeneration: 0,
                Candidate: activationRecord,
                ExpectedModelVersion: activationRecord.Model.Version,
                RequestedAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(400)));
        Assert.True(activated.IsSuccess, activated.ErrorMessage);
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var visualJudge = new AspNetVisualCostReceiptJudge(0.0600m);
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider,
            visualJudge);
        var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            composition,
            new UnexpectedCandidateStrategy(),
            artifactDb,
            () => DateTimeOffset.UnixEpoch.AddMinutes(401),
            activationStateRootDirectory: activationStateRoot,
            modelLifecycleAuthToken: modelLifecycleAuthToken,
            artifactRefResolver: new DesignConversionServerOwnedLocalArtifactRefResolver(serverOwnedArtifactRoot),
            routeAdmissionOptions: new DesignConversionModelServerRouteAdmissionOptions(
                enabled: true,
                currentToken: routeAdmissionToken,
                tenantId: routeAdmissionTenantId),
            visualCostBudgetStore: visualCostBudgetStore);

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        var startupBuilder = app.ConfigureDesignConversionModelServer(registry);
        Assert.NotNull(startupBuilder);

        await app.StartAsync();
        try
        {
            var startupMetadata = app.Services
                .GetRequiredService<EndpointDataSource>()
                .Endpoints
                .Select(endpoint => endpoint.Metadata.GetMetadata<DesignConversionModelServerAspNetHostStartupMetadata>())
                .Where(metadata => metadata is not null)
                .Single(metadata => string.Equals(
                    metadata!.MappedRoute,
                    DesignConversionModelServerEndpointRegistry.EndpointPath,
                    StringComparison.Ordinal))!;
            Assert.Equal(
                DesignConversionModelServerAspNetHostStartup.HostStartupSurface,
                startupMetadata.StartupSurface);
            Assert.Equal(
                DesignConversionModelServerAspNetHostStartup.HostStartupBoundary,
                startupMetadata.Boundary);
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                startupMetadata.MappedRoute);
            Assert.Equal(
                DesignConversionModelServerAspNetHostStartup.FullLocalRouteSet(),
                startupMetadata.MappedRoutes);
            Assert.True(startupMetadata.LocalOnly);
            Assert.True(startupMetadata.RefsOnly);
            Assert.False(startupMetadata.PublicDeploymentVerified);
            Assert.False(startupMetadata.CrossRepoCallerVerified);
            Assert.False(startupMetadata.ExternalProviderConfigured);
            Assert.False(startupMetadata.ProductReady);

            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };

            var baseRequest = CreateEvaluateRequest("pass-evaluate-aspnet-host-request", privateMarker);
            var admittedRequest = WithTrustedVisualCostEntitlement(
                WithServerOwnedAspNetLocalArtifactProofs(baseRequest, serverOwnedArtifactRoot),
                trustedVisualCostUserId,
                trustedVisualCostTier,
                trustedVisualCostMonthlyCap,
                routeAdmissionTenantId);
            var request = admittedRequest with
            {
                Metadata = new Dictionary<string, string>(admittedRequest.Metadata ?? new Dictionary<string, string>())
                {
                    ["asp_net_route_verified"] = bool.TrueString,
                    ["asp_net_route_verification_authority"] = "client_supplied_spoof",
                    ["asp_net_route_verification_state"] = "client_supplied_spoof",
                    ["persistence_verified"] = bool.TrueString,
                    ["local_artifact_db_persistence_verified"] = bool.TrueString,
                    ["artifact_db_record_source"] = "client_supplied_spoof"
                }
            };
            using var evaluateMessage = CreateAspNetEvaluateAdmissionRequest(
                JsonSerializer.Serialize(request, DesignConversionModelServerHttpHandler.JsonOptions),
                routeAdmissionToken,
                "refs-only-aspnet-host-evaluate-idempotency");

            using var response = await client.SendAsync(evaluateMessage);
            var bodyJson = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(IPAddress.Loopback.ToString(), baseAddress.Host);
            Assert.True(baseAddress.Port > 0);
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                response.Headers.GetValues("x-design-conversion-endpoint").Single());
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                response.Headers.GetValues("x-design-conversion-asp-net-route").Single());
            Assert.Equal(
                bool.TrueString,
                response.Headers.GetValues("x-design-conversion-asp-net-host-verified").Single());
            Assert.Equal(
                nameof(DesignConversionModelServerAspNetEndpointMapper),
                response.Headers.GetValues("x-design-conversion-asp-net-endpoint-mapper").Single());
            Assert.Equal(
                DesignConversionModelServerAspNetEndpointMapper.AspNetEndpointMapperBoundary,
                response.Headers.GetValues("x-design-conversion-asp-net-boundary").Single());
            Assert.Equal(
                DesignConversionModelServerAspNetEndpointMapper.AspNetHostKind,
                response.Headers.GetValues("x-design-conversion-asp-net-host-kind").Single());
            Assert.Equal(
                bool.TrueString,
                response.Headers.GetValues("x-design-conversion-localhost-loopback-only").Single());
            Assert.Equal(
                bool.TrueString,
                response.Headers.GetValues("x-design-conversion-socket-opened").Single());
            Assert.Equal(
                bool.FalseString,
                response.Headers.GetValues("x-design-conversion-public-deployment-verified").Single());
            Assert.Equal(
                bool.FalseString,
                response.Headers.GetValues("x-design-conversion-public-production-default-ready").Single());
            Assert.Equal(
                bool.FalseString,
                response.Headers.GetValues("x-design-conversion-product-ready").Single());

            Assert.DoesNotContain(privateMarker, bodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(request.Html, bodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(request.Css, bodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(request.DesignTree.Payload!, bodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(request.OriginalTree.Payload!, bodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(artifactDbRoot, bodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(activationStateRoot, bodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(serverOwnedArtifactRoot, bodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(visualCostBudgetStoreRoot, bodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(ledgerRoot, bodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(routeAdmissionToken, bodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(routeAdmissionTenantId, bodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(modelLifecycleAuthToken, bodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(trustedVisualCostUserId, bodyJson, StringComparison.Ordinal);

            using var document = JsonDocument.Parse(bodyJson);
            var root = document.RootElement;
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                root.GetProperty("endpoint").GetString());
            Assert.Equal("POST", root.GetProperty("method").GetString());
            Assert.Equal(DesignConversionModelServerHttpHandler.LocalHttpSeam, root.GetProperty("httpSeam").GetString());
            Assert.Equal(DesignConversionModelServerApi.ApiShape, root.GetProperty("apiShape").GetString());
            Assert.Equal(DesignConversionModelServerApi.RuntimeSurface, root.GetProperty("runtimeSurface").GetString());
            Assert.Equal(200, root.GetProperty("statusCode").GetInt32());
            Assert.Equal(DesignConversionThinClientStatus.Completed, root.GetProperty("contractStatus").GetString());
            Assert.Equal(request.RequestId, root.GetProperty("requestId").GetString());
            Assert.True(root.GetProperty("modelTrainingVerified").GetBoolean());
            Assert.False(root.GetProperty("modelUploadReady").GetBoolean());
            Assert.False(root.GetProperty("deploymentVerified").GetBoolean());
            Assert.False(root.GetProperty("publicHttpEndpointVerified").GetBoolean());
            Assert.False(root.GetProperty("platformIntegrationVerified").GetBoolean());
            Assert.False(root.GetProperty("productReady").GetBoolean());
            Assert.True(root.GetProperty("evidenceRefs").GetArrayLength() > 0);
            AssertJsonStringArrayDoesNotContain(root.GetProperty("evidenceRefs"), $"{Flow47VerifiedKey}=false");

            var runtimeStatus = root.GetProperty("runtimeStatus");
            Assert.Equal(DesignConversionThinClientStatus.Completed, runtimeStatus.GetProperty("contractStatus").GetString());
            Assert.Equal(request.RequestId, runtimeStatus.GetProperty("job").GetProperty("requestId").GetString());
            Assert.True(runtimeStatus.GetProperty("artifacts").GetArrayLength() > 0);
            AssertThinRuntimeFlow47Provenance(runtimeStatus);

            var metadata = root.GetProperty("boundaryMetadata");
            Assert.Equal(
                nameof(DesignConversionModelServerAspNetEndpointMapper),
                metadata.GetProperty("asp_net_endpoint_mapper").GetString());
            Assert.Equal(
                DesignConversionModelServerAspNetEndpointMapper.AspNetEndpointMapperBoundary,
                metadata.GetProperty("asp_net_endpoint_mapper_boundary").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_host_verified").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_minimal_api_pipeline_verified").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_route_matched").GetString());
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                metadata.GetProperty("asp_net_route_path").GetString());
            AssertTrustedAspNetRouteMetadata(
                metadata,
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                nameof(DesignConversionModelServerHttpHandler));
            Assert.NotEqual("client_supplied_spoof", metadata.GetProperty("asp_net_route_verification_authority").GetString());
            Assert.NotEqual("client_supplied_spoof", metadata.GetProperty("asp_net_route_verification_state").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_registry_dispatch_verified").GetString());
            Assert.Equal(
                nameof(DesignConversionModelServerHttpHandler),
                metadata.GetProperty("asp_net_handler_dispatch_target").GetString());
            Assert.Equal(
                DesignConversionModelServerAspNetEndpointMapper.AspNetHostKind,
                metadata.GetProperty("asp_net_host_kind").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("localhost_loopback_only").GetString());
            Assert.Equal("127.0.0.1", metadata.GetProperty("loopback_host").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_kestrel_socket_opened").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("local_loopback_http_call_executed").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("runtime_invoked").GetString());
            AssertAspNetAdmittedStableResponseMetadata(metadata);
            Assert.True(metadata.TryGetProperty("render_qa_result_count", out var renderQaResultCountMetadata));
            Assert.True(metadata.TryGetProperty("comparison_artifact_count", out var comparisonArtifactCountMetadata));
            var renderQaResultCount = int.Parse(renderQaResultCountMetadata.GetString()!);
            var comparisonArtifactCount = int.Parse(comparisonArtifactCountMetadata.GetString()!);
            Assert.True(renderQaResultCount >= 0);
            Assert.True(comparisonArtifactCount >= 0);
            Assert.Equal(bool.TrueString, metadata.GetProperty("refs_only_public_response").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("response_contains_raw_private_data").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("public_http_endpoint_verified").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("cross_repo_caller_verified").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("deployment_verified").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("model_upload_ready").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("ModelUploadReady").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("public_production_default_ready").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("external_provider_called").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("network_call_executed").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("external_network_call_executed").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
            var jobId = root.GetProperty("jobId").GetString();
            Assert.False(string.IsNullOrWhiteSpace(jobId));
            AssertPersistenceVerifiedMetadata(metadata, jobId!, request.RequestId);
            var evaluatePersistenceRecordId = metadata.GetProperty("persistence_record_id").GetString();
            using (var statusRequest = CreateAspNetStatusReadbackAdmissionRequest(
                       DesignConversionModelServerStatusHandler.EndpointPathPrefix + jobId,
                       routeAdmissionToken))
            using (var statusResponse = await client.SendAsync(statusRequest))
            {
                var statusBodyJson = await statusResponse.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.OK, statusResponse.StatusCode);
                Assert.DoesNotContain(privateMarker, statusBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(request.Html, statusBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(request.Css, statusBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(artifactDbRoot, statusBodyJson, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(activationStateRoot, statusBodyJson, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(serverOwnedArtifactRoot, statusBodyJson, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(visualCostBudgetStoreRoot, statusBodyJson, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(ledgerRoot, statusBodyJson, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(routeAdmissionToken, statusBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(routeAdmissionTenantId, statusBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(modelLifecycleAuthToken, statusBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(trustedVisualCostUserId, statusBodyJson, StringComparison.Ordinal);

                using var statusDocument = JsonDocument.Parse(statusBodyJson);
                var statusRoot = statusDocument.RootElement;
                Assert.Equal(DesignConversionThinClientStatus.Completed, statusRoot.GetProperty("contractStatus").GetString());
                Assert.False(statusRoot.GetProperty("productReady").GetBoolean());
                AssertThinRuntimeFlow47Provenance(statusRoot);

                var statusMetadata = statusRoot.GetProperty("boundaryMetadata");
                AssertAspNetAdmittedStableResponseMetadata(statusMetadata);
                AssertTrustedAspNetRouteMetadata(
                    statusMetadata,
                    DesignConversionModelServerEndpointRegistry.StatusReadbackEndpointPath,
                    nameof(DesignConversionModelServerStatusHandler));
                Assert.Equal(
                    nameof(DesignConversionModelServerStatusHandler),
                    statusMetadata.GetProperty("asp_net_handler_dispatch_target").GetString());
                AssertPersistenceVerifiedMetadata(statusMetadata, jobId!, request.RequestId);
                Assert.Equal(
                    evaluatePersistenceRecordId,
                    statusMetadata.GetProperty("persistence_record_id").GetString());
                Assert.Equal(bool.TrueString, statusMetadata.GetProperty("model_training_verified").GetString());
                Assert.Equal(bool.FalseString, statusMetadata.GetProperty("model_upload_ready").GetString());
                Assert.Equal(bool.FalseString, statusMetadata.GetProperty("deployment_verified").GetString());
                Assert.Equal(bool.FalseString, statusMetadata.GetProperty("public_http_endpoint_verified").GetString());
                Assert.Equal(bool.FalseString, statusMetadata.GetProperty("product_ready").GetString());
            }

            var records = await artifactDb.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(
                JobId: jobId,
                RequestId: request.RequestId));
            Assert.True(records.IsSuccess, records.ErrorMessage);
            var record = Assert.Single(records.Value!);
            Assert.Equal(evaluatePersistenceRecordId, record.RecordId);
            Assert.Equal(jobId, record.JobId);
            Assert.Equal(request.RequestId, record.RequestId);
            Assert.Equal(nameof(DesignConversionThinClientJobRuntime), record.Metadata["artifact_db_record_source"]);
            Assert.Equal(bool.TrueString, record.Metadata["local_artifact_db"]);
            Assert.Equal(bool.TrueString, record.Metadata["storage_training_artifact_db"]);
            Assert.Equal(bool.TrueString, record.Metadata["training_database_refs_only_trace"]);
            Assert.Equal(bool.TrueString, record.Metadata["raw_payload_redacted"]);
            Assert.Equal(metadata.GetProperty("persistence_expected_render_ref_count").GetString(), record.RenderArtifactRefs.Count.ToString());

            Assert.NotEmpty(browserAdapter.Requests);
            Assert.NotEmpty(comparisonProvider.Requests);
        }
        finally
        {
            await app.StopAsync();
            DeleteDirectoryIfExists(artifactDbRoot);
        }
    }

    [Fact]
    public async Task PostEvaluate_AspNetEndpointMapper_InternalAliasUsesSameHandlerAndPreservesAdmissionAndLegacyRoute()
    {
        const string privateMarker = "PRIVATE_INTERNAL_EVALUATE_ALIAS_ASPNET_SECRET";
        const string routeAdmissionToken = "internal-evaluate-alias-route-token-0123456789";
        const string routeAdmissionTenantId = "tenant-internal-evaluate-alias-alpha";
        const string trustedVisualCostUserId = "platform-user-internal-evaluate-alias-alpha";
        const string trustedVisualCostTier = "paid";
        const string trustedVisualCostMonthlyCap = "125.75";
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "design-conversion-internal-evaluate-alias-budget");
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var visualJudge = new AspNetVisualCostReceiptJudge(0.0600m);
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider,
            visualJudge);
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            Path.Combine(tempRoot, "artifact-db"));
        var visualCostBudgetStore = new JsonlDesignConversionVisualCostBudgetStore(
            Path.Combine(tempRoot, "visual-cost-budget-store"));
        var api = new DesignConversionModelServerApi(
            new DesignConversionInternalServiceThinClientJobEvaluator(composition.Service),
            () => DateTimeOffset.UnixEpoch.AddMinutes(742),
            artifactDb,
            visualCostBudgetStore: visualCostBudgetStore);
        var admission = DesignConversionModelServerRouteAdmission.Create(
            new DesignConversionModelServerRouteAdmissionOptions(
                enabled: true,
                currentToken: routeAdmissionToken,
                tenantId: routeAdmissionTenantId),
            () => DateTimeOffset.UnixEpoch.AddMinutes(742));
        var handler = new DesignConversionModelServerHttpHandler(api, admission);
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionEvaluate(handler);
        Assert.True(registered.IsSuccess, registered.ErrorMessage);

        var internalResolved = registry.Resolve(
            DesignConversionModelServerEndpointRegistry.EndpointMethod,
            DesignConversionModelServerEndpointRegistry.InternalEndpointPath);
        Assert.True(internalResolved.IsSuccess, internalResolved.ErrorMessage);
        Assert.Same(handler, internalResolved.Value!.Handler.Target);
        Assert.Same(handler.RouteAdmission, internalResolved.Value.RouteAdmission);

        DesignConversionModelServerApiRequest CreateTenantBoundEvaluateRequest(string requestId)
        {
            var request = CreateEvaluateRequest(requestId, privateMarker);
            return request with
            {
                Budget = request.Budget! with
                {
                    UserId = trustedVisualCostUserId,
                    Tier = trustedVisualCostTier,
                    MaxImprovementCandidates = 1,
                    MaxRenderQaRuns = 2,
                    BudgetUsd = 0.25m,
                    Metadata = new Dictionary<string, string>()
                },
                Metadata = new Dictionary<string, string>(
                    request.Metadata ?? new Dictionary<string, string>(),
                    StringComparer.Ordinal)
                {
                    ["tenant_id"] = routeAdmissionTenantId,
                    [DesignConversionModelServerApi.ServerAssignedDesignTierMetadataKey] =
                        trustedVisualCostTier,
                    [DesignConversionModelServerApi.ServerAssignedMonthlyVisualCostLimitUsdMetadataKey] =
                        trustedVisualCostMonthlyCap,
                    [DesignConversionModelServerApi.DesignTierAuthorityMetadataKey] =
                        DesignConversionModelServerApi.PlatformTenantConfigurationAuthority,
                    [DesignConversionModelServerApi.MonthlyVisualCostLimitAuthorityMetadataKey] =
                        DesignConversionModelServerApi.PlatformTenantConfigurationAuthority,
                    [DesignConversionModelServerApi.UserIdentityAuthorityMetadataKey] =
                        DesignConversionModelServerApi.VerifiedInboundPrincipalAuthority
                }
            };
        }

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.MapDesignConversionModelServerEvaluate(registry);
        app.MapDesignConversionModelServerInternalEvaluate(registry);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };

            var missingTokenRequest = CreateTenantBoundEvaluateRequest("internal-alias-missing-token");
            using (var missingTokenContent = new StringContent(
                       JsonSerializer.Serialize(missingTokenRequest, DesignConversionModelServerHttpHandler.JsonOptions),
                       Encoding.UTF8,
                       "application/json"))
            using (var missingTokenResponse = await client.PostAsync(
                       DesignConversionModelServerEndpointRegistry.InternalEndpointPath,
                       missingTokenContent))
            {
                var missingTokenBodyJson = await missingTokenResponse.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.Unauthorized, missingTokenResponse.StatusCode);
                Assert.Equal(
                    DesignConversionModelServerEndpointRegistry.InternalEndpointPath,
                    missingTokenResponse.Headers.GetValues("x-design-conversion-endpoint").Single());
                Assert.Equal(
                    DesignConversionModelServerEndpointRegistry.InternalEndpointPath,
                    missingTokenResponse.Headers.GetValues("x-design-conversion-asp-net-route").Single());
                Assert.Contains(
                    "DESIGN_CONVERSION_ROUTE_ADMISSION_TOKEN_REQUIRED",
                    missingTokenBodyJson,
                    StringComparison.Ordinal);
                Assert.DoesNotContain(routeAdmissionToken, missingTokenBodyJson, StringComparison.Ordinal);
                Assert.Empty(browserAdapter.Requests);
                Assert.Empty(comparisonProvider.Requests);

                using var missingTokenDocument = JsonDocument.Parse(missingTokenBodyJson);
                var missingTokenRoot = missingTokenDocument.RootElement;
                Assert.Equal(
                    DesignConversionModelServerEndpointRegistry.InternalEndpointPath,
                    missingTokenRoot.GetProperty("endpoint").GetString());
                Assert.Equal("POST", missingTokenRoot.GetProperty("method").GetString());
                Assert.Equal(
                    "DESIGN_CONVERSION_ROUTE_ADMISSION_TOKEN_REQUIRED",
                    missingTokenRoot.GetProperty("errorCode").GetString());
                var missingTokenMetadata = missingTokenRoot.GetProperty("boundaryMetadata");
                Assert.Equal(
                    DesignConversionModelServerEndpointRegistry.InternalEndpointPath,
                    missingTokenMetadata.GetProperty("http_path").GetString());
                Assert.Equal(
                    DesignConversionModelServerEndpointRegistry.InternalEndpointPath,
                    missingTokenMetadata.GetProperty("route_admission_path").GetString());
                Assert.Equal(bool.FalseString, missingTokenMetadata.GetProperty("route_admission_authorized").GetString());
                Assert.Equal(bool.FalseString, missingTokenMetadata.GetProperty("runtime_invoked").GetString());
                Assert.Equal(bool.FalseString, missingTokenMetadata.GetProperty("public_http_endpoint_verified").GetString());
            }

            var internalRequest = CreateTenantBoundEvaluateRequest("internal-alias-valid-request");
            using (var internalMessage = CreateAspNetEvaluateAdmissionRequest(
                       JsonSerializer.Serialize(internalRequest, DesignConversionModelServerHttpHandler.JsonOptions),
                       routeAdmissionToken,
                       "internal-alias-valid-idempotency",
                       DesignConversionModelServerEndpointRegistry.InternalEndpointPath))
            using (var internalResponse = await client.SendAsync(internalMessage))
            {
                var internalBodyJson = await internalResponse.Content.ReadAsStringAsync();
                Assert.True(
                    internalResponse.StatusCode == HttpStatusCode.OK,
                    $"Expected internal alias status {HttpStatusCode.OK}, actual {internalResponse.StatusCode}. Body: {internalBodyJson}");
                Assert.Equal(
                    DesignConversionModelServerEndpointRegistry.InternalEndpointPath,
                    internalResponse.Headers.GetValues("x-design-conversion-endpoint").Single());
                Assert.Equal(
                    DesignConversionModelServerEndpointRegistry.InternalEndpointPath,
                    internalResponse.Headers.GetValues("x-design-conversion-asp-net-route").Single());
                Assert.DoesNotContain(privateMarker, internalBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(trustedVisualCostUserId, internalBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(routeAdmissionToken, internalBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(routeAdmissionTenantId, internalBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(internalRequest.Html, internalBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(internalRequest.Css, internalBodyJson, StringComparison.Ordinal);

                using var internalDocument = JsonDocument.Parse(internalBodyJson);
                var internalRoot = internalDocument.RootElement;
                Assert.Equal(
                    DesignConversionModelServerEndpointRegistry.InternalEndpointPath,
                    internalRoot.GetProperty("endpoint").GetString());
                Assert.Equal("POST", internalRoot.GetProperty("method").GetString());
                Assert.False(internalRoot.GetProperty("publicHttpEndpointVerified").GetBoolean());
                Assert.False(internalRoot.GetProperty("deploymentVerified").GetBoolean());
                Assert.False(internalRoot.GetProperty("productReady").GetBoolean());

                var persistedRecords = await artifactDb.ListRecordsAsync(
                    new DesignConversionLocalArtifactDbQuery(RequestId: internalRequest.RequestId));
                Assert.True(persistedRecords.IsSuccess, persistedRecords.ErrorMessage);
                Assert.Single(persistedRecords.Value!);

                var internalMetadata = internalRoot.GetProperty("boundaryMetadata");
                Assert.Equal(
                    "POST " + DesignConversionModelServerEndpointRegistry.InternalEndpointPath,
                    internalMetadata.GetProperty("http_endpoint_shape").GetString());
                Assert.Equal(
                    DesignConversionModelServerEndpointRegistry.InternalEndpointPath,
                    internalMetadata.GetProperty("http_path").GetString());
                Assert.Equal(
                    DesignConversionModelServerEndpointRegistry.InternalEndpointPath,
                    internalMetadata.GetProperty("route_admission_path").GetString());
                Assert.Equal(bool.TrueString, internalMetadata.GetProperty("route_admission_authorized").GetString());
                Assert.Equal(bool.TrueString, internalMetadata.GetProperty("route_admission_token_accepted").GetString());
                Assert.Equal(bool.TrueString, internalMetadata.GetProperty("idempotency_key_accepted").GetString());
                Assert.Equal(bool.TrueString, internalMetadata.GetProperty("route_admission_tenant_bound").GetString());
                Assert.Equal(bool.TrueString, internalMetadata.GetProperty("trusted_visual_cost_entitlement_bound").GetString());
                Assert.Equal(
                    trustedVisualCostTier,
                    internalMetadata.GetProperty("trusted_visual_cost_entitlement_design_tier").GetString());
                Assert.Equal(
                    trustedVisualCostMonthlyCap,
                    internalMetadata.GetProperty("trusted_visual_cost_entitlement_monthly_limit_usd").GetString());
                Assert.Equal(bool.FalseString, internalMetadata.GetProperty("trusted_visual_cost_entitlement_raw_user_exposed").GetString());
                Assert.Equal(bool.TrueString, internalMetadata.GetProperty("trusted_visual_cost_reservation_store_wired").GetString());
                Assert.Equal(
                    DesignConversionModelActivationRegistry.BuildTenantIdRef(routeAdmissionTenantId),
                    internalMetadata.GetProperty("route_admission_tenant_id_ref").GetString());
                Assert.Equal(bool.FalseString, internalMetadata.GetProperty("route_admission_raw_tenant_id_exposed").GetString());
                Assert.Equal(bool.TrueString, internalMetadata.GetProperty("runtime_invoked").GetString());
                AssertTrustedAspNetRouteMetadata(
                    internalMetadata,
                    DesignConversionModelServerEndpointRegistry.InternalEndpointPath,
                    nameof(DesignConversionModelServerHttpHandler));
                Assert.Equal(bool.FalseString, internalMetadata.GetProperty("public_http_endpoint_verified").GetString());
                Assert.Equal(bool.FalseString, internalMetadata.GetProperty("deployment_verified").GetString());
            }

            var requestCountBeforeLegacy = browserAdapter.Requests.Count;
            var comparisonCountBeforeLegacy = comparisonProvider.Requests.Count;
            var legacyRequest = CreateTenantBoundEvaluateRequest("legacy-evaluate-still-valid-request");
            using (var legacyMessage = CreateAspNetEvaluateAdmissionRequest(
                       JsonSerializer.Serialize(legacyRequest, DesignConversionModelServerHttpHandler.JsonOptions),
                       routeAdmissionToken,
                       "legacy-evaluate-still-valid-idempotency"))
            using (var legacyResponse = await client.SendAsync(legacyMessage))
            {
                var legacyBodyJson = await legacyResponse.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.OK, legacyResponse.StatusCode);
                Assert.DoesNotContain(privateMarker, legacyBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(trustedVisualCostUserId, legacyBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(routeAdmissionToken, legacyBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(routeAdmissionTenantId, legacyBodyJson, StringComparison.Ordinal);
                Assert.True(browserAdapter.Requests.Count > requestCountBeforeLegacy);
                Assert.True(comparisonProvider.Requests.Count > comparisonCountBeforeLegacy);
                Assert.Equal(
                    DesignConversionModelServerEndpointRegistry.EndpointPath,
                    legacyResponse.Headers.GetValues("x-design-conversion-endpoint").Single());
                Assert.Equal(
                    DesignConversionModelServerEndpointRegistry.EndpointPath,
                    legacyResponse.Headers.GetValues("x-design-conversion-asp-net-route").Single());

                using var legacyDocument = JsonDocument.Parse(legacyBodyJson);
                var legacyRoot = legacyDocument.RootElement;
                Assert.Equal(
                    DesignConversionModelServerEndpointRegistry.EndpointPath,
                    legacyRoot.GetProperty("endpoint").GetString());
                var legacyMetadata = legacyRoot.GetProperty("boundaryMetadata");
                Assert.Equal(
                    DesignConversionModelServerEndpointRegistry.EndpointPath,
                    legacyMetadata.GetProperty("http_path").GetString());
                Assert.Equal(bool.TrueString, legacyMetadata.GetProperty("runtime_invoked").GetString());
                AssertTrustedAspNetRouteMetadata(
                    legacyMetadata,
                    DesignConversionModelServerEndpointRegistry.EndpointPath,
                    nameof(DesignConversionModelServerHttpHandler));
            }
        }
        finally
        {
            await app.StopAsync();
            DeleteDirectoryIfExists(tempRoot);
        }
    }

    [Fact]
    public async Task ConfigureDesignConversionModelServer_EvaluateRoutesExposeRouteSpecificStartupMetadataAndCompositeConventions()
    {
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "design-conversion-evaluate-route-startup-metadata"));
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            new RecordingBrowserAdapter(),
            new RecordingComparisonProvider(score: 0.96));
        var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            composition,
            new UnexpectedCandidateStrategy(),
            artifactDb,
            () => DateTimeOffset.UnixEpoch.AddMinutes(743));

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        var startupBuilder = app.ConfigureDesignConversionModelServer(registry);
        startupBuilder.Add(endpointBuilder =>
            endpointBuilder.Metadata.Add(new StartupConventionProbeMetadata("evaluate-route-startup-convention")));

        await app.StartAsync();
        try
        {
            var endpoints = app.Services
                .GetRequiredService<EndpointDataSource>()
                .Endpoints
                .OfType<RouteEndpoint>()
                .ToArray();

            var publicEndpoint = Assert.Single(endpoints, endpoint => string.Equals(
                endpoint.RoutePattern.RawText,
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                StringComparison.Ordinal));
            var internalEndpoint = Assert.Single(endpoints, endpoint => string.Equals(
                endpoint.RoutePattern.RawText,
                DesignConversionModelServerEndpointRegistry.InternalEndpointPath,
                StringComparison.Ordinal));

            AssertEvaluateStartupEndpoint(
                publicEndpoint,
                DesignConversionModelServerEndpointRegistry.EndpointPath);
            AssertEvaluateStartupEndpoint(
                internalEndpoint,
                DesignConversionModelServerEndpointRegistry.InternalEndpointPath);
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Fact]
    public async Task PostEvaluate_AspNetHostStartup_SpoofedHostHeaderKeepsDefaultSubmitServerOwnedAndFlow47Unverified()
    {
        const string privateMarker = "PRIVATE_FLOW47_HOST_SPOOF_ASPNET_SECRET";
        const string spoofedHost = "spoofed-public.example:443";
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "design-conversion-flow47-host-spoof"));
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider);
        var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            composition,
            new UnexpectedCandidateStrategy(),
            artifactDb,
            () => DateTimeOffset.UnixEpoch.AddMinutes(517));

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(registry);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };
            var request = CreateEvaluateRequest("flow47-host-spoof-aspnet-request", privateMarker);
            using var message = new HttpRequestMessage(
                HttpMethod.Post,
                DesignConversionModelServerEndpointRegistry.EndpointPath)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(request, DesignConversionModelServerHttpHandler.JsonOptions),
                    Encoding.UTF8,
                    "application/json")
            };
            message.Headers.Host = spoofedHost;

            using var response = await client.SendAsync(message);
            var bodyJson = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.DoesNotContain(privateMarker, bodyJson, StringComparison.Ordinal);
            using var document = JsonDocument.Parse(bodyJson);
            var root = document.RootElement;
            Assert.False(root.GetProperty("productReady").GetBoolean());

            var metadata = root.GetProperty("boundaryMetadata");
            AssertTrustedAspNetRouteMetadata(
                metadata,
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                nameof(DesignConversionModelServerHttpHandler));
            AssertTrustedAspNetDefaultSubmitMetadata(metadata, baseAddress);
            Assert.DoesNotContain(spoofedHost, metadata.GetProperty("default_submit_base_url").GetString(), StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(spoofedHost, metadata.GetProperty("default_submit_request_uri").GetString(), StringComparison.OrdinalIgnoreCase);
            Assert.Equal(string.Empty, metadata.GetProperty("flow47_base_url").GetString());
            Assert.Equal(string.Empty, metadata.GetProperty("flow47_endpoint_path").GetString());
            Assert.Equal(string.Empty, metadata.GetProperty("flow47_submit_request_uri").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("public_production_default_ready").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Fact]
    public async Task PostEvaluate_AspNetHostStartup_RuntimeExceptionMapsApiUnexpectedFailureTo500WithoutPrivateLeak()
    {
        const string privateExceptionMarker = "PRIVATE_N12_1_ASPNET_RUNTIME_THROW_SECRET";
        const string privateRequestMarker = "PRIVATE_N12_1_ASPNET_REQUEST_SECRET";
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider);
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "design-conversion-aspnet-runtime-exception-authority");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(tempRoot);
        var throwingApi = new DesignConversionModelServerApi(
            composition.Service,
            () => throw new InvalidOperationException(
                $"ASP.NET runtime clock failed with private marker {privateExceptionMarker}."));
        var throwingEvaluateHandler = new DesignConversionModelServerHttpHandler(throwingApi);
        var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            composition,
            new UnexpectedCandidateStrategy(),
            artifactDb,
            () => DateTimeOffset.UnixEpoch.AddMinutes(812));
        Assert.True(registry.HasServerOwnedLocalImprovementLoopRegistrationAuthority);
        Assert.Equal(
            DesignConversionModelServerAspNetHostStartup.FullLocalRouteSet().Count,
            registry.RegisteredRouteCount);
        ReplaceEvaluateRegistrationHandlerForTest(
            registry,
            throwingEvaluateHandler.HandleAsync);

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(registry);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };
            var request = CreateEvaluateRequest("n12-1-aspnet-handler-exception", privateRequestMarker);
            var requestJson = JsonSerializer.Serialize(request, DesignConversionModelServerHttpHandler.JsonOptions);
            using var content = new StringContent(
                requestJson,
                Encoding.UTF8,
                "application/json");

            using var response = await client.PostAsync(
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                content);
            var bodyJson = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                response.Headers.GetValues("x-design-conversion-asp-net-route").Single());
            Assert.Equal(
                bool.TrueString,
                response.Headers.GetValues("x-design-conversion-asp-net-host-verified").Single());
            Assert.Equal(
                nameof(DesignConversionModelServerAspNetEndpointMapper),
                response.Headers.GetValues("x-design-conversion-asp-net-endpoint-mapper").Single());
            Assert.Equal("500", response.Headers.GetValues("x-design-conversion-status-code").Single());
            Assert.DoesNotContain(privateExceptionMarker, bodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(privateRequestMarker, bodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(requestJson, bodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(request.Html!, bodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(request.Css!, bodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(request.DesignTree.Payload!, bodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(request.OriginalTree.Payload!, bodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(nameof(InvalidOperationException), bodyJson, StringComparison.Ordinal);

            using var document = JsonDocument.Parse(bodyJson);
            var root = document.RootElement;
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                root.GetProperty("endpoint").GetString());
            Assert.Equal("POST", root.GetProperty("method").GetString());
            Assert.Equal(DesignConversionModelServerHttpHandler.LocalHttpSeam, root.GetProperty("httpSeam").GetString());
            Assert.Equal(500, root.GetProperty("statusCode").GetInt32());
            Assert.Equal(
                DesignConversionModelServerApi.UnexpectedRequestFailureErrorCode,
                root.GetProperty("errorCode").GetString());
            Assert.Equal(
                DesignConversionModelServerApi.UnexpectedRequestFailureErrorMessage,
                root.GetProperty("errorMessage").GetString());

            var metadata = root.GetProperty("boundaryMetadata");
            Assert.Equal("500", metadata.GetProperty("http_status_code").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("refs_only_public_response").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("response_contains_raw_private_data").GetString());
            Assert.Equal(
                nameof(DesignConversionModelServerHttpHandler),
                metadata.GetProperty("asp_net_handler_dispatch_target").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_registry_dispatch_verified").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_host_verified").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_minimal_api_pipeline_verified").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_route_matched").GetString());
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                metadata.GetProperty("asp_net_route_path").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("asp_net_route_verified").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("asp_net_route_verification_successful_handler_dispatch").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("public_http_endpoint_verified").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("deployment_verified").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
            Assert.Empty(browserAdapter.Requests);
            Assert.Empty(comparisonProvider.Requests);
        }
        finally
        {
            await app.StopAsync();
            DeleteDirectoryIfExists(tempRoot);
        }
    }

    [Fact]
    public async Task PostEvaluate_AspNetEndpointMapper_PreBodyDispatchExceptionReturnsSafeJson500()
    {
        const string privateExceptionMarker = "PRIVATE_ASPNET_PREBODY_DISPATCH_THROW_SECRET";
        const string privateRequestMarker = "PRIVATE_ASPNET_PREBODY_REQUEST_SECRET";
        const string privatePathMarker = @"C:\private\design-conversion\prebody-dispatch-secret.json";
        const string routeAdmissionToken = "aspnet-prebody-dispatch-route-token-0123456789";
        var admission = DesignConversionModelServerRouteAdmission.Create(
            new DesignConversionModelServerRouteAdmissionOptions(
                enabled: true,
                currentToken: routeAdmissionToken),
            () => DateTimeOffset.UnixEpoch.AddMinutes(811));
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider);
        var api = new DesignConversionModelServerApi(
            composition.Service,
            () => DateTimeOffset.UnixEpoch.AddMinutes(811));
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionEvaluate(
            new DesignConversionModelServerHttpHandler(api, admission));
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        ReplaceEvaluateRegistrationHandlerForTest(
            registry,
            (_, _) => throw new InvalidOperationException(
                $"Pre-body ASP.NET dispatch failed {privateExceptionMarker} at {privatePathMarker}."));

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.MapDesignConversionModelServerEvaluate(registry);

        await app.StartAsync();
        try
        {
            using var client = new HttpClient
            {
                BaseAddress = ResolveStartedAddress(app),
                Timeout = TimeSpan.FromSeconds(5)
            };
            using var request = CreateAspNetEvaluateAdmissionRequest(
                $"{{\"private\":\"{privateRequestMarker}\"}}",
                token: null,
                idempotencyKey: null);

            using var response = await client.SendAsync(request);
            var bodyJson = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                response.Headers.GetValues("x-design-conversion-asp-net-route").Single());
            Assert.Equal(
                nameof(DesignConversionModelServerAspNetEndpointMapper),
                response.Headers.GetValues("x-design-conversion-asp-net-endpoint-mapper").Single());
            Assert.Equal("500", response.Headers.GetValues("x-design-conversion-status-code").Single());
            Assert.DoesNotContain(privateExceptionMarker, bodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(privateRequestMarker, bodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(privatePathMarker, bodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(routeAdmissionToken, bodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(nameof(InvalidOperationException), bodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(@"C:\", bodyJson, StringComparison.Ordinal);

            using var document = JsonDocument.Parse(bodyJson);
            var root = document.RootElement;
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                root.GetProperty("endpoint").GetString());
            Assert.Equal("POST", root.GetProperty("method").GetString());
            Assert.Equal(500, root.GetProperty("statusCode").GetInt32());
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.EndpointUnhandledErrorCode,
                root.GetProperty("errorCode").GetString());
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.EndpointUnhandledErrorMessage,
                root.GetProperty("errorMessage").GetString());

            var metadata = root.GetProperty("boundaryMetadata");
            Assert.Equal("500", metadata.GetProperty("http_status_code").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("refs_only_public_response").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("response_contains_raw_private_data").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("asp_net_registry_dispatch_verified").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_host_verified").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_minimal_api_pipeline_verified").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_route_matched").GetString());
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                metadata.GetProperty("asp_net_route_path").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("asp_net_route_verified").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("asp_net_route_verification_successful_handler_dispatch").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("public_http_endpoint_verified").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("deployment_verified").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
            Assert.Empty(browserAdapter.Requests);
            Assert.Empty(comparisonProvider.Requests);
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Theory]
    [InlineData("foreign-cancellation")]
    [InlineData("fatal-graph")]
    public async Task AspNetEndpointMapper_RequestGuardForeignCancellationOrFatalGraphEscapes(
        string exceptionKind)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = DesignConversionModelServerEndpointRegistry.EndpointPath;
        context.Response.Body = new MemoryStream();
        Exception expected = exceptionKind == "foreign-cancellation"
            ? new OperationCanceledException(
                "PRIVATE_ASPNET_GUARD_FOREIGN_CANCELLATION_SECRET",
                new CancellationToken(canceled: true))
            : new AggregateException(
                "PRIVATE_ASPNET_GUARD_FATAL_GRAPH_SECRET",
                new OutOfMemoryException("Constructed fatal node; no memory pressure used."));

        var thrown = await Record.ExceptionAsync(() => InvokeAspNetRequestExceptionGuardForTest(
            context,
            () => Task.FromException(expected)));

        Assert.Same(expected, thrown);
        if (exceptionKind == "fatal-graph")
        {
            var aggregate = Assert.IsType<AggregateException>(thrown);
            Assert.IsType<OutOfMemoryException>(Assert.Single(aggregate.InnerExceptions));
        }
        else
        {
            var cancellation = Assert.IsType<OperationCanceledException>(thrown);
            Assert.True(cancellation.CancellationToken.IsCancellationRequested);
            Assert.False(context.RequestAborted.IsCancellationRequested);
        }

        Assert.Equal(0, context.Response.Body.Length);
        Assert.False(context.Response.HasStarted);
    }

    [Fact]
    public async Task AspNetEndpointMapper_RequestGuardOrdinaryFailureReturnsSanitizedUnhandled()
    {
        const string privateMarker = "PRIVATE_ASPNET_GUARD_NONFATAL_SECRET";
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = DesignConversionModelServerEndpointRegistry.EndpointPath;
        context.Response.Body = new MemoryStream();

        await InvokeAspNetRequestExceptionGuardForTest(
            context,
            () => Task.FromException(new InvalidOperationException(privateMarker)));

        Assert.Equal(500, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8, leaveOpen: true);
        var bodyJson = await reader.ReadToEndAsync();
        Assert.DoesNotContain(privateMarker, bodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(InvalidOperationException), bodyJson, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(bodyJson);
        Assert.Equal(
            DesignConversionModelServerEndpointRegistry.EndpointUnhandledErrorCode,
            document.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal(
            DesignConversionModelServerEndpointRegistry.EndpointUnhandledErrorMessage,
            document.RootElement.GetProperty("errorMessage").GetString());
    }

    [Fact]
    public async Task AspNetEndpointMapper_RequestGuardSuppliedRequestCancellationEscapes()
    {
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = DesignConversionModelServerEndpointRegistry.EndpointPath;
        context.RequestAborted = cancellationSource.Token;
        context.Response.Body = new MemoryStream();
        var expected = new OperationCanceledException(
            "PRIVATE_ASPNET_GUARD_SUPPLIED_CANCELLATION_SECRET",
            cancellationSource.Token);

        var thrown = await Assert.ThrowsAsync<OperationCanceledException>(() =>
            InvokeAspNetRequestExceptionGuardForTest(
                context,
                () => Task.FromException(expected)));

        Assert.Same(expected, thrown);
        Assert.Equal(0, context.Response.Body.Length);
        Assert.False(context.Response.HasStarted);
    }

    [Fact]
    public void ApplyAspNetMetadata_NonLoopbackConnectionKeepsFlow47AndDefaultSubmitMissing()
    {
        var context = new DefaultHttpContext();
        context.Connection.LocalIpAddress = IPAddress.Parse("203.0.113.10");
        context.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.20");
        context.Connection.LocalPort = 51347;
        context.Request.Scheme = Uri.UriSchemeHttp;

        var metadata = InvokeAspNetMetadataForTest(
            context,
            loopbackOnly: false,
            socketOpened: true,
            localAddress: "203.0.113.10");

        AssertMissingAspNetDefaultSubmitMetadata(metadata);
        AssertUnverifiedAspNetRouteMetadata(
            metadata,
            DesignConversionModelServerEndpointRegistry.EndpointPath);
        Assert.Equal(bool.FalseString, MetadataValue(metadata, "public_production_default_ready"));
        Assert.Equal(bool.FalseString, MetadataValue(metadata, "product_ready"));
    }

    [Fact]
    public void ApplyAspNetMetadata_RequestPathMismatchKeepsRouteUnverified()
    {
        var context = new DefaultHttpContext();
        context.Connection.LocalIpAddress = IPAddress.Loopback;
        context.Connection.RemoteIpAddress = IPAddress.Loopback;
        context.Connection.LocalPort = 51348;
        context.Request.Scheme = Uri.UriSchemeHttp;

        var metadata = InvokeAspNetMetadataForTest(
            context,
            loopbackOnly: true,
            socketOpened: true,
            localAddress: IPAddress.Loopback.ToString(),
            requestPath: "/api/design-conversion/unknown");

        AssertUnverifiedAspNetRouteMetadata(
            metadata,
            DesignConversionModelServerEndpointRegistry.EndpointPath);
        Assert.Equal(bool.TrueString, MetadataValue(metadata, "asp_net_route_verification_successful_handler_dispatch"));
        Assert.Equal(bool.FalseString, MetadataValue(metadata, "product_ready"));
    }

    [Theory]
    [InlineData("designTree.payload")]
    [InlineData("originalTree.payload")]
    public async Task PostEvaluate_AspNetHostStartup_OversizedPayload_ReturnsPayloadTooLargeBadRequestWithoutRuntime(
        string oversizedFieldPath)
    {
        const string privateMarker = "PRIVATE_PAYLOAD_TOO_LARGE_EVALUATE_ASPNET_HOST";
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "design-conversion-evaluate-aspnet-host-payload-too-large"));
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var candidateStrategy = new UnexpectedCandidateStrategy();
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider);
        var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            composition,
            candidateStrategy,
            artifactDb,
            () => DateTimeOffset.UnixEpoch.AddMinutes(501));

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(registry);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };

            var baseRequest = CreateEvaluateRequest("payload-too-large-evaluate-aspnet-host-request", privateMarker);
            var oversizedPayload = new string('x', DesignConversionModelServerPayloadSizeGuard.MaxRawPayloadChars + 1);
            var request = oversizedFieldPath switch
            {
                "designTree.payload" => baseRequest with
                {
                    DesignTree = baseRequest.DesignTree with
                    {
                        Payload = oversizedPayload
                    }
                },
                "originalTree.payload" => baseRequest with
                {
                    OriginalTree = baseRequest.OriginalTree with
                    {
                        Payload = oversizedPayload
                    }
                },
                _ => throw new ArgumentOutOfRangeException(
                    nameof(oversizedFieldPath),
                    oversizedFieldPath,
                    "Unknown oversized payload field.")
            };
            using var content = new StringContent(
                JsonSerializer.Serialize(request, DesignConversionModelServerHttpHandler.JsonOptions),
                Encoding.UTF8,
                "application/json");

            using var response = await client.PostAsync(
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                content);
            var bodyJson = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(DesignConversionModelServerPayloadSizeGuard.ErrorCode, bodyJson, StringComparison.Ordinal);
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                response.Headers.GetValues("x-design-conversion-asp-net-route").Single());
            Assert.Equal(
                bool.TrueString,
                response.Headers.GetValues("x-design-conversion-asp-net-host-verified").Single());
            Assert.Equal(
                nameof(DesignConversionModelServerAspNetEndpointMapper),
                response.Headers.GetValues("x-design-conversion-asp-net-endpoint-mapper").Single());

            using var document = JsonDocument.Parse(bodyJson);
            var root = document.RootElement;
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                root.GetProperty("endpoint").GetString());
            Assert.Equal("POST", root.GetProperty("method").GetString());
            Assert.Equal(DesignConversionModelServerPayloadSizeGuard.ErrorCode, root.GetProperty("errorCode").GetString());
            Assert.Contains(oversizedFieldPath, root.GetProperty("errorMessage").GetString(), StringComparison.OrdinalIgnoreCase);

            var metadata = root.GetProperty("boundaryMetadata");
            Assert.Equal(
                nameof(DesignConversionModelServerHttpHandler),
                metadata.GetProperty("asp_net_handler_dispatch_target").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_registry_dispatch_verified").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_host_verified").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_minimal_api_pipeline_verified").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_route_matched").GetString());
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                metadata.GetProperty("asp_net_route_path").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("external_provider_called").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("network_call_executed").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());

            Assert.Empty(browserAdapter.Requests);
            Assert.Empty(comparisonProvider.Requests);
            Assert.Empty(candidateStrategy.Requests);
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Theory]
    [InlineData("designTree.payload")]
    [InlineData("originalTree.payload")]
    public async Task PostImprovementLoop_AspNetHostStartup_OversizedPayload_ReturnsPayloadTooLargeBadRequestWithoutCoordinator(
        string oversizedFieldPath)
    {
        const string privateMarker = "PRIVATE_PAYLOAD_TOO_LARGE_LOOP_ASPNET_HOST";
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var candidateStrategy = new UnexpectedCandidateStrategy();
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider);
        var registry = DesignConversionModelServerAspNetHostStartup.CreateImprovementLoopRegistry(
            composition,
            candidateStrategy);

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(registry);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };

            var basePackage = CreatePackage("payload-too-large-loop-aspnet-host-request", privateMarker);
            var oversizedPayload = new string('y', DesignConversionModelServerPayloadSizeGuard.MaxRawPayloadChars + 1);
            var initialPackage = oversizedFieldPath switch
            {
                "designTree.payload" => basePackage with
                {
                    DesignTree = basePackage.DesignTree! with
                    {
                        Payload = oversizedPayload
                    }
                },
                "originalTree.payload" => basePackage with
                {
                    OriginalTree = basePackage.OriginalTree! with
                    {
                        Payload = oversizedPayload
                    }
                },
                _ => throw new ArgumentOutOfRangeException(
                    nameof(oversizedFieldPath),
                    oversizedFieldPath,
                    "Unknown oversized payload field.")
            };
            var request = new DesignConversionModelServerImprovementLoopRequest(
                LoopId: "payload-too-large-loop-aspnet-host",
                InitialPackage: initialPackage,
                MaxAttempts: 2,
                Metadata: new Dictionary<string, string>
                {
                    ["payload_too_large_host_regression"] = bool.TrueString
                });
            using var content = new StringContent(
                JsonSerializer.Serialize(request, DesignConversionModelServerHttpHandler.JsonOptions),
                Encoding.UTF8,
                "application/json");

            using var response = await client.PostAsync(
                DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
                content);
            var bodyJson = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(DesignConversionModelServerPayloadSizeGuard.ErrorCode, bodyJson, StringComparison.Ordinal);
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
                response.Headers.GetValues("x-design-conversion-asp-net-route").Single());
            Assert.Equal(
                bool.TrueString,
                response.Headers.GetValues("x-design-conversion-asp-net-host-verified").Single());
            Assert.Equal(
                nameof(DesignConversionModelServerAspNetEndpointMapper),
                response.Headers.GetValues("x-design-conversion-asp-net-endpoint-mapper").Single());

            using var document = JsonDocument.Parse(bodyJson);
            var root = document.RootElement;
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
                root.GetProperty("endpoint").GetString());
            Assert.Equal("POST", root.GetProperty("method").GetString());
            Assert.Equal(DesignConversionModelServerPayloadSizeGuard.ErrorCode, root.GetProperty("errorCode").GetString());
            Assert.Contains(oversizedFieldPath, root.GetProperty("errorMessage").GetString(), StringComparison.OrdinalIgnoreCase);

            var metadata = root.GetProperty("boundaryMetadata");
            Assert.Equal(
                nameof(DesignConversionModelServerImprovementLoopHandler),
                metadata.GetProperty("asp_net_handler_dispatch_target").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_registry_dispatch_verified").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_host_verified").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_minimal_api_pipeline_verified").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_route_matched").GetString());
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
                metadata.GetProperty("asp_net_route_path").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("improvement_loop_coordinator_configured").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("external_provider_called").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("network_call_executed").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());

            Assert.Empty(browserAdapter.Requests);
            Assert.Empty(comparisonProvider.Requests);
            Assert.Empty(candidateStrategy.Requests);
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Fact]
    public async Task PostEvaluate_AspNetHostStartup_MalformedJson_ReturnsBadRequestEnvelopeWithoutRuntime()
    {
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "design-conversion-evaluate-aspnet-host-malformed-json"));
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var candidateStrategy = new UnexpectedCandidateStrategy();
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider);
        var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            composition,
            candidateStrategy,
            artifactDb,
            () => DateTimeOffset.UnixEpoch.AddMinutes(511));

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(registry);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };
            using var content = new StringContent(
                "{\"requestId\":\"malformed-evaluate\"",
                Encoding.UTF8,
                "application/json");

            using var response = await client.PostAsync(
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                content);
            var bodyJson = await response.Content.ReadAsStringAsync();

            AssertAspNetHostInputValidationError(
                response,
                bodyJson,
                HttpStatusCode.BadRequest,
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                nameof(DesignConversionModelServerHttpHandler),
                "REQUEST_JSON_INVALID");
            Assert.Contains("invalid JSON", bodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.Empty(browserAdapter.Requests);
            Assert.Empty(comparisonProvider.Requests);
            Assert.Empty(candidateStrategy.Requests);
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Fact]
    public async Task PostEvaluate_AspNetHostStartup_UnsupportedContentType_ReturnsUnsupportedMediaTypeEnvelopeWithoutRuntime()
    {
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "design-conversion-evaluate-aspnet-host-unsupported-content-type"));
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var candidateStrategy = new UnexpectedCandidateStrategy();
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider);
        var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            composition,
            candidateStrategy,
            artifactDb,
            () => DateTimeOffset.UnixEpoch.AddMinutes(512));

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(registry);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };
            using var content = new StringContent(
                "{}",
                Encoding.UTF8,
                "text/plain");

            using var response = await client.PostAsync(
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                content);
            var bodyJson = await response.Content.ReadAsStringAsync();

            AssertAspNetHostInputValidationError(
                response,
                bodyJson,
                HttpStatusCode.UnsupportedMediaType,
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                nameof(DesignConversionModelServerHttpHandler),
                "UNSUPPORTED_MEDIA_TYPE");
            Assert.Contains("requires application/json", bodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.Empty(browserAdapter.Requests);
            Assert.Empty(comparisonProvider.Requests);
            Assert.Empty(candidateStrategy.Requests);
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Fact]
    public async Task PostImprovementLoop_AspNetHostStartup_MalformedJson_ReturnsBadRequestEnvelopeWithoutCoordinator()
    {
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var candidateStrategy = new UnexpectedCandidateStrategy();
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider);
        var registry = DesignConversionModelServerAspNetHostStartup.CreateImprovementLoopRegistry(
            composition,
            candidateStrategy);

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(registry);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };
            using var content = new StringContent(
                "{\"loopId\":\"malformed-loop\"",
                Encoding.UTF8,
                "application/json");

            using var response = await client.PostAsync(
                DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
                content);
            var bodyJson = await response.Content.ReadAsStringAsync();

            AssertAspNetHostInputValidationError(
                response,
                bodyJson,
                HttpStatusCode.BadRequest,
                DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
                nameof(DesignConversionModelServerImprovementLoopHandler),
                "REQUEST_JSON_INVALID");
            Assert.Contains("invalid JSON", bodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.Empty(browserAdapter.Requests);
            Assert.Empty(comparisonProvider.Requests);
            Assert.Empty(candidateStrategy.Requests);
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Fact]
    public async Task PostImprovementLoop_AspNetHostStartup_EnabledAdmissionRejectsBeforeMalformedBodyAndParsesOnlyAfterAdmission()
    {
        const string routeAdmissionCurrentToken = "aspnet-loop-current-route-token-0123456789";
        const string routeAdmissionTenantId = "tenant-aspnet-loop-admission-alpha";
        const string privateMarker = "PRIVATE_ASPNET_LOOP_PRE_BODY_ADMISSION_SECRET";
        const string missingAuthIdempotencyKey = "aspnet-loop-prebody-missing-auth-key";
        const string admittedMalformedIdempotencyKey = "aspnet-loop-prebody-admitted-malformed-key";
        var malformedBodyJson = $$"""
            {"loopId":"aspnet-loop-prebody","metadata":{"private_marker":"{{privateMarker}}","rawTenant":"{{routeAdmissionTenantId}}"}
            """;
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "design-conversion-loop-aspnet-host-prebody-admission"));
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var candidateStrategy = new UnexpectedCandidateStrategy();
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider);
        var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            composition,
            candidateStrategy,
            artifactDb,
            () => DateTimeOffset.UnixEpoch.AddMinutes(520),
            routeAdmissionOptions: new DesignConversionModelServerRouteAdmissionOptions(
                enabled: true,
                currentToken: routeAdmissionCurrentToken,
                tenantId: routeAdmissionTenantId));

        var evaluateRegistration = registry.Resolve(
            DesignConversionModelServerEndpointRegistry.EndpointMethod,
            DesignConversionModelServerEndpointRegistry.EndpointPath);
        var loopRegistration = registry.Resolve(
            DesignConversionModelServerEndpointRegistry.EndpointMethod,
            DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath);
        Assert.True(evaluateRegistration.IsSuccess, evaluateRegistration.ErrorMessage);
        Assert.True(loopRegistration.IsSuccess, loopRegistration.ErrorMessage);
        Assert.NotNull(evaluateRegistration.Value!.RouteAdmission);
        Assert.Same(evaluateRegistration.Value!.RouteAdmission, loopRegistration.Value!.RouteAdmission);
        var routeAdmission = loopRegistration.Value!.RouteAdmission!;
        Assert.True(routeAdmission.Enabled);
        Assert.True(routeAdmission.Ready);
        Assert.True(routeAdmission.TenantBound);
        var routeAdmissionMetadata = routeAdmission.PublicMetadata();
        Assert.True((bool)routeAdmissionMetadata["routeAdmissionEnabled"]);
        Assert.Equal("enabled", routeAdmissionMetadata["routeAdmissionConfigurationStatus"]);
        Assert.True((bool)routeAdmissionMetadata["routeAdmissionTenantBound"]);
        Assert.Equal(
            DesignConversionModelActivationRegistry.BuildTenantIdRef(routeAdmissionTenantId),
            routeAdmissionMetadata["routeAdmissionTenantIdRef"]);
        Assert.DoesNotContain(
            routeAdmissionTenantId,
            JsonSerializer.Serialize(routeAdmissionMetadata),
            StringComparison.Ordinal);

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(registry);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };

            using (var request = CreateAspNetImprovementLoopAdmissionRequest(
                       malformedBodyJson,
                       token: null,
                       idempotencyKey: missingAuthIdempotencyKey))
            using (var response = await client.SendAsync(request))
            {
                var bodyJson = await response.Content.ReadAsStringAsync();
                AssertAspNetHostInputValidationError(
                    response,
                    bodyJson,
                    HttpStatusCode.Unauthorized,
                    DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
                    nameof(DesignConversionModelServerImprovementLoopHandler),
                    "DESIGN_CONVERSION_ROUTE_ADMISSION_TOKEN_REQUIRED");
                Assert.DoesNotContain("REQUEST_JSON_INVALID", bodyJson, StringComparison.Ordinal);
                AssertAspNetResponseAndBoundaryMetadataDoNotContain(
                    bodyJson,
                    routeAdmissionCurrentToken,
                    routeAdmissionTenantId,
                    missingAuthIdempotencyKey,
                    privateMarker,
                    malformedBodyJson);

                using var document = JsonDocument.Parse(bodyJson);
                var metadata = document.RootElement.GetProperty("boundaryMetadata");
                Assert.Equal(bool.FalseString, metadata.GetProperty("route_admission_authorized").GetString());
                Assert.Equal(bool.FalseString, metadata.GetProperty("route_admission_token_supplied").GetString());
                Assert.Equal(bool.FalseString, metadata.GetProperty("route_admission_token_accepted").GetString());
                Assert.Equal(bool.FalseString, metadata.GetProperty("idempotency_key_accepted").GetString());
                Assert.Equal(bool.TrueString, metadata.GetProperty("route_admission_tenant_bound").GetString());
            }

            Assert.Empty(browserAdapter.Requests);
            Assert.Empty(comparisonProvider.Requests);
            Assert.Empty(candidateStrategy.Requests);

            using (var request = CreateAspNetImprovementLoopAdmissionRequest(
                       malformedBodyJson,
                       routeAdmissionCurrentToken,
                       idempotencyKey: null))
            using (var response = await client.SendAsync(request))
            {
                var bodyJson = await response.Content.ReadAsStringAsync();
                AssertAspNetHostInputValidationError(
                    response,
                    bodyJson,
                    HttpStatusCode.BadRequest,
                    DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
                    nameof(DesignConversionModelServerImprovementLoopHandler),
                    "DESIGN_CONVERSION_IDEMPOTENCY_KEY_REQUIRED");
                Assert.Contains(
                    "improvement-loop endpoint requires an Idempotency-Key header",
                    bodyJson,
                    StringComparison.Ordinal);
                Assert.DoesNotContain("evaluate endpoint requires", bodyJson, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("REQUEST_JSON_INVALID", bodyJson, StringComparison.Ordinal);
                AssertAspNetResponseAndBoundaryMetadataDoNotContain(
                    bodyJson,
                    routeAdmissionCurrentToken,
                    routeAdmissionTenantId,
                    privateMarker,
                    malformedBodyJson);

                using var document = JsonDocument.Parse(bodyJson);
                var metadata = document.RootElement.GetProperty("boundaryMetadata");
                Assert.Equal(bool.FalseString, metadata.GetProperty("route_admission_authorized").GetString());
                Assert.Equal(bool.TrueString, metadata.GetProperty("route_admission_token_supplied").GetString());
                Assert.Equal(bool.TrueString, metadata.GetProperty("route_admission_token_accepted").GetString());
                Assert.Equal(bool.FalseString, metadata.GetProperty("idempotency_key_accepted").GetString());
                Assert.Equal(bool.TrueString, metadata.GetProperty("route_admission_tenant_bound").GetString());
            }

            Assert.Empty(browserAdapter.Requests);
            Assert.Empty(comparisonProvider.Requests);
            Assert.Empty(candidateStrategy.Requests);

            using (var request = CreateAspNetImprovementLoopAdmissionRequest(
                       malformedBodyJson,
                       routeAdmissionCurrentToken,
                       admittedMalformedIdempotencyKey))
            using (var response = await client.SendAsync(request))
            {
                var bodyJson = await response.Content.ReadAsStringAsync();
                AssertAspNetHostInputValidationError(
                    response,
                    bodyJson,
                    HttpStatusCode.BadRequest,
                    DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
                    nameof(DesignConversionModelServerImprovementLoopHandler),
                    "REQUEST_JSON_INVALID");
                Assert.Contains("invalid JSON", bodyJson, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(
                    "DESIGN_CONVERSION_ROUTE_ADMISSION_TOKEN_REQUIRED",
                    bodyJson,
                    StringComparison.Ordinal);
                AssertAspNetResponseAndBoundaryMetadataDoNotContain(
                    bodyJson,
                    routeAdmissionCurrentToken,
                    routeAdmissionTenantId,
                    admittedMalformedIdempotencyKey,
                    privateMarker,
                    malformedBodyJson);

                using var document = JsonDocument.Parse(bodyJson);
                var metadata = document.RootElement.GetProperty("boundaryMetadata");
                Assert.Equal(bool.TrueString, metadata.GetProperty("route_admission_authorized").GetString());
                Assert.Equal(bool.TrueString, metadata.GetProperty("route_admission_token_supplied").GetString());
                Assert.Equal(bool.TrueString, metadata.GetProperty("route_admission_token_accepted").GetString());
                Assert.Equal(bool.TrueString, metadata.GetProperty("idempotency_key_accepted").GetString());
                Assert.Equal(bool.TrueString, metadata.GetProperty("idempotency_route_scoped").GetString());
                Assert.Equal(bool.TrueString, metadata.GetProperty("route_admission_tenant_bound").GetString());
                Assert.Equal(
                    DesignConversionModelActivationRegistry.BuildTenantIdRef(routeAdmissionTenantId),
                    metadata.GetProperty("route_admission_tenant_id_ref").GetString());
                Assert.Equal(bool.FalseString, metadata.GetProperty("route_admission_raw_tenant_id_exposed").GetString());
                Assert.Equal(bool.FalseString, metadata.GetProperty("idempotency_payload_digest_exposed").GetString());
                Assert.Equal("ref_only", metadata.GetProperty("route_admission_method").GetString());
                Assert.Equal("ref_only", metadata.GetProperty("route_admission_path").GetString());
            }

            Assert.Empty(browserAdapter.Requests);
            Assert.Empty(comparisonProvider.Requests);
            Assert.Empty(candidateStrategy.Requests);
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Fact]
    public async Task PostImprovementLoop_AspNetHostStartup_UnsupportedContentType_ReturnsUnsupportedMediaTypeEnvelopeWithoutCoordinator()
    {
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var candidateStrategy = new UnexpectedCandidateStrategy();
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider);
        var registry = DesignConversionModelServerAspNetHostStartup.CreateImprovementLoopRegistry(
            composition,
            candidateStrategy);

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(registry);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };
            using var content = new StringContent(
                "{}",
                Encoding.UTF8,
                "text/plain");

            using var response = await client.PostAsync(
                DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
                content);
            var bodyJson = await response.Content.ReadAsStringAsync();

            AssertAspNetHostInputValidationError(
                response,
                bodyJson,
                HttpStatusCode.UnsupportedMediaType,
                DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
                nameof(DesignConversionModelServerImprovementLoopHandler),
                "UNSUPPORTED_MEDIA_TYPE");
            Assert.Contains("requires application/json", bodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.Empty(browserAdapter.Requests);
            Assert.Empty(comparisonProvider.Requests);
            Assert.Empty(candidateStrategy.Requests);
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task PostEvaluate_AspNetHostStartup_MissingOrEmptyBody_ReturnsBadRequestEnvelopeWithoutRuntime(
        bool omitContent)
    {
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "design-conversion-evaluate-aspnet-host-empty-body"));
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var candidateStrategy = new UnexpectedCandidateStrategy();
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider);
        var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            composition,
            candidateStrategy,
            artifactDb,
            () => DateTimeOffset.UnixEpoch.AddMinutes(513));

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(registry);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };
            using var content = omitContent
                ? null
                : new StringContent(string.Empty, Encoding.UTF8, "application/json");

            using var response = await client.PostAsync(
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                content);
            var bodyJson = await response.Content.ReadAsStringAsync();

            AssertAspNetHostInputValidationError(
                response,
                bodyJson,
                HttpStatusCode.BadRequest,
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                nameof(DesignConversionModelServerHttpHandler),
                "REQUEST_BODY_REQUIRED");
            Assert.Contains("requires a JSON request body", bodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.Empty(browserAdapter.Requests);
            Assert.Empty(comparisonProvider.Requests);
            Assert.Empty(candidateStrategy.Requests);
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Fact]
    public async Task PostEvaluate_AspNetHostStartup_NullJsonBody_ReturnsBadRequestEnvelopeWithoutRuntime()
    {
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "design-conversion-evaluate-aspnet-host-null-json-body"));
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var candidateStrategy = new UnexpectedCandidateStrategy();
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider);
        var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            composition,
            candidateStrategy,
            artifactDb,
            () => DateTimeOffset.UnixEpoch.AddMinutes(514));

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(registry);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };
            using var content = new StringContent("null", Encoding.UTF8, "application/json");

            using var response = await client.PostAsync(
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                content);
            var bodyJson = await response.Content.ReadAsStringAsync();

            AssertAspNetHostInputValidationError(
                response,
                bodyJson,
                HttpStatusCode.BadRequest,
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                nameof(DesignConversionModelServerHttpHandler),
                "REQUEST_BODY_REQUIRED");
            Assert.Contains("empty JSON request", bodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.Empty(browserAdapter.Requests);
            Assert.Empty(comparisonProvider.Requests);
            Assert.Empty(candidateStrategy.Requests);
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Theory]
    [InlineData("true")]
    [InlineData("42")]
    [InlineData("[]")]
    public async Task PostEvaluate_AspNetHostStartup_TopLevelJsonScalarOrArrayBody_ReturnsBadRequestEnvelopeWithoutRuntime(
        string bodyJson)
    {
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "design-conversion-evaluate-aspnet-host-invalid-json-shape"));
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var candidateStrategy = new UnexpectedCandidateStrategy();
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider);
        var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            composition,
            candidateStrategy,
            artifactDb,
            () => DateTimeOffset.UnixEpoch.AddMinutes(515));

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(registry);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };
            using var content = new StringContent(bodyJson, Encoding.UTF8, "application/json");

            using var response = await client.PostAsync(
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                content);
            var responseBodyJson = await response.Content.ReadAsStringAsync();

            AssertAspNetHostInputValidationError(
                response,
                responseBodyJson,
                HttpStatusCode.BadRequest,
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                nameof(DesignConversionModelServerHttpHandler),
                "REQUEST_JSON_INVALID");
            Assert.Contains("invalid JSON", responseBodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.Empty(browserAdapter.Requests);
            Assert.Empty(comparisonProvider.Requests);
            Assert.Empty(candidateStrategy.Requests);
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task PostImprovementLoop_AspNetHostStartup_MissingOrEmptyBody_ReturnsBadRequestEnvelopeWithoutCoordinator(
        bool omitContent)
    {
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var candidateStrategy = new UnexpectedCandidateStrategy();
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider);
        var registry = DesignConversionModelServerAspNetHostStartup.CreateImprovementLoopRegistry(
            composition,
            candidateStrategy);

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(registry);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };
            using var content = omitContent
                ? null
                : new StringContent(string.Empty, Encoding.UTF8, "application/json");

            using var response = await client.PostAsync(
                DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
                content);
            var bodyJson = await response.Content.ReadAsStringAsync();

            AssertAspNetHostInputValidationError(
                response,
                bodyJson,
                HttpStatusCode.BadRequest,
                DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
                nameof(DesignConversionModelServerImprovementLoopHandler),
                "REQUEST_BODY_REQUIRED");
            Assert.Contains("requires a JSON request body", bodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.Empty(browserAdapter.Requests);
            Assert.Empty(comparisonProvider.Requests);
            Assert.Empty(candidateStrategy.Requests);
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Fact]
    public async Task PostImprovementLoop_AspNetHostStartup_NullJsonBody_ReturnsBadRequestEnvelopeWithoutCoordinator()
    {
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var candidateStrategy = new UnexpectedCandidateStrategy();
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider);
        var registry = DesignConversionModelServerAspNetHostStartup.CreateImprovementLoopRegistry(
            composition,
            candidateStrategy);

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(registry);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };
            using var content = new StringContent("null", Encoding.UTF8, "application/json");

            using var response = await client.PostAsync(
                DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
                content);
            var bodyJson = await response.Content.ReadAsStringAsync();

            AssertAspNetHostInputValidationError(
                response,
                bodyJson,
                HttpStatusCode.BadRequest,
                DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
                nameof(DesignConversionModelServerImprovementLoopHandler),
                "IMPROVEMENT_LOOP_REQUEST_REQUIRED");
            Assert.Contains("request body is required", bodyJson, StringComparison.OrdinalIgnoreCase);

            using var document = JsonDocument.Parse(bodyJson);
            var metadata = document.RootElement.GetProperty("boundaryMetadata");
            Assert.Equal(bool.TrueString, metadata.GetProperty("improvement_loop_coordinator_configured").GetString());

            Assert.Empty(browserAdapter.Requests);
            Assert.Empty(comparisonProvider.Requests);
            Assert.Empty(candidateStrategy.Requests);
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Theory]
    [InlineData("true")]
    [InlineData("42")]
    [InlineData("[]")]
    public async Task PostImprovementLoop_AspNetHostStartup_TopLevelJsonScalarOrArrayBody_ReturnsBadRequestEnvelopeWithoutCoordinator(
        string bodyJson)
    {
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var candidateStrategy = new UnexpectedCandidateStrategy();
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider);
        var registry = DesignConversionModelServerAspNetHostStartup.CreateImprovementLoopRegistry(
            composition,
            candidateStrategy);

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(registry);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };
            using var content = new StringContent(bodyJson, Encoding.UTF8, "application/json");

            using var response = await client.PostAsync(
                DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
                content);
            var responseBodyJson = await response.Content.ReadAsStringAsync();

            AssertAspNetHostInputValidationError(
                response,
                responseBodyJson,
                HttpStatusCode.BadRequest,
                DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
                nameof(DesignConversionModelServerImprovementLoopHandler),
                "REQUEST_JSON_INVALID");
            Assert.Contains("invalid JSON", responseBodyJson, StringComparison.OrdinalIgnoreCase);

            using var document = JsonDocument.Parse(responseBodyJson);
            var metadata = document.RootElement.GetProperty("boundaryMetadata");
            Assert.Equal(bool.TrueString, metadata.GetProperty("improvement_loop_coordinator_configured").GetString());

            Assert.Empty(browserAdapter.Requests);
            Assert.Empty(comparisonProvider.Requests);
            Assert.Empty(candidateStrategy.Requests);
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Theory]
    [InlineData("evaluate_unsupported_source_kind", DesignConversionModelServerEndpointRegistry.EndpointPath, nameof(DesignConversionModelServerHttpHandler), "SOURCE_KIND_UNSUPPORTED")]
    [InlineData("evaluate_missing_source_tree_ref_shape", DesignConversionModelServerEndpointRegistry.EndpointPath, nameof(DesignConversionModelServerHttpHandler), "DESIGN_CONVERSION_SOURCE_TREE_REQUIRED")]
    [InlineData("evaluate_missing_original_tree_ref_shape", DesignConversionModelServerEndpointRegistry.EndpointPath, nameof(DesignConversionModelServerHttpHandler), "DESIGN_CONVERSION_ORIGINAL_TREE_REQUIRED")]
    [InlineData("evaluate_missing_artifact_refs", DesignConversionModelServerEndpointRegistry.EndpointPath, nameof(DesignConversionModelServerHttpHandler), "DESIGN_CONVERSION_SCREENSHOT_OR_RENDER_REQUIRED")]
    [InlineData("evaluate_unsupported_screenshot_ref", DesignConversionModelServerEndpointRegistry.EndpointPath, nameof(DesignConversionModelServerHttpHandler), "DESIGN_CONVERSION_ARTIFACT_REF_PROOF_REQUIRED")]
    [InlineData("evaluate_unsupported_design_render_ref", DesignConversionModelServerEndpointRegistry.EndpointPath, nameof(DesignConversionModelServerHttpHandler), "DESIGN_CONVERSION_ARTIFACT_REF_PROOF_REQUIRED")]
    [InlineData("evaluate_invalid_viewport", DesignConversionModelServerEndpointRegistry.EndpointPath, nameof(DesignConversionModelServerHttpHandler), "DESIGN_CONVERSION_VIEWPORT_REQUIRED")]
    [InlineData("improvement_loop_unsupported_source_kind", DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath, nameof(DesignConversionModelServerImprovementLoopHandler), "SOURCE_KIND_UNSUPPORTED")]
    [InlineData("improvement_loop_missing_source_tree_ref_shape", DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath, nameof(DesignConversionModelServerImprovementLoopHandler), "DESIGN_CONVERSION_SOURCE_TREE_REQUIRED")]
    [InlineData("improvement_loop_missing_original_tree_ref_shape", DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath, nameof(DesignConversionModelServerImprovementLoopHandler), "DESIGN_CONVERSION_ORIGINAL_TREE_REQUIRED")]
    [InlineData("improvement_loop_missing_artifact_refs", DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath, nameof(DesignConversionModelServerImprovementLoopHandler), "DESIGN_CONVERSION_SCREENSHOT_OR_RENDER_REQUIRED")]
    [InlineData("improvement_loop_unsupported_screenshot_ref", DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath, nameof(DesignConversionModelServerImprovementLoopHandler), "DESIGN_CONVERSION_ARTIFACT_REF_PROOF_REQUIRED")]
    [InlineData("improvement_loop_unsupported_design_render_ref", DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath, nameof(DesignConversionModelServerImprovementLoopHandler), "DESIGN_CONVERSION_ARTIFACT_REF_PROOF_REQUIRED")]
    [InlineData("improvement_loop_invalid_viewport", DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath, nameof(DesignConversionModelServerImprovementLoopHandler), "DESIGN_CONVERSION_VIEWPORT_REQUIRED")]
    public async Task RegisteredDesignConversionRoute_AspNetHostStartup_ValidJsonObjectValidationFailures_ReturnsBadRequestEnvelopeWithoutDownstreamInvocation(
        string scenario,
        string endpointPath,
        string handlerName,
        string expectedErrorCode)
    {
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "design-conversion-aspnet-host-valid-object-validation"));
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var candidateStrategy = new UnexpectedCandidateStrategy();
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider);
        var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            composition,
            candidateStrategy,
            artifactDb,
            () => DateTimeOffset.UnixEpoch.AddMinutes(516));

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(registry);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };
            using var content = new StringContent(
                CreateAspNetHostValidObjectValidationBodyJson(scenario),
                Encoding.UTF8,
                "application/json");

            using var response = await client.PostAsync(endpointPath, content);
            var responseBodyJson = await response.Content.ReadAsStringAsync();

            AssertAspNetHostInputValidationError(
                response,
                responseBodyJson,
                HttpStatusCode.BadRequest,
                endpointPath,
                handlerName,
                expectedErrorCode);
            Assert.Empty(browserAdapter.Requests);
            Assert.Empty(comparisonProvider.Requests);
            Assert.Empty(candidateStrategy.Requests);
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Theory]
    [InlineData("artifact_uri_ref")]
    [InlineData("outside_root_file_ref")]
    public async Task PostEvaluate_AspNetHostStartup_StrictServerOwnedArtifactProofRejectsInvalidRefsBeforeRuntime(
        string scenario)
    {
        const string privateMarker = "PRIVATE_STRICT_ARTIFACT_REF_ASPNET_NEGATIVE_SECRET";
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "design-conversion-aspnet-strict-artifact-proof-negative");
        var artifactRoot = Path.Combine(tempRoot, "server-owned-artifacts");
        var outsideRoot = Path.Combine(tempRoot, "outside-artifacts");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(Path.Combine(tempRoot, "artifact-db"));
        var inRootArtifact = WriteServerOwnedAspNetPngArtifact(artifactRoot, $"strict-{scenario}-in-root");
        var outsideRootArtifact = WriteServerOwnedAspNetPngArtifact(outsideRoot, $"strict-{scenario}-outside-root");
        var artifactRef = scenario switch
        {
            "artifact_uri_ref" => inRootArtifact.ArtifactRef with
            {
                Uri = "artifact://pass143/live-local/client-controlled-screenshot.png"
            },
            "outside_root_file_ref" => outsideRootArtifact.ArtifactRef,
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "Unknown strict artifact proof scenario.")
        };

        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var candidateStrategy = new UnexpectedCandidateStrategy();
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider);
        var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            composition,
            candidateStrategy,
            artifactDb,
            () => DateTimeOffset.UnixEpoch.AddMinutes(518),
            artifactRefResolver: new DesignConversionServerOwnedLocalArtifactRefResolver(artifactRoot));

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(registry);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };
            var baseRequest = CreateEvaluateRequest($"strict-artifact-proof-{scenario}", privateMarker);
            var request = baseRequest with
            {
                Screenshots = [artifactRef],
                DesignRenders = [],
                Metadata = new Dictionary<string, string>(baseRequest.Metadata ?? new Dictionary<string, string>())
                {
                    ["artifact_ref_proof_verified"] = bool.TrueString,
                    ["persistence_verified"] = bool.TrueString,
                    ["product_ready"] = bool.TrueString
                }
            };
            using var content = new StringContent(
                JsonSerializer.Serialize(request, DesignConversionModelServerHttpHandler.JsonOptions),
                Encoding.UTF8,
                "application/json");

            using var response = await client.PostAsync(
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                content);
            var responseBodyJson = await response.Content.ReadAsStringAsync();

            AssertAspNetHostInputValidationError(
                response,
                responseBodyJson,
                HttpStatusCode.BadRequest,
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                nameof(DesignConversionModelServerHttpHandler),
                "DESIGN_CONVERSION_ARTIFACT_REF_PROOF_REQUIRED");
            Assert.DoesNotContain(privateMarker, responseBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(request.Html, responseBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(request.Css, responseBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(request.DesignTree.Payload!, responseBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(request.OriginalTree.Payload!, responseBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(tempRoot, responseBodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.Empty(browserAdapter.Requests);
            Assert.Empty(comparisonProvider.Requests);
            Assert.Empty(candidateStrategy.Requests);
        }
        finally
        {
            await app.StopAsync();
            DeleteDirectoryIfExists(tempRoot);
        }
    }

    [Fact]
    public async Task PostEvaluate_AspNetHostStartup_StrictServerOwnedArtifactProofAcceptsInRootPngAndPersistsStatus()
    {
        const string privateMarker = "PRIVATE_STRICT_ARTIFACT_REF_ASPNET_POSITIVE_SECRET";
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "design-conversion-aspnet-strict-artifact-proof-positive");
        var artifactRoot = Path.Combine(tempRoot, "server-owned-artifacts");
        var artifactDbRoot = Path.Combine(tempRoot, "artifact-db");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
        var proof = WriteServerOwnedAspNetPngArtifact(artifactRoot, "dummy_corp-placeholder-strict-server-owned-positive-proof");
        var proofMetadata = (proof.ArtifactRef.Metadata ?? new Dictionary<string, string>())
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        proofMetadata["business_account"] = "dummy_corp";
        proofMetadata["campaign_label"] = "placeholder launch proof";
        proof = proof with
        {
            ArtifactRef = proof.ArtifactRef with
            {
                Metadata = proofMetadata
            }
        };
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider);
        var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            composition,
            new UnexpectedCandidateStrategy(),
            artifactDb,
            () => DateTimeOffset.UnixEpoch.AddMinutes(519),
            artifactRefResolver: new DesignConversionServerOwnedLocalArtifactRefResolver(artifactRoot));

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(registry);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };
            var baseRequest = CreateEvaluateRequest("strict-artifact-proof-positive", privateMarker);
            var request = baseRequest with
            {
                Screenshots = [proof.ArtifactRef],
                DesignRenders = [],
                Metadata = new Dictionary<string, string>(baseRequest.Metadata ?? new Dictionary<string, string>())
                {
                    ["artifact_ref_proof_verified"] = bool.TrueString,
                    ["persistence_verified"] = bool.TrueString,
                    ["local_artifact_db_persistence_verified"] = bool.TrueString,
                    ["product_ready"] = bool.TrueString
                }
            };
            using var content = new StringContent(
                JsonSerializer.Serialize(request, DesignConversionModelServerHttpHandler.JsonOptions),
                Encoding.UTF8,
                "application/json");

            using var response = await client.PostAsync(
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                content);
            var bodyJson = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.DoesNotContain(privateMarker, bodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(request.Html, bodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(request.Css, bodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(request.DesignTree.Payload!, bodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(request.OriginalTree.Payload!, bodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(tempRoot, bodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(new Uri(proof.LocalPath).AbsoluteUri, bodyJson, StringComparison.OrdinalIgnoreCase);

            using var document = JsonDocument.Parse(bodyJson);
            var root = document.RootElement;
            Assert.Equal(DesignConversionThinClientStatus.NotReady, root.GetProperty("contractStatus").GetString());
            Assert.Equal(request.RequestId, root.GetProperty("requestId").GetString());
            Assert.False(root.GetProperty("publicHttpEndpointVerified").GetBoolean());
            Assert.False(root.GetProperty("deploymentVerified").GetBoolean());
            Assert.False(root.GetProperty("modelTrainingVerified").GetBoolean());
            Assert.False(root.GetProperty("modelUploadReady").GetBoolean());
            Assert.False(root.GetProperty("productReady").GetBoolean());

            var metadata = root.GetProperty("boundaryMetadata");
            AssertTrustedAspNetRouteMetadata(
                metadata,
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                nameof(DesignConversionModelServerHttpHandler));
            Assert.Equal(bool.TrueString, metadata.GetProperty("runtime_invoked").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("public_http_endpoint_verified").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("deployment_verified").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("model_training_verified").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("model_upload_ready").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("ModelUploadReady").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
            var runtimeProvenance = root.GetProperty("runtimeStatus").GetProperty("provenance");
            Assert.Equal(
                DesignConversionVisualQaBoundaryPolicy.DecisionBlocked,
                runtimeProvenance.GetProperty("final_visual_decision").GetString());
            Assert.Equal(
                DesignConversionVisualQaBoundaryPolicy.ViewportOperationFailed,
                runtimeProvenance.GetProperty("tier_policy_stop_reason").GetString());
            Assert.Equal(
                DesignConversionThinClientStatus.NotReady,
                runtimeProvenance.GetProperty("thin_client_terminal_status").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("persistence_caller_claim_accepted").GetString());
            Assert.Equal(bool.TrueString, metadata.GetProperty("persistence_status_spoof_rejected").GetString());

            var jobId = root.GetProperty("jobId").GetString();
            Assert.False(string.IsNullOrWhiteSpace(jobId));
            AssertPersistenceVerifiedMetadata(metadata, jobId!, request.RequestId);
            var evaluatePersistenceRecordId = metadata.GetProperty("persistence_record_id").GetString();

            using (var statusResponse = await client.GetAsync(DesignConversionModelServerStatusHandler.EndpointPathPrefix + jobId))
            {
                var statusBodyJson = await statusResponse.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.OK, statusResponse.StatusCode);
                Assert.DoesNotContain(privateMarker, statusBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(request.Html, statusBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(request.Css, statusBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(tempRoot, statusBodyJson, StringComparison.OrdinalIgnoreCase);

                using var statusDocument = JsonDocument.Parse(statusBodyJson);
                var statusRoot = statusDocument.RootElement;
                Assert.Equal(DesignConversionThinClientStatus.NotReady, statusRoot.GetProperty("contractStatus").GetString());
                Assert.False(statusRoot.GetProperty("publicHttpEndpointVerified").GetBoolean());
                Assert.False(statusRoot.GetProperty("deploymentVerified").GetBoolean());
                Assert.False(statusRoot.GetProperty("productReady").GetBoolean());

                var statusMetadata = statusRoot.GetProperty("boundaryMetadata");
                AssertTrustedAspNetRouteMetadata(
                    statusMetadata,
                    DesignConversionModelServerEndpointRegistry.StatusReadbackEndpointPath,
                    nameof(DesignConversionModelServerStatusHandler));
                AssertPersistenceVerifiedMetadata(statusMetadata, jobId!, request.RequestId);
                Assert.Equal(
                    evaluatePersistenceRecordId,
                    statusMetadata.GetProperty("persistence_record_id").GetString());
                Assert.Equal(bool.FalseString, statusMetadata.GetProperty("public_http_endpoint_verified").GetString());
                Assert.Equal(bool.FalseString, statusMetadata.GetProperty("deployment_verified").GetString());
                Assert.Equal(bool.FalseString, statusMetadata.GetProperty("model_training_verified").GetString());
                Assert.Equal(bool.FalseString, statusMetadata.GetProperty("model_upload_ready").GetString());
                Assert.Equal(bool.FalseString, statusMetadata.GetProperty("product_ready").GetString());
            }

            var records = await artifactDb.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(
                JobId: jobId,
                RequestId: request.RequestId));
            Assert.True(records.IsSuccess, records.ErrorMessage);
            var record = Assert.Single(records.Value!);
            Assert.Equal(evaluatePersistenceRecordId, record.RecordId);
            Assert.Equal(jobId, record.JobId);
            Assert.Equal(request.RequestId, record.RequestId);
            Assert.NotEmpty(browserAdapter.Requests);
            Assert.NotEmpty(comparisonProvider.Requests);
        }
        finally
        {
            await app.StopAsync();
            DeleteDirectoryIfExists(tempRoot);
        }
    }

    [Fact]
    public async Task GetVisualCosts_AspNetHostStartup_AcceptedTenantBoundCompletedJobReturnsOneRecordAndPrivacySafeNoStore()
    {
        const string routeAdmissionCurrentToken = "aspnet-visual-cost-route-token-0123456789";
        const string routeAdmissionTenantId = "tenant-aspnet-visual-cost-alpha";
        const string rawUserId = "raw-user-aspnet-visual-cost-91B7";
        const string privateMarker = "PRIVATE_ASPNET_VISUAL_COST_SECRET_91B7";
        const string trustedVisualCostTier = "paid";
        const string trustedVisualCostMonthlyCap = "125.75";
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "design-conversion-aspnet-visual-cost-success");
        var artifactDbRoot = Path.Combine(tempRoot, "artifact-db");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
        var activationStateRoot = Path.Combine(tempRoot, "activation-state");
        var utcNow = () => DateTimeOffset.UnixEpoch.AddMinutes(731);
        var activeCandidate =
            await DesignConversionActivatedModelRepositoryHostRuntimeTests.BuildActivationRecordAsync(
                artifactDbRoot,
                routeAdmissionTenantId,
                "aspnet-visual-cost-success-active",
                batchCount: 1);
        var activationRegistry = new DesignConversionModelActivationRegistry(
            activationStateRoot,
            utcNow,
            allowedArtifactRootDirectory: artifactDbRoot);
        var activation = await activationRegistry.ActivateAsync(
            new DesignConversionModelActivationCommand(
                routeAdmissionTenantId,
                RequestedByRef: "architect:aspnet-visual-cost-success",
                Reason: "pre-seed a valid tenant active model before visual-cost evaluation",
                IdempotencyKey: "activate:aspnet-visual-cost-success",
                ExpectedGeneration: 0,
                Candidate: activeCandidate,
                ExpectedModelVersion: activeCandidate.Model.Version,
                RequestedAtUtc: utcNow()));
        Assert.True(activation.IsSuccess, activation.ErrorMessage);
        var ledgerRoot = Path.Combine(tempRoot, "visual-cost-ledger");
        using var ledgerRootOverride = UseVisualCostLedgerRoot(ledgerRoot);
        var visualCostBudgetStoreRoot = Path.Combine(tempRoot, "visual-cost-budget-store");
        var visualCostBudgetStore = new JsonlDesignConversionVisualCostBudgetStore(visualCostBudgetStoreRoot);
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var visualJudge = new AspNetVisualCostReceiptJudge(0.0600m);
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider,
            visualJudge);
        var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            composition,
            new UnexpectedCandidateStrategy(),
            artifactDb,
            utcNow,
            activationStateRootDirectory: activationStateRoot,
            modelLifecycleAuthToken: "aspnet-visual-cost-model-lifecycle-token-0123456789",
            routeAdmissionOptions: new DesignConversionModelServerRouteAdmissionOptions(
                enabled: true,
                currentToken: routeAdmissionCurrentToken,
                tenantId: routeAdmissionTenantId),
            visualCostBudgetStore: visualCostBudgetStore);

        var visualCostRegistration = registry.Resolve(
            DesignConversionModelServerEndpointRegistry.VisualCostReadbackEndpointMethod,
            DesignConversionModelServerVisualCostHandler.EndpointPathPrefix + "thin-job-visual-cost-registry-probe" +
            DesignConversionModelServerVisualCostHandler.EndpointPathSuffix);
        Assert.True(visualCostRegistration.IsSuccess, visualCostRegistration.ErrorMessage);
        Assert.NotNull(visualCostRegistration.Value!.RouteAdmission);
        Assert.Equal(
            nameof(DesignConversionModelServerVisualCostHandler),
            visualCostRegistration.Value.BoundaryMetadata["delegates_to_handler"]);
        Assert.NotEqual(
            nameof(DesignConversionModelServerStatusHandler),
            visualCostRegistration.Value.BoundaryMetadata["delegates_to_handler"]);

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(registry);

        await app.StartAsync();
        try
        {
            var startupMetadata = app.Services
                .GetRequiredService<EndpointDataSource>()
                .Endpoints
                .Select(endpoint => endpoint.Metadata.GetMetadata<DesignConversionModelServerAspNetHostStartupMetadata>())
                .Where(metadata => metadata is not null)
                .Single(metadata => string.Equals(
                    metadata!.MappedRoute,
                    DesignConversionModelServerEndpointRegistry.VisualCostReadbackEndpointPath,
                    StringComparison.Ordinal))!;
            Assert.Equal(
                DesignConversionModelServerAspNetHostStartup.HostStartupSurface,
                startupMetadata.StartupSurface);
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry.VisualCostReadbackEndpointPath,
                startupMetadata.MappedRoute);
            Assert.Equal(
                DesignConversionModelServerAspNetHostStartup.FullLocalRouteSet(),
                startupMetadata.MappedRoutes);
            Assert.True(startupMetadata.LocalOnly);
            Assert.True(startupMetadata.RefsOnly);
            Assert.False(startupMetadata.PublicDeploymentVerified);
            Assert.False(startupMetadata.CrossRepoCallerVerified);
            Assert.False(startupMetadata.ProductReady);

            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };

            var baseRequest = CreateEvaluateRequest("visual-cost-aspnet-exact-request", privateMarker);
            var evaluateRequest = baseRequest with
            {
                Html = $"<main data-private=\"{privateMarker}\">Visual cost</main>",
                Css = $".visual-cost{{--private:'{privateMarker}';display:grid;}}",
                DesignTree = baseRequest.DesignTree with
                {
                    Payload = $"{{\"private\":\"{privateMarker}\",\"kind\":\"figma\"}}"
                },
                OriginalTree = baseRequest.OriginalTree with
                {
                    Payload = $"{{\"private\":\"original-{privateMarker}\",\"kind\":\"figma\"}}"
                },
                Budget = baseRequest.Budget! with
                {
                    UserId = rawUserId,
                    Tier = trustedVisualCostTier,
                    MaxImprovementCandidates = 1,
                    MaxRenderQaRuns = 1,
                    BudgetUsd = 0.25m,
                    Metadata = new Dictionary<string, string>()
                },
                Metadata = new Dictionary<string, string>(
                    baseRequest.Metadata ?? new Dictionary<string, string>(),
                    StringComparer.Ordinal)
                {
                    ["tenant_id"] = routeAdmissionTenantId,
                    [DesignConversionModelServerApi.ServerAssignedDesignTierMetadataKey] =
                        trustedVisualCostTier,
                    [DesignConversionModelServerApi.ServerAssignedMonthlyVisualCostLimitUsdMetadataKey] =
                        trustedVisualCostMonthlyCap,
                    [DesignConversionModelServerApi.DesignTierAuthorityMetadataKey] =
                        DesignConversionModelServerApi.PlatformTenantConfigurationAuthority,
                    [DesignConversionModelServerApi.MonthlyVisualCostLimitAuthorityMetadataKey] =
                        DesignConversionModelServerApi.PlatformTenantConfigurationAuthority,
                    [DesignConversionModelServerApi.UserIdentityAuthorityMetadataKey] =
                        DesignConversionModelServerApi.VerifiedInboundPrincipalAuthority
                }
            };
            using (var evaluate = CreateAspNetEvaluateAdmissionRequest(
                       JsonSerializer.Serialize(evaluateRequest, DesignConversionModelServerHttpHandler.JsonOptions),
                       routeAdmissionCurrentToken,
                       "visual-cost-aspnet-evaluate-key"))
            using (var evaluateResponse = await client.SendAsync(evaluate))
            {
                var evaluateBodyJson = await evaluateResponse.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.OK, evaluateResponse.StatusCode);
                Assert.DoesNotContain(privateMarker, evaluateBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(rawUserId, evaluateBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(routeAdmissionCurrentToken, evaluateBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(routeAdmissionTenantId, evaluateBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(ledgerRoot, evaluateBodyJson, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(visualCostBudgetStoreRoot, evaluateBodyJson, StringComparison.OrdinalIgnoreCase);
                using var evaluateDocument = JsonDocument.Parse(evaluateBodyJson);
                var evaluateMetadata = evaluateDocument.RootElement.GetProperty("boundaryMetadata");
                Assert.Equal(bool.TrueString, evaluateMetadata.GetProperty("trusted_visual_cost_entitlement_bound").GetString());
                Assert.Equal(
                    trustedVisualCostTier,
                    evaluateMetadata.GetProperty("trusted_visual_cost_entitlement_design_tier").GetString());
                Assert.Equal(
                    trustedVisualCostMonthlyCap,
                    evaluateMetadata.GetProperty("trusted_visual_cost_entitlement_monthly_limit_usd").GetString());
                Assert.Equal(bool.FalseString, evaluateMetadata.GetProperty("trusted_visual_cost_entitlement_raw_user_exposed").GetString());
                Assert.Equal(bool.TrueString, evaluateMetadata.GetProperty("trusted_visual_cost_reservation_store_wired").GetString());
            }

            Assert.Equal(1, visualJudge.InvocationCount);

            string jobId;
            var records = await artifactDb.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(
                RequestId: evaluateRequest.RequestId));
            Assert.True(records.IsSuccess, records.ErrorMessage);
            jobId = Assert.Single(records.Value!).JobId;

            var visualRoute = VisualCostRoute(jobId);
            using (var readbackRequest = CreateAspNetVisualCostReadbackAdmissionRequest(
                       visualRoute,
                       routeAdmissionCurrentToken))
            using (var readbackResponse = await client.SendAsync(readbackRequest))
            {
                var bodyJson = await readbackResponse.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.OK, readbackResponse.StatusCode);
                AssertNoStoreHeaders(readbackResponse);
                Assert.Equal(
                    DesignConversionModelServerEndpointRegistry.VisualCostReadbackEndpointPath,
                    readbackResponse.Headers.GetValues("x-design-conversion-asp-net-route").Single());
                Assert.Equal(
                    bool.FalseString,
                    readbackResponse.Headers.GetValues("x-design-conversion-product-ready").Single());
                AssertAspNetResponseAndBoundaryMetadataDoNotContain(
                    bodyJson,
                    jobId,
                    evaluateRequest.RequestId,
                    rawUserId,
                    privateMarker,
                    evaluateRequest.Html,
                    evaluateRequest.Css,
                    evaluateRequest.DesignTree.Payload,
                    evaluateRequest.OriginalTree.Payload,
                    routeAdmissionCurrentToken,
                    routeAdmissionTenantId,
                    ledgerRoot,
                    visualCostBudgetStoreRoot);

                using var document = JsonDocument.Parse(bodyJson);
                var root = document.RootElement;
                Assert.Equal(DesignConversionModelServerVisualCostHandler.EndpointTemplatePath, root.GetProperty("endpoint").GetString());
                Assert.Equal("GET", root.GetProperty("method").GetString());
                Assert.Equal(200, root.GetProperty("statusCode").GetInt32());
                Assert.False(root.GetProperty("productReady").GetBoolean());
                Assert.False(root.GetProperty("deploymentVerified").GetBoolean());
                Assert.False(root.GetProperty("publicHttpEndpointVerified").GetBoolean());

                var summary = root.GetProperty("summary");
                Assert.Equal(1, summary.GetProperty("recordCount").GetInt32());
                Assert.Equal(0.0600m, summary.GetProperty("totalCostUsd").GetDecimal());
                Assert.True(summary.GetProperty("measurementComplete").GetBoolean());
                Assert.Equal(0, summary.GetProperty("missingMeasurementCount").GetInt32());
                Assert.Equal(1, summary.GetProperty("evaluationAttemptCount").GetInt32());
                Assert.Equal(
                    DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
                    summary.GetProperty("costSource").GetString());

                var record = Assert.Single(root.GetProperty("records").EnumerateArray());
                Assert.Equal(0.0600m, record.GetProperty("totalCostUsd").GetDecimal());
                Assert.True(record.GetProperty("measurementComplete").GetBoolean());
                Assert.Equal(0, record.GetProperty("missingMeasurementCount").GetInt32());
                Assert.Equal(1, record.GetProperty("evaluationAttemptCount").GetInt32());
                Assert.StartsWith(
                    JsonlDesignConversionVisualCostLedger.RecordIdPrefix + "-",
                    record.GetProperty("recordId").GetString(),
                    StringComparison.Ordinal);
                Assert.StartsWith(
                    JsonlDesignConversionVisualCostLedger.EvaluationJobRefPrefix + "-",
                    record.GetProperty("evaluationJobRef").GetString(),
                    StringComparison.Ordinal);
                Assert.StartsWith(
                    JsonlDesignConversionVisualCostLedger.SubmittedRequestRefPrefix + "-",
                    record.GetProperty("submittedRequestRef").GetString(),
                    StringComparison.Ordinal);

                var metadata = root.GetProperty("boundaryMetadata");
                AssertTrustedAspNetRouteMetadata(
                    metadata,
                    DesignConversionModelServerEndpointRegistry.VisualCostReadbackEndpointPath,
                    nameof(DesignConversionModelServerVisualCostHandler));
                Assert.Equal(
                    DesignConversionModelServerVisualCostHandler.EndpointTemplatePath,
                    metadata.GetProperty("http_path").GetString());
                Assert.Equal(
                    DesignConversionModelServerVisualCostHandler.EndpointTemplatePath,
                    metadata.GetProperty("route_admission_path").GetString());
                Assert.Equal(bool.TrueString, metadata.GetProperty("runtime_visual_cost_records_readback_invoked").GetString());
                Assert.Equal(bool.TrueString, metadata.GetProperty("visual_cost_response_refs_only").GetString());
                Assert.Equal("1", metadata.GetProperty("visual_cost_record_count").GetString());
                Assert.Equal(
                    0.0600m,
                    decimal.Parse(metadata.GetProperty("visual_cost_total_cost_usd").GetString()!, CultureInfo.InvariantCulture));
                Assert.Equal(bool.FalseString, metadata.GetProperty("raw_route_job_id_included").GetString());
                Assert.Equal(bool.FalseString, metadata.GetProperty("raw_job_id_included").GetString());
                Assert.Equal(bool.FalseString, metadata.GetProperty("raw_request_id_included").GetString());
                Assert.Equal(bool.FalseString, metadata.GetProperty("provider_payload_included").GetString());
                Assert.Equal(bool.FalseString, metadata.GetProperty("filesystem_root_included").GetString());
                Assert.Equal(bool.FalseString, metadata.GetProperty("sensitive_principal_data_included").GetString());
                Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
                AssertVisualCostBoundaryMetadataHasNoIdentityRefs(metadata);
            }

            using (var repeatedRequest = CreateAspNetVisualCostReadbackAdmissionRequest(
                       visualRoute,
                       routeAdmissionCurrentToken))
            using (var repeatedResponse = await client.SendAsync(repeatedRequest))
            {
                var repeatedBodyJson = await repeatedResponse.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.OK, repeatedResponse.StatusCode);
                AssertNoStoreHeaders(repeatedResponse);
                using var repeatedDocument = JsonDocument.Parse(repeatedBodyJson);
                Assert.Equal(1, repeatedDocument.RootElement.GetProperty("summary").GetProperty("recordCount").GetInt32());
                Assert.Single(repeatedDocument.RootElement.GetProperty("records").EnumerateArray());
            }

            Assert.Equal(1, visualJudge.InvocationCount);
        }
        finally
        {
            await app.StopAsync();
            DeleteDirectoryIfExists(tempRoot);
        }
    }

    [Fact]
    public async Task GetVisualCosts_AspNetHostStartup_UnknownAdmissionAllowedJobReturnsEmptyWithoutOracle()
    {
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "design-conversion-aspnet-visual-cost-unknown");
        using var ledgerRootOverride = UseVisualCostLedgerRoot(Path.Combine(tempRoot, "visual-cost-ledger"));
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider);
        var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            composition,
            new UnexpectedCandidateStrategy(),
            artifactDb: null,
            utcNow: () => DateTimeOffset.UnixEpoch.AddMinutes(732));

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(registry);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };

            using var response = await client.GetAsync(VisualCostRoute("thin-job-visual-cost-unknown-allowed"));
            var bodyJson = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            AssertNoStoreHeaders(response);
            using var document = JsonDocument.Parse(bodyJson);
            var root = document.RootElement;
            Assert.Equal(0, root.GetProperty("summary").GetProperty("recordCount").GetInt32());
            Assert.Empty(root.GetProperty("records").EnumerateArray());
            Assert.Equal(0m, root.GetProperty("summary").GetProperty("totalCostUsd").GetDecimal());
            Assert.False(root.GetProperty("summary").GetProperty("measurementComplete").GetBoolean());
            Assert.Equal(
                DesignConversionVisualCostAccounting.UnknownCostSource,
                root.GetProperty("summary").GetProperty("costSource").GetString());

            var metadata = root.GetProperty("boundaryMetadata");
            AssertTrustedAspNetRouteMetadata(
                metadata,
                DesignConversionModelServerEndpointRegistry.VisualCostReadbackEndpointPath,
                nameof(DesignConversionModelServerVisualCostHandler));
            Assert.Equal(bool.TrueString, metadata.GetProperty("visual_cost_unknown_job_empty_success").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
        }
        finally
        {
            await app.StopAsync();
            DeleteDirectoryIfExists(tempRoot);
        }
    }

    [Fact]
    public async Task GetVisualCosts_AspNetHostStartup_AdmissionDenialsDoNotReadCorruptLedger()
    {
        const string routeAdmissionCurrentToken = "aspnet-visual-cost-denial-route-token-0123456789";
        const string routeAdmissionTenantId = "tenant-aspnet-visual-cost-denial";
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "design-conversion-aspnet-visual-cost-denial");
        var ledgerRoot = Path.Combine(tempRoot, "visual-cost-ledger");
        Directory.CreateDirectory(ledgerRoot);
        await File.WriteAllTextAsync(
            Path.Combine(ledgerRoot, JsonlDesignConversionVisualCostLedger.RecordsFileName),
            "{ not-json");
        using var ledgerRootOverride = UseVisualCostLedgerRoot(ledgerRoot);
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider);
        var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            composition,
            new UnexpectedCandidateStrategy(),
            artifactDb: null,
            utcNow: () => DateTimeOffset.UnixEpoch.AddMinutes(733),
            routeAdmissionOptions: new DesignConversionModelServerRouteAdmissionOptions(
                enabled: true,
                currentToken: routeAdmissionCurrentToken,
                tenantId: routeAdmissionTenantId));

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(registry);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };
            var deniedRoute = VisualCostRoute("thin-job-visual-cost-denied");
            var malformedDeniedRoute =
                DesignConversionModelServerVisualCostHandler.EndpointPathPrefix +
                "%20" +
                DesignConversionModelServerVisualCostHandler.EndpointPathSuffix;

            using (var missingAuthRequest = CreateAspNetVisualCostReadbackAdmissionRequest(
                       deniedRoute,
                       token: null))
            using (var missingAuthResponse = await client.SendAsync(missingAuthRequest))
            {
                var bodyJson = await missingAuthResponse.Content.ReadAsStringAsync();
                AssertVisualCostError(
                    missingAuthResponse,
                    bodyJson,
                    HttpStatusCode.Unauthorized,
                    "DESIGN_CONVERSION_ROUTE_ADMISSION_TOKEN_REQUIRED",
                    expectedRuntimeReadbackInvoked: false);
                AssertVisualCostDenialDidNotReadCorruptLedger(
                    bodyJson,
                    ledgerRoot,
                    routeAdmissionCurrentToken,
                    routeAdmissionTenantId);
            }

            using (var missingAuthMalformedRequest = CreateAspNetVisualCostReadbackAdmissionRequest(
                       malformedDeniedRoute,
                       token: null))
            using (var missingAuthMalformedResponse = await client.SendAsync(missingAuthMalformedRequest))
            {
                var bodyJson = await missingAuthMalformedResponse.Content.ReadAsStringAsync();
                AssertVisualCostError(
                    missingAuthMalformedResponse,
                    bodyJson,
                    HttpStatusCode.Unauthorized,
                    "DESIGN_CONVERSION_ROUTE_ADMISSION_TOKEN_REQUIRED",
                    expectedRuntimeReadbackInvoked: false);
                Assert.DoesNotContain("JOB_ID_", bodyJson, StringComparison.Ordinal);
                AssertVisualCostDenialDidNotReadCorruptLedger(
                    bodyJson,
                    ledgerRoot,
                    routeAdmissionCurrentToken,
                    routeAdmissionTenantId);
            }

            using (var invalidAuthRequest = CreateAspNetVisualCostReadbackAdmissionRequest(
                       deniedRoute,
                       token: "wrong-visual-cost-token-0123456789"))
            using (var invalidAuthResponse = await client.SendAsync(invalidAuthRequest))
            {
                var bodyJson = await invalidAuthResponse.Content.ReadAsStringAsync();
                AssertVisualCostError(
                    invalidAuthResponse,
                    bodyJson,
                    HttpStatusCode.Forbidden,
                    "DESIGN_CONVERSION_ROUTE_ADMISSION_TOKEN_INVALID",
                    expectedRuntimeReadbackInvoked: false);
                AssertVisualCostDenialDidNotReadCorruptLedger(
                    bodyJson,
                    ledgerRoot,
                    routeAdmissionCurrentToken,
                    routeAdmissionTenantId);
            }

            using (var unboundJobRequest = CreateAspNetVisualCostReadbackAdmissionRequest(
                       deniedRoute,
                       routeAdmissionCurrentToken))
            using (var unboundJobResponse = await client.SendAsync(unboundJobRequest))
            {
                var bodyJson = await unboundJobResponse.Content.ReadAsStringAsync();
                AssertVisualCostError(
                    unboundJobResponse,
                    bodyJson,
                    HttpStatusCode.NotFound,
                    "JOB_NOT_FOUND",
                    expectedRuntimeReadbackInvoked: false);
                AssertVisualCostDenialDidNotReadCorruptLedger(
                    bodyJson,
                    ledgerRoot,
                    routeAdmissionCurrentToken,
                    routeAdmissionTenantId);
            }

            Assert.Empty(browserAdapter.Requests);
            Assert.Empty(comparisonProvider.Requests);
        }
        finally
        {
            await app.StopAsync();
            DeleteDirectoryIfExists(tempRoot);
        }
    }

    [Fact]
    public async Task GetVisualCosts_AspNetHostStartup_CrossTenantAllowedTokenDeniesBeforeCorruptLedgerRead()
    {
        const string alphaToken = "aspnet-visual-cost-cross-tenant-alpha-token-0123456789";
        const string betaToken = "aspnet-visual-cost-cross-tenant-beta-token-0123456789";
        const string alphaTenantId = "tenant-aspnet-visual-cost-alpha-cross";
        const string betaTenantId = "tenant-aspnet-visual-cost-beta-cross";
        const string alphaPrivateMarker = "PRIVATE_VISUAL_COST_CROSS_TENANT_ALPHA";
        const string alphaTrustedVisualCostUserId = "platform-user-visual-cost-alpha-cross";
        const string alphaTrustedVisualCostTier = "paid";
        const string alphaTrustedVisualCostMonthlyCap = "125.75";
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "design-conversion-aspnet-visual-cost-cross-tenant");
        var artifactDbRoot = Path.Combine(tempRoot, "artifact-db");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
        var ledgerRoot = Path.Combine(tempRoot, "visual-cost-ledger");
        using var ledgerRootOverride = UseVisualCostLedgerRoot(ledgerRoot);
        var alphaVisualCostBudgetStoreRoot = Path.Combine(tempRoot, "alpha-visual-cost-budget-store");
        var betaVisualCostBudgetStoreRoot = Path.Combine(tempRoot, "beta-visual-cost-budget-store");

        try
        {
            var alphaActivationStateRoot = Path.Combine(tempRoot, "alpha-activation-state");
            var alphaUtcNow = () => DateTimeOffset.UnixEpoch.AddMinutes(736);
            var alphaActiveCandidate =
                await DesignConversionActivatedModelRepositoryHostRuntimeTests.BuildActivationRecordAsync(
                    artifactDbRoot,
                    alphaTenantId,
                    "aspnet-visual-cost-cross-tenant-active",
                    batchCount: 1);
            var alphaActivationRegistry = new DesignConversionModelActivationRegistry(
                alphaActivationStateRoot,
                alphaUtcNow,
                allowedArtifactRootDirectory: artifactDbRoot);
            var alphaActivation = await alphaActivationRegistry.ActivateAsync(
                new DesignConversionModelActivationCommand(
                    alphaTenantId,
                    RequestedByRef: "architect:aspnet-visual-cost-cross-tenant",
                    Reason: "pre-seed a valid tenant active model before cross-tenant ledger evaluation",
                    IdempotencyKey: "activate:aspnet-visual-cost-cross-tenant",
                    ExpectedGeneration: 0,
                    Candidate: alphaActiveCandidate,
                    ExpectedModelVersion: alphaActiveCandidate.Model.Version,
                    RequestedAtUtc: alphaUtcNow()));
            Assert.True(alphaActivation.IsSuccess, alphaActivation.ErrorMessage);
            var alphaBrowserAdapter = new RecordingBrowserAdapter();
            var alphaComparisonProvider = new RecordingComparisonProvider(score: 0.96);
            var alphaComposition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
                alphaBrowserAdapter,
                alphaComparisonProvider,
                new AspNetVisualCostReceiptJudge(0.0400m));
            var alphaRegistry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
                alphaComposition,
                new UnexpectedCandidateStrategy(),
                artifactDb,
                alphaUtcNow,
                activationStateRootDirectory: alphaActivationStateRoot,
                modelLifecycleAuthToken: "aspnet-visual-cost-cross-tenant-lifecycle-token-0123456789",
                routeAdmissionOptions: new DesignConversionModelServerRouteAdmissionOptions(
                    enabled: true,
                    currentToken: alphaToken,
                    tenantId: alphaTenantId),
                visualCostBudgetStore: new JsonlDesignConversionVisualCostBudgetStore(alphaVisualCostBudgetStoreRoot));
            var alphaBuilder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = []
            });
            alphaBuilder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

            string jobId;
            await using (var alphaApp = alphaBuilder.Build())
            {
                alphaApp.ConfigureDesignConversionModelServer(alphaRegistry);
                await alphaApp.StartAsync();
                try
                {
                    using var alphaClient = new HttpClient
                    {
                        BaseAddress = ResolveStartedAddress(alphaApp),
                        Timeout = TimeSpan.FromSeconds(5)
                    };
                    var baseEvaluateRequest = CreateEvaluateRequest(
                        "visual-cost-cross-tenant-alpha-request",
                        alphaPrivateMarker);
                    var evaluateRequest = baseEvaluateRequest with
                    {
                        Budget = baseEvaluateRequest.Budget! with
                        {
                            UserId = alphaTrustedVisualCostUserId,
                            Tier = alphaTrustedVisualCostTier,
                            MaxImprovementCandidates = 1,
                            MaxRenderQaRuns = 1,
                            BudgetUsd = 0.25m,
                            Metadata = new Dictionary<string, string>()
                        },
                        Metadata = new Dictionary<string, string>(
                            baseEvaluateRequest.Metadata ?? new Dictionary<string, string>(),
                            StringComparer.Ordinal)
                        {
                            ["tenant_id"] = alphaTenantId,
                            [DesignConversionModelServerApi.ServerAssignedDesignTierMetadataKey] =
                                alphaTrustedVisualCostTier,
                            [DesignConversionModelServerApi.ServerAssignedMonthlyVisualCostLimitUsdMetadataKey] =
                                alphaTrustedVisualCostMonthlyCap,
                            [DesignConversionModelServerApi.DesignTierAuthorityMetadataKey] =
                                DesignConversionModelServerApi.PlatformTenantConfigurationAuthority,
                            [DesignConversionModelServerApi.MonthlyVisualCostLimitAuthorityMetadataKey] =
                                DesignConversionModelServerApi.PlatformTenantConfigurationAuthority,
                            [DesignConversionModelServerApi.UserIdentityAuthorityMetadataKey] =
                                DesignConversionModelServerApi.VerifiedInboundPrincipalAuthority
                        }
                    };
                    using var evaluate = CreateAspNetEvaluateAdmissionRequest(
                        JsonSerializer.Serialize(evaluateRequest, DesignConversionModelServerHttpHandler.JsonOptions),
                        alphaToken,
                        "visual-cost-cross-tenant-alpha-key");
                    using var evaluateResponse = await alphaClient.SendAsync(evaluate);
                    var evaluateBodyJson = await evaluateResponse.Content.ReadAsStringAsync();
                    Assert.Equal(HttpStatusCode.OK, evaluateResponse.StatusCode);
                    Assert.DoesNotContain(alphaPrivateMarker, evaluateBodyJson, StringComparison.Ordinal);
                    Assert.DoesNotContain(alphaTrustedVisualCostUserId, evaluateBodyJson, StringComparison.Ordinal);
                    Assert.DoesNotContain(alphaToken, evaluateBodyJson, StringComparison.Ordinal);
                    Assert.DoesNotContain(alphaTenantId, evaluateBodyJson, StringComparison.Ordinal);
                    Assert.DoesNotContain(alphaVisualCostBudgetStoreRoot, evaluateBodyJson, StringComparison.OrdinalIgnoreCase);
                    using var evaluateDocument = JsonDocument.Parse(evaluateBodyJson);
                    var evaluateMetadata = evaluateDocument.RootElement.GetProperty("boundaryMetadata");
                    Assert.Equal(bool.TrueString, evaluateMetadata.GetProperty("trusted_visual_cost_entitlement_bound").GetString());
                    Assert.Equal(
                        alphaTrustedVisualCostTier,
                        evaluateMetadata.GetProperty("trusted_visual_cost_entitlement_design_tier").GetString());
                    Assert.Equal(
                        alphaTrustedVisualCostMonthlyCap,
                        evaluateMetadata.GetProperty("trusted_visual_cost_entitlement_monthly_limit_usd").GetString());
                    Assert.Equal(bool.FalseString, evaluateMetadata.GetProperty("trusted_visual_cost_entitlement_raw_user_exposed").GetString());
                    Assert.Equal(bool.TrueString, evaluateMetadata.GetProperty("trusted_visual_cost_reservation_store_wired").GetString());

                    var records = await artifactDb.ListRecordsAsync(new DesignConversionLocalArtifactDbQuery(
                        RequestId: evaluateRequest.RequestId));
                    Assert.True(records.IsSuccess, records.ErrorMessage);
                    jobId = Assert.Single(records.Value!).JobId;
                }
                finally
                {
                    await alphaApp.StopAsync();
                }
            }

            Directory.CreateDirectory(ledgerRoot);
            await File.WriteAllTextAsync(
                Path.Combine(ledgerRoot, JsonlDesignConversionVisualCostLedger.RecordsFileName),
                "{ not-json");

            var betaBrowserAdapter = new RecordingBrowserAdapter();
            var betaComparisonProvider = new RecordingComparisonProvider(score: 0.96);
            var betaComposition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
                betaBrowserAdapter,
                betaComparisonProvider);
            var betaRegistry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
                betaComposition,
                new UnexpectedCandidateStrategy(),
                artifactDb: null,
                utcNow: () => DateTimeOffset.UnixEpoch.AddMinutes(737),
                routeAdmissionOptions: new DesignConversionModelServerRouteAdmissionOptions(
                    enabled: true,
                    currentToken: betaToken,
                    tenantId: betaTenantId),
                visualCostBudgetStore: new JsonlDesignConversionVisualCostBudgetStore(betaVisualCostBudgetStoreRoot));
            var betaBuilder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = []
            });
            betaBuilder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

            await using var betaApp = betaBuilder.Build();
            betaApp.ConfigureDesignConversionModelServer(betaRegistry);
            await betaApp.StartAsync();
            try
            {
                using var betaClient = new HttpClient
                {
                    BaseAddress = ResolveStartedAddress(betaApp),
                    Timeout = TimeSpan.FromSeconds(5)
                };
                using var crossTenantRequest = CreateAspNetVisualCostReadbackAdmissionRequest(
                    VisualCostRoute(jobId),
                    betaToken);
                using var crossTenantResponse = await betaClient.SendAsync(crossTenantRequest);
                var bodyJson = await crossTenantResponse.Content.ReadAsStringAsync();

                AssertVisualCostError(
                    crossTenantResponse,
                    bodyJson,
                    HttpStatusCode.NotFound,
                    "JOB_NOT_FOUND",
                    expectedRuntimeReadbackInvoked: false);
                AssertVisualCostDenialDidNotReadCorruptLedger(
                    bodyJson,
                    ledgerRoot,
                    alphaVisualCostBudgetStoreRoot,
                    betaVisualCostBudgetStoreRoot,
                    jobId,
                    alphaPrivateMarker,
                    alphaTrustedVisualCostUserId,
                    alphaToken,
                    betaToken,
                    alphaTenantId,
                    betaTenantId);
                Assert.Empty(betaBrowserAdapter.Requests);
                Assert.Empty(betaComparisonProvider.Requests);
            }
            finally
            {
                await betaApp.StopAsync();
            }
        }
        finally
        {
            DeleteDirectoryIfExists(tempRoot);
        }
    }

    [Fact]
    public async Task GetVisualCosts_AspNetHostStartup_CorruptLedgerReturnsSafe503AndNoStore()
    {
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "design-conversion-aspnet-visual-cost-corrupt");
        var ledgerRoot = Path.Combine(tempRoot, "visual-cost-ledger");
        Directory.CreateDirectory(ledgerRoot);
        await File.WriteAllTextAsync(
            Path.Combine(ledgerRoot, JsonlDesignConversionVisualCostLedger.RecordsFileName),
            "{ not-json");
        using var ledgerRootOverride = UseVisualCostLedgerRoot(ledgerRoot);
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider);
        var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            composition,
            new UnexpectedCandidateStrategy(),
            artifactDb: null,
            utcNow: () => DateTimeOffset.UnixEpoch.AddMinutes(734));

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(registry);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };
            const string jobId = "thin-job-visual-cost-corrupt-aspnet";

            using var response = await client.GetAsync(VisualCostRoute(jobId));
            var bodyJson = await response.Content.ReadAsStringAsync();

            AssertVisualCostError(
                response,
                bodyJson,
                HttpStatusCode.ServiceUnavailable,
                DesignConversionThinClientJobRuntime.VisualCostRecordsReadFailedErrorCode,
                expectedRuntimeReadbackInvoked: true);
            Assert.Contains(DesignConversionThinClientJobRuntime.VisualCostRecordsReadFailedSafeMessage, bodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(ledgerRoot, bodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(JsonlDesignConversionVisualCostLedger.RecordsFileName, bodyJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(jobId, bodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain("not-json", bodyJson, StringComparison.OrdinalIgnoreCase);

            using var document = JsonDocument.Parse(bodyJson);
            var metadata = document.RootElement.GetProperty("boundaryMetadata");
            Assert.Equal(bool.TrueString, metadata.GetProperty("runtime_visual_cost_records_readback_invoked").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("raw_route_job_id_included").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("filesystem_root_included").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
        }
        finally
        {
            await app.StopAsync();
            DeleteDirectoryIfExists(tempRoot);
        }
    }

    [Theory]
    [InlineData("%20")]
    [InlineData("bad%5Cjob")]
    public async Task GetVisualCosts_AspNetHostStartup_BlankOrMalformedJobReturnsBadRequestNoStore(
        string rawJobSegment)
    {
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "design-conversion-aspnet-visual-cost-invalid-job");
        using var ledgerRootOverride = UseVisualCostLedgerRoot(Path.Combine(tempRoot, "visual-cost-ledger"));
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider);
        var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            composition,
            new UnexpectedCandidateStrategy(),
            artifactDb: null,
            utcNow: () => DateTimeOffset.UnixEpoch.AddMinutes(735));

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(registry);

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
                DesignConversionModelServerVisualCostHandler.EndpointPathPrefix +
                rawJobSegment +
                DesignConversionModelServerVisualCostHandler.EndpointPathSuffix);
            var bodyJson = await response.Content.ReadAsStringAsync();

            AssertVisualCostBadRequestJobError(response, bodyJson);
            Assert.DoesNotContain(rawJobSegment, bodyJson, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            await app.StopAsync();
            DeleteDirectoryIfExists(tempRoot);
        }
    }

    [Fact]
    public async Task ConfigureDesignConversionModelServer_FullLocalRegistry_MapsEvaluateStatusDpoAndTrainingIntakeRoutes()
    {
        const string privateMarker = "PRIVATE_PASS_FULL_STARTUP_ROUTE_SET_SECRET";
        const string routeAdmissionToken = "full-startup-route-set-token-0123456789";
        const string routeAdmissionTenantId = "tenant-full-startup-route-set-alpha";
        const string modelLifecycleAuthToken = "full-startup-model-lifecycle-token-0123456789";
        const string trustedVisualCostUserId = "full-startup-route-set-user";
        const string trustedVisualCostTier = "paid";
        const string trustedVisualCostMonthlyCap = "125.75";
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "design-conversion-full-startup-route-set");
        var artifactDbRoot = Path.Combine(tempRoot, "artifact-db");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(artifactDbRoot);
        var activationStateRoot = Path.Combine(tempRoot, "activation-state");
        var serverOwnedArtifactRoot = Path.Combine(tempRoot, "server-owned-artifacts");
        var visualCostBudgetStoreRoot = Path.Combine(tempRoot, "visual-cost-budget-store");
        var ledgerRoot = Path.Combine(tempRoot, "visual-cost-ledger");
        using var ledgerRootOverride = UseVisualCostLedgerRoot(ledgerRoot);
        var visualCostBudgetStore = new JsonlDesignConversionVisualCostBudgetStore(visualCostBudgetStoreRoot);
        var activationRecord = await BuildAspNetHostActivationRecordAsync(
            artifactDbRoot,
            routeAdmissionTenantId,
            "full-startup-route-set-active-model");
        var activationRegistry = new DesignConversionModelActivationRegistry(
            activationStateRoot,
            () => DateTimeOffset.UnixEpoch.AddMinutes(300),
            allowedArtifactRootDirectory: artifactDbRoot);
        var activated = await activationRegistry.ActivateAsync(
            new DesignConversionModelActivationCommand(
                routeAdmissionTenantId,
                RequestedByRef: "aspnet-full-startup-route-set-test",
                Reason: "full local route set evaluate fixture activation",
                IdempotencyKey: "activate:full-startup-route-set",
                ExpectedGeneration: 0,
                Candidate: activationRecord,
                ExpectedModelVersion: activationRecord.Model.Version,
                RequestedAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(300)));
        Assert.True(activated.IsSuccess, activated.ErrorMessage);
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var visualJudge = new AspNetVisualCostReceiptJudge(0.0600m);
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            comparisonProvider,
            visualJudge);
        var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            composition,
            new UnexpectedCandidateStrategy(),
            artifactDb,
            () => DateTimeOffset.UnixEpoch.AddMinutes(301),
            activationStateRootDirectory: activationStateRoot,
            modelLifecycleAuthToken: modelLifecycleAuthToken,
            artifactRefResolver: new DesignConversionServerOwnedLocalArtifactRefResolver(serverOwnedArtifactRoot),
            routeAdmissionOptions: new DesignConversionModelServerRouteAdmissionOptions(
                enabled: true,
                currentToken: routeAdmissionToken,
                tenantId: routeAdmissionTenantId),
            visualCostBudgetStore: visualCostBudgetStore);

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        var startupBuilder = app.ConfigureDesignConversionModelServer(registry);
        Assert.NotNull(startupBuilder);

        await app.StartAsync();
        try
        {
            Assert.Equal(11, DesignConversionModelServerAspNetHostStartup.FullLocalRouteSet().Count);
            Assert.Contains(
                DesignConversionModelServerEndpointRegistry.Flow38QuarantineEndpointPath,
                DesignConversionModelServerAspNetHostStartup.FullLocalRouteSet());
            var startupMetadatas = app.Services
                .GetRequiredService<EndpointDataSource>()
                .Endpoints
                .Select(endpoint => endpoint.Metadata.GetMetadata<DesignConversionModelServerAspNetHostStartupMetadata>())
                .Where(metadata => metadata is not null)
                .Select(metadata => metadata!)
                .ToArray();
            Assert.Equal(DesignConversionModelServerAspNetHostStartup.FullLocalRouteSet().Count, startupMetadatas.Length);
            Assert.Equal(
                DesignConversionModelServerAspNetHostStartup.FullLocalRouteSet().Order(StringComparer.Ordinal),
                startupMetadatas.Select(metadata => metadata.MappedRoute).Order(StringComparer.Ordinal));
            Assert.All(startupMetadatas, metadata =>
            {
                Assert.Equal(DesignConversionModelServerAspNetHostStartup.FullLocalRouteSet(), metadata.MappedRoutes);
                Assert.True(metadata.LocalOnly);
                Assert.True(metadata.RefsOnly);
                Assert.False(metadata.PublicDeploymentVerified);
                Assert.False(metadata.CrossRepoCallerVerified);
                Assert.False(metadata.ExternalProviderConfigured);
                Assert.False(metadata.ProductReady);
            });

            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };

            var baseEvaluateRequest = CreateEvaluateRequest("pass-full-startup-evaluate-request", privateMarker);
            var evaluateRequest = WithTrustedVisualCostEntitlement(
                WithServerOwnedAspNetLocalArtifactProofs(baseEvaluateRequest, serverOwnedArtifactRoot),
                trustedVisualCostUserId,
                trustedVisualCostTier,
                trustedVisualCostMonthlyCap,
                routeAdmissionTenantId);
            using (var evaluateMessage = CreateAspNetEvaluateAdmissionRequest(
                       JsonSerializer.Serialize(evaluateRequest, DesignConversionModelServerHttpHandler.JsonOptions),
                       routeAdmissionToken,
                       "full-startup-route-set-evaluate-idempotency"))
            using (var evaluateResponse = await client.SendAsync(evaluateMessage))
            {
                var evaluateBodyJson = await evaluateResponse.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.OK, evaluateResponse.StatusCode);
                Assert.Equal(
                    DesignConversionModelServerEndpointRegistry.EndpointPath,
                    evaluateResponse.Headers.GetValues("x-design-conversion-asp-net-route").Single());
                Assert.Equal(
                    bool.FalseString,
                    evaluateResponse.Headers.GetValues("x-design-conversion-product-ready").Single());
                Assert.DoesNotContain(privateMarker, evaluateBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(evaluateRequest.Html, evaluateBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(evaluateRequest.Css, evaluateBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(evaluateRequest.DesignTree.Payload!, evaluateBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(evaluateRequest.OriginalTree.Payload!, evaluateBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(tempRoot, evaluateBodyJson, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(artifactDbRoot, evaluateBodyJson, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(ledgerRoot, evaluateBodyJson, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(visualCostBudgetStoreRoot, evaluateBodyJson, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(serverOwnedArtifactRoot, evaluateBodyJson, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(activationStateRoot, evaluateBodyJson, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(routeAdmissionToken, evaluateBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(routeAdmissionTenantId, evaluateBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(modelLifecycleAuthToken, evaluateBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(trustedVisualCostUserId, evaluateBodyJson, StringComparison.Ordinal);

                using var evaluateDocument = JsonDocument.Parse(evaluateBodyJson);
                var evaluateRoot = evaluateDocument.RootElement;
                Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointPath, evaluateRoot.GetProperty("endpoint").GetString());
                Assert.Equal("POST", evaluateRoot.GetProperty("method").GetString());
                Assert.Equal(DesignConversionModelServerHttpHandler.LocalHttpSeam, evaluateRoot.GetProperty("httpSeam").GetString());
                Assert.Equal(DesignConversionModelServerApi.ApiShape, evaluateRoot.GetProperty("apiShape").GetString());
                Assert.Equal(DesignConversionModelServerApi.RuntimeSurface, evaluateRoot.GetProperty("runtimeSurface").GetString());
                Assert.Equal(200, evaluateRoot.GetProperty("statusCode").GetInt32());
                Assert.Equal(DesignConversionThinClientStatus.Completed, evaluateRoot.GetProperty("contractStatus").GetString());
                Assert.Equal("pass-full-startup-evaluate-request", evaluateRoot.GetProperty("requestId").GetString());
                Assert.False(evaluateRoot.GetProperty("publicHttpEndpointVerified").GetBoolean());
                Assert.False(evaluateRoot.GetProperty("deploymentVerified").GetBoolean());
                Assert.False(evaluateRoot.GetProperty("productReady").GetBoolean());

                var evaluateMetadata = evaluateRoot.GetProperty("boundaryMetadata");
                Assert.Equal(
                    nameof(DesignConversionModelServerHttpHandler),
                    evaluateMetadata.GetProperty("asp_net_handler_dispatch_target").GetString());
                Assert.Equal(bool.TrueString, evaluateMetadata.GetProperty("asp_net_registry_dispatch_verified").GetString());
                Assert.Equal(bool.TrueString, evaluateMetadata.GetProperty("runtime_invoked").GetString());
                Assert.True(int.Parse(evaluateMetadata.GetProperty("render_qa_result_count").GetString()!) > 0);
                Assert.True(int.Parse(evaluateMetadata.GetProperty("comparison_artifact_count").GetString()!) > 0);
                Assert.Equal(bool.TrueString, evaluateMetadata.GetProperty("refs_only_public_response").GetString());
                Assert.Equal(bool.FalseString, evaluateMetadata.GetProperty("raw_html_included").GetString());
                Assert.Equal(bool.FalseString, evaluateMetadata.GetProperty("raw_css_included").GetString());
                Assert.Equal(bool.FalseString, evaluateMetadata.GetProperty("raw_design_tree_included").GetString());
                Assert.Equal(bool.FalseString, evaluateMetadata.GetProperty("response_contains_raw_private_data").GetString());
                Assert.Equal(bool.FalseString, evaluateMetadata.GetProperty("public_http_endpoint_verified").GetString());
                Assert.Equal(bool.FalseString, evaluateMetadata.GetProperty("deployment_verified").GetString());
                Assert.Equal(bool.FalseString, evaluateMetadata.GetProperty("public_production_default_ready").GetString());
                Assert.Equal(bool.FalseString, evaluateMetadata.GetProperty("product_ready").GetString());
            }

            Assert.NotEmpty(browserAdapter.Requests);
            Assert.NotEmpty(comparisonProvider.Requests);

            using (var dpoRequest = CreateAspNetStatusReadbackAdmissionRequest(
                       DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath +
                       "?includeBlocked=true&limit=10",
                       routeAdmissionToken))
            using (var dpoResponse = await client.SendAsync(dpoRequest))
            {
                var dpoBodyJson = await dpoResponse.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.OK, dpoResponse.StatusCode);
                Assert.Equal(
                    DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath,
                    dpoResponse.Headers.GetValues("x-design-conversion-asp-net-route").Single());
                Assert.Equal(
                    bool.FalseString,
                    dpoResponse.Headers.GetValues("x-design-conversion-product-ready").Single());
                Assert.DoesNotContain(privateMarker, dpoBodyJson, StringComparison.Ordinal);

                using var dpoDocument = JsonDocument.Parse(dpoBodyJson);
                var dpoRoot = dpoDocument.RootElement;
                Assert.Equal(DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath, dpoRoot.GetProperty("endpoint").GetString());
                Assert.Equal("GET", dpoRoot.GetProperty("method").GetString());
                Assert.Empty(dpoRoot.GetProperty("rows").EnumerateArray());
                Assert.Equal(0, dpoRoot.GetProperty("rowCount").GetInt32());
                Assert.False(dpoRoot.GetProperty("productReady").GetBoolean());

                var dpoMetadata = dpoRoot.GetProperty("boundaryMetadata");
                Assert.Equal(
                    nameof(DesignConversionModelServerDpoUsefulnessHandler),
                    dpoMetadata.GetProperty("asp_net_handler_dispatch_target").GetString());
                Assert.Equal(bool.TrueString, dpoMetadata.GetProperty("dpo_usefulness_response_refs_only").GetString());
                Assert.Equal("0", dpoMetadata.GetProperty("dpo_usefulness_row_count").GetString());
                Assert.Equal(bool.FalseString, dpoMetadata.GetProperty("public_http_endpoint_verified").GetString());
                Assert.Equal(bool.FalseString, dpoMetadata.GetProperty("deployment_verified").GetString());
                Assert.Equal(bool.FalseString, dpoMetadata.GetProperty("product_ready").GetString());
            }

            using (var trainingContent = new StringContent(
                       JsonSerializer.Serialize(new
                       {
                           action = "list",
                           metadata = new Dictionary<string, string>
                           {
                               ["private_marker"] = privateMarker
                           }
                       }),
                       Encoding.UTF8,
                       "application/json"))
            using (var trainingResponse = await client.PostAsync(
                       DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                       trainingContent))
            {
                var trainingBodyJson = await trainingResponse.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.OK, trainingResponse.StatusCode);
                Assert.Equal(
                    DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath,
                    trainingResponse.Headers.GetValues("x-design-conversion-asp-net-route").Single());
                Assert.Equal(
                    bool.FalseString,
                    trainingResponse.Headers.GetValues("x-design-conversion-product-ready").Single());
                Assert.DoesNotContain(privateMarker, trainingBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain("<main", trainingBodyJson, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("raw_provider_payload", trainingBodyJson, StringComparison.OrdinalIgnoreCase);

                using var trainingDocument = JsonDocument.Parse(trainingBodyJson);
                var trainingRoot = trainingDocument.RootElement;
                Assert.Equal(DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath, trainingRoot.GetProperty("endpoint").GetString());
                Assert.Equal("POST", trainingRoot.GetProperty("method").GetString());
                Assert.Equal("list", trainingRoot.GetProperty("action").GetString());
                Assert.Equal(0, trainingRoot.GetProperty("intakeCount").GetInt32());
                Assert.False(trainingRoot.GetProperty("productReady").GetBoolean());
                Assert.False(trainingRoot.GetProperty("deploymentVerified").GetBoolean());
                Assert.False(trainingRoot.GetProperty("publicHttpEndpointVerified").GetBoolean());

                var trainingMetadata = trainingRoot.GetProperty("boundaryMetadata");
                Assert.Equal(
                    nameof(DesignConversionModelServerTrainingIntakeHandler),
                    trainingMetadata.GetProperty("asp_net_handler_dispatch_target").GetString());
                Assert.Equal(bool.TrueString, trainingMetadata.GetProperty("training_intake_response_refs_only").GetString());
                Assert.Equal(bool.FalseString, trainingMetadata.GetProperty("raw_html_included").GetString());
                Assert.Equal(bool.FalseString, trainingMetadata.GetProperty("raw_css_included").GetString());
                Assert.Equal(bool.FalseString, trainingMetadata.GetProperty("raw_design_tree_included").GetString());
                Assert.Equal(bool.FalseString, trainingMetadata.GetProperty("response_contains_raw_private_data").GetString());
                Assert.Equal(bool.FalseString, trainingMetadata.GetProperty("product_ready").GetString());
            }

            using (var statusRequest = CreateAspNetStatusReadbackAdmissionRequest(
                       DesignConversionModelServerStatusHandler.EndpointPathPrefix + "thin-job-full-startup-missing",
                       routeAdmissionToken))
            using (var statusResponse = await client.SendAsync(statusRequest))
            {
                var statusBodyJson = await statusResponse.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.NotFound, statusResponse.StatusCode);
                Assert.Equal(
                    DesignConversionModelServerEndpointRegistry.StatusReadbackEndpointPath,
                    statusResponse.Headers.GetValues("x-design-conversion-asp-net-route").Single());
                Assert.Equal(
                    bool.FalseString,
                    statusResponse.Headers.GetValues("x-design-conversion-product-ready").Single());
                Assert.DoesNotContain(privateMarker, statusBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(routeAdmissionToken, statusBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(routeAdmissionTenantId, statusBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(modelLifecycleAuthToken, statusBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(trustedVisualCostUserId, statusBodyJson, StringComparison.Ordinal);
                Assert.DoesNotContain(tempRoot, statusBodyJson, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(artifactDbRoot, statusBodyJson, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(visualCostBudgetStoreRoot, statusBodyJson, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(serverOwnedArtifactRoot, statusBodyJson, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(activationStateRoot, statusBodyJson, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(ledgerRoot, statusBodyJson, StringComparison.OrdinalIgnoreCase);

                using var statusDocument = JsonDocument.Parse(statusBodyJson);
                var statusRoot = statusDocument.RootElement;
                Assert.Equal(DesignConversionModelServerStatusHandler.EndpointTemplatePath, statusRoot.GetProperty("endpoint").GetString());
                Assert.Equal("GET", statusRoot.GetProperty("method").GetString());
                Assert.Equal("JOB_NOT_FOUND", statusRoot.GetProperty("errorCode").GetString());

                var statusMetadata = statusRoot.GetProperty("boundaryMetadata");
                Assert.Equal(
                    nameof(DesignConversionModelServerStatusHandler),
                    statusMetadata.GetProperty("asp_net_handler_dispatch_target").GetString());
                Assert.Equal(bool.TrueString, statusMetadata.GetProperty("status_readback_response_refs_only").GetString());
                Assert.Equal(bool.FalseString, statusMetadata.GetProperty("public_http_endpoint_verified").GetString());
                Assert.Equal(bool.FalseString, statusMetadata.GetProperty("deployment_verified").GetString());
                Assert.Equal(bool.FalseString, statusMetadata.GetProperty("product_ready").GetString());
            }
        }
        finally
        {
            await app.StopAsync();
            DeleteDirectoryIfExists(tempRoot);
        }
    }

    [Fact]
    public async Task ConfigureDesignConversionModelServer_WithArtifactUploadHandler_MapsFullRoutesAndStoresRawPng()
    {
        const string uploadToken = "full-startup-upload-token-0123456789";
        const string tenantId = "tenant-full-startup-upload-alpha";
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "design-conversion-full-startup-artifact-upload");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(Path.Combine(tempRoot, "artifact-db"));
        var uploadRoot = Path.Combine(tempRoot, "artifact-upload-root");
        var ledgerRoot = Path.Combine(tempRoot, "visual-cost-ledger");
        using var ledgerRootOverride = UseVisualCostLedgerRoot(ledgerRoot);
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            new RecordingBrowserAdapter(),
            new RecordingComparisonProvider(score: 0.96));
        var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            composition,
            new UnexpectedCandidateStrategy(),
            artifactDb,
            () => DateTimeOffset.UnixEpoch.AddMinutes(303));
        var uploadHandler = new DesignConversionModelServerArtifactUploadHttpHandler(
            new DesignConversionModelServerArtifactUploadStore(uploadRoot, maxBytes: 1024 * 1024),
            DesignConversionModelServerRouteAdmission.Create(
                new DesignConversionModelServerRouteAdmissionOptions(
                    enabled: true,
                    currentToken: uploadToken,
                    tenantId: tenantId),
                () => DateTimeOffset.UnixEpoch.AddMinutes(303)));
        var expectedRoutes = DesignConversionModelServerAspNetHostStartup.FullLocalRouteSet()
            .Concat([DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath])
            .ToArray();

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        Assert.Equal(11, DesignConversionModelServerAspNetHostStartup.FullLocalRouteSet().Count);
        Assert.Equal(
            "artifactUploadHandler",
            Assert.Throws<ArgumentNullException>(() =>
                app.ConfigureDesignConversionModelServer(
                    registry,
                    (DesignConversionModelServerArtifactUploadHttpHandler)null!)).ParamName);
        Assert.Equal(
            "artifactUploadHandler",
            Assert.Throws<ArgumentNullException>(() =>
                ((IEndpointRouteBuilder)app).ConfigureDesignConversionModelServer(
                    registry,
                    (DesignConversionModelServerArtifactUploadHttpHandler)null!)).ParamName);
        var startupBuilder = app.ConfigureDesignConversionModelServer(registry, uploadHandler);
        Assert.NotNull(startupBuilder);

        await app.StartAsync();
        try
        {
            var startupMetadatas = app.Services
                .GetRequiredService<EndpointDataSource>()
                .Endpoints
                .Select(endpoint => endpoint.Metadata.GetMetadata<DesignConversionModelServerAspNetHostStartupMetadata>())
                .Where(metadata => metadata is not null)
                .Select(metadata => metadata!)
                .ToArray();
            Assert.Equal(12, startupMetadatas.Length);
            Assert.Equal(
                expectedRoutes.Order(StringComparer.Ordinal),
                startupMetadatas.Select(metadata => metadata.MappedRoute).Order(StringComparer.Ordinal));
            Assert.Single(startupMetadatas, metadata => string.Equals(
                metadata.MappedRoute,
                DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath,
                StringComparison.Ordinal));
            Assert.All(startupMetadatas, metadata =>
            {
                Assert.Equal(expectedRoutes, metadata.MappedRoutes);
                Assert.Equal(1, metadata.MappedRoutes.Count(route => string.Equals(
                    route,
                    DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath,
                    StringComparison.Ordinal)));
                Assert.True(metadata.LocalOnly);
                Assert.True(metadata.RefsOnly);
                Assert.False(metadata.PublicDeploymentVerified);
                Assert.False(metadata.CrossRepoCallerVerified);
                Assert.False(metadata.ProductReady);
            });

            var uploadEndpoint = app.Services
                .GetRequiredService<EndpointDataSource>()
                .Endpoints
                .OfType<RouteEndpoint>()
                .Single(endpoint => string.Equals(
                    '/' + (endpoint.RoutePattern.RawText ?? string.Empty).TrimStart('/'),
                    DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath,
                    StringComparison.Ordinal));
            Assert.Equal(
                [HttpMethod.Post.Method],
                uploadEndpoint.Metadata.GetMetadata<HttpMethodMetadata>()!.HttpMethods);
            var uploadMetadata = uploadEndpoint.Metadata.GetMetadata<DesignConversionModelServerAspNetHostStartupMetadata>();
            Assert.NotNull(uploadMetadata);
            Assert.Equal(DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath, uploadMetadata!.MappedRoute);
            Assert.Equal(expectedRoutes, uploadMetadata.MappedRoutes);

            var png = BuildFixturePng(18, 14, Sha256Text("full-startup-artifact-upload"));
            var sha = $"sha256:{Sha256Bytes(png)}";
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(10)
            };
            HttpRequestMessage BuildUploadRequest(string viewportId)
            {
                var content = new ByteArrayContent(png);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                var uploadRequest = new HttpRequestMessage(
                    HttpMethod.Post,
                    DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath)
                {
                    Content = content
                };
                uploadRequest.Headers.TryAddWithoutValidation(
                    DesignConversionModelServerRouteAdmission.AuthorizationHeaderName,
                    "Bearer " + uploadToken);
                uploadRequest.Headers.TryAddWithoutValidation(
                    DesignConversionModelServerRouteAdmission.IdempotencyHeaderName,
                    "full-startup-upload-key");
                uploadRequest.Headers.TryAddWithoutValidation(
                    DesignConversionModelServerArtifactUploadHttpHandler.ArtifactIdHeaderName,
                    "full-startup-upload-artifact");
                uploadRequest.Headers.TryAddWithoutValidation(
                    DesignConversionModelServerArtifactUploadHttpHandler.ArtifactKindHeaderName,
                    "screenshot");
                uploadRequest.Headers.TryAddWithoutValidation(
                    DesignConversionModelServerArtifactUploadHttpHandler.ExpectedSha256HeaderName,
                    sha);
                uploadRequest.Headers.TryAddWithoutValidation(
                    DesignConversionModelServerArtifactUploadHttpHandler.WidthHeaderName,
                    "9");
                uploadRequest.Headers.TryAddWithoutValidation(
                    DesignConversionModelServerArtifactUploadHttpHandler.HeightHeaderName,
                    "7");
                uploadRequest.Headers.TryAddWithoutValidation(
                    DesignConversionModelServerArtifactUploadHttpHandler.ViewportWidthHeaderName,
                    "9");
                uploadRequest.Headers.TryAddWithoutValidation(
                    DesignConversionModelServerArtifactUploadHttpHandler.ViewportHeightHeaderName,
                    "7");
                uploadRequest.Headers.TryAddWithoutValidation(
                    DesignConversionModelServerArtifactUploadHttpHandler.PngWidthHeaderName,
                    "18");
                uploadRequest.Headers.TryAddWithoutValidation(
                    DesignConversionModelServerArtifactUploadHttpHandler.PngHeightHeaderName,
                    "14");
                uploadRequest.Headers.TryAddWithoutValidation(
                    DesignConversionModelServerArtifactUploadHttpHandler.DeviceScaleFactorHeaderName,
                    "2");
                uploadRequest.Headers.TryAddWithoutValidation(
                    DesignConversionModelServerArtifactUploadHttpHandler.ViewportIdHeaderName,
                    viewportId);
                return uploadRequest;
            }

            using var request = BuildUploadRequest("source-9x7@2~full-startup");
            using var response = await client.SendAsync(request);
            var bodyJson = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
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
                bool.FalseString,
                response.Headers.GetValues("x-design-conversion-product-ready").Single());

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
            Assert.Equal(sha, body.GetProperty("sha256").GetString());
            Assert.Equal("full-startup-upload-artifact", body.GetProperty("artifactId").GetString());
            Assert.Equal("screenshot", body.GetProperty("kind").GetString());
            Assert.Equal(9, body.GetProperty("width").GetInt32());
            Assert.Equal(7, body.GetProperty("height").GetInt32());

            var artifactRef = JsonSerializer.Deserialize<DesignConversionModelServerArtifactRef>(
                body.GetProperty("artifactRef").GetRawText(),
                DesignConversionModelServerHttpHandler.JsonOptions);
            Assert.NotNull(artifactRef);
            var resolver = new DesignConversionServerOwnedLocalArtifactRefResolver(uploadRoot);
            var resolved = resolver.Validate(artifactRef!, "full startup artifact upload");
            Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
            Assert.Equal(sha, artifactRef!.Sha256);
            Assert.NotNull(artifactRef.Metadata);
            var artifactMetadata = artifactRef.Metadata!;
            Assert.Equal("9", artifactMetadata["viewport_width"]);
            Assert.Equal("7", artifactMetadata["viewport_height"]);
            Assert.Equal("18", artifactMetadata["png_width"]);
            Assert.Equal("14", artifactMetadata["png_height"]);
            Assert.Equal("2", artifactMetadata["device_scale_factor"]);
            Assert.Equal("source-9x7@2~full-startup", artifactMetadata["viewport_id"]);

            using var conflictRequest = BuildUploadRequest("source-9x7@2~full-startup-conflict");
            using var conflictResponse = await client.SendAsync(conflictRequest);
            var conflictJson = await conflictResponse.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.Conflict, conflictResponse.StatusCode);
            using var conflictDocument = JsonDocument.Parse(conflictJson);
            Assert.Equal(
                "DESIGN_CONVERSION_IDEMPOTENCY_KEY_CONFLICT",
                conflictDocument.RootElement.GetProperty("errorCode").GetString());

            var publishedPngs = Directory.Exists(uploadRoot)
                ? Directory.EnumerateFiles(uploadRoot, "*.png", SearchOption.AllDirectories).ToArray()
                : [];
            Assert.Single(publishedPngs);
            Assert.Equal(png, await File.ReadAllBytesAsync(publishedPngs[0]));
            Assert.Empty(Directory.Exists(uploadRoot)
                ? Directory.EnumerateFiles(uploadRoot, "*.tmp", SearchOption.AllDirectories).ToArray()
                : []);
            Assert.Empty(Directory.Exists(uploadRoot)
                ? Directory.EnumerateDirectories(uploadRoot, "*staging*", SearchOption.AllDirectories).ToArray()
                : []);

            var boundaryMetadata = body.GetProperty("boundaryMetadata");
            Assert.Equal(
                DesignConversionModelServerArtifactUploadHttpHandler.EndpointPath,
                boundaryMetadata.GetProperty("asp_net_route_path").GetString());
            Assert.Equal(
                nameof(DesignConversionModelServerArtifactUploadHttpHandler),
                boundaryMetadata.GetProperty("asp_net_handler_dispatch_target").GetString());
            Assert.Equal(bool.TrueString, boundaryMetadata.GetProperty("asp_net_route_verified").GetString());
            Assert.Equal(bool.TrueString, boundaryMetadata.GetProperty("live_http_route_registered").GetString());
            Assert.Equal(bool.TrueString, boundaryMetadata.GetProperty("aspnet_endpoint_mapped").GetString());
            Assert.Equal(bool.TrueString, boundaryMetadata.GetProperty("upload_store_invoked").GetString());
            Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("raw_bytes_included").GetString());
            Assert.Equal(bool.FalseString, boundaryMetadata.GetProperty("product_ready").GetString());
        }
        finally
        {
            await app.StopAsync();
            DeleteDirectoryIfExists(tempRoot);
        }
    }

    [Fact]
    public async Task ConfigureDesignConversionModelServer_ExternalHttpCompositionIsRejectedBeforeAppOrProviderCall()
    {
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "design-conversion-aspnet-external-provider-authority-rejection");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(Path.Combine(tempRoot, "artifact-db"));
        var visualArtifactRoot = Path.Combine(tempRoot, "visual-artifacts");
        Directory.CreateDirectory(visualArtifactRoot);
        var handler = new EchoingRealVisualJudgeHttpHandler();
        var input = new DesignConversionExternalVisualProviderConfigInput(
            Endpoint: "https://visual-judge.example/v1/judge",
            ProviderName: "stale-external-provider",
            ModelId: "stale-external-model",
            ApiKey: "test-only-never-sent",
            ApiKeyHeaderName: "X-Visual-Judge-Key",
            ApiKeyScheme: string.Empty,
            ServerOwnedCostPerRequestUsd: "0.0125",
            ServerOwnedImageArtifactRoot: visualArtifactRoot);
        var comparisonProvider =
            DesignConversionExternalVisualProviderConfigIntake.TryCreateComparisonProvider(
                input,
                handler);
        var visualJudge =
            DesignConversionExternalVisualProviderConfigIntake.TryCreateVisualJudge(
                input,
                handler);
        Assert.True(comparisonProvider.IsSuccess, comparisonProvider.ErrorMessage);
        Assert.True(visualJudge.IsSuccess, visualJudge.ErrorMessage);

        var browserAdapter = new RecordingBrowserAdapter(visualArtifactRoot);
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocal(
            browserAdapter,
            comparisonProvider.Value!,
            visualJudge.Value);

        try
        {
            var directCoordinator = new DesignConversionImprovementLoopCoordinator(
                composition.Service,
                new UnexpectedCandidateStrategy(),
                artifactDb);
            var directImprovementLoopHandler =
                new DesignConversionModelServerImprovementLoopHandler(directCoordinator);
            var directRegistry = new DesignConversionModelServerEndpointRegistry();
            var directRegistration = directRegistry.RegisterDesignConversionImprovementLoop(
                directImprovementLoopHandler);
            Assert.False(directRegistration.IsSuccess);
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry
                    .ServerOwnedLocalCompositionAuthorityRequiredErrorCode,
                directRegistration.ErrorCode);
            Assert.Equal(0, directRegistry.RegisteredRouteCount);

            var exception = Assert.Throws<DesignConversionModelServerCompositionAuthorityException>(
                () => DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
                    composition,
                    new UnexpectedCandidateStrategy(),
                    artifactDb,
                    () => DateTimeOffset.UnixEpoch.AddMinutes(371)));

            Assert.Equal(
                DesignConversionModelServerEndpointRegistry
                    .ServerOwnedLocalCompositionAuthorityRequiredErrorCode,
                exception.ErrorCode);
            Assert.Equal(
                DesignConversionModelServerEndpointRegistry
                    .ServerOwnedLocalCompositionAuthorityRequiredErrorMessage,
                exception.Message);

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = []
            });
            await using var app = builder.Build();
            var routeBuilder = (IEndpointRouteBuilder)app;
            var mappedEndpointCountBefore = routeBuilder.DataSources
                .SelectMany(dataSource => dataSource.Endpoints)
                .Count();
            var directMapperException = Assert.Throws<DesignConversionModelServerCompositionAuthorityException>(
                () => app.MapDesignConversionModelServerImprovementLoop(directRegistry));
            Assert.Equal(directRegistration.ErrorCode, directMapperException.ErrorCode);
            Assert.Equal(
                mappedEndpointCountBefore,
                routeBuilder.DataSources.SelectMany(dataSource => dataSource.Endpoints).Count());
            var directHostException = Assert.Throws<DesignConversionModelServerCompositionAuthorityException>(
                () => app.ConfigureDesignConversionModelServer(directRegistry));
            Assert.Equal(directRegistration.ErrorCode, directHostException.ErrorCode);
            Assert.Equal(
                mappedEndpointCountBefore,
                routeBuilder.DataSources.SelectMany(dataSource => dataSource.Endpoints).Count());
            app.Lifetime.StopApplication();
            Assert.False(directRegistry.IsDisposed);

            var mapperException = Assert.Throws<DesignConversionModelServerCompositionAuthorityException>(
                () => app.MapDesignConversionModelServerImprovementLoopFromComposition(
                    composition,
                    new UnexpectedCandidateStrategy()));
            Assert.Equal(exception.ErrorCode, mapperException.ErrorCode);
            Assert.Equal(
                mappedEndpointCountBefore,
                routeBuilder.DataSources.SelectMany(dataSource => dataSource.Endpoints).Count());
            Assert.Equal(0, handler.CallCount);
            Assert.Empty(browserAdapter.Requests);
        }
        finally
        {
            DeleteDirectoryIfExists(tempRoot);
        }
    }

    [Fact]
    public async Task MapDesignConversionModelServerImprovementLoop_ServerOwnedSafeFactoryCreatesAuthorizedRoute()
    {
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "design-conversion-authorized-direct-map");

        try
        {
            var composition = DesignConversionLocalServiceComposition
                .CreateServerOwnedDeterministicLocalPng(tempRoot);
            using var registry = new DesignConversionModelServerEndpointRegistry();
            var registered = registry.RegisterDesignConversionImprovementLoopFromComposition(
                composition,
                new UnexpectedCandidateStrategy());
            Assert.True(registered.IsSuccess, registered.ErrorMessage);
            Assert.True(registry.HasServerOwnedLocalImprovementLoopRegistrationAuthority);
            Assert.Equal(1, registry.RegisteredRouteCount);

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = []
            });
            await using var app = builder.Build();
            var routeBuilder = (IEndpointRouteBuilder)app;
            var mappedEndpointCountBefore = routeBuilder.DataSources
                .SelectMany(dataSource => dataSource.Endpoints)
                .Count();

            var endpoint = app.MapDesignConversionModelServerImprovementLoop(registry);

            Assert.NotNull(endpoint);
            Assert.True(
                routeBuilder.DataSources.SelectMany(dataSource => dataSource.Endpoints).Count() >
                mappedEndpointCountBefore);
        }
        finally
        {
            DeleteDirectoryIfExists(tempRoot);
        }
    }






    [Fact]
    public async Task PostImprovementLoop_AspNetHostStartup_OmittedSharedClientUsesFixedProviderAndDoesNotUseFakeHandler()
    {
        const string privateMarker = "PRIVATE_FIXED_VISUAL_PROVIDER_PATH_SECRET";
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "design-conversion-aspnet-fixed-visual-provider-path");
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(Path.Combine(tempRoot, "artifact-db"));
        using var ledgerRootOverride = UseVisualCostLedgerRoot(Path.Combine(tempRoot, "visual-cost-ledger"));
        var handler = new EchoingRealVisualJudgeHttpHandler();
        using var unusedExternalClient = new HttpClient(handler, disposeHandler: false)
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
        var browserAdapter = new RecordingBrowserAdapter();
        var fixedComparisonProvider = new RecordingComparisonProvider(score: 0.96);
        var candidateStrategy = new UnexpectedCandidateStrategy();
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalForLiveHostTesting(
            browserAdapter,
            fixedComparisonProvider);
        var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
            composition,
            candidateStrategy,
            artifactDb,
            () => DateTimeOffset.UnixEpoch.AddMinutes(433));

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = []
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.ConfigureDesignConversionModelServer(registry);

        await app.StartAsync();
        try
        {
            var baseAddress = ResolveStartedAddress(app);
            using var client = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(5)
            };

            var basePackage = CreatePackage("fixed-provider-loop-request", privateMarker);
            var package = basePackage with
            {
                Budget = basePackage.Budget with
                {
                    MaxImprovementCandidates = 1,
                    MaxRenderQaRuns = 2,
                    BudgetUsd = 1.00m
                }
            };
            var request = new DesignConversionModelServerImprovementLoopRequest(
                LoopId: "fixed-provider-loop",
                InitialPackage: package,
                MaxAttempts: 2,
                Metadata: new Dictionary<string, string>
                {
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

            Assert.True(
                response.StatusCode == HttpStatusCode.OK,
                $"Expected fixed-provider improvement-loop status {HttpStatusCode.OK}, actual {response.StatusCode}. Body: {bodyJson}");
            using var document = JsonDocument.Parse(bodyJson);
            var root = document.RootElement;

            var metadata = root.GetProperty("boundaryMetadata");
            Assert.Equal(bool.TrueString, metadata.GetProperty("runtime_invoked").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("network_call_executed").GetString());
            Assert.Equal(bool.FalseString, metadata.GetProperty("external_provider_called").GetString());
            foreach (var obsoleteCarrier in new[]
                     {
                         "external_visual_provider_request_config_supported",
                         "external_visual_provider_request_config_present",
                         "external_visual_provider_request_scoped_resolver_configured",
                         "external_visual_provider_shared_http_client_injected"
                     })
            {
                Assert.False(metadata.TryGetProperty(obsoleteCarrier, out _));
            }

            Assert.True(
                fixedComparisonProvider.Requests.Count > 0,
                $"Expected omitted-client path to use fixed provider; body: {bodyJson}");
            Assert.Equal(0, handler.CallCount);
        }
        finally
        {
            await app.StopAsync();
            DeleteDirectoryIfExists(tempRoot);
        }
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

    private static void AssertTrustedAspNetDefaultSubmitMetadata(JsonElement metadata, Uri baseAddress)
    {
        var expectedBaseUrl = baseAddress.GetLeftPart(UriPartial.Authority);
        var expectedSubmitUri = new Uri(
            new Uri(expectedBaseUrl + "/"),
            DesignConversionModelServerEndpointRegistry.EndpointPath.TrimStart('/'));

        Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointRoute, metadata.GetProperty("default_submit_endpoint_route").GetString());
        Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointMethod, metadata.GetProperty("default_submit_endpoint_method").GetString());
        Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointPath, metadata.GetProperty("default_submit_endpoint_path").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("default_submit_base_url_configured").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("default_submit_base_url_missing").GetString());
        Assert.Equal(
            DesignConversionModelServerReadyJsonSubmitClient.ConfiguredSubmitStatus,
            metadata.GetProperty("default_submit_base_url_status").GetString());
        Assert.Equal(string.Empty, metadata.GetProperty("default_submit_base_url_missing_setting").GetString());
        Assert.Equal(string.Empty, metadata.GetProperty("default_submit_base_url_next_action").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("default_submit_base_url_loopback").GetString());
        Assert.Equal(
            DesignConversionModelServerAspNetEndpointMapper.AspNetLiveLocalBaseUrlSource,
            metadata.GetProperty("default_submit_base_url_source").GetString());
        Assert.Equal(expectedBaseUrl, metadata.GetProperty("default_submit_base_url").GetString());
        Assert.Equal(Uri.UriSchemeHttp, metadata.GetProperty("default_submit_base_url_scheme").GetString());
        Assert.Equal(string.Empty, metadata.GetProperty("default_submit_base_url_path_base").GetString());
        Assert.Equal(expectedBaseUrl, metadata.GetProperty("default_submit_base_url_authority").GetString());
        Assert.Equal(expectedSubmitUri.ToString(), metadata.GetProperty("default_submit_request_uri").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("flow47_verified").GetString());
        Assert.Equal(
            DesignConversionModelServerCallerContract.Flow47VerificationAuthority,
            metadata.GetProperty("flow47_verification_authority").GetString());
        Assert.Equal(
            DesignConversionModelServerAspNetEndpointMapper.AspNetFlow47VerificationStateNotEvaluated,
            metadata.GetProperty("flow47_verification_state").GetString());
        Assert.Equal(
            DesignConversionModelServerAspNetEndpointMapper.AspNetFlow47SubmitStatusNotProven,
            metadata.GetProperty("flow47_submit_status").GetString());
        Assert.Equal(
            DesignConversionModelServerAspNetEndpointMapper.AspNetFlow47MissingProofName,
            metadata.GetProperty("flow47_missing_setting_names").GetString());
        Assert.Equal(
            DesignConversionModelServerAspNetEndpointMapper.AspNetFlow47NextAction,
            metadata.GetProperty("flow47_next_action").GetString());
        Assert.Equal(
            DesignConversionModelServerAspNetEndpointMapper.AspNetFlow47BaseUrlSource,
            metadata.GetProperty("flow47_base_url_source").GetString());
        Assert.Equal(string.Empty, metadata.GetProperty("flow47_base_url").GetString());
        Assert.Equal(string.Empty, metadata.GetProperty("flow47_endpoint_path").GetString());
        Assert.Equal(string.Empty, metadata.GetProperty("flow47_submit_request_uri").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("flow47_live_local_loopback_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("public_production_default_ready").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
    }

    private static void AssertAspNetAdmittedStableResponseMetadata(JsonElement metadata)
    {
        Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointRoute, metadata.GetProperty("default_submit_endpoint_route").GetString());
        Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointMethod, metadata.GetProperty("default_submit_endpoint_method").GetString());
        Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointPath, metadata.GetProperty("default_submit_endpoint_path").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("default_submit_base_url_configured").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("default_submit_base_url_missing").GetString());
        Assert.Equal(
            DesignConversionModelServerReadyJsonSubmitClient.ConfiguredSubmitStatus,
            metadata.GetProperty("default_submit_base_url_status").GetString());
        Assert.Equal(string.Empty, metadata.GetProperty("asp_net_local_port").GetString());
        Assert.Equal("durable_replay_response_redacted", metadata.GetProperty("bound_port_mode").GetString());
        Assert.Equal("durable_replay_response_stable_redacted", metadata.GetProperty("default_submit_base_url_source").GetString());
        Assert.Equal(string.Empty, metadata.GetProperty("default_submit_base_url").GetString());
        Assert.Equal(string.Empty, metadata.GetProperty("default_submit_base_url_authority").GetString());
        Assert.Equal(string.Empty, metadata.GetProperty("default_submit_request_uri").GetString());
        Assert.Equal("durable_replay_response_stable_redacted", metadata.GetProperty("flow47_base_url_source").GetString());
        Assert.Equal(string.Empty, metadata.GetProperty("flow47_base_url").GetString());
        Assert.Equal(string.Empty, metadata.GetProperty("flow47_submit_request_uri").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("idempotency_durable_replay_response_metadata_stable").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("idempotency_durable_replay_response_volatiles_redacted").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("idempotency_durable_replay_response_fake_endpoint").GetString());
    }

    private static void AssertTrustedAspNetRouteMetadata(
        JsonElement metadata,
        string expectedRoutePath,
        string expectedHandlerName)
    {
        var expectedRouteId = AspNetRouteIdForTest(expectedRoutePath);
        Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_route_verified").GetString());
        Assert.Equal(
            DesignConversionModelServerAspNetEndpointMapper.AspNetRouteVerificationAuthority,
            metadata.GetProperty("asp_net_route_verification_authority").GetString());
        Assert.Equal(
            DesignConversionModelServerAspNetEndpointMapper.AspNetRouteVerifiedState,
            metadata.GetProperty("asp_net_route_verification_state").GetString());
        Assert.Equal(expectedRouteId, metadata.GetProperty("asp_net_route_id").GetString());
        Assert.Equal(expectedRouteId, metadata.GetProperty("asp_net_route_verification_route_id").GetString());
        Assert.Equal(expectedRoutePath, metadata.GetProperty("asp_net_route_path").GetString());
        Assert.Equal(expectedRoutePath, metadata.GetProperty("asp_net_route_verification_route_path").GetString());
        Assert.Equal(expectedHandlerName, metadata.GetProperty("asp_net_handler_dispatch_target").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_route_verification_successful_handler_dispatch").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("localhost_loopback_only").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_kestrel_socket_opened").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("local_loopback_http_call_executed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("public_http_endpoint_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("deployment_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
    }

    private static void AssertEvaluateStartupEndpoint(RouteEndpoint endpoint, string expectedMappedRoute)
    {
        Assert.Equal(expectedMappedRoute, endpoint.RoutePattern.RawText);
        var startupMetadata = endpoint.Metadata.GetMetadata<DesignConversionModelServerAspNetHostStartupMetadata>();
        Assert.NotNull(startupMetadata);
        Assert.Equal(
            DesignConversionModelServerAspNetHostStartup.HostStartupSurface,
            startupMetadata!.StartupSurface);
        Assert.Equal(
            DesignConversionModelServerAspNetHostStartup.HostStartupBoundary,
            startupMetadata.Boundary);
        Assert.Equal(expectedMappedRoute, startupMetadata.MappedRoute);
        Assert.Equal(
            DesignConversionModelServerAspNetHostStartup.FullLocalRouteSet(),
            startupMetadata.MappedRoutes);
        Assert.Contains(
            DesignConversionModelServerEndpointRegistry.EndpointPath,
            startupMetadata.MappedRoutes);
        Assert.Contains(
            DesignConversionModelServerEndpointRegistry.InternalEndpointPath,
            startupMetadata.MappedRoutes);
        Assert.True(startupMetadata.LocalOnly);
        Assert.True(startupMetadata.RefsOnly);
        Assert.False(startupMetadata.PublicDeploymentVerified);
        Assert.False(startupMetadata.CrossRepoCallerVerified);
        Assert.False(startupMetadata.ExternalProviderConfigured);
        Assert.False(startupMetadata.ProductReady);

        var conventionProbe = endpoint.Metadata.GetMetadata<StartupConventionProbeMetadata>();
        Assert.NotNull(conventionProbe);
        Assert.Equal("evaluate-route-startup-convention", conventionProbe!.Marker);
    }

    private static void AssertUnverifiedAspNetRouteMetadata(JsonObject metadata, string expectedRoutePath)
    {
        var expectedRouteId = AspNetRouteIdForTest(expectedRoutePath);
        Assert.Equal(bool.FalseString, MetadataValue(metadata, "asp_net_route_verified"));
        Assert.Equal(
            DesignConversionModelServerAspNetEndpointMapper.AspNetRouteVerificationAuthority,
            MetadataValue(metadata, "asp_net_route_verification_authority"));
        Assert.Equal(
            "not_verified_server_owned_loopback_route_dispatch",
            MetadataValue(metadata, "asp_net_route_verification_state"));
        Assert.Equal(expectedRouteId, MetadataValue(metadata, "asp_net_route_id"));
        Assert.Equal(expectedRouteId, MetadataValue(metadata, "asp_net_route_verification_route_id"));
        Assert.Equal(expectedRoutePath, MetadataValue(metadata, "asp_net_route_path"));
        Assert.Equal(expectedRoutePath, MetadataValue(metadata, "asp_net_route_verification_route_path"));
    }

    private static JsonObject InvokeAspNetMetadataForTest(
        HttpContext context,
        bool loopbackOnly,
        bool socketOpened,
        string localAddress,
        string requestPath = DesignConversionModelServerEndpointRegistry.EndpointPath,
        string routePath = DesignConversionModelServerEndpointRegistry.EndpointPath,
        string handlerName = nameof(DesignConversionModelServerHttpHandler),
        bool responseDispatchSucceeded = true,
        bool publishLiveRouteMappingFlags = false,
        bool registryDispatchVerified = true)
    {
        context.Request.Path = requestPath;
        var metadata = new JsonObject();
        var method = typeof(DesignConversionModelServerAspNetEndpointMapper).GetMethod(
            "ApplyAspNetMetadata",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        method.Invoke(
            null,
            new object[]
            {
                metadata,
                context,
                loopbackOnly,
                socketOpened,
                localAddress,
                routePath,
                handlerName,
                responseDispatchSucceeded,
                publishLiveRouteMappingFlags,
                registryDispatchVerified
            });

        return metadata;
    }

    private static Task InvokeAspNetRequestExceptionGuardForTest(
        HttpContext context,
        Func<Task> dispatch)
    {
        var method = typeof(DesignConversionModelServerAspNetEndpointMapper).GetMethod(
            "DispatchWithRequestExceptionGuardAsync",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return Assert.IsAssignableFrom<Task>(method.Invoke(
            null,
            [
                context,
                DesignConversionModelServerEndpointRegistry.EndpointPath,
                nameof(DesignConversionModelServerHttpHandler),
                dispatch
            ]));
    }

    private static void ReplaceEvaluateRegistrationHandlerForTest(
        DesignConversionModelServerEndpointRegistry registry,
        Func<DesignConversionModelServerHttpRequest, CancellationToken, Task<DesignConversionModelServerHttpResponse>> handler)
    {
        var field = typeof(DesignConversionModelServerEndpointRegistry).GetField(
            "_routes",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(field);
        var routes = Assert.IsType<Dictionary<string, DesignConversionModelServerEndpointRegistration>>(
            field!.GetValue(registry));
        const string key = DesignConversionModelServerEndpointRegistry.EndpointMethod + " " +
                           DesignConversionModelServerEndpointRegistry.EndpointPath;
        Assert.True(routes.TryGetValue(key, out var registration), "Evaluate route must be registered before replacement.");
        routes[key] = registration! with
        {
            Handler = handler
        };
    }

    private static void AssertMissingAspNetDefaultSubmitMetadata(JsonObject metadata)
    {
        Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointRoute, MetadataValue(metadata, "default_submit_endpoint_route"));
        Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointMethod, MetadataValue(metadata, "default_submit_endpoint_method"));
        Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointPath, MetadataValue(metadata, "default_submit_endpoint_path"));
        Assert.Equal(bool.FalseString, MetadataValue(metadata, "default_submit_base_url_configured"));
        Assert.Equal(bool.TrueString, MetadataValue(metadata, "default_submit_base_url_missing"));
        Assert.Equal(
            DesignConversionModelServerCallerContract.DefaultSubmitBaseUrlMissingStatus,
            MetadataValue(metadata, "default_submit_base_url_status"));
        Assert.Equal("BaseUrl", MetadataValue(metadata, "default_submit_base_url_missing_setting"));
        Assert.Equal(
            DesignConversionModelServerCallerContract.DefaultSubmitBaseUrlNextAction,
            MetadataValue(metadata, "default_submit_base_url_next_action"));
        Assert.Equal(bool.FalseString, MetadataValue(metadata, "default_submit_base_url_loopback"));
        Assert.Equal("untrusted_asp_net_context", MetadataValue(metadata, "default_submit_base_url_source"));
        Assert.Equal(string.Empty, MetadataValue(metadata, "default_submit_base_url"));
        Assert.Equal(string.Empty, MetadataValue(metadata, "default_submit_request_uri"));
        Assert.Equal(bool.FalseString, MetadataValue(metadata, Flow47VerifiedKey));
        Assert.Equal(
            DesignConversionModelServerCallerContract.Flow47VerificationAuthority,
            MetadataValue(metadata, "flow47_verification_authority"));
        Assert.Equal("not_evaluated_untrusted_asp_net_context", MetadataValue(metadata, "flow47_verification_state"));
        Assert.Equal(
            DesignConversionModelServerCallerContract.DefaultSubmitBaseUrlMissingStatus,
            MetadataValue(metadata, "flow47_submit_status"));
        Assert.Equal("BaseUrl", MetadataValue(metadata, "flow47_missing_setting_names"));
        Assert.Equal(
            DesignConversionModelServerCallerContract.DefaultSubmitBaseUrlNextAction,
            MetadataValue(metadata, "flow47_next_action"));
        Assert.Equal("untrusted_asp_net_context", MetadataValue(metadata, "flow47_base_url_source"));
        Assert.Equal(string.Empty, MetadataValue(metadata, "flow47_base_url"));
        Assert.Equal(string.Empty, MetadataValue(metadata, "flow47_endpoint_path"));
        Assert.Equal(string.Empty, MetadataValue(metadata, "flow47_submit_request_uri"));
        Assert.Equal(bool.FalseString, MetadataValue(metadata, "flow47_live_local_loopback_verified"));
    }

    private static string MetadataValue(JsonObject metadata, string key)
        => metadata[key]?.GetValue<string>() ?? throw new KeyNotFoundException(key);

    private static string AspNetRouteIdForTest(string routePath)
        => routePath switch
        {
            DesignConversionModelServerEndpointRegistry.EndpointPath => "design_conversion_evaluate",
            DesignConversionModelServerEndpointRegistry.InternalEndpointPath => "design_conversion_internal_evaluate",
            DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath => "design_conversion_improvement_loop",
            DesignConversionModelServerEndpointRegistry.StatusReadbackEndpointPath => "design_conversion_status_readback",
            DesignConversionModelServerEndpointRegistry.VisualCostReadbackEndpointPath => "design_conversion_visual_cost_readback",
            DesignConversionModelServerEndpointRegistry.DpoUsefulnessEndpointPath => "design_conversion_dpo_usefulness",
            DesignConversionModelServerEndpointRegistry.TrainingIntakeEndpointPath => "design_conversion_training_intake",
            DesignConversionModelServerEndpointRegistry.ModelActivationEndpointPath => "design_conversion_model_activation",
            DesignConversionModelServerEndpointRegistry.HumanApprovalChallengeEndpointPath => "design_conversion_human_approval_challenge",
            DesignConversionModelServerEndpointRegistry.HumanApprovalApproveEndpointPath => "design_conversion_human_approval_approve",
            _ => "unknown"
        };

    private static void AssertThinRuntimeFlow47Provenance(JsonElement runtimeStatus)
    {
        if (runtimeStatus.TryGetProperty("job", out var job) &&
            job.TryGetProperty("metadata", out var jobMetadata))
        {
            Assert.False(jobMetadata.TryGetProperty(Flow47VerifiedKey, out _));
            Assert.Equal(
                DesignConversionThinClientJobRuntime.Flow47VerificationAuthority,
                jobMetadata.GetProperty("flow47_verification_authority").GetString());
            Assert.Equal(
                DesignConversionThinClientJobRuntime.Flow47VerificationState,
                jobMetadata.GetProperty("flow47_verification_state").GetString());
        }

        var warnings = runtimeStatus
            .GetProperty("warnings")
            .EnumerateArray()
            .Select(warning => warning.GetString())
            .ToArray();
        Assert.DoesNotContain($"{Flow47VerifiedKey}=false", warnings);
        Assert.Contains(
            $"flow47_verification_authority={DesignConversionThinClientJobRuntime.Flow47VerificationAuthority}",
            warnings);
        Assert.Contains(
            $"flow47_verification_state={DesignConversionThinClientJobRuntime.Flow47VerificationState}",
            warnings);

        if (runtimeStatus.TryGetProperty("evidenceRefs", out var evidenceRefs))
            AssertJsonStringArrayDoesNotContain(evidenceRefs, $"{Flow47VerifiedKey}=false");

        if (runtimeStatus.TryGetProperty("provenance", out var provenance))
        {
            Assert.False(provenance.TryGetProperty(Flow47VerifiedKey, out _));
            Assert.Equal(
                DesignConversionThinClientJobRuntime.Flow47VerificationAuthority,
                provenance.GetProperty("flow47_verification_authority").GetString());
            Assert.Equal(
                DesignConversionThinClientJobRuntime.Flow47VerificationState,
                provenance.GetProperty("flow47_verification_state").GetString());
        }
    }

    private static void AssertPersistenceVerifiedMetadata(
        JsonElement metadata,
        string jobId,
        string requestId)
    {
        var persistenceVerified = metadata.GetProperty("persistence_verified").GetString();
        Assert.True(
            string.Equals(bool.TrueString, persistenceVerified, StringComparison.Ordinal),
            "Expected server-owned local artifact DB persistence to verify; " +
            $"persistence_verified={persistenceVerified}; " +
            $"failure_reason={metadata.GetProperty("persistence_failure_reason").GetString()}; " +
            $"append_reported={metadata.GetProperty("persistence_server_append_reported").GetString()}; " +
            $"fresh_readback={metadata.GetProperty("persistence_fresh_readback_success").GetString()}; " +
            $"record_count={metadata.GetProperty("persistence_fresh_readback_record_count").GetString()}; " +
            $"server_owned_record_count={metadata.GetProperty("persistence_server_owned_record_count").GetString()}");
        Assert.Equal(
            "server_owned_local_artifact_db_append_fresh_readback",
            metadata.GetProperty("persistence_verification_authority").GetString());
        Assert.Equal(
            "verified_by_server_append_and_fresh_job_request_readback",
            metadata.GetProperty("persistence_verification_state").GetString());
        Assert.Equal("none", metadata.GetProperty("persistence_failure_reason").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("persistence_server_append_reported").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("persistence_fresh_readback_success").GetString());
        Assert.Equal(jobId, metadata.GetProperty("persistence_fresh_readback_query_job_id").GetString());
        Assert.Equal(requestId, metadata.GetProperty("persistence_fresh_readback_query_request_id").GetString());
        Assert.Equal("1", metadata.GetProperty("persistence_fresh_readback_record_count").GetString());
        Assert.Equal("1", metadata.GetProperty("persistence_server_owned_record_count").GetString());
        Assert.Equal(
            nameof(DesignConversionThinClientJobRuntime),
            metadata.GetProperty("persistence_record_source").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("persistence_record_job_bound").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("persistence_record_request_bound").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("persistence_record_refs_only").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("persistence_raw_content_free").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("persistence_no_local_path_public").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("persistence_conversion_result_bound").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("persistence_record_index_consistent").GetString());
        Assert.Equal(
            metadata.GetProperty("persistence_expected_render_ref_count").GetString(),
            metadata.GetProperty("persistence_record_render_ref_count").GetString());
        Assert.Equal(
            metadata.GetProperty("persistence_expected_render_ref_count").GetString(),
            metadata.GetProperty("persistence_bound_render_ref_count").GetString());
        Assert.NotEqual("0", metadata.GetProperty("persistence_expected_render_ref_count").GetString());
        Assert.False(string.IsNullOrWhiteSpace(metadata.GetProperty("persistence_record_id").GetString()));
        Assert.Equal(bool.FalseString, metadata.GetProperty("persistence_caller_claim_accepted").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("persistence_status_spoof_rejected").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("persistence_public_refs_only").GetString());
    }

    private static void AssertJsonStringArrayDoesNotContain(JsonElement array, string unexpected)
    {
        Assert.DoesNotContain(
            unexpected,
            array.EnumerateArray().Select(item => item.GetString()).ToArray());
    }

    private static void AssertTrustedReadinessMarkersDoNotCarryFalse(JsonElement root)
    {
        foreach (var marker in new[] { "browser_e2e_verified", "visual_fidelity_verified" })
        {
            var falseMarker = $"{marker}=false";
            AssertJsonStringArrayDoesNotContain(root.GetProperty("warnings"), falseMarker);
            AssertJsonStringArrayDoesNotContain(root.GetProperty("evidenceRefs"), falseMarker);
            if (root.TryGetProperty("runtimeStatus", out var runtimeStatus))
                AssertJsonStringArrayDoesNotContain(runtimeStatus.GetProperty("warnings"), falseMarker);
        }
    }

    private static void AssertReadinessMarkersCarryFalse(JsonElement root)
    {
        foreach (var marker in new[] { "browser_e2e_verified", "visual_fidelity_verified" })
        {
            var falseMarker = $"{marker}=false";
            Assert.Contains(
                root.GetProperty("warnings").EnumerateArray(),
                item => string.Equals(item.GetString(), falseMarker, StringComparison.Ordinal));
            Assert.Contains(
                root.GetProperty("evidenceRefs").EnumerateArray(),
                item => string.Equals(item.GetString(), falseMarker, StringComparison.Ordinal));
            if (root.TryGetProperty("runtimeStatus", out var runtimeStatus))
            {
                Assert.Contains(
                    runtimeStatus.GetProperty("warnings").EnumerateArray(),
                    item => string.Equals(item.GetString(), falseMarker, StringComparison.Ordinal));
            }
        }
    }

    private static void AssertTrustedVisualProviderNetworkBoundary(JsonElement metadata)
    {
        Assert.Equal(bool.TrueString, metadata.GetProperty("network_call_executed").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("external_network_call_executed").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("external_provider_called").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("real_external_provider_executed").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("external_ai_visual_judge_executed").GetString());
        Assert.Equal("0.973", metadata.GetProperty("visual_provider_visual_judge_score").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("visual_provider_visual_judge_passed").GetString());
        Assert.Equal("0.917", metadata.GetProperty("visual_provider_visual_judge_confidence").GetString());
        Assert.Equal("1", metadata.GetProperty("visual_provider_visual_judge_issue_count").GetString());
        Assert.Equal(
            bool.TrueString,
            metadata.GetProperty(DesignConversionModelServerNetworkBoundary.ServerOwnedVisualProviderNetworkCallVerifiedKey).GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("provider_response_identity_verified").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("provider_response_schema_verified").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty(ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey).GetString());
        Assert.Equal(
            bool.TrueString,
            metadata.GetProperty(ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey).GetString());
        Assert.Equal(
            "openai-loopback-visual-provider",
            metadata.GetProperty(ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderNameKey).GetString());
        Assert.Equal(
            "gpt-loopback-visual-verifier-v1",
            metadata.GetProperty(ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderModelKey).GetString());
        Assert.Contains(
            $":{ExternalHttpDesignConversionVisualJudge.ServerOwnedVisualResultSegment}",
            metadata.GetProperty(ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualResultIdKey).GetString(),
            StringComparison.Ordinal);
        Assert.Contains(
            $":{ExternalHttpDesignConversionVisualJudge.ServerOwnedVisualResultSegment}:comparison-artifact",
            metadata.GetProperty(ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey).GetString(),
            StringComparison.Ordinal);
    }

    private static void AssertRuntimeNotCompletedVisualFidelityMetadata(JsonElement metadata)
    {
        Assert.Equal(bool.FalseString, metadata.GetProperty("browser_e2e_verified").GetString());
        Assert.Equal("runtime_not_completed", metadata.GetProperty("browser_e2e_failure_reason").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("visual_fidelity_verified").GetString());
        Assert.Equal(
            "not_verified_by_server_owned_browser_and_visual_provider_receipts",
            metadata.GetProperty("visual_fidelity_verification_state").GetString());
        Assert.Equal("runtime_not_completed", metadata.GetProperty("visual_fidelity_failure_reason").GetString());
    }

    private static HttpRequestMessage CreateAspNetImprovementLoopAdmissionRequest(
        string bodyJson,
        string? token,
        string? idempotencyKey)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath)
        {
            Content = new StringContent(bodyJson, Encoding.UTF8, "application/json")
        };

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.TryAddWithoutValidation(
                DesignConversionModelServerRouteAdmission.AuthorizationHeaderName,
                "Bearer " + token);
        }

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            request.Headers.TryAddWithoutValidation(
                DesignConversionModelServerRouteAdmission.IdempotencyHeaderName,
                idempotencyKey);
        }

        return request;
    }

    private static HttpRequestMessage CreateAspNetEvaluateAdmissionRequest(
        string bodyJson,
        string? token,
        string? idempotencyKey,
        string route = DesignConversionModelServerEndpointRegistry.EndpointPath)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            route)
        {
            Content = new StringContent(bodyJson, Encoding.UTF8, "application/json")
        };

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.TryAddWithoutValidation(
                DesignConversionModelServerRouteAdmission.AuthorizationHeaderName,
                "Bearer " + token);
        }

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            request.Headers.TryAddWithoutValidation(
                DesignConversionModelServerRouteAdmission.IdempotencyHeaderName,
                idempotencyKey);
        }

        return request;
    }

    private static HttpRequestMessage CreateAspNetVisualCostReadbackAdmissionRequest(
        string route,
        string? token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, route);
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.TryAddWithoutValidation(
                DesignConversionModelServerRouteAdmission.AuthorizationHeaderName,
                "Bearer " + token);
        }

        return request;
    }

    private static HttpRequestMessage CreateAspNetStatusReadbackAdmissionRequest(
        string route,
        string? token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, route);
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.TryAddWithoutValidation(
                DesignConversionModelServerRouteAdmission.AuthorizationHeaderName,
                "Bearer " + token);
        }

        return request;
    }

    private static string VisualCostRoute(string jobId)
        => DesignConversionModelServerVisualCostHandler.EndpointPathPrefix +
           Uri.EscapeDataString(jobId) +
           DesignConversionModelServerVisualCostHandler.EndpointPathSuffix;

    private static IDisposable UseVisualCostLedgerRoot(string ledgerRoot)
    {
        var variableName = JsonlDesignConversionVisualCostLedger.VisualCostLedgerRootEnvironmentVariable;
        var previous = Environment.GetEnvironmentVariable(variableName);
        Environment.SetEnvironmentVariable(variableName, ledgerRoot);
        return new EnvironmentVariableScope(variableName, previous);
    }

    private static void AssertNoStoreHeaders(HttpResponseMessage response)
    {
        Assert.Equal("no-store, no-cache, max-age=0", response.Headers.GetValues("Cache-Control").Single());
        Assert.Equal("no-cache", response.Headers.GetValues("Pragma").Single());
        Assert.Equal("0", response.Content.Headers.GetValues("Expires").Single());
    }

    private static void AssertVisualCostError(
        HttpResponseMessage response,
        string bodyJson,
        HttpStatusCode expectedStatus,
        string expectedErrorCode,
        bool expectedRuntimeReadbackInvoked)
    {
        Assert.Equal(expectedStatus, response.StatusCode);
        AssertNoStoreHeaders(response);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(
            DesignConversionModelServerEndpointRegistry.VisualCostReadbackEndpointPath,
            response.Headers.GetValues("x-design-conversion-asp-net-route").Single());
        Assert.Equal(
            ((int)expectedStatus).ToString(CultureInfo.InvariantCulture),
            response.Headers.GetValues("x-design-conversion-status-code").Single());
        Assert.Equal(
            bool.FalseString,
            response.Headers.GetValues("x-design-conversion-product-ready").Single());

        using var document = JsonDocument.Parse(bodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionModelServerVisualCostHandler.EndpointTemplatePath, root.GetProperty("endpoint").GetString());
        Assert.Equal("GET", root.GetProperty("method").GetString());
        Assert.Equal((int)expectedStatus, root.GetProperty("statusCode").GetInt32());
        Assert.Equal(expectedErrorCode, root.GetProperty("errorCode").GetString());
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("errorMessage").GetString()));

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(
            nameof(DesignConversionModelServerVisualCostHandler),
            metadata.GetProperty("asp_net_handler_dispatch_target").GetString());
        Assert.Equal(
            DesignConversionModelServerEndpointRegistry.VisualCostReadbackEndpointPath,
            metadata.GetProperty("asp_net_route_path").GetString());
        Assert.Equal(
            DesignConversionModelServerVisualCostHandler.EndpointTemplatePath,
            metadata.GetProperty("http_path").GetString());
        Assert.Equal(
            DesignConversionModelServerVisualCostHandler.EndpointTemplatePath,
            metadata.GetProperty("route_admission_path").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_registry_dispatch_verified").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("visual_cost_response_refs_only").GetString());
        Assert.Equal(expectedRuntimeReadbackInvoked.ToString(), metadata.GetProperty("runtime_visual_cost_records_readback_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("public_http_endpoint_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("deployment_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("sensitive_principal_data_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
        AssertVisualCostBoundaryMetadataHasNoIdentityRefs(metadata);
    }

    private static void AssertVisualCostBadRequestJobError(
        HttpResponseMessage response,
        string bodyJson)
    {
        AssertVisualCostError(
            response,
            bodyJson,
            HttpStatusCode.BadRequest,
            "JOB_ID_INVALID",
            expectedRuntimeReadbackInvoked: false);
        Assert.DoesNotContain("DESIGN_CONVERSION_ROUTE_ADMISSION_TOKEN_REQUIRED", bodyJson, StringComparison.Ordinal);
    }

    private static void AssertVisualCostDenialDidNotReadCorruptLedger(
        string bodyJson,
        string ledgerRoot,
        params string[] forbiddenValues)
    {
        Assert.DoesNotContain(
            DesignConversionThinClientJobRuntime.VisualCostRecordsReadFailedErrorCode,
            bodyJson,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            DesignConversionThinClientJobRuntime.VisualCostRecordsReadFailedSafeMessage,
            bodyJson,
            StringComparison.Ordinal);
        Assert.DoesNotContain(ledgerRoot, bodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(JsonlDesignConversionVisualCostLedger.RecordsFileName, bodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("not-json", bodyJson, StringComparison.OrdinalIgnoreCase);
        AssertAspNetResponseAndBoundaryMetadataDoNotContain(bodyJson, forbiddenValues);

        using var document = JsonDocument.Parse(bodyJson);
        var metadata = document.RootElement.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_visual_cost_records_readback_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("sensitive_principal_data_included").GetString());
        AssertVisualCostBoundaryMetadataHasNoIdentityRefs(metadata);
    }

    private static void AssertVisualCostBoundaryMetadataHasNoIdentityRefs(JsonElement metadata)
    {
        foreach (var property in metadata.EnumerateObject())
        {
            var name = property.Name;
            Assert.False(
                IsForbiddenVisualCostIdentityMetadataKey(name),
                $"Visual-cost metadata key leaks identity/ref/hash shape: {name}");

            if (property.Value.ValueKind == JsonValueKind.String)
            {
                var value = property.Value.GetString() ?? string.Empty;
                Assert.DoesNotContain("design-conversion-route-admission-scope", value, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("tenant-", value, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("raw-user", value, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("PRIVATE_", value, StringComparison.Ordinal);
            }
        }
    }

    private static bool IsForbiddenVisualCostIdentityMetadataKey(string key)
    {
        var normalized = key.ToLowerInvariant();
        if (normalized.Contains("scope_ref", StringComparison.Ordinal))
            return true;
        if (normalized.Contains("tenant", StringComparison.Ordinal) ||
            normalized.Contains("account", StringComparison.Ordinal) ||
            normalized.Contains("principal", StringComparison.Ordinal))
        {
            return normalized.Contains("identity", StringComparison.Ordinal) ||
                   normalized.Contains("_id", StringComparison.Ordinal) ||
                   normalized.EndsWith("id", StringComparison.Ordinal) ||
                   normalized.Contains("hash", StringComparison.Ordinal) ||
                   normalized.Contains("ref", StringComparison.Ordinal);
        }

        if (!normalized.Contains("user", StringComparison.Ordinal))
            return false;

        return normalized.Contains("identity", StringComparison.Ordinal) ||
               normalized.Contains("_id", StringComparison.Ordinal) ||
               normalized.EndsWith("id", StringComparison.Ordinal) ||
               normalized.Contains("hash", StringComparison.Ordinal) ||
               normalized.Contains("ref", StringComparison.Ordinal);
    }

    private static void AssertAspNetResponseAndBoundaryMetadataDoNotContain(
        string bodyJson,
        params string?[] forbiddenValues)
    {
        using var document = JsonDocument.Parse(bodyJson);
        var metadataJson = document.RootElement.GetProperty("boundaryMetadata").GetRawText();
        foreach (var forbidden in forbiddenValues)
        {
            if (string.IsNullOrEmpty(forbidden))
                continue;

            Assert.DoesNotContain(forbidden, bodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(forbidden, metadataJson, StringComparison.Ordinal);
        }
    }

    private static void AssertAspNetHostInputValidationError(
        HttpResponseMessage response,
        string bodyJson,
        HttpStatusCode expectedStatus,
        string endpointPath,
        string handlerName,
        string expectedErrorCode)
    {
        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains(expectedErrorCode, bodyJson, StringComparison.Ordinal);
        Assert.Equal(
            endpointPath,
            response.Headers.GetValues("x-design-conversion-asp-net-route").Single());
        Assert.Equal(
            bool.TrueString,
            response.Headers.GetValues("x-design-conversion-asp-net-host-verified").Single());
        Assert.Equal(
            nameof(DesignConversionModelServerAspNetEndpointMapper),
            response.Headers.GetValues("x-design-conversion-asp-net-endpoint-mapper").Single());
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
        Assert.Equal(
            handlerName,
            metadata.GetProperty("asp_net_handler_dispatch_target").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_registry_dispatch_verified").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_host_verified").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_minimal_api_pipeline_verified").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_route_matched").GetString());
        Assert.Equal(endpointPath, metadata.GetProperty("asp_net_route_path").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("asp_net_route_verified").GetString());
        Assert.Equal(
            DesignConversionModelServerAspNetEndpointMapper.AspNetRouteVerificationAuthority,
            metadata.GetProperty("asp_net_route_verification_authority").GetString());
        Assert.Equal(
            "not_verified_server_owned_loopback_route_dispatch",
            metadata.GetProperty("asp_net_route_verification_state").GetString());
        Assert.Equal(AspNetRouteIdForTest(endpointPath), metadata.GetProperty("asp_net_route_id").GetString());
        Assert.Equal(AspNetRouteIdForTest(endpointPath), metadata.GetProperty("asp_net_route_verification_route_id").GetString());
        Assert.Equal(endpointPath, metadata.GetProperty("asp_net_route_verification_route_path").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("asp_net_route_verification_successful_handler_dispatch").GetString());
        Assert.Equal(((int)expectedStatus).ToString(), metadata.GetProperty("http_status_code").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("external_provider_called").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("network_call_executed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
    }

    private static void AssertCallerShapedResponse(
        HttpResponseMessage response,
        string bodyJson,
        CallerShapedRequestFixture fixture,
        Uri baseAddress)
    {
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(IPAddress.Loopback.ToString(), baseAddress.Host);
        Assert.True(baseAddress.Port > 0);
        Assert.Equal(
            bool.TrueString,
            response.Headers.GetValues("x-design-conversion-asp-net-host-verified").Single());
        Assert.Equal(
            nameof(DesignConversionModelServerAspNetEndpointMapper),
            response.Headers.GetValues("x-design-conversion-asp-net-endpoint-mapper").Single());
        Assert.Equal(
            bool.FalseString,
            response.Headers.GetValues("x-design-conversion-public-deployment-verified").Single());
        Assert.Equal(
            bool.FalseString,
            response.Headers.GetValues("x-design-conversion-public-production-default-ready").Single());

        Assert.DoesNotContain(fixture.PrivateMarker, bodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(fixture.Html, bodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(fixture.Css, bodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(fixture.DesignTreePayload, bodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(fixture.OriginalTreePayload, bodyJson, StringComparison.Ordinal);

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
        Assert.Equal(DesignConversionThinClientStatus.Completed, root.GetProperty("contractStatus").GetString());
        Assert.Equal(fixture.LoopId, root.GetProperty("loopId").GetString());
        Assert.Equal(fixture.RequestId, root.GetProperty("requestId").GetString());
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, root.GetProperty("stopReason").GetString());
        Assert.True(root.GetProperty("completed").GetBoolean());
        Assert.True(root.GetProperty("qualityReached").GetBoolean());
        Assert.False(root.GetProperty("providerRequiredOrBlocked").GetBoolean());
        Assert.Equal(1, root.GetProperty("attemptCount").GetInt32());
        Assert.True(root.GetProperty("evidenceRefs").GetArrayLength() > 0);
        Assert.Single(root.GetProperty("attempts").EnumerateArray());

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(
            nameof(DesignConversionModelServerAspNetEndpointMapper),
            metadata.GetProperty("asp_net_endpoint_mapper").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_host_verified").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_minimal_api_pipeline_verified").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_route_matched").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_registry_dispatch_verified").GetString());
        Assert.Equal(
            nameof(DesignConversionModelServerImprovementLoopHandler),
            metadata.GetProperty("asp_net_handler_dispatch_target").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("localhost_loopback_only").GetString());
        Assert.Equal("127.0.0.1", metadata.GetProperty("loopback_host").GetString());
        Assert.Equal("asp_net_runtime_bound", metadata.GetProperty("bound_port_mode").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("asp_net_kestrel_socket_opened").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("local_loopback_http_call_executed").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("refs_only_public_response").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("public_http_endpoint_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("cross_repo_caller_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("deployment_verified").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("public_production_default_ready").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("external_provider_called").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("network_call_executed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("external_network_call_executed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("external_ai_visual_judge_executed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("external_visual_ai_executed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("real_external_provider_executed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty(DesignConversionVisualAiProviderSelectionPolicy.RealCloudProviderClaimedKey).GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("response_contains_raw_private_data").GetString());
        AssertTestOnlyVisualJudgeBoundary(root);
    }

    private static void AssertTestOnlyVisualJudgeBoundary(JsonElement root)
    {
        var attempts = root.GetProperty("attempts").EnumerateArray().ToArray();
        Assert.Single(attempts);
        var attempt = attempts[0];
        Assert.Equal(2, attempt.GetProperty("visualJudgeResultRefs").GetArrayLength());

        var publicMetadata = attempt.GetProperty("publicMetadata");
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.ConfiguredOrLocalLabelerBoundary,
            publicMetadata.GetProperty("visual_judge_provider_boundary").GetString());
        Assert.Equal("2", publicMetadata.GetProperty("visual_judge_result_count").GetString());
        Assert.Equal("0", publicMetadata.GetProperty("estimated_cost_usd").GetString());
        Assert.Equal(bool.FalseString, publicMetadata.GetProperty("external_ai_visual_judge_executed").GetString());
        Assert.Equal(bool.FalseString, publicMetadata.GetProperty("product_ready").GetString());
        Assert.Equal(bool.FalseString, publicMetadata.GetProperty("trained_feature_claimed").GetString());

        var testProviderOnlyEvidenceRef = DesignConversionDpoLeakageGuard.HashRef(
            "dcm-loop-evidence",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker);
        var productNotReadyEvidenceRef = DesignConversionDpoLeakageGuard.HashRef(
            "dcm-loop-evidence",
            DesignConversionVisualQaBoundaryPolicy.ProductNotReadyMarker);
        var externalAiNotExecutedEvidenceRef = DesignConversionDpoLeakageGuard.HashRef(
            "dcm-loop-evidence",
            "external_ai_visual_judge_executed:false");
        var externalAiExecutedEvidenceRef = DesignConversionDpoLeakageGuard.HashRef(
            "dcm-loop-evidence",
            "external_ai_visual_judge_executed:true");
        var visualAiExecutedTrueEvidenceRef = DesignConversionDpoLeakageGuard.HashRef(
            "dcm-loop-evidence",
            "visual_ai_executed_true");
        var attemptEvidenceRefs = attempt.GetProperty("evidenceRefs")
            .EnumerateArray()
            .Select(reference => reference.GetString())
            .ToArray();
        Assert.Contains(testProviderOnlyEvidenceRef, attemptEvidenceRefs);

        var evidenceRefs = root.GetProperty("evidenceRefs")
            .EnumerateArray()
            .Select(reference => reference.GetString())
            .ToArray();
        Assert.Contains(testProviderOnlyEvidenceRef, evidenceRefs);
        Assert.Contains(productNotReadyEvidenceRef, evidenceRefs);
        Assert.Contains(externalAiNotExecutedEvidenceRef, evidenceRefs);
        Assert.DoesNotContain(externalAiExecutedEvidenceRef, evidenceRefs);
        Assert.DoesNotContain(visualAiExecutedTrueEvidenceRef, evidenceRefs);
    }

    private static CallerShapedRequestFixture CreateFigmaCallerShapedRequest()
    {
        const string privateMarker = "PRIVATE_PASS142_FIGMA_LIVE_LOCAL_SECRET";
        const string loopId = "pass142-figma-live-local-loop";
        const string requestId = "pass142-figma-live-local-request";
        const string html = "<main data-secret=\"PRIVATE_PASS142_FIGMA_LIVE_LOCAL_SECRET\"><h1>Pass142 Figma</h1></main>";
        const string css = ".pass142-figma{background-image:url('PRIVATE_PASS142_FIGMA_LIVE_LOCAL_SECRET');}";
        const string rootJson = "{\"kind\":\"figma-root\",\"secret\":\"PRIVATE_PASS142_FIGMA_LIVE_LOCAL_SECRET\",\"children\":[{\"name\":\"CTA\"}]}";

        var body = new JsonObject
        {
            ["loopId"] = loopId,
            ["initialPackage"] = new JsonObject
            {
                ["requestId"] = requestId,
                ["sourceKind"] = "Figma",
                ["designTree"] = new JsonObject
                {
                    ["refId"] = "figma-design-tree:pass142",
                    ["kind"] = "figma-json",
                    ["sha256"] = "sha256:pass142-figma-design-tree",
                    ["payload"] = rootJson,
                    ["format"] = "figma-json",
                    ["metadata"] = new JsonObject
                    {
                        ["callerRepo"] = "FigmaToCode-design-gen",
                        ["treeRole"] = "generated_design_tree",
                        ["privateMarker"] = privateMarker
                    }
                },
                ["originalTree"] = new JsonObject
                {
                    ["refId"] = "figma-original-tree:pass142",
                    ["kind"] = "figma-json",
                    ["sha256"] = "sha256:pass142-figma-original-tree",
                    ["payload"] = rootJson,
                    ["format"] = "figma-json",
                    ["metadata"] = new JsonObject
                    {
                        ["callerRepo"] = "FigmaToCode-design-gen",
                        ["treeRole"] = "original_figma_tree",
                        ["privateMarker"] = privateMarker
                    }
                },
                ["html"] = html,
                ["css"] = css,
                ["screenshots"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["artifactId"] = "figma-frame-screenshot-0",
                        ["kind"] = "screenshot",
                        ["width"] = 1440,
                        ["height"] = 900,
                        ["uri"] = "artifact://design-conversion-local/pass142/figma-frame-screenshot-0",
                        ["sha256"] = "sha256:pass142-figma-frame-screenshot",
                        ["metadata"] = new JsonObject
                        {
                            ["sourceTreePath"] = "source/screenshot.jpg",
                            ["originalUriPolicy"] = "not_sent_to_public_response"
                        }
                    },
                    new JsonObject
                    {
                        ["artifactId"] = "figma-frame-screenshot-1",
                        ["kind"] = "screenshot",
                        ["width"] = 390,
                        ["height"] = 844,
                        ["uri"] = "artifact://design-conversion-local/pass142/figma-frame-screenshot-1",
                        ["sha256"] = "sha256:pass142-figma-frame-mobile-screenshot",
                        ["metadata"] = new JsonObject
                        {
                            ["sourceTreePath"] = "source/screenshot-mobile.jpg",
                            ["originalUriPolicy"] = "not_sent_to_public_response"
                        }
                    }
                },
                ["designRenders"] = new JsonArray(),
                ["budget"] = new JsonObject
                {
                    ["userId"] = "figma-local-user-redacted",
                    ["tier"] = "local-pass140-contract",
                    ["maxImprovementCandidates"] = 1,
                    ["maxRenderQaRuns"] = 2,
                    ["budgetUsd"] = 0,
                    ["metadata"] = new JsonObject
                    {
                        ["rawHtmlSha256"] = "sha256:pass142-figma-html",
                        ["rawCssSha256"] = "sha256:pass142-figma-css",
                        ["privateMarker"] = privateMarker
                    }
                },
                ["metadata"] = new JsonObject
                {
                    ["callerRepo"] = "FigmaToCode-design-gen",
                    ["callerContract"] = "design_conversion_model_server_improvement_loop_route_contract_v1",
                    ["responsePolicy"] = "refs_or_hashes_only",
                    ["privateMarker"] = privateMarker
                }
            },
            ["maxAttempts"] = 1,
            ["metadata"] = new JsonObject
            {
                ["callerRepo"] = "FigmaToCode-design-gen",
                ["localContractProof"] = "codex-pass140",
                ["liveLocalProof"] = "codex-pass142",
                ["refsOnlyPublicResponseRequired"] = "true",
                ["privateMarker"] = privateMarker
            }
        };

        return new CallerShapedRequestFixture(
            SourceKind: nameof(DesignConversionSourceKind.Figma),
            LoopId: loopId,
            RequestId: requestId,
            BodyJson: body.ToJsonString(DesignConversionModelServerHttpHandler.JsonOptions),
            PrivateMarker: privateMarker,
            Html: html,
            Css: css,
            DesignTreePayload: rootJson,
            OriginalTreePayload: rootJson);
    }

    private static CallerShapedRequestFixture CreateCanvaCallerShapedRequest()
    {
        const string privateMarker = "PRIVATE_PASS142_CANVA_LIVE_LOCAL_SECRET";
        const string loopId = "pass142-canva-live-local-loop";
        const string requestId = "pass142-canva-live-local-request";
        const string html = "<main data-secret=\"PRIVATE_PASS142_CANVA_LIVE_LOCAL_SECRET\"><h1>Pass142 Canva</h1></main>";
        const string css = ".pass142-canva{background-image:url('PRIVATE_PASS142_CANVA_LIVE_LOCAL_SECRET');}";
        const string sourceTreeJson = "[{\"path\":\"canva-selection.json\",\"content\":\"PRIVATE_PASS142_CANVA_LIVE_LOCAL_SECRET\"},{\"path\":\"generated.html\"},{\"path\":\"generated.css\"}]";
        const string originalTreeJson = "{\"sourceKind\":\"canva\",\"scope\":\"text-only\",\"secret\":\"PRIVATE_PASS142_CANVA_LIVE_LOCAL_SECRET\"}";

        var body = new JsonObject
        {
            ["loopId"] = loopId,
            ["initialPackage"] = new JsonObject
            {
                ["requestId"] = requestId,
                ["sourceKind"] = "Canva",
                ["designTree"] = new JsonObject
                {
                    ["refId"] = "canva-design-tree:pass142",
                    ["kind"] = "canva-source-tree",
                    ["sha256"] = "sha256:pass142-canva-source-tree",
                    ["payload"] = sourceTreeJson,
                    ["format"] = "canva-source-tree-json",
                    ["metadata"] = new JsonObject
                    {
                        ["callerRepo"] = "CanvaToCode",
                        ["treeRole"] = "generated_source_tree",
                        ["privateMarker"] = privateMarker
                    }
                },
                ["originalTree"] = new JsonObject
                {
                    ["refId"] = "canva-original-tree:pass142",
                    ["kind"] = "canva-selection-tree",
                    ["sha256"] = "sha256:pass142-canva-original-tree",
                    ["payload"] = originalTreeJson,
                    ["format"] = "canva-selection-json",
                    ["metadata"] = new JsonObject
                    {
                        ["callerRepo"] = "CanvaToCode",
                        ["treeRole"] = "original_canva_selection_tree",
                        ["privateMarker"] = privateMarker
                    }
                },
                ["html"] = html,
                ["css"] = css,
                ["screenshots"] = new JsonArray(),
                ["designRenders"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["artifactId"] = "canva-design-render:pass142",
                        ["kind"] = "design-render",
                        ["width"] = 1280,
                        ["height"] = 720,
                        ["uri"] = "artifact://design-conversion-local/pass142/canva-design-render",
                        ["sha256"] = "sha256:pass142-canva-design-render",
                        ["metadata"] = new JsonObject
                        {
                            ["sourceTreePath"] = "generated.html+generated.css",
                            ["fallbackReason"] = "canva_text_adapter_no_export_screenshot_api",
                            ["screenshotsUnavailableReason"] = "Canva text adapter does not expose a design export screenshot API."
                        }
                    },
                    new JsonObject
                    {
                        ["artifactId"] = "canva-design-render-mobile:pass142",
                        ["kind"] = "design-render",
                        ["width"] = 390,
                        ["height"] = 844,
                        ["uri"] = "artifact://design-conversion-local/pass142/canva-design-render-mobile",
                        ["sha256"] = "sha256:pass142-canva-design-render-mobile",
                        ["metadata"] = new JsonObject
                        {
                            ["sourceTreePath"] = "generated.html+generated.css",
                            ["fallbackReason"] = "canva_text_adapter_no_export_screenshot_api",
                            ["screenshotsUnavailableReason"] = "Canva text adapter does not expose a design export screenshot API."
                        }
                    }
                },
                ["budget"] = new JsonObject
                {
                    ["userId"] = "canva-local-user-redacted",
                    ["tier"] = "local-pass141-contract",
                    ["maxImprovementCandidates"] = 1,
                    ["maxRenderQaRuns"] = 2,
                    ["budgetUsd"] = 0,
                    ["metadata"] = new JsonObject
                    {
                        ["rawHtmlSha256"] = "sha256:pass142-canva-html",
                        ["rawCssSha256"] = "sha256:pass142-canva-css",
                        ["privateMarker"] = privateMarker
                    }
                },
                ["metadata"] = new JsonObject
                {
                    ["callerRepo"] = "CanvaToCode",
                    ["callerContract"] = "design_conversion_model_server_improvement_loop_route_contract_v1",
                    ["responsePolicy"] = "refs_or_hashes_only",
                    ["sourceScope"] = "text-only",
                    ["screenshotPolicy"] = "design_render_ref_fallback_no_canva_export",
                    ["privateMarker"] = privateMarker
                }
            },
            ["maxAttempts"] = 1,
            ["metadata"] = new JsonObject
            {
                ["callerRepo"] = "CanvaToCode",
                ["localContractProof"] = "codex-pass141",
                ["liveLocalProof"] = "codex-pass142",
                ["refsOnlyPublicResponseRequired"] = "true",
                ["externalProviderCalled"] = "false",
                ["privateMarker"] = privateMarker
            }
        };

        return new CallerShapedRequestFixture(
            SourceKind: nameof(DesignConversionSourceKind.Canva),
            LoopId: loopId,
            RequestId: requestId,
            BodyJson: body.ToJsonString(DesignConversionModelServerHttpHandler.JsonOptions),
            PrivateMarker: privateMarker,
            Html: html,
            Css: css,
            DesignTreePayload: sourceTreeJson,
            OriginalTreePayload: originalTreeJson);
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
            Html: $"<main data-secret=\"{privateMarker}\">Pass139</main>",
            Css: $".pass139{{background-image:url('{privateMarker}');}}",
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
                Tier: "pass139-aspnet-host-startup",
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

    private static DesignConversionModelServerApiRequest CreateEvaluateRequest(
        string requestId,
        string privateMarker)
    {
        var package = CreatePackage(requestId, privateMarker);
        return new DesignConversionModelServerApiRequest(
            RequestId: package.RequestId,
            SourceKind: DesignConversionSourceKind.Figma,
            DesignTree: package.DesignTree!,
            OriginalTree: package.OriginalTree!,
            Html: package.Html!,
            Css: package.Css!,
            Screenshots: package.Screenshots,
            DesignRenders: package.DesignRenders,
            Budget: package.Budget,
            Metadata: package.Metadata);
    }

    private static DesignConversionModelServerApiRequest WithTrustedVisualCostEntitlement(
        DesignConversionModelServerApiRequest request,
        string userId,
        string tier,
        string monthlyLimitUsd,
        string tenantId)
        => request with
        {
            Budget = request.Budget! with
            {
                UserId = userId,
                Tier = tier,
                MaxImprovementCandidates = 1,
                MaxRenderQaRuns = 2,
                BudgetUsd = 0.25m,
                Metadata = new Dictionary<string, string>()
            },
            Metadata = new Dictionary<string, string>(
                request.Metadata ?? new Dictionary<string, string>(),
                StringComparer.Ordinal)
            {
                ["tenant_id"] = tenantId,
                [DesignConversionModelServerApi.ServerAssignedDesignTierMetadataKey] = tier,
                [DesignConversionModelServerApi.ServerAssignedMonthlyVisualCostLimitUsdMetadataKey] = monthlyLimitUsd,
                [DesignConversionModelServerApi.DesignTierAuthorityMetadataKey] =
                    DesignConversionModelServerApi.PlatformTenantConfigurationAuthority,
                [DesignConversionModelServerApi.MonthlyVisualCostLimitAuthorityMetadataKey] =
                    DesignConversionModelServerApi.PlatformTenantConfigurationAuthority,
                [DesignConversionModelServerApi.UserIdentityAuthorityMetadataKey] =
                    DesignConversionModelServerApi.VerifiedInboundPrincipalAuthority
            }
        };

    private static DesignConversionModelServerApiRequest WithServerOwnedAspNetLocalArtifactProofs(
        DesignConversionModelServerApiRequest request,
        string artifactRoot)
        => request with
        {
            Screenshots = ToServerOwnedAspNetLocalArtifactProofs(
                request.Screenshots,
                artifactRoot,
                "screenshot"),
            DesignRenders = ToServerOwnedAspNetLocalArtifactProofs(
                request.DesignRenders,
                artifactRoot,
                "design-render")
        };

    private static IReadOnlyList<DesignConversionModelServerArtifactRef>? ToServerOwnedAspNetLocalArtifactProofs(
        IReadOnlyList<DesignConversionModelServerArtifactRef>? artifacts,
        string artifactRoot,
        string role)
    {
        if (artifacts is null)
            return null;

        var materialized = new List<DesignConversionModelServerArtifactRef>(artifacts.Count);
        for (var index = 0; index < artifacts.Count; index++)
        {
            var artifact = artifacts[index];
            var proof = WriteServerOwnedAspNetPngArtifact(
                Path.Combine(artifactRoot, role),
                $"{role}-{index + 1}-{artifact.ArtifactId}",
                artifact.Width,
                artifact.Height);
            var metadata = (artifact.Metadata ?? new Dictionary<string, string>())
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
            foreach (var pair in proof.ArtifactRef.Metadata ?? new Dictionary<string, string>())
                metadata[pair.Key] = pair.Value;
            metadata["server_owned_artifact_fixture_role"] = role;

            materialized.Add(proof.ArtifactRef with
            {
                ArtifactId = artifact.ArtifactId,
                Kind = artifact.Kind,
                Metadata = metadata
            });
        }

        return materialized.ToArray();
    }

    private static async Task<DesignConversionPromotedModelActivationRecord> BuildAspNetHostActivationRecordAsync(
        string artifactDbRoot,
        string tenantId,
        string name)
    {
        var outputDirectory = Path.Combine(artifactDbRoot, "models", name);
        Directory.CreateDirectory(outputDirectory);

        var machine = new DesignConversionMachine();
        var trained = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(machine, new LearningBatch<DesignDomain>(
            BatchId: $"{name}-batch-0",
            DomainTag: DesignConversionMachine.DomainTag,
            TrainingItems:
            [
                new LearningTrainingItem(
                    ItemId: $"{name}-item-0",
                    Prompt: JsonSerializer.Serialize(new Dictionary<string, object>
                    {
                        ["unit_id"] = DesignConversionRuntimeScaffoldImproverUnit.UnitId,
                        ["source_kind"] = nameof(DesignConversionSourceKind.Canva),
                        ["source_manifest_hash"] = $"{name}-manifest-source",
                        ["source_identity_hash"] = $"{name}-identity",
                        ["source_structure_hash"] = $"{name}-structure"
                    }),
                    ChosenResponse: "{\"html\":\"<main class=\\\"layout-a\\\"></main>\",\"css\":\".layout-a{display:grid;gap:16px}\"}",
                    RejectedResponse: "{\"html\":\"<main class=\\\"layout-b\\\"></main>\",\"css\":\".layout-b{display:block;gap:2px}\"}")
            ],
            SourceIds: [$"{name}-source-0"]));
        Assert.True(trained.IsSuccess, trained.ErrorMessage);

        var checkpointPath = Path.Combine(outputDirectory, $"{name}-checkpoint.json");
        var saved = await machine.SaveCheckpointAsync(checkpointPath);
        Assert.True(saved.IsSuccess, saved.ErrorMessage);
        var loaded = await DesignConversionMachine.LoadCheckpointAsync(checkpointPath);
        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        Assert.True(DesignConversionMachine.CheckpointHasRealLearnedWeights(loaded.Value!.Checkpoint));

        var tokenizerPath = Path.Combine(outputDirectory, $"{name}-tokenizer.json");
        var policyPath = Path.Combine(outputDirectory, $"{name}-policy.json");
        await File.WriteAllTextAsync(tokenizerPath, "{\"kind\":\"aspnet-host-activation-tokenizer\"}");
        await File.WriteAllTextAsync(policyPath, "{\"kind\":\"aspnet-host-activation-policy\"}");

        var checkpointFullPath = Path.GetFullPath(checkpointPath);
        var checkpointSha = Sha256Bytes(await File.ReadAllBytesAsync(checkpointFullPath));
        var manifestId = $"{name}-manifest";
        var manifestRef = $"manifest://design-conversion/{manifestId}";
        var promotionRef = DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix +
                           Uri.EscapeDataString(manifestId);
        var artifacts = new DesignConversionTrainingManifestArtifacts(
            checkpointFullPath,
            Path.GetFullPath(tokenizerPath),
            Path.GetFullPath(policyPath));
        var manifest = new DesignConversionTrainingManifest(
            ManifestId: manifestId,
            MachineId: loaded.Value.Checkpoint.MachineId,
            DomainTag: loaded.Value.Checkpoint.DomainTag,
            Version: loaded.Value.Checkpoint.Version,
            CheckpointRef: checkpointFullPath,
            ManifestRef: manifestRef,
            CreatedAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(300),
            AcceptedRecordCount: 1,
            QuarantinedRecordCount: 0,
            UnitIds: [DesignConversionRuntimeScaffoldImproverUnit.UnitId],
            Batches:
            [
                new DesignConversionTrainingManifestBatch(
                    BatchId: $"{name}-manifest-batch",
                    UnitId: DesignConversionRuntimeScaffoldImproverUnit.UnitId,
                    RecordCount: 1,
                    RecordIds: [$"{name}-record-0"],
                    SourceIds: [$"{name}-source-0"])
            ],
            EvidenceRefs:
            [
                "DESIGN_CONVERSION_LOCAL_PROMOTION_RECEIPT_WRITTEN",
                "MODEL_SERVER_UPLOAD_NOT_ATTEMPTED_LOCAL_RECEIPT",
                $"CHECKPOINT_PATH:{checkpointFullPath}"
            ],
            ModelServerUploadAttempted: false,
            ModelServerPromotionClaimed: false,
            DeterministicLogicClaimedAsLearning: false)
        {
            PromotionArtifacts = artifacts,
            PromotionUploadRef = promotionRef,
            LocalPromotionReceiptWritten = true,
            PromotionReceiptKind = "local_receipt_only"
        };
        var publication = new DesignConversionPromotionPublicationResult(
            new DesignConversionPromotionDecision(
                DesignConversionPromotionDecisionKind.Promote,
                ["ALL_PROMOTION_GATES_PASSED"],
                ["DESIGN_CONVERSION_PROMOTION_GATES_PASSED"]),
            manifest,
            new DesignConversionPromotionUploadReceipt(
                UploadId: $"{name}-local-receipt",
                RemoteRef: promotionRef,
                UploadedAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(300),
                EvidenceRefs: ["DESIGN_CONVERSION_LOCAL_PROMOTION_RECEIPT_WRITTEN"]));
        var model = new DesignConversionPromotedLocalModel(
            ModelId: $"{manifestId}:{checkpointSha[..12]}",
            ManifestId: manifest.ManifestId,
            ManifestRef: manifest.ManifestRef,
            CheckpointPath: checkpointFullPath,
            CheckpointSha256: checkpointSha,
            MachineId: loaded.Value.Checkpoint.MachineId,
            DomainTag: loaded.Value.Checkpoint.DomainTag,
            Version: loaded.Value.Checkpoint.Version,
            LearnedWeightArtifactCount: DesignConversionMachine.CountRealLearnedWeightArtifacts(loaded.Value.Checkpoint),
            LearnedWeightObservationCount: DesignConversionMachine.CountRealLearnedWeightObservations(loaded.Value.Checkpoint),
            CheckpointFreshLoadVerified: true,
            CheckpointContainsRealTrainedWeights: true,
            PromotionManifestClaimed: true,
            LocalOnly: true,
            EvidenceRefs:
            [
                DesignConversionPromotedLocalModelLoader.LocalPromotionBoundary,
                "DESIGN_CONVERSION_LOCAL_PROMOTION_RECEIPT_WRITTEN",
                "DESIGN_CONVERSION_PROMOTED_LOCAL_CHECKPOINT_FRESH_LOADED",
                "DESIGN_CONVERSION_PROMOTED_LOCAL_CHECKPOINT_REAL_WEIGHTS_VERIFIED"
            ]);

        return new DesignConversionPromotedModelActivationRecord(
            DesignConversionModelActivationRegistry.BuildTenantIdRef(tenantId),
            publication,
            model);
    }

    private static string CreateAspNetHostValidObjectValidationBodyJson(string scenario)
    {
        const string privateMarker = "PRIVATE_PASS138_ASPNET_HOST_VALIDATION_SECRET";
        var package = CreatePackage($"pass138-aspnet-{scenario}", privateMarker);

        return scenario switch
        {
            "evaluate_unsupported_source_kind" => SerializeEvaluatePackage(package with
            {
                SourceKind = "Sketch"
            }),
            "evaluate_missing_source_tree_ref_shape" => SerializeEvaluatePackage(package with
            {
                DesignTree = WithoutPayloadRefShape(package.DesignTree!)
            }),
            "evaluate_missing_original_tree_ref_shape" => SerializeEvaluatePackage(package with
            {
                OriginalTree = WithoutPayloadRefShape(package.OriginalTree!)
            }),
            "evaluate_missing_artifact_refs" => SerializeEvaluatePackage(package with
            {
                Screenshots = [],
                DesignRenders = []
            }),
            "evaluate_unsupported_screenshot_ref" => SerializeEvaluatePackage(package with
            {
                Screenshots = [WithUnsupportedProofRef(package.Screenshots![0], "screenshot")],
                DesignRenders = []
            }),
            "evaluate_unsupported_design_render_ref" => SerializeEvaluatePackage(package with
            {
                Screenshots = [],
                DesignRenders = [WithUnsupportedProofRef(package.DesignRenders![0], "design-render")]
            }),
            "evaluate_invalid_viewport" => SerializeEvaluatePackage(package with
            {
                Screenshots = [WithInvalidViewport(package.Screenshots![0])],
                DesignRenders = [WithInvalidViewport(package.DesignRenders![0])]
            }),
            "improvement_loop_unsupported_source_kind" => SerializeImprovementLoopRequest(package with
            {
                SourceKind = "Sketch"
            }),
            "improvement_loop_missing_source_tree_ref_shape" => SerializeImprovementLoopRequest(package with
            {
                DesignTree = WithoutPayloadRefShape(package.DesignTree!)
            }),
            "improvement_loop_missing_original_tree_ref_shape" => SerializeImprovementLoopRequest(package with
            {
                OriginalTree = WithoutPayloadRefShape(package.OriginalTree!)
            }),
            "improvement_loop_missing_artifact_refs" => SerializeImprovementLoopRequest(package with
            {
                Screenshots = [],
                DesignRenders = []
            }),
            "improvement_loop_unsupported_screenshot_ref" => SerializeImprovementLoopRequest(package with
            {
                Screenshots = [WithUnsupportedProofRef(package.Screenshots![0], "screenshot")],
                DesignRenders = []
            }),
            "improvement_loop_unsupported_design_render_ref" => SerializeImprovementLoopRequest(package with
            {
                Screenshots = [],
                DesignRenders = [WithUnsupportedProofRef(package.DesignRenders![0], "design-render")]
            }),
            "improvement_loop_invalid_viewport" => SerializeImprovementLoopRequest(package with
            {
                Screenshots = [WithInvalidViewport(package.Screenshots![0])],
                DesignRenders = [WithInvalidViewport(package.DesignRenders![0])]
            }),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "Unknown ASP.NET-host validation scenario.")
        };
    }

    private static DesignConversionModelServerPayloadRef WithoutPayloadRefShape(
        DesignConversionModelServerPayloadRef source)
        => source with
        {
            RefId = string.Empty,
            Sha256 = null,
            Payload = null
        };

    private static DesignConversionModelServerArtifactRef WithUnsupportedProofRef(
        DesignConversionModelServerArtifactRef artifact,
        string kind)
        => artifact with
        {
            ArtifactId = $"unsupported-{kind}-proof",
            Uri = $"https://client.example.invalid/{kind}-proof.png",
            Sha256 = $"sha256:unsupported-{kind}-proof"
        };

    private static DesignConversionModelServerArtifactRef WithInvalidViewport(
        DesignConversionModelServerArtifactRef artifact)
        => artifact with
        {
            Width = 0,
            Height = 0
        };

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
                    ["pass"] = "codex-pass138-aspnet-host-validation"
                }),
            DesignConversionModelServerHttpHandler.JsonOptions);

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
            Uri: $"artifact://pass139/{id}.png",
            Sha256: $"sha256:{id}");

    private static DesignConversionModelServerLocalCallerPackage MaterializeServerOwnedAspNetVisualArtifacts(
        DesignConversionModelServerLocalCallerPackage package,
        string artifactRoot)
        => package with
        {
            Screenshots = MaterializeServerOwnedAspNetVisualArtifacts(
                artifactRoot,
                package.RequestId,
                package.Screenshots,
                "screenshot"),
            DesignRenders = MaterializeServerOwnedAspNetVisualArtifacts(
                artifactRoot,
                package.RequestId,
                package.DesignRenders,
                "design-render")
        };

    private static DesignConversionModelServerApiRequest MaterializeServerOwnedAspNetVisualArtifacts(
        DesignConversionModelServerApiRequest request,
        string artifactRoot)
        => request with
        {
            Screenshots = MaterializeServerOwnedAspNetVisualArtifacts(
                artifactRoot,
                request.RequestId,
                request.Screenshots,
                "screenshot"),
            DesignRenders = MaterializeServerOwnedAspNetVisualArtifacts(
                artifactRoot,
                request.RequestId,
                request.DesignRenders,
                "design-render")
        };

    private static IReadOnlyList<DesignConversionModelServerArtifactRef>? MaterializeServerOwnedAspNetVisualArtifacts(
        string artifactRoot,
        string requestId,
        IReadOnlyList<DesignConversionModelServerArtifactRef>? artifacts,
        string role)
    {
        if (artifacts is null)
            return null;

        Directory.CreateDirectory(artifactRoot);
        var materialized = new List<DesignConversionModelServerArtifactRef>(artifacts.Count);
        for (var index = 0; index < artifacts.Count; index++)
        {
            var artifact = artifacts[index];
            var seed = Sha256Text($"{requestId}|{role}|{artifact.ArtifactId}|{artifact.Width}x{artifact.Height}|{index}");
            var bytes = BuildFixturePng(artifact.Width, artifact.Height, seed);
            var path = Path.Combine(
                artifactRoot,
                "expected",
                $"{SafePathSegment(requestId)}-{index + 1}-{SafePathSegment(artifact.ArtifactId)}.png");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, bytes);
            var sha = $"sha256:{Sha256Bytes(bytes)}";
            Assert.DoesNotContain(artifactRoot, sha, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(path, sha, StringComparison.OrdinalIgnoreCase);
            var metadata = (artifact.Metadata ?? new Dictionary<string, string>())
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
            metadata["server_owned_visual_provider_fixture"] = bool.TrueString;
            metadata["server_owned_visual_provider_fixture_role"] = role;

            materialized.Add(artifact with
            {
                Uri = sha,
                Sha256 = sha,
                Metadata = metadata
            });
        }

        return materialized.ToArray();
    }

    private static string CreateServerOwnedAspNetVisualArtifactRoot()
        => Path.Combine(
            Path.GetTempPath(),
            "design-conversion-aspnet-external-visual-provider-artifacts",
            Guid.NewGuid().ToString("N"));

    private static ServerOwnedAspNetArtifactFixture WriteServerOwnedAspNetPngArtifact(
        string artifactRoot,
        string artifactId,
        int width = 64,
        int height = 48)
    {
        Directory.CreateDirectory(artifactRoot);
        var seed = Sha256Text(artifactId);
        var bytes = BuildFixturePng(width, height, seed);
        var path = Path.Combine(artifactRoot, $"{SafePathSegment(artifactId)}.png");
        File.WriteAllBytes(path, bytes);

        return new ServerOwnedAspNetArtifactFixture(
            new DesignConversionModelServerArtifactRef(
                ArtifactId: artifactId,
                Kind: "screenshot",
                Width: width,
                Height: height,
                Uri: new Uri(path).AbsoluteUri,
                Sha256: $"sha256:{Sha256Bytes(bytes)}",
                Metadata: new Dictionary<string, string>
                {
                    ["proof_boundary"] = DesignConversionServerOwnedLocalArtifactRefResolver.Boundary
                }),
            path);
    }

    private static void DeleteDirectoryIfExists(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }

    private static string Sha256Text(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static string Sha256Bytes(byte[] value)
        => Convert.ToHexString(SHA256.HashData(value)).ToLowerInvariant();

    private static string WriteFixturePng(
        string requestId,
        string captureRequestId,
        RenderQaResolution resolution,
        string seed,
        string? artifactRoot = null)
    {
        var bytes = BuildFixturePng(resolution.Width, resolution.Height, seed);
        var root = artifactRoot is null
            ? Path.Combine(
                Path.GetTempPath(),
                "design-conversion-pass139-aspnet-host-png-fixtures",
                SafePathSegment(requestId))
            : Path.Combine(
                artifactRoot,
                "browser-captures",
                SafePathSegment(requestId));
        Directory.CreateDirectory(root);

        var path = Path.Combine(root, $"{SafePathSegment(captureRequestId)}-{resolution.Width}x{resolution.Height}.png");
        File.WriteAllBytes(path, bytes);
        return path;
    }

    private static byte[] BuildFixturePng(int width, int height, string seedHex)
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
                var stripe = ((x / Math.Max(1, width / 10)) + (y / Math.Max(1, height / 8))) % 2;
                rgba[offset] = (byte)((seedA + x + (stripe * 41)) % 256);
                rgba[offset + 1] = (byte)((seedB + y + (stripe * 67)) % 256);
                rgba[offset + 2] = (byte)((seedC + x + y + 83) % 256);
                rgba[offset + 3] = 255;
            }
        }

        using var png = new MemoryStream();
        png.Write([137, 80, 78, 71, 13, 10, 26, 10]);
        WritePngChunk(png, "IHDR", BuildPngIhdr(width, height));
        WritePngChunk(png, "IDAT", CompressPngScanlines(rgba, width, height));
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

    private static byte[] CompressPngScanlines(byte[] rgba, int width, int height)
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

    private static string SafePathSegment(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
            builder.Append(char.IsLetterOrDigit(ch) || ch is '-' or '_' ? ch : '-');

        var segment = builder.ToString().Trim('-');
        return string.IsNullOrWhiteSpace(segment) ? Sha256Text(value)[..16] : segment;
    }

    private sealed class RecordingBrowserAdapter : IDesignConversionBrowserProcessAdapter
    {
        private readonly string? _artifactRoot;

        public RecordingBrowserAdapter(string? artifactRoot = null)
            => _artifactRoot = artifactRoot;

        public List<DesignConversionBrowserCaptureRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionBrowserCaptureResult>> CaptureScreenshotAsync(
            DesignConversionBrowserCaptureRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var sha = Sha256Text(string.Join(
                "|",
                request.CaptureRequestId,
                request.RequestId,
                request.Resolution.ResolutionId,
                request.Resolution.Width,
                request.Resolution.Height,
                request.DocumentHtml));
            var screenshotPath = WriteFixturePng(
                request.RequestId,
                request.CaptureRequestId,
                request.Resolution,
                sha,
                _artifactRoot);
            var screenshotSha = Sha256Bytes(File.ReadAllBytes(screenshotPath));

            return Task.FromResult(OperationResult<DesignConversionBrowserCaptureResult>.Ok(
                new DesignConversionBrowserCaptureResult(
                    ScreenshotUri: screenshotPath,
                    ScreenshotSha256: screenshotSha,
                    ElapsedMs: 3,
                    Metadata: new Dictionary<string, string>
                    {
                        ["browser_adapter_kind"] = "pass139-recording-browser-adapter",
                        ["browser_process_boundary"] = "fake_adapter_no_real_browser_for_aspnet_host_startup"
                    },
                    Warnings: [])));
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
                VerdictId: $"{request.ComparisonRequestId}:pass139-verdict",
                Score: score,
                Defects: [],
                EvidenceRefs: [$"pass139-comparison:{request.ComparisonRequestId}"],
                Passed: true);

            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Ok(
                new DesignConversionQaComparisonArtifact(
                    ArtifactId: $"{request.ComparisonRequestId}:pass139-artifact",
                    ComparisonRequestId: request.ComparisonRequestId,
                    Expected: request.Expected,
                    Actual: request.Actual,
                    Verdict: verdict,
                    DiffImageUri: $"artifact://pass139/diff/{Sha256Text(request.ComparisonRequestId)}.json",
                    DiffImageSha256: $"sha256:{Sha256Text(request.ComparisonRequestId + ":diff")}",
                    Metrics: new Dictionary<string, double>
                    {
                        ["visual_distance"] = 1.0 - score
                    },
                    EvidenceRefs: ["pass139-comparison-provider"])
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

    private sealed class RecordingTestOnlyVisualJudge(double score) : IDesignConversionVisualJudge
    {
        public List<DesignConversionVisualJudgeRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
            DesignConversionVisualJudgeRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var metadata = DesignConversionVisualAiProviderSelectionPolicy.MergeInto(
                new Dictionary<string, string>
                {
                    ["configured"] = bool.TrueString,
                    ["provider_boundary"] = DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                    [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker] = bool.TrueString,
                    ["external_provider_call"] = bool.FalseString,
                    ["external_ai_visual_judge_executed"] = bool.FalseString,
                    ["external_visual_ai_executed"] = bool.FalseString,
                    ["visual_ai_executed"] = bool.FalseString,
                    ["network_call_executed"] = bool.FalseString,
                    ["external_network_call_executed"] = bool.FalseString,
                    ["external_provider_called"] = bool.FalseString,
                    ["real_external_provider_executed"] = bool.FalseString,
                    ["product_ready"] = bool.FalseString,
                    ["estimated_cost_usd"] = "0"
                },
                DesignConversionVisualAiProviderSelectionPolicy.DescribeLocalStub(
                    "aspnet-test-only-visual-judge",
                    DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker));
            var evidenceRefs = new[]
            {
                $"aspnet-test-only-visual-judge:{request.JudgeRequestId}",
                DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                DesignConversionVisualQaBoundaryPolicy.VisualJudgeProviderBoundaryMarker,
                DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker,
                "external_ai_visual_judge_executed:false"
            };
            var verdict = new VisualVerdict(
                VerdictId: $"{request.JudgeRequestId}:aspnet-test-only-verdict",
                Score: score,
                Defects: [],
                EvidenceRefs: evidenceRefs,
                Passed: true);

            return Task.FromResult(OperationResult<DesignConversionVisualJudgeResult>.Ok(
                new DesignConversionVisualJudgeResult(
                    JudgeResultId: $"{request.JudgeRequestId}:aspnet-test-only-result",
                    JudgeRequestId: request.JudgeRequestId,
                    Status: "TEST_ONLY_VISUAL_JUDGE_VERDICT_RECEIVED",
                    IsBlocked: false,
                    Verdict: verdict,
                    Provider: new DesignConversionVisualJudgeProviderMetadata(
                        ProviderName: "aspnet-test-only-visual-judge",
                        EndpointHost: "local.test",
                        ModelId: "aspnet-test-only-visual-judge-v1",
                        ExternalCallExecuted: false,
                        Metadata: metadata),
                    EstimatedCostUsd: 0m,
                    TokenMetadata: new Dictionary<string, double>(),
                    EvidenceRefs: evidenceRefs,
                    Metadata: metadata)));
        }
    }

    private sealed class EnvironmentVariableScope : IDisposable
    {
        private readonly string _variableName;
        private readonly string? _previousValue;

        public EnvironmentVariableScope(string variableName, string? previousValue)
        {
            _variableName = variableName;
            _previousValue = previousValue;
        }

        public void Dispose()
            => Environment.SetEnvironmentVariable(_variableName, _previousValue);
    }

    private sealed class AspNetVisualCostReceiptJudge(decimal estimatedCostUsd) : IDesignConversionVisualJudge
    {
        private const int ReceiptAttemptCount = 2;

        public int InvocationCount { get; private set; }

        public Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
            DesignConversionVisualJudgeRequest request,
            CancellationToken cancellationToken = default)
        {
            InvocationCount++;
            var evidenceRefs = new[]
            {
                $"aspnet-visual-cost-judge:{request.JudgeRequestId}"
            };
            var metadata = BuildVerifiedServerOwnedReceiptMetadata(estimatedCostUsd);
            var verdict = new VisualVerdict(
                VerdictId: $"{request.JudgeRequestId}:aspnet-visual-cost-verdict",
                Score: 0.96,
                Defects: [],
                EvidenceRefs: evidenceRefs,
                Passed: true);

            return Task.FromResult(OperationResult<DesignConversionVisualJudgeResult>.Ok(
                new DesignConversionVisualJudgeResult(
                    JudgeResultId: $"{request.JudgeRequestId}:aspnet-visual-cost-result",
                    JudgeRequestId: request.JudgeRequestId,
                    Status: ExternalHttpDesignConversionVisualJudge.StatusOk,
                    IsBlocked: false,
                    Verdict: verdict,
                    Provider: new DesignConversionVisualJudgeProviderMetadata(
                        ProviderName: "aspnet-visual-cost-receipt-judge",
                        EndpointHost: "aspnet-visual-cost.local.test",
                        ModelId: "aspnet-visual-cost-receipt-v1",
                        ExternalCallExecuted: true,
                        Metadata: new Dictionary<string, string>(metadata, StringComparer.Ordinal)),
                    EstimatedCostUsd: estimatedCostUsd,
                    TokenMetadata: new Dictionary<string, double>(),
                    EvidenceRefs: evidenceRefs,
                    Metadata: new Dictionary<string, string>(metadata, StringComparer.Ordinal))));
        }

        private static Dictionary<string, string> BuildVerifiedServerOwnedReceiptMetadata(decimal totalCostUsd)
            => new(StringComparer.Ordinal)
            {
                ["cost_source"] = DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
                ["external_ai_visual_judge_executed"] = bool.TrueString,
                ["visual_ai_executed"] = bool.TrueString,
                [ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey] =
                    ReceiptAttemptCount.ToString(CultureInfo.InvariantCulture),
                [ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey] =
                    ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceValue,
                [ExternalHttpDesignConversionVisualJudge.ServerOwnedCostPerAttemptUsdKey] =
                    (totalCostUsd / ReceiptAttemptCount).ToString("0.0000", CultureInfo.InvariantCulture),
                [ExternalHttpDesignConversionVisualJudge.ServerOwnedTotalCostUsdKey] =
                    totalCostUsd.ToString("0.0000", CultureInfo.InvariantCulture),
                [ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingKey] =
                    ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingFixedPerAttempt
            };
    }

    private sealed class EchoingRealVisualJudgeHttpHandler : HttpMessageHandler
    {
        public int CallCount { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            var body = request.Content is null
                ? "{}"
                : await request.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            var judgeRequestId = root.GetProperty("judge_request_id").GetString();
            var requestId = root.GetProperty("request_id").GetString();
            var provider = root.GetProperty("provider").GetString();
            var model = root.GetProperty("model").GetString();

            var response = new
            {
                judge_request_id = judgeRequestId,
                request_id = requestId,
                provider,
                judge_result_id = "-result",
                verdict_id = $"{judgeRequestId}:trusted-network-verdict",
                score = 0.973,
                passed = true,
                confidence = 0.917,
                model_id = model,
                cost_estimate_usd = 0.0125,
                tokens = new { input = 88, output = 24, total = 112 },
                evidence_refs = new[] { "trusted-loopback-provider:evidence" },
                issues = new[]
                {
                    new
                    {
                        id = $"{judgeRequestId}:trusted-network-issue:1",
                        category = "visual-note",
                        severity = "info",
                        description = "Loopback provider returned one bounded visual note.",
                        x = 0,
                        y = 0,
                        width = 1,
                        height = 1
                    }
                }
            };

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(response),
                    Encoding.UTF8,
                    "application/json")
            };
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
                "PASS139 expects quality reached before a candidate is requested.",
                "PASS139_UNEXPECTED_CANDIDATE_REQUEST"));
        }
    }

    private sealed class UnexpectedFlow38QuarantineStore : IDesignConversionFlow38QuarantineStore
    {
        public Task<DesignConversionFlow38QuarantineStoreResult> AdmitAsync(
            DesignConversionFlow38QuarantinedSourceRow row,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException(
                "FLOW38 quarantine persistence is not expected in this host-startup boundary test.");
    }

    private sealed record ServerOwnedAspNetArtifactFixture(
        DesignConversionModelServerArtifactRef ArtifactRef,
        string LocalPath);

    private sealed record StartupConventionProbeMetadata(string Marker);

    private sealed record CallerShapedRequestFixture(
        string SourceKind,
        string LoopId,
        string RequestId,
        string BodyJson,
        string PrivateMarker,
        string Html,
        string Css,
        string DesignTreePayload,
        string OriginalTreePayload);
}
