// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Text.Json;
using AgentInfrastructure;
using ModelServer.Contracts;

public sealed class ProjectWikiQueryPlanningTrainingPipeline
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ProjectWikiQueryPlanningDpoGenerator _generator;
    private readonly ProjectWikiQueryPlanningTrainingRunner _runner;
    private readonly ProjectWikiQueryPlanningManifestPublisher _publisher;
    private readonly ProjectWikiDpoMetricRunner _dpoMetricRunner;

    public ProjectWikiQueryPlanningTrainingPipeline(
        ProjectWikiQueryPlanningDpoGenerator? generator = null,
        ProjectWikiQueryPlanningTrainingRunner? runner = null,
        ProjectWikiQueryPlanningManifestPublisher? publisher = null,
        ProjectWikiDpoMetricRunner? dpoMetricRunner = null)
    {
        _generator = generator ?? new ProjectWikiQueryPlanningDpoGenerator();
        _runner = runner ?? new ProjectWikiQueryPlanningTrainingRunner();
        _publisher = publisher ?? new ProjectWikiQueryPlanningManifestPublisher();
        _dpoMetricRunner = dpoMetricRunner ?? new ProjectWikiDpoMetricRunner();
    }

    public async Task<OperationResult<ProjectWikiQueryPlanningTrainingPipelineResult>> RunOneBatchAsync(
        string artifactRoot,
        string batchId = "project-wiki-query-planning-batch-001",
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<ProjectWikiQueryPlanningTrainingPipelineResult>.Fail("Pipeline cancelled.", "PIPELINE_CANCELLED");

        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<ProjectWikiQueryPlanningTrainingPipelineResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        var train = await _generator.BuildBatchAsync(batchId, heldOut: false, ct);
        if (!train.IsSuccess)
            return train.MapError<ProjectWikiQueryPlanningTrainingPipelineResult>();

        var heldOut = await _generator.BuildBatchAsync($"{batchId}-heldout", heldOut: true, ct);
        if (!heldOut.IsSuccess)
            return heldOut.MapError<ProjectWikiQueryPlanningTrainingPipelineResult>();

        var run = await _runner.TrainAndEvaluateAsync(train.Value!.Pairs, heldOut.Value!.Pairs, ct);
        if (!run.IsSuccess)
            return run.MapError<ProjectWikiQueryPlanningTrainingPipelineResult>();

        var caseDiagnostics = await WriteCaseDiagnosticsAsync(artifactRoot, batchId, run.Value!, ct);
        if (!caseDiagnostics.IsSuccess)
            return caseDiagnostics.MapError<ProjectWikiQueryPlanningTrainingPipelineResult>();

        var dpoMetrics = await _dpoMetricRunner.TrainAndEvaluateAsync(
            ProjectWikiQueryPlanningFlow.FlowId,
            batchId,
            train.Value.Pairs.Select(ToMetricPair).ToArray(),
            heldOut.Value!.Pairs.Select(ToMetricPair).ToArray(),
            artifactRoot,
            ct);
        if (!dpoMetrics.IsSuccess)
            return dpoMetrics.MapError<ProjectWikiQueryPlanningTrainingPipelineResult>();

        var publicationWarnings = await ValidatePublicationGateAsync(batchId, dpoMetrics.Value!, ct);

        if (!string.Equals(run.Value!.Verdict, "PROJECT_WIKI_QUERY_PLANNING_GATE_PASSED", StringComparison.Ordinal) ||
            dpoMetrics.Value!.HeldOutAccuracy < 0.75 ||
            publicationWarnings.Length > 0)
        {
            return OperationResult<ProjectWikiQueryPlanningTrainingPipelineResult>.Ok(new ProjectWikiQueryPlanningTrainingPipelineResult(
                batchId,
                train.Value,
                heldOut.Value!,
                run.Value,
                null,
                "Add 50-row remediation batch for ProjectWikiQueryPlanningFlow and rerun.",
                [$"query_planning_case_diagnostics={caseDiagnostics.Value}", $"query_plan_accuracy={run.Value.QueryPlanAccuracy:0.######}", $"required_touchpoint_recall={run.Value.RequiredTouchpointRecall:0.######}", $"multi_hop_sequence_accuracy={run.Value.MultiHopSequenceAccuracy:0.######}", $"missing_evidence_detection={run.Value.MissingEvidenceDetection:0.######}", "query_planning_gate_failed_publication_skipped", .. publicationWarnings],
                dpoMetrics.Value));
        }

        var published = await _publisher.PublishAsync(run.Value, artifactRoot, serverBaseUrl, ct);
        if (!published.IsSuccess)
            return published.MapError<ProjectWikiQueryPlanningTrainingPipelineResult>();

        return OperationResult<ProjectWikiQueryPlanningTrainingPipelineResult>.Ok(new ProjectWikiQueryPlanningTrainingPipelineResult(
            batchId,
            train.Value,
            heldOut.Value!,
            run.Value,
            published.Value!,
            "Source next 50 rows for the weakest measured wiki unit.",
            [$"query_planning_case_diagnostics={caseDiagnostics.Value}"],
            dpoMetrics.Value));
    }

    private static async Task<OperationResult<string>> WriteCaseDiagnosticsAsync(
        string artifactRoot,
        string batchId,
        ProjectWikiQueryPlanningTrainingRunResult run,
        CancellationToken ct)
    {
        try
        {
            var root = Path.Combine(artifactRoot, "query-planning-case-diagnostics");
            Directory.CreateDirectory(root);
            var path = Path.Combine(root, $"{batchId}-cases.jsonl");
            await using var writer = new StreamWriter(path, false);
            foreach (var item in run.Cases)
            {
                var json = JsonSerializer.Serialize(item, JsonOptions);
                await writer.WriteLineAsync(json.AsMemory(), ct);
            }

            return OperationResult<string>.Ok(path);
        }
        catch (Exception ex)
        {
            return OperationResult<string>.Fail($"Failed to write query-planning case diagnostics: {ex.Message}", "QUERY_PLANNING_CASE_DIAGNOSTICS_WRITE_FAILED");
        }
    }

    private static ProjectWikiDpoMetricPair ToMetricPair(ProjectWikiQueryPlanningDpoPair pair)
        => new(
            pair.PairId,
            "Task: build a complete project-local and current-version wiki query plan before answering. " +
            "The plan must include required evidence families, mapper/value-flow checks for entity changes, caller/callee impact, tests or validation surfaces, stale-version checks, wrong-project checks, and missing-evidence abstention.\n" +
            $"User question: {pair.A}",
            pair.Chosen,
            pair.Rejected);

    private static async Task<string[]> ValidatePublicationGateAsync(
        string batchId,
        ProjectWikiDpoMetricRunResult metrics,
        CancellationToken ct)
    {
        var warnings = new List<string>();
        if (!batchId.Contains("promotion-confirmation", StringComparison.OrdinalIgnoreCase))
            warnings.Add("query_planning_publication_requires_promotion_confirmation_batch");

        if (string.IsNullOrWhiteSpace(metrics.PolicyCheckpointPath) ||
            string.IsNullOrWhiteSpace(metrics.ReferenceCheckpointPath) ||
            !File.Exists(metrics.PolicyCheckpointPath) ||
            !File.Exists(metrics.ReferenceCheckpointPath))
            warnings.Add("query_planning_publication_requires_policy_and_reference_checkpoints");

        if (!File.Exists(metrics.PairDiagnosticsPath))
        {
            warnings.Add("query_planning_pair_diagnostics_missing");
            return warnings.ToArray();
        }

        var diagnostics = new List<ProjectWikiDpoMetricPairDiagnostic>();
        foreach (var line in await File.ReadAllLinesAsync(metrics.PairDiagnosticsPath, ct))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var diagnostic = JsonSerializer.Deserialize<ProjectWikiDpoMetricPairDiagnostic>(line, JsonOptions);
            if (diagnostic is not null)
                diagnostics.Add(diagnostic);
        }

        foreach (var group in diagnostics
            .Where(item => string.Equals(item.Split, "heldout", StringComparison.OrdinalIgnoreCase))
            .GroupBy(item => item.RejectedType, StringComparer.OrdinalIgnoreCase))
        {
            if (group.Average(item => item.Accuracy) < 0.75)
                warnings.Add($"query_planning_heldout_accuracy_below_gate_{group.Key}");

            if (group.Average(item => item.RewardMargin) < 0)
                warnings.Add($"query_planning_heldout_margin_negative_{group.Key}");
        }

        return warnings.ToArray();
    }
}
