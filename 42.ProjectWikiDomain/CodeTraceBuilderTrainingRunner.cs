// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Diagnostics;
using AgentInfrastructure;

public sealed class CodeTraceBuilderTrainingRunner
{
    private readonly CodeTraceBuilderUnit _builder;

    public CodeTraceBuilderTrainingRunner(CodeTraceBuilderUnit? builder = null)
    {
        _builder = builder ?? new CodeTraceBuilderUnit();
    }

    public Task<OperationResult<CodeTraceBuilderTrainingRunResult>> TrainAndEvaluateAsync(
        IReadOnlyList<CodeTraceBuilderDpoPair> trainingRows,
        IReadOnlyList<CodeTraceBuilderDpoPair> heldOutRows,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<CodeTraceBuilderTrainingRunResult>.Fail("Training cancelled.", "CODE_TRACE_TRAINING_CANCELLED"));

        if (trainingRows is null)
            return Task.FromResult(OperationResult<CodeTraceBuilderTrainingRunResult>.Fail("Training rows are required.", "TRAINING_ROWS_REQUIRED"));

        if (heldOutRows is null)
            return Task.FromResult(OperationResult<CodeTraceBuilderTrainingRunResult>.Fail("Held-out rows are required.", "HELDOUT_ROWS_REQUIRED"));

        if (trainingRows.Count != 50)
            return Task.FromResult(OperationResult<CodeTraceBuilderTrainingRunResult>.Fail("Code-trace training batches must contain exactly 50 rows.", "TRAINING_BATCH_SIZE_NOT_50"));

        if (heldOutRows.Count != 50)
            return Task.FromResult(OperationResult<CodeTraceBuilderTrainingRunResult>.Fail("Code-trace held-out batches must contain exactly 50 rows.", "HELDOUT_BATCH_SIZE_NOT_50"));

        var validated = ValidateRows(trainingRows.Concat(heldOutRows));
        if (!validated.IsSuccess)
            return Task.FromResult(validated.MapError<CodeTraceBuilderTrainingRunResult>());

        var stopwatch = Stopwatch.StartNew();
        var process = Process.GetCurrentProcess();
        process.Refresh();
        var cpuBefore = process.TotalProcessorTime;
        var peakWorkingSetMb = BytesToMb(process.WorkingSet64);
        var cases = heldOutRows.Select(row =>
        {
            var analysis = _builder.Analyze(row.ProjectId, row.SourceId, row.SourceText).Value!;
            process.Refresh();
            peakWorkingSetMb = Math.Max(peakWorkingSetMb, BytesToMb(process.WorkingSet64));
            return new CodeTraceBuilderTrainingCaseResult(
                row.PairId,
                Recall(row.ExpectedMethods, analysis.Methods),
                Recall(row.ExpectedCallEdges, analysis.CallEdges),
                Recall(row.ExpectedValueFlows, analysis.ValueFlows),
                analysis.Confidence);
        }).ToArray();

        process.Refresh();
        stopwatch.Stop();
        var methodRecall = Average(cases.Select(item => item.MethodRecall));
        var callEdgeRecall = Average(cases.Select(item => item.CallEdgeRecall));
        var valueFlowRecall = Average(cases.Select(item => item.ValueFlowRecall));
        var wrongAnnotationRate = Average(trainingRows.Concat(heldOutRows).Select(row => row.Rejected.Contains("[WRONG:", StringComparison.Ordinal) ? 1.0 : 0.0));
        var confidence = Average(cases.Select(item => item.Confidence));
        var passed = methodRecall >= 0.98 &&
                     callEdgeRecall >= 0.95 &&
                     valueFlowRecall >= 0.95 &&
                     wrongAnnotationRate >= 1.0;
        var unitTrace = new WikiUnitTrace(
            "code-trace-builder-batch",
            trainingRows.Count,
            heldOutRows.Count,
            [
                new WikiUnitTraceResult(
                    CodeTraceBuilderUnit.UnitId,
                    "v1",
                    "code-trace-dpo-batch-gate",
                    $"{trainingRows.Count} train rows / {heldOutRows.Count} held-out rows",
                    $"method_recall {methodRecall:0.######}; call_edge_recall {callEdgeRecall:0.######}; value_flow_recall {valueFlowRecall:0.######}",
                    Math.Min(methodRecall, Math.Min(callEdgeRecall, valueFlowRecall)),
                    passed ? "PASS" : "FAIL",
                    passed ? "" : "code_trace_gate_failed",
                    passed ? "" : "Add focused 50-row remediation batch for method/call/value-flow trace gaps.")
            ],
            passed ? [] : [CodeTraceBuilderUnit.UnitId],
            passed ? [] : ["Add 50 code-trace rows for failed trace dimension."],
            0,
            !passed);

        return Task.FromResult(OperationResult<CodeTraceBuilderTrainingRunResult>.Ok(new CodeTraceBuilderTrainingRunResult(
            trainingRows.Count,
            heldOutRows.Count,
            methodRecall,
            callEdgeRecall,
            valueFlowRecall,
            wrongAnnotationRate,
            confidence,
            (process.TotalProcessorTime - cpuBefore).TotalMilliseconds,
            peakWorkingSetMb,
            stopwatch.Elapsed.TotalMilliseconds,
            passed ? "CODE_TRACE_BUILDER_GATE_PASSED" : "CODE_TRACE_BUILDER_GATE_FAILED",
            cases,
            unitTrace)));
    }

    private static OperationResult<bool> ValidateRows(IEnumerable<CodeTraceBuilderDpoPair> rows)
    {
        foreach (var row in rows)
        {
            if (string.Equals(row.Chosen.Trim(), row.Rejected.Trim(), StringComparison.Ordinal))
                return OperationResult<bool>.Fail($"chosen equals rejected for {row.PairId}.", "CHOSEN_EQUALS_REJECTED");

            if (!row.Rejected.Contains("[WRONG:", StringComparison.Ordinal))
                return OperationResult<bool>.Fail($"rejected must contain [WRONG:] for {row.PairId}.", "MISSING_WRONG_ANNOTATION");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static double Recall(string[] expected, string[] actual)
    {
        if (expected.Length == 0)
            return actual.Length == 0 ? 1.0 : 0.0;

        var actualSet = actual.ToHashSet(StringComparer.Ordinal);
        return expected.Count(actualSet.Contains) / (double)expected.Length;
    }

    private static double Average(IEnumerable<double> values)
    {
        var materialized = values.ToArray();
        return materialized.Length == 0 ? 0 : materialized.Average();
    }

    private static double BytesToMb(long bytes)
        => bytes / 1024.0 / 1024.0;
}
