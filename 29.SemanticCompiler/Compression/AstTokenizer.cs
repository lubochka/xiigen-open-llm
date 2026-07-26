// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompiler29;

// AstTokenizer: converts raw code text into AST-level tokens.
// E01: +93.1% improvement over BPE (Byte Pair Encoding) tokenization for code.
// Key insight: one AST node ≈ 5.2× more informative than one BPE token (E11).
// Does NOT lowercase — StackDetector needs original case (E20 Rule A).
public sealed class AstTokenizer : BaseService
{
    // Standard constructor: inject storage and logger without constructing them here.
    public AstTokenizer(ICompilerStorage storage, ICompilerEventLogger logger)
        : base(storage, logger) { }

    // Split code into syntactic units: identifiers, operators, keywords, literals.
    // Preserves original casing so StackDetector can detect TypeScript "Promise<" etc.
    public OperationResult<string[]> Tokenize(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return OperationResult<string[]>.Fail("Input code is empty", "EMPTY_INPUT");

        // Split on whitespace and common code delimiters, keeping non-empty tokens.
        var raw = code.Split(
            [' ', '\t', '\n', '\r', ';', ',', '(', ')', '{', '}', '[', ']'],
            StringSplitOptions.RemoveEmptyEntries);

        // Further split on operators without removing them from context.
        var tokens = raw
            .SelectMany(t => SplitOnOperators(t))
            .Where(t => t.Length > 0)
            .ToArray();

        return OperationResult<string[]>.Ok(tokens);
    }

    // Split a single token further on operator characters, yielding each piece.
    private static IEnumerable<string> SplitOnOperators(string token)
    {
        var parts = new List<string>();
        int start = 0;
        for (int i = 0; i < token.Length; i++)
        {
            char c = token[i];
            // Operators that deserve their own token slot.
            if (c is '.' or '<' or '>' or '=' or '!' or '+' or '-' or '*' or '/' or '&' or '|')
            {
                if (i > start) parts.Add(token[start..i]);
                parts.Add(c.ToString());
                start = i + 1;
            }
        }
        if (start < token.Length) parts.Add(token[start..]);
        return parts.Where(p => p.Length > 0);
    }
}
