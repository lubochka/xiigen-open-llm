// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using ContinuousLearning;

namespace LlmFromScratch.Tests;

public sealed class DpoPairSchemaV3Tests : IDisposable
{
    private const string GoldenPairId =
        "b9f73a016a48e04301d4b1677abb8623cbb21f51e3e7f7897c7d04c6eadb4db0";
    private const string GoldenRecordSha256 =
        "1783532ad3aa91cf880f37103cd9f5f00e835223aff0f09b20a3b6c308085904";
    private const string LegacyContentBindingRejectedErrorCode =
        "DPO_TRANSACTION_LEGACY_CONTENT_BINDING_REJECTED";
    private const string LegacyContentBindingRejectedMessage =
        "Legacy DPO pair content binding cannot enter a current durable DPO training transaction.";
    private static readonly string ManifestDigest = new('c', 64);
    private static readonly string LayerDigest = new('d', 64);
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        $"dpo-schema-v3-{Guid.NewGuid():N}");

    [Fact]
    public void CanonicalLocalArtifact_WritesFixedV3BytesHashAndIdentity_AndRoundTrips()
    {
        var pair = Pair(Provenance("v3-golden", includeArtifact: true));
        var added = new FileDpoPairStore(_root).Add(pair);

        Assert.True(added.IsSuccess, added.ErrorMessage);
        Assert.Equal(GoldenPairId, added.Value);
        var path = Assert.Single(Directory.GetFiles(_root, "*.dpo-pair.json"));
        var actual = File.ReadAllText(path);
        const string expected =
            "{\"schemaVersion\":3,\"pairId\":\"b9f73a016a48e04301d4b1677abb8623cbb21f51e3e7f7897c7d04c6eadb4db0\",\"prompt\":\"same prompt\",\"chosen\":\"same chosen\",\"rejected\":\"same rejected\",\"domain\":\"BUG\",\"capturedAt\":\"1970-01-01T00:00:00.0000000+00:00\",\"teacherProvenance\":{\"provenance\":{\"sourceKind\":\"ExternalTeacherOutput\",\"family\":\"Qwen\",\"accessMode\":\"LocalHostedOpenWeight\",\"hostingProvider\":\"Ollama\",\"modelId\":\"qwen3.5:9b\",\"modelVersion\":\"cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc\",\"modelLicense\":\"Apache20\",\"providerTermsChecked\":false,\"providerTermsAllowTraining\":false,\"closedProprietaryModel\":false,\"teacherOutputUsedForTraining\":true,\"requestId\":\"v3-golden\",\"invocationCount\":1,\"modelArtifactDigestKind\":\"ollama_model_layer_sha256\",\"modelArtifactDigest\":\"dddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd\"},\"governanceDecision\":{\"licenseAccepted\":true,\"isQuarantined\":false,\"evidenceRefs\":[\"sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\"]}}}";
        Assert.Equal(expected, actual);
        Assert.Equal(
            GoldenRecordSha256,
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(actual)))
                .ToLowerInvariant());

        var root = JsonNode.Parse(actual)!.AsObject();
        Assert.Equal(3, root["schemaVersion"]!.GetValue<int>());
        Assert.Equal(8, root.Count);
        Assert.Equal(
            15,
            root["teacherProvenance"]!["provenance"]!.AsObject().Count);

        var loaded = new FileDpoPairStore(_root).GetAll();
        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        Assert.Equal(pair, Assert.Single(loaded.Value!));
    }

    [Fact]
    public void SameManifestAndLayer_ReAddIsIdempotent()
    {
        var pair = Pair(Provenance("same-artifact", includeArtifact: true));
        var store = new FileDpoPairStore(_root);

        var first = store.Add(pair);
        var second = new FileDpoPairStore(_root).Add(pair);

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(first.Value, second.Value);
        Assert.Single(Directory.GetFiles(_root, "*.dpo-pair.json"));
    }

    [Fact]
    public void ArtifactIdentity_ParticipatesInV3PairIdentity()
    {
        var firstRoot = Path.Combine(_root, "artifact-identity-first");
        var secondRoot = Path.Combine(_root, "artifact-identity-second");
        var first = new FileDpoPairStore(firstRoot).Add(
            Pair(Provenance("same-request", includeArtifact: true)));
        var second = new FileDpoPairStore(secondRoot).Add(
            Pair(Provenance("same-request", includeArtifact: true) with
            {
                ModelArtifactDigest = new string('e', 64)
            }));

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.NotEqual(first.Value, second.Value);
        Assert.Single(Directory.GetFiles(firstRoot, "*.dpo-pair.json"));
        Assert.Single(Directory.GetFiles(secondRoot, "*.dpo-pair.json"));
    }

    [Fact]
    public void SameRequestWithChangedArtifact_IsRejectedWithoutReplacingOriginal()
    {
        var firstPair = Pair(Provenance(
            "same-request-artifact-replay",
            includeArtifact: true));
        var changedPair = Pair(Provenance(
            "same-request-artifact-replay",
            includeArtifact: true) with
        {
            ModelArtifactDigest = new string('e', 64)
        });
        var first = new FileDpoPairStore(_root).Add(firstPair);
        var changed = new FileDpoPairStore(_root).Add(changedPair);

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.False(changed.IsSuccess);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.DuplicateRequestIdErrorCode,
            changed.ErrorCode);
        Assert.Null(changed.Value);
        Assert.Single(Directory.GetFiles(_root, "*.dpo-pair.json"));
        var readback = new FileDpoPairStore(_root).GetAll();
        Assert.True(readback.IsSuccess, readback.ErrorMessage);
        Assert.Equal(
            LayerDigest,
            Assert.Single(readback.Value!).TeacherProvenance!.ModelArtifactDigest);
    }

    [Theory]
    [InlineData(63)]
    [InlineData(65)]
    public void ArtifactDigestSizeBoundaries_AreRejectedBeforeMutation(int length)
    {
        var provenance = Provenance("digest-boundary", includeArtifact: true) with
        {
            ModelArtifactDigest = new string('d', length)
        };

        var result = new FileDpoPairStore(_root).Add(Pair(provenance));

        Assert.Equal("INVALID_DPO_PAIR", result.ErrorCode);
        Assert.False(Directory.Exists(_root));
    }

    [Fact]
    public void Exact64ByteArtifactDigestBoundary_IsAccepted()
    {
        var result = new FileDpoPairStore(_root).Add(
            Pair(Provenance("digest-exact", includeArtifact: true)));

        Assert.True(result.IsSuccess, result.ErrorMessage);
    }

    [Fact]
    public void PartialMalformedOrNonlocalArtifact_IsRejectedBeforeMutation()
    {
        var valid = Provenance("invalid-artifact", includeArtifact: true);
        var invalid = new[]
        {
            valid with { ModelArtifactDigestKind = string.Empty },
            valid with { ModelArtifactDigest = string.Empty },
            valid with { ModelArtifactDigestKind = "ollama_manifest_sha256" },
            valid with { ModelVersion = new string('C', 64) },
            valid with { ModelVersion = new string('c', 63) },
            valid with { ModelVersion = new string('c', 65) },
            valid with { ModelArtifactDigest = new string('D', 64) },
            valid with
            {
                AccessMode = TrainingTeacherAccessMode.OnlineHostedOpenWeight,
                HostingProvider = TrainingTeacherHostingProvider.DashScope
            },
            valid with { Family = OpenWeightTeacherFamily.Mistral },
            valid with { ModelId = "qwen3.5-9b" },
            valid with { ModelLicense = TrainingTeacherModelLicense.MIT },
            valid with { ProviderTermsChecked = true },
            valid with { ProviderTermsAllowTraining = true },
            valid with { TeacherOutputUsedForTraining = false },
            valid with { InvocationCount = 0 },
        };
        var store = new FileDpoPairStore(_root);

        Assert.All(invalid, provenance => Assert.Equal(
            "INVALID_DPO_PAIR",
            store.Add(Pair(provenance)).ErrorCode));
        Assert.False(Directory.Exists(_root));
    }

    [Theory]
    [InlineData("domain")]
    [InlineData("digest-value")]
    [InlineData("digest-null")]
    [InlineData("digest-missing")]
    [InlineData("digest-duplicate")]
    [InlineData("digest-empty")]
    [InlineData("digest-wrong-json-kind")]
    [InlineData("kind-null")]
    [InlineData("kind-missing")]
    [InlineData("kind-duplicate")]
    [InlineData("kind-empty")]
    [InlineData("kind-wrong-value")]
    [InlineData("kind-wrong-json-kind")]
    [InlineData("metadata-null")]
    [InlineData("metadata-wrong-json-kind")]
    [InlineData("provenance-null")]
    [InlineData("provenance-missing")]
    [InlineData("provenance-duplicate")]
    [InlineData("provenance-wrong-json-kind")]
    [InlineData("extra-property")]
    public void V3DomainArtifactOrExactShapeTamper_IsCorrupt(string scenario)
    {
        Assert.True(new FileDpoPairStore(_root).Add(
            Pair(Provenance("tamper", includeArtifact: true))).IsSuccess);
        var path = Assert.Single(Directory.GetFiles(_root, "*.dpo-pair.json"));
        var json = File.ReadAllText(path);
        var root = JsonNode.Parse(json)!.AsObject();
        var metadata = root["teacherProvenance"]!.AsObject();
        var provenance = metadata["provenance"]!.AsObject();
        switch (scenario)
        {
            case "domain": root["domain"] = "SECURITY"; break;
            case "digest-value": provenance["modelArtifactDigest"] = new string('e', 64); break;
            case "digest-null": provenance["modelArtifactDigest"] = null; break;
            case "digest-missing": provenance.Remove("modelArtifactDigest"); break;
            case "digest-empty": provenance["modelArtifactDigest"] = string.Empty; break;
            case "digest-wrong-json-kind": provenance["modelArtifactDigest"] = 64; break;
            case "kind-null": provenance["modelArtifactDigestKind"] = null; break;
            case "kind-missing": provenance.Remove("modelArtifactDigestKind"); break;
            case "kind-empty": provenance["modelArtifactDigestKind"] = string.Empty; break;
            case "kind-wrong-value": provenance["modelArtifactDigestKind"] = "ollama_manifest_sha256"; break;
            case "kind-wrong-json-kind": provenance["modelArtifactDigestKind"] = true; break;
            case "metadata-null": root["teacherProvenance"] = null; break;
            case "metadata-wrong-json-kind": root["teacherProvenance"] = true; break;
            case "provenance-null": metadata["provenance"] = null; break;
            case "provenance-missing": metadata.Remove("provenance"); break;
            case "provenance-wrong-json-kind": metadata["provenance"] = new JsonArray(); break;
            case "extra-property": provenance["unexpected"] = true; break;
        }

        json = root.ToJsonString();
        if (scenario == "digest-duplicate")
        {
            json = json.Replace(
                $"\"modelArtifactDigest\":\"{LayerDigest}\"",
                $"\"modelArtifactDigest\":\"{LayerDigest}\",\"modelArtifactDigest\":\"{LayerDigest}\"",
                StringComparison.Ordinal);
        }
        else if (scenario == "kind-duplicate")
        {
            const string property =
                "\"modelArtifactDigestKind\":\"ollama_model_layer_sha256\"";
            json = json.Replace(
                property,
                property + "," + property,
                StringComparison.Ordinal);
        }
        else if (scenario == "provenance-duplicate")
        {
            var provenanceJson = provenance.ToJsonString();
            json = json.Replace(
                $"\"provenance\":{provenanceJson}",
                $"\"provenance\":{provenanceJson},\"provenance\":{provenanceJson}",
                StringComparison.Ordinal);
        }
        File.WriteAllText(path, json);

        var loaded = new FileDpoPairStore(_root).GetAll();
        Assert.Equal("DPO_PAIR_STORE_CORRUPT", loaded.ErrorCode);
    }

    [Fact]
    public void HistoricalV2LocalQwen_RemainsReadableButCannotPassCurrentAdmission()
    {
        Assert.True(new FileDpoPairStore(_root).Add(
            Pair(Provenance("legacy-v2", includeArtifact: false))).IsSuccess);
        var path = Assert.Single(Directory.GetFiles(_root, "*.dpo-pair.json"));
        var root = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        Assert.Equal(2, root["schemaVersion"]!.GetValue<int>());
        Assert.Equal(
            13,
            root["teacherProvenance"]!["provenance"]!.AsObject().Count);

        var loaded = new FileDpoPairStore(_root).GetAll();
        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        var provenance = Assert.Single(loaded.Value!).TeacherProvenance!;
        Assert.Equal(string.Empty, provenance.ModelArtifactDigestKind);
        Assert.Equal(string.Empty, provenance.ModelArtifactDigest);

        var policy = new LegalTrainingTeacherPolicyEntry(
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            "qwen3.5:9b",
            ManifestDigest,
            TrainingTeacherModelLicense.Apache20,
            Evidence('a'),
            Evidence('b'))
        {
            ModelArtifactDigestKind =
                LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
            ModelArtifactDigest = LayerDigest
        };
        var authority = LegalTrainingTeacherAdmissionAuthority.Create(
            [policy],
            1,
            Enumerable.Range(1, 32).Select(static value => (byte)value).ToArray());
        Assert.True(authority.IsSuccess, authority.ErrorMessage);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ProvenanceInvalidErrorCode,
            authority.Value!.ValidateBatch([provenance]).ErrorCode);
    }

    [Fact]
    public void HistoricalV3Pair_PrepareTransactionRejectsExplicitlyWithoutResealOrRewrite()
    {
        var legacyPair = Pair(Provenance("legacy-v3-transaction", includeArtifact: true));
        var store = new FileDpoPairStore(_root);
        Assert.True(store.Add(legacyPair).IsSuccess);
        var legacyPath = Assert.Single(Directory.GetFiles(_root, "*.dpo-pair.json"));
        var legacyBytes = File.ReadAllBytes(legacyPath);
        var legacyHash = Sha256Hex(legacyBytes);
        var pairCount = Directory.GetFiles(_root, "*.dpo-pair.json").Length;

        var prepared = new FileDpoPairStore(_root).PrepareTransaction(["BUG"]);

        Assert.False(prepared.IsSuccess);
        Assert.Equal(LegacyContentBindingRejectedErrorCode, prepared.ErrorCode);
        Assert.Equal(LegacyContentBindingRejectedMessage, prepared.ErrorMessage);
        Assert.Null(prepared.Value);
        Assert.Equal(pairCount, Directory.GetFiles(_root, "*.dpo-pair.json").Length);
        Assert.Equal(legacyHash, Sha256Hex(File.ReadAllBytes(legacyPath)));
        Assert.Empty(Directory.GetFiles(_root, "*.dpo-transaction.json"));
        var loaded = new FileDpoPairStore(_root).GetAll();
        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        Assert.Equal(legacyPair, Assert.Single(loaded.Value!));
    }

    [Fact]
    public void MixedHistoricalV3AndCurrentV4Pairs_FailClosedWithoutPartialTransaction()
    {
        var legacyPair = Pair(Provenance("legacy-v3-mixed", includeArtifact: true));
        var currentPair = CurrentPair("BUG", "current-v4-mixed");
        var store = new FileDpoPairStore(_root);
        Assert.True(store.Add(legacyPair).IsSuccess);
        Assert.True(store.Add(currentPair).IsSuccess);
        var beforePaths = Directory.GetFiles(_root, "*.dpo-pair.json");
        Assert.Equal(2, beforePaths.Length);
        var beforeHashes = beforePaths.ToDictionary(
            static path => path,
            static path => Sha256Hex(File.ReadAllBytes(path)),
            StringComparer.Ordinal);

        var prepared = new FileDpoPairStore(_root).PrepareTransaction(["BUG"]);

        Assert.False(prepared.IsSuccess);
        Assert.Equal(LegacyContentBindingRejectedErrorCode, prepared.ErrorCode);
        Assert.Equal(LegacyContentBindingRejectedMessage, prepared.ErrorMessage);
        Assert.Null(prepared.Value);
        Assert.Empty(Directory.GetFiles(_root, "*.dpo-transaction.json"));
        Assert.Equal(2, Directory.GetFiles(_root, "*.dpo-pair.json").Length);
        foreach (var (path, hash) in beforeHashes)
            Assert.Equal(hash, Sha256Hex(File.ReadAllBytes(path)));
        var loaded = new FileDpoPairStore(_root).GetAll();
        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        Assert.Equal(2, loaded.Value!.Count);
        Assert.Contains(loaded.Value!, pair => pair == legacyPair);
        Assert.Contains(loaded.Value!, pair => pair == currentPair);
    }

    [Fact]
    public void HistoricalV3PairOutsideRequestedDomain_DoesNotBlockCurrentV4Transaction()
    {
        var legacyPair = Pair("BUG", Provenance("legacy-v3-outside", includeArtifact: true));
        var currentPair = CurrentPair("SECURITY", "current-v4-requested");
        var store = new FileDpoPairStore(_root);
        Assert.True(store.Add(legacyPair).IsSuccess);
        Assert.True(store.Add(currentPair).IsSuccess);

        var prepared = new FileDpoPairStore(_root).PrepareTransaction(["SECURITY"]);

        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
        var transaction = Assert.IsType<FileDpoPairStore.TransactionHandle>(
            prepared.Value);
        var snapshot = transaction.Read().Value!;
        var selected = Assert.Single(snapshot.Pairs);
        Assert.Equal(currentPair, selected.Pair);
        Assert.Equal("SECURITY", Assert.Single(snapshot.Domains).Domain);
        Assert.Single(Directory.GetFiles(_root, "*.dpo-transaction.json"));
        var pending = new FileDpoPairStore(_root).GetAll().Value!;
        Assert.Single(pending, pair => pair == legacyPair);
        Assert.DoesNotContain(pending, pair => pair == currentPair);
    }

    private static DpoPair Pair(TrainingTeacherProvenance provenance)
        => Pair("BUG", provenance);

    private static DpoPair Pair(string domain, TrainingTeacherProvenance provenance)
        => new(
            "same prompt",
            "same chosen",
            "same rejected",
            domain,
            DateTimeOffset.UnixEpoch)
        {
            TeacherProvenance = provenance,
            GovernanceDecision = new TrainingDataGovernanceDecision(
                true,
                false,
                [Evidence('a')])
        };

    private static DpoPair CurrentPair(string domain, string requestId)
    {
        var pair = new DpoPair(
            $"current prompt {requestId}",
            "same chosen",
            "same rejected",
            domain,
            DateTimeOffset.UnixEpoch)
        {
            TeacherProvenance = Provenance(requestId, includeArtifact: true),
            GovernanceDecision = new TrainingDataGovernanceDecision(
                true,
                false,
                [Evidence('a')])
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

    private static TrainingTeacherProvenance Provenance(
        string requestId,
        bool includeArtifact)
    {
        var provenance = new TrainingTeacherProvenance(
            TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            "qwen3.5:9b",
            ManifestDigest,
            TrainingTeacherModelLicense.Apache20,
            ProviderTermsChecked: false,
            ProviderTermsAllowTraining: false,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: true,
            requestId,
            InvocationCount: 1);
        return includeArtifact
            ? provenance with
            {
                ModelArtifactDigestKind =
                    LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
                ModelArtifactDigest = LayerDigest
            }
            : provenance;
    }

    private static string Evidence(char value)
        => $"sha256:{new string(value, 64)}";

    private static string Sha256Hex(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }


    [Fact]
    public void BatchReservation_RejectsCapacityBeforePairMutation()
    {
        var store = new FileDpoPairStore(_root, maximumRecordCount: 1);
        Assert.True(store.Add(Pair(Provenance("capacity-existing", true))).IsSuccess);

        var reserved = store.ReserveBatch(
            "reservation-capacity",
            [Pair(Provenance("capacity-new", true)) with { Prompt = "new" }]);

        Assert.False(reserved.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_CAPACITY", reserved.ErrorCode);
        Assert.Single(store.GetAll().Value!);
    }


    [Fact]
    public void BatchReservation_RejectsDuplicateIdentityAndTeacherRequestBeforeMarker()
    {
        var store = new FileDpoPairStore(_root);
        var same = Pair(Provenance("batch-duplicate", true));
        var duplicateIdentity = store.ReserveBatch(
            "reservation-duplicate-identity",
            [same, same]);
        Assert.Equal(
            "DPO_PAIR_BATCH_RESERVATION_DUPLICATE_IDENTITY",
            duplicateIdentity.ErrorCode);

        Assert.True(store.Add(Pair(Provenance("retained-request", true))).IsSuccess);
        var duplicateRequest = store.ReserveBatch(
            "reservation-duplicate-request",
            [Pair(Provenance("retained-request", true)) with { Prompt = "different" }]);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.DuplicateRequestIdErrorCode,
            duplicateRequest.ErrorCode);
        Assert.False(File.Exists(Path.Combine(
            _root,
            ".dpo-pair-batch-reservation.json")));
    }


    [Fact]
    public void BatchReservation_TamperAndCompetingReservationFailClosed()
    {
        var store = new FileDpoPairStore(_root);
        var first = store.ReserveBatch(
            "reservation-first",
            [Pair(Provenance("reservation-first", true))]);
        Assert.True(first.IsSuccess, first.ErrorMessage);
        var competing = new FileDpoPairStore(_root).ReserveBatch(
            "reservation-competing",
            [Pair(Provenance("reservation-competing", true)) with { Prompt = "other" }]);
        Assert.False(competing.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_BUSY", competing.ErrorCode);

        var markerPath = Path.Combine(
            _root,
            ".dpo-pair-batch-reservation.json");
        var canonicalMarker = File.ReadAllText(markerPath);
        var marker = JsonNode.Parse(canonicalMarker)!.AsObject();
        marker["ignoredExtraProperty"] = true;
        File.WriteAllText(markerPath, marker.ToJsonString());
        var extraProperty = new FileDpoPairStore(_root).ReserveBatch(
            "reservation-first",
            [Pair(Provenance("reservation-first", true))]);
        Assert.False(extraProperty.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_CORRUPT", extraProperty.ErrorCode);

        File.WriteAllText(markerPath, canonicalMarker);
        marker = JsonNode.Parse(canonicalMarker)!.AsObject();
        marker["batchSha256"] = "sha256:" + new string('f', 64);
        File.WriteAllText(markerPath, marker.ToJsonString());
        var tampered = new FileDpoPairStore(_root).ReserveBatch(
            "reservation-first",
            [Pair(Provenance("reservation-first", true))]);
        Assert.False(tampered.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_CORRUPT", tampered.ErrorCode);
    }

}
