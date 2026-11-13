// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;

public sealed class ProjectWikiRequestRoutingTrainingPipeline
{
    private const double MinimumSignalHeldOutAccuracy = 0.75;
    private const double DpoWatchHeldOutAccuracy = 0.92;
    private const int PreferenceTokenCount = 12;

    private readonly ProjectWikiRequestRoutingDpoGenerator _generator;
    private readonly ProjectWikiRequestRoutingTrainingRunner _runner;
    private readonly ProjectWikiRequestRoutingManifestPublisher _publisher;
    private readonly ProjectWikiDpoMetricRunner _dpoMetricRunner;

    public ProjectWikiRequestRoutingTrainingPipeline(
        ProjectWikiRequestRoutingDpoGenerator? generator = null,
        ProjectWikiRequestRoutingTrainingRunner? runner = null,
        ProjectWikiRequestRoutingManifestPublisher? publisher = null,
        ProjectWikiDpoMetricRunner? dpoMetricRunner = null)
    {
        _generator = generator ?? new ProjectWikiRequestRoutingDpoGenerator();
        _runner = runner ?? new ProjectWikiRequestRoutingTrainingRunner();
        _publisher = publisher ?? new ProjectWikiRequestRoutingManifestPublisher();
        _dpoMetricRunner = dpoMetricRunner ?? new ProjectWikiDpoMetricRunner(epochCount: 8, learningRate: 1e-3f, deterministicFreshInitialization: true);
    }

