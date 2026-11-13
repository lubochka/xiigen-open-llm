// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

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

    // Single-word markers that always signal a new task clause.
    private static readonly HashSet<string> HardSplitWords =
        new(StringComparer.OrdinalIgnoreCase) { "then", "next", "also", "additionally" };

    // Multi-word markers that always signal a new task clause.
    private static readonly string[][] HardSplitPhrases =
    [
        ["and", "then"],
        ["after", "that"],
        ["and", "also"]
    ];

    // Verbs that can start an independent imperative task on the right side of "and".
    private static readonly HashSet<string> ImperativeVerbs = new(StringComparer.OrdinalIgnoreCase)
    {
        "add", "analyze", "build", "check", "create", "debug", "delete", "deploy",
        "extract", "find", "fix", "format", "generate", "implement", "list", "load",
        "move", "parse", "read", "refactor", "remove", "rename", "report", "review",
        "run", "search", "test", "update", "validate", "verify", "write"
    };

    // Nouns give each side enough object signal to be treated as a complete task.
    private static readonly HashSet<string> Nouns = new(StringComparer.OrdinalIgnoreCase)
    {
        "api", "bug", "capability", "class", "code", "component", "data", "dataset",
        "document", "documentation", "edge", "error", "file", "flow", "function",
        "issue", "logic", "method", "model", "null", "plan", "project", "prompt",
        "result", "service", "skill", "state", "task", "test", "token", "unit"
    };

    // Dependent pronouns make the right side part of the same task:
    // "Find null AND log them" means log the found nulls, not a new task.
    private static readonly HashSet<string> DependentPronouns = new(StringComparer.OrdinalIgnoreCase)
    {
        "it", "its", "them", "they", "this", "that", "these", "those"
    };

    // Decompose a prompt into sub-task strings at conjunction boundaries.
    // Returns single-element array when no true boundary is found.
    public OperationResult<string[]> Decompose(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return OperationResult<string[]>.Fail("Prompt is empty", "EMPTY_PROMPT");

        var words = prompt.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (words.Length < 3)
            return OperationResult<string[]>.Ok([prompt.Trim()]);

        var splitIndices = new List<(int Index, int MarkerLength)>();
        for (var index = 0; index < words.Length; index++)
        {
            var phraseLength = MatchHardSplitPhrase(words, index);
            if (phraseLength > 0)
            {
                splitIndices.Add((index, phraseLength));
                index += phraseLength - 1;
                continue;
            }

            var normalized = Normalize(words[index]);
            if (HardSplitWords.Contains(normalized))
            {
                splitIndices.Add((index, 1));
                continue;
            }

            if (string.Equals(normalized, "and", StringComparison.OrdinalIgnoreCase) &&
                IsTaskBoundary(words[..index], words[(index + 1)..]))
            {
                splitIndices.Add((index, 1));
            }
        }

        if (splitIndices.Count == 0)
            return OperationResult<string[]>.Ok([prompt.Trim()]);

        var parts = new List<string>();
        var start = 0;
        foreach (var (index, markerLength) in splitIndices)
        {
            AddPart(words, start, index, parts);
            start = index + markerLength;
        }

        AddPart(words, start, words.Length, parts);
        return OperationResult<string[]>.Ok(parts.Count == 0 ? [prompt.Trim()] : parts.ToArray());
    }

    private static int MatchHardSplitPhrase(string[] words, int index)
    {
        foreach (var phrase in HardSplitPhrases)
        {
            if (index + phrase.Length > words.Length)
                continue;

            var matches = true;
            for (var offset = 0; offset < phrase.Length; offset++)
            {
                if (!string.Equals(Normalize(words[index + offset]), phrase[offset], StringComparison.OrdinalIgnoreCase))
                {
                    matches = false;
                    break;
                }
            }

            if (matches)
                return phrase.Length;
        }

        return 0;
    }

    private static bool IsTaskBoundary(string[] leftWords, string[] rightWords)
    {
        if (leftWords.Length == 0 || rightWords.Length == 0)
            return false;

        var rightFirst = Normalize(rightWords[0]);
        if (!ImperativeVerbs.Contains(rightFirst))
            return false;
        if (rightWords.Length > 1 && DependentPronouns.Contains(Normalize(rightWords[1])))
            return false;

        return HasCompleteTaskShape(leftWords) && HasCompleteTaskShape(rightWords);
    }

    private static bool HasCompleteTaskShape(string[] words)
    {
        var normalized = words.Select(Normalize).Where(word => word.Length > 0).ToArray();
        var hasVerb = normalized.Any(word => ImperativeVerbs.Contains(word) || word.EndsWith("ing", StringComparison.Ordinal));
        var hasNoun = normalized.Any(word => Nouns.Contains(word) || LooksLikeCodeIdentifier(word));
        return hasVerb && hasNoun;
    }

    private static bool LooksLikeCodeIdentifier(string word)
    {
        return word.Length > 2 && (word.Contains("service", StringComparison.OrdinalIgnoreCase) ||
                                  word.Contains("manager", StringComparison.OrdinalIgnoreCase) ||
                                  word.Contains("controller", StringComparison.OrdinalIgnoreCase) ||
                                  word.Contains("provider", StringComparison.OrdinalIgnoreCase));
    }

    private static void AddPart(string[] words, int start, int end, List<string> parts)
    {
        if (start >= end)
            return;

        var part = string.Join(" ", words[start..end]).Trim();
        if (!string.IsNullOrWhiteSpace(part))
            parts.Add(part);
    }

    private static string Normalize(string word)
        => word.ToLowerInvariant().Trim('.', ',', ';', ':', '!', '?', '"', '\'');
}
