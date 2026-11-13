namespace ExperimentsV2;

// E29: DpoPairValue — высококачественные DPO-пары ускоряют обучение.
// Baseline:  обучение на всех файлах с равным весом
// Enhanced:  приоритет файлов с высоким разнообразием предикатов ("chosen" в DPO)
// Метрика:   pred_accuracy на валидационных парах
public sealed class E29DpoPairValue
{
    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        if (files.Count < 6)
            return OperationResult<ExperimentV2Result>.Fail("Need >= 6 files");

        var fileVecs = files
            .Select(f => (File: f, Vec: BuildPredVec(f.Text), Pairs: ExtractPairs(f.Text)))
            .Where(x => x.Pairs.Count > 0)
            .ToList();

        if (fileVecs.Count < 4)
            return OperationResult<ExperimentV2Result>.Fail("Too few files with predicates");

        int split    = (int)(fileVecs.Count * 0.8);
        var train    = fileVecs.Take(split).ToList();
        var valPairs = fileVecs.Skip(split).SelectMany(x => x.Pairs).ToList();

        if (valPairs.Count < 3)
            return OperationResult<ExperimentV2Result>.Fail("Too few validation pairs");

        // DPO pair value = predicate diversity (distinct active dims / 22)
        var sorted       = train.OrderByDescending(x => x.Vec.Count(v => v > 0)).ToList();
        var chosen       = sorted.Take(sorted.Count / 2 + 1).ToList();
        double meanValue = sorted.Average(x => x.Vec.Count(v => v > 0) / 22.0);

        // ── Baseline: все файлы поровну ───────────────────────────────────────
        var t0 = Stopwatch();
        var allPairs  = train.SelectMany(x => x.Pairs).ToList();
        var baseModel = BuildBigram(allPairs);
        double baseAcc    = Evaluate(baseModel, valPairs);
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: chosen × 3 (имитация DPO-взвешивания) ──────────────────
        var t1 = Stopwatch();
        var chosenPairs = chosen.SelectMany(x => x.Pairs).ToList();
        var weighted    = allPairs.Concat(chosenPairs).Concat(chosenPairs).ToList();
        var enhModel    = BuildBigram(weighted);
        double enhAcc     = Evaluate(enhModel, valPairs);
        double enhancedMs = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E29-DPO-PAIR-VALUE",
            Tier              = tier,
            BaselinePrimary   = baseAcc,
            EnhancedPrimary   = enhAcc,
            PrimaryMetricName = "pred_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = meanValue,
            ScalingSignalNote = "mean_pred_diversity (выше = богаче DPO пары)",
            ShouldScaleUp     = valPairs.Count < 10,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"trainFiles={train.Count}, valPairs={valPairs.Count}, " +
                    $"chosenFiles={chosen.Count}, meanValue={meanValue:F2}, " +
                    $"baseAcc={baseAcc:P1}, enhAcc={enhAcc:P1}"
        });
    }

    private static float[] BuildPredVec(string text)
    {
        var vec = new float[22];
        var r = LogicExtractor.Extract(text);
        if (!r.IsSuccess || r.Value!.Count == 0) return vec;
        foreach (var p in r.Value!) if (p >= 0 && p < 22) vec[p]++;
        float total = vec.Sum();
        if (total > 0) for (int i = 0; i < 22; i++) vec[i] /= total;
        return vec;
    }

    private static List<(int, int)> ExtractPairs(string text)
    {
        var r = LogicExtractor.Extract(text);
        if (!r.IsSuccess || r.Value!.Count < 2) return [];
        var preds = r.Value!;
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

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
