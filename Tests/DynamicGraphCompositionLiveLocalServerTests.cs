// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ExperimentsV2.LogicalFlow;
using LlmFromScratch.DynamicGraphCompositionLiveLocalServer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OrchestratorModel.Logical;
using Domain = SemanticCompiler29.Domain;
using SemanticCompilerAdapter;

[CollectionDefinition(CollectionName, DisableParallelization = true)]
public sealed class DynamicGraphCompositionLiveLocalServerCollection
{
    public const string CollectionName = "Dynamic graph composition live-local server";
}

[Collection(DynamicGraphCompositionLiveLocalServerCollection.CollectionName)]
public sealed class DynamicGraphCompositionLiveLocalServerTests
{
    private const string TenantId = "tenant-a";
    private const string SelectedTargetFlowId = SecurityVerification34FlowAdapter.FlowId;
    private const string SelectedTargetScenarioId = SecurityVerification34FlowAdapter.ScenarioId;
    private const string SelectedTargetUserRequest =
        "Our backend authentication service has verified controls in production: password minimum length 15 characters, OTP expiry 10 minutes, TOTP window 30 seconds, session idle timeout 20 minutes with risk analysis RA-AUTH-003, re-authentication before username email password or MFA factor changes, JWT validation with RS256 only, none algorithm rejected, exp nbf and aud validated, and RBAC plus per-resource ownership checks on all API endpoints. Does this implementation satisfy OWASP ASVS 5.0.0 for sub-phase 3.4?";
    private const string SelectedTargetAuthorityUnitId = "live-selected-target-authority";
    private const string SelectedTargetEvaluationId = "live-selected-target-evaluation";
    private const string FingerprintA =
        "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string FingerprintB =
        "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
    private const string FingerprintC =
        "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc";
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromSeconds(90);
    private static readonly JsonSerializerOptions CatalogJsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly string[] SelectedTargetLeakageSignals =
        ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"];
    private static readonly string[] SelectedTargetIdentityProbeIds =
        ["source-id-control-01", "source-path-control-02", "correlation-id-control-03"];
    private static readonly string[] SelectedTargetIdentityControlledSignals =
        ["source_id", "source_path", "correlation_id"];
    private static readonly string[] SelectedTargetIdentityControlValues =
    [
        "edge-authority-source-id-control",
        "identity-control://edge-authority-source-path",
        "edge-authority-correlation-id-control"
    ];

