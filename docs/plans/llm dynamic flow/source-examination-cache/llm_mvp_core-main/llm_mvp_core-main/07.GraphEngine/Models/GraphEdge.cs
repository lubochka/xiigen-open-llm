namespace GraphEngine.Models;

public record GraphEdge
{
    public string From         { get; init; } = "";
    public string To           { get; init; } = "";
    public string RelationType { get; init; } = ""; // Calls | Defines | Inherits | Implements
    public double Weight       { get; init; } = 1.0;
}
