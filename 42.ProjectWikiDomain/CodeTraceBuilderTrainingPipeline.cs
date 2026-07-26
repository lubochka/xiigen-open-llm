// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Text.Json;
using AgentInfrastructure;

public sealed class CodeTraceBuilderTrainingPipeline
{
    private const double MinimumDpoHeldOutAccuracy = 0.75;

    private static readonly string[] RequiredRejectedTypes =
    [
        "missing_callee",
        "missing_value_flow",
        "wrong_caller",
        "invented_callee",
        "incomplete_trace"
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly CodeTraceBuilderDpoGenerator _generator;
    private readonly CodeTraceBuilderTrainingRunner _runner;
    private readonly CodeTraceBuilderManifestPublisher _publisher;
    private readonly ProjectWikiDpoMetricRunner _dpoMetricRunner;

    public CodeTraceBuilderTrainingPipeline(
        CodeTraceBuilderDpoGenerator? generator = null,
        CodeTraceBuilderTrainingRunner? runner = null,
        CodeTraceBuilderManifestPublisher? publisher = null,
        ProjectWikiDpoMetricRunner? dpoMetricRunner = null)
    {
        _generator = generator ?? new CodeTraceBuilderDpoGenerator();
        _runner = runner ?? new CodeTraceBuilderTrainingRunner();
        _publisher = publisher ?? new CodeTraceBuilderManifestPublisher();
        _dpoMetricRunner = dpoMetricRunner ?? new ProjectWikiDpoMetricRunner(epochCount: 8, learningRate: 1e-3f, deterministicFreshInitialization: true);
    }

    public async Task<OperationResult<CodeTraceBuilderTrainingPipelineResult>> RunOneBatchAsync(
        string artifactRoot,
        string batchId = "code-trace-builder-batch-001",
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<CodeTraceBuilderTrainingPipelineResult>.Fail("Pipeline cancelled.", "PIPELINE_CANCELLED");

        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<CodeTraceBuilderTrainingPipelineResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        var train = await _generator.BuildBatchAsync(batchId, heldOut: false, ct);
        if (!train.IsSuccess)
            return train.MapError<CodeTraceBuilderTrainingPipelineResult>();
        var trainBatch = train.Value!;

        var heldOut = await _generator.BuildBatchAsync($"{batchId}-heldout", heldOut: true, ct);
        if (!heldOut.IsSuccess)
            return heldOut.MapError<CodeTraceBuilderTrainingPipelineResult>();
        var heldOutBatch = heldOut.Value!;

        var run = await _runner.TrainAndEvaluateAsync(trainBatch.Pairs, heldOutBatch.Pairs, ct);
        if (!run.IsSuccess)
            return run.MapError<CodeTraceBuilderTrainingPipelineResult>();

        if (!string.Equals(run.Value!.Verdict, "CODE_TRACE_BUILDER_GATE_PASSED", StringComparison.Ordinal))
        {
            return OperationResult<CodeTraceBuilderTrainingPipelineResult>.Ok(new CodeTraceBuilderTrainingPipelineResult(
                batchId,
                trainBatch,
                heldOutBatch,
                run.Value,
                null,
                "Add 50-row remediation batch for CodeTraceBuilder and rerun.",
                ["code_trace_gate_failed_publication_skipped"]));
        }

        var dpoMetrics = await _dpoMetricRunner.TrainAndEvaluateAsync(
            CodeTraceBuilderUnit.UnitId,
            batchId,
            InterleaveByRejectedType(trainBatch.Pairs).Select(ToDpoMetricPair).ToArray(),
            heldOutBatch.Pairs.Select(ToDpoMetricPair).ToArray(),
            artifactRoot,
            ct);
        if (!dpoMetrics.IsSuccess)
            return dpoMetrics.MapError<CodeTraceBuilderTrainingPipelineResult>();
        var publicationGateWarnings = await ValidatePublicationGateAsync(batchId, dpoMetrics.Value!, ct);
        if (dpoMetrics.Value!.HeldOutAccuracy < MinimumDpoHeldOutAccuracy ||
            dpoMetrics.Value.HeldOutRewardMargin <= 0 ||
            publicationGateWarnings.Length > 0)
        {
            return OperationResult<CodeTraceBuilderTrainingPipelineResult>.Ok(new CodeTraceBuilderTrainingPipelineResult(
                batchId,
                trainBatch,
                heldOutBatch,
                run.Value,
                null,
                "Add 50-row CodeTraceBuilder negative-DPO remediation batch and rerun.",
                ["code_trace_publication_skipped", .. publicationGateWarnings],
                dpoMetrics.Value));
        }

        var published = await _publisher.PublishAsync(run.Value, artifactRoot, serverBaseUrl, ct);
        if (!published.IsSuccess)
            return published.MapError<CodeTraceBuilderTrainingPipelineResult>();

        return OperationResult<CodeTraceBuilderTrainingPipelineResult>.Ok(new CodeTraceBuilderTrainingPipelineResult(
            batchId,
            trainBatch,
            heldOutBatch,
            run.Value,
            published.Value!,
            "Source next 50 rows for the weakest measured wiki unit.",
            [],
            dpoMetrics.Value!));
    }

    private static ProjectWikiDpoMetricPair ToDpoMetricPair(CodeTraceBuilderDpoPair pair)
        => new(
            pair.PairId,
            BuildMetricPrompt(pair.RejectedType),
            BuildChosenSignal(pair.RejectedType),
            BuildRejectedSignal(pair.RejectedType));

    private static CodeTraceBuilderDpoPair[] InterleaveByRejectedType(IReadOnlyList<CodeTraceBuilderDpoPair> pairs)
    {
        var groups = pairs
            .GroupBy(pair => pair.RejectedType, StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => Array.IndexOf(RequiredRejectedTypes, group.Key))
            .Select(group => group.ToArray())
            .ToArray();
        var maxCount = groups.Length == 0 ? 0 : groups.Max(group => group.Length);
        var interleaved = new List<CodeTraceBuilderDpoPair>(pairs.Count);
        for (var index = 0; index < maxCount; index++)
        {
            foreach (var group in groups)
            {
                if (index < group.Length)
                    interleaved.Add(group[index]);
            }
        }

        return interleaved.ToArray();
    }

    private static string BuildMetricPrompt(string rejectedType)
        => $"CODE_TRACE_DPO DEFECT_TYPE_{NormalizeToken(rejectedType)} choose complete source grounded trace reject failed trace. Primary gate checked methods calls values terminal return.";

    private static string BuildChosenSignal(string rejectedType)
    {
        var preferred = PreferenceTokens("PREFERRED_CODE_TRACE_OUTPUT", 12);
        var defectType = $"DEFECT_TYPE_{NormalizeToken(rejectedType)}";
        var detail = rejectedType switch
        {
            "missing_callee" => "MISSING_CALLEE_FALSE CALLEE_SOURCE_VISIBLE_TRUE",
            "missing_value_flow" => "MISSING_VALUE_FLOW_FALSE VALUE_FLOWS_COMPLETE_TRUE RETURN_FLOW_COMPLETE_TRUE",
            "wrong_caller" => "WRONG_CALLER_FALSE CALLER_SOURCE_VISIBLE_TRUE",
            "invented_callee" => "INVENTED_CALLEE_FALSE EXTRA_SYMBOL_PRESENT_FALSE",
            "incomplete_trace" => "INCOMPLETE_TRACE_FALSE TERMINAL_RETURN_COMPLETE_TRUE",
            _ => "TRACE_FAILURE_FALSE"
        };
        return string.Join(" ", [
            preferred,
            defectType,
            "CHOSEN_IS_CORRECT",
            "REJECTED_IS_WRONG",
            "TRACE_DEFECT_NONE",
            "SOURCE_GROUNDED_TRUE",
            "CALL_EDGES_COMPLETE_TRUE",
            "VALUE_FLOWS_COMPLETE_TRUE",
            detail,
            "FINAL_PASS",
            preferred
        ]);
    }

    private static string BuildRejectedSignal(string rejectedType)
    {
        var rejected = PreferenceTokens("REJECTED_CODE_TRACE_OUTPUT", 12);
        var defectType = $"DEFECT_TYPE_{NormalizeToken(rejectedType)}";
        var detail = rejectedType switch
        {
            "missing_callee" => "TRACE_DEFECT_MISSING_CALLEE CALL_EDGES_COMPLETE_FALSE CALLEE_SOURCE_VISIBLE_FALSE",
            "missing_value_flow" => "TRACE_DEFECT_MISSING_VALUE_FLOW VALUE_FLOWS_COMPLETE_FALSE RETURN_FLOW_COMPLETE_FALSE",
            "wrong_caller" => "TRACE_DEFECT_WRONG_CALLER CALLER_SOURCE_VISIBLE_FALSE CALL_EDGES_COMPLETE_FALSE",
            "invented_callee" => "TRACE_DEFECT_INVENTED_CALLEE EXTRA_SYMBOL_PRESENT_TRUE CALLEE_SOURCE_VISIBLE_FALSE",
            "incomplete_trace" => "TRACE_DEFECT_INCOMPLETE_TRACE TERMINAL_RETURN_COMPLETE_FALSE VALUE_FLOWS_COMPLETE_FALSE",
            _ => $"TRACE_DEFECT_{NormalizeToken(rejectedType)}"
        };
        return string.Join(" ", [
            rejected,
            defectType,
            $"[WRONG: {rejectedType}]",
            "REJECTED_IS_WRONG",
            "CHOSEN_IS_CORRECT",
            "SOURCE_GROUNDED_FALSE",
            detail,
            "FINAL_FAIL",
            rejected
        ]);
    }

    private static string PreferenceTokens(string token, int count)
        => string.Join(" ", Enumerable.Repeat(token, count));

    private static string NormalizeToken(string value)
        => string.IsNullOrWhiteSpace(value)
            ? "UNKNOWN"
            : string.Concat(value.Select(character => char.IsLetterOrDigit(character) ? char.ToUpperInvariant(character) : '_'));

    private static async Task<string[]> ValidatePublicationGateAsync(
        string batchId,
        ProjectWikiDpoMetricRunResult metrics,
        CancellationToken ct)
    {
        var warnings = new List<string>();
        if (!IsPromotionConfirmationBatch(batchId))
            warnings.Add("code_trace_publication_requires_promotion_confirmation_batch");

        if (string.IsNullOrWhiteSpace(metrics.PolicyCheckpointPath) ||
            string.IsNullOrWhiteSpace(metrics.ReferenceCheckpointPath) ||
            !File.Exists(metrics.PolicyCheckpointPath) ||
            !File.Exists(metrics.ReferenceCheckpointPath))
            warnings.Add("code_trace_publication_requires_policy_and_reference_checkpoints");

        if (metrics.InitialPolicyReferenceMaxAbsDelta < 0)
            warnings.Add("code_trace_initial_policy_reference_delta_missing");

        if (metrics.HeldOutRewardMargin <= 0)
            warnings.Add("code_trace_heldout_margin_not_positive");

        if (!File.Exists(metrics.PairDiagnosticsPath))
        {
            warnings.Add("code_trace_pair_diagnostics_missing");
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
        foreach (var rejectedType in RequiredRejectedTypes)
        {
            var group = heldOut
                .Where(item => string.Equals(item.RejectedType, rejectedType, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (group.Length == 0)
            {
                warnings.Add($"code_trace_missing_heldout_type_{rejectedType}");
                continue;
            }

            if (group.Average(item => item.Accuracy) < MinimumDpoHeldOutAccuracy)
                warnings.Add($"code_trace_heldout_accuracy_below_gate_{rejectedType}");

            if (group.Average(item => item.RewardMargin) <= 0)
                warnings.Add($"code_trace_heldout_margin_not_positive_{rejectedType}");
        }

        return warnings.ToArray();
    }

    private static bool IsPromotionConfirmationBatch(string batchId)
        => batchId.Contains("promotion-confirmation", StringComparison.OrdinalIgnoreCase) &&
           !batchId.Contains("replay", StringComparison.OrdinalIgnoreCase) &&
           !batchId.Contains("focus", StringComparison.OrdinalIgnoreCase) &&
           !batchId.Contains("probe", StringComparison.OrdinalIgnoreCase) &&
           !batchId.Contains("remediation", StringComparison.OrdinalIgnoreCase);
}
