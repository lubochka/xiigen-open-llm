// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace OrchestratorModel.Logical;

// This flow wraps the six ILogicalUnit implementations from TD1 (42.ProjectWikiDomain)
// into the topology that ILogicalOrchestrator expects. It is the first "proper" path
// for project-wiki orchestration requests, replacing the direct SubmitUserTaskApi call.
// Governed by: TECHNICAL-DEBT-CLOSURE-PHASE-0.md (TD2).
// Flow id: project-wiki-orchestration-flow

using System.Globalization;
using AgentInfrastructure;         // OperationResult<T>
using AgentInfrastructure.Logical; // ILogicalFlow, LogicalFlowRequest, LogicalFlowResult, etc.
using ProjectWikiDomain;           // SubmitUserTaskApi, SubmitUserTaskRequest

// ProjectWikiOrchestrationFlow implements ILogicalFlow<string> where the string input
// is the user prompt. Project root paths are carried in request.Metadata.
public sealed class ProjectWikiOrchestrationFlow : ILogicalFlow<string>
{
    // FlowId is the stable identifier used to register this flow in the orchestrator registry.
    public const string FlowId = "project-wiki-orchestration-flow";

    // ScenarioId is the scenario routing key used by AdaptiveLogicalOrchestrator.RunExplicitScenarioAsync.
    public const string ScenarioId = "project-wiki-orchestration-flow";

    // _submitUserTaskApi runs the six Bootstrap units (intent, scan, evidence, select, compose, verify).
    private readonly SubmitUserTaskApi _submitUserTaskApi;

    // When called without arguments, the flow creates a default SubmitUserTaskApi that uses
    // all six Bootstrap units. When called with a custom api, the caller controls the unit implementations.
    public ProjectWikiOrchestrationFlow(SubmitUserTaskApi? submitUserTaskApi = null)
    {
        // Store the api — Bootstrap units are the default, trained units can replace them later.
        _submitUserTaskApi = submitUserTaskApi ?? new SubmitUserTaskApi();
    }

