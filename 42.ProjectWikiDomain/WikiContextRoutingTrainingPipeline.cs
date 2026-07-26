// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;

public sealed class WikiContextRoutingTrainingPipeline
{
    private const double MinimumDpoHeldOutAccuracy = 0.75;

    private readonly WikiContextRoutingDpoGenerator _generator;
    private readonly WikiContextRoutingTrainingRunner _runner;
    private readonly WikiContextRoutingManifestPublisher _publisher;
    private readonly ProjectWikiDpoMetricRunner _dpoMetricRunner;

    public WikiContextRoutingTrainingPipeline(
        WikiContextRoutingDpoGenerator? generator = null,
        WikiContextRoutingTrainingRunner? runner = null,
        WikiContextRoutingManifestPublisher? publisher = null,
        ProjectWikiDpoMetricRunner? dpoMetricRunner = null)
    {
        _generator = generator ?? new WikiContextRoutingDpoGenerator();
        _runner = runner ?? new WikiContextRoutingTrainingRunner();
        _publisher = publisher ?? new WikiContextRoutingManifestPublisher();
        _dpoMetricRunner = dpoMetricRunner ?? new ProjectWikiDpoMetricRunner();
    }

    public async Task<OperationResult<ProjectWikiContextRoutingTrainingPipelineResult>> RunOneBatchAsync(
        string artifactRoot,
        string batchId = "wiki-context-routing-batch-001",
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<ProjectWikiContextRoutingTrainingPipelineResult>.Fail("Pipeline cancelled.", "PIPELINE_CANCELLED");

        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<ProjectWikiContextRoutingTrainingPipelineResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        var train = await _generator.BuildBatchAsync(batchId, heldOut: false, ct);
        if (!train.IsSuccess)
            return train.MapError<ProjectWikiContextRoutingTrainingPipelineResult>();

        var heldOut = await _generator.BuildBatchAsync($"{batchId}-heldout", heldOut: true, ct);
        if (!heldOut.IsSuccess)
            return heldOut.MapError<ProjectWikiContextRoutingTrainingPipelineResult>();

        var run = await _runner.TrainAndEvaluateAsync(train.Value!.Pairs, heldOut.Value!.Pairs, ct);
        if (!run.IsSuccess)
            return run.MapError<ProjectWikiContextRoutingTrainingPipelineResult>();

        if (!string.Equals(run.Value!.Verdict, "WIKI_CONTEXT_ROUTING_GATE_PASSED", StringComparison.Ordinal))
        {
            return OperationResult<ProjectWikiContextRoutingTrainingPipelineResult>.Ok(new ProjectWikiContextRoutingTrainingPipelineResult(
                batchId,
                train.Value,
                heldOut.Value!,
                run.Value,
                null,
                "Add 50-row remediation batch for WikiContextRoutingUnit and rerun.",
                ["context_routing_gate_failed_publication_skipped"]));
        }

        var dpoMetrics = await _dpoMetricRunner.TrainAndEvaluateAsync(
            WikiContextRoutingUnit.UnitId,
            batchId,
            train.Value.Pairs.Select(ToDpoMetricPair).ToArray(),
            heldOut.Value!.Pairs.Select(ToDpoMetricPair).ToArray(),
            artifactRoot,
            ct);
        if (!dpoMetrics.IsSuccess)
            return dpoMetrics.MapError<ProjectWikiContextRoutingTrainingPipelineResult>();
        if (dpoMetrics.Value!.HeldOutAccuracy < MinimumDpoHeldOutAccuracy)
        {
            return OperationResult<ProjectWikiContextRoutingTrainingPipelineResult>.Ok(new ProjectWikiContextRoutingTrainingPipelineResult(
                batchId,
                train.Value,
                heldOut.Value!,
                run.Value,
                null,
                "Add 50-row WikiContextRoutingUnit negative-DPO remediation batch and rerun.",
                ["context_routing_dpo_accuracy_below_0_75_publication_skipped"],
                dpoMetrics.Value));
        }

        var published = await _publisher.PublishAsync(run.Value, artifactRoot, serverBaseUrl, ct);
        if (!published.IsSuccess)
            return published.MapError<ProjectWikiContextRoutingTrainingPipelineResult>();

        return OperationResult<ProjectWikiContextRoutingTrainingPipelineResult>.Ok(new ProjectWikiContextRoutingTrainingPipelineResult(
            batchId,
            train.Value,
            heldOut.Value!,
            run.Value,
            published.Value!,
            "Source next 50 rows for the weakest measured wiki unit.",
            [],
            dpoMetrics.Value!));
    }

    private static ProjectWikiDpoMetricPair ToDpoMetricPair(ProjectWikiContextRoutingDpoPair pair)
        => new(pair.PairId, pair.A, pair.Chosen, pair.Rejected);
}
