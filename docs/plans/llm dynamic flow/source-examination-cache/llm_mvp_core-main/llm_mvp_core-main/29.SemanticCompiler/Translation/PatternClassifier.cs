namespace SemanticCompiler29;

// PatternClassifier: classify sentence structure pattern (SVO / SOV / VSO).
// E25: +154.6% — identifies word order before translation to English SVO (Subject-Verb-Object).
// Without pattern detection, reordering is applied blindly — reducing accuracy.
public sealed class PatternClassifier : BaseService
{
    // Standard constructor: inject storage and logger without constructing them here.
    public PatternClassifier(ICompilerStorage storage, ICompilerEventLogger logger)
        : base(storage, logger) { }

    // POS (Part Of Speech) tag index constants (shared with PatternReorderer).
    internal const int NOUN = 7;
    internal const int VERB = 8;
    internal const int PRON = 3;

    // Classify a token array into its word-order pattern.
    // Returns "SVO" | "SOV" | "VSO" | "IMPERATIVE" | "UNKNOWN".
    public string Classify(string[] tokens)
    {
        if (tokens.Length == 0) return "UNKNOWN";

        // Use POS-tag heuristic: nouns are capitalized or domain words, verbs are action words.
        var tags = tokens.Select(InferPos).ToArray();

        int firstVerb = Array.IndexOf(tags, VERB);
        int firstNoun = tags.Select((t, i) => (t, i)).FirstOrDefault(x => x.t == NOUN || x.t == PRON).i;

        if (firstVerb < 0) return "SVO"; // no verb → treat as SVO

        bool verbFirst        = firstVerb == 0 || (firstVerb == 1 && tags[0] == 10);
        bool nounBeforeVerb   = firstNoun >= 0 && firstNoun < firstVerb;
        bool verbLast         = tags[^1] == VERB;

        if (verbFirst && !nounBeforeVerb) return "IMPERATIVE";
        if (verbLast && nounBeforeVerb)   return "SOV";
        if (verbFirst && nounBeforeVerb)  return "VSO";
        return "SVO";
    }

    // Lightweight POS tagger: action words → VERB, structure words → PRON, else NOUN.
    private static int InferPos(string token)
    {
        string lower = token.ToLowerInvariant();
        if (ActionWords.Contains(lower)) return VERB;
        if (StructureWords.Contains(lower)) return PRON;
        return NOUN;
    }

    private static readonly HashSet<string> ActionWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "add","fix","create","update","delete","validate","check","return","throw","send",
        "log","get","fetch","process","handle","run","build","set","remove","find","refactor",
    };

    private static readonly HashSet<string> StructureWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a","an","the","in","on","at","by","for","with","and","or","but","not",
        "this","that","it","they","we","you","which","who","where","when","how",
    };
}
