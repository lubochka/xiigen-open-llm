// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using AgentInfrastructure;
using System.Diagnostics;

public sealed class OrchestrationEvaluationRunner
{
    private readonly OrchestrationAcceptanceGateEvaluator _gateEvaluator;

    public OrchestrationEvaluationRunner()
        : this(new OrchestrationAcceptanceGateEvaluator())
    {
    }

    public OrchestrationEvaluationRunner(OrchestrationAcceptanceGateEvaluator gateEvaluator)
    {
        _gateEvaluator = gateEvaluator;
    }

    public async Task<OperationResult<OrchestrationEvaluationRunReport>> RunPromptFileGateAsync(
        DistributedRoutingRuntime runtime,
        CancellationToken ct = default)
    {
        if (runtime is null)
            return OperationResult<OrchestrationEvaluationRunReport>.Fail("Distributed routing runtime is required.", "ROUTING_RUNTIME_REQUIRED");

        var process = Process.GetCurrentProcess();
        var cpuBefore = process.TotalProcessorTime;
        var memoryBefore = GC.GetTotalMemory(forceFullCollection: false);
        var stopwatch = Stopwatch.StartNew();

        var cases = OrchestrationRoutingCurriculum.CreatePromptFileAcceptanceCases();
        var gate = await _gateEvaluator.EvaluateAsync(runtime.Flow, cases, ct);
        if (!gate.IsSuccess)
            return gate.MapError<OrchestrationEvaluationRunReport>();

        stopwatch.Stop();
        process.Refresh();
        var cpuAfter = process.TotalProcessorTime;
        var memoryAfter = GC.GetTotalMemory(forceFullCollection: false);
        var metrics = new OrchestrationEvaluationMetrics(
            gate.Value!.ScenarioCount,
            gate.Value.RoutedCorrect,
            gate.Value.PhaseCorrect,
            gate.Value.RoutingAccuracy,
            gate.Value.PhaseCoverage,
            stopwatch.ElapsedMilliseconds,
            (cpuAfter - cpuBefore).TotalMilliseconds,
            memoryAfter - memoryBefore);

        var report = new OrchestrationEvaluationRunReport(
            $"orchestration-prompt-file-gate-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            OrchestrationRoutingCurriculum.V309TrainingRunId,
            OrchestrationRoutingCurriculum.V309DatasetSha256,
            metrics,
            gate.Value.Cases);

        return OperationResult<OrchestrationEvaluationRunReport>.Ok(report);
    }
}

public sealed record OrchestrationEvaluationRunReport(
    string RunId,
    string TrainingRunId,
    string SourceDatasetSha256,
    OrchestrationEvaluationMetrics Metrics,
    OrchestrationAcceptanceCaseResult[] Cases);

public sealed record OrchestrationEvaluationMetrics(
    int ScenarioCount,
    int RoutedCorrect,
    int PhaseCorrect,
    double RoutingAccuracy,
    double PhaseCoverage,
    long ElapsedMilliseconds,
    double CpuMilliseconds,
    long ManagedMemoryDeltaBytes);
