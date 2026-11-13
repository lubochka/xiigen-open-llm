using System.Text.RegularExpressions;

namespace ExperimentsV2;

// SemanticNormalizer converts natural-language prompt text to a normalized
// word list suitable for predicate-profile analysis in E21 (PromptSplitter).
// Does NOT do camelCase splitting — input is English prose, not C# identifiers.
public static class SemanticNormalizer
{
    private static readonly Regex SplitPattern = new(@"[^a-zA-Z]+", RegexOptions.Compiled);

    public static List<string> Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return new List<string>();
        return SplitPattern.Split(text.ToLowerInvariant())
            .Where(w => w.Length >= 2)
            .Where(w => !SemanticTokenizerPipeline.IsStopword(w))
            .Select(SemanticTokenizerPipeline.Stem)
            .Where(w => w.Length >= 2)
            .ToList();
    }
}
