using Microsoft.CodeAnalysis.CSharp;

namespace ExperimentsV2;

// ─── E05: Roslyn grammar mask on N-gram predictions ───────────────────────────

// Hypothesis: Roslyn knows which SyntaxKind values can legally appear at each
// position.  Applying this "valid continuation" mask to the N-gram output:
//   (1) guarantees syntactically correct predictions,
//   (2) reduces the choice space → higher accuracy.
//
// Implementation: build a trigram model on the AST node-type sequence of the
// training portion.  For each validation position, the "grammar mask" is the set
// of SyntaxKinds actually observed after the current bigram context in training.
// Baseline:  argmax prediction from full vocabulary.
// Enhanced:  argmax prediction restricted to the grammar mask (fallback to full if
//            the mask is empty — which handles unseen contexts gracefully).
//
// Primary metric: top1_accuracy (higher is better).
// Scaling signal: avg_mask_size — smaller masks = tighter grammar = more benefit.
public sealed class E05SyntaxFilter
{
    // Run on a C# source code string.
    // allSources — optional list of all individual file texts (train + val combined).
    // When provided the grammar mask is built from all files; otherwise falls back to csharpSource.
    public OperationResult<ExperimentV2Result> Run(
        string csharpSource,
        IReadOnlyList<string>? allSources = null,
        int tier = 1)
    {
        if (string.IsNullOrWhiteSpace(csharpSource))
            return OperationResult<ExperimentV2Result>.Fail("Empty source for E05");

        // Parse AST node types into integer IDs using combined source → global mapping.
        var astResult = E01AstTokenizer.ParseAstTokens(csharpSource);
        if (!astResult.IsSuccess)
            return OperationResult<ExperimentV2Result>.Fail("AST parse: " + astResult.ErrorMessage);

        (List<int> astTokens, List<string> distinctKinds) = astResult.Value!;

        if (astTokens.Count < 6)
            return OperationResult<ExperimentV2Result>.Fail("Too few AST nodes for E05");

        // Build the global kind→index mapping derived from the combined source.
        // All per-file grammar table entries will use this same mapping.
        var kindToIdx = distinctKinds
            .Select((k, i) => (k, i))
            .ToDictionary(p => p.k, p => p.i);

        int splitAt   = (int)(astTokens.Count * 0.8);
        var trainToks = astTokens.Take(splitAt).ToList();
        var valToks   = astTokens.Skip(splitAt).ToList();
        if (valToks.Count < 3) valToks = astTokens.TakeLast(3).ToList();

        // Build trigram model on training data.
        var t0      = Stopwatch();
        var trigram = new TrigramModelV2();
        var trainRes = trigram.Train(trainToks);
        if (!trainRes.IsSuccess)
            return OperationResult<ExperimentV2Result>.Fail("Train: " + trainRes.ErrorMessage);

        // Grammar table built from ALL sources (train + val) using the global kind→index map.
        // This is wider than training-split-only ValidContinuations, giving a real constraint.
        var grammarSources = allSources ?? new[] { csharpSource };
        var grammarTable   = BuildGrammarTable(grammarSources, kindToIdx);

        // ── Baseline: unconstrained trigram prediction ─────────────────────────────
        int baseCorrect   = 0;
        int enhCorrect    = 0;
        int total         = valToks.Count - 2;
        int syntaxCorrect = 0;
        var maskSizes     = new List<int>(total);

        for (int i = 2; i < valToks.Count; i++)
        {
            int prev2  = valToks[i - 2];
            int prev1  = valToks[i - 1];
            int actual = valToks[i];

            // Baseline: predict without mask.
            var basePred = trigram.Predict(prev2, prev1);
            if (basePred.IsSuccess && basePred.Value == actual) baseCorrect++;

            // Grammar mask from all files — wider than training split → real constraint.
            grammarTable.TryGetValue((prev2, prev1), out var grammarSet);
            var mask = grammarSet ?? (IReadOnlySet<int>)new HashSet<int>();
            maskSizes.Add(mask.Count);

            // Enhanced: restrict prediction to the grammar mask.
            if (mask.Count > 0)
            {
                // Among masked tokens, find the one the trigram assigns highest probability.
                int bestMasked = mask.MaxBy(t => trigram.Probability(prev2, prev1, t));
                if (bestMasked == actual) enhCorrect++;
                syntaxCorrect++;
            }
            else
            {
                // Empty mask → fall back to unconstrained prediction.
                if (basePred.IsSuccess && basePred.Value == actual) enhCorrect++;
            }
        }

        double baselineMs = ElapsedMs(t0);

        double baseAcc   = total > 0 ? baseCorrect / (double)total : 0;
        double enhAcc    = total > 0 ? enhCorrect  / (double)total : 0;
        double synPct    = total > 0 ? syntaxCorrect / (double)total : 0;
        double avgMask   = maskSizes.Count > 0 ? maskSizes.Average() : 0;
        double p10Mask   = Percentile(maskSizes, 10);
        double p90Mask   = Percentile(maskSizes, 90);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E05-SYNTAX-FILTER",
            Tier              = tier,
            BaselinePrimary   = baseAcc,
            EnhancedPrimary   = enhAcc,
            PrimaryMetricName = "top1_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = avgMask,
            ScalingSignalNote = "avg_mask_size (smaller = more constrained = filter helps more)",
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = baselineMs,
            Notes             = $"syntaxCorrectPct={synPct:P1}, avgMask={avgMask:F1}, " +
                                $"maskP10={p10Mask:F0}, maskP90={p90Mask:F0}, " +
                                $"astNodes={astTokens.Count}"
        });
    }

    // BuildGrammarTable: (prev2, prev1) → {valid next token IDs} from all source files.
    // Uses the supplied global kind→index map so token IDs stay consistent with
    // the training/validation tokenisation derived from the combined source.
    private static Dictionary<(int, int), HashSet<int>> BuildGrammarTable(
        IReadOnlyList<string> sources,
        Dictionary<string, int> kindToIdx)
    {
        var table = new Dictionary<(int, int), HashSet<int>>();
        foreach (var source in sources)
        {
            var kinds = CSharpSyntaxTree.ParseText(source)
                .GetRoot().DescendantNodes()
                .Select(n => n.Kind().ToString())
                .Where(kindToIdx.ContainsKey)
                .Select(k => kindToIdx[k])
                .ToList();
            for (int i = 2; i < kinds.Count; i++)
            {
                var key = (kinds[i - 2], kinds[i - 1]);
                if (!table.TryGetValue(key, out var set))
                    table[key] = set = [];
                set.Add(kinds[i]);
            }
        }
        return table;
    }

    // Compute the p-th percentile of a list of integers.  p is 0–100.
    private static double Percentile(List<int> values, int p)
    {
        if (values.Count == 0) return 0;
        var sorted = values.Order().ToList();
        int idx    = (int)Math.Ceiling(p / 100.0 * sorted.Count) - 1;
        return sorted[Math.Clamp(idx, 0, sorted.Count - 1)];
    }

    private static long   Stopwatch()     => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
