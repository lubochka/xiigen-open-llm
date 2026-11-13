// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Проверки производных чисел точности опыта P10-PRIMARY-01, подслучай P10.1.
//
// Закрываемый дефект. Выводимые числа точности — наибольшая погрешность и во
// сколько раз она меньше допуска — считались вручную при написании отчёта, и
// ни одна строка кода не сверяла заявленную границу с измеренной. Поэтому в
// отчёт попало «с запасом не менее четырёх раз», хотя запас по абсолютному
// допуску равен 3,713, и «не хуже 3,6e-4» для полусуммы запасов, хотя
// измеренная равна 3,6132e-4. Обе границы заявлены в выгодную сторону.
//
// Второй закрываемый дефект. Единственный общий вердикт сохранённого файла
// назывался «каждая координата согласовалась хотя бы на одном шаге». Он
// слабее утверждения отчёта «все шесть сравнений прошли»: координата могла
// разойтись на одном шаге и всё равно попасть в общий «да».
//
// Числа опорного случая взяты из уже сохранённой попытки
// experiments-v2/RP01-MICRO/P1001/results/attempt-0001/derivative-comparison.json.
// Ни одного обращения к модели, ни одного обновления весов здесь нет. Проверки
// вызывают ДЕЙСТВУЮЩИЙ построитель, который компилируется и в хост опыта, и сюда.

namespace LlmFromScratch.Tests;

using P1001Experiment;

public sealed class P1001DerivativeAccuracySummaryTests
{
    /// <summary>Предельная абсолютная погрешность согласия, закреплённая паспортом опыта.</summary>
    private const double MaxAbsoluteTolerance = 1e-4;

    /// <summary>Предельная относительная погрешность согласия, закреплённая паспортом опыта.</summary>
    private const double MaxRelativeTolerance = 1e-2;

    /// <summary>Порог, ниже которого конечная разность считается околонулевой.</summary>
    private const double NearZeroThreshold = 1e-6;

    /// <summary>Заявление первичного отчёта: запас по абсолютному допуску «не менее четырёх раз».</summary>
    private const double StatedAbsoluteMarginLowerBound = 4d;

    /// <summary>Заявление первичного отчёта: наибольшая относительная погрешность «4,1e-4».</summary>
    private const double StatedRelativeErrorUpperBound = 4.1e-4;

    /// <summary>Заявление первичного отчёта: полусумма запасов «не хуже 3,6e-4».</summary>
    private const double StatedMarginSymmetryUpperBound = 3.6e-4;

    /// <summary>
    /// Опорный случай: все шесть сохранённых сравнений проходят по отдельности, а
    /// производные числа совпадают с пересчётом по сохранённому файлу.
    /// </summary>
    [Fact]
    public void SavedRunDerivedFiguresMatchTheSavedComparisons()
    {
        var figures = Build(SavedComparisons()); // Действующий построитель на сохранённых числах.

        Assert.Equal(6, figures.ComparisonCount); // Две координаты на три значения шага возмущения.
        Assert.Equal(6, figures.AgreedComparisonCount); // Каждое сравнение уложилось в допуск по отдельности.
        Assert.True(figures.AllComparisonsAgreed); // Результат опыта не меняется: исправлены выводимые числа.
        Assert.Equal(2.6932268261364856E-05, figures.MaxAbsoluteError); // Худшая абсолютная погрешность из сохранённого файла.
        Assert.Equal("v_proj[0] шаг 1.0E-003", figures.MaxAbsoluteErrorRow); // Худшим оказалось сравнение проекции значения на самом мелком шаге.
        Assert.Equal(0.00040615593886819115, figures.MaxRelativeError); // Худшая относительная погрешность из сохранённого файла.
        Assert.Equal(0.0003613174194470048, figures.MaxAbsoluteMarginSymmetry); // Худшая полусумма запасов из сохранённого файла.
        Assert.Equal(3.7130181175067603, figures.AbsoluteToleranceMarginFactor, 10); // Запас по абсолютному допуску: 3,713, а не четыре.
        Assert.Equal(24.62108526066703, figures.RelativeToleranceMarginFactor, 10); // Запас по относительному допуску — отдельная величина.
        Assert.Equal(0, figures.NearZeroComparisonCount); // Ни одна конечная разность не оказалась околонулевой.
        Assert.False(figures.NearZeroRuleApplied); // Особое правило околонулевой величины не применялось.
    }

