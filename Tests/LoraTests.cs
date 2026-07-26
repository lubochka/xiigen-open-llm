// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using TensorCore.Layers;
using LoRA;
using TransformerModel;
using TransformerModel.Config;

namespace LlmFromScratch.Tests;

// Unit tests for LoRA — config, LoraLinear adapter, and LoraApplicator.
// Behavioral assertion gate: concrete shape, gradient, and value assertions.
public class LoraTests
{
    // ──────────────────────────────────────────────────────────────────────────
    // LoraConfig
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void LoraConfig_Scaling_IsAlphaOverRank()
    {
        var cfg = new LoraConfig { Rank = 8, Alpha = 16f };
        Assert.Equal(2f, cfg.Scaling, precision: 6);
    }

    [Fact]
    public void LoraConfig_DefaultTargetModules_ContainsQAndV()
    {
        var cfg = new LoraConfig();
        Assert.Contains("q_proj", cfg.TargetModules);
        Assert.Contains("v_proj", cfg.TargetModules);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // LoraLinear
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void LoraLinear_Forward_OutputShapeMatchesBase()
    {
        var base_  = new Linear(inFeatures: 8, outFeatures: 4, useBias: false);
        var config = new LoraConfig { Rank = 2, Alpha = 4f, Dropout = 0f };
        var lora   = new LoraLinear(base_, config);

        var input  = new Tensor(new float[3 * 8], new TensorShape(3, 8));
        var output = lora.Forward(input);

        // Shape must match the base linear: [3, 4]
        Assert.Equal(new[] { 3, 4 }, output.Shape.Dimensions);
    }

    [Fact]
    public void LoraLinear_BaseWeights_AreFrozen()
    {
        var base_  = new Linear(inFeatures: 6, outFeatures: 4, useBias: false);
        var config = new LoraConfig { Rank = 2, Alpha = 4f };
        _ = new LoraLinear(base_, config);

        Assert.False(base_.Weight.Value.RequiresGrad,
            "Base weights must be frozen after LoraLinear wraps them");
    }

    [Fact]
    public void LoraLinear_LoraParams_RequireGrad()
    {
        var base_  = new Linear(inFeatures: 6, outFeatures: 4, useBias: false);
        var config = new LoraConfig { Rank = 2, Alpha = 4f };
        var lora   = new LoraLinear(base_, config);

        var trainable = lora.Parameters().Where(p => p.Value.RequiresGrad).ToList();

        // Only loraA and loraB are trainable (2 parameters)
        Assert.Equal(2, trainable.Count);
    }

    [Fact]
    public void LoraLinear_InitiallySameAsBase_BecauseLoraB_IsZero()
    {
        // With LoraB = zeros, LoRA delta = 0, so output must equal base output exactly
        var base_  = new Linear(inFeatures: 4, outFeatures: 4, useBias: false);
        var config = new LoraConfig { Rank = 2, Alpha = 2f, Dropout = 0f };
        var lora   = new LoraLinear(base_, config);

        // Give loraB data that is already zero (it is by default)
        var input     = new Tensor(new float[] { 1f, 0f, 0f, 0f }, new TensorShape(1, 4));
        var baseOut   = base_.Forward(input);
        var loraOut   = lora.Forward(input);

        for (int i = 0; i < baseOut.Shape.TotalElements; i++)
            Assert.Equal(baseOut.Data[i], loraOut.Data[i], precision: 5);
    }

    [Fact]
    public void LoraLinear_NonZeroLoraB_ChangesOutput()
    {
        // After setting loraB to non-zero, output should differ from base
        var base_  = new Linear(inFeatures: 4, outFeatures: 4, useBias: false);
        var config = new LoraConfig { Rank = 2, Alpha = 2f, Dropout = 0f };
        var lora   = new LoraLinear(base_, config);

        // loraA and loraB are the two trainable params (RequiresGrad=true).
        // Take the second one (loraB — the output projection matrix).
        var loraB = lora.Parameters().Where(p => p.Value.RequiresGrad).ElementAt(1);
        Array.Fill(loraB.Value.Data, 1f);

        var input   = new Tensor(new float[] { 1f, 1f, 1f, 1f }, new TensorShape(1, 4));
        var baseOut = base_.Forward(input);
        var loraOut = lora.Forward(input);

        bool differs = false;
        for (int i = 0; i < baseOut.Shape.TotalElements; i++)
            if (MathF.Abs(baseOut.Data[i] - loraOut.Data[i]) > 1e-6f) { differs = true; break; }

        Assert.True(differs, "LoRA output must differ from base once loraB is nonzero");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // LoraApplicator
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void LoraApplicator_Apply_FreezesAllBaseParams()
    {
        var cfg   = new ModelConfig { VocabSize = 64, ContextLen = 8, HiddenDim = 16,
                                      NumLayers = 1, NumHeads = 2, NumKvHeads = 1,
                                      FfnHiddenDim = 32 };
        var model  = new LlmModel(cfg);
        var loraCfg = new LoraConfig { Rank = 2, Alpha = 4f, Dropout = 0f };

        LoraApplicator.Apply(model, loraCfg);

        // Every base parameter must have RequiresGrad = false after Apply
        Assert.All(model.Parameters(), p =>
            Assert.False(p.Value.RequiresGrad, $"Parameter should be frozen"));
    }

    [Fact]
    public void LoraApplicator_Apply_ReturnsAdapters()
    {
        var cfg   = new ModelConfig { VocabSize = 64, ContextLen = 8, HiddenDim = 16,
                                      NumLayers = 1, NumHeads = 2, NumKvHeads = 1,
                                      FfnHiddenDim = 32 };
        var model   = new LlmModel(cfg);
        var loraCfg = new LoraConfig { Rank = 2, Alpha = 4f, TargetModules = ["q_proj", "v_proj"] };

        var adapters = LoraApplicator.Apply(model, loraCfg);

        // Should return at least one adapter per layer (q_proj and v_proj)
        Assert.True(adapters.Count > 0, "LoraApplicator must return at least one adapter");
    }
}
