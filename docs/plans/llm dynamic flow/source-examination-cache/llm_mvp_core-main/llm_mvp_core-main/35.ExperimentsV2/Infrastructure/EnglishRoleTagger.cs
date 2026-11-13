namespace ExperimentsV2;

// Tags English words with one of 12 syntactic roles using word dictionaries + suffix rules.
// Pure C# — no Python or ML dependency.  Used by E08a (non-code syntax experiment).
//
// Roles: 0=UNKNOWN 1=ARTICLE 2=NOUN 3=VERB 4=ADJ 5=ADV
//        6=PREP    7=CONJ    8=PRON 9=PUNCT 10=NUM 11=PROPN
public static class EnglishRoleTagger
{
    public const int RoleCount = 12;

    private static readonly HashSet<string> Articles = new(StringComparer.OrdinalIgnoreCase)
        { "a", "an", "the" };

    private static readonly HashSet<string> Preps = new(StringComparer.OrdinalIgnoreCase)
        { "in", "on", "at", "by", "for", "with", "from", "to", "of", "about",
          "into", "through", "during", "before", "after", "above", "below",
          "between", "among", "under", "over", "upon", "within", "without",
          "against", "toward", "towards", "around", "along", "beside", "beyond" };

    private static readonly HashSet<string> Conjs = new(StringComparer.OrdinalIgnoreCase)
        { "and", "or", "but", "nor", "yet", "so", "because", "if", "when",
          "while", "although", "though", "since", "unless", "until", "whereas",
          "however", "therefore", "thus", "hence", "furthermore", "moreover" };

    private static readonly HashSet<string> Prons = new(StringComparer.OrdinalIgnoreCase)
        { "i", "me", "my", "myself", "you", "your", "yourself", "he", "him", "his",
          "himself", "she", "her", "hers", "herself", "it", "its", "itself",
          "we", "us", "our", "ourselves", "they", "them", "their", "themselves",
          "who", "whom", "whose", "which", "that", "this", "these", "those" };

    private static readonly HashSet<string> CommonVerbs = new(StringComparer.OrdinalIgnoreCase)
        { "is", "are", "was", "were", "be", "been", "being", "have", "has", "had",
          "do", "does", "did", "will", "would", "shall", "should", "may", "might",
          "must", "can", "could", "need", "dare", "ought", "get", "got",
          "make", "made", "take", "took", "go", "went", "come", "came",
          "see", "saw", "know", "knew", "think", "thought", "say", "said" };

    private static readonly HashSet<string> CommonAdjs = new(StringComparer.OrdinalIgnoreCase)
        { "good", "bad", "big", "small", "large", "little", "great", "old", "new",
          "high", "low", "long", "short", "right", "wrong", "true", "false",
          "early", "late", "first", "last", "many", "few", "more", "most", "less",
          "same", "other", "own", "free", "full", "open", "hard", "easy",
          "clear", "dark", "light", "black", "white", "red", "blue", "green" };

    // Tag a single token and return its role id.
    public static int Tag(string word)
    {
        if (string.IsNullOrEmpty(word)) return 0;

        if (word.Length == 1 && !char.IsLetterOrDigit(word[0])) return 9; // PUNCT
        if (char.IsDigit(word[0])) return 10; // NUM

        string clean = word.Trim('.', ',', ';', ':', '!', '?', '"', '\'', '(', ')');
        if (string.IsNullOrEmpty(clean)) return 9;

        if (Articles.Contains(clean)) return 1;
        if (Prons.Contains(clean))    return 8;
        if (Preps.Contains(clean))    return 6;
        if (Conjs.Contains(clean))    return 7;
        if (CommonVerbs.Contains(clean)) return 3;
        if (CommonAdjs.Contains(clean))  return 4;

        string lower = clean.ToLowerInvariant();

        if (lower.EndsWith("ly"))                                             return 5; // ADV
        if (lower.EndsWith("ing") || lower.EndsWith("ize") ||
            lower.EndsWith("ify") || lower.EndsWith("ate"))                   return 3; // VERB
        if (lower.EndsWith("tion") || lower.EndsWith("ness") ||
            lower.EndsWith("ment") || lower.EndsWith("ity") ||
            lower.EndsWith("ism")  || lower.EndsWith("ist"))                  return 2; // NOUN
        if (lower.EndsWith("ful") || lower.EndsWith("less") ||
            lower.EndsWith("ous") || lower.EndsWith("ive") ||
            lower.EndsWith("ic"))                                              return 4; // ADJ

        if (char.IsUpper(clean[0])) return 11; // PROPN (capitalised word)

        return 2; // default: NOUN
    }

    // Tokenise text and return a list of role ids for every token.
    public static List<int> TagText(string text)
    {
        var result = new List<int>();
        int i = 0;
        while (i < text.Length)
        {
            while (i < text.Length && char.IsWhiteSpace(text[i])) i++;
            if (i >= text.Length) break;

            int start = i;
            if (char.IsLetterOrDigit(text[i]) || text[i] == '\'')
            {
                while (i < text.Length &&
                       (char.IsLetterOrDigit(text[i]) || text[i] == '\'' || text[i] == '-'))
                    i++;
            }
            else
            {
                i++;
            }

            result.Add(Tag(text[start..i]));
        }
        return result;
    }
}
