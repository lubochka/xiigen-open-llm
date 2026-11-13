// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Diagnostics;
using AgentInfrastructure;         // OperationResult<T>
using AgentInfrastructure.Logical; // ILogicalOrchestrator, LogicalOrchestratorRequest, LogicalExecutionFlags

public sealed record ProjectWikiPluginOrchestrationRequest(
    string PromptText,
    string ActiveRepoPath,
    string[] LinkedRepoPaths,
    string RepoScopeClass,
    string[] ExpectedStack,
    string RequestId,
    string SessionId,
    bool AllowTraining,
    bool AllowProjectLocalWikiUpdate,
    string PluginHost,
    string UserFacingEntrypoint);

public sealed record ProjectWikiPluginOrchestrationMetrics(
    long CpuMs,
    long RamMb,
    long ElapsedMs);

public sealed record ProjectWikiPluginOrchestrationResult(
    string Status,
    string[] OrchestrationPlan,
    string[] SelectedUnits,
    string[] WikiQueryPlan,
    string[] EvidenceRequirements,
    string[] MissingEvidence,
    string BlockedReason,
    string AnswerPreviewOrPlanSummary,
    string StateUpdate,
    string RepoScopeClass,
    string ActiveRepoPath,
    string[] LinkedRepoPaths,
    string UserFacingEntrypoint,
    ProjectWikiPluginOrchestrationMetrics Metrics);

// ProjectWikiOrchestrationFlowId is the flow scenario id for the orchestrator path.
// Defined here as a constant so the adapter does not need to reference OrchestratorModel.
internal static class ProjectWikiOrchestrationFlowConstants
{
    // FlowId is the registered scenario used by ILogicalOrchestrator.RunAsync.
    public const string ScenarioId = "project-wiki-orchestration-flow";
}

public sealed class ProjectWikiPluginOrchestrationAdapter
{
    private readonly ProjectWikiQueryPlanningFlow _queryPlanner;
    // _submitUserTaskApi is the BOOTSTRAP_FALLBACK path when no orchestrator is provided.
    private readonly SubmitUserTaskApi _submitUserTaskApi;
    // _orchestrator is optional — when provided, the adapter uses ILogicalOrchestrator.RunAsync.
    private readonly ILogicalOrchestrator? _orchestrator;

    // When called without arguments, the adapter uses the BOOTSTRAP_FALLBACK path (direct SubmitUserTaskApi).
    // When called with an ILogicalOrchestrator, the adapter routes through project-wiki-orchestration-flow.
    public ProjectWikiPluginOrchestrationAdapter(
        ProjectWikiQueryPlanningFlow? queryPlanner = null,
        SubmitUserTaskApi? submitUserTaskApi = null,
        ILogicalOrchestrator? orchestrator = null)
    {
        // Store the query planner for wiki-specific query planning.
        _queryPlanner = queryPlanner ?? new ProjectWikiQueryPlanningFlow();
        // Store the fallback api — used when orchestrator is null.
        _submitUserTaskApi = submitUserTaskApi ?? new SubmitUserTaskApi();
        // Store the orchestrator — null means BOOTSTRAP_FALLBACK active.
        _orchestrator = orchestrator;
    }

    public async Task<OperationResult<ProjectWikiPluginOrchestrationResult>> OrchestrateAsync(
        ProjectWikiPluginOrchestrationRequest request,
        CancellationToken ct = default)
    {
        var started = Stopwatch.StartNew();
        if (ct.IsCancellationRequested)
            return OperationResult<ProjectWikiPluginOrchestrationResult>.Fail("Plugin orchestration cancelled.", "PLUGIN_ORCHESTRATION_CANCELLED");

        if (request is null)
            return OperationResult<ProjectWikiPluginOrchestrationResult>.Fail("Plugin orchestration request is required.", "PLUGIN_ORCHESTRATION_REQUEST_REQUIRED");

        if (string.IsNullOrWhiteSpace(request.PromptText))
            return OperationResult<ProjectWikiPluginOrchestrationResult>.Fail("Prompt text is required.", "PLUGIN_ORCHESTRATION_PROMPT_REQUIRED");

        if (string.IsNullOrWhiteSpace(request.ActiveRepoPath))
            return OperationResult<ProjectWikiPluginOrchestrationResult>.Fail("Active repository path is required.", "PLUGIN_ORCHESTRATION_REPO_REQUIRED");

        var projectRoots = BuildProjectRoots(request);
        var question = new ProjectWikiUserQuestion(
            string.IsNullOrWhiteSpace(request.RequestId) ? $"plugin-{Guid.NewGuid():N}" : request.RequestId,
            Path.GetFileName(request.ActiveRepoPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)),
            request.ActiveRepoPath,
            "plugin-runtime",
            request.PromptText,
            string.Join("+", request.ExpectedStack ?? []),
            KnownProjectAliases: projectRoots.Select(Path.GetFileName).Where(name => !string.IsNullOrWhiteSpace(name)).ToArray()!);
        var planned = await _queryPlanner.PlanAsync(question, ct);
        if (!planned.IsSuccess)
            return planned.MapError<ProjectWikiPluginOrchestrationResult>();

