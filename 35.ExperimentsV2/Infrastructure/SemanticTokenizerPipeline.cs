// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExperimentsV2;

// Three-level semantic compression pipeline for E15→E16→E17.
//   Level 0 (baseline for E15): raw identifier words — camelCase split only
//   Level 1 (E15 enhanced):    + stopword removal + dictionary-free stemming
//   Level 2 (E16 enhanced):    + synonym collapse to ~30 semantic ops
//   Level 3 (E17 enhanced):    + logical predicates from AST roles (12 fixed)
public static class SemanticTokenizerPipeline
{
    // ── Level 0: raw identifier words (baseline for E15) ─────────────────────

    public static List<string> Level0Tokens(string csharpSource)
    {
        var root   = CSharpSyntaxTree.ParseText(csharpSource).GetRoot();
        var result = new List<string>();
        foreach (var token in root.DescendantTokens())
            if (token.Kind() == SyntaxKind.IdentifierToken)
                result.AddRange(SplitIdentifier(token.ValueText));
        return result;
    }

    // ── Level 1: split + stopword removal + stemming (E15) ───────────────────

    public static List<string> Level1Tokens(string csharpSource)
        => Level0Tokens(csharpSource)
            .Where(w => !IsStopword(w))
            .Select(Stem)
            .Where(w => w.Length >= 2)
            .ToList();

    // ── Level 2: synonym collapse (E16) ──────────────────────────────────────

    public static List<string> Level2Tokens(string csharpSource)
        => Level1Tokens(csharpSource).Select(CollapseToOp).ToList();

    // ── Level 3: logical predicates from AST roles (E17) ─────────────────────

    public static List<string> Level3Tokens(string csharpSource)
    {
        var root = CSharpSyntaxTree.ParseText(csharpSource).GetRoot();
        var result = new List<string>();
        foreach (var node in root.DescendantNodes())
        {
            string? pred = ToLogicalPredicate(node);
            if (pred != null) result.Add(pred);
        }
        return result.Count >= 4 ? result : Level2Tokens(csharpSource);
    }

    // ── Vocabulary encoding: string tokens → integer IDs ─────────────────────

    public static (List<int> Encoded, Dictionary<string, int> Vocab)
        Encode(List<string> tokens)
    {
        var vocab   = new Dictionary<string, int>(StringComparer.Ordinal);
        var encoded = new List<int>(tokens.Count);
        foreach (var t in tokens)
        {
            if (!vocab.TryGetValue(t, out int id))
                vocab[t] = id = vocab.Count;
            encoded.Add(id);
        }
        return (encoded, vocab);
    }

    // ── CamelCase / PascalCase / snake_case splitter ─────────────────────────

    public static IEnumerable<string> SplitIdentifier(string name)
    {
        if (string.IsNullOrEmpty(name)) yield break;

        // snake_case
        if (name.Contains('_'))
        {
            foreach (var part in name.Split('_', StringSplitOptions.RemoveEmptyEntries))
                foreach (var w in SplitCamel(part))
                    yield return w.ToLowerInvariant();
            yield break;
        }

        foreach (var w in SplitCamel(name))
            yield return w.ToLowerInvariant();
    }

    private static IEnumerable<string> SplitCamel(string s)
    {
        int start = 0;
        for (int i = 1; i < s.Length; i++)
        {
            if (char.IsUpper(s[i]) && !char.IsUpper(s[i - 1]))
            {
                string seg = s[start..i];
                if (seg.Length > 0) yield return seg;
                start = i;
            }
            else if (char.IsUpper(s[i]) && i + 1 < s.Length && char.IsLower(s[i + 1])
                     && char.IsUpper(s[i - 1]))
            {
                string seg = s[start..(i)];
                if (seg.Length > 0) yield return seg;
                start = i;
            }
        }
        string tail = s[start..];
        if (tail.Length > 0) yield return tail;
    }

    // ── Stopwords (articles, prepositions, auxiliaries) ──────────────────────

