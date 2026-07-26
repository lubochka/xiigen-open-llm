// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Text.Json;
using AgentInfrastructure;

public sealed class CrossStackBusinessFlowGraphTrainingPipeline
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    private readonly CrossStackBusinessFlowGraphDpoGenerator _generator;
    private readonly CrossStackBusinessFlowGraphTrainingRunner _runner;
    private readonly CrossStackBusinessFlowGraphManifestPublisher _publisher;
    private readonly ProjectWikiDpoMetricRunner _dpoMetricRunner;

    public CrossStackBusinessFlowGraphTrainingPipeline(
        CrossStackBusinessFlowGraphDpoGenerator? generator = null,
        CrossStackBusinessFlowGraphTrainingRunner? runner = null,
        CrossStackBusinessFlowGraphManifestPublisher? publisher = null,
        ProjectWikiDpoMetricRunner? dpoMetricRunner = null)
    {
        _generator = generator ?? new CrossStackBusinessFlowGraphDpoGenerator();
        _runner = runner ?? new CrossStackBusinessFlowGraphTrainingRunner();
        _publisher = publisher ?? new CrossStackBusinessFlowGraphManifestPublisher();
        _dpoMetricRunner = dpoMetricRunner ?? new ProjectWikiDpoMetricRunner(epochCount: 8, learningRate: 1e-3f);
    }

    public async Task<OperationResult<CrossStackBusinessFlowGraphTrainingPipelineResult>> RunOneBatchAsync(
        string artifactRoot,
        string batchId = "cross-stack-business-flow-graph-batch-001",
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<CrossStackBusinessFlowGraphTrainingPipelineResult>.Fail("Pipeline cancelled.", "PIPELINE_CANCELLED");
        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<CrossStackBusinessFlowGraphTrainingPipelineResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        var train = await _generator.BuildBatchAsync(batchId, heldOut: false, ct);
        if (!train.IsSuccess)
            return train.MapError<CrossStackBusinessFlowGraphTrainingPipelineResult>();
        var trainBatch = train.Value!;

        var heldOut = await _generator.BuildBatchAsync($"{batchId}-heldout", heldOut: true, ct);
        if (!heldOut.IsSuccess)
            return heldOut.MapError<CrossStackBusinessFlowGraphTrainingPipelineResult>();
        var heldOutBatch = heldOut.Value!;

        var persisted = await WriteBatchArtifactsAsync(artifactRoot, batchId, trainBatch, heldOutBatch, ct);
        if (!persisted.IsSuccess)
            return persisted.MapError<CrossStackBusinessFlowGraphTrainingPipelineResult>();

        var dpoMetrics = await _dpoMetricRunner.TrainAndEvaluateAsync(
            CrossStackBusinessFlowGraphUnit.UnitId,
            batchId,
            ToMetricRows(trainBatch.Pairs),
            ToMetricRows(heldOutBatch.Pairs),
            artifactRoot,
            ct);
        if (!dpoMetrics.IsSuccess)
            return dpoMetrics.MapError<CrossStackBusinessFlowGraphTrainingPipelineResult>();
        var dpoRun = dpoMetrics.Value!;

        var run = await _runner.TrainAndEvaluateAsync(trainBatch.Pairs, heldOutBatch.Pairs, ct);
        if (!run.IsSuccess)
            return run.MapError<CrossStackBusinessFlowGraphTrainingPipelineResult>();

        var canPublish = string.Equals(run.Value!.Verdict, "CROSS_STACK_BUSINESS_FLOW_GRAPH_GATE_PASSED", StringComparison.Ordinal) &&
                         dpoRun.FinalAccuracy >= 0.75 &&
                         dpoRun.FinalRewardMargin > 0 &&
                         dpoRun.HeldOutAccuracy >= 0.75;
        if (!canPublish)
        {
            return OperationResult<CrossStackBusinessFlowGraphTrainingPipelineResult>.Ok(new CrossStackBusinessFlowGraphTrainingPipelineResult(
                batchId,
                trainBatch,
                heldOutBatch,
                run.Value,
                null,
                "Add 50-row remediation batch for CrossStackBusinessFlowGraphUnit and rerun.",
                ["cross_stack_business_flow_graph_publication_skipped"],
                dpoRun));
        }

        var published = await _publisher.PublishAsync(run.Value, dpoRun, artifactRoot, serverBaseUrl, ct);
        if (!published.IsSuccess)
            return published.MapError<CrossStackBusinessFlowGraphTrainingPipelineResult>();

        return OperationResult<CrossStackBusinessFlowGraphTrainingPipelineResult>.Ok(new CrossStackBusinessFlowGraphTrainingPipelineResult(
            batchId,
            trainBatch,
            heldOutBatch,
            run.Value,
            published.Value!,
            "Rerun readiness; then source the next weakest wiki unit.",
            [],
            dpoRun));
    }

    private static async Task<OperationResult<bool>> WriteBatchArtifactsAsync(
        string artifactRoot,
        string batchId,
        CrossStackBusinessFlowGraphDpoBatch trainBatch,
        CrossStackBusinessFlowGraphDpoBatch heldOutBatch,
        CancellationToken ct)
    {
        try
        {
            var runRoot = Path.Combine(artifactRoot, "dpo-runs", CrossStackBusinessFlowGraphUnit.UnitId, batchId);
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
            return OperationResult<bool>.Fail($"Failed to persist cross-stack graph DPO rows: {ex.Message}", "CROSS_STACK_GRAPH_DPO_ROW_PERSIST_FAILED");
        }
    }

    private static ProjectWikiDpoMetricPair[] ToMetricRows(IEnumerable<CrossStackBusinessFlowGraphDpoPair> pairs)
        => pairs.Select(pair => new ProjectWikiDpoMetricPair(
            pair.PairId,
            $"{pair.A}\nTask: choose the answer that preserves the complete Angular to .NET to SQL stored procedure business flow graph.",
            $"ACCEPTED_COMPLETE_GRAPH PASS_TRUE COMPLETE_CROSS_STACK_PATH COMPLETE_CROSS_STACK_PATH COMPLETE_CROSS_STACK_PATH required_paths: {string.Join("; ", pair.RequiredPathIds)} evidence: {string.Join("|", pair.EvidenceIds)}",
            $"REJECTED_INCOMPLETE_GRAPH FAIL_FALSE MISSING_CROSS_STACK_PATH MISSING_CROSS_STACK_PATH MISSING_CROSS_STACK_PATH [WRONG: {pair.RejectedType} - required Angular to .NET to SQL stored procedure path or evidence is missing.]")).ToArray();

    private static async Task WriteJsonlAsync(
        string path,
        IEnumerable<CrossStackBusinessFlowGraphDpoPair> pairs,
        CancellationToken ct)
    {
        await using var stream = File.Create(path);
        await using var writer = new StreamWriter(stream);
        foreach (var pair in pairs)
            await writer.WriteLineAsync(JsonSerializer.Serialize(pair, JsonOptions).AsMemory(), ct);
    }
}
