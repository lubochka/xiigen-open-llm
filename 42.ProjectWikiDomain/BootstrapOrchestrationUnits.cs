// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

// This file contains the six Bootstrap implementations of the orchestration unit interfaces.
// Each Bootstrap class contains the exact logic that previously lived as hardcoded C# inside
// TaskOrchestrationUnit or its helper classes. No behaviour changes — these are the starting
// point for DPO (Direct Preference Optimization) training, not the final trained state.
// Governed by: TECHNICAL-DEBT-CLOSURE-PHASE-0.md (TD1), design rule A-LM-03.

using System.Text.RegularExpressions;
using AgentInfrastructure;         // OperationResult<T>
using AgentInfrastructure.Logical; // ILogicalUnit, LogicalUnitRequest, LogicalUnitResult

// ---------------------------------------------------------------------------
// Bootstrap 1 — IntentBuilder
// ---------------------------------------------------------------------------

// BootstrapIntentBuilderUnit parses a user prompt using the same Regex.Split
// logic that previously lived in TaskOrchestrationUnit.BuildIntent.
public sealed class BootstrapIntentBuilderUnit : IIntentBuilderUnit
{
    // ExecuteAsync parses the prompt in the request input and returns a TaskOrchestrationIntent.
    public Task<OperationResult<LogicalUnitResult<TaskOrchestrationIntent>>> ExecuteAsync(
        LogicalUnitRequest<string> request,
        CancellationToken cancellationToken = default)
    {
        // Return a cancellation failure immediately if the token was already cancelled.
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(
                OperationResult<LogicalUnitResult<TaskOrchestrationIntent>>.Fail(
                    "Intent building cancelled.", "INTENT_BUILDER_CANCELLED"));

        // Build the intent using the same Regex logic as the original BuildIntent method.
        var intent = BuildIntent(request.Input);

        // Wrap the result in the LogicalUnitResult envelope required by ILogicalUnit.
        var result = new LogicalUnitResult<TaskOrchestrationIntent>(
            UnitId: request.UnitId,
            UnitRole: "intent-builder",
            DomainTag: request.DomainTag,
            Output: intent,
            Confidence: 1.0, // Bootstrap units are deterministic — confidence is always 1.
            PredicateIds: [],
            DomainTags: [request.DomainTag],
            EvidenceIds: [],
            Warnings: []);

        return Task.FromResult(OperationResult<LogicalUnitResult<TaskOrchestrationIntent>>.Ok(result));
    }

    // BuildIntent replicates the original static BuildIntent logic: normalise whitespace,
    // split on comma/and/then/->, extract PascalCase object names.
    private static TaskOrchestrationIntent BuildIntent(string prompt)
    {
        // Normalise the prompt by collapsing all whitespace runs into single spaces.
        var normalized = Regex.Replace(prompt.Trim(), @"\s+", " ");

        // Extract work items using the same comma/keyword/arrow split as the original code.
        var workItems = ExtractWorkItems(normalized);

        // Extract PascalCase identifiers as user-named objects (same Regex as original).
        var objects = Regex.Matches(normalized, @"\b[A-Z][A-Za-z0-9./_-]{2,}\b")
            .Select(match => match.Value)
            .Distinct(StringComparer.Ordinal)
            .Take(12)
            .ToArray();

        // Truncate the goal to 240 characters to match the original cap.
        var goal = normalized.Length <= 240 ? normalized : normalized[..240] + "...";

        return new TaskOrchestrationIntent(goal, workItems, objects);
    }

