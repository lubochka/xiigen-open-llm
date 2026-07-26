// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AutonomousLearning;
using AgentInfrastructure;
using ContinuousLearning;
using SC29 = SemanticCompiler29;

namespace LlmFromScratch.Tests;

// 16.AutonomousLearning — night-time dataset collection + automated LoRA training cycle.
// Experiment basis: E41 IdleDetector (+100%), E40V2 TrustScorer (+50%), E43 ReplayBuffer (+23%).
// Gate B: dotnet test --filter "AutonomousLearning_" → 22 passed, 0 failed (final target)
public class AutonomousLearningTests
{
    // Shared null storage — no persisted state needed for unit tests.
    private static readonly ISessionStorage Storage = new NullSessionStorageAL();

    // ── Phase 1: IdleDetector (E41 four-condition algorithm) ─────────────────────

    [Fact]
    public void AutonomousLearning_IdleDetector_AllConditionsMet_ReturnsSafeToTrain()
    {
        // Gate B: cpu=10%, idle=15min, charging=true, battery=60%, disk=5GB → SafeToTrain.
        // All four E41 conditions pass, so training is permitted.
        var detector = new IdleDetector(Storage);
        var status   = new SystemStatus(
            CpuPercent:       10.0,
            InputIdleMinutes: 15.0,
            IsCharging:       true,
            BatteryPercent:   60.0,
            DiskFreeGb:       5.0);

        var result = detector.Check(status);

        Assert.True(result.IsSuccess);
        Assert.Equal(IdleDetectorResult.SafeToTrain, result.Value);
    }

    [Fact]
    public void AutonomousLearning_IdleDetector_HighCpu_ReturnsWait()
    {
        // Gate B: cpu=25% (above the 20% threshold) → WaitHighCpu regardless of other fields.
        // E41 condition 1 fails: machine is under active CPU load.
        var detector = new IdleDetector(Storage);
        var status   = new SystemStatus(
            CpuPercent:       25.0,
            InputIdleMinutes: 15.0,
            IsCharging:       true,
            BatteryPercent:   60.0,
            DiskFreeGb:       5.0);

        var result = detector.Check(status);

        Assert.True(result.IsSuccess);
        Assert.Equal(IdleDetectorResult.WaitHighCpu, result.Value);
    }

    [Fact]
    public void AutonomousLearning_IdleDetector_RecentInput_ReturnsWait()
    {
        // Gate B: cpu=10%, idle=5min (below the 10-minute threshold) → WaitRecentInput.
        // E41 condition 2 fails: user was active within the last 10 minutes.
        var detector = new IdleDetector(Storage);
        var status   = new SystemStatus(
            CpuPercent:       10.0,
            InputIdleMinutes: 5.0,
            IsCharging:       true,
            BatteryPercent:   60.0,
            DiskFreeGb:       5.0);

        var result = detector.Check(status);

        Assert.True(result.IsSuccess);
        Assert.Equal(IdleDetectorResult.WaitRecentInput, result.Value);
    }

    [Fact]
    public void AutonomousLearning_IdleDetector_LowBattery_ReturnsWait()
    {
        // Gate B: battery=30%, not charging → WaitLowBattery.
        // E41 condition 3 fails: device is not on AC power and battery is below 50%.
        var detector = new IdleDetector(Storage);
        var status   = new SystemStatus(
            CpuPercent:       10.0,
            InputIdleMinutes: 15.0,
            IsCharging:       false,
            BatteryPercent:   30.0,
            DiskFreeGb:       5.0);

        var result = detector.Check(status);

        Assert.True(result.IsSuccess);
        Assert.Equal(IdleDetectorResult.WaitLowBattery, result.Value);
    }

