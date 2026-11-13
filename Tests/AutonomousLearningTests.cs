// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AutonomousLearning;
using AgentInfrastructure;
using ContinuousLearning;
using LoRA;
using TransformerModel;
using TransformerModel.Config;
using SC29 = SemanticCompiler29;

namespace LlmFromScratch.Tests;

// 16.AutonomousLearning — night-time dataset collection + automated LoRA training cycle.
// Experiment basis: E41 IdleDetector (+100%), E40V2 TrustScorer (+50%), E43 ReplayBuffer (+23%).
// Gate B: dotnet test --filter "AutonomousLearning_" must remain fully green.
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
            buffer.Add(new TrainingExample($"old-p{i}", $"old-r{i}", "CODE_Q", 1.0, DateTimeOffset.UtcNow));

        // Create 7 "new" CODE_Q examples.
        var newExamples = Enumerable.Range(0, 7)
            .Select(i => new TrainingExample($"new-p{i}", $"new-r{i}", "CODE_Q", 1.0, DateTimeOffset.UtcNow))
            .ToArray();

        var batch = buffer.Sample(newExamples);

        // Total must be 10 (7 new + 3 replay): confirms the 70/30 mix ratio.
        Assert.Equal(10, batch.Count);
        // Exactly 7 CODE_Q (new) examples in the batch — replay does not displace new examples.
        Assert.Equal(7, batch.Count(e => e.Prompt.StartsWith("new-", StringComparison.Ordinal)));
        // Exactly 3 BUG (replay) examples in the batch — 30% of total.
        Assert.Equal(3, batch.Count(e => e.Prompt.StartsWith("old-", StringComparison.Ordinal)));
        Assert.All(batch, e => Assert.Equal("CODE_Q", e.Domain));
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

    [Fact]
    public void AutonomousLearning_ReplayBuffer_NeverMixesAnotherAdapterDomain()
    {
        var buffer = new ReplayBuffer(Storage, seed: 42);
        for (var index = 0; index < 10; index++)
        {
            buffer.Add(new TrainingExample(
                $"bug-p{index}",
                $"bug-r{index}",
                "BUG",
                1.0,
                DateTimeOffset.UtcNow));
        }
        var newExamples = Enumerable.Range(0, 7)
            .Select(index => new TrainingExample(
                $"code-p{index}",
                $"code-r{index}",
                "CODE_Q",
                1.0,
                DateTimeOffset.UtcNow))
            .ToArray();

        var batch = buffer.Sample(newExamples);

        Assert.Equal(7, batch.Count);
        Assert.All(batch, example => Assert.Equal("CODE_Q", example.Domain));
    }

    // ── Phase 4: AutonomousTrainer ────────────────────────────────────────────────

    [Fact]
    public void AutonomousLearning_ReplayBuffer_CapsHistoryAtConfiguredCapacity()
    {
        var buffer = new ReplayBuffer(Storage, seed: 42, maxCapacity: 3);

        for (int i = 0; i < 5; i++)
            buffer.Add(new TrainingExample($"old-p{i}", $"old-r{i}", "CODE_Q", 1.0, DateTimeOffset.UtcNow));

        Assert.Equal(3, buffer.Count);

        var batch = buffer.Sample(Enumerable.Range(0, 7)
            .Select(i => new TrainingExample($"new-p{i}", $"new-r{i}", "CODE_Q", 1.0, DateTimeOffset.UtcNow))
            .ToArray());

        var replayedPrompts = batch
            .Where(example => example.Prompt.StartsWith("old-", StringComparison.Ordinal))
            .Select(example => example.Prompt)
            .OrderBy(prompt => prompt, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(["old-p2", "old-p3", "old-p4"], replayedPrompts);
    }

    [Fact]
    public void AutonomousLearning_ReplayBuffer_MinimumCapacityIsOne()
    {
        var buffer = new ReplayBuffer(Storage, seed: 42, maxCapacity: 0);

        buffer.Add(new TrainingExample("old-p0", "old-r0", "CODE_Q", 1.0, DateTimeOffset.UtcNow));
        buffer.Add(new TrainingExample("old-p1", "old-r1", "CODE_Q", 1.0, DateTimeOffset.UtcNow));

        Assert.Equal(1, buffer.Count);
        var batch = buffer.Sample([
            new TrainingExample("new-p0", "new-r0", "CODE_Q", 1.0, DateTimeOffset.UtcNow),
            new TrainingExample("new-p1", "new-r1", "CODE_Q", 1.0, DateTimeOffset.UtcNow),
            new TrainingExample("new-p2", "new-r2", "CODE_Q", 1.0, DateTimeOffset.UtcNow)
        ]);

        Assert.Single(batch, example => example.Prompt.StartsWith("old-", StringComparison.Ordinal));
        Assert.Contains(batch, example => example.Prompt == "old-p1");
    }

    [Fact]
    public async Task AutonomousLearning_AutonomousTrainer_RunsCycleAndSavesAdapter()
    {
        // Gate B: 10 new examples (≥ MinExamples=5) → TrainAsync returns IsSuccess=true
        //         and Value contains an adapter path string.
        using var artifact = new AdapterArtifactFixtureAL();
        var stubTrainer = new StubIncrementalTrainerAL(artifact.AdapterPath);
        var buffer      = new ReplayBuffer(Storage, seed: 42);
        var trainer     = new AutonomousTrainer(Storage, stubTrainer, buffer);
        var examples    = Enumerable.Range(0, 10)
            .Select(i => ProvenanceTestFactory.WithSource(
                new TrainingExample($"p{i}", $"r{i}", "CODE_Q", 1.0, DateTimeOffset.UtcNow),
                $"autonomous/direct/{i}"))
            .ToList();

        var result = await trainer.TrainAsync("CODE_Q", examples, []);

        Assert.True(result.IsSuccess);
        // Adapter path is non-empty — confirms IncrementalTrainer was called and returned output.
        Assert.False(string.IsNullOrEmpty(result.Value!.AdapterPath));
        // Buffer now holds the 10 new examples added after successful training.
        Assert.Equal(10, buffer.Count);
    }

    [Fact]
    public async Task AutonomousLearning_AutonomousTrainer_ForwardsCancellationToken()
    {
        using var artifact = new AdapterArtifactFixtureAL();
        var stubTrainer = new StubIncrementalTrainerAL(artifact.AdapterPath);
        var trainer = new AutonomousTrainer(
            Storage,
            stubTrainer,
            new ReplayBuffer(Storage, seed: 42));
        var examples = Enumerable.Range(0, 5)
            .Select(i => ProvenanceTestFactory.WithSource(
                new TrainingExample(
                    $"p{i}", $"r{i}", "CODE_Q", 1.0, DateTimeOffset.UtcNow),
                $"autonomous/cancellation/{i}"))
            .ToList();
        using var cancellation = new CancellationTokenSource();

        var result = await trainer.TrainAsync(
            "CODE_Q",
            examples,
            [],
            cancellation.Token);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(cancellation.Token, stubTrainer.LastCancellationToken);
    }

    [Fact]
    public async Task AutonomousLearning_TrainPrepared_RejectsManifestNotBoundToFrozenBatch()
    {
        using var artifact = new AdapterArtifactFixtureAL();
        var stubTrainer = new StubIncrementalTrainerAL(artifact.AdapterPath);
        var buffer = new ReplayBuffer(Storage, seed: 42);
        var trainer = new AutonomousTrainer(Storage, stubTrainer, buffer);
        var examples = Enumerable.Range(0, AutonomousTrainer.MinExamples)
            .Select(index => ProvenanceTestFactory.WithSource(
                new TrainingExample(
                    $"p{index}",
                    $"r{index}",
                    "CODE_Q",
                    1d,
                    DateTimeOffset.UtcNow),
                $"autonomous/forged-batch/{index}"))
            .ToArray();
        var prepared = trainer.PrepareBatch("CODE_Q", examples, []).Value!;
        var forged = prepared with
        {
            TrainingSources = prepared.TrainingSources with
            {
                Sha256 = new string('f', 64),
            },
        };

        var result = await trainer.TrainPreparedAsync(forged);

        Assert.False(result.IsSuccess);
        Assert.Equal("CANDIDATE_TRAINING_MANIFEST_MISMATCH", result.ErrorCode);
        Assert.Equal(0, stubTrainer.CallCount);
        Assert.Equal(0, buffer.Count);
    }

    [Fact]
    public async Task AutonomousLearning_AutonomousTrainer_MinimumExamplesNotMet_ReturnsFail()
    {
        // Gate B: 2 new examples (< MinExamples=5) → TrainAsync returns Fail with INSUFFICIENT_EXAMPLES.
        // Training on too few examples produces unreliable gradient updates — fail fast.
        using var artifact = new AdapterArtifactFixtureAL();
        var stubTrainer = new StubIncrementalTrainerAL(artifact.AdapterPath);
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

    [Fact]
    public void AutonomousLearning_AutonomousTrainer_MixedNewDomainsFailBeforeReplayOrTraining()
    {
        using var artifact = new AdapterArtifactFixtureAL();
        var stubTrainer = new StubIncrementalTrainerAL(artifact.AdapterPath);
        var buffer = new ReplayBuffer(Storage, seed: 42);
        var trainer = new AutonomousTrainer(Storage, stubTrainer, buffer);
        var examples = Enumerable.Range(0, 5)
            .Select(index => ProvenanceTestFactory.WithSource(
                new TrainingExample(
                    $"p{index}",
                    $"r{index}",
                    index == 4 ? "BUG" : "CODE_Q",
                    1.0,
                    DateTimeOffset.UtcNow),
                $"autonomous/mixed/{index}"))
            .ToArray();

        var result = trainer.PrepareBatch("CODE_Q", examples, []);

        Assert.False(result.IsSuccess);
        Assert.Equal("INVALID_TRAINING_EXAMPLE", result.ErrorCode);
        Assert.Equal(0, buffer.Count);
        Assert.Equal(0, stubTrainer.CallCount);
    }

    // ── Phase 5: AutonomousLearningPipeline (full 8-step cycle) ──────────────────

    [Fact]
    public async Task AutonomousLearning_Pipeline_IdleCheck_BlocksRunWhenActive()
    {
        // Gate B: system monitor reports CPU=80% → RunAsync returns Fail with SYSTEM_ACTIVE.
        // The E41 IdleDetector must block the cycle when the machine is not idle.
        using var setup = BuildPipelineWithRegistry(
            cpuPercent:    80.0,  // ← above 20% threshold — machine is active
            baselineScore: 0.50,
            enhancedScore: 0.75,
            exampleCount:  10);

        var result = await setup.Pipeline.RunAsync("CODE_Q");

        Assert.False(result.IsSuccess);
        Assert.Equal("SYSTEM_ACTIVE", result.ErrorCode);
    }

    [Fact]
    public async Task AutonomousLearning_Pipeline_FullCycle_CompletesWithoutError()
    {
        // Gate B: all stubs succeed, 10 examples collected → RunAsync returns IsSuccess=true.
        // Confirms all 8 pipeline steps wire together without exception or Fail propagation.
        using var setup = BuildPipelineWithRegistry(
            cpuPercent:    10.0,  // ← below 20% threshold — machine is idle
            baselineScore: 0.50,
            enhancedScore: 0.75,
            exampleCount:  10);

        var result = await setup.Pipeline.RunAsync("CODE_Q");
        var registered = setup.Registry.GetAdapter("CODE_Q");

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value!.ExamplesProcessed);
        Assert.True(Path.IsPathFullyQualified(result.Value.AdapterPath));
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        Assert.Equal(result.Value.AdapterPath, registered.Value!.AdapterPath);
    }

    [Fact]
    public async Task AutonomousLearning_Pipeline_BeforeAfterMeasurement_EnhancedExceedsBaseline()
    {
        // Gate B: baseline=0.50, enhanced=0.75 → EnhancedAccuracy > BaselineAccuracy.
        // Validates that the pipeline measures before AND after training and returns both values.
        // Gate-logic fixture only: this test injects target-bound scores; the production
        // HeldOutLoraAccuracyEvaluator model run is covered by its dedicated tests.
        using var setup = BuildPipelineWithRegistry(
            cpuPercent:    10.0,
            baselineScore: 0.50,
            enhancedScore: 0.75,
            exampleCount:  10);

        var result = await setup.Pipeline.RunAsync("CODE_Q");

        Assert.True(result.IsSuccess);
        // Enhanced accuracy must exceed baseline — one training cycle produced a measurable delta.
        Assert.True(
            result.Value!.EnhancedAccuracy > result.Value!.BaselineAccuracy,
            $"Expected enhanced ({result.Value!.EnhancedAccuracy}) > baseline ({result.Value!.BaselineAccuracy}).");
    }

    [Fact]
    public async Task AutonomousLearning_Pipeline_ForwardsCancellationTokenToIncrementalTrainer()
    {
        using var setup = BuildPipelineWithRegistry(
            cpuPercent: 10.0,
            baselineScore: 0.50,
            enhancedScore: 0.75,
            exampleCount: 10);
        using var cancellation = new CancellationTokenSource();

        var result = await setup.Pipeline.RunAsync("CODE_Q", cancellation.Token);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(
            cancellation.Token,
            setup.IncrementalTrainer.LastCancellationToken);
    }

    [Fact]
    public async Task AutonomousLearning_Pipeline_AccuracyRegression_DoesNotRegisterAdapter()
    {
        // Gate B: baseline=0.80, enhanced=0.70 -> fail and keep the registry unchanged.
        // Candidate adapters are promoted only when the after-score is strictly better.
        using var setup = BuildPipelineWithRegistry(
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

    [Fact]
    public async Task AutonomousLearning_Pipeline_InvalidArtifactCannotBeRegistered()
    {
        var artifact = new AdapterArtifactFixtureAL(structurallyValid: false);
        using var setup = BuildPipelineWithRegistry(
            cpuPercent: 10.0,
            baselineScore: 0.50,
            enhancedScore: 0.75,
            exampleCount: 10,
            artifact: artifact);

        var result = await setup.Pipeline.RunAsync("CODE_Q");
        var lookup = setup.Registry.GetAdapter("CODE_Q");

        Assert.False(result.IsSuccess);
        Assert.Equal("ADAPTER_CHECKPOINT_INVALID", result.ErrorCode);
        Assert.True(lookup.IsSuccess, lookup.ErrorMessage);
        Assert.Null(lookup.Value);
    }

    [Fact]
    public async Task AutonomousLearning_Pipeline_RequiresSuccessfulExactRegistrationReadback()
    {
        var cases = new Func<string, ContinuousLearning.IDomainModelRegistry>[]
        {
            path => new ScriptedRegistryAL(
                OperationResult<bool>.Ok(false),
                OperationResult<ModelAdapter?>.Ok(null),
                OperationResult<ModelAdapter?>.Ok(null)),
            path => new ScriptedRegistryAL(
                OperationResult<bool>.Ok(true),
                OperationResult<ModelAdapter?>.Ok(null),
                OperationResult<ModelAdapter?>.Ok(null)),
            path => new ScriptedRegistryAL(
                OperationResult<bool>.Ok(true),
                OperationResult<ModelAdapter?>.Ok(null),
                OperationResult<ModelAdapter?>.Ok(
                    new ModelAdapter(
                        "CODE_Q",
                        path + ".different",
                        10,
                        DateTimeOffset.UtcNow))),
            path => new ScriptedRegistryAL(
                OperationResult<bool>.Ok(true),
                OperationResult<ModelAdapter?>.Ok(null),
                OperationResult<ModelAdapter?>.Ok(
                    new ModelAdapter(
                        "BUG",
                        path,
                        10,
                        DateTimeOffset.UtcNow))),
        };

        foreach (var buildRegistry in cases)
        {
            var artifact = new AdapterArtifactFixtureAL();
            using var setup = BuildPipelineWithRegistry(
                cpuPercent: 10.0,
                baselineScore: 0.50,
                enhancedScore: 0.75,
                exampleCount: 10,
                artifact: artifact,
                registryOverride: buildRegistry(artifact.AdapterPath));

            var result = await setup.Pipeline.RunAsync("CODE_Q");

            Assert.False(result.IsSuccess);
            Assert.Equal("ADAPTER_REGISTRATION_INCOMPLETE", result.ErrorCode);
        }
    }

    [Fact]
    public async Task AutonomousLearning_Pipeline_CancellationDuringSuccessfulRegisterFinishesCommittedReadback()
    {
        using var cancellation = new CancellationTokenSource();
        var artifact = new AdapterArtifactFixtureAL();
        var registry = new CancelDuringRegisterRegistryAL(cancellation);
        using var setup = BuildPipelineWithRegistry(
            cpuPercent: 10.0,
            baselineScore: 0.50,
            enhancedScore: 0.75,
            exampleCount: 10,
            artifact: artifact,
            registryOverride: registry);

        var result = await setup.Pipeline.RunAsync(
            "CODE_Q",
            cancellation.Token);

        Assert.True(cancellation.IsCancellationRequested);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, registry.RegisterCount);
        Assert.Equal(2, registry.GetCount);
        Assert.Equal("CODE_Q", registry.RegisteredAdapter!.Domain);
        Assert.Equal(
            Path.GetFullPath(artifact.AdapterPath),
            result.Value!.AdapterPath);
        Assert.Equal(
            result.Value.AdapterPath,
            registry.RegisteredAdapter.AdapterPath);
    }

    [Fact]
    public async Task AutonomousLearning_Pipeline_AccuracyTarget_ColdStartScoresExactUnregisteredCandidateInOrder()
    {
        var events = new List<string>();
        var registry = new RecordingRegistryAL(initialAdapter: null, events);
        ModelAdapter? evaluatedCandidate = null;
        var evaluator = new TargetAwareAccuracyAL((domain, targetAdapter) =>
        {
            Assert.Equal("CODE_Q", domain);
            if (targetAdapter is null)
            {
                events.Add("Eval:baseline");
                Assert.Null(registry.CurrentAdapter);
                Assert.Equal(0, registry.RegisterCount);
                return OperationResult<double>.Ok(0.42);
            }

            events.Add("Eval:candidate");
            evaluatedCandidate = targetAdapter;
            Assert.Null(registry.CurrentAdapter);
            Assert.Equal(0, registry.RegisterCount);
            return OperationResult<double>.Ok(0.73);
        });
        using var setup = BuildPipelineWithRegistry(
            cpuPercent: 10.0,
            baselineScore: 0.0,
            enhancedScore: 0.0,
            exampleCount: 10,
            registryOverride: registry,
            evaluatorOverride: evaluator,
            events: events);

        var result = await setup.Pipeline.RunAsync("CODE_Q");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(0.42, result.Value!.BaselineAccuracy);
        Assert.Equal(0.73, result.Value.EnhancedAccuracy);
        Assert.Equal(2, evaluator.Calls.Count);
        Assert.Null(evaluator.Calls[0].TargetAdapter);
        Assert.Equal(AccuracyEvaluationPhase.Baseline, evaluator.Calls[0].Phase);
        Assert.Equal(AccuracyEvaluationPhase.Candidate, evaluator.Calls[1].Phase);
        Assert.Null(evaluator.Calls[0].ExpectedArtifacts);
        Assert.NotNull(evaluator.Calls[1].ExpectedArtifacts);
        var actualSourceIds = evaluator.Calls[0].PlannedTrainingSources.SourceIds;
        Assert.Equal(
            Enumerable.Range(0, 10)
                .Select(i => ProvenanceTestFactory.SourceId(
                    $"autonomous/{i}",
                    $"p{i}\nr{i}"))
                .OrderBy(static source => source, StringComparer.Ordinal)
                .ToArray(),
            actualSourceIds.ToArray());
        Assert.Equal(
            evaluator.Calls[0].PlannedTrainingSources.Sha256,
            evaluator.Calls[1].PlannedTrainingSources.Sha256);
        Assert.Equal(
            evaluator.Calls[1].PlannedTrainingSources.Sha256,
            evaluatedCandidate!.Provenance!.TrainingSources.Sha256);
        Assert.Same(evaluatedCandidate, registry.OfferedAdapter);
        Assert.Equal(
            ["Get:baseline", "Eval:baseline", "Train", "Eval:candidate", "Register", "Get:readback"],
            events);
    }

    [Fact]
    public async Task AutonomousLearning_Pipeline_AccuracyTarget_ExistingAdapterIsReplacedOnlyAfterCandidateScore()
    {
        var events = new List<string>();
        using var oldArtifact = new AdapterArtifactFixtureAL();
        var oldExample = new TrainingExample(
            "old-p",
            "old-r",
            "CODE_Q",
            1.0,
            DateTimeOffset.UtcNow.AddDays(-1),
            ProvenanceTestFactory.SourceId("old/source", "old-p\nold-r"));
        var oldAdapter = ProvenanceTestFactory.Adapter(
            "CODE_Q",
            oldArtifact.AdapterPath,
            [oldExample]) with { TrainedAt = DateTimeOffset.UtcNow.AddDays(-1) };
        var registry = new RecordingRegistryAL(oldAdapter, events);
        ModelAdapter? evaluatedCandidate = null;
        var evaluator = new TargetAwareAccuracyAL((domain, targetAdapter) =>
        {
            Assert.Equal("CODE_Q", domain);
            if (ReferenceEquals(targetAdapter, oldAdapter))
            {
                events.Add("Eval:baseline");
                Assert.Same(oldAdapter, registry.CurrentAdapter);
                return OperationResult<double>.Ok(0.51);
            }

            events.Add("Eval:candidate");
            evaluatedCandidate = targetAdapter;
            Assert.NotNull(targetAdapter);
            Assert.NotSame(oldAdapter, targetAdapter);
            Assert.Same(oldAdapter, registry.CurrentAdapter);
            Assert.Equal(0, registry.RegisterCount);
            return OperationResult<double>.Ok(0.79);
        });
        using var setup = BuildPipelineWithRegistry(
            cpuPercent: 10.0,
            baselineScore: 0.0,
            enhancedScore: 0.0,
            exampleCount: 10,
            registryOverride: registry,
            evaluatorOverride: evaluator,
            events: events);

        var result = await setup.Pipeline.RunAsync("CODE_Q");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Same(oldAdapter, evaluator.Calls[0].TargetAdapter);
        Assert.Same(evaluatedCandidate, evaluator.Calls[1].TargetAdapter);
        Assert.Contains(
            oldExample.SourceId!,
            evaluator.Calls[0].PlannedTrainingSources.SourceIds);
        Assert.Equal(
            evaluator.Calls[0].PlannedTrainingSources.Sha256,
            evaluator.Calls[1].PlannedTrainingSources.Sha256);
        Assert.Equal(
            evaluator.Calls[1].PlannedTrainingSources.Sha256,
            evaluatedCandidate!.Provenance!.TrainingSources.Sha256);
        Assert.Same(evaluatedCandidate, registry.OfferedAdapter);
        Assert.NotSame(oldAdapter, registry.CurrentAdapter);
        Assert.Equal(
            ["Get:baseline", "Eval:baseline", "Train", "Eval:candidate", "Register", "Get:readback"],
            events);
    }

    [Fact]
    public async Task AutonomousLearning_Pipeline_RejectsEvaluatorResultNotBoundToCandidateProvenance()
    {
        var artifacts = new AccuracyEvaluationArtifactIdentity(
            new string('1', 64),
            new string('2', 64),
            new string('3', 64));
        var evaluator = new DelegateAccuracyEvaluatorAL(request =>
            OperationResult<AccuracyEvaluationResult>.Ok(
                new AccuracyEvaluationResult(
                    request.Phase == AccuracyEvaluationPhase.Baseline ? 0.4 : 0.8,
                    request.ExpectedArtifacts ?? artifacts,
                    request.Phase == AccuracyEvaluationPhase.Candidate
                        ? new string('f', 64)
                        : null,
                    request.PlannedTrainingSources.Sha256)));
        var registry = new RecordingRegistryAL(null, []);
        using var setup = BuildPipelineWithRegistry(
            cpuPercent: 10.0,
            baselineScore: 0.0,
            enhancedScore: 0.0,
            exampleCount: 10,
            registryOverride: registry,
            evaluatorOverride: evaluator);

        var result = await setup.Pipeline.RunAsync("CODE_Q");

        Assert.False(result.IsSuccess);
        Assert.Equal("ACCURACY_EVALUATION_PROVENANCE_MISMATCH", result.ErrorCode);
        Assert.Equal(1, setup.IncrementalTrainer.CallCount);
        Assert.Equal(0, registry.RegisterCount);
    }

    [Fact]
    public async Task AutonomousLearning_Pipeline_AccuracyFailure_BaselineRegistryFailPropagatesExactlyAndStops()
    {
        var registry = new ScriptedRegistryAL(
            OperationResult<bool>.Ok(true),
            OperationResult<ModelAdapter?>.Fail(
                "stored adapter rejected",
                "STORED_ADAPTER_REJECTED"));
        var evaluator = new TargetAwareAccuracyAL(
            (_, _) => OperationResult<double>.Ok(0.5));
        using var setup = BuildPipelineWithRegistry(
            10.0, 0.0, 0.0, 10,
            registryOverride: registry,
            evaluatorOverride: evaluator);

        var result = await setup.Pipeline.RunAsync("CODE_Q");

        Assert.False(result.IsSuccess);
        Assert.Equal("stored adapter rejected", result.ErrorMessage);
        Assert.Equal("STORED_ADAPTER_REJECTED", result.ErrorCode);
        Assert.Empty(evaluator.Calls);
        Assert.Equal(0, setup.IncrementalTrainer.CallCount);
        Assert.Equal(0, registry.RegisterCount);
    }

    [Fact]
    public async Task AutonomousLearning_Pipeline_MalformedBaselineProvenanceFailsBeforeEvaluationOrTraining()
    {
        var events = new List<string>();
        using var oldArtifact = new AdapterArtifactFixtureAL();
        var oldAdapter = ProvenanceTestFactory.Adapter(
            "CODE_Q",
            oldArtifact.AdapterPath,
            [ProvenanceTestFactory.WithSource(
                new TrainingExample(
                    "old-p",
                    "old-r",
                    "CODE_Q",
                    1d,
                    DateTimeOffset.UtcNow),
                "old/malformed")]);
        oldAdapter = oldAdapter with
        {
            Provenance = oldAdapter.Provenance! with
            {
                TrainingSources = null!,
            },
        };
        var registry = new RecordingRegistryAL(oldAdapter, events);
        var evaluator = new TargetAwareAccuracyAL(
            (_, _) => OperationResult<double>.Ok(0.5));
        using var setup = BuildPipelineWithRegistry(
            10.0,
            0.0,
            0.0,
            10,
            registryOverride: registry,
            evaluatorOverride: evaluator,
            events: events);

        var result = await setup.Pipeline.RunAsync("CODE_Q");

        Assert.False(result.IsSuccess);
        Assert.Equal("ADAPTER_PROVENANCE_MISMATCH", result.ErrorCode);
        Assert.Empty(evaluator.Calls);
        Assert.Equal(0, setup.IncrementalTrainer.CallCount);
        Assert.Equal(0, registry.RegisterCount);
    }

    [Fact]
    public async Task AutonomousLearning_Pipeline_AccuracyFailure_BaselineRegistryNonFatalThrowIsSanitized()
    {
        var registry = new DelegateRegistryAL(
            _ => throw new InvalidOperationException("private registry detail"));
        var evaluator = new TargetAwareAccuracyAL(
            (_, _) => OperationResult<double>.Ok(0.5));
        using var setup = BuildPipelineWithRegistry(
            10.0, 0.0, 0.0, 10,
            registryOverride: registry,
            evaluatorOverride: evaluator);

        var result = await setup.Pipeline.RunAsync("CODE_Q");

        Assert.False(result.IsSuccess);
        Assert.Equal("Baseline adapter lookup failed.", result.ErrorMessage);
        Assert.Equal("ACCURACY_EVALUATION_FAILED", result.ErrorCode);
        Assert.DoesNotContain("private registry detail", result.ErrorMessage);
        Assert.Empty(evaluator.Calls);
        Assert.Equal(0, setup.IncrementalTrainer.CallCount);
        Assert.Equal(0, registry.RegisterCount);
    }

    [Theory]
    [InlineData("cancellation")]
    [InlineData("fatal")]
    [InlineData("aggregate-cancellation")]
    [InlineData("aggregate-fatal")]
    public async Task AutonomousLearning_Pipeline_AccuracyFailure_BaselineRegistryLookupCancellationAndFatalGraphsEscape(
        string exceptionKind)
    {
        var expected = CreateAccuracyException(exceptionKind);
        var registry = new DelegateRegistryAL(_ => throw expected);
        var evaluator = new TargetAwareAccuracyAL(
            (_, _) => OperationResult<double>.Ok(0.5));
        using var setup = BuildPipelineWithRegistry(
            10.0, 0.0, 0.0, 10,
            registryOverride: registry,
            evaluatorOverride: evaluator);

        var actual = await Record.ExceptionAsync(
            () => setup.Pipeline.RunAsync("CODE_Q"));

        Assert.Same(expected, actual);
        Assert.Empty(evaluator.Calls);
        Assert.Equal(0, setup.IncrementalTrainer.CallCount);
        Assert.Equal(0, registry.RegisterCount);
    }

    [Fact]
    public async Task AutonomousLearning_Pipeline_AccuracyFailure_BaselineEvaluatorFailPropagatesExactlyAndStops()
    {
        var registry = new ScriptedRegistryAL(
            OperationResult<bool>.Ok(true),
            OperationResult<ModelAdapter?>.Ok(null));
        var evaluator = new TargetAwareAccuracyAL(
            (_, _) => OperationResult<double>.Fail(
                "baseline evaluator unavailable",
                "REFERENCE_SET_UNAVAILABLE"));
        using var setup = BuildPipelineWithRegistry(
            10.0, 0.0, 0.0, 10,
            registryOverride: registry,
            evaluatorOverride: evaluator);

        var result = await setup.Pipeline.RunAsync("CODE_Q");

        Assert.False(result.IsSuccess);
        Assert.Equal("baseline evaluator unavailable", result.ErrorMessage);
        Assert.Equal("REFERENCE_SET_UNAVAILABLE", result.ErrorCode);
        Assert.Single(evaluator.Calls);
        Assert.Equal(0, setup.IncrementalTrainer.CallCount);
        Assert.Equal(0, registry.RegisterCount);
    }

    [Fact]
    public async Task AutonomousLearning_Pipeline_AccuracyFailure_BaselineEvaluatorNonFatalThrowIsSanitized()
    {
        var registry = new ScriptedRegistryAL(
            OperationResult<bool>.Ok(true),
            OperationResult<ModelAdapter?>.Ok(null));
        var evaluator = new TargetAwareAccuracyAL(
            (_, _) => throw new IOException("private evaluator detail"));
        using var setup = BuildPipelineWithRegistry(
            10.0, 0.0, 0.0, 10,
            registryOverride: registry,
            evaluatorOverride: evaluator);

        var result = await setup.Pipeline.RunAsync("CODE_Q");

        Assert.False(result.IsSuccess);
        Assert.Equal("Baseline accuracy evaluation failed.", result.ErrorMessage);
        Assert.Equal("ACCURACY_EVALUATION_FAILED", result.ErrorCode);
        Assert.DoesNotContain("private evaluator detail", result.ErrorMessage);
        Assert.Equal(0, setup.IncrementalTrainer.CallCount);
        Assert.Equal(0, registry.RegisterCount);
    }

    [Theory]
    [InlineData("cancellation")]
    [InlineData("fatal")]
    [InlineData("aggregate-cancellation")]
    [InlineData("aggregate-fatal")]
    public async Task AutonomousLearning_Pipeline_AccuracyFailure_BaselineCancellationAndFatalGraphsEscape(
        string exceptionKind)
    {
        var expected = CreateAccuracyException(exceptionKind);
        var registry = new ScriptedRegistryAL(
            OperationResult<bool>.Ok(true),
            OperationResult<ModelAdapter?>.Ok(null));
        var evaluator = new TargetAwareAccuracyAL((_, _) => throw expected);
        using var setup = BuildPipelineWithRegistry(
            10.0, 0.0, 0.0, 10,
            registryOverride: registry,
            evaluatorOverride: evaluator);

        var actual = await Record.ExceptionAsync(
            () => setup.Pipeline.RunAsync("CODE_Q"));

        Assert.Same(expected, actual);
        Assert.Equal(0, setup.IncrementalTrainer.CallCount);
        Assert.Equal(0, registry.RegisterCount);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(-0.001)]
    [InlineData(1.001)]
    public async Task AutonomousLearning_Pipeline_AccuracyFailure_InvalidBaselineStopsBeforeTraining(
        double invalidScore)
    {
        var registry = new ScriptedRegistryAL(
            OperationResult<bool>.Ok(true),
            OperationResult<ModelAdapter?>.Ok(null));
        var evaluator = new TargetAwareAccuracyAL(
            (_, targetAdapter) => OperationResult<double>.Ok(
                targetAdapter is null ? invalidScore : 0.5));
        using var setup = BuildPipelineWithRegistry(
            10.0, 0.0, 0.0, 10,
            registryOverride: registry,
            evaluatorOverride: evaluator);

        var result = await setup.Pipeline.RunAsync("CODE_Q");

        Assert.False(result.IsSuccess);
        Assert.Equal("INVALID_ACCURACY_SCORE", result.ErrorCode);
        Assert.Equal(0, setup.IncrementalTrainer.CallCount);
        Assert.Equal(0, registry.RegisterCount);
    }

    [Theory]
    [InlineData(0.0, 0.5, true)]
    [InlineData(1.0, 1.0, false)]
    public async Task AutonomousLearning_Pipeline_AccuracyTarget_BaselineBoundariesAreValid(
        double baseline,
        double enhanced,
        bool expectSuccess)
    {
        using var setup = BuildPipelineWithRegistry(
            10.0, baseline, enhanced, 10);

        var result = await setup.Pipeline.RunAsync("CODE_Q");

        Assert.Equal(expectSuccess, result.IsSuccess);
        Assert.NotEqual("INVALID_ACCURACY_SCORE", result.ErrorCode);
        Assert.Equal(1, setup.IncrementalTrainer.CallCount);
        if (!expectSuccess)
            Assert.Equal("ACCURACY_NOT_IMPROVED", result.ErrorCode);
    }

    [Fact]
    public async Task AutonomousLearning_Pipeline_AccuracyFailure_EnhancedEvaluatorFailPropagatesExactlyWithoutRegister()
    {
        var registry = new ScriptedRegistryAL(
            OperationResult<bool>.Ok(true),
            OperationResult<ModelAdapter?>.Ok(null));
        var evaluator = new TargetAwareAccuracyAL(
            (_, targetAdapter) => targetAdapter is null
                ? OperationResult<double>.Ok(0.4)
                : OperationResult<double>.Fail(
                    "candidate evaluator unavailable",
                    "CANDIDATE_REFERENCE_UNAVAILABLE"));
        using var setup = BuildPipelineWithRegistry(
            10.0, 0.0, 0.0, 10,
            registryOverride: registry,
            evaluatorOverride: evaluator);

        var result = await setup.Pipeline.RunAsync("CODE_Q");

        Assert.False(result.IsSuccess);
        Assert.Equal("candidate evaluator unavailable", result.ErrorMessage);
        Assert.Equal("CANDIDATE_REFERENCE_UNAVAILABLE", result.ErrorCode);
        Assert.Equal(1, setup.IncrementalTrainer.CallCount);
        Assert.Equal(0, registry.RegisterCount);
    }

    [Fact]
    public async Task AutonomousLearning_Pipeline_AccuracyFailure_EnhancedEvaluatorNonFatalThrowIsSanitized()
    {
        var registry = new ScriptedRegistryAL(
            OperationResult<bool>.Ok(true),
            OperationResult<ModelAdapter?>.Ok(null));
        var evaluator = new TargetAwareAccuracyAL(
            (_, targetAdapter) => targetAdapter is null
                ? OperationResult<double>.Ok(0.4)
                : throw new IOException("private candidate detail"));
        using var setup = BuildPipelineWithRegistry(
            10.0, 0.0, 0.0, 10,
            registryOverride: registry,
            evaluatorOverride: evaluator);

        var result = await setup.Pipeline.RunAsync("CODE_Q");

        Assert.False(result.IsSuccess);
        Assert.Equal("Enhanced accuracy evaluation failed.", result.ErrorMessage);
        Assert.Equal("ACCURACY_EVALUATION_FAILED", result.ErrorCode);
        Assert.DoesNotContain("private candidate detail", result.ErrorMessage);
        Assert.Equal(1, setup.IncrementalTrainer.CallCount);
        Assert.Equal(0, registry.RegisterCount);
    }

    [Theory]
    [InlineData("cancellation")]
    [InlineData("fatal")]
    [InlineData("aggregate-cancellation")]
    [InlineData("aggregate-fatal")]
    public async Task AutonomousLearning_Pipeline_AccuracyFailure_EnhancedCancellationAndFatalGraphsEscape(
        string exceptionKind)
    {
        var expected = CreateAccuracyException(exceptionKind);
        var registry = new ScriptedRegistryAL(
            OperationResult<bool>.Ok(true),
            OperationResult<ModelAdapter?>.Ok(null));
        var evaluator = new TargetAwareAccuracyAL(
            (_, targetAdapter) => targetAdapter is null
                ? OperationResult<double>.Ok(0.4)
                : throw expected);
        using var setup = BuildPipelineWithRegistry(
            10.0, 0.0, 0.0, 10,
            registryOverride: registry,
            evaluatorOverride: evaluator);

        var actual = await Record.ExceptionAsync(
            () => setup.Pipeline.RunAsync("CODE_Q"));

        Assert.Same(expected, actual);
        Assert.Equal(1, setup.IncrementalTrainer.CallCount);
        Assert.Equal(0, registry.RegisterCount);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(-0.001)]
    [InlineData(1.001)]
    public async Task AutonomousLearning_Pipeline_AccuracyFailure_InvalidEnhancedStopsBeforeRegister(
        double invalidScore)
    {
        var registry = new ScriptedRegistryAL(
            OperationResult<bool>.Ok(true),
            OperationResult<ModelAdapter?>.Ok(null));
        var evaluator = new TargetAwareAccuracyAL(
            (_, targetAdapter) => OperationResult<double>.Ok(
                targetAdapter is null ? 0.4 : invalidScore));
        using var setup = BuildPipelineWithRegistry(
            10.0, 0.0, 0.0, 10,
            registryOverride: registry,
            evaluatorOverride: evaluator);

        var result = await setup.Pipeline.RunAsync("CODE_Q");

        Assert.False(result.IsSuccess);
        Assert.Equal("INVALID_ACCURACY_SCORE", result.ErrorCode);
        Assert.Equal(1, setup.IncrementalTrainer.CallCount);
        Assert.Equal(0, registry.RegisterCount);
    }

    [Theory]
    [InlineData(0.0, 0.0, false)]
    [InlineData(0.5, 1.0, true)]
    public async Task AutonomousLearning_Pipeline_AccuracyTarget_EnhancedBoundariesAreValid(
        double baseline,
        double enhanced,
        bool expectSuccess)
    {
        using var setup = BuildPipelineWithRegistry(
            10.0, baseline, enhanced, 10);

        var result = await setup.Pipeline.RunAsync("CODE_Q");

        Assert.Equal(expectSuccess, result.IsSuccess);
        Assert.NotEqual("INVALID_ACCURACY_SCORE", result.ErrorCode);
        Assert.Equal(1, setup.IncrementalTrainer.CallCount);
        if (!expectSuccess)
            Assert.Equal("ACCURACY_NOT_IMPROVED", result.ErrorCode);
    }

    [Theory]
    [InlineData(0.5, 0.5)]
    [InlineData(0.8, 0.7)]
    public async Task AutonomousLearning_Pipeline_AccuracyFailure_ValidEqualityOrRegressionIsNotImproved(
        double baseline,
        double enhanced)
    {
        var registry = new ScriptedRegistryAL(
            OperationResult<bool>.Ok(true),
            OperationResult<ModelAdapter?>.Ok(null));
        using var setup = BuildPipelineWithRegistry(
            10.0, baseline, enhanced, 10,
            registryOverride: registry);

        var result = await setup.Pipeline.RunAsync("CODE_Q");

        Assert.False(result.IsSuccess);
        Assert.Equal("ACCURACY_NOT_IMPROVED", result.ErrorCode);
        Assert.Equal(1, setup.IncrementalTrainer.CallCount);
        Assert.Equal(0, registry.RegisterCount);
    }

    [Fact]
    public async Task AutonomousLearning_Pipeline_CancellationAfterTrainingStopsBeforeCandidateEvaluation()
    {
        using var cancellation = new CancellationTokenSource();
        var registry = new ScriptedRegistryAL(
            OperationResult<bool>.Ok(true),
            OperationResult<ModelAdapter?>.Ok(null));
        var evaluator = new TargetAwareAccuracyAL(
            (_, targetAdapter) => OperationResult<double>.Ok(
                targetAdapter is null ? 0.4 : 0.7));
        using var setup = BuildPipelineWithRegistry(
            10.0, 0.0, 0.0, 10,
            registryOverride: registry,
            evaluatorOverride: evaluator,
            afterTrain: cancellation.Cancel);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => setup.Pipeline.RunAsync("CODE_Q", cancellation.Token));

        Assert.Single(evaluator.Calls);
        Assert.Equal(1, setup.IncrementalTrainer.CallCount);
        Assert.Equal(0, registry.RegisterCount);
    }

    [Fact]
    public async Task AutonomousLearning_Pipeline_CancellationAfterEnhancedScoreStopsBeforeRegister()
    {
        using var cancellation = new CancellationTokenSource();
        var registry = new ScriptedRegistryAL(
            OperationResult<bool>.Ok(true),
            OperationResult<ModelAdapter?>.Ok(null));
        var evaluator = new TargetAwareAccuracyAL((_, targetAdapter) =>
        {
            if (targetAdapter is null)
                return OperationResult<double>.Ok(0.4);

            cancellation.Cancel();
            return OperationResult<double>.Ok(0.7);
        });
        using var setup = BuildPipelineWithRegistry(
            10.0, 0.0, 0.0, 10,
            registryOverride: registry,
            evaluatorOverride: evaluator);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => setup.Pipeline.RunAsync("CODE_Q", cancellation.Token));

        Assert.Equal(2, evaluator.Calls.Count);
        Assert.Equal(1, setup.IncrementalTrainer.CallCount);
        Assert.Equal(0, registry.RegisterCount);
    }

    private static Exception CreateAccuracyException(string kind)
        => kind switch
        {
            "cancellation" => new OperationCanceledException("cancel"),
            "fatal" => new OutOfMemoryException("constructed fatal marker"),
            "aggregate-cancellation" => new AggregateException(
                new InvalidOperationException(
                    "wrapper",
                    new OperationCanceledException("nested cancel"))),
            "aggregate-fatal" => new AggregateException(
                new InvalidOperationException(
                    "wrapper",
                    new OutOfMemoryException("constructed nested fatal marker"))),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };

    // ── Pipeline builder helper ───────────────────────────────────────────────────

    // BuildPipelineWithRegistry — construct AutonomousLearningPipeline with fully stubbed
    // dependencies and an owned, structurally valid adapter-only artifact.
    private static PipelineSetupAL BuildPipelineWithRegistry(
        double cpuPercent,
        double baselineScore,
        double enhancedScore,
        int    exampleCount,
        AdapterArtifactFixtureAL? artifact = null,
        ContinuousLearning.IDomainModelRegistry? registryOverride = null,
        IAccuracyEvaluator? evaluatorOverride = null,
        IList<string>? events = null,
        Action? afterTrain = null)
    {
        artifact          ??= new AdapterArtifactFixtureAL();

        // Stub system monitor: returns the configured CPU% with all other conditions idle.
        var systemMonitor  = new StubSystemMonitorAL(new SystemStatus(
            CpuPercent: cpuPercent, InputIdleMinutes: 20.0,
            IsCharging: true, BatteryPercent: 80.0, DiskFreeGb: 10.0));

        // Stub data provider: returns exampleCount pre-built CODE_Q examples.
        var dataProvider   = new StubDataProviderAL(exampleCount);

        // Stub accuracy evaluator: derive the score from the actual target, never call order.
        var evaluator = evaluatorOverride ?? new TargetAwareAccuracyAL(
            (requestedDomain, targetAdapter) =>
            {
                if (!StringComparer.Ordinal.Equals(requestedDomain, "CODE_Q"))
                {
                    return OperationResult<double>.Fail(
                        "Unexpected evaluation domain.",
                        "UNEXPECTED_EVALUATION_DOMAIN");
                }

                var targetsCandidate = targetAdapter is not null
                    && StringComparer.Ordinal.Equals(
                        targetAdapter.AdapterPath,
                        artifact.AdapterPath);
                return OperationResult<double>.Ok(
                    targetsCandidate ? enhancedScore : baselineScore);
            });

        // Stub IncrementalTrainer: returns an owned real adapter-only artifact.
        var stubTrainer    = new StubIncrementalTrainerAL(
            artifact.AdapterPath,
            events,
            afterTrain,
            (registryOverride as RecordingRegistryAL)
                ?.CurrentAdapter
                ?.Provenance
                ?.TrainingSources);
        var buffer         = new ReplayBuffer(Storage, seed: 42);
        var registry       = registryOverride
            ?? new DomainModelRegistry(
                Storage,
                artifact.RootDirectory);

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

        return new PipelineSetupAL(pipeline, registry, stubTrainer, artifact);
    }

    private sealed class PipelineSetupAL : IDisposable
    {
        private readonly AdapterArtifactFixtureAL _artifact;

        public AutonomousLearningPipeline Pipeline { get; }
        public ContinuousLearning.IDomainModelRegistry Registry { get; }
        public StubIncrementalTrainerAL IncrementalTrainer { get; }

        public PipelineSetupAL(
            AutonomousLearningPipeline pipeline,
            ContinuousLearning.IDomainModelRegistry registry,
            StubIncrementalTrainerAL incrementalTrainer,
            AdapterArtifactFixtureAL artifact)
        {
            Pipeline = pipeline;
            Registry = registry;
            IncrementalTrainer = incrementalTrainer;
            _artifact = artifact;
        }

        public void Dispose() => _artifact.Dispose();
    }

    private sealed class AdapterArtifactFixtureAL : IDisposable
    {
        public string RootDirectory { get; }
        public string AdapterPath { get; }

        public AdapterArtifactFixtureAL(bool structurallyValid = true)
        {
            RootDirectory = Path.Combine(
                Path.GetTempPath(),
                $"autonomous-learning-adapter-{Guid.NewGuid():N}");
            Directory.CreateDirectory(RootDirectory);
            AdapterPath = Path.Combine(RootDirectory, "CODE_Q.lora");

            if (!structurallyValid)
            {
                File.WriteAllBytes(AdapterPath, [1, 2, 3, 4]);
                return;
            }

            var model = new LlmModel(new ModelConfig
            {
                VocabSize = 32,
                ContextLen = 8,
                HiddenDim = 8,
                NumLayers = 1,
                NumHeads = 2,
                NumKvHeads = 1,
                FfnHiddenDim = 16,
            });
            _ = LoraApplicator.Apply(
                model,
                new LoraConfig
                {
                    Rank = 2,
                    Alpha = 4f,
                    Dropout = 0f,
                    TargetModules = ["q_proj", "v_proj"],
                });
            model.SaveAdapterCheckpoint(AdapterPath);
        }

        public void Dispose()
        {
            if (Directory.Exists(RootDirectory))
                Directory.Delete(RootDirectory, recursive: true);
        }
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
            .Select(i => new TrainingExample(
                $"p{i}",
                $"r{i}",
                "CODE_Q",
                1.0,
                DateTimeOffset.UtcNow,
                ProvenanceTestFactory.SourceId($"autonomous/{i}", $"p{i}\nr{i}")))
            .ToList();
        return Task.FromResult(OperationResult<IReadOnlyList<TrainingExample>>.Ok(examples));
    }
}

