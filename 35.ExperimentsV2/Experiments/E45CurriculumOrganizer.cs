// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E45: CurriculumOrganizer — порядок обучения (простое сначала, важное последним).
// Baseline:  случайный порядок всех данных
// Enhanced:  easy (70%) → replay (20%) → hard/security (10%, последними)
// Метрика:   final_security_accuracy + learning_slope по раундам
public sealed class E45CurriculumOrganizer
{
    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        if (files.Count < 4)
            return OperationResult<ExperimentV2Result>.Fail("Need >= 4 files");

        var rng       = new Random(42);
        var easyFiles = files.Take((int)(files.Count * 0.7)).ToList();
        var hardFiles = files.Skip((int)(files.Count * 0.7)).ToList();

        var easyPairs = ExtractPairs(easyFiles);
        var hardPairs = ExtractPairs(hardFiles);

        if (easyPairs.Count < 4)
            return OperationResult<ExperimentV2Result>.Fail("Too few pairs");

        int hSplit    = Math.Max(1, (int)(hardPairs.Count * 0.8));
        var hardTrain = hardPairs.Take(hSplit).ToList();
        var hardVal   = hardPairs.Count > hSplit
            ? hardPairs.Skip(hSplit).ToList()
            : hardPairs.TakeLast(1).ToList();

        // ── Baseline: случайный порядок ───────────────────────────────────────
        var t0 = Stopwatch();
        var allPairs = easyPairs.Concat(hardTrain).OrderBy(_ => rng.Next()).ToList();
        var baseAccs = new List<double>();
        for (int r = 1; r <= 3; r++)
        {
            var part = allPairs.Take(allPairs.Count * r / 3).ToList();
            baseAccs.Add(Evaluate(BuildBigram(part), hardVal));
        }
        double baseSlope  = ComputeSlope(baseAccs);
        double baseFinal  = baseAccs[^1];
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: curriculum порядок ──────────────────────────────────────
        var t1 = Stopwatch();
        int replayN    = (int)(hardTrain.Count * 0.2);
        var replayBuf  = easyPairs.OrderBy(_ => rng.Next()).Take(replayN).ToList();
        var curriculum = easyPairs.Concat(replayBuf).Concat(hardTrain).ToList();

        var enhAccs = new List<double>();
        for (int r = 1; r <= 3; r++)
        {
            var part = curriculum.Take(curriculum.Count * r / 3).ToList();
            enhAccs.Add(Evaluate(BuildBigram(part), hardVal));
        }
        double enhSlope   = ComputeSlope(enhAccs);
        double enhFinal   = enhAccs[^1];
        double enhancedMs = ElapsedMs(t1);
        double slopeGain  = baseSlope > 1e-9 ? (enhSlope - baseSlope) / baseSlope : 0;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E45-CURRICULUM-ORGANIZER",
            Tier              = tier,
            BaselinePrimary   = baseFinal,
            EnhancedPrimary   = enhFinal,
            PrimaryMetricName = "final_security_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = slopeGain,
            ScalingSignalNote = "slope_gain (быстрее выходит на хорошую security-точность)",
            ShouldScaleUp     = hardVal.Count < 3,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"easyPairs={easyPairs.Count}, hardPairs={hardPairs.Count}, " +
                    $"base: [{string.Join(",", baseAccs.Select(a => $"{a:P0}"))}] slope={baseSlope:F3}, " +
                    $"enh:  [{string.Join(",", enhAccs.Select(a => $"{a:P0}"))}] slope={enhSlope:F3}, " +
                    $"slopeGain={slopeGain:+.1%}"
        });
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
