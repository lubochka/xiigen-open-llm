// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TensorCore.Core;

using System.Diagnostics;

// Снимок измерителя участка правого градиента: сколько раз участок вызван,
// сколько тиков занял и сколько умножений-сложений выполнил.
public sealed record GradBSnapshot(long Calls, long ElapsedTicks, long MultiplyAddCount, long StopwatchFrequency);

// Общая измерительная обвязка обоих плеч опыта. Она одинакова в A и B и сама
// не является фактором: считает только вызовы участка, его время и объём работы.
public static class GradBExperimentTelemetry
{
    private static long _calls;
    private static long _elapsedTicks;
    private static long _multiplyAddCount;

    // Обнуление перед каждым шагом обучения.
    public static void Reset()
    {
        Interlocked.Exchange(ref _calls, 0);
        Interlocked.Exchange(ref _elapsedTicks, 0);
        Interlocked.Exchange(ref _multiplyAddCount, 0);
    }

    // Начало интервала участка.
    public static long Begin() => Stopwatch.GetTimestamp();

    // Конец интервала: прибавить один вызов, затраченные тики и объём работы.
    public static void End(long startTimestamp, long multiplyAdds)
    {
        Interlocked.Add(ref _elapsedTicks, Stopwatch.GetTimestamp() - startTimestamp);
        Interlocked.Increment(ref _calls);
        Interlocked.Add(ref _multiplyAddCount, multiplyAdds);
    }

    // Чтение накопленных значений после шага обучения.
    public static GradBSnapshot Snapshot()
        => new(
            Interlocked.Read(ref _calls),
            Interlocked.Read(ref _elapsedTicks),
            Interlocked.Read(ref _multiplyAddCount),
            Stopwatch.Frequency);
}
