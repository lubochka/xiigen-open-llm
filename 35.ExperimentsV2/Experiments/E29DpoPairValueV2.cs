// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E29v2: DpoPairValue — chosen × 2 TrainStep vs chosen × 1 on neural network.
// Baseline:  all pairs trained × 1 (equal weight)
// Enhanced:  chosen (high-diversity) pairs trained × 2 (DPO signal)
// Hypothesis: double gradient step on chosen pairs moves weights more toward
//             high-quality predicate transitions than single-step training.
public sealed class E29DpoPairValueV2
{
    private readonly int _epochs;
    public E29DpoPairValueV2(int epochs = 3) => _epochs = epochs;

    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        const int vocabSize = 22; // LogicExtractor produces predicates 0-21
        var allPreds = new List<int>();
        foreach (var (_, t) in files)
        {
            var r = LogicExtractor.Extract(t);
            if (r.IsSuccess) allPreds.AddRange(r.Value!);
        }
        if (allPreds.Count < 10)
            return OperationResult<ExperimentV2Result>.Fail("Too few predicates");

        var (trainPreds, valPreds) = StratifiedSplit.Split(
            allPreds, p => p, trainRatio: 0.8, seed: 42);
        if (valPreds.Count == 0) valPreds = allPreds.TakeLast(3).ToList();

        // Find "chosen" half: distinct predicate types (higher diversity)
        var distinctByPos = new HashSet<int>();
        var chosenIdx = new HashSet<int>();
        for (int i = 0; i < trainPreds.Count; i++)
        {
            if (distinctByPos.Add(trainPreds[i]))
                chosenIdx.Add(i);
        }

        // ── Baseline: all pairs × 1 TrainStep ─────────────────────────────────
        var t0      = Stopwatch();
        var baseNet = new ProbeNeuralNetwork(vocabSize, 32, 0.02f, 42);
        for (int ep = 0; ep < _epochs; ep++)
            for (int i = 0; i < trainPreds.Count - 1; i++)
                baseNet.TrainStep(trainPreds[i], trainPreds[i + 1]);
        double baseAcc    = baseNet.Evaluate(valPreds);
        int    baseN      = (trainPreds.Count - 1) * _epochs;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: chosen pairs × 2 (DPO signal) ──────────────────────────
        var t1  = Stopwatch();
        var enh = new ProbeNeuralNetwork(vocabSize, 32, 0.02f, 42);
        for (int ep = 0; ep < _epochs; ep++)
            for (int i = 0; i < trainPreds.Count - 1; i++)
            {
                enh.TrainStep(trainPreds[i], trainPreds[i + 1]);
                if (chosenIdx.Contains(i))
                    enh.TrainStep(trainPreds[i], trainPreds[i + 1]); // ×2 for chosen
            }
        double enhAcc     = enh.Evaluate(valPreds);
        int    enhN       = (trainPreds.Count - 1 + chosenIdx.Count) * _epochs;
        double effGain    = baseAcc > 0 && baseN > 0
            ? (enhAcc / enhN - baseAcc / baseN) / (baseAcc / baseN) : 0;
        double enhancedMs = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E29V2-DPO-PAIR-VALUE",
            Tier              = tier,
            BaselinePrimary   = baseAcc,
            EnhancedPrimary   = enhAcc,
            PrimaryMetricName = "top1_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = effGain,
            ScalingSignalNote = "efficiency_gain (accuracy/N_steps improvement)",
            ShouldScaleUp     = valPreds.Count < 10,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"trainPreds={trainPreds.Count}, val={valPreds.Count}, " +
                    $"chosenCount={chosenIdx.Count}, " +
                    $"baseAcc={baseAcc:P1}(N={baseN}), enhAcc={enhAcc:P1}(N={enhN}), " +
                    $"efficiencyGain={effGain:+.1%}"
        });
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
