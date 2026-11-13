// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// ContextualDictionary selects the correct word translation based on
// neighboring POS tags, resolving ambiguity without a neural network.
public sealed class ContextualDictionary
{
    public record DictionaryEntry(
        string SourceWord,    // source word (normalised stem)
        int[]  ContextBefore, // POS tags to the left (1-2 words)
        int[]  ContextAfter,  // POS tags to the right (1-2 words)
        string Translation,   // correct translation for this context
        int    Priority);     // tiebreak when distance ties (higher = preferred)

    private static readonly int[] Empty = Array.Empty<int>();

    private readonly List<DictionaryEntry> _entries = new();
    private readonly int _k;

    public ContextualDictionary(int k = 3) => _k = k;

    public void AddEntry(DictionaryEntry entry) => _entries.Add(entry);

    // Build default dictionary for common technical Russian words
    public static ContextualDictionary BuildDefault()
    {
        var dict = new ContextualDictionary(k: 3);
        // POS indices: DET=0, PREP=1, CONJ=2, PRON=3, AUX=4, PUNCT=5, NUM=6
        //              NOUN=7, VERB=8, ADJ=9, ADV=10, WORD=11

        // "запис" (записать/запись) — log vs write vs record
        dict.AddEntry(new("запис", new[]{8},    new[]{7},    "log",        10));
        dict.AddEntry(new("запис", new[]{8},    new[]{11},   "write",       8));
        dict.AddEntry(new("запис", new[]{7},    new[]{7},    "record",      7));
        // "обработ" (обработка) — processing vs handling
        dict.AddEntry(new("обработ", new[]{7,9}, Empty,      "processing", 10));
        dict.AddEntry(new("обработ", Empty,      new[]{7},   "handling",    9));
        // "поток" — thread vs flow vs stream
        dict.AddEntry(new("поток", new[]{8},    Empty,       "thread",     10));
        dict.AddEntry(new("поток", Empty,        new[]{7},   "flow",        9));
        dict.AddEntry(new("поток", new[]{9},    Empty,       "stream",      8));
        // "запрос" — request vs query vs prompt
        dict.AddEntry(new("запрос", Empty,       new[]{11},  "request",    10));
        dict.AddEntry(new("запрос", Empty,       new[]{7},   "query",       9));
        dict.AddEntry(new("запрос", new[]{7},    Empty,      "prompt",      8));
        // "проверк" (проверку/проверка) — check vs validation
        dict.AddEntry(new("проверк", new[]{8},   Empty,      "check",      10));
        dict.AddEntry(new("проверк", Empty,      new[]{7,9}, "validation",  9));
        // "верн" (вернуть) — return vs restore
        dict.AddEntry(new("верн",   Empty,      new[]{7,3},  "return",     10));
        dict.AddEntry(new("верн",   Empty,      new[]{9,7},  "restore",     9));
        // Fallback entries (no context → first-entry priority)
        dict.AddEntry(new("запис",   Empty, Empty, "write",   1));
        dict.AddEntry(new("обработ", Empty, Empty, "process", 1));
        dict.AddEntry(new("поток",   Empty, Empty, "flow",    1));
        dict.AddEntry(new("запрос",  Empty, Empty, "request", 1));
        dict.AddEntry(new("проверк", Empty, Empty, "check",   1));
        dict.AddEntry(new("верн",    Empty, Empty, "return",  1));
        return dict;
    }

    // Enhanced: find best translation given neighboring POS context
    public string TranslateWithContext(
        string sourceWord,
        int[] contextBefore,
        int[] contextAfter)
    {
        string stem = Stem(sourceWord);
        var candidates = _entries.Where(e =>
            stem.StartsWith(e.SourceWord, StringComparison.OrdinalIgnoreCase) ||
            e.SourceWord.StartsWith(stem, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (candidates.Count == 0) return sourceWord;

        return candidates
            .OrderBy(e => ContextDistance(contextBefore, contextAfter,
                                          e.ContextBefore, e.ContextAfter))
            .ThenByDescending(e => e.Priority)
            .First().Translation;
    }

    // Baseline: highest-priority entry regardless of context
    public string TranslateBaseline(string sourceWord)
    {
        string stem = Stem(sourceWord);
        return _entries
            .Where(e => stem.StartsWith(e.SourceWord, StringComparison.OrdinalIgnoreCase) ||
                        e.SourceWord.StartsWith(stem, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(e => e.Priority)
            .FirstOrDefault()?.Translation ?? sourceWord;
    }

    private static double ContextDistance(
        int[] lb, int[] la, int[] rb, int[] ra)
    {
        double dist = 0;
        // Before-context match
        if (rb.Length > 0 && lb.Length > 0)
            dist += lb.Any(x => rb.Contains(x)) ? 0 : 1.5;
        else if (rb.Length == 0 && lb.Length > 0)
            dist += 0.5; // entry has no before-context but query does — small penalty
        // After-context match
        if (ra.Length > 0 && la.Length > 0)
            dist += la.Any(x => ra.Contains(x)) ? 0 : 1.5;
        else if (ra.Length == 0 && la.Length > 0)
            dist += 0.5; // entry has no after-context but query does — small penalty
        if (rb.Length == 0 && ra.Length == 0) dist += 1.0; // fallback entry penalty
        return dist;
    }

    private static string Stem(string word) =>
        word.Length > 5 ? word[..5].ToLowerInvariant() : word.ToLowerInvariant();
}
