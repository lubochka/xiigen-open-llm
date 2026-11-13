// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DpoPairSideAuthorKind = DPO.Models.DpoPairSideAuthorKind;

namespace LlmFromScratch.Tests;

public sealed class CodeDocumentationOneLessonCheckpointSelectionTests(
    Xunit.Abstractions.ITestOutputHelper output)
{
    [Fact]
    public void FreshSelection_ProvesRealOwnerTupleLiteralCommitmentsAndReadbackId()
    {
        using var fixture = new Fixture();
        var selection = Selection(fixture.First.Manifest);
        Assert.Equal(ParentOracle(selection.Parent), Encoding.UTF8.GetString(
            CodeDocumentationOneLessonV2Wire.CanonicalBytes(JsonSerializer.SerializeToElement(selection.Parent))));
        Assert.Equal(CandidateOracle(selection.Candidate), Encoding.UTF8.GetString(
            CodeDocumentationOneLessonV2Wire.CanonicalBytes(JsonSerializer.SerializeToElement(selection.Candidate))));
        Assert.Equal(DomainHash("xiigen-s04-parentcheckpoint-v2\n", ParentOracle(selection.Parent)),
            selection.ParentCheckpointSha256);
        Assert.Equal(DomainHash("xiigen-s04-candidateselection-v2\n", CandidateOracle(selection.Candidate)),
            selection.CandidateSelectionSha256);
        var fresh = Require(selection.OpenFresh(fixture.Repository, fixture.Runtime));
        Assert.Equal(selection.Parent, fresh.Selection.Parent);
        Assert.Equal(selection.Candidate, fresh.Selection.Candidate);
        Assert.Equal(selection.ParentCheckpointSha256, fresh.Selection.ParentCheckpointSha256);
        Assert.Equal(selection.CandidateSelectionSha256, fresh.Selection.CandidateSelectionSha256);
        Assert.Equal(DomainHash("xiigen-s04-checkpoint-id-v2\n", ParentOracle(selection.Parent)), fresh.CheckpointId);
        Assert.NotEqual(selection.ParentCheckpointSha256, fresh.CheckpointId);
        Assert.Null(typeof(CodeDocumentationOneLessonParentCheckpointV2).GetProperty("CheckpointId"));
        Assert.Null(typeof(CodeDocumentationOneLessonCheckpointSelection).GetProperty("CheckpointId",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
        Assert.DoesNotContain(typeof(CodeDocumentationOneLessonCheckpointSelectionOpenResult).GetProperties(),
            p => typeof(Stream).IsAssignableFrom(p.PropertyType));
        AssertExclusiveWeightReopen(fixture.First);
        AssertNoExecutionArtifacts(fixture);
        LogRuntimeIdentities();
    }

    [Theory]
    [InlineData("step")]
    [InlineData("domain")]
    [InlineData("pattern")]
    [InlineData("model")]
    [InlineData("version")]
    [InlineData("manifest")]
    [InlineData("weights")]
    [InlineData("length")]
    [InlineData("candidate-id")]
    [InlineData("candidate-sha")]
    public void FullCommitments_CoverEveryPrimitiveAndExactIntegerBytes(string field)
    {
        var parent = ShapeParent(); var candidate = ShapeCandidate();
        var original = Require(CodeDocumentationOneLessonCheckpointSelection.Create(parent, candidate));
        var changedParent = field switch
        {
            "step" => parent with { Step = int.MaxValue },
            "domain" => parent with { DomainSlug = "csharp" },
            "pattern" => parent with { DesignPatternSlug = "other-pattern" },
            "model" => parent with { ModelIdSlug = "other-model" },
            "version" => parent with { ModelVersion = "v2" },
            "manifest" => parent with { ManifestSha256 = Hex('a') },
            "weights" => parent with { WeightsSha256 = Hex('b') },
            "length" => parent with { WeightsByteLength = RepositoryOwnedModelCheckpointManifestSealer.MaximumWeightBytes },
            _ => parent,
        };
        var changedCandidate = field switch
        {
            "candidate-id" => candidate with { CandidateId = new('A', 128) },
            "candidate-sha" => candidate with { CandidateSha256 = Hex('c') },
            _ => candidate,
        };
        var changed = Require(CodeDocumentationOneLessonCheckpointSelection.Create(changedParent, changedCandidate));
        Assert.Equal(DomainHash("xiigen-s04-parentcheckpoint-v2\n", ParentOracle(changedParent)), changed.ParentCheckpointSha256);
        Assert.Equal(DomainHash("xiigen-s04-candidateselection-v2\n", CandidateOracle(changedCandidate)), changed.CandidateSelectionSha256);
        if (field.StartsWith("candidate", StringComparison.Ordinal))
        {
            Assert.NotEqual(original.CandidateSelectionSha256, changed.CandidateSelectionSha256);
            Assert.Equal(original.ParentCheckpointSha256, changed.ParentCheckpointSha256);
        }
        else
        {
            Assert.NotEqual(original.ParentCheckpointSha256, changed.ParentCheckpointSha256);
            Assert.Equal(original.CandidateSelectionSha256, changed.CandidateSelectionSha256);
        }
    }

    [Theory]
    [InlineData("null-parent")]
    [InlineData("null-candidate")]
    [InlineData("step-zero")]
    [InlineData("step-negative")]
    [InlineData("domain-null")]
    [InlineData("domain-dot")]
    [InlineData("pattern-upper")]
    [InlineData("model-root")]
    [InlineData("version-97")]
    [InlineData("manifest-upper")]
    [InlineData("manifest-null")]
    [InlineData("weights-short")]
    [InlineData("weights-null")]
    [InlineData("length-zero")]
    [InlineData("length-negative")]
    [InlineData("length-over")]
    [InlineData("candidate-null")]
    [InlineData("candidate-empty")]
    [InlineData("candidate-leading")]
    [InlineData("candidate-space")]
    [InlineData("candidate-unicode")]
    [InlineData("candidate-129")]
    [InlineData("candidate-hash-upper")]
    [InlineData("candidate-hash-short")]
    [InlineData("candidate-hash-null")]
    public void PureCreate_RejectsInvalidPrimitiveThenRestoresControl(string fault)
    {
        CodeDocumentationOneLessonParentCheckpointV2? parent = ShapeParent();
        CodeDocumentationOneLessonCandidateSelectionV2? candidate = ShapeCandidate();
        Assert.True(CodeDocumentationOneLessonCheckpointSelection.Create(parent, candidate).IsSuccess);
        parent = fault switch
        {
            "null-parent" => null,
            "step-zero" => parent with { Step = 0 },
            "step-negative" => parent with { Step = -1 },
            "domain-null" => parent with { DomainSlug = null! },
            "domain-dot" => parent with { DomainSlug = "bug.test" },
            "pattern-upper" => parent with { DesignPatternSlug = "Pattern" },
            "model-root" => parent with { ModelIdSlug = "/model" },
            "version-97" => parent with { ModelVersion = new('a', 97) },
            "manifest-upper" => parent with { ManifestSha256 = new('A', 64) },
            "manifest-null" => parent with { ManifestSha256 = null! },
            "weights-short" => parent with { WeightsSha256 = "a" },
            "weights-null" => parent with { WeightsSha256 = null! },
            "length-zero" => parent with { WeightsByteLength = 0 },
            "length-negative" => parent with { WeightsByteLength = -1 },
            "length-over" => parent with { WeightsByteLength = RepositoryOwnedModelCheckpointManifestSealer.MaximumWeightBytes + 1 },
            _ => parent,
        };
        candidate = fault switch
        {
            "null-candidate" => null,
            "candidate-null" => candidate with { CandidateId = null! },
            "candidate-empty" => candidate with { CandidateId = "" },
            "candidate-leading" => candidate with { CandidateId = "-candidate" },
            "candidate-space" => candidate with { CandidateId = "candidate " },
            "candidate-unicode" => candidate with { CandidateId = "cändidate" },
            "candidate-129" => candidate with { CandidateId = new('a', 129) },
            "candidate-hash-upper" => candidate with { CandidateSha256 = new('A', 64) },
            "candidate-hash-short" => candidate with { CandidateSha256 = "a" },
            "candidate-hash-null" => candidate with { CandidateSha256 = null! },
            _ => candidate,
        };
        Assert.Equal(CodeDocumentationOneLessonCheckpointSelection.InvalidErrorCode,
            CodeDocumentationOneLessonCheckpointSelection.Create(parent, candidate).ErrorCode);
        Assert.True(CodeDocumentationOneLessonCheckpointSelection.Create(ShapeParent(), ShapeCandidate()).IsSuccess);
    }

    [Fact]
    public void PureCreate_UsesExistingSlugCeilingTokenGrammarAndScalarWriter()
    {
        var parent = ShapeParent() with { DomainSlug = new('a', 96), DesignPatternSlug = new('b', 96),
            ModelIdSlug = new('c', 96), ModelVersion = new('d', 96), Step = int.MaxValue,
            WeightsByteLength = RepositoryOwnedModelCheckpointManifestSealer.MaximumWeightBytes };
        var candidate = ShapeCandidate() with { CandidateId = "A0._:-" + new string('z', 122) };
        var value = Require(CodeDocumentationOneLessonCheckpointSelection.Create(parent, candidate));
        Assert.Equal(DomainHash("xiigen-s04-parentcheckpoint-v2\n", ParentOracle(parent)), value.ParentCheckpointSha256);
        Assert.Equal(DomainHash("xiigen-s04-candidateselection-v2\n", CandidateOracle(candidate)), value.CandidateSelectionSha256);
        using var array = JsonDocument.Parse("{\"x\":[]}");
        Assert.Throws<JsonException>(() => CodeDocumentationOneLessonV2Wire.CanonicalBytes(array.RootElement));
        using var number = JsonDocument.Parse("{\"x\":1.5}");
        Assert.Throws<JsonException>(() => CodeDocumentationOneLessonV2Wire.CanonicalBytes(number.RootElement));
    }

    [Theory]
    [InlineData("step")]
    [InlineData("domain")]
    [InlineData("pattern")]
    [InlineData("model")]
    [InlineData("version")]
    [InlineData("manifest")]
    [InlineData("weights")]
    [InlineData("length")]
    public void FreshSelection_RejectsEachChangedPinAndDisposesThenRestoresControl(string fault)
    {
        using var fixture = new Fixture();
        var valid = Selection(fixture.First.Manifest);
        Assert.True(valid.OpenFresh(fixture.Repository, fixture.Runtime).IsSuccess);
        var parent = valid.Parent;
        var changed = parent with { Step = fault == "step" ? 2 : parent.Step,
            DomainSlug = fault == "domain" ? "other" : parent.DomainSlug,
            DesignPatternSlug = fault == "pattern" ? "other" : parent.DesignPatternSlug,
            ModelIdSlug = fault == "model" ? "other" : parent.ModelIdSlug,
            ModelVersion = fault == "version" ? "v2" : parent.ModelVersion,
            ManifestSha256 = fault == "manifest" ? Hex('b') : parent.ManifestSha256,
            WeightsSha256 = fault == "weights" ? Hex('c') : parent.WeightsSha256,
            WeightsByteLength = fault == "length" ? parent.WeightsByteLength + 1 : parent.WeightsByteLength };
        var invalid = Require(CodeDocumentationOneLessonCheckpointSelection.Create(changed, valid.Candidate));
        Assert.False(invalid.OpenFresh(fixture.Repository, fixture.Runtime).IsSuccess);
        AssertExclusiveWeightReopen(fixture.First);
        Assert.True(valid.OpenFresh(fixture.Repository, fixture.Runtime).IsSuccess);
        AssertNoExecutionArtifacts(fixture);
    }

    [Theory]
    [InlineData("manifest")]
    [InlineData("weights-same-length")]
    [InlineData("weights-missing")]
    [InlineData("dpo-manifest")]
    [InlineData("dpo-missing")]
    [InlineData("dpo-pair")]
    public void FreshSelection_RejectsActualOwnerCorruptionAndRestoresBytes(string fault)
    {
        using var fixture = new Fixture();
        var valid = Selection(fixture.First.Manifest);
        Assert.True(valid.OpenFresh(fixture.Repository, fixture.Runtime).IsSuccess);
        var path = fault switch
        {
            "manifest" => fixture.First.Address.ManifestPath,
            "weights-same-length" or "weights-missing" => fixture.First.Address.WeightsPath,
            "dpo-pair" => Path.Combine(fixture.Repository, fixture.DpoManifest.OrderedPairs[0].RepositoryRelativePairFilePath),
            _ => fixture.DpoAddress.ManifestPath,
        };
        var bytes = File.ReadAllBytes(path);
        try
        {
            if (fault.EndsWith("missing", StringComparison.Ordinal)) File.Delete(path);
            else { var corrupt = bytes.ToArray(); corrupt[0] ^= 0x01; File.WriteAllBytes(path, corrupt); Assert.Equal(bytes.Length, new FileInfo(path).Length); }
            Assert.Equal(RepositoryOwnedModelCheckpointLoader.CorruptCandidateErrorCode,
                valid.OpenFresh(fixture.Repository, fixture.Runtime).ErrorCode);
            AssertNoExecutionArtifacts(fixture);
        }
        finally { File.WriteAllBytes(path, bytes); }
        Assert.True(valid.OpenFresh(fixture.Repository, fixture.Runtime).IsSuccess);
        AssertExclusiveWeightReopen(fixture.First);
    }

    [Fact]
    public void NewerCompatibleSelection_CannotFallBackToPinnedOlderStep()
    {
        using var fixture = new Fixture();
        var pinned = Selection(fixture.First.Manifest);
        Assert.True(pinned.OpenFresh(fixture.Repository, fixture.Runtime).IsSuccess);
        var newer = fixture.Seal(2, fixture.First.Manifest);
        Assert.Equal(CodeDocumentationOneLessonCheckpointSelection.InvalidErrorCode,
            pinned.OpenFresh(fixture.Repository, fixture.Runtime).ErrorCode);
        AssertExclusiveWeightReopen(newer);
        var fresh = Require(Selection(newer.Manifest).OpenFresh(fixture.Repository, fixture.Runtime));
        Assert.Equal(2, fresh.Selection.Parent.Step);
        Assert.Equal(DomainHash("xiigen-s04-checkpoint-id-v2\n", ParentOracle(fresh.Selection.Parent)), fresh.CheckpointId);
        fixture.RemoveCheckpoint(newer);
        Assert.True(pinned.OpenFresh(fixture.Repository, fixture.Runtime).IsSuccess);
        AssertNoExecutionArtifacts(fixture);
    }

    [Fact]
    public void ValidHigherIncompatible_IsSkippedButItsCorruptionStillFailsWholeDiscovery()
    {
        using var fixture = new Fixture();
        var pinned = Selection(fixture.First.Manifest);
        var higher = fixture.Seal(2, fixture.First.Manifest, fixture.Runtime with { TrainerSha256 = Hex('9') });
        Assert.True(pinned.OpenFresh(fixture.Repository, fixture.Runtime).IsSuccess);
        var bytes = File.ReadAllBytes(higher.Address.WeightsPath);
        try
        {
            var corrupt = bytes.ToArray(); corrupt[0] ^= 0x01; File.WriteAllBytes(higher.Address.WeightsPath, corrupt);
            Assert.Equal(RepositoryOwnedModelCheckpointLoader.CorruptCandidateErrorCode,
                pinned.OpenFresh(fixture.Repository, fixture.Runtime).ErrorCode);
        }
        finally { File.WriteAllBytes(higher.Address.WeightsPath, bytes); }
        Assert.True(pinned.OpenFresh(fixture.Repository, fixture.Runtime).IsSuccess);
        AssertExclusiveWeightReopen(fixture.First);
        AssertNoExecutionArtifacts(fixture);
    }

    [Fact]
    public void CorruptAnyLowerNumericCandidate_FailsWholeDiscoveryWithValidLatest()
    {
        using var fixture = new Fixture();
        var latest = fixture.Seal(3, fixture.First.Manifest);
        var valid = Selection(latest.Manifest);
        Assert.True(valid.OpenFresh(fixture.Repository, fixture.Runtime).IsSuccess);
        var bad = Require(RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(
            fixture.Repository, "bug", "learning-unit", "tiny-model", "v1", 2));
        Directory.CreateDirectory(bad.CheckpointRoot);
        File.WriteAllText(bad.ManifestPath, "{}");
        Assert.Equal(RepositoryOwnedModelCheckpointLoader.CorruptCandidateErrorCode,
            valid.OpenFresh(fixture.Repository, fixture.Runtime).ErrorCode);
        fixture.RemoveCheckpoint(new(bad, latest.Manifest));
        Assert.True(valid.OpenFresh(fixture.Repository, fixture.Runtime).IsSuccess);
        AssertNoExecutionArtifacts(fixture);
    }

    [Fact]
    public void ServerCompatibility_IsCurrentOwnerInputAndCandidateRemainsIdentityOnly()
    {
        using var fixture = new Fixture();
        var valid = Selection(fixture.First.Manifest);
        Assert.Equal(RepositoryOwnedModelCheckpointLoader.IncompatibleCandidateErrorCode,
            valid.OpenFresh(fixture.Repository, fixture.Runtime with { ConfigSha256 = Hex('9') }).ErrorCode);
        Assert.Equal(RepositoryOwnedModelCheckpointLoader.InvalidRequestErrorCode,
            valid.OpenFresh(fixture.Repository, fixture.Runtime with { ConfigSha256 = "invalid" }).ErrorCode);
        var candidateOnly = Require(CodeDocumentationOneLessonCheckpointSelection.Create(valid.Parent,
            new("another:server-candidate", Hex('f'))));
        Assert.True(candidateOnly.OpenFresh(fixture.Repository, fixture.Runtime).IsSuccess);
        Assert.True(valid.OpenFresh(fixture.Repository, fixture.Runtime).IsSuccess);
        AssertNoExecutionArtifacts(fixture);
    }

    [Fact]
    public void HeldDpoLineage_RemainsIntegrityValidWhileFreshSourceRejects()
    {
        using var fixture = new Fixture();
        var selection = Selection(fixture.First.Manifest);
        var bytes = File.ReadAllBytes(fixture.DpoAddress.ManifestPath);
        var source = new CodeDocumentationOneLessonSourceV2("source", "TrainingPairs", "bug", "learning-unit", "v1",
            Sha256(bytes), bytes.LongLength, fixture.DpoManifest.ManifestSha256);
        var sources = Require(CodeDocumentationOneLessonSourceSelection.Create([source],
            [new(fixture.DpoManifest.OrderedPairs[0].PairId, "source")]));
        Assert.True(sources.OpenFresh(fixture.Repository).IsSuccess);
        var store = new FileDpoPairStore(fixture.DpoAddress.PairsRoot);
        var held = Require(store.PrepareTransaction([fixture.Pair.Domain]));
        Assert.NotNull(held);
        try
        {
            Assert.Empty(Require(store.GetAll()));
            Assert.True(RepositoryOwnedDpoDatasetManifestSealer.OpenSealedForManifestIntegrity(fixture.DpoAddress).IsSuccess);
            Assert.True(selection.OpenFresh(fixture.Repository, fixture.Runtime).IsSuccess);
            Assert.False(sources.OpenFresh(fixture.Repository).IsSuccess);
        }
        finally { Assert.True(held.ReleaseIfUnmutated().IsSuccess); }
        Assert.True(selection.OpenFresh(fixture.Repository, fixture.Runtime).IsSuccess);
        Assert.True(sources.OpenFresh(fixture.Repository).IsSuccess);
        AssertNoExecutionArtifacts(fixture);
    }

    [Theory]
    [InlineData("parent-compact")]
    [InlineData("candidate-compact")]
    [InlineData("old-candidate")]
    [InlineData("old-manifest")]
    [InlineData("old-weights")]
    [InlineData("v1-attached")]
    public void Catalog_RejectsEachMismatchAndRestoresAcceptedControl(string fault)
    {
        var selection = Require(CodeDocumentationOneLessonCheckpointSelection.Create(ShapeParent(), ShapeCandidate()));
        var valid = Grant(selection);
        Assert.True(CodeDocumentationOneLessonGrantCatalog.FromTrustedGrants([valid]).Resolve(Selector()).IsSuccess);
        var bad = fault switch
        {
            "parent-compact" => valid with { DescriptorsV2 = valid.DescriptorsV2! with { ParentCheckpointSha256 = Hex('a') } },
            "candidate-compact" => valid with { DescriptorsV2 = valid.DescriptorsV2! with { CandidateSelectionSha256 = Hex('b') } },
            "old-candidate" => valid with { CandidateSha256 = Hex('c') },
            "old-manifest" => valid with { ParentManifestSha256 = Hex('d') },
            "old-weights" => valid with { ParentWeightsSha256 = Hex('e') },
            _ => valid with { FormatVersion = 1, DescriptorsV2 = null },
        };
        Assert.Equal(CodeDocumentationOneLessonGrantCatalog.InvalidCatalogErrorCode,
            CodeDocumentationOneLessonGrantCatalog.FromTrustedGrants([bad]).Resolve(Selector()).ErrorCode);
        Assert.True(CodeDocumentationOneLessonGrantCatalog.FromTrustedGrants([valid]).Resolve(Selector()).IsSuccess);
    }

    [Fact]
    public void Catalog_CopiesInputsPreservesSourceSelectionAndPureReplayAndOldWire()
    {
        var parent = ShapeParent(); var candidate = ShapeCandidate();
        var selection = Require(CodeDocumentationOneLessonCheckpointSelection.Create(parent, candidate));
        Assert.NotSame(parent, selection.Parent); Assert.NotSame(candidate, selection.Candidate);
        var source = Require(CodeDocumentationOneLessonSourceSelection.Create(
            [new("source", "TrainingPairs", "bug", "learning-unit", "v1", Hex('a'), 1, Hex('b'))],
            [new(Hex('c'), "source")]));
        var input = new[] { Grant(selection) with { SourceSelection = source,
            DescriptorsV2 = Grant(selection).DescriptorsV2! with { SourceSelectionSha256 = source.SourceSelectionSha256 } } };
        var catalog = CodeDocumentationOneLessonGrantCatalog.FromTrustedGrants(input);
        var resolved = Require(catalog.Resolve(Selector()));
        parent = parent with { Step = 2 }; candidate = candidate with { CandidateId = "mutated" };
        input[0] = input[0] with { CheckpointSelection = Require(CodeDocumentationOneLessonCheckpointSelection.Create(parent, candidate)) };
        var replay = Require(catalog.Revalidate(resolved.GrantId, resolved.GrantVersion));
        Assert.Same(resolved, replay);
        Assert.NotSame(selection, resolved.CheckpointSelection);
        Assert.NotSame(selection.Parent, resolved.CheckpointSelection!.Parent);
        Assert.Equal(selection.Parent, replay.CheckpointSelection!.Parent);
        Assert.Equal(selection.Candidate, replay.CheckpointSelection.Candidate);
        Assert.Equal(source.SourceSelectionSha256, replay.SourceSelection!.SourceSelectionSha256);
        Assert.NotSame(source, replay.SourceSelection);
        using var wire = JsonDocument.Parse(JsonSerializer.Serialize(resolved));
        Assert.Equal(new[] { "CandidateSha256", "ConfigurationSha256", "ExamConditionSha256", "GrantId", "GrantSelector", "GrantVersion",
            "ObjectiveId", "ParentManifestSha256", "ParentWeightsSha256", "TrainingRunId" },
            wire.RootElement.EnumerateObject().Select(p => p.Name).OrderBy(n => n, StringComparer.Ordinal));
        var compact = resolved with { CheckpointSelection = null, SourceSelection = null };
        Assert.True(CodeDocumentationOneLessonGrantCatalog.FromTrustedGrants([compact]).Resolve(Selector()).IsSuccess);
        var v1 = compact with { FormatVersion = 1, DescriptorsV2 = null };
        var json = "{\"SchemaVersion\":1,\"Grants\":[" + JsonSerializer.Serialize(v1) + "]}";
        Assert.True(CodeDocumentationOneLessonGrantCatalog.Load(_ => json).Resolve(Selector()).IsSuccess);
    }

    // Independent literal JCS oracles: valid tuple grammars exclude escaping characters.
    private static string ParentOracle(CodeDocumentationOneLessonParentCheckpointV2 p)
        => "{\"DesignPatternSlug\":\"" + p.DesignPatternSlug + "\",\"DomainSlug\":\"" + p.DomainSlug +
           "\",\"ManifestSha256\":\"" + p.ManifestSha256 + "\",\"ModelIdSlug\":\"" + p.ModelIdSlug +
           "\",\"ModelVersion\":\"" + p.ModelVersion + "\",\"Step\":" + p.Step.ToString(CultureInfo.InvariantCulture) +
           ",\"WeightsByteLength\":" + p.WeightsByteLength.ToString(CultureInfo.InvariantCulture) +
           ",\"WeightsSha256\":\"" + p.WeightsSha256 + "\"}";
    private static string CandidateOracle(CodeDocumentationOneLessonCandidateSelectionV2 c)
        => "{\"CandidateId\":\"" + c.CandidateId + "\",\"CandidateSha256\":\"" + c.CandidateSha256 + "\"}";
    private static string DomainHash(string prefix, string oracle) => Sha256(Encoding.UTF8.GetBytes(prefix + oracle));
    private static string Hex(char value) => new(value, 64);
    private static string Sha256(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static CodeDocumentationOneLessonParentCheckpointV2 ShapeParent() => new(1, "bug", "learning-unit", "tiny-model", "v1", Hex('1'), Hex('2'), 7);
    private static CodeDocumentationOneLessonCandidateSelectionV2 ShapeCandidate() => new("candidate:1", Hex('3'));
    private static CodeDocumentationOneLessonParentCheckpointV2 Parent(RepositoryOwnedModelCheckpointManifest m)
        => new(m.Step, m.Domain, m.DesignPattern, m.ModelId, m.Version, m.ManifestSha256, m.WeightsSha256, m.WeightsByteLength);
    private static CodeDocumentationOneLessonCheckpointSelection Selection(RepositoryOwnedModelCheckpointManifest m)
        => Require(CodeDocumentationOneLessonCheckpointSelection.Create(Parent(m), ShapeCandidate()));
    private static CodeDocumentationOneLessonSelectorV1 Selector() => new("objective", "selector");
    private static CodeDocumentationOneLessonGrantV1 Grant(CodeDocumentationOneLessonCheckpointSelection s)
        => new("grant", "v2", "selector", "objective", "checkpoint-selection-run", s.Candidate.CandidateSha256,
            s.Parent.ManifestSha256, s.Parent.WeightsSha256, Hex('4'), Hex('5'))
        { FormatVersion = 2, CheckpointSelection = s, DescriptorsV2 = new(2, Hex('1'), s.ParentCheckpointSha256,
            s.CandidateSelectionSha256, Hex('4'), Hex('5'), 1, 2, Hex('6'), Hex('7')) };
    private static T Require<T>(OperationResult<T> result)
    {
        Assert.True(result.IsSuccess, result.ErrorCode + ": " + result.ErrorMessage);
        Assert.NotNull(result.Value); return result.Value!;
    }
    private static void AssertExclusiveWeightReopen(Checkpoint checkpoint)
    {
        // Same-process check on this host; not a retained lease or cross-platform proof.
        using var stream = new FileStream(checkpoint.Address.WeightsPath, FileMode.Open, FileAccess.Read, FileShare.None);
        Assert.Equal(checkpoint.Manifest.WeightsByteLength, stream.Length);
    }
    private static void AssertNoExecutionArtifacts(Fixture f)
        => Assert.DoesNotContain(Directory.GetFiles(f.Repository, "*", SearchOption.AllDirectories),
            p => p.EndsWith("session.jsonl", StringComparison.Ordinal) || p.Contains("baseline", StringComparison.Ordinal) ||
                 p.Contains(Path.DirectorySeparatorChar + "attempts" + Path.DirectorySeparatorChar, StringComparison.Ordinal));
    private void LogRuntimeIdentities()
    {
        var test = typeof(CodeDocumentationOneLessonCheckpointSelectionTests).Assembly;
        foreach (var identity in new[] {
            (Kind: "production-dll", Path: typeof(RepositoryOwnedModelCheckpointManifestSealer).Assembly.Location),
            (Kind: "test-dll", Path: test.Location),
            (Kind: "runtimeconfig", Path: Path.ChangeExtension(test.Location, ".runtimeconfig.json")),
            (Kind: "deps", Path: Path.ChangeExtension(test.Location, ".deps.json")) })
        {
            var bytes = File.ReadAllBytes(identity.Path);
            output.WriteLine("CHECKPOINT_SELECTION_RUNTIME " + JsonSerializer.Serialize(new {
                kind = identity.Kind, path = identity.Path, sha256 = Sha256(bytes), byteLength = bytes.LongLength }));
        }
    }

    private sealed record Checkpoint(RepositoryOwnedModelCheckpointAddress Address, RepositoryOwnedModelCheckpointManifest Manifest);
    private sealed class Fixture : IDisposable
    {
        private string Root { get; } = Path.Combine(Path.GetTempPath(), "checkpoint-selection-" + Guid.NewGuid().ToString("N"));
        internal string Repository { get; }
        internal RepositoryOwnedModelRuntimeCompatibility Runtime { get; } = new(Hex('b'), Hex('c'), Hex('d'), Hex('e'), Hex('f'), "tiny-checkpoint-v1");
        internal RepositoryOwnedDpoDatasetAddress DpoAddress { get; }
        internal RepositoryOwnedDpoDatasetManifest DpoManifest { get; }
        internal DpoPair Pair { get; }
        internal Checkpoint First { get; }
        internal Fixture()
        {
            Repository = Path.Combine(Root, "repo"); Directory.CreateDirectory(Repository);
            foreach (var name in new[] { "LlmFromScratch.sln", "llm_mvp_core.sln", "AGENTS.md" })
                File.WriteAllText(Path.Combine(Repository, name), "synthetic fixture sentinel");
            Directory.CreateDirectory(Path.Combine(Repository, "14.ContinuousLearning"));
            Directory.CreateDirectory(Path.Combine(Repository, "11.DPO"));
            var identity = Require(RepositoryOwnedDpoDatasetManifestSealer.CreateRequestIdentity(
                [Sha256(Encoding.UTF8.GetBytes("synthetic-paid-reference"))],
                new(OpenWeightTeacherFamily.Qwen.ToString(), "qwen3.5:9b", Hex('a'),
                    LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind, Hex('1')),
                new("synthetic-generation", "v1", Hex('2')), new("synthetic-creator", "v1", Hex('3'))));
            var passport = new TrainingTeacherProvenance(TrainingTeacherSourceKind.ExternalTeacherOutput,
                OpenWeightTeacherFamily.Qwen, TrainingTeacherAccessMode.LocalHostedOpenWeight,
                TrainingTeacherHostingProvider.Ollama, "qwen3.5:9b", Hex('a'), TrainingTeacherModelLicense.Apache20,
                ProviderTermsChecked: false, ProviderTermsAllowTraining: false, ClosedProprietaryModel: false,
                TeacherOutputUsedForTraining: true, Sha256(Encoding.UTF8.GetBytes("synthetic-request")), InvocationCount: 1)
            { ModelArtifactDigestKind = LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind, ModelArtifactDigest = Hex('1') };
            Pair = Require(DpoPairTrainingContentAuthority.Bind(new DpoPair("tiny prompt", "chosen", "rejected", "bug", DateTimeOffset.UnixEpoch)
            { TeacherProvenance = passport, GovernanceDecision = new(true, false, ["sha256:" + Hex('a')]),
                ChosenAuthorship = new(DpoPairSideAuthorKind.OpenTeacher, passport), RejectedAuthorship = new(DpoPairSideAuthorKind.LocalModel) }));
            DpoAddress = Require(RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(Repository, "bug", "learning-unit", "v1"));
            DpoManifest = Require(RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(DpoAddress, identity, [Pair]));
            First = Seal(1, null);
        }
        internal Checkpoint Seal(int step, RepositoryOwnedModelCheckpointManifest? parent, RepositoryOwnedModelRuntimeCompatibility? runtime = null)
        {
            var address = Require(RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(
                Repository, "bug", "learning-unit", "tiny-model", "v1", step));
            var request = new RepositoryOwnedModelCheckpointManifestRequest(
                parent is null ? null : new(parent.RepositoryRelativeManifestPath, parent.Step, parent.WeightsSha256, parent.WeightsByteLength),
                parent is null ? "synthetic tiny-weight first checkpoint" : string.Empty,
                new(DpoAddress.ManifestRelativePath, DpoManifest.ManifestSha256), runtime ?? Runtime,
                DateTimeOffset.UnixEpoch, ["test:checkpoint-selection"]);
            using var weights = new MemoryStream(new byte[] { 0, 1, 2, 3, 4, 5, (byte)step }, writable: false);
            return new(address, Require(RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(address, weights, request)));
        }
        internal void RemoveCheckpoint(Checkpoint c)
        {
            var target = Path.GetFullPath(c.Address.CheckpointRoot);
            Assert.StartsWith(Path.GetFullPath(Repository) + Path.DirectorySeparatorChar, target);
            Assert.True(c.Address.Step > 1);
            Directory.Delete(target, recursive: true);
        }
        public void Dispose()
        {
            // Only this unique owned synthetic temp fixture, never the live repository.
            var target = Path.GetFullPath(Root);
            Assert.StartsWith(Path.GetFullPath(Path.GetTempPath()), target);
            if (Directory.Exists(target)) Directory.Delete(target, recursive: true);
        }
    }
}
