// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SearchEngine;

// Mutable in-memory postings list for a single term.
public sealed class PostingsList
{
    private readonly Dictionary<int, (int freq, List<int> positions)> _docs = [];

    public void Add(int docId, int position)
    {
        if (!_docs.TryGetValue(docId, out var entry))
        {
            entry = (0, []);
            _docs[docId] = entry;
        }
        var (freq, positions) = _docs[docId];
        positions.Add(position);
        _docs[docId] = (freq + 1, positions);
    }

    public IReadOnlyList<Posting> ToPostings()
        => _docs.Select(kv => new Posting(kv.Key, kv.Value.freq, kv.Value.positions))
                .OrderBy(p => p.DocId)
                .ToList();
}
