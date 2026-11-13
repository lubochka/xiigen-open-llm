// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using AgentInfrastructure.Logical;

namespace LlmFromScratch.Tests;

// Проверяет второго боевого потребителя монитора сходимости — цикл обучения
// предпочтениями (Direct Preference Optimization) в проекте обучения
// способностей. Там две разные болезни, а не одна: недостижимость при коротком
// бюджете шагов и невозможность остановки при нулевом пороге. Оба признака
// считаются из настройки и не зависят друг от друга. Числа выведены из
// фактического поведения монитора и из живого контракта бюджета шагов, а не из
// памяти автора теста.
public class CapabilityTrainingDpoConvergenceReachabilityTests
{
    // Собирает предупреждения диагностики, чтобы признак можно было проверить
    // без запуска обучения.
    private sealed class CapturingTraceListener : TraceListener
    {
        // Собранные строки предупреждений этого прогона.
        internal List<string> Captured { get; } = new();

        // Trace.TraceWarning приходит сюда одним готовым сообщением.
        public override void WriteLine(string? message)
        {
            // Пустые строки формата журнала не несут смысла для проверки.
            if (!string.IsNullOrEmpty(message))
                Captured.Add(message);
        }

        // Частичная запись нас не интересует: признак выдаётся целой строкой.
        public override void Write(string? message)
        {
        }
    }

    // Воспроизводит ровно нынешний продуктовый цикл обучения этой единицы:
    // монитор не создаётся при нулевом пороге, вопрос о сходимости задаётся
    // только на шагах, кратных интервалу наблюдения, и только когда монитор
    // существует. Порядок проверок — как в продукте.
    private static int SimulateTrainingLoop(
        int stepBudget,
        int windowSteps,
        float threshold,
        float flatLoss)
    {
        // Монитор создаётся так же, как в цикле обучения после правки.
        var monitor = threshold == 0f
            ? null
            : new DefaultLogicalCapabilityTrainer.ConvergenceMonitor(
                threshold,
                windowSteps);
        // Сколько шагов цикл фактически выполнил до выхода.
        var executed = 0;
        for (var step = 1; step <= stepBudget; step++)
        {
            // Каждый шаг считается выполненным до проверки сходимости.
            executed = step;
            // Та же тройная проверка в том же порядке, что и в продукте.
            if (step % windowSteps == 0 &&
                monitor is not null &&
                monitor.ShouldStop(flatLoss))
            {
                break;
            }
        }

        return executed;
    }

    // Воспроизводит прежний продуктовый цикл до правки: монитор создавался
    // всегда, в том числе при нулевом пороге, и его всегда спрашивали. Нужен
    // ровно для доказательства наблюдаемой нейтральности правки.
    private static int SimulateTrainingLoopBeforeRepair(
        int stepBudget,
        int windowSteps,
        float threshold,
        float flatLoss)
    {
        // Прежнее безусловное создание монитора.
        var monitor = new DefaultLogicalCapabilityTrainer.ConvergenceMonitor(
            threshold,
            windowSteps);
        // Сколько шагов выполнил прежний цикл.
        var executed = 0;
        for (var step = 1; step <= stepBudget; step++)
        {
            // Шаг считается выполненным до проверки, как и раньше.
            executed = step;
            // Прежнее условие без проверки существования монитора.
            if (step % windowSteps == 0 && monitor.ShouldStop(flatLoss))
                break;
        }

        return executed;
    }

    [Fact]
    public void Wrapper_CannotReportStopBeforeThirdObservation()
    {
        // Полностью постоянная потеря — самый благоприятный случай для
        // остановки. Если тонкая обёртка не может ответить "пора" даже здесь,
        // значит раньше третьего наблюдения она не может ответить вообще.
        var monitor = new DefaultLogicalCapabilityTrainer.ConvergenceMonitor(
            threshold: 0.01f,
            windowSteps: 1);

        // Первое наблюдение только запоминает исходную потерю.
        Assert.False(monitor.ShouldStop(0.5f));
        // Второе наблюдение даёт лишь одно устойчивое сравнение.
        Assert.False(monitor.ShouldStop(0.5f));
        // Только третье наблюдение даёт два устойчивых сравнения подряд.
        Assert.True(monitor.ShouldStop(0.5f));
    }

