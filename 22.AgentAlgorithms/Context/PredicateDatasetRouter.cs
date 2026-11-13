// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentAlgorithms;

// Production source for E22 dataset routing.
// Predicts the relevant dataset buckets from an E17 predicate vector with KNN voting.
public sealed class PredicateDatasetRouter
{
    public static readonly string[] DatasetNames =
        ["Generic", "CSharp", "TypeScript", "BugFix", "Orchestration", "Tests"];

    // Память маршрутизатора: таблица принятых уроков. Поле намеренно не readonly —
    // приём партии публикует собственную новую таблицу вместо дописывания в прежнюю.
    private List<(int[] Predicates, bool[] Datasets)> _train = [];
    private readonly int _k;
    private readonly int _datasetCount;

    public PredicateDatasetRouter(int k = 3, int datasetCount = 6)
    {
        _k = k;
        _datasetCount = datasetCount;
    }

    // Принимает партию уроков в память маршрутизатора собственными копиями обоих массивов строки.
    public void Train(IReadOnlyList<(int[] Predicates, bool[] Datasets)> examples)
    {
        // Новая приватная таблица собирается отдельно, чтобы в память легли только собственные массивы.
        var ownedRows = new List<(int[] Predicates, bool[] Datasets)>(examples.Count);

        // Каждая строка партии переносится в память поэлементной копией, а не ссылкой вызывающей стороны.
        foreach (var example in examples)
        {
            // Под признаки строки выделяется собственный массив ровно той длины, какую дал вызывающий.
            var ownedPredicates = new int[example.Predicates.Length];

            // Значения признаков переписываются в собственный массив: ссылка вызывающей стороны в память не попадает.
            Array.Copy(example.Predicates, ownedPredicates, ownedPredicates.Length);

            // Под метки строки выделяется собственный массив той же длины, какую дал вызывающий:
            // действующая политика ширины сохраняется — короткую маску чтение дополнит значениями false,
            // а элементы сверх числа корзин оно по-прежнему проигнорирует.
            var ownedDatasets = new bool[example.Datasets.Length];

            // Значения меток переписываются в собственный массив: вторая ссылка вызывающей стороны тоже отсекается.
            Array.Copy(example.Datasets, ownedDatasets, ownedDatasets.Length);

            // В таблицу памяти кладётся строка, оба массива которой принадлежат маршрутизатору.
            ownedRows.Add((ownedPredicates, ownedDatasets));
        }

        // Собранная таблица публикуется целиком и становится памятью маршрутизатора.
        _train = ownedRows;
    }

    public bool[] Predict(int[] queryPredicates)
    {
        if (_train.Count == 0)
            return Enumerable.Repeat(true, _datasetCount).ToArray();

        // Запрос без единого признака сравнивать не с чем: расстояние Жаккара от пустого
        // набора до любой непустой учебной строки равно единице, поэтому все уроки
        // одинаково «близки», а устойчивая сортировка ниже просто берёт первый уложенный
        // урок. Его метка — вид осмысленного решения без основания, и зависит она от
        // порядка укладки уроков, а не от запроса. Поэтому обученный маршрутизатор здесь
        // отказывается выбирать: возвращается вектор нужной длины, в котором не отмечена
        // ни одна корзина. Проверка стоит ПОСЛЕ ветки пустой памяти — необученный
        // маршрутизатор сохраняет прежнее умолчание «брать все корзины» — и ДО поиска
        // соседей. Образец `is { Length: 0 }` выбран вместо `Length == 0` намеренно:
        // на пустой ссылке он не срабатывает, и прежняя обработка null не меняется.
        if (queryPredicates is { Length: 0 })
            // Новый массив нужной длины уже заполнен значениями false: ни одна корзина не выбрана.
            return new bool[_datasetCount];

        var neighbours = _train
            .OrderBy(row => JaccardDistance(queryPredicates, row.Predicates))
            .Take(_k)
            .ToArray();

        // Take(_k) yields fewer rows than _k when memory holds fewer than _k examples,
        // so the majority divisor is the neighbour count actually found, not the configured _k.
        var actualNeighbourCount = neighbours.Length;

        var result = new bool[_datasetCount];
        for (var index = 0; index < _datasetCount; index++)
        {
            var votes = neighbours.Count(row => index < row.Datasets.Length && row.Datasets[index]);
            // A bucket wins when its votes strictly exceed half of the neighbours actually found;
            // integer division is unchanged, so an even split still refuses the bucket.
            result[index] = votes > actualNeighbourCount / 2;
        }

        return result;
    }

    public static bool[] AutoLabel(string source)
    {
        var label = new bool[DatasetNames.Length];
        var lower = source.ToLowerInvariant();

        label[0] = source.Length > 100;
        label[1] = lower.Contains("namespace ", StringComparison.Ordinal) ||
                   lower.Contains("using system", StringComparison.Ordinal);
        label[2] = (lower.Contains("interface ", StringComparison.Ordinal) && lower.Contains("injectable", StringComparison.Ordinal)) ||
                   lower.Contains("export default", StringComparison.Ordinal) ||
                   lower.Contains("@module", StringComparison.Ordinal);
        label[3] = lower.Contains("throw ", StringComparison.Ordinal) ||
                   lower.Contains("catch (", StringComparison.Ordinal) ||
                   NeedsNullCheck(source);
        label[4] = (lower.Contains("flow", StringComparison.Ordinal) && lower.Contains("phase", StringComparison.Ordinal)) ||
                   lower.Contains("orchestrat", StringComparison.Ordinal) ||
                   lower.Contains("pipeline", StringComparison.Ordinal);
        label[5] = lower.Contains("[fact]", StringComparison.Ordinal) ||
                   lower.Contains("[test]", StringComparison.Ordinal) ||
                   lower.Contains("assert.", StringComparison.Ordinal) ||
                   lower.Contains("[testmethod]", StringComparison.Ordinal);

        return label;
    }

    private static bool NeedsNullCheck(string source)
    {
        var lower = source.ToLowerInvariant();
        return lower.Contains(".", StringComparison.Ordinal) &&
               !lower.Contains(" null", StringComparison.Ordinal) &&
               !lower.Contains("is null", StringComparison.Ordinal);
    }

    private static double JaccardDistance(int[] left, int[] right)
    {
        var leftSet = new HashSet<int>(left);
        var rightSet = new HashSet<int>(right);
        var intersection = leftSet.Count(rightSet.Contains);
        var union = leftSet.Count + rightSet.Count - intersection;
        return union == 0 ? 0.0 : 1.0 - (double)intersection / union;
    }
}
