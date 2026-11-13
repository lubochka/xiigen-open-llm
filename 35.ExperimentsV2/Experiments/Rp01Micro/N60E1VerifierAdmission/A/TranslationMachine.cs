// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
// N60-E1 extraction of 27.TranslationDomain/TranslationMachine.cs: constants 14-16, fields 19-21 and
// 27-29, the full TrainStructuredBatchAsync 89-146, helpers 662-684 and 707-712, native construction
// 716-724 and the trainer dictionary 760-770. No ILearningMachine, no _flow, no _runtime, no RunAsync,
// no legacy TrainBatchAsync, no checkpoint import or promotion. Identical in both arms.

namespace N60E1.A;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

// CountingTrainer observes how many Unit lessons were attempted and completed.
// It delegates exactly one real call per lesson and holds no gold data and no admission condition.
public sealed class CountingTrainer(ITranslationUnitTrainer inner) : ITranslationUnitTrainer
{
    private readonly ITranslationUnitTrainer _inner = inner;

    public string UnitId => _inner.UnitId;

    public int Attempted { get; private set; }

    public int Completed { get; private set; }

    public void TrainOnExample(TranslationLearningExample example)
    {
        Attempted++;
        _inner.TrainOnExample(example);
        Completed++;
    }

    public double MeasureAccuracy(IReadOnlyList<TranslationLearningExample> heldOut)
        => _inner.MeasureAccuracy(heldOut);
}

public sealed class TranslationMachine
{
    public const string MachineId = "translation-machine";
    public const string DomainTag = "TranslationDomain";
    private const double LegacyUnverifiedRewardMargin = 0.03d;

    private readonly IReadOnlyDictionary<string, ITranslationUnitTrainer> _unitTrainers;
    private readonly List<LearningBatchResult> _trainingHistory = [];
    private readonly List<TranslationStructuredTrainingIdentityCheckpoint> _structuredTrainingIdentities = [];

    private string _currentVersion = "0.1.0";
    private double _accuracyAfterLastBatch;
    private bool _hasStructuredTraining;

    // Typed references point at the real unit objects, never at the counting wrappers.
    internal SemanticTokenizerUnit SemanticTokenizer { get; }

    internal QualityVerifierUnit Verifier { get; }

    internal TranslationKnnStore KnnStore { get; }

    internal IReadOnlyList<ExecutorUnit> Executors { get; }

    internal IReadOnlyList<CountingTrainer> Counters { get; }

    public TranslationMachine()
    {
        // Native construction, TranslationMachine.cs 716-724.
        var knnStore = new TranslationKnnStore();
        var semanticTokenizer = new SemanticTokenizerUnit();
        var executorRuEn = new ExecutorUnit(TranslationDirection.RU_EN, knnStore);
        var executorEnRu = new ExecutorUnit(TranslationDirection.EN_RU, knnStore);
        var executorHeEn = new ExecutorUnit(TranslationDirection.HE_EN, knnStore);
        var executorEnHe = new ExecutorUnit(TranslationDirection.EN_HE, knnStore);
        var executorEsEn = new ExecutorUnit(TranslationDirection.ES_EN, knnStore);
        var executorEnEs = new ExecutorUnit(TranslationDirection.EN_ES, knnStore);
        var verifier = new QualityVerifierUnit();

        var counters = new List<CountingTrainer>();
        CountingTrainer Wrap(ITranslationUnitTrainer unit)
        {
            var wrapper = new CountingTrainer(unit);
            counters.Add(wrapper);
            return wrapper;
        }

        // Native trainer dictionary, TranslationMachine.cs 760-770, with one counting wrapper per object.
        _unitTrainers = new Dictionary<string, ITranslationUnitTrainer>(StringComparer.Ordinal)
        {
            [semanticTokenizer.UnitId] = Wrap(semanticTokenizer),
            [executorRuEn.UnitId] = Wrap(executorRuEn),
            [executorEnRu.UnitId] = Wrap(executorEnRu),
            [executorHeEn.UnitId] = Wrap(executorHeEn),
            [executorEnHe.UnitId] = Wrap(executorEnHe),
            [executorEsEn.UnitId] = Wrap(executorEsEn),
            [executorEnEs.UnitId] = Wrap(executorEnEs),
            [verifier.UnitId] = Wrap(verifier)
        };

        SemanticTokenizer = semanticTokenizer;
        Verifier = verifier;
        KnnStore = knnStore;
        Executors = [executorRuEn, executorEnRu, executorHeEn, executorEnHe, executorEsEn, executorEnEs];
        Counters = counters;
    }

    // Source-identical copy of TranslationMachine.cs 89-146.
    public Task<OperationResult<LearningBatchResult>> TrainStructuredBatchAsync(
        TranslationLearningBatch batch,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<LearningBatchResult>.Fail("Structured training cancelled.", "CANCELLED"));

        if (batch is null)
            return Task.FromResult(OperationResult<LearningBatchResult>.Fail("Structured training batch is required.", "BATCH_REQUIRED"));

