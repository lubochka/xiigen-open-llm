// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Diagnostics;
using AgentInfrastructure;

public sealed class ProjectWikiAnswerComposerTrainingRunner
{
    public const string UnitId = "project-wiki-answer-composer-unit";
    public const string ScenarioId = "project-wiki-answer-composition";

    private readonly ProjectLocalWikiModelUnit _composer;

    public ProjectWikiAnswerComposerTrainingRunner(ProjectLocalWikiModelUnit? composer = null)
    {
        _composer = composer ?? new ProjectLocalWikiModelUnit();
    }

    public async Task<OperationResult<ProjectWikiAnswerComposerTrainingRunResult>> TrainAndEvaluateAsync(
        IReadOnlyList<ProjectWikiAnswerComposerDpoPair> trainingRows,
        IReadOnlyList<ProjectWikiAnswerComposerDpoPair> heldOutRows,
        CancellationToken ct = default)
    {
        if (trainingRows is null)
            return OperationResult<ProjectWikiAnswerComposerTrainingRunResult>.Fail("Training rows are required.", "TRAINING_ROWS_REQUIRED");

        if (heldOutRows is null)
            return OperationResult<ProjectWikiAnswerComposerTrainingRunResult>.Fail("Held-out rows are required.", "HELDOUT_ROWS_REQUIRED");

        if (trainingRows.Count != 50)
            return OperationResult<ProjectWikiAnswerComposerTrainingRunResult>.Fail("Project wiki answer-composer training batches must contain exactly 50 rows.", "TRAINING_BATCH_SIZE_NOT_50");

        if (heldOutRows.Count != 50)
            return OperationResult<ProjectWikiAnswerComposerTrainingRunResult>.Fail("Project wiki answer-composer held-out batches must contain exactly 50 rows.", "HELDOUT_BATCH_SIZE_NOT_50");

        var validated = ValidateRows(trainingRows.Concat(heldOutRows));
        if (!validated.IsSuccess)
            return validated.MapError<ProjectWikiAnswerComposerTrainingRunResult>();

        var stopwatch = Stopwatch.StartNew();
        var process = Process.GetCurrentProcess();
        process.Refresh();
        var cpuBefore = process.TotalProcessorTime;
        var peakWorkingSetMb = BytesToMb(process.WorkingSet64);
        var cases = new List<ProjectWikiAnswerComposerTrainingCaseResult>(heldOutRows.Count);

        foreach (var row in heldOutRows)
        {
            if (ct.IsCancellationRequested)
                return OperationResult<ProjectWikiAnswerComposerTrainingRunResult>.Fail("Answer-composer training was cancelled.", "ANSWER_COMPOSER_TRAINING_CANCELLED");

            var context = BuildContext(row);
            var answered = await _composer.AnswerAsync(row.Request, context, ct);
            if (!answered.IsSuccess)
                return answered.MapError<ProjectWikiAnswerComposerTrainingRunResult>();

            var answer = answered.Value!;
            var exactEvidenceHit = row.RequiredEvidenceIds.Length == 0 ||
                                   row.RequiredEvidenceIds.All(id => answer.EvidenceIds.Contains(id, StringComparer.Ordinal));
            var citationCorrect = row.RequiredEvidenceIds.Length == 0
                ? answer.EvidenceIds.Length == 0
                : row.RequiredEvidenceIds.All(id => answer.Answer.Contains(id, StringComparison.Ordinal));
            var missingEvidenceHandled = row.RequiredEvidenceIds.Length != 0 ||
                                         !string.IsNullOrWhiteSpace(answer.MissingEvidence);
            var missingSections = MissingSections(row.RequiredAnswerSections, answer.Answer, answer.MissingEvidence).ToArray();
            cases.Add(new ProjectWikiAnswerComposerTrainingCaseResult(
                row.PairId,
                row.QuestionType,
                row.RequiredEvidenceIds,
                answer.EvidenceIds,
                exactEvidenceHit,
                citationCorrect,
                missingSections.Length == 0,
                missingEvidenceHandled,
                missingSections));

            process.Refresh();
            peakWorkingSetMb = Math.Max(peakWorkingSetMb, BytesToMb(process.WorkingSet64));
        }

        process.Refresh();
        stopwatch.Stop();
        var exactEvidenceAccuracy = Average(cases.Select(item => item.ExactEvidenceHit ? 1.0 : 0.0));
        var citationAccuracy = Average(cases.Select(item => item.CitationCorrect ? 1.0 : 0.0));
        var answerShapeAccuracy = Average(cases.Select(item => item.AnswerShapeCorrect ? 1.0 : 0.0));
        var missingEvidenceAccuracy = Average(cases
            .Where(item => item.ExpectedEvidenceIds.Length == 0)
            .Select(item => item.MissingEvidenceHandled ? 1.0 : 0.0));
        var wrongAnnotationRate = Average(trainingRows.Concat(heldOutRows).Select(row => row.Rejected.Contains("[WRONG:", StringComparison.Ordinal) ? 1.0 : 0.0));
        var passed = exactEvidenceAccuracy >= 0.95 &&
                     citationAccuracy >= 0.98 &&
                     answerShapeAccuracy >= 0.95 &&
                     missingEvidenceAccuracy >= 0.95 &&
                     wrongAnnotationRate >= 1.0;
        var failedUnits = passed ? Array.Empty<string>() : [UnitId];
        var remediation = passed
            ? Array.Empty<string>()
            : ["Add 50 ProjectWikiAnswerComposer DPO rows targeting missing answer sections, citation failures, or abstention failures and rerun this gate."];
        var unitTrace = new WikiUnitTrace(
            "project-wiki-answer-composer-batch",
            trainingRows.Count,
            heldOutRows.Count,
            [
                new WikiUnitTraceResult(
                    UnitId,
                    "v1",
                    "answer-composer-dpo-batch-gate",
                    $"{trainingRows.Count} train rows / {heldOutRows.Count} held-out rows",
                    $"exact_evidence_accuracy {exactEvidenceAccuracy:0.######}; citation_accuracy {citationAccuracy:0.######}; answer_shape_accuracy {answerShapeAccuracy:0.######}; missing_evidence_abstention {missingEvidenceAccuracy:0.######}",
                    Math.Min(exactEvidenceAccuracy, Math.Min(citationAccuracy, answerShapeAccuracy)),
                    passed ? "PASS" : "FAIL",
                    passed ? "" : "answer_composer_gate_failed",
                    passed ? "" : "Add focused 50-row remediation batch and rerun held-out gate.")
            ],
            failedUnits,
            remediation,
            0,
            !passed);

        return OperationResult<ProjectWikiAnswerComposerTrainingRunResult>.Ok(new ProjectWikiAnswerComposerTrainingRunResult(
            trainingRows.Count,
            heldOutRows.Count,
            exactEvidenceAccuracy,
            citationAccuracy,
            answerShapeAccuracy,
            missingEvidenceAccuracy,
            wrongAnnotationRate,
            (process.TotalProcessorTime - cpuBefore).TotalMilliseconds,
            peakWorkingSetMb,
            stopwatch.Elapsed.TotalMilliseconds,
            passed ? "PROJECT_WIKI_ANSWER_COMPOSER_GATE_PASSED" : "PROJECT_WIKI_ANSWER_COMPOSER_GATE_FAILED",
            cases.ToArray(),
            unitTrace));
    }