    // ExecuteAsync runs the six orchestration units in sequence and returns a LogicalFlowResult
    // whose FinalHumanResult contains the full human-facing answer route.
    public async Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
        LogicalFlowRequest<string> request,
        CancellationToken cancellationToken = default)
    {
        // Return a cancellation failure immediately if the token was already cancelled.
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalFlowResult>.Fail("Project-wiki orchestration flow cancelled.", "CANCELLED");

        // Require a non-null request — defensive guard at the flow boundary.
        if (request is null)
            return OperationResult<LogicalFlowResult>.Fail("Logical flow request is required.", "REQUEST_REQUIRED");

        // Reject requests for a different flow id to prevent misrouting.
        if (!string.Equals(request.FlowId, FlowId, StringComparison.Ordinal))
            return OperationResult<LogicalFlowResult>.Fail(
                $"Unsupported flow id '{request.FlowId}'. Expected '{FlowId}'.", "UNSUPPORTED_FLOW_ID");

        // Read the active project root from metadata — required for file-system scanning.
        var activeRepoPath = ReadString(request.Metadata, "active_repo_path", "");
        if (string.IsNullOrWhiteSpace(activeRepoPath))
        {
            // Return a flow result with a blocked verdict when the project root is missing.
            return OperationResult<LogicalFlowResult>.Ok(BuildMissingProjectRootResult(request));
        }

        // Read linked repository paths from metadata — optional for cross-stack reasoning.
        var linkedRepoPathsCsv = ReadString(request.Metadata, "linked_repo_paths", "");
        var linkedRepoPaths = string.IsNullOrWhiteSpace(linkedRepoPathsCsv)
            ? Array.Empty<string>()
            : linkedRepoPathsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // Read the user-facing entrypoint from metadata — defaults to "xiigen.orchestratePrompt".
        var entrypoint = ReadString(request.Metadata, "user_facing_entrypoint", "xiigen.orchestratePrompt");

        // Build the SubmitUserTaskRequest from the prompt string and project root metadata.
        var taskRequest = new SubmitUserTaskRequest(
            UserPrompt: request.Input,
            ActiveProjectRoot: activeRepoPath,
            LinkedProjectRoots: linkedRepoPaths,
            UserFacingEntrypoint: entrypoint ?? "xiigen.orchestratePrompt");

        // Run the six Bootstrap units through SubmitUserTaskApi.
        var taskResult = await _submitUserTaskApi.SubmitAsync(taskRequest, cancellationToken);

        // Return the unit-chain failure if any step failed.
        if (!taskResult.IsSuccess)
            return taskResult.MapError<LogicalFlowResult>();

        // Build a minimal LogicalFlowSummary from the orchestration result.
        var task = taskResult.Value!;
        var passed = task.Status == "ORCHESTRATED";
        var summary = new LogicalFlowSummary(
            SummaryId: $"{FlowId}-{request.RunId}",
            Tier: 2,
            BaselinePrimary: 0,
            EnhancedPrimary: passed ? 1.0 : 0.0,
            PrimaryMetricName: "orchestration_pass",
            LowerIsBetter: false,
            Passed: passed,
            ShouldScaleUp: false,
            NextAction: passed ? "return_to_plugin" : "request_more_evidence",
            ScalingSignal: 0,
            ScalingSignalNote: "",
            Notes: $"intent_work_items={task.Intent.RequestedWorkItems.Length}; evidence_questions={task.EvidencePlan.EvidenceQuestions.Length}",
            BaselineTimeMs: 0,
            EnhancedTimeMs: 0);

        // Build the metrics dictionary from the orchestration result fields.
        var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["status"]                  = task.Status,
            ["intent_goal_length"]      = task.Intent.Goal.Length.ToString(CultureInfo.InvariantCulture),
            ["work_items"]              = task.Intent.RequestedWorkItems.Length.ToString(CultureInfo.InvariantCulture),
            ["evidence_questions"]      = task.EvidencePlan.EvidenceQuestions.Length.ToString(CultureInfo.InvariantCulture),
            ["evidence_gaps"]           = task.EvidencePlan.EvidenceGaps.Length.ToString(CultureInfo.InvariantCulture),
            ["selected_units"]          = task.UnitSelection.Units.Length.ToString(CultureInfo.InvariantCulture),
            ["has_enough_structure"]    = task.HumanRoute.HasEnoughStructure ? "1" : "0",
            ["project_roots"]           = task.Project.ProjectRoots.Length.ToString(CultureInfo.InvariantCulture),
            ["stack_facts"]             = task.Project.StackFacts.Length.ToString(CultureInfo.InvariantCulture)
        };

        // Build the evidence ids from selected units for traceability.
        var evidenceIds = task.UnitSelection.Units
            .Select(unit => $"wiki-unit:{unit.ToLowerInvariant().Replace(" ", "-", StringComparison.Ordinal)}")
            .ToArray();

        // Build and return the LogicalFlowResult carrying the full human-route answer.
        var flowResult = new LogicalFlowResult(
            FlowId: FlowId,
            ScenarioId: ScenarioId,
            OverallVerdict: passed ? "ORCHESTRATED" : "NEEDS_MORE_EVIDENCE",
            FinalHumanResult: task.HumanRoute.UserAnswer,
            Summary: summary,
            Metrics: metrics,
            EvidenceIds: evidenceIds,
            Warnings: task.HumanRoute.Warnings);

        return OperationResult<LogicalFlowResult>.Ok(flowResult);
    }

    // BuildMissingProjectRootResult returns a blocked flow result when active_repo_path is absent.
    private static LogicalFlowResult BuildMissingProjectRootResult(LogicalFlowRequest<string> request)
    {
        // Build a minimal summary that marks the flow as failed due to missing project root.
        var summary = new LogicalFlowSummary(
            SummaryId: $"{FlowId}-{request.RunId}-missing-root",
            Tier: 2,
            BaselinePrimary: 0,
            EnhancedPrimary: 0,
            PrimaryMetricName: "orchestration_pass",
            LowerIsBetter: false,
            Passed: false,
            ShouldScaleUp: false,
            NextAction: "provide_active_repo_path_in_metadata",
            ScalingSignal: 0,
            ScalingSignalNote: "active_repo_path key missing from metadata",
            Notes: "project-wiki-orchestration-flow requires active_repo_path in request metadata",
            BaselineTimeMs: 0,
            EnhancedTimeMs: 0);

        // Return the blocked result with an explanatory human message.
        return new LogicalFlowResult(
            FlowId: FlowId,
            ScenarioId: ScenarioId,
            OverallVerdict: "BLOCKED_MISSING_PROJECT_ROOT",
            FinalHumanResult: "project-wiki-orchestration-flow: active_repo_path is required in metadata.",
            Summary: summary,
            Metrics: new Dictionary<string, string> { ["status"] = "BLOCKED_MISSING_PROJECT_ROOT" },
            EvidenceIds: [],
            Warnings: ["active_repo_path missing from metadata"]);
    }

    // ReadString reads a string value from the metadata dictionary, falling back to the default.
    private static string? ReadString(
        IReadOnlyDictionary<string, object> metadata,
        string key,
        string? defaultValue)
    {
        // Return the default when the key is absent from metadata.
        if (!metadata.TryGetValue(key, out var value))
            return defaultValue;

        // Convert the value to a string — metadata values can be any object type.
        return value?.ToString() ?? defaultValue;
    }
}
