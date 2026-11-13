// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using ProjectWikiDomain;

/// <summary>
/// Проверяемое поведение продукта: дверь приёмки обученной модели обязана
/// отказать, когда показатель не измерен, и обязана сделать это ДО сравнения с
/// порогом.
///
/// Почему это не придирка. В C# любое сравнение с «не-числом» ложно, поэтому
/// проверка «отказать, если показатель ниже порога» при неизмеренном числе
/// молча отвечает «порог пройден» и пускает негодную обученную модель в
/// публикацию. С плюс бесконечностью беда зеркальная: она больше любого
/// порога, поэтому положительная форма проверки объявляет её успехом.
///
/// Где это вероятнее всего. В запасе предпочтения и в начальном расхождении
/// обучаемой и справочной модели: это разности логарифмов вероятностей, их
/// область значений не ограничена по смыслу, нулевая вероятность даёт
/// бесконечность, а вычитание бесконечностей — не-число. Точность по смыслу
/// должна лежать на отрезке от нуля до единицы, но проверки диапазона в этом
/// домене нет, поэтому это ожидание, а не гарантия кода.
///
/// Что проверка НЕ утверждает. Общее правило отвергает ровно «не-число» и обе
/// бесконечности. Конечное бессмысленное значение — например точность порядка
/// 1e308 — остаётся измеренным и проходит порог; это отдельная открытая
/// находка, а не закрытый случай. Для среднего по диагностике это значит, что
/// закрыт перелив суммы в бесконечность, для которого нужны ДВА огромных
/// члена; один огромный член при малых остальных даёт конечное среднее и
/// остаётся непойманным.
///
/// Граница утверждения этих проверок. Здесь доказывается ровно общее правило
/// измеренности двери публикации и её решение на враждебных числах. То, что
/// каждая из четырнадцати дверей конвейеров обучения спрашивает это же правило
/// до сравнения с порогом, прогоном НЕ доказано: чтобы дойти до тех дверей,
/// нужен полный учебный прогон конвейера, который в этот кусок работы не
/// входит. Тот слой доказывается дифом и компилятором.
/// </summary>
public sealed class PublicationGateMeasuredRuleRejectsNonFiniteMetricsTests
{
    // Три враждебных значения, ровно те, из-за которых порог молчит: «не
    // число», плюс бесконечность и минус бесконечность.
    public static TheoryData<string, double> UnmeasuredValues() => new()
    {
        { "не-число", double.NaN },
        { "плюс бесконечность", double.PositiveInfinity },
        { "минус бесконечность", double.NegativeInfinity }
    };

    // Те же три значения, умноженные на три читаемых дверью публикации поля:
    // конечная точность на обучающем наборе, конечный запас предпочтения и
    // конечная точность на отложенной выборке.
    public static TheoryData<string, string, double> UnmeasuredFieldCases()
    {
        // Набор случаев собирается перечислением, чтобы ни одно поле и ни одно
        // враждебное значение не осталось непроверенным по забывчивости.
        var cases = new TheoryData<string, string, double>();
        foreach (var field in new[]
                 {
                     "конечная точность на обучающем наборе",
                     "конечный запас предпочтения",
                     "конечная точность на отложенной выборке"
                 })
        {
            // Каждое поле проверяется всеми тремя враждебными значениями.
            cases.Add(field, "не-число", double.NaN);
            cases.Add(field, "плюс бесконечность", double.PositiveInfinity);
            cases.Add(field, "минус бесконечность", double.NegativeInfinity);
        }

        return cases;
    }

    [Theory]
    [MemberData(nameof(UnmeasuredValues))]
    public void UnmeasuredValue_IsNotAcceptedAsMeasurement(
        string caseName,
        double value)
    {
        // Общее правило обязано назвать такое значение неизмеренным: именно на
        // этом определении стоят все двери приёмки этого домена.
        Assert.False(
            ProjectWikiDpoPublicationMetricGate.IsMeasured(value),
            $"Значение «{caseName}» не является измерением и обязано быть отвергнуто общим правилом.");

        // И короткая форма для дверей, читающих несколько чисел, обязана
        // отвечать то же самое, иначе два определения разошлись бы.
        Assert.True(
            ProjectWikiDpoPublicationMetricGate.AnyNotMeasured(0.90, value),
            $"Набор с значением «{caseName}» обязан считаться содержащим неизмеренное число.");
    }

