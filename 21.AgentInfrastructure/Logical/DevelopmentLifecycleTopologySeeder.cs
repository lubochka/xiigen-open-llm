// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

// This record reports what the lifecycle topology seeder installed.
public sealed record DevelopmentLifecycleSeedResult(
    int DomainCount,
    int PhaseCount,
    int SubPhaseCount,
    int TransitionCount,
    int RegisteredFlowCount,
    string[] RegisteredSubPhaseIds);

// This seeder installs the canonical whole-development-area topology and one flow per sub-phase.
public static class DevelopmentLifecycleTopologySeeder
{
    // The canonical software-development domain id.
    public const string DomainId = "software-development";

    // The canonical source id for lifecycle topology bootstrap entries.
    public const string SourceId = "whole-development-area-topology-v1";

    private static readonly Dictionary<string, UniversalNineNodeDevelopmentFlow> FlowCache = new(StringComparer.Ordinal);
    private static readonly object FlowCacheLock = new();

    // Seed the topology registry and flow registry with the complete development lifecycle.
    public static OperationResult<DevelopmentLifecycleSeedResult> Seed(
        ILogicalTopologyRegistry topology,
        InMemoryLogicalFlowRegistry flows,
        string version = "1.0.0")
    {
        // Reject missing topology storage through the shared result envelope.
        if (topology is null)
            return OperationResult<DevelopmentLifecycleSeedResult>.Fail("Topology registry is required.", "TOPOLOGY_REGISTRY_REQUIRED");

        // Reject missing flow storage through the shared result envelope.
        if (flows is null)
            return OperationResult<DevelopmentLifecycleSeedResult>.Fail("Flow registry is required.", "FLOW_REGISTRY_REQUIRED");

        // Register the development domain.
        var domain = topology.AddOrUpdateDomain(new LogicalDomain(
            DomainId,
            "Software Development",
            "Owns whole-development-area orchestration across UI/UX, functional specification, technical design, implementation, testing, QA, and QA automation.",
            ["DEVELOPMENT"],
            [SourceId]));
        if (!domain.IsSuccess)
            return OperationResult<DevelopmentLifecycleSeedResult>.Fail(domain.ErrorMessage, domain.ErrorCode);

        // Register phases.
        foreach (var phase in PhaseDefinitions())
        {
            var registeredPhase = topology.AddOrUpdatePhase(new LogicalPhase(
                phase.PhaseId,
                phase.Name,
                phase.Purpose,
                [DomainId],
                phase.PredicateIds,
                [SourceId]));
            if (!registeredPhase.IsSuccess)
                return OperationResult<DevelopmentLifecycleSeedResult>.Fail(registeredPhase.ErrorMessage, registeredPhase.ErrorCode);
        }

        // Register sub-phases and generic flows.
        var registeredSubPhaseIds = new List<string>();
        var registeredFlowCount = 0;
        foreach (var subPhase in SubPhaseDefinitions())
        {
            var registeredSubPhase = topology.AddOrUpdateSubPhase(new LogicalSubPhase(
                subPhase.SubPhaseId,
                subPhase.PhaseId,
                subPhase.Name,
                subPhase.Purpose,
                [DomainId],
                subPhase.PredicateIds,
                [SourceId]));
            if (!registeredSubPhase.IsSuccess)
                return OperationResult<DevelopmentLifecycleSeedResult>.Fail(registeredSubPhase.ErrorMessage, registeredSubPhase.ErrorCode);

            registeredSubPhaseIds.Add(registeredSubPhase.Value!.SubPhase.SubPhaseId);

            // Preserve any specific flow already registered for this sub-phase.
            var hasSpecificFlow = flows.ListDescriptors().Any(descriptor =>
                string.Equals(descriptor.SubPhaseId, subPhase.SubPhaseId, StringComparison.Ordinal));
            if (hasSpecificFlow)
                continue;

            // Register one universal nine-node flow so every sub-phase is executable immediately.
            var flowId = $"{UniversalNineNodeDevelopmentFlow.FlowIdPrefix}-{subPhase.SubPhaseId}";
            var scenarioId = $"{UniversalNineNodeDevelopmentFlow.ScenarioIdPrefix}-{subPhase.SubPhaseId}";
            var registration = flows.Register(
                flowId,
                scenarioId,
                version,
                subPhase.PhaseId,
                subPhase.SubPhaseId,
                subPhase.TaskType,
                [DomainId],
                GetOrCreateFlow(subPhase));
            if (!registration.IsSuccess)
                return OperationResult<DevelopmentLifecycleSeedResult>.Fail(registration.ErrorMessage, registration.ErrorCode);

            registeredFlowCount++;
        }

        // Register phase transitions and sub-phase transitions.
        var transitionCount = 0;
        foreach (var transition in TransitionDefinitions())
        {
            var registeredTransition = topology.AddOrUpdateTransition(transition);
            if (!registeredTransition.IsSuccess)
                return OperationResult<DevelopmentLifecycleSeedResult>.Fail(registeredTransition.ErrorMessage, registeredTransition.ErrorCode);

            transitionCount++;
        }

        // Return seed metrics.
        var result = new DevelopmentLifecycleSeedResult(
            DomainCount: 1,
            PhaseCount: PhaseDefinitions().Length,
            SubPhaseCount: SubPhaseDefinitions().Length,
            TransitionCount: transitionCount,
            RegisteredFlowCount: registeredFlowCount,
            RegisteredSubPhaseIds: registeredSubPhaseIds.ToArray());

        return OperationResult<DevelopmentLifecycleSeedResult>.Ok(result);
    }

