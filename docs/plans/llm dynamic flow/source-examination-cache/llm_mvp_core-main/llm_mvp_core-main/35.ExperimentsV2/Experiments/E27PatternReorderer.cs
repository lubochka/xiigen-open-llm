namespace ExperimentsV2;

// E27: PatternReorderer — reorder words from source pattern into SVO.
//
// Hypothesis: a rule table (SOV→SVO, VSO→SVO, TOPIC_OV→SVO, etc.) correctly
// reorders words into English order when the pattern is known.
//
// Baseline:  keep original word order (exact match vs expected)
// Enhanced:  apply transformation table for the source pattern
// Dataset:   7 fixed test cases covering all non-trivial transformations
// Primary metric: exact_match_rate (output matches expected order)
// Scaling signal: structural_accuracy (IsSVO check on enhanced output)
//
// No ML here — pure rules. Accuracy = 100% when pattern is correct.
public sealed class E27PatternReorderer
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var testCases = BuildTestCases();

        // ── Baseline: how many inputs already match the expected output ───────
        var t0 = Stopwatch();
        int baseOk = testCases.Count(tc =>
            tc.InputWords.Select(w => w.Text).SequenceEqual(tc.ExpectedOrder));
        double baselineAcc = baseOk / (double)testCases.Count;
        double baselineMs  = ElapsedMs(t0);

        // ── Enhanced: apply reorder table ────────────────────────────────────
        var t1 = Stopwatch();
        int enhOk  = 0;
        int svoOk  = 0;
        foreach (var tc in testCases)
        {
            var reordered = PatternReorderer.Reorder(tc.InputWords.ToList(), tc.SourcePattern);
            if (reordered.Select(w => w.Text).SequenceEqual(tc.ExpectedOrder)) enhOk++;
            if (PatternReorderer.IsSVO(reordered)) svoOk++;
        }
        double enhancedAcc = enhOk  / (double)testCases.Count;
        double svoAcc      = svoOk  / (double)testCases.Count;
        double enhancedMs  = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E27-PATTERN-REORDERER",
            Tier              = tier,
            BaselinePrimary   = baselineAcc,
            EnhancedPrimary   = enhancedAcc,
            PrimaryMetricName = "exact_match_rate",
            LowerIsBetter     = false,
            ScalingSignal     = svoAcc,
            ScalingSignalNote = "structural_accuracy (IsSVO check on reordered output)",
            ShouldScaleUp     = false, // fixed rule table — more tiers won't help
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"testCases={testCases.Count}, " +
                    $"baseMatch={baselineAcc:P1}, " +
                    $"enhMatch={enhancedAcc:P1}, " +
                    $"svoAcc={svoAcc:P1}"
        });
    }

    private static List<ReorderTestCase> BuildTestCases()
    {
        // TaggedWord(text, posTag): PRON=3, PREP=1, NOUN=7, VERB=8
        return new List<ReorderTestCase>
        {
            // SOV → SVO: "Я check add" → "Я add check"
            new(
                InputWords:    new[] { new TaggedWord("I",     3), new TaggedWord("check",  7), new TaggedWord("add",    8) },
                SourcePattern: SentencePatternClassifier.SOV,
                ExpectedOrder: new[] { "I", "add", "check" }),

            // SOV with prep: "Я null check to method add" → "Я add null check to method"
            new(
                InputWords:    new[] { new TaggedWord("I", 3), new TaggedWord("null", 7), new TaggedWord("check", 7), new TaggedWord("to", 1), new TaggedWord("method", 7), new TaggedWord("add", 8) },
                SourcePattern: SentencePatternClassifier.SOV,
                ExpectedOrder: new[] { "I", "add", "null", "check", "to", "method" }),

            // VSO → SVO: "add I check" → "I add check"
            new(
                InputWords:    new[] { new TaggedWord("add",   8), new TaggedWord("I",     3), new TaggedWord("check",  7) },
                SourcePattern: SentencePatternClassifier.VSO,
                ExpectedOrder: new[] { "I", "add", "check" }),

            // IMPERATIVE_V: unchanged
            new(
                InputWords:    new[] { new TaggedWord("Add", 8), new TaggedWord("null", 7), new TaggedWord("check", 7), new TaggedWord("to", 1), new TaggedWord("method", 7) },
                SourcePattern: SentencePatternClassifier.IMPERATIVE_V,
                ExpectedOrder: new[] { "Add", "null", "check", "to", "method" }),

            // IMPERATIVE_VS → IMPERATIVE_V: remove "you"
            new(
                InputWords:    new[] { new TaggedWord("Add", 8), new TaggedWord("you", 3), new TaggedWord("check", 7), new TaggedWord("to", 1), new TaggedWord("method", 7) },
                SourcePattern: SentencePatternClassifier.IMPERATIVE_VS,
                ExpectedOrder: new[] { "Add", "check", "to", "method" }),

            // TOPIC_OV → SVO: "check add" → "add check"
            new(
                InputWords:    new[] { new TaggedWord("check",  7), new TaggedWord("add",    8) },
                SourcePattern: SentencePatternClassifier.TOPIC_OV,
                ExpectedOrder: new[] { "add", "check" }),

            // SVO: unchanged
            new(
                InputWords:    new[] { new TaggedWord("I",     3), new TaggedWord("add",    8), new TaggedWord("check",  7) },
                SourcePattern: SentencePatternClassifier.SVO,
                ExpectedOrder: new[] { "I", "add", "check" }),
        };
    }

    private sealed record ReorderTestCase(
        TaggedWord[] InputWords,
        int          SourcePattern,
        string[]     ExpectedOrder);

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
