// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;
using AgentInfrastructure;
using ContinuousLearning;
using LoRA;
using TransformerModel;
using TransformerModel.Config;

namespace LlmFromScratch.Tests;

public sealed class DpoPairTransactionRegressionTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        $"dpo-transaction-regressions-{Guid.NewGuid():N}");

    [Fact]
    public void MutationStartedWithoutExactStage_RequiresRecoveryAndRejectsConsumedRequest()
    {
        var store = Store("recovery");
        Assert.True(store.Add(Pair("BUG", "recovery-request")).IsSuccess);
        var transaction = Assert.IsType<FileDpoPairStore.TransactionHandle>(
            store.PrepareTransaction(["BUG"]).Value);
        var seed = Seed(FileDpoPairStore.NoParentAdapterIdentity);
        using (var prepared = transaction.PrepareCandidate("BUG", seed).Value!)
            Assert.Null(prepared.ExistingArtifact);

        Assert.True(transaction.BeginMutation("BUG").IsSuccess);

        var retried = transaction.PrepareCandidate("BUG", seed);
        Assert.False(retried.IsSuccess);
        Assert.Equal("DPO_TRANSACTION_RECOVERY_REQUIRED", retried.ErrorCode);

        var snapshot = transaction.Read().Value!;
        var pairs = snapshot.Pairs.Select(static value => value.Pair).ToArray();
        var content = DpoPairTrainingContentAuthority.Project(pairs).Value!;
        var admission = transaction.ValidateTeacherBatch(
            "BUG",
            content,
            pairs.Select(static value => value.TeacherProvenance).ToArray());
        Assert.False(admission.IsSuccess);
        Assert.Equal("DPO_TRANSACTION_RECOVERY_REQUIRED", admission.ErrorCode);
    }

    [Fact]
    public void CandidateSeed_RejectsDifferentExactParentAdapterIdentity()
    {
        var store = Store("parent-identity");
        Assert.True(store.Add(Pair("BUG", "parent-request")).IsSuccess);
        var transaction = Assert.IsType<FileDpoPairStore.TransactionHandle>(
            store.PrepareTransaction(["BUG"]).Value);
        using (transaction.PrepareCandidate("BUG", Seed(new string('b', 64))).Value!)
        {
        }

        var changed = transaction.PrepareCandidate(
            "BUG",
            Seed(new string('c', 64)));

        Assert.False(changed.IsSuccess);
        Assert.Equal("DPO_TRANSACTION_INPUT_CHANGED", changed.ErrorCode);
    }

    [Fact]
    public void PrepareTransaction_FiltersUnroutablePairsBeforeBoundedSelection()
    {
        var store = Store("routing");
        Assert.True(store.Add(Pair("ORPHAN", "orphan-request")).IsSuccess);
        Assert.True(store.Add(Pair("BUG", "bug-request")).IsSuccess);

        var prepared = store.PrepareTransaction(["BUG"]);

        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
        var transaction = Assert.IsType<FileDpoPairStore.TransactionHandle>(
            prepared.Value);
        var selected = Assert.Single(transaction.Read().Value!.Pairs);
        Assert.Equal("BUG", selected.Pair.Domain);
        var pending = Assert.Single(store.GetAll().Value!);
        Assert.Equal("ORPHAN", pending.Domain);
    }

    [Fact]
    public void FullPendingPairCapacity_CanStillReserveTransactionForDrain()
    {
        var store = new FileDpoPairStore(
            Path.Combine(_root, "capacity"),
            maximumRecordCount: 1);
        Assert.True(store.Add(Pair("BUG", "capacity-request")).IsSuccess);

        var prepared = store.PrepareTransaction(["BUG"]);

        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
        Assert.NotNull(prepared.Value);
        Assert.Empty(store.GetAll().Value!);
    }

    [Fact]
    public void ReplayLedgerCapacity_IsReservedBeforeNewAdmission()
    {
        Assert.True(FileDpoPairStore.HasReplayLedgerEntryCapacity(
            FileDpoPairStore.MaximumReplayLedgerEntryCount - 1,
            requestedEntryCount: 1));
        Assert.False(FileDpoPairStore.HasReplayLedgerEntryCapacity(
            FileDpoPairStore.MaximumReplayLedgerEntryCount,
            requestedEntryCount: 1));
        Assert.False(FileDpoPairStore.HasReplayLedgerEntryCapacity(
            FileDpoPairStore.MaximumReplayLedgerEntryCount - 1,
            requestedEntryCount: 2));
    }

    [Fact]
    public void CandidatePreparation_ReservesLargerFutureDurableState()
    {
        var pair = Pair("BUG", "future-capacity-request");
        var seed = Seed(FileDpoPairStore.NoParentAdapterIdentity);
        var baseline = Store("future-capacity-baseline");
        Assert.True(baseline.Add(pair).IsSuccess);
        var baselineTransaction = Assert.IsType<
            FileDpoPairStore.TransactionHandle>(
            baseline.PrepareTransaction(["BUG"]).Value);
        using (var prepared = baselineTransaction.PrepareCandidate("BUG", seed).Value!)
        {
        }
        var candidatePreparedPath = Assert.Single(Directory.EnumerateFiles(
            Path.Combine(_root, "future-capacity-baseline"),
            "*.dpo-transaction.json"));
        var candidatePreparedLength = new FileInfo(candidatePreparedPath).Length;

        var constrained = new FileDpoPairStore(
            Path.Combine(_root, "future-capacity-constrained"),
            maximumTransactionRecordBytesForTesting: candidatePreparedLength);
        Assert.True(constrained.Add(pair).IsSuccess);
        var constrainedTransaction = Assert.IsType<
            FileDpoPairStore.TransactionHandle>(
            constrained.PrepareTransaction(["BUG"]).Value);

        var rejected = constrainedTransaction.PrepareCandidate("BUG", seed);

        Assert.False(rejected.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_CAPACITY", rejected.ErrorCode);
        var domain = Assert.Single(constrainedTransaction.Read().Value!.Domains);
        Assert.Equal(
            FileDpoPairStore.DpoTransactionDomainState.Prepared,
            domain.State);
        Assert.False(constrainedTransaction.Read().Value!.MutationStarted);
    }

    [Fact]
    public void PublishedTransaction_RestartRetainsReplayLedgerAndCandidate()
    {
        var pair = Pair("BUG", "lifecycle-request");
        var store = Store("lifecycle-pairs");
        var transaction = PrepareTransaction(store, pair);
        var artifact = CompleteCandidate(transaction);
        var adapter = Adapter(transaction, artifact);
        var registry = new DomainModelRegistry(
            new DpoTransactionNullStorage(),
            Path.Combine(_root, "lifecycle-registry"));

        var published = ((IDpoRegistryPublicationTransaction)registry)
            .PublishBatchAndCommit(
                [adapter],
                durable =>
                {
                    var exact = durable.Single();
                    var recorded = transaction.RecordPublished(
                        exact.Domain,
                        exact.Provenance!);
                    return recorded.IsSuccess
                        ? transaction.CommitDrain()
                        : recorded;
                });

        Assert.True(published.IsSuccess, published.ErrorMessage);
        Assert.True(File.Exists(artifact.AdapterPath));
        var restarted = Store("lifecycle-pairs");
        var pairId = restarted.Add(pair);
        Assert.True(pairId.IsSuccess, pairId.ErrorMessage);
        Assert.Empty(restarted.GetAll().Value!);
        var duplicateRequest = restarted.Add(
            Pair("BUG", "lifecycle-request", "-different-content"));
        Assert.False(duplicateRequest.IsSuccess);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.DuplicateRequestIdErrorCode,
            duplicateRequest.ErrorCode);
        using var ledger = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(
            _root,
            "lifecycle-pairs",
            ".dpo-replay-ledger.json")));
        var entry = Assert.Single(
            ledger.RootElement.GetProperty("entries").EnumerateArray());
        Assert.Equal(pairId.Value, entry.GetProperty("pairId").GetString());
        Assert.Equal(
            pair.TeacherProvenance!.RequestId,
            entry.GetProperty("requestId").GetString());
        Assert.Equal(
            pair.TeacherProvenance.TrainingContentSha256,
            entry.GetProperty("contentSha256").GetString());
        Assert.NotNull(new DomainModelRegistry(
                new DpoTransactionNullStorage(),
                Path.Combine(_root, "lifecycle-registry"))
            .GetAdapter("BUG").Value);
    }

    [Fact]
    public void InterruptedCompaction_IdempotentRetryConverges()
    {
        var interrupt = 1;
        var pairRoot = Path.Combine(_root, "compaction-retry-pairs");
        var store = new FileDpoPairStore(
            pairRoot,
            afterReplayLedgerWrittenBeforePairCompactionForTesting: () =>
            {
                if (Interlocked.Exchange(ref interrupt, 0) == 1)
                    throw new IOException("Injected compaction interruption.");
            });
        var pair = Pair("BUG", "compaction-retry-request");
        var transaction = PrepareTransaction(store, pair);
        var artifact = CompleteCandidate(transaction);
        var adapter = Adapter(transaction, artifact);
        var registry = new DomainModelRegistry(
            new DpoTransactionNullStorage(),
            Path.Combine(_root, "compaction-retry-registry"));

        var interrupted = ((IDpoRegistryPublicationTransaction)registry)
            .PublishBatchAndCommit(
                [adapter],
                durable =>
                {
                    var exact = durable.Single();
                    var recorded = transaction.RecordPublished(
                        exact.Domain,
                        exact.Provenance!);
                    return recorded.IsSuccess
                        ? transaction.CommitDrain()
                        : recorded;
                });

        Assert.False(interrupted.IsSuccess);
        Assert.True(File.Exists(Path.Combine(
            pairRoot,
            ".dpo-replay-ledger.json")));
        Assert.Single(Directory.EnumerateFiles(
            pairRoot,
            "*.dpo-transaction.json"));
        Assert.True(File.Exists(artifact.AdapterPath));

        var retry = transaction.CommitDrain();

        Assert.True(retry.IsSuccess, retry.ErrorMessage);
        Assert.Empty(Directory.EnumerateFiles(
            pairRoot,
            "*.dpo-transaction.json"));
        Assert.True(File.Exists(artifact.AdapterPath));
        var restarted = new FileDpoPairStore(pairRoot);
        Assert.True(restarted.Add(pair).IsSuccess);
        Assert.Empty(restarted.GetAll().Value!);
    }

    [Fact]
    public void SchemaV3AdapterTransaction_RemainsReadableAsAdapterPublication()
    {
        var root = Path.Combine(_root, "schema-v3-adapter");
        var store = new FileDpoPairStore(root);
        var transaction = PrepareTransaction(
            store,
            Pair("BUG", "schema-v3-request"));
        var path = Assert.Single(Directory.EnumerateFiles(
            root,
            "*.dpo-transaction.json"));
        var json = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        json["schemaVersion"] = 3;
        json.Remove("kind");
        json.Remove("externalConsumption");
        foreach (var pair in json["pairs"]!.AsArray())
        {
            var value = pair!.AsObject();
            value.Remove("groupId");
            value.Remove("groupOrdinal");
            value.Remove("groupRole");
        }
        foreach (var request in json["requests"]!.AsArray())
        {
            var value = request!.AsObject();
            value.Remove("groupId");
            value.Remove("pairIds");
            value.Remove("contentSha256s");
        }
        RewriteHistoricalPreparedTransaction(path, json, atomicGroup: false);

        var resumed = new FileDpoPairStore(root).PrepareTransaction(["BUG"]);

        Assert.True(resumed.IsSuccess, resumed.ErrorMessage);
        Assert.Equal(transaction.TransactionId, resumed.Value!.TransactionId);
        Assert.Equal(
            FileDpoPairStore.DpoTransactionState.Prepared,
            resumed.Value.Read().Value!.State);
    }

    [Fact]
    public void ReplaySchemaV1_RemainsReadableAndReservesExactPair()
    {
        var root = Path.Combine(_root, "replay-v1");
        var store = new FileDpoPairStore(root);
        var pair = Pair("BUG", "replay-v1-request");
        var pairId = store.Add(pair).Value!;
        var replay = new JsonObject
        {
            ["schemaVersion"] = 1,
            ["entries"] = new JsonArray
            {
                new JsonObject
                {
                    ["pairId"] = pairId,
                    ["requestId"] = pair.TeacherProvenance!.RequestId,
                    ["contentSha256"] =
                        pair.TeacherProvenance.TrainingContentSha256,
                },
            },
            ["recordSha256"] = string.Empty,
        };
        RewriteHashedJson(
            Path.Combine(root, ".dpo-replay-ledger.json"),
            replay);

        var prepared = new FileDpoPairStore(root).PrepareTransaction(["BUG"]);

        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
        Assert.Null(prepared.Value);
    }

    [Fact]
    public void LegacyFixture_DoesNotCallCurrentTransactionHasher()
    {
        var fixture = typeof(DpoPairTransactionRegressionTests).GetMethod(
            nameof(RewriteHistoricalPreparedTransaction),
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var productionHasher = typeof(FileDpoPairStore).GetMethod(
            "Compute" + "TransactionRecordSha256",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var il = fixture.GetMethodBody()!.GetILAsByteArray()!;
        var token = BitConverter.GetBytes(productionHasher.MetadataToken);

        Assert.False(ContainsBytes(il, token));
        Assert.DoesNotContain(
            "Compute" + "TransactionRecordSha256",
            LoadedStrings(fixture));
    }

    private FileDpoPairStore Store(string name)
        => new(Path.Combine(_root, name));

    private FileDpoPairStore.TransactionHandle PrepareTransaction(
        FileDpoPairStore store,
        DpoPair pair)
    {
        Assert.True(store.Add(pair).IsSuccess);
        return Assert.IsType<FileDpoPairStore.TransactionHandle>(
            store.PrepareTransaction([pair.Domain]).Value);
    }

    private static TrainedAdapterArtifact CompleteCandidate(
        FileDpoPairStore.TransactionHandle transaction)
    {
        var seed = Seed(FileDpoPairStore.NoParentAdapterIdentity);
        var prepared = transaction.PrepareCandidate("BUG", seed);
        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
        // Поток файла закрывается внутри вспомогательного метода до начала
        // изменения данных, но само резервирование кандидата держим до конца
        // операции.
        using var reservation = prepared.Value!;
        var model = new LlmModel(ModelConfiguration());
        _ = LoraApplicator.Apply(model, LoraConfiguration());
        SaveAdapterCheckpointOwnerOnly(model, reservation.StagingPath);
        Assert.True(transaction.BeginMutation("BUG").IsSuccess);
        var completed = transaction.CompleteCandidate("BUG");
        Assert.True(completed.IsSuccess, completed.ErrorMessage);
        return completed.Value!;
    }

    private static ModelAdapter Adapter(
        FileDpoPairStore.TransactionHandle transaction,
        TrainedAdapterArtifact artifact)
    {
        var domain = Assert.Single(transaction.Read().Value!.Domains);
        return new ModelAdapter(
            domain.Domain,
            artifact.AdapterPath,
            domain.TrainingExamples,
            domain.TrainedAt,
            artifact.Provenance);
    }

    private static FileDpoPairStore.CandidateProvenanceSeed Seed(
        string parentAdapterIdentity)
    {
        var manifest = TrainingSourceProvenance.BuildManifest(
            [TrainingSourceProvenance.CreateSourceId(
                "tests/dpo-transaction",
                "exact supervised bytes")]).Value!;
        var model = TrainingSourceProvenance.Identify(ModelConfiguration());
        var lora = TrainingSourceProvenance.Identify(LoraConfiguration());
        return new FileDpoPairStore.CandidateProvenanceSeed(
            manifest,
            parentAdapterIdentity,
            new string('d', 64),
            new string('e', 64),
            model,
            lora,
            1,
            new string('f', 64));
    }

    private static ModelConfig ModelConfiguration() => new()
    {
        VocabSize = 256,
        ContextLen = 16,
        HiddenDim = 8,
        NumLayers = 1,
        NumHeads = 2,
        NumKvHeads = 1,
        FfnHiddenDim = 16,
    };

    private static LoraConfig LoraConfiguration() => new()
    {
        Rank = 2,
        Alpha = 4f,
        Dropout = 0.05f,
        TargetModules = ["q_proj", "v_proj"],
    };

    private static DpoPair Pair(
        string domain,
        string requestId,
        string promptSuffix = "")
    {
        var pair = new DpoPair(
            $"prompt-{requestId}{promptSuffix}",
            "chosen",
            "rejected",
            domain,
            DateTimeOffset.UnixEpoch)
        {
            TeacherProvenance = new TrainingTeacherProvenance(
                TrainingTeacherSourceKind.ExternalTeacherOutput,
                OpenWeightTeacherFamily.Qwen,
                TrainingTeacherAccessMode.LocalHostedOpenWeight,
                TrainingTeacherHostingProvider.Ollama,
                "qwen3.5:9b",
                new string('a', 64),
                TrainingTeacherModelLicense.Apache20,
                ProviderTermsChecked: false,
                ProviderTermsAllowTraining: false,
                ClosedProprietaryModel: false,
                TeacherOutputUsedForTraining: true,
                requestId,
                InvocationCount: 1)
            {
                ModelArtifactDigestKind =
                    LegalTrainingTeacherAdmissionAuthority
                        .OllamaModelLayerSha256DigestKind,
                ModelArtifactDigest = new string('1', 64),
            },
            GovernanceDecision = new TrainingDataGovernanceDecision(
                true,
                false,
                [$"sha256:{new string('2', 64)}"]),
        };
        var bound = DpoPairTrainingContentAuthority.Bind(pair);
        Assert.True(bound.IsSuccess, bound.ErrorMessage);
        var boundPair = bound.Value!;
        return boundPair with
        {
            ChosenAuthorship = new DpoPairSideAuthorship(
                DPO.Models.DpoPairSideAuthorKind.OpenTeacher,
                boundPair.TeacherProvenance),
            RejectedAuthorship = new DpoPairSideAuthorship(
                DPO.Models.DpoPairSideAuthorKind.LocalModel),
        };
    }

    private static void RewriteHashedJson(string path, JsonObject json)
    {
        json["recordSha256"] = string.Empty;
        var canonical = json.ToJsonString();
        json["recordSha256"] = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(canonical)))
            .ToLowerInvariant();
        WriteOwnerOnlyText(path, json.ToJsonString());
    }

    private static void WriteOwnerOnlyText(string path, string content)
    {
        // Этот тест вручную создаёт старый журнал повтора. Записываем его в
        // файл с доступом только владельцу, чтобы проверка читала именно
        // исторический JSON, а не падала раньше на правах временного файла.
        using var stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(
            path,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            FileOptions.WriteThrough);
        var bytes = Encoding.UTF8.GetBytes(content);
        stream.Write(bytes);
        stream.Flush(flushToDisk: true);
    }

    private static void SaveAdapterCheckpointOwnerOnly(LlmModel model, string path)
    {
        // Файл сохранённых параметров модели затем проверяется защищённым
        // хранилищем. Записываем его в уже открытый защищённый поток и
        // закрываем до начала изменения данных, чтобы обычная запись по имени
        // не оставила временный файл с неподходящими правами.
        using var stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(
            path,
            FileMode.CreateNew,
            FileAccess.ReadWrite,
            FileShare.None,
            FileOptions.WriteThrough);
        model.SaveAdapterCheckpoint(stream);
        stream.Flush(flushToDisk: true);
    }

    internal static void RewriteHistoricalPreparedTransaction(
        string path,
        JsonObject json,
        bool atomicGroup)
    {
        json["recordSha256"] = string.Empty;
        var canonical = HistoricalPreparedTransactionBytes(json, atomicGroup);
        json["recordSha256"] = Convert.ToHexString(SHA256.HashData(canonical))
            .ToLowerInvariant();
        File.WriteAllBytes(path, HistoricalPreparedTransactionBytes(json, atomicGroup));
    }

    private static byte[] HistoricalPreparedTransactionBytes(
        JsonObject json,
        bool atomicGroup)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        writer.WriteStartObject();
        writer.WriteNumber("schemaVersion", json["schemaVersion"]!.GetValue<int>());
        writer.WriteString("transactionId", json["transactionId"]!.GetValue<string>());
        writer.WriteString("pairBatchSha256", json["pairBatchSha256"]!.GetValue<string>());
        writer.WriteString(
            "preparedAt",
            DateTimeOffset.Parse(
                json["preparedAt"]!.GetValue<string>(),
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind));
        writer.WriteNumber("state", json["state"]!.GetValue<int>());
        writer.WriteStartArray("pairs");
        foreach (var pairNode in json["pairs"]!.AsArray())
        {
            var pair = pairNode!.AsObject();
            writer.WriteStartObject();
            writer.WriteString("pairId", pair["pairId"]!.GetValue<string>());
            writer.WriteString("domain", pair["domain"]!.GetValue<string>());
            if (atomicGroup)
            {
                writer.WriteString("groupId", pair["groupId"]!.GetValue<string>());
                writer.WriteNumber("groupOrdinal", pair["groupOrdinal"]!.GetValue<int>());
                writer.WriteNumber("groupRole", pair["groupRole"]!.GetValue<int>());
            }
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteStartArray("domains");
        foreach (var domainNode in json["domains"]!.AsArray())
        {
            var domain = domainNode!.AsObject();
            writer.WriteStartObject();
            writer.WriteString("domain", domain["domain"]!.GetValue<string>());
            writer.WriteNumber("state", domain["state"]!.GetValue<int>());
            writer.WriteNull("seed");
            writer.WriteString("adapterSha256", domain["adapterSha256"]!.GetValue<string>());
            writer.WriteString(
                "trainedAt",
                DateTimeOffset.Parse(
                    domain["trainedAt"]!.GetValue<string>(),
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.RoundtripKind));
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteStartArray("requests");
        foreach (var requestNode in json["requests"]!.AsArray())
        {
            var request = requestNode!.AsObject();
            writer.WriteStartObject();
            writer.WriteString("requestId", request["requestId"]!.GetValue<string>());
            writer.WriteString("contentSha256", request["contentSha256"]!.GetValue<string>());
            writer.WriteString("pairId", request["pairId"]!.GetValue<string>());
            writer.WriteNumber("state", request["state"]!.GetValue<int>());
            if (atomicGroup)
            {
                writer.WriteString("groupId", request["groupId"]!.GetValue<string>());
                writer.WriteStartArray("pairIds");
                foreach (var value in request["pairIds"]!.AsArray())
                    writer.WriteStringValue(value!.GetValue<string>());
                writer.WriteEndArray();
                writer.WriteStartArray("contentSha256s");
                foreach (var value in request["contentSha256s"]!.AsArray())
                    writer.WriteStringValue(value!.GetValue<string>());
                writer.WriteEndArray();
            }
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteString("recordSha256", json["recordSha256"]!.GetValue<string>());
        writer.WriteEndObject();
        writer.Flush();
        return stream.ToArray();
    }

    private static bool ContainsBytes(byte[] bytes, byte[] candidate)
    {
        for (var index = 0; index <= bytes.Length - candidate.Length; index++)
        {
            if (bytes.AsSpan(index, candidate.Length).SequenceEqual(candidate))
                return true;
        }
        return false;
    }

    private static IReadOnlyList<string> LoadedStrings(MethodInfo method)
    {
        var il = method.GetMethodBody()!.GetILAsByteArray()!;
        var values = new List<string>();
        for (var index = 0; index <= il.Length - 5; index++)
        {
            if (il[index] != 0x72)
                continue;
            try
            {
                values.Add(method.Module.ResolveString(
                    BitConverter.ToInt32(il, index + 1)));
            }
            catch (ArgumentException)
            {
                // Not an ldstr token boundary; continue the static scan.
            }
        }
        return values;
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }
}

file sealed class DpoTransactionNullStorage : ISessionStorage
{
    public Task<OperationResult<bool>> SaveAsync(
        string sessionId,
        AgentState state)
        => Task.FromResult(OperationResult<bool>.Ok(true));

    public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
        => Task.FromResult(OperationResult<AgentState>.Fail(
            "Not persisted.",
            "NULL_STORAGE"));

    public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
        => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok([]));
}