    /// <summary>
    /// Главный закрываемый дефект: заявленная отчётом нижняя граница запаса в четыре
    /// раза выше измеренных 3,713 и обязана быть отвергнута.
    /// </summary>
    [Fact]
    public void PublishedClaimOfFourfoldAbsoluteMarginDoesNotHold()
    {
        var figures = Build(SavedComparisons()); // Измеренный запас по сохранённым числам.

        var claim = P1001DerivativeAccuracySummary.CheckLowerBound(
            "отчёт: запас не менее четырёх раз", StatedAbsoluteMarginLowerBound, figures.AbsoluteToleranceMarginFactor);

        Assert.False(claim.Holds); // Заявление завышено: 4 больше измеренных 3,713.
        Assert.Equal("нижняя", claim.Direction); // Это именно нижняя граница запаса.
        Assert.Equal(4d, claim.Stated); // Что было заявлено человеческим текстом.
        Assert.Equal(3.7130181175067603, claim.Measured, 10); // Что измерено по сохранённым сравнениям.
        Assert.Contains("ВЫШЕ измеренной", claim.Human, StringComparison.Ordinal); // Человеческое объяснение называет направление ошибки.
    }

    /// <summary>
    /// Второй закрываемый дефект того же класса: заявленная верхняя граница полусуммы
    /// запасов ниже измеренной и обязана быть отвергнута.
    /// </summary>
    [Fact]
    public void PublishedClaimOfMarginSymmetryUpperBoundDoesNotHold()
    {
        var figures = Build(SavedComparisons()); // Измеренная полусумма запасов по сохранённым числам.

        var claim = P1001DerivativeAccuracySummary.CheckUpperBound(
            "отчёт: полусумма запасов не хуже 3,6e-4", StatedMarginSymmetryUpperBound, figures.MaxAbsoluteMarginSymmetry);

        Assert.False(claim.Holds); // Заявление занижено: 3,6e-4 меньше измеренных 3,6132e-4.
        Assert.Equal("верхняя", claim.Direction); // Это именно верхняя граница погрешности.
        Assert.Equal(0.0003613174194470048, claim.Measured); // Измеренное значение из сохранённого файла.
        Assert.Contains("НИЖЕ измеренной", claim.Human, StringComparison.Ordinal); // Человеческое объяснение называет направление ошибки.
    }

    /// <summary>
    /// Различающая проверка: третье заявление отчёта верно и обязано быть принято.
    /// Без неё сверка границ отвергала бы всё подряд и ничего не доказывала.
    /// </summary>
    [Fact]
    public void PublishedClaimOfRelativeErrorUpperBoundHolds()
    {
        var figures = Build(SavedComparisons()); // Измеренная относительная погрешность.

        var claim = P1001DerivativeAccuracySummary.CheckUpperBound(
            "отчёт: наибольшая относительная погрешность 4,1e-4", StatedRelativeErrorUpperBound, figures.MaxRelativeError);

        Assert.True(claim.Holds); // Заявленная граница 4,1e-4 не ниже измеренных 4,0616e-4.
        Assert.Contains("не ниже измеренной", claim.Human, StringComparison.Ordinal); // Человеческое объяснение подтверждает принятие.
    }

    /// <summary>
    /// Запас считается по худшему сравнению. По среднему он получился бы заметно
    /// больше четырёх, и заявление отчёта выглядело бы верным.
    /// </summary>
    [Fact]
    public void MarginIsComputedFromTheWorstComparisonNotFromTheAverage()
    {
        var saved = SavedComparisons(); // Сохранённые сравнения попытки.
        var figures = Build(saved); // Производные числа по ним.

        var meanAbsolute = saved.Average(row => row.AbsoluteError); // Среднее значение погрешности.
        var marginFromMean = MaxAbsoluteTolerance / meanAbsolute; // Запас, который дало бы среднее.

        Assert.Equal(saved.Max(row => row.AbsoluteError), figures.MaxAbsoluteError); // Взято именно худшее сравнение.
        Assert.Equal(8.619528526716854, marginFromMean, 10); // По среднему запас получился бы 8,62.
        Assert.True(marginFromMean > StatedAbsoluteMarginLowerBound); // По среднему заявление «не менее четырёх» выглядело бы верным.
        Assert.True(figures.AbsoluteToleranceMarginFactor < StatedAbsoluteMarginLowerBound); // По худшему оно неверно.
    }

