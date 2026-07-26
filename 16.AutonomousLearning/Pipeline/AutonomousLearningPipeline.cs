// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AutonomousLearning;

using AgentInfrastructure;
using ContinuousLearning;

// AutonomousLearningPipeline — full 8-step autonomous learning cycle.
// Runs overnight when the machine is idle and coordinates all subcomponents.
//
// Step 1: E41 IdleDetector     → is the machine safe to train on?
// Step 2: before-measurement   → baseline accuracy (real LoRA, not bigram — E28-E31 lesson)
// Step 3: ITrainingDataProvider → collect + label .NET examples (wraps steps 2-4 from plan)
// Step 4: DatasetWeighter      → apply D-09 class-imbalance formula
// Step 5: AutonomousTrainer    → replay mix (E43) + IncrementalTrainer LoRA
// Step 6: after-measurement    → candidate accuracy using the trained adapter
// Step 7: IDomainModelRegistry → promote only if candidate accuracy improves
// Step 8: return CycleResult   → caller asserts enhanced > baseline
//
// E28-E31 hard lesson: accuracy is measured via IAccuracyEvaluator — never a bigram proxy.
public sealed class AutonomousLearningPipeline : BaseService
{
    // _idleDetector — E41 four-condition check; blocks the cycle if machine is active.
    private readonly IdleDetector      _idleDetector;

    // _systemMonitor — provides the SystemStatus snapshot consumed by IdleDetector.
    private readonly ISystemMonitor    _systemMonitor;

    // _dataProvider — collects labeled examples (wraps finder + scorer + preprocessor).
    private readonly ITrainingDataProvider _dataProvider;

    // _weighter — applies D-09 class-imbalance formula to the collected examples.
    private readonly DatasetWeighter   _weighter;

    // _trainer — AutonomousTrainer wrapping IncrementalTrainer + ReplayBuffer (E43).
    private readonly AutonomousTrainer _trainer;

    // _registry — stores and retrieves domain LoRA adapters after training.
    private readonly ContinuousLearning.IDomainModelRegistry _registry;

    // _evaluator — measures before/after accuracy; must use real LoRA scoring (E28-E31).
    private readonly IAccuracyEvaluator _evaluator;

    // Constructor: all dependencies injected — fabric-first pattern (Rule 2).
    public AutonomousLearningPipeline(
        ISessionStorage        storage,
        IdleDetector           idleDetector,
        ISystemMonitor         systemMonitor,
        ITrainingDataProvider  dataProvider,
        DatasetWeighter        weighter,
        AutonomousTrainer      trainer,
        ContinuousLearning.IDomainModelRegistry registry,
        IAccuracyEvaluator     evaluator)
        : base(storage)
    {
        // Store all injected components — none are constructed here.
        _idleDetector  = idleDetector;
        _systemMonitor = systemMonitor;
        _dataProvider  = dataProvider;
        _weighter      = weighter;
        _trainer       = trainer;
        _registry      = registry;
        _evaluator     = evaluator;
    }

