// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E44: DatasetWeighter — вес × 10 на security примерах.
// Baseline:  равные веса (все × 1)
// Enhanced:  security примеры × 10 (повторить 10 раз в обучении)
// Метрика:   weighted_f1 = 0.7 × security_accuracy + 0.3 × general_accuracy
public sealed class E44DatasetWeighter
{
    private const float SecurityWeight = 10.0f;
    private const float SecurityFocus  = 0.7f;

    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        var rng = new Random(42);

        var generalFiles  = files.Where(f => !IsSecurityFile(f.Text)).ToList();
        var securityFiles = files.Where(f => IsSecurityFile(f.Text)).ToList();

        if (generalFiles.Count < 2 || securityFiles.Count < 1)
            return OperationResult<ExperimentV2Result>.Fail(
                $"Too few files: general={generalFiles.Count}, security={securityFiles.Count}");

        var genPairs = ExtractPairs(generalFiles);
        var secPairs = ExtractPairs(securityFiles);

        if (genPairs.Count < 4 || secPairs.Count < 2)
            return OperationResult<ExperimentV2Result>.Fail("Too few pairs");

        int gSplit   = (int)(genPairs.Count * 0.8);
        int sSplit   = (int)(secPairs.Count * 0.8);
        var genTrain = genPairs.Take(gSplit).ToList();
        var secTrain = secPairs.Take(sSplit).ToList();
        var genVal   = genPairs.Skip(gSplit).ToList();
        var secVal   = secPairs.Skip(sSplit).ToList();

        // ── Baseline: равные веса ─────────────────────────────────────────────
        var t0 = Stopwatch();
        var baseModel    = BuildBigram(genTrain.Concat(secTrain));
        double baseGenAcc = Evaluate(baseModel, genVal);
        double baseSecAcc = Evaluate(baseModel, secVal);
        double baseWF1    = SecurityFocus * baseSecAcc + (1 - SecurityFocus) * baseGenAcc;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: security × 10 ───────────────────────────────────────────
        var t1 = Stopwatch();
        var weightedSec = Enumerable.Range(0, (int)SecurityWeight)
            .SelectMany(_ => secTrain).ToList();
        var enhModel     = BuildBigram(genTrain.Concat(weightedSec).OrderBy(_ => rng.Next()));
        double enhGenAcc = Evaluate(enhModel, genVal);
        double enhSecAcc = Evaluate(enhModel, secVal);
        double enhWF1    = SecurityFocus * enhSecAcc + (1 - SecurityFocus) * enhGenAcc;
        double enhancedMs = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E44-DATASET-WEIGHTER",
            Tier              = tier,
            BaselinePrimary   = baseWF1,
            EnhancedPrimary   = enhWF1,
            PrimaryMetricName = "weighted_f1",
            LowerIsBetter     = false,
            ScalingSignal     = enhSecAcc - baseSecAcc,
            ScalingSignalNote = "security_accuracy_gain",
            ShouldScaleUp     = secVal.Count < 5,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"genPairs={genPairs.Count}, secPairs={secPairs.Count}, weight={SecurityWeight}x, " +
                    $"base: gen={baseGenAcc:P1} sec={baseSecAcc:P1} wF1={baseWF1:F2}, " +
                    $"enh:  gen={enhGenAcc:P1} sec={enhSecAcc:P1} wF1={enhWF1:F2}"
        });
    }

    private static bool IsSecurityFile(string text)
    {
        var lower = text.ToLowerInvariant();
        if (lower.Contains("throw") ||
            (lower.Contains("null") && lower.Contains("return false")))
            return true;
        try { return ActionLabelGenerator.Label(text) == ActionLabelGenerator.ADD_NULL_CHECK; }
        catch { return false; }
    }

    private static List<(int, int)> ExtractPairs(IEnumerable<(string, string)> files)
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

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
