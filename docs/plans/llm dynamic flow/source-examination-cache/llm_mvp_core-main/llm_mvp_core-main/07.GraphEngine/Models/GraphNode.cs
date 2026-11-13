namespace GraphEngine.Models;

public record GraphNode
{
    public string Id            { get; init; } = "";
    public string Name          { get; init; } = "";
    public string Type          { get; init; } = ""; // Class | Method | File | Interface
    public string FilePath      { get; init; } = "";
    public string Signature     { get; init; } = "";
    public string RawText       { get; init; } = "";
    public float[] Embedding    { get; init; } = [];
    public double PageRankScore { get; set; }
}
