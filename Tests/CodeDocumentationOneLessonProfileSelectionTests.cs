// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace LlmFromScratch.Tests;

using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentInfrastructure;
using ContinuousLearning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using ModelServer;
using System.Net.Http.Json;
using TrainableRouter.Training;

public sealed class CodeDocumentationOneLessonProfileSelectionTests(Xunit.Abstractions.ITestOutputHelper output)
{
    private static readonly string[] Units = ["unit-orchestration-plan-v309", "unit-ru-en-general", "unit-dotnet-legacy-doc-l1"];
    private static readonly JsonSerializerOptions JsonOptions = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    private static readonly CodeDocumentationOneLessonSelectorV1 Selector = new("objective", "selector");

    [Fact]
    public void CompleteLiteral_NormalStartupAndFreshOwnerReproduceAllInputsWithoutEffects()
    {
        using var f = new Fixture();
        var expected = Jcs(JsonSerializer.SerializeToNode(f.Expected())!);
        var selected = f.Describe(); var actual = Encoding.UTF8.GetString(CodeDocumentationOneLessonV2Wire.CanonicalProfileSelectionBytes(selected));
        Assert.Equal(expected, actual); // The oracle constructs every row/projection/config/map independently of the production writer.
        Assert.Equal(Hash("xiigen-s04-profileselection-v2\n" + expected), selected.ProfileSelectionSha256);
        Assert.Equal("7ee46ca3fefa902562eef1dc6a7a7f1dbc4f351d6bd61925a34a538bc40dd137", selected.ProfileSelectionSha256); // Independently calculated from the full Python literal preimage.
        Assert.NotEqual(Hash("xiigen-s04-profile-selection-v2\n" + expected), selected.ProfileSelectionSha256);
        var before = f.Files(); var startup = Startup(f.Wire(selected)); var catalog = startup.OneLessonGrantCatalog;
        var grant = Require(catalog.Resolve(Selector)); Assert.Same(grant, Require(catalog.Revalidate("grant", "v2")));
        Assert.NotNull(grant.SourceSelection); Assert.NotNull(grant.CheckpointSelection); Assert.NotNull(grant.CorpusSelection);
        Assert.NotNull(grant.EvaluationPlanSelection); Assert.NotNull(grant.EvaluatorSelection); Assert.NotNull(grant.ProfileSelection);
        Assert.Equal(selected.ProfileSelectionSha256, grant.DescriptorsV2!.ProfileSelectionSha256);
        Assert.Equal(expected, Encoding.UTF8.GetString(CodeDocumentationOneLessonV2Wire.CanonicalProfileSelectionBytes(grant.ProfileSelection!)));
        var opened = Require(f.Batch.OpenProfileInputsFresh(catalog, Selector, f.ModelRoot));
        Assert.True(opened.IsOwnedBy(f.Batch, f.Runner)); Assert.False(opened.IsOwnedBy(new(), f.Runner));
        Assert.Equal(50, opened.TrainingRows.Length); Assert.Equal(50, opened.EvaluationRows.Length);
        Assert.Equal("train49", opened.TrainingRows[49].ExampleId); Assert.Equal("eval49", opened.EvaluationRows[49].ExampleId);
        Assert.Equal("evidence:49", opened.EvaluationExamples[49].Request.KnownCallees[0].EvidenceIds[1]);
        Assert.Equal(expected, Encoding.UTF8.GetString(CodeDocumentationOneLessonV2Wire.CanonicalProfileSelectionBytes(opened.Selection)));
        Assert.Equal(before, f.Files());
        output.WriteLine("PROFILE_FIXTURE " + JsonSerializer.Serialize(new { preimageBytes = Encoding.UTF8.GetByteCount(expected), catalogBytes = Encoding.UTF8.GetByteCount(f.Wire(selected)), sha256 = selected.ProfileSelectionSha256, rows = 100 }));
        foreach (var a in new[] { typeof(CodeDocumentationOneLessonGrantCatalog).Assembly, typeof(CodeDocumentationBatchFileRunner).Assembly, GetType().Assembly })
        {
            var path = a.Location; output.WriteLine("PROFILE_SELECTION_MODULE " + JsonSerializer.Serialize(new { path, sha256 = Sha(File.ReadAllBytes(path)), byteLength = new FileInfo(path).Length,
                utcTicks = File.GetLastWriteTimeUtc(path).Ticks.ToString(CultureInfo.InvariantCulture), mvid = a.ManifestModule.ModuleVersionId.ToString(), informationalVersion = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion }));
        }
    }

    [Fact]
    public async Task SameCatalogMappedEndpoint_RetainsNormalV2RefusalAndCreatesNoHistory()
    {
        using var f = new Fixture(); var selected = f.Describe(); var before = f.Files();
        var startup = Startup(f.Wire(selected)); var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0"); builder.Services.AddSingleton(startup);
        await using var app = builder.Build(); app.MapCodeDocumentationOneLessonPreflight(f.Root);
        var sameStartup = app.Services.GetRequiredService<ContinuousLearningModelServerStartup>();
        Assert.Same(startup, sameStartup); Assert.Same(startup.OneLessonGrantCatalog, sameStartup.OneLessonGrantCatalog);
        Assert.NotNull(Require(sameStartup.OneLessonGrantCatalog.Resolve(Selector)).ProfileSelection);
        await app.StartAsync();
        try
        {
            var addresses = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!;
            using var client = new HttpClient { BaseAddress = new Uri(Assert.Single(addresses.Addresses)) };
            using var response = await client.PostAsJsonAsync(CodeDocumentationOneLessonPreflightWireContract.EndpointPath, new CodeDocumentationOneLessonPreflightRequestV1(Selector));
            Assert.Equal(503, (int)response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<CodeDocumentationOneLessonPreflightResponseV1>();
            Assert.NotNull(body); Assert.False(body.IsSuccess); Assert.Null(body.TerminalOutcome);
            Assert.Equal(CodeDocumentationOneLessonSession.BaselinePrerequisiteUnavailableErrorCode, body.ErrorCode);
            const string objectiveSha = "d454bec656d32723404961023ed9cb3c37221687c40b498a57b3eefa93c4c5f4";
            Assert.Equal(objectiveSha, Hash("objective"));
            var lockPath = Path.Combine(f.ModelRoot.ModelArtifactRoot, "code-documentation-one-lesson-sessions", objectiveSha, ".owner.lock");
            var lockBytes = Encoding.UTF8.GetBytes("s04-one-lesson-session-v2\nd454bec656d32723404961023ed9cb3c37221687c40b498a57b3eefa93c4c5f4\n");
            Assert.Equal(lockBytes, File.ReadAllBytes(lockPath));
            Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(lockPath));
            if (OperatingSystem.IsLinux()) Assert.Equal(SecureLocalFileBoundary.OwnerFileMode, File.GetUnixFileMode(lockPath));
            var permittedLock = Path.GetRelativePath(f.Root, lockPath) + ":" + Sha(lockBytes);
            Assert.Equal(before.Append(permittedLock).Order(StringComparer.Ordinal).ToArray(), f.Files()); // Exact file set preserves both inputs and neighbor; only the literal owner-lock may appear.
            Assert.Equal(lockPath, Assert.Single(Directory.GetFiles(f.ModelRoot.ModelArtifactRoot, "*", SearchOption.AllDirectories))); // No model artifact, receipt, Genesis, or history is created by the refusal.
            Assert.Empty(Directory.GetFiles(f.Root, "session.jsonl", SearchOption.AllDirectories));
        }
        finally { await app.StopAsync(); }
    }

