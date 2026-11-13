// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using TensorCore.Core;
using Inference;

namespace LlmFromScratch.Tests;

// Unit tests for Sampler — greedy, temperature, top-k, top-p.
// Behavioral assertion gate: конкретные token IDs проверяются, не просто "вернул int".
public class SamplerTests
{
    [Fact]
    public void Greedy_Temperature0_ReturnsArgmax()
    {
        // Логиты [0, 0, 10, 0, 0] — argmax на индексе 2
        var logits = new Tensor(new float[] { 0f, 0f, 10f, 0f, 0f }, new TensorShape(5));

        int token = Sampler.Sample(logits, temperature: 0f);

        Assert.Equal(2, token);
    }

    [Fact]
    public void Greedy_AllEqual_ReturnsSomeToken()
    {
        var logits = new Tensor(new float[10], new TensorShape(10)); // все нули

        int token = Sampler.Sample(logits, temperature: 0f);

        Assert.InRange(token, 0, 9);
    }

    [Fact]
    public void TopK1_TemperaturePositive_AlwaysReturnsArgmax()
    {
        // При topK=1 выживает только argmax → результат детерминирован
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

        // topP=1.0 означает все токены допустимы
        int token = Sampler.Sample(logits, temperature: 1.0f, topP: 1.0f);

        Assert.InRange(token, 0, 4);
    }

    [Fact]
    public void TopP_VerySmall_ReturnsMostLikelyToken()
    {
        // Логит 100 у индекса 4 делает все остальные вероятности ≈ 0
        var logits = new Tensor(new float[] { 0f, 0f, 0f, 0f, 100f }, new TensorShape(5));

        int token = Sampler.Sample(logits, temperature: 1.0f, topP: 0.01f);

        Assert.Equal(4, token);
    }

    [Fact]
    public void Sample_SeededRandom_ReturnsExactMultiDrawSequence()
    {
        var logits = new Tensor(new float[4], new TensorShape(4));
        var random = new SequenceRandom([0.10, 0.40, 0.60, 0.90]);

        var tokens = Enumerable.Range(0, 4)
            .Select(_ => Sampler.Sample(logits, random, temperature: 1f, topK: 0, topP: 1f))
            .ToArray();

        Assert.Equal([0, 1, 2, 3], tokens);
        Assert.Equal(4, random.NextDoubleCallCount);
    }

    [Fact]
    public void Sample_SeededRandom_IsolatedFromInterleavedUnseededSampling()
    {
        var logits = new Tensor(new float[4], new TensorShape(4));
        var expectedRandom = new CountingSeededRandom(1701);
        var actualRandom = new CountingSeededRandom(1701);
        int[] expected = SampleSequence(logits, expectedRandom);

        var tokens = new List<int>();
        for (int index = 0; index < 8; index++)
        {
            tokens.Add(Sampler.Sample(logits, actualRandom, temperature: 1f, topK: 0, topP: 1f));
            _ = Sampler.Sample(logits, temperature: 1f, topK: 0, topP: 1f);
        }

        Assert.Equal(expected, tokens);
        Assert.Equal(8, expectedRandom.NextDoubleCallCount);
        Assert.Equal(8, actualRandom.NextDoubleCallCount);
    }

    [Fact]
    public void Sample_Greedy_DoesNotConsumeTheProvidedRandom()
    {
        var logits = new Tensor(new float[] { 0f, 4f, 2f }, new TensorShape(3));
        var random = new ThrowingRandom();

        int token = Sampler.Sample(logits, random, temperature: 0f, topK: 0, topP: 1f);

        Assert.Equal(1, token);
    }

    [Fact]
    public void Sample_InternalRandomPath_RejectsNullRandom()
    {
        var logits = new Tensor(new float[4], new TensorShape(4));

        Assert.Throws<ArgumentNullException>(() => Sampler.Sample(
            logits,
            null!,
            temperature: 1f,
            topK: 0,
            topP: 1f));
    }

    // Главный контроль границы: числа берутся из сохранённого показания настоящего обучения,
    // а не переписываются в код теста. Нулевая случайная величина — нижний край промежутка.
    [Fact]
    public void ZeroDraw_LearnedTopOneDistribution_ReturnsTheOnlyTokenWithPositiveProbability()
    {
        var logits = LoadLearnedRowLogits("T1");
        var random = new SequenceRandom([0d]);

        int token = Sampler.Sample(logits, random, temperature: 1f, topK: 1, topP: 1f);

        // Сохранённая строка содержит ровно 256 чисел: это доказательство, что вектор прочитан из файла.
        Assert.Equal(256, logits.Shape.TotalElements);
        // При верхнем-K, равном единице, допустим единственный токен «плюс» с номером 43.
        // Прежнее нестрогое сравнение возвращало здесь нулевой токен с нулевой вероятностью.
        Assert.Equal(43, token);
        // Случайная величина прочитана ровно один раз: лишних обращений к датчику нет.
        Assert.Equal(1, random.NextDoubleCallCount);
    }

