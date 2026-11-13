// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using Experiments;
using Xunit.Abstractions;

namespace LlmFromScratch.Tests.ExperimentFixes;

// Проводка исправленного читателя в настоящем потребителе
// (строка работ «иерархический урок содержит правильную последнюю цель»,
// код правки FIX-HCL, карточка 53-hierarchical-local-history.md).
//
// Зачем эта проверка существует. Рядом уже есть проверка самого читателя на
// исторической панели опыта (семь событий оркестрации, сто пятьдесят два
// обучающих токена, шестнадцать экзаменационных запросов). Но проверка
// помощника не доказывает, что его вызывает тот код, который реально считает
// качество опыта. Эта проверка вызывает именно настоящего потребителя —
// открытый метод `E05HierarchicalPrediction.Run` — и утверждает только то, что
// выводится руками из смысла договора.
//
// Где в исходнике проходит проверяемая проводка:
//   34.Experiments/Experiments/E05HierarchicalPrediction.cs:101
//       обучение иерархии передано в единственный названный шов
//       `E05LocalHierarchy.Fit`;
//   34.Experiments/Experiments/E05HierarchicalPrediction.cs:105-113
//       создаётся `E05HierarchyReader`, вызываются `Reset`, `Observe` текущего
//       токена и `PredictNext`, а глобальный ответ читателя сравнивается с
//       настоящим следующим событием. Другого читателя в `Run` нет.
//
// Честный предел этой проверки, названный прямо. Утверждения ниже НЕ отличают
// исправленный адрес от прежнего. Причина не в лени, а в самом продукте:
// состав кластеров внутри `Run` строит `MachineLearning.KMeansAlgorithm`
// (вызов на строке 84 того же файла), а его начальные центры выбираются
// случайным генератором с зерном 42 (14.MachineLearning/Algorithms/KMeans.cs,
// строки 22-37). Поэтому для произвольного входа разбиение на кластеры нельзя
// вывести руками — его можно только повторить, промоделировав сам продукт, а
// ожидаемое значение, снятое с продукта, доказательством не является.
// Единственные входы, чьё разбиение выводится руками, — вырожденные: если у
// всех токенов одинаковый вектор признаков, все попадают в один кластер, и
// тогда предсказанный кластер совпадает с исходным, последнее наблюдённое
// событие цели — это и есть текущий токен, то есть исправленный и прежний
// адрес совпадают по построению и отличить их невозможно в принципе.
// Поэтому различающий вход через `Run` с руками выведенным ожиданием здесь не
// изобретается. Чувствительность проводки доказывается наблюдением: ниже в
// вывод проверки печатается фактическое число правильных ответов
// иерархической ноги, и это число сравнивается между прогоном с исправлением и
// прогоном с временно убранным исправлением (журналы в
// artifacts/fbe/FIX-HCL/attempt-0001).
public sealed class HierarchicalLocalHistoryRunWiringTests
{
    // Куда печатать наблюдённые числа, чтобы они попали в файл результатов прогона.
    private readonly ITestOutputHelper _output;

    // xUnit сам передаёт сюда приёмник вывода для этого метода проверки.
    public HierarchicalLocalHistoryRunWiringTests(ITestOutputHelper output) => _output = output;

    // Длина входа: пять циклов по сто различных событий.
    private const int TokenCount = 500;

    // Число различных событий в цикле; оно же число уникальных токенов обучения.
    private const int CycleLength = 100;

    // Доля обучения в `Run` равна 0.8, поэтому обучение получает первые 400 токенов.
    private const int ExpectedTrainCount = 400;

    // Проверочная часть — остальные 100 токенов, значит пар для предсказания 99.
    private const int ExpectedValidationPairs = 99;

    // Размер словаря в `Run` считается как наибольший токен обучения плюс один.
    private const int ExpectedVocabSize = 100;

    // Число кластеров в `Run`: не меньше пяти, не больше пятидесяти, иначе
    // число уникальных токенов делённое на пять, то есть сто делить на пять.
    private const int ExpectedClusterCount = 20;