// TargetAwareAccuracyAL records the exact production target and delegates the score decision.
file sealed class TargetAwareAccuracyAL : IAccuracyEvaluator
{
    private readonly Func<string, ModelAdapter?, OperationResult<double>> _evaluate;
    private static readonly AccuracyEvaluationArtifactIdentity Artifacts = new(
        new string('1', 64),
        new string('2', 64),
        TrainingSourceProvenance.Identify(new ModelConfig
        {
            VocabSize = 256,
            ContextLen = 32,
            HiddenDim = 8,
            NumLayers = 1,
            NumHeads = 2,
            NumKvHeads = 1,
            FfnHiddenDim = 16,
        }).Sha256);

    public List<AccuracyEvaluationRequest> Calls { get; } = [];

    public TargetAwareAccuracyAL(
        Func<string, ModelAdapter?, OperationResult<double>> evaluate)
        => _evaluate = evaluate;

    public OperationResult<AccuracyEvaluationResult> Evaluate(
        AccuracyEvaluationRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Calls.Add(request);
        var result = _evaluate(request.Domain, request.TargetAdapter);
        return result.IsSuccess
            ? OperationResult<AccuracyEvaluationResult>.Ok(
                new AccuracyEvaluationResult(
                    result.Value,
                    request.ExpectedArtifacts ?? Artifacts,
                    request.TargetAdapter?.Provenance?.AdapterSha256,
                    request.PlannedTrainingSources.Sha256))
            : result.MapError<AccuracyEvaluationResult>();
    }
}