    private static OperationResult<bool> ValidateRows(IEnumerable<ProjectWikiAnswerComposerDpoPair> rows)
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

            if (row.RequiredEvidenceIds.Length > 0 && row.Evidence is null)
                return OperationResult<bool>.Fail($"evidence record is required for grounded pair {row.PairId}.", "EVIDENCE_REQUIRED");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static ProjectWikiRetrievedContext BuildContext(ProjectWikiAnswerComposerDpoPair row)
    {
        if (row.Evidence is null)
            return new ProjectWikiRetrievedContext(ProjectWikiRetrievalMode.Mode3LexicalEvidence, [], null, false, ["heldout_missing_evidence_case"]);

        return new ProjectWikiRetrievedContext(
            ProjectWikiRetrievalMode.Mode3LexicalEvidence,
            [new ProjectWikiEvidenceHit(row.Evidence, 1.0, "heldout-evidence")],
            null,
            false,
            []);
    }

    private static IEnumerable<string> MissingSections(IEnumerable<string> requiredSections, string answer, string missingEvidence)
    {
        foreach (var section in requiredSections)
        {
            if (section == "missing_evidence")
            {
                if (string.IsNullOrWhiteSpace(missingEvidence))
                    yield return section;
                continue;
            }

            var token = section switch
            {
                "meaning" => "Project-local wiki answer",
                "location" => "Location:",
                "parameters" => "Parameters:",
                "returns" => "Returns:",
                "callers" => "Called by:",
                "callees" => "Calls:",
                "workflow" => "Workflow trace:",
                "impact" => "Impact path:",
                "evidence" => "Evidence:",
                _ => section
            };
            if (!answer.Contains(token, StringComparison.OrdinalIgnoreCase))
                yield return section;
        }
    }

    private static double Average(IEnumerable<double> values)
    {
        var materialized = values.ToArray();
        return materialized.Length == 0 ? 1.0 : materialized.Average();
    }

    private static double BytesToMb(long bytes)
        => bytes / 1024.0 / 1024.0;
}
