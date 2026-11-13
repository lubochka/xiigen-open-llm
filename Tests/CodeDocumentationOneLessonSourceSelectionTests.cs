// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using TrainableRouter.Training;
using DpoPairSideAuthorKind = DPO.Models.DpoPairSideAuthorKind;

namespace LlmFromScratch.Tests;

public sealed class CodeDocumentationOneLessonSourceSelectionTests(
    Xunit.Abstractions.ITestOutputHelper output)
{
    [Fact]
    public async Task SelectedTeacherHistory_EntireOwnerOrderAndClosedAliasGuards()
    {
        using (var h = CodeDocumentationPreparedBaselineTests.TeacherContextHarness())
        {
            var (b, a) = await CodeDocumentationPreparedBaselineTests.BindTeacherCompleted(h);
            var graph = CodeDocumentationPreparedBaselineTests.ReadTeacherHistory(h, b); var files = h.Files();
            Assert.Equal(new[] { "history-source", "ordinary-source" }, graph.Sources.Select(s => s.Descriptor.SourceId));
            Assert.Equal(2, graph.Pairs.Count);
            foreach (var source in graph.Sources)
            {
                var address = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(h.Fixture.RepositoryRoot,
                    source.Descriptor.DomainSlug, source.Descriptor.DesignPatternSlug, source.Descriptor.DatasetVersion);
                Assert.True(address.IsSuccess, address.ErrorCode);
                var actual = RepositoryOwnedDpoDatasetManifestSealer.OpenSealedFileIdentity(address.Value!, source.Manifest.RequestIdentity);
                Assert.True(actual.IsSuccess, actual.ErrorMessage);
                Assert.Equal(source.Descriptor.ManifestFileSha256, actual.Value!.ManifestFileSha256);
                Assert.Equal(source.Descriptor.ManifestFileByteLength, actual.Value.ManifestFileByteLength);
                Assert.Equal(source.Manifest.RequestIdentity.RequestIdentitySha256, actual.Value.Manifest.RequestIdentity.RequestIdentitySha256);
                Assert.Equal(source.Manifest.OrderedPairs, actual.Value.Manifest.OrderedPairs);
            }
            var path = Path.Combine(h.Fixture.RepositoryRoot, graph.Pairs[0].Row.RepositoryRelativePairFilePath.Replace('/', Path.DirectorySeparatorChar));
            var bytes = File.ReadAllBytes(path); var ticks = File.GetLastWriteTimeUtc(path); Exception? primary = null;
            try
            {
                File.WriteAllBytes(path, bytes.Concat(new byte[] { (byte)' ' }).ToArray());
                var c = b.Readback.Baseline.Calibration;
                var bad = h.Grant.SourceSelection!.OpenDeclaredContextHistoryExcludingOriginal(h.Fixture.RepositoryRoot, b.Context.Corpus,
                    c.RepresentativeAssessment.Cases, c.Calibration.Observations);
                Assert.False(bad.IsSuccess); Assert.Null(bad.Value); Assert.Equal(RepositoryOwnedDpoDatasetManifestSealer.InvalidErrorCode, bad.ErrorCode);
            }
            catch (Exception ex) { primary = ex; throw; }
            finally { CodeDocumentationPreparedBaselineTests.RestoreFixtureAfterFault(
                () => CodeDocumentationPreparedBaselineTests.RestoreOwnedHistoryPair(h, path, bytes, ticks), primary); }
            Assert.True(graph.SameGraph(CodeDocumentationPreparedBaselineTests.ReadTeacherHistory(h, b))); h.AssertFiles(files);
            var references = h.Grant.SourceSelection!.Sources.Reverse().ToArray();
            var reordered = CodeDocumentationOneLessonSourceSelection.Create(references, h.Grant.SourceSelection.PairSelections, h.Grant.SourceSelection.TeachingCurriculum);
            Assert.False(reordered.IsSuccess); Assert.Equal(CodeDocumentationOneLessonSourceSelection.InvalidErrorCode, reordered.ErrorCode);
            Assert.Equal(10, h.Client.Calls); Assert.Equal((1, 1, false), h.Open.Session.SelectedStudentAccounting);
        }
        foreach (var fault in new[] { "original-task", "original-digest" })
        {
            using var h = CodeDocumentationPreparedBaselineTests.TeacherContextHarness(historyFault: fault);
            var (b, a) = await CodeDocumentationPreparedBaselineTests.BindTeacherCompleted(h); var files = h.Files(); var invoked = 0;
            var result = await CodeDocumentationPreparedBaselineTests.ConsumeTeacherContent(h, b, a, assess: (_, _) =>
            { invoked++; throw new InvalidOperationException("Closed typed history leaves must refuse before assessment."); });
            Assert.False(result.IsSuccess, fault); Assert.Null(result.Value);
            Assert.Equal(CodeDocumentationOneLessonSourceSelection.HistoryInvalidErrorCode, result.ErrorCode); Assert.Equal(0, invoked);
            Assert.Equal(10, h.Client.Calls); h.AssertFiles(files);
        }
        foreach (var count in new[] { 0, 2 })
        {
            using var h = CodeDocumentationPreparedBaselineTests.TeacherContextHarness(historyPairs: count);
            var (b, a) = await CodeDocumentationPreparedBaselineTests.BindTeacherCompleted(h);
            var files = h.Files(); var calls = 0;
            var result = await CodeDocumentationPreparedBaselineTests.ConsumeTeacherContent(h, b, a,
                assess: (_, _) => { calls++; throw new InvalidOperationException("Whole history cardinality must refuse before assessment."); });
            Assert.False(result.IsSuccess); Assert.Null(result.Value);
            Assert.Equal(CodeDocumentationOneLessonSourceSelection.HistoryRequiredErrorCode, result.ErrorCode);
            Assert.Equal(0, calls);
            // Cardinality refusal must follow real full-file sealing of every ordered
            // pair, rather than an earlier fixture request-id collision or metadata-only proof.
            Assert.Equal(count, h.Fixture.ContextHistoryCreationInputs.Count);
            Assert.Equal(count == 0 ? new[] { "ordinary-source" } : new[] { "history-source", "ordinary-source" },
                h.Grant.SourceSelection!.Sources.Select(s => s.SourceId));
            var retainedPairs = h.Grant.SourceSelection!.Sources.SelectMany(s =>
            {
                var address = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(h.Fixture.RepositoryRoot, s.DomainSlug, s.DesignPatternSlug, s.DatasetVersion);
                Assert.True(address.IsSuccess, address.ErrorMessage);
                var actual = RepositoryOwnedDpoDatasetManifestSealer.OpenSealedFileIdentity(address.Value!, h.Fixture.DpoRequestIdentity);
                Assert.True(actual.IsSuccess, actual.ErrorCode + ":" + actual.ErrorMessage);
                Assert.Equal(actual.Value!.Manifest.PairCount, actual.Value.Pairs.Count);
                if (s.SourceId == "history-source")
                {
                    Assert.Equal(count, actual.Value.Pairs.Count);
                    var expectedById = h.Fixture.ContextHistoryCreationInputs.ToDictionary(
                        p => FileDpoPairStore.ComputeIdentity(p, schemaVersion: 5), StringComparer.Ordinal);
                    Assert.Equal(count, expectedById.Count);
                    Assert.Equal(expectedById.Keys.OrderBy(id => id, StringComparer.Ordinal), actual.Value.Manifest.OrderedPairs.Select(row => row.PairId));
                    var expectedInManifestOrder = actual.Value.Manifest.OrderedPairs.Select(row => expectedById[row.PairId]).ToArray();
                    Assert.Equal(actual.Value.Manifest.OrderedPairs.Select(row => row.PairId),
                        actual.Value.Pairs.Select(p => FileDpoPairStore.ComputeIdentity(p, schemaVersion: 5)));
                    Assert.Equal(expectedInManifestOrder.Select(p => (p.TeacherProvenance!.RequestId, p.Prompt, p.Chosen, p.Rejected)),
                        actual.Value.Pairs.Select(p => (p.TeacherProvenance!.RequestId, p.Prompt, p.Chosen, p.Rejected)));
                }
                return actual.Value.Pairs;
            }).ToArray();
            Assert.Equal(count + 1, retainedPairs.Length);
            Assert.Equal(count + 1, retainedPairs.Select(p => p.TeacherProvenance!.RequestId).Distinct(StringComparer.Ordinal).Count());
            Assert.Equal(10, h.Client.Calls); h.AssertFiles(files);
        }
    }


    // An independent declared table. Source/item/category array order is deliberately nonlexical.
    internal static CodeDocumentationOneLessonTeachingCurriculumV2 Curriculum()
        => new("declared-policy", "v2",
            [new("teach-b", "corpus-b", "bug", "missing-teaching", "v2", Hex('1'), 11, Hex('2'), Hex('3'), 13),
             new("teach-a", "corpus-a", "bug", "missing-teaching", "v1", Hex('4'), 17, Hex('5'), Hex('6'), 19)],
            [new("item-b", "teach-b", "sha256:" + Hex('7'), "sha256:" + Hex('8')),
             new("item-a", "teach-a", "sha256:" + Hex('9'), "sha256:" + Hex('a'))],
            [new("category-b", ["item-b", "item-a"]), new("category-a", ["item-a", "item-b"])]);

    internal static string CurriculumLiteral =>
        "{\"CategoryBindings\":[{\"Category\":\"category-b\",\"OrderedItemIds\":[\"item-b\",\"item-a\"]},{\"Category\":\"category-a\",\"OrderedItemIds\":[\"item-a\",\"item-b\"]}]," +
        "\"Items\":[{\"ItemId\":\"item-b\",\"ReferenceEntryId\":\"sha256:" + Hex('7') + "\",\"SourceEntityFingerprint\":\"sha256:" + Hex('8') + "\",\"SourceId\":\"teach-b\"}," +
        "{\"ItemId\":\"item-a\",\"ReferenceEntryId\":\"sha256:" + Hex('9') + "\",\"SourceEntityFingerprint\":\"sha256:" + Hex('a') + "\",\"SourceId\":\"teach-a\"}]," +
        "\"PolicyId\":\"declared-policy\",\"PolicyVersion\":\"v2\",\"Sources\":[" +
        "{\"CorpusId\":\"corpus-b\",\"CorpusSha256\":\"" + Hex('2') + "\",\"CorpusVersion\":\"v2\",\"DomainSlug\":\"bug\",\"ManifestFileByteLength\":11,\"ManifestFileSha256\":\"" + Hex('1') + "\",\"PayloadByteLength\":13,\"PayloadSha256\":\"" + Hex('3') + "\",\"SourceId\":\"teach-b\",\"SubjectSlug\":\"missing-teaching\"}," +
        "{\"CorpusId\":\"corpus-a\",\"CorpusSha256\":\"" + Hex('5') + "\",\"CorpusVersion\":\"v1\",\"DomainSlug\":\"bug\",\"ManifestFileByteLength\":17,\"ManifestFileSha256\":\"" + Hex('4') + "\",\"PayloadByteLength\":19,\"PayloadSha256\":\"" + Hex('6') + "\",\"SourceId\":\"teach-a\",\"SubjectSlug\":\"missing-teaching\"}]}";

    [Fact]
    public void TeachingCurriculum_DeepSnapshotCatalogRecopyAndRealFreshOwnerRetainEveryArray()
    {
        using var fixture = new Fixture(); var dataset = fixture.Seal(0); var input = Curriculum();
        var selection = Require(CodeDocumentationOneLessonSourceSelection.Create([dataset.Source],
            [new(dataset.Row.PairId, dataset.Source.SourceId)], input));
        var expected = Oracle(selection)[..^1] + ",\"TeachingCurriculum\":" + CurriculumLiteral + "}";
        var hash = Sha256(Encoding.UTF8.GetBytes("xiigen-s04-source-selection-v2\n" + expected));
        Assert.Equal(expected, Encoding.UTF8.GetString(CodeDocumentationOneLessonV2Wire.CanonicalSourceSelectionBytes(selection)));
        Assert.Equal(hash, selection.SourceSelectionSha256);
        var catalog = CodeDocumentationOneLessonGrantCatalog.FromTrustedGrants([Grant(selection)], 8);
        var copied = Require(catalog.Resolve(new("objective", "selector"))).SourceSelection!;
        Assert.NotSame(selection, copied);
        MutateArrays(input); MutateArrays(selection.TeachingCurriculum!); MutateArrays(copied.TeachingCurriculum!);
        Assert.Equal(CurriculumLiteral, TeachingJson(selection.TeachingCurriculum!));
        Assert.Equal(CurriculumLiteral, TeachingJson(copied.TeachingCurriculum!));
        Assert.Equal(hash, copied.SourceSelectionSha256);
        var fresh = Require(copied.OpenFresh(fixture.Repository));
        Assert.Equal(hash, fresh.Selection.SourceSelectionSha256);
        Assert.Equal(CurriculumLiteral, TeachingJson(fresh.Selection.TeachingCurriculum!));
        Assert.Equal(dataset.Row, fresh.SelectedRow);
        var missing = Require(RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(
            fixture.Repository, "bug", "missing-teaching", "v1"));
        Assert.False(Directory.Exists(Path.GetDirectoryName(missing.ManifestPath)));
        var altered = Require(CodeDocumentationOneLessonSourceSelection.Create([dataset.Source],
            [new(dataset.Row.PairId, dataset.Source.SourceId)], Curriculum() with { PolicyVersion = "v3" }));
        Assert.Equal(CodeDocumentationOneLessonGrantCatalog.InvalidCatalogErrorCode,
            CodeDocumentationOneLessonGrantCatalog.FromTrustedGrants([Grant(altered) with
            { DescriptorsV2 = Descriptors(hash) }], 8).Resolve(new("objective", "selector")).ErrorCode);
        Assert.Equal(hash, Require(catalog.Resolve(new("objective", "selector"))).SourceSelection!.SourceSelectionSha256);
        AssertNoEffects(fixture); LogRuntimeIdentities();
    }

    private static void MutateArrays(CodeDocumentationOneLessonTeachingCurriculumV2 value)
    {
        value.Sources[0] = value.Sources[0] with { CorpusId = "mutated" };
        value.Items[0] = value.Items[0] with { ReferenceEntryId = "sha256:" + Hex('f') };
        value.CategoryBindings[0].OrderedItemIds[0] = "mutated";
        value.CategoryBindings[1] = new("mutated", ["mutated"]);
    }

    public static IEnumerable<object[]> TeachingLeafCases()
        => LeafPaths(JsonNode.Parse(CurriculumLiteral)!).Select(p => new object[] { p });

    [Theory] [MemberData(nameof(TeachingLeafCases))]
    public void TeachingCurriculum_EveryLeafChangesWholeHashOrRejectsLinkedContract(string path)
    {
        var control = Require(CodeDocumentationOneLessonSourceSelection.Create([ShapeSource()], [Mapping()], Curriculum()));
        var tree = JsonNode.Parse(CurriculumLiteral)!; var leaf = NodeAt(tree, path);
        var split = path.LastIndexOf('.'); var parent = split < 0 ? tree : NodeAt(tree, path[..split]);
        var key = split < 0 ? path : path[(split + 1)..];
        var originalText = leaf.GetValueKind() == JsonValueKind.String ? leaf.GetValue<string>() : "";
        JsonNode replacement = leaf.GetValueKind() == JsonValueKind.Number ? JsonValue.Create(leaf.GetValue<long>() + 1)! :
            JsonValue.Create(leaf.GetValue<string>().StartsWith("sha256:", StringComparison.Ordinal) ? "sha256:" + Hex('e') :
                leaf.GetValue<string>().Length == 64 ? Hex('e') : leaf.GetValue<string>() + "x")!;
        if (path.Contains(".OrderedItemIds.", StringComparison.Ordinal))
        {
            // Preserve the link invariant while changing each observed ordered cell.
            var a = parent.AsArray(); var first = a[0]!.DeepClone(); a[0] = a[1]!.DeepClone(); a[1] = first;
        }
        else if (path.StartsWith("Items.", StringComparison.Ordinal) && key == "SourceId")
        {
            var index = int.Parse(path.Split('.')[1], CultureInfo.InvariantCulture); var other = 1 - index;
            parent[key] = tree["Sources"]![other]!["SourceId"]!.DeepClone();
            tree["Items"]![other]!["SourceId"] = originalText;
        }
        else
        {
            parent[key] = replacement;
            if (path.StartsWith("Sources.", StringComparison.Ordinal) && key == "SourceId")
                foreach (var item in tree["Items"]!.AsArray())
                    if (item!["SourceId"]!.GetValue<string>() == originalText) item["SourceId"] = replacement.DeepClone();
            if (path.StartsWith("Items.", StringComparison.Ordinal) && key == "ItemId")
                foreach (var binding in tree["CategoryBindings"]!.AsArray())
                    for (var i = 0; i < binding!["OrderedItemIds"]!.AsArray().Count; i++)
                        if (binding["OrderedItemIds"]![i]!.GetValue<string>() == originalText)
                            binding["OrderedItemIds"]![i] = replacement.DeepClone();
        }
        var changed = CodeDocumentationOneLessonSourceSelection.Create([ShapeSource()], [Mapping()],
            tree.Deserialize<CodeDocumentationOneLessonTeachingCurriculumV2>()!);
        Assert.True(changed.IsSuccess, path + ": " + changed.ErrorCode);
        Assert.NotEqual(control.SourceSelectionSha256, changed.Value!.SourceSelectionSha256);
        Assert.Equal(control.SourceSelectionSha256, Require(CodeDocumentationOneLessonSourceSelection.Create(
            [ShapeSource()], [Mapping()], Curriculum())).SourceSelectionSha256);
    }

    [Theory] [InlineData("sources")] [InlineData("items")] [InlineData("bindings")] [InlineData("ordered-items")] [InlineData("ordered-items-second")]
    public void TeachingCurriculum_EveryArrayOrderIsCommittedWithoutSorting(string array)
    {
        var input = Curriculum(); var original = Require(CodeDocumentationOneLessonSourceSelection.Create([ShapeSource()], [Mapping()], input));
        switch (array)
        {
            case "sources": Array.Reverse(input.Sources); break;
            case "items": Array.Reverse(input.Items); break;
            case "bindings": Array.Reverse(input.CategoryBindings); break;
            default: Array.Reverse(input.CategoryBindings[array == "ordered-items-second" ? 1 : 0].OrderedItemIds); break;
        }
        Assert.NotEqual(original.SourceSelectionSha256, Require(CodeDocumentationOneLessonSourceSelection.Create(
            [ShapeSource()], [Mapping()], input)).SourceSelectionSha256);
    }

    [Theory]
    [InlineData("null-sources")] [InlineData("null-items")] [InlineData("null-bindings")] [InlineData("null-source")]
    [InlineData("null-item")] [InlineData("null-binding")] [InlineData("null-order")] [InlineData("null-id")]
    [InlineData("empty-sources")] [InlineData("empty-items")] [InlineData("empty-bindings")] [InlineData("empty-order")]
    [InlineData("duplicate-source")] [InlineData("duplicate-address")] [InlineData("duplicate-item")]
    [InlineData("duplicate-entry")] [InlineData("duplicate-category")] [InlineData("duplicate-order")]
    [InlineData("missing-source")] [InlineData("missing-item")] [InlineData("unused-source")] [InlineData("unused-item")]
    [InlineData("bare-entry")] [InlineData("bare-fingerprint")] [InlineData("ref-manifest")] [InlineData("ref-corpus")] [InlineData("ref-payload")]
    [InlineData("manifest-zero")] [InlineData("manifest-over")] [InlineData("payload-zero")] [InlineData("payload-over")]
    [InlineData("unicode")]
    public void TeachingCurriculum_RejectsMalformedWholeSnapshotAndRestoresValidControl(string fault)
    {
        var c = Curriculum();
        switch (fault)
        {
            case "null-sources": c = c with { Sources = null! }; break;
            case "null-items": c = c with { Items = null! }; break;
            case "null-bindings": c = c with { CategoryBindings = null! }; break;
            case "null-source": c.Sources[0] = null!; break;
            case "null-item": c.Items[0] = null!; break;
            case "null-binding": c.CategoryBindings[0] = null!; break;
            case "null-order": c.CategoryBindings[0] = c.CategoryBindings[0] with { OrderedItemIds = null! }; break;
            case "null-id": c.CategoryBindings[0].OrderedItemIds[0] = null!; break;
            case "empty-sources": c = c with { Sources = [] }; break;
            case "empty-items": c = c with { Items = [] }; break;
            case "empty-bindings": c = c with { CategoryBindings = [] }; break;
            case "empty-order": c.CategoryBindings[0] = c.CategoryBindings[0] with { OrderedItemIds = [] }; break;
            case "duplicate-source": c.Sources[1] = c.Sources[1] with { SourceId = c.Sources[0].SourceId }; break;
            case "duplicate-address": c.Sources[1] = c.Sources[1] with { CorpusVersion = c.Sources[0].CorpusVersion }; break;
            case "duplicate-item": c.Items[1] = c.Items[1] with { ItemId = c.Items[0].ItemId }; break;
            case "duplicate-entry": c.Items[1] = c.Items[0] with { ItemId = "item-a" }; break;
            case "duplicate-category": c.CategoryBindings[1] = c.CategoryBindings[1] with { Category = c.CategoryBindings[0].Category }; break;
            case "duplicate-order": c.CategoryBindings[0].OrderedItemIds[1] = c.CategoryBindings[0].OrderedItemIds[0]; break;
            case "missing-source": c.Items[0] = c.Items[0] with { SourceId = "absent" }; break;
            case "missing-item": c.CategoryBindings[0].OrderedItemIds[0] = "absent"; break;
            case "unused-source": c = c with { Sources = [.. c.Sources, c.Sources[0] with { SourceId = "unused", CorpusVersion = "v3" }] }; break;
            case "unused-item": c = c with { Items = [.. c.Items, c.Items[0] with { ItemId = "unused", ReferenceEntryId = "sha256:" + Hex('f') }] }; break;
            case "bare-entry": c.Items[0] = c.Items[0] with { ReferenceEntryId = Hex('7') }; break;
            case "bare-fingerprint": c.Items[0] = c.Items[0] with { SourceEntityFingerprint = Hex('8') }; break;
            case "ref-manifest": c.Sources[0] = c.Sources[0] with { ManifestFileSha256 = "sha256:" + Hex('1') }; break;
            case "ref-corpus": c.Sources[0] = c.Sources[0] with { CorpusSha256 = "sha256:" + Hex('2') }; break;
            case "ref-payload": c.Sources[0] = c.Sources[0] with { PayloadSha256 = "sha256:" + Hex('3') }; break;
            case "manifest-zero": c.Sources[0] = c.Sources[0] with { ManifestFileByteLength = 0 }; break;
            case "manifest-over": c.Sources[0] = c.Sources[0] with { ManifestFileByteLength = RepositoryOwnedReferenceCorpusManifestSealer.MaximumManifestBytes + 1 }; break;
            case "payload-zero": c.Sources[0] = c.Sources[0] with { PayloadByteLength = 0 }; break;
            case "payload-over": c.Sources[0] = c.Sources[0] with { PayloadByteLength = RepositoryOwnedReferenceCorpusManifestSealer.MaximumPayloadBytes + 1 }; break;
            default: c = c with { PolicyId = "policy\ud800" }; break;
        }
        Assert.Equal(CodeDocumentationOneLessonSourceSelection.InvalidErrorCode,
            CodeDocumentationOneLessonSourceSelection.Create([ShapeSource()], [Mapping()], c).ErrorCode);
        Assert.True(CodeDocumentationOneLessonSourceSelection.Create([ShapeSource()], [Mapping()], Curriculum()).IsSuccess);
    }

    [Theory] [MemberData(nameof(TeachingLeafCases))]
    public void TeachingCurriculum_NullEveryProvidedLeafRejectsBeforeSerialization(string path)
    {
        var tree = JsonNode.Parse(CurriculumLiteral)!; var split = path.LastIndexOf('.');
        var parent = split < 0 ? tree : NodeAt(tree, path[..split]); var key = split < 0 ? path : path[(split + 1)..];
        if (parent is JsonArray a) a[int.Parse(key, CultureInfo.InvariantCulture)] = null; else parent[key] = null;
        // Nonnullable long fields also reject at typed JSON admission instead of acquiring a zero default.
        try
        {
            var typed = tree.Deserialize<CodeDocumentationOneLessonTeachingCurriculumV2>();
            Assert.Equal(CodeDocumentationOneLessonSourceSelection.InvalidErrorCode,
                CodeDocumentationOneLessonSourceSelection.Create([ShapeSource()], [Mapping()], typed).ErrorCode);
        }
        catch (JsonException) { Assert.Equal(JsonValueKind.Number, NodeAt(JsonNode.Parse(CurriculumLiteral)!, path).GetValueKind()); }
        Assert.True(CodeDocumentationOneLessonSourceSelection.Create([ShapeSource()], [Mapping()], Curriculum()).IsSuccess);
    }

    [Fact]
    public void TeachingCurriculum_FiniteCompleteTableHasReferenceBoundsAndNoEvaluationCountQuota()
    {
        var control = Curriculum();
        var sources = Enumerable.Range(0, 201).Select(i => control.Sources[0] with
        { SourceId = "teach" + i, CorpusVersion = "v" + i,
          ManifestFileByteLength = RepositoryOwnedReferenceCorpusManifestSealer.MaximumManifestBytes,
          PayloadByteLength = RepositoryOwnedReferenceCorpusManifestSealer.MaximumPayloadBytes }).ToArray();
        var items = Enumerable.Range(0, 201).Select(i => control.Items[0] with
        { ItemId = "item" + i, SourceId = sources[i].SourceId, ReferenceEntryId = "sha256:" + i.ToString("x64", CultureInfo.InvariantCulture) }).ToArray();
        var c = control with { PolicyId = "external:unapproved", Sources = sources, Items = items,
            CategoryBindings = [new("external:category", items.Select(i => i.ItemId).ToArray())] };
        var selected = Require(CodeDocumentationOneLessonSourceSelection.Create([ShapeSource()], [Mapping()], c));
        Assert.Equal(201, selected.TeachingCurriculum!.Sources.Length);
        Assert.Equal(201, selected.TeachingCurriculum.Items.Length);
        Assert.Equal(201, selected.TeachingCurriculum.CategoryBindings[0].OrderedItemIds.Length);
        Assert.Single(selected.Sources); Assert.Single(selected.PairSelections);
    }

    [Theory] [InlineData("source-id")] [InlineData("item-id")] [InlineData("item-source-link")]
    public void TeachingCurriculum_ValidLinkedIdentityChangesCommitWholeGraph(string field)
    {
        var c = Curriculum(); var original = Require(CodeDocumentationOneLessonSourceSelection.Create([ShapeSource()], [Mapping()], c));
        if (field == "source-id")
        { c.Sources[0] = c.Sources[0] with { SourceId = "replacement" }; c.Items[0] = c.Items[0] with { SourceId = "replacement" }; }
        else if (field == "item-id")
        {
            c.Items[0] = c.Items[0] with { ItemId = "replacement" };
            foreach (var b in c.CategoryBindings) for (var i = 0; i < b.OrderedItemIds.Length; i++)
                if (b.OrderedItemIds[i] == "item-b") b.OrderedItemIds[i] = "replacement";
        }
        else
        { c.Items[0] = c.Items[0] with { SourceId = "teach-a" }; c.Items[1] = c.Items[1] with { SourceId = "teach-b" }; }
        Assert.NotEqual(original.SourceSelectionSha256, Require(CodeDocumentationOneLessonSourceSelection.Create([ShapeSource()], [Mapping()], c)).SourceSelectionSha256);
    }

    private static JsonNode NodeAt(JsonNode value, string path)
    { foreach (var key in path.Split('.', StringSplitOptions.RemoveEmptyEntries)) value = value is JsonArray a ? a[int.Parse(key, CultureInfo.InvariantCulture)]! : value[key]!; return value; }
    private static IEnumerable<string> LeafPaths(JsonNode value, string path = "")
    {
        if (value is JsonObject o) foreach (var p in o) foreach (var leaf in LeafPaths(p.Value!, path.Length == 0 ? p.Key : path + "." + p.Key)) yield return leaf;
        else if (value is JsonArray a) for (var i = 0; i < a.Count; i++) foreach (var leaf in LeafPaths(a[i]!, path + "." + i)) yield return leaf;
        else yield return path;
    }
    internal static string TeachingJson(CodeDocumentationOneLessonTeachingCurriculumV2 c)
    {
        string Canonical(JsonNode n) => n is JsonObject o ? "{" + string.Join(",", o.OrderBy(x => x.Key, StringComparer.Ordinal).Select(p => JsonSerializer.Serialize(p.Key) + ":" + Canonical(p.Value!))) + "}" :
            n is JsonArray a ? "[" + string.Join(",", a.Select(x => Canonical(x!))) + "]" : n.ToJsonString();
        return Canonical(JsonSerializer.SerializeToNode(c)!);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(8)]
    public void FreshSelection_ProvesCompleteRealSealedSetAndExactPair(int count)
    {
        using var fixture = new Fixture();
        var datasets = Enumerable.Range(0, count).Select(i => fixture.Seal(i)).ToArray();
        var target = datasets[^1];
        var selection = Require(CodeDocumentationOneLessonSourceSelection.Create(
            datasets.Select(d => d.Source), [new(target.Row.PairId, target.Source.SourceId)]));
        Assert.Equal(count, selection.Sources.Count);
        Assert.Equal(Oracle(selection), Encoding.UTF8.GetString(
            CodeDocumentationOneLessonV2Wire.CanonicalSourceSelectionBytes(selection)));
        Assert.Equal(Sha256(Encoding.UTF8.GetBytes("xiigen-s04-source-selection-v2\n" + Oracle(selection))),
            selection.SourceSelectionSha256);
        var fresh = Require(selection.OpenFresh(fixture.Repository));
        Assert.Equal(selection.SourceSelectionSha256, fresh.Selection.SourceSelectionSha256);
        Assert.Equal(selection.Sources, fresh.Selection.Sources);
        Assert.Equal(target.Source.SourceId, fresh.SourceId);
        Assert.Equal(target.Row, fresh.SelectedRow);
        Assert.Equal(JsonSerializer.Serialize(target.Pair), JsonSerializer.Serialize(fresh.Pair));
        var old = Require(RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(target.Address, fixture.Identity));
        var full = Require(RepositoryOwnedDpoDatasetManifestSealer.OpenSealedFileIdentity(target.Address, fixture.Identity));
        Assert.Equal(JsonSerializer.Serialize(old.Manifest), JsonSerializer.Serialize(full.Manifest));
        Assert.Equal(JsonSerializer.Serialize(old.Pairs), JsonSerializer.Serialize(full.Pairs));
        Assert.Equal(target.Source.ManifestFileSha256, full.ManifestFileSha256);
        Assert.Equal(target.Source.ManifestFileByteLength, full.ManifestFileByteLength);
        Assert.Equal(RepositoryOwnedDpoDatasetManifestSealer.RequestIdentityMismatchErrorCode,
            RepositoryOwnedDpoDatasetManifestSealer.OpenSealedFileIdentity(target.Address,
                Require(RepositoryOwnedDpoDatasetManifestSealer.CreateRequestIdentity([Hex('e')],
                    fixture.Identity.OpenTeacher, fixture.Identity.GenerationPolicy, fixture.Identity.Creator))).ErrorCode);
        AssertNoEffects(fixture);
        LogRuntimeIdentities();
    }

    [Fact]
    public void CanonicalWriter_DefaultArraysRemainRejectedAndCompactGoldenIsUnchanged()
    {
        using var array = JsonDocument.Parse("{\"Sources\":[]}");
        Assert.Throws<JsonException>(() => CodeDocumentationOneLessonV2Wire.CanonicalBytes(array.RootElement));
        Assert.Throws<JsonException>(() => CodeDocumentationOneLessonV2Wire.IsCanonical(array.RootElement));
        var descriptor = Descriptors(Hex('1'), 200, 5000);
        var expected = "{\"CandidateSelectionSha256\":\"" + Hex('3') + "\",\"CaseCount\":200," +
            "\"CorpusDescriptorSha256\":\"" + Hex('4') + "\",\"EvaluationPlanSha256\":\"" + Hex('5') +
            "\",\"EvaluatorProfileSha256\":\"" + Hex('6') + "\",\"ParentCheckpointSha256\":\"" + Hex('2') +
            "\",\"ProfileSelectionSha256\":\"" + Hex('7') + "\",\"RepeatCount\":5000,\"SchemaVersion\":2," +
            "\"SourceSelectionSha256\":\"" + Hex('1') + "\"}";
        var bytes = CodeDocumentationOneLessonV2Wire.CanonicalBytes(JsonSerializer.SerializeToElement(descriptor));
        Assert.Equal(Encoding.UTF8.GetBytes(expected), bytes);
        Assert.Equal(697, bytes.Length);
        // This additional golden has a feasible joint budget (200 * 25 = 5000).
        // The 697-byte corner above measures independent scalar ceilings only.
        var feasible = CodeDocumentationOneLessonV2Wire.CanonicalBytes(
            JsonSerializer.SerializeToElement(Descriptors(Hex('1'), 200, 25)));
        Assert.Equal(Encoding.UTF8.GetBytes(expected.Replace("\"RepeatCount\":5000", "\"RepeatCount\":25",
            StringComparison.Ordinal)), feasible);
        Assert.Equal(695, feasible.Length);
        var selection = Require(CodeDocumentationOneLessonSourceSelection.Create([ShapeSource()], [Mapping()]));
        Assert.Equal(Oracle(selection), Encoding.UTF8.GetString(
            CodeDocumentationOneLessonV2Wire.CanonicalSourceSelectionBytes(selection)));
    }

    [Theory]
    [InlineData("role")]
    [InlineData("null-role")]
    [InlineData("token-leading")]
    [InlineData("token-space")]
    [InlineData("token-unicode")]
    [InlineData("token-129")]
    [InlineData("null-token")]
    [InlineData("domain-dot")]
    [InlineData("domain-root")]
    [InlineData("domain-empty")]
    [InlineData("null-domain")]
    [InlineData("pattern-upper")]
    [InlineData("version-97")]
    [InlineData("raw-upper")]
    [InlineData("sealed-short")]
    [InlineData("null-hash")]
    [InlineData("length-zero")]
    [InlineData("length-over")]
    public void PureFreeze_RejectsSingleInvalidSourceFieldAndRestoresControl(string fault)
    {
        var valid = ShapeSource();
        var invalid = fault switch
        {
            "role" => valid with { Role = "ClosedEvaluation" },
            "null-role" => valid with { Role = null! },
            "token-leading" => valid with { SourceId = "-source" },
            "token-space" => valid with { SourceId = " source" },
            "token-unicode" => valid with { SourceId = "söurce" },
            "token-129" => valid with { SourceId = new('a', 129) },
            "null-token" => valid with { SourceId = null! },
            "domain-dot" => valid with { DomainSlug = "bug.test" },
            "domain-root" => valid with { DomainSlug = "/bug" },
            "domain-empty" => valid with { DomainSlug = "" },
            "null-domain" => valid with { DomainSlug = null! },
            "pattern-upper" => valid with { DesignPatternSlug = "Pattern" },
            "version-97" => valid with { DatasetVersion = new('a', 97) },
            "raw-upper" => valid with { ManifestFileSha256 = new('A', 64) },
            "sealed-short" => valid with { SealedManifestSha256 = "a" },
            "null-hash" => valid with { ManifestFileSha256 = null! },
            "length-zero" => valid with { ManifestFileByteLength = 0 },
            _ => valid with { ManifestFileByteLength = 8_388_609 },
        };
        Assert.True(CodeDocumentationOneLessonSourceSelection.Create([valid], [Mapping()]).IsSuccess);
        Assert.Equal(CodeDocumentationOneLessonSourceSelection.InvalidErrorCode,
            CodeDocumentationOneLessonSourceSelection.Create([invalid], [Mapping()]).ErrorCode);
        Assert.True(CodeDocumentationOneLessonSourceSelection.Create([valid], [Mapping()]).IsSuccess);
    }

    [Theory]
    [InlineData("empty")]
    [InlineData("nine")]
    [InlineData("duplicate-id")]
    [InlineData("duplicate-address")]
    [InlineData("unsorted")]
    [InlineData("null-list")]
    [InlineData("null-row")]
    [InlineData("map-empty")]
    [InlineData("map-two")]
    [InlineData("map-null")]
    [InlineData("map-null-row")]
    [InlineData("map-hash")]
    [InlineData("map-token")]
    [InlineData("map-unresolved")]
    public void PureFreeze_RejectsWholeShapeFaultWithoutSelectingSubset(string fault)
    {
        var valid = ShapeSource();
        CodeDocumentationOneLessonSourceV2[]? sources = [valid];
        CodeDocumentationOneLessonPairSelectionV2[]? pairs = [Mapping()];
        switch (fault)
        {
            case "empty": sources = []; break;
            case "nine": sources = Enumerable.Range(0, 9).Select(i => valid with
                { SourceId = "source" + i, DatasetVersion = "v" + i }).ToArray(); break;
            case "duplicate-id": sources = [valid, valid with { DatasetVersion = "v2" }]; break;
            case "duplicate-address": sources = [valid, valid with { SourceId = "source1" }]; break;
            case "unsorted": sources = [valid with { SourceId = "source1", DatasetVersion = "v2" }, valid]; break;
            case "null-list": sources = null; break;
            case "null-row": sources = [null!]; break;
            case "map-empty": pairs = []; break;
            case "map-two": pairs = [Mapping(), Mapping()]; break;
            case "map-null": pairs = null; break;
            case "map-null-row": pairs = [null!]; break;
            case "map-hash": pairs = [Mapping() with { TrainingPairId = Hex('a').ToUpperInvariant() }]; break;
            case "map-token": pairs = [Mapping() with { SourceId = "-source" }]; break;
            case "map-unresolved": pairs = [Mapping() with { SourceId = "other" }]; break;
        }
        Assert.Equal(CodeDocumentationOneLessonSourceSelection.InvalidErrorCode,
            CodeDocumentationOneLessonSourceSelection.Create(sources, pairs).ErrorCode);
        Assert.True(CodeDocumentationOneLessonSourceSelection.Create([valid], [Mapping()]).IsSuccess);
    }

    [Fact]
    public void PureFreeze_ExactBoundsAndCatalogReplayRetainFrozenMaterialDespiteCallerMutation()
    {
        var source = ShapeSource() with { SourceId = "A" + new string('_', 127),
            DomainSlug = new('a', 96), ManifestFileByteLength = 8_388_608 };
        var sources = new List<CodeDocumentationOneLessonSourceV2> { source };
        var pairs = new List<CodeDocumentationOneLessonPairSelectionV2> { new(Hex('a'), source.SourceId) };
        var selection = Require(CodeDocumentationOneLessonSourceSelection.Create(sources, pairs));
        var committed = selection.SourceSelectionSha256;
        var grant = Grant(selection);
        var v1Bytes = JsonSerializer.SerializeToUtf8Bytes(grant);
        Assert.Equal(v1Bytes, JsonSerializer.SerializeToUtf8Bytes(grant with { SourceSelection = null }));
        var catalog = CodeDocumentationOneLessonGrantCatalog.FromTrustedGrants([grant]);
        sources[0] = ShapeSource(); pairs.Clear();
        var resolved = Require(catalog.Resolve(new(grant.ObjectiveId, grant.GrantSelector)));
        var replay = Require(catalog.Revalidate(grant.GrantId, grant.GrantVersion));
        Assert.Same(resolved.SourceSelection, replay.SourceSelection);
        Assert.Equal(committed, replay.SourceSelection!.SourceSelectionSha256);
        Assert.Equal(source, Assert.Single(replay.SourceSelection.Sources));
        Assert.Equal(source.SourceId, Assert.Single(replay.SourceSelection.PairSelections).SourceId);
        Assert.False(selection.Sources is CodeDocumentationOneLessonSourceV2[]);
        Assert.True(((ICollection<CodeDocumentationOneLessonSourceV2>)selection.Sources).IsReadOnly);
        Assert.Throws<NotSupportedException>(() => ((IList<CodeDocumentationOneLessonSourceV2>)selection.Sources)[0] = ShapeSource());
        Assert.Throws<NotSupportedException>(() => ((IList<CodeDocumentationOneLessonPairSelectionV2>)selection.PairSelections).Clear());
        Assert.Equal(committed, selection.SourceSelectionSha256);
        Assert.Equal(CodeDocumentationOneLessonGrantCatalog.InvalidCatalogErrorCode,
            CodeDocumentationOneLessonGrantCatalog.FromTrustedGrants([grant with { FormatVersion = 1, DescriptorsV2 = null }])
                .Resolve(new(grant.ObjectiveId, grant.GrantSelector)).ErrorCode);
        Assert.Equal(CodeDocumentationOneLessonGrantCatalog.InvalidCatalogErrorCode,
            CodeDocumentationOneLessonGrantCatalog.FromTrustedGrants([grant with
                { DescriptorsV2 = grant.DescriptorsV2! with { SourceSelectionSha256 = Hex('f') } }])
                .Resolve(new(grant.ObjectiveId, grant.GrantSelector)).ErrorCode);
        Assert.True(CodeDocumentationOneLessonGrantCatalog.FromTrustedGrants([grant with { SourceSelection = null }])
            .Resolve(new(grant.ObjectiveId, grant.GrantSelector)).IsSuccess);
    }

    [Theory]
    [InlineData("raw-hash")]
    [InlineData("raw-length")]
    [InlineData("sealed-hash")]
    [InlineData("layout")]
    [InlineData("bad-last")]
    [InlineData("zero-pair")]
    [InlineData("wrong-source")]
    [InlineData("pair-corruption")]
    public void FreshSelection_RejectsRealOwnerFaultAndRestoresValidTwin(string fault)
    {
        using var fixture = new Fixture();
        var first = fixture.Seal(0);
        var last = fixture.Seal(1);
        var sources = new[] { first.Source, last.Source };
        var mapping = new CodeDocumentationOneLessonPairSelectionV2(first.Row.PairId, first.Source.SourceId);
        var valid = Require(CodeDocumentationOneLessonSourceSelection.Create(sources, [mapping]));
        Assert.True(valid.OpenFresh(fixture.Repository).IsSuccess);
        string? mutatedPath = null;
        byte[]? original = null;
        switch (fault)
        {
            case "raw-hash": sources[1] = last.Source with { ManifestFileSha256 = Hex('b') }; break;
            case "raw-length": sources[1] = last.Source with { ManifestFileByteLength = last.Source.ManifestFileByteLength + 1 }; break;
            case "sealed-hash": sources[1] = last.Source with { SealedManifestSha256 = Hex('c') }; break;
            case "zero-pair": mapping = mapping with { TrainingPairId = Hex('d') }; break;
            case "wrong-source": mapping = mapping with { SourceId = last.Source.SourceId }; break;
            case "layout":
                mutatedPath = last.Address.ManifestPath; original = File.ReadAllBytes(mutatedPath);
                File.WriteAllBytes(mutatedPath, original.Concat(new byte[] { (byte)' ' }).ToArray());
                Assert.Equal(last.Source.SealedManifestSha256,
                    Require(RepositoryOwnedDpoDatasetManifestSealer.OpenManifestIdentityOnly(last.Address)).Manifest.ManifestSha256);
                break;
            case "bad-last":
                mutatedPath = last.Address.ManifestPath; original = File.ReadAllBytes(mutatedPath);
                File.WriteAllText(mutatedPath, "{}"); break;
            case "pair-corruption":
                mutatedPath = Path.Combine(fixture.Repository, first.Row.RepositoryRelativePairFilePath);
                original = File.ReadAllBytes(mutatedPath);
                var corrupt = original.ToArray(); corrupt[0] = (byte)'!'; File.WriteAllBytes(mutatedPath, corrupt);
                Assert.Equal(original.Length, new FileInfo(mutatedPath).Length);
                Assert.True(RepositoryOwnedDpoDatasetManifestSealer.OpenManifestFileIdentityOnly(first.Address).IsSuccess);
                break;
        }
        var bad = Require(CodeDocumentationOneLessonSourceSelection.Create(sources, [mapping]));
        Assert.False(bad.OpenFresh(fixture.Repository).IsSuccess);
        AssertNoEffects(fixture);
        if (mutatedPath is not null) File.WriteAllBytes(mutatedPath, original!);
        Assert.True(valid.OpenFresh(fixture.Repository).IsSuccess);
    }

    [Fact]
    public void FreshSelection_RejectsSameActualSealedPairInTwoOtherwiseValidSources()
    {
        using var fixture = new Fixture();
        var first = fixture.Seal(0);
        var second = fixture.Seal(1, first.Pair);
        Assert.Equal(first.Row.PairId, second.Row.PairId);
        Assert.True(RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(first.Address, fixture.Identity).IsSuccess);
        Assert.True(RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(second.Address, fixture.Identity).IsSuccess);
        var selection = Require(CodeDocumentationOneLessonSourceSelection.Create([first.Source, second.Source],
            [new(first.Row.PairId, first.Source.SourceId)]));
        Assert.Equal(CodeDocumentationOneLessonSourceSelection.InvalidErrorCode, selection.OpenFresh(fixture.Repository).ErrorCode);
        var twin = Require(CodeDocumentationOneLessonSourceSelection.Create([first.Source],
            [new(first.Row.PairId, first.Source.SourceId)]));
        Assert.True(twin.OpenFresh(fixture.Repository).IsSuccess);
        AssertNoEffects(fixture);
    }

    [Fact]
    public void FreshSelection_RejectsActualTransactionHeldRowEvenThoughIntegrityOpenStillSucceeds()
    {
        using var fixture = new Fixture();
        var dataset = fixture.Seal(0);
        var selection = Require(CodeDocumentationOneLessonSourceSelection.Create([dataset.Source],
            [new(dataset.Row.PairId, dataset.Source.SourceId)]));
        Assert.True(selection.OpenFresh(fixture.Repository).IsSuccess);
        var store = new FileDpoPairStore(dataset.Address.PairsRoot);
        var held = Require(store.PrepareTransaction([dataset.Pair.Domain]));
        Assert.NotNull(held);
        Assert.Empty(Require(store.GetAll()));
        Assert.True(RepositoryOwnedDpoDatasetManifestSealer.OpenSealedForManifestIntegrity(dataset.Address).IsSuccess);
        Assert.False(selection.OpenFresh(fixture.Repository).IsSuccess);
        AssertNoEffects(fixture);
        Assert.True(held.ReleaseIfUnmutated().IsSuccess);
        Assert.True(selection.OpenFresh(fixture.Repository).IsSuccess);
    }

    [Fact]
    public void NonselectedPairBytes_AreOutsideManifestOnlyProof()
    {
        using var fixture = new Fixture();
        var first = fixture.Seal(0); var other = fixture.Seal(1);
        var path = Path.Combine(fixture.Repository, other.Row.RepositoryRelativePairFilePath);
        var bytes = File.ReadAllBytes(path); var corrupt = bytes.ToArray(); corrupt[0] = (byte)'!';
        File.WriteAllBytes(path, corrupt);
        var selection = Require(CodeDocumentationOneLessonSourceSelection.Create([first.Source, other.Source],
            [new(first.Row.PairId, first.Source.SourceId)]));
        Assert.True(selection.OpenFresh(fixture.Repository).IsSuccess);
        Assert.False(RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(other.Address, fixture.Identity).IsSuccess);
        File.WriteAllBytes(path, bytes);
        Assert.True(RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(other.Address, fixture.Identity).IsSuccess);
        AssertNoEffects(fixture);
    }

    [Fact]
    public void DeclaredTeachingReadback_WholeGenuineGraphKeepsOrderClosedMetadataAndOwnedViews()
    {
        using var f = new Fixture(); var d = f.Seal(0);
        var a = f.SealTeaching(0); var b = f.SealTeaching(1);
        var input = ActualCurriculum(b, a);
        var selection = TeachingSelection(d, input);
        var catalog = CodeDocumentationOneLessonGrantCatalog.FromTrustedGrants([Grant(selection)]);
        var recopied = Require(catalog.Resolve(new("objective", "selector"))).SourceSelection!;
        Assert.NotSame(selection, recopied);
        Assert.Equal(selection.SourceSelectionSha256, recopied.SourceSelectionSha256);
        var before = FixtureBytes(f);
        var graph = Require(selection.OpenDeclaredTeachingCorpusIdentities(f.Repository));
        Assert.Equal(selection.SourceSelectionSha256, graph.Selection.SourceSelectionSha256);
        Assert.Equal(CodeDocumentationOneLessonV2Wire.CanonicalSourceSelectionBytes(selection),
            CodeDocumentationOneLessonV2Wire.CanonicalSourceSelectionBytes(graph.Selection));
        Assert.Equal(new[] { b.Descriptor.SourceId, a.Descriptor.SourceId }, graph.Sources.Select(s => s.Descriptor.SourceId));
        foreach (var expected in new[] { b, a })
        {
            var actual = Assert.Single(graph.Sources.Where(s => s.Descriptor.SourceId == expected.Descriptor.SourceId));
            var owner = Require(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedFileIdentity(expected.Address));
            Assert.Equal(expected.Descriptor, actual.Descriptor);
            Assert.Equal(owner.Manifest, actual.Manifest);
            Assert.Equal(owner.ManifestFileSha256, actual.ManifestFileSha256);
            Assert.Equal(owner.ManifestFileByteLength, actual.ManifestFileByteLength);
            Assert.Equal(owner.Corpus.Entries.Select(e => e.EntryId), actual.Entries.Select(e => e.EntryId));
            foreach (var row in owner.Corpus.Entries)
            {
                var safe = Assert.Single(actual.Entries.Where(e => e.EntryId == row.EntryId));
                Assert.Equal(row.SourceEntityFingerprint, safe.SourceEntityFingerprint);
                Assert.Equal(row.DomainTag, safe.DomainTag); Assert.Equal(row.SubjectId, safe.SubjectId);
                Assert.Equal(row.Partition, safe.Partition);
                Assert.Equal(FrozenReferenceEntry.ComputeNormalizedTextSha256(row.OwnerInput), safe.OwnerInputSha256);
                Assert.Equal(row.PaidReferenceProvenance, safe.PaidReferenceProvenance);
                Assert.True(safe.PaidReferenceProvenance.ClosedPaidReference);
                Assert.True(safe.PaidReferenceProvenance.PaidReferenceOutputNeverTrainable);
                Assert.True(safe.PaidReferenceProvenance.PaidReferenceOutputNeverDpoPairSide);
            }
        }
        Assert.Equal(input.Items.Select(i => i.ItemId), graph.Items.Select(i => i.DeclaredItem.ItemId));
        Assert.Contains(graph.Items, i => i.Entry.Partition == FrozenReferencePartition.Train);
        Assert.Contains(graph.Items, i => i.Entry.Partition == FrozenReferencePartition.HeldOut);
        foreach (var item in graph.Items)
        {
            Assert.Equal(item.DeclaredItem.SourceId, item.Source.Descriptor.SourceId);
            Assert.Equal(item.DeclaredItem.ReferenceEntryId, item.Entry.EntryId);
            Assert.Equal(item.DeclaredItem.SourceEntityFingerprint, item.Entry.SourceEntityFingerprint);
            Assert.Same(Assert.Single(item.Source.Entries.Where(e => e.EntryId == item.Entry.EntryId)), item.Entry);
        }
        Assert.Equal(input.CategoryBindings.Select(c => c.Category), graph.CategoryBindings.Select(c => c.Category));
        for (var i = 0; i < input.CategoryBindings.Length; i++)
        {
            Assert.Equal(input.CategoryBindings[i].OrderedItemIds, graph.CategoryBindings[i].OrderedItemIds);
            Assert.Equal(input.CategoryBindings[i].OrderedItemIds, graph.CategoryBindings[i].Items.Select(x => x.DeclaredItem.ItemId));
            foreach (var item in graph.CategoryBindings[i].Items) Assert.Same(graph.Items.Single(x => x.DeclaredItem.ItemId == item.DeclaredItem.ItemId), item);
        }
        // Neither full corpus rows nor raw paid/owner canaries can escape through any nested exposed property.
        var exposed = ExposedGraphStrings(graph).ToArray();
        Assert.DoesNotContain(exposed, x => x.Contains("OWNER-CLOSED-CANARY", StringComparison.Ordinal) || x.Contains("PAID-CLOSED-CANARY", StringComparison.Ordinal));
        Assert.Throws<NotSupportedException>(() => ((IList<CodeDocumentationOneLessonSourceSelection.DeclaredTeachingSourceReadback>)graph.Sources).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<CodeDocumentationOneLessonSourceSelection.DeclaredTeachingEntryIdentity>)graph.Sources[0].Entries).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<string>)graph.CategoryBindings[0].OrderedItemIds)[0] = "mutated");
        Assert.Throws<NotSupportedException>(() => ((IList<CodeDocumentationOneLessonSourceSelection.DeclaredTeachingItemReadback>)graph.CategoryBindings[0].Items).Clear());
        MutateArrays(input); MutateArrays(selection.TeachingCurriculum!); MutateArrays(graph.Selection.TeachingCurriculum!);
        MutateArrays(recopied.TeachingCurriculum!);
        var fresh = Require(recopied.OpenFresh(f.Repository));
        Assert.Equal(selection.SourceSelectionSha256, fresh.Selection.SourceSelectionSha256);
        Assert.Equal(graph.Selection.SourceSelectionSha256, Require(fresh.Selection.OpenDeclaredTeachingCorpusIdentities(f.Repository)).Selection.SourceSelectionSha256);
        var again = Require(selection.OpenDeclaredTeachingCorpusIdentities(f.Repository));
        Assert.Equal(graph.Selection.SourceSelectionSha256, again.Selection.SourceSelectionSha256);
        Assert.Equal(graph.Sources.SelectMany(s => s.Entries).Select(e => e.EntryId), again.Sources.SelectMany(s => s.Entries).Select(e => e.EntryId));
        Assert.Equal(graph.CategoryBindings[0].OrderedItemIds, again.CategoryBindings[0].OrderedItemIds);
        AssertFixtureBytes(f, before); AssertNoEffects(f); LogRuntimeIdentities();
    }

    [Theory]
    [InlineData("manifest-hash")] [InlineData("manifest-length")] [InlineData("corpus-hash")]
    [InlineData("payload-hash")] [InlineData("payload-length")] [InlineData("domain")]
    [InlineData("subject")] [InlineData("version")]
    public void DeclaredTeachingReadback_EveryOwnerPinRejectsWithGenuineAddressControl(string fault)
    {
        using var f = new Fixture(); var d = f.Seal(0); var a = f.SealTeaching(0); var b = f.SealTeaching(1);
        var valid = TeachingSelection(d, ActualCurriculum(b, a));
        Assert.True(valid.OpenDeclaredTeachingCorpusIdentities(f.Repository).IsSuccess);
        var input = ActualCurriculum(b, a); var s = input.Sources[0];
        if (fault is "domain" or "subject" or "version")
        {
            var alternate = f.SealTeaching(2, domain: fault == "domain" ? "translation" : s.DomainSlug,
                subject: fault == "subject" ? "alternate" : s.SubjectSlug,
                version: fault == "version" ? "alternate" : s.CorpusVersion);
            Assert.True(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedFileIdentity(alternate.Address).IsSuccess);
            s = s with { DomainSlug = alternate.Descriptor.DomainSlug, SubjectSlug = alternate.Descriptor.SubjectSlug,
                CorpusVersion = alternate.Descriptor.CorpusVersion };
        }
        else s = fault switch
        {
            "manifest-hash" => s with { ManifestFileSha256 = Hex('e') },
            "manifest-length" => s with { ManifestFileByteLength = s.ManifestFileByteLength + 1 },
            "corpus-hash" => s with { CorpusSha256 = Hex('e') },
            "payload-hash" => s with { PayloadSha256 = Hex('e') },
            _ => s with { PayloadByteLength = s.PayloadByteLength + 1 },
        };
        input.Sources[0] = s;
        var before = FixtureBytes(f);
        Assert.True(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedFileIdentity(b.Address).IsSuccess);
        var changed = TeachingSelection(d, input);
        Assert.Equal(CodeDocumentationOneLessonSourceSelection.InvalidErrorCode, changed.OpenDeclaredTeachingCorpusIdentities(f.Repository).ErrorCode);
        Assert.NotEqual(valid.SourceSelectionSha256, changed.SourceSelectionSha256);
        Assert.True(valid.OpenDeclaredTeachingCorpusIdentities(f.Repository).IsSuccess);
        AssertFixtureBytes(f, before); AssertNoEffects(f);
    }

    [Theory] [InlineData("missing-entry")] [InlineData("fingerprint")] [InlineData("foreign-entry")]
    public void DeclaredTeachingReadback_ActualJoinRejectsAbsentWrongFingerprintAndOtherGenuineSource(string fault)
    {
        using var f = new Fixture(); var d = f.Seal(0); var a = f.SealTeaching(0); var b = f.SealTeaching(1);
        var valid = TeachingSelection(d, ActualCurriculum(b, a));
        Assert.True(valid.OpenDeclaredTeachingCorpusIdentities(f.Repository).IsSuccess);
        Assert.True(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedFileIdentity(a.Address).IsSuccess);
        Assert.True(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedFileIdentity(b.Address).IsSuccess);
        var c = ActualCurriculum(b, a); var original = c.Items[0];
        c.Items[0] = fault switch
        {
            "missing-entry" => original with { ReferenceEntryId = "sha256:" + Hex('e') },
            "fingerprint" => original with { SourceEntityFingerprint = "sha256:" + Hex('e') },
            _ => original with { ReferenceEntryId = a.Corpus.Entries[0].EntryId, SourceEntityFingerprint = a.Corpus.Entries[0].SourceEntityFingerprint },
        };
        var before = FixtureBytes(f); var result = TeachingSelection(d, c).OpenDeclaredTeachingCorpusIdentities(f.Repository);
        Assert.Equal(CodeDocumentationOneLessonSourceSelection.InvalidErrorCode, result.ErrorCode);
        Assert.Null(result.Value); Assert.DoesNotContain("CANARY", result.ErrorMessage);
        c.Items[0] = original;
        Assert.True(TeachingSelection(d, c).OpenDeclaredTeachingCorpusIdentities(f.Repository).IsSuccess);
        AssertFixtureBytes(f, before); AssertNoEffects(f);
    }

    [Theory] [InlineData("manifest-missing")] [InlineData("manifest-tamper")]
    [InlineData("payload-missing")] [InlineData("unused-row-tamper")]
    public void DeclaredTeachingReadback_WholeOwnerRejectsMissingOrCorruptedFilesAndRestores(string fault)
    {
        using var f = new Fixture(); var d = f.Seal(0); var a = f.SealTeaching(0); var b = f.SealTeaching(1);
        var selection = TeachingSelection(d, ActualCurriculum(b, a));
        Assert.True(selection.OpenDeclaredTeachingCorpusIdentities(f.Repository).IsSuccess);
        var path = fault.StartsWith("manifest", StringComparison.Ordinal) ? a.Address.ManifestPath : a.PayloadPath;
        var bytes = File.ReadAllBytes(path); var before = FixtureBytes(f);
        try
        {
            if (fault.EndsWith("missing", StringComparison.Ordinal)) File.Delete(path);
            else if (fault == "unused-row-tamper")
            {
                // Alter an actual unused payload row while retaining valid JSON; full payload pins must detect it.
                var tree = JsonNode.Parse(bytes)!; var unused = a.Corpus.Entries[^1].EntryId;
                var row = tree["entries"]!.AsArray().Single(x => x!["entryId"]!.GetValue<string>() == unused)!;
                row["ownerInput"] = "OWNER-CLOSED-CANARY-corrupted-unused";
                File.WriteAllBytes(path, JsonSerializer.SerializeToUtf8Bytes(tree));
            }
            else { var corrupt = bytes.ToArray(); corrupt[0] = (byte)'!'; File.WriteAllBytes(path, corrupt); }
            var owner = RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedFileIdentity(a.Address);
            Assert.False(owner.IsSuccess);
            var result = selection.OpenDeclaredTeachingCorpusIdentities(f.Repository);
            Assert.False(result.IsSuccess); Assert.Equal(owner.ErrorCode, result.ErrorCode); Assert.Null(result.Value);
            Assert.DoesNotContain("CANARY", result.ErrorMessage);
        }
        finally { File.WriteAllBytes(path, bytes); }
        Assert.True(selection.OpenDeclaredTeachingCorpusIdentities(f.Repository).IsSuccess);
        AssertFixtureBytes(f, before); AssertNoEffects(f);
    }

    [Fact]
    public void DeclaredTeachingReadback_GenuineReplacementNeedsItsWholePinsItemsAndNewSourceCommitment()
    {
        using var f = new Fixture(); var d = f.Seal(0); var a = f.SealTeaching(0); var b = f.SealTeaching(1); var other = f.SealTeaching(2);
        var original = TeachingSelection(d, ActualCurriculum(b, a));
        var oldGraph = Require(original.OpenDeclaredTeachingCorpusIdentities(f.Repository));
        Assert.True(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedFileIdentity(other.Address).IsSuccess);
        var incomplete = ActualCurriculum(b, a);
        incomplete.Sources[0] = other.Descriptor with { SourceId = b.Descriptor.SourceId, CorpusId = b.Descriptor.CorpusId };
        Assert.Equal(CodeDocumentationOneLessonSourceSelection.InvalidErrorCode,
            TeachingSelection(d, incomplete).OpenDeclaredTeachingCorpusIdentities(f.Repository).ErrorCode);
        var replacement = ActualCurriculum(other, a);
        var next = TeachingSelection(d, replacement); var graph = Require(next.OpenDeclaredTeachingCorpusIdentities(f.Repository));
        Assert.NotEqual(original.SourceSelectionSha256, next.SourceSelectionSha256);
        Assert.Equal(other.Descriptor, graph.Sources[0].Descriptor);
        Assert.Equal(oldGraph.Selection.SourceSelectionSha256, Require(original.OpenDeclaredTeachingCorpusIdentities(f.Repository)).Selection.SourceSelectionSha256);
        // Labels are declared namespace keys; a consistent rename is descriptive and has its own SOURCE hash.
        replacement.Sources[0] = replacement.Sources[0] with { SourceId = "copied-label", CorpusId = "unrelated-to-owner-name" };
        for (var i = 0; i < replacement.Items.Length; i++)
            if (replacement.Items[i].SourceId == other.Descriptor.SourceId) replacement.Items[i] = replacement.Items[i] with { SourceId = "copied-label" };
        var labelled = TeachingSelection(d, replacement);
        Assert.Equal("unrelated-to-owner-name", Require(labelled.OpenDeclaredTeachingCorpusIdentities(f.Repository)).Sources[0].Descriptor.CorpusId);
        Assert.NotEqual(next.SourceSelectionSha256, labelled.SourceSelectionSha256); AssertNoEffects(f);
    }

    [Fact]
    public void DeclaredTeachingReadback_201ActualRowsAndAllLinksExceedEvaluationQuotaWithoutTruncation()
    {
        using var f = new Fixture(); var d = f.Seal(0); var a = f.SealTeaching(0, 201); var b = f.SealTeaching(1);
        var c = ActualCurriculum(b, a, everyEntry: true); var selection = TeachingSelection(d, c);
        var before = FixtureBytes(f); var graph = Require(selection.OpenDeclaredTeachingCorpusIdentities(f.Repository));
        Assert.Equal(a.Corpus.Entries.Select(e => e.EntryId), graph.Sources[1].Entries.Select(e => e.EntryId));
        Assert.Equal(201, graph.Sources[1].Entries.Count);
        Assert.Equal(c.Items.Select(i => i.ReferenceEntryId), graph.Items.Select(i => i.Entry.EntryId));
        Assert.Equal(c.Items.Select(i => i.ItemId), graph.CategoryBindings[0].OrderedItemIds);
        foreach (var item in graph.Items) Assert.Same(item.Source.Entries.Single(e => e.EntryId == item.DeclaredItem.ReferenceEntryId), item.Entry);
        AssertFixtureBytes(f, before); AssertNoEffects(f);
    }

    [Fact]
    public void DeclaredTeachingReadback_PreCancelledAndAbsentFailBeforeIoWithRealControl()
    {
        using var f = new Fixture(); var d = f.Seal(0); var a = f.SealTeaching(0); var b = f.SealTeaching(1);
        var selection = TeachingSelection(d, ActualCurriculum(b, a)); var before = FixtureBytes(f);
        using var cancel = new CancellationTokenSource(); cancel.Cancel();
        var observed = 0; var prior = RepositoryOwnedTrainingArtifactCloneCustodyPreparer.RootPreparationObservedForTesting;
        try
        {
            RepositoryOwnedTrainingArtifactCloneCustodyPreparer.RootPreparationObservedForTesting = _ => observed++;
            Assert.Throws<OperationCanceledException>(() => selection.OpenDeclaredTeachingCorpusIdentities("invalid-root", cancel.Token));
            Assert.Equal(0, observed);
            var absent = Require(CodeDocumentationOneLessonSourceSelection.Create([d.Source], [new(d.Row.PairId, d.Source.SourceId)]));
            Assert.Equal(CodeDocumentationOneLessonSourceSelection.InvalidErrorCode, absent.OpenDeclaredTeachingCorpusIdentities("invalid-root").ErrorCode);
            Assert.Equal(0, observed);
            Assert.True(selection.OpenDeclaredTeachingCorpusIdentities(f.Repository).IsSuccess);
            if (OperatingSystem.IsLinux()) Assert.True(observed > 0); else Assert.Equal(0, observed);
        }
        finally { RepositoryOwnedTrainingArtifactCloneCustodyPreparer.RootPreparationObservedForTesting = prior; }
        AssertFixtureBytes(f, before); AssertNoEffects(f);
    }

    [Theory] [InlineData("cancel-first")] [InlineData("tamper-first-reopen")] [InlineData("tamper-last-reopen")]
    public void DeclaredTeachingReadback_FiniteEverySourceRecheckAndMidCancellationUseExistingCustodyHook(string fault)
    {
        using var f = new Fixture(); var d = f.Seal(0); var a = f.SealTeaching(0); var b = f.SealTeaching(1);
        var selection = TeachingSelection(d, ActualCurriculum(b, a));
        Assert.True(selection.OpenDeclaredTeachingCorpusIdentities(f.Repository).IsSuccess);
        var before = FixtureBytes(f); var target = fault == "tamper-last-reopen" ? a : b;
        var bytes = File.ReadAllBytes(target.PayloadPath); var prior = RepositoryOwnedTrainingArtifactCloneCustodyPreparer.RootPreparationObservedForTesting;
        using var cancellation = new CancellationTokenSource(); var opens = 0; var injected = 0;
        try
        {
            RepositoryOwnedTrainingArtifactCloneCustodyPreparer.RootPreparationObservedForTesting = observation =>
            {
                if (observation.RepositoryRelativeRoot != "training-data/reference-corpora") return;
                opens++;
                var trigger = fault == "cancel-first" ? 1 : fault == "tamper-first-reopen" ? 3 : 4;
                if (opens != trigger) return;
                injected++;
                if (fault == "cancel-first") cancellation.Cancel();
                else { var corrupt = bytes.ToArray(); corrupt[^1] ^= 1; File.WriteAllBytes(target.PayloadPath, corrupt); }
            };
            if (OperatingSystem.IsLinux())
            {
                if (fault == "cancel-first") Assert.Throws<OperationCanceledException>(() => selection.OpenDeclaredTeachingCorpusIdentities(f.Repository, cancellation.Token));
                else { var result = selection.OpenDeclaredTeachingCorpusIdentities(f.Repository); Assert.False(result.IsSuccess); Assert.Null(result.Value); }
                Assert.Equal(1, injected); Assert.Equal(fault == "cancel-first" ? 1 : fault == "tamper-first-reopen" ? 3 : 4, opens);
                output.WriteLine("TEACHING_EXISTING_LINUX_CUSTODY_HOOK " + fault + " opens=" + opens + " injected=" + injected);
            }
            else
            {
                // The existing owner has no Windows race hook. Execute real tamper and cancellation controls, without claiming a Windows race.
                Assert.True(selection.OpenDeclaredTeachingCorpusIdentities(f.Repository).IsSuccess); Assert.Equal(0, opens);
                using var pre = new CancellationTokenSource(); pre.Cancel();
                Assert.Throws<OperationCanceledException>(() => selection.OpenDeclaredTeachingCorpusIdentities(f.Repository, pre.Token));
                var corrupt = bytes.ToArray(); corrupt[^1] ^= 1; File.WriteAllBytes(target.PayloadPath, corrupt);
                Assert.False(selection.OpenDeclaredTeachingCorpusIdentities(f.Repository).IsSuccess);
                output.WriteLine("TEACHING_WINDOWS_REAL_TAMPER_PRE_CANCEL; mid-open race NOT_PROVEN; reference DACL NOT_PROVEN");
            }
        }
        finally
        {
            RepositoryOwnedTrainingArtifactCloneCustodyPreparer.RootPreparationObservedForTesting = prior;
            File.WriteAllBytes(target.PayloadPath, bytes);
        }
        Assert.True(selection.OpenDeclaredTeachingCorpusIdentities(f.Repository).IsSuccess);
        AssertFixtureBytes(f, before); AssertNoEffects(f);
    }

    [Fact]
    public void DeclaredTeachingReadback_UnsafeActualAddressKeepsExistingOwnerGuard()
    {
        using var f = new Fixture(); var d = f.Seal(0); var a = f.SealTeaching(0); var b = f.SealTeaching(1);
        var selection = TeachingSelection(d, ActualCurriculum(b, a));
        Assert.True(selection.OpenDeclaredTeachingCorpusIdentities(f.Repository).IsSuccess);
        var before = FixtureBytes(f);
        var result = selection.OpenDeclaredTeachingCorpusIdentities(Path.Combine(f.Repository, ".."));
        Assert.False(result.IsSuccess); Assert.Null(result.Value);
        Assert.True(selection.OpenDeclaredTeachingCorpusIdentities(f.Repository).IsSuccess);
        AssertFixtureBytes(f, before); AssertNoEffects(f);
    }

    internal static IEnumerable<string> ExposedGraphStrings(object root)
    {
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        IEnumerable<string> Walk(object? value)
        {
            if (value is null) yield break;
            Assert.False(value is FrozenReferenceCorpus or FrozenReferenceEntry);
            Assert.DoesNotContain("AuthoringSource", value.GetType().Name); Assert.DoesNotContain("Permit", value.GetType().Name);
            if (value is string text) { yield return text; yield break; }
            if (value.GetType().IsPrimitive || value.GetType().IsEnum || value is DateTimeOffset) yield break;
            if (!visited.Add(value)) yield break;
            if (value is System.Collections.IEnumerable sequence)
            { foreach (var item in sequence) foreach (var s in Walk(item)) yield return s; yield break; }
            foreach (var p in value.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))
            {
                if (p.GetMethod is not { } getter || (!getter.IsPublic && !getter.IsAssembly)) continue;
                Assert.DoesNotContain(p.Name, new[] { "OwnerInput", "PaidModelReferenceAnswer", "Corpus", "Pair", "Lease", "Permission" });
                if (p.GetIndexParameters().Length == 0) foreach (var s in Walk(p.GetValue(value))) yield return s;
            }
        }
        return Walk(root);
    }

    [Theory]
    [InlineData("clean")]
    [InlineData("unused-last")]
    [InlineData("last-source")]
    public void OriginalExclusion_CompleteActualRowsAndLastSourceAreComparedAfterGenuineOwnerPins(string fault)
    {
        using var f = new Fixture(); var dpo = f.Seal(0); var original = f.SealTeaching(99).Corpus;
        var curriculum = GenuineTeaching(f.Repository, original, fault, count: 202);
        var selection = TeachingSelection(dpo, curriculum);
        var identity = Require(selection.OpenDeclaredTeachingCorpusIdentities(f.Repository));
        Assert.Equal(2, identity.Sources.Count); Assert.All(identity.Sources, s => Assert.Equal(202, s.Entries.Count));
        Assert.Equal(curriculum.Sources.Select(s => s.SourceId), identity.Sources.Select(s => s.Descriptor.SourceId));
        Assert.Equal(curriculum.Items.Select(i => i.ItemId), identity.Items.Select(i => i.DeclaredItem.ItemId));
        Assert.Equal(curriculum.CategoryBindings.Select(c => c.Category), identity.CategoryBindings.Select(c => c.Category));
        var before = FixtureBytes(f);
        var inspected = selection.OpenDeclaredTeachingCorpusIdentitiesExcludingOriginal(f.Repository, original, [], []);
        Assert.Equal(fault == "clean", inspected.IsSuccess);
        if (fault != "clean") Assert.Equal("ONE_LESSON_TEACHING_ORIGINAL_OVERLAP", inspected.ErrorCode);
        AssertFixtureBytes(f, before); AssertNoEffects(f); LogRuntimeIdentities();
    }

    [Fact]
    public void SelectionMaterialCandidates_WholeEligibleBytesAndOrder()
    {
        using var f = new Fixture(); var original = f.SealTeaching(99).Corpus;
        var first = f.SealTeaching(7, ownerPrefix: "Полная задача 中文\n" + new string('x', 2200) + "\nTAIL task ",
            paidPrefix: "Полный reference 中文\n" + new string('y', 2300) + "\nTAIL reference ");
        var second = f.SealTeaching(2); var declared = ActualCurriculum(first, second);
        var curriculum = declared with { CategoryBindings = [new("missing_essential_behavior", declared.CategoryBindings[0].OrderedItemIds),
            new("invented_behavior", declared.CategoryBindings[1].OrderedItemIds)] };
        var selection = TeachingSelection(f.Seal(0), curriculum); var before = FixtureBytes(f);
        // The expected full tuples come from the independently retained sealed creation corpus, never the returned batch.
        var expected = curriculum.Items.Select(i => (Item: i, Entry: (i.SourceId == first.Descriptor.SourceId ? first : second)
            .Corpus.Entries.Single(e => e.EntryId == i.ReferenceEntryId))).ToArray();
        var result = Require(selection.OpenTeachingSelectionMaterialsExcludingOriginal(f.Repository,
            ["invented_behavior", "missing_essential_behavior"], original, [], []));
        Assert.Equal(expected.Length, result.Materials.Count);
        Assert.Equal(curriculum.Items.Select(i => i.ItemId), result.Materials.Select(m => m.Item.DeclaredItem.ItemId));
        Assert.False(curriculum.Items.Select(i => i.ItemId).Order(StringComparer.Ordinal).SequenceEqual(result.Materials.Select(m => m.Item.DeclaredItem.ItemId), StringComparer.Ordinal));
        for (var i = 0; i < expected.Length; i++)
        {
            var cell = result.Materials[i]; Assert.Equal(expected[i].Item, cell.Item.DeclaredItem);
            Assert.Equal(expected[i].Entry.OwnerInput, cell.OwnerInput); Assert.Equal(expected[i].Entry.PaidModelReferenceAnswer, cell.PaidModelReferenceAnswer);
            Assert.Equal(expected[i].Entry.PaidReferenceProvenance, cell.PaidReferenceProvenance);
            Assert.Equal(expected[i].Entry.SourceEntityFingerprint, cell.Item.Entry.SourceEntityFingerprint);
        }
        Assert.Contains(result.Materials, m => m.OwnerInput.Length > 2200 && m.OwnerInput.Contains("TAIL task", StringComparison.Ordinal));
        Assert.Contains(result.Materials, m => m.PaidModelReferenceAnswer.Length > 2300 && m.PaidModelReferenceAnswer.Contains("TAIL reference", StringComparison.Ordinal));
        Assert.Equal(curriculum.Sources, result.Graph.Sources.Select(s => s.Descriptor));
        foreach (var binding in curriculum.CategoryBindings)
            Assert.Equal(binding.OrderedItemIds, result.Graph.CategoryBindings.Single(c => c.Category == binding.Category).OrderedItemIds);
        Assert.Equal(selection.SourceSelectionSha256, result.Graph.Selection.SourceSelectionSha256);
        AssertFixtureBytes(f, before); AssertNoEffects(f);
    }

    [Fact]
    public void SelectionMaterialCandidates_UnusedSourceAndOriginalAliasesRefuse()
    {
        using var f = new Fixture(); var original = f.SealTeaching(99).Corpus;
        var first = f.SealTeaching(7); var unused = f.SealTeaching(2); var declared = ActualCurriculum(first, unused);
        // Every declared item stays mapped; only the first category is diagnosed, so the second source remains ineligible but must still reopen.
        CodeDocumentationOneLessonTeachingCurriculumV2 OnlyFirstSourceEligible(CodeDocumentationOneLessonTeachingCurriculumV2 value)
            => value with { CategoryBindings = [
                new("missing_essential_behavior", value.Items.Where(i => i.SourceId == value.Sources[0].SourceId).Select(i => i.ItemId).ToArray()),
                new("invented_behavior", value.Items.Where(i => i.SourceId != value.Sources[0].SourceId).Select(i => i.ItemId).ToArray())] };
        var curriculum = OnlyFirstSourceEligible(declared);
        var selection = TeachingSelection(f.Seal(0), curriculum);
        OperationResult<CodeDocumentationOneLessonSourceSelection.TeachingSelectionMaterialReadback> Open()
            => selection.OpenTeachingSelectionMaterialsExcludingOriginal(f.Repository, ["missing_essential_behavior"], original, [], []);
        var control = Require(Open()); Assert.All(control.Materials, m => Assert.Equal(first.Descriptor.SourceId, m.Item.DeclaredItem.SourceId));
        Assert.Equal(curriculum.Items.Select(i => i.ItemId).Order(StringComparer.Ordinal), curriculum.CategoryBindings.SelectMany(c => c.OrderedItemIds).Order(StringComparer.Ordinal));
        Assert.Equal(2, control.Graph.Sources.Count); var before = FixtureBytes(f); var path = unused.PayloadPath;
        var bytes = File.ReadAllBytes(path); var ticks = File.GetLastWriteTimeUtc(path); var previous = CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting;
        var injected = 0; Exception? primary = null;
        try
        {
            CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting = point =>
            {
                if (point != "raw-staged") return;
                injected++; CodeDocumentationPreparedBaselineTests.RestoreReferenceOwnerFile(path, bytes.Concat(new byte[] { (byte)' ' }).ToArray(), ticks);
            };
            var bad = Open(); output.WriteLine("SELECTION_UNUSED_SOURCE code=" + bad.ErrorCode + " success=" + bad.IsSuccess + " injected=" + injected);
            Assert.False(bad.IsSuccess); Assert.Null(bad.Value);
            Assert.Equal(RepositoryOwnedReferenceCorpusManifestSealer.InvalidErrorCode, bad.ErrorCode); Assert.Equal(1, injected);
        }
        catch (Exception ex) { primary = ex; throw; }
        finally
        {
            CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting = previous;
            CodeDocumentationPreparedBaselineTests.RestoreFixtureAfterFault(() => CodeDocumentationPreparedBaselineTests.RestoreReferenceOwnerFile(path, bytes, ticks), primary);
        }
        Assert.True(Open().IsSuccess); AssertFixtureBytes(f, before); Assert.Equal(ticks, File.GetLastWriteTimeUtc(path));
        var alias = GenuineTeaching(f.Repository, original, "unused-last");
        alias = OnlyFirstSourceEligible(alias);
        var aliasSelection = TeachingSelection(f.Seal(1), alias);
        Assert.True(aliasSelection.OpenDeclaredTeachingCorpusIdentities(f.Repository).IsSuccess);
        var refusal = aliasSelection.OpenTeachingSelectionMaterialsExcludingOriginal(f.Repository, ["missing_essential_behavior"], original, [], []);
        output.WriteLine("SELECTION_UNUSED_ORIGINAL_ALIAS code=" + refusal.ErrorCode + " success=" + refusal.IsSuccess);
        Assert.Equal("ONE_LESSON_TEACHING_ORIGINAL_OVERLAP", refusal.ErrorCode); Assert.Null(refusal.Value);
        Assert.True(Open().IsSuccess); AssertNoEffects(f);
    }

    [Fact]
    public void SelectedTeachingMaterial_GenuineSelectedOwnerPreservesFullUtf8AndPins()
    {
        using var f = new Fixture(); var original = f.SealTeaching(99).Corpus;
        var first = f.SealTeaching(7, ownerPrefix: "Полная задача 中文\nsecond line\t", paidPrefix: "Полный reference 中文\nfinal line\t");
        var unused = f.SealTeaching(2); var curriculum = ActualCurriculum(first, unused);
        var selection = TeachingSelection(f.Seal(0), curriculum); var selected = curriculum.Items[1];
        Assert.NotEqual(curriculum.Items[0].ItemId, selected.ItemId);
        var expected = first.Corpus.Entries.Single(e => e.EntryId == selected.ReferenceEntryId);
        var before = FixtureBytes(f);
        var material = Require(selection.OpenSelectedTeachingMaterialExcludingOriginal(f.Repository,
            selected.ItemId, selected.SourceId, selected.ReferenceEntryId, selected.SourceEntityFingerprint, original, [], []));
        Assert.Equal(expected.OwnerInput, material.OwnerInput); Assert.Equal(expected.PaidModelReferenceAnswer, material.PaidModelReferenceAnswer);
        Assert.Equal(Encoding.UTF8.GetBytes(expected.OwnerInput), material.CopyOwnerInputUtf8());
        Assert.Equal(Encoding.UTF8.GetBytes(expected.PaidModelReferenceAnswer), material.CopyPaidReferenceUtf8());
        var task = material.CopyOwnerInputUtf8(); var reference = material.CopyPaidReferenceUtf8(); task[0] ^= 1; reference[0] ^= 1;
        Assert.Equal(Encoding.UTF8.GetBytes(expected.OwnerInput), material.CopyOwnerInputUtf8());
        Assert.Equal(Encoding.UTF8.GetBytes(expected.PaidModelReferenceAnswer), material.CopyPaidReferenceUtf8());
        Assert.Equal(expected.PaidReferenceProvenance, material.PaidReferenceProvenance);
        Assert.Equal(selected, material.Item.DeclaredItem); Assert.Equal(selection.SourceSelectionSha256, material.Graph.Selection.SourceSelectionSha256);
        Assert.Equal(curriculum.Sources, material.Graph.Sources.Select(s => s.Descriptor));
        Assert.Equal(first.Descriptor, material.Item.Source.Descriptor); Assert.Equal(expected.SourceEntityFingerprint, material.Item.Entry.SourceEntityFingerprint);
        AssertFixtureBytes(f, before); AssertNoEffects(f);
    }

    [Fact]
    public void SelectedTeachingMaterial_ChangedUnusedSourceAndClosedAliasReject()
    {
        using var f = new Fixture(); var original = f.SealTeaching(99).Corpus;
        var first = f.SealTeaching(7); var unused = f.SealTeaching(2); var curriculum = ActualCurriculum(first, unused);
        var selection = TeachingSelection(f.Seal(0), curriculum); var selected = curriculum.Items[1];
        Assert.True(Open(selection).IsSuccess, "genuine non-first selected material must be valid before faults");
        var before = FixtureBytes(f); var path = unused.PayloadPath; var bytes = File.ReadAllBytes(path); var ticks = File.GetLastWriteTimeUtc(path);
        var beforeTicks = before.Keys.ToDictionary(p => p, File.GetLastWriteTimeUtc, StringComparer.Ordinal);
        Exception? primaryFailure = null;
        try
        {
            CodeDocumentationPreparedBaselineTests.RestoreReferenceOwnerFile(path, bytes.Concat(new byte[] { (byte)' ' }).ToArray(), ticks);
            var changed = Open(selection); Assert.False(changed.IsSuccess, "changed unused source must fail complete owner pins");
            Assert.Equal(RepositoryOwnedReferenceCorpusManifestSealer.InvalidErrorCode, changed.ErrorCode);
            output.WriteLine("MATERIAL_UNUSED_SOURCE_FAULT code=" + changed.ErrorCode + " actualBytesChanged=True");
        }
        catch (Exception failure) { primaryFailure = failure; throw; }
        finally { CodeDocumentationPreparedBaselineTests.RestoreFixtureAfterFault(() => CodeDocumentationPreparedBaselineTests.RestoreReferenceOwnerFile(path, bytes, ticks), primaryFailure); }
        Assert.True(Open(selection).IsSuccess, "restored actual unused owner must restore the valid control");
        AssertFixtureBytes(f, before); Assert.Equal(ticks, File.GetLastWriteTimeUtc(path)); AssertNoEffects(f);
        foreach (var file in beforeTicks) Assert.Equal(file.Value, File.GetLastWriteTimeUtc(file.Key));
        var aliasSelection = TeachingSelection(f.Seal(1), GenuineTeaching(f.Repository, original, "unused-last"));
        var alias = aliasSelection.TeachingCurriculum!.Items[1];
        Assert.True(aliasSelection.OpenDeclaredTeachingCorpusIdentities(f.Repository).IsSuccess, "closed alias still has genuine structural pins");
        var result = aliasSelection.OpenSelectedTeachingMaterialExcludingOriginal(f.Repository,
            alias.ItemId, alias.SourceId, alias.ReferenceEntryId, alias.SourceEntityFingerprint, original, [], []);
        Assert.Equal("ONE_LESSON_TEACHING_ORIGINAL_OVERLAP", result.ErrorCode); AssertNoEffects(f);
        OperationResult<CodeDocumentationOneLessonSourceSelection.SelectedTeachingMaterialReadback> Open(CodeDocumentationOneLessonSourceSelection owner)
            => owner.OpenSelectedTeachingMaterialExcludingOriginal(f.Repository, selected.ItemId, selected.SourceId,
                selected.ReferenceEntryId, selected.SourceEntityFingerprint, original, [], []);
    }

    [Fact]
    public void OriginalExclusion_CalculationOnlyDistinctLastRepeatLastCellDefeatsFirstCellOnlyTraversal()
    {
        using var f = new Fixture(); var original = f.SealTeaching(99).Corpus; var dpo = f.Seal(0);
        const string lateAnswer = "distinct last-cell synthetic saved answer";
        var curriculum = GenuineTeaching(f.Repository, original, "saved-answer", savedAnswer: lateAnswer);
        var selection = TeachingSelection(dpo, curriculum);
        Require(selection.OpenDeclaredTeachingCorpusIdentities(f.Repository));
        var first = Sha256(Encoding.UTF8.GetBytes("first synthetic answer"));
        var last = Sha256(Encoding.UTF8.GetBytes(lateAnswer));
        var ids = original.Entries.Take(2).Select(e => e.EntryId).ToArray();
        CodeDocumentationSavedModelAssessmentCalibrationObservation Repeat(string[] hashes)
            => new("calculation-only", 2, 0, 0, first, first, first, ids, hashes, []);
        CodeDocumentationSavedModelAssessmentCaseResult[] cases = [new(ids[0], false, "missing", "", first, 0)];
        // This DTO is only calculation input: no producer authority or Session witness is fabricated.
        Assert.True(selection.OpenDeclaredTeachingCorpusIdentitiesExcludingOriginal(f.Repository, original, cases,
            [Repeat([first, first]), Repeat([first, first])]).IsSuccess);
        var result = selection.OpenDeclaredTeachingCorpusIdentitiesExcludingOriginal(f.Repository, original, cases,
            [Repeat([first, first]), Repeat([first, last])]);
        Assert.Equal("ONE_LESSON_TEACHING_ORIGINAL_OVERLAP", result.ErrorCode);
        Assert.NotEqual(first, last); AssertNoEffects(f);
    }

    [Fact]
    public void OriginalExclusion_GenuineFrozenNfcAndCrLfAliasesPassOwnerThenRejectWholeClosedText()
    {
        foreach (var fault in new[] { "normalized-owner", "normalized-paid" })
        {
            using var f = new Fixture();
            var original = f.SealTeaching(99, ownerPrefix: "OWNER-e\u0301\r\n", paidPrefix: "PAID-é\n").Corpus;
            Assert.Contains("é\n", original.Entries[0].OwnerInput);
            Assert.DoesNotContain("\r", original.Entries[0].OwnerInput);
            Assert.Equal(original.Entries[0].OwnerInput.Normalize(NormalizationForm.FormC), original.Entries[0].OwnerInput);
            var selection = TeachingSelection(f.Seal(0), GenuineTeaching(f.Repository, original, fault));
            Require(selection.OpenDeclaredTeachingCorpusIdentities(f.Repository));
            var before = FixtureBytes(f);
            var result = selection.OpenDeclaredTeachingCorpusIdentitiesExcludingOriginal(f.Repository, original, [], []);
            Assert.Equal("ONE_LESSON_TEACHING_ORIGINAL_OVERLAP", result.ErrorCode); Assert.Null(result.Value);
            AssertFixtureBytes(f, before); AssertNoEffects(f);
        }

        using var rawFixture = new Fixture();
        const string rawOwner = "Document this C# source exactly:\r\npublic static string Value() => \"e\u0301\";\r\n";
        const string rawReference = "Value returns \"e\u0301\".\r\n";
        const string rawOwnerSha = "sha256:d9007d42465d3658a5552e19e2689e0846aafcbbaeffe630e334925a87292407";
        const string rawReferenceSha = "sha256:c7aa4435150e7da7568551426666dad0833ccbd08d13d3ad2968177fb0ee9692";
        var rawProvenance = Require(PaidReferenceProvenance.CreateRawUtf8("paid-provider", "paid-model", "v1",
            "raw-original-reference", DateTimeOffset.UnixEpoch, rawReference));
        var rawEntry = Require(FrozenReferenceEntry.CreateRawCodeDocumentation(rawOwner, rawReference, rawProvenance,
            "raw-csharp", FrozenReferencePartition.HeldOut,
            FrozenReferenceEntry.ComputeNormalizedTextSha256("independent raw ORIGINAL source family")));
        var rawCorpus = Require(FrozenReferenceCorpus.Build("raw-original-corpus", "raw-original-v1",
            DateTimeOffset.UnixEpoch, [rawEntry]));
        var rawAddress = Require(RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(rawFixture.Repository,
            "csharp-code-documentation", "raw-csharp", "raw-original-v1"));
        Require(RepositoryOwnedReferenceCorpusManifestSealer.PersistAndSeal(rawAddress, rawCorpus));
        var rawOriginal = Require(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(rawAddress)).Corpus;
        var reopened = Assert.Single(rawOriginal.Entries);
        Assert.Equal(rawOwner, reopened.OwnerInput); Assert.Equal(rawReference, reopened.PaidModelReferenceAnswer);
        Assert.Equal(rawOwnerSha, FrozenReferenceCanonicalization.RawUtf8Sha256Ref(reopened.OwnerInput));
        Assert.Equal(rawReferenceSha, reopened.PaidReferenceProvenance.ReferenceAnswerRawUtf8Sha256);
        Assert.Empty(reopened.PaidReferenceProvenance.ReferenceAnswerSha256);
        Assert.True(reopened.PaidReferenceProvenance.ClosedPaidReference);
        Assert.True(reopened.PaidReferenceProvenance.PaidReferenceOutputNeverTrainable);
        Assert.True(reopened.PaidReferenceProvenance.PaidReferenceOutputNeverDpoPairSide);
        var rawSelection = TeachingSelection(rawFixture.Seal(0), GenuineTeaching(rawFixture.Repository, rawOriginal));
        var rawBefore = FixtureBytes(rawFixture);
        Require(rawSelection.OpenDeclaredTeachingCorpusIdentitiesExcludingOriginal(rawFixture.Repository, rawOriginal, [], []));
        var rawOverlap = rawSelection.OpenDeclaredTeachingCorpusIdentitiesExcludingOriginal(rawFixture.Repository,
            rawOriginal, [], [], additionalExposedMetadata: [rawReferenceSha]);
        Assert.Equal("ONE_LESSON_TEACHING_ORIGINAL_OVERLAP", rawOverlap.ErrorCode); Assert.Null(rawOverlap.Value);
        AssertFixtureBytes(rawFixture, rawBefore); AssertNoEffects(rawFixture);
    }

    [Theory]
    [InlineData("representative-null-id")]
    [InlineData("repeat-null-id")]
    [InlineData("prefixed-report-digest")]
    public void OriginalExclusion_CalculationRejectsInvalidSavedIdentityAndDistinctDigestGrammarBeforeTeachingIo(string fault)
    {
        using var f = new Fixture(); var original = f.SealTeaching(99).Corpus;
        var selection = TeachingSelection(f.Seal(0), GenuineTeaching(f.Repository, original));
        Require(selection.OpenDeclaredTeachingCorpusIdentities(f.Repository));
        var digest = Sha256(Encoding.UTF8.GetBytes("saved answer")); var id = original.Entries[0].EntryId;
        CodeDocumentationSavedModelAssessmentCaseResult[] cases = [new(fault == "representative-null-id" ? null! : id, false, "missing", "", fault == "prefixed-report-digest" ? "sha256:" + digest : digest, 0)];
        CodeDocumentationSavedModelAssessmentCalibrationObservation[] observations = [new("calculation-only", 1, 0, 0, digest, digest, digest,
            [fault == "repeat-null-id" ? null! : id], [digest], [])];
        var before = FixtureBytes(f);
        var result = selection.OpenDeclaredTeachingCorpusIdentitiesExcludingOriginal(Path.Combine(f.Repository, "missing-root"), original, cases, observations);
        Assert.Equal(CodeDocumentationOneLessonSourceSelection.InvalidErrorCode, result.ErrorCode); Assert.Null(result.Value);
        AssertFixtureBytes(f, before); AssertNoEffects(f);
    }

    [Fact]
    public async Task SelectedStudent_WholeSourceAndOriginalChangesRejectBeforeAndAfterGenerate()
    {
        using(var control=CodeDocumentationPreparedBaselineTests.SelectedStudentHarness())
        {
            var (b,a)=await CodeDocumentationPreparedBaselineTests.BindSelectedStudent(control);
            var received=await CodeDocumentationPreparedBaselineTests.ConsumeSelectedStudent(control,b,a);
            Assert.True(received.IsSuccess,received.ErrorCode+":"+received.ErrorMessage);Assert.Equal("b",received.Value!.StudentResponse);
        }
        foreach(var target in new[] {"original","unused-source"})
        foreach(var stage in new[] {"before","after"})
        {
            using var h=CodeDocumentationPreparedBaselineTests.SelectedStudentHarness();var (b,a)=await CodeDocumentationPreparedBaselineTests.BindSelectedStudent(h);
            Assert.True(CodeDocumentationPreparedBaselineTests.ConsumeSelectedMaterial(h,b,a).IsSuccess);
            var source=h.Grant.SourceSelection!.TeachingCurriculum!.Sources.Single(s=>s.SourceId=="teach-7");
            var address=RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(h.Fixture.RepositoryRoot,source.DomainSlug,source.SubjectSlug,source.CorpusVersion).Value!;
            var path=target=="original"?Path.Combine(h.Fixture.CorpusAddress.CorpusRoot,RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName):
                Path.Combine(address.CorpusRoot,RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName);
            var bytes=File.ReadAllBytes(path);var ticks=File.GetLastWriteTimeUtc(path);var files=h.Files();var history=h.HistoryBytes();
            var writes=0;var entered=0;var returned=0;var final=0;Exception? primary=null;
            void WriteFault(){CodeDocumentationPreparedBaselineTests.RestoreReferenceOwnerFile(path,bytes.Concat(new byte[]{(byte)' '}).ToArray(),ticks);writes++;}
            try
            {
                h.Open.Session.SelectedStudentDurablePointForTesting=point=>
                {
                    if(point=="student-before-generate")entered++;
                    if(point=="student-after-generate"){returned++;if(stage=="after")WriteFault();}
                    if(point=="student-final-owner-checked")final++;
                };
                if(stage=="before")WriteFault();
                var refused=await CodeDocumentationPreparedBaselineTests.ConsumeSelectedStudent(h,b,a);
                Assert.False(refused.IsSuccess,target+"/"+stage+" must reach full owner guard");
                Assert.Equal(RepositoryOwnedReferenceCorpusManifestSealer.InvalidErrorCode,refused.ErrorCode);
                Assert.Equal(1,writes);Assert.Equal(stage=="before"?0:1,entered);Assert.Equal(stage=="before"?0:1,returned);Assert.Equal(1,final);
                Assert.Equal(10,h.Client.Calls);Assert.Equal(history,h.HistoryBytes()[..history.Length]);
                Assert.Equal(stage=="before"?(0,0,false):(1,0,true),h.Open.Session.SelectedStudentAccounting);
                output.WriteLine("SELECTED_STUDENT_OWNER target="+target+" stage="+stage+" code="+refused.ErrorCode+" writes="+writes+" entered="+entered+" returned="+returned);
            }
            catch(Exception ex){primary=ex;throw;}
            finally
            {
                h.Open.Session.SelectedStudentDurablePointForTesting=null;
                CodeDocumentationPreparedBaselineTests.RestoreFixtureAfterFault(()=>CodeDocumentationPreparedBaselineTests.RestoreReferenceOwnerFile(path,bytes,ticks),primary);
            }
            // Exact genuine reference-owner restoration must restore full material validity even when the response effect is conservatively unknown.
            var calibration=b.Readback.Baseline.Calibration;var item=a.Item;
            var restored=h.Grant.SourceSelection.OpenSelectedTeachingMaterialExcludingOriginal(h.Fixture.RepositoryRoot,item.ItemId,item.SourceId,item.ReferenceEntryId,item.SourceEntityFingerprint,
                b.Context.Corpus,calibration.RepresentativeAssessment.Cases,calibration.Calibration.Observations);
            Assert.True(restored.IsSuccess,restored.ErrorMessage);Assert.Equal("Teach branch Y",restored.Value!.OwnerInput);
            Assert.Equal(bytes,File.ReadAllBytes(path));Assert.Equal(ticks,File.GetLastWriteTimeUtc(path));
            if(stage=="before"){h.AssertFiles(files);Assert.Equal("b",(await CodeDocumentationPreparedBaselineTests.ConsumeSelectedStudent(h,b,a)).Value!.StudentResponse);}
            else
            {
                var after=h.Files();Assert.Equal(files.Keys.Order(),after.Keys.Order());
                foreach(var owned in files.Keys.Where(p=>p!=h.Fixture.OrdinaryHistory)){Assert.Equal(files[owned].Bytes,after[owned].Bytes);Assert.Equal(files[owned].Ticks,after[owned].Ticks);}
                Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode,(await CodeDocumentationPreparedBaselineTests.ConsumeSelectedStudent(h,b,a)).ErrorCode);
                Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode,h.Reopen().ErrorCode);
            }
        }
    }

    internal static CodeDocumentationOneLessonTeachingCurriculumV2 GenuineTeaching(string repository,
        FrozenReferenceCorpus original, string fault = "clean", int count = 4, string? savedAnswer = null,
        string? fixtureNamespace = null, Action<RepositoryOwnedReferenceCorpusAddress>? registerOwnedAddress = null)
    {
        var closed = original.Entries[0];
        var rawSelectedContext = fault == "student-task-branches-raw"; // Opt in to a separate all-v2 corpus; default fixtures retain their exact v1 path.
        var selectedBranches = new Dictionary<string, FrozenReferenceEntry>(StringComparer.Ordinal);
        TeachingDataset Seal(int index)
        {
            var entries = Enumerable.Range(0, count).Select(i =>
            {
                var seed = "original-exclusion-" + index + "-" + i;
                var owner = "independent teaching task " + seed; var paid = "independent teaching paid answer " + seed;
                if ((fault == "student-task-branches" || rawSelectedContext) && i == 1) owner = index == 7 ? "Teach branch X" : "Teach branch Y"; // Preserve the existing tiny Student tasks for the isolated raw-reference branch.
                if (rawSelectedContext) paid += "\r\nCafe\u0301\tEND"; // Raw reference bytes visibly differ from NFC/LF normalization.
                if (fault == "material-unicode") { owner = "Полная задача 中文\nsecond line\t" + seed; paid = "Полный reference 中文\nfinal line\t" + seed; }
                var fingerprint = FrozenReferenceEntry.ComputeNormalizedTextSha256("teaching source " + seed);
                var request = "teaching-request-" + seed;
                var changed = index == 7 && i == count - 1;
                if (changed)
                {
                    if (fault is "owner" or "unused-last") owner = closed.OwnerInput;
                    if (fault == "paid") paid = closed.PaidModelReferenceAnswer;
                    if (fault == "normalized-owner") owner = closed.OwnerInput.Normalize(NormalizationForm.FormD).Replace("\n", "\r\n", StringComparison.Ordinal);
                    if (fault == "normalized-paid") paid = closed.PaidModelReferenceAnswer.Normalize(NormalizationForm.FormD).Replace("\n", "\r\n", StringComparison.Ordinal);
                    if (fault == "inclusion") owner = "teaching prefix\n" + closed.OwnerInput.Replace("\n", "\r\n", StringComparison.Ordinal) + "\nteaching suffix";
                    if (fault == "fingerprint") fingerprint = closed.SourceEntityFingerprint;
                    if (fault == "reference") owner = "indirect original ref " + closed.EntryId;
                    if (fault == "saved-answer") owner = savedAnswer!;
                    if (fault == "provenance") request = closed.EntryId;
                    if (fault == "digest-metadata") request = Sha256(Encoding.UTF8.GetBytes(savedAnswer!));
                }
                if (index == 2 && i == count - 1 && fault == "last-source") owner = closed.OwnerInput;
                if (rawSelectedContext)
                {
                    var rawProvenance = Require(PaidReferenceProvenance.CreateRawUtf8("synthetic-paid-provider", "synthetic-paid-model", "v1",
                        request, DateTimeOffset.UnixEpoch, paid));
                    return Require(FrozenReferenceEntry.CreateRawCodeDocumentation(owner, paid, rawProvenance, "learning-unit",
                        i % 2 == 0 ? FrozenReferencePartition.Train : FrozenReferencePartition.HeldOut, fingerprint));
                }
                var provenance = Require(PaidReferenceProvenance.Create("synthetic-paid-provider", "synthetic-paid-model", "v1",
                    request, DateTimeOffset.UnixEpoch, FrozenReferenceEntry.ComputeNormalizedTextSha256(paid)));
                return Require(FrozenReferenceEntry.Create(owner, paid, provenance, "bug", "learning-unit",
                    i % 2 == 0 ? FrozenReferencePartition.Train : FrozenReferencePartition.HeldOut, fingerprint));
            }).ToArray();
            // Private repositories retain the existing fixture addresses; shared-checkout harnesses use their own stable lane.
            var version = (rawSelectedContext ? "raw-selected-context-v" : "exclusion-reference-v") + index + (fixtureNamespace is null ? "" : "-" + fixtureNamespace);
            var corpus = Require(FrozenReferenceCorpus.Build(fault == "manifest" && index == 7 ? closed.EntryId : "independent-teaching-corpus-" + index,
                version, DateTimeOffset.UnixEpoch, entries));
            var address = Require(RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(repository,
                rawSelectedContext ? CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainSlug : "bug", "learning-unit", version));
            registerOwnedAddress?.Invoke(address); // Register the exact validated synthetic path before even a partial seal can create it.
            Require(RepositoryOwnedReferenceCorpusManifestSealer.PersistAndSeal(address, corpus));
            var actual = Require(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedFileIdentity(address));
            if (fault == "student-task-branches" || rawSelectedContext)
            {
                var task = index == 7 ? "Teach branch X" : "Teach branch Y";
                var branch = actual.Corpus.Entries.Single(e => e.Partition == FrozenReferencePartition.HeldOut && e.OwnerInput == task);
                Assert.Equal(Encoding.UTF8.GetBytes(task),Encoding.UTF8.GetBytes(branch.OwnerInput));
                Assert.Equal(entries[1].EntryId,branch.EntryId);Assert.Equal(entries[1].SourceEntityFingerprint,branch.SourceEntityFingerprint);
                Assert.Equal(corpus.CorpusSha256,actual.Corpus.CorpusSha256);
                selectedBranches.Add("item-teach-" + index + "-0",branch);
            }
            var m = actual.Manifest;
            var sourceId = index == 7 && fault == "source-label" ? closed.EntryId : index == 7 && fault == "source-label-bare" ? closed.EntryId[7..] : "teach-" + index;
            return new(address, new(sourceId, "teaching-corpus-" + index, m.Domain, m.Subject, m.CorpusVersion,
                actual.ManifestFileSha256, actual.ManifestFileByteLength, m.CorpusSha256[7..], m.PayloadSha256, m.PayloadByteLength), corpus);
        }
        // The deliberately unsorted source order and unused final rows are preserved through the real owner.
        var curriculum = ActualCurriculum(Seal(7), Seal(2));
        if (fault == "student-task-branches" || rawSelectedContext)
        {
            // Corpus owner sorts by hashed EntryId; generator ordinal1 does not identify the first HeldOut row.
            // Bind the existing synthetic item IDs to the exact reopened branch entries before the grant freezes this graph.
            Assert.Equal(2,selectedBranches.Count);
            curriculum = curriculum with { Items = curriculum.Items.Select(item => selectedBranches.TryGetValue(item.ItemId,out var branch)
                ? item with { ReferenceEntryId = branch.EntryId, SourceEntityFingerprint = branch.SourceEntityFingerprint } : item).ToArray() };
        }
        if (fault == "item-label")
        {
            var old = curriculum.Items[0].ItemId;
            curriculum = curriculum with { Items = curriculum.Items.Select(i => i.ItemId == old ? i with { ItemId = closed.EntryId } : i).ToArray(),
                CategoryBindings = curriculum.CategoryBindings.Select(c => c with { OrderedItemIds = c.OrderedItemIds.Select(id => id == old ? closed.EntryId : id).ToArray() }).ToArray() };
        }
        if (fault == "category-label") curriculum = curriculum with { CategoryBindings = curriculum.CategoryBindings.Select((c, i) => i == 0 ? c with { Category = closed.EntryId } : c).ToArray() };
        return curriculum;
    }

    internal static CodeDocumentationOneLessonTeachingCurriculumV2 GenuineNativePreparation(string repository,
        string fixtureNamespace, Action<RepositoryOwnedReferenceCorpusAddress> registerOwnedAddress, string? expandedTask = null, bool protectedProcessStrings = false)
    {
        Assert.Equal(32, fixtureNamespace.Length); Assert.True(fixtureNamespace.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f'));
        TeachingDataset Seal(int index, int count)
        {
            var entries = Enumerable.Range(0, count).Select(i => {
                var owner = expandedTask is not null && index == 7 && i == 0 ? expandedTask : protectedProcessStrings
                    ? $"Полная задача 中文\nsecond line\toriginal-exclusion-{index}-{i}" : $"中文\nT{index}/{i}\tEND";
                var paid = protectedProcessStrings ? $"Полный reference 中文\nfinal line\toriginal-exclusion-{index}-{i}" : $"中文\nR{index}/{i}\tEND";
                var provenance = Require(PaidReferenceProvenance.Create("fp", "fm", "v1", $"n{index}-{i}", DateTimeOffset.UnixEpoch,
                    FrozenReferenceEntry.ComputeNormalizedTextSha256(paid)));
                return Require(FrozenReferenceEntry.Create(owner, paid, provenance, "bug", "np",
                    i % 2 == 0 ? FrozenReferencePartition.Train : FrozenReferencePartition.HeldOut,
                    FrozenReferenceEntry.ComputeNormalizedTextSha256($"teaching source original-exclusion-{index}-{i}")));
            }).ToArray();
            var version = $"np{index}-" + fixtureNamespace;
            var corpus = Require(FrozenReferenceCorpus.Build("n" + index, version, DateTimeOffset.UnixEpoch, entries));
            var address = Require(RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(repository, "bug", "np", version));
            registerOwnedAddress(address); // Register the exact fresh namespace before the genuine sealer touches it.
            Require(RepositoryOwnedReferenceCorpusManifestSealer.PersistAndSeal(address, corpus));
            var actual = Require(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedFileIdentity(address)); var m = actual.Manifest;
            Assert.Equal(count, actual.Corpus.Entries.Count);
            return new(address, new(index == 7 ? "a" : "b", "c" + index, m.Domain, m.Subject, m.CorpusVersion,
                actual.ManifestFileSha256, actual.ManifestFileByteLength, m.CorpusSha256[7..], m.PayloadSha256, m.PayloadByteLength), actual.Corpus);
        }
        var first = Seal(7, protectedProcessStrings ? 2 : 3); var second = Seal(2, 1);
        var items = new[] { first, second }.SelectMany(s => s.Corpus.Entries.Select((e, i) =>
            new CodeDocumentationOneLessonTeachingItemV2(s.Descriptor.SourceId + i, s.Descriptor.SourceId, e.EntryId, e.SourceEntityFingerprint))).ToArray();
        return new("np", "v2", [first.Descriptor, second.Descriptor], items,
            [new("missing_essential_behavior", items.Where(x => x.SourceId == "a").Select(x => x.ItemId).ToArray()), new("unused", ["b0"])]);
    }

    internal static void WithNativePreparationSealedGraph(Action<CodeDocumentationOneLessonSourceSelection.DeclaredTeachingCorpusReadback,
        IReadOnlyDictionary<string, FrozenReferenceCorpus>> inspect, bool legacyFullMaterial = false, bool protectedProcessStrings = false)
    {
        using var fixture = new Fixture();
        var lane = Guid.NewGuid().ToString("N");
        var training = fixture.Seal(1, domain: "csharp-code-documentation", subject: "ilearning-unit", version: "s0304-" + lane, sourceId: "ordinary-source");
        void Register(RepositoryOwnedReferenceCorpusAddress address)
        {
            Assert.Equal(Path.GetFullPath(fixture.Repository), Path.GetFullPath(address.RepositoryRoot));
            Assert.False(Directory.Exists(address.CorpusRoot));
        }
        var curriculum = legacyFullMaterial
            ? GenuineTeaching(fixture.Repository, fixture.SealTeaching(98).Corpus, "material-unicode", fixtureNamespace: "s0304-" + lane, registerOwnedAddress: Register)
            : GenuineNativePreparation(fixture.Repository, lane, Register, protectedProcessStrings: protectedProcessStrings);
        if (legacyFullMaterial) curriculum = curriculum with { CategoryBindings = [
            new("missing_essential_behavior", curriculum.CategoryBindings[0].OrderedItemIds),
            new("invented_behavior", curriculum.CategoryBindings[1].OrderedItemIds)] };
        var selection = TeachingSelection(training, curriculum);
        var graph = Require(selection.OpenDeclaredTeachingCorpusIdentities(fixture.Repository));
        var corpora = curriculum.Sources.ToDictionary(s => s.SourceId, s => Require(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedFileIdentity(
            Require(RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(fixture.Repository, s.DomainSlug, s.SubjectSlug, s.CorpusVersion)))).Corpus,
            StringComparer.Ordinal);
        inspect(graph, corpora); // Real sealed owner DATA; this helper creates no Session or protected disclosure receipt.
    }

    private static CodeDocumentationOneLessonSourceSelection TeachingSelection(Dataset d, CodeDocumentationOneLessonTeachingCurriculumV2 c)
        => Require(CodeDocumentationOneLessonSourceSelection.Create([d.Source], [new(d.Row.PairId, d.Source.SourceId)], c));
    private static CodeDocumentationOneLessonTeachingCurriculumV2 ActualCurriculum(TeachingDataset first, TeachingDataset second, bool everyEntry = false)
    {
        var items = new[] { first, second }.SelectMany(s => (everyEntry ? s.Corpus.Entries :
                new[] { s.Corpus.Entries.First(e => e.Partition == FrozenReferencePartition.HeldOut),
                    s.Corpus.Entries.First(e => e.Partition == FrozenReferencePartition.Train) })
            .Select((e, i) => new CodeDocumentationOneLessonTeachingItemV2("item-" + s.Descriptor.SourceId + "-" + i,
                s.Descriptor.SourceId, e.EntryId, e.SourceEntityFingerprint))).ToArray();
        return new("unapproved-descriptive-policy", "v2", [first.Descriptor, second.Descriptor], items,
            [new("category-b", items.Select(i => i.ItemId).ToArray()), new("category-a", items.Reverse().Select(i => i.ItemId).ToArray())]);
    }
    private static Dictionary<string, string> FixtureBytes(Fixture f)
        => Directory.GetFiles(f.Repository, "*", SearchOption.AllDirectories).ToDictionary(p => p, p => Sha256(File.ReadAllBytes(p)), StringComparer.Ordinal);
    private static void AssertFixtureBytes(Fixture f, Dictionary<string, string> before)
    {
        var after = FixtureBytes(f); Assert.Equal(before.Keys.Order(StringComparer.Ordinal), after.Keys.Order(StringComparer.Ordinal));
        foreach (var row in before) Assert.Equal(row.Value, after[row.Key]);
    }

    private sealed record TeachingDataset(RepositoryOwnedReferenceCorpusAddress Address,
        CodeDocumentationOneLessonTeachingSourceV2 Descriptor, FrozenReferenceCorpus Corpus)
    { internal string PayloadPath => Path.Combine(Address.CorpusRoot, RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName); }

    private static string Oracle(CodeDocumentationOneLessonSourceSelection selection)
        => "{\"PairSelections\":[" + string.Join(",", selection.PairSelections.Select(p =>
            "{\"SourceId\":\"" + p.SourceId + "\",\"TrainingPairId\":\"" + p.TrainingPairId + "\"}")) +
            "],\"Sources\":[" + string.Join(",", selection.Sources.Select(s =>
                "{\"DatasetVersion\":\"" + s.DatasetVersion + "\",\"DesignPatternSlug\":\"" + s.DesignPatternSlug +
                "\",\"DomainSlug\":\"" + s.DomainSlug + "\",\"ManifestFileByteLength\":" +
                s.ManifestFileByteLength.ToString(CultureInfo.InvariantCulture) + ",\"ManifestFileSha256\":\"" +
                s.ManifestFileSha256 + "\",\"Role\":\"" + s.Role + "\",\"SealedManifestSha256\":\"" +
                s.SealedManifestSha256 + "\",\"SourceId\":\"" + s.SourceId + "\"}")) + "]}";

    private static CodeDocumentationOneLessonSourceV2 ShapeSource()
        => new("source0", "TrainingPairs", "bug", "learning-unit", "v1", Hex('1'), 1, Hex('2'));
    private static CodeDocumentationOneLessonPairSelectionV2 Mapping() => new(Hex('a'), "source0");
    private static CodeDocumentationOneLessonDescriptorsV2 Descriptors(string source, int cases = 1, int repeats = 2)
        => new(2, source, Hex('2'), Hex('3'), Hex('4'), Hex('5'), cases, repeats, Hex('6'), Hex('7'));
    private static CodeDocumentationOneLessonGrantV1 Grant(CodeDocumentationOneLessonSourceSelection selection)
        => new("grant", "v2", "selector", "objective", "source-selection-run", Hex('1'), Hex('2'), Hex('3'), Hex('4'), Hex('5'))
        { FormatVersion = 2, DescriptorsV2 = Descriptors(selection.SourceSelectionSha256), SourceSelection = selection };
    private static string Hex(char value) => new(value, 64);
    private static string Sha256(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static T Require<T>(OperationResult<T> result)
    {
        Assert.True(result.IsSuccess, result.ErrorCode + ": " + result.ErrorMessage);
        Assert.NotNull(result.Value);
        return result.Value!;
    }

    private void LogRuntimeIdentities()
    {
        var test = typeof(CodeDocumentationOneLessonSourceSelectionTests).Assembly;
        foreach (var identity in new[] {
            (Kind: "production-dll", Path: typeof(RepositoryOwnedDpoDatasetManifestSealer).Assembly.Location),
            (Kind: "session-production-dll", Path: typeof(TrainableRouter.Training.CodeDocumentationOneLessonSession).Assembly.Location),
            (Kind: "test-dll", Path: test.Location),
            (Kind: "runtimeconfig", Path: Path.ChangeExtension(test.Location, ".runtimeconfig.json")),
            (Kind: "deps", Path: Path.ChangeExtension(test.Location, ".deps.json")) })
        {
            var bytes = File.ReadAllBytes(identity.Path);
            output.WriteLine("SOURCE_SELECTION_RUNTIME " + JsonSerializer.Serialize(new {
                kind = identity.Kind, path = identity.Path, sha256 = Sha256(bytes), byteLength = bytes.LongLength }));
        }
    }

    private static void AssertNoEffects(Fixture fixture)
    {
        Assert.DoesNotContain(Directory.GetFiles(fixture.Repository, "*", SearchOption.AllDirectories),
            p => p.EndsWith("session.jsonl", StringComparison.Ordinal) ||
                 p.EndsWith(".safetensors", StringComparison.Ordinal) || p.Contains("baseline", StringComparison.Ordinal));
        Assert.False(Directory.Exists(Path.Combine(fixture.Repository, "training-artifacts", "models")));
    }

    private sealed record Dataset(RepositoryOwnedDpoDatasetAddress Address,
        CodeDocumentationOneLessonSourceV2 Source, RepositoryOwnedDpoDatasetManifestPair Row, DpoPair Pair);

    private sealed class Fixture : IDisposable
    {
        private string Root { get; } = Path.Combine(Path.GetTempPath(), "source-selection-" + Guid.NewGuid().ToString("N"));
        internal string Repository { get; }
        internal RepositoryOwnedDpoDatasetRequestIdentity Identity { get; }
        internal Fixture()
        {
            Repository = Path.Combine(Root, "repo"); Directory.CreateDirectory(Repository);
            foreach (var name in new[] { "LlmFromScratch.sln", "llm_mvp_core.sln", "AGENTS.md" })
                File.WriteAllText(Path.Combine(Repository, name), "synthetic fixture sentinel");
            Directory.CreateDirectory(Path.Combine(Repository, "14.ContinuousLearning"));
            Directory.CreateDirectory(Path.Combine(Repository, "11.DPO"));
            Identity = Require(RepositoryOwnedDpoDatasetManifestSealer.CreateRequestIdentity(
                [Sha256(Encoding.UTF8.GetBytes("synthetic-paid-reference"))],
                new(OpenWeightTeacherFamily.Qwen.ToString(), "qwen3.5:9b", Hex('a'),
                    LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind, Hex('1')),
                new("synthetic-generation", "v1", Hex('2')), new("synthetic-creator", "v1", Hex('3'))));
        }
        internal Dataset Seal(int index, DpoPair? reused = null, string domain = "bug", string subject = "learning-unit", string? version = null, string? sourceId = null)
        {
            var address = Require(RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
                Repository, domain, subject, version ?? "v" + index));
            var passport = new TrainingTeacherProvenance(TrainingTeacherSourceKind.ExternalTeacherOutput,
                OpenWeightTeacherFamily.Qwen, TrainingTeacherAccessMode.LocalHostedOpenWeight,
                TrainingTeacherHostingProvider.Ollama, "qwen3.5:9b", Hex('a'), TrainingTeacherModelLicense.Apache20,
                ProviderTermsChecked: false, ProviderTermsAllowTraining: false, ClosedProprietaryModel: false,
                TeacherOutputUsedForTraining: true, Sha256(Encoding.UTF8.GetBytes("synthetic-request-" + index)), InvocationCount: 1)
            { ModelArtifactDigestKind = LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
                ModelArtifactDigest = Hex('1') };
            var pair = reused ?? Require(DpoPairTrainingContentAuthority.Bind(new DpoPair("prompt-" + index,
                "chosen-" + index, "rejected-" + index, domain, DateTimeOffset.Parse("2026-10-03T00:00:00Z", CultureInfo.InvariantCulture))
            { TeacherProvenance = passport, GovernanceDecision = new(true, false, ["sha256:" + Hex('a')]),
                ChosenAuthorship = new(DpoPairSideAuthorKind.OpenTeacher, passport),
                RejectedAuthorship = new(DpoPairSideAuthorKind.LocalModel) }));
            var manifest = Require(RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(address, Identity, [pair]));
            var bytes = File.ReadAllBytes(address.ManifestPath);
            return new(address, new(sourceId ?? "source" + index, "TrainingPairs", domain, subject, version ?? "v" + index,
                Sha256(bytes), bytes.LongLength, manifest.ManifestSha256), Assert.Single(manifest.OrderedPairs), pair);
        }
        internal TeachingDataset SealTeaching(int index, int count = 4, string domain = "bug", string subject = "learning-unit", string? version = null,
            string? ownerPrefix = null, string? paidPrefix = null)
        {
            version ??= "reference-v" + index;
            var entries = Enumerable.Range(0, count).Select(i =>
            {
                var seed = index + "-" + domain + "-" + subject + "-" + version + "-" + i;
                var answer = (paidPrefix ?? "PAID-CLOSED-CANARY-") + seed;
                var provenance = Require(PaidReferenceProvenance.Create("synthetic-paid-provider", "synthetic-paid-model", "v1",
                    "request-" + seed, new(2026, 10, 7, 10, 0, 0, TimeSpan.Zero), FrozenReferenceEntry.ComputeNormalizedTextSha256(answer)));
                return Require(FrozenReferenceEntry.Create((ownerPrefix ?? "OWNER-CLOSED-CANARY-") + seed, answer, provenance, domain, subject,
                    i % 2 == 0 ? FrozenReferencePartition.Train : FrozenReferencePartition.HeldOut,
                    FrozenReferenceEntry.ComputeNormalizedTextSha256("independent-source-" + seed)));
            });
            var corpus = Require(FrozenReferenceCorpus.Build("human-corpus-name-" + index, version, new(2026, 10, 7, 12, 0, 0, TimeSpan.Zero), entries));
            var address = Require(RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(Repository, domain, subject, version));
            var manifest = Require(RepositoryOwnedReferenceCorpusManifestSealer.PersistAndSeal(address, corpus));
            var owner = Require(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedFileIdentity(address));
            Assert.Equal(manifest, owner.Manifest);
            var descriptor = new CodeDocumentationOneLessonTeachingSourceV2("teach-" + index, "declared-corpus-label-" + index,
                manifest.Domain, manifest.Subject, manifest.CorpusVersion, owner.ManifestFileSha256, owner.ManifestFileByteLength,
                manifest.CorpusSha256[7..], manifest.PayloadSha256, manifest.PayloadByteLength);
            return new(address, descriptor, corpus);
        }
        public void Dispose()
        {
            // This is only this test's unique synthetic fixture, never the repository.
            if (Directory.Exists(Root)) Directory.Delete(Root, recursive: true);
        }
    }
}
