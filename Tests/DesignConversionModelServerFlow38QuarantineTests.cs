// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Net;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32.SafeHandles;
using ModelServer.DesignConversion;

[Collection(DesignConversionLocalHostTestCollectionNames.CollectionName)]
public sealed class DesignConversionModelServerFlow38QuarantineTests
{
    private const string Token = "flow38-core-quarantine-test-token";
    private const string Tenant = "tenant-flow38-a";
    private const string Correlation = "correlation-flow38-a";
    private const string EventId = "flow38-feedback:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

    [Fact]
    public void Handler_SourceCategoryCanon_ContainsEightExactValues()
    {
        Assert.Equal(
            [
                "local",
                "human",
                "open_weight",
                "hosted",
                "deterministic_baseline",
                "self_judge",
                "client_unverified",
                "third_party"
            ],
            DesignConversionFlow38SourceCategoryCanon.CanonicalValues);
        Assert.Equal(
            DesignConversionFlow38SourceCategory.ClientUnverified,
            DesignConversionFlow38SourceCategoryCanon.ParseExact("client_unverified"));
        Assert.Equal(
            DesignConversionFlow38SourceCategory.ThirdParty,
            DesignConversionFlow38SourceCategoryCanon.ParseExact("third_party"));
    }

    [Fact]
    public async Task Handler_SourceCategoryCanon_OnlyClientOriginCategoriesPersistInQuarantineIntake()
    {
        var root = CreateTemporaryRoot();
        try
        {
            var handler = CreateHandler(root);
            var accepted = new[] { "client_unverified", "third_party" };
            var rejected = DesignConversionFlow38SourceCategoryCanon.CanonicalValues
                .Except(accepted, StringComparer.Ordinal)
                .ToArray();
            var rejectedEventIdSuffixes = new[] { 'c', 'd', 'e', 'f', '1', '2' };

            for (var index = 0; index < accepted.Length; index++)
            {
                var row = CreateRow() with
                {
                    EventId = FlowEventId((char)('a' + index)),
                    SourceCategory = accepted[index]
                };
                var response = await handler.HandleAsync(CreateRequest(row));

                Assert.Equal(202, response.StatusCode);
                using var document = JsonDocument.Parse(response.BodyJson);
                Assert.False(document.RootElement.GetProperty("dpoPairCreated").GetBoolean());
                Assert.False(document.RootElement.GetProperty("trainingTriggered").GetBoolean());
                Assert.False(document.RootElement.GetProperty("dpoTrainReady").GetBoolean());
            }

            for (var index = 0; index < rejected.Length; index++)
            {
                var row = CreateRow() with
                {
                    EventId = FlowEventId(rejectedEventIdSuffixes[index]),
                    SourceCategory = rejected[index]
                };
                var response = await handler.HandleAsync(CreateRequest(row));

                Assert.Equal(422, response.StatusCode);
                AssertErrorCode(response, "FLOW38_SOURCE_CATEGORY_INVALID");
            }

            Assert.Equal(accepted.Length, File.ReadAllLines(
                Path.Combine(root, JsonlDesignConversionFlow38QuarantineStore.FileName)).Length);
        }
        finally
        {
            DeleteTemporaryRoot(root);
        }
    }

    [Fact]
    public async Task Handler_SourceCategoryCanon_RejectsUnknownWrongCaseAndMissingWithoutPersistence()
    {
        var root = CreateTemporaryRoot();
        try
        {
            var handler = CreateHandler(root);
            foreach (var category in new[] { "Client_Unverified", "unknown", string.Empty, " " })
            {
                var response = await handler.HandleAsync(CreateRequest(CreateRow() with
                {
                    SourceCategory = category
                }));

                Assert.Equal(422, response.StatusCode);
                AssertErrorCode(response, "FLOW38_SOURCE_CATEGORY_INVALID");
            }

            var payload = JsonNode.Parse(JsonSerializer.Serialize(CreateRow(), JsonOptions))!.AsObject();
            payload.Remove("sourceCategory");
            var missing = await handler.HandleAsync(CreateRequest(CreateRow()) with
            {
                BodyJson = payload.ToJsonString(JsonOptions)
            });

            Assert.Equal(400, missing.StatusCode);
            AssertErrorCode(missing, "FLOW38_QUARANTINE_SCHEMA_INVALID");
            Assert.False(File.Exists(Path.Combine(root, JsonlDesignConversionFlow38QuarantineStore.FileName)));
        }
        finally
        {
            DeleteTemporaryRoot(root);
        }
    }

    [Fact]
    public async Task Handler_ClientOriginCannotBeWashedIntoTrainReadyCommonAcceptOrSemanticQuality()
    {
        var root = CreateTemporaryRoot();
        try
        {
            var handler = CreateHandler(root);
            var categories = new[] { "client_unverified", "third_party" };
            foreach (var category in categories)
            {
                foreach (var row in new[]
                         {
                             CreateRow() with
                             {
                                 EventId = FlowEventId('d'),
                                 SourceCategory = category,
                                 LegalVerdict = "ACCEPT",
                                 ProvenanceRefs = ["provenance://flow38/allowlisted-common-training"]
                             },
                             CreateRow() with
                             {
                                 EventId = FlowEventId('e'),
                                 SourceCategory = category,
                                 AllowedUse = "CommonGlobal",
                                 ProvenanceRefs = ["provenance://flow38/allowlisted-common-training"]
                             },
                             CreateRow() with
                             {
                                 EventId = FlowEventId('1'),
                                 SourceCategory = category,
                                 Route = "common",
                                 ProvenanceRefs = ["provenance://flow38/allowlisted-common-training"]
                             },
                             CreateRow() with
                             {
                                 EventId = FlowEventId('2'),
                                 SourceCategory = category,
                                 AllowedUse = "CommonGlobal",
                                 Route = "common",
                                 ProvenanceRefs = ["provenance://flow38/allowlisted-common-training"]
                             },
                             CreateRow() with
                             {
                                 EventId = FlowEventId('f'),
                                 SourceCategory = category,
                                 SemanticQualityClaimed = true,
                                 ProvenanceRefs = ["provenance://flow38/allowlisted-common-training"]
                             },
                             CreateRow() with
                             {
                                 EventId = FlowEventId('0'),
                                 SourceCategory = category,
                                 DpoTrainReady = true,
                                 ProvenanceRefs = ["provenance://flow38/allowlisted-common-training"]
                             }
                         })
                {
                    var response = await handler.HandleAsync(CreateRequest(row));

                    Assert.Equal(422, response.StatusCode);
                    AssertErrorCode(response, "FLOW38_QUARANTINE_INVARIANT_VIOLATION");
                }
            }

            Assert.False(File.Exists(Path.Combine(root, JsonlDesignConversionFlow38QuarantineStore.FileName)));
        }
        finally
        {
            DeleteTemporaryRoot(root);
        }
    }