    // Поясняющий оракул: он объясняет логику на трёх числах, но не заменяет учебный источник выше.
    [Fact]
    public void ZeroDraw_SimpleOracle_ReturnsTheOnlyTokenWithPositiveProbability()
    {
        var logits = new Tensor(new float[] { 0f, 3f, 1f }, new TensorShape(3));
        var random = new SequenceRandom([0d]);

        int token = Sampler.Sample(logits, random, temperature: 1f, topK: 1, topP: 1f);

        // Наибольшее число стоит на позиции 1, поэтому после отбора по верхнему-K только оно имеет вес.
        Assert.Equal(1, token);
    }

    // Стык двух положительных промежутков: ровная половина уже принадлежит второму токену,
    // а чуть меньшая величина — ещё первому. Запасной возврат последнего индекса здесь не участвует.
    [Theory]
    [InlineData(0.5d, 1)]
    [InlineData(0.49999999d, 0)]
    public void DrawAtTheBoundaryBetweenTwoPositiveIntervals_SelectsTheIntervalThatContainsIt(
        double draw,
        int expectedToken)
    {
        // Два одинаковых числа дают после нормировки ровно по половине вероятности на токен.
        var logits = new Tensor(new float[] { 0f, 0f }, new TensorShape(2));
        var random = new SequenceRandom([draw]);

        int token = Sampler.Sample(logits, random, temperature: 1f, topK: 0, topP: 1f);

        Assert.Equal(expectedToken, token);
    }

    // Читает сохранённые логиты одной строки опыта последовательной фильтрации прямо с диска.
    private static Tensor LoadLearnedRowLogits(string rowIdentifier)
    {
        // Путь к сохранённому показанию опыта: состояние после четырёх настоящих уроков.
        string savedReadoutPath = Path.Combine(
            FindRepositoryRoot(),
            "experiments-v2",
            "RP01-MICRO",
            "P0801",
            "results",
            "attempt-0001",
            "after.json");
        // Разбор сохранённого файла; при его отсутствии тест обязан упасть, а не подставить числа.
        using var savedReadout = JsonDocument.Parse(File.ReadAllText(savedReadoutPath));
        // Нужная строка выбирается по её обозначению, а не по порядковому номеру в файле.
        var row = savedReadout.RootElement
            .GetProperty("rows")
            .EnumerateArray()
            .Single(candidate => candidate.GetProperty("id").GetString() == rowIdentifier);
        // Сами числа переносятся в вектор без округления и без пересчёта.
        float[] savedLogits = row.GetProperty("logits")
            .EnumerateArray()
            .Select(value => value.GetSingle())
            .ToArray();
        return new Tensor(savedLogits, new TensorShape(savedLogits.Length));
    }

    // Поднимается от папки сборки до корня хранилища, чтобы путь к сохранённому файлу не зависел от запуска.
    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln")))
                return current.FullName;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("SAMPLER_TESTS_REPOSITORY_ROOT_NOT_FOUND");
    }

    private sealed class CountingSeededRandom : Random
    {
        public CountingSeededRandom(int seed)
            : base(seed)
        {
        }

        public int NextDoubleCallCount { get; private set; }

        public override double NextDouble()
        {
            NextDoubleCallCount++;
            return base.NextDouble();
        }
    }

    private sealed class ThrowingRandom : Random
    {
        public override double NextDouble()
        {
            throw new InvalidOperationException("SAMPLER_GREEDY_SHOULD_NOT_USE_RANDOM");
        }
    }

    private static int[] SampleSequence(Tensor logits, Random random) => Enumerable.Range(0, 8)
        .Select(_ => Sampler.Sample(logits, random, temperature: 1f, topK: 0, topP: 1f))
        .ToArray();

    private sealed class SequenceRandom : Random
    {
        private readonly Queue<double> _values;

        public SequenceRandom(IEnumerable<double> values)
        {
            _values = new Queue<double>(values);
        }

        public int NextDoubleCallCount { get; private set; }

        public override double NextDouble()
        {
            NextDoubleCallCount++;
            return _values.Dequeue();
        }
    }
}
