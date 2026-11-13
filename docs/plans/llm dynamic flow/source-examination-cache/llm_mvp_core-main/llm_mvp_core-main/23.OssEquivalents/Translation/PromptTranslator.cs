namespace OssEquivalents;

// E25+E26+E27 translation pipeline: detects language → tags POS → translates with context → reorders to SVO.
// Applies E20 Scenario B: detect source language BEFORE any normalization.
public interface IPromptTranslator
{
    OperationResult<string> ToEnglish(string prompt);
    OperationResult<string> FromEnglish(string englishResponse, string targetLanguage);
}

public sealed class PromptTranslator : IPromptTranslator
{
    private readonly PatternClassifier  _classifier  = new();
    private readonly PatternReorderer   _reorderer   = new();
    private readonly TranslationDictionary _dict     = TranslationDictionary.BuildDefault();

    // E20: language detection from character ranges — run first, before any normalization.
    private static string DetectLanguage(string text)
    {
        int cyrillic = text.Count(c => c >= 0x0410 && c <= 0x044F);  // Russian
        int hebrew   = text.Count(c => c >= 0x05D0 && c <= 0x05EA);  // Hebrew
        int latin    = text.Count(char.IsAsciiLetter);

        if (cyrillic > latin && cyrillic > hebrew) return "Russian";
        if (hebrew > latin && hebrew > cyrillic)   return "Hebrew";
        return "English";
    }

    public OperationResult<string> ToEnglish(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return OperationResult<string>.Fail("Empty prompt.", "EMPTY");

        // Step 1 (E20): detect source language BEFORE any processing
        string srcLang = DetectLanguage(prompt);
        if (srcLang == "English")
            return OperationResult<string>.Ok(prompt.Trim());

        // Step 2: tokenize
        var tokens = prompt.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Step 3: assign POS tags
        var tagged = tokens.Select(t => new PatternReorderer.TaggedWord(t, AssignPos(t, srcLang))).ToList();

        // Step 4: classify sentence pattern (E25)
        var posTags = tagged.Select(w => w.PosTag).ToList();
        int pattern = _classifier.Classify(posTags);

        // Step 5: translate each word with context (E26)
        var translated = TranslateAll(tagged);

        // Step 6: reorder to English SVO (E27)
        var reordered = _reorderer.Reorder(translated, pattern);

        string result = string.Join(" ", reordered.Select(w => w.Text)).Trim();
        return OperationResult<string>.Ok(result);
    }

    public OperationResult<string> FromEnglish(string englishResponse, string targetLanguage)
        => OperationResult<string>.Ok(englishResponse); // translation back is out of scope for E25-27

    private List<PatternReorderer.TaggedWord> TranslateAll(List<PatternReorderer.TaggedWord> words)
    {
        var result = new List<PatternReorderer.TaggedWord>();
        int i = 0;
        while (i < words.Count)
        {
            var w = words[i];

            // ASCII/Latin words keep their text (already English technical terms like "null")
            if (IsAscii(w.Text))
            {
                result.Add(w);
                i++;
                continue;
            }

            // Build context arrays from neighbors
            int[] ctxBefore = i > 0              ? new[] { words[i - 1].PosTag } : Array.Empty<int>();
            int[] ctxAfter  = i < words.Count - 1 ? new[] { words[i + 1].PosTag } : Array.Empty<int>();

            string translation = _dict.TranslateWithContext(w.Text, ctxBefore, ctxAfter);

            // Compound noun: if translation ends with a plain noun AND next word is an English
            // technical term, prepend it to form English compound ("check" + "null" → "null check")
            if (!IsAscii(w.Text) && i + 1 < words.Count && IsAscii(words[i + 1].Text)
                && !translation.Contains(' '))
            {
                string nextWord = words[i + 1].Text;
                translation = nextWord + " " + translation;
                i++; // consume the English word (it's now part of the compound)
            }

            result.Add(new PatternReorderer.TaggedWord(translation, w.PosTag));
            i++;
        }
        return result;
    }

    // Assign POS tag from character set and known endings.
    private static int AssignPos(string word, string language)
    {
        if (IsAscii(word)) return PosTags.WORD; // English technical token

        return language switch
        {
            "Russian" => AssignRussianPos(word),
            "Hebrew"  => AssignHebrewPos(word),
            _         => PosTags.NOUN,
        };
    }

    private static int AssignRussianPos(string w)
    {
        string lower = w.ToLowerInvariant();
        // Imperative/infinitive: ends in й, ть, ти, ь following a consonant pattern
        if (lower.EndsWith("й") || lower.EndsWith("ть") || lower.EndsWith("ти"))
            return PosTags.VERB;
        // Soft sign after common verb stems (добавь, проверь, верни)
        if (lower.EndsWith("ь") && lower.Length > 3)
            return PosTags.VERB;
        return PosTags.NOUN;
    }

    private static int AssignHebrewPos(string w)
    {
        // Common Hebrew imperative verb endings (ף, ב, ל imperatives)
        // Simple heuristic: short words (≤4 chars) tend to be verbs in imperatives
        return w.Length <= 4 ? PosTags.VERB : PosTags.NOUN;
    }

    private static bool IsAscii(string word)
        => word.All(c => c < 128);
}
