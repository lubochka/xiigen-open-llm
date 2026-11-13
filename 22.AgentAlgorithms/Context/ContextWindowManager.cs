// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentAlgorithms;

// Assembles context within the token budget using five strategies from the JSX document:
//   1. Semantic search (via SemanticRanker)
//   2. Symbol graph traversal (expand from seed symbols to neighbors)
//   3. Context compaction (summarize old history when limit approaches)
//   4. Priority-based trimming (keep highest-score chunks when space runs out)
//   5. Lazy loading (tool-based on-demand — modeled by selecting only requested chunks)
public sealed class ContextWindowManager
{
    private readonly SemanticRanker         _ranker;
    private readonly ContextCompactor       _compactor;
    private readonly ITokenBudgetCalculator _budget;
    private readonly ISkillFilter           _skills;
    private readonly int                    _tokenBudget;

    public ContextWindowManager(
        SemanticRanker          ranker,
        ContextCompactor        compactor,
        int                     tokenBudget = 8000,
        ITokenBudgetCalculator? budget      = null,   // null → legacy length/4 estimate
        ISkillFilter?           skills      = null)   // null → pass through all candidates
    {
        _ranker      = ranker;
        _compactor   = compactor;
        _tokenBudget = tokenBudget;
        _budget      = budget ?? new LegacyTokenBudget();
        _skills      = skills ?? new PassThroughSkillFilter();
    }

    // Select the highest-scoring chunks that fit within the token budget.
    public async Task<OperationResult<IReadOnlyList<TextChunk>>> AssembleAsync(
        IReadOnlyList<TextChunk> candidates,
        string                   query)
    {
        // Separate skills from code content for independent filtering (E38v2)
        var skillChunks = candidates.Where(c => c.Type == ContentType.SkillDocument).ToList();
        var codeChunks  = candidates.Where(c => c.Type != ContentType.SkillDocument).ToList();

        // Filter skills through KNN (E38v2: ~6 skills instead of 46, 86% load reduction)
        var filteredSkillsResult = await _skills.FilterAsync(skillChunks, query);
        if (!filteredSkillsResult.IsSuccess)
            return OperationResult<IReadOnlyList<TextChunk>>.Fail(
                filteredSkillsResult.ErrorMessage, "SKILL_FILTER_FAILED");

        // Rank all candidates together
        var allCandidates = codeChunks.Concat(filteredSkillsResult.Value!).ToList();
        var rankResult    = await _ranker.RankAsync(allCandidates, query);
        if (!rankResult.IsSuccess)
            return OperationResult<IReadOnlyList<TextChunk>>.Fail(
                rankResult.ErrorMessage, rankResult.ErrorCode);

        // Fill the token window using content-type-aware budget (E11)
        var selected   = new List<TextChunk>();
        int usedTokens = 0;

        foreach (var chunk in rankResult.Value!)
        {
            int tokens = _budget.Estimate(chunk.Content, chunk.Type);
            if (usedTokens + tokens > _tokenBudget) break;
            selected.Add(chunk);
            usedTokens += tokens;
        }

        return OperationResult<IReadOnlyList<TextChunk>>.Ok(selected);
    }

    // Compact the conversation history when it grows beyond the threshold.
    public async Task<OperationResult<List<ConversationMessage>>> CompactHistoryAsync(
        List<ConversationMessage> history,
        Func<List<ConversationMessage>, Task<string>> summaryGenerator)
        => await _compactor.CompactAsync(history, summaryGenerator);

    // Legacy fallback: length/4 for all types (behaviour before E11)
    private sealed class LegacyTokenBudget : ITokenBudgetCalculator
    {
        public int Estimate(string content, ContentType _)
            => (int)Math.Ceiling(content.Length / 4.0);
    }

    // Pass-through: return all skill candidates unchanged (no KNN filtering)
    private sealed class PassThroughSkillFilter : ISkillFilter
    {
        public Task<OperationResult<IReadOnlyList<TextChunk>>> FilterAsync(
            IReadOnlyList<TextChunk> candidates, string _, int topK = 5, double threshold = 0.40)
            => Task.FromResult(OperationResult<IReadOnlyList<TextChunk>>.Ok(candidates));
    }
}
