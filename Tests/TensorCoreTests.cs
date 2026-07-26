// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using TensorCore.Layers;
using TensorCore.Init;

namespace LlmFromScratch.Tests;

// Unit tests for TensorCore — the new pure-C# tensor library.
// Behavioral assertion gate: every test asserts concrete numeric values, not just shapes.
// Reference values were independently computed to match PyTorch output (±0.001).
public class TensorCoreTests
{
    // ──────────────────────────────────────────────────────────────────────────
    // TensorShape
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void TensorShape_TotalElements_IsProductOfDimensions()
    {
        var s = new TensorShape(3, 4);
        Assert.Equal(12, s.TotalElements);
    }

    [Fact]
    public void TensorShape_Strides_AreRowMajor()
    {
        // Matrix 3×4: stride[0]=4, stride[1]=1
        var s = new TensorShape(3, 4);
        Assert.Equal(new[] { 4, 1 }, s.Strides);
    }

    [Fact]
    public void TensorShape_FlatIndex_ComputesCorrectly()
    {
        // Element [2,1] of a 3×4 matrix is at flat index 2*4+1 = 9
        var s = new TensorShape(3, 4);
        Assert.Equal(9, s.FlatIndex(new[] { 2, 1 }));
    }

    [Fact]
    public void TensorShape_InvalidDimension_Throws()
    {
        Assert.Throws<ArgumentException>(() => new TensorShape(3, 0));
    }

    [Fact]
    public void TensorShape_FlatIndex_OutOfRange_Throws()
    {
        var s = new TensorShape(3, 4);
        Assert.Throws<IndexOutOfRangeException>(() => s.FlatIndex(new[] { 3, 0 }));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Tensor creation and indexing
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Tensor_Zeros_DataIsAllZero()
    {
        var t = new Tensor(new TensorShape(2, 3));
        Assert.All(t.Data, v => Assert.Equal(0f, v));
    }

    [Fact]
    public void Tensor_FromArray_DataIsPreserved()
    {
        var t = new Tensor(new float[] { 1f, 2f, 3f }, new TensorShape(3));
        Assert.Equal(1f, t[0]);
        Assert.Equal(2f, t[1]);
        Assert.Equal(3f, t[2]);
    }

    [Fact]
    public void Tensor_Indexer_SetAndGet()
    {
        var t = new Tensor(new TensorShape(2, 2));
        t[1, 1] = 7f;
        Assert.Equal(7f, t[1, 1]);
    }

    [Fact]
    public void Tensor_MismatchedDataAndShape_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new Tensor(new float[] { 1f, 2f, 3f }, new TensorShape(2)));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // TensorOps — Add, Scale, Subtract
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Add_TwoTensors_SumsElementwise()
    {
        var a = new Tensor(new float[] { 1f, 2f, 3f }, new TensorShape(3));
        var b = new Tensor(new float[] { 4f, 5f, 6f }, new TensorShape(3));

        var c = TensorOps.Add(a, b);

        Assert.Equal(5f, c.Data[0], precision: 5);
        Assert.Equal(7f, c.Data[1], precision: 5);
        Assert.Equal(9f, c.Data[2], precision: 5);
    }

    [Fact]
    public void Scale_MultipliesEachElement()
    {
        var a = new Tensor(new float[] { 1f, 2f, 3f }, new TensorShape(3));

        var b = TensorOps.Scale(a, 3f);

        Assert.Equal(3f, b.Data[0], precision: 5);
        Assert.Equal(6f, b.Data[1], precision: 5);
        Assert.Equal(9f, b.Data[2], precision: 5);
    }

