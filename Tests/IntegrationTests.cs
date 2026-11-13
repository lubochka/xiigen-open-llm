// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using DatasetPipeline;
using GraphEngine;
using GraphEngine.Models;
using Tokenizer;
using VectorSearch;

namespace LlmFromScratch.Tests;

/// <summary>
/// End-to-end integration tests verifying data flows across component boundaries.
/// Based on E2E test matrix builder skill: every scenario is a named chain with
/// explicit assertions at each boundary crossing (contract-driven, not synthetic).
///
/// Covers the data-flow integrity rule: output of producer matches input contract of consumer.
/// </summary>
public class IntegrationTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // Chain 1: Tokenizer → NpyWriter (DatasetPipeline)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Chain_Tokenize_WriteNpy_OutputIsReadableBytes()
    {
        // Arrange
        var bpe = new BpeTokenizer();
        bpe.Train("hello world hello world", vocabSize: 270);
        var tokens = bpe.Encode("hello world");

        // Assert tokenizer contract: tokens are valid vocab indices
        Assert.All(tokens, t => Assert.InRange(t, 0, bpe.VocabSize - 1));

        // Act: write tokens as uint16 shard
        var tmp = Path.GetTempFileName() + ".npy";
        ushort[] shard = tokens.Select(t => (ushort)t).ToArray();
        NpyWriter.Write(tmp, shard);

        // Assert NpyWriter contract: file is non-empty, starts with numpy magic
        var bytes = File.ReadAllBytes(tmp);
        File.Delete(tmp);
        Assert.True(bytes.Length > 128);
        Assert.Equal(0x93, bytes[0]);
        Assert.Equal((byte)'N', bytes[1]);
    }

    [Fact]
    public void Chain_Tokenize_WriteNpy_TokenValuesPreserved()
    {
        var bpe = new BpeTokenizer();
        bpe.Train("abcabc", vocabSize: 260);
        var tokens = bpe.Encode("abc");

        var tmp = Path.GetTempFileName() + ".npy";
        ushort[] shard = tokens.Select(t => (ushort)t).ToArray();
        NpyWriter.Write(tmp, shard);

        var bytes = File.ReadAllBytes(tmp);
        File.Delete(tmp);

        // Verify each token value survives the round-trip through the .npy file
        const int headerSize = 128;
        for (int i = 0; i < shard.Length; i++)
        {
            ushort fromFile = BitConverter.ToUInt16(bytes, headerSize + i * 2);
            Assert.Equal(shard[i], fromFile);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Chain 2: KnowledgeGraph → PageRank → GraphSerializer (full graph flow)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Chain_BuildGraph_ComputePageRank_Serialize_Deserialize()
    {
        // Build
        var g = new KnowledgeGraph();
        for (int i = 0; i < 5; i++)
            g.AddNode(new GraphNode { Id = $"n{i}", Name = $"N{i}", Type = "Class" });
        for (int i = 0; i < 4; i++)
            g.AddEdge(new GraphEdge { From = $"n{i}", To = $"n{i+1}", RelationType = "Calls", Weight = 1 });

        // PageRank
        var pr = new PageRankCalculator();
        pr.ComputePageRank(g);

        // Assert PageRank contract before serialization: scores are positive and sum to ~1
        Assert.True(g.Nodes.Values.All(n => n.PageRankScore > 0));
        Assert.Equal(1.0, g.Nodes.Values.Sum(n => n.PageRankScore), precision: 3);

        // Serialize + deserialize
        var tmp = Path.GetTempFileName() + ".graph.json";
        GraphSerializer.Save(g, tmp);
        var loaded = GraphSerializer.Load(tmp);
        File.Delete(tmp);

        // Assert deserialized graph contract: structure matches original
        Assert.Equal(5, loaded.NodeCount);
        Assert.Equal(4, loaded.EdgeCount);
        Assert.True(loaded.TryGetNode("n2", out var n2));
        Assert.Equal("N2", n2.Name);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Chain 3: BruteForce vs HNSW agreement (nearest neighbour contract)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Chain_BruteForce_Hnsw_AgreeOnNearestNeighbour()
    {
        // Load both indexes with the same data
        var vectors = new (float[] vec, string id)[]
        {
            (new float[] { 1, 0, 0 }, "x-axis"),
            (new float[] { 0, 1, 0 }, "y-axis"),
            (new float[] { 0, 0, 1 }, "z-axis"),
            (new float[] { 1, 1, 0 }, "xy"),
            (new float[] { 1, 0, 1 }, "xz"),
        };

        var bf   = new BruteForceIndex();
        var hnsw = new HnswIndex(m: 8, efConstruction: 40, efSearch: 40);

        foreach (var (vec, id) in vectors)
        {
            bf.Add(vec, id);
            hnsw.Add(vec, id);
        }

        float[] query = { 0.9f, 0.1f, 0f };

        var bfTop1   = bf.Search(query, topK: 1)[0].id;
        var hnswTop1 = hnsw.Search(query, topK: 1)[0].id;

        // Both exact and approximate search must agree on the nearest neighbour
        Assert.Equal(bfTop1, hnswTop1);
    }

    [Fact]
    public void Chain_BruteForce_Hnsw_TopK_BothReturnSameSize()
    {
        var bf   = new BruteForceIndex();
        var hnsw = new HnswIndex(m: 4, efConstruction: 20, efSearch: 20);

        for (int i = 0; i < 8; i++)
        {
            float[] v = { i, i * 0.5f };
            bf.Add(v, $"v{i}");
            hnsw.Add(v, $"v{i}");
        }

        float[] q = { 3f, 2f };
        var bfResults   = bf.Search(q, topK: 3);
        var hnswResults = hnsw.Search(q, topK: 3);

        Assert.Equal(3, bfResults.Count);
        Assert.Equal(3, hnswResults.Count);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Chain 4: Scheduler → AdamW parameter contract
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Chain_Scheduler_LrIsPositiveAndBounded()
    {
        var sched = new TrainingLoop.Schedulers.CosineWithWarmup(
            maxLr: 1e-3f, minLr: 1e-4f, warmupSteps: 50, totalSteps: 500);

        // Step 0: warmup starts at 0 — below minLr is expected (linear ramp begins)
        Assert.Equal(0f, sched.GetLr(0), precision: 6);

        // After warmup peak, cosine decay keeps lr in [minLr, maxLr]
        for (int step = 50; step <= 500; step += 50)
        {
            float lr = sched.GetLr(step);
            Assert.True(lr >= 1e-4f - 1e-8f, $"lr {lr} below minLr at step {step}");
            Assert.True(lr <= 1e-3f + 1e-8f, $"lr {lr} above maxLr at step {step}");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Chain 5: GraphSerializer atomic write — tmp then rename (DNA rule 7)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Chain_GraphSerializer_AtomicWrite_NoTmpFileLeftOver()
    {
        var g = new KnowledgeGraph();
        g.AddNode(new GraphNode { Id = "a", Name = "A", Type = "Class" });

        var dir  = Path.GetTempPath();
        var path = Path.Combine(dir, $"test-graph-{Guid.NewGuid()}.json");
        var tmp  = path + ".tmp";

        GraphSerializer.Save(g, path);

        // After atomic write: target exists, .tmp is gone
        Assert.True(File.Exists(path));
        Assert.False(File.Exists(tmp));
        File.Delete(path);
    }
}
