# Executor Reference - Verify All 31 Sub-Phases Against Code
# Version: 2.0 — 2026-05-08
# Supersedes: EXECUTOR-PROMPT-REGISTER-ALL-31-SUB-PHASES-v1.md

---

## 2026-05-11 Routing-Flow Correction

This document is a topology reference and verification prompt. It is not enough
to train routing as two separate helper units.

For DPO acceptance, the target is:

```csharp
OrchestrationRoutingFlow : ILogicalFlow<LogicalOrchestratorRequest>
```

or equivalent `AdaptiveLogicalOrchestrator` behavior.

`PromptRoutingInputCompilerUnit` and `MultiSubPhaseRouterUnit` rows may be used
to teach internal stations, but a routing batch is accepted only when held-out
requests run through the full flow: compile, route, detect knowledge edges,
resolve flow/factory boundary, and return final verdict. Otherwise use
`PAIRSET_READY_ROUTING_FLOW_PENDING`.

---

## What changed from v1

**DPO pair notes added per sub-phase group.** Each phase block now states which
confusion patterns are most likely for its sub-phases and which rejected type
to use when writing DPO pairs for that group.

**ARCH and PLAN marked as priority targets.** Sub-phases 3.1, 3.2, 3.5, and 6.1
are the primary accuracy gap. Sub-phases 5.1, 5.2, and 5.3 are secondary gap.
These markings guide which sub-phases to write pairs for first after registration.

---

## 2026-05-11 Run-Experience Fix

Operational runbook:
`C:\Users\lubao\Documents\xiigen\llm mvp\external sources learning 2\documentation\TRAINING-SET-BUILD-AND-TESTING-RUNBOOK-v1.md`

This executor prompt registers topology. It must not flatten source authority.
When writing learning sets from executor output, preserve the difference between:

- the executor action,
- the source document that justifies the action,
- the prompt or user request that initiated the action,
- the state/evidence row that proves it happened.

User-intent and session-start documents may be first-class A-side authorities.
Do not send them to review-required merely because they are not code files.

Additional pair gates for all executor-derived rows:

```text
chosen != rejected
rejected contains [WRONG: ...]
no mojibake in prompt/a_side/chosen/rejected
the sub-phase id is explicit
the transition type is explicit
the source span is explicit
ledger/status rows stay evidence-only
```

Use small standalone ablations before adding executor-derived rows to the
orchestration/plans/skills baseline.

---

## What this session does and why

The multi-sub-phase router scores every sub-phase registered in
`ILogicalTopologyRegistry` against each incoming prompt. Current code already
seeds the whole development topology: `LogicalOrchestratorAssistant.CreateDefault()`
delegates to `DevelopmentLifecycleTopologySeeder.Seed(...)`, and that seeder
registers all 7 phases, all 31 sub-phases, and one universal nine-node flow per
sub-phase unless a specific flow already exists.

This file is therefore not an instruction to add manual `AddOrUpdateSubPhase`
blocks to `LogicalOrchestratorAssistant.cs`. That was the historical v1 state.
Running those edits now would duplicate authority already owned by the seeder
and risks teaching stale sub-phase names to DPO pairs.

Use this document as a code-aligned topology reference and verification prompt:
verify the seeder, verify the compiler lexicon, then author pairs from the
current code-backed definitions below.

---

## How the router uses registered sub-phases

The current `MultiSubPhaseRouterUnit` scoring formula is:

Without context carry-forward:
- 55% predicate overlap between `CompiledPredicateIds` and the sub-phase's
  `PredicateIds` array
- 25% exact sub-phase mention
- 20% text token overlap between the compiled source document tokens and tokens
  from `SubPhaseId + Name + Purpose`
- 2% E34V2 hybrid rank tie-breaker

With context carry-forward:
- 50% predicate overlap
- 20% exact sub-phase mention
- 15% text token overlap
- 5% E34V2 hybrid rank
- 10% context score

The `Name` and `Purpose` fields are routing signal. They must contain domain
vocabulary that real developer prompts also contain. The `PredicateIds` array
must list compiler predicate IDs that can actually be emitted by
`DevelopmentPromptCompiler`.

The current compiler predicate IDs and trigger terms are:

