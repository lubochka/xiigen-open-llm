// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E41: IdleDetector — безопасно ли начать автономное обучение.
// Baseline:  одно условие — cpu_usage < 20%
// Enhanced:  все 4 условия (CPU + input + battery + disk)
// Метрика:   idle_accuracy + false_start_rate (дорогая ошибка)
public sealed class E41IdleDetector
{
    public const int SAFE_TO_TRAIN = 1;
    public const int WAIT          = 0;

    public sealed record SystemState(
        float CpuUsagePct,
        int   InputIdleMinutes,
        float BatteryPct,
        bool  IsCharging,
        float DiskFreeGb,
        bool  ActuallyIdle);

    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var states = GenerateStates(seed: 42, n: tier * 80);
        var rng    = new Random(42);
        var val    = states.OrderBy(_ => rng.Next()).Skip((int)(states.Count * 0.8)).ToList();

        // ── Baseline: только CPU < 20% ────────────────────────────────────────
        var t0 = Stopwatch();
        var basePreds     = val.Select(s => s.CpuUsagePct < 20f ? SAFE_TO_TRAIN : WAIT).ToList();
        int baseOk        = val.Zip(basePreds).Count(p => (p.First.ActuallyIdle ? 1 : 0) == p.Second);
        int baseFalseStart = val.Zip(basePreds)
            .Count(p => p.Second == SAFE_TO_TRAIN && !p.First.ActuallyIdle);
        int notIdleCount  = val.Count(s => !s.ActuallyIdle);
        double baseAcc    = baseOk / (double)val.Count;
        double baseFSR    = notIdleCount > 0 ? baseFalseStart / (double)notIdleCount : 0;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: все 4 условия ───────────────────────────────────────────
        var t1 = Stopwatch();
        var enhPreds   = val.Select(PredictFourConditions).ToList();
        int enhOk      = val.Zip(enhPreds).Count(p => (p.First.ActuallyIdle ? 1 : 0) == p.Second);
        int enhFalseStart = val.Zip(enhPreds)
            .Count(p => p.Second == SAFE_TO_TRAIN && !p.First.ActuallyIdle);
        int enhFalseIdle  = val.Zip(enhPreds)
            .Count(p => p.Second == WAIT && p.First.ActuallyIdle);
        double enhAcc     = enhOk / (double)val.Count;
        double enhFSR     = notIdleCount > 0 ? enhFalseStart / (double)notIdleCount : 0;
        int idleCount     = val.Count(s => s.ActuallyIdle);
        double enhFIR     = idleCount > 0 ? enhFalseIdle / (double)idleCount : 0;
        double enhancedMs = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E41-IDLE-DETECTOR",
            Tier              = tier,
            BaselinePrimary   = baseAcc,
            EnhancedPrimary   = enhAcc,
            PrimaryMetricName = "idle_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = baseFSR - enhFSR,
            ScalingSignalNote = "false_start_reduction (батарея не садится зря)",
            ShouldScaleUp     = val.Count < 20,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"val={val.Count}, baseAcc={baseAcc:P1}(FSR={baseFSR:P1}), " +
                    $"enhAcc={enhAcc:P1}(FSR={enhFSR:P1},FIR={enhFIR:P1})"
        });
    }

    private static int PredictFourConditions(SystemState s)
    {
        bool cpuOk     = s.CpuUsagePct < 20f;
        bool inputOk   = s.InputIdleMinutes >= 10;
        bool batteryOk = s.IsCharging || s.BatteryPct >= 50f;
        bool diskOk    = s.DiskFreeGb >= 2.0f;
        return (cpuOk && inputOk && batteryOk && diskOk) ? SAFE_TO_TRAIN : WAIT;
    }

    private static List<SystemState> GenerateStates(int seed, int n)
    {
        var rng = new Random(seed);
        var templates = new SystemState[]
        {
            new(5f,  30, 80f, false, 10f, true),
            new(3f,  60, 95f, true,  5f,  true),
            new(10f, 15, 55f, true,  3f,  true),
            new(15f, 45, 70f, false, 8f,  true),
            new(85f, 30, 80f, false, 10f, false),
            new(60f, 60, 90f, true,  5f,  false),
            new(45f, 0,  75f, false, 6f,  false),
            new(5f,  0,  80f, false, 10f, false),
            new(8f,  3,  70f, true,  5f,  false),
            new(5f,  30, 15f, false, 10f, false),
            new(10f, 20, 25f, false, 8f,  false),
            new(5f,  30, 80f, false, 0.5f, false),
            new(8f,  60, 90f, true,  1.2f, false),
            new(18f, 5,  30f, false, 0.8f, false),
            new(19f, 8,  45f, false, 1.5f, false),
        };
        var result = new List<SystemState>();
        while (result.Count < n)
            result.Add(templates[rng.Next(templates.Length)]);
        return result;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
