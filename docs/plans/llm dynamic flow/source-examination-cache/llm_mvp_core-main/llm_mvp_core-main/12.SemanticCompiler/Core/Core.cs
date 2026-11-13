namespace SemanticCompiler;

// ─── Result envelope ─────────────────────────────────────────────────────────

public sealed class OperationResult<T>
{
    public bool   IsSuccess    { get; }
    public T?     Value        { get; }
    public string ErrorMessage { get; }
    public string ErrorCode    { get; }

    private OperationResult(bool ok, T? v, string msg, string code)
        => (IsSuccess, Value, ErrorMessage, ErrorCode) = (ok, v, msg, code);

    public static OperationResult<T> Ok(T value)
        => new(true, value, "", "");
    public static OperationResult<T> Fail(string message, string code = "ERROR")
        => new(false, default, message, code);
}

// ─── Domain and stack labels (D-04 canonical names) ──────────────────────────

// Five domains from D-04 DECISIONS-LOCKED.md.
// Short names used in code; long names in comments only.
public enum DomainLabel
{
    CODE_Q,   // CodeQuestion — explain how code works
    BUG,      // BugFix — fix an error or unexpected behaviour
    PLAN,     // Planning — plan or orchestrate a sequence of steps
    ARCH,     // Architecture — design system structure
    REFACTOR, // Refactoring — improve code without changing behaviour
}

// Technology stack detected by StackDetector (E20).
public enum StackLabel { CSharp, TypeScript, Python, Generic }

// ─── Pipeline result ─────────────────────────────────────────────────────────

// Full result of running the SemanticCompiler pipeline on one input.
public sealed record CompilationResult(
    StackLabel        Stack,
    DomainLabel       Domain,
    IReadOnlyList<int> Predicates,       // E17 logical predicate sequence
    IReadOnlyList<string> Ops,           // E16 semantic op tokens
    IReadOnlyList<string> NormalizedWords,// E15 stemmed words
    IReadOnlyList<PromptSegment> Segments // E21 prompt segments
);

// One segment detected by the PromptSplitter (E21).
public sealed record PromptSegment(string Text, int WordStart, int WordEnd);

// Tagged word with POS (Part-Of-Speech) tag for translation (E25-E27).
public sealed record TaggedWord(string Text, int PosTag);

// ─── Main interface ───────────────────────────────────────────────────────────

// ISemanticCompiler is the production contract for the E01-E27 pipeline.
// Pipeline order (E20 CRITICAL): StackDetector → Domain → Normalize → Collapse → Extract.
public interface ISemanticCompiler
{
    // Detect technology stack from raw source (BEFORE normalization — E20 rule).
    OperationResult<StackLabel> DetectStack(string source);

    // Classify request domain from prompt text using predicate profile (E18).
    OperationResult<DomainLabel> ClassifyDomain(string prompt);

    // Remove stopwords + stem all identifier words from C# source (E15).
    OperationResult<IReadOnlyList<string>> NormalizeSource(string csharpSource);

    // Collapse stemmed words to semantic op families (E16).
    OperationResult<IReadOnlyList<string>> CollapseToOps(IReadOnlyList<string> stemmedWords);

    // Extract logical predicate sequence from C# source using Roslyn (E17).
    OperationResult<IReadOnlyList<int>> ExtractPredicates(string csharpSource);

    // Split a natural-language prompt into task segments (E21).
    OperationResult<IReadOnlyList<PromptSegment>> SplitPrompt(string prompt);

    // Classify the stop signal for a sub-task (E33).
    OperationResult<int> DetectStopSignal(string taskDescription, bool hasIterations, bool hasTechTerms);

    // Retrieve best matching code template for predicate sequence (E23).
    OperationResult<string> BeautifyPredicates(IReadOnlyList<int> predicates, StackLabel stack);

    // Run the full pipeline and return combined CompilationResult.
    OperationResult<CompilationResult> Compile(string source);
}
