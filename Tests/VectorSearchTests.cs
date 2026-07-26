// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using VectorSearch;

namespace LlmFromScratch.Tests;

public class VectorSearchTests
{
    private static float[] Vec(params float[] v) => v;

    [Fact]
    public void BruteForce_ReturnsExactNearestNeighbour()
    {
        var idx = new BruteForceIndex();
        idx.Add(Vec(1, 0, 0), "a");
        idx.Add(Vec(0, 1, 0), "b");
        idx.Add(Vec(0, 0, 1), "c");

        var results = idx.Search(Vec(1, 0.1f, 0), topK: 1);

        Assert.Single(results);
        Assert.Equal("a", results[0].id);
    }

    [Fact]
    public void BruteForce_TopK_LimitedCorrectly()
    {
        var idx = new BruteForceIndex();
        for (int i = 0; i < 10; i++)
            idx.Add(Vec(i, 0), $"v{i}");

        var results = idx.Search(Vec(5, 0), topK: 3);

        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void Hnsw_ReturnsReasonableNearestNeighbour()
    {
        var idx = new HnswIndex(m: 4, efConstruction: 20, efSearch: 10);
        idx.Add(Vec(1, 0), "a");
        idx.Add(Vec(0, 1), "b");
        idx.Add(Vec(-1, 0), "c");

        var results = idx.Search(Vec(1, 0.01f), topK: 1);

        Assert.Single(results);
        Assert.Equal("a", results[0].id);
    }

    [Fact]
    public void Hnsw_SaveLoad_PreservesResults()
    {
        var idx = new HnswIndex(m: 4, efConstruction: 20, efSearch: 10);
        idx.Add(Vec(1, 0), "a");
        idx.Add(Vec(0, 1), "b");

        var tmp = Path.GetTempFileName() + ".hnsw";
        idx.Save(tmp);
        var loaded = HnswIndex.Load(tmp);
        File.Delete(tmp);

        var results = loaded.Search(Vec(1, 0), topK: 1);
        Assert.Equal("a", results[0].id);
    }

    [Fact]
    public void CosineSimilarity_IdenticalVectors_ReturnsOne()
    {
        float sim = Distances.CosineSimilarity(Vec(1, 2, 3), Vec(1, 2, 3));
        Assert.Equal(1f, sim, precision: 5);
    }

    [Fact]
    public void CosineSimilarity_OrthogonalVectors_ReturnsZero()
    {
        float sim = Distances.CosineSimilarity(Vec(1, 0), Vec(0, 1));
        Assert.Equal(0f, sim, precision: 5);
    }
}
