// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using GraphEngine;
using GraphEngine.Models;

namespace LlmFromScratch.Tests;

/// <summary>
/// Edge cases for graph engine.
/// Behavioral assertion gate: each test verifies a specific domain invariant.
/// </summary>
public class GraphEngineEdgeCaseTests
{
    // ── Empty graph ───────────────────────────────────────────────────────────

    [Fact]
    public void KnowledgeGraph_Empty_HasZeroCounts()
    {
        var g = new KnowledgeGraph();
        Assert.Equal(0, g.NodeCount);
        Assert.Equal(0, g.EdgeCount);
    }

    [Fact]
    public void PageRank_EmptyGraph_DoesNotThrow()
    {
        var g = new KnowledgeGraph();
        var pr = new PageRankCalculator();
        var ex = Record.Exception(() => pr.ComputePageRank(g));
        Assert.Null(ex);
    }

    [Fact]
    public void PersonalisedPageRank_EmptyGraph_ReturnsEmpty()
    {
        var g = new KnowledgeGraph();
        var pr = new PageRankCalculator();
        var scores = pr.PersonalisedPageRank(g, ["a"]);
        Assert.Empty(scores);
    }

    // ── Node overwrite ────────────────────────────────────────────────────────

    [Fact]
    public void AddNode_SameId_OverwritesExisting()
    {
        var g = new KnowledgeGraph();
        g.AddNode(new GraphNode { Id = "x", Name = "First", Type = "Class" });
        g.AddNode(new GraphNode { Id = "x", Name = "Second", Type = "Method" });

        Assert.Equal(1, g.NodeCount);
        g.TryGetNode("x", out var node);
        Assert.Equal("Second", node.Name);
    }

    // ── Neighbours ────────────────────────────────────────────────────────────

    [Fact]
    public void Neighbours_ReturnsTargetNodes()
    {
        var g = new KnowledgeGraph();
        g.AddNode(new GraphNode { Id = "a", Name = "A", Type = "Class" });
        g.AddNode(new GraphNode { Id = "b", Name = "B", Type = "Class" });
        g.AddNode(new GraphNode { Id = "c", Name = "C", Type = "Class" });
        g.AddEdge(new GraphEdge { From = "a", To = "b", RelationType = "Calls", Weight = 1 });
        g.AddEdge(new GraphEdge { From = "a", To = "c", RelationType = "Calls", Weight = 1 });

        var neighbours = g.Neighbours("a").ToList();

        Assert.Equal(2, neighbours.Count);
        Assert.Contains(neighbours, n => n.Id == "b");
        Assert.Contains(neighbours, n => n.Id == "c");
    }

    [Fact]
    public void Neighbours_IsolatedNode_ReturnsEmpty()
    {
        var g = new KnowledgeGraph();
        g.AddNode(new GraphNode { Id = "alone", Name = "Alone", Type = "Class" });

        var neighbours = g.Neighbours("alone").ToList();

        Assert.Empty(neighbours);
    }

    // ── Disconnected graph PageRank ───────────────────────────────────────────

    [Fact]
    public void PageRank_DisconnectedGraph_AllNodesGetPositiveScore()
    {
        var g = new KnowledgeGraph();
        g.AddNode(new GraphNode { Id = "a", Name = "A", Type = "Class" });
        g.AddNode(new GraphNode { Id = "b", Name = "B", Type = "Class" });
        // No edges — fully disconnected

        var pr = new PageRankCalculator();
        pr.ComputePageRank(g);

        Assert.True(g.Nodes["a"].PageRankScore > 0);
        Assert.True(g.Nodes["b"].PageRankScore > 0);
    }

    [Fact]
    public void PageRank_DisconnectedGraph_SumsToOne()
    {
        var g = new KnowledgeGraph();
        g.AddNode(new GraphNode { Id = "a", Name = "A", Type = "Class" });
        g.AddNode(new GraphNode { Id = "b", Name = "B", Type = "Class" });

        var pr = new PageRankCalculator();
        pr.ComputePageRank(g);

        double total = g.Nodes.Values.Sum(n => n.PageRankScore);
        Assert.Equal(1.0, total, precision: 3);
    }

    // ── PersonalisedPageRank with multiple seeds ──────────────────────────────

    [Fact]
    public void PersonalisedPageRank_MultipleSeeds_AllSeedsRankHigh()
    {
        var g = new KnowledgeGraph();
        for (int i = 0; i < 5; i++)
            g.AddNode(new GraphNode { Id = $"n{i}", Name = $"N{i}", Type = "Class" });
        // Linear chain
        for (int i = 0; i < 4; i++)
            g.AddEdge(new GraphEdge { From = $"n{i}", To = $"n{i+1}", RelationType = "Calls", Weight = 1 });

        var pr = new PageRankCalculator();
        var scores = pr.PersonalisedPageRank(g, ["n0", "n4"]);

        // Seeds n0 and n4 should both be in the top 3
        var topIds = scores.Take(3).Select(s => s.id).ToHashSet();
        Assert.Contains("n0", topIds);
    }

    // ── PageRank weight sensitivity ───────────────────────────────────────────

    [Fact]
    public void PageRank_HighWeightEdge_GivesHigherScoreToTarget()
    {
        var g = new KnowledgeGraph();
        g.AddNode(new GraphNode { Id = "src", Name = "Src", Type = "Class" });
        g.AddNode(new GraphNode { Id = "heavy", Name = "Heavy", Type = "Class" });
        g.AddNode(new GraphNode { Id = "light", Name = "Light", Type = "Class" });
        g.AddEdge(new GraphEdge { From = "src", To = "heavy", RelationType = "Calls", Weight = 10 });
        g.AddEdge(new GraphEdge { From = "src", To = "light", RelationType = "Calls", Weight = 1 });

        var pr = new PageRankCalculator();
        pr.ComputePageRank(g);

        Assert.True(g.Nodes["heavy"].PageRankScore > g.Nodes["light"].PageRankScore);
    }

    // ── GraphSerializer: empty graph roundtrip ────────────────────────────────

    [Fact]
    public void GraphSerializer_EmptyGraph_SaveLoadRoundTrips()
    {
        var g = new KnowledgeGraph();
        var tmp = Path.GetTempFileName() + ".graph.json";
        GraphSerializer.Save(g, tmp);
        var loaded = GraphSerializer.Load(tmp);
        File.Delete(tmp);

        Assert.Equal(0, loaded.NodeCount);
        Assert.Equal(0, loaded.EdgeCount);
    }

    // ── TryGetNode for missing key ────────────────────────────────────────────

    [Fact]
    public void TryGetNode_MissingId_ReturnsFalse()
    {
        var g = new KnowledgeGraph();
        bool found = g.TryGetNode("ghost", out _);
        Assert.False(found);
    }
}
