namespace SemanticCompiler29;

// ContextualDictionary: word → meaning given surrounding context.
// E26: +150.0% — "null" in C# vs "null" in SQL have different semantic meanings.
// Context is a small window of neighbouring tokens, not the full document.
public sealed class ContextualDictionary : BaseService
{
    // Standard constructor: inject storage and logger without constructing them here.
    public ContextualDictionary(ICompilerStorage storage, ICompilerEventLogger logger)
        : base(storage, logger) { }

    // Context window: how many tokens on each side to examine.
    private const int WindowSize = 2;

    // Ambiguous word → (context signal → preferred meaning).
    private static readonly Dictionary<string, Dictionary<string, string>> AmbiguityMap =
        new(StringComparer.OrdinalIgnoreCase)
    {
        ["null"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["SELECT"]    = "NULL (SQL)",       // SQL context: NULL is a SQL value
            ["WHERE"]     = "NULL (SQL)",
            ["csharp"]    = "null (C#)",        // C# context: null reference
            ["typescript"]= "null (TypeScript)", // TS context: null or undefined
            ["python"]    = "None",             // Python: None, not null
            ["default"]   = "null",
        },
        ["record"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["log"]    = "log entry",           // logging context: a log record
            ["class"]  = "C# record type",      // C# context: record class
            ["default"]= "record",
        },
        ["request"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["http"]     = "HTTP request",      // HTTP context
            ["validate"] = "validate request",  // validation context
            ["default"]  = "request",
        },
    };

    // Look up the best meaning of a word given its surrounding context tokens.
    public string Lookup(string word, string[] context)
    {
        if (!AmbiguityMap.TryGetValue(word, out var meanings))
            return word; // not ambiguous — return as-is

        // Search the context window for a matching signal.
        var window = context.Take(WindowSize * 2).ToArray();
        foreach (var token in window)
            if (meanings.TryGetValue(token, out string? meaning))
                return meaning;

        // Return the default meaning when no context signal is found.
        return meanings.TryGetValue("default", out string? dflt) ? dflt : word;
    }
}
