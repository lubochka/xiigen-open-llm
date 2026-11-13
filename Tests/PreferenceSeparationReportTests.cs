// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Проверки разделения трёх разных величин, которые раньше в отчётах опыта
// назывались одним словом «точность».
//
// Все числа берутся из уже сохранённого показания опыта P02-PRIMARY-01: ни одного
// обращения к модели, ни одного обновления весов. Проверки вызывают действующий
// построитель отчёта, который компилируется и в хост опыта, и сюда; собственной
// копии формулы в тесте нет.

namespace LlmFromScratch.Tests;

using System.Text.Json;
using P0201Experiment;

public sealed class PreferenceSeparationReportTests
{
    /// <summary>Отвергаемые стороны двух учебных пар опыта, как их задаёт сам опыт.</summary>
    private static readonly Dictionary<string, string> RejectedSideByRowId =
        new(StringComparer.Ordinal) { ["T1"] = "-", ["T2"] = "+" };

    /// <summary>
    /// Отрицательный контроль из задания: во второй учебной паре после плеча A
    /// политика всё ещё ставит неверный знак выше верного, а относительно эталона
    /// всё равно сдвинулась в нужную сторону. Три величины обязаны разойтись.
    /// </summary>
    [Fact]
    public void SecondTrainingPairAfterFirstArmSeparatesImprovementPreferenceAndCorrectAnswer()
    {
        var recorded = LoadRecordedArm("arm_a");

        var separation = PreferenceSeparationReport.Build(
            recorded.Reference,
            recorded.Policy,
            RejectedSideByRowId,
            recorded.StepRewardMargins);

        var secondPair = separation.Rows.Single(row => row.RowId == "T2");

        // Записанная разность chosen - rejected второй пары после плеча A.
        Assert.Equal(-0.1827326d, secondPair.PolicyLogProbabilityDifference, 7);
        // Относительно эталона сдвиг положителен: первая величина истинна.
        Assert.True(secondPair.PanelPairReferenceRelativeImprovement);
        // Сама политика предпочитает неверный знак: вторая величина ложна.
        Assert.False(secondPair.PolicyPreferenceWin);
        // Порождённый ответ неверен: третья величина ложна.
        Assert.False(secondPair.GeneratedAnswerCorrect);
        // Прямая проверка против прежней подмены понятий: эти два признака не равны.
        Assert.NotEqual(secondPair.PanelPairReferenceRelativeImprovement, secondPair.PolicyPreferenceWin);
        // И правильность ответа тоже не повторяет улучшение относительно эталона.
        Assert.NotEqual(secondPair.PanelPairReferenceRelativeImprovement, secondPair.GeneratedAnswerCorrect);
        // Поле ответа переносит порождённую строку как есть; что оно не выведено из
        // логарифмических вероятностей, доказывает отдельная проверка расхождения ниже.
        Assert.Equal("+", secondPair.ProducedAnswer);
        Assert.Equal("-", secondPair.GoldAnswer);
    }

    /// <summary>Положительная пара имеет собственную разность сторон больше нуля.</summary>
    [Fact]
    public void FirstTrainingPairAfterFirstArmKeepsItsOwnPositiveSideDifference()
    {
        var recorded = LoadRecordedArm("arm_a");

        var separation = PreferenceSeparationReport.Build(
            recorded.Reference,
            recorded.Policy,
            RejectedSideByRowId,
            recorded.StepRewardMargins);

        var firstPair = separation.Rows.Single(row => row.RowId == "T1");

        // Записанная разность chosen - rejected первой пары после плеча A.
        Assert.Equal(1.8790083d, firstPair.PolicyLogProbabilityDifference, 6);
        // Здесь все три признака совпадают по знаку, и это не делает их одной величиной.
        Assert.True(firstPair.PanelPairReferenceRelativeImprovement);
        Assert.True(firstPair.PolicyPreferenceWin);
        Assert.True(firstPair.GeneratedAnswerCorrect);
    }

