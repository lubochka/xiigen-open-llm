// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Globalization;
using ContinuousLearning;
using Xunit.Abstractions;

public sealed class RepositoryOwnedReferenceCorpusManifestTests
{
    private readonly ITestOutputHelper output;

    public RepositoryOwnedReferenceCorpusManifestTests(ITestOutputHelper output)
        => this.output = output;

    private static readonly DateTimeOffset CapturedAtUtc =
        new(2026, 8, 26, 10, 15, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset FrozenAtUtc =
        new(2026, 8, 26, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Sealer_WritesPayloadAndManifestThenFreshOpenReconstructsEquivalentCorpus()
    {
        using var fixture = new RepositoryOwnedReferenceCorpusFixture();
        var corpus = CreateCorpus("v0001");
        Assert.Equal("sha256:b13f5b7f1e68ebf1747634c2bfb6c15359105a399d7252a3448b9a3687065cbd", corpus.CorpusSha256); // Source-derived from unchanged HEAD before this run; this assertion checks the actual v1 factory result.
        var address = CreateAddress(fixture, "v0001");

        var manifest = Require(
            RepositoryOwnedReferenceCorpusManifestSealer.PersistAndSeal(
                address,
                corpus));

        var payloadPath = Path.Combine(
            address.CorpusRoot,
            RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName);
        Assert.True(File.Exists(payloadPath));
        Assert.True(File.Exists(address.ManifestPath));
        Assert.Equal(address.CorpusRootRelativePath, manifest
            .RepositoryRelativeCorpusDirectory);
        Assert.Equal(
            address.CorpusRootRelativePath + "/corpus.json",
            manifest.RepositoryRelativePayloadPath);
        Assert.Equal(address.ManifestRelativePath, manifest
            .RepositoryRelativeManifestPath);
        Assert.Equal(corpus.CorpusSha256, manifest.CorpusSha256);
        Assert.Equal(corpus.Count, manifest.EntryCount);
        Assert.Equal(Sha256(File.ReadAllBytes(payloadPath)), manifest.PayloadSha256);
        Assert.Equal(new FileInfo(payloadPath).Length, manifest.PayloadByteLength);

        corpus = null!;
        var opened = Require(
            RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(address));

        Assert.Equal(manifest, opened.Manifest);
        Assert.Equal(2, opened.Corpus.Count);
        Assert.Equal("translation-en-ru", opened.Corpus.CorpusName);
        Assert.Equal("v0001", opened.Corpus.CorpusVersion);
        Assert.Equal("translation", opened.Corpus.DomainTag);
        Assert.Equal("en-ru", opened.Corpus.SubjectId);
        Assert.Equal(FrozenAtUtc, opened.Corpus.FrozenAtUtc);
        Assert.Equal(manifest.CorpusSha256, opened.Corpus.CorpusSha256);
        Assert.Equal(opened.Corpus.EntryNames, opened.Corpus
            .Entries.Select(static entry => entry.EntryId));
        Assert.Contains(opened.Corpus.Entries, static entry =>
            entry.Partition == FrozenReferencePartition.Train);
        Assert.Contains(opened.Corpus.Entries, static entry =>
            entry.Partition == FrozenReferencePartition.HeldOut);
        Assert.All(opened.Corpus.Entries, static entry =>
        {
            Assert.True(entry.PaidReferenceProvenance.ClosedPaidReference);
            Assert.True(entry.PaidReferenceProvenance
                .PaidReferenceOutputNeverTrainable);
            Assert.True(entry.PaidReferenceProvenance
                .PaidReferenceOutputNeverDpoPairSide);
        });
        Assert.DoesNotContain(
            "closedPaidReference",
            File.ReadAllText(payloadPath),
            StringComparison.OrdinalIgnoreCase);

        const string rawInput = "Document this C# source exactly:\r\npublic static string Value() => \"e\u0301\";\r\n";
        const string rawReference = "Value returns \"e\u0301\".\r\n";
        const string rawInputSha = "d9007d42465d3658a5552e19e2689e0846aafcbbaeffe630e334925a87292407";
        const string rawReferenceSha = "c7aa4435150e7da7568551426666dad0833ccbd08d13d3ad2968177fb0ee9692";
        var family = SourceFingerprint("independent-raw-csharp-family");
        var rawProvenance = Require(PaidReferenceProvenance.CreateRawUtf8("paid-provider", "paid-model", "2026-08-26",
            "raw-csharp-reference", CapturedAtUtc, rawReference));
        var rawEntry = Require(FrozenReferenceEntry.CreateRawCodeDocumentation(rawInput, rawReference, rawProvenance,
            "raw-csharp", FrozenReferencePartition.HeldOut, family));
        Assert.Equal("sha256:" + rawReferenceSha, rawProvenance.ReferenceAnswerRawUtf8Sha256);
        Assert.Empty(rawProvenance.ReferenceAnswerSha256);
        Assert.False(FrozenReferenceEntry.CreateRawCodeDocumentation(rawInput, rawReference + "changed", rawProvenance,
            "raw-csharp", FrozenReferencePartition.HeldOut, family).IsSuccess);
        Assert.False(FrozenReferenceEntry.CreateRawCodeDocumentation(rawInput, rawReference,
            CreateProvenance("legacy-csharp", rawReference), "raw-csharp", FrozenReferencePartition.HeldOut, family).IsSuccess);
        var composedReference = rawReference.Normalize(NormalizationForm.FormC);
        var composedProvenance = Require(PaidReferenceProvenance.CreateRawUtf8("paid-provider", "paid-model", "2026-08-26",
            "raw-csharp-reference", CapturedAtUtc, composedReference));
        var composedEntry = Require(FrozenReferenceEntry.CreateRawCodeDocumentation(rawInput.Normalize(NormalizationForm.FormC),
            composedReference, composedProvenance, "raw-csharp", FrozenReferencePartition.HeldOut, family));
        Assert.NotEqual(rawProvenance.ReferenceAnswerRawUtf8Sha256, composedProvenance.ReferenceAnswerRawUtf8Sha256);
        Assert.NotEqual(rawEntry.EntryId, composedEntry.EntryId);
        Assert.Equal(rawEntry.SourceEntityFingerprint, composedEntry.SourceEntityFingerprint);
        var legacySameDomain = Require(FrozenReferenceEntry.Create(rawInput, rawReference,
            CreateProvenance("legacy-same-domain", rawReference), rawEntry.DomainTag, rawEntry.SubjectId,
            rawEntry.Partition, SourceFingerprint("independent-legacy-csharp-family")));
        Assert.NotEqual(rawEntry.EntryId, legacySameDomain.EntryId);
        Assert.NotEqual(rawEntry.SourceEntityFingerprint, legacySameDomain.SourceEntityFingerprint);
        var mixedVersions = FrozenReferenceCorpus.Build("mixed-versions", "v0002", FrozenAtUtc,
            [rawEntry, legacySameDomain]);
        Assert.False(mixedVersions.IsSuccess);
        Assert.Equal("FROZEN_REFERENCE_CORPUS_VERSION_MISMATCH", mixedVersions.ErrorCode);
        var rawCorpus = Require(FrozenReferenceCorpus.Build("raw-csharp-corpus", "v0002", FrozenAtUtc, [rawEntry]));
        var rawAddress = Require(RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(fixture.RepositoryRoot,
            "csharp-code-documentation", "raw-csharp", "v0002"));
        var rawManifest = Require(RepositoryOwnedReferenceCorpusManifestSealer.PersistAndSeal(rawAddress, rawCorpus));
        Assert.Equal(RepositoryOwnedReferenceCorpusManifestSealer.RawUtf8Schema, rawManifest.Schema);
        Assert.Equal(RepositoryOwnedReferenceCorpusManifestSealer.RawUtf8Context, rawManifest.Context);
        rawCorpus = null!;
        var rawOpened = Require(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(rawAddress));
        var reopenedEntry = Assert.Single(rawOpened.Corpus.Entries);
        Assert.Equal(rawManifest, rawOpened.Manifest);
        Assert.Equal(rawEntry.EntryId, reopenedEntry.EntryId);
        Assert.Equal(rawEntry.CanonicalRepresentation, reopenedEntry.CanonicalRepresentation);
        Assert.Equal(rawInput, reopenedEntry.OwnerInput);
        Assert.Equal(rawReference, reopenedEntry.PaidModelReferenceAnswer);
        Assert.Equal(family, reopenedEntry.SourceEntityFingerprint);
        Assert.Equal(rawProvenance.CanonicalRepresentation, reopenedEntry.PaidReferenceProvenance.CanonicalRepresentation);
        Assert.Equal(rawProvenance.ProvenanceSha256, reopenedEntry.PaidReferenceProvenance.ProvenanceSha256);
        Assert.True(reopenedEntry.PaidReferenceProvenance.ClosedPaidReference);
        Assert.True(reopenedEntry.PaidReferenceProvenance.PaidReferenceOutputNeverTrainable);
        Assert.True(reopenedEntry.PaidReferenceProvenance.PaidReferenceOutputNeverDpoPairSide);
        Assert.Equal(rawInputSha, CodeDocumentationStudentGeneration.InputSha256(reopenedEntry.OwnerInput));
        var prompt = CodeDocumentationSavedModelAssessmentService.BuildCheckerPrompt(reopenedEntry, "synthetic Student answer");
        using var checkerPayload = JsonDocument.Parse(prompt[prompt.IndexOf('{')..]);
        Assert.Equal(reopenedEntry.EntryId, checkerPayload.RootElement.GetProperty("caseId").GetString());
        Assert.Equal(rawInput, checkerPayload.RootElement.GetProperty("ownerTask").GetString());
        Assert.Equal(rawReference, checkerPayload.RootElement.GetProperty("closedReferenceAnswer").GetString());
        Assert.Equal("synthetic Student answer", checkerPayload.RootElement.GetProperty("studentAnswer").GetString());
        using (var rawPayload = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(rawAddress.CorpusRoot,
                   RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName))))
        {
            Assert.Equal(RepositoryOwnedReferenceCorpusManifestSealer.RawUtf8PayloadSchema, rawPayload.RootElement.GetProperty("schema").GetString());
            var persistedProvenance = rawPayload.RootElement.GetProperty("entries")[0].GetProperty("paidReferenceProvenance");
            Assert.Equal("sha256:" + rawReferenceSha, persistedProvenance.GetProperty("referenceAnswerRawUtf8Sha256").GetString());
            Assert.False(persistedProvenance.TryGetProperty("referenceAnswerSha256", out _));
        }
        var mismatch = JsonNode.Parse(File.ReadAllText(rawAddress.ManifestPath))!.AsObject();
        mismatch["context"] = RepositoryOwnedReferenceCorpusManifestSealer.CurrentContext;
        File.WriteAllBytes(rawAddress.ManifestPath, IndependentlyResealManifest(mismatch));
        Assert.False(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(rawAddress).IsSuccess);
        output.WriteLine("RAW_CSHARP_ROUNDTRIP " + JsonSerializer.Serialize(new
        {
            ownerInputSha256 = CodeDocumentationStudentGeneration.InputSha256(reopenedEntry.OwnerInput),
            referenceRawUtf8Sha256 = reopenedEntry.PaidReferenceProvenance.ReferenceAnswerRawUtf8Sha256,
            rawEntryId = reopenedEntry.EntryId, familyFingerprint = reopenedEntry.SourceEntityFingerprint,
            v1CorpusSha256 = opened.Corpus.CorpusSha256, rawCorpusSha256 = rawOpened.Corpus.CorpusSha256,
            exactReopenAndFormatter = true, rightsPreserved = true, selfHashedTupleMismatchRefused = true,
            modelGenerationPerformed = false, teacherCalls = 0
        }));
    }