    [Fact]
    public void AutonomousLearning_IdleDetector_LowDisk_ReturnsWait()
    {
        // Gate B: disk=1.5GB (below the 2 GB minimum) → WaitLowDisk.
        // E41 condition 4 fails: insufficient free space for dataset + LoRA artefacts.
        var detector = new IdleDetector(Storage);
        var status   = new SystemStatus(
            CpuPercent:       10.0,
            InputIdleMinutes: 15.0,
            IsCharging:       true,
            BatteryPercent:   60.0,
            DiskFreeGb:       1.5);

        var result = detector.Check(status);

        Assert.True(result.IsSuccess);
        Assert.Equal(IdleDetectorResult.WaitLowDisk, result.Value);
    }

    // ── Phase 2: TrustScorer (E40V2 K-NN on 7 source features) ──────────────────

    [Fact]
    public void AutonomousLearning_TrustScorer_InternalRepoWithCI_ScoresHigh()
    {
        // Gate B: IsInternal + HasCIPassing + HasTests + DomainMatch + PopularityScore=2
        //         + UpdateDaysAgo=5 → total ≥ 0.70 → TrustLevel.High.
        // Represents a well-maintained internal .NET monorepo with green CI.
        var scorer   = new TrustScorer(Storage);
        var features = new SourceFeatures(
            IsInternal:     true,
            HasCIPassing:   true,
            HasTests:       true,
            UpdateDaysAgo:  5,
            PopularityScore: 2,
            IsOfficialDocs: false,
            DomainMatch:    true);

        var result = scorer.Score(features);

        Assert.True(result.IsSuccess);
        Assert.Equal(TrustLevel.High, result.Value!.Level);
    }

    [Fact]
    public void AutonomousLearning_TrustScorer_UnknownBlog_ScoresLow()
    {
        // Gate B: all signals false/0, stale (365 days) → score < 0.30 → TrustLevel.Low.
        // Represents an unknown blog with no CI, no tests, and no stars.
        var scorer   = new TrustScorer(Storage);
        var features = new SourceFeatures(
            IsInternal:     false,
            HasCIPassing:   false,
            HasTests:       false,
            UpdateDaysAgo:  365,
            PopularityScore: 0,
            IsOfficialDocs: false,
            DomainMatch:    false);

        var result = scorer.Score(features);

        Assert.True(result.IsSuccess);
        Assert.Equal(TrustLevel.Low, result.Value!.Level);
    }

    [Fact]
    public void AutonomousLearning_TrustScorer_OfficialDocs_ScoresHigh()
    {
        // Gate B: IsOfficialDocs + HasCIPassing + DomainMatch + PopularityScore=3
        //         + UpdateDaysAgo=1 → total ≥ 0.70 → TrustLevel.High.
        // Represents docs.microsoft.com/dotnet — canonical reference source.
        var scorer   = new TrustScorer(Storage);
        var features = new SourceFeatures(
            IsInternal:     false,
            HasCIPassing:   true,
            HasTests:       false,
            UpdateDaysAgo:  1,
            PopularityScore: 3,
            IsOfficialDocs: true,
            DomainMatch:    true);

        var result = scorer.Score(features);

        Assert.True(result.IsSuccess);
        Assert.Equal(TrustLevel.High, result.Value!.Level);
    }

    // ── Phase 2: DataSourceFinder ────────────────────────────────────────────────

