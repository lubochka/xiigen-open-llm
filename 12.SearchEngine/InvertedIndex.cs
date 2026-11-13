// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SearchEngine;

// Thread-safe in-memory inverted index.
public sealed class InvertedIndex
{
    private readonly object _lock = new();
    private readonly Dictionary<string, PostingsList>  _terms = [];
    private readonly List<DocumentInfo>                _docs  = [];
    private long _totalTerms;

    public int DocCount => _docs.Count;
    public float AvgDocLen => _docs.Count == 0 ? 0f : (float)_totalTerms / _docs.Count;

    public IReadOnlyList<DocumentInfo> Documents
    {
        get { lock (_lock) return _docs.AsReadOnly(); }
    }

    public int Add(string externalId, IReadOnlyList<AnalyzedToken> tokens)
    {
        lock (_lock)
        {
            int docId = _docs.Count;
            _docs.Add(new DocumentInfo(docId, externalId, tokens.Count));
            _totalTerms += tokens.Count;

            foreach (var tok in tokens)
            {
                if (!_terms.TryGetValue(tok.Term, out var pl))
                {
                    pl = new PostingsList();
                    _terms[tok.Term] = pl;
                }
                pl.Add(docId, tok.Position);
            }

            return docId;
        }
    }

    public IReadOnlyList<Posting> GetPostings(string term)
    {
        lock (_lock)
            return _terms.TryGetValue(term, out var pl) ? pl.ToPostings() : [];
    }

    public IReadOnlyList<string> Terms
    {
        get { lock (_lock) return [.. _terms.Keys]; }
    }

    public IndexSnapshot ToSnapshot()
    {
        lock (_lock)
        {
            var snap = new IndexSnapshot
            {
                Documents = [.._docs],
                AvgDocLen = AvgDocLen
            };
            foreach (var (term, pl) in _terms)
                snap.Postings[term] = pl.ToPostings()
                    .Select(p => new PostingData
                    {
                        DocId     = p.DocId,
                        Frequency = p.Frequency,
                        Positions = [..p.Positions]
                    })
                    .ToList();
            return snap;
        }
    }

    public static InvertedIndex FromSnapshot(IndexSnapshot snap)
    {
        var idx = new InvertedIndex();
        foreach (var doc in snap.Documents)
            idx._docs.Add(doc);
        idx._totalTerms = snap.Documents.Sum(d => d.TermCount);

        foreach (var (term, postingDatas) in snap.Postings)
        {
            var pl = new PostingsList();
            foreach (var pd in postingDatas)
                foreach (var pos in pd.Positions)
                    pl.Add(pd.DocId, pos);
            idx._terms[term] = pl;
        }
        return idx;
    }
}
