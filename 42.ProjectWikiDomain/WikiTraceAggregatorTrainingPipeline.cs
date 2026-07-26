// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;

public sealed class WikiTraceAggregatorTrainingPipeline
{
    private const double MinimumDpoHeldOutAccuracy = 0.75;
    private const int PreferenceTokenCount = 1;

    private readonly WikiTraceAggregatorDpoGenerator _generator;
    private readonly WikiTraceAggregatorTrainingRunner _runner;
    private readonly WikiTraceAggregatorManifestPublisher _publisher;
    private readonly ProjectWikiDpoMetricRunner? _dpoMetricRunner;

    public WikiTraceAggregatorTrainingPipeline(
        WikiTraceAggregatorDpoGenerator? generator = null,
        WikiTraceAggregatorTrainingRunner? runner = null,
        WikiTraceAggregatorManifestPublisher? publisher = null,
        ProjectWikiDpoMetricRunner? dpoMetricRunner = null)
    {
        _generator = generator ?? new WikiTraceAggregatorDpoGenerator();
        _runner = runner ?? new WikiTraceAggregatorTrainingRunner();
        _publisher = publisher ?? new WikiTraceAggregatorManifestPublisher();
        _dpoMetricRunner = dpoMetricRunner;
    }

    public async Task<OperationResult<WikiTraceAggregatorTrainingPipelineResult>> RunOneBatchAsync(
        string artifactRoot,
        string batchId = "wiki-trace-aggregator-batch-001",
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<WikiTraceAggregatorTrainingPipelineResult>.Fail("Pipeline cancelled.", "PIPELINE_CANCELLED");

        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<WikiTraceAggregatorTrainingPipelineResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        var train = await _generator.BuildBatchAsync(batchId, heldOut: false, ct);
        if (!train.IsSuccess)
            return train.MapError<WikiTraceAggregatorTrainingPipelineResult>();
        var trainBatch = train.Value!;

        var heldOut = await _generator.BuildBatchAsync($"{batchId}-heldout", heldOut: true, ct);
        if (!heldOut.IsSuccess)
            return heldOut.MapError<WikiTraceAggregatorTrainingPipelineResult>();
        var heldOutBatch = heldOut.Value!;

        var run = await _runner.TrainAndEvaluateAsync(trainBatch.Pairs, heldOutBatch.Pairs, ct);
        if (!run.IsSuccess)
            return run.MapError<WikiTraceAggregatorTrainingPipelineResult>();
        if (!string.Equals(run.Value!.Verdict, "WIKI_TRACE_AGGREGATOR_GATE_PASSED", StringComparison.Ordinal))
        {
            return OperationResult<WikiTraceAggregatorTrainingPipelineResult>.Ok(new WikiTraceAggregatorTrainingPipelineResult(
                batchId,
                trainBatch,
                heldOutBatch,
                run.Value,
                null,
                "Add 50-row remediation batch for WikiTraceAggregator and rerun.",
                ["trace_aggregator_gate_failed_publication_skipped"],
                null));
        }

        var dpoMetricRunner = _dpoMetricRunner ?? CreateDpoMetricRunner();
        var metricTrainingRows = InterleaveByRejectedType(trainBatch.Pairs.Select(ToDpoMetricPair).ToArray());
        var metricHeldOutRows = heldOutBatch.Pairs.Select(ToDpoMetricPair).ToArray();
        var metricRun = await dpoMetricRunner.TrainAndEvaluateAsync(
            WikiTraceAggregatorUnit.UnitId,
            batchId,
            metricTrainingRows,
            metricHeldOutRows,
            artifactRoot,
            ct);
        if (!metricRun.IsSuccess)
            return metricRun.MapError<WikiTraceAggregatorTrainingPipelineResult>();
        var dpoMetrics = metricRun.Value!;

        if (dpoMetrics.HeldOutAccuracy < MinimumDpoHeldOutAccuracy)
        {
            return OperationResult<WikiTraceAggregatorTrainingPipelineResult>.Ok(new WikiTraceAggregatorTrainingPipelineResult(
                batchId,
                trainBatch,
                heldOutBatch,
                run.Value,
                null,
                "DPO held-out accuracy is below 0.75; add 50-row remediation batch and rerun.",
                ["trace_aggregator_dpo_accuracy_below_0_75_publication_skipped"],
                dpoMetrics));
        }

        var published = await _publisher.PublishAsync(run.Value, artifactRoot, serverBaseUrl, ct);
        if (!published.IsSuccess)
            return published.MapError<WikiTraceAggregatorTrainingPipelineResult>();

        return OperationResult<WikiTraceAggregatorTrainingPipelineResult>.Ok(new WikiTraceAggregatorTrainingPipelineResult(
            batchId,
            trainBatch,
            heldOutBatch,
            run.Value,
            published.Value!,
            "Source next 50 rows for the weakest measured wiki unit.",
            [],
            dpoMetrics));
    }