    [Fact]
    public void AutonomousLearning_DataSourceFinder_DefaultSources_ReturnsNonEmpty()
    {
        // Gate B: null config → DefaultSources used → Find() returns IsSuccess=true, Count ≥ 1.
        // Confirms the 5 Tier 1 .NET sources are always available without explicit config.
        var finder = new DataSourceFinder(Storage);
        var result = finder.Find();

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value!);
    }

    [Fact]
    public void AutonomousLearning_DataSourceFinder_EmptyConfig_ReturnsFailWithCode()
    {
        // Gate B: explicit empty list → Find() returns IsSuccess=false, ErrorCode="EMPTY_CONFIG".
        // Validates that a misconfigured finder fails fast rather than silently returning nothing.
        var finder = new DataSourceFinder(Storage, configured: Array.Empty<SourceEntry>());
        var result = finder.Find();

        Assert.False(result.IsSuccess);
        Assert.Equal("EMPTY_CONFIG", result.ErrorCode);
    }

    // ── Phase 3: DatasetPreprocessor (E20 gate) ──────────────────────────────────

    [Fact]
    public void AutonomousLearning_DatasetPreprocessor_StackDetectorRunsBeforeNormalization()
    {
        // Gate B: ISemanticCompiler29.Compile must be called during Process.
        //         Output domain must match what the compiler returned (proves E20 gate is wired).
        // E20 rule: StackDetector runs FIRST on raw text — normalization cannot precede it.
        var compiler  = new TrackingCompilerAL(SC29.Domain.CODE_Q);
        var cache     = new NullDatasetCacheAL();
        var pre       = new DatasetPreprocessor(Storage, compiler, cache);
        var source    = new SourceEntry("https://github.com/dotnet/runtime", Tier: 1, Trust: TrustLevel.High);

        var result = pre.Process("public class Foo { }", source);

        // Compiler was called — E20 gate exists in the pipeline.
        Assert.True(compiler.WasCalled, "ISemanticCompiler29 must be called in Process before normalization.");
        // Domain in result matches the compiler's output — not a hardcoded constant.
        Assert.True(result.IsSuccess);
        Assert.Equal("CODE_Q", result.Value!.Domain);
    }

    [Fact]
    public void AutonomousLearning_DatasetPreprocessor_LowTrustSource_Filtered()
    {
        // Gate B: source with TrustLevel.Low → Fail with LOW_TRUST_SOURCE before any processing.
        // LOW trust sources are excluded at the first guard — no compiler call occurs.
        var compiler = new TrackingCompilerAL(SC29.Domain.CODE_Q);
        var cache    = new NullDatasetCacheAL();
        var pre      = new DatasetPreprocessor(Storage, compiler, cache);
        var source   = new SourceEntry("https://unknown-blog.example.com", Tier: 3, Trust: TrustLevel.Low);

        var result = pre.Process("some code", source);

        Assert.False(result.IsSuccess);
        Assert.Equal("LOW_TRUST_SOURCE", result.ErrorCode);
        // Compiler must NOT have been called — filtering happens before E20 gate.
        Assert.False(compiler.WasCalled, "Compiler must not run for LOW trust sources.");
    }

    [Fact]
    public void AutonomousLearning_DatasetPreprocessor_EmptyText_ReturnsFail()
    {
        // Gate B: empty string → Fail with EMPTY_TEXT before any processing.
        // An empty example cannot be labeled and must be rejected immediately.
        var compiler = new TrackingCompilerAL(SC29.Domain.CODE_Q);
        var cache    = new NullDatasetCacheAL();
        var pre      = new DatasetPreprocessor(Storage, compiler, cache);
        var source   = new SourceEntry("https://github.com/dotnet/runtime", Tier: 1, Trust: TrustLevel.High);

        var result = pre.Process("   ", source);

        Assert.False(result.IsSuccess);
        Assert.Equal("EMPTY_TEXT", result.ErrorCode);
    }

    // ── Phase 3: DatasetWeighter (D-09 formula) ──────────────────────────────────

    [Fact]
    public void AutonomousLearning_DatasetWeighter_Formula_Produces4Point0ForStandardInput()
    {
        // Gate B (D-09): ComputeMaxWeight(1000, 5, 50) == 4.0.
        // Regression guard against the E44 failure: ×10 constant produced -98% accuracy.
        // The formula is total / (classes × rare), NOT total × 10.
        Assert.Equal(4.0, DatasetWeighter.ComputeMaxWeight(1000, 5, 50));
    }

    [Fact]
    public void AutonomousLearning_DatasetWeighter_ZeroRareExamples_ReturnsFail()
    {
        // Gate B: rareExamples=0 → Weight() returns Fail with ZERO_RARE error code.
        // Prevents divide-by-zero and signals a dataset configuration error to the caller.
        var weighter  = new DatasetWeighter(Storage);
        var examples  = new[] { new TrainingExample("p", "r", "CODE_Q", 1.0, DateTimeOffset.UtcNow) };
        var result    = weighter.Weight(examples, numClasses: 5, rareExamples: 0);

        Assert.False(result.IsSuccess);
        Assert.Equal("ZERO_RARE", result.ErrorCode);
    }

    // ── Phase 4: ReplayBuffer (E43 70/30 mix) ────────────────────────────────────

    [Fact]
    public void AutonomousLearning_ReplayBuffer_MixRatio_Is70New30Old()
    {
        // Gate B: 7 new examples + buffer with 10 old → Sample returns 10 total (7 new + 3 old).
        // Validates E43 formula: replayCount = round(7 × 0.3/0.7) = 3 → 7+3 = 10.
        var buffer = new ReplayBuffer(Storage, seed: 42);

        // Populate buffer with 10 "old" BUG examples.
        for (int i = 0; i < 10; i++)
            buffer.Add(new TrainingExample($"old-p{i}", $"old-r{i}", "BUG", 1.0, DateTimeOffset.UtcNow));

        // Create 7 "new" CODE_Q examples.
        var newExamples = Enumerable.Range(0, 7)
            .Select(i => new TrainingExample($"new-p{i}", $"new-r{i}", "CODE_Q", 1.0, DateTimeOffset.UtcNow))
            .ToArray();

        var batch = buffer.Sample(newExamples);

        // Total must be 10 (7 new + 3 replay): confirms the 70/30 mix ratio.
        Assert.Equal(10, batch.Count);
        // Exactly 7 CODE_Q (new) examples in the batch — replay does not displace new examples.
        Assert.Equal(7, batch.Count(e => e.Domain == "CODE_Q"));
        // Exactly 3 BUG (replay) examples in the batch — 30% of total.
        Assert.Equal(3, batch.Count(e => e.Domain == "BUG"));
    }

    [Fact]
    public void AutonomousLearning_ReplayBuffer_EmptyBuffer_Returns100PercentNew()
    {
        // Gate B: empty buffer → Sample returns newExamples unchanged (count=5, all new).
        // When no history exists, no catastrophic forgetting is possible — no mixing needed.
        var buffer      = new ReplayBuffer(Storage, seed: 42);
        var newExamples = Enumerable.Range(0, 5)
            .Select(i => new TrainingExample($"p{i}", $"r{i}", "CODE_Q", 1.0, DateTimeOffset.UtcNow))
            .ToArray();

        var batch = buffer.Sample(newExamples);

        // All 5 examples returned — no replay additions when buffer is empty.
        Assert.Equal(5, batch.Count);
        Assert.All(batch, e => Assert.Equal("CODE_Q", e.Domain));
    }

    // ── Phase 4: AutonomousTrainer ────────────────────────────────────────────────

    [Fact]
    public void AutonomousLearning_ReplayBuffer_CapsHistoryAtConfiguredCapacity()
    {
        var buffer = new ReplayBuffer(Storage, seed: 42, maxCapacity: 3);

        for (int i = 0; i < 5; i++)
            buffer.Add(new TrainingExample($"old-p{i}", $"old-r{i}", "BUG", 1.0, DateTimeOffset.UtcNow));

        Assert.Equal(3, buffer.Count);

        var batch = buffer.Sample(Enumerable.Range(0, 7)
            .Select(i => new TrainingExample($"new-p{i}", $"new-r{i}", "CODE_Q", 1.0, DateTimeOffset.UtcNow))
            .ToArray());

        var replayedPrompts = batch
            .Where(example => example.Domain == "BUG")
            .Select(example => example.Prompt)
            .OrderBy(prompt => prompt, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(["old-p2", "old-p3", "old-p4"], replayedPrompts);
    }

    [Fact]
    public void AutonomousLearning_ReplayBuffer_MinimumCapacityIsOne()
    {
        var buffer = new ReplayBuffer(Storage, seed: 42, maxCapacity: 0);

        buffer.Add(new TrainingExample("old-p0", "old-r0", "BUG", 1.0, DateTimeOffset.UtcNow));
        buffer.Add(new TrainingExample("old-p1", "old-r1", "BUG", 1.0, DateTimeOffset.UtcNow));

        Assert.Equal(1, buffer.Count);
        var batch = buffer.Sample([
            new TrainingExample("new-p0", "new-r0", "CODE_Q", 1.0, DateTimeOffset.UtcNow),
            new TrainingExample("new-p1", "new-r1", "CODE_Q", 1.0, DateTimeOffset.UtcNow),
            new TrainingExample("new-p2", "new-r2", "CODE_Q", 1.0, DateTimeOffset.UtcNow)
        ]);

        Assert.Single(batch, example => example.Domain == "BUG");
        Assert.Contains(batch, example => example.Prompt == "old-p1");
    }

    [Fact]
    public async Task AutonomousLearning_AutonomousTrainer_RunsCycleAndSavesAdapter()
    {
        // Gate B: 10 new examples (≥ MinExamples=5) → TrainAsync returns IsSuccess=true
        //         and Value contains an adapter path string.
        var stubTrainer = new StubIncrementalTrainerAL("adapters/CODE_Q.lora");
        var buffer      = new ReplayBuffer(Storage, seed: 42);
        var trainer     = new AutonomousTrainer(Storage, stubTrainer, buffer);
        var examples    = Enumerable.Range(0, 10)
            .Select(i => new TrainingExample($"p{i}", $"r{i}", "CODE_Q", 1.0, DateTimeOffset.UtcNow))
            .ToList();

        var result = await trainer.TrainAsync("CODE_Q", examples, []);

        Assert.True(result.IsSuccess);
        // Adapter path is non-empty — confirms IncrementalTrainer was called and returned output.
        Assert.False(string.IsNullOrEmpty(result.Value));
        // Buffer now holds the 10 new examples added after successful training.
        Assert.Equal(10, buffer.Count);
    }

    [Fact]
    public async Task AutonomousLearning_AutonomousTrainer_MinimumExamplesNotMet_ReturnsFail()
    {
        // Gate B: 2 new examples (< MinExamples=5) → TrainAsync returns Fail with INSUFFICIENT_EXAMPLES.
        // Training on too few examples produces unreliable gradient updates — fail fast.
        var stubTrainer = new StubIncrementalTrainerAL("adapters/CODE_Q.lora");
        var buffer      = new ReplayBuffer(Storage, seed: 42);
        var trainer     = new AutonomousTrainer(Storage, stubTrainer, buffer);
        var examples    = Enumerable.Range(0, 2)
            .Select(i => new TrainingExample($"p{i}", $"r{i}", "CODE_Q", 1.0, DateTimeOffset.UtcNow))
            .ToList();

        var result = await trainer.TrainAsync("CODE_Q", examples, []);

        Assert.False(result.IsSuccess);
        Assert.Equal("INSUFFICIENT_EXAMPLES", result.ErrorCode);
        // Buffer must NOT have been updated — guard fired before replay mixing.
        Assert.Equal(0, buffer.Count);
    }

    // ── Phase 5: AutonomousLearningPipeline (full 8-step cycle) ──────────────────

    [Fact]
    public async Task AutonomousLearning_Pipeline_IdleCheck_BlocksRunWhenActive()
    {
        // Gate B: system monitor reports CPU=80% → RunAsync returns Fail with SYSTEM_ACTIVE.
        // The E41 IdleDetector must block the cycle when the machine is not idle.
        var pipeline = BuildPipeline(
            cpuPercent:    80.0,  // ← above 20% threshold — machine is active
            baselineScore: 0.50,
            enhancedScore: 0.75,
            exampleCount:  10);

        var result = await pipeline.RunAsync("CODE_Q");

        Assert.False(result.IsSuccess);
        Assert.Equal("SYSTEM_ACTIVE", result.ErrorCode);
    }

    [Fact]
    public async Task AutonomousLearning_Pipeline_FullCycle_CompletesWithoutError()
    {
        // Gate B: all stubs succeed, 10 examples collected → RunAsync returns IsSuccess=true.
        // Confirms all 8 pipeline steps wire together without exception or Fail propagation.
        var pipeline = BuildPipeline(
            cpuPercent:    10.0,  // ← below 20% threshold — machine is idle
            baselineScore: 0.50,
            enhancedScore: 0.75,
            exampleCount:  10);

        var result = await pipeline.RunAsync("CODE_Q");

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value!.ExamplesProcessed);
    }

    [Fact]
    public async Task AutonomousLearning_Pipeline_BeforeAfterMeasurement_EnhancedExceedsBaseline()
    {
        // Gate B: baseline=0.50, enhanced=0.75 → EnhancedAccuracy > BaselineAccuracy.
        // Validates that the pipeline measures before AND after training and returns both values.
        // E28-E31 lesson: accuracy uses real LoRA scoring (IAccuracyEvaluator), not bigram proxy.
        var pipeline = BuildPipeline(
            cpuPercent:    10.0,
            baselineScore: 0.50,
            enhancedScore: 0.75,
            exampleCount:  10);

        var result = await pipeline.RunAsync("CODE_Q");

        Assert.True(result.IsSuccess);
        // Enhanced accuracy must exceed baseline — one training cycle produced a measurable delta.
        Assert.True(
            result.Value!.EnhancedAccuracy > result.Value!.BaselineAccuracy,
            $"Expected enhanced ({result.Value!.EnhancedAccuracy}) > baseline ({result.Value!.BaselineAccuracy}).");
    }

    [Fact]
    public async Task AutonomousLearning_Pipeline_AccuracyRegression_DoesNotRegisterAdapter()
    {
        // Gate B: baseline=0.80, enhanced=0.70 -> fail and keep the registry unchanged.
        // Candidate adapters are promoted only when the after-score is strictly better.
        var setup = BuildPipelineWithRegistry(
            cpuPercent:    10.0,
            baselineScore: 0.80,
            enhancedScore: 0.70,
            exampleCount:  10);

        var result = await setup.Pipeline.RunAsync("CODE_Q");
        var adapter = setup.Registry.GetAdapter("CODE_Q");

        Assert.False(result.IsSuccess);
        Assert.Equal("ACCURACY_NOT_IMPROVED", result.ErrorCode);
        Assert.True(adapter.IsSuccess, adapter.ErrorMessage);
        Assert.Null(adapter.Value);
    }

    // ── Pipeline builder helper ───────────────────────────────────────────────────

    // BuildPipeline — construct AutonomousLearningPipeline with fully stubbed dependencies.
    // cpuPercent: controls whether IdleDetector passes or blocks.
    // baselineScore / enhancedScore: accuracy values returned before and after training.
    // exampleCount: how many examples the stub data provider returns.
    private static AutonomousLearningPipeline BuildPipeline(
        double cpuPercent,
        double baselineScore,
        double enhancedScore,
        int    exampleCount)
        => BuildPipelineWithRegistry(cpuPercent, baselineScore, enhancedScore, exampleCount).Pipeline;

    private static (AutonomousLearningPipeline Pipeline, DomainModelRegistry Registry) BuildPipelineWithRegistry(
        double cpuPercent,
        double baselineScore,
        double enhancedScore,
        int    exampleCount)
    {
        // Stub system monitor: returns the configured CPU% with all other conditions idle.
        var systemMonitor  = new StubSystemMonitorAL(new SystemStatus(
            CpuPercent: cpuPercent, InputIdleMinutes: 20.0,
            IsCharging: true, BatteryPercent: 80.0, DiskFreeGb: 10.0));

        // Stub data provider: returns exampleCount pre-built CODE_Q examples.
        var dataProvider   = new StubDataProviderAL(exampleCount);

        // Stub accuracy evaluator: returns baselineScore first, then enhancedScore.
        var evaluator      = new SequentialAccuracyAL(baselineScore, enhancedScore);

        // Stub IncrementalTrainer: returns a fixed adapter path without gradient computation.
        var stubTrainer    = new StubIncrementalTrainerAL("adapters/CODE_Q.lora");
        var buffer         = new ReplayBuffer(Storage, seed: 42);
        var registry       = new DomainModelRegistry(Storage, new NullCodeWriterAL(), "test-adapters");

        // Assemble all components.
        var pipeline = new AutonomousLearningPipeline(
            storage:       Storage,
            idleDetector:  new IdleDetector(Storage),
            systemMonitor: systemMonitor,
            dataProvider:  dataProvider,
            weighter:      new DatasetWeighter(Storage),
            trainer:       new AutonomousTrainer(Storage, stubTrainer, buffer),
            registry:      registry,
            evaluator:     evaluator);

        return (pipeline, registry);
    }
}