    /// <summary>
    /// Защита третьей величины — правильности ответа — от подмены второй величиной,
    /// выбором стороны пары. На записанных строках опыта эти два признака совпадают
    /// во всех шестнадцати наблюдениях (два плеча × четыре строки панели × до и после),
    /// поэтому реализация, которая вместо сравнения выданной строки с верной считала бы
    /// знак разности сторон, прошла бы все остальные проверки незамеченной. Здесь панель
    /// построена вручную именно так, чтобы признаки разошлись, причём в ОБЕ стороны:
    /// одна строка отвечена верно при проигранной стороне, другая отвечена неверно при
    /// выигранной. Оба случая стоят в одной проверке, потому что они ловят одну и ту же
    /// одиночную подмену с двух сторон и работают на одной и той же построенной панели.
    /// Считает эти наблюдения действующий построитель: своей копии формулы в проверке нет,
    /// обращений к модели и обновлений весов нет.
    /// </summary>
    [Fact]
    public void CorrectAnswerAndPreferenceWinDivergeInBothDirectionsOnConstructedRows()
    {
        // Снятие эталона: обе строки нужны только как база сравнения для первой величины.
        var reference = new[]
        {
            new PanelObservation { RowId = "D1", Gold = "+", Produced = "+", TrainingRow = true, LogProbabilityOfPlus = -2.0d, LogProbabilityOfMinus = -0.4d }, // У эталона верный знак ещё ниже неверного.
            new PanelObservation { RowId = "D2", Gold = "-", Produced = "-", TrainingRow = true, LogProbabilityOfPlus = -0.5d, LogProbabilityOfMinus = -0.9d } // У эталона верный знак тоже ниже неверного.
        };
        // Снятие политики: именно здесь признаки нарочно разведены в разные стороны.
        var policy = new[]
        {
            new PanelObservation { RowId = "D1", Gold = "+", Produced = "+", TrainingRow = true, LogProbabilityOfPlus = -1.2d, LogProbabilityOfMinus = -0.4d }, // Ответ верен, а сторона проиграна.
            new PanelObservation { RowId = "D2", Gold = "-", Produced = "+", TrainingRow = true, LogProbabilityOfPlus = -1.1d, LogProbabilityOfMinus = -0.3d } // Ответ неверен, а сторона выиграна.
        };
        // Отвергаемые стороны обеих построенных пар объявлены явно.
        var rejectedSide = new Dictionary<string, string>(StringComparer.Ordinal) { ["D1"] = "-", ["D2"] = "+" };

        // Квитанций шагов обучателя у построенной панели нет: проверка касается построчных признаков.
        var separation = PreferenceSeparationReport.Build(reference, policy, rejectedSide, Array.Empty<double>());

        var correctButLosingSide = separation.Rows.Single(row => row.RowId == "D1");
        // Разность верной и неверной стороны отрицательна: правильную сторону политика не выбрала.
        Assert.Equal(-0.8d, correctButLosingSide.PolicyLogProbabilityDifference, 10);
        Assert.False(correctButLosingSide.PolicyPreferenceWin);
        // Но выданная строка совпала с верной, и правильность ответа читается именно из неё.
        Assert.True(correctButLosingSide.GeneratedAnswerCorrect);
        // Прямой оракул подмены: на этой строке два признака обязаны быть разными.
        Assert.NotEqual(correctButLosingSide.GeneratedAnswerCorrect, correctButLosingSide.PolicyPreferenceWin);

        var wrongAnswerButWinningSide = separation.Rows.Single(row => row.RowId == "D2");
        // Зеркальный случай: верная сторона выше неверной, а выдан всё равно неверный знак.
        Assert.Equal(0.8d, wrongAnswerButWinningSide.PolicyLogProbabilityDifference, 10);
        Assert.True(wrongAnswerButWinningSide.PolicyPreferenceWin);
        Assert.False(wrongAnswerButWinningSide.GeneratedAnswerCorrect);
        // Тот же оракул с другой стороны: подмена ломается и при обратном расхождении.
        Assert.NotEqual(wrongAnswerButWinningSide.GeneratedAnswerCorrect, wrongAnswerButWinningSide.PolicyPreferenceWin);

        // Счёт верных ответов опирается на выданные строки, а не на знаки разностей:
        // верно отвечена D1, а правильную сторону политика выбрала на D2. Перечни строк
        // у этих двух величин не просто различаются числами — они не пересекаются вовсе.
        Assert.Equal(new[] { "D1" }, separation.GeneratedAnswerCorrectness.AllRows.CorrectRowIds);
        Assert.Equal(new[] { "D2" }, separation.GeneratedAnswerCorrectness.AllRows.IncorrectRowIds);
        Assert.Equal(new[] { "D2" }, separation.Rows.Where(row => row.PolicyPreferenceWin).Select(row => row.RowId).ToArray());
    }

