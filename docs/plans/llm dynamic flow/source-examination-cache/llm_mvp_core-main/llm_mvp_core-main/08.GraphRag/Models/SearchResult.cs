namespace GraphRag.Models;

public record SearchResult
{
    public string NodeId    { get; init; } = "";
    public string Name      { get; init; } = "";
    public string Type      { get; init; } = "";
    public string RawText   { get; init; } = "";
    public float  VectorScore    { get; init; }
    public double GraphScore     { get; init; }
    public double CombinedScore  { get; init; }
    public string Source    { get; init; } = ""; // "local" | "global" | "hybrid"
}