file sealed class DelegateAccuracyEvaluatorAL(
    Func<AccuracyEvaluationRequest, OperationResult<AccuracyEvaluationResult>> evaluate)
    : IAccuracyEvaluator
{
    public OperationResult<AccuracyEvaluationResult> Evaluate(
        AccuracyEvaluationRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return evaluate(request);
    }
}

file sealed class RecordingRegistryAL : ContinuousLearning.IDomainModelRegistry
{
    private readonly IList<string> _events;

    public ModelAdapter? CurrentAdapter { get; private set; }
    public ModelAdapter? OfferedAdapter { get; private set; }
    public int GetCount { get; private set; }
    public int RegisterCount { get; private set; }

    public RecordingRegistryAL(
        ModelAdapter? initialAdapter,
        IList<string> events)
        => (CurrentAdapter, _events) = (initialAdapter, events);

    public OperationResult<bool> Register(ModelAdapter adapter)
    {
        _events.Add("Register");
        RegisterCount++;
        OfferedAdapter = adapter;
        CurrentAdapter = adapter with
        {
            AdapterPath = Path.GetFullPath(adapter.AdapterPath),
        };
        return OperationResult<bool>.Ok(true);
    }

    public OperationResult<ModelAdapter?> GetAdapter(string domain)
    {
        _events.Add(GetCount == 0 ? "Get:baseline" : "Get:readback");
        GetCount++;
        return OperationResult<ModelAdapter?>.Ok(CurrentAdapter);
    }

    public OperationResult<IReadOnlyList<ModelAdapter>> ListAll()
        => OperationResult<IReadOnlyList<ModelAdapter>>.Ok(
            CurrentAdapter is null ? [] : [CurrentAdapter]);
}

