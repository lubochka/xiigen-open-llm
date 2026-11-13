namespace ExperimentsV2;

// FamilyCollapser maps normalized words to semantic op families (GET_OP, CREATE_OP, …).
// Wraps SemanticTokenizerPipeline.CollapseToOp for use in E21 PromptSplitter.
public static class FamilyCollapser
{
    public static List<string> Collapse(List<string> words)
        => words.Select(SemanticTokenizerPipeline.CollapseToOp).ToList();

    // Tokenize plain-text (not C# source) into semantic op tags for E32-E37.
    public static List<string> CollapseString(string text)
        => text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
               .Select(w => w.Trim('.', ',', '!', '?', ';', ':', '"', '\'').ToLowerInvariant())
               .Where(w => w.Length >= 2 && !SemanticTokenizerPipeline.IsStopword(w))
               .Select(w => SemanticTokenizerPipeline.CollapseToOp(SemanticTokenizerPipeline.Stem(w)))
               .ToList();
}
