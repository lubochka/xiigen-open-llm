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

public sealed class TextGeneratorSeededSamplingTests
{
    private const int PrimarySeed = 1701;
    private const int SecondarySeed = 481;
    private const int SampleCount = 8;

    [Fact]
    public void Generate_SeededOverload_ReplaysTheExactMultiDrawTokenSequence()
    {
        var tokenizer = TinyTokenizer();
        var generator = CreateUniformGenerator(tokenizer);
        int[] expectedTokens = ExpectedUniformTokens(PrimarySeed);

        string result = GenerateSeeded(generator, PrimarySeed);

        Assert.Equal(tokenizer.Decode(expectedTokens.ToList()), result);
        Assert.NotEqual(tokenizer.Decode(Enumerable.Repeat(expectedTokens[0], SampleCount).ToList()), result);
    }

    [Fact]
    public void Generate_SeededOverload_UsesTheSuppliedSecondSeedForExactReplay()
    {
        var tokenizer = TinyTokenizer();
        var generator = CreateUniformGenerator(tokenizer);
        int[] expectedTokens = ExpectedUniformTokens(SecondarySeed);

        string result = GenerateSeeded(generator, SecondarySeed);

        Assert.Equal(tokenizer.Decode(expectedTokens.ToList()), result);
    }

    [Fact]
    public void Generate_SeededOverload_PassesOneSeededRandomInstanceAcrossTheDecode()
    {
        var tokenizer = TinyTokenizer();
        var samples = new List<Random>();
        var draws = new List<double>();
        var generator = CreateTrackingSamplerGenerator(tokenizer, samples, draws);
        var expectedRandom = new Random(PrimarySeed);
        double[] expectedDraws = Enumerable.Range(0, SampleCount)
            .Select(_ => expectedRandom.NextDouble())
            .ToArray();

        _ = GenerateSeeded(generator, PrimarySeed);

        Assert.Equal(expectedDraws, draws);
        Assert.Equal(SampleCount, samples.Count);
        Assert.All(samples, sample => Assert.Same(samples[0], sample));
    }

    [Fact]
    public void Generate_UnseededCancellationOverload_PassesRandomSharedAcrossTheDecode()
    {
        var tokenizer = TinyTokenizer();
        var samples = new List<Random>();
        var draws = new List<double>();
        var generator = CreateTrackingSamplerGenerator(tokenizer, samples, draws);

        _ = generator.Generate(
            "\0",
            SampleCount,
            1f,
            0,
            1f,
            CancellationToken.None);

        Assert.Equal(SampleCount, draws.Count);
        Assert.All(samples, sample => Assert.Same(Random.Shared, sample));
    }

    [Fact]
    public void Constructor_WithNullSampler_RejectsTheInvalidInternalSeam()
    {
        var tokenizer = TinyTokenizer();
        var model = CreateUniformModel(contextLength: 16);

        Assert.Throws<ArgumentNullException>(() => new TextGenerator(
            model,
            tokenizer,
            layers => new KvCache(layers),
            null!));
    }

    [Fact]
    public void Generate_SeededReplay_IsolatedFromUnrelatedGlobalSampling()
    {
        var tokenizer = TinyTokenizer();
        var generator = CreateUniformGenerator(tokenizer);
        var logits = new Tensor(new float[4], new TensorShape(4));

        string first = GenerateSeeded(generator, PrimarySeed);
        for (int index = 0; index < 32; index++)
            _ = Sampler.Sample(logits, temperature: 1f, topK: 0, topP: 1f);
        string replay = GenerateSeeded(generator, PrimarySeed);

        Assert.Equal(tokenizer.Decode(ExpectedUniformTokens(PrimarySeed).ToList()), first);
        Assert.Equal(first, replay);
    }

    [Fact]
    public void Generate_LegacyOverloads_RemainCallableAndReturnTheExistingMaxZeroResult()
    {
        var tokenizer = TinyTokenizer();
        var generator = CreateUniformGenerator(tokenizer);

        string defaultArguments = generator.Generate("\0", maxNewTokens: 0);
        string fiveArguments = generator.Generate("\0", 0, 1f, 0, 1f);
        string cancellationTokenArgument = generator.Generate(
            "\0",
            0,
            1f,
            0,
            1f,
            CancellationToken.None);

        Assert.Equal(string.Empty, defaultArguments);
        Assert.Equal(defaultArguments, fiveArguments);
        Assert.Equal(defaultArguments, cancellationTokenArgument);
    }

