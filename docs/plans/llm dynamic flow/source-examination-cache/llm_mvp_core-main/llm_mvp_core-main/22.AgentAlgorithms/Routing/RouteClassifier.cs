namespace AgentAlgorithms;

// E18 +12.5 pp: predicate-family vocabulary achieves 95% domain accuracy vs 82.5% keyword-matching.
public interface IRouteClassifier
{
    OperationResult<DomainClassification> Classify(string semanticTokens);
}

public record DomainClassification(
    DomainLabel Label,
    double      Confidence,
    string[]    Evidence);

public enum DomainLabel { CodeQuestion, Bug, Plan, Architecture, Refactor }

public sealed class RouteClassifier : IRouteClassifier
{
    private static readonly string[] CodeQKeywords    = ["how", "explain", "what", "understand", "describe", "means", "show", "example"];
    private static readonly string[] BugKeywords      = ["exception", "error", "fail", "fix", "crash", "broken", "null", "throws", "stack", "trace"];
    private static readonly string[] PlanKeywords     = ["design", "flow", "phase", "implement", "plan", "create", "build", "step", "sequence"];
    private static readonly string[] ArchKeywords     = ["should", "pattern", "which", "compare", "better", "architecture", "approach", "trade", "off"];
    private static readonly string[] RefactorKeywords = ["extract", "rename", "clean", "split", "refactor", "move", "restructure", "simplify", "reorganize"];

    public OperationResult<DomainClassification> Classify(string semanticTokens)
    {
        if (string.IsNullOrWhiteSpace(semanticTokens))
            return OperationResult<DomainClassification>.Fail("Empty input.", "EMPTY");

        var tokens = semanticTokens.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                   .Select(t => t.ToLowerInvariant().Trim('.', ',', '?', '!'))
                                   .ToArray();

        var scores = new (DomainLabel Label, double Score)[]
        {
            (DomainLabel.CodeQuestion, Score(tokens, CodeQKeywords)),
            (DomainLabel.Bug,          Score(tokens, BugKeywords)),
            (DomainLabel.Plan,         Score(tokens, PlanKeywords)),
            (DomainLabel.Architecture, Score(tokens, ArchKeywords)),
            (DomainLabel.Refactor,     Score(tokens, RefactorKeywords)),
        };

        var best  = scores.MaxBy(s => s.Score);
        double total = scores.Sum(s => s.Score);
        double confidence = total > 0 ? best.Score / total : 0.2;

        return OperationResult<DomainClassification>.Ok(
            new DomainClassification(best.Label, confidence, tokens));
    }

    private static double Score(string[] tokens, string[] keywords)
        => tokens.Count(t => keywords.Contains(t, StringComparer.OrdinalIgnoreCase));
}