    // RunAsync — execute the full autonomous learning cycle for one domain.
    // Returns CycleResult with before/after accuracy and adapter path.
    // Returns Fail when: machine is active (SYSTEM_ACTIVE), collection fails, training fails.
    public async Task<OperationResult<CycleResult>> RunAsync(
        string            domain,
        CancellationToken ct = default)
    {
        // ── Step 1: E41 IdleDetector ──────────────────────────────────────────
        // Sample current machine state and evaluate all four E41 conditions.
        var statusResult = _systemMonitor.Sample();
        if (!statusResult.IsSuccess)
            return OperationResult<CycleResult>.Fail(
                $"System monitor failed: {statusResult.ErrorMessage}", "MONITOR_FAILED");

        var idleResult = _idleDetector.Check(statusResult.Value!);
        if (!idleResult.IsSuccess || idleResult.Value != IdleDetectorResult.SafeToTrain)
            return OperationResult<CycleResult>.Fail(
                $"Machine not idle: {idleResult.Value}. Cycle deferred.", "SYSTEM_ACTIVE");

        // ── Step 2: Measure baseline accuracy (before training) ───────────────
        // E28-E31 rule: use real LoRA adapter scoring — not a bigram proxy.
        // When no adapter exists yet, baseline returns 0.0 (cold start).
        var baselineResult = _evaluator.Evaluate(domain);
        var baseline = baselineResult.IsSuccess ? baselineResult.Value! : 0.0;

        // ── Step 3: Collect labeled training examples ─────────────────────────
        // ITrainingDataProvider wraps: DataSourceFinder → TrustScorer → DatasetPreprocessor.
        // E20 gate is applied inside DatasetPreprocessor — StackDetector runs first.
        var collectResult = await _dataProvider.CollectAsync(domain, ct);
        if (!collectResult.IsSuccess)
            return OperationResult<CycleResult>.Fail(
                $"Data collection failed: {collectResult.ErrorMessage}", "COLLECTION_FAILED");

        var rawExamples = collectResult.Value!;

        // ── Step 4: Apply D-09 class-imbalance weighting ──────────────────────
        // numClasses = 5 (D-04 canonical domains: CODE_Q, BUG, PLAN, ARCH, REFACTOR).
        // rareExamples = number of examples in the smallest represented class.
        var domainCount  = rawExamples.GroupBy(e => e.Domain).Count();
        var rareCount    = rawExamples.GroupBy(e => e.Domain).Min(g => g.Count());
        IReadOnlyList<TrainingExample> weightedExamples;

        if (rareCount > 0 && domainCount > 0)
        {
            // Safe to apply D-09 formula — at least one class with examples present.
            var weightResult = _weighter.Weight(rawExamples, domainCount, rareCount);
            weightedExamples = weightResult.IsSuccess ? weightResult.Value! : rawExamples;
        }
        else
        {
            // Degenerate case: single class or empty — skip weighting, use uniform weight.
            weightedExamples = rawExamples;
        }

        // ── Step 5: Train with AutonomousTrainer (E43 replay + IncrementalTrainer) ──
        // ReplayBuffer mixes 70% new + 30% old; IncrementalTrainer runs LoRA rank=8, alpha=16.
        var trainResult = await _trainer.TrainAsync(domain, weightedExamples, []);
        if (!trainResult.IsSuccess)
            return OperationResult<CycleResult>.Fail(
                $"Training failed: {trainResult.ErrorMessage}", trainResult.ErrorCode);

        // Step 6: Build the trained adapter candidate.
        var adapter = new ModelAdapter(
            Domain:           domain,
            AdapterPath:      trainResult.Value!,
            TrainingExamples: weightedExamples.Count,
            TrainedAt:        DateTimeOffset.UtcNow);

        // Step 7: Measure enhanced accuracy before promotion.
        // Same evaluator and test set as Step 2; only improved candidates are registered.
        var enhancedResult  = _evaluator.Evaluate(domain);
        var enhanced        = enhancedResult.IsSuccess ? enhancedResult.Value! : baseline;

        if (enhanced <= baseline)
            return OperationResult<CycleResult>.Fail(
                $"Candidate adapter was not promoted because enhanced accuracy {enhanced:0.###} did not exceed baseline {baseline:0.###}.",
                "ACCURACY_NOT_IMPROVED");

        var registerResult = _registry.Register(adapter);
        if (!registerResult.IsSuccess)
            return OperationResult<CycleResult>.Fail(
                $"Adapter registration failed: {registerResult.ErrorMessage}",
                registerResult.ErrorCode);

        // ── Step 8: Return CycleResult for caller assertion ───────────────────
        // Gate B: enhanced must exceed baseline (measurable delta from one cycle).
        return OperationResult<CycleResult>.Ok(new CycleResult(
            BaselineAccuracy:  baseline,
            EnhancedAccuracy:  enhanced,
            AdapterPath:       trainResult.Value!,
            ExamplesProcessed: weightedExamples.Count));
    }
}
