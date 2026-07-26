// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using Embeddings;
using GraphEngine;
using GraphRag.Models;
using VectorSearch;

namespace GraphRag;

/// <summary>
/// Local (semantic) search: embed query, find nearest vectors, return matching nodes.
/// </summary>
public class LocalSearch
{
    private readonly EmbeddingModel _embedder;
    private readonly IVectorIndex   _vectorIndex;
    private readonly KnowledgeGraph _graph;

    public LocalSearch(EmbeddingModel embedder, IVectorIndex vectorIndex, KnowledgeGraph graph)
    {
        _embedder    = embedder;
        _vectorIndex = vectorIndex;
        _graph       = graph;
    }

    public List<SearchResult> Search(string query, int topK = 10)
    {
        float[] queryVec = _embedder.Embed(query);
        var hits = _vectorIndex.Search(queryVec, topK);

        return hits
            .Where(h => _graph.TryGetNode(h.id, out _))
            .Select(h =>
            {
                _graph.TryGetNode(h.id, out var node);
                return new SearchResult
                {
                    NodeId       = node!.Id,
                    Name         = node.Name,
                    Type         = node.Type,
                    RawText      = node.RawText,
                    VectorScore  = h.score,
                    GraphScore   = node.PageRankScore,
                    CombinedScore = h.score + node.PageRankScore,
                    Source       = "local"
                };
            })
            .OrderByDescending(r => r.CombinedScore)
            .ToList();
    }
}
