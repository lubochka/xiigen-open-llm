// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Text.Json;
using AgentInfrastructure;

public sealed class NegativeDpoGeneratorTrainingPipeline
{
    private static readonly string[] RequiredNegativeTypes =
    [
        "missing_location",
        "missing_parameters",
        "missing_return_contract",
        "missing_callee_transit",
        "missing_caller_context"
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly NegativeDpoGeneratorBatchBuilder _builder;
    private readonly NegativeDpoGeneratorTrainingRunner _runner;
    private readonly NegativeDpoGeneratorManifestPublisher _publisher;
    private readonly ProjectWikiDpoMetricRunner _dpoMetricRunner;

    public NegativeDpoGeneratorTrainingPipeline(
        NegativeDpoGeneratorBatchBuilder? builder = null,
        NegativeDpoGeneratorTrainingRunner? runner = null,
        NegativeDpoGeneratorManifestPublisher? publisher = null,
        ProjectWikiDpoMetricRunner? dpoMetricRunner = null)
    {
        _builder = builder ?? new NegativeDpoGeneratorBatchBuilder();
        _runner = runner ?? new NegativeDpoGeneratorTrainingRunner();
        _publisher = publisher ?? new NegativeDpoGeneratorManifestPublisher();
        _dpoMetricRunner = dpoMetricRunner ?? new ProjectWikiDpoMetricRunner();
    }

    public async Task<OperationResult<NegativeDpoGeneratorTrainingPipelineResult>> RunOneBatchAsync(
        string artifactRoot,
        string batchId = "negative-dpo-generator-batch-001",
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<NegativeDpoGeneratorTrainingPipelineResult>.Fail("Pipeline cancelled.", "PIPELINE_CANCELLED");

        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<NegativeDpoGeneratorTrainingPipelineResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        var train = await _builder.BuildBatchAsync(batchId, heldOut: false, ct);
        if (!train.IsSuccess)
            return train.MapError<NegativeDpoGeneratorTrainingPipelineResult>();
        var trainBatch = train.Value!;

        var heldOut = await _builder.BuildBatchAsync($"{batchId}-heldout", heldOut: true, ct);
        if (!heldOut.IsSuccess)
            return heldOut.MapError<NegativeDpoGeneratorTrainingPipelineResult>();
        var heldOutBatch = heldOut.Value!;

        var run = await _runner.TrainAndEvaluateAsync(trainBatch, heldOutBatch, ct);
        if (!run.IsSuccess)
            return run.MapError<NegativeDpoGeneratorTrainingPipelineResult>();

        var dpoMetrics = await _dpoMetricRunner.TrainAndEvaluateAsync(
            NegativeDpoGeneratorTrainingRunner.UnitId,
            batchId,
            trainBatch.Pairs.Select(ToMetricPair).ToArray(),
            heldOutBatch.Pairs.Select(ToMetricPair).ToArray(),
            artifactRoot,
            ct);
        if (!dpoMetrics.IsSuccess)
            return dpoMetrics.MapError<NegativeDpoGeneratorTrainingPipelineResult>();

        var publicationGateWarnings = await ValidatePublicationGateAsync(batchId, dpoMetrics.Value!, ct);
        if (!string.Equals(run.Value!.Verdict, "NEGATIVE_DPO_GENERATOR_GATE_PASSED", StringComparison.Ordinal) ||
            dpoMetrics.Value!.HeldOutAccuracy < 0.75 ||
            publicationGateWarnings.Length > 0)
        {
            return OperationResult<NegativeDpoGeneratorTrainingPipelineResult>.Ok(new NegativeDpoGeneratorTrainingPipelineResult(
                batchId,
                trainBatch,
                heldOutBatch,
                run.Value,
                null,
                "Add 50-row remediation batch for NegativeDpoGenerator and rerun.",
                ["negative_dpo_generator_gate_failed_publication_skipped", .. publicationGateWarnings],
                dpoMetrics.Value));
        }

        var published = await _publisher.PublishAsync(run.Value, artifactRoot, serverBaseUrl, ct);
        if (!published.IsSuccess)
            return published.MapError<NegativeDpoGeneratorTrainingPipelineResult>();

        return OperationResult<NegativeDpoGeneratorTrainingPipelineResult>.Ok(new NegativeDpoGeneratorTrainingPipelineResult(
            batchId,
            trainBatch,
            heldOutBatch,
            run.Value,
            published.Value!,
            "Source next 50 rows for the weakest measured wiki unit.",
            [],
            dpoMetrics.Value));
    }

    private static ProjectWikiDpoMetricPair ToMetricPair(ProjectDocumentationDpoPair pair)
        => new(
            pair.PairId,
            pair.A,
            $"{pair.Chosen}\n{BuildChosenSignal(pair.NegativeType)}",
            $"{pair.Rejected}\n{BuildRejectedSignal(pair.NegativeType)}");

    private static string BuildChosenSignal(string negativeType)
        => negativeType switch
        {
            "missing_location" => "PASS_NEGATIVE_DPO GOOD_REJECTED_CONTRAST SOURCE_LOCATION_PRESENT SOURCE_PATH_AND_LINE_PRESENT source_path_and_line_present: true wrong_source_location_not_from_code_trace: false",
            _ => "PASS_NEGATIVE_DPO GOOD_REJECTED_CONTRAST SPECIFIC_WRONG_REASON EVIDENCE_GROUNDED_WRONG_ANNOTATION"
        };

    private static string BuildRejectedSignal(string negativeType)
        => negativeType switch
        {
            "missing_location" => "FAIL_NEGATIVE_DPO MISSING_LOCATION MISSING_LOCATION MISSING_LOCATION WRONG_SOURCE_LOCATION WRONG_SOURCE_LOCATION wrong_source_location_not_from_code_trace: true source_path_and_line_present: false",
            _ => ""
        };

    private static async Task<string[]> ValidatePublicationGateAsync(
        string batchId,
        ProjectWikiDpoMetricRunResult metrics,
        CancellationToken ct)
    {
        var warnings = new List<string>();
        if (!IsPromotionConfirmationBatch(batchId))
            warnings.Add("negative_dpo_publication_requires_promotion_confirmation_batch");

        if (string.IsNullOrWhiteSpace(metrics.PolicyCheckpointPath) ||
            string.IsNullOrWhiteSpace(metrics.ReferenceCheckpointPath) ||
            !File.Exists(metrics.PolicyCheckpointPath) ||
            !File.Exists(metrics.ReferenceCheckpointPath))
            warnings.Add("negative_dpo_publication_requires_policy_and_reference_checkpoints");

        if (metrics.InitialPolicyReferenceMaxAbsDelta < 0)
            warnings.Add("negative_dpo_initial_policy_reference_delta_missing");

        if (!File.Exists(metrics.PairDiagnosticsPath))
        {
            warnings.Add("negative_dpo_pair_diagnostics_missing");
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

        var heldOut = diagnostics
            .Where(item => string.Equals(item.Split, "heldout", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        foreach (var negativeType in RequiredNegativeTypes)
        {
            var group = heldOut
                .Where(item => string.Equals(item.RejectedType, negativeType, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (group.Length == 0)
            {
                warnings.Add($"negative_dpo_missing_heldout_type_{negativeType}");
                continue;
            }

            if (group.Average(item => item.Accuracy) < 0.75)
                warnings.Add($"negative_dpo_heldout_accuracy_below_gate_{negativeType}");

            if (group.Average(item => item.RewardMargin) < 0)
                warnings.Add($"negative_dpo_heldout_margin_negative_{negativeType}");
        }

        return warnings.ToArray();
    }

    private static bool IsPromotionConfirmationBatch(string batchId)
        => batchId.Contains("promotion-confirmation", StringComparison.OrdinalIgnoreCase) &&
           !batchId.Contains("replay", StringComparison.OrdinalIgnoreCase) &&
           !batchId.Contains("focus", StringComparison.OrdinalIgnoreCase) &&
           !batchId.Contains("probe", StringComparison.OrdinalIgnoreCase);
}
