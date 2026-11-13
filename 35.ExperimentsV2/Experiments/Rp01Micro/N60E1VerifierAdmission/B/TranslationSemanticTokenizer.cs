// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace N60E1.B;

using System.Text.RegularExpressions;

// Converts source text into the structural address used by the trainable
// translation units. It does not decide translations; it only computes POS,
// roles, concepts and KNN signatures from the source sentence.
public sealed partial class TranslationSemanticTokenizer
{
    private static readonly Dictionary<(string Stem, SemanticConcept Prev), SemanticConcept> E26Mappings = new()
    {
        { ("записа", SemanticConcept.ERROR_EXCEPTION), SemanticConcept.LOG_OP },
        { ("записа", SemanticConcept.DATA_ENTITY), SemanticConcept.WRITE_OP },
        { ("поток", SemanticConcept.LAUNCH_OP), SemanticConcept.THREAD_REF },
        { ("поток", SemanticConcept.PROCESSING_OF), SemanticConcept.THREAD_REF },
        { ("обработ", SemanticConcept.ERROR_EXCEPTION), SemanticConcept.HANDLE_OP },

        // Legacy mojibake keys kept so old tests and docs-authored examples still work.
        { ("Ð·Ð°Ð¿Ð¸ÑÐ°", SemanticConcept.ERROR_EXCEPTION), SemanticConcept.LOG_OP },
        { ("Ð·Ð°Ð¿Ð¸ÑÐ°", SemanticConcept.DATA_ENTITY), SemanticConcept.WRITE_OP },
        { ("Ð¿Ð¾Ñ‚Ð¾Ðº", SemanticConcept.LAUNCH_OP), SemanticConcept.THREAD_REF },
        { ("Ð¿Ð¾Ñ‚Ð¾Ðº", SemanticConcept.PROCESSING_OF), SemanticConcept.THREAD_REF },
        { ("Ð¾Ð±Ñ€Ð°Ð±Ð¾Ñ‚", SemanticConcept.ERROR_EXCEPTION), SemanticConcept.HANDLE_OP },
    };

    private static readonly Dictionary<string, SemanticConcept> BaseConceptsRu
        = new(StringComparer.OrdinalIgnoreCase)
    {
        { "добавит", SemanticConcept.ADD_OP },
        { "добавь", SemanticConcept.ADD_OP },
        { "проверк", SemanticConcept.VALIDATE_CHECK },
        { "проверит", SemanticConcept.VALIDATE_CHECK },
        { "null", SemanticConcept.NULL_LITERAL },
        { "метод", SemanticConcept.METHOD_REF },
        { "функци", SemanticConcept.METHOD_REF },
        { "ошибк", SemanticConcept.ERROR_EXCEPTION },
        { "исключен", SemanticConcept.ERROR_EXCEPTION },
        { "лог", SemanticConcept.LOG_OP },
        { "обработк", SemanticConcept.PROCESSING_OF },
        { "данны", SemanticConcept.DATA_ENTITY },
        { "файл", SemanticConcept.DATA_ENTITY },
        { "запустит", SemanticConcept.LAUNCH_OP },
        { "верн", SemanticConcept.RETURN_OP },
        { "поток", SemanticConcept.UNKNOWN },
        { "записа", SemanticConcept.UNKNOWN },
        { "обработ", SemanticConcept.UNKNOWN },

        // Legacy mojibake keys kept for existing unit tests.
        { "Ð´Ð¾Ð±Ð°Ð²Ð¸Ñ‚", SemanticConcept.ADD_OP },
        { "Ð´Ð¾Ð±Ð°Ð²ÑŒ", SemanticConcept.ADD_OP },
        { "Ð¿Ñ€Ð¾Ð²ÐµÑ€Ðº", SemanticConcept.VALIDATE_CHECK },
        { "Ð¿Ñ€Ð¾Ð²ÐµÑ€Ð¸Ñ‚", SemanticConcept.VALIDATE_CHECK },
        { "Ð¼ÐµÑ‚Ð¾Ð´", SemanticConcept.METHOD_REF },
        { "Ñ„ÑƒÐ½ÐºÑ†Ð¸", SemanticConcept.METHOD_REF },
        { "Ð¾ÑˆÐ¸Ð±Ðº", SemanticConcept.ERROR_EXCEPTION },
        { "Ð¸ÑÐºÐ»ÑŽÑ‡ÐµÐ½", SemanticConcept.ERROR_EXCEPTION },
        { "Ð»Ð¾Ð³", SemanticConcept.LOG_OP },
        { "Ð¾Ð±Ñ€Ð°Ð±Ð¾Ñ‚Ðº", SemanticConcept.PROCESSING_OF },
        { "Ð´Ð°Ð½Ð½Ñ‹", SemanticConcept.DATA_ENTITY },
        { "Ñ„Ð°Ð¹Ð»", SemanticConcept.DATA_ENTITY },
        { "Ð·Ð°Ð¿ÑƒÑÑ‚Ð¸Ñ‚", SemanticConcept.LAUNCH_OP },
        { "Ð²ÐµÑ€Ð½", SemanticConcept.RETURN_OP },
        { "Ð¿Ð¾Ñ‚Ð¾Ðº", SemanticConcept.UNKNOWN },
        { "Ð·Ð°Ð¿Ð¸ÑÐ°", SemanticConcept.UNKNOWN },
        { "Ð¾Ð±Ñ€Ð°Ð±Ð¾Ñ‚", SemanticConcept.UNKNOWN },
    };

