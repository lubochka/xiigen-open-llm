// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Независимый эталон последовательных отсечений при выборе следующего знака (R28-S1).
//
// Зачем он нужен. Проверки восьмого опыта обязаны сравнивать продуктовый выбор не
// сам с собой, а с отдельно посчитанным ожиданием. Поэтому здесь нет ни одной
// ссылки на продуктовый выбиральщик и ни одного продуктового типа: ни Sampler,
// ни Tensor, ни TensorOps. Весь счёт идёт на простых массивах чисел двойной
// точности по описанию правил, а не по чужому коду.
//
// Что он считает. Для заданных логитов, температуры, числа кандидатов и суммы
// вероятностей он возвращает СОСТАВ кандидатов: какие номера знаков остались и с
// какой вероятностью. Отдельно он умеет выбрать знак по накопленной вероятности с
// той же строгой границей, что стоит в продукте.
//
// Чего он НЕ делает. Он не обращается к модели, не обучает, не читает файлы и
// ничего не измеряет по ресурсам.

namespace LlmFromScratch.Tests.ExperimentFixes;

/// <summary>Независимый счёт состава кандидатов: сначала число кандидатов, затем сумма вероятностей.</summary>
internal static class P08SamplerCompositionReference
{
    /// <summary>Состав кандидатов: номера оставшихся знаков и их вероятности в том же порядке.</summary>
    internal sealed record Composition(IReadOnlyList<int> TokenIds, IReadOnlyList<double> Probabilities);

    /// <summary>
    /// Считает состав кандидатов по описанию правил: деление на температуру, маска по числу
    /// кандидатов, отсечение по сумме вероятностей на уже замаскированном, мягкий максимум.
    /// </summary>
    internal static Composition Compose(IReadOnlyList<float> logits, float temperature, int topK, float topP)
    {
        // Без чисел считать нечего: пустой вход — это ошибка самого вызова проверки.
        ArgumentNullException.ThrowIfNull(logits);
        if (logits.Count == 0) throw new ArgumentException("P08_REFERENCE_EMPTY_LOGITS");
        // Нулевая и отрицательная температура — это жадная ветвь, у неё нет состава кандидатов.
        if (temperature <= 0f) throw new ArgumentException("P08_REFERENCE_GREEDY_HAS_NO_COMPOSITION");

        // Делим логиты на температуру ровно так же, как это делает продукт: умножением на
        // обратную величину и с округлением до одинарной точности на каждом шаге.
        float inverseTemperature = 1f / temperature;
        var scaled = new float[logits.Count];
        for (int index = 0; index < scaled.Length; index++)
            scaled[index] = logits[index] * inverseTemperature;

        // Первое правило: оставить только k лучших кандидатов, остальным поставить
        // минус бесконечность. При topK = 0 правило выключено и ничего не меняет.
        var masked = topK > 0 ? MaskByCandidateCount(scaled, topK) : (float[])scaled.Clone();

        // Второе правило: отсечение по сумме вероятностей. Оно работает по уже
        // замаскированным числам, поэтому сумма набирается по короткому списку.
        var kept = topP < 1.0f ? MaskByProbabilitySum(masked, topP) : masked;

        // Мягкий максимум превращает оставшиеся логиты в вероятности.
        var probabilities = SoftMaximum(kept);

        // В состав попадают только знаки с ненулевой вероятностью, по возрастанию номера:
        // именно в этом порядке продукт обходит кандидатов при случайном выборе.
        var tokenIds = new List<int>();
        var keptProbabilities = new List<double>();
        for (int index = 0; index < probabilities.Length; index++)
        {
            if (probabilities[index] <= 0d) continue;
            tokenIds.Add(index);
            keptProbabilities.Add(probabilities[index]);
        }

        return new Composition(tokenIds, keptProbabilities);
    }

    /// <summary>Жадный выбор: номер наибольшего логита, при равенстве — наименьший номер.</summary>
    internal static int SelectGreedily(IReadOnlyList<float> logits)
    {
        ArgumentNullException.ThrowIfNull(logits);
        if (logits.Count == 0) throw new ArgumentException("P08_REFERENCE_EMPTY_LOGITS");

        int bestIndex = 0; // Номер наибольшего найденного значения.
        for (int index = 1; index < logits.Count; index++)
            if (logits[index] > logits[bestIndex]) bestIndex = index; // Строгое сравнение сохраняет первый из равных.

        return bestIndex;
    }

    /// <summary>
    /// Выбор знака по накопленной вероятности с той же строгой границей, что в продукте:
    /// правый край промежутка кандидату не принадлежит, поэтому нулевая масса не выбирается.
    /// </summary>
    internal static int Select(Composition composition, double draw)
    {
        ArgumentNullException.ThrowIfNull(composition);

        double accumulated = 0d; // Накопленная вероятность пройденных кандидатов.
        for (int position = 0; position < composition.TokenIds.Count; position++)
        {
            accumulated += composition.Probabilities[position]; // Прибавляем вес очередного кандидата.
            if (draw < accumulated) return composition.TokenIds[position]; // Строгая граница, как в продукте.
        }

        // Остаточный случай накопления: последний кандидат состава.
        return composition.TokenIds[^1];
    }