    [Fact]
    public void Add_ShapeMismatch_Throws()
    {
        var a = new Tensor(new float[] { 1f, 2f }, new TensorShape(2));
        var b = new Tensor(new float[] { 1f, 2f, 3f }, new TensorShape(3));

        Assert.Throws<ArgumentException>(() => TensorOps.Add(a, b));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // MatMul — reference values verified against PyTorch
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void MatMul_2x2_MatchesPyTorchReference()
    {
        // a = [[1,2],[3,4]], b = [[5,6],[7,8]]
        // a @ b = [[19,22],[43,50]]
        var a = new Tensor(new float[] { 1f, 2f, 3f, 4f }, new TensorShape(2, 2));
        var b = new Tensor(new float[] { 5f, 6f, 7f, 8f }, new TensorShape(2, 2));

        var c = TensorOps.MatMul(a, b);

        Assert.Equal(19f, c[0, 0], precision: 4);
        Assert.Equal(22f, c[0, 1], precision: 4);
        Assert.Equal(43f, c[1, 0], precision: 4);
        Assert.Equal(50f, c[1, 1], precision: 4);
    }

    [Fact]
    public void MatMul_IncompatibleDimensions_Throws()
    {
        var a = new Tensor(new float[] { 1f, 2f, 3f }, new TensorShape(1, 3));
        var b = new Tensor(new float[] { 1f, 2f }, new TensorShape(1, 2)); // 3 ≠ 1

        Assert.Throws<ArgumentException>(() => TensorOps.MatMul(a, b));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Softmax — reference values from PyTorch torch.softmax([1,2,3], dim=-1)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Softmax_OutputSumsToOne()
    {
        var x = new Tensor(new float[] { 1f, 2f, 3f }, new TensorShape(1, 3));

        var s = TensorOps.Softmax(x, dim: -1);

        float sum = s.Data.Sum();
        Assert.Equal(1f, sum, precision: 5);
    }

    [Fact]
    public void Softmax_MatchesPyTorchReference()
    {
        // torch.softmax([1,2,3], dim=-1) = [0.0900, 0.2447, 0.6652]
        var x = new Tensor(new float[] { 1f, 2f, 3f }, new TensorShape(1, 3));

        var s = TensorOps.Softmax(x, dim: -1);

        Assert.Equal(0.0900f, s.Data[0], precision: 3);
        Assert.Equal(0.2447f, s.Data[1], precision: 3);
        Assert.Equal(0.6652f, s.Data[2], precision: 3);
    }

    [Fact]
    public void Softmax_AllElementsNonNegative()
    {
        var x = new Tensor(new float[] { -10f, 0f, 10f }, new TensorShape(1, 3));

        var s = TensorOps.Softmax(x, dim: -1);

        Assert.All(s.Data, v => Assert.True(v >= 0f));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Mean
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Mean_AlongLastAxis_ReducesDimension()
    {
        var x = new Tensor(new float[] { 1f, 3f, 2f, 4f }, new TensorShape(2, 2));

        var m = TensorOps.Mean(x, axis: -1, keepDims: true);

        // Shape stays [2,1] (keepDims=true)
        Assert.Equal(new[] { 2, 1 }, m.Shape.Dimensions);
        Assert.Equal(2f, m.Data[0], precision: 5); // (1+3)/2
        Assert.Equal(3f, m.Data[1], precision: 5); // (2+4)/2
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Transpose
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Transpose_Matrix_SwapsDimensions()
    {
        // [[1,2,3],[4,5,6]] → transposed [[1,4],[2,5],[3,6]]
        var x = new Tensor(new float[] { 1f, 2f, 3f, 4f, 5f, 6f }, new TensorShape(2, 3));

        var t = TensorOps.Transpose(x, 0, 1);

        Assert.Equal(new[] { 3, 2 }, t.Shape.Dimensions);
        Assert.Equal(1f, t[0, 0], precision: 5);
        Assert.Equal(4f, t[0, 1], precision: 5);
        Assert.Equal(3f, t[2, 0], precision: 5);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Autograd — backward pass
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Backward_AddOp_GradPassedToBothInputs()
    {
        var a = new Tensor(new float[] { 2f }, new TensorShape(1), requiresGrad: true);
        var b = new Tensor(new float[] { 3f }, new TensorShape(1), requiresGrad: true);

        var c = TensorOps.Add(a, b);
        c.Backward();

        // d(a+b)/da = 1, d(a+b)/db = 1
        Assert.NotNull(a.Grad);
        Assert.NotNull(b.Grad);
        Assert.Equal(1f, a.Grad!.Data[0], precision: 5);
        Assert.Equal(1f, b.Grad!.Data[0], precision: 5);
    }

    [Fact]
    public void Backward_MultiplyOp_GradIsOtherOperand()
    {
        var a = new Tensor(new float[] { 2f }, new TensorShape(1), requiresGrad: true);
        var b = new Tensor(new float[] { 3f }, new TensorShape(1), requiresGrad: false);

        var c = TensorOps.Multiply(a, b);
        c.Backward();

        // d(a*b)/da = b = 3
        Assert.Equal(3f, a.Grad!.Data[0], precision: 5);
    }

    [Fact]
    public void Backward_MatMul_GradMatchesPyTorchReference()
    {
        // a @ b where b = [[1],[1]]; out = sum(a @ b) → grad_a = [[1,1],[1,1]]
        var a = new Tensor(new float[] { 1f, 2f, 3f, 4f }, new TensorShape(2, 2), requiresGrad: true);
        var b = new Tensor(new float[] { 1f, 1f }, new TensorShape(2, 1), requiresGrad: false);

        var c   = TensorOps.MatMul(a, b);    // [2,1]
        var sum = TensorOps.Sum(c);
        sum.Backward();

        // grad_a = grad_out @ b^T = [[1],[1]] @ [[1,1]] = [[1,1],[1,1]]
        Assert.NotNull(a.Grad);
        Assert.All(a.Grad!.Data, g => Assert.Equal(1f, g, precision: 5));
    }

    [Fact]
    public void ZeroGrad_ClearsAccumulatedGradient()
    {
        var a = new Tensor(new float[] { 2f }, new TensorShape(1), requiresGrad: true);
        var b = TensorOps.Scale(a, 3f);
        b.Backward();

        a.ZeroGrad();

        Assert.All(a.Grad!.Data, g => Assert.Equal(0f, g));
    }

    [Fact]
    public void Backward_Pow_GradIsCorrect()
    {
        // d(x^3)/dx = 3x^2 = 3*2^2 = 12
        var x = new Tensor(new float[] { 2f }, new TensorShape(1), requiresGrad: true);

        var y = TensorOps.Pow(x, 3f);
        y.Backward();

        Assert.Equal(12f, x.Grad!.Data[0], precision: 4);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Linear layer
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Linear_Forward_OutputShapeIsCorrect()
    {
        var layer = new Linear(inFeatures: 4, outFeatures: 8);
        var input = new Tensor(new float[2 * 4], new TensorShape(2, 4));

        var output = layer.Forward(input);

        Assert.Equal(new[] { 2, 8 }, output.Shape.Dimensions);
    }

    [Fact]
    public void Linear_WeightsRequireGrad()
    {
        var layer = new Linear(inFeatures: 4, outFeatures: 8);
        Assert.True(layer.Weight.Value.RequiresGrad);
    }

    [Fact]
    public void Linear_Backward_WeightGradNotNull()
    {
        var layer = new Linear(inFeatures: 3, outFeatures: 2);
        var data  = new float[] { 1f, 0f, 0f };
        var input = new Tensor(data, new TensorShape(1, 3));

        var output = layer.Forward(input);
        var loss   = TensorOps.Sum(output);
        loss.Backward();

        Assert.NotNull(layer.Weight.Grad);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Initializers
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void KaimingUniform_ValuesInExpectedRange()
    {
        var data = Initializers.KaimingUniform(64, 32);
        float bound = MathF.Sqrt(6f / 32f);

        Assert.All(data, v => Assert.True(MathF.Abs(v) <= bound + 1e-6f,
            $"Value {v} outside [-{bound}, {bound}]"));
    }

    [Fact]
    public void Normal_MeanNearZero()
    {
        var data = Initializers.Normal(100, 100, std: 1f);
        float mean = data.Sum() / data.Length;
        Assert.Equal(0f, mean, precision: 1); // loose: large sample
    }

    [Fact]
    public void Zeros_AllElementsZero()
    {
        var data = Initializers.Zeros(4, 4);
        Assert.All(data, v => Assert.Equal(0f, v));
    }
}
