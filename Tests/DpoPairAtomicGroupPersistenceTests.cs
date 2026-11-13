// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using System.Text.Json.Nodes;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;
using ContinuousLearning;
using LoRA;
using TransformerModel;
using TransformerModel.Config;

namespace LlmFromScratch.Tests;

public sealed class DpoPairAtomicGroupPersistenceTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        $"dpo-atomic-group-{Guid.NewGuid():N}");

    [Fact]
    public void AtomicAdd_RestartReturnsExactlyTwoChildrenInTargetOrder()
    {
        var group = Group("atomic-restart");
        var store = Store("atomic-restart");

        var added = store.AddAtomicGroup(group);

        Assert.True(added.IsSuccess, added.ErrorMessage);
        Assert.Equal(group.GroupCommitmentSha256, added.Value!.GroupCommitmentSha256);
        Assert.Single(Directory.EnumerateFiles(
            Path.Combine(_root, "atomic-restart"),
            "*.dpo-pair-group.json"));
        Assert.Empty(Directory.EnumerateFiles(
            Path.Combine(_root, "atomic-restart"),
            "*.dpo-pair.json"));
        var pairs = Store("atomic-restart").GetAll().Value!;
        Assert.Equal(2, pairs.Count);
        Assert.Equal("critic-prompt", pairs[0].Prompt);
        Assert.Equal("improver-prompt", pairs[1].Prompt);
    }

    [Fact]
    public void InvalidSecondChild_ChangesNeitherDurableQueueNorGroupBytes()
    {
        var group = Group("invalid-second");
        var children = group.Children.ToArray();
        children[1] = children[1] with
        {
            Pair = children[1].Pair with { Rejected = "substituted" },
        };
        var invalid = group with { Children = Array.AsReadOnly(children) };
        var root = Path.Combine(_root, "invalid-second");
        var store = new FileDpoPairStore(root);

        var added = store.AddAtomicGroup(invalid);

        Assert.False(added.IsSuccess);
        Assert.Equal("DPO_PAIR_ATOMIC_GROUP_INVALID", added.ErrorCode);
        Assert.False(Directory.Exists(root));
    }

    [Fact]
    public void SameTargetReorderThirdAndMixedInvocation_AllFailBeforeStorage()
    {
        var group = Group("shape-negative");
        var children = group.Children.ToArray();
        var sameTarget = group with
        {
            Children = Array.AsReadOnly([
                children[0],
                children[0] with
                {
                    Ordinal = 1,
                    Role = DpoPairAtomicGroupRole.ImproverUnit,
                },
            ]),
        };
        var reordered = group with
        {
            Children = Array.AsReadOnly([children[1], children[0]]),
        };
        var third = group with
        {
            Children = Array.AsReadOnly([children[0], children[1], children[1]]),
        };
        var other = Group("other-invocation");
        var mixed = group with
        {
            Children = Array.AsReadOnly([children[0], other.Children[1]]),
        };

        foreach (var (name, invalid) in new[]
                 {
                     ("same", sameTarget),
                     ("reordered", reordered),
                     ("third", third),
                     ("mixed", mixed),
                 })
        {
            var root = Path.Combine(_root, "shape-" + name);
            var result = new FileDpoPairStore(root).AddAtomicGroup(invalid);
            Assert.False(result.IsSuccess);
            Assert.Equal("DPO_PAIR_ATOMIC_GROUP_INVALID", result.ErrorCode);
            Assert.False(Directory.Exists(root));
        }
    }

    [Fact]
    public void TamperedCommittedGroup_FailsClosedWithoutExposingSibling()
    {
        var group = Group("tamper");
        var store = Store("tamper");
        Assert.True(store.AddAtomicGroup(group).IsSuccess);
        var path = Assert.Single(Directory.EnumerateFiles(
            Path.Combine(_root, "tamper"),
            "*.dpo-pair-group.json"));
        var json = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        var substitutedCommitment = new string('f', 64);
        json["groupCommitmentSha256"] = substitutedCommitment;
        var renamed = Path.Combine(
            Path.GetDirectoryName(path)!,
            substitutedCommitment + ".dpo-pair-group.json");
        File.WriteAllText(renamed, json.ToJsonString());
        File.Delete(path);

        var read = Store("tamper").GetAll();

        Assert.False(read.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_CORRUPT", read.ErrorCode);
    }

    [Fact]
    public void DuplicateSameValuedRootProperty_FailsCorruptWithoutSiblingExposure()
    {
        var store = Store("duplicate-root-property");
        Assert.True(store.AddAtomicGroup(Group("duplicate-root-property")).IsSuccess);
        var root = Path.Combine(_root, "duplicate-root-property");
        var path = Assert.Single(Directory.EnumerateFiles(
            root,
            "*.dpo-pair-group.json"));
        var json = File.ReadAllText(path);
        File.WriteAllText(
            path,
            json.Insert(1, "\"domain\":\"xiigen.dpo-pair-atomic-group.v1\","));

        var read = new FileDpoPairStore(root).GetAll();

        Assert.False(read.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_CORRUPT", read.ErrorCode);
    }

    [Fact]
    public void DuplicateSameValuedChildProperty_FailsCorruptWithoutSiblingExposure()
    {
        var store = Store("duplicate-child-property");
        Assert.True(store.AddAtomicGroup(Group("duplicate-child-property")).IsSuccess);
        var root = Path.Combine(_root, "duplicate-child-property");
        var path = Assert.Single(Directory.EnumerateFiles(
            root,
            "*.dpo-pair-group.json"));
        var json = File.ReadAllText(path);
        const string ordinal = "\"ordinal\":0,";
        Assert.Contains(ordinal, json, StringComparison.Ordinal);
        File.WriteAllText(
            path,
            json.Replace(
                ordinal,
                ordinal + ordinal,
                StringComparison.Ordinal));

        var read = new FileDpoPairStore(root).GetAll();

        Assert.False(read.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_CORRUPT", read.ErrorCode);
    }

    [Fact]
    public void InterruptedGroupTemporary_IsIgnoredAndExposesNeitherSibling()
    {
        var root = Path.Combine(_root, "torn");
        CreateOwnerOnlyTestDirectory(root);
        WriteOwnerOnlyText(Path.Combine(root, ".torn.group.tmp"), "{\"children\":[{}]");

        var read = new FileDpoPairStore(root).GetAll();

        Assert.True(read.IsSuccess, read.ErrorMessage);
        Assert.Empty(read.Value!);
    }

    [Fact]
    public void CapacityAndRequestConflicts_RejectWholeGroup()
    {
        var group = Group("capacity-request");
        var capacityRoot = Path.Combine(_root, "capacity");
        var constrained = new FileDpoPairStore(capacityRoot, maximumRecordCount: 1);
        var rejected = constrained.AddAtomicGroup(group);
        Assert.False(rejected.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_CAPACITY", rejected.ErrorCode);
        Assert.Empty(Directory.EnumerateFiles(capacityRoot, "*.dpo-pair-group.json"));

        var store = Store("request-conflict");
        Assert.True(store.AddAtomicGroup(group).IsSuccess);
        var duplicate = BoundPair("different", group.RequestId, "different");
        var conflict = store.Add(duplicate);
        Assert.False(conflict.IsSuccess);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.DuplicateRequestIdErrorCode,
            conflict.ErrorCode);
        Assert.Equal(2, store.GetAll().Value!.Count);
    }

    [Fact]
    public async Task ConcurrentExactGroupRetry_PublishesOneAtomicRecord()
    {
        var group = Group("concurrent");
        var root = Path.Combine(_root, "concurrent");
        var first = new FileDpoPairStore(root);
        var second = new FileDpoPairStore(root);

        var results = await Task.WhenAll(
            Task.Run(() => first.AddAtomicGroup(group)),
            Task.Run(() => second.AddAtomicGroup(group)));

        Assert.All(results, result => Assert.True(result.IsSuccess, result.ErrorMessage));
        Assert.Single(Directory.EnumerateFiles(root, "*.dpo-pair-group.json"));
        Assert.Equal(2, new FileDpoPairStore(root).GetAll().Value!.Count);
    }

    [Fact]
    public void MutableEnvelopeList_CannotChangeStoredGroupAfterBoundedCapture()
    {
        var group = Group("mutable-envelope");
        var mutable = new SelfClearingAtomicGroupChildren(group.Children);
        var envelope = group with { Children = mutable };
        var store = Store("mutable-envelope");

        var added = store.AddAtomicGroup(envelope);

        Assert.True(added.IsSuccess, added.ErrorMessage);
        Assert.Empty(mutable);
        var stored = store.GetAll().Value!;
        Assert.Equal(2, stored.Count);
        Assert.Equal("critic-prompt", stored[0].Prompt);
        Assert.Equal("improver-prompt", stored[1].Prompt);
    }

    [Fact]
    public void TransactionLimitNeverSplitsGroup_AndAbortRestoresBoth()
    {
        var group = Group("selection");
        var root = Path.Combine(_root, "selection");
        var constrained = new FileDpoPairStore(
            root,
            maximumTransactionPairCountForTesting: 1);
        Assert.True(constrained.AddAtomicGroup(group).IsSuccess);
        var limited = constrained.PrepareTransaction(["BUG"]);
        Assert.True(limited.IsSuccess, limited.ErrorMessage);
        Assert.Null(limited.Value);
        Assert.Equal(2, constrained.GetAll().Value!.Count);

        var restarted = new FileDpoPairStore(
            root,
            maximumTransactionPairCountForTesting: 2);
        var transaction = Assert.IsType<FileDpoPairStore.TransactionHandle>(
            restarted.PrepareTransaction(["BUG"]).Value);
        var selected = transaction.Read().Value!.Pairs;
        Assert.Equal(2, selected.Count);
        Assert.Equal("critic-prompt", selected[0].Pair.Prompt);
        Assert.Equal("improver-prompt", selected[1].Pair.Prompt);
        Assert.Empty(restarted.GetAll().Value!);
        Assert.True(transaction.ReleaseIfUnmutated().IsSuccess);
        Assert.Equal(2, new FileDpoPairStore(root).GetAll().Value!.Count);
    }

    [Fact]
    public void PreparedTransaction_RestartRetainsBothSiblingsAndOneRequest()
    {
        var group = Group("transaction-restart");
        var store = Store("transaction-restart");
        Assert.True(store.AddAtomicGroup(group).IsSuccess);
        var first = Assert.IsType<FileDpoPairStore.TransactionHandle>(
            store.PrepareTransaction(["BUG"]).Value);
        Assert.Equal(2, first.Read().Value!.Pairs.Count);

        var resumed = Assert.IsType<FileDpoPairStore.TransactionHandle>(
            Store("transaction-restart").PrepareTransaction(["BUG"]).Value);

        Assert.Equal(first.TransactionId, resumed.TransactionId);
        Assert.Equal(2, resumed.Read().Value!.Pairs.Count);
        Assert.Single(Directory.EnumerateFiles(
            Path.Combine(_root, "transaction-restart"),
            "*.dpo-transaction.json"));
    }

    [Fact]
    public void CommittedTransaction_CompactsGroupAndRestartReplaysOneParentRequest()
    {
        var group = Group("commit-replay");
        var root = Path.Combine(_root, "commit-replay");
        var store = new FileDpoPairStore(root);
        Assert.True(store.AddAtomicGroup(group).IsSuccess);
        var transaction = Assert.IsType<FileDpoPairStore.TransactionHandle>(
            store.PrepareTransaction(["BUG"]).Value);
        var selected = transaction.Read().Value!.Pairs
            .Select(static value => value.Pair)
            .ToArray();
        var content = DpoPairTrainingContentAuthority.Project(selected).Value!;
        Assert.True(transaction.ValidateTeacherBatch(
            "BUG",
            content,
            selected.Select(static pair => pair.TeacherProvenance).ToArray())
            .IsSuccess);

        var seed = Seed();
        using (var reservation = transaction.PrepareCandidate("BUG", seed).Value!)
        {
            var model = new LlmModel(ModelConfiguration());
            _ = LoraApplicator.Apply(model, LoraConfiguration());
            SaveAdapterCheckpointOwnerOnly(model, reservation.StagingPath);
        }
        Assert.True(transaction.BeginMutation("BUG").IsSuccess);
        var completed = transaction.CompleteCandidate("BUG");
        Assert.True(completed.IsSuccess, completed.ErrorMessage);
        var artifact = completed.Value!;
        var domain = Assert.Single(transaction.Read().Value!.Domains);
        var adapter = new ModelAdapter(
            domain.Domain,
            artifact.AdapterPath,
            domain.TrainingExamples,
            domain.TrainedAt,
            artifact.Provenance);
        var registry = new DomainModelRegistry(
            new AtomicGroupNullStorage(),
            Path.Combine(_root, "commit-replay-registry"));
        var published = ((IDpoRegistryPublicationTransaction)registry)
            .PublishBatchAndCommit(
                [adapter],
                durable =>
                {
                    var recorded = transaction.RecordPublished(
                        "BUG",
                        durable.Single().Provenance!);
                    return recorded.IsSuccess
                        ? transaction.CommitDrain()
                        : recorded;
                });

        Assert.True(published.IsSuccess, published.ErrorMessage);
        Assert.Empty(Directory.EnumerateFiles(root, "*.dpo-pair-group.json"));
        Assert.Empty(new FileDpoPairStore(root).GetAll().Value!);
        Assert.True(new FileDpoPairStore(root).AddAtomicGroup(group).IsSuccess);
        var ledger = JsonNode.Parse(File.ReadAllText(Path.Combine(
            root,
            ".dpo-replay-ledger.json")))!.AsObject();
        var entry = Assert.Single(ledger["entries"]!.AsArray()).AsObject();
        Assert.Equal(group.GroupCommitmentSha256, entry["groupId"]!.GetValue<string>());
        Assert.Equal(2, entry["pairIds"]!.AsArray().Count);
        Assert.Equal(2, entry["contentSha256s"]!.AsArray().Count);
    }

    [Fact]
    public void OrdinarySinglePairBehavior_RemainsOnePairPerRequest()
    {
        var store = Store("ordinary");
        var pair = BoundPair("ordinary", "ordinary-request", "ordinary");

        Assert.True(store.Add(pair).IsSuccess);
        var pending = Assert.Single(Store("ordinary").GetAll().Value!);
        Assert.Equal(pair.Prompt, pending.Prompt);
    }

    [Fact]
    public void SchemaV4AtomicGroupTransaction_RemainsAdapterPublication()
    {
        var root = Path.Combine(_root, "schema-v4-adapter");
        var store = new FileDpoPairStore(root);
        var group = Group("schema-v4-request");
        Assert.True(store.AddAtomicGroup(group).IsSuccess);
        var transaction = Assert.IsType<FileDpoPairStore.TransactionHandle>(
            store.PrepareTransaction(["BUG"]).Value);
        var path = Assert.Single(Directory.EnumerateFiles(
            root,
            "*.dpo-transaction.json"));
        var json = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        json["schemaVersion"] = 4;
        json.Remove("kind");
        json.Remove("externalConsumption");
        DpoPairTransactionRegressionTests.RewriteHistoricalPreparedTransaction(
            path,
            json,
            atomicGroup: true);

        var resumed = new FileDpoPairStore(root).PrepareTransaction(["BUG"]);

        Assert.True(resumed.IsSuccess, resumed.ErrorMessage);
        Assert.Equal(transaction.TransactionId, resumed.Value!.TransactionId);
        Assert.Equal(2, resumed.Value.Read().Value!.Pairs.Count);
    }

    [Fact]
    public void ReplaySchemaV2_RemainsReadableAndReservesWholeAtomicGroup()
    {
        var root = Path.Combine(_root, "replay-v2");
        var store = new FileDpoPairStore(root);
        var group = Group("replay-v2-request");
        Assert.True(store.AddAtomicGroup(group).IsSuccess);
        var replay = new JsonObject
        {
            ["schemaVersion"] = 2,
            ["entries"] = new JsonArray
            {
                new JsonObject
                {
                    ["pairId"] = string.Empty,
                    ["requestId"] = group.RequestId,
                    ["contentSha256"] = group.GroupCommitmentSha256,
                    ["groupId"] = group.GroupCommitmentSha256,
                    ["pairIds"] = new JsonArray(group.Children
                        .OrderBy(static child => child.Ordinal)
                        .Select(static child => JsonValue.Create(child.PairId))
                        .ToArray()),
                    ["contentSha256s"] = new JsonArray(group.Children
                        .OrderBy(static child => child.Ordinal)
                        .Select(static child => JsonValue.Create(child.ContentSha256))
                        .ToArray()),
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

    private FileDpoPairStore Store(string name)
        => new(Path.Combine(_root, name));

    private static DpoPairAtomicGroupEnvelope Group(string requestId)
    {
        var critic = BoundPair("critic-prompt", requestId, "critic");
        var improver = BoundPair("improver-prompt", requestId, "improver");
        var group = DpoPairAtomicGroupEnvelope.Create(critic, improver);
        Assert.True(group.IsSuccess, group.ErrorMessage);
        return group.Value!;
    }

    private static DpoPair BoundPair(
        string prompt,
        string requestId,
        string evidence)
    {
        var pair = new DpoPair(
            prompt,
            "chosen",
            "rejected",
            "BUG",
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
                ModelArtifactDigestKind = LegalTrainingTeacherAdmissionAuthority
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

    private static FileDpoPairStore.CandidateProvenanceSeed Seed()
    {
        var manifest = TrainingSourceProvenance.BuildManifest(
            [TrainingSourceProvenance.CreateSourceId(
                "tests/dpo-atomic-group",
                "exact supervised bytes")]).Value!;
        return new FileDpoPairStore.CandidateProvenanceSeed(
            manifest,
            FileDpoPairStore.NoParentAdapterIdentity,
            new string('d', 64),
            new string('e', 64),
            TrainingSourceProvenance.Identify(ModelConfiguration()),
            TrainingSourceProvenance.Identify(LoraConfiguration()),
            2,
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

    private static void RewriteHashedJson(string path, JsonObject json)
    {
        json["recordSha256"] = string.Empty;
        var canonical = json.ToJsonString();
        json["recordSha256"] = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(canonical)))
            .ToLowerInvariant();
        WriteOwnerOnlyText(path, json.ToJsonString());
    }

    private static void CreateOwnerOnlyTestDirectory(string path)
        => SecureLocalFileBoundary.CreateOwnerOnlyDirectory(path);

    private static void WriteOwnerOnlyText(string path, string content)
    {
        // Эти тесты вручную создают файлы, которые в продукте пишет
        // защищённая файловая граница. Поэтому тестовый файл создаётся с
        // доступом только владельцу: иначе проверка ломается на правах, а не
        // на содержании атомарной группы или журнала повтора.
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
        // Файл сохранённых параметров модели затем читает защищённое
        // хранилище. Записываем его в уже открытый защищённый поток, чтобы
        // обычная запись по имени не создала временный файл с неподходящими
        // правами.
        using var stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(
            path,
            FileMode.CreateNew,
            FileAccess.ReadWrite,
            FileShare.None,
            FileOptions.WriteThrough);
        model.SaveAdapterCheckpoint(stream);
        stream.Flush(flushToDisk: true);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }
}

file sealed class AtomicGroupNullStorage : ISessionStorage
{
    public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
        => Task.FromResult(OperationResult<bool>.Ok(true));

    public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
        => Task.FromResult(OperationResult<AgentState>.Fail(
            "Not persisted.",
            "NULL_STORAGE"));

    public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
        => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok([]));
}

file sealed class SelfClearingAtomicGroupChildren(
    IEnumerable<DpoPairAtomicGroupChild> source)
    : IReadOnlyList<DpoPairAtomicGroupChild>
{
    private readonly List<DpoPairAtomicGroupChild> _items = source.ToList();

    public int Count => _items.Count;
    public DpoPairAtomicGroupChild this[int index] => _items[index];

    public IEnumerator<DpoPairAtomicGroupChild> GetEnumerator()
    {
        var snapshot = _items.ToArray();
        foreach (var item in snapshot)
            yield return item;
        _items.Clear();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => GetEnumerator();
}
