// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное покрытие договора подсчёта «честное воздержание памяти формул — это
// правильный ответ, а не провал исполнения» для проверки опыта на закрытых
// формулах (файл 35.ExperimentsV2/Experiments/E13BHeldOutValidation.cs).
//
// Что здесь проверяется по-человечески. В проекте есть маленькая обучаемая
// память формул. Её учат целыми записями вида «Выручка минус Затраты равно
// Прибыль», она запоминает соответствие «левая часть → правая часть», а потом по
// левой части возвращает правую. Опыт на закрытых формулах измеряет две разные
// вещи сразу: выученные формулы обязаны отвечать точно, а невыученные обязаны
// честно промолчать. Второе плечо и есть предмет этого класса проверок.
//
// Что было сломано. Перенос сделал ответ памяти «такой формулы я не учила»
// честным типизированным отказом: отдельный код причины вместо успешного
// конверта с пустым значением. Подсчёт опыта остался на старом договоре и
// опознавал незнание по пустому успешному конверту. Такого конверта больше не
// бывает, поэтому законное воздержание уходило в общую ветку «обращение не
// удалось» и записывалось как провал исполнения: правильность ложная, причина
// «исполнение сорвалось». Доля воздержания при этом оставалась единицей, потому
// что она считается по полю совпадения, а не по полю правильности, — и поломка
// не была видна сверху.
//
// Почему это не поймал никто. У этой единицы опыта до сих пор не было ни одного
// класса проверок: её читает только отдельная программа-запускатель
// (experiments-v2/E13B-EXPRESSION-PREFIX-COMPLETION/runner/
// E13BHeldOutValidationRunner/Program.cs). Сборка оставалась зелёной, а поведение
// уехало молча.
//
// Бюджет. Опыт прогоняется ровно дважды на весь класс проверок: один раз обычно
// и один раз с заранее отменённым признаком отмены. Каждый прогон — это одно
// обучение из двадцати уроков и тридцать четыре обращения к памяти (двадцать
// обучающих формул, десять закрытых, четыре варианта записи). Итого два
// обучения, сорок уроков и шестьдесят восемь обращений. Ни одного обращения к
// диску, ни одной внешней модели, ни одной пары предпочтений.
//
// Чего это покрытие НЕ доказывает. Оно не доказывает, что память формул умеет
// выводить новые формулы: верный ответ для выученной формулы здесь — точный
// возврат выученной правой части, а для невыученной — молчание. Обобщения на
// невиданную формулу здесь ноль. Качество обучения здесь не измеряется, веса не
// меняются, контрольные точки не сохраняются.

using ExperimentsV2;

namespace LlmFromScratch.Tests.ExperimentFixes;

// Общая заготовка прогонов. xUnit создаёт её один раз на весь класс проверок,
// поэтому назначенный бюджет обучений и обращений расходуется ровно один раз.
public sealed class E13BAbstentionPanel
{
    // Обычный прогон опыта: признак отмены не тронут, память формул отвечает
    // выученным значением на знакомую левую часть и отказом «формулы нет в
    // памяти» на незнакомую.
    public E13BHeldOutValidationResult Normal { get; }

    // Прогон с заранее отменённым признаком отмены: память формул отвечает
    // отказом с ДРУГИМ кодом причины — «отменено». Это настоящий сбой
    // исполнения, а не воздержание, и подсчёт обязан различать эти два случая.
    public E13BHeldOutValidationResult Cancelled { get; }

    public E13BAbstentionPanel()
    {
        // Первый прогон: обычный, без отмены.
        var normal = new E13BHeldOutValidation().RunAsync().GetAwaiter().GetResult();

        // Сам конверт прогона обязан быть успешным: обучение проходит, падать
        // здесь нечему. Содержательные утверждения живут в проверках ниже.
        Assert.True(normal.IsSuccess, normal.ErrorMessage);
        Normal = normal.Value!;

        // Второй прогон: источник признака отмены отменяется ДО вызова, поэтому
        // каждое обращение к памяти получает отказ «отменено».
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var cancelled = new E13BHeldOutValidation()
            .RunAsync(cancellation.Token)
            .GetAwaiter()
            .GetResult();

        // Опыт не бросает исключение на отмену, а возвращает сводку с нулевыми
        // долями, поэтому конверт прогона здесь тоже успешный.
        Assert.True(cancelled.IsSuccess, cancelled.ErrorMessage);
        Cancelled = cancelled.Value!;
    }
}

