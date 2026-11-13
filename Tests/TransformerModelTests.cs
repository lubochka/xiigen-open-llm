// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using TensorCore.Layers;
using LoRA;
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

        var root = Path.Combine(Path.GetTempPath(), $"llm-checkpoint-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "model.bin");
        Directory.CreateDirectory(root);

        try
        {
            model.SaveCheckpoint(path);

            // Corrupt the model weights
            foreach (var p in model.Parameters())
                Array.Fill(p.Value.Data, 0f);

            model.LoadCheckpoint(path);
            var after = model.Forward(ids).Data;

            for (int i = 0; i < before.Length; i++)
                Assert.Equal(before[i], after[i], precision: 4);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LlmModel_LoadCheckpointStream_RoundTripsTwiceAndLeavesCallerStreamOpen()
    {
        var source = new LlmModel(TinyCfg);
        var ids = new Tensor(new float[] { 1f, 2f }, new TensorShape(1, 2));
        var expected = source.Forward(ids).Data.ToArray();
        var root = Path.Combine(Path.GetTempPath(), $"llm-checkpoint-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "model.bin");
        Directory.CreateDirectory(root);

        try
        {
            source.SaveCheckpoint(path);
            using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);
            var first = new LlmModel(TinyCfg);
            first.LoadCheckpoint(stream);
            Assert.True(stream.CanRead);
            Assert.Equal(stream.Length, stream.Position);

            stream.Position = 0;
            var second = new LlmModel(TinyCfg);
            second.LoadCheckpoint(stream);
            Assert.True(stream.CanRead);
            Assert.Equal(stream.Length, stream.Position);
            Assert.Equal(expected, first.Forward(ids).Data);
            Assert.Equal(expected, second.Forward(ids).Data);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LlmModel_CheckpointParameterCountClassifier_AcceptsOnlyTypedCountMismatch()
    {
        var loraConfig = new LoraConfig
        {
            Rank = 2,
            Alpha = 4,
            Dropout = 0f,
            TargetModules = ["q_proj"]
        };
        using var checkpoint = new MemoryStream();
        var source = new LlmModel(TinyCfg);
        _ = LoraApplicator.Apply(source, loraConfig);
        source.SaveCheckpointToStream(checkpoint);

        checkpoint.Position = 0;
        var baseOnly = new LlmModel(TinyCfg);
        var mismatch = Assert.ThrowsAny<Exception>(
            () => baseOnly.LoadCheckpoint(checkpoint));
        Assert.True(LlmModel.IsCheckpointParameterCountMismatch(mismatch));

        checkpoint.Position = 0;
        var adapterAware = new LlmModel(TinyCfg);
        _ = LoraApplicator.Apply(adapterAware, loraConfig);
        adapterAware.LoadCheckpoint(checkpoint);
    }

    [Fact]
    public void LlmModel_CheckpointParameterCountClassifier_RejectsOtherValidationFailures()
    {
        using var checkpoint = new MemoryStream();
        new LlmModel(TinyCfg).SaveCheckpointToStream(checkpoint);
        var checkpointBytes = checkpoint.ToArray();

        using var truncated = new MemoryStream(
            checkpointBytes.Take(checkpointBytes.Length - 1).ToArray());
        var malformed = Assert.ThrowsAny<Exception>(
            () => new LlmModel(TinyCfg).LoadCheckpoint(truncated));
        Assert.False(LlmModel.IsCheckpointParameterCountMismatch(malformed));

        using var compatibleBytes = new MemoryStream(checkpointBytes);
        var configMismatch = Assert.ThrowsAny<Exception>(
            () => new LlmModel(DifferentVocabularyConfig())
                .LoadCheckpoint(compatibleBytes));
        Assert.False(LlmModel.IsCheckpointParameterCountMismatch(configMismatch));
    }

    [Fact]
    public void LlmModel_SaveCheckpoint_NoTmpFileLeftBehind()
    {
        var model = new LlmModel(TinyCfg);
        var root = Path.Combine(Path.GetTempPath(), $"llm-checkpoint-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "model.bin");
        Directory.CreateDirectory(root);

        try
        {
            model.SaveCheckpoint(path);

            Assert.True(File.Exists(path));
            Assert.Empty(Directory.EnumerateFiles(
                root,
                $"{Path.GetFileName(path)}.*.tmp"));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LlmModel_SaveCheckpoint_PublishesLoadableFileWithoutTouchingLegacyTempSentinel()
    {
        var root = Path.Combine(Path.GetTempPath(), $"llm-checkpoint-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "model.bin");
        var legacyTemporaryPath = path + ".tmp";
        var sentinel = new byte[] { 0x11, 0x22, 0x33 };
        Directory.CreateDirectory(root);

        try
        {
            File.WriteAllBytes(legacyTemporaryPath, sentinel);
            var source = new LlmModel(TinyCfg);
            var ids = new Tensor(new float[] { 1f, 2f }, new TensorShape(1, 2));
            var expected = source.Forward(ids).Data.ToArray();

            source.SaveCheckpoint(path);

            Assert.True(File.Exists(path));
            Assert.True(new FileInfo(path).Length > 0);
            Assert.Equal(sentinel, File.ReadAllBytes(legacyTemporaryPath));
            Assert.Empty(Directory.EnumerateFiles(
                root,
                $"{Path.GetFileName(path)}.*.tmp"));

            var destination = new LlmModel(TinyCfg);
            destination.LoadCheckpoint(path);
            Assert.Equal(expected, destination.Forward(ids).Data);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LlmModel_SaveCheckpoint_WhenPublicationFails_ThrowsIncompleteAndCleansOwnedTemp()
    {
        var root = Path.Combine(Path.GetTempPath(), $"llm-checkpoint-{Guid.NewGuid():N}");
        var finalPath = Path.Combine(root, "published-directory");
        Directory.CreateDirectory(finalPath);

        try
        {
            var exception = Assert.ThrowsAny<Exception>(() => new LlmModel(TinyCfg).SaveCheckpoint(finalPath));

            Assert.Equal("CheckpointValidationException", exception.GetType().Name);
            Assert.StartsWith(
                "LLM_CHECKPOINT_INCOMPLETE:",
                exception.Message,
                StringComparison.Ordinal);
            Assert.Empty(Directory.EnumerateFiles(
                root,
                $"{Path.GetFileName(finalPath)}.*.tmp"));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
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

    private static ModelConfig DifferentVocabularyConfig()
        => new()
        {
            VocabSize = TinyCfg.VocabSize + 1,
            ContextLen = TinyCfg.ContextLen,
            HiddenDim = TinyCfg.HiddenDim,
            NumLayers = TinyCfg.NumLayers,
            NumHeads = TinyCfg.NumHeads,
            NumKvHeads = TinyCfg.NumKvHeads,
            FfnHiddenDim = TinyCfg.FfnHiddenDim,
            RopeTheta = TinyCfg.RopeTheta,
            RmsNormEps = TinyCfg.RmsNormEps,
        };
}