    /// <summary>Границы накопленной вероятности состава: по одной на каждого кандидата.</summary>
    internal static double[] CumulativeBoundaries(Composition composition)
    {
        ArgumentNullException.ThrowIfNull(composition);

        var boundaries = new double[composition.Probabilities.Count]; // Правый край промежутка каждого кандидата.
        double accumulated = 0d; // Накопленная вероятность пройденных кандидатов.
        for (int position = 0; position < boundaries.Length; position++)
        {
            accumulated += composition.Probabilities[position]; // Прибавляем вес очередного кандидата.
            boundaries[position] = accumulated; // Запоминаем правый край его промежутка.
        }

        return boundaries;
    }

    /// <summary>Маска по числу кандидатов: вне k лучших — минус бесконечность, остальные без изменений.</summary>
    private static float[] MaskByCandidateCount(float[] scaled, int topK)
    {
        int candidateCount = Math.Min(topK, scaled.Length); // Больше кандидатов, чем есть, оставить нельзя.

        // Порог — k-е по убыванию значение; сравнение с ним нестрогое, поэтому при равных
        // значениях на границе кандидатов остаётся больше k. Это повторяет правило продукта.
        var descending = (float[])scaled.Clone();
        Array.Sort(descending, static (left, right) => right.CompareTo(left)); // Убывающий порядок значений.
        float threshold = descending[candidateCount - 1]; // Наименьшее из k наибольших значений.

        var masked = new float[scaled.Length]; // Числа после отсечения по числу кандидатов.
        for (int index = 0; index < masked.Length; index++)
            masked[index] = scaled[index] >= threshold ? scaled[index] : float.NegativeInfinity;

        return masked;
    }

    /// <summary>
    /// Маска по сумме вероятностей: кандидаты обходятся по убыванию, и остаётся тот, перед
    /// которым накопленная сумма ещё не достигла порога. Хотя бы один кандидат остаётся всегда.
    /// </summary>
    private static float[] MaskByProbabilitySum(float[] masked, float topP)
    {
        var order = DescendingOrder(masked); // Номера кандидатов по убыванию значения.
        var orderedValues = new float[order.Length]; // Значения в том же убывающем порядке.
        for (int position = 0; position < order.Length; position++)
            orderedValues[position] = masked[order[position]];

        var orderedProbabilities = SoftMaximum(orderedValues); // Перенормировка оставшегося.

        var kept = new float[masked.Length]; // Числа после отсечения по сумме вероятностей.
        Array.Fill(kept, float.NegativeInfinity); // По умолчанию кандидат исключён.
        double accumulatedBefore = 0d; // Сумма вероятностей кандидатов, стоящих до текущего.
        for (int position = 0; position < order.Length; position++)
        {
            // Кандидат остаётся, если сумма перед ним ещё не добрала порог; первый остаётся
            // при любом положительном пороге, потому что сумма перед ним равна нулю.
            if (accumulatedBefore < topP) kept[order[position]] = masked[order[position]];
            accumulatedBefore += orderedProbabilities[position]; // Сдвигаем накопленную сумму.
        }

        // Правило «хотя бы один кандидат остаётся»: при нулевом пороге наиболее вероятный
        // кандидат возвращается обратно, иначе состав оказался бы пустым.
        if (float.IsNegativeInfinity(kept[order[0]])) kept[order[0]] = masked[order[0]];

        return kept;
    }

    /// <summary>Устойчивый убывающий порядок номеров: при равных значениях меньший номер идёт первым.</summary>
    private static int[] DescendingOrder(float[] values)
    {
        var order = new int[values.Length]; // Номера кандидатов в исходном порядке.
        for (int index = 0; index < order.Length; index++) order[index] = index;

        // Сортировка устойчивая по построению: ключ сравнения включает сам номер,
        // поэтому равные значения сохраняют возрастание номеров, как в продукте.
        Array.Sort(order, (left, right) =>
        {
            int byValue = values[right].CompareTo(values[left]); // Сначала большее значение.
            return byValue != 0 ? byValue : left.CompareTo(right); // При равенстве — меньший номер.
        });

        return order;
    }

    /// <summary>Мягкий максимум: вычитание наибольшего, показательная функция, нормировка.</summary>
    private static double[] SoftMaximum(float[] values)
    {
        double maximum = double.NegativeInfinity; // Наибольшее значение набора.
        foreach (var value in values)
            if (value > maximum) maximum = value;

        var exponentials = new double[values.Length]; // Ненормированные веса кандидатов.
        double sum = 0d; // Сумма весов для нормировки.
        for (int index = 0; index < values.Length; index++)
        {
            // Исключённый кандидат имеет минус бесконечность, и его вес равен нулю.
            exponentials[index] = float.IsNegativeInfinity(values[index]) ? 0d : Math.Exp(values[index] - maximum);
            sum += exponentials[index];
        }

        var probabilities = new double[values.Length]; // Готовые вероятности кандидатов.
        for (int index = 0; index < probabilities.Length; index++)
            probabilities[index] = exponentials[index] / sum;

        return probabilities;
    }
}
