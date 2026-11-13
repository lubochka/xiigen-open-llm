using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SemanticCompiler;

// Three-level semantic compression pipeline (E15→E16→E17).
//   Level 0: raw identifier words — camelCase split only (baseline for E15)
//   Level 1: + stopword removal + dictionary-free stemming (E15 +9.7%)
//   Level 2: + synonym collapse to ~30 semantic ops (E16 +8.4%)
//   Level 3: + logical predicates from AST roles, 12 fixed (E17 +97%)
public static class SemanticTokenizerPipeline
{
    // ── Level 0: raw identifier words ────────────────────────────────────────

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
        var root   = CSharpSyntaxTree.ParseText(csharpSource).GetRoot();
        var result = new List<string>();
        foreach (var node in root.DescendantNodes())
        {
            string? pred = ToLogicalPredicate(node);
            if (pred != null) result.Add(pred);
        }
        // Fall back to Level2 when AST yields too few nodes (e.g. expression snippets)
        return result.Count >= 4 ? result : Level2Tokens(csharpSource);
    }

    // ── Vocabulary encoding ───────────────────────────────────────────────────

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

    // ── Stopwords ─────────────────────────────────────────────────────────────

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
        ["get"]="GET_OP",["fetch"]="GET_OP",["find"]="GET_OP",["load"]="GET_OP",
        ["query"]="GET_OP",["read"]="GET_OP",["select"]="GET_OP",["lookup"]="GET_OP",
        ["search"]="GET_OP",["retriev"]="GET_OP",["explain"]="GET_OP",["show"]="GET_OP",
        ["list"]="GET_OP",["what"]="GET_OP",["why"]="GET_OP",
        ["valid"]="VALIDATE_OP",["check"]="VALIDATE_OP",["verif"]="VALIDATE_OP",
        ["assert"]="VALIDATE_OP",["confirm"]="VALIDATE_OP",["ensur"]="VALIDATE_OP",
        ["test"]="VALIDATE_OP",["unit"]="VALIDATE_OP",
        ["creat"]="CREATE_OP",["make"]="CREATE_OP",["build"]="CREATE_OP",
        ["construct"]="CREATE_OP",["new"]="CREATE_OP",["init"]="CREATE_OP",
        ["initialis"]="CREATE_OP",["generat"]="CREATE_OP",["add"]="CREATE_OP",
        ["implement"]="CREATE_OP",["inject"]="CREATE_OP",["introduc"]="CREATE_OP",
        ["updat"]="UPDATE_OP",["set"]="UPDATE_OP",["write"]="UPDATE_OP",
        ["save"]="UPDATE_OP",["store"]="UPDATE_OP",["put"]="UPDATE_OP",
        ["persist"]="UPDATE_OP",["modifi"]="UPDATE_OP",
        ["delet"]="DELETE_OP",["remov"]="DELETE_OP",["clear"]="DELETE_OP",
        ["clean"]="DELETE_OP",["reset"]="DELETE_OP",["purge"]="DELETE_OP",
        ["process"]="PROCESS_OP",["handl"]="PROCESS_OP",["execut"]="PROCESS_OP",
        ["run"]="PROCESS_OP",["perform"]="PROCESS_OP",["appli"]="PROCESS_OP",
        ["fix"]="PROCESS_OP",["refactor"]="PROCESS_OP",["extract"]="PROCESS_OP",
        ["migrat"]="PROCESS_OP",
        ["convert"]="CONVERT_OP",["pars"]="CONVERT_OP",["transform"]="CONVERT_OP",
        ["format"]="CONVERT_OP",["serial"]="CONVERT_OP",["deserial"]="CONVERT_OP",
        ["send"]="SEND_OP",["publish"]="SEND_OP",["emit"]="SEND_OP",
        ["dispatch"]="SEND_OP",["notifi"]="SEND_OP",["post"]="SEND_OP",
        ["log"]="LOG_OP",["trace"]="LOG_OP",["debug"]="LOG_OP",
        ["report"]="LOG_OP",["record"]="LOG_OP",["track"]="LOG_OP",
        ["auth"]="AUTH_OP",["login"]="AUTH_OP",["token"]="AUTH_OP",
        ["permiss"]="AUTH_OP",["authoris"]="AUTH_OP",["access"]="AUTH_OP",
    };

    public static string CollapseToOp(string stemmed)
    {
        if (SynonymMap.TryGetValue(stemmed, out string? op)) return op;
        string? bestKey = null;
        foreach (var key in SynonymMap.Keys)
            if (stemmed.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                if (bestKey is null || key.Length > bestKey.Length)
                    bestKey = key;
        return bestKey is not null ? SynonymMap[bestKey] : stemmed;
    }

    // ── Logical predicates from AST roles (E17, 12 fixed) ────────────────────

    internal static string? ToLogicalPredicate(Microsoft.CodeAnalysis.SyntaxNode node)
    {
        return node switch
        {
            IfStatementSyntax ifStmt when ifStmt.Condition.DescendantNodesAndSelf()
                .Any(n => n.Kind() == SyntaxKind.NullLiteralExpression ||
                          n.Kind() == SyntaxKind.IsPatternExpression)
                => "NULL_GUARD",
            ReturnStatementSyntax ret when ret.Expression != null &&
                (ret.Expression.Kind() == SyntaxKind.NullLiteralExpression ||
                 ret.Expression.Kind() == SyntaxKind.FalseLiteralExpression ||
                 ret.Expression.ToString().Trim() is "-1" or "0")
                => "FAIL",
            ForEachStatementSyntax  => "ITERATE",
            ForStatementSyntax      => "ITERATE",
            WhileStatementSyntax    => "ITERATE",
            TryStatementSyntax      => "HANDLE_ERROR",
            CatchClauseSyntax       => "HANDLE_ERROR",
            AwaitExpressionSyntax   => "AWAIT",
            ThrowStatementSyntax    => "THROW",
            ThrowExpressionSyntax   => "THROW",
            LocalDeclarationStatementSyntax => "DECLARE",
            AssignmentExpressionSyntax      => "ASSIGN",
            ObjectCreationExpressionSyntax  => "CREATE",
            InvocationExpressionSyntax inv
                when inv.Parent is not AwaitExpressionSyntax => "CALL",
            IfStatementSyntax => "BRANCH",
            ReturnStatementSyntax => "RETURN",
            _ => null,
        };
    }
}
