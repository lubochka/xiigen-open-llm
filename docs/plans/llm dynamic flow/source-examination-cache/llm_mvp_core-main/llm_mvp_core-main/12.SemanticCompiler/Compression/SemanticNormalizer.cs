using System.Text.RegularExpressions;

namespace SemanticCompiler;

// SemanticNormalizer converts plain-text prompts to normalized word lists.
// E15: +9.7% perplexity reduction via stopword removal + suffix stemming.
// Input: English prose (not C# identifiers).
// Output: stemmed word list without stopwords, length >= 2.
public static class SemanticNormalizer
{
    private static readonly Regex SplitPattern = new(@"[^a-zA-Z]+", RegexOptions.Compiled);

    public static List<string> Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];
        return SplitPattern.Split(text.ToLowerInvariant())
            .Where(w => w.Length >= 2)
            .Where(w => !SemanticTokenizerPipeline.IsStopword(w))
            .Select(SemanticTokenizerPipeline.Stem)
            .Where(w => w.Length >= 2)
            .ToList();
    }
}
