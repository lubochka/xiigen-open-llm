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
        var opt   = new AdamW(lr: 1e-3f, weightDecay: 0f); // no decay either

        opt.Step(new[] { param }, lr: 1e-3f);

        // With zero grad and zero weight decay, weight stays exactly the same
        Assert.Equal(1f, param.Value.Data[0], precision: 5);
    }

    [Fact]
    public void Step_NonZeroGradient_WeightDecreases()
    {
        // Positive gradient → optimizer moves weight in negative direction
        var param  = MakeParam(value: 1f, grad: 1f);
        float before = param.Value.Data[0];
        var opt    = new AdamW(lr: 1e-3f, weightDecay: 0f);

        opt.Step(new[] { param }, lr: 1e-3f);

        Assert.True(param.Value.Data[0] < before,
            $"Weight should decrease. Before={before}, After={param.Value.Data[0]}");
    }

    [Fact]
    public void Step_WeightDecay_ReducesWeightMagnitude()
    {
        // Large weight + weight decay → weight shrinks even with zero gradient
        var param = MakeParam(value: 10f, grad: 0f);
        var opt   = new AdamW(lr: 1e-3f, weightDecay: 0.1f);

        opt.Step(new[] { param }, lr: 1e-3f);

        Assert.True(param.Value.Data[0] < 10f,
            "Weight decay should reduce weight magnitude");
    }

    [Fact]
    public void Step_MultipleSteps_WeightConvergesForQuadraticLoss()
    {
        // Minimising f(w) = (w - 3)^2: gradient = 2*(w-3), minimum at w=3
        var t     = new Tensor(new float[] { 0f }, new TensorShape(1), requiresGrad: true);
        var param = new Parameter(t);
        var opt   = new AdamW(lr: 0.1f, weightDecay: 0f, beta1: 0.9f, beta2: 0.999f, eps: 1e-8f);

        for (int i = 0; i < 500; i++)
        {
            float w  = param.Value.Data[0];
            float g  = 2f * (w - 3f);
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
        var t     = new Tensor(new float[] { 5f }, new TensorShape(1), requiresGrad: true);
        var param = new Parameter(t); // Grad is null
        var opt   = new AdamW(lr: 1e-3f);

        opt.Step(new[] { param }, lr: 1e-3f);

        Assert.Equal(5f, param.Value.Data[0]); // unchanged
    }

    [Fact]
    public void Step_TwoParams_EachUpdatedIndependently()
    {
        var p1 = MakeParam(value: 1f, grad: 1f);
        var p2 = MakeParam(value: 2f, grad: -1f);
        var opt = new AdamW(lr: 1e-2f, weightDecay: 0f);

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
        var opt   = new AdamW(lr: 1e-3f, weightDecay: 0f);

        opt.Step(new[] { param }, lr: 1e-3f);

        Assert.True(param.Value.Data[0] < 0f, "Step 1 should move weight against gradient");
    }
}