    private static UniversalNineNodeDevelopmentFlow GetOrCreateFlow(LifecycleSubPhaseDefinition subPhase)
    {
        lock (FlowCacheLock)
        {
            if (FlowCache.TryGetValue(subPhase.SubPhaseId, out var existing))
                return existing;

            var created = new UniversalNineNodeDevelopmentFlow(
                subPhase.PhaseId,
                subPhase.SubPhaseId,
                subPhase.TaskType,
                [DomainId],
                subPhase.PredicateIds);
            FlowCache[subPhase.SubPhaseId] = created;
            return created;
        }
    }

    // Return the seven canonical development phases.
    public static LifecyclePhaseDefinition[] PhaseDefinitions()
    {
        return
        [
            new("1", "UI/UX Definition", "Owns visual language, interaction shape, accessibility intent, and user experience constraints.", ["UI"]),
            new("2", "Functional Specification", "Owns business capability, acceptance criteria, data rules, and behavior boundaries.", ["DOCUMENT"]),
            new("3", "Technical Design", "Owns architecture, contracts, security model, data model, and operational design.", ["ARCHITECTURE"]),
            new("4", "Implementation", "Owns executable code, client and backend integration, infrastructure code, and refactoring.", ["IMPLEMENT"]),
            new("5", "Testing", "Owns unit, integration, end-to-end, regression, and compatibility testing.", ["TEST"]),
            new("6", "QA", "Owns acceptance review, release risk, security/compliance review, and reliability validation.", ["QA"]),
            new("7", "QA Automation", "Owns automated harnesses, CI gates, monitoring feedback, reporting, and learning-loop evidence.", ["AUTOMATION"])
        ];
    }