    [Fact]
    public async Task Generate_SeededParallelRequests_AreRequestIsolated()
    {
        var tokenizer = TinyTokenizer();
        var generator = CreateUniformGenerator(tokenizer);
        string expected = tokenizer.Decode(ExpectedUniformTokens(PrimarySeed).ToList());

        var requests = Enumerable.Range(0, 8)
            .Select(_ => Task.Run(() => GenerateSeeded(generator, PrimarySeed)))
            .ToArray();

        string[] results = await Task.WhenAll(requests);

        Assert.All(results, result => Assert.Equal(expected, result));
    }

    [Fact]
    public void Generate_SeededOverload_PreservesPreflightAndCancellationBeforeCacheCreation()
    {
        var tokenizer = TinyTokenizer();
        int cacheFactoryCalls = 0;
        var generator = new TextGenerator(
            CreateUniformModel(contextLength: 1),
            tokenizer,
            layers =>
            {
                cacheFactoryCalls++;
                return new KvCache(layers);
            });

        AssertStableCode(
            "TEXT_GENERATOR_CONTEXT_BUDGET_EXCEEDED",
            () => generator.Generate("\0", 2, 1f, 0, 1f, PrimarySeed, CancellationToken.None));
        AssertStableCode(
            "TEXT_GENERATOR_PROMPT_CONTEXT_LENGTH_EXCEEDED",
            () => generator.Generate("\0\0", 0, 1f, 0, 1f, PrimarySeed, CancellationToken.None));
        Assert.Equal(string.Empty, generator.Generate("\0", 0, 1f, 0, 1f, PrimarySeed, CancellationToken.None));

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => generator.Generate(
            "\0",
            1,
            1f,
            0,
            1f,
            PrimarySeed,
            cancellation.Token));

        Assert.Equal(0, cacheFactoryCalls);
    }

    private static string GenerateSeeded(TextGenerator generator, int seed) => generator.Generate(
        "\0",
        maxNewTokens: SampleCount,
        temperature: 1f,
        topK: 0,
        topP: 1f,
        seed: seed,
        cancellationToken: CancellationToken.None);

    private static TextGenerator CreateUniformGenerator(BpeTokenizer tokenizer) => new(
        CreateUniformModel(contextLength: 16),
        tokenizer);

    private static TextGenerator CreateTrackingSamplerGenerator(
        BpeTokenizer tokenizer,
        List<Random> samples,
        List<double> draws) => new(
        CreateUniformModel(contextLength: 16),
        tokenizer,
        layers => new KvCache(layers),
        (logits, random, temperature, topK, topP) =>
        {
            samples.Add(random);
            draws.Add(random.NextDouble());
            return 0;
        });

    private static BpeTokenizer TinyTokenizer()
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train("\0", vocabSize: 256);
        return tokenizer;
    }

    private static LlmModel CreateUniformModel(int contextLength)
    {
        var model = new LlmModel(new ModelConfig
        {
            VocabSize = 4,
            ContextLen = contextLength,
            HiddenDim = 8,
            NumLayers = 2,
            NumHeads = 4,
            NumKvHeads = 2,
            FfnHiddenDim = 16
        });
        foreach (var parameter in model.BaseParameters)
            Array.Clear(parameter.Value.Data);
        return model;
    }

    private static void AssertStableCode(string expectedCode, Action action)
    {
        var exception = Assert.ThrowsAny<Exception>(action);
        Assert.Equal(expectedCode, exception.Message);
    }

    private static int[] ExpectedUniformTokens(int seed)
    {
        var random = new Random(seed);
        return Enumerable.Range(0, SampleCount)
            .Select(_ => UniformToken(random.NextDouble()))
            .ToArray();
    }

    private static int UniformToken(double randomValue)
    {
        double accumulatedProbability = 0d;
        for (int token = 0; token < 4; token++)
        {
            accumulatedProbability += 0.25d;
            if (randomValue <= accumulatedProbability)
                return token;
        }

        return 3;
    }
}
