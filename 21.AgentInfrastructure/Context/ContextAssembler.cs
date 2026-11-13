// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure;

// ─── Context chunk ────────────────────────────────────────────────────────────

// One piece of context to include in the prompt: a file snippet with a relevance score.
public sealed record ContextChunk(string FilePath, string Content, double RelevanceScore);

// ─── Stage 03 — Context assembler ─────────────────────────────────────────────

// Selects and ranks code snippets to include in the prompt, respecting the token budget.
// Strategy: rank all available chunks by relevance score descending, then add until budget full.
public sealed class ContextAssembler
{
    // The maximum tokens to include (leave room for the system prompt and user task).
    private readonly int _tokenBudget;

    public ContextAssembler(int tokenBudget = 8000)
        => _tokenBudget = tokenBudget;

    // Assemble the context that fits within the token budget.
    // chunks     — candidate context pieces, each with a relevance score
    // Returns the selected chunks in descending relevance order.
    public OperationResult<IReadOnlyList<ContextChunk>> Assemble(IReadOnlyList<ContextChunk> chunks)
    {
        if (chunks.Count == 0)
            return OperationResult<IReadOnlyList<ContextChunk>>.Ok([]);

        // Sort by relevance score descending so the most relevant chunk comes first.
        var ranked = chunks.OrderByDescending(c => c.RelevanceScore).ToList();

        var selected = new List<ContextChunk>();
        int usedTokens = 0;

        foreach (var chunk in ranked)
        {
            int chunkTokens = TokenCounter.Estimate(chunk.Content);
            if (usedTokens + chunkTokens > _tokenBudget) break;

            selected.Add(chunk);
            usedTokens += chunkTokens;
        }

        return OperationResult<IReadOnlyList<ContextChunk>>.Ok(selected);
    }

    // Total token count for the assembled context.
    public static int TotalTokens(IReadOnlyList<ContextChunk> chunks)
        => chunks.Sum(c => TokenCounter.Estimate(c.Content));
}
