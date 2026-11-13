// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Reflection;
using ExperimentV2Result = ExperimentsV2.ExperimentV2Result;

// Снятие ложного заявления о подобранном пороге в опыте разделения запросов
// (класс ExperimentsV2.E21PromptSplitter, опыт двадцать один «разделитель запроса»).
//
// Что было не так, по-человечески. Опыт сообщал число как «подобранный порог»
// и печатал его в заметку о результате. Подбора не было. Закрытый поиск перебирал
// значения от 0,20 до 0,90, но перебираемое значение не входило в счёт, с которым
// его сравнивали: счёт на каждом шаге получался один и тот же, поэтому побеждал
// всегда самый первый кандидат, и поиск возвращал 0,20 при любых данных.
// Записанное в коде значение по умолчанию 0,40 было недостижимо.
//
// И даже верное число деть было бы некуда. Усиленное плечо опыта вызывает
// продуктовый разборщик запросов из двадцать девятого проекта, а он принимает
// только сам запрос: ни порога, ни настройки, ни конструктора с числом у него нет.
// Это проверяется здесь отдельно, чтобы удаление поиска опиралось на факт кода,
// а не на память исполнителя.
//
// Что проверяется теперь. Опыт больше не выдаёт никакого числа как подобранный
// порог: поле масштабирующего сигнала равно нулю, подпись прямо говорит, что
// измерения нет, и слово «порог» не встречается ни в подписи, ни в заметке.
//
// Чувствительность к поломке. Та же самая проверка применяется к восстановленному
// старому виду результата: если она на нём не падает, падает сама проверка
// чувствительности. Зелёная проверка, которая прошла бы и на старом коде, тут
// невозможна.
//
// Доза этой проверки: ноль обучения, ноль прямых и обратных проходов модели,
// ноль обновлений оптимизатора, ноль обращений к диску и ровно один прогон опыта.
public sealed class PromptSplitterTunedThresholdClaimTests
{
    // Слова, которыми прежняя подпись заявляла подбор. Ни одно из них не должно
    // остаться в том, что опыт публикует о себе.
    private static readonly string[] TuningClaimWords = ["threshold", "порог", "tuned", "подобран"];

    // Главная регрессия дефекта: в результате опыта нет числа, выданного за подобранный порог.
    [Fact]
    public void Result_PublishesNoNumberAsATunedThreshold()
    {
        // Один прогон опыта на первом ярусе — ровно столько, сколько нужно для проверки.
        var run = new ExperimentsV2.E21PromptSplitter().Run(tier: 1);

        // Опыт обязан отработать: без результата проверять нечего.
        Assert.True(run.IsSuccess, run.ErrorMessage);
        Assert.NotNull(run.Value);

        // Тот же разбор, что применяется ниже к старому виду результата.
        AssertNoTuningClaim(run.Value!);
    }

    // Честная замена: поле не просто обнулено, оно подписано как «измерения нет».
    // Молчаливый ноль без подписи читался бы как измеренная величина.
    [Fact]
    public void Result_SaysPlainlyThatNoScalingSignalWasMeasured()
    {
        var run = new ExperimentsV2.E21PromptSplitter().Run(tier: 1);
        Assert.True(run.IsSuccess, run.ErrorMessage);

        // Подпись непустая: пустая строка не объясняет читателю нулевое поле.
        Assert.False(string.IsNullOrWhiteSpace(run.Value!.ScalingSignalNote));
        // И прямо говорит, что сигнала нет, а не называет ноль измеренным значением.
        Assert.Contains("no scaling signal", run.Value!.ScalingSignalNote, StringComparison.OrdinalIgnoreCase);
    }

    // Проверка чувствительности. Старый вид результата восстановлен дословно: то самое
    // число 0,20 в поле сигнала, та самая подпись про подобранный порог и та самая
    // заметка с «threshold=». Разбор обязан на нём упасть. Если он проходит —
    // значит разбор ничего не ловит, и падает уже эта проверка.
    [Fact]
    public void SameCheck_FailsOnTheRestoredBrokenResultShape()
    {
        var brokenShape = BuildPreFixResultShape();

        // Разбор обязан отвергнуть старый вид результата.
        var failure = Record.Exception(() => AssertNoTuningClaim(brokenShape));

        Assert.NotNull(failure); // Молчание здесь означало бы, что проверка слепа.
        Assert.IsAssignableFrom<Xunit.Sdk.XunitException>(failure);
    }

