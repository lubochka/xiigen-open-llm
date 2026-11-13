// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using TransformerModel.Layers;

namespace LlmFromScratch.Tests;

public sealed class RotaryEmbeddingPositionOffsetTests
{
    [Fact]
    public void GetFrequencies_OffsetMatchesCorrespondingRowsOfLongerSequence()
    {
        var rope = new RotaryEmbedding(dim: 8, theta: 10_000f);

        var (fullCos, fullSin) = rope.GetFrequencies(seqLen: 8, positionOffset: 0, CancellationToken.None);
        var (offsetCos, offsetSin) = rope.GetFrequencies(seqLen: 3, positionOffset: 5, CancellationToken.None);

        const int half = 4;
        for (var row = 0; row < 3; row++)
        for (var column = 0; column < half; column++)
        {
            AssertClose(fullCos.Data[((row + 5) * half) + column], offsetCos.Data[(row * half) + column]);
            AssertClose(fullSin.Data[((row + 5) * half) + column], offsetSin.Data[(row * half) + column]);
        }
    }

    [Fact]
    public void GetFrequencies_OffsetUsesAbsolutePositionsInFormula()
    {
        var rope = new RotaryEmbedding(dim: 4, theta: 100f);

        var (cos, sin) = rope.GetFrequencies(seqLen: 2, positionOffset: 2, CancellationToken.None);

        var expectedCos = new[]
        {
            MathF.Cos(2f), MathF.Cos(0.2f),
            MathF.Cos(3f), MathF.Cos(0.3f)
        };
        var expectedSin = new[]
        {
            MathF.Sin(2f), MathF.Sin(0.2f),
            MathF.Sin(3f), MathF.Sin(0.3f)
        };

        AssertArraysClose(expectedCos, cos.Data);
        AssertArraysClose(expectedSin, sin.Data);
    }

    [Fact]
    public void GetFrequencies_ExistingOverloadsAndPositionalDefaultPreserveOffsetZeroBehavior()
    {
        var rope = new RotaryEmbedding(dim: 4, theta: 100f);

        var legacy = rope.GetFrequencies(3);
        var withToken = rope.GetFrequencies(3, CancellationToken.None);
        var withPositionalDefault = rope.GetFrequencies(3, default);
        var explicitOffset = rope.GetFrequencies(3, positionOffset: 0, CancellationToken.None);

        AssertArraysClose(explicitOffset.cos.Data, legacy.cos.Data);
        AssertArraysClose(explicitOffset.sin.Data, legacy.sin.Data);
        AssertArraysClose(explicitOffset.cos.Data, withToken.cos.Data);
        AssertArraysClose(explicitOffset.sin.Data, withToken.sin.Data);
        AssertArraysClose(explicitOffset.cos.Data, withPositionalDefault.cos.Data);
        AssertArraysClose(explicitOffset.sin.Data, withPositionalDefault.sin.Data);
    }

    [Theory]
    [InlineData(0, 0, "seqLen")]
    [InlineData(-1, 0, "seqLen")]
    [InlineData(1, -1, "positionOffset")]
    [InlineData(2, int.MaxValue, "positionOffset")]
    public void GetFrequencies_InvalidRangesFailBeforeAllocation(int seqLen, int positionOffset, string parameterName)
    {
        var rope = new RotaryEmbedding(dim: 4, theta: 100f);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => rope.GetFrequencies(seqLen, positionOffset, CancellationToken.None));

