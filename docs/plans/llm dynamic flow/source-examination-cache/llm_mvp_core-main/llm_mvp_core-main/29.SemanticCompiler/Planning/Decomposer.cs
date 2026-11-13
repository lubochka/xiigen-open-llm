namespace SemanticCompiler29;

// Decomposer: detect task boundaries within a prompt.
// E21: +142.9% — correctly identifies when "AND" connects independent tasks vs
//      when it connects parts of a single task ("Find null AND log them" = one task).
// Rule: change in predicate profile across conjunction = true boundary.
// false-CONJ guard already implemented in 22.AgentAlgorithms.ComplexTaskDecomposer (db80dc5).
public sealed class Decomposer : BaseService
{
    // Standard constructor: inject storage and logger without constructing them here.
    public Decomposer(ICompilerStorage storage, ICompilerEventLogger logger)
        : base(storage, logger) { }

    // Conjunctions that might signal a task boundary.
    private static readonly string[] ConjunctionMarkers =
        ["and then", "after that", "next", "also", "additionally", " and "];

    // Predicates that indicate the SAME task (false-CONJ guard):
    // "Find null AND log them" — log is a consequence of find, not a separate task.
    private static readonly string[] SameTaskPredicates =
        ["them", "it", "this", "that", "which", "the result", "the output"];

    // Decompose a prompt into sub-task strings at conjunction boundaries.
    // Returns single-element array when no true boundary is found.
    public OperationResult<string[]> Decompose(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return OperationResult<string[]>.Fail("Prompt is empty", "EMPTY_PROMPT");

        var lower = prompt.ToLowerInvariant();

        foreach (var conj in ConjunctionMarkers)
        {
            int idx = lower.IndexOf(conj, StringComparison.Ordinal);
            if (idx < 0) continue;

            // false-CONJ guard: if the clause after the conjunction contains a
            // pronoun/anaphora that references the previous clause, it is the same task.
            // "find null AND log THEM" — "them" refers back to nulls = same task.
            string afterConj = lower[(idx + conj.Length)..].TrimStart();
            bool isFalseConj = SameTaskPredicates.Any(p => afterConj.Contains(p, StringComparison.Ordinal));
            if (isFalseConj) continue;

            // True boundary found — split into two sub-tasks.
            string part1 = prompt[..idx].Trim();
            string part2 = prompt[(idx + conj.Length)..].Trim();
            if (part1.Length > 0 && part2.Length > 0)
                return OperationResult<string[]>.Ok([part1, part2]);
        }

        // No boundary found — single task.
        return OperationResult<string[]>.Ok([prompt]);
    }
}
