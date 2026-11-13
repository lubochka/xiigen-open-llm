namespace ExperimentsV2;

// E30v2: PredicateCache — word-level Jaccard replaces predicate-vector cosine.
// Root cause of E30v1 failure (-48.3%): predicate vectors for distinct C# operations
// are nearly identical (all contain NULL_GUARD + VALIDATE + SUCCESS) →
// cosine threshold 0.85 fires on semantically unrelated queries → wrong cached actions.
// Fix: SemanticNormalizer.Normalize preserves domain words → Jaccard on words
//      distinguishes "null check" vs "unit test" vs "auth review".
// Baseline:  predicate-vector cosine cache (E30v1 approach)
// Enhanced:  word-level Jaccard cache (threshold = 0.30) + same bigram fallback
public sealed class E30PredicateCacheV2
{
    private const double WordSimilarityThreshold = 0.30;
    private const double VecSimilarityThreshold  = 0.85;

    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var cacheData = BuildCacheData();
        var testCases = BuildTestCases();

        // ── Baseline: predicate-vector cosine (old E30 approach) ─────────────
        // All NL query strings → LogicExtractor finds nothing → all-zero vectors
        // → cosine = 0 → no cache hits → always returns fallback label (0)
        var t0 = Stopwatch();
        var vecCache = new List<(float[] Vec, int Label)>();
        foreach (var (text, label) in cacheData)
            vecCache.Add((PredicateCache.BuildVector(text), label));

        int baseHits = 0, baseOk = 0;
        foreach (var (text, expected) in testCases)
        {
            float[] qVec = PredicateCache.BuildVector(text);
            var best = vecCache
                .Select(e => (e.Label, Sim: CosineSim(qVec, e.Vec)))
                .OrderByDescending(x => x.Sim)
                .First();
            int predicted = best.Sim >= (float)VecSimilarityThreshold
                ? best.Label : 0; // fallback = most common label
            if (best.Sim >= (float)VecSimilarityThreshold) baseHits++;
            if (predicted == expected) baseOk++;
        }
        double baseHitRate = baseHits / (double)testCases.Count;
        double baseAcc     = baseOk   / (double)testCases.Count;
        double baselineMs  = ElapsedMs(t0);

        // ── Enhanced: word-level Jaccard cache ───────────────────────────────
        var t1 = Stopwatch();
        var wordCache = new List<(HashSet<string> Words, int Label)>();
        foreach (var (text, label) in cacheData)
            wordCache.Add((SemanticNormalizer.Normalize(text).ToHashSet(), label));

        int enhHits = 0, enhOk = 0;
        foreach (var (text, expected) in testCases)
        {
            var qWords = SemanticNormalizer.Normalize(text).ToHashSet();
            var best = wordCache
                .Select(e => (e.Label, Sim: WordJaccard(qWords, e.Words)))
                .OrderByDescending(x => x.Sim)
                .First();
            int predicted = best.Sim >= WordSimilarityThreshold
                ? best.Label : 0;
            if (best.Sim >= WordSimilarityThreshold) enhHits++;
            if (predicted == expected) enhOk++;
        }
        double enhHitRate = enhHits / (double)testCases.Count;
        double enhAcc     = enhOk   / (double)testCases.Count;
        double enhancedMs = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E30V2-PREDICATE-CACHE",
            Tier              = tier,
            BaselinePrimary   = baseAcc,
            EnhancedPrimary   = enhAcc,
            PrimaryMetricName = "cache_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = enhHitRate - baseHitRate,
            ScalingSignalNote = "hit_rate_delta (word cache fires more selectively)",
            ShouldScaleUp     = testCases.Count < 15,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"cacheSize={cacheData.Count}, testCases={testCases.Count}, " +
                    $"base: hits={baseHits}({baseHitRate:P0}) acc={baseAcc:P1}, " +
                    $"enh: hits={enhHits}({enhHitRate:P0}) acc={enhAcc:P1}"
        });
    }

    // Cache entries: 2 examples per category (8 categories × 2 = 16)
    private static List<(string Text, int Label)> BuildCacheData() =>
    [
        // 0 = null_check
        ("Add null validation for the user parameter before processing the request",         0),
        ("Insert guard clause checking null before calling the downstream service",          0),
        // 1 = unit_test
        ("Write unit tests for the authentication service class covering edge cases",        1),
        ("Create test coverage for the payment processor module with mock dependencies",     1),
        // 2 = code_review
        ("Review the controller for security vulnerabilities and coding standard issues",    2),
        ("Check the pull request for code quality compliance and documentation gaps",        2),
        // 3 = planning
        ("Design the architecture for microservice user registration workflow steps",        3),
        ("Create implementation plan for feature flag system integration timeline",          3),
        // 4 = refactor
        ("Refactor the duplicated validation logic into a shared reusable helper method",    4),
        ("Extract common error handling code into a base exception handler class",           4),
        // 5 = docs
        ("Write documentation for the REST API endpoint specifications and examples",        5),
        ("Add XML comments to all public methods in the service and repository layers",      5),
        // 6 = deploy
        ("Configure environment variables for the production deployment pipeline settings",  6),
        ("Set up connection strings and secrets for the staging environment configuration",  6),
        // 7 = debug
        ("Investigate memory leak in the batch processing background service worker",        7),
        ("Profile performance bottleneck in the database query execution path method",       7),
    ];

    // Test cases: 1-2 per category, verifying correct cache behavior
    private static List<(string Text, int Expected)> BuildTestCases() =>
    [
        ("Insert null guard for the customer object before method invocation call",          0),
        ("Validate that the input parameter is not null before the service processes it",    0),
        ("Add test cases for the order service business logic and validation rules",         1),
        ("Implement unit test suite for the inventory management service module",            1),
        ("Inspect the feature branch for best practices and security review compliance",     2),
        ("Evaluate the merge request code quality and adherence to architectural standards", 2),
        ("Draft the implementation roadmap for cache integration module planning steps",     3),
        ("Plan the rollout strategy for the new notification service architecture",          3),
        ("Extract the repeated exception handling into reusable middleware component",       4),
        ("Consolidate duplicated database access patterns into a shared repository layer",   4),
        ("Generate API reference documentation for the endpoint contracts and schemas",      5),
        ("Document public interface methods with usage examples and parameter descriptions", 5),
        ("Update the deployment configuration for the new cloud region infrastructure",      6),
        ("Prepare environment configuration and secrets for the production cloud deployment",6),
        ("Trace performance regression in the report generation and rendering system",       7),
        ("Debug memory allocation pattern during high-load batch processing operations",     7),
    ];

    private static double WordJaccard(HashSet<string> a, HashSet<string> b)
    {
        if (a.Count == 0 && b.Count == 0) return 1.0;
        int inter = a.Count(w => b.Contains(w));
        int union  = a.Count + b.Count - inter;
        return union == 0 ? 0 : (double)inter / union;
    }

    private static float CosineSim(float[] a, float[] b)
    {
        float dot = 0, na = 0, nb = 0;
        for (int i = 0; i < a.Length; i++) { dot += a[i]*b[i]; na += a[i]*a[i]; nb += b[i]*b[i]; }
        float denom = MathF.Sqrt(na) * MathF.Sqrt(nb);
        return denom < 1e-8f ? 0f : dot / denom;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
