// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using AgentInfrastructure.Logical;
using TensorCore.Core;
using TransformerModel.Config;
using TransformerModel.Layers;

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
    public void SimdOps_CancellableMatVecRow_SuccessMatchesLegacyOutput()
    {
        var a = BuildData(16 * 32, seed: 91);
        var b = BuildData(32 * 24, seed: 92);
        var transposed = SimdOps.Transpose2D(b, rows: 32, cols: 24);
        var legacy = new float[16 * 24 + 6];
        var cancellable = new float[16 * 24 + 6];
        Array.Fill(legacy, -19f);
        Array.Fill(cancellable, -19f);

        SimdOps.MatVecRow(a, 0, 16, 32, transposed, 0, 24, legacy, 3);
        SimdOps.MatVecRow(
            a,
            0,
            16,
            32,
            transposed,
            0,
            24,
            cancellable,
            3,
            CancellationToken.None);

        Assert.Equal(legacy, cancellable);
    }

    [Fact]
    public void SimdOps_CancellableMatVecRow_DoesNotMutateDestinationOnCancellation()
    {
        const int rows = 2;
        const int columns = 1_024;
        const int outputColumns = 128;
        var a = new float[rows * columns];
        var transposed = new float[columns * outputColumns];
        var destination = Enumerable.Repeat(37f, rows * outputColumns).ToArray();
        var expected = destination.ToArray();

        AssertDeterministicMidLoopCancellation(
            "SimdOps.MatVecRow",
            token => SimdOps.MatVecRow(
                a,
                0,
                rows,
                columns,
                transposed,
                0,
                outputColumns,
                destination,
                0,
                token));

        Assert.Equal(expected, destination);
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
    public void TensorOps_MatMul_BackwardVectorTailMatchesNumericalGradients()
    {
        var rows = System.Numerics.Vector<float>.Count + 1;
        const int inner = 5;
        var columns = System.Numerics.Vector<float>.Count + 3;
        var a = new Tensor(BuildData(rows * inner, 301).Select(x => x * 0.1f).ToArray(),
            new TensorShape(rows, inner), requiresGrad: true);
        var b = new Tensor(BuildData(inner * columns, 302).Select(x => x * 0.1f).ToArray(),
            new TensorShape(inner, columns), requiresGrad: true);
        var result = TensorOps.MatMul(a, b);
        var upstream = BuildData(result.Data.Length, 303).Select(x => x * 0.1f).ToArray();

        float[] NumericalGradient(Tensor operand)
        {
            const float epsilon = 1e-3f;
            var gradient = new float[operand.Data.Length];
            double Loss()
            {
                var value = TensorOps.MatMul(a, b);
                var sum = 0d;
                for (var index = 0; index < value.Data.Length; index++)
                    sum += (double)value.Data[index] * upstream[index];
                return sum;
            }

            for (var index = 0; index < operand.Data.Length; index++)
            {
                var original = operand.Data[index];
                operand.Data[index] = original + epsilon;
                var plus = Loss();
                operand.Data[index] = original - epsilon;
                var minus = Loss();
                operand.Data[index] = original;
                gradient[index] = (float)((plus - minus) / (2 * epsilon));
            }
            return gradient;
        }

        var expectedA = NumericalGradient(a);
        var expectedB = NumericalGradient(b);
        result.Backward(new Tensor(upstream, result.Shape));

        AssertClose(expectedA, a.Grad!.Data, 1e-4f);
        AssertClose(expectedB, b.Grad!.Data, 1e-4f);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void TensorOps_MatMul_BackwardReusedRightBatchesAccumulateExistingGradients(
        bool leftRequiresGrad, bool rightRequiresGrad)
    {
        const int batches = 4, rightBatches = 2, rows = 3, inner = 5;
        var columns = System.Numerics.Vector<float>.Count + 3;
        var a = new Tensor(BuildData(batches * rows * inner, 311),
            new TensorShape(batches, rows, inner), leftRequiresGrad);
        var b = new Tensor(BuildData(rightBatches * inner * columns, 312),
            new TensorShape(rightBatches, inner, columns), rightRequiresGrad);
        var existingA = BuildData(a.Data.Length, 313);
        var existingB = BuildData(b.Data.Length, 314);
        if (leftRequiresGrad) a.Grad = new Tensor(existingA, a.Shape);
        if (rightRequiresGrad) b.Grad = new Tensor(existingB, b.Shape);
        var result = TensorOps.MatMul(a, b);
        var upstream = BuildData(result.Data.Length, 315);
        var expectedA = existingA.ToArray();
        var expectedB = existingB.ToArray();
        var deltaA = new float[a.Data.Length];
        var deltaB = new float[b.Data.Length];
        for (var batch = 0; batch < batches; batch++)
        for (var row = 0; row < rows; row++)
        for (var k = 0; k < inner; k++)
        for (var column = 0; column < columns; column++)
        {
            var aIndex = (batch * rows + row) * inner + k;
            var bIndex = ((batch % rightBatches) * inner + k) * columns + column;
            var outputIndex = (batch * rows + row) * columns + column;
            deltaA[aIndex] += upstream[outputIndex] * b.Data[bIndex];
            deltaB[bIndex] += a.Data[aIndex] * upstream[outputIndex];
        }
        for (var i = 0; i < expectedA.Length; i++) expectedA[i] += deltaA[i];
        for (var i = 0; i < expectedB.Length; i++) expectedB[i] += deltaB[i];

        result.Backward(new Tensor(upstream, result.Shape));

        Assert.Equal(leftRequiresGrad || rightRequiresGrad, result.RequiresGrad);
        if (leftRequiresGrad) AssertClose(expectedA, a.Grad!.Data, 1e-5f);
        else Assert.Null(a.Grad);
        if (rightRequiresGrad) AssertClose(expectedB, b.Grad!.Data, 1e-5f);
        else Assert.Null(b.Grad);
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
    public void SimdOps_CancellableTransposeInPlace_SuccessMatchesLegacyOutput()
    {
        var source = BuildData(33 * 47, seed: 107);
        var legacy = Enumerable.Repeat(-7f, source.Length + 8).ToArray();
        var cancellable = legacy.ToArray();

        SimdOps.Transpose2DInPlace(source, 0, 33, 47, legacy, 4);
        SimdOps.Transpose2DInPlace(
            source,
            0,
            33,
            47,
            cancellable,
            4,
            CancellationToken.None);

        Assert.Equal(legacy, cancellable);
    }

    [Fact]
    public void SimdOps_CancellableTransposeInPlace_DoesNotMutateDestinationOnCancellation()
    {
        const int rows = 128;
        const int cols = 1_024;
        var source = new float[rows * cols];
        var destination = Enumerable.Repeat(11f, rows * cols).ToArray();
        var expected = destination.ToArray();

        AssertDeterministicMidLoopCancellation(
            "SimdOps.Transpose2D",
            token => SimdOps.Transpose2DInPlace(
                source,
                0,
                rows,
                cols,
                destination,
                0,
                token));

        Assert.Equal(expected, destination);
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
    public void TensorOps_CancellableTranspose_SuccessMatchesLegacyOutput()
    {
        var source = new Tensor(BuildData(3 * 5 * 7, seed: 131), new TensorShape(3, 5, 7));

        var legacy = TensorOps.Transpose(source, 0, 2);
        var cancellable = TensorOps.Transpose(source, 0, 2, CancellationToken.None);

        Assert.Equal(legacy.Shape.Dimensions, cancellable.Shape.Dimensions);
        Assert.Equal(legacy.Data, cancellable.Data);
    }

    [Fact]
    public void TensorOps_TransposeCancellationInterruptsLargeActiveKernel()
    {
        const int dimension = 32;
        var source = new Tensor(
            new float[dimension * dimension * dimension],
            new TensorShape(dimension, dimension, dimension));
        var expected = source.Data.ToArray();

        AssertDeterministicMidLoopCancellation(
            "TensorOps.Transpose",
            token => TensorOps.Transpose(source, 0, 2, token));

        Assert.Equal(expected, source.Data);
    }

    [Fact]
    public void TensorOps_SoftmaxCancellationInterruptsActiveKernelWithoutMutatingInput()
    {
        const int dimension = 16_384;
        var input = new Tensor(
            BuildData(dimension, seed: 211),
            new TensorShape(1, dimension));

        AssertDeterministicCancellationLeavesInputUnchanged(
            "TensorOps.Softmax",
            input,
            token => TensorOps.Softmax(input, -1, token));
    }

    [Fact]
    public void TensorOps_CausalMaskCancellationInterruptsActiveSquareKernelWithoutMutatingInput()
    {
        const int dimension = 128;
        var input = new Tensor(
            new float[dimension * dimension],
            new TensorShape(1, dimension, dimension));

        AssertDeterministicCancellationLeavesInputUnchanged(
            "TensorOps.ApplyCausalMask",
            input,
            token => TensorOps.ApplyCausalMask(input, float.NegativeInfinity, token));
    }

    [Fact]
    public void TensorOps_CausalMaskWithPastCancellationInterruptsActiveKernelWithoutMutatingInput()
    {
        const int queryLength = 128;
        const int pastKeyLength = 128;
        const int keyLength = 256;
        var input = new Tensor(
            new float[queryLength * keyLength],
            new TensorShape(1, queryLength, keyLength));

        AssertDeterministicCancellationLeavesInputUnchanged(
            "TensorOps.ApplyCausalMask",
            input,
            token => TensorOps.ApplyCausalMaskWithPast(
                input,
                float.NegativeInfinity,
                pastKeyLength,
                token));
    }

    [Fact]
    public void RmsNorm_CancellationInterruptsActiveKernelWithoutMutatingInput()
    {
        const int rows = 1;
        const int hidden = 16_384;
        var input = new Tensor(
            BuildData(rows * hidden, seed: 223),
            new TensorShape(rows, hidden));
        var norm = new RmsNorm(hidden, 1.0e-5f);

        AssertDeterministicCancellationLeavesInputUnchanged(
            "TensorOps.Pow",
            input,
            token => norm.Forward(input, token));
    }

    [Fact]
    public void RotaryEmbedding_CancellationInterruptsActiveKernelWithoutMutatingInput()
    {
        const int heads = 1;
        const int sequence = 64;
        const int dimension = 256;
        var input = new Tensor(
            new float[heads * sequence * dimension],
            new TensorShape(1, heads, sequence, dimension));
        var cos = new Tensor(
            Enumerable.Repeat(1f, sequence * (dimension / 2)).ToArray(),
            new TensorShape(sequence, dimension / 2));
        var sin = new Tensor(
            new float[sequence * (dimension / 2)],
            new TensorShape(sequence, dimension / 2));

        AssertDeterministicCancellationLeavesInputUnchanged(
            "TensorOps.ApplyRotaryEmbedding",
            input,
            token => RotaryEmbedding.Apply(input, cos, sin, token));
    }

    [Fact]
    public async Task TensorKernelProgress_ObserversAreExecutionContextLocal()
    {
        static Task<string[]> ObserveOnWorker(string identity)
            => Task.Run(() =>
            {
                var observations = new List<string>();
                using (TensorKernelProgress.Observe(progress =>
                           observations.Add(
                               $"{identity}:{progress.KernelName}:{progress.CompletedWork}")))
                {
                    _ = TensorOps.Pow(
                        new Tensor(new float[8_192], new TensorShape(1, 8_192)),
                        2f,
                        CancellationToken.None);
                }

                return observations.ToArray();
            });

        var results = await Task.WhenAll(ObserveOnWorker("first"), ObserveOnWorker("second"));

        Assert.NotEmpty(results[0]);
        Assert.NotEmpty(results[1]);
        Assert.All(results[0], value => Assert.StartsWith("first:", value, StringComparison.Ordinal));
        Assert.All(results[1], value => Assert.StartsWith("second:", value, StringComparison.Ordinal));
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

    private static void AssertDeterministicCancellationLeavesInputUnchanged(
        string expectedKernel,
        Tensor input,
        Action<CancellationToken> operation)
    {
        var expected = input.Data.ToArray();
        AssertDeterministicMidLoopCancellation(expectedKernel, operation);
        Assert.Equal(expected, input.Data);
    }

    private static void AssertDeterministicMidLoopCancellation(
        string expectedKernel,
        Action<CancellationToken> operation)
    {
        using var cancellation = new CancellationTokenSource();
        using var progress = new ManualResetEventSlim();
        using var cancellationIssued = new ManualResetEventSlim();
        var observed = 0;
        using var observer = TensorKernelProgress.Observe(kernelProgress =>
        {
            if (!StringComparer.Ordinal.Equals(kernelProgress.KernelName, expectedKernel)
                || Interlocked.Exchange(ref observed, 1) != 0)
            {
                return;
            }

            Assert.Equal(TensorKernelProgressStage.Progress, kernelProgress.Stage);
            Assert.True(kernelProgress.CompletedWork > 0);
            progress.Set();
            if (!cancellationIssued.Wait(TimeSpan.FromSeconds(5)))
                throw new TimeoutException("Cancellation thread did not acknowledge kernel progress.");
        });
        var canceller = new Thread(() =>
        {
            progress.Wait();
            cancellation.Cancel();
            cancellationIssued.Set();
        });
        canceller.Start();

        Exception? failure = null;
        try
        {
            operation(cancellation.Token);
        }
        catch (Exception exception)
        {
            failure = exception;
        }
        finally
        {
            progress.Set();
            Assert.True(canceller.Join(TimeSpan.FromSeconds(5)));
        }

        Assert.Equal(1, Volatile.Read(ref observed));
        Assert.IsType<OperationCanceledException>(failure);
    }

    private static ModelConfig InvokeBuildTinyConfig()
    {
        var method = typeof(DefaultLogicalCapabilityTrainer).GetMethod("BuildTinyConfig", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return Assert.IsType<ModelConfig>(method.Invoke(null, null));
    }
}