    /// <summary>
    /// Запас по абсолютному и по относительному допуску — две разные оси. Запас по
    /// относительной оси больше четырёх и не подтверждает заявление об абсолютной.
    /// </summary>
    [Fact]
    public void AbsoluteAndRelativeMarginsAreSeparateAxes()
    {
        var figures = Build(SavedComparisons()); // Оба запаса по сохранённым числам.

        Assert.True(figures.AbsoluteToleranceMarginFactor < StatedAbsoluteMarginLowerBound); // Абсолютная ось: 3,713 меньше четырёх.
        Assert.True(figures.RelativeToleranceMarginFactor > StatedAbsoluteMarginLowerBound); // Относительная ось: 24,6 больше четырёх.
        Assert.NotEqual(figures.AbsoluteToleranceMarginFactor, figures.RelativeToleranceMarginFactor, 6); // Подменять одну ось другой нельзя.
    }

    /// <summary>
    /// Прежний более слабый вердикт сохранённого файла — «каждая координата
    /// согласовалась хотя бы на одном шаге» — не равен согласию всех сравнений.
    /// </summary>
    [Fact]
    public void CoordinateAgreementOnSomeEpsilonIsNotAgreementOfEveryComparison()
    {
        var saved = SavedComparisons(); // Сохранённые сравнения попытки.
        var withOneDivergence = saved.ToArray(); // Копия для синтетического расхождения.
        withOneDivergence[2] = Diverge(withOneDivergence[2]); // Проекция запроса расходится на самом крупном шаге.

        var figures = Build(withOneDivergence); // Тот же построитель на синтетическом наборе.

        Assert.True(figures.EveryCoordinateAgreedOnSomeEpsilon); // Прежний вердикт всё ещё говорит «да».
        Assert.False(figures.AllComparisonsAgreed); // Согласие по каждому сравнению отдельно говорит «нет».
        Assert.Equal(5, figures.AgreedComparisonCount); // Пять сравнений из шести уложились в допуск.
        Assert.Equal(6, figures.ComparisonCount); // Ни одно сравнение не потеряно из счёта.
    }

    /// <summary>
    /// Потеря худшего сравнения обязана быть видна в счёте, а не прятаться за
    /// выросшим запасом: иначе из пяти строк можно заявить результат шести.
    /// </summary>
    [Fact]
    public void DroppingTheWorstComparisonIsVisibleInTheCountAndTheMargin()
    {
        var saved = SavedComparisons(); // Сохранённые сравнения попытки.
        var full = Build(saved); // Производные числа по полному набору.
        var withoutWorst = saved.Where(row => row.RowId != full.MaxAbsoluteErrorRow).ToArray(); // Набор без худшего сравнения.

        var shortened = Build(withoutWorst); // Производные числа по укороченному набору.

        Assert.Equal(5, shortened.ComparisonCount); // Счёт сравнений уменьшился и это видно.
        Assert.True(shortened.AbsoluteToleranceMarginFactor > full.AbsoluteToleranceMarginFactor); // Без худшей строки запас растёт.
        Assert.Equal(3.911021357916917, shortened.AbsoluteToleranceMarginFactor, 10); // Он становится 3,911 вместо 3,713.
        Assert.True(shortened.AbsoluteToleranceMarginFactor < StatedAbsoluteMarginLowerBound); // Даже после потери худшей строки заявление «не менее четырёх» остаётся неверным.
    }

    /// <summary>Подправленная вручную сохранённая абсолютная погрешность отвергается.</summary>
    [Fact]
    public void TamperedSavedAbsoluteErrorIsRefused()
    {
        var saved = SavedComparisons(); // Сохранённые сравнения попытки.
        saved[0] = saved[0] with { AbsoluteError = saved[0].AbsoluteError / 10d }; // Погрешность занижена в десять раз.

        var error = Assert.Throws<InvalidOperationException>(() => Build(saved)); // Построитель обязан отказать.

        Assert.StartsWith("P1001_DERIVED_ABSOLUTE_ERROR_MISMATCH", error.Message, StringComparison.Ordinal); // Отказ назван точно.
    }

