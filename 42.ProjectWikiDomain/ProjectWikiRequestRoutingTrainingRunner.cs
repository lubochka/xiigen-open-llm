// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Diagnostics;
using AgentInfrastructure;

public sealed class ProjectWikiRequestRoutingTrainingRunner
{
    public Task<OperationResult<ProjectWikiRequestRoutingTrainingRunResult>> TrainAndEvaluateAsync(
        IReadOnlyList<ProjectWikiRequestRoutingDpoPair> trainingRows,
        IReadOnlyList<ProjectWikiRequestRoutingDpoPair> heldOutRows,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<ProjectWikiRequestRoutingTrainingRunResult>.Fail("Training cancelled.", "REQUEST_ROUTING_TRAINING_CANCELLED"));

        if (trainingRows is null)
            return Task.FromResult(OperationResult<ProjectWikiRequestRoutingTrainingRunResult>.Fail("Training rows are required.", "TRAINING_ROWS_REQUIRED"));

        if (heldOutRows is null)
            return Task.FromResult(OperationResult<ProjectWikiRequestRoutingTrainingRunResult>.Fail("Held-out rows are required.", "HELDOUT_ROWS_REQUIRED"));

        if (trainingRows.Count != 50)
            return Task.FromResult(OperationResult<ProjectWikiRequestRoutingTrainingRunResult>.Fail("Request-routing training batches must contain exactly 50 rows.", "TRAINING_BATCH_SIZE_NOT_50"));

        if (heldOutRows.Count != 50)
            return Task.FromResult(OperationResult<ProjectWikiRequestRoutingTrainingRunResult>.Fail("Request-routing held-out batches must contain exactly 50 rows.", "HELDOUT_BATCH_SIZE_NOT_50"));

        var validated = ValidateRows(trainingRows.Concat(heldOutRows));
        if (!validated.IsSuccess)
            return Task.FromResult(validated.MapError<ProjectWikiRequestRoutingTrainingRunResult>());

        var stopwatch = Stopwatch.StartNew();
        var process = Process.GetCurrentProcess();
        process.Refresh();
        var cpuBefore = process.TotalProcessorTime;
        var peakWorkingSetMb = BytesToMb(process.WorkingSet64);
        var cases = new List<ProjectWikiRequestRoutingTrainingCaseResult>(heldOutRows.Count);
        var model = ProjectWikiRequestRoutingTrainableModel.Train(trainingRows);
        foreach (var row in heldOutRows)
        {
            var routed = model.RouteForEvaluation(row.Prompt);

            cases.Add(new ProjectWikiRequestRoutingTrainingCaseResult(
                row.PairId,
                row.ExpectedRouteKind,
                routed.RouteKind,
                row.ExpectedRouteKind == routed.RouteKind,
                routed.Confidence));
            process.Refresh();
            peakWorkingSetMb = Math.Max(peakWorkingSetMb, BytesToMb(process.WorkingSet64));
        }

        process.Refresh();
        stopwatch.Stop();
        var routeAccuracy = Average(cases.Select(item => item.RouteCorrect ? 1.0 : 0.0));
        var wrongAnnotationRate = Average(trainingRows.Concat(heldOutRows).Select(row => row.Rejected.Contains("[WRONG:", StringComparison.Ordinal) ? 1.0 : 0.0));
        var confidence = Average(cases.Select(item => item.Confidence));
        var heldOutGatePassed = routeAccuracy >= ProjectWikiRequestRoutingTrainableModel.MinimumHeldOutRouteAccuracy &&
                                wrongAnnotationRate >= ProjectWikiRequestRoutingTrainableModel.MinimumWrongAnnotationRate &&
                                confidence >= ProjectWikiRequestRoutingTrainableModel.MinimumAverageConfidence;
        var semanticQualityClaimed = false;
        var gateEvidence = new ProjectWikiRequestRoutingGateEvidence(
            $"project-wiki-request-routing-heldout-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            trainingRows.Count,
            heldOutRows.Count,
            routeAccuracy,
            wrongAnnotationRate,
            confidence,
            semanticQualityClaimed,
            heldOutGatePassed,
            heldOutGatePassed && semanticQualityClaimed,
            ProjectWikiRequestRoutingTrainableModel.MinimumHeldOutRouteAccuracy,
            ProjectWikiRequestRoutingTrainableModel.MinimumWrongAnnotationRate,
            ProjectWikiRequestRoutingTrainableModel.MinimumAverageConfidence,
            ProjectWikiRequestRoutingTrainableModel.MinimumTopTwoMargin,
            ProjectWikiRequestRoutingTrainableModel.MinimumSelectionConfidence,
            heldOutGatePassed
                ? ["semantic_quality_claimed=false_publication_blocked_until_semantic_gate_exists"]
                : ["request_routing_heldout_gate_failed", "semantic_quality_claimed=false_publication_blocked_until_semantic_gate_exists"]);
        var unitTrace = new WikiUnitTrace(
            "project-wiki-request-routing-batch",
            trainingRows.Count,
            heldOutRows.Count,
            [
                new WikiUnitTraceResult(
                    ProjectWikiRequestRouterUnit.UnitId,
                    "v1",
                    gateEvidence.EvaluationId,
                    $"{trainingRows.Count} train rows / {heldOutRows.Count} held-out rows",
                    $"route_accuracy {routeAccuracy:0.######}; wrong_annotation_rate {wrongAnnotationRate:0.######}; confidence {confidence:0.######}; semantic_quality_claimed=false",
                    routeAccuracy,
                    heldOutGatePassed ? "PASS" : "FAIL",
                    heldOutGatePassed ? "" : "request_routing_gate_failed",
                    heldOutGatePassed ? "" : "Add focused 50-row remediation batch and rerun held-out gate.")
            ],
            heldOutGatePassed ? [] : [ProjectWikiRequestRouterUnit.UnitId],
            heldOutGatePassed ? [] : ["Add 50 ProjectWikiRequestRouterUnit rows for failed prompt types."],
            0,
            !heldOutGatePassed);

        return Task.FromResult(OperationResult<ProjectWikiRequestRoutingTrainingRunResult>.Ok(new ProjectWikiRequestRoutingTrainingRunResult(
            trainingRows.Count,
            heldOutRows.Count,
            routeAccuracy,
            wrongAnnotationRate,
            confidence,
            (process.TotalProcessorTime - cpuBefore).TotalMilliseconds,
            peakWorkingSetMb,
            stopwatch.Elapsed.TotalMilliseconds,
            heldOutGatePassed ? "PROJECT_WIKI_REQUEST_ROUTING_GATE_PASSED" : "PROJECT_WIKI_REQUEST_ROUTING_GATE_FAILED",
            cases.ToArray(),
            unitTrace,
            model.WithGateEvidence(gateEvidence).ToSnapshot(gateEvidence),
            gateEvidence)));
    }

    private static OperationResult<bool> ValidateRows(IEnumerable<ProjectWikiRequestRoutingDpoPair> rows)
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
