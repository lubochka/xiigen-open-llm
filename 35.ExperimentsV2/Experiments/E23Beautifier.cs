// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using Microsoft.CodeAnalysis.CSharp;

namespace ExperimentsV2;

// E23: Beautifier — K-NN on predicates selects the right code template.
//
// Hypothesis: the predicate sequence of a C# file is sufficient to identify
// which code template it implements, enabling predicate → code generation
// as the final step of the E21→E22→E23 routing pipeline.
//
// Baseline:  random template from the stack's library
// Enhanced:  K-NN (Jaccard on predicate sets) → nearest template
// Primary metric: syntax_correct_pct (Roslyn parses C# templates without errors)
// Scaling signal: template_match_rate (correct template chosen)
public sealed class E23Beautifier
{
    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        var testCases = BuildTestCases(files);
        if (testCases.Count < 1)
            return OperationResult<ExperimentV2Result>.Fail(
                $"No test cases found. Files must contain null-guard or validate patterns.");

        var rng = new Random(42);

        // ── Baseline: random template for the stack ────────────────────────────
        var t0 = Stopwatch();
        int baseSyntaxOk = 0;
        int baseMatchOk  = 0;
        foreach (var tc in testCases)
        {
            var stackTmpl = BeautifierTemplateLibrary.ForStack(tc.Stack);
            if (stackTmpl.Count == 0) continue;
            var rand = stackTmpl[rng.Next(stackTmpl.Count)];
            if (SyntaxCorrect(rand.Code, tc.Stack)) baseSyntaxOk++;
            if (rand.Description == tc.ExpectedDescription) baseMatchOk++;
        }
        double baselineAcc      = baseSyntaxOk / (double)testCases.Count;
        double baselineMatchRate = baseMatchOk / (double)testCases.Count;
        double baselineMs       = ElapsedMs(t0);

        // ── Enhanced: K-NN on predicate sets ──────────────────────────────────
        var t1 = Stopwatch();
        int enhSyntaxOk = 0, matchOk = 0;
        foreach (var tc in testCases)
        {
            var stackTmpl = BeautifierTemplateLibrary.ForStack(tc.Stack);
            if (stackTmpl.Count == 0) continue;

            var best = stackTmpl
                .OrderBy(t => JaccardDist(tc.Predicates, t.Predicates))
                .First();

            if (SyntaxCorrect(best.Code, tc.Stack)) enhSyntaxOk++;
            if (best.Description == tc.ExpectedDescription) matchOk++;
        }
        double enhancedAcc = enhSyntaxOk / (double)testCases.Count;
        double matchRate   = matchOk    / (double)testCases.Count;
        double enhancedMs  = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E23-BEAUTIFIER",
            Tier              = tier,
            BaselinePrimary   = baselineMatchRate,  // random template match rate
            EnhancedPrimary   = matchRate,           // K-NN template match rate
            PrimaryMetricName = "template_match_rate",
            LowerIsBetter     = false,
            ScalingSignal     = enhancedAcc,
            ScalingSignalNote = "syntax_correct_pct (all templates should parse without errors)",
            ShouldScaleUp     = testCases.Count < 8,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"testCases={testCases.Count}, " +
                    $"templates={BeautifierTemplateLibrary.All.Count}, " +
                    $"baseMatch={baselineMatchRate:P1}, enhMatch={matchRate:P1}, " +
                    $"baseSyntax={baselineAcc:P1}, enhSyntax={enhancedAcc:P1}"
        });
    }

    // Predicates that appear in at least one template — used to strip noise predicates
    // from real files before K-NN matching (CALL, BRANCH, DECLARE etc. aren't in any
    // template and would compress Jaccard distances to a narrow range, hiding signal).
    private static readonly HashSet<int> TemplateVocab = new(
        BeautifierTemplateLibrary.All.SelectMany(t => t.Predicates));

    private static List<(int[] Predicates, string Stack, string ExpectedDescription)>
        BuildTestCases(IReadOnlyList<(string Path, string Text)> files)
    {
        var cases = new List<(int[], string, string)>();
        foreach (var (_, text) in files)
        {
            var predOp = LogicExtractor.Extract(text);
            if (!predOp.IsSuccess) continue;

            // Project to template vocab: removes noise (CALL, BRANCH, DECLARE…)
            // so K-NN distances reflect pattern structure, not file verbosity.
            int[] preds = predOp.Value!
                .Where(p => TemplateVocab.Contains(p))
                .Distinct()
                .ToArray();
            if (preds.Length == 0) continue;

            bool hasNullGuard = preds.Contains(LogicExtractor.NULL_GUARD);
            bool hasFail      = preds.Contains(LogicExtractor.FAIL);
            bool hasValidate  = preds.Contains(LogicExtractor.VALIDATE_OP);
            bool hasSuccess   = preds.Contains(LogicExtractor.SUCCESS);  // = RETURN = 11

            // SUCCESS(=RETURN) is present in almost every function.
            // Use the "| Success" template variant when it is detected.
            string? expected =
                hasNullGuard && hasFail && hasSuccess ? "Null guard → Fail | Success"
              : hasNullGuard && hasFail               ? "Null guard → Fail"
              : hasValidate  && hasFail && hasSuccess ? "Validate → Fail | Success"
              : hasValidate  && hasFail               ? "Validate → Fail"
              : null;
            if (expected == null) continue;

            cases.Add((preds, BeautifierTemplateLibrary.CSHARP, expected));
        }
        return cases;
    }

    private static bool SyntaxCorrect(string code, string stack)
    {
        if (stack != BeautifierTemplateLibrary.CSHARP) return true;
        string filled = code
            .Replace("{VAR}",        "x")
            .Replace("{TYPE}",       "object")
            .Replace("{COLLECTION}", "items")
            .Replace("{ITEM}",       "Item")
            .Replace("{MESSAGE}",    "msg")
            .Replace("{RESULT}",     "result")
            .Replace("{FIELD}",      "Value")
            .Replace("{TRANSFORM}",  "transform");
        string wrapped = $"class T {{ void M() {{ {filled} }} }}";
        var tree = CSharpSyntaxTree.ParseText(wrapped);
        return !tree.GetDiagnostics()
            .Any(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
    }

    private static double JaccardDist(int[] a, int[] b)
    {
        var setA  = new HashSet<int>(a);
        var setB  = new HashSet<int>(b);
        int inter = setA.Count(x => setB.Contains(x));
        int union = setA.Count + setB.Count - inter;
        return union == 0 ? 0 : 1.0 - (double)inter / union;
    }

    private static long   Stopwatch() => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