    /// <summary>Подправленная вручную сохранённая относительная погрешность отвергается.</summary>
    [Fact]
    public void TamperedSavedRelativeErrorIsRefused()
    {
        var saved = SavedComparisons(); // Сохранённые сравнения попытки.
        saved[3] = saved[3] with { RelativeError = saved[3].RelativeError / 2d }; // Погрешность занижена вдвое.

        var error = Assert.Throws<InvalidOperationException>(() => Build(saved)); // Построитель обязан отказать.

        Assert.StartsWith("P1001_DERIVED_RELATIVE_ERROR_MISMATCH", error.Message, StringComparison.Ordinal); // Отказ назван точно.
    }

    /// <summary>Признак околонулевой величины, выставленный мимо порога, отвергается.</summary>
    [Fact]
    public void NearZeroFlagThatDisagreesWithTheThresholdIsRefused()
    {
        var saved = SavedComparisons(); // Сохранённые сравнения попытки.
        saved[5] = saved[5] with { NearZeroFiniteDifference = true }; // Признак выставлен вручную при неоколонулевой разности.

        var error = Assert.Throws<InvalidOperationException>(() => Build(saved)); // Построитель обязан отказать.

        Assert.StartsWith("P1001_DERIVED_NEAR_ZERO_FLAG_MISMATCH", error.Message, StringComparison.Ordinal); // Отказ назван точно.
    }

    /// <summary>Пустой набор сравнений не превращается в нулевую погрешность.</summary>
    [Fact]
    public void EmptyComparisonListIsRefused()
    {
        var error = Assert.Throws<InvalidOperationException>(() => Build([])); // Пустой набор обязан быть отвергнут.

        Assert.Equal("P1001_DERIVED_NO_COMPARISONS", error.Message); // Отказ назван точно.
    }

    /// <summary>Повтор одного сравнения отвергается: он исказил бы и счёт, и худшее значение.</summary>
    [Fact]
    public void DuplicateComparisonIsRefused()
    {
        var saved = SavedComparisons(); // Сохранённые сравнения попытки.
        saved[4] = saved[3]; // Одно сравнение повторено дважды.

        var error = Assert.Throws<InvalidOperationException>(() => Build(saved)); // Построитель обязан отказать.

        Assert.StartsWith("P1001_DERIVED_DUPLICATE_COMPARISON", error.Message, StringComparison.Ordinal); // Отказ назван точно.
    }

    /// <summary>Нечисловое поле не превращается в ноль.</summary>
    [Fact]
    public void NonFiniteFieldIsRefused()
    {
        var saved = SavedComparisons(); // Сохранённые сравнения попытки.
        saved[1] = saved[1] with { MarginSymmetry = double.NaN }; // Поле стало нечисловым.

        var error = Assert.Throws<InvalidOperationException>(() => Build(saved)); // Построитель обязан отказать.

        Assert.StartsWith("P1001_DERIVED_NON_FINITE", error.Message, StringComparison.Ordinal); // Отказ назван точно.
    }

    /// <summary>Неположительный допуск отвергается до любого вывода чисел.</summary>
    [Fact]
    public void NonPositiveToleranceIsRefused()
    {
        var error = Assert.Throws<InvalidOperationException>(() =>
            P1001DerivativeAccuracySummary.Build(SavedComparisons(), 0d, MaxRelativeTolerance, NearZeroThreshold)); // Нулевой допуск бессмыслен.

        Assert.Equal("P1001_DERIVED_TOLERANCE_NOT_POSITIVE", error.Message); // Отказ назван точно.
    }

    /// <summary>Вызов действующего построителя с допусками паспорта опыта.</summary>
    private static P1001DerivativeAccuracyFigures Build(IReadOnlyList<P1001DerivativeComparison> comparisons)
        => P1001DerivativeAccuracySummary.Build(comparisons, MaxAbsoluteTolerance, MaxRelativeTolerance, NearZeroThreshold);

