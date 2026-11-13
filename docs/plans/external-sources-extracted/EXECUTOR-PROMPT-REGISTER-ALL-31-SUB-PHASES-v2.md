# Executor Session — Register All 31 Sub-Phases in the Topology
# Version: 2.0 — 2026-05-08
# Supersedes: EXECUTOR-PROMPT-REGISTER-ALL-31-SUB-PHASES-v1.md

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
`ILogicalTopologyRegistry` against each incoming prompt. It currently has only
one sub-phase registered: 3.4 (security and authorization). This means every
prompt that does not mention security routes to a knowledge-edge result, even
though most other development sub-phases are conceptually covered.

Registering all 31 sub-phases makes the router aware of every development domain.
Prompts will fan out across the correct territories. Knowledge-edge branches will
appear only for sub-phases that have no registered executable flow — which is
still all sub-phases except 3.4. That is the correct and honest state: the router
knows about all territories; only 3.4 has a flow behind it.

This session registers topology entries only. It does not register flows. It does
not write pair files. It does not change tests for the adaptive orchestrator beyond
adding the registration verification tests.

---

## How the router uses registered sub-phases

The scoring formula is:
- 70% predicate overlap between `CompiledPredicateIds` and the sub-phase's
  `PredicateIds` array
- 30% text token overlap between the compiled source document tokens and tokens
  from `SubPhaseId + Name + Purpose`

The `Name` and `Purpose` fields are routing signal — they must contain domain
vocabulary that real developer prompts also contain. The `PredicateIds` array
must list the compiler predicate IDs that match this domain.

The nine compiler predicate IDs and their trigger terms:

```
SECURITY      — jwt, authentication, auth, oauth, oidc, token, session, asvs, mfa, rbac, security, authorization
AUTHORIZATION — authorization, authorisation, permission, rbac, scope, tenant, ownership
API_CONTRACT  — api, endpoint, contract, schema, request, response, openapi
ARCHITECTURE  — architecture, component, topology, boundary, dependency, service
IMPLEMENT     — "implement the", "implementation plan", connector, provider, "integration layer", "code change"
TEST          — test, testing, "integration test", "unit test", coverage, fixture
QA            — qa, quality, release, acceptance, verification
UI            — ui, ux, screen, layout, design, interaction, accessibility
DOCUMENT      — document, documentation, spec, specification, requirements
```

---

## The 31 sub-phases to register

Add these `AddOrUpdateSubPhase` calls inside `CreateDefault()` in
`LogicalOrchestratorAssistant.cs`, after the existing 3.4 registration block
and before the prompt compiler and router construction.

Return the `OperationResult` and fail early for each unsuccessful registration
using the same pattern as the existing 3.4 block.

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

- 5.1 Unit Test Design confused with 5.2 Integration Tests (mock isolation
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
    "Integration Tests",
    "Owns integration test design, cross-component validation, database integration tests, and fixture construction.",
    ["integration-tests"],
    ["TEST"],
    [])

new LogicalSubPhase("5.3", "5",
    "End-to-End Scenario Testing",
    "Owns end-to-end scenario testing, user journey automation, acceptance test execution, and full-stack flow verification.",
    ["end-to-end-tests"],
    ["TEST"],
    [])

new LogicalSubPhase("5.4", "5",
    "Performance and Load Tests",
    "Owns performance benchmarking, load test design, throughput measurement, latency profiling, and stress test execution.",
    ["performance-and-load-tests"],
    ["TEST", "QA"],
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

DPO pair note: Phase 7 sub-phases carry QA and AUTOMATION predicates.
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
    "Owns coverage reporting, test result aggregation, quality dashboards, and audit trail generation.",
    ["coverage-and-reporting"],
    ["QA", "TEST"],
    [])
```

---

## File change summary

One file modified: `15.OrchestratorModel/Logical/LogicalOrchestratorAssistant.cs`

Location: inside `CreateDefault()`, after the existing 3.4 registration block,
before the prompt compiler and router construction.

The 30 new registration blocks follow the same pattern as the existing 3.4 block.
Each has a corresponding early-return failure check.

---

## New test file: Tests/TopologySubPhaseRegistrationTests.cs

```csharp
[Fact]
public void CreateDefault_RegistersAllThirtyOneSubPhases()
{
    var storage = new InMemorySessionStorage();
    var result = LogicalOrchestratorAssistant.CreateDefault(storage);
    Assert.True(result.IsSuccess, result.ErrorMessage);
    // Verify via routing round-trip — see test 2 for an architecture prompt.
}

