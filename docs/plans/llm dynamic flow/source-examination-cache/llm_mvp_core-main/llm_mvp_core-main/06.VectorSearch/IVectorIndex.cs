namespace VectorSearch;

/// <summary>DNA rule 4: interface contract for all vector index implementations.</summary>
public interface IVectorIndex
{
    int Count { get; }
    void Add(float[] vector, string id, Dictionary<string, string>? metadata = null);
    List<(string id, float score)> Search(float[] query, int topK);
}