    [Theory]
    [InlineData("порог точности", 0.75)]
    [InlineData("ноль", 0.0)]
    [InlineData("отрицательный запас предпочтения", -0.30)]
    [InlineData("очень большое, но конечное число", 1.7e308)]
    public void HonestFiniteNumber_IsAcceptedAsMeasurement(
        string caseName,
        double value)
    {
        // Обратная сторона правила: честное конечное число обязано проходить,
        // иначе проверка измеренности закрыла бы и годные прогоны.
        Assert.True(
            ProjectWikiDpoPublicationMetricGate.IsMeasured(value),
            $"Значение «{caseName}» измерено и обязано проходить общее правило.");

        // И набор из таких чисел не содержит неизмеренных.
        Assert.False(
            ProjectWikiDpoPublicationMetricGate.AnyNotMeasured(value, 0.90, 0.42));
    }

    [Fact]
    public void EmptySetOfNumbers_HasNothingUnmeasured()
    {
        // Пустой набор нечего признавать неизмеренным: это важно, потому что
        // иначе дверь без прочитанных чисел отказывала бы всегда.
        Assert.False(ProjectWikiDpoPublicationMetricGate.AnyNotMeasured());
    }

    [Fact]
    public void NullSetOfNumbers_IsRefused_BecauseMeasurementCannotBeConfirmed()
    {
        // Пустая ссылка — это не «проверять нечего», а «подтвердить
        // измеренность нечем». Для двери приёмки запас по умолчанию обязан быть
        // отказывающим: разрешающий ответ открыл бы дверь ровно тогда, когда
        // проверка не состоялась, то есть повторил бы болезнь, против которой
        // стоит всё это правило.
        Assert.True(ProjectWikiDpoPublicationMetricGate.AnyNotMeasured(null));
    }

    [Theory]
    [MemberData(nameof(UnmeasuredFieldCases))]
    public void UnmeasuredPublicationMetric_IsRefusedBeforeTheThresholdComparison(
        string fieldName,
        string valueName,
        double unmeasuredValue)
    {
        // Здоровый прогон, в котором испорчено ровно одно число: так видно, что
        // отказ вызвало именно оно, а не что-то ещё.
        var candidate = HealthyRun();
        candidate = fieldName switch
        {
            "конечная точность на обучающем наборе" =>
                candidate with { FinalAccuracy = unmeasuredValue },
            "конечный запас предпочтения" =>
                candidate with { FinalRewardMargin = unmeasuredValue },
            _ => candidate with { HeldOutAccuracy = unmeasuredValue }
        };

        var gate = ProjectWikiDpoPublicationMetricGate.Evaluate(candidate);

        // Отказ обязателен: публиковать модель, показателя которой у нас нет,
        // нельзя ни при каком пороге.
        Assert.False(
            gate.IsSuccess,
            $"Поле «{fieldName}» со значением «{valueName}» обязано закрыть дверь публикации.");

        // И причина обязана быть отличима от «не дотянул до порога»: человек
        // должен видеть, что числа просто нет, иначе он пойдёт улучшать модель
        // вместо того, чтобы починить измерение.
        Assert.Equal(
            ProjectWikiDpoPublicationMetricGate.MetricsNotMeasuredErrorCode,
            gate.ErrorCode);

        // Короткий ответ «да или нет» обязан совпадать с кодом отказа, иначе
        // места, собирающие решение из нескольких условий, разошлись бы с
        // местами, печатающими причину.
        Assert.False(ProjectWikiDpoPublicationMetricGate.Allows(candidate));
    }

    [Fact]
    public void HonestRun_PassesTheGate_SoTheMeasuredCheckDoesNotCloseGoodRuns()
    {
        // Честный прогон: все три читаемых числа измерены и выдерживают порог.
        var candidate = HealthyRun();

        var gate = ProjectWikiDpoPublicationMetricGate.Evaluate(candidate);

        // Дверь обязана открыться, иначе проверка измеренности была бы просто
        // запретом публикации вообще.
        Assert.True(gate.IsSuccess, gate.ErrorMessage);
        Assert.True(ProjectWikiDpoPublicationMetricGate.Allows(candidate));
    }

