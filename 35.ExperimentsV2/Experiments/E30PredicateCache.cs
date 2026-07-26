// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E30: PredicateCache — a cache of semantically similar predicate vectors.
// Baseline:  bigram model without a cache
// Enhanced:  cache (cosine similarity of 22-dimensional vectors) + bigram as fallback
// Metric:    pred_accuracy + cache_hit_rate
public sealed class E30PredicateCache
{
    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        if (files.Count < 4)
            return OperationResult<ExperimentV2Result>.Fail("Need >= 4 files");

        int split      = (int)(files.Count * 0.7);
        var trainFiles = files.Take(split).ToList();
        var testFiles  = files.Skip(split).ToList();

        var trainPairs = ExtractPairsWithVecs(trainFiles);
        var testPairs  = ExtractPairsWithVecs(testFiles);

        if (trainPairs.Count < 5 || testPairs.Count < 3)
            return OperationResult<ExperimentV2Result>.Fail("Too few predicate pairs");

        // ── Baseline: pure bigram model ────────────────────────────────
        var t0 = Stopwatch();
        var bigram = BuildBigram(trainPairs.Select(x => (x.Current, x.Next)));
        int baseOk = testPairs.Count(p =>
        {
            if (!bigram.TryGetValue(p.Current, out var d) || d.Count == 0) return false;
            return d.MaxBy(kv => kv.Value).Key == p.Next;
        });
        double baseAcc    = baseOk / (double)testPairs.Count;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: cache + bigram as fallback ──────────────────────────────
        var t1 = Stopwatch();
        var cache = new PredicateCache(threshold: 0.85f);
        foreach (var p in trainPairs) cache.Add(p.Vec, p.Next);

        int hits = 0, cacheOk = 0, enhOk = 0;
        foreach (var p in testPairs)
        {
            var (found, cached) = cache.Lookup(p.Vec);
            int prediction;
            if (found)
            {
                hits++;
                prediction = cached;
                if (cached == p.Next) cacheOk++;
            }
            else
            {
                prediction = bigram.TryGetValue(p.Current, out var d) && d.Count > 0
                    ? d.MaxBy(kv => kv.Value).Key : -1;
            }
            if (prediction == p.Next) enhOk++;
        }
        double enhAcc     = enhOk / (double)testPairs.Count;
        double hitRate    = hits / (double)testPairs.Count;
        double cacheAcc   = hits > 0 ? cacheOk / (double)hits : 0;
        double enhancedMs = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E30-PREDICATE-CACHE",
            Tier              = tier,
            BaselinePrimary   = baseAcc,
            EnhancedPrimary   = enhAcc,
            PrimaryMetricName = "pred_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = hitRate,
            ScalingSignalNote = "cache_hit_rate (higher = more pairs cached)",
            ShouldScaleUp     = testPairs.Count < 10,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"trainPairs={trainPairs.Count}, testPairs={testPairs.Count}, " +
                    $"cacheSize={cache.StoreCount}, hits={hits}({hitRate:P1}), " +
                    $"cacheAcc={cacheAcc:P1}, baseAcc={baseAcc:P1}, enhAcc={enhAcc:P1}"
        });
    }

    private sealed record PairWithVec(float[] Vec, int Current, int Next);

    private static List<PairWithVec> ExtractPairsWithVecs(
        IReadOnlyList<(string Path, string Text)> files)
    {
        var result = new List<PairWithVec>();
        foreach (var (_, text) in files)
        {
            var r = LogicExtractor.Extract(text);
            if (!r.IsSuccess || r.Value!.Count < 2) continue;
            var preds = r.Value!;
            var vec   = PredicateCache.BuildVector(text);
            for (int i = 0; i < preds.Count - 1; i++)
                result.Add(new PairWithVec(vec, preds[i], preds[i + 1]));
        }
        return result;
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

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