    private static readonly HashSet<string> RuPrepositions
        = new(StringComparer.OrdinalIgnoreCase)
    {
        "в", "на", "из", "к", "у", "о", "с", "со", "по", "за", "от", "до", "при", "под", "над",
        "через", "перед", "после", "без", "для", "около", "посреди",
        "Ð²", "Ð½Ð°", "Ð¸Ð·", "Ðº", "Ñƒ", "Ð¾", "Ñ", "Ð¿Ð¾", "Ð·Ð°", "Ð¾Ñ‚", "Ð´Ð¾", "Ð¿Ñ€Ð¸", "Ð¿Ð¾Ð´", "Ð½Ð°Ð´"
    };

    private static readonly HashSet<string> RuPronouns
        = new(StringComparer.OrdinalIgnoreCase)
    {
        "я", "ты", "он", "она", "оно", "мы", "вы", "они", "его", "ее", "её", "ему", "ей", "их",
        "тот", "та", "те", "это", "котором", "который", "которая", "которые", "что",
        "Ð¾Ð½", "Ð¾Ð½Ð°", "Ñ", "Ð¼Ñ‹", "Ñ‚Ñ‹", "Ð¾Ð½Ð¸"
    };

    private static readonly HashSet<string> RuAdverbAllowList
        = new(StringComparer.OrdinalIgnoreCase)
    {
        "долго", "горестно", "нежно", "дальновидно", "напыщенно", "уныло",
        "раздражённо", "раздраженно", "дерзко", "покорно", "торжественно",
        "бесстрашно", "пронзительно", "злобно", "смиренно", "сметливо",
        "обидчиво", "важно", "смирно"
    };

