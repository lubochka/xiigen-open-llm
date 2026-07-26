// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E19: ComplexityRouter — single step or multi-step decomposition needed?
//
// Hypothesis: the number of transitions between different FamilyCollapser tag
// families predicts MULTI_STEP more reliably than simple conjunction counting.
//
// "Find null and log them" has a conjunction but stays in the same op family
// (GET_OP → GET_OP) — only 1 transition, should be SINGLE_STEP.
// "Add auth and write tests" crosses families (CREATE_OP → VALIDATE_OP) —
// multiple transitions → MULTI_STEP.
//
// Baseline:  presence of conjunction words (and/then/also → MULTI_STEP)
// Enhanced:  FamilyCollapser tag transition count > tuned threshold
// Dataset:   synthetic prompts: 50% single, 50% multi + deliberate false-CONJs
// Primary metric: binary_accuracy
// Scaling signal: fpr_reduction (fewer false MULTI_STEP on false CONJs)
public sealed class E19ComplexityRouter
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var examples = GenerateExamples(seed: 42, count: tier * 80);
        int split    = (int)(examples.Count * 0.8);
        var train    = examples.Take(split).ToList();
        var val      = examples.Skip(split).ToList();

        int bestThreshold = ComplexityRouter.TuneThreshold(train);
        var router        = new ComplexityRouter(bestThreshold);

        // ── Baseline: conjunction word detection ──────────────────────────────
        var t0 = Stopwatch();
        int baseCorrect   = val.Count(e => ComplexityRouter.PredictByConj(e.Prompt) == e.Complexity);
        var falseSingles  = val.Where(e => e.Complexity == ComplexityRouter.SINGLE_STEP).ToList();
        int baseFP        = falseSingles.Count(e =>
            ComplexityRouter.PredictByConj(e.Prompt) == ComplexityRouter.MULTI_STEP);
        double baselineAcc = baseCorrect / (double)val.Count;
        double baseFPR     = falseSingles.Count > 0 ? baseFP / (double)falseSingles.Count : 0;
        double baselineMs  = ElapsedMs(t0);

        // ── Enhanced: tag transition count ────────────────────────────────────
        var t1 = Stopwatch();
        int enhCorrect    = val.Count(e => router.Predict(e.Prompt) == e.Complexity);
        int enhFP         = falseSingles.Count(e =>
            router.Predict(e.Prompt) == ComplexityRouter.MULTI_STEP);
        double enhancedAcc = enhCorrect / (double)val.Count;
        double enhFPR      = falseSingles.Count > 0 ? enhFP / (double)falseSingles.Count : 0;
        double enhancedMs  = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E19-COMPLEXITY-ROUTER",
            Tier              = tier,
            BaselinePrimary   = baselineAcc,
            EnhancedPrimary   = enhancedAcc,
            PrimaryMetricName = "binary_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = baseFPR - enhFPR,
            ScalingSignalNote = "fpr_reduction (fewer false MULTI_STEP on false CONJs)",
            ShouldScaleUp     = val.Count < 20,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"examples={examples.Count}, threshold={bestThreshold}, " +
                    $"baseAcc={baselineAcc:P1} FPR={baseFPR:P1}, " +
                    $"enhAcc={enhancedAcc:P1} FPR={enhFPR:P1}, " +
                    $"fpr_delta={baseFPR - enhFPR:+0.0%;-0.0%}"
        });
    }

    private static List<(string Prompt, int Complexity)> GenerateExamples(int seed, int count)
    {
        var rng = new Random(seed);
        var singleTemplates = new[]
        {
            "Add null check to {method} before accessing {prop}",
            "Fix NullReference in {svc} when {obj} is missing",
            "What does {method} return when {obj} is null",
            "Refactor {svc} to use the new {method}",
            "Explain how {svc} handles {obj} validation",
            // False CONJs — one task with a conjunction (same domain on both sides)
            "Find methods that return null and log them",
            "Check null and empty before accessing {prop}",
            "Validate {obj} and return result",
            "Read {obj} and write output",
            "Get {obj} and process it",
            "Find null references and list them",
            "Check the return value and verify type",
            "Get {obj} and check its state",
            "Read the config and parse the value",
            "Validate input and check for nulls",
        };
        var multiTemplates = new[]
        {
            "Add null check to {method} and write unit tests",
            "Fix {svc} then refactor the validation logic",
            "Create {method} in {svc} and update the tests",
            "Refactor {svc} and then plan the new flow structure",
            "Add {method} to {svc} and write integration tests for edge cases",
        };
        var svcs    = new[] { "UserService", "AuthService", "TokenService" };
        var methods = new[] { "GetUser", "ValidateToken", "BuildSearchFilter" };
        var objs    = new[] { "user", "token", "order", "session" };
        var props   = new[] { "Id", "Name", "TenantId" };

        string Fill(string tmpl) => tmpl
            .Replace("{svc}",    svcs[rng.Next(svcs.Length)])
            .Replace("{method}", methods[rng.Next(methods.Length)])
            .Replace("{obj}",    objs[rng.Next(objs.Length)])
            .Replace("{prop}",   props[rng.Next(props.Length)]);

        var result = new List<(string, int)>();
        while (result.Count < count)
        {
            bool isMulti   = rng.Next(2) == 0;
            var  templates = isMulti ? multiTemplates : singleTemplates;
            result.Add((Fill(templates[rng.Next(templates.Length)]),
                isMulti ? ComplexityRouter.MULTI_STEP : ComplexityRouter.SINGLE_STEP));
        }
        return result;
    }

    private static long   Stopwatch() => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
