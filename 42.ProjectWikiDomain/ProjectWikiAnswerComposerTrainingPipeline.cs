// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;

public sealed class ProjectWikiAnswerComposerTrainingPipeline
{
    private const double MinimumDpoHeldOutAccuracy = 0.75;

    private readonly ProjectWikiAnswerComposerDpoGenerator _generator;
    private readonly ProjectWikiAnswerComposerTrainingRunner _runner;
    private readonly ProjectWikiAnswerComposerManifestPublisher _publisher;
    private readonly ProjectWikiDpoMetricRunner _dpoMetricRunner;

    public ProjectWikiAnswerComposerTrainingPipeline(
        ProjectWikiAnswerComposerDpoGenerator? generator = null,
        ProjectWikiAnswerComposerTrainingRunner? runner = null,
        ProjectWikiAnswerComposerManifestPublisher? publisher = null,
        ProjectWikiDpoMetricRunner? dpoMetricRunner = null)
    {
        _generator = generator ?? new ProjectWikiAnswerComposerDpoGenerator();
        _runner = runner ?? new ProjectWikiAnswerComposerTrainingRunner();
        _publisher = publisher ?? new ProjectWikiAnswerComposerManifestPublisher();
        _dpoMetricRunner = dpoMetricRunner ?? new ProjectWikiDpoMetricRunner();
    }

    public async Task<OperationResult<ProjectWikiAnswerComposerTrainingPipelineResult>> RunOneBatchAsync(
        string artifactRoot,
        string batchId = "project-wiki-answer-composer-batch-001",
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<ProjectWikiAnswerComposerTrainingPipelineResult>.Fail("Pipeline cancelled.", "PIPELINE_CANCELLED");

        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<ProjectWikiAnswerComposerTrainingPipelineResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        var train = await _generator.BuildBatchAsync(batchId, heldOut: false, ct);
        if (!train.IsSuccess)
            return train.MapError<ProjectWikiAnswerComposerTrainingPipelineResult>();

        var heldOut = await _generator.BuildBatchAsync($"{batchId}-heldout", heldOut: true, ct);
        if (!heldOut.IsSuccess)
            return heldOut.MapError<ProjectWikiAnswerComposerTrainingPipelineResult>();

        var run = await _runner.TrainAndEvaluateAsync(train.Value!.Pairs, heldOut.Value!.Pairs, ct);
        if (!run.IsSuccess)
            return run.MapError<ProjectWikiAnswerComposerTrainingPipelineResult>();

        var dpoMetrics = await _dpoMetricRunner.TrainAndEvaluateAsync(
            ProjectWikiAnswerComposerTrainingRunner.UnitId,
            batchId,
            train.Value!.Pairs.Select(ToDpoMetricPair).ToArray(),
            heldOut.Value!.Pairs.Select(ToDpoMetricPair).ToArray(),
            artifactRoot,
            ct);
        if (!dpoMetrics.IsSuccess)
            return dpoMetrics.MapError<ProjectWikiAnswerComposerTrainingPipelineResult>();

        if (!string.Equals(run.Value!.Verdict, "PROJECT_WIKI_ANSWER_COMPOSER_GATE_PASSED", StringComparison.Ordinal))
        {
            return OperationResult<ProjectWikiAnswerComposerTrainingPipelineResult>.Ok(new ProjectWikiAnswerComposerTrainingPipelineResult(
                batchId,
                train.Value,
                heldOut.Value!,
                run.Value,
                null,
                "Add 50-row remediation batch for ProjectWikiAnswerComposer and rerun.",
                ["answer_composer_gate_failed_publication_skipped"],
                dpoMetrics.Value));
        }

        if (dpoMetrics.Value!.HeldOutAccuracy < MinimumDpoHeldOutAccuracy)
        {
            return OperationResult<ProjectWikiAnswerComposerTrainingPipelineResult>.Ok(new ProjectWikiAnswerComposerTrainingPipelineResult(
                batchId,
                train.Value,
                heldOut.Value!,
                run.Value,
                null,
                "Add 50-row ProjectWikiAnswerComposer negative-DPO remediation batch and rerun.",
                ["answer_composer_dpo_accuracy_below_0_75_publication_skipped"],
                dpoMetrics.Value));
        }

        var published = await _publisher.PublishAsync(run.Value, artifactRoot, serverBaseUrl, ct);
        if (!published.IsSuccess)
            return published.MapError<ProjectWikiAnswerComposerTrainingPipelineResult>();

        return OperationResult<ProjectWikiAnswerComposerTrainingPipelineResult>.Ok(new ProjectWikiAnswerComposerTrainingPipelineResult(
            batchId,
            train.Value,
            heldOut.Value!,
            run.Value,
            published.Value!,
            "Source next 50 rows for the weakest measured wiki unit.",
            [],
            dpoMetrics.Value!));
    }

    private static ProjectWikiDpoMetricPair ToDpoMetricPair(ProjectWikiAnswerComposerDpoPair pair)
        => new(
            pair.PairId,
            "Task: compose a project-local wiki answer. The chosen answer must stay inside the requested project, cite accepted evidence ids, include the requested code facts, and abstain when evidence is missing.\nQuestion: " + pair.A,
            pair.Chosen,
            pair.Rejected);
}
