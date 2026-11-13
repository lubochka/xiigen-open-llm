// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace LlmFromScratch.Tests;

using System.Collections;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentInfrastructure;
using ContinuousLearning;

public sealed class CodeDocumentationOneLessonCorpusSelectionTests(
    Xunit.Abstractions.ITestOutputHelper output)
{
    [Fact]
    public void FreshSelection_RealClosedCorpusAndRuntimeIdentities()
    {
        using var fixture = new Fixture(1);
        var selection = fixture.Selection();
        var manifestBytes = File.ReadAllBytes(fixture.Address.ManifestPath);
        var payloadBytes = File.ReadAllBytes(fixture.PayloadPath);
        var fresh = Require(selection.OpenFresh(fixture.Repository, Settings()));
        Assert.Equal(selection.Corpus, fresh.Selection.Corpus);
        Assert.Equal(selection.CaseIds, fresh.Selection.CaseIds);
        Assert.Equal(selection.CorpusDescriptorSha256, fresh.Selection.CorpusDescriptorSha256);
        Assert.Equal(fixture.Corpus.CanonicalRepresentation, fresh.Corpus.CanonicalRepresentation);
        Assert.Equal(fixture.Corpus.CorpusSha256, fresh.Corpus.CorpusSha256);
        Assert.Equal("sha256:" + selection.Corpus.CorpusSha256, fresh.Corpus.CorpusSha256);
        Assert.NotEqual(selection.Corpus.CorpusId, fresh.Corpus.CorpusName);
        Assert.Equal(manifestBytes, File.ReadAllBytes(fixture.Address.ManifestPath));
        Assert.Equal(payloadBytes, File.ReadAllBytes(fixture.PayloadPath));
        AssertLiteral(selection);
        AssertNoExecutionArtifacts(fixture);
        LogRuntimeIdentities();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(200)]
    public void FreshSelection_AllHeldOutIdsAndFullTenFieldLiteralOracle(int count)
    {
        using var fixture = new Fixture(count);
        var selection = fixture.Selection();
        var fresh = Require(selection.OpenFresh(fixture.Repository, Settings()));
        Assert.Equal(count, selection.CaseIds.Count);
        Assert.Equal(fixture.HeldOutIds, fresh.Selection.CaseIds);
        Assert.Equal(count + 1, fresh.Corpus.Count); // Train payload is verified too.
        AssertLiteral(selection);
        if (count == 200)
            Assert.True(Encoding.UTF8.GetByteCount(Oracle(selection.Corpus, selection.CaseIds)) > 697);
        AssertNoExecutionArtifacts(fixture);
    }

    [Theory]
    [InlineData("id")]
    [InlineData("domain")]
    [InlineData("subject")]
    [InlineData("version")]
    [InlineData("manifest-hash")]
    [InlineData("corpus-hash")]
    [InlineData("manifest-length")]
    [InlineData("payload-hash")]
    [InlineData("payload-length")]
    [InlineData("case-id")]
    public void Commitment_EveryValidFieldAndMembershipChangesIndependentHash(string field)
    {
        var original = Require(CodeDocumentationOneLessonCorpusSelection.Create(Shape(), [Id('1')]));
        var changed = Require(CodeDocumentationOneLessonCorpusSelection.Create(
            Change(Shape(), field), [field == "case-id" ? Id('2') : Id('1')]));
        AssertLiteral(original);
        AssertLiteral(changed);
        Assert.NotEqual(original.CorpusDescriptorSha256, changed.CorpusDescriptorSha256);
        Assert.False(CodeDocumentationOneLessonCorpusSelection.Create(
            Shape() with { Purpose = "Training" }, [Id('1')]).IsSuccess);
        Assert.Contains("\"Purpose\":\"ClosedEvaluation\"", Oracle(original.Corpus, original.CaseIds));
    }

    [Theory]
    [InlineData("null")]
    [InlineData("id-null")]
    [InlineData("id-empty")]
    [InlineData("id-leading")]
    [InlineData("id-space")]
    [InlineData("id-unicode")]
    [InlineData("id-129")]
    [InlineData("purpose")]
    [InlineData("purpose-null")]
    [InlineData("domain-null")]
    [InlineData("domain-dot")]
    [InlineData("subject-upper")]
    [InlineData("subject-root")]
    [InlineData("version-97")]
    [InlineData("manifest-upper")]
    [InlineData("manifest-null")]
    [InlineData("corpus-prefixed")]
    [InlineData("corpus-upper")]
    [InlineData("corpus-null")]
    [InlineData("payload-short")]
    [InlineData("payload-upper")]
    [InlineData("payload-null")]
    [InlineData("manifest-zero")]
    [InlineData("manifest-negative")]
    [InlineData("manifest-over")]
    [InlineData("payload-zero")]
    [InlineData("payload-negative")]
    [InlineData("payload-over")]
    public void PureCreate_InvalidPrimitiveFailsThenRestores(string fault)
    {
        Assert.True(CodeDocumentationOneLessonCorpusSelection.Create(Shape(), [Id('1')]).IsSuccess);
        var c = Shape();
        CodeDocumentationOneLessonCorpusV2? invalid = fault switch
        {
            "null" => null,
            "id-null" => c with { CorpusId = null! },
            "id-empty" => c with { CorpusId = "" },
            "id-leading" => c with { CorpusId = "-corpus" },
            "id-space" => c with { CorpusId = "corpus " },
            "id-unicode" => c with { CorpusId = "cörpus" },
            "id-129" => c with { CorpusId = new('a', 129) },
            "purpose" => c with { Purpose = "closedevaluation" },
            "purpose-null" => c with { Purpose = null! },
            "domain-null" => c with { DomainSlug = null! },
            "domain-dot" => c with { DomainSlug = "a.b" },
            "subject-upper" => c with { SubjectSlug = "Subject" },
            "subject-root" => c with { SubjectSlug = "/subject" },
            "version-97" => c with { CorpusVersion = new('a', 97) },
            "manifest-upper" => c with { ManifestFileSha256 = Hex('A') },
            "manifest-null" => c with { ManifestFileSha256 = null! },
            "corpus-prefixed" => c with { CorpusSha256 = Id('2') },
            "corpus-upper" => c with { CorpusSha256 = Hex('B') },
            "corpus-null" => c with { CorpusSha256 = null! },
            "payload-short" => c with { PayloadSha256 = "a" },
            "payload-upper" => c with { PayloadSha256 = Hex('C') },
            "payload-null" => c with { PayloadSha256 = null! },
            "manifest-zero" => c with { ManifestFileByteLength = 0 },
            "manifest-negative" => c with { ManifestFileByteLength = -1 },
            "manifest-over" => c with { ManifestFileByteLength = RepositoryOwnedReferenceCorpusManifestSealer.MaximumManifestBytes + 1 },
            "payload-zero" => c with { PayloadByteLength = 0 },
            "payload-negative" => c with { PayloadByteLength = -1 },
            "payload-over" => c with { PayloadByteLength = RepositoryOwnedReferenceCorpusManifestSealer.MaximumPayloadBytes + 1 },
            _ => throw new InvalidOperationException(fault),
        };
        Assert.Equal(CodeDocumentationOneLessonCorpusSelection.InvalidErrorCode,
            CodeDocumentationOneLessonCorpusSelection.Create(invalid, [Id('1')]).ErrorCode);
        AssertLiteral(Require(CodeDocumentationOneLessonCorpusSelection.Create(c, [Id('1')])));
    }

    [Fact]
    public void PureCreate_ExactInclusiveOwnerCapsAndAsciiSlugTokenBoundaries()
    {
        var c = Shape() with { CorpusId = "A0._:-" + new string('z', 122),
            DomainSlug = new('a', 96), SubjectSlug = new('b', 96), CorpusVersion = new('c', 96),
            ManifestFileByteLength = RepositoryOwnedReferenceCorpusManifestSealer.MaximumManifestBytes,
            PayloadByteLength = RepositoryOwnedReferenceCorpusManifestSealer.MaximumPayloadBytes };
        var s = Require(CodeDocumentationOneLessonCorpusSelection.Create(c, [Id('0')]));
        Assert.Equal(128, s.Corpus.CorpusId.Length);
        AssertLiteral(s);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("empty")]
    [InlineData("201")]
    [InlineData("duplicate")]
    [InlineData("unsorted")]
    [InlineData("bare")]
    [InlineData("uppercase")]
    [InlineData("prefix")]
    [InlineData("null-id")]
    [InlineData("short")]
    public void PureCreate_RejectsInvalidCompleteSequenceWithoutSortingOrTruncating(string fault)
    {
        string[]? invalid = fault switch
        {
            "null" => null,
            "empty" => [],
            "201" => Enumerable.Range(0, 201).Select(i => "sha256:" + i.ToString("x64", CultureInfo.InvariantCulture)).ToArray(),
            "duplicate" => [Id('1'), Id('1')],
            "unsorted" => [Id('2'), Id('1')],
            "bare" => [Hex('1')],
            "uppercase" => [Id('A')],
            "prefix" => ["SHA256:" + Hex('1')],
            "null-id" => [null!],
            "short" => ["sha256:a"],
            _ => throw new InvalidOperationException(fault),
        };
        Assert.True(CodeDocumentationOneLessonCorpusSelection.Create(Shape(), [Id('1'), Id('2')]).IsSuccess);
        Assert.Equal(CodeDocumentationOneLessonCorpusSelection.InvalidErrorCode,
            CodeDocumentationOneLessonCorpusSelection.Create(Shape(), invalid).ErrorCode);
        AssertLiteral(Require(CodeDocumentationOneLessonCorpusSelection.Create(Shape(), [Id('1'), Id('2')])));
    }

    [Fact]
    public void PureCreate_CopiesArrayListAndRecordAndExposesOnlyReadOnlyIds()
    {
        var c = Shape();
        var ids = new[] { Id('1'), Id('2') };
        var selection = Require(CodeDocumentationOneLessonCorpusSelection.Create(c, ids));
        var list = ids.ToList();
        var fromList = Require(CodeDocumentationOneLessonCorpusSelection.Create(c, list));
        var hash = selection.CorpusDescriptorSha256;
        ids[0] = Id('9'); list[0] = Id('a'); c = c with { CorpusId = "changed" };
        Assert.Equal(new[] { Id('1'), Id('2') }, selection.CaseIds);
        Assert.Equal(selection.CaseIds, fromList.CaseIds);
        Assert.Equal(hash, fromList.CorpusDescriptorSha256);
        Assert.NotSame(c, selection.Corpus);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)selection.CaseIds)[0] = Id('b'));
        Assert.Throws<NotSupportedException>(() => ((IList)selection.CaseIds).Add(Id('b')));
        Assert.Equal(hash, selection.CorpusDescriptorSha256);
        AssertLiteral(selection);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("extra")]
    [InlineData("train")]
    [InlineData("selected-max")]
    public void FreshSelection_RejectsChangedCompleteSetOrCurrentLimitThenRestores(string fault)
    {
        using var fixture = new Fixture(2);
        var good = fixture.Selection();
        Assert.True(good.OpenFresh(fixture.Repository, Settings()).IsSuccess);
        var ids = fault switch
        {
            "missing" => fixture.HeldOutIds[..1],
            "extra" => fixture.HeldOutIds.Append(Id('0')).OrderBy(x => x, StringComparer.Ordinal).ToArray(),
            "train" => new[] { fixture.HeldOutIds[0], fixture.Corpus.Entries.Single(e => e.Partition == FrozenReferencePartition.Train).EntryId }
                .OrderBy(x => x, StringComparer.Ordinal).ToArray(),
            _ => fixture.HeldOutIds,
        };
        var changed = Require(CodeDocumentationOneLessonCorpusSelection.Create(good.Corpus, ids));
        var result = changed.OpenFresh(fixture.Repository, Settings(fault == "selected-max" ? 1 : 200));
        Assert.Equal(CodeDocumentationOneLessonCorpusSelection.InvalidErrorCode, result.ErrorCode);
        Assert.True(good.OpenFresh(fixture.Repository, Settings()).IsSuccess);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(201)]
    public void FreshSelection_ValidOwnerWithZeroOr201HeldOutNeverAcceptsPrefix(int count)
    {
        using var fixture = new Fixture(count);
        Assert.Equal(count, Require(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(fixture.Address))
            .Corpus.Entries.Count(e => e.Partition == FrozenReferencePartition.HeldOut));
        var ids = count == 0 ? new[] { fixture.Corpus.Entries[0].EntryId } : fixture.HeldOutIds[..200];
        var startup = Require(CodeDocumentationOneLessonCorpusSelection.Create(fixture.Descriptor, ids));
        Assert.Equal(CodeDocumentationOneLessonCorpusSelection.InvalidErrorCode,
            startup.OpenFresh(fixture.Repository, Settings()).ErrorCode);
        using var control = new Fixture(1);
        Assert.True(control.Selection().OpenFresh(control.Repository, Settings()).IsSuccess);
    }

    [Theory]
    [InlineData("domain")]
    [InlineData("subject")]
    [InlineData("version")]
    [InlineData("manifest-hash")]
    [InlineData("corpus-hash")]
    [InlineData("manifest-length")]
    [InlineData("payload-hash")]
    [InlineData("payload-length")]
    public void FreshSelection_EachChangedOwnerPinFailsThenRestores(string field)
    {
        using var fixture = new Fixture(1);
        var good = fixture.Selection();
        Assert.True(good.OpenFresh(fixture.Repository, Settings()).IsSuccess);
        var changed = Require(CodeDocumentationOneLessonCorpusSelection.Create(Change(good.Corpus, field), good.CaseIds));
        Assert.False(changed.OpenFresh(fixture.Repository, Settings()).IsSuccess);
        Assert.True(good.OpenFresh(fixture.Repository, Settings()).IsSuccess);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("tokens-zero")]
    [InlineData("tokens-over")]
    [InlineData("temperature-nan")]
    [InlineData("temperature-infinity")]
    [InlineData("temperature-negative")]
    [InlineData("temperature-over")]
    [InlineData("topk-negative")]
    [InlineData("topk-over")]
    [InlineData("topp-zero")]
    [InlineData("topp-over")]
    [InlineData("topp-nan")]
    [InlineData("topp-infinity")]
    [InlineData("cases-zero")]
    [InlineData("cases-over")]
    [InlineData("answer-zero")]
    [InlineData("answer-over")]
    public void FreshSelection_ExistingGenerationValidatorRejectsBeforeRootIoThenRestores(string fault)
    {
        using var fixture = new Fixture(1);
        var good = fixture.Selection(); var settings = Settings();
        CodeDocumentationSavedModelAssessmentGenerationSettings? invalid = fault switch
        {
            "null" => null,
            "tokens-zero" => settings with { MaxNewTokens = 0 },
            "tokens-over" => settings with { MaxNewTokens = 2049 },
            "temperature-nan" => settings with { Temperature = float.NaN },
            "temperature-infinity" => settings with { Temperature = float.PositiveInfinity },
            "temperature-negative" => settings with { Temperature = -1 },
            "temperature-over" => settings with { Temperature = 2.01f },
            "topk-negative" => settings with { TopK = -1 },
            "topk-over" => settings with { TopK = 10001 },
            "topp-zero" => settings with { TopP = 0 },
            "topp-over" => settings with { TopP = 1.01f },
            "topp-nan" => settings with { TopP = float.NaN },
            "topp-infinity" => settings with { TopP = float.PositiveInfinity },
            "cases-zero" => settings with { MaxHeldOutCases = 0 },
            "cases-over" => settings with { MaxHeldOutCases = 201 },
            "answer-zero" => settings with { MaxAnswerChars = 0 },
            "answer-over" => settings with { MaxAnswerChars = 65537 },
            _ => throw new InvalidOperationException(fault),
        };
        Assert.True(good.OpenFresh(fixture.Repository, settings).IsSuccess);
        // Null would fail typed root resolution if that route were reached.
        Assert.Equal(CodeDocumentationOneLessonCorpusSelection.InvalidErrorCode,
            good.OpenFresh(null!, invalid).ErrorCode);
        Assert.True(good.OpenFresh(fixture.Repository, settings).IsSuccess);
        Assert.True(good.OpenFresh(fixture.Repository, settings with { Seed = int.MinValue }).IsSuccess);
        Assert.True(good.OpenFresh(fixture.Repository, settings with { Seed = int.MaxValue,
            MaxNewTokens = 2048, Temperature = 2, TopK = 10000, TopP = 1, MaxAnswerChars = 65536 }).IsSuccess);
    }

    [Theory]
    [InlineData("manifest-missing")]
    [InlineData("payload-missing")]
    [InlineData("manifest-byte")]
    [InlineData("payload-byte")]
    [InlineData("unknown")]
    [InlineData("duplicate")]
    [InlineData("case-alias")]
    [InlineData("missing-member")]
    [InlineData("null-member")]
    [InlineData("wrong-type")]
    [InlineData("malformed")]
    [InlineData("utf8")]
    public void FreshSelection_RealOwnerRawFaultRejectsAndExactRestorationPasses(string fault)
    {
        using var fixture = new Fixture(1);
        var selection = fixture.Selection();
        Assert.True(selection.OpenFresh(fixture.Repository, Settings()).IsSuccess);
        var path = fault.StartsWith("payload", StringComparison.Ordinal) ? fixture.PayloadPath : fixture.Address.ManifestPath;
        var original = File.ReadAllBytes(path);
        if (fault.EndsWith("missing", StringComparison.Ordinal)) File.Delete(path);
        else if (fault.EndsWith("byte", StringComparison.Ordinal))
        {
            var bytes = original.ToArray(); bytes[^2] ^= 1; File.WriteAllBytes(path, bytes);
            Assert.Equal(original.Length, new FileInfo(path).Length);
        }
        else if (fault == "utf8") File.WriteAllBytes(path, [0xff]);
        else if (fault == "malformed") File.WriteAllText(path, "{");
        else
        {
            var node = JsonNode.Parse(original)!.AsObject();
            var text = Encoding.UTF8.GetString(original);
            switch (fault)
            {
                case "unknown": node["extra"] = true; break;
                case "duplicate": text = "{\"schema\":" + JsonSerializer.Serialize(node["schema"]!.GetValue<string>()) + "," + text[1..]; break;
                case "case-alias": node["Schema"] = node["schema"]!.GetValue<string>(); break;
                case "missing-member": node.Remove("schema"); break;
                case "null-member": node["schema"] = null; break;
                case "wrong-type": node["schema"] = 1; break;
                default: throw new InvalidOperationException(fault);
            }
            File.WriteAllText(path, fault == "duplicate" ? text : node.ToJsonString());
        }
        Assert.False(selection.OpenFresh(fixture.Repository, Settings()).IsSuccess);
        File.WriteAllBytes(path, original);
        Assert.True(selection.OpenFresh(fixture.Repository, Settings()).IsSuccess);
        Assert.Equal(original, File.ReadAllBytes(path));
    }

    [Fact]
    public void FreshSelection_SameLengthOwnerAcceptedManifestOrderingStillFailsRawPin()
    {
        using var fixture = new Fixture(1);
        var selection = fixture.Selection(); var original = File.ReadAllBytes(fixture.Address.ManifestPath);
        var node = JsonNode.Parse(original)!.AsObject();
        var reversed = new JsonObject(node.Reverse().Select(p => new KeyValuePair<string, JsonNode?>(p.Key, p.Value?.DeepClone())));
        // Match the existing owner's default escaping (including UTC '+'),
        // changing member order alone while preserving raw length and semantics.
        var changed = Encoding.UTF8.GetBytes(reversed.ToJsonString());
        Assert.Equal(original.Length, changed.Length);
        Assert.NotEqual(Sha(original), Sha(changed));
        File.WriteAllBytes(fixture.Address.ManifestPath, changed);
        Assert.True(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(fixture.Address).IsSuccess);
        Assert.Equal(CodeDocumentationOneLessonCorpusSelection.InvalidErrorCode,
            selection.OpenFresh(fixture.Repository, Settings()).ErrorCode);
        File.WriteAllBytes(fixture.Address.ManifestPath, original);
        Assert.True(selection.OpenFresh(fixture.Repository, Settings()).IsSuccess);
    }

    [Theory]
    [InlineData("hash")]
    [InlineData("count")]
    [InlineData("v1")]
    public void Catalog_RejectsCorpusCommitmentCountOrV1AttachmentThenRestores(string fault)
    {
        var s = Require(CodeDocumentationOneLessonCorpusSelection.Create(Shape(), [Id('1')]));
        var grant = Grant(s); Assert.True(Catalog(grant).Resolve(Selector()).IsSuccess);
        var invalid = fault switch
        {
            "hash" => grant with { DescriptorsV2 = grant.DescriptorsV2! with { CorpusDescriptorSha256 = Hex('f') } },
            "count" => grant with { DescriptorsV2 = grant.DescriptorsV2! with { CaseCount = 2 } },
            "v1" => grant with { FormatVersion = 1, DescriptorsV2 = null },
            _ => throw new InvalidOperationException(fault),
        };
        Assert.Equal(CodeDocumentationOneLessonGrantCatalog.InvalidCatalogErrorCode,
            Catalog(invalid).Resolve(Selector()).ErrorCode);
        Assert.True(Catalog(grant).Resolve(Selector()).IsSuccess);
    }

    [Fact]
    public void Catalog_CopiesAllThreeSelectionsAndReplayIsPureAndOldV1ShapeUnchanged()
    {
        var source = Require(CodeDocumentationOneLessonSourceSelection.Create(
            [new("source0", "TrainingPairs", "bug", "learning-unit", "v1", Hex('1'), 1, Hex('2'))],
            [new(Hex('a'), "source0")]));
        var checkpoint = Require(CodeDocumentationOneLessonCheckpointSelection.Create(
            new(1, "bug", "learning-unit", "tiny-model", "v1", Hex('1'), Hex('2'), 7), new("candidate:1", Hex('3'))));
        var ids = new[] { Id('1') };
        var corpus = Require(CodeDocumentationOneLessonCorpusSelection.Create(Shape(), ids));
        var grant = Grant(corpus) with { SourceSelection = source, CheckpointSelection = checkpoint,
            CandidateSha256 = checkpoint.Candidate.CandidateSha256,
            ParentManifestSha256 = checkpoint.Parent.ManifestSha256,
            ParentWeightsSha256 = checkpoint.Parent.WeightsSha256,
            DescriptorsV2 = Grant(corpus).DescriptorsV2! with { SourceSelectionSha256 = source.SourceSelectionSha256,
                ParentCheckpointSha256 = checkpoint.ParentCheckpointSha256, CandidateSelectionSha256 = checkpoint.CandidateSelectionSha256 } };
        var catalog = Catalog(grant);
        ids[0] = Id('f'); grant = grant with { CorpusSelection = null, SourceSelection = null, CheckpointSelection = null };
        var frozen = Require(catalog.Resolve(Selector()));
        var replay = Require(catalog.Revalidate(frozen.GrantId, frozen.GrantVersion));
        Assert.Same(frozen, replay);
        Assert.NotSame(corpus, frozen.CorpusSelection); Assert.NotSame(corpus.Corpus, frozen.CorpusSelection!.Corpus);
        Assert.NotSame(source, frozen.SourceSelection); Assert.NotSame(checkpoint, frozen.CheckpointSelection);
        Assert.Equal(corpus.CorpusDescriptorSha256, frozen.CorpusSelection.CorpusDescriptorSha256);
        Assert.Equal(source.SourceSelectionSha256, frozen.SourceSelection!.SourceSelectionSha256);
        Assert.Equal(checkpoint.ParentCheckpointSha256, frozen.CheckpointSelection!.ParentCheckpointSha256);
        Assert.Equal(checkpoint.CandidateSelectionSha256, frozen.CheckpointSelection.CandidateSelectionSha256);
        Assert.Equal(new[] { Id('1') }, frozen.CorpusSelection.CaseIds);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)frozen.CorpusSelection.CaseIds)[0] = Id('2'));
        var v1 = grant with { FormatVersion = 1, DescriptorsV2 = null };
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(v1));
        Assert.Equal(new[] { "GrantId", "GrantVersion", "GrantSelector", "ObjectiveId", "TrainingRunId", "CandidateSha256",
            "ParentManifestSha256", "ParentWeightsSha256", "ConfigurationSha256", "ExamConditionSha256" },
            json.RootElement.EnumerateObject().Select(p => p.Name));
        Assert.True(CodeDocumentationOneLessonGrantCatalog.Load(_ => "{\"SchemaVersion\":1,\"Grants\":[" + json.RootElement.GetRawText() + "]}")
            .Resolve(Selector()).IsSuccess);
    }

    [Fact]
    public void RestrictedWriter_DefaultArraysFloatNullRejectAndSourceCheckpointLiteralsUnchanged()
    {
        foreach (var raw in new[] { "{\"x\":[]}", "{\"x\":1.5}", "{\"x\":null}" })
        {
            using var json = JsonDocument.Parse(raw);
            Assert.Throws<JsonException>(() => CodeDocumentationOneLessonV2Wire.CanonicalBytes(json.RootElement));
            Assert.Throws<JsonException>(() => CodeDocumentationOneLessonV2Wire.IsCanonical(json.RootElement));
        }
        var source = Require(CodeDocumentationOneLessonSourceSelection.Create(
            [new("source0", "TrainingPairs", "bug", "learning-unit", "v1", Hex('1'), 1, Hex('2'))], [new(Hex('a'), "source0")]));
        var sourceLiteral = "{\"PairSelections\":[{\"SourceId\":\"source0\",\"TrainingPairId\":\"" + Hex('a') +
            "\"}],\"Sources\":[{\"DatasetVersion\":\"v1\",\"DesignPatternSlug\":\"learning-unit\",\"DomainSlug\":\"bug\",\"ManifestFileByteLength\":1,\"ManifestFileSha256\":\"" +
            Hex('1') + "\",\"Role\":\"TrainingPairs\",\"SealedManifestSha256\":\"" + Hex('2') + "\",\"SourceId\":\"source0\"}]}";
        Assert.Equal(Encoding.UTF8.GetBytes(sourceLiteral), CodeDocumentationOneLessonV2Wire.CanonicalSourceSelectionBytes(source));
        Assert.Equal(DomainHash("xiigen-s04-source-selection-v2\n", sourceLiteral), source.SourceSelectionSha256);
        var checkpoint = Require(CodeDocumentationOneLessonCheckpointSelection.Create(
            new(1, "bug", "learning-unit", "tiny-model", "v1", Hex('1'), Hex('2'), 7), new("candidate:1", Hex('3'))));
        var parentLiteral = "{\"DesignPatternSlug\":\"learning-unit\",\"DomainSlug\":\"bug\",\"ManifestSha256\":\"" + Hex('1') +
            "\",\"ModelIdSlug\":\"tiny-model\",\"ModelVersion\":\"v1\",\"Step\":1,\"WeightsByteLength\":7,\"WeightsSha256\":\"" + Hex('2') + "\"}";
        var candidateLiteral = "{\"CandidateId\":\"candidate:1\",\"CandidateSha256\":\"" + Hex('3') + "\"}";
        Assert.Equal(DomainHash("xiigen-s04-parentcheckpoint-v2\n", parentLiteral), checkpoint.ParentCheckpointSha256);
        Assert.Equal(DomainHash("xiigen-s04-candidateselection-v2\n", candidateLiteral), checkpoint.CandidateSelectionSha256);
        var compact = Grant(Require(CodeDocumentationOneLessonCorpusSelection.Create(Shape(), [Id('1')]))).DescriptorsV2!
            with { CaseCount = 200, RepeatCount = 5000 };
        var bytes = CodeDocumentationOneLessonV2Wire.CanonicalBytes(JsonSerializer.SerializeToElement(compact));
        Assert.Equal(697, bytes.Length);
        using var descriptor = JsonDocument.Parse(bytes);
        Assert.Equal(compact, CodeDocumentationOneLessonV2Wire.ReadDescriptors(descriptor.RootElement));
    }

    // Independent literal JCS: these validated token/hash/slug grammars need no escaping.
    private static string Oracle(CodeDocumentationOneLessonCorpusV2 c, IEnumerable<string> ids)
        => "{\"CaseIds\":[" + string.Join(",", ids.Select(id => "\"" + id + "\"")) + "],\"Corpus\":{" +
            "\"CorpusId\":\"" + c.CorpusId + "\",\"CorpusSha256\":\"" + c.CorpusSha256 +
            "\",\"CorpusVersion\":\"" + c.CorpusVersion + "\",\"DomainSlug\":\"" + c.DomainSlug +
            "\",\"ManifestFileByteLength\":" + c.ManifestFileByteLength.ToString(CultureInfo.InvariantCulture) +
            ",\"ManifestFileSha256\":\"" + c.ManifestFileSha256 + "\",\"PayloadByteLength\":" +
            c.PayloadByteLength.ToString(CultureInfo.InvariantCulture) + ",\"PayloadSha256\":\"" + c.PayloadSha256 +
            "\",\"Purpose\":\"" + c.Purpose + "\",\"SubjectSlug\":\"" + c.SubjectSlug + "\"}}";
    private static void AssertLiteral(CodeDocumentationOneLessonCorpusSelection s)
    {
        var expected = Oracle(s.Corpus, s.CaseIds);
        Assert.Equal(Encoding.UTF8.GetBytes(expected), CodeDocumentationOneLessonV2Wire.CanonicalCorpusSelectionBytes(s));
        Assert.Equal(DomainHash("xiigen-s04-corpusdescriptor-v2\n", expected), s.CorpusDescriptorSha256);
    }
    private static CodeDocumentationOneLessonCorpusV2 Change(CodeDocumentationOneLessonCorpusV2 c, string field)
        => field switch
        {
            "id" => c with { CorpusId = "other-catalog-id" },
            "domain" => c with { DomainSlug = "other-domain" },
            "subject" => c with { SubjectSlug = "other-subject" },
            "version" => c with { CorpusVersion = "v2" },
            "manifest-hash" => c with { ManifestFileSha256 = Hex('a') },
            "corpus-hash" => c with { CorpusSha256 = Hex('b') },
            "manifest-length" => c with { ManifestFileByteLength = c.ManifestFileByteLength + 1 },
            "payload-hash" => c with { PayloadSha256 = Hex('c') },
            "payload-length" => c with { PayloadByteLength = c.PayloadByteLength + 1 },
            "case-id" => c,
            _ => throw new InvalidOperationException(field),
        };
    private static CodeDocumentationOneLessonCorpusV2 Shape()
        => new("opaque:corpus-1", "ClosedEvaluation", "translation", "en-ru", "v1", Hex('1'), Hex('2'), 1, Hex('3'), 1);
    private static CodeDocumentationSavedModelAssessmentGenerationSettings Settings(int max = 200) => new(1, 0, 0, 1, 0, max, 1);
    private static string Hex(char c) => new(c, 64);
    private static string Id(char c) => "sha256:" + Hex(c);
    private static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static string DomainHash(string domain, string literal) => Sha(Encoding.UTF8.GetBytes(domain + literal));
    private static CodeDocumentationOneLessonSelectorV1 Selector() => new("objective", "selector");
    private static CodeDocumentationOneLessonGrantV1 Grant(CodeDocumentationOneLessonCorpusSelection s)
        => new("grant", "v2", "selector", "objective", "corpus-selection-run", Hex('1'), Hex('2'), Hex('3'), Hex('4'), Hex('5'))
        { FormatVersion = 2, CorpusSelection = s, DescriptorsV2 = new(2, Hex('1'), Hex('2'), Hex('3'),
            s.CorpusDescriptorSha256, Hex('5'), s.CaseIds.Count, 2, Hex('6'), Hex('7')) };
    private static CodeDocumentationOneLessonGrantCatalog Catalog(CodeDocumentationOneLessonGrantV1 g)
        => CodeDocumentationOneLessonGrantCatalog.FromTrustedGrants([g]);
    private static T Require<T>(OperationResult<T> result)
    {
        Assert.True(result.IsSuccess, result.ErrorCode + ": " + result.ErrorMessage);
        Assert.NotNull(result.Value); return result.Value!;
    }
    private void LogRuntimeIdentities()
    {
        var test = typeof(CodeDocumentationOneLessonCorpusSelectionTests).Assembly;
        foreach (var identity in new[] {
            (Kind: "production-dll", Path: typeof(RepositoryOwnedReferenceCorpusManifestSealer).Assembly.Location),
            (Kind: "test-dll", Path: test.Location),
            (Kind: "runtimeconfig", Path: Path.ChangeExtension(test.Location, ".runtimeconfig.json")),
            (Kind: "deps", Path: Path.ChangeExtension(test.Location, ".deps.json")) })
        {
            var bytes = File.ReadAllBytes(identity.Path);
            output.WriteLine("CORPUS_SELECTION_RUNTIME " + JsonSerializer.Serialize(new {
                kind = identity.Kind, path = identity.Path, sha256 = Sha(bytes), byteLength = bytes.LongLength }));
        }
    }
    private static void AssertNoExecutionArtifacts(Fixture f)
    {
        Assert.DoesNotContain(Directory.GetFiles(f.Repository, "*", SearchOption.AllDirectories), p =>
            p.EndsWith("session.jsonl", StringComparison.Ordinal) || p.EndsWith(".safetensors", StringComparison.Ordinal) ||
            p.Contains("baseline", StringComparison.Ordinal) || p.Contains(Path.DirectorySeparatorChar + "attempts" + Path.DirectorySeparatorChar, StringComparison.Ordinal));
        Assert.False(Directory.Exists(Path.Combine(f.Repository, "training-artifacts", "models")));
    }
    private sealed class Fixture : IDisposable
    {
        private string Root { get; } = Path.Combine(Path.GetTempPath(), "corpus-selection-" + Guid.NewGuid().ToString("N"));
        internal string Repository { get; }
        internal RepositoryOwnedReferenceCorpusAddress Address { get; }
        internal string PayloadPath => Path.Combine(Address.CorpusRoot, RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName);
        internal FrozenReferenceCorpus Corpus { get; }
        internal CodeDocumentationOneLessonCorpusV2 Descriptor { get; }
        internal string[] HeldOutIds => Corpus.Entries.Where(e => e.Partition == FrozenReferencePartition.HeldOut)
            .Select(e => e.EntryId).OrderBy(x => x, StringComparer.Ordinal).ToArray();
        internal Fixture(int heldOutCount)
        {
            Repository = Path.Combine(Root, "repo"); Directory.CreateDirectory(Repository);
            foreach (var name in new[] { "LlmFromScratch.sln", "llm_mvp_core.sln", "AGENTS.md" })
                File.WriteAllText(Path.Combine(Repository, name), "synthetic fixture sentinel");
            Directory.CreateDirectory(Path.Combine(Repository, "14.ContinuousLearning"));
            Directory.CreateDirectory(Path.Combine(Repository, "11.DPO"));
            var entries = Enumerable.Range(0, heldOutCount).Select(i => Entry("heldout-" + i, FrozenReferencePartition.HeldOut))
                .Prepend(Entry("train", FrozenReferencePartition.Train));
            Corpus = Require(FrozenReferenceCorpus.Build("human-corpus-name", "v1", new(2026, 10, 3, 12, 0, 0, TimeSpan.Zero), entries));
            Address = Require(RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(Repository, "translation", "en-ru", "v1"));
            var manifest = Require(RepositoryOwnedReferenceCorpusManifestSealer.PersistAndSeal(Address, Corpus));
            var bytes = File.ReadAllBytes(Address.ManifestPath);
            Assert.StartsWith("sha256:", manifest.CorpusSha256);
            Assert.True(FrozenReferenceCanonicalization.IsCanonicalSha256Ref(manifest.CorpusSha256));
            Descriptor = new("opaque:catalog-key", "ClosedEvaluation", manifest.Domain, manifest.Subject, manifest.CorpusVersion,
                Sha(bytes), manifest.CorpusSha256[7..], bytes.LongLength, manifest.PayloadSha256, manifest.PayloadByteLength);
        }
        internal CodeDocumentationOneLessonCorpusSelection Selection()
            => Require(CodeDocumentationOneLessonCorpusSelection.Create(Descriptor, HeldOutIds));
        private static FrozenReferenceEntry Entry(string seed, FrozenReferencePartition partition)
        {
            var answer = "synthetic closed reference " + seed;
            var provenance = Require(PaidReferenceProvenance.Create("synthetic-paid-provider", "synthetic-paid-model", "v1",
                "request-" + seed, new(2026, 10, 3, 10, 0, 0, TimeSpan.Zero), FrozenReferenceEntry.ComputeNormalizedTextSha256(answer)));
            return Require(FrozenReferenceEntry.Create("synthetic owner input " + seed, answer, provenance, "translation", "en-ru",
                partition, FrozenReferenceEntry.ComputeNormalizedTextSha256("independent-source-entity:" + seed)));
        }
        public void Dispose()
        {
            // Delete only this unique owned synthetic fixture after resolved-path proof.
            var target = Path.GetFullPath(Root);
            Assert.StartsWith(Path.GetFullPath(Path.GetTempPath()) + "corpus-selection-", target);
            if (Directory.Exists(target)) Directory.Delete(target, recursive: true);
        }
    }
}