    [Fact]
    public void Wrapper_AtZeroThresholdNeverReportsStopEvenForIdenticalLosses()
    {
        // Нулевой порог: сравнение "изменение меньше нуля" ложно даже при
        // полностью равных потерях, потому что ноль меньше нуля не бывает.
        var monitor = new DefaultLogicalCapabilityTrainer.ConvergenceMonitor(
            threshold: 0f,
            windowSteps: 1);

        // Двадцать одинаковых наблюдений подряд — и ни одного "пора".
        for (var observation = 1; observation <= 20; observation++)
            Assert.False(monitor.ShouldStop(0.5f));
    }

    [Fact]
    public void Wrapper_AtNegativeZeroThresholdIsEquallyDead()
    {
        // Отрицательный ноль проходит проверку настроек, потому что "меньше
        // нуля" для него ложно, и внутри монитора он равен нулю.
        var monitor = new DefaultLogicalCapabilityTrainer.ConvergenceMonitor(
            threshold: -0f,
            windowSteps: 1);

        // Поведение в точности как у обычного нуля: остановка невозможна.
        for (var observation = 1; observation <= 10; observation++)
            Assert.False(monitor.ShouldStop(0.5f));
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(3, 1)]
    [InlineData(1, 2)]
    [InlineData(5, 2)]
    [InlineData(5, 3)]
    [InlineData(5, 5)]
    [InlineData(1, 25)]
    [InlineData(5, 25)]
    public void TrainingLoop_NeverShortensRunWithinThreeObservationIntervals(
        int stepBudget,
        int windowSteps)
    {
        // Потеря не меняется вообще, то есть сходимость очевидна человеку.
        var executed = SimulateTrainingLoop(
            stepBudget,
            windowSteps,
            threshold: 0.01f,
            flatLoss: 0.5f);

        // Цикл всё равно доходит до конца бюджета: сокращать нечем.
        Assert.Equal(stepBudget, executed);
    }

    [Theory]
    [InlineData(4, 1)]
    [InlineData(5, 1)]
    public void TrainingLoop_ShortensRunOnlyAboveThreeObservationIntervals(
        int stepBudget,
        int windowSteps)
    {
        // Та же постоянная потеря при бюджете больше трёх интервалов.
        var executed = SimulateTrainingLoop(
            stepBudget,
            windowSteps,
            threshold: 0.01f,
            flatLoss: 0.5f);

        // Выход приходит ровно на третьем наблюдении и экономит шаги.
        Assert.Equal(3 * windowSteps, executed);
        Assert.True(executed < stepBudget);
    }

    [Fact]
    public void TrainingLoop_ExactlyTwoAllowedConfigurationsCanEverShortenTheRun()
    {
        // Бюджет шагов этой единицы жёстко ограничен живым контрактом.
        var minimumBudget = LogicalDpoTrainingUnit.MinimumSupportedMaxStepCount;
        var maximumBudget = LogicalDpoTrainingUnit.MaximumSupportedMaxStepCount;
        // Интервалы берём с большим запасом вокруг значения по умолчанию 25.
        const int maximumInterval = 30;
        // Сюда попадут все настройки, которые реально сократили прогон.
        var shortening = new List<(int Budget, int Interval)>();
        for (var budget = minimumBudget; budget <= maximumBudget; budget++)
        {
            for (var interval = 1; interval <= maximumInterval; interval++)
            {
                // Самый благоприятный для остановки случай: ровная потеря.
                var executed = SimulateTrainingLoop(
                    budget,
                    interval,
                    threshold: 0.002f,
                    flatLoss: 0.5f);
                // Сокращением считаем только фактическую экономию шагов.
                if (executed < budget)
                    shortening.Add((budget, interval));
            }
        }

        // Весь список исчерпывается двумя настройками, и обе с интервалом 1.
        Assert.Equal(
            new[] { (4, 1), (5, 1) },
            shortening.OrderBy(row => row.Budget).ToArray());
        // Отдельное утверждение по требованию: интервал от 2 и выше мертв
        // при любом допустимом бюджете этой единицы.
        Assert.DoesNotContain(shortening, row => row.Interval >= 2);
    }

