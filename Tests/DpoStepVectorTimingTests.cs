// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
using System.Security.Cryptography;
using System.Text.Json;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DPO;
using DPO.Models;
using LoRA;
using TensorCore.Core;
using TensorCore.Layers;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;
using TransformerModel.Layers;
using Xunit.Abstractions;

namespace LlmFromScratch.Tests;

// Tiny engineering fixtures only: no published TRAIN, checkpoint, panel, generation or learning claim.
public sealed class DpoStepVectorTimingTests(ITestOutputHelper output)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private static readonly ModelConfig Config = new()
    { VocabSize = 256, ContextLen = 8, HiddenDim = 8, NumLayers = 1, NumHeads = 2, NumKvHeads = 1, FfnHiddenDim = 16 };

    [Fact]
    public void TimedExactStepVector_MatchesUntimedMetricsWeightsOptimizerRandomAndLegacyJson()
    {
        var root = NewRoot("parity");
        var a = Fixture(); var b = Fixture();
        var untimed = Trainer(a, Path.Combine(root, "untimed"));
        var timed = Trainer(b, Path.Combine(root, "timed"));
        var referenceBefore = Bytes(b.Reference);
        var baseBefore = b.Policy.BaseParameters.Select(x => x.Value.Data.ToArray()).ToArray();
        var parametersBefore = Bytes(b.Policy);
        var timing = new DpoStepVectorTiming();
        var expected = untimed.StepVector(Batches(a.Tokenizer), 1);
        var actual = timed.StepVector(Batches(b.Tokenizer), 1, timing);

        Assert.Equal(JsonSerializer.Serialize(expected, Json), JsonSerializer.Serialize(actual, Json));
        Assert.Equal(Bytes(a.Policy), Bytes(b.Policy));
        Assert.NotEqual(parametersBefore, Bytes(b.Policy));
        Assert.Equal(untimed.ExportOptimizerState().ToBytes(), timed.ExportOptimizerState().ToBytes());
        Assert.Equal(RandomBytes(a, 1), RandomBytes(b, 1));
        Assert.Equal(referenceBefore, Bytes(b.Reference));
        for (var i = 0; i < baseBefore.Length; i++) Assert.Equal(baseBefore[i], b.Policy.BaseParameters[i].Value.Data);
        Assert.Equal(File.ReadAllBytes(Path.Combine(root, "untimed", DpoTrainer.BuildMetricsFileName(1))),
            File.ReadAllBytes(Path.Combine(root, "timed", DpoTrainer.BuildMetricsFileName(1))));
        Assert.Equal(File.ReadAllBytes(Path.Combine(root, "untimed", DpoTrainer.BuildPolicyCheckpointFileName(1))),
            File.ReadAllBytes(Path.Combine(root, "timed", DpoTrainer.BuildPolicyCheckpointFileName(1))));

        var snapshot = timing.Snapshot();
        Assert.Equal("RETURNED", snapshot.Outcome); Assert.Equal(1, snapshot.Step);
        Assert.Equal(2, snapshot.VectorBatchesCompleted);
        AssertPhase(snapshot.ReferenceForward, 4, 4); AssertPhase(snapshot.PolicyForward, 4, 4);
        AssertPhase(snapshot.Backward, 4, 4); AssertPhase(snapshot.ApplyOptimizer, 1, 1);
        AssertContained(snapshot);
        Assert.Throws<InvalidOperationException>(() => timed.StepVector(Batches(b.Tokenizer), 2, timing));
        Assert.Equal(1, timed.ExportOptimizerState().Step);
        Assert.False(File.Exists(Path.Combine(root, "timed", DpoTrainer.BuildMetricsFileName(2))));
        SaveEvidence("timed-untimed-parity.json", new { fixtureRoot = root, parity = "EXACT_METRICS_WEIGHTS_OPTIMIZER_RNG_AND_PUBLISHED_BYTES", snapshot });
    }

    [Fact]
    public void AdmissionFailure_LeavesAllPhaseCountsZeroAndDurationsAbsent()
    {
        var root = NewRoot("admission"); var fixture = Fixture(); var trainer = Trainer(fixture, root);
        var before = Bytes(fixture.Policy); var timing = new DpoStepVectorTiming();
        Assert.Throws<ArgumentException>(() => trainer.StepVector([], 1, timing));
        var snapshot = timing.Snapshot(); Assert.Equal("THREW", snapshot.Outcome);
        AssertPhase(snapshot.ReferenceForward, 0, 0); AssertPhase(snapshot.PolicyForward, 0, 0);
        AssertPhase(snapshot.Backward, 0, 0); AssertPhase(snapshot.ApplyOptimizer, 0, 0);
        Assert.Equal(0, snapshot.VectorBatchesCompleted); Assert.Equal(before, Bytes(fixture.Policy));
        Assert.Equal(0, trainer.ExportOptimizerState().Step);
        SaveEvidence("admission-failure.json", snapshot);
    }

    [Fact]
    public void ThrownReferenceSide_CountsOnlyOneCompletedSideAndPreservesOriginalException()
    {
        var root = NewRoot("partial-reference"); var policy = PlainModel(); var reference = PlainModel();
        foreach (var parameter in reference.Parameters()) parameter.Value.RequiresGrad = false;
        var expected = new IOException("synthetic second reference forward failure");
        reference.ExecuteAdapterMutation(lease =>
        {
            var projection = lease.InspectProjection(0, "q_proj");
            lease.PopulateAdapterPair(0, "q_proj", new ThrowOnSecondForward(projection.BaseProjection, expected));
            return 0;
        });
        var trainer = new DpoTrainer(policy, reference, root, DpoMutationAdmissionCapability.Issue(new TestLease()));
        var before = Bytes(policy); var stateBefore = trainer.ExportOptimizerState().ToBytes(); var timing = new DpoStepVectorTiming();
        Assert.Same(expected, Assert.Throws<IOException>(() => trainer.StepVector(Batches(Tokenizer()), 1, timing)));
        var snapshot = timing.Snapshot(); Assert.Equal("THREW", snapshot.Outcome);
        AssertPhase(snapshot.ReferenceForward, 2, 1); AssertPhase(snapshot.PolicyForward, 0, 0);
        AssertPhase(snapshot.Backward, 0, 0); AssertPhase(snapshot.ApplyOptimizer, 0, 0);
        Assert.Equal(0, snapshot.VectorBatchesCompleted); AssertContained(snapshot);
        Assert.Equal(before, Bytes(policy)); Assert.Equal(stateBefore, trainer.ExportOptimizerState().ToBytes());
        Assert.False(File.Exists(Path.Combine(root, DpoTrainer.BuildMetricsFileName(1))));
        SaveEvidence("partial-reference-failure.json", snapshot);
    }

    [Fact]
    public void PublicationFailure_RetainsCompletedComputeButRollsBackWeightsOptimizerAndRandom()
    {
        var root = NewRoot("publication-failure"); var fixture = Fixture();
        var expected = new IOException("synthetic publication failure");
        var trainer = Trainer(fixture, root, new DpoTrainerPublicationObserver { BeforePolicyFinalMove = _ => throw expected });
        var before = Bytes(fixture.Policy); var optimizerBefore = trainer.ExportOptimizerState().ToBytes(); var randomBefore = RandomBytes(fixture, 0);
        var timing = new DpoStepVectorTiming();
        Assert.Same(expected, Assert.Throws<IOException>(() => trainer.StepVector(Batches(fixture.Tokenizer), 1, timing)));
        var snapshot = timing.Snapshot(); Assert.Equal("THREW", snapshot.Outcome);
        AssertPhase(snapshot.ReferenceForward, 4, 4); AssertPhase(snapshot.PolicyForward, 4, 4);
        AssertPhase(snapshot.Backward, 4, 4); AssertPhase(snapshot.ApplyOptimizer, 1, 1);
        Assert.Equal(2, snapshot.VectorBatchesCompleted); AssertContained(snapshot);
        Assert.Equal(before, Bytes(fixture.Policy)); Assert.Equal(optimizerBefore, trainer.ExportOptimizerState().ToBytes());
        Assert.Equal(randomBefore, RandomBytes(fixture, 0));
        Assert.False(File.Exists(Path.Combine(root, DpoTrainer.BuildPolicyCheckpointFileName(1))));
        Assert.False(File.Exists(Path.Combine(root, DpoTrainer.BuildMetricsFileName(1))));
        SaveEvidence("publication-failure.json", snapshot);
    }

    [Fact]
    public void ExistingOfflineRunner_TinyFixtureSurfacesActualTimingInJournalAndReceiptAndRefusesReplay()
    {
        var root = NewRoot("runner"); var fixture = Fixture();
        var source = Path.Combine(root, "tiny-source.bin"); File.WriteAllBytes(source, Bytes(fixture.Policy));
        var sourceHash = Hash(File.ReadAllBytes(source));
        var input = Path.Combine(root, "tiny-simulation.jsonl");
        File.WriteAllText(input, JsonSerializer.Serialize(new { id = "tiny-1", dataset_version = "engineering-fixture-v1",
            domain = "csharp", pair_kind = "CODE_TO_DOC", provenance = "simulation", prompt = "a", chosen = "b", rejected = "cc" }) + "\n");
        var modelLoads = 0;
        var options = new SimulationDpoRunOptions(root, "tiny-engineering-observation", "calibration", input, Hash(File.ReadAllBytes(input)), 1, 1, 1, null,
            _ => { modelLoads++; return new(fixture.Policy, fixture.Reference, fixture.Tokenizer, fixture.Lora, fixture.Seeds, source, sourceHash,
                _ => Fixture().Policy); });
        var result = CodeDocumentationSimulationDpoExperiment.Run(options);
        Assert.True(result.IsSuccess, result.ErrorCode + ":" + result.ErrorMessage);
        var receiptBytes = File.ReadAllBytes(result.Value!);
        using var receipt = JsonDocument.Parse(receiptBytes);
        var report = receipt.RootElement.GetProperty("stepVectorTiming");
        Assert.Equal("step-vector-phase-timing-v1", report.GetProperty("schema").GetString());
        Assert.Equal(0, report.GetProperty("journalWriteFailures").GetInt32());
        Assert.Equal(1, receipt.RootElement.GetProperty("stepVectorCallsCompleted").GetInt32());
        var attempt = Assert.Single(report.GetProperty("attempts").EnumerateArray());
        Assert.Equal("RETURNED", attempt.GetProperty("outcome").GetString());
        foreach (var name in new[] { "referenceForward", "policyForward", "backward", "applyOptimizer" })
        {
            var count = name == "applyOptimizer" ? 1 : 2;
            Assert.Equal(count, report.GetProperty(name).GetProperty("completedCalls").GetInt64());
            Assert.Equal(count, attempt.GetProperty(name).GetProperty("completedCalls").GetInt64());
            Assert.True(report.GetProperty(name).GetProperty("completedMilliseconds").GetDouble() >= 0);
        }
        Assert.Equal(JsonValueKind.Null, receipt.RootElement.GetProperty("uninstrumented").GetProperty("optimizerOnlyMilliseconds").ValueKind);
        var journalPath = Path.Combine(Path.GetDirectoryName(result.Value!)!, "stages.jsonl");
        var events = File.ReadAllLines(journalPath).Select(x => JsonDocument.Parse(x)).ToArray();
        try
        {
            var timingEvent = Assert.Single(events.Where(x => x.RootElement.GetProperty("stage").GetString() == "training_step_phases"));
            Assert.Equal(attempt.Deserialize<DpoStepVectorTimingSnapshot>(Json),
                timingEvent.RootElement.GetProperty("detail").Deserialize<DpoStepVectorTimingSnapshot>(Json));
            Assert.Contains(events, x => x.RootElement.GetProperty("stage").GetString() == "training_step_total");
        }
        finally { foreach (var item in events) item.Dispose(); }
        var replay = CodeDocumentationSimulationDpoExperiment.Run(options with { RunId = "tiny-engineering-replay-refused" });
        Assert.False(replay.IsSuccess); Assert.Equal(CodeDocumentationSimulationDpoExperiment.ReplayError, replay.ErrorCode);
        Assert.Equal(1, modelLoads); Assert.Equal(receiptBytes, File.ReadAllBytes(result.Value!));
        SaveEvidence("tiny-runner-observation.json", new { fixtureRoot = root, receipt = result.Value, journal = journalPath, report,
            effect = receipt.RootElement.GetProperty("effect"), completedCalls = 1, replayRefused = true, scientificLearningMeasured = false });
    }

    // Запись наблюдения этой проверки проходит, потому что опубликованность судится по
    // ФАКТИЧЕСКОМУ целевому пути прогона, а не по устойчивому родителю.
    //
    // Какую беду это закрывает. Устойчивый родитель попытки уже отслеживается хранилищем. Если
    // судить опубликованность по нему, отказ срабатывал бы ВСЕГДА, и проверка больше никогда не
    // смогла бы записать наблюдение — это прямо нарушает правило о том, что доказательства
    // подаются в хранилище, а не стираются. Если же судить по уникальному каталогу прогона, он
    // всегда пуст, запись проходит, а отказ остаётся наготове ровно на тот случай, когда кому-то
    // позже передадут уже опубликованный путь.
    //
    // Почему это испытание нужно. Оно падает, если кто-нибудь вернёт вердикт на устойчивый
    // родитель (тогда запись будет отклонена) или включит перенаправление (тогда разрешённый
    // каталог уедет из учебных артефактов). То есть оно закрепляет принятое решение, а не
    // пересказывает его.
    [Fact]
    public void ObservationWrite_JudgesPublicationByActualRunTargetSoEvidenceStillReachesStorage()
    {
        var repository = RepositoryRoot(); // Рабочая копия, подтверждённая файлом проекта обучения предпочтений.
        var parent = Path.GetFullPath(AttemptDirectory(repository)); // Устойчивый родитель назначенной попытки.
        var target = Path.Combine(parent, "verdict-scope-" + Guid.NewGuid().ToString("N")); // Фактический целевой путь прогона.

        // Вердикт берётся по фактическому целевому пути: он уникален и хранилищем не отслеживается.
        var targetPublished = ExperimentFixes.ExperimentEvidenceSink.IsPublishedEvidenceDirectory(
            repository, target, out var targetVerdict);
        Assert.False(targetPublished); // Уникальный каталог прогона опубликованным наблюдением не является.

        // При политике отказа запись по фактическому пути обязана ПРОЙТИ, а не отклониться.
        var sink = ExperimentFixes.ExperimentEvidenceSink.ForDeclaredDirectory(
            repository, target, ExperimentFixes.PublishedEvidencePolicy.RefuseToWrite);
        Assert.Equal(target, sink.WriteRoot); // Разрешённый каталог записи совпадает с назначенным.
        Assert.Equal(target, sink.DeclaredDirectory); // Подмены назначенного каталога не произошло.
        Assert.False(sink.RedirectedFromPublishedEvidence); // Перенаправление здесь запрещено и не случилось.

        // Наблюдение действительно ложится на диск внутри учебных артефактов, а не в игнорируемую папку.
        var written = sink.WriteTextExactly("verdict-scope-observation.json", "{}\n");
        Assert.True(File.Exists(written)); // Файл наблюдения создан.
        Assert.StartsWith(parent, written, StringComparison.Ordinal); // Запись осталась внутри назначенной попытки.

        // Для человека рядом записывается вердикт по устойчивому родителю: он показывает, почему
        // родитель нельзя брать за единицу опубликованности — по нему отказ был бы всегда.
        ExperimentFixes.ExperimentEvidenceSink.IsPublishedEvidenceDirectory(repository, parent, out var parentVerdict);
        output.WriteLine("Вердикт по фактическому целевому пути: " + targetVerdict); // Причина разрешения записи.
        output.WriteLine("Вердикт по устойчивому родителю (не единица опубликованности): " + parentVerdict);
        SaveEvidence("verdict-scope.json", new { parent, target, targetPublished, targetVerdict, parentVerdict, written });
    }

    private static void AssertPhase(DpoPhaseTimingSnapshot phase, long started, long completed)
    {
        Assert.Equal(started, phase.StartedCalls); Assert.Equal(completed, phase.CompletedCalls);
        Assert.Equal(started - completed, phase.IncompleteCalls);
        if (completed == 0) Assert.Null(phase.CompletedMilliseconds);
        else { Assert.NotNull(phase.CompletedMilliseconds); Assert.True(double.IsFinite(phase.CompletedMilliseconds.Value)); Assert.True(phase.CompletedMilliseconds.Value >= 0); }
    }
    private static void AssertContained(DpoStepVectorTimingSnapshot snapshot)
    {
        Assert.NotNull(snapshot.ElapsedMilliseconds);
        var inner = new[] { snapshot.ReferenceForward, snapshot.PolicyForward, snapshot.Backward, snapshot.ApplyOptimizer }
            .Sum(x => x.CompletedMilliseconds ?? 0);
        Assert.True(inner <= snapshot.ElapsedMilliseconds.Value);
    }
    private static IReadOnlyList<DpoVectorBatch> Batches(BpeTokenizer tokenizer)
        => new[] { ("a", "b", "cc"), ("d", "ee", "f") }.Select(x => new DpoVectorBatch(
            DpoExactPairMaterial.CreateBatch(tokenizer, x.Item1, x.Item2, x.Item3,
                DpoTrainerPairCustoms.CreateAdmitted(x.Item1, x.Item2, x.Item3, DpoPairSideAuthorKind.DeterministicGenerator,
                    DpoPairSideAuthorKind.DeterministicGenerator, "simulation").Admission), [1f, 0f])).ToArray();
    private static BpeTokenizer Tokenizer() { var tokenizer = new BpeTokenizer(); tokenizer.Train("fixture", 256); return tokenizer; }
    private static LlmModel PlainModel()
    {
        var model = new LlmModel(Config); var random = new Random(73);
        foreach (var parameter in model.Parameters())
            for (var i = 0; i < parameter.Value.Data.Length; i++)
                parameter.Value.Data[i] = parameter.Value.Shape.Rank == 1 ? 1 : (float)(random.NextDouble() - .5) * .15f;
        return model;
    }
    private sealed record ModelFixture(LlmModel Policy, LlmModel Reference, BpeTokenizer Tokenizer, LoraConfig Lora,
        LoraPairRandomSeedAuthority Seeds, IReadOnlyList<LoraLinear> Adapters);
    private static ModelFixture Fixture()
    {
        var policy = PlainModel(); var reference = PlainModel();
        var lora = new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] };
        var seeds = new LoraPairRandomSeedAuthority(73, "tiny.policy", "tiny.reference");
        var applied = LoraApplicator.ApplyFreshSeededPair(policy, reference, lora, seeds.RootInitializationSeed,
            seeds.PolicyDropoutStreamDomain, seeds.ReferenceDropoutStreamDomain);
        foreach (var parameter in reference.Parameters()) parameter.Value.RequiresGrad = false;
        return new(policy, reference, Tokenizer(), lora, seeds, applied.FirstModelAdapters);
    }
    private static DpoTrainer Trainer(ModelFixture fixture, string path, DpoTrainerPublicationObserver? observer = null)
    {
        var recipient = DpoProjectLoraRecipient.Capture(fixture.Policy, fixture.Reference, fixture.Adapters, fixture.Lora);
        var topology = LoraRandomStateIdentity.ComputeTopologySha256(Config, fixture.Lora, fixture.Seeds);
        var identity = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(fixture.Lora, fixture.Seeds);
        return DpoTrainer.CreateForProjectLora(fixture.Policy, fixture.Reference, path, DpoMutationAdmissionCapability.Issue(new TestLease()),
            recipient, DpoExecutionProfile.Capture(Batches(fixture.Tokenizer)[0].Batch, recipient.Bindings), null, observer,
            step => fixture.Policy.CaptureRandomState(topology, identity, step),
            state => fixture.Policy.RestoreRandomState(state, new(topology, identity)));
    }
    private static byte[] RandomBytes(ModelFixture fixture, int step)
        => JsonSerializer.SerializeToUtf8Bytes(fixture.Policy.CaptureRandomState(
            LoraRandomStateIdentity.ComputeTopologySha256(Config, fixture.Lora, fixture.Seeds),
            LoraRandomStateIdentity.ComputeLoraConfigurationSha256(fixture.Lora, fixture.Seeds), step));
    private static byte[] Bytes(LlmModel model) { using var stream = new MemoryStream(); model.SaveCheckpointToStream(stream); return stream.ToArray(); }
    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    /// <summary>Имя назначенной попытки инженерной проверки: именно её отслеживает хранилище.</summary>
    //
    // Зачем имя вынесено в одно место. Назначенная попытка — это каталог, который хранилище уже
    // отслеживает, поэтому именно он решает вопрос «можно ли сюда писать». Раньше это имя стояло
    // строкой прямо внутри создания каталога, и спросить про его опубликованность было негде.
    private const string AttemptDirectoryName = "rp01-stage-timing-20261016";

    /// <summary>Корень рабочей копии, подтверждённый файлом проекта обучения предпочтений.</summary>
    //
    // Почему корень подтверждается файлом проекта. Поддельный корень во временной папке с
    // подставленным файлом решения выглядел бы как рабочая копия; наличие проекта обучения
    // предпочтений (DPO — Direct Preference Optimization, обучение на парах предпочтений)
    // отличает настоящую рабочую копию от такой подделки.
    private static string RepositoryRoot()
    {
        var repository = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..")); // Четыре уровня вверх от каталога сборки испытаний.
        Assert.True(File.Exists(Path.Combine(repository, "11.DPO", "DPO.csproj"))); // Доказательство рабочей копии, а не временной папки.
        return repository; // Корень отдаётся вызывающему как есть.
    }

    /// <summary>Полный путь назначенной попытки внутри учебных артефактов.</summary>
    private static string AttemptDirectory(string repository)
        => Path.Combine(repository, "training-artifacts", "engineering-checks", AttemptDirectoryName); // Назначенная карточкой попытка.

    private string NewRoot(string name)
    {
        var repository = RepositoryRoot(); // Рабочая копия, подтверждённая файлом проекта обучения предпочтений.
        var declared = Path.Combine(AttemptDirectory(repository), name + "-" + Guid.NewGuid().ToString("N")); // Каталог показаний этого прогона.
        //
        // Зачем здесь общий приёмник доказательств опытов. Прогон незащищённого писателя сам
        // затирает опубликованные байты: как только каталог закоммичен, он становится чужим
        // наблюдением, и проверить опыт нельзя, не испортив его. Приёмник спрашивает само
        // хранилище и при опубликованном каталоге отклоняет запись заметным исключением с
        // точным путём. Перенаправление здесь запрещено намеренно: уход в игнорируемую папку
        // нарушил бы прямое правило о том, что под учебными артефактами ничего не игнорируется.
        var sink = ExperimentFixes.ExperimentEvidenceSink.ForDeclaredDirectory(
            repository,
            declared,
            ExperimentFixes.PublishedEvidencePolicy.RefuseToWrite);
        var root = sink.EnsureWriteRootDirectory(); // Разрешённый каталог записи создаётся, и место записи один раз объявляется.
        output.WriteLine("Retained tiny engineering fixture: " + root); // Человек видит точный путь показаний прогона.
        return root; // Дальше испытание пишет только внутрь разрешённого каталога.
    }
    private void SaveEvidence(string name, object value)
    {
        var json = JsonSerializer.Serialize(value, Json); output.WriteLine(json);
        var directory = Environment.GetEnvironmentVariable("XIIGEN_RP01_STAGE_TIMING_EVIDENCE");
        if (directory is not null) { Directory.CreateDirectory(directory); File.WriteAllText(Path.Combine(directory, name), json); }
    }
    private sealed class TestLease : IDpoMutationAdmissionIssuer
    {
        public IDisposable AcquireDpoTrainerMutationPermit() => new CancellationTokenSource();
        public void Dispose() { }
    }
    private sealed class ThrowOnSecondForward(Linear projection, Exception error) : Module, IAttentionProjectionAdapter
    {
        private int _calls;
        public Linear BaseProjection => projection;
        public Module RegistrationModule => this;
        public IReadOnlyList<(string Suffix, Parameter Parameter)> AdapterParameters => [];
        public Tensor Forward(Tensor input, bool training) => ++_calls == 2 ? throw error : projection.Forward(input);
    }
}
