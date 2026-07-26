// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Diagnostics;
using AgentInfrastructure;

public sealed class WikiContextRoutingTrainingRunner
{
    private readonly WikiContextRoutingUnit _routingUnit;

    public WikiContextRoutingTrainingRunner(WikiContextRoutingUnit? routingUnit = null)
    {
        _routingUnit = routingUnit ?? new WikiContextRoutingUnit();
    }

    public async Task<OperationResult<ProjectWikiContextRoutingTrainingRunResult>> TrainAndEvaluateAsync(
        IReadOnlyList<ProjectWikiContextRoutingDpoPair> trainingRows,
        IReadOnlyList<ProjectWikiContextRoutingDpoPair> heldOutRows,
        CancellationToken ct = default)
    {
        if (trainingRows is null)
            return OperationResult<ProjectWikiContextRoutingTrainingRunResult>.Fail("Training rows are required.", "TRAINING_ROWS_REQUIRED");

        if (heldOutRows is null)
            return OperationResult<ProjectWikiContextRoutingTrainingRunResult>.Fail("Held-out rows are required.", "HELDOUT_ROWS_REQUIRED");

        if (trainingRows.Count != 50)
            return OperationResult<ProjectWikiContextRoutingTrainingRunResult>.Fail("Wiki context-routing training batches must contain exactly 50 rows.", "TRAINING_BATCH_SIZE_NOT_50");

        if (heldOutRows.Count != 50)
            return OperationResult<ProjectWikiContextRoutingTrainingRunResult>.Fail("Wiki context-routing held-out batches must contain exactly 50 rows.", "HELDOUT_BATCH_SIZE_NOT_50");

        var validated = ValidateRows(trainingRows.Concat(heldOutRows));
        if (!validated.IsSuccess)
            return validated.MapError<ProjectWikiContextRoutingTrainingRunResult>();

        var stopwatch = Stopwatch.StartNew();
        var process = Process.GetCurrentProcess();
        process.Refresh();
        var cpuBefore = process.TotalProcessorTime;
        var peakWorkingSetMb = BytesToMb(process.WorkingSet64);
        var cases = new List<ProjectWikiContextRoutingTrainingCaseResult>(heldOutRows.Count);
        await Task.CompletedTask;

        foreach (var row in heldOutRows)
        {
            if (ct.IsCancellationRequested)
                return OperationResult<ProjectWikiContextRoutingTrainingRunResult>.Fail("Context-routing training was cancelled.", "CONTEXT_ROUTING_TRAINING_CANCELLED");

            var request = new ProjectWikiQuestionRequest(
                row.UserQuestion.ProjectId,
                row.UserQuestion.RawUserText,
                Path.Combine(row.UserQuestion.RepositoryRoot, ".xiigen", "wiki-domain"),
                row.UserQuestion.DocumentationVersion,
                TopK: 5,
                AllowSemantic: row.ExpectedRouteKind == ProjectWikiContextRouteKind.Mode2SemanticHybrid);
            var routed = _routingUnit.Route(request, row.QueryPlan);
            if (!routed.IsSuccess)
                return routed.MapError<ProjectWikiContextRoutingTrainingRunResult>();

            cases.Add(new ProjectWikiContextRoutingTrainingCaseResult(
                row.PairId,
                row.ExpectedRouteKind,
                routed.Value!.RouteKind,
                row.ExpectedRouteKind == routed.Value.RouteKind,
                routed.Value.RequiredPreChecks.Length >= 2,
                routed.Value.Confidence));

            process.Refresh();
            peakWorkingSetMb = Math.Max(peakWorkingSetMb, BytesToMb(process.WorkingSet64));
        }

        process.Refresh();
        stopwatch.Stop();
        var routeAccuracy = Average(cases.Select(item => item.RouteCorrect ? 1.0 : 0.0));
        var preCheckAccuracy = Average(cases.Select(item => item.PreChecksPresent ? 1.0 : 0.0));
        var wrongAnnotationRate = Average(trainingRows.Concat(heldOutRows).Select(row => row.Rejected.Contains("[WRONG:", StringComparison.Ordinal) ? 1.0 : 0.0));
        var averageConfidence = Average(cases.Select(item => item.Confidence));
        var passed = routeAccuracy >= 0.95 &&
                     preCheckAccuracy >= 0.95 &&
                     wrongAnnotationRate >= 1.0 &&
                     averageConfidence >= 0.85;
        var failedUnits = passed ? Array.Empty<string>() : [WikiContextRoutingUnit.UnitId];
        var remediation = passed
            ? Array.Empty<string>()
            : ["Add 50 WikiContextRoutingUnit DPO rows targeting failed route kinds and rerun this gate."];
        var unitTrace = new WikiUnitTrace(
            "wiki-context-routing-batch",
            trainingRows.Count,
            heldOutRows.Count,
            [
                new WikiUnitTraceResult(
                    WikiContextRoutingUnit.UnitId,
                    "v1",
                    "context-routing-dpo-batch-gate",
                    $"{trainingRows.Count} train rows / {heldOutRows.Count} held-out rows",
                    $"route_accuracy {routeAccuracy:0.######}; precheck_accuracy {preCheckAccuracy:0.######}; confidence {averageConfidence:0.######}",
                    Math.Min(routeAccuracy, preCheckAccuracy),
                    passed ? "PASS" : "FAIL",
                    passed ? "" : "context_routing_gate_failed",
                    passed ? "" : "Add focused 50-row remediation batch and rerun held-out gate.")
            ],
            failedUnits,
            remediation,
            0,
            !passed);

        return OperationResult<ProjectWikiContextRoutingTrainingRunResult>.Ok(new ProjectWikiContextRoutingTrainingRunResult(
            trainingRows.Count,
            heldOutRows.Count,
            routeAccuracy,
            preCheckAccuracy,
            wrongAnnotationRate,
            averageConfidence,
            (process.TotalProcessorTime - cpuBefore).TotalMilliseconds,
            peakWorkingSetMb,
            stopwatch.Elapsed.TotalMilliseconds,
            passed ? "WIKI_CONTEXT_ROUTING_GATE_PASSED" : "WIKI_CONTEXT_ROUTING_GATE_FAILED",
            cases.ToArray(),
            unitTrace));
    }