    private static readonly HashSet<string> Stopwords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a","an","the","of","in","on","at","by","for","with","about","from","to",
        "into","through","is","are","was","were","be","been","being",
        "have","has","had","do","does","did","can","could","will","would","shall","should",
        "may","might","must","need","used","let","get","got","i","we","it","its",
        "and","or","but","not","no","as","if","so","then","else","this","that",
    };

    public static bool IsStopword(string w) => Stopwords.Contains(w);

    // ── Dictionary-free stemmer (suffix stripping, E15) ──────────────────────

    public static string Stem(string word)
    {
        // Ordered by specificity: longest suffixes first
        string[] suffixes = ["isation","ization","nesses","ments","tions","ings",
                             "ness","ment","tion","ing","ful","ous","ive","ed","er","ly"];
        foreach (var s in suffixes)
            if (word.Length > s.Length + 3 && word.EndsWith(s, StringComparison.OrdinalIgnoreCase))
                return word[..^s.Length];
        return word;
    }

    // ── Synonym collapse to semantic ops (E16) ────────────────────────────────

    private static readonly Dictionary<string, string> SynonymMap =
        new(StringComparer.OrdinalIgnoreCase)
    {
        // Retrieval ops
        ["get"]      = "GET_OP", ["fetch"]  = "GET_OP", ["find"]   = "GET_OP",
        ["load"]     = "GET_OP", ["query"]  = "GET_OP", ["read"]   = "GET_OP",
        ["select"]   = "GET_OP", ["lookup"] = "GET_OP", ["search"] = "GET_OP",
        ["retriev"]  = "GET_OP", // stemmed form of "retrieve"

        // Validation ops
        ["valid"]    = "VALIDATE_OP", ["check"]  = "VALIDATE_OP",
        ["verif"]    = "VALIDATE_OP", ["assert"] = "VALIDATE_OP",
        ["confirm"]  = "VALIDATE_OP", ["ensur"]  = "VALIDATE_OP",

        // Creation ops
        ["creat"]    = "CREATE_OP", ["make"]    = "CREATE_OP",
        ["build"]    = "CREATE_OP", ["construct"]= "CREATE_OP",
        ["new"]      = "CREATE_OP", ["init"]    = "CREATE_OP",
        ["initialis"]= "CREATE_OP", ["generat"] = "CREATE_OP",

        // Update ops
        ["updat"]    = "UPDATE_OP", ["set"]     = "UPDATE_OP",
        ["write"]    = "UPDATE_OP", ["save"]    = "UPDATE_OP",
        ["store"]    = "UPDATE_OP", ["put"]     = "UPDATE_OP",
        ["persist"]  = "UPDATE_OP", ["modifi"]  = "UPDATE_OP",

        // Delete ops
        ["delet"]    = "DELETE_OP", ["remov"]   = "DELETE_OP",
        ["clear"]    = "DELETE_OP", ["clean"]   = "DELETE_OP",
        ["reset"]    = "DELETE_OP", ["purge"]   = "DELETE_OP",

        // Process/transform ops
        ["process"]  = "PROCESS_OP", ["handl"]  = "PROCESS_OP",
        ["execut"]   = "PROCESS_OP", ["run"]    = "PROCESS_OP",
        ["perform"]  = "PROCESS_OP", ["appli"]  = "PROCESS_OP",

        // Convert/parse ops
        ["convert"]  = "CONVERT_OP", ["pars"]   = "CONVERT_OP",
        ["transform"]= "CONVERT_OP", ["format"] = "CONVERT_OP",
        ["serial"]   = "CONVERT_OP", ["deserial"]= "CONVERT_OP",

        // Send/publish ops
        ["send"]     = "SEND_OP", ["publish"] = "SEND_OP",
        ["emit"]     = "SEND_OP", ["dispatch"]= "SEND_OP",
        ["notifi"]   = "SEND_OP", ["post"]    = "SEND_OP",

        // Log/report ops
        ["log"]      = "LOG_OP", ["trace"]   = "LOG_OP",
        ["debug"]    = "LOG_OP", ["report"]  = "LOG_OP",
        ["record"]   = "LOG_OP", ["track"]   = "LOG_OP",

        // Auth/security ops
        ["auth"]     = "AUTH_OP", ["login"]   = "AUTH_OP",
        ["token"]    = "AUTH_OP", ["permiss"] = "AUTH_OP",
        ["authoris"] = "AUTH_OP", ["access"]  = "AUTH_OP",

        // NL verbs for natural-language prompt analysis (E21)
        ["add"]       = "CREATE_OP", ["implement"] = "CREATE_OP",
        ["inject"]    = "CREATE_OP", ["introduc"]  = "CREATE_OP",
        ["fix"]       = "PROCESS_OP", ["refactor"]  = "PROCESS_OP",
        ["extract"]   = "PROCESS_OP", ["migrat"]    = "PROCESS_OP",
        ["test"]      = "VALIDATE_OP", ["unit"]     = "VALIDATE_OP",
        ["explain"]   = "GET_OP",    ["show"]      = "GET_OP",
        ["list"]      = "GET_OP",    ["what"]      = "GET_OP",
        ["why"]       = "GET_OP",
        ["send"]      = "SEND_OP",   ["emit"]      = "SEND_OP",
    };

    public static string CollapseToOp(string stemmed)
    {
        if (SynonymMap.TryGetValue(stemmed, out string? op)) return op;
        // Prefix match: the stemmer may leave suffix chars ("validat" starts with "valid").
        // Pick the longest matching key to avoid short-key false positives.
        string? bestKey = null;
        foreach (var key in SynonymMap.Keys)
            if (stemmed.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                if (bestKey is null || key.Length > bestKey.Length)
                    bestKey = key;
        return bestKey is not null ? SynonymMap[bestKey] : stemmed;
    }

    // ── Logical predicates from AST roles (E17, 12 fixed) ────────────────────

    // Maps a Roslyn node to one of 12 logical predicates (or null = skip).
    internal static string? ToLogicalPredicate(Microsoft.CodeAnalysis.SyntaxNode node)
    {
        return node switch
        {
            // NULL_GUARD: if-condition checks for null
            IfStatementSyntax ifStmt when ifStmt.Condition.DescendantNodesAndSelf()
                .Any(n => n.Kind() == SyntaxKind.NullLiteralExpression ||
                          n.Kind() == SyntaxKind.IsPatternExpression)
                => "NULL_GUARD",

            // FAIL: return with null, false, or -1
            ReturnStatementSyntax ret when ret.Expression != null &&
                (ret.Expression.Kind() == SyntaxKind.NullLiteralExpression ||
                 ret.Expression.Kind() == SyntaxKind.FalseLiteralExpression ||
                 ret.Expression.ToString().Trim() is "-1" or "0")
                => "FAIL",

            // ITERATE: any loop
            ForEachStatementSyntax  => "ITERATE",
            ForStatementSyntax      => "ITERATE",
            WhileStatementSyntax    => "ITERATE",

            // HANDLE_ERROR: try/catch
            TryStatementSyntax  => "HANDLE_ERROR",
            CatchClauseSyntax   => "HANDLE_ERROR",

            // AWAIT: async call
            AwaitExpressionSyntax => "AWAIT",

            // THROW: throw expression/statement
            ThrowStatementSyntax    => "THROW",
            ThrowExpressionSyntax   => "THROW",

            // DECLARE: local variable declaration
            LocalDeclarationStatementSyntax => "DECLARE",

            // ASSIGN: assignment
            AssignmentExpressionSyntax => "ASSIGN",

            // CREATE: object creation
            ObjectCreationExpressionSyntax => "CREATE",

            // CALL: method invocation (non-await)
            InvocationExpressionSyntax inv
                when inv.Parent is not AwaitExpressionSyntax => "CALL",

            // BRANCH: if without null check
            IfStatementSyntax => "BRANCH",

            // RETURN: plain return
            ReturnStatementSyntax => "RETURN",

            _ => null,
        };
    }
}