    [Theory]
    [MemberData(nameof(UnmeasuredValues))]
    public void ThresholdComparisonAlone_CannotRefuseUnmeasuredRewardMargin(
        string caseName,
        double unmeasuredValue)
    {
        // Этот случай объясняет, зачем вообще нужна отдельная проверка. Берём
        // запас предпочтения — именно его область значений не ограничена — и
        // смотрим, что делает одно только сравнение с порогом.
        var candidate = HealthyRun() with { FinalRewardMargin = unmeasuredValue };

        // Прежняя форма отказа «показатель не больше нуля» на «не-числе» и на
        // плюс бесконечности молчит: сравнение ложно, значит отказа нет.
        var thresholdFormRefuses =
            candidate.FinalRewardMargin
                <= ProjectWikiDpoPublicationMetricGate
                    .ExclusiveFloorForFinalTrainingRewardMargin;
        var expectedThresholdFormRefuses = double.IsNegativeInfinity(unmeasuredValue);
        Assert.Equal(expectedThresholdFormRefuses, thresholdFormRefuses);

        // А общее правило закрывает дверь во всех трёх случаях, включая те, где
        // сравнение с порогом бессильно.
        Assert.False(
            ProjectWikiDpoPublicationMetricGate.Allows(candidate),
            $"Запас предпочтения «{caseName}» обязан закрывать дверь независимо от порога.");
    }

    [Theory]
    [MemberData(nameof(UnmeasuredValues))]
    public void AverageOfDiagnostics_InheritsUnmeasuredValueFromOneMember(
        string caseName,
        double unmeasuredValue)
    {
        // Арифметическое свойство, на которое опираются шесть дверей со
        // средними: один неизмеренный член группы делает неизмеренным всё
        // среднее. Это свойство арифметики, а не живой путь беды: в файл
        // диагностик ни «не-число», ни бесконечность попасть не могут, потому
        // что разбор идёт без разрешения именованных нечисловых литералов.
        // Живой путь — перелив суммы, он проверяется отдельно ниже.
        var average = new[] { 0.90, unmeasuredValue, 0.95 }.Average();

        Assert.False(
            ProjectWikiDpoPublicationMetricGate.IsMeasured(average),
            $"Среднее с членом «{caseName}» не измерено и обязано быть отвергнуто.");
    }

    [Fact]
    public void AverageOfHugeFiniteMembers_OverflowsToInfinity_AndIsRefused()
    {
        // Самый правдоподобный путь беды в живом прогоне: в файл диагностик
        // попадают только конечные числа, но два огромных конечных члена
        // переливают сумму в бесконечность, и среднее становится больше любого
        // порога. Именно этот вход закрывают шесть дверей со средними.
        var hugeFiniteMembers = new[] { 1.7e308, 1.7e308 };
        Assert.All(
            hugeFiniteMembers,
            member => Assert.True(
                ProjectWikiDpoPublicationMetricGate.IsMeasured(member),
                "Члены группы обязаны быть конечными, иначе проверка доказывала бы другое."));

        var average = hugeFiniteMembers.Average();

        // Среднее двух конечных чисел оказалось бесконечным — и обязано быть
        // отвергнуто общим правилом.
        Assert.True(double.IsPositiveInfinity(average));
        Assert.False(ProjectWikiDpoPublicationMetricGate.IsMeasured(average));
    }

    // Здоровый прогон обучения предпочтений: все числа измерены и выдерживают
    // порог. Испорченный случай получается из него одной подменой поля.
    private static ProjectWikiDpoMetricRunResult HealthyRun()
        => new(
            "publication-gate-measured-rule-run",
            "publication-gate-measured-rule-unit",
            "publication-gate-measured-rule-batch",
            "",
            "",
            "",
            1,
            0.01,
            "deterministic_fresh_policy",
            "copied_from_deterministic_fresh_policy",
            0,
            "",
            "",
            50,
            50,
            0.10,
            0.95,
            0.50,
            0.12,
            0.90,
            0.40,
            0,
            0,
            1,
            0,
            "",
            []);
}
