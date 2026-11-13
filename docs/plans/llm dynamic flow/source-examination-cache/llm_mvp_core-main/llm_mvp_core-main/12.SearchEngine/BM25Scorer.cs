namespace SearchEngine;

// BM25 (Okapi BM25) scoring — Robertson & Zaragoza (2009).
// Score(d,q) = Σ IDF(qi) * (tf*(k1+1)) / (tf + k1*(1 - b + b*dl/avgdl))
// IDF(qi) = ln((N - df + 0.5) / (df + 0.5) + 1)
public sealed class BM25Scorer
{
    private const float K1 = 1.2f;
    private const float B  = 0.75f;

    private readonly InvertedIndex _index;

    public BM25Scorer(InvertedIndex index) => _index = index;

    public IReadOnlyList<SearchResult> Score(IReadOnlyList<AnalyzedToken> queryTokens, int topK)
    {
        int   N      = _index.DocCount;
        float avgdl  = _index.AvgDocLen;
        if (N == 0 || queryTokens.Count == 0) return [];

        var scores = new Dictionary<int, float>();

        foreach (var qt in queryTokens)
        {
            var postings = _index.GetPostings(qt.Term);
            int df       = postings.Count;
            if (df == 0) continue;

            float idf = MathF.Log((N - df + 0.5f) / (df + 0.5f) + 1f);

            foreach (var posting in postings)
            {
                int   docId = posting.DocId;
                float dl    = _index.Documents[docId].TermCount;
                float tf    = posting.Frequency;
                float norm  = K1 * (1f - B + B * dl / (avgdl == 0 ? 1f : avgdl));
                float bm25  = idf * (tf * (K1 + 1f)) / (tf + norm);

                scores[docId] = scores.GetValueOrDefault(docId) + bm25;
            }
        }

        return scores
            .OrderByDescending(kv => kv.Value)
            .Take(topK)
            .Select(kv => new SearchResult(_index.Documents[kv.Key].ExternalId, kv.Value))
            .ToList();
    }
}
