// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DesignConversionDomain;
using ModelServer.DesignConversion;
using DesignDomain = DesignConversionDomain.DesignConversionDomain;

public sealed class DesignConversionModelActivationRegistryTests
{
    private const string TenantA = "tenant-design-conversion-alpha";
    private const string TenantB = "tenant-design-conversion-beta";
    private const string LocalToken = "model-lifecycle-local-token";
    private const long ExpectedMaxStoreBytes = 16L * 1024 * 1024;
    private static readonly UTF8Encoding ReplacementUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: false);
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);
    private static readonly JsonSerializerOptions PromotionJournalJsonOptions = new(
        DesignConversionCheckpointJsonOptions.Standard)
    {
        WriteIndented = false
    };

    private sealed record StoredActivationOperation(
        string TenantIdRef,
        string OperationKeyRef,
        string OperationKind,
        DateTimeOffset CreatedAtUtc);

    [Fact]
    public async Task ActivatePublishedModel_FirstActivationStoresTenantScopedActiveVersion()
    {
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(root, TenantA, "first", batchCount: 1);
        var api = CreateApi(root);

        var result = await api.ActivatePublishedModelAsync(
            Auth(TenantA, "activate:first", expectedGeneration: 0),
            candidate,
            expectedModelVersion: candidate.Model.Version);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(result.Value!.Applied);
        Assert.False(result.Value.Replay);
        Assert.Equal(1, result.Value.Generation);
        Assert.Equal(candidate.Model.ModelId, result.Value.ActiveVersion.ModelId);
        Assert.Null(result.Value.PreviousVersion);
        Assert.False(result.Value.ProductReady);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["modelserver_model_lifecycle_internal_only"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["modelserver_model_lifecycle_public_http_route_registered"]);
        Assert.Equal(DesignConversionModelActivationRegistry.BuildTenantIdRef(TenantA), result.Value.TenantIdRef);
    }

    [Fact]
    public async Task ActivatePublishedModel_SecondActivationCapturesPreviousVersion()
    {
        var root = NewRoot();
        var first = await BuildActivationRecordAsync(root, TenantA, "first", batchCount: 1);
        var second = await BuildActivationRecordAsync(root, TenantA, "second", batchCount: 2);
        var api = CreateApi(root);

        var firstResult = await api.ActivatePublishedModelAsync(Auth(TenantA, "activate:first", 0), first, first.Model.Version);
        Assert.True(firstResult.IsSuccess, firstResult.ErrorMessage);

        var secondResult = await api.ActivatePublishedModelAsync(Auth(TenantA, "activate:second", 1), second, second.Model.Version);

        Assert.True(secondResult.IsSuccess, secondResult.ErrorMessage);
        Assert.Equal(2, secondResult.Value!.Generation);
        Assert.Equal(second.Model.ModelId, secondResult.Value.ActiveVersion.ModelId);
        Assert.NotNull(secondResult.Value.PreviousVersion);
        Assert.Equal(first.Model.ModelId, secondResult.Value.PreviousVersion!.ModelId);
        Assert.Equal(first.Model.CheckpointSha256, secondResult.Value.PreviousVersion.CheckpointSha256);
    }

    [Fact]
    public async Task TenantCanonicalization_CaseVariantsUseSameActivationRefAndIdempotencyReplay()
    {
        const string canonicalTenant = "tenant-design-conversion-case-alpha";
        const string caseVariantTenant = "Tenant-Design-Conversion-Case-Alpha";
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(root, canonicalTenant, "case-variant", batchCount: 1);
        var api = CreateApi(root);

        var activated = await api.ActivatePublishedModelAsync(
            Auth(caseVariantTenant, "activate:case-variant", expectedGeneration: 0),
            candidate,
            expectedModelVersion: candidate.Model.Version);
        var replay = await api.ActivatePublishedModelAsync(
            Auth(canonicalTenant, "activate:case-variant", expectedGeneration: 0),
            candidate,
            expectedModelVersion: candidate.Model.Version);
        var active = await new DesignConversionModelActivationRegistry(root).ReadActiveAsync(caseVariantTenant.ToUpperInvariant());

        Assert.True(activated.IsSuccess, activated.ErrorMessage);
        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        Assert.True(active.IsSuccess, active.ErrorMessage);
        Assert.Equal(DesignConversionModelActivationRegistry.BuildTenantIdRef(canonicalTenant), activated.Value!.TenantIdRef);
        Assert.Equal(activated.Value.TenantIdRef, replay.Value!.TenantIdRef);
        Assert.Equal(activated.Value.TenantIdRef, active.Value!.TenantIdRef);
        Assert.False(replay.Value.Applied);
        Assert.True(replay.Value.Replay);
        Assert.Equal(1, active.Value.Generation);
    }

    [Theory]
    [MemberData(nameof(InvalidTenantIds))]
    public async Task TenantCanonicalization_RejectsUnsafeTenantIdentifiers(string tenantId)
    {
        var canonical = DesignConversionModelActivationRegistry.TryCanonicalizeTenantId(tenantId);
        var active = await new DesignConversionModelActivationRegistry(NewRoot()).ReadActiveAsync(tenantId);

        Assert.False(canonical.IsSuccess);
        Assert.False(active.IsSuccess);
        Assert.True(
            active.ErrorCode is "DESIGN_CONVERSION_MODEL_ACTIVATION_TENANT_ID_REQUIRED"
                or "DESIGN_CONVERSION_MODEL_ACTIVATION_TENANT_ID_INVALID",
            active.ErrorCode);
    }

    [Fact]
    public async Task ReadActiveAsync_FreshRegistryReloadKeepsActiveAndPreviousMetadata()
    {
        var root = NewRoot();
        var first = await BuildActivationRecordAsync(root, TenantA, "first", batchCount: 1);
        var second = await BuildActivationRecordAsync(root, TenantA, "second", batchCount: 2);
        var api = CreateApi(root);
        Assert.True((await api.ActivatePublishedModelAsync(Auth(TenantA, "activate:first", 0), first)).IsSuccess);
        Assert.True((await api.ActivatePublishedModelAsync(Auth(TenantA, "activate:second", 1), second)).IsSuccess);

        var reloaded = new DesignConversionModelActivationRegistry(root);
        var active = await reloaded.ReadActiveAsync(TenantA);

        Assert.True(active.IsSuccess, active.ErrorMessage);
        Assert.Equal(2, active.Value!.Generation);
        Assert.Equal(second.Model.ModelId, active.Value.ActiveVersion.ModelId);
        Assert.NotNull(active.Value.PreviousVersion);
        Assert.Equal(first.Model.ModelId, active.Value.PreviousVersion!.ModelId);
    }

    [Fact]
    public async Task ReadActiveAsync_UnchangedDurableStoreUsesSingleReadPerRegistryInstance()
    {
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(root, TenantA, "single-read", batchCount: 1);
        var writer = CreateApi(root);
        Assert.True((await writer.ActivatePublishedModelAsync(Auth(TenantA, "activate:single-read", 0), candidate)).IsSuccess);
        var counting = CreateRegistryWithReadCounter(root);

        var first = await counting.Registry.ReadActiveAsync(TenantA);
        var second = await counting.Registry.ReadActiveAsync(TenantA);

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(1, first.Value!.Generation);
        Assert.Equal(first.Value.ActiveVersion.ModelId, second.Value!.ActiveVersion.ModelId);
        Assert.Equal(1, counting.ReadCount());
    }

    [Fact]
    public async Task ActivateAsync_SuccessfulSaveUpdatesCacheForSubsequentRead()
    {
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(root, TenantA, "save-cache", batchCount: 1);
        var counting = CreateRegistryWithReadCounter(root);
        var api = CreateApi(root, counting.Registry);

        var activated = await api.ActivatePublishedModelAsync(Auth(TenantA, "activate:save-cache", 0), candidate);
        var active = await counting.Registry.ReadActiveAsync(TenantA);

        Assert.True(activated.IsSuccess, activated.ErrorMessage);
        Assert.True(active.IsSuccess, active.ErrorMessage);
        Assert.Equal(1, active.Value!.Generation);
        Assert.Equal(activated.Value!.ActiveVersion.ModelId, active.Value.ActiveVersion.ModelId);
        Assert.Equal(0, counting.ReadCount());
    }

    [Fact]
    public async Task ActivateAsync_WarmCacheStillReloadsDurableStoreForMutationAuthority()
    {
        var root = NewRoot();
        var first = await BuildActivationRecordAsync(root, TenantA, "mutation-authority-first", batchCount: 1);
        var second = await BuildActivationRecordAsync(root, TenantA, "mutation-authority-second", batchCount: 2);
        var writer = CreateRegistry(root);
        Assert.True((await writer.ActivateAsync(
            ActivationCommand(TenantA, "activate:mutation-authority-first", 0, first))).IsSuccess);
        var counting = CreateRegistryWithReadCounter(root);

        var warm = await counting.Registry.ReadActiveAsync(TenantA);
        var warmReadCount = counting.ReadCount();
        var activated = await counting.Registry.ActivateAsync(
            ActivationCommand(TenantA, "activate:mutation-authority-second", 1, second));

        Assert.True(warm.IsSuccess, warm.ErrorMessage);
        Assert.Equal(1, warm.Value!.Generation);
        Assert.Equal(first.Model.ModelId, warm.Value.ActiveVersion.ModelId);
        Assert.Equal(1, warmReadCount);
        Assert.True(activated.IsSuccess, activated.ErrorMessage);
        Assert.True(activated.Value!.Applied);
        Assert.False(activated.Value.Replay);
        Assert.Equal(2, activated.Value.Generation);
        Assert.Equal(second.Model.ModelId, activated.Value.ActiveVersion.ModelId);
        Assert.Equal(warmReadCount + 1, counting.ReadCount());
    }

    [Fact]
    public async Task ReadActiveAsync_WarmCacheBypassesHeldWriterGate()
    {
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(root, TenantA, "warm-gate-bypass", batchCount: 1);
        using var gate = new SemaphoreSlim(1, 1);
        var counting = CreateRegistryWithReadCounter(root, gate);
        var api = CreateApi(root, counting.Registry);
        var activated = await api.ActivatePublishedModelAsync(Auth(TenantA, "activate:warm-gate-bypass", 0), candidate);
        Assert.True(activated.IsSuccess, activated.ErrorMessage);
        Assert.Equal(0, counting.ReadCount());

        await gate.WaitAsync();
        try
        {
            var active = await counting.Registry.ReadActiveAsync(TenantA)
                .WaitAsync(TimeSpan.FromSeconds(2));

            Assert.True(active.IsSuccess, active.ErrorMessage);
            Assert.Equal(1, active.Value!.Generation);
            Assert.Equal(candidate.Model.ModelId, active.Value.ActiveVersion.ModelId);
            Assert.Equal(0, counting.ReadCount());
        }
        finally
        {
            gate.Release();
        }
    }

    [Fact]
    public async Task ReadActiveAsync_ChangedDurableStoreWaitsForGateBeforeRefreshingStaleWarmCache()
    {
        var root = NewRoot();
        var first = await BuildActivationRecordAsync(root, TenantA, "changed-gate-first", batchCount: 1);
        var second = await BuildActivationRecordAsync(root, TenantA, "changed-gate-second", batchCount: 2);
        using var gate = new SemaphoreSlim(1, 1);
        var counting = CreateRegistryWithReadCounter(root, gate);
        var api = CreateApi(root, counting.Registry);
        Assert.True((await api.ActivatePublishedModelAsync(Auth(TenantA, "activate:changed-gate-first", 0), first)).IsSuccess);
        Assert.Equal(0, counting.ReadCount());

        var external = CreateApi(root);
        var externallyActivated = await external.ActivatePublishedModelAsync(Auth(TenantA, "activate:changed-gate-second", 1), second);
        Assert.True(externallyActivated.IsSuccess, externallyActivated.ErrorMessage);
        File.SetLastWriteTimeUtc(ActivationStatePath(root), DateTime.UtcNow.AddMinutes(5));

        await gate.WaitAsync();
        var activeTask = counting.Registry.ReadActiveAsync(TenantA);
        try
        {
            await Task.Delay(TimeSpan.FromMilliseconds(150));
            Assert.False(activeTask.IsCompleted);
        }
        finally
        {
            gate.Release();
        }

        var active = await activeTask.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.True(active.IsSuccess, active.ErrorMessage);
        Assert.Equal(2, active.Value!.Generation);
        Assert.Equal(second.Model.ModelId, active.Value.ActiveVersion.ModelId);
        Assert.Equal(1, counting.ReadCount());
    }

    [Fact]
    public async Task ReadActiveAsync_ParallelWarmReadsUseCachedSnapshotWithoutDurableReread()
    {
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(root, TenantA, "parallel-warm", batchCount: 1);
        var writer = CreateApi(root);
        Assert.True((await writer.ActivatePublishedModelAsync(Auth(TenantA, "activate:parallel-warm", 0), candidate)).IsSuccess);
        var counting = CreateRegistryWithReadCounter(root);
        var warm = await counting.Registry.ReadActiveAsync(TenantA);
        Assert.True(warm.IsSuccess, warm.ErrorMessage);
        Assert.Equal(1, counting.ReadCount());

        var reads = await Task.WhenAll(Enumerable.Range(0, 32)
            .Select(_ => counting.Registry.ReadActiveAsync(TenantA)));

        Assert.All(reads, active =>
        {
            Assert.True(active.IsSuccess, active.ErrorMessage);
            Assert.Equal(1, active.Value!.Generation);
            Assert.Equal(candidate.Model.ModelId, active.Value.ActiveVersion.ModelId);
        });
        Assert.Equal(1, counting.ReadCount());
    }

    [Fact]
    public async Task ReadActiveAsync_ExternalReplacementRefreshesCachedStore()
    {
        var root = NewRoot();
        var first = await BuildActivationRecordAsync(root, TenantA, "external-first", batchCount: 1);
        var second = await BuildActivationRecordAsync(root, TenantA, "external-second", batchCount: 2);
        var counting = CreateRegistryWithReadCounter(root);
        var api = CreateApi(root, counting.Registry);
        Assert.True((await api.ActivatePublishedModelAsync(Auth(TenantA, "activate:external-first", 0), first)).IsSuccess);
        Assert.Equal(0, counting.ReadCount());

        var external = CreateApi(root);
        var externallyActivated = await external.ActivatePublishedModelAsync(Auth(TenantA, "activate:external-second", 1), second);
        Assert.True(externallyActivated.IsSuccess, externallyActivated.ErrorMessage);
        File.SetLastWriteTimeUtc(ActivationStatePath(root), DateTime.UtcNow.AddMinutes(5));

        var active = await counting.Registry.ReadActiveAsync(TenantA);

        Assert.True(active.IsSuccess, active.ErrorMessage);
        Assert.Equal(2, active.Value!.Generation);
        Assert.Equal(second.Model.ModelId, active.Value.ActiveVersion.ModelId);
        Assert.Equal(1, counting.ReadCount());
    }

    [Fact]
    public async Task ReadActiveAsync_ReplacementDuringReadRetriesAndReturnsNewerStore()
    {
        var root = NewRoot();
        var first = await BuildActivationRecordAsync(root, TenantA, "during-read-first", batchCount: 1);
        var second = await BuildActivationRecordAsync(root, TenantA, "during-read-second", batchCount: 2);
        var api = CreateApi(root);
        Assert.True((await api.ActivatePublishedModelAsync(Auth(TenantA, "activate:during-read-first", 0), first)).IsSuccess);
        var statePath = ActivationStatePath(root);
        var oldJson = await File.ReadAllTextAsync(statePath, Encoding.UTF8);
        Assert.True((await api.ActivatePublishedModelAsync(Auth(TenantA, "activate:during-read-second", 1), second)).IsSuccess);
        var newerJson = await File.ReadAllTextAsync(statePath, Encoding.UTF8);
        await File.WriteAllTextAsync(statePath, oldJson, Encoding.UTF8);
        File.SetLastWriteTimeUtc(statePath, DateTime.UtcNow.AddMinutes(-5));

        var readCount = 0;
        var registry = new DesignConversionModelActivationRegistry(
            root,
            () => DateTimeOffset.Parse("2026-07-10T10:00:00Z"),
            async (path, encoding, ct) =>
            {
                var call = Interlocked.Increment(ref readCount);
                if (call == 1)
                {
                    var replacementPath = path + ".replacement";
                    await File.WriteAllTextAsync(replacementPath, newerJson, encoding, ct);
                    File.Move(replacementPath, path, overwrite: true);
                    File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddMinutes(5));
                    return oldJson;
                }

                return await File.ReadAllTextAsync(path, encoding, ct);
            });

        var active = await registry.ReadActiveAsync(TenantA);

        Assert.True(active.IsSuccess, active.ErrorMessage);
        Assert.Equal(2, active.Value!.Generation);
        Assert.Equal(second.Model.ModelId, active.Value.ActiveVersion.ModelId);
        Assert.Equal(2, Volatile.Read(ref readCount));
    }

    [Fact]
    public async Task ActivateAsync_MalformedExistingStoreFailsClosedWithoutOverwritingDurableBytes()
    {
        var root = NewRoot();
        var first = await BuildActivationRecordAsync(root, TenantA, "malformed-first", batchCount: 1);
        var second = await BuildActivationRecordAsync(root, TenantA, "malformed-second", batchCount: 2);
        var api = CreateApi(root);
        Assert.True((await api.ActivatePublishedModelAsync(Auth(TenantA, "activate:malformed-first", 0), first)).IsSuccess);
        var statePath = ActivationStatePath(root);
        var malformedBytes = Encoding.UTF8.GetBytes(
            "{\"schemaVersion\":\"design-conversion-model-activation-store-v1\",\"tenants\":[");
        await File.WriteAllBytesAsync(statePath, malformedBytes);

        var failed = await CreateRegistry(root).ActivateAsync(
            ActivationCommand(TenantA, "activate:malformed-second", expectedGeneration: 1, second));

        var afterBytes = await File.ReadAllBytesAsync(statePath);
        AssertStoreReadFailure(failed);
        Assert.Equal(malformedBytes, afterBytes);
        Assert.DoesNotContain(second.Model.ModelId, Encoding.UTF8.GetString(afterBytes));
        AssertNoActivationTempFiles(root);
    }

    [Fact]
    public async Task ReadActiveAsync_MalformedExistingStoreFailsClosedInsteadOfActiveVersionNotFound()
    {
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(root, TenantA, "malformed-read", batchCount: 1);
        var api = CreateApi(root);
        Assert.True((await api.ActivatePublishedModelAsync(Auth(TenantA, "activate:malformed-read", 0), candidate)).IsSuccess);
        var statePath = ActivationStatePath(root);
        var malformedBytes = Encoding.UTF8.GetBytes("{not-valid-json");
        await File.WriteAllBytesAsync(statePath, malformedBytes);

        var active = await CreateRegistry(root).ReadActiveAsync(TenantA);

        AssertStoreReadFailure(active);
        Assert.NotEqual("DESIGN_CONVERSION_MODEL_ACTIVATION_ACTIVE_VERSION_NOT_FOUND", active.ErrorCode);
        Assert.Equal(malformedBytes, await File.ReadAllBytesAsync(statePath));
        AssertNoActivationTempFiles(root);
    }

    [Fact]
    public async Task ReadActiveAsync_OversizedDurableStoreFailsBeforeReaderAndPreservesBytes()
    {
        var root = NewRoot();
        Directory.CreateDirectory(root);
        var statePath = ActivationStatePath(root);
        await using (var stream = new FileStream(
                         statePath,
                         FileMode.CreateNew,
                         FileAccess.ReadWrite,
                         FileShare.ReadWrite | FileShare.Delete,
                         bufferSize: 1,
                         FileOptions.Asynchronous | FileOptions.SequentialScan))
        {
            stream.SetLength(ExpectedMaxStoreBytes + 1);
        }

        var beforeLength = new FileInfo(statePath).Length;
        var beforeHash = await ReadSha256Async(statePath);
        var readCount = 0;
        var registry = new DesignConversionModelActivationRegistry(
            root,
            () => DateTimeOffset.Parse("2026-07-10T10:00:00Z"),
            (path, encoding, ct) =>
            {
                Interlocked.Increment(ref readCount);
                return Task.FromResult(
                    "{\"schemaVersion\":\"design-conversion-model-activation-store-v1\",\"tenants\":[],\"operations\":[]}");
            });

        var active = await registry.ReadActiveAsync(TenantA);

        Assert.True(
            !active.IsSuccess &&
            string.Equals(
                active.ErrorCode,
                "DESIGN_CONVERSION_MODEL_ACTIVATION_STORE_READ_FAILED",
                StringComparison.Ordinal) &&
            string.Equals(
                active.ErrorMessage,
                "DesignConversion model activation durable store could not be read safely.",
                StringComparison.Ordinal) &&
            Volatile.Read(ref readCount) == 0,
            $"Expected fail-closed store read before body reader; actual success={active.IsSuccess}, code={active.ErrorCode}, message={active.ErrorMessage}, readerCount={Volatile.Read(ref readCount)}.");
        Assert.Equal(beforeLength, new FileInfo(statePath).Length);
        Assert.Equal(beforeHash, await ReadSha256Async(statePath));
        AssertNoPrivateStoreLeak(active, statePath, TenantA);
        var publicCap = typeof(DesignConversionModelActivationRegistry).GetField(
            "MaximumActivationStoreBytes",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.GetValue(null);
        Assert.Equal(ExpectedMaxStoreBytes, Assert.IsType<long>(publicCap));
    }

    [Fact]
    public async Task ReadActiveAndActivateAsync_InvalidUtf8ExistingStoreFailClosedWithoutMutatingDurableBytes()
    {
        var root = NewRoot();
        var first = await BuildActivationRecordAsync(root, TenantA, "invalid-utf8-first", batchCount: 1);
        var second = await BuildActivationRecordAsync(root, TenantA, "invalid-utf8-second", batchCount: 2);
        var api = CreateApi(root);
        Assert.True((await api.ActivatePublishedModelAsync(Auth(TenantA, "activate:invalid-utf8-first", 0), first)).IsSuccess);
        var statePath = ActivationStatePath(root);
        var validBytes = await File.ReadAllBytesAsync(statePath);
        var invalidBytes = InjectInvalidUtf8IntoJsonString(validBytes, first.Model.ModelId);
        var replacementDecodedJson = ReplacementUtf8.GetString(invalidBytes);
        using var replacementDecodedDocument = JsonDocument.Parse(replacementDecodedJson);
        Assert.Contains("\uFFFD", replacementDecodedJson);
        Assert.Equal(
            DesignConversionModelActivationRegistry.StoreSchemaVersion,
            replacementDecodedDocument.RootElement.GetProperty("schemaVersion").GetString());
        await File.WriteAllBytesAsync(statePath, invalidBytes);

        var read = await CreateRegistry(root).ReadActiveAsync(TenantA);

        AssertStoreReadFailure(read);
        Assert.NotEqual("DESIGN_CONVERSION_MODEL_ACTIVATION_ACTIVE_VERSION_NOT_FOUND", read.ErrorCode);
        AssertNoPrivateStoreLeak(read, statePath, first.Model.ModelId);
        Assert.Equal(invalidBytes, await File.ReadAllBytesAsync(statePath));

        var activation = await CreateRegistry(root).ActivateAsync(
            ActivationCommand(TenantA, "activate:invalid-utf8-second", expectedGeneration: 1, second));

        AssertStoreReadFailure(activation);
        AssertNoPrivateStoreLeak(activation, statePath, second.Model.ModelId);
        var afterBytes = await File.ReadAllBytesAsync(statePath);
        Assert.Equal(invalidBytes, afterBytes);
        Assert.DoesNotContain(second.Model.ModelId, ReplacementUtf8.GetString(afterBytes));
        AssertNoActivationTempFiles(root);
    }

    [Fact]
    public async Task ActivateAsync_UnsupportedSchemaExistingStoreFailsClosedWithoutOverwritingDurableBytes()
    {
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(root, TenantA, "unsupported-schema", batchCount: 1);
        var statePath = ActivationStatePath(root);
        var unsupportedBytes = Encoding.UTF8.GetBytes(
            """
            {
              "schemaVersion": "design-conversion-model-activation-store-v0",
              "tenants": [],
              "operations": []
            }
            """);
        await File.WriteAllBytesAsync(statePath, unsupportedBytes);

        var failed = await CreateRegistry(root).ActivateAsync(
            ActivationCommand(TenantA, "activate:unsupported-schema", expectedGeneration: 0, candidate));

        AssertStoreReadFailure(failed);
        Assert.Equal(unsupportedBytes, await File.ReadAllBytesAsync(statePath));
        AssertNoActivationTempFiles(root);
    }

    [Fact]
    public async Task ReadActiveAsync_CancellationDuringDurableReadReturnsCancelledAndPreservesState()
    {
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(root, TenantA, "cancelled-read", batchCount: 1);
        var api = CreateApi(root);
        Assert.True((await api.ActivatePublishedModelAsync(Auth(TenantA, "activate:cancelled-read", 0), candidate)).IsSuccess);
        var statePath = ActivationStatePath(root);
        var originalBytes = await File.ReadAllBytesAsync(statePath);
        using var cts = new CancellationTokenSource();
        var registry = new DesignConversionModelActivationRegistry(
            root,
            () => DateTimeOffset.Parse("2026-07-10T10:00:00Z"),
            (path, encoding, ct) =>
            {
                cts.Cancel();
                return Task.FromCanceled<string>(ct);
            });

        var active = await registry.ReadActiveAsync(TenantA, cts.Token);

        Assert.False(active.IsSuccess);
        Assert.Equal("CANCELLED", active.ErrorCode);
        Assert.Equal("DesignConversion model activation store read cancelled.", active.ErrorMessage);
        Assert.Equal(originalBytes, await File.ReadAllBytesAsync(statePath));
        AssertNoActivationTempFiles(root);
    }

    [Fact]
    public async Task ReadActiveAsync_DeletedDurableStoreInvalidatesWarmCache()
    {
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(root, TenantA, "deleted-store", batchCount: 1);
        var writer = CreateApi(root);
        Assert.True((await writer.ActivatePublishedModelAsync(Auth(TenantA, "activate:deleted-store", 0), candidate)).IsSuccess);
        var counting = CreateRegistryWithReadCounter(root);
        var warm = await counting.Registry.ReadActiveAsync(TenantA);
        Assert.True(warm.IsSuccess, warm.ErrorMessage);
        Assert.Equal(1, counting.ReadCount());

        File.Delete(ActivationStatePath(root));

        var active = await counting.Registry.ReadActiveAsync(TenantA);

        Assert.False(active.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_MODEL_ACTIVATION_ACTIVE_VERSION_NOT_FOUND", active.ErrorCode);
        Assert.Equal(1, counting.ReadCount());
    }

    [Fact]
    public async Task ActivateAsync_FailedSaveDoesNotMutateCachedSnapshot()
    {
        var root = NewRoot();
        var first = await BuildActivationRecordAsync(root, TenantA, "snapshot-first", batchCount: 1);
        var second = await BuildActivationRecordAsync(root, TenantA, "snapshot-second", batchCount: 2);
        var counting = CreateRegistryWithReadCounter(root);
        var api = CreateApi(root, counting.Registry);
        Assert.True((await api.ActivatePublishedModelAsync(Auth(TenantA, "activate:snapshot-first", 0), first)).IsSuccess);
        var statePath = ActivationStatePath(root);
        File.SetAttributes(statePath, File.GetAttributes(statePath) | FileAttributes.ReadOnly);

        try
        {
            var failed = await api.ActivatePublishedModelAsync(Auth(TenantA, "activate:snapshot-second", 1), second);
            Assert.False(failed.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_MODEL_ACTIVATION_STORE_MUTATION_FAILED", failed.ErrorCode);
            Assert.Equal(
                "DesignConversion model activation durable store could not be mutated safely.",
                failed.ErrorMessage);
        }
        finally
        {
            File.SetAttributes(statePath, File.GetAttributes(statePath) & ~FileAttributes.ReadOnly);
        }

        var readCountAfterFailedMutation = counting.ReadCount();
        var active = await counting.Registry.ReadActiveAsync(TenantA);
        var durable = await new DesignConversionModelActivationRegistry(root).ReadActiveAsync(TenantA);

        Assert.True(active.IsSuccess, active.ErrorMessage);
        Assert.True(durable.IsSuccess, durable.ErrorMessage);
        Assert.Equal(1, active.Value!.Generation);
        Assert.Equal(first.Model.ModelId, active.Value.ActiveVersion.ModelId);
        Assert.Equal(1, durable.Value!.Generation);
        Assert.Equal(first.Model.ModelId, durable.Value.ActiveVersion.ModelId);
        Assert.Equal(1, readCountAfterFailedMutation);
        Assert.Equal(readCountAfterFailedMutation, counting.ReadCount());
    }

    [Fact]
    public async Task RollbackActiveModel_RestoresImmediatePreviousVersionWithoutDeletingCheckpoints()
    {
        var root = NewRoot();
        var first = await BuildActivationRecordAsync(root, TenantA, "first", batchCount: 1);
        var second = await BuildActivationRecordAsync(root, TenantA, "second", batchCount: 2);
        var api = CreateApi(root);
        Assert.True((await api.ActivatePublishedModelAsync(Auth(TenantA, "activate:first", 0), first)).IsSuccess);
        Assert.True((await api.ActivatePublishedModelAsync(Auth(TenantA, "activate:second", 1), second)).IsSuccess);

        var rollback = await api.RollbackActiveModelAsync(Auth(TenantA, "rollback:second-to-first", 2));

        Assert.True(rollback.IsSuccess, rollback.ErrorMessage);
        Assert.True(rollback.Value!.Applied);
        Assert.Equal(3, rollback.Value.Generation);
        Assert.Equal(first.Model.ModelId, rollback.Value.ActiveVersion.ModelId);
        Assert.Null(rollback.Value.PreviousVersion);
        Assert.False(rollback.Value.ProductReady);
        Assert.True(File.Exists(first.Model.CheckpointPath));
        Assert.True(File.Exists(second.Model.CheckpointPath));
        Assert.Contains("DESIGN_CONVERSION_MODEL_ROLLBACK_RESTORED_CHECKPOINT_REVALIDATED", rollback.Value.EvidenceRefs);
        Assert.Contains("DESIGN_CONVERSION_MODEL_ROLLBACK_ONE_STEP_PREVIOUS_VERSION_CONSUMED", rollback.Value.EvidenceRefs);
        Assert.DoesNotContain("DESIGN_CONVERSION_MODEL_ROLLBACK_CHECKPOINTS_PRESERVED", rollback.Value.EvidenceRefs);
        Assert.Equal(bool.FalseString, rollback.Value.BoundaryMetadata["model_activation_checkpoints_deleted"]);
        Assert.Equal(bool.FalseString, rollback.Value.BoundaryMetadata["model_activation_previous_version_recorded"]);

        var active = await new DesignConversionModelActivationRegistry(root).ReadActiveAsync(TenantA);
        Assert.True(active.IsSuccess, active.ErrorMessage);
        Assert.Equal(3, active.Value!.Generation);
        Assert.Equal(first.Model.ModelId, active.Value.ActiveVersion.ModelId);
        Assert.Null(active.Value.PreviousVersion);
        Assert.True(File.Exists(first.Model.CheckpointPath));
        Assert.True(File.Exists(second.Model.CheckpointPath));
    }

    [Fact]
    public async Task RollbackActiveModel_ConsumesPreviousVersionSoFreshSecondRollbackFailsClosed()
    {
        var root = NewRoot();
        var first = await BuildActivationRecordAsync(root, TenantA, "first", batchCount: 1);
        var second = await BuildActivationRecordAsync(root, TenantA, "second", batchCount: 2);
        var api = CreateApi(root);
        Assert.True((await api.ActivatePublishedModelAsync(Auth(TenantA, "activate:first", 0), first)).IsSuccess);
        Assert.True((await api.ActivatePublishedModelAsync(Auth(TenantA, "activate:second", 1), second)).IsSuccess);

        var firstRollback = await api.RollbackActiveModelAsync(Auth(TenantA, "rollback:second-to-first", 2));

        Assert.True(firstRollback.IsSuccess, firstRollback.ErrorMessage);
        Assert.Equal(3, firstRollback.Value!.Generation);
        Assert.Equal(first.Model.ModelId, firstRollback.Value.ActiveVersion.ModelId);
        Assert.Null(firstRollback.Value.PreviousVersion);

        var secondRollback = await api.RollbackActiveModelAsync(Auth(TenantA, "rollback:fresh-second-attempt", 3));
        var active = await new DesignConversionModelActivationRegistry(root).ReadActiveAsync(TenantA);

        Assert.False(secondRollback.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_MODEL_ROLLBACK_PREVIOUS_VERSION_REQUIRED", secondRollback.ErrorCode);
        Assert.True(active.IsSuccess, active.ErrorMessage);
        Assert.Equal(3, active.Value!.Generation);
        Assert.Equal(first.Model.ModelId, active.Value.ActiveVersion.ModelId);
        Assert.Null(active.Value.PreviousVersion);
        Assert.True(File.Exists(first.Model.CheckpointPath));
        Assert.True(File.Exists(second.Model.CheckpointPath));
    }

    [Fact]
    public async Task RollbackActiveModel_MissingPreviousCheckpointFailsClosedWithoutMutatingState()
    {
        var root = NewRoot();
        var first = await BuildActivationRecordAsync(root, TenantA, "first", batchCount: 1);
        var second = await BuildActivationRecordAsync(root, TenantA, "second", batchCount: 2);
        var api = CreateApi(root);
        Assert.True((await api.ActivatePublishedModelAsync(Auth(TenantA, "activate:first", 0), first)).IsSuccess);
        Assert.True((await api.ActivatePublishedModelAsync(Auth(TenantA, "activate:second", 1), second)).IsSuccess);
        var originalCheckpointBytes = await File.ReadAllBytesAsync(first.Model.CheckpointPath);
        var rollback = Auth(TenantA, "rollback:missing-previous-checkpoint", 2);
        File.Delete(first.Model.CheckpointPath);

        var failed = await api.RollbackActiveModelAsync(rollback);
        var activeAfterFailure = await new DesignConversionModelActivationRegistry(root).ReadActiveAsync(TenantA);

        Assert.False(failed.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_MODEL_ROLLBACK_CHECKPOINT_MISSING_OR_UNREADABLE", failed.ErrorCode);
        AssertSecondActivationStillActive(activeAfterFailure, first, second);

        await File.WriteAllBytesAsync(first.Model.CheckpointPath, originalCheckpointBytes);
        var retry = await api.RollbackActiveModelAsync(rollback);

        Assert.True(retry.IsSuccess, retry.ErrorMessage);
        Assert.True(retry.Value!.Applied);
        Assert.False(retry.Value.Replay);
        Assert.Equal(3, retry.Value.Generation);
        Assert.Equal(first.Model.ModelId, retry.Value.ActiveVersion.ModelId);
        Assert.Contains("DESIGN_CONVERSION_MODEL_ROLLBACK_RESTORED_CHECKPOINT_REVALIDATED", retry.Value.EvidenceRefs);
    }

    [Fact]
    public async Task RollbackActiveModel_TamperedPreviousCheckpointFailsClosedWithoutMutatingState()
    {
        var root = NewRoot();
        var first = await BuildActivationRecordAsync(root, TenantA, "first", batchCount: 1);
        var second = await BuildActivationRecordAsync(root, TenantA, "second", batchCount: 2);
        var api = CreateApi(root);
        Assert.True((await api.ActivatePublishedModelAsync(Auth(TenantA, "activate:first", 0), first)).IsSuccess);
        Assert.True((await api.ActivatePublishedModelAsync(Auth(TenantA, "activate:second", 1), second)).IsSuccess);
        await File.WriteAllTextAsync(first.Model.CheckpointPath, "{\"kind\":\"tampered-design-conversion-checkpoint\"}");

        var failed = await api.RollbackActiveModelAsync(Auth(TenantA, "rollback:tampered-previous-checkpoint", 2));
        var activeAfterFailure = await new DesignConversionModelActivationRegistry(root).ReadActiveAsync(TenantA);

        Assert.False(failed.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_MODEL_ROLLBACK_CHECKPOINT_SHA_MISMATCH", failed.ErrorCode);
        Assert.Contains("SHA-256", failed.ErrorMessage);
        AssertSecondActivationStillActive(activeAfterFailure, first, second);
    }

    [Fact]
    public async Task RollbackActiveModel_RepeatedOperationKeyIsDurableIdempotentReplay()
    {
        var root = NewRoot();
        var first = await BuildActivationRecordAsync(root, TenantA, "first", batchCount: 1);
        var second = await BuildActivationRecordAsync(root, TenantA, "second", batchCount: 2);
        var api = CreateApi(root);
        Assert.True((await api.ActivatePublishedModelAsync(Auth(TenantA, "activate:first", 0), first)).IsSuccess);
        Assert.True((await api.ActivatePublishedModelAsync(Auth(TenantA, "activate:second", 1), second)).IsSuccess);
        var auth = Auth(TenantA, "rollback:idempotent", 2);
        var firstRollback = await api.RollbackActiveModelAsync(auth);
        Assert.True(firstRollback.IsSuccess, firstRollback.ErrorMessage);

        var replay = await api.RollbackActiveModelAsync(auth);

        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        Assert.False(replay.Value!.Applied);
        Assert.True(replay.Value.Replay);
        Assert.Equal(firstRollback.Value!.Generation, replay.Value.Generation);
        Assert.Equal(firstRollback.Value.ActiveVersion.ModelId, replay.Value.ActiveVersion.ModelId);
        Assert.Null(firstRollback.Value.PreviousVersion);
        Assert.Null(replay.Value.PreviousVersion);
        Assert.Contains("DESIGN_CONVERSION_MODEL_ACTIVATION_IDEMPOTENCY_REPLAY", replay.Value.EvidenceRefs);
    }

    [Fact]
    public async Task ActivatePublishedModel_RejectsInvalidUnverifiedPromotionRef()
    {
        var root = NewRoot();
        var valid = await BuildActivationRecordAsync(root, TenantA, "invalid-ref", batchCount: 1);
        var invalidPublication = valid.Publication with
        {
            Manifest = valid.Publication.Manifest with
            {
                PromotionUploadRef = "artifact://attacker/checkpoint.json"
            }
        };
        var invalid = valid with { Publication = invalidPublication };
        var api = CreateApi(root);

        var result = await api.ActivatePublishedModelAsync(Auth(TenantA, "activate:invalid-ref", 0), invalid);
        var active = await new DesignConversionModelActivationRegistry(root).ReadActiveAsync(TenantA);

        Assert.False(result.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_MODEL_ACTIVATION_PROMOTION_REF_UNVERIFIED", result.ErrorCode);
        Assert.False(active.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_MODEL_ACTIVATION_ACTIVE_VERSION_NOT_FOUND", active.ErrorCode);
    }

    [Fact]
    public async Task ActivateAsync_AllowedArtifactRootAcceptsFullyQualifiedInRootArtifacts()
    {
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(root, TenantA, "allowed-root-positive", batchCount: 1);
        var registry = CreateRegistry(root, allowedArtifactRootDirectory: root);

        var result = await registry.ActivateAsync(
            ActivationCommand(TenantA, "activate:allowed-root-positive", 0, candidate));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(candidate.Model.ModelId, result.Value!.ActiveVersion.ModelId);
        Assert.False(result.Value.ProductReady);
    }

    [Fact]
    public async Task ActivateAsync_RejectsExistingCheckpointOutsideAllowedArtifactRootWithoutLeakingPath()
    {
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(root, TenantA, "outside-checkpoint", batchCount: 1);
        var outsideRoot = Path.Combine(Path.GetTempPath(), "llm_mvp_core", "design-conversion-model-activation-outside", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(outsideRoot);
        var outsideCheckpoint = Path.Combine(outsideRoot, "outside-checkpoint.json");
        File.Copy(candidate.Model.CheckpointPath, outsideCheckpoint);
        var forged = WithArtifactPaths(candidate, checkpointPath: outsideCheckpoint);

        var result = await CreateRegistry(root, allowedArtifactRootDirectory: root).ActivateAsync(
            ActivationCommand(TenantA, "activate:outside-checkpoint", 0, forged));

        Assert.False(result.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_MODEL_ACTIVATION_CHECKPOINT_PATH_INVALID", result.ErrorCode);
        AssertNoPathLeak(result, outsideRoot, outsideCheckpoint);
    }

    [Fact]
    public async Task ActivateAsync_RejectsExistingTokenizerOutsideAllowedArtifactRootWithoutLeakingPath()
    {
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(root, TenantA, "outside-tokenizer", batchCount: 1);
        var outsideRoot = Path.Combine(Path.GetTempPath(), "llm_mvp_core", "design-conversion-model-activation-outside", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(outsideRoot);
        var outsideTokenizer = Path.Combine(outsideRoot, "outside-tokenizer.json");
        await File.WriteAllTextAsync(outsideTokenizer, "{\"kind\":\"outside-tokenizer\"}", StrictUtf8);
        var forged = WithArtifactPaths(candidate, tokenizerPath: outsideTokenizer);

        var result = await CreateRegistry(root, allowedArtifactRootDirectory: root).ActivateAsync(
            ActivationCommand(TenantA, "activate:outside-tokenizer", 0, forged));

        Assert.False(result.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_MODEL_ACTIVATION_SUPPORTING_ARTIFACT_PATH_INVALID", result.ErrorCode);
        AssertNoPathLeak(result, outsideRoot, outsideTokenizer);
    }

    [Fact]
    public async Task ActivateAsync_RejectsExistingPolicyOutsideAllowedArtifactRootWithoutLeakingPath()
    {
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(root, TenantA, "outside-policy", batchCount: 1);
        var outsideRoot = Path.Combine(Path.GetTempPath(), "llm_mvp_core", "design-conversion-model-activation-outside", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(outsideRoot);
        var outsidePolicy = Path.Combine(outsideRoot, "outside-policy.json");
        await File.WriteAllTextAsync(outsidePolicy, "{\"kind\":\"outside-policy\"}", StrictUtf8);
        var forged = WithArtifactPaths(candidate, policyPath: outsidePolicy);

        var result = await CreateRegistry(root, allowedArtifactRootDirectory: root).ActivateAsync(
            ActivationCommand(TenantA, "activate:outside-policy", 0, forged));

        Assert.False(result.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_MODEL_ACTIVATION_SUPPORTING_ARTIFACT_PATH_INVALID", result.ErrorCode);
        AssertNoPathLeak(result, outsideRoot, outsidePolicy);
    }

    [Fact]
    public async Task ActivateAsync_RejectsExistingRelativeCheckpointInputWithoutLeakingPath()
    {
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(root, TenantA, "relative-checkpoint", batchCount: 1);
        var relativeName = $"relative-activation-checkpoint-{Guid.NewGuid():N}.json";
        var relativeTarget = Path.Combine(Environment.CurrentDirectory, relativeName);
        try
        {
            File.Copy(candidate.Model.CheckpointPath, relativeTarget);
            var forged = WithArtifactPaths(candidate, checkpointPath: relativeName);

            var result = await CreateRegistry(root, allowedArtifactRootDirectory: root).ActivateAsync(
                ActivationCommand(TenantA, "activate:relative-checkpoint", 0, forged));

            Assert.False(result.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_MODEL_ACTIVATION_CHECKPOINT_PATH_NOT_FULLY_QUALIFIED", result.ErrorCode);
            AssertNoPathLeak(result, relativeTarget, Environment.CurrentDirectory);
        }
        finally
        {
            if (File.Exists(relativeTarget))
                File.Delete(relativeTarget);
        }
    }

    [Fact]
    public async Task ActivateAsync_RejectsPrefixConfusableSiblingArtifactRootWithoutLeakingPath()
    {
        var root = NewRoot();
        var stateRoot = Path.Combine(root, "state");
        var allowedRoot = Path.Combine(root, "artifact-root");
        var candidate = await BuildActivationRecordAsync(allowedRoot, TenantA, "prefix-confusable", batchCount: 1);
        var siblingRoot = allowedRoot + "-sibling";
        Directory.CreateDirectory(siblingRoot);
        var siblingCheckpoint = Path.Combine(siblingRoot, "prefix-confusable-checkpoint.json");
        File.Copy(candidate.Model.CheckpointPath, siblingCheckpoint);
        var forged = WithArtifactPaths(candidate, checkpointPath: siblingCheckpoint);

        var result = await CreateRegistry(stateRoot, allowedArtifactRootDirectory: allowedRoot).ActivateAsync(
            ActivationCommand(TenantA, "activate:prefix-confusable", 0, forged));

        Assert.False(result.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_MODEL_ACTIVATION_CHECKPOINT_PATH_INVALID", result.ErrorCode);
        AssertNoPathLeak(result, siblingRoot, siblingCheckpoint);
    }

    [Fact]
    public async Task RollbackAsync_RejectsStoredPreviousCheckpointEscapeBeforeReadingWithoutLeakingPath()
    {
        var root = NewRoot();
        var stateRoot = Path.Combine(root, "state");
        var allowedRoot = Path.Combine(root, "artifact-root");
        var first = await BuildActivationRecordAsync(allowedRoot, TenantA, "rollback-escape-first", batchCount: 1);
        var second = await BuildActivationRecordAsync(allowedRoot, TenantA, "rollback-escape-second", batchCount: 2);
        var registry = CreateRegistry(stateRoot, allowedArtifactRootDirectory: allowedRoot);
        Assert.True((await registry.ActivateAsync(ActivationCommand(TenantA, "activate:rollback-escape-first", 0, first))).IsSuccess);
        Assert.True((await registry.ActivateAsync(ActivationCommand(TenantA, "activate:rollback-escape-second", 1, second))).IsSuccess);
        var outsideRoot = allowedRoot + "-rollback-sibling";
        Directory.CreateDirectory(outsideRoot);
        var outsideCheckpoint = Path.Combine(outsideRoot, "rollback-escape-first-checkpoint.json");
        File.Copy(first.Model.CheckpointPath, outsideCheckpoint);
        await RewriteStoredPreviousCheckpointPathAsync(stateRoot, outsideCheckpoint);

        var failed = await registry.RollbackAsync(
            RollbackCommand(TenantA, "rollback:escaped-previous", expectedGeneration: 2));
        var active = await registry.ReadActiveAsync(TenantA);

        Assert.False(failed.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_MODEL_ROLLBACK_CHECKPOINT_MISSING_OR_UNREADABLE", failed.ErrorCode);
        AssertNoPathLeak(failed, outsideRoot, outsideCheckpoint);
        AssertSecondActivationStillActive(active, first, second);
    }

    [Fact]
    public async Task ActivateAsync_RejectsReparsePointEscapeWhenArtifactParentIsReparsePoint()
    {
        var root = NewRoot();
        var stateRoot = Path.Combine(root, "state");
        var allowedRoot = Path.Combine(root, "artifact-root");
        var candidate = await BuildActivationRecordAsync(allowedRoot, TenantA, "reparse-parent", batchCount: 1);
        var reparseDirectory = Path.GetDirectoryName(candidate.Model.CheckpointPath)!;
        var reparseDirectoryFullPath = Path.GetFullPath(reparseDirectory);
        var pathComparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        FileAttributes GetAttributes(string path)
        {
            var attributes = File.GetAttributes(path);
            return string.Equals(Path.GetFullPath(path), reparseDirectoryFullPath, pathComparison)
                ? attributes | FileAttributes.ReparsePoint
                : attributes;
        }

        var result = await CreateRegistry(
                stateRoot,
                allowedArtifactRootDirectory: allowedRoot,
                getAttributes: GetAttributes)
            .ActivateAsync(ActivationCommand(TenantA, "activate:reparse-escape", 0, candidate));

        Assert.False(result.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_MODEL_ACTIVATION_CHECKPOINT_PATH_INVALID", result.ErrorCode);
        AssertNoPathLeak(result, reparseDirectory, candidate.Model.CheckpointPath);
    }

    [Fact]
    public async Task ActivatePublishedModel_CancelledPromotionAuthorityReadPreservesCancelledError()
    {
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(root, TenantA, "cancelled-authority", batchCount: 1);
        var api = CreateApi(root);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await api.ActivatePublishedModelAsync(
            Auth(TenantA, "activate:cancelled-authority", 0),
            candidate,
            expectedModelVersion: candidate.Model.Version,
            ct: cts.Token);
        var active = await new DesignConversionModelActivationRegistry(root).ReadActiveAsync(TenantA);

        Assert.False(result.IsSuccess);
        Assert.Equal("CANCELLED", result.ErrorCode);
        Assert.False(active.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_MODEL_ACTIVATION_ACTIVE_VERSION_NOT_FOUND", active.ErrorCode);
    }

    [Fact]
    public async Task ActivatePublishedModel_RejectsCrossTenantSubstitution()
    {
        var root = NewRoot();
        var tenantARecord = await BuildActivationRecordAsync(root, TenantA, "tenant-a", batchCount: 1);
        var api = CreateApi(root);

        var result = await api.ActivatePublishedModelAsync(Auth(TenantB, "activate:wrong-tenant", 0), tenantARecord);

        Assert.False(result.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_MODEL_ACTIVATION_TENANT_SCOPE_MISMATCH", result.ErrorCode);
    }

    [Fact]
    public async Task ActivatePublishedModel_FailedOperationLeavesPreviousActiveVersionUnchanged()
    {
        var root = NewRoot();
        var first = await BuildActivationRecordAsync(root, TenantA, "first", batchCount: 1);
        var invalid = await BuildActivationRecordAsync(root, TenantA, "invalid-version", batchCount: 2);
        var api = CreateApi(root);
        var activated = await api.ActivatePublishedModelAsync(Auth(TenantA, "activate:first", 0), first, first.Model.Version);
        Assert.True(activated.IsSuccess, activated.ErrorMessage);

        var failed = await api.ActivatePublishedModelAsync(
            Auth(TenantA, "activate:bad-version", 1),
            invalid,
            expectedModelVersion: "wrong-version");
        var active = await new DesignConversionModelActivationRegistry(root).ReadActiveAsync(TenantA);

        Assert.False(failed.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_MODEL_ACTIVATION_VERSION_SCOPE_MISMATCH", failed.ErrorCode);
        Assert.True(active.IsSuccess, active.ErrorMessage);
        Assert.Equal(1, active.Value!.Generation);
        Assert.Equal(first.Model.ModelId, active.Value.ActiveVersion.ModelId);
        Assert.Null(active.Value.PreviousVersion);
    }

    [Fact]
    public async Task ActivatePublishedModel_ConcurrentStaleGenerationLoserCannotOverwriteWinner()
    {
        var root = NewRoot();
        var first = await BuildActivationRecordAsync(root, TenantA, "first", batchCount: 1);
        var second = await BuildActivationRecordAsync(root, TenantA, "second", batchCount: 2);
        var third = await BuildActivationRecordAsync(root, TenantA, "third", batchCount: 3);
        var api = CreateApi(root);
        var activated = await api.ActivatePublishedModelAsync(Auth(TenantA, "activate:first", 0), first);
        Assert.True(activated.IsSuccess, activated.ErrorMessage);

        var attempts = await Task.WhenAll(
            api.ActivatePublishedModelAsync(Auth(TenantA, "activate:second", 1), second),
            api.ActivatePublishedModelAsync(Auth(TenantA, "activate:third", 1), third));
        var active = await new DesignConversionModelActivationRegistry(root).ReadActiveAsync(TenantA);

        Assert.Equal(1, attempts.Count(result => result.IsSuccess));
        Assert.Equal(1, attempts.Count(result =>
            !result.IsSuccess &&
            string.Equals(result.ErrorCode, "DESIGN_CONVERSION_MODEL_ACTIVATION_GENERATION_CONFLICT", StringComparison.Ordinal)));
        Assert.True(active.IsSuccess, active.ErrorMessage);
        Assert.Equal(2, active.Value!.Generation);
        Assert.Contains(
            active.Value.ActiveVersion.ModelId,
            new[] { second.Model.ModelId, third.Model.ModelId });
        Assert.NotEqual(first.Model.ModelId, active.Value.ActiveVersion.ModelId);
    }

    [Fact]
    public async Task ActivateAsync_MultiInstanceHeldMutationLeaseSerializesStaleGenerationWinner()
    {
        var root = NewRoot();
        var second = await BuildActivationRecordAsync(root, TenantA, "lease-race-second", batchCount: 2);
        var third = await BuildActivationRecordAsync(root, TenantA, "lease-race-third", batchCount: 3);
        var registryA = CreateRegistry(root);
        var registryB = CreateRegistry(root);
        using var heldLease = HoldActivationMutationLease(root);

        var attemptA = registryA.ActivateAsync(ActivationCommand(TenantA, "activate:lease-race-second", 0, second));
        var attemptB = registryB.ActivateAsync(ActivationCommand(TenantA, "activate:lease-race-third", 0, third));

        await Task.Delay(TimeSpan.FromMilliseconds(200));
        Assert.False(attemptA.IsCompleted, "first activation completed while the durable mutation lease was externally held");
        Assert.False(attemptB.IsCompleted, "second activation completed while the durable mutation lease was externally held");

        heldLease.Dispose();
        var attempts = await Task.WhenAll(
            attemptA.WaitAsync(TimeSpan.FromSeconds(10)),
            attemptB.WaitAsync(TimeSpan.FromSeconds(10)));
        var active = await CreateRegistry(root).ReadActiveAsync(TenantA);
        var winner = attempts.Single(result => result.IsSuccess);
        var conflict = attempts.Single(result => !result.IsSuccess);

        Assert.True(winner.Value!.Applied);
        Assert.False(winner.Value.Replay);
        Assert.Equal(1, winner.Value.Generation);
        Assert.Equal("DESIGN_CONVERSION_MODEL_ACTIVATION_GENERATION_CONFLICT", conflict.ErrorCode);
        Assert.True(active.IsSuccess, active.ErrorMessage);
        Assert.Equal(1, active.Value!.Generation);
        Assert.Equal(winner.Value.ActiveVersion.ModelId, active.Value.ActiveVersion.ModelId);
        Assert.Contains(
            active.Value.ActiveVersion.ModelId,
            new[] { second.Model.ModelId, third.Model.ModelId });
    }

    [Fact]
    public async Task ActivateAsync_HeldMutationLeaseCancellationFailsClosedWithoutMutatingState()
    {
        var root = NewRoot();
        var first = await BuildActivationRecordAsync(root, TenantA, "lease-cancel-first", batchCount: 1);
        var second = await BuildActivationRecordAsync(root, TenantA, "lease-cancel-second", batchCount: 2);
        var initial = await CreateRegistry(root).ActivateAsync(ActivationCommand(TenantA, "activate:lease-cancel-first", 0, first));
        Assert.True(initial.IsSuccess, initial.ErrorMessage);
        var statePath = ActivationStatePath(root);
        var originalBytes = await File.ReadAllBytesAsync(statePath);
        using var heldLease = HoldActivationMutationLease(root);
        using var cts = new CancellationTokenSource();

        var cancelledTask = CreateRegistry(root).ActivateAsync(
            ActivationCommand(TenantA, "activate:lease-cancel-second", 1, second),
            cts.Token);

        await Task.Delay(TimeSpan.FromMilliseconds(200));
        Assert.False(cancelledTask.IsCompleted, "activation completed while the durable mutation lease was externally held");
        cts.Cancel();

        var cancelled = await cancelledTask.WaitAsync(TimeSpan.FromSeconds(10));
        var activeWhileHeld = await CreateRegistry(root).ReadActiveAsync(TenantA);

        Assert.False(cancelled.IsSuccess);
        Assert.Equal("CANCELLED", cancelled.ErrorCode);
        Assert.Equal(originalBytes, await File.ReadAllBytesAsync(statePath));
        Assert.True(activeWhileHeld.IsSuccess, activeWhileHeld.ErrorMessage);
        Assert.Equal(1, activeWhileHeld.Value!.Generation);
        Assert.Equal(first.Model.ModelId, activeWhileHeld.Value.ActiveVersion.ModelId);

        heldLease.Dispose();
        var retry = await CreateRegistry(root).ActivateAsync(
            ActivationCommand(TenantA, "activate:lease-cancel-second", 1, second));
        var activeAfterRetry = await CreateRegistry(root).ReadActiveAsync(TenantA);

        Assert.True(retry.IsSuccess, retry.ErrorMessage);
        Assert.True(retry.Value!.Applied);
        Assert.False(retry.Value.Replay);
        Assert.Equal(2, retry.Value.Generation);
        Assert.True(activeAfterRetry.IsSuccess, activeAfterRetry.ErrorMessage);
        Assert.Equal(2, activeAfterRetry.Value!.Generation);
        Assert.Equal(second.Model.ModelId, activeAfterRetry.Value.ActiveVersion.ModelId);
    }

    [Fact]
    public async Task RollbackAsync_HeldMutationLeaseWaitsThenMutatesExactlyOnce()
    {
        var root = NewRoot();
        var first = await BuildActivationRecordAsync(root, TenantA, "lease-rollback-first", batchCount: 1);
        var second = await BuildActivationRecordAsync(root, TenantA, "lease-rollback-second", batchCount: 2);
        var registry = CreateRegistry(root);
        Assert.True((await registry.ActivateAsync(ActivationCommand(TenantA, "activate:lease-rollback-first", 0, first))).IsSuccess);
        Assert.True((await registry.ActivateAsync(ActivationCommand(TenantA, "activate:lease-rollback-second", 1, second))).IsSuccess);
        using var heldLease = HoldActivationMutationLease(root);

        var rollbackTask = CreateRegistry(root).RollbackAsync(
            RollbackCommand(TenantA, "rollback:lease-held-second-to-first", expectedGeneration: 2));

        await Task.Delay(TimeSpan.FromMilliseconds(200));
        Assert.False(rollbackTask.IsCompleted, "rollback completed while the durable mutation lease was externally held");

        heldLease.Dispose();
        var rollback = await rollbackTask.WaitAsync(TimeSpan.FromSeconds(10));
        var active = await CreateRegistry(root).ReadActiveAsync(TenantA);

        Assert.True(rollback.IsSuccess, rollback.ErrorMessage);
        Assert.True(rollback.Value!.Applied);
        Assert.False(rollback.Value.Replay);
        Assert.Equal(3, rollback.Value.Generation);
        Assert.Equal(first.Model.ModelId, rollback.Value.ActiveVersion.ModelId);
        Assert.Null(rollback.Value.PreviousVersion);
        Assert.True(active.IsSuccess, active.ErrorMessage);
        Assert.Equal(3, active.Value!.Generation);
        Assert.Equal(first.Model.ModelId, active.Value.ActiveVersion.ModelId);
        Assert.Null(active.Value.PreviousVersion);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(1, 2)]
    public void OperationRetention_InvalidInjectedLimitsFailFast(int retainedOperationCount, int retainedOperationCountPerTenant)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            CreateRegistry(NewRoot(), retainedOperationCount, retainedOperationCountPerTenant));
    }

    [Fact]
    public async Task OperationRetention_DurableJsonOperationsNeverExceedGlobalAndPerTenantCaps()
    {
        var root = NewRoot();
        var tenantA = await BuildActivationRecordAsync(root, TenantA, "retention-tenant-a", batchCount: 1);
        var tenantB = await BuildActivationRecordAsync(root, TenantB, "retention-tenant-b", batchCount: 1);
        var now = DateTimeOffset.Parse("2026-07-10T10:00:00Z");
        var registry = CreateRegistry(root, retainedOperationCount: 3, retainedOperationCountPerTenant: 2, () => now);
        var tenantAResults = new List<OperationResult<DesignConversionModelActivationResult>>();
        var tenantBResults = new List<OperationResult<DesignConversionModelActivationResult>>();

        for (var index = 0; index < 3; index++)
        {
            now = now.AddMinutes(1);
            var result = await registry.ActivateAsync(
                ActivationCommand(TenantA, $"activate:retention-a-{index}", index, tenantA));
            Assert.True(result.IsSuccess, result.ErrorMessage);
            tenantAResults.Add(result);
        }

        for (var index = 0; index < 3; index++)
        {
            now = now.AddMinutes(1);
            var result = await registry.ActivateAsync(
                ActivationCommand(TenantB, $"activate:retention-b-{index}", index, tenantB));
            Assert.True(result.IsSuccess, result.ErrorMessage);
            tenantBResults.Add(result);
        }

        var operations = await ReadStoredOperationsAsync(root);
        var retainedKeys = operations
            .Select(operation => operation.OperationKeyRef)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var expectedRetainedKeys = new[]
            {
                tenantAResults[2].Value!.OperationKeyRef,
                tenantBResults[1].Value!.OperationKeyRef,
                tenantBResults[2].Value!.OperationKeyRef
            }
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(3, operations.Count);
        Assert.All(
            operations.GroupBy(operation => operation.TenantIdRef, StringComparer.Ordinal),
            tenantOperations => Assert.True(tenantOperations.Count() <= 2, tenantOperations.Key));
        Assert.Contains(operations, operation => string.Equals(operation.OperationKeyRef, tenantBResults[2].Value!.OperationKeyRef, StringComparison.Ordinal));
        Assert.Equal(expectedRetainedKeys, retainedKeys);
    }

    [Fact]
    public async Task OperationRetention_RetainedNewestKeyReplaysAndDifferentPayloadConflicts()
    {
        var root = NewRoot();
        var retainedCandidate = await BuildActivationRecordAsync(root, TenantA, "retention-retained", batchCount: 1);
        var conflictingCandidate = await BuildActivationRecordAsync(root, TenantA, "retention-conflicting", batchCount: 2);
        var now = DateTimeOffset.Parse("2026-07-10T10:00:00Z");
        var registry = CreateRegistry(root, retainedOperationCount: 3, retainedOperationCountPerTenant: 3, () => now);
        OperationResult<DesignConversionModelActivationResult>? newest = null;

        for (var index = 0; index < 3; index++)
        {
            now = now.AddMinutes(1);
            newest = await registry.ActivateAsync(
                ActivationCommand(TenantA, $"activate:retention-retained-{index}", index, retainedCandidate));
            Assert.True(newest.IsSuccess, newest.ErrorMessage);
        }

        var activeBeforeReplay = await registry.ReadActiveAsync(TenantA);
        var replayCommand = ActivationCommand(
            TenantA,
            "activate:retention-retained-2",
            expectedGeneration: 2,
            retainedCandidate);
        var replay = await registry.ActivateAsync(replayCommand);
        var activeAfterReplay = await registry.ReadActiveAsync(TenantA);
        var conflict = await registry.ActivateAsync(ActivationCommand(
            TenantA,
            "activate:retention-retained-2",
            expectedGeneration: 2,
            conflictingCandidate));
        var activeAfterConflict = await registry.ReadActiveAsync(TenantA);

        Assert.True(activeBeforeReplay.IsSuccess, activeBeforeReplay.ErrorMessage);
        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        Assert.False(replay.Value!.Applied);
        Assert.True(replay.Value.Replay);
        Assert.Equal(newest!.Value!.Generation, replay.Value.Generation);
        Assert.True(activeAfterReplay.IsSuccess, activeAfterReplay.ErrorMessage);
        Assert.Equal(activeBeforeReplay.Value!.Generation, activeAfterReplay.Value!.Generation);
        Assert.Equal(activeBeforeReplay.Value.ActiveVersion.ModelId, activeAfterReplay.Value.ActiveVersion.ModelId);
        Assert.False(conflict.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_MODEL_ACTIVATION_IDEMPOTENCY_KEY_CONFLICT", conflict.ErrorCode);
        Assert.True(activeAfterConflict.IsSuccess, activeAfterConflict.ErrorMessage);
        Assert.Equal(activeBeforeReplay.Value.Generation, activeAfterConflict.Value!.Generation);
        Assert.Equal(activeBeforeReplay.Value.ActiveVersion.ModelId, activeAfterConflict.Value.ActiveVersion.ModelId);
    }

    [Fact]
    public async Task OperationRetention_TieHandlingIsDeterministicAndKeepsJustAppliedRecord()
    {
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(root, TenantA, "retention-tie", batchCount: 1);
        var now = DateTimeOffset.Parse("2026-07-10T10:00:00Z");
        var registry = CreateRegistry(root, retainedOperationCount: 2, retainedOperationCountPerTenant: 2, () => now);

        var first = await registry.ActivateAsync(ActivationCommand(TenantA, "activate:tie-a", 0, candidate));
        var second = await registry.ActivateAsync(ActivationCommand(TenantA, "activate:tie-b", 1, candidate));
        var third = await registry.ActivateAsync(ActivationCommand(TenantA, "activate:tie-c", 2, candidate));
        var operations = await ReadStoredOperationsAsync(root);
        var deterministicPreviousSurvivor = new[]
            {
                first.Value!.OperationKeyRef,
                second.Value!.OperationKeyRef
            }
            .Order(StringComparer.Ordinal)
            .First();

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.True(third.IsSuccess, third.ErrorMessage);
        Assert.Equal(2, operations.Count);
        Assert.Contains(operations, operation => string.Equals(operation.OperationKeyRef, third.Value!.OperationKeyRef, StringComparison.Ordinal));
        Assert.Contains(operations, operation => string.Equals(operation.OperationKeyRef, deterministicPreviousSurvivor, StringComparison.Ordinal));
        Assert.All(operations, operation => Assert.Equal(now, operation.CreatedAtUtc));
    }

    [Fact]
    public async Task OperationRetention_CallerForgedRequestedAtCannotKeepOldOperationOrEvictNewerServerTime()
    {
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(root, TenantA, "retention-forged-request", batchCount: 1);
        var now = DateTimeOffset.Parse("2026-07-10T10:00:00Z");
        var registry = CreateRegistry(root, retainedOperationCount: 2, retainedOperationCountPerTenant: 2, () => now);

        var forgedOld = await registry.ActivateAsync(ActivationCommand(
            TenantA,
            "activate:forged-old",
            expectedGeneration: 0,
            candidate,
            requestedAtUtc: DateTimeOffset.Parse("2099-01-01T00:00:00Z")));
        now = now.AddMinutes(1);
        var middle = await registry.ActivateAsync(ActivationCommand(
            TenantA,
            "activate:server-middle",
            expectedGeneration: 1,
            candidate,
            requestedAtUtc: DateTimeOffset.Parse("2000-01-01T00:00:00Z")));
        now = now.AddMinutes(1);
        var newest = await registry.ActivateAsync(ActivationCommand(
            TenantA,
            "activate:server-newest",
            expectedGeneration: 2,
            candidate,
            requestedAtUtc: DateTimeOffset.Parse("1999-01-01T00:00:00Z")));
        var operations = await ReadStoredOperationsAsync(root);

        Assert.True(forgedOld.IsSuccess, forgedOld.ErrorMessage);
        Assert.True(middle.IsSuccess, middle.ErrorMessage);
        Assert.True(newest.IsSuccess, newest.ErrorMessage);
        Assert.Equal(2, operations.Count);
        Assert.DoesNotContain(operations, operation => string.Equals(operation.OperationKeyRef, forgedOld.Value!.OperationKeyRef, StringComparison.Ordinal));
        Assert.Contains(operations, operation => string.Equals(operation.OperationKeyRef, middle.Value!.OperationKeyRef, StringComparison.Ordinal));
        Assert.Contains(operations, operation => string.Equals(operation.OperationKeyRef, newest.Value!.OperationKeyRef, StringComparison.Ordinal));
        Assert.All(operations, operation => Assert.True(operation.CreatedAtUtc < DateTimeOffset.Parse("2099-01-01T00:00:00Z")));
    }

    [Fact]
    public async Task OperationRetention_EvictedKeyWithStaleGenerationFailsCasAndLeavesActiveStateUnchanged()
    {
        var root = NewRoot();
        var first = await BuildActivationRecordAsync(root, TenantA, "retention-stale-first", batchCount: 1);
        var second = await BuildActivationRecordAsync(root, TenantA, "retention-stale-second", batchCount: 2);
        var now = DateTimeOffset.Parse("2026-07-10T10:00:00Z");
        var registry = CreateRegistry(root, retainedOperationCount: 1, retainedOperationCountPerTenant: 1, () => now);

        var old = await registry.ActivateAsync(ActivationCommand(TenantA, "activate:stale-evicted", 0, first));
        now = now.AddMinutes(1);
        var latest = await registry.ActivateAsync(ActivationCommand(TenantA, "activate:stale-latest", 1, second));
        var operationsBeforeReuse = await ReadStoredOperationsAsync(root);
        var reuse = await registry.ActivateAsync(ActivationCommand(TenantA, "activate:stale-evicted", 0, first));
        var active = await registry.ReadActiveAsync(TenantA);
        var operationsAfterReuse = await ReadStoredOperationsAsync(root);

        Assert.True(old.IsSuccess, old.ErrorMessage);
        Assert.True(latest.IsSuccess, latest.ErrorMessage);
        Assert.Single(operationsBeforeReuse);
        Assert.Equal(latest.Value!.OperationKeyRef, operationsBeforeReuse[0].OperationKeyRef);
        Assert.False(reuse.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_MODEL_ACTIVATION_GENERATION_CONFLICT", reuse.ErrorCode);
        Assert.True(active.IsSuccess, active.ErrorMessage);
        Assert.Equal(2, active.Value!.Generation);
        Assert.Equal(second.Model.ModelId, active.Value.ActiveVersion.ModelId);
        Assert.Single(operationsAfterReuse);
        Assert.Equal(latest.Value.OperationKeyRef, operationsAfterReuse[0].OperationKeyRef);
    }

    [Fact]
    public async Task OperationRetention_MarkersExposeBoundedLimitsOnNormalReplayAndReadActive()
    {
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(root, TenantA, "retention-markers", batchCount: 1);
        var command = ActivationCommand(TenantA, "activate:retention-markers", 0, candidate);
        var registry = CreateRegistry(root, retainedOperationCount: 5, retainedOperationCountPerTenant: 5);

        var applied = await registry.ActivateAsync(command);
        var currentLimitRegistry = CreateRegistry(root, retainedOperationCount: 3, retainedOperationCountPerTenant: 2);
        var replay = await currentLimitRegistry.ActivateAsync(command);
        var active = await currentLimitRegistry.ReadActiveAsync(TenantA);

        Assert.True(applied.IsSuccess, applied.ErrorMessage);
        AssertRetentionMarkers(applied.Value!, retainedOperationCount: 5, retainedOperationCountPerTenant: 5);
        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        Assert.True(replay.Value!.Replay);
        AssertRetentionMarkers(replay.Value, retainedOperationCount: 3, retainedOperationCountPerTenant: 2);
        Assert.True(active.IsSuccess, active.ErrorMessage);
        AssertRetentionMarkers(active.Value!, retainedOperationCount: 3, retainedOperationCountPerTenant: 2);
    }

    [Fact]
    public async Task OperationRetention_RollbackResultExposesBoundedLimits()
    {
        var root = NewRoot();
        var first = await BuildActivationRecordAsync(root, TenantA, "retention-rollback-first", batchCount: 1);
        var second = await BuildActivationRecordAsync(root, TenantA, "retention-rollback-second", batchCount: 2);
        var registry = CreateRegistry(root, retainedOperationCount: 2, retainedOperationCountPerTenant: 2);
        var firstActivation = await registry.ActivateAsync(ActivationCommand(TenantA, "activate:rollback-first", 0, first));
        var secondActivation = await registry.ActivateAsync(ActivationCommand(TenantA, "activate:rollback-second", 1, second));

        var rollback = await registry.RollbackAsync(new DesignConversionModelRollbackCommand(
            TenantId: TenantA,
            RequestedByRef: "architect:design-conversion-model-lifecycle-test",
            Reason: "focused rollback retention marker test",
            IdempotencyKey: "rollback:retention-markers",
            ExpectedGeneration: 2,
            RequestedAtUtc: DateTimeOffset.Parse("2026-07-10T10:00:00Z")));

        Assert.True(firstActivation.IsSuccess, firstActivation.ErrorMessage);
        Assert.True(secondActivation.IsSuccess, secondActivation.ErrorMessage);
        Assert.True(rollback.IsSuccess, rollback.ErrorMessage);
        Assert.True(rollback.Value!.Applied);
        Assert.False(rollback.Value.Replay);
        Assert.Equal("rollback", rollback.Value.OperationKind);
        Assert.Contains("DESIGN_CONVERSION_MODEL_ROLLBACK_APPLIED", rollback.Value.EvidenceRefs);
        AssertRetentionMarkers(rollback.Value, retainedOperationCount: 2, retainedOperationCountPerTenant: 2);
    }

    [Fact]
    public async Task OperationRetention_RestartReloadPreservesBoundedStoreAndRetainedReplay()
    {
        var root = NewRoot();
        var candidate = await BuildActivationRecordAsync(root, TenantA, "retention-reload", batchCount: 1);
        var now = DateTimeOffset.Parse("2026-07-10T10:00:00Z");
        var registry = CreateRegistry(root, retainedOperationCount: 3, retainedOperationCountPerTenant: 2, () => now);

        var evicted = await registry.ActivateAsync(ActivationCommand(TenantA, "activate:reload-evicted", 0, candidate));
        now = now.AddMinutes(1);
        var retained = await registry.ActivateAsync(ActivationCommand(TenantA, "activate:reload-retained", 1, candidate));
        now = now.AddMinutes(1);
        var newest = await registry.ActivateAsync(ActivationCommand(TenantA, "activate:reload-newest", 2, candidate));
        var operationsBeforeReload = await ReadStoredOperationsAsync(root);

        var reloaded = CreateRegistry(root, retainedOperationCount: 3, retainedOperationCountPerTenant: 2, () => now.AddMinutes(10));
        var replay = await reloaded.ActivateAsync(ActivationCommand(TenantA, "activate:reload-newest", 2, candidate));
        var operationsAfterReload = await ReadStoredOperationsAsync(root);

        Assert.True(evicted.IsSuccess, evicted.ErrorMessage);
        Assert.True(retained.IsSuccess, retained.ErrorMessage);
        Assert.True(newest.IsSuccess, newest.ErrorMessage);
        Assert.Equal(2, operationsBeforeReload.Count);
        Assert.DoesNotContain(operationsBeforeReload, operation => string.Equals(operation.OperationKeyRef, evicted.Value!.OperationKeyRef, StringComparison.Ordinal));
        Assert.Contains(operationsBeforeReload, operation => string.Equals(operation.OperationKeyRef, retained.Value!.OperationKeyRef, StringComparison.Ordinal));
        Assert.Contains(operationsBeforeReload, operation => string.Equals(operation.OperationKeyRef, newest.Value!.OperationKeyRef, StringComparison.Ordinal));
        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        Assert.False(replay.Value!.Applied);
        Assert.True(replay.Value.Replay);
        Assert.Equal(2, operationsAfterReload.Count);
        Assert.Equal(
            operationsBeforeReload.Select(operation => operation.OperationKeyRef).Order(StringComparer.Ordinal),
            operationsAfterReload.Select(operation => operation.OperationKeyRef).Order(StringComparer.Ordinal));
    }

    public static IEnumerable<object[]> InvalidTenantIds()
    {
        yield return [string.Empty];
        yield return ["   "];
        yield return ["tenant:design-conversion:alpha"];
        yield return ["tenant/design-conversion-alpha"];
        yield return ["tenant\\design-conversion-alpha"];
        yield return ["https://tenant-design-conversion-alpha"];
        yield return ["../tenant-design-conversion-alpha"];
        yield return ["."];
        yield return [".."];
        yield return ["-tenant-design-conversion-alpha"];
        yield return ["tenant-design-conversion-alpha-"];
        yield return ["tenant--design-conversion-alpha"];
        yield return ["tenant design-conversion-alpha"];
        yield return ["tenant\tdesign-conversion-alpha"];
        yield return ["tenant\ndesign-conversion-alpha"];
        yield return ["tenant\u0001design-conversion-alpha"];
        yield return ["ten\u0430nt-design-conversion-alpha"];
        yield return ["tenant-\u00e9-design-conversion-alpha"];
        yield return [new string('a', DesignConversionModelActivationRegistry.MaximumCanonicalTenantIdLength + 1)];
    }

    private static DesignConversionModelServerModelActivationApi CreateApi(string root)
        => CreateApi(
            root,
            new DesignConversionModelActivationRegistry(root, () => DateTimeOffset.Parse("2026-07-10T10:00:00Z")));

    private static DesignConversionModelServerModelActivationApi CreateApi(
        string root,
        DesignConversionModelActivationRegistry registry)
        => new(
            registry,
            LocalToken,
            new JsonlDesignConversionLocalArtifactDb(ArtifactDbRoot(root)));

    private static DesignConversionModelActivationRegistry CreateRegistry(
        string root,
        string? allowedArtifactRootDirectory = null,
        Func<string, FileAttributes>? getAttributes = null)
        => new(
            root,
            () => DateTimeOffset.Parse("2026-07-10T10:00:00Z"),
            File.ReadAllTextAsync,
            allowedArtifactRootDirectory: allowedArtifactRootDirectory,
            getAttributes: getAttributes);

    private static DesignConversionModelActivationRegistry CreateRegistry(
        string root,
        int retainedOperationCount,
        int retainedOperationCountPerTenant,
        Func<DateTimeOffset>? utcNow = null)
        => new(
            root,
            utcNow ?? (() => DateTimeOffset.Parse("2026-07-10T10:00:00Z")),
            File.ReadAllTextAsync,
            new DesignConversionModelActivationOperationRetentionLimits(
                retainedOperationCount,
                retainedOperationCountPerTenant));

    private static string ArtifactDbRoot(string root)
        => Path.Combine(root, "artifact-db");

    private static DesignConversionPromotedModelActivationRecord WithArtifactPaths(
        DesignConversionPromotedModelActivationRecord record,
        string? checkpointPath = null,
        string? tokenizerPath = null,
        string? policyPath = null)
    {
        Assert.NotNull(record.Publication.Manifest.PromotionArtifacts);
        var artifacts = record.Publication.Manifest.PromotionArtifacts!;
        var nextCheckpointPath = checkpointPath ?? artifacts.CheckpointPath;
        var nextArtifacts = artifacts with
        {
            CheckpointPath = nextCheckpointPath,
            TokenizerPath = tokenizerPath ?? artifacts.TokenizerPath,
            PolicyPath = policyPath ?? artifacts.PolicyPath
        };
        var nextManifest = record.Publication.Manifest with
        {
            CheckpointRef = nextCheckpointPath,
            PromotionArtifacts = nextArtifacts
        };

        return record with
        {
            Publication = record.Publication with { Manifest = nextManifest },
            Model = checkpointPath is null
                ? record.Model
                : record.Model with { CheckpointPath = checkpointPath }
        };
    }

    private static async Task RewriteStoredPreviousCheckpointPathAsync(string root, string checkpointPath)
    {
        var fullPath = Path.GetFullPath(checkpointPath);
        var statePath = ActivationStatePath(root);
        var state = JsonNode.Parse(await File.ReadAllTextAsync(statePath, StrictUtf8))!.AsObject();
        var tenants = state["tenants"]!.AsArray();
        Assert.Single(tenants);
        var previousVersion = tenants[0]!["previousVersion"]!.AsObject();
        previousVersion["checkpointPath"] = fullPath;
        previousVersion["checkpointPathRef"] = BuildCheckpointPathRef(fullPath);
        await File.WriteAllTextAsync(statePath, state.ToJsonString(), StrictUtf8);
    }

    private static string BuildCheckpointPathRef(string checkpointPath)
        => $"sha256:design-conversion-model-activation-checkpoint-path:{Sha256Text(checkpointPath)}";

    private static string Sha256Text(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static (DesignConversionModelActivationRegistry Registry, Func<int> ReadCount) CreateRegistryWithReadCounter(
        string root,
        SemaphoreSlim? gate = null)
    {
        var readCount = 0;
        var registry = new DesignConversionModelActivationRegistry(
            root,
            () => DateTimeOffset.Parse("2026-07-10T10:00:00Z"),
            async (path, encoding, ct) =>
            {
                Interlocked.Increment(ref readCount);
                return await File.ReadAllTextAsync(path, encoding, ct);
            },
            gate: gate);
        return (registry, () => Volatile.Read(ref readCount));
    }

    private static DesignConversionModelServerModelActivationAuthorization Auth(
        string tenantId,
        string idempotencyKey,
        long expectedGeneration)
        => new(
            TenantId: tenantId,
            RequestedByRef: "architect:design-conversion-model-lifecycle-test",
            Reason: $"focused activation lifecycle test {idempotencyKey}",
            IdempotencyKey: idempotencyKey,
            ExpectedGeneration: expectedGeneration,
            LocalAuthorizationToken: LocalToken);

    private static DesignConversionModelActivationCommand ActivationCommand(
        string tenantId,
        string idempotencyKey,
        long expectedGeneration,
        DesignConversionPromotedModelActivationRecord candidate,
        DateTimeOffset? requestedAtUtc = null)
        => new(
            TenantId: tenantId,
            RequestedByRef: "architect:design-conversion-model-lifecycle-test",
            Reason: $"focused activation lifecycle test {idempotencyKey}",
            IdempotencyKey: idempotencyKey,
            ExpectedGeneration: expectedGeneration,
            Candidate: candidate,
            ExpectedModelVersion: candidate.Model.Version,
            RequestedAtUtc: requestedAtUtc ?? DateTimeOffset.Parse("2026-07-10T10:00:00Z"));

    private static DesignConversionModelRollbackCommand RollbackCommand(
        string tenantId,
        string idempotencyKey,
        long expectedGeneration,
        DateTimeOffset? requestedAtUtc = null)
        => new(
            TenantId: tenantId,
            RequestedByRef: "architect:design-conversion-model-lifecycle-test",
            Reason: $"focused activation lifecycle test {idempotencyKey}",
            IdempotencyKey: idempotencyKey,
            ExpectedGeneration: expectedGeneration,
            RequestedAtUtc: requestedAtUtc ?? DateTimeOffset.Parse("2026-07-10T10:00:00Z"));

    private static void AssertStoreReadFailure<T>(OperationResult<T> result)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_MODEL_ACTIVATION_STORE_READ_FAILED", result.ErrorCode);
        Assert.Equal(
            "DesignConversion model activation durable store could not be read safely.",
            result.ErrorMessage);
    }

    private static void AssertNoPrivateStoreLeak<T>(OperationResult<T> result, string statePath, params string[] privateFragments)
    {
        var errorMessage = result.ErrorMessage ?? string.Empty;
        Assert.DoesNotContain("\uFFFD", errorMessage);
        Assert.DoesNotContain(statePath, errorMessage);
        foreach (var fragment in privateFragments.Where(value => !string.IsNullOrEmpty(value)))
            Assert.DoesNotContain(fragment, errorMessage);
    }

    private static void AssertNoPathLeak<T>(OperationResult<T> result, params string[] privateFragments)
    {
        var errorMessage = result.ErrorMessage ?? string.Empty;
        Assert.DoesNotContain("\uFFFD", errorMessage);
        foreach (var fragment in privateFragments.Where(value => !string.IsNullOrEmpty(value)))
            Assert.DoesNotContain(fragment, errorMessage, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertNoActivationTempFiles(string root)
        => Assert.Empty(Directory.EnumerateFiles(
            Path.GetFullPath(root),
            "design-conversion-model-activation-state.json.*.tmp"));

    private static async Task<IReadOnlyList<StoredActivationOperation>> ReadStoredOperationsAsync(string root)
    {
        await using var stream = File.OpenRead(ActivationStatePath(root));
        using var document = await JsonDocument.ParseAsync(stream);
        return document.RootElement
            .GetProperty("operations")
            .EnumerateArray()
            .Select(operation => new StoredActivationOperation(
                operation.GetProperty("tenantIdRef").GetString() ?? string.Empty,
                operation.GetProperty("operationKeyRef").GetString() ?? string.Empty,
                operation.GetProperty("operationKind").GetString() ?? string.Empty,
                operation.GetProperty("createdAtUtc").GetDateTimeOffset()))
            .ToArray();
    }

    private static void AssertRetentionMarkers(
        DesignConversionModelActivationResult result,
        int retainedOperationCount,
        int retainedOperationCountPerTenant)
    {
        Assert.Contains("DESIGN_CONVERSION_MODEL_ACTIVATION_IDEMPOTENCY_RETENTION_BOUNDED", result.EvidenceRefs);
        Assert.Equal(bool.TrueString, result.BoundaryMetadata["model_activation_idempotency_retention_bounded"]);
        Assert.Equal(bool.FalseString, result.BoundaryMetadata["model_activation_idempotency_retention_unbounded"]);
        Assert.Equal(bool.FalseString, result.BoundaryMetadata["model_activation_idempotency_infinite_retention_claimed"]);
        Assert.Equal(
            retainedOperationCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
            result.BoundaryMetadata["model_activation_idempotency_retained_operation_count"]);
        Assert.Equal(
            retainedOperationCountPerTenant.ToString(System.Globalization.CultureInfo.InvariantCulture),
            result.BoundaryMetadata["model_activation_idempotency_retained_operation_count_per_tenant"]);
    }

    private static byte[] InjectInvalidUtf8IntoJsonString(byte[] sourceBytes, string stringValue)
    {
        var valueBytes = Encoding.UTF8.GetBytes(stringValue);
        var valueIndex = IndexOfBytes(sourceBytes, valueBytes);
        Assert.True(valueIndex >= 0, $"Could not find JSON string value to corrupt: {stringValue}");

        var invalidBytes = sourceBytes.ToArray();
        invalidBytes[valueIndex] = 0xC3;
        return invalidBytes;
    }

    private static int IndexOfBytes(byte[] sourceBytes, byte[] valueBytes)
    {
        if (valueBytes.Length == 0 || valueBytes.Length > sourceBytes.Length)
            return -1;

        for (var index = 0; index <= sourceBytes.Length - valueBytes.Length; index++)
        {
            var matches = true;
            for (var valueIndex = 0; valueIndex < valueBytes.Length; valueIndex++)
            {
                if (sourceBytes[index + valueIndex] == valueBytes[valueIndex])
                    continue;

                matches = false;
                break;
            }

            if (matches)
                return index;
        }

        return -1;
    }

    private static void AssertSecondActivationStillActive(
        OperationResult<DesignConversionModelActivationResult> active,
        DesignConversionPromotedModelActivationRecord first,
        DesignConversionPromotedModelActivationRecord second)
    {
        Assert.True(active.IsSuccess, active.ErrorMessage);
        Assert.Equal(2, active.Value!.Generation);
        Assert.Equal(second.Model.ModelId, active.Value.ActiveVersion.ModelId);
        Assert.NotNull(active.Value.PreviousVersion);
        Assert.Equal(first.Model.ModelId, active.Value.PreviousVersion!.ModelId);
    }

    private static async Task<DesignConversionPromotedModelActivationRecord> BuildActivationRecordAsync(
        string root,
        string tenantId,
        string name,
        int batchCount)
    {
        var outputDirectory = Path.Combine(root, "models", name);
        Directory.CreateDirectory(outputDirectory);
        var machine = new DesignConversionMachine();
        for (var index = 0; index < batchCount; index++)
        {
            var trained = await machine.TrainBatchAsync(new LearningBatch<DesignDomain>(
                BatchId: $"{name}-batch-{index}",
                DomainTag: DesignConversionMachine.DomainTag,
                TrainingItems:
                [
                    new LearningTrainingItem(
                        ItemId: $"{name}-item-{index}",
                        Prompt: JsonSerializer.Serialize(new Dictionary<string, object>
                        {
                            ["unit_id"] = DesignConversionRuntimeScaffoldCriticUnit.UnitId,
                            ["source_kind"] = "Figma",
                            ["source_manifest_hash"] = $"{name}-manifest-source-{index}",
                            ["source_identity_hash"] = $"{name}-identity-{index}",
                            ["source_structure_hash"] = $"{name}-structure-{index}",
                            ["pair_family"] = "visual"
                        }),
                        ChosenResponse: JsonSerializer.Serialize(new Dictionary<string, object>
                        {
                            ["visual_score"] = 0.96,
                            ["accepted"] = true
                        }),
                        RejectedResponse: JsonSerializer.Serialize(new Dictionary<string, object>
                        {
                            ["visual_score"] = 0.12,
                            ["accepted"] = false
                        }))
                ],
                SourceIds: [$"{name}-source-{index}"]));
            Assert.True(trained.IsSuccess, trained.ErrorMessage);
        }

        var checkpointPath = Path.Combine(outputDirectory, $"{name}-checkpoint.json");
        var save = await machine.SaveCheckpointAsync(checkpointPath);
        Assert.True(save.IsSuccess, save.ErrorMessage);
        var load = await DesignConversionMachine.LoadCheckpointAsync(checkpointPath);
        Assert.True(load.IsSuccess, load.ErrorMessage);
        Assert.True(DesignConversionMachine.CheckpointHasRealLearnedWeights(load.Value!.Checkpoint));

        var tokenizerPath = Path.Combine(outputDirectory, $"{name}-tokenizer.json");
        var policyPath = Path.Combine(outputDirectory, $"{name}-policy.json");
        await File.WriteAllTextAsync(tokenizerPath, "{\"kind\":\"activation-test-tokenizer\"}");
        await File.WriteAllTextAsync(policyPath, "{\"kind\":\"activation-test-policy\"}");

        var checkpointFullPath = Path.GetFullPath(checkpointPath);
        var checkpointSha = await ReadSha256Async(checkpointFullPath);
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
            MachineId: load.Value.Checkpoint.MachineId,
            DomainTag: load.Value.Checkpoint.DomainTag,
            Version: load.Value.Checkpoint.Version,
            CheckpointRef: checkpointFullPath,
            ManifestRef: manifestRef,
            CreatedAtUtc: DateTimeOffset.Parse("2026-07-10T10:00:00Z"),
            AcceptedRecordCount: batchCount,
            QuarantinedRecordCount: 0,
            UnitIds: [DesignConversionRuntimeScaffoldCriticUnit.UnitId],
            Batches:
            [
                new DesignConversionTrainingManifestBatch(
                    BatchId: $"{name}-manifest-batch",
                    UnitId: DesignConversionRuntimeScaffoldCriticUnit.UnitId,
                    RecordCount: batchCount,
                    RecordIds: Enumerable.Range(0, batchCount).Select(index => $"{name}-record-{index}").ToArray(),
                    SourceIds: Enumerable.Range(0, batchCount).Select(index => $"{name}-source-{index}").ToArray())
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
        var decision = new DesignConversionPromotionDecision(
            DesignConversionPromotionDecisionKind.Promote,
            ["ALL_PROMOTION_GATES_PASSED"],
            ["DESIGN_CONVERSION_PROMOTION_GATES_PASSED"]);
        var receipt = new DesignConversionPromotionUploadReceipt(
            UploadId: $"{name}-local-receipt",
            RemoteRef: promotionRef,
            UploadedAtUtc: DateTimeOffset.Parse("2026-07-10T10:00:00Z"),
            EvidenceRefs: ["DESIGN_CONVERSION_LOCAL_PROMOTION_RECEIPT_WRITTEN"]);
        var publication = new DesignConversionPromotionPublicationResult(decision, manifest, receipt);
        var modelId = $"{manifestId}:{checkpointSha[..12]}";
        var model = new DesignConversionPromotedLocalModel(
            ModelId: modelId,
            ManifestId: manifest.ManifestId,
            ManifestRef: manifest.ManifestRef,
            CheckpointPath: checkpointFullPath,
            CheckpointSha256: checkpointSha,
            MachineId: load.Value.Checkpoint.MachineId,
            DomainTag: load.Value.Checkpoint.DomainTag,
            Version: load.Value.Checkpoint.Version,
            LearnedWeightArtifactCount: DesignConversionMachine.CountRealLearnedWeightArtifacts(load.Value.Checkpoint),
            LearnedWeightObservationCount: DesignConversionMachine.CountRealLearnedWeightObservations(load.Value.Checkpoint),
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

        var record = new DesignConversionPromotedModelActivationRecord(
            DesignConversionModelActivationRegistry.BuildTenantIdRef(tenantId),
            publication,
            model);
        await PersistPromotionAuthorityRecordAsync(root, tenantId, name, record, batchCount);

        return record;
    }

    private static async Task PersistPromotionAuthorityRecordAsync(
        string root,
        string tenantId,
        string name,
        DesignConversionPromotedModelActivationRecord record,
        int batchCount)
    {
        var promotion = BuildPromotionAuthorityRecord(tenantId, name, record, batchCount);
        var journalPath = PromotionJournalPath(root);
        Directory.CreateDirectory(Path.GetDirectoryName(journalPath)!);
        await File.AppendAllTextAsync(
            journalPath,
            JsonSerializer.Serialize(promotion, PromotionJournalJsonOptions) + Environment.NewLine,
            StrictUtf8);
    }

    private static string PromotionJournalPath(string root)
        => Path.Combine(
            ArtifactDbRoot(root),
            JsonlDesignConversionLocalArtifactDb.TrainingIntakeLocalPromotionFileName);

    private static DesignConversionTrainingIntakeLocalPromotionResult BuildPromotionAuthorityRecord(
        string tenantId,
        string name,
        DesignConversionPromotedModelActivationRecord record,
        int batchCount)
    {
        var run = new DesignConversionTrainingIntakeRunResult(
            RunId: $"{name}-registry-authority-run",
            TrainingIntakeId: $"{name}-registry-authority-intake",
            PreflightId: $"{name}-registry-authority-preflight",
            AcceptedCandidateCount: batchCount,
            ResolvedTrainingRecordCount: batchCount,
            MayStartTraining: true,
            SourceManifestStatus: "proven",
            SourceDisjointStatus: "proven",
            TrainingRunnerInvoked: true,
            LocalLifecycleHookAttempted: true,
            TrainingStarted: true,
            CheckpointMutated: true,
            CheckpointCandidateCreated: true,
            WeightsProduced: true,
            ModelUploadReady: true,
            TrainingExecutionBoundary: "model_activation_registry_test_server_owned_promotion_authority",
            TrainingExecutionStatus: "completed",
            RunnerBlockedReason: string.Empty,
            CheckpointStatus: "created",
            CheckpointMutationStatus: "mutated",
            WeightProofStatus: "real_weights_proven",
            MeasurementGateStatus: DesignConversionMeasurementGateStatus.Passed,
            PromotionGateStatus: "PROMOTION_ALLOWED_BY_MEASUREMENT_GATE_AND_REAL_WEIGHTS",
            UploadReadinessStatus: "upload_ready_local_promotion_gate_passed_no_external_upload",
            PromotionBlockedReasons: [],
            RunnerEvidence: null,
            Warnings: [],
            BlockReasons: [],
            BoundaryMetadata: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["product_ready"] = bool.FalseString
            });

        return new DesignConversionTrainingIntakeLocalPromotionResult(
            TrainingRun: run,
            LocalPromotionReady: true,
            ExternalCloudUploadAttempted: false,
            PromotionOutputDirectory: Path.GetDirectoryName(record.Model.CheckpointPath) ?? string.Empty,
            PromotedCheckpointPath: record.Model.CheckpointPath,
            Publication: record.Publication,
            PromotedLocalModel: record.Model,
            EvidenceRefs:
            [
                "DESIGN_CONVERSION_TRAINING_INTAKE_LOCAL_PROMOTION_READY",
                "DESIGN_CONVERSION_LOCAL_PROMOTION_ACTIVE_STATE_RECORDED",
                "DESIGN_CONVERSION_MODEL_ACTIVATION_REGISTRY_TEST_DURABLE_PROMOTION_RECORD"
            ],
            Warnings: ["local_promotion_ready"],
            BlockReasons: [],
            BoundaryMetadata: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["schema_version"] = JsonlDesignConversionLocalArtifactDb.TrainingIntakeLocalPromotionSchemaVersion,
                ["training_intake_tenant_id_ref"] = DesignConversionModelActivationRegistry.BuildTenantIdRef(tenantId),
                ["local_promotion_record_kind"] = "promotion",
                ["local_promotion_ready"] = bool.TrueString,
                ["local_promotion_not_ready"] = bool.FalseString,
                ["local_promotion_active_local_model_ref"] = record.Publication.Manifest.PromotionUploadRef ?? string.Empty,
                ["local_promotion_active_model_id"] = record.Model.ModelId,
                ["local_promotion_active_checkpoint_sha256"] = record.Model.CheckpointSha256,
                ["promotion_upload_ref"] = record.Publication.Manifest.PromotionUploadRef ?? string.Empty,
                ["local_promoted_model_id"] = record.Model.ModelId,
                ["local_promoted_checkpoint_sha256"] = record.Model.CheckpointSha256,
                ["deployment_verified"] = bool.FalseString,
                ["product_ready"] = bool.FalseString
            });
    }

    private static string NewRoot()
        => Path.Combine(Path.GetTempPath(), "llm_mvp_core", "design-conversion-model-activation", Guid.NewGuid().ToString("N"));

    private static string ActivationStatePath(string root)
        => Path.Combine(Path.GetFullPath(root), "design-conversion-model-activation-state.json");

    private static string ActivationMutationLockPath(string root)
        => Path.Combine(Path.GetFullPath(root), DesignConversionModelActivationRegistry.MutationLockFileName);

    private static FileStream HoldActivationMutationLease(string root)
    {
        var lockPath = ActivationMutationLockPath(root);
        Directory.CreateDirectory(Path.GetDirectoryName(lockPath)!);
        return new FileStream(
            lockPath,
            FileMode.OpenOrCreate,
            FileAccess.ReadWrite,
            FileShare.None,
            bufferSize: 1,
            FileOptions.None);
    }

    private static async Task<string> ReadSha256Async(string path)
    {
        var bytes = await File.ReadAllBytesAsync(path);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }
}
