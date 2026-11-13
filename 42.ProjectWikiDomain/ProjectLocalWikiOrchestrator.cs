// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Diagnostics;
using System.Globalization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class ProjectLocalWikiOrchestrator : ILogicalOrchestrator
{
    public const string FlowId = "project-local-wiki-orchestrator";
    public const string ScenarioId = "project_local_wiki_answering";

    private readonly ProjectWikiQueryPlanningFlow _queryPlanningFlow;
    private readonly WikiContextRetriever _retriever;
    private readonly ProjectLocalWikiModelUnit _modelUnit;
    private readonly ProjectAnswerVerifier _verifier;
    private readonly ProjectEvidenceIndex _evidenceIndex;
    private readonly string _projectId;
    private readonly string _wikiDomainRoot;

    public ProjectLocalWikiOrchestrator(
        ProjectWikiQueryPlanningFlow queryPlanningFlow,
        WikiContextRetriever retriever,
        ProjectLocalWikiModelUnit modelUnit,
        ProjectAnswerVerifier verifier,
        ProjectEvidenceIndex evidenceIndex,
        string projectId,
        string wikiDomainRoot)
    {
        _queryPlanningFlow = queryPlanningFlow;
        _retriever = retriever;
        _modelUnit = modelUnit;
        _verifier = verifier;
        _evidenceIndex = evidenceIndex;
        _projectId = projectId;
        _wikiDomainRoot = wikiDomainRoot;
    }

    public async Task<OperationResult<ProjectWikiAnswer>> AskAsync(
        ProjectWikiQuestionRequest request,
        CancellationToken ct = default)
    {
        var answer = await AskWithTraceAsync(request, ct);
        if (!answer.IsSuccess)
            return answer.MapError<ProjectWikiAnswer>();

        return OperationResult<ProjectWikiAnswer>.Ok(answer.Value!.Answer);
    }

    public async Task<OperationResult<ProjectWikiAnswerTraceResult>> AskWithTraceAsync(
        ProjectWikiQuestionRequest request,
        CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var process = Process.GetCurrentProcess();
        var cpuStart = process.TotalProcessorTime;
        var memoryStart = GC.GetTotalMemory(forceFullCollection: false);
        var planned = await _queryPlanningFlow.PlanAsync(BuildUserQuestion(request), ct);
        if (!planned.IsSuccess)
            return planned.MapError<ProjectWikiAnswerTraceResult>();

        var context = await _retriever.RetrieveAsync(request, planned.Value!, ct);
        if (!context.IsSuccess)
            return context.MapError<ProjectWikiAnswerTraceResult>();

        var answer = await _modelUnit.AnswerAsync(request, context.Value!, ct);
        if (!answer.IsSuccess)
            return answer.MapError<ProjectWikiAnswerTraceResult>();

        var answerValue = answer.Value!;
        var enrichedWarnings = answerValue.Warnings
            .Concat(planned.Value!.Warnings)
            .Concat(["query_plan_applied"])
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        answerValue = answerValue with { Warnings = enrichedWarnings };

        var verified = _verifier.Verify(answerValue, _evidenceIndex, request.ProjectId);
        if (verified.IsPassed)
        {
            var passedTrace = BuildTrace(request, planned.Value!, context.Value!, answerValue, verified, stopwatch, cpuStart, memoryStart, "PASS");
            return OperationResult<ProjectWikiAnswerTraceResult>.Ok(new ProjectWikiAnswerTraceResult(answerValue, planned.Value!, passedTrace));
        }

        var warnings = answerValue.Warnings
            .Concat(verified.Warnings)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var repaired = answerValue with
        {
            Confidence = Math.Min(answerValue.Confidence, 0.64),
            Warnings = warnings,
            MissingEvidence = string.IsNullOrWhiteSpace(answerValue.MissingEvidence)
                ? "answer_verification_failed"
                : answerValue.MissingEvidence
        };

        var trace = BuildTrace(request, planned.Value!, context.Value!, repaired, verified, stopwatch, cpuStart, memoryStart, "CONCERN");
        return OperationResult<ProjectWikiAnswerTraceResult>.Ok(new ProjectWikiAnswerTraceResult(repaired, planned.Value!, trace));
    }

    public async Task<OperationResult<LogicalOrchestratorResult>> RunAsync(
        LogicalOrchestratorRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            return OperationResult<LogicalOrchestratorResult>.Fail("Logical orchestrator request is required.", "REQUEST_REQUIRED");

        if (string.IsNullOrWhiteSpace(request.UserRequest))
            return OperationResult<LogicalOrchestratorResult>.Fail("User request cannot be empty.", "EMPTY_REQUEST");

        var flags = request.Flags ?? new LogicalExecutionFlags();
        var wikiRequest = BuildWikiRequest(request);
        var answer = await AskWithTraceAsync(wikiRequest, cancellationToken);
        if (!answer.IsSuccess)
            return answer.MapError<LogicalOrchestratorResult>();

        var answerValue = answer.Value!.Answer;
        var queryPlan = answer.Value.QueryPlan;
        var trace = answer.Value.Trace;
        var verification = _verifier.Verify(answerValue, _evidenceIndex, wikiRequest.ProjectId);
        var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["project_id"] = answerValue.ProjectId,
            ["documentation_version"] = answerValue.DocumentationVersion,
            ["retrieval_mode"] = answerValue.RetrievalMode.ToString(),
            ["evidence_count"] = answerValue.EvidenceIds.Length.ToString(CultureInfo.InvariantCulture),
            ["confidence"] = answerValue.Confidence.ToString("0.######", CultureInfo.InvariantCulture),
            ["citation_accuracy"] = verification.CitationAccuracy.ToString("0.######", CultureInfo.InvariantCulture),
            ["project_boundary_passed"] = verification.IsProjectBoundaryPassed ? "1" : "0",
            ["semantic_unavailable"] = answerValue.Warnings.Contains("semantic_retrieval_unavailable_fallback_to_bm25", StringComparer.Ordinal) ? "1" : "0",
            ["query_plan_invoked"] = "1",
            ["query_plan_intent"] = queryPlan.Intent.ToString(),
            ["query_plan_step_count"] = queryPlan.RequiredQuerySteps.Length.ToString(CultureInfo.InvariantCulture),
            ["query_plan_sufficiency_score"] = queryPlan.PlanSufficiencyScore.ToString("0.######", CultureInfo.InvariantCulture),
            ["answer_readiness_score"] = queryPlan.AnswerReadinessScore.ToString("0.######", CultureInfo.InvariantCulture),
            ["wiki_query_trace_status"] = trace.FinalStatus,
            ["wiki_query_trace_cpu_ms"] = trace.CpuMs.ToString("0.######", CultureInfo.InvariantCulture),
            ["wiki_query_trace_elapsed_ms"] = trace.ElapsedMs.ToString("0.######", CultureInfo.InvariantCulture),
            ["wiki_query_trace_peak_ram_mb"] = trace.PeakRamMb.ToString("0.######", CultureInfo.InvariantCulture),
            ["executed_query_step_count"] = trace.ExecutedQuerySteps.Length.ToString(CultureInfo.InvariantCulture),
            ["wiki_domain_root"] = wikiRequest.WikiDomainRoot
        };
        var passed = verification.IsPassed && answerValue.Confidence >= 0.65;
        var verdict = passed ? "PROJECT_WIKI_ANSWER_READY" : "PROJECT_WIKI_ANSWER_REVIEW_REQUIRED";
        var summary = new LogicalFlowSummary(
            request.RunId,
            1,
            0,
            answerValue.Confidence,
            "project_wiki_answer_confidence",
            false,
            passed,
            false,
            verdict,
            verification.CitationAccuracy,
            answerValue.RetrievalMode.ToString(),
            $"Answered project-local wiki question with {answerValue.EvidenceIds.Length} evidence citations.",
            0,
            0);
        var flowResult = new LogicalFlowResult(
            FlowId,
            request.ScenarioId,
            verdict,
            answerValue.Answer,
            summary,
            metrics,
            answerValue.EvidenceIds,
            answerValue.Warnings);

        return OperationResult<LogicalOrchestratorResult>.Ok(new LogicalOrchestratorResult(
            request.ScenarioId,
            FlowId,
            verdict,
            answerValue.Answer,
            flags,
            flowResult)
        {
            BranchResults = [flowResult],
            Warnings = answerValue.Warnings
        });
    }

    private WikiQueryTrace BuildTrace(
        ProjectWikiQuestionRequest request,
        ProjectWikiQueryPlan plan,
        ProjectWikiRetrievedContext context,
        ProjectWikiAnswer answer,
        ProjectWikiVerificationResult verification,
        Stopwatch stopwatch,
        TimeSpan cpuStart,
        long memoryStart,
        string status)
    {
        stopwatch.Stop();
        var cpuMs = (Process.GetCurrentProcess().TotalProcessorTime - cpuStart).TotalMilliseconds;
        var memoryEnd = GC.GetTotalMemory(forceFullCollection: false);
        var peakRamMb = Math.Max(memoryStart, memoryEnd) / 1024d / 1024d;
        var executedSteps = plan.RequiredQuerySteps
            .Select(step => step.StepId)
            .ToArray();
        var missingEvidence = string.IsNullOrWhiteSpace(answer.MissingEvidence)
            ? []
            : answer.MissingEvidence.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var plannerUnit = new WikiUnitTraceResult(
            ProjectWikiQueryPlanningFlow.FlowId,
            "v1",
            "deterministic-pretrain-surface",
            request.Question,
            $"{plan.Intent} / {plan.RequiredQuerySteps.Length} query steps",
            plan.AnswerReadinessScore,
            plan.AnswerReadinessScore >= 0.9 ? "PASS" : "CONCERN",
            plan.AnswerReadinessScore >= 0.9 ? "" : "query_plan_below_answer_readiness_gate",
            plan.AnswerReadinessScore >= 0.9 ? "" : "Add 50 ProjectWikiQueryPlanningFlow DPO rows for this intent.");
        var retrieverUnit = new WikiUnitTraceResult(
            nameof(WikiContextRetriever),
            "v1",
            "bm25-plus-optional-graphrag",
            $"{plan.RequiredQuerySteps.Length} planned steps",
            $"{context.EvidenceHits.Count} evidence hits / {context.Mode}",
            context.EvidenceHits.Count > 0 ? 1.0 : 0.0,
            context.EvidenceHits.Count > 0 ? "PASS" : "FAIL",
            context.EvidenceHits.Count > 0 ? "" : "no_evidence_retrieved",
            context.EvidenceHits.Count > 0 ? "" : "Improve evidence index or add focused query-planning rows.");
        var verifierUnit = new WikiUnitTraceResult(
            nameof(ProjectAnswerVerifier),
            "v1",
            "deterministic-verifier",
            string.Join(", ", answer.EvidenceIds),
            verification.IsPassed ? "citations accepted" : string.Join(", ", verification.Warnings),
            verification.CitationAccuracy,
            verification.IsPassed ? "PASS" : "CONCERN",
            verification.IsPassed ? "" : "answer_verification_failed",
            verification.IsPassed ? "" : "Add verifier negatives for invalid/missing citations.");

        return new WikiQueryTrace(
            $"wiki-query-trace-{Math.Abs(request.Question.GetHashCode(StringComparison.Ordinal)).ToString(CultureInfo.InvariantCulture)}",
            request.ProjectId,
            request.DocumentationVersion,
            plan.Intent.ToString(),
            GuessExpectedLevel(plan.Intent),
            true,
            plan.PlanId,
            plan.Intent.ToString(),
            plan.Entities,
            plan.RequiredQuerySteps.Select(step => $"{step.StepId}:{step.StepType}").ToArray(),
            executedSteps,
            [],
            missingEvidence,
            plan.PlanSufficiencyScore,
            plan.AnswerReadinessScore,
            [ProjectWikiQueryPlanningFlow.FlowId, nameof(WikiContextRetriever), nameof(ProjectLocalWikiModelUnit), nameof(ProjectAnswerVerifier)],
            [plannerUnit, retrieverUnit, verifierUnit],
            context.EvidenceHits.Select(hit => hit.Evidence.EvidenceId).ToArray(),
            answer.EvidenceIds,
            missingEvidence,
            answer.Confidence,
            verification.IsPassed ? "PASS" : "CONCERN",
            cpuMs,
            stopwatch.Elapsed.TotalMilliseconds,
            peakRamMb,
            status);
    }

    private string GuessExpectedLevel(ProjectWikiQueryIntent intent)
        => intent switch
        {
            ProjectWikiQueryIntent.FieldAddition or ProjectWikiQueryIntent.FieldRename or ProjectWikiQueryIntent.EntityImpact or ProjectWikiQueryIntent.RegressionRisk => "Level 3 developer change reasoning",
            ProjectWikiQueryIntent.ArchitectureBoundary => "Level 4 architectural reasoning",
            ProjectWikiQueryIntent.MissingEvidenceAbstention or ProjectWikiQueryIntent.WrongProjectRejection or ProjectWikiQueryIntent.StaleDocumentationRejection => "Level 5 evidence discipline",
            ProjectWikiQueryIntent.WorkflowTrace or ProjectWikiQueryIntent.ChangeLocation => "Level 2 local dependency and transit",
            _ => "Level 1 symbol understanding"
        };

    private ProjectWikiQuestionRequest BuildWikiRequest(LogicalOrchestratorRequest request)
    {
        var metadata = request.Metadata ?? new Dictionary<string, object>();
        return new ProjectWikiQuestionRequest(
            ReadString(metadata, "project_id", _projectId),
            request.UserRequest,
            ReadString(metadata, "wiki_domain_root", _wikiDomainRoot),
            ReadString(metadata, "documentation_version", ""),
            ReadInt(metadata, "top_k", 5),
            ReadBool(metadata, "allow_semantic", true));
    }

    private ProjectWikiUserQuestion BuildUserQuestion(ProjectWikiQuestionRequest request)
        => new(
            $"project-wiki-question-{request.ProjectId}-{Math.Abs(request.Question.GetHashCode(StringComparison.Ordinal)).ToString(CultureInfo.InvariantCulture)}",
            request.ProjectId,
            request.WikiDomainRoot,
            request.DocumentationVersion,
            request.Question,
            "",
            "",
            [request.ProjectId],
            []);

    private static string ReadString(IReadOnlyDictionary<string, object> metadata, string key, string fallback)
        => metadata.TryGetValue(key, out var value) && value is not null && !string.IsNullOrWhiteSpace(value.ToString())
            ? value.ToString()!
            : fallback;

    private static int ReadInt(IReadOnlyDictionary<string, object> metadata, string key, int fallback)
        => metadata.TryGetValue(key, out var value) &&
           int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) &&
           parsed > 0
            ? parsed
            : fallback;

    private static bool ReadBool(IReadOnlyDictionary<string, object> metadata, string key, bool fallback)
        => metadata.TryGetValue(key, out var value) &&
           bool.TryParse(value.ToString(), out var parsed)
            ? parsed
            : fallback;
}