    public async Task<OperationResult<ProjectWikiRequestRoutingTrainingPipelineResult>> RunOneBatchAsync(
        string artifactRoot,
        string batchId = "project-wiki-request-routing-batch-001",
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<ProjectWikiRequestRoutingTrainingPipelineResult>.Fail("Pipeline cancelled.", "PIPELINE_CANCELLED");

        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<ProjectWikiRequestRoutingTrainingPipelineResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        var train = await _generator.BuildBatchAsync(batchId, heldOut: false, ct);
        if (!train.IsSuccess)
            return train.MapError<ProjectWikiRequestRoutingTrainingPipelineResult>();
        var trainBatch = train.Value!;

        var heldOut = await _generator.BuildBatchAsync($"{batchId}-heldout", heldOut: true, ct);
        if (!heldOut.IsSuccess)
            return heldOut.MapError<ProjectWikiRequestRoutingTrainingPipelineResult>();
        var heldOutBatch = heldOut.Value!;

        var run = await _runner.TrainAndEvaluateAsync(trainBatch.Pairs, heldOutBatch.Pairs, ct);
        if (!run.IsSuccess)
            return run.MapError<ProjectWikiRequestRoutingTrainingPipelineResult>();

        var metricTrain = await _generator.BuildBatchAsync($"{batchId}-signal-token", heldOut: false, ct);
        if (!metricTrain.IsSuccess)
            return metricTrain.MapError<ProjectWikiRequestRoutingTrainingPipelineResult>();

        var metricHeldOut = await _generator.BuildBatchAsync($"{batchId}-heldout-signal-token", heldOut: true, ct);
        if (!metricHeldOut.IsSuccess)
            return metricHeldOut.MapError<ProjectWikiRequestRoutingTrainingPipelineResult>();

        var dpoMetrics = await _dpoMetricRunner.TrainAndEvaluateAsync(
            ProjectWikiRequestRouterUnit.UnitId,
            batchId,
            InterleaveByRejectedType(metricTrain.Value!.Pairs).Select(ToDpoMetricPair).ToArray(),
            InterleaveByRejectedType(metricHeldOut.Value!.Pairs).Select(ToDpoMetricPair).ToArray(),
            artifactRoot,
            ct);
        if (!dpoMetrics.IsSuccess)
            return dpoMetrics.MapError<ProjectWikiRequestRoutingTrainingPipelineResult>();
        var metrics = dpoMetrics.Value!;

        var snapshot = run.Value!.ModelSnapshot;
        var snapshotAllowsPublication = snapshot is not null && snapshot.PublishAllowed;
        if (!string.Equals(run.Value.Verdict, "PROJECT_WIKI_REQUEST_ROUTING_GATE_PASSED", StringComparison.Ordinal) ||
            metrics.HeldOutAccuracy < MinimumSignalHeldOutAccuracy ||
            !snapshotAllowsPublication)
        {
            var warnings = new List<string>();
            if (!string.Equals(run.Value.Verdict, "PROJECT_WIKI_REQUEST_ROUTING_GATE_PASSED", StringComparison.Ordinal))
                warnings.Add("request_routing_structural_gate_failed_publication_skipped");
            if (metrics.HeldOutAccuracy < MinimumSignalHeldOutAccuracy)
                warnings.Add("request_routing_dpo_accuracy_below_0_75_publication_skipped");
            if (snapshot is null)
                warnings.Add("request_routing_snapshot_missing_publication_skipped");
            else if (!snapshot.GatePassed)
                warnings.Add("request_routing_snapshot_gate_failed_publication_skipped");
            else if (!snapshot.SemanticQualityClaimed)
                warnings.Add("request_routing_semantic_quality_false_publication_skipped");
            else if (!snapshot.PublishAllowed)
                warnings.Add("request_routing_snapshot_not_publishable_publication_skipped");

            return OperationResult<ProjectWikiRequestRoutingTrainingPipelineResult>.Ok(new ProjectWikiRequestRoutingTrainingPipelineResult(
                batchId,
                trainBatch,
                heldOutBatch,
                run.Value,
                null,
                metrics.HeldOutAccuracy < MinimumSignalHeldOutAccuracy
                    ? "Add 50-row remediation batch for ProjectWikiRequestRouterUnit and rerun."
                    : !snapshotAllowsPublication
                    ? "Keep ProjectWikiRequestRouterUnit unpublished until snapshot evidence claims semantic quality through an approved gate."
                    : "Continue 50-row ProjectWikiRequestRouter training; route gate is not reached.",
                warnings.ToArray(),
                metrics));
        }

        var published = await _publisher.PublishAsync(run.Value, artifactRoot, serverBaseUrl, ct);
        if (!published.IsSuccess)
            return published.MapError<ProjectWikiRequestRoutingTrainingPipelineResult>();
        var publicationWarnings = metrics.HeldOutAccuracy < DpoWatchHeldOutAccuracy
            ? new[] { "request_routing_dpo_accuracy_below_0_92_continue_negative_dpo_remediation" }
            : [];

        return OperationResult<ProjectWikiRequestRoutingTrainingPipelineResult>.Ok(new ProjectWikiRequestRoutingTrainingPipelineResult(
            batchId,
            trainBatch,
            heldOutBatch,
            run.Value,
            published.Value!,
            "Source next 50 rows for the weakest measured wiki unit.",
            publicationWarnings,
            metrics));
    }

    private static ProjectWikiDpoMetricPair ToDpoMetricPair(ProjectWikiRequestRoutingDpoPair pair)
    {
        var rejectedToken = NormalizeToken(pair.RejectedType);
        var preferred = PreferenceTokens("PREFERRED_CODE_TRACE_OUTPUT", PreferenceTokenCount);
        var rejected = PreferenceTokens("REJECTED_CODE_TRACE_OUTPUT", PreferenceTokenCount);
        return new ProjectWikiDpoMetricPair(
            pair.PairId,
            string.Join(" ", [
                "CODE_TRACE_DPO",
                $"DEFECT_TYPE_{rejectedToken}",
                "choose complete source grounded route reject failed route. Primary gate route taxonomy project unit training boundary."
            ]),
            string.Join(" ", [
                preferred,
                $"DEFECT_TYPE_{rejectedToken}",
                "CHOSEN_IS_CORRECT",
                "REJECTED_IS_WRONG",
                "TRACE_DEFECT_NONE",
                "SOURCE_GROUNDED_TRUE",
                "CALL_EDGES_COMPLETE_TRUE",
                "VALUE_FLOWS_COMPLETE_TRUE",
                RouteCorrectionTokens(pair.RejectedType),
                "FINAL_PASS",
                preferred
            ]),
            string.Join(" ", [
                rejected,
                $"DEFECT_TYPE_{rejectedToken}",
                $"[WRONG: {pair.RejectedType}]",
                "REJECTED_IS_WRONG",
                "CHOSEN_IS_CORRECT",
                "SOURCE_GROUNDED_FALSE",
                RouteDefectTokens(pair.RejectedType),
                "CALL_EDGES_COMPLETE_FALSE",
                "FINAL_FAIL",
                rejected
            ]));
    }

