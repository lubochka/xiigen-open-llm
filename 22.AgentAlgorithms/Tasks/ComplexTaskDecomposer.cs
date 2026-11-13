// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentAlgorithms;

// Breaks a multi-step task description into a list of sub-tasks.
// Uses simple heuristics: splits on conjunctions ("and", "then", "also", "additionally")
// and assigns a required role and complexity based on keywords.
// False conjunctions (right side is not an independent operation) are re-joined.
public sealed class ComplexTaskDecomposer
{
    // Words that indicate the task should be split at this point.
    private static readonly string[] SplitConjunctions
        = ["and then", " then ", " also ", " additionally ", " and "];

    // Imperative verbs that signal an independent operation on the right side of a conjunction.
    private static readonly HashSet<string> ImperativeVerbs = new(StringComparer.OrdinalIgnoreCase)
    {
        "read", "write", "find", "search", "refactor", "rename", "move",
        "test", "verify", "run", "build", "deploy", "delete", "create",
        "add", "update", "remove", "fix", "implement", "check",
        "analyze", "review", "generate", "extract", "parse",
        "report", "format", "validate"
    };

    // Pronoun-only objects that indicate the right side depends on the left (false conjunction).
    private static readonly HashSet<string> DependentPronouns = new(StringComparer.OrdinalIgnoreCase)
    {
        "them", "it", "they", "those", "these", "him", "her", "its"
    };

    public OperationResult<IReadOnlyList<SubTask>> Decompose(string taskDescription)
    {
        if (string.IsNullOrWhiteSpace(taskDescription))
            return OperationResult<IReadOnlyList<SubTask>>.Fail("Task description is empty.", "EMPTY_TASK");

        var parts = SplitAtConjunctions(taskDescription);

        var subTasks = parts
            .Select(p => new SubTask(p.Trim(), InferRole(p)))
            .ToList();

        return OperationResult<IReadOnlyList<SubTask>>.Ok(subTasks);
    }

    // Split a description string at known conjunctions, re-joining false conjunctions.
    private static List<string> SplitAtConjunctions(string text)
    {
        var result = new List<string> { text };

        foreach (var conjunction in SplitConjunctions)
        {
            var newResult = new List<string>();
            foreach (var part in result)
            {
                int idx = part.IndexOf(conjunction, StringComparison.OrdinalIgnoreCase);
                if (idx > 0)
                {
                    string left  = part[..idx].Trim();
                    string right = part[(idx + conjunction.Length)..].Trim();

                    if (IsIndependentOperation(right))
                    {
                        newResult.Add(left);
                        newResult.Add(right);
                    }
                    else
                    {
                        newResult.Add(part); // re-join: false conjunction
                    }
                }
                else
                {
                    newResult.Add(part);
                }
            }
            result = newResult;
        }

        return result.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
    }

    // Returns true when the right side of a conjunction is a self-contained imperative:
    // starts with an imperative verb AND does not use a dependent pronoun as the direct object.
    private static bool IsIndependentOperation(string part)
    {
        var words = part.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0) return false;

        if (!ImperativeVerbs.Contains(words[0])) return false;

        // If the second word is a dependent pronoun the right side is not independent.
        if (words.Length >= 2 && DependentPronouns.Contains(words[1])) return false;

        return true;
    }

    // Infer the required sub-agent role from keyword patterns.
    private static string InferRole(string part)
    {
        var lower = part.ToLowerInvariant();
        if (lower.Contains("read") || lower.Contains("search") || lower.Contains("find"))
            return "reader";
        if (lower.Contains("refactor") || lower.Contains("rename") || lower.Contains("move"))
            return "refactorer";
        if (lower.Contains("test") || lower.Contains("verify"))
            return "tester";
        return "coder";
    }
}
