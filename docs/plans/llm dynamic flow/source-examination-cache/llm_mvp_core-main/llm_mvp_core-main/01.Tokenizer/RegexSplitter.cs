using System.Text.RegularExpressions;

namespace Tokenizer;

/// <summary>
/// Pre-tokenises text into word-like chunks before BPE merges.
/// Pattern mirrors GPT-2's cl100k split: contractions, letters, digits, whitespace.
/// </summary>
public static class RegexSplitter
{
    private static readonly Regex Pattern = new(
        @"'s|'t|'re|'ve|'m|'ll|'d| ?\p{L}+| ?\p{N}+| ?[^\s\p{L}\p{N}]+|\s+(?!\S)|\s+",
        RegexOptions.Compiled);

    public static IEnumerable<string> Split(string text) =>
        Pattern.Matches(text).Select(m => m.Value);
}