    /// <summary>
    /// Главная проверка подмены: на одних и тех же записанных строках три величины
    /// дают три разных числа. Любая реализация, которая считает их одной формулой,
    /// обязана провалить эту проверку.
    /// </summary>
    [Fact]
    public void ThreeQuantitiesOfTheSameArmProduceThreeDifferentNumbers()
    {
        var recorded = LoadRecordedArm("arm_a");

        var separation = PreferenceSeparationReport.Build(
            recorded.Reference,
            recorded.Policy,
            RejectedSideByRowId,
            recorded.StepRewardMargins);

        // Доля ШАГОВ плеча с положительным средним взвешенным запасом: три шага из четырёх.
        Assert.Equal(0.75d, separation.StepPositiveMeanRewardMarginRate.Value!.Value, 10);
        Assert.Equal(3, separation.StepPositiveMeanRewardMarginRate.Numerator);
        Assert.Equal(4, separation.StepPositiveMeanRewardMarginRate.Denominator);

        // Панельная соседка под собственным именем: обе учебные пары панели улучшились.
        Assert.Equal(1.0d, separation.PanelPairReferenceRelativeImprovementRate.Value!.Value, 10);
        Assert.Equal(2, separation.PanelPairReferenceRelativeImprovementRate.Numerator);
        Assert.Equal(2, separation.PanelPairReferenceRelativeImprovementRate.Denominator);

        // Доля выбора правильной стороны пары: только одна пара из двух.
        Assert.Equal(0.5d, separation.PolicyPreferenceWinRate.Value!.Value, 10);
        Assert.Equal(1, separation.PolicyPreferenceWinRate.Numerator);
        Assert.Equal(2, separation.PolicyPreferenceWinRate.Denominator);

        // Фактические верные ответы: две строки панели из четырёх, с перечнем строк.
        Assert.Equal(2, separation.GeneratedAnswerCorrectness.AllRows.Correct);
        Assert.Equal(4, separation.GeneratedAnswerCorrectness.AllRows.Total);
        Assert.Equal(new[] { "T1", "E2" }, separation.GeneratedAnswerCorrectness.AllRows.CorrectRowIds);
        Assert.Equal(new[] { "T2", "E1" }, separation.GeneratedAnswerCorrectness.AllRows.IncorrectRowIds);
        Assert.Equal(1, separation.GeneratedAnswerCorrectness.TrainingRows.Correct);
        Assert.Equal(2, separation.GeneratedAnswerCorrectness.TrainingRows.Total);
        Assert.Equal(1, separation.GeneratedAnswerCorrectness.ExamRows.Correct);
        Assert.Equal(2, separation.GeneratedAnswerCorrectness.ExamRows.Total);

        // Величины одного плеча различаются: доля шагов с положительным средним
        // запасом 0,750, доля пар конечной панели 1,000, выбор правильной стороны
        // 0,500, фактические верные ответы 2 из 4. Одно имя на два разных числа недопустимо.
        Assert.NotEqual(
            separation.StepPositiveMeanRewardMarginRate.Value!.Value,
            separation.PanelPairReferenceRelativeImprovementRate.Value!.Value);
        Assert.NotEqual(
            separation.PanelPairReferenceRelativeImprovementRate.Value!.Value,
            separation.PolicyPreferenceWinRate.Value!.Value);
    }

    /// <summary>
    /// Доля улучшения относительно эталона 0,75 сопровождается нулём новых верных
    /// ответов: число верных строк до и после плеча A одинаково. Именно эта пара
    /// чисел и была обманом прежнего отчёта.
    /// </summary>
    [Fact]
    public void ImprovementRateOfThreeQuartersComesWithNoNewCorrectAnswer()
    {
        var recorded = LoadRecordedArm("arm_a");

        // Разделение состояния эталона против самого себя даёт счёт верных ответов «до».
        var before = PreferenceSeparationReport.Build(
            recorded.Reference,
            recorded.Reference,
            RejectedSideByRowId,
            recorded.StepRewardMargins);
        // Разделение состояния политики против эталона даёт счёт верных ответов «после».
        var after = PreferenceSeparationReport.Build(
            recorded.Reference,
            recorded.Policy,
            RejectedSideByRowId,
            recorded.StepRewardMargins);

        // Доля шагов с положительным средним взвешенным запасом равна 0,750.
        Assert.Equal(0.75d, after.StepPositiveMeanRewardMarginRate.Value!.Value, 10);
        Assert.Equal(2, before.GeneratedAnswerCorrectness.AllRows.Correct);
        Assert.Equal(2, after.GeneratedAnswerCorrectness.AllRows.Correct);
        // Сравнение эталона с самим собой не даёт ни одного улучшения относительно эталона.
        Assert.Equal(0.0d, before.PanelPairReferenceRelativeImprovementRate.Value!.Value, 10);
        // Правильность ответа при этом у обоих снятий одинакова и не равна нулю.
        Assert.Equal(
            before.GeneratedAnswerCorrectness.AllRows.CorrectRowIds,
            after.GeneratedAnswerCorrectness.AllRows.CorrectRowIds);
    }

    /// <summary>Во втором плече картина та же: улучшение против эталона есть, верная сторона выбрана не везде.</summary>
    [Fact]
    public void SecondArmShowsTheSameSeparationOnItsOwnRecordedNumbers()
    {
        var recorded = LoadRecordedArm("arm_b");

        var separation = PreferenceSeparationReport.Build(
            recorded.Reference,
            recorded.Policy,
            RejectedSideByRowId,
            recorded.StepRewardMargins);

        // Доля шагов: у плеча B все четыре шага дали положительный средний запас вознаграждения.
        Assert.Equal(1.0d, separation.StepPositiveMeanRewardMarginRate.Value!.Value, 10);
        // Панельная соседка: обе учебные пары улучшились относительно эталона.
        Assert.Equal(1.0d, separation.PanelPairReferenceRelativeImprovementRate.Value!.Value, 10);
        // Но правильную сторону политика выбирает только в одной паре из двух.
        Assert.Equal(0.5d, separation.PolicyPreferenceWinRate.Value!.Value, 10);
        // И число верных ответов панели осталось тем же, что и у плеча A.
        Assert.Equal(2, separation.GeneratedAnswerCorrectness.AllRows.Correct);
        Assert.Equal(-0.1560287d, separation.Rows.Single(row => row.RowId == "T2").PolicyLogProbabilityDifference, 6);
    }

