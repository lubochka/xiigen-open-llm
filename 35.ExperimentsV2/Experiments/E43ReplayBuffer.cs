// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E43: ReplayBuffer — 70/30 mixing prevents catastrophic forgetting.
// Baseline:  train only on new data (100% SECURITY in round 2)
// Enhanced:  70% SECURITY + 30% from the Replay Buffer (CODE_Q from round 1)
// Metric:    retention_accuracy on CODE_Q after training on SECURITY
public sealed class E43ReplayBuffer
{
    private const float ReplayRatio = 0.3f;

    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        if (files.Count < 4)
            return OperationResult<ExperimentV2Result>.Fail("Need >= 4 files");

        var rng       = new Random(42);
        int split     = (int)(files.Count * 0.5);
        var oldDomain = files.Take(split).ToList();
        var newDomain = files.Skip(split).ToList();

        var oldPairs = ExtractPairs(oldDomain);
        var newPairs = ExtractPairs(newDomain);

        if (oldPairs.Count < 5 || newPairs.Count < 5)
            return OperationResult<ExperimentV2Result>.Fail("Too few pairs");

        int valSplit = (int)(oldPairs.Count * 0.8);
        var oldTrain = oldPairs.Take(valSplit).ToList();
        var oldVal   = oldPairs.Skip(valSplit).ToList();

        var baseModel    = BuildBigram(oldTrain);
        double baseAccOnOld = Evaluate(baseModel, oldVal);

        // ── Baseline: new data only (no replay) ────────────────────────
        var t0 = Stopwatch();
        var noReplayModel   = BuildBigram(newPairs);
        double noReplayAcc  = Evaluate(noReplayModel, oldVal);
        double forgetting   = baseAccOnOld - noReplayAcc;
        double baselineMs   = ElapsedMs(t0);

        // ── Enhanced: 70% new + 30% Replay Buffer ───────────────────────────
        var t1 = Stopwatch();
        int replayCount  = (int)(newPairs.Count * ReplayRatio / (1 - ReplayRatio));
        var replayBuffer = oldTrain.OrderBy(_ => rng.Next()).Take(replayCount).ToList();
        var mixedPairs   = newPairs.Concat(replayBuffer).OrderBy(_ => rng.Next()).ToList();
        var replayModel  = BuildBigram(mixedPairs);
        double replayAcc = Evaluate(replayModel, oldVal);
        double retention = baseAccOnOld > 0 ? replayAcc / baseAccOnOld : 0;
        double enhancedMs = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E43-REPLAY-BUFFER",
            Tier              = tier,
            BaselinePrimary   = noReplayAcc,
            EnhancedPrimary   = replayAcc,
            PrimaryMetricName = "retention_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = retention,
            ScalingSignalNote = "retention_rate (1.0 = full retention, < 0.7 = poor)",
            ShouldScaleUp     = oldVal.Count < 10,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"oldPairs={oldPairs.Count}, newPairs={newPairs.Count}, " +
                    $"replay={replayBuffer.Count}, baseOnOld={baseAccOnOld:P1}, " +
                    $"noReplay={noReplayAcc:P1}(forgot={forgetting:+.1%}), " +
                    $"withReplay={replayAcc:P1}(retained={retention:P1})"
        });
    }

    private static List<(int, int)> ExtractPairs(IReadOnlyList<(string, string)> files)
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
