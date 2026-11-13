namespace ExperimentsV2;

// StopSignalDetector classifies a unit of work:
// STOP (this is a step inside an episode) or CONTINUE (this is its own sub-episode).
public static class StopSignalDetector
{
    public const int CONTINUE    = 0;
    public const int STOP_PHASES = 1; // just execution order, not sub-episodes
    public const int STOP_TECH   = 2; // requires technical context to explain
    public const int STOP_SINGLE = 3; // one document produced once, no structure
    public const int STOP_NOLOOP = 4; // done once and accepted, no iteration

    public static readonly string[] SignalNames =
        ["CONTINUE", "STOP_PHASES", "STOP_TECH", "STOP_SINGLE", "STOP_NOLOOP"];

    // Rule-based baseline: apply all 4 signals sequentially.
    public static int PredictByRules(string taskDescription, bool hasIterations, bool hasTechTerms)
    {
        var lower = taskDescription.ToLowerInvariant();

        bool isPhaseSequence =
            new[] { "then ", "next ", "after that", "first ", "step ", "finally " }
            .Any(w => lower.Contains(w)) &&
            !lower.Contains("goal") && !lower.Contains("objective");
        if (isPhaseSequence) return STOP_PHASES;

        if (hasTechTerms) return STOP_TECH;

        if (lower.Contains("single ") || lower.Contains("one document") ||
            lower.Contains("produced once")) return STOP_SINGLE;

        if (!hasIterations) return STOP_NOLOOP;

        return CONTINUE;
    }

    // Synthetic training examples from orchestrator documents.
    public static List<(string Description, bool HasIter, bool HasTech, int Signal)>
        GenerateExamples(int seed, int n)
    {
        var rng = new Random(seed);
        var templates = new (string Desc, bool Iter, bool Tech, int Signal)[]
        {
            ("First read the requirements, then decide the architecture, then implement it",
             false, false, STOP_PHASES),
            ("Check the configuration, then run the tests, then deploy the result",
             false, false, STOP_PHASES),
            ("Open the file, parse the content, write the output to disk",
             false, false, STOP_PHASES),
            ("Configure the DI container to resolve the scoped service correctly",
             false, true, STOP_TECH),
            ("Fix the JWT token validation to handle the RS256 algorithm edge case",
             false, true, STOP_TECH),
            ("Produce the architecture diagram that will govern all subsequent decisions",
             false, false, STOP_SINGLE),
            ("Create the single requirements document approved first pass",
             false, false, STOP_SINGLE),
            ("Analyse the module dependencies to understand the coupling",
             false, false, STOP_NOLOOP),
            ("Write the summary of findings from the code review",
             false, false, STOP_NOLOOP),
            ("Implement the authentication flow with multiple rounds of review and correction",
             true, false, CONTINUE),
            ("Design the database schema, review with team, iterate based on feedback",
             true, false, CONTINUE),
            ("Build the search feature: first version, review output, three rounds of fixes",
             true, false, CONTINUE),
            ("Migrate the platform in stages, validate each stage before proceeding",
             false, false, CONTINUE),
        };

        var result = new List<(string, bool, bool, int)>();
        while (result.Count < n)
            result.Add(templates[rng.Next(templates.Length)]);
        return result;
    }
}
