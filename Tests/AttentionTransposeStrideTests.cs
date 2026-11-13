// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using TensorCore.Core;

// FIX-ATS transfer, plan 45-attention-transpose-strides.md, from AT01.
//
// Attention swaps "batch × positions × heads × width" and "batch × heads ×
// positions × width" many times per training step. For that one shape — rank
// four, middle axes — the destination is now computed straight from the four
// sizes, so no coordinate array is built at all. Narrowed on 27.10 to exactly
// the shape the experiment measured (audit finding F3): every other rank and
// axis pair keeps the general path it always had, and the cases below check
// both sides of that border.
//
// The expected values below are computed by explicit nested loops written out
// here, not by calling the operation being tested. That is the point: if the
// addressing were wrong in any way, this gold would disagree.
public sealed class AttentionTransposeStrideTests
{
    private static Tensor Filled(int[] dims, int seed)
    {
        var shape = new TensorShape(dims);
        var data = new float[shape.TotalElements];
        for (var i = 0; i < data.Length; i++)
            data[i] = -0.5f + (0.011f * ((i + seed) % 197));
        return new Tensor(data, shape);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 1)]
    [InlineData(-3, -2)]
    [InlineData(-2, -3)]
    public void SwappingTheMiddleAxesMatchesAnExplicitCoordinateWalk(int first, int second)
    {
        const int A = 2, B = 3, C = 4, D = 5;
        var x = Filled([A, B, C, D], seed: 13);
        var moved = TensorOps.Transpose(x, first, second);

        Assert.Equal([A, C, B, D], moved.Shape.Dimensions);

        // The independent answer, by hand: element (a, b, c, d) of the input is
        // element (a, c, b, d) of the output.
        for (var a = 0; a < A; a++)
        for (var b = 0; b < B; b++)
        for (var c = 0; c < C; c++)
        for (var d = 0; d < D; d++)
        {
            var source = (((((a * B) + b) * C) + c) * D) + d;
            var destination = (((((a * C) + c) * B) + b) * D) + d;
            Assert.Equal(x.Data[source], moved.Data[destination]);
        }
    }

    [Fact]
    public void TheAttentionShapeRoundTripsBackToItself()
    {
        // The real use: heads out, work, heads back. Two swaps must return the
        // original tensor exactly, not merely something of the right shape.
        var x = Filled([2, 7, 3, 8], seed: 29);
        var out1 = TensorOps.Transpose(x, 1, 2);
        var back = TensorOps.Transpose(out1, 1, 2);

        Assert.Equal(x.Shape.Dimensions, back.Shape.Dimensions);
        for (var i = 0; i < x.Data.Length; i++)
        {
            Assert.Equal(
                BitConverter.SingleToInt32Bits(x.Data[i]),
                BitConverter.SingleToInt32Bits(back.Data[i]));
        }
    }

    [Fact]
    public void TheLastTwoAxesStillTakeTheirOwnPathAndAgree()
    {
        // Swapping the final two axes has a separate contiguous path. It must
        // keep working and must agree with the hand walk as well.
        const int A = 2, B = 3, C = 4, D = 5;
        var x = Filled([A, B, C, D], seed: 47);
        var moved = TensorOps.Transpose(x, -2, -1);

        Assert.Equal([A, B, D, C], moved.Shape.Dimensions);
        for (var a = 0; a < A; a++)
        for (var b = 0; b < B; b++)
        for (var c = 0; c < C; c++)
        for (var d = 0; d < D; d++)
        {
            var source = (((((a * B) + b) * C) + c) * D) + d;
            var destination = (((((a * B) + b) * D) + d) * C) + c;
            Assert.Equal(x.Data[source], moved.Data[destination]);
        }
    }

    [Fact]
    public void SwappingAnAxisWithItselfIsACopy()
    {
        var x = Filled([2, 3, 4], seed: 61);
        var same = TensorOps.Transpose(x, 1, 1);

        Assert.Equal(x.Shape.Dimensions, same.Shape.Dimensions);
        Assert.NotSame(x.Data, same.Data);
        for (var i = 0; i < x.Data.Length; i++)
            Assert.Equal(x.Data[i], same.Data[i]);
    }

    [Fact]
    public void TheGradientComesBackThroughTheSameSwap()
    {
        // Backward transposes the upstream the same way, so a gradient placed
        // on one output cell must land on exactly the matching input cell.
        var x = Filled([2, 3, 4, 2], seed: 71);
        x.RequiresGrad = true;
        var moved = TensorOps.Transpose(x, 1, 2);

        var upstream = new float[moved.Shape.TotalElements];
        upstream[((((1 * 4) + 2) * 3) + 1) * 2] = 5f;
        moved.Backward(new Tensor(upstream, moved.Shape));

        Assert.NotNull(x.Grad);
        var expectedCell = ((((1 * 3) + 1) * 4) + 2) * 2;
        for (var i = 0; i < x.Grad!.Data.Length; i++)
            Assert.Equal(i == expectedCell ? 5f : 0f, x.Grad.Data[i]);
    }

    [Fact]
    public void TwoUsesOfTheSameInputAccumulateBothGradients()
    {
        // Repair R2 of the 27.10 transfer audit. Plan 45 assigns this case and
        // the transfer skipped it: one trainable input feeding two swaps whose
        // outputs are added. The input gradient must be the sum of two
        // independent inverse maps, not one of them and not the last one.
        var x = Filled([2, 3, 4, 2], seed: 29);
        x.RequiresGrad = true;
        var first = TensorOps.Transpose(x, 1, 2);
        var second = TensorOps.Transpose(x, 1, 2);
        var joined = TensorOps.Add(first, second);

        // A spread-out upstream rather than all ones: equal values would hide a
        // mapping that moves gradient to the wrong cell.
        var upstream = new float[joined.Shape.TotalElements];
        for (var i = 0; i < upstream.Length; i++)
            upstream[i] = 1f + i;
        joined.Backward(new Tensor(upstream, joined.Shape));

        // The gold is built here by walking the four positions by hand and
        // adding the same cell twice, once per branch.
        const int B = 2, A = 3, C = 4, D = 2;
        var expected = new float[x.Shape.TotalElements];
        for (var b = 0; b < B; b++)
        for (var a = 0; a < A; a++)
        for (var c = 0; c < C; c++)
        for (var d = 0; d < D; d++)
        {
            var source = ((((b * A) + a) * C + c) * D) + d;
            var moved = ((((b * C) + c) * A + a) * D) + d;
            expected[source] += upstream[moved];
            expected[source] += upstream[moved];
        }

        Assert.NotNull(x.Grad);
        for (var i = 0; i < expected.Length; i++)
            Assert.Equal(expected[i], x.Grad!.Data[i], 4);
    }

    [Fact]
    public void CancellingOnTheFirstProgressLeavesTheInputUnchanged()
    {
        // The other case plan 45 assigns and the transfer skipped. The shape is
        // the one the plan names: 2×17×19×23 is 14 858 elements, so the kernel
        // reports progress at 4096 and checks the token right after reporting.
        // Nothing of the half-filled result may reach the caller, and the input
        // must come back byte for byte as it went in.
        var shape = new TensorShape(2, 17, 19, 23);
        var data = new float[shape.TotalElements];
        for (var i = 0; i < data.Length; i++)
            data[i] = -3f + (i % 61) * 0.25f;
        var source = new Tensor(data, shape);
        var before = source.Data.ToArray();

        using var cancellation = new CancellationTokenSource();
        using var observer = TensorKernelProgress.Observe(progress =>
        {
            if (StringComparer.Ordinal.Equals(progress.KernelName, "TensorOps.Transpose")
                && progress.CompletedWork > 0)
                cancellation.Cancel();
        });

        Assert.Throws<OperationCanceledException>(
            () => TensorOps.Transpose(source, 1, 2, cancellation.Token));
        Assert.Equal(before, source.Data);
    }

    [Fact]
    public void RanksOtherThanFourKeepTheGeneralPath()
    {
        // The narrowing means a rank-five middle-axis swap no longer takes the
        // direct formula. It must still be right, otherwise the border between
        // the two paths would be a silent trap.
        const int N = 1, A = 2, C = 3, P = 4, Q = 5;
        var x = Filled([N, A, C, P, Q], seed: 137);
        var moved = TensorOps.Transpose(x, 1, 2);

        Assert.Equal([N, C, A, P, Q], moved.Shape.Dimensions);
        for (var n = 0; n < N; n++)
        for (var a = 0; a < A; a++)
        for (var c = 0; c < C; c++)
        for (var p = 0; p < P; p++)
        for (var q = 0; q < Q; q++)
        {
            var sourceIndex = ((((((n * A) + a) * C + c) * P) + p) * Q) + q;
            var destination = ((((((n * C) + c) * A + a) * P) + p) * Q) + q;
            Assert.Equal(x.Data[sourceIndex], moved.Data[destination]);
        }
    }

    [Fact]
    public void FirstAndLastAxesSwapCorrectlyToo()
    {
        // A non-adjacent swap takes the general path, which the narrowing left
        // untouched. It must still map every element correctly.
        const int A = 3, B = 2, C = 4;
        var x = Filled([A, B, C], seed: 83);
        var moved = TensorOps.Transpose(x, 0, 2);

        Assert.Equal([C, B, A], moved.Shape.Dimensions);
        for (var a = 0; a < A; a++)
        for (var b = 0; b < B; b++)
        for (var c = 0; c < C; c++)
        {
            var source = (((a * B) + b) * C) + c;
            var destination = (((c * B) + b) * A) + a;
            Assert.Equal(x.Data[source], moved.Data[destination]);
        }
    }
}
