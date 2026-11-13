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

public sealed class CodeDocumentationOneLessonEvaluatorSelectionTests(Xunit.Abstractions.ITestOutputHelper output)
{
    [Fact]
    public void FullFresh_RealOwnersNoncanonicalNoiseAndCompleteGenerationDigestMatchLiteralAndRuntimeIdentities()
    {
        using var f = new Fixture(2);
        var settings = Settings() with { Seed = int.MinValue, Temperature = BitConverter.Int32BitsToSingle(int.MinValue) };
        var cfg = Config(8); var noise = new MeasurementNoisePolicyConfig(2, 1.25);
        var selected = f.Select(settings, cfg, noise);
        AssertLiteral(selected);
        var rawBefore = File.ReadAllBytes(f.PayloadPath); var manifestBefore = File.ReadAllBytes(f.Address.ManifestPath);
        var opened = Require(selected.OpenFresh(f.Repository, settings, cfg, noise, Hex('4'), Hex('5')));
        Assert.NotSame(selected, opened.Selection);
        Assert.Equal(selected.EvaluatorProfileSha256, opened.Selection.EvaluatorProfileSha256);
        Assert.Equal(f.Corpus.Entries.Count, opened.Corpus.Entries.Count);
        Assert.Equal(8, opened.QualityGatePlan.ExpectedTotalEvaluationCallCount);
        var actualHeldOut = opened.Corpus.Entries.Where(e => e.Partition == FrozenReferencePartition.HeldOut).OrderBy(e => e.EntryId, StringComparer.Ordinal).ToArray();
        Assert.Equal(2, actualHeldOut.Length);
        Assert.Equal(CodeDocumentationSavedModelAssessmentService.ComputeGenerationSettingsSha256(settings, actualHeldOut), selected.Evaluator.GenerationSettingsSha256);
        Assert.NotEqual(CodeDocumentationSavedModelAssessmentService.ComputeGenerationSettingsSha256(settings, actualHeldOut[..1]), selected.Evaluator.GenerationSettingsSha256);
        Assert.Equal(rawBefore, File.ReadAllBytes(f.PayloadPath)); Assert.Equal(manifestBefore, File.ReadAllBytes(f.Address.ManifestPath));
        Assert.Equal("neighbor preserved", File.ReadAllText(f.Neighbor));
        Assert.False(Directory.Exists(Path.Combine(f.Repository, "training-artifacts", "models")));
        Assert.DoesNotContain(Directory.GetFiles(f.Repository, "*", SearchOption.AllDirectories), p => p.EndsWith("session.jsonl", StringComparison.Ordinal) || p.Contains(Path.DirectorySeparatorChar + "attempts" + Path.DirectorySeparatorChar, StringComparison.Ordinal));
        LogRuntimeIdentities();
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    public void TypedSignedSeed_AllInt32ExtremesReachOnlyClosedEvaluatorWriterAndRealGenerationOwner(int seed)
    {
        using var f = new Fixture(1); var settings = Settings() with { Seed = seed };
        var selected = f.Select(settings, Config(4), Noise()); AssertLiteral(selected);
        using var json = JsonDocument.Parse(CodeDocumentationOneLessonV2Wire.CanonicalEvaluatorProfileBytes(selected));
        Assert.Equal(seed.ToString(CultureInfo.InvariantCulture), json.RootElement.GetProperty("GenerationSettings").GetProperty("Seed").GetRawText());
        Assert.Equal("0", json.RootElement.GetProperty("EvaluationProfile").GetProperty("Seed").GetRawText());
        Assert.True(selected.OpenFresh(f.Repository, settings, Config(4), Noise(), Hex('4'), Hex('5')).IsSuccess);
    }

    [Theory]
    [InlineData(1, 5000)]
    [InlineData(200, 25)]
    public void FullSelection_CompleteJointHardCapAndTokenBoundariesUseActualOwners(int count, int repeat)
    {
        var c = PureCorpus(Enumerable.Range(0, count).Select(i => "sha256:" + i.ToString("x64", CultureInfo.InvariantCulture)));
        var cfg = Config(10000); var noise = new MeasurementNoisePolicyConfig(repeat, 1.25);
        var e = Shape(Settings() with { MaxNewTokens = 2048, Temperature = 2, TopK = 10000, TopP = 1, Seed = int.MaxValue, MaxAnswerChars = 65536 }, cfg, noise) with { EvaluatorId = new('a', 128), Version = new('v', 64) };
        AssertLiteral(Select(c, e));
        Assert.Equal(5000, Plan(c, repeat, e.EvaluatorId).ExpectedEvaluationCallCount);
        Assert.False(CodeDocumentationOneLessonEvaluatorSelection.Create(c, Plan(c, repeat, e.EvaluatorId), e with { CurrentConfiguration = e.CurrentConfiguration with { MaxTeacherCalls = 9999 } }).IsSuccess);
    }

    [Fact]
    public void FullBytes_DistinguishNegativeZeroAndRemainInvariantAcrossCulture()
    {
        var c = PureCorpus(); var g = Shape();
        var plus = Select(c, g); var minus = Select(c, g with { GenerationSettings = g.GenerationSettings with { Temperature = "-0" } });
        Assert.True(0f.Equals(BitConverter.Int32BitsToSingle(int.MinValue)));
        Assert.NotEqual(plus.EvaluatorProfileSha256, minus.EvaluatorProfileSha256); AssertLiteral(plus); AssertLiteral(minus);
        var culture = CultureInfo.CurrentCulture; var ui = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR"); CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("tr-TR");
            Assert.Equal(minus.EvaluatorProfileSha256, Select(c, minus.Evaluator).EvaluatorProfileSha256);
            AssertLiteral(minus);
        }
        finally { CultureInfo.CurrentCulture = culture; CultureInfo.CurrentUICulture = ui; }
        using var f = new Fixture(1);
        var actualMinus = Settings() with { Temperature = BitConverter.Int32BitsToSingle(int.MinValue) };
        var selected = f.Select(actualMinus, Config(4), Noise());
        var missing = Path.Combine(f.Repository, "missing-owner");
        Assert.Equal(CodeDocumentationOneLessonEvaluatorSelection.InvalidErrorCode, selected.OpenFresh(missing, Settings(), Config(4), Noise(), Hex('4'), Hex('5')).ErrorCode);
        Assert.False(Directory.Exists(missing));
        Assert.True(selected.OpenFresh(f.Repository, actualMinus, Config(4), Noise(), Hex('4'), Hex('5')).IsSuccess);
    }

