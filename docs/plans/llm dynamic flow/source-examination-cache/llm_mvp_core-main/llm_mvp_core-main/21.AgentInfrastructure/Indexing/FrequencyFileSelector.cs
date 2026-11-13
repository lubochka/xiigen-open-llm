namespace AgentInfrastructure;

// Selects the most frequently-called files from a codebase using incoming call
// counts from a SymbolGraph.
// E02 result: +26.5% accuracy — methods called more often → in context first.
// Replaces PageRankFileSelector (PageRank was −18% to −25% on all tiers).
// Note: SymbolGraph.ComputePageRank() is still called internally by CodebaseIndexer
// for RepoMap symbol sorting — only its use for file selection is replaced here.
public sealed class FrequencyFileSelector
{
    private readonly SymbolGraph _graph;

    public FrequencyFileSelector(SymbolGraph graph)
        => _graph = graph;

    // Return topN files ordered by total incoming call count of their symbols.
    // "Hot files" — files whose methods are most frequently called by other code.
    public IReadOnlyList<string> SelectCoreFiles(IReadOnlyList<string> allFiles, int topN = 5)
    {
        return allFiles
            .Select(file => (
                File:      file,
                CallCount: _graph.AllNodes
                    .Where(n => n.FilePath == file)
                    .Sum(n => _graph.InDegree(n.Name))))
            .OrderByDescending(x => x.CallCount)
            .Take(topN)
            .Select(x => x.File)
            .ToList();
    }
}
