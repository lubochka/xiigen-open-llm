// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Globalization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class ProjectDocumentationPipelineFlowAdapter : ILogicalFlow<string>
{
    public const string FlowId = "project-documentation-pipeline-flow";
    public const string ScenarioId = "project_documentation_pipeline";

    private readonly ProjectDocumentationPipelineOrchestrator _pipeline;

    public ProjectDocumentationPipelineFlowAdapter(ProjectDocumentationPipelineOrchestrator? pipeline = null)
    {
        _pipeline = pipeline ?? new ProjectDocumentationPipelineOrchestrator();
    }

    public async Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
        LogicalFlowRequest<string> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalFlowResult>.Fail("Cancelled.", "CANCELLED");

        if (request is null)
            return OperationResult<LogicalFlowResult>.Fail("Logical flow request is required.", "REQUEST_REQUIRED");

        var projectRoot = ReadString(request.Metadata, "project_root", "");
        if (string.IsNullOrWhiteSpace(projectRoot))
            return OperationResult<LogicalFlowResult>.Ok(BuildProjectRootRequiredResult(request));

        var result = await _pipeline.ExecuteAsync(new ProjectDocumentationPipelineRequest(
            projectRoot,
            ReadString(request.Metadata, "wiki_domain_root", null),
            ReadString(request.Metadata, "documentation_version", "") ?? "",
            ReadBool(request.Metadata, "persist_artifacts", false),
            ReadBool(request.Metadata, "verify_coverage", true)), cancellationToken);
        if (!result.IsSuccess)
            return result.MapError<LogicalFlowResult>();

        var value = result.Value!;
        var coverage = value.CoverageReport;
        var validation = value.ValidationReport;
        var passed = coverage is { IsPassed: true } && validation is { IsPassed: true };
        var verdict = passed
            ? "DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING"
            : "DOCUMENTATION_GRAPH_REVIEW_REQUIRED";
        var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["project_id"] = value.ProjectId,
            ["documentation_version"] = value.DocumentationVersion,
            ["trace_status"] = value.KnowledgeGraph.CodeTrace.Status,
            ["source_file_count"] = value.KnowledgeGraph.CodeTrace.SourceFileCount.ToString(CultureInfo.InvariantCulture),
            ["method_count"] = value.MethodCount.ToString(CultureInfo.InvariantCulture),
            ["call_edge_count"] = value.CallEdgeCount.ToString(CultureInfo.InvariantCulture),
            ["value_flow_edge_count"] = value.ValueFlowEdgeCount.ToString(CultureInfo.InvariantCulture),
            ["negative_dpo_pair_count"] = value.NegativeDpoPairCount.ToString(CultureInfo.InvariantCulture),
            ["negative_dto_acceptance_rate"] = value.NegativeDtoAcceptanceRate.ToString("0.######", CultureInfo.InvariantCulture),
            ["coverage_passed"] = passed ? "1" : "0",
            ["exact_evidence_accuracy"] = (coverage?.ExactEvidenceAccuracy ?? 0).ToString("0.######", CultureInfo.InvariantCulture),
            ["developer_reasoning_validation_passed"] = validation is { IsPassed: true } ? "1" : "0",
            ["developer_reasoning_exact_evidence_accuracy"] = (validation?.ExactEvidenceAccuracy ?? 0).ToString("0.######", CultureInfo.InvariantCulture),
            ["entity_impact_trace_pass_rate"] = (validation?.EntityImpactTracePassRate ?? 0).ToString("0.######", CultureInfo.InvariantCulture),
            ["validation_cpu_ms"] = (validation?.CpuMs ?? 0).ToString("0.######", CultureInfo.InvariantCulture),
            ["validation_elapsed_ms"] = (validation?.ElapsedMs ?? 0).ToString("0.######", CultureInfo.InvariantCulture),
            ["validation_peak_ram_mb"] = (validation?.PeakRamMb ?? 0).ToString("0.######", CultureInfo.InvariantCulture),
            ["training_status"] = "runner_missing"
        };
        CopyMetadataMetric(request.Metadata, metrics, "routed_phase_id");
        CopyMetadataMetric(request.Metadata, metrics, "routed_sub_phase_id");
        CopyMetadataMetric(request.Metadata, metrics, "project_wiki_pre_route_kind");
        CopyMetadataMetric(request.Metadata, metrics, "project_wiki_pre_route_target_unit_id");
        CopyMetadataMetric(request.Metadata, metrics, "project_wiki_pre_route_confidence");
        var summary = new LogicalFlowSummary(
            request.RunId,
            value.MethodCount,
            0,
            Math.Min(coverage?.ExactEvidenceAccuracy ?? 0, validation?.EntityImpactTracePassRate ?? 0),
            "project_documentation_trace_coverage",
            false,
            passed,
            false,
            verdict,
            validation?.ExactEvidenceAccuracy ?? coverage?.ExactEvidenceAccuracy ?? 0,
            "documentation_pipeline",
            $"Project code trace built for {value.ProjectId}; developer-reasoning validation passed={(validation is { IsPassed: true })}; training runner is missing.",
            0,
            0);
        var final = $"Project {value.ProjectId} code trace and documentation graph built from one orchestrated request. Developer-reasoning validation exact_accuracy={(validation?.ExactEvidenceAccuracy ?? 0).ToString("0.######", CultureInfo.InvariantCulture)}. Training runner is missing, so no trained weights were claimed.";

        return OperationResult<LogicalFlowResult>.Ok(new LogicalFlowResult(
            FlowId,
            ScenarioId,
            verdict,
            final,
            summary,
            metrics,
            value.KnowledgeGraph.EvidenceRecords.Select(record => record.EvidenceId).ToArray(),
            value.Warnings));
    }

    private static LogicalFlowResult BuildProjectRootRequiredResult(LogicalFlowRequest<string> request)
    {
        var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["trace_status"] = "INTAKE_REJECTED_PROJECT_ROOT_REQUIRED",
            ["training_status"] = "not_started",
            ["coverage_passed"] = "0"
        };
        var summary = new LogicalFlowSummary(
            request.RunId,
            0,
            0,
            0,
            "project_documentation_trace_coverage",
            false,
            false,
            false,
            "PROJECT_ROOT_REQUIRED",
            0,
            "documentation_pipeline",
            "The documentation/wiki flow requires metadata project_root before code tracing can start.",
            0,
            0);

        return new LogicalFlowResult(
            FlowId,
            ScenarioId,
            "PROJECT_ROOT_REQUIRED",
            "Project documentation/wiki request was routed correctly, but metadata project_root is required before code tracing can start.",
            summary,
            metrics,
            [],
            ["project_root_required"]);
    }

    private static string? ReadString(IReadOnlyDictionary<string, object> metadata, string key, string? fallback)
        => metadata.TryGetValue(key, out var value) && value is not null && !string.IsNullOrWhiteSpace(value.ToString())
            ? value.ToString()
            : fallback;

    private static bool ReadBool(IReadOnlyDictionary<string, object> metadata, string key, bool fallback)
        => metadata.TryGetValue(key, out var value) &&
           bool.TryParse(value.ToString(), out var parsed)
            ? parsed
            : fallback;

    private static void CopyMetadataMetric(
        IReadOnlyDictionary<string, object> metadata,
        Dictionary<string, string> metrics,
        string key)
    {
        if (metadata.TryGetValue(key, out var value) && value is not null && !string.IsNullOrWhiteSpace(value.ToString()))
            metrics[key] = value.ToString()!;
    }
}