[Fact]
public async Task MultiSubPhaseRouter_ArchitecturePrompt_RoutesToSubPhase31()
{
    var storage = new InMemorySessionStorage();
    var assistant = LogicalOrchestratorAssistant.CreateDefault(storage).Value!;

    var result = await assistant.RunAsync(
        "We need to define the component boundaries and service topology for the new payment service.",
        scenarioId: null,
        flags: new LogicalExecutionFlags());

    Assert.True(result.IsSuccess, result.ErrorMessage);

    var branches = result.Value!.BranchResults;
    Assert.True(branches.Any(b =>
        b.Metrics.TryGetValue("missing_sub_phase", out var sp) && sp == "3.1" ||
        b.ScenarioId.Contains("3.1")),
        "Expected at least one branch routed to sub-phase 3.1");
}

[Fact]
public async Task MultiSubPhaseRouter_TestingPrompt_RoutesToSubPhase52()
{
    var storage = new InMemorySessionStorage();
    var assistant = LogicalOrchestratorAssistant.CreateDefault(storage).Value!;

    var result = await assistant.RunAsync(
        "We need integration tests for the tenant isolation across the authentication and API layers.",
        scenarioId: null,
        flags: new LogicalExecutionFlags());

    Assert.True(result.IsSuccess, result.ErrorMessage);

    var branches = result.Value!.BranchResults;
    Assert.True(branches.Any(b =>
        b.Metrics.TryGetValue("missing_sub_phase", out var sp) && sp == "5.2" ||
        b.ScenarioId.Contains("5.2")),
        "Expected at least one branch routed to sub-phase 5.2");
}

[Fact]
public async Task MultiSubPhaseRouter_MultiDomainPrompt_RoutesBothSecurityAndTesting()
{
    var storage = new InMemorySessionStorage();
    var assistant = LogicalOrchestratorAssistant.CreateDefault(storage).Value!;

    var result = await assistant.RunAsync(
        "Add JWT authentication and write integration tests for tenant isolation.",
        scenarioId: null,
        flags: new LogicalExecutionFlags());

    Assert.True(result.IsSuccess, result.ErrorMessage);

    // Security branch (3.4) should be a real flow result, not a knowledge edge.
    var securityBranch = result.Value!.BranchResults.FirstOrDefault(b =>
        b.ScenarioId == SecurityVerification34FlowAdapter.ScenarioId ||
        (b.Metrics.TryGetValue("missing_sub_phase", out var sp) && sp == "3.4"));
    Assert.NotNull(securityBranch);

    // Testing branch (5.2) should be a knowledge-edge result.
    var testBranch = result.Value!.BranchResults.FirstOrDefault(b =>
        b.Metrics.TryGetValue("missing_sub_phase", out var sp) && sp == "5.2");
    Assert.NotNull(testBranch);
    Assert.Equal("KNOWLEDGE_EDGE", testBranch!.OverallVerdict);
}
```

---

## Gate

```powershell
dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj
# Expected: >= current floor, 0 failed

dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj `
    --filter "TopologySubPhaseRegistration"
# Expected: all pass

dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj `
    --filter "AdaptiveLogicalOrchestrator"
# Expected: all pass
```

---

## What happens after this session

The router will return routing decisions for all 31 sub-phases when a prompt
contains matching predicates or vocabulary. Every sub-phase except 3.4 will
produce a `KNOWLEDGE_EDGE` branch result.

This is the correct and honest state. The machine knows what it does not know.

Learning set pair writers can now produce pairs for all 31 sub-phases. The ARCH
and PLAN confusion tables in the examination method document apply immediately.
Coverage becomes measurable: when a flow is registered for sub-phase 5.2, the
5.2 branch changes from KNOWLEDGE_EDGE to a real result. Progress is visible
in the routing output.

---

## Rules for this session

One file modified. One test file created. No behavior changes to existing flows.
No changes to the adaptive orchestrator, router units, compiler, or flow registry.
No new flows registered. No pair files produced.

If any registration call fails at runtime, fix the `LogicalSubPhase` record
construction — not the registry.

Do not add sub-phases beyond the 31 listed. Do not change the existing 3.4
registration.

Test floor: do not drop below current floor.
