// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E37: MergerSynthesizer — select the best of several agent responses.
// Baseline:  longest response (more = more complete?)
// Enhanced:  maximum predicate coverage of the query
// Metric:    quality_selection_accuracy (correctly picking the best of 3)
public sealed class E37MergerSynthesizer
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var testCases = GenerateTestCases(seed: 42, n: tier * 20);
        var rng       = new Random(42);
        var shuffled  = testCases.OrderBy(_ => rng.Next()).ToList();
        int split     = (int)(shuffled.Count * 0.8);
        var val       = shuffled.Skip(split).ToList();

        // ── Baseline: longest response ────────────────────────────────────────
        var t0 = Stopwatch();
        int baseOk = val.Count(tc =>
        {
            int longest = tc.Responses.Select((r, i) => (Length: r.Length, Idx: i))
                .OrderByDescending(x => x.Length).First().Idx;
            return longest == tc.BestIdx;
        });
        double baseAcc = baseOk / (double)val.Count;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: predicate coverage ──────────────────────────────────────
        var t1 = Stopwatch();
        int enhOk = val.Count(tc =>
        {
            var qTags = FamilyCollapser.CollapseString(tc.Query).ToHashSet();
            int best  = tc.Responses
                .Select((resp, idx) =>
                {
                    var rTags    = FamilyCollapser.CollapseString(resp).ToHashSet();
                    double cov   = qTags.Count > 0
                        ? qTags.Count(t => rTags.Contains(t)) / (double)qTags.Count : 0;
                    return (Coverage: cov, Idx: idx);
                })
                .OrderByDescending(x => x.Coverage)
                .First().Idx;
            return best == tc.BestIdx;
        });
        double enhAcc     = enhOk / (double)val.Count;
        double enhancedMs = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E37-MERGER-SYNTHESIZER",
            Tier              = tier,
            BaselinePrimary   = baseAcc,
            EnhancedPrimary   = enhAcc,
            PrimaryMetricName = "quality_selection_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = val.Count(tc =>
                tc.Responses.Max(r => r.Length) != tc.Responses[tc.BestIdx].Length)
                / (double)val.Count,
            ScalingSignalNote = "length_mismatch_rate (best answer != longest answer)",
            ShouldScaleUp     = val.Count < 10,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"testCases={testCases.Count}, val={val.Count}, " +
                    $"baseAcc={baseAcc:P1}, enhAcc={enhAcc:P1}"
        });
    }

    private sealed record MergeTestCase(
        string   Query,
        string[] Responses,
        int      BestIdx);

    private static List<MergeTestCase> GenerateTestCases(int seed, int n)
    {
        var rng = new Random(seed);
        var templates = new MergeTestCase[]
        {
            new("Add null check to ValidateToken before accessing user properties",
                new[] {
                    "You should consider improving the error handling in this method.",
                    "Add if (token == null) return OperationResult.Fail before accessing token.Name.",
                    "The ValidateToken method is in UserService.cs and handles authentication."
                }, BestIdx: 1),

            new("Split the authentication feature into manageable sub-tasks",
                new[] {
                    "The authentication feature involves login, token management, and session handling. These can be separate tasks.",
                    "Authentication is a critical security component that needs careful implementation.",
                    "You will need to create a database table for sessions."
                }, BestIdx: 0),

            new("Which parts of the migration can run at the same time?",
                new[] {
                    "Migration tasks are complex and should be planned carefully before starting.",
                    "The user data migration and the product catalogue migration are independent and can run in parallel.",
                    "You need to migrate the user data, then the product catalogue, then the orders."
                }, BestIdx: 1),

            new("What is the first step when rebuilding a system from scratch?",
                new[] {
                    "Start by understanding what the current system does and what problems it has before designing anything new.",
                    "Rebuilding a system from scratch involves many phases: requirements gathering, architecture design, team planning, technology selection, prototype development, review cycles, and iterative implementation with continuous integration.",
                    "You should use modern frameworks and ensure the new system has proper test coverage."
                }, BestIdx: 0),
        };
        var result = new List<MergeTestCase>();
        while (result.Count < n)
            result.Add(templates[rng.Next(templates.Length)]);
        return result;
    }

    private static long   Stopwatch()     => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
