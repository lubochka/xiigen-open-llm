// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using LoRA;
using TensorCore.Core;
using TensorCore.Layers;
using TransformerModel;
using TransformerModel.Config;
using TransformerModel.Layers;

namespace LlmFromScratch.Tests;

public class LoraTests
{
    [Fact]
    public void LoraApplicator_InvalidConfigurationsFailBeforeModelMutation()
    {
        var invalidCases = new (LoraConfig? Config, string Code)[]
        {
            (null, "LORA_CONFIG_REQUIRED"),
            (new LoraConfig { Rank = 0 }, "LORA_RANK_INVALID"),
            (new LoraConfig { Rank = -1 }, "LORA_RANK_INVALID"),
            (new LoraConfig { Alpha = float.NaN }, "LORA_ALPHA_INVALID"),
            (new LoraConfig { Alpha = float.PositiveInfinity }, "LORA_ALPHA_INVALID"),
            (new LoraConfig { Alpha = float.NegativeInfinity }, "LORA_ALPHA_INVALID"),
            (new LoraConfig { Dropout = float.NaN }, "LORA_DROPOUT_INVALID"),
            (new LoraConfig { Dropout = float.PositiveInfinity }, "LORA_DROPOUT_INVALID"),
            (new LoraConfig { Dropout = -0.1f }, "LORA_DROPOUT_INVALID"),
            (new LoraConfig { Dropout = 1f }, "LORA_DROPOUT_INVALID"),
            (new LoraConfig { TargetModules = null! }, "LORA_TARGET_MODULES_REQUIRED"),
            (new LoraConfig { TargetModules = [null!] }, "LORA_TARGET_MODULE_INVALID"),
            (new LoraConfig { TargetModules = ["Q_PROJ"] }, "LORA_TARGET_MODULE_INVALID"),
            (new LoraConfig { TargetModules = [" q_proj"] }, "LORA_TARGET_MODULE_INVALID"),
            (new LoraConfig { TargetModules = ["proj"] }, "LORA_TARGET_MODULE_INVALID")
        };

        foreach (var (config, code) in invalidCases)
        {
            var model = new LlmModel(TinyConfig());
            var before = CaptureModel(model);
            var exception = Assert.ThrowsAny<Exception>(() =>
                LoraApplicator.Apply(model, config!));
            Assert.Contains(code, exception.Message, StringComparison.Ordinal);
            AssertModelSnapshot(before, model);
        }
    }

    [Fact]
    public void LoraApplicator_EmptyTargetsIsNoOp()
    {
        var model = new LlmModel(TinyConfig());
        var before = CaptureModel(model);

        var adapters = LoraApplicator.Apply(
            model,
            new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = [] });