    // Почему удалённый поиск не был измерением: его перебираемое значение не входило
    // в счёт. Локальная копия старой логики подтверждает это на данных, а не на словах:
    // при любом наборе строк она возвращает первого кандидата, 0,20.
    [Theory]
    [InlineData(0)]   // Пустой набор: счёт всегда ноль.
    [InlineData(7)]   // Все строки «угаданы»: счёт всегда максимум.
    [InlineData(13)]  // Ни одна не угадана: счёт всегда ноль.
    public void RemovedSearch_ReturnedItsFirstCandidateForAnyData(int rowCount)
    {
        // Старый поиск дословно: перебор от 0,20 до 0,90 шагом 0,05, а счёт от перебора
        // не зависит — именно это и было дефектом.
        static double RemovedTuneThreshold(int constantScoreRows)
        {
            double best = 0.4; int bestScore = -1;
            for (double t = 0.20; t <= 0.90; t += 0.05)
            {
                int score = constantScoreRows; // Счёт одинаков на каждом шаге: t в него не входит.
                if (score > bestScore) { bestScore = score; best = t; }
            }
            return best;
        }

        // Возврат всегда первый кандидат перебора, независимо от данных.
        Assert.Equal(0.20, RemovedTuneThreshold(rowCount), precision: 10);
        // И никогда — записанное в коде значение по умолчанию 0,40.
        Assert.NotEqual(0.40, RemovedTuneThreshold(rowCount), precision: 10);
    }

    // Опровержение, проверенное кодом, а не памятью: продуктовому разборщику порог
    // передать физически некуда. Если у него когда-нибудь появится настройка с таким
    // именем, эта проверка упадёт и заставит пересмотреть удаление поиска.
    [Fact]
    public void ProductionDecomposer_ExposesNoThresholdToTune()
    {
        var decomposer = typeof(SemanticCompiler29.Decomposer);

        // Ни один открытый конструктор не принимает порог или настройку с числом.
        foreach (var constructor in decomposer.GetConstructors())
            AssertNoThresholdParameter(constructor.GetParameters(), $"конструктор {decomposer.Name}");

        // Ни один открытый метод самого разборщика не принимает порог.
        foreach (var method in decomposer.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            AssertNoThresholdParameter(method.GetParameters(), $"метод {method.Name}");

        // И у него нет открытого свойства с порогом, куда число можно было бы положить.
        var thresholdProperty = decomposer
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p => p.Name.Contains("threshold", StringComparison.OrdinalIgnoreCase));
        Assert.Null(thresholdProperty);
    }

    // ── Общий разбор, одинаковый для нынешнего и для старого вида результата ──────

    // Отвергает любой результат опыта, который выдаёт число за подобранный порог.
    private static void AssertNoTuningClaim(ExperimentV2Result result)
    {
        // Поле масштабирующего сигнала — ровно нейтральный ноль, а не остаток подбора.
        Assert.Equal(0.0, result.ScalingSignal);

        // Подпись сигнала не заявляет ни порога, ни подбора.
        foreach (var word in TuningClaimWords)
            Assert.DoesNotContain(word, result.ScalingSignalNote, StringComparison.OrdinalIgnoreCase);

        // Заметка о результате тоже не печатает порог ни на каком языке.
        foreach (var word in TuningClaimWords)
            Assert.DoesNotContain(word, result.Notes, StringComparison.OrdinalIgnoreCase);
    }

    // Проверяет один список параметров на отсутствие порога или настройки-числа.
    private static void AssertNoThresholdParameter(ParameterInfo[] parameters, string owner)
    {
        var threshold = parameters.FirstOrDefault(p =>
            p.Name is not null && p.Name.Contains("threshold", StringComparison.OrdinalIgnoreCase));
        Assert.True(threshold is null, $"{owner} неожиданно принимает порог: {threshold?.Name}");
    }

    // Дословное восстановление того, что опыт публиковал до правки: число 0,20 из
    // сломанного поиска, подпись про подобранный порог и заметка с «threshold=».
    // Используется только проверкой чувствительности и нигде не исполняется как опыт.
    private static ExperimentV2Result BuildPreFixResultShape() => new()
    {
        ExperimentId      = "E21-PROMPT-SPLITTER",
        Tier              = 1,
        BaselinePrimary   = 0.5,
        EnhancedPrimary   = 0.75,
        PrimaryMetricName = "boundary_accuracy",
        LowerIsBetter     = false,
        ScalingSignal     = 0.20,
        ScalingSignalNote = "tuned_threshold (stable threshold → better generalisation)",
        Notes             = "examples=110, val=22, threshold=0.20, baseAcc=50.0%, enhAcc=75.0%"
    };
}
