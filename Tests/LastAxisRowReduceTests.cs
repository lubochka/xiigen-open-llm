// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using TensorCore.Core;

// FIX-RRD transfer, plan 44-rms-row-reduce.md, from RD01.
//
// Summing the last axis is what normalisation does on every layer of every
// step. The general reduction built a fresh coordinate array for each single
// number it added, then converted those coordinates back into a flat index —
// for numbers that were already lying next to each other in memory.
//
// The narrow path reads the row directly. The order of addition is unchanged,
// so the sums must come out bit for bit identical; that, and the fact that the
// other shapes still go through the general path, is what is checked here.
public sealed class LastAxisRowReduceTests
{
    private static Tensor Filled(int[] dims, int seed)
    {
        var shape = new TensorShape(dims);
        var data = new float[shape.TotalElements];
        for (var i = 0; i < data.Length; i++)
            data[i] = -0.37f + (0.013f * ((i + seed) % 173));
        return new Tensor(data, shape);
    }

    [Theory]
    [InlineData(new[] { 4 })]
    [InlineData(new[] { 3, 5 })]
    [InlineData(new[] { 2, 3, 7 })]
    [InlineData(new[] { 2, 2, 3, 16 })]
    [InlineData(new[] { 1, 1, 1, 1 })]
    public void TheRowPathSumsExactlyWhatTheHandWrittenSumDoes(int[] dims)
    {
        var x = Filled(dims, seed: 11);
        var mean = TensorOps.Mean(x, axis: -1, keepDims: true);

        var rowLength = dims[^1];
        var rows = x.Data.Length / rowLength;
        Assert.Equal(rows, mean.Data.Length);

        for (var row = 0; row < rows; row++)
        {
            // The independent answer: add the row in the same ascending order,
            // then divide. Written here, not taken from the implementation.
            var expected = 0f;
            for (var k = 0; k < rowLength; k++)
                expected += x.Data[(row * rowLength) + k];
            expected /= rowLength;

            Assert.Equal(
                BitConverter.SingleToInt32Bits(expected),
                BitConverter.SingleToInt32Bits(mean.Data[row]));
        }
    }

    [Fact]
    public void TheRowPathIsNotTakenForOtherAxesAndThoseStillAgree()
    {
        // Only the last axis is contiguous. Earlier axes must keep going
        // through the general path, and must keep giving the same answer.
        var x = Filled([2, 3, 4], seed: 29);

        var overMiddle = TensorOps.Mean(x, axis: 1, keepDims: true);
        Assert.Equal([2, 1, 4], overMiddle.Shape.Dimensions);
        for (var outer = 0; outer < 2; outer++)
        for (var inner = 0; inner < 4; inner++)
        {
            var expected = 0f;
            for (var middle = 0; middle < 3; middle++)
                expected += x.Data[(outer * 12) + (middle * 4) + inner];
            expected /= 3;
            Assert.Equal(expected, overMiddle.Data[(outer * 4) + inner], 6);
        }

        var overFirst = TensorOps.Mean(x, axis: 0, keepDims: true);
        Assert.Equal([1, 3, 4], overFirst.Shape.Dimensions);
        for (var i = 0; i < 12; i++)
            Assert.Equal((x.Data[i] + x.Data[i + 12]) / 2f, overFirst.Data[i], 6);
    }

    [Fact]
    public void DroppingTheAxisStillWorksAndMatchesKeepingIt()
    {
        // Without keepDims the result has a lower rank, so the narrow path does
        // not apply; the numbers must still be the same ones.
        var x = Filled([3, 8], seed: 53);
        var kept = TensorOps.Mean(x, axis: -1, keepDims: true);
        var dropped = TensorOps.Mean(x, axis: -1, keepDims: false);

        Assert.Equal([3, 1], kept.Shape.Dimensions);
        Assert.Equal([3], dropped.Shape.Dimensions);
        for (var row = 0; row < 3; row++)
            Assert.Equal(kept.Data[row], dropped.Data[row], 6);
    }

    [Fact]
    public void NormalisationOverTheLastAxisIsUnchangedEndToEnd()
    {
        // The real consumer: a root-mean-square style normalisation reads the
        // mean of the last axis. Its output must be untouched.
        var x = Filled([2, 4, 6], seed: 97);
        var squared = TensorOps.Multiply(x, x);
        var meanSquare = TensorOps.Mean(squared, axis: -1, keepDims: true);

        for (var row = 0; row < 8; row++)
        {
            var expected = 0f;
            for (var k = 0; k < 6; k++)
            {
                var value = x.Data[(row * 6) + k];
                expected += value * value;
            }

            expected /= 6;
            Assert.Equal(
                BitConverter.SingleToInt32Bits(expected),
                BitConverter.SingleToInt32Bits(meanSquare.Data[row]));
        }
    }

    [Fact]
    public void TheGradientOfTheMeanIsUnchanged()
    {
        var x = Filled([2, 5], seed: 7);
        x.RequiresGrad = true;
        var mean = TensorOps.Mean(x, axis: -1, keepDims: true);
        TensorOps.Sum(mean).Backward();

        // Every element of a row carries the same share of its row's mean.
        Assert.NotNull(x.Grad);
        foreach (var value in x.Grad!.Data)
            Assert.Equal(1f / 5f, value, 6);
    }
}