        Assert.Empty(adapters);
        AssertModelSnapshot(before, model);
        Assert.All(model.BaseParameters, parameter => Assert.True(parameter.Value.RequiresGrad));
    }

    [Fact]
    public void LoraLinear_Construction_DoesNotMutateBase()
    {
        var baseProjection = new Linear(4, 3, useBias: true);
        baseProjection.Weight.Value.Grad =
            new Tensor(Enumerable.Repeat(7f, 12).ToArray(), baseProjection.Weight.Value.Shape);
        baseProjection.Bias!.Value.Grad =
            new Tensor(Enumerable.Repeat(5f, 3).ToArray(), baseProjection.Bias.Value.Shape);
        var beforeWeight = CaptureParameter(baseProjection.Weight);
        var beforeBias = CaptureParameter(baseProjection.Bias);

        var adapter = new LoraLinear(
            baseProjection,
            new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0 });

        AssertParameterSnapshot(beforeWeight, baseProjection.Weight);
        AssertParameterSnapshot(beforeBias, baseProjection.Bias);
        Assert.Equal([adapter.AdapterParameters[0].Parameter, adapter.AdapterParameters[1].Parameter],
            adapter.Parameters().ToArray());
        Assert.DoesNotContain(baseProjection.Weight, adapter.Parameters());
        Assert.DoesNotContain(baseProjection.Bias, adapter.Parameters());
    }

    [Fact]
    public void LoraApplicator_IncrementalVThenQv_HasSameCanonicalOrderAsFreshQv()
    {
        var incremental = new LlmModel(TinyConfig());
        var baseCount = incremental.BaseParameters.Count;
        var vOnly = LoraApplicator.Apply(incremental, Config(["v_proj"]));
        var retainedV = Assert.Single(vOnly);

        var incrementalQv = LoraApplicator.Apply(
            incremental,
            Config(["v_proj", "q_proj", "q_proj"]));
        Assert.Same(retainedV, incrementalQv[1]);

        var fresh = new LlmModel(TinyConfig());
        var freshQv = LoraApplicator.Apply(fresh, Config(["q_proj", "v_proj"]));
        Assert.Equal(2, freshQv.Count);

        var incrementalEntries = incremental.GetCheckpointParameterEntries()
            .Skip(baseCount)
            .Select(entry => (entry.Key, Shape: string.Join(",", entry.Parameter.Value.Shape.Dimensions)))
            .ToArray();
        var freshEntries = fresh.GetCheckpointParameterEntries()
            .Skip(fresh.BaseParameters.Count)
            .Select(entry => (entry.Key, Shape: string.Join(",", entry.Parameter.Value.Shape.Dimensions)))
            .ToArray();

        Assert.Equal(freshEntries, incrementalEntries);
        Assert.Equal(
            [
                "lora.block_0000.q_proj.lora_a",
                "lora.block_0000.q_proj.lora_b",
                "lora.block_0000.v_proj.lora_a",
                "lora.block_0000.v_proj.lora_b"
            ],
            incrementalEntries.Select(entry => entry.Key).ToArray());
        Assert.Equal(baseCount + 4, incremental.Parameters().Count());
        Assert.Null(incremental.GetAttachedProjectionAdapter(0, "k_proj"));
        Assert.Null(incremental.GetAttachedProjectionAdapter(0, "o_proj"));
    }

    [Fact]
    public void LoraApplicator_ValidApplyFreezesBaseAndOwnsOnlyCanonicalAdapterParameters()
    {
        var model = new LlmModel(TinyConfig());
        var baseReferences = model.BaseParameters.ToArray();

        var adapters = LoraApplicator.Apply(model, Config(["v_proj", "q_proj"]));
        var all = model.Parameters().ToArray();

        Assert.Equal(2, adapters.Count);
        Assert.Equal(baseReferences, all.Take(baseReferences.Length).ToArray());
        Assert.All(baseReferences, parameter => Assert.False(parameter.Value.RequiresGrad));
        Assert.Equal(baseReferences.Length + 4, all.Length);
        Assert.Equal(all.Length, all.Distinct(ReferenceEqualityComparer.Instance).Count());
        Assert.All(all.Skip(baseReferences.Length), parameter => Assert.True(parameter.Value.RequiresGrad));
        Assert.All(adapters, adapter => Assert.Equal(2, adapter.Parameters().Count()));
    }

    [Fact]
    public void LoraApplicator_SameConfigurationReusesAndReenablesAdapters()
    {
        var model = new LlmModel(TinyConfig());
        var first = LoraApplicator.Apply(model, Config(["q_proj", "v_proj"]));
        var count = model.Parameters().Count();
        first[0].AdapterParameters[0].Parameter.Value.RequiresGrad = false;

        var second = LoraApplicator.Apply(model, Config(["v_proj", "q_proj"]));

        Assert.Equal(count, model.Parameters().Count());
        Assert.All(first.Zip(second), pair => Assert.Same(pair.First, pair.Second));
        Assert.All(second.SelectMany(adapter => adapter.AdapterParameters),
            pair => Assert.True(pair.Parameter.Value.RequiresGrad));
    }

    [Fact]
    public void LoraApplicator_DifferentConfigurationFailsWithoutMutation()
    {
        var model = new LlmModel(TinyConfig());
        _ = LoraApplicator.Apply(model, Config(["q_proj"]));
        var before = CaptureModel(model);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            LoraApplicator.Apply(
                model,
                new LoraConfig { Rank = 4, Alpha = 4, Dropout = 0, TargetModules = ["q_proj"] }));

        Assert.Contains("LORA_ADAPTER_CONFIG_MISMATCH", exception.Message, StringComparison.Ordinal);
        AssertModelSnapshot(before, model);
    }

    [Fact]
    public void LoraApplicator_LateSlotConflict_DoesNotFreezeOrPartiallyAttach()
    {
        var model = new LlmModel(TinyConfig());
        RecordingAdapter? injected = null;
        model.ExecuteAdapterMutation(lease =>
        {
            var state = lease.InspectProjection(0, "v_proj");
            injected = new RecordingAdapter(state.BaseProjection);
            lease.PopulateAdapterPair(0, "v_proj", injected);
            return 0;
        });
        var before = CaptureModel(model);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            LoraApplicator.Apply(model, Config(["q_proj", "v_proj"])));

        Assert.Contains("LORA_PROJECTION_ADAPTER_CONFLICT", exception.Message, StringComparison.Ordinal);
        Assert.Null(model.GetAttachedProjectionAdapter(0, "q_proj"));
        Assert.Same(injected, model.GetAttachedProjectionAdapter(0, "v_proj"));
        AssertModelSnapshot(before, model);
    }

    [Fact]
    public void LoraApplicator_RetainedLeaseCannotMutateOutsideModelLock()
    {
        var model = new LlmModel(TinyConfig());
        LlmModel.AdapterMutationLease? retained = null;
        model.ExecuteAdapterMutation(lease =>
        {
            retained = lease;
            _ = lease.InspectProjection(0, "q_proj");
            return 0;
        });
        var before = CaptureModel(model);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            retained!.InspectProjection(0, "q_proj"));

        Assert.Contains("LORA_MUTATION_LEASE_INVALID", exception.Message, StringComparison.Ordinal);
        AssertModelSnapshot(before, model);
    }

    [Fact]
    public async Task LoraApplicator_ConcurrentIncompatibleApply_SerializesWithoutPartialState()
    {
        var model = new LlmModel(TinyConfig());
        using var aInside = new ManualResetEventSlim();
        using var releaseA = new ManualResetEventSlim();
        using var bBefore = new ManualResetEventSlim();
        using var bInside = new ManualResetEventSlim();

        var taskA = Task.Run(() => LoraApplicator.Apply(
            model,
            Config(["q_proj", "v_proj"]),
            phase =>
            {
                if (phase == LoraApplyPhase.AfterCandidateConstruction)
                {
                    aInside.Set();
                    Assert.True(releaseA.Wait(TimeSpan.FromSeconds(10)));
                }
            }));
        Assert.True(aInside.Wait(TimeSpan.FromSeconds(10)));

        var taskB = Task.Run(() => Record.Exception(() => LoraApplicator.Apply(
            model,
            new LoraConfig { Rank = 4, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] },
            phase =>
            {
                if (phase == LoraApplyPhase.BeforeModelLock)
                    bBefore.Set();
                if (phase == LoraApplyPhase.InsideModelLock)
                    bInside.Set();
            })));
        Assert.True(bBefore.Wait(TimeSpan.FromSeconds(10)));
        Assert.False(bInside.Wait(TimeSpan.FromMilliseconds(150)));

        releaseA.Set();
        var winner = await taskA;
        var loser = await taskB;

        Assert.True(bInside.IsSet);
        Assert.NotNull(loser);
        Assert.Contains("LORA_ADAPTER_CONFIG_MISMATCH", loser!.Message, StringComparison.Ordinal);
        Assert.Equal(2, winner.Count);
        Assert.All(winner, adapter => Assert.Equal(2, adapter.Rank));
        Assert.All(model.BaseParameters, parameter => Assert.False(parameter.Value.RequiresGrad));
        Assert.Equal(
            [
                "lora.block_0000.q_proj.lora_a",
                "lora.block_0000.q_proj.lora_b",
                "lora.block_0000.v_proj.lora_a",
                "lora.block_0000.v_proj.lora_b"
            ],
            model.GetCheckpointParameterEntries()
                .Skip(model.BaseParameters.Count)
                .Select(entry => entry.Key)
                .ToArray());
    }

    [Fact]
    public void LoraLinear_AnswerModeUsesUndroppedBasisDelta()
    {
        var baseProjection = new Linear(2, 2, useBias: false);
        Array.Clear(baseProjection.Weight.Value.Data);
        var adapter = new LoraLinear(
            baseProjection,
            new LoraConfig { Rank = 1, Alpha = 1, Dropout = 0.5f });
        SetBasis(adapter, inputIndex: 0, outputIndex: 1);
        var input = new Tensor([1f, 0f], new TensorShape(1, 2));

        var answer = adapter.Forward(input);
        var training = adapter.Forward(input, training: true);

        Assert.Equal(0f, answer.Data[0]);
        Assert.Equal(1f, answer.Data[1]);
        Assert.True(training.Data[1] is 0f or 2f);
        Assert.NotEqual(1f, training.Data[1]);
    }

    [Fact]
    public void LlmModel_ForwardModeReachesAttachedProjection()
    {
        var model = new LlmModel(TinyConfig());
        RecordingAdapter? recorder = null;
        model.ExecuteAdapterMutation(lease =>
        {
            var state = lease.InspectProjection(0, "q_proj");
            recorder = new RecordingAdapter(state.BaseProjection);
            lease.PopulateAdapterPair(0, "q_proj", recorder);
            return 0;
        });
        var ids = new Tensor([1f], new TensorShape(1, 1));

        _ = model.Forward(ids);
        Assert.False(recorder!.LastTraining);
        _ = model.Forward(ids, training: true);
        Assert.True(recorder.LastTraining);
    }

    [Fact]
    public void LlmModel_NormalForwardUsesLiveAdapterDelta()
    {
        var model = new LlmModel(TinyConfig(equalProjectionShapes: true));
        PrepareControlledSingleTokenModel(model);
        var ids = new Tensor([1f], new TensorShape(1, 1));
        var before = model.Forward(ids).Data.ToArray();
        var adapter = Assert.Single(LoraApplicator.Apply(model, Config(["v_proj"])));
        Assert.Equal(before, model.Forward(ids).Data);

        SetBasis(adapter, inputIndex: 0, outputIndex: 0);
        var after = model.Forward(ids).Data;

        var firstNormScale = 1f / MathF.Sqrt(0.25f + model.Config.RmsNormEps);
        var liveAttentionComponent =
            firstNormScale + adapter.Scaling * firstNormScale;
        var liveResidual = 1f + liveAttentionComponent;
        var expectedFirstLogit =
            liveResidual
            / MathF.Sqrt((liveResidual * liveResidual / model.Config.HiddenDim)
                         + model.Config.RmsNormEps);

        Assert.Equal(3f * firstNormScale, liveAttentionComponent);
        Assert.Equal(expectedFirstLogit, after[0]);
        Assert.All(after.Skip(1), value => Assert.Equal(0f, value));
        Assert.NotEqual(before[0], after[0]);
    }

    [Fact]
    public void AttentionLayout_ReshapeTransposeRoundTripPreservesValuesAndGradient()
    {
        const int batch = 2;
        const int sequence = 3;
        const int heads = 2;
        const int headDimension = 4;
        var values = Enumerable.Range(0, batch * sequence * heads * headDimension)
            .Select(index => -0.7f + index * 0.037f)
            .ToArray();
        var x = new Tensor(
            values,
            new TensorShape(batch, sequence, heads * headDimension),
            requiresGrad: true);

        var split = TensorOps.Transpose(
            TensorOps.Reshape(x, batch, sequence, heads, headDimension),
            1,
            2);
        var expectedSplit = new float[values.Length];
        for (var b = 0; b < batch; b++)
        for (var h = 0; h < heads; h++)
        for (var t = 0; t < sequence; t++)
        for (var d = 0; d < headDimension; d++)
        {
            var source = ((b * sequence + t) * heads + h) * headDimension + d;
            var destination = ((b * heads + h) * sequence + t) * headDimension + d;
            expectedSplit[destination] = values[source];
        }

        Assert.Equal([batch, heads, sequence, headDimension], split.Shape.Dimensions);
        AssertArraysClose(expectedSplit, split.Data, absoluteTolerance: 0f, relativeTolerance: 0f);

        var roundTrip = TensorOps.Reshape(
            TensorOps.Transpose(split, 1, 2),
            batch,
            sequence,
            heads * headDimension);
        AssertArraysClose(values, roundTrip.Data, absoluteTolerance: 0f, relativeTolerance: 0f);

        var upstream = Enumerable.Range(0, values.Length)
            .Select(index => 0.013f * (index + 1) - 0.21f)
            .ToArray();
        roundTrip.Backward(new Tensor(upstream, roundTrip.Shape));

        Assert.NotNull(x.Grad);
        Assert.Equal(x.Shape.Dimensions, x.Grad!.Shape.Dimensions);
        AssertArraysClose(upstream, x.Grad.Data, absoluteTolerance: 0f, relativeTolerance: 0f);
    }

    [Fact]
    public void TensorOps_MatMulBroadcastBackwardReducesAllLeadingMatrices()
    {
        var aValues = new[]
        {
            -0.8f, 0.3f, 0.5f, -0.2f, 0.9f, 0.4f,
            -0.6f, 1.1f, 0.2f, 0.7f, -0.4f, 0.8f
        };
        var bValues = new[] { 0.7f, -0.5f, 0.2f, 1.3f };
        var upstream = new[]
        {
            0.2f, -0.1f, 0.4f, 0.7f, -0.3f, 0.5f,
            -0.6f, 0.8f, 0.9f, -0.2f, 0.1f, -0.4f
        };
        var a = new Tensor(aValues, new TensorShape(2, 3, 2), requiresGrad: true);
        var b = new Tensor(bValues, new TensorShape(2, 2), requiresGrad: true);
        var result = TensorOps.MatMul(a, b);

        var expectedForward = new float[12];
        var expectedA = new float[12];
        var expectedB = new float[4];
        for (var batch = 0; batch < 2; batch++)
        for (var row = 0; row < 3; row++)
        for (var column = 0; column < 2; column++)
        {
            var outputIndex = (batch * 3 + row) * 2 + column;
            for (var inner = 0; inner < 2; inner++)
            {
                var aIndex = (batch * 3 + row) * 2 + inner;
                var bIndex = inner * 2 + column;
                expectedForward[outputIndex] += aValues[aIndex] * bValues[bIndex];
                expectedA[aIndex] += upstream[outputIndex] * bValues[bIndex];
                expectedB[bIndex] += aValues[aIndex] * upstream[outputIndex];
            }
        }

        AssertArraysClose(expectedForward, result.Data, 1e-6f, 1e-6f);

        const float epsilon = 1e-3f;
        var numericalB = new float[b.Data.Length];
        for (var index = 0; index < b.Data.Length; index++)
        {
            var original = b.Data[index];
            b.Data[index] = original + epsilon;
            var plus = WeightedSum(TensorOps.MatMul(a, b), upstream);
            b.Data[index] = original - epsilon;
            var minus = WeightedSum(TensorOps.MatMul(a, b), upstream);
            b.Data[index] = original;
            numericalB[index] = (plus - minus) / (2f * epsilon);
        }

        result.Backward(new Tensor(upstream, result.Shape));

        Assert.NotNull(a.Grad);
        Assert.NotNull(b.Grad);
        Assert.Equal(a.Shape.Dimensions, a.Grad!.Shape.Dimensions);
        Assert.Equal(b.Shape.Dimensions, b.Grad!.Shape.Dimensions);
        AssertArraysClose(expectedA, a.Grad.Data, 1e-6f, 1e-6f);
        AssertArraysClose(expectedB, b.Grad.Data, 1e-6f, 1e-6f);
        AssertArraysClose(numericalB, b.Grad.Data, 2e-4f, 2e-3f);
    }

    [Fact]
    public void TensorOps_RepeatInterleaveBackwardSumsRepeatedHeads()
    {
        var values = new[] { -0.8f, 0.2f, 0.6f, -0.4f, 1.1f, 0.3f, -0.7f, 0.9f };
        var x = new Tensor(values, new TensorShape(1, 2, 2, 2), requiresGrad: true);
        var repeated = TensorOps.RepeatInterleave(x, repeats: 2, dim: 1);
        var expectedForward = new float[16];
        for (var head = 0; head < 2; head++)
        for (var repeat = 0; repeat < 2; repeat++)
        for (var sequence = 0; sequence < 2; sequence++)
        for (var dimension = 0; dimension < 2; dimension++)
        {
            var source = (head * 2 + sequence) * 2 + dimension;
            var destination = ((head * 2 + repeat) * 2 + sequence) * 2 + dimension;
            expectedForward[destination] = values[source];
        }

        Assert.Equal([1, 4, 2, 2], repeated.Shape.Dimensions);
        AssertArraysClose(expectedForward, repeated.Data, 0f, 0f);

        var upstream = Enumerable.Range(0, repeated.Data.Length)
            .Select(index => -0.23f + index * 0.041f)
            .ToArray();
        var expectedGradient = new float[values.Length];
        for (var head = 0; head < 2; head++)
        for (var repeat = 0; repeat < 2; repeat++)
        for (var sequence = 0; sequence < 2; sequence++)
        for (var dimension = 0; dimension < 2; dimension++)
        {
            var source = (head * 2 + sequence) * 2 + dimension;
            var destination = ((head * 2 + repeat) * 2 + sequence) * 2 + dimension;
            expectedGradient[source] += upstream[destination];
        }

        const float epsilon = 1e-3f;
        var numerical = NumericalGradient(
            x,
            () => WeightedSum(TensorOps.RepeatInterleave(x, repeats: 2, dim: 1), upstream),
            epsilon);
        repeated.Backward(new Tensor(upstream, repeated.Shape));

        Assert.NotNull(x.Grad);
        Assert.Equal(x.Shape.Dimensions, x.Grad!.Shape.Dimensions);
        AssertArraysClose(expectedGradient, x.Grad.Data, 1e-6f, 1e-6f);
        AssertArraysClose(numerical, x.Grad.Data, 2e-4f, 2e-3f);
    }

    [Fact]
    public void RotaryEmbedding_BackwardMatchesTransposeRotationAndFiniteDifference()
    {
        const int batch = 1;
        const int heads = 2;
        const int sequence = 3;
        const int dimension = 4;
        const int half = dimension / 2;
        var x = new Tensor(
            Enumerable.Range(0, batch * heads * sequence * dimension)
                .Select(index => -0.9f + 0.073f * index)
                .ToArray(),
            new TensorShape(batch, heads, sequence, dimension),
            requiresGrad: true);
        var cos = new Tensor(
            [0.91f, 0.73f, 0.62f, -0.48f, -0.35f, 0.84f],
            new TensorShape(sequence, half),
            requiresGrad: true);
        var sin = new Tensor(
            [0.17f, -0.41f, 0.78f, 0.31f, -0.66f, 0.29f],
            new TensorShape(sequence, half),
            requiresGrad: true);
        var rotated = RotaryEmbedding.Apply(x, cos, sin);
        var expectedForward = new float[x.Data.Length];
        var upstream = Enumerable.Range(0, x.Data.Length)
            .Select(index => 0.19f - index * 0.017f)
            .ToArray();
        var expectedGradient = new float[x.Data.Length];
        for (var b = 0; b < batch; b++)
        for (var h = 0; h < heads; h++)
        for (var t = 0; t < sequence; t++)
        for (var i = 0; i < half; i++)
        {
            var firstIndex = ((b * heads + h) * sequence + t) * dimension + i;
            var secondIndex = firstIndex + half;
            var frequencyIndex = t * half + i;
            expectedForward[firstIndex] =
                x.Data[firstIndex] * cos.Data[frequencyIndex]
                - x.Data[secondIndex] * sin.Data[frequencyIndex];
            expectedForward[secondIndex] =
                x.Data[firstIndex] * sin.Data[frequencyIndex]
                + x.Data[secondIndex] * cos.Data[frequencyIndex];
            expectedGradient[firstIndex] =
                upstream[firstIndex] * cos.Data[frequencyIndex]
                + upstream[secondIndex] * sin.Data[frequencyIndex];
            expectedGradient[secondIndex] =
                -upstream[firstIndex] * sin.Data[frequencyIndex]
                + upstream[secondIndex] * cos.Data[frequencyIndex];
        }

        AssertArraysClose(expectedForward, rotated.Data, 1e-6f, 1e-6f);
        var numerical = NumericalGradient(
            x,
            () => WeightedSum(RotaryEmbedding.Apply(x, cos, sin), upstream),
            epsilon: 1e-3f);
        rotated.Backward(new Tensor(upstream, rotated.Shape));

        Assert.NotNull(x.Grad);
        Assert.Equal(x.Shape.Dimensions, x.Grad!.Shape.Dimensions);
        AssertArraysClose(expectedGradient, x.Grad.Data, 1e-6f, 1e-6f);
        AssertArraysClose(numerical, x.Grad.Data, 3e-4f, 3e-3f);
        Assert.Null(cos.Grad);
        Assert.Null(sin.Grad);
    }

    [Fact]
    public void RotaryEmbedding_OddFinalDimensionPreservesPriorUnpairedZeroAndZeroGradient()
    {
        const int batch = 1;
        const int heads = 2;
        const int sequence = 2;
        const int dimension = 5;
        const int half = dimension / 2;
        var x = new Tensor(
            Enumerable.Range(0, batch * heads * sequence * dimension)
                .Select(index => -0.8f + 0.09f * index)
                .ToArray(),
            new TensorShape(batch, heads, sequence, dimension),
            requiresGrad: true);
        var cos = new Tensor(
            [0.87f, -0.43f, 0.61f, 0.72f],
            new TensorShape(sequence, half),
            requiresGrad: true);
        var sin = new Tensor(
            [0.31f, 0.56f, -0.28f, 0.49f],
            new TensorShape(sequence, half),
            requiresGrad: true);
        var upstream = Enumerable.Range(0, x.Data.Length)
            .Select(index => -0.3f + 0.047f * index)
            .ToArray();
        var expectedForward = new float[x.Data.Length];
        var expectedGradient = new float[x.Data.Length];

        for (var b = 0; b < batch; b++)
        for (var h = 0; h < heads; h++)
        for (var t = 0; t < sequence; t++)
        {
            var baseOffset = ((b * heads + h) * sequence + t) * dimension;
            var frequencyOffset = t * half;
            for (var i = 0; i < half; i++)
            {
                var firstIndex = baseOffset + i;
                var secondIndex = baseOffset + half + i;
                var cosine = cos.Data[frequencyOffset + i];
                var sine = sin.Data[frequencyOffset + i];
                expectedForward[firstIndex] =
                    x.Data[firstIndex] * cosine - x.Data[secondIndex] * sine;
                expectedForward[secondIndex] =
                    x.Data[firstIndex] * sine + x.Data[secondIndex] * cosine;
                expectedGradient[firstIndex] =
                    upstream[firstIndex] * cosine + upstream[secondIndex] * sine;
                expectedGradient[secondIndex] =
                    -upstream[firstIndex] * sine + upstream[secondIndex] * cosine;
            }

            var unpairedIndex = baseOffset + dimension - 1;
            Assert.NotEqual(0f, x.Data[unpairedIndex]);
            Assert.Equal(0f, expectedForward[unpairedIndex]);
            Assert.Equal(0f, expectedGradient[unpairedIndex]);
        }

        var rotated = RotaryEmbedding.Apply(x, cos, sin);
        AssertArraysClose(expectedForward, rotated.Data, 0f, 0f);
        rotated.Backward(new Tensor(upstream, rotated.Shape));

        Assert.NotNull(x.Grad);
        AssertArraysClose(expectedGradient, x.Grad!.Data, 0f, 0f);
        Assert.Null(cos.Grad);
        Assert.Null(sin.Grad);
    }

    [Fact]
    public void TensorOps_ApplyCausalMaskBackwardZerosFuturePositions()
    {
        const int sequence = 3;
        const int matrixCount = 4;
        var scores = new Tensor(
            Enumerable.Range(0, matrixCount * sequence * sequence)
                .Select(index => -0.5f + index * 0.031f)
                .ToArray(),
            new TensorShape(2, 2, sequence, sequence),
            requiresGrad: true);
        var masked = TensorOps.ApplyCausalMask(scores, float.NegativeInfinity);
        var upstream = Enumerable.Range(0, scores.Data.Length)
            .Select(index => -0.17f + index * 0.019f)
            .ToArray();
        var expectedGradient = new float[scores.Data.Length];

        for (var matrix = 0; matrix < matrixCount; matrix++)
        for (var row = 0; row < sequence; row++)
        for (var column = 0; column < sequence; column++)
        {
            var index = matrix * sequence * sequence + row * sequence + column;
            if (column <= row)
            {
                Assert.Equal(scores.Data[index], masked.Data[index]);
                expectedGradient[index] = upstream[index];
            }
            else
            {
                Assert.True(float.IsNegativeInfinity(masked.Data[index]));
            }
        }

        float FiniteObjective()
        {
            var current = TensorOps.ApplyCausalMask(scores, float.NegativeInfinity);
            var total = 0f;
            for (var matrix = 0; matrix < matrixCount; matrix++)
            for (var row = 0; row < sequence; row++)
            for (var column = 0; column <= row; column++)
            {
                var index = matrix * sequence * sequence + row * sequence + column;
                total += current.Data[index] * upstream[index];
            }

            return total;
        }

        var numerical = NumericalGradient(scores, FiniteObjective, epsilon: 1e-3f);
        masked.Backward(new Tensor(upstream, masked.Shape));

        Assert.NotNull(scores.Grad);
        Assert.Equal(scores.Shape.Dimensions, scores.Grad!.Shape.Dimensions);
        AssertArraysClose(expectedGradient, scores.Grad.Data, 0f, 0f);
        AssertArraysClose(numerical, scores.Grad.Data, 2e-4f, 2e-3f);
    }

    [Fact]
    public void TensorOps_MultiplyBroadcastBackwardReducesPerRowScale()
    {
        const int batch = 2;
        const int rows = 3;
        const int columns = 4;
        var a = new Tensor(
            Enumerable.Range(0, batch * rows * columns)
                .Select(index => -0.7f + index * 0.061f)
                .ToArray(),
            new TensorShape(batch, rows, columns),
            requiresGrad: true);
        var b = new Tensor(
            [0.8f, -1.2f, 0.45f, -0.65f, 1.1f, 0.27f],
            new TensorShape(batch, rows, 1),
            requiresGrad: true);
        var upstream = Enumerable.Range(0, a.Data.Length)
            .Select(index => 0.31f - index * 0.023f)
            .ToArray();
        var multiplied = TensorOps.MultiplyBroadcast(a, b);
        var expectedForward = new float[a.Data.Length];
        var expectedA = new float[a.Data.Length];
        var expectedB = new float[b.Data.Length];
        for (var batchIndex = 0; batchIndex < batch; batchIndex++)
        for (var row = 0; row < rows; row++)
        for (var column = 0; column < columns; column++)
        {
            var aIndex = (batchIndex * rows + row) * columns + column;
            var bIndex = batchIndex * rows + row;
            expectedForward[aIndex] = a.Data[aIndex] * b.Data[bIndex];
            expectedA[aIndex] = upstream[aIndex] * b.Data[bIndex];
            expectedB[bIndex] += upstream[aIndex] * a.Data[aIndex];
        }

        AssertArraysClose(expectedForward, multiplied.Data, 1e-6f, 1e-6f);
        var numericalB = NumericalGradient(
            b,
            () => WeightedSum(TensorOps.MultiplyBroadcast(a, b), upstream),
            epsilon: 1e-3f);
        multiplied.Backward(new Tensor(upstream, multiplied.Shape));

        Assert.NotNull(a.Grad);
        Assert.NotNull(b.Grad);
        Assert.Equal(a.Shape.Dimensions, a.Grad!.Shape.Dimensions);
        Assert.Equal(b.Shape.Dimensions, b.Grad!.Shape.Dimensions);
        AssertArraysClose(expectedA, a.Grad.Data, 1e-6f, 1e-6f);
        AssertArraysClose(expectedB, b.Grad.Data, 1e-6f, 1e-6f);
        AssertArraysClose(numericalB, b.Grad.Data, 3e-4f, 3e-3f);
    }

    [Fact]
    public void TensorOps_MultiplyBroadcastBackwardReducesGammaAcrossRows()
    {
        const int batch = 2;
        const int rows = 3;
        const int columns = 4;
        var a = new Tensor(
            Enumerable.Range(0, batch * rows * columns)
                .Select(index => -0.4f + index * 0.047f)
                .ToArray(),
            new TensorShape(batch, rows, columns),
            requiresGrad: true);
        var gamma = new Tensor(
            [0.7f, -1.3f, 0.29f, 1.11f],
            new TensorShape(columns),
            requiresGrad: true);
        var upstream = Enumerable.Range(0, a.Data.Length)
            .Select(index => -0.22f + index * 0.029f)
            .ToArray();
        var multiplied = TensorOps.MultiplyBroadcast(a, gamma);
        var expectedForward = new float[a.Data.Length];
        var expectedA = new float[a.Data.Length];
        var expectedGamma = new float[columns];
        for (var batchIndex = 0; batchIndex < batch; batchIndex++)
        for (var row = 0; row < rows; row++)
        for (var column = 0; column < columns; column++)
        {
            var index = (batchIndex * rows + row) * columns + column;
            expectedForward[index] = a.Data[index] * gamma.Data[column];
            expectedA[index] = upstream[index] * gamma.Data[column];
            expectedGamma[column] += upstream[index] * a.Data[index];
        }

        AssertArraysClose(expectedForward, multiplied.Data, 1e-6f, 1e-6f);
        var numericalGamma = NumericalGradient(
            gamma,
            () => WeightedSum(TensorOps.MultiplyBroadcast(a, gamma), upstream),
            epsilon: 1e-3f);
        multiplied.Backward(new Tensor(upstream, multiplied.Shape));

        Assert.NotNull(a.Grad);
        Assert.NotNull(gamma.Grad);
        Assert.Equal(a.Shape.Dimensions, a.Grad!.Shape.Dimensions);
        Assert.Equal(gamma.Shape.Dimensions, gamma.Grad!.Shape.Dimensions);
        AssertArraysClose(expectedA, a.Grad.Data, 1e-6f, 1e-6f);
        AssertArraysClose(expectedGamma, gamma.Grad.Data, 1e-6f, 1e-6f);
        AssertArraysClose(numericalGamma, gamma.Grad.Data, 3e-4f, 3e-3f);
    }

    [Fact]
    public void RmsNorm_BackwardMatchesReferenceForInputAndGamma()
    {
        const int batch = 2;
        const int rows = 3;
        const int columns = 4;
        const float epsilon = 1e-5f;
        var x = new Tensor(
            new[]
            {
                -0.9f, 0.2f, 0.7f, -0.4f,
                0.3f, -1.1f, 0.5f, 0.8f,
                -0.6f, 0.9f, -0.2f, 1.2f,
                0.4f, -0.7f, 1.1f, 0.1f,
                -1.3f, 0.6f, 0.25f, -0.45f,
                0.75f, -0.35f, -0.8f, 1.4f
            },
            new TensorShape(batch, rows, columns),
            requiresGrad: true);
        var norm = new RmsNorm(columns, epsilon);
        var gamma = norm.Parameters().Single().Value;
        var gammaValues = new[] { 0.75f, -1.2f, 0.43f, 1.31f };
        Array.Copy(gammaValues, gamma.Data, gammaValues.Length);
        var upstream = Enumerable.Range(0, x.Data.Length)
            .Select(index => -0.27f + index * 0.021f)
            .ToArray();
        var output = norm.Forward(x);
        var expectedForward = new float[x.Data.Length];
        var expectedX = new float[x.Data.Length];
        var expectedGamma = new float[columns];
        for (var outer = 0; outer < batch * rows; outer++)
        {
            double sumSquares = 0;
            for (var column = 0; column < columns; column++)
            {
                var value = x.Data[outer * columns + column];
                sumSquares += value * value;
            }

            var inverseRms = 1.0 / Math.Sqrt(sumSquares / columns + epsilon);
            double coupled = 0;
            for (var column = 0; column < columns; column++)
            {
                var index = outer * columns + column;
                expectedForward[index] = (float)(x.Data[index] * inverseRms * gammaValues[column]);
                expectedGamma[column] += (float)(upstream[index] * x.Data[index] * inverseRms);
                coupled += upstream[index] * gammaValues[column] * x.Data[index];
            }

            for (var column = 0; column < columns; column++)
            {
                var index = outer * columns + column;
                expectedX[index] = (float)(
                    upstream[index] * gammaValues[column] * inverseRms
                    - x.Data[index] * Math.Pow(inverseRms, 3) * coupled / columns);
            }
        }

        AssertArraysClose(expectedForward, output.Data, 2e-6f, 2e-6f);
        var numericalX = NumericalGradient(x, () => WeightedSum(norm.Forward(x), upstream), 1e-3f);
        var numericalGamma = NumericalGradient(gamma, () => WeightedSum(norm.Forward(x), upstream), 1e-3f);
        output.Backward(new Tensor(upstream, output.Shape));

        Assert.NotNull(x.Grad);
        Assert.NotNull(gamma.Grad);
        Assert.Equal(x.Shape.Dimensions, x.Grad!.Shape.Dimensions);
        Assert.Equal(gamma.Shape.Dimensions, gamma.Grad!.Shape.Dimensions);
        Assert.All(output.Data, value => Assert.True(float.IsFinite(value)));
        Assert.All(x.Grad.Data, value => Assert.True(float.IsFinite(value)));
        Assert.All(gamma.Grad.Data, value => Assert.True(float.IsFinite(value)));
        AssertArraysClose(expectedX, x.Grad.Data, 3e-5f, 3e-4f);
        AssertArraysClose(expectedGamma, gamma.Grad.Data, 3e-5f, 3e-4f);
        AssertArraysClose(numericalX, x.Grad.Data, 7e-4f, 7e-3f);
        AssertArraysClose(numericalGamma, gamma.Grad.Data, 7e-4f, 7e-3f);

        var zero = new Tensor(new float[8], new TensorShape(1, 2, columns), requiresGrad: true);
        var zeroOutput = norm.Forward(zero);
        zeroOutput.Backward(new Tensor(Enumerable.Repeat(1f, 8).ToArray(), zeroOutput.Shape));
        Assert.All(zeroOutput.Data, value => Assert.True(float.IsFinite(value)));
        Assert.NotNull(zero.Grad);
        Assert.All(zero.Grad!.Data, value => Assert.True(float.IsFinite(value)));
    }

    [Fact]
    public void LoraAttention_AllProjectionBGradientsMatchFiniteDifferenceThroughFinalNorm()
    {
        foreach (var projection in new[] { "q_proj", "k_proj", "v_proj", "o_proj" })
        {
            var model = CreateDeterministicGradientModel();
            var adapter = Assert.Single(LoraApplicator.Apply(
                model,
                new LoraConfig
                {
                    Rank = 1,
                    Alpha = 1,
                    Dropout = 0,
                    TargetModules = [projection]
                }));
            var loraA = adapter.AdapterParameters[0].Parameter.Value;
            var loraB = adapter.AdapterParameters[1].Parameter.Value;
            for (var index = 0; index < loraA.Data.Length; index++)
                loraA.Data[index] = 0.36f + 0.071f * index;
            Array.Clear(loraB.Data);

            var ids = new Tensor([1f, 3f, 5f], new TensorShape(1, 3));
            var upstream = Enumerable.Range(0, 3 * model.Config.VocabSize)
                .Select(index => 0.23f * MathF.Sin(0.41f * (index + 1))
                                 + 0.07f * MathF.Cos(0.19f * (index + 2)))
                .ToArray();
            var numerical = NumericalGradient(
                loraB,
                () => WeightedSum(model.Forward(ids, training: true), upstream),
                epsilon: 2e-3f);
            var maximumIndex = Enumerable.Range(0, numerical.Length)
                .MaxBy(index => MathF.Abs(numerical[index]));
            Assert.True(
                MathF.Abs(numerical[maximumIndex]) > 1e-6f,
                $"{projection} numerical B gradient was not observable.");

            model.ZeroGrad();
            var logits = model.Forward(ids, training: true);
            logits.Backward(new Tensor(upstream, logits.Shape));

            Assert.NotNull(loraB.Grad);
            Assert.Equal(loraB.Shape.Dimensions, loraB.Grad!.Shape.Dimensions);
            Assert.All(loraB.Grad.Data, value => Assert.True(float.IsFinite(value)));
            AssertClose(
                numerical[maximumIndex],
                loraB.Grad.Data[maximumIndex],
                absoluteTolerance: 2e-3f,
                relativeTolerance: 8e-2f,
                context: $"{projection} B[{maximumIndex}]");
        }
    }

    [Fact]
    public void LoraAttention_ZeroDropoutTrainingAndInferenceForwardAreValueIdentical()
    {
        var model = CreateDeterministicGradientModel();
        var adapters = LoraApplicator.Apply(
            model,
            new LoraConfig
            {
                Rank = 2,
                Alpha = 3,
                Dropout = 0,
                TargetModules = ["q_proj", "k_proj", "v_proj", "o_proj"]
            });
        for (var adapterIndex = 0; adapterIndex < adapters.Count; adapterIndex++)
        {
            var a = adapters[adapterIndex].AdapterParameters[0].Parameter.Value;
            var b = adapters[adapterIndex].AdapterParameters[1].Parameter.Value;
            for (var index = 0; index < a.Data.Length; index++)
                a.Data[index] = -0.21f + 0.037f * (index + 2 * adapterIndex);
            for (var index = 0; index < b.Data.Length; index++)
                b.Data[index] = 0.16f - 0.029f * (index + adapterIndex);
        }

        var ids = new Tensor([1f, 3f, 5f], new TensorShape(1, 3));
        var inference = model.Forward(ids, training: false);
        var training = model.Forward(ids, training: true);
        var wrapper = model.Forward(ids);

        Assert.Equal(inference.Shape.Dimensions, training.Shape.Dimensions);
        Assert.Equal(inference.Shape.Dimensions, wrapper.Shape.Dimensions);
        AssertArraysClose(inference.Data, training.Data, 0f, 0f);
        AssertArraysClose(inference.Data, wrapper.Data, 0f, 0f);
    }

    [Fact]
    public void TensorOps_SiluBackwardMatchesAnalyticAndCentralDifference()
    {
        var x = new Tensor(
            [-3.1f, -1.2f, -0.35f, 0f, 0.42f, 1.7f, 3.4f],
            new TensorShape(7),
            requiresGrad: true);
        var upstream = new[] { -0.7f, 0.2f, 1.1f, -0.4f, 0.63f, -0.19f, 0.37f };
        var output = TensorOps.Silu(x);
        var expectedForward = new float[x.Data.Length];
        var localGradient = new float[x.Data.Length];
        var existingGradient = new[] { 0.13f, -0.27f, 0.41f, -0.09f, 0.22f, -0.31f, 0.17f };
        var expectedAccumulatedGradient = new float[x.Data.Length];
        for (var index = 0; index < localGradient.Length; index++)
        {
            var sigmoid = 1f / (1f + MathF.Exp(-x.Data[index]));
            expectedForward[index] = x.Data[index] / (1f + MathF.Exp(-x.Data[index]));
            localGradient[index] = upstream[index]
                                   * sigmoid
                                   * (1f + x.Data[index] * (1f - sigmoid));
            expectedAccumulatedGradient[index] =
                existingGradient[index] + localGradient[index];
        }

        Assert.Equal(expectedForward, output.Data);
        var numerical = NumericalGradient(
            x,
            () => WeightedSum(TensorOps.Silu(x), upstream),
            epsilon: 1e-3f);
        x.Grad = new Tensor(existingGradient, x.Shape);
        output.Backward(new Tensor(upstream, output.Shape));

        Assert.NotNull(x.Grad);
        Assert.Equal(x.Shape.Dimensions, x.Grad!.Shape.Dimensions);
        AssertArraysClose(expectedAccumulatedGradient, x.Grad.Data, 2e-6f, 2e-6f);
        var accumulatedLocalGradient = x.Grad.Data
            .Zip(existingGradient, (accumulated, existing) => accumulated - existing)
            .ToArray();
        AssertArraysClose(numerical, accumulatedLocalGradient, 4e-4f, 4e-3f);
    }

    [Fact]
    public void LlmModel_BaseOnlyCheckpointWritesVersionTwoAndRoundTrips()
    {
        var root = CreateRoot();
        try
        {
            var model = new LlmModel(TinyConfig());
            FillSequential(model.BaseParameters);
            var actualPath = Path.Combine(root, "actual.bin");
            model.SaveCheckpoint(actualPath);

            using (var reader = new BinaryReader(File.OpenRead(actualPath)))
            {
                Assert.Equal(unchecked((int)0xC14F5241), reader.ReadInt32());
                Assert.Equal(2, reader.ReadInt32());
                Assert.Equal(1, reader.ReadInt32());
                Assert.Equal(32, reader.ReadInt32());
            }
            var loaded = new LlmModel(TinyConfig());
            loaded.LoadCheckpoint(actualPath);
            AssertParameterValues(model.BaseParameters, loaded.BaseParameters);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LlmModel_LegacyLoadIsStrictAndZeroMutationOnLateFailure()
    {
        var root = CreateRoot();
        try
        {
            var source = new LlmModel(TinyConfig());
            var valid = Path.Combine(root, "valid.bin");
            WriteLegacyFixture(valid, source.BaseParameters);
            var malformed = Path.Combine(root, "trailing.bin");
            File.WriteAllBytes(malformed, File.ReadAllBytes(valid).Append((byte)0x7f).ToArray());
            var destination = new LlmModel(TinyConfig());
            FillSequential(destination.BaseParameters, 100);
            var before = CaptureValues(destination.Parameters());

            var exception = Assert.ThrowsAny<Exception>(() => destination.LoadCheckpoint(malformed));

            Assert.Contains("LLM_CHECKPOINT_MALFORMED", exception.Message, StringComparison.Ordinal);
            AssertValues(before, destination.Parameters());
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LlmModel_LegacyCheckpointValidationMatrixHasStableCodesAndZeroMutation()
    {
        var root = CreateRoot();
        try
        {
            var source = new LlmModel(TinyConfig());
            FillSequential(source.BaseParameters);

            void Verify(
                string caseName,
                string expectedCode,
                Action<string> writeCheckpoint,
                string? expectedMessage = null)
            {
                var path = Path.Combine(root, caseName + ".bin");
                writeCheckpoint(path);
                var destination = new LlmModel(TinyConfig());
                FillSequential(destination.BaseParameters, 500);
                AssertCheckpointRejectedWithoutMutation(
                    destination,
                    path,
                    expectedCode,
                    root,
                    caseName,
                    expectedMessage);
            }

            Verify(
                "legacy-wrong-count",
                "LLM_CHECKPOINT_PARAMETER_COUNT_MISMATCH",
                path =>
                {
                    using var writer = new BinaryWriter(File.Create(path));
                    writer.Write(source.BaseParameters.Count - 1);
                });
            Verify(
                "legacy-wrong-rank",
                "LLM_CHECKPOINT_PARAMETER_SHAPE_MISMATCH",
                path =>
                {
                    using var writer = new BinaryWriter(File.Create(path));
                    writer.Write(source.BaseParameters.Count);
                    writer.Write(source.BaseParameters[0].Value.Shape.Rank + 1);
                });
            Verify(
                "legacy-wrong-shape",
                "LLM_CHECKPOINT_PARAMETER_SHAPE_MISMATCH",
                path =>
                {
                    using var writer = new BinaryWriter(File.Create(path));
                    var first = source.BaseParameters[0].Value;
                    writer.Write(source.BaseParameters.Count);
                    writer.Write(first.Shape.Rank);
                    for (var index = 0; index < first.Shape.Rank; index++)
                    {
                        writer.Write(
                            first.Shape.Dimensions[index]
                            + (index == 0 ? 1 : 0));
                    }
                });
            Verify(
                "legacy-truncated-value",
                "LLM_CHECKPOINT_MALFORMED",
                path =>
                {
                    WriteLegacyFixture(path, source.BaseParameters);
                    var bytes = File.ReadAllBytes(path);
                    File.WriteAllBytes(path, bytes[..^1]);
                });
            const string nonFiniteMessage =
                "LLM_CHECKPOINT_MALFORMED: Checkpoint contains a non-finite parameter value.";
            Verify(
                "legacy-nan-final-value",
                "LLM_CHECKPOINT_MALFORMED",
                path => WriteLegacyFixtureWithFinalValue(path, source.BaseParameters, float.NaN),
                nonFiniteMessage);
            Verify(
                "legacy-positive-infinity-final-value",
                "LLM_CHECKPOINT_MALFORMED",
                path => WriteLegacyFixtureWithFinalValue(path, source.BaseParameters, float.PositiveInfinity),
                nonFiniteMessage);
            Verify(
                "legacy-negative-infinity-final-value",
                "LLM_CHECKPOINT_MALFORMED",
                path => WriteLegacyFixtureWithFinalValue(path, source.BaseParameters, float.NegativeInfinity),
                nonFiniteMessage);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LlmModel_KeyedCheckpointValidationMatrixHasStableCodesAndZeroMutation()
    {
        var root = CreateRoot();
        try
        {
            var source = new LlmModel(TinyConfig(equalProjectionShapes: true));
            _ = LoraApplicator.Apply(source, Config(["q_proj", "v_proj"]));
            FillSequential(source.Parameters());
            var canonical = CreateVersionOneRawKeyedEntries(source);

            LlmModel CreateDestination()
            {
                var destination = new LlmModel(TinyConfig(equalProjectionShapes: true));
                _ = LoraApplicator.Apply(destination, Config(["q_proj", "v_proj"]));
                FillSequential(destination.Parameters(), 700);
                return destination;
            }

            void Verify(
                string caseName,
                string expectedCode,
                Action<string> writeCheckpoint)
            {
                var path = Path.Combine(root, caseName + ".bin");
                writeCheckpoint(path);
                AssertCheckpointRejectedWithoutMutation(
                    CreateDestination(),
                    path,
                    expectedCode,
                    root,
                    caseName);
            }

            Verify(
                "keyed-unsupported-version",
                "LLM_CHECKPOINT_FORMAT_UNSUPPORTED",
                path => WriteKeyedFixture(path, version: 99, canonical));
            Verify(
                "keyed-duplicate-key",
                "LLM_CHECKPOINT_PARAMETER_KEY_MISMATCH",
                path =>
                {
                    var entries = CloneRawKeyedEntries(canonical);
                    entries[1] = entries[1] with
                    {
                        KeyBytes = entries[0].KeyBytes.ToArray()
                    };
                    WriteKeyedFixture(path, version: 1, entries);
                });
            Verify(
                "keyed-missing-key",
                "LLM_CHECKPOINT_PARAMETER_KEY_MISMATCH",
                path =>
                {
                    var entries = CloneRawKeyedEntries(canonical);
                    entries[1] = entries[1] with
                    {
                        KeyBytes = Encoding.UTF8.GetBytes("base.missing")
                    };
                    WriteKeyedFixture(path, version: 1, entries);
                });
            Verify(
                "keyed-invalid-utf8",
                "LLM_CHECKPOINT_MALFORMED",
                path =>
                {
                    var entries = CloneRawKeyedEntries(canonical);
                    entries[0] = entries[0] with { KeyBytes = [0xc3, 0x28] };
                    WriteKeyedFixture(path, version: 1, entries);
                });
            Verify(
                "keyed-negative-rank",
                "LLM_CHECKPOINT_MALFORMED",
                path =>
                {
                    var entries = CloneRawKeyedEntries(canonical);
                    entries[0] = entries[0] with { Rank = -1, Dimensions = [] };
                    WriteKeyedFixture(path, version: 1, entries);
                });
            Verify(
                "keyed-negative-dimension",
                "LLM_CHECKPOINT_MALFORMED",
                path =>
                {
                    var entries = CloneRawKeyedEntries(canonical);
                    var dimensions = entries[0].Dimensions.ToArray();
                    dimensions[0] = -1;
                    entries[0] = entries[0] with { Dimensions = dimensions };
                    WriteKeyedFixture(path, version: 1, entries);
                });
            Verify(
                "keyed-overflow-dimensions",
                "LLM_CHECKPOINT_MALFORMED",
                path =>
                {
                    var entries = CloneRawKeyedEntries(canonical);
                    entries[0] = entries[0] with
                    {
                        Dimensions = [int.MaxValue, int.MaxValue]
                    };
                    WriteKeyedFixture(path, version: 1, entries);
                });
            Verify(
                "keyed-wrong-payload-length",
                "LLM_CHECKPOINT_MALFORMED",
                path =>
                {
                    var entries = CloneRawKeyedEntries(canonical);
                    entries[0] = entries[0] with
                    {
                        PayloadBytes = entries[0].PayloadBytes + sizeof(float)
                    };
                    WriteKeyedFixture(path, version: 1, entries);
                });
            Verify(
                "keyed-truncated-value",
                "LLM_CHECKPOINT_MALFORMED",
                path =>
                {
                    var entries = CloneRawKeyedEntries(canonical);
                    var last = entries.Length - 1;
                    entries[last] = entries[last] with
                    {
                        Values = entries[last].Values[..^1]
                    };
                    WriteKeyedFixture(path, version: 1, entries);
                });
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LlmModel_KeyedRankTwoIntoSameKeyRankFourRejectsWithoutMutation()
    {
        var root = CreateRoot();
        try
        {
            var source = new LlmModel(TinyConfig(equalProjectionShapes: true));
            _ = LoraApplicator.Apply(
                source,
                new LoraConfig
                {
                    Rank = 2,
                    Alpha = 4,
                    Dropout = 0,
                    TargetModules = ["q_proj"]
                });
            FillSequential(source.Parameters());
            var path = Path.Combine(root, "rank-two.bin");
            source.SaveCheckpoint(path);

            var destination = new LlmModel(TinyConfig(equalProjectionShapes: true));
            _ = LoraApplicator.Apply(
                destination,
                new LoraConfig
                {
                    Rank = 4,
                    Alpha = 4,
                    Dropout = 0,
                    TargetModules = ["q_proj"]
                });
            FillSequential(destination.Parameters(), 900);

            AssertCheckpointRejectedWithoutMutation(
                destination,
                path,
                "LLM_CHECKPOINT_PARAMETER_SHAPE_MISMATCH",
                root,
                "rank-two-into-rank-four");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LlmModel_KeyedQvCheckpointRoundTripsAndBareRejectsWithoutMutation()
    {
        var root = CreateRoot();
        try
        {
            var source = new LlmModel(TinyConfig(equalProjectionShapes: true));
            _ = LoraApplicator.Apply(source, Config(["q_proj", "v_proj"]));
            FillSequential(source.GetCheckpointParameterEntries().Select(entry => entry.Parameter));
            var path = Path.Combine(root, "qv.bin");
            source.SaveCheckpoint(path);
            using (var reader = new BinaryReader(File.OpenRead(path)))
            {
                Assert.Equal(unchecked((int)0xC14F5241), reader.ReadInt32());
                Assert.Equal(2, reader.ReadInt32());
                Assert.Equal(1, reader.ReadInt32());
                Assert.Equal(32, reader.ReadInt32());
            }

            var destination = new LlmModel(TinyConfig(equalProjectionShapes: true));
            _ = LoraApplicator.Apply(destination, Config(["q_proj", "v_proj"]));
            destination.LoadCheckpoint(path);
            AssertParameterValues(source.Parameters(), destination.Parameters());

            var bare = new LlmModel(TinyConfig(equalProjectionShapes: true));
            var before = CaptureValues(bare.Parameters());
            var exception = Assert.ThrowsAny<Exception>(() => bare.LoadCheckpoint(path));
            Assert.Contains("LLM_CHECKPOINT_PARAMETER_COUNT_MISMATCH", exception.Message, StringComparison.Ordinal);
            AssertValues(before, bare.Parameters());
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LlmModel_KeyedQvToOqSameShapeRejectsByKeyWithoutMutation()
    {
        var root = CreateRoot();
        try
        {
            var source = new LlmModel(TinyConfig(equalProjectionShapes: true));
            _ = LoraApplicator.Apply(source, Config(["q_proj", "v_proj"]));
            var path = Path.Combine(root, "q_proj-v_proj.bin");
            source.SaveCheckpoint(path);

            var destination = new LlmModel(TinyConfig(equalProjectionShapes: true));
            _ = LoraApplicator.Apply(destination, Config(["o_proj", "v_proj"]));
            var before = CaptureValues(destination.Parameters());
            var exception = Assert.ThrowsAny<Exception>(() => destination.LoadCheckpoint(path));

            Assert.Contains("LLM_CHECKPOINT_PARAMETER_KEY_MISMATCH", exception.Message, StringComparison.Ordinal);
            AssertValues(before, destination.Parameters());
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LlmModel_KeySwapAndTrailingByteRejectBeforeCopy()
    {
        var root = CreateRoot();
        try
        {
            var source = new LlmModel(TinyConfig(equalProjectionShapes: true));
            _ = LoraApplicator.Apply(source, Config(["q_proj", "v_proj"]));
            var valid = Path.Combine(root, "valid-keyed.bin");
            source.SaveCheckpoint(valid);

            var swapped = Path.Combine(root, "swapped.bin");
            var bytes = File.ReadAllBytes(valid);
            ReplaceFirstAscii(bytes, "q_proj", "o_proj");
            File.WriteAllBytes(swapped, bytes);

            var destination = new LlmModel(TinyConfig(equalProjectionShapes: true));
            _ = LoraApplicator.Apply(destination, Config(["q_proj", "v_proj"]));
            FillSequential(destination.Parameters(), 300);
            var before = CaptureValues(destination.Parameters());
            var keyException = Assert.ThrowsAny<Exception>(() => destination.LoadCheckpoint(swapped));
            Assert.Contains("LLM_CHECKPOINT_PARAMETER_KEY_MISMATCH", keyException.Message, StringComparison.Ordinal);
            AssertValues(before, destination.Parameters());

            var trailing = Path.Combine(root, "trailing-keyed.bin");
            File.WriteAllBytes(trailing, File.ReadAllBytes(valid).Append((byte)1).ToArray());
            var malformed = Assert.ThrowsAny<Exception>(() => destination.LoadCheckpoint(trailing));
            Assert.Contains("LLM_CHECKPOINT_MALFORMED", malformed.Message, StringComparison.Ordinal);
            AssertValues(before, destination.Parameters());
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LlmModel_LegacyLoadIntoAttachedModelPreservesAdapterValues()
    {
        var root = CreateRoot();
        try
        {
            var bare = new LlmModel(TinyConfig());
            FillSequential(bare.BaseParameters, 10);
            var path = Path.Combine(root, "legacy.bin");
            WriteLegacyFixture(path, bare.BaseParameters);

            var attached = new LlmModel(TinyConfig());
            var adapter = Assert.Single(LoraApplicator.Apply(attached, Config(["q_proj"])));
            FillSequential(adapter.AdapterParameters.Select(pair => pair.Parameter), 900);
            var adapterBefore = CaptureValues(adapter.Parameters());
            attached.LoadCheckpoint(path);

            AssertParameterValues(bare.BaseParameters, attached.BaseParameters);
            AssertValues(adapterBefore, adapter.Parameters());
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LlmModel_AdapterCheckpoint_RoundTripsExactAdapterMatricesWithoutChangingBase()
    {
        var root = CreateRoot();
        try
        {
            var source = new LlmModel(TinyConfig(equalProjectionShapes: true));
            var sourceAdapters = LoraApplicator.Apply(source, Config(["q_proj", "v_proj"]));
            FillSequential(
                sourceAdapters.SelectMany(adapter =>
                    adapter.AdapterParameters.Select(pair => pair.Parameter)),
                1000);
            var path = Path.Combine(root, "adapter-only.bin");

            source.SaveAdapterCheckpoint(path);

            var rawEntries = ReadKeyedFixture(path);
            Assert.Equal(
                [
                    "lora.block_0000.q_proj.lora_a",
                    "lora.block_0000.q_proj.lora_b",
                    "lora.block_0000.v_proj.lora_a",
                    "lora.block_0000.v_proj.lora_b"
                ],
                rawEntries.Select(entry => Encoding.UTF8.GetString(entry.KeyBytes)).ToArray());
            Assert.All(
                rawEntries,
                entry => Assert.StartsWith(
                    "lora.",
                    Encoding.UTF8.GetString(entry.KeyBytes),
                    StringComparison.Ordinal));

            var destination = new LlmModel(TinyConfig(equalProjectionShapes: true));
            FillSequential(destination.BaseParameters, 400);
            var destinationAdapters = LoraApplicator.Apply(
                destination,
                Config(["q_proj", "v_proj"]));
            FillSequential(
                destinationAdapters.SelectMany(adapter =>
                    adapter.AdapterParameters.Select(pair => pair.Parameter)),
                700);
            var baseBefore = CaptureValues(destination.BaseParameters);

            destination.LoadAdapterCheckpoint(path);

            AssertParameterValues(
                sourceAdapters.SelectMany(adapter =>
                    adapter.AdapterParameters.Select(pair => pair.Parameter)),
                destinationAdapters.SelectMany(adapter =>
                    adapter.AdapterParameters.Select(pair => pair.Parameter)));
            AssertValues(baseBefore, destination.BaseParameters);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LlmModel_StaticAdapterArtifactValidator_AcceptsRealAdapterOnlyCheckpoint()
    {
        var root = CreateRoot();
        try
        {
            var model = new LlmModel(TinyConfig(equalProjectionShapes: true));
            _ = LoraApplicator.Apply(model, Config(["q_proj", "v_proj"]));
            FillSequential(model.Parameters(), 100);
            var path = Path.Combine(root, "valid-adapter.bin");
            model.SaveAdapterCheckpoint(path);

            LlmModel.ValidateAdapterCheckpointArtifact(path);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LlmModel_StaticAdapterArtifactValidator_RejectsMalformedAndNonAdapterArtifacts()
    {
        var root = CreateRoot();
        try
        {
            var model = new LlmModel(TinyConfig(equalProjectionShapes: true));
            _ = LoraApplicator.Apply(model, Config(["q_proj", "v_proj"]));
            FillSequential(model.Parameters(), 100);
            var valid = Path.Combine(root, "valid-adapter.bin");
            model.SaveAdapterCheckpoint(valid);
            var validBytes = File.ReadAllBytes(valid);
            var validEntries = ReadKeyedFixture(valid);

            var truncated = Path.Combine(root, "truncated.bin");
            File.WriteAllBytes(truncated, validBytes[..^1]);

            var trailing = Path.Combine(root, "trailing.bin");
            File.WriteAllBytes(trailing, validBytes.Append((byte)0x5a).ToArray());

            var malformedLength = Path.Combine(root, "malformed-length.bin");
            var malformedEntries = CloneRawKeyedEntries(validEntries);
            malformedEntries[^1] = malformedEntries[^1] with
            {
                PayloadBytes = malformedEntries[^1].PayloadBytes + sizeof(float)
            };
            WriteKeyedFixture(malformedLength, version: 1, malformedEntries);

            var zeroEntry = Path.Combine(root, "zero-entry.bin");
            WriteKeyedFixture(zeroEntry, version: 1, []);

            var fullCheckpoint = Path.Combine(root, "full-checkpoint.bin");
            model.SaveCheckpoint(fullCheckpoint);

            var nonFinite = Path.Combine(root, "non-finite.bin");
            var nonFiniteEntries = CloneRawKeyedEntries(validEntries);
            nonFiniteEntries[0].Values[0] = float.NaN;
            WriteKeyedFixture(nonFinite, version: 1, nonFiniteEntries);

            var duplicateKey = Path.Combine(root, "duplicate-key.bin");
            var duplicateEntries = CloneRawKeyedEntries(validEntries);
            duplicateEntries[1] = duplicateEntries[1] with
            {
                KeyBytes = duplicateEntries[0].KeyBytes.ToArray()
            };
            WriteKeyedFixture(duplicateKey, version: 1, duplicateEntries);

            var invalidUtf8 = Path.Combine(root, "invalid-utf8.bin");
            var invalidUtf8Entries = CloneRawKeyedEntries(validEntries);
            invalidUtf8Entries[0].KeyBytes[0] = 0xff;
            WriteKeyedFixture(invalidUtf8, version: 1, invalidUtf8Entries);

            var emptyAdapterKey = Path.Combine(root, "empty-adapter-key.bin");
            var emptyAdapterKeyEntries = CloneRawKeyedEntries(validEntries);
            emptyAdapterKeyEntries[0] = emptyAdapterKeyEntries[0] with
            {
                KeyBytes = Encoding.UTF8.GetBytes("lora.")
            };
            WriteKeyedFixture(emptyAdapterKey, version: 1, emptyAdapterKeyEntries);

            foreach (var path in new[]
                     {
                         truncated,
                         trailing,
                         malformedLength,
                         zeroEntry,
                         fullCheckpoint,
                         nonFinite,
                         duplicateKey,
                         invalidUtf8,
                         emptyAdapterKey
                     })
            {
                Assert.ThrowsAny<Exception>(
                    () => LlmModel.ValidateAdapterCheckpointArtifact(path));
            }
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LlmModel_AdapterCheckpoint_RejectsSameShapeWrongAttachmentWithoutAnyMutation()
    {
        var root = CreateRoot();
        try
        {
            var source = new LlmModel(TinyConfig(equalProjectionShapes: true));
            _ = LoraApplicator.Apply(source, Config(["q_proj", "v_proj"]));
            FillSequential(source.Parameters(), 100);
            var path = Path.Combine(root, "q-v-adapter.bin");
            source.SaveAdapterCheckpoint(path);

            var destination = new LlmModel(TinyConfig(equalProjectionShapes: true));
            _ = LoraApplicator.Apply(destination, Config(["o_proj", "v_proj"]));
            FillSequential(destination.Parameters(), 900);
            AssertAdapterCheckpointRejectedWithoutMutation(
                destination,
                path,
                "LLM_CHECKPOINT_PARAMETER_KEY_MISMATCH");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LlmModel_AdapterCheckpoint_LateMalformedArtifactsNeverMutateBaseOrAdapters()
    {
        var root = CreateRoot();
        try
        {
            var source = new LlmModel(TinyConfig(equalProjectionShapes: true));
            _ = LoraApplicator.Apply(source, Config(["q_proj", "v_proj"]));
            FillSequential(source.Parameters(), 100);
            var valid = Path.Combine(root, "valid-adapter.bin");
            source.SaveAdapterCheckpoint(valid);

            var trailing = Path.Combine(root, "trailing-adapter.bin");
            File.WriteAllBytes(
                trailing,
                File.ReadAllBytes(valid).Append((byte)0x5a).ToArray());

            var truncated = Path.Combine(root, "truncated-adapter.bin");
            var validBytes = File.ReadAllBytes(valid);
            File.WriteAllBytes(truncated, validBytes[..^1]);

            var malformed = Path.Combine(root, "malformed-adapter.bin");
            var entries = ReadKeyedFixture(valid);
            var last = entries.Length - 1;
            entries[last] = entries[last] with
            {
                PayloadBytes = entries[last].PayloadBytes + sizeof(float)
            };
            WriteKeyedFixture(malformed, version: 1, entries);

            foreach (var path in new[] { trailing, truncated, malformed })
            {
                var destination = new LlmModel(TinyConfig(equalProjectionShapes: true));
                _ = LoraApplicator.Apply(destination, Config(["q_proj", "v_proj"]));
                FillSequential(destination.Parameters(), 800);
                AssertAdapterCheckpointRejectedWithoutMutation(
                    destination,
                    path,
                    "LLM_CHECKPOINT_MALFORMED");
            }
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LlmModel_AdapterCheckpoint_RequiresAttachedAdaptersBeforeFileAccess()
    {
        var root = CreateRoot();
        try
        {
            var model = new LlmModel(TinyConfig());
            var path = Path.Combine(root, "must-not-exist.bin");

            var saveException = Assert.ThrowsAny<Exception>(
                () => model.SaveAdapterCheckpoint(path));
            Assert.StartsWith(
                "LLM_ADAPTER_CHECKPOINT_ADAPTERS_REQUIRED:",
                saveException.Message,
                StringComparison.Ordinal);
            Assert.False(File.Exists(path));

            var loadException = Assert.ThrowsAny<Exception>(
                () => model.LoadAdapterCheckpoint(path));
            Assert.StartsWith(
                "LLM_ADAPTER_CHECKPOINT_ADAPTERS_REQUIRED:",
                loadException.Message,
                StringComparison.Ordinal);
            Assert.False(File.Exists(path));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LlmModel_AdapterCheckpoint_CallerOwnedStream_ValidatesAndLoadsExactBytesWithoutClosing()
    {
        var root = CreateRoot();
        try
        {
            var source = new LlmModel(TinyConfig(equalProjectionShapes: true));
            var sourceAdapters = LoraApplicator.Apply(
                source,
                Config(["q_proj", "v_proj"]));
            var sourceAdapterParameters = sourceAdapters
                .SelectMany(static adapter => adapter.AdapterParameters)
                .Select(static entry => entry.Parameter)
                .ToArray();
            FillSequential(sourceAdapterParameters, 100);
            var path = Path.Combine(root, "stream-adapter.bin");
            source.SaveAdapterCheckpoint(path);
            var bytes = File.ReadAllBytes(path);

            using var stream = new MemoryStream(bytes, writable: false);
            LlmModel.ValidateAdapterCheckpointArtifact(stream);
            Assert.True(stream.CanRead);
            Assert.Equal(stream.Length, stream.Position);

            var destination = new LlmModel(TinyConfig(equalProjectionShapes: true));
            var destinationAdapters = LoraApplicator.Apply(
                destination,
                Config(["q_proj", "v_proj"]));
            var destinationAdapterParameters = destinationAdapters
                .SelectMany(static adapter => adapter.AdapterParameters)
                .Select(static entry => entry.Parameter)
                .ToArray();
            FillSequential(destinationAdapterParameters, 900);
            var baseBefore = CaptureValues(destination.BaseParameters);

            stream.Position = 0;
            destination.LoadAdapterCheckpoint(stream);

            Assert.True(stream.CanRead);
            Assert.Equal(stream.Length, stream.Position);
            AssertValues(baseBefore, destination.BaseParameters);
            AssertParameterValues(sourceAdapterParameters, destinationAdapterParameters);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LlmModel_AdapterCheckpoint_TruncatedCallerOwnedStream_LeavesModelUnchangedAndStreamOpen()
    {
        var root = CreateRoot();
        try
        {
            var source = new LlmModel(TinyConfig(equalProjectionShapes: true));
            _ = LoraApplicator.Apply(source, Config(["q_proj", "v_proj"]));
            FillSequential(source.Parameters(), 100);
            var path = Path.Combine(root, "valid-stream-adapter.bin");
            source.SaveAdapterCheckpoint(path);
            var bytes = File.ReadAllBytes(path);

            var destination = new LlmModel(TinyConfig(equalProjectionShapes: true));
            _ = LoraApplicator.Apply(destination, Config(["q_proj", "v_proj"]));
            FillSequential(destination.Parameters(), 900);
            var before = CaptureValues(destination.Parameters());
            using var stream = new MemoryStream(bytes[..^1], writable: false);

            var exception = Assert.ThrowsAny<Exception>(
                () => destination.LoadAdapterCheckpoint(stream));

            Assert.StartsWith(
                "LLM_CHECKPOINT_MALFORMED:",
                exception.Message,
                StringComparison.Ordinal);
            Assert.True(stream.CanRead);
            AssertValues(before, destination.Parameters());
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LlmModel_AdapterCheckpoint_InvalidNonblankPathFailsStableWithoutMutationOrArtifacts()
    {
        var root = CreateRoot();
        try
        {
            var model = new LlmModel(TinyConfig(equalProjectionShapes: true));
            _ = LoraApplicator.Apply(model, Config(["q_proj", "v_proj"]));
            FillSequential(model.Parameters(), 500);
            var before = CaptureValues(model.Parameters());
            var invalidPath =
                root + Path.DirectorySeparatorChar + "invalid\0adapter.bin";

            var exception = Assert.ThrowsAny<Exception>(
                () => model.SaveAdapterCheckpoint(invalidPath));

            Assert.StartsWith(
                "LLM_ADAPTER_CHECKPOINT_INCOMPLETE:",
                exception.Message,
                StringComparison.Ordinal);
            AssertValues(before, model.Parameters());
            Assert.False(File.Exists(invalidPath));
            Assert.Empty(Directory.GetFileSystemEntries(root));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LlmModel_AdapterCheckpoint_AtomicOverwriteReplacesBytesAndLeavesNoTempResidue()
    {
        var root = CreateRoot();
        try
        {
            var model = new LlmModel(TinyConfig(equalProjectionShapes: true));
            var adapters = LoraApplicator.Apply(model, Config(["q_proj", "v_proj"]));
            var adapterParameters = adapters
                .SelectMany(adapter =>
                    adapter.AdapterParameters.Select(pair => pair.Parameter))
                .ToArray();
            var path = Path.Combine(root, "atomic-adapter.bin");
            File.WriteAllBytes(path, [0x01, 0x02, 0x03]);

            FillSequential(adapterParameters, 100);
            model.SaveAdapterCheckpoint(path);
            var first = File.ReadAllBytes(path);
            Assert.NotEmpty(first);
            Assert.NotEqual(new byte[] { 0x01, 0x02, 0x03 }, first);

            FillSequential(adapterParameters, 500);
            model.SaveAdapterCheckpoint(path);
            var second = File.ReadAllBytes(path);

            Assert.NotEmpty(second);
            Assert.NotEqual(first, second);
            Assert.Empty(
                Directory.GetFiles(
                    root,
                    $"{Path.GetFileName(path)}.*.tmp",
                    SearchOption.TopDirectoryOnly));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LlmModel_AdapterOnlyCheckpointPrepublicationFailurePreservesExistingFinalAndCleansTemp()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var root = CreateRoot();
        try
        {
            var model = new LlmModel(TinyConfig(equalProjectionShapes: true));
            var adapters = LoraApplicator.Apply(model, Config(["q_proj", "v_proj"]));
            var adapterParameters = adapters
                .SelectMany(adapter =>
                    adapter.AdapterParameters.Select(pair => pair.Parameter))
                .ToArray();
            var path = Path.Combine(root, "locked-final-adapter.bin");

            FillSequential(adapterParameters, 100);
            model.SaveAdapterCheckpoint(path);
            var existingBytes = File.ReadAllBytes(path);
            FillSequential(adapterParameters, 700);

            Exception exception;
            using (new FileStream(
                       path,
                       FileMode.Open,
                       FileAccess.Read,
                       FileShare.None))
            {
                exception = Assert.ThrowsAny<Exception>(
                    () => model.SaveAdapterCheckpoint(path));
            }

            Assert.StartsWith(
                "LLM_ADAPTER_CHECKPOINT_INCOMPLETE:",
                exception.Message,
                StringComparison.Ordinal);
            Assert.Equal(existingBytes, File.ReadAllBytes(path));
            Assert.Empty(
                Directory.GetFiles(
                    root,
                    $"{Path.GetFileName(path)}.*.tmp",
                    SearchOption.TopDirectoryOnly));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LlmModel_AdapterCheckpoint_CannotCrossLoadWithFullCheckpoint()
    {
        var root = CreateRoot();
        try
        {
            var source = new LlmModel(TinyConfig(equalProjectionShapes: true));
            _ = LoraApplicator.Apply(source, Config(["q_proj", "v_proj"]));
            FillSequential(source.Parameters(), 100);
            var fullPath = Path.Combine(root, "full.bin");
            var adapterPath = Path.Combine(root, "adapter.bin");
            source.SaveCheckpoint(fullPath);
            source.SaveAdapterCheckpoint(adapterPath);

            var adapterDestination = new LlmModel(TinyConfig(equalProjectionShapes: true));
            _ = LoraApplicator.Apply(
                adapterDestination,
                Config(["q_proj", "v_proj"]));
            FillSequential(adapterDestination.Parameters(), 700);
            AssertAdapterCheckpointRejectedWithoutMutation(
                adapterDestination,
                fullPath,
                "LLM_CHECKPOINT_ARTIFACT_KIND_MISMATCH");

            var fullDestination = new LlmModel(TinyConfig(equalProjectionShapes: true));
            _ = LoraApplicator.Apply(fullDestination, Config(["q_proj", "v_proj"]));
            FillSequential(fullDestination.Parameters(), 900);
            var before = CaptureValues(fullDestination.Parameters());
            var exception = Assert.ThrowsAny<Exception>(
                () => fullDestination.LoadCheckpoint(adapterPath));

            Assert.StartsWith(
                "LLM_CHECKPOINT_ARTIFACT_KIND_MISMATCH:",
                exception.Message,
                StringComparison.Ordinal);
            AssertValues(before, fullDestination.Parameters());
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static LoraConfig Config(string[] targets)
        => new() { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = targets };

    private static ModelConfig TinyConfig(bool equalProjectionShapes = false)
        => new()
        {
            VocabSize = 16,
            ContextLen = 4,
            HiddenDim = 4,
            NumLayers = 1,
            NumHeads = 1,
            NumKvHeads = equalProjectionShapes ? 1 : 1,
            FfnHiddenDim = 8
        };

    private static LlmModel CreateDeterministicGradientModel()
    {
        var model = new LlmModel(
            new ModelConfig
            {
                VocabSize = 8,
                ContextLen = 4,
                HiddenDim = 4,
                NumLayers = 1,
                NumHeads = 2,
                NumKvHeads = 1,
                FfnHiddenDim = 8,
                RmsNormEps = 1e-5f,
                RopeTheta = 10_000f
            });
        var parameterOrdinal = 0;
        foreach (var parameter in model.BaseParameters)
        {
            for (var index = 0; index < parameter.Value.Data.Length; index++)
            {
                parameter.Value.Data[index] = parameter.Value.Shape.Rank == 1
                    ? 0.72f + 0.041f * index + 0.013f * parameterOrdinal
                    : 0.31f * MathF.Sin(0.37f * (index + 1) + 0.23f * parameterOrdinal)
                      + 0.17f * MathF.Cos(0.19f * (index + 2) - 0.11f * parameterOrdinal);
            }

            parameterOrdinal++;
        }

        return model;
    }

    private static float WeightedSum(Tensor tensor, IReadOnlyList<float> weights)
    {
        Assert.Equal(tensor.Data.Length, weights.Count);
        var total = 0f;
        for (var index = 0; index < tensor.Data.Length; index++)
            total += tensor.Data[index] * weights[index];
        return total;
    }

    private static float[] NumericalGradient(
        Tensor variable,
        Func<float> objective,
        float epsilon)
    {
        var gradient = new float[variable.Data.Length];
        for (var index = 0; index < variable.Data.Length; index++)
        {
            var original = variable.Data[index];
            variable.Data[index] = original + epsilon;
            var plus = objective();
            variable.Data[index] = original - epsilon;
            var minus = objective();
            variable.Data[index] = original;
            gradient[index] = (plus - minus) / (2f * epsilon);
        }

        return gradient;
    }

    private static void AssertArraysClose(
        IReadOnlyList<float> expected,
        IReadOnlyList<float> actual,
        float absoluteTolerance,
        float relativeTolerance)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (var index = 0; index < expected.Count; index++)
        {
            AssertClose(
                expected[index],
                actual[index],
                absoluteTolerance,
                relativeTolerance,
                $"index {index}");
        }
    }

    private static void AssertClose(
        float expected,
        float actual,
        float absoluteTolerance,
        float relativeTolerance,
        string context)
    {
        var tolerance = absoluteTolerance + relativeTolerance * MathF.Abs(expected);
        Assert.True(
            MathF.Abs(expected - actual) <= tolerance,
            $"{context}: expected {expected:R}, actual {actual:R}, tolerance {tolerance:R}.");
    }

    private static void SetBasis(LoraLinear adapter, int inputIndex, int outputIndex)
    {
        var a = adapter.AdapterParameters[0].Parameter.Value;
        var b = adapter.AdapterParameters[1].Parameter.Value;
        Array.Clear(a.Data);
        Array.Clear(b.Data);
        a.Data[inputIndex] = 1f;
        b.Data[outputIndex * adapter.Rank] = 1f;
    }

    private static void PrepareControlledSingleTokenModel(LlmModel model)
    {
        foreach (var parameter in model.BaseParameters)
            Array.Clear(parameter.Value.Data);
        Array.Fill(model.BaseParameters[1].Value.Data, 1f);
        Array.Fill(model.BaseParameters[6].Value.Data, 1f);
        Array.Fill(model.BaseParameters[10].Value.Data, 1f);
        model.BaseParameters[0].Value.Data[model.Config.HiddenDim] = 1f;
        SetIdentity(model.BaseParameters[2].Value);
        SetIdentity(model.BaseParameters[3].Value);
        SetIdentity(model.BaseParameters[4].Value);
        SetIdentity(model.BaseParameters[5].Value);
        model.BaseParameters[11].Value.Data[0] = 1f;
    }

    private static void SetIdentity(Tensor tensor)
    {
        Array.Clear(tensor.Data);
        var rows = tensor.Shape.Dimensions[0];
        var columns = tensor.Shape.Dimensions[1];
        for (var index = 0; index < Math.Min(rows, columns); index++)
            tensor.Data[(index * columns) + index] = 1f;
    }

    private static ModelSnapshot CaptureModel(LlmModel model)
        => new(
            model.Parameters().ToArray(),
            model.Parameters().Select(CaptureParameter).ToArray(),
            new[]
            {
                model.GetAttachedProjectionAdapter(0, "q_proj"),
                model.GetAttachedProjectionAdapter(0, "k_proj"),
                model.GetAttachedProjectionAdapter(0, "v_proj"),
                model.GetAttachedProjectionAdapter(0, "o_proj")
            });

    private static void AssertModelSnapshot(ModelSnapshot expected, LlmModel actual)
    {
        Assert.Equal(expected.ParameterReferences, actual.Parameters().ToArray());
        for (var index = 0; index < expected.Parameters.Length; index++)
            AssertParameterSnapshot(expected.Parameters[index], expected.ParameterReferences[index]);
        Assert.Equal(expected.Adapters,
        [
            actual.GetAttachedProjectionAdapter(0, "q_proj"),
            actual.GetAttachedProjectionAdapter(0, "k_proj"),
            actual.GetAttachedProjectionAdapter(0, "v_proj"),
            actual.GetAttachedProjectionAdapter(0, "o_proj")
        ]);
    }

    private static ParameterSnapshot CaptureParameter(Parameter parameter)
        => new(
            parameter.Value.Data.ToArray(),
            parameter.Value.RequiresGrad,
            parameter.Value.Grad,
            parameter.Value.Grad?.Data.ToArray());

    private static void AssertParameterSnapshot(ParameterSnapshot expected, Parameter actual)
    {
        Assert.Equal(expected.Values, actual.Value.Data);
        Assert.Equal(expected.RequiresGrad, actual.Value.RequiresGrad);
        Assert.Same(expected.GradientReference, actual.Value.Grad);
        Assert.Equal(expected.GradientValues, actual.Value.Grad?.Data);
    }

    private static float[][] CaptureValues(IEnumerable<Parameter> parameters)
        => parameters.Select(parameter => parameter.Value.Data.ToArray()).ToArray();

    private static void AssertValues(float[][] expected, IEnumerable<Parameter> actual)
    {
        var actualArray = actual.ToArray();
        Assert.Equal(expected.Length, actualArray.Length);
        for (var index = 0; index < expected.Length; index++)
            Assert.Equal(expected[index], actualArray[index].Value.Data);
    }

    private static void AssertParameterValues(
        IEnumerable<Parameter> expected,
        IEnumerable<Parameter> actual)
        => AssertValues(CaptureValues(expected), actual);

    private static void FillSequential(IEnumerable<Parameter> parameters, int start = 1)
    {
        var value = start;
        foreach (var parameter in parameters)
        {
            for (var index = 0; index < parameter.Value.Data.Length; index++)
                parameter.Value.Data[index] = value++;
        }
    }

    private static RawKeyedCheckpointEntry[] CreateVersionOneRawKeyedEntries(LlmModel model)
        => model.GetCheckpointParameterEntries()
            .Select((entry, index) => new RawKeyedCheckpointEntry(
                Encoding.UTF8.GetBytes(
                    index < model.BaseParameters.Count
                        ? $"base.{index:D6}"
                        : entry.Key),
                entry.Parameter.Value.Shape.Rank,
                entry.Parameter.Value.Shape.Dimensions.ToArray(),
                checked((long)entry.Parameter.Value.Data.Length * sizeof(float)),
                entry.Parameter.Value.Data.ToArray()))
            .ToArray();

    private static RawKeyedCheckpointEntry[] CloneRawKeyedEntries(
        IReadOnlyList<RawKeyedCheckpointEntry> entries)
        => entries
            .Select(entry => entry with
            {
                KeyBytes = entry.KeyBytes.ToArray(),
                Dimensions = entry.Dimensions.ToArray(),
                Values = entry.Values.ToArray()
            })
            .ToArray();

    private static RawKeyedCheckpointEntry[] ReadKeyedFixture(string path)
    {
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        Assert.Equal(unchecked((int)0xC14F5241), reader.ReadInt32());
        Assert.Equal(2, reader.ReadInt32());
        _ = reader.ReadInt32();
        var fingerprintLength = reader.ReadInt32();
        Assert.Equal(32, fingerprintLength);
        Assert.Equal(fingerprintLength, reader.ReadBytes(fingerprintLength).Length);
        var count = reader.ReadInt32();
        Assert.True(count >= 0);
        var entries = new RawKeyedCheckpointEntry[count];
        for (var entryIndex = 0; entryIndex < count; entryIndex++)
        {
            var keyLength = reader.ReadInt32();
            Assert.True(keyLength > 0);
            var keyBytes = reader.ReadBytes(keyLength);
            Assert.Equal(keyLength, keyBytes.Length);
            var rank = reader.ReadInt32();
            Assert.True(rank >= 0);
            var dimensions = new int[rank];
            for (var dimensionIndex = 0; dimensionIndex < rank; dimensionIndex++)
                dimensions[dimensionIndex] = reader.ReadInt32();
            var payloadBytes = reader.ReadInt64();
            Assert.True(payloadBytes >= 0 && payloadBytes % sizeof(float) == 0);
            var values = new float[checked((int)(payloadBytes / sizeof(float)))];
            for (var valueIndex = 0; valueIndex < values.Length; valueIndex++)
                values[valueIndex] = reader.ReadSingle();
            entries[entryIndex] = new RawKeyedCheckpointEntry(
                keyBytes,
                rank,
                dimensions,
                payloadBytes,
                values);
        }

        Assert.Equal(stream.Length, stream.Position);
        return entries;
    }

    private static void WriteKeyedFixture(
        string path,
        int version,
        IReadOnlyList<RawKeyedCheckpointEntry> entries)
    {
        using var writer = new BinaryWriter(File.Create(path));
        writer.Write(unchecked((int)0xC14F5241));
        writer.Write(version);
        writer.Write(entries.Count);
        foreach (var entry in entries)
        {
            writer.Write(entry.KeyBytes.Length);
            writer.Write(entry.KeyBytes);
            writer.Write(entry.Rank);
            foreach (var dimension in entry.Dimensions)
                writer.Write(dimension);
            writer.Write(entry.PayloadBytes);
            foreach (var value in entry.Values)
                writer.Write(value);
        }
    }

    private static void AssertCheckpointRejectedWithoutMutation(
        LlmModel destination,
        string malformedCheckpointPath,
        string expectedCode,
        string root,
        string caseName,
        string? expectedMessage = null)
    {
        var valuesBefore = CaptureValues(destination.Parameters());
        var bytesBeforePath = Path.Combine(root, caseName + "-before.bin");
        var bytesAfterPath = Path.Combine(root, caseName + "-after.bin");
        destination.SaveCheckpoint(bytesBeforePath);
        var bytesBefore = File.ReadAllBytes(bytesBeforePath);

        var exception = Assert.ThrowsAny<Exception>(
            () => destination.LoadCheckpoint(malformedCheckpointPath));

        Assert.StartsWith(expectedCode + ":", exception.Message, StringComparison.Ordinal);
        if (expectedMessage is not null)
            Assert.Equal(expectedMessage, exception.Message);
        AssertValues(valuesBefore, destination.Parameters());
        destination.SaveCheckpoint(bytesAfterPath);
        Assert.Equal(bytesBefore, File.ReadAllBytes(bytesAfterPath));
    }

    private static void AssertAdapterCheckpointRejectedWithoutMutation(
        LlmModel destination,
        string checkpointPath,
        string expectedCode)
    {
        var before = CaptureValues(destination.Parameters());

        var exception = Assert.ThrowsAny<Exception>(
            () => destination.LoadAdapterCheckpoint(checkpointPath));

        Assert.StartsWith(expectedCode + ":", exception.Message, StringComparison.Ordinal);
        AssertValues(before, destination.Parameters());
    }

    private static void WriteLegacyFixture(string path, IReadOnlyList<Parameter> parameters)
    {
        using var writer = new BinaryWriter(File.Create(path));
        writer.Write(parameters.Count);
        foreach (var parameter in parameters)
        {
            writer.Write(parameter.Value.Shape.Rank);
            foreach (var dimension in parameter.Value.Shape.Dimensions)
                writer.Write(dimension);
            foreach (var value in parameter.Value.Data)
                writer.Write(value);
        }
    }

    private static void WriteLegacyFixtureWithFinalValue(
        string path,
        IReadOnlyList<Parameter> parameters,
        float finalValue)
    {
        using var writer = new BinaryWriter(File.Create(path));
        writer.Write(parameters.Count);
        for (var parameterIndex = 0; parameterIndex < parameters.Count; parameterIndex++)
        {
            var parameter = parameters[parameterIndex];
            writer.Write(parameter.Value.Shape.Rank);
            foreach (var dimension in parameter.Value.Shape.Dimensions)
                writer.Write(dimension);

            for (var valueIndex = 0; valueIndex < parameter.Value.Data.Length; valueIndex++)
            {
                writer.Write(
                    parameterIndex == parameters.Count - 1 && valueIndex == parameter.Value.Data.Length - 1
                        ? finalValue
                        : parameter.Value.Data[valueIndex]);
            }
        }
    }

    private static void ReplaceFirstAscii(byte[] bytes, string from, string to)
    {
        var source = Encoding.ASCII.GetBytes(from);
        var replacement = Encoding.ASCII.GetBytes(to);
        Assert.Equal(source.Length, replacement.Length);
        for (var index = 0; index <= bytes.Length - source.Length; index++)
        {
            if (!bytes.AsSpan(index, source.Length).SequenceEqual(source))
                continue;
            replacement.CopyTo(bytes, index);
            return;
        }

        Assert.Fail($"Key fragment '{from}' was not found.");
    }

    private static string CreateRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), $"f36-lora-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        return root;
    }

    private sealed record ParameterSnapshot(
        float[] Values,
        bool RequiresGrad,
        Tensor? GradientReference,
        float[]? GradientValues);

    private sealed record ModelSnapshot(
        Parameter[] ParameterReferences,
        ParameterSnapshot[] Parameters,
        IAttentionProjectionAdapter?[] Adapters);

    private sealed record RawKeyedCheckpointEntry(
        byte[] KeyBytes,
        int Rank,
        int[] Dimensions,
        long PayloadBytes,
        float[] Values);

    private sealed class RecordingAdapter : Module, IAttentionProjectionAdapter
    {
        public Linear BaseProjection { get; }
        public Module RegistrationModule => this;
        public IReadOnlyList<(string Suffix, Parameter Parameter)> AdapterParameters => [];
        public bool LastTraining { get; private set; }

        public RecordingAdapter(Linear baseProjection)
        {
            BaseProjection = baseProjection;
        }

        public Tensor Forward(Tensor input, bool training)
        {
            LastTraining = training;
            return BaseProjection.Forward(input);
        }
    }
}
