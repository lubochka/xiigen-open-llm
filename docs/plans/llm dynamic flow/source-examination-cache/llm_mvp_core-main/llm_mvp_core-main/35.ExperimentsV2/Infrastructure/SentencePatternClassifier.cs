namespace ExperimentsV2;

// SentencePatternClassifier determines word-order pattern from POS tag sequences.
// Eight patterns cover 95% of technical prompts across languages.
public sealed class SentencePatternClassifier
{
    public const int SVO          = 0;
    public const int SOV          = 1;
    public const int VSO          = 2;
    public const int IMPERATIVE_V = 3;
    public const int IMPERATIVE_VS= 4;
    public const int QUESTION_SV  = 5;
    public const int QUESTION_VS  = 6;
    public const int TOPIC_OV     = 7;

    public static readonly string[] PatternNames =
    [
        "SVO","SOV","VSO","IMPERATIVE_V","IMPERATIVE_VS",
        "QUESTION_SV","QUESTION_VS","TOPIC_OV"
    ];

    // POS indices from EnglishRoleTagger
    // DET=0, PREP=1, CONJ=2, PRON=3, AUX=4, PUNCT=5, NUM=6
    // NOUN=7, VERB=8, ADJ=9, ADV=10, WORD=11
    private const int NOUN_ID = 7;
    private const int VERB_ID = 8;
    private const int PRON_ID = 3;

    private readonly List<(int[] PosSeq, int Pattern)> _train = new();
    private readonly int _k;

    public SentencePatternClassifier(int k = 3) => _k = k;

    public void Train(IReadOnlyList<(int[] PosSeq, int Pattern)> examples)
    {
        _train.Clear();
        _train.AddRange(examples);
    }

    // Enhanced: K-NN on POS bigrams (Jaccard distance)
    public int Predict(int[] posSequence)
    {
        if (_train.Count == 0) return IMPERATIVE_V;
        return _train
            .OrderBy(t => SequenceDistance(posSequence, t.PosSeq))
            .Take(_k)
            .GroupBy(t => t.Pattern)
            .OrderByDescending(g => g.Count())
            .First().Key;
    }

    // Baseline: rules based on first-verb position
    public static int PredictByRules(int[] posSeq)
    {
        if (posSeq.Length == 0) return IMPERATIVE_V;

        int firstVerb = Array.IndexOf(posSeq, VERB_ID);
        int firstNoun = posSeq.ToList().FindIndex(p => p == NOUN_ID || p == PRON_ID);

        if (firstVerb == -1) return SVO;

        bool startsWithVerb   = firstVerb == 0 || (firstVerb == 1 && posSeq[0] == 10);
        bool hasSubjectBefore = firstNoun >= 0 && firstNoun < firstVerb;
        bool endsWithVerb     = posSeq[^1] == VERB_ID;

        if (startsWithVerb && !hasSubjectBefore) return IMPERATIVE_V;
        if (endsWithVerb && hasSubjectBefore)    return SOV;
        if (startsWithVerb && hasSubjectBefore)  return VSO;
        return SVO;
    }

    // Jaccard distance on POS bigrams
    private static double SequenceDistance(int[] a, int[] b)
    {
        var bigramsA = new HashSet<(int, int)>();
        var bigramsB = new HashSet<(int, int)>();
        for (int i = 0; i < a.Length - 1; i++) bigramsA.Add((a[i], a[i + 1]));
        for (int i = 0; i < b.Length - 1; i++) bigramsB.Add((b[i], b[i + 1]));
        int inter = bigramsA.Count(x => bigramsB.Contains(x));
        int union = bigramsA.Count + bigramsB.Count - inter;
        return union == 0 ? 0 : 1.0 - (double)inter / union;
    }

    // Generate synthetic POS sequences for each pattern
    public static List<(int[] PosSeq, int Pattern)> GenerateExamples(int seed, int perPattern)
    {
        var rng    = new Random(seed);
        var result = new List<(int[], int)>();
        var templates = new Dictionary<int, int[][]>
        {
            [SVO]           = [[PRON_ID,VERB_ID,NOUN_ID], [NOUN_ID,VERB_ID,NOUN_ID,1,NOUN_ID]],
            [SOV]           = [[PRON_ID,NOUN_ID,VERB_ID], [NOUN_ID,NOUN_ID,1,NOUN_ID,VERB_ID]],
            [VSO]           = [[VERB_ID,PRON_ID,NOUN_ID], [VERB_ID,NOUN_ID,NOUN_ID,1,NOUN_ID]],
            [IMPERATIVE_V]  = [[VERB_ID,NOUN_ID],          [VERB_ID,NOUN_ID,1,NOUN_ID]],
            [IMPERATIVE_VS] = [[VERB_ID,PRON_ID,NOUN_ID,1,NOUN_ID]],
            [QUESTION_SV]   = [[PRON_ID,VERB_ID,4,NOUN_ID,5],[PRON_ID,VERB_ID,NOUN_ID,5]],
            [QUESTION_VS]   = [[VERB_ID,4,PRON_ID,NOUN_ID,5],[4,VERB_ID,PRON_ID,NOUN_ID,5]],
            [TOPIC_OV]      = [[NOUN_ID,VERB_ID],[NOUN_ID,1,NOUN_ID,VERB_ID]],
        };
        foreach (var (pattern, tmplList) in templates)
            for (int i = 0; i < perPattern; i++)
            {
                var tmpl = tmplList[rng.Next(tmplList.Length)];
                var seq  = tmpl.ToList();
                if (rng.Next(3) == 0) seq.Insert(rng.Next(seq.Count), 9); // ADJ variation
                result.Add((seq.ToArray(), pattern));
            }
        // Shuffle so 80/20 split gets a balanced mix of all patterns
        return result.OrderBy(_ => rng.Next()).ToList();
    }
}
