// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace LlmFromScratch.Tests;

using System.Collections;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using ContinuousLearning;

public sealed class CodeDocumentationOneLessonEvaluationPlanSelectionTests(
    Xunit.Abstractions.ITestOutputHelper output)
{
    [Theory]
    [InlineData(1, 5000)]
    [InlineData(200, 25)]
    public void PurePlan_AllCasesAndJointBoundariesMatchIndependentPlainJcsOracle(int count, int repeat)
    {
        var ids = Enumerable.Range(0, count).Select(i => "sha256:" + i.ToString("x64", CultureInfo.InvariantCulture)).ToArray();
        var corpus = Require(CodeDocumentationOneLessonCorpusSelection.Create(Shape(), ids));
        var plan = Plan(corpus, repeat);
        Assert.Equal(5000, plan.ExpectedEvaluationCallCount);
        Assert.Equal(ids, plan.CaseIds);
        Assert.Equal("opaque:corpus-1", plan.CorpusId);
        AssertLiteral(plan);
        using var json = JsonDocument.Parse(CodeDocumentationOneLessonV2Wire.CanonicalEvaluationPlanBytes(plan));
        Assert.Equal(new[] { "CaseIds", "CorpusId", "EvaluatorId", "ExpectedEvaluationCallCount", "PlanId", "RepeatCount" },
            json.RootElement.EnumerateObject().Select(p => p.Name));
        Assert.False(json.RootElement.TryGetProperty("PlanSha256", out _));
        Assert.Equal(count, json.RootElement.GetProperty("CaseIds").GetArrayLength());
        Assert.NotEqual(DomainHash("xiigen-s04-evaluation-plan-v2\n", Oracle(plan)), plan.PlanSha256);
    }

    [Theory]
    [InlineData("plan")]
    [InlineData("evaluator")]
    [InlineData("repeat")]
    [InlineData("corpus")]
    [InlineData("cases")]
    public void PurePlan_EachIndependentFullFieldChangeChangesHash(string field)
    {
        var corpus = Require(CodeDocumentationOneLessonCorpusSelection.Create(Shape(), [Id('1'), Id('2')]));
        var original = Plan(corpus, 2);
        var changedCorpus = field switch
        {
            "corpus" => Require(CodeDocumentationOneLessonCorpusSelection.Create(Shape() with { CorpusId = "other:corpus" }, corpus.CaseIds)),
            "cases" => Require(CodeDocumentationOneLessonCorpusSelection.Create(Shape(), [Id('1'), Id('3')])),
            _ => corpus,
        };
        var repeat = field == "repeat" ? 3 : 2;
        var changed = Require(CodeDocumentationOneLessonEvaluationPlanSelection.Create(changedCorpus,
            field == "plan" ? "other:plan" : "plan:1", field == "evaluator" ? "other:evaluator" : "evaluator:1",
            repeat, changedCorpus.CaseIds.Count * repeat));
        AssertLiteral(original); AssertLiteral(changed);
        Assert.NotEqual(original.PlanSha256, changed.PlanSha256);
    }

    [Theory]
    [InlineData("null-corpus")]
    [InlineData("null-plan")]
    [InlineData("empty-plan")]
    [InlineData("leading-plan")]
    [InlineData("space-plan")]
    [InlineData("unicode-plan")]
    [InlineData("long-plan")]
    [InlineData("null-evaluator")]
    [InlineData("empty-evaluator")]
    [InlineData("leading-evaluator")]
    [InlineData("space-evaluator")]
    [InlineData("unicode-evaluator")]
    [InlineData("long-evaluator")]
    [InlineData("repeat-zero")]
    [InlineData("repeat-one")]
    [InlineData("repeat-negative")]
    [InlineData("pin-zero")]
    [InlineData("pin-negative")]
    [InlineData("pin-mismatch")]
    [InlineData("joint-bound")]
    [InlineData("overflow")]
    public void PurePlan_InvalidInputFailsAndRestoredCompletePlanPasses(string fault)
    {
        var count = fault == "joint-bound" ? 200 : fault == "overflow" ? 3 : 1;
        var corpus = Require(CodeDocumentationOneLessonCorpusSelection.Create(Shape(),
            Enumerable.Range(0, count).Select(i => "sha256:" + i.ToString("x64", CultureInfo.InvariantCulture))));
        AssertLiteral(Plan(corpus, 2));
        string? BadToken(string suffix) => suffix switch
        {
            "null" => null, "empty" => "", "leading" => "-bad", "space" => "bad token",
            "unicode" => "évaluator", "long" => new('a', 129), _ => throw new InvalidOperationException(suffix),
        };
        var planId = fault.EndsWith("-plan", StringComparison.Ordinal) ? BadToken(fault[..^5]) : "plan:1";
        var evaluator = fault.EndsWith("-evaluator", StringComparison.Ordinal) ? BadToken(fault[..^10]) : "evaluator:1";
        var repeat = fault switch { "repeat-zero" => 0, "repeat-one" => 1, "repeat-negative" => -1,
            "joint-bound" => 26, "overflow" => 1_431_655_766, _ => 2 };
        var expected = fault switch { "pin-zero" => 0, "pin-negative" => -1, "pin-mismatch" => 3,
            "joint-bound" => 5200, "overflow" => 2, _ => count * 2 };
        Assert.Equal(CodeDocumentationOneLessonEvaluationPlanSelection.InvalidErrorCode,
            CodeDocumentationOneLessonEvaluationPlanSelection.Create(fault == "null-corpus" ? null : corpus,
                planId, evaluator, repeat, expected).ErrorCode);
        AssertLiteral(Plan(corpus, 2));
    }

    [Fact]
    public void PurePlan_CopiesOwnerCasesAndReadOnlyWrapperCannotEscape()
    {
        var ids = new[] { Id('1'), Id('2') };
        var corpus = Require(CodeDocumentationOneLessonCorpusSelection.Create(Shape(), ids));
        var plan = Require(CodeDocumentationOneLessonEvaluationPlanSelection.Create(corpus,
            "A0._:-" + new string('z', 122), "B0._:-" + new string('x', 122), 2, 4));
        var bytes = CodeDocumentationOneLessonV2Wire.CanonicalEvaluationPlanBytes(plan);
        ids[0] = Id('9');
        Assert.NotSame(corpus.CaseIds, plan.CaseIds);
        Assert.Equal(new[] { Id('1'), Id('2') }, plan.CaseIds);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)plan.CaseIds)[0] = Id('a'));
        Assert.Throws<NotSupportedException>(() => ((IList)plan.CaseIds).Add(Id('a')));
        Assert.Equal(bytes, CodeDocumentationOneLessonV2Wire.CanonicalEvaluationPlanBytes(plan));
        AssertLiteral(plan);
    }

    [Fact]
    public void FreshPlan_RealOwnerSnapshotsAndFourLoadedRuntimeIdentities()
    {
        using var fixture = new Fixture(2);
        var corpus = fixture.Selection(); var plan = Plan(corpus, 3);
        var manifest = File.ReadAllBytes(fixture.Address.ManifestPath);
        var payload = File.ReadAllBytes(fixture.PayloadPath);
        var fresh = Require(plan.OpenFresh(fixture.Repository, Settings() with { Seed = int.MinValue }, Configuration(12), Noise(3)));
        Assert.Equal(corpus.Corpus.CorpusId, fresh.Selection.CorpusId);
        Assert.Equal(corpus.CaseIds, fresh.Selection.CaseIds);
        Assert.Equal(plan.PlanSha256, fresh.Selection.PlanSha256);
        Assert.Equal(fixture.Corpus.CorpusSha256, fresh.Corpus.CorpusSha256);
        Assert.Equal(fixture.Corpus.CanonicalRepresentation, fresh.Corpus.CanonicalRepresentation);
        Assert.Equal(new CodeDocumentationSavedModelAssessmentCalibrationPlan(3, 2, 6), fresh.QualityGatePlan.BaselinePlan);
        Assert.Equal(12, fresh.QualityGatePlan.ExpectedTotalEvaluationCallCount);
        Assert.Equal(manifest, File.ReadAllBytes(fixture.Address.ManifestPath));
        Assert.Equal(payload, File.ReadAllBytes(fixture.PayloadPath));
        AssertLiteral(fresh.Selection); AssertNoExecutionArtifacts(fixture); LogRuntimeIdentities();
    }

    [Theory]
    [InlineData("null-config")]
    [InlineData("null-noise")]
    [InlineData("repeat")]
    public void FreshPlan_CurrentPolicyDisagreementRejectsBeforeMissingRootOwnerIo(string fault)
    {
        using var fixture = new Fixture(1); var plan = Plan(fixture.Selection(), 2);
        Assert.True(plan.OpenFresh(fixture.Repository, Settings(), Configuration(4), Noise(2)).IsSuccess);
        var missing = Path.Combine(fixture.Repository, "missing-root");
        Assert.Equal(CodeDocumentationOneLessonEvaluationPlanSelection.InvalidErrorCode,
            plan.OpenFresh(missing, Settings(), fault == "null-config" ? null : Configuration(4),
                fault == "null-noise" ? null : Noise(fault == "repeat" ? 3 : 2)).ErrorCode);
        Assert.False(Directory.Exists(missing));
        Assert.NotEqual(CodeDocumentationOneLessonEvaluationPlanSelection.InvalidErrorCode,
            plan.OpenFresh(missing, Settings(), Configuration(4), Noise(2)).ErrorCode);
        Assert.True(plan.OpenFresh(fixture.Repository, Settings(), Configuration(4), Noise(2)).IsSuccess);
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(1, 5000, 9999)]
    [InlineData(200, 25, 9999)]
    public void FreshPlan_ActualJointBudgetFailurePropagatesAndExactBudgetRestores(int count, int repeat, int lowerBudget)
    {
        using var fixture = new Fixture(count); var corpus = fixture.Selection(); var plan = Plan(corpus, repeat);
        var config = Configuration(lowerBudget); var noise = Noise(repeat);
        Assert.True(CodeDocumentationSavedModelAssessmentCalibrationService.PlanCalibration(fixture.Corpus, Settings(), config, noise).IsSuccess);
        var owner = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.PlanQualityGate(fixture.Corpus, Settings(), config, noise);
        Assert.False(owner.IsSuccess);
        var result = plan.OpenFresh(fixture.Repository, Settings(), config, noise);
        Assert.Equal(owner.ErrorCode, result.ErrorCode); Assert.Equal(owner.ErrorMessage, result.ErrorMessage);
        var fresh = Require(plan.OpenFresh(fixture.Repository, Settings() with { Seed = int.MaxValue }, Configuration(count * repeat * 2), noise));
        Assert.Equal(count, fresh.QualityGatePlan.BaselinePlan.HeldOutCaseCount);
        Assert.Equal(repeat, fresh.QualityGatePlan.BaselinePlan.RepeatCount);
        Assert.Equal(count * repeat, fresh.QualityGatePlan.BaselinePlan.ExpectedEvaluationCallCount);
        Assert.Equal(count * repeat * 2, fresh.QualityGatePlan.ExpectedTotalEvaluationCallCount);
        Assert.Equal(corpus.CaseIds, fresh.Selection.CaseIds); AssertLiteral(fresh.Selection); AssertNoExecutionArtifacts(fixture);
    }

    [Theory]
    [InlineData("null-settings")]
    [InlineData("new-tokens")]
    [InlineData("temperature")]
    [InlineData("top-k")]
    [InlineData("top-p")]
    [InlineData("answer")]
    [InlineData("maximum")]
    [InlineData("manifest")]
    [InlineData("cases")]
    public void FreshPlan_CurrentGenerationAndCompleteCorpusErrorsComeFromSameOwnerThenRestore(string fault)
    {
        using var fixture = new Fixture(2); var goodCorpus = fixture.Selection(); var good = Plan(goodCorpus, 2);
        var config = Configuration(8); var noise = Noise(2);
        Assert.True(good.OpenFresh(fixture.Repository, Settings(), config, noise).IsSuccess);
        CodeDocumentationSavedModelAssessmentGenerationSettings? settings = fault switch
        {
            "null-settings" => null, "new-tokens" => Settings() with { MaxNewTokens = 0 },
            "temperature" => Settings() with { Temperature = float.NaN }, "top-k" => Settings() with { TopK = -1 },
            "top-p" => Settings() with { TopP = 0 }, "answer" => Settings() with { MaxAnswerChars = 0 },
            "maximum" => Settings(1), _ => Settings(),
        };
        var selectedCorpus = fault switch
        {
            "manifest" => Require(CodeDocumentationOneLessonCorpusSelection.Create(goodCorpus.Corpus with { ManifestFileSha256 = Hex('f') }, goodCorpus.CaseIds)),
            "cases" => Require(CodeDocumentationOneLessonCorpusSelection.Create(goodCorpus.Corpus, [fixture.HeldOutIds[0]])),
            _ => goodCorpus,
        };
        var owner = selectedCorpus.OpenFresh(fixture.Repository, settings);
        Assert.False(owner.IsSuccess);
        var rejected = Plan(selectedCorpus, 2).OpenFresh(fixture.Repository, settings, config, noise);
        Assert.Equal(owner.ErrorCode, rejected.ErrorCode); Assert.Equal(owner.ErrorMessage, rejected.ErrorMessage);
        var restored = Require(good.OpenFresh(fixture.Repository, Settings(), config, noise));
        Assert.Equal(goodCorpus.CaseIds, restored.Selection.CaseIds);
        Assert.Equal(good.PlanSha256, restored.Selection.PlanSha256);
    }

    [Fact]
    public void FreshPlan_Actual201CaseOwnerCannotBeAcceptedBySelectedFirst200()
    {
        using var fixture = new Fixture(201);
        var corpus = Require(CodeDocumentationOneLessonCorpusSelection.Create(fixture.Descriptor, fixture.HeldOutIds[..200]));
        var owner = corpus.OpenFresh(fixture.Repository, Settings()); Assert.False(owner.IsSuccess);
        var result = Plan(corpus, 2).OpenFresh(fixture.Repository, Settings(), Configuration(800), Noise(2));
        Assert.Equal(owner.ErrorCode, result.ErrorCode); Assert.Equal(owner.ErrorMessage, result.ErrorMessage);
        using var restored = new Fixture(200);
        Assert.True(Plan(restored.Selection(), 2).OpenFresh(restored.Repository, Settings(), Configuration(800), Noise(2)).IsSuccess);
    }

    [Theory]
    [InlineData("v1")]
    [InlineData("missing-corpus")]
    [InlineData("hash")]
    [InlineData("count")]
    [InlineData("repeat")]
    public void Catalog_RejectsPlanAttachmentVersionMissingDependencyAndCompactMismatchThenRestores(string fault)
    {
        var corpus = Require(CodeDocumentationOneLessonCorpusSelection.Create(Shape(), [Id('1')]));
        var grant = Grant(Plan(corpus, 2), corpus);
        Assert.True(Catalog(grant).Resolve(Selector()).IsSuccess);
        var invalid = fault switch
        {
            "v1" => grant with { FormatVersion = 1, DescriptorsV2 = null, CorpusSelection = null },
            "missing-corpus" => grant with { CorpusSelection = null },
            "hash" => grant with { DescriptorsV2 = grant.DescriptorsV2! with { EvaluationPlanSha256 = Hex('f') } },
            "count" => grant with { DescriptorsV2 = grant.DescriptorsV2! with { CaseCount = 2 } },
            "repeat" => grant with { DescriptorsV2 = grant.DescriptorsV2! with { RepeatCount = 3 } },
            _ => throw new InvalidOperationException(fault),
        };
        Assert.Equal(CodeDocumentationOneLessonGrantCatalog.InvalidCatalogErrorCode, Catalog(invalid).Resolve(Selector()).ErrorCode);
        Assert.True(Catalog(grant).Resolve(Selector()).IsSuccess);
    }

    [Fact]
    public void Catalog_FreezesAllFourAttachmentsRetainsPureReplayAndJsonIgnoredLegacyWire()
    {
        var source = Source(); var checkpoint = Checkpoint();
        var corpus = Require(CodeDocumentationOneLessonCorpusSelection.Create(Shape(), [Id('1')])); var plan = Plan(corpus, 2);
        var grant = Grant(plan, corpus) with { SourceSelection = source, CheckpointSelection = checkpoint,
            CandidateSha256 = checkpoint.Candidate.CandidateSha256, ParentManifestSha256 = checkpoint.Parent.ManifestSha256,
            ParentWeightsSha256 = checkpoint.Parent.WeightsSha256,
            DescriptorsV2 = Grant(plan, corpus).DescriptorsV2! with { SourceSelectionSha256 = source.SourceSelectionSha256,
                ParentCheckpointSha256 = checkpoint.ParentCheckpointSha256, CandidateSelectionSha256 = checkpoint.CandidateSelectionSha256 } };
        var catalog = Catalog(grant); var frozen = Require(catalog.Resolve(Selector()));
        Assert.Same(frozen, Require(catalog.Revalidate(frozen.GrantId, frozen.GrantVersion)));
        Assert.NotSame(source, frozen.SourceSelection); Assert.NotSame(checkpoint, frozen.CheckpointSelection);
        Assert.NotSame(corpus, frozen.CorpusSelection); Assert.NotSame(plan, frozen.EvaluationPlanSelection);
        Assert.NotSame(corpus.Corpus, frozen.CorpusSelection!.Corpus);
        Assert.NotSame(plan.CaseIds, frozen.EvaluationPlanSelection!.CaseIds);
        Assert.Equal(source.SourceSelectionSha256, frozen.SourceSelection!.SourceSelectionSha256);
        Assert.Equal(checkpoint.ParentCheckpointSha256, frozen.CheckpointSelection!.ParentCheckpointSha256);
        Assert.Equal(checkpoint.CandidateSelectionSha256, frozen.CheckpointSelection.CandidateSelectionSha256);
        Assert.Equal(corpus.CorpusDescriptorSha256, frozen.CorpusSelection.CorpusDescriptorSha256);
        Assert.Equal(plan.PlanSha256, frozen.EvaluationPlanSelection.PlanSha256); AssertLiteral(frozen.EvaluationPlanSelection);
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(grant));
        Assert.Equal(new[] { "GrantId", "GrantVersion", "GrantSelector", "ObjectiveId", "TrainingRunId", "CandidateSha256",
            "ParentManifestSha256", "ParentWeightsSha256", "ConfigurationSha256", "ExamConditionSha256" },
            json.RootElement.EnumerateObject().Select(p => p.Name));
        var legacy = grant with { FormatVersion = 1, DescriptorsV2 = null, SourceSelection = null,
            CheckpointSelection = null, CorpusSelection = null, EvaluationPlanSelection = null };
        Assert.True(Catalog(legacy).Resolve(Selector()).IsSuccess);
        Assert.True(Catalog(grant with { EvaluationPlanSelection = null }).Resolve(Selector()).IsSuccess);
    }

    [Fact]
    public void RestrictedWriter_DefaultRejectionsAndExistingSelectionPreimagesRemainUnchanged()
    {
        foreach (var raw in new[] { "{\"x\":[]}", "{\"x\":-1}", "{\"x\":1.5}", "{\"x\":null}" })
        {
            using var json = JsonDocument.Parse(raw);
            Assert.Throws<JsonException>(() => CodeDocumentationOneLessonV2Wire.CanonicalBytes(json.RootElement));
            Assert.Throws<JsonException>(() => CodeDocumentationOneLessonV2Wire.IsCanonical(json.RootElement));
        }
        var source = Source();
        var sourceLiteral = "{\"PairSelections\":[{\"SourceId\":\"source0\",\"TrainingPairId\":\"" + Hex('a') +
            "\"}],\"Sources\":[{\"DatasetVersion\":\"v1\",\"DesignPatternSlug\":\"learning-unit\",\"DomainSlug\":\"bug\",\"ManifestFileByteLength\":1,\"ManifestFileSha256\":\"" +
            Hex('1') + "\",\"Role\":\"TrainingPairs\",\"SealedManifestSha256\":\"" + Hex('2') + "\",\"SourceId\":\"source0\"}]}";
        Assert.Equal(Encoding.UTF8.GetBytes(sourceLiteral), CodeDocumentationOneLessonV2Wire.CanonicalSourceSelectionBytes(source));
        Assert.Equal(DomainHash("xiigen-s04-source-selection-v2\n", sourceLiteral), source.SourceSelectionSha256);
        var checkpoint = Checkpoint();
        var parent = "{\"DesignPatternSlug\":\"learning-unit\",\"DomainSlug\":\"bug\",\"ManifestSha256\":\"" + Hex('1') +
            "\",\"ModelIdSlug\":\"tiny-model\",\"ModelVersion\":\"v1\",\"Step\":1,\"WeightsByteLength\":7,\"WeightsSha256\":\"" + Hex('2') + "\"}";
        var candidate = "{\"CandidateId\":\"candidate:1\",\"CandidateSha256\":\"" + Hex('3') + "\"}";
        Assert.Equal(DomainHash("xiigen-s04-parentcheckpoint-v2\n", parent), checkpoint.ParentCheckpointSha256);
        Assert.Equal(DomainHash("xiigen-s04-candidateselection-v2\n", candidate), checkpoint.CandidateSelectionSha256);
        var corpus = Require(CodeDocumentationOneLessonCorpusSelection.Create(Shape(), [Id('1')]));
        var corpusLiteral = "{\"CaseIds\":[\"" + Id('1') + "\"],\"Corpus\":{\"CorpusId\":\"opaque:corpus-1\",\"CorpusSha256\":\"" + Hex('2') +
            "\",\"CorpusVersion\":\"v1\",\"DomainSlug\":\"translation\",\"ManifestFileByteLength\":1,\"ManifestFileSha256\":\"" + Hex('1') +
            "\",\"PayloadByteLength\":1,\"PayloadSha256\":\"" + Hex('3') + "\",\"Purpose\":\"ClosedEvaluation\",\"SubjectSlug\":\"en-ru\"}}";
        Assert.Equal(Encoding.UTF8.GetBytes(corpusLiteral), CodeDocumentationOneLessonV2Wire.CanonicalCorpusSelectionBytes(corpus));
        Assert.Equal(DomainHash("xiigen-s04-corpusdescriptor-v2\n", corpusLiteral), corpus.CorpusDescriptorSha256);
        var compact = Grant(Plan(corpus, 2), corpus).DescriptorsV2! with { CaseCount = 200, RepeatCount = 5000 };
        var bytes = CodeDocumentationOneLessonV2Wire.CanonicalBytes(JsonSerializer.SerializeToElement(compact));
        Assert.Equal(697, bytes.Length);
        using var parsed = JsonDocument.Parse(bytes); Assert.Equal(compact, CodeDocumentationOneLessonV2Wire.ReadDescriptors(parsed.RootElement));
    }

    private static string Oracle(CodeDocumentationOneLessonEvaluationPlanSelection p)
        => "{\"CaseIds\":[" + string.Join(",", p.CaseIds.Select(id => "\"" + id + "\"")) + "],\"CorpusId\":\"" + p.CorpusId +
            "\",\"EvaluatorId\":\"" + p.EvaluatorId + "\",\"ExpectedEvaluationCallCount\":" + p.ExpectedEvaluationCallCount.ToString(CultureInfo.InvariantCulture) +
            ",\"PlanId\":\"" + p.PlanId + "\",\"RepeatCount\":" + p.RepeatCount.ToString(CultureInfo.InvariantCulture) + "}";
    private static void AssertLiteral(CodeDocumentationOneLessonEvaluationPlanSelection p)
    {
        var bytes = Encoding.UTF8.GetBytes(Oracle(p));
        Assert.Equal(bytes, CodeDocumentationOneLessonV2Wire.CanonicalEvaluationPlanBytes(p)); Assert.Equal(Sha(bytes), p.PlanSha256);
    }
    private static CodeDocumentationOneLessonEvaluationPlanSelection Plan(CodeDocumentationOneLessonCorpusSelection c, int repeat)
        => Require(CodeDocumentationOneLessonEvaluationPlanSelection.Create(c, "plan:1", "evaluator:1", repeat, checked(c.CaseIds.Count * repeat)));
    private static CodeDocumentationOneLessonCorpusV2 Shape() => new("opaque:corpus-1", "ClosedEvaluation", "translation", "en-ru", "v1", Hex('1'), Hex('2'), 1, Hex('3'), 1);
    private static CodeDocumentationSavedModelAssessmentGenerationSettings Settings(int max = 200) => new(1, 0, 0, 1, 0, max, 1);
    private static MeasurementNoisePolicyConfig Noise(int repeat) => new(repeat, 1);
    private static ContinuousLearningTp1DpoConfiguration Configuration(int budget)
        => Require(ContinuousLearningTp1DpoConfiguration.Create("qwen-code-doc-quality-test:9b", Hex('a'), Hex('b'), budget,
            Id('c'), Id('d'), Enumerable.Range(1, 32).Select(v => checked((byte)v)).ToArray()));
    private static CodeDocumentationOneLessonSourceSelection Source() => Require(CodeDocumentationOneLessonSourceSelection.Create(
        [new("source0", "TrainingPairs", "bug", "learning-unit", "v1", Hex('1'), 1, Hex('2'))], [new(Hex('a'), "source0")]));
    private static CodeDocumentationOneLessonCheckpointSelection Checkpoint() => Require(CodeDocumentationOneLessonCheckpointSelection.Create(
        new(1, "bug", "learning-unit", "tiny-model", "v1", Hex('1'), Hex('2'), 7), new("candidate:1", Hex('3'))));
    private static string Hex(char c) => new(c, 64);
    private static string Id(char c) => "sha256:" + Hex(c);
    private static string Sha(byte[] b) => Convert.ToHexString(SHA256.HashData(b)).ToLowerInvariant();
    private static string DomainHash(string domain, string literal) => Sha(Encoding.UTF8.GetBytes(domain + literal));
    private static CodeDocumentationOneLessonSelectorV1 Selector() => new("objective", "selector");
    private static CodeDocumentationOneLessonGrantV1 Grant(CodeDocumentationOneLessonEvaluationPlanSelection p, CodeDocumentationOneLessonCorpusSelection c)
        => new("grant", "v2", "selector", "objective", "evaluation-plan-run", Hex('1'), Hex('2'), Hex('3'), Hex('4'), Hex('5'))
        { FormatVersion = 2, CorpusSelection = c, EvaluationPlanSelection = p, DescriptorsV2 = new(2, Hex('1'), Hex('2'), Hex('3'),
            c.CorpusDescriptorSha256, p.PlanSha256, p.CaseIds.Count, p.RepeatCount, Hex('6'), Hex('7')) };
    private static CodeDocumentationOneLessonGrantCatalog Catalog(CodeDocumentationOneLessonGrantV1 grant) => CodeDocumentationOneLessonGrantCatalog.FromTrustedGrants([grant]);
    private static T Require<T>(OperationResult<T> r) { Assert.True(r.IsSuccess, r.ErrorCode + ": " + r.ErrorMessage); Assert.NotNull(r.Value); return r.Value!; }
    private void LogRuntimeIdentities()
    {
        var test = typeof(CodeDocumentationOneLessonEvaluationPlanSelectionTests).Assembly;
        foreach (var identity in new[] {
            (Kind: "production-dll", Path: typeof(RepositoryOwnedReferenceCorpusManifestSealer).Assembly.Location),
            (Kind: "test-dll", Path: test.Location),
            (Kind: "runtimeconfig", Path: Path.ChangeExtension(test.Location, ".runtimeconfig.json")),
            (Kind: "deps", Path: Path.ChangeExtension(test.Location, ".deps.json")) })
        {
            var bytes = File.ReadAllBytes(identity.Path);
            output.WriteLine("EVALUATION_PLAN_RUNTIME " + JsonSerializer.Serialize(new { kind = identity.Kind, path = identity.Path, sha256 = Sha(bytes), byteLength = bytes.LongLength }));
        }
    }
    private static void AssertNoExecutionArtifacts(Fixture f)
    {
        Assert.DoesNotContain(Directory.GetFiles(f.Repository, "*", SearchOption.AllDirectories), p => p.EndsWith("session.jsonl", StringComparison.Ordinal) ||
            p.EndsWith(".safetensors", StringComparison.Ordinal) || p.Contains("baseline", StringComparison.Ordinal) ||
            p.Contains(Path.DirectorySeparatorChar + "attempts" + Path.DirectorySeparatorChar, StringComparison.Ordinal));
        Assert.False(Directory.Exists(Path.Combine(f.Repository, "training-artifacts", "models")));
    }
    private sealed class Fixture : IDisposable
    {
        private string Root { get; } = Path.Combine(Path.GetTempPath(), "evaluation-plan-" + Guid.NewGuid().ToString("N"));
        internal string Repository { get; }
        internal RepositoryOwnedReferenceCorpusAddress Address { get; }
        internal string PayloadPath => Path.Combine(Address.CorpusRoot, RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName);
        internal FrozenReferenceCorpus Corpus { get; }
        internal CodeDocumentationOneLessonCorpusV2 Descriptor { get; }
        internal string[] HeldOutIds => Corpus.Entries.Where(e => e.Partition == FrozenReferencePartition.HeldOut).Select(e => e.EntryId).OrderBy(x => x, StringComparer.Ordinal).ToArray();
        internal Fixture(int count)
        {
            Repository = Path.Combine(Root, "repo"); Directory.CreateDirectory(Repository);
            foreach (var name in new[] { "LlmFromScratch.sln", "llm_mvp_core.sln", "AGENTS.md" }) File.WriteAllText(Path.Combine(Repository, name), "synthetic fixture sentinel");
            Directory.CreateDirectory(Path.Combine(Repository, "14.ContinuousLearning")); Directory.CreateDirectory(Path.Combine(Repository, "11.DPO"));
            Corpus = Require(FrozenReferenceCorpus.Build("human-corpus-name", "v1", new(2026, 10, 3, 12, 0, 0, TimeSpan.Zero),
                Enumerable.Range(0, count).Select(i => Entry("heldout-" + i, FrozenReferencePartition.HeldOut)).Prepend(Entry("train", FrozenReferencePartition.Train))));
            Address = Require(RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(Repository, "translation", "en-ru", "v1"));
            var manifest = Require(RepositoryOwnedReferenceCorpusManifestSealer.PersistAndSeal(Address, Corpus));
            var bytes = File.ReadAllBytes(Address.ManifestPath);
            Assert.True(FrozenReferenceCanonicalization.IsCanonicalSha256Ref(manifest.CorpusSha256));
            Descriptor = new("opaque:catalog-key", "ClosedEvaluation", manifest.Domain, manifest.Subject, manifest.CorpusVersion,
                Sha(bytes), manifest.CorpusSha256[7..], bytes.LongLength, manifest.PayloadSha256, manifest.PayloadByteLength);
        }
        internal CodeDocumentationOneLessonCorpusSelection Selection() => Require(CodeDocumentationOneLessonCorpusSelection.Create(Descriptor, HeldOutIds));
        private static FrozenReferenceEntry Entry(string seed, FrozenReferencePartition partition)
        {
            var answer = "synthetic closed reference " + seed;
            var provenance = Require(PaidReferenceProvenance.Create("synthetic-paid-provider", "synthetic-paid-model", "v1", "request-" + seed,
                new(2026, 10, 3, 10, 0, 0, TimeSpan.Zero), FrozenReferenceEntry.ComputeNormalizedTextSha256(answer)));
            return Require(FrozenReferenceEntry.Create("synthetic owner input " + seed, answer, provenance, "translation", "en-ru", partition,
                FrozenReferenceEntry.ComputeNormalizedTextSha256("independent-source-entity:" + seed)));
        }
        public void Dispose()
        {
            var target = Path.GetFullPath(Root); Assert.StartsWith(Path.GetFullPath(Path.GetTempPath()) + "evaluation-plan-", target);
            if (Directory.Exists(target)) Directory.Delete(target, recursive: true);
        }
    }
}
