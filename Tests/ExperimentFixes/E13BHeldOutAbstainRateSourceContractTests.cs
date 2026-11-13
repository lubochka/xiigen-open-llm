// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное покрытие договора «доля воздержания по закрытым формулам считается
// по признаку воздержания, а не как единица минус доля совпадений» для проверки
// опыта на закрытых формулах
// (файл 35.ExperimentsV2/Experiments/E13BHeldOutValidation.cs).
//
// Что здесь проверяется по-человечески. В проекте есть маленькая обучаемая
// память формул: её учат целыми записями вида «Выручка минус Затраты равно
// Прибыль», а потом по левой части она возвращает правую. Опыт на закрытых
// формулах измеряет два разных умения сразу: выученные формулы обязаны отвечать
// точно, а невыученные — честно промолчать. Это молчание опыт называет
// воздержанием и считает его долю.
//
// Что было сломано. Доля воздержания считалась как «единица минус доля
// совпадений», то есть по полю совпадения. Сорванное обращение к памяти — тоже
// не совпадение, поэтому прогон, в котором ни одно обращение не состоялось,
// показывал долю воздержания единицу: сводка докладывала «сто процентов
// честного молчания» о прогоне, где не промолчал никто. Поля правильности и
// причины в этом плече были разведены верно, но в общий исход не впадали — доля
// правильных по плечу закрытых формул не читалась вообще.
//
// Чем это ловится. Заготовка прогонов соседнего класса проверок делает ровно два
// прогона опыта: обычный и с заранее отменённым признаком отмены. Во втором
// каждое обращение к памяти получает отказ с кодом «отменено» — это настоящий
// сбой исполнения, а не воздержание. Значит, доля воздержания во втором прогоне
// обязана быть нулём, а не единицей.
//
// Бюджет. Заготовка прогонов создаётся для этого класса проверок отдельно от
// соседнего класса: два обучения по двадцать уроков и шестьдесят восемь
// обращений к памяти формул. Ни одного обращения к диску, ни одной внешней
// модели, ни одной пары предпочтений.
//
// Чего это покрытие НЕ доказывает. Оно не доказывает, что память формул умеет
// выводить новые формулы, и не измеряет качество обучения. Оно также не
// доказывает поведение общего исхода в прогоне, где сорвалось ТОЛЬКО плечо
// закрытых формул: через настоящую рабочую поверхность признак отмены отменяет
// сразу все три плеча, а подменять поверхность заглушкой наряд запрещает. Для
// этого одного случая доказательством остаётся чтение правки продукта.

using ExperimentsV2;

namespace LlmFromScratch.Tests.ExperimentFixes;

