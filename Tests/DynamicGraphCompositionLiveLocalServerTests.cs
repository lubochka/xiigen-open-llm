// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AgentInfrastructure.Logical;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OrchestratorModel.Logical;

[CollectionDefinition(CollectionName, DisableParallelization = true)]
public sealed class DynamicGraphCompositionLiveLocalServerCollection
{
    public const string CollectionName = "Dynamic graph composition live-local server";
}

[Collection(DynamicGraphCompositionLiveLocalServerCollection.CollectionName)]
public sealed class DynamicGraphCompositionLiveLocalServerTests
{
    private const string TenantId = "tenant-a";
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromSeconds(90);

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
                Assert.False(boundary.GetProperty("applies_graph").GetBoolean());
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
        var persistentRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            $"xiigen-persistence-test-{Guid.NewGuid():N}");
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
                builder = WebApplication.CreateBuilder(new WebApplicationOptions
                {
                    Args = Array.Empty<string>(),
                    ApplicationName = typeof(Program).Assembly.FullName,
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
    public void ProjectBoundary_HasOnlyTheTwoApprovedDirectProjectReferences()
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
                "..\\..\\19.DynamicGraphComposition.Contracts\\19.DynamicGraphComposition.Contracts.csproj"
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
        Assert.DoesNotContain("TenantId,", registry, StringComparison.Ordinal);
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

    private static Process StartServer(
        string tempRoot,
        string[] serverArguments,
        IReadOnlyDictionary<string, string>? environment = null,
        IReadOnlyDictionary<string, string>? inheritedEnvironmentForTest = null)
    {
        var repoRoot = FindRepoRoot();
        var dll = Path.Combine(
            repoRoot, "tools", "DynamicGraphCompositionLiveLocalServer", "bin", "Debug", "net9.0",
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
        while (!File.Exists(readyFile))
        {
            if (process.HasExited)
            {
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                throw new Xunit.Sdk.XunitException(
                    $"Server exited {process.ExitCode} before readiness. stdout={output} stderr={error}");
            }
            await Task.Delay(50, timeout.Token);
        }

        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(readyFile, timeout.Token));
        var root = document.RootElement;
        Assert.Equal("ready", root.GetProperty("status").GetString());
        Assert.True(root.GetProperty("loopback_only").GetBoolean());
        Assert.False(root.GetProperty("tenant_binding_is_authentication").GetBoolean());
        Assert.Equal(expectedPersistent, root.GetProperty("persistent").GetBoolean());
        Assert.True(root.GetProperty("executes_assistant").GetBoolean());
        Assert.False(root.GetProperty("applies_graph").GetBoolean());
        Assert.False(root.GetProperty("performs_training").GetBoolean());
        var baseUrl = new Uri(root.GetProperty("base_url").GetString()!);
        Assert.True(IPAddress.IsLoopback(IPAddress.Parse(baseUrl.Host)));
        Assert.NotEqual(0, baseUrl.Port);
        return baseUrl;
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

    private static void Stop(Process process)
    {
        if (!process.HasExited)
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit(10_000);
        }
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
