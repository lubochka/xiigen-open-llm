// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Diagnostics;
using AgentInfrastructure;

public sealed class StackAndVersionTrainingRunner
{
    private readonly StackAndVersionClassifierUnit _classifier;

    public StackAndVersionTrainingRunner(StackAndVersionClassifierUnit? classifier = null)
    {
        _classifier = classifier ?? new StackAndVersionClassifierUnit();
    }

    public Task<OperationResult<StackAndVersionTrainingRunResult>> TrainAndEvaluateAsync(
        IReadOnlyList<StackAndVersionDpoPair> trainingRows,
        IReadOnlyList<StackAndVersionDpoPair> heldOutRows,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<StackAndVersionTrainingRunResult>.Fail("Training cancelled.", "STACK_VERSION_TRAINING_CANCELLED"));

        if (trainingRows is null)
            return Task.FromResult(OperationResult<StackAndVersionTrainingRunResult>.Fail("Training rows are required.", "TRAINING_ROWS_REQUIRED"));

        if (heldOutRows is null)
            return Task.FromResult(OperationResult<StackAndVersionTrainingRunResult>.Fail("Held-out rows are required.", "HELDOUT_ROWS_REQUIRED"));

        if (trainingRows.Count != 50)
            return Task.FromResult(OperationResult<StackAndVersionTrainingRunResult>.Fail("Stack/version training batches must contain exactly 50 rows.", "TRAINING_BATCH_SIZE_NOT_50"));

        if (heldOutRows.Count != 50)
            return Task.FromResult(OperationResult<StackAndVersionTrainingRunResult>.Fail("Stack/version held-out batches must contain exactly 50 rows.", "HELDOUT_BATCH_SIZE_NOT_50"));

        var validated = ValidateRows(trainingRows.Concat(heldOutRows));
        if (!validated.IsSuccess)
            return Task.FromResult(validated.MapError<StackAndVersionTrainingRunResult>());

        var stopwatch = Stopwatch.StartNew();
        var process = Process.GetCurrentProcess();
        process.Refresh();
        var cpuBefore = process.TotalProcessorTime;
        var peakWorkingSetMb = BytesToMb(process.WorkingSet64);
        var cases = heldOutRows.Select(row =>
        {
            var classified = _classifier.Classify(row.CsprojText).Value!;
            process.Refresh();
            peakWorkingSetMb = Math.Max(peakWorkingSetMb, BytesToMb(process.WorkingSet64));
            return new StackAndVersionTrainingCaseResult(
                row.PairId,
                classified.RuntimeFamily == row.ExpectedRuntimeFamily,
                classified.TargetFramework == row.ExpectedTargetFramework,
                classified.CSharpLanguageVersion == row.ExpectedCSharpLanguageVersion,
                classified.ProjectStyle == row.ExpectedProjectStyle,
                classified.Confidence);
        }).ToArray();

        process.Refresh();
        stopwatch.Stop();
        var runtimeAccuracy = Average(cases.Select(item => item.RuntimeCorrect ? 1.0 : 0.0));
        var targetAccuracy = Average(cases.Select(item => item.TargetFrameworkCorrect ? 1.0 : 0.0));
        var languageAccuracy = Average(cases.Select(item => item.LanguageVersionCorrect ? 1.0 : 0.0));
        var styleAccuracy = Average(cases.Select(item => item.ProjectStyleCorrect ? 1.0 : 0.0));
        var wrongAnnotationRate = Average(trainingRows.Concat(heldOutRows).Select(row => row.Rejected.Contains("[WRONG:", StringComparison.Ordinal) ? 1.0 : 0.0));
        var confidence = Average(cases.Select(item => item.Confidence));
        var passed = runtimeAccuracy >= 0.98 &&
                     targetAccuracy >= 0.98 &&
                     languageAccuracy >= 0.95 &&
                     styleAccuracy >= 0.98 &&
                     wrongAnnotationRate >= 1.0;
        var unitTrace = new WikiUnitTrace(
            "stack-and-version-classifier-batch",
            trainingRows.Count,
            heldOutRows.Count,
            [
                new WikiUnitTraceResult(
                    StackAndVersionClassifierUnit.UnitId,
                    "v1",
                    "stack-version-dpo-batch-gate",
                    $"{trainingRows.Count} train rows / {heldOutRows.Count} held-out rows",
                    $"runtime_accuracy {runtimeAccuracy:0.######}; target_accuracy {targetAccuracy:0.######}; language_accuracy {languageAccuracy:0.######}; style_accuracy {styleAccuracy:0.######}",
                    Math.Min(Math.Min(runtimeAccuracy, targetAccuracy), Math.Min(languageAccuracy, styleAccuracy)),
                    passed ? "PASS" : "FAIL",
                    passed ? "" : "stack_version_gate_failed",
                    passed ? "" : "Add focused 50-row remediation batch for failed stack/version dimension.")
            ],
            passed ? [] : [StackAndVersionClassifierUnit.UnitId],
            passed ? [] : ["Add 50 stack/version rows for failed dimension."],
            0,
            !passed);

        return Task.FromResult(OperationResult<StackAndVersionTrainingRunResult>.Ok(new StackAndVersionTrainingRunResult(
            trainingRows.Count,
            heldOutRows.Count,
            runtimeAccuracy,
            targetAccuracy,
            languageAccuracy,
            styleAccuracy,
            wrongAnnotationRate,
            confidence,
            (process.TotalProcessorTime - cpuBefore).TotalMilliseconds,
            peakWorkingSetMb,
            stopwatch.Elapsed.TotalMilliseconds,
            passed ? "STACK_VERSION_CLASSIFIER_GATE_PASSED" : "STACK_VERSION_CLASSIFIER_GATE_FAILED",
            cases,
            unitTrace)));
    }

    private static OperationResult<bool> ValidateRows(IEnumerable<StackAndVersionDpoPair> rows)
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
