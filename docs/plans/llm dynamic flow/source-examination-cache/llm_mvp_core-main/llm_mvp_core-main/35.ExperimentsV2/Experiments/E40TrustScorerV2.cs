namespace ExperimentsV2;

// E40v2: TrustScorer — fixes seed-42 data distribution issue with StratifiedSplit.
// Root cause of E40v1 (+0.0%): seed=42 shuffle put all LOW-trust examples in val
// → K-NN had no LOW examples in train → predicted MEDIUM for all LOW → same as baseline.
// Fix: StratifiedSplit ensures LOW/MEDIUM/HIGH all appear proportionally in both splits.
// Baseline:  simple rule: is_internal || is_official ? HIGH : MEDIUM
// Enhanced:  K-NN (k=5) on 7-feature vector
public sealed class E40TrustScorerV2
{
    public const int LOW    = 0;
    public const int MEDIUM = 1;
    public const int HIGH   = 2;

    public static readonly string[] TrustNames = ["LOW", "MEDIUM", "HIGH"];

    public sealed record SourceFeatures(
        bool IsInternal,
        bool HasCIPassing,
        bool HasTests,
        int  UpdateDaysAgo,
        int  PopularityScore,
        bool IsOfficialDocs,
        bool DomainMatch);

    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var examples = GenerateExamples(seed: 42, n: tier * 50);

        // StratifiedSplit by TrustLevel ensures all 3 classes in both splits
        var (train, val) = StratifiedSplit.Split(
            examples, x => x.TrustLevel, trainRatio: 0.8, seed: 42);

        if (val.Count < 3)
            return OperationResult<ExperimentV2Result>.Fail("Too few val examples");

        // ── Baseline: simple rule ─────────────────────────────────────────────
        var t0 = Stopwatch();
        int baseOk = val.Count(x =>
        {
            int pred = x.Features.IsInternal || x.Features.IsOfficialDocs ? HIGH : MEDIUM;
            return pred == x.TrustLevel;
        });
        double baseAcc    = baseOk / (double)val.Count;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: K-NN on feature vector ─────────────────────────────────
        var t1 = Stopwatch();
        var trainVecs = train.Select(x => (Vec: ToVector(x.Features), Trust: x.TrustLevel)).ToList();
        int enhOk = val.Count(x =>
        {
            float[] q = ToVector(x.Features);
            int predicted = trainVecs
                .OrderBy(t => EuclideanDist(q, t.Vec))
                .Take(5)
                .GroupBy(t => t.Trust)
                .OrderByDescending(g => g.Count())
                .First().Key;
            return predicted == x.TrustLevel;
        });
        double enhAcc     = enhOk / (double)val.Count;
        double enhancedMs = ElapsedMs(t1);

        string perLevel = string.Join(", ", Enumerable.Range(0, 3).Select(lvl =>
        {
            var sub = val.Where(x => x.TrustLevel == lvl).ToList();
            if (sub.Count == 0) return $"{TrustNames[lvl]}=N/A";
            var vecs = train.Select(x => (Vec: ToVector(x.Features), Trust: x.TrustLevel)).ToList();
            int ok = sub.Count(x =>
            {
                float[] q = ToVector(x.Features);
                int pred  = vecs.OrderBy(t => EuclideanDist(q, t.Vec)).Take(5)
                    .GroupBy(t => t.Trust).OrderByDescending(g => g.Count()).First().Key;
                return pred == lvl;
            });
            return $"{TrustNames[lvl]}={ok}/{sub.Count}";
        }));

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E40V2-TRUST-SCORER",
            Tier              = tier,
            BaselinePrimary   = baseAcc,
            EnhancedPrimary   = enhAcc,
            PrimaryMetricName = "trust_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = (double)val.Count(x => x.TrustLevel == LOW) / val.Count,
            ScalingSignalNote = "low_trust_ratio (more LOW = harder task)",
            ShouldScaleUp     = val.Count < 15,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"examples={examples.Count}, train={train.Count}, val={val.Count}, " +
                    $"baseAcc={baseAcc:P1}, enhAcc={enhAcc:P1}, per_level=[{perLevel}]"
        });
    }

    private static float[] ToVector(SourceFeatures f) =>
    [
        f.IsInternal      ? 1f : 0f,
        f.HasCIPassing    ? 1f : 0f,
        f.HasTests        ? 1f : 0f,
        Math.Max(0, 1f - f.UpdateDaysAgo / 365f),
        f.PopularityScore / 3f,
        f.IsOfficialDocs  ? 1f : 0f,
        f.DomainMatch     ? 1f : 0f,
    ];

    private static double EuclideanDist(float[] a, float[] b)
        => Math.Sqrt(a.Zip(b).Sum(p => Math.Pow(p.First - p.Second, 2)));

    private sealed record Example(SourceFeatures Features, int TrustLevel);

    private static List<Example> GenerateExamples(int seed, int n)
    {
        var rng = new Random(seed);
        var templates = new Example[]
        {
            new(new(true,  true,  true,  0,   0, false, true),  HIGH),
            new(new(true,  true,  true,  5,   0, false, true),  HIGH),
            new(new(false, false, false, 0,   0, true,  true),  HIGH),
            new(new(true,  false, true,  30,  0, false, false), HIGH),
            new(new(false, true,  true,  7,   3, false, true),  HIGH),
            new(new(false, true,  true,  30,  2, false, true),  MEDIUM),
            new(new(false, false, true,  60,  2, false, true),  MEDIUM),
            new(new(false, true,  false, 14,  2, false, false), MEDIUM),
            new(new(false, false, false, 7,   0, true,  false), MEDIUM),
            new(new(false, true,  true,  90,  1, false, false), MEDIUM),
            new(new(false, false, false, 200, 0, false, false), LOW),
            new(new(false, false, false, 365, 0, false, true),  LOW),
            new(new(false, false, false, 30,  0, false, false), LOW),
            new(new(false, true,  false, 180, 0, false, false), LOW),
        };
        var result = new List<Example>();
        while (result.Count < n)
            result.Add(templates[rng.Next(templates.Length)]);
        return result;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
