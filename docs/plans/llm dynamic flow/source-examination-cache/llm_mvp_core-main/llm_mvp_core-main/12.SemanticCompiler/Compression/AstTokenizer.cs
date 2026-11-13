namespace SemanticCompiler;

// AstTokenizer extracts identifier words from C# source using Roslyn's AST.
// E01: +93% perplexity reduction vs BPE (Byte Pair Encoding) tokenization.
// Three levels of compression available: Level0 (raw), Level1 (E15), Level2 (E16).
public static class AstTokenizer
{
    // Level 0: raw identifier words — baseline used to measure E15 improvement
    public static IReadOnlyList<string> Tokenize(string csharpSource)
    {
        if (string.IsNullOrWhiteSpace(csharpSource))
            return Array.Empty<string>();
        return SemanticTokenizerPipeline.Level0Tokens(csharpSource).AsReadOnly();
    }

    // Level 1: stopword removal + suffix stemming (E15)
    public static IReadOnlyList<string> TokenizeNormalized(string csharpSource)
    {
        if (string.IsNullOrWhiteSpace(csharpSource))
            return Array.Empty<string>();
        return SemanticTokenizerPipeline.Level1Tokens(csharpSource).AsReadOnly();
    }

    // Level 2: semantic op families (E16)
    public static IReadOnlyList<string> TokenizeSemanticOps(string csharpSource)
    {
        if (string.IsNullOrWhiteSpace(csharpSource))
            return Array.Empty<string>();
        return SemanticTokenizerPipeline.Level2Tokens(csharpSource).AsReadOnly();
    }

    // Level 3: logical predicate string tokens (E17) — use LogicExtractor for integers
    public static IReadOnlyList<string> TokenizePredicates(string csharpSource)
    {
        if (string.IsNullOrWhiteSpace(csharpSource))
            return Array.Empty<string>();
        return SemanticTokenizerPipeline.Level3Tokens(csharpSource).AsReadOnly();
    }
}
