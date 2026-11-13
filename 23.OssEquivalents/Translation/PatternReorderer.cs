// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace OssEquivalents;

// E27 +250%: exact-match reordering of sentence patterns to English SVO.
// Rule table covers SOV→SVO (move V after S), VSO→SVO (move S before V), etc.
public sealed class PatternReorderer
{
    public record TaggedWord(string Text, int PosTag);

    private static readonly Dictionary<int, Func<List<TaggedWord>, List<TaggedWord>>> Transformations = new()
    {
        // SVO — already correct English order
        [SentencePatterns.SVO] = words => words,

        // SOV → SVO: move verb after subject+object chain
        [SentencePatterns.SOV] = words =>
        {
            var result = words.ToList();
            int verbIdx = result.FindIndex(w => w.PosTag == PosTags.VERB);
            if (verbIdx > 0)
            {
                var verb = result[verbIdx];
                result.RemoveAt(verbIdx);
                // Insert verb after first noun (subject position)
                int nounIdx = result.FindIndex(w => w.PosTag == PosTags.NOUN);
                result.Insert(nounIdx >= 0 ? nounIdx + 1 : 0, verb);
            }
            return result;
        },

        // VSO → SVO: move subject before verb
        [SentencePatterns.VSO] = words =>
        {
            var result  = words.ToList();
            int subjIdx = result.FindIndex(1, w => w.PosTag == PosTags.NOUN || w.PosTag == PosTags.PRON);
            if (subjIdx > 0)
            {
                var subj = result[subjIdx];
                result.RemoveAt(subjIdx);
                result.Insert(0, subj);
            }
            return result;
        },

        // IMPERATIVE_V — V already first, object follows, correct for English
        [SentencePatterns.IMPERATIVE_V] = words => words,

        // TOPIC_OV → SVO: move topic-object after verb
        [SentencePatterns.TOPIC_OV] = words =>
        {
            var result   = words.ToList();
            int verbIdx  = result.FindIndex(w => w.PosTag == PosTags.VERB);
            if (verbIdx <= 0) return result;
            // Words before verb = topic/object → move to end
            var topic = result[..verbIdx];
            var rest  = result[verbIdx..];
            return [..rest, ..topic];
        },
    };

    public List<TaggedWord> Reorder(List<TaggedWord> words, int pattern)
    {
        if (Transformations.TryGetValue(pattern, out var fn))
            return fn(words);
        return words;
    }

    // True if words are already in SVO order (first NOUN before first VERB).
    public static bool IsSVO(List<TaggedWord> words)
    {
        int firstVerb = words.FindIndex(w => w.PosTag == PosTags.VERB);
        int firstNoun = words.FindIndex(w => w.PosTag == PosTags.NOUN);
        return firstNoun >= 0 && firstVerb >= 0 && firstNoun < firstVerb;
    }
}
