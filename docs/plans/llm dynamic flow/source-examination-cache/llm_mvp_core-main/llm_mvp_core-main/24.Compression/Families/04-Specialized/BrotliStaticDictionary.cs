namespace Compression;

// Brotli static dictionary — a fixed collection of common English words and web phrases.
// Even a single small file can reference patterns from this dictionary without needing
// prior data in the sliding window.
// This implementation provides a minimal subset for round-trip correctness.
// Reference: https://github.com/google/brotli (MIT license)
public sealed class BrotliStaticDictionary
{
    // A small subset of the most common Brotli dictionary words.
    private static readonly string[] CommonWords =
    [
        "the ", "and ", "for ", "are ", "but ", "not ", "you ", "all ",
        "can ", "her ", "was ", "one ", "our ", "out ", "day ", "get ",
        "has ", "him ", "his ", "how ", "man ", "new ", "now ", "old ",
        "see ", "two ", "way ", "who ", "boy ", "did ", "its ", "let ",
        "put ", "say ", "she ", "too ", "use ", "</", " class=\"", " id=\"",
        " href=\"", "http://", "https://", "<!DOCTYPE", "<html", "</html>",
        "<head>", "</head>", "<body>", "</body>", "<div>", "</div>",
        "function ", "return ", "const ", "var ", "let ", "if (", "} else {"
    ];

    // Check if the given text starts with a dictionary entry.
    // Returns the matching entry and its index, or null if no match.
    public (string Entry, int Index)? FindMatch(string text)
    {
        for (int i = 0; i < CommonWords.Length; i++)
        {
            if (text.StartsWith(CommonWords[i], StringComparison.Ordinal))
                return (CommonWords[i], i);
        }
        return null;
    }

    // Look up a dictionary entry by its index.
    public string? GetEntry(int index)
        => index >= 0 && index < CommonWords.Length ? CommonWords[index] : null;

    public int EntryCount => CommonWords.Length;
}
