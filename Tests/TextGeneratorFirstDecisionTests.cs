// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
using Inference;
using TensorCore.Core;
using Tokenizer;
using TransformerModel;
using TransformerModel.Caching;
using TransformerModel.Config;

namespace LlmFromScratch.Tests;

public sealed class TextGeneratorFirstDecisionTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public void ObservationCopiesTheActualFirstSamplerInputWithoutAddingForwards(int budget)
    {
        var tokenizer = new BpeTokenizer(); tokenizer.Train("\0", 256);
        var model = new LlmModel(new ModelConfig { VocabSize = 256, ContextLen = 8, HiddenDim = 8,
            NumLayers = 1, NumHeads = 2, NumKvHeads = 1, FfnHiddenDim = 16 });
        foreach (var parameter in model.BaseParameters) Array.Clear(parameter.Value.Data);
        var caches = new List<CountingCache>();
        int samplerCalls = 0;
        var generator = new TextGenerator(model, tokenizer,
            layers => { var cache = new CountingCache(layers); caches.Add(cache); return cache; },
            (logits, random, temperature, topK, topP) =>
            {
                samplerCalls++;
                int selected = Sampler.Sample(logits, random, temperature, topK, topP);
                // Симулируем sampler, использующий свой вход как workspace: снимок должен пережить это.
                Array.Fill(logits.Data, 123f);
                return selected;
            });

        var ordinary = generator.GenerateDetailed("\0\0", budget, 0, 0, 1, 17, null, CancellationToken.None);
        var observed = generator.GenerateWithFirstDecision("\0\0", budget, 0, 0, 1, 17, null, CancellationToken.None);

        Assert.Equal(ordinary.Text, observed.Receipt.Text);
        Assert.Equal(ordinary.PromptTokenCount, observed.Receipt.PromptTokenCount);
        Assert.Equal(ordinary.GeneratedTokenCount, observed.Receipt.GeneratedTokenCount);
        Assert.Equal(ordinary.StopReason, observed.Receipt.StopReason);
        Assert.Equal(budget * 2, samplerCalls);
        Assert.Equal(2, caches.Count);
        Assert.All(caches, cache => Assert.Equal(new[] { 2 }.Concat(Enumerable.Repeat(1, budget - 1)), cache.QueryLengths));
        Assert.All(caches, cache => Assert.Throws<ObjectDisposedException>(() => cache.GetUniformSequenceLength()));
        Assert.Equal(1, observed.PrefillForwardCalls);
        Assert.Equal(budget - 1, observed.DecodeForwardCalls);
        Assert.Equal(0, observed.Decision.SelectedTokenId); // First-index tie behavior of the real greedy sampler.
        Assert.Equal([0, 0], observed.Decision.PromptTokenIds);
        Assert.Equal(256, observed.Decision.Logits.Count);
        Assert.All(observed.Decision.Logits, value => Assert.Equal(0f, value));
        Assert.Throws<NotSupportedException>(() => ((IList<float>)observed.Decision.Logits)[0] = 99);
        Assert.Throws<NotSupportedException>(() => ((IList<int>)observed.Decision.PromptTokenIds)[0] = 99);
    }

    [Fact]
    public void CancelledObservationDoesNotAllocateCacheOrSample()
    {
        var tokenizer = new BpeTokenizer(); tokenizer.Train("\0", 256);
        var model = new LlmModel(new ModelConfig { VocabSize = 256, ContextLen = 8, HiddenDim = 8,
            NumLayers = 1, NumHeads = 2, NumKvHeads = 1, FfnHiddenDim = 16 });
        var generator = new TextGenerator(model, tokenizer, _ => throw new Exception("Unexpected cache allocation"),
            (_, _, _, _, _) => throw new Exception("Unexpected sampler call"));
        using var cancelled = new CancellationTokenSource(); cancelled.Cancel();
        Assert.Throws<OperationCanceledException>(() => generator.GenerateWithFirstDecision("\0", 1, 0, 0, 1, 17, null, cancelled.Token));
    }

    private sealed class CountingCache(int layers) : KvCache(layers), ITransformerKvCache
    {
        public List<int> QueryLengths { get; } = [];
        (Tensor keys, Tensor values) ITransformerKvCache.Update(int layer, Tensor newK, Tensor newV)
        {
            QueryLengths.Add(newK.Shape.Dimensions[2]);
            return base.Update(layer, newK, newV);
        }
    }
}
