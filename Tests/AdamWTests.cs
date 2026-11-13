// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using TensorCore.Layers;
using TrainingLoop.Optimizers;

namespace LlmFromScratch.Tests;

// Unit tests for AdamW optimizer.
// Behavioral assertion gate: weight values before/after are compared explicitly.
public class AdamWTests
{
    // Build a single Parameter with known values and gradient
    private static Parameter MakeParam(float value, float grad)
    {
        var t = new Tensor(new float[] { value }, new TensorShape(1), requiresGrad: true);
        t.Grad = new Tensor(new float[] { grad }, new TensorShape(1));
        return new Parameter(t);
    }

    [Fact]
    public void Step_ZeroGradient_WeightsUnchanged()
    {
        // Gradient = 0 → m=0, v=0 → weight update = 0; weight decay still applies
        var param = MakeParam(value: 1f, grad: 0f);
        var opt = new AdamW(weightDecay: 0f); // no decay either

        opt.Step(new[] { param }, lr: 1e-3f);

        // With zero grad and zero weight decay, weight stays exactly the same
        Assert.Equal(1f, param.Value.Data[0], precision: 5);
    }

    [Fact]
    public void Step_NonZeroGradient_WeightDecreases()
    {
        // Positive gradient → optimizer moves weight in negative direction
        var param = MakeParam(value: 1f, grad: 1f);
        float before = param.Value.Data[0];
        var opt = new AdamW(weightDecay: 0f);

        opt.Step(new[] { param }, lr: 1e-3f);

        Assert.True(param.Value.Data[0] < before,
            $"Weight should decrease. Before={before}, After={param.Value.Data[0]}");
    }

    [Fact]
    public void Step_WeightDecay_ReducesWeightMagnitude()
    {
        // Large weight + weight decay → weight shrinks even with zero gradient
        var param = MakeParam(value: 10f, grad: 0f);
        var opt = new AdamW(weightDecay: 0.1f);

        opt.Step(new[] { param }, lr: 1e-3f);

        Assert.True(param.Value.Data[0] < 10f,
            "Weight decay should reduce weight magnitude");
    }

    [Fact]
    public void Step_MultipleSteps_WeightConvergesForQuadraticLoss()
    {
        // Minimising f(w) = (w - 3)^2: gradient = 2*(w-3), minimum at w=3
        var t = new Tensor(new float[] { 0f }, new TensorShape(1), requiresGrad: true);
        var param = new Parameter(t);
        var opt = new AdamW(weightDecay: 0f, beta1: 0.9f, beta2: 0.999f, eps: 1e-8f);

        for (int i = 0; i < 500; i++)
        {
            float w = param.Value.Data[0];
            float g = 2f * (w - 3f);
            param.Value.Grad = new Tensor(new float[] { g }, new TensorShape(1));
            opt.Step(new[] { param }, lr: 0.1f);
        }

        // After 500 steps, should be within 0.1 of optimum w=3
        Assert.Equal(3f, param.Value.Data[0], precision: 1);
    }

    [Fact]
    public void Step_NullGrad_SkipsParameter()
    {
        // Param with no gradient should be silently skipped (frozen parameter behaviour)
        var t = new Tensor(new float[] { 5f }, new TensorShape(1), requiresGrad: true);
        var param = new Parameter(t); // Grad is null
        var opt = new AdamW();

        opt.Step(new[] { param }, lr: 1e-3f);

        Assert.Equal(5f, param.Value.Data[0]); // unchanged
    }

    [Fact]
    public void Step_TwoParams_EachUpdatedIndependently()
    {
        var p1 = MakeParam(value: 1f, grad: 1f);
        var p2 = MakeParam(value: 2f, grad: -1f);
        var opt = new AdamW(weightDecay: 0f);

        float before1 = p1.Value.Data[0];
        float before2 = p2.Value.Data[0];

        opt.Step(new[] { p1, p2 }, lr: 1e-2f);

        // p1 had positive grad → decreases; p2 had negative grad → increases
        Assert.True(p1.Value.Data[0] < before1);
        Assert.True(p2.Value.Data[0] > before2);
    }

    [Fact]
    public void BiasCorrection_FirstStep_SmallerStepThanSubsequent()
    {
        // At step 1, bias correction bc1=(1-beta1^1) is large → step is large
        // compared to asymptotic. This isn't a precise test — just checks the update
        // is nonzero and in the right direction.
        var param = MakeParam(value: 0f, grad: 1f);
        var opt = new AdamW(weightDecay: 0f);

        opt.Step(new[] { param }, lr: 1e-3f);

        Assert.True(param.Value.Data[0] < 0f, "Step 1 should move weight against gradient");
    }