    [Fact]
    public void EveryOriginalTypedStringLeaf_RejectsUnpairedSurrogatesBeforeJsonReplacement()
    {
        using var f = new Fixture(); var before = f.Files(); var baseline = f.Expected();
        var selected = Require(CodeDocumentationOneLessonProfileSelection.Create(baseline));
        var paths = TypedStringLeaves(baseline).ToArray(); var validReplacementControls = 0;
        foreach (var path in paths)
        {
            foreach (var malformed in new[] { "\uD800", "\uDC00" })
            {
                var typed = f.Expected(); SetTypedString(typed, path, malformed);
                var rejected = CodeDocumentationOneLessonProfileSelection.Create(typed);
                Assert.False(rejected.IsSuccess, path); Assert.Null(rejected.Value);
                Assert.Equal(CodeDocumentationOneLessonProfileSelection.InvalidErrorCode, rejected.ErrorCode);
            }
            var replacement = f.Expected(); SetTypedString(replacement, path, "\uFFFD");
            if (!CodeDocumentationOneLessonProfileSelection.Create(replacement).IsSuccess) continue; // Some identities and numeric strings have stricter existing value rules.
            validReplacementControls++;
            var paired = f.Expected(); SetTypedString(paired, path, "\uD83D\uDE80");
            Assert.True(CodeDocumentationOneLessonProfileSelection.Create(paired).IsSuccess, path); // Valid supplementary Unicode remains admitted in every otherwise admissible string position.
        }
        Assert.True(validReplacementControls > 0); // These controls distinguish Unicode rejection from unrelated identity/numeric guards.
        foreach (var malformed in new[] { "\uD800", "\uDC00" })
        {
            var typed = f.Expected(); var context = typed.EvaluationRoutingInputs[^1].Context;
            context[malformed] = context["stack_label"]; context.Remove("stack_label");
            var rejected = CodeDocumentationOneLessonProfileSelection.Create(typed);
            Assert.False(rejected.IsSuccess); Assert.Null(rejected.Value);
            Assert.Equal(CodeDocumentationOneLessonProfileSelection.InvalidErrorCode, rejected.ErrorCode);
        }
        Assert.Equal(selected.ProfileSelectionSha256, Require(CodeDocumentationOneLessonProfileSelection.Create(f.Expected())).ProfileSelectionSha256);
        Assert.Equal(before, f.Files());
        output.WriteLine($"PROFILE_ORIGINAL_UNICODE stringLeaves={paths.Length} validReplacementControls={validReplacementControls}");
    }

    [Fact]
    public void MissingCurrentCandidateMetadata_RejectsCompleteSelectedMaterialBeforeEffects()
    {
        using var f = new Fixture(); var selection = f.Describe(); var before = f.Files(); var catalog = Load(f.Wire(selection));
        Assert.True(f.Batch.OpenProfileInputsFresh(catalog, Selector, f.ModelRoot).IsSuccess); // The same complete catalog/files have a valid control on the original owner.
        var runner = new CodeDocumentationTrainingRunner(new RoutingProfileTrainer(new RoutingUnitMetadataRegistry(f.Metadata.Take(2).ToArray())));
        var missing = new CodeDocumentationBatchFileRunner(runner);
        var reopened = missing.OpenProfileInputsFresh(catalog, Selector, f.ModelRoot);
        Assert.False(reopened.IsSuccess); Assert.Null(reopened.Value);
        Assert.Equal(CodeDocumentationOneLessonProfileSelection.InvalidErrorCode, reopened.ErrorCode);
        var described = missing.DescribeProfileSelection(f.ModelRoot, "input/train.jsonl", "input/eval.jsonl");
        Assert.False(described.IsSuccess); Assert.Null(described.Value);
        Assert.Equal(CodeDocumentationOneLessonProfileSelection.InvalidErrorCode, described.ErrorCode);
        Assert.Equal(before, f.Files());
    }

    [Fact]
    public void EveryIndependentProjectionLeaf_ChangesCommitmentOrRejects()
    {
        using var f = new Fixture(); var selected = f.Describe(); var node = JsonSerializer.SerializeToNode(f.Expected())!;
        foreach (var path in Leaves(node))
        {
            var modified = node.DeepClone(); var original = At(modified, path); var split = path.LastIndexOf('.');
            var parent = At(modified, split < 0 ? "" : path[..split]); var key = path[(split + 1)..];
            var changed = original.GetValueKind() switch
            {
                JsonValueKind.String => JsonValue.Create(original.GetValue<string>() + "x"),
                JsonValueKind.True or JsonValueKind.False => JsonValue.Create(!original.GetValue<bool>()),
                _ => JsonValue.Create(original.GetValue<long>() + 1),
            };
            Set(parent, key, changed); var result = Load(f.WireNode(modified)).Resolve(Selector);
            if (result.IsSuccess) Assert.NotEqual(selected.ProfileSelectionSha256, result.Value!.ProfileSelection!.ProfileSelectionSha256);
            else Assert.Equal(CodeDocumentationOneLessonGrantCatalog.InvalidCatalogErrorCode, result.ErrorCode);
        }
        Assert.Equal(selected.ProfileSelectionSha256, f.Describe().ProfileSelectionSha256);
    }

