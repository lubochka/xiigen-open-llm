// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AutonomousLearning;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
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
// E28-E31 hard lesson: bind accuracy to the exact requested target — never a proxy.
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

    // _evaluator — target-bound seam. A concrete evaluator exists, but this
    // pipeline still has no non-test composition root.
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
        ct.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(domain)
            || !StringComparer.Ordinal.Equals(domain, domain.Trim()))
        {
            return OperationResult<CycleResult>.Fail(
                "The autonomous-learning domain is invalid.",
                "INVALID_TRAINING_EXAMPLE");
        }
        // ── Step 1: E41 IdleDetector ──────────────────────────────────────────
        // Sample current machine state and evaluate all four E41 conditions.
        var statusResult = _systemMonitor.Sample();
        if (!statusResult.IsSuccess)
            return OperationResult<CycleResult>.Fail(
                $"System monitor failed: {statusResult.ErrorMessage}", "MONITOR_FAILED");

        if (statusResult.Value is null)
            return OperationResult<CycleResult>.Fail(
                "System monitor returned no status.",
                "MONITOR_FAILED");
        var idleResult = _idleDetector.Check(statusResult.Value);
        if (!idleResult.IsSuccess || idleResult.Value != IdleDetectorResult.SafeToTrain)
            return OperationResult<CycleResult>.Fail(
                $"Machine not idle: {idleResult.Value}. Cycle deferred.", "SYSTEM_ACTIVE");

        // ── Step 2: Collect labeled training examples before any measurement ─
        // The exact batch provenance is required by both baseline and candidate
        // evaluation; no catalog-declared training manifest is trusted.
        // ITrainingDataProvider wraps: DataSourceFinder → TrustScorer → DatasetPreprocessor.
        // E20 gate is applied inside DatasetPreprocessor — StackDetector runs first.
        var collectResult = await _dataProvider.CollectAsync(domain, ct);
        if (!collectResult.IsSuccess)
            return OperationResult<CycleResult>.Fail(
                $"Data collection failed: {collectResult.ErrorMessage}", "COLLECTION_FAILED");

        if (collectResult.Value is null || collectResult.Value.Count == 0)
            return OperationResult<CycleResult>.Fail(
                "Data collection returned no training examples.",
                "COLLECTION_FAILED");
        var rawExamples = collectResult.Value;
        if (rawExamples.Any(example =>
                example is null
                || !StringComparer.Ordinal.Equals(example.Domain, domain)))
            return OperationResult<CycleResult>.Fail(
                "Data collection returned an example for a different domain.",
                "INVALID_TRAINING_EXAMPLE");

        // ── Step 3: Apply D-09 class-imbalance weighting ──────────────────────
        // numClasses = 5 (D-04 canonical domains: CODE_Q, BUG, PLAN, ARCH, REFACTOR).
        // rareExamples = number of examples in the smallest represented class.
        var domainCount  = rawExamples.GroupBy(e => e.Domain).Count();
        var rareCount    = rawExamples.GroupBy(e => e.Domain).Min(g => g.Count());
        IReadOnlyList<TrainingExample> weightedExamples;

        if (rareCount > 0 && domainCount > 0)
        {
            // Safe to apply D-09 formula — at least one class with examples present.
            var weightResult = _weighter.Weight(rawExamples, domainCount, rareCount);
            if (!weightResult.IsSuccess || weightResult.Value is null)
                return OperationResult<CycleResult>.Fail(
                    weightResult.ErrorMessage,
                    weightResult.ErrorCode);
            weightedExamples = weightResult.Value;
        }
        else
        {
            // Degenerate case: single class or empty — skip weighting, use uniform weight.
            weightedExamples = rawExamples;
        }

        // ── Step 4: Freeze the exact replay-mixed batch and its source manifest.
        var batchResult = _trainer.PrepareBatch(domain, weightedExamples, [], ct);
        if (!batchResult.IsSuccess || batchResult.Value is null)
            return OperationResult<CycleResult>.Fail(
                batchResult.ErrorMessage,
                batchResult.ErrorCode);
        var trainingBatch = batchResult.Value;

        // ── Step 5: Resolve the current target and planned complete provenance.
        OperationResult<ModelAdapter?> baselineAdapterResult;
        try
        {
            baselineAdapterResult = _registry.GetAdapter(domain);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<CycleResult>.Fail(
                "Baseline adapter lookup failed.",
                "ACCURACY_EVALUATION_FAILED");
        }
        if (!baselineAdapterResult.IsSuccess)
            return OperationResult<CycleResult>.Fail(
                baselineAdapterResult.ErrorMessage,
                baselineAdapterResult.ErrorCode);
        if (baselineAdapterResult.Value is not null
            && baselineAdapterResult.Value.Provenance is null)
            return OperationResult<CycleResult>.Fail(
                "The baseline adapter has no durable provenance.",
                "ADAPTER_PROVENANCE_REQUIRED");
        if (baselineAdapterResult.Value?.Provenance is { } baselineProvenance
            && !IsCompleteProvenance(baselineProvenance))
            return OperationResult<CycleResult>.Fail(
                "The baseline adapter provenance is invalid.",
                "ADAPTER_PROVENANCE_MISMATCH");

        var plannedManifestResult = TrainingSourceProvenance.Combine(
            baselineAdapterResult.Value?.Provenance?.TrainingSources,
            trainingBatch.TrainingSources);
        if (!plannedManifestResult.IsSuccess || plannedManifestResult.Value is null)
            return OperationResult<CycleResult>.Fail(
                plannedManifestResult.ErrorMessage,
                plannedManifestResult.ErrorCode);
        var plannedManifest = plannedManifestResult.Value;

        // ── Step 6: Measure baseline on the exact planned source boundary.
        OperationResult<AccuracyEvaluationResult> baselineResult;
        try
        {
            baselineResult = _evaluator.Evaluate(
                new AccuracyEvaluationRequest(
                    domain,
                    baselineAdapterResult.Value,
                    plannedManifest,
                    AccuracyEvaluationPhase.Baseline,
                    ExpectedArtifacts: null),
                ct);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<CycleResult>.Fail(
                "Baseline accuracy evaluation failed.",
                "ACCURACY_EVALUATION_FAILED");
        }
        if (!baselineResult.IsSuccess || baselineResult.Value is null)
            return OperationResult<CycleResult>.Fail(
                baselineResult.ErrorMessage,
                baselineResult.ErrorCode);
        if (!EvaluationResultMatchesRequest(
                baselineResult.Value,
                plannedManifest,
                baselineAdapterResult.Value,
                expectedArtifacts: null))
            return OperationResult<CycleResult>.Fail(
                "Baseline evaluation result was not bound to the requested artifacts and sources.",
                "ACCURACY_EVALUATION_PROVENANCE_MISMATCH");
        var baseline = baselineResult.Value.Accuracy;
        if (!IsValidAccuracy(baseline))
            return OperationResult<CycleResult>.Fail(
                "Baseline accuracy score must be finite and within [0, 1].",
                "INVALID_ACCURACY_SCORE");

        // ── Step 7: Train the frozen batch.
        ct.ThrowIfCancellationRequested();
        var trainResult = await _trainer.TrainPreparedAsync(trainingBatch, ct);
        if (!trainResult.IsSuccess)
            return OperationResult<CycleResult>.Fail(
                $"Training failed: {trainResult.ErrorMessage}", trainResult.ErrorCode);
        if (trainResult.Value is null
            || string.IsNullOrWhiteSpace(trainResult.Value.AdapterPath)
            || trainResult.Value.Provenance is null
            || !IsCompleteProvenance(trainResult.Value.Provenance))
        {
            return OperationResult<CycleResult>.Fail(
                "Training did not publish a complete adapter checkpoint.",
                "ADAPTER_CHECKPOINT_INCOMPLETE");
        }
        if (!ManifestEquals(
                trainResult.Value.Provenance.TrainingSources,
                plannedManifest))
            return OperationResult<CycleResult>.Fail(
                "Trainer provenance does not match the frozen training batch.",
                "CANDIDATE_TRAINING_MANIFEST_MISMATCH");

        ct.ThrowIfCancellationRequested();

        // Step 8: Build the trained adapter candidate from trainer-owned provenance.
        var adapter = new ModelAdapter(
            Domain:           domain,
            AdapterPath:      trainResult.Value.AdapterPath,
            TrainingExamples: weightedExamples.Count,
            TrainedAt:        DateTimeOffset.UtcNow,
            Provenance:       trainResult.Value.Provenance);

        // Step 7: Measure enhanced accuracy before promotion.
        // The same target-bound seam scores both targets; only improved candidates register.
        OperationResult<AccuracyEvaluationResult> enhancedResult;
        try
        {
            // Score the local candidate before it is visible through the registry.
            enhancedResult = _evaluator.Evaluate(
                new AccuracyEvaluationRequest(
                    domain,
                    adapter,
                    plannedManifest,
                    AccuracyEvaluationPhase.Candidate,
                    baselineResult.Value.Artifacts),
                ct);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<CycleResult>.Fail(
                "Enhanced accuracy evaluation failed.",
                "ACCURACY_EVALUATION_FAILED");
        }

        if (!enhancedResult.IsSuccess || enhancedResult.Value is null)
        {
            return OperationResult<CycleResult>.Fail(
                enhancedResult.ErrorMessage,
                enhancedResult.ErrorCode);
        }

        if (!EvaluationResultMatchesRequest(
                enhancedResult.Value,
                plannedManifest,
                adapter,
                baselineResult.Value.Artifacts))
            return OperationResult<CycleResult>.Fail(
                "Candidate evaluation result was not bound to the requested artifacts and sources.",
                "ACCURACY_EVALUATION_PROVENANCE_MISMATCH");

        var enhanced = enhancedResult.Value.Accuracy;
        if (!IsValidAccuracy(enhanced))
        {
            return OperationResult<CycleResult>.Fail(
                "Enhanced accuracy score must be finite and within [0, 1].",
                "INVALID_ACCURACY_SCORE");
        }

        if (enhanced <= baseline)
            return OperationResult<CycleResult>.Fail(
                $"Candidate adapter was not promoted because enhanced accuracy {enhanced:0.###} did not exceed baseline {baseline:0.###}.",
                "ACCURACY_NOT_IMPROVED");

        ct.ThrowIfCancellationRequested();
        var registerResult = _registry.Register(adapter);
        if (!registerResult.IsSuccess)
            return OperationResult<CycleResult>.Fail(
                $"Adapter registration failed: {registerResult.ErrorMessage}",
                registerResult.ErrorCode);
        if (registerResult.Value is not true)
        {
            return OperationResult<CycleResult>.Fail(
                "Adapter registration did not complete.",
                "ADAPTER_REGISTRATION_INCOMPLETE");
        }

        var readback = _registry.GetAdapter(domain);
        if (!readback.IsSuccess)
        {
            return OperationResult<CycleResult>.Fail(
                $"Adapter registration readback failed: {readback.ErrorMessage}",
                readback.ErrorCode);
        }

        string normalizedCandidatePath;
        try
        {
            normalizedCandidatePath = Path.GetFullPath(adapter.AdapterPath);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<CycleResult>.Fail(
                "Adapter registration candidate path could not be normalized.",
                "ADAPTER_REGISTRATION_INCOMPLETE");
        }

        if (readback.Value is null
            || !StringComparer.Ordinal.Equals(readback.Value.Domain, domain)
            || !StringComparer.Ordinal.Equals(
                readback.Value.AdapterPath,
                normalizedCandidatePath)
            || !ProvenanceEquals(
                readback.Value.Provenance,
                adapter.Provenance))
        {
            return OperationResult<CycleResult>.Fail(
                "Adapter registration readback did not match the candidate.",
                "ADAPTER_REGISTRATION_INCOMPLETE");
        }

        // ── Step 8: Return CycleResult for caller assertion ───────────────────
        // Gate B: enhanced must exceed baseline (measurable delta from one cycle).
        return OperationResult<CycleResult>.Ok(new CycleResult(
            BaselineAccuracy:  baseline,
            EnhancedAccuracy:  enhanced,
            AdapterPath:       readback.Value.AdapterPath,
            ExamplesProcessed: weightedExamples.Count));
    }

    private static bool IsValidAccuracy(double score)
        => double.IsFinite(score) && score >= 0.0 && score <= 1.0;

    private static bool IsCompleteProvenance(ModelAdapterProvenance provenance)
        => provenance.TrainingSources is not null
           && provenance.Model is not null
           && provenance.Lora is not null
           && provenance.Lora.OrderedTargetModules is not null
           && TrainingSourceProvenance.IsCanonicalSha256(provenance.AdapterSha256)
           && TrainingSourceProvenance.IsCanonicalSha256(provenance.BaseCheckpointSha256)
           && TrainingSourceProvenance.IsCanonicalSha256(provenance.TokenizerSha256)
           && TrainingSourceProvenance.IsCanonicalSha256(provenance.Model.Sha256)
           && TrainingSourceProvenance.IsCanonicalSha256(provenance.Lora.Sha256)
           && IsValidManifest(provenance.TrainingSources);

    private static bool IsValidManifest(TrainingSourceManifest manifest)
    {
        if (manifest.SourceIds is null)
            return false;
        var rebuilt = TrainingSourceProvenance.BuildManifest(manifest.SourceIds);
        return rebuilt.IsSuccess
               && rebuilt.Value is not null
               && StringComparer.Ordinal.Equals(rebuilt.Value.Sha256, manifest.Sha256)
               && rebuilt.Value.SourceIds.SequenceEqual(
                   manifest.SourceIds,
                   StringComparer.Ordinal);
    }

    private static bool ManifestEquals(
        TrainingSourceManifest left,
        TrainingSourceManifest right)
    {
        return IsValidManifest(left)
               && IsValidManifest(right)
               && StringComparer.Ordinal.Equals(left.Sha256, right.Sha256)
               && left.SourceIds.SequenceEqual(
                   right.SourceIds,
                   StringComparer.Ordinal);
    }

    private static bool EvaluationResultMatchesRequest(
        AccuracyEvaluationResult result,
        TrainingSourceManifest plannedSources,
        ModelAdapter? target,
        AccuracyEvaluationArtifactIdentity? expectedArtifacts)
        => result.Artifacts is not null
           && StringComparer.Ordinal.Equals(
               result.TrainingSourceManifestSha256,
               plannedSources.Sha256)
           && StringComparer.Ordinal.Equals(
               result.AdapterSha256,
               target?.Provenance?.AdapterSha256)
           && (expectedArtifacts is null || result.Artifacts == expectedArtifacts)
           && (target is null
               || target.Provenance is { } provenance
               && provenance.Model is not null
               && StringComparer.Ordinal.Equals(
                   provenance.BaseCheckpointSha256,
                   result.Artifacts.BaseCheckpointSha256)
               && StringComparer.Ordinal.Equals(
                   provenance.TokenizerSha256,
                   result.Artifacts.TokenizerSha256)
               && StringComparer.Ordinal.Equals(
                   provenance.Model.Sha256,
                   result.Artifacts.ModelConfigurationSha256))
           && TrainingSourceProvenance.IsCanonicalSha256(
               result.Artifacts.BaseCheckpointSha256)
           && TrainingSourceProvenance.IsCanonicalSha256(
               result.Artifacts.TokenizerSha256)
           && TrainingSourceProvenance.IsCanonicalSha256(
               result.Artifacts.ModelConfigurationSha256);

    private static bool ProvenanceEquals(
        ModelAdapterProvenance? left,
        ModelAdapterProvenance? right)
        => left is not null
           && right is not null
           && left.TrainingSources is not null
           && right.TrainingSources is not null
           && left.Model is not null
           && right.Model is not null
           && left.Lora is not null
           && right.Lora is not null
           && left.Lora.OrderedTargetModules is not null
           && right.Lora.OrderedTargetModules is not null
           && StringComparer.Ordinal.Equals(left.AdapterSha256, right.AdapterSha256)
           && StringComparer.Ordinal.Equals(left.BaseCheckpointSha256, right.BaseCheckpointSha256)
           && StringComparer.Ordinal.Equals(left.TokenizerSha256, right.TokenizerSha256)
           && left.Model == right.Model
           && StringComparer.Ordinal.Equals(
               left.TrainingSources.Sha256,
               right.TrainingSources.Sha256)
           && left.TrainingSources.SourceIds.SequenceEqual(
               right.TrainingSources.SourceIds,
               StringComparer.Ordinal)
           && left.Lora.Rank == right.Lora.Rank
           && left.Lora.Alpha.Equals(right.Lora.Alpha)
           && left.Lora.Scaling.Equals(right.Lora.Scaling)
           && left.Lora.Dropout.Equals(right.Lora.Dropout)
           && StringComparer.Ordinal.Equals(left.Lora.Sha256, right.Lora.Sha256)
           && left.Lora.OrderedTargetModules.SequenceEqual(
               right.Lora.OrderedTargetModules,
               StringComparer.Ordinal);
}