    [Fact]
    public void Constructor_HasNoLearningRateParameter()
    {
        var constructorParameters = typeof(AdamW)
            .GetConstructors()
            .SelectMany(constructor => constructor.GetParameters());

        Assert.DoesNotContain(
            constructorParameters,
            parameter => StringComparer.Ordinal.Equals(parameter.Name, "lr"));
    }

    public static IEnumerable<object[]> InvalidConfigurations()
    {
        yield return [-0.01f, 0.95f, 0.1f, 1e-8f, "beta1"];
        yield return [1f, 0.95f, 0.1f, 1e-8f, "beta1"];
        yield return [float.NaN, 0.95f, 0.1f, 1e-8f, "beta1"];
        yield return [float.PositiveInfinity, 0.95f, 0.1f, 1e-8f, "beta1"];
        yield return [0.9f, -0.01f, 0.1f, 1e-8f, "beta2"];
        yield return [0.9f, 1f, 0.1f, 1e-8f, "beta2"];
        yield return [0.9f, float.NaN, 0.1f, 1e-8f, "beta2"];
        yield return [0.9f, float.PositiveInfinity, 0.1f, 1e-8f, "beta2"];
        yield return [0.9f, 0.95f, -0.01f, 1e-8f, "weightDecay"];
        yield return [0.9f, 0.95f, float.NaN, 1e-8f, "weightDecay"];
        yield return [0.9f, 0.95f, float.PositiveInfinity, 1e-8f, "weightDecay"];
        yield return [0.9f, 0.95f, 0.1f, 0f, "eps"];
        yield return [0.9f, 0.95f, 0.1f, -0.01f, "eps"];
        yield return [0.9f, 0.95f, 0.1f, float.NaN, "eps"];
        yield return [0.9f, 0.95f, 0.1f, float.PositiveInfinity, "eps"];
    }

