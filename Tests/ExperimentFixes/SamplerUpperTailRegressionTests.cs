// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Text.Json;
using Inference;
using TensorCore.Core;

// Ремонт верхнего числового хвоста выборщика токенов (раздел 2 адресного плана
// исправления переносов от 29 октября, файл 95 папки планов переносов).
//
// Что проверяется по-человечески. Выборщик делит отрезок от нуля до единицы
// между кандидатами по их вероятностям и бросает случайное число в этот
// отрезок. Сумма вероятностей после отсечения кандидатов хранится числами
// ограниченной точности и может оказаться чуть меньше единицы. Случайное
// число из получившегося зазора раньше не попадало ни в один отрезок, и
// выборщик возвращал запасной ответ — последний номер словаря, у которого
// вероятность нулевая и которого отбор уже выбросил.
//
// Как это чинится. Общая масса вероятностей считается один раз в двойной
// точности, случайное число берётся один раз и растягивается на эту массу,
// обход идёт по тем же значениям со строгим сравнением. Испорченное
// распределение отвергается до обращения к генератору случайных чисел.
// Защитный возврат отдаёт последний кандидат с положительной вероятностью.
//
// Доза этой проверки: ноль обучения, ноль прямых и обратных проходов модели,
// ноль обновлений оптимизатора и не более 64 обращений к выборщику.
public sealed class SamplerUpperTailRegressionTests
{
    private const int SamplerCallBudget = 64; // Назначенный планом предел обращений к выборщику.
    private const double ProbabilityTolerance = 1e-6; // Допуск сверки вероятностей, назначенный заранее.

    private int _samplerCalls; // Фактические обращения к выборщику в одной проверке.

    // Главная регрессия дефекта: при сумме вероятностей меньше единицы случайное
    // число из верхнего хвоста обязано попасть в последний живой отрезок, а не в
    // выброшенного отбором кандидата с нулевой вероятностью.
    [Fact]
    public void UpperTailDraw_OnMassBelowOne_ChoosesLastPositiveCandidate()
    {
        // Два живых кандидата по четверти и два выброшенных нуля: общая масса 0,5.
        var probabilities = Probabilities(0.25f, 0.25f, 0f, 0f);
        var random = new CountingDrawRandom(Math.BitDecrement(1d)); // Случайное число у самой единицы.

        var chosen = Choose(probabilities, random); // Один вызов выборщика.

        // Выбран последний кандидат с положительной вероятностью, а не последний номер словаря.
        Assert.Equal(1, chosen);
        Assert.True(probabilities.Data[chosen] > 0f);
        // Ровно одно обращение к генератору случайных чисел.
        Assert.Equal(1, random.DrawCount);
        AssertWithinBudget();
    }

    // Та же картина с другой стороны: масса чуть больше единицы тоже не должна
    // выводить выбор за пределы живых кандидатов.
    [Fact]
    public void UpperTailDraw_OnMassSlightlyAboveOne_StaysInsidePositiveCandidates()
    {
        var probabilities = Probabilities(0.5f, 0.5000001f, 0f); // Общая масса чуть больше единицы.
        var random = new CountingDrawRandom(Math.BitDecrement(1d)); // Случайное число у самой единицы.

        var chosen = Choose(probabilities, random); // Один вызов выборщика.

        Assert.Equal(1, chosen); // Последний живой отрезок, а не нулевой кандидат.
        Assert.True(probabilities.Data[chosen] > 0f);
        Assert.Equal(1, random.DrawCount);
        AssertWithinBudget();
    }