    /// <summary>
    /// Машинный отчёт обязан нести каждую величину под её собственным однозначным
    /// именем и с полным паспортом: единица наблюдения, числитель, знаменатель, веса,
    /// агрегатор, фаза и источники. Доля ШАГОВ с положительным средним взвешенным
    /// запасом стоит под именем step_positive_mean_reward_margin_rate; среднее долей
    /// ПАР по квитанциям шагов стоит отдельно; прежнее двусмысленное имя, под которым
    /// раньше публиковались обе эти величины, из схемы отчёта убрано.
    /// </summary>
    [Fact]
    public void SerializedReportCarriesEachQuantityUnderItsOwnNameWithFullMeaning()
    {
        var recorded = LoadRecordedArm("arm_a");

        var separation = PreferenceSeparationReport.Build(
            recorded.Reference,
            recorded.Policy,
            RejectedSideByRowId,
            recorded.StepRewardMargins,
            recorded.StepPairImprovementRates);

        var json = JsonSerializer.Serialize(separation, new JsonSerializerOptions { WriteIndented = true });
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        // Доля ШАГОВ плеча с положительным средним взвешенным запасом: три шага из четырёх.
        Assert.Equal(0.75d, root.GetProperty("step_positive_mean_reward_margin_rate").GetProperty("value").GetDouble(), 10);
        // Среднее долей ПАР по четырём квитанциям плеча A (0; 1; 1; 1) стоит отдельной величиной.
        var meanOfRates = root.GetProperty("mean_of_step_pair_improvement_rates");
        Assert.True(meanOfRates.GetProperty("value_known").GetBoolean());
        Assert.Equal(0.75d, meanOfRates.GetProperty("value").GetDouble(), 10);
        Assert.Equal(4, meanOfRates.GetProperty("step_receipt_count").GetInt32());
        // Панельная соседка стоит под отдельным именем и несёт своё отличное число (1,000).
        Assert.Equal(1.0d, root.GetProperty("panel_pair_reference_relative_improvement_rate").GetProperty("value").GetDouble(), 10);
        Assert.Equal(0.5d, root.GetProperty("policy_preference_win_rate").GetProperty("value").GetDouble(), 10);
        Assert.Equal(2, root.GetProperty("generated_answer_correctness").GetProperty("all_rows").GetProperty("correct").GetInt32());
        // Двусмысленное имя, под которым раньше публиковались сразу две разные
        // величины, из схемы отчёта убрано; прежнее имя панельной соседки тоже.
        Assert.False(root.TryGetProperty("reference_relative_improvement_rate", out _));
        Assert.False(root.TryGetProperty("step_reference_relative_improvement_rate", out _));

        // Каждая величина несёт полный паспорт смысла, а доли — ещё числитель и знаменатель.
        foreach (var rateName in new[] { "step_positive_mean_reward_margin_rate", "panel_pair_reference_relative_improvement_rate", "policy_preference_win_rate" })
        {
            var rate = root.GetProperty(rateName);
            Assert.True(rate.TryGetProperty("numerator", out _));
            Assert.True(rate.TryGetProperty("denominator", out _));
            AssertMeaningIsComplete(rate.GetProperty("meaning"));
            Assert.Contains("не доля правильных ответов", rate.GetProperty("meaning").GetProperty("not_the_same_as").GetString());
        }
        AssertMeaningIsComplete(meanOfRates.GetProperty("meaning"));
        AssertMeaningIsComplete(root.GetProperty("generated_answer_correctness").GetProperty("meaning"));

        // Единицы наблюдения соседних величин названы прямо и различаются.
        Assert.StartsWith("шаг обучения предпочтению", root.GetProperty("step_positive_mean_reward_margin_rate").GetProperty("meaning").GetProperty("observation_unit").GetString());
        Assert.StartsWith("учебная пара конечной панели", root.GetProperty("panel_pair_reference_relative_improvement_rate").GetProperty("meaning").GetProperty("observation_unit").GetString());

        // Имя «Accuracy» в разделённом блоке не используется вовсе.
        Assert.DoesNotContain("Accuracy", json, StringComparison.Ordinal);
        // Запас вознаграждения прямо помечен как не норма и не сила градиента.
        Assert.Contains("не норма и не сила градиента", root.GetProperty("reward_margin_note").GetString());
        // Разные знаменатели соседних величин названы прямо в самом отчёте.
        Assert.Contains("РАЗНЫЕ знаменатели", root.GetProperty("different_denominators_note").GetString());
        // Отрицательный контроль виден и в самом машинном отчёте.
        var secondPair = root.GetProperty("rows").EnumerateArray().Single(row => row.GetProperty("row_id").GetString() == "T2");
        Assert.True(secondPair.GetProperty("panel_pair_reference_relative_improvement").GetBoolean());
        Assert.False(secondPair.GetProperty("policy_preference_win").GetBoolean());
        Assert.False(secondPair.GetProperty("generated_answer_correct").GetBoolean());
    }

