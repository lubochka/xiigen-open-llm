// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// ComplexityRouter determines request complexity: SINGLE_STEP or MULTI_STEP.
// SINGLE_STEP — one response, one operation.
// MULTI_STEP  — needs decomposition into sub-tasks.
public sealed class ComplexityRouter
{
    public const int SINGLE_STEP = 0;
    public const int MULTI_STEP  = 1;

    // Number of transitions between different FamilyCollapser tags above which = MULTI_STEP
    private readonly int _transitionThreshold;

    public ComplexityRouter(int transitionThreshold = 3)
        => _transitionThreshold = transitionThreshold;

    // Enhanced: count transitions between different action-family groups.
    // SemanticNormalizer+FamilyCollapser is designed for C# code tokens, not English
    // sentences. Direct word-to-family mapping gives a meaningful transition signal
    // for English prompts: "Fix svc then refactor" = BUGFIX→CHANGE (2 families = MULTI).
    // "Find null and log them" = QUERY→QUERY (1 family = SINGLE).
    public int Predict(string prompt)
    {
        var words    = prompt.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var families = words.Select(GetWordFamily).Where(f => f != null).ToList();
        if (families.Count < 2) return SINGLE_STEP;

        int transitions = 0;
        for (int i = 1; i < families.Count; i++)
            if (families[i] != families[i - 1])
                transitions++;

        return transitions >= _transitionThreshold ? MULTI_STEP : SINGLE_STEP;
    }

    private static string? GetWordFamily(string word) =>
        new[] { "fix", "debug", "error", "fail", "wrong", "broken", "exception" }.Any(word.Contains) ? "BUGFIX" :
        new[] { "add", "create", "implement", "refactor", "update", "remove", "build" }.Any(word.Contains) ? "CREATE" :
        new[] { "test", "unit", "integration", "coverage" }.Any(word.Contains) ? "TEST" :
        new[] { "valid", "check", "verify", "ensure", "assert" }.Any(word.Contains) ? "VALIDATE" :
        new[] { "get", "find", "show", "list", "explain", "what", "how", "where" }.Any(word.Contains) ? "QUERY" :
        new[] { "plan", "flow", "step", "phase", "design", "architect", "sequence" }.Any(word.Contains) ? "PLAN" :
        null;

    // Baseline: presence of a conjunction word
    public static int PredictByConj(string prompt)
    {
        var lower     = prompt.ToLowerInvariant();
        var conjWords = new[] { " and ", " then ", " after that ", " also ", " plus " };
        return conjWords.Any(c => lower.Contains(c)) ? MULTI_STEP : SINGLE_STEP;
    }

    // Tune the optimal transition threshold on training data
    public static int TuneThreshold(List<(string Prompt, int Complexity)> train)
    {
        int best = 3; int bestScore = 0;
        for (int t = 1; t <= 8; t++)
        {
            var r     = new ComplexityRouter(t);
            int score = train.Count(e => r.Predict(e.Prompt) == e.Complexity);
            if (score > bestScore) { bestScore = score; best = t; }
        }
        return best;
    }
}