    public SemanticTokenSequence Tokenize(string rawText, string sourceLang)
    {
        var words = rawText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var tokens = new List<SemanticToken>(words.Length);
        var prevConcept = SemanticConcept.UNKNOWN;

        foreach (var word in words)
        {
            var (pos, lemma) = ClassifyPos(word, sourceLang);
            var concept = ResolveConcept(lemma, pos, prevConcept, sourceLang);
            var role = DetermineRole(pos, tokens);

            tokens.Add(new SemanticToken(word, lemma, pos, role, concept));

            if (concept != SemanticConcept.UNKNOWN)
                prevConcept = concept;
        }

        var posSeq = tokens.Select(t => t.Pos).ToArray();
        var pattern = ClassifyPattern(posSeq);
        var ruleTags = BuildRuleTags(sourceLang, tokens);

        var bigrams = new List<(TranslationPos, TranslationPos)>(Math.Max(0, posSeq.Length - 1));
        for (var i = 0; i + 1 < posSeq.Length; i++)
            bigrams.Add((posSeq[i], posSeq[i + 1]));

        var sigConcepts = string.Join("|",
            tokens.Where(t => t.Concept != SemanticConcept.UNKNOWN)
                  .Select(t => t.Concept.ToString())
                  .Concat(ruleTags));
        var signature = string.IsNullOrEmpty(sigConcepts)
            ? pattern.ToString()
            : $"{pattern}|{sigConcepts}";

        var prefixConcepts = string.Join("|",
            tokens.Where(t => t.Concept != SemanticConcept.UNKNOWN)
                  .Take(2)
                  .Select(t => t.Concept.ToString())
                  .Concat(ruleTags.Take(3)));
        var prefixSignature = string.IsNullOrEmpty(prefixConcepts)
            ? pattern.ToString()
            : $"{pattern}|{prefixConcepts}";

        var structureSignature = $"{pattern}|{string.Join("|", tokens.Select(t => t.Pos.ToString()).Concat(ruleTags))}";

        return new SemanticTokenSequence(
            pattern,
            tokens,
            bigrams,
            signature,
            prefixSignature,
            structureSignature,
            ruleTags);
    }

    private static IReadOnlyList<string> BuildRuleTags(string sourceLang, IReadOnlyList<SemanticToken> tokens)
    {
        var tags = new List<string>();
        if (!string.Equals(sourceLang, "ru", StringComparison.OrdinalIgnoreCase))
            return tags;

        var firstVerbIndex = tokens.ToList().FindIndex(token => token.Pos == TranslationPos.VERB);
        var subject = firstVerbIndex > 0
            ? tokens.Take(firstVerbIndex).FirstOrDefault(token => token.Pos is TranslationPos.PRON or TranslationPos.NOUN)
            : tokens.FirstOrDefault(token => token.Pos is TranslationPos.PRON or TranslationPos.NOUN);
        var subjectGender = ResolveRussianSubjectGenderUtf8(subject) ?? ResolveRussianSubjectGender(subject);
        if (subjectGender is not null)
            AddTag($"RU:SUBJECT:{subjectGender}");

        foreach (var token in tokens)
        {
            if (token.Pos == TranslationPos.VERB)
            {
                AddTag("RU:TENSE:PAST");
                var verbGender = ResolveRussianPastVerbGenderUtf8(token) ?? ResolveRussianPastVerbGender(token);
                if (verbGender is not null)
                {
                    AddTag($"RU:VERB_GENDER:{verbGender}");
                    if (subjectGender is not null)
                        AddTag($"RU:AGREEMENT:SUBJECT_{subjectGender}_PAST_{verbGender}");
                }
            }

            if (token.Pos == TranslationPos.NOUN_GEN)
                AddTag("RU:CASE:GENITIVE");

            if (token.Pos == TranslationPos.PREP)
                foreach (var caseTag in ResolveRussianPrepositionCasesUtf8(token.Lemma).Concat(ResolveRussianPrepositionCases(token.Lemma)))
                    AddTag(caseTag);
        }

        return tags;

        void AddTag(string tag)
        {
            if (!tags.Contains(tag, StringComparer.Ordinal))
                tags.Add(tag);
        }
    }

    private static string? ResolveRussianSubjectGender(SemanticToken? token)
    {
        if (token is null)
            return null;

        var value = token.Word;
        if (value.Contains("Ð°", StringComparison.Ordinal) ||
            value.Contains("ÃÂ°", StringComparison.Ordinal) ||
            value.Contains("ÃƒÂÃ‚Â°", StringComparison.Ordinal))
            return "FEM";

        if (value.Contains("Ð½", StringComparison.Ordinal) ||
            value.Contains("ÃÂ½", StringComparison.Ordinal) ||
            value.Contains("ÃƒÂÃ‚Â½", StringComparison.Ordinal) ||
            value.Contains("Ñ‚", StringComparison.Ordinal) ||
            value.Contains("Ã‘â€š", StringComparison.Ordinal))
            return "MASC";

        return null;
    }

