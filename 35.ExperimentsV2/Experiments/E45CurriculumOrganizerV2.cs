// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E45v2: CurriculumOrganizer — TrainStep order matters for neural network.
// Root cause of E45v1 (+0.0%): bigram counts are commutative → curriculum ordering
// of pair insertions produces identical count tables as random ordering.
// Fix: ProbeNeuralNetwork has recency bias — the most recent TrainStep calls
//      have the largest influence on current weights (earlier steps partially overwritten).
// Baseline:  random ordering of TrainStep calls each epoch
// Enhanced:  easy (general) first, hard (security/complex) last each epoch
public sealed class E45CurriculumOrganizerV2
{
    private readonly int _epochs;
    public E45CurriculumOrganizerV2(int epochs = 3) => _epochs = epochs;

    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        const int vocabSize = 22; // LogicExtractor produces predicates 0-21
        var rng = new Random(42);

        var easyFiles = files.Take((int)(files.Count * 0.7)).ToList();
        var hardFiles = files.Skip((int)(files.Count * 0.7)).ToList();
        var easyPreds = ExtractPreds(easyFiles);
        var hardPreds = ExtractPreds(hardFiles);
        if (easyPreds.Count < 4)
            return OperationResult<ExperimentV2Result>.Fail("Too few predicates");

        var hardVal   = hardPreds.TakeLast(Math.Max(1, hardPreds.Count / 4)).ToList();
        var hardTrain = hardPreds.SkipLast(hardVal.Count).ToList();

        // ── Baseline: random ordering each epoch ──────────────────────────────
        var t0      = Stopwatch();
        var baseNet = new ProbeNeuralNetwork(vocabSize, 32, 0.02f, 42);
        var baseAccs = new List<double>();
        for (int ep = 0; ep < _epochs; ep++)
        {
            var mixed = easyPreds.Concat(hardTrain).OrderBy(_ => rng.Next()).ToList();
            for (int i = 0; i < mixed.Count - 1; i++)
                baseNet.TrainStep(mixed[i], mixed[i + 1]);
            baseAccs.Add(hardVal.Count > 1 ? baseNet.Evaluate(hardVal) : 0);
        }
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: easy first, hard last (recency bias) ───────────────────
        var t1     = Stopwatch();
        var enhNet = new ProbeNeuralNetwork(vocabSize, 32, 0.02f, 42);
        var enhAccs = new List<double>();
        for (int ep = 0; ep < _epochs; ep++)
        {
            // Easy predicates first (foundation knowledge)
            for (int i = 0; i < easyPreds.Count - 1; i++)
                enhNet.TrainStep(easyPreds[i], easyPreds[i + 1]);
            // Hard predicates last — recency bias means these weights are freshest
            for (int i = 0; i < hardTrain.Count - 1; i++)
                enhNet.TrainStep(hardTrain[i], hardTrain[i + 1]);
            enhAccs.Add(hardVal.Count > 1 ? enhNet.Evaluate(hardVal) : 0);
        }
        double enhancedMs = ElapsedMs(t1);

        double baseSlope = ComputeSlope(baseAccs);
        double enhSlope  = ComputeSlope(enhAccs);
        double slopeGain = Math.Abs(baseSlope) > 1e-6
            ? (enhSlope - baseSlope) / Math.Abs(baseSlope) : 0;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E45V2-CURRICULUM-ORGANIZER",
            Tier              = tier,
            BaselinePrimary   = baseAccs[^1],
            EnhancedPrimary   = enhAccs[^1],
            PrimaryMetricName = "final_hard_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = slopeGain,
            ScalingSignalNote = "slope_gain (curriculum speeds convergence on hard examples)",
            ShouldScaleUp     = hardVal.Count < 3,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"easyPreds={easyPreds.Count}, hardPreds={hardPreds.Count}, " +
                    $"epochs={_epochs}, hardVal={hardVal.Count}, " +
                    $"baseAccs=[{string.Join(",", baseAccs.Select(a => $"{a:P0}"))}] slope={baseSlope:F4}, " +
                    $"enhAccs=[{string.Join(",", enhAccs.Select(a => $"{a:P0}"))}] slope={enhSlope:F4}, " +
                    $"slopeGain={slopeGain:+.1%}"
        });
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

    private static double ComputeSlope(List<double> v)
    {
        if (v.Count < 2) return 0;
        double n = v.Count, sx = 0, sy = 0, sxy = 0, sx2 = 0;
        for (int i = 0; i < v.Count; i++) { sx += i; sy += v[i]; sxy += i*v[i]; sx2 += i*i; }
        double d = n * sx2 - sx * sx;
        return d < 1e-9 ? 0 : (n * sxy - sx * sy) / d;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
