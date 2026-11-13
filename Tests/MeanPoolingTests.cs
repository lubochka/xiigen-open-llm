// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using Embeddings;

namespace LlmFromScratch.Tests;

// Unit tests for MeanPooling — pool and normalize.
// Behavioral assertion gate: output shapes and numeric properties are asserted.
public class MeanPoolingTests
{
    [Fact]
    public void Pool_SingleToken_ReturnsTokenVector()
    {
        // 1 batch, 1 token, 4 dims — mask all 1s
        var tokData = new float[1 * 1 * 4];
        Array.Fill(tokData, 1f);
        var tokens = new Tensor(tokData, new TensorShape(1, 1, 4));

        var maskData = new float[] { 1f };
        var mask = new Tensor(maskData, new TensorShape(1, 1));

        var pooled = MeanPooling.Pool(tokens, mask);

        // Форма должна быть [1, 4]
        Assert.Equal(new int[] { 1, 4 }, pooled.Shape.Dimensions);
        // Все значения должны быть 1.0 (среднее одного единичного токена)
        Assert.All(pooled.Data, v => Assert.Equal(1f, v, precision: 5));
    }

    [Fact]
    public void Pool_TwoTokensEqualMask_ReturnsMean()
    {
        // Batch=1, seq=2, dim=1: токен0=[2], токен1=[4] → среднее=[3]
        var tokens = new Tensor(new float[] { 2f, 4f }, new TensorShape(1, 2, 1));
        var mask   = new Tensor(new float[] { 1f, 1f }, new TensorShape(1, 2));

        var pooled = MeanPooling.Pool(tokens, mask);

        Assert.Equal(3f, pooled.Data[0], precision: 4);
    }

    [Fact]
    public void Pool_PaddingMasked_IgnoresPaddingToken()
    {
        // Batch=1, seq=2, dim=1: токен0=[4], токен1=[99] (padding)
        // mask=[1,0] → только токен 0 → среднее = 4
        var tokens = new Tensor(new float[] { 4f, 99f }, new TensorShape(1, 2, 1));
        var mask   = new Tensor(new float[] { 1f, 0f }, new TensorShape(1, 2));

        var pooled = MeanPooling.Pool(tokens, mask);

        Assert.Equal(4f, pooled.Data[0], precision: 4);
    }

    [Fact]
    public void Normalize_OutputHasUnitL2Norm()
    {
        // Вектор [3, 4] имеет L2-норму 5; после нормализации норма = 1
        var vecs   = new Tensor(new float[] { 3f, 4f }, new TensorShape(1, 2));
        var normed = MeanPooling.Normalize(vecs);

        float x    = normed.Data[0];
        float y    = normed.Data[1];
        float norm = MathF.Sqrt(x * x + y * y);

        Assert.Equal(1f, norm, precision: 5);
    }

    [Fact]
    public void Normalize_OutputShape_Unchanged()
    {
        var onesData = new float[4 * 8];
        Array.Fill(onesData, 1f);
        var vecs   = new Tensor(onesData, new TensorShape(4, 8));
        var normed = MeanPooling.Normalize(vecs);

        Assert.Equal(new int[] { 4, 8 }, normed.Shape.Dimensions);
    }

    [Fact]
    public void Normalize_ZeroVector_DoesNotThrow()
    {
        var vecs = new Tensor(new float[4], new TensorShape(1, 4));
        var ex   = Record.Exception(() => MeanPooling.Normalize(vecs));
        Assert.Null(ex);
    }

    [Fact]
    public void Pool_OutputShape_IsBatchByHidden()
    {
        // Batch=3, seq=5, hidden=16
        var rng    = new Random(42);
        var data   = Enumerable.Range(0, 3 * 5 * 16).Select(_ => (float)rng.NextDouble()).ToArray();
        var tokens = new Tensor(data, new TensorShape(3, 5, 16));

        var maskData = new float[3 * 5];
        Array.Fill(maskData, 1f);
        var mask = new Tensor(maskData, new TensorShape(3, 5));

        var pooled = MeanPooling.Pool(tokens, mask);

        Assert.Equal(new int[] { 3, 16 }, pooled.Shape.Dimensions);
    }
}
