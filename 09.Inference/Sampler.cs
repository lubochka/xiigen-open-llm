// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;

namespace Inference;

// Стратегии выборки следующего токена: greedy, temperature, top-k, top-p.
// DNA rule 1: все настройки именованные, никаких magic numbers.
public static class Sampler
{
    // logits: [vocabSize] float тензор
    public static int Sample(Tensor logits, float temperature = 1.0f, int topK = 0, float topP = 1.0f)
    {
        return Sample(logits, Random.Shared, temperature, topK, topP);
    }

    internal static int Sample(Tensor logits, Random random, float temperature, int topK, float topP)
    {
        ArgumentNullException.ThrowIfNull(random);
        // Температура ≤ 0 → greedy (argmax)
        if (temperature <= 0f)
            return TensorOps.Argmax(logits);

        // Применяем температуру: логиты делим на температуру
        var scaled = TensorOps.Scale(logits, 1f / temperature);

        Tensor probs; // Распределение вероятностей кандидатов, из которого делается случайный выбор.
        if (topK > 0 && topP < 1.0f)
        {
            // Заданы оба ограничения сразу. Сначала оставляем только k лучших кандидатов,
            // причём оставляем их логитами, а не готовыми вероятностями: второе правило
            // обязано считать сумму вероятностей уже по этому короткому списку.
            var keptByCandidateCount = TopKMask(scaled, topK);
            // Теперь отсечение по сумме вероятностей работает по замаскированным логитам и
            // само выполняет перенормировку. Прежде при заданном k это правило молча терялось.
            probs = TopPFilter(keptByCandidateCount, topP);
        }
        else if (topK > 0)
            probs = TopKFilter(scaled, topK); // Задано только число кандидатов: поведение прежнее.
        else if (topP < 1.0f)
            probs = TopPFilter(scaled, topP); // Задана только сумма вероятностей: поведение прежнее.
        else
            probs = TensorOps.Softmax(scaled, dim: -1); // Ограничений нет: поведение прежнее.

        return MultinomialSample(probs, random);
    }

    // Top-K фильтр: оставляем только k самых вероятных токенов.
    // Это та же маска по числу кандидатов плюс завершающий softmax. Маска вынесена отдельно,
    // чтобы её можно было передать дальше в отсечение по сумме вероятностей без промежуточной
    // перенормировки; поведение случаев «только k» и «только p» от этого не меняется.
    private static Tensor TopKFilter(Tensor logits, int k)
        => TensorOps.Softmax(TopKMask(logits, k), dim: -1);

    // Маска по числу кандидатов: вне k лучших ставим минус бесконечность и возвращаем логиты.
    private static Tensor TopKMask(Tensor logits, int k)
    {
        int n = logits.Shape.TotalElements; // Сколько всего кандидатов в словаре.
        k = Math.Min(k, n); // Нельзя оставить больше кандидатов, чем их есть в словаре.

        // Находим k-й по убыванию элемент через частичную сортировку
        var sorted    = logits.Data.OrderByDescending(v => v).Take(k).ToArray(); // k наибольших значений по убыванию.
        float threshold = sorted[^1]; // наименьший из top-k

        var filtered = new float[n]; // Логиты после отсечения по числу кандидатов.
        for (int i = 0; i < n; i++)
            // Сравнение нестрогое: при равных значениях на границе остаётся больше k кандидатов,
            // и это прежнее поведение отбора сохраняется без изменений.
            filtered[i] = logits.Data[i] >= threshold ? logits.Data[i] : float.NegativeInfinity;

        return new Tensor(filtered, logits.Shape); // Возвращаются именно логиты, а не вероятности.
    }

    // Top-P (nucleus) фильтр: берём токены пока суммарная вероятность < p
    private static Tensor TopPFilter(Tensor logits, float p)
    {
        int n = logits.Shape.TotalElements;

        // Отсортировать по убыванию
        var (sortedVals, sortedIdx) = TensorOps.SortDescending(logits);

        // Softmax отсортированных → cumulative sum
        var sortedProbs = TensorOps.Softmax(sortedVals, dim: -1);
        var cumProbs    = TensorOps.CumSum(sortedProbs, dim: 0);

        // Маскируем токены за пределами ядра (nucleus)
        // Сдвигаем маску вправо чтобы включить первый токен, превысивший p
        var filtered = new float[n];
        for (int i = 0; i < n; i++)
        {
            float cumPrev = i == 0 ? 0f : cumProbs.Data[i - 1];
            int   origIdx = (int)sortedIdx.Data[i];
            filtered[origIdx] = cumPrev < p ? logits.Data[origIdx] : float.NegativeInfinity;
        }

        // Хотя бы один токен должен остаться (первый — наиболее вероятный)
        int topIdx = (int)sortedIdx.Data[0];
        if (float.IsNegativeInfinity(filtered[topIdx]))
            filtered[topIdx] = logits.Data[topIdx];

        return TensorOps.Softmax(new Tensor(filtered, logits.Shape), dim: -1);
    }

    // Мультиномиальная выборка: выбираем токен пропорционально вероятностям.
    // Внутренний вход открыт проверкам, чтобы недопустимую общую массу можно было
    // задать напрямую; публичного входа ради этого не добавляется.
    internal static int MultinomialSample(Tensor probs, Random random)
    {
        int n = probs.Shape.TotalElements; // Сколько кандидатов в распределении.
        double mass = 0d; // Общая масса вероятностей, посчитанная один раз в двойной точности.
        int lastPositiveIndex = -1; // Последний кандидат с положительной вероятностью.
        for (int i = 0; i < n; i++)
        {
            float value = probs.Data[i]; // Вероятность одного кандидата.
            // Отрицательное или нечисловое значение — это испорченное распределение,
            // а не редкий край: выбирать по нему нельзя, и генератор ещё не трогали.
            if (!float.IsFinite(value) || value < 0f)
                throw new InvalidOperationException("SAMPLER_INVALID_PROBABILITY_MASS");
            if (value > 0f) lastPositiveIndex = i; // Запоминаем последний живой отрезок.
            mass += value; // Общая масса копится в двойной точности.
        }

        // Неположительная или нечисловая общая масса тоже отвергается до генератора:
        // иначе случайная величина делилась бы на отсутствующее распределение.
        if (!double.IsFinite(mass) || mass <= 0d || lastPositiveIndex < 0)
            throw new InvalidOperationException("SAMPLER_INVALID_PROBABILITY_MASS");

        // Ровно один вызов генератора, растянутый на фактическую общую массу: при сумме
        // чуть меньше единицы зазора между последним отрезком и единицей больше не остаётся.
        double r = random.NextDouble() * mass;
        double acc = 0d; // Накопленная сумма вероятностей.
        for (int i = 0; i < n; i++)
        {
            acc += probs.Data[i]; // Правая граница отрезка этого кандидата.
            // Отрезок этого токена занимает промежуток от прежней накопленной суммы до новой,
            // причём правый край в него не входит: только строгое сравнение не позволяет
            // нулевой случайной величине попасть в токен с нулевой вероятностью, у которого
            // накопленная сумма ещё не сдвинулась с места.
            if (r < acc) return i;
        }

        // Защитный возврат только на случай потери точности при сложении: он отдаёт
        // последний кандидат с положительной вероятностью, а не последний номер словаря.
        return lastPositiveIndex;
    }
}
