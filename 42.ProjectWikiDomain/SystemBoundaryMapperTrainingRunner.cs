// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Diagnostics;
using AgentInfrastructure;

public sealed class SystemBoundaryMapperTrainingRunner
{
    private readonly SystemBoundaryMapperUnit _mapper;

    public SystemBoundaryMapperTrainingRunner(SystemBoundaryMapperUnit? mapper = null)
    {
        _mapper = mapper ?? new SystemBoundaryMapperUnit();
    }

    public Task<OperationResult<SystemBoundaryMapperTrainingRunResult>> TrainAndEvaluateAsync(
        IReadOnlyList<SystemBoundaryMapperDpoPair> trainingRows,
        IReadOnlyList<SystemBoundaryMapperDpoPair> heldOutRows,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<SystemBoundaryMapperTrainingRunResult>.Fail("Training cancelled.", "SYSTEM_BOUNDARY_TRAINING_CANCELLED"));

        if (trainingRows is null)
            return Task.FromResult(OperationResult<SystemBoundaryMapperTrainingRunResult>.Fail("Training rows are required.", "TRAINING_ROWS_REQUIRED"));

        if (heldOutRows is null)
            return Task.FromResult(OperationResult<SystemBoundaryMapperTrainingRunResult>.Fail("Held-out rows are required.", "HELDOUT_ROWS_REQUIRED"));

        if (trainingRows.Count != 50)
            return Task.FromResult(OperationResult<SystemBoundaryMapperTrainingRunResult>.Fail("System-boundary training batches must contain exactly 50 rows.", "TRAINING_BATCH_SIZE_NOT_50"));

        if (heldOutRows.Count != 50)
            return Task.FromResult(OperationResult<SystemBoundaryMapperTrainingRunResult>.Fail("System-boundary held-out batches must contain exactly 50 rows.", "HELDOUT_BATCH_SIZE_NOT_50"));

        var validated = ValidateRows(trainingRows.Concat(heldOutRows));
        if (!validated.IsSuccess)
            return Task.FromResult(validated.MapError<SystemBoundaryMapperTrainingRunResult>());

        var stopwatch = Stopwatch.StartNew();
        var process = Process.GetCurrentProcess();
        process.Refresh();
        var cpuBefore = process.TotalProcessorTime;
        var peakWorkingSetMb = BytesToMb(process.WorkingSet64);
        var cases = heldOutRows.Select(row =>
        {
            var mapped = _mapper.Map(row.PairId, row.ProjectEvidence).Value!;
            process.Refresh();
            peakWorkingSetMb = Math.Max(peakWorkingSetMb, BytesToMb(process.WorkingSet64));
            return new SystemBoundaryMapperTrainingCaseResult(
                row.PairId,
                Recall(row.RequiredEdgeIds, mapped.Edges.Select(edge => edge.EdgeId).ToArray()),
                UnknownAccuracy(row.Chosen, mapped.UnknownBoundaries),
                Recall(row.EvidenceIds, mapped.Edges.SelectMany(edge => edge.EvidenceIds).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()),
                mapped.Confidence);
        }).ToArray();

        process.Refresh();
        stopwatch.Stop();
        var boundaryEdgeAccuracy = Average(cases.Select(item => item.BoundaryEdgeAccuracy));
        var unknownBoundaryAccuracy = Average(cases.Select(item => item.UnknownBoundaryAccuracy));
        var evidenceGroundingRate = Average(cases.Select(item => item.EvidenceGroundingRate));
        var wrongAnnotationRate = Average(trainingRows.Concat(heldOutRows).Select(row => row.Rejected.Contains("[WRONG:", StringComparison.Ordinal) ? 1.0 : 0.0));
        var passed = boundaryEdgeAccuracy >= 0.95 &&
                     unknownBoundaryAccuracy >= 0.95 &&
                     evidenceGroundingRate >= 0.98 &&
                     wrongAnnotationRate >= 1.0;
        var unitTrace = new WikiUnitTrace(
            "system-boundary-mapper-batch",
            trainingRows.Count,
            heldOutRows.Count,
            [
                new WikiUnitTraceResult(
                    SystemBoundaryMapperUnit.UnitId,
                    "v1",
                    "system-boundary-dpo-batch-gate",
                    $"{trainingRows.Count} train rows / {heldOutRows.Count} held-out rows",
                    $"boundary_edge_accuracy {boundaryEdgeAccuracy:0.######}; unknown_boundary_accuracy {unknownBoundaryAccuracy:0.######}; evidence_grounding_rate {evidenceGroundingRate:0.######}",
                    Math.Min(boundaryEdgeAccuracy, Math.Min(unknownBoundaryAccuracy, evidenceGroundingRate)),
                    passed ? "PASS" : "FAIL",
                    passed ? "" : "system_boundary_gate_failed",
                    passed ? "" : "Add focused 50-row remediation batch for missed cross-repository or unknown-boundary evidence.")
            ],
            passed ? [] : [SystemBoundaryMapperUnit.UnitId],
            passed ? [] : ["Add 50 system-boundary rows for failed edge/evidence dimension."],
            0,
            !passed);

        return Task.FromResult(OperationResult<SystemBoundaryMapperTrainingRunResult>.Ok(new SystemBoundaryMapperTrainingRunResult(
            trainingRows.Count,
            heldOutRows.Count,
            boundaryEdgeAccuracy,
            unknownBoundaryAccuracy,
            evidenceGroundingRate,
            wrongAnnotationRate,
            (process.TotalProcessorTime - cpuBefore).TotalMilliseconds,
            peakWorkingSetMb,
            stopwatch.Elapsed.TotalMilliseconds,
            passed ? "SYSTEM_BOUNDARY_MAPPER_GATE_PASSED" : "SYSTEM_BOUNDARY_MAPPER_GATE_FAILED",
            cases,
            unitTrace)));
    }

    private static OperationResult<bool> ValidateRows(IEnumerable<SystemBoundaryMapperDpoPair> rows)
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

    private static double UnknownAccuracy(string chosen, string[] actualUnknowns)
    {
        var expectedNone = chosen.Contains("unknown_boundaries: none", StringComparison.OrdinalIgnoreCase);
        return expectedNone == (actualUnknowns.Length == 0) ? 1.0 : 0.0;
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
