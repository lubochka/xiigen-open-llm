namespace VectorSearch;

public record IndexEntry(string Id, float[] Vector, Dictionary<string, string> Metadata);
