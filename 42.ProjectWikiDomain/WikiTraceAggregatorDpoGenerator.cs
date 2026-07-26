// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;

public sealed class WikiTraceAggregatorDpoGenerator
{
    private readonly WikiTraceAggregatorUnit _aggregator;

    public WikiTraceAggregatorDpoGenerator(WikiTraceAggregatorUnit? aggregator = null)
    {
        _aggregator = aggregator ?? new WikiTraceAggregatorUnit();
    }

    public Task<OperationResult<WikiTraceAggregatorDpoBatch>> BuildBatchAsync(
        string batchId = "wiki-trace-aggregator-batch-001",
        bool heldOut = false,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<WikiTraceAggregatorDpoBatch>.Fail("Cancelled.", "CANCELLED"));

        var signal = batchId.Contains("signal", StringComparison.OrdinalIgnoreCase);
        var target = batchId.Contains("target", StringComparison.OrdinalIgnoreCase);
        var crossUnit = batchId.Contains("cross-unit", StringComparison.OrdinalIgnoreCase) ||
                        batchId.Contains("sequence", StringComparison.OrdinalIgnoreCase) ||
                        batchId.Contains("precheck", StringComparison.OrdinalIgnoreCase);
        var ownerFocused = batchId.Contains("owner", StringComparison.OrdinalIgnoreCase) ||
                           batchId.Contains("no-precheck", StringComparison.OrdinalIgnoreCase);
        var contextFocused = batchId.Contains("context-before", StringComparison.OrdinalIgnoreCase) ||
                             batchId.Contains("context-only", StringComparison.OrdinalIgnoreCase);
        var genericCrossUnit = batchId.Contains("generic", StringComparison.OrdinalIgnoreCase) ||
                               batchId.Contains("compact", StringComparison.OrdinalIgnoreCase);
        var falseFailureFocused = batchId.Contains("false-failure", StringComparison.OrdinalIgnoreCase) ||
                                  batchId.Contains("falsefailure", StringComparison.OrdinalIgnoreCase);
        var seeds = falseFailureFocused ? BuildFalseFailureFocusedSeeds(heldOut) : contextFocused ? BuildCrossUnitFocusedSeeds(heldOut, "context_before_query_plan_verified") : ownerFocused ? BuildCrossUnitOwnerSeeds(heldOut) : crossUnit ? BuildCrossUnitSeeds(heldOut) : BuildSeeds(heldOut);
        var pairs = seeds.Select((seed, index) =>
        {
            var aggregated = _aggregator.Aggregate(seed.BatchId, seed.QueryTraces, seed.UnitTraces).Value!;
            return new WikiTraceAggregatorDpoPair(
                $"{batchId}-pair-{index + 1:000}",
                seed.QueryTraces,
                seed.UnitTraces,
                aggregated.FailedUnits,
                aggregated.RemediationActions,
                aggregated.StopBlindTraining,
                BuildTraceInput(seed),
                crossUnit ? BuildCrossUnitChosen(seed.RejectedType, aggregated, !genericCrossUnit) : target ? BuildTargetChosen(aggregated) : signal ? BuildSignalChosen(seed.RejectedType, aggregated) : BuildChosen(aggregated),
                crossUnit ? BuildCrossUnitRejected(seed.RejectedType, aggregated) : target ? BuildTargetRejected(seed.RejectedType, aggregated) : signal ? BuildSignalRejected(seed.RejectedType, aggregated) : BuildRejected(seed.RejectedType, aggregated),
                seed.RejectedType);
        }).ToArray();
        var wrongAnnotationRate = pairs.Count(pair => pair.Rejected.Contains("[WRONG:", StringComparison.Ordinal)) / (double)pairs.Length;
        var warnings = new List<string>();
        if (pairs.Length != 50)
            warnings.Add("batch_row_count_not_50");
        if (wrongAnnotationRate < 1.0)
            warnings.Add("wrong_annotation_rate_below_gate");

