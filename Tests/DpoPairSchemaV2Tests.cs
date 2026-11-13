// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using System.Text.Json.Nodes;
using ContinuousLearning;

namespace LlmFromScratch.Tests;

public sealed class DpoPairSchemaV2Tests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        $"dpo-schema-v2-{Guid.NewGuid():N}");

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void UndefinedProvenanceEnum_IsRejectedBeforeMutation_ThenValidAddSucceeds(
        int field)
    {
        var provenance = Provenance("undefined");
        provenance = field switch
        {
            0 => provenance with { SourceKind = (TrainingTeacherSourceKind)int.MaxValue },
            1 => provenance with { Family = (OpenWeightTeacherFamily)int.MaxValue },
            2 => provenance with { AccessMode = (TrainingTeacherAccessMode)int.MaxValue },
            3 => provenance with { HostingProvider = (TrainingTeacherHostingProvider)int.MaxValue },
            4 => provenance with { ModelLicense = (TrainingTeacherModelLicense)int.MaxValue },
            _ => throw new ArgumentOutOfRangeException(nameof(field)),
        };
        var store = new FileDpoPairStore(_root);

        var invalid = store.Add(Pair(provenance));

        Assert.Equal("INVALID_DPO_PAIR", invalid.ErrorCode);
        Assert.False(Directory.Exists(_root));

        var valid = store.Add(Pair(Provenance("valid")));
        Assert.True(valid.IsSuccess, valid.ErrorMessage);
        Assert.Single(Directory.GetFiles(_root, "*.dpo-pair.json"));
        Assert.Empty(Directory.GetFiles(_root, "*.tmp"));
    }

    [Fact]
    public void LiteralSchemaV1ExactBytesAndFixedIdentity_RemainReadable()
    {
        const string pairId =
            "b7f2ff8b66473b5aa0280f7b728f193a11a962583c6246450ab94fc0fc94cc5b";
        const string literal =
            "{\"schemaVersion\":1,\"pairId\":\"b7f2ff8b66473b5aa0280f7b728f193a11a962583c6246450ab94fc0fc94cc5b\",\"prompt\":\"legacy prompt\",\"chosen\":\"legacy chosen\",\"rejected\":\"legacy rejected\",\"domain\":\"BUG\",\"capturedAt\":\"1970-01-01T00:00:00.0000000+00:00\"}";
        WriteLiteral(pairId, literal);

        var restarted = new FileDpoPairStore(_root).GetAll();

        Assert.True(restarted.IsSuccess, restarted.ErrorMessage);
        var loaded = Assert.Single(restarted.Value!);
        Assert.Equal("legacy prompt", loaded.Prompt);
        Assert.Equal("legacy chosen", loaded.Chosen);
        Assert.Equal("legacy rejected", loaded.Rejected);
        Assert.Equal("BUG", loaded.Domain);
        Assert.Equal(DateTimeOffset.UnixEpoch, loaded.CapturedAt);
        Assert.Null(loaded.TeacherProvenance);
        Assert.Null(loaded.GovernanceDecision);
        Assert.Equal(literal, File.ReadAllText(Path.Combine(
            _root,
            pairId + ".dpo-pair.json")));
    }

    [Fact]
    public void NewMetadataFreeWrite_UsesHistoricalV2NullBytesAndIdentity_Idempotently()
    {
        const string pairId =
            "e1f64e764f0a483870cd16d231194ac38d17d0dd2f1a5630c019a859f255de3d";
        const string expected =
            "{\"schemaVersion\":2,\"pairId\":\"e1f64e764f0a483870cd16d231194ac38d17d0dd2f1a5630c019a859f255de3d\",\"prompt\":\"v2 prompt\",\"chosen\":\"v2 chosen\",\"rejected\":\"v2 rejected\",\"domain\":\"BUG\",\"capturedAt\":\"1970-01-01T00:00:00.0000000+00:00\",\"teacherProvenance\":null}";
        var pair = new DpoPair(
            "v2 prompt",
            "v2 chosen",
            "v2 rejected",
            "BUG",
            DateTimeOffset.UnixEpoch);
        WriteLiteral(pairId, expected);
        var literalRead = new FileDpoPairStore(_root).GetAll();
        var second = new FileDpoPairStore(_root).Add(pair);
        var loaded = new FileDpoPairStore(_root).GetAll();

        Assert.True(literalRead.IsSuccess, literalRead.ErrorMessage);
        Assert.Equal(pair, Assert.Single(literalRead.Value!));
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(pairId, second.Value);
        Assert.Equal(expected, File.ReadAllText(Assert.Single(
            Directory.GetFiles(_root, "*.dpo-pair.json"))));
        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        Assert.Equal(pair, Assert.Single(loaded.Value!));

        Directory.Delete(_root, recursive: true);
        var first = new FileDpoPairStore(_root).Add(pair);
        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.Equal(pairId, first.Value);
        Assert.Equal(expected, File.ReadAllText(Assert.Single(
            Directory.GetFiles(_root, "*.dpo-pair.json"))));
    }

    [Fact]
    public void SchemaV2MissingExplicitTeacherProvenanceProperty_IsCorrupt()
    {
        var store = new FileDpoPairStore(_root);
        Assert.True(store.Add(Pair(Provenance("missing-provenance-property"))).IsSuccess);
        var path = Assert.Single(Directory.GetFiles(_root, "*.dpo-pair.json"));
        var root = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        Assert.Equal(2, root["schemaVersion"]!.GetValue<int>());
        Assert.True(root.Remove("teacherProvenance"));
        File.WriteAllText(path, root.ToJsonString());

        var result = new FileDpoPairStore(_root).GetAll();

        Assert.Equal("DPO_PAIR_STORE_CORRUPT", result.ErrorCode);
    }

    [Fact]
    public void DifferentTeacherProvenance_ProducesDifferentDurableIdentity()
    {
        var store = new FileDpoPairStore(_root);
        var first = store.Add(Pair(Provenance("request-one")));
        var second = store.Add(Pair(Provenance("request-two")));

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.NotEqual(first.Value, second.Value);
        Assert.Equal(2, Directory.GetFiles(_root, "*.dpo-pair.json").Length);
    }

    [Fact]
    public void TeacherProvenanceTamper_MakesCanonicalRecordCorrupt()
    {
        var store = new FileDpoPairStore(_root);
        Assert.True(store.Add(Pair(Provenance("original-request"))).IsSuccess);
        var path = Assert.Single(Directory.GetFiles(_root, "*.dpo-pair.json"));
        var root = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        Assert.Equal(8, root.Count);
        root["teacherProvenance"]!["provenance"]!["requestId"] = "tampered-request";
        File.WriteAllText(path, root.ToJsonString());

        var result = new FileDpoPairStore(_root).GetAll();

        Assert.Equal("DPO_PAIR_STORE_CORRUPT", result.ErrorCode);
    }

    [Fact]
    public void GovernanceTamper_MakesCanonicalRecordCorrupt()
    {
        var store = new FileDpoPairStore(_root);
        Assert.True(store.Add(Pair(Provenance("governance-tamper"))).IsSuccess);
        var path = Assert.Single(Directory.GetFiles(_root, "*.dpo-pair.json"));
        var root = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        root["teacherProvenance"]!["governanceDecision"]!["isQuarantined"] = true;
        File.WriteAllText(path, root.ToJsonString());

        var result = new FileDpoPairStore(_root).GetAll();

        Assert.Equal("DPO_PAIR_STORE_CORRUPT", result.ErrorCode);
    }

    [Fact]
    public void GovernanceEvidence_IsSnapshottedAndRoundTripsInSchemaV2()
    {
        var evidence = new List<string> { Evidence('a') };
        var governance = new TrainingDataGovernanceDecision(true, false, evidence);
        evidence[0] = Evidence('b');
        evidence.Add(Evidence('c'));
        var pair = Pair(Provenance("governance")) with
        {
            GovernanceDecision = governance,
        };

        Assert.True(new FileDpoPairStore(_root).Add(pair).IsSuccess);
        var loaded = new FileDpoPairStore(_root).GetAll();

        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        var decision = Assert.Single(loaded.Value!).GovernanceDecision!;
        Assert.Equal([Evidence('a')], decision.EvidenceRefs);
    }

    private static DpoPair Pair(TrainingTeacherProvenance provenance)
        => new(
            "same prompt",
            "same chosen",
            "same rejected",
            "BUG",
            DateTimeOffset.UtcNow)
        {
            TeacherProvenance = provenance,
            GovernanceDecision = new TrainingDataGovernanceDecision(
                true,
                false,
                [Evidence('a')]),
        };

    // Deliberately historical schema-v2 provenance. Current legal admission
    // rejects this pre-manifest-digest identity, but durable v2 reads remain
    // permissive for already persisted records.
    private static TrainingTeacherProvenance Provenance(string requestId)
        => new(
            TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            "qwen3.5-9b",
            "2026-07-31",
            TrainingTeacherModelLicense.Apache20,
            ProviderTermsChecked: true,
            ProviderTermsAllowTraining: true,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: true,
            requestId,
            InvocationCount: 1);

    private void WriteLiteral(string pairId, string literal)
    {
        if (OperatingSystem.IsWindows())
            Directory.CreateDirectory(_root);
        else
        {
            // Литеральные исторические записи готовятся вне обычного писателя,
            // поэтому временный каталог теста в Linux сразу создаём приватным. Само
            // хранилище обязано отказать небезопасному уже существующему
            // каталогу, а не менять права вместо вызывающего теста.
            Directory.CreateDirectory(
                _root,
                UnixFileMode.UserRead
                    | UnixFileMode.UserWrite
                    | UnixFileMode.UserExecute);
        }
        var path = Path.Combine(_root, pairId + ".dpo-pair.json");
        File.WriteAllText(path, literal);
        if (!OperatingSystem.IsWindows())
        {
            File.SetUnixFileMode(
                path,
                UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }

    private static string Evidence(char digit)
        => $"sha256:{new string(digit, 64)}";

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }
}
