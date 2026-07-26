// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using Inference;

namespace LlmFromScratch.Tests;

// Unit tests for Sampler — greedy, temperature, top-k, top-p.
// Behavioral assertion gate: specific token IDs are checked, not just "returned an int".
public class SamplerTests
{
    [Fact]
    public void Greedy_Temperature0_ReturnsArgmax()
    {
        // Logits [0, 0, 10, 0, 0] — argmax at index 2
        var logits = new Tensor(new float[] { 0f, 0f, 10f, 0f, 0f }, new TensorShape(5));

        int token = Sampler.Sample(logits, temperature: 0f);

        Assert.Equal(2, token);
    }

    [Fact]
    public void Greedy_AllEqual_ReturnsSomeToken()
    {
        var logits = new Tensor(new float[10], new TensorShape(10)); // all zeros

        int token = Sampler.Sample(logits, temperature: 0f);

        Assert.InRange(token, 0, 9);
    }

    [Fact]
    public void TopK1_TemperaturePositive_AlwaysReturnsArgmax()
    {
        // With topK=1 only the argmax survives → the result is deterministic
        var logits = new Tensor(new float[] { 1f, 2f, 100f, 0f }, new TensorShape(4));

        int token = Sampler.Sample(logits, temperature: 1.0f, topK: 1);

        Assert.Equal(2, token);
    }

    [Fact]
    public void Sample_HighTemperature_StaysInRange()
    {
        var logits = new Tensor(new float[] { 1f, 1f, 1f, 1f, 1f }, new TensorShape(5));

        for (int i = 0; i < 20; i++)
        {
            int token = Sampler.Sample(logits, temperature: 2.0f);
            Assert.InRange(token, 0, 4);
        }
    }

    [Fact]
    public void TopP1_AllTokensEligible_ResultInRange()
    {
        var logits = new Tensor(new float[] { 1f, 2f, 3f, 4f, 5f }, new TensorShape(5));

        // topP=1.0 means all tokens are allowed
        int token = Sampler.Sample(logits, temperature: 1.0f, topP: 1.0f);

        Assert.InRange(token, 0, 4);
    }

    [Fact]
    public void TopP_VerySmall_ReturnsMostLikelyToken()
    {
        // Logit 100 at index 4 makes all the other probabilities ≈ 0
        var logits = new Tensor(new float[] { 0f, 0f, 0f, 0f, 100f }, new TensorShape(5));

        int token = Sampler.Sample(logits, temperature: 1.0f, topP: 0.01f);

        Assert.Equal(4, token);
    }
}
