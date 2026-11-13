namespace ExperimentsV2;

// LogicExtractor converts C# source into a unified integer predicate sequence
// by combining Level3 AST predicates (0-11) and Level2 semantic ops (12-21).
// Used by E22 (DatasetRouter) and E23 (Beautifier) for K-NN on predicate space.
public static class LogicExtractor
{
    // ── Level3 AST predicates (0–11) ─────────────────────────────────────────
    public const int NULL_GUARD   = 0;
    public const int FAIL         = 1;
    public const int ITERATE      = 2;
    public const int HANDLE_ERROR = 3;
    public const int AWAIT        = 4;
    public const int THROW_OP     = 5;
    public const int DECLARE      = 6;
    public const int ASSIGN       = 7;
    public const int CREATE       = 8;
    public const int CALL         = 9;
    public const int BRANCH       = 10;
    public const int RETURN       = 11;
    public const int SUCCESS      = 11;  // alias — plain return = success path

    // Sentinel — not a valid predicate; acts as "distance-1 from everything" in K-NN
    public const int UNKNOWN     = -1;

    // ── Level2 semantic ops (12–21) ───────────────────────────────────────────
    public const int GET_CALL    = 12;
    public const int VALIDATE_OP = 13;
    public const int CREATE_OP   = 14;
    public const int UPDATE_OP   = 15;
    public const int DELETE_OP   = 16;
    public const int PROCESS_OP  = 17;
    public const int TRANSFORM   = 18;
    public const int SEND_OP     = 19;
    public const int LOG_OP      = 20;
    public const int AUTH_OP     = 21;

    private static readonly Dictionary<string, int> Map = new(StringComparer.Ordinal)
    {
        ["NULL_GUARD"]   = NULL_GUARD,   ["FAIL"]         = FAIL,
        ["ITERATE"]      = ITERATE,      ["HANDLE_ERROR"]  = HANDLE_ERROR,
        ["AWAIT"]        = AWAIT,        ["THROW"]         = THROW_OP,
        ["DECLARE"]      = DECLARE,      ["ASSIGN"]        = ASSIGN,
        ["CREATE"]       = CREATE,       ["CALL"]          = CALL,
        ["BRANCH"]       = BRANCH,       ["RETURN"]        = RETURN,
        ["GET_OP"]       = GET_CALL,     ["VALIDATE_OP"]   = VALIDATE_OP,
        ["CREATE_OP"]    = CREATE_OP,    ["UPDATE_OP"]     = UPDATE_OP,
        ["DELETE_OP"]    = DELETE_OP,    ["PROCESS_OP"]    = PROCESS_OP,
        ["CONVERT_OP"]   = TRANSFORM,    ["SEND_OP"]       = SEND_OP,
        ["LOG_OP"]       = LOG_OP,       ["AUTH_OP"]       = AUTH_OP,
    };

    // Extract integer predicate sequence from C# source.
    // Combines Level3 AST predicates + Level2 semantic ops for maximum signal.
    public static OperationResult<IReadOnlyList<int>> Extract(string csharpSource)
    {
        if (string.IsNullOrWhiteSpace(csharpSource))
            return OperationResult<IReadOnlyList<int>>.Fail("Empty source");

        var level3 = SemanticTokenizerPipeline.Level3Tokens(csharpSource);
        var level2ops = SemanticTokenizerPipeline.Level2Tokens(csharpSource)
            .Where(t => t.EndsWith("_OP", StringComparison.Ordinal));

        var result = level3.Concat(level2ops)
            .Select(s => Map.TryGetValue(s, out int id) ? id : -1)
            .Where(id => id >= 0)
            .ToList();

        // Text-level FAIL signal: fluent OperationResult.Fail() pattern not caught by Level3Tokens
        if (!result.Contains(FAIL) &&
            (csharpSource.Contains(".Fail(") || csharpSource.Contains("throw new ")))
            result.Add(FAIL);

        // Text-level NULL_GUARD: explicit null comparisons not always caught by Level3Tokens
        if (!result.Contains(NULL_GUARD) &&
            (csharpSource.Contains("== null") || csharpSource.Contains("!= null") ||
             csharpSource.Contains("is null")))
            result.Add(NULL_GUARD);

        if (result.Count == 0)
            return OperationResult<IReadOnlyList<int>>.Fail("No predicates extracted");

        return OperationResult<IReadOnlyList<int>>.Ok(result.AsReadOnly());
    }
}
