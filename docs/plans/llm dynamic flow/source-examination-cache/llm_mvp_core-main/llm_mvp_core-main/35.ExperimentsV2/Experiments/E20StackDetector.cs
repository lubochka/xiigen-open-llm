namespace ExperimentsV2;

// E20: StackDetector — identify the technology stack from normalized text.
//
// Hypothesis: stack-specific tokens (task/ienumerable for C#, promise/injectable
// for TypeScript, def/yield for Python) survive E15 normalization and are
// sufficient to identify the stack with K-NN on marker frequency vectors.
//
// Key architectural question: if marker_survival_rate > 30% → run E20 AFTER
// E15 normalization (each stack gets its own FamilyCollapser vocabulary).
// If survival < 30% → run E20 BEFORE normalization on raw tokens.
//
// Baseline:  marker voting (count stack-specific tokens per stack, take max)
// Enhanced:  K-NN (cosine on normalized marker frequency vector)
// Dataset:   synthetic code snippets, 4 stacks, generated programmatically
// Primary metric: top1_accuracy (4 classes, random = 25%)
// Scaling signal: marker_survival_rate (% of stack markers surviving E15)
public sealed class E20StackDetector
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        int perStack    = tier * 15;
        var rawExamples = StackDetector.GenerateSyntheticExamples(seed: 42, perStack);

        // Run through E15 normalization
        var examples = rawExamples
            .Select(e => (
                Normalized: string.Join(" ", SemanticNormalizer.Normalize(e.Code)),
                Stack:       e.Stack))
            .ToList();

        // Measure marker survival rate after normalization
        var knownMarkers = new[] { "task", "promise", "def", "injectable", "ienumerable", "yield" };
        int totalMarkers = 0, survivedMarkers = 0;
        foreach (var (norm, stack) in examples.Where(e => e.Stack < 3))
        {
            var words = norm.ToLowerInvariant().Split(' ').ToHashSet();
            totalMarkers    += knownMarkers.Length;
            survivedMarkers += knownMarkers.Count(m => words.Contains(m));
        }
        double survivalRate = totalMarkers > 0 ? survivedMarkers / (double)totalMarkers : 0;

        int split = (int)(examples.Count * 0.8);
        var train = examples.Take(split).ToList();
        var val   = examples.Skip(split).ToList();

        var detector = new StackDetector(k: 3);
        detector.Train(train);

        // ── Baseline: marker voting ────────────────────────────────────────────
        var t0 = Stopwatch();
        int baseCorrect    = val.Count(e => StackDetector.PredictByMarkers(e.Normalized) == e.Stack);
        double baselineAcc  = baseCorrect / (double)val.Count;
        double baselineMs   = ElapsedMs(t0);

        // ── Enhanced: K-NN ────────────────────────────────────────────────────
        var t1 = Stopwatch();
        int enhCorrect    = val.Count(e => detector.Predict(e.Normalized) == e.Stack);
        double enhancedAcc = enhCorrect / (double)val.Count;
        double enhancedMs  = ElapsedMs(t1);

        string perStackNotes = string.Join(", ", Enumerable.Range(0, 4).Select(s =>
        {
            var sv = val.Where(e => e.Stack == s).ToList();
            if (sv.Count == 0) return $"{StackDetector.StackNames[s]}=N/A";
            int c = sv.Count(e => detector.Predict(e.Normalized) == s);
            return $"{StackDetector.StackNames[s]}={c}/{sv.Count}";
        }));

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E20-STACK-DETECTOR",
            Tier              = tier,
            BaselinePrimary   = baselineAcc,
            EnhancedPrimary   = enhancedAcc,
            PrimaryMetricName = "top1_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = survivalRate,
            ScalingSignalNote = "marker_survival_rate (% of stack markers surviving E15)",
            // Low survival → E15 kills markers → need to detect stack before normalization
            ShouldScaleUp     = survivalRate < 0.3 || val.Count < 20,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"perStack={perStack}, survival={survivalRate:P1}, " +
                    $"baseAcc={baselineAcc:P1}, enhAcc={enhancedAcc:P1}, " +
                    $"per_stack=[{perStackNotes}]"
        });
    }

    private static long   Stopwatch() => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
