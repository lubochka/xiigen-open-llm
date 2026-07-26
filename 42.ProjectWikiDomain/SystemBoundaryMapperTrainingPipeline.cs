// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Text.Json;
using AgentInfrastructure;

public sealed class SystemBoundaryMapperTrainingPipeline
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    private readonly SystemBoundaryMapperDpoGenerator _generator;
    private readonly SystemBoundaryMapperTrainingRunner _runner;
    private readonly SystemBoundaryMapperManifestPublisher _publisher;

    public SystemBoundaryMapperTrainingPipeline(
        SystemBoundaryMapperDpoGenerator? generator = null,
        SystemBoundaryMapperTrainingRunner? runner = null,
        SystemBoundaryMapperManifestPublisher? publisher = null)
    {
        _generator = generator ?? new SystemBoundaryMapperDpoGenerator();
        _runner = runner ?? new SystemBoundaryMapperTrainingRunner();
        _publisher = publisher ?? new SystemBoundaryMapperManifestPublisher();
    }

    public async Task<OperationResult<SystemBoundaryMapperTrainingPipelineResult>> RunOneBatchAsync(
        string artifactRoot,
        string batchId = "system-boundary-mapper-batch-001",
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<SystemBoundaryMapperTrainingPipelineResult>.Fail("Pipeline cancelled.", "PIPELINE_CANCELLED");

        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<SystemBoundaryMapperTrainingPipelineResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        var train = await _generator.BuildBatchAsync(batchId, heldOut: false, ct);
        if (!train.IsSuccess)
            return train.MapError<SystemBoundaryMapperTrainingPipelineResult>();
        var trainBatch = train.Value!;

        var heldOut = await _generator.BuildBatchAsync($"{batchId}-heldout", heldOut: true, ct);
        if (!heldOut.IsSuccess)
            return heldOut.MapError<SystemBoundaryMapperTrainingPipelineResult>();
        var heldOutBatch = heldOut.Value!;

        var persisted = await WriteBatchArtifactsAsync(artifactRoot, batchId, trainBatch, heldOutBatch, ct);
        if (!persisted.IsSuccess)
            return persisted.MapError<SystemBoundaryMapperTrainingPipelineResult>();

        var run = await _runner.TrainAndEvaluateAsync(trainBatch.Pairs, heldOutBatch.Pairs, ct);
        if (!run.IsSuccess)
            return run.MapError<SystemBoundaryMapperTrainingPipelineResult>();

        if (!string.Equals(run.Value!.Verdict, "SYSTEM_BOUNDARY_MAPPER_GATE_PASSED", StringComparison.Ordinal))
        {
            return OperationResult<SystemBoundaryMapperTrainingPipelineResult>.Ok(new SystemBoundaryMapperTrainingPipelineResult(
                batchId,
                trainBatch,
                heldOutBatch,
                run.Value,
                null,
                "Add 50-row remediation batch for SystemBoundaryMapperUnit and rerun.",
                ["system_boundary_gate_failed_publication_skipped"]));
        }

        var published = await _publisher.PublishAsync(run.Value, artifactRoot, serverBaseUrl, ct);
        if (!published.IsSuccess)
            return published.MapError<SystemBoundaryMapperTrainingPipelineResult>();

        return OperationResult<SystemBoundaryMapperTrainingPipelineResult>.Ok(new SystemBoundaryMapperTrainingPipelineResult(
            batchId,
            trainBatch,
            heldOutBatch,
            run.Value,
            published.Value!,
            "Rerun readiness; then source the next weakest wiki unit.",
            []));
    }

    private static async Task<OperationResult<bool>> WriteBatchArtifactsAsync(
        string artifactRoot,
        string batchId,
        SystemBoundaryMapperDpoBatch trainBatch,
        SystemBoundaryMapperDpoBatch heldOutBatch,
        CancellationToken ct)
    {
        try
        {
            var runRoot = Path.Combine(artifactRoot, "dpo-runs", SystemBoundaryMapperUnit.UnitId, batchId);
            Directory.CreateDirectory(runRoot);
            await WriteJsonlAsync(Path.Combine(runRoot, "dpo-pairs.jsonl"), trainBatch.Pairs, ct);
            await WriteJsonlAsync(Path.Combine(runRoot, "heldout-dpo-pairs.jsonl"), heldOutBatch.Pairs, ct);
            await File.WriteAllTextAsync(Path.Combine(runRoot, "batch-summary.json"), JsonSerializer.Serialize(new
            {
                train_batch_id = trainBatch.BatchId,
                heldout_batch_id = heldOutBatch.BatchId,
                train_rows = trainBatch.RowCount,
                heldout_rows = heldOutBatch.RowCount,
                train_wrong_annotation_rate = trainBatch.WrongAnnotationRate,
                heldout_wrong_annotation_rate = heldOutBatch.WrongAnnotationRate
            }, JsonOptions), ct);
            return OperationResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            return OperationResult<bool>.Fail($"Failed to persist system-boundary DPO rows: {ex.Message}", "SYSTEM_BOUNDARY_DPO_ROW_PERSIST_FAILED");
        }
    }

    private static async Task WriteJsonlAsync(
        string path,
        IEnumerable<SystemBoundaryMapperDpoPair> pairs,
        CancellationToken ct)
    {
        await using var stream = File.Create(path);
        await using var writer = new StreamWriter(stream);
        foreach (var pair in pairs)
            await writer.WriteLineAsync(JsonSerializer.Serialize(pair, JsonOptions).AsMemory(), ct);
    }
}