    [Fact]
    public void RealRunConsumer_ReachesFixedReaderSeam_AndCountsOverEveryValidationPair()
    {
        // ── Шаг 1. Вход, который удовлетворяет собственным защитам потребителя ──

        // Чистый цикл событий: каждое событие имеет ровно одного наблюдённого
        // последователя, поэтому счётная модель переходов обучается однозначно.
        var tokens = new List<int>(TokenCount);
        // Токен на позиции i равен остатку i по длине цикла.
        for (var index = 0; index < TokenCount; index++)
            tokens.Add(index % CycleLength);

        // Защита потребителя требует не меньше десяти токенов; у нас пятьсот.
        Assert.True(tokens.Count >= 10);
        // Вторая защита требует не меньше пятидесяти уникальных токенов обучения;
        // в первых четырёхста токенах их ровно сто, и это считается руками.
        Assert.Equal(ExpectedVocabSize, tokens.Take(ExpectedTrainCount).Distinct().Count());

        // ── Шаг 2. Вызов настоящего потребителя ──────────────────────────────

        // Это тот самый открытый метод, который считает качество опыта E05.
        var runResult = new E05HierarchicalPrediction().Run(tokens);
        // Отказ здесь означал бы, что проводка до шва вообще не доходит.
        Assert.True(runResult.IsSuccess, runResult.ErrorMessage);
        // Значение результата нужно дальше для разбора обеих ног опыта.
        var experiment = runResult.Value!;

        // ── Шаг 3. Ожидания, выведенные руками из смысла договора ────────────

        // Опыт обязан назвать себя своим собственным именем.
        Assert.Equal("E05", experiment.ExperimentId);

        // Заметка потребителя выводится руками: кластеров двадцать, размер
        // словаря сто, уникальных токенов обучения сто. Это доказывает, что
        // шов получил именно те входы, на которых он должен работать.
        Assert.Equal(
            $"K={ExpectedClusterCount}, vocabSize={ExpectedVocabSize}, seenTokens={ExpectedVocabSize}",
            experiment.Notes);

        // Обе ноги опыта обязаны сообщить длину исходного набора без искажения.
        Assert.Equal(TokenCount, experiment.Baseline.DatasetTokenCount);
        Assert.Equal(TokenCount, experiment.Enhanced.DatasetTokenCount);

        // Плоская счётная модель на чистом цикле обязана быть точной: в каждой
        // строке её таблицы ровно один ненулевой столбец — настоящий
        // последователь, поэтому все девяносто девять проверочных пар угаданы.
        // Единица здесь написана руками из этого рассуждения, а не снята с кода.
        Assert.Equal(1.0, experiment.Baseline.AccuracyFraction);

        // Доля иерархической ноги обязана быть долей целого числа правильных
        // ответов на девяносто девять проверочных пар: по договору читатель
        // даёт ровно одно предсказание на каждую пару. Нецелое произведение
        // означало бы другой знаменатель, то есть другой цикл чтения.
        var enhancedCorrectExact = experiment.Enhanced.AccuracyFraction * ExpectedValidationPairs;
        // Округление до ближайшего целого — это и есть число правильных ответов.
        var enhancedCorrect = (int)Math.Round(enhancedCorrectExact);
        // Отклонение от целого допускается только в пределах ошибки деления.
        Assert.True(
            Math.Abs(enhancedCorrectExact - enhancedCorrect) < 1e-9,
            $"Доля иерархической ноги {experiment.Enhanced.AccuracyFraction} не является долей целого числа правильных ответов из {ExpectedValidationPairs}.");
        // Число правильных ответов не может быть отрицательным или больше числа пар.
        Assert.InRange(enhancedCorrect, 0, ExpectedValidationPairs);

        // ── Шаг 4. Наблюдение для сравнения прогонов, а не утверждение ───────

        // Эти строки печатаются в файл результатов прогона. Прогон с временно
        // убранным исправлением адреса даёт здесь другое число, и именно это
        // сравнение доказывает, что настоящий потребитель читает обученную
        // таблицу по исправленному адресу. Само число ожиданием не является.
        _output.WriteLine($"FIX-HCL-RUN-WIRING enhanced_correct={enhancedCorrect} of {ExpectedValidationPairs}");
        _output.WriteLine($"FIX-HCL-RUN-WIRING enhanced_accuracy={experiment.Enhanced.AccuracyFraction:R}");
        _output.WriteLine($"FIX-HCL-RUN-WIRING baseline_accuracy={experiment.Baseline.AccuracyFraction:R}");
        _output.WriteLine($"FIX-HCL-RUN-WIRING notes={experiment.Notes}");
    }
}