    [Fact]
    public void EveryClosedObjectAndField_RejectsUnknownDuplicateAliasMissingNullWrongType()
    {
        using var f = new Fixture(); var node = JsonSerializer.SerializeToNode(f.Expected())!;
        foreach (var path in Objects(node))
        {
            var valid = f.WireNode(node); Assert.True(Load(valid).Resolve(Selector).IsSuccess);
            var target = At(node, path).AsObject(); var first = target.First();
            Bad(valid.Replace(Jcs(target), "{" + Q(first.Key) + ":" + Jcs(first.Value!) + "," + Jcs(target)[1..], StringComparison.Ordinal));
            foreach (var fault in new[] { "unknown", "alias" })
            {
                var copy = node.DeepClone(); var o = At(copy, path).AsObject();
                if (fault == "unknown") o["Unknown"] = 0;
                else { var value = o[first.Key]; o.Remove(first.Key); o[first.Key == first.Key.ToLowerInvariant() ? first.Key.ToUpperInvariant() : first.Key.ToLowerInvariant()] = value; }
                Bad(f.WireNode(copy));
            }
            foreach (var field in target.Select(p => p.Key)) foreach (var fault in new[] { "missing", "null", "wrong" })
            {
                var copy = node.DeepClone(); var o = At(copy, path).AsObject();
                if (fault == "missing") o.Remove(field);
                else if (fault == "null") o[field] = null;
                else o[field] = o[field] is JsonObject or JsonArray ? JsonValue.Create("bad") : new JsonObject();
                Bad(f.WireNode(copy));
            }
        }
    }

    [Theory]
    [InlineData("NaN")] [InlineData("Infinity")] [InlineData("-Infinity")] [InlineData("0.20")]
    [InlineData("2e1")] [InlineData("0,2")] [InlineData("+0.2")]
    public void NonCanonicalFloatingValues_RejectBeforeFresh(string text)
    {
        using var f = new Fixture(); var node = JsonSerializer.SerializeToNode(f.Expected())!;
        node["RoutingConfiguration"]!["ScorerBaseConfidence"] = text; Bad(f.WireNode(node));
        node = JsonSerializer.SerializeToNode(f.Expected())!; node["RuntimeArtifactMap"]![2]!["LoraAlpha"] = text; Bad(f.WireNode(node));
    }