    [Theory]
    [InlineData(ReferenceCorpusTamperKind.PayloadByte)]
    [InlineData(ReferenceCorpusTamperKind.ManifestHash)]
    [InlineData(ReferenceCorpusTamperKind.ManifestIdentity)]
    [InlineData(ReferenceCorpusTamperKind.ManifestCount)]
    [InlineData(ReferenceCorpusTamperKind.ManifestPayloadPath)]
    public void OpenSealed_FailsClosedWhenManifestContentOrPathIsTampered(
        ReferenceCorpusTamperKind tamperKind)
    {
        using var fixture = new RepositoryOwnedReferenceCorpusFixture();
        var corpus = CreateCorpus("v0002");
        var address = CreateAddress(fixture, "v0002");
        Require(RepositoryOwnedReferenceCorpusManifestSealer.PersistAndSeal(
            address,
            corpus));

        ApplyTamper(address, tamperKind);

        var opened =
            RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(address);

        Assert.False(opened.IsSuccess);
        Assert.Equal(
            RepositoryOwnedReferenceCorpusManifestSealer.InvalidErrorCode,
            opened.ErrorCode);
    }

    [Fact]
    public void PersistAndSeal_SameAddressSameBytesIsIdempotentButChangedBytesConflict()
    {
        using var fixture = new RepositoryOwnedReferenceCorpusFixture();
        var address = CreateAddress(fixture, "v0003");
        var original = CreateCorpus("v0003", firstReference: "первый эталон");
        var changed = CreateCorpus("v0003", firstReference: "подменённый эталон");

        var firstManifest = Require(
            RepositoryOwnedReferenceCorpusManifestSealer.Seal(
                address,
                original));
        var payloadPath = Path.Combine(
            address.CorpusRoot,
            RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName);
        var originalPayloadBytes = File.ReadAllBytes(payloadPath);
        var secondManifest = Require(
            RepositoryOwnedReferenceCorpusManifestSealer.Seal(
                address,
                original));

        var conflict =
            RepositoryOwnedReferenceCorpusManifestSealer.PersistAndSeal(
                address,
                changed);
        var afterConflictPayloadBytes = File.ReadAllBytes(payloadPath);
        var reopened = Require(
            RepositoryOwnedReferenceCorpusManifestSealer.Open(address));

        Assert.Equal(firstManifest, secondManifest);
        Assert.False(conflict.IsSuccess);
        Assert.Equal(
            RepositoryOwnedReferenceCorpusManifestSealer.ConflictErrorCode,
            conflict.ErrorCode);
        Assert.Equal(originalPayloadBytes, afterConflictPayloadBytes);
        Assert.Equal(original.CorpusSha256, reopened.Corpus.CorpusSha256);
        Assert.NotEqual(changed.CorpusSha256, reopened.Corpus.CorpusSha256);
    }

