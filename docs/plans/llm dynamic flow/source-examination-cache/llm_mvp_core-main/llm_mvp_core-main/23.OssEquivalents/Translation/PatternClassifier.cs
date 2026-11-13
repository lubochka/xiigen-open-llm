namespace OssEquivalents;

// Sentence pattern constants — mirrors E25 SentencePatternClassifier but rule-based only.
public static class SentencePatterns
{
    public const int SVO        = 0;
    public const int SOV        = 1;
    public const int VSO        = 2;
    public const int IMPERATIVE_V  = 3;
    public const int TOPIC_OV   = 4;
}

// POS tag constants matching the tokenizer convention used by E25–E27.
internal static class PosTags
{
    public const int NOUN = 7, VERB = 8, ADJ = 9, ADV = 10, WORD = 11, PRON = 3;
}

// Rule-based sentence pattern classifier (E25 result: K-NN improves +154% over rules baseline,
// but rules are sufficient for the translation pipeline's supported patterns).
public sealed class PatternClassifier
{
    public int Classify(IReadOnlyList<int> posTags)
    {
        if (posTags.Count == 0) return SentencePatterns.SVO;

        int firstVerb = IndexOf(posTags, PosTags.VERB);
        int firstNoun = IndexOf(posTags, PosTags.NOUN);

        // Starts with VERB and no subject before it → IMPERATIVE_V
        if (posTags[0] == PosTags.VERB)
            return SentencePatterns.IMPERATIVE_V;

        if (firstVerb < 0 || firstNoun < 0) return SentencePatterns.SVO;

        // Subject (NOUN/PRON) before VERB → SVO or SOV
        if (firstNoun < firstVerb)
        {
            // Look for object after verb
            int objIdx = IndexOfAfter(posTags, PosTags.NOUN, firstVerb);
            return objIdx >= 0 ? SentencePatterns.SOV : SentencePatterns.SVO;
        }

        // VERB before NOUN → VSO
        return SentencePatterns.VSO;
    }

    private static int IndexOf(IReadOnlyList<int> list, int value)
    {
        for (int i = 0; i < list.Count; i++)
            if (list[i] == value) return i;
        return -1;
    }

    private static int IndexOfAfter(IReadOnlyList<int> list, int value, int afterIdx)
    {
        for (int i = afterIdx + 1; i < list.Count; i++)
            if (list[i] == value) return i;
        return -1;
    }
}
