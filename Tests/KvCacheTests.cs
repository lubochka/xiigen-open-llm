// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using Inference;

namespace LlmFromScratch.Tests;

// Unit tests for KvCache — verifies sequence dimension growth and clear.
// Behavioral assertion gate: SeqLen and tensor shapes are asserted explicitly.
public class KvCacheTests : IDisposable
{
    private readonly KvCache _cache = new(numLayers: 2);

    public void Dispose() => _cache.Dispose();

    [Fact]
    public void Initial_SeqLen_IsZero()
    {
        Assert.Equal(0, _cache.SeqLen);
    }

    [Fact]
    public void Update_FirstSlice_SeqLenBecomesOne()
    {
        var k = new Tensor(new float[1 * 4 * 1 * 8], new TensorShape(1, 4, 1, 8));
        var v = new Tensor(new float[1 * 4 * 1 * 8], new TensorShape(1, 4, 1, 8));

        _cache.Update(0, k, v);

        Assert.Equal(1, _cache.SeqLen);
    }

    [Fact]
    public void Update_TwoSlices_SeqLenBecomesTwo()
    {
        var k1 = new Tensor(new float[1 * 4 * 1 * 8], new TensorShape(1, 4, 1, 8));
        var v1 = new Tensor(new float[1 * 4 * 1 * 8], new TensorShape(1, 4, 1, 8));

        var k2Data = new float[1 * 4 * 1 * 8];
        Array.Fill(k2Data, 1f);
        var k2 = new Tensor(k2Data, new TensorShape(1, 4, 1, 8));
        var v2 = new Tensor(k2Data.ToArray(), new TensorShape(1, 4, 1, 8));

        _cache.Update(0, k1, v1);
        _cache.Update(0, k2, v2);

        Assert.Equal(2, _cache.SeqLen);
    }

    [Fact]
    public void Update_ReturnsConcatenatedKeys()
    {
        var k1Data = new float[1 * 2 * 1 * 4];
        Array.Fill(k1Data, 1f);
        var k1 = new Tensor(k1Data, new TensorShape(1, 2, 1, 4));
        var v1 = new Tensor(new float[1 * 2 * 1 * 4], new TensorShape(1, 2, 1, 4));

        var k2Data = new float[1 * 2 * 1 * 4];
        Array.Fill(k2Data, 2f);
        var k2 = new Tensor(k2Data, new TensorShape(1, 2, 1, 4));
        var v2 = new Tensor(new float[1 * 2 * 1 * 4], new TensorShape(1, 2, 1, 4));

        _cache.Update(0, k1, v1);
        var (keys, _) = _cache.Update(0, k2, v2);

        // After two updates the seq axis should be 2
        Assert.Equal(2, keys.Shape.Dimensions[2]);
    }

    [Fact]
    public void Clear_ResetsSeqLen()
    {
        var k = new Tensor(new float[1 * 4 * 1 * 8], new TensorShape(1, 4, 1, 8));
        var v = new Tensor(new float[1 * 4 * 1 * 8], new TensorShape(1, 4, 1, 8));
        _cache.Update(0, k, v);
        _cache.Update(0, k, v);

        _cache.Clear();

        Assert.Equal(0, _cache.SeqLen);
    }

    [Fact]
    public void Update_MultipleLayersIndependent()
    {
        var k0 = new Tensor(new float[1 * 2 * 1 * 4], new TensorShape(1, 2, 1, 4));
        var v0 = new Tensor(new float[1 * 2 * 1 * 4], new TensorShape(1, 2, 1, 4));
        var k1Data = new float[1 * 2 * 1 * 4];
        Array.Fill(k1Data, 1f);
        var k1 = new Tensor(k1Data, new TensorShape(1, 2, 1, 4));
        var v1 = new Tensor(k1Data.ToArray(), new TensorShape(1, 2, 1, 4));

        _cache.Update(0, k0, v0);
        _cache.Update(1, k1, v1);

        // Layer 0 updated — SeqLen = 1
        Assert.Equal(1, _cache.SeqLen);
    }
}
