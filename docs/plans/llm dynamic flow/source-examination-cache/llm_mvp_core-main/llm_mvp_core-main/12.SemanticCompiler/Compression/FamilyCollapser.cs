namespace SemanticCompiler;

// FamilyCollapser maps normalized words to semantic op families (GET_OP, CREATE_OP, …).
// E16: +8.4% perplexity reduction by collapsing synonyms to ~30 semantic ops.
// After E15 normalization — applied to the stemmed word list.
public static class FamilyCollapser
{
    // Collapse a list of stemmed words to op-family tokens
    public static List<string> Collapse(List<string> stemmedWords)
        => stemmedWords.Select(SemanticTokenizerPipeline.CollapseToOp).ToList();

    // Tokenize raw text directly into semantic op tags (skips AST — for NL prompts)
    public static List<string> CollapseString(string text)
        => text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
               .Select(w => w.Trim('.', ',', '!', '?', ';', ':', '"', '\'').ToLowerInvariant())
               .Where(w => w.Length >= 2 && !SemanticTokenizerPipeline.IsStopword(w))
               .Select(w => SemanticTokenizerPipeline.CollapseToOp(SemanticTokenizerPipeline.Stem(w)))
               .ToList();
}
