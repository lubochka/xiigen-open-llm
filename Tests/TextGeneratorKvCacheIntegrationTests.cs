// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using Inference;
using TensorCore.Core;
using Tokenizer;
using TransformerModel;
using TransformerModel.Caching;
using TransformerModel.Config;

namespace LlmFromScratch.Tests;

public sealed class TextGeneratorKvCacheIntegrationTests
{
    [Fact]
    public void Generate_PrefillsThenDecodesWithOneRequestLocalCache()
    {
        var tokenizer = TinyTokenizer();
        var caches = new List<TrackingKvCache>();
        var generator = CreateGenerator(
            CreateTokenZeroModel(),
            tokenizer,
            layers => AddCache(caches, layers));

        var result = generator.Generate("\0\0", maxNewTokens: 3, temperature: 0f);

        var cache = Assert.Single(caches);
        Assert.Equal(tokenizer.Decode([0, 0, 0]), result);
        Assert.Equal([0, 1, 0, 1, 0, 1], cache.UpdatedLayers);
        Assert.Equal([(2, 2), (2, 2), (1, 1), (1, 1), (1, 1), (1, 1)], cache.UpdateDeltas);
        AssertDisposed(cache);
    }

    [Fact]
    public void Generate_UsesDistinctDisposedCachesForSeparateRequests()
    {
        var tokenizer = TinyTokenizer();
        var caches = new List<TrackingKvCache>();
        var generator = CreateGenerator(
            CreateTokenZeroModel(),
            tokenizer,
            layers => AddCache(caches, layers));

        var first = generator.Generate("\0\0", maxNewTokens: 3, temperature: 0f);
        var second = generator.Generate("\0\0", maxNewTokens: 3, temperature: 0f);

        Assert.Equal(tokenizer.Decode([0, 0, 0]), first);
        Assert.Equal(first, second);
        Assert.Equal(2, caches.Count);
        Assert.NotSame(caches[0], caches[1]);
        Assert.Equal([(2, 2), (2, 2)], caches[0].UpdateDeltas.Take(2));
        Assert.Equal([(2, 2), (2, 2)], caches[1].UpdateDeltas.Take(2));
        AssertDisposed(caches[0]);
        AssertDisposed(caches[1]);
    }

    [Fact]
    public void Generate_DecodesSampledEosButDoesNotFeedItBack()
    {
        var tokenizer = TinyTokenizer();
        var caches = new List<TrackingKvCache>();
        var generator = CreateGenerator(
            CreateEosModel(),
            tokenizer,
            layers => AddCache(caches, layers));

        var result = generator.Generate("\0\0", maxNewTokens: 2, temperature: 0f);

        var cache = Assert.Single(caches);
        Assert.Equal(tokenizer.Decode([255]), result);
        Assert.Equal([0, 1], cache.UpdatedLayers);
        Assert.Equal([(2, 2), (2, 2)], cache.UpdateDeltas);
        AssertDisposed(cache);
    }

    [Fact]
    public void Generate_EnforcesPromptAndGenerationContextBoundariesBeforeCacheCreation()
    {
        var tokenizer = TinyTokenizer();
        var caches = new List<TrackingKvCache>();
        var generator = CreateGenerator(
            CreateTokenZeroModel(contextLength: 2),
            tokenizer,
            layers => AddCache(caches, layers));

        _ = generator.Generate("\0\0", maxNewTokens: 1, temperature: 0f);
        Assert.Single(caches);

        AssertStableCode(
            "TEXT_GENERATOR_CONTEXT_BUDGET_EXCEEDED",
            () => generator.Generate("\0\0", maxNewTokens: 2, temperature: 0f));
        Assert.Single(caches);

        AssertStableCode(
            "TEXT_GENERATOR_PROMPT_CONTEXT_LENGTH_EXCEEDED",
            () => generator.Generate("\0\0\0", maxNewTokens: 1, temperature: 0f));
        Assert.Single(caches);

        _ = generator.Generate("\0", maxNewTokens: 2, temperature: 0f);
        Assert.Equal(2, caches.Count);

        AssertStableCode(
            "TEXT_GENERATOR_CONTEXT_BUDGET_EXCEEDED",
            () => generator.Generate("\0", maxNewTokens: 3, temperature: 0f));
        Assert.Equal(2, caches.Count);
        Assert.All(caches, AssertDisposed);
    }

