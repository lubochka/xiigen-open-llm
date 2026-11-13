namespace ExperimentsV2;

// E35: DependencyDetector — are sub-episodes A and B sequential or parallel?
// Baseline:  always SEQUENTIAL (safe but slow)
// Enhanced:  K-NN on predicate overlap + "produces/consumes" signal
// Metric:    dependency_accuracy + parallel_opportunity_rate
//            (how many parallel pairs are correctly identified)
public sealed class E35DependencyDetector
{
    public const int SEQUENTIAL = 0;
    public const int PARALLEL   = 1;

    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var pairs    = GenerateDependencyPairs(seed: 42, n: tier * 50);
        var rng      = new Random(42);
        var shuffled = pairs.OrderBy(_ => rng.Next()).ToList();
        int split    = (int)(shuffled.Count * 0.8);
        var val      = shuffled.Skip(split).ToList();

        // Detect dependency: A produces what B consumes.
        bool IsSequential(string a, string b)
        {
            var aTags   = FamilyCollapser.CollapseString(a).ToHashSet();
            var bTags   = FamilyCollapser.CollapseString(b).ToHashSet();
            bool aCreates = aTags.Contains("CREATE_OP") || aTags.Contains("UPDATE_OP");
            bool bGets    = bTags.Contains("GET_OP");
            bool overlap  = aTags.Count(t => bTags.Contains(t)) >= 2;
            return (aCreates && bGets) || overlap;
        }

        // ── Baseline: always SEQUENTIAL ───────────────────────────────────────
        var t0 = Stopwatch();
        int baseOk    = val.Count(p => p.Expected == SEQUENTIAL);
        var parallels  = val.Where(p => p.Expected == PARALLEL).ToList();
        double baseAcc = baseOk / (double)val.Count;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced ──────────────────────────────────────────────────────────
        var t1 = Stopwatch();
        int enhOk    = val.Count(p =>
            (IsSequential(p.A, p.B) ? SEQUENTIAL : PARALLEL) == p.Expected);
        int enhParOk = parallels.Count(p => !IsSequential(p.A, p.B));
        double enhAcc = enhOk / (double)val.Count;
        double enhPOR = parallels.Count > 0 ? enhParOk / (double)parallels.Count : 0;
        double enhancedMs = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E35-DEPENDENCY-DETECTOR",
            Tier              = tier,
            BaselinePrimary   = baseAcc,
            EnhancedPrimary   = enhAcc,
            PrimaryMetricName = "dependency_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = enhPOR,
            ScalingSignalNote = "parallel_opportunity_rate (parallel pairs correctly found)",
            ShouldScaleUp     = val.Count < 15,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"pairs={pairs.Count}, parallels={parallels.Count}, " +
                    $"baseAcc={baseAcc:P1}(POR=0%), " +
                    $"enhAcc={enhAcc:P1}(POR={enhPOR:P1})"
        });
    }

    private sealed record DependencyPair(string A, string B, int Expected);

    private static List<DependencyPair> GenerateDependencyPairs(int seed, int n)
    {
        var rng = new Random(seed);
        var templates = new DependencyPair[]
        {
            // SEQUENTIAL (A → B)
            new("Design the database schema", "Implement the data access layer", SEQUENTIAL),
            new("Build the authentication service", "Implement the protected routes", SEQUENTIAL),
            new("Create the API contracts", "Build the client that consumes them", SEQUENTIAL),
            new("Write the architecture document", "Implement the components it defines", SEQUENTIAL),
            new("Define the data model", "Create the migration scripts for it", SEQUENTIAL),
            new("Analyse the existing codebase", "Design the refactoring plan based on findings", SEQUENTIAL),
            new("Build the base model", "Fine-tune the adapter on project data", SEQUENTIAL),
            new("Set up the test infrastructure", "Write the integration tests that use it", SEQUENTIAL),
            // PARALLEL (A || B)
            new("Write the unit tests", "Write the documentation", PARALLEL),
            new("Implement the search feature", "Implement the notification feature", PARALLEL),
            new("Fix the login bug", "Fix the export bug", PARALLEL),
            new("Write tests for module A", "Write tests for module B", PARALLEL),
            new("Migrate the user data", "Migrate the product catalogue", PARALLEL),
            new("Review the API design", "Review the database schema", PARALLEL),
        };
        var result = new List<DependencyPair>();
        while (result.Count < n)
            result.Add(templates[rng.Next(templates.Length)]);
        return result;
    }

    private static long   Stopwatch()     => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