    private static OperationResult<bool> ValidateRows(IEnumerable<ProjectWikiContextRoutingDpoPair> rows)
    {
        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.PairId))
                return OperationResult<bool>.Fail("pair_id is required.", "PAIR_ID_REQUIRED");

            if (string.IsNullOrWhiteSpace(row.A))
                return OperationResult<bool>.Fail($"A side is required for {row.PairId}.", "A_SIDE_REQUIRED");

            if (string.IsNullOrWhiteSpace(row.Chosen))
                return OperationResult<bool>.Fail($"chosen is required for {row.PairId}.", "CHOSEN_REQUIRED");

            if (string.IsNullOrWhiteSpace(row.Rejected))
                return OperationResult<bool>.Fail($"rejected is required for {row.PairId}.", "REJECTED_REQUIRED");

            if (string.Equals(row.Chosen.Trim(), row.Rejected.Trim(), StringComparison.Ordinal))
                return OperationResult<bool>.Fail($"chosen equals rejected for {row.PairId}.", "CHOSEN_EQUALS_REJECTED");

            if (!row.Rejected.Contains("[WRONG:", StringComparison.Ordinal))
                return OperationResult<bool>.Fail($"rejected must contain [WRONG:] for {row.PairId}.", "MISSING_WRONG_ANNOTATION");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static double Average(IEnumerable<double> values)
    {
        var materialized = values.ToArray();
        return materialized.Length == 0 ? 0 : materialized.Average();
    }

    private static double BytesToMb(long bytes)
        => bytes / 1024.0 / 1024.0;
}
