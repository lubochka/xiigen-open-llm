// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Diagnostics;
using AgentInfrastructure;

public sealed class ProjectWikiQueryPlanningTrainingRunner
{
    private readonly ProjectWikiQueryPlanningFlow _planner;

    public ProjectWikiQueryPlanningTrainingRunner(ProjectWikiQueryPlanningFlow? planner = null)
    {
        _planner = planner ?? new ProjectWikiQueryPlanningFlow();
    }

    public async Task<OperationResult<ProjectWikiQueryPlanningTrainingRunResult>> TrainAndEvaluateAsync(
        IReadOnlyList<ProjectWikiQueryPlanningDpoPair> trainingRows,
        IReadOnlyList<ProjectWikiQueryPlanningDpoPair> heldOutRows,
        CancellationToken ct = default)
    {
        if (trainingRows is null)
            return OperationResult<ProjectWikiQueryPlanningTrainingRunResult>.Fail("Training rows are required.", "TRAINING_ROWS_REQUIRED");

        if (heldOutRows is null)
            return OperationResult<ProjectWikiQueryPlanningTrainingRunResult>.Fail("Held-out rows are required.", "HELDOUT_ROWS_REQUIRED");

        if (trainingRows.Count != 50)
            return OperationResult<ProjectWikiQueryPlanningTrainingRunResult>.Fail("Project wiki query-planning training batches must contain exactly 50 rows.", "TRAINING_BATCH_SIZE_NOT_50");

        if (heldOutRows.Count != 50)
            return OperationResult<ProjectWikiQueryPlanningTrainingRunResult>.Fail("Project wiki query-planning held-out batches must contain exactly 50 rows.", "HELDOUT_BATCH_SIZE_NOT_50");

        var validated = ValidateRows(trainingRows.Concat(heldOutRows));
        if (!validated.IsSuccess)
            return validated.MapError<ProjectWikiQueryPlanningTrainingRunResult>();

        var stopwatch = Stopwatch.StartNew();
        var process = Process.GetCurrentProcess();
        process.Refresh();
        var cpuBefore = process.TotalProcessorTime;
        var peakWorkingSetMb = BytesToMb(process.WorkingSet64);
        var cases = new List<ProjectWikiQueryPlanningTrainingCaseResult>(heldOutRows.Count);

        foreach (var row in heldOutRows)
        {
            if (ct.IsCancellationRequested)
                return OperationResult<ProjectWikiQueryPlanningTrainingRunResult>.Fail("Query-planning training was cancelled.", "QUERY_PLANNING_TRAINING_CANCELLED");

            var planned = await _planner.PlanAsync(new ProjectWikiUserQuestion(
                row.PairId,
                ReadProjectId(row),
                "",
                "heldout-v1",
                row.A),
                ct);

            if (!planned.IsSuccess)
                return planned.MapError<ProjectWikiQueryPlanningTrainingRunResult>();

            var plan = planned.Value!;
            cases.Add(new ProjectWikiQueryPlanningTrainingCaseResult(
                row.PairId,
                row.Intent,
                plan.Intent,
                row.Intent == plan.Intent,
                CoversRequiredTouchpoints(plan),
                plan.RequiredQuerySteps.Any(step => step.StepType == ProjectWikiQueryStepType.MissingEvidenceCheck),
                plan.PlanSufficiencyScore,
                plan.AnswerReadinessScore));

            process.Refresh();
            peakWorkingSetMb = Math.Max(peakWorkingSetMb, BytesToMb(process.WorkingSet64));
        }

        process.Refresh();
        stopwatch.Stop();
        var queryPlanAccuracy = Average(cases.Select(item => item.IntentCorrect ? 1.0 : 0.0));
        var requiredTouchpointRecall = Average(cases.Select(item => item.RequiredTouchpointsCovered ? 1.0 : 0.0));
        var multiHopSequenceAccuracy = Average(cases.Select(item => item.PlanSufficiencyScore >= 0.9 ? 1.0 : 0.0));
        var missingEvidenceDetection = Average(cases.Select(item => item.MissingEvidenceCheckPresent ? 1.0 : 0.0));
        var wrongAnnotationRate = Average(trainingRows.Concat(heldOutRows).Select(row => row.Rejected.Contains("[WRONG:", StringComparison.Ordinal) ? 1.0 : 0.0));
        var answerReadiness = Average(cases.Select(item => item.AnswerReadinessScore));
        var passed = queryPlanAccuracy >= 0.90 &&
                     requiredTouchpointRecall >= 0.95 &&
                     multiHopSequenceAccuracy >= 0.90 &&
                     missingEvidenceDetection >= 0.95 &&
                     wrongAnnotationRate >= 1.0 &&
                     answerReadiness >= 0.90;
        var failedUnits = passed ? Array.Empty<string>() : [ProjectWikiQueryPlanningFlow.FlowId];
        var remediation = passed
            ? Array.Empty<string>()
            : ["Add 50 ProjectWikiQueryPlanningFlow DPO rows targeting failed held-out intents and rerun this gate."];
        var unitTrace = new WikiUnitTrace(
            "project-wiki-query-planning-batch",
            trainingRows.Count,
            heldOutRows.Count,
            [
                new WikiUnitTraceResult(
                    ProjectWikiQueryPlanningFlow.FlowId,
                    "v1",
                    "query-planning-dpo-batch-gate",
                    $"{trainingRows.Count} train rows / {heldOutRows.Count} held-out rows",
                    $"accuracy {queryPlanAccuracy:0.######}; touchpoint_recall {requiredTouchpointRecall:0.######}; missing_evidence {missingEvidenceDetection:0.######}",
                    answerReadiness,
                    passed ? "PASS" : "FAIL",
                    passed ? "" : "query_planning_gate_failed",
                    passed ? "" : "Add focused 50-row remediation batch and rerun held-out gate.")
            ],
            failedUnits,
            remediation,
            0,
            !passed);

        return OperationResult<ProjectWikiQueryPlanningTrainingRunResult>.Ok(new ProjectWikiQueryPlanningTrainingRunResult(
            trainingRows.Count,
            heldOutRows.Count,
            queryPlanAccuracy,
            requiredTouchpointRecall,
            multiHopSequenceAccuracy,
            missingEvidenceDetection,
            wrongAnnotationRate,
            answerReadiness,
            (process.TotalProcessorTime - cpuBefore).TotalMilliseconds,
            peakWorkingSetMb,
            stopwatch.Elapsed.TotalMilliseconds,
            passed ? "PROJECT_WIKI_QUERY_PLANNING_GATE_PASSED" : "PROJECT_WIKI_QUERY_PLANNING_GATE_FAILED",
            cases.ToArray(),
            unitTrace));
    }