// ── File-scoped fakes ─────────────────────────────────────────────────────────
// Suffix "AL" prevents name collision with fakes in other test files (e.g., "CL" in ContinuousLearningTests).

// StubSystemMonitorAL — ISystemMonitor stub that returns a configurable SystemStatus snapshot.
// Used by Pipeline tests to control CPU/battery/disk/idle values for IdleDetector.
file sealed class StubSystemMonitorAL : ISystemMonitor
{
    // _status — the fixed status returned from every Sample call.
    private readonly SystemStatus _status;

    // Constructor: configure the status this stub will report.
    public StubSystemMonitorAL(SystemStatus status) => _status = status;

    // Sample — returns the configured status snapshot without querying OS metrics.
    public OperationResult<SystemStatus> Sample()
        => OperationResult<SystemStatus>.Ok(_status);
}

// StubDataProviderAL — ITrainingDataProvider stub that returns N synthetic CODE_Q examples.
// Used by Pipeline tests to supply a controlled example set without network access.
file sealed class StubDataProviderAL : ITrainingDataProvider
{
    // _count — number of synthetic TrainingExample values to return.
    private readonly int _count;

    // Constructor: configure how many examples are returned from CollectAsync.
    public StubDataProviderAL(int count) => _count = count;

    // CollectAsync — builds and returns _count synthetic CODE_Q examples immediately.
    public Task<OperationResult<IReadOnlyList<TrainingExample>>> CollectAsync(
        string domain, CancellationToken ct)
    {
        // Build synthetic examples; domain is "CODE_Q" regardless of the requested domain.
        var examples = (IReadOnlyList<TrainingExample>)Enumerable.Range(0, _count)
            .Select(i => new TrainingExample($"p{i}", $"r{i}", "CODE_Q", 1.0, DateTimeOffset.UtcNow))
            .ToList();
        return Task.FromResult(OperationResult<IReadOnlyList<TrainingExample>>.Ok(examples));
    }
}

