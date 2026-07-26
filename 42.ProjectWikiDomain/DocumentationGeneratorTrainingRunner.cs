// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Diagnostics;
using AgentInfrastructure;

public sealed class DocumentationGeneratorTrainingRunner
{
    private readonly DocumentationGeneratorUnit _generator;

    public DocumentationGeneratorTrainingRunner(DocumentationGeneratorUnit? generator = null)
    {
        _generator = generator ?? new DocumentationGeneratorUnit();
    }

    public Task<OperationResult<DocumentationGeneratorTrainingRunResult>> TrainAndEvaluateAsync(
        IReadOnlyList<DocumentationGeneratorDpoPair> trainingRows,
        IReadOnlyList<DocumentationGeneratorDpoPair> heldOutRows,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<DocumentationGeneratorTrainingRunResult>.Fail("Training cancelled.", "DOCUMENTATION_GENERATOR_TRAINING_CANCELLED"));

        if (trainingRows is null)
            return Task.FromResult(OperationResult<DocumentationGeneratorTrainingRunResult>.Fail("Training rows are required.", "TRAINING_ROWS_REQUIRED"));

        if (heldOutRows is null)
            return Task.FromResult(OperationResult<DocumentationGeneratorTrainingRunResult>.Fail("Held-out rows are required.", "HELDOUT_ROWS_REQUIRED"));

        if (trainingRows.Count != 50)
            return Task.FromResult(OperationResult<DocumentationGeneratorTrainingRunResult>.Fail("Documentation-generator training batches must contain exactly 50 rows.", "TRAINING_BATCH_SIZE_NOT_50"));

        if (heldOutRows.Count != 50)
            return Task.FromResult(OperationResult<DocumentationGeneratorTrainingRunResult>.Fail("Documentation-generator held-out batches must contain exactly 50 rows.", "HELDOUT_BATCH_SIZE_NOT_50"));

        var validated = ValidateRows(trainingRows.Concat(heldOutRows));
        if (!validated.IsSuccess)
            return Task.FromResult(validated.MapError<DocumentationGeneratorTrainingRunResult>());

        var stopwatch = Stopwatch.StartNew();
        var process = Process.GetCurrentProcess();
        process.Refresh();
        var cpuBefore = process.TotalProcessorTime;
        var peakWorkingSetMb = BytesToMb(process.WorkingSet64);
        var cases = heldOutRows.Select(row =>
        {
            var generated = _generator.Generate(row.Unit).Value!;
            process.Refresh();
            peakWorkingSetMb = Math.Max(peakWorkingSetMb, BytesToMb(process.WorkingSet64));
            var flags = new[]
            {
                generated.HasMeaning,
                generated.HasLocation,
                generated.HasParameters,
                generated.HasReturnValue,
                generated.HasCallers,
                generated.HasCallees,
                generated.HasStackVersion,
                generated.HasEvidenceIds
            };
            return new DocumentationGeneratorTrainingCaseResult(
                row.PairId,
                flags.All(flag => flag),
                flags.Count(flag => flag) / (double)flags.Length,
                generated.Confidence);
        }).ToArray();

        process.Refresh();
        stopwatch.Stop();
        var fieldCoverage = Average(cases.Select(item => item.FieldCoverage));
        var wrongAnnotationRate = Average(trainingRows.Concat(heldOutRows).Select(row => row.Rejected.Contains("[WRONG:", StringComparison.Ordinal) ? 1.0 : 0.0));
        var confidence = Average(cases.Select(item => item.Confidence));
        var passed = fieldCoverage >= 0.98 && wrongAnnotationRate >= 1.0 && cases.All(item => item.RequiredFieldsPresent);
        var unitTrace = new WikiUnitTrace(
            "documentation-generator-batch",
            trainingRows.Count,
            heldOutRows.Count,
            [
                new WikiUnitTraceResult(
                    DocumentationGeneratorUnit.UnitId,
                    "v1",
                    "documentation-generator-dpo-batch-gate",
                    $"{trainingRows.Count} train rows / {heldOutRows.Count} held-out rows",
                    $"required_field_coverage {fieldCoverage:0.######}",
                    fieldCoverage,
                    passed ? "PASS" : "FAIL",
                    passed ? "" : "documentation_generator_gate_failed",
                    passed ? "" : "Add focused 50-row remediation batch for missing documentation fields.")
            ],
            passed ? [] : [DocumentationGeneratorUnit.UnitId],
            passed ? [] : ["Add 50 documentation-generation rows for missing field coverage."],
            0,
            !passed);

        return Task.FromResult(OperationResult<DocumentationGeneratorTrainingRunResult>.Ok(new DocumentationGeneratorTrainingRunResult(
            trainingRows.Count,
            heldOutRows.Count,
            fieldCoverage,
            wrongAnnotationRate,
            confidence,
            (process.TotalProcessorTime - cpuBefore).TotalMilliseconds,
            peakWorkingSetMb,
            stopwatch.Elapsed.TotalMilliseconds,
            passed ? "DOCUMENTATION_GENERATOR_GATE_PASSED" : "DOCUMENTATION_GENERATOR_GATE_FAILED",
            cases,
            unitTrace)));
    }

    private static OperationResult<bool> ValidateRows(IEnumerable<DocumentationGeneratorDpoPair> rows)
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

    private static double Average(IEnumerable<double> values)
    {
        var materialized = values.ToArray();
        return materialized.Length == 0 ? 0 : materialized.Average();
    }

    private static double BytesToMb(long bytes)
        => bytes / 1024.0 / 1024.0;
}