    [Fact]
    public void UnreachabilityPredicate_AgreesWithSimulatedLoopOverWholeAllowedSpace()
    {
        // Границы бюджета берём из живого контракта единицы.
        var minimumBudget = LogicalDpoTrainingUnit.MinimumSupportedMaxStepCount;
        var maximumBudget = LogicalDpoTrainingUnit.MaximumSupportedMaxStepCount;
        // Интервалы с запасом, включая значение по умолчанию записи запроса.
        const int maximumInterval = 30;
        // Сколько пар настроек фактически сверено.
        var comparedRows = 0;
        for (var budget = minimumBudget; budget <= maximumBudget; budget++)
        {
            for (var interval = 1; interval <= maximumInterval; interval++)
            {
                // Наблюдаемое поведение: прогон не был сокращён ни на шаг.
                var executed = SimulateTrainingLoop(
                    budget,
                    interval,
                    threshold: 0.002f,
                    flatLoss: 0.5f);
                var loopNeverShortenedTheRun = executed == budget;
                // Признак обязан совпадать с этим наблюдаемым поведением.
                Assert.Equal(
                    loopNeverShortenedTheRun,
                    LogicalDpoTrainingUnit.ConvergenceDetectionIsUnreachable(
                        budget,
                        interval,
                        convergenceThreshold: 0.002f));
                comparedRows++;
            }
        }

        // Сверка действительно прошла по всему допустимому пространству.
        Assert.Equal((maximumBudget - minimumBudget + 1) * maximumInterval, comparedRows);
    }

    [Fact]
    public void ZeroThreshold_RepairIsObservationallyNeutralOverWholeAllowedSpace()
    {
        // Границы бюджета — те же живые константы контракта.
        var minimumBudget = LogicalDpoTrainingUnit.MinimumSupportedMaxStepCount;
        var maximumBudget = LogicalDpoTrainingUnit.MaximumSupportedMaxStepCount;
        const int maximumInterval = 30;
        for (var budget = minimumBudget; budget <= maximumBudget; budget++)
        {
            for (var interval = 1; interval <= maximumInterval; interval++)
            {
                // Прежний цикл создавал монитор всегда и всегда его спрашивал.
                var before = SimulateTrainingLoopBeforeRepair(
                    budget,
                    interval,
                    threshold: 0f,
                    flatLoss: 0.5f);
                // Нынешний цикл при нулевом пороге монитор не создаёт.
                var after = SimulateTrainingLoop(
                    budget,
                    interval,
                    threshold: 0f,
                    flatLoss: 0.5f);
                // Число выполненных шагов совпадает: правка ничего не меняет.
                Assert.Equal(before, after);
                // И в обоих случаях прогон доходит до конца бюджета.
                Assert.Equal(budget, after);
            }
        }
    }

    [Fact]
    public void TwoSignsAreIndependent_ZeroThresholdIsNeverReportedAsUnreachability()
    {
        // Настройка, которая без нуля была бы недостижимой.
        Assert.False(
            LogicalDpoTrainingUnit.ConvergenceDetectionIsUnreachable(
                maxStepCount: 1,
                convergenceWindowSteps: 25,
                convergenceThreshold: 0f));
        // И настройка, которая без нуля была бы вполне достижимой.
        Assert.False(
            LogicalDpoTrainingUnit.ConvergenceDetectionIsUnreachable(
                maxStepCount: 5,
                convergenceWindowSteps: 1,
                convergenceThreshold: 0f));
        // Обе относятся к признаку нуля, а не к признаку недостижимости.
        Assert.True(
            LogicalDpoTrainingUnit
                .ConvergenceDetectionIsDisabledByZeroThreshold(0f));
        // Отрицательный ноль — тот же случай по стандарту сравнения чисел.
        Assert.True(
            LogicalDpoTrainingUnit
                .ConvergenceDetectionIsDisabledByZeroThreshold(-0f));
        // Любой рабочий порог признаком нуля не считается.
        Assert.False(
            LogicalDpoTrainingUnit
                .ConvergenceDetectionIsDisabledByZeroThreshold(0.002f));
    }