    // ExtractWorkItems replicates the original static ExtractWorkItems logic.
    private static string[] ExtractWorkItems(string prompt)
    {
        // Replace colon and semicolon with comma to normalise separators.
        var text = prompt
            .Replace(":", ",", StringComparison.Ordinal)
            .Replace(";", ",", StringComparison.Ordinal);

        // Split on comma, 'and', 'then', and '->' to produce work item candidates.
        var fragments = Regex.Split(text, @",|\band\b|\bthen\b|->", RegexOptions.IgnoreCase)
            .Select(fragment => fragment.Trim(' ', '.', '-', '\t', '\r', '\n'))
            .Where(fragment => fragment.Length > 4)
            .Where(fragment => !fragment.StartsWith("User wants", StringComparison.OrdinalIgnoreCase))
            .Where(fragment => !fragment.StartsWith("User enters", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(14)
            .ToArray();

        // Fall back to the full prompt if no fragments were extracted.
        return fragments.Length == 0 ? [prompt] : fragments;
    }
}

// ---------------------------------------------------------------------------
// Bootstrap 2 — ProjectScanner
// ---------------------------------------------------------------------------

// BootstrapProjectScannerUnit scans the file system of the project roots and
// produces a ProjectUnderstanding record using the same folder/extension checks
// as the original ProjectUnderstandingUnit.UnderstandAsync.
public sealed class BootstrapProjectScannerUnit : IProjectScannerUnit
{
    // ExecuteAsync delegates to the existing ProjectUnderstandingUnit to avoid
    // duplicating the file-system scan logic. The existing unit is already correct.
    public async Task<OperationResult<LogicalUnitResult<ProjectUnderstanding>>> ExecuteAsync(
        LogicalUnitRequest<SubmitUserTaskRequest> request,
        CancellationToken cancellationToken = default)
    {
        // Return a cancellation failure immediately if the token was already cancelled.
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalUnitResult<ProjectUnderstanding>>.Fail(
                "Project scanning cancelled.", "PROJECT_SCANNER_CANCELLED");

        // Delegate to the existing unit which contains all file-system evidence logic.
        var existingUnit = new ProjectUnderstandingUnit();
        var scanResult = await existingUnit.UnderstandAsync(request.Input, cancellationToken);
        if (!scanResult.IsSuccess)
            return scanResult.MapError<LogicalUnitResult<ProjectUnderstanding>>();

        // Wrap the result in the LogicalUnitResult envelope required by ILogicalUnit.
        var result = new LogicalUnitResult<ProjectUnderstanding>(
            UnitId: request.UnitId,
            UnitRole: "project-scanner",
            DomainTag: request.DomainTag,
            Output: scanResult.Value!,
            Confidence: 1.0, // Bootstrap scan is deterministic — confidence is always 1.
            PredicateIds: [],
            DomainTags: [request.DomainTag],
            EvidenceIds: [],
            Warnings: []);

        return OperationResult<LogicalUnitResult<ProjectUnderstanding>>.Ok(result);
    }
}

// ---------------------------------------------------------------------------
// Bootstrap 3 — EvidencePlanner
// ---------------------------------------------------------------------------

// BootstrapEvidencePlannerUnit generates the evidence questions and gap list using
// the same fixed-question list and keyword matching as the original EvidencePlanner.
public sealed class BootstrapEvidencePlannerUnit : IEvidencePlannerUnit
{
    // ExecuteAsync delegates to the existing EvidencePlanner to avoid duplicating logic.
    public Task<OperationResult<LogicalUnitResult<TaskEvidencePlan>>> ExecuteAsync(
        LogicalUnitRequest<EvidencePlannerInput> request,
        CancellationToken cancellationToken = default)
    {
        // Return a cancellation failure immediately if the token was already cancelled.
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(
                OperationResult<LogicalUnitResult<TaskEvidencePlan>>.Fail(
                    "Evidence planning cancelled.", "EVIDENCE_PLANNER_CANCELLED"));

        // Delegate to the existing EvidencePlanner with the input's intent and project.
        var existingPlanner = new EvidencePlanner();
        var plan = existingPlanner.PlanEvidence(request.Input.Intent, request.Input.Project);

        // Wrap the result in the LogicalUnitResult envelope required by ILogicalUnit.
        var result = new LogicalUnitResult<TaskEvidencePlan>(
            UnitId: request.UnitId,
            UnitRole: "evidence-planner",
            DomainTag: request.DomainTag,
            Output: plan,
            Confidence: 1.0, // Bootstrap planner is deterministic — confidence is always 1.
            PredicateIds: [],
            DomainTags: [request.DomainTag],
            EvidenceIds: [],
            Warnings: []);

        return Task.FromResult(OperationResult<LogicalUnitResult<TaskEvidencePlan>>.Ok(result));
    }
}

// ---------------------------------------------------------------------------
// Bootstrap 4 — UnitSelector
// ---------------------------------------------------------------------------

// BootstrapUnitSelectorUnit selects execution units using the same if-chain logic
// as the original UnitSelector.SelectUnits.
public sealed class BootstrapUnitSelectorUnit : IUnitSelectorUnit
{
    // ExecuteAsync delegates to the existing UnitSelector to avoid duplicating the if-chain.
    public Task<OperationResult<LogicalUnitResult<TaskUnitSelection>>> ExecuteAsync(
        LogicalUnitRequest<UnitSelectorInput> request,
        CancellationToken cancellationToken = default)
    {
        // Return a cancellation failure immediately if the token was already cancelled.
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(
                OperationResult<LogicalUnitResult<TaskUnitSelection>>.Fail(
                    "Unit selection cancelled.", "UNIT_SELECTOR_CANCELLED"));

        // Delegate to the existing UnitSelector with the three inputs from the aggregate.
        var existingSelector = new UnitSelector();
        var selection = existingSelector.SelectUnits(
            request.Input.Intent,
            request.Input.Project,
            request.Input.EvidencePlan);

        // Wrap the result in the LogicalUnitResult envelope required by ILogicalUnit.
        var result = new LogicalUnitResult<TaskUnitSelection>(
            UnitId: request.UnitId,
            UnitRole: "unit-selector",
            DomainTag: request.DomainTag,
            Output: selection,
            Confidence: 1.0, // Bootstrap selector is deterministic — confidence is always 1.
            PredicateIds: [],
            DomainTags: [request.DomainTag],
            EvidenceIds: [],
            Warnings: []);

        return Task.FromResult(OperationResult<LogicalUnitResult<TaskUnitSelection>>.Ok(result));
    }
}

// ---------------------------------------------------------------------------
// Bootstrap 5 — RouteComposer
// ---------------------------------------------------------------------------

// BootstrapRouteComposerUnit builds the human-facing answer string using the same
// StringBuilder template logic as the original HumanRouteComposer.Compose.
public sealed class BootstrapRouteComposerUnit : IRouteComposerUnit
{
    // ExecuteAsync delegates to the existing HumanRouteComposer to avoid duplicating templates.
    public Task<OperationResult<LogicalUnitResult<HumanTaskRoute>>> ExecuteAsync(
        LogicalUnitRequest<RouteComposerInput> request,
        CancellationToken cancellationToken = default)
    {
        // Return a cancellation failure immediately if the token was already cancelled.
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(
                OperationResult<LogicalUnitResult<HumanTaskRoute>>.Fail(
                    "Route composing cancelled.", "ROUTE_COMPOSER_CANCELLED"));

        // Delegate to the existing HumanRouteComposer with the five inputs from the aggregate.
        var existingComposer = new HumanRouteComposer();
        var route = existingComposer.Compose(
            request.Input.Request,
            request.Input.Intent,
            request.Input.Project,
            request.Input.EvidencePlan,
            request.Input.Units);

        // Wrap the result in the LogicalUnitResult envelope required by ILogicalUnit.
        var result = new LogicalUnitResult<HumanTaskRoute>(
            UnitId: request.UnitId,
            UnitRole: "route-composer",
            DomainTag: request.DomainTag,
            Output: route,
            Confidence: 1.0, // Bootstrap composer is deterministic — confidence is always 1.
            PredicateIds: [],
            DomainTags: [request.DomainTag],
            EvidenceIds: [],
            Warnings: []);

        return Task.FromResult(OperationResult<LogicalUnitResult<HumanTaskRoute>>.Ok(result));
    }
}

// ---------------------------------------------------------------------------
// Bootstrap 6 — OrchestrationVerifier
// ---------------------------------------------------------------------------

// BootstrapOrchestrationVerifierUnit verifies the route structure using the same
// non-empty string check logic as the original OrchestrationVerifier.Verify.
public sealed class BootstrapOrchestrationVerifierUnit : IOrchestrationVerifierUnit
{
    // ExecuteAsync delegates to the existing OrchestrationVerifier to avoid duplicating checks.
    public Task<OperationResult<LogicalUnitResult<VerifierResult>>> ExecuteAsync(
        LogicalUnitRequest<VerifierInput> request,
        CancellationToken cancellationToken = default)
    {
        // Return a cancellation failure immediately if the token was already cancelled.
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(
                OperationResult<LogicalUnitResult<VerifierResult>>.Fail(
                    "Orchestration verification cancelled.", "VERIFIER_CANCELLED"));

        // Delegate to the existing OrchestrationVerifier with the four inputs from the aggregate.
        var existingVerifier = new OrchestrationVerifier();
        var passes = existingVerifier.Verify(
            request.Input.Route,
            request.Input.Intent,
            request.Input.Project,
            request.Input.EvidencePlan);

        // Build a typed VerifierResult instead of the raw bool the original returned.
        var verifierResult = new VerifierResult(
            Passes: passes,
            Verdict: passes ? "ORCHESTRATED" : "NEEDS_MORE_EVIDENCE");

        // Wrap the result in the LogicalUnitResult envelope required by ILogicalUnit.
        var result = new LogicalUnitResult<VerifierResult>(
            UnitId: request.UnitId,
            UnitRole: "orchestration-verifier",
            DomainTag: request.DomainTag,
            Output: verifierResult,
            Confidence: 1.0, // Bootstrap verifier is deterministic — confidence is always 1.
            PredicateIds: [],
            DomainTags: [request.DomainTag],
            EvidenceIds: [],
            Warnings: []);

        return Task.FromResult(OperationResult<LogicalUnitResult<VerifierResult>>.Ok(result));
    }
}