```
SECURITY      - jwt, authentication, auth, oauth, oidc, token, session, asvs, mfa, rbac, security, authorization
AUTHORIZATION - authorization, authorisation, permission, rbac, scope, tenant, ownership
API_CONTRACT  - api, endpoint, contract, schema, request, response, openapi
ARCHITECTURE  - architecture, component topology, system boundary, dependency direction, project, codebase
IMPLEMENT     - implement the, implementation plan, connector, provider, integration layer, code change, fix, bug, issue
TEST          - test, testing, integration test, unit test, coverage, fixture, verify, validation, regression
QA            - qa, quality, release, acceptance, verification
UI            - ui, ux, screen, layout, figma, frame, interaction, accessibility
DOCUMENT      - documentation, docs, readme, write/create/generate document, spec, specification, requirements
AUTOMATION    - automation, automate, automated, ci, pipeline, quality gate, build gate, monitoring feedback, coverage reporting
LEARNING      - skill, skills, claude code, learning, teach, training, reusable guidance, knowledge, model feedback
```

Historical warning: older copies of this guide say nine predicates and omit
`LEARNING`; that is stale. Older code also omitted `AUTOMATION`; current code
must emit it because Phase 7 topology uses it.

---

## The 31 code-backed sub-phases

These are the definitions currently owned by
`21.AgentInfrastructure/Logical/DevelopmentLifecycleTopologySeeder.cs`. If this
document and the seeder disagree, the seeder wins and this document must be
updated before any new learning pairs are written.

Do not write pairs for a sub-phase that is not registered in the seeder. In
particular, `3.6` is not registered in current code; Families 9-14 that mention
`3.6` must be held or remapped before training.

---

### Phase 1 — UI and UX Definition

DPO pair note: sub-phases 1.x all carry the UI predicate. The main confusion
when writing pairs is between 1.1 Design System (owns tokens and primitives),
1.2 Layout (owns responsive structure), and 1.3 Interaction (owns navigation
paths and states). Use Type A rejected with the adjacent sub-phase. 1.4
Accessibility Review also carries QA — it can be confused with 6.1 Manual QA.

```csharp
new LogicalSubPhase("1.1", "1",
    "Design System and Tokens",
    "Owns visual tokens, component primitives, spacing, typography, color, and reusable UI rules.",
    ["ui-and-ux-definition"],
    ["UI"],
    [])

new LogicalSubPhase("1.2", "1",
    "Layout and Responsive Structure",
    "Owns page layout, responsive behavior, density, and information structure.",
    ["ui-and-ux-definition"],
    ["UI"],
    [])

new LogicalSubPhase("1.3", "1",
    "Interaction and Navigation Model",
    "Owns interaction states, navigation paths, controls, and user task movement.",
    ["ui-and-ux-definition"],
    ["UI"],
    [])

new LogicalSubPhase("1.4", "1",
    "Accessibility and Usability Review",
    "Owns accessibility constraints, usability risks, and user-facing quality checks.",
    ["ui-and-ux-definition"],
    ["UI", "QA"],
    [])
```

---

### Phase 2 — Requirements and Business Rules

DPO pair note: sub-phases 2.x carry DOCUMENT and QA predicates. The main
confusion is between 2.1 Business Capability Definition (owns the what and
who) and 2.2 User Stories (owns observable behavior and acceptance gates).
2.4 Edge Cases is the sub-phase most likely to be confused with Stage 9
corruption work — they are different: 2.4 owns requirement-level edge cases,
Stage 9 owns training corruption cases.

```csharp
new LogicalSubPhase("2.1", "2",
    "Business Capability Definition",
    "Owns capability boundaries, user value, actors, and expected business outcomes.",
    ["requirements-and-business-rules"],
    ["DOCUMENT"],
    [])

new LogicalSubPhase("2.2", "2",
    "User Stories and Acceptance Criteria",
    "Owns story slices, acceptance gates, and observable behavior requirements.",
    ["requirements-and-business-rules"],
    ["DOCUMENT", "QA"],
    [])

new LogicalSubPhase("2.3", "2",
    "Data and Validation Rules",
    "Owns input data, validation policy, state transitions, and failure behavior.",
    ["requirements-and-business-rules"],
    ["API_CONTRACT", "DOCUMENT"],
    [])

new LogicalSubPhase("2.4", "2",
    "Edge Cases and Boundary Conditions",
    "Owns edge cases, boundary examples, exception paths, and ambiguity closure.",
    ["requirements-and-business-rules"],
    ["DOCUMENT"],
    [])
```

---

### Phase 3 — Technical Design

Sub-phase 3.4 is already registered. Add the remaining four.

DPO pair note: **Phase 3 is the primary ARCH accuracy gap.** Sub-phases 3.1,
3.2, and 3.5 had zero training batches in calibration run 2026-05-06. These
must be the first target for ARCH pair writing after registration.

The most common confusion for Phase 3 sub-phases:

- 3.1 Architecture and Component Topology confused with 4.1 Backend Services
  (writing the class instead of defining the boundary) — Type A rejected
- 3.2 Data Model confused with 4.1 Backend Services (writing the repository
  instead of defining the entity shape) — Type A rejected
