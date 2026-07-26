// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using TensorCore.Layers;
using TransformerModel;
using TransformerModel.Config;
using TransformerModel.Layers;

namespace LlmFromScratch.Tests;

// Unit tests for TransformerModel layers built on TensorCore.
// Behavioral assertion gate: shapes and numeric properties are asserted.
public class TransformerModelTests
{
    private static readonly ModelConfig TinyCfg = new()
    {
        VocabSize    = 64,
        ContextLen   = 8,
        HiddenDim    = 16,
        NumLayers    = 1,
        NumHeads     = 2,
        NumKvHeads   = 1,
        FfnHiddenDim = 32
    };

    // ──────────────────────────────────────────────────────────────────────────
    // RmsNorm
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void RmsNorm_OutputShape_Unchanged()
    {
        var norm = new RmsNorm(dim: 16, eps: 1e-5f);
        var rng  = new Random(1);
        var data = Enumerable.Range(0, 2 * 4 * 16).Select(_ => (float)rng.NextDouble()).ToArray();
        var x    = new Tensor(data, new TensorShape(2, 4, 16));

        var out_ = norm.Forward(x);

        Assert.Equal(x.Shape.Dimensions, out_.Shape.Dimensions);
    }

    [Fact]
    public void RmsNorm_ZeroInput_DoesNotThrow()
    {
        var norm = new RmsNorm(dim: 4, eps: 1e-5f);
        var x    = new Tensor(new float[1 * 1 * 4], new TensorShape(1, 1, 4));

        var ex = Record.Exception(() => norm.Forward(x));
        Assert.Null(ex);
    }

    [Fact]
    public void RmsNorm_NormalisesEachVector_ApproximatelyToUnitRms()
    {
        // When gamma = ones, each normalised vector should have RMS ≈ 1
        var norm = new RmsNorm(dim: 4, eps: 1e-5f);
        var x    = new Tensor(new float[] { 1f, 2f, 3f, 4f }, new TensorShape(1, 1, 4));

        var out_ = norm.Forward(x);

        float sumSq = out_.Data.Sum(v => v * v);
        float rms   = MathF.Sqrt(sumSq / 4f);
        Assert.Equal(1f, rms, precision: 4);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // RotaryEmbedding
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void RotaryEmbedding_GetFrequencies_OutputShape()
    {
        var rope        = new RotaryEmbedding(dim: 8, theta: 10000f);
        var (cos, sin)  = rope.GetFrequencies(seqLen: 4);

        // Should be [seqLen, dim/2]
        Assert.Equal(new[] { 4, 4 }, cos.Shape.Dimensions);
        Assert.Equal(new[] { 4, 4 }, sin.Shape.Dimensions);
    }

    [Fact]
    public void RotaryEmbedding_Apply_PreservesNorm()
    {
        // RoPE is a rotation — it must preserve the L2 norm of each vector
        var rope       = new RotaryEmbedding(dim: 4, theta: 10000f);
        var (cos, sin) = rope.GetFrequencies(seqLen: 2);

        var data = new float[] { 1f, 0f, 0f, 1f,  // head 0, t=0
                                  0f, 1f, 1f, 0f }; // head 0, t=1
        var x    = new Tensor(data, new TensorShape(1, 1, 2, 4)); // [B,H,T,D]

        var rotated = RotaryEmbedding.Apply(x, cos, sin);

        // Norm of first vector before and after
        float normBefore = MathF.Sqrt(1f + 0f + 0f + 1f);
        float normAfter  = MathF.Sqrt(rotated.Data.Take(4).Sum(v => v * v));
        Assert.Equal(normBefore, normAfter, precision: 4);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // LlmModel — forward pass
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void LlmModel_Forward_OutputShape_IsBatchSeqVocab()
    {
        var model  = new LlmModel(TinyCfg);
        var ids    = new Tensor(new float[] { 1f, 2f, 3f, 4f }, new TensorShape(1, 4));

        var logits = model.Forward(ids);

        Assert.Equal(new[] { 1, 4, TinyCfg.VocabSize }, logits.Shape.Dimensions);
    }

    [Fact]
    public void LlmModel_ForwardHidden_OutputShape_IsBatchSeqHidden()
    {
        var model  = new LlmModel(TinyCfg);
        var ids    = new Tensor(new float[] { 5f, 6f }, new TensorShape(1, 2));

        var hidden = model.ForwardHidden(ids);

        Assert.Equal(new[] { 1, 2, TinyCfg.HiddenDim }, hidden.Shape.Dimensions);
    }

    [Fact]
    public void LlmModel_Forward_NaNFree()
    {
        var model  = new LlmModel(TinyCfg);
        var ids    = new Tensor(new float[] { 0f, 1f, 2f }, new TensorShape(1, 3));

        var logits = model.Forward(ids);

        Assert.All(logits.Data, v => Assert.False(float.IsNaN(v), $"NaN in logits: {v}"));
    }

    [Fact]
    public void LlmModel_SaveAndLoad_RoundTrip()
    {
        var model = new LlmModel(TinyCfg);
        var ids   = new Tensor(new float[] { 1f, 2f }, new TensorShape(1, 2));

        var before = model.Forward(ids).Data.ToArray();

        var tmp = Path.GetTempFileName() + ".bin";
        model.SaveCheckpoint(tmp);

        // Corrupt the model weights
        foreach (var p in model.Parameters())
            Array.Fill(p.Value.Data, 0f);

        model.LoadCheckpoint(tmp);
        File.Delete(tmp);

        var after = model.Forward(ids).Data;

        for (int i = 0; i < before.Length; i++)
            Assert.Equal(before[i], after[i], precision: 4);
    }

    [Fact]
    public void LlmModel_SaveCheckpoint_NoTmpFileLeftBehind()
    {
        var model = new LlmModel(TinyCfg);
        var path  = Path.GetTempFileName() + ".bin";

        model.SaveCheckpoint(path);

        Assert.True(File.Exists(path));
        Assert.False(File.Exists(path + ".tmp"), ".tmp file must not remain after atomic save");
        File.Delete(path);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // TransformerBlock
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void TransformerBlock_Forward_OutputShape_Unchanged()
    {
        var block = new TransformerBlock(TinyCfg);
        var data  = new float[1 * 4 * TinyCfg.HiddenDim];
        var x     = new Tensor(data, new TensorShape(1, 4, TinyCfg.HiddenDim));

        var out_ = block.Forward(x);

        Assert.Equal(x.Shape.Dimensions, out_.Shape.Dimensions);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // ModelConfig computed properties (existing tests kept for regression)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void ModelConfig_HeadDim_IsHiddenOverHeads()
    {
        var cfg = new ModelConfig { HiddenDim = 512, NumHeads = 8 };
        Assert.Equal(64, cfg.HeadDim);
    }

    [Fact]
    public void ModelConfig_KvGroupSize_IsHeadsOverKvHeads()
    {
        var cfg = new ModelConfig { NumHeads = 8, NumKvHeads = 2 };
        Assert.Equal(4, cfg.KvGroupSize);
    }
}