    private static string? ResolveRussianPastVerbGender(SemanticToken token)
    {
        var value = token.Word;
        if (LooksLikeFemininePastMojibakeVerb(value) ||
            value.EndsWith("Ð»Ð°", StringComparison.Ordinal) ||
            value.EndsWith("ÃÂ»ÃÂ°", StringComparison.Ordinal))
            return "FEM";

        if (LooksLikeMasculinePastMojibakeVerb(value) ||
            value.EndsWith("Ð»", StringComparison.Ordinal) ||
            value.EndsWith("ÃÂ»", StringComparison.Ordinal))
            return "MASC";

        if (value.EndsWith("Ð»Ð¸", StringComparison.Ordinal) ||
            value.EndsWith("ÃÂ»ÃÂ¸", StringComparison.Ordinal))
            return "PLUR";

        return null;
    }

    private static IReadOnlyList<string> ResolveRussianPrepositionCases(string lemma)
    {
        if (string.Equals(lemma, "Ðº", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(lemma, "ÃÂº", StringComparison.OrdinalIgnoreCase))
            return ["RU:CASE:DATIVE"];

        if (string.Equals(lemma, "Ð¸Ð·", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(lemma, "Ð¾Ñ‚", StringComparison.OrdinalIgnoreCase))
            return ["RU:CASE:GENITIVE"];

        if (string.Equals(lemma, "Ñ", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(lemma, "ÑÐ¾", StringComparison.OrdinalIgnoreCase))
            return ["RU:CASE:INSTRUMENTAL_OR_GENITIVE"];

        if (string.Equals(lemma, "Ð²", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(lemma, "Ð½Ð°", StringComparison.OrdinalIgnoreCase))
            return ["RU:CASE:ACCUSATIVE_OR_PREPOSITIONAL"];

        return [];
    }

    private static (TranslationPos Pos, string Lemma) ClassifyPos(string word, string lang)
    {
        var clean = CleanWord(word);
        var lower = clean.ToLowerInvariant();
        var comparable = LooksLikeUtf8Mojibake(clean) ? clean : lower;

        if (IsAsciiCodeIdentifier(clean))
            return (TranslationPos.CODE_IDENTIFIER, clean);

        if (lower is "null" or "true" or "false")
            return (TranslationPos.TECH_TERM, lower);

        if (lang == "ru")
        {
            if (IsRussianPrepositionUtf8(lower) || RuPrepositions.Contains(comparable) || RuPrepositions.Contains(lower))
                return (TranslationPos.PREP, comparable);

            if (IsRussianPronounUtf8(lower) || RuPronouns.Contains(comparable) || RuPronouns.Contains(lower) || LooksLikeMojibakeRuPronoun(comparable))
                return (TranslationPos.PRON, comparable);

            if (LooksLikeRussianAdverbUtf8(lower) || LooksLikeRussianAdverb(comparable) || LooksLikeRussianAdverb(lower))
                return (TranslationPos.ADV, comparable);

            if (LooksLikeRussianVerbUtf8(lower) || LooksLikeFemininePastMojibakeVerb(comparable) || LooksLikeMasculinePastMojibakeVerb(comparable) || LooksLikeRussianVerb(comparable) || LooksLikeRussianVerb(lower))
                return (TranslationPos.VERB, TrimFemininePastMojibakeSuffix(TrimVerbSuffix(comparable)));

            if (LooksLikeRussianGenitiveUtf8(lower) || LooksLikeRussianGenitive(comparable) || LooksLikeRussianGenitive(lower))
                return (TranslationPos.NOUN_GEN, comparable);

            return (TranslationPos.NOUN, comparable);
        }

        return (TranslationPos.NOUN, clean);
    }

    private static SemanticConcept ResolveConcept(
        string lemma,
        TranslationPos pos,
        SemanticConcept prevConcept,
        string lang)
    {
        if (pos == TranslationPos.CODE_IDENTIFIER) return SemanticConcept.CODE_IDENTIFIER;
        if (pos == TranslationPos.TECH_TERM && lemma == "null") return SemanticConcept.NULL_LITERAL;
        if (pos == TranslationPos.PREP || pos == TranslationPos.PRON || pos == TranslationPos.ADV)
            return SemanticConcept.UNKNOWN;

        if (lang == "ru")
        {
            var baseConcept = LookupBaseConcept(lemma);
            if (baseConcept == SemanticConcept.UNKNOWN)
            {
                var stem = lemma.Length > 6 ? lemma[..6] : lemma;
                if (E26Mappings.TryGetValue((stem, prevConcept), out var contextual))
                    return contextual;

                stem = lemma.Length > 5 ? lemma[..5] : lemma;
                if (E26Mappings.TryGetValue((stem, prevConcept), out contextual))
                    return contextual;
            }

            return baseConcept;
        }

        return SemanticConcept.UNKNOWN;
    }

    private static SemanticConcept LookupBaseConcept(string lemma)
    {
        foreach (var (key, concept) in BaseConceptsRu)
        {
            if (lemma.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                return concept;
        }

        return SemanticConcept.UNKNOWN;
    }

    private static string DetermineRole(TranslationPos pos, List<SemanticToken> prev)
    {
        if (pos == TranslationPos.PRON) return "SUBJECT";
        if (pos == TranslationPos.ADV) return "MANNER";
        if (pos == TranslationPos.VERB && prev.All(t => t.Pos != TranslationPos.PRON)) return "IMPERATIVE";
        if (pos == TranslationPos.CODE_IDENTIFIER) return "CODE_NAME";
        if (pos == TranslationPos.TECH_TERM) return "MODIFIER";
        if (pos == TranslationPos.PREP) return "PREP_MARKER";
        if ((pos == TranslationPos.NOUN || pos == TranslationPos.NOUN_GEN) && prev.Any(t => t.Role == "IMPERATIVE"))
            return "DIRECT_OBJ";

        return pos.ToString();
    }

    private static SentencePattern ClassifyPattern(TranslationPos[] seq)
    {
        if (seq.Length == 0) return SentencePattern.UNKNOWN;

        var firstVerb = Array.IndexOf(seq, TranslationPos.VERB);
        var hasPronBefore = firstVerb > 0 && seq[..firstVerb].Any(p => p == TranslationPos.PRON);

        if (firstVerb == 0 && !hasPronBefore) return SentencePattern.IMPERATIVE_V;
        if (hasPronBefore && firstVerb > 0 && seq[^1] == TranslationPos.VERB) return SentencePattern.SOV;
        if (firstVerb >= 0) return SentencePattern.SVO;
        return SentencePattern.SVO;
    }

    private static string TrimVerbSuffix(string value)
    {
        foreach (var suffix in new[] { "ться", "лась", "лся", "ала", "яла", "ила", "или", "ите", "ть", "ти", "ёт", "ет", "ит", "ут", "ют", "ел", "ал", "ял", "ил", "й", "л" })
            if (value.EndsWith(suffix, StringComparison.Ordinal) && value.Length > suffix.Length + 1)
                return value[..^suffix.Length];

        foreach (var suffix in new[] { "Ñ‚ÑŒ", "Ñ‚Ð¸", "Ð¸Ñ‚Ðµ", "Ñ‘Ñ‚", "ÐµÑ‚", "Ð¸Ñ‚", "Ð¹" })
            if (value.EndsWith(suffix, StringComparison.Ordinal) && value.Length > suffix.Length + 1)
                return value[..^suffix.Length];

        return value;
    }

    private static bool LooksLikeRussianVerb(string lower)
        => lower.EndsWith("ть", StringComparison.Ordinal) ||
           lower.EndsWith("ти", StringComparison.Ordinal) ||
           lower.EndsWith("йте", StringComparison.Ordinal) ||
           lower.EndsWith("ите", StringComparison.Ordinal) ||
           lower.EndsWith("ёт", StringComparison.Ordinal) ||
           lower.EndsWith("ет", StringComparison.Ordinal) ||
           lower.EndsWith("ит", StringComparison.Ordinal) ||
           lower.EndsWith("ут", StringComparison.Ordinal) ||
           lower.EndsWith("ют", StringComparison.Ordinal) ||
           lower.EndsWith("лся", StringComparison.Ordinal) ||
           lower.EndsWith("лась", StringComparison.Ordinal) ||
           (lower.EndsWith("ла", StringComparison.Ordinal) && lower.Length > 3) ||
           lower.EndsWith("али", StringComparison.Ordinal) ||
           lower.EndsWith("или", StringComparison.Ordinal) ||
           lower.EndsWith("ала", StringComparison.Ordinal) ||
           lower.EndsWith("яла", StringComparison.Ordinal) ||
           lower.EndsWith("ила", StringComparison.Ordinal) ||
           lower.EndsWith("ел", StringComparison.Ordinal) ||
           lower.EndsWith("ал", StringComparison.Ordinal) ||
           lower.EndsWith("ял", StringComparison.Ordinal) ||
           lower.EndsWith("ил", StringComparison.Ordinal) ||
           (lower.EndsWith("л", StringComparison.Ordinal) && lower.Length > 3) ||
           lower.EndsWith("Ñ‚ÑŒ", StringComparison.Ordinal) ||
           lower.EndsWith("Ð¸Ñ‚Ðµ", StringComparison.Ordinal);

    private static bool LooksLikeRussianGenitive(string lower)
        => lower.EndsWith("ки", StringComparison.Ordinal) ||
           lower.EndsWith("ых", StringComparison.Ordinal) ||
           lower.EndsWith("ий", StringComparison.Ordinal) ||
           lower.EndsWith("ей", StringComparison.Ordinal) ||
           lower.EndsWith("ого", StringComparison.Ordinal) ||
           lower.EndsWith("его", StringComparison.Ordinal) ||
           lower.EndsWith("ÐºÐ¸", StringComparison.Ordinal) ||
           lower.EndsWith("Ñ‹Ñ…", StringComparison.Ordinal);

    private static bool LooksLikeRussianAdverb(string lower)
    {
        if (RuAdverbAllowList.Contains(lower))
            return true;

        if (lower.Length < 4 || RuPronouns.Contains(lower) || RuPrepositions.Contains(lower))
            return false;

        return lower.EndsWith("но", StringComparison.Ordinal) ||
               lower.EndsWith("ко", StringComparison.Ordinal) ||
               lower.EndsWith("ло", StringComparison.Ordinal) ||
               lower.EndsWith("тельно", StringComparison.Ordinal) ||
               lower.EndsWith("щенно", StringComparison.Ordinal) ||
               lower.EndsWith("ённо", StringComparison.Ordinal) ||
               lower.EndsWith("енно", StringComparison.Ordinal) ||
               lower.EndsWith("Ð¾", StringComparison.Ordinal) ||
               lower.EndsWith("Ð½Ð¾", StringComparison.Ordinal);
    }

    private static string CleanWord(string word)
    {
        var value = word ?? string.Empty;
        if (LooksLikeUtf8Mojibake(value))
            return value.Trim().Trim('\'', '"', '`', '.', ',', '!', '?', ';', ':', '(', ')', '[', ']');

        return PunctuationRegex().Replace(value, string.Empty);
    }

    private static bool LooksLikeUtf8Mojibake(string value)
        => (value ?? string.Empty).Contains("Ð", StringComparison.Ordinal) ||
           (value ?? string.Empty).Contains("Ñ", StringComparison.Ordinal) ||
           (value ?? string.Empty).Contains("Ã", StringComparison.Ordinal);

    private static bool LooksLikeMojibakeRuPronoun(string value)
        => string.Equals(value, "ÃÅ¾ÃÂ½", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(value, "ÃÅ¾ÃÂ½ÃÂ°", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(value, "ÐžÐ½", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(value, "ÐžÐ½Ð°", StringComparison.OrdinalIgnoreCase);

    private static bool LooksLikeFemininePastMojibakeVerb(string value)
        => (value ?? string.Empty).EndsWith("Ð»Ð°", StringComparison.Ordinal) ||
           (value ?? string.Empty).EndsWith("ÃÂ»ÃÂ°", StringComparison.Ordinal);

    private static bool LooksLikeMasculinePastMojibakeVerb(string value)
        => ((value ?? string.Empty).EndsWith("ÃÂ»", StringComparison.Ordinal) ||
            (value ?? string.Empty).EndsWith("ÃƒÂÃ‚Â»", StringComparison.Ordinal) ||
            (value ?? string.Empty).EndsWith("»", StringComparison.Ordinal)) &&
           (value ?? string.Empty).Length > 4;

    private static string TrimFemininePastMojibakeSuffix(string value)
    {
        var safe = value ?? string.Empty;
        foreach (var suffix in new[] { "Ð»Ð°", "ÃÂ»ÃÂ°" })
            if (safe.EndsWith(suffix, StringComparison.Ordinal) && safe.Length > suffix.Length + 1)
                return safe[..^suffix.Length];

        return safe;
    }

    private static string? ResolveRussianSubjectGenderUtf8(SemanticToken? token)
    {
        if (token is null)
            return null;

        var value = (token.Word ?? string.Empty).ToLowerInvariant();
        var lemma = (token.Lemma ?? string.Empty).ToLowerInvariant();
        if (value is "она" or "та" || lemma is "она" or "та")
            return "FEM";

        if (value is "он" or "тот" || lemma is "он" or "тот")
            return "MASC";

        if (token.Pos == TranslationPos.NOUN && ContainsCyrillic(value))
        {
            if (value.EndsWith("а", StringComparison.Ordinal) ||
                value.EndsWith("я", StringComparison.Ordinal))
                return "FEM";

            return "MASC";
        }

        return null;
    }

    private static string? ResolveRussianPastVerbGenderUtf8(SemanticToken token)
    {
        var value = (token.Word ?? string.Empty).ToLowerInvariant();
        if (value.EndsWith("лась", StringComparison.Ordinal) ||
            value.EndsWith("ла", StringComparison.Ordinal))
            return "FEM";

        if (value.EndsWith("лись", StringComparison.Ordinal) ||
            value.EndsWith("ли", StringComparison.Ordinal))
            return "PLUR";

        if (value.EndsWith("лся", StringComparison.Ordinal) ||
            (value.EndsWith("л", StringComparison.Ordinal) && value.Length > 3))
            return "MASC";

        return null;
    }

    private static IReadOnlyList<string> ResolveRussianPrepositionCasesUtf8(string lemma)
    {
        if (string.Equals(lemma, "к", StringComparison.OrdinalIgnoreCase))
            return ["RU:CASE:DATIVE"];

        if (string.Equals(lemma, "из", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(lemma, "от", StringComparison.OrdinalIgnoreCase))
            return ["RU:CASE:GENITIVE"];

        if (string.Equals(lemma, "с", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(lemma, "со", StringComparison.OrdinalIgnoreCase))
            return ["RU:CASE:INSTRUMENTAL_OR_GENITIVE"];

        if (string.Equals(lemma, "в", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(lemma, "на", StringComparison.OrdinalIgnoreCase))
            return ["RU:CASE:ACCUSATIVE_OR_PREPOSITIONAL"];

        return [];
    }

    private static bool IsRussianPrepositionUtf8(string value)
        => value is "в" or "на" or "из" or "к" or "у" or "о" or "с" or "со" or "по" or "за" or "от" or "до" or "при" or "под" or "над"
            or "через" or "перед" or "после" or "без" or "для" or "около" or "посреди";

    private static bool IsRussianPronounUtf8(string value)
        => value is "я" or "ты" or "он" or "она" or "оно" or "мы" or "вы" or "они" or "его" or "ее" or "её" or "ему" or "ей" or "их"
            or "тот" or "та" or "те" or "это" or "котором" or "который" or "которая" or "которые" or "что";

    private static bool LooksLikeRussianVerbUtf8(string lower)
        => lower.EndsWith("ть", StringComparison.Ordinal) ||
           lower.EndsWith("ти", StringComparison.Ordinal) ||
           lower.EndsWith("йте", StringComparison.Ordinal) ||
           lower.EndsWith("ите", StringComparison.Ordinal) ||
           lower.EndsWith("ёт", StringComparison.Ordinal) ||
           lower.EndsWith("ет", StringComparison.Ordinal) ||
           lower.EndsWith("ит", StringComparison.Ordinal) ||
           lower.EndsWith("ут", StringComparison.Ordinal) ||
           lower.EndsWith("ют", StringComparison.Ordinal) ||
           lower.EndsWith("лся", StringComparison.Ordinal) ||
           lower.EndsWith("лась", StringComparison.Ordinal) ||
           lower.EndsWith("али", StringComparison.Ordinal) ||
           lower.EndsWith("или", StringComparison.Ordinal) ||
           lower.EndsWith("ала", StringComparison.Ordinal) ||
           lower.EndsWith("яла", StringComparison.Ordinal) ||
           lower.EndsWith("ила", StringComparison.Ordinal) ||
           lower.EndsWith("ел", StringComparison.Ordinal) ||
           lower.EndsWith("ал", StringComparison.Ordinal) ||
           lower.EndsWith("ял", StringComparison.Ordinal) ||
           lower.EndsWith("ил", StringComparison.Ordinal) ||
           (lower.EndsWith("л", StringComparison.Ordinal) && lower.Length > 3);

    private static bool LooksLikeRussianGenitiveUtf8(string lower)
        => lower.EndsWith("ки", StringComparison.Ordinal) ||
           lower.EndsWith("ых", StringComparison.Ordinal) ||
           lower.EndsWith("ий", StringComparison.Ordinal) ||
           lower.EndsWith("ей", StringComparison.Ordinal) ||
           lower.EndsWith("ого", StringComparison.Ordinal) ||
           lower.EndsWith("его", StringComparison.Ordinal);

    private static bool LooksLikeRussianAdverbUtf8(string lower)
    {
        if (lower.Length < 4 || IsRussianPronounUtf8(lower) || IsRussianPrepositionUtf8(lower))
            return false;

        return lower.EndsWith("но", StringComparison.Ordinal) ||
               lower.EndsWith("ко", StringComparison.Ordinal) ||
               lower.EndsWith("ло", StringComparison.Ordinal) ||
               lower.EndsWith("тельно", StringComparison.Ordinal) ||
               lower.EndsWith("щенно", StringComparison.Ordinal) ||
               lower.EndsWith("ённо", StringComparison.Ordinal) ||
               lower.EndsWith("енно", StringComparison.Ordinal);
    }

    private static bool ContainsCyrillic(string value)
        => (value ?? string.Empty).Any(ch => ch is >= '\u0400' and <= '\u04FF');

    private static bool IsAsciiCodeIdentifier(string word)
    {
        if (string.IsNullOrWhiteSpace(word) || word.Length <= 2)
            return false;

        if (!word.All(ch => ch < 128 && (char.IsLetterOrDigit(ch) || ch == '_' || ch == '$')))
            return false;

        if (word.Contains('_', StringComparison.Ordinal) || word.Contains('$', StringComparison.Ordinal))
            return true;

        if (word.Any(char.IsDigit) && word.Any(char.IsLetter))
            return true;

        var uppercaseCount = word.Count(char.IsUpper);
        var lowercaseCount = word.Count(char.IsLower);
        if (char.IsLower(word[0]) && uppercaseCount > 0)
            return true;

        return char.IsUpper(word[0]) && uppercaseCount > 1 && lowercaseCount > 0;
    }

    [GeneratedRegex(@"^[\p{P}\p{S}]+|[\p{P}\p{S}]+$")]
    private static partial Regex PunctuationRegex();
}
