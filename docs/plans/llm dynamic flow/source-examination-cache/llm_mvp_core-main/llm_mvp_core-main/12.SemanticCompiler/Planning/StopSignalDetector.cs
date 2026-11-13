namespace SemanticCompiler;

// StopSignalDetector classifies a work unit: should the orchestrator stop here
// or continue to sub-decompose?
// E33: 4 stop signals detected by rule matching.
public static class StopSignalDetector
{
    public const int CONTINUE    = 0;
    public const int STOP_PHASES = 1; // sequential phases, not sub-episodes
    public const int STOP_TECH   = 2; // requires technical context to explain
    public const int STOP_SINGLE = 3; // one document produced once, no structure
    public const int STOP_NOLOOP = 4; // done once and accepted, no iteration

    public static readonly string[] SignalNames =
        ["CONTINUE", "STOP_PHASES", "STOP_TECH", "STOP_SINGLE", "STOP_NOLOOP"];

    // Rule-based detection (E33): apply all 4 signals in priority order
    public static int Detect(string taskDescription, bool hasIterations, bool hasTechTerms)
    {
        var lower = taskDescription.ToLowerInvariant();

        bool isPhaseSequence =
            new[] { "then ", "next ", "after that", "first ", "step ", "finally " }
            .Any(w => lower.Contains(w)) &&
            !lower.Contains("goal") && !lower.Contains("objective");

        if (isPhaseSequence) return STOP_PHASES;
        if (hasTechTerms)    return STOP_TECH;

        if (lower.Contains("single ") || lower.Contains("one document") ||
            lower.Contains("produced once")) return STOP_SINGLE;

        if (!hasIterations) return STOP_NOLOOP;

        return CONTINUE;
    }

    // Convenience: detect with automatic hasIterations / hasTechTerms inference
    public static int Detect(string taskDescription)
    {
        var lower = taskDescription.ToLowerInvariant();
        bool hasIterations = new[] { "round","iteration","review","feedback","again","repeat" }
            .Any(w => lower.Contains(w));
        bool hasTechTerms = new[] { "jwt","di","ioc","linq","async","await","ienumerable",
                                    "nullable","generic","dependency","injection","roslyn" }
            .Any(w => lower.Contains(w));
        return Detect(taskDescription, hasIterations, hasTechTerms);
    }
}