file sealed class DelegateRegistryAL : ContinuousLearning.IDomainModelRegistry
{
    private readonly Func<string, OperationResult<ModelAdapter?>> _getAdapter;

    public int GetCount { get; private set; }
    public int RegisterCount { get; private set; }

    public DelegateRegistryAL(
        Func<string, OperationResult<ModelAdapter?>> getAdapter)
        => _getAdapter = getAdapter;

    public OperationResult<bool> Register(ModelAdapter adapter)
    {
        RegisterCount++;
        return OperationResult<bool>.Ok(true);
    }

    public OperationResult<ModelAdapter?> GetAdapter(string domain)
    {
        GetCount++;
        return _getAdapter(domain);
    }

    public OperationResult<IReadOnlyList<ModelAdapter>> ListAll()
        => OperationResult<IReadOnlyList<ModelAdapter>>.Ok([]);
}

file sealed class ScriptedRegistryAL : ContinuousLearning.IDomainModelRegistry
{
    private readonly OperationResult<bool> _registrationResult;
    private readonly Queue<OperationResult<ModelAdapter?>> _getResults;

    public int GetCount { get; private set; }
    public int RegisterCount { get; private set; }

    public ScriptedRegistryAL(
        OperationResult<bool> registrationResult,
        params OperationResult<ModelAdapter?>[] getResults)
        => (_registrationResult, _getResults) = (
            registrationResult,
            new Queue<OperationResult<ModelAdapter?>>(getResults));

    public OperationResult<bool> Register(ModelAdapter adapter)
    {
        RegisterCount++;
        return _registrationResult;
    }

    public OperationResult<ModelAdapter?> GetAdapter(string domain)
    {
        GetCount++;
        return _getResults.Count > 0
            ? _getResults.Dequeue()
            : OperationResult<ModelAdapter?>.Fail(
                "Registry lookup sequence exhausted.",
                "REGISTRY_SEQUENCE_EXHAUSTED");
    }

    public OperationResult<IReadOnlyList<ModelAdapter>> ListAll()
        => OperationResult<IReadOnlyList<ModelAdapter>>.Ok([]);
}