    private static string NormalizeToken(string value)
        => value.ToUpperInvariant().Replace('-', '_');

    private static string RouteCorrectionTokens(string rejectedType)
        => rejectedType switch
        {
            "route_to_answer_without_planning" =>
                "ROUTE_TO_ANSWER_WITHOUT_PLANNING_FALSE QUERY_PLANNING_REQUIRED_TRUE NO_DIRECT_ANSWER_TRUE",
            "route_training_to_project_repo" =>
                "ROUTE_TRAINING_TO_PROJECT_REPO_FALSE TRAINING_STAYS_IN_CORE_TRUE NO_PROJECT_REPO_STORAGE_TRUE",
            "route_to_static_script" =>
                "ROUTE_TO_STATIC_SCRIPT_FALSE TRAINABLE_UNIT_SELECTED_TRUE",
            "wrong_training_unit" =>
                "WRONG_TRAINING_UNIT_FALSE EXPECTED_UNIT_SELECTED_TRUE",
            "route_non_wiki_to_project_wiki" =>
                "ROUTE_NON_WIKI_TO_PROJECT_WIKI_FALSE NON_WIKI_PROMPT_REJECTED_TRUE",
            "route_validation_to_answer" =>
                "ROUTE_VALIDATION_TO_ANSWER_FALSE VALIDATION_GATE_REQUIRED_TRUE",
            "route_project_unit_to_core" =>
                "ROUTE_PROJECT_UNIT_TO_CORE_FALSE PROJECT_UNIT_STORAGE_BOUNDARY_TRUE",
            "route_to_non_wiki" =>
                "ROUTE_TO_NON_WIKI_FALSE PROJECT_WIKI_PATH_SELECTED_TRUE",
            _ => "ROUTE_DEFECT_FALSE CORRECT_ROUTE_SELECTED_TRUE"
        };

    private static string RouteDefectTokens(string rejectedType)
        => rejectedType switch
        {
            "route_to_answer_without_planning" =>
                "ROUTE_DEFECT_ANSWER_WITHOUT_PLANNING",
            "route_training_to_project_repo" =>
                "ROUTE_DEFECT_PROJECT_REPO_TRAINING_STORAGE",
            "route_to_static_script" =>
                "ROUTE_DEFECT_STATIC_SCRIPT",
            "wrong_training_unit" =>
                "ROUTE_DEFECT_WRONG_TRAINING_UNIT",
            "route_non_wiki_to_project_wiki" =>
                "ROUTE_DEFECT_FALSE_PROJECT_WIKI",
            "route_validation_to_answer" =>
                "ROUTE_DEFECT_VALIDATION_AS_ANSWER",
            "route_project_unit_to_core" =>
                "ROUTE_DEFECT_PROJECT_UNIT_TO_CORE",
            "route_to_non_wiki" =>
                "ROUTE_DEFECT_DROP_PROJECT_WIKI",
            _ => "ROUTE_DEFECT_UNKNOWN"
        };

    private static string PreferenceTokens(string token, int count)
        => string.Join(" ", Enumerable.Repeat(token, count));

    private static ProjectWikiRequestRoutingDpoPair[] InterleaveByRejectedType(ProjectWikiRequestRoutingDpoPair[] pairs)
    {
        var buckets = pairs
            .GroupBy(pair => pair.RejectedType, StringComparer.Ordinal)
            .Select(group => new Queue<ProjectWikiRequestRoutingDpoPair>(group))
            .ToArray();
        var interleaved = new List<ProjectWikiRequestRoutingDpoPair>(pairs.Length);

        while (buckets.Any(bucket => bucket.Count > 0))
        {
            foreach (var bucket in buckets)
            {
                if (bucket.Count > 0)
                    interleaved.Add(bucket.Dequeue());
            }
        }

        return interleaved.ToArray();
    }
}
