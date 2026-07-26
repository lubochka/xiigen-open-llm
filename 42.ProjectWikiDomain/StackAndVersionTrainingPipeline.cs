// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;

public sealed class StackAndVersionTrainingPipeline
{
    private readonly StackAndVersionDpoGenerator _generator;
    private readonly StackAndVersionTrainingRunner _runner;
    private readonly StackAndVersionManifestPublisher _publisher;

    public StackAndVersionTrainingPipeline(
        StackAndVersionDpoGenerator? generator = null,
        StackAndVersionTrainingRunner? runner = null,
        StackAndVersionManifestPublisher? publisher = null)
    {
        _generator = generator ?? new StackAndVersionDpoGenerator();
        _runner = runner ?? new StackAndVersionTrainingRunner();
        _publisher = publisher ?? new StackAndVersionManifestPublisher();
    }

    public async Task<OperationResult<StackAndVersionTrainingPipelineResult>> RunOneBatchAsync(
        string artifactRoot,
        string batchId = "stack-version-classifier-batch-001",
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<StackAndVersionTrainingPipelineResult>.Fail("Pipeline cancelled.", "PIPELINE_CANCELLED");

        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<StackAndVersionTrainingPipelineResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        var train = await _generator.BuildBatchAsync(batchId, heldOut: false, ct);
        if (!train.IsSuccess)
            return train.MapError<StackAndVersionTrainingPipelineResult>();
        var trainBatch = train.Value!;

        var heldOut = await _generator.BuildBatchAsync($"{batchId}-heldout", heldOut: true, ct);
        if (!heldOut.IsSuccess)
            return heldOut.MapError<StackAndVersionTrainingPipelineResult>();
        var heldOutBatch = heldOut.Value!;

        var run = await _runner.TrainAndEvaluateAsync(trainBatch.Pairs, heldOutBatch.Pairs, ct);
        if (!run.IsSuccess)
            return run.MapError<StackAndVersionTrainingPipelineResult>();

        if (!string.Equals(run.Value!.Verdict, "STACK_VERSION_CLASSIFIER_GATE_PASSED", StringComparison.Ordinal))
        {
            return OperationResult<StackAndVersionTrainingPipelineResult>.Ok(new StackAndVersionTrainingPipelineResult(
                batchId,
                trainBatch,
                heldOutBatch,
                run.Value,
                null,
                "Add 50-row remediation batch for StackAndVersionClassifier and rerun.",
                ["stack_version_gate_failed_publication_skipped"]));
        }

        var published = await _publisher.PublishAsync(run.Value, artifactRoot, serverBaseUrl, ct);
        if (!published.IsSuccess)
            return published.MapError<StackAndVersionTrainingPipelineResult>();

        return OperationResult<StackAndVersionTrainingPipelineResult>.Ok(new StackAndVersionTrainingPipelineResult(
            batchId,
            trainBatch,
            heldOutBatch,
            run.Value,
            published.Value!,
            "Source next 50 rows for the weakest measured wiki unit.",
            []));
    }
}