    private static OperationResult<bool> ValidateRows(IEnumerable<ProjectWikiQueryPlanningDpoPair> rows)
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

    private static bool CoversRequiredTouchpoints(ProjectWikiQueryPlan plan)
    {
        if (plan.Intent is ProjectWikiQueryIntent.FieldAddition or ProjectWikiQueryIntent.EntityImpact or ProjectWikiQueryIntent.FieldRename)
        {
            var stepTypes = plan.RequiredQuerySteps.Select(step => step.StepType).ToArray();
            return stepTypes.Contains(ProjectWikiQueryStepType.DtoLookup) &&
                   stepTypes.Contains(ProjectWikiQueryStepType.MapperLookup) &&
                   stepTypes.Contains(ProjectWikiQueryStepType.PersistenceLookup) &&
                   stepTypes.Contains(ProjectWikiQueryStepType.CallerLookup) &&
                   stepTypes.Contains(ProjectWikiQueryStepType.WorkflowBranchLookup) &&
                   stepTypes.Contains(ProjectWikiQueryStepType.MissingEvidenceCheck);
        }

        if (plan.Intent is ProjectWikiQueryIntent.StoredProcedureImpact)
        {
            var stepTypes = plan.RequiredQuerySteps.Select(step => step.StepType).ToArray();
            return stepTypes.Contains(ProjectWikiQueryStepType.StoredProcedureLookup) &&
                   stepTypes.Contains(ProjectWikiQueryStepType.SqlParameterLookup) &&
                   stepTypes.Contains(ProjectWikiQueryStepType.SqlTableDependencyLookup) &&
                   stepTypes.Contains(ProjectWikiQueryStepType.SqlResultShapeLookup) &&
                   stepTypes.Contains(ProjectWikiQueryStepType.SqlSideEffectLookup) &&
                   stepTypes.Contains(ProjectWikiQueryStepType.CallerLookup) &&
                   stepTypes.Contains(ProjectWikiQueryStepType.ValueFlowLookup) &&
                   stepTypes.Contains(ProjectWikiQueryStepType.MissingEvidenceCheck);
        }

        return plan.RequiredQuerySteps.Length >= 3;
    }

    private static string ReadProjectId(ProjectWikiQueryPlanningDpoPair row)
        => row.PairId.Contains("genietasks", StringComparison.OrdinalIgnoreCase) ? "genietasks" : "askapediaweb";

    private static double Average(IEnumerable<double> values)
    {
        var materialized = values.ToArray();
        return materialized.Length == 0 ? 0 : materialized.Average();
    }

    private static double BytesToMb(long bytes)
        => bytes / 1024.0 / 1024.0;
}
