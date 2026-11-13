// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompiler;

// CSharpBeautifier converts a predicate list to a human-readable C# summary.
// Used when no template match is found or when stack is CSharp.
public static class CSharpBeautifier
{
    // Predicate → readable C# phrase
    private static readonly Dictionary<int, string> PredicatePhrase = new()
    {
        [LogicExtractor.NULL_GUARD]   = "null guard",
        [LogicExtractor.FAIL]         = "return Fail",
        [LogicExtractor.ITERATE]      = "iteration",
        [LogicExtractor.HANDLE_ERROR] = "error handling",
        [LogicExtractor.AWAIT]        = "async/await",
        [LogicExtractor.THROW_OP]     = "throw exception",
        [LogicExtractor.DECLARE]      = "declaration",
        [LogicExtractor.ASSIGN]       = "assignment",
        [LogicExtractor.CREATE]       = "object creation",
        [LogicExtractor.CALL]         = "method call",
        [LogicExtractor.BRANCH]       = "conditional branch",
        [LogicExtractor.SUCCESS]      = "return Ok",
        [LogicExtractor.GET_CALL]     = "get call",
        [LogicExtractor.VALIDATE_OP]  = "validation",
        [LogicExtractor.CREATE_OP]    = "create operation",
        [LogicExtractor.UPDATE_OP]    = "update operation",
        [LogicExtractor.DELETE_OP]    = "delete operation",
        [LogicExtractor.PROCESS_OP]   = "processing",
        [LogicExtractor.TRANSFORM]    = "transform",
        [LogicExtractor.SEND_OP]      = "send operation",
        [LogicExtractor.LOG_OP]       = "logging",
        [LogicExtractor.AUTH_OP]      = "authorization check",
    };

    // Render predicate list as a readable description of the method structure
    public static string Beautify(IReadOnlyList<int> predicates, StackLabel stack)
    {
        if (predicates.Count == 0)
            return stack == StackLabel.CSharp
                ? "// No predicates detected — empty method body"
                : "# No predicates detected";

        var lines = new List<string>();

        if (stack == StackLabel.CSharp)
        {
            lines.Add("// Method structure:");
            foreach (var p in predicates)
            {
                string phrase = PredicatePhrase.GetValueOrDefault(p, $"predicate_{p}");
                lines.Add($"//   → {phrase}");
            }
        }
        else
        {
            lines.Add("# Method structure:");
            foreach (var p in predicates)
            {
                string phrase = PredicatePhrase.GetValueOrDefault(p, $"predicate_{p}");
                lines.Add($"#   → {phrase}");
            }
        }

        return string.Join("\n", lines);
    }

    // Predicate count summary (used in Gate B assertions)
    public static string Summarize(IReadOnlyList<int> predicates)
    {
        var counts = predicates
            .GroupBy(p => p)
            .Select(g => $"{PredicatePhrase.GetValueOrDefault(g.Key, $"p{g.Key}")}×{g.Count()}");
        return string.Join(", ", counts);
    }
}
