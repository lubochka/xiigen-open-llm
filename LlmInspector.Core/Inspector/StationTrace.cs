// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmInspector.Inspector;

using System.Diagnostics;

// This record captures one lightweight machine-resource sample for a trace event.
public sealed record StationResourceSnapshot(
    double ProcessCpuPercent,
    double ProcessMemoryMb,
    double SystemMemoryUsedPercent,
    double SystemAvailableMemoryMb,
    DateTimeOffset CapturedAtUtc);

// This record captures one start or completion event for a logical runtime station.
public sealed record StationTrace(
    string TraceId,
    string? ParentTraceId,
    string RunId,
    int Depth,
    int WaveIndex,
    StationKind Kind,
    string StationName,
    string StationRole,
    IReadOnlyList<PropertyNode> Input,
    IReadOnlyList<PropertyNode>? Output,
    string? ErrorCode,
    string? ErrorMessage,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    StationResourceSnapshot? Resources = null);

// This sampler captures process CPU and memory without changing inspected behavior.
internal static class StationResourceSampler
{
    // This gate protects the previous CPU sample used for delta calculations.
    private static readonly object Gate = new();

    // The last observed total process CPU time is used to compute a percent delta.
    private static TimeSpan? LastProcessorTime;

    // The last capture time is used with the CPU delta.
    private static DateTimeOffset? LastCapturedAtUtc;

    // Capture reads a current resource snapshot for a station trace event.
    public static StationResourceSnapshot Capture()
    {
        // Read the current process so working set and CPU time come from the inspected app.
        using var process = Process.GetCurrentProcess();

        // Refresh process counters before reading them.
        process.Refresh();

        // Capture the current wall-clock timestamp.
        var now = DateTimeOffset.UtcNow;

        // Read the cumulative process CPU time.
        var processorTime = process.TotalProcessorTime;

        // Compute process CPU percent from the previous sample.
        var processCpuPercent = ComputeProcessCpuPercent(processorTime, now);

        // Convert working-set bytes to megabytes for the UI.
        var processMemoryMb = BytesToMegabytes(process.WorkingSet64);

        // Read runtime memory information as the safest cross-runtime system signal.
        var memoryInfo = GC.GetGCMemoryInfo();

        // Convert available memory bytes to megabytes for the UI.
        var systemAvailableMemoryMb = BytesToMegabytes(memoryInfo.TotalAvailableMemoryBytes);

        // Compute system memory pressure from the runtime memory load counters.
        var systemMemoryUsedPercent = ComputeSystemMemoryUsedPercent(memoryInfo);

        // Return the resource sample for the trace.
        return new StationResourceSnapshot(
            processCpuPercent,
            processMemoryMb,
            systemMemoryUsedPercent,
            systemAvailableMemoryMb,
            now);
    }

    // ComputeProcessCpuPercent calculates process CPU use between the last sample and this sample.
    private static double ComputeProcessCpuPercent(TimeSpan processorTime, DateTimeOffset now)
    {
        // Lock the sample state so simultaneous station traces do not corrupt the delta.
        lock (Gate)
        {
            // Return zero for the first sample because no prior delta exists.
            if (LastProcessorTime is null || LastCapturedAtUtc is null)
            {
                // Store the first CPU sample for the next event.
                LastProcessorTime = processorTime;

                // Store the first timestamp for the next event.
                LastCapturedAtUtc = now;

                // Return a calm first value.
                return 0;
            }

            // Compute elapsed CPU milliseconds since the previous sample.
            var cpuDeltaMs = (processorTime - LastProcessorTime.Value).TotalMilliseconds;

            // Compute elapsed wall-clock milliseconds since the previous sample.
            var elapsedMs = (now - LastCapturedAtUtc.Value).TotalMilliseconds;

            // Store this CPU sample for the next event.
            LastProcessorTime = processorTime;

            // Store this timestamp for the next event.
            LastCapturedAtUtc = now;

            // Return zero when events happen too close together to measure safely.
            if (elapsedMs <= 0)
                return 0;

            // Compute percent across all available processors.
            var percent = cpuDeltaMs / elapsedMs / Math.Max(1, Environment.ProcessorCount) * 100;

            // Clamp noisy values into a stable display range.
            return Math.Clamp(percent, 0, 100);
        }
    }

    // ComputeSystemMemoryUsedPercent calculates memory pressure from runtime memory counters.
    private static double ComputeSystemMemoryUsedPercent(GCMemoryInfo memoryInfo)
    {
        // Return zero when the runtime does not provide a usable total-memory value.
        if (memoryInfo.TotalAvailableMemoryBytes <= 0)
            return 0;

        // Compute the memory pressure percent from memory load over available memory.
        var percent = (double)memoryInfo.MemoryLoadBytes / memoryInfo.TotalAvailableMemoryBytes * 100;

        // Clamp noisy values into a stable display range.
        return Math.Clamp(percent, 0, 100);
    }

    // BytesToMegabytes converts bytes into a compact megabyte value.
    private static double BytesToMegabytes(long bytes)
    {
        // Divide by one mebibyte so memory values match common process tools.
        return bytes / 1024d / 1024d;
    }
}
