// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Diagnostics;
using AgentInfrastructure;

public sealed class WikiTraceAggregatorTrainingRunner
{
    private readonly WikiTraceAggregatorUnit _aggregator;

    public WikiTraceAggregatorTrainingRunner(WikiTraceAggregatorUnit? aggregator = null)
    {
        _aggregator = aggregator ?? new WikiTraceAggregatorUnit();
    }

    public Task<OperationResult<WikiTraceAggregatorTrainingRunResult>> TrainAndEvaluateAsync(
        IReadOnlyList<WikiTraceAggregatorDpoPair> trainingRows,
        IReadOnlyList<WikiTraceAggregatorDpoPair> heldOutRows,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<WikiTraceAggregatorTrainingRunResult>.Fail("Training cancelled.", "TRACE_AGGREGATOR_TRAINING_CANCELLED"));

        if (trainingRows is null)
            return Task.FromResult(OperationResult<WikiTraceAggregatorTrainingRunResult>.Fail("Training rows are required.", "TRAINING_ROWS_REQUIRED"));

        if (heldOutRows is null)
            return Task.FromResult(OperationResult<WikiTraceAggregatorTrainingRunResult>.Fail("Held-out rows are required.", "HELDOUT_ROWS_REQUIRED"));

        if (trainingRows.Count != 50)
            return Task.FromResult(OperationResult<WikiTraceAggregatorTrainingRunResult>.Fail("Trace-aggregator training batches must contain exactly 50 rows.", "TRAINING_BATCH_SIZE_NOT_50"));

        if (heldOutRows.Count != 50)
            return Task.FromResult(OperationResult<WikiTraceAggregatorTrainingRunResult>.Fail("Trace-aggregator held-out batches must contain exactly 50 rows.", "HELDOUT_BATCH_SIZE_NOT_50"));

        var validated = ValidateRows(trainingRows.Concat(heldOutRows));
        if (!validated.IsSuccess)
            return Task.FromResult(validated.MapError<WikiTraceAggregatorTrainingRunResult>());

        var stopwatch = Stopwatch.StartNew();
        var process = Process.GetCurrentProcess();
        process.Refresh();
        var cpuBefore = process.TotalProcessorTime;
        var peakWorkingSetMb = BytesToMb(process.WorkingSet64);
        var cases = heldOutRows.Select(row =>
        {
            var result = _aggregator.Aggregate(row.PairId, row.QueryTraces, row.UnitTraces).Value!;
            process.Refresh();
            peakWorkingSetMb = Math.Max(peakWorkingSetMb, BytesToMb(process.WorkingSet64));
            return new WikiTraceAggregatorTrainingCaseResult(
                row.PairId,
                SetEquals(result.FailedUnits, row.ExpectedFailedUnits),
                SetEquals(result.RemediationActions, row.ExpectedRemediationActions),
                result.StopBlindTraining == row.ExpectedStopBlindTraining,
                result.Confidence);
        }).ToArray();

        process.Refresh();
        stopwatch.Stop();
        var failedUnitAccuracy = Average(cases.Select(item => item.FailedUnitsCorrect ? 1.0 : 0.0));
        var remediationAccuracy = Average(cases.Select(item => item.RemediationCorrect ? 1.0 : 0.0));
        var stopAccuracy = Average(cases.Select(item => item.StopDecisionCorrect ? 1.0 : 0.0));
        var wrongAnnotationRate = Average(trainingRows.Concat(heldOutRows).Select(row => row.Rejected.Contains("[WRONG:", StringComparison.Ordinal) ? 1.0 : 0.0));
        var confidence = Average(cases.Select(item => item.Confidence));
        var passed = failedUnitAccuracy >= 0.98 &&
                     remediationAccuracy >= 0.98 &&
                     stopAccuracy >= 0.98 &&
                     wrongAnnotationRate >= 1.0;
        var unitTrace = new WikiUnitTrace(
            "wiki-trace-aggregator-batch",
            trainingRows.Count,
            heldOutRows.Count,
            [
                new WikiUnitTraceResult(
                    WikiTraceAggregatorUnit.UnitId,
                    "v1",
                    "trace-aggregator-dpo-batch-gate",
                    $"{trainingRows.Count} train rows / {heldOutRows.Count} held-out rows",
                    $"failed_unit_accuracy {failedUnitAccuracy:0.######}; remediation_accuracy {remediationAccuracy:0.######}; stop_accuracy {stopAccuracy:0.######}",
                    Math.Min(failedUnitAccuracy, Math.Min(remediationAccuracy, stopAccuracy)),
                    passed ? "PASS" : "FAIL",
                    passed ? "" : "trace_aggregator_gate_failed",
                    passed ? "" : "Add focused 50-row remediation batch for failed-unit/remediation/stop-decision trace aggregation.")
            ],
            passed ? [] : [WikiTraceAggregatorUnit.UnitId],
            passed ? [] : ["Add 50 trace-aggregator rows for failed aggregation dimension."],
            0,
            !passed);

        return Task.FromResult(OperationResult<WikiTraceAggregatorTrainingRunResult>.Ok(new WikiTraceAggregatorTrainingRunResult(
            trainingRows.Count,
            heldOutRows.Count,
            failedUnitAccuracy,
            remediationAccuracy,
            stopAccuracy,
            wrongAnnotationRate,
            confidence,
            (process.TotalProcessorTime - cpuBefore).TotalMilliseconds,
            peakWorkingSetMb,
            stopwatch.Elapsed.TotalMilliseconds,
            passed ? "WIKI_TRACE_AGGREGATOR_GATE_PASSED" : "WIKI_TRACE_AGGREGATOR_GATE_FAILED",
            cases,
            unitTrace)));
    }

    private static OperationResult<bool> ValidateRows(IEnumerable<WikiTraceAggregatorDpoPair> rows)
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

    private static bool SetEquals(string[] actual, string[] expected)
        => actual.ToHashSet(StringComparer.Ordinal).SetEquals(expected);

    private static double Average(IEnumerable<double> values)
    {
        var materialized = values.ToArray();
        return materialized.Length == 0 ? 0 : materialized.Average();
    }

    private static double BytesToMb(long bytes)
        => bytes / 1024.0 / 1024.0;
}