public sealed class E13BAbstentionScoringContractTests
    : IClassFixture<E13BAbstentionPanel>
{
    // Причина в записи: формула в уроках не встречалась. Это ожидаемый ответ для
    // плеча закрытых формул.
    private const string NotSeenReason = "expression_not_seen";

    // Причина в записи: ответ пришёл из выученной памяти.
    private const string ExactMatchReason = "exact_match";

    // Причина в записи: обращение к памяти сорвалось. Это НЕ воздержание.
    private const string ExecutionFailedReason = "execution_failed";

    // Единственный невыученный запрос набора вариантов записи. Он выписан
    // буквально, чтобы проверка не брала его из самого результата.
    private const string UnknownVariantQuery = "A + B = ?";

    private readonly E13BAbstentionPanel _panel;

    public E13BAbstentionScoringContractTests(E13BAbstentionPanel panel)
        => _panel = panel;

    // ── Главные проверки починки ─────────────────────────────────────────────
    // Проверка первая. Все десять закрытых формул честно промолчали, и подсчёт
    // обязан назвать это правильным ответом, потому что это плечо как раз и
    // ожидало незнания.
    [Fact]
    public void HeldOutAbstention_IsCountedCorrect()
    {
        // Размер плеча выписан буквально: десять закрытых формул опыта.
        Assert.Equal(10, _panel.Normal.HeldOutResults.Count);

        // Считаются строки с истинной правильностью.
        var correct = _panel.Normal.HeldOutResults.Count(row => row.Correct);

        Assert.Equal(10, correct);
    }

    // Проверка вторая. Причина в записи обязана по-человечески означать
    // «формулу не учили», а не «исполнение сорвалось»: это разные события, и
    // отчёт запускателя печатает именно причину.
    [Fact]
    public void HeldOutAbstention_ReasonSaysExpressionWasNotLearned()
    {
        var notSeen = _panel.Normal.HeldOutResults
            .Count(row => string.Equals(row.Reason, NotSeenReason, StringComparison.Ordinal));

        var executionFailed = _panel.Normal.HeldOutResults
            .Count(row => string.Equals(row.Reason, ExecutionFailedReason, StringComparison.Ordinal));

        Assert.Equal(10, notSeen);
        Assert.Equal(0, executionFailed);
    }

    // Проверка третья. Тот же договор на наборе вариантов записи: единственный
    // невыученный запрос обязан быть засчитан правильным воздержанием.
    [Fact]
    public void UnknownNormalisationVariant_IsCountedCorrect()
    {
        var row = _panel.Normal.NormalisationVariantResults
            .Single(candidate => string.Equals(
                candidate.Partial,
                UnknownVariantQuery,
                StringComparison.Ordinal));

        Assert.True(
            row.Correct,
            $"Запрос '{row.Partial}' в уроках не встречался, и этот набор как раз " +
            "ожидал незнания, поэтому воздержание обязано считаться правильным " +
            "ответом, а не провалом исполнения.");

        Assert.Equal(NotSeenReason, row.Reason);

        // Воздержание не является совпадением ни при каком ожидании.
        Assert.False(row.Matched);

        // У отказа нет ни значения ответа, ни уверенности.
        Assert.Equal("", row.Predicted);
        Assert.Equal(0.0, row.Confidence);
    }

    // Проверка четвёртая. Числовая доля правильных на наборе вариантов записи:
    // три выученных варианта плюс одно законное воздержание — это четыре из
    // четырёх, а не три из четырёх.
    [Fact]
    public void NormalisationVariantAccuracy_IsOne()
    {
        Assert.Equal(4, _panel.Normal.NormalisationVariantResults.Count);
        Assert.Equal(1.0, _panel.Normal.NormalisationVariantAccuracy);
    }

    // Проверка пятая. Общий исход опыта. Доля воздержания и доля ложных
    // срабатываний по закрытым формулам были правильными и до починки — именно
    // поэтому поломка не была видна сверху; сломан был общий исход, который
    // требует единицы на наборе вариантов записи.
    [Fact]
    public void OverallOutcome_IsSuccess()
    {
        Assert.Equal(1.0, _panel.Normal.HeldOutAbstainRate);
        Assert.Equal(0.0, _panel.Normal.FalsePositiveRate);

        Assert.True(
            _panel.Normal.Success,
            "Выученные формулы отвечают точно, закрытые формулы честно молчат, " +
            "варианты записи разобраны верно — общий исход опыта обязан быть " +
            "успешным.");
    }

    // ── Контроли сохранности уже работающего умения ──────────────────────────
    // Проверка шестая. Все двадцать обучающих формул отвечают тем же значением и
    // той же уверенностью, что и до починки. Ожидаемая правая часть берётся из
    // текста обучающей формулы, а не из ответа памяти, иначе проверка сравнивала
    // бы ответ сам с собой.
    [Fact]
    public void TrainedExpressions_AnswerAndConfidenceUnchanged()
    {
        Assert.Equal(20, _panel.Normal.TrainingResults.Count);

        foreach (var row in _panel.Normal.TrainingResults)
        {
            Assert.True(row.Matched, $"Выученная формула '{row.Partial}' не ответила.");
            Assert.True(row.Correct, $"Выученная формула '{row.Partial}' ответила неверно.");
            Assert.Equal(row.Expected, row.Predicted);
            Assert.Equal(1.0, row.Confidence);
            Assert.Equal(ExactMatchReason, row.Reason);
        }

        // Доли по обучающему плечу остаются полными.
        Assert.Equal(1.0, _panel.Normal.TrainingMatchedRate);
        Assert.Equal(1.0, _panel.Normal.TrainingCompletionAccuracy);
    }

    // Проверка седьмая. Три выученных варианта записи — с пробелами, без
    // пробелов и заглавными буквами — по-прежнему дают одну и ту же выученную
    // правую часть с полной уверенностью.
    [Fact]
    public void LearnedNormalisationVariants_AnswerUnchanged()
    {
        var learned = _panel.Normal.NormalisationVariantResults
            .Where(row => !string.Equals(
                row.Partial,
                UnknownVariantQuery,
                StringComparison.Ordinal))
            .ToList();

        Assert.Equal(3, learned.Count);

        foreach (var row in learned)
        {
            Assert.True(row.Matched, $"Выученный вариант '{row.Partial}' не ответил.");
            Assert.True(row.Correct, $"Выученный вариант '{row.Partial}' ответил неверно.");
            Assert.Equal("Profit", row.Predicted);
            Assert.Equal(1.0, row.Confidence);
            Assert.Equal(ExactMatchReason, row.Reason);
        }
    }

    // ── Защита от перекоррекции ──────────────────────────────────────────────
    // Проверка восьмая. Настоящий сбой исполнения обязан остаться сбоем. Сбой
    // вызывается через настоящую рабочую поверхность, без подмен и без новых
    // зависимостей: признак отмены передаётся уже отменённым, и тогда память
    // формул отвечает отказом с кодом «отменено», а не кодом «формулы нет в
    // памяти». Если бы починка засчитывала правильным любой отказ вообще, эта
    // проверка покраснела бы — в том числе на плече закрытых формул, где
    // ожидание «не знает» совпало бы со сбоем.
    [Fact]
    public void RealExecutionFailure_StaysExecutionFailed()
    {
        var everyRow = _panel.Cancelled.TrainingResults
            .Concat(_panel.Cancelled.HeldOutResults)
            .Concat(_panel.Cancelled.NormalisationVariantResults)
            .ToList();

        // Двадцать обучающих формул, десять закрытых и четыре варианта записи.
        Assert.Equal(34, everyRow.Count);

        // Ни одна строка не имеет права считаться правильной.
        Assert.Equal(0, everyRow.Count(row => row.Correct));

        // И у каждой строки причина — именно провал исполнения.
        Assert.Equal(
            34,
            everyRow.Count(row => string.Equals(
                row.Reason,
                ExecutionFailedReason,
                StringComparison.Ordinal)));

        Assert.False(
            _panel.Cancelled.Success,
            "Прогон, в котором ни одно обращение к памяти не состоялось, не " +
            "имеет права отчитаться успехом.");
    }
}