- 3.5 Observability and Operational Design confused with 7.3 Monitoring and
  Feedback Loop (configuring the dashboard before designing what to measure)
  — Type A rejected

```csharp
new LogicalSubPhase("3.1", "3",
    "Architecture and Component Topology",
    "Owns component boundaries, dependency direction, lifecycle topology, and system responsibilities.",
    ["architecture-topology"],
    ["ARCHITECTURE"],
    [])

new LogicalSubPhase("3.2", "3",
    "Data Model and Persistence Design",
    "Owns entity shape, storage access, consistency, and persistence constraints.",
    ["data-persistence"],
    ["ARCHITECTURE"],
    [])

new LogicalSubPhase("3.3", "3",
    "API Contract Definition",
    "Owns endpoint shape, request and response contracts, schema, and compatibility rules.",
    ["api-contract"],
    ["API_CONTRACT"],
    [])

new LogicalSubPhase("3.5", "3",
    "Observability and Operational Design",
    "Owns logging, metrics, traces, alerting, and operational feedback signals.",
    ["observability-design"],
    ["ARCHITECTURE"],
    [])
```

---

### Phase 4 — Implementation

DPO pair note: Phase 4 sub-phases carry IMPLEMENT and API_CONTRACT predicates.
These sub-phases are performing adequately in calibration — CODE quality is
not the primary gap. Do not prioritise pair writing here until ARCH and PLAN
gaps are closed.

The most useful pairs for 4.x are Type C rejected — teaching the boundary
between ARCH design (Phase 3) and implementation (Phase 4). For example,
a prompt asking to "implement the service" routes to 4.1, and the Type C
rejected routes it to 3.1 (design first, then implement).

```csharp
new LogicalSubPhase("4.1", "4",
    "Backend Services",
    "Owns backend service implementation, provider connectors, integration code, and application service changes.",
    ["backend-services"],
    ["IMPLEMENT"],
    [])

new LogicalSubPhase("4.2", "4",
    "Frontend and Client Integration",
    "Owns client implementation, UI integration, state wiring, and API consumption.",
    ["frontend-client-integration"],
    ["UI", "IMPLEMENT"],
    [])

new LogicalSubPhase("4.3", "4",
    "Infrastructure and Deployment Code",
    "Owns infrastructure code, deployment shape, environment wiring, and runtime platform changes.",
    ["infrastructure-deployment"],
    ["IMPLEMENT"],
    [])

new LogicalSubPhase("4.4", "4",
    "Refactoring and Code Quality",
    "Owns internal code improvement, simplification, dependency cleanup, and maintainability.",
    ["refactoring-code-quality"],
    ["IMPLEMENT"],
    [])

new LogicalSubPhase("4.5", "4",
    "Integration and Provider Connectors",
    "Owns external provider integration, adapters, protocol mapping, and connector reliability.",
    ["provider-connectors"],
    ["IMPLEMENT", "API_CONTRACT"],
    [])
```

---

### Phase 5 — Testing

DPO pair note: **Phase 5 sub-phases are empty in training.** Sub-phases 5.1,
5.2, and 5.3 had zero pairs in calibration run 2026-05-06. These are the
secondary priority after ARCH.

The most common confusion in Phase 5:

- 5.1 Unit Test Design confused with 5.2 Integration Test Design (mock isolation
  sufficient vs real container required) — Type A rejected; Testcontainers
  documentation is the authority
- 5.3 End-to-End Tests confused with 6.1 Manual QA (automation vs human
  review) — Type A rejected

```csharp
new LogicalSubPhase("5.1", "5",
    "Unit Test Design",
    "Owns isolated test cases, local behavior checks, and component-level verification.",
    ["unit-tests"],
    ["TEST"],
    [])

new LogicalSubPhase("5.2", "5",
    "Integration Test Design",
    "Owns multi-component tests, provider tests, storage tests, and boundary verification.",
    ["integration-tests"],
    ["TEST", "API_CONTRACT"],
    [])

new LogicalSubPhase("5.3", "5",
    "End-to-End Scenario Testing",
    "Owns user journey tests, full scenario coverage, and cross-system execution evidence.",
    ["end-to-end-tests"],
    ["TEST", "QA"],
    [])

new LogicalSubPhase("5.4", "5",
    "Regression and Compatibility Testing",
    "Owns regression coverage, compatibility checks, and known-behavior preservation.",
    ["regression-compatibility"],
    ["TEST"],
    [])
```

---

### Phase 6 — Review

DPO pair note: **6.1 Manual QA and Acceptance Review is an ARCH gap sub-phase.**
It is the most commonly confused with automated testing. Every 6.1 pair should
use Type A rejected with 5.1 or 5.3 as the rejected sub-phase.