    [Fact]
    public void PersistAndSeal_RejectsAddressThatDoesNotMatchCorpusIdentity()
    {
        using var fixture = new RepositoryOwnedReferenceCorpusFixture();
        var corpus = CreateCorpus("v0004");
        var wrongVersionAddress = CreateAddress(fixture, "v9999");

        var result = RepositoryOwnedReferenceCorpusManifestSealer.PersistAndSeal(
            wrongVersionAddress,
            corpus);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            RepositoryOwnedReferenceCorpusManifestSealer.InvalidErrorCode,
            result.ErrorCode);
        Assert.False(Directory.Exists(wrongVersionAddress.CorpusRoot));
    }

    [Fact]
    public void OpenSealedFileIdentity_ReturnsRawOracleAndEquivalentFullyReconstructedCorpus()
    {
        using var fixture = new RepositoryOwnedReferenceCorpusFixture();
        var original = CreateCorpus("v0010");
        var address = CreateAddress(fixture, "v0010");
        var manifest = Require(RepositoryOwnedReferenceCorpusManifestSealer
            .PersistAndSeal(address, original));
        var before = SnapshotPreparedFixture(fixture);

        var identity = Require(RepositoryOwnedReferenceCorpusManifestSealer
            .OpenSealedFileIdentity(address));
        var publicOpen = Require(RepositoryOwnedReferenceCorpusManifestSealer
            .OpenSealed(address));

        AssertRawManifestOracle(address, identity);
        Assert.NotEqual(manifest.ManifestSha256, identity.ManifestFileSha256);
        Assert.Matches("^[0-9a-f]{64}$", identity.ManifestFileSha256);
        Assert.Matches("^sha256:[0-9a-f]{64}$", identity.Manifest.CorpusSha256);
        Assert.Equal(71, identity.Manifest.CorpusSha256.Length);
        Assert.Equal(manifest, identity.Manifest);
        Assert.Equal(publicOpen.Manifest, identity.Manifest);
        AssertEquivalentCorpus(original, identity.Corpus);
        AssertEquivalentCorpus(publicOpen.Corpus, identity.Corpus);
        Assert.Equal(before, SnapshotPreparedFixture(fixture));
        LogLoadedRuntimeIdentities();
    }

    [Fact]
    public void OpenSealedFileIdentity_LayoutChangesRawIdentityButPreservesCanonicalCorpus()
    {
        using var fixture = new RepositoryOwnedReferenceCorpusFixture();
        var address = SealFixture(fixture, "v0011");
        var originalBytes = File.ReadAllBytes(address.ManifestPath);
        var original = AssertBothEntriesOpen(address);
        var manifestJson = JsonNode.Parse(originalBytes)!.AsObject();
        var layoutBytes = JsonSerializer.SerializeToUtf8Bytes(
            manifestJson,
            new JsonSerializerOptions { WriteIndented = true });
        Assert.NotEqual(originalBytes.LongLength, layoutBytes.LongLength);
        Assert.NotEqual(Sha256(originalBytes), Sha256(layoutBytes));
        File.WriteAllBytes(address.ManifestPath, layoutBytes);
        var before = SnapshotPreparedFixture(fixture);

        var changed = AssertBothEntriesOpen(address);

        AssertRawManifestOracle(address, changed);
        Assert.NotEqual(original.ManifestFileSha256, changed.ManifestFileSha256);
        Assert.NotEqual(original.ManifestFileByteLength, changed.ManifestFileByteLength);
        Assert.Equal(original.Manifest, changed.Manifest);
        AssertEquivalentCorpus(original.Corpus, changed.Corpus);
        Assert.Equal(before, SnapshotPreparedFixture(fixture));
    }

    [Fact]
    public void OpenSealedFileIdentity_PayloadCorruptionRejectsBothEntriesAndRestoreReopens()
    {
        using var fixture = new RepositoryOwnedReferenceCorpusFixture();
        var address = SealFixture(fixture, "v0012");
        var original = AssertBothEntriesOpen(address);
        var payloadPath = Path.Combine(address.CorpusRoot,
            RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName);
        var originalPayload = File.ReadAllBytes(payloadPath);
        var originalManifest = File.ReadAllBytes(address.ManifestPath);
        var before = SnapshotPreparedFixture(fixture);
        var changedPayload = (byte[])originalPayload.Clone();
        var marker = Encoding.UTF8.GetBytes("\"requestId\":\"");
        var start = originalPayload.AsSpan().IndexOf(marker);
        Assert.True(start >= 0);
        changedPayload[start + marker.Length] ^= 1;
        Assert.Equal(originalPayload.LongLength, changedPayload.LongLength);
        Assert.NotEqual(Sha256(originalPayload), Sha256(changedPayload));
        File.WriteAllBytes(payloadPath, changedPayload);
        var faulted = SnapshotPreparedFixture(fixture);

        AssertBothEntriesReject(address);

        Assert.Equal(originalManifest, File.ReadAllBytes(address.ManifestPath));
        Assert.Equal(faulted, SnapshotPreparedFixture(fixture));
        File.WriteAllBytes(payloadPath, originalPayload);
        var restored = AssertBothEntriesOpen(address);
        Assert.Equal(original.Manifest, restored.Manifest);
        AssertEquivalentCorpus(original.Corpus, restored.Corpus);
        Assert.Equal(original.ManifestFileSha256, restored.ManifestFileSha256);
        Assert.Equal(original.ManifestFileByteLength, restored.ManifestFileByteLength);
        Assert.Equal(before, SnapshotPreparedFixture(fixture));
    }

    [Theory]
    [InlineData(ManifestFileFault.Missing)]
    [InlineData(ManifestFileFault.Malformed)]
    [InlineData(ManifestFileFault.DuplicateProperty)]
    [InlineData(ManifestFileFault.UnknownProperty)]
    [InlineData(ManifestFileFault.CaseAlias)]
    [InlineData(ManifestFileFault.InvalidUtf8)]
    [InlineData(ManifestFileFault.OverManifestLimit)]
    [InlineData(ManifestFileFault.AddressBinding)]
    [InlineData(ManifestFileFault.SelfHash)]
    [InlineData(ManifestFileFault.DeclaredPayloadHash)]
    [InlineData(ManifestFileFault.DeclaredPayloadLength)]
    public void OpenSealedFileIdentity_SingleManifestFaultRejectsBothEntriesThenRestores(
        ManifestFileFault fault)
    {
        using var fixture = new RepositoryOwnedReferenceCorpusFixture();
        var address = SealFixture(fixture, "v0013");
        var original = AssertBothEntriesOpen(address);
        var originalBytes = File.ReadAllBytes(address.ManifestPath);
        var before = SnapshotPreparedFixture(fixture);

        ApplySingleManifestFault(address, originalBytes, fault);
        var faulted = SnapshotPreparedFixture(fixture);
        AssertBothEntriesReject(address);
        Assert.Equal(faulted, SnapshotPreparedFixture(fixture));

        File.WriteAllBytes(address.ManifestPath, originalBytes);
        var restored = AssertBothEntriesOpen(address);
        Assert.Equal(original.Manifest, restored.Manifest);
        AssertEquivalentCorpus(original.Corpus, restored.Corpus);
        Assert.Equal(original.ManifestFileSha256, restored.ManifestFileSha256);
        Assert.Equal(original.ManifestFileByteLength, restored.ManifestFileByteLength);
        Assert.Equal(before, SnapshotPreparedFixture(fixture));
    }

    public static IEnumerable<object[]> ManifestRequiredFieldFaults()
    {
        string[] properties =
        [
            "schema", "context", "domain", "subject", "corpusVersion",
            "corpusName", "frozenAtUtc", "entryCount", "corpusSha256",
            "repositoryRelativeCorpusDirectory", "repositoryRelativePayloadPath",
            "payloadByteLength", "payloadSha256", "repositoryRelativeManifestPath",
            "manifestSha256",
        ];
        foreach (var property in properties)
        foreach (var fault in Enum.GetValues<RequiredFieldFault>())
            yield return [property, fault];
    }

    [Theory]
    [MemberData(nameof(ManifestRequiredFieldFaults))]
    public void OpenSealedFileIdentity_RequiredFieldFaultRejectsBothEntriesThenRestores(
        string property,
        RequiredFieldFault fault)
    {
        using var fixture = new RepositoryOwnedReferenceCorpusFixture();
        var address = SealFixture(fixture, "v0014");
        var original = AssertBothEntriesOpen(address);
        var originalBytes = File.ReadAllBytes(address.ManifestPath);
        var before = SnapshotPreparedFixture(fixture);
        var json = JsonNode.Parse(originalBytes)!.AsObject();
        switch (fault)
        {
            case RequiredFieldFault.Missing:
                Assert.True(json.Remove(property));
                break;
            case RequiredFieldFault.Null:
                json[property] = null;
                break;
            case RequiredFieldFault.WrongType:
                json[property] = property is "entryCount" or "payloadByteLength"
                    ? JsonValue.Create("wrong-number-type")
                    : JsonValue.Create(123);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(fault));
        }
        File.WriteAllBytes(address.ManifestPath, JsonSerializer.SerializeToUtf8Bytes(json));
        var faulted = SnapshotPreparedFixture(fixture);
        AssertBothEntriesReject(address);
        Assert.Equal(faulted, SnapshotPreparedFixture(fixture));

        File.WriteAllBytes(address.ManifestPath, originalBytes);
        var restored = AssertBothEntriesOpen(address);
        Assert.Equal(original.Manifest, restored.Manifest);
        AssertEquivalentCorpus(original.Corpus, restored.Corpus);
        Assert.Equal(original.ManifestFileSha256, restored.ManifestFileSha256);
        Assert.Equal(original.ManifestFileByteLength, restored.ManifestFileByteLength);
        Assert.Equal(before, SnapshotPreparedFixture(fixture));
    }

    private static RepositoryOwnedReferenceCorpusAddress SealFixture(
        RepositoryOwnedReferenceCorpusFixture fixture,
        string version)
    {
        var address = CreateAddress(fixture, version);
        Require(RepositoryOwnedReferenceCorpusManifestSealer.PersistAndSeal(
            address, CreateCorpus(version)));
        return address;
    }

    private static RepositoryOwnedReferenceCorpusFileIdentityOpenResult AssertBothEntriesOpen(
        RepositoryOwnedReferenceCorpusAddress address)
    {
        var identity = Require(RepositoryOwnedReferenceCorpusManifestSealer
            .OpenSealedFileIdentity(address));
        var publicOpen = Require(RepositoryOwnedReferenceCorpusManifestSealer
            .OpenSealed(address));
        AssertRawManifestOracle(address, identity);
        Assert.Equal(publicOpen.Manifest, identity.Manifest);
        AssertEquivalentCorpus(publicOpen.Corpus, identity.Corpus);
        return identity;
    }

    private static void AssertBothEntriesReject(RepositoryOwnedReferenceCorpusAddress address)
    {
        var identity = RepositoryOwnedReferenceCorpusManifestSealer
            .OpenSealedFileIdentity(address);
        var publicOpen = RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(address);
        Assert.False(identity.IsSuccess);
        Assert.False(publicOpen.IsSuccess);
        Assert.Null(identity.Value);
        Assert.Null(publicOpen.Value);
        Assert.Equal(RepositoryOwnedReferenceCorpusManifestSealer.InvalidErrorCode,
            identity.ErrorCode);
        Assert.Equal(publicOpen.ErrorCode, identity.ErrorCode);
        Assert.Equal("Repository-owned frozen reference corpus manifest is invalid.",
            identity.ErrorMessage);
        Assert.Equal(publicOpen.ErrorMessage, identity.ErrorMessage);
    }

    private static void AssertRawManifestOracle(
        RepositoryOwnedReferenceCorpusAddress address,
        RepositoryOwnedReferenceCorpusFileIdentityOpenResult identity)
    {
        var oracle = File.ReadAllBytes(address.ManifestPath);
        Assert.Equal(Sha256(oracle), identity.ManifestFileSha256);
        Assert.Equal(oracle.LongLength, identity.ManifestFileByteLength);
    }

    private static void AssertEquivalentCorpus(FrozenReferenceCorpus expected,
        FrozenReferenceCorpus actual)
    {
        Assert.Equal(expected.CorpusName, actual.CorpusName);
        Assert.Equal(expected.CorpusVersion, actual.CorpusVersion);
        Assert.Equal(expected.DomainTag, actual.DomainTag);
        Assert.Equal(expected.SubjectId, actual.SubjectId);
        Assert.Equal(expected.FrozenAtUtc, actual.FrozenAtUtc);
        Assert.Equal(expected.Count, actual.Count);
        Assert.Equal(expected.CorpusSha256, actual.CorpusSha256);
        Assert.Equal(expected.CanonicalRepresentation, actual.CanonicalRepresentation);
        Assert.Equal(expected.EntryNames, actual.EntryNames);
        Assert.Equal(expected.SplitProof.TrainSourceEntityFingerprints,
            actual.SplitProof.TrainSourceEntityFingerprints);
        Assert.Equal(expected.SplitProof.HeldOutSourceEntityFingerprints,
            actual.SplitProof.HeldOutSourceEntityFingerprints);
        Assert.Equal(expected.SplitProof.SourceEntityFingerprints,
            actual.SplitProof.SourceEntityFingerprints);
        Assert.Equal(expected.SplitProof.CrossPartitionSourceEntityOverlapCount,
            actual.SplitProof.CrossPartitionSourceEntityOverlapCount);
        Assert.Equal(expected.SplitProof.Method, actual.SplitProof.Method);
        Assert.Equal(expected.SplitProof.OverlapThreshold, actual.SplitProof.OverlapThreshold);
        for (var index = 0; index < expected.Count; index++)
        {
            var left = expected.Entries[index];
            var right = actual.Entries[index];
            Assert.Equal(left.EntryId, right.EntryId);
            Assert.Equal(left.Partition, right.Partition);
            Assert.Equal(left.DomainTag, right.DomainTag);
            Assert.Equal(left.SubjectId, right.SubjectId);
            Assert.Equal(left.OwnerInput, right.OwnerInput);
            Assert.Equal(left.PaidModelReferenceAnswer, right.PaidModelReferenceAnswer);
            Assert.Equal(left.SourceEntityFingerprint, right.SourceEntityFingerprint);
            Assert.Equal(left.CanonicalRepresentation, right.CanonicalRepresentation);
            var provenance = right.PaidReferenceProvenance;
            Assert.Equal(left.PaidReferenceProvenance.ProviderRef, provenance.ProviderRef);
            Assert.Equal(left.PaidReferenceProvenance.ModelId, provenance.ModelId);
            Assert.Equal(left.PaidReferenceProvenance.ModelVersion, provenance.ModelVersion);
            Assert.Equal(left.PaidReferenceProvenance.RequestId, provenance.RequestId);
            Assert.Equal(left.PaidReferenceProvenance.CapturedAtUtc, provenance.CapturedAtUtc);
            Assert.Equal(left.PaidReferenceProvenance.ReferenceAnswerSha256,
                provenance.ReferenceAnswerSha256);
            Assert.Equal(left.PaidReferenceProvenance.ProvenanceSha256,
                provenance.ProvenanceSha256);
            Assert.Equal(left.PaidReferenceProvenance.CanonicalRepresentation,
                provenance.CanonicalRepresentation);
            Assert.True(provenance.ClosedPaidReference);
            Assert.True(provenance.PaidReferenceOutputNeverTrainable);
            Assert.True(provenance.PaidReferenceOutputNeverDpoPairSide);
        }
    }

    private static string[] SnapshotPreparedFixture(RepositoryOwnedReferenceCorpusFixture fixture)
        => Directory.EnumerateFileSystemEntries(fixture.RepositoryRoot, "*",
                SearchOption.AllDirectories)
            .OrderBy(static path => path, StringComparer.Ordinal)
            .Select(path =>
            {
                var relative = Path.GetRelativePath(fixture.RepositoryRoot, path);
                if (Directory.Exists(path))
                    return "directory:" + relative;
                var bytes = File.ReadAllBytes(path);
                return $"file:{relative}:{bytes.LongLength}:{Sha256(bytes)}";
            }).ToArray();

    private static void ApplySingleManifestFault(RepositoryOwnedReferenceCorpusAddress address,
        byte[] original,
        ManifestFileFault fault)
    {
        var text = Encoding.UTF8.GetString(original);
        var json = JsonNode.Parse(original)!.AsObject();
        // Check the independent test resealing oracle on the valid control
        // before using it to reach downstream address/payload guards.
        AssertCanonicalSelfHash(json);
        byte[] changed;
        switch (fault)
        {
            case ManifestFileFault.Missing:
                File.Delete(address.ManifestPath);
                return;
            case ManifestFileFault.Malformed:
                changed = original[..^1];
                break;
            case ManifestFileFault.DuplicateProperty:
                changed = Encoding.UTF8.GetBytes(text[..^1] + ",\"schema\":" +
                    JsonSerializer.Serialize(json["schema"]!.GetValue<string>()) + "}");
                break;
            case ManifestFileFault.UnknownProperty:
                json.Add("unknown", true);
                changed = JsonSerializer.SerializeToUtf8Bytes(json);
                break;
            case ManifestFileFault.CaseAlias:
                changed = Encoding.UTF8.GetBytes(text.Replace("\"schema\":",
                    "\"Schema\":", StringComparison.Ordinal));
                break;
            case ManifestFileFault.InvalidUtf8:
                changed = (byte[])original.Clone();
                var stringStart = original.AsSpan().IndexOf(Encoding.UTF8.GetBytes("\"schema\":\""));
                Assert.True(stringStart >= 0);
                changed[stringStart + "\"schema\":\"".Length] = 0xff;
                break;
            case ManifestFileFault.OverManifestLimit:
                changed = new byte[2 * 1024 * 1024 + 1];
                Array.Fill(changed, (byte)' ');
                original.CopyTo(changed, 0);
                break;
            case ManifestFileFault.AddressBinding:
                json["domain"] = "other-domain";
                changed = IndependentlyResealManifest(json);
                break;
            case ManifestFileFault.SelfHash:
                json["manifestSha256"] = new string('0', 64);
                changed = JsonSerializer.SerializeToUtf8Bytes(json);
                break;
            case ManifestFileFault.DeclaredPayloadHash:
                json["payloadSha256"] = new string('0', 64);
                changed = IndependentlyResealManifest(json);
                break;
            case ManifestFileFault.DeclaredPayloadLength:
                json["payloadByteLength"] = json["payloadByteLength"]!.GetValue<long>() + 1;
                changed = IndependentlyResealManifest(json);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(fault));
        }
        Assert.NotEqual(Sha256(original), Sha256(changed));
        File.WriteAllBytes(address.ManifestPath, changed);
        Assert.Equal(changed, File.ReadAllBytes(address.ManifestPath));
    }

    private static byte[] IndependentlyResealManifest(JsonObject json)
    {
        json["manifestSha256"] = string.Empty;
        json["manifestSha256"] = Sha256(JsonSerializer.SerializeToUtf8Bytes(json));
        AssertCanonicalSelfHash(json);
        return JsonSerializer.SerializeToUtf8Bytes(json);
    }

    private static void AssertCanonicalSelfHash(JsonObject json)
    {
        var declared = json["manifestSha256"]!.GetValue<string>();
        var unsigned = (JsonObject)json.DeepClone();
        unsigned["manifestSha256"] = string.Empty;
        Assert.Equal(declared, Sha256(JsonSerializer.SerializeToUtf8Bytes(unsigned)));
    }

    private void LogLoadedRuntimeIdentities()
    {
        var testAssembly = typeof(RepositoryOwnedReferenceCorpusManifestTests).Assembly;
        var productionAssembly = typeof(RepositoryOwnedReferenceCorpusManifestSealer).Assembly;
        var identities = new[]
        {
            (Kind: "production-dll", Path: productionAssembly.Location),
            (Kind: "test-dll", Path: testAssembly.Location),
            (Kind: "runtimeconfig", Path: Path.ChangeExtension(testAssembly.Location,
                ".runtimeconfig.json")),
            (Kind: "deps", Path: Path.ChangeExtension(testAssembly.Location, ".deps.json")),
        };
        foreach (var identity in identities)
        {
            var bytes = File.ReadAllBytes(identity.Path);
            output.WriteLine("CORPUS_FILE_IDENTITY_RUNTIME " + JsonSerializer.Serialize(new
            {
                kind = identity.Kind,
                path = identity.Path,
                sha256 = Sha256(bytes),
                byteLength = bytes.LongLength,
            }));
        }
    }

    public enum ManifestFileFault
    {
        Missing, Malformed, DuplicateProperty, UnknownProperty, CaseAlias,
        InvalidUtf8, OverManifestLimit, AddressBinding, SelfHash,
        DeclaredPayloadHash, DeclaredPayloadLength,
    }

    public enum RequiredFieldFault { Missing, Null, WrongType }

    private static FrozenReferenceCorpus CreateCorpus(
        string corpusVersion,
        string firstReference = "устойчивый train эталон")
        => Require(FrozenReferenceCorpus.Build(
            "translation-en-ru",
            corpusVersion,
            FrozenAtUtc,
            new[]
            {
                CreateEntry(
                    "source-train-" + corpusVersion,
                    FrozenReferencePartition.Train,
                    firstReference),
                CreateEntry(
                    "source-heldout-" + corpusVersion,
                    FrozenReferencePartition.HeldOut,
                    "устойчивый held-out эталон")
            }));

    private static FrozenReferenceEntry CreateEntry(
        string sourceSeed,
        FrozenReferencePartition partition,
        string referenceAnswer)
    {
        var provenance = CreateProvenance(sourceSeed, referenceAnswer);
        return Require(FrozenReferenceEntry.Create(
            "переведи строку " + sourceSeed,
            referenceAnswer,
            provenance,
            "translation",
            "en-ru",
            partition,
            SourceFingerprint(sourceSeed)));
    }

    private static PaidReferenceProvenance CreateProvenance(
        string sourceSeed,
        string referenceAnswer)
        => Require(PaidReferenceProvenance.Create(
            "paid-provider",
            "paid-model",
            "2026-08-26",
            "request-" + sourceSeed,
            CapturedAtUtc,
            FrozenReferenceEntry.ComputeNormalizedTextSha256(referenceAnswer)));

    private static string SourceFingerprint(string seed)
        => FrozenReferenceEntry.ComputeNormalizedTextSha256("source-entity:" + seed);

    private static RepositoryOwnedReferenceCorpusAddress CreateAddress(
        RepositoryOwnedReferenceCorpusFixture fixture,
        string corpusVersion)
        => Require(RepositoryOwnedTrainingArtifactLayout
            .CreateReferenceCorpusAddress(
                fixture.RepositoryRoot,
                "translation",
                "en-ru",
                corpusVersion));

    private static void ApplyTamper(
        RepositoryOwnedReferenceCorpusAddress address,
        ReferenceCorpusTamperKind tamperKind)
    {
        var payloadPath = Path.Combine(
            address.CorpusRoot,
            RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName);
        switch (tamperKind)
        {
            case ReferenceCorpusTamperKind.PayloadByte:
                var payload = File.ReadAllBytes(payloadPath);
                payload[^2] = payload[^2] == (byte)'0' ? (byte)'1' : (byte)'0';
                File.WriteAllBytes(payloadPath, payload);
                return;
            case ReferenceCorpusTamperKind.ManifestHash:
                ReplaceJsonString(
                    address.ManifestPath,
                    "manifestSha256",
                    new string('0', 64));
                return;
            case ReferenceCorpusTamperKind.ManifestIdentity:
                ReplaceJsonString(address.ManifestPath, "domain", "other-domain");
                return;
            case ReferenceCorpusTamperKind.ManifestCount:
                ReplaceJsonNumber(address.ManifestPath, "entryCount", 3);
                return;
            case ReferenceCorpusTamperKind.ManifestPayloadPath:
                ReplaceJsonString(
                    address.ManifestPath,
                    "repositoryRelativePayloadPath",
                    "../escape/corpus.json");
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(tamperKind));
        }
    }

    private static void ReplaceJsonString(
        string path,
        string propertyName,
        string replacement)
    {
        var marker = "\"" + propertyName + "\":\"";
        var text = File.ReadAllText(path);
        var start = text.IndexOf(marker, StringComparison.Ordinal);
        Assert.True(start >= 0, "manifest string property not found: " + propertyName);
        start += marker.Length;
        var end = text.IndexOf('"', start);
        Assert.True(end > start, "manifest string property end not found: " + propertyName);
        File.WriteAllText(
            path,
            text[..start] + replacement + text[end..],
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static void ReplaceJsonNumber(
        string path,
        string propertyName,
        int replacement)
    {
        var marker = "\"" + propertyName + "\":";
        var text = File.ReadAllText(path);
        var start = text.IndexOf(marker, StringComparison.Ordinal);
        Assert.True(start >= 0, "manifest number property not found: " + propertyName);
        start += marker.Length;
        var end = start;
        while (end < text.Length && char.IsDigit(text[end]))
            end++;
        Assert.True(end > start, "manifest number property end not found: " + propertyName);
        File.WriteAllText(
            path,
            text[..start] + replacement.ToString(CultureInfo.InvariantCulture) +
            text[end..],
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static string Sha256(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static T Require<T>(AgentInfrastructure.OperationResult<T> result)
    {
        Assert.True(
            result.IsSuccess,
            $"Expected success but got {result.ErrorCode}: {result.ErrorMessage}");
        Assert.NotNull(result.Value);
        return result.Value!;
    }

    public enum ReferenceCorpusTamperKind
    {
        PayloadByte,
        ManifestHash,
        ManifestIdentity,
        ManifestCount,
        ManifestPayloadPath,
    }

    private sealed class RepositoryOwnedReferenceCorpusFixture : IDisposable
    {
        internal RepositoryOwnedReferenceCorpusFixture()
        {
            Root = Path.Combine(
                Path.GetTempPath(),
                $"repo-owned-reference-corpus-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Root);
            RepositoryRoot = Path.Combine(Root, "repo");
            Directory.CreateDirectory(RepositoryRoot);
            File.WriteAllText(
                Path.Combine(RepositoryRoot, "LlmFromScratch.sln"),
                "fixture solution sentinel");
            Directory.CreateDirectory(Path.Combine(
                RepositoryRoot,
                "14.ContinuousLearning"));
            Directory.CreateDirectory(Path.Combine(RepositoryRoot, "11.DPO"));
        }

        internal string Root { get; }
        internal string RepositoryRoot { get; }

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }
}
