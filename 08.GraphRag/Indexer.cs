// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using Embeddings;
using GraphEngine;
using GraphEngine.Models;
using VectorSearch;

namespace GraphRag;

/// <summary>
/// Indexes a KnowledgeGraph into a vector index for local (semantic) search.
/// DNA rule 8: graph is fully built before vector index is populated.
/// </summary>
public class Indexer
{
    private readonly EmbeddingModel _embedder;
    private readonly IVectorIndex   _vectorIndex;

    public Indexer(EmbeddingModel embedder, IVectorIndex vectorIndex)
    {
        _embedder    = embedder;
        _vectorIndex = vectorIndex;
    }

    public async Task IndexGraphAsync(KnowledgeGraph graph, CancellationToken ct = default)
    {
        foreach (var node in graph.Nodes.Values)
        {
            ct.ThrowIfCancellationRequested();

            string text = BuildNodeText(node);
            float[] embedding = await Task.Run(() => _embedder.Embed(text), ct);

            var meta = new Dictionary<string, string>
            {
                ["name"]     = node.Name,
                ["type"]     = node.Type,
                ["filePath"] = node.FilePath
            };
            _vectorIndex.Add(embedding, node.Id, meta);
        }
    }

    private static string BuildNodeText(GraphNode node)
    {
        if (!string.IsNullOrEmpty(node.RawText)) return node.RawText;
        if (!string.IsNullOrEmpty(node.Signature)) return node.Signature;
        return node.Name;
    }
}