        return Task.FromResult(OperationResult<WikiTraceAggregatorDpoBatch>.Ok(new WikiTraceAggregatorDpoBatch(
            batchId,
            pairs.Length,
            wrongAnnotationRate,
            pairs,
            warnings.ToArray())));
    }

    private static string BuildChosen(WikiTraceAggregationResult result)
        => $"failed_units: {string.Join(", ", result.FailedUnits)}\nremediation: {string.Join(", ", result.RemediationActions)}\nstop_blind_training: {result.StopBlindTraining}\nquery_pass_rate: {result.QueryPassRate:0.######}";

    private static string BuildTraceInput(TraceSeed seed)
    {
        var queryLines = seed.QueryTraces.Select(trace => string.Join(" | ", [
            $"query_id={trace.QueryId}",
            $"question_type={trace.QuestionType}",
            $"planning_invoked={trace.QueryPlanningFlowInvoked}",
            $"planned_steps={string.Join(">", trace.PlannedQuerySteps)}",
            $"executed_steps={string.Join(">", trace.ExecutedQuerySteps)}",
            $"skipped_steps={string.Join(">", trace.SkippedQuerySteps)}",
            $"missing_evidence={string.Join(",", trace.MissingEvidence)}",
            $"routed_units={string.Join(",", trace.RoutedUnits)}",
            $"unit_results={string.Join(",", trace.UnitResults.Select(unit => $"{unit.UnitId}:{unit.PassOrFail}:{unit.FailureType}"))}",
            $"final_status={trace.FinalStatus}"
        ]));
        var unitLines = seed.UnitTraces.Select(trace => string.Join(" | ", [
            $"batch_id={trace.BatchId}",
            $"failed_units={string.Join(",", trace.FailedUnits)}",
            $"remediation={string.Join(";", trace.RemediationBatchesRequired)}",
            $"stop_blind_training={trace.StopBlindTraining}",
            $"metrics={string.Join(",", trace.UnitMetrics.Select(unit => $"{unit.UnitId}:{unit.PassOrFail}:{unit.FailureType}"))}"
        ]));
        return string.Join("\n", [
            "aggregate wiki traces into failed units, concrete remediation actions, and blind-continuation decision",
            "query_traces:",
            string.Join("\n", queryLines),
            "unit_traces:",
            string.Join("\n", unitLines)
        ]);
    }

    private static string BuildSignalChosen(string rejectedType, WikiTraceAggregationResult result)
    {
        var failed = result.FailedUnits.Length == 0
            ? "FAILED_UNITS_NONE"
            : string.Join(" ", result.FailedUnits.Select(unit => $"FAILED_UNIT_{TokenSafe(unit)}"));
        var remediation = result.RemediationActions.Length == 0
            ? "REMEDIATION_NONE_REQUIRED"
            : "REMEDIATION_ACTIONS_PRESENT " + string.Join(" ", result.RemediationActions.Select(action => $"REMEDIATION_{TokenSafe(action)}"));
        var defectCorrection = rejectedType switch
        {
            "ignores_failed_unit" => "CORRECTS_FAILED_UNIT_IGNORED FAILED_UNIT_EXPLICIT FAILED_UNIT_NOT_HIDDEN",
            "continues_blind_training" => "CORRECTS_BLIND_CONTINUATION BLIND_CONTINUATION_BLOCKED GAP_ANALYSIS_DONE STOP_DECISION_ENFORCED",
            "missing_remediation" => "CORRECTS_MISSING_REMEDIATION NEXT_50_ROW_BATCH_NAMED NEXT_BATCH_ACTION_PRESENT REMEDIATION_NOT_EMPTY",
            "false_failure" => "CORRECTS_FALSE_FAILURE NO_FALSE_FAILURE_INVENTED PASSED_TRACE_CONFIRMED NO_FAILED_UNIT_TRACE_READ NO_REMEDIATION_TRACE_READ DO_NOT_TRAIN_NEXT_BATCH",
            _ => "CORRECTS_INCOMPLETE_TRACE_SUMMARY TRACE_SUMMARY_COMPLETE ALL_REQUIRED_FIELDS_PRESENT"
        };
        return string.Join(" ", [
            "GOOD_WIKI_TRACE_AGGREGATION",
            defectCorrection,
            failed,
            remediation,
            result.StopBlindTraining ? "STOP_BLIND_TRAINING_TRUE" : "STOP_BLIND_TRAINING_FALSE",
            result.QueryPassRate < 0.75 ? "QUERY_PASS_RATE_BELOW_GATE" : "QUERY_PASS_RATE_ACCEPTABLE",
            "FAILED_UNIT_TRACE_READ",
            "REMEDIATION_TRACE_READ",
            "NEXT_BATCH_DECISION_SUPPORTED"
        ]);
    }

    private static string BuildTargetChosen(WikiTraceAggregationResult result)
        => string.Join(" ", [
            "GOOD_TRACE_AGGREGATOR_RESULT",
            result.FailedUnits.Length == 0 ? "FAILED_UNIT_ABSENT" : "FAILED_UNIT_PRESENT",
            result.RemediationActions.Length == 0 ? "REMEDIATION_NOT_NEEDED" : "REMEDIATION_PRESENT",
            result.StopBlindTraining ? "STOP_BLIND_TRAINING_TRUE" : "STOP_BLIND_TRAINING_FALSE",
            result.QueryPassRate < 0.75 ? "QUERY_PASS_RATE_FAIL" : "QUERY_PASS_RATE_PASS",
            result.StopBlindTraining ? "NEXT_ACTION_REMEDIATE" : "NEXT_ACTION_CONTINUE",
            "TRACE_SUMMARY_COMPLETE"
        ]);

    private static string BuildRejected(string rejectedType, WikiTraceAggregationResult result)
        => rejectedType switch
        {
            "context_before_query_plan_verified" =>
                "failed_units: none\nremediation: none\nstop_blind_training: false\nquery_pass_rate: 0\n[WRONG: context_before_query_plan_verified hides that context routing ran before project, documentation-version, and evidence prechecks were verified by ProjectWikiQueryPlanningFlow.]",
            "answer_before_evidence_analysis" =>
                "failed_units: none\nremediation: none\nstop_blind_training: false\nquery_pass_rate: 0\n[WRONG: answer_before_evidence_analysis allows the answer composer to respond before the retrieved evidence groups are analyzed.]",
            "wrong_owner_for_no_prechecks" =>
                "failed_units: WikiContextRoutingUnit\nremediation: Repeat context-routing label rows.\nstop_blind_training: true\nquery_pass_rate: 0\n[WRONG: wrong_owner_for_no_prechecks assigns a cross-unit missing-query-plan failure to context routing label rows instead of training ProjectWikiQueryPlanningFlow and WikiTraceAggregator.]",
            "missing_sequence_remediation" =>
                $"failed_units: {string.Join(", ", result.FailedUnits)}\nremediation: none\nstop_blind_training: true\nquery_pass_rate: {result.QueryPassRate:0.######}\n[WRONG: missing_sequence_remediation names a sequence failure but gives no concrete next 50-row remediation action.]",
            "ignores_failed_unit" =>
                "failed_units: none\nremediation: none\nstop_blind_training: false\n[WRONG: ignores_failed_unit hides a failing wiki unit and prevents the next 50-row remediation batch.]",
            "continues_blind_training" =>
                $"failed_units: {string.Join(", ", result.FailedUnits)}\nstop_blind_training: false\n[WRONG: continues_blind_training keeps adding rows after flat or failed coverage instead of analyzing the measured gap.]",
            "missing_remediation" =>
                $"failed_units: {string.Join(", ", result.FailedUnits)}\nremediation: none\n[WRONG: missing_remediation gives no concrete next training action for the weak unit.]",
            "false_failure" =>
                "failed_units: ProjectAnswerVerifier\nstop_blind_training: true\n[WRONG: false_failure invents a failed unit when the traces passed and wastes training on the wrong component.]",
            _ =>
                $"failed_units: none\nremediation: none\nstop_blind_training: false\nquery_pass_rate: {result.QueryPassRate:0.######}\n[WRONG: incomplete_trace_summary omits failed units, remediation actions, or stop decision needed by the training protocol.]"
        };

    private static string BuildSignalRejected(string rejectedType, WikiTraceAggregationResult result)
        => rejectedType switch
        {
            "ignores_failed_unit" =>
                $"BAD_WIKI_TRACE_AGGREGATION FAILED_UNITS_NONE REMEDIATION_ACTIONS_PRESENT STOP_BLIND_TRAINING_{TokenSafe(result.StopBlindTraining.ToString())} FAILED_UNIT_TRACE_IGNORED\n[WRONG: ignores_failed_unit hides a failing wiki unit and prevents the next 50-row remediation batch.]",
            "continues_blind_training" =>
                $"BAD_WIKI_TRACE_AGGREGATION {FailedUnitTokens(result)} STOP_BLIND_TRAINING_FALSE BLIND_CONTINUATION_ALLOWED GAP_ANALYSIS_SKIPPED\n[WRONG: continues_blind_training keeps adding rows after flat or failed coverage instead of analyzing the measured gap.]",
            "missing_remediation" =>
                $"BAD_WIKI_TRACE_AGGREGATION {FailedUnitTokens(result)} REMEDIATION_NONE NEXT_BATCH_ACTION_MISSING\n[WRONG: missing_remediation gives no concrete next training action for the weak unit.]",
            "false_failure" =>
                "BAD_WIKI_TRACE_AGGREGATION FAILED_UNIT_PROJECT_ANSWER_VERIFIER STOP_BLIND_TRAINING_TRUE FALSE_FAILURE_INVENTED\n[WRONG: false_failure invents a failed unit when the traces passed and wastes training on the wrong component.]",
            _ =>
                "BAD_WIKI_TRACE_AGGREGATION PARTIAL_TRACE_SUMMARY FAILED_UNITS_OR_REMEDIATION_OR_STOP_DECISION_MISSING\n[WRONG: incomplete_trace_summary omits failed units, remediation actions, or stop decision needed by the training protocol.]"
        };

    private static string BuildTargetRejected(string rejectedType, WikiTraceAggregationResult result)
        => rejectedType switch
        {
            "ignores_failed_unit" =>
                "BAD_TRACE_AGGREGATOR_RESULT FAILED_UNIT_ABSENT REMEDIATION_MISSING STOP_BLIND_TRAINING_FALSE TRACE_SUMMARY_INCOMPLETE\n[WRONG: ignores_failed_unit hides a failing wiki unit and prevents the next 50-row remediation batch.]",
            "continues_blind_training" =>
                "BAD_TRACE_AGGREGATOR_RESULT FAILED_UNIT_PRESENT REMEDIATION_PRESENT STOP_BLIND_TRAINING_FALSE BLIND_CONTINUATION_ALLOWED GAP_ANALYSIS_SKIPPED\n[WRONG: continues_blind_training keeps adding rows after flat or failed coverage instead of analyzing the measured gap.]",
            "missing_remediation" =>
                "BAD_TRACE_AGGREGATOR_RESULT FAILED_UNIT_PRESENT REMEDIATION_MISSING STOP_BLIND_TRAINING_TRUE NEXT_ACTION_MISSING\n[WRONG: missing_remediation gives no concrete next training action for the weak unit.]",
            "false_failure" =>
                "BAD_TRACE_AGGREGATOR_RESULT FAILED_UNIT_PRESENT REMEDIATION_PRESENT STOP_BLIND_TRAINING_TRUE FALSE_FAILURE_INVENTED\n[WRONG: false_failure invents a failed unit when the traces passed and wastes training on the wrong component.]",
            _ =>
                "BAD_TRACE_AGGREGATOR_RESULT TRACE_SUMMARY_INCOMPLETE FAILED_UNIT_UNKNOWN REMEDIATION_UNKNOWN STOP_DECISION_UNKNOWN\n[WRONG: incomplete_trace_summary omits failed units, remediation actions, or stop decision needed by the training protocol.]"
        };

    private static string BuildCrossUnitChosen(string rejectedType, WikiTraceAggregationResult result, bool includeDefectToken)
        => string.Join(" ", [
            "GOOD_CROSS_UNIT_TRACE_AGGREGATION",
            includeDefectToken ? CrossUnitDefectToken(rejectedType) : "TRACE_SEQUENCE_DEFECT_DETECTED",
            result.FailedUnits.Length == 0 ? "FAILED_UNITS_NONE" : FailedUnitTokens(result),
            result.RemediationActions.Length == 0 ? "REMEDIATION_NONE_REQUIRED" : "REMEDIATION_ACTIONS_PRESENT",
            result.StopBlindTraining ? "STOP_BLIND_TRAINING_TRUE" : "STOP_BLIND_TRAINING_FALSE",
            result.QueryPassRate < 0.75 ? "QUERY_PASS_RATE_BELOW_GATE" : "QUERY_PASS_RATE_ACCEPTABLE",
            "QUERY_PLANNING_BEFORE_CONTEXT_REQUIRED",
            "CONTEXT_AFTER_VERIFIED_PLAN_REQUIRED",
            "ANSWER_AFTER_EVIDENCE_ANALYSIS_REQUIRED",
            "NEXT_BATCH_DECISION_SUPPORTED_BY_TRACE"
        ]);

    private static string BuildCrossUnitRejected(string rejectedType, WikiTraceAggregationResult result)
        => rejectedType switch
        {
            "context_before_query_plan_verified" =>
                "BAD_CROSS_UNIT_TRACE_AGGREGATION FAILED_UNIT_ABSENT REMEDIATION_MISSING STOP_BLIND_TRAINING_FALSE CONTEXT_ROUTING_ALLOWED_BEFORE_QUERY_PLAN_VERIFICATION\n[WRONG: context_before_query_plan_verified hides that WikiContextRoutingUnit ran before ProjectWikiQueryPlanningFlow verified project/version/evidence prechecks, so blind routing continues.]",
            "answer_before_evidence_analysis" =>
                "BAD_CROSS_UNIT_TRACE_AGGREGATION FAILED_UNIT_ABSENT REMEDIATION_MISSING STOP_BLIND_TRAINING_FALSE ANSWER_ALLOWED_BEFORE_EVIDENCE_ANALYSIS\n[WRONG: answer_before_evidence_analysis lets WikiAnswerComposer answer from first hits before query-plan evidence groups are analyzed.]",
            "wrong_owner_for_no_prechecks" =>
                "BAD_CROSS_UNIT_TRACE_AGGREGATION FAILED_UNIT_WIKI_CONTEXT_ROUTING_UNIT REMEDIATION_REPEAT_CONTEXT_LABEL_ROWS STOP_BLIND_TRAINING_TRUE WRONG_UNIT_OWNER\n[WRONG: wrong_owner_for_no_prechecks assigns a cross-unit missing-query-plan failure to context routing label rows instead of training ProjectWikiQueryPlanningFlow and WikiTraceAggregator.]",
            "missing_sequence_remediation" =>
                $"BAD_CROSS_UNIT_TRACE_AGGREGATION {FailedUnitTokens(result)} REMEDIATION_MISSING STOP_BLIND_TRAINING_TRUE SEQUENCE_FAILURE_NOT_ACTIONABLE\n[WRONG: missing_sequence_remediation names the failed unit but does not specify the next 50-row remediation batch for the broken orchestration stage.]",
            _ =>
                "BAD_CROSS_UNIT_TRACE_AGGREGATION TRACE_SEQUENCE_IGNORED FAILED_UNIT_UNKNOWN REMEDIATION_UNKNOWN STOP_DECISION_UNKNOWN\n[WRONG: incomplete_trace_summary ignores unit order and cannot prove query planning, context retrieval, and answering happened in the required sequence.]"
        };

    private static string CrossUnitDefectToken(string rejectedType)
        => rejectedType switch
        {
            "context_before_query_plan_verified" => "DETECTED_CONTEXT_BEFORE_QUERY_PLAN_VERIFIED",
            "answer_before_evidence_analysis" => "DETECTED_ANSWER_BEFORE_EVIDENCE_ANALYSIS",
            "wrong_owner_for_no_prechecks" => "DETECTED_NO_PRECHECKS_OWNER_IS_QUERY_PLANNING_AND_TRACE_AGGREGATOR",
            "missing_sequence_remediation" => "DETECTED_SEQUENCE_REMEDIATION_REQUIRED",
            _ => "DETECTED_INCOMPLETE_TRACE_SUMMARY"
        };

    private static string FailedUnitTokens(WikiTraceAggregationResult result)
        => result.FailedUnits.Length == 0
            ? "FAILED_UNITS_NONE"
            : string.Join(" ", result.FailedUnits.Select(unit => $"FAILED_UNIT_{TokenSafe(unit)}"));

    private static TraceSeed[] BuildSeeds(bool heldOut)
    {
        var failedUnits = heldOut
            ? new[] { EntityImpactMapperUnit.UnitId, ProjectAnswerVerifier.UnitId, WikiContextRoutingUnit.UnitId, CodeTraceBuilderUnit.UnitId, DocumentationGeneratorUnit.UnitId }
            : new[] { ProjectWikiQueryPlanningFlow.FlowId, WikiContextRoutingUnit.UnitId, ProjectWikiAnswerComposerTrainingRunner.UnitId, EntityImpactMapperUnit.UnitId, ProjectAnswerVerifier.UnitId };
        var rows = new List<TraceSeed>(50);
        for (var repeat = 0; repeat < 10; repeat++)
        {
            foreach (var failedUnit in failedUnits)
            {
                var rejectedType = RejectedType(repeat);
                var pass = repeat % 5 == 4;
                var unitResult = new WikiUnitTraceResult(
                    failedUnit,
                    "v1",
                    "checkpoint",
                    "input",
                    pass ? "passed" : "failed",
                    pass ? 0.95 : 0.62,
                    pass ? "PASS" : "FAIL",
                    pass ? "" : "unit_quality_below_gate",
                    pass ? "" : $"Add 50 remediation rows for {failedUnit}.");
                var queryTrace = QueryTrace($"q-{repeat}-{failedUnit}", pass, unitResult);
                var unitTrace = new WikiUnitTrace(
                    $"unit-trace-{repeat}-{failedUnit}",
                    50,
                    50,
                    [unitResult],
                    pass ? [] : [failedUnit],
                    pass ? [] : [$"Add 50 remediation rows for {failedUnit}."],
                    pass ? 0.02 : 0,
                    !pass);
                rows.Add(new TraceSeed(
                    $"trace-batch-{repeat}-{failedUnit}",
                    [queryTrace],
                    [unitTrace],
                    rejectedType));
            }
        }

        return rows.Take(50).ToArray();
    }

    private static TraceSeed[] BuildCrossUnitSeeds(bool heldOut)
    {
        var rows = new List<TraceSeed>(50);
        var failedUnits = heldOut
            ? new[] { WikiContextRoutingUnit.UnitId, ProjectWikiAnswerComposerTrainingRunner.UnitId, ProjectWikiQueryPlanningFlow.FlowId, WikiTraceAggregatorUnit.UnitId, EntityImpactMapperUnit.UnitId }
            : new[] { ProjectWikiQueryPlanningFlow.FlowId, WikiContextRoutingUnit.UnitId, ProjectWikiAnswerComposerTrainingRunner.UnitId, WikiTraceAggregatorUnit.UnitId, ProjectAnswerVerifier.UnitId };
        for (var repeat = 0; repeat < 10; repeat++)
        {
            foreach (var failedUnit in failedUnits)
            {
                var rejectedType = CrossUnitRejectedType(repeat);
                var unitResult = new WikiUnitTraceResult(
                    failedUnit,
                    "v1",
                    "checkpoint",
                    CrossUnitInputSummary(rejectedType),
                    CrossUnitOutputSummary(rejectedType),
                    0.58,
                    "FAIL",
                    rejectedType,
                    CrossUnitRemediation(failedUnit, rejectedType));
                var queryTrace = CrossUnitQueryTrace($"cross-{repeat}-{failedUnit}", rejectedType, unitResult);
                var unitTrace = new WikiUnitTrace(
                    $"cross-unit-trace-{repeat}-{failedUnit}",
                    50,
                    50,
                    [unitResult],
                    [failedUnit],
                    [CrossUnitRemediation(failedUnit, rejectedType)],
                    0,
                    true);
                rows.Add(new TraceSeed(
                    $"cross-unit-batch-{repeat}-{failedUnit}",
                    [queryTrace],
                    [unitTrace],
                    rejectedType));
            }
        }

        return rows.Take(50).ToArray();
    }

    private static TraceSeed[] BuildCrossUnitOwnerSeeds(bool heldOut)
    {
        var failedUnits = heldOut
            ? new[] { WikiContextRoutingUnit.UnitId, ProjectWikiQueryPlanningFlow.FlowId, WikiTraceAggregatorUnit.UnitId, EntityImpactMapperUnit.UnitId, ProjectAnswerVerifier.UnitId }
            : new[] { ProjectWikiQueryPlanningFlow.FlowId, WikiContextRoutingUnit.UnitId, WikiTraceAggregatorUnit.UnitId, ProjectWikiAnswerComposerTrainingRunner.UnitId, ProjectAnswerVerifier.UnitId };
        var rows = new List<TraceSeed>(50);
        for (var repeat = 0; repeat < 10; repeat++)
        {
            foreach (var failedUnit in failedUnits)
            {
                var rejectedType = (repeat % 5) switch
                {
                    0 or 3 => "wrong_owner_for_no_prechecks",
                    1 or 4 => "context_before_query_plan_verified",
                    _ => "missing_sequence_remediation"
                };
                var ownerUnit = string.Equals(rejectedType, "wrong_owner_for_no_prechecks", StringComparison.Ordinal)
                    ? WikiTraceAggregatorUnit.UnitId
                    : failedUnit;
                var unitResult = new WikiUnitTraceResult(
                    ownerUnit,
                    "v1",
                    "checkpoint",
                    CrossUnitInputSummary(rejectedType),
                    CrossUnitOutputSummary(rejectedType),
                    0.60,
                    "FAIL",
                    rejectedType,
                    CrossUnitRemediation(ownerUnit, rejectedType));
                var queryTrace = CrossUnitQueryTrace($"owner-{repeat}-{failedUnit}", rejectedType, unitResult);
                var unitTrace = new WikiUnitTrace(
                    $"owner-trace-{repeat}-{failedUnit}",
                    50,
                    50,
                    [unitResult],
                    [ownerUnit],
                    [CrossUnitRemediation(ownerUnit, rejectedType)],
                    0,
                    true);
                rows.Add(new TraceSeed(
                    $"owner-batch-{repeat}-{failedUnit}",
                    [queryTrace],
                    [unitTrace],
                    rejectedType));
            }
        }

        return rows.Take(50).ToArray();
    }

    private static TraceSeed[] BuildCrossUnitFocusedSeeds(bool heldOut, string rejectedType)
    {
        var failedUnits = heldOut
            ? new[] { WikiContextRoutingUnit.UnitId, ProjectWikiQueryPlanningFlow.FlowId, WikiTraceAggregatorUnit.UnitId, EntityImpactMapperUnit.UnitId, ProjectAnswerVerifier.UnitId }
            : new[] { ProjectWikiQueryPlanningFlow.FlowId, WikiContextRoutingUnit.UnitId, WikiTraceAggregatorUnit.UnitId, ProjectWikiAnswerComposerTrainingRunner.UnitId, ProjectAnswerVerifier.UnitId };
        var rows = new List<TraceSeed>(50);
        for (var repeat = 0; repeat < 10; repeat++)
        {
            foreach (var failedUnit in failedUnits)
            {
                var unitResult = new WikiUnitTraceResult(
                    failedUnit,
                    "v1",
                    "checkpoint",
                    CrossUnitInputSummary(rejectedType),
                    CrossUnitOutputSummary(rejectedType),
                    0.60,
                    "FAIL",
                    rejectedType,
                    CrossUnitRemediation(failedUnit, rejectedType));
                var queryTrace = CrossUnitQueryTrace($"focused-{repeat}-{failedUnit}", rejectedType, unitResult);
                var unitTrace = new WikiUnitTrace(
                    $"focused-trace-{repeat}-{failedUnit}",
                    50,
                    50,
                    [unitResult],
                    [failedUnit],
                    [CrossUnitRemediation(failedUnit, rejectedType)],
                    0,
                    true);
                rows.Add(new TraceSeed(
                    $"focused-batch-{repeat}-{failedUnit}",
                    [queryTrace],
                    [unitTrace],
                    rejectedType));
            }
        }

        return rows.Take(50).ToArray();
    }

    private static TraceSeed[] BuildFalseFailureFocusedSeeds(bool heldOut)
    {
        var passedUnits = heldOut
            ? new[] { EntityImpactMapperUnit.UnitId, ProjectAnswerVerifier.UnitId, WikiContextRoutingUnit.UnitId, CodeTraceBuilderUnit.UnitId, DocumentationGeneratorUnit.UnitId }
            : new[] { ProjectWikiQueryPlanningFlow.FlowId, WikiContextRoutingUnit.UnitId, ProjectWikiAnswerComposerTrainingRunner.UnitId, EntityImpactMapperUnit.UnitId, ProjectAnswerVerifier.UnitId };
        var rows = new List<TraceSeed>(50);
        for (var repeat = 0; repeat < 10; repeat++)
        {
            foreach (var passedUnit in passedUnits)
            {
                var unitResult = new WikiUnitTraceResult(
                    passedUnit,
                    "v1",
                    "checkpoint",
                    "all required wiki trace fields were verified",
                    "unit passed with evidence and no remediation required",
                    0.96,
                    "PASS",
                    "",
                    "");
                var queryTrace = QueryTrace($"false-failure-pass-{repeat}-{passedUnit}", true, unitResult);
                var unitTrace = new WikiUnitTrace(
                    $"false-failure-unit-trace-{repeat}-{passedUnit}",
                    50,
                    50,
                    [unitResult],
                    [],
                    [],
                    0.05,
                    false);
                rows.Add(new TraceSeed(
                    $"false-failure-batch-{repeat}-{passedUnit}",
                    [queryTrace],
                    [unitTrace],
                    "false_failure"));
            }
        }

        return rows.Take(50).ToArray();
    }

    private static WikiQueryTrace QueryTrace(string id, bool pass, WikiUnitTraceResult unitResult)
        => new(
            id,
            "generic-dotnet-docs",
            "doc-v1",
            "entity_impact_question",
            "Level 3 developer change reasoning",
            true,
            $"plan-{id}",
            nameof(ProjectWikiQueryIntent.EntityImpact),
            ["Campaign"],
            ["step-1:EntityDefinitionLookup", "step-2:CallerLookup"],
            ["step-1", "step-2"],
            [],
            [],
            pass ? 0.95 : 0.62,
            pass ? 0.95 : 0.62,
            [unitResult.UnitId],
            [unitResult],
            pass ? ["ev:Campaign"] : [],
            pass ? ["ev:Campaign"] : [],
            pass ? [] : ["ev:Campaign"],
            pass ? 0.9 : 0.55,
            pass ? "PASS" : "FAIL",
            1,
            1,
            1,
            pass ? "PASS" : "FAIL");

    private static WikiQueryTrace CrossUnitQueryTrace(string id, string rejectedType, WikiUnitTraceResult unitResult)
    {
        var planned = new[] { "step-1:ProjectBoundaryCheck", "step-2:DocumentationVersionCheck", "step-3:EvidenceCompletenessCheck", "step-4:ContextRetrieval", "step-5:EvidenceAnalysis", "step-6:AnswerComposition" };
        var executed = rejectedType switch
        {
            "context_before_query_plan_verified" => new[] { "step-4:ContextRetrieval", "step-1:ProjectBoundaryCheck" },
            "answer_before_evidence_analysis" => new[] { "step-1:ProjectBoundaryCheck", "step-2:DocumentationVersionCheck", "step-4:ContextRetrieval", "step-6:AnswerComposition" },
            "wrong_owner_for_no_prechecks" => new[] { "step-4:ContextRetrieval" },
            "missing_sequence_remediation" => new[] { "step-1:ProjectBoundaryCheck", "step-2:DocumentationVersionCheck", "step-3:EvidenceCompletenessCheck" },
            _ => new[] { "step-1:ProjectBoundaryCheck" }
        };
        var skipped = planned.Except(executed, StringComparer.Ordinal).ToArray();
        return new WikiQueryTrace(
            id,
            "generic-dotnet-docs",
            "doc-v1",
            "entity_impact_question",
            "Level 3 developer change reasoning",
            true,
            $"plan-{id}",
            nameof(ProjectWikiQueryIntent.EntityImpact),
            ["Campaign", "DiscountCode"],
            planned,
            executed,
            skipped,
            skipped,
            0.58,
            0.42,
            [ProjectWikiQueryPlanningFlow.FlowId, WikiContextRoutingUnit.UnitId, ProjectWikiAnswerComposerTrainingRunner.UnitId],
            [unitResult],
            [],
            [],
            skipped,
            0.48,
            "FAIL",
            1,
            1,
            1,
            "FAIL");
    }

    private static string RejectedType(int repeat)
        => (repeat % 5) switch
        {
            0 => "ignores_failed_unit",
            1 => "continues_blind_training",
            2 => "missing_remediation",
            3 => "incomplete_trace_summary",
            _ => "false_failure"
        };

    private static string CrossUnitRejectedType(int repeat)
        => (repeat % 5) switch
        {
            0 => "context_before_query_plan_verified",
            1 => "answer_before_evidence_analysis",
            2 => "wrong_owner_for_no_prechecks",
            3 => "missing_sequence_remediation",
            _ => "incomplete_trace_summary"
        };

    private static string CrossUnitInputSummary(string rejectedType)
        => rejectedType switch
        {
            "context_before_query_plan_verified" => "context routing was invoked before project/version/evidence prechecks were verified",
            "answer_before_evidence_analysis" => "answer composition happened before retrieved evidence groups were analyzed",
            "wrong_owner_for_no_prechecks" => "no_pre_checks debt was assigned to context labels instead of the query-plan sequence",
            "missing_sequence_remediation" => "trace names sequence failure but does not name next training action",
            _ => "trace summary omits the sequence proof"
        };

    private static string CrossUnitOutputSummary(string rejectedType)
        => rejectedType switch
        {
            "context_before_query_plan_verified" => "ProjectWikiQueryPlanningFlow precheck order is missing before context routing",
            "answer_before_evidence_analysis" => "WikiAnswerComposer answered before evidence-analysis readiness",
            "wrong_owner_for_no_prechecks" => "failed owner selection for no_pre_checks",
            "missing_sequence_remediation" => "remediation action is missing",
            _ => "incomplete trace aggregation"
        };

    private static string CrossUnitRemediation(string failedUnit, string rejectedType)
        => rejectedType switch
        {
            "context_before_query_plan_verified" => $"Add 50 cross-unit trace rows requiring ProjectWikiQueryPlanningFlow to verify prechecks before {failedUnit}.",
            "answer_before_evidence_analysis" => $"Add 50 answer-sequence trace rows requiring evidence analysis before {failedUnit}.",
            "wrong_owner_for_no_prechecks" => "Stop WikiContextRoutingUnit label-only remediation; train ProjectWikiQueryPlanningFlow and WikiTraceAggregator on query-plan proof rows.",
            "missing_sequence_remediation" => $"Add 50 remediation-action rows for {failedUnit} with explicit next-batch owner.",
            _ => $"Add 50 incomplete-sequence trace rows for {failedUnit}."
        };

    private sealed record TraceSeed(
        string BatchId,
        WikiQueryTrace[] QueryTraces,
        WikiUnitTrace[] UnitTraces,
        string RejectedType);

    private static string TokenSafe(string value)
    {
        var chars = value.Select(ch => char.IsLetterOrDigit(ch) ? char.ToUpperInvariant(ch) : '_').ToArray();
        return new string(chars).Trim('_');
    }
}
