namespace OssEquivalents;

// E26 +150%: context-aware translation using K-NN on neighboring POS tags.
// Key fix: adds small penalty when an entry has no context but query does,
// preventing no-context fallback entries from winning over specific-context ones.
public sealed class TranslationDictionary
{
    public record Entry(
        string SourceWord,
        int[]  ContextBefore,
        int[]  ContextAfter,
        string Translation,
        int    Priority);

    private static readonly int[] Empty = Array.Empty<int>();

    private readonly List<Entry> _entries = new();

    public void Add(Entry entry) => _entries.Add(entry);

    // Highest-priority entry regardless of context (baseline).
    public string TranslateBaseline(string word)
    {
        string stem = Stem(word);
        return _entries
            .Where(e => Matches(stem, e.SourceWord))
            .OrderByDescending(e => e.Priority)
            .FirstOrDefault()?.Translation ?? word;
    }

    // Context-aware translation using POS-tag distance (E26).
    public string TranslateWithContext(string word, int[] contextBefore, int[] contextAfter)
    {
        string stem = Stem(word);
        var candidates = _entries.Where(e => Matches(stem, e.SourceWord)).ToList();
        if (candidates.Count == 0) return word;

        return candidates
            .OrderBy(e => Distance(contextBefore, contextAfter, e.ContextBefore, e.ContextAfter))
            .ThenByDescending(e => e.Priority)
            .First().Translation;
    }

    // Build the default dictionary for Russian technical words and imperative verbs.
    public static TranslationDictionary BuildDefault()
    {
        var d = new TranslationDictionary();

        // Russian verbs (imperative / infinitive stems)
        d.Add(new("добав", Empty, Empty, "Add",    20));  // добавь / добавить
        d.Add(new("найд",  Empty, Empty, "find",   20));  // найди / найти
        d.Add(new("верн",  Empty, new[]{3,7},  "return",  18));
        d.Add(new("верн",  Empty, new[]{9,7},  "restore", 17));
        d.Add(new("верн",  Empty, Empty, "return", 1));

        // "запис" — log vs write vs record
        d.Add(new("запис", new[]{8}, new[]{7},  "log",    10));
        d.Add(new("запис", new[]{8}, new[]{11}, "write",   8));
        d.Add(new("запис", new[]{7}, new[]{7},  "record",  7));
        d.Add(new("запис", Empty, Empty, "write", 1));

        // "поток" — thread vs flow vs stream (E26 key disambiguation)
        d.Add(new("поток", new[]{8},  Empty,      "thread", 10));
        d.Add(new("поток", Empty,     new[]{7},   "flow",    9));
        d.Add(new("поток", new[]{9},  Empty,      "stream",  8));
        d.Add(new("поток", Empty,     Empty,      "flow",    1));

        // "запрос" — request vs query vs prompt
        d.Add(new("запрос", Empty, new[]{11}, "request", 10));
        d.Add(new("запрос", Empty, new[]{7},  "query",    9));
        d.Add(new("запрос", new[]{7}, Empty,  "prompt",   8));
        d.Add(new("запрос", Empty, Empty, "request", 1));

        // "проверк" — check vs validation (noun forms: проверку, проверка)
        d.Add(new("проверк", new[]{8}, Empty,      "check",      10));
        d.Add(new("проверк", Empty,    new[]{9,7},  "validation",  9));
        d.Add(new("проверк", Empty,    Empty,       "check",       1));

        // "обработ" — processing vs handling
        d.Add(new("обработ", new[]{7,9}, Empty,    "processing", 10));
        d.Add(new("обработ", Empty,      new[]{7}, "handling",    9));
        d.Add(new("обработ", Empty, Empty, "processing", 1));

        // Hebrew verbs and nouns (compound forming handled by PromptTranslator, not here)
        d.Add(new("הוסף",  Empty, Empty, "Add",    20));  // הוסף = add (imperative)
        d.Add(new("בדיקת", Empty, Empty, "check",   9));  // בדיקת = check; "null check" built inline
        d.Add(new("שיטת",  Empty, Empty, "method",  9));
        d.Add(new("ערך",   Empty, Empty, "value",   9));

        return d;
    }

    private static bool Matches(string stem, string entryWord)
        => stem.StartsWith(entryWord, StringComparison.OrdinalIgnoreCase) ||
           entryWord.StartsWith(stem, StringComparison.OrdinalIgnoreCase);

    // E26 fix: penalise entries with no context when the query has context.
    private static double Distance(int[] lb, int[] la, int[] rb, int[] ra)
    {
        double dist = 0;
        if (rb.Length > 0 && lb.Length > 0)
            dist += lb.Any(x => rb.Contains(x)) ? 0 : 1.5;
        else if (rb.Length == 0 && lb.Length > 0)
            dist += 0.5;

        if (ra.Length > 0 && la.Length > 0)
            dist += la.Any(x => ra.Contains(x)) ? 0 : 1.5;
        else if (ra.Length == 0 && la.Length > 0)
            dist += 0.5;

        if (rb.Length == 0 && ra.Length == 0) dist += 1.0; // fallback entry penalty
        return dist;
    }

    private static string Stem(string word)
        => word.Length > 5 ? word[..5].ToLowerInvariant() : word.ToLowerInvariant();
}