file sealed class CancelDuringRegisterRegistryAL
    : ContinuousLearning.IDomainModelRegistry
{
    private readonly CancellationTokenSource _cancellation;

    public int RegisterCount { get; private set; }
    public int GetCount { get; private set; }
    public ModelAdapter? RegisteredAdapter { get; private set; }

    public CancelDuringRegisterRegistryAL(
        CancellationTokenSource cancellation)
        => _cancellation = cancellation;

    public OperationResult<bool> Register(ModelAdapter adapter)
    {
        RegisterCount++;
        RegisteredAdapter = adapter with
        {
            AdapterPath = Path.GetFullPath(adapter.AdapterPath)
        };
        _cancellation.Cancel();
        return OperationResult<bool>.Ok(true);
    }

    public OperationResult<ModelAdapter?> GetAdapter(string domain)
    {
        GetCount++;
        return OperationResult<ModelAdapter?>.Ok(RegisteredAdapter);
    }

    public OperationResult<IReadOnlyList<ModelAdapter>> ListAll()
        => OperationResult<IReadOnlyList<ModelAdapter>>.Ok(
            RegisteredAdapter is null ? [] : [RegisteredAdapter]);
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
sealed class StubIncrementalTrainerAL : IIncrementalTrainer
{
    // _adapterPath — the path this stub will report as the trained adapter location.
    private readonly string _adapterPath;
    private readonly IList<string>? _events;
    private readonly Action? _afterTrain;
    private readonly TrainingSourceManifest? _inheritedSources;
    public CancellationToken LastCancellationToken { get; private set; }
    public int CallCount { get; private set; }

    // Constructor: configure the adapter path returned on success.
    public StubIncrementalTrainerAL(
        string adapterPath,
        IList<string>? events = null,
        Action? afterTrain = null,
        TrainingSourceManifest? inheritedSources = null)
        => (_adapterPath, _events, _afterTrain, _inheritedSources) = (
            adapterPath,
            events,
            afterTrain,
            inheritedSources);

    // TrainAsync — returns the configured adapter path without performing any training.
    public Task<OperationResult<TrainedAdapterArtifact>> TrainAsync(
        string domain,
        IReadOnlyList<TrainingExample> examples,
        IReadOnlyList<DpoPair> dpoPairs,
        CancellationToken cancellationToken = default)
    {
        CallCount++;
        _events?.Add("Train");
        LastCancellationToken = cancellationToken;
        cancellationToken.ThrowIfCancellationRequested();
        _afterTrain?.Invoke();
        var artifact = ProvenanceTestFactory.Artifact(_adapterPath, examples);
        if (_inheritedSources is not null)
        {
            var combined = TrainingSourceProvenance.Combine(
                _inheritedSources,
                artifact.Provenance.TrainingSources).Value!;
            artifact = artifact with
            {
                Provenance = artifact.Provenance with
                {
                    TrainingSources = combined,
                },
            };
        }
        return Task.FromResult(OperationResult<TrainedAdapterArtifact>.Ok(artifact));
    }
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
