// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Diagnostics;
using AgentInfrastructure;

public sealed class ProjectAnswerVerifierTrainingRunner
{
    private readonly ProjectAnswerVerifier _verifier;

    public ProjectAnswerVerifierTrainingRunner(ProjectAnswerVerifier? verifier = null)
    {
        _verifier = verifier ?? new ProjectAnswerVerifier();
    }

    public Task<OperationResult<ProjectAnswerVerifierTrainingRunResult>> TrainAndEvaluateAsync(
        IReadOnlyList<ProjectAnswerVerifierDpoPair> trainingRows,
        IReadOnlyList<ProjectAnswerVerifierDpoPair> heldOutRows,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<ProjectAnswerVerifierTrainingRunResult>.Fail("Training cancelled.", "ANSWER_VERIFIER_TRAINING_CANCELLED"));

        if (trainingRows is null)
            return Task.FromResult(OperationResult<ProjectAnswerVerifierTrainingRunResult>.Fail("Training rows are required.", "TRAINING_ROWS_REQUIRED"));

        if (heldOutRows is null)
            return Task.FromResult(OperationResult<ProjectAnswerVerifierTrainingRunResult>.Fail("Held-out rows are required.", "HELDOUT_ROWS_REQUIRED"));

        if (trainingRows.Count != 50)
            return Task.FromResult(OperationResult<ProjectAnswerVerifierTrainingRunResult>.Fail("Answer-verifier training batches must contain exactly 50 rows.", "TRAINING_BATCH_SIZE_NOT_50"));

        if (heldOutRows.Count != 50)
            return Task.FromResult(OperationResult<ProjectAnswerVerifierTrainingRunResult>.Fail("Answer-verifier held-out batches must contain exactly 50 rows.", "HELDOUT_BATCH_SIZE_NOT_50"));

        var validated = ValidateRows(trainingRows.Concat(heldOutRows));
        if (!validated.IsSuccess)
            return Task.FromResult(validated.MapError<ProjectAnswerVerifierTrainingRunResult>());

        var stopwatch = Stopwatch.StartNew();
        var process = Process.GetCurrentProcess();
        process.Refresh();
        var cpuBefore = process.TotalProcessorTime;
        var peakWorkingSetMb = BytesToMb(process.WorkingSet64);
        var cases = heldOutRows.Select(row =>
        {
            var index = ProjectEvidenceIndex.CreateFromRecords(row.EvidenceRecords).Value!;
            var verified = _verifier.Verify(row.Answer, index, row.ExpectedProjectId);
            process.Refresh();
            peakWorkingSetMb = Math.Max(peakWorkingSetMb, BytesToMb(process.WorkingSet64));
            return new ProjectAnswerVerifierTrainingCaseResult(
                row.PairId,
                verified.IsPassed == row.ExpectedPassed,
                SetEquals(verified.Warnings, row.ExpectedWarnings),
                verified.CitationAccuracy);
        }).ToArray();

        process.Refresh();
        stopwatch.Stop();
        var passFailAccuracy = Average(cases.Select(item => item.PassFailCorrect ? 1.0 : 0.0));
        var warningSetAccuracy = Average(cases.Select(item => item.WarningSetCorrect ? 1.0 : 0.0));
        var citationAccuracy = Average(cases.Select(item => item.CitationAccuracy));
        var wrongAnnotationRate = Average(trainingRows.Concat(heldOutRows).Select(row => row.Rejected.Contains("[WRONG:", StringComparison.Ordinal) ? 1.0 : 0.0));
        var passed = passFailAccuracy >= 0.98 &&
                     warningSetAccuracy >= 0.95 &&
                     wrongAnnotationRate >= 1.0;
        var unitTrace = new WikiUnitTrace(
            "project-answer-verifier-batch",
            trainingRows.Count,
            heldOutRows.Count,
            [
                new WikiUnitTraceResult(
                    ProjectAnswerVerifier.UnitId,
                    "v1",
                    "answer-verifier-dpo-batch-gate",
                    $"{trainingRows.Count} train rows / {heldOutRows.Count} held-out rows",
                    $"pass_fail_accuracy {passFailAccuracy:0.######}; warning_set_accuracy {warningSetAccuracy:0.######}; citation_accuracy {citationAccuracy:0.######}",
                    Math.Min(passFailAccuracy, warningSetAccuracy),
                    passed ? "PASS" : "FAIL",
                    passed ? "" : "answer_verifier_gate_failed",
                    passed ? "" : "Add focused 50-row remediation batch for citation, boundary, or abstention verification.")
            ],
            passed ? [] : [ProjectAnswerVerifier.UnitId],
            passed ? [] : ["Add 50 answer-verifier rows for failed verification dimension."],
            0,
            !passed);

        return Task.FromResult(OperationResult<ProjectAnswerVerifierTrainingRunResult>.Ok(new ProjectAnswerVerifierTrainingRunResult(
            trainingRows.Count,
            heldOutRows.Count,
            passFailAccuracy,
            warningSetAccuracy,
            citationAccuracy,
            wrongAnnotationRate,
            (process.TotalProcessorTime - cpuBefore).TotalMilliseconds,
            peakWorkingSetMb,
            stopwatch.Elapsed.TotalMilliseconds,
            passed ? "PROJECT_ANSWER_VERIFIER_GATE_PASSED" : "PROJECT_ANSWER_VERIFIER_GATE_FAILED",
            cases,
            unitTrace)));
    }

    private static OperationResult<bool> ValidateRows(IEnumerable<ProjectAnswerVerifierDpoPair> rows)
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
