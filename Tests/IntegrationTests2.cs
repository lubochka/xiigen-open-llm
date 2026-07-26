// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using TensorCore.Layers;
using Embeddings;
using Inference;
using LoRA;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;
using VectorSearch;

namespace LlmFromScratch.Tests;

// Integration tests for chains involving TensorCore-based sub-projects.
// Each test verifies data flow across at least two component boundaries.
// Behavioral assertion gate: concrete shape, value, and contract properties asserted.
public class IntegrationTests2
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

    // Helper: build a model whose VocabSize matches the tokenizer vocab
    private static LlmModel ModelForBpe(BpeTokenizer bpe) => new LlmModel(new ModelConfig
    {
        VocabSize    = bpe.VocabSize,
        ContextLen   = 8,
        HiddenDim    = 16,
        NumLayers    = 1,
        NumHeads     = 2,
        NumKvHeads   = 1,
        FfnHiddenDim = 32
    });

    // ──────────────────────────────────────────────────────────────────────────
    // Chain: Tokenizer → EmbeddingModel → float[]
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Chain_Tokenize_Embed_ProducesCorrectDimension()
    {
        var bpe    = new BpeTokenizer();
        bpe.Train("hello world hello world", vocabSize: 270);
        var model  = ModelForBpe(bpe);
        var embMdl = new EmbeddingModel(model, bpe, maxLen: 8);

        float[] vec = embMdl.Embed("hello");

        Assert.Equal(model.Config.HiddenDim, vec.Length);
    }

    [Fact]
    public void Chain_Tokenize_Embed_OutputIsUnitNorm()
    {
        var bpe    = new BpeTokenizer();
        bpe.Train("the quick brown fox", vocabSize: 270);
        var model  = ModelForBpe(bpe);
        var embMdl = new EmbeddingModel(model, bpe, maxLen: 8);

        float[] vec = embMdl.Embed("the");
        float norm  = MathF.Sqrt(vec.Sum(v => v * v));

        Assert.Equal(1f, norm, precision: 4);
    }

    [Fact]
    public void Chain_Tokenize_EmbedBatch_AllVectorsHaveCorrectDimension()
    {
        var bpe    = new BpeTokenizer();
        bpe.Train("abc def ghi jkl", vocabSize: 265);
        var model  = ModelForBpe(bpe);
        var embMdl = new EmbeddingModel(model, bpe, maxLen: 8);

        var vecs = embMdl.EmbedBatch(["abc", "def", "ghi"]);

        Assert.Equal(3, vecs.Length);
        foreach (var v in vecs)
            Assert.Equal(TinyCfg.HiddenDim, v.Length);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Chain: EmbeddingModel → BruteForceIndex → nearest neighbour
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Chain_Embed_Store_Search_FindsNearestNeighbour()
    {
        var bpe    = new BpeTokenizer();
        bpe.Train("alpha beta gamma delta", vocabSize: 270);
        var model  = ModelForBpe(bpe);
        var embMdl = new EmbeddingModel(model, bpe, maxLen: 8);
        var index  = new BruteForceIndex();

        string[] texts = ["alpha", "beta", "gamma", "delta"];
        foreach (var t in texts)
            index.Add(embMdl.Embed(t), t);

        // Searching for exactly stored text must return itself as top-1
        float[] q       = embMdl.Embed("alpha");
        var results     = index.Search(q, topK: 1);

        Assert.Equal("alpha", results[0].id);
        Assert.Equal(1f, results[0].score, precision: 4);
    }

    [Fact]
    public void Chain_Embed_Store_Search_TopKReturnsKResults()
    {
        var bpe    = new BpeTokenizer();
        bpe.Train("one two three four five", vocabSize: 268);
        var model  = ModelForBpe(bpe);
        var embMdl = new EmbeddingModel(model, bpe, maxLen: 8);
        var index  = new BruteForceIndex();

        foreach (var w in new[] { "one", "two", "three", "four", "five" })
            index.Add(embMdl.Embed(w), w);

        var results = index.Search(embMdl.Embed("one"), topK: 3);

        Assert.Equal(3, results.Count);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Chain: LlmModel → KvCache → Sampler (single decode step)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Chain_ModelForward_Sampler_ProducesValidToken()
    {
        var model     = new LlmModel(TinyCfg);
        var inputTids = new Tensor(new float[] { 1f, 2f, 3f }, new TensorShape(1, 3));

        var logits    = model.Forward(inputTids);   // [1, 3, VocabSize]
        int vocabSize = TinyCfg.VocabSize;

        // Extract last-position logits [VocabSize]
        var lastData = new float[vocabSize];
        Array.Copy(logits.Data, 2 * vocabSize, lastData, 0, vocabSize);
        var lastLogits = new Tensor(lastData, new TensorShape(vocabSize));

        int nextToken = Sampler.Sample(lastLogits, temperature: 0f); // greedy

        Assert.InRange(nextToken, 0, TinyCfg.VocabSize - 1);
    }

    [Fact]
    public void Chain_KvCache_Concat_GrowsCorrectly()
    {
        using var cache = new KvCache(numLayers: 1);

        // Simulate two decode steps feeding K/V of shape [1,2,1,8] each step
        for (int step = 1; step <= 3; step++)
        {
            var k = new Tensor(new float[1 * 2 * 1 * 8], new TensorShape(1, 2, 1, 8));
            var v = new Tensor(new float[1 * 2 * 1 * 8], new TensorShape(1, 2, 1, 8));
            cache.Update(0, k, v);
            Assert.Equal(step, cache.SeqLen);
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Chain: LlmModel → LoraApplicator → only LoRA params trainable
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Chain_LoraApply_OnlyAdapterParamsAreTrainable()
    {
        var model   = new LlmModel(TinyCfg);
        var loraCfg = new LoraConfig { Rank = 2, Alpha = 4f, Dropout = 0f };

        var adapters = LoraApplicator.Apply(model, loraCfg);

        // All model base params must be frozen
        Assert.All(model.Parameters(), p =>
            Assert.False(p.Value.RequiresGrad));

        // Each adapter has exactly 2 trainable LoRA params (loraA, loraB); base weight is frozen
        foreach (var adapter in adapters)
            Assert.Equal(2, adapter.Parameters().Count(p => p.Value.RequiresGrad));
    }

    [Fact]
    public void Chain_LoraForward_ProducesNonNanOutput()
    {
        var model   = new LlmModel(TinyCfg);
        var loraCfg = new LoraConfig { Rank = 2, Alpha = 4f, Dropout = 0f };
        var adapters = LoraApplicator.Apply(model, loraCfg);

        Assert.True(adapters.Count > 0);

        var input  = new Tensor(new float[1 * TinyCfg.HiddenDim], new TensorShape(1, TinyCfg.HiddenDim));
        var output = adapters[0].Forward(input);

        Assert.All(output.Data, v => Assert.False(float.IsNaN(v)));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Chain: TensorCore backward → parameter changes after optimizer step
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Chain_ForwardBackwardStep_UpdatesLinearWeights()
    {
        var layer = new Linear(inFeatures: 4, outFeatures: 2);
        var opt   = new TrainingLoop.Optimizers.AdamW(lr: 1e-2f, weightDecay: 0f);

        float[] before = layer.Weight.Value.Data.ToArray();

        // Forward: ones → sum → backward
        var input  = new Tensor(new float[] { 1f, 1f, 1f, 1f }, new TensorShape(1, 4));
        var output = layer.Forward(input);
        var loss   = TensorOps.Sum(output);
        loss.Backward();

        opt.Step(layer.Parameters(), lr: 1e-2f);

        // Weights must have changed
        bool changed = layer.Weight.Value.Data
            .Zip(before, (a, b) => MathF.Abs(a - b))
            .Any(diff => diff > 1e-9f);

        Assert.True(changed, "Optimizer step must update at least one weight");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Chain: TextGenerator produces a string (smoke test)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Chain_TextGenerator_Generate_ReturnsString()
    {
        var bpe   = new BpeTokenizer();
        bpe.Train("hello world hello world test test test", vocabSize: 270);
        var model = ModelForBpe(bpe);
        var gen   = new TextGenerator(model, bpe);

        // Just 3 new tokens — fast for a test
        string result = gen.Generate("hello", maxNewTokens: 3, temperature: 0f);

        // Must return a string (possibly empty if all EOS, that's fine)
        Assert.NotNull(result);
    }
}
