namespace SearchEngine;

// Facade that composes analyzer + index + scorer behind ISearchEngine.
public sealed class SearchEngineFacade : ISearchEngine
{
    private readonly IAnalyzer     _analyzer;
    private readonly InvertedIndex _index;
    private readonly BM25Scorer    _scorer;

    public SearchEngineFacade(IAnalyzer? analyzer = null)
    {
        _analyzer = analyzer ?? new StandardAnalyzer();
        _index    = new InvertedIndex();
        _scorer   = new BM25Scorer(_index);
    }

    public OperationResult<bool> Add(string externalId, string text)
    {
        if (string.IsNullOrWhiteSpace(externalId))
            return OperationResult<bool>.Fail("externalId must not be empty");

        var tokens = _analyzer.Analyze(text ?? "");
        _index.Add(externalId, tokens);
        return OperationResult<bool>.Ok(true);
    }

    public OperationResult<IReadOnlyList<SearchResult>> Search(string query, int topK = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
            return OperationResult<IReadOnlyList<SearchResult>>.Ok([]);

        var queryTokens = _analyzer.Analyze(query);
        var results     = _scorer.Score(queryTokens, topK);
        return OperationResult<IReadOnlyList<SearchResult>>.Ok(results);
    }

    // Expose index snapshot for persistence
    public IndexSnapshot Snapshot() => _index.ToSnapshot();

    // Restore from snapshot
    public static SearchEngineFacade FromSnapshot(IndexSnapshot snap, IAnalyzer? analyzer = null)
    {
        var facade = new SearchEngineFacade(analyzer);
        var restored = InvertedIndex.FromSnapshot(snap);
        // Replace the default empty index with the restored one via reflection-safe copy
        var field = typeof(SearchEngineFacade).GetField("_index",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        field.SetValue(facade, restored);
        var scorerField = typeof(SearchEngineFacade).GetField("_scorer",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        scorerField.SetValue(facade, new BM25Scorer(restored));
        return facade;
    }
}
