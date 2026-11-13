// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DPO;

// FIX-LC transfer, plan 13-dpo-reference-cache.md, from LC-REF-CACHE.
//
// The frozen reference model gives the same two numbers for the same pair every
// time it is asked, because it never changes. Re-running it for a pair the
// trainer has already scored spends a full forward pass to recompute a constant.
// This holds the last few of those answers.
//
// What it is NOT: it never chooses chosen/rejected, never judges a pair, never
// substitutes for admission, and never lets a hit skip a validation. It stores
// two floats, nothing else — no tensors, no graph, no policy state.
internal sealed class DpoReferenceScoreCache
{
    // Everything that could make the two numbers different. If any part differs,
    // it is a different question and the reference has to answer it again.
    internal sealed class Key
    {
        internal Key(
            string digestSha256,
            string tokenizerSha256,
            int tokenizerVocabSize,
            string materialVersion,
            string framingVersion,
            string objectiveVersion,
            string referenceIdentity,
            // FIX-SC: версия входа — какой именно вход получала модель, когда эти два
            // числа были получены. Ответ, снятый на полном входе, не выдаётся за ответ
            // на сдвинутом префиксе, даже если материал, словарь и эталон те же.
            string inputVersion)
        {
            DigestSha256 = digestSha256;
            TokenizerSha256 = tokenizerSha256;
            TokenizerVocabSize = tokenizerVocabSize;
            MaterialVersion = materialVersion;
            FramingVersion = framingVersion;
            ObjectiveVersion = objectiveVersion;
            ReferenceIdentity = referenceIdentity;
            InputVersion = inputVersion;
        }

        internal string DigestSha256 { get; }

        internal string TokenizerSha256 { get; }

        internal int TokenizerVocabSize { get; }

        internal string MaterialVersion { get; }

        internal string FramingVersion { get; }

        internal string ObjectiveVersion { get; }

        internal string ReferenceIdentity { get; }

        /// <summary>FIX-SC: вход, на котором эти два числа были получены.</summary>
        internal string InputVersion { get; }

        // Exact ordinal comparison of the already verified digest and versions.
        // No hash code shortcut: with a capacity of two a direct comparison is
        // both cheaper and impossible to confuse with a collision.
        internal bool Same(Key other)
            => string.Equals(DigestSha256, other.DigestSha256, StringComparison.Ordinal)
                && string.Equals(TokenizerSha256, other.TokenizerSha256, StringComparison.Ordinal)
                && TokenizerVocabSize == other.TokenizerVocabSize
                && string.Equals(MaterialVersion, other.MaterialVersion, StringComparison.Ordinal)
                && string.Equals(FramingVersion, other.FramingVersion, StringComparison.Ordinal)
                && string.Equals(ObjectiveVersion, other.ObjectiveVersion, StringComparison.Ordinal)
                && string.Equals(ReferenceIdentity, other.ReferenceIdentity, StringComparison.Ordinal)
                // FIX-SC: другой вход — другой вопрос, и эталон отвечает на него заново.
                && string.Equals(InputVersion, other.InputVersion, StringComparison.Ordinal);
    }

    private readonly List<(Key Key, float Chosen, float Rejected)> _entries = [];
    private readonly int _capacity;

    internal DpoReferenceScoreCache(int capacity)
    {
        // Capacity zero means the cache is off: every lookup misses and nothing
        // is ever stored. That is the comparison arm, not a broken cache.
        _capacity = Math.Max(0, capacity);
    }

    internal int Capacity => _capacity;

    internal int Lookups { get; private set; }

    internal int Hits { get; private set; }

    internal int Misses { get; private set; }

    internal int Stores { get; private set; }

    internal int Count => _entries.Count;

    internal bool TryGet(Key key, out float chosen, out float rejected)
    {
        chosen = 0f;
        rejected = 0f;
        if (_capacity == 0)
            return false;

        Lookups++;
        foreach (var entry in _entries)
        {
            if (!entry.Key.Same(key))
                continue;

            // A hit does not move the entry: the eviction order stays the order
            // of insertion, so behaviour does not depend on the read pattern.
            chosen = entry.Chosen;
            rejected = entry.Rejected;
            Hits++;
            return true;
        }

        Misses++;
        return false;
    }

    // Called only once BOTH reference scores are in hand. A failure between the
    // two leaves no half-written entry.
    internal void Store(Key key, float chosen, float rejected)
    {
        if (_capacity == 0)
            return;

        for (var index = 0; index < _entries.Count; index++)
        {
            if (!_entries[index].Key.Same(key))
                continue;

            _entries[index] = (key, chosen, rejected);
            Stores++;
            return;
        }

        // Oldest insertion leaves first.
        if (_entries.Count >= _capacity)
            _entries.RemoveAt(0);

        _entries.Add((key, chosen, rejected));
        Stores++;
    }

    // Forget everything. Used when a step is rolled back or a continuation is
    // restored: after that the trainer is not the same trainer any more.
    internal void Clear() => _entries.Clear();
}
