// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// PatternReorderer rearranges words from a source word-order pattern into SVO.
// Pure rule table — no ML needed when the pattern is correctly identified.
public static class PatternReorderer
{
    private static readonly Dictionary<int, Func<List<TaggedWord>, List<TaggedWord>>>
        Transformations = new()
    {
        // SVO → SVO: no change
        [SentencePatternClassifier.SVO] = words => words,

        // SOV → SVO: move the verb to immediately after the subject
        [SentencePatternClassifier.SOV] = words =>
        {
            var result  = words.ToList();
            int verbIdx = result.FindIndex(w => w.PosTag == 8);
            if (verbIdx > 1)
            {
                var verb    = result[verbIdx];
                result.RemoveAt(verbIdx);
                int subjEnd = result.FindIndex(w => w.PosTag == 7 || w.PosTag == 3) + 1;
                result.Insert(Math.Max(1, subjEnd), verb);
            }
            return result;
        },

        // VSO → SVO: move the subject before the verb
        [SentencePatternClassifier.VSO] = words =>
        {
            var result  = words.ToList();
            // FindIndex(1,...) starts after the leading verb
            int subjIdx = result.FindIndex(1, w => w.PosTag == 7 || w.PosTag == 3);
            if (subjIdx > 0) // > 0 so index 1 (direct VSO) is also moved
            {
                var subj = result[subjIdx];
                result.RemoveAt(subjIdx);
                result.Insert(0, subj);
            }
            return result;
        },

        // IMPERATIVE_V: no change (English imperatives are also verb-first)
        [SentencePatternClassifier.IMPERATIVE_V] = words => words,

        // IMPERATIVE_VS → IMPERATIVE_V: remove the explicit subject pronoun
        [SentencePatternClassifier.IMPERATIVE_VS] = words =>
            words.Where(w => w.PosTag != 3).ToList(),

        // TOPIC_OV → SVO: move the fronted topic to end
        [SentencePatternClassifier.TOPIC_OV] = words =>
        {
            var result  = words.ToList();
            int verbIdx = result.FindIndex(w => w.PosTag == 8);
            if (verbIdx > 0)
            {
                var topic = result.Take(verbIdx).ToList();
                var rest  = result.Skip(verbIdx).ToList();
                return rest.Concat(topic).ToList();
            }
            return result;
        },
    };

    // Reorder words from sourcePattern into SVO-compatible order
    public static List<TaggedWord> Reorder(List<TaggedWord> words, int sourcePattern)
    {
        if (!Transformations.TryGetValue(sourcePattern, out var transform))
            return words;
        return transform(words);
    }

    // True when the word list has subject (or no subject at all) before the verb
    public static bool IsSVO(List<TaggedWord> words)
    {
        int firstVerb = words.FindIndex(w => w.PosTag == 8);
        int firstNoun = words.FindIndex(w => w.PosTag == 7 || w.PosTag == 3);
        if (firstVerb == -1 || firstNoun == -1) return true;
        return firstNoun < firstVerb;
    }
}

// Word with its POS tag
public record TaggedWord(string Text, int PosTag);