    /// <summary>Увести сравнение за оба допуска, сохранив согласованность его полей.</summary>
    private static P1001DerivativeComparison Diverge(P1001DerivativeComparison row)
    {
        var shifted = row.AnalyticDerivative * 0.5d; // Конечная разность уводится вдвое от аналитической производной.
        var absolute = Math.Abs(row.AnalyticDerivative - shifted); // Абсолютная погрешность пересчитана честно.
        var scale = Math.Max(Math.Abs(row.AnalyticDerivative), Math.Abs(shifted)); // Масштаб относительной погрешности.
        return row with
        {
            FiniteDifference = shifted, // Новая конечная разность.
            AbsoluteError = absolute, // Согласованная абсолютная погрешность.
            RelativeError = scale > 0d ? absolute / scale : 0d, // Согласованная относительная погрешность.
            NearZeroFiniteDifference = Math.Abs(shifted) < NearZeroThreshold // Признак пересчитан по тому же порогу.
        };
    }

    /// <summary>
    /// Шесть сравнений из сохранённого файла попытки
    /// experiments-v2/RP01-MICRO/P1001/results/attempt-0001/derivative-comparison.json.
    /// Числа перенесены без изменения; обращений к модели здесь нет.
    /// </summary>
    private static P1001DerivativeComparison[] SavedComparisons() =>
    [
        new()
        {
            Coordinate = "q_proj", BindingKey = "lora.block_0000.q_proj.lora_b", Index = 0, Epsilon = 0.001,
            AnalyticDerivative = -0.16927593784053738, FiniteDifference = -0.16927719116210938,
            AbsoluteError = 1.2533215719978674E-06, RelativeError = 7.403960116502737E-06,
            NearZeroFiniteDifference = false, MarginSymmetry = -3.5107077565044165E-06
        },
        new()
        {
            Coordinate = "q_proj", BindingKey = "lora.block_0000.q_proj.lora_b", Index = 0, Epsilon = 0.003,
            AnalyticDerivative = -0.16927593784053738, FiniteDifference = -0.16926725705464682,
            AbsoluteError = 8.680785890557985E-06, RelativeError = 5.128186558171974E-05,
            NearZeroFiniteDifference = false, MarginSymmetry = -3.258587094023824E-05
        },
        new()
        {
            Coordinate = "q_proj", BindingKey = "lora.block_0000.q_proj.lora_b", Index = 0, Epsilon = 0.01,
            AnalyticDerivative = -0.16927593784053738, FiniteDifference = -0.16925036907196045,
            AbsoluteError = 2.5568768576927914E-05, RelativeError = 0.00015104786246119872,
            NearZeroFiniteDifference = false, MarginSymmetry = -0.0003613174194470048
        },
        new()
        {
            Coordinate = "v_proj", BindingKey = "lora.block_0000.v_proj.lora_b", Index = 0, Epsilon = 0.001,
            AnalyticDerivative = -0.0662832350443607, FiniteDifference = -0.06631016731262207,
            AbsoluteError = 2.6932268261364856E-05, RelativeError = 0.00040615593886819115,
            NearZeroFiniteDifference = false, MarginSymmetry = -4.053144948557019E-07
        },
        new()
        {
            Coordinate = "v_proj", BindingKey = "lora.block_0000.v_proj.lora_b", Index = 0, Epsilon = 0.003,
            AnalyticDerivative = -0.0662832350443607, FiniteDifference = -0.06629029909769694,
            AbsoluteError = 7.064053336239273E-06, RelativeError = 0.00010656239951230952,
            NearZeroFiniteDifference = false, MarginSymmetry = -3.26038571074605E-06
        },
        new()
        {
            Coordinate = "v_proj", BindingKey = "lora.block_0000.v_proj.lora_b", Index = 0, Epsilon = 0.01,
            AnalyticDerivative = -0.0662832350443607, FiniteDifference = -0.06628334522247314,
            AbsoluteError = 1.1017811243907438E-07, RelativeError = 1.6622292080955333E-06,
            NearZeroFiniteDifference = false, MarginSymmetry = -3.6221754271537066E-05
        }
    ];
}