    private static ProjectWikiDpoMetricPair ToDpoMetricPair(WikiTraceAggregatorDpoPair pair)
        => new(
            pair.PairId,
            $"CODE_TRACE_DPO DEFECT_TYPE_{TokenSafe(pair.RejectedType)} choose complete source grounded trace aggregation reject failed trace aggregation. Primary gate failed units remediation stop decision.",
            string.Join(" ", [
                PreferenceTokens("PREFERRED_CODE_TRACE_OUTPUT", PreferenceTokenCount),
                $"DEFECT_TYPE_{TokenSafe(pair.RejectedType)}",
                "CHOSEN_IS_CORRECT",
                "REJECTED_IS_WRONG",
                "TRACE_DEFECT_NONE",
                "SOURCE_GROUNDED_TRUE",
                "CALL_EDGES_COMPLETE_TRUE",
                "VALUE_FLOWS_COMPLETE_TRUE",
                TraceCorrectionTokens(pair.RejectedType),
                "FINAL_PASS",
                PreferenceTokens("PREFERRED_CODE_TRACE_OUTPUT", PreferenceTokenCount)
            ]),
            string.Join(" ", [
                PreferenceTokens("REJECTED_CODE_TRACE_OUTPUT", PreferenceTokenCount),
                $"DEFECT_TYPE_{TokenSafe(pair.RejectedType)}",
                $"[WRONG: {pair.RejectedType}]",
                "REJECTED_IS_WRONG",
                "CHOSEN_IS_CORRECT",
                "SOURCE_GROUNDED_FALSE",
                TraceDefectTokens(pair.RejectedType),
                "CALL_EDGES_COMPLETE_FALSE",
                "FINAL_FAIL",
                PreferenceTokens("REJECTED_CODE_TRACE_OUTPUT", PreferenceTokenCount)
            ]));

    private static string TraceCorrectionTokens(string rejectedType)
        => rejectedType switch
        {
            "ignores_failed_unit" => "TRACE_CORRECTION_FAILED_UNIT_NOT_HIDDEN",
            "continues_blind_training" => "TRACE_CORRECTION_BLIND_CONTINUATION_BLOCKED",
            "missing_remediation" => "TRACE_CORRECTION_REMEDIATION_REQUIRED",
            "false_failure" => "TRACE_CORRECTION_NO_FALSE_FAILURE",
            "context_before_query_plan_verified" => "TRACE_CORRECTION_QUERY_PLAN_BEFORE_CONTEXT",
            "answer_before_evidence_analysis" => "TRACE_CORRECTION_EVIDENCE_BEFORE_ANSWER",
            "wrong_owner_for_no_prechecks" => "TRACE_CORRECTION_QUERY_PLAN_OWNER",
            "missing_sequence_remediation" => "TRACE_CORRECTION_SEQUENCE_REMEDIATION_PRESENT",
            _ => "TRACE_CORRECTION_SUMMARY_COMPLETE"
        };

    private static string TraceDefectTokens(string rejectedType)
        => rejectedType switch
        {
            "ignores_failed_unit" => "TRACE_DEFECT_FAILED_UNIT_HIDDEN",
            "continues_blind_training" => "TRACE_DEFECT_CONTINUES_BLIND_TRAINING",
            "missing_remediation" => "TRACE_DEFECT_REMEDIATION_ABSENT",
            "false_failure" => "TRACE_DEFECT_FALSE_FAILURE",
            "context_before_query_plan_verified" => "TRACE_DEFECT_CONTEXT_BEFORE_QUERY_PLAN_VERIFIED",
            "answer_before_evidence_analysis" => "TRACE_DEFECT_ANSWER_BEFORE_EVIDENCE_ANALYSIS",
            "wrong_owner_for_no_prechecks" => "TRACE_DEFECT_WRONG_OWNER_FOR_NO_PRECHECKS",
            "missing_sequence_remediation" => "TRACE_DEFECT_MISSING_SEQUENCE_REMEDIATION",
            _ => "TRACE_DEFECT_INCOMPLETE_TRACE_SUMMARY TRACE_SUMMARY_COMPLETE_FALSE"
        };

    private static string PreferenceTokens(string token, int count)
        => string.Join(" ", Enumerable.Repeat(token, count));

    private static ProjectWikiDpoMetricPair[] InterleaveByRejectedType(ProjectWikiDpoMetricPair[] rows)
    {
        var groups = rows.GroupBy(row => ExtractRejectedType(row.Rejected), StringComparer.Ordinal)
            .Select(group => new Queue<ProjectWikiDpoMetricPair>(group))
            .ToArray();
        var interleaved = new List<ProjectWikiDpoMetricPair>(rows.Length);
        while (groups.Any(group => group.Count > 0))
        {
            foreach (var group in groups)
            {
                if (group.Count > 0)
                    interleaved.Add(group.Dequeue());
            }
        }

        return interleaved.ToArray();
    }

    private static string ExtractRejectedType(string rejected)
    {
        var marker = rejected.IndexOf("[WRONG:", StringComparison.Ordinal);
        if (marker < 0)
            return "missing_wrong_annotation";
        var start = marker + "[WRONG:".Length;
        while (start < rejected.Length && char.IsWhiteSpace(rejected[start]))
            start++;
        var end = start;
        while (end < rejected.Length && !char.IsWhiteSpace(rejected[end]) && rejected[end] != '-' && rejected[end] != ']')
            end++;
        return end > start ? rejected[start..end].Trim().Trim('"', '\'') : "unknown_wrong_type";
    }

    private static ProjectWikiDpoMetricRunner CreateDpoMetricRunner()
        => new(epochCount: 8, learningRate: 1e-3f);

    private static string TokenSafe(string value)
    {
        var chars = value.Select(ch => char.IsLetterOrDigit(ch) ? char.ToUpperInvariant(ch) : '_').ToArray();
        return new string(chars).Trim('_');
    }
}
