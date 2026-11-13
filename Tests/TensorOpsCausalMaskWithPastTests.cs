// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using TensorCore.Core;

public sealed class TensorOpsCausalMaskWithPastTests
{
    [Fact]
    public void ApplyCausalMaskWithPast_MasksOnlyFutureKeysForEachQueryRow()
    {
        var scores = new Tensor(
            Enumerable.Range(1, 10).Select(value => (float)value).ToArray(),
            new TensorShape(1, 1, 2, 5));

        var masked = TensorOps.ApplyCausalMaskWithPast(scores, fillValue: -99f, pastKeyLength: 3);

        Assert.Equal([1f, 2f, 3f, 4f, -99f, 6f, 7f, 8f, 9f, 10f], masked.Data);
    }

    [Fact]
    public void ApplyCausalMaskWithPast_AllowsEveryKeyForSingleQueryAcrossLeadingMatrices()
    {
        var source = Enumerable.Range(1, 8).Select(value => (float)value).ToArray();
        var scores = new Tensor(source, new TensorShape(1, 2, 1, 4));

        var masked = TensorOps.ApplyCausalMaskWithPast(scores, fillValue: -99f, pastKeyLength: 3);

        Assert.Equal(source, masked.Data);
    }

    [Fact]
    public void ApplyCausalMaskWithPast_BackwardCopiesOnlyAllowedUpstreamGradients()
    {
        var scores = new Tensor(
            Enumerable.Range(1, 10).Select(value => (float)value).ToArray(),
            new TensorShape(1, 1, 2, 5),
            requiresGrad: true);
        var upstream = Enumerable.Range(10, 10).Select(value => (float)value).ToArray();

        var masked = TensorOps.ApplyCausalMaskWithPast(scores, float.NegativeInfinity, pastKeyLength: 3);
        masked.Backward(new Tensor(upstream, masked.Shape));

        Assert.NotNull(scores.Grad);
        Assert.Equal([10f, 11f, 12f, 13f, 0f, 15f, 16f, 17f, 18f, 19f], scores.Grad!.Data);
    }

    [Fact]
    public void ApplyCausalMaskWithPast_RejectsInvalidDimensions()
    {
        var rectangular = new Tensor(new float[10], new TensorShape(1, 2, 5));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            TensorOps.ApplyCausalMaskWithPast(rectangular, float.NegativeInfinity, pastKeyLength: -1));
        Assert.Throws<ArgumentException>(() =>
            TensorOps.ApplyCausalMaskWithPast(new Tensor(new float[5], new TensorShape(5)), float.NegativeInfinity, pastKeyLength: 0));
        Assert.Throws<ArgumentException>(() =>
            TensorOps.ApplyCausalMaskWithPast(rectangular, float.NegativeInfinity, pastKeyLength: 2));
    }

    [Fact]
    public void ApplyCausalMask_OldOverloadsRemainPastZeroSquareOnly()
    {
        var square = new Tensor(
            Enumerable.Range(1, 9).Select(value => (float)value).ToArray(),
            new TensorShape(1, 3, 3));
        var rectangular = new Tensor(new float[10], new TensorShape(1, 2, 5));

        var legacy = TensorOps.ApplyCausalMask(square, float.NegativeInfinity);
        var legacyWithToken = TensorOps.ApplyCausalMask(square, float.NegativeInfinity, CancellationToken.None);
        var explicitPastZero = TensorOps.ApplyCausalMaskWithPast(square, float.NegativeInfinity, pastKeyLength: 0);

        Assert.Equal(explicitPastZero.Data, legacy.Data);
        Assert.Equal(explicitPastZero.Data, legacyWithToken.Data);
        Assert.Throws<ArgumentException>(() => TensorOps.ApplyCausalMask(rectangular, float.NegativeInfinity));
        Assert.Throws<ArgumentException>(() => TensorOps.ApplyCausalMask(rectangular, float.NegativeInfinity, CancellationToken.None));
    }

    [Fact]
    public void ApplyCausalMaskWithPast_DoesNotMutateInput()
    {
        var source = Enumerable.Range(1, 10).Select(value => (float)value).ToArray();
        var scores = new Tensor(source.ToArray(), new TensorShape(1, 1, 2, 5));

        _ = TensorOps.ApplyCausalMaskWithPast(scores, float.NegativeInfinity, pastKeyLength: 3);

        Assert.Equal(source, scores.Data);
    }

    [Fact]
    public void ApplyCausalMask_PositionalDefaultLiteralBindsCancellationTokenSquareOverload()
    {
        var square = new Tensor(
            Enumerable.Range(1, 9).Select(value => (float)value).ToArray(),
            new TensorShape(1, 3, 3));
        var rectangular = new Tensor(new float[10], new TensorShape(1, 2, 5));

        var actual = TensorOps.ApplyCausalMask(square, float.NegativeInfinity, default);
        var expected = TensorOps.ApplyCausalMask(square, float.NegativeInfinity, CancellationToken.None);

        Assert.Equal(expected.Data, actual.Data);
        Assert.Throws<ArgumentException>(() =>
            TensorOps.ApplyCausalMask(rectangular, float.NegativeInfinity, default));
    }

    [Fact]
    public void ApplyCausalMaskWithPast_MasksEveryLeadingMatrixAtItsOwnBaseOffset()
    {
        const int pastKeyLength = 3;
        const int queryLength = 2;
        const int keyLength = 5;
        const int matrixCount = 4;
        var source = Enumerable.Range(1, matrixCount * queryLength * keyLength)
            .Select(value => (float)value)
            .ToArray();
        var scores = new Tensor(source, new TensorShape(2, 2, queryLength, keyLength));

        var masked = TensorOps.ApplyCausalMaskWithPast(scores, fillValue: -99f, pastKeyLength: pastKeyLength);

        for (var matrix = 0; matrix < matrixCount; matrix++)
        for (var row = 0; row < queryLength; row++)
        for (var column = 0; column < keyLength; column++)
        {
            var index = matrix * queryLength * keyLength + row * keyLength + column;
            if (column > pastKeyLength + row)
                Assert.Equal(-99f, masked.Data[index]);
            else
                Assert.Equal(source[index], masked.Data[index]);
        }
    }
}