// SequentialAccuracyAL — IAccuracyEvaluator stub that returns accuracy values in sequence.
// First Evaluate call returns values[0] (baseline); second call returns values[1] (enhanced).
// Validates that the pipeline measures accuracy both before and after training.
file sealed class SequentialAccuracyAL : IAccuracyEvaluator
{
    // _values — queue of accuracy values to return in order.
    private readonly Queue<double> _values;

    // Constructor: configure the sequence of accuracy values (typically baseline then enhanced).
    public SequentialAccuracyAL(params double[] values)
        => _values = new Queue<double>(values);

    // Evaluate — dequeues and returns the next accuracy value in the configured sequence.
    public OperationResult<double> Evaluate(string domain)
        => _values.Count > 0
            ? OperationResult<double>.Ok(_values.Dequeue())
            : OperationResult<double>.Fail("Accuracy sequence exhausted.", "EXHAUSTED");
}

// NullCodeWriterAL — ICodeWriter no-op stub used by DomainModelRegistry in Pipeline tests.
// Write calls succeed silently; no files are created on disk during tests.
file sealed class NullCodeWriterAL : ICodeWriter
{
    // WriteFileAsync — reports success without writing anything; avoids disk I/O in tests.
    public Task<OperationResult<bool>> WriteFileAsync(string path, string content)
        => Task.FromResult(OperationResult<bool>.Ok(true));
}