    [Fact]
    public void Generate_ValidatesBeforeCreatingACacheAndMaxZeroDoesNotCallTheModel()
    {
        var tokenizer = TinyTokenizer();
        var caches = new List<TrackingKvCache>();
        var generator = CreateGenerator(
            CreateTokenZeroModel(contextLength: 1),
            tokenizer,
            layers => AddCache(caches, layers));

        AssertStableCode(
            "TEXT_GENERATOR_PROMPT_EMPTY",
            () => generator.Generate(string.Empty, maxNewTokens: 1, temperature: 0f));
        AssertStableCode(
            "TEXT_GENERATOR_MAX_NEW_TOKENS_NEGATIVE",
            () => generator.Generate("\0", maxNewTokens: -1, temperature: 0f));
        AssertStableCode(
            "TEXT_GENERATOR_PROMPT_CONTEXT_LENGTH_EXCEEDED",
            () => generator.Generate("\0\0", maxNewTokens: 0, temperature: 0f));
        Assert.Equal(string.Empty, generator.Generate("\0", maxNewTokens: 0, temperature: 0f));

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => generator.Generate(
            null!,
            maxNewTokens: 1,
            temperature: 0f,
            topK: 0,
            topP: 1f,
            cancellationToken: cancellation.Token));

        Assert.Empty(caches);
    }

    [Fact]
    public void Generate_PreservesTheLegacyOverloadAndDefaultLiteralMeansZeroNewTokens()
    {
        var tokenizer = TinyTokenizer();
        var caches = new List<TrackingKvCache>();
        var generator = CreateGenerator(
            CreateTokenZeroModel(),
            tokenizer,
            layers => AddCache(caches, layers));

        var legacy = generator.Generate("\0", 1, 0f, 0, 1f);
        var defaultLiteral = generator.Generate("\0", default);

        Assert.Equal(tokenizer.Decode([0]), legacy);
        Assert.Equal(string.Empty, defaultLiteral);
        Assert.Single(caches);
        AssertDisposed(caches[0]);
    }

    [Fact]
    public void Generate_PropagatesCancellationToCachedModelAndDisposesClearedCache()
    {
        var tokenizer = TinyTokenizer();
        using var cancellation = new CancellationTokenSource();
        TrackingKvCache? captured = null;
        var generator = CreateGenerator(
            CreateTokenZeroModel(),
            tokenizer,
            layers => captured = new TrackingKvCache(
                layers,
                cancellation,
                cancelAfterUpdate: 1));

        Assert.Throws<OperationCanceledException>(() => generator.Generate(
            "\0\0",
            maxNewTokens: 3,
            temperature: 0f,
            topK: 0,
            topP: 1f,
            cancellationToken: cancellation.Token));

        var cache = Assert.IsType<TrackingKvCache>(captured);
        Assert.Equal(1, cache.ExplicitClearCount);
        Assert.Equal([0, 0], cache.SequenceLengthsAtExplicitClear);
        AssertDisposed(cache);
    }

    [Fact]
    public void ConstructorAndFactoryGuardsFailDeterministically()
    {
        var tokenizer = TinyTokenizer();
        var model = CreateTokenZeroModel();

        Assert.Throws<ArgumentNullException>(() => new TextGenerator(null!, tokenizer));
        Assert.Throws<ArgumentNullException>(() => new TextGenerator(model, null!));
        Assert.Throws<ArgumentNullException>(() => new TextGenerator(model, tokenizer, null!));
        var generator = new TextGenerator(model, tokenizer, _ => null!);

        AssertStableCode(
            "TEXT_GENERATOR_CACHE_FACTORY_RETURNED_NULL",
            () => generator.Generate("\0", maxNewTokens: 1, temperature: 0f));
    }

    private static TextGenerator CreateGenerator(
        LlmModel model,
        BpeTokenizer tokenizer,
        Func<int, KvCache> cacheFactory) => new(model, tokenizer, cacheFactory);

    private static TrackingKvCache AddCache(List<TrackingKvCache> caches, int layers)
    {
        var cache = new TrackingKvCache(layers);
        caches.Add(cache);
        return cache;
    }

    private static BpeTokenizer TinyTokenizer()
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train("\0", vocabSize: 256);
        Assert.Equal([0, 0], tokenizer.Encode("\0\0"));
        // FIX-EOS-MARKER-01: прежде здесь закреплялось, что признак конца равен
        // 255 — последней записи словаря на 256 токенов. Теперь словарь прямо
        // говорит, что признака конца НЕ объявляет: последняя запись — обычный
        // байтовый токен, а не служебный признак.
        Assert.Null(tokenizer.EosToken);
        return tokenizer;
    }

    private static LlmModel CreateTokenZeroModel(int contextLength = 8)
    {
        var model = new LlmModel(TinyConfig(contextLength));
        foreach (var parameter in model.BaseParameters)
            Array.Clear(parameter.Value.Data);
        return model;
    }

    private static LlmModel CreateEosModel()
    {
        var model = new LlmModel(TinyConfig(contextLength: 8));
        foreach (var parameter in model.BaseParameters)
            Array.Clear(parameter.Value.Data);

        var config = model.Config;
        foreach (var parameter in model.BaseParameters.Where(parameter =>
                     parameter.Value.Shape.Rank == 1
                     && parameter.Value.Shape.Dimensions[0] == config.HiddenDim))
        {
            Array.Fill(parameter.Value.Data, 1f);
        }

        var vocabHiddenMatrices = model.BaseParameters
            .Where(parameter => parameter.Value.Shape.Rank == 2
                && parameter.Value.Shape.Dimensions[0] == config.VocabSize
                && parameter.Value.Shape.Dimensions[1] == config.HiddenDim)
            .ToList();
        Assert.Equal(2, vocabHiddenMatrices.Count);
        foreach (var parameter in vocabHiddenMatrices)
        {
            Array.Fill(parameter.Value.Data, 1f, 0, config.HiddenDim);
            Array.Fill(parameter.Value.Data, 2f, 255 * config.HiddenDim, config.HiddenDim);
        }

        return model;
    }

    private static ModelConfig TinyConfig(int contextLength) => new()
    {
        VocabSize = 256,
        ContextLen = contextLength,
        HiddenDim = 8,
        NumLayers = 2,
        NumHeads = 4,
        NumKvHeads = 2,
        FfnHiddenDim = 16
    };

    private static void AssertStableCode(string expectedCode, Action action)
    {
        var exception = Assert.ThrowsAny<Exception>(action);
        Assert.Equal(expectedCode, exception.Message);
    }

    private static void AssertDisposed(TrackingKvCache cache)
    {
        Assert.Throws<ObjectDisposedException>(() => cache.GetUniformSequenceLength());
    }

    private sealed class TrackingKvCache : KvCache, ITransformerKvCache
    {
        private readonly CancellationTokenSource? _cancellation;
        private readonly int? _cancelAfterUpdate;

        public TrackingKvCache(
            int numLayers,
            CancellationTokenSource? cancellation = null,
            int? cancelAfterUpdate = null)
            : base(numLayers)
        {
            _cancellation = cancellation;
            _cancelAfterUpdate = cancelAfterUpdate;
        }

        public List<int> UpdatedLayers { get; } = [];
        public List<(int KeyDelta, int ValueDelta)> UpdateDeltas { get; } = [];
        public int ExplicitClearCount { get; private set; }
        public int[] SequenceLengthsAtExplicitClear { get; private set; } = [];

        int ITransformerKvCache.LayerCount => base.LayerCount;

        int ITransformerKvCache.GetSequenceLength(int layer) => base.GetSequenceLength(layer);

        int ITransformerKvCache.GetUniformSequenceLength() => base.GetUniformSequenceLength();

        (Tensor keys, Tensor values) ITransformerKvCache.Update(
            int layer,
            Tensor newK,
            Tensor newV)
        {
            UpdatedLayers.Add(layer);
            UpdateDeltas.Add((newK.Shape.Dimensions[2], newV.Shape.Dimensions[2]));
            var result = base.Update(layer, newK, newV);
            if (_cancelAfterUpdate == UpdateDeltas.Count)
                _cancellation!.Cancel();
            return result;
        }

        void ITransformerKvCache.Clear()
        {
            ExplicitClearCount++;
            base.Clear();
            SequenceLengthsAtExplicitClear = Enumerable.Range(0, base.LayerCount)
                .Select(base.GetSequenceLength)
                .ToArray();
        }
    }
}
