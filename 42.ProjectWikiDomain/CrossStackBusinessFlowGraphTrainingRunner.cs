// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Diagnostics;
using AgentInfrastructure;

public sealed class CrossStackBusinessFlowGraphTrainingRunner
{
    private readonly CrossStackBusinessFlowGraphUnit _unit;

    public CrossStackBusinessFlowGraphTrainingRunner(CrossStackBusinessFlowGraphUnit? unit = null)
    {
        _unit = unit ?? new CrossStackBusinessFlowGraphUnit();
    }

    public Task<OperationResult<CrossStackBusinessFlowGraphTrainingRunResult>> TrainAndEvaluateAsync(
        IReadOnlyList<CrossStackBusinessFlowGraphDpoPair> trainingRows,
        IReadOnlyList<CrossStackBusinessFlowGraphDpoPair> heldOutRows,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<CrossStackBusinessFlowGraphTrainingRunResult>.Fail("Training cancelled.", "CROSS_STACK_GRAPH_TRAINING_CANCELLED"));
        if (trainingRows is null)
            return Task.FromResult(OperationResult<CrossStackBusinessFlowGraphTrainingRunResult>.Fail("Training rows are required.", "TRAINING_ROWS_REQUIRED"));
        if (heldOutRows is null)
            return Task.FromResult(OperationResult<CrossStackBusinessFlowGraphTrainingRunResult>.Fail("Held-out rows are required.", "HELDOUT_ROWS_REQUIRED"));
        if (trainingRows.Count != 50)
            return Task.FromResult(OperationResult<CrossStackBusinessFlowGraphTrainingRunResult>.Fail("Cross-stack graph training batches must contain exactly 50 rows.", "TRAINING_BATCH_SIZE_NOT_50"));
        if (heldOutRows.Count != 50)
            return Task.FromResult(OperationResult<CrossStackBusinessFlowGraphTrainingRunResult>.Fail("Cross-stack graph held-out batches must contain exactly 50 rows.", "HELDOUT_BATCH_SIZE_NOT_50"));

        var validated = ValidateRows(trainingRows.Concat(heldOutRows));
        if (!validated.IsSuccess)
            return Task.FromResult(validated.MapError<CrossStackBusinessFlowGraphTrainingRunResult>());

        var stopwatch = Stopwatch.StartNew();
        var process = Process.GetCurrentProcess();
        process.Refresh();
        var cpuBefore = process.TotalProcessorTime;
        var peakWorkingSetMb = BytesToMb(process.WorkingSet64);
        var cases = heldOutRows.Select(row =>
        {
            var graph = _unit.Build(ResolveGraphId(row), row.BoundaryMap).Value!;
            process.Refresh();
            peakWorkingSetMb = Math.Max(peakWorkingSetMb, BytesToMb(process.WorkingSet64));
            return new CrossStackBusinessFlowGraphTrainingCaseResult(
                row.PairId,
                Recall(row.RequiredPathIds, graph.ClientToStoredProcedurePaths),
                graph.ClientServerEdgeRecall,
                graph.RepositoryToStoredProcedureEdgeRecall,
                Recall(row.EvidenceIds, graph.Nodes.SelectMany(node => node.EvidenceIds).Concat(graph.Edges.SelectMany(edge => edge.EvidenceIds)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()),
                graph.ClientToStoredProcedurePathRecall);
        }).ToArray();

        process.Refresh();
        stopwatch.Stop();
        var pathRecall = Average(cases.Select(item => item.ClientToStoredProcedurePathRecall));
        var clientServerRecall = Average(cases.Select(item => item.ClientServerEdgeRecall));
        var storedProcedureRecall = Average(cases.Select(item => item.StoredProcedureEdgeRecall));
        var evidenceGroundingRate = Average(cases.Select(item => item.EvidenceGroundingRate));
        var wrongAnnotationRate = Average(trainingRows.Concat(heldOutRows).Select(row => row.Rejected.Contains("[WRONG:", StringComparison.Ordinal) ? 1.0 : 0.0));
        var passed = pathRecall >= 0.95 &&
                     clientServerRecall >= 0.85 &&
                     storedProcedureRecall >= 0.85 &&
                     evidenceGroundingRate >= 0.98 &&
                     wrongAnnotationRate >= 1.0;
        var unitTrace = new WikiUnitTrace(
            "cross-stack-business-flow-graph-batch",
            trainingRows.Count,
            heldOutRows.Count,
            [
                new WikiUnitTraceResult(
                    CrossStackBusinessFlowGraphUnit.UnitId,
                    "v1",
                    "cross-stack-business-flow-graph-dpo-batch-gate",
                    $"{trainingRows.Count} train rows / {heldOutRows.Count} held-out rows",
                    $"path_recall {pathRecall:0.######}; client_server_edge_recall {clientServerRecall:0.######}; stored_procedure_edge_recall {storedProcedureRecall:0.######}; evidence_grounding_rate {evidenceGroundingRate:0.######}",
                    Math.Min(pathRecall, Math.Min(clientServerRecall, Math.Min(storedProcedureRecall, evidenceGroundingRate))),
                    passed ? "PASS" : "FAIL",
                    passed ? "" : "cross_stack_business_flow_graph_gate_failed",
                    passed ? "" : "Add focused 50-row remediation batch for missing Angular to .NET to stored procedure graph paths.")
            ],
            passed ? [] : [CrossStackBusinessFlowGraphUnit.UnitId],
            passed ? [] : ["Add 50 cross-stack business-flow rows for failed path/evidence dimension."],
            0,
            !passed);

        return Task.FromResult(OperationResult<CrossStackBusinessFlowGraphTrainingRunResult>.Ok(new CrossStackBusinessFlowGraphTrainingRunResult(
            trainingRows.Count,
            heldOutRows.Count,
            pathRecall,
            clientServerRecall,
            storedProcedureRecall,
            evidenceGroundingRate,
            wrongAnnotationRate,
            (process.TotalProcessorTime - cpuBefore).TotalMilliseconds,
            peakWorkingSetMb,
            stopwatch.Elapsed.TotalMilliseconds,
            passed ? "CROSS_STACK_BUSINESS_FLOW_GRAPH_GATE_PASSED" : "CROSS_STACK_BUSINESS_FLOW_GRAPH_GATE_FAILED",
            cases,
            unitTrace)));
    }

    private static OperationResult<bool> ValidateRows(IEnumerable<CrossStackBusinessFlowGraphDpoPair> rows)
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

    private static string ResolveGraphId(CrossStackBusinessFlowGraphDpoPair row)
    {
        var firstPath = row.RequiredPathIds.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(firstPath))
            return row.PairId;

        var marker = firstPath.IndexOf(":angular-", StringComparison.OrdinalIgnoreCase);
        return marker > 0 ? firstPath[..marker] : row.PairId;
    }

    private static double Recall(string[] expected, string[] actual)
    {
        if (expected.Length == 0)
            return actual.Length == 0 ? 1.0 : 0.0;

        var actualSet = actual.ToHashSet(StringComparer.OrdinalIgnoreCase);
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