    [Fact(Timeout = 120000)]
    public async Task ChildProcess_RequiresConfiguredTenantAndReadyFileWithoutPublishingReadiness()
    {
        var root = CreateTempRoot();
        var readyFile = Path.Combine(root, "ready.json");
        await File.WriteAllTextAsync(readyFile, "stale");
        using var process = StartServer(
            root,
            ["--ready-file", readyFile],
            inheritedEnvironmentForTest: new Dictionary<string, string>
            {
                ["XIIGEN_DYNAMIC_GRAPH_TENANT_ID"] = TenantId,
                ["DOTNET_STARTUP_HOOKS"] = Path.Combine(root, "must-be-scrubbed.dll")
            });
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await process.WaitForExitAsync(timeout.Token);

            var stdout = await stdoutTask;
            var stderr = await stderrTask;
            Assert.Equal(2, process.ExitCode);
            Assert.Equal(string.Empty, stdout);
            Assert.Equal("STARTUP_CONFIGURATION_INVALID" + Environment.NewLine, stderr);
            Assert.False(File.Exists(readyFile));
        }
        finally
        {
            Stop(process);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task ChildProcess_InvalidConfiguredTenantExitsNonzeroWithoutReadyFile()
    {
        var root = CreateTempRoot();
        var readyFile = Path.Combine(root, "ready.json");
        await File.WriteAllTextAsync(readyFile, "stale");
        using var process = StartServer(
            root, ["--tenant-id", "invalid tenant", "--ready-file", readyFile]);
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await process.WaitForExitAsync(timeout.Token);

            var stdout = await stdoutTask;
            var stderr = await stderrTask;
            Assert.Equal(2, process.ExitCode);
            Assert.Equal(string.Empty, stdout);
            Assert.Equal("STARTUP_CONFIGURATION_INVALID" + Environment.NewLine, stderr);
            Assert.False(File.Exists(readyFile));
        }
        finally
        {
            Stop(process);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task ChildProcess_OverlappingReadyFileNeverDeletesStateVictimOnConfigurationFailure()
    {
        var root = CreateTempRoot();
        var stateDirectory = Path.Combine(root, "state");
        Directory.CreateDirectory(stateDirectory);
        var victimFile = Path.Combine(stateDirectory, "victim.bin");
        byte[] victimBytes =
        [
            0xEF, 0xBB, 0xBF, 0x00, 0xFF, 0xFE, 0x80, 0xC0, 0xF5, 0x0D, 0x0A, 0x1A, 0x7F
        ];
        await File.WriteAllBytesAsync(victimFile, victimBytes);
        using var process = StartServer(
            root,
            [
                "--tenant-id", "invalid tenant",
                "--state-directory", stateDirectory,
                "--ready-file", victimFile
            ]);
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await process.WaitForExitAsync(timeout.Token);

            var stdout = await stdoutTask;
            var stderr = await stderrTask;
            Assert.Equal(2, process.ExitCode);
            Assert.Equal(string.Empty, stdout);
            Assert.Equal("STARTUP_CONFIGURATION_INVALID" + Environment.NewLine, stderr);
            Assert.True(File.Exists(victimFile));
            Assert.Equal(victimBytes, await File.ReadAllBytesAsync(victimFile));
        }
        finally
        {
            Stop(process);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task ChildProcess_AtomicReadyRemainsEphemeralLoopbackWhenAspNetCoreUrlsIsHostile()
    {
        var root = CreateTempRoot();
        var readyFile = Path.Combine(root, "ready.json");
        using var process = StartServer(
            root,
            ["--tenant-id", TenantId, "--ready-file", readyFile],
            new Dictionary<string, string> { ["ASPNETCORE_URLS"] = "http://0.0.0.0:54321" });
        try
        {
            var baseUrl = await WaitForReadyAsync(process, readyFile);
            Assert.Equal("127.0.0.1", baseUrl.Host);
            Assert.NotEqual(54321, baseUrl.Port);
            Assert.True(File.Exists(readyFile));
            Assert.False(process.HasExited);
        }
        finally
        {
            Stop(process);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task ChildProcess_HealthAndReadinessPublishExactHonestLocalBoundaries()
    {
        var root = CreateTempRoot();
        var readyFile = Path.Combine(root, "ready.json");
        using var process = StartServer(root, ["--tenant-id", TenantId, "--ready-file", readyFile]);
        try
        {
            var baseUrl = await WaitForReadyAsync(process, readyFile);
            using var client = new HttpClient { BaseAddress = baseUrl };
            foreach (var route in new[] { "/api/v1/dynamic-graph/health", "/api/v1/dynamic-graph/readiness" })
            {
                using var response = await client.GetAsync(route);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                var boundary = document.RootElement;
                Assert.True(boundary.GetProperty("loopback_only").GetBoolean());
                Assert.False(boundary.GetProperty("tenant_binding_is_authentication").GetBoolean());
                Assert.False(boundary.GetProperty("persistent").GetBoolean());
                Assert.True(boundary.GetProperty("executes_assistant").GetBoolean());
                Assert.True(boundary.GetProperty("applies_graph").GetBoolean());
                Assert.False(boundary.GetProperty("performs_training").GetBoolean());
            }
        }
        finally
        {
            Stop(process);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task ChildProcess_RealComposeAndReadKeepRevisionOneAndReadinessAfterComposition()
    {
        var root = CreateTempRoot();
        var readyFile = Path.Combine(root, "ready.json");
        using var process = StartServer(root, ["--tenant-id", TenantId, "--ready-file", readyFile]);
        try
        {
            var baseUrl = await WaitForReadyAsync(process, readyFile);
            using var client = new HttpClient { BaseAddress = baseUrl };
            using var create = await PostProfileAsync(client, CreateProfile(TenantId), "real-compose-0001", TenantId);
            Assert.Equal(HttpStatusCode.Created, create.StatusCode);
            using var created = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
            var result = created.RootElement.GetProperty("result");
            Assert.Equal(1, result.GetProperty("revision").GetInt64());
            Assert.Equal(3, result.GetProperty("node_count").GetInt32());
            Assert.Equal(2, result.GetProperty("edge_count").GetInt32());
            var sessionId = created.RootElement.GetProperty("session_id").GetString()!;
            using var read = await GetSessionAsync(client, sessionId, TenantId);
            Assert.Equal(HttpStatusCode.OK, read.StatusCode);
            using var readiness = await client.GetAsync("/api/v1/dynamic-graph/readiness");
            Assert.Equal(HttpStatusCode.OK, readiness.StatusCode);
        }
        finally
        {
            Stop(process);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task ChildProcess_SameDigestReplaysAndDifferentDigestConflicts()
    {
        var root = CreateTempRoot();
        var readyFile = Path.Combine(root, "ready.json");
        using var process = StartServer(root, ["--tenant-id", TenantId, "--ready-file", readyFile]);
        try
        {
            var baseUrl = await WaitForReadyAsync(process, readyFile);
            using var client = new HttpClient { BaseAddress = baseUrl };
            var profile = CreateProfile(TenantId);
            using var first = await PostProfileAsync(client, profile, "digest-key-0001", TenantId);
            using var firstJson = JsonDocument.Parse(await first.Content.ReadAsStringAsync());
            using var replay = await PostProfileAsync(client, profile with { }, "digest-key-0001", TenantId);
            Assert.Equal(HttpStatusCode.OK, replay.StatusCode);
            using var replayJson = JsonDocument.Parse(await replay.Content.ReadAsStringAsync());
            Assert.Equal(
                firstJson.RootElement.GetProperty("session_id").GetString(),
                replayJson.RootElement.GetProperty("session_id").GetString());
            using var conflict = await PostProfileAsync(
                client, profile with { TraceRef = "trace:different" }, "digest-key-0001", TenantId);
            Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        }
        finally
        {
            Stop(process);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task ChildProcess_TenantHeaderIsRequiredAndMismatchPrecedesExecutorAndLookup()
    {
        var root = CreateTempRoot();
        var readyFile = Path.Combine(root, "ready.json");
        using var process = StartServer(root, ["--tenant-id", TenantId, "--ready-file", readyFile]);
        try
        {
            var baseUrl = await WaitForReadyAsync(process, readyFile);
            using var client = new HttpClient { BaseAddress = baseUrl };
            using var missingRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/dynamic-graph/sessions")
            {
                Content = JsonContent.Create(new { profile = CreateProfile(TenantId) })
            };
            missingRequest.Headers.Add("Idempotency-Key", "tenant-key-0001");
            using var missing = await client.SendAsync(missingRequest);
            Assert.Equal(HttpStatusCode.BadRequest, missing.StatusCode);
            using var mismatch = await PostProfileAsync(
                client, CreateProfile(TenantId), "tenant-key-0002", "tenant-b");
            Assert.Equal(HttpStatusCode.Forbidden, mismatch.StatusCode);
            using var lookup = await GetSessionAsync(client, "dgs_" + new string('0', 32), "tenant-b");
            Assert.Equal(HttpStatusCode.Forbidden, lookup.StatusCode);
        }
        finally
        {
            Stop(process);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task ChildProcess_RejectsUnsupportedMediaAndOversizedBodyBeforeExecutor()
    {
        var root = CreateTempRoot();
        var readyFile = Path.Combine(root, "ready.json");
        using var process = StartServer(
            root,
            ["--tenant-id", TenantId, "--ready-file", readyFile, "--max-request-body-bytes", "1024"]);
        try
        {
            var baseUrl = await WaitForReadyAsync(process, readyFile);
            using var client = new HttpClient { BaseAddress = baseUrl };
            using var textRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/dynamic-graph/sessions")
            {
                Content = new StringContent("{}", Encoding.UTF8, "text/plain")
            };
            using var media = await client.SendAsync(textRequest);
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, media.StatusCode);

            using var largeRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/dynamic-graph/sessions")
            {
                Content = new StringContent("\"" + new string('x', 2048) + "\"", Encoding.UTF8, "application/json")
            };
            using var large = await client.SendAsync(largeRequest);
            Assert.Equal(HttpStatusCode.RequestEntityTooLarge, large.StatusCode);
        }
        finally
        {
            Stop(process);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task ChildProcess_BoundedSessionCapacityReturnsServiceUnavailable()
    {
        var root = CreateTempRoot();
        var readyFile = Path.Combine(root, "ready.json");
        using var process = StartServer(
            root,
            ["--tenant-id", TenantId, "--ready-file", readyFile, "--max-sessions", "1"]);
        try
        {
            var baseUrl = await WaitForReadyAsync(process, readyFile);
            using var client = new HttpClient { BaseAddress = baseUrl };
            using var first = await PostProfileAsync(client, CreateProfile(TenantId), "capacity-key-0001", TenantId);
            Assert.Equal(HttpStatusCode.Created, first.StatusCode);
            using var capacity = await PostProfileAsync(
                client, CreateProfile(TenantId) with { ProfileId = "second-profile" }, "capacity-key-0002", TenantId);
            Assert.Equal(HttpStatusCode.ServiceUnavailable, capacity.StatusCode);
            await AssertErrorCodeAsync(capacity, "HOST_SESSION_CAPACITY_EXHAUSTED");
        }
        finally
        {
            Stop(process);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task ChildProcess_PublicPayloadsDoNotLeakTenantProvenanceKeyPathOrHeavyAssemblyNames()
    {
        var root = CreateTempRoot();
        var readyFile = Path.Combine(root, "ready.json");
        using var process = StartServer(root, ["--tenant-id", TenantId, "--ready-file", readyFile]);
        try
        {
            var baseUrl = await WaitForReadyAsync(process, readyFile);
            using var client = new HttpClient { BaseAddress = baseUrl };
            var sourceSecret = "source-secret-991";
            var traceSecret = "trace-secret-992";
            var keySecret = "secret-key-0993";
            var profile = CreateProfile(TenantId) with { SourceRef = sourceSecret, TraceRef = traceSecret };
            using var success = await PostProfileAsync(client, profile, keySecret, TenantId);
            var successText = await success.Content.ReadAsStringAsync();
            using var conflict = await PostProfileAsync(
                client, profile with { SignalId = "changed" }, keySecret, TenantId);
            var errorText = await conflict.Content.ReadAsStringAsync();
            using var health = await client.GetAsync("/api/v1/dynamic-graph/health");
            Stop(process);
            var stdout = await process.StandardOutput.ReadToEndAsync();
            var stderr = await process.StandardError.ReadToEndAsync();
            Assert.Equal("READY", stdout.Trim());
            Assert.Equal(string.Empty, stderr);
            var publicText = string.Join("\n", successText, errorText,
                await health.Content.ReadAsStringAsync(), await File.ReadAllTextAsync(readyFile), stdout, stderr);

            foreach (var forbidden in new[]
                     { TenantId, sourceSecret, traceSecret, keySecret, root, "DPO", "TrainingLoop" })
                Assert.DoesNotContain(forbidden, publicText, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Stop(process);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact(Timeout = 180000)]
    public async Task ChildProcess_ComposesReplaysReadsAndEnforcesSafeHttpBoundary()
    {
        var root = CreateTempRoot();
        var readyFile = Path.Combine(root, "ready.json");
        using var process = StartServer(root, ["--tenant-id", TenantId, "--ready-file", readyFile]);
        try
        {
            var baseUrl = await WaitForReadyAsync(process, readyFile);
            using var client = new HttpClient { BaseAddress = baseUrl, Timeout = TimeSpan.FromSeconds(20) };

            using var health = await client.GetAsync("/api/v1/dynamic-graph/health");
            using var readiness = await client.GetAsync("/api/v1/dynamic-graph/readiness");
            Assert.Equal(HttpStatusCode.OK, health.StatusCode);
            Assert.Equal(HttpStatusCode.OK, readiness.StatusCode);
            Assert.DoesNotContain(root, await health.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);

            var profile = CreateProfile(TenantId);
            using var created = await PostProfileAsync(client, profile, "compose-key-0001", TenantId, "request.created");
            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            using var createdJson = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
            var createdRoot = createdJson.RootElement;
            var sessionId = createdRoot.GetProperty("session_id").GetString()!;
            Assert.Equal($"/api/v1/dynamic-graph/sessions/{sessionId}", created.Headers.Location!.OriginalString);
            Assert.Matches("^dgs_[0-9a-f]{32}$", sessionId);
            Assert.Equal("request.created", createdRoot.GetProperty("request_id").GetString());
            Assert.Equal("created", createdRoot.GetProperty("status").GetString());
            Assert.Equal(1, createdRoot.GetProperty("result").GetProperty("revision").GetInt64());
            Assert.Equal(3, createdRoot.GetProperty("result").GetProperty("node_count").GetInt32());
            Assert.Equal(2, createdRoot.GetProperty("result").GetProperty("edge_count").GetInt32());
            var createdBoundaries = createdRoot.GetProperty("boundaries");
            Assert.True(createdBoundaries.GetProperty("loopback_only").GetBoolean());
            Assert.False(createdBoundaries.GetProperty("tenant_binding_is_authentication").GetBoolean());
            Assert.False(createdBoundaries.GetProperty("persistent").GetBoolean());
            Assert.True(createdBoundaries.GetProperty("executes_assistant").GetBoolean());
            Assert.False(createdBoundaries.GetProperty("applies_graph").GetBoolean());
            Assert.False(createdBoundaries.GetProperty("performs_training").GetBoolean());

            using var replay = await PostProfileAsync(client, profile, "compose-key-0001", TenantId);
            Assert.Equal(HttpStatusCode.OK, replay.StatusCode);
            using var replayJson = JsonDocument.Parse(await replay.Content.ReadAsStringAsync());
            Assert.Equal(sessionId, replayJson.RootElement.GetProperty("session_id").GetString());
            Assert.Equal("replayed", replayJson.RootElement.GetProperty("status").GetString());

            using var read = await GetSessionAsync(client, sessionId, TenantId);
            Assert.Equal(HttpStatusCode.OK, read.StatusCode);
            using var readJson = JsonDocument.Parse(await read.Content.ReadAsStringAsync());
            Assert.Equal(sessionId, readJson.RootElement.GetProperty("session_id").GetString());

            using var run = await PostAssistantRunAsync(
                client,
                sessionId,
                "Review the Phase 3 technical-design to Phase 4 implementation transition.",
                TenantId,
                "request.run");
            Assert.Equal(HttpStatusCode.OK, run.StatusCode);
            using var runJson = JsonDocument.Parse(await run.Content.ReadAsStringAsync());
            var runRoot = runJson.RootElement;
            Assert.Equal("request.run", runRoot.GetProperty("request_id").GetString());
            Assert.Equal(sessionId, runRoot.GetProperty("session_id").GetString());
            Assert.Equal("completed", runRoot.GetProperty("status").GetString());
            var runResult = runRoot.GetProperty("result");
            Assert.Equal("T34-P3-P4-AUTONOMOUS-LEARNING-PLAN", runResult.GetProperty("scenario_id").GetString());
            Assert.Equal("T34-PHASE-3-TO-4-TRANSITION-GATE", runResult.GetProperty("selected_flow_id").GetString());
            Assert.Equal("PASS_WITH_GAPS", runResult.GetProperty("overall_verdict").GetString());
            Assert.Contains("Verdict:", runResult.GetProperty("final_human_result").GetString(), StringComparison.Ordinal);
            Assert.Equal("True", runResult.GetProperty("metrics").GetProperty("transition_success").GetString());
            Assert.Single(runResult.GetProperty("evidence_ids").EnumerateArray());
            var runBoundaries = runRoot.GetProperty("boundaries");
            Assert.True(runBoundaries.GetProperty("loopback_only").GetBoolean());
            Assert.False(runBoundaries.GetProperty("tenant_binding_is_authentication").GetBoolean());
            Assert.False(runBoundaries.GetProperty("persistent").GetBoolean());
            Assert.True(runBoundaries.GetProperty("executes_assistant").GetBoolean());
            Assert.False(runBoundaries.GetProperty("applies_graph").GetBoolean());
            Assert.False(runBoundaries.GetProperty("performs_training").GetBoolean());

            using var conflict = await PostProfileAsync(
                client, profile with { SignalId = "different-signal" }, "compose-key-0001", TenantId);
            Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
            await AssertErrorCodeAsync(conflict, "HOST_IDEMPOTENCY_CONFLICT");

            using var headerMismatch = await PostProfileAsync(client, profile, "compose-key-0002", "tenant-b");
            Assert.Equal(HttpStatusCode.Forbidden, headerMismatch.StatusCode);
            using var profileMismatch = await PostProfileAsync(
                client, CreateProfile("tenant-b"), "compose-key-0003", TenantId);
            Assert.Equal(HttpStatusCode.Forbidden, profileMismatch.StatusCode);

            using var trained = await PostProfileAsync(
                client,
                profile with
                {
                    NodeDefinitions = profile.NodeDefinitions
                        .Select((node, index) => index == 0
                            ? node with { Descriptor = node.Descriptor with { IsTrained = true } }
                            : node)
                        .ToArray()
                },
                "compose-key-0004",
                TenantId);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, trained.StatusCode);
            await AssertErrorCodeAsync(trained, "HOST_TRAINED_NODE_INPUT_FORBIDDEN");

            using var unknown = await GetSessionAsync(client, "dgs_" + new string('0', 32), TenantId);
            Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
            using var crossTenantRead = await GetSessionAsync(client, sessionId, "tenant-b");
            Assert.Equal(HttpStatusCode.Forbidden, crossTenantRead.StatusCode);

            using var wrongMediaRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/dynamic-graph/sessions")
            {
                Content = new StringContent("{}", Encoding.UTF8, "text/plain")
            };
            using var wrongMedia = await client.SendAsync(wrongMediaRequest);
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, wrongMedia.StatusCode);

            using var malformedRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/dynamic-graph/sessions")
            {
                Content = new StringContent("{", Encoding.UTF8, "application/json")
            };
            malformedRequest.Headers.Add("X-XIIGen-Tenant-Id", TenantId);
            malformedRequest.Headers.Add("Idempotency-Key", "compose-key-0005");
            using var malformed = await client.SendAsync(malformedRequest);
            Assert.Equal(HttpStatusCode.BadRequest, malformed.StatusCode);
            Assert.DoesNotContain(root, await malformed.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Stop(process);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task ChildProcess_ExpiredSessionReturnsGoneAndRemainsDistinctFromUnknown()
    {
        var root = CreateTempRoot();
        var readyFile = Path.Combine(root, "ready.json");
        using var process = StartServer(
            root,
            ["--tenant-id", TenantId, "--ready-file", readyFile, "--session-ttl-seconds", "1"]);
        try
        {
            var baseUrl = await WaitForReadyAsync(process, readyFile);
            using var client = new HttpClient { BaseAddress = baseUrl, Timeout = TimeSpan.FromSeconds(20) };
            using var created = await PostProfileAsync(client, CreateProfile(TenantId), "expiry-key-0001", TenantId);
            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            using var json = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
            var sessionId = json.RootElement.GetProperty("session_id").GetString()!;

            await Task.Delay(TimeSpan.FromMilliseconds(1200));
            using var expired = await GetSessionAsync(client, sessionId, TenantId);
            Assert.Equal(HttpStatusCode.Gone, expired.StatusCode);
            await AssertErrorCodeAsync(expired, "HOST_SESSION_EXPIRED");
            using var expiredAgain = await GetSessionAsync(client, sessionId, TenantId);
            Assert.Equal(HttpStatusCode.Gone, expiredAgain.StatusCode);
            await Task.Delay(TimeSpan.FromMilliseconds(1100));
            using var expiredAfterRetention = await GetSessionAsync(client, sessionId, TenantId);
            Assert.Equal(HttpStatusCode.NotFound, expiredAfterRetention.StatusCode);
            using var unknown = await GetSessionAsync(client, "dgs_" + new string('f', 32), TenantId);
            Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        }
        finally
        {
            Stop(process);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task ChildProcess_ChunkedOversizeReturnsSanitizedJson413Envelope()
    {
        var root = CreateTempRoot();
        var readyFile = Path.Combine(root, "ready.json");
        using var process = StartServer(
            root,
            ["--tenant-id", TenantId, "--ready-file", readyFile, "--max-request-body-bytes", "1024"]);
        try
        {
            var baseUrl = await WaitForReadyAsync(process, readyFile);
            using var tcp = new TcpClient();
            await tcp.ConnectAsync(baseUrl.Host, baseUrl.Port);
            await using var stream = tcp.GetStream();
            var body = "{\"padding\":\"" + new string('x', 2048) + "\"}";
            var bodyBytes = Encoding.UTF8.GetBytes(body);
            var headers = string.Join("\r\n",
                "POST /api/v1/dynamic-graph/sessions HTTP/1.1",
                $"Host: {baseUrl.Host}:{baseUrl.Port}",
                "Transfer-Encoding: chunked",
                "Content-Type: application/json",
                $"X-XIIGen-Tenant-Id: {TenantId}",
                "Idempotency-Key: chunked-key-0001",
                "X-Request-Id: chunked.request",
                "Connection: close",
                "",
                "");
            await stream.WriteAsync(Encoding.ASCII.GetBytes(headers));
            await stream.WriteAsync(Encoding.ASCII.GetBytes(bodyBytes.Length.ToString("x") + "\r\n"));
            await stream.WriteAsync(bodyBytes);
            await stream.WriteAsync(Encoding.ASCII.GetBytes("\r\n0\r\n\r\n"));
            await stream.FlushAsync();

            using var responseBuffer = new MemoryStream();
            await stream.CopyToAsync(responseBuffer);
            var raw = Encoding.UTF8.GetString(responseBuffer.ToArray());
            Assert.StartsWith("HTTP/1.1 413", raw, StringComparison.Ordinal);
            Assert.Contains("application/json", raw, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("HOST_REQUEST_BODY_LIMIT_EXCEEDED", raw, StringComparison.Ordinal);
            Assert.Contains("chunked.request", raw, StringComparison.Ordinal);
            Assert.DoesNotContain(root, raw, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("BadHttpRequestException", raw, StringComparison.Ordinal);
            Assert.DoesNotContain("stack", raw, StringComparison.OrdinalIgnoreCase);
            Stop(process);
            Assert.Equal("READY", (await process.StandardOutput.ReadToEndAsync()).Trim());
            Assert.Equal(string.Empty, await process.StandardError.ReadToEndAsync());
        }
        finally
        {
            Stop(process);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact(Timeout = 180000)]
    public async Task ChildProcess_ForbiddenStartupAssemblyNeverPublishesReadyAndUsesFixedMarkerOnly()
    {
        var root = CreateTempRoot();
        var readyFile = Path.Combine(root, "ready.json");
        var hookRoot = Path.Combine(
            AppContext.BaseDirectory,
            "dynamic-graph-startup-hooks",
            Guid.NewGuid().ToString("N"));
        var hook = BuildForbiddenAssemblyStartupHook(hookRoot);
        using var process = StartServer(
            root,
            ["--tenant-id", TenantId, "--ready-file", readyFile],
            new Dictionary<string, string> { ["DOTNET_STARTUP_HOOKS"] = hook });
        try
        {
            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await process.WaitForExitAsync(timeout.Token);

            Assert.Equal(3, process.ExitCode);
            Assert.False(File.Exists(readyFile));
            Assert.Equal(string.Empty, await stdoutTask);
            Assert.Equal("NOT_READY" + Environment.NewLine, await stderrTask);
        }
        finally
        {
            Stop(process);
            if (Directory.Exists(hookRoot))
                Directory.Delete(hookRoot, recursive: true);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact(Timeout = 180000)]
    public async Task ChildProcess_PersistentSessionRestoresReadReplayAndConflictAcrossRestart()
    {
        var root = CreateTempRoot();
        var persistentRoot = CreatePersistentTempRoot("xiigen-persistence-test");
        var stateDirectory = Path.Combine(persistentRoot, "state");
        var firstReadyFile = Path.Combine(root, "first-ready.json");
        var secondReadyFile = Path.Combine(root, "second-ready.json");
        var profile = CreateProfile(TenantId);
        const string idempotencyKey = "persistent-restart-0001";
        string sessionId;

        try
        {
            using var firstProcess = StartServer(
                root,
                [
                    "--tenant-id", TenantId,
                    "--ready-file", firstReadyFile,
                    "--state-directory", stateDirectory
                ]);
            try
            {
                var firstBaseUrl = await WaitForReadyAsync(firstProcess, firstReadyFile, expectedPersistent: true);
                using var firstClient = new HttpClient { BaseAddress = firstBaseUrl };
                using var created = await PostProfileAsync(
                    firstClient,
                    profile,
                    idempotencyKey,
                    TenantId);
                Assert.Equal(HttpStatusCode.Created, created.StatusCode);
                using var createdJson = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
                sessionId = createdJson.RootElement.GetProperty("session_id").GetString()!;
            }
            finally
            {
                Stop(firstProcess);
            }

            using var secondProcess = StartServer(
                root,
                [
                    "--tenant-id", TenantId,
                    "--ready-file", secondReadyFile,
                    "--state-directory", stateDirectory
                ]);
            try
            {
                var secondBaseUrl = await WaitForReadyAsync(secondProcess, secondReadyFile, expectedPersistent: true);
                using var secondClient = new HttpClient { BaseAddress = secondBaseUrl };

                using var restored = await GetSessionAsync(secondClient, sessionId, TenantId);
                Assert.Equal(HttpStatusCode.OK, restored.StatusCode);
                using var restoredJson = JsonDocument.Parse(await restored.Content.ReadAsStringAsync());
                Assert.Equal(sessionId, restoredJson.RootElement.GetProperty("session_id").GetString());

                using var topologyRun = await PostTopologyRunAsync(
                    secondClient,
                    sessionId,
                    "Run the restored session against its retained current topology.",
                    TenantId,
                    "persistent.topology.run");
                Assert.Equal(HttpStatusCode.OK, topologyRun.StatusCode);
                using var topologyRunJson = JsonDocument.Parse(
                    await topologyRun.Content.ReadAsStringAsync());
                Assert.Equal(
                    1,
                    topologyRunJson.RootElement
                        .GetProperty("topology")
                        .GetProperty("revision")
                        .GetInt64());
                Assert.True(
                    topologyRunJson.RootElement
                        .GetProperty("boundaries")
                        .GetProperty("persistent")
                        .GetBoolean());

                using var replay = await PostProfileAsync(
                    secondClient,
                    profile with { },
                    idempotencyKey,
                    TenantId);
                Assert.Equal(HttpStatusCode.OK, replay.StatusCode);
                using var replayJson = JsonDocument.Parse(await replay.Content.ReadAsStringAsync());
                Assert.Equal("replayed", replayJson.RootElement.GetProperty("status").GetString());
                Assert.Equal(sessionId, replayJson.RootElement.GetProperty("session_id").GetString());

                using var conflict = await PostProfileAsync(
                    secondClient,
                    profile with { TraceRef = "trace:persistent-restart-conflict" },
                    idempotencyKey,
                    TenantId);
                Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
                await AssertErrorCodeAsync(conflict, "HOST_IDEMPOTENCY_CONFLICT");
            }
            finally
            {
                Stop(secondProcess);
            }
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
            if (Directory.Exists(persistentRoot))
                Directory.Delete(persistentRoot, recursive: true);
        }
    }

    [Fact(Timeout = 180000)]
    public async Task ChildProcess_PersistentV2CheckpointHmacOnlyTamperFailsStartupWithoutKeyLeak()
    {
        var root = CreateTempRoot();
        var persistentRoot = CreatePersistentTempRoot("xiigen-persistence-v2-tamper-test");
        var stateDirectory = Path.Combine(persistentRoot, "state");
        var firstReadyFile = Path.Combine(root, "first-ready.json");
        var secondReadyFile = Path.Combine(root, "second-ready.json");

        try
        {
            using (var firstProcess = StartServer(
                       root,
                       [
                           "--tenant-id", TenantId,
                           "--ready-file", firstReadyFile,
                           "--state-directory", stateDirectory
                       ]))
            {
                try
                {
                    var baseUrl = await WaitForReadyAsync(
                        firstProcess,
                        firstReadyFile,
                        expectedPersistent: true);
                    using var client = new HttpClient { BaseAddress = baseUrl };
                    using var created = await PostProfileAsync(
                        client,
                        CreateProfile(TenantId),
                        "persistent-v2-tamper-0001",
                        TenantId);
                    Assert.Equal(HttpStatusCode.Created, created.StatusCode);
                }
                finally
                {
                    Stop(firstProcess);
                }
            }

            var checkpointPath = Assert.Single(Directory.EnumerateFiles(
                Path.Combine(stateDirectory, "sessions"),
                "*.json",
                SearchOption.TopDirectoryOnly));
            var keyBytes = await File.ReadAllBytesAsync(
                Path.Combine(stateDirectory, "index.key"));
            var checkpointJson = await File.ReadAllTextAsync(checkpointPath);
            Assert.Equal(32, keyBytes.Length);
            Assert.DoesNotContain(
                Convert.ToHexString(keyBytes),
                checkpointJson,
                StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(
                Convert.ToBase64String(keyBytes),
                checkpointJson,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                "persistent-v2-tamper-0001",
                checkpointJson,
                StringComparison.Ordinal);
            Assert.DoesNotContain("index_key", checkpointJson, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("private_key", checkpointJson, StringComparison.OrdinalIgnoreCase);

            string originalMac;
            string originalPayload;
            using (var checkpointDocument = JsonDocument.Parse(checkpointJson))
            {
                var envelope = checkpointDocument.RootElement;
                Assert.Equal(
                    ["schema", "payload", "hmac_sha256"],
                    envelope.EnumerateObject().Select(property => property.Name));
                Assert.Equal(
                    "dynamic-graph-composition-session.v2",
                    envelope.GetProperty("schema").GetString());
                Assert.Matches(
                    "^[0-9A-F]{64}$",
                    envelope.GetProperty("hmac_sha256").GetString()!);
                var payload = envelope.GetProperty("payload");
                originalMac = envelope.GetProperty("hmac_sha256").GetString()!;
                originalPayload = payload.GetRawText();
                Assert.Equal(
                    "dynamic-graph-composition-session.v2",
                    payload.GetProperty("schema").GetString());
                Assert.Equal(
                    [
                        "schema",
                        "session_id",
                        "tenant_index",
                        "opaque_idempotency_index",
                        "created_at_utc",
                        "expires_at_utc",
                        "bootstrap",
                        "current"
                    ],
                    payload.EnumerateObject().Select(property => property.Name));
                Assert.Equal(
                    "derived_initial",
                    payload.GetProperty("current")
                        .GetProperty("proof")
                        .GetProperty("mutation_proof")
                        .GetProperty("mode")
                        .GetString());
            }

            var replacementMac = new string(originalMac[0] == 'A' ? 'B' : 'A', 64);
            Assert.Matches("^[0-9A-F]{64}$", replacementMac);
            Assert.NotEqual(originalMac, replacementMac);
            var originalMacProperty = $"\"hmac_sha256\":\"{originalMac}\"";
            Assert.Single(
                Regex.Matches(checkpointJson, Regex.Escape(originalMacProperty)).Cast<Match>());
            var tampered = checkpointJson.Replace(
                originalMacProperty,
                $"\"hmac_sha256\":\"{replacementMac}\"",
                StringComparison.Ordinal);
            Assert.NotEqual(checkpointJson, tampered);
            await File.WriteAllTextAsync(checkpointPath, tampered, Encoding.UTF8);
            using (var tamperedDocument = JsonDocument.Parse(
                       await File.ReadAllTextAsync(checkpointPath)))
            {
                Assert.Equal(
                    originalPayload,
                    tamperedDocument.RootElement.GetProperty("payload").GetRawText());
                Assert.Equal(
                    replacementMac,
                    tamperedDocument.RootElement.GetProperty("hmac_sha256").GetString());
            }

            using var secondProcess = StartServer(
                root,
                [
                    "--tenant-id", TenantId,
                    "--ready-file", secondReadyFile,
                    "--state-directory", stateDirectory
                ]);
            try
            {
                var stdoutTask = secondProcess.StandardOutput.ReadToEndAsync();
                var stderrTask = secondProcess.StandardError.ReadToEndAsync();
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                await secondProcess.WaitForExitAsync(timeout.Token);
                Assert.Equal(3, secondProcess.ExitCode);
                Assert.False(File.Exists(secondReadyFile));
                Assert.Equal(string.Empty, await stdoutTask);
                Assert.Equal("NOT_READY" + Environment.NewLine, await stderrTask);
            }
            finally
            {
                Stop(secondProcess);
            }
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
            if (Directory.Exists(persistentRoot))
                Directory.Delete(persistentRoot, recursive: true);
        }
    }

    [Fact(Timeout = 180000)]
    public async Task ChildProcess_PersistentV2CheckpointDifferentIndexKeyFailsStartup()
    {
        var root = CreateTempRoot();
        var persistentRoot = CreatePersistentTempRoot("xiigen-persistence-v2-key-mismatch-test");
        var stateDirectory = Path.Combine(persistentRoot, "state");
        var firstReadyFile = Path.Combine(root, "first-ready.json");
        var secondReadyFile = Path.Combine(root, "second-ready.json");

        try
        {
            using (var firstProcess = StartServer(
                       root,
                       [
                           "--tenant-id", TenantId,
                           "--ready-file", firstReadyFile,
                           "--state-directory", stateDirectory
                       ]))
            {
                try
                {
                    var baseUrl = await WaitForReadyAsync(
                        firstProcess,
                        firstReadyFile,
                        expectedPersistent: true);
                    using var client = new HttpClient { BaseAddress = baseUrl };
                    using var created = await PostProfileAsync(
                        client,
                        CreateProfile(TenantId),
                        "persistent-v2-key-mismatch-0001",
                        TenantId);
                    Assert.Equal(HttpStatusCode.Created, created.StatusCode);
                }
                finally
                {
                    Stop(firstProcess);
                }
            }

            var checkpointPath = Assert.Single(Directory.EnumerateFiles(
                Path.Combine(stateDirectory, "sessions"),
                "*.json",
                SearchOption.TopDirectoryOnly));
            var checkpointBytes = await File.ReadAllBytesAsync(checkpointPath);
            var keyPath = Path.Combine(stateDirectory, "index.key");
            var originalKey = await File.ReadAllBytesAsync(keyPath);
            Assert.Equal(32, originalKey.Length);
            var replacementKey = originalKey
                .Select(value => (byte)(value ^ byte.MaxValue))
                .ToArray();
            Assert.Equal(32, replacementKey.Length);
            Assert.False(originalKey.SequenceEqual(replacementKey));

            await File.WriteAllBytesAsync(keyPath, replacementKey);
            Assert.Equal(replacementKey, await File.ReadAllBytesAsync(keyPath));
            Assert.Equal(checkpointBytes, await File.ReadAllBytesAsync(checkpointPath));

            using var secondProcess = StartServer(
                root,
                [
                    "--tenant-id", TenantId,
                    "--ready-file", secondReadyFile,
                    "--state-directory", stateDirectory
                ]);
            try
            {
                var stdoutTask = secondProcess.StandardOutput.ReadToEndAsync();
                var stderrTask = secondProcess.StandardError.ReadToEndAsync();
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                await secondProcess.WaitForExitAsync(timeout.Token);
                Assert.Equal(3, secondProcess.ExitCode);
                Assert.False(File.Exists(secondReadyFile));
                Assert.Equal(string.Empty, await stdoutTask);
                Assert.Equal("NOT_READY" + Environment.NewLine, await stderrTask);
            }
            finally
            {
                Stop(secondProcess);
            }
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
            if (Directory.Exists(persistentRoot))
                Directory.Delete(persistentRoot, recursive: true);
        }
    }

    [Fact(Timeout = 180000)]
    public async Task ChildProcess_CatalogSessionSelectsExactProfileAndIgnoresInvalidUnselectedProfile()
    {
        var root = CreateTempRoot();
        var readyFile = Path.Combine(root, "ready.json");
        using var process = StartServer(root, ["--tenant-id", TenantId, "--ready-file", readyFile]);
        try
        {
            var baseUrl = await WaitForReadyAsync(process, readyFile);
            using var client = new HttpClient { BaseAddress = baseUrl };
            var invalidUnselected = CreateProfileWithId("ignored-invalid") with
            {
                NodeDefinitions = [],
                Budget = new TopologyBuildBudget(0, 0, 0, 0)
            };
            var selected = CreateProfileWithId("catalog-selected");

            using var created = await PostCatalogAsync(
                client,
                [invalidUnselected, selected],
                "catalog-selected",
                "catalog-select-0001");

            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            Assert.NotNull(created.Headers.Location);
            using var createdJson = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
            var createdRoot = createdJson.RootElement;
            var sessionId = createdRoot.GetProperty("session_id").GetString()!;
            Assert.Equal("catalog-selected", createdRoot.GetProperty("result").GetProperty("profile_id").GetString());

            using var read = await GetSessionAsync(client, sessionId, TenantId);
            Assert.Equal(HttpStatusCode.OK, read.StatusCode);
            using var run = await PostAssistantRunAsync(
                client,
                sessionId,
                "Review the Phase 3 technical-design to Phase 4 implementation transition.",
                TenantId,
                "catalog.run.0001");
            Assert.Equal(HttpStatusCode.OK, run.StatusCode);
        }
        finally
        {
            Stop(process);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact(Timeout = 180000)]
    public async Task ChildProcess_CatalogSessionEnforcesExactQueryAndStrictValidFormDecoding()
    {
        var root = CreateTempRoot();
        var readyFile = Path.Combine(root, "ready.json");
        using var process = StartServer(root, ["--tenant-id", TenantId, "--ready-file", readyFile]);
        try
        {
            var baseUrl = await WaitForReadyAsync(process, readyFile);
            using var client = new HttpClient { BaseAddress = baseUrl };
            var cases = new[]
            {
                (Id: "profile/with?reserved=ok", Raw: "profile%2Fwith%3Freserved=ok", Key: "query-reserved-0001"),
                (Id: "profile-שלום", Raw: "profile-%D7%A9%D7%9C%D7%95%D7%9D", Key: "query-unicode-0002"),
                (Id: "profile+plus space", Raw: "profile%2Bplus+space", Key: "query-plus-0003")
            };

            foreach (var testCase in cases)
            {
                using var response = await PostCatalogAsync(
                    client,
                    [CreateProfileWithId(testCase.Id)],
                    testCase.Raw,
                    testCase.Key);
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                Assert.Equal(
                    testCase.Id,
                    document.RootElement.GetProperty("result").GetProperty("profile_id").GetString());
            }

            var body = Catalog(CreateProfileWithId("query-invalid"));
            using var missing = await PostCatalogBytesAsync(
                client, body, null, "query-invalid-0004");
            await AssertErrorEnvelopeAsync(
                missing,
                HttpStatusCode.BadRequest,
                "HOST_SELECTED_PROFILE_QUERY_INVALID",
                "An exact single selected_profile_id query parameter is required.");
            using var wrongCase = await PostCatalogBytesAsync(
                client, body, "?Selected_profile_id=query-invalid", "query-invalid-0005");
            await AssertErrorEnvelopeAsync(
                wrongCase,
                HttpStatusCode.BadRequest,
                "HOST_SELECTED_PROFILE_QUERY_INVALID",
                "An exact single selected_profile_id query parameter is required.");
            using var extra = await PostCatalogBytesAsync(
                client, body, "?selected_profile_id=query-invalid&extra=value", "query-invalid-0006");
            await AssertErrorEnvelopeAsync(
                extra,
                HttpStatusCode.BadRequest,
                "HOST_SELECTED_PROFILE_QUERY_INVALID",
                "An exact single selected_profile_id query parameter is required.");
        }
        finally
        {
            Stop(process);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact(Timeout = 180000)]
    public async Task ChildProcess_CatalogSessionRejectsRawMalformedQueryEncodingsAndMultiplicity()
    {
        var root = CreateTempRoot();
        var readyFile = Path.Combine(root, "ready.json");
        using var process = StartServer(root, ["--tenant-id", TenantId, "--ready-file", readyFile]);
        try
        {
            var baseUrl = await WaitForReadyAsync(process, readyFile);
            var body = Catalog(CreateProfileWithId("raw-query"));
            var targets = new[]
            {
                "/api/v1/dynamic-graph/catalog-sessions?selected_profile_id=%",
                "/api/v1/dynamic-graph/catalog-sessions?selected_profile_id=%G0",
                "/api/v1/dynamic-graph/catalog-sessions?selected_profile_id=%C3%28",
                "/api/v1/dynamic-graph/catalog-sessions?%73elected_profile_id=raw-query",
                "/api/v1/dynamic-graph/catalog-sessions?selected_profile_id=raw-query&selected_profile_id=raw-query",
                "/api/v1/dynamic-graph/catalog-sessions?selected_profile_id=raw-query&extra=value"
            };

            for (var index = 0; index < targets.Length; index++)
            {
                var raw = await SendRawCatalogRequestAsync(
                    baseUrl,
                    targets[index],
                    body,
                    $"raw-query-{index:D4}");
                Assert.StartsWith("HTTP/1.1 400", raw, StringComparison.Ordinal);
                Assert.Contains("HOST_SELECTED_PROFILE_QUERY_INVALID", raw, StringComparison.Ordinal);
                Assert.DoesNotContain("stack", raw, StringComparison.OrdinalIgnoreCase);
            }
        }
        finally
        {
            Stop(process);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact(Timeout = 180000)]
    public async Task ChildProcess_CatalogSessionMapsBodyAndSelectionFailuresToExactSafeEnvelopes()
    {
        var root = CreateTempRoot();
        var readyFile = Path.Combine(root, "ready.json");
        using var process = StartServer(root, ["--tenant-id", TenantId, "--ready-file", readyFile]);
        try
        {
            var baseUrl = await WaitForReadyAsync(process, readyFile);
            using var client = new HttpClient { BaseAddress = baseUrl };
            const string rejectedMessage = "The deterministic composition profile was rejected.";
            var validCatalog = Catalog(CreateProfileWithId("body-selected"));
            var duplicateCatalog = Catalog(
                CreateProfileWithId("duplicate-profile"),
                CreateProfileWithId("duplicate-profile"));
            var bomCatalog = new byte[] { 0xEF, 0xBB, 0xBF }.Concat(validCatalog).ToArray();
            var cases = new[]
            {
                (Body: Array.Empty<byte>(), Query: "?selected_profile_id=body-selected", Code: "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_CATALOG_REQUIRED"),
                (Body: Encoding.UTF8.GetBytes("{}"), Query: "?selected_profile_id=body-selected", Code: "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_CATALOG_INVALID"),
                (Body: Encoding.UTF8.GetBytes("["), Query: "?selected_profile_id=body-selected", Code: "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_CATALOG_INVALID"),
                (Body: bomCatalog, Query: "?selected_profile_id=body-selected", Code: "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_CATALOG_INVALID"),
                (Body: duplicateCatalog, Query: "?selected_profile_id=duplicate-profile", Code: "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_CATALOG_DUPLICATE_PROFILE_ID"),
                (Body: validCatalog, Query: "?selected_profile_id=missing-profile", Code: "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_SELECTION_NOT_FOUND"),
                (Body: validCatalog, Query: "?selected_profile_id=", Code: "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_SELECTION_ID_REQUIRED")
            };

            for (var index = 0; index < cases.Length; index++)
            {
                using var response = await PostCatalogBytesAsync(
                    client,
                    cases[index].Body,
                    cases[index].Query,
                    $"catalog-body-{index:D4}");
                await AssertErrorEnvelopeAsync(
                    response,
                    HttpStatusCode.BadRequest,
                    cases[index].Code,
                    rejectedMessage);
            }

            using var wrongMedia = await PostCatalogBytesAsync(
                client,
                validCatalog,
                "?selected_profile_id=body-selected",
                "catalog-body-media",
                mediaType: "text/plain");
            await AssertErrorEnvelopeAsync(
                wrongMedia,
                HttpStatusCode.UnsupportedMediaType,
                "HOST_JSON_CONTENT_TYPE_REQUIRED",
                "The request content type must be JSON.");

            using var missingTenant = await PostCatalogBytesAsync(
                client,
                validCatalog,
                "?selected_profile_id=body-selected",
                "catalog-auth-tenant-missing",
                includeTenantHeader: false);
            await AssertErrorEnvelopeAsync(
                missingTenant,
                HttpStatusCode.BadRequest,
                "HOST_TENANT_HEADER_INVALID",
                "A safe X-XIIGen-Tenant-Id header is required.");

            using var mismatchedTenant = await PostCatalogBytesAsync(
                client,
                validCatalog,
                "?selected_profile_id=body-selected",
                "catalog-auth-tenant-mismatch",
                tenantId: "tenant-b");
            await AssertErrorEnvelopeAsync(
                mismatchedTenant,
                HttpStatusCode.Forbidden,
                "HOST_TENANT_BINDING_MISMATCH",
                "The request does not match the configured tenant binding.");

            using var missingIdempotencyKey = await PostCatalogBytesAsync(
                client,
                validCatalog,
                "?selected_profile_id=body-selected",
                "catalog-auth-idempotency-missing",
                includeIdempotencyKey: false);
            await AssertErrorEnvelopeAsync(
                missingIdempotencyKey,
                HttpStatusCode.BadRequest,
                "HOST_IDEMPOTENCY_KEY_INVALID",
                "A safe 8-128 character Idempotency-Key is required.");

            var mismatchedProfileCatalog = Catalog(
                CreateProfile("tenant-b") with { ProfileId = "profile-tenant-mismatch" });
            using var mismatchedProfileTenant = await PostCatalogBytesAsync(
                client,
                mismatchedProfileCatalog,
                "?selected_profile_id=profile-tenant-mismatch",
                "catalog-profile-tenant-mismatch");
            await AssertErrorEnvelopeAsync(
                mismatchedProfileTenant,
                HttpStatusCode.Forbidden,
                "HOST_PROFILE_TENANT_MISMATCH",
                "The profile does not match the configured tenant binding.");
        }
        finally
        {
            Stop(process);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact(Timeout = 180000)]
    public async Task ChildProcess_CatalogSessionBoundsDeclaredChunkedAndAbortedBodiesWithoutLosingReadiness()
    {
        var root = CreateTempRoot();
        var readyFile = Path.Combine(root, "ready.json");
        using var process = StartServer(
            root,
            ["--tenant-id", TenantId, "--ready-file", readyFile, "--max-request-body-bytes", "8192"]);
        try
        {
            var baseUrl = await WaitForReadyAsync(process, readyFile);
            using var client = new HttpClient { BaseAddress = baseUrl };
            const int exactLimit = 8192;
            var validCatalog = Catalog(CreateProfileWithId("exact-limit"));
            Assert.True(validCatalog.Length < exactLimit);
            var exactLimitCatalog = validCatalog
                .Concat(Enumerable.Repeat((byte)' ', exactLimit - validCatalog.Length))
                .ToArray();
            Assert.Equal(exactLimit, exactLimitCatalog.Length);

            using var exactDeclared = await PostCatalogBytesAsync(
                client,
                exactLimitCatalog,
                "?selected_profile_id=exact-limit",
                "catalog-exact-declared");
            Assert.Equal(HttpStatusCode.Created, exactDeclared.StatusCode);
            using (var exactDeclaredJson = JsonDocument.Parse(await exactDeclared.Content.ReadAsStringAsync()))
            {
                Assert.Equal(
                    "exact-limit",
                    exactDeclaredJson.RootElement.GetProperty("result").GetProperty("profile_id").GetString());
            }

            var exactChunked = await SendRawCatalogRequestAsync(
                baseUrl,
                "/api/v1/dynamic-graph/catalog-sessions?selected_profile_id=exact-limit",
                exactLimitCatalog,
                "catalog-exact-chunked",
                chunked: true);
            Assert.True(
                exactChunked.StartsWith("HTTP/1.1 201", StringComparison.Ordinal),
                exactChunked);
            Assert.Contains("\"profile_id\":\"exact-limit\"", exactChunked, StringComparison.Ordinal);

            var oversized = exactLimitCatalog.Concat([(byte)' ']).ToArray();
            Assert.Equal(exactLimit + 1, oversized.Length);
            using var declared = await PostCatalogBytesAsync(
                client,
                oversized,
                "?selected_profile_id=exact-limit",
                "catalog-declared-oversize");
            await AssertErrorEnvelopeAsync(
                declared,
                HttpStatusCode.RequestEntityTooLarge,
                "HOST_REQUEST_BODY_LIMIT_EXCEEDED",
                "The request body exceeds the local limit.");

            var chunked = await SendRawCatalogRequestAsync(
                baseUrl,
                "/api/v1/dynamic-graph/catalog-sessions?selected_profile_id=exact-limit",
                oversized,
                "catalog-chunked-oversize",
                chunked: true);
            Assert.StartsWith("HTTP/1.1 413", chunked, StringComparison.Ordinal);
            Assert.Contains("HOST_REQUEST_BODY_LIMIT_EXCEEDED", chunked, StringComparison.Ordinal);

            await SendAbortedCatalogRequestAsync(baseUrl);
            await Task.Delay(200);
            using var readiness = await client.GetAsync("/api/v1/dynamic-graph/readiness");
            Assert.Equal(HttpStatusCode.OK, readiness.StatusCode);
            Assert.False(process.HasExited);
        }
        finally
        {
            Stop(process);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact(Timeout = 180000)]
    public async Task ChildProcess_CatalogSessionIdempotencyUsesOnlySelectedDigestAcrossCatalogAndInlineRoutes()
    {
        var root = CreateTempRoot();
        var readyFile = Path.Combine(root, "ready.json");
        using var process = StartServer(root, ["--tenant-id", TenantId, "--ready-file", readyFile]);
        try
        {
            var baseUrl = await WaitForReadyAsync(process, readyFile);
            using var client = new HttpClient { BaseAddress = baseUrl };
            var selected = CreateProfileWithId("digest-selected");
            var firstCatalog = new[] { CreateProfileWithId("unselected-a"), selected };
            const string key = "catalog-digest-0001";

            using var created = await PostCatalogAsync(
                client, firstCatalog, "digest-selected", key);
            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            var sessionId = await ReadSessionIdAsync(created);

            var reordered = new[]
            {
                selected with { },
                CreateProfileWithId("unselected-b") with { Budget = new TopologyBuildBudget(0, 0, 0, 0) }
            };
            using var replay = await PostCatalogAsync(
                client, reordered, "digest-selected", key);
            Assert.Equal(HttpStatusCode.OK, replay.StatusCode);
            Assert.Equal(sessionId, await ReadSessionIdAsync(replay));

            using var inlineReplay = await PostProfileAsync(client, selected, key, TenantId);
            Assert.Equal(HttpStatusCode.OK, inlineReplay.StatusCode);
            Assert.Equal(sessionId, await ReadSessionIdAsync(inlineReplay));

            using var conflict = await PostCatalogAsync(
                client,
                [selected with { TraceRef = "trace:selected-conflict" }],
                "digest-selected",
                key);
            await AssertErrorEnvelopeAsync(
                conflict,
                HttpStatusCode.Conflict,
                "HOST_IDEMPOTENCY_CONFLICT",
                "The idempotency key is bound to another request.");
        }
        finally
        {
            Stop(process);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact(Timeout = 180000)]
    public async Task ChildProcess_CatalogSessionConcurrentIdenticalRequestsCreateOnceThenReplayStably()
    {
        var root = CreateTempRoot();
        var readyFile = Path.Combine(root, "ready.json");
        using var process = StartServer(
            root,
            [
                "--tenant-id", TenantId,
                "--ready-file", readyFile,
                "--max-concurrent-compositions", "8"
            ]);
        try
        {
            var baseUrl = await WaitForReadyAsync(process, readyFile);
            using var client = new HttpClient { BaseAddress = baseUrl };
            var catalog = new[] { CreateProfileWithId("concurrent-selected") };
            const string key = "catalog-concurrent-0001";
            var responses = await Task.WhenAll(Enumerable.Range(0, 8).Select(index =>
                PostCatalogAsync(
                    client,
                    catalog,
                    "concurrent-selected",
                    key,
                    $"catalog.concurrent.{index:D2}")));
            try
            {
                Assert.Equal(1, responses.Count(response => response.StatusCode == HttpStatusCode.Created));
                var successfulSessionIds = new List<string>();
                foreach (var response in responses)
                {
                    if (response.StatusCode is HttpStatusCode.Created or HttpStatusCode.OK)
                    {
                        successfulSessionIds.Add(await ReadSessionIdAsync(response));
                    }
                    else
                    {
                        await AssertErrorEnvelopeAsync(
                            response,
                            HttpStatusCode.Conflict,
                            "HOST_IDEMPOTENCY_PENDING",
                            "The idempotency key has a composition in progress.");
                    }
                }
                Assert.NotEmpty(successfulSessionIds);
                Assert.Single(successfulSessionIds.Distinct(StringComparer.Ordinal));

                using var stableReplay = await PostCatalogAsync(
                    client, catalog, "concurrent-selected", key);
                Assert.Equal(HttpStatusCode.OK, stableReplay.StatusCode);
                Assert.Equal(successfulSessionIds[0], await ReadSessionIdAsync(stableReplay));
            }
            finally
            {
                foreach (var response in responses) response.Dispose();
            }
        }
        finally
        {
            Stop(process);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact(Timeout = 180000)]
    public async Task ChildProcess_CatalogAndInlineSessionsShareOneBoundedCapacity()
    {
        var root = CreateTempRoot();
        var readyFile = Path.Combine(root, "ready.json");
        using var process = StartServer(
            root,
            ["--tenant-id", TenantId, "--ready-file", readyFile, "--max-sessions", "1"]);
        try
        {
            var baseUrl = await WaitForReadyAsync(process, readyFile);
            using var client = new HttpClient { BaseAddress = baseUrl };
            using var catalogCreated = await PostCatalogAsync(
                client,
                [CreateProfileWithId("capacity-catalog")],
                "capacity-catalog",
                "capacity-catalog-0001");
            Assert.Equal(HttpStatusCode.Created, catalogCreated.StatusCode);

            using var inlineCapacity = await PostProfileAsync(
                client,
                CreateProfileWithId("capacity-inline"),
                "capacity-inline-0002",
                TenantId);
            await AssertErrorEnvelopeAsync(
                inlineCapacity,
                HttpStatusCode.ServiceUnavailable,
                "HOST_SESSION_CAPACITY_EXHAUSTED",
                "The bounded local session capacity is exhausted.");
        }
        finally
        {
            Stop(process);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact(Timeout = 240000)]
    public async Task ChildProcess_PersistentCatalogSessionRestoresRunsReplaysReorderedAndConflicts()
    {
        var root = CreateTempRoot();
        var persistentRoot = CreatePersistentTempRoot("xiigen-catalog-persistence-test");
        var stateDirectory = Path.Combine(persistentRoot, "state");
        var firstReadyFile = Path.Combine(root, "first-ready.json");
        var secondReadyFile = Path.Combine(root, "second-ready.json");
        var selected = CreateProfileWithId("persistent-catalog-selected");
        const string key = "persistent-catalog-0001";
        string sessionId;

        try
        {
            using (var firstProcess = StartServer(
                       root,
                       [
                           "--tenant-id", TenantId,
                           "--ready-file", firstReadyFile,
                           "--state-directory", stateDirectory
                       ]))
            {
                try
                {
                    var firstBaseUrl = await WaitForReadyAsync(
                        firstProcess, firstReadyFile, expectedPersistent: true);
                    using var firstClient = new HttpClient { BaseAddress = firstBaseUrl };
                    using var created = await PostCatalogAsync(
                        firstClient,
                        [CreateProfileWithId("persistent-unselected"), selected],
                        "persistent-catalog-selected",
                        key);
                    Assert.Equal(HttpStatusCode.Created, created.StatusCode);
                    sessionId = await ReadSessionIdAsync(created);
                    using var firstRun = await PostAssistantRunAsync(
                        firstClient,
                        sessionId,
                        "Review the Phase 3 technical-design to Phase 4 implementation transition.",
                        TenantId,
                        "persistent.catalog.run.1");
                    Assert.Equal(HttpStatusCode.OK, firstRun.StatusCode);
                }
                finally
                {
                    Stop(firstProcess);
                }
            }

            using var secondProcess = StartServer(
                root,
                [
                    "--tenant-id", TenantId,
                    "--ready-file", secondReadyFile,
                    "--state-directory", stateDirectory
                ]);
            try
            {
                var secondBaseUrl = await WaitForReadyAsync(
                    secondProcess, secondReadyFile, expectedPersistent: true);
                using var secondClient = new HttpClient { BaseAddress = secondBaseUrl };
                using var restored = await GetSessionAsync(secondClient, sessionId, TenantId);
                Assert.Equal(HttpStatusCode.OK, restored.StatusCode);
                using var restoredRun = await PostAssistantRunAsync(
                    secondClient,
                    sessionId,
                    "Review the Phase 3 technical-design to Phase 4 implementation transition.",
                    TenantId,
                    "persistent.catalog.run.2");
                Assert.Equal(HttpStatusCode.OK, restoredRun.StatusCode);

                using var replay = await PostCatalogAsync(
                    secondClient,
                    [
                        selected with { },
                        CreateProfileWithId("persistent-unselected-changed") with
                        {
                            Budget = new TopologyBuildBudget(0, 0, 0, 0)
                        }
                    ],
                    "persistent-catalog-selected",
                    key);
                Assert.Equal(HttpStatusCode.OK, replay.StatusCode);
                Assert.Equal(sessionId, await ReadSessionIdAsync(replay));

                using var conflict = await PostCatalogAsync(
                    secondClient,
                    [selected with { SignalId = "persistent-selected-conflict" }],
                    "persistent-catalog-selected",
                    key);
                await AssertErrorEnvelopeAsync(
                    conflict,
                    HttpStatusCode.Conflict,
                    "HOST_IDEMPOTENCY_CONFLICT",
                    "The idempotency key is bound to another request.");
            }
            finally
            {
                Stop(secondProcess);
            }
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
            if (Directory.Exists(persistentRoot)) Directory.Delete(persistentRoot, recursive: true);
        }
    }

    [Fact]
    public void NormalizedSource_StartupCleanupResolverHasUniqueOrderedSafetyBindings()
    {
        var optionsSource = NormalizeSource(File.ReadAllText(Path.Combine(
            FindRepoRoot(), "tools", "DynamicGraphCompositionLiveLocalServer",
            "DynamicGraphCompositionServerOptions.cs")));
        var resolverBlock = ExtractUniqueBoundedSourceBlock(
            optionsSource,
            "public static string? FindReadyFileForStartupCleanup(",
            "public void Validate()");
        AssertTokensInOrder(
            resolverBlock,
            "var cli = ParseCli(args);",
            "var configuredReadyFile = cli.TryGetValue(\"--ready-file\"",
            "XIIGEN_DYNAMIC_GRAPH_READY_FILE",
            "var configuredStateDirectory = cli.TryGetValue(\"--state-directory\"",
            "XIIGEN_DYNAMIC_GRAPH_STATE_DIRECTORY",
            "NormalizeReadyFile(configuredReadyFile);",
            "NormalizeStateDirectory(configuredStateDirectory);",
            "EnsureReadyFileOutsideState(",
            "return normalizedReadyFile;");
    }

    [Fact]
    public void NormalizedSource_StartupConfigurationBoundaryHasUniqueOrderedCleanupBindings()
    {
        var program = NormalizeSource(File.ReadAllText(Path.Combine(
            FindRepoRoot(), "tools", "DynamicGraphCompositionLiveLocalServer", "Program.cs")));
        var configurationBlock = ExtractUniqueBoundedSourceBlock(
            program,
            "string? readyFileForStartupCleanup = null;",
            "WebApplicationBuilder builder;");
        AssertTokensInOrder(
            configurationBlock,
            "string? readyFileForStartupCleanup = null;",
            "FindReadyFileForStartupCleanup(args);",
            "DynamicGraphCompositionServerOptions.Parse(args);",
            "ReadyFile.RemoveStale(options.ReadyFile);",
            "catch (Exception exception) when (FatalExceptionPolicy.IsNonFatal(exception))",
            "if (readyFileForStartupCleanup is not null)",
            "ReadyFile.RemoveSafe(readyFileForStartupCleanup);",
            "Console.Error.WriteLine(\"STARTUP_CONFIGURATION_INVALID\");",
            "Environment.ExitCode = 2;",
            "return;");
    }

    [Fact]
    public void NormalizedSource_BuilderInitializationBoundaryIsExactOrderedAndFailClosed()
    {
        var program = NormalizeSource(File.ReadAllText(Path.Combine(
            FindRepoRoot(), "tools", "DynamicGraphCompositionLiveLocalServer", "Program.cs")));
        var actual = ExtractUniqueBoundedSourceBlock(
            program,
            "WebApplicationBuilder builder;",
            "DynamicGraphCompositionSessionPersistence persistenceOwner;");
        var expected = NormalizeSource(
            """
            WebApplicationBuilder builder;
            try
            {
                var contentRoot = Path.GetDirectoryName(options.ReadyFile)!;
                DynamicGraphCompositionFileSystemPolicy.EnsureSafeExistingComponents(contentRoot);
                Directory.CreateDirectory(contentRoot);
                DynamicGraphCompositionFileSystemPolicy.EnsureDirectory(contentRoot);
                builder = WebApplication.CreateBuilder(new WebApplicationOptions
                {
                    Args = Array.Empty<string>(),
                    ApplicationName = typeof(Program).Assembly.FullName,
                    ContentRootPath = contentRoot,
                });
                builder.WebHost.UseUrls(DynamicGraphCompositionServerOptions.LoopbackUrl);
                builder.WebHost.ConfigureKestrel(kestrel => kestrel.Limits.MaxRequestBodySize = options.MaxRequestBodyBytes);
                builder.Logging.ClearProviders();
                builder.Services.AddSingleton(options);
            }
            catch (Exception exception) when (FatalExceptionPolicy.IsNonFatal(exception))
            {
                Console.Error.WriteLine("NOT_READY");
                Environment.ExitCode = 3;
                return;
            }
            """);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void NormalizedSource_StartupReadinessPreflightPrecedesListenerBindingAndPostStartRaceGateRemains()
    {
        var program = NormalizeSource(File.ReadAllText(Path.Combine(
            FindRepoRoot(), "tools", "DynamicGraphCompositionLiveLocalServer", "Program.cs")));
        var preflight = ExtractUniqueBoundedSourceBlock(
            program,
            "try\n{\n    if (!readiness.AllowsComposition(requireReady: false))",
            "await app.StartAsync();");
        var expectedPreflight = NormalizeSource(
            """
            try
            {
                if (!readiness.AllowsComposition(requireReady: false))
                {
                    Console.Error.WriteLine("NOT_READY");
                    Environment.ExitCode = 3;
                    return;
                }
            """);
        Assert.Equal(expectedPreflight, preflight);

        var postStart = ExtractUniqueBoundedSourceBlock(
            program,
            "await app.StartAsync();",
            "await app.WaitForShutdownAsync();");
        var expectedPostStart = NormalizeSource(
            """
            await app.StartAsync();
            if (!readiness.PublishReady(app.Urls.Single()))
            {
                Console.Error.WriteLine("NOT_READY");
                Environment.ExitCode = 3;
                return;
            }
            Console.WriteLine("READY");
            """);
        Assert.Equal(expectedPostStart, postStart);
    }

    [Fact]
    public void ReflectionInvocationHelper_UnwrapsTheOriginalThrowingSentinel()
    {
        var fixtureMethods = typeof(DynamicGraphCompositionLiveLocalServerTests)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Where(method => method.Name == nameof(ThrowReflectionInvocationSentinel))
            .ToArray();
        Assert.True(
            fixtureMethods.Length == 1,
            $"Expected exactly one throwing reflection fixture, but found {fixtureMethods.Length}: " +
            string.Join(", ", fixtureMethods.Select(method => method.ToString())));
        var fixtureMethod = fixtureMethods[0];
        Assert.True(
            fixtureMethod.ReturnType == typeof(object) && fixtureMethod.GetParameters().Length == 0,
            $"Unexpected throwing reflection fixture signature: {fixtureMethod}.");

        var thrown = Assert.Throws<ReflectionInvocationSentinelException>(() =>
        {
            InvokeStaticUnwrapped(fixtureMethod, []);
        });

        Assert.Equal(ReflectionInvocationSentinelException.FixedMessage, thrown.Message);
    }

    [Fact]
    public void StartupCleanupResolver_UsesStructuredCliAndStatePrecedenceAndRejectsAmbiguity()
    {
        var serverAssemblyPath = Path.Combine(
            FindRepoRoot(), "tools", "DynamicGraphCompositionLiveLocalServer", "bin", "Debug", "net9.0",
            "DynamicGraphCompositionLiveLocalServer.dll");
        Assert.True(
            File.Exists(serverAssemblyPath),
            $"Build the live-local server before running the startup cleanup resolver test: {serverAssemblyPath}");
        var assembly = Assembly.LoadFrom(serverAssemblyPath);
        const string optionsTypeName =
            "LlmFromScratch.DynamicGraphCompositionLiveLocalServer.DynamicGraphCompositionServerOptions";
        var optionsTypes = assembly.GetTypes()
            .Where(type => type.FullName == optionsTypeName)
            .ToArray();
        Assert.True(
            optionsTypes.Length == 1,
            $"Expected exactly one {optionsTypeName}, but found {optionsTypes.Length}.");
        var optionsType = optionsTypes[0];
        const string persistenceExceptionTypeName =
            "LlmFromScratch.DynamicGraphCompositionLiveLocalServer.DynamicGraphCompositionPersistenceException";
        var persistenceExceptionTypes = assembly.GetTypes()
            .Where(type => type.FullName == persistenceExceptionTypeName)
            .ToArray();
        Assert.True(
            persistenceExceptionTypes.Length == 1,
            $"Expected exactly one {persistenceExceptionTypeName}, but found {persistenceExceptionTypes.Length}.");
        var persistenceExceptionType = persistenceExceptionTypes[0];
        var resolverMethods = optionsType
            .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(method => method.Name == "FindReadyFileForStartupCleanup")
            .ToArray();
        Assert.True(
            resolverMethods.Length == 1,
            $"Expected exactly one FindReadyFileForStartupCleanup method, but found " +
            $"{resolverMethods.Length}: {string.Join(", ", resolverMethods.Select(method => method.ToString()))}");
        var resolverMethod = resolverMethods[0];
        var resolverParameters = resolverMethod.GetParameters();
        Assert.True(
            resolverMethod.ReturnType == typeof(string)
            && resolverParameters.Length == 2
            && resolverParameters[0].ParameterType == typeof(string[])
            && resolverParameters[1].ParameterType == typeof(Func<string, string?>),
            $"Unexpected startup cleanup resolver signature: {resolverMethod}.");

        var root = CreateTempRoot();
        try
        {
            var cliReadyFile = Path.Combine(root, "cli-ready.json");
            var cliStateDirectory = Path.Combine(root, "cli-state");
            var environmentReadyFile = Path.Combine(root, "environment-ready.json");
            var environmentStateDirectory = Path.Combine(root, "environment-state");
            var requestedCliEnvironmentKeys = new List<string>();
            Func<string, string?> hostileEnvironment = key =>
            {
                requestedCliEnvironmentKeys.Add(key);
                return key switch
                {
                    "XIIGEN_DYNAMIC_GRAPH_READY_FILE" => environmentReadyFile,
                    "XIIGEN_DYNAMIC_GRAPH_STATE_DIRECTORY" => root,
                    _ => throw new Xunit.Sdk.XunitException($"Unexpected environment key: {key}")
                };
            };
            var positionalCli = new[]
            {
                "--tenant-id", "--ready-file",
                "--ready-file", cliReadyFile,
                "--state-directory", cliStateDirectory
            };
            Assert.Equal(
                Path.GetFullPath(cliReadyFile),
                InvokeStaticUnwrapped(
                    resolverMethod,
                    [positionalCli, hostileEnvironment]));
            Assert.Empty(requestedCliEnvironmentKeys);

            var requestedEnvironmentKeys = new List<string>();
            Func<string, string?> environmentFallback = key =>
            {
                requestedEnvironmentKeys.Add(key);
                return key switch
                {
                    "XIIGEN_DYNAMIC_GRAPH_READY_FILE" => environmentReadyFile,
                    "XIIGEN_DYNAMIC_GRAPH_STATE_DIRECTORY" => environmentStateDirectory,
                    _ => throw new Xunit.Sdk.XunitException($"Unexpected environment key: {key}")
                };
            };
            Assert.Equal(
                Path.GetFullPath(environmentReadyFile),
                InvokeStaticUnwrapped(
                    resolverMethod,
                    [new[] { "--tenant-id", TenantId }, environmentFallback]));
            Assert.Equal(
                ["XIIGEN_DYNAMIC_GRAPH_READY_FILE", "XIIGEN_DYNAMIC_GRAPH_STATE_DIRECTORY"],
                requestedEnvironmentKeys);

            var overlappingStateDirectory = Path.Combine(root, "overlapping-state");
            var overlappingReadyFile = Path.Combine(overlappingStateDirectory, "victim.bin");
            Func<string, string?> overlappingStateEnvironment = key => key switch
            {
                "XIIGEN_DYNAMIC_GRAPH_STATE_DIRECTORY" => overlappingStateDirectory,
                _ => throw new Xunit.Sdk.XunitException($"Unexpected environment key: {key}")
            };
            var overlapFailure = Record.Exception(() => InvokeStaticUnwrapped(
                resolverMethod,
                [new[] { "--ready-file", overlappingReadyFile }, overlappingStateEnvironment]));
            var overlapConfigurationFailure = Assert.IsType<InvalidOperationException>(overlapFailure);
            Assert.NotNull(overlapConfigurationFailure.InnerException);
            Assert.Equal(persistenceExceptionType, overlapConfigurationFailure.InnerException.GetType());

            Func<string, string?> whitespaceStateEnvironment = key => key switch
            {
                "XIIGEN_DYNAMIC_GRAPH_STATE_DIRECTORY" => " ",
                _ => throw new Xunit.Sdk.XunitException($"Unexpected environment key: {key}")
            };
            var whitespaceStateFailure = Record.Exception(() => InvokeStaticUnwrapped(
                resolverMethod,
                [new[] { "--ready-file", cliReadyFile }, whitespaceStateEnvironment]));
            var whitespaceStateConfigurationFailure =
                Assert.IsType<InvalidOperationException>(whitespaceStateFailure);
            Assert.NotNull(whitespaceStateConfigurationFailure.InnerException);
            Assert.Equal(
                persistenceExceptionType,
                whitespaceStateConfigurationFailure.InnerException.GetType());

            var environmentReadForWhitespaceCli = false;
            Func<string, string?> whitespaceCliFallback = _ =>
            {
                environmentReadForWhitespaceCli = true;
                return environmentReadyFile;
            };
            Assert.Null(InvokeStaticUnwrapped(
                resolverMethod,
                [new[] { "--ready-file", " " }, whitespaceCliFallback]));
            Assert.False(environmentReadForWhitespaceCli);
            Assert.Null(InvokeStaticUnwrapped(
                resolverMethod,
                [Array.Empty<string>(), new Func<string, string?>(_ => "\t")]));

            var nullArgs = Assert.Throws<ArgumentNullException>(() =>
            {
                InvokeStaticUnwrapped(resolverMethod, [null, environmentFallback]);
            });
            Assert.Equal("args", nullArgs.ParamName);
            _ = Assert.Throws<InvalidOperationException>(() =>
            {
                InvokeStaticUnwrapped(
                    resolverMethod,
                    [new[] { "--ready-file", cliReadyFile, "--ready-file", environmentReadyFile },
                        environmentFallback]);
            });
            _ = Assert.Throws<InvalidOperationException>(() =>
            {
                InvokeStaticUnwrapped(resolverMethod, [new[] { "--ready-file" }, environmentFallback]);
            });
            _ = Assert.Throws<InvalidOperationException>(() =>
            {
                InvokeStaticUnwrapped(
                    resolverMethod,
                    [new[] { "--unknown", cliReadyFile }, environmentFallback]);
            });
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact(Timeout = 10000)]
    public async Task FatalExceptionPolicy_TraversesCompleteReferenceGraphAndTerminatesCycles()
    {
        var serverAssemblyPath = Path.Combine(
            FindRepoRoot(), "tools", "DynamicGraphCompositionLiveLocalServer", "bin", "Debug", "net9.0",
            "DynamicGraphCompositionLiveLocalServer.dll");
        Assert.True(
            File.Exists(serverAssemblyPath),
            $"Build the live-local server before running the policy reflection test: {serverAssemblyPath}");
        var assembly = Assembly.LoadFrom(serverAssemblyPath);
        var policyTypes = assembly.GetTypes()
            .Where(type => type.Name == "FatalExceptionPolicy"
                || type.Name.EndsWith("__FatalExceptionPolicy", StringComparison.Ordinal))
            .ToArray();
        Assert.True(
            policyTypes.Length == 1,
            $"Expected exactly one FatalExceptionPolicy type, but found {policyTypes.Length}: " +
            string.Join(", ", policyTypes.Select(type => type.FullName)));
        var policyType = policyTypes[0];
        Assert.True(
            policyType.Name.EndsWith("__FatalExceptionPolicy", StringComparison.Ordinal)
            && policyType.IsAbstract
            && policyType.IsSealed
            && !policyType.IsPublic
            && !policyType.IsNestedPublic,
            $"Expected a non-public file-local static FatalExceptionPolicy, but resolved " +
            $"{policyType.FullName} ({policyType.Attributes}).");
        var policyMethods = policyType
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(method => method.Name == "IsNonFatal")
            .ToArray();
        Assert.True(
            policyMethods.Length == 1,
            $"Expected exactly one IsNonFatal method on {policyType.FullName}, but found " +
            $"{policyMethods.Length}: {string.Join(", ", policyMethods.Select(method => method.ToString()))}");
        var policyMethod = policyMethods[0];
        var policyParameters = policyMethod.GetParameters();
        Assert.True(
            policyMethod.IsStatic
            && policyMethod.ReturnType == typeof(bool)
            && policyParameters is [{ ParameterType: var parameterType }]
            && parameterType == typeof(Exception),
            $"Expected static bool IsNonFatal(Exception), but resolved {policyMethod}.");

        AssertPolicyResult(policyMethod, new InvalidOperationException("ordinary"), expected: true, "ordinary");
        AssertPolicyResult(policyMethod, new OutOfMemoryException("direct fatal"), expected: false, "direct fatal");
        AssertPolicyResult(
            policyMethod,
            new ApplicationException("outer", new BadImageFormatException("nested fatal")),
            expected: false,
            "nested-inner fatal");
        AssertPolicyResult(
            policyMethod,
            new AggregateException(
                new InvalidOperationException("first nonfatal"),
                new AccessViolationException("non-first fatal")),
            expected: false,
            "non-first aggregate fatal");

        var shared = new InvalidOperationException("shared nonfatal");
        AssertPolicyResult(
            policyMethod,
            new AggregateException(shared, new ApplicationException("shared wrapper", shared), shared),
            expected: true,
            "repeated shared references");

        var exceptionFields = typeof(Exception).GetFields(
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        var innerFields = exceptionFields
            .Where(field => field.FieldType == typeof(Exception))
            .ToArray();
        Assert.True(
            innerFields.Length == 1,
            $"Expected exactly one private Exception-typed inner field on System.Exception, but found " +
            $"{innerFields.Length}. Declared fields: " +
            string.Join(", ", exceptionFields.Select(
                field => $"{field.Name}:{field.FieldType.FullName}")));
        var innerField = innerFields[0];
        var cycle = new InvalidOperationException("cycle");
        var cycleMutationFailure = Record.Exception(() => innerField.SetValue(cycle, cycle));
        Assert.True(
            cycleMutationFailure is null,
            $"Failed to create deterministic Exception.InnerException cycle through " +
            $"{innerField.DeclaringType?.FullName}.{innerField.Name}: {cycleMutationFailure}");
        Assert.Same(cycle, cycle.InnerException);
        await Task.Run(
                () => AssertPolicyResult(policyMethod, cycle, expected: true, "cyclic nonfatal graph"))
            .WaitAsync(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ProjectBoundary_HasOnlyTheThreeApprovedDirectProjectReferences()
    {
        var projectPath = Path.Combine(
            FindRepoRoot(), "tools", "DynamicGraphCompositionLiveLocalServer",
            "DynamicGraphCompositionLiveLocalServer.csproj");
        var project = File.ReadAllText(projectPath);
        var references = System.Xml.Linq.XDocument.Parse(project)
            .Descendants("ProjectReference")
            .Select(element => element.Attribute("Include")!.Value.Replace('/', '\\'))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            [
                "..\\..\\15.OrchestratorModel\\OrchestratorModel.csproj",
                "..\\..\\19.DynamicGraphComposition.Contracts\\19.DynamicGraphComposition.Contracts.csproj",
                "..\\..\\43.SmartSkillsDomain\\43.SmartSkillsDomain.csproj"
            ],
            references);
        Assert.DoesNotContain("04.TrainingLoop", project, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("11.DPO", project, StringComparison.OrdinalIgnoreCase);

        var serverRoot = Path.GetDirectoryName(projectPath)!;
        var program = File.ReadAllText(Path.Combine(serverRoot, "Program.cs"));
        var registry = File.ReadAllText(Path.Combine(serverRoot, "DynamicGraphCompositionSessionRegistry.cs"));
        var persistence = File.ReadAllText(Path.Combine(serverRoot, "DynamicGraphCompositionSessionPersistence.cs"));
        Assert.Contains("builder.Logging.ClearProviders();", program, StringComparison.Ordinal);
        var syntaxRoot = CSharpSyntaxTree.ParseText(program).GetRoot();
        var composeFunction = Assert.Single(
            syntaxRoot.DescendantNodes().OfType<LocalFunctionStatementSyntax>(),
            function => function.Identifier.ValueText == "Compose");
        var requireReadyParameter = Assert.Single(
            composeFunction.ParameterList.Parameters,
            parameter => parameter.Identifier.ValueText == "requireReady");
        Assert.True(
            requireReadyParameter.Type is PredefinedTypeSyntax predefinedType
            && predefinedType.Keyword.IsKind(SyntaxKind.BoolKeyword),
            $"Compose.requireReady must remain Boolean: {requireReadyParameter}");
        var composeBody = Assert.IsType<BlockSyntax>(composeFunction.Body);
        var readinessGates = composeBody.Statements
            .OfType<IfStatementSyntax>()
            .Where(IsRequiredCompositionReadinessGate)
            .ToArray();
        Assert.Equal(3, readinessGates.Length);
        var allowsCompositionCalls = composeFunction.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(invocation => IsSimpleInvocation(
                invocation, "readiness", "AllowsComposition", "requireReady"))
            .ToArray();
        Assert.Equal(3, allowsCompositionCalls.Length);
        Assert.DoesNotContain(
            composeFunction.DescendantNodes().OfType<MemberAccessExpressionSyntax>(),
            member => member.Expression is IdentifierNameSyntax { Identifier.ValueText: "readiness" }
                      && member.Name.Identifier.ValueText == "IsReady");

        var composeCall = Assert.Single(
            composeFunction.DescendantNodes().OfType<InvocationExpressionSyntax>(),
            invocation => IsSimpleInvocation(invocation, "executor", "Compose", "profile"));
        var readCurrentCall = Assert.Single(
            composeFunction.DescendantNodes().OfType<InvocationExpressionSyntax>(),
            invocation =>
                    invocation.Expression is MemberAccessExpressionSyntax
                    {
                        Expression: MemberAccessExpressionSyntax
                        {
                            Expression: IdentifierNameSyntax { Identifier.ValueText: "composed" },
                            Name.Identifier.ValueText: "Value"
                        },
                        Name.Identifier.ValueText: "ReadCurrent"
                    }
                    && invocation.ArgumentList.Arguments.Count == 0);
        var composeStatement = Assert.IsType<LocalDeclarationStatementSyntax>(composeCall.FirstAncestorOrSelf<StatementSyntax>());
        var readCurrentStatement = Assert.IsType<LocalDeclarationStatementSyntax>(readCurrentCall.FirstAncestorOrSelf<StatementSyntax>());
        var statements = composeBody.Statements.ToArray();
        var composeIndex = Array.IndexOf(statements, composeStatement);
        var readCurrentIndex = Array.IndexOf(statements, readCurrentStatement);
        Assert.True(composeIndex > 0, "The executor composition call must be a direct Compose-body statement.");
        Assert.True(readCurrentIndex > composeIndex, "ReadCurrent must follow the executor composition call.");
        Assert.Same(readinessGates[0], statements[composeIndex - 1]);
        Assert.Same(readinessGates[1], statements[composeIndex + 1]);
        Assert.Same(readinessGates[2], statements[readCurrentIndex + 1]);
        Assert.True(Regex.Matches(registry, "publicationAllowed\\(\\)").Count >= 3);
        AssertPersistenceCryptographicBoundary(registry, persistence);
        Assert.Contains("internal bool RollbackPublished(SessionEntry? entry)", registry, StringComparison.Ordinal);
        Assert.Contains("IdempotencyStateIdentity = idempotencyState", registry, StringComparison.Ordinal);
        Assert.Contains("ReferenceEquals(state, entry.IdempotencyStateIdentity)", registry, StringComparison.Ordinal);
        Assert.Contains("internal void InvalidateAll()", registry, StringComparison.Ordinal);
        Assert.Contains("_sessions.Clear();", registry, StringComparison.Ordinal);
        Assert.Contains("_expiredSessions.Clear();", registry, StringComparison.Ordinal);
        Assert.Contains("_idempotency.Clear();", registry, StringComparison.Ordinal);
        Assert.DoesNotContain("tenantId +", registry, StringComparison.Ordinal);
        var registrySyntaxRoot = CSharpSyntaxTree.ParseText(registry).GetRoot();
        var registryType = Assert.Single(
            registrySyntaxRoot.DescendantNodes().OfType<ClassDeclarationSyntax>(),
            declaration => declaration.Identifier.ValueText
                           == "DynamicGraphCompositionSessionRegistry");
        var currentReadEqualsCheckpoint = Assert.Single(
            registryType.Members.OfType<MethodDeclarationSyntax>(),
            method => method.Identifier.ValueText == "CurrentReadEqualsCheckpoint");
        Assert.Equal(
            2,
            currentReadEqualsCheckpoint.DescendantTokens()
                .Count(token => token.ValueText == "TenantId"));
        var expectedTenantEquality = SyntaxFactory.ParseExpression(
            "string.Equals(current.Descriptor.TenantId, profile.TenantId, StringComparison.Ordinal)");
        Assert.Contains(
            currentReadEqualsCheckpoint.DescendantNodes().OfType<InvocationExpressionSyntax>(),
            invocation => HasEquivalentTokens(invocation, expectedTenantEquality));
        var registryOutsideCheckpointEquality = registry.Remove(
            currentReadEqualsCheckpoint.FullSpan.Start,
            currentReadEqualsCheckpoint.FullSpan.Length);
        Assert.DoesNotContain(
            "TenantId,",
            registryOutsideCheckpointEquality,
            StringComparison.Ordinal);
        Assert.Contains("readiness.RegisterInvalidation(registryOwner.InvalidateAll);", program, StringComparison.Ordinal);
        Assert.Contains("registry.RollbackPublished(created.Session);", program, StringComparison.Ordinal);
        var readinessType = Assert.Single(
            syntaxRoot.DescendantNodes().OfType<ClassDeclarationSyntax>(),
            declaration => declaration.Identifier.ValueText == "StartupReadinessState");
        AssertInvalidationRaceBoundary(Assert.Single(
            readinessType.Members.OfType<MethodDeclarationSyntax>(),
            method => method.Identifier.ValueText == "FailClosed"));
        AssertInvalidationRaceBoundary(Assert.Single(
            readinessType.Members.OfType<MethodDeclarationSyntax>(),
            method => method.Identifier.ValueText == "OnAssemblyLoad"));
        Assert.Contains("ApplicationStopping.Register", program, StringComparison.Ordinal);
        Assert.Contains("finally\n{\n    ReadyFile.RemoveSafe", program.Replace("\r\n", "\n"), StringComparison.Ordinal);
        Assert.Contains("await app.DisposeAsync()", program, StringComparison.Ordinal);
    }

    [Fact]
    public void ReadyCleanupFunction_IsIdempotentAndApplicationStoppingAndFinallyAreWired()
    {
        var repoRoot = FindRepoRoot();
        var serverAssemblyPath = Path.Combine(
            repoRoot, "tools", "DynamicGraphCompositionLiveLocalServer", "bin", "Debug", "net9.0",
            "DynamicGraphCompositionLiveLocalServer.dll");
        Assert.True(
            File.Exists(serverAssemblyPath),
            $"Build the live-local server before running the ready cleanup reflection test: {serverAssemblyPath}");
        var assembly = Assembly.LoadFrom(serverAssemblyPath);
        var readyFileTypes = assembly.GetTypes()
            .Where(type => type.FullName == "ReadyFile")
            .ToArray();
        Assert.True(
            readyFileTypes.Length == 1,
            $"Expected exactly one ReadyFile type, but found {readyFileTypes.Length}.");
        var readyFileType = readyFileTypes[0];
        var removeSafeMethods = readyFileType
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Where(method => method.Name == "RemoveSafe")
            .ToArray();
        Assert.True(
            removeSafeMethods.Length == 1,
            $"Expected exactly one ReadyFile.RemoveSafe method, but found {removeSafeMethods.Length}: " +
            string.Join(", ", removeSafeMethods.Select(method => method.ToString())));
        var removeSafe = removeSafeMethods[0];
        Assert.True(
            removeSafe.ReturnType == typeof(void)
            && removeSafe.GetParameters() is [{ ParameterType: var parameterType }]
            && parameterType == typeof(string),
            $"Unexpected ReadyFile.RemoveSafe signature: {removeSafe}.");
        var root = CreateTempRoot();
        var readyFile = Path.Combine(root, "ready.json");
        try
        {
            File.WriteAllText(readyFile, "ready");
            InvokeStaticUnwrapped(removeSafe, [readyFile]);
            Assert.False(File.Exists(readyFile));
            InvokeStaticUnwrapped(removeSafe, [readyFile]);
            Assert.False(File.Exists(readyFile));

            var program = File.ReadAllText(Path.Combine(
                repoRoot, "tools", "DynamicGraphCompositionLiveLocalServer", "Program.cs"));
            Assert.Contains("ApplicationStopping.Register", program, StringComparison.Ordinal);
            Assert.Contains("finally\n{\n    ReadyFile.RemoveSafe", program.Replace("\r\n", "\n"), StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void SelectedTargetHttpBoundary_IsStrictOpaqueOrderedAndDoesNotWidenAuthority()
    {
        var serverRoot = Path.Combine(
            FindRepoRoot(), "tools", "DynamicGraphCompositionLiveLocalServer");
        var contracts = NormalizeSource(File.ReadAllText(Path.Combine(
            serverRoot, "DynamicGraphCompositionHttpContracts.cs")));
        var program = NormalizeSource(File.ReadAllText(Path.Combine(serverRoot, "Program.cs")));
        var contractsRoot = CSharpSyntaxTree.ParseText(contracts).GetRoot();
        AssertSourceJsonContract(
            contractsRoot,
            "AdmitSelectedTargetEvidenceHttpRequest",
            strict: true,
            "target",
            "authority_result",
            "novelty_request");
        AssertSourceJsonContract(
            contractsRoot,
            "RunAdmittedSelectedTargetHttpRequest",
            strict: true,
            "evidence_handle",
            "run_id",
            "scenario_id",
            "user_request");
        AssertSourceJsonContract(
            contractsRoot,
            "SelectedTargetEvidenceAdmissionHttpResponse",
            strict: false,
            "request_id",
            "session_id",
            "status",
            "evidence_handle",
            "expires_at_utc",
            "boundaries");
        AssertSourceJsonContract(
            contractsRoot,
            "SelectedTargetRunSpeedHttpResponse",
            strict: false,
            "evaluated_row_count",
            "inference_elapsed_ms",
            "throughput_rows_per_sec",
            "routing_elapsed_ms",
            "candidate_sample_count",
            "batch_sample_count",
            "per_candidate_elapsed_ms",
            "per_batch_elapsed_ms");
        AssertSourceJsonContract(
            contractsRoot,
            "AdmittedSelectedTargetRunHttpResponse",
            strict: false,
            "request_id",
            "session_id",
            "status",
            "target_id",
            "execution_id",
            "evidence_ids",
            "direct_target_invocation_count",
            "run_speed",
            "boundaries");
        Assert.Contains("PropertyNameCaseInsensitive = false", contracts, StringComparison.Ordinal);
        Assert.Contains(
            "UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow",
            contracts,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "[property: JsonPropertyName(\"association\")]",
            contracts,
            StringComparison.Ordinal);
        Assert.All(
            Regex.Matches(
                    contracts,
                    "JsonPropertyName\\(\\\"(?<name>[^\\\"]+)\\\"\\)",
                    RegexOptions.CultureInvariant)
                .Cast<Match>()
                .Select(match => match.Groups["name"].Value),
            propertyName => Assert.Equal(
                propertyName.ToLowerInvariant(),
                propertyName,
                StringComparer.Ordinal));
        Assert.Contains(
            "DynamicGraphEdgeAuthorityAblationEvaluationRunner.ImportValidated(",
            contracts,
            StringComparison.Ordinal);
        Assert.DoesNotContain("System.Reflection", contracts, StringComparison.Ordinal);
        Assert.DoesNotContain("BindingFlags", contracts, StringComparison.Ordinal);
        Assert.DoesNotContain(".Invoke(", contracts, StringComparison.Ordinal);

        const string evidenceRoute =
            "POST /api/v1/dynamic-graph/sessions/{sessionId}/selected-target-evidence";
        const string runRoute =
            "POST /api/v1/dynamic-graph/sessions/{sessionId}/selected-target-runs";
        Assert.Contains(
            "app.MapPost(\"/api/v1/dynamic-graph/sessions/{sessionId}/selected-target-evidence\"",
            program,
            StringComparison.Ordinal);
        Assert.Contains($"\"{evidenceRoute}\"", program, StringComparison.Ordinal);
        Assert.Contains(
            "app.MapPost(\"/api/v1/dynamic-graph/sessions/{sessionId}/selected-target-runs\"",
            program,
            StringComparison.Ordinal);
        Assert.Contains($"\"{runRoute}\"", program, StringComparison.Ordinal);
        Assert.True(
            program.IndexOf($"\"{evidenceRoute}\"", StringComparison.Ordinal)
            < program.IndexOf($"\"{runRoute}\"", StringComparison.Ordinal));
        Assert.True(
            program.IndexOf($"\"{runRoute}\"", StringComparison.Ordinal)
            < program.IndexOf(
                "\"POST /api/v1/dynamic-graph/sessions/{sessionId}/runs\"",
                StringComparison.Ordinal));

        var admissionHandler = ExtractUniqueBoundedSourceBlock(
            program,
            "app.MapPost(\"/api/v1/dynamic-graph/sessions/{sessionId}/selected-target-evidence\"",
            "app.MapPost(\"/api/v1/dynamic-graph/sessions/{sessionId}/selected-target-runs\"");
        AssertFirstOccurrencesInOrder(
            admissionHandler,
            "if (!readiness.IsReady)",
            "if (!context.Request.HasJsonContentType())",
            "if (context.Request.ContentLength > options.MaxRequestBodyBytes)",
            "ValidateConfiguredTenant(context, options, requestId)",
            "Regex.IsMatch(sessionId, \"^dgs_[0-9a-f]{32}$\"",
            "DeserializeStrictAsync<AdmitSelectedTargetEvidenceHttpRequest>",
            "body?.Target is null || body.AuthorityResult is null || body.NoveltyRequest is null",
            "registry.AdmitSelectedTargetEvidence(",
            "SelectedTargetEvidenceAdmission(");
        Assert.Contains(
            "SelectedTargetEvidenceAdmissionStatus.CapacityExceeded",
            admissionHandler,
            StringComparison.Ordinal);
        Assert.Contains(
            "SelectedTargetEvidenceAdmissionStatus.ForbiddenAssembly",
            admissionHandler,
            StringComparison.Ordinal);

        var evidenceStatusMapper = ExtractUniqueBoundedSourceBlock(
            program,
            "static int StatusForSelectedTargetEvidenceCode(string code)",
            "static int StatusForSelectedTargetExecutionCode(string code)");
        AssertFirstOccurrencesInOrder(
            evidenceStatusMapper,
            "if (code == \"CANCELLED\")",
            "if (code == \"HOST_SELECTED_TARGET_EVIDENCE_ADMISSION_RESULT_REQUIRED\")",
            "if (code.EndsWith(\"_REQUIRED\", StringComparison.Ordinal))");
        Assert.Contains(
            "if (code == \"HOST_SELECTED_TARGET_EVIDENCE_ADMISSION_RESULT_REQUIRED\")\n"
            + "        return 500;",
            evidenceStatusMapper,
            StringComparison.Ordinal);
        Assert.Contains(
            "if (code.EndsWith(\"_REQUIRED\", StringComparison.Ordinal))\n"
            + "        return 400;",
            evidenceStatusMapper,
            StringComparison.Ordinal);

        var registryEvidenceTests = NormalizeSource(File.ReadAllText(Path.Combine(
            FindRepoRoot(),
            "Tests",
            "DynamicGraphCompositionSelectedTargetEvidenceHandleTests.cs")));
        var nullAdmissionEvidence = ExtractUniqueBoundedSourceBlock(
            registryEvidenceTests,
            "public async Task Admit_NullAndSuccessfulNullInvokerShapesReturnExactRequiredFailure()",
            "public async Task Admit_InitialForbiddenExpiredUnknownAndCapacityPrecedeCancellationAndInvocation()");
        Assert.Contains(
            "const string requiredCode = \"HOST_SELECTED_TARGET_EVIDENCE_ADMISSION_RESULT_REQUIRED\";",
            nullAdmissionEvidence,
            StringComparison.Ordinal);
        Assert.Contains(
            "new RecordingEvidenceInvoker { Handler = (_, _, _) => null }",
            nullAdmissionEvidence,
            StringComparison.Ordinal);
        Assert.Contains(".Ok(null!)", nullAdmissionEvidence, StringComparison.Ordinal);
        Assert.Contains(
            "AssertRequiredFailure(nullEnvelope, requiredCode, requiredMessage);",
            nullAdmissionEvidence,
            StringComparison.Ordinal);
        Assert.Contains(
            "AssertRequiredFailure(successfulNull, requiredCode, requiredMessage);",
            nullAdmissionEvidence,
            StringComparison.Ordinal);

        var executionHandler = ExtractUniqueBoundedSourceBlock(
            program,
            "app.MapPost(\"/api/v1/dynamic-graph/sessions/{sessionId}/selected-target-runs\"",
            "app.MapPost(\"/api/v1/dynamic-graph/sessions/{sessionId}/topology-mutations\"");
        AssertFirstOccurrencesInOrder(
            executionHandler,
            "if (!readiness.IsReady)",
            "if (!context.Request.HasJsonContentType())",
            "if (context.Request.ContentLength > options.MaxRequestBodyBytes)",
            "ValidateConfiguredTenant(context, options, requestId)",
            "Regex.IsMatch(sessionId, \"^dgs_[0-9a-f]{32}$\"",
            "DeserializeStrictAsync<RunAdmittedSelectedTargetHttpRequest>",
            "\"^dgeh_[0-9a-f]{32}$\"",
            "DynamicGraphCompositionServerOptions.IsSafeIdentifier(body?.RunId",
            "DynamicGraphCompositionServerOptions.IsSafeIdentifier(body?.ScenarioId",
            "IsSafeSelectedTargetInput(body?.UserRequest)",
            "registry.RunAdmittedSelectedTargetAsync(",
            "association = receipt.Association",
            "result = receipt.ExecutionResult",
            "runSpeed = receipt.RunSpeed",
            "receipt.DirectTargetInvocationCount != 1",
            "!IsValidSelectedTargetRunSpeedMeasurement(measurement)",
            "AdmittedSelectedTargetRun(");
        _ = Assert.Single(
            Regex.Matches(
                    executionHandler,
                    Regex.Escape("registry.RunAdmittedSelectedTargetAsync("),
                    RegexOptions.CultureInvariant)
                .Cast<Match>());
        Assert.DoesNotContain(
            "IDynamicGraphLogicalOrchestratorAdmittedSelectedTargetExecutionSession",
            executionHandler,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            ".RunAdmittedAsync(",
            executionHandler,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "DynamicGraphRegistryL3SelectedTargetExecutor",
            executionHandler,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "new DynamicGraphEdgeAuthoritySelectionTargetExecutionReceipt",
            executionHandler,
            StringComparison.Ordinal);
        _ = Assert.Single(
            Regex.Matches(
                executionHandler,
                Regex.Escape("if (!readiness.IsReady)"),
                RegexOptions.CultureInvariant)
                .Cast<Match>());
        var registryCallEnd = executionHandler.IndexOf(
            "catch (OperationCanceledException",
            executionHandler.IndexOf(
                "registry.RunAdmittedSelectedTargetAsync(",
                StringComparison.Ordinal),
            StringComparison.Ordinal);
        Assert.True(registryCallEnd > 0);
        var afterRegistryCall = executionHandler[registryCallEnd..];
        Assert.DoesNotContain(
            "if (!readiness.IsReady)",
            afterRegistryCall,
            StringComparison.Ordinal);
        Assert.Contains(
            "SelectedTargetSessionExecutionStatus.Unavailable",
            executionHandler,
            StringComparison.Ordinal);
        Assert.Contains(
            "receipt.DirectTargetInvocationCount != 1",
            executionHandler,
            StringComparison.Ordinal);
        Assert.True(
            executionHandler.IndexOf(
                "receipt.DirectTargetInvocationCount != 1",
                StringComparison.Ordinal)
            < executionHandler.IndexOf(
                "AdmittedSelectedTargetRun(",
                StringComparison.Ordinal));

        var responseHelper = ExtractUniqueBoundedSourceBlock(
            program,
            "static IResult AdmittedSelectedTargetRun(",
            "static bool IsNonFatalException(");
        Assert.DoesNotContain(".Association", responseHelper, StringComparison.Ordinal);
        Assert.DoesNotContain("receipt.", responseHelper, StringComparison.Ordinal);
        Assert.Contains(
            "new SelectedTargetRunSpeedHttpResponse(",
            responseHelper,
            StringComparison.Ordinal);
        Assert.Contains("runSpeed.EvaluatedRowCount", responseHelper, StringComparison.Ordinal);
        Assert.Contains("axis.InferenceElapsedMs", responseHelper, StringComparison.Ordinal);
        Assert.Contains("axis.ThroughputRowsPerSec", responseHelper, StringComparison.Ordinal);
        Assert.Contains("axis.RoutingElapsedMs", responseHelper, StringComparison.Ordinal);
        Assert.Contains("runSpeed.CandidateSampleCount", responseHelper, StringComparison.Ordinal);
        Assert.Contains("runSpeed.BatchSampleCount", responseHelper, StringComparison.Ordinal);
        Assert.Contains("axis.PerCandidateElapsedMs", responseHelper, StringComparison.Ordinal);
        Assert.Contains("axis.PerBatchElapsedMs", responseHelper, StringComparison.Ordinal);
        Assert.DoesNotContain("MeasureRunSpeed", executionHandler, StringComparison.Ordinal);
        Assert.DoesNotContain("MeasureRunSpeed", responseHelper, StringComparison.Ordinal);
        Assert.DoesNotContain("ThroughputRowsPerSec =", responseHelper, StringComparison.Ordinal);

        var serverAssemblyPath = Path.Combine(
            serverRoot,
            "bin",
            "Debug",
            "net9.0",
            "DynamicGraphCompositionLiveLocalServer.dll");
        Assert.True(
            File.Exists(serverAssemblyPath),
            $"Build the live-local server before running the selected-target boundary test: {serverAssemblyPath}");
        var assembly = Assembly.LoadFrom(serverAssemblyPath);
        var runSpeedValidator = Assert.Single(
            assembly.GetTypes()
                .SelectMany(type => type.GetMethods(
                    BindingFlags.Static
                    | BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.DeclaredOnly)),
            method => method.Name.Contains(
                          "IsValidSelectedTargetRunSpeedMeasurement",
                          StringComparison.Ordinal)
                      && method.GetParameters().Length == 1);
        Assert.Equal(typeof(bool), runSpeedValidator.ReturnType);
        Assert.Equal(
            typeof(DynamicGraphRunSpeedMeasurement),
            Assert.Single(runSpeedValidator.GetParameters()).ParameterType);
        var validMeasurement = new DynamicGraphRunSpeedMeasurement(
            EvaluatedRowCount: 1,
            CandidateSampleCount: 0,
            BatchSampleCount: 0,
            RunSpeed: new DynamicGraphRunSpeedAxis(
                InferenceElapsedMs: 10,
                ThroughputRowsPerSec: 100d,
                RoutingElapsedMs: 0,
                PerCandidateElapsedMs: null,
                PerBatchElapsedMs: null));
        var forgedMeasurement = validMeasurement with
        {
            RunSpeed = validMeasurement.RunSpeed with
            {
                ThroughputRowsPerSec = 7d
            }
        };

        Assert.True(Assert.IsType<bool>(
            InvokeStaticUnwrapped(runSpeedValidator, [validMeasurement])));
        Assert.False(Assert.IsType<bool>(
            InvokeStaticUnwrapped(runSpeedValidator, [forgedMeasurement])));
    }

    [Fact]
    public void CurrentTopologyHttpBoundary_IsBoundedReadOnlyAndFailClosed()
    {
        var serverRoot = Path.Combine(
            FindRepoRoot(), "tools", "DynamicGraphCompositionLiveLocalServer");
        var contracts = NormalizeSource(File.ReadAllText(Path.Combine(
            serverRoot, "DynamicGraphCompositionHttpContracts.cs")));
        var registry = NormalizeSource(File.ReadAllText(Path.Combine(
            serverRoot, "DynamicGraphCompositionSessionRegistry.cs")));
        var program = NormalizeSource(File.ReadAllText(Path.Combine(serverRoot, "Program.cs")));
        var contractsRoot = CSharpSyntaxTree.ParseText(contracts).GetRoot();

        AssertSourceJsonContract(
            contractsRoot,
            "CompositionSessionHttpResponse",
            strict: false,
            "request_id",
            "session_id",
            "status",
            "created_at_utc",
            "expires_at_utc",
            "result",
            "boundaries");
        AssertSourceJsonContract(
            contractsRoot,
            "CurrentTopologyEvidenceHttpResponse",
            strict: false,
            "profile_id",
            "revision",
            "signal_id",
            "source_kind",
            "node_count",
            "edge_count");
        AssertSourceJsonContract(
            contractsRoot,
            "CurrentTopologyHttpResponse",
            strict: false,
            "request_id",
            "session_id",
            "status",
            "bootstrap",
            "current",
            "revision_advanced_since_bootstrap",
            "boundaries");
        Assert.DoesNotContain("JsonPropertyName(\"result_scope\")", contracts, StringComparison.Ordinal);
        var topologyContracts = ExtractUniqueBoundedSourceBlock(
            contracts,
            "public sealed record CurrentTopologyEvidenceHttpResponse(",
            "[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]\npublic sealed record RunCompositionSessionHttpRequest(");
        foreach (var forbidden in new[]
                 {
                     "source_ref",
                     "trace_ref",
                     "topology",
                     "nodes",
                     "edges",
                     "tenant_id",
                     "domain_tag",
                     "context_id",
                     "budget",
                     "mutation_id",
                     "training"
                 })
        {
            Assert.DoesNotContain(
                $"JsonPropertyName(\"{forbidden}\")",
                topologyContracts,
                StringComparison.Ordinal);
        }

        const string route = "GET /api/v1/dynamic-graph/sessions/{sessionId}/topology";
        Assert.Equal(
            1,
            Regex.Matches(program, $"\"{Regex.Escape(route)}\"", RegexOptions.CultureInvariant).Count);
        var handler = ExtractUniqueBoundedSourceBlock(
            program,
            "app.MapGet(\"/api/v1/dynamic-graph/sessions/{sessionId}/topology\"",
            "app.MapPost(\"/api/v1/dynamic-graph/sessions/{sessionId}/selected-target-evidence\"");
        var handlerRoot = CSharpSyntaxTree.ParseText(handler).GetRoot();
        AssertFirstOccurrencesInOrder(
            handler,
            "if (!readiness.IsReady)",
            "ValidateConfiguredTenant(context, options, requestId)",
            "Regex.IsMatch(sessionId, \"^dgs_[0-9a-f]{32}$\"",
            "registry.ReadCurrentTopology(",
            "CurrentTopologySessionStatus.Expired",
            "CurrentTopologySessionStatus.Unknown",
            "CurrentTopologySessionStatus.Unavailable",
            "CurrentTopologySessionStatus.ForbiddenAssembly",
            "operation is null || (operation.IsSuccess && operation.Value is null)",
            "!operation.IsSuccess",
            "CreateCurrentTopologyResponse(");
        AssertIfReturnsExactError(
            handlerRoot,
            "read.Status == CurrentTopologySessionStatus.Expired",
            410,
            "HOST_SESSION_EXPIRED");
        AssertIfReturnsExactError(
            handlerRoot,
            "read.Status == CurrentTopologySessionStatus.Unknown",
            404,
            "HOST_SESSION_NOT_FOUND");
        AssertIfReturnsExactError(
            handlerRoot,
            "read.Status == CurrentTopologySessionStatus.Unavailable",
            422,
            "HOST_TOPOLOGY_READ_UNAVAILABLE");
        AssertIfReturnsExactError(
            handlerRoot,
            "read.Status == CurrentTopologySessionStatus.ForbiddenAssembly",
            503,
            "FORBIDDEN_TRAINING_ASSEMBLY_LOADED");
        AssertIfReturnsExactError(
            handlerRoot,
            "read.Session is null || operation is null || (operation.IsSuccess && operation.Value is null)",
            500,
            "HOST_TOPOLOGY_READ_RESULT_REQUIRED");
        AssertIfReturnsExactError(
            handlerRoot,
            "!operation.IsSuccess",
            500,
            "HOST_TOPOLOGY_READ_FAILED");
        var exceptionCatch = Assert.Single(
            handlerRoot.DescendantNodes().OfType<CatchClauseSyntax>());
        Assert.True(HasEquivalentTokens(
            Assert.IsType<CatchFilterClauseSyntax>(exceptionCatch.Filter).FilterExpression,
            SyntaxFactory.ParseExpression("IsNonFatalException(exception)")));
        _ = AssertReturnsExactError(
            exceptionCatch.Block,
            500,
            "HOST_TOPOLOGY_READ_EXCEPTION");
        Assert.DoesNotContain("SafeDownstreamCode", handler, StringComparison.Ordinal);
        Assert.DoesNotContain("operation.Error", handler, StringComparison.Ordinal);
        Assert.DoesNotContain("_runSlots", handler, StringComparison.Ordinal);

        var registryRead = ExtractUniqueBoundedSourceBlock(
            registry,
            "internal CurrentTopologySessionResult ReadCurrentTopology(",
            "private static CurrentTopologySessionResult? MapCurrentTopologyLookup(");
        Assert.Equal(
            3,
            Regex.Matches(
                registryRead,
                Regex.Escape("GetCore(tenantId, sessionId)"),
                RegexOptions.CultureInvariant).Count);
        Assert.Equal(
            1,
            Regex.Matches(
                registryRead,
                Regex.Escape("entry.ReadCurrentTopology()"),
                RegexOptions.CultureInvariant).Count);
        Assert.Equal(
            3,
            Regex.Matches(
                registryRead,
                Regex.Escape("if (IsInvalidated || !readAllowed())"),
                RegexOptions.CultureInvariant).Count);
        AssertFirstOccurrencesInOrder(
            registryRead,
            "if (IsInvalidated || !readAllowed())",
            "var preLookup = GetCore(tenantId, sessionId)",
            "var readLookup = GetCore(tenantId, sessionId)",
            "ReferenceEquals(preLookup.Session, readLookup.Session)",
            "var result = entry.ReadCurrentTopology()",
            "var postLookup = GetCore(tenantId, sessionId)",
            "ReferenceEquals(entry, postLookup.Session)");
        Assert.DoesNotContain("_runSlots", registryRead, StringComparison.Ordinal);
        Assert.DoesNotContain("catch", registryRead, StringComparison.Ordinal);

        var response = ExtractUniqueBoundedSourceBlock(
            program,
            "static CurrentTopologyHttpResponse? CreateCurrentTopologyResponse(",
            "static bool IsSafeTopologyEvidenceSignal(");
        Assert.Contains("current.Revision < bootstrap.Revision", response, StringComparison.Ordinal);
        Assert.Contains(
            "bootstrap.NodeCount is < 0 or > DynamicGraphCompositionServerOptions.MaxGraphNodes",
            response,
            StringComparison.Ordinal);
        Assert.Contains(
            "bootstrap.EdgeCount is < 0 or > DynamicGraphCompositionServerOptions.MaxGraphEdges",
            response,
            StringComparison.Ordinal);
        Assert.Contains(
            "!string.Equals(retainedDescriptor.TenantId, configuredTenantId, StringComparison.Ordinal)",
            response,
            StringComparison.Ordinal);
        Assert.Contains(
            "current.InitialRevisionAdvanced != bootstrap.InitialRevisionAdvanced",
            response,
            StringComparison.Ordinal);
        Assert.Contains("Enum.IsDefined(current.SourceKind)", response, StringComparison.Ordinal);
        Assert.Contains(
            "string.Equals(\n            bootstrap.SourceKind,\n            bootstrapSourceKind.ToString(),\n            StringComparison.Ordinal)",
            response,
            StringComparison.Ordinal);
        Assert.Contains("current.Revision == bootstrap.Revision", response, StringComparison.Ordinal);
        Assert.Contains("current.SourceKind != bootstrapSourceKind", response, StringComparison.Ordinal);
        Assert.Contains("current.NodeCount != bootstrap.NodeCount", response, StringComparison.Ordinal);
        Assert.Contains("current.EdgeCount != bootstrap.EdgeCount", response, StringComparison.Ordinal);
        Assert.Contains("current.Revision > bootstrap.Revision", response, StringComparison.Ordinal);
        Assert.Contains("Persistent: persistent", response, StringComparison.Ordinal);
        Assert.Contains("ExecutesAssistant: false", response, StringComparison.Ordinal);
        Assert.Contains("AppliesGraph: false", response, StringComparison.Ordinal);
        Assert.Contains("PerformsTraining: false", response, StringComparison.Ordinal);
    }

    [Fact]
    public void CurrentTopologyResponse_CoherenceRejectsEveryMismatchedCarrier()
    {
        var serverAssemblyPath = Path.Combine(
            FindRepoRoot(),
            "tools",
            "DynamicGraphCompositionLiveLocalServer",
            "bin",
            "Debug",
            "net9.0",
            "DynamicGraphCompositionLiveLocalServer.dll");
        Assert.True(
            File.Exists(serverAssemblyPath),
            $"Build the live-local server before running the topology response reflection test: {serverAssemblyPath}");
        var assembly = Assembly.LoadFrom(serverAssemblyPath);
        var method = Assert.Single(
            assembly.GetTypes()
                .SelectMany(type => type.GetMethods(
                    BindingFlags.Static
                    | BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.DeclaredOnly)),
            candidate => candidate.Name.Contains(
                             "CreateCurrentTopologyResponse",
                             StringComparison.Ordinal)
                         && candidate.GetParameters().Length == 6);
        var bootstrapType = Assert.Single(
            assembly.GetTypes(),
            type => type.FullName
                    == "LlmFromScratch.DynamicGraphCompositionLiveLocalServer.CompositionEvidenceHttpResponse");
        var sessionEntryType = Assert.Single(
            assembly.GetTypes(),
            type => type.FullName
                    == "LlmFromScratch.DynamicGraphCompositionLiveLocalServer.DynamicGraphCompositionSessionRegistry+SessionEntry");
        var sessionEntryConstructor = Assert.Single(
            sessionEntryType.GetConstructors(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic),
            constructor => constructor.GetParameters().Length == 9);

        object CreateBootstrap(
            string profileId = "profile-current",
            long revision = 1,
            string sourceKind = "LogicalRegistryProjection",
            int nodeCount = 2,
            int edgeCount = 1,
            bool initialRevisionAdvanced = true)
        {
            var value = Activator.CreateInstance(
                bootstrapType,
                [
                    profileId,
                    revision,
                    sourceKind,
                    nodeCount,
                    edgeCount,
                    initialRevisionAdvanced
                ]);
            Assert.NotNull(value);
            return value;
        }

        object CreateSession(
            object candidateBootstrap,
            DynamicGraphLogicalOrchestratorSessionDescriptor candidateDescriptor)
            => sessionEntryConstructor.Invoke(
                [
                    "dgs_" + new string('a', 32),
                    "opaque-tenant-index",
                    DateTimeOffset.UtcNow,
                    DateTimeOffset.UtcNow.AddMinutes(1),
                    candidateBootstrap,
                    new CurrentTopologyReadSession(candidateDescriptor),
                    "opaque-idempotency-index",
                    new string('A', 64),
                    new object()
                ]);

        var descriptor = new DynamicGraphLogicalOrchestratorSessionDescriptor(
            "profile-current",
            TenantId,
            "domain-current",
            "context-current");
        var bootstrap = CreateBootstrap();
        var session = CreateSession(bootstrap, descriptor);
        var valid = new DynamicGraphLogicalOrchestratorSessionResult(
            descriptor,
            1,
            "signal-current",
            DynamicGraphBuildSourceKind.LogicalRegistryProjection,
            2,
            1,
            true);

        object Invoke(
            object candidateSession,
            DynamicGraphLogicalOrchestratorSessionResult candidateCurrent)
        {
            var value = InvokeStaticUnwrapped(
                method,
                [
                    "request.current",
                    "dgs_" + new string('a', 32),
                    TenantId,
                    candidateSession,
                    candidateCurrent,
                    false
                ]);
            Assert.NotNull(value);
            Assert.Equal("CurrentTopologyHttpResponse", value.GetType().Name);
            return value;
        }

        var response = Invoke(session, valid);
        var responseJson = JsonSerializer.SerializeToElement(response);
        Assert.Equal("current", responseJson.GetProperty("status").GetString());
        Assert.Equal(1, responseJson.GetProperty("bootstrap").GetProperty("revision").GetInt64());
        Assert.Equal(1, responseJson.GetProperty("current").GetProperty("revision").GetInt64());
        Assert.False(responseJson.GetProperty("revision_advanced_since_bootstrap").GetBoolean());
        Assert.False(responseJson.GetProperty("boundaries").GetProperty("persistent").GetBoolean());
        Assert.False(responseJson.GetProperty("boundaries").GetProperty("executes_assistant").GetBoolean());
        Assert.False(responseJson.GetProperty("boundaries").GetProperty("applies_graph").GetBoolean());
        Assert.False(responseJson.GetProperty("boundaries").GetProperty("performs_training").GetBoolean());

        object? InvokeRaw(
            object candidateSession,
            DynamicGraphLogicalOrchestratorSessionResult candidateCurrent)
            => InvokeStaticUnwrapped(
                method,
                [
                    "request.current",
                    "dgs_" + new string('a', 32),
                    TenantId,
                    candidateSession,
                    candidateCurrent,
                    false
                ]);

        var mismatchedCurrent = new[]
        {
            valid with
            {
                Descriptor = descriptor with { ProfileId = "other-profile" }
            },
            valid with
            {
                Descriptor = descriptor with { TenantId = "other-tenant" }
            },
            valid with
            {
                Descriptor = descriptor with { DomainTag = "other-domain" }
            },
            valid with
            {
                Descriptor = descriptor with { ContextId = "other-context" }
            },
            valid with { Revision = 0 },
            valid with { SourceKind = (DynamicGraphBuildSourceKind)999 },
            valid with { NodeCount = -1 },
            valid with { NodeCount = 129 },
            valid with { EdgeCount = -1 },
            valid with { EdgeCount = 257 },
            valid with { SignalId = "unsafe\nsignal" },
            valid with { SignalId = new string('s', 129) },
            valid with { SourceKind = DynamicGraphBuildSourceKind.DeterministicBootstrap },
            valid with { NodeCount = 3 },
            valid with { EdgeCount = 2 }
        };
        Assert.All(mismatchedCurrent, current => Assert.Null(InvokeRaw(session, current)));

        var invalidBootstrapCases = new[]
        {
            CreateBootstrap(profileId: "other-profile"),
            CreateBootstrap(revision: 0),
            CreateBootstrap(sourceKind: "UnknownSourceKind")
        };
        Assert.All(
            invalidBootstrapCases,
            invalidBootstrap => Assert.Null(InvokeRaw(
                CreateSession(invalidBootstrap, descriptor),
                valid)));

        Assert.Null(InvokeRaw(
            CreateSession(
                bootstrap,
                descriptor with { ProfileId = "retained-other-profile" }),
            valid));
        var wrongTenantDescriptor = descriptor with { TenantId = "tenant-other" };
        Assert.Null(InvokeRaw(
            CreateSession(bootstrap, wrongTenantDescriptor),
            valid with { Descriptor = wrongTenantDescriptor }));
        Assert.Null(InvokeRaw(
            session,
            valid with { InitialRevisionAdvanced = false }));

        var falseBootstrap = CreateBootstrap(initialRevisionAdvanced: false);
        var falseSession = CreateSession(falseBootstrap, descriptor);
        Assert.Null(InvokeRaw(falseSession, valid));
        Assert.NotNull(InvokeRaw(
            falseSession,
            valid with { InitialRevisionAdvanced = false }));

        var advancedCurrent = valid with
        {
            Revision = 2,
            SignalId = "signal-machine-proposal",
            SourceKind = DynamicGraphBuildSourceKind.MachineTopologyProposal,
            NodeCount = 3,
            EdgeCount = 2
        };
        var invalidAdvancedBootstrapCases = new[]
        {
            CreateBootstrap(nodeCount: -1),
            CreateBootstrap(nodeCount: 129),
            CreateBootstrap(edgeCount: -1),
            CreateBootstrap(edgeCount: 257),
            CreateBootstrap(sourceKind: "1"),
            CreateBootstrap(sourceKind: " LogicalRegistryProjection "),
            CreateBootstrap(sourceKind: "logicalRegistryProjection")
        };
        Assert.All(
            invalidAdvancedBootstrapCases,
            invalidBootstrap => Assert.Null(InvokeRaw(
                CreateSession(invalidBootstrap, descriptor),
                advancedCurrent)));

        foreach (var sourceKind in Enum.GetValues<DynamicGraphBuildSourceKind>())
        {
            Assert.NotNull(InvokeRaw(
                CreateSession(CreateBootstrap(sourceKind: sourceKind.ToString()), descriptor),
                advancedCurrent));
        }

        var advanced = Invoke(session, advancedCurrent);
        var advancedJson = JsonSerializer.SerializeToElement(advanced);
        Assert.True(advancedJson.GetProperty("revision_advanced_since_bootstrap").GetBoolean());
        Assert.Equal(2, advancedJson.GetProperty("current").GetProperty("revision").GetInt64());
        Assert.Equal(
            "MachineTopologyProposal",
            advancedJson.GetProperty("current").GetProperty("source_kind").GetString());
    }

    [Fact]
    public void TopologyMutationHttpBoundary_IsStrictEphemeralCapabilityOwnedAndMutationSensitive()
    {
        var serverRoot = Path.Combine(
            FindRepoRoot(), "tools", "DynamicGraphCompositionLiveLocalServer");
        var contracts = NormalizeSource(File.ReadAllText(Path.Combine(
            serverRoot, "DynamicGraphCompositionHttpContracts.cs")));
        var registry = NormalizeSource(File.ReadAllText(Path.Combine(
            serverRoot, "DynamicGraphCompositionSessionRegistry.cs")));
        var program = NormalizeSource(File.ReadAllText(Path.Combine(serverRoot, "Program.cs")));
        var contractsRoot = CSharpSyntaxTree.ParseText(contracts).GetRoot();

        AssertSourceJsonContract(
            contractsRoot,
            "ApplyTopologyMutationHttpRequest",
            strict: true,
            "expected_revision",
            "profile");
        AssertSourceJsonContract(
            contractsRoot,
            "TopologyMutationAttemptHttpResponse",
            strict: false,
            "signal_id",
            "source_kind",
            "mutation_applied");
        AssertSourceJsonContract(
            contractsRoot,
            "TopologyMutationCurrentHttpResponse",
            strict: false,
            "revision",
            "signal_id",
            "source_kind",
            "node_count",
            "edge_count");
        AssertSourceJsonContract(
            contractsRoot,
            "TopologyMutationBudgetHttpResponse",
            strict: false,
            "max_nodes",
            "actual_nodes",
            "max_edges",
            "actual_edges",
            "max_depth",
            "actual_depth",
            "max_parallel_branches",
            "actual_parallel_branches",
            "within_budget",
            "exceeded_dimensions");
        AssertSourceJsonContract(
            contractsRoot,
            "TopologyMutationHttpResponse",
            strict: false,
            "request_id",
            "session_id",
            "status",
            "previous_revision",
            "current_revision",
            "revision_advanced",
            "attempt",
            "current",
            "budget",
            "boundaries");
        var mutationContracts = ExtractUniqueBoundedSourceBlock(
            contracts,
            "[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]\npublic sealed record ApplyTopologyMutationHttpRequest(",
            "public sealed record AssistantRunHttpResponse(");
        foreach (var forbidden in new[]
                 {
                     "JsonPropertyName(\"source_ref\")",
                     "JsonPropertyName(\"trace_ref\")",
                     "JsonPropertyName(\"topology\")",
                     "JsonPropertyName(\"nodes\")",
                     "JsonPropertyName(\"edges\")",
                     "JsonPropertyName(\"mutation_evidence\")",
                     "JsonPropertyName(\"added_node",
                     "JsonPropertyName(\"removed_node",
                     "JsonPropertyName(\"updated_node",
                     "JsonPropertyName(\"added_edge",
                     "JsonPropertyName(\"removed_edge",
                     "JsonPropertyName(\"updated_edge"
                 })
        {
            Assert.DoesNotContain(forbidden, mutationContracts, StringComparison.Ordinal);
        }

        const string mutationRoute =
            "POST /api/v1/dynamic-graph/sessions/{sessionId}/topology-mutations";
        const string topologyRoute =
            "POST /api/v1/dynamic-graph/sessions/{sessionId}/topology-runs";
        Assert.True(
            program.IndexOf($"\"{mutationRoute}\"", StringComparison.Ordinal)
            < program.IndexOf($"\"{topologyRoute}\"", StringComparison.Ordinal));
        var handler = ExtractUniqueBoundedSourceBlock(
            program,
            "app.MapPost(\"/api/v1/dynamic-graph/sessions/{sessionId}/topology-mutations\"",
            "app.MapPost(\"/api/v1/dynamic-graph/sessions/{sessionId}/topology-runs\"");
        var handlerRoot = CSharpSyntaxTree.ParseText(handler).GetRoot();
        AssertFirstOccurrencesInOrder(
            handler,
            "if (!readiness.IsReady)",
            "if (!context.Request.HasJsonContentType())",
            "if (context.Request.ContentLength > options.MaxRequestBodyBytes)",
            "ValidateConfiguredTenant(context, options, requestId)",
            "Regex.IsMatch(sessionId, \"^dgs_[0-9a-f]{32}$\"",
            "if (persistenceOwner.IsEnabled)",
            "DeserializeStrictAsync<ApplyTopologyMutationHttpRequest>",
            "body?.ExpectedRevision is null || body.ExpectedRevision < 0",
            "DynamicGraphCompositionProfilePolicy.Admit(body.Profile, options.TenantId)",
            "registry.ApplyTopologyMutationAsync(",
            "TopologyMutationSessionStatus.Expired",
            "TopologyMutationSessionStatus.Unknown",
            "TopologyMutationSessionStatus.Busy",
            "TopologyMutationSessionStatus.ForbiddenAssembly",
            "TopologyMutationSessionStatus.Unavailable",
            "TopologyMutationSessionStatus.EphemeralModeRequired",
            "CreateTopologyMutationResponse(");
        _ = Assert.Single(
            Regex.Matches(
                    handler,
                    Regex.Escape("registry.ApplyTopologyMutationAsync("),
                    RegexOptions.CultureInvariant)
                .Cast<Match>());
        var registryCallIndex = handler.IndexOf(
            "registry.ApplyTopologyMutationAsync(",
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "if (!readiness.IsReady)",
            handler[registryCallIndex..],
            StringComparison.Ordinal);
        AssertIfReturnsExactError(
            handlerRoot,
            "executed.Status == TopologyMutationSessionStatus.Expired",
            410,
            "HOST_SESSION_EXPIRED");
        AssertIfReturnsExactError(
            handlerRoot,
            "executed.Status == TopologyMutationSessionStatus.Unknown",
            404,
            "HOST_SESSION_NOT_FOUND");
        AssertIfReturnsExactError(
            handlerRoot,
            "executed.Status == TopologyMutationSessionStatus.Busy",
            429,
            "HOST_RUN_CONCURRENCY_EXHAUSTED");
        AssertIfReturnsExactError(
            handlerRoot,
            "executed.Status == TopologyMutationSessionStatus.ForbiddenAssembly",
            503,
            "FORBIDDEN_TRAINING_ASSEMBLY_LOADED");
        AssertIfReturnsExactError(
            handlerRoot,
            "executed.Status == TopologyMutationSessionStatus.Unavailable",
            422,
            "HOST_TOPOLOGY_MUTATION_UNAVAILABLE");
        AssertIfReturnsExactError(
            handlerRoot,
            "operation is null || (operation.IsSuccess && operation.Value is null)",
            500,
            "HOST_TOPOLOGY_MUTATION_RESULT_INCOHERENT");
        var operationFailure = Assert.Single(
            handlerRoot.DescendantNodes().OfType<IfStatementSyntax>(),
            statement => HasEquivalentTokens(
                statement.Condition,
                SyntaxFactory.ParseExpression("!operation.IsSuccess")));
        var operationFailureBlock = Assert.IsType<BlockSyntax>(operationFailure.Statement);
        AssertForwardedDownstreamCodeMapping(
            operationFailureBlock,
            "code == \"HOST_TOPOLOGY_MUTATION_PROFILE_INVALID\"",
            400,
            "The topology mutation profile was rejected.");
        AssertForwardedDownstreamCodeMapping(
            operationFailureBlock,
            "code is \"HOST_TOPOLOGY_MUTATION_PROFILE_ID_MISMATCH\""
            + " or \"HOST_TOPOLOGY_MUTATION_SCOPE_MISMATCH\""
            + " or \"DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_REVISION_CONFLICT\"",
            409,
            "The topology mutation conflicts with the current session.");
        AssertForwardedDownstreamCodeMapping(
            operationFailureBlock,
            "IsAllowedTopologyMutationCallerCode(code)",
            422,
            "The topology mutation profile was rejected.");
        _ = AssertReturnsExactError(
            operationFailureBlock.Statements[^1],
            500,
            "HOST_TOPOLOGY_MUTATION_EXCEPTION");

        var callerCodeWhitelist = ExtractUniqueBoundedSourceBlock(
            program,
            "static bool IsAllowedTopologyMutationCallerCode(string code)",
            "static bool IsSafeSelectedTargetInput(string? value)");
        var expectedCallerCodeWhitelist = NormalizeSource(
            """
            static bool IsAllowedTopologyMutationCallerCode(string code)
                => code is
                    "DYNAMIC_GRAPH_REGISTRY_PROJECTION_SCOPE_INVALID"
                    or "DYNAMIC_GRAPH_REGISTRY_PROJECTION_SIGNAL_REFERENCES_INVALID"
                    or "DYNAMIC_GRAPH_REGISTRY_PROJECTION_NODE_BINDINGS_REQUIRED"
                    or "DYNAMIC_GRAPH_REGISTRY_PROJECTION_EDGE_BINDINGS_INVALID"
                    or "DYNAMIC_GRAPH_REGISTRY_PROJECTION_DUPLICATE_NODE_BINDING"
                    or "DYNAMIC_GRAPH_REGISTRY_PROJECTION_SUB_PHASE_NOT_FOUND"
                    or "DYNAMIC_GRAPH_REGISTRY_PROJECTION_NODE_MISMATCH"
                    or "DYNAMIC_GRAPH_REGISTRY_PROJECTION_DUPLICATE_EDGE_BINDING"
                    or "DYNAMIC_GRAPH_REGISTRY_PROJECTION_TRANSITIONS_REQUIRED"
                    or "DYNAMIC_GRAPH_REGISTRY_PROJECTION_TRANSITION_NOT_PROJECTABLE"
                    or "DYNAMIC_GRAPH_REGISTRY_PROJECTION_DUPLICATE_TRANSITION_COVERAGE"
                    or "DYNAMIC_GRAPH_REGISTRY_PROJECTION_INCOMPLETE_TRANSITION_COVERAGE"
                    or "DYNAMIC_GRAPH_REGISTRY_PROJECTION_EDGE_SOURCE_MISMATCH"
                    or "DYNAMIC_GRAPH_TOPOLOGY_BUILD_BUDGET_INVALID"
                    or "DYNAMIC_GRAPH_TENANT_REQUIRED"
                    or "DYNAMIC_GRAPH_DOMAIN_REQUIRED"
                    or "DYNAMIC_GRAPH_CONTEXT_REQUIRED"
                    or "DYNAMIC_GRAPH_NODES_REQUIRED"
                    or "DYNAMIC_GRAPH_NODE_REQUIRED"
                    or "DYNAMIC_GRAPH_NODE_ID_REQUIRED"
                    or "DYNAMIC_GRAPH_DUPLICATE_NODE_ID"
                    or "DYNAMIC_GRAPH_NODE_DESCRIPTOR_REQUIRED"
                    or "DYNAMIC_GRAPH_NODE_DESCRIPTOR_ID_MISMATCH"
                    or "DYNAMIC_GRAPH_NODE_INPUT_TYPE_REQUIRED"
                    or "DYNAMIC_GRAPH_NODE_OUTPUT_TYPE_REQUIRED"
                    or "DYNAMIC_GRAPH_NODE_DOMAIN_REQUIRED"
                    or "DYNAMIC_GRAPH_DOMAIN_MISMATCH"
                    or "DYNAMIC_GRAPH_NODE_AUTHORITY_ROLE_REQUIRED"
                    or "DYNAMIC_GRAPH_NODE_AUTHORITY_ROLE_INVALID"
                    or "DYNAMIC_GRAPH_NODE_DETERMINISTIC_AUTHORITY_FORBIDDEN"
                    or "DYNAMIC_GRAPH_NODE_UNTRAINED_SEMANTIC_CLAIM"
                    or "DYNAMIC_GRAPH_NODE_SEMANTIC_CLAIM_ROLE_REQUIRED"
                    or "DYNAMIC_GRAPH_NODE_AUTHORITY_STATE_MISMATCH"
                    or "DYNAMIC_GRAPH_NODE_CHECKPOINT_REQUIRED"
                    or "DYNAMIC_GRAPH_NODE_MANIFEST_REQUIRED"
                    or "DYNAMIC_GRAPH_ORDER_INCOMPLETE"
                    or "DYNAMIC_GRAPH_ORDER_ENTRY_REQUIRED"
                    or "DYNAMIC_GRAPH_ORDER_ID_REQUIRED"
                    or "DYNAMIC_GRAPH_DUPLICATE_ORDER_ID"
                    or "DYNAMIC_GRAPH_ORDER_NODE_ID_REQUIRED"
                    or "DYNAMIC_GRAPH_DUPLICATE_ORDER_NODE"
                    or "DYNAMIC_GRAPH_DANGLING_ORDER_NODE"
                    or "DYNAMIC_GRAPH_ORDER_POSITION_INVALID"
                    or "DYNAMIC_GRAPH_DUPLICATE_ORDER_POSITION"
                    or "DYNAMIC_GRAPH_TENANT_MISMATCH"
                    or "DYNAMIC_GRAPH_CONTEXT_MISMATCH"
                    or "DYNAMIC_GRAPH_ORDER_NOT_CONTIGUOUS"
                    or "DYNAMIC_GRAPH_EDGE_REQUIRED"
                    or "DYNAMIC_GRAPH_EDGE_ID_REQUIRED"
                    or "DYNAMIC_GRAPH_DUPLICATE_EDGE_ID"
                    or "DYNAMIC_GRAPH_EDGE_KIND_INVALID"
                    or "DYNAMIC_GRAPH_EDGE_SOURCE_REQUIRED"
                    or "DYNAMIC_GRAPH_DANGLING_SOURCE_NODE"
                    or "DYNAMIC_GRAPH_EDGE_TARGETS_REQUIRED"
                    or "DYNAMIC_GRAPH_FAN_IN_CARDINALITY_INVALID"
                    or "DYNAMIC_GRAPH_MANY_TO_MANY_CARDINALITY_INVALID"
                    or "DYNAMIC_GRAPH_RETURN_LOOP_CARDINALITY_INVALID"
                    or "DYNAMIC_GRAPH_RETURN_LOOP_PARALLEL_FORBIDDEN"
                    or "DYNAMIC_GRAPH_EDGE_AUTHORITY_REQUIRED"
                    or "DYNAMIC_GRAPH_EDGE_AUTHORITY_LEVEL_REQUIRED"
                    or "DYNAMIC_GRAPH_EDGE_AUTHORITY_LEVEL_INVALID"
                    or "DYNAMIC_GRAPH_EDGE_DETERMINISTIC_AUTHORITY_FORBIDDEN"
                    or "DYNAMIC_GRAPH_EDGE_BASELINE_UNIT_REQUIRED"
                    or "DYNAMIC_GRAPH_EDGE_BASELINE_ROLE_REQUIRED"
                    or "DYNAMIC_GRAPH_EDGE_BASELINE_ROLE_INVALID"
                    or "DYNAMIC_GRAPH_EDGE_LEAKAGE_EXCLUSIONS_REQUIRED"
                    or "DYNAMIC_GRAPH_EDGE_LEAKAGE_EXCLUSION_REQUIRED"
                    or "DYNAMIC_GRAPH_EDGE_SOURCE_PROOF_REQUIRED"
                    or "DYNAMIC_GRAPH_EDGE_CUTOVER_PROOF_REQUIRED"
                    or "DYNAMIC_GRAPH_EDGE_ABLATION_PROOF_REQUIRED"
                    or "DYNAMIC_GRAPH_EDGE_TARGET_REQUIRED"
                    or "DYNAMIC_GRAPH_DUPLICATE_EDGE_TARGET"
                    or "DYNAMIC_GRAPH_DANGLING_TARGET_NODE"
                    or "DYNAMIC_GRAPH_SELF_EDGE"
                    or "DYNAMIC_GRAPH_RETURN_LOOP_DIRECTION_INVALID"
                    or "DYNAMIC_GRAPH_CYCLE_DETECTED";
            """);
        Assert.Equal(expectedCallerCodeWhitelist, callerCodeWhitelist);
        Assert.DoesNotContain(
            "DYNAMIC_GRAPH_TOPOLOGY_REQUIRED",
            callerCodeWhitelist,
            StringComparison.Ordinal);
        var cancellationCatches = handlerRoot.DescendantNodes()
            .OfType<CatchClauseSyntax>()
            .Where(clause => clause.Declaration?.Type is IdentifierNameSyntax
            {
                Identifier.ValueText: "OperationCanceledException"
            })
            .ToArray();
        Assert.Equal(2, cancellationCatches.Length);
        Assert.All(cancellationCatches, clause =>
        {
            var filter = Assert.IsType<CatchFilterClauseSyntax>(clause.Filter);
            Assert.True(HasEquivalentTokens(
                filter.FilterExpression,
                SyntaxFactory.ParseExpression(
                    "cancellationToken.IsCancellationRequested && FatalExceptionPolicy.IsNonFatal(exception)")));
            _ = AssertReturnsExactError(clause.Block, 408, "HOST_REQUEST_CANCELLED");
        });
        var exceptionCatch = Assert.Single(
            handlerRoot.DescendantNodes().OfType<CatchClauseSyntax>(),
            clause => clause.Declaration?.Type is IdentifierNameSyntax
            {
                Identifier.ValueText: "Exception"
            });
        Assert.True(HasEquivalentTokens(
            Assert.IsType<CatchFilterClauseSyntax>(exceptionCatch.Filter).FilterExpression,
            SyntaxFactory.ParseExpression("IsNonFatalException(exception)")));
        _ = AssertReturnsExactError(
            exceptionCatch.Block,
            500,
            "HOST_TOPOLOGY_MUTATION_EXCEPTION");

        var responseHelper = ExtractUniqueBoundedSourceBlock(
            program,
            "static TopologyMutationHttpResponse? CreateTopologyMutationResponse(",
            "static IResult SelectedTargetEvidenceAdmission(");
        var responseRoot = CSharpSyntaxTree.ParseText(responseHelper).GetRoot();
        var expectedCoherenceCondition = SyntaxFactory.ParseExpression(
            """
            build is null
            || build.Topology is null
            || build.BudgetProof is null
            || build.MutationEvidence is null
            || build.BudgetProof.ExceededDimensions is null
            || current is null
            || current.Topology is null
            || result.PreviousRevision != expectedRevision
            || result.CurrentRevision != current.Revision
            || !string.Equals(build.MutationEvidence.SignalId, profile.SignalId, StringComparison.Ordinal)
            || build.MutationEvidence.SourceKind != DynamicGraphBuildSourceKind.LogicalRegistryProjection
            || !string.Equals(build.MutationEvidence.SourceRef, profile.SourceRef, StringComparison.Ordinal)
            || !string.Equals(build.MutationEvidence.TraceRef, profile.TraceRef, StringComparison.Ordinal)
            || !string.Equals(build.Topology.TenantId, profile.TenantId, StringComparison.Ordinal)
            || !string.Equals(build.Topology.DomainTag, profile.DomainTag, StringComparison.Ordinal)
            || !string.Equals(build.Topology.ContextId, profile.ContextId, StringComparison.Ordinal)
            || current.SourceKind != DynamicGraphBuildSourceKind.LogicalRegistryProjection
            || !string.Equals(current.Topology.TenantId, profile.TenantId, StringComparison.Ordinal)
            || !string.Equals(current.Topology.DomainTag, profile.DomainTag, StringComparison.Ordinal)
            || !string.Equals(current.Topology.ContextId, profile.ContextId, StringComparison.Ordinal)
            || build.BudgetProof.MaxNodes != profile.Budget.MaxNodes
            || build.BudgetProof.MaxEdges != profile.Budget.MaxEdges
            || build.BudgetProof.MaxDepth != profile.Budget.MaxDepth
            || build.BudgetProof.MaxParallelBranches != profile.Budget.MaxParallelBranches
            || build.BudgetProof.ActualNodes != profile.NodeDefinitions.Length
            || build.BudgetProof.ActualEdges != profile.EdgeBindings.Length
            """);
        _ = Assert.Single(
            responseRoot.DescendantNodes().OfType<IfStatementSyntax>(),
            statement => HasEquivalentTokens(
                statement.Condition,
                expectedCoherenceCondition));
        var expectedBudgetProofValidation = NormalizeSource(
            """
            var expectedExceededDimensions = new List<string>(4);
            if (proof.ActualNodes > proof.MaxNodes)
                expectedExceededDimensions.Add("nodes");
            if (proof.ActualEdges > proof.MaxEdges)
                expectedExceededDimensions.Add("edges");
            if (proof.ActualDepth > proof.MaxDepth)
                expectedExceededDimensions.Add("depth");
            if (proof.ActualParallelBranches > proof.MaxParallelBranches)
                expectedExceededDimensions.Add("parallel_branches");

            if (!proof.ExceededDimensions.SequenceEqual(
                    expectedExceededDimensions,
                    StringComparer.Ordinal)
                || proof.WithinBudget != (expectedExceededDimensions.Count == 0))
            {
                return null;
            }
            """);
        Assert.Contains(
            expectedBudgetProofValidation,
            ExtractUniqueBoundedSourceBlock(
                responseHelper,
                "var expectedExceededDimensions = new List<string>(4);",
                "string status;"),
            StringComparison.Ordinal);
        Assert.Contains("\"applied\"", responseHelper, StringComparison.Ordinal);
        Assert.Contains("\"unchanged\"", responseHelper, StringComparison.Ordinal);
        Assert.Contains("\"budget_rejected\"", responseHelper, StringComparison.Ordinal);
        Assert.Contains(
            "\"DYNAMIC_GRAPH_TOPOLOGY_BUILD_BUDGET_EXCEEDED\"",
            responseHelper,
            StringComparison.Ordinal);
        Assert.Contains(
            "proof.ExceededDimensions.ToArray()",
            responseHelper,
            StringComparison.Ordinal);
        Assert.Contains(
            "current.SourceKind.ToString()",
            responseHelper,
            StringComparison.Ordinal);
        Assert.Contains(
            "result.CurrentRevision,\n        result.RevisionAdvanced,\n        new TopologyMutationAttemptHttpResponse(\n"
            + "            build.MutationEvidence.SignalId,\n"
            + "            build.MutationEvidence.SourceKind.ToString(),\n"
            + "            build.MutationEvidence.MutationApplied),\n"
            + "        new TopologyMutationCurrentHttpResponse(\n"
            + "            current.Revision,\n"
            + "            current.SignalId,\n"
            + "            current.SourceKind.ToString(),\n"
            + "            current.Topology.Nodes.Count,\n"
            + "            current.Topology.Edges.Count)",
            responseHelper,
            StringComparison.Ordinal);
        Assert.Contains(
            "Persistent: false,\n            ExecutesAssistant: false,\n            AppliesGraph: true,\n            PerformsTraining: false",
            responseHelper,
            StringComparison.Ordinal);
        var responseConstruction = Assert.Single(
            responseRoot.DescendantNodes().OfType<ObjectCreationExpressionSyntax>(),
            expression => expression.Type is IdentifierNameSyntax
            {
                Identifier.ValueText: "TopologyMutationHttpResponse"
            });
        Assert.DoesNotContain(
            responseConstruction.DescendantNodes().OfType<MemberAccessExpressionSyntax>(),
            access => access.Name.Identifier.ValueText is "SourceRef" or "TraceRef");

        var registryMethod = ExtractUniqueBoundedSourceBlock(
            registry,
            "internal async Task<TopologyMutationSessionResult> ApplyTopologyMutationAsync(",
            "public async Task<SelectedTargetSessionExecutionResult> RunSelectedTargetAsync(");
        Assert.DoesNotContain(
            "ExecuteSessionOperationAsync<",
            registryMethod,
            StringComparison.Ordinal);
        AssertFirstOccurrencesInOrder(
            registryMethod,
            "if (_persistence.IsEnabled)",
            "if (IsInvalidated || !executionAllowed())",
            "DynamicGraphCompositionProfilePolicy.Admit(profile, tenantId)",
            "var lookup = GetCore(tenantId, sessionId)",
            "if (!lookup.Session.SupportsTopologyMutation)",
            "ValidateTopologyMutationScope(lookup.Session, admittedProfile)",
            "_runSlots.WaitAsync(0, cancellationToken)");
        var afterSlot = registryMethod[
            registryMethod.IndexOf(
                "_runSlots.WaitAsync(0, cancellationToken)",
                StringComparison.Ordinal)..];
        AssertFirstOccurrencesInOrder(
            afterSlot,
            "if (IsInvalidated || !executionAllowed())",
            "lookup = GetCore(tenantId, sessionId)",
            "if (!lookup.Session.SupportsTopologyMutation)",
            "ValidateTopologyMutationScope(lookup.Session, admittedProfile)",
            "CreateOwnedRegistryProjection(admittedProfile)",
            "cancellationToken.ThrowIfCancellationRequested()",
            "lookup.Session.ApplyRegistryProjection(");
        var applyIndex = registryMethod.IndexOf(
            "lookup.Session.ApplyRegistryProjection(",
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "executionAllowed()",
            registryMethod[applyIndex..],
            StringComparison.Ordinal);
        Assert.DoesNotContain("ReadCurrent(", registryMethod, StringComparison.Ordinal);
        Assert.Contains(
            "entry.Result.ProfileId",
            registryMethod,
            StringComparison.Ordinal);
        Assert.Contains(
            "descriptor.ProfileId",
            registryMethod,
            StringComparison.Ordinal);
        Assert.Contains(
            "descriptor.TenantId",
            registryMethod,
            StringComparison.Ordinal);
        Assert.Contains(
            "descriptor.DomainTag",
            registryMethod,
            StringComparison.Ordinal);
        Assert.Contains(
            "descriptor.ContextId",
            registryMethod,
            StringComparison.Ordinal);
    }

    [Fact]
    public void TopologyRunHttpBoundary_IsStrictCapabilityOwnedAndDoesNotRereadOrFabricate()
    {
        var serverRoot = Path.Combine(
            FindRepoRoot(), "tools", "DynamicGraphCompositionLiveLocalServer");
        var contracts = NormalizeSource(File.ReadAllText(Path.Combine(
            serverRoot, "DynamicGraphCompositionHttpContracts.cs")));
        var registry = NormalizeSource(File.ReadAllText(Path.Combine(
            serverRoot, "DynamicGraphCompositionSessionRegistry.cs")));
        var program = NormalizeSource(File.ReadAllText(Path.Combine(serverRoot, "Program.cs")));
        var contractsRoot = CSharpSyntaxTree.ParseText(contracts).GetRoot();

        AssertSourceJsonContract(
            contractsRoot,
            "RunCompositionSessionHttpRequest",
            strict: true,
            "user_request");
        AssertSourceJsonContract(
            contractsRoot,
            "TopologyRunEvidenceHttpResponse",
            strict: false,
            "revision",
            "signal_id",
            "source_kind");
        AssertSourceJsonContract(
            contractsRoot,
            "TopologyAssistantRunHttpResponse",
            strict: false,
            "request_id",
            "session_id",
            "status",
            "topology",
            "result",
            "boundaries");

        const string topologyRoute =
            "POST /api/v1/dynamic-graph/sessions/{sessionId}/topology-runs";
        const string legacyRoute =
            "POST /api/v1/dynamic-graph/sessions/{sessionId}/runs";
        Assert.Contains(
            "app.MapPost(\"/api/v1/dynamic-graph/sessions/{sessionId}/topology-runs\"",
            program,
            StringComparison.Ordinal);
        Assert.True(
            program.IndexOf($"\"{topologyRoute}\"", StringComparison.Ordinal)
            < program.IndexOf($"\"{legacyRoute}\"", StringComparison.Ordinal));

        var handler = ExtractUniqueBoundedSourceBlock(
            program,
            "app.MapPost(\"/api/v1/dynamic-graph/sessions/{sessionId}/topology-runs\"",
            "app.MapPost(\"/api/v1/dynamic-graph/sessions/{sessionId}/runs\"");
        var handlerRoot = CSharpSyntaxTree.ParseText(handler).GetRoot();
        AssertFirstOccurrencesInOrder(
            handler,
            "if (!readiness.IsReady)",
            "if (!context.Request.HasJsonContentType())",
            "if (context.Request.ContentLength > options.MaxRequestBodyBytes)",
            "ValidateConfiguredTenant(context, options, requestId)",
            "Regex.IsMatch(sessionId, \"^dgs_[0-9a-f]{32}$\"",
            "DeserializeStrictAsync<RunCompositionSessionHttpRequest>",
            "string.IsNullOrWhiteSpace(body?.UserRequest)",
            "body.UserRequest.Length > DynamicGraphCompositionServerOptions.MaxInputLength",
            "registry.RunOnCurrentTopologyAsync(",
            "TopologySessionExecutionStatus.Expired",
            "TopologySessionExecutionStatus.Unknown",
            "TopologySessionExecutionStatus.Unavailable",
            "TopologySessionExecutionStatus.Busy",
            "TopologySessionExecutionStatus.ForbiddenAssembly",
            "HOST_TOPOLOGY_RUN_RESULT_REQUIRED",
            "SafeDownstreamCode(operation.ErrorCode)",
            "StatusForAssistantCode(code)",
            "TopologyAssistantRun(");
        _ = Assert.Single(
            Regex.Matches(
                    handler,
                    Regex.Escape("registry.RunOnCurrentTopologyAsync("),
                    RegexOptions.CultureInvariant)
                .Cast<Match>());
        Assert.Equal(
            2,
            Regex.Matches(
                    handler,
                    Regex.Escape("if (!readiness.IsReady)"),
                    RegexOptions.CultureInvariant)
                .Count);
        var registryCallIndex = handler.IndexOf(
            "registry.RunOnCurrentTopologyAsync(",
            StringComparison.Ordinal);
        var postExecutionReadinessIndex = handler.IndexOf(
            "if (!readiness.IsReady)",
            registryCallIndex,
            StringComparison.Ordinal);
        Assert.True(
            registryCallIndex >= 0 && postExecutionReadinessIndex > registryCallIndex,
            "The post-execution readiness gate must follow the topology registry call.");
        Assert.Contains("HOST_TOPOLOGY_RUN_UNAVAILABLE", handler, StringComparison.Ordinal);
        Assert.Contains("HOST_TOPOLOGY_RUN_EXCEPTION", handler, StringComparison.Ordinal);
        Assert.Contains("HOST_REQUEST_CANCELLED", handler, StringComparison.Ordinal);
        Assert.Contains("HOST_RUN_CONCURRENCY_EXHAUSTED", handler, StringComparison.Ordinal);

        AssertIfReturnsExactError(
            handlerRoot,
            "executed.Status == TopologySessionExecutionStatus.Expired",
            410,
            "HOST_SESSION_EXPIRED");
        AssertIfReturnsExactError(
            handlerRoot,
            "executed.Status == TopologySessionExecutionStatus.Unknown",
            404,
            "HOST_SESSION_NOT_FOUND");
        AssertIfReturnsExactError(
            handlerRoot,
            "executed.Status == TopologySessionExecutionStatus.Unavailable",
            422,
            "HOST_TOPOLOGY_RUN_UNAVAILABLE");
        AssertIfReturnsExactError(
            handlerRoot,
            "executed.Status == TopologySessionExecutionStatus.Busy",
            429,
            "HOST_RUN_CONCURRENCY_EXHAUSTED");
        AssertIfReturnsExactError(
            handlerRoot,
            "executed.Status == TopologySessionExecutionStatus.ForbiddenAssembly",
            503,
            "FORBIDDEN_TRAINING_ASSEMBLY_LOADED");
        AssertIfReturnsExactError(
            handlerRoot,
            "operation is null || (operation.IsSuccess && operation.Value is null)",
            500,
            "HOST_TOPOLOGY_RUN_RESULT_REQUIRED");

        var cancellationCatches = handlerRoot.DescendantNodes()
            .OfType<CatchClauseSyntax>()
            .Where(clause => clause.Declaration?.Type is IdentifierNameSyntax
            {
                Identifier.ValueText: "OperationCanceledException"
            })
            .ToArray();
        Assert.Equal(2, cancellationCatches.Length);
        var expectedCancellationRequested = SyntaxFactory.ParseExpression(
            "cancellationToken.IsCancellationRequested");
        var expectedNonFatalCancellation = SyntaxFactory.ParseExpression(
            "FatalExceptionPolicy.IsNonFatal(exception)");
        Assert.All(cancellationCatches, clause =>
        {
            var filter = Assert.IsType<CatchFilterClauseSyntax>(clause.Filter);
            var conjunction = Assert.IsType<BinaryExpressionSyntax>(
                filter.FilterExpression);
            Assert.True(conjunction.IsKind(SyntaxKind.LogicalAndExpression));
            Assert.True(
                HasEquivalentTokens(
                    conjunction.Left,
                    expectedCancellationRequested),
                "Cancellation filters must bind the requested-token check as the left operand.");
            Assert.True(
                HasEquivalentTokens(
                    conjunction.Right,
                    expectedNonFatalCancellation),
                "Cancellation filters must bind the fatal-policy check as the right operand.");
            _ = AssertReturnsExactError(
                clause.Block,
                408,
                "HOST_REQUEST_CANCELLED");
        });

        var nonFatalExceptionCatch = Assert.Single(
            handlerRoot.DescendantNodes().OfType<CatchClauseSyntax>(),
            clause => clause.Declaration?.Type is IdentifierNameSyntax
                      {
                          Identifier.ValueText: "Exception"
                      });
        var nonFatalFilter = Assert.IsType<CatchFilterClauseSyntax>(
            nonFatalExceptionCatch.Filter);
        var expectedNonFatalFilter = SyntaxFactory.ParseExpression(
            "IsNonFatalException(exception)");
        Assert.True(
            HasEquivalentTokens(
                nonFatalFilter.FilterExpression,
                expectedNonFatalFilter),
            "The general exception catch must use only the exact nonfatal policy filter.");
        _ = AssertReturnsExactError(
            nonFatalExceptionCatch.Block,
            500,
            "HOST_TOPOLOGY_RUN_EXCEPTION");

        var registryInvocation = Assert.Single(
            handlerRoot.DescendantNodes().OfType<InvocationExpressionSyntax>(),
            invocation => invocation.Expression is MemberAccessExpressionSyntax
            {
                Expression: IdentifierNameSyntax { Identifier.ValueText: "registry" },
                Name.Identifier.ValueText: "RunOnCurrentTopologyAsync"
            });
        var readinessCondition = SyntaxFactory.ParseExpression("!readiness.IsReady");
        var postExecutionReadiness = Assert.Single(
            handlerRoot.DescendantNodes().OfType<IfStatementSyntax>(),
            statement => statement.SpanStart > registryInvocation.SpanStart
                         && HasEquivalentTokens(statement.Condition, readinessCondition));
        var postReadinessError = AssertReturnsExactError(
            postExecutionReadiness.Statement,
            503,
            expectedCode: null);
        var expectedReadinessCode = SyntaxFactory.ParseExpression(
            "readiness.ReasonCode ?? \"HOST_NOT_READY\"");
        Assert.True(
            HasEquivalentTokens(
                postReadinessError.ArgumentList.Arguments[2].Expression,
                expectedReadinessCode),
            "The post-execution readiness branch must retain its safe reason-code fallback.");

        Assert.DoesNotContain("registry.Get(", handler, StringComparison.Ordinal);
        Assert.DoesNotContain(".ReadCurrent(", handler, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "IDynamicGraphLogicalOrchestratorTopologyRunSession",
            handler,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "new DynamicGraphLogicalOrchestratorTopologyRunResult",
            handler,
            StringComparison.Ordinal);
        Assert.DoesNotContain("AdaptiveLogicalOrchestrator", handler, StringComparison.Ordinal);

        var responseHelper = ExtractUniqueBoundedSourceBlock(
            program,
            "static IResult TopologyAssistantRun(",
            "static IResult SelectedTargetEvidenceAdmission(");
        AssertFirstOccurrencesInOrder(
            responseHelper,
            "result.TopologyRevision",
            "result.TopologySignalId",
            "result.TopologySourceKind.ToString()",
            "result.Run.ScenarioId",
            "result.Run.SelectedFlowId",
            "result.Run.OverallVerdict",
            "result.Run.FinalHumanResult",
            "result.Run.Metrics",
            "result.Run.EvidenceIds",
            "new CompositionBoundaryHttpResponse(Persistent: persistent)");
        Assert.DoesNotContain(
            "universal-nine-node-development-5.1",
            responseHelper,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "universal-nine-node-development-flow-5.1",
            responseHelper,
            StringComparison.Ordinal);

        var registryMethod = ExtractUniqueBoundedSourceBlock(
            registry,
            "internal async Task<TopologySessionExecutionResult> RunOnCurrentTopologyAsync(",
            "public async Task<SelectedTargetSessionExecutionResult> RunSelectedTargetAsync(");
        Assert.Contains("ExecuteSessionOperationAsync<", registryMethod, StringComparison.Ordinal);
        Assert.Contains("entry.SupportsTopologyRun", registryMethod, StringComparison.Ordinal);
        Assert.Contains("entry.ExecuteTopologyRunAsync(", registryMethod, StringComparison.Ordinal);
        Assert.Contains(
            "SessionOperationStatus.Unavailable => TopologySessionExecutionResult.Unavailable()",
            registryMethod,
            StringComparison.Ordinal);
        Assert.DoesNotContain("_runSession.RunAsync(", registryMethod, StringComparison.Ordinal);
    }

    [Fact(Timeout = 180000)]
    public async Task ChildProcess_TopologyRunReturnsPinnedTopologyAndActualChildRun()
    {
        var root = CreateTempRoot();
        var readyFile = Path.Combine(root, "ready.json");
        using var process = StartServer(
            root,
            ["--tenant-id", TenantId, "--ready-file", readyFile]);
        try
        {
            var baseUrl = await WaitForReadyAsync(process, readyFile);
            using var client = new HttpClient { BaseAddress = baseUrl };
            var profile = CreateProfile(TenantId);
            using var created = await PostProfileAsync(
                client,
                profile,
                "topology-run-key-0001",
                TenantId,
                "topology.create");
            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            var sessionId = await ReadSessionIdAsync(created);
            using (var createdDocument = JsonDocument.Parse(
                       await created.Content.ReadAsStringAsync()))
            {
                var createdRoot = createdDocument.RootElement;
                Assert.Equal(
                    [
                        "request_id",
                        "session_id",
                        "status",
                        "created_at_utc",
                        "expires_at_utc",
                        "result",
                        "boundaries"
                    ],
                    createdRoot.EnumerateObject().Select(property => property.Name));
                Assert.False(createdRoot.TryGetProperty("result_scope", out _));
            }
            await AssertCurrentTopologyAsync(
                client,
                sessionId,
                bootstrapRevision: 1,
                currentRevision: 1,
                profile.SignalId,
                "LogicalRegistryProjection",
                nodeCount: 3,
                edgeCount: 2,
                revisionAdvanced: false,
                persistent: false);

            using var response = await PostTopologyRunAsync(
                client,
                sessionId,
                "Check the technical-design to implementation transition.",
                TenantId,
                "topology.run");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var responseRoot = document.RootElement;
            Assert.Equal("topology.run", responseRoot.GetProperty("request_id").GetString());
            Assert.Equal(sessionId, responseRoot.GetProperty("session_id").GetString());
            Assert.Equal("completed", responseRoot.GetProperty("status").GetString());
            var topology = responseRoot.GetProperty("topology");
            Assert.Equal(1, topology.GetProperty("revision").GetInt64());
            Assert.Equal(profile.SignalId, topology.GetProperty("signal_id").GetString());
            Assert.Equal(
                "LogicalRegistryProjection",
                topology.GetProperty("source_kind").GetString());
            var result = responseRoot.GetProperty("result");
            Assert.Equal(
                "universal-nine-node-development-5.1",
                result.GetProperty("scenario_id").GetString());
            Assert.Equal(
                "universal-nine-node-development-flow-5.1",
                result.GetProperty("selected_flow_id").GetString());
            Assert.False(string.IsNullOrWhiteSpace(
                result.GetProperty("final_human_result").GetString()));
            var boundaries = responseRoot.GetProperty("boundaries");
            Assert.True(boundaries.GetProperty("loopback_only").GetBoolean());
            Assert.False(boundaries.GetProperty("tenant_binding_is_authentication").GetBoolean());
            Assert.False(boundaries.GetProperty("persistent").GetBoolean());
            Assert.True(boundaries.GetProperty("executes_assistant").GetBoolean());
            Assert.False(boundaries.GetProperty("applies_graph").GetBoolean());
            Assert.False(boundaries.GetProperty("performs_training").GetBoolean());
        }
        finally
        {
            Stop(process);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact(Timeout = 180000)]
    public async Task ChildProcess_EphemeralTopologyMutationAppliesCasNoOpBudgetAndValidation()
    {
        var root = CreateTempRoot();
        var readyFile = Path.Combine(root, "ready.json");
        using var process = StartServer(
            root,
            ["--tenant-id", TenantId, "--ready-file", readyFile]);
        try
        {
            var baseUrl = await WaitForReadyAsync(process, readyFile);
            using var client = new HttpClient { BaseAddress = baseUrl };
            var baseline = CreateProfile(TenantId);
            using var created = await PostProfileAsync(
                client,
                baseline,
                "topology-mutation-key-0001",
                TenantId,
                "topology.mutation.create");
            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            var sessionId = await ReadSessionIdAsync(created);
            using (var createdDocument = JsonDocument.Parse(
                       await created.Content.ReadAsStringAsync()))
            {
                var createdRoot = createdDocument.RootElement;
                Assert.Equal(
                    [
                        "request_id",
                        "session_id",
                        "status",
                        "created_at_utc",
                        "expires_at_utc",
                        "result",
                        "boundaries"
                    ],
                    createdRoot.EnumerateObject().Select(property => property.Name));
                Assert.False(createdRoot.TryGetProperty("result_scope", out _));
            }
            await AssertCurrentTopologyAsync(
                client,
                sessionId,
                bootstrapRevision: 1,
                currentRevision: 1,
                baseline.SignalId,
                "LogicalRegistryProjection",
                nodeCount: 3,
                edgeCount: 2,
                revisionAdvanced: false,
                persistent: false);

            var mutation = baseline with
            {
                SignalId = "live-local-signal-revision-2",
                SourceRef = "logical-registry:live-local-profile:revision-2",
                TraceRef = "trace:live-local-profile:revision-2",
                NodeDefinitions = [.. baseline.NodeDefinitions, CreateNode("4.2")],
                EdgeBindings =
                [
                    .. baseline.EdgeBindings,
                    CreateEdge(
                        "live-edge-41-42",
                        "transition-sub-phase-4.1-to-4.2",
                        "live-authority-41")
                ]
            };
            using var applied = await PostTopologyMutationAsync(
                client,
                sessionId,
                1,
                mutation,
                TenantId,
                "topology.mutation.applied");
            Assert.Equal(HttpStatusCode.OK, applied.StatusCode);
            using var appliedDocument = JsonDocument.Parse(
                await applied.Content.ReadAsStringAsync());
            var appliedRoot = appliedDocument.RootElement;
            Assert.Equal("applied", appliedRoot.GetProperty("status").GetString());
            Assert.Equal(1, appliedRoot.GetProperty("previous_revision").GetInt64());
            Assert.Equal(2, appliedRoot.GetProperty("current_revision").GetInt64());
            Assert.True(appliedRoot.GetProperty("revision_advanced").GetBoolean());
            Assert.Equal(
                mutation.SignalId,
                appliedRoot.GetProperty("attempt").GetProperty("signal_id").GetString());
            Assert.Equal(
                "LogicalRegistryProjection",
                appliedRoot.GetProperty("attempt").GetProperty("source_kind").GetString());
            Assert.True(
                appliedRoot.GetProperty("attempt").GetProperty("mutation_applied").GetBoolean());
            Assert.Equal(
                mutation.SignalId,
                appliedRoot.GetProperty("current").GetProperty("signal_id").GetString());
            Assert.Equal(
                "LogicalRegistryProjection",
                appliedRoot.GetProperty("current").GetProperty("source_kind").GetString());
            Assert.Equal(
                4,
                appliedRoot.GetProperty("current").GetProperty("node_count").GetInt32());
            Assert.Equal(
                3,
                appliedRoot.GetProperty("current").GetProperty("edge_count").GetInt32());
            Assert.False(
                appliedRoot.GetProperty("current").TryGetProperty("profile_id", out _));
            var appliedBudget = appliedRoot.GetProperty("budget");
            Assert.True(appliedBudget.GetProperty("within_budget").GetBoolean());
            Assert.Equal(4, appliedBudget.GetProperty("actual_nodes").GetInt32());
            Assert.Equal(3, appliedBudget.GetProperty("actual_edges").GetInt32());
            Assert.Empty(appliedBudget.GetProperty("exceeded_dimensions").EnumerateArray());
            var boundaries = appliedRoot.GetProperty("boundaries");
            Assert.False(boundaries.GetProperty("persistent").GetBoolean());
            Assert.False(boundaries.GetProperty("executes_assistant").GetBoolean());
            Assert.True(boundaries.GetProperty("applies_graph").GetBoolean());
            Assert.False(boundaries.GetProperty("performs_training").GetBoolean());
            Assert.DoesNotContain("source_ref", appliedRoot.EnumerateObject().Select(p => p.Name));
            Assert.DoesNotContain("trace_ref", appliedRoot.EnumerateObject().Select(p => p.Name));
            Assert.DoesNotContain("topology", appliedRoot.EnumerateObject().Select(p => p.Name));
            Assert.DoesNotContain("nodes", appliedRoot.EnumerateObject().Select(p => p.Name));
            Assert.DoesNotContain("edges", appliedRoot.EnumerateObject().Select(p => p.Name));

            using (var legacyRead = await GetSessionAsync(client, sessionId, TenantId))
            {
                Assert.Equal(HttpStatusCode.OK, legacyRead.StatusCode);
                using var legacyDocument = JsonDocument.Parse(
                    await legacyRead.Content.ReadAsStringAsync());
                Assert.Equal(
                    1,
                    legacyDocument.RootElement.GetProperty("result")
                        .GetProperty("revision").GetInt64());
            }
            await AssertCurrentTopologyAsync(
                client,
                sessionId,
                bootstrapRevision: 1,
                currentRevision: 2,
                mutation.SignalId,
                "LogicalRegistryProjection",
                nodeCount: 4,
                edgeCount: 3,
                revisionAdvanced: true,
                persistent: false);

            using var topologyRun = await PostTopologyRunAsync(
                client,
                sessionId,
                "Run the revision two topology.",
                TenantId,
                "topology.mutation.run");
            Assert.Equal(HttpStatusCode.OK, topologyRun.StatusCode);
            using var runDocument = JsonDocument.Parse(
                await topologyRun.Content.ReadAsStringAsync());
            Assert.Equal(
                2,
                runDocument.RootElement.GetProperty("topology")
                    .GetProperty("revision").GetInt64());
            Assert.Equal(
                mutation.SignalId,
                runDocument.RootElement.GetProperty("topology")
                    .GetProperty("signal_id").GetString());
            Assert.False(string.IsNullOrWhiteSpace(
                runDocument.RootElement.GetProperty("result")
                    .GetProperty("final_human_result").GetString()));
            await AssertCurrentTopologyAsync(
                client,
                sessionId,
                1,
                2,
                mutation.SignalId,
                "LogicalRegistryProjection",
                4,
                3,
                revisionAdvanced: true,
                persistent: false);

            using var unchanged = await PostTopologyMutationAsync(
                client,
                sessionId,
                2,
                mutation,
                TenantId,
                "topology.mutation.unchanged");
            Assert.Equal(HttpStatusCode.OK, unchanged.StatusCode);
            using var unchangedDocument = JsonDocument.Parse(
                await unchanged.Content.ReadAsStringAsync());
            Assert.Equal(
                "unchanged",
                unchangedDocument.RootElement.GetProperty("status").GetString());
            Assert.Equal(
                2,
                unchangedDocument.RootElement.GetProperty("current_revision").GetInt64());
            Assert.False(
                unchangedDocument.RootElement.GetProperty("revision_advanced").GetBoolean());
            Assert.False(
                unchangedDocument.RootElement.GetProperty("attempt")
                    .GetProperty("mutation_applied").GetBoolean());
            Assert.Equal(
                4,
                unchangedDocument.RootElement.GetProperty("current")
                    .GetProperty("node_count").GetInt32());
            Assert.Equal(
                3,
                unchangedDocument.RootElement.GetProperty("current")
                    .GetProperty("edge_count").GetInt32());
            await AssertCurrentTopologyAsync(
                client,
                sessionId,
                1,
                2,
                mutation.SignalId,
                "LogicalRegistryProjection",
                4,
                3,
                revisionAdvanced: true,
                persistent: false);

            using var stale = await PostTopologyMutationAsync(
                client,
                sessionId,
                1,
                mutation with { SignalId = "stale-signal" },
                TenantId,
                "topology.mutation.stale");
            await AssertErrorEnvelopeAsync(
                stale,
                HttpStatusCode.Conflict,
                "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_REVISION_CONFLICT",
                "The topology mutation conflicts with the current session.");
            await AssertCurrentTopologyAsync(
                client,
                sessionId,
                1,
                2,
                mutation.SignalId,
                "LogicalRegistryProjection",
                4,
                3,
                revisionAdvanced: true,
                persistent: false);

            var budgetRejectedProfile = mutation with
            {
                SignalId = "budget-rejected-signal",
                Budget = new TopologyBuildBudget(3, 3, 8, 8)
            };
            using var budgetRejected = await PostTopologyMutationAsync(
                client,
                sessionId,
                2,
                budgetRejectedProfile,
                TenantId,
                "topology.mutation.budget");
            Assert.Equal(HttpStatusCode.OK, budgetRejected.StatusCode);
            using var budgetDocument = JsonDocument.Parse(
                await budgetRejected.Content.ReadAsStringAsync());
            Assert.Equal(
                "budget_rejected",
                budgetDocument.RootElement.GetProperty("status").GetString());
            Assert.Equal(
                2,
                budgetDocument.RootElement.GetProperty("current_revision").GetInt64());
            Assert.False(
                budgetDocument.RootElement.GetProperty("revision_advanced").GetBoolean());
            Assert.False(
                budgetDocument.RootElement.GetProperty("attempt")
                    .GetProperty("mutation_applied").GetBoolean());
            Assert.False(
                budgetDocument.RootElement.GetProperty("budget")
                    .GetProperty("within_budget").GetBoolean());
            Assert.Contains(
                "nodes",
                budgetDocument.RootElement.GetProperty("budget")
                    .GetProperty("exceeded_dimensions")
                    .EnumerateArray()
                    .Select(item => item.GetString()));
            Assert.Equal(
                mutation.SignalId,
                budgetDocument.RootElement.GetProperty("current")
                    .GetProperty("signal_id").GetString());
            Assert.Equal(
                4,
                budgetDocument.RootElement.GetProperty("current")
                    .GetProperty("node_count").GetInt32());
            Assert.Equal(
                3,
                budgetDocument.RootElement.GetProperty("current")
                    .GetProperty("edge_count").GetInt32());
            await AssertCurrentTopologyAsync(
                client,
                sessionId,
                1,
                2,
                mutation.SignalId,
                "LogicalRegistryProjection",
                4,
                3,
                revisionAdvanced: true,
                persistent: false);

            var invalidFanIn = mutation with
            {
                SignalId = "invalid-fan-in",
                EdgeBindings =
                [
                    .. baseline.EdgeBindings,
                    CreateEdge(
                        "invalid-fan-in-edge",
                        "transition-sub-phase-4.1-to-4.2",
                        "invalid-fan-in-authority") with
                    {
                        Kind = GraphEdgeKind.FanIn
                    }
                ]
            };
            using var fanIn = await PostTopologyMutationAsync(
                client,
                sessionId,
                2,
                invalidFanIn,
                TenantId,
                "topology.mutation.fan-in");
            await AssertErrorEnvelopeAsync(
                fanIn,
                HttpStatusCode.UnprocessableEntity,
                "DYNAMIC_GRAPH_FAN_IN_CARDINALITY_INVALID",
                "The topology mutation profile was rejected.");
            await AssertCurrentTopologyAsync(
                client,
                sessionId,
                1,
                2,
                mutation.SignalId,
                "LogicalRegistryProjection",
                4,
                3,
                revisionAdvanced: true,
                persistent: false);

            var invalidManyToMany = invalidFanIn with
            {
                SignalId = "invalid-many-to-many",
                EdgeBindings =
                [
                    .. baseline.EdgeBindings,
                    invalidFanIn.EdgeBindings[^1] with
                    {
                        EdgeId = "invalid-many-to-many-edge",
                        Kind = GraphEdgeKind.ManyToMany
                    }
                ]
            };
            using var manyToMany = await PostTopologyMutationAsync(
                client,
                sessionId,
                2,
                invalidManyToMany,
                TenantId,
                "topology.mutation.many-to-many");
            await AssertErrorEnvelopeAsync(
                manyToMany,
                HttpStatusCode.UnprocessableEntity,
                "DYNAMIC_GRAPH_MANY_TO_MANY_CARDINALITY_INVALID",
                "The topology mutation profile was rejected.");
            await AssertCurrentTopologyAsync(
                client,
                sessionId,
                1,
                2,
                mutation.SignalId,
                "LogicalRegistryProjection",
                4,
                3,
                revisionAdvanced: true,
                persistent: false);

            using var retainedTopologyRun = await PostTopologyRunAsync(
                client,
                sessionId,
                "Confirm invalid mutations retained revision two.",
                TenantId,
                "topology.mutation.retained");
            Assert.Equal(HttpStatusCode.OK, retainedTopologyRun.StatusCode);
            using var retainedTopologyDocument = JsonDocument.Parse(
                await retainedTopologyRun.Content.ReadAsStringAsync());
            var retainedTopology = retainedTopologyDocument.RootElement.GetProperty("topology");
            Assert.Equal(2, retainedTopology.GetProperty("revision").GetInt64());
            Assert.Equal(
                mutation.SignalId,
                retainedTopology.GetProperty("signal_id").GetString());

        }
        finally
        {
            Stop(process);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact(Timeout = 180000)]
    public async Task ChildProcess_PersistentTopologyMutationRejectsBeforeBodyParsingAndRestoresRevisionOne()
    {
        var root = CreateTempRoot();
        var persistentRoot = CreatePersistentTempRoot("xiigen-mutation-persistence-test");
        var stateDirectory = Path.Combine(persistentRoot, "state");
        var firstReady = Path.Combine(root, "first-ready.json");
        string sessionId;
        using (var first = StartServer(
                   root,
                   [
                       "--tenant-id", TenantId,
                       "--ready-file", firstReady,
                       "--state-directory", stateDirectory
                   ]))
        {
            try
            {
                var baseUrl = await WaitForReadyAsync(
                    first,
                    firstReady,
                    expectedPersistent: true);
                using var client = new HttpClient { BaseAddress = baseUrl };
                using var created = await PostProfileAsync(
                    client,
                    CreateProfile(TenantId),
                    "persistent-mutation-key-0001",
                    TenantId,
                    "persistent.mutation.create");
                Assert.Equal(HttpStatusCode.Created, created.StatusCode);
                sessionId = await ReadSessionIdAsync(created);
                await AssertCurrentTopologyAsync(
                    client,
                    sessionId,
                    1,
                    1,
                    CreateProfile(TenantId).SignalId,
                    "LogicalRegistryProjection",
                    3,
                    2,
                    revisionAdvanced: false,
                    persistent: true);

                using var malformedRequest = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"/api/v1/dynamic-graph/sessions/{sessionId}/topology-mutations")
                {
                    Content = new StringContent("{", Encoding.UTF8, "application/json")
                };
                malformedRequest.Headers.Add("X-XIIGen-Tenant-Id", TenantId);
                malformedRequest.Headers.Add("X-Request-Id", "persistent.mutation.reject");
                using var rejected = await client.SendAsync(malformedRequest);
                await AssertErrorEnvelopeAsync(
                    rejected,
                    HttpStatusCode.Conflict,
                    "HOST_TOPOLOGY_MUTATION_REQUIRES_EPHEMERAL_MODE",
                    "Topology mutation requires ephemeral session mode.");
            }
            finally
            {
                Stop(first);
            }
        }

        var secondReady = Path.Combine(root, "second-ready.json");
        using var second = StartServer(
            root,
            [
                "--tenant-id", TenantId,
                "--ready-file", secondReady,
                "--state-directory", stateDirectory
            ]);
        try
        {
            var baseUrl = await WaitForReadyAsync(
                second,
                secondReady,
                expectedPersistent: true);
            using var client = new HttpClient { BaseAddress = baseUrl };
            using var restored = await GetSessionAsync(client, sessionId, TenantId);
            Assert.Equal(HttpStatusCode.OK, restored.StatusCode);
            using var document = JsonDocument.Parse(
                await restored.Content.ReadAsStringAsync());
            Assert.Equal(
                1,
                document.RootElement.GetProperty("result")
                    .GetProperty("revision").GetInt64());
            await AssertCurrentTopologyAsync(
                client,
                sessionId,
                1,
                1,
                CreateProfile(TenantId).SignalId,
                "LogicalRegistryProjection",
                3,
                2,
                revisionAdvanced: false,
                persistent: true);
        }
        finally
        {
            Stop(second);
            Directory.Delete(root, recursive: true);
            if (Directory.Exists(persistentRoot))
                Directory.Delete(persistentRoot, recursive: true);
        }
    }

    [Fact(Timeout = 180000)]
    public async Task ChildProcess_TopologyRunEnforcesStrictBodyTenantLimitUnknownAndExpiry()
    {
        var root = CreateTempRoot();
        var readyFile = Path.Combine(root, "ready.json");
        using var process = StartServer(
            root,
            [
                "--tenant-id", TenantId,
                "--ready-file", readyFile,
                "--max-request-body-bytes", "8192",
                "--session-ttl-seconds", "1"
            ]);
        try
        {
            var baseUrl = await WaitForReadyAsync(process, readyFile);
            using var client = new HttpClient { BaseAddress = baseUrl };
            using var created = await PostProfileAsync(
                client,
                CreateProfile(TenantId),
                "topology-guard-key-0001",
                TenantId);
            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            var sessionId = await ReadSessionIdAsync(created);

            using var unknownMemberRequest = new HttpRequestMessage(
                HttpMethod.Post,
                $"/api/v1/dynamic-graph/sessions/{sessionId}/topology-runs")
            {
                Content = new StringContent(
                    "{\"user_request\":\"valid\",\"unexpected\":true}",
                    Encoding.UTF8,
                    "application/json")
            };
            unknownMemberRequest.Headers.Add("X-XIIGen-Tenant-Id", TenantId);
            using var unknownMember = await client.SendAsync(unknownMemberRequest);
            await AssertErrorEnvelopeAsync(
                unknownMember,
                HttpStatusCode.BadRequest,
                "HOST_TOPOLOGY_RUN_REQUEST_BODY_INVALID",
                "A valid topology run request body is required.");

            using var wrongTenant = await PostTopologyRunAsync(
                client,
                sessionId,
                "valid",
                "tenant-b",
                "topology.wrong-tenant");
            await AssertErrorEnvelopeAsync(
                wrongTenant,
                HttpStatusCode.Forbidden,
                "HOST_TENANT_BINDING_MISMATCH",
                "The request does not match the configured tenant binding.");

            using var unknown = await PostTopologyRunAsync(
                client,
                "dgs_" + new string('0', 32),
                "valid",
                TenantId,
                "topology.unknown");
            await AssertErrorEnvelopeAsync(
                unknown,
                HttpStatusCode.NotFound,
                "HOST_SESSION_NOT_FOUND",
                "The session was not found.");

            using var oversized = await PostTopologyRunAsync(
                client,
                sessionId,
                new string('x', 9000),
                TenantId,
                "topology.oversized");
            await AssertErrorCodeAsync(oversized, "HOST_REQUEST_BODY_LIMIT_EXCEEDED");
            Assert.Equal(HttpStatusCode.RequestEntityTooLarge, oversized.StatusCode);

            await Task.Delay(TimeSpan.FromMilliseconds(1100));
            using var expired = await PostTopologyRunAsync(
                client,
                sessionId,
                "valid",
                TenantId,
                "topology.expired");
            await AssertErrorEnvelopeAsync(
                expired,
                HttpStatusCode.Gone,
                "HOST_SESSION_EXPIRED",
                "The session has expired.");
        }
        finally
        {
            Stop(process);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ChildProcess_SelectedTargetEvidenceHandleExecutesOnceAndStaysOpaque()
    {
        var root = CreateTempRoot();
        var readyFile = Path.Combine(root, "ready.json");
        using var process = StartServer(
            root,
            ["--tenant-id", TenantId, "--ready-file", readyFile]);
        try
        {
            var baseUrl = await WaitForReadyAsync(process, readyFile);
            using var client = new HttpClient { BaseAddress = baseUrl };
            using var readyDocument = JsonDocument.Parse(await File.ReadAllTextAsync(readyFile));
            Assert.Equal(
                [
                    "GET /api/v1/dynamic-graph/health",
                    "GET /api/v1/dynamic-graph/readiness",
                    "POST /api/v1/dynamic-graph/l2-untrained-honesty-runs",
                    "POST /api/v1/dynamic-graph/sessions",
                    "POST /api/v1/dynamic-graph/catalog-sessions",
                    "GET /api/v1/dynamic-graph/sessions/{sessionId}",
                    "GET /api/v1/dynamic-graph/sessions/{sessionId}/topology",
                    "POST /api/v1/dynamic-graph/sessions/{sessionId}/selected-target-evidence",
                    "POST /api/v1/dynamic-graph/sessions/{sessionId}/selected-target-runs",
                    "POST /api/v1/dynamic-graph/sessions/{sessionId}/topology-mutations",
                    "POST /api/v1/dynamic-graph/sessions/{sessionId}/topology-runs",
                    "POST /api/v1/dynamic-graph/sessions/{sessionId}/runs"
                ],
                readyDocument.RootElement.GetProperty("routes")
                    .EnumerateArray()
                    .Select(item => item.GetString()!)
                    .ToArray());

            using var created = await PostProfileAsync(
                client,
                CreateProfile(TenantId),
                "selected-target-key-0001",
                TenantId,
                "selected-target.create");
            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            using var createdDocument = JsonDocument.Parse(
                await created.Content.ReadAsStringAsync());
            var sessionId = createdDocument.RootElement.GetProperty("session_id").GetString();
            Assert.NotNull(sessionId);
            Assert.Matches("^dgs_[0-9a-f]{32}$", sessionId);

            using var genericRun = await PostAssistantRunAsync(
                client,
                sessionId!,
                "Preserve the existing generic assistant route.",
                TenantId,
                "selected-target.generic");
            Assert.Equal(HttpStatusCode.OK, genericRun.StatusCode);

            using var malformedHandle = await PostSelectedTargetRunAsync(
                client,
                sessionId,
                "not-a-handle",
                "selected-run-malformed",
                SelectedTargetScenarioId,
                "This must not execute.",
                "selected-target.malformed");
            Assert.Equal(HttpStatusCode.NotFound, malformedHandle.StatusCode);
            await AssertErrorCodeAsync(
                malformedHandle,
                "HOST_SELECTED_TARGET_EVIDENCE_NOT_FOUND");

            var wrongCaseRequest = new HttpRequestMessage(
                HttpMethod.Post,
                $"/api/v1/dynamic-graph/sessions/{sessionId}/selected-target-evidence")
            {
                Content = JsonContent.Create(new Dictionary<string, object?>
                {
                    ["Target"] = null,
                    ["authority_result"] = null,
                    ["novelty_request"] = null
                })
            };
            wrongCaseRequest.Headers.Add("X-XIIGen-Tenant-Id", TenantId);
            wrongCaseRequest.Headers.Add("X-Request-Id", "selected-target.case");
            using (wrongCaseRequest)
            using (var wrongCase = await client.SendAsync(wrongCaseRequest))
            {
                Assert.Equal(HttpStatusCode.BadRequest, wrongCase.StatusCode);
                await AssertErrorCodeAsync(
                    wrongCase,
                    "HOST_SELECTED_TARGET_EVIDENCE_BODY_INVALID");
            }

            var evidence = CreateSelectedTargetAdmissionPayload();
            using var widened = await PostSelectedTargetEvidenceAsync(
                client,
                sessionId,
                evidence.Target,
                evidence.AuthorityResult,
                evidence.NoveltyRequest,
                "selected-target.widened",
                includeAssociation: true);
            Assert.Equal(HttpStatusCode.BadRequest, widened.StatusCode);
            await AssertErrorCodeAsync(
                widened,
                "HOST_SELECTED_TARGET_EVIDENCE_BODY_INVALID");

            using var admitted = await PostSelectedTargetEvidenceAsync(
                client,
                sessionId,
                evidence.Target,
                evidence.AuthorityResult,
                evidence.NoveltyRequest,
                "selected-target.admit");
            var admittedText = await admitted.Content.ReadAsStringAsync();
            Assert.True(
                admitted.StatusCode == HttpStatusCode.Created,
                $"Expected selected-target admission 201, got {(int)admitted.StatusCode}: {admittedText}");
            Assert.DoesNotContain("association", admittedText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("authority_result", admittedText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("novelty_request", admittedText, StringComparison.OrdinalIgnoreCase);
            using var admittedDocument = JsonDocument.Parse(admittedText);
            var admittedRoot = admittedDocument.RootElement;
            Assert.Equal("selected-target.admit", admittedRoot.GetProperty("request_id").GetString());
            Assert.Equal(sessionId, admittedRoot.GetProperty("session_id").GetString());
            Assert.Equal("admitted", admittedRoot.GetProperty("status").GetString());
            var handle = admittedRoot.GetProperty("evidence_handle").GetString();
            Assert.NotNull(handle);
            Assert.Matches("^dgeh_[0-9a-f]{32}$", handle);
            Assert.True(
                admittedRoot.GetProperty("expires_at_utc").GetDateTimeOffset()
                > DateTimeOffset.UtcNow);

            using var executed = await PostSelectedTargetRunAsync(
                client,
                sessionId,
                handle!,
                "selected-run-0001",
                SelectedTargetScenarioId,
                SelectedTargetUserRequest,
                "selected-target.execute");
            var executedText = await executed.Content.ReadAsStringAsync();
            Assert.True(
                executed.StatusCode == HttpStatusCode.OK,
                $"Expected admitted selected-target execution 200, got {(int)executed.StatusCode}: {executedText}");
            Assert.DoesNotContain("association", executedText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("authority", executedText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("novelty", executedText, StringComparison.OrdinalIgnoreCase);
            using var executedDocument = JsonDocument.Parse(executedText);
            var executedRoot = executedDocument.RootElement;
            Assert.Equal(
                [
                    "request_id",
                    "session_id",
                    "status",
                    "target_id",
                    "execution_id",
                    "evidence_ids",
                    "direct_target_invocation_count",
                    "run_speed",
                    "boundaries"
                ],
                executedRoot.EnumerateObject().Select(property => property.Name));
            Assert.Equal("selected-target.execute", executedRoot.GetProperty("request_id").GetString());
            Assert.Equal(sessionId, executedRoot.GetProperty("session_id").GetString());
            Assert.Equal("completed", executedRoot.GetProperty("status").GetString());
            Assert.Equal(
                SelectedTargetFlowId,
                executedRoot.GetProperty("target_id").GetString());
            Assert.Equal(
                "selected-run-0001",
                executedRoot.GetProperty("execution_id").GetString());
            var evidenceIds = executedRoot.GetProperty("evidence_ids")
                .EnumerateArray()
                .Select(item => item.GetString())
                .ToArray();
            Assert.NotEmpty(evidenceIds);
            Assert.DoesNotContain(evidenceIds, id =>
                string.IsNullOrWhiteSpace(id)
                || !StringComparer.Ordinal.Equals(id, id.Trim())
                || id.Any(char.IsControl));
            Assert.Equal(
                evidenceIds.Length,
                evidenceIds.Distinct(StringComparer.Ordinal).Count());
            Assert.Equal(
                1,
                executedRoot.GetProperty("direct_target_invocation_count").GetInt32());
            AssertSelectedTargetRunSpeed(executedRoot);
            Assert.False(executedRoot.TryGetProperty("association", out _));

            using var replay = await PostSelectedTargetRunAsync(
                client,
                sessionId,
                handle,
                "selected-run-0002",
                SelectedTargetScenarioId,
                "A consumed handle must not execute again.",
                "selected-target.replay");
            Assert.Equal(HttpStatusCode.NotFound, replay.StatusCode);
            await AssertErrorCodeAsync(
                replay,
                "HOST_SELECTED_TARGET_EVIDENCE_NOT_FOUND");

            using var unknown = await PostSelectedTargetRunAsync(
                client,
                $"dgs_{new string('a', 32)}",
                $"dgeh_{new string('b', 32)}",
                "selected-run-unknown",
                SelectedTargetScenarioId,
                "An unknown session and handle must share the safe not-found envelope.",
                "selected-target.unknown");
            Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
            await AssertErrorCodeAsync(
                unknown,
                "HOST_SELECTED_TARGET_EVIDENCE_NOT_FOUND");
        }
        finally
        {
            Stop(process);
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }

        var expiryRoot = CreateTempRoot();
        var expiryReadyFile = Path.Combine(expiryRoot, "ready.json");
        using var expiryProcess = StartServer(
            expiryRoot,
            [
                "--tenant-id", TenantId,
                "--ready-file", expiryReadyFile,
                "--session-ttl-seconds", "1"
            ]);
        try
        {
            var baseUrl = await WaitForReadyAsync(expiryProcess, expiryReadyFile);
            using var client = new HttpClient { BaseAddress = baseUrl };
            using var created = await PostProfileAsync(
                client,
                CreateProfile(TenantId),
                "selected-target-expiry-0001",
                TenantId,
                "selected-target.expiry.create");
            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            using var createdDocument = JsonDocument.Parse(
                await created.Content.ReadAsStringAsync());
            var sessionId = createdDocument.RootElement.GetProperty("session_id").GetString();
            await Task.Delay(TimeSpan.FromMilliseconds(1300));

            using var expired = await PostSelectedTargetRunAsync(
                client,
                sessionId!,
                $"dgeh_{new string('c', 32)}",
                "selected-run-expired",
                SelectedTargetScenarioId,
                "Expired session mapping.",
                "selected-target.expired");
            Assert.Equal(HttpStatusCode.Gone, expired.StatusCode);
            await AssertErrorCodeAsync(expired, "HOST_SESSION_EXPIRED");
        }
        finally
        {
            Stop(expiryProcess);
            if (Directory.Exists(expiryRoot)) Directory.Delete(expiryRoot, recursive: true);
        }
    }

    [Fact(Timeout = 180000)]
    public async Task ChildProcess_PersistentSessionKeepsSelectedTargetHandlesEphemeralAcrossRestart()
    {
        var root = CreateTempRoot();
        var persistentRoot = CreatePersistentTempRoot("xiigen-selected-target-persistence-test");
        var stateDirectory = Path.Combine(persistentRoot, "state");
        var firstReadyFile = Path.Combine(root, "first-ready.json");
        var secondReadyFile = Path.Combine(root, "second-ready.json");
        string sessionId;
        string staleHandle;

        try
        {
            using (var firstProcess = StartServer(
                       root,
                       [
                           "--tenant-id", TenantId,
                           "--ready-file", firstReadyFile,
                           "--state-directory", stateDirectory
                       ]))
            {
                try
                {
                    var firstBaseUrl = await WaitForReadyAsync(
                        firstProcess,
                        firstReadyFile,
                        expectedPersistent: true);
                    using var firstClient = new HttpClient { BaseAddress = firstBaseUrl };
                    using var created = await PostProfileAsync(
                        firstClient,
                        CreateProfile(TenantId),
                        "selected-target-persistent-0001",
                        TenantId,
                        "selected-target.persist.create");
                    Assert.Equal(HttpStatusCode.Created, created.StatusCode);
                    using var createdDocument = JsonDocument.Parse(
                        await created.Content.ReadAsStringAsync());
                    var createdRoot = createdDocument.RootElement;
                    sessionId = createdRoot.GetProperty("session_id").GetString()!;
                    Assert.Matches("^dgs_[0-9a-f]{32}$", sessionId);
                    Assert.True(
                        createdRoot.GetProperty("boundaries")
                            .GetProperty("persistent")
                            .GetBoolean());

                    var evidence = CreateSelectedTargetAdmissionPayload();
                    using var admitted = await PostSelectedTargetEvidenceAsync(
                        firstClient,
                        sessionId,
                        evidence.Target,
                        evidence.AuthorityResult,
                        evidence.NoveltyRequest,
                        "selected-target.persist.admit.before-restart");
                    var admittedText = await admitted.Content.ReadAsStringAsync();
                    Assert.True(
                        admitted.StatusCode == HttpStatusCode.Created,
                        $"Expected selected-target admission 201, got {(int)admitted.StatusCode}: {admittedText}");
                    Assert.DoesNotContain("association", admittedText, StringComparison.OrdinalIgnoreCase);
                    Assert.DoesNotContain("authority", admittedText, StringComparison.OrdinalIgnoreCase);
                    Assert.DoesNotContain("novelty", admittedText, StringComparison.OrdinalIgnoreCase);
                    using var admittedDocument = JsonDocument.Parse(admittedText);
                    var admittedRoot = admittedDocument.RootElement;
                    Assert.Equal(
                        [
                            "request_id",
                            "session_id",
                            "status",
                            "evidence_handle",
                            "expires_at_utc",
                            "boundaries"
                        ],
                        admittedRoot.EnumerateObject().Select(property => property.Name));
                    Assert.Equal(
                        "selected-target.persist.admit.before-restart",
                        admittedRoot.GetProperty("request_id").GetString());
                    Assert.Equal(sessionId, admittedRoot.GetProperty("session_id").GetString());
                    Assert.Equal("admitted", admittedRoot.GetProperty("status").GetString());
                    staleHandle = admittedRoot.GetProperty("evidence_handle").GetString()!;
                    Assert.Matches("^dgeh_[0-9a-f]{32}$", staleHandle);
                    Assert.True(
                        admittedRoot.GetProperty("expires_at_utc").GetDateTimeOffset()
                        > DateTimeOffset.UtcNow);
                    Assert.False(
                        admittedRoot.GetProperty("boundaries")
                            .GetProperty("persistent")
                            .GetBoolean());
                }
                finally
                {
                    Stop(firstProcess);
                }
            }

            using var secondProcess = StartServer(
                root,
                [
                    "--tenant-id", TenantId,
                    "--ready-file", secondReadyFile,
                    "--state-directory", stateDirectory
                ]);
            try
            {
                var secondBaseUrl = await WaitForReadyAsync(
                    secondProcess,
                    secondReadyFile,
                    expectedPersistent: true);
                using var secondClient = new HttpClient { BaseAddress = secondBaseUrl };

                using var restored = await GetSessionAsync(secondClient, sessionId, TenantId);
                Assert.Equal(HttpStatusCode.OK, restored.StatusCode);
                using var restoredDocument = JsonDocument.Parse(
                    await restored.Content.ReadAsStringAsync());
                var restoredRoot = restoredDocument.RootElement;
                Assert.Equal(sessionId, restoredRoot.GetProperty("session_id").GetString());
                Assert.True(
                    restoredRoot.GetProperty("boundaries")
                        .GetProperty("persistent")
                        .GetBoolean());

                using var stale = await PostSelectedTargetRunAsync(
                    secondClient,
                    sessionId,
                    staleHandle,
                    "selected-run-stale-after-restart",
                    SelectedTargetScenarioId,
                    "An evidence handle from the prior process must not execute.",
                    "selected-target.persist.stale");
                await AssertErrorEnvelopeAsync(
                    stale,
                    HttpStatusCode.NotFound,
                    "HOST_SELECTED_TARGET_EVIDENCE_NOT_FOUND",
                    "The selected-target evidence was not found.");

                var freshEvidence = CreateSelectedTargetAdmissionPayload();
                using var freshAdmission = await PostSelectedTargetEvidenceAsync(
                    secondClient,
                    sessionId,
                    freshEvidence.Target,
                    freshEvidence.AuthorityResult,
                    freshEvidence.NoveltyRequest,
                    "selected-target.persist.admit.after-restart");
                var freshAdmissionText = await freshAdmission.Content.ReadAsStringAsync();
                Assert.True(
                    freshAdmission.StatusCode == HttpStatusCode.Created,
                    $"Expected selected-target admission 201, got {(int)freshAdmission.StatusCode}: {freshAdmissionText}");
                Assert.DoesNotContain("association", freshAdmissionText, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("authority", freshAdmissionText, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("novelty", freshAdmissionText, StringComparison.OrdinalIgnoreCase);
                using var freshAdmissionDocument = JsonDocument.Parse(freshAdmissionText);
                var freshAdmissionRoot = freshAdmissionDocument.RootElement;
                var freshHandle = freshAdmissionRoot.GetProperty("evidence_handle").GetString()!;
                Assert.Matches("^dgeh_[0-9a-f]{32}$", freshHandle);
                Assert.NotEqual(staleHandle, freshHandle);
                Assert.False(
                    freshAdmissionRoot.GetProperty("boundaries")
                        .GetProperty("persistent")
                        .GetBoolean());

                using var executed = await PostSelectedTargetRunAsync(
                    secondClient,
                    sessionId,
                    freshHandle,
                    "selected-run-after-restart",
                    SelectedTargetScenarioId,
                    SelectedTargetUserRequest,
                    "selected-target.persist.execute");
                var executedText = await executed.Content.ReadAsStringAsync();
                Assert.True(
                    executed.StatusCode == HttpStatusCode.OK,
                    $"Expected selected-target execution 200, got {(int)executed.StatusCode}: {executedText}");
                Assert.DoesNotContain("association", executedText, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("authority", executedText, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("novelty", executedText, StringComparison.OrdinalIgnoreCase);
                using var executedDocument = JsonDocument.Parse(executedText);
                var executedRoot = executedDocument.RootElement;
                Assert.Equal(
                    [
                        "request_id",
                        "session_id",
                        "status",
                        "target_id",
                        "execution_id",
                        "evidence_ids",
                        "direct_target_invocation_count",
                        "run_speed",
                        "boundaries"
                    ],
                    executedRoot.EnumerateObject().Select(property => property.Name));
                Assert.Equal(
                    "selected-target.persist.execute",
                    executedRoot.GetProperty("request_id").GetString());
                Assert.Equal(sessionId, executedRoot.GetProperty("session_id").GetString());
                Assert.Equal("completed", executedRoot.GetProperty("status").GetString());
                Assert.Equal(
                    SelectedTargetFlowId,
                    executedRoot.GetProperty("target_id").GetString());
                Assert.Equal(
                    "selected-run-after-restart",
                    executedRoot.GetProperty("execution_id").GetString());
                var evidenceIds = executedRoot.GetProperty("evidence_ids")
                    .EnumerateArray()
                    .Select(item => item.GetString())
                    .ToArray();
                Assert.NotEmpty(evidenceIds);
                Assert.DoesNotContain(evidenceIds, id =>
                    string.IsNullOrWhiteSpace(id)
                    || !StringComparer.Ordinal.Equals(id, id.Trim())
                    || id.Any(char.IsControl));
                Assert.Equal(
                    evidenceIds.Length,
                    evidenceIds.Distinct(StringComparer.Ordinal).Count());
                Assert.Equal(
                    1,
                    executedRoot.GetProperty("direct_target_invocation_count").GetInt32());
                AssertSelectedTargetRunSpeed(executedRoot);
                Assert.False(
                    executedRoot.GetProperty("boundaries")
                        .GetProperty("persistent")
                        .GetBoolean());

                using var replay = await PostSelectedTargetRunAsync(
                    secondClient,
                    sessionId,
                    freshHandle,
                    "selected-run-after-restart-replay",
                    SelectedTargetScenarioId,
                    "A consumed handle must remain unavailable.",
                    "selected-target.persist.replay");
                await AssertErrorEnvelopeAsync(
                    replay,
                    HttpStatusCode.NotFound,
                    "HOST_SELECTED_TARGET_EVIDENCE_NOT_FOUND",
                    "The selected-target evidence was not found.");
            }
            finally
            {
                Stop(secondProcess);
            }
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
            if (Directory.Exists(persistentRoot))
                Directory.Delete(persistentRoot, recursive: true);
        }
    }

    private static byte[] Catalog(params DynamicGraphLogicalOrchestratorBootstrapProfile[] profiles)
        => JsonSerializer.SerializeToUtf8Bytes(profiles, CatalogJsonOptions);

    private static void AssertSourceJsonContract(
        SyntaxNode contractsRoot,
        string recordName,
        bool strict,
        params string[] expectedPropertyNames)
    {
        var declaration = Assert.Single(
            contractsRoot.DescendantNodes().OfType<RecordDeclarationSyntax>(),
            candidate => candidate.Identifier.ValueText == recordName);
        var handlingAttributes = declaration.AttributeLists
            .SelectMany(list => list.Attributes)
            .Where(attribute =>
                attribute.Name.ToString().Contains(
                    "JsonUnmappedMemberHandling",
                    StringComparison.Ordinal))
            .ToArray();
        if (strict)
        {
            var handling = Assert.Single(handlingAttributes);
            Assert.Contains(
                "JsonUnmappedMemberHandling.Disallow",
                handling.ArgumentList?.ToString(),
                StringComparison.Ordinal);
        }
        else
        {
            Assert.Empty(handlingAttributes);
        }

        var parameterList = Assert.IsType<ParameterListSyntax>(declaration.ParameterList);
        Assert.Equal(
            expectedPropertyNames,
            parameterList.Parameters.Select(parameter =>
            {
                var attribute = Assert.Single(
                    parameter.AttributeLists.SelectMany(list => list.Attributes),
                    candidate => candidate.Name.ToString().Contains(
                        "JsonPropertyName",
                        StringComparison.Ordinal));
                var argument = Assert.Single(attribute.ArgumentList!.Arguments);
                return Assert.IsType<LiteralExpressionSyntax>(argument.Expression)
                    .Token.ValueText;
            }));
    }

    private static void AssertFirstOccurrencesInOrder(
        string sourceBlock,
        params string[] tokens)
    {
        var previousIndex = -1;
        string? previousToken = null;
        foreach (var token in tokens)
        {
            var index = sourceBlock.IndexOf(token, StringComparison.Ordinal);
            Assert.True(
                index >= 0,
                $"Binding token was not found: {token}\nBlock:\n{sourceBlock}");
            Assert.True(
                index > previousIndex,
                $"Binding token is out of order: {token}\nPrevious token: {previousToken}\nBlock:\n{sourceBlock}");
            previousIndex = index;
            previousToken = token;
        }
    }

    private static void AssertSelectedTargetRunSpeed(JsonElement responseRoot)
    {
        var runSpeed = responseRoot.GetProperty("run_speed");
        Assert.Equal(
            [
                "evaluated_row_count",
                "inference_elapsed_ms",
                "throughput_rows_per_sec",
                "routing_elapsed_ms",
                "candidate_sample_count",
                "batch_sample_count",
                "per_candidate_elapsed_ms",
                "per_batch_elapsed_ms"
            ],
            runSpeed.EnumerateObject().Select(property => property.Name));
        Assert.Equal(1, runSpeed.GetProperty("evaluated_row_count").GetInt32());
        var inferenceElapsedMs = runSpeed.GetProperty("inference_elapsed_ms").GetInt64();
        Assert.True(inferenceElapsedMs > 0);
        var throughputRowsPerSec =
            runSpeed.GetProperty("throughput_rows_per_sec").GetDouble();
        Assert.True(double.IsFinite(throughputRowsPerSec));
        Assert.True(throughputRowsPerSec > 0d);
        Assert.Equal(1000d / inferenceElapsedMs, throughputRowsPerSec);
        Assert.Equal(0L, runSpeed.GetProperty("routing_elapsed_ms").GetInt64());
        Assert.Equal(0, runSpeed.GetProperty("candidate_sample_count").GetInt32());
        Assert.Equal(0, runSpeed.GetProperty("batch_sample_count").GetInt32());
        Assert.Equal(
            JsonValueKind.Null,
            runSpeed.GetProperty("per_candidate_elapsed_ms").ValueKind);
        Assert.Equal(
            JsonValueKind.Null,
            runSpeed.GetProperty("per_batch_elapsed_ms").ValueKind);
    }

    private static async Task<HttpResponseMessage> PostSelectedTargetEvidenceAsync(
        HttpClient client,
        string sessionId,
        DynamicGraphMeasurementTarget target,
        EdgeAuthorityExecutionResult authorityResult,
        DynamicGraphEdgeAuthorityNoveltyEvaluationRequest noveltyRequest,
        string requestId,
        bool includeAssociation = false)
    {
        var payload = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["target"] = target,
            ["authority_result"] = CreateSelectedTargetAuthorityWire(authorityResult),
            ["novelty_request"] = CreateSelectedTargetNoveltyWire(noveltyRequest)
        };
        if (includeAssociation)
            payload["association"] = new { forbidden = true };

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/v1/dynamic-graph/sessions/{sessionId}/selected-target-evidence")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add("X-XIIGen-Tenant-Id", TenantId);
        request.Headers.Add("X-Request-Id", requestId);
        return await client.SendAsync(request);
    }

    private static object CreateSelectedTargetAuthorityWire(
        EdgeAuthorityExecutionResult result)
        => new
        {
            normalized_input = result.NormalizedInput,
            proposed_candidates = result.ProposedCandidates,
            decision = CreateSelectedTargetDecisionWire(result.Decision),
            authority_evidence = result.AuthorityEvidence,
            candidate_order_leakage_report = new
            {
                edge_id = result.CandidateOrderLeakageReport.EdgeId,
                resolved_authority_unit_id =
                    result.CandidateOrderLeakageReport.ResolvedAuthorityUnitId,
                is_order_invariant = result.CandidateOrderLeakageReport.IsOrderInvariant,
                probes = result.CandidateOrderLeakageReport.Probes.Select(probe => new
                {
                    probe_id = probe.ProbeId,
                    candidate_order = probe.CandidateOrder,
                    decision = CreateSelectedTargetDecisionWire(probe.Decision)
                }),
                excluded_leakage_signals =
                    result.CandidateOrderLeakageReport.ExcludedLeakageSignals
            },
            identity_leakage_report = new
            {
                edge_id = result.IdentityLeakageReport.EdgeId,
                resolved_authority_unit_id =
                    result.IdentityLeakageReport.ResolvedAuthorityUnitId,
                is_identity_invariant = result.IdentityLeakageReport.IsIdentityInvariant,
                probes = result.IdentityLeakageReport.Probes.Select(probe => new
                {
                    probe_id = probe.ProbeId,
                    controlled_signal = probe.ControlledSignal,
                    control_value = probe.ControlValue,
                    decision = CreateSelectedTargetDecisionWire(probe.Decision)
                }),
                excluded_leakage_signals =
                    result.IdentityLeakageReport.ExcludedLeakageSignals
            },
            unit_confidence = result.UnitConfidence,
            evidence_ids = result.EvidenceIds,
            warnings = result.Warnings
        };

    private static object CreateSelectedTargetDecisionWire(EdgeAuthorityDecision decision)
        => new
        {
            ordered_targets = decision.OrderedTargets.Select(target => new
            {
                target_sub_phase_id = target.TargetSubPhaseId,
                confidence = target.Confidence
            }),
            fan_out_count = decision.FanOutCount,
            knowledge_edge = decision.KnowledgeEdge,
            authority_source = decision.AuthoritySource,
            ablation_baseline_rank = decision.AblationBaselineRank
        };

    private static object CreateSelectedTargetNoveltyWire(
        DynamicGraphEdgeAuthorityNoveltyEvaluationRequest request)
    {
        var pairedSelection = request.PairedSelection;
        ArgumentNullException.ThrowIfNull(pairedSelection);

        return new
        {
            paired_selection = new
            {
                evaluation_id = pairedSelection.EvaluationId,
                edge_id = pairedSelection.EdgeId,
                authority_unit_id = pairedSelection.AuthorityUnitId,
                scorer_id = pairedSelection.ScorerId,
                scorer_version = pairedSelection.ScorerVersion,
                threshold_set_id = pairedSelection.ThresholdSetId,
                normalization_id = pairedSelection.NormalizationId,
                split_proof = pairedSelection.SplitProof,
                rows = pairedSelection.Rows,
                rows_evaluated = pairedSelection.RowsEvaluated,
                learned_correct_count = pairedSelection.LearnedCorrectCount,
                learned_selection_accuracy =
                    pairedSelection.LearnedSelectionAccuracy,
                baseline_correct_count = pairedSelection.BaselineCorrectCount,
                baseline_selection_accuracy =
                    pairedSelection.BaselineSelectionAccuracy,
                accuracy_drop = pairedSelection.AccuracyDrop,
                normalized_accuracy_drop =
                    pairedSelection.NormalizedAccuracyDrop,
                meets_q11_numerical_gate =
                    pairedSelection.MeetsQ11NumericalGate
            },
            training_bindings = request.TrainingBindings,
            requested_row_ids = request.RequestedRowIds,
            novelty_rows = request.NoveltyRows,
            caller_attested_fingerprint_normalization_id =
                request.CallerAttestedFingerprintNormalizationId,
            caller_attested_tokenization_id = request.CallerAttestedTokenizationId
        };
    }

    private static async Task<HttpResponseMessage> PostSelectedTargetRunAsync(
        HttpClient client,
        string sessionId,
        string evidenceHandle,
        string runId,
        string scenarioId,
        string userRequest,
        string requestId)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/v1/dynamic-graph/sessions/{sessionId}/selected-target-runs")
        {
            Content = JsonContent.Create(new
            {
                evidence_handle = evidenceHandle,
                run_id = runId,
                scenario_id = scenarioId,
                user_request = userRequest
            })
        };
        request.Headers.Add("X-XIIGen-Tenant-Id", TenantId);
        request.Headers.Add("X-Request-Id", requestId);
        return await client.SendAsync(request);
    }

    private static (
        DynamicGraphMeasurementTarget Target,
        EdgeAuthorityExecutionResult AuthorityResult,
        DynamicGraphEdgeAuthorityNoveltyEvaluationRequest NoveltyRequest)
        CreateSelectedTargetAdmissionPayload()
    {
        var paired = CreateSelectedTargetPairedEvaluation();
        return (
            new DynamicGraphMeasurementTarget(
                SelectedTargetFlowId,
                DynamicGraphMeasurementTargetLevel.L3LogicalFlow,
                DynamicGraphMeasurementMechanismKind.LearningSubject,
                false),
            CreateSelectedTargetAuthorityResult(),
            new DynamicGraphEdgeAuthorityNoveltyEvaluationRequest(
                paired,
                [
                    new DynamicGraphEdgeAuthorityNoveltyTrainingBinding(
                        "train-a",
                        "train-hash",
                        [FingerprintA],
                        [FingerprintA])
                ],
                paired.Rows.Select(row => row.RowId).ToArray(),
                Enumerable.Range(0, paired.Rows.Count)
                    .Select(index => new DynamicGraphEdgeAuthorityNoveltyRowInput(
                        $"row-{index:D3}",
                        index % 2 == 0 ? FingerprintB : FingerprintC,
                        [index % 2 == 0 ? FingerprintB : FingerprintC]))
                    .ToArray(),
                "fingerprint-normalization-v1",
                "tokenization-v1"));
    }

    private static DynamicGraphEdgeAuthorityAblationEvaluationResult
        CreateSelectedTargetPairedEvaluation()
    {
        const int rowCount = 2;
        const int learnedCorrect = 2;
        const int baselineCorrect = 1;
        var split = new DynamicGraphMeasurementSplitProof(
            ["train-a"],
            ["eval-a"],
            ["train-hash", "eval-hash"],
            0,
            "simhash",
            0.85d);
        var rows = Enumerable.Range(0, rowCount)
            .Select(index =>
            {
                var learnedMatches = index < learnedCorrect;
                var baselineMatches = index < baselineCorrect;
                return new DynamicGraphEdgeAuthorityAblationRowEvidence(
                    $"row-{index:D3}",
                    "eval-a",
                    "eval-hash",
                    "document-000",
                    "repo://eval/a",
                    $"label-{index:D3}",
                    "target-a",
                    learnedMatches ? "target-a" : "target-b",
                    baselineMatches ? "target-a" : "target-b",
                    learnedMatches,
                    baselineMatches);
            })
            .ToArray();
        var learnedAccuracy = (double)learnedCorrect / rowCount;
        var baselineAccuracy = (double)baselineCorrect / rowCount;
        var rawDrop = learnedAccuracy - baselineAccuracy;
        var normalizedDrop = Math.Round(rawDrop, 12, MidpointRounding.AwayFromZero);
        var imported = DynamicGraphEdgeAuthorityAblationEvaluationRunner.ImportValidated(
            new DynamicGraphEdgeAuthorityAblationEvaluationImport(
                SelectedTargetEvaluationId,
                SelectedTargetFlowId,
                SelectedTargetAuthorityUnitId,
                DynamicGraphEdgeAuthorityAblationEvaluationResult.ProducedScorerId,
                DynamicGraphEdgeAuthorityAblationEvaluationResult.ProducedScorerVersion,
                DynamicGraphEdgeAuthorityAblationEvaluationResult.ProducedThresholdSetId,
                DynamicGraphEdgeAuthorityAblationEvaluationResult.ProducedNormalizationId,
                split,
                rows,
                learnedCorrect,
                learnedAccuracy,
                baselineCorrect,
                baselineAccuracy,
                rawDrop,
                normalizedDrop,
                normalizedDrop >= DynamicGraphAblationMeter.MinimumAccuracyDrop
            ));
        Assert.True(imported.IsSuccess, $"{imported.ErrorCode}: {imported.ErrorMessage}");
        return imported.Value!;
    }

    private static EdgeAuthorityExecutionResult CreateSelectedTargetAuthorityResult()
    {
        var candidates = Enumerable.Range(0, 3)
            .Select(index => new EdgeCandidate(
                $"target-{(char)('a' + index)}",
                [$"PREDICATE_{(char)('A' + index)}"],
                0.2d + (index * 0.2d),
                RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole))
            .ToArray();
        var decision = new EdgeAuthorityDecision(
            [("target-c", 0.93d)],
            1,
            false,
            RoutingAuthorityVocabulary.TrainedEdgeUnitRole,
            candidates
                .OrderByDescending(candidate => candidate.BaselineScore)
                .ThenBy(candidate => candidate.TargetSubPhaseId, StringComparer.Ordinal)
                .Select(candidate => candidate.TargetSubPhaseId)
                .ToArray());
        var normalizedInput = new EdgeAuthorityInput(
            SelectedTargetFlowId,
            ["edge", "selection", "target"],
            candidates.SelectMany(candidate => candidate.EvidencePredicates).ToArray(),
            new ContextSignals(
                [Domain.CODE_Q],
                ["completed-sub-phase"],
                ["active-edge"],
                new Dictionary<Domain, double> { [Domain.CODE_Q] = 0.8d }),
            candidates,
            SelectedTargetLeakageSignals);
        var orderReport = new DynamicGraphEdgeCandidateOrderLeakageReport(
            SelectedTargetFlowId,
            SelectedTargetAuthorityUnitId,
            true,
            Enumerable.Range(0, 5)
                .Select(index => new DynamicGraphEdgeCandidateOrderPermutationProbe(
                    $"candidate-order-permutation-{index + 1:D2}",
                    CreateSelectedTargetPermutation(candidates, index),
                    CopySelectedTargetDecision(decision)))
                .ToArray(),
            SelectedTargetLeakageSignals);
        var identityReport = new DynamicGraphEdgeIdentityLeakageReport(
            SelectedTargetFlowId,
            SelectedTargetAuthorityUnitId,
            true,
            Enumerable.Range(0, 3)
                .Select(index => new DynamicGraphEdgeIdentityControlProbe(
                    SelectedTargetIdentityProbeIds[index],
                    SelectedTargetIdentityControlledSignals[index],
                    SelectedTargetIdentityControlValues[index],
                    CopySelectedTargetDecision(decision)))
                .ToArray(),
            SelectedTargetLeakageSignals);

        return new EdgeAuthorityExecutionResult(
            normalizedInput,
            candidates.Select(CopySelectedTargetCandidate).ToArray(),
            CopySelectedTargetDecision(decision),
            new DynamicGraphEdgeAuthorityEvidence(
                SelectedTargetFlowId,
                SelectedTargetAuthorityUnitId,
                "L2",
                false,
                "live-selected-target-baseline-unit",
                "candidate_generator",
                SelectedTargetLeakageSignals,
                "live-selected-target-source-proof",
                "live-selected-target-cutover-proof",
                "live-selected-target-ablation-proof"),
            orderReport,
            identityReport,
            0.93d,
            ["live-selected-target-evidence"],
            []);
    }

    private static EdgeCandidate[] CreateSelectedTargetPermutation(
        IReadOnlyList<EdgeCandidate> candidates,
        int index)
    {
        var source = candidates.Select(CopySelectedTargetCandidate).ToArray();
        var offset = index switch
        {
            0 => 0,
            1 => 1,
            2 => 2,
            3 => -1,
            4 => -2,
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };
        if (offset < 0)
            return RotateSelectedTargetCandidates(source.Reverse().ToArray(), -offset - 1);
        return RotateSelectedTargetCandidates(source, offset);
    }

    private static EdgeCandidate[] RotateSelectedTargetCandidates(
        IReadOnlyList<EdgeCandidate> source,
        int offset)
        => Enumerable.Range(0, source.Count)
            .Select(index => CopySelectedTargetCandidate(source[(index + offset) % source.Count]))
            .ToArray();

    private static EdgeCandidate CopySelectedTargetCandidate(EdgeCandidate source)
        => source with { EvidencePredicates = source.EvidencePredicates.ToArray() };

    private static EdgeAuthorityDecision CopySelectedTargetDecision(EdgeAuthorityDecision source)
        => new(
            source.OrderedTargets.ToArray(),
            source.FanOutCount,
            source.KnowledgeEdge,
            source.AuthoritySource,
            source.AblationBaselineRank.ToArray());

    private static DynamicGraphLogicalOrchestratorBootstrapProfile CreateProfileWithId(string profileId)
        => CreateProfile(TenantId) with { ProfileId = profileId };

    private static Task<HttpResponseMessage> PostCatalogAsync(
        HttpClient client,
        DynamicGraphLogicalOrchestratorBootstrapProfile[] profiles,
        string rawSelectedProfileId,
        string idempotencyKey,
        string requestId = "catalog.request.default")
        => PostCatalogBytesAsync(
            client,
            Catalog(profiles),
            $"?selected_profile_id={rawSelectedProfileId}",
            idempotencyKey,
            requestId: requestId);

    private static async Task<HttpResponseMessage> PostCatalogBytesAsync(
        HttpClient client,
        byte[] body,
        string? rawQuery,
        string idempotencyKey,
        string mediaType = "application/json",
        string requestId = "catalog.request.default",
        string tenantId = TenantId,
        bool includeTenantHeader = true,
        bool includeIdempotencyKey = true)
    {
        var content = new ByteArrayContent(body);
        content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/v1/dynamic-graph/catalog-sessions" + rawQuery)
        {
            Content = content
        };
        if (includeTenantHeader)
            request.Headers.Add("X-XIIGen-Tenant-Id", tenantId);
        if (includeIdempotencyKey)
            request.Headers.Add("Idempotency-Key", idempotencyKey);
        request.Headers.Add("X-Request-Id", requestId);
        return await client.SendAsync(request);
    }

    private static async Task<string> SendRawCatalogRequestAsync(
        Uri baseUrl,
        string requestTarget,
        byte[] body,
        string requestId,
        bool chunked = false)
    {
        using var tcp = new TcpClient();
        await tcp.ConnectAsync(baseUrl.Host, baseUrl.Port);
        await using var stream = tcp.GetStream();
        var framing = chunked
            ? "Transfer-Encoding: chunked"
            : $"Content-Length: {body.Length}";
        var headers = string.Join(
            "\r\n",
            $"POST {requestTarget} HTTP/1.1",
            $"Host: {baseUrl.Host}:{baseUrl.Port}",
            framing,
            "Content-Type: application/json",
            $"X-XIIGen-Tenant-Id: {TenantId}",
            $"Idempotency-Key: {requestId}",
            $"X-Request-Id: {requestId}",
            "Connection: close",
            "",
            "");
        await stream.WriteAsync(Encoding.ASCII.GetBytes(headers));
        if (chunked)
        {
            await stream.WriteAsync(Encoding.ASCII.GetBytes(body.Length.ToString("x") + "\r\n"));
            await stream.WriteAsync(body);
            await stream.WriteAsync(Encoding.ASCII.GetBytes("\r\n0\r\n\r\n"));
        }
        else
        {
            await stream.WriteAsync(body);
        }
        await stream.FlushAsync();

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        using var response = new MemoryStream();
        await stream.CopyToAsync(response, timeout.Token);
        return Encoding.UTF8.GetString(response.ToArray());
    }

    private static async Task SendAbortedCatalogRequestAsync(Uri baseUrl)
    {
        using var tcp = new TcpClient();
        await tcp.ConnectAsync(baseUrl.Host, baseUrl.Port);
        await using var stream = tcp.GetStream();
        var headers = string.Join(
            "\r\n",
            "POST /api/v1/dynamic-graph/catalog-sessions?selected_profile_id=aborted HTTP/1.1",
            $"Host: {baseUrl.Host}:{baseUrl.Port}",
            "Transfer-Encoding: chunked",
            "Content-Type: application/json",
            $"X-XIIGen-Tenant-Id: {TenantId}",
            "Idempotency-Key: catalog-aborted-0001",
            "X-Request-Id: catalog.aborted.0001",
            "Connection: close",
            "",
            "",
            "400\r\n{\"partial\":true");
        await stream.WriteAsync(Encoding.ASCII.GetBytes(headers));
        await stream.FlushAsync();
    }

    private static async Task<string> ReadSessionIdAsync(HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("session_id").GetString()!;
    }

    private static async Task AssertErrorEnvelopeAsync(
        HttpResponseMessage response,
        HttpStatusCode expectedStatus,
        string expectedCode,
        string expectedMessage)
    {
        var responseText = await response.Content.ReadAsStringAsync();
        Assert.True(
            response.StatusCode == expectedStatus,
            $"Expected {(int)expectedStatus}/{expectedCode} but received " +
            $"{(int)response.StatusCode}: {responseText}");
        Assert.Null(response.Headers.Location);
        using var document = JsonDocument.Parse(responseText);
        var root = document.RootElement;
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("request_id").GetString()));
        Assert.Equal(expectedCode, root.GetProperty("code").GetString());
        Assert.Equal(expectedMessage, root.GetProperty("message").GetString());
        Assert.False(root.TryGetProperty("stack_trace", out _));
    }

    private static async Task<HttpResponseMessage> PostProfileAsync(
        HttpClient client,
        DynamicGraphLogicalOrchestratorBootstrapProfile profile,
        string idempotencyKey,
        string tenantId,
        string requestId = "request.default")
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/dynamic-graph/sessions")
        {
            Content = JsonContent.Create(new { profile, trained_input = (object?)null })
        };
        request.Headers.Add("X-XIIGen-Tenant-Id", tenantId);
        request.Headers.Add("Idempotency-Key", idempotencyKey);
        request.Headers.Add("X-Request-Id", requestId);
        return await client.SendAsync(request);
    }

    private static async Task<HttpResponseMessage> PostAssistantRunAsync(
        HttpClient client,
        string sessionId,
        string userRequest,
        string tenantId,
        string requestId)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post, $"/api/v1/dynamic-graph/sessions/{sessionId}/runs")
        {
            Content = JsonContent.Create(new { user_request = userRequest })
        };
        request.Headers.Add("X-XIIGen-Tenant-Id", tenantId);
        request.Headers.Add("X-Request-Id", requestId);
        return await client.SendAsync(request);
    }

    private static async Task<HttpResponseMessage> PostTopologyRunAsync(
        HttpClient client,
        string sessionId,
        string userRequest,
        string tenantId,
        string requestId)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/v1/dynamic-graph/sessions/{sessionId}/topology-runs")
        {
            Content = JsonContent.Create(new { user_request = userRequest })
        };
        request.Headers.Add("X-XIIGen-Tenant-Id", tenantId);
        request.Headers.Add("X-Request-Id", requestId);
        return await client.SendAsync(request);
    }

    private static async Task<HttpResponseMessage> PostTopologyMutationAsync(
        HttpClient client,
        string sessionId,
        long expectedRevision,
        DynamicGraphLogicalOrchestratorBootstrapProfile profile,
        string tenantId,
        string requestId)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/v1/dynamic-graph/sessions/{sessionId}/topology-mutations")
        {
            Content = JsonContent.Create(new { expected_revision = expectedRevision, profile })
        };
        request.Headers.Add("X-XIIGen-Tenant-Id", tenantId);
        request.Headers.Add("X-Request-Id", requestId);
        return await client.SendAsync(request);
    }

    private static string NormalizeSource(string source)
        => source.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Trim();

    private static string ExtractUniqueBoundedSourceBlock(
        string source,
        string startMarker,
        string endMarker)
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        Assert.True(start >= 0, $"Start marker was not found: {startMarker}");
        Assert.Equal(start, source.LastIndexOf(startMarker, StringComparison.Ordinal));
        var end = source.IndexOf(endMarker, start + startMarker.Length, StringComparison.Ordinal);
        Assert.True(end > start, $"End marker was not found after the start marker: {endMarker}");
        Assert.Equal(end, source.LastIndexOf(endMarker, StringComparison.Ordinal));
        var lineStart = source.LastIndexOf('\n', start) + 1;
        var indentation = start - lineStart;
        var lines = source[lineStart..end].TrimEnd().Split('\n');
        Assert.All(
            lines.Where(line => !string.IsNullOrWhiteSpace(line)),
            line => Assert.True(
                line.Length >= indentation && line[..indentation].All(char.IsWhiteSpace),
                $"Source block line does not retain its base indentation: {line}"));
        return string.Join(
            "\n",
            lines.Select(line => line.Length >= indentation ? line[indentation..] : string.Empty));
    }

    private static void AssertTokensInOrder(string sourceBlock, params string[] tokens)
    {
        var previousIndex = -1;
        string? previousToken = null;
        foreach (var token in tokens)
        {
            var matches = Regex.Matches(
                sourceBlock,
                Regex.Escape(token),
                RegexOptions.CultureInvariant);
            Assert.True(
                matches.Count == 1,
                $"Expected exactly one binding token but found {matches.Count}: {token}\nBlock:\n{sourceBlock}");
            var index = matches[0].Index;
            Assert.True(
                index > previousIndex,
                $"Binding token is out of order: {token}\nPrevious token: {previousToken}\nBlock:\n{sourceBlock}");
            previousIndex = index;
            previousToken = token;
        }
    }

    private static bool IsRequiredCompositionReadinessGate(IfStatementSyntax statement)
        => statement.Else is null
           && statement.Condition is PrefixUnaryExpressionSyntax prefix
           && prefix.IsKind(SyntaxKind.LogicalNotExpression)
           && prefix.Operand is InvocationExpressionSyntax invocation
           && IsSimpleInvocation(invocation, "readiness", "AllowsComposition", "requireReady")
           && statement.Statement is ThrowStatementSyntax
           {
               Expression: ObjectCreationExpressionSyntax
               {
                   Type: IdentifierNameSyntax { Identifier.ValueText: "ForbiddenTrainingAssemblyException" },
                   ArgumentList.Arguments.Count: 0,
                   Initializer: null
               }
           };

    private static void AssertPersistenceCryptographicBoundary(
        string registrySource,
        string persistenceSource)
    {
        var registryRoot = CSharpSyntaxTree.ParseText(registrySource).GetRoot();
        var registryType = Assert.Single(
            registryRoot.DescendantNodes().OfType<ClassDeclarationSyntax>(),
            declaration => declaration.Identifier.ValueText == "DynamicGraphCompositionSessionRegistry");
        var registryDeriveIndex = Assert.Single(
            registryType.Members.OfType<MethodDeclarationSyntax>(),
            method => method.Identifier.ValueText == "DeriveIndex");
        AssertParamsStringArrayParameter(registryDeriveIndex, "components");
        Assert.Null(registryDeriveIndex.Body);
        var registryExpressionBody = Assert.IsType<ArrowExpressionClauseSyntax>(
            registryDeriveIndex.ExpressionBody);
        var registryDelegation = Assert.IsType<InvocationExpressionSyntax>(
            registryExpressionBody.Expression);
        Assert.True(
            IsSimpleInvocation(
                registryDelegation,
                "_persistence",
                "DeriveIndex",
                "components"),
            $"Registry DeriveIndex must delegate only its components parameter: {registryExpressionBody}");

        var persistenceRoot = CSharpSyntaxTree.ParseText(persistenceSource).GetRoot();
        var persistenceType = Assert.Single(
            persistenceRoot.DescendantNodes().OfType<ClassDeclarationSyntax>(),
            declaration => declaration.Identifier.ValueText == "DynamicGraphCompositionSessionPersistence");
        var persistenceDeriveIndex = Assert.Single(
            persistenceType.Members.OfType<MethodDeclarationSyntax>(),
            method => method.Identifier.ValueText == "DeriveIndex");
        AssertParamsStringArrayParameter(persistenceDeriveIndex, "components");
        Assert.Null(persistenceDeriveIndex.ExpressionBody);
        Assert.IsType<BlockSyntax>(persistenceDeriveIndex.Body);

        var expectedFramingLoop = Assert.IsType<ForStatementSyntax>(SyntaxFactory.ParseStatement(
            """
            for (var index = 0; index < encodedCount; index++)
            {
                var component = encoded[index]!;
                BinaryPrimitives.WriteInt32BigEndian(payload.AsSpan(offset, sizeof(int)), component.Length);
                offset += sizeof(int);
                component.CopyTo(payload, offset);
                offset += component.Length;
            }
            """));
        var framingLoop = Assert.Single(
            persistenceDeriveIndex.DescendantNodes().OfType<ForStatementSyntax>(),
            loop => HasEquivalentTokens(loop, expectedFramingLoop));
        var expectedDigestStatement = SyntaxFactory.ParseStatement(
            "digest = HMACSHA256.HashData(_indexKey, payload);");
        var expectedReturnStatement = Assert.IsType<ReturnStatementSyntax>(SyntaxFactory.ParseStatement(
            "return Convert.ToHexString(digest);"));
        var digestCall = Assert.Single(
            persistenceDeriveIndex.DescendantNodes().OfType<InvocationExpressionSyntax>(),
            invocation => IsSimpleInvocation(
                invocation, "HMACSHA256", "HashData", "_indexKey", "payload"));
        var digestStatement = Assert.IsType<ExpressionStatementSyntax>(
            digestCall.FirstAncestorOrSelf<StatementSyntax>());
        Assert.True(
            HasEquivalentTokens(digestStatement, expectedDigestStatement),
            $"DeriveIndex must assign the scoped HMAC digest exactly: {digestStatement}");
        var digestReturn = Assert.Single(
            persistenceDeriveIndex.DescendantNodes().OfType<ReturnStatementSyntax>(),
            statement => HasEquivalentTokens(statement, expectedReturnStatement));
        var framingBlock = Assert.IsType<BlockSyntax>(framingLoop.Parent);
        var deriveTry = Assert.IsType<TryStatementSyntax>(framingBlock.Parent);
        Assert.Same(deriveTry.Block, framingBlock);
        Assert.Same(persistenceDeriveIndex.Body, deriveTry.Parent);
        var framingIndex = framingBlock.Statements.IndexOf(framingLoop);
        Assert.True(
            framingIndex >= 0 && framingIndex + 2 < framingBlock.Statements.Count,
            "The DeriveIndex framing loop must have direct digest and return successors.");
        Assert.Same(digestStatement, framingBlock.Statements[framingIndex + 1]);
        Assert.Same(digestReturn, framingBlock.Statements[framingIndex + 2]);

        var dispose = Assert.Single(
            persistenceType.Members.OfType<MethodDeclarationSyntax>(),
            method => method.Identifier.ValueText == "Dispose");
        Assert.Empty(dispose.ParameterList.Parameters);
        var disposeBody = Assert.IsType<BlockSyntax>(dispose.Body);
        var waitForOperations = Assert.Single(
            dispose.DescendantNodes().OfType<InvocationExpressionSyntax>(),
            invocation => IsIdentifierInvocation(invocation, "WaitForActiveOperations"));
        var disposeKeyZeroing = Assert.Single(
            dispose.DescendantNodes().OfType<InvocationExpressionSyntax>(),
            invocation => IsSimpleInvocation(
                invocation, "CryptographicOperations", "ZeroMemory", "_indexKey"));
        var disposeLifetimeLock = Assert.Single(
            dispose.DescendantNodes().OfType<InvocationExpressionSyntax>(),
            invocation => IsConditionalDisposeInvocation(invocation, "_lifetimeLock"));
        var disposeTreeLease = Assert.Single(
            dispose.DescendantNodes().OfType<InvocationExpressionSyntax>(),
            invocation => IsConditionalDisposeInvocation(invocation, "_treeLease"));
        var waitStatement = Assert.IsType<ExpressionStatementSyntax>(
            waitForOperations.FirstAncestorOrSelf<StatementSyntax>());
        Assert.Same(disposeBody, waitStatement.Parent);
        var disposeKeyTry = Assert.IsType<TryStatementSyntax>(
            disposeKeyZeroing.FirstAncestorOrSelf<TryStatementSyntax>());
        Assert.Same(disposeBody, disposeKeyTry.Parent);
        Assert.Same(disposeKeyTry.Block, disposeKeyZeroing.FirstAncestorOrSelf<BlockSyntax>());
        var disposeFinally = Assert.IsType<FinallyClauseSyntax>(disposeKeyTry.Finally);
        Assert.Contains(disposeLifetimeLock, disposeFinally.Block.DescendantNodes().OfType<InvocationExpressionSyntax>());
        Assert.Contains(disposeTreeLease, disposeFinally.Block.DescendantNodes().OfType<InvocationExpressionSyntax>());
        Assert.True(
            waitForOperations.SpanStart < disposeKeyZeroing.SpanStart
            && disposeKeyZeroing.SpanStart < disposeLifetimeLock.SpanStart
            && disposeLifetimeLock.SpanStart < disposeTreeLease.SpanStart,
            "Persistence Dispose must drain operations, zero the index key, then release owned resources.");

        var cleanup = Assert.Single(
            persistenceType.Members.OfType<MethodDeclarationSyntax>(),
            method => method.Identifier.ValueText == "CleanupAfterFailedConstruction");
        var expectedCleanupZeroing = Assert.IsType<IfStatementSyntax>(SyntaxFactory.ParseStatement(
            """
            if (assignedIndexKey is not null)
                CryptographicOperations.ZeroMemory(assignedIndexKey);
            """));
        var cleanupZeroing = Assert.Single(
            cleanup.DescendantNodes().OfType<IfStatementSyntax>(),
            statement => HasEquivalentTokens(statement, expectedCleanupZeroing));
        var cleanupKeyZeroing = Assert.Single(
            cleanup.DescendantNodes().OfType<InvocationExpressionSyntax>(),
            invocation => IsSimpleInvocation(
                invocation, "CryptographicOperations", "ZeroMemory", "assignedIndexKey"));
        Assert.Contains(cleanupKeyZeroing, cleanupZeroing.DescendantNodes().OfType<InvocationExpressionSyntax>());
        var cleanupLifetimeLock = Assert.Single(
            cleanup.DescendantNodes().OfType<InvocationExpressionSyntax>(),
            invocation => IsConditionalDisposeInvocation(invocation, "_lifetimeLock"));
        var cleanupTreeLease = Assert.Single(
            cleanup.DescendantNodes().OfType<InvocationExpressionSyntax>(),
            invocation => IsConditionalDisposeInvocation(invocation, "_treeLease"));
        var cleanupKeyTry = Assert.IsType<TryStatementSyntax>(
            cleanupKeyZeroing.FirstAncestorOrSelf<TryStatementSyntax>());
        Assert.Same(cleanupKeyTry.Block, cleanupZeroing.Parent);
        var cleanupFinally = Assert.IsType<FinallyClauseSyntax>(cleanupKeyTry.Finally);
        Assert.Contains(cleanupLifetimeLock, cleanupFinally.Block.DescendantNodes().OfType<InvocationExpressionSyntax>());
        Assert.Contains(cleanupTreeLease, cleanupFinally.Block.DescendantNodes().OfType<InvocationExpressionSyntax>());
        Assert.True(
            cleanupKeyZeroing.SpanStart < cleanupLifetimeLock.SpanStart
            && cleanupLifetimeLock.SpanStart < cleanupTreeLease.SpanStart,
            "Failed construction must zero its assigned index key before releasing owned resources.");

        var constructor = Assert.Single(
            persistenceType.Members.OfType<ConstructorDeclarationSyntax>(),
            declaration => declaration.Identifier.ValueText ==
                           "DynamicGraphCompositionSessionPersistence");
        var cleanupCalls = constructor.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(invocation => IsIdentifierInvocation(
                invocation, "CleanupAfterFailedConstruction", "assignedIndexKey"))
            .ToArray();
        Assert.Equal(2, cleanupCalls.Length);
        var cleanupCatchClauses = cleanupCalls
            .Select(call => call.FirstAncestorOrSelf<CatchClauseSyntax>())
            .ToArray();
        Assert.All(cleanupCatchClauses, clause => Assert.NotNull(clause));
        Assert.NotSame(cleanupCatchClauses[0], cleanupCatchClauses[1]);
    }

    private static void AssertParamsStringArrayParameter(
        MethodDeclarationSyntax method,
        string parameterName)
    {
        var parameter = Assert.Single(method.ParameterList.Parameters);
        Assert.Equal(parameterName, parameter.Identifier.ValueText);
        Assert.True(
            parameter.Modifiers.Count == 1
            && parameter.Modifiers[0].IsKind(SyntaxKind.ParamsKeyword),
            $"{method.Identifier.ValueText}.{parameterName} must be the sole params parameter.");
        Assert.True(
            parameter.Type is ArrayTypeSyntax
            {
                ElementType: PredefinedTypeSyntax { Keyword.RawKind: (int)SyntaxKind.StringKeyword },
                RankSpecifiers.Count: 1
            } arrayType
            && arrayType.RankSpecifiers[0].Sizes.Count == 1
            && arrayType.RankSpecifiers[0].Sizes[0].IsKind(SyntaxKind.OmittedArraySizeExpression),
            $"{method.Identifier.ValueText}.{parameterName} must be string[].");
    }

    private static bool IsConditionalDisposeInvocation(
        InvocationExpressionSyntax invocation,
        string owner)
        => invocation.Expression is MemberBindingExpressionSyntax
           {
               Name.Identifier.ValueText: "Dispose"
           }
           && invocation.ArgumentList.Arguments.Count == 0
           && invocation.Parent is ConditionalAccessExpressionSyntax
           {
               Expression: IdentifierNameSyntax { Identifier.ValueText: var actualOwner },
               WhenNotNull: var whenNotNull
           }
           && ReferenceEquals(whenNotNull, invocation)
           && string.Equals(actualOwner, owner, StringComparison.Ordinal);

    private static bool HasEquivalentTokens(SyntaxNode actual, SyntaxNode expected)
        => actual.RawKind == expected.RawKind
           && actual.DescendantTokens()
               .Select(token => (token.RawKind, token.Text))
               .SequenceEqual(expected.DescendantTokens()
                   .Select(token => (token.RawKind, token.Text)));

    private static void AssertIfReturnsExactError(
        SyntaxNode root,
        string conditionSource,
        int expectedStatus,
        string expectedCode)
    {
        var expectedCondition = SyntaxFactory.ParseExpression(conditionSource);
        var statement = Assert.Single(
            root.DescendantNodes().OfType<IfStatementSyntax>(),
            candidate => HasEquivalentTokens(candidate.Condition, expectedCondition));
        _ = AssertReturnsExactError(statement.Statement, expectedStatus, expectedCode);
    }

    private static void AssertForwardedDownstreamCodeMapping(
        SyntaxNode root,
        string conditionSource,
        int expectedStatus,
        string expectedMessage)
    {
        var expectedCondition = SyntaxFactory.ParseExpression(conditionSource);
        var statement = Assert.Single(
            root.DescendantNodes().OfType<IfStatementSyntax>(),
            candidate => HasEquivalentTokens(candidate.Condition, expectedCondition));
        var invocation = AssertReturnsExactError(
            statement.Statement,
            expectedStatus,
            expectedCode: null);
        Assert.True(
            invocation.ArgumentList.Arguments[2].Expression is IdentifierNameSyntax
            {
                Identifier.ValueText: "code"
            });
        var message = Assert.IsType<LiteralExpressionSyntax>(
            invocation.ArgumentList.Arguments[3].Expression);
        Assert.True(message.IsKind(SyntaxKind.StringLiteralExpression));
        Assert.Equal(expectedMessage, message.Token.ValueText);
    }

    private static InvocationExpressionSyntax AssertReturnsExactError(
        SyntaxNode owner,
        int expectedStatus,
        string? expectedCode)
    {
        var returnStatement = owner is ReturnStatementSyntax directReturn
            ? directReturn
            : Assert.Single(owner.DescendantNodes().OfType<ReturnStatementSyntax>());
        var invocation = Assert.IsType<InvocationExpressionSyntax>(returnStatement.Expression);
        Assert.True(
            invocation.Expression is IdentifierNameSyntax { Identifier.ValueText: "Error" },
            $"Expected an Error(...) return but found: {returnStatement}");
        Assert.Equal(4, invocation.ArgumentList.Arguments.Count);

        var status = Assert.IsType<LiteralExpressionSyntax>(
            invocation.ArgumentList.Arguments[0].Expression);
        Assert.True(status.IsKind(SyntaxKind.NumericLiteralExpression));
        Assert.Equal(expectedStatus.ToString(), status.Token.ValueText);
        Assert.True(
            invocation.ArgumentList.Arguments[1].Expression is IdentifierNameSyntax
            {
                Identifier.ValueText: "requestId"
            });

        if (expectedCode is not null)
        {
            var code = Assert.IsType<LiteralExpressionSyntax>(
                invocation.ArgumentList.Arguments[2].Expression);
            Assert.True(code.IsKind(SyntaxKind.StringLiteralExpression));
            Assert.Equal(expectedCode, code.Token.ValueText);
        }

        return invocation;
    }

    private static void AssertInvalidationRaceBoundary(MethodDeclarationSyntax method)
    {
        var assignments = method.DescendantNodes().OfType<AssignmentExpressionSyntax>().ToArray();
        var callbacksSnapshot = Assert.Single(
            assignments,
            assignment => assignment.Left is IdentifierNameSyntax { Identifier.ValueText: "callbacks" }
                          && assignment.Right is InvocationExpressionSyntax invocation
                          && IsSimpleInvocation(invocation, "_invalidationCallbacks", "ToArray"));
        var workLeaseAdmission = Assert.Single(
            assignments,
            assignment => assignment.Left is IdentifierNameSyntax { Identifier.ValueText: "workLease" }
                          && assignment.Right is InvocationExpressionSyntax invocation
                          && IsIdentifierInvocation(invocation, "AdmitInvalidationWork"));
        var callbacksClear = Assert.Single(
            method.DescendantNodes().OfType<InvocationExpressionSyntax>(),
            invocation => IsSimpleInvocation(invocation, "_invalidationCallbacks", "Clear"));
        var staleReadyFileRemoval = Assert.Single(
            method.DescendantNodes().OfType<InvocationExpressionSyntax>(),
            invocation => IsSimpleInvocation(invocation, "ReadyFile", "RemoveStale", "_readyFile"));
        var invalidationLoop = Assert.Single(
            method.DescendantNodes().OfType<ForEachStatementSyntax>(),
            loop => loop.Identifier.ValueText == "callback"
                    && loop.Expression is IdentifierNameSyntax { Identifier.ValueText: "callbacks" }
                    && loop.Statement.DescendantNodesAndSelf()
                        .OfType<InvocationExpressionSyntax>()
                        .Any(invocation => IsIdentifierInvocation(
                            invocation, "InvokeInvalidation", "callback")));

        Assert.True(
            callbacksSnapshot.SpanStart < workLeaseAdmission.SpanStart
            && workLeaseAdmission.SpanStart < callbacksClear.SpanStart
            && callbacksClear.SpanStart < staleReadyFileRemoval.SpanStart
            && staleReadyFileRemoval.SpanStart < invalidationLoop.SpanStart,
            $"{method.Identifier.ValueText} must snapshot, lease, clear, remove readiness, then invalidate in order.");
        var invalidationLock = Assert.IsType<LockStatementSyntax>(
            callbacksClear.FirstAncestorOrSelf<LockStatementSyntax>());
        Assert.Same(invalidationLock, callbacksSnapshot.FirstAncestorOrSelf<LockStatementSyntax>());
        Assert.Same(invalidationLock, workLeaseAdmission.FirstAncestorOrSelf<LockStatementSyntax>());
        Assert.Null(staleReadyFileRemoval.FirstAncestorOrSelf<LockStatementSyntax>());
        Assert.Null(invalidationLoop.FirstAncestorOrSelf<LockStatementSyntax>());
        var workLeaseScope = Assert.IsType<UsingStatementSyntax>(
            staleReadyFileRemoval.FirstAncestorOrSelf<UsingStatementSyntax>());
        Assert.True(
            workLeaseScope.Expression is IdentifierNameSyntax { Identifier.ValueText: "workLease" },
            $"{method.Identifier.ValueText} must hold its invalidation work lease while publishing callbacks.");
        Assert.Same(workLeaseScope, invalidationLoop.FirstAncestorOrSelf<UsingStatementSyntax>());
    }

    private static bool IsSimpleInvocation(
        InvocationExpressionSyntax invocation,
        string receiver,
        string method,
        params string[] arguments)
    {
        if (invocation.Expression is not MemberAccessExpressionSyntax
            {
                Expression: IdentifierNameSyntax { Identifier.ValueText: var actualReceiver },
                Name.Identifier.ValueText: var actualMethod
            }
            || !string.Equals(actualReceiver, receiver, StringComparison.Ordinal)
            || !string.Equals(actualMethod, method, StringComparison.Ordinal)
            || invocation.ArgumentList.Arguments.Count != arguments.Length)
        {
            return false;
        }

        return invocation.ArgumentList.Arguments
            .Select(argument => argument.Expression)
            .OfType<IdentifierNameSyntax>()
            .Select(identifier => identifier.Identifier.ValueText)
            .SequenceEqual(arguments, StringComparer.Ordinal);
    }

    private static bool IsIdentifierInvocation(
        InvocationExpressionSyntax invocation,
        string method,
        params string[] arguments)
    {
        if (invocation.Expression is not IdentifierNameSyntax { Identifier.ValueText: var actualMethod }
            || !string.Equals(actualMethod, method, StringComparison.Ordinal)
            || invocation.ArgumentList.Arguments.Count != arguments.Length)
        {
            return false;
        }

        return invocation.ArgumentList.Arguments
            .Select(argument => argument.Expression)
            .OfType<IdentifierNameSyntax>()
            .Select(identifier => identifier.Identifier.ValueText)
            .SequenceEqual(arguments, StringComparer.Ordinal);
    }

    private static void AssertPolicyResult(
        MethodInfo policyMethod,
        Exception exception,
        bool expected,
        string scenario)
    {
        var actual = InvokeStaticUnwrapped(policyMethod, [exception]);
        Assert.True(
            actual is bool,
            $"FatalExceptionPolicy returned a non-Boolean result for {scenario}: {actual?.GetType().FullName ?? "null"}");
        Assert.Equal(expected, (bool)actual);
    }

    private static object? InvokeStaticUnwrapped(MethodInfo method, object?[]? arguments)
    {
        try
        {
            return method.Invoke(null, arguments);
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw new InvalidOperationException("The captured reflection exception unexpectedly returned.");
        }
    }

    private static object ThrowReflectionInvocationSentinel()
        => throw new ReflectionInvocationSentinelException();

    private static async Task<HttpResponseMessage> GetSessionAsync(
        HttpClient client,
        string sessionId,
        string tenantId)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get, $"/api/v1/dynamic-graph/sessions/{sessionId}");
        request.Headers.Add("X-XIIGen-Tenant-Id", tenantId);
        return await client.SendAsync(request);
    }

    private static async Task<HttpResponseMessage> GetCurrentTopologyAsync(
        HttpClient client,
        string sessionId,
        string tenantId)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/v1/dynamic-graph/sessions/{sessionId}/topology");
        request.Headers.Add("X-XIIGen-Tenant-Id", tenantId);
        return await client.SendAsync(request);
    }

    private static async Task AssertCurrentTopologyAsync(
        HttpClient client,
        string sessionId,
        long bootstrapRevision,
        long currentRevision,
        string signalId,
        string sourceKind,
        int nodeCount,
        int edgeCount,
        bool revisionAdvanced,
        bool persistent)
    {
        using var response = await GetCurrentTopologyAsync(client, sessionId, TenantId);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var text = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(text);
        var root = document.RootElement;
        Assert.Equal(
            [
                "request_id",
                "session_id",
                "status",
                "bootstrap",
                "current",
                "revision_advanced_since_bootstrap",
                "boundaries"
            ],
            root.EnumerateObject().Select(property => property.Name));
        Assert.Equal(sessionId, root.GetProperty("session_id").GetString());
        Assert.Equal("current", root.GetProperty("status").GetString());
        Assert.Equal(
            bootstrapRevision,
            root.GetProperty("bootstrap").GetProperty("revision").GetInt64());
        var current = root.GetProperty("current");
        Assert.Equal(
            ["profile_id", "revision", "signal_id", "source_kind", "node_count", "edge_count"],
            current.EnumerateObject().Select(property => property.Name));
        Assert.Equal("live-local-profile", current.GetProperty("profile_id").GetString());
        Assert.Equal(currentRevision, current.GetProperty("revision").GetInt64());
        Assert.Equal(signalId, current.GetProperty("signal_id").GetString());
        Assert.Equal(sourceKind, current.GetProperty("source_kind").GetString());
        Assert.Equal(nodeCount, current.GetProperty("node_count").GetInt32());
        Assert.Equal(edgeCount, current.GetProperty("edge_count").GetInt32());
        Assert.Equal(
            revisionAdvanced,
            root.GetProperty("revision_advanced_since_bootstrap").GetBoolean());
        var boundaries = root.GetProperty("boundaries");
        Assert.Equal(persistent, boundaries.GetProperty("persistent").GetBoolean());
        Assert.False(boundaries.GetProperty("executes_assistant").GetBoolean());
        Assert.False(boundaries.GetProperty("applies_graph").GetBoolean());
        Assert.False(boundaries.GetProperty("performs_training").GetBoolean());
        foreach (var forbidden in new[]
                 {
                     "source_ref",
                     "trace_ref",
                     "topology",
                     "nodes",
                     "edges",
                     "tenant_id",
                     "domain_tag",
                     "context_id",
                     "budget",
                     "mutation_id",
                     "training"
                 })
        {
            Assert.DoesNotContain($"\"{forbidden}\"", text, StringComparison.Ordinal);
        }
    }

    private static async Task AssertErrorCodeAsync(HttpResponseMessage response, string expected)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(expected, document.RootElement.GetProperty("code").GetString());
        Assert.False(document.RootElement.TryGetProperty("stack_trace", out _));
    }

    private static DynamicGraphLogicalOrchestratorBootstrapProfile CreateProfile(string tenantId)
        => new(
            ProfileId: "live-local-profile",
            TenantId: tenantId,
            DomainTag: "software-development",
            ContextId: "live-local-context",
            SignalId: "live-local-signal",
            SourceRef: "logical-registry:live-local-profile",
            TraceRef: "trace:live-local-profile",
            NodeDefinitions:
            [
                CreateNode("3.4"),
                CreateNode("3.5"),
                CreateNode("4.1")
            ],
            EdgeBindings:
            [
                CreateEdge("live-edge-34-35", "transition-sub-phase-3.4-to-3.5", "live-authority-34"),
                CreateEdge("live-edge-35-41", "transition-sub-phase-3.5-to-4.1", "live-authority-35")
            ],
            Budget: new TopologyBuildBudget(8, 8, 8, 8));

    private static DynamicGraphLogicalOrchestratorNodeDefinition CreateNode(string nodeId)
        => new(
            nodeId,
            new DynamicGraphNodeDescriptor(
                nodeId,
                "software-development",
                "System.String",
                "System.String",
                IsTrained: false,
                DeterministicFinalAuthority: false,
                RuntimeSemanticQualityClaimed: false,
                AuthorityRole: "baseline_candidate_generator",
                CheckpointRef: null,
                ManifestRef: null));

    private static DynamicGraphRegistryEdgeBinding CreateEdge(
        string edgeId,
        string transitionId,
        string authorityId)
        => new(
            edgeId,
            [transitionId],
            GraphEdgeKind.FanOut,
            ParallelEligible: false,
            authorityId,
            "L2",
            DeterministicFinalAuthority: false,
            $"baseline-{edgeId}",
            "candidate_generator",
            ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"],
            $"source-proof:{edgeId}",
            $"cutover-proof:{edgeId}",
            $"ablation-proof:{edgeId}");

    [Fact]
    public void PromptToTaskUntrainedHonestyRoute_UsesExactContractsAdmissionAndEagerService()
    {
        var repoRoot = FindRepoRoot();
        var serverRoot = Path.Combine(
            repoRoot,
            "tools",
            "DynamicGraphCompositionLiveLocalServer");
        var contracts = NormalizeSource(File.ReadAllText(Path.Combine(
            serverRoot,
            "DynamicGraphCompositionHttpContracts.cs")));
        var program = NormalizeSource(File.ReadAllText(Path.Combine(serverRoot, "Program.cs")));
        var hostProject = NormalizeSource(File.ReadAllText(Path.Combine(
            serverRoot,
            "DynamicGraphCompositionLiveLocalServer.csproj")));
        var testsProject = NormalizeSource(File.ReadAllText(Path.Combine(
            repoRoot,
            "Tests",
            "LlmFromScratch.Tests.csproj")));
        var contractsRoot = CSharpSyntaxTree.ParseText(contracts).GetRoot();

        AssertSourceJsonContract(
            contractsRoot,
            "PromptToTaskUntrainedHonestyRunHttpRequest",
            strict: true,
            "evaluation_id",
            "run_id",
            "prompt_id",
            "raw_prompt",
            "user_language");
        AssertSourceJsonContract(
            contractsRoot,
            "PromptToTaskUntrainedHonestyTargetHttpResponse",
            strict: false,
            "target_id",
            "level",
            "mechanism_kind",
            "deterministic_final_authority");
        AssertSourceJsonContract(
            contractsRoot,
            "PromptToTaskUntrainedHonestyProducerHttpResponse",
            strict: false,
            "unit_id",
            "unit_role",
            "domain_tag",
            "version",
            "registration_order");
        AssertSourceJsonContract(
            contractsRoot,
            "PromptToTaskUntrainedHonestyMeasurementHttpResponse",
            strict: false,
            "runtime_reported_not_ready",
            "has_no_trained_state_evidence",
            "has_no_semantic_classification_evidence",
            "meets_untrained_honesty_gate");
        AssertSourceJsonContract(
            contractsRoot,
            "PromptToTaskUntrainedHonestyBoundaryHttpResponse",
            strict: false,
            "loopback_only",
            "tenant_binding_is_authentication",
            "persistent",
            "executes_unit",
            "measurement_only",
            "performs_training",
            "claims_trained_state",
            "claims_semantic_quality",
            "changes_authority",
            "persists_result");
        AssertSourceJsonContract(
            contractsRoot,
            "PromptToTaskUntrainedHonestyHttpResponse",
            strict: false,
            "request_id",
            "status",
            "evaluation_id",
            "target",
            "producer",
            "measurement",
            "direct_unit_invocation_count",
            "boundaries");

        const string route = "/api/v1/dynamic-graph/l2-untrained-honesty-runs";
        Assert.Contains($"app.MapPost(\"{route}\"", program, StringComparison.Ordinal);
        Assert.Contains($"\"POST {route}\"", program, StringComparison.Ordinal);
        var handler = ExtractUniqueBoundedSourceBlock(
            program,
            $"app.MapPost(\"{route}\"",
            "app.MapPost(\"/api/v1/dynamic-graph/sessions\"");
        AssertFirstOccurrencesInOrder(
            handler,
            "if (!readiness.IsReady)",
            "if (!context.Request.HasJsonContentType())",
            "if (context.Request.ContentLength > options.MaxRequestBodyBytes)",
            "ValidateConfiguredTenant(context, options, requestId)",
            "DeserializeStrictAsync<PromptToTaskUntrainedHonestyRunHttpRequest>",
            "body is null",
            "promptToTaskUntrainedHonestyExecutionOwner.ExecuteAsync(",
            "PromptToTaskUntrainedHonestyRun(");
        Assert.Equal(
            1,
            Regex.Matches(
                    handler,
                    Regex.Escape(
                        "promptToTaskUntrainedHonestyExecutionOwner.ExecuteAsync("),
                    RegexOptions.CultureInvariant)
                .Count);
        Assert.Equal(
            2,
            Regex.Matches(
                    handler,
                    Regex.Escape("if (!readiness.IsReady)"),
                    RegexOptions.CultureInvariant)
                .Count);
        Assert.True(
            handler.LastIndexOf("if (!readiness.IsReady)", StringComparison.Ordinal)
            > handler.IndexOf(
                "promptToTaskUntrainedHonestyExecutionOwner.ExecuteAsync(",
                StringComparison.Ordinal));
        Assert.Contains(
            "SnapshotExceptionGraphClassifier.IsNonFatal(exception)",
            handler,
            StringComparison.Ordinal);
        Assert.Contains(
            "HOST_L2_UNTRAINED_HONESTY_BODY_INVALID",
            handler,
            StringComparison.Ordinal);
        var cancelledMapping = ExtractUniqueBoundedSourceBlock(
            handler,
            "if (executed.Status == PromptToTaskUntrainedHonestyExecutionStatus.Cancelled)",
            "if (executed.Status == PromptToTaskUntrainedHonestyExecutionStatus.ConcurrencyLimited)");
        Assert.Contains("408", cancelledMapping, StringComparison.Ordinal);
        Assert.Contains(
            "HOST_L2_UNTRAINED_HONESTY_REQUEST_CANCELLED",
            cancelledMapping,
            StringComparison.Ordinal);
        Assert.DoesNotContain("DynamicGraphCompositionSessionRegistry", handler, StringComparison.Ordinal);
        Assert.DoesNotContain("registry.", handler, StringComparison.Ordinal);

        AssertFirstOccurrencesInOrder(
            program,
            "builder.Services.AddSingleton<PromptToTaskUntrainedHonestyExecutionService>();",
            "app = builder.Build();",
            "new StartupReadinessState(options.ReadyFile, persistenceOwner.IsEnabled)",
            "app.Services.GetRequiredService<PromptToTaskUntrainedHonestyExecutionService>()",
            "registryOwner.Restore(",
            "await app.StartAsync();",
            "readiness.PublishReady(app.Urls.Single())");

        var projectReferences = Regex.Matches(
                hostProject,
                "ProjectReference Include=\"(?<path>[^\"]+)\"",
                RegexOptions.CultureInvariant)
            .Cast<Match>()
            .Select(match => match.Groups["path"].Value)
            .ToArray();
        Assert.Equal(
            [
                @"..\..\15.OrchestratorModel\OrchestratorModel.csproj",
                @"..\..\19.DynamicGraphComposition.Contracts\19.DynamicGraphComposition.Contracts.csproj",
                @"..\..\43.SmartSkillsDomain\43.SmartSkillsDomain.csproj",
            ],
            projectReferences);
        Assert.DoesNotContain("TrainingLoop", hostProject, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(@"\DPO\", hostProject, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            @"..\tools\DynamicGraphCompositionLiveLocalServer\PromptToTaskUntrainedHonestyExecutionService.cs",
            testsProject,
            StringComparison.Ordinal);
    }

    [Fact(Timeout = 180000)]
    public async Task ChildProcess_PromptToTaskUntrainedHonestyRoute_IsStrictBoundedAndLeakFree()
    {
        const string route = "/api/v1/dynamic-graph/l2-untrained-honesty-runs";
        const string rawPrompt = "Create a deterministic local task without training.";
        const string validBody =
            "{\"evaluation_id\":\"evaluation:live:001\",\"run_id\":\"run:live:001\"," +
            "\"prompt_id\":\"prompt:live:001\",\"raw_prompt\":\"" + rawPrompt +
            "\",\"user_language\":\"en\"}";
        var root = CreateTempRoot();
        var readyFile = Path.Combine(root, "ready.json");
        using var process = StartServer(
            root,
            [
                "--tenant-id", TenantId,
                "--ready-file", readyFile,
                "--max-request-body-bytes", "1024",
            ]);
        try
        {
            var baseUrl = await WaitForReadyAsync(process, readyFile);
            using var client = new HttpClient
            {
                BaseAddress = baseUrl,
                Timeout = TimeSpan.FromSeconds(20),
            };

            async Task<HttpResponseMessage> SendJsonAsync(
                string json,
                string? tenant,
                string requestId)
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, route)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json"),
                };
                if (tenant is not null)
                    request.Headers.Add("X-XIIGen-Tenant-Id", tenant);
                request.Headers.Add("X-Request-Id", requestId);
                return await client.SendAsync(request);
            }

            using var success = await SendJsonAsync(
                validBody,
                TenantId,
                "l2-untrained.success");
            var successText = await success.Content.ReadAsStringAsync();
            Assert.True(
                success.StatusCode == HttpStatusCode.OK,
                $"Expected L2 untrained honesty 200, got {(int)success.StatusCode}: {successText}");
            using var document = JsonDocument.Parse(successText);
            var response = document.RootElement;
            Assert.Equal(
                [
                    "request_id",
                    "status",
                    "evaluation_id",
                    "target",
                    "producer",
                    "measurement",
                    "direct_unit_invocation_count",
                    "boundaries",
                ],
                response.EnumerateObject().Select(property => property.Name));
            Assert.Equal("l2-untrained.success", response.GetProperty("request_id").GetString());
            Assert.Equal("measured", response.GetProperty("status").GetString());
            Assert.Equal("evaluation:live:001", response.GetProperty("evaluation_id").GetString());
            Assert.Equal(1, response.GetProperty("direct_unit_invocation_count").GetInt32());

            var target = response.GetProperty("target");
            Assert.Equal(
                ["target_id", "level", "mechanism_kind", "deterministic_final_authority"],
                target.EnumerateObject().Select(property => property.Name));
            Assert.Equal("prompt_to_task_intent_unit", target.GetProperty("target_id").GetString());
            Assert.Equal("L2LogicalUnit", target.GetProperty("level").GetString());
            Assert.Equal("LearningSubject", target.GetProperty("mechanism_kind").GetString());
            Assert.False(target.GetProperty("deterministic_final_authority").GetBoolean());

            var producer = response.GetProperty("producer");
            Assert.Equal(
                ["unit_id", "unit_role", "domain_tag", "version", "registration_order"],
                producer.EnumerateObject().Select(property => property.Name));
            Assert.Equal("prompt_to_task_intent_unit", producer.GetProperty("unit_id").GetString());
            Assert.Equal("intent_extraction", producer.GetProperty("unit_role").GetString());
            Assert.Equal(
                "SmartSkillsDomain.PromptToTask",
                producer.GetProperty("domain_tag").GetString());
            Assert.Equal("1.0.0", producer.GetProperty("version").GetString());
            Assert.Equal(1, producer.GetProperty("registration_order").GetInt64());

            var measurement = response.GetProperty("measurement");
            Assert.Equal(
                [
                    "runtime_reported_not_ready",
                    "has_no_trained_state_evidence",
                    "has_no_semantic_classification_evidence",
                    "meets_untrained_honesty_gate",
                ],
                measurement.EnumerateObject().Select(property => property.Name));
            Assert.All(
                measurement.EnumerateObject(),
                property => Assert.True(property.Value.GetBoolean(), property.Name));

            var boundaries = response.GetProperty("boundaries");
            Assert.Equal(
                [
                    "loopback_only",
                    "tenant_binding_is_authentication",
                    "persistent",
                    "executes_unit",
                    "measurement_only",
                    "performs_training",
                    "claims_trained_state",
                    "claims_semantic_quality",
                    "changes_authority",
                    "persists_result",
                ],
                boundaries.EnumerateObject().Select(property => property.Name));
            Assert.True(boundaries.GetProperty("loopback_only").GetBoolean());
            Assert.False(boundaries.GetProperty("tenant_binding_is_authentication").GetBoolean());
            Assert.False(boundaries.GetProperty("persistent").GetBoolean());
            Assert.True(boundaries.GetProperty("executes_unit").GetBoolean());
            Assert.True(boundaries.GetProperty("measurement_only").GetBoolean());
            Assert.False(boundaries.GetProperty("performs_training").GetBoolean());
            Assert.False(boundaries.GetProperty("claims_trained_state").GetBoolean());
            Assert.False(boundaries.GetProperty("claims_semantic_quality").GetBoolean());
            Assert.False(boundaries.GetProperty("changes_authority").GetBoolean());
            Assert.False(boundaries.GetProperty("persists_result").GetBoolean());

            Assert.DoesNotContain(rawPrompt, successText, StringComparison.Ordinal);
            foreach (var forbidden in new[]
                     {
                         "raw_prompt", "sha256:", "live-local:l2-untrained-honesty:v1",
                         "runtime_evidence", "inspected_type", "forbidden_shortcut",
                         "producer_type", "input_type", "output_type", "constraints",
                         "context_fields", "contract_evidence", "structured_task", "tenant_id",
                         "secret",
                     })
            {
                Assert.DoesNotContain(forbidden, successText, StringComparison.OrdinalIgnoreCase);
            }

            using (var wrongMediaRequest = new HttpRequestMessage(HttpMethod.Post, route)
            {
                Content = new StringContent(validBody, Encoding.UTF8, "text/plain"),
            })
            {
                wrongMediaRequest.Headers.Add("X-XIIGen-Tenant-Id", TenantId);
                wrongMediaRequest.Headers.Add("X-Request-Id", "l2-untrained.media");
                using var wrongMedia = await client.SendAsync(wrongMediaRequest);
                await AssertErrorEnvelopeAsync(
                    wrongMedia,
                    HttpStatusCode.UnsupportedMediaType,
                    "HOST_JSON_CONTENT_TYPE_REQUIRED",
                    "The request content type must be JSON.");
            }

            using (var missingTenant = await SendJsonAsync(
                       validBody,
                       tenant: null,
                       "l2-untrained.missing-tenant"))
            {
                Assert.Equal(HttpStatusCode.BadRequest, missingTenant.StatusCode);
                await AssertErrorCodeAsync(missingTenant, "HOST_TENANT_HEADER_INVALID");
            }
            using (var wrongTenant = await SendJsonAsync(
                       validBody,
                       "tenant-b",
                       "l2-untrained.wrong-tenant"))
            {
                Assert.Equal(HttpStatusCode.Forbidden, wrongTenant.StatusCode);
                await AssertErrorCodeAsync(wrongTenant, "HOST_TENANT_BINDING_MISMATCH");
            }

            foreach (var invalidBody in new[]
                     {
                         validBody[..^1] + ",\"unknown\":true}",
                         validBody.Replace("\"evaluation_id\"", "\"Evaluation_id\"", StringComparison.Ordinal),
                         validBody.Replace(
                             "\"run_id\":\"run:live:001\"",
                             "\"run_id\":\"run:live:001\",\"run_id\":\"run:live:002\"",
                             StringComparison.Ordinal),
                         "{",
                     })
            {
                using var invalid = await SendJsonAsync(
                    invalidBody,
                    TenantId,
                    $"l2-untrained.strict-{Guid.NewGuid():N}");
                Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
                await AssertErrorCodeAsync(
                    invalid,
                    "HOST_L2_UNTRAINED_HONESTY_BODY_INVALID");
            }

            using (var unsafeSemantic = await SendJsonAsync(
                       validBody.Replace(
                           "evaluation:live:001",
                           "unsafe evaluation",
                           StringComparison.Ordinal),
                       TenantId,
                       "l2-untrained.semantic"))
            {
                Assert.Equal(HttpStatusCode.BadRequest, unsafeSemantic.StatusCode);
                await AssertErrorCodeAsync(
                    unsafeSemantic,
                    "HOST_L2_UNTRAINED_HONESTY_REQUEST_INVALID");
            }

            using (var declaredOversize = await SendJsonAsync(
                       "{\"evaluation_id\":\"evaluation:large\",\"run_id\":\"run:large\"," +
                       "\"prompt_id\":\"prompt:large\",\"raw_prompt\":\"" +
                       new string('x', 2048) +
                       "\",\"user_language\":\"en\"}",
                       TenantId,
                       "l2-untrained.declared"))
            {
                Assert.Equal(HttpStatusCode.RequestEntityTooLarge, declaredOversize.StatusCode);
                await AssertErrorCodeAsync(
                    declaredOversize,
                    "HOST_REQUEST_BODY_LIMIT_EXCEEDED");
            }

            using (var tcp = new TcpClient())
            {
                await tcp.ConnectAsync(baseUrl.Host, baseUrl.Port);
                await using var stream = tcp.GetStream();
                var chunkedBody = Encoding.UTF8.GetBytes(
                    "{\"evaluation_id\":\"evaluation:chunked\",\"run_id\":\"run:chunked\"," +
                    "\"prompt_id\":\"prompt:chunked\",\"raw_prompt\":\"" +
                    new string('x', 2048) +
                    "\",\"user_language\":\"en\"}");
                var headers = string.Join(
                    "\r\n",
                    $"POST {route} HTTP/1.1",
                    $"Host: {baseUrl.Host}:{baseUrl.Port}",
                    "Transfer-Encoding: chunked",
                    "Content-Type: application/json",
                    $"X-XIIGen-Tenant-Id: {TenantId}",
                    "X-Request-Id: l2-untrained.chunked",
                    "Connection: close",
                    "",
                    "");
                await stream.WriteAsync(Encoding.ASCII.GetBytes(headers));
                await stream.WriteAsync(
                    Encoding.ASCII.GetBytes(chunkedBody.Length.ToString("x") + "\r\n"));
                await stream.WriteAsync(chunkedBody);
                await stream.WriteAsync(Encoding.ASCII.GetBytes("\r\n0\r\n\r\n"));
                await stream.FlushAsync();
                using var responseBuffer = new MemoryStream();
                await stream.CopyToAsync(responseBuffer);
                var raw = Encoding.UTF8.GetString(responseBuffer.ToArray());
                Assert.StartsWith("HTTP/1.1 413", raw, StringComparison.Ordinal);
                Assert.Contains("HOST_REQUEST_BODY_LIMIT_EXCEEDED", raw, StringComparison.Ordinal);
                Assert.DoesNotContain("BadHttpRequestException", raw, StringComparison.Ordinal);
            }

            using var readinessResponse =
                await client.GetAsync("/api/v1/dynamic-graph/readiness");
            Assert.Equal(HttpStatusCode.OK, readinessResponse.StatusCode);

            Stop(process);
            Assert.Equal("READY", (await process.StandardOutput.ReadToEndAsync()).Trim());
            Assert.Equal(string.Empty, await process.StandardError.ReadToEndAsync());
        }
        finally
        {
            Stop(process);
            Directory.Delete(root, recursive: true);
        }
    }

    private static Process StartServer(
        string tempRoot,
        string[] serverArguments,
        IReadOnlyDictionary<string, string>? environment = null,
        IReadOnlyDictionary<string, string>? inheritedEnvironmentForTest = null)
    {
        var repoRoot = FindRepoRoot();
#if DEBUG
        const string buildConfiguration = "Debug";
#else
        const string buildConfiguration = "Release";
#endif
        var dll = Path.Combine(
            repoRoot, "tools", "DynamicGraphCompositionLiveLocalServer", "bin", buildConfiguration, "net9.0",
            "DynamicGraphCompositionLiveLocalServer.dll");
        Assert.True(File.Exists(dll), $"Build the live-local server before running focused process tests: {dll}");
        var startInfo = new ProcessStartInfo
        {
            FileName = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet",
            WorkingDirectory = repoRoot,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add(dll);
        foreach (var argument in serverArguments) startInfo.ArgumentList.Add(argument);
        if (inheritedEnvironmentForTest is not null)
        {
            foreach (var pair in inheritedEnvironmentForTest)
                startInfo.Environment[pair.Key] = pair.Value;
        }
        var inheritedConfigurationKeys = startInfo.Environment.Keys
            .Where(key => key.StartsWith("XIIGEN_DYNAMIC_GRAPH_", StringComparison.OrdinalIgnoreCase)
                || string.Equals(key, "DOTNET_STARTUP_HOOKS", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        foreach (var key in inheritedConfigurationKeys) startInfo.Environment.Remove(key);
        if (environment is not null)
        {
            foreach (var pair in environment) startInfo.Environment[pair.Key] = pair.Value;
        }
        var process = Process.Start(startInfo)!;
        return process;
    }

    private static string BuildForbiddenAssemblyStartupHook(string hookRoot)
    {
        var output = Path.Combine(hookRoot, "out");
        Directory.CreateDirectory(hookRoot);
        File.WriteAllText(
            Path.Combine(hookRoot, "ForbiddenAssemblyStartupHook.csproj"),
            "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net9.0</TargetFramework></PropertyGroup></Project>");
        File.WriteAllText(
            Path.Combine(hookRoot, "StartupHook.cs"),
            "using System.Reflection; using System.Reflection.Emit; " +
            "public static class StartupHook { public static void Initialize() => " +
            "AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(\"DPO\"), AssemblyBuilderAccess.Run); }");

        var startInfo = new ProcessStartInfo
        {
            FileName = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet",
            WorkingDirectory = hookRoot,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add("build");
        startInfo.ArgumentList.Add("ForbiddenAssemblyStartupHook.csproj");
        startInfo.ArgumentList.Add("--nologo");
        startInfo.ArgumentList.Add("-v:q");
        startInfo.ArgumentList.Add("-o");
        startInfo.ArgumentList.Add(output);
        using var build = Process.Start(startInfo)!;
        build.WaitForExit(60_000);
        var stdout = build.StandardOutput.ReadToEnd();
        var stderr = build.StandardError.ReadToEnd();
        Assert.True(build.HasExited && build.ExitCode == 0, $"Startup hook build failed. stdout={stdout} stderr={stderr}");
        var hook = Path.Combine(output, "ForbiddenAssemblyStartupHook.dll");
        Assert.True(File.Exists(hook));
        return hook;
    }

    private static async Task<Uri> WaitForReadyAsync(
        Process process,
        string readyFile,
        bool expectedPersistent = false)
    {
        using var timeout = new CancellationTokenSource(StartupTimeout);
        Exception? transientReadFailure = null;
        while (true)
        {
            if (process.HasExited)
            {
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                throw new Xunit.Sdk.XunitException(
                    $"Server exited {process.ExitCode} before readiness. stdout={output} stderr={error}");
            }

            if (File.Exists(readyFile))
            {
                try
                {
                    using var document = JsonDocument.Parse(
                        await File.ReadAllTextAsync(readyFile, timeout.Token));
                    var root = document.RootElement;
                    Assert.Equal("ready", root.GetProperty("status").GetString());
                    Assert.True(root.GetProperty("loopback_only").GetBoolean());
                    Assert.False(root.GetProperty("tenant_binding_is_authentication").GetBoolean());
                    Assert.Equal(expectedPersistent, root.GetProperty("persistent").GetBoolean());
                    Assert.True(root.GetProperty("executes_assistant").GetBoolean());
                    Assert.Equal(!expectedPersistent, root.GetProperty("applies_graph").GetBoolean());
                    Assert.False(root.GetProperty("performs_training").GetBoolean());
                    Assert.Equal(
                        [
                            "GET /api/v1/dynamic-graph/health",
                            "GET /api/v1/dynamic-graph/readiness",
                            "POST /api/v1/dynamic-graph/l2-untrained-honesty-runs",
                            "POST /api/v1/dynamic-graph/sessions",
                            "POST /api/v1/dynamic-graph/catalog-sessions",
                            "GET /api/v1/dynamic-graph/sessions/{sessionId}",
                            "GET /api/v1/dynamic-graph/sessions/{sessionId}/topology",
                            "POST /api/v1/dynamic-graph/sessions/{sessionId}/selected-target-evidence",
                            "POST /api/v1/dynamic-graph/sessions/{sessionId}/selected-target-runs",
                            "POST /api/v1/dynamic-graph/sessions/{sessionId}/topology-mutations",
                            "POST /api/v1/dynamic-graph/sessions/{sessionId}/topology-runs",
                            "POST /api/v1/dynamic-graph/sessions/{sessionId}/runs"
                        ],
                        root.GetProperty("routes").EnumerateArray().Select(route => route.GetString()));
                    var baseUrl = new Uri(root.GetProperty("base_url").GetString()!);
                    Assert.True(IPAddress.IsLoopback(IPAddress.Parse(baseUrl.Host)));
                    Assert.NotEqual(0, baseUrl.Port);
                    return baseUrl;
                }
                catch (Exception exception) when (
                    (exception is IOException or JsonException)
                    && !timeout.IsCancellationRequested)
                {
                    transientReadFailure = exception;
                }
            }

            try
            {
                await Task.Delay(50, timeout.Token);
            }
            catch (OperationCanceledException) when (transientReadFailure is not null)
            {
                throw new IOException(
                    "The readiness file remained unreadable until the startup timeout.",
                    transientReadFailure);
            }
        }
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln")))
                return current.FullName;
            current = current.Parent;
        }
        throw new DirectoryNotFoundException("Repository root was not found.");
    }

    private static string CreateTempRoot()
    {
        var root = Path.Combine(
            Path.GetTempPath(), "dynamic-graph-live-local-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static string CreatePersistentTempRoot(string prefix)
    {
        var localApplicationData =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var baseDirectory = string.IsNullOrWhiteSpace(localApplicationData)
            ? Path.GetTempPath()
            : localApplicationData;
        var root = Path.Combine(
            baseDirectory,
            $"{prefix}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        if (OperatingSystem.IsWindows())
        {
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
            FileSystemAclExtensions.SetAccessControl(new DirectoryInfo(root), security);
        }
        else
        {
            File.SetUnixFileMode(
                root,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }

        return root;
    }

    private static DynamicGraphCompositionSessionRegistry.SessionEntry
        CreateCurrentTopologySession(
            CompositionEvidenceHttpResponse bootstrap,
            DynamicGraphLogicalOrchestratorSessionDescriptor descriptor)
        => new(
            "dgs_" + new string('a', 32),
            "opaque-tenant-index",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMinutes(1),
            bootstrap,
            new CurrentTopologyReadSession(descriptor),
            "opaque-idempotency-index",
            new string('A', 64),
            new object());

    private static void Stop(Process process)
    {
        if (!process.HasExited)
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit(10_000);
        }
    }

    private sealed class CurrentTopologyReadSession :
        IDynamicGraphLogicalOrchestratorRunSession,
        IDynamicGraphLogicalOrchestratorCompositionSession
    {
        private readonly DynamicGraphLogicalOrchestratorSessionDescriptor _descriptor;

        internal CurrentTopologyReadSession(
            DynamicGraphLogicalOrchestratorSessionDescriptor descriptor)
        {
            _descriptor = descriptor;
        }

        public DynamicGraphLogicalOrchestratorSessionDescriptor Descriptor
            => _descriptor with { };

        public OperationResult<DynamicGraphLogicalOrchestratorSessionResult> ReadCurrent()
            => OperationResult<DynamicGraphLogicalOrchestratorSessionResult>.Fail(
                "not used",
                "NOT_USED");

        public Task<OperationResult<DynamicGraphLogicalOrchestratorRunResult>> RunAsync(
            DynamicGraphLogicalOrchestratorRunRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                OperationResult<DynamicGraphLogicalOrchestratorRunResult>.Fail(
                    "not used",
                    "NOT_USED"));
    }

    private sealed class ReflectionInvocationSentinelException : Exception
    {
        internal const string FixedMessage = "REFLECTION_INVOCATION_SENTINEL";

        internal ReflectionInvocationSentinelException()
            : base(FixedMessage)
        {
        }
    }
}