    [Fact]
    public void SignedZeroCulturesAndSignedEvaluatorSeeds_PreserveExactDeclaredGrammar()
    {
        using var f = new Fixture(); var expected = f.Expected(); var plus = Require(CodeDocumentationOneLessonProfileSelection.Create(expected));
        var minus = Require(CodeDocumentationOneLessonProfileSelection.Create(expected with { UnitMetadata = expected.UnitMetadata.Select(m => m with { BaselinePrior = "-0" }).ToArray() }));
        Assert.NotEqual(plus.ProfileSelectionSha256, minus.ProfileSelectionSha256);
        var culture = CultureInfo.CurrentCulture;
        try
        {
            foreach (var name in new[] { "en-US", "ru-RU", "tr-TR" })
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(name); Assert.Equal(plus.ProfileSelectionSha256, f.Describe().ProfileSelectionSha256);
                foreach (var seed in new[] { int.MinValue, -1, 0, int.MaxValue })
                    Assert.Equal(seed, Require(Load(f.Wire(plus, seed)).Resolve(Selector)).EvaluatorSelection!.Evaluator.GenerationSettings.Seed);
            }
        }
        finally { CultureInfo.CurrentCulture = culture; }
    }

    [Theory]
    [InlineData("whitespace")] [InlineData("padding")] [InlineData("alphabet")] [InlineData("bits")]
    [InlineData("utf8")] [InlineData("length")] [InlineData("hash")] [InlineData("mixed")]
    [InlineData("direction")] [InlineData("metadata-order")] [InlineData("map-order")] [InlineData("candidate-order")]
    public void CompleteValidWire_TargetedStructuralFaultsFailClosed(string fault)
    {
        using var f = new Fixture(); var node = JsonSerializer.SerializeToNode(f.Expected())!; var a = node["ArtifactMap"]![0]!;
        switch (fault)
        {
            case "whitespace": a["RawUtf8Base64"] = " " + a["RawUtf8Base64"]!.GetValue<string>(); break;
            case "padding": a["RawUtf8Base64"] = a["RawUtf8Base64"]!.GetValue<string>() + "="; break;
            case "alphabet": a["RawUtf8Base64"] = "!" + a["RawUtf8Base64"]!.GetValue<string>()[1..]; break;
            case "bits": SetArtifact(a, [0x7b]); a["RawUtf8Base64"] = "ex=="; break;
            case "utf8": SetArtifact(a, [0xff]); break;
            case "length": a["FileByteLength"] = a["FileByteLength"]!.GetValue<long>() + 1; break;
            case "hash": a["FileSha256"] = new string('f', 64); break;
            case "mixed": Bad(f.WireNode(node).Replace("\"ProfileSelection\":", "\"ProfileSelectionSha256\":\"" + new string('7', 64) + "\",\"ProfileSelection\":", StringComparison.Ordinal)); return;
            case "direction": node["RoutingTrainingInputs"]![0]!["RequiredDirection"] = "direction"; break;
            case "metadata-order": Swap(node["UnitMetadata"]!.AsArray()); break;
            case "map-order": Swap(node["RuntimeArtifactMap"]!.AsArray()); break;
            case "candidate-order": Swap(node["EvaluationRoutingInputs"]![49]!["CandidateUnitIds"]!.AsArray()); break;
        }
        Bad(f.WireNode(node)); Assert.True(Load(f.Wire(f.Describe())).Resolve(Selector).IsSuccess);
    }

    [Theory]
    [InlineData("last-train")] [InlineData("last-eval")] [InlineData("blank")] [InlineData("crlf")]
    [InlineData("bom")] [InlineData("order")] [InlineData("same-length")] [InlineData("missing")]
    [InlineData("replacement")]
    public void FrozenStartup_RejectsEveryChangedRawMaterialAndKeepsNeighbor(string fault)
    {
        using var f = new Fixture(); var catalog = Load(f.Wire(f.Describe())); var neighbor = File.ReadAllBytes(f.Neighbor);
        var file = fault == "last-eval" ? f.Evaluation : f.Training; var bytes = File.ReadAllBytes(file);
        switch (fault)
        {
            case "missing": File.Delete(file); break;
            case "replacement": File.Move(file, file + ".old"); f.Write(file, Encoding.UTF8.GetBytes("replacement")); break;
            case "blank": f.Write(file, bytes.Concat(Encoding.UTF8.GetBytes("\n")).ToArray()); break;
            case "bom": f.Write(file, new byte[] { 0xef, 0xbb, 0xbf }.Concat(bytes).ToArray()); break;
            case "crlf": f.Write(file, Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(bytes).Replace("\n", "\r\n", StringComparison.Ordinal))); break;
            case "order": f.Write(file, Encoding.UTF8.GetBytes(string.Join("\n", Encoding.UTF8.GetString(bytes).Split('\n', StringSplitOptions.RemoveEmptyEntries).Reverse()) + "\n")); break;
            case "same-length": bytes[Array.IndexOf(bytes, (byte)'C')] = (byte)'D'; f.Write(file, bytes); break;
            default: f.Write(file, Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(bytes).Replace("evidence:49", "evidence:changed", StringComparison.Ordinal))); break;
        }
        var result = f.Batch.OpenProfileInputsFresh(catalog, Selector, f.ModelRoot); Assert.False(result.IsSuccess); Assert.Null(result.Value);
        Assert.Equal(CodeDocumentationOneLessonProfileSelection.InvalidErrorCode, result.ErrorCode); Assert.Equal(neighbor, File.ReadAllBytes(f.Neighbor));
    }

    [Theory]
    [InlineData("missing", "ONE_LESSON_PROFILE_SELECTION_INVALID")]
    [InlineData("duplicate", "ONE_LESSON_PROFILE_SELECTION_INVALID")]
    [InlineData("alias", "ONE_LESSON_PROFILE_SELECTION_INVALID")]
    [InlineData("unknown", "ONE_LESSON_PROFILE_SELECTION_INVALID")]
    [InlineData("gate", "ONE_LESSON_PROFILE_SELECTION_INVALID")]
    [InlineData("call-null", "ONE_LESSON_PROFILE_SELECTION_INVALID")]
    [InlineData("callee-null", "ONE_LESSON_PROFILE_SELECTION_INVALID")]
    [InlineData("evidence-null", "ONE_LESSON_PROFILE_SELECTION_INVALID")]
    [InlineData("surrogate", "ONE_LESSON_PROFILE_SELECTION_INVALID")]
    [InlineData("wrong", "MISSING_WRONG_ANNOTATION")]
    [InlineData("equal", "CHOSEN_EQUALS_REJECTED")]
    [InlineData("unit", "ONE_LESSON_PROFILE_SELECTION_INVALID")]
    public void DescriptiveMalformedRawCanLoadButCannotPassRealOwnerGrammar(string fault, string error)
    {
        using var f = new Fixture(); var selection = f.Describe(); var raw = JsonNode.Parse(Fixture.Row("train", 49))!.AsObject();
        switch (fault)
        {
            case "missing": raw.Remove("example_id"); break;
            case "alias": raw["Example_Id"] = raw["example_id"]!.DeepClone(); raw.Remove("example_id"); break;
            case "unknown": raw["unknown"] = true; break;
            case "gate": raw["expect_source_gate_pass"] = null; break;
            case "call-null": raw["calls"]![0] = null; break;
            case "callee-null": raw["known_callees"]![0] = null; break;
            case "evidence-null": raw["known_callees"]![0]!["evidence_ids"]![0] = null; break;
            case "wrong": raw["rejected"] = "wrong"; break;
            case "equal": raw["chosen"] = raw["rejected"]!.DeepClone(); break;
            case "unit": raw["expected_unit_id"] = "unknown-unit"; break;
        }
        var line = Jcs(raw);
        if (fault == "duplicate") line = "{\"example_id\":\"duplicate\"," + line[1..];
        if (fault == "surrogate") line = line.Replace("\"C# code49\"", "\"\\ud800\"", StringComparison.Ordinal);
        var text = string.Join("\n", Enumerable.Range(0, 49).Select(i => Fixture.Row("train", i))) + "\n" + line + "\n";
        var bytes = Encoding.UTF8.GetBytes(text); f.Write(f.Training, bytes);
        var v = selection.Snapshot; v.ArtifactMap[0] = v.ArtifactMap[0] with { RawUtf8Base64 = Convert.ToBase64String(bytes), FileByteLength = bytes.Length, FileSha256 = Sha(bytes) };
        var descriptive = Require(CodeDocumentationOneLessonProfileSelection.Create(v)); var catalog = Load(f.Wire(descriptive));
        Assert.NotNull(Require(catalog.Resolve(Selector)).ProfileSelection); // Startup commits raw spelling without asserting row semantic admission.
        var before = f.Files(); var result = f.Batch.OpenProfileInputsFresh(catalog, Selector, f.ModelRoot);
        Assert.False(result.IsSuccess); Assert.Null(result.Value); Assert.Equal(error, result.ErrorCode); Assert.Equal(before, f.Files());
    }

    [Fact]
    public void EveryRawRowFieldAndNestedMember_UsesClosedGrammarBeforeSameConversion()
    {
        using var f = new Fixture(); var baseline = f.Describe(); var original = JsonNode.Parse(Fixture.Row("train", 49))!;
        void Rejected(JsonNode row)
        {
            var bytes = Encoding.UTF8.GetBytes(string.Join("\n", Enumerable.Range(0, 49).Select(i => Fixture.Row("train", i))) + "\n" + Jcs(row) + "\n");
            f.Write(f.Training, bytes); var v = baseline.Snapshot;
            v.ArtifactMap[0] = v.ArtifactMap[0] with { RawUtf8Base64 = Convert.ToBase64String(bytes), FileByteLength = bytes.Length, FileSha256 = Sha(bytes) };
            var descriptive = Require(CodeDocumentationOneLessonProfileSelection.Create(v)); var result = f.Batch.OpenProfileInputsFresh(Load(f.Wire(descriptive)), Selector, f.ModelRoot);
            Assert.False(result.IsSuccess); Assert.Null(result.Value); Assert.Equal(CodeDocumentationOneLessonProfileSelection.InvalidErrorCode, result.ErrorCode);
        }
        foreach (var field in original.AsObject().Select(p => p.Key))
        {
            var bad = original.DeepClone(); bad[field] = new JsonObject(); Rejected(bad);
            bad = original.DeepClone(); var value = bad[field]; bad.AsObject().Remove(field); bad[field.ToUpperInvariant()] = value; Rejected(bad);
        }
        foreach (var group in new[] { "calls", "known_callees" })
        {
            var fields = original[group]![0]!.AsObject().Select(p => p.Key).ToArray();
            foreach (var field in fields)
            {
                var bad = original.DeepClone(); bad[group]![0]!.AsObject().Remove(field); Rejected(bad);
                bad = original.DeepClone(); bad[group]![0]![field] = null; Rejected(bad);
                bad = original.DeepClone(); bad[group]![0]![field] = new JsonObject(); Rejected(bad);
            }
        }
        f.Write(f.Training, Encoding.UTF8.GetBytes(string.Join("\n", Enumerable.Range(0, 50).Select(i => Fixture.Row("train", i))) + "\n"));
        Assert.Equal(baseline.ProfileSelectionSha256, f.Describe().ProfileSelectionSha256);
    }

    [Fact]
    public void BomCrLfAndBlankLines_AreCompleteByteInputsWithOneLegacyCompatibleLineProjection()
    {
        using var f = new Fixture(); var original = f.Describe();
        foreach (var path in new[] { f.Training, f.Evaluation })
        {
            var text = "\r\n" + Encoding.UTF8.GetString(File.ReadAllBytes(path)).Replace("\n", "\r\n\r\n", StringComparison.Ordinal);
            f.Write(path, new byte[] { 0xef, 0xbb, 0xbf }.Concat(Encoding.UTF8.GetBytes(text)).ToArray());
        }
        var selected = f.Describe(); Assert.NotEqual(original.ProfileSelectionSha256, selected.ProfileSelectionSha256);
        var opened = Require(f.Batch.OpenProfileInputsFresh(Load(f.Wire(selected)), Selector, f.ModelRoot));
        Assert.Equal(50, opened.TrainingRows.Length); Assert.Equal(50, opened.EvaluationRows.Length);
        Assert.Equal(Jcs(JsonSerializer.SerializeToNode(original.Snapshot.RoutingTrainingInputs)!), Jcs(JsonSerializer.SerializeToNode(opened.RoutingTrainingInputs)!));
        Assert.Equal(File.ReadAllBytes(f.Training), Convert.FromBase64String(opened.Selection.Snapshot.ArtifactMap[0].RawUtf8Base64));
    }

    [Fact]
    public void DeepFreeze_AllInputAndReturnedVectorsAndCurrentRegistryAreIndependent()
    {
        using var f = new Fixture(); var input = f.Expected(); var selected = Require(CodeDocumentationOneLessonProfileSelection.Create(input));
        var catalog = Load(f.Wire(selected)); var frozen = Require(catalog.Resolve(Selector)).ProfileSelection!;
        var bytes = CodeDocumentationOneLessonV2Wire.CanonicalProfileSelectionBytes(frozen);
        input.UnitMetadata[2].AllowedInputs[0] = "mutated"; input.RoutingTrainingInputs[49].CandidateUnitIds[0] = "mutated";
        input.EvaluationRoutingInputs[49].Context["stack_label"] = "mutated"; input.RoutingConfiguration.TrainerStopWords[0] = "mutated";
        input.RuntimeArtifactMap[2].LoraTargetModules[0] = "mutated";
        Assert.Equal(bytes, CodeDocumentationOneLessonV2Wire.CanonicalProfileSelectionBytes(frozen));
        var opened = Require(f.Batch.OpenProfileInputsFresh(catalog, Selector, f.ModelRoot));
        opened.TrainingRows[49].KnownCallees![0].EvidenceIds[1] = "mutated";
        opened.EvaluationExamples[49].Request.KnownCallees[0].EvidenceIds[1] = "mutated";
        opened.RoutingTrainingInputs[49].CandidateUnitIds[0] = "mutated";
        opened.EvaluationRoutingInputs[49].Context["stack_label"] = "mutated";
        opened.UnitMetadata[2].AllowedInputs[0] = "mutated";
        Assert.Equal("evidence:49", opened.TrainingRows[49].KnownCallees![0].EvidenceIds[1]);
        Assert.Equal("evidence:49", opened.EvaluationExamples[49].Request.KnownCallees[0].EvidenceIds[1]);
        Assert.Equal(Units, opened.RoutingTrainingInputs[49].CandidateUnitIds); Assert.Equal("stack", opened.EvaluationRoutingInputs[49].Context["stack_label"]);
        Assert.Equal("prompt", opened.UnitMetadata[2].AllowedInputs[0]);
        f.Metadata[2].AllowedInputs[0] = "actual-current-registry-change";
        var bad = f.Batch.OpenProfileInputsFresh(catalog, Selector, f.ModelRoot); Assert.False(bad.IsSuccess); Assert.Null(bad.Value);
        Assert.Equal(bytes, CodeDocumentationOneLessonV2Wire.CanonicalProfileSelectionBytes(frozen));
    }

    [Fact]
    public void RepeatedOverlappingIdsAndDefaults_PreserveFiftyPositionsAndLegacyOwnerPolicy()
    {
        using var f = new Fixture(); var lines = string.Join("\n", Enumerable.Repeat("{\"example_id\":\"same\",\"code_text\":\"code\",\"chosen\":\"yes\",\"rejected\":\"[WRONG: no]\"}", 50)) + "\n";
        f.Write(f.Training, Encoding.UTF8.GetBytes(lines)); f.Write(f.Evaluation, Encoding.UTF8.GetBytes(lines));
        var selection = f.Describe(); var opened = Require(f.Batch.OpenProfileInputsFresh(Load(f.Wire(selection)), Selector, f.ModelRoot));
        Assert.Equal(50, opened.TrainingRows.Count(x => x.ExampleId == "same")); Assert.Equal(50, opened.EvaluationRows.Count(x => x.ExampleId == "same"));
        Assert.All(opened.TrainingExamples, e => { Assert.True(e.ExpectSourceGatePass); Assert.True(e.ExpectVersionGatePass); Assert.True(e.ExpectTransitGatePass);
            Assert.Equal(Units[2], e.ExpectedUnitId); Assert.Empty(e.Request.Calls); Assert.Empty(e.Request.KnownCallees); Assert.Equal("", e.Request.SourceId); });
        Assert.Equal("The row asks for version-aware bottom-up legacy .NET documentation.", opened.RoutingTrainingInputs[49].CorrectReason);
    }

    [Theory]
    [InlineData("../escape.jsonl")] [InlineData("/absolute.jsonl")] [InlineData("input//rows.jsonl")]
    [InlineData("input/CON.jsonl")] [InlineData("input/file. ")] [InlineData("C:/file")]
    [InlineData("input\\rows.jsonl")] [InlineData("input/./rows.jsonl")]
    public void InvalidPathsAndAliases_NeverOpenMaterial(string path)
    {
        using var f = new Fixture(); var before = f.Files(); var result = f.Batch.DescribeProfileSelection(f.ModelRoot, path, "input/eval.jsonl");
        Assert.False(result.IsSuccess); Assert.Null(result.Value); Assert.Equal(before, f.Files());
    }

    [Fact]
    public void ReparseAndWrongOwnerModeAndSamePath_RejectBeforeMaterialAdmission()
    {
        using var f = new Fixture(); Assert.False(f.Batch.DescribeProfileSelection(f.ModelRoot, "input/train.jsonl", "input/train.jsonl").IsSuccess);
        if (OperatingSystem.IsLinux())
        {
            File.SetUnixFileMode(f.Training, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.GroupRead);
            Assert.False(f.Batch.DescribeProfileSelection(f.ModelRoot, "input/train.jsonl", "input/eval.jsonl").IsSuccess);
            File.SetUnixFileMode(f.Training, SecureLocalFileBoundary.OwnerFileMode);
            File.CreateSymbolicLink(Path.Combine(f.Root, "input", "link.jsonl"), f.Training);
            Assert.False(f.Batch.DescribeProfileSelection(f.ModelRoot, "input/link.jsonl", "input/eval.jsonl").IsSuccess);
        }
        Assert.NotNull(f.Describe());
    }

    [Fact]
    public void DepthEightAndCatalogByteCeiling_KeepCompleteControlAndRejectOverflow()
    {
        using var f = new Fixture(); var selected = f.Describe(); var wire = f.Wire(selected);
        using var depth = JsonDocument.Parse(wire, new JsonDocumentOptions { MaxDepth = 8 }); Assert.NotNull(Require(Load(wire).Resolve(Selector)).ProfileSelection);
        var node = JsonSerializer.SerializeToNode(f.Expected())!; node["EvaluationRoutingInputs"]![49]!["Context"]!["stack_label"] = new JsonArray(new JsonArray("extra-depth"));
        Bad(f.WireNode(node));
        var v = f.Expected(); var huge = v.RoutingTrainingInputs.ToArray(); huge[49] = huge[49] with { Prompt = new string('x', 1_048_576) };
        Assert.False(CodeDocumentationOneLessonProfileSelection.Create(v with { RoutingTrainingInputs = huge }).IsSuccess);
        var oversized = wire + new string(' ', 1_048_577 - Encoding.UTF8.GetByteCount(wire)); Assert.Equal(1_048_577, Encoding.UTF8.GetByteCount(oversized)); Bad(oversized);
        var exact = wire + new string(' ', 1_048_576 - Encoding.UTF8.GetByteCount(wire)); Assert.NotNull(Require(Load(exact).Resolve(Selector)).ProfileSelection);
    }

    [Fact]
    public void OpaqueFullAndCompactCompatibilityHaveNoNewMaterialAuthority()
    {
        using var f = new Fixture(); var opaque = Load(OldFullWire()); var grant = Require(opaque.Resolve(Selector)); Assert.Null(grant.ProfileSelection);
        var result = f.Batch.OpenProfileInputsFresh(opaque, Selector, f.ModelRoot); Assert.Null(result.Value); Assert.Equal(CodeDocumentationBatchFileRunner.ProfileMaterialUnavailableErrorCode, result.ErrorCode);
        var tree = JsonNode.Parse(OldFullWire())!; tree["Grants"]![0]!["DescriptorsV2"] = JsonSerializer.SerializeToNode(grant.DescriptorsV2);
        var compact = Load(Jcs(tree)); Assert.Null(Require(compact.Resolve(Selector)).ProfileSelection);
        Assert.Equal(CodeDocumentationBatchFileRunner.ProfileMaterialUnavailableErrorCode, f.Batch.OpenProfileInputsFresh(compact, Selector, f.ModelRoot).ErrorCode);
        var v1 = grant with { FormatVersion = 1, DescriptorsV2 = null, SourceSelection = null, CheckpointSelection = null,
            CorpusSelection = null, EvaluationPlanSelection = null, EvaluatorSelection = null, ProfileSelection = f.Describe() };
        Assert.False(CodeDocumentationOneLessonGrantCatalog.FromTrustedGrants([v1]).Resolve(Selector).IsSuccess);
    }

    private static void SetArtifact(JsonNode node, byte[] bytes)
    { node["RawUtf8Base64"] = Convert.ToBase64String(bytes); node["FileByteLength"] = bytes.LongLength; node["FileSha256"] = Sha(bytes); }
    private static void Swap(JsonArray a) { var first = a[0]!.DeepClone(); a[0] = a[1]!.DeepClone(); a[1] = first; }
    private static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static string Hash(string text) => Sha(Encoding.UTF8.GetBytes(text));
    private static string Q(string s) => JsonSerializer.Serialize(s, JsonOptions);
    private static string Jcs(JsonNode n) => n switch
    { JsonObject o => "{" + string.Join(",", o.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => Q(p.Key) + ":" + (p.Value is null ? "null" : Jcs(p.Value)))) + "}",
      JsonArray a => "[" + string.Join(",", a.Select(v => v is null ? "null" : Jcs(v))) + "]", _ => n.ToJsonString(JsonOptions) };
    private static JsonNode At(JsonNode n, string p) { foreach (var k in p.Split('.', StringSplitOptions.RemoveEmptyEntries)) n = n is JsonArray a ? a[int.Parse(k, CultureInfo.InvariantCulture)]! : n[k]!; return n; }
    private static void Set(JsonNode n, string k, JsonNode? v) { if (n is JsonArray a) a[int.Parse(k, CultureInfo.InvariantCulture)] = v; else n[k] = v; }
    private static IEnumerable<string> TypedStringLeaves(object value, string path = "")
    {
        if (value is string) { yield return path; yield break; }
        if (value is bool or int or long) yield break;
        if (value is Array a)
        { for (var i = 0; i < a.Length; i++) foreach (var leaf in TypedStringLeaves(a.GetValue(i)!, path + "." + i)) yield return leaf; yield break; }
        if (value is Dictionary<string, string> dictionary)
        { foreach (var entry in dictionary) yield return path + "." + entry.Key; yield break; }
        foreach (var property in value.GetType().GetProperties())
            foreach (var leaf in TypedStringLeaves(property.GetValue(value)!, path.Length == 0 ? property.Name : path + "." + property.Name)) yield return leaf;
    }
    private static void SetTypedString(object root, string path, string text)
    {
        var parts = path.Split('.'); var node = root;
        foreach (var part in parts[..^1])
            node = node is Array a ? a.GetValue(int.Parse(part, CultureInfo.InvariantCulture))! : node.GetType().GetProperty(part)!.GetValue(node)!;
        if (node is Array array) array.SetValue(text, int.Parse(parts[^1], CultureInfo.InvariantCulture));
        else if (node is Dictionary<string, string> dictionary) dictionary[parts[^1]] = text;
        else node.GetType().GetProperty(parts[^1])!.SetValue(node, text); // Direct typed mutation preserves the unpaired UTF-16 code unit; no serializer is used to build this negative.
    }
    private static IEnumerable<string> Leaves(JsonNode n, string path = "")
    {
        if (n is JsonObject o) foreach (var p in o) foreach (var l in Leaves(p.Value!, path.Length == 0 ? p.Key : path + "." + p.Key)) yield return l;
        else if (n is JsonArray a) for (var i = 0; i < a.Count; i++) foreach (var l in Leaves(a[i]!, path + "." + i)) yield return l;
        else yield return path;
    }
    private static IEnumerable<string> Objects(JsonNode n, string path = "")
    {
        if (n is JsonObject o) { yield return path; foreach (var p in o) foreach (var l in Objects(p.Value!, path.Length == 0 ? p.Key : path + "." + p.Key)) yield return l; }
        else if (n is JsonArray a) for (var i = 0; i < a.Count; i++) foreach (var l in Objects(a[i]!, path + "." + i)) yield return l;
    }
    private static T Require<T>(OperationResult<T> r) { Assert.True(r.IsSuccess, r.ErrorCode + ":" + r.ErrorMessage); Assert.NotNull(r.Value); return r.Value!; }
    private static ContinuousLearningModelServerStartup Startup(string wire) => ContinuousLearningModelServerStartup.Create(k =>
        k == CodeDocumentationOneLessonGrantCatalog.EnvironmentVariable ? wire : k == "XIIGEN_CONTINUOUS_LEARNING_MAX_TEACHER_CALLS" ? "8" : k == "XIIGEN_CONTINUOUS_LEARNING_COMPOSITION_ENABLED" ? "false" : null);
    private static CodeDocumentationOneLessonGrantCatalog Load(string wire) => Startup(wire).OneLessonGrantCatalog;
    private static void Bad(string wire) => Assert.Equal(CodeDocumentationOneLessonGrantCatalog.InvalidCatalogErrorCode, Load(wire).Resolve(Selector).ErrorCode);
    private static string OldFullWire(int seed = int.MinValue)
    {
        // Reuse the existing independently literal five-selection test fixture; this never calls a production canonical writer.
        var type = typeof(CodeDocumentationOneLessonStartupSelectionTests).GetNestedType("Wire", BindingFlags.NonPublic)!;
        var instance = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single().Invoke([2, 2, 8, 1, seed, "0"]);
        return (string)type.GetProperty("Json", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(instance)!;
    }

    private sealed class Fixture : IDisposable
    {
        internal string Root { get; } = Path.Combine(Path.GetTempPath(), "s04-profile-" + Guid.NewGuid().ToString("N"));
        internal string Training => Path.Combine(Root, "input", "train.jsonl"); internal string Evaluation => Path.Combine(Root, "input", "eval.jsonl");
        internal string Neighbor => Path.Combine(Root, "neighbor"); internal RepositoryOwnedModelArtifactRoot ModelRoot { get; }
        internal UnitRoutingMetadata[] Metadata { get; }
        internal CodeDocumentationTrainingRunner Runner { get; } internal CodeDocumentationBatchFileRunner Batch { get; }
        internal Fixture()
        {
            Directory.CreateDirectory(Root); Directory.CreateDirectory(Path.Combine(Root, "14.ContinuousLearning")); Directory.CreateDirectory(Path.Combine(Root, "11.DPO"));
            Directory.CreateDirectory(Path.Combine(Root, "input")); File.WriteAllText(Path.Combine(Root, "LlmFromScratch.sln"), ""); File.WriteAllText(Neighbor, "preserved");
            ModelRoot = Require(RepositoryOwnedTrainingArtifactLayout.ResolveModelArtifactRoot(Root));
            Metadata = Enumerable.Range(0, 3).Select(i => new UnitRoutingMetadata(Units[i], "role" + i, "domain" + i, ["prompt", "custom:" + i], 0.2, "metadata:" + i, 0.05)).ToArray();
            Runner = new(new RoutingProfileTrainer(new RoutingUnitMetadataRegistry(Metadata))); Batch = new(Runner);
            Write(Training, Encoding.UTF8.GetBytes(string.Join("\n", Enumerable.Range(0, 50).Select(i => Row("train", i))) + "\n"));
            Write(Evaluation, Encoding.UTF8.GetBytes(string.Join("\n", Enumerable.Range(0, 50).Select(i => Row("eval", i))) + "\n"));
        }
        internal void Write(string path, byte[] bytes) { File.WriteAllBytes(path, bytes); if (OperatingSystem.IsLinux()) File.SetUnixFileMode(path, SecureLocalFileBoundary.OwnerFileMode); }
        internal string[] Files() => Directory.GetFiles(Root, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal).Select(p => Path.GetRelativePath(Root, p) + ":" + Sha(File.ReadAllBytes(p))).ToArray();
        internal CodeDocumentationOneLessonProfileSelection Describe() => Require(Batch.DescribeProfileSelection(ModelRoot, "input/train.jsonl", "input/eval.jsonl"));
        internal string Wire(CodeDocumentationOneLessonProfileSelection selection, int seed = int.MinValue) => WireNode(JsonSerializer.SerializeToNode(selection.Snapshot)!, seed);
        internal string WireNode(JsonNode profile, int seed = int.MinValue)
        { var tree = JsonNode.Parse(OldFullWire(seed))!; var d = tree["Grants"]![0]!["DescriptorsV2"]!.AsObject(); d.Remove("ProfileSelectionSha256"); d["ProfileSelection"] = profile.DeepClone(); return Jcs(tree); }
        internal static string Row(string role, int i) => Jcs(JsonSerializer.SerializeToNode(new
        {
            example_id = role + i.ToString(CultureInfo.InvariantCulture), code_text = "C# code" + i, chosen = "chosen:" + i, rejected = "[WRONG: rejected:" + i + "]",
            source_id = "source:" + i, repository = "repository", commit_sha = "commit", file_path = "file.cs", symbol_id = "symbol:" + i,
            app_side = "SERVER", runtime_family = "NET_FRAMEWORK", target_framework = "net48", csharp_language_version = "CSharp7", language_version_confidence = "explicit",
            project_style = "legacy", web_framework = "WebForms", web_framework_version = "4", frontend_framework = "none", frontend_framework_version = "0",
            stack_label = "stack", artifact_kind = "method", documentation_level = "D3", expected_unit_id = Units[2], correct_reason = "reason:" + i,
            wrong_route_consequence = "consequence:" + i, expect_version_gate_pass = true, expect_transit_gate_pass = false, expect_source_gate_pass = true,
            calls = new[] { new { caller_symbol_id = "caller", callee_symbol_id = "callee", call_kind = "direct" } },
            known_callees = new[] { new { symbol_id = "callee", summary = "summary:" + i, evidence_ids = new[] { "evidence:base", "evidence:" + i } } },
        })!);
        internal CodeDocumentationProfilePreimageV2 Expected()
        {
            CodeDocumentationProfileArtifactV2 A(string role, string relative, string path)
            { var b = File.ReadAllBytes(path); return new(role, relative, Sha(b), b.Length, Convert.ToBase64String(b)); }
            string Prompt(int i) => $"Document legacy C# code bottom-up for stack net48 CSharp7. Symbol: symbol:{i}. Level: D3.";
            const string task = "documentation legacy dotnet code documentation bottom-up callee caller propagation SERVER NET_FRAMEWORK net48 CSharp7 WebForms none stack method D3";
            var metadata = Enumerable.Range(0, 3).Select(i => new CodeDocumentationProfileMetadataV2(Units[i], "role" + i, "domain" + i, ["prompt", "custom:" + i], "0.2", "metadata:" + i, "0.05")).ToArray();
            var training = Enumerable.Range(0, 50).Select(i => new CodeDocumentationProfileTrainingV2("train" + i, Prompt(i), task, false, "", true, "documentation", Units.ToArray(), Units[2], Units[0], "reason:" + i, "consequence:" + i)).ToArray();
            var evaluation = Enumerable.Range(0, 50).Select(i => new CodeDocumentationProfileEvaluationV2("eval" + i, Prompt(i), task, false, "", true, "documentation", Units.ToArray(), new(StringComparer.Ordinal)
            { ["app_side"] = "SERVER", ["runtime_family"] = "NET_FRAMEWORK", ["target_framework"] = "net48", ["csharp_language_version"] = "CSharp7", ["project_style"] = "legacy", ["web_framework"] = "WebForms", ["frontend_framework"] = "none", ["documentation_level"] = "D3", ["stack_label"] = "stack" })).ToArray();
            var config = new CodeDocumentationProfileConfigurationV2("the and for with that this into from before after unit model task request prompt please need should which will can must how what when where user system make build create write handle route".Split(' ').Order(StringComparer.Ordinal).ToArray(),
                3, " \t\r\n.,:;/\\-_()[]{}\"'", "C#", "csharp", ".NET", "dotnet", true, "OrdinalIgnoreCase", "OrdinalIgnoreCase", "baseline_candidate_generator",
                "20", "2", "0.99", "0.2", "0.12", "0.02", "0.08", "0.4", "0.05", "trainable-routing-flow-thresholds-v1", "code-documentation-routing-flow", "legacy_dotnet_bottom_up_documentation");
            CodeDocumentationProfileRuntimeV2 Runtime(int i, string role, string domain, string version, string endpoint, string manifest, string branch, string run, string dataset, string tokenizer)
                => new(Units[i], role, domain, version, "https://models.local", endpoint, manifest, branch, run, dataset, tokenizer, 2048, 96, 64, 2, 4, 4, 128, false, "", false, "", 2, "4", "0.05", ["q_proj", "v_proj"]);
            CodeDocumentationProfileRuntimeV2[] map = [Runtime(0, "orchestration-planning-unit", "orchestration", "v309", "/models/unit-orchestration-plan-v309", "manifest-orchestration-v309", "orchestration/v309-plan-routing", "train-ready-v309-v307-plus-v308-plan-head16-full-20260512-004000", "18A52F31385638748D38D559A13EF00A9F89A120F27F7EF4B340BD237E6D2938", "server://tokenizers/orchestration-v309/tokenizer.json"),
                Runtime(1, "translation-unit", "translation", "v1", "/models/unit-ru-en-general", "manifest-translation-ru-en-v1", "translation/ru-en-general", "translation-placeholder-run", "translation-placeholder-dataset", "server://tokenizers/translation-general/tokenizer.json"),
                Runtime(2, "documentation-unit", "documentation", "v1", "/models/unit-dotnet-legacy-doc-l1", "manifest-dotnet-doc-v1", "documentation/dotnet-legacy-l1", "documentation-runner-adapter-v1", "documentation-runner-adapter-dataset", "server://tokenizers/dotnet-doc/tokenizer.json")];
            return new(2, [A("TrainingRows", "input/train.jsonl", Training), A("HeldOutRows", "input/eval.jsonl", Evaluation)], metadata, training, evaluation, config, map);
        }
        public void Dispose()
        { var full = Path.GetFullPath(Root); Assert.StartsWith(Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, full, StringComparison.Ordinal); Assert.StartsWith("s04-profile-", Path.GetFileName(full), StringComparison.Ordinal); Directory.Delete(full, true); }
    }
}
