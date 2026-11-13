namespace ExperimentsV2;

// E46: ConfidenceGate — detect unknown domain from classifier confidence.
// Baseline:  single threshold 0.5 → any max_conf > 0.5 means KNOWN
// Enhanced:  optimal threshold tuned on train to maximise unknown detection
//            while minimising false_known (expensive: system says KNOWN when it's not)
// Metric:    unknown_detection_rate + false_known_rate (costly mistake)
public sealed class E46ConfidenceGate
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var rng      = new Random(42);
        var examples = GenerateConfidenceExamples(rng, n: tier * 60);
        var (train, val) = StratifiedSplit.Split(
            examples, x => x.IsKnown, trainRatio: 0.8, seed: 42);

        double bestThreshold = TuneThreshold(train);
        int unknownCount     = val.Count(x => !x.IsKnown);

        // ── Baseline: fixed threshold 0.5 ─────────────────────────────────────
        var t0 = Stopwatch();
        int baseDetected  = val.Count(x => !x.IsKnown && MaxConf(x.Confidences) < 0.5);
        int baseFalseKnown = val.Count(x => !x.IsKnown && MaxConf(x.Confidences) >= 0.5);
        double baseDetRate = unknownCount > 0 ? baseDetected  / (double)unknownCount : 0;
        double baseFKR     = unknownCount > 0 ? baseFalseKnown / (double)unknownCount : 0;
        double baselineMs  = ElapsedMs(t0);

        // ── Enhanced: tuned threshold ─────────────────────────────────────────
        var t1 = Stopwatch();
        int enhDetected   = val.Count(x => !x.IsKnown && MaxConf(x.Confidences) < bestThreshold);
        int enhFalseKnown = val.Count(x => !x.IsKnown && MaxConf(x.Confidences) >= bestThreshold);
        double enhDetRate  = unknownCount > 0 ? enhDetected  / (double)unknownCount : 0;
        double enhFKR      = unknownCount > 0 ? enhFalseKnown / (double)unknownCount : 0;
        double enhancedMs  = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E46-CONFIDENCE-GATE",
            Tier              = tier,
            BaselinePrimary   = baseDetRate,
            EnhancedPrimary   = enhDetRate,
            PrimaryMetricName = "unknown_detection_rate",
            LowerIsBetter     = false,
            ScalingSignal     = baseFKR - enhFKR,
            ScalingSignalNote = "false_known_reduction (fewer missed unknown domains)",
            ShouldScaleUp     = val.Count < 15,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"val={val.Count}, unknown={unknownCount}, threshold={bestThreshold:F2}, " +
                    $"baseDetRate={baseDetRate:P1}(FKR={baseFKR:P1}), " +
                    $"enhDetRate={enhDetRate:P1}(FKR={enhFKR:P1})"
        });
    }

    private static float MaxConf(float[] confs) => confs.Max();

    private static double TuneThreshold(
        IReadOnlyList<(float[] Confidences, bool IsKnown)> train)
    {
        double best = 0.5, bestF1 = 0;
        for (double t = 0.30; t <= 0.80; t += 0.05)
        {
            int tp = train.Count(x => !x.IsKnown && x.Confidences.Max() < t);
            int fp = train.Count(x =>  x.IsKnown && x.Confidences.Max() < t);
            int fn = train.Count(x => !x.IsKnown && x.Confidences.Max() >= t);
            double prec = tp + fp > 0 ? tp / (double)(tp + fp) : 0;
            double rec  = tp + fn > 0 ? tp / (double)(tp + fn) : 0;
            double f1   = prec + rec > 0 ? 2 * prec * rec / (prec + rec) : 0;
            if (f1 > bestF1) { bestF1 = f1; best = t; }
        }
        return best;
    }

    private static List<(float[] Confidences, bool IsKnown)> GenerateConfidenceExamples(
        Random rng, int n)
    {
        var result = new List<(float[], bool)>();
        while (result.Count < n)
        {
            bool isKnown = rng.Next(2) == 0;
            float[] confs;
            if (isKnown)
            {
                // Known domain: one dominant confidence (0.60-0.90)
                confs = Enumerable.Range(0, 5).Select(_ => (float)(rng.NextDouble() * 0.15)).ToArray();
                confs[rng.Next(5)] = 0.60f + (float)(rng.NextDouble() * 0.30);
                float sum = confs.Sum(); if (sum > 0) for (int i = 0; i < confs.Length; i++) confs[i] /= sum;
            }
            else
            {
                // Unknown domain: flat distribution (0.10-0.30 each)
                confs = Enumerable.Range(0, 5).Select(_ => 0.10f + (float)(rng.NextDouble() * 0.20)).ToArray();
                float sum = confs.Sum(); if (sum > 0) for (int i = 0; i < confs.Length; i++) confs[i] /= sum;
            }
            result.Add((confs, isKnown));
        }
        return result;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
