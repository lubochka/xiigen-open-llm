namespace AgentAlgorithms;

// Skill retriever based on E38v2 experiment results.
// Selects top-K skills using semantic search, with confidence-based fallback.
// Confidence = top score / total top-K score mass.
// When confidence >= threshold, returns the filtered top-K skills.
// When confidence < threshold (uniform distribution), falls back to all candidates.
public sealed class KnnSkillFilter : ISkillFilter
{
    private readonly ISemanticSearchProvider _search;

    public KnnSkillFilter(ISemanticSearchProvider search) => _search = search;

    public async Task<OperationResult<IReadOnlyList<TextChunk>>> FilterAsync(
        IReadOnlyList<TextChunk> skillCandidates,
        string                   taskDescription,
        int                      topK                = 5,
        double                   confidenceThreshold = 0.40)
    {
        if (!skillCandidates.Any())
            return OperationResult<IReadOnlyList<TextChunk>>.Ok(skillCandidates);

        var searchResult = await _search.SearchAsync(taskDescription, topK: topK);

        if (!searchResult.IsSuccess || !searchResult.Value!.Any())
            return OperationResult<IReadOnlyList<TextChunk>>.Ok(skillCandidates);

        double topScore   = searchResult.Value!.First().Score;
        double totalScore = searchResult.Value!.Sum(r => r.Score) + 1e-9;
        double confidence = topScore / totalScore;

        if (confidence < confidenceThreshold)
            return OperationResult<IReadOnlyList<TextChunk>>.Ok(skillCandidates);

        var topIds   = searchResult.Value!.Select(r => r.ChunkId).ToHashSet(StringComparer.Ordinal);
        var filtered = skillCandidates.Where(c => topIds.Contains(c.Id)).ToList();
        return OperationResult<IReadOnlyList<TextChunk>>.Ok(filtered);
    }
}