public sealed class E13BHeldOutAbstainRateSourceContractTests
    : IClassFixture<E13BAbstentionPanel>
{
    // Причина в записи: формула в уроках не встречалась. Это и есть признак
    // воздержания, по которому обязана считаться его доля.
    private const string NotSeenReason = "expression_not_seen";

    // Причина в записи: обращение к памяти сорвалось. Это НЕ воздержание.
    private const string ExecutionFailedReason = "execution_failed";

    // Имя поля сводки, которое обязано нести долю правильных по плечу закрытых
    // формул. Поле читается по имени, а не через точку: до правки продукта
    // такого поля в сводке нет вовсе, а красный прогон обязан собираться и
    // давать числа, а не ошибку сборки.
    private const string HeldOutCorrectRateName = "HeldOutCorrectRate";

    private readonly E13BAbstentionPanel _panel;

    public E13BHeldOutAbstainRateSourceContractTests(E13BAbstentionPanel panel)
        => _panel = panel;

    // ── Главная проверка починки ─────────────────────────────────────────────
    // Проверка первая. Прогон, в котором ни одно обращение к памяти не
    // состоялось: по закрытым формулам не промолчал никто, поэтому доля
    // воздержания обязана быть нулём, а общий исход опыта — неуспехом.
    [Fact]
    public void CancelledRun_HeldOutAbstainRateIsZero_AndOutcomeIsNotSuccess()
    {
        // Размер плеча выписан буквально: десять закрытых формул опыта.
        Assert.Equal(10, _panel.Cancelled.HeldOutResults.Count);

        Assert.Equal(0.0, _panel.Cancelled.HeldOutAbstainRate);

        Assert.False(
            _panel.Cancelled.Success,
            "Прогон, в котором ни одно обращение к памяти не состоялось, не " +
            "имеет права отчитаться успехом.");
    }

    // Проверка вторая. Опора главной проверки: в том же прогоне сами строки
    // плеча закрытых формул уже говорят правду — ни одного воздержания, десять
    // сбоев исполнения, ни одной правильной строки. Доля воздержания обязана
    // читаться именно из этих строк, а не спорить с ними.
    [Fact]
    public void CancelledRun_HeldOutRowsSayExecutionFailed_AndNoneAbstained()
    {
        var rows = _panel.Cancelled.HeldOutResults;

        Assert.Equal(10, rows.Count);

        Assert.Equal(
            0,
            rows.Count(row => string.Equals(row.Reason, NotSeenReason, StringComparison.Ordinal)));

        Assert.Equal(
            10,
            rows.Count(row => string.Equals(row.Reason, ExecutionFailedReason, StringComparison.Ordinal)));

        Assert.Equal(0, rows.Count(row => row.Correct));
    }

    // Проверка третья. Сам договор подсчёта, на обоих прогонах сразу: доля
    // воздержания равна доле строк с причиной «формула не встречалась». В
    // обычном прогоне это десять из десяти, в прогоне с отменой — ноль из
    // десяти. Подсчёт «единица минус доля совпадений» даёт в обычном прогоне то
    // же число и расходится только на сорванном, поэтому проверка смотрит оба.
    [Fact]
    public void HeldOutAbstainRate_EqualsShareOfRowsWithNotSeenReason_InBothRuns()
    {
        AssertAbstainRateIsCountedFromRows(_panel.Normal, "обычный прогон");

        AssertAbstainRateIsCountedFromRows(
            _panel.Cancelled,
            "прогон с заранее отменённым признаком отмены");
    }

    // Проверка четвёртая. Сводка опыта обязана нести долю правильных по плечу
    // закрытых формул: в обычном прогоне это единица, в прогоне с отменой —
    // ноль. Без этого поля общему исходу нечего требовать от правильности
    // закрытого плеча.
    [Fact]
    public void Summary_CarriesHeldOutCorrectRate_OneWhenNormal_ZeroWhenCancelled()
    {
        Assert.Equal(1.0, HeldOutCorrectRate(_panel.Normal));

        Assert.Equal(0.0, HeldOutCorrectRate(_panel.Cancelled));
    }

    // Проверка пятая. То же поле не имеет права быть постоянным числом: оно
    // обязано считаться по строкам плеча закрытых формул на обоих прогонах.
    [Fact]
    public void HeldOutCorrectRate_EqualsShareOfCorrectRows_InBothRuns()
    {
        AssertHeldOutCorrectRateIsCountedFromRows(_panel.Normal, "обычный прогон");

        AssertHeldOutCorrectRateIsCountedFromRows(
            _panel.Cancelled,
            "прогон с заранее отменённым признаком отмены");
    }

    // ── Защита от перекоррекции ──────────────────────────────────────────────
    // Проверка шестая. Обычный прогон не имеет права пострадать от починки: все
    // десять закрытых формул честно молчат, ложных срабатываний нет, общий исход
    // успешен.
    [Fact]
    public void NormalRun_HeldOutAbstainRateIsOne_AndOutcomeIsSuccess()
    {
        Assert.Equal(1.0, _panel.Normal.HeldOutAbstainRate);

        Assert.Equal(0.0, _panel.Normal.FalsePositiveRate);

        Assert.True(
            _panel.Normal.Success,
            "Выученные формулы отвечают точно, закрытые формулы честно молчат, " +
            "варианты записи разобраны верно — общий исход опыта обязан быть " +
            "успешным.");
    }

    // Проверка седьмая. Доля ложных срабатываний остаётся при своём источнике —
    // поле совпадения. Это её правильный источник: ложное срабатывание — это
    // ответ памяти на формулу, которой она не училась. Если бы починка перевела
    // и эту долю на признак воздержания, в прогоне с отменой она стала бы
    // единицей, и эта проверка покраснела бы.
    [Fact]
    public void FalsePositiveRate_StaysShareOfMatchedRows_InBothRuns()
    {
        AssertFalsePositiveRateIsCountedFromMatchedRows(_panel.Normal, "обычный прогон");

        AssertFalsePositiveRateIsCountedFromMatchedRows(
            _panel.Cancelled,
            "прогон с заранее отменённым признаком отмены");
    }

    // ── Общие утверждения проверок ───────────────────────────────────────────
    // Доля воздержания обязана совпадать с долей строк, у которых причина —
    // «формула не встречалась».
    private static void AssertAbstainRateIsCountedFromRows(
        E13BHeldOutValidationResult summary,
        string runName)
    {
        var abstained = summary.HeldOutResults
            .Count(row => string.Equals(row.Reason, NotSeenReason, StringComparison.Ordinal));

        var expected = abstained / (double)summary.HeldOutResults.Count;

        Assert.True(
            summary.HeldOutAbstainRate == expected,
            $"{runName}: доля воздержания обязана считаться по признаку воздержания. " +
            $"Строк с причиной «формула не встречалась» {abstained} из " +
            $"{summary.HeldOutResults.Count}, значит ожидается {expected}, а сводка " +
            $"сообщает {summary.HeldOutAbstainRate}.");
    }

    // Доля правильных по плечу закрытых формул обязана совпадать с долей строк,
    // отмеченных правильными.
    private static void AssertHeldOutCorrectRateIsCountedFromRows(
        E13BHeldOutValidationResult summary,
        string runName)
    {
        var correct = summary.HeldOutResults.Count(row => row.Correct);

        var expected = correct / (double)summary.HeldOutResults.Count;

        var actual = HeldOutCorrectRate(summary);

        Assert.True(
            actual == expected,
            $"{runName}: доля правильных по плечу закрытых формул обязана считаться " +
            $"по строкам. Правильных строк {correct} из {summary.HeldOutResults.Count}, " +
            $"значит ожидается {expected}, а сводка сообщает {actual}.");
    }

    // Доля ложных срабатываний обязана совпадать с долей строк, где память
    // формул всё-таки ответила.
    private static void AssertFalsePositiveRateIsCountedFromMatchedRows(
        E13BHeldOutValidationResult summary,
        string runName)
    {
        var matched = summary.HeldOutResults.Count(row => row.Matched);

        var expected = matched / (double)summary.HeldOutResults.Count;

        Assert.True(
            summary.FalsePositiveRate == expected,
            $"{runName}: доля ложных срабатываний обязана считаться по полю " +
            $"совпадения. Ответивших строк {matched} из " +
            $"{summary.HeldOutResults.Count}, значит ожидается {expected}, а сводка " +
            $"сообщает {summary.FalsePositiveRate}.");
    }

    // Чтение доли правильных по плечу закрытых формул из сводки по имени поля.
    // До правки продукта такого поля нет, и тогда красным становится именно это
    // утверждение, а не сборка проекта проверок.
    private static double HeldOutCorrectRate(E13BHeldOutValidationResult summary)
    {
        var property = typeof(E13BHeldOutValidationResult).GetProperty(HeldOutCorrectRateName);

        Assert.NotNull(property);

        Assert.Equal(typeof(double), property!.PropertyType);

        return (double)property.GetValue(summary)!;
    }
}