    /// <summary>
    /// Отрицательный контроль против смешения агрегаций. Внутри ОДНОГО шага знаки
    /// запасов разные: пары [+1, −3]. При равных весах доля ПАР этого шага равна 0,5,
    /// а средний взвешенный запас равен −1, поэтому шаг НЕ засчитывается и доля ШАГОВ
    /// равна нулю. При исходных весах [0, 1] нулевой вес сначала поднимается до 0,01 и
    /// только потом нормируется, поэтому доля пар равна 0,01/1,01, а не 0,5.
    /// Существующее сравнение трёх шагов из четырёх против двух пар из четырёх эту
    /// подмену не ловит: там знаки внутри каждого шага однородны и числа совпадают.
    /// </summary>
    [Fact]
    public void StepWithOppositeSignMarginsSeparatesPairShareFromStepShare()
    {
        // Равные веса: доля пар 0,5, средний взвешенный запас −1, шаг не засчитан.
        var equalWeights = PreferenceSeparationReport.DescribeStepAggregation(
            new[] { 1.0d, -3.0d },
            new[] { 1.0d, 1.0d });
        Assert.Equal(new[] { 0.5d, 0.5d }, equalWeights.FlooredNormalisedWeights);
        Assert.Equal(0.5d, equalWeights.StepPairImprovementRate, 10);
        Assert.Equal(-1.0d, equalWeights.StepMeanWeightedRewardMargin, 10);
        Assert.False(equalWeights.StepCountsAsPositive);
        // Две величины ОДНОГО шага разошлись: доля пар 0,5 против признака шага «ноль».
        Assert.NotEqual(
            equalWeights.StepPairImprovementRate,
            equalWeights.StepCountsAsPositive ? 1.0d : 0.0d);

        // Исходные веса [0, 1]: нижняя отсечка 0,01 не даёт нулевой паре исчезнуть.
        var zeroWeightPair = PreferenceSeparationReport.DescribeStepAggregation(
            new[] { 1.0d, -3.0d },
            new[] { 0.0d, 1.0d });
        Assert.Equal(0.01d / 1.01d, zeroWeightPair.StepPairImprovementRate, 12);
        Assert.NotEqual(0.5d, zeroWeightPair.StepPairImprovementRate, 3);
        Assert.Equal(-2.99d / 1.01d, zeroWeightPair.StepMeanWeightedRewardMargin, 12);
        Assert.False(zeroWeightPair.StepCountsAsPositive);

        // Тот же шаг, пройденный через действующий построитель отчёта: доля ШАГОВ равна
        // нулю, а среднее долей ПАР равно 0,5. Реализация, считающая их одной формулой,
        // обязана провалить именно эту проверку.
        var separation = PreferenceSeparationReport.Build(
            MixedSignReference,
            MixedSignPolicy,
            MixedSignRejectedSide,
            new[] { equalWeights.StepMeanWeightedRewardMargin },
            new[] { equalWeights.StepPairImprovementRate });

        Assert.Equal(0.0d, separation.StepPositiveMeanRewardMarginRate.Value!.Value, 10);
        Assert.Equal(0, separation.StepPositiveMeanRewardMarginRate.Numerator);
        Assert.Equal(1, separation.StepPositiveMeanRewardMarginRate.Denominator);
        Assert.True(separation.MeanOfStepPairImprovementRates.ValueKnown);
        Assert.Equal(0.5d, separation.MeanOfStepPairImprovementRates.Value!.Value, 10);
        Assert.NotEqual(
            separation.StepPositiveMeanRewardMarginRate.Value!.Value,
            separation.MeanOfStepPairImprovementRates.Value!.Value);

        // И с исходными весами [0, 1] те же две величины снова разные.
        var flooredSeparation = PreferenceSeparationReport.Build(
            MixedSignReference,
            MixedSignPolicy,
            MixedSignRejectedSide,
            new[] { zeroWeightPair.StepMeanWeightedRewardMargin },
            new[] { zeroWeightPair.StepPairImprovementRate });
        Assert.Equal(0.0d, flooredSeparation.StepPositiveMeanRewardMarginRate.Value!.Value, 10);
        Assert.Equal(0.01d / 1.01d, flooredSeparation.MeanOfStepPairImprovementRates.Value!.Value, 12);
    }

