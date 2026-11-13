// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using AgentInfrastructure.Logical;
using ContinuousLearning;

namespace LlmFromScratch.Tests;

public sealed class DpoPairExternalConsumptionTransactionTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        $"dpo-external-consumption-{Guid.NewGuid():N}");

    [Fact]
    public void ExternalConsumption_IsAnOptionalTypedStoreCapability()
    {
        IDpoPairStore supported = Store("typed-capability");
        IDpoPairStore unsupported = new UnsupportedDpoPairStore();
        var coordinator = Assert.IsAssignableFrom<
            IDpoPairExternalConsumptionTransactionCoordinator>(supported);
        Assert.False(unsupported is
            IDpoPairExternalConsumptionTransactionCoordinator);

        var group = Group("typed-capability");
        var receipt = AssertSuccess(supported.AddAtomicGroup(group));
        Assert.NotNull(AssertSuccess(
            coordinator.PrepareExternalConsumptionTransaction(receipt, "BUG")));
    }

    [Fact]
    public void ExactGroup_EndToEnd_CommitsTypedReplayAndRestartsCommitted()
    {
        var group = Group("external-e2e");
        var store = Store("external-e2e");
        var receipt = AssertSuccess(store.AddAtomicGroup(group));
        var transaction = Assert.IsType<
            FileDpoPairStore.ExternalConsumptionTransactionHandle>(
            AssertSuccess(store.PrepareExternalConsumptionTransaction(
                receipt,
                "BUG")));
        Assert.Equal(
            FileDpoPairStore.DpoExternalConsumptionState.Reserved,
            AssertSuccess(transaction.Read()).State);

        var (batch, provenance) = Aggregate(group.RequestId);
        Assert.True(transaction.BindAggregate(batch, provenance).IsSuccess);
        Assert.Equal(
            FileDpoPairStore.DpoExternalConsumptionState.AggregateBound,
            AssertSuccess(transaction.Read()).State);

        var authorization = AssertSuccess(transaction.BeginMutation());
        var started = AssertSuccess(transaction.Read());
        Assert.Equal(
            FileDpoPairStore.DpoExternalConsumptionState.MutationStarted,
            started.State);
        var transactionJson = JsonNode.Parse(File.ReadAllText(Assert.Single(
            Directory.EnumerateFiles(
                Path.Combine(_root, "external-e2e"),
                "*.dpo-transaction.json"))))!.AsObject();
        Assert.Equal(1, transactionJson["kind"]!.GetValue<int>());
        Assert.Equal(
            2,
            transactionJson["externalConsumption"]!["state"]!.GetValue<int>());
        Assert.Equal(
            1,
            transactionJson["requests"]![0]!["state"]!.GetValue<int>());

        var result = Result('a', 'b');
        var completed = AssertSuccess(transaction.Complete(authorization, result));
        Assert.Equal(group.GroupCommitmentSha256, completed.GroupCommitmentSha256);
        Assert.Equal(group.RequestId, completed.RequestId);
        Assert.Equal(batch.Rows[0].Sha256, completed.ParentContentSha256);
        Assert.Equal(receipt.OrderedPairIds, completed.OrderedPairIds);
        Assert.Equal(result.ResultSha256, completed.ResultSha256);
        Assert.Equal(result.CheckpointSha256, completed.CheckpointSha256);

        var root = Path.Combine(_root, "external-e2e");
        Assert.Empty(Directory.EnumerateFiles(root, "*.dpo-pair-group.json"));
        Assert.Empty(Directory.EnumerateFiles(root, "*.dpo-transaction.json"));
        Assert.Single(Directory.EnumerateFiles(root, "*.dpo-external-consumption.json"));
        var replay = JsonNode.Parse(File.ReadAllText(
            Path.Combine(root, ".dpo-replay-ledger.json")))!.AsObject();
        var replayEntry = Assert.Single(replay["entries"]!.AsArray()).AsObject();
        Assert.Equal(1, replayEntry["kind"]!.GetValue<int>());
        Assert.NotNull(replayEntry["externalConsumption"]);
        Assert.Equal(2, replayEntry["pairIds"]!.AsArray().Count);
        Assert.Equal(2, replayEntry["contentSha256s"]!.AsArray().Count);

        var restarted = Assert.IsType<
            FileDpoPairStore.ExternalConsumptionTransactionHandle>(
            AssertSuccess(Store("external-e2e")
                .PrepareExternalConsumptionTransaction(receipt, "BUG")));
        var replayed = AssertSuccess(restarted.Read());
        Assert.Equal(
            FileDpoPairStore.DpoExternalConsumptionState.Committed,
            replayed.State);
        Assert.Equal(result.CheckpointSha256, replayed.CheckpointSha256);
    }

    [Fact]
    public void ForgedReceiptAndWrongDomainOrderOrRequest_NeverSelectAGroup()
    {
        var group = Group("forged-receipt");
        var store = Store("forged-receipt");
        var receipt = AssertSuccess(store.AddAtomicGroup(group));
        var invalid = new[]
        {
            (receipt with { GroupCommitmentSha256 = new string('f', 64) }, "BUG"),
            (receipt with { RequestId = "wrong-request" }, "BUG"),
            (receipt with
            {
                OrderedPairIds = receipt.OrderedPairIds.Reverse().ToArray(),
            }, "BUG"),
            (receipt, "OTHER"),
        };

        foreach (var (forged, domain) in invalid)
        {
            var rejected = store.PrepareExternalConsumptionTransaction(forged, domain);
            Assert.False(rejected.IsSuccess);
            Assert.Equal("DPO_EXTERNAL_CONSUMPTION_INVALID", rejected.ErrorCode);
        }
        Assert.Empty(Directory.EnumerateFiles(
            Path.Combine(_root, "forged-receipt"),
            "*.dpo-transaction.json"));
        Assert.NotNull(AssertSuccess(
            store.PrepareExternalConsumptionTransaction(receipt, "BUG")));
    }

    [Fact]
    public void AggregateBinding_RequiresOneExactParentAndUnchangedProvenance()
    {
        var group = Group("aggregate-binding");
        var store = Store("aggregate-binding");
        var receipt = AssertSuccess(store.AddAtomicGroup(group));
        var transaction = Assert.IsType<
            FileDpoPairStore.ExternalConsumptionTransactionHandle>(
            AssertSuccess(store.PrepareExternalConsumptionTransaction(
                receipt,
                "BUG")));
        var (batch, provenance) = Aggregate(group.RequestId);
        var twoRows = AssertSuccess(BoundedDpoTrainingContentProjector.Project([
            Row("parent-one"),
            Row("parent-two"),
        ]));
        var wrongRequest = provenance with { RequestId = "wrong-request" };
        var wrongInvocation = provenance with { InvocationCount = 2 };
        var wrongDigest = provenance with
        {
            TrainingContentSha256 = new string('e', 64),
        };

        Assert.False(transaction.BindAggregate(twoRows, provenance).IsSuccess);
        Assert.False(transaction.BindAggregate(batch, wrongRequest).IsSuccess);
        Assert.False(transaction.BindAggregate(batch, wrongInvocation).IsSuccess);
        Assert.False(transaction.BindAggregate(batch, wrongDigest).IsSuccess);
        Assert.Equal(
            FileDpoPairStore.DpoExternalConsumptionState.Reserved,
            AssertSuccess(transaction.Read()).State);
        Assert.True(transaction.BindAggregate(batch, provenance).IsSuccess);
        Assert.True(transaction.BindAggregate(batch, provenance).IsSuccess);
    }

    [Fact]
    public void DurableBegin_IsSingleUseAndStartedTransactionNeverReleasesOrRetrains()
    {
        var group = Group("durable-begin");
        var store = Store("durable-begin");
        var receipt = AssertSuccess(store.AddAtomicGroup(group));
        var transaction = Assert.IsType<
            FileDpoPairStore.ExternalConsumptionTransactionHandle>(
            AssertSuccess(store.PrepareExternalConsumptionTransaction(
                receipt,
                "BUG")));
        var (batch, provenance) = Aggregate(group.RequestId);
        Assert.True(transaction.BindAggregate(batch, provenance).IsSuccess);
        var authorization = AssertSuccess(transaction.BeginMutation());

        Assert.False(transaction.ReleaseIfUnmutated().Value);
        var secondBegin = transaction.BeginMutation();
        Assert.False(secondBegin.IsSuccess);
        Assert.Equal("DPO_TRANSACTION_RECOVERY_REQUIRED", secondBegin.ErrorCode);
        var adapterAttempt = store.PrepareTransaction(["BUG"]);
        Assert.False(adapterAttempt.IsSuccess);
        Assert.Equal("DPO_TRANSACTION_RECOVERY_REQUIRED", adapterAttempt.ErrorCode);

        var first = transaction.Complete(authorization, Result('1', '2'));
        Assert.True(first.IsSuccess, first.ErrorMessage);
        var second = transaction.Complete(authorization, Result('1', '2'));
        Assert.False(second.IsSuccess);
    }

    [Fact]
    public void StartedWithoutManifest_RestartIsStableRecoveryRequired()
    {
        var group = Group("started-no-artifact");
        var store = Store("started-no-artifact");
        var receipt = AssertSuccess(store.AddAtomicGroup(group));
        var transaction = Assert.IsType<
            FileDpoPairStore.ExternalConsumptionTransactionHandle>(
            AssertSuccess(store.PrepareExternalConsumptionTransaction(
                receipt,
                "BUG")));
        var (batch, provenance) = Aggregate(group.RequestId);
        Assert.True(transaction.BindAggregate(batch, provenance).IsSuccess);
        _ = AssertSuccess(transaction.BeginMutation());

        var first = Store("started-no-artifact")
            .PrepareExternalConsumptionTransaction(receipt, "BUG");
        var second = Store("started-no-artifact")
            .PrepareExternalConsumptionTransaction(receipt, "BUG");

        Assert.False(first.IsSuccess);
        Assert.False(second.IsSuccess);
        Assert.Equal("DPO_TRANSACTION_RECOVERY_REQUIRED", first.ErrorCode);
        Assert.Equal(first.ErrorCode, second.ErrorCode);
        Assert.False(transaction.ReleaseIfUnmutated().Value);
    }

    [Fact]
    public void ManifestTamper_FailsRestartReadback()
    {
        var group = Group("manifest-tamper");
        var store = Store("manifest-tamper");
        var receipt = AssertSuccess(store.AddAtomicGroup(group));
        var transaction = Assert.IsType<
            FileDpoPairStore.ExternalConsumptionTransactionHandle>(
            AssertSuccess(store.PrepareExternalConsumptionTransaction(
                receipt,
                "BUG")));
        var (batch, provenance) = Aggregate(group.RequestId);
        Assert.True(transaction.BindAggregate(batch, provenance).IsSuccess);
        var authorization = AssertSuccess(transaction.BeginMutation());
        Assert.True(transaction.Complete(authorization, Result('3', '4')).IsSuccess);
        var root = Path.Combine(_root, "manifest-tamper");
        var path = Assert.Single(Directory.EnumerateFiles(
            root,
            "*.dpo-external-consumption.json"));
        var json = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        json["checkpointSha256"] = new string('f', 64);
        File.WriteAllText(path, json.ToJsonString());

        var restarted = Store("manifest-tamper")
            .PrepareExternalConsumptionTransaction(receipt, "BUG");

        Assert.False(restarted.IsSuccess);
        Assert.Equal("DPO_EXTERNAL_CONSUMPTION_INVALID", restarted.ErrorCode);
    }

    [Theory]
    [InlineData("parentBatchSha256")]
    [InlineData("parentProvenanceSha256")]
    public void ParentAuthorityTamper_WithRehashedManifest_FailsRestart(
        string propertyName)
    {
        var name = $"parent-authority-{propertyName}";
        var group = Group(name);
        var store = Store(name);
        var receipt = AssertSuccess(store.AddAtomicGroup(group));
        var transaction = Assert.IsType<
            FileDpoPairStore.ExternalConsumptionTransactionHandle>(
            AssertSuccess(store.PrepareExternalConsumptionTransaction(
                receipt,
                "BUG")));
        var (batch, provenance) = Aggregate(group.RequestId);
        Assert.True(transaction.BindAggregate(batch, provenance).IsSuccess);
        var authorization = AssertSuccess(transaction.BeginMutation());
        Assert.True(transaction.Complete(authorization, Result('7', '8')).IsSuccess);
        var path = Assert.Single(Directory.EnumerateFiles(
            Path.Combine(_root, name),
            "*.dpo-external-consumption.json"));
        var manifest = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        Assert.Equal(
            manifest["manifestSha256"]!.GetValue<string>(),
            ComputeManifestHash(manifest));
        manifest[propertyName] = new string('e', 64);
        RewriteManifestHash(path, manifest);

        var restarted = Store(name)
            .PrepareExternalConsumptionTransaction(receipt, "BUG");

        Assert.False(restarted.IsSuccess);
        Assert.Equal("DPO_EXTERNAL_CONSUMPTION_INVALID", restarted.ErrorCode);
    }

    [Theory]
    [InlineData("_afterExternalManifestWrittenBeforeReadyForTesting", false)]
    [InlineData("_afterExternalReadyWrittenBeforeCommitForTesting", true)]
    public void ExactExternalWriteBoundary_RestartConvergesWithoutRemutation(
        string hookName,
        bool readyWasPersisted)
    {
        var name = $"boundary-{readyWasPersisted}";
        var root = Path.Combine(_root, name);
        var store = new FileDpoPairStore(root);
        SetPrivateBoundaryHook(store, hookName);
        var group = Group(name);
        var receipt = AssertSuccess(store.AddAtomicGroup(group));
        var transaction = Assert.IsType<
            FileDpoPairStore.ExternalConsumptionTransactionHandle>(
            AssertSuccess(store.PrepareExternalConsumptionTransaction(
                receipt,
                "BUG")));
        var (batch, provenance) = Aggregate(group.RequestId);
        Assert.True(transaction.BindAggregate(batch, provenance).IsSuccess);
        var authorization = AssertSuccess(transaction.BeginMutation());

        var interrupted = transaction.Complete(
            authorization,
            Result('9', 'a'));

        Assert.False(interrupted.IsSuccess);
        var persisted = JsonNode.Parse(File.ReadAllText(Assert.Single(
            Directory.EnumerateFiles(root, "*.dpo-transaction.json"))))!
            .AsObject();
        Assert.Equal(readyWasPersisted ? 4 : 1, persisted["state"]!.GetValue<int>());
        Assert.Equal(
            readyWasPersisted ? 3 : 2,
            persisted["externalConsumption"]!["state"]!.GetValue<int>());
        Assert.Equal(
            0,
            persisted["domains"]![0]!["state"]!.GetValue<int>());
        Assert.DoesNotContain(
            persisted["domains"]!.AsArray(),
            value => value!["state"]!.GetValue<int>() == 4);
        var manifestPath = Assert.Single(Directory.EnumerateFiles(
            root,
            "*.dpo-external-consumption.json"));
        var manifestBytes = File.ReadAllBytes(manifestPath);

        var resumed = Assert.IsType<
            FileDpoPairStore.ExternalConsumptionTransactionHandle>(
            AssertSuccess(new FileDpoPairStore(root)
                .PrepareExternalConsumptionTransaction(receipt, "BUG")));

        Assert.Equal(
            FileDpoPairStore.DpoExternalConsumptionState.Committed,
            AssertSuccess(resumed.Read()).State);
        Assert.Equal(manifestBytes, File.ReadAllBytes(manifestPath));
        Assert.Empty(Directory.EnumerateFiles(root, "*.dpo-pair-group.json"));
        Assert.Empty(Directory.EnumerateFiles(root, "*.dpo-transaction.json"));
        var retrain = resumed.BeginMutation();
        Assert.False(retrain.IsSuccess);
    }

    [Fact]
    public async Task ConcurrentPrepare_MintsOneExactTransaction()
    {
        var group = Group("concurrent-prepare");
        var root = Path.Combine(_root, "concurrent-prepare");
        var first = new FileDpoPairStore(root);
        var receipt = AssertSuccess(first.AddAtomicGroup(group));
        var second = new FileDpoPairStore(root);

        var prepared = await Task.WhenAll(
            Task.Run(() => first.PrepareExternalConsumptionTransaction(
                receipt,
                "BUG")),
            Task.Run(() => second.PrepareExternalConsumptionTransaction(
                receipt,
                "BUG")));

        Assert.All(prepared, result => Assert.True(result.IsSuccess, result.ErrorMessage));
        Assert.Equal(prepared[0].Value!.TransactionId, prepared[1].Value!.TransactionId);
        Assert.Single(Directory.EnumerateFiles(root, "*.dpo-transaction.json"));
    }

    [Fact]
    public void ReplayWrittenBeforeGroupDeletion_RestartFinishesTwoOrNeitherCompaction()
    {
        var group = Group("replay-boundary");
        var root = Path.Combine(_root, "replay-boundary");
        var store = new FileDpoPairStore(
            root,
            afterReplayLedgerWrittenBeforePairCompactionForTesting: () =>
                throw new IOException("simulated crash boundary"));
        var receipt = AssertSuccess(store.AddAtomicGroup(group));
        var transaction = Assert.IsType<
            FileDpoPairStore.ExternalConsumptionTransactionHandle>(
            AssertSuccess(store.PrepareExternalConsumptionTransaction(
                receipt,
                "BUG")));
        var (batch, provenance) = Aggregate(group.RequestId);
        Assert.True(transaction.BindAggregate(batch, provenance).IsSuccess);
        var authorization = AssertSuccess(transaction.BeginMutation());

        var interrupted = transaction.Complete(authorization, Result('5', '6'));

        Assert.False(interrupted.IsSuccess);
        Assert.Single(Directory.EnumerateFiles(root, "*.dpo-pair-group.json"));
        var committedPath = Assert.Single(Directory.EnumerateFiles(
            root,
            "*.dpo-transaction.json"));
        var committedJson = JsonNode.Parse(File.ReadAllText(committedPath))!
            .AsObject();
        Assert.Equal(3, committedJson["state"]!.GetValue<int>());
        Assert.NotEqual(2, committedJson["state"]!.GetValue<int>());
        Assert.Equal(
            4,
            committedJson["externalConsumption"]!["state"]!.GetValue<int>());
        Assert.Equal(
            0,
            committedJson["domains"]![0]!["state"]!.GetValue<int>());
        Assert.True(File.Exists(Path.Combine(root, ".dpo-replay-ledger.json")));
        var resumed = new FileDpoPairStore(root)
            .PrepareExternalConsumptionTransaction(receipt, "BUG");
        Assert.True(resumed.IsSuccess, resumed.ErrorMessage);
        Assert.Empty(Directory.EnumerateFiles(root, "*.dpo-pair-group.json"));
        Assert.Empty(Directory.EnumerateFiles(root, "*.dpo-transaction.json"));
        Assert.Equal(
            FileDpoPairStore.DpoExternalConsumptionState.Committed,
            AssertSuccess(resumed.Value!.Read()).State);
    }

    private FileDpoPairStore Store(string name)
        => new(Path.Combine(_root, name));

    private static DpoPairAtomicGroupEnvelope Group(string requestId)
    {
        var critic = BoundPair("critic-prompt", requestId);
        var improver = BoundPair("improver-prompt", requestId);
        return AssertSuccess(DpoPairAtomicGroupEnvelope.Create(critic, improver));
    }

    private static DpoPair BoundPair(string prompt, string requestId)
    {
        var pair = new DpoPair(
            prompt,
            "chosen",
            "rejected",
            "BUG",
            DateTimeOffset.UnixEpoch)
        {
            TeacherProvenance = Provenance(requestId, string.Empty),
            GovernanceDecision = new TrainingDataGovernanceDecision(
                true,
                false,
                [$"sha256:{new string('2', 64)}"]),
        };
        var bound = AssertSuccess(DpoPairTrainingContentAuthority.Bind(pair));
        return bound with
        {
            ChosenAuthorship = new DpoPairSideAuthorship(
                DPO.Models.DpoPairSideAuthorKind.OpenTeacher,
                bound.TeacherProvenance),
            RejectedAuthorship = new DpoPairSideAuthorship(
                DPO.Models.DpoPairSideAuthorKind.LocalModel),
        };
    }

    private static (DpoTrainingContentBatch Batch, TrainingTeacherProvenance Provenance)
        Aggregate(string requestId)
    {
        var batch = AssertSuccess(BoundedDpoTrainingContentProjector.Project([
            Row("aggregate-parent"),
        ]));
        return (batch, Provenance(requestId, batch.Rows[0].Sha256));
    }

    private static DpoTrainingContentRowInput Row(string prompt)
        => new(prompt, "aggregate-chosen", "aggregate-rejected", [], "external-parent");

    private static TrainingTeacherProvenance Provenance(
        string requestId,
        string trainingContentSha256)
        => new(
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
            TrainingContentSha256 = trainingContentSha256,
        };

    private static DpoExternalConsumptionResult Result(char result, char checkpoint)
        => new(
            DpoExternalConsumerKind.DesignConversionDualTargetTeacherAggregate,
            new string(result, 64),
            new string(checkpoint, 64));

    private static void SetPrivateBoundaryHook(
        FileDpoPairStore store,
        string fieldName)
    {
        var field = typeof(FileDpoPairStore).GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        field.SetValue(store, (Action)(() =>
            throw new IOException($"simulated {fieldName}")));
    }

    private static void RewriteManifestHash(string path, JsonObject manifest)
    {
        manifest["manifestSha256"] = ComputeManifestHash(manifest);
        File.WriteAllBytes(path, ExternalManifestBytes(manifest));
    }

    private static string ComputeManifestHash(JsonObject manifest)
    {
        var previous = manifest["manifestSha256"]!.GetValue<string>();
        manifest["manifestSha256"] = string.Empty;
        var digest = Convert.ToHexString(
                SHA256.HashData(ExternalManifestBytes(manifest)))
            .ToLowerInvariant();
        manifest["manifestSha256"] = previous;
        return digest;
    }

    private static byte[] ExternalManifestBytes(JsonObject manifest)
    {
        using var stream = new MemoryStream();
        using var writer = new System.Text.Json.Utf8JsonWriter(stream);
        writer.WriteStartObject();
        writer.WriteNumber("schemaVersion", manifest["schemaVersion"]!.GetValue<int>());
        writer.WriteString("context", manifest["context"]!.GetValue<string>());
        writer.WriteString("transactionId", manifest["transactionId"]!.GetValue<string>());
        writer.WriteString("groupCommitmentSha256", manifest["groupCommitmentSha256"]!.GetValue<string>());
        writer.WriteString("requestId", manifest["requestId"]!.GetValue<string>());
        writer.WriteString("domain", manifest["domain"]!.GetValue<string>());
        writer.WriteString("parentBatchSha256", manifest["parentBatchSha256"]!.GetValue<string>());
        writer.WriteString("parentContentSha256", manifest["parentContentSha256"]!.GetValue<string>());
        writer.WriteString("parentProvenanceSha256", manifest["parentProvenanceSha256"]!.GetValue<string>());
        writer.WriteStartArray("orderedPairIds");
        foreach (var value in manifest["orderedPairIds"]!.AsArray())
            writer.WriteStringValue(value!.GetValue<string>());
        writer.WriteEndArray();
        writer.WriteStartArray("orderedContentSha256s");
        foreach (var value in manifest["orderedContentSha256s"]!.AsArray())
            writer.WriteStringValue(value!.GetValue<string>());
        writer.WriteEndArray();
        writer.WriteNumber("consumerKind", manifest["consumerKind"]!.GetValue<int>());
        writer.WriteString("resultSha256", manifest["resultSha256"]!.GetValue<string>());
        writer.WriteString("checkpointSha256", manifest["checkpointSha256"]!.GetValue<string>());
        writer.WriteString("manifestSha256", manifest["manifestSha256"]!.GetValue<string>());
        writer.WriteEndObject();
        writer.Flush();
        return stream.ToArray();
    }

    private static T AssertSuccess<T>(AgentInfrastructure.OperationResult<T> result)
    {
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return Assert.IsType<T>(result.Value);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }

    private sealed class UnsupportedDpoPairStore : IDpoPairStore
    {
        public AgentInfrastructure.OperationResult<string> Add(DpoPair pair)
            => AgentInfrastructure.OperationResult<string>.Fail(
                "Unsupported.",
                "UNSUPPORTED");

        public AgentInfrastructure.OperationResult<IReadOnlyList<DpoPair>> GetAll()
            => AgentInfrastructure.OperationResult<IReadOnlyList<DpoPair>>.Ok([]);
    }
}
