namespace SemanticCompiler29;

// StackDetector: detect programming language from raw text.
// CRITICAL (E20 Rule A): must run on RAW text, BEFORE any normalisation.
// TypeScript marker "Promise<" becomes "promise<" after normalisation — undetectable.
// C# markers (using/async/await) survive normalisation — order matters for TS/PY only.
//
// C# structural markers (added after genietasks 90%→100% fix):
//   "{ get; set; }", "public class ", "= string.Empty"
//   — lean EF (Entity Framework) model classes have no using/namespace/async
//     but do have these property accessors.
public sealed class StackDetector : BaseService, IStackDetector29
{
    // Minimum marker count required to declare a language (avoids false positives).
    private const int MinMarkers = 2;

    // Standard constructor: inject storage and logger without constructing them here.
    public StackDetector(ICompilerStorage storage, ICompilerEventLogger logger)
        : base(storage, logger) { }

    // TypeScript-specific markers — many are destroyed by lowercasing normalisation.
    private static readonly string[] TypeScriptMarkers =
    [
        "Promise<", "Observable<", "Injectable", "NgModule", "@Component",
        "interface ", "export const", "export function", "export async ",
        "export class", "readonly ", ": string", ": number", ": boolean",
        "?.","??", "async (", "=> {", "import {", "from '", "from \"",
        "(): ", // return-type annotation e.g. fetch(): Promise<string>
    ];

    // C# structural markers — survive normalisation, used for lean EF model classes.
    private static readonly string[] CSharpStructuralMarkers =
    [
        "{ get; set; }", "{ get; }", "public class ", "= string.Empty",
        "namespace ", "using System", "public static ", "private readonly ",
        "async Task", "Task<", "IEnumerable<", "List<", "Dictionary<",
        "[Required]", "[Key]", "DbSet<",
    ];

    // Python-specific markers.
    private static readonly string[] PythonMarkers =
    [
        "def ", "self.", "import ", "from ", "class ", "__init__",
        "None", "True", "False", "elif ", "lambda ", "yield ",
        "async def", "await ", "list[", "dict[", "tuple[",
    ];

    // C# keyword markers — survive normalisation.
    private static readonly string[] CSharpKeywordMarkers =
    [
        "using ", "namespace ", "async ", "await ", "var ", "typeof(",
        "nameof(", "is null", "is not null", "switch {", "=> ",
    ];

    // Detect the stack label from raw (un-normalised) source text.
    // Returns "csharp" | "typescript" | "python" | "unknown".
    public string Detect(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText)) return "unknown";

        int tsScore = CountMatches(rawText, TypeScriptMarkers);
        int pyScore = CountMatches(rawText, PythonMarkers);
        int csScore = CountMatches(rawText, CSharpKeywordMarkers)
                    + CountMatches(rawText, CSharpStructuralMarkers);

        // TypeScript check before normalisation — its markers are case-sensitive.
        if (tsScore >= MinMarkers && tsScore > csScore && tsScore > pyScore)
            return "typescript";

        if (pyScore >= MinMarkers && pyScore > csScore)
            return "python";

        if (csScore >= MinMarkers)
            return "csharp";

        return "unknown";
    }

    // Count how many markers from the set appear in the raw text.
    private static int CountMatches(string text, string[] markers)
        => markers.Count(m => text.Contains(m, StringComparison.Ordinal));
}
