// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace LlmFromScratch.Tests;

using System.Buffers.Binary;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentInfrastructure;
using ContinuousLearning;
using Microsoft.Extensions.DependencyInjection;
using ModelServer;

[Collection(ModelServerContinuousLearningEnvironmentCollection.Name)]
public sealed class CodeDocumentationOneLessonStartupSelectionTests(Xunit.Abstractions.ITestOutputHelper output)
{
    [Fact]
    public async Task ProtectedTeachingDisclosure_PublicHostAndCallbackCoreRetainSeparateProvenance()
    {
        var json = TeachingDisclosureLiteral();
        using var environment = new TeachingDisclosureProcessEnvironment(json, compositionDisabled: true);
        using var ready = new Sentinel();
        var publicHost = Program.CreateLocalLoopbackHost(["--ready-file", Path.Combine(ready.Root, "public", "ready.json")]);
        Microsoft.AspNetCore.Builder.WebApplication? callback = null;
        try
        {
            var startup = publicHost.Services.GetRequiredService<ContinuousLearningModelServerStartup>();
            Assert.Same(startup, publicHost.Services.GetRequiredService<ContinuousLearningModelServerStartup>());
            var grant = Require(startup.OneLessonGrantCatalog.Resolve(Selector()));
            var approval = Require(startup.ResolveProtectedTeachingDisclosureApproval(grant));
            callback = Program.CreateLocalLoopbackHostCore(["--ready-file", Path.Combine(ready.Root, "callback", "ready.json")], Environment.GetEnvironmentVariable, Program.RepositoryOwnedTrainingTrustedRepositoryRoot);
            var unprotected = callback.Services.GetRequiredService<ContinuousLearningModelServerStartup>();
            var other = Require(unprotected.OneLessonGrantCatalog.Resolve(Selector()));
            Assert.Equal(CodeDocumentationOneLessonV2Wire.CanonicalSourceSelectionBytes(grant.SourceSelection!), CodeDocumentationOneLessonV2Wire.CanonicalSourceSelectionBytes(other.SourceSelection!));
            Assert.Equal(CodeDocumentationOneLessonTeachingDisclosureApproval.Unavailable, unprotected.ResolveProtectedTeachingDisclosureApproval(other).ErrorCode);
            var builder = Program.CreateLocalLoopbackBuilder();
            await using var built = builder.Build();
            var builderStartup = built.Services.GetRequiredService<ContinuousLearningModelServerStartup>();
            Assert.True(builderStartup.ResolveProtectedTeachingDisclosureApproval(Require(builderStartup.OneLessonGrantCatalog.Resolve(Selector()))).IsSuccess);
            Assert.Equal(CodeDocumentationOneLessonTeachingDisclosureApproval.OwnerMismatch, startup.CheckProtectedTeachingDisclosureRevision(Require(builderStartup.ResolveProtectedTeachingDisclosureApproval(Require(builderStartup.OneLessonGrantCatalog.Resolve(Selector()))))).ErrorCode);
            publicHost.Lifetime.StopApplication();
            Assert.Equal(CodeDocumentationOneLessonTeachingDisclosureApproval.Stale, startup.CheckProtectedTeachingDisclosureRevision(approval).ErrorCode);
            Assert.Equal(CodeDocumentationOneLessonTeachingDisclosureApproval.Stale, startup.ResolveProtectedTeachingDisclosureApproval(grant).ErrorCode);
        }
        finally { callback?.Lifetime.StopApplication(); if (callback is not null) await callback.DisposeAsync(); publicHost.Lifetime.StopApplication(); await publicHost.DisposeAsync(); }
        environment.AssertStillConfigured();
    }