    // Return the canonical 31 sub-phases.
    public static LifecycleSubPhaseDefinition[] SubPhaseDefinitions()
    {
        return
        [
            new("1.1", "1", "Design System and Tokens", "Owns visual tokens, component primitives, spacing, typography, color, and reusable UI rules.", "design-system", ["UI"]),
            new("1.2", "1", "Layout and Responsive Structure", "Owns page layout, responsive behavior, density, and information structure.", "layout-responsive", ["UI"]),
            new("1.3", "1", "Interaction and Navigation Model", "Owns interaction states, navigation paths, controls, and user task movement.", "interaction-navigation", ["UI"]),
            new("1.4", "1", "Accessibility and Usability Review", "Owns accessibility constraints, usability risks, and user-facing quality checks.", "accessibility-usability", ["UI", "QA"]),
            new("2.1", "2", "Business Capability Definition", "Owns capability boundaries, user value, actors, and expected business outcomes.", "business-capability", ["DOCUMENT"]),
            new("2.2", "2", "User Stories and Acceptance Criteria", "Owns story slices, acceptance gates, and observable behavior requirements.", "acceptance-criteria", ["DOCUMENT", "QA"]),
            new("2.3", "2", "Data and Validation Rules", "Owns input data, validation policy, state transitions, and failure behavior.", "data-validation", ["API_CONTRACT", "DOCUMENT"]),
            new("2.4", "2", "Edge Cases and Boundary Conditions", "Owns edge cases, boundary examples, exception paths, and ambiguity closure.", "edge-cases", ["DOCUMENT"]),
            new("3.1", "3", "Architecture and Component Topology", "Owns component boundaries, dependency direction, lifecycle topology, and system responsibilities.", "architecture-topology", ["ARCHITECTURE"]),
            new("3.2", "3", "Data Model and Persistence Design", "Owns entity shape, storage access, consistency, and persistence constraints.", "data-persistence", ["ARCHITECTURE"]),
            new("3.3", "3", "API Contract Definition", "Owns endpoint shape, request and response contracts, schema, and compatibility rules.", "api-contract", ["API_CONTRACT"]),
            new("3.4", "3", "Security and Authorization Model", "Owns security model, authentication, authorization, token, OAuth, OIDC, ASVS, session, and tenant-isolation routing.", "security-verification", ["SECURITY", "AUTHORIZATION"]),
            new("3.5", "3", "Observability and Operational Design", "Owns logging, metrics, traces, alerting, and operational feedback signals.", "observability-design", ["ARCHITECTURE"]),
            new("4.1", "4", "Backend Services", "Owns backend service implementation, provider connectors, integration code, and application service changes.", "backend-services", ["IMPLEMENT"]),
            new("4.2", "4", "Frontend and Client Integration", "Owns client implementation, UI integration, state wiring, and API consumption.", "frontend-client-integration", ["UI", "IMPLEMENT"]),
            new("4.3", "4", "Infrastructure and Deployment Code", "Owns infrastructure code, deployment shape, environment wiring, and runtime platform changes.", "infrastructure-deployment", ["IMPLEMENT"]),
            new("4.4", "4", "Refactoring and Code Quality", "Owns internal code improvement, simplification, dependency cleanup, and maintainability.", "refactoring-code-quality", ["IMPLEMENT"]),
            new("4.5", "4", "Integration and Provider Connectors", "Owns external provider integration, adapters, protocol mapping, and connector reliability.", "provider-connectors", ["IMPLEMENT", "API_CONTRACT"]),
            new("5.1", "5", "Unit Test Design", "Owns isolated test cases, local behavior checks, and component-level verification.", "unit-tests", ["TEST"]),
            new("5.2", "5", "Integration Test Design", "Owns multi-component tests, provider tests, storage tests, and boundary verification.", "integration-tests", ["TEST", "API_CONTRACT"]),
            new("5.3", "5", "End-to-End Scenario Testing", "Owns user journey tests, full scenario coverage, and cross-system execution evidence.", "end-to-end-tests", ["TEST", "QA"]),
            new("5.4", "5", "Regression and Compatibility Testing", "Owns regression coverage, compatibility checks, and known-behavior preservation.", "regression-compatibility", ["TEST"]),
            new("6.1", "6", "Manual QA and Acceptance Review", "Owns human acceptance review, exploratory validation, and release-facing quality judgment.", "manual-qa", ["QA"]),
            new("6.2", "6", "Release Readiness and Risk Review", "Owns release readiness, risk gates, operational readiness, and release decision evidence.", "release-risk", ["QA"]),
            new("6.3", "6", "Security and Compliance Review", "Owns implemented-security review, compliance evidence, vulnerability checks, and control validation.", "security-compliance-review", ["QA"]),
            new("6.4", "6", "Performance and Reliability Review", "Owns latency, load, reliability, resource usage, and operational resilience checks.", "performance-reliability", ["QA"]),
            new("7.1", "7", "Automated Test Harness", "Owns automation harness design, runners, fixtures, and repeatable validation machinery.", "automation-harness", ["TEST", "AUTOMATION"]),
            new("7.2", "7", "CI Pipeline and Quality Gates", "Owns continuous integration, quality gates, build checks, and release automation.", "ci-quality-gates", ["AUTOMATION"]),
            new("7.3", "7", "Monitoring and Feedback Loop", "Owns runtime observation, feedback ingestion, learning signals, and operational loop closure.", "monitoring-feedback", ["AUTOMATION"]),
            new("7.4", "7", "Coverage and Reporting", "Owns coverage measurement, result reporting, dashboards, and audit-ready evidence.", "coverage-reporting", ["TEST", "DOCUMENT"]),
            new("7.5", "7", "Learning Set and Model Feedback", "Owns learning-set updates, model feedback, capability gaps, and retraining evidence.", "learning-feedback", ["LEARNING"])
        ];
    }

    // Return lifecycle transition edges.
    public static LogicalTransition[] TransitionDefinitions()
    {
        var transitions = new List<LogicalTransition>();

        // Add phase-level handoffs.
        var phaseIds = PhaseDefinitions().Select(phase => phase.PhaseId).ToArray();
        for (var index = 0; index < phaseIds.Length - 1; index++)
        {
            transitions.Add(new LogicalTransition(
                $"transition-phase-{phaseIds[index]}-to-{phaseIds[index + 1]}",
                "phase",
                phaseIds[index],
                "phase",
                phaseIds[index + 1],
                $"Carries completed phase {phaseIds[index]} context into phase {phaseIds[index + 1]}.",
                ["DEVELOPMENT"],
                ["PRESERVE_CONSTRAINTS", "CARRY_CONTEXT_FORWARD"],
                [SourceId]));
        }

        // Add sub-phase level handoffs.
        var subPhaseIds = SubPhaseDefinitions().Select(subPhase => subPhase.SubPhaseId).ToArray();
        for (var index = 0; index < subPhaseIds.Length - 1; index++)
        {
            transitions.Add(new LogicalTransition(
                $"transition-sub-phase-{subPhaseIds[index]}-to-{subPhaseIds[index + 1]}",
                "sub-phase",
                subPhaseIds[index],
                "sub-phase",
                subPhaseIds[index + 1],
                $"Carries completed sub-phase {subPhaseIds[index]} context into sub-phase {subPhaseIds[index + 1]}.",
                ["DEVELOPMENT"],
                ["PRESERVE_CONSTRAINTS", "CARRY_CONTEXT_FORWARD"],
                [SourceId]));
        }

        return transitions.ToArray();
    }
}

// This record stores one phase seed definition.
public sealed record LifecyclePhaseDefinition(string PhaseId, string Name, string Purpose, string[] PredicateIds);

// This record stores one sub-phase seed definition.
public sealed record LifecycleSubPhaseDefinition(string SubPhaseId, string PhaseId, string Name, string Purpose, string TaskType, string[] PredicateIds);
