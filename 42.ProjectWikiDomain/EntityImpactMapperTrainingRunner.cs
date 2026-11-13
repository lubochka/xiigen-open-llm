// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Diagnostics;
using AgentInfrastructure;

public sealed class EntityImpactMapperTrainingRunner
{
    private readonly EntityImpactMapperUnit _mapper;

    public EntityImpactMapperTrainingRunner(EntityImpactMapperUnit? mapper = null)
    {
        _mapper = mapper ?? new EntityImpactMapperUnit();
    }

    public Task<OperationResult<EntityImpactMapperTrainingRunResult>> TrainAndEvaluateAsync(
        IReadOnlyList<EntityImpactMapperDpoPair> trainingRows,
        IReadOnlyList<EntityImpactMapperDpoPair> heldOutRows,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<EntityImpactMapperTrainingRunResult>.Fail("Training cancelled.", "ENTITY_IMPACT_TRAINING_CANCELLED"));

        if (trainingRows is null)
            return Task.FromResult(OperationResult<EntityImpactMapperTrainingRunResult>.Fail("Training rows are required.", "TRAINING_ROWS_REQUIRED"));

        if (heldOutRows is null)
            return Task.FromResult(OperationResult<EntityImpactMapperTrainingRunResult>.Fail("Held-out rows are required.", "HELDOUT_ROWS_REQUIRED"));

        if (trainingRows.Count != 50)
            return Task.FromResult(OperationResult<EntityImpactMapperTrainingRunResult>.Fail("Entity-impact training batches must contain exactly 50 rows.", "TRAINING_BATCH_SIZE_NOT_50"));

        if (heldOutRows.Count != 50)
            return Task.FromResult(OperationResult<EntityImpactMapperTrainingRunResult>.Fail("Entity-impact held-out batches must contain exactly 50 rows.", "HELDOUT_BATCH_SIZE_NOT_50"));

        var validated = ValidateRows(trainingRows.Concat(heldOutRows));
        if (!validated.IsSuccess)
            return Task.FromResult(validated.MapError<EntityImpactMapperTrainingRunResult>());

        var stopwatch = Stopwatch.StartNew();
        var process = Process.GetCurrentProcess();
        process.Refresh();
        var cpuBefore = process.TotalProcessorTime;
        var peakWorkingSetMb = BytesToMb(process.WorkingSet64);
        var cases = heldOutRows.Select(row =>
        {
            var map = _mapper.Map(row.ProjectId, row.EntityName, row.EvidenceRecords).Value!;
            process.Refresh();
            peakWorkingSetMb = Math.Max(peakWorkingSetMb, BytesToMb(process.WorkingSet64));
            return new EntityImpactMapperTrainingCaseResult(
                row.PairId,
                Recall(row.ExpectedEvidenceIds, map.EvidenceIds),
                Recall(row.ExpectedTouchpoints, Touchpoints(map)),
                map.Confidence);
        }).ToArray();

        process.Refresh();
        stopwatch.Stop();
        var evidenceRecall = Average(cases.Select(item => item.EvidenceRecall));
        var touchpointRecall = Average(cases.Select(item => item.TouchpointRecall));
        var wrongAnnotationRate = Average(trainingRows.Concat(heldOutRows).Select(row => row.Rejected.Contains("[WRONG:", StringComparison.Ordinal) ? 1.0 : 0.0));
        var confidence = Average(cases.Select(item => item.Confidence));
        var passed = evidenceRecall >= 0.98 &&
                     touchpointRecall >= 0.95 &&
                     wrongAnnotationRate >= 1.0;
        var unitTrace = new WikiUnitTrace(
            "entity-impact-mapper-batch",
            trainingRows.Count,
            heldOutRows.Count,
            [
                new WikiUnitTraceResult(
                    EntityImpactMapperUnit.UnitId,
                    "v1",
                    "entity-impact-dpo-batch-gate",
                    $"{trainingRows.Count} train rows / {heldOutRows.Count} held-out rows",
                    $"evidence_recall {evidenceRecall:0.######}; touchpoint_recall {touchpointRecall:0.######}",
                    Math.Min(evidenceRecall, touchpointRecall),
                    passed ? "PASS" : "FAIL",
                    passed ? "" : "entity_impact_gate_failed",
                    passed ? "" : "Add focused 50-row remediation batch for missed entity touchpoints.")
            ],
            passed ? [] : [EntityImpactMapperUnit.UnitId],
            passed ? [] : ["Add 50 entity-impact rows for failed touchpoint/evidence dimension."],
            0,
            !passed);

        return Task.FromResult(OperationResult<EntityImpactMapperTrainingRunResult>.Ok(new EntityImpactMapperTrainingRunResult(
            trainingRows.Count,
            heldOutRows.Count,
            evidenceRecall,
            touchpointRecall,
            wrongAnnotationRate,
            confidence,
            (process.TotalProcessorTime - cpuBefore).TotalMilliseconds,
            peakWorkingSetMb,
            stopwatch.Elapsed.TotalMilliseconds,
            passed ? "ENTITY_IMPACT_MAPPER_GATE_PASSED" : "ENTITY_IMPACT_MAPPER_GATE_FAILED",
            cases,
            unitTrace)));
    }

    private static OperationResult<bool> ValidateRows(IEnumerable<EntityImpactMapperDpoPair> rows)
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

    private static string[] Touchpoints(EntityImpactMapResult map)
        => map.InputTouchpoints
            .Concat(map.ValidationTouchpoints)
            .Concat(map.MappingTouchpoints)
            .Concat(map.CalculationTouchpoints)
            .Concat(map.PersistenceTouchpoints)
            .Concat(map.OutputTouchpoints)
            .Concat(map.TestTouchpoints)
            .Concat(map.UnknownTouchpoints)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

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