    [Fact]
    public void UnreachabilityPredicate_DoesNotDependOnMonitorExistence()
    {
        // Признак обязан быть чистой функцией настройки, иначе после отказа от
        // создания монитора он молча исчез бы вместе с ним. Проверяем это по
        // самой подписи: среди параметров нет ни монитора, ни его типа.
        var predicate = typeof(LogicalDpoTrainingUnit).GetMethod(
            nameof(LogicalDpoTrainingUnit.ConvergenceDetectionIsUnreachable),
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.NotNull(predicate);
        // Все параметры — только числа настройки.
        Assert.Equal(
            new[] { typeof(int), typeof(int), typeof(float) },
            predicate!.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
    }

    [Fact]
    public void RecordOwnDefaultConfigurationIsAlreadyUnreachable()
    {
        // Значения по умолчанию читаем из живой записи запроса, а не из памяти.
        var primaryConstructor = typeof(LogicalDpoTrainingRequest)
            .GetConstructors()
            .Single();
        var parameters = primaryConstructor.GetParameters();
        var defaultBudget = (int)parameters
            .Single(parameter => parameter.Name == "MaxStepCount")
            .DefaultValue!;
        var defaultInterval = (int)parameters
            .Single(parameter => parameter.Name == "ConvergenceWindowSteps")
            .DefaultValue!;
        var defaultThreshold = (float)parameters
            .Single(parameter => parameter.Name == "ConvergenceThreshold")
            .DefaultValue!;

        // Порог по умолчанию рабочий, то есть это не случай нулевого порога.
        Assert.False(
            LogicalDpoTrainingUnit
                .ConvergenceDetectionIsDisabledByZeroThreshold(defaultThreshold));
        // А сама настройка по умолчанию уже недостижима.
        Assert.True(
            LogicalDpoTrainingUnit.ConvergenceDetectionIsUnreachable(
                defaultBudget,
                defaultInterval,
                defaultThreshold));
        // И цикл с этими значениями действительно проходит весь бюджет.
        Assert.Equal(
            defaultBudget,
            SimulateTrainingLoop(
                defaultBudget,
                defaultInterval,
                defaultThreshold,
                flatLoss: 0.5f));
    }

    [Fact]
    public void ZeroThresholdConfiguration_EmitsOnlyTheZeroWarning()
    {
        // Редкий бюджет, чтобы соседние тесты не могли выдать такой же след.
        const int stepBudget = 4447;
        var listener = new CapturingTraceListener();
        Trace.Listeners.Add(listener);
        try
        {
            // Тот же вызов, который делает продуктовый цикл перед первым шагом.
            LogicalDpoTrainingUnit.TraceConvergenceDetectionDiagnostics(
                stepBudget,
                convergenceWindowSteps: 25,
                convergenceThreshold: 0f);
            Trace.Flush();
        }
        finally
        {
            Trace.Listeners.Remove(listener);
        }

        // Ровно один наш след.
        var ours = listener.Captured
            .Where(message => message.Contains(
                stepBudget.ToString(CultureInfo.InvariantCulture),
                StringComparison.Ordinal))
            .ToArray();
        Assert.Single(ours);
        // Это именно признак нулевого порога.
        Assert.Contains("disabled by a zero threshold", ours[0], StringComparison.Ordinal);
        // И он не шумит про недостижимость по той же настройке.
        Assert.DoesNotContain("unreachable", ours[0], StringComparison.Ordinal);
    }

    [Fact]
    public void UnreachableConfiguration_EmitsOnlyTheUnreachabilityWarningWithTheHardCap()
    {
        // Редкий интервал, чтобы след нельзя было спутать с соседним тестом.
        const int stepBudget = 7;
        const int windowSteps = 8887;
        var listener = new CapturingTraceListener();
        Trace.Listeners.Add(listener);
        try
        {
            // Рабочий порог: говорить должен признак недостижимости.
            LogicalDpoTrainingUnit.TraceConvergenceDetectionDiagnostics(
                stepBudget,
                windowSteps,
                convergenceThreshold: 0.002f);
            Trace.Flush();
        }
        finally
        {
            Trace.Listeners.Remove(listener);
        }

        // Ровно один наш след.
        var ours = listener.Captured
            .Where(message => message.Contains(
                windowSteps.ToString(CultureInfo.InvariantCulture),
                StringComparison.Ordinal))
            .ToArray();
        Assert.Single(ours);
        // В следе видны оба числа настройки.
        Assert.Contains(
            stepBudget.ToString(CultureInfo.InvariantCulture),
            ours[0]);
        Assert.Contains("unreachable", ours[0], StringComparison.Ordinal);
        // И жёсткий потолок бюджета, из которого следует смертельность
        // интервала от 2 и выше для этой единицы.
        Assert.Contains(
            "capped at " +
            LogicalDpoTrainingUnit.MaximumSupportedMaxStepCount
                .ToString(CultureInfo.InvariantCulture) +
            " steps",
            ours[0],
            StringComparison.Ordinal);
        // И он не шумит про нулевой порог.
        Assert.DoesNotContain("zero threshold", ours[0], StringComparison.Ordinal);
    }

    [Fact]
    public void RealLiveCallerConfigurationWithDefaultIntervalIsReported()
    {
        // Так выглядят два боевых вызывающих из трёх: интервал они не передают
        // вовсе, поэтому берётся значение по умолчанию записи, а бюджет шагов
        // не может превысить жёсткий потолок единицы.
        var stepBudget = LogicalDpoTrainingUnit.MaximumSupportedMaxStepCount;
        const int defaultInterval = 25;
        var listener = new CapturingTraceListener();
        Trace.Listeners.Add(listener);
        try
        {
            // Тот же вызов, что в продуктовом цикле.
            LogicalDpoTrainingUnit.TraceConvergenceDetectionDiagnostics(
                stepBudget,
                defaultInterval,
                convergenceThreshold: 0.002f);
            Trace.Flush();
        }
        finally
        {
            Trace.Listeners.Remove(listener);
        }

        // Эта боевая настройка обязана получить признак недостижимости.
        var ours = listener.Captured
            .Where(message =>
                message.Contains("unreachable", StringComparison.Ordinal) &&
                message.Contains(
                    "intervals of " +
                    defaultInterval.ToString(CultureInfo.InvariantCulture) +
                    " steps",
                    StringComparison.Ordinal))
            .ToArray();
        Assert.Single(ours);
    }

    [Fact]
    public void ReachableConfiguration_EmitsNoDiagnosticWarning()
    {
        // Единственная живая достижимая раскладка этой единицы: интервал 1 и
        // бюджет строго больше трёх интервалов.
        const int windowSteps = 1;
        var stepBudget = LogicalDpoTrainingUnit.MaximumSupportedMaxStepCount;
        var listener = new CapturingTraceListener();
        Trace.Listeners.Add(listener);
        try
        {
            // Достижимая настройка не должна получать ложный сигнал.
            LogicalDpoTrainingUnit.TraceConvergenceDetectionDiagnostics(
                stepBudget,
                windowSteps,
                convergenceThreshold: 0.002f);
            Trace.Flush();
        }
        finally
        {
            Trace.Listeners.Remove(listener);
        }

        // Ни одного следа про сходимость в этом прогоне.
        Assert.DoesNotContain(
            listener.Captured,
            message => message.Contains(
                "convergence detection",
                StringComparison.Ordinal));
    }
}
