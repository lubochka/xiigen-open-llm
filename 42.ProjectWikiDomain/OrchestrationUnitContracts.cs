// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

// This file defines the six ILogicalUnit<TIn, TOut> interfaces that wrap the static
// decision components in TaskOrchestrationUnit and their aggregate input/output types.
// Governed by: TECHNICAL-DEBT-CLOSURE-PHASE-0.md (TD1).
// Design rule A-LM-03: Bootstrap implementations keep existing static logic —
// no behaviour change during transition; starting point for DPO training.

using AgentInfrastructure;         // OperationResult<T>
using AgentInfrastructure.Logical; // ILogicalUnit<TIn, TOut>, LogicalUnitRequest/Result

// ---------------------------------------------------------------------------
// Aggregate input types for units that need multiple inputs
// ---------------------------------------------------------------------------

// EvidencePlannerInput combines the intent and project understanding required by the evidence planner.
public sealed record EvidencePlannerInput(
    // Intent carries the parsed goal and work items extracted from the user prompt.
    TaskOrchestrationIntent Intent,
    // Project carries the stack and code evidence facts from the file-system scan.
    ProjectUnderstanding Project
);

// UnitSelectorInput combines the intent, project understanding, and evidence plan for unit selection.
public sealed record UnitSelectorInput(
    // Intent carries the parsed goal and work items extracted from the user prompt.
    TaskOrchestrationIntent Intent,
    // Project carries the stack and code evidence facts from the file-system scan.
    ProjectUnderstanding Project,
    // EvidencePlan carries the questions and gaps identified by the evidence planner.
    TaskEvidencePlan EvidencePlan
);

// RouteComposerInput bundles all five inputs required by the human route composer.
public sealed record RouteComposerInput(
    // Request is the original user task request with prompt and project roots.
    SubmitUserTaskRequest Request,
    // Intent carries the parsed goal and work items.
    TaskOrchestrationIntent Intent,
    // Project carries the stack and code evidence facts.
    ProjectUnderstanding Project,
    // EvidencePlan carries the questions and gaps.
    TaskEvidencePlan EvidencePlan,
    // Units carries the selected units and their reasons.
    TaskUnitSelection Units
);

// VerifierInput bundles the four inputs required by the orchestration verifier.
public sealed record VerifierInput(
    // Route is the composed human-facing answer route.
    HumanTaskRoute Route,
    // Intent carries the parsed goal and work items.
    TaskOrchestrationIntent Intent,
    // Project carries the stack and code evidence facts.
    ProjectUnderstanding Project,
    // EvidencePlan carries the questions and gaps.
    TaskEvidencePlan EvidencePlan
);

// VerifierResult is the typed output of the orchestration verifier.
// It replaces the raw bool from OrchestrationVerifier.Verify so the result
// can be carried inside LogicalUnitResult<TOutput>.
public sealed record VerifierResult(
    // Passes is true when the route has enough structure to proceed to execution.
    bool Passes,
    // Verdict is the human-readable reason string for this pass/fail outcome.
    string Verdict
);

// ---------------------------------------------------------------------------
// Unit interfaces — six new ILogicalUnit contracts for the orchestration pipeline
// ---------------------------------------------------------------------------

// IIntentBuilderUnit parses a raw user prompt string into a structured TaskOrchestrationIntent.
// Wraps the BuildIntent static method that used Regex splitting.
public interface IIntentBuilderUnit
    : ILogicalUnit<string, TaskOrchestrationIntent> { }

// IProjectScannerUnit scans the file system of the project roots in the request
// and produces a ProjectUnderstanding evidence record.
// Wraps ProjectUnderstandingUnit.UnderstandAsync.
public interface IProjectScannerUnit
    : ILogicalUnit<SubmitUserTaskRequest, ProjectUnderstanding> { }

// IEvidencePlannerUnit generates the evidence questions and gap list
// from the parsed intent and project understanding.
// Wraps EvidencePlanner.PlanEvidence.
public interface IEvidencePlannerUnit
    : ILogicalUnit<EvidencePlannerInput, TaskEvidencePlan> { }

// IUnitSelectorUnit selects the set of execution units required for this task
// based on intent, project context, and evidence gaps.
// Wraps UnitSelector.SelectUnits.
public interface IUnitSelectorUnit
    : ILogicalUnit<UnitSelectorInput, TaskUnitSelection> { }

// IRouteComposerUnit assembles the full human-facing answer route
// from all upstream pipeline results.
// Wraps HumanRouteComposer.Compose.
public interface IRouteComposerUnit
    : ILogicalUnit<RouteComposerInput, HumanTaskRoute> { }

// IOrchestrationVerifierUnit verifies that the route has sufficient structure
// to be returned to the user as an orchestrated response.
// Wraps OrchestrationVerifier.Verify; returns VerifierResult instead of bool.
public interface IOrchestrationVerifierUnit
    : ILogicalUnit<VerifierInput, VerifierResult> { }