    [Fact]
    public void PureCreate_DeepCopiesEveryRecordAndRetainsImmutableCompleteDependencies()
    {
        var ids = new[] { Id('1'), Id('2') }; var corpus = PureCorpus(ids); var plan = Plan(corpus); var e = Shape();
        var selected = Require(CodeDocumentationOneLessonEvaluatorSelection.Create(corpus, plan, e));
        Assert.NotSame(e, selected.Evaluator); Assert.NotSame(e.GenerationSettings, selected.Evaluator.GenerationSettings);
        Assert.NotSame(e.NoisePolicy, selected.Evaluator.NoisePolicy); Assert.NotSame(e.EvaluationProfile, selected.Evaluator.EvaluationProfile);
        Assert.NotSame(e.CurrentConfiguration, selected.Evaluator.CurrentConfiguration); Assert.NotSame(e.ScorerIdentity, selected.Evaluator.ScorerIdentity);
        ids[0] = Id('9'); e = e with { Version = "changed" };
        Assert.Equal(new[] { Id('1'), Id('2') }, corpus.CaseIds); Assert.Equal(corpus.CaseIds, plan.CaseIds);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)corpus.CaseIds)[0] = Id('9'));
        Assert.Throws<NotSupportedException>(() => ((IList)plan.CaseIds).Add(Id('9')));
        Assert.Equal("v2", selected.Evaluator.Version); AssertLiteral(selected);
        Assert.False(CodeDocumentationOneLessonEvaluatorSelection.Create(null, plan, selected.Evaluator).IsSuccess);
        Assert.False(CodeDocumentationOneLessonEvaluatorSelection.Create(corpus, null, selected.Evaluator).IsSuccess);
        Assert.False(CodeDocumentationOneLessonEvaluatorSelection.Create(corpus, plan, null).IsSuccess);
        Assert.False(CodeDocumentationOneLessonEvaluatorSelection.Create(PureCorpus([Id('3')]), plan, selected.Evaluator).IsSuccess);
    }

    public static IEnumerable<object[]> ValidFieldChanges()
        => new[] { "id", "version", "configuration-pin", "exam-pin", "generation-pin", "tokens", "temperature", "top-k", "top-p", "seed", "maximum-cases", "maximum-answer", "noise-repeat", "noise-multiplier", "model", "manifest", "layer", "budget", "retention", "license", "dpo", "domain" }.Select(s => new object[] { s });

    [Theory]
    [MemberData(nameof(ValidFieldChanges))]
    public void EachValidFullFieldMutationChangesIndependentLiteralCommitment(string field)
    {
        var c = PureCorpus(); var e = Shape(); var original = Select(c, e); var g = e.GenerationSettings; var cfg = e.CurrentConfiguration;
        e = field switch
        {
            "id" => e with { EvaluatorId = "other:evaluator" }, "version" => e with { Version = new('v', 64) },
            "configuration-pin" => e with { ConfigurationSha256 = Hex('6') }, "exam-pin" => e with { ExamConditionSha256 = Hex('6') },
            "generation-pin" => e with { GenerationSettingsSha256 = Hex('7') },
            "tokens" => e with { GenerationSettings = g with { MaxNewTokens = 2 } },
            "temperature" => e with { GenerationSettings = g with { Temperature = "0.5" } },
            "top-k" => e with { GenerationSettings = g with { TopK = 3 } },
            "top-p" => e with { GenerationSettings = g with { TopP = "0.5" } },
            "seed" => e with { GenerationSettings = g with { Seed = int.MinValue } },
            "maximum-cases" => e with { GenerationSettings = g with { MaxHeldOutCases = 199 } },
            "maximum-answer" => e with { GenerationSettings = g with { MaxAnswerChars = 2 } },
            "noise-repeat" => e with { NoisePolicy = new(3, "1.25"), NoisePolicyIdentitySha256 = new MeasurementNoisePolicyConfig(3, 1.25).IdentitySha256 },
            "noise-multiplier" => e with { NoisePolicy = new(2, "2"), NoisePolicyIdentitySha256 = new MeasurementNoisePolicyConfig(2, 2).IdentitySha256 },
            "model" => e with { CurrentConfiguration = cfg with { ModelId = "qwen-code-doc-other:9b" } },
            "manifest" => e with { CurrentConfiguration = cfg with { ExpectedOllamaManifestDigest = Hex('6') } },
            "layer" => e with { CurrentConfiguration = cfg with { ExpectedOllamaModelLayerDigest = Hex('6') } },
            "budget" => e with { CurrentConfiguration = cfg with { MaxTeacherCalls = 12 } },
            "retention" => e with { CurrentConfiguration = cfg with { LocalRetentionPolicyRef = Id('6') } },
            "license" => e with { CurrentConfiguration = cfg with { OutputLicenseRef = Id('6') } },
            "dpo" => e with { CurrentConfiguration = cfg with { DpoMaxStepCount = cfg.DpoMaxStepCount + 1 } },
            "domain" => e, _ => throw new InvalidOperationException(field),
        };
        if (field == "domain") c = Require(CodeDocumentationOneLessonCorpusSelection.Create(c.Corpus with { DomainSlug = "bug" }, c.CaseIds));
        e = e with { ScorerIdentity = CodeDocumentationSavedModelAssessmentCalibrationService.BuildCurrentScorerIdentity(c.Corpus.DomainSlug, e.CurrentConfiguration.ModelId, e.CurrentConfiguration.ExpectedOllamaManifestDigest) };
        var changed = Select(c, e); AssertLiteral(original); AssertLiteral(changed);
        Assert.NotEqual(original.EvaluatorProfileSha256, changed.EvaluatorProfileSha256);
    }

    public static IEnumerable<object[]> InvalidFields()
        => new[] { "id-null", "id-empty", "id-leading", "id-space", "id-unicode", "id-129", "version-65", "version-null", "configuration-pin", "exam-pin", "generation-pin", "noise-pin", "null-generation", "null-noise", "null-profile", "null-config", "null-scorer", "tokens", "top-k", "maximum-cases", "maximum-answer", "noise-repeat", "noise-multiplier", "noise-zero", "noise-infinity", "model", "manifest", "layer", "budget-zero", "budget-hard", "budget-joint", "retention", "license", "dpo", "scorer-domain", "scorer-model", "scorer-manifest", "scorer-profile", "scorer-metric", "scorer-long", "scorer-null", "profile-id", "profile-purpose", "profile-format", "profile-mode", "profile-context", "profile-temperature", "profile-seed", "profile-think", "profile-predict", "profile-timeout", "profile-hash" }.Select(s => new object[] { s });

    [Theory]
    [MemberData(nameof(InvalidFields))]
    public void InvalidNestedFieldsAndFixedOwnerMutationsRejectBeforeHashAndRestore(string fault)
    {
        var c = PureCorpus(); var good = Shape(); var g = good.GenerationSettings; var n = good.NoisePolicy; var cfg = good.CurrentConfiguration; var p = good.EvaluationProfile; var s = good.ScorerIdentity;
        var bad = fault switch
        {
            "id-null" => good with { EvaluatorId = null! }, "id-empty" => good with { EvaluatorId = "" },
            "id-leading" => good with { EvaluatorId = "-invalid" }, "id-space" => good with { EvaluatorId = "has space" },
            "id-unicode" => good with { EvaluatorId = "évaluator" }, "id-129" => good with { EvaluatorId = new('a', 129) },
            "version-65" => good with { Version = new('a', 65) }, "version-null" => good with { Version = null! },
            "configuration-pin" => good with { ConfigurationSha256 = Id('4') }, "exam-pin" => good with { ExamConditionSha256 = Hex('A') },
            "generation-pin" => good with { GenerationSettingsSha256 = "short" }, "noise-pin" => good with { NoisePolicyIdentitySha256 = Hex('0') },
            "null-generation" => good with { GenerationSettings = null! }, "null-noise" => good with { NoisePolicy = null! },
            "null-profile" => good with { EvaluationProfile = null! }, "null-config" => good with { CurrentConfiguration = null! }, "null-scorer" => good with { ScorerIdentity = null! },
            "tokens" => good with { GenerationSettings = g with { MaxNewTokens = 2049 } }, "top-k" => good with { GenerationSettings = g with { TopK = 10001 } },
            "maximum-cases" => good with { GenerationSettings = g with { MaxHeldOutCases = 201 } }, "maximum-answer" => good with { GenerationSettings = g with { MaxAnswerChars = 65537 } },
            "noise-repeat" => good with { NoisePolicy = n with { RepeatCount = 1 } }, "noise-multiplier" => good with { NoisePolicy = n with { StandardDeviationMultiplier = "1.250" } },
            "noise-zero" => good with { NoisePolicy = n with { StandardDeviationMultiplier = "0" } }, "noise-infinity" => good with { NoisePolicy = n with { StandardDeviationMultiplier = "Infinity" } },
            "model" => good with { CurrentConfiguration = cfg with { ModelId = "unapproved" } }, "manifest" => good with { CurrentConfiguration = cfg with { ExpectedOllamaManifestDigest = Id('a') } },
            "layer" => good with { CurrentConfiguration = cfg with { ExpectedOllamaModelLayerDigest = Hex('A') } }, "budget-zero" => good with { CurrentConfiguration = cfg with { MaxTeacherCalls = 0 } },
            "budget-hard" => good with { CurrentConfiguration = cfg with { MaxTeacherCalls = 10001 } }, "budget-joint" => good with { CurrentConfiguration = cfg with { MaxTeacherCalls = 3 } },
            "retention" => good with { CurrentConfiguration = cfg with { LocalRetentionPolicyRef = Hex('c') } }, "license" => good with { CurrentConfiguration = cfg with { OutputLicenseRef = Id('A') } },
            "dpo" => good with { CurrentConfiguration = cfg with { DpoMaxStepCount = 0 } },
            "scorer-domain" => good with { ScorerIdentity = s with { DomainTag = "bug" } }, "scorer-model" => good with { ScorerIdentity = s with { ScorerId = "other:model" } },
            "scorer-manifest" => good with { ScorerIdentity = s with { ScorerVersion = Hex('6') } }, "scorer-profile" => good with { ScorerIdentity = s with { NormalizationProfileId = "other:profile" } },
            "scorer-metric" => good with { ScorerIdentity = s with { MetricProfileId = "other:metric" } }, "scorer-long" => good with { ScorerIdentity = s with { DomainTag = new('a', 129) } },
            "scorer-null" => good with { ScorerIdentity = s with { MetricProfileId = null! } },
            "profile-id" => good with { EvaluationProfile = p with { ProfileId = "other:profile" } }, "profile-purpose" => good with { EvaluationProfile = p with { Purpose = "training_authoring" } },
            "profile-format" => good with { EvaluationProfile = p with { FormatIdentity = "other" } }, "profile-mode" => good with { EvaluationProfile = p with { RequestFormatMode = "other" } },
            "profile-context" => good with { EvaluationProfile = p with { NumContext = 4096 } }, "profile-temperature" => good with { EvaluationProfile = p with { Temperature = "-0" } },
            "profile-seed" => good with { EvaluationProfile = p with { Seed = int.MinValue } }, "profile-think" => good with { EvaluationProfile = p with { Think = true } },
            "profile-predict" => good with { EvaluationProfile = p with { NumPredict = 257 } }, "profile-timeout" => good with { EvaluationProfile = p with { RequestTimeoutSeconds = "121" } },
            "profile-hash" => good with { EvaluationProfile = p with { ConfigurationSha256 = Hex('6') } }, _ => throw new InvalidOperationException(fault),
        };
        Assert.Equal(CodeDocumentationOneLessonEvaluatorSelection.InvalidErrorCode, CodeDocumentationOneLessonEvaluatorSelection.Create(c, Plan(c), bad).ErrorCode);
        AssertLiteral(Select(c, good));
    }

    [Theory]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("-Infinity")]
    [InlineData("0.0")]
    [InlineData(" 0")]
    [InlineData("0 ")]
    [InlineData("0e0")]
    [InlineData("+0")]
    [InlineData("0,5")]
    [InlineData("３")]
    [InlineData(null)]
    [InlineData("12345678901234567890123456789012345")]
    [InlineData("2.1")]
    public void GenerationTemperature_RejectsNonfiniteNoncanonicalAndOwnerBounds(string? text)
    {
        var c = PureCorpus(); var e = Shape();
        Assert.False(CodeDocumentationOneLessonEvaluatorSelection.Create(c, Plan(c), e with { GenerationSettings = e.GenerationSettings with { Temperature = text! } }).IsSuccess);
        AssertLiteral(Select(c, e));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-0")]
    [InlineData("1.01")]
    [InlineData("1.0")]
    [InlineData("NaN")]
    public void GenerationTopP_UsesCanonicalStringAndActualOwnerOpenLowerBound(string text)
    {
        var c = PureCorpus(); var e = Shape();
        Assert.False(CodeDocumentationOneLessonEvaluatorSelection.Create(c, Plan(c), e with { GenerationSettings = e.GenerationSettings with { TopP = text } }).IsSuccess);
        AssertLiteral(Select(c, e));
    }

    [Theory]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("-1")]
    [InlineData("0")]
    [InlineData("-0")]
    [InlineData("1.250")]
    [InlineData(" 1.25")]
    [InlineData("1.25e0")]
    [InlineData(null)]
    public void NoiseDoubleR_RejectsNonfiniteAliasesAndRealConstructorBounds(string? text)
    {
        var c = PureCorpus(); var e = Shape();
        Assert.False(CodeDocumentationOneLessonEvaluatorSelection.Create(c, Plan(c), e with { NoisePolicy = e.NoisePolicy with { StandardDeviationMultiplier = text! } }).IsSuccess);
        AssertLiteral(Select(c, e));
    }

    public static IEnumerable<object[]> FreshFaults()
        => new[] { "null-settings", "invalid-settings", "null-config", "null-noise", "configuration-pin", "exam-pin", "malformed-pin", "tokens", "temperature", "top-k", "top-p", "seed", "maximum-cases", "maximum-answer", "noise-repeat", "noise-multiplier", "model", "manifest", "layer", "budget", "retention", "license", "dpo" }.Select(s => new object[] { s });

    [Theory]
    [MemberData(nameof(FreshFaults))]
    public void Fresh_EveryActualOwnerFieldMismatchRejectsBeforeMissingRootAndRestorationPasses(string fault)
    {
        using var f = new Fixture(2); var g = Settings(); var cfg = Config(8); var noise = Noise(); var selected = f.Select(g, cfg, noise);
        CodeDocumentationSavedModelAssessmentGenerationSettings? changed = fault switch
        {
            "null-settings" => null, "invalid-settings" => g with { Temperature = float.NaN }, "tokens" => g with { MaxNewTokens = 2 },
            "temperature" => g with { Temperature = 0.5f }, "top-k" => g with { TopK = 1 }, "top-p" => g with { TopP = 0.5f },
            "seed" => g with { Seed = int.MaxValue }, "maximum-cases" => g with { MaxHeldOutCases = 199 }, "maximum-answer" => g with { MaxAnswerChars = 2 }, _ => g,
        };
        var ownerCfg = fault switch
        {
            "null-config" => null, "model" => Config(8, model: "qwen-code-doc-other:9b"), "manifest" => Config(8, manifest: Hex('6')),
            "layer" => Config(8, layer: Hex('6')), "budget" => Config(9), "retention" => Config(8, retention: Id('6')),
            "license" => Config(8, license: Id('6')), "dpo" => Config(8, dpo: cfg.DpoMaxStepCount + 1), _ => cfg,
        };
        var ownerNoise = fault switch { "null-noise" => null, "noise-repeat" => new MeasurementNoisePolicyConfig(3, 1.25), "noise-multiplier" => new MeasurementNoisePolicyConfig(2, 2), _ => noise };
        var missing = Path.Combine(f.Repository, "missing-owner");
        var rejected = selected.OpenFresh(missing, changed, ownerCfg, ownerNoise, fault == "configuration-pin" ? Hex('6') : fault == "malformed-pin" ? Id('4') : Hex('4'), fault == "exam-pin" ? Hex('6') : Hex('5'));
        Assert.Equal(CodeDocumentationOneLessonEvaluatorSelection.InvalidErrorCode, rejected.ErrorCode); Assert.Null(rejected.Value); Assert.False(Directory.Exists(missing));
        Assert.True(selected.OpenFresh(f.Repository, g, cfg, noise, Hex('4'), Hex('5')).IsSuccess);
    }

    [Theory]
    [InlineData("manifest")]
    [InlineData("payload")]
    [InlineData("entry")]
    [InlineData("provenance")]
    public void Fresh_StrictActualOwnerRejectsChangedBytesAndRestorePasses(string fault)
    {
        using var f = new Fixture(2); var selected = f.Select(Settings(), Config(8), Noise());
        Assert.True(selected.OpenFresh(f.Repository, Settings(), Config(8), Noise(), Hex('4'), Hex('5')).IsSuccess);
        var path = fault == "manifest" ? f.Address.ManifestPath : f.PayloadPath; var original = File.ReadAllBytes(path);
        try
        {
            if (fault is "entry" or "provenance" or "manifest")
            {
                var node = JsonNode.Parse(original)!;
                var changed = ChangeFirstNamedValue(node, fault == "entry" ? "EntryId" : fault == "manifest" ? "CorpusName" : "ProviderRef", fault == "entry" ? Id('f') : fault == "manifest" ? "changed-corpus-name" : "changed-provider");
                Assert.True(changed); File.WriteAllText(path, node.ToJsonString());
            }
            else File.WriteAllBytes(path, original.Concat(new byte[] { 32 }).ToArray());
            var owner = RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedFileIdentity(f.Address); Assert.False(owner.IsSuccess);
            var rejected = selected.OpenFresh(f.Repository, Settings(), Config(8), Noise(), Hex('4'), Hex('5'));
            Assert.Equal(owner.ErrorCode, rejected.ErrorCode); Assert.Equal(owner.ErrorMessage, rejected.ErrorMessage);
        }
        finally { File.WriteAllBytes(path, original); }
        Assert.True(selected.OpenFresh(f.Repository, Settings(), Config(8), Noise(), Hex('4'), Hex('5')).IsSuccess);
        Assert.Equal("neighbor preserved", File.ReadAllText(f.Neighbor));
    }

    [Fact]
    public void Fresh_GenerationPinUsesAllActualHeldOutEntriesAndBadPinThenRestoreIsNotOwnerStub()
    {
        using var f = new Fixture(2); var good = f.Select(Settings(), Config(8), Noise());
        var subsetHash = CodeDocumentationSavedModelAssessmentService.ComputeGenerationSettingsSha256(Settings(), f.Corpus.Entries.Where(e => e.Partition == FrozenReferencePartition.HeldOut).Take(1).ToArray());
        Assert.NotEqual(subsetHash, good.Evaluator.GenerationSettingsSha256);
        var bad = Select(f.Selection(), good.Evaluator with { GenerationSettingsSha256 = subsetHash });
        Assert.False(bad.OpenFresh(f.Repository, Settings(), Config(8), Noise(), Hex('4'), Hex('5')).IsSuccess);
        Assert.True(good.OpenFresh(f.Repository, Settings(), Config(8), Noise(), Hex('4'), Hex('5')).IsSuccess);
        var actual201 = new Fixture(201);
        using (actual201)
        {
            var first200 = Require(CodeDocumentationOneLessonCorpusSelection.Create(actual201.Descriptor, actual201.HeldOutIds[..200]));
            var settings = Settings(); var selected = Select(first200, Shape(settings, Config(800), Noise(), Hex('6')));
            var owner = first200.OpenFresh(actual201.Repository, settings); Assert.False(owner.IsSuccess);
            Assert.Equal(owner.ErrorCode, selected.OpenFresh(actual201.Repository, settings, Config(800), Noise(), Hex('4'), Hex('5')).ErrorCode);
        }
    }

    [Fact]
    public void Catalog_FreezesAllFiveAttachmentsAndKeepsLegacyWireAndOptionalCompatibility()
    {
        var c = PureCorpus(); var selected = Select(c, Shape()); var grant = Grant(c, selected);
        var source = Source(); var checkpoint = Checkpoint();
        grant = grant with { SourceSelection = source, CheckpointSelection = checkpoint,
            CandidateSha256 = checkpoint.Candidate.CandidateSha256, ParentManifestSha256 = checkpoint.Parent.ManifestSha256,
            ParentWeightsSha256 = checkpoint.Parent.WeightsSha256, DescriptorsV2 = grant.DescriptorsV2! with {
                SourceSelectionSha256 = source.SourceSelectionSha256, ParentCheckpointSha256 = checkpoint.ParentCheckpointSha256, CandidateSelectionSha256 = checkpoint.CandidateSelectionSha256 } };
        var catalog = Catalog(grant); var frozen = Require(catalog.Resolve(Selector()));
        Assert.Same(frozen, Require(catalog.Revalidate(frozen.GrantId, frozen.GrantVersion)));
        Assert.NotSame(source, frozen.SourceSelection); Assert.NotSame(checkpoint, frozen.CheckpointSelection);
        Assert.NotSame(c, frozen.CorpusSelection); Assert.NotSame(grant.EvaluationPlanSelection, frozen.EvaluationPlanSelection);
        Assert.NotSame(selected, frozen.EvaluatorSelection); Assert.NotSame(selected.Evaluator, frozen.EvaluatorSelection!.Evaluator);
        Assert.Equal(source.SourceSelectionSha256, frozen.SourceSelection!.SourceSelectionSha256);
        Assert.Equal(checkpoint.ParentCheckpointSha256, frozen.CheckpointSelection!.ParentCheckpointSha256);
        Assert.Equal(checkpoint.CandidateSelectionSha256, frozen.CheckpointSelection.CandidateSelectionSha256);
        Assert.Equal(c.CorpusDescriptorSha256, frozen.CorpusSelection!.CorpusDescriptorSha256);
        Assert.Equal(grant.EvaluationPlanSelection!.PlanSha256, frozen.EvaluationPlanSelection!.PlanSha256);
        Assert.Equal(selected.EvaluatorProfileSha256, frozen.EvaluatorSelection.EvaluatorProfileSha256); AssertLiteral(frozen.EvaluatorSelection);
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(grant));
        Assert.Equal(new[] { "GrantId", "GrantVersion", "GrantSelector", "ObjectiveId", "TrainingRunId", "CandidateSha256", "ParentManifestSha256", "ParentWeightsSha256", "ConfigurationSha256", "ExamConditionSha256" }, json.RootElement.EnumerateObject().Select(p => p.Name));
        Assert.True(Catalog(grant with { EvaluatorSelection = null }).Resolve(Selector()).IsSuccess);
        var legacy = grant with { FormatVersion = 1, DescriptorsV2 = null, SourceSelection = null, CheckpointSelection = null, CorpusSelection = null, EvaluationPlanSelection = null, EvaluatorSelection = null };
        Assert.True(Catalog(legacy).Resolve(Selector()).IsSuccess);
    }

    [Theory]
    [InlineData("v1")]
    [InlineData("missing-corpus")]
    [InlineData("missing-plan")]
    [InlineData("evaluator-hash")]
    [InlineData("plan-hash")]
    [InlineData("count")]
    [InlineData("repeat")]
    [InlineData("configuration-pin")]
    [InlineData("exam-pin")]
    [InlineData("budget")]
    [InlineData("full-plan-id")]
    public void Catalog_RejectsVersionDependencyFullCompactScalarAndProtectedBudgetMismatchesThenRestores(string fault)
    {
        var c = PureCorpus(); var selected = Select(c, Shape()); var grant = Grant(c, selected);
        var bad = fault switch
        {
            "v1" => grant with { FormatVersion = 1, DescriptorsV2 = null, CorpusSelection = null, EvaluationPlanSelection = null },
            "missing-corpus" => grant with { CorpusSelection = null }, "missing-plan" => grant with { EvaluationPlanSelection = null },
            "evaluator-hash" => grant with { DescriptorsV2 = grant.DescriptorsV2! with { EvaluatorProfileSha256 = Hex('6') } },
            "plan-hash" => grant with { DescriptorsV2 = grant.DescriptorsV2! with { EvaluationPlanSha256 = Hex('6') } },
            "count" => grant with { DescriptorsV2 = grant.DescriptorsV2! with { CaseCount = 2 } }, "repeat" => grant with { DescriptorsV2 = grant.DescriptorsV2! with { RepeatCount = 3 } },
            "configuration-pin" => grant with { ConfigurationSha256 = Hex('6') }, "exam-pin" => grant with { ExamConditionSha256 = Hex('6') },
            "full-plan-id" => grant with { EvaluationPlanSelection = Plan(c, evaluator: "other:evaluator"), DescriptorsV2 = grant.DescriptorsV2! with { EvaluationPlanSha256 = Plan(c, evaluator: "other:evaluator").PlanSha256 } },
            _ => grant,
        };
        Assert.Equal(CodeDocumentationOneLessonGrantCatalog.InvalidCatalogErrorCode, Catalog(bad, fault == "budget" ? 9 : 8).Resolve(Selector()).ErrorCode);
        Assert.True(Catalog(grant).Resolve(Selector()).IsSuccess);
    }

    [Fact]
    public void DefaultWriter_RemainsNonnegativeInt64AndLiteralSourceCorpusPlanPreimagesStayUnchanged()
    {
        using (var json = JsonDocument.Parse("{\"Seed\":2147483648}")) Assert.Equal("{\"Seed\":2147483648}", Encoding.UTF8.GetString(CodeDocumentationOneLessonV2Wire.CanonicalBytes(json.RootElement)));
        foreach (var raw in new[] { "{\"Seed\":-1}", "{\"Seed\":1.5}", "{\"Seed\":[]}", "{\"Seed\":null}", "{\"Seed\":9223372036854775808}" })
        {
            using var json = JsonDocument.Parse(raw); Assert.Throws<JsonException>(() => CodeDocumentationOneLessonV2Wire.CanonicalBytes(json.RootElement));
        }
        var sourceLiteral = "{\"PairSelections\":[{\"SourceId\":\"source0\",\"TrainingPairId\":\"" + Hex('a') + "\"}],\"Sources\":[{\"DatasetVersion\":\"v1\",\"DesignPatternSlug\":\"learning-unit\",\"DomainSlug\":\"bug\",\"ManifestFileByteLength\":1,\"ManifestFileSha256\":\"" + Hex('1') + "\",\"Role\":\"TrainingPairs\",\"SealedManifestSha256\":\"" + Hex('2') + "\",\"SourceId\":\"source0\"}]}";
        Assert.Equal(Encoding.UTF8.GetBytes(sourceLiteral), CodeDocumentationOneLessonV2Wire.CanonicalSourceSelectionBytes(Source()));
        var c = PureCorpus(); var corpusLiteral = "{\"CaseIds\":[\"" + Id('1') + "\"],\"Corpus\":{\"CorpusId\":\"opaque:corpus\",\"CorpusSha256\":\"" + Hex('2') + "\",\"CorpusVersion\":\"v1\",\"DomainSlug\":\"translation\",\"ManifestFileByteLength\":1,\"ManifestFileSha256\":\"" + Hex('1') + "\",\"PayloadByteLength\":1,\"PayloadSha256\":\"" + Hex('3') + "\",\"Purpose\":\"ClosedEvaluation\",\"SubjectSlug\":\"en-ru\"}}";
        Assert.Equal(Encoding.UTF8.GetBytes(corpusLiteral), CodeDocumentationOneLessonV2Wire.CanonicalCorpusSelectionBytes(c));
        var planLiteral = "{\"CaseIds\":[\"" + Id('1') + "\"],\"CorpusId\":\"opaque:corpus\",\"EvaluatorId\":\"evaluator:1\",\"ExpectedEvaluationCallCount\":2,\"PlanId\":\"plan:1\",\"RepeatCount\":2}";
        Assert.Equal(Encoding.UTF8.GetBytes(planLiteral), CodeDocumentationOneLessonV2Wire.CanonicalEvaluationPlanBytes(Plan(c)));
        Assert.Equal(Sha(Encoding.UTF8.GetBytes(planLiteral)), Plan(c).PlanSha256);
        AssertLiteral(Select(c, Shape()));
    }

    private static string Literal(CodeDocumentationOneLessonEvaluatorV2 e)
    {
        var c = e.CurrentConfiguration; var p = e.EvaluationProfile; var g = e.GenerationSettings; var n = e.NoisePolicy; var s = e.ScorerIdentity;
        return "{\"ConfigurationSha256\":" + Q(e.ConfigurationSha256) + ",\"CurrentConfiguration\":{\"DpoMaxStepCount\":" + I(c.DpoMaxStepCount) +
            ",\"ExpectedOllamaManifestDigest\":" + Q(c.ExpectedOllamaManifestDigest) + ",\"ExpectedOllamaModelLayerDigest\":" + Q(c.ExpectedOllamaModelLayerDigest) +
            ",\"LocalRetentionPolicyRef\":" + Q(c.LocalRetentionPolicyRef) + ",\"MaxTeacherCalls\":" + I(c.MaxTeacherCalls) + ",\"ModelId\":" + Q(c.ModelId) + ",\"OutputLicenseRef\":" + Q(c.OutputLicenseRef) +
            "},\"EvaluationProfile\":{\"ConfigurationSha256\":" + Q(p.ConfigurationSha256) + ",\"FormatIdentity\":" + Q(p.FormatIdentity) + ",\"NumContext\":" + I(p.NumContext) +
            ",\"NumPredict\":" + I(p.NumPredict) + ",\"ProfileId\":" + Q(p.ProfileId) + ",\"Purpose\":" + Q(p.Purpose) + ",\"RequestFormatMode\":" + Q(p.RequestFormatMode) +
            ",\"RequestTimeoutSeconds\":" + Q(p.RequestTimeoutSeconds) + ",\"Seed\":" + I(p.Seed) + ",\"Temperature\":" + Q(p.Temperature) + ",\"Think\":" + (p.Think ? "true" : "false") +
            "},\"EvaluatorId\":" + Q(e.EvaluatorId) + ",\"ExamConditionSha256\":" + Q(e.ExamConditionSha256) + ",\"GenerationSettings\":{\"MaxAnswerChars\":" + I(g.MaxAnswerChars) +
            ",\"MaxHeldOutCases\":" + I(g.MaxHeldOutCases) + ",\"MaxNewTokens\":" + I(g.MaxNewTokens) + ",\"Seed\":" + I(g.Seed) + ",\"Temperature\":" + Q(g.Temperature) + ",\"TopK\":" + I(g.TopK) + ",\"TopP\":" + Q(g.TopP) +
            "},\"GenerationSettingsSha256\":" + Q(e.GenerationSettingsSha256) + ",\"NoisePolicy\":{\"RepeatCount\":" + I(n.RepeatCount) + ",\"StandardDeviationMultiplier\":" + Q(n.StandardDeviationMultiplier) +
            "},\"NoisePolicyIdentitySha256\":" + Q(e.NoisePolicyIdentitySha256) + ",\"ScorerIdentity\":{\"DomainTag\":" + Q(s.DomainTag) + ",\"MetricProfileId\":" + Q(s.MetricProfileId) +
            ",\"NormalizationProfileId\":" + Q(s.NormalizationProfileId) + ",\"ScorerId\":" + Q(s.ScorerId) + ",\"ScorerVersion\":" + Q(s.ScorerVersion) + "},\"Version\":" + Q(e.Version) + "}";
    }
    private static void AssertLiteral(CodeDocumentationOneLessonEvaluatorSelection s)
    {
        var literal = Literal(s.Evaluator); Assert.Equal(Encoding.UTF8.GetBytes(literal), CodeDocumentationOneLessonV2Wire.CanonicalEvaluatorProfileBytes(s));
        Assert.Equal(Sha(Encoding.UTF8.GetBytes("xiigen-s04-evaluatorprofile-v2\n" + literal)), s.EvaluatorProfileSha256);
        using var json = JsonDocument.Parse(literal);
        Assert.Equal(new[] { "ConfigurationSha256", "CurrentConfiguration", "EvaluationProfile", "EvaluatorId", "ExamConditionSha256", "GenerationSettings", "GenerationSettingsSha256", "NoisePolicy", "NoisePolicyIdentitySha256", "ScorerIdentity", "Version" }, json.RootElement.EnumerateObject().Select(p => p.Name));
        Assert.False(json.RootElement.TryGetProperty("EvaluatorProfileSha256", out _));
        Assert.Equal(JsonValueKind.String, json.RootElement.GetProperty("GenerationSettings").GetProperty("Temperature").ValueKind);
    }
    private static string Q(string text) => "\"" + text + "\"";
    private static string I(int n) => n.ToString(CultureInfo.InvariantCulture);
    private static string Hex(char c) => new(c, 64);
    private static string Id(char c) => "sha256:" + Hex(c);
    private static string Sha(byte[] b) => Convert.ToHexString(SHA256.HashData(b)).ToLowerInvariant();
    private static T Require<T>(OperationResult<T> r) { Assert.True(r.IsSuccess, r.ErrorCode + ": " + r.ErrorMessage); Assert.NotNull(r.Value); return r.Value!; }
    private static CodeDocumentationSavedModelAssessmentGenerationSettings Settings() => new(1, 0, 0, 1, 0, 200, 1);
    private static MeasurementNoisePolicyConfig Noise() => new(2, 1.25);
    private static ContinuousLearningTp1DpoConfiguration Config(int budget, string model = "qwen-code-doc-quality-test:9b", string? manifest = null, string? layer = null, string? retention = null, string? license = null, int dpo = StableDpoTrainingExecutor.DefaultDpoStepCount)
        => Require(ContinuousLearningTp1DpoConfiguration.Create(model, manifest ?? Hex('a'), layer ?? Hex('b'), budget, retention ?? Id('c'), license ?? Id('d'), Enumerable.Range(1, 32).Select(v => checked((byte)v)).ToArray(), dpo));
    private static CodeDocumentationOneLessonEvaluatorV2 Shape(CodeDocumentationSavedModelAssessmentGenerationSettings? g = null, ContinuousLearningTp1DpoConfiguration? c = null, MeasurementNoisePolicyConfig? n = null, string? generationHash = null)
    {
        g ??= Settings(); c ??= Config(8); n ??= Noise(); var p = LocalQwenOllamaInferenceProfileAuthority.EvaluationDescriptor;
        return new("evaluator:1", "v2", Hex('4'), Hex('5'), generationHash ?? Hex('6'), n.IdentitySha256,
            CodeDocumentationSavedModelAssessmentCalibrationService.BuildCurrentScorerIdentity("translation", c.ModelId, c.ExpectedOllamaManifestDigest),
            new(g.MaxNewTokens, g.Temperature.ToString("R", CultureInfo.InvariantCulture), g.TopK, g.TopP.ToString("R", CultureInfo.InvariantCulture), g.Seed, g.MaxHeldOutCases, g.MaxAnswerChars),
            new(n.RepeatCount, n.StandardDeviationMultiplier.ToString("R", CultureInfo.InvariantCulture)),
            new(p.ProfileId, p.Purpose, p.FormatIdentity, p.RequestFormatMode.ToString(), p.NumContext, p.Temperature.ToString("R", CultureInfo.InvariantCulture), p.Seed, p.Think!.Value, p.NumPredict!.Value, p.RequestTimeout.TotalSeconds.ToString("R", CultureInfo.InvariantCulture), p.Evidence.ConfigurationSha256),
            new(c.ModelId, c.ExpectedOllamaManifestDigest, c.ExpectedOllamaModelLayerDigest, c.MaxTeacherCalls, c.LocalRetentionPolicyRef, c.OutputLicenseRef, c.DpoMaxStepCount));
    }
    private static CodeDocumentationOneLessonCorpusSelection PureCorpus(IEnumerable<string>? ids = null)
        => Require(CodeDocumentationOneLessonCorpusSelection.Create(new("opaque:corpus", "ClosedEvaluation", "translation", "en-ru", "v1", Hex('1'), Hex('2'), 1, Hex('3'), 1), ids ?? new[] { Id('1') }));
    private static CodeDocumentationOneLessonEvaluationPlanSelection Plan(CodeDocumentationOneLessonCorpusSelection c, int repeat = 2, string evaluator = "evaluator:1")
        => Require(CodeDocumentationOneLessonEvaluationPlanSelection.Create(c, "plan:1", evaluator, repeat, checked(c.CaseIds.Count * repeat)));
    private static CodeDocumentationOneLessonEvaluatorSelection Select(CodeDocumentationOneLessonCorpusSelection c, CodeDocumentationOneLessonEvaluatorV2 e)
        => Require(CodeDocumentationOneLessonEvaluatorSelection.Create(c, Plan(c, e.NoisePolicy.RepeatCount, e.EvaluatorId), e));
    private static CodeDocumentationOneLessonSourceSelection Source() => Require(CodeDocumentationOneLessonSourceSelection.Create([new("source0", "TrainingPairs", "bug", "learning-unit", "v1", Hex('1'), 1, Hex('2'))], [new(Hex('a'), "source0")]));
    private static CodeDocumentationOneLessonCheckpointSelection Checkpoint() => Require(CodeDocumentationOneLessonCheckpointSelection.Create(new(1, "bug", "learning-unit", "tiny-model", "v1", Hex('1'), Hex('2'), 7), new("candidate:1", Hex('3'))));
    private static CodeDocumentationOneLessonSelectorV1 Selector() => new("objective", "selector");
    private static CodeDocumentationOneLessonGrantV1 Grant(CodeDocumentationOneLessonCorpusSelection c, CodeDocumentationOneLessonEvaluatorSelection selected)
        => new("grant", "v2", "selector", "objective", "evaluator-profile-run", Hex('1'), Hex('2'), Hex('3'), Hex('4'), Hex('5'))
        { FormatVersion = 2, CorpusSelection = c, EvaluationPlanSelection = Plan(c), EvaluatorSelection = selected,
            DescriptorsV2 = new(2, Hex('1'), Hex('2'), Hex('3'), c.CorpusDescriptorSha256, Plan(c).PlanSha256, c.CaseIds.Count, 2, selected.EvaluatorProfileSha256, Hex('7')) };
    private static CodeDocumentationOneLessonGrantCatalog Catalog(CodeDocumentationOneLessonGrantV1 g, int budget = 8) => CodeDocumentationOneLessonGrantCatalog.FromTrustedGrants([g], budget);
    private void LogRuntimeIdentities()
    {
        var tests = typeof(CodeDocumentationOneLessonEvaluatorSelectionTests).Assembly;
        foreach (var identity in new[] { (Kind: "production-dll", Path: typeof(RepositoryOwnedReferenceCorpusManifestSealer).Assembly.Location), (Kind: "test-dll", Path: tests.Location), (Kind: "runtimeconfig", Path: Path.ChangeExtension(tests.Location, ".runtimeconfig.json")), (Kind: "deps", Path: Path.ChangeExtension(tests.Location, ".deps.json")) })
        {
            var bytes = File.ReadAllBytes(identity.Path);
            output.WriteLine("EVALUATOR_PROFILE_RUNTIME " + JsonSerializer.Serialize(new { kind = identity.Kind, path = identity.Path, sha256 = Sha(bytes), byteLength = bytes.LongLength }));
        }
    }
    private static bool ChangeFirstNamedValue(JsonNode node, string name, string replacement)
    {
        if (node is JsonObject obj)
        {
            var key = obj.Select(p => p.Key).FirstOrDefault(k => string.Equals(k, name, StringComparison.OrdinalIgnoreCase));
            if (key is not null) { obj[key] = replacement; return true; }
            foreach (var child in obj.Select(p => p.Value).Where(n => n is not null)) if (ChangeFirstNamedValue(child!, name, replacement)) return true;
        }
        if (node is JsonArray array) foreach (var child in array.Where(n => n is not null)) if (ChangeFirstNamedValue(child!, name, replacement)) return true;
        return false;
    }
    private sealed class Fixture : IDisposable
    {
        private string Root { get; } = Path.Combine(Path.GetTempPath(), "evaluator-profile-" + Guid.NewGuid().ToString("N"));
        internal string Repository { get; }
        internal string Neighbor => Path.Combine(Root, "neighbor-sentinel.txt");
        internal RepositoryOwnedReferenceCorpusAddress Address { get; }
        internal string PayloadPath => Path.Combine(Address.CorpusRoot, RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName);
        internal FrozenReferenceCorpus Corpus { get; }
        internal CodeDocumentationOneLessonCorpusV2 Descriptor { get; }
        internal string[] HeldOutIds => Corpus.Entries.Where(e => e.Partition == FrozenReferencePartition.HeldOut).Select(e => e.EntryId).OrderBy(x => x, StringComparer.Ordinal).ToArray();
        internal Fixture(int count)
        {
            Repository = Path.Combine(Root, "repo"); Directory.CreateDirectory(Repository); File.WriteAllText(Neighbor, "neighbor preserved");
            foreach (var name in new[] { "LlmFromScratch.sln", "llm_mvp_core.sln", "AGENTS.md" }) File.WriteAllText(Path.Combine(Repository, name), "synthetic fixture sentinel");
            Directory.CreateDirectory(Path.Combine(Repository, "14.ContinuousLearning")); Directory.CreateDirectory(Path.Combine(Repository, "11.DPO"));
            Corpus = Require(FrozenReferenceCorpus.Build("human-corpus-name", "v1", new(2026, 10, 3, 12, 0, 0, TimeSpan.Zero), Enumerable.Range(0, count).Select(i => Entry("heldout-" + i, FrozenReferencePartition.HeldOut)).Prepend(Entry("train", FrozenReferencePartition.Train))));
            Address = Require(RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(Repository, "translation", "en-ru", "v1"));
            var manifest = Require(RepositoryOwnedReferenceCorpusManifestSealer.PersistAndSeal(Address, Corpus)); var bytes = File.ReadAllBytes(Address.ManifestPath);
            Assert.True(FrozenReferenceCanonicalization.IsCanonicalSha256Ref(manifest.CorpusSha256));
            Descriptor = new("opaque:catalog-key", "ClosedEvaluation", manifest.Domain, manifest.Subject, manifest.CorpusVersion, Sha(bytes), manifest.CorpusSha256[7..], bytes.LongLength, manifest.PayloadSha256, manifest.PayloadByteLength);
        }
        internal CodeDocumentationOneLessonCorpusSelection Selection() => Require(CodeDocumentationOneLessonCorpusSelection.Create(Descriptor, HeldOutIds));
        internal CodeDocumentationOneLessonEvaluatorSelection Select(CodeDocumentationSavedModelAssessmentGenerationSettings settings, ContinuousLearningTp1DpoConfiguration config, MeasurementNoisePolicyConfig noise)
        {
            var heldOut = Corpus.Entries.Where(e => e.Partition == FrozenReferencePartition.HeldOut).OrderBy(e => e.EntryId, StringComparer.Ordinal).ToArray();
            return CodeDocumentationOneLessonEvaluatorSelectionTests.Select(Selection(), Shape(settings, config, noise, CodeDocumentationSavedModelAssessmentService.ComputeGenerationSettingsSha256(settings, heldOut)));
        }
        private static FrozenReferenceEntry Entry(string seed, FrozenReferencePartition partition)
        {
            var answer = "synthetic closed reference " + seed;
            var provenance = Require(PaidReferenceProvenance.Create("synthetic-paid-provider", "synthetic-paid-model", "v1", "request-" + seed, new(2026, 10, 3, 10, 0, 0, TimeSpan.Zero), FrozenReferenceEntry.ComputeNormalizedTextSha256(answer)));
            return Require(FrozenReferenceEntry.Create("synthetic owner input " + seed, answer, provenance, "translation", "en-ru", partition, FrozenReferenceEntry.ComputeNormalizedTextSha256("independent-source-entity:" + seed)));
        }
        public void Dispose()
        {
            var target = Path.GetFullPath(Root); Assert.StartsWith(Path.GetFullPath(Path.GetTempPath()) + "evaluator-profile-", target);
            Assert.Equal("neighbor preserved", File.ReadAllText(Neighbor));
            Assert.StartsWith(target + Path.DirectorySeparatorChar, Path.GetFullPath(Repository));
            if (Directory.Exists(Repository)) Directory.Delete(Repository, recursive: true);
            Assert.Equal("neighbor preserved", File.ReadAllText(Neighbor));
            if (Directory.Exists(target)) Directory.Delete(target, recursive: true);
        }
    }
}
