// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using GraphEngine;
using GraphEngine.Models;

namespace LlmFromScratch.Tests;

public class GraphEngineTests
{
    private static KnowledgeGraph BuildTriangle()
    {
        var g = new KnowledgeGraph();
        g.AddNode(new GraphNode { Id = "a", Name = "A", Type = "Class" });
        g.AddNode(new GraphNode { Id = "b", Name = "B", Type = "Class" });
        g.AddNode(new GraphNode { Id = "c", Name = "C", Type = "Class" });
        g.AddEdge(new GraphEdge { From = "a", To = "b", RelationType = "Calls", Weight = 1 });
        g.AddEdge(new GraphEdge { From = "b", To = "c", RelationType = "Calls", Weight = 1 });
        g.AddEdge(new GraphEdge { From = "c", To = "a", RelationType = "Calls", Weight = 1 });
        return g;
    }

    [Fact]
    public void AddNode_NodeIsRetrievable()
    {
        var g = new KnowledgeGraph();
        g.AddNode(new GraphNode { Id = "x", Name = "X", Type = "Method" });
        Assert.True(g.TryGetNode("x", out var node));
        Assert.Equal("X", node.Name);
    }

    [Fact]
    public void AddEdge_UnknownNode_Throws()
    {
        var g = new KnowledgeGraph();
        g.AddNode(new GraphNode { Id = "a", Name = "A", Type = "Class" });
        Assert.Throws<InvalidOperationException>(() =>
            g.AddEdge(new GraphEdge { From = "a", To = "z", RelationType = "Calls" }));
    }

    [Fact]
    public void OutEdges_InEdges_Correct()
    {
        var g = BuildTriangle();
        Assert.Single(g.OutEdges("a"));
        Assert.Equal("b", g.OutEdges("a")[0].To);
        Assert.Single(g.InEdges("a"));
        Assert.Equal("c", g.InEdges("a")[0].From);
    }

    [Fact]
    public void PageRank_SumsToOne()
    {
        var g = BuildTriangle();
        var pr = new PageRankCalculator();
        pr.ComputePageRank(g);

        double total = g.Nodes.Values.Sum(n => n.PageRankScore);
        Assert.Equal(1.0, total, precision: 4);
    }

    [Fact]
    public void PersonalisedPageRank_SeedHasHighestScore()
    {
        var g = BuildTriangle();
        var pr = new PageRankCalculator();
        var scores = pr.PersonalisedPageRank(g, ["a"]);

        Assert.Equal("a", scores[0].id);
    }

    [Fact]
    public void GraphSerializer_SaveLoad_RoundTrips()
    {
        var g = BuildTriangle();
        var tmp = Path.GetTempFileName() + ".graph.json";
        GraphSerializer.Save(g, tmp);
        var loaded = GraphSerializer.Load(tmp);
        File.Delete(tmp);

        Assert.Equal(3, loaded.NodeCount);
        Assert.Equal(3, loaded.EdgeCount);
    }
}