    [Fact]
    public async Task Handler_ValidQuarantine_PersistsAndReplaysAcrossStoreRestart()
    {
        var root = CreateTemporaryRoot();
        try
        {
            var row = CreateRow();
            var firstHandler = new DesignConversionModelServerFlow38QuarantineHandler(
                new JsonlDesignConversionFlow38QuarantineStore(
                    root,
                    () => DateTimeOffset.UnixEpoch.AddMinutes(38)),
                Token);

            var created = await firstHandler.HandleAsync(CreateRequest(row));

            Assert.Equal(202, created.StatusCode);
            using (var document = JsonDocument.Parse(created.BodyJson))
            {
                var body = document.RootElement;
                Assert.Equal(EventId, body.GetProperty("eventId").GetString());
                Assert.Equal("QUARANTINE", body.GetProperty("legalVerdict").GetString());
                Assert.Equal("PersonalAdaptive", body.GetProperty("allowedUse").GetString());
                Assert.False(body.GetProperty("semanticQualityClaimed").GetBoolean());
                Assert.False(body.GetProperty("dpoTrainReady").GetBoolean());
                Assert.False(body.GetProperty("dpoPairCreated").GetBoolean());
                Assert.False(body.GetProperty("trainingTriggered").GetBoolean());
                Assert.True(body.GetProperty("quarantinePersisted").GetBoolean());
                Assert.True(body.GetProperty("created").GetBoolean());
                Assert.False(body.GetProperty("replayed").GetBoolean());
                Assert.StartsWith("flow38-quarantine://sha256:", body.GetProperty("recordRef").GetString(), StringComparison.Ordinal);
            }

            Assert.DoesNotContain(row.ChosenOutputRef, created.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(row.RejectedOutputRef, created.BodyJson, StringComparison.Ordinal);
            var recordsPath = Path.Combine(root, JsonlDesignConversionFlow38QuarantineStore.FileName);
            Assert.Single(await File.ReadAllLinesAsync(recordsPath));

            var restartedHandler = new DesignConversionModelServerFlow38QuarantineHandler(
                new JsonlDesignConversionFlow38QuarantineStore(root),
                Token);
            var replay = await restartedHandler.HandleAsync(CreateRequest(row));

            Assert.Equal(200, replay.StatusCode);
            using (var document = JsonDocument.Parse(replay.BodyJson))
            {
                Assert.False(document.RootElement.GetProperty("created").GetBoolean());
                Assert.True(document.RootElement.GetProperty("replayed").GetBoolean());
                Assert.False(document.RootElement.GetProperty("trainingTriggered").GetBoolean());
            }
            Assert.Single(await File.ReadAllLinesAsync(recordsPath));
        }
        finally
        {
            DeleteTemporaryRoot(root);
        }
    }

    [Fact]
    public async Task Handler_TrainReadyOrCommonMutation_IsRejectedWithoutPersistence()
    {
        var root = CreateTemporaryRoot();
        try
        {
            var handler = new DesignConversionModelServerFlow38QuarantineHandler(
                new JsonlDesignConversionFlow38QuarantineStore(root),
                Token);

            var trainReady = await handler.HandleAsync(CreateRequest(CreateRow() with
            {
                DpoTrainReady = true
            }));
            var common = await handler.HandleAsync(CreateRequest(CreateRow() with
            {
                AllowedUse = "CommonGlobal",
                Route = "common"
            }));

            Assert.Equal(422, trainReady.StatusCode);
            Assert.Equal(422, common.StatusCode);
            AssertErrorCode(trainReady, "FLOW38_QUARANTINE_INVARIANT_VIOLATION");
            AssertErrorCode(common, "FLOW38_QUARANTINE_INVARIANT_VIOLATION");
            Assert.False(File.Exists(Path.Combine(root, JsonlDesignConversionFlow38QuarantineStore.FileName)));
        }
        finally
        {
            DeleteTemporaryRoot(root);
        }
    }

    [Fact]
    public async Task Handler_EventReplayWithDifferentEvidence_ConflictsWithoutSecondRecord()
    {
        var root = CreateTemporaryRoot();
        try
        {
            var handler = new DesignConversionModelServerFlow38QuarantineHandler(
                new JsonlDesignConversionFlow38QuarantineStore(root),
                Token);
            var created = await handler.HandleAsync(CreateRequest(CreateRow()));
            var conflict = await handler.HandleAsync(CreateRequest(CreateRow() with
            {
                ChosenOutputHash = Digest('d')
            }));

            Assert.Equal(202, created.StatusCode);
            Assert.Equal(409, conflict.StatusCode);
            AssertErrorCode(conflict, "FLOW38_QUARANTINE_EVENT_CONFLICT");
            Assert.Single(await File.ReadAllLinesAsync(
                Path.Combine(root, JsonlDesignConversionFlow38QuarantineStore.FileName)));
        }
        finally
        {
            DeleteTemporaryRoot(root);
        }
    }

    [Fact]
    public async Task Handler_RawOutputPropertyOrTransportSpoof_IsRejectedWithoutPersistence()
    {
        var root = CreateTemporaryRoot();
        try
        {
            var row = CreateRow();
            var handler = new DesignConversionModelServerFlow38QuarantineHandler(
                new JsonlDesignConversionFlow38QuarantineStore(root),
                Token);
            var rawPayload = JsonNode.Parse(JsonSerializer.Serialize(row, JsonOptions))!.AsObject();
            rawPayload["chosenOutput"] = "private raw generated output";
            var rawRequest = CreateRequest(row) with { BodyJson = rawPayload.ToJsonString(JsonOptions) };
            var spoofedHeaders = new Dictionary<string, string>(CreateHeaders(), StringComparer.OrdinalIgnoreCase)
            {
                ["X-Tenant-Id"] = "tenant-spoof"
            };

            var raw = await handler.HandleAsync(rawRequest);
            var spoofed = await handler.HandleAsync(CreateRequest(row) with { Headers = spoofedHeaders });

            Assert.Equal(400, raw.StatusCode);
            AssertErrorCode(raw, "FLOW38_QUARANTINE_SCHEMA_INVALID");
            Assert.Equal(422, spoofed.StatusCode);
            AssertErrorCode(spoofed, "FLOW38_TRANSPORT_IDENTITY_MISMATCH");
            Assert.False(File.Exists(Path.Combine(root, JsonlDesignConversionFlow38QuarantineStore.FileName)));
        }
        finally
        {
            DeleteTemporaryRoot(root);
        }
    }

    [Fact]
    public async Task AspNetMapper_LoopbackRoute_PersistsBoundedAuthenticatedQuarantine()
    {
        var root = CreateTemporaryRoot();
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionFlow38Quarantine(
            new DesignConversionModelServerFlow38QuarantineHandler(
                new JsonlDesignConversionFlow38QuarantineStore(root),
                Token));
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        Assert.Equal(
            bool.TrueString,
            registered.Value!.BoundaryMetadata["flow38_quarantine_store_configured"]);

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = [] });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
        await using var app = builder.Build();
        app.MapDesignConversionModelServerFlow38Quarantine(registry);

