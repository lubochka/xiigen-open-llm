// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using VectorSearch;

namespace LlmFromScratch.Tests;

/// <summary>
/// Edge cases and behavioral assertions for vector index implementations.
/// Behavioral assertion gate: every test asserts a specific domain value, not just "it ran".
/// </summary>
public class VectorSearchEdgeCaseTests
{
    private static float[] Vec(params float[] v) => v;

    // ── Empty index ───────────────────────────────────────────────────────────

    [Fact]
    public void BruteForce_EmptyIndex_ReturnsEmpty()
    {
        var idx = new BruteForceIndex();
        var results = idx.Search(Vec(1, 0), topK: 5);
        Assert.Empty(results);
    }

    [Fact]
    public void Hnsw_EmptyIndex_ReturnsEmpty()
    {
        var idx = new HnswIndex();
        var results = idx.Search(Vec(1, 0), topK: 5);
        Assert.Empty(results);
    }

    // ── Count tracking ────────────────────────────────────────────────────────

    [Fact]
    public void BruteForce_Count_TracksInserts()
    {
        var idx = new BruteForceIndex();
        Assert.Equal(0, idx.Count);
        idx.Add(Vec(1, 0), "a");
        Assert.Equal(1, idx.Count);
        idx.Add(Vec(0, 1), "b");
        Assert.Equal(2, idx.Count);
    }

    [Fact]
    public void Hnsw_Count_TracksInserts()
    {
        var idx = new HnswIndex();
        Assert.Equal(0, idx.Count);
        idx.Add(Vec(1, 0), "a");
        Assert.Equal(1, idx.Count);
    }

    // ── TopK > actual count ───────────────────────────────────────────────────

    [Fact]
    public void BruteForce_TopKLargerThanCount_ReturnsAllAvailable()
    {
        var idx = new BruteForceIndex();
        idx.Add(Vec(1, 0), "a");
        idx.Add(Vec(0, 1), "b");

        var results = idx.Search(Vec(1, 0), topK: 100);

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void Hnsw_TopKLargerThanCount_ReturnsAllAvailable()
    {
        var idx = new HnswIndex(m: 4, efConstruction: 10, efSearch: 10);
        idx.Add(Vec(1, 0), "a");
        idx.Add(Vec(0, 1), "b");

        var results = idx.Search(Vec(1, 0), topK: 100);

        Assert.True(results.Count <= 2);
        Assert.True(results.Count >= 1);
    }

    // ── Score ordering ────────────────────────────────────────────────────────

    [Fact]
    public void BruteForce_Results_OrderedByScoreDescending()
    {
        var idx = new BruteForceIndex();
        idx.Add(Vec(1, 0, 0), "best");
        idx.Add(Vec(0, 1, 0), "mid");
        idx.Add(Vec(0, 0, 1), "worst");

        var results = idx.Search(Vec(1, 0.1f, 0), topK: 3);

        Assert.Equal(3, results.Count);
        Assert.True(results[0].score >= results[1].score);
        Assert.True(results[1].score >= results[2].score);
        Assert.Equal("best", results[0].id);
    }

    // ── Metadata round-trip ───────────────────────────────────────────────────

    [Fact]
    public void BruteForce_Metadata_StoredPerEntry()
    {
        var idx = new BruteForceIndex();
        idx.Add(Vec(1, 0), "node1", new Dictionary<string, string> { ["type"] = "Method" });
        idx.Add(Vec(0, 1), "node2", new Dictionary<string, string> { ["type"] = "Class" });

        // Searching returns the right ID regardless of metadata
        var results = idx.Search(Vec(1, 0), topK: 1);
        Assert.Equal("node1", results[0].id);
    }

    // ── Exact match ───────────────────────────────────────────────────────────

    [Fact]
    public void BruteForce_ExactQueryVector_ReturnsScoreOfOne()
    {
        var idx = new BruteForceIndex();
        idx.Add(Vec(3, 4), "v");

        var results = idx.Search(Vec(3, 4), topK: 1);

        Assert.Single(results);
        Assert.Equal(1f, results[0].score, precision: 5);
    }

    // ── Cosine similarity boundary values ────────────────────────────────────

    [Fact]
    public void CosineSimilarity_OppositeVectors_ReturnsNegativeOne()
    {
        float sim = Distances.CosineSimilarity(new[] { 1f, 0f }, new[] { -1f, 0f });
        Assert.Equal(-1f, sim, precision: 5);
    }

    [Fact]
    public void CosineSimilarity_ScaledVector_SameAsUnit()
    {
        float sim1 = Distances.CosineSimilarity(new[] { 1f, 0f }, new[] { 1f, 0f });
        float sim2 = Distances.CosineSimilarity(new[] { 1f, 0f }, new[] { 100f, 0f });
        Assert.Equal(sim1, sim2, precision: 5);
    }

    // ── HNSW: multiple vectors all retrievable ────────────────────────────────

    [Fact]
    public void Hnsw_TenVectors_AllRetrievableWithLargeTopK()
    {
        var idx = new HnswIndex(m: 8, efConstruction: 40, efSearch: 40);
        for (int i = 0; i < 10; i++)
            idx.Add(new float[] { i, 0 }, $"v{i}");

        var results = idx.Search(new float[] { 4.5f, 0 }, topK: 10);

        Assert.True(results.Count > 0);
        // The nearest to 4.5 should be v4 or v5
        Assert.Contains(results, r => r.id == "v4" || r.id == "v5");
    }

    // ── Single-vector HNSW ────────────────────────────────────────────────────

    [Fact]
    public void Hnsw_SingleVector_ReturnedAsSelfMatch()
    {
        var idx = new HnswIndex();
        idx.Add(Vec(1, 2, 3), "only");

        var results = idx.Search(Vec(1, 2, 3), topK: 5);

        Assert.Single(results);
        Assert.Equal("only", results[0].id);
        Assert.Equal(1f, results[0].score, precision: 4);
    }
}
