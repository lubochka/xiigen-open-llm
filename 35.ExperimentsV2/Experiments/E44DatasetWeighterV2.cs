// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E44v2: DatasetWeighter — security × 10 TrainStep on neural network.
// Root cause of E44v1 (-0.7%): bigram counts are order-invariant → repeating pairs
// × 10 just increments counters → same prediction as × 1 unless ties change.
// Fix: ProbeNeuralNetwork gradients accumulate 10 times on security pairs →
//      weights genuinely shift toward security predicate patterns.
// Baseline:  all pairs × 1 TrainStep (equal weight)
// Enhanced:  security pairs × 10 TrainStep (10× gradient signal)
public sealed class E44DatasetWeighterV2
{
    private const int   SecurityWeight = 10;
    private const float SecurityFocus  = 0.7f;
    private readonly int _epochs;
    public E44DatasetWeighterV2(int epochs = 3) => _epochs = epochs;

    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        const int vocabSize = 22; // LogicExtractor produces predicates 0-21
        var rng = new Random(42);

        var genFiles = files.Where(f => !IsSecurityFile(f.Text)).ToList();
        var secFiles = files.Where(f => IsSecurityFile(f.Text)).ToList();
        if (genFiles.Count < 2 || secFiles.Count < 1)
            return OperationResult<ExperimentV2Result>.Fail(
                $"Need general and security files: gen={genFiles.Count}, sec={secFiles.Count}");

        var genPreds = ExtractPreds(genFiles);
        var secPreds = ExtractPreds(secFiles);
        if (genPreds.Count < 4 || secPreds.Count < 2)
            return OperationResult<ExperimentV2Result>.Fail("Too few predicates");

        var (genTrain, genVal) = StratifiedSplit.Split(genPreds, p => p, 0.8, 42);
        var (secTrain, secVal) = StratifiedSplit.Split(secPreds, p => p, 0.8, 42);

        // ── Baseline: all × 1 TrainStep ───────────────────────────────────────
        var t0      = Stopwatch();
        var baseNet = new ProbeNeuralNetwork(vocabSize, 32, 0.02f, 42);
        for (int ep = 0; ep < _epochs; ep++)
        {
            var mixed = genTrain.Concat(secTrain).OrderBy(_ => rng.Next()).ToList();
            for (int i = 0; i < mixed.Count - 1; i++)
                baseNet.TrainStep(mixed[i], mixed[i + 1]);
        }
        double baseGen = baseNet.Evaluate(genVal);
        double baseSec = secVal.Count > 1 ? baseNet.Evaluate(secVal) : baseGen;
        double baseWF1 = SecurityFocus * baseSec + (1 - SecurityFocus) * baseGen;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: security × 10 TrainStep ────────────────────────────────
        var t1  = Stopwatch();
        var enh = new ProbeNeuralNetwork(vocabSize, 32, 0.02f, 42);
        for (int ep = 0; ep < _epochs; ep++)
        {
            // General × 1 first
            for (int i = 0; i < genTrain.Count - 1; i++)
                enh.TrainStep(genTrain[i], genTrain[i + 1]);
            // Security × SecurityWeight (gradient applied N times)
            for (int i = 0; i < secTrain.Count - 1; i++)
                for (int w = 0; w < SecurityWeight; w++)
                    enh.TrainStep(secTrain[i], secTrain[i + 1]);
        }
        double enhGen = enh.Evaluate(genVal);
        double enhSec = secVal.Count > 1 ? enh.Evaluate(secVal) : enhGen;
        double enhWF1 = SecurityFocus * enhSec + (1 - SecurityFocus) * enhGen;
        double enhancedMs = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E44V2-DATASET-WEIGHTER",
            Tier              = tier,
            BaselinePrimary   = baseWF1,
            EnhancedPrimary   = enhWF1,
            PrimaryMetricName = "weighted_f1",
            LowerIsBetter     = false,
            ScalingSignal     = enhSec - baseSec,
            ScalingSignalNote = "security_accuracy_gain (primary goal of weighting)",
            ShouldScaleUp     = secVal.Count < 3,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"genPreds={genPreds.Count}, secPreds={secPreds.Count}, weight={SecurityWeight}x, " +
                    $"base: gen={baseGen:P1} sec={baseSec:P1} wF1={baseWF1:F2}, " +
                    $"enh:  gen={enhGen:P1} sec={enhSec:P1} wF1={enhWF1:F2}"
        });
    }

    private static bool IsSecurityFile(string text)
    {
        try { return ActionLabelGenerator.Label(text) == ActionLabelGenerator.ADD_NULL_CHECK; }
        catch { return false; }
    }

    private static List<int> ExtractPreds(IEnumerable<(string, string)> files)
    {
        var r = new List<int>();
        foreach (var (_, t) in files)
        {
            var res = LogicExtractor.Extract(t);
            if (res.IsSuccess) r.AddRange(res.Value!);
        }
        return r;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