6.3 Security and Compliance Review is Wave 2 priority. It must be distinguished
from 3.4 (design the security model) using Type A rejected — 6.3 owns reviewing
whether the implementation satisfies the model, not designing the model itself.

```csharp
new LogicalSubPhase("6.1", "6",
    "Manual QA and Acceptance Review",
    "Owns human acceptance review, exploratory validation, and release-facing quality judgment.",
    ["manual-qa"],
    ["QA"],
    [])

new LogicalSubPhase("6.2", "6",
    "Release Readiness and Risk Review",
    "Owns release readiness, risk gates, operational readiness, and release decision evidence.",
    ["release-risk"],
    ["QA"],
    [])

new LogicalSubPhase("6.3", "6",
    "Security and Compliance Review",
    "Owns implemented-security review, compliance evidence, vulnerability checks, and control validation.",
    ["security-compliance-review"],
    ["QA"],
    [])

new LogicalSubPhase("6.4", "6",
    "Performance and Reliability Review",
    "Owns latency, load, reliability, resource usage, and operational resilience checks.",
    ["performance-reliability"],
    ["QA"],
    [])
```

---

### Phase 7 — Automation and Maintenance

DPO pair note: Phase 7 sub-phases carry TEST, DOCUMENT, AUTOMATION, and LEARNING predicates.
The main confusion is between 7.2 CI Pipeline (owns build gates and test
automation integration) and 7.3 Monitoring and Feedback Loop (owns runtime
observation). This confusion also appears in ARCH pairs where 3.5 Observability
Design is confused with 7.3 — designing what to measure is 3.5, operating
the measurement infrastructure is 7.3.

```csharp
new LogicalSubPhase("7.1", "7",
    "Automated Test Harness",
    "Owns automation harness design, runners, fixtures, and repeatable validation machinery.",
    ["automation-harness"],
    ["TEST", "AUTOMATION"],
    [])

new LogicalSubPhase("7.2", "7",
    "CI Pipeline and Quality Gates",
    "Owns continuous integration, quality gates, build checks, and release automation.",
    ["ci-quality-gates"],
    ["AUTOMATION"],
    [])

new LogicalSubPhase("7.3", "7",
    "Monitoring and Feedback Loop",
    "Owns runtime observation, feedback ingestion, learning signals, and operational loop closure.",
    ["monitoring-feedback"],
    ["AUTOMATION"],
    [])

new LogicalSubPhase("7.4", "7",
    "Coverage and Reporting",
    "Owns coverage measurement, result reporting, dashboards, and audit-ready evidence.",
    ["coverage-reporting"],
    ["TEST", "DOCUMENT"],
    [])

new LogicalSubPhase("7.5", "7",
    "Learning Set and Model Feedback",
    "Owns learning-set updates, model feedback, capability gaps, and retraining evidence.",
    ["learning-feedback"],
    ["LEARNING"],
    [])
```

---

## File change summary

Current code state:

- `15.OrchestratorModel/Logical/LogicalOrchestratorAssistant.cs` already calls `DevelopmentLifecycleTopologySeeder.Seed(...)`.
- `21.AgentInfrastructure/Logical/DevelopmentLifecycleTopologySeeder.cs` is the authority for all 31 sub-phase definitions.
- `21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs` is the authority for the current scoring formula.
- `29.SemanticCompiler/Development/DevelopmentPromptCompiler.cs` is the authority for emitted compiler predicates.

No manual registration blocks should be added to `CreateDefault()` unless the
seeder is removed. If routing does not reach a sub-phase, first check whether
the compiler can emit that sub-phase predicate and whether anchor terms match.

---

## Gate

```powershell
dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj --filter "PromptRoutingInputCompilerUnitTests"
dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj --filter "DevelopmentLifecycleTopologySeederTests|UniversalFlowSingletonTests"
```

Expected: all selected tests pass, with no drop below the current suite floor.

---

## Pair-authoring constraints after this correction

- Do not create new DPO pairs that describe `5.4` as performance/load testing; current code defines `5.4` as regression and compatibility testing.
- Do not create new DPO pairs that target `3.6`; current code has no `3.6` sub-phase.
- Treat `LEARNING` as a compiler-emitted predicate and use it for `7.5` learning-set/model-feedback prompts.
- Treat `AUTOMATION` as a compiler-emitted predicate required for Phase 7 routing.
- Use code-backed names and predicates from the seeder when a guide, registry, or extracted copy disagrees.
- If a document claims only `3.4` is registered, mark that document stale for topology state. Current code seeds all 31 sub-phases.

Test floor: do not drop below current floor.