    /// <summary>
    /// Проверка записи определений, которую реально собирает хозяин опыта. Прежняя
    /// проверка касалась только внутреннего построителя, поэтому неверную строку
    /// внешнего писателя она не ловила. Здесь проверяется общий метод, который хозяин
    /// опыта вызывает в своём блоке определений, и отдельно — что он его вызывает.
    /// </summary>
    [Fact]
    public void MetricDefinitionsWrittenByTheExperimentHostDescribeEachQuantityHonestly()
    {
        var definitions = PreferenceSeparationReport.BuildMetricDefinitions();
        var json = JsonSerializer.Serialize(definitions, new JsonSerializerOptions { WriteIndented = true });
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        // Главная исправленная неправда: поле ОДНОЙ квитанции описывается как доля ПАР
        // этого шага, а не как доля шагов.
        var receiptField = root.GetProperty("accuracy_field_in_step_metrics");
        Assert.StartsWith("пара обучения предпочтению", receiptField.GetProperty("observation_unit").GetString());
        Assert.Contains("доля ПАР одного шага", receiptField.GetProperty("definition").GetString());
        Assert.Contains("НЕ доля шагов", receiptField.GetProperty("not_the_same_as").GetString());
        Assert.Contains("0,01", receiptField.GetProperty("weights").GetString());
        // Прежняя формулировка «хранит долю шагов» не должна встречаться нигде в записи.
        Assert.DoesNotContain("хранит долю шагов", json, StringComparison.Ordinal);

        // Доля ШАГОВ стоит под собственным однозначным именем, среднее долей — под своим.
        var stepRate = root.GetProperty("step_positive_mean_reward_margin_rate");
        Assert.StartsWith("шаг обучения предпочтению", stepRate.GetProperty("observation_unit").GetString());
        Assert.Contains("сколько шагов", stepRate.GetProperty("numerator").GetString());
        var meanOfRates = root.GetProperty("mean_of_step_pair_improvement_rates");
        Assert.Contains("сумма дробей", meanOfRates.GetProperty("numerator").GetString());
        Assert.Contains("восстанавливать", meanOfRates.GetProperty("not_the_same_as").GetString());
        // Двусмысленного имени, под которым публиковались сразу две величины, здесь нет.
        Assert.False(root.TryGetProperty("reference_relative_improvement_rate", out _));

        // Каждая из шести величин несёт полный паспорт из семи обязательных полей.
        foreach (var quantityName in new[]
                 {
                     "accuracy_field_in_step_metrics",
                     "step_positive_mean_reward_margin_rate",
                     "mean_of_step_pair_improvement_rates",
                     "panel_pair_reference_relative_improvement_rate",
                     "policy_preference_win_rate",
                     "generated_answer_correctness"
                 })
            AssertMeaningIsComplete(root.GetProperty(quantityName));

        // Разные знаменатели, разобранный контрпример и граница нового имени записаны прямо.
        Assert.Contains("РАЗНЫЕ знаменатели", root.GetProperty("different_denominators_warning").GetString());
        Assert.Contains("[+1, −3]", root.GetProperty("worked_counterexample").GetString());
        Assert.Contains("0,01/1,01", root.GetProperty("worked_counterexample").GetString());
        Assert.Contains("не переименовываются", root.GetProperty("name_proposal_boundary").GetString());
        Assert.Contains("не менялась", root.GetProperty("signature_change_is_not_learning").GetString());

        // Хозяин опыта обязан писать именно эту запись, а не собственный набор строк:
        // иначе проверка снова перестала бы видеть внешнего писателя.
        var hostSource = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(), "experiments-v2", "RP01-MICRO", "P0201", "Program.cs"));
        Assert.Contains("metric_definitions = PreferenceSeparationReport.BuildMetricDefinitions()", hostSource, StringComparison.Ordinal);
        Assert.DoesNotContain("accuracy_field_in_step_metrics = \"", hostSource, StringComparison.Ordinal);
    }

    /// <summary>
    /// Неизмеренное время обычных уроков записывается пустым значением с причиной, а
    /// не нулём: ноль был бы утверждением о мгновенном обучении. Проверяется исходник
    /// хозяина опыта, потому что само это поле заполняет он.
    /// </summary>
    [Fact]
    public void UnmeasuredSupervisedTrainingTimeIsWrittenAsUnknownNotZero()
    {
        var hostSource = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(), "experiments-v2", "RP01-MICRO", "P0201", "Program.cs"));

        // Поле допускает отсутствие значения и несёт рядом человеческую причину.
        Assert.Contains("double? SftWallMs, string? SftWallMsUnknownReason", hostSource, StringComparison.Ordinal);
        Assert.Contains("wall_ms_arm_b_sft_unknown_reason = armB.SftWallMsUnknownReason", hostSource, StringComparison.Ordinal);
        Assert.Contains("wall_ms_arm_b_sft_measured = armB.SftWallMs is not null", hostSource, StringComparison.Ordinal);
        // Прежняя подстановка нуля вместо неизмеренного времени исчезла.
        Assert.DoesNotContain("watch.Elapsed.TotalMilliseconds, 0d", hostSource, StringComparison.Ordinal);
    }

    /// <summary>Паспорт величины обязан нести все семь обязательных полей заполненными.</summary>
    private static void AssertMeaningIsComplete(JsonElement meaning)
    {
        foreach (var field in new[] { "observation_unit", "numerator", "denominator", "weights", "aggregator", "phase", "definition", "not_the_same_as" })
            Assert.False(string.IsNullOrWhiteSpace(meaning.GetProperty(field).GetString()), "пустое поле паспорта величины: " + field);

        // Источники перечисляются списком и не могут быть пустыми.
        Assert.NotEmpty(meaning.GetProperty("sources").EnumerateArray().ToArray());
    }

    /// <summary>Снятие эталона для шага с разными знаками запасов: обе строки нужны только как база.</summary>
    private static readonly PanelObservation[] MixedSignReference =
    {
        new() { RowId = "M1", Gold = "+", Produced = "+", TrainingRow = true, LogProbabilityOfPlus = -0.9d, LogProbabilityOfMinus = -0.5d },
        new() { RowId = "M2", Gold = "-", Produced = "-", TrainingRow = true, LogProbabilityOfPlus = -0.5d, LogProbabilityOfMinus = -0.9d }
    };

    /// <summary>Снятие политики для того же шага: панель здесь нужна только для построчных признаков.</summary>
    private static readonly PanelObservation[] MixedSignPolicy =
    {
        new() { RowId = "M1", Gold = "+", Produced = "+", TrainingRow = true, LogProbabilityOfPlus = -0.4d, LogProbabilityOfMinus = -0.6d },
        new() { RowId = "M2", Gold = "-", Produced = "+", TrainingRow = true, LogProbabilityOfPlus = -0.4d, LogProbabilityOfMinus = -1.2d }
    };

    /// <summary>Отвергаемые стороны обеих построенных пар объявлены явно.</summary>
    private static readonly Dictionary<string, string> MixedSignRejectedSide =
        new(StringComparer.Ordinal) { ["M1"] = "-", ["M2"] = "+" };

    /// <summary>Пустой знаменатель оставляет долю неизвестной и не подставляет ноль.</summary>
    [Fact]
    public void EmptyDenominatorLeavesTheRateUnknownInsteadOfZero()
    {
        var recorded = LoadRecordedArm("arm_a");

        // Ни одной объявленной пары: обе относительные доли считать не из чего.
        var separation = PreferenceSeparationReport.Build(
            recorded.Reference,
            recorded.Policy,
            new Dictionary<string, string>(StringComparer.Ordinal),
            Array.Empty<double>());

        Assert.Null(separation.PanelPairReferenceRelativeImprovementRate.Value);
        Assert.Null(separation.PolicyPreferenceWinRate.Value);
        Assert.Null(separation.StepPositiveMeanRewardMarginRate.Value);
        Assert.Equal(0, separation.PolicyPreferenceWinRate.Denominator);
        // Среднее долей квитанций остаётся НЕИЗВЕСТНЫМ, а не нулём и не восстановленным
        // из среднего запаса: без переданных долей пар считать его не из чего.
        Assert.False(separation.MeanOfStepPairImprovementRates.ValueKnown);
        Assert.Null(separation.MeanOfStepPairImprovementRates.Value);
        Assert.Equal(0, separation.MeanOfStepPairImprovementRates.StepReceiptCount);
        Assert.Contains("неизвестно", separation.MeanOfStepPairImprovementRates.UnknownReason);
        // Фактические верные ответы измеримы всегда и остаются прежними.
        Assert.Equal(2, separation.GeneratedAnswerCorrectness.AllRows.Correct);
    }

    /// <summary>Нулевая база не превращается в процент улучшения.</summary>
    [Fact]
    public void ZeroBaselineRefusesAPercentAndKeepsTheAbsoluteDifference()
    {
        // Первый запас вознаграждения плеча A равен нулю по построению опыта.
        var reading = PreferenceSeparationReport.DescribeRelativeGain(0d, 0.0027732193d, "первый запас вознаграждения плеча");

        Assert.False(reading.RelativeGainDefined);
        Assert.Null(reading.RelativeGainPercent);
        Assert.Equal(0.0027732193d, reading.AbsoluteDifference, 10);
        Assert.Contains("база равна нулю", reading.Reason);
    }

    /// <summary>Смена знака показателя также не переводится в процент улучшения.</summary>
    [Fact]
    public void SignChangeRefusesAPercentOfImprovement()
    {
        var reading = PreferenceSeparationReport.DescribeRelativeGain(-0.0005d, 0.0005d, "знаковый показатель");

        Assert.False(reading.RelativeGainDefined);
        Assert.Null(reading.RelativeGainPercent);
        Assert.Equal(0.001d, reading.AbsoluteDifference, 10);
        Assert.Contains("знак показателя меняется", reading.Reason);
    }

    /// <summary>
    /// Малый локальный эффект наблюдаемого участка обучения предпочтению сохраняется:
    /// прирост за три наблюдаемых интервала у плеча B больше примерно на 1,504%.
    /// </summary>
    [Fact]
    public void SmallLocalGainOfTheObservedPreferenceSegmentIsKept()
    {
        var firstArm = LoadRecordedArm("arm_a").StepRewardMargins;
        var secondArm = LoadRecordedArm("arm_b").StepRewardMargins;

        var firstArmGain = firstArm[^1] - firstArm[0];
        var secondArmGain = secondArm[^1] - secondArm[0];
        var reading = PreferenceSeparationReport.DescribeRelativeGain(
            firstArmGain,
            secondArmGain,
            "прирост запаса вознаграждения за наблюдаемые интервалы обучения предпочтению");

        Assert.Equal(0.0012550415d, firstArmGain, 10);
        Assert.Equal(0.0012739184d, secondArmGain, 10);
        Assert.True(reading.RelativeGainDefined);
        Assert.Equal(1.504d, reading.RelativeGainPercent!.Value, 2);
    }

    /// <summary>
    /// Квитанция обучателя остаётся побайтово прежней, а явное имя той же величины
    /// доступно потребителям в коде. Так совместимость уже записанных показаний не
    /// нарушается, а читатель отчёта перестаёт принимать одну величину за другую.
    /// </summary>
    [Fact]
    public void TrainerReceiptKeepsItsHistoricalBytesAndGainsAnExplicitlyNamedOutput()
    {
        const string recordedReceipt =
            "{\"Step\":7,\"Loss\":0.25,\"ChosenReward\":0.5,\"RejectedReward\":-0.5,\"RewardMargin\":1,\"Accuracy\":1}";
        var metrics = JsonSerializer.Deserialize<DPO.Models.DpoMetrics>(recordedReceipt)!;

        // Записанная квитанция читается и записывается обратно теми же байтами.
        Assert.Equal(recordedReceipt, JsonSerializer.Serialize(metrics));
        // Явное имя даёт то же самое число: это переименование подписи, а не новое измерение.
        Assert.Equal(metrics.Accuracy, metrics.ReferenceRelativeImprovementRate);
        // В самой квитанции нового поля нет, поэтому прежние показания остаются совместимыми.
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(metrics));
        Assert.True(document.RootElement.TryGetProperty("Accuracy", out _));
        Assert.False(document.RootElement.TryGetProperty("ReferenceRelativeImprovementRate", out _));
    }

    /// <summary>Записанные наблюдения одного плеча сохранённого показания опыта.</summary>
    private sealed record RecordedArm(
        PanelObservation[] Reference,
        PanelObservation[] Policy,
        double[] StepRewardMargins,
        double[] StepPairImprovementRates);

    /// <summary>
    /// Читает уже сохранённое показание опыта. Это чтение выполненного учебного
    /// опыта: ни одного обращения к модели и ни одного обновления весов здесь нет.
    /// </summary>
    private static RecordedArm LoadRecordedArm(string armName)
    {
        var metricsPath = Path.Combine(
            FindRepositoryRoot(),
            "experiments-v2", "RP01-MICRO", "P0201", "results", "attempt-0001", "metrics.json");
        Assert.True(File.Exists(metricsPath), "сохранённое показание опыта не найдено: " + metricsPath);

        using var document = JsonDocument.Parse(File.ReadAllBytes(metricsPath));
        var root = document.RootElement;

        // Признак учебной строки берётся из описания панели этого же показания.
        var trainingRowById = root.GetProperty("panel").GetProperty("rows").EnumerateArray()
            .ToDictionary(
                row => row.GetProperty("Id").GetString()!,
                row => row.GetProperty("retention_row").GetBoolean(),
                StringComparer.Ordinal);

        var arm = root.GetProperty(armName);
        var rows = arm.GetProperty("rows").EnumerateArray().ToArray();

        // Снятие эталона — это состояние сохранённого старта, записанное как «до».
        var reference = rows.Select(row => new PanelObservation
        {
            RowId = row.GetProperty("Id").GetString()!,
            Gold = row.GetProperty("gold").GetString()!,
            Produced = row.GetProperty("produced_before").GetString()!,
            TrainingRow = trainingRowById[row.GetProperty("Id").GetString()!],
            LogProbabilityOfPlus = row.GetProperty("log_p_plus_before").GetDouble(),
            LogProbabilityOfMinus = row.GetProperty("log_p_minus_before").GetDouble()
        }).ToArray();

        // Снятие политики — состояние того же плеча в конце обучения.
        var policy = rows.Select(row => new PanelObservation
        {
            RowId = row.GetProperty("Id").GetString()!,
            Gold = row.GetProperty("gold").GetString()!,
            Produced = row.GetProperty("produced_after").GetString()!,
            TrainingRow = trainingRowById[row.GetProperty("Id").GetString()!],
            LogProbabilityOfPlus = row.GetProperty("log_p_plus_after").GetDouble(),
            LogProbabilityOfMinus = row.GetProperty("log_p_minus_after").GetDouble()
        }).ToArray();

        // Средние взвешенные запасы вознаграждения всех шагов берутся из тех же сохранённых квитанций.
        var margins = arm.GetProperty("dpo_step_metrics").EnumerateArray()
            .Select(step => step.GetProperty("RewardMargin").GetDouble())
            .ToArray();

        // Доли ПАР каждого шага лежат в историческом поле той же квитанции: единица
        // наблюдения этого числа — пара шага, а не шаг плеча.
        var pairImprovementRates = arm.GetProperty("dpo_step_metrics").EnumerateArray()
            .Select(step => step.GetProperty("Accuracy").GetDouble())
            .ToArray();

        return new RecordedArm(reference, policy, margins, pairImprovementRates);
    }

    /// <summary>Корень хранилища находится по файлу решения вверх от каталога сборки.</summary>
    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Корень хранилища не найден.");
    }
}
