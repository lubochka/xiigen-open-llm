// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Diagnostics;
using System.Globalization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class ProjectDocumentationPipelineOrchestrator : ILogicalOrchestrator
{
    public const string FlowId = "project-documentation-pipeline-orchestrator";
    public const string ScenarioId = "project_documentation_pipeline";

    private readonly DocumentationOrchestrator _documentation;
    private readonly ProjectWikiCoverageVerifier _coverageVerifier;
    private readonly ProjectWikiRealProjectValidationRunner _validationRunner;

    public ProjectDocumentationPipelineOrchestrator(
        DocumentationOrchestrator? documentation = null,
        ProjectWikiCoverageVerifier? coverageVerifier = null,
        ProjectWikiRealProjectValidationRunner? validationRunner = null)
    {
        _documentation = documentation ?? new DocumentationOrchestrator();
        _coverageVerifier = coverageVerifier ?? new ProjectWikiCoverageVerifier();
        _validationRunner = validationRunner ?? new ProjectWikiRealProjectValidationRunner();
    }

    public async Task<OperationResult<ProjectDocumentationPipelineResult>> ExecuteAsync(
        ProjectDocumentationPipelineRequest request,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<ProjectDocumentationPipelineResult>.Fail("Cancelled.", "CANCELLED");

        if (request is null || string.IsNullOrWhiteSpace(request.ProjectRoot))
            return OperationResult<ProjectDocumentationPipelineResult>.Fail("Project root is required.", "PROJECT_ROOT_REQUIRED");

        var started = Stopwatch.StartNew();
        var generated = await _documentation.GenerateAsync(new ProjectDocumentationRequest(
            request.ProjectRoot,
            request.WikiDomainRoot,
            request.DocumentationVersion,
            request.PersistArtifacts), ct);
        if (!generated.IsSuccess)
            return generated.MapError<ProjectDocumentationPipelineResult>();

        ProjectWikiCoverageReport? coverage = null;
        ProjectWikiValidationProjectReport? validation = null;
        if (request.VerifyCoverage)
        {
            var verified = await _coverageVerifier.VerifyGraphAsync(
                generated.Value!.KnowledgeGraph,
                generated.Value.WikiDomainRoot,
                ct);
            if (!verified.IsSuccess)
                return verified.MapError<ProjectDocumentationPipelineResult>();

            coverage = verified.Value;

            var validated = await _validationRunner.ValidateGraphAsync(
                generated.Value.KnowledgeGraph,
                generated.Value.WikiDomainRoot,
                int.MaxValue,
                ct);
            if (!validated.IsSuccess)
                return validated.MapError<ProjectDocumentationPipelineResult>();

            validation = validated.Value;
        }

        started.Stop();
        var warnings = generated.Value!.Warnings
            .Concat(coverage is { IsPassed: false } ? coverage.Failures : [])
            .Concat(validation is { IsPassed: false } ? validation.Failures : [])
            .Append("documentation_graph_ready_training_runner_missing")
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return OperationResult<ProjectDocumentationPipelineResult>.Ok(new ProjectDocumentationPipelineResult(
            generated.Value.ProjectId,
            generated.Value.DocumentationVersion,
            generated.Value.MethodCount,
            generated.Value.CallEdgeCount,
            generated.Value.ValueFlowEdgeCount,
            generated.Value.NegativeDpoPairCount,
            generated.Value.NegativeDtoAcceptanceRate,
            coverage,
            validation,
            generated.Value.KnowledgeGraph,
            warnings));
    }

    public async Task<OperationResult<LogicalOrchestratorResult>> RunAsync(
        LogicalOrchestratorRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            return OperationResult<LogicalOrchestratorResult>.Fail("Logical orchestrator request is required.", "REQUEST_REQUIRED");

        var metadata = request.Metadata ?? new Dictionary<string, object>();
        var projectRoot = ReadString(metadata, "project_root", "");
        if (string.IsNullOrWhiteSpace(projectRoot))
            return OperationResult<LogicalOrchestratorResult>.Fail("Metadata project_root is required.", "PROJECT_ROOT_REQUIRED");

        var pipeline = await ExecuteAsync(new ProjectDocumentationPipelineRequest(
            projectRoot,
            ReadString(metadata, "wiki_domain_root", null),
            ReadString(metadata, "documentation_version", "") ?? "",
            ReadBool(metadata, "persist_artifacts", false),
            ReadBool(metadata, "verify_coverage", true)), cancellationToken);
        if (!pipeline.IsSuccess)
            return pipeline.MapError<LogicalOrchestratorResult>();

        var value = pipeline.Value!;
        var coverage = value.CoverageReport;
        var validation = value.ValidationReport;
        var status = coverage is { IsPassed: true } && validation is { IsPassed: true }
            ? "DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING"
            : "DOCUMENTATION_GRAPH_REVIEW_REQUIRED";
        var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["project_id"] = value.ProjectId,
            ["documentation_version"] = value.DocumentationVersion,
            ["method_count"] = value.MethodCount.ToString(CultureInfo.InvariantCulture),
            ["call_edge_count"] = value.CallEdgeCount.ToString(CultureInfo.InvariantCulture),
            ["value_flow_edge_count"] = value.ValueFlowEdgeCount.ToString(CultureInfo.InvariantCulture),
            ["negative_dpo_pair_count"] = value.NegativeDpoPairCount.ToString(CultureInfo.InvariantCulture),
            ["negative_dto_acceptance_rate"] = value.NegativeDtoAcceptanceRate.ToString("0.######", CultureInfo.InvariantCulture),
            ["coverage_passed"] = coverage is { IsPassed: true } ? "1" : "0",
            ["exact_evidence_accuracy"] = (coverage?.ExactEvidenceAccuracy ?? 0).ToString("0.######", CultureInfo.InvariantCulture),
            ["developer_reasoning_validation_passed"] = validation is { IsPassed: true } ? "1" : "0",
            ["developer_reasoning_exact_evidence_accuracy"] = (validation?.ExactEvidenceAccuracy ?? 0).ToString("0.######", CultureInfo.InvariantCulture),
            ["entity_impact_trace_pass_rate"] = (validation?.EntityImpactTracePassRate ?? 0).ToString("0.######", CultureInfo.InvariantCulture),
            ["validation_cpu_ms"] = (validation?.CpuMs ?? 0).ToString("0.######", CultureInfo.InvariantCulture),
            ["validation_elapsed_ms"] = (validation?.ElapsedMs ?? 0).ToString("0.######", CultureInfo.InvariantCulture),
            ["validation_peak_ram_mb"] = (validation?.PeakRamMb ?? 0).ToString("0.######", CultureInfo.InvariantCulture),
            ["training_status"] = "runner_missing"
        };
        var summary = new LogicalFlowSummary(
            request.RunId,
            value.MethodCount,
            0,
            Math.Min(coverage?.ExactEvidenceAccuracy ?? 0, validation?.EntityImpactTracePassRate ?? 0),
            "project_documentation_graph_coverage",
            false,
            coverage is { IsPassed: true } && validation is { IsPassed: true },
            false,
            status,
            validation?.ExactEvidenceAccuracy ?? coverage?.ExactEvidenceAccuracy ?? 0,
            "documentation_pipeline",
            $"Built project documentation knowledge graph for {value.ProjectId}; developer-reasoning validation passed={(validation is { IsPassed: true })}; training runner is missing.",
            0,
            0);
        var output = $"Project {value.ProjectId} documentation graph built with {value.MethodCount} methods, {value.CallEdgeCount} call edges, {value.NegativeDpoPairCount} negative DPO pairs, and developer-reasoning validation exact_accuracy={(validation?.ExactEvidenceAccuracy ?? 0).ToString("0.######", CultureInfo.InvariantCulture)}. Training runner is still missing; no trained-weight claim was made.";
        var flowResult = new LogicalFlowResult(
            FlowId,
            request.ScenarioId,
            status,
            output,
            summary,
            metrics,
            value.KnowledgeGraph.EvidenceRecords.Select(record => record.EvidenceId).ToArray(),
            value.Warnings);

        return OperationResult<LogicalOrchestratorResult>.Ok(new LogicalOrchestratorResult(
            request.ScenarioId,
            FlowId,
            status,
            output,
            request.Flags ?? new LogicalExecutionFlags(),
            flowResult)
        {
            BranchResults = [flowResult],
            Warnings = value.Warnings
        });
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
}
