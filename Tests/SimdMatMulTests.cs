// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using AgentInfrastructure.Logical;
using TensorCore.Core;
using TransformerModel.Config;

public sealed class SimdMatMulTests
{
    [Fact]
    public void SimdOps_MatVecRow_MatchesScalarReference()
    {
        float[] a =
        [
            1, 2, 3,
            4, 5, 6
        ];
        float[] b =
        [
            7, 8,
            9, 10,
            11, 12
        ];
        var transposed = SimdOps.Transpose2D(b, rows: 3, cols: 2);
        var result = new float[4];

        SimdOps.MatVecRow(a, 0, aRows: 2, aCols: 3, transposed, 0, bCols: 2, result, 0);

        Assert.Equal([58f, 64f, 139f, 154f], result);
    }

    [Fact]
    public void TensorOps_MatMul_MatchesScalarReferenceAtAttentionShape()
    {
        var a = BuildData(64 * 32, seed: 11);
        var b = BuildData(32 * 32, seed: 23);
        var result = TensorOps.MatMul(new Tensor(a, new TensorShape(64, 32)), new Tensor(b, new TensorShape(32, 32)));
        var expected = ScalarMatMul(a, 64, 32, b, 32);

        AssertClose(expected, result.Data, tolerance: 1.0e-4f);
    }

    [Fact]
    public void TensorOps_MatMul_MatchesScalarReferenceAtFeedForwardShape()
    {
        var a = BuildData(64 * 32, seed: 31);
        var b = BuildData(32 * 64, seed: 47);
        var result = TensorOps.MatMul(new Tensor(a, new TensorShape(64, 32)), new Tensor(b, new TensorShape(32, 64)));
        var expected = ScalarMatMul(a, 64, 32, b, 64);

        AssertClose(expected, result.Data, tolerance: 1.0e-4f);
    }

    [Fact]
    public void TensorOps_MatMul_MatchesSmallReferenceMatrix()
    {
        var a = new Tensor([1f, 2f, 3f, 4f], new TensorShape(2, 2));
        var b = new Tensor([5f, 6f, 7f, 8f], new TensorShape(2, 2));

        var result = TensorOps.MatMul(a, b);

        Assert.Equal([19f, 22f, 43f, 50f], result.Data);
    }

    [Fact]
    public void TensorOps_MatMul_BackwardGradientsMatchScalarReference()
    {
        var a = new Tensor([1f, 2f, 3f, 4f, 5f, 6f], new TensorShape(2, 3), requiresGrad: true);
        var b = new Tensor([7f, 8f, 9f, 10f, 11f, 12f], new TensorShape(3, 2), requiresGrad: true);

        var loss = TensorOps.Sum(TensorOps.MatMul(a, b));
        loss.Backward();

        AssertClose([15f, 19f, 23f, 15f, 19f, 23f], a.Grad!.Data, tolerance: 1.0e-5f);
        AssertClose([5f, 5f, 7f, 7f, 9f, 9f], b.Grad!.Data, tolerance: 1.0e-5f);
    }

    [Fact]
    public void SimdOps_Transpose2DInPlace_MatchesReference()
    {
        float[] source =
        [
            1, 2, 3,
            4, 5, 6
        ];
        var result = new float[6];

        SimdOps.Transpose2DInPlace(source, 0, rows: 2, cols: 3, result, 0);

        Assert.Equal([1f, 4f, 2f, 5f, 3f, 6f], result);
    }

    [Fact]
    public void TensorOps_Transpose_LastTwoDimensionsFastPathMatchesBatchedReference()
    {
        var source = new Tensor(
            [1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f],
            new TensorShape(2, 2, 3));

        var result = TensorOps.Transpose(source, -2, -1);

        Assert.Equal([2, 3, 2], result.Shape.Dimensions);
        Assert.Equal([1f, 4f, 2f, 5f, 3f, 6f, 7f, 10f, 8f, 11f, 9f, 12f], result.Data);
    }

    [Fact]
    public void DefaultLogicalCapabilityTrainer_ModelConfigStillUsesA58Depth()
    {
        var config = InvokeBuildTinyConfig();

        Assert.Equal(512, config.VocabSize);
        Assert.Equal(64, config.ContextLen);
        Assert.Equal(32, config.HiddenDim);
        Assert.Equal(4, config.NumLayers);
        Assert.Equal(4, config.NumHeads);
        Assert.Equal(2, config.NumKvHeads);
        Assert.Equal(64, config.FfnHiddenDim);
    }

    private static float[] BuildData(int count, int seed)
    {
        var random = new Random(seed);
        var data = new float[count];
        for (var index = 0; index < data.Length; index++)
            data[index] = (float)((random.NextDouble() * 2.0) - 1.0);
        return data;
    }

    private static float[] ScalarMatMul(float[] a, int aRows, int aCols, float[] b, int bCols)
    {
        var result = new float[aRows * bCols];
        for (var row = 0; row < aRows; row++)
        for (var col = 0; col < bCols; col++)
        {
            var sum = 0f;
            for (var k = 0; k < aCols; k++)
                sum += a[(row * aCols) + k] * b[(k * bCols) + col];
            result[(row * bCols) + col] = sum;
        }

        return result;
    }

    private static void AssertClose(float[] expected, float[] actual, float tolerance)
    {
        Assert.Equal(expected.Length, actual.Length);
        for (var index = 0; index < expected.Length; index++)
            Assert.True(Math.Abs(expected[index] - actual[index]) <= tolerance, $"index={index}; expected={expected[index]}; actual={actual[index]}");
    }

    private static ModelConfig InvokeBuildTinyConfig()
    {
        var method = typeof(DefaultLogicalCapabilityTrainer).GetMethod("BuildTinyConfig", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return Assert.IsType<ModelConfig>(method.Invoke(null, null));
    }
}
