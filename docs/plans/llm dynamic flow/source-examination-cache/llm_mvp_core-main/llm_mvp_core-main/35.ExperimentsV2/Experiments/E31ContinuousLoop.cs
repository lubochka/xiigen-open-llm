namespace ExperimentsV2;

// E31: ContinuousLoop — инкрементальное обучение: slope скорости улучшения.
// Baseline:  однократное обучение, измерение точности по раундам
// Enhanced:  3 раунда с recency-весом × 2 на новых данных
// Метрика:   learning_slope (скорость роста точности по раундам)
public sealed class E31ContinuousLoop
{
    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        if (files.Count < 6)
            return OperationResult<ExperimentV2Result>.Fail("Need >= 6 files");

        var allPairs = ExtractAllPairs(files);
        if (allPairs.Count < 15)
            return OperationResult<ExperimentV2Result>.Fail("Too few pairs");

        int valSplit = (int)(allPairs.Count * 0.8);
        var trainAll = allPairs.Take(valSplit).ToList();
        var valPairs = allPairs.Skip(valSplit).ToList();

        // ── Baseline: одинаковые веса по раундам ─────────────────────────────
        var t0 = Stopwatch();
        var baseAccs = new List<double>();
        for (int r = 1; r <= 3; r++)
        {
            var part = trainAll.Take(trainAll.Count * r / 3).ToList();
            baseAccs.Add(Evaluate(BuildBigram(part), valPairs));
        }
        double baseSlope  = ComputeSlope(baseAccs);
        double baseFinal  = baseAccs[^1];
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: новые данные каждого раунда × 2 (recency weight) ────────
        var t1 = Stopwatch();
        var enhAccs = new List<double>();
        for (int r = 1; r <= 3; r++)
        {
            int take   = trainAll.Count * r / 3;
            int prev   = trainAll.Count * (r - 1) / 3;
            var older  = trainAll.Take(prev).ToList();
            var recent = trainAll.Skip(prev).Take(take - prev).ToList();
            var weighted = older.Concat(recent).Concat(recent).ToList();
            enhAccs.Add(Evaluate(BuildBigram(weighted), valPairs));
        }
        double enhSlope   = ComputeSlope(enhAccs);
        double enhFinal   = enhAccs[^1];
        double enhancedMs = ElapsedMs(t1);
        double slopeGain  = baseSlope > 1e-9 ? (enhSlope - baseSlope) / baseSlope : 0;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E31-CONTINUOUS-LOOP",
            Tier              = tier,
            BaselinePrimary   = baseSlope,
            EnhancedPrimary   = enhSlope,
            PrimaryMetricName = "learning_slope",
            LowerIsBetter     = false,
            ScalingSignal     = slopeGain,
            ScalingSignalNote = "slope_gain% (быстрее улучшается — лучше)",
            ShouldScaleUp     = valPairs.Count < 10,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"totalPairs={allPairs.Count}, valPairs={valPairs.Count}, " +
                    $"base: rounds=[{string.Join(",", baseAccs.Select(a => $"{a:P0}"))}] slope={baseSlope:F4}, " +
                    $"enh:  rounds=[{string.Join(",", enhAccs.Select(a => $"{a:P0}"))}] slope={enhSlope:F4}, " +
                    $"slopeGain={slopeGain:+.1%}, finalBase={baseFinal:P1} finalEnh={enhFinal:P1}"
        });
    }

    private static List<(int, int)> ExtractAllPairs(IReadOnlyList<(string, string)> files)
    {
        var preds = new List<int>();
        foreach (var (_, text) in files)
        {
            var r = LogicExtractor.Extract(text);
            if (r.IsSuccess) preds.AddRange(r.Value!);
        }
        return preds.Zip(preds.Skip(1)).Select(p => (p.First, p.Second)).ToList();
    }

    private static Dictionary<int, Dictionary<int, int>> BuildBigram(
        IEnumerable<(int, int)> pairs)
    {
        var m = new Dictionary<int, Dictionary<int, int>>();
        foreach (var (c, n) in pairs)
        {
            if (!m.TryGetValue(c, out var d)) m[c] = d = new();
            d[n] = d.GetValueOrDefault(n, 0) + 1;
        }
        return m;
    }

    private static double Evaluate(Dictionary<int, Dictionary<int, int>> model,
        List<(int, int)> val)
    {
        if (val.Count == 0) return 0;
        return val.Count(p =>
        {
            if (!model.TryGetValue(p.Item1, out var d) || d.Count == 0) return false;
            return d.MaxBy(kv => kv.Value).Key == p.Item2;
        }) / (double)val.Count;
    }

    private static double ComputeSlope(List<double> values)
    {
        if (values.Count < 2) return 0;
        double n = values.Count, sx = 0, sy = 0, sxy = 0, sx2 = 0;
        for (int i = 0; i < values.Count; i++)
        { sx += i; sy += values[i]; sxy += i * values[i]; sx2 += i * i; }
        double d = n * sx2 - sx * sx;
        return d < 1e-9 ? 0 : (n * sxy - sx * sy) / d;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
