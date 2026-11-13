// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using ExperimentsV2.LogicalFlow;
using Inference;
using LoRA;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchestratorModel.Logical;
using ProductHost.CapabilityServing;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;

[Collection(ProductHostEnvironmentCollection.Name)]
public sealed class ProductHostCapabilityServingTests
{
    [Fact(Timeout = 120_000)]
    public async Task RestoredCapability_IsServedThroughRealLoopbackHost_WithoutReadinessOverclaim()
    {
        using var fixture = ProductHostFixture.Create();
        await using var app = ProductHost.Program.CreateHost(
            [CapabilityServingHostConfiguration.StateRootArgumentName, fixture.StateRoot],
            fixture.ReplayVerificationCapability);

        await app.StartAsync();
        try
        {
            var address = ResolveSingleLoopbackAddress(app);
            using var client = new HttpClient { BaseAddress = address };

            using var readiness = await client.GetAsync(CapabilityServingAspNetEndpointMapper.ReadinessPath);
            Assert.Equal(HttpStatusCode.OK, readiness.StatusCode);
            Assert.Equal("false", Assert.Single(
                readiness.Headers.GetValues("X-XIIGen-Product-Ready")));
            Assert.Equal("false", Assert.Single(
                readiness.Headers.GetValues("X-XIIGen-Performs-Training")));
            using var readinessJson = JsonDocument.Parse(await readiness.Content.ReadAsStringAsync());
            var readinessRoot = readinessJson.RootElement;
            Assert.True(readinessRoot.GetProperty("capabilityServingReady").GetBoolean());
            Assert.False(readinessRoot.GetProperty("productReady").GetBoolean());
            Assert.False(readinessRoot.GetProperty("performsTraining").GetBoolean());
            Assert.False(readinessRoot.GetProperty("dpoReady").GetBoolean());
            Assert.False(readinessRoot.GetProperty("teacherReady").GetBoolean());
            Assert.Equal(fixture.Manifest.ScenarioId, fixture.ScenarioId);
            Assert.Equal(1, readinessRoot.GetProperty("scenarioCount").GetInt32());
            Assert.Equal(0, readinessRoot.GetProperty("assistantInvocationCount").GetInt64());
            Assert.Equal(
                "50.ProductHost->15.OrchestratorModel->18.CapabilityTraining",
                readinessRoot.GetProperty("entrypointTraceId").GetString());

            foreach (var unauthorizedScenario in new[]
                     {
                         T34LogicalFlowAdapter.ScenarioId,
                         SecurityVerification34FlowAdapter.ScenarioId
                     })
            {
                using var unauthorized = await client.PostAsJsonAsync(
                    CapabilityServingAspNetEndpointMapper.ExecutionPath,
                    new
                    {
                        userRequest = "must not reach the assistant",
                        scenarioId = unauthorizedScenario
                    });
                Assert.Equal(HttpStatusCode.NotFound, unauthorized.StatusCode);
                await AssertErrorCodeAsync(
                    unauthorized,
                    "CAPABILITY_SERVING_SCENARIO_NOT_AUTHORIZED");
            }

            using (var afterRejectedScenarios = await client.GetAsync(
                       CapabilityServingAspNetEndpointMapper.ReadinessPath))
            using (var afterRejectedJson = JsonDocument.Parse(
                       await afterRejectedScenarios.Content.ReadAsStringAsync()))
            {
                Assert.Equal(
                    0,
                    afterRejectedJson.RootElement
                        .GetProperty("assistantInvocationCount")
                        .GetInt64());
            }

            using var response = await client.PostAsJsonAsync(
                CapabilityServingAspNetEndpointMapper.ExecutionPath,
                new
                {
                    userRequest = "implement the backend endpoint",
                    scenarioId = fixture.Manifest.ScenarioId
                });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = json.RootElement;
            Assert.Equal(fixture.Manifest.FlowId, root.GetProperty("selectedFlowId").GetString());
            Assert.Equal(fixture.Manifest.ScenarioId, root.GetProperty("scenarioId").GetString());
            Assert.Contains(
                fixture.Manifest.OutputText,
                root.GetProperty("finalHumanResult").GetString(),
                StringComparison.Ordinal);
            Assert.Contains(
                "For your request: \"implement the backend endpoint\"",
                root.GetProperty("finalHumanResult").GetString(),
                StringComparison.Ordinal);
            Assert.Equal(
                "True",
                root.GetProperty("metrics").GetProperty("retrieval_unit_selected").GetString());
            Assert.False(root.GetProperty("productReady").GetBoolean());
            Assert.False(root.GetProperty("performsTraining").GetBoolean());
            Assert.False(root.GetProperty("dpoReady").GetBoolean());
            Assert.False(root.GetProperty("teacherReady").GetBoolean());

            using (var afterExecution = await client.GetAsync(
                       CapabilityServingAspNetEndpointMapper.ReadinessPath))
            using (var afterExecutionJson = JsonDocument.Parse(
                       await afterExecution.Content.ReadAsStringAsync()))
            {
                Assert.Equal(
                    1,
                    afterExecutionJson.RootElement
                        .GetProperty("assistantInvocationCount")
                        .GetInt64());
            }

            var removedModelServerRoutes = new (HttpMethod Method, string Path)[]
            {
                (HttpMethod.Post, "/api/design-conversion/evaluate"),
                (HttpMethod.Post, "/internal/design-conversion/evaluate"),
                (HttpMethod.Post, "/api/design-conversion/improvement-loop"),
                (HttpMethod.Get, "/api/design-conversion/jobs/test-job/visual-costs"),
                (HttpMethod.Get, "/api/design-conversion/status/test-job"),
                (HttpMethod.Get, "/api/design-conversion/dpo-usefulness"),
                (HttpMethod.Post, "/api/design-conversion/training-intake"),
                (HttpMethod.Post, "/api/design-conversion/flow38/quarantined-feedback"),
                (HttpMethod.Post, "/api/design-conversion/model-activation"),
                (HttpMethod.Post, "/api/design-conversion/human-approval/challenge"),
                (HttpMethod.Post, "/api/design-conversion/human-approval/approve"),
                (HttpMethod.Post, "/api/design-conversion/artifacts")
            };
            foreach (var route in removedModelServerRoutes)
            {
                using var request = new HttpRequestMessage(route.Method, route.Path);
                if (route.Method != HttpMethod.Get)
                    request.Content = new StringContent("{}", Encoding.UTF8, "application/json");
                using var removedRoute = await client.SendAsync(request);
                Assert.Equal(HttpStatusCode.NotFound, removedRoute.StatusCode);
            }
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Fact]
    public void CurrentSignedFixture_WithoutReplayVerificationCapability_FailsClosedBeforeStorage()
    {
        using var fixture = ProductHostFixture.Create();
        var policyBefore = File.ReadAllBytes(
            fixture.Manifest.PolicyCheckpointPath);
        var tokenizerBefore = File.ReadAllBytes(fixture.Manifest.TokenizerPath);
        var snapshotBefore = File.ReadAllBytes(fixture.SnapshotPath);

        var result = CapabilityServingCompositionRoot.Create(
            fixture.Configuration);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            "CAPABILITY_SERVING_REGISTRY_RESTORE_FAILED",
            result.ErrorCode);
        AssertSessionStorageAbsent(fixture.SessionDatabasePath);
        Assert.Equal(
            policyBefore,
            File.ReadAllBytes(fixture.Manifest.PolicyCheckpointPath));
        Assert.Equal(
            tokenizerBefore,
            File.ReadAllBytes(fixture.Manifest.TokenizerPath));
        Assert.Equal(snapshotBefore, File.ReadAllBytes(fixture.SnapshotPath));
    }