        var plan = planned.Value!;
        var missingEvidence = FindMissingEvidence(projectRoots);
        var selectedUnits = SelectUnits(request, plan, missingEvidence);
        var orchestrationPlan = BuildOrchestrationPlan(request, plan, missingEvidence);
        // Choose the orchestrator path or the BOOTSTRAP_FALLBACK depending on configuration.
        string humanAnswer;
        string[] taskEvidenceQuestions;
        bool hasEnoughStructure;

        if (_orchestrator is not null)
        {
            // Orchestrator path: call ILogicalOrchestrator.RunAsync with project-wiki-orchestration-flow.
            // Project root paths are passed through Metadata so the flow can build SubmitUserTaskRequest.
            var linkedPathsCsv = string.Join(",", request.LinkedRepoPaths ?? []);
            var orchestratorRequest = new LogicalOrchestratorRequest(
                RunId: $"wiki-plugin-{Guid.NewGuid():N}",
                ScenarioId: ProjectWikiOrchestrationFlowConstants.ScenarioId,
                UserRequest: request.PromptText,
                Flags: new LogicalExecutionFlags(),
                Metadata: new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    // active_repo_path carries the primary repository root for file-system scanning.
                    ["active_repo_path"]        = request.ActiveRepoPath,
                    // linked_repo_paths carries a comma-separated list of additional repository roots.
                    ["linked_repo_paths"]       = linkedPathsCsv,
                    // user_facing_entrypoint identifies which IDE command launched the orchestration.
                    ["user_facing_entrypoint"]  = request.UserFacingEntrypoint ?? "xiigen.orchestratePrompt"
                });

            // Call ILogicalOrchestrator.RunAsync with the project-wiki-orchestration-flow request.
            var orchestratorResult = await _orchestrator.RunAsync(orchestratorRequest, ct);
            if (!orchestratorResult.IsSuccess)
                return orchestratorResult.MapError<ProjectWikiPluginOrchestrationResult>();

