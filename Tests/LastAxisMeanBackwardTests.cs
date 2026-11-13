// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using TensorCore.Core;

// FIX-MB transfer, plan 41-neural-learning-resource-deltas.md, from MB01.
//
// The backward side of averaging the last axis. Every element of a row carries
// the same share of its row's mean: dL/dx[r·D + c] = dL/dmean[r] / D. When the
// averaged axis is the last one and is kept as length one, the source cell for
// element i is just i / D. The previous code decomposed i into coordinates,
// copied them, zeroed one, and folded them back into a flat index — once per
// element of the tensor.
//
// The expected values below come from that derivative, written out by hand,
// not from the implementation.
public sealed class LastAxisMeanBackwardTests
{
    private static Tensor Filled(int[] dims, int seed)
    {
        var shape = new TensorShape(dims);
        var data = new float[shape.TotalElements];
        for (var i = 0; i < data.Length; i++)
            data[i] = -0.41f + (0.017f * ((i + seed) % 149));
        return new Tensor(data, shape);
    }

    [Theory]
    [InlineData(new[] { 6 })]
    [InlineData(new[] { 2, 3 })]
    [InlineData(new[] { 2, 3, 4 })]
    [InlineData(new[] { 2, 2, 2, 5 })]
    [InlineData(new[] { 3, 1 })]
    public void EveryElementCarriesTheSameShareOfItsOwnRow(int[] dims)
    {
        var x = Filled(dims, seed: 3);
        x.RequiresGrad = true;
        var mean = TensorOps.Mean(x, axis: -1, keepDims: true);

        // A different upstream weight per row, so a mistake that reads the
        // wrong row cannot hide behind equal numbers.
        var rowLength = dims[^1];
        var rows = x.Data.Length / rowLength;
        var upstream = new float[rows];
        for (var row = 0; row < rows; row++)
            upstream[row] = -0.6f + (0.37f * row);
        mean.Backward(new Tensor(upstream, mean.Shape));

        Assert.NotNull(x.Grad);
        for (var row = 0; row < rows; row++)
        for (var column = 0; column < rowLength; column++)
        {
            var expected = upstream[row] / rowLength;
            Assert.Equal(expected, x.Grad!.Data[(row * rowLength) + column], 6);
        }
    }

    [Fact]
    public void EarlierAxesStillGoThroughTheGeneralPathAndAgree()
    {
        // Only the last axis is contiguous, so an earlier axis must keep the
        // general path. The upstream below is deliberately different for every
        // output cell: with a uniform upstream each element would receive the
        // same share whichever cell was read, and a wrong source address would
        // be invisible. This is the shape of the mistake the narrow path makes
        // if its axis guard is removed.
        var x = Filled([2, 3, 4], seed: 19);
        x.RequiresGrad = true;
        var mean = TensorOps.Mean(x, axis: 1, keepDims: true);

        var upstream = new float[mean.Shape.TotalElements];
        for (var i = 0; i < upstream.Length; i++)
            upstream[i] = 1f + i;
        mean.Backward(new Tensor(upstream, mean.Shape));

        Assert.NotNull(x.Grad);
        for (var outer = 0; outer < 2; outer++)
        for (var middle = 0; middle < 3; middle++)
        for (var inner = 0; inner < 4; inner++)
        {
            // The averaged axis is the middle one, so the share of element
            // (outer, middle, inner) comes from output cell (outer, 0, inner).
            var expected = upstream[(outer * 4) + inner] / 3f;
            Assert.Equal(expected, x.Grad!.Data[(outer * 12) + (middle * 4) + inner], 6);
        }
    }

    [Fact]
    public void DroppingTheAxisStillThrowsOnBackward_AKnownDefectThisChangeDoesNotFix()
    {
        // Recorded as it is, not as it should be. Dropping the averaged axis
        // leaves the general path building coordinates of the input's rank and
        // handing them to a shape of one rank less, which throws. The defect
        // predates this change, is named in the plan's registry row as out of
        // scope for this factor, and is deliberately left alone here: widening
        // the narrow path to cover it would be a different, unmeasured change.
        //
        // The test exists so the boundary is visible and so that whoever does
        // fix it sees this expectation fail and updates it on purpose.
        var dropped = Filled([3, 5], seed: 23);
        dropped.RequiresGrad = true;
        var mean = TensorOps.Mean(dropped, axis: -1, keepDims: false);

        Assert.Throws<ArgumentException>(() => TensorOps.Sum(mean).Backward());

        // The kept-axis form, which this change does cover, works.
        var kept = Filled([3, 5], seed: 23);
        kept.RequiresGrad = true;
        TensorOps.Sum(TensorOps.Mean(kept, axis: -1, keepDims: true)).Backward();
        foreach (var value in kept.Grad!.Data)
            Assert.Equal(1f / 5f, value, 6);
    }

    [Fact]
    public void AnExistingGradientIsAddedToRatherThanReplaced()
    {
        // The share is written into a fresh buffer and then accumulated onto
        // whatever gradient the tensor already carried. Two backward passes
        // must therefore give twice the share, not the share once.
        var x = Filled([2, 4], seed: 31);
        x.RequiresGrad = true;

        TensorOps.Sum(TensorOps.Mean(x, axis: -1, keepDims: true)).Backward();
        var afterFirst = x.Grad!.Data.ToArray();
        TensorOps.Sum(TensorOps.Mean(x, axis: -1, keepDims: true)).Backward();

        for (var i = 0; i < afterFirst.Length; i++)
        {
            Assert.Equal(1f / 4f, afterFirst[i], 6);
            Assert.Equal(2f / 4f, x.Grad!.Data[i], 6);
        }
    }

    [Fact]
    public void ARowOfOneGivesTheWholeShareToItsSingleElement()
    {
        var x = Filled([4, 1], seed: 41);
        x.RequiresGrad = true;
        var mean = TensorOps.Mean(x, axis: -1, keepDims: true);
        mean.Backward(new Tensor([1f, -2f, 3f, -4f], mean.Shape));

        Assert.Equal(1f, x.Grad!.Data[0], 6);
        Assert.Equal(-2f, x.Grad.Data[1], 6);
        Assert.Equal(3f, x.Grad.Data[2], 6);
        Assert.Equal(-4f, x.Grad.Data[3], 6);
    }
}
