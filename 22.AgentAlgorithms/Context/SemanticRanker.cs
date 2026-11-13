// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentAlgorithms;

// ─── Text chunk ───────────────────────────────────────────────────────────────

public sealed record TextChunk(
    string Id,
    string Content,
    double Score = 0.0,
    ContentType Type = ContentType.NaturalLanguage);  // default preserves all existing call sites

// ─── Semantic ranker ──────────────────────────────────────────────────────────

// Ranks text chunks by relevance to a query using a provided search provider.
// In production the provider calls an embedding model; in tests it returns fixed scores.
public sealed class SemanticRanker
{
    private readonly ISemanticSearchProvider _search;

    public SemanticRanker(ISemanticSearchProvider search)
        => _search = search;

    // Return the chunks sorted by relevance to the query, descending.
    public async Task<OperationResult<IReadOnlyList<TextChunk>>> RankAsync(
        IReadOnlyList<TextChunk> chunks,
        string query)
    {
        if (chunks.Count == 0)
            return OperationResult<IReadOnlyList<TextChunk>>.Ok([]);

        var searchResult = await _search.SearchAsync(query, topK: chunks.Count);
        if (!searchResult.IsSuccess)
            return OperationResult<IReadOnlyList<TextChunk>>.Fail(
                searchResult.ErrorMessage, searchResult.ErrorCode);

        // Build a lookup from chunk id to score.
        var scoreMap = searchResult.Value!.ToDictionary(r => r.ChunkId, r => r.Score, StringComparer.Ordinal);

        var ranked = chunks
            .Select(c => c with { Score = scoreMap.GetValueOrDefault(c.Id, 0.0) })
            .OrderByDescending(c => c.Score)
            .ToList();

        return OperationResult<IReadOnlyList<TextChunk>>.Ok(ranked);
    }
}