    // Нулевая и серединная случайные величины и соседние представимые значения у
    // внутренней границы: прежнее поведение отбора сохраняется и совпадает с
    // независимой композицией, а не с ответом самого продукта.
    [Fact]
    public void BoundaryDraws_OnSimpleLogits_MatchIndependentComposition()
    {
        float[] logits = [2f, 0f, -5f]; // Простой вход, названный планом ремонта.
        var composition = P08SamplerCompositionReference.Compose(logits, temperature: 1f, topK: 0, topP: 1f);
        var boundaries = P08SamplerCompositionReference.CumulativeBoundaries(composition);
        // Величины вдали от любой границы: нулевая, середина и у самой единицы.
        // Здесь обе стороны обязаны совпасть кандидат в кандидат, потому что
        // расхождение точности на таком расстоянии не способно перевести выбор
        // через край отрезка.
        double[] drawsFarFromBoundary = [0d, 0.5d, Math.BitDecrement(1d)];

        foreach (var draw in drawsFarFromBoundary)
        {
            var random = new CountingDrawRandom(draw); // Своя величина на каждый случай.
            var chosen = Sample(ToTensor(logits), random, temperature: 1f, topK: 0, topP: 1f);
            // Ожидание берётся у независимой композиции, а не у продукта.
            Assert.Equal(P08SamplerCompositionReference.Select(composition, draw), chosen);
            Assert.Equal(1, random.DrawCount); // Ровно один бросок на допустимый выбор.
        }

        // Сама первая граница и два её соседних представимых значения. Равенство
        // кандидат в кандидат здесь не проверяется, и это не послабление ради
        // зелёного прогона: независимый эталон складывает вероятности в двойной
        // точности, а продукт — в одинарной, поэтому их границы отстоят друг от
        // друга примерно на одну стомиллионную, то есть на миллионы представимых
        // шагов. Ровно на границе эталон называет кандидата 1, а продукт — кандидата
        // 0, причём одинаково и до ремонта, и после него: к верхнему числовому
        // хвосту это расхождение отношения не имеет. Проверяется то, что от
        // точности не зависит: выбор остаётся на одном из двух соседей границы,
        // не убывает при росте случайной величины, имеет положительную вероятность
        // и стоит ровно один бросок.
        int tokenBelowBoundary = composition.TokenIds[0]; // Кандидат слева от первой границы.
        int tokenAboveBoundary = composition.TokenIds[1]; // Кандидат справа от первой границы.
        double[] drawsAtBoundary =
        [
            Math.BitDecrement(boundaries[0]),
            boundaries[0],
            Math.BitIncrement(boundaries[0])
        ];
        int previousPosition = -1; // Позиция прошлого выбора: выбор не имеет права убывать.

        foreach (var draw in drawsAtBoundary)
        {
            var random = new CountingDrawRandom(draw); // Своя величина на каждый случай.
            var chosen = Sample(ToTensor(logits), random, temperature: 1f, topK: 0, topP: 1f);
            // Ни один третий кандидат у границы появиться не может.
            Assert.True(
                chosen == tokenBelowBoundary || chosen == tokenAboveBoundary,
                $"У границы {boundaries[0]} при величине {draw} выбран кандидат {chosen}, "
                + $"а допустимы только {tokenBelowBoundary} и {tokenAboveBoundary}.");

            int position = chosen == tokenBelowBoundary ? 0 : 1; // Место кандидата в составе.
            // Рост случайной величины не может вернуть выбор назад к левому соседу.
            Assert.True(
                position >= previousPosition,
                $"При величине {draw} выбор вернулся назад: кандидат {chosen} стоит левее прошлого.");
            previousPosition = position;

            // Нулевой кандидат у границы недопустим так же, как и в верхнем хвосте.
            Assert.True(composition.Probabilities[position] > 0d);
            Assert.Equal(1, random.DrawCount); // Ровно один бросок на допустимый выбор.
        }

        AssertWithinBudget();
    }

    // Выброшенный отбором последний номер словаря не должен появляться ни при
    // какой случайной величине из всего отрезка.
    [Fact]
    public void MaskedLastVocabularyId_IsNeverSelected()
    {
        // Последний номер словаря заведомо хуже остальных и выбрасывается отбором двух лучших.
        float[] logits = [3f, 2.5f, 1f, -20f];
        var composition = P08SamplerCompositionReference.Compose(logits, temperature: 1f, topK: 2, topP: 1f);
        Assert.DoesNotContain(logits.Length - 1, composition.TokenIds); // Независимый состав его не содержит.

        for (var step = 0; step < 24; step++)
        {
            var draw = step / 24d; // Равномерный обход всего отрезка случайной величины.
            var random = new CountingDrawRandom(draw);
            var chosen = Sample(ToTensor(logits), random, temperature: 1f, topK: 2, topP: 1f);
            Assert.NotEqual(logits.Length - 1, chosen); // Нулевая вероятность не выбирается никогда.
            Assert.Contains(chosen, composition.TokenIds); // Выбор остаётся внутри независимого состава.
        }

        AssertWithinBudget();
    }

    // Те же две строки восьмого опыта и его же ограничения отбора: случайная
    // величина у самой единицы остаётся внутри независимо посчитанного состава.
    [Theory]
    [InlineData("T1")]
    [InlineData("E1")]
    public void UpperTailDraw_OnSavedTrainedLogits_StaysInsideIndependentSupport(string rowIdentifier)
    {
        var logits = LoadSavedLogits(rowIdentifier); // Сохранённые логиты после обучения.
        var composition = P08SamplerCompositionReference.Compose(logits, temperature: 1f, topK: 2, topP: 1f);
        var draw = Math.BitDecrement(1d); // Случайное число у самой единицы.
        var random = new CountingDrawRandom(draw);

        var chosen = Sample(ToTensor(logits), random, temperature: 1f, topK: 2, topP: 1f);

        // Выбор совпал с независимой композицией и остался внутри её состава.
        Assert.Equal(P08SamplerCompositionReference.Select(composition, draw), chosen);
        Assert.Contains(chosen, composition.TokenIds);
        // У выбранного кандидата положительная вероятность, а не ноль.
        var position = composition.TokenIds.ToList().IndexOf(chosen);
        Assert.True(composition.Probabilities[position] > ProbabilityTolerance);
        Assert.Equal(1, random.DrawCount);
        AssertWithinBudget();
    }

