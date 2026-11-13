namespace GraphRag.Models;

public record RetrievalContext
{
    public string Query          { get; init; } = "";
    public List<SearchResult> Results { get; init; } = [];
    public string ContextText    { get; init; } = "";
    public int    TokenCount     { get; init; }
}