        if (batch.Examples.Count == 0)
            return Task.FromResult(OperationResult<LearningBatchResult>.Fail(
                "Structured training batch must contain at least one example.", "BATCH_EMPTY"));

        var startMs = Environment.TickCount64;
        var accuracyBefore = _accuracyAfterLastBatch;
        var trainedUnits = new HashSet<string>(StringComparer.Ordinal);

        foreach (var example in batch.Examples.Where(example => !example.HeldOut))
        {
            if (!_unitTrainers.TryGetValue(example.UnitId, out var trainer))
                continue;

            trainer.TrainOnExample(example);
            trainedUnits.Add(example.UnitId);
            if (!_structuredTrainingIdentities.Any(identity =>
                    string.Equals(identity.ExampleId, example.ExampleId, StringComparison.Ordinal) &&
                    string.Equals(identity.UnitId, example.UnitId, StringComparison.Ordinal)))
                _structuredTrainingIdentities.Add(new TranslationStructuredTrainingIdentityCheckpoint(
                    example.ExampleId,
                    example.SourceId,
                    example.UnitId,
                    batch.BatchId));
        }

        var heldOut = batch.Examples.Where(example => example.HeldOut).ToArray();
        var accuracyAfter = heldOut.Length > 0
            ? MeasureHeldOutAccuracy(heldOut)
            : MeasureUnitCoverage(trainedUnits);

        _hasStructuredTraining = trainedUnits.Count > 0;
        _accuracyAfterLastBatch = accuracyAfter;
        IncrementPatchVersion();

        var result = new LearningBatchResult(
            BatchId: batch.BatchId,
            RowsTrained: batch.Examples.Count(example => !example.HeldOut),
            AccuracyBefore: accuracyBefore,
            AccuracyAfter: accuracyAfter,
            RewardMargin: 0.0,
            CpuMs: Environment.TickCount64 - startMs,
            PeakRamMb: GC.GetTotalMemory(forceFullCollection: false) / (1024 * 1024),
            ElapsedMs: Environment.TickCount64 - startMs,
            PromotedUnitIds: trainedUnits.Order(StringComparer.Ordinal).ToArray());

        _trainingHistory.Add(result);
        return Task.FromResult(OperationResult<LearningBatchResult>.Ok(result));
    }

    // Source-identical copy of TranslationMachine.cs 662-672.
    private double MeasureHeldOutAccuracy(IReadOnlyList<TranslationLearningExample> heldOut)
    {
        var scored = heldOut
            .GroupBy(example => example.UnitId)
            .Select(group => _unitTrainers.TryGetValue(group.Key, out var trainer)
                ? trainer.MeasureAccuracy(group.ToArray())
                : 0.0)
            .ToArray();

        return scored.Length == 0 ? 0.0 : scored.Average();
    }

    // Source-identical copy of TranslationMachine.cs 674-684.
    private static double MeasureUnitCoverage(IReadOnlySet<string> trainedUnits)
    {
        var required = new[]
        {
            TranslationUnitIds.SemanticTokenizer,
            TranslationUnitIds.ExecutorRuEn,
            TranslationUnitIds.QualityVerifier
        };

        return required.Count(trainedUnits.Contains) / (double)required.Length;
    }

    // Source-identical copy of TranslationMachine.cs 707-712.
    private void IncrementPatchVersion()
    {
        var parts = _currentVersion.Split('.');
        if (parts.Length == 3 && int.TryParse(parts[2], out var patch))
            _currentVersion = $"{parts[0]}.{parts[1]}.{patch + 1}";
    }

    // ExportObservedState returns the native learned state of every unit plus the machine bookkeeping.
    // All copies live in one assembly, so the existing internal ExportCheckpoint methods are reachable
    // without reflection and without a new friend grant.
    internal object ExportObservedState() => new
    {
        learnedState = new
        {
            semanticTokenizer = SemanticTokenizer.ExportCheckpoint(),
            verifier = Verifier.ExportCheckpoint(),
            knnStore = KnnStore.ExportCheckpoint(),
            executors = Executors.Select(executor => new { unitId = executor.UnitId, state = executor.ExportCheckpoint() }).ToArray()
        },
        machine = new
        {
            currentVersion = _currentVersion,
            accuracyAfterLastBatch = _accuracyAfterLastBatch,
            hasStructuredTraining = _hasStructuredTraining,
            structuredTrainingIdentities = _structuredTrainingIdentities,
            trainingHistory = _trainingHistory
        },
        counters = Counters
            .Select(counter => new { unitId = counter.UnitId, attempted = counter.Attempted, completed = counter.Completed })
            .Where(counter => counter.attempted > 0)
            .ToArray(),
        counterTotals = new
        {
            attempted = Counters.Sum(counter => counter.Attempted),
            completed = Counters.Sum(counter => counter.Completed),
            verifierAttempted = Counters.Where(counter => counter.UnitId == TranslationUnitIds.QualityVerifier).Sum(counter => counter.Attempted),
            verifierCompleted = Counters.Where(counter => counter.UnitId == TranslationUnitIds.QualityVerifier).Sum(counter => counter.Completed)
        }
    };
}