        await app.StartAsync();
        try
        {
            using var client = new HttpClient
            {
                BaseAddress = ResolveStartedAddress(app),
                Timeout = TimeSpan.FromSeconds(5)
            };
            using var message = new HttpRequestMessage(
                HttpMethod.Post,
                DesignConversionModelServerFlow38QuarantineHandler.EndpointPath)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(CreateRow(), JsonOptions),
                    Encoding.UTF8,
                    "application/json")
            };
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            message.Headers.Add("X-Tenant-Id", Tenant);
            message.Headers.Add("X-Correlation-Id", Correlation);
            message.Headers.Add("X-Idempotency-Key", EventId);

            using var response = await client.SendAsync(message);
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            Assert.Equal(
                "design_conversion_flow38_quarantine",
                response.Headers.GetValues("x-design-conversion-asp-net-route-id").Single());
            Assert.Equal(
                bool.TrueString,
                response.Headers.GetValues("x-design-conversion-live-route-registered").Single());
            using var document = JsonDocument.Parse(body);
            Assert.True(document.RootElement.GetProperty("quarantinePersisted").GetBoolean());
            Assert.False(document.RootElement.GetProperty("trainingTriggered").GetBoolean());
            Assert.Equal(
                bool.TrueString,
                document.RootElement.GetProperty("boundaryMetadata").GetProperty("asp_net_route_verified").GetString());

            using var oversizedMessage = new HttpRequestMessage(
                HttpMethod.Post,
                DesignConversionModelServerFlow38QuarantineHandler.EndpointPath)
            {
                Content = new StringContent(
                    new string('x', DesignConversionModelServerFlow38QuarantineHandler.MaximumRequestBodyBytes + 1),
                    Encoding.UTF8,
                    "application/json")
            };
            oversizedMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            oversizedMessage.Headers.Add("X-Tenant-Id", Tenant);
            oversizedMessage.Headers.Add("X-Correlation-Id", Correlation);
            oversizedMessage.Headers.Add("X-Idempotency-Key", EventId);
            using var oversizedResponse = await client.SendAsync(oversizedMessage);
            var oversizedBody = await oversizedResponse.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.RequestEntityTooLarge, oversizedResponse.StatusCode);
            using var oversizedDocument = JsonDocument.Parse(oversizedBody);
            Assert.Equal(
                "REQUEST_BODY_TOO_LARGE",
                oversizedDocument.RootElement.GetProperty("errorCode").GetString());
            Assert.Single(await File.ReadAllLinesAsync(
                Path.Combine(root, JsonlDesignConversionFlow38QuarantineStore.FileName)));
        }
        finally
        {
            await app.StopAsync();
            DeleteTemporaryRoot(root);
        }
    }

    [Fact]
    public async Task AspNetMapper_ChunkedInvalidUtf8_ReturnsJsonInvalidWithoutPersistence()
    {
        var root = CreateTemporaryRoot();
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionFlow38Quarantine(
            new DesignConversionModelServerFlow38QuarantineHandler(
                new JsonlDesignConversionFlow38QuarantineStore(root),
                Token));
        Assert.True(registered.IsSuccess, registered.ErrorMessage);

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = [] });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
        await using var app = builder.Build();
        app.MapDesignConversionModelServerFlow38Quarantine(registry);
        await app.StartAsync();
        try
        {
            using var client = new HttpClient
            {
                BaseAddress = ResolveStartedAddress(app),
                Timeout = TimeSpan.FromSeconds(5)
            };
            var invalidUtf8 = Encoding.UTF8.GetBytes("{\"eventId\":\"")
                .Concat(new byte[] { 0xc3, 0x28 })
                .Concat(Encoding.UTF8.GetBytes("\"}"))
                .ToArray();
            using var message = new HttpRequestMessage(
                HttpMethod.Post,
                DesignConversionModelServerFlow38QuarantineHandler.EndpointPath)
            {
                Content = new ChunkedByteContent(invalidUtf8)
            };
            message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            Assert.Null(message.Content.Headers.ContentLength);
            message.Headers.TransferEncodingChunked = true;
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            message.Headers.Add("X-Tenant-Id", Tenant);
            message.Headers.Add("X-Correlation-Id", Correlation);
            message.Headers.Add("X-Idempotency-Key", EventId);

            using var response = await client.SendAsync(message);
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            using var document = JsonDocument.Parse(body);
            Assert.Equal("REQUEST_JSON_INVALID", document.RootElement.GetProperty("errorCode").GetString());
            Assert.False(document.RootElement.GetProperty("dpoPairCreated").GetBoolean());
            Assert.False(document.RootElement.GetProperty("dpoTrainReady").GetBoolean());
            Assert.False(document.RootElement.GetProperty("trainingTriggered").GetBoolean());

            using var oversizedMessage = new HttpRequestMessage(
                HttpMethod.Post,
                DesignConversionModelServerFlow38QuarantineHandler.EndpointPath)
            {
                Content = new ChunkedByteContent(
                    Enumerable.Repeat(
                            (byte)'x',
                            DesignConversionModelServerFlow38QuarantineHandler.MaximumRequestBodyBytes + 1)
                        .ToArray())
            };
            oversizedMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            Assert.Null(oversizedMessage.Content.Headers.ContentLength);
            oversizedMessage.Headers.TransferEncodingChunked = true;
            oversizedMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            oversizedMessage.Headers.Add("X-Tenant-Id", Tenant);
            oversizedMessage.Headers.Add("X-Correlation-Id", Correlation);
            oversizedMessage.Headers.Add("X-Idempotency-Key", EventId);
            using var oversizedResponse = await client.SendAsync(oversizedMessage);
            var oversizedBody = await oversizedResponse.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.RequestEntityTooLarge, oversizedResponse.StatusCode);
            using var oversizedDocument = JsonDocument.Parse(oversizedBody);
            Assert.Equal(
                "REQUEST_BODY_TOO_LARGE",
                oversizedDocument.RootElement.GetProperty("errorCode").GetString());
            Assert.False(File.Exists(Path.Combine(root, JsonlDesignConversionFlow38QuarantineStore.FileName)));
        }
        finally
        {
            await app.StopAsync();
            DeleteTemporaryRoot(root);
        }
    }

    [Fact]
    public async Task Handler_MissingOrWrongAuthenticationAndUnconfiguredStore_FailClosed()
    {
        var root = CreateTemporaryRoot();
        try
        {
            var row = CreateRow();
            var handler = new DesignConversionModelServerFlow38QuarantineHandler(
                new JsonlDesignConversionFlow38QuarantineStore(root),
                Token);
            var missingHeaders = new Dictionary<string, string>(CreateHeaders(row), StringComparer.OrdinalIgnoreCase);
            missingHeaders.Remove("Authorization");
            var wrongHeaders = new Dictionary<string, string>(CreateHeaders(row), StringComparer.OrdinalIgnoreCase)
            {
                ["Authorization"] = "Bearer wrong-token"
            };

            var missing = await handler.HandleAsync(CreateRequest(row) with { Headers = missingHeaders });
            var wrong = await handler.HandleAsync(CreateRequest(row) with { Headers = wrongHeaders });
            var unconfigured = await new DesignConversionModelServerFlow38QuarantineHandler(null, null)
                .HandleAsync(CreateRequest(row));

            Assert.Equal(401, missing.StatusCode);
            AssertErrorCode(missing, "FLOW38_QUARANTINE_AUTHENTICATION_FAILED");
            Assert.Equal(401, wrong.StatusCode);
            AssertErrorCode(wrong, "FLOW38_QUARANTINE_AUTHENTICATION_FAILED");
            Assert.Equal(503, unconfigured.StatusCode);
            AssertErrorCode(unconfigured, "FLOW38_QUARANTINE_INTAKE_NOT_CONFIGURED");
            Assert.False(File.Exists(Path.Combine(root, JsonlDesignConversionFlow38QuarantineStore.FileName)));
        }
        finally
        {
            DeleteTemporaryRoot(root);
        }
    }

    [Fact]
    public async Task Handler_MissingDuplicateOrEscapedDuplicateRequestProperties_AreRejected()
    {
        var root = CreateTemporaryRoot();
        try
        {
            var row = CreateRow();
            var handler = new DesignConversionModelServerFlow38QuarantineHandler(
                new JsonlDesignConversionFlow38QuarantineStore(root),
                Token);
            var valid = JsonSerializer.Serialize(row, JsonOptions);
            var missingNode = JsonNode.Parse(valid)!.AsObject();
            missingNode.Remove("route");
            var payloads = new[]
            {
                missingNode.ToJsonString(JsonOptions),
                "{\"route\":\"adaptive_or_quarantine\"," + valid[1..],
                "{\"r\\u006fute\":\"adaptive_or_quarantine\"," + valid[1..]
            };

            foreach (var payload in payloads)
            {
                var response = await handler.HandleAsync(CreateRequest(row) with { BodyJson = payload });
                Assert.Equal(400, response.StatusCode);
                AssertErrorCode(response, "FLOW38_QUARANTINE_SCHEMA_INVALID");
            }

            Assert.False(File.Exists(Path.Combine(root, JsonlDesignConversionFlow38QuarantineStore.FileName)));
        }
        finally
        {
            DeleteTemporaryRoot(root);
        }
    }

    [Fact]
    public async Task Store_IncompleteTerminalFramesAndBoundedParserCorruption_FailClosedWithoutMutation()
    {
        var root = CreateTemporaryRoot();
        try
        {
            var seeded = await SeedValidStoreAsync(root);
            var recordsPath = Path.Combine(root, JsonlDesignConversionFlow38QuarantineStore.FileName);
            var truncatedSuffix = Encoding.UTF8.GetBytes("{\"schemaVersion\":");
            var truncated = new byte[seeded.Length + truncatedSuffix.Length];
            seeded.CopyTo(truncated, 0);
            truncatedSuffix.CopyTo(truncated, seeded.Length);
            var oversized = Enumerable.Repeat(
                    (byte)'x',
                    JsonlDesignConversionFlow38QuarantineStore.MaximumPersistedLineBytes + 1)
                .Append((byte)'\n')
                .ToArray();
            var corruptions = new[]
            {
                seeded[..^1],
                truncated,
                oversized,
                new byte[] { 0xc3, 0x28, (byte)'\n' },
                Encoding.UTF8.GetBytes(" \t\r\n"),
                Encoding.UTF8.GetBytes("{\n")
            };

            foreach (var corruption in corruptions)
            {
                await File.WriteAllBytesAsync(recordsPath, corruption);
                var response = await CreateHandler(root).HandleAsync(CreateRequest(CreateRow()));
                Assert.Equal(503, response.StatusCode);
                AssertErrorCode(response, "FLOW38_QUARANTINE_PERSISTENCE_UNAVAILABLE");
                Assert.Equal(corruption, await File.ReadAllBytesAsync(recordsPath));
            }
        }
        finally
        {
            DeleteTemporaryRoot(root);
        }
    }

    [Fact]
    public async Task Store_PersistedEnvelopeAndSourceSchemaCorruption_FailsClosed()
    {
        var root = CreateTemporaryRoot();
        try
        {
            var seeded = await SeedValidStoreAsync(root);
            var recordsPath = Path.Combine(root, JsonlDesignConversionFlow38QuarantineStore.FileName);
            var validJson = Encoding.UTF8.GetString(seeded.AsSpan(0, seeded.Length - 1));

            var unknownOuter = JsonNode.Parse(validJson)!.AsObject();
            unknownOuter["unexpectedOuter"] = true;
            var missingOuter = JsonNode.Parse(validJson)!.AsObject();
            missingOuter.Remove("fingerprint");
            var unknownSource = JsonNode.Parse(validJson)!.AsObject();
            unknownSource["source"]!.AsObject()["unexpectedSource"] = true;
            var missingSource = JsonNode.Parse(validJson)!.AsObject();
            missingSource["source"]!.AsObject().Remove("retrievalId");
            const string sourceMarker = "\"source\":{";
            Assert.Contains(sourceMarker, validJson, StringComparison.Ordinal);

            var corruptJson = new[]
            {
                unknownOuter.ToJsonString(JsonOptions),
                missingOuter.ToJsonString(JsonOptions),
                "{\"eventId\":\"duplicate\"," + validJson[1..],
                "{\"event\\u0049d\":\"escaped-duplicate\"," + validJson[1..],
                unknownSource.ToJsonString(JsonOptions),
                missingSource.ToJsonString(JsonOptions),
                validJson.Replace(sourceMarker, sourceMarker + "\"eventId\":\"duplicate\",", StringComparison.Ordinal),
                validJson.Replace(sourceMarker, sourceMarker + "\"event\\u0049d\":\"escaped-duplicate\",", StringComparison.Ordinal)
            };

            foreach (var json in corruptJson)
            {
                var corruption = Encoding.UTF8.GetBytes(json + "\n");
                await File.WriteAllBytesAsync(recordsPath, corruption);
                var response = await CreateHandler(root).HandleAsync(CreateRequest(CreateRow()));
                Assert.Equal(503, response.StatusCode);
                AssertErrorCode(response, "FLOW38_QUARANTINE_PERSISTENCE_UNAVAILABLE");
                Assert.Equal(corruption, await File.ReadAllBytesAsync(recordsPath));
            }
        }
        finally
        {
            DeleteTemporaryRoot(root);
        }
    }

    [Fact]
    public async Task Store_TotalByteAndRecordCapacity_RefuseAppendWithoutMutation()
    {
        var byteRoot = CreateTemporaryRoot();
        var recordRoot = CreateTemporaryRoot();
        try
        {
            var seeded = await SeedValidStoreAsync(byteRoot);
            var bytePath = Path.Combine(byteRoot, JsonlDesignConversionFlow38QuarantineStore.FileName);
            var scanByteLimited = CreateHandler(
                byteRoot,
                new DesignConversionFlow38QuarantineStoreOptions
                {
                    MaximumStoreBytes = seeded.LongLength - 1,
                    MaximumRecordCount = DesignConversionFlow38QuarantineStoreOptions.DefaultMaximumRecordCount
                });
            var scanByteResponse = await scanByteLimited.HandleAsync(CreateRequest(CreateRow()));
            Assert.Equal(503, scanByteResponse.StatusCode);
            AssertErrorCode(scanByteResponse, "FLOW38_QUARANTINE_PERSISTENCE_UNAVAILABLE");
            Assert.Equal(seeded, await File.ReadAllBytesAsync(bytePath));

            var byteLimited = CreateHandler(
                byteRoot,
                new DesignConversionFlow38QuarantineStoreOptions
                {
                    MaximumStoreBytes = seeded.LongLength,
                    MaximumRecordCount = DesignConversionFlow38QuarantineStoreOptions.DefaultMaximumRecordCount
                });
            var secondRow = CreateRow() with { EventId = FlowEventId('b') };

            var byteResponse = await byteLimited.HandleAsync(CreateRequest(secondRow));

            Assert.Equal(503, byteResponse.StatusCode);
            AssertErrorCode(byteResponse, "FLOW38_QUARANTINE_PERSISTENCE_UNAVAILABLE");
            Assert.Equal(seeded, await File.ReadAllBytesAsync(bytePath));

            var recordLimited = CreateHandler(
                recordRoot,
                new DesignConversionFlow38QuarantineStoreOptions
                {
                    MaximumStoreBytes = DesignConversionFlow38QuarantineStoreOptions.DefaultMaximumStoreBytes,
                    MaximumRecordCount = 1
                });
            var first = await recordLimited.HandleAsync(CreateRequest(CreateRow()));
            var beforeRejectedAppend = await File.ReadAllBytesAsync(
                Path.Combine(recordRoot, JsonlDesignConversionFlow38QuarantineStore.FileName));
            var second = await recordLimited.HandleAsync(CreateRequest(secondRow));

            Assert.Equal(202, first.StatusCode);
            Assert.Equal(503, second.StatusCode);
            AssertErrorCode(second, "FLOW38_QUARANTINE_PERSISTENCE_UNAVAILABLE");
            Assert.Equal(
                beforeRejectedAppend,
                await File.ReadAllBytesAsync(Path.Combine(recordRoot, JsonlDesignConversionFlow38QuarantineStore.FileName)));

            var createdUnderDefaultCapacity = await CreateHandler(recordRoot).HandleAsync(CreateRequest(secondRow));
            Assert.Equal(202, createdUnderDefaultCapacity.StatusCode);
            var twoRecordBytes = await File.ReadAllBytesAsync(
                Path.Combine(recordRoot, JsonlDesignConversionFlow38QuarantineStore.FileName));
            var scanRecordLimited = CreateHandler(
                recordRoot,
                new DesignConversionFlow38QuarantineStoreOptions
                {
                    MaximumStoreBytes = DesignConversionFlow38QuarantineStoreOptions.DefaultMaximumStoreBytes,
                    MaximumRecordCount = 1
                });
            var scanRecordResponse = await scanRecordLimited.HandleAsync(CreateRequest(CreateRow()));
            Assert.Equal(503, scanRecordResponse.StatusCode);
            AssertErrorCode(scanRecordResponse, "FLOW38_QUARANTINE_PERSISTENCE_UNAVAILABLE");
            Assert.Equal(
                twoRecordBytes,
                await File.ReadAllBytesAsync(Path.Combine(recordRoot, JsonlDesignConversionFlow38QuarantineStore.FileName)));
        }
        finally
        {
            DeleteTemporaryRoot(byteRoot);
            DeleteTemporaryRoot(recordRoot);
        }
    }

    [Fact]
    public void Store_RecordCounterRejectsIntMaxCapacityPlusOneWithoutOverflow()
    {
        Assert.Equal(
            int.MaxValue,
            JsonlDesignConversionFlow38QuarantineStore.IncrementRecordCount(int.MaxValue - 1, int.MaxValue));
        Assert.Throws<InvalidDataException>(() =>
            JsonlDesignConversionFlow38QuarantineStore.IncrementRecordCount(int.MaxValue, int.MaxValue));
    }

    [Fact]
    public void Store_UnixNativeFlagProfilesMatchLinuxAndDarwin()
    {
        var linux = JsonlDesignConversionFlow38QuarantineStore.UnixNativeFlagsFor(
            DesignConversionFlow38StoragePlatform.Linux);
        Assert.Equal(0, linux.OpenReadOnly);
        Assert.Equal(0x0002, linux.OpenReadWrite);
        Assert.Equal(0x0040, linux.OpenCreate);
        Assert.Equal(0x0080, linux.OpenExclusive);
        Assert.Equal(0x10000, linux.OpenDirectory);
        Assert.Equal(0x20000, linux.OpenNoFollow);
        Assert.Equal(0x80000, linux.OpenCloseOnExec);
        Assert.Equal(0xB0000, linux.DirectoryOpenFlags);
        Assert.Equal(0xA0002, linux.FileOpenFlags);
        Assert.Equal(0x06, linux.ExclusiveNonBlockingLock);

        var macOs = JsonlDesignConversionFlow38QuarantineStore.UnixNativeFlagsFor(
            DesignConversionFlow38StoragePlatform.MacOS);
        Assert.Equal(0, macOs.OpenReadOnly);
        Assert.Equal(0x0002, macOs.OpenReadWrite);
        Assert.Equal(0x0200, macOs.OpenCreate);
        Assert.Equal(0x0800, macOs.OpenExclusive);
        Assert.Equal(0x100000, macOs.OpenDirectory);
        Assert.Equal(0x0100, macOs.OpenNoFollow);
        Assert.Equal(0x01000000, macOs.OpenCloseOnExec);
        Assert.Equal(0x01100100, macOs.DirectoryOpenFlags);
        Assert.Equal(0x01000102, macOs.FileOpenFlags);
        Assert.Equal(0x06, macOs.ExclusiveNonBlockingLock);

        Assert.NotEqual(linux.OpenCreate, macOs.OpenCreate);
        Assert.NotEqual(linux.OpenExclusive, macOs.OpenExclusive);
        Assert.Throws<PlatformNotSupportedException>(() =>
            JsonlDesignConversionFlow38QuarantineStore.UnixNativeFlagsFor(
                DesignConversionFlow38StoragePlatform.Unsupported));
        Assert.Throws<PlatformNotSupportedException>(() =>
            JsonlDesignConversionFlow38QuarantineStore.UnixNativeFlagsFor(
                DesignConversionFlow38StoragePlatform.Windows));
    }

    [Fact]
    public async Task Handler_UnsupportedStoragePlatformReturnsStable503WithoutPersistence()
    {
        var root = CreateTemporaryRoot();
        try
        {
            var response = await CreateHandler(
                    root,
                    new DesignConversionFlow38QuarantineStoreOptions
                    {
                        StoragePlatformForTesting = DesignConversionFlow38StoragePlatform.Unsupported
                    })
                .HandleAsync(CreateRequest(CreateRow()));

            Assert.Equal(503, response.StatusCode);
            AssertErrorCode(response, "FLOW38_QUARANTINE_PERSISTENCE_UNAVAILABLE");
            using var document = JsonDocument.Parse(response.BodyJson);
            Assert.Equal(
                bool.FalseString,
                document.RootElement
                    .GetProperty("boundaryMetadata")
                    .GetProperty("flow38_quarantine_persisted")
                    .GetString());
            Assert.False(Directory.Exists(root));
        }
        finally
        {
            DeleteTemporaryRoot(root);
        }
    }

    [Fact]
    public async Task Store_MutableProvenanceIsSnapshottedBeforeWaitingForFileLock()
    {
        var root = CreateTemporaryRoot();
        try
        {
            Directory.CreateDirectory(root);
            var mutableProvenance = new List<string> { "provenance://flow38/original" };
            var row = CreateRow() with { ProvenanceRefs = mutableProvenance };
            var store = new JsonlDesignConversionFlow38QuarantineStore(root);
            var lockPath = Path.Combine(root, JsonlDesignConversionFlow38QuarantineStore.LockFileName);
            await using var blockingLock = new FileStream(
                lockPath,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.None);

            var admission = store.AdmitAsync(row);
            Assert.False(admission.IsCompleted);
            mutableProvenance[0] = "provenance://flow38/mutated-after-admission";
            await blockingLock.DisposeAsync();
            var result = await admission;

            Assert.True(result.Created);
            var recordsPath = Path.Combine(root, JsonlDesignConversionFlow38QuarantineStore.FileName);
            using (var document = JsonDocument.Parse((await File.ReadAllLinesAsync(recordsPath)).Single()))
            {
                Assert.Equal(
                    "provenance://flow38/original",
                    document.RootElement.GetProperty("source").GetProperty("provenanceRefs")[0].GetString());
                Assert.Equal(result.Fingerprint, document.RootElement.GetProperty("fingerprint").GetString());
            }

            var originalRow = CreateRow() with
            {
                ProvenanceRefs = new List<string> { "provenance://flow38/original" }
            };
            var replay = await new JsonlDesignConversionFlow38QuarantineStore(root).AdmitAsync(originalRow);
            Assert.True(replay.Replayed);
            Assert.Equal(result.Fingerprint, replay.Fingerprint);
        }
        finally
        {
            DeleteTemporaryRoot(root);
        }
    }

    [Fact]
    public async Task Store_ReparseRootIsRejectedWithoutOutsideWrite_WhenSymlinkCreationIsSupported()
    {
        var container = CreateTemporaryRoot();
        var outside = CreateTemporaryRoot();
        var linkedRoot = Path.Combine(container, "linked-root");
        Directory.CreateDirectory(container);
        Directory.CreateDirectory(outside);
        try
        {
            CreateDirectoryLinkOrFail(linkedRoot, outside);

            var response = await CreateHandler(linkedRoot).HandleAsync(CreateRequest(CreateRow()));

            Assert.Equal(503, response.StatusCode);
            AssertErrorCode(response, "FLOW38_QUARANTINE_PERSISTENCE_UNAVAILABLE");
            Assert.Empty(Directory.EnumerateFileSystemEntries(outside));
        }
        finally
        {
            RemoveDirectoryLinkIfPresent(linkedRoot);
            DeleteTemporaryRoot(container);
            DeleteTemporaryRoot(outside);
        }
    }

    [Fact]
    public async Task Store_RecordAndLockReparseFilesAreRejectedWithoutOutsideWrite_WhenSymlinkCreationIsSupported()
    {
        var container = CreateTemporaryRoot();
        var outside = CreateTemporaryRoot();
        Directory.CreateDirectory(container);
        Directory.CreateDirectory(outside);
        var sentinel = Encoding.UTF8.GetBytes("outside-must-remain-unchanged");
        try
        {
            foreach (var leafName in new[]
                     {
                         JsonlDesignConversionFlow38QuarantineStore.FileName,
                         JsonlDesignConversionFlow38QuarantineStore.LockFileName
                     })
            {
                var root = Path.Combine(container, Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(root);
                var outsideTarget = Path.Combine(outside, Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(outsideTarget);
                var sentinelPath = Path.Combine(outsideTarget, "sentinel.txt");
                await File.WriteAllBytesAsync(sentinelPath, sentinel);
                var linkedLeaf = Path.Combine(root, leafName);
                CreateDirectoryLinkOrFail(linkedLeaf, outsideTarget);

                var response = await CreateHandler(root).HandleAsync(CreateRequest(CreateRow()));

                Assert.Equal(503, response.StatusCode);
                AssertErrorCode(response, "FLOW38_QUARANTINE_PERSISTENCE_UNAVAILABLE");
                Assert.Equal(sentinel, await File.ReadAllBytesAsync(sentinelPath));
                RemoveDirectoryLinkIfPresent(linkedLeaf);
            }
        }
        finally
        {
            DeleteTemporaryRoot(container);
            DeleteTemporaryRoot(outside);
        }
    }

    [Fact]
    public async Task Store_DanglingRecordAndLockLinksFailWithoutCreatingOutsideTargets()
    {
        var container = CreateTemporaryRoot();
        var outside = CreateTemporaryRoot();
        Directory.CreateDirectory(container);
        Directory.CreateDirectory(outside);
        try
        {
            foreach (var leafName in new[]
                     {
                         JsonlDesignConversionFlow38QuarantineStore.FileName,
                         JsonlDesignConversionFlow38QuarantineStore.LockFileName
                     })
            {
                var root = Path.Combine(container, Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(root);
                var outsideTarget = Path.Combine(outside, Guid.NewGuid().ToString("N") + ".missing");
                var linkedLeaf = Path.Combine(root, leafName);
                CreateDirectoryLinkOrFail(linkedLeaf, outsideTarget);

                var response = await CreateHandler(root).HandleAsync(CreateRequest(CreateRow()));

                Assert.Equal(503, response.StatusCode);
                AssertErrorCode(response, "FLOW38_QUARANTINE_PERSISTENCE_UNAVAILABLE");
                Assert.False(File.Exists(outsideTarget));
                Assert.False(Directory.Exists(outsideTarget));
                RemoveDirectoryLinkIfPresent(linkedLeaf);
            }
        }
        finally
        {
            DeleteTemporaryRoot(container);
            DeleteTemporaryRoot(outside);
        }
    }

    [Fact]
    public async Task Store_LeafSwapBetweenValidationAndSafeOpenNeverFollowsOutsideTarget()
    {
        var container = CreateTemporaryRoot();
        var outside = CreateTemporaryRoot();
        Directory.CreateDirectory(container);
        Directory.CreateDirectory(outside);
        var sentinel = Encoding.UTF8.GetBytes("outside-swap-target-must-remain-unchanged");
        try
        {
            var cases = new[]
            {
                (Stage: DesignConversionFlow38SafeOpenStage.LockFile, Seed: false, Append: false, Leaf: JsonlDesignConversionFlow38QuarantineStore.LockFileName),
                (Stage: DesignConversionFlow38SafeOpenStage.RecordRead, Seed: true, Append: false, Leaf: JsonlDesignConversionFlow38QuarantineStore.FileName),
                (Stage: DesignConversionFlow38SafeOpenStage.RecordAppend, Seed: true, Append: true, Leaf: JsonlDesignConversionFlow38QuarantineStore.FileName)
            };

            foreach (var testCase in cases)
            {
                var root = Path.Combine(container, Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(root);
                if (testCase.Seed)
                    await SeedValidStoreAsync(root);
                else
                    await File.WriteAllBytesAsync(Path.Combine(root, testCase.Leaf), []);

                var outsideTarget = Path.Combine(outside, Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(outsideTarget);
                var sentinelPath = Path.Combine(outsideTarget, "sentinel.txt");
                await File.WriteAllBytesAsync(sentinelPath, sentinel);
                var hookCount = 0;
                var options = new DesignConversionFlow38QuarantineStoreOptions
                {
                    BeforeSafeOpenForTesting = (stage, path) =>
                    {
                        if (stage != testCase.Stage || Interlocked.Exchange(ref hookCount, 1) != 0)
                            return;
                        File.Delete(path);
                        CreateDirectoryLinkOrFail(path, outsideTarget);
                    }
                };
                var requestRow = testCase.Append
                    ? CreateRow() with { EventId = FlowEventId('b') }
                    : CreateRow();

                var response = await CreateHandler(root, options).HandleAsync(CreateRequest(requestRow));

                Assert.Equal(1, hookCount);
                Assert.Equal(503, response.StatusCode);
                AssertErrorCode(response, "FLOW38_QUARANTINE_PERSISTENCE_UNAVAILABLE");
                Assert.Equal(sentinel, await File.ReadAllBytesAsync(sentinelPath));
                RemoveDirectoryLinkIfPresent(Path.Combine(root, testCase.Leaf));
            }
        }
        finally
        {
            DeleteTemporaryRoot(container);
            DeleteTemporaryRoot(outside);
        }
    }

    [Fact]
    public async Task Store_RootAndAncestorSwapBeforeSafeLeaseNeverRedirectsOutside()
    {
        var container = CreateTemporaryRoot();
        var outsideContainer = CreateTemporaryRoot();
        Directory.CreateDirectory(container);
        Directory.CreateDirectory(outsideContainer);
        try
        {
            foreach (var swapAncestor in new[] { false, true })
            {
                var caseRoot = Path.Combine(container, Guid.NewGuid().ToString("N"));
                var ancestor = Path.Combine(caseRoot, "ancestor");
                var storeRoot = Path.Combine(ancestor, "store");
                Directory.CreateDirectory(storeRoot);
                var outside = Path.Combine(outsideContainer, Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(outside);
                var swapPath = swapAncestor ? ancestor : storeRoot;
                var backupPath = swapPath + "-original";
                var hookCount = 0;
                var options = new DesignConversionFlow38QuarantineStoreOptions
                {
                    BeforeSafeOpenForTesting = (stage, _) =>
                    {
                        if (stage != DesignConversionFlow38SafeOpenStage.RootLease ||
                            Interlocked.Exchange(ref hookCount, 1) != 0)
                        {
                            return;
                        }

                        Directory.Move(swapPath, backupPath);
                        CreateDirectoryLinkOrFail(swapPath, outside);
                    }
                };

                var response = await CreateHandler(storeRoot, options).HandleAsync(CreateRequest(CreateRow()));

                Assert.Equal(1, hookCount);
                Assert.Equal(503, response.StatusCode);
                AssertErrorCode(response, "FLOW38_QUARANTINE_PERSISTENCE_UNAVAILABLE");
                Assert.Empty(Directory.EnumerateFileSystemEntries(outside));
                RemoveDirectoryLinkIfPresent(swapPath);
            }
        }
        finally
        {
            DeleteTemporaryRoot(container);
            DeleteTemporaryRoot(outsideContainer);
        }
    }

    [Fact]
    public async Task Store_RootMutationAfterSafeLeaseNeverRedirectsOutside()
    {
        var container = CreateTemporaryRoot();
        var outside = CreateTemporaryRoot();
        var root = Path.Combine(container, "leased-root");
        var movedRoot = root + "-original";
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(outside);
        var hookCount = 0;
        var mutationApplied = false;
        try
        {
            var options = new DesignConversionFlow38QuarantineStoreOptions
            {
                BeforeSafeOpenForTesting = (stage, _) =>
                {
                    if (stage != DesignConversionFlow38SafeOpenStage.LockFile ||
                        Interlocked.Exchange(ref hookCount, 1) != 0)
                    {
                        return;
                    }

                    if (OperatingSystem.IsWindows())
                    {
                        CreateDirectoryLinkOrFail(root, outside);
                        mutationApplied = true;
                    }
                    else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                    {
                        Directory.Move(root, movedRoot);
                        Directory.CreateSymbolicLink(root, outside);
                        mutationApplied = true;
                    }
                    else
                    {
                        throw new PlatformNotSupportedException("The FLOW-38 root mutation test requires Windows, Linux, or macOS.");
                    }
                }
            };

            var response = await CreateHandler(root, options).HandleAsync(CreateRequest(CreateRow()));

            Assert.Equal(1, hookCount);
            Assert.Empty(Directory.EnumerateFileSystemEntries(outside));
            if (OperatingSystem.IsWindows())
            {
                Assert.True(mutationApplied);
                Assert.Equal(503, response.StatusCode);
                AssertErrorCode(response, "FLOW38_QUARANTINE_PERSISTENCE_UNAVAILABLE");
            }
            else
            {
                Assert.True(mutationApplied);
                Assert.Equal(202, response.StatusCode);
                Assert.True(File.Exists(Path.Combine(movedRoot, JsonlDesignConversionFlow38QuarantineStore.FileName)));
            }
        }
        finally
        {
            if (mutationApplied)
                RemoveDirectoryLinkIfPresent(root);
            DeleteTemporaryRoot(container);
            DeleteTemporaryRoot(outside);
        }
    }

    [Fact]
    public async Task Store_MissingRootSegmentCreationAfterParentLeaseNeverRedirectsOutside()
    {
        var container = CreateTemporaryRoot();
        var outside = CreateTemporaryRoot();
        var parent = Path.Combine(container, "leased-parent");
        var movedParent = parent + "-original";
        var missingChild = Path.Combine(parent, "missing-child");
        var storeRoot = Path.Combine(missingChild, "store");
        var outsideStore = Path.Combine(outside, "missing-child", "store");
        Directory.CreateDirectory(parent);
        Directory.CreateDirectory(outside);
        var sentinel = Encoding.UTF8.GetBytes("outside-parent-segment-sentinel");
        var sentinelPath = Path.Combine(outside, "sentinel.txt");
        await File.WriteAllBytesAsync(sentinelPath, sentinel);
        var hookCount = 0;
        var mutationApplied = false;
        try
        {
            var pathComparison = OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;
            var options = new DesignConversionFlow38QuarantineStoreOptions
            {
                BeforeSafeOpenForTesting = (stage, path) =>
                {
                    if (stage != DesignConversionFlow38SafeOpenStage.RootSegmentCreate ||
                        !string.Equals(path, missingChild, pathComparison) ||
                        Interlocked.Exchange(ref hookCount, 1) != 0)
                    {
                        return;
                    }

                    if (OperatingSystem.IsWindows())
                    {
                        CreateDirectoryLinkOrFail(parent, outside);
                        mutationApplied = true;
                    }
                    else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                    {
                        Directory.Move(parent, movedParent);
                        Directory.CreateSymbolicLink(parent, outside);
                        mutationApplied = true;
                    }
                    else
                    {
                        throw new PlatformNotSupportedException(
                            "The FLOW-38 root-segment creation test requires Windows, Linux, or macOS.");
                    }
                }
            };

            var response = await CreateHandler(storeRoot, options).HandleAsync(CreateRequest(CreateRow()));

            Assert.Equal(1, hookCount);
            Assert.True(mutationApplied);
            Assert.Equal(sentinel, await File.ReadAllBytesAsync(sentinelPath));
            Assert.False(Directory.Exists(Path.Combine(outside, "missing-child")));
            Assert.False(File.Exists(Path.Combine(
                outsideStore,
                JsonlDesignConversionFlow38QuarantineStore.FileName)));
            Assert.False(File.Exists(Path.Combine(
                outsideStore,
                JsonlDesignConversionFlow38QuarantineStore.LockFileName)));
            if (OperatingSystem.IsWindows())
            {
                Assert.Equal(503, response.StatusCode);
                AssertErrorCode(response, "FLOW38_QUARANTINE_PERSISTENCE_UNAVAILABLE");
            }
            else
            {
                Assert.Equal(202, response.StatusCode);
                Assert.True(File.Exists(Path.Combine(
                    movedParent,
                    "missing-child",
                    "store",
                    JsonlDesignConversionFlow38QuarantineStore.FileName)));
            }
        }
        finally
        {
            if (mutationApplied)
                RemoveDirectoryLinkIfPresent(parent);
            DeleteTemporaryRoot(container);
            DeleteTemporaryRoot(outside);
        }
    }

    [Fact]
    public async Task Store_DistinctInstancesConcurrentSameEvidence_CreateExactlyOneFrame()
    {
        var root = CreateTemporaryRoot();
        try
        {
            var row = CreateRow();
            var admissions = Enumerable.Range(0, 8)
                .Select(_ => new JsonlDesignConversionFlow38QuarantineStore(root).AdmitAsync(row))
                .ToArray();

            var results = await Task.WhenAll(admissions);

            Assert.Single(results, result => result.Created);
            Assert.Equal(7, results.Count(result => result.Replayed));
            Assert.DoesNotContain(results, result => result.Conflict);
            Assert.Single(await File.ReadAllLinesAsync(
                Path.Combine(root, JsonlDesignConversionFlow38QuarantineStore.FileName)));
        }
        finally
        {
            DeleteTemporaryRoot(root);
        }
    }

    [Fact]
    public async Task Store_DistinctInstancesConcurrentDifferentEvidence_CreateOneAndConflictOne()
    {
        var root = CreateTemporaryRoot();
        try
        {
            var row = CreateRow();
            var changed = row with { ChosenOutputHash = Digest('d') };
            var results = await Task.WhenAll(
                new JsonlDesignConversionFlow38QuarantineStore(root).AdmitAsync(row),
                new JsonlDesignConversionFlow38QuarantineStore(root).AdmitAsync(changed));

            Assert.Single(results, result => result.Created);
            Assert.Single(results, result => result.Conflict);
            Assert.DoesNotContain(results, result => result.Replayed);
            Assert.Single(await File.ReadAllLinesAsync(
                Path.Combine(root, JsonlDesignConversionFlow38QuarantineStore.FileName)));
        }
        finally
        {
            DeleteTemporaryRoot(root);
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = false,
        WriteIndented = false
    };

    private static DesignConversionModelServerFlow38QuarantineHandler CreateHandler(
        string root,
        DesignConversionFlow38QuarantineStoreOptions? options = null) =>
        new(
            new JsonlDesignConversionFlow38QuarantineStore(root, options: options),
            Token);

    private static async Task<byte[]> SeedValidStoreAsync(string root)
    {
        var response = await CreateHandler(root).HandleAsync(CreateRequest(CreateRow()));
        Assert.Equal(202, response.StatusCode);
        return await File.ReadAllBytesAsync(
            Path.Combine(root, JsonlDesignConversionFlow38QuarantineStore.FileName));
    }

    private static DesignConversionFlow38QuarantinedSourceRow CreateRow() => new(
        EventId,
        Correlation,
        Tenant,
        "cycle-flow38-a",
        "pattern-flow38-a",
        "retrieval-flow38-a",
        Digest('1'),
        "artifact://flow38/chosen-output-ref",
        Digest('2'),
        "artifact://flow38/rejected-output-ref",
        Digest('3'),
        "client_unverified",
        "terms://flow38/pending",
        "license://flow38/pending",
        ["provenance://flow38/client-submission"],
        "redaction://flow38/v1",
        "client_supplied_unverified",
        "client_supplied_unverified",
        "QUARANTINE",
        "PersonalAdaptive",
        "adaptive_or_quarantine",
        SemanticQualityClaimed: false,
        DpoTrainReady: false);

    private static DesignConversionModelServerHttpRequest CreateRequest(
        DesignConversionFlow38QuarantinedSourceRow row) => new(
        Method: "POST",
        Path: DesignConversionModelServerFlow38QuarantineHandler.EndpointPath,
        BodyJson: JsonSerializer.Serialize(row, JsonOptions),
        ContentType: "application/json; charset=utf-8",
        Headers: CreateHeaders(row));

    private static IReadOnlyDictionary<string, string> CreateHeaders(
        DesignConversionFlow38QuarantinedSourceRow? row = null)
    {
        row ??= CreateRow();
        return
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Authorization"] = $"Bearer {Token}",
            ["X-Tenant-Id"] = row.TenantScope,
            ["X-Correlation-Id"] = row.CorrelationId,
            ["X-Idempotency-Key"] = row.EventId
        };
    }

    private static string Digest(char value) => "sha256:" + new string(value, 64);

    private static string FlowEventId(char value) => "flow38-feedback:" + new string(value, 64);

    private static void AssertErrorCode(DesignConversionModelServerHttpResponse response, string expected)
    {
        using var document = JsonDocument.Parse(response.BodyJson);
        Assert.Equal(expected, document.RootElement.GetProperty("errorCode").GetString());
        Assert.False(document.RootElement.GetProperty("dpoTrainReady").GetBoolean());
        Assert.False(document.RootElement.GetProperty("dpoPairCreated").GetBoolean());
        Assert.False(document.RootElement.GetProperty("trainingTriggered").GetBoolean());
    }

    private static Uri ResolveStartedAddress(WebApplication app)
    {
        var address = app.Services
            .GetRequiredService<IServer>()
            .Features
            .Get<IServerAddressesFeature>()!
            .Addresses
            .Single();
        var uri = new Uri(address);
        Assert.True(IPAddress.IsLoopback(IPAddress.Parse(uri.Host)));
        return uri;
    }

    private static string CreateTemporaryRoot() =>
        Path.Combine(Path.GetTempPath(), "flow38-core-quarantine-tests", Guid.NewGuid().ToString("N"));

    private static void DeleteTemporaryRoot(string root)
    {
        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }

    private static void CreateDirectoryLinkOrFail(string linkPath, string targetPath)
    {
        try
        {
            Directory.CreateSymbolicLink(linkPath, targetPath);
        }
        catch (Exception exception) when (exception is IOException or NotSupportedException or PlatformNotSupportedException or UnauthorizedAccessException)
        {
            if (!OperatingSystem.IsWindows())
            {
                throw new InvalidOperationException(
                    $"Environment could not exercise the required directory-link safety branch: {exception.GetType().Name}: {exception.Message}",
                    exception);
            }

            CreateWindowsJunctionOrFail(linkPath, targetPath, exception);
        }
    }

    private static void CreateWindowsJunctionOrFail(string linkPath, string targetPath, Exception symbolicLinkFailure)
    {
        const uint genericWrite = 0x40000000;
        const uint openExisting = 3;
        const uint backupSemantics = 0x02000000;
        const uint openReparsePoint = 0x00200000;
        const uint setReparsePoint = 0x000900A4;
        const uint mountPointTag = 0xA0000003;

        var directoryAlreadyExisted = Directory.Exists(linkPath);
        Directory.CreateDirectory(linkPath);
        using var handle = CreateFileForJunction(
            linkPath,
            genericWrite,
            (uint)(FileShare.Read | FileShare.Write | FileShare.Delete),
            IntPtr.Zero,
            openExisting,
            backupSemantics | openReparsePoint,
            IntPtr.Zero);
        if (handle.IsInvalid)
        {
            var error = Marshal.GetLastPInvokeError();
            if (!directoryAlreadyExisted)
                Directory.Delete(linkPath);
            throw new InvalidOperationException(
                $"Environment denied both symbolic-link and junction creation (CreateFile Win32={error}).",
                symbolicLinkFailure);
        }

        var printName = Path.TrimEndingDirectorySeparator(Path.GetFullPath(targetPath));
        var substituteName = @"\??\" + printName;
        var substituteBytes = Encoding.Unicode.GetBytes(substituteName);
        var printBytes = Encoding.Unicode.GetBytes(printName);
        var pathBytes = substituteBytes.Length + sizeof(ushort) + printBytes.Length + sizeof(ushort);
        var reparseDataLength = checked((ushort)(8 + pathBytes));
        var buffer = new byte[8 + reparseDataLength];
        using (var memory = new MemoryStream(buffer, writable: true))
        using (var writer = new BinaryWriter(memory, Encoding.Unicode, leaveOpen: true))
        {
            writer.Write(mountPointTag);
            writer.Write(reparseDataLength);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write(checked((ushort)substituteBytes.Length));
            writer.Write(checked((ushort)(substituteBytes.Length + sizeof(ushort))));
            writer.Write(checked((ushort)printBytes.Length));
            writer.Write(substituteBytes);
            writer.Write((ushort)0);
            writer.Write(printBytes);
            writer.Write((ushort)0);
        }

        if (!DeviceIoControlForJunction(
                handle,
                setReparsePoint,
                buffer,
                buffer.Length,
                IntPtr.Zero,
                0,
                out _,
                IntPtr.Zero))
        {
            var error = Marshal.GetLastPInvokeError();
            handle.Dispose();
            if (!directoryAlreadyExisted)
                Directory.Delete(linkPath);
            throw new InvalidOperationException(
                $"Environment denied both symbolic-link and junction creation (FSCTL_SET_REPARSE_POINT Win32={error}).",
                symbolicLinkFailure);
        }
    }

    private static void RemoveDirectoryLinkIfPresent(string linkPath)
    {
        try
        {
            Directory.Delete(linkPath);
        }
        catch (DirectoryNotFoundException)
        {
            if (OperatingSystem.IsWindows())
                _ = RemoveDirectoryForJunction(linkPath);
        }
    }

    [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateFileForJunction(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    [DllImport("kernel32.dll", EntryPoint = "DeviceIoControl", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeviceIoControlForJunction(
        SafeFileHandle device,
        uint controlCode,
        byte[] inputBuffer,
        int inputBufferSize,
        IntPtr outputBuffer,
        int outputBufferSize,
        out int bytesReturned,
        IntPtr overlapped);

    [DllImport("kernel32.dll", EntryPoint = "RemoveDirectoryW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RemoveDirectoryForJunction(string pathName);

    private sealed class ChunkedByteContent(byte[] bytes) : HttpContent
    {
        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            await stream.WriteAsync(bytes);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }
}