// TrackingCompilerAL — ISemanticCompiler29 stub that records whether Compile was called.
// Used by DatasetPreprocessor tests to verify the E20 gate is wired in the pipeline.
file sealed class TrackingCompilerAL : SC29.ISemanticCompiler29
{
    // Configured domain returned from every Compile call.
    private readonly SC29.Domain _domain;

    // WasCalled — set to true the first time Compile is invoked.
    public bool WasCalled { get; private set; }

    // Constructor: configure the domain the stub will classify any input as.
    public TrackingCompilerAL(SC29.Domain domain) => _domain = domain;

    // Compile — records the call and returns the configured domain.
    public SC29.OperationResult<SC29.PipelineOutput> Compile(SC29.PipelineInput input)
    {
        // Mark as called so the test can assert E20 gate ran.
        WasCalled = true;
        // Return a minimal PipelineOutput with the configured domain.
        return SC29.OperationResult<SC29.PipelineOutput>.Ok(
            new SC29.PipelineOutput(_domain, [], "csharp", 10.0));
    }
}

// NullDatasetCacheAL — IDomainDatasetCache no-op stub for DatasetPreprocessor tests.
// Accepts Add calls silently; tests do not need to inspect cached content.
file sealed class NullDatasetCacheAL : IDomainDatasetCache
{
    // Add — always succeeds; example is discarded (no storage in tests).
    public OperationResult<bool> Add(TrainingExample example)
        => OperationResult<bool>.Ok(true);

    // GetByDomain — returns empty list; tests do not read from cache.
    public OperationResult<IReadOnlyList<TrainingExample>> GetByDomain(string domain)
        => OperationResult<IReadOnlyList<TrainingExample>>.Ok(
               (IReadOnlyList<TrainingExample>)Array.Empty<TrainingExample>());

    // TotalCount — returns 0; tests do not verify cache accumulation.
    public OperationResult<int> TotalCount()
        => OperationResult<int>.Ok(0);

    // FlushAsync — succeeds silently; no files written in tests.
    public Task<OperationResult<bool>> FlushAsync()
        => Task.FromResult(OperationResult<bool>.Ok(true));
}

