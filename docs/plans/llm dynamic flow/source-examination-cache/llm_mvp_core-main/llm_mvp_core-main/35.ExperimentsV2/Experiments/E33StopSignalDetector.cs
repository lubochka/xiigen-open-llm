namespace ExperimentsV2;

// E33: StopSignalDetector — when to stop decomposing.
// Baseline:  keyword rules (4 signals in sequence)
// Enhanced:  K-NN on family tags + hasIter/hasTech features
// Metric:    binary_accuracy (STOP vs CONTINUE)
//            + false_continue_reduction (costliest error: creating agents that shouldn't exist)
public sealed class E33StopSignalDetector
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var data     = StopSignalDetector.GenerateExamples(seed: 42, n: tier * 60);
        var rng      = new Random(42);
        var shuffled = data.OrderBy(_ => rng.Next()).ToList();
        int split    = (int)(shuffled.Count * 0.8);
        var train    = shuffled.Take(split).ToList();
        var val      = shuffled.Skip(split).ToList();

        // E33-check: verify STOP/CONTINUE ratio in val-set is not severely imbalanced
        int stopCount     = val.Count(x => x.Signal > 0);
        int continueCount = val.Count(x => x.Signal == 0);
        double stopRatio  = val.Count > 0 ? stopCount / (double)val.Count : 0;
        Console.WriteLine($"  E33 val-set: STOP={stopCount}, CONTINUE={continueCount}, ratio={stopRatio:P1} (n={val.Count})");
        if (stopRatio < 0.15 || stopRatio > 0.85)
        {
            Console.WriteLine($"  E33 ⚠ Severe class imbalance (ratio={stopRatio:P1}) → applying StratifiedSplit");
            var (st, sv) = StratifiedSplit.Split(data, x => x.Signal > 0 ? 1 : 0, trainRatio: 0.8, seed: 42);
            train = st;
            val   = sv;
            stopCount     = val.Count(x => x.Signal > 0);
            continueCount = val.Count(x => x.Signal == 0);
            stopRatio     = val.Count > 0 ? stopCount / (double)val.Count : 0;
            Console.WriteLine($"  E33 after stratified: STOP={stopCount}, CONTINUE={continueCount}, ratio={stopRatio:P1}");
        }
        else
        {
            Console.WriteLine("  E33 ✓ Class distribution acceptable");
        }

        var trainFeatures = train.Select(x => (
            Tags:    FamilyCollapser.CollapseString(x.Description),
            HasIter: x.HasIter,
            HasTech: x.HasTech,
            Signal:  x.Signal)).ToList();

        int Predict(string desc, bool hasIter, bool hasTech)
        {
            var qTags = FamilyCollapser.CollapseString(desc);
            return trainFeatures
                .OrderBy(t =>
                {
                    double tagDist = TagDist(qTags, t.Tags);
                    double feaDist = (hasIter == t.HasIter ? 0 : 0.5) +
                                     (hasTech == t.HasTech ? 0 : 0.5);
                    return tagDist + feaDist;
                })
                .Take(3)
                .GroupBy(t => t.Signal > 0 ? 1 : 0)
                .OrderByDescending(g => g.Count())
                .First().Key;
        }

        // ── Baseline: rule-based ──────────────────────────────────────────────
        var t0 = Stopwatch();
        int baseOk = val.Count(x =>
        {
            int predicted = StopSignalDetector.PredictByRules(x.Description, x.HasIter, x.HasTech);
            return (predicted > 0) == (x.Signal > 0);
        });
        var stops       = val.Where(x => x.Signal > 0).ToList();
        int baseFalseCont = stops.Count(x =>
            StopSignalDetector.PredictByRules(x.Description, x.HasIter, x.HasTech) == 0);
        double baseAcc  = baseOk / (double)val.Count;
        double baseFCR  = stops.Count > 0 ? baseFalseCont / (double)stops.Count : 0;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: K-NN ────────────────────────────────────────────────────
        var t1 = Stopwatch();
        int enhOk = val.Count(x =>
            Predict(x.Description, x.HasIter, x.HasTech) == (x.Signal > 0 ? 1 : 0));
        int enhFalseCont = stops.Count(x =>
            Predict(x.Description, x.HasIter, x.HasTech) == 0);
        double enhAcc = enhOk / (double)val.Count;
        double enhFCR = stops.Count > 0 ? enhFalseCont / (double)stops.Count : 0;
        double enhancedMs = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E33-STOP-SIGNAL-DETECTOR",
            Tier              = tier,
            BaselinePrimary   = baseAcc,
            EnhancedPrimary   = enhAcc,
            PrimaryMetricName = "binary_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = baseFCR - enhFCR,
            ScalingSignalNote = "false_continue_reduction (fewer unnecessary agents)",
            ShouldScaleUp     = val.Count < 15,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"trainN={train.Count}, stops={stops.Count}, " +
                    $"baseAcc={baseAcc:P1} FCR={baseFCR:P1}, " +
                    $"enhAcc={enhAcc:P1} FCR={enhFCR:P1}, " +
                    $"false_continue_reduction={baseFCR - enhFCR:+.1%}"
        });
    }

    private static double TagDist(List<string> a, List<string> b)
    {
        var sa = new HashSet<string>(a);
        var sb = new HashSet<string>(b);
        int inter = sa.Count(x => sb.Contains(x));
        int union  = sa.Count + sb.Count - inter;
        return union == 0 ? 0 : 1.0 - (double)inter / union;
    }

    private static long   Stopwatch()     => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