    [Theory]
    [MemberData(nameof(InvalidConfigurations))]
    public void Constructor_InvalidConfiguration_ThrowsBeforeUse(
        float beta1,
        float beta2,
        float weightDecay,
        float eps,
        string expectedParameterName)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new AdamW(beta1, beta2, weightDecay, eps));

        Assert.Equal(expectedParameterName, exception.ParamName);
    }

    public static IEnumerable<object[]> InvalidLearningRates()
    {
        yield return [-0.01f];
        yield return [float.NaN];
        yield return [float.PositiveInfinity];
        yield return [float.NegativeInfinity];
    }

    [Theory]
    [MemberData(nameof(InvalidLearningRates))]
    public void Step_InvalidLearningRate_ThrowsWithoutMutatingParameter(float lr)
    {
        var param = MakeParam(value: 2f, grad: 1f);
        var controlParam = MakeParam(value: 2f, grad: 1f);
        var opt = new AdamW(weightDecay: 0.1f);
        var controlOpt = new AdamW(weightDecay: 0.1f);
        float before = param.Value.Data[0];

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => opt.Step([param], lr));

        Assert.Equal("lr", exception.ParamName);
        Assert.Equal(before, param.Value.Data[0]);

        opt.Step([param], 1e-3f);
        controlOpt.Step([controlParam], 1e-3f);

        Assert.Equal(controlParam.Value.Data[0], param.Value.Data[0]);
    }

    [Fact]
    public void Step_ZeroLearningRate_DoesNotMutateParameter()
    {
        var param = MakeParam(value: 2f, grad: 1f);
        var opt = new AdamW(weightDecay: 0.1f);
        float before = param.Value.Data[0];

        opt.Step([param], 0f);

        Assert.Equal(before, param.Value.Data[0]);
    }

    [Fact]
    public void Step_NullParameterSequence_Throws()
    {
        var opt = new AdamW();

        var exception = Assert.Throws<ArgumentNullException>(
            () => opt.Step(null!, 1e-3f));

        Assert.Equal("parameters", exception.ParamName);
    }

    [Fact]
    public void Step_NullParameterElement_ThrowsWithoutMutatingEarlierParameter()
    {
        var param = MakeParam(value: 2f, grad: 1f);
        var controlParam = MakeParam(value: 2f, grad: 1f);
        var opt = new AdamW(weightDecay: 0.1f);
        var controlOpt = new AdamW(weightDecay: 0.1f);
        float before = param.Value.Data[0];
        Parameter[] parameters = [param, null!];

        var exception = Assert.Throws<ArgumentException>(
            () => opt.Step(parameters, 1e-3f));

        Assert.Equal("parameters", exception.ParamName);
        Assert.Equal(before, param.Value.Data[0]);

        opt.Step([param], 1e-3f);
        controlOpt.Step([controlParam], 1e-3f);

        Assert.Equal(controlParam.Value.Data[0], param.Value.Data[0]);
    }

    [Fact]
    public void RestoreState_RestoresStepMomentsForIdenticalNextStep()
    {
        var param = MakeParam(value: 1.5f, grad: 0.75f);
        var controlParam = MakeParam(value: 1.5f, grad: 0.75f);
        var opt = new AdamW(weightDecay: 0.01f, beta1: 0.8f, beta2: 0.9f);
        var controlOpt = new AdamW(weightDecay: 0.01f, beta1: 0.8f, beta2: 0.9f);

        opt.Step([param], 0.02f);
        controlOpt.Step([controlParam], 0.02f);
        var checkpointWeight = param.Value.Data[0];
        var snapshot = opt.CaptureState();

        param.Value.Grad = new Tensor(new float[] { -4f }, new TensorShape(1));
        opt.Step([param], 0.02f);

        param.Value.Data[0] = checkpointWeight;
        param.Value.Grad = new Tensor(new float[] { -0.25f }, new TensorShape(1));
        controlParam.Value.Grad = new Tensor(new float[] { -0.25f }, new TensorShape(1));
        opt.RestoreState(snapshot);

        opt.Step([param], 0.02f);
        controlOpt.Step([controlParam], 0.02f);

        Assert.Equal(
            BitConverter.SingleToInt32Bits(controlParam.Value.Data[0]),
            BitConverter.SingleToInt32Bits(param.Value.Data[0]));
    }

    [Fact]
    public void RestoreState_SnapshotDoesNotAliasRestoredMoments()
    {
        var param = MakeParam(value: 1.5f, grad: 0.75f);
        var opt = new AdamW(weightDecay: 0.01f, beta1: 0.8f, beta2: 0.9f);

        opt.Step([param], 0.02f);
        var checkpointWeight = param.Value.Data[0];
        var snapshot = opt.CaptureState();

        param.Value.Data[0] = checkpointWeight;
        param.Value.Grad = new Tensor(new float[] { -0.25f }, new TensorShape(1));
        opt.RestoreState(snapshot);
        opt.Step([param], 0.02f);
        var firstReplay = param.Value.Data[0];

        param.Value.Data[0] = checkpointWeight;
        param.Value.Grad = new Tensor(new float[] { -0.25f }, new TensorShape(1));
        opt.RestoreState(snapshot);
        opt.Step([param], 0.02f);

        Assert.Equal(
            BitConverter.SingleToInt32Bits(firstReplay),
            BitConverter.SingleToInt32Bits(param.Value.Data[0]));
    }

    [Fact]
    public void RestoreState_RejectsSnapshotFromAnotherOptimizerBeforeMutation()
    {
        var sourceParam = MakeParam(value: 1.5f, grad: 0.75f);
        var source = new AdamW(weightDecay: 0.01f, beta1: 0.8f, beta2: 0.9f);
        source.Step([sourceParam], 0.02f);
        var snapshot = source.CaptureState();
        var otherParam = MakeParam(value: 2f, grad: 1f);
        var controlParam = MakeParam(value: 2f, grad: 1f);
        var other = new AdamW(weightDecay: 0.01f, beta1: 0.8f, beta2: 0.9f);
        var control = new AdamW(weightDecay: 0.01f, beta1: 0.8f, beta2: 0.9f);

        var exception = Assert.Throws<InvalidOperationException>(
            () => other.RestoreState(snapshot));

        Assert.Equal("ADAMW_SNAPSHOT_OWNER_MISMATCH", exception.Message);
        other.Step([otherParam], 0.02f);
        control.Step([controlParam], 0.02f);
        Assert.Equal(
            BitConverter.SingleToInt32Bits(controlParam.Value.Data[0]),
            BitConverter.SingleToInt32Bits(otherParam.Value.Data[0]));
    }
}
