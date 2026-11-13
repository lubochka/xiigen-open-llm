using System.Text.Json;
using GraphEngine.Models;

namespace GraphEngine;

/// <summary>
/// JSON persistence for KnowledgeGraph.
/// DNA rule 7: atomic write (write .tmp then rename).
/// </summary>
public static class GraphSerializer
{
    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = false };

    public static void Save(KnowledgeGraph graph, string path)
    {
        var data = new SerializedGraph
        {
            Nodes = graph.Nodes.Values.ToList(),
            Edges = graph.Nodes.Keys
                .SelectMany(id => graph.OutEdges(id))
                .ToList()
        };

        var tmp = path + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(data, JsonOpts));
        File.Move(tmp, path, overwrite: true);
    }

    public static KnowledgeGraph Load(string path)
    {
        var data = JsonSerializer.Deserialize<SerializedGraph>(File.ReadAllText(path))
            ?? throw new InvalidOperationException("Failed to deserialize graph");

        var graph = new KnowledgeGraph();
        foreach (var node in data.Nodes) graph.AddNode(node);
        foreach (var edge in data.Edges) graph.AddEdge(edge);
        return graph;
    }

    private class SerializedGraph
    {
        public List<GraphNode> Nodes { get; set; } = [];
        public List<GraphEdge> Edges { get; set; } = [];
    }
}
