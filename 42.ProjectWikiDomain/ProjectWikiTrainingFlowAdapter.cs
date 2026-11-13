// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Globalization;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class ProjectWikiTrainingFlowAdapter : ILogicalFlow<string>
{
    public const string FlowId = "project-wiki-training-flow";
    public const string ScenarioId = "project_wiki_training_orchestrator";

    private static readonly JsonSerializerOptions ReportJsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly ProjectWikiTrainingOrchestrator _trainingOrchestrator;

    public ProjectWikiTrainingFlowAdapter(ProjectWikiTrainingOrchestrator? trainingOrchestrator = null)
    {
        _trainingOrchestrator = trainingOrchestrator ?? new ProjectWikiTrainingOrchestrator();
    }

    public async Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
        LogicalFlowRequest<string> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalFlowResult>.Fail("Cancelled.", "CANCELLED");

        if (request is null)
            return OperationResult<LogicalFlowResult>.Fail("Logical flow request is required.", "REQUEST_REQUIRED");

        var trainingMode = ReadString(request.Metadata, "training_mode", "");
        if (IsPlanNextTrainingActionMode(trainingMode))
            return await ExecutePlanNextTrainingActionAsync(request, cancellationToken);

        var artifactRoot = ReadString(request.Metadata, "artifact_root", "");
        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<LogicalFlowResult>.Ok(BuildArtifactRootRequiredResult(request));

        var result = await _trainingOrchestrator.RunAsync(
            request.Input,
            artifactRoot,
            ReadString(request.Metadata, "batch_id", ""),
            ReadString(request.Metadata, "server_base_url", "https://models.local"),
            ReadString(request.Metadata, "learning_data_root", ""),
            cancellationToken);
        if (!result.IsSuccess)
            return result.MapError<LogicalFlowResult>();

        var value = result.Value!;
        var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["route_kind"] = value.RoutingDecision.RouteKind.ToString(),
            ["target_unit_id"] = value.TargetUnitId,
            ["batch_id"] = value.BatchId,
            ["training_row_count"] = value.TrainingRowCount.ToString(CultureInfo.InvariantCulture),
            ["held_out_row_count"] = value.HeldOutRowCount.ToString(CultureInfo.InvariantCulture),
            ["primary_accuracy"] = value.PrimaryAccuracy.ToString("0.######", CultureInfo.InvariantCulture),
            ["secondary_accuracy"] = value.SecondaryAccuracy.ToString("0.######", CultureInfo.InvariantCulture),
            ["wrong_annotation_rate"] = value.WrongAnnotationRate.ToString("0.######", CultureInfo.InvariantCulture),
            ["cpu_ms"] = value.CpuMilliseconds.ToString("0.######", CultureInfo.InvariantCulture),
            ["peak_ram_mb"] = value.PeakWorkingSetMb.ToString("0.######", CultureInfo.InvariantCulture),
            ["elapsed_ms"] = value.ElapsedMilliseconds.ToString("0.######", CultureInfo.InvariantCulture),
            ["manifest_path"] = value.GeneratedCapabilityManifestPath ?? "",
            ["training_status"] = value.TrainingRowCount <= 0
                ? "not_training_route"
                : value.GeneratedCapabilityManifestPath is not null
                    ? "published"
                    : "trained_not_published"
        };
        CopyMetadataMetric(request.Metadata, metrics, "routed_phase_id");
        CopyMetadataMetric(request.Metadata, metrics, "routed_sub_phase_id");
        CopyMetadataMetric(request.Metadata, metrics, "project_wiki_pre_route_kind");
        CopyMetadataMetric(request.Metadata, metrics, "project_wiki_pre_route_target_unit_id");
        CopyMetadataMetric(request.Metadata, metrics, "project_wiki_pre_route_confidence");
        AddOptionalMetric(metrics, "dpo_loss", value.DpoLoss);
        AddOptionalMetric(metrics, "dpo_accuracy", value.DpoAccuracy);
        AddOptionalMetric(metrics, "dpo_reward_margin", value.DpoRewardMargin);
        AddOptionalMetric(metrics, "dpo_heldout_loss", value.DpoHeldOutLoss);
        AddOptionalMetric(metrics, "dpo_heldout_accuracy", value.DpoHeldOutAccuracy);
        AddOptionalMetric(metrics, "dpo_heldout_reward_margin", value.DpoHeldOutRewardMargin);
        AddOptionalMetric(metrics, "dpo_average_cpu_percent", value.DpoAverageCpuPercent);
        AddOptionalMetric(metrics, "dpo_cpu_time_ms", value.DpoCpuTimeMs);
        AddOptionalMetric(metrics, "dpo_peak_ram_mb", value.DpoPeakWorkingSetMb);
        AddOptionalMetric(metrics, "dpo_elapsed_ms", value.DpoTotalElapsedMs);
        if (!string.IsNullOrWhiteSpace(value.DpoRunnerReportPath))
            metrics["dpo_runner_report_path"] = value.DpoRunnerReportPath!;
        if (!string.IsNullOrWhiteSpace(value.DpoPairDiagnosticsPath))
            metrics["dpo_pair_diagnostics_path"] = value.DpoPairDiagnosticsPath!;
        var runnerReport = value.TrainingRowCount > 0
            ? await WriteRunnerReportAsync(request, value, artifactRoot, metrics, cancellationToken)
            : OperationResult<string>.Ok("");
        if (!runnerReport.IsSuccess)
            return runnerReport.MapError<LogicalFlowResult>();

        if (!string.IsNullOrWhiteSpace(runnerReport.Value))
            metrics["runner_report_path"] = runnerReport.Value!;

        var summary = new LogicalFlowSummary(
            request.RunId,
            value.TrainingRowCount,
            0,
            value.PrimaryAccuracy,
            "project_wiki_training_unit_metric",
            false,
            value.GeneratedCapabilityManifestPath is not null,
            false,
            value.Verdict,
            value.PrimaryAccuracy,
            value.TargetUnitId,
            $"Routed project-wiki training prompt to {value.TargetUnitId}; rows={value.TrainingRowCount}; primary_accuracy={value.PrimaryAccuracy.ToString("0.######", CultureInfo.InvariantCulture)}; next={value.NextAction}",
            value.CpuMilliseconds,
            value.ElapsedMilliseconds);
        var final = $"Project wiki training route={value.RoutingDecision.RouteKind}; target={value.TargetUnitId}; verdict={value.Verdict}; train_rows={value.TrainingRowCount}; heldout_rows={value.HeldOutRowCount}; primary_accuracy={value.PrimaryAccuracy.ToString("0.######", CultureInfo.InvariantCulture)}; dpo_accuracy={FormatOptional(value.DpoAccuracy)}; dpo_reward_margin={FormatOptional(value.DpoRewardMargin)}; dpo_loss={FormatOptional(value.DpoLoss)}; dpo_heldout_accuracy={FormatOptional(value.DpoHeldOutAccuracy)}; dpo_cpu_avg_percent={FormatOptional(value.DpoAverageCpuPercent)}; dpo_cpu_time_ms={FormatOptional(value.DpoCpuTimeMs)}; dpo_ram_peak_mb={FormatOptional(value.DpoPeakWorkingSetMb)}; dpo_elapsed_ms={FormatOptional(value.DpoTotalElapsedMs)}; next={value.NextAction}";

        return OperationResult<LogicalFlowResult>.Ok(new LogicalFlowResult(
            FlowId,
            ScenarioId,
            value.Verdict,
            final,
            summary,
            metrics,
            BuildEvidenceIds(value.GeneratedCapabilityManifestPath, runnerReport.Value),
            value.Warnings));
    }

    private async Task<OperationResult<LogicalFlowResult>> ExecutePlanNextTrainingActionAsync(
        LogicalFlowRequest<string> request,
        CancellationToken cancellationToken)
    {
        var learningDataRoot = ReadString(request.Metadata, "learning_data_root", "");
        if (string.IsNullOrWhiteSpace(learningDataRoot))
            return OperationResult<LogicalFlowResult>.Fail("Learning data root is required for registry-driven next-action planning.", "LEARNING_DATA_ROOT_REQUIRED");

        var planned = _trainingOrchestrator.PlanNextTrainingAction(learningDataRoot);
        if (!planned.IsSuccess)
            return planned.MapError<LogicalFlowResult>();

        var value = planned.Value!;
        var action = value.NextTrainingAction;
        var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["training_mode"] = "plan_next_training_action",
            ["training_status"] = "plan_only_no_batch_created",
            ["route_kind"] = "registry_next_training_action",
            ["target_unit_id"] = action.TargetUnitName,
            ["registry_entry_count"] = value.RegistryEntryCount.ToString(CultureInfo.InvariantCulture),
            ["accepted_count"] = ReadStatusCount(value.StatusCounts, "accepted"),
            ["rejected_count"] = ReadStatusCount(value.StatusCounts, "rejected"),
            ["experimental_no_authority_count"] = ReadStatusCount(value.StatusCounts, "experimental_no_authority"),
            ["blocked_count"] = ReadStatusCount(value.StatusCounts, "blocked"),
            ["recommended_batch_strategy"] = value.RecommendedBatchStrategy,
            ["recommended_batch_size"] = "50",
            ["new_training_batch_allowed"] = value.NewTrainingBatchAllowed.ToString(),
            ["stop_blind_training"] = action.StopBlindTraining.ToString(),
            ["training_row_count"] = "0",
            ["held_out_row_count"] = "0",
            ["primary_accuracy"] = "UNKNOWN",
            ["secondary_accuracy"] = "UNKNOWN",
            ["wrong_annotation_rate"] = "UNKNOWN",
            ["cpu_ms"] = "UNKNOWN",
            ["peak_ram_mb"] = "UNKNOWN",
            ["elapsed_ms"] = "UNKNOWN"
        };

        var report = await WriteNextTrainingActionReportAsync(request, value, learningDataRoot, metrics, cancellationToken);
        if (!report.IsSuccess)
            return report.MapError<LogicalFlowResult>();

        metrics["next_training_action_report_path"] = report.Value!;
        var final = $"Project wiki next training action target={action.TargetUnitName}; strategy={value.RecommendedBatchStrategy}; reason={action.Reason}; evidence_batches={string.Join(",", action.EvidenceBatchIds)}; new_batch_allowed={value.NewTrainingBatchAllowed}; training_started=false; new_batches_created=0";
        var summary = new LogicalFlowSummary(
            request.RunId,
            0,
            0,
            0,
            "project_wiki_next_training_action",
            false,
            false,
            false,
            "NEXT_TRAINING_ACTION_SELECTED",
            0,
            action.TargetUnitName,
            final,
            0,
            0);

        return OperationResult<LogicalFlowResult>.Ok(new LogicalFlowResult(
            FlowId,
            ScenarioId,
            "NEXT_TRAINING_ACTION_SELECTED",
            final,
            summary,
            metrics,
            BuildEvidenceIds(report.Value, null),
            value.Warnings));
    }

    private static async Task<OperationResult<string>> WriteRunnerReportAsync(
        LogicalFlowRequest<string> request,
        ProjectWikiTrainingOrchestratorResult value,
        string artifactRoot,
        IReadOnlyDictionary<string, string> metrics,
        CancellationToken ct)
    {
        try
        {
            var hasDpoMetrics = value.DpoLoss.HasValue &&
                                value.DpoAccuracy.HasValue &&
                                value.DpoRewardMargin.HasValue &&
                                value.DpoHeldOutAccuracy.HasValue;
            var missingRunnerMetrics = hasDpoMetrics
                ? new[] { "reward_margin_delta", "loss_delta" }
                : ["reward_margin", "reward_margin_delta", "loss", "loss_delta", "heldout_accuracy"];
            var reportRoot = Path.Combine(artifactRoot, "runner-reports");
            Directory.CreateDirectory(reportRoot);
            var reportPath = Path.Combine(reportRoot, $"{Sanitize(value.BatchId)}-{Sanitize(value.TargetUnitId)}-section22.json");
            var cpuTimeMs = value.DpoCpuTimeMs ?? value.CpuMilliseconds;
            var elapsedMs = value.DpoTotalElapsedMs ?? value.ElapsedMilliseconds;
            var ramPeakMb = value.DpoPeakWorkingSetMb ?? value.PeakWorkingSetMb;
            var report = new
            {
                report_scope = "training_runner_report",
                source_of_truth = value.DpoRunnerReportPath ?? nameof(ProjectWikiTrainingFlowAdapter),
                report_artifact_path = reportPath,
                artifact_written_before_message = true,
                runner_output_available = true,
                section_22_applicable = true,
                false_positive_risk = hasDpoMetrics ? "none" : "metrics_missing",
                completion_claim_allowed = false,
                allowed_verdict_words = new[] { value.Verdict },
                missing_runner_metrics = missingRunnerMetrics,
                run_id = request.RunId,
                unit_name = value.TargetUnitId,
                batch_id = value.BatchId,
                rows_trained_this_run = value.TrainingRowCount,
                rows_trained_total_for_unit = value.TrainingRowCount,
                standalone_or_combo = "standalone",
                weighted_accuracy = value.PrimaryAccuracy,
                weighted_accuracy_delta = "UNKNOWN",
                reward_margin = FormatOptional(value.DpoRewardMargin),
                reward_margin_delta = "UNKNOWN",
                loss = FormatOptional(value.DpoLoss),
                loss_delta = "UNKNOWN",
                heldout_accuracy = FormatOptional(value.DpoHeldOutAccuracy),
                heldout_loss = FormatOptional(value.DpoHeldOutLoss),
                heldout_reward_margin = FormatOptional(value.DpoHeldOutRewardMargin),
                dpo_accuracy = FormatOptional(value.DpoAccuracy),
                exact_evidence_accuracy = metrics.TryGetValue("exact_evidence_accuracy", out var exactEvidenceAccuracy) ? exactEvidenceAccuracy : "UNKNOWN",
                hallucination_rate = metrics.TryGetValue("hallucination_rate", out var hallucinationRate) ? hallucinationRate : "UNKNOWN",
                cpu_peak_percent = "UNKNOWN",
                cpu_avg_percent = FormatOptional(value.DpoAverageCpuPercent),
                cpu_time_ms = cpuTimeMs,
                ram_peak_mb = ramPeakMb,
                ram_delta_mb = "UNKNOWN",
                elapsed_ms = elapsedMs,
                elapsed_per_row_ms = value.TrainingRowCount == 0 ? 0 : elapsedMs / value.TrainingRowCount,
                next_action = value.NextAction,
                verdict = value.Verdict,
                manifest_path = value.GeneratedCapabilityManifestPath ?? "",
                dpo_runner_report_path = value.DpoRunnerReportPath ?? "",
                dpo_pair_diagnostics_path = value.DpoPairDiagnosticsPath ?? "",
                warnings = value.Warnings
            };

            await File.WriteAllTextAsync(reportPath, JsonSerializer.Serialize(report, ReportJsonOptions), ct);
            return OperationResult<string>.Ok(reportPath);
        }
        catch (Exception ex)
        {
            return OperationResult<string>.Fail($"Failed to write runner report: {ex.Message}", "RUNNER_REPORT_WRITE_FAILED");
        }
    }

    private static async Task<OperationResult<string>> WriteNextTrainingActionReportAsync(
        LogicalFlowRequest<string> request,
        ProjectWikiTrainingLoopPlanResult value,
        string learningDataRoot,
        IReadOnlyDictionary<string, string> metrics,
        CancellationToken ct)
    {
        try
        {
            var reportRoot = Path.Combine(learningDataRoot, "reports");
            Directory.CreateDirectory(reportRoot);
            var reportPath = Path.Combine(reportRoot, "PROJECT-WIKI-NEXT-TRAINING-ACTION.json");
            var report = new
            {
                report_scope = "next_training_action",
                source_of_truth = "PROJECT-WIKI-TRAINING-ASSET-REGISTRY.json",
                report_artifact_path = reportPath,
                artifact_written_before_message = true,
                training_started = false,
                new_batches_created = 0,
                run_id = request.RunId,
                registry_entry_count = value.RegistryEntryCount,
                status_counts = value.StatusCounts,
                target_unit_name = value.NextTrainingAction.TargetUnitName,
                reason = value.NextTrainingAction.Reason,
                evidence_batch_ids = value.NextTrainingAction.EvidenceBatchIds,
                recommended_batch_strategy = value.RecommendedBatchStrategy,
                recommended_batch_size = 50,
                new_training_batch_allowed = value.NewTrainingBatchAllowed,
                stop_blind_training = value.NextTrainingAction.StopBlindTraining,
                metrics,
                warnings = value.Warnings
            };

            await File.WriteAllTextAsync(reportPath, JsonSerializer.Serialize(report, ReportJsonOptions), ct);
            return OperationResult<string>.Ok(reportPath);
        }
        catch (Exception ex)
        {
            return OperationResult<string>.Fail($"Failed to write next-training-action report: {ex.Message}", "NEXT_TRAINING_ACTION_REPORT_WRITE_FAILED");
        }
    }

    private static string[] BuildEvidenceIds(string? manifestPath, string? runnerReportPath)
        => new[] { manifestPath ?? "", runnerReportPath ?? "" }
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToArray();

    private static string Sanitize(string value)
        => string.Join("_", value.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));

    private static LogicalFlowResult BuildArtifactRootRequiredResult(LogicalFlowRequest<string> request)
    {
        var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["training_status"] = "not_started",
            ["route_kind"] = "unknown",
            ["target_unit_id"] = "",
            ["training_row_count"] = "0",
            ["held_out_row_count"] = "0",
            ["primary_accuracy"] = "0",
            ["cpu_ms"] = "0",
            ["peak_ram_mb"] = "0",
            ["elapsed_ms"] = "0"
        };
        var summary = new LogicalFlowSummary(
            request.RunId,
            0,
            0,
            0,
            "project_wiki_training_unit_metric",
            false,
            false,
            false,
            "ARTIFACT_ROOT_REQUIRED",
            0,
            "project-wiki-training",
            "The project wiki training flow requires metadata artifact_root before it can publish a unit manifest.",
            0,
            0);

        return new LogicalFlowResult(
            FlowId,
            ScenarioId,
            "ARTIFACT_ROOT_REQUIRED",
            "Project wiki training request was routed correctly, but metadata artifact_root is required before a 50-row batch can run and publish.",
            summary,
            metrics,
            [],
            ["artifact_root_required"]);
    }

    private static string ReadString(IReadOnlyDictionary<string, object> metadata, string key, string fallback)
        => metadata.TryGetValue(key, out var value) && value is not null && !string.IsNullOrWhiteSpace(value.ToString())
            ? value.ToString()!
            : fallback;

    private static void CopyMetadataMetric(
        IReadOnlyDictionary<string, object> metadata,
        Dictionary<string, string> metrics,
        string key)
    {
        if (metadata.TryGetValue(key, out var value) && value is not null && !string.IsNullOrWhiteSpace(value.ToString()))
            metrics[key] = value.ToString()!;
    }

    private static void AddOptionalMetric(Dictionary<string, string> metrics, string key, double? value)
    {
        if (value.HasValue)
            metrics[key] = value.Value.ToString("0.######", CultureInfo.InvariantCulture);
    }

    private static bool IsPlanNextTrainingActionMode(string mode)
        => string.Equals(mode, "plan_next_training_action", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(mode, "registry_plan_only", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(mode, "next_training_action", StringComparison.OrdinalIgnoreCase);

    private static string ReadStatusCount(IReadOnlyDictionary<string, int> statusCounts, string status)
        => statusCounts.TryGetValue(status, out var count)
            ? count.ToString(CultureInfo.InvariantCulture)
            : "0";

    private static string FormatOptional(double? value)
        => value.HasValue ? value.Value.ToString("0.######", CultureInfo.InvariantCulture) : "UNKNOWN";
}
