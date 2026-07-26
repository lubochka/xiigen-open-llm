// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E24: AttributedRouter — each dataset gets only its slice of the prompt.
//
// Hypothesis: attributed routing cuts irrelevant text per dataset by 40-60%
// when the prompt contains two independent segments (e.g. code-change + tests).
//
// Baseline:  full prompt → all active datasets (E22 behaviour)
//            Metric: avg fraction of irrelevant text per active dataset
// Enhanced:  attributed segment → only its dataset(s)
//            Metric: same avg noise ratio, should be much lower
//
// Diagnostic signal: segment_precision — fraction of correctly attributed pairs
public sealed class E24AttributedRouter
{
    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        if (files.Count < 4)
            return OperationResult<ExperimentV2Result>.Fail("Need ≥ 4 files for E24");

        // ── Train DatasetRouter from C# files (same as E22) ───────────────────
        var examples = new List<(int[] Predicates, bool[] Datasets)>();
        foreach (var (_, text) in files)
        {
            var predOp = LogicExtractor.Extract(text);
            if (!predOp.IsSuccess || predOp.Value!.Count == 0) continue;
            bool[] label = AgentAlgorithms.PredicateDatasetRouter.AutoLabel(text);
            if (label.Any(b => b))
                examples.Add((predOp.Value!.ToArray(), label));
        }

        if (examples.Count < 4)
            return OperationResult<ExperimentV2Result>.Fail(
                $"Too few labelled examples: {examples.Count}");

        int split = (int)(examples.Count * 0.8);
        var router = new AgentAlgorithms.PredicateDatasetRouter(k: 3);
        router.Train(examples.Take(split).ToList());

        var promptCompiler = new SemanticCompiler29.DevelopmentPromptCompiler();
        var attributedRouter = new SemanticCompilerAdapter.AttributedSegmentRouter();

        var testCases = BuildTestCases();

        // ── Baseline: full prompt → all datasets → avg noise per dataset ──────
        var t0 = Stopwatch();
        double baseNoiseSum = 0;
        int    baseActive   = 0;

        foreach (var tc in testCases)
        {
            int totalLen = tc.FullPrompt.Length;
            foreach (var (_, relevantText) in tc.GroundTruth)
            {
                baseNoiseSum += 1.0 - (double)relevantText.Length / totalLen;
                baseActive++;
            }
        }
        double baselineNoise = baseActive > 0 ? baseNoiseSum / baseActive : 1.0;
        double baselineMs    = ElapsedMs(t0);

        // ── Enhanced: attributed routing ──────────────────────────────────────
        var t1 = Stopwatch();
        double enhNoiseSum    = 0;
        int    enhActive      = 0;
        int    correctAttrib  = 0;
        int    totalAttrib    = 0;

        foreach (var tc in testCases)
        {
            var compileOp = promptCompiler.Compile(tc.FullPrompt);
            if (!compileOp.IsSuccess) continue;
            var routing = attributedRouter.Route(BuildSegmentInputs(compileOp.Value!));

            foreach (var (expectedDataset, expectedText) in tc.GroundTruth)
            {
                totalAttrib++;
                string? actual = ForDataset(routing, expectedDataset);

                if (actual == null)
                {
                    enhNoiseSum += 1.0;
                    enhActive++;
                    continue;
                }

                double noise = 1.0 - (double)expectedText.Length / actual.Length;
                enhNoiseSum += Math.Max(0, noise);
                enhActive++;

                // Correctly attributed if the subprompt contains the first word of the expected text
                string firstWord = expectedText.Split(' ').First();
                if (actual.Contains(firstWord, StringComparison.OrdinalIgnoreCase))
                    correctAttrib++;
            }
        }

        double enhancedNoise = enhActive > 0 ? enhNoiseSum / enhActive : 1.0;
        double segPrecision  = totalAttrib > 0 ? correctAttrib / (double)totalAttrib : 0;
        double enhancedMs    = ElapsedMs(t1);