    [Fact]
    public void TeachingDisclosureV3_ClosedDecoderRejectsMalformedDuplicateAndMismatchedApprovals()
    {
        var json = TeachingDisclosureLiteral(); Assert.True(Load(json).Resolve(Selector()).IsSuccess);
        Assert.ThrowsAny<JsonException>(() => JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 8 }));
        using (var complete = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 9 })) Assert.Equal(3, complete.RootElement.GetProperty("SchemaVersion").GetInt32());
        foreach (var legacyVersion in new[] { 1, 2 })
        {
            var legacy = JsonNode.Parse(json)!; legacy["SchemaVersion"] = legacyVersion;
            var legacyJson = Jcs(legacy);
            Assert.ThrowsAny<JsonException>(() => JsonDocument.Parse(legacyJson, new JsonDocumentOptions { MaxDepth = 8 }));
            Bad(legacyJson); // The ninth container is never admitted through a non-V3 dispatch.
        }
        foreach (var fault in new[] { "extra", "missing", "wrong-grant", "duplicate", "revision", "future", "source", "unused-entry-policy", "hash-grammar" })
        {
            var root = JsonNode.Parse(json)!; var a = root["TeachingDisclosureApprovals"]![0]!;
            switch (fault)
            {
                case "extra": a["CallbackAuthority"] = true; break;
                case "missing": a.AsObject().Remove("Recipient"); break;
                case "wrong-grant": a["GrantId"] = "foreign"; break;
                case "duplicate": root["TeachingDisclosureApprovals"]!.AsArray().Add(a.DeepClone()); break;
                case "revision": a["ApprovalRevision"] = 0; break;
                case "future": a["ApprovedAtUtc"] = "9999-01-01T00:00:00.0000000Z"; break;
                case "source": a["SourceSelection"]!["TeachingCurriculum"]!["PolicyId"] = "foreign-policy"; break;
                case "unused-entry-policy": a["AllTeachingSources"]![0]!["Entries"]![0]!["Eligibility"]!["LicenseAccepted"] = false; break;
                case "hash-grammar": a["AllTeachingSources"]![0]!["Entries"]![0]!["OwnerInputSha256"] = Hex('a'); break;
            }
            Bad(Jcs(root)); Assert.True(Load(json).Resolve(Selector()).IsSuccess);
        }
        Bad(json.Replace("\"ApprovalRevision\":1", "\"ApprovalRevision\":1,\"ApprovalRevision\":1", StringComparison.Ordinal));
        Bad(json.Replace("\"ApprovalRevision\":1", "\"ApprovalRevision\":1e0", StringComparison.Ordinal));
    }

    private static string TeachingDisclosureLiteral()
    {
        var wire = new Wire(); var v2 = TeachingFullLiteral(wire, null); var root = JsonNode.Parse(v2)!;
        var grant = Good(v2); var sources = new JsonArray();
        foreach (var descriptor in grant.SourceSelection!.TeachingCurriculum!.Sources)
            sources.Add(JsonSerializer.SerializeToNode(new { Descriptor = descriptor, Entries = new[] { new {
                EntryId = "fixture-entry", SourceEntityFingerprint = "sha256:" + Hex('a'), OwnerInputSha256 = "sha256:" + Hex('b'),
                ReferenceAnswerSha256 = "sha256:" + Hex('c'), PaidReferenceProvenanceSha256 = "sha256:" + Hex('d'),
                Eligibility = new { LicenseAccepted = true, IsQuarantined = false, EvidenceRefs = new[] { "sha256:" + Hex('e') } } } } }));
        return AddTeachingDisclosure(root, grant, sources);
    }
    internal static string TeachingDisclosureFixtureLiteral(CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.EndpointTrainingFixture fixture, bool rawUtf8References = false)
    {
        var grant = fixture.OrdinaryGrant; var root = JsonNode.Parse(fixture.ReadEnvironment(CodeDocumentationOneLessonGrantCatalog.EnvironmentVariable)!)!;
        var graph = Require(grant.SourceSelection!.OpenDeclaredTeachingCorpusIdentities(fixture.RepositoryRoot)); var sources = new JsonArray();
        foreach (var source in graph.Sources)
            sources.Add(rawUtf8References ? JsonSerializer.SerializeToNode(new { source.Descriptor, Entries = source.Entries.Select(e => new {
                e.EntryId, e.SourceEntityFingerprint, e.OwnerInputSha256, ReferenceAnswerRawUtf8Sha256 = e.PaidReferenceProvenance.ReferenceAnswerRawUtf8Sha256,
                PaidReferenceProvenanceSha256 = e.PaidReferenceProvenance.ProvenanceSha256,
                Eligibility = new { LicenseAccepted = true, IsQuarantined = false, EvidenceRefs = new[] { "sha256:" + Hex('e') } } }).ToArray() }) : JsonSerializer.SerializeToNode(new { source.Descriptor, Entries = source.Entries.Select(e => new {
                e.EntryId, e.SourceEntityFingerprint, e.OwnerInputSha256, ReferenceAnswerSha256 = e.PaidReferenceProvenance.ReferenceAnswerSha256,
                PaidReferenceProvenanceSha256 = e.PaidReferenceProvenance.ProvenanceSha256,
                Eligibility = new { LicenseAccepted = true, IsQuarantined = false, EvidenceRefs = new[] { "sha256:" + Hex('e') } } }).ToArray() }));
        return AddTeachingDisclosure(root, grant, sources, rawUtf8References ? 2 : 1);
    }
    private static string AddTeachingDisclosure(JsonNode root, CodeDocumentationOneLessonGrantV1 grant, JsonArray sources, int schemaVersion = 1)
    {
        var source = new JsonObject { ["Sources"] = JsonSerializer.SerializeToNode(grant.SourceSelection!.Sources), ["PairSelections"] = JsonSerializer.SerializeToNode(grant.SourceSelection.PairSelections), ["TeachingCurriculum"] = JsonSerializer.SerializeToNode(grant.SourceSelection.TeachingCurriculum) };
        root["SchemaVersion"] = 3;
        root["TeachingDisclosureApprovals"] = new JsonArray(JsonSerializer.SerializeToNode(new { SchemaVersion = schemaVersion, ApprovalId = "synthetic-legal-operator-fixture", ApprovalRevision = 1,
            grant.GrantId, grant.GrantVersion, ApproverId = "fixture-operator", ApprovedAtUtc = "2026-01-01T00:00:00.0000000Z", OperatorDecision = "PermitFullTeachingSourceDisclosure",
            Use = "PedagogicalSelectionContextOnly", Recipient = "ServerOwnedNativeCodexPedagogicalSelection", Model = "gpt-6-astra", ReasoningEffort = "ultra",
            Evidence = new[] { new { EvidenceRef = "test:synthetic-legal-truth", EvidenceSha256 = Hex('f') } }, SourceSelection = source, AllTeachingSources = sources }));
        return Jcs(root);
    }
    internal sealed class TeachingDisclosureProcessEnvironment : IDisposable
    {
        private readonly Dictionary<string, string?> before = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string?> configured = new(StringComparer.Ordinal);
        internal string Json { get; }
        internal TeachingDisclosureProcessEnvironment(string json, CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.EndpointTrainingFixture? fixture = null, bool compositionDisabled = false)
        {
            Json = json;
            if (fixture is not null)
            {
                var values = (Dictionary<string, string?>)typeof(CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.EndpointTrainingFixture).GetField("_environment", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(fixture)!;
                foreach (var p in values) Set(p.Key, p.Value);
            }
            Set(CodeDocumentationOneLessonGrantCatalog.EnvironmentVariable, json); Set(BudgetKey, fixture?.ReadEnvironment(BudgetKey) ?? "8");
            if (compositionDisabled)
            {
                Set("XIIGEN_CONTINUOUS_LEARNING_COMPOSITION_ENABLED", "false"); Set(Program.RepositoryOwnedTrainingEnabledEnvironmentName, "false");
                foreach (var key in new[] { CodeDocumentationSavedModelAssessmentProfileResolver.EnvironmentName, CodeDocumentationCertificationProvisioning.EnvironmentName, ModelServer.AutonomousPromotion.AutonomousPromotionEvaluationConfigurationLoader.ConfigurationEnvironmentName }) Set(key, null);
            }
        }
        private void Set(string key, string? value) { if (!before.ContainsKey(key)) before[key] = Environment.GetEnvironmentVariable(key); configured[key] = value; Environment.SetEnvironmentVariable(key, value); }
        internal void AssertStillConfigured() { foreach (var p in configured) Assert.Equal(p.Value, Environment.GetEnvironmentVariable(p.Key)); }
        public void Dispose() { foreach (var p in before) Environment.SetEnvironmentVariable(p.Key, p.Value); foreach (var p in before) Assert.Equal(p.Value, Environment.GetEnvironmentVariable(p.Key)); }
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public void TeachingCurriculum_BothIndependentFullFormsRetainSourceCommitmentAndMissingTeachingSentinel(bool fullProfile)
    {
        using var sentinel = new Sentinel(); var before = sentinel.Snapshot(); var wire = new Wire();
        using var profileFixture = fullProfile ? CreateExistingProfileFixture() : null;
        var json = TeachingFullLiteral(wire, profileFixture);
        var startup = ContinuousLearningModelServerStartup.Create(name => name == CodeDocumentationOneLessonGrantCatalog.EnvironmentVariable ? json :
            name == BudgetKey ? "8" : name == "XIIGEN_CONTINUOUS_LEARNING_COMPOSITION_ENABLED" ? "false" :
            name.Contains("ROOT", StringComparison.Ordinal) ? sentinel.Missing : null);
        var selected = Require(startup.OneLessonGrantCatalog.Resolve(Selector()));
        var sourceBytes = wire.SourcePreimage[..^1] + ",\"TeachingCurriculum\":" + CodeDocumentationOneLessonSourceSelectionTests.CurriculumLiteral + "}";
        Assert.Equal(DomainHash("xiigen-s04-source-selection-v2\n", sourceBytes), selected.DescriptorsV2!.SourceSelectionSha256);
        Assert.Equal(Encoding.UTF8.GetBytes(sourceBytes), CodeDocumentationOneLessonV2Wire.CanonicalSourceSelectionBytes(selected.SourceSelection!));
        Assert.Equal(CodeDocumentationOneLessonSourceSelectionTests.CurriculumLiteral,
            CodeDocumentationOneLessonSourceSelectionTests.TeachingJson(selected.SourceSelection!.TeachingCurriculum!));
        Assert.Equal(fullProfile, selected.ProfileSelection is not null);
        Assert.Equal(before, sentinel.Snapshot()); Assert.False(Directory.Exists(sentinel.Missing));
        AssertWholeLiteral(wire, Good(wire.Json)); // Original absent branch is still independently literal.
    }

    // Use the existing actual batch-owner fixture's independent Expected preimage, never the production canonical writer.
    private static IDisposable CreateExistingProfileFixture()
        => (IDisposable)Activator.CreateInstance(typeof(CodeDocumentationOneLessonProfileSelectionTests)
            .GetNestedType("Fixture", BindingFlags.NonPublic)!, nonPublic: true)!;
    private static string TeachingFullLiteral(Wire wire, IDisposable? profileFixture = null)
    {
        var tree = JsonNode.Parse(wire.Json)!; var descriptor = At(tree, "Grants.0.DescriptorsV2").AsObject();
        descriptor["TeachingCurriculum"] = JsonNode.Parse(CodeDocumentationOneLessonSourceSelectionTests.CurriculumLiteral);
        if (profileFixture is not null)
        {
            descriptor.Remove("ProfileSelectionSha256");
            var expected = profileFixture.GetType().GetMethod("Expected", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(profileFixture, null);
            descriptor["ProfileSelection"] = JsonSerializer.SerializeToNode((CodeDocumentationProfilePreimageV2)expected!);
        }
        return Jcs(tree);
    }

    public static IEnumerable<object[]> TeachingObjects()
    {
        foreach (var p in new[] { "", "Sources.0", "Items.0", "CategoryBindings.0" })
            foreach (var fault in new[] { "extra", "duplicate", "null", "missing" }) yield return [p, fault];
    }
    [Theory] [MemberData(nameof(TeachingObjects))]
    public void TeachingCurriculum_FullDecoderRejectsEveryAmbiguousObjectAndRestoresLiteral(string path, string fault)
    {
        var json = TeachingFullLiteral(new Wire()); var tree = JsonNode.Parse(json)!;
        var curriculum = At(tree, "Grants.0.DescriptorsV2.TeachingCurriculum"); var node = At(curriculum, path).AsObject();
        var first = node.First();
        if (fault == "extra") node["Unknown"] = 1;
        else if (fault == "null") node[first.Key] = null;
        else if (fault == "missing") node.Remove(first.Key);
        else
        {
            var raw = Jcs(node); var duplicate = raw.Insert(1, Q(first.Key) + ":" + Jcs(first.Value!) + ",");
            Bad(json.Replace(raw, duplicate, StringComparison.Ordinal)); Good(json); return;
        }
        Bad(Jcs(tree)); Good(json);
    }

    [Theory] [InlineData("null")] [InlineData("empty")] [InlineData("numeric-alias")]
    [InlineData("depth")] [InlineData("both-profiles")] [InlineData("compact")] [InlineData("noncanonical")]
    public void TeachingCurriculum_NewFullShapePreservesClosedWireRules(string fault)
    {
        var good = TeachingFullLiteral(new Wire()); var tree = JsonNode.Parse(good)!; var d = At(tree, "Grants.0.DescriptorsV2");
        switch (fault)
        {
            case "null": d["TeachingCurriculum"] = null; break;
            case "empty": d["TeachingCurriculum"] = new JsonObject(); break;
            case "numeric-alias": Bad(good.Replace("\"ManifestFileByteLength\":11", "\"ManifestFileByteLength\":1.1e1", StringComparison.Ordinal)); Good(good); return;
            case "depth": d["TeachingCurriculum"]!["CategoryBindings"]![0]!["OrderedItemIds"] = JsonNode.Parse("[[[[[[[[\"item-b\"]]]]]]]]"); break;
            case "both-profiles": d["ProfileSelection"] = new JsonObject(); break;
            case "compact":
                var compact = JsonSerializer.SerializeToNode(Good(good).DescriptorsV2)!; compact["TeachingCurriculum"] = d["TeachingCurriculum"]!.DeepClone();
                At(tree, "Grants.0")["DescriptorsV2"] = compact; break;
            default: Bad(good.Replace("\"TeachingCurriculum\":", "\"TeachingCurriculum\": ", StringComparison.Ordinal)); Good(good); return;
        }
        Bad(Jcs(tree)); Good(good);
    }

    [Fact]
    public void NormalDisabledStartup_LoadsFiveFrozenSelectionsFromIndependentLiteralWithoutRootIo()
    {
        using var sentinel = new Sentinel(); var wire = new Wire(); var reads = new List<string>();
        var before = sentinel.Snapshot();
        var startup = ContinuousLearningModelServerStartup.Create(name =>
        {
            reads.Add(name);
            if (name == CodeDocumentationOneLessonGrantCatalog.EnvironmentVariable) return wire.Json;
            if (name == BudgetKey) return "8";
            if (name == "XIIGEN_CONTINUOUS_LEARNING_COMPOSITION_ENABLED") return "false";
            return name.Contains("ROOT", StringComparison.Ordinal) ? sentinel.Missing : null;
        });
        var selected = Require(startup.OneLessonGrantCatalog.Resolve(Selector()));
        Assert.Equal(CodeDocumentationOneLessonGrantCatalog.EnvironmentVariable, reads[0]);
        Assert.Equal(8, startup.OneLessonGrantCatalog.MaxTeacherCalls);
        AssertWholeLiteral(wire, selected);
        Assert.Equal(before, sentinel.Snapshot()); Assert.False(Directory.Exists(sentinel.Missing));
        Assert.Equal(selected, Require(startup.OneLessonGrantCatalog.Revalidate("grant", "v2")));
        var tests = typeof(CodeDocumentationOneLessonStartupSelectionTests).Assembly;
        foreach (var item in new[] { (Kind: "production-dll", Path: typeof(CodeDocumentationOneLessonGrantCatalog).Assembly.Location),
            (Kind: "test-dll", Path: tests.Location), (Kind: "runtimeconfig", Path: Path.ChangeExtension(tests.Location, ".runtimeconfig.json")),
            (Kind: "deps", Path: Path.ChangeExtension(tests.Location, ".deps.json")) })
        {
            var bytes = File.ReadAllBytes(item.Path);
            output.WriteLine("STARTUP_SELECTION_RUNTIME " + JsonSerializer.Serialize(new { kind = item.Kind, path = item.Path, sha256 = Sha(bytes), byteLength = bytes.LongLength }));
        }
    }

    [Theory]
    [InlineData(int.MinValue)] [InlineData(-1)] [InlineData(0)] [InlineData(int.MaxValue)]
    public void SignedGenerationSeed_AllInt32ValuesUseTypedFullCanonicalPath(int seed)
    {
        var wire = new Wire(seed: seed); var selected = Good(wire.Json); AssertWholeLiteral(wire, selected);
        Assert.Equal(seed, selected.EvaluatorSelection!.Evaluator.GenerationSettings.Seed);
        Assert.Equal(0, selected.EvaluatorSelection.Evaluator.EvaluationProfile.Seed);
    }

    [Fact]
    public void NegativeZeroAndCultures_PreserveExactFullBytesAndCommitments()
    {
        var plus = new Wire(temperature: "0"); var minus = new Wire(temperature: "-0");
        Assert.NotEqual(Good(plus.Json).DescriptorsV2!.EvaluatorProfileSha256, Good(minus.Json).DescriptorsV2!.EvaluatorProfileSha256);
        var culture = CultureInfo.CurrentCulture; var ui = CultureInfo.CurrentUICulture;
        try
        {
            foreach (var name in new[] { "en-US", "ru-RU", "tr-TR" })
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(name); CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;
                AssertWholeLiteral(minus, Good(minus.Json));
            }
        }
        finally { CultureInfo.CurrentCulture = culture; CultureInfo.CurrentUICulture = ui; }
        Assert.Equal(LocalQwenOllamaInferenceProfileAuthority.Evaluation.ConfigurationSha256, ProfileHash());
    }

    public static IEnumerable<object[]> RequiredFieldFaults()
    {
        foreach (var (path, fields) in Schema)
            foreach (var field in fields.Split(','))
                foreach (var kind in new[] { "missing", "null", "wrong-type" }) yield return [path, field, kind];
    }

    [Theory] [MemberData(nameof(RequiredFieldFaults))]
    public void EveryIndependentRequiredField_RejectsMissingNullAndWrongTypeThenRestores(string path, string field, string fault)
    {
        var wire = new Wire(); Good(wire.Json); var json = JsonNode.Parse(wire.Json)!;
        var target = At(json, path).AsObject();
        if (fault == "missing") target.Remove(field);
        else if (fault == "null") target[field] = null;
        else target[field] = WrongType(target[field]!);
        Bad(Jcs(json)); AssertWholeLiteral(wire, Good(wire.Json));
    }

    public static IEnumerable<object[]> ObjectFaults()
    {
        foreach (var (path, _) in Schema)
            foreach (var kind in new[] { "duplicate", "case-alias", "unknown" }) yield return [path, kind];
    }

    [Theory] [MemberData(nameof(ObjectFaults))]
    public void EveryObjectLevel_RawDuplicateAliasUnknownRejectsThenRestores(string path, string fault)
    {
        var wire = new Wire(); var tree = JsonNode.Parse(wire.Json)!; var target = At(tree, path).AsObject();
        var key = target.First().Key; var original = Jcs(target); string corrupt;
        if (fault == "duplicate") corrupt = Jcs(tree).Replace(original, "{" + Q(key) + ":" + Jcs(target[key]!) + "," + original[1..], StringComparison.Ordinal);
        else
        {
            if (fault == "case-alias") { var value = target[key]; target.Remove(key); target[key.ToLowerInvariant()] = value; }
            else target["Unknown"] = 1;
            corrupt = Jcs(tree);
        }
        Assert.NotEqual(wire.Json, corrupt); Bad(corrupt); Good(wire.Json);
    }

    [Theory]
    [InlineData("2147483648")] [InlineData("-2147483649")] [InlineData("1.5")]
    [InlineData("1e0")] [InlineData("-0")] [InlineData("null")] [InlineData("true")] [InlineData("\"1\"")]
    public void RawGenerationSeed_InvalidInputRejectsWithoutPrivateWriterTestingHook(string raw)
    {
        var wire = new Wire(seed: 0);
        Bad(wire.Json.Replace("\"MaxNewTokens\":1,\"Seed\":0", "\"MaxNewTokens\":1,\"Seed\":" + raw, StringComparison.Ordinal)); Good(wire.Json);
    }

    [Theory]
    [InlineData(int.MinValue)] [InlineData(-1)] [InlineData(int.MaxValue)]
    public void ProfileSeed_IsFixedActualOwnerValueRatherThanGenericSeedRange(int seed)
        => MutateBad("Grants.0.DescriptorsV2.Evaluator.EvaluationProfile.Seed", JsonValue.Create(seed)!);

    public static IEnumerable<object[]> OwnerFaults()
    {
        const string d = "Grants.0.DescriptorsV2."; const string e = d + "Evaluator.";
        foreach (var row in new (string Path, string Value)[] {
            (d+"Sources.0.SourceId","\"_bad\""),(d+"Sources.0.Role","\"trainingpairs\""),(d+"Sources.0.DomainSlug","\"Bug\""),
            (d+"Sources.0.ManifestFileByteLength","0"),(d+"Sources.0.ManifestFileByteLength","8388609"),(d+"Sources.0.SealedManifestSha256","\"ABC\""),
            (d+"PairSelections.0.SourceId","\"missing\""),(d+"PairSelections.0.TrainingPairId","\"sha256:bad\""),
            (d+"ParentCheckpoint.Step","0"),(d+"ParentCheckpoint.WeightsByteLength","0"),(d+"ParentCheckpoint.WeightsByteLength","17179869185"),
            (d+"Candidate.CandidateId","\" bad\""),(d+"Corpus.Purpose","\"Training\""),(d+"Corpus.SubjectSlug","\"../escape\""),
            (d+"Corpus.ManifestFileByteLength","0"),(d+"Corpus.ManifestFileByteLength","2097153"),(d+"Corpus.PayloadByteLength","0"),(d+"Corpus.PayloadByteLength","33554433"),
            (d+"EvaluationPlan.PlanId","\"_bad\""),(d+"EvaluationPlan.CorpusId","\"different\""),(d+"EvaluationPlan.RepeatCount","1"),
            (d+"EvaluationPlan.ExpectedEvaluationCallCount","3"),(d+"EvaluationPlan.PlanSha256",Q(Hex('f'))),
            (d+"ProfileSelectionSha256","\"ABC\""),(e+"EvaluatorId","\"different\""),(e+"Version",Q(new('v',65))),
            (e+"GenerationSettings.MaxNewTokens","0"),(e+"GenerationSettings.MaxNewTokens","2049"),(e+"GenerationSettings.TopK","10001"),
            (e+"GenerationSettings.MaxHeldOutCases","1"),(e+"GenerationSettings.MaxHeldOutCases","201"),(e+"GenerationSettings.MaxAnswerChars","0"),(e+"GenerationSettings.MaxAnswerChars","65537"),
            (e+"GenerationSettings.Temperature","\"-0.1\""),(e+"GenerationSettings.Temperature","\"2.1\""),(e+"GenerationSettings.Temperature","\"0.0\""),
            (e+"GenerationSettings.Temperature","\"NaN\""),(e+"GenerationSettings.Temperature","0"),(e+"GenerationSettings.TopP","\"0\""),(e+"GenerationSettings.TopP","\"1.1\""),
            (e+"NoisePolicy.RepeatCount","3"),(e+"NoisePolicy.StandardDeviationMultiplier","\"0\""),(e+"NoisePolicy.StandardDeviationMultiplier","\"Infinity\""),
            (e+"NoisePolicy.StandardDeviationMultiplier","\"1.250\""),(e+"NoisePolicyIdentitySha256",Q(Hex('f'))),
            (e+"EvaluationProfile.Think","true"),(e+"EvaluationProfile.NumPredict","257"),(e+"EvaluationProfile.ProfileId","\"other\""),
            (e+"EvaluationProfile.RequestTimeoutSeconds","\"121\""),(e+"EvaluationProfile.ConfigurationSha256",Q(Hex('f'))),
            (e+"ScorerIdentity.DomainTag","\"bug\""),(e+"ScorerIdentity.ScorerVersion",Q(Hex('f'))),(e+"ScorerIdentity.MetricProfileId","\"other\""),
            (e+"CurrentConfiguration.ModelId","\"unowned-model:9b\""),(e+"CurrentConfiguration.LocalRetentionPolicyRef","\"bare\""),
            (e+"CurrentConfiguration.MaxTeacherCalls","0"),(e+"CurrentConfiguration.MaxTeacherCalls","10001"),(e+"CurrentConfiguration.DpoMaxStepCount","0"),
            ("Grants.0.CandidateSha256",Q(Hex('f'))),("Grants.0.ParentManifestSha256",Q(Hex('f'))),("Grants.0.ParentWeightsSha256",Q(Hex('f'))),
            ("Grants.0.ConfigurationSha256",Q(Hex('f'))),("Grants.0.ExamConditionSha256",Q(Hex('f'))) }) yield return [row.Path,row.Value];
    }

    [Theory] [MemberData(nameof(OwnerFaults))]
    public void ActualOwnerBoundsAndScalarBindings_RejectOneFaultRestoreControl(string path, string raw)
        => MutateBad(path, JsonNode.Parse(raw)!);

    [Theory]
    [InlineData(1, 5000, 10000)] [InlineData(200, 25, 10000)]
    public void CompleteHardCap_AdmitsAllCasesAndExactDerivedArithmetic(int count, int repeats, int budget)
    {
        var wire = new Wire(count, repeats, budget); var grant = Good(wire.Json, budget); AssertWholeLiteral(wire, grant);
        Assert.Equal(5000, grant.EvaluationPlanSelection!.ExpectedEvaluationCallCount);
        Assert.Equal(count, grant.CorpusSelection!.CaseIds.Count); Assert.Equal(wire.Cases.Last(), grant.EvaluationPlanSelection.CaseIds.Last());
    }

    [Theory]
    [InlineData(1, 5001)] [InlineData(200, 26)] [InlineData(200, int.MaxValue)]
    public void CheckedCompleteArithmetic_RejectsOverCapOrOverflow(int count, int repeats)
    {
        var wire = new Wire(count, repeats, 10000); var node = JsonNode.Parse(wire.Json)!;
        if (repeats == int.MaxValue) { At(node, "Grants.0.DescriptorsV2.EvaluationPlan")["ExpectedEvaluationCallCount"] = 1; RefreshPlanHash(node); }
        Bad(Jcs(node),10000); Good(new Wire().Json);
    }

    [Theory]
    [InlineData("7")] [InlineData("0")] [InlineData("10001")] [InlineData("-1")]
    [InlineData("8.0")] [InlineData("")] [InlineData("2147483648")]
    public void SameProtectedBudget_RejectsMismatchOrCurrentParserFailure(string budget)
    {
        var wire = new Wire(); Assert.Equal(CodeDocumentationOneLessonGrantCatalog.InvalidCatalogErrorCode, Load(wire.Json,budget).Resolve(Selector()).ErrorCode); Good(wire.Json);
    }

    [Fact]
    public void CompleteSourcesPairsAndCases_RejectInvalidVectorsAndRetainLastElements()
    {
        var wire = new Wire(count:200,sources:8,budget:10000); var good = Good(wire.Json,10000); AssertWholeLiteral(wire,good);
        foreach(var path in new[]{"Sources","PairSelections","EvaluationPlan.CaseIds"})
        {
            var tree=JsonNode.Parse(wire.Json)!; At(tree,"Grants.0.DescriptorsV2."+path).AsArray().Clear(); Bad(Jcs(tree),10000);
        }
        foreach(var path in new[]{"Sources","PairSelections","EvaluationPlan.CaseIds"})
        {
            var tree=JsonNode.Parse(wire.Json)!; var list=At(tree,"Grants.0.DescriptorsV2."+path).AsArray(); list.Add(list[0]!.DeepClone()); Bad(Jcs(tree),10000);
        }
        var swapped=JsonNode.Parse(wire.Json)!; var sourceList=At(swapped,"Grants.0.DescriptorsV2.Sources").AsArray(); var first=sourceList[0]!.DeepClone(); sourceList[0]=sourceList[1]!.DeepClone(); sourceList[1]=first; Bad(Jcs(swapped),10000);
        var casesSwap=JsonNode.Parse(wire.Json)!; var caseList=At(casesSwap,"Grants.0.DescriptorsV2.EvaluationPlan.CaseIds").AsArray(); var c=caseList[0]!.DeepClone(); caseList[0]=caseList[1]!.DeepClone(); caseList[1]=c; RefreshPlanHash(casesSwap); Bad(Jcs(casesSwap),10000);
        var sourceChange=JsonNode.Parse(wire.Json)!; At(sourceChange,"Grants.0.DescriptorsV2.Sources.7")["ManifestFileByteLength"]=2;
        Assert.NotEqual(good.DescriptorsV2!.SourceSelectionSha256,Good(Jcs(sourceChange),10000).DescriptorsV2!.SourceSelectionSha256);
        var caseChange=JsonNode.Parse(wire.Json)!; At(caseChange,"Grants.0.DescriptorsV2.EvaluationPlan.CaseIds").AsArray()[199]="sha256:"+Hex('f'); RefreshPlanHash(caseChange);
        var changed=Good(Jcs(caseChange),10000); Assert.NotEqual(good.DescriptorsV2.CorpusDescriptorSha256,changed.DescriptorsV2!.CorpusDescriptorSha256); Assert.NotEqual(good.DescriptorsV2.EvaluationPlanSha256,changed.DescriptorsV2.EvaluationPlanSha256);
        Assert.Equal(8,good.SourceSelection!.Sources.Count); Assert.Equal(wire.Cases,good.CorpusSelection!.CaseIds); Good(wire.Json,10000);
    }

    [Theory]
    [InlineData("Sources")] [InlineData("PairSelections")] [InlineData("EvaluationPlan.CaseIds")]
    public void NullVectorElement_FailsCompleteDecoding(string path)
    {
        var wire=new Wire();var tree=JsonNode.Parse(wire.Json)!;At(tree,"Grants.0.DescriptorsV2."+path).AsArray()[0]=null;Bad(Jcs(tree));Good(wire.Json);
    }

    [Fact]
    public void FullCanonicalBytes_RejectOrderWhitespaceEscapingAndNumericAliases()
    {
        var wire=new Wire(); var tree=JsonNode.Parse(wire.Json)!; var descriptor=At(tree,"Grants.0.DescriptorsV2").AsObject(); var canonical=Jcs(descriptor);
        Bad(wire.Json.Replace(canonical," "+canonical[..1]+" "+canonical[1..],StringComparison.Ordinal));
        var reverse=new JsonObject(descriptor.Reverse().Select(p=>new KeyValuePair<string,JsonNode?>(p.Key,p.Value?.DeepClone())));
        Bad(wire.Json.Replace(canonical,reverse.ToJsonString(JsonOptions),StringComparison.Ordinal));
        Bad(wire.Json.Replace("\"SourceId\":\"source00\"","\"SourceId\":\"\\u0073ource00\"",StringComparison.Ordinal));
        Bad(wire.Json.Replace("\"ManifestFileByteLength\":1","\"ManifestFileByteLength\":1e0",StringComparison.Ordinal));
        Bad(wire.Json.Replace("\"RepeatCount\":2","\"RepeatCount\":2.0",StringComparison.Ordinal));
        Good(wire.Json);
    }

    [Fact]
    public void ScalarAuthorityLimits_KeepProfileGenericAndGenerationPinsExplicitlyProtected()
    {
        var wire=new Wire();var tree=JsonNode.Parse(wire.Json)!;var d=At(tree,"Grants.0.DescriptorsV2");d["ProfileSelectionSha256"]=Hex('8');
        var original=Good(wire.Json);var changed=Good(Jcs(tree));Assert.Equal(Hex('8'),changed.DescriptorsV2!.ProfileSelectionSha256);
        Assert.Equal(original.DescriptorsV2!.EvaluatorProfileSha256,changed.DescriptorsV2.EvaluatorProfileSha256);
        At(tree,"Grants.0")["ConfigurationSha256"]=Hex('9');At(tree,"Grants.0.DescriptorsV2.Evaluator")["ConfigurationSha256"]=Hex('9');
        At(tree,"Grants.0")["ExamConditionSha256"]=Hex('a');At(tree,"Grants.0.DescriptorsV2.Evaluator")["ExamConditionSha256"]=Hex('a');
        At(tree,"Grants.0.DescriptorsV2.Evaluator")["GenerationSettingsSha256"]=Hex('b');
        changed=Good(Jcs(tree));Assert.Equal(Hex('9'),changed.ConfigurationSha256);Assert.Equal(Hex('a'),changed.ExamConditionSha256);Assert.Equal(Hex('b'),changed.EvaluatorSelection!.Evaluator.GenerationSettingsSha256);
        Assert.NotEqual(original.DescriptorsV2.EvaluatorProfileSha256,changed.DescriptorsV2!.EvaluatorProfileSha256);
    }

    [Fact]
    public void FrozenInputs_AreIndependentOfNodesDocumentsAndWritableVectors()
    {
        var wire=new Wire();var tree=JsonNode.Parse(wire.Json)!; CodeDocumentationOneLessonGrantV1 grant;
        using(var doc=JsonDocument.Parse(Jcs(tree)))grant=Good(doc.RootElement.GetRawText());
        At(tree,"Grants.0.DescriptorsV2.Sources.0")["SourceId"]="changed";At(tree,"Grants.0.DescriptorsV2.EvaluationPlan.CaseIds").AsArray()[0]="bad";
        AssertWholeLiteral(wire,grant);Assert.Throws<NotSupportedException>(()=>((IList)grant.SourceSelection!.Sources)[0]=null);
        Assert.Throws<NotSupportedException>(()=>((IList)grant.SourceSelection!.PairSelections).Clear());
        Assert.Throws<NotSupportedException>(()=>((IList<string>)grant.CorpusSelection!.CaseIds)[0]="bad");
        Assert.Throws<NotSupportedException>(()=>((IList<string>)grant.EvaluationPlanSelection!.CaseIds).Add("bad"));
        Assert.Equal("bug",grant.SourceSelection!.Sources[0].DomainSlug);Assert.Equal("bug",grant.CheckpointSelection!.Parent.DomainSlug);Assert.Equal("translation",grant.CorpusSelection!.Corpus.DomainSlug);
    }

    [Fact]
    public void FullAndCompactAndV1_RemainDisjointUnderSameCatalogAndLegacyWire()
    {
        var wire=new Wire();var full=Good(wire.Json);var compactTree=JsonNode.Parse(wire.Json)!;
        At(compactTree,"Grants.0")["DescriptorsV2"]=JsonSerializer.SerializeToNode(full.DescriptorsV2);var compact=Jcs(compactTree);
        var selected=Good(compact,0);Assert.Equal(full.DescriptorsV2,selected.DescriptorsV2);Assert.Null(selected.SourceSelection);Assert.Null(selected.EvaluatorSelection);
        var hybrid=JsonNode.Parse(wire.Json)!;At(hybrid,"Grants.0.DescriptorsV2")["EvaluatorProfileSha256"]=full.DescriptorsV2!.EvaluatorProfileSha256;Bad(Jcs(hybrid));
        var legacy=new CodeDocumentationOneLessonGrantV1("grant","v2","selector","objective","run",Hex('3'),Hex('1'),Hex('2'),Hex('4'),Hex('5'));
        var expected="{\"GrantId\":\"grant\",\"GrantVersion\":\"v2\",\"GrantSelector\":\"selector\",\"ObjectiveId\":\"objective\",\"TrainingRunId\":\"run\",\"CandidateSha256\":"+Q(Hex('3'))+",\"ParentManifestSha256\":"+Q(Hex('1'))+",\"ParentWeightsSha256\":"+Q(Hex('2'))+",\"ConfigurationSha256\":"+Q(Hex('4'))+",\"ExamConditionSha256\":"+Q(Hex('5'))+"}";
        Assert.Equal(expected,JsonSerializer.Serialize(legacy));Assert.Equal(expected,JsonSerializer.Serialize(full));
        var v1="{\"SchemaVersion\":1,\"Grants\":["+expected+"]}";Assert.Equal(1,Good(v1).FormatVersion);
        Good(v1.Replace("\"GrantId\":\"grant\"","\"GrantId\":\"old\",\"GrantId\":\"grant\"",StringComparison.Ordinal));
        Bad(wire.Json.Replace("\"SchemaVersion\":2,\"Grants\"","\"SchemaVersion\":1,\"Grants\"",StringComparison.Ordinal));
        var mixed=JsonNode.Parse(wire.Json)!;var second=At(compactTree,"Grants.0").DeepClone();second["GrantSelector"]="selector2";second["ObjectiveId"]="objective2";mixed["Grants"]!.AsArray().Add(second);
        var catalog=Load(Jcs(mixed));Assert.NotNull(Require(catalog.Resolve(Selector())).EvaluatorSelection);Assert.Null(Require(catalog.Resolve(new("objective2","selector2"))).EvaluatorSelection);
    }

    [Fact]
    public void WholeCatalog_InvalidMemberDuplicateSelectorObjectiveAndGrantCeilingsFailClosed()
    {
        var wire=new Wire();var tree=JsonNode.Parse(wire.Json)!;var grants=tree["Grants"]!.AsArray();var first=grants[0]!.DeepClone();
        for(var i=1;i<32;i++){var item=first.DeepClone();item["GrantSelector"]="selector"+i;item["ObjectiveId"]="objective"+i;grants.Add(item);}
        Assert.NotNull(Require(Load(Jcs(tree)).Resolve(new("objective31","selector31"))).SourceSelection);
        var wide=tree.DeepClone();wide["Grants"]!.AsArray().Add(first.DeepClone());Bad(Jcs(wide));
        var duplicate=tree.DeepClone();duplicate["Grants"]!.AsArray()[31]!["GrantSelector"]="selector";Bad(Jcs(duplicate));
        duplicate=tree.DeepClone();duplicate["Grants"]!.AsArray()[31]!["ObjectiveId"]="objective";Bad(Jcs(duplicate));
        var poisoned=tree.DeepClone();poisoned["Grants"]!.AsArray()[31]!["CandidateSha256"]="bad";Bad(Jcs(poisoned));Good(wire.Json);
    }

    [Fact]
    public void DocumentUtf8AndDepthCeilings_KeepFullDepthSixAndOuterWhitespaceCompatibility()
    {
        var wire=new Wire();var bytes=Encoding.UTF8.GetByteCount(wire.Json);var exact=wire.Json+new string(' ',1048576-bytes);Good(exact);Bad(exact+" ");
        var multibyte=wire.Json+" "+new string(' ',1048576-bytes-1);Good(multibyte);
        var node=JsonNode.Parse(wire.Json)!;At(node,"Grants.0")["GrantId"]="é";var utf8=Jcs(node);var len=Encoding.UTF8.GetByteCount(utf8);Good(utf8+new string(' ',1048576-len));Bad(utf8+new string(' ',1048577-len));
        foreach(var bad in new[]{"\uFEFF"+wire.Json,wire.Json+"\ud800",wire.Json.Replace("\"GrantId\":\"grant\"","\"GrantId\":\"\\ud800\"",StringComparison.Ordinal),wire.Json+"/*comment*/",wire.Json[..^1]+",}"})Bad(bad);
        var deep=JsonNode.Parse(wire.Json)!;At(deep,"Grants.0.DescriptorsV2")["Unknown"]=JsonNode.Parse("[[[[[[[[[0]]]]]]]]]");Bad(Jcs(deep));
        using var doc=JsonDocument.Parse(wire.Json,new JsonDocumentOptions{MaxDepth=6});Assert.Equal(JsonValueKind.Object,doc.RootElement.ValueKind);Good(wire.Json);
    }

    [Fact]
    public void DefaultWriter_StillRejectsNegativeFractionArrayNullAndAllowsWidePositiveInt64()
    {
        using(var positive=JsonDocument.Parse("{\"Seed\":2147483648}"))Assert.Equal("{\"Seed\":2147483648}",Encoding.UTF8.GetString(CodeDocumentationOneLessonV2Wire.CanonicalBytes(positive.RootElement)));
        foreach(var bad in new[]{"{\"Seed\":-1}","{\"Seed\":1.5}","[]","null"}){using var doc=JsonDocument.Parse(bad);Assert.Throws<JsonException>(()=>CodeDocumentationOneLessonV2Wire.CanonicalBytes(doc.RootElement));}
    }

    [Fact]
    public void ActualPrimitiveCeilings_AdmitCompleteDistinctValuesAndRejectPlusOne()
    {
        var wire=new Wire();var tree=JsonNode.Parse(wire.Json)!;var d=At(tree,"Grants.0.DescriptorsV2");var e=d["Evaluator"]!;
        var token=new string('a',128);var slug=new string('s',96);
        d["Sources"]![0]!["SourceId"]=token;d["PairSelections"]![0]!["SourceId"]=token;
        d["Sources"]![0]!["ManifestFileByteLength"]=8388608L;
        foreach(var field in new[]{"DomainSlug","DesignPatternSlug","DatasetVersion"})d["Sources"]![0]![field]=slug;
        foreach(var field in new[]{"DomainSlug","DesignPatternSlug","ModelIdSlug","ModelVersion"})d["ParentCheckpoint"]![field]=slug;
        d["ParentCheckpoint"]!["Step"]=int.MaxValue;d["ParentCheckpoint"]!["WeightsByteLength"]=17179869184L;d["Candidate"]!["CandidateId"]=token;
        d["Corpus"]!["CorpusId"]=token;d["Corpus"]!["ManifestFileByteLength"]=2097152L;d["Corpus"]!["PayloadByteLength"]=33554432L;
        foreach(var field in new[]{"DomainSlug","SubjectSlug","CorpusVersion"})d["Corpus"]![field]=slug;
        d["EvaluationPlan"]!["CorpusId"]=token;d["EvaluationPlan"]!["PlanId"]=token;d["EvaluationPlan"]!["EvaluatorId"]=token;
        e["EvaluatorId"]=token;e["Version"]=new string('v',64);e["ScorerIdentity"]!["DomainTag"]=slug;
        e["GenerationSettings"]!["MaxNewTokens"]=2048;e["GenerationSettings"]!["Temperature"]="2";e["GenerationSettings"]!["TopK"]=10000;
        e["GenerationSettings"]!["MaxAnswerChars"]=65536;e["CurrentConfiguration"]!["DpoMaxStepCount"]=5;
        RefreshPlanHash(tree);var selected=Good(Jcs(tree));Assert.Equal(17179869184L,selected.CheckpointSelection!.Parent.WeightsByteLength);
        Assert.Equal(2048,selected.EvaluatorSelection!.Evaluator.GenerationSettings.MaxNewTokens);Assert.Equal(65536,selected.EvaluatorSelection.Evaluator.GenerationSettings.MaxAnswerChars);
        foreach(var path in new[]{"Sources.0.SourceId","Candidate.CandidateId","Corpus.CorpusId","EvaluationPlan.PlanId","Evaluator.EvaluatorId"})
        {var bad=tree.DeepClone();var p="Grants.0.DescriptorsV2."+path;var split=p.LastIndexOf('.');At(bad,p[..split])[p[(split+1)..]]=token+"a";RefreshPlanHash(bad);Bad(Jcs(bad));}
        foreach(var path in new[]{"Sources.0.DomainSlug","ParentCheckpoint.ModelVersion","Corpus.SubjectSlug"})
        {var bad=tree.DeepClone();var p="Grants.0.DescriptorsV2."+path;var split=p.LastIndexOf('.');At(bad,p[..split])[p[(split+1)..]]=slug+"s";Bad(Jcs(bad));}
        var dpo=tree.DeepClone();At(dpo,"Grants.0.DescriptorsV2.Evaluator.CurrentConfiguration")["DpoMaxStepCount"]=6;Bad(Jcs(dpo));Good(Jcs(tree));
    }

    public static IEnumerable<object[]> FullLeafSensitivities()
    {
        var literal=JsonNode.Parse(new Wire().Json)!;
        foreach(var (path,fields) in Schema.Where(s=>s.Path.StartsWith("Grants.0.DescriptorsV2",StringComparison.Ordinal)))
            foreach(var field in fields.Split(','))
                if(At(literal,path)[field] is JsonValue)yield return [path,field];
    }

    [Theory] [MemberData(nameof(FullLeafSensitivities))]
    public void EveryFullLeaf_ChangesCompactCommitmentOrRejectsActualContract(string path,string field)
    {
        var wire=new Wire();var initial=Good(wire.Json);var tree=JsonNode.Parse(wire.Json)!;var target=At(tree,path);var original=target[field]!;
        target[field]=original.GetValueKind() switch{
            JsonValueKind.String=>JsonValue.Create(original.GetValue<string>().Length==64?(original.GetValue<string>()[0]=='f'?'e':'f')+original.GetValue<string>()[1..]:original.GetValue<string>()+"x"),
            JsonValueKind.True or JsonValueKind.False=>JsonValue.Create(!original.GetValue<bool>()),
            _=>JsonValue.Create(original.GetValue<long>()+1)};
        var result=Load(Jcs(tree)).Resolve(Selector());
        if(result.IsSuccess){Assert.NotNull(result.Value);Assert.NotEqual(Jcs(JsonSerializer.SerializeToNode(initial.DescriptorsV2)!),Jcs(JsonSerializer.SerializeToNode(result.Value!.DescriptorsV2)!));}
        else Assert.Equal(CodeDocumentationOneLessonGrantCatalog.InvalidCatalogErrorCode,result.ErrorCode);
        AssertWholeLiteral(wire,Good(wire.Json));
    }

    private static readonly (string Path,string Fields)[] Schema = [
        ("","SchemaVersion,Grants"),("Grants.0","GrantId,GrantVersion,GrantSelector,ObjectiveId,TrainingRunId,CandidateSha256,ParentManifestSha256,ParentWeightsSha256,ConfigurationSha256,ExamConditionSha256,DescriptorsV2"),
        ("Grants.0.DescriptorsV2","SchemaVersion,Sources,PairSelections,ParentCheckpoint,Candidate,Corpus,EvaluationPlan,Evaluator,ProfileSelectionSha256"),
        ("Grants.0.DescriptorsV2.Sources.0","SourceId,Role,DomainSlug,DesignPatternSlug,DatasetVersion,ManifestFileSha256,ManifestFileByteLength,SealedManifestSha256"),
        ("Grants.0.DescriptorsV2.PairSelections.0","TrainingPairId,SourceId"),("Grants.0.DescriptorsV2.ParentCheckpoint","Step,DomainSlug,DesignPatternSlug,ModelIdSlug,ModelVersion,ManifestSha256,WeightsSha256,WeightsByteLength"),
        ("Grants.0.DescriptorsV2.Candidate","CandidateId,CandidateSha256"),("Grants.0.DescriptorsV2.Corpus","CorpusId,Purpose,DomainSlug,SubjectSlug,CorpusVersion,ManifestFileSha256,CorpusSha256,ManifestFileByteLength,PayloadSha256,PayloadByteLength"),
        ("Grants.0.DescriptorsV2.EvaluationPlan","PlanId,PlanSha256,CorpusId,EvaluatorId,CaseIds,RepeatCount,ExpectedEvaluationCallCount"),
        ("Grants.0.DescriptorsV2.Evaluator","EvaluatorId,Version,ConfigurationSha256,ExamConditionSha256,GenerationSettingsSha256,NoisePolicyIdentitySha256,ScorerIdentity,GenerationSettings,NoisePolicy,EvaluationProfile,CurrentConfiguration"),
        ("Grants.0.DescriptorsV2.Evaluator.ScorerIdentity","DomainTag,ScorerId,ScorerVersion,NormalizationProfileId,MetricProfileId"),
        ("Grants.0.DescriptorsV2.Evaluator.GenerationSettings","MaxNewTokens,Temperature,TopK,TopP,Seed,MaxHeldOutCases,MaxAnswerChars"),
        ("Grants.0.DescriptorsV2.Evaluator.NoisePolicy","RepeatCount,StandardDeviationMultiplier"),("Grants.0.DescriptorsV2.Evaluator.EvaluationProfile","ProfileId,Purpose,FormatIdentity,RequestFormatMode,NumContext,Temperature,Seed,Think,NumPredict,RequestTimeoutSeconds,ConfigurationSha256"),
        ("Grants.0.DescriptorsV2.Evaluator.CurrentConfiguration","ModelId,ExpectedOllamaManifestDigest,ExpectedOllamaModelLayerDigest,MaxTeacherCalls,LocalRetentionPolicyRef,OutputLicenseRef,DpoMaxStepCount")];
    private const string BudgetKey="XIIGEN_CONTINUOUS_LEARNING_MAX_TEACHER_CALLS";
    private static readonly JsonSerializerOptions JsonOptions=new(){Encoder=JavaScriptEncoder.UnsafeRelaxedJsonEscaping};
    private static string Jcs(JsonNode node)=>node switch{JsonObject o=>"{"+string.Join(",",o.OrderBy(p=>p.Key,StringComparer.Ordinal).Select(p=>Q(p.Key)+":"+(p.Value is null?"null":Jcs(p.Value))))+"}",JsonArray a=>"["+string.Join(",",a.Select(v=>v is null?"null":Jcs(v)))+"]",_=>node.ToJsonString(JsonOptions)};
    private static string Q(string value)=>JsonSerializer.Serialize(value,JsonOptions);
    private static string I(long value)=>value.ToString(CultureInfo.InvariantCulture);
    private static string Hex(char c)=>new(c,64);
    private static string Sha(byte[] data)=>Convert.ToHexString(SHA256.HashData(data)).ToLowerInvariant();
    private static string Hash(string text)=>Sha(Encoding.UTF8.GetBytes(text));
    private static string DomainHash(string domain,string json)=>Hash(domain+json);
    private static T Require<T>(OperationResult<T> result){Assert.True(result.IsSuccess,result.ErrorCode+": "+result.ErrorMessage);Assert.NotNull(result.Value);return result.Value!;}
    private static CodeDocumentationOneLessonSelectorV1 Selector()=>new("objective","selector");
    private static CodeDocumentationOneLessonGrantCatalog Load(string json,string budget="8")=>CodeDocumentationOneLessonGrantCatalog.Load(name=>name==CodeDocumentationOneLessonGrantCatalog.EnvironmentVariable?json:name==BudgetKey?budget:null);
    private static CodeDocumentationOneLessonGrantV1 Good(string json,int budget=8)=>Require(Load(json,I(budget)).Resolve(Selector()));
    private static void Bad(string json,int budget=8)=>Assert.Equal(CodeDocumentationOneLessonGrantCatalog.InvalidCatalogErrorCode,Load(json,I(budget)).Resolve(Selector()).ErrorCode);
    private static JsonNode At(JsonNode node,string path){foreach(var key in path.Split('.',StringSplitOptions.RemoveEmptyEntries))node=node is JsonArray a?a[int.Parse(key,CultureInfo.InvariantCulture)]!:node[key]!;return node;}
    private static JsonNode WrongType(JsonNode original)=>original is JsonArray?new JsonObject():original is JsonObject?new JsonArray():original.GetValueKind()==JsonValueKind.String?JsonValue.Create(1)!:JsonValue.Create("wrong")!;
    private static void MutateBad(string path,JsonNode replacement){var wire=new Wire();var node=JsonNode.Parse(wire.Json)!;var split=path.LastIndexOf('.');At(node,path[..split])[path[(split+1)..]]=replacement;Bad(Jcs(node));Good(wire.Json);}
    private static void RefreshPlanHash(JsonNode tree){var plan=At(tree,"Grants.0.DescriptorsV2.EvaluationPlan").AsObject();var copy=plan.DeepClone().AsObject();copy.Remove("PlanSha256");plan["PlanSha256"]=Hash(Jcs(copy));}
    private static string ProfileHash()
    {
        using var stream=new MemoryStream();
        foreach(var value in new[]{"xiigen:local-qwen-ollama-inference-profile:v1\0","profile_id","qwen35-local-evaluation-v1","purpose","evaluation_only","format","json","num_ctx","8192","temperature","0","seed","0","think","false","num_predict","256","request_timeout_seconds","120"})
        {var bytes=Encoding.UTF8.GetBytes(value);var length=new byte[8];BinaryPrimitives.WriteInt64BigEndian(length,bytes.LongLength);stream.Write(length);stream.Write(bytes);}
        return Sha(stream.ToArray());
    }
    private static void AssertWholeLiteral(Wire wire,CodeDocumentationOneLessonGrantV1 grant)
    {
        var d=grant.DescriptorsV2!;Assert.Equal(2,grant.FormatVersion);Assert.NotNull(grant.SourceSelection);Assert.NotNull(grant.CheckpointSelection);Assert.NotNull(grant.CorpusSelection);Assert.NotNull(grant.EvaluationPlanSelection);Assert.NotNull(grant.EvaluatorSelection);
        Assert.Equal(DomainHash("xiigen-s04-source-selection-v2\n",wire.SourcePreimage),d.SourceSelectionSha256);Assert.Equal(DomainHash("xiigen-s04-parentcheckpoint-v2\n",wire.Parent),d.ParentCheckpointSha256);
        Assert.Equal(DomainHash("xiigen-s04-candidateselection-v2\n",wire.Candidate),d.CandidateSelectionSha256);Assert.Equal(DomainHash("xiigen-s04-corpusdescriptor-v2\n",wire.CorpusPreimage),d.CorpusDescriptorSha256);
        Assert.Equal(Hash(wire.PlanPreimage),d.EvaluationPlanSha256);Assert.Equal(DomainHash("xiigen-s04-evaluatorprofile-v2\n",wire.Evaluator),d.EvaluatorProfileSha256);Assert.Equal(Hex('7'),d.ProfileSelectionSha256);Assert.Equal(wire.Cases.Length,d.CaseCount);Assert.Equal(wire.Repeats,d.RepeatCount);
        Assert.Equal(wire.Sources,Jcs(JsonSerializer.SerializeToNode(grant.SourceSelection!.Sources)!));Assert.Equal(wire.Pairs,Jcs(JsonSerializer.SerializeToNode(grant.SourceSelection.PairSelections)!));
        Assert.Equal(wire.Parent,Jcs(JsonSerializer.SerializeToNode(grant.CheckpointSelection!.Parent)!));Assert.Equal(wire.Candidate,Jcs(JsonSerializer.SerializeToNode(grant.CheckpointSelection.Candidate)!));Assert.Equal(wire.Corpus,Jcs(JsonSerializer.SerializeToNode(grant.CorpusSelection!.Corpus)!));
        Assert.Equal(wire.Cases,grant.CorpusSelection.CaseIds);Assert.Equal(wire.Cases,grant.EvaluationPlanSelection!.CaseIds);Assert.Equal(wire.Evaluator,Jcs(JsonSerializer.SerializeToNode(grant.EvaluatorSelection!.Evaluator)!));
        Assert.Equal(Encoding.UTF8.GetBytes(wire.SourcePreimage),CodeDocumentationOneLessonV2Wire.CanonicalSourceSelectionBytes(grant.SourceSelection));Assert.Equal(Encoding.UTF8.GetBytes(wire.CorpusPreimage),CodeDocumentationOneLessonV2Wire.CanonicalCorpusSelectionBytes(grant.CorpusSelection));
        Assert.Equal(Encoding.UTF8.GetBytes(wire.PlanPreimage),CodeDocumentationOneLessonV2Wire.CanonicalEvaluationPlanBytes(grant.EvaluationPlanSelection));Assert.Equal(Encoding.UTF8.GetBytes(wire.Evaluator),CodeDocumentationOneLessonV2Wire.CanonicalEvaluatorProfileBytes(grant.EvaluatorSelection));
    }
    private sealed class Wire
    {
        internal string Json{get;} internal string Sources{get;} internal string Pairs{get;} internal string Parent{get;} internal string Candidate{get;} internal string Corpus{get;} internal string PlanPreimage{get;} internal string Evaluator{get;} internal string[] Cases{get;} internal int Repeats{get;}
        internal string SourcePreimage=>"{\"PairSelections\":"+Pairs+",\"Sources\":"+Sources+"}";
        internal string CorpusPreimage=>"{\"CaseIds\":["+string.Join(",",Cases.Select(Q))+"],\"Corpus\":"+Corpus+"}";
        internal Wire(int count=2,int repeats=2,int budget=8,int sources=1,int seed=int.MinValue,string temperature="0")
        {
            Repeats=repeats;Cases=Enumerable.Range(0,count).Select(i=>"sha256:"+i.ToString("x64",CultureInfo.InvariantCulture)).ToArray();
            Sources="["+string.Join(",",Enumerable.Range(0,sources).Select(i=>"{\"DatasetVersion\":\"v"+I(i+1)+"\",\"DesignPatternSlug\":\"learning-unit\",\"DomainSlug\":\"bug\",\"ManifestFileByteLength\":1,\"ManifestFileSha256\":"+Q(Hex('1'))+",\"Role\":\"TrainingPairs\",\"SealedManifestSha256\":"+Q(Hex('2'))+",\"SourceId\":\"source"+i.ToString("00",CultureInfo.InvariantCulture)+"\"}"))+"]";
            Pairs="[{\"SourceId\":\"source00\",\"TrainingPairId\":"+Q(Hex('a'))+"}]";
            Parent="{\"DesignPatternSlug\":\"learning-unit\",\"DomainSlug\":\"bug\",\"ManifestSha256\":"+Q(Hex('1'))+",\"ModelIdSlug\":\"tiny-model\",\"ModelVersion\":\"v1\",\"Step\":1,\"WeightsByteLength\":7,\"WeightsSha256\":"+Q(Hex('2'))+"}";
            Candidate="{\"CandidateId\":\"candidate:1\",\"CandidateSha256\":"+Q(Hex('3'))+"}";
            Corpus="{\"CorpusId\":\"opaque:corpus\",\"CorpusSha256\":"+Q(Hex('2'))+",\"CorpusVersion\":\"v1\",\"DomainSlug\":\"translation\",\"ManifestFileByteLength\":1,\"ManifestFileSha256\":"+Q(Hex('1'))+",\"PayloadByteLength\":1,\"PayloadSha256\":"+Q(Hex('3'))+",\"Purpose\":\"ClosedEvaluation\",\"SubjectSlug\":\"en-ru\"}";
            var caseArray="["+string.Join(",",Cases.Select(Q))+"]";
            PlanPreimage="{\"CaseIds\":"+caseArray+",\"CorpusId\":\"opaque:corpus\",\"EvaluatorId\":\"evaluator:1\",\"ExpectedEvaluationCallCount\":"+I((long)count*repeats)+",\"PlanId\":\"plan:1\",\"RepeatCount\":"+I(repeats)+"}";
            var plan=PlanPreimage.Replace(",\"RepeatCount\":",",\"PlanSha256\":"+Q(Hash(PlanPreimage))+",\"RepeatCount\":",StringComparison.Ordinal);
            Evaluator="{\"ConfigurationSha256\":"+Q(Hex('4'))+",\"CurrentConfiguration\":{\"DpoMaxStepCount\":"+I(StableDpoTrainingExecutor.DefaultDpoStepCount)+",\"ExpectedOllamaManifestDigest\":"+Q(Hex('a'))+",\"ExpectedOllamaModelLayerDigest\":"+Q(Hex('b'))+",\"LocalRetentionPolicyRef\":"+Q("sha256:"+Hex('c'))+",\"MaxTeacherCalls\":"+I(budget)+",\"ModelId\":\"qwen-code-doc-quality-test:9b\",\"OutputLicenseRef\":"+Q("sha256:"+Hex('d'))+"},"+
                "\"EvaluationProfile\":{\"ConfigurationSha256\":"+Q(ProfileHash())+",\"FormatIdentity\":\"json\",\"NumContext\":8192,\"NumPredict\":256,\"ProfileId\":\"qwen35-local-evaluation-v1\",\"Purpose\":\"evaluation_only\",\"RequestFormatMode\":\"ExplicitIdentity\",\"RequestTimeoutSeconds\":\"120\",\"Seed\":0,\"Temperature\":\"0\",\"Think\":false},\"EvaluatorId\":\"evaluator:1\",\"ExamConditionSha256\":"+Q(Hex('5'))+
                ",\"GenerationSettings\":{\"MaxAnswerChars\":1,\"MaxHeldOutCases\":200,\"MaxNewTokens\":1,\"Seed\":"+I(seed)+",\"Temperature\":"+Q(temperature)+",\"TopK\":0,\"TopP\":\"1\"},\"GenerationSettingsSha256\":"+Q(Hex('6'))+",\"NoisePolicy\":{\"RepeatCount\":"+I(repeats)+",\"StandardDeviationMultiplier\":\"1.25\"},\"NoisePolicyIdentitySha256\":"+Q(Hash("xiigen.measurement-noise-policy.v1\n"+I(repeats)+"\n1.25"))+
                ",\"ScorerIdentity\":{\"DomainTag\":\"translation\",\"MetricProfileId\":\"code-documentation-case-pass-fraction-v1\",\"NormalizationProfileId\":\"qwen35-local-evaluation-v1\",\"ScorerId\":\"qwen-code-doc-quality-test:9b\",\"ScorerVersion\":"+Q(Hex('a'))+"},\"Version\":\"v2\"}";
            var descriptor="{\"Candidate\":"+Candidate+",\"Corpus\":"+Corpus+",\"EvaluationPlan\":"+plan+",\"Evaluator\":"+Evaluator+",\"PairSelections\":"+Pairs+",\"ParentCheckpoint\":"+Parent+",\"ProfileSelectionSha256\":"+Q(Hex('7'))+",\"SchemaVersion\":2,\"Sources\":"+Sources+"}";
            var grant="{\"CandidateSha256\":"+Q(Hex('3'))+",\"ConfigurationSha256\":"+Q(Hex('4'))+",\"DescriptorsV2\":"+descriptor+",\"ExamConditionSha256\":"+Q(Hex('5'))+",\"GrantId\":\"grant\",\"GrantSelector\":\"selector\",\"GrantVersion\":\"v2\",\"ObjectiveId\":\"objective\",\"ParentManifestSha256\":"+Q(Hex('1'))+",\"ParentWeightsSha256\":"+Q(Hex('2'))+",\"TrainingRunId\":\"run\"}";
            Json="{\"SchemaVersion\":2,\"Grants\":["+grant+"]}";
        }
    }
    private sealed class Sentinel:IDisposable
    {
        internal string Root{get;}=Path.Combine(Path.GetTempPath(),"s04-startup-decoder-"+Guid.NewGuid().ToString("N"));internal string Missing=>Path.Combine(Root,"missing");
        internal Sentinel(){Directory.CreateDirectory(Root);File.WriteAllText(Path.Combine(Root,"neighbor"),"preserved");}
        internal string[] Snapshot()=>Directory.GetFileSystemEntries(Root,"*",SearchOption.AllDirectories).OrderBy(p=>p,StringComparer.Ordinal).Select(p=>Path.GetRelativePath(Root,p)+":"+(File.Exists(p)?Sha(File.ReadAllBytes(p)):"directory")).ToArray();
        public void Dispose(){var full=Path.GetFullPath(Root);var temp=Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar;Assert.StartsWith(temp,full,StringComparison.Ordinal);Assert.StartsWith("s04-startup-decoder-",Path.GetFileName(full),StringComparison.Ordinal);Directory.Delete(full,true);}
    }
}
