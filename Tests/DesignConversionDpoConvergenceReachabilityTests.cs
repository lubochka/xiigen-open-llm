// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Diagnostics;
using System.Globalization;
using DesignConversionDomain;
using DPO;

namespace LlmFromScratch.Tests;

// Проверяет, что обнаружение сходимости DPO (Direct Preference Optimization)
// больше не выключается молча: конфигурация с бюджетом шагов меньше трёх
// интервалов наблюдения получает видимый признак недостижимости, а достижимая
// конфигурация не получает ложного сигнала. Числа в тестах выведены из
// фактического поведения монитора, а не из константы в коде.
public class DesignConversionDpoConvergenceReachabilityTests
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

    // Воспроизводит ровно условие продуктового цикла моста: монитор
    // спрашивают только на шагах, кратных интервалу наблюдения.
    private static int SimulateBridgeLoop(
        int stepBudget,
        int windowSteps,
        float threshold,
        float flatLoss)
    {
        // Монитор создаётся так же, как в ExecuteDpoStepLoop.
        var monitor = threshold == 0f
            ? null
            : new DpoConvergenceMonitor(threshold, windowSteps);
        // Сколько шагов цикл фактически выполнил до выхода.
        var executed = 0;
        for (var step = 1; step <= stepBudget; step++)
        {
            // Каждый шаг считается выполненным до проверки сходимости.
            executed = step;
            // Та же тройная проверка, что и в продуктовом цикле.
            if (monitor is not null &&
                step % windowSteps == 0 &&
                monitor.ShouldStop(flatLoss))
            {
                break;
            }
        }

        return executed;
    }

    [Fact]
    public void Monitor_CannotReportStopBeforeThirdObservation()
    {
        // Полностью постоянная потеря — самый благоприятный случай для
        // остановки: если монитор не может ответить "пора" даже здесь, значит
        // он не может ответить "пора" раньше третьего наблюдения вообще.
        var monitor = new DpoConvergenceMonitor(threshold: 0.01f, windowSteps: 1);

        // Первое наблюдение только запоминает исходную потерю.
        Assert.False(monitor.ShouldStop(0.5f));
        // Второе наблюдение даёт лишь одно устойчивое сравнение.
        Assert.False(monitor.ShouldStop(0.5f));
        // Только третье наблюдение даёт два устойчивых сравнения подряд.
        Assert.True(monitor.ShouldStop(0.5f));
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(3, 1)]
    [InlineData(25, 25)]
    [InlineData(50, 25)]
    [InlineData(74, 25)]
    [InlineData(75, 25)]
    public void BridgeLoop_NeverShortensRunUpToThreeObservationIntervals(
        int stepBudget,
        int windowSteps)
    {
        // Потеря не меняется вообще, то есть сходимость очевидна человеку.
        var executed = SimulateBridgeLoop(
            stepBudget,
            windowSteps,
            threshold: 0.01f,
            flatLoss: 0.5f);

        // Цикл всё равно доходит до конца бюджета: сократить прогон нечем.
        Assert.Equal(stepBudget, executed);
    }

    [Theory]
    [InlineData(4, 1)]
    [InlineData(76, 25)]
    [InlineData(100, 25)]
    public void BridgeLoop_ShortensRunOnlyAboveThreeObservationIntervals(
        int stepBudget,
        int windowSteps)
    {
        // Та же постоянная потеря при бюджете больше трёх интервалов.
        var executed = SimulateBridgeLoop(
            stepBudget,
            windowSteps,
            threshold: 0.01f,
            flatLoss: 0.5f);

        // Выход приходит ровно на третьем наблюдении и реально экономит шаги.
        Assert.Equal(3 * windowSteps, executed);
        Assert.True(executed < stepBudget);
    }

    [Fact]
    public void BridgeLoop_AtExactlyThreeIntervalsDetectsOnLastStepAndSavesNothing()
    {
        // Ровно три интервала наблюдения при интервале по умолчанию.
        const int windowSteps = 25;
        const int stepBudget = 3 * windowSteps;

        // Монитор, которого спросили три раза, сходимость всё-таки находит.
        var monitor = new DpoConvergenceMonitor(
            threshold: 0.01f,
            windowSteps: windowSteps);
        Assert.False(monitor.ShouldStop(0.5f));
        Assert.False(monitor.ShouldStop(0.5f));
        Assert.True(monitor.ShouldStop(0.5f));

        // Но в цикле это третье наблюдение совпадает с последним шагом бюджета,
        // поэтому ни одного шага не сэкономлено - именно поэтому такая настройка
        // тоже считается недостижимой и получает предупреждение.
        Assert.Equal(
            stepBudget,
            SimulateBridgeLoop(
                stepBudget,
                windowSteps,
                threshold: 0.01f,
                flatLoss: 0.5f));
        Assert.True(
            DesignConversionDpoPolicyCheckpointBridge
                .ConvergenceDetectionIsUnreachable(
                    stepBudget,
                    windowSteps,
                    convergenceThreshold: 0.01f));
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(3, 1)]
    [InlineData(4, 1)]
    [InlineData(24, 25)]
    [InlineData(25, 25)]
    [InlineData(50, 25)]
    [InlineData(74, 25)]
    [InlineData(75, 25)]
    [InlineData(100, 25)]
    public void UnreachabilityPredicate_AgreesWithSimulatedLoopBehaviour(
        int stepBudget,
        int windowSteps)
    {
        // Фактическое поведение цикла при полностью постоянной потере.
        var executed = SimulateBridgeLoop(
            stepBudget,
            windowSteps,
            threshold: 0.01f,
            flatLoss: 0.5f);
        // Прогон не сократился даже при идеально ровной потере — значит проверка
        // ничего не решает и настройка недостижима.
        var simulationNeverShortenedTheRun = executed == stepBudget;

        // Признак обязан совпасть с измеренным поведением на каждой паре чисел.
        Assert.Equal(
            simulationNeverShortenedTheRun,
            DesignConversionDpoPolicyCheckpointBridge
                .ConvergenceDetectionIsUnreachable(
                    stepBudget,
                    windowSteps,
                    convergenceThreshold: 0.01f));
    }

    [Fact]
    public void UnreachabilityPredicate_TreatsZeroThresholdAsIntentionalDisable()
    {
        // Порог 0 отключает проверку намеренно: это не скрытый дефект.
        Assert.False(
            DesignConversionDpoPolicyCheckpointBridge
                .ConvergenceDetectionIsUnreachable(
                    dpoStepCount: 1,
                    convergenceWindowSteps: 25,
                    convergenceThreshold: 0f));
    }

    [Fact]
    public void UnreachabilityPredicate_SurvivesVeryLargeObservationInterval()
    {
        // Трёхкратный интервал не должен переполнить целое число и случайно
        // превратить недостижимую настройку в достижимую.
        Assert.True(
            DesignConversionDpoPolicyCheckpointBridge
                .ConvergenceDetectionIsUnreachable(
                    dpoStepCount: int.MaxValue,
                    convergenceWindowSteps: int.MaxValue,
                    convergenceThreshold: 0.002f));
    }

    [Fact]
    public void UnreachableConfiguration_EmitsOneDiagnosticWarningWithBothNumbers()
    {
        // Редкие числа, чтобы соседние тесты не могли выдать такой же след.
        const int stepBudget = 7;
        const int windowSteps = 9991;
        var listener = new CapturingTraceListener();
        Trace.Listeners.Add(listener);
        try
        {
            // Тот же вызов, который делает продуктовый цикл перед первым шагом.
            DesignConversionDpoPolicyCheckpointBridge
                .TraceUnreachableConvergenceDetection(
                    stepBudget,
                    windowSteps,
                    convergenceThreshold: 0.002f);
            Trace.Flush();
        }
        finally
        {
            Trace.Listeners.Remove(listener);
        }

        // Ровно один след с нашим интервалом наблюдения.
        var ours = listener.Captured
            .Where(message => message.Contains(
                windowSteps.ToString(CultureInfo.InvariantCulture),
                StringComparison.Ordinal))
            .ToArray();
        Assert.Single(ours);
        // В следе видны оба числа, из которых следует недостижимость.
        Assert.Contains(
            stepBudget.ToString(CultureInfo.InvariantCulture),
            ours[0]);
        Assert.Contains("unreachable", ours[0], StringComparison.Ordinal);
    }

    [Fact]
    public void ReachableConfiguration_EmitsNoDiagnosticWarning()
    {
        // Редкие числа достижимой настройки: три интервала это 993 шага, а бюджет
        // 1000 строго больше, поэтому выход на шаге 993 экономит 7 шагов.
        const int stepBudget = 1000;
        const int windowSteps = 331;
        var listener = new CapturingTraceListener();
        Trace.Listeners.Add(listener);
        try
        {
            // Достижимая настройка не должна получать ложный сигнал.
            DesignConversionDpoPolicyCheckpointBridge
                .TraceUnreachableConvergenceDetection(
                    stepBudget,
                    windowSteps,
                    convergenceThreshold: 0.002f);
            Trace.Flush();
        }
        finally
        {
            Trace.Listeners.Remove(listener);
        }

        // Ни одного следа с нашим интервалом наблюдения.
        Assert.DoesNotContain(
            listener.Captured,
            message => message.Contains(
                windowSteps.ToString(CultureInfo.InvariantCulture),
                StringComparison.Ordinal));
    }
}