        double noiseReduction = baselineNoise > 1e-9
            ? (baselineNoise - enhancedNoise) / baselineNoise
            : 0;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E24-ATTRIBUTED-ROUTER",
            Tier              = tier,
            BaselinePrimary   = baselineNoise,
            EnhancedPrimary   = enhancedNoise,
            PrimaryMetricName = "avg_noise_ratio",
            LowerIsBetter     = true,
            ScalingSignal     = segPrecision,
            ScalingSignalNote = "segment_precision (fraction of correctly attributed pairs)",
            ShouldScaleUp     = false,  // hard-coded test cases; scaling is via router training data
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"testCases={testCases.Count}, routerExamples={examples.Count}, " +
                    $"baseNoise={baselineNoise:P1}, enhNoise={enhancedNoise:P1}, " +
                    $"noiseReduction={noiseReduction:P1}, " +
                    $"segPrecision={segPrecision:P1}({correctAttrib}/{totalAttrib})"
        });
    }

    // Hardcoded test cases with known correct attribution.
    private static List<TestCase> BuildTestCases() => new()
    {
        // Two-segment: code change + tests
        new(
            "Add null check to ValidateToken and write unit tests for edge cases",
            new()
            {
                ["CSharp"]  = "Add null check to ValidateToken",
                ["BugFix"]  = "Add null check to ValidateToken",
                ["Tests"]   = "write unit tests for edge cases",
                ["Generic"] = "Add null check to ValidateToken"
            }),

        // Two-segment: refactor + orchestration
        new(
            "Refactor the auth service then create the flow plan for phase A",
            new()
            {
                ["CSharp"]        = "Refactor the auth service",
                ["Orchestration"] = "create the flow plan for phase A",
                ["Generic"]       = "Refactor the auth service"
            }),

        // Single-domain: bug fix
        new(
            "Fix NullReferenceException in UserService when user is not found",
            new()
            {
                ["BugFix"]  = "Fix NullReferenceException in UserService when user is not found",
                ["CSharp"]  = "Fix NullReferenceException in UserService when user is not found",
                ["Generic"] = "Fix NullReferenceException in UserService when user is not found"
            }),

        // False CONJ — must stay one segment
        new(
            "Find methods that return null and log them to the output",
            new()
            {
                ["CSharp"]  = "Find methods that return null and log them to the output",
                ["Generic"] = "Find methods that return null and log them to the output"
            }),

        // Three-domain
        new(
            "Add auth check to UserService and write tests then update the flow plan",
            new()
            {
                ["CSharp"]        = "Add auth check to UserService",
                ["Tests"]         = "write tests",
                ["Orchestration"] = "update the flow plan",
                ["Generic"]       = "Add auth check to UserService"
            }),
    };

    private sealed record TestCase(
        string FullPrompt,
        Dictionary<string, string> GroundTruth);

    private static SemanticCompilerAdapter.AttributedSegmentInput[] BuildSegmentInputs(SemanticCompiler29.DevelopmentPromptCompilation compiled)
    {
        return compiled.Segments.Length <= 1
            ? [new SemanticCompilerAdapter.AttributedSegmentInput("experiment-segment-1", compiled.Text, compiled.DevelopmentPredicateIds, compiled.Domain)]
            : compiled.Segments
                .Select((segment, index) => new SemanticCompilerAdapter.AttributedSegmentInput(
                    $"experiment-segment-{index + 1}",
                    segment.Text,
                    segment.DevelopmentPredicateIds,
                    segment.Domain))
                .ToArray();
    }

    private static string? ForDataset(
        IReadOnlyList<SemanticCompilerAdapter.AttributedSegmentDocument> routing,
        string dataset)
    {
        if (routing.Count == 0)
            return null;

        if (dataset.Equals("Generic", StringComparison.OrdinalIgnoreCase))
            return routing[0].Text;

        return routing.FirstOrDefault(segment => MatchesDataset(segment, dataset))?.Text;
    }

    private static bool MatchesDataset(SemanticCompilerAdapter.AttributedSegmentDocument segment, string dataset)
    {
        var predicates = segment.PredicateIds;
        return dataset switch
        {
            "BugFix" => segment.Domain == SemanticCompiler29.Domain.BUG,
            "Orchestration" => segment.Domain == SemanticCompiler29.Domain.PLAN,
            "Tests" => predicates.Contains("TEST", StringComparer.Ordinal) || predicates.Contains("QA", StringComparer.Ordinal),
            "CSharp" => segment.Domain is SemanticCompiler29.Domain.CODE_Q or SemanticCompiler29.Domain.PLAN or SemanticCompiler29.Domain.REFACTOR,
            _ => false
        };
    }

    private static long   Stopwatch() => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