        Assert.Equal(parameterName, exception.ParamName);
    }

    [Fact]
    public void GetFrequencies_BoundaryPositionsThatFitInt32AreAllowed()
    {
        var rope = new RotaryEmbedding(dim: 4, theta: 100f);

        var maxOnly = rope.GetFrequencies(seqLen: 1, positionOffset: int.MaxValue, CancellationToken.None);
        var maxMinusOneAndMax = rope.GetFrequencies(seqLen: 2, positionOffset: int.MaxValue - 1, CancellationToken.None);

        Assert.Equal(new[] { 1, 2 }, maxOnly.cos.Shape.Dimensions);
        Assert.Equal(new[] { 1, 2 }, maxOnly.sin.Shape.Dimensions);
        Assert.Equal(new[] { 2, 2 }, maxMinusOneAndMax.cos.Shape.Dimensions);
        Assert.Equal(new[] { 2, 2 }, maxMinusOneAndMax.sin.Shape.Dimensions);
    }

    [Fact]
    public void Apply_WithOffsetFrequenciesMatchesManualRotationAndTransposeGradient()
    {
        const int batch = 1;
        const int heads = 1;
        const int sequence = 2;
        const int dimension = 4;
        const int half = dimension / 2;
        var rope = new RotaryEmbedding(dimension, theta: 100f);
        var (cos, sin) = rope.GetFrequencies(sequence, positionOffset: 1, CancellationToken.None);
        var x = new Tensor(
            [0.5f, -1f, 1.5f, 2f, -0.25f, 0.75f, -1.25f, 1.75f],
            new TensorShape(batch, heads, sequence, dimension),
            requiresGrad: true);
        var upstream = new[] { 0.2f, -0.4f, 0.6f, -0.8f, 1f, -1.2f, 1.4f, -1.6f };
        var expectedForward = new float[x.Data.Length];
        var expectedGradient = new float[x.Data.Length];

        for (var t = 0; t < sequence; t++)
        for (var i = 0; i < half; i++)
        {
            var firstIndex = (t * dimension) + i;
            var secondIndex = firstIndex + half;
            var frequencyIndex = (t * half) + i;
            var cosine = cos.Data[frequencyIndex];
            var sine = sin.Data[frequencyIndex];
            var inverseFrequency = 1f / MathF.Pow(100f, 2f * i / dimension);
            var angle = (t + 1) * inverseFrequency;
            AssertClose(MathF.Cos(angle), cosine);
            AssertClose(MathF.Sin(angle), sine);
            expectedForward[firstIndex] = x.Data[firstIndex] * cosine - x.Data[secondIndex] * sine;
            expectedForward[secondIndex] = x.Data[firstIndex] * sine + x.Data[secondIndex] * cosine;
            expectedGradient[firstIndex] = upstream[firstIndex] * cosine + upstream[secondIndex] * sine;
            expectedGradient[secondIndex] = -upstream[firstIndex] * sine + upstream[secondIndex] * cosine;
        }

        var output = RotaryEmbedding.Apply(x, cos, sin);
        output.Backward(new Tensor(upstream, output.Shape));

        AssertArraysClose(expectedForward, output.Data);
        Assert.NotNull(x.Grad);
        AssertArraysClose(expectedGradient, x.Grad!.Data);
        Assert.Null(cos.Grad);
        Assert.Null(sin.Grad);
    }

    [Fact]
    public void GetFrequencies_PreCancelledTokenThrowsBeforeAnyWork()
    {
        var rope = new RotaryEmbedding(dim: 4, theta: 100f);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.Throws<OperationCanceledException>(
            () => rope.GetFrequencies(seqLen: 3, positionOffset: 0, cancellation.Token));
    }

    private static void AssertArraysClose(IReadOnlyList<float> expected, IReadOnlyList<float> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (var index = 0; index < expected.Count; index++)
            AssertClose(expected[index], actual[index]);
    }

    private static void AssertClose(float expected, float actual)
    {
        const float absoluteTolerance = 2e-6f;
        const float relativeTolerance = 2e-6f;
        var tolerance = absoluteTolerance + relativeTolerance * MathF.Abs(expected);
        Assert.True(
            MathF.Abs(expected - actual) <= tolerance,
            $"Expected {expected:R}, actual {actual:R}, tolerance {tolerance:R}.");
    }
}
