// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentAlgorithms;

// Task decomposer that uses a bigram boundary model to avoid false splits at compound phrases.
// Inspired by E03 (trigram Markov, +10–15% stable across Scale 0–3 in 34.Experiments).
// Splits on "and" only when the (prev, next) word pair across the conjunction is a
// known task-boundary pattern from the training corpus.
// "then", "also", "additionally" always split (unambiguous clause boundaries).
//
// Example: "Find methods that return null and log them"
//   ComplexTaskDecomposer splits → ["Find methods that return null", "log them"]  (wrong)
//   NgramTaskDecomposer does NOT split → one task                                 (correct)
//   because ("null", "log") is not a known boundary pair.
public sealed class NgramTaskDecomposer
{
    // Known (prev, next) word pairs across a conjunction that reliably separate two
    // independent sub-tasks in a task description. Derived from typical task corpora.
    private static readonly (string Prev, string Next)[] BoundaryCorpus =
    [
        ("method",    "write"),   ("method",    "add"),      ("method",    "update"), ("method",    "test"),
        ("class",     "add"),     ("class",     "write"),    ("class",     "update"), ("class",     "run"),
        ("service",   "add"),     ("service",   "write"),    ("service",   "update"), ("service",   "run"),
        ("logic",     "move"),    ("logic",     "extract"),  ("logic",     "add"),
        ("file",      "process"), ("file",      "read"),     ("file",      "write"),
        ("code",      "run"),     ("code",      "test"),     ("code",      "document"),
        ("bug",       "run"),     ("bug",       "verify"),   ("bug",       "test"),
        ("them",      "run"),     ("them",      "verify"),   ("them",      "test"),
        ("it",        "run"),     ("it",        "test"),     ("it",        "verify"),
        ("callers",   "run"),     ("callers",   "test"),
        ("tests",     "run"),     ("tests",     "verify"),
        ("interface", "add"),     ("interface", "update"),
    ];

    // These conjunctions always mark a task boundary, regardless of context.
    private static readonly HashSet<string> HardSplitWords =
        new(StringComparer.OrdinalIgnoreCase) { "then", "also", "additionally" };

    private readonly Dictionary<string, HashSet<string>> _boundaries;

    public NgramTaskDecomposer()
    {
        _boundaries = new(StringComparer.OrdinalIgnoreCase);
        foreach (var (prev, next) in BoundaryCorpus)
        {
            if (!_boundaries.TryGetValue(prev, out var nexts))
                _boundaries[prev] = nexts = new(StringComparer.OrdinalIgnoreCase);
            nexts.Add(next);
        }
    }

    public OperationResult<IReadOnlyList<SubTask>> Decompose(string taskDescription)
    {
        if (string.IsNullOrWhiteSpace(taskDescription))
            return OperationResult<IReadOnlyList<SubTask>>.Fail("Task description is empty.", "EMPTY_TASK");

        var words    = taskDescription.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var parts    = SplitWords(words);
        var subTasks = parts
            .Select(p => new SubTask(p.Trim(), InferRole(p)))
            .ToList();

        return OperationResult<IReadOnlyList<SubTask>>.Ok(subTasks);
    }

    private List<string> SplitWords(string[] words)
    {
        var parts = new List<string>();
        int start = 0;

        for (int i = 1; i < words.Length - 1; i++)
        {
            string lower = words[i].ToLowerInvariant().Trim('.', ',', ';');

            if (HardSplitWords.Contains(lower))
            {
                parts.Add(string.Join(" ", words[start..i]).Trim());
                start = i + 1;
                continue;
            }

            if (lower != "and") continue;

            // "and": split only when (prev, next) matches a known boundary pair
            string prev = Normalize(words[i - 1]);
            string next = Normalize(words[i + 1]);

            if (IsBoundaryPair(prev, next))
            {
                parts.Add(string.Join(" ", words[start..i]).Trim());
                start = i + 1;
            }
        }

        parts.Add(string.Join(" ", words[start..]).Trim());
        return parts.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
    }

    private bool IsBoundaryPair(string prev, string next)
    {
        // Direct match
        if (_boundaries.TryGetValue(prev, out var nexts) && nexts.Contains(next))
            return true;

        // Suffix match for PascalCase names: "UserService" → matches "service" key
        foreach (var (key, values) in _boundaries)
            if (prev.EndsWith(key, StringComparison.OrdinalIgnoreCase) && values.Contains(next))
                return true;

        return false;
    }

    private static string Normalize(string word)
        => word.ToLowerInvariant().Trim('.', ',', ';', '!', '?');

    private static string InferRole(string part)
    {
        var lower = part.ToLowerInvariant();
        if (lower.Contains("read")    || lower.Contains("search") || lower.Contains("find"))
            return "reader";
        if (lower.Contains("refactor") || lower.Contains("rename") || lower.Contains("move"))
            return "refactorer";
        if (lower.Contains("test")    || lower.Contains("verify"))
            return "tester";
        return "coder";
    }
}