    [Fact]
    public void SignedPayloadMutation_WithMatchingVerifier_FailsClosedBeforeStorage()
    {
        using var fixture = ProductHostFixture.Create();
        fixture.RewriteSnapshot(snapshot => snapshot with
        {
            GeneratedCapabilities =
            [
                snapshot.GeneratedCapabilities[0] with
                {
                    OutputText = "MUTATED_AFTER_EXECUTABLE_ATTESTATION"
                }
            ]
        });

        var result = CapabilityServingCompositionRoot.Create(
            fixture.Configuration,
            fixture.ReplayVerificationCapability);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            "CAPABILITY_SERVING_REGISTRY_RESTORE_FAILED",
            result.ErrorCode);
        AssertSessionStorageAbsent(fixture.SessionDatabasePath);
    }

    [Fact]
    public void CurrentSignedFixture_WithForeignAuthorityVerifier_FailsClosedBeforeStorage()
    {
        using var fixture = ProductHostFixture.Create();
        using var foreignAuthorityFixture = ProductHostFixture.Create();

        var result = CapabilityServingCompositionRoot.Create(
            fixture.Configuration,
            foreignAuthorityFixture.ReplayVerificationCapability);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            "CAPABILITY_SERVING_REGISTRY_RESTORE_FAILED",
            result.ErrorCode);
        AssertSessionStorageAbsent(fixture.SessionDatabasePath);
    }

    [Theory]
    [InlineData("schema-v1")]
    [InlineData("stripped-executable-attestation")]
    public void LegacyOrStrippedExecutableManifest_CannotRestoreForExecution(
        string mutation)
    {
        using var fixture = ProductHostFixture.Create();
        fixture.RewriteSnapshot(snapshot => snapshot with
        {
            GeneratedCapabilities =
            [
                mutation switch
                {
                    "schema-v1" => snapshot.GeneratedCapabilities[0] with
                    {
                        SchemaVersion =
                            GeneratedCapabilityCheckpointManifest
                                .CurrentSchemaVersion - 1
                    },
                    "stripped-executable-attestation" =>
                        snapshot.GeneratedCapabilities[0] with
                        {
                            ExecutableManifestAttestation = null
                        },
                    _ => throw new InvalidOperationException(
                        "Unknown executable-manifest mutation.")
                }
            ]
        });

        var result = CapabilityServingCompositionRoot.Create(
            fixture.Configuration,
            fixture.ReplayVerificationCapability);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            "CAPABILITY_SERVING_REGISTRY_RESTORE_FAILED",
            result.ErrorCode);
        AssertSessionStorageAbsent(fixture.SessionDatabasePath);
    }

    [Fact(Timeout = 120_000)]
    public async Task GenerationEndpoint_UsesExactRestoredRuntimeSeedAndVerifiedProvenance()
    {
        const string prompt = "implement the backend endpoint";
        const int maxNewTokens = 3;
        const int seed = 1_234_567;
        using var fixture = ProductHostFixture.Create();
        var policyBefore = File.ReadAllBytes(fixture.Manifest.PolicyCheckpointPath);
        var tokenizerBefore = File.ReadAllBytes(fixture.Manifest.TokenizerPath);
        var snapshotBefore = File.ReadAllBytes(fixture.SnapshotPath);
        var expected = fixture.CreateIndependentGenerator().Generate(
            prompt,
            maxNewTokens,
            temperature: 0.8f,
            topK: Math.Min(50, fixture.Manifest.VocabSize),
            topP: 0.95f,
            seed,
            CancellationToken.None);
        await using var app = ProductHost.Program.CreateHost(
            [CapabilityServingHostConfiguration.StateRootArgumentName, fixture.StateRoot],
            fixture.ReplayVerificationCapability);

        await app.StartAsync();
        try
        {
            using var client = new HttpClient { BaseAddress = ResolveSingleLoopbackAddress(app) };
            string? firstText = null;
            for (var attempt = 0; attempt < 2; attempt++)
            {
                using var response = await client.PostAsJsonAsync(
                    CapabilityServingAspNetEndpointMapper.GenerationPath,
                    new
                    {
                        userRequest = prompt,
                        scenarioId = fixture.Manifest.ScenarioId,
                        maxNewTokens,
                        seed
                });
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(response.Headers.CacheControl?.NoStore);
                using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                var root = json.RootElement;
                var generatedText = root.GetProperty("generatedText").GetString();
                Assert.Equal(expected, generatedText);
                firstText ??= generatedText;
                Assert.Equal(firstText, generatedText);
                Assert.Equal(fixture.Manifest.ScenarioId, root.GetProperty("scenarioId").GetString());
                Assert.Equal(fixture.Manifest.FlowId, root.GetProperty("flowId").GetString());
                Assert.Equal(fixture.Manifest.UnitId, root.GetProperty("unitId").GetString());
                Assert.Equal(seed, root.GetProperty("seed").GetInt32());
                Assert.Equal(maxNewTokens, root.GetProperty("maxNewTokens").GetInt32());
                Assert.Equal(fixture.Manifest.TokenizerSha256, root.GetProperty("tokenizerSha256").GetString());
                Assert.Equal(fixture.Manifest.PolicyCheckpointSha256, root.GetProperty("policyCheckpointSha256").GetString());
                Assert.Equal(fixture.Manifest.RuntimeProvenanceSha256, root.GetProperty("runtimeProvenanceSha256").GetString());
                Assert.Equal(
                    CapabilityServingCompositionRoot.GenerationInferenceMode,
                    root.GetProperty("inferenceMode").GetString());
                Assert.True(root.GetProperty("capabilityServingReady").GetBoolean());
                Assert.False(root.GetProperty("productReady").GetBoolean());
                Assert.False(root.GetProperty("performsTraining").GetBoolean());
                Assert.False(root.GetProperty("dpoReady").GetBoolean());
                Assert.False(root.GetProperty("teacherReady").GetBoolean());
                Assert.Equal(
                    CapabilityServingCompositionRoot.ServingBoundary,
                    root.GetProperty("boundary").GetString());
            }

            using var readiness = await client.GetAsync(
                CapabilityServingAspNetEndpointMapper.ReadinessPath);
            using var readinessJson = JsonDocument.Parse(
                await readiness.Content.ReadAsStringAsync());
            Assert.Equal(
                0,
                readinessJson.RootElement.GetProperty("assistantInvocationCount").GetInt64());
            Assert.Equal(policyBefore, File.ReadAllBytes(fixture.Manifest.PolicyCheckpointPath));
            Assert.Equal(tokenizerBefore, File.ReadAllBytes(fixture.Manifest.TokenizerPath));
            Assert.Equal(snapshotBefore, File.ReadAllBytes(fixture.SnapshotPath));
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Fact(Timeout = 120_000)]
    public async Task GenerationBoundary_RejectsUnauthorizedLegacyAndContextOverflowBeforeInference()
    {
        using var fixture = ProductHostFixture.Create();
        var generationCalls = 0;
        var root = CapabilityServingCompositionRoot.CreateForTesting(
            fixture.Configuration,
            generationRunnerOverride: (_, _, _, _, _, _, _, _) =>
            {
                Interlocked.Increment(ref generationCalls);
                return "SHOULD_NOT_RUN";
            },
            replayVerificationCapability:
                fixture.ReplayVerificationCapability);
        Assert.True(root.IsSuccess, root.ErrorMessage);

        var unauthorized = await root.Value!.GenerateAsync(
            new CapabilityGenerationRequest(
                "valid prompt",
                "unauthorized-scenario",
                MaxNewTokens: 1,
                Seed: 7));
        Assert.False(unauthorized.IsSuccess);
        Assert.Equal("CAPABILITY_GENERATION_SCENARIO_NOT_AUTHORIZED", unauthorized.ErrorCode);
        Assert.Equal(0, generationCalls);

        foreach (var invalidLimit in new[] { 0, -1, 513 })
        {
            var invalid = await root.Value.GenerateAsync(
                new CapabilityGenerationRequest(
                    "valid prompt",
                    fixture.Manifest.ScenarioId,
                    invalidLimit,
                    Seed: 7));
            Assert.False(invalid.IsSuccess);
            Assert.Equal("CAPABILITY_GENERATION_MAX_NEW_TOKENS_INVALID", invalid.ErrorCode);
            Assert.Equal(0, generationCalls);
        }

        var overflow = await root.Value.GenerateAsync(
            new CapabilityGenerationRequest(
                new string('z', fixture.Manifest.ContextLen + 8),
                fixture.Manifest.ScenarioId,
                MaxNewTokens: 1,
                Seed: 7));
        Assert.False(overflow.IsSuccess);
        Assert.Equal("CAPABILITY_GENERATION_CONTEXT_BUDGET_EXCEEDED", overflow.ErrorCode);
        Assert.Equal(0, generationCalls);

        using var legacyFixture = ProductHostFixture.Create(useDefaultRuntimeValues: true);
        legacyFixture.RewriteSnapshot(snapshot =>
        {
            var legacy = legacyFixture.Manifest with
            {
                RopeTheta = null,
                RmsNormEps = null,
                TokenizerSha256 = null,
                PolicyCheckpointSha256 = null,
                RuntimeProvenanceSha256 = null
            };
            return WithExactBindings(snapshot, legacy);
        });
        var legacyRestore = CapabilityServingCompositionRoot.Create(
            legacyFixture.Configuration,
            legacyFixture.ReplayVerificationCapability);
        Assert.False(legacyRestore.IsSuccess);
        Assert.Equal(
            "CAPABILITY_SERVING_REGISTRY_RESTORE_FAILED",
            legacyRestore.ErrorCode);
        AssertSessionStorageAbsent(legacyFixture.SessionDatabasePath);
    }

    [Fact(Timeout = 120_000)]
    public async Task GenerationEndpoint_RejectsNonExactJsonAndOversizedBodies()
    {
        using var fixture = ProductHostFixture.Create();
        var generationCalls = 0;
        var composition = CapabilityServingCompositionRoot.CreateForTesting(
            fixture.Configuration,
            generationRunnerOverride: (_, _, _, _, _, _, _, _) =>
            {
                Interlocked.Increment(ref generationCalls);
                return "PARSER_MUST_NOT_REACH_GENERATION";
            },
            replayVerificationCapability:
                fixture.ReplayVerificationCapability);
        Assert.True(composition.IsSuccess, composition.ErrorMessage);
        await using var app = CreateLoopbackTestHost(composition.Value!);
        await app.StartAsync();
        try
        {
            using var client = new HttpClient { BaseAddress = ResolveSingleLoopbackAddress(app) };
            var invalidBodies = new List<string>
            {
                $$"""{"userRequest":"one","scenarioId":"{{fixture.ScenarioId}}","maxNewTokens":1,"seed":2,"extra":true}""",
                $$"""{"userRequest":"one","userRequest":"two","scenarioId":"{{fixture.ScenarioId}}","maxNewTokens":1,"seed":2}""",
                $$"""{"userRequest":"one","scenarioId":"{{fixture.ScenarioId}}","scenarioId":"duplicate","maxNewTokens":1,"seed":2}""",
                $$"""{"userRequest":"one","scenarioId":"{{fixture.ScenarioId}}","maxNewTokens":1,"maxNewTokens":2,"seed":2}""",
                $$"""{"userRequest":"one","scenarioId":"{{fixture.ScenarioId}}","maxNewTokens":1,"seed":2,"seed":3}""",
                $$"""{"scenarioId":"{{fixture.ScenarioId}}","maxNewTokens":1,"seed":2}""",
                $$"""{"userRequest":"one","maxNewTokens":1,"seed":2}""",
                $$"""{"userRequest":"one","scenarioId":"{{fixture.ScenarioId}}","seed":2}""",
                $$"""{"userRequest":"one","scenarioId":"{{fixture.ScenarioId}}","maxNewTokens":1}""",
                $$"""{"userRequest":1,"scenarioId":"{{fixture.ScenarioId}}","maxNewTokens":1,"seed":2}""",
                $$"""{"userRequest":"one","scenarioId":1,"maxNewTokens":1,"seed":2}""",
                $$"""{"userRequest":"one","scenarioId":"{{fixture.ScenarioId}}","maxNewTokens":"1","seed":2}""",
                $$"""{"userRequest":"one","scenarioId":"{{fixture.ScenarioId}}","maxNewTokens":1,"seed":"2"}""",
                $$"""{"userRequest":"one","scenarioId":"{{fixture.ScenarioId}}","maxNewTokens":1.0,"seed":2}""",
                $$"""{"userRequest":"one","scenarioId":"{{fixture.ScenarioId}}","maxNewTokens":1,"seed":2.0}""",
                $$"""{"userRequest":"one","scenarioId":"{{fixture.ScenarioId}}","maxNewTokens":2147483648,"seed":2}""",
                $$"""{"userRequest":"one","scenarioId":"{{fixture.ScenarioId}}","maxNewTokens":1,"seed":-2147483649}""",
                $$"""{"userRequest":null,"scenarioId":"{{fixture.ScenarioId}}","maxNewTokens":1,"seed":2}""",
                $$"""{"userRequest":"one","scenarioId":null,"maxNewTokens":1,"seed":2}""",
                $$"""{"userRequest":"one","scenarioId":"{{fixture.ScenarioId}}","maxNewTokens":null,"seed":2}""",
                $$"""{"userRequest":"one","scenarioId":"{{fixture.ScenarioId}}","maxNewTokens":1,"seed":null}""",
                $$"""{"UserRequest":"one","scenarioId":"{{fixture.ScenarioId}}","maxNewTokens":1,"seed":2}""",
                $$"""{"userRequest":"one","scenarioId":"{{fixture.ScenarioId}}","maxNewTokens":1e0,"seed":2}""",
                $$"""{"userRequest":"one",/* comments are forbidden */"scenarioId":"{{fixture.ScenarioId}}","maxNewTokens":1,"seed":2}""",
                $$"""{"userRequest":"one","scenarioId":"{{fixture.ScenarioId}}","maxNewTokens":1,"seed":2,}""",
                $$"""["not-an-object","{{fixture.ScenarioId}}",1,2]"""
            };
            invalidBodies.Add(
                "{\"userRequest\":" +
                new string('[', 17) +
                "\"one\"" +
                new string(']', 17) +
                $",\"scenarioId\":\"{fixture.ScenarioId}\",\"maxNewTokens\":1,\"seed\":2}}");
            foreach (var body in invalidBodies)
            {
                using var response = await PostRawJsonAsync(
                    client,
                    CapabilityServingAspNetEndpointMapper.GenerationPath,
                    body);
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                await AssertErrorCodeAsync(response, "CAPABILITY_GENERATION_JSON_INVALID");
            }
            Assert.Equal(0, Volatile.Read(ref generationCalls));

            using (var wrongContentType = new HttpRequestMessage(
                       HttpMethod.Post,
                       CapabilityServingAspNetEndpointMapper.GenerationPath))
            {
                wrongContentType.Content = new StringContent("{}", Encoding.UTF8, "text/plain");
                using var response = await client.SendAsync(wrongContentType);
                Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
                await AssertErrorCodeAsync(
                    response,
                    "CAPABILITY_GENERATION_CONTENT_TYPE_REQUIRED");
            }

            using var unauthorized = await client.PostAsJsonAsync(
                CapabilityServingAspNetEndpointMapper.GenerationPath,
                new
                {
                    userRequest = "valid prompt",
                    scenarioId = "unauthorized-scenario",
                    maxNewTokens = 1,
                    seed = 2
                });
            Assert.Equal(HttpStatusCode.NotFound, unauthorized.StatusCode);
            await AssertErrorCodeAsync(
                unauthorized,
                "CAPABILITY_GENERATION_SCENARIO_NOT_AUTHORIZED");

            foreach (var invalidLimit in new[] { 0, -1, 513 })
            {
                using var response = await client.PostAsJsonAsync(
                    CapabilityServingAspNetEndpointMapper.GenerationPath,
                    new
                    {
                        userRequest = "valid prompt",
                        scenarioId = fixture.Manifest.ScenarioId,
                        maxNewTokens = invalidLimit,
                        seed = 2
                    });
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                await AssertErrorCodeAsync(
                    response,
                    "CAPABILITY_GENERATION_MAX_NEW_TOKENS_INVALID");
            }

            using (var overflow = await client.PostAsJsonAsync(
                       CapabilityServingAspNetEndpointMapper.GenerationPath,
                       new
                       {
                           userRequest = new string('z', fixture.Manifest.ContextLen + 8),
                           scenarioId = fixture.Manifest.ScenarioId,
                           maxNewTokens = 1,
                           seed = 2
                       }))
            {
                Assert.Equal(HttpStatusCode.UnprocessableEntity, overflow.StatusCode);
                await AssertErrorCodeAsync(
                    overflow,
                    "CAPABILITY_GENERATION_CONTEXT_BUDGET_EXCEEDED");
            }

            var oversized = new byte[checked(
                (int)CapabilityServingAspNetEndpointMapper.MaximumRequestBodyBytes + 1)];
            using var fixedLength = new HttpRequestMessage(
                HttpMethod.Post,
                CapabilityServingAspNetEndpointMapper.GenerationPath)
            {
                Content = new ByteArrayContent(oversized)
            };
            fixedLength.Content.Headers.ContentType =
                MediaTypeHeaderValue.Parse("application/json");
            using var oversizedResponse = await client.SendAsync(fixedLength);
            Assert.Equal(HttpStatusCode.RequestEntityTooLarge, oversizedResponse.StatusCode);
            await AssertErrorCodeAsync(
                oversizedResponse,
                "CAPABILITY_GENERATION_BODY_TOO_LARGE");

            using var chunked = new HttpRequestMessage(
                HttpMethod.Post,
                CapabilityServingAspNetEndpointMapper.GenerationPath)
            {
                Content = new UnknownLengthContent(oversized)
            };
            chunked.Headers.TransferEncodingChunked = true;
            chunked.Content.Headers.ContentType =
                MediaTypeHeaderValue.Parse("application/json");
            using var chunkedResponse = await client.SendAsync(chunked);
            Assert.Equal(HttpStatusCode.RequestEntityTooLarge, chunkedResponse.StatusCode);
            await AssertErrorCodeAsync(
                chunkedResponse,
                "CAPABILITY_GENERATION_BODY_TOO_LARGE");
            Assert.Equal(0, Volatile.Read(ref generationCalls));
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Theory]
    [InlineData("aggregate-cancellation")]
    [InlineData("aggregate-out-of-memory")]
    [InlineData("argument-fatal-inner")]
    public async Task GenerationBoundary_ExceptionGraphEscapesAndReleasesScenarioGate(
        string mutation)
    {
        using var fixture = ProductHostFixture.Create();
        Exception injected = mutation switch
        {
            "aggregate-cancellation" => new AggregateException(
                new OperationCanceledException("nested cancellation"),
                new InvalidOperationException("ordinary cleanup failure")),
            "aggregate-out-of-memory" => new AggregateException(
                new OutOfMemoryException("nested fatal failure")),
            "argument-fatal-inner" => new ArgumentException(
                "TEXT_GENERATOR_CONTEXT_BUDGET_EXCEEDED",
                new OutOfMemoryException("nested fatal failure")),
            _ => throw new InvalidOperationException("Unknown exception mutation.")
        };
        var generationCalls = 0;
        var composition = CapabilityServingCompositionRoot.CreateForTesting(
            fixture.Configuration,
            generationRunnerOverride: (_, _, _, _, _, _, _, _) =>
            {
                if (Interlocked.Increment(ref generationCalls) == 1)
                    throw injected;
                return "safe-next-generation";
            },
            replayVerificationCapability:
                fixture.ReplayVerificationCapability);
        Assert.True(composition.IsSuccess, composition.ErrorMessage);
        var request = new CapabilityGenerationRequest(
            "implement the backend endpoint",
            fixture.Manifest.ScenarioId,
            MaxNewTokens: 1,
            Seed: 19);

        var escaped = await Assert.ThrowsAnyAsync<Exception>(
            () => composition.Value!.GenerateAsync(request));
        Assert.Same(injected, escaped);

        var next = await composition.Value!.GenerateAsync(request);
        Assert.True(next.IsSuccess, next.ErrorMessage);
        Assert.Equal("safe-next-generation", next.Value!.GeneratedText);
        Assert.Equal(2, Volatile.Read(ref generationCalls));
    }

    [Fact(Timeout = 120_000)]
    public async Task GenerationGate_CancelledWaiterNeverStartsAndCannotReleaseActiveInference()
    {
        using var fixture = ProductHostFixture.Create();
        using var firstStarted = new ManualResetEventSlim(false);
        using var releaseFirst = new ManualResetEventSlim(false);
        var generationCalls = 0;
        var root = CapabilityServingCompositionRoot.CreateForTesting(
            fixture.Configuration,
            generationRunnerOverride: (_, _, _, _, _, _, _, cancellationToken) =>
            {
                var call = Interlocked.Increment(ref generationCalls);
                if (call == 1)
                {
                    firstStarted.Set();
                    releaseFirst.Wait(cancellationToken);
                }
                return $"generated-{call}";
            },
            replayVerificationCapability:
                fixture.ReplayVerificationCapability);
        Assert.True(root.IsSuccess, root.ErrorMessage);

        var request = new CapabilityGenerationRequest(
            "implement the backend endpoint",
            fixture.Manifest.ScenarioId,
            MaxNewTokens: 1,
            Seed: 11);
        var first = root.Value!.GenerateAsync(request);
        Assert.True(firstStarted.Wait(TimeSpan.FromSeconds(10)));
        Assert.False(first.IsCompleted);

        using var secondCancellation = new CancellationTokenSource();
        var second = root.Value.GenerateAsync(request, secondCancellation.Token);
        Assert.False(second.IsCompleted);
        Assert.Equal(1, Volatile.Read(ref generationCalls));
        Assert.False(first.IsCompleted);
        secondCancellation.Cancel();
        var cancelled = await second;
        Assert.False(cancelled.IsSuccess);
        Assert.Equal("CANCELLED", cancelled.ErrorCode);
        Assert.Equal(1, Volatile.Read(ref generationCalls));
        Assert.False(first.IsCompleted);

        releaseFirst.Set();
        var completed = await first;
        Assert.True(completed.IsSuccess, completed.ErrorMessage);
        Assert.Equal("generated-1", completed.Value!.GeneratedText);

        var third = await root.Value.GenerateAsync(request);
        Assert.True(third.IsSuccess, third.ErrorMessage);
        Assert.Equal("generated-2", third.Value!.GeneratedText);
        Assert.Equal(2, Volatile.Read(ref generationCalls));
    }

    [Fact]
    public void InvalidOrOutsideRootState_FailsBeforeAHostCanBeCreated()
    {
        Assert.Throws<ArgumentException>(() => ProductHost.Program.CreateHost([]));

        using var missingSnapshot = ProductHostFixture.Create(writeSnapshot: false);
        Assert.Throws<ArgumentException>(() => ProductHost.Program.CreateHost(
            [CapabilityServingHostConfiguration.StateRootArgumentName, missingSnapshot.StateRoot]));

        using var outsideArtifacts = ProductHostFixture.Create(policyOutsideStateRoot: true);
        var exception = Assert.Throws<InvalidOperationException>(() => ProductHost.Program.CreateHost(
            [CapabilityServingHostConfiguration.StateRootArgumentName, outsideArtifacts.StateRoot]));
        Assert.Equal("CAPABILITY_SERVING_SNAPSHOT_INVALID", exception.Message);
    }

    [Fact]
    public void HostileEndpointConfiguration_IsRejectedBeforeHostCreation()
    {
        using var fixture = ProductHostFixture.Create();
        var stateArgs = new[]
        {
            CapabilityServingHostConfiguration.StateRootArgumentName,
            fixture.StateRoot
        };

        Assert.Throws<ArgumentException>(() => ProductHost.Program.CreateHost(
            [.. stateArgs, "--urls=http://0.0.0.0:5050"]));
        Assert.Throws<ArgumentException>(() => ProductHost.Program.CreateHost(
            [.. stateArgs, "--http_ports=5050"]));
        Assert.Throws<ArgumentException>(() => ProductHost.Program.CreateHost(
            [.. stateArgs, "--kestrel:Endpoints:Public:Url=http://0.0.0.0:5050"]));

        AssertHostileEnvironmentRejected(
            stateArgs,
            "ASPNETCORE_URLS",
            "http://0.0.0.0:5050");
        AssertHostileEnvironmentRejected(
            stateArgs,
            "DOTNET_HTTP_PORTS",
            "5050");
        AssertHostileEnvironmentRejected(
            stateArgs,
            "Kestrel__Endpoints__Public__Url",
            "http://0.0.0.0:5050");
    }

    [Fact(Timeout = 120_000)]
    public async Task HttpBoundary_EnforcesContentTypeStrictJsonAndBodyLimits()
    {
        using var fixture = ProductHostFixture.Create();
        await using var app = ProductHost.Program.CreateHost(
            [CapabilityServingHostConfiguration.StateRootArgumentName, fixture.StateRoot],
            fixture.ReplayVerificationCapability);
        await app.StartAsync();
        try
        {
            using var client = new HttpClient { BaseAddress = ResolveSingleLoopbackAddress(app) };
            var validJson = JsonSerializer.Serialize(new
            {
                userRequest = "implement the backend endpoint",
                scenarioId = fixture.ScenarioId
            });

            using (var validWithParameter = new HttpRequestMessage(
                       HttpMethod.Post,
                       CapabilityServingAspNetEndpointMapper.ExecutionPath))
            {
                validWithParameter.Content = new StringContent(validJson, Encoding.UTF8);
                validWithParameter.Content.Headers.ContentType =
                    MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
                using var response = await client.SendAsync(validWithParameter);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            using (var jsonp = new HttpRequestMessage(
                       HttpMethod.Post,
                       CapabilityServingAspNetEndpointMapper.ExecutionPath))
            {
                jsonp.Content = new StringContent(validJson, Encoding.UTF8);
                jsonp.Content.Headers.ContentType =
                    MediaTypeHeaderValue.Parse("application/jsonp");
                using var response = await client.SendAsync(jsonp);
                Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
                await AssertErrorCodeAsync(
                    response,
                    "CAPABILITY_SERVING_CONTENT_TYPE_REQUIRED");
            }

            using (var malformed = new HttpRequestMessage(
                       HttpMethod.Post,
                       CapabilityServingAspNetEndpointMapper.ExecutionPath))
            {
                malformed.Content = new StringContent(validJson, Encoding.UTF8);
                malformed.Content.Headers.Remove("Content-Type");
                Assert.True(malformed.Content.Headers.TryAddWithoutValidation(
                    "Content-Type",
                    "application/json; charset"));
                using var response = await client.SendAsync(malformed);
                Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
                await AssertErrorCodeAsync(
                    response,
                    "CAPABILITY_SERVING_CONTENT_TYPE_REQUIRED");
            }

            using (var unmapped = new HttpRequestMessage(
                       HttpMethod.Post,
                       CapabilityServingAspNetEndpointMapper.ExecutionPath))
            {
                unmapped.Content = new StringContent(
                    $$"""{"userRequest":"implement","scenarioId":"{{fixture.ScenarioId}}","extra":true}""",
                    Encoding.UTF8,
                    "application/json");
                using var response = await client.SendAsync(unmapped);
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                await AssertErrorCodeAsync(response, "CAPABILITY_SERVING_JSON_INVALID");
            }

            var exactJsonFailures = new[]
            {
                $$"""{"userRequest":"one","userRequest":"two","scenarioId":"{{fixture.ScenarioId}}"}""",
                $$"""{"userRequest":"one","scenarioId":"{{fixture.ScenarioId}}","scenarioId":"duplicate"}""",
                """{"userRequest":"missing scenario"}""",
                $$"""{"scenarioId":"{{fixture.ScenarioId}}"}""",
                $$"""{"userRequest":42,"scenarioId":"{{fixture.ScenarioId}}"}""",
                """{"userRequest":"wrong scenario type","scenarioId":42}""",
                $$"""{"UserRequest":"wrong case","scenarioId":"{{fixture.ScenarioId}}"}""",
                $$"""["not-an-object","{{fixture.ScenarioId}}"]"""
            };
            foreach (var invalidJson in exactJsonFailures)
            {
                using var invalid = new HttpRequestMessage(
                    HttpMethod.Post,
                    CapabilityServingAspNetEndpointMapper.ExecutionPath)
                {
                    Content = new StringContent(
                        invalidJson,
                        Encoding.UTF8,
                        "application/json")
                };
                using var response = await client.SendAsync(invalid);
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                await AssertErrorCodeAsync(
                    response,
                    "CAPABILITY_SERVING_JSON_INVALID");
            }

            using (var forbiddenControl = new HttpRequestMessage(
                       HttpMethod.Post,
                       CapabilityServingAspNetEndpointMapper.ExecutionPath))
            {
                forbiddenControl.Content = new StringContent(
                    $$"""{"userRequest":"unsafe\u0001payload","scenarioId":"{{fixture.ScenarioId}}"}""",
                    Encoding.UTF8,
                    "application/json");
                using var response = await client.SendAsync(forbiddenControl);
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                await AssertErrorCodeAsync(
                    response,
                    "CAPABILITY_SERVING_USER_REQUEST_INVALID");
            }

            var oversized = new byte[checked(
                (int)CapabilityServingAspNetEndpointMapper.MaximumRequestBodyBytes + 1)];
            using (var fixedLength = new HttpRequestMessage(
                       HttpMethod.Post,
                       CapabilityServingAspNetEndpointMapper.ExecutionPath))
            {
                fixedLength.Content = new ByteArrayContent(oversized);
                fixedLength.Content.Headers.ContentType =
                    MediaTypeHeaderValue.Parse("application/json");
                using var response = await client.SendAsync(fixedLength);
                Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
                await AssertErrorCodeAsync(response, "CAPABILITY_SERVING_BODY_TOO_LARGE");
            }

            using (var chunked = new HttpRequestMessage(
                       HttpMethod.Post,
                       CapabilityServingAspNetEndpointMapper.ExecutionPath))
            {
                chunked.Headers.TransferEncodingChunked = true;
                chunked.Content = new UnknownLengthContent(oversized);
                chunked.Content.Headers.ContentType =
                    MediaTypeHeaderValue.Parse("application/json");
                using var response = await client.SendAsync(chunked);
                Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
                await AssertErrorCodeAsync(response, "CAPABILITY_SERVING_BODY_TOO_LARGE");
            }
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Fact(Timeout = 120_000)]
    public async Task CancellationAndDownstreamScenarioFailure_AreStableAndDoNotMutateServingArtifacts()
    {
        using var fixture = ProductHostFixture.Create();
        var configuration = CapabilityServingHostConfiguration.ResolveRequired(
            [CapabilityServingHostConfiguration.StateRootArgumentName, fixture.StateRoot],
            _ => null);
        var root = CapabilityServingCompositionRoot.Create(
            configuration,
            fixture.ReplayVerificationCapability);
        Assert.True(root.IsSuccess, root.ErrorMessage);
        var policyBefore = File.ReadAllBytes(fixture.Manifest.PolicyCheckpointPath);
        var tokenizerBefore = File.ReadAllBytes(fixture.Manifest.TokenizerPath);
        var snapshotBefore = File.ReadAllBytes(fixture.SnapshotPath);

        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        var cancelledResult = await root.Value!.ExecuteAsync(
            new CapabilityServingRequest("implement the backend endpoint", fixture.Manifest.ScenarioId),
            cancelled.Token);
        Assert.False(cancelledResult.IsSuccess);
        Assert.Equal("CANCELLED", cancelledResult.ErrorCode);

        var downstreamRoot = CapabilityServingCompositionRoot.CreateForTesting(
            configuration,
            runnerOverride: (_, _, _, _, _) =>
                Task.FromResult(OperationResult<LogicalAssistantRunResult>.Fail(
                    "Injected downstream failure.",
                    "ORCHESTRATOR_FAILED")),
            replayVerificationCapability:
                fixture.ReplayVerificationCapability);
        Assert.True(downstreamRoot.IsSuccess, downstreamRoot.ErrorMessage);
        var downstreamResult = await downstreamRoot.Value!.ExecuteAsync(
            new CapabilityServingRequest(
                "implement the backend endpoint",
                fixture.Manifest.ScenarioId));
        Assert.False(downstreamResult.IsSuccess);
        Assert.Equal("ORCHESTRATOR_FAILED", downstreamResult.ErrorCode);
        Assert.Equal(1, downstreamRoot.Value.AssistantInvocationCount);

        await using var app = ProductHost.Program.CreateHost(
            [CapabilityServingHostConfiguration.StateRootArgumentName, fixture.StateRoot],
            fixture.ReplayVerificationCapability);
        await app.StartAsync();
        try
        {
            using var client = new HttpClient
            {
                BaseAddress = ResolveSingleLoopbackAddress(app)
            };
            using var response = await client.PostAsJsonAsync(
                CapabilityServingAspNetEndpointMapper.ExecutionPath,
                new
                {
                    userRequest = "implement the backend endpoint",
                    scenarioId = "unknown-production-scenario"
                });
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(
                "CAPABILITY_SERVING_SCENARIO_NOT_AUTHORIZED",
                json.RootElement.GetProperty("errorCode").GetString());
        }
        finally
        {
            await app.StopAsync();
        }

        Assert.Equal(policyBefore, File.ReadAllBytes(fixture.Manifest.PolicyCheckpointPath));
        Assert.Equal(tokenizerBefore, File.ReadAllBytes(fixture.Manifest.TokenizerPath));
        Assert.Equal(snapshotBefore, File.ReadAllBytes(fixture.SnapshotPath));
        Assert.Empty(Directory.GetFiles(
            fixture.CheckpointRoot,
            TeachingRegistryPersistence.GeneratedCapabilityManifestFileName,
            SearchOption.AllDirectories));
    }

    [Fact]
    public void ServingSnapshotCaps_FailClosedBeforeSessionStorageIsCreated()
    {
        var mutations = new Func<TeachingRegistrySnapshot, TeachingRegistrySnapshot>[]
        {
            snapshot => snapshot with
            {
                Units = Enumerable.Repeat(
                    snapshot.Units[0],
                    CapabilityServingCompositionRoot.MaximumRegistryEntries + 1).ToArray()
            },
            snapshot => snapshot with
            {
                GeneratedCapabilities =
                [
                    snapshot.GeneratedCapabilities[0] with
                    {
                        OutputText = new string('x', 32_769)
                    }
                ]
            },
            snapshot => snapshot with
            {
                GeneratedCapabilities =
                [
                    snapshot.GeneratedCapabilities[0] with
                    {
                        HiddenDim = CapabilityServingCompositionRoot.MaximumHiddenDimension + 1
                    }
                ]
            },
            snapshot => snapshot with
            {
                GeneratedCapabilities =
                [
                    snapshot.GeneratedCapabilities[0] with
                    {
                        VocabSize = CapabilityServingCompositionRoot.MaximumVocabularySize,
                        HiddenDim = 128,
                        NumHeads = 8,
                        NumKvHeads = 8,
                        FfnHiddenDim = 256
                    }
                ]
            },
            snapshot => snapshot with
            {
                GeneratedCapabilities =
                [
                    snapshot.GeneratedCapabilities[0] with
                    {
                        LoraTargetModules = Enumerable.Repeat(
                            "q_proj",
                            CapabilityServingCompositionRoot.MaximumLoraTargets + 1).ToArray()
                    }
                ]
            },
            snapshot => snapshot with
            {
                CheckpointPaths = [.. snapshot.CheckpointPaths, snapshot.CheckpointPaths[0]]
            }
        };

        foreach (var mutation in mutations)
        {
            using var fixture = ProductHostFixture.Create();
            fixture.RewriteSnapshot(mutation);
            var result = CapabilityServingCompositionRoot.Create(fixture.Configuration);
            Assert.False(result.IsSuccess);
            Assert.Equal("CAPABILITY_SERVING_SNAPSHOT_INVALID", result.ErrorCode);
            Assert.False(File.Exists(fixture.SessionDatabasePath));
        }

        using (var oversizedSnapshot = ProductHostFixture.Create())
        {
            using (var stream = new FileStream(
                       oversizedSnapshot.SnapshotPath,
                       FileMode.Truncate,
                       FileAccess.Write,
                       FileShare.None))
            {
                stream.SetLength(CapabilityServingCompositionRoot.MaximumSnapshotBytes + 1);
            }
            ProductHostFixture.SecureFixtureFile(oversizedSnapshot.SnapshotPath);
            var result = CapabilityServingCompositionRoot.Create(
                oversizedSnapshot.Configuration);
            Assert.False(result.IsSuccess);
            Assert.Equal("CAPABILITY_SERVING_SNAPSHOT_INVALID", result.ErrorCode);
            Assert.False(File.Exists(oversizedSnapshot.SessionDatabasePath));
        }

        using (var unmappedSnapshot = ProductHostFixture.Create())
        {
            var json = File.ReadAllText(unmappedSnapshot.SnapshotPath);
            File.WriteAllText(
                unmappedSnapshot.SnapshotPath,
                "{\"unexpected\":true," + json.TrimStart()[1..]);
            ProductHostFixture.SecureFixtureFile(unmappedSnapshot.SnapshotPath);
            var result = CapabilityServingCompositionRoot.Create(
                unmappedSnapshot.Configuration);
            Assert.False(result.IsSuccess);
            Assert.Equal("CAPABILITY_SERVING_SNAPSHOT_INVALID", result.ErrorCode);
            Assert.False(File.Exists(unmappedSnapshot.SessionDatabasePath));
        }
    }

    [Theory]
    [InlineData("omitted-unit")]
    [InlineData("candidate")]
    [InlineData("rejected")]
    [InlineData("mismatched-unit")]
    [InlineData("mismatched-manifest")]
    [InlineData("mismatched-flow")]
    [InlineData("forbidden-payload-control")]
    public void ExecutableSnapshotBinding_FailsClosedBeforeSessionStorage(
        string mutation)
    {
        using var fixture = ProductHostFixture.Create();
        fixture.RewriteSnapshot(snapshot =>
        {
            var manifest = snapshot.GeneratedCapabilities[0];
            return mutation switch
            {
                "omitted-unit" => snapshot with { Units = [] },
                "candidate" => snapshot with
                {
                    Units =
                    [
                        snapshot.Units[0] with
                        {
                            TrustState = nameof(LogicalUnitTrustState.Candidate)
                        }
                    ],
                    GeneratedCapabilities =
                    [
                        manifest with
                        {
                            TrustState = nameof(LogicalUnitTrustState.Candidate)
                        }
                    ]
                },
                "rejected" => snapshot with
                {
                    Units =
                    [
                        snapshot.Units[0] with
                        {
                            TrustState = nameof(LogicalUnitTrustState.Rejected)
                        }
                    ],
                    GeneratedCapabilities =
                    [
                        manifest with
                        {
                            TrustState = nameof(LogicalUnitTrustState.Rejected)
                        }
                    ]
                },
                "mismatched-unit" => snapshot with
                {
                    Units =
                    [
                        snapshot.Units[0] with { DomainTag = "BUG" }
                    ]
                },
                "mismatched-manifest" => snapshot with
                {
                    GeneratedCapabilities =
                    [
                        manifest with { UnitRole = "other-generated-role" }
                    ]
                },
                "mismatched-flow" => snapshot with
                {
                    Flows =
                    [
                        snapshot.Flows[0] with { SubPhaseId = "4.2" }
                    ]
                },
                "forbidden-payload-control" => snapshot with
                {
                    GeneratedCapabilities =
                    [
                        manifest with { OutputText = "unsafe\u0001payload" }
                    ]
                },
                _ => throw new InvalidOperationException("Unknown test mutation.")
            };
        });

        var result = CapabilityServingCompositionRoot.Create(fixture.Configuration);

        Assert.False(result.IsSuccess);
        Assert.Equal("CAPABILITY_SERVING_SNAPSHOT_INVALID", result.ErrorCode);
        Assert.False(File.Exists(fixture.SessionDatabasePath));
    }

    [Fact]
    public void AggregateModelAndArtifactBudgets_FailBeforeModelOrDatabaseOpen()
    {
        using (var modelBudget = ProductHostFixture.Create())
        {
            modelBudget.RewriteSnapshot(snapshot =>
            {
                var large = snapshot.GeneratedCapabilities[0] with
                {
                    VocabSize = 1_000,
                    HiddenDim = 1_024,
                    NumLayers = 4,
                    NumHeads = 8,
                    NumKvHeads = 8,
                    FfnHiddenDim = 4_096
                };
                var second = CloneBinding(large, "aggregate");
                return WithExactBindings(snapshot, large, second);
            });

            var result = CapabilityServingCompositionRoot.Create(
                modelBudget.Configuration);

            Assert.False(result.IsSuccess);
            Assert.Equal("CAPABILITY_SERVING_SNAPSHOT_INVALID", result.ErrorCode);
            Assert.False(File.Exists(modelBudget.SessionDatabasePath));
        }

        using (var artifactBudget = ProductHostFixture.Create())
        {
            var firstPolicy = Path.Combine(
                artifactBudget.CheckpointRoot,
                "aggregate-policy-a.bin");
            var secondPolicy = Path.Combine(
                artifactBudget.CheckpointRoot,
                "aggregate-policy-b.bin");
            var individualLength =
                (CapabilityServingCompositionRoot.MaximumAggregateArtifactBytes / 2) + 1;
            foreach (var path in new[] { firstPolicy, secondPolicy })
            {
                using (var stream = new FileStream(
                           path,
                           FileMode.CreateNew,
                           FileAccess.Write,
                           FileShare.None))
                {
                    stream.SetLength(individualLength);
                }
                ProductHostFixture.SecureFixtureFile(path);
            }

            artifactBudget.RewriteSnapshot(snapshot =>
            {
                var first = snapshot.GeneratedCapabilities[0] with
                {
                    PolicyCheckpointPath = firstPolicy
                };
                var second = CloneBinding(first, "aggregate") with
                {
                    PolicyCheckpointPath = secondPolicy
                };
                return WithExactBindings(snapshot, first, second) with
                {
                    CheckpointPaths =
                    [
                        firstPolicy,
                        artifactBudget.Manifest.TokenizerPath,
                        secondPolicy
                    ]
                };
            });

            var artifactsFullyVerified = false;
            var result = CapabilityServingCompositionRoot.CreateForTesting(
                artifactBudget.Configuration,
                afterArtifactsLeased: _ => artifactsFullyVerified = true);

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "CAPABILITY_SERVING_ARTIFACT_BUDGET_EXCEEDED",
                result.ErrorCode);
            Assert.False(artifactsFullyVerified);
            Assert.False(File.Exists(artifactBudget.SessionDatabasePath));
        }
    }

    [Fact]
    public void VocabularyDominantModel_UsesCompleteParameterBudgetBeforeArtifactsOrStorage()
    {
        using var fixture = ProductHostFixture.Create();
        fixture.RewriteSnapshot(snapshot => snapshot with
        {
            GeneratedCapabilities =
            [
                snapshot.GeneratedCapabilities[0] with
                {
                    VocabSize = 1_000_000,
                    HiddenDim = 64,
                    NumLayers = 1,
                    NumHeads = 8,
                    NumKvHeads = 8,
                    FfnHiddenDim = 64,
                    LoraRank = 1,
                    LoraTargetModules = ["q_proj"]
                }
            ]
        });
        var artifactsFullyVerified = false;

        var result = CapabilityServingCompositionRoot.CreateForTesting(
            fixture.Configuration,
            afterArtifactsLeased: _ => artifactsFullyVerified = true);

        Assert.False(result.IsSuccess);
        Assert.Equal("CAPABILITY_SERVING_SNAPSHOT_INVALID", result.ErrorCode);
        Assert.False(artifactsFullyVerified);
        AssertSessionStorageAbsent(fixture.SessionDatabasePath);
    }

    [Theory]
    [InlineData("noncanonical")]
    [InlineData("duplicate")]
    public void InvalidLoraTargetIdentity_FailsBeforeArtifactsOrStorage(string mutation)
    {
        using var fixture = ProductHostFixture.Create();
        fixture.RewriteSnapshot(snapshot => snapshot with
        {
            GeneratedCapabilities =
            [
                snapshot.GeneratedCapabilities[0] with
                {
                    LoraTargetModules = mutation switch
                    {
                        "noncanonical" => ["Q_proj"],
                        "duplicate" => ["q_proj", "q_proj"],
                        _ => throw new InvalidOperationException("Unknown test mutation.")
                    }
                }
            ]
        });
        var artifactsFullyVerified = false;

        var result = CapabilityServingCompositionRoot.CreateForTesting(
            fixture.Configuration,
            afterArtifactsLeased: _ => artifactsFullyVerified = true);

        Assert.False(result.IsSuccess);
        Assert.Equal("CAPABILITY_SERVING_SNAPSHOT_INVALID", result.ErrorCode);
        Assert.False(artifactsFullyVerified);
        AssertSessionStorageAbsent(fixture.SessionDatabasePath);
    }

    [Theory]
    [InlineData("missing-generated-flow")]
    [InlineData("deterministic-selection")]
    [InlineData("learned-selection")]
    [InlineData("wrong-unit")]
    public async Task SuccessfulLookingRunnerWithoutAuthorizedGeneratedMetrics_FailsBoundary(
        string mutation)
    {
        using var fixture = ProductHostFixture.Create();
        var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["generated_capability_flow"] = bool.TrueString,
            ["retrieval_unit_selected"] = bool.TrueString,
            ["learned_unit_selected"] = bool.FalseString,
            ["retrieval_unit_id"] = fixture.Manifest.UnitId
        };
        switch (mutation)
        {
            case "missing-generated-flow":
                metrics.Remove("generated_capability_flow");
                break;
            case "deterministic-selection":
                metrics["retrieval_unit_selected"] = bool.FalseString;
                break;
            case "learned-selection":
                metrics["learned_unit_selected"] = bool.TrueString;
                break;
            case "wrong-unit":
                metrics["retrieval_unit_id"] = "deterministic-fallback-unit";
                break;
            default:
                throw new InvalidOperationException("Unknown test mutation.");
        }

        var root = CapabilityServingCompositionRoot.CreateForTesting(
            fixture.Configuration,
            runnerOverride: (_, scenarioId, flags, _, _) =>
                Task.FromResult(OperationResult<LogicalAssistantRunResult>.Ok(
                    new LogicalAssistantRunResult(
                        scenarioId!,
                        fixture.Manifest.FlowId,
                        "PASS",
                        "DETERMINISTIC_FALLBACK_MUST_NOT_BE_SERVED",
                        flags!,
                        metrics,
                        []))),
            replayVerificationCapability:
                fixture.ReplayVerificationCapability);
        Assert.True(root.IsSuccess, root.ErrorMessage);

        var result = await root.Value!.ExecuteAsync(
            new CapabilityServingRequest(
                "implement the backend endpoint",
                fixture.Manifest.ScenarioId));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            "CAPABILITY_SERVING_EXECUTABLE_BOUNDARY_VIOLATION",
            result.ErrorCode);
        Assert.DoesNotContain(
            "DETERMINISTIC_FALLBACK_MUST_NOT_BE_SERVED",
            result.ErrorMessage,
            StringComparison.Ordinal);
    }

    [Fact(Timeout = 120_000)]
    public async Task MultilinePayloadAndAuthoritativeHtmlBlock_ArePreservedExactly()
    {
        const string request =
            "Implement this component:\r\n\t<section>\r\n\t</section>";
        const string authoritative =
            "```html\r\n<section>\r\n\t<h1>Generated</h1>\r\n</section>\r\n```";
        using var fixture = ProductHostFixture.Create(
            trainingText: request,
            outputText: authoritative,
            originalPrompt: request);
        await using var app = ProductHost.Program.CreateHost(
            [CapabilityServingHostConfiguration.StateRootArgumentName, fixture.StateRoot],
            fixture.ReplayVerificationCapability);
        await app.StartAsync();
        try
        {
            using var client = new HttpClient
            {
                BaseAddress = ResolveSingleLoopbackAddress(app)
            };
            using var response = await client.PostAsJsonAsync(
                CapabilityServingAspNetEndpointMapper.ExecutionPath,
                new
                {
                    userRequest = request,
                    scenarioId = fixture.Manifest.ScenarioId
                });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var json = JsonDocument.Parse(
                await response.Content.ReadAsStringAsync());
            Assert.StartsWith(
                authoritative,
                json.RootElement.GetProperty("finalHumanResult").GetString(),
                StringComparison.Ordinal);
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Fact]
    public void OversizedArtifactAndSameObjectMutation_FailClosedWithoutStorageLeak()
    {
        using (var oversizedArtifact = ProductHostFixture.Create())
        {
            using (var stream = new FileStream(
                       oversizedArtifact.Manifest.TokenizerPath,
                       FileMode.Open,
                       FileAccess.Write,
                       FileShare.None))
            {
                stream.SetLength(CapabilityServingCompositionRoot.MaximumTokenizerBytes + 1);
            }
            var result = CapabilityServingCompositionRoot.Create(
                oversizedArtifact.Configuration);
            Assert.False(result.IsSuccess);
            Assert.Equal("CAPABILITY_SERVING_ARTIFACT_INVALID", result.ErrorCode);
            Assert.False(File.Exists(oversizedArtifact.SessionDatabasePath));
        }

        using (var unmutated = ProductHostFixture.Create())
        {
            var unmutatedControl =
                CapabilityServingCompositionRoot.CreateForTesting(
                    unmutated.Configuration,
                    replayVerificationCapability:
                        unmutated.ReplayVerificationCapability);
            Assert.True(
                unmutatedControl.IsSuccess,
                unmutatedControl.ErrorMessage);
        }

        using (var mutated = ProductHostFixture.Create())
        {
            TeachingRegistrySnapshot? exactObject = null;
            var result = CapabilityServingCompositionRoot.CreateForTesting(
                mutated.Configuration,
                afterArtifactsLeased: snapshot =>
                {
                    exactObject = snapshot;
                    snapshot.Units[0] = snapshot.Units[0] with
                    {
                        UnitId = "mutated-after-validation"
                    };
                },
                replayVerificationCapability:
                    mutated.ReplayVerificationCapability);
            Assert.NotNull(exactObject);
            Assert.False(result.IsSuccess);
            Assert.Equal("CAPABILITY_SERVING_REGISTRY_RESTORE_FAILED", result.ErrorCode);
            Assert.False(File.Exists(mutated.SessionDatabasePath));
        }
    }

    [Fact]
    public void ArtifactPathReplacement_IsBlockedOrDetectedWhileLeaseIsHeld()
    {
        using var fixture = ProductHostFixture.Create();
        var replacement = Path.Combine(fixture.CheckpointRoot, "replacement-tokenizer.json");
        File.Copy(fixture.Manifest.TokenizerPath, replacement);
        ProductHostFixture.SecureFixtureFile(replacement);
        var replacementBlocked = false;
        var replacementCompleted = false;

        var result = CapabilityServingCompositionRoot.CreateForTesting(
            fixture.Configuration,
            afterArtifactsLeased: _ =>
            {
                try
                {
                    File.Move(
                        replacement,
                        fixture.Manifest.TokenizerPath,
                        overwrite: true);
                    replacementCompleted = true;
                }
                catch (Exception exception)
                    when (exception is IOException or UnauthorizedAccessException)
                {
                    replacementBlocked = true;
                }
            },
            replayVerificationCapability:
                fixture.ReplayVerificationCapability);

        if (OperatingSystem.IsWindows())
        {
            Assert.True(
                replacementBlocked,
                $"replacementCompleted={replacementCompleted}; result={result.ErrorCode}:{result.ErrorMessage}");
            Assert.False(replacementCompleted);
            Assert.True(result.IsSuccess, result.ErrorMessage);
        }
        else
        {
            Assert.True(replacementCompleted);
            Assert.False(result.IsSuccess);
            Assert.Equal("CAPABILITY_SERVING_ARTIFACT_INVALID", result.ErrorCode);
        }
    }

    [Fact]
    public void UnixPermissionBoundary_RejectsGroupOrOtherAccess()
    {
        if (OperatingSystem.IsWindows())
            return;

        using var fixture = ProductHostFixture.Create();
        File.SetUnixFileMode(
            fixture.SnapshotPath,
            UnixFileMode.UserRead |
            UnixFileMode.UserWrite |
            UnixFileMode.GroupRead);
        var result = CapabilityServingCompositionRoot.Create(fixture.Configuration);
        Assert.False(result.IsSuccess);
        Assert.Equal("CAPABILITY_SERVING_SNAPSHOT_INVALID", result.ErrorCode);
        Assert.False(File.Exists(fixture.SessionDatabasePath));
    }

    [Fact]
    public void ReparseCheckpointArtifact_IsRejectedBeforeRestore()
    {
        if (OperatingSystem.IsWindows())
            return;

        using var fixture = ProductHostFixture.Create();
        var outside = Path.Combine(
            Path.GetTempPath(),
            $"xiigen-product-host-link-target-{Guid.NewGuid():N}.json");
        File.Copy(fixture.Manifest.TokenizerPath, outside);
        ProductHostFixture.SecureFixtureFile(outside);
        try
        {
            File.Delete(fixture.Manifest.TokenizerPath);
            File.CreateSymbolicLink(fixture.Manifest.TokenizerPath, outside);
            var result = CapabilityServingCompositionRoot.Create(fixture.Configuration);
            Assert.False(result.IsSuccess);
            Assert.Equal("CAPABILITY_SERVING_SNAPSHOT_INVALID", result.ErrorCode);
            Assert.False(File.Exists(fixture.SessionDatabasePath));
        }
        finally
        {
            File.Delete(outside);
        }
    }

    [Fact]
    public void WindowsAclBoundary_RejectsNonOwnerGrant()
    {
        if (!OperatingSystem.IsWindows())
            return;

        using var fixture = ProductHostFixture.Create();
        var info = new FileInfo(fixture.SnapshotPath);
        var security = FileSystemAclExtensions.GetAccessControl(info);
        security.AddAccessRule(new FileSystemAccessRule(
            new SecurityIdentifier(WellKnownSidType.WorldSid, null),
            FileSystemRights.ReadData,
            AccessControlType.Allow));
        FileSystemAclExtensions.SetAccessControl(info, security);

        var result = CapabilityServingCompositionRoot.Create(fixture.Configuration);
        Assert.False(result.IsSuccess);
        Assert.Equal("CAPABILITY_SERVING_SNAPSHOT_INVALID", result.ErrorCode);
        Assert.False(File.Exists(fixture.SessionDatabasePath));
    }

    private static Uri ResolveSingleLoopbackAddress(Microsoft.AspNetCore.Builder.WebApplication app)
    {
        var server = app.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>()?.Addresses
            ?? throw new InvalidOperationException("ProductHost did not publish an address.");
        var published = Assert.Single(addresses);
        var address = new Uri(published, UriKind.Absolute);
        Assert.Equal(Uri.UriSchemeHttp, address.Scheme);
        Assert.True(IPAddress.TryParse(address.Host, out var ipAddress));
        Assert.True(IPAddress.IsLoopback(ipAddress));
        Assert.True(address.Port > 0);
        return address;
    }

    private static WebApplication CreateLoopbackTestHost(
        CapabilityServingCompositionRoot compositionRoot)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(ProductHost.Program).Assembly.FullName
        });
        builder.WebHost.UseKestrel(options =>
        {
            options.Limits.MaxRequestBodySize =
                CapabilityServingAspNetEndpointMapper.MaximumRequestBodyBytes + 1;
            options.Listen(IPAddress.Loopback, 0);
        });
        builder.Logging.ClearProviders();

        var app = builder.Build();
        app.ConfigureCapabilityServing(compositionRoot);
        return app;
    }

    private static async Task AssertErrorCodeAsync(HttpResponseMessage response, string expected)
    {
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(expected, json.RootElement.GetProperty("errorCode").GetString());
    }

    private static async Task<HttpResponseMessage> PostRawJsonAsync(
        HttpClient client,
        string path,
        string body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        return await client.SendAsync(request);
    }

    private static void AssertHostileEnvironmentRejected(
        IReadOnlyList<string> stateArgs,
        string name,
        string value)
    {
        var previous = Environment.GetEnvironmentVariable(name);
        try
        {
            Environment.SetEnvironmentVariable(name, value);
            Assert.Throws<ArgumentException>(() => ProductHost.Program.CreateHost([.. stateArgs]));
        }
        finally
        {
            Environment.SetEnvironmentVariable(name, previous);
        }
    }

    private static GeneratedCapabilityCheckpointManifest CloneBinding(
        GeneratedCapabilityCheckpointManifest source,
        string suffix)
        => source with
        {
            UnitId = $"{source.UnitId}-{suffix}",
            FlowId = $"{source.FlowId}-{suffix}",
            ScenarioId = $"{source.ScenarioId}-{suffix}",
            ExampleId = $"{source.ExampleId}-{suffix}"
        };

    private static TeachingRegistrySnapshot WithExactBindings(
        TeachingRegistrySnapshot snapshot,
        params GeneratedCapabilityCheckpointManifest[] manifests)
        => snapshot with
        {
            Units = manifests.Select((manifest, index) =>
                new TeachingUnitRegistryEntry(
                    manifest.UnitId,
                    manifest.UnitRole,
                    manifest.DomainTag,
                    manifest.Version,
                    manifest.TrustState,
                    IsLearned: false,
                    typeof(string).AssemblyQualifiedName!,
                    typeof(string).AssemblyQualifiedName!,
                    RegistrationOrder:
                        manifest.UnitRegistrationOrder ??
                        1_000 + (index * 2),
                    TeachingRegistrySnapshotKinds.GeneratedCapabilityUnit)).ToArray(),
            Flows = manifests.Select((manifest, index) =>
                new TeachingFlowRegistryEntry(
                    manifest.FlowId,
                    manifest.ScenarioId,
                    manifest.Version,
                    typeof(string).AssemblyQualifiedName!,
                    manifest.PhaseId,
                    manifest.SubPhaseId,
                    manifest.TaskType,
                    manifest.DomainTags,
                    RegistrationOrder:
                        manifest.FlowRegistrationOrder ??
                        1_001 + (index * 2),
                    TeachingRegistrySnapshotKinds.GeneratedCapabilityFlow)).ToArray(),
            GeneratedCapabilities = manifests
        };

    private static void AssertSessionStorageAbsent(string databasePath)
    {
        Assert.False(File.Exists(databasePath));
        Assert.False(File.Exists(databasePath + "-wal"));
        Assert.False(File.Exists(databasePath + "-shm"));
        Assert.False(File.Exists(databasePath + "-journal"));
    }

    private sealed class UnknownLengthContent(byte[] bytes) : HttpContent
    {
        protected override Task SerializeToStreamAsync(
            Stream stream,
            TransportContext? context)
            => stream.WriteAsync(bytes).AsTask();

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }

    private sealed class ProductHostFixture : IDisposable
    {
        private ProductHostFixture(
            string stateRoot,
            string checkpointRoot,
            string snapshotPath,
            string? outsideRoot,
            GeneratedCapabilityCheckpointManifest manifest,
            LogicalCapabilityReplayVerificationCapability
                replayVerificationCapability)
        {
            StateRoot = stateRoot;
            CheckpointRoot = checkpointRoot;
            SnapshotPath = snapshotPath;
            OutsideRoot = outsideRoot;
            Manifest = manifest;
            ReplayVerificationCapability = replayVerificationCapability;
        }

        public string StateRoot { get; }
        public string CheckpointRoot { get; }
        public string SnapshotPath { get; }
        public string? OutsideRoot { get; }
        public GeneratedCapabilityCheckpointManifest Manifest { get; }
        public LogicalCapabilityReplayVerificationCapability
            ReplayVerificationCapability { get; }
        public string ScenarioId => Manifest.ScenarioId;
        public string SessionDatabasePath => Path.Combine(
            StateRoot,
            CapabilityServingHostConfiguration.SessionDatabaseFileName);
        public CapabilityServingHostConfiguration Configuration =>
            CapabilityServingHostConfiguration.ResolveRequired(
                [CapabilityServingHostConfiguration.StateRootArgumentName, StateRoot],
                _ => null);

        public TextGenerator CreateIndependentGenerator()
        {
            var tokenizer = BpeTokenizer.Load(Manifest.TokenizerPath);
            var config = new ModelConfig
            {
                VocabSize = Manifest.VocabSize,
                ContextLen = Manifest.ContextLen,
                HiddenDim = Manifest.HiddenDim,
                NumLayers = Manifest.NumLayers,
                NumHeads = Manifest.NumHeads,
                NumKvHeads = Manifest.NumKvHeads,
                FfnHiddenDim = Manifest.FfnHiddenDim,
                RopeTheta = Manifest.RopeTheta!.Value,
                RmsNormEps = Manifest.RmsNormEps!.Value
            };
            var lora = new LoraConfig
            {
                Rank = Manifest.LoraRank,
                Alpha = Manifest.LoraAlpha,
                Dropout = Manifest.LoraDropout,
                TargetModules = [.. Manifest.LoraTargetModules]
            };
            var model = new LlmModel(config);
            LoraApplicator.Apply(model, lora);
            model.LoadCheckpoint(Manifest.PolicyCheckpointPath);
            return new TextGenerator(model, tokenizer);
        }

        public void RewriteSnapshot(
            Func<TeachingRegistrySnapshot, TeachingRegistrySnapshot> mutation)
        {
            var loaded = TeachingRegistryPersistence.Load(SnapshotPath);
            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
            Assert.NotNull(loaded.Value);
            var save = TeachingRegistryPersistence.Save(mutation(loaded.Value!), SnapshotPath);
            Assert.True(save.IsSuccess, save.ErrorMessage);
            SecureFixtureFile(SnapshotPath);
        }

        public static ProductHostFixture Create(
            bool writeSnapshot = true,
            bool policyOutsideStateRoot = false,
            string trainingText = "implement the backend endpoint",
            string outputText = "AUTHORITATIVE_RESTORED_CAPABILITY_RESPONSE",
            string originalPrompt = "implement the backend endpoint",
            bool useDefaultRuntimeValues = false)
        {
            var stateRoot = Path.Combine(
                Path.GetTempPath(),
                $"xiigen-product-host-test-{Guid.NewGuid():N}");
            var checkpointRoot = Path.Combine(
                stateRoot,
                CapabilityServingHostConfiguration.CheckpointDirectoryName);
            Directory.CreateDirectory(checkpointRoot);
            SecureFixtureDirectory(stateRoot);
            SecureFixtureDirectory(checkpointRoot);
            string? outsideRoot = null;

            try
            {
                var tokenizer = new BpeTokenizer();
                tokenizer.Train(
                    $"{trainingText}\n{outputText}",
                    vocabSize: 260);
                var tokenizerPath = Path.Combine(checkpointRoot, "tokenizer.json");
                tokenizer.Save(tokenizerPath);
                SecureFixtureFile(tokenizerPath);

                var runtimeDefaults = new ModelConfig();
                var modelConfig = new ModelConfig
                {
                    VocabSize = tokenizer.VocabSize,
                    ContextLen = 32,
                    HiddenDim = 8,
                    NumLayers = 1,
                    NumHeads = 1,
                    NumKvHeads = 1,
                    FfnHiddenDim = 16,
                    RopeTheta = useDefaultRuntimeValues
                        ? runtimeDefaults.RopeTheta
                        : 12_345f,
                    RmsNormEps = useDefaultRuntimeValues
                        ? runtimeDefaults.RmsNormEps
                        : 2.5e-5f
                };
                var loraConfig = new LoraConfig
                {
                    Rank = 2,
                    Alpha = 4.0f,
                    Dropout = 0.0f,
                    TargetModules = ["q_proj", "v_proj"]
                };
                var model = new LlmModel(modelConfig);
                LoraApplicator.Apply(model, loraConfig);
                var policyRoot = checkpointRoot;
                if (policyOutsideStateRoot)
                {
                    outsideRoot = Path.Combine(
                        Path.GetTempPath(),
                        $"xiigen-product-host-outside-{Guid.NewGuid():N}");
                    Directory.CreateDirectory(outsideRoot);
                    SecureFixtureDirectory(outsideRoot);
                    policyRoot = outsideRoot;
                }

                var policyPath = Path.Combine(policyRoot, "policy.bin");
                model.SaveCheckpoint(policyPath);
                SecureFixtureFile(policyPath);
                var manifest = new GeneratedCapabilityCheckpointManifest(
                    UnitId: "product-host-generated-unit",
                    UnitRole: DefaultLogicalCapabilityTrainer.GeneratedCapabilityUnitRole,
                    DomainTag: "PLAN",
                    Version: "1.0.0",
                    TrustState: nameof(LogicalUnitTrustState.Probationary),
                    FlowId: DefaultLogicalCapabilityTrainer.GeneratedCapabilityFlowIdPrefix + "-product-host",
                    ScenarioId: "product-host-capability-scenario",
                    PhaseId: "4",
                    SubPhaseId: "4.1",
                    TaskType: "backend-services",
                    TrainingText: trainingText,
                    OutputText: outputText,
                    ExampleId: "product-host-example",
                    PredicateIds: ["IMPLEMENT"],
                    DomainTags: ["backend-services"],
                    PolicyCheckpointPath: policyPath,
                    TokenizerPath: tokenizerPath,
                    VocabSize: modelConfig.VocabSize,
                    ContextLen: modelConfig.ContextLen,
                    HiddenDim: modelConfig.HiddenDim,
                    NumLayers: modelConfig.NumLayers,
                    NumHeads: modelConfig.NumHeads,
                    NumKvHeads: modelConfig.NumKvHeads,
                    FfnHiddenDim: modelConfig.FfnHiddenDim,
                    LoraRank: loraConfig.Rank,
                    LoraAlpha: loraConfig.Alpha,
                    LoraDropout: loraConfig.Dropout,
                    LoraTargetModules: loraConfig.TargetModules,
                    CreatedAtUtc: DateTimeOffset.UnixEpoch,
                    OriginalPrompt: originalPrompt,
                    RopeTheta: modelConfig.RopeTheta,
                    RmsNormEps: modelConfig.RmsNormEps);
                var tokenizerSha256 = BpeTokenizer.ComputeArtifactSha256(tokenizerPath);
                var policySha256 = ComputeFileSha256(policyPath);
                var runtimeProvenance = RuntimeProvenance.ComputeSha256(
                    modelConfig,
                    loraConfig,
                    tokenizerSha256,
                    policySha256);
                Assert.True(runtimeProvenance.IsSuccess, runtimeProvenance.ErrorMessage);
                manifest = manifest with
                {
                    TokenizerSha256 = tokenizerSha256,
                    PolicyCheckpointSha256 = policySha256,
                    RuntimeProvenanceSha256 = runtimeProvenance.Value!
                };

                const long unitRegistrationOrder = 1_000;
                const long flowRegistrationOrder = 1_001;
                var subPhase = new LogicalSubPhase(
                    manifest.SubPhaseId,
                    manifest.PhaseId,
                    "Backend Services",
                    "Owns backend service implementation and integration changes.",
                    manifest.DomainTags,
                    manifest.PredicateIds,
                    []);
                var source = new LogicalFlowResult(
                    "product-host-fixture-source-flow",
                    "product-host-fixture-source-scenario",
                    "PASS",
                    outputText,
                    new LogicalFlowSummary(
                        "product-host-fixture-source",
                        1,
                        0,
                        0,
                        "generated_capability",
                        false,
                        false,
                        false,
                        "GENERATE_CAPABILITY",
                        1,
                        manifest.SubPhaseId,
                        "Authorized deterministic fixture source.",
                        0,
                        0),
                    new Dictionary<string, string>(StringComparer.Ordinal),
                    [],
                    []);
                var deterministicInputs =
                    new LogicalTrainingContentDeterministicInputs(
                        originalPrompt,
                        manifest.DomainTag,
                        subPhase.PhaseId,
                        subPhase.SubPhaseId,
                        subPhase.Name,
                        subPhase.Purpose,
                        subPhase.PredicateIds,
                        subPhase.DomainIds);
                Assert.True(
                    LogicalTrainingContentProvenance
                        .TryComputeDeterministicTrainingIdentities(
                            source,
                            deterministicInputs,
                            out var sourceBindingSha256,
                            out var replaySealGroupSha256));
                var current = LogicalCapabilityTrainingPreferenceProjector
                    .ProjectCurrent(originalPrompt, source, subPhase);
                Assert.True(current.IsSuccess, current.ErrorMessage);
                var replay = LogicalCapabilityTrainingPreferenceProjector
                    .ProjectReplayPayload(
                        [sourceBindingSha256],
                        replaySealGroupSha256,
                        manifest.DomainTag,
                        manifest.SubPhaseId,
                        manifest.ExampleId,
                        manifest.TrainingText,
                        manifest.OutputText,
                        manifest.PredicateIds,
                        manifest.DomainTags);
                Assert.True(replay.IsSuccess, replay.ErrorMessage);
                var provenance = LogicalTrainingContentProvenance
                    .IssueDeterministicSynthetic(
                        source,
                        deterministicInputs,
                        current.Value!.DpoPreferenceSha256,
                        [sourceBindingSha256],
                        replay.Value!.Descriptor.ReplayPayloadSha256,
                        replay.Value.Descriptor.DpoPreferenceSha256);
                var batch = DefaultLogicalCapabilityTrainer.BuildAdmissionBatch(
                    "product-host-fixture-current-admission",
                    manifest.DomainTag,
                    [new LogicalDpoPreferenceExample(
                        current.Value.Prompt,
                        current.Value.Chosen,
                        current.Value.Rejected,
                        current.Value.TeachingVector)
                    {
                        TrainingContentProvenance = provenance
                    }]);
                Assert.True(batch.IsSuccess, batch.ErrorMessage);
                var legalAuthority = LegalTrainingTeacherAdmissionAuthority
                    .Create(
                        [],
                        maxTeacherCalls: 0,
                        System.Security.Cryptography.RandomNumberGenerator
                            .GetBytes(32));
                Assert.True(
                    legalAuthority.IsSuccess,
                    legalAuthority.ErrorMessage);
                var legal = Assert.IsType<
                    LegalTrainingTeacherAdmissionAuthority>(
                    legalAuthority.Value);
                var serverAuthority =
                    ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue(
                        legal.ValidateBatch,
                        static () => true,
                        sealReplay:
                            legal.SealLogicalCapabilityReplay,
                        verifyReplay:
                            legal.VerifyLogicalCapabilityReplay,
                        sealExecutableManifest:
                            legal
                                .SealLogicalCapabilityExecutableManifest,
                        verifyExecutableManifest:
                            legal
                                .VerifyLogicalCapabilityExecutableManifest);
                var leaseResult =
                    ((ILogicalCapabilityTrainingAdmissionAuthority)
                        serverAuthority).AcquireTrainingLease(batch.Value!);
                Assert.True(leaseResult.IsSuccess, leaseResult.ErrorMessage);
                using var lease = leaseResult.Value!;
                var replayAttestation = lease.SealReplay(
                    replay.Value.Descriptor);
                Assert.True(
                    replayAttestation.IsSuccess,
                    replayAttestation.ErrorMessage);
                var knnCorpus = GeneratedCapabilityKnnCorpusProjector.Project(
                    [new GeneratedCapabilityKnnExample(
                        manifest.ExampleId,
                        manifest.TrainingText,
                        manifest.OutputText,
                        manifest.PredicateIds,
                        manifest.DomainTags)]);
                Assert.True(knnCorpus.IsSuccess, knnCorpus.ErrorMessage);
                manifest = manifest with
                {
                    SchemaVersion =
                        GeneratedCapabilityCheckpointManifest
                            .CurrentSchemaVersion,
                    ReplayAttestation = replayAttestation.Value,
                    UnitRegistrationOrder = unitRegistrationOrder,
                    FlowRegistrationOrder = flowRegistrationOrder,
                    ExecutableManifestAttestation = null,
                    KnnExamples = knnCorpus.Value!.Examples,
                    KnnCorpusSha256 = knnCorpus.Value.Sha256
                };
                var executableManifestSha256 =
                    GeneratedCapabilityExecutableManifestProjector
                        .ProjectSha256(
                            manifest,
                            tokenizerSha256,
                            policySha256);
                Assert.True(
                    executableManifestSha256.IsSuccess,
                    executableManifestSha256.ErrorMessage);
                var executableCapability =
                    LogicalCapabilityExecutableManifestAttestationCapability
                        .Issue(lease);
                Assert.True(
                    executableCapability.IsSuccess,
                    executableCapability.ErrorMessage);
                var executableAttestation = executableCapability.Value!.Seal(
                    executableManifestSha256.Value!,
                    replayAttestation.Value!);
                Assert.True(
                    executableAttestation.IsSuccess,
                    executableAttestation.ErrorMessage);
                manifest = manifest with
                {
                    ExecutableManifestAttestation =
                        executableAttestation.Value
                };
                var replayVerification = serverAuthority
                    .IssueReplayVerificationCapability();
                Assert.True(
                    replayVerification.IsSuccess,
                    replayVerification.ErrorMessage);
                var snapshot = new TeachingRegistrySnapshot(
                    SnapshotId: "product-host-snapshot",
                    BundleId: "product-host-bundle",
                    RowIndex: 1,
                    CreatedAtUtc: DateTimeOffset.UnixEpoch,
                    Units:
                    [
                        new TeachingUnitRegistryEntry(
                            manifest.UnitId,
                            manifest.UnitRole,
                            manifest.DomainTag,
                            manifest.Version,
                            manifest.TrustState,
                            IsLearned: false,
                            typeof(string).AssemblyQualifiedName!,
                            typeof(string).AssemblyQualifiedName!,
                            RegistrationOrder: unitRegistrationOrder,
                            TeachingRegistrySnapshotKinds.GeneratedCapabilityUnit)
                    ],
                    Flows:
                    [
                        new TeachingFlowRegistryEntry(
                            manifest.FlowId,
                            manifest.ScenarioId,
                            manifest.Version,
                            typeof(string).AssemblyQualifiedName!,
                            manifest.PhaseId,
                            manifest.SubPhaseId,
                            manifest.TaskType,
                            manifest.DomainTags,
                            RegistrationOrder: flowRegistrationOrder,
                            TeachingRegistrySnapshotKinds.GeneratedCapabilityFlow)
                    ],
                    GeneratedCapabilities: [manifest],
                    CheckpointPaths: [policyPath, tokenizerPath]);
                var snapshotPath = Path.Combine(
                    stateRoot,
                    CapabilityServingHostConfiguration.SnapshotFileName);
                if (writeSnapshot)
                {
                    var save = TeachingRegistryPersistence.Save(snapshot, snapshotPath);
                    Assert.True(save.IsSuccess, save.ErrorMessage);
                    SecureFixtureFile(snapshotPath);
                }

                return new ProductHostFixture(
                    stateRoot,
                    checkpointRoot,
                    snapshotPath,
                    outsideRoot,
                    manifest,
                    replayVerification.Value!);
            }
            catch
            {
                if (Directory.Exists(stateRoot))
                    Directory.Delete(stateRoot, recursive: true);
                if (outsideRoot is not null && Directory.Exists(outsideRoot))
                    Directory.Delete(outsideRoot, recursive: true);
                throw;
            }
        }

        public void Dispose()
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(StateRoot))
                Directory.Delete(StateRoot, recursive: true);
            if (OutsideRoot is not null && Directory.Exists(OutsideRoot))
                Directory.Delete(OutsideRoot, recursive: true);
        }

        private static string ComputeFileSha256(string path)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(stream)).ToLowerInvariant();
        }

        private static void SecureFixtureDirectory(string path)
        {
            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(
                    path,
                    UnixFileMode.UserRead |
                    UnixFileMode.UserWrite |
                    UnixFileMode.UserExecute);
                return;
            }

            using var identity = WindowsIdentity.GetCurrent();
            var sid = identity.User!;
            var security = new DirectorySecurity();
            security.SetOwner(sid);
            security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
            security.AddAccessRule(new FileSystemAccessRule(
                sid,
                FileSystemRights.FullControl,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None,
                AccessControlType.Allow));
            FileSystemAclExtensions.SetAccessControl(new DirectoryInfo(path), security);
        }

        internal static void SecureFixtureFile(string path)
        {
            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(
                    path,
                    UnixFileMode.UserRead | UnixFileMode.UserWrite);
                return;
            }

            using var identity = WindowsIdentity.GetCurrent();
            var sid = identity.User!;
            var security = new FileSecurity();
            security.SetOwner(sid);
            security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
            security.AddAccessRule(new FileSystemAccessRule(
                sid,
                FileSystemRights.FullControl,
                AccessControlType.Allow));
            FileSystemAclExtensions.SetAccessControl(new FileInfo(path), security);
        }
    }
}

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ProductHostEnvironmentCollection
{
    public const string Name = "ProductHost environment isolation";
}