            // Extract the human answer and evidence data from the orchestrator result.
            humanAnswer           = orchestratorResult.Value!.FinalHumanResult;
            taskEvidenceQuestions = [];  // evidence questions available through the flow's Metrics dict
            hasEnoughStructure    = orchestratorResult.Value.OverallVerdict == "ORCHESTRATED";
        }
        else
        {
            // BOOTSTRAP_FALLBACK: flow not registered — falling back to direct SubmitUserTaskApi.
            // This path should not be active after TD2 completes. Log a warning to track usage.
            // BOOTSTRAP_FALLBACK active for project-wiki-orchestration-flow.
            // This indicates the orchestrator is not configured in the adapter.
            var taskRoute = await _submitUserTaskApi.SubmitAsync(
                new SubmitUserTaskRequest(
                    request.PromptText,
                    request.ActiveRepoPath,
                    request.LinkedRepoPaths ?? [],
                    request.UserFacingEntrypoint),
                ct);
            if (!taskRoute.IsSuccess)
                return taskRoute.MapError<ProjectWikiPluginOrchestrationResult>();

            // Extract the human answer and evidence data from the Bootstrap API result.
            humanAnswer           = taskRoute.Value!.HumanRoute.UserAnswer;
            taskEvidenceQuestions = taskRoute.Value.EvidencePlan.EvidenceQuestions;
            hasEnoughStructure    = taskRoute.Value.HumanRoute.HasEnoughStructure;
        }

        var evidenceRequirements = plan.RequiredEvidenceTypes
            .Concat(plan.SufficiencyChecks)
            .Concat(taskEvidenceQuestions)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var status = missingEvidence.Length == 0 && hasEnoughStructure
            ? "ORCHESTRATED"
            : "BLOCKED_WITH_GAPS";
        var blockedReason = missingEvidence.Length == 0
            ? ""
            : "Project-local wiki evidence is missing for at least one repository; return blocked state instead of guessing.";
        started.Stop();

        var result = new ProjectWikiPluginOrchestrationResult(
            status,
            orchestrationPlan,
            selectedUnits,
            plan.RequiredQuerySteps.Select(step => step.QueryText).ToArray(),
            evidenceRequirements,
            missingEvidence,
            blockedReason,
            humanAnswer,
            missingEvidence.Length == 0 ? "plugin_orchestration_ready" : "plugin_orchestration_waiting_for_project_evidence",
            NormalizeRepoScope(request, projectRoots),
            request.ActiveRepoPath,
            request.LinkedRepoPaths ?? [],
            request.UserFacingEntrypoint,
            new ProjectWikiPluginOrchestrationMetrics(
                CpuMs: 0,
                RamMb: GC.GetTotalMemory(false) / 1024 / 1024,
                ElapsedMs: started.ElapsedMilliseconds));

        return OperationResult<ProjectWikiPluginOrchestrationResult>.Ok(result);
    }

    private static string[] BuildProjectRoots(ProjectWikiPluginOrchestrationRequest request)
        => new[] { request.ActiveRepoPath }
            .Concat(request.LinkedRepoPaths ?? [])
            .Where(root => !string.IsNullOrWhiteSpace(root))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static string NormalizeRepoScope(ProjectWikiPluginOrchestrationRequest request, string[] projectRoots)
    {
        if (!string.IsNullOrWhiteSpace(request.RepoScopeClass) && request.RepoScopeClass != "active_repository")
            return request.RepoScopeClass;

        return projectRoots.Length > 1 ? "linked_repository_group" : "active_repository";
    }

    private static string[] FindMissingEvidence(string[] projectRoots)
    {
        var missing = new List<string>();
        foreach (var root in projectRoots)
        {
            var evidenceRoot = Path.Combine(root, ".xiigen", "wiki-domain", "evidence");
            var centralEvidenceRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "xiigen",
                "llm mvp",
                "localwikis",
                Path.GetFileName(root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)),
                "evidence");
            if (!Directory.Exists(evidenceRoot) && !Directory.Exists(centralEvidenceRoot))
                missing.Add($"missing project-local wiki evidence: {evidenceRoot} or {centralEvidenceRoot}");
        }

        return missing.ToArray();
    }

    private static string[] SelectUnits(
        ProjectWikiPluginOrchestrationRequest request,
        ProjectWikiQueryPlan plan,
        string[] missingEvidence)
    {
        var units = new List<string>
        {
            "ProjectWikiRequestRouterUnit",
            "StackAndVersionClassifierUnit",
            "SystemBoundaryMapperUnit",
            "ProjectWikiQueryPlanningFlow"
        };
        if (request.LinkedRepoPaths is { Length: > 0 } || plan.RequiredQuerySteps.Any(step => step.StepType == ProjectWikiQueryStepType.CrossRepositoryBoundaryLookup))
            units.Add("CrossStackBusinessFlowGraphUnit");
        units.Add("WikiContextRoutingUnit");
        units.Add("ProjectWikiAnswerComposer");
        units.Add("ProjectAnswerVerifier");
        units.Add("WikiTraceAggregator");
        if (missingEvidence.Length > 0)
            units.Add("ProjectWikiTrainingOrchestrator");

        return units.Distinct(StringComparer.Ordinal).ToArray();
    }

    private static string[] BuildOrchestrationPlan(
        ProjectWikiPluginOrchestrationRequest request,
        ProjectWikiQueryPlan plan,
        string[] missingEvidence)
    {
        var phases = new List<string>
        {
            "accept_plugin_prompt",
            "resolve_active_and_linked_repositories",
            "classify_stack_and_repo_scope",
            "build_project_wiki_query_plan",
            "select_trainable_wiki_units",
            "check_project_local_evidence",
            missingEvidence.Length == 0 ? "compose_evidence_grounded_answer" : "return_missing_evidence_blocked_state",
            "record_plugin_orchestration_trace"
        };
        if (request.AllowTraining && missingEvidence.Length > 0)
            phases.Add("route_missing_evidence_to_project_wiki_training_orchestrator");
        if (plan.Intent is ProjectWikiQueryIntent.StoredProcedureImpact)
            phases.Insert(4, "include_sql_stored_procedure_trace");

        return phases.ToArray();
    }

    private static string BuildSummary(
        ProjectWikiPluginOrchestrationRequest request,
        ProjectWikiQueryPlan plan,
        string[] missingEvidence)
    {
        var queryCount = plan.RequiredQuerySteps.Length;
        var baseSummary = $"XIIGen planned {queryCount} wiki queries for plugin entrypoint {request.UserFacingEntrypoint}; intent={plan.Intent}; repo_scope={request.RepoScopeClass}.";
        if (missingEvidence.Length == 0)
            return $"{baseSummary} Evidence is present; answer composition may proceed.";

        return $"{baseSummary} Evidence is missing, so the plugin must show blocked state rather than a guessed answer.";
    }
}