// StubIncrementalTrainerAL — IIncrementalTrainer stub that returns a fixed adapter path.
// Used by AutonomousTrainer tests to avoid real LoRA gradient computation.
file sealed class StubIncrementalTrainerAL : IIncrementalTrainer
{
    // _adapterPath — the path this stub will report as the trained adapter location.
    private readonly string _adapterPath;

    // Constructor: configure the adapter path returned on success.
    public StubIncrementalTrainerAL(string adapterPath) => _adapterPath = adapterPath;

    // TrainAsync — returns the configured adapter path without performing any training.
    public Task<OperationResult<string>> TrainAsync(
        string domain,
        IReadOnlyList<TrainingExample> examples,
        IReadOnlyList<DpoPair> dpoPairs)
        => Task.FromResult(OperationResult<string>.Ok(_adapterPath));
}

// NullSessionStorageAL — ISessionStorage that succeeds silently; tests do not use persisted state.
file sealed class NullSessionStorageAL : ISessionStorage
{
    // SaveAsync — report success without writing anything; avoids disk I/O in tests.
    public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
        => Task.FromResult(OperationResult<bool>.Ok(true));

    // LoadAsync — returns Fail; no state was ever saved, so nothing can be loaded.
    public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
        => Task.FromResult(OperationResult<AgentState>.Fail("Not persisted.", "NULL_STORAGE"));

    // ListSessionsAsync — returns an empty list; no sessions exist in the null store.
    public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
        => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(
               (IReadOnlyList<string>)Array.Empty<string>()));
}
