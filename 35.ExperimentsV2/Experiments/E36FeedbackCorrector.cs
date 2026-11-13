// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E36: FeedbackCorrector — route user correction to the right component.
// Baseline:  always update the most frequent component
// Enhanced:  K-NN on correction tags → route to correct component
// Metric:    routing_accuracy (5 components: E32/E33/E34/E35/E37)
public sealed class E36FeedbackCorrector
{
    public const int DECOMPOSER  = 32;
    public const int STOP_SIGNAL = 33;
    public const int QUERY_MATCH = 34;
    public const int DEPENDENCY  = 35;
    public const int MERGER      = 37;

    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var examples = GenerateFeedbackExamples(seed: 42, n: tier * 40);
        var rng      = new Random(42);
        var shuffled = examples.OrderBy(_ => rng.Next()).ToList();
        int split    = (int)(shuffled.Count * 0.8);
        var train    = shuffled.Take(split).ToList();
        var val      = shuffled.Skip(split).ToList();

        var trainK = train.Select(x => (
            Tags:      FamilyCollapser.CollapseString(x.Correction),
            Component: x.TargetComponent)).ToList();

        // ── Baseline: always predict most frequent component ──────────────────
        var t0 = Stopwatch();
        int mostFreq = train.GroupBy(x => x.TargetComponent)
            .OrderByDescending(g => g.Count()).First().Key;
        int baseOk    = val.Count(x => x.TargetComponent == mostFreq);
        double baseAcc = baseOk / (double)val.Count;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: K-NN on correction tags ────────────────────────────────
        var t1 = Stopwatch();
        int enhOk = val.Count(x =>
        {
            var qTags = FamilyCollapser.CollapseString(x.Correction);
            int predicted = trainK
                .OrderBy(t => TagDist(qTags, t.Tags))
                .Take(3)
                .GroupBy(t => t.Component)
                .OrderByDescending(g => g.Count())
                .First().Key;
            return predicted == x.TargetComponent;
        });
        double enhAcc = enhOk / (double)val.Count;
        double enhancedMs = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E36-FEEDBACK-CORRECTOR",
            Tier              = tier,
            BaselinePrimary   = baseAcc,
            EnhancedPrimary   = enhAcc,
            PrimaryMetricName = "routing_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = (double)val.Select(x => x.TargetComponent).Distinct().Count() / 5.0,
            ScalingSignalNote = "component_coverage (fraction of 5 components in val)",
            ShouldScaleUp     = val.Count < 10,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"trainN={train.Count}, valN={val.Count}, " +
                    $"baseAcc={baseAcc:P1}(freq={mostFreq}), enhAcc={enhAcc:P1}"
        });
    }

    private sealed record FeedbackExample(string Correction, int TargetComponent);

    private static List<FeedbackExample> GenerateFeedbackExamples(int seed, int n)
    {
        var rng = new Random(seed);
        var templates = new FeedbackExample[]
        {
            // → DECOMPOSER (sub-episode count)
            new("No, that's one task not three separate episodes", DECOMPOSER),
            new("These should be combined into a single step", DECOMPOSER),
            new("You split this into too many pieces — it's all one thing", DECOMPOSER),
            new("This needs to be broken down further into sub-steps", DECOMPOSER),
            // → STOP_SIGNAL (stop or continue)
            new("This should have its own dedicated sub-episode", STOP_SIGNAL),
            new("These are just execution steps, not separate episodes", STOP_SIGNAL),
            new("This is too granular — it doesn't need to be a separate step", STOP_SIGNAL),
            // → QUERY_MATCH (which episode was matched)
            new("This is more like the migration problem, not the architecture problem", QUERY_MATCH),
            new("The closest example is the WordPress integration, not the business planning one", QUERY_MATCH),
            new("You matched this to the wrong previous episode", QUERY_MATCH),
            // → DEPENDENCY (execution order)
            new("These two parts can run in parallel, you don't need to wait", DEPENDENCY),
            new("This has to finish before that one can start", DEPENDENCY),
            new("The order matters here — you can't reverse these steps", DEPENDENCY),
            // → MERGER (answer selection)
            new("Use the second answer, the first one missed the point", MERGER),
            new("Combine the two responses — each covers something the other missed", MERGER),
            new("The shorter answer was more accurate, ignore the longer one", MERGER),
        };
        var result = new List<FeedbackExample>();
        while (result.Count < n)
            result.Add(templates[rng.Next(templates.Length)]);
        return result;
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
