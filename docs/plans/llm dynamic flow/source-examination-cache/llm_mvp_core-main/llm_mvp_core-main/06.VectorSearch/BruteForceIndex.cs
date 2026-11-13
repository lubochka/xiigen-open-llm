namespace VectorSearch;

/// <summary>
/// Exact nearest-neighbour search by scanning every vector.
/// O(n) per query — correct but slow for large collections.
/// Use for: small collections (&lt;10,000 vectors) or verifying HNSW correctness.
/// DNA rule 4: implements IVectorIndex interface.
/// </summary>
public class BruteForceIndex : IVectorIndex
{
    private readonly List<IndexEntry> _entries = [];

    public int Count => _entries.Count;

    public void Add(float[] vector, string id, Dictionary<string, string>? metadata = null) =>
        _entries.Add(new IndexEntry(id, vector, metadata ?? []));

    public List<(string id, float score)> Search(float[] query, int topK)
    {
        return _entries
            .Select(e => (e.Id, Distances.CosineSimilarity(query, e.Vector)))
            .OrderByDescending(t => t.Item2)
            .Take(topK)
            .ToList();
    }

    public void Save(string path) =>
        throw new NotImplementedException("BruteForceIndex is in-memory only");

    public static BruteForceIndex Load(string path) =>
        throw new NotImplementedException("BruteForceIndex is in-memory only");
}
