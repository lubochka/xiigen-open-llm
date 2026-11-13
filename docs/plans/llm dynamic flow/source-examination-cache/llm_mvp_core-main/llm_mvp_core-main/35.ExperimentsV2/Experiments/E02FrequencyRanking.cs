using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExperimentsV2;

// ─── E02: Frequency-based method ranking (T9-style) vs alphabetical ───────────

// Hypothesis: when a user types `myList.`, offering methods in frequency order
// (most-used first) beats alphabetical order because popular methods like Add/Count
// are selected far more often than rare ones like Aggregate/AsParallel.
//
// Baseline:  alphabetical ordering → rank of the actual method called.
// Enhanced:  frequency ordering (higher call count = lower rank) → rank of actual method.
// Metric:    Top-1 accuracy (fraction where the predicted method ranks #1).
//
// Scaling signal: confidence_ratio = fraction of (receiver, method) pairs that
//                 have ≥ 10 observations.  Grows with dataset size.
public sealed class E02FrequencyRanking
{
    // Run Tier 1 experiment on one or more C# source code strings.
    // Each string becomes one "document" of observed method invocations.
    public OperationResult<ExperimentV2Result> Run(IReadOnlyList<string> sources, int tier = 1)
    {
        if (sources.Count == 0)
            return OperationResult<ExperimentV2Result>.Fail("No source documents for E02");

        var t0 = Stopwatch();

        // Collect all method invocations: (receiver, methodName).
        var allCalls = new List<(string Receiver, string Method)>();
        foreach (var src in sources)
        {
            var extracted = ExtractMethodCalls(src);
            if (extracted.IsSuccess) allCalls.AddRange(extracted.Value!);
        }

        if (allCalls.Count == 0)
            return OperationResult<ExperimentV2Result>.Fail("No method invocations found in sources");

        // Build frequency table: frequencyTable[receiver][method] = count.
        var freqTable = new Dictionary<string, Dictionary<string, int>>();
        foreach (var (receiver, method) in allCalls)
        {
            if (!freqTable.TryGetValue(receiver, out var methods))
            {
                methods = new Dictionary<string, int>();
                freqTable[receiver] = methods;
            }
            methods[method] = methods.GetValueOrDefault(method, 0) + 1;
        }

        // ── Baseline: alphabetical rank of actual method ─────────────────────────
        // For each invocation, what rank does the actual method have in alphabetical
        // order of all methods ever called on that receiver?
        double baselineMs     = ElapsedMs(t0);
        var    t1             = Stopwatch();
        int    baseCorrect    = 0;  // rank = 1
        int    enhCorrect     = 0;  // rank = 1
        int    total          = allCalls.Count;

        foreach (var (receiver, method) in allCalls)
        {
            if (!freqTable.TryGetValue(receiver, out var methods)) continue;

            // Alphabetical ranking.
            var alpha = methods.Keys.OrderBy(m => m, StringComparer.OrdinalIgnoreCase).ToList();
            if (alpha.Count > 0 && alpha[0] == method) baseCorrect++;

            // Frequency ranking (descending count).
            var freq = methods.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).ToList();
            if (freq.Count > 0 && freq[0] == method) enhCorrect++;
        }

        double baselineTop1 = total > 0 ? baseCorrect / (double)total : 0;
        double enhancedTop1 = total > 0 ? enhCorrect  / (double)total : 0;
        double enhancedMs   = ElapsedMs(t1);

        // Confidence ratio: fraction of unique (receiver, method) pairs with ≥10 observations.
        int totalPairs  = freqTable.Values.Sum(d => d.Count);
        int confPairs   = freqTable.Values.Sum(d => d.Count(kv => kv.Value >= 10));
        double confRatio = totalPairs > 0 ? confPairs / (double)totalPairs : 0;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E02-FREQUENCY-RANKING",
            Tier              = tier,
            BaselinePrimary   = baselineTop1,
            EnhancedPrimary   = enhancedTop1,
            PrimaryMetricName = "top1_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = confRatio,
            ScalingSignalNote = "confidence_ratio (pairs with >=10 obs / total pairs)",
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes             = $"totalCalls={total}, uniqueReceivers={freqTable.Count}, " +
                                $"confRatio={confRatio:P1}",
            // confRatio=0 means no pair has >=10 observations — not enough data, not a bad approach.
            ShouldScaleUp     = confRatio == 0
        });
    }

    // Parse a C# source string and extract all (receiver, methodName) invocation pairs.
    // Uses only syntactic information — no semantic model required.
    public static OperationResult<List<(string Receiver, string Method)>>
        ExtractMethodCalls(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
            return OperationResult<List<(string, string)>>.Fail("Empty source");

        var tree    = CSharpSyntaxTree.ParseText(source);
        var root    = tree.GetRoot();
        var results = new List<(string, string)>();

        // Walk all invocation expressions: foo.Bar(...), list.Add(...), etc.
        foreach (var invoc in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (invoc.Expression is not MemberAccessExpressionSyntax memberAccess) continue;

            // The method name is the right-hand identifier.
            string method = memberAccess.Name.Identifier.Text;

            // The receiver is the expression to the left of the dot.
            // Use the last identifier segment for normalisation
            // (e.g. "myList" from "this.myList", "a" from "a.b.c.Foo()").
            string receiver = ExtractLeafIdentifier(memberAccess.Expression);

            if (!string.IsNullOrEmpty(receiver) && !string.IsNullOrEmpty(method))
                results.Add((receiver, method));
        }

        return OperationResult<List<(string, string)>>.Ok(results);
    }

    // Walk an expression and return the leftmost simple identifier text.
    // "obj.Prop" → "obj", "this.field" → "this", "GetValue()" → "".
    private static string ExtractLeafIdentifier(ExpressionSyntax expr)
    {
        return expr switch
        {
            IdentifierNameSyntax id      => id.Identifier.Text,
            MemberAccessExpressionSyntax m => ExtractLeafIdentifier(m.Expression),
            ThisExpressionSyntax _        => "this",
            _                             => ""
        };
    }

    private static long   Stopwatch()     => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
