// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Diagnostics;
using AgentInfrastructure;

public sealed class NegativeDpoGeneratorTrainingRunner
{
    public const string UnitId = "negative-dpo-generator-unit";
    public const string ScenarioId = "project_wiki_negative_dpo_generation";

    private static readonly string[] RequiredNegativeTypes =
    [
        "missing_location",
        "missing_parameters",
        "missing_return_contract",
        "missing_callee_transit",
        "missing_caller_context"
    ];

    public Task<OperationResult<NegativeDpoGeneratorTrainingRunResult>> TrainAndEvaluateAsync(
        NegativeDpoGeneratorBatch trainingBatch,
        NegativeDpoGeneratorBatch heldOutBatch,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<NegativeDpoGeneratorTrainingRunResult>.Fail("Training cancelled.", "NEGATIVE_DPO_TRAINING_CANCELLED"));

        if (trainingBatch is null)
            return Task.FromResult(OperationResult<NegativeDpoGeneratorTrainingRunResult>.Fail("Training batch is required.", "TRAINING_BATCH_REQUIRED"));

        if (heldOutBatch is null)
            return Task.FromResult(OperationResult<NegativeDpoGeneratorTrainingRunResult>.Fail("Held-out batch is required.", "HELDOUT_BATCH_REQUIRED"));

        if (trainingBatch.Pairs.Length != 50)
            return Task.FromResult(OperationResult<NegativeDpoGeneratorTrainingRunResult>.Fail("Negative DPO training batches must contain exactly 50 rows.", "TRAINING_BATCH_SIZE_NOT_50"));

        if (heldOutBatch.Pairs.Length != 50)
            return Task.FromResult(OperationResult<NegativeDpoGeneratorTrainingRunResult>.Fail("Negative DPO held-out batches must contain exactly 50 rows.", "HELDOUT_BATCH_SIZE_NOT_50"));

        var validated = ValidateRows(trainingBatch.Pairs.Concat(heldOutBatch.Pairs));
        if (!validated.IsSuccess)
            return Task.FromResult(validated.MapError<NegativeDpoGeneratorTrainingRunResult>());

        var stopwatch = Stopwatch.StartNew();
        var process = Process.GetCurrentProcess();
        process.Refresh();
        var cpuBefore = process.TotalProcessorTime;
        var peakWorkingSetMb = BytesToMb(process.WorkingSet64);
        var cases = heldOutBatch.Pairs.Select(pair =>
        {
            process.Refresh();
            peakWorkingSetMb = Math.Max(peakWorkingSetMb, BytesToMb(process.WorkingSet64));
            return new NegativeDpoGeneratorTrainingCaseResult(
                pair.PairId,
                pair.NegativeType,
                !string.Equals(pair.Chosen.Trim(), pair.Rejected.Trim(), StringComparison.Ordinal),
                pair.Rejected.Contains("[WRONG:", StringComparison.Ordinal),
                pair.EvidenceIds.Length > 0,
                RejectedOmitsRequiredFact(pair));
        }).ToArray();

