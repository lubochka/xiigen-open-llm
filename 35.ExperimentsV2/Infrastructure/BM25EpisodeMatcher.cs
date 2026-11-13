// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// BM25 matcher for episode retrieval.
// Operates on E15-normalized words — after lemmatization but BEFORE FamilyCollapser,
// so domain-specific words like "WordPress", "modules", "wireframes" survive.
public sealed class BM25EpisodeMatcher
{
    private readonly float _k1 = 1.5f;
    private readonly float _b  = 0.75f;

    private readonly Dictionary<string, Dictionary<int, int>> _index = new();
    private readonly Dictionary<int, int> _docLengths = new();
    private double _avgDocLen;

    public void Index(IReadOnlyList<(string Text, int EpisodeId)> examples)
    {
        _index.Clear(); _docLengths.Clear();
        foreach (var (text, epId) in examples)
        {
            var words = SemanticNormalizer.Normalize(text);
            _docLengths[epId] = _docLengths.GetValueOrDefault(epId, 0) + words.Count;
            foreach (var w in words)
            {
                if (!_index.TryGetValue(w, out var d)) _index[w] = d = new();
                d[epId] = d.GetValueOrDefault(epId, 0) + 1;
            }
        }
        _avgDocLen = _docLengths.Count > 0 ? _docLengths.Values.Average() : 1;
    }

    public List<(int EpisodeId, double Score)> Search(string query, int topK = 3)
    {
        var qWords = SemanticNormalizer.Normalize(query).ToHashSet();
        var scores = new Dictionary<int, double>();
        int N      = _docLengths.Count;
        if (N == 0) return [];

        foreach (var word in qWords)
        {
            if (!_index.TryGetValue(word, out var postings)) continue;
            int    df  = postings.Count;
            double idf = Math.Log((N - df + 0.5) / (df + 0.5) + 1.0);
            foreach (var (epId, tf) in postings)
            {
                int    docLen  = _docLengths.GetValueOrDefault(epId, 1);
                double tfNorm  = (tf * (_k1 + 1)) /
                    (tf + _k1 * (1 - _b + _b * docLen / _avgDocLen));
                scores[epId] = scores.GetValueOrDefault(epId, 0) + idf * tfNorm;
            }
        }
        return scores.OrderByDescending(kv => kv.Value)
            .Take(topK)
            .Select(kv => (kv.Key, kv.Value))
            .ToList();
    }
}
