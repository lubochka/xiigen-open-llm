using Embeddings;
using GraphEngine;
using GraphEngine.Models;
using GraphRag.Models;
using VectorSearch;

namespace GraphRag;

/// <summary>
/// Global search: find seed nodes via vector similarity, then expand via
/// personalised PageRank to surface structurally important related nodes.
/// </summary>
public class GlobalSearch
{
    private readonly EmbeddingModel      _embedder;
    private readonly IVectorIndex        _vectorIndex;
    private readonly KnowledgeGraph      _graph;
    private readonly PageRankCalculator  _pageRank;

    public GlobalSearch(
        EmbeddingModel embedder,
        IVectorIndex vectorIndex,
        KnowledgeGraph graph,
        PageRankCalculator pageRank)
    {
        _embedder    = embedder;
        _vectorIndex = vectorIndex;
        _graph       = graph;
        _pageRank    = pageRank;
    }

    public List<SearchResult> Search(string query, int seedK = 5, int topK = 20)
    {
        float[] queryVec = _embedder.Embed(query);
        var seeds = _vectorIndex.Search(queryVec, seedK).Select(h => h.id);

        var pprScores = _pageRank.PersonalisedPageRank(_graph, seeds);

        return pprScores
            .Take(topK)
            .Where(t => _graph.TryGetNode(t.id, out _))
            .Select(t =>
            {
                _graph.TryGetNode(t.id, out var node);
                return new SearchResult
                {
                    NodeId        = node!.Id,
                    Name          = node.Name,
                    Type          = node.Type,
                    RawText       = node.RawText,
                    GraphScore    = t.score,
                    CombinedScore = t.score,
                    Source        = "global"
                };
            })
            .ToList();
    }
}