        process.Refresh();
        stopwatch.Stop();
        var separationRate = Average(cases.Select(item => item.ChosenRejectedDifferent ? 1.0 : 0.0));
        var wrongAnnotationRate = Average(cases.Select(item => item.WrongAnnotationPresent ? 1.0 : 0.0));
        var evidenceGroundingRate = Average(cases.Select(item => item.EvidenceIdsPresent ? 1.0 : 0.0));
        var rejectedDefectAccuracy = Average(cases.Select(item => item.RejectedOmitsRequiredFact ? 1.0 : 0.0));
        var negativeTypeCoverage = heldOutBatch.NegativeTypeCoverageRate;
        var passed = separationRate >= 1.0 &&
                     wrongAnnotationRate >= 1.0 &&
                     evidenceGroundingRate >= 1.0 &&
                     rejectedDefectAccuracy >= 0.95 &&
                     negativeTypeCoverage >= 1.0;
        var failedUnits = passed ? Array.Empty<string>() : [UnitId];
        var remediation = passed
            ? Array.Empty<string>()
            : ["Add 50 NegativeDpoGenerator rows targeting failed negative types and rerun held-out gate."];
        var unitTrace = new WikiUnitTrace(
            "negative-dpo-generator-batch",
            trainingBatch.Pairs.Length,
            heldOutBatch.Pairs.Length,
            [
                new WikiUnitTraceResult(
                    UnitId,
                    "v1",
                    "negative-dpo-generator-batch-gate",
                    $"{trainingBatch.Pairs.Length} train rows / {heldOutBatch.Pairs.Length} held-out rows",
                    $"separation_rate {separationRate:0.######}; wrong_annotation_rate {wrongAnnotationRate:0.######}; rejected_defect_accuracy {rejectedDefectAccuracy:0.######}; type_coverage {negativeTypeCoverage:0.######}",
                    Math.Min(rejectedDefectAccuracy, negativeTypeCoverage),
                    passed ? "PASS" : "FAIL",
                    passed ? "" : "negative_dpo_generator_gate_failed",
                    passed ? "" : "Add focused 50-row remediation batch and rerun held-out gate.")
            ],
            failedUnits,
            remediation,
            0,
            !passed);

        return Task.FromResult(OperationResult<NegativeDpoGeneratorTrainingRunResult>.Ok(new NegativeDpoGeneratorTrainingRunResult(
            trainingBatch.Pairs.Length,
            heldOutBatch.Pairs.Length,
            separationRate,
            wrongAnnotationRate,
            evidenceGroundingRate,
            rejectedDefectAccuracy,
            negativeTypeCoverage,
            (process.TotalProcessorTime - cpuBefore).TotalMilliseconds,
            peakWorkingSetMb,
            stopwatch.Elapsed.TotalMilliseconds,
            passed ? "NEGATIVE_DPO_GENERATOR_GATE_PASSED" : "NEGATIVE_DPO_GENERATOR_GATE_FAILED",
            cases,
            unitTrace)));
    }

    private static OperationResult<bool> ValidateRows(IEnumerable<ProjectDocumentationDpoPair> rows)
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

    private static bool RejectedOmitsRequiredFact(ProjectDocumentationDpoPair pair)
        => pair.NegativeType switch
        {
            "missing_location" => pair.Rejected.Contains("wrong_source_location_not_from_code_trace", StringComparison.OrdinalIgnoreCase) && pair.Chosen.Contains("source_path_and_line_present", StringComparison.OrdinalIgnoreCase),
            "missing_parameters" => pair.Rejected.Contains("actual_input_values_missing_from_code_trace", StringComparison.OrdinalIgnoreCase) && pair.Chosen.Contains("actual_parameters:", StringComparison.OrdinalIgnoreCase),
            "missing_return_contract" => pair.Rejected.Contains("actual_output_value_missing_from_code_trace", StringComparison.OrdinalIgnoreCase) && pair.Chosen.Contains("actual_return:", StringComparison.OrdinalIgnoreCase),
            "missing_callee_transit" => pair.Rejected.Contains("actual_callees_missing_from_code_trace", StringComparison.OrdinalIgnoreCase) && pair.Chosen.Contains("actual_callees:", StringComparison.OrdinalIgnoreCase),
            "missing_caller_context" => pair.Rejected.Contains("wrong_callers:", StringComparison.OrdinalIgnoreCase) &&
                                        pair.Rejected.Contains("caller_path_wrong", StringComparison.OrdinalIgnoreCase) &&
                                        pair.Chosen.Contains("actual_callers:", StringComparison.OrdinalIgnoreCase) &&
                                        pair.Chosen.Contains("caller_path_complete", StringComparison.OrdinalIgnoreCase),
            _ => pair.Rejected.Contains("[WRONG:", StringComparison.Ordinal)
        };

    private static double Average(IEnumerable<double> values)
    {
        var materialized = values.ToArray();
        return materialized.Length == 0 ? 0 : materialized.Average();
    }

    private static double BytesToMb(long bytes)
        => bytes / 1024.0 / 1024.0;
}
