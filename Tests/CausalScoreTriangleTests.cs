// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using TensorCore.Core;

// FIX-CS transfer, plan 41-neural-learning-resource-deltas.md, from CS01.
//
// In causal attention every score above the diagonal is computed and then
// immediately replaced by negative infinity by the mask. For a sequence of
// length T that is T(T−1)/2 dot products whose result is never read. The narrow
// kernel computes only the visible cells.
//
// The claim that has to hold: after the mask, the two paths are the same —
// same probabilities, same context, same gradients. Not approximately; the
// visible cells are computed by the identical inner loop, so they are bitwise
// equal. That is what is checked below, on the full product as the reference.
public sealed class CausalScoreTriangleTests
{
    // The assigned input shape: deterministic, spread across positive and
    // negative values so a sign error cannot hide.
    private static Tensor Filled(int b, int h, int t, int d, int seed)
    {
        var data = new float[b * h * t * d];
        for (var i = 0; i < data.Length; i++)
            data[i] = -0.2f + (0.007f * ((i + seed) % 211));
        return new Tensor(data, new TensorShape([b, h, t, d]));
    }

    [Theory]
    [InlineData(1, 4, 1, 16)]
    [InlineData(1, 4, 3, 16)]
    [InlineData(1, 4, 21, 16)]
    [InlineData(2, 2, 5, 4)]
    public void VisibleCellsAreBitwiseEqualToTheFullProduct(int batch, int heads, int time, int depth)
    {
        var q = Filled(batch, heads, time, depth, seed: 0);
        var keys = Filled(batch, heads, time, depth, seed: 37);
        var kT = TensorOps.Transpose(keys, -2, -1);

        var full = TensorOps.MatMul(q, kT);
        var causal = TensorOps.CausalScores(q, kT);

        Assert.Equal(full.Shape.Dimensions, causal.Shape.Dimensions);
        for (var row = 0; row < time; row++)
        for (var column = 0; column < time; column++)
        for (var plane = 0; plane < batch * heads; plane++)
        {
            var index = (plane * time * time) + (row * time) + column;
            if (column <= row)
            {
                // Visible: identical to the last bit, not merely close.
                Assert.Equal(
                    BitConverter.SingleToInt32Bits(full.Data[index]),
                    BitConverter.SingleToInt32Bits(causal.Data[index]));
            }
            else
            {
                // Invisible: left at zero, because the mask is about to
                // overwrite it anyway.
                Assert.Equal(0f, causal.Data[index]);
            }
        }
    }

    [Theory]
    [InlineData(1, 4, 21, 16)]
    [InlineData(2, 2, 5, 4)]
    public void AfterTheMaskTheProbabilitiesAndContextAreIdentical(int batch, int heads, int time, int depth)
    {
        var q = Filled(batch, heads, time, depth, seed: 0);
        var keys = Filled(batch, heads, time, depth, seed: 37);
        var values = Filled(batch, heads, time, depth, seed: 91);
        var kT = TensorOps.Transpose(keys, -2, -1);

        var fromFull = Context(TensorOps.MatMul(q, kT), values, depth);
        var fromCausal = Context(TensorOps.CausalScores(q, kT), values, depth);

        for (var i = 0; i < fromFull.Data.Length; i++)
        {
            Assert.Equal(
                BitConverter.SingleToInt32Bits(fromFull.Data[i]),
                BitConverter.SingleToInt32Bits(fromCausal.Data[i]));
        }
    }

    [Fact]
    public void GradientsForQueryAndKeyAreIdenticalThroughTheMask()
    {
        const int Batch = 1, Heads = 2, Time = 7, Depth = 8;

        var (fullQ, fullK) = Gradients(useCausal: false, Batch, Heads, Time, Depth);
        var (causalQ, causalK) = Gradients(useCausal: true, Batch, Heads, Time, Depth);

        // Upstream above the diagonal is zero after the mask, so the narrow
        // kernel's backward must land on exactly the same numbers.
        for (var i = 0; i < fullQ.Length; i++)
            Assert.Equal(fullQ[i], causalQ[i], 6);
        for (var i = 0; i < fullK.Length; i++)
            Assert.Equal(fullK[i], causalK[i], 6);
    }

    [Fact]
    public void TheNarrowKernelRefusesWhatItCannotPromise()
    {
        var square = Filled(1, 2, 4, 4, seed: 0);
        var squareT = TensorOps.Transpose(Filled(1, 2, 4, 4, seed: 5), -2, -1);

        // Rank other than four: not an attention score block.
        var rankThree = new Tensor(new float[8], new TensorShape([2, 2, 2]));
        Assert.Throws<ArgumentException>(() => TensorOps.CausalScores(rankThree, squareT));

        // Different head counts: the pairing would be meaningless.
        var otherHeads = Filled(1, 3, 4, 4, seed: 0);
        Assert.Throws<ArgumentException>(() => TensorOps.CausalScores(otherHeads, squareT));

        // A rectangular block means past keys are present, and then the
        // diagonal is no longer the visibility boundary.
        var longerKeys = TensorOps.Transpose(Filled(1, 2, 6, 4, seed: 5), -2, -1);
        Assert.Throws<ArgumentException>(() => TensorOps.CausalScores(square, longerKeys));

        // Mismatched inner dimension is simply an invalid product.
        var wrongInner = Filled(1, 2, 4, 5, seed: 0);
        Assert.Throws<ArgumentException>(() => TensorOps.CausalScores(wrongInner, squareT));
    }

    private static Tensor Context(Tensor scores, Tensor values, int depth)
    {
        var scaled = TensorOps.Scale(scores, 1f / MathF.Sqrt(depth));
        var masked = TensorOps.ApplyCausalMask(scaled, float.NegativeInfinity);
        var weights = TensorOps.Softmax(masked, dim: -1);
        return TensorOps.MatMul(weights, values);
    }

    private static (float[] Query, float[] Key) Gradients(bool useCausal, int batch, int heads, int time, int depth)
    {
        var q = Filled(batch, heads, time, depth, seed: 0);
        var keys = Filled(batch, heads, time, depth, seed: 37);
        var values = Filled(batch, heads, time, depth, seed: 91);
        q.RequiresGrad = true;
        keys.RequiresGrad = true;

        var kT = TensorOps.Transpose(keys, -2, -1);
        var scores = useCausal ? TensorOps.CausalScores(q, kT) : TensorOps.MatMul(q, kT);
        var context = Context(scores, values, depth);

        // One scalar objective so a single backward pass defines both gradients.
        var loss = TensorOps.Sum(context);
        loss.Backward();

        return (q.Grad!.Data.ToArray(), keys.Grad!.Data.ToArray());
    }
}