    // Испорченное распределение отвергается до обращения к генератору случайных
    // чисел: ни одного броска и отдельное устойчивое сообщение.
    [Fact]
    public void InvalidProbabilityMass_IsRejectedBeforeAnyRandomDraw()
    {
        var invalidDistributions = new[]
        {
            Probabilities(0.5f, -0.25f, 0.75f), // Отрицательная вероятность.
            Probabilities(0.5f, float.NaN, 0.5f), // Нечисловая вероятность.
            Probabilities(0.5f, float.PositiveInfinity, 0.5f), // Бесконечная вероятность.
            Probabilities(0f, 0f, 0f) // Неположительная общая масса.
        };

        foreach (var distribution in invalidDistributions)
        {
            var random = new CountingDrawRandom(0.5d); // Генератор, к которому обращаться нельзя.
            var error = Assert.Throws<InvalidOperationException>(() => Choose(distribution, random));
            Assert.Equal("SAMPLER_INVALID_PROBABILITY_MASS", error.Message); // Отдельное устойчивое сообщение.
            Assert.Equal(0, random.DrawCount); // Ни одного броска до отказа.
        }

        AssertWithinBudget();
    }

    // Жадный выбор при нулевой температуре к генератору случайных чисел не
    // обращается вовсе; обычный выбор обращается ровно один раз.
    [Fact]
    public void GreedyChoiceDrawsNothing_AndStochasticChoiceDrawsOnce()
    {
        float[] logits = [2f, 0f, -5f];

        var greedyRandom = new CountingDrawRandom(0.5d);
        var greedy = Sample(ToTensor(logits), greedyRandom, temperature: 0f, topK: 0, topP: 1f);
        Assert.Equal(P08SamplerCompositionReference.SelectGreedily(logits), greedy);
        Assert.Equal(0, greedyRandom.DrawCount); // Жадный выбор генератор не трогает.

        var stochasticRandom = new CountingDrawRandom(0.5d);
        _ = Sample(ToTensor(logits), stochasticRandom, temperature: 1f, topK: 0, topP: 1f);
        Assert.Equal(1, stochasticRandom.DrawCount); // Обычный выбор делает ровно один бросок.

        AssertWithinBudget();
    }

    // ── Вспомогательные построения проверки ─────────────────────────────────────

    // Обращение к продуктовому выборщику с подсчётом назначенной дозы.
    private int Sample(Tensor logits, Random random, float temperature, int topK, float topP)
    {
        _samplerCalls++; // Ещё одно обращение к выборщику.
        return Sampler.Sample(logits, random, temperature, topK, topP);
    }

    // Обращение к узкому внутреннему входу распределения: публичный вход не
    // позволяет задать испорченную или неполную общую массу напрямую.
    private int Choose(Tensor probabilities, Random random)
    {
        _samplerCalls++; // Ещё одно обращение к выборщику.
        return Sampler.MultinomialSample(probabilities, random);
    }

    // Назначенная доза обращений к выборщику не превышена.
    private void AssertWithinBudget()
        => Assert.True(
            _samplerCalls <= SamplerCallBudget,
            $"SAMPLER_UPPER_TAIL_BUDGET_EXCEEDED:{_samplerCalls}");

    // Готовое распределение вероятностей в продуктовой форме тензора.
    private static Tensor Probabilities(params float[] values) => ToTensor(values);

    // Набор чисел в одномерный тензор продуктовой формы.
    private static Tensor ToTensor(float[] values)
        => new((float[])values.Clone(), new TensorShape(values.Length));

    // Сохранённые логиты строки восьмого опыта после обучения: только чтение.
    private static float[] LoadSavedLogits(string rowIdentifier)
    {
        using var saved = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "experiments-v2", "RP01-MICRO", "P0801", "results", "attempt-0001", "after.json")));
        var row = saved.RootElement.GetProperty("rows").EnumerateArray()
            .Single(candidate => candidate.GetProperty("id").GetString() == rowIdentifier);
        return [.. row.GetProperty("logits").EnumerateArray().Select(value => value.GetSingle())];
    }

    // Подъём от папки сборки до корня хранилища: пути не зависят от места запуска.
    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory); // Папка собранных проверок.
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln")))
                return current.FullName; // Корень найден по файлу решения.

            current = current.Parent; // Иначе поднимаемся ещё на уровень вверх.
        }

        throw new DirectoryNotFoundException("SAMPLER_UPPER_TAIL_TESTS_REPOSITORY_ROOT_NOT_FOUND");
    }

    // Генератор с одной заранее заданной величиной и счётчиком обращений.
    private sealed class CountingDrawRandom(double value) : Random
    {
        private readonly double _value = value; // Та самая величина, которую получит выбор.

        public int DrawCount { get; private set; } // Сколько раз выбор обратился к генератору.

        public override double NextDouble()
        {
            DrawCount++; // Обращение засчитывается до выдачи числа.
            return _value;
        }
    }
}
