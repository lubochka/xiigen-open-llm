// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using GraphEngine.Models;

namespace GraphEngine;

/// <summary>
/// Directed weighted graph of code entities and their relationships.
/// DNA rule 4: concrete implementation behind a clear data structure contract.
/// </summary>
public class KnowledgeGraph
{
    private readonly Dictionary<string, GraphNode> _nodes = new();
    private readonly Dictionary<string, List<GraphEdge>> _outEdges = new();
    private readonly Dictionary<string, List<GraphEdge>> _inEdges = new();

    public IReadOnlyDictionary<string, GraphNode> Nodes => _nodes;

    public void AddNode(GraphNode node)
    {
        _nodes[node.Id] = node;
        _outEdges.TryAdd(node.Id, []);
        _inEdges.TryAdd(node.Id, []);
    }

    public void AddEdge(GraphEdge edge)
    {
        if (!_nodes.ContainsKey(edge.From) || !_nodes.ContainsKey(edge.To))
            throw new InvalidOperationException($"Both endpoints must exist: {edge.From} → {edge.To}");

        _outEdges[edge.From].Add(edge);
        _inEdges[edge.To].Add(edge);
    }

    public IReadOnlyList<GraphEdge> OutEdges(string nodeId) =>
        _outEdges.TryGetValue(nodeId, out var edges) ? edges : [];

    public IReadOnlyList<GraphEdge> InEdges(string nodeId) =>
        _inEdges.TryGetValue(nodeId, out var edges) ? edges : [];

    public IEnumerable<GraphNode> Neighbours(string nodeId) =>
        OutEdges(nodeId)
            .Select(e => e.To)
            .Where(_nodes.ContainsKey)
            .Select(id => _nodes[id]);

    public bool TryGetNode(string id, out GraphNode node) => _nodes.TryGetValue(id, out node!);

    public int NodeCount => _nodes.Count;
    public int EdgeCount => _outEdges.Values.Sum(list => list.Count);
}
