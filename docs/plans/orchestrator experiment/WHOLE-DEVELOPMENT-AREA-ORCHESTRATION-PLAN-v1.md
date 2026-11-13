# Whole Development Area Orchestration Plan — v1
# Date: 2026-05-05
# Scope: Full development lifecycle orchestration across all 7 phases and 31 sub-phases
# Sources: XIIGen real system + approved external sources
# Precondition: Learning cycle plan (LEARNING-CYCLE-IMPLEMENTATION-PLAN-v1.md) complete

---

## What This Plan Is Testing

The question this plan answers is not whether the orchestrator can classify
a document. It is whether the orchestrator can manage a real development
challenge from the first conversation through a working result — making the
right decisions at every phase transition, carrying context forward correctly,
recognizing when a decision in one phase constrains the next, and knowing when
to stop and surface a conflict rather than continuing silently.

Every scenario in this plan comes from a real system. XIIGen is the primary
source because its development history is fully documented and its phase
transitions are recorded. External approved sources (OWASP, NIST, IETF) supply
the domain constraints the orchestrator must apply during the relevant sub-phases.

---

## Why Orchestration Differs From What Has Been Tested So Far

Everything tested so far has been single-sub-phase. An input arrives, the
system routes it to one sub-phase, a verdict is returned. That proves the
cells work. It does not prove the orchestrator works.

Orchestration is a different test. A real development challenge spans multiple
sub-phases, the output of each sub-phase becomes input to the next, decisions
constrain later options, and the orchestrator must recognize when a downstream
sub-phase cannot proceed because an upstream decision was incomplete or
contradictory. A single-sub-phase system that passes all its unit tests can
still fail completely as an orchestrator if it does not carry context, does not
enforce phase ordering, and does not surface cross-phase conflicts.

The three things that distinguish orchestration from classification:

Phase ordering. Sub-phase 3.1 (architecture decisions) must complete before
sub-phase 3.4 (security controls), because the security model depends on the
architectural isolation model. An orchestrator that routes security questions
before the architecture is decided produces answers that may be technically
correct in isolation but wrong for the specific system being built.

Context carry-forward. A decision made in sub-phase 3.1 — for example, that
the system uses per-tenant database isolation — must be visible to sub-phase
4.1 when the backend services are being designed. An orchestrator that loses
context between sub-phases forces the developer to repeat themselves and risks
producing inconsistent guidance.

Conflict detection. Sometimes a developer request contains implicit conflict.
A request to add a new tenant in a system that declared single-tenant
architecture in sub-phase 3.1 is a conflict, not a classification task. The
orchestrator must detect that the request contradicts an earlier decision and
surface it rather than producing an answer that silently violates the
architecture.

---

## The Development Lifecycle Being Orchestrated

The orchestrator manages seven phases, each with sub-phases. The XIIGen
development history provides real transitions across all seven.

Phase 1 — UI and UX Definition (sub-phases 1.1 through 1.5)
Design system and tokens, screen and layout structure, interaction model and
state transitions, accessibility and compliance, motion and animation.

Phase 2 — Requirements and Business Rules (sub-phases 2.1 through 2.4)
User stories and acceptance criteria, business rules, integration point
specification, data model boundaries.

Phase 3 — Technical Design (sub-phases 3.1 through 3.5)
Architecture and component topology, data schema and migration design, API
contract definition, security and authorization model, infrastructure and
deployment topology.

Phase 4 — Implementation (sub-phases 4.1 through 4.5)
Backend services, frontend and client rendering, integration and provider
connectors, data migration execution, documentation.

Phase 5 — Testing (sub-phases 5.1 through 5.4)
Unit and contract tests, integration tests, end-to-end scenario tests,
performance and load tests.

Phase 6 — Review (sub-phases 6.1 through 6.4)
Functional review, regression coverage verification, security and compliance
review, accessibility audit.

Phase 7 — Automation and Maintenance (sub-phases 7.1 through 7.4)
Test scenario automation, regression suite maintenance, CI/CD gate
configuration, coverage and reporting.

---

## The Orchestration Scenarios

Each scenario is a real development challenge drawn from the XIIGen history
or from its external source integration work. Each scenario spans multiple
phases. Each scenario has a defined oracle — what the orchestrator should
produce, in what order, and what it should refuse to do.

---

### Scenario O1 — Multi-Tenant Authentication Addition
Source: XIIGen authentication gap remediation work (real project history)
Phases covered: 3.1, 3.3, 3.4, 4.1, 5.2, 6.3

A developer says: "We need to add multi-tenant JWT authentication to the API.
Each tenant must be isolated — one tenant's token must never access another
tenant's data."

What the orchestrator must do in order:

Sub-phase 3.1 first. Before any authentication design, the orchestrator must
establish what the current architecture says about tenant isolation. In the
XIIGen case, the architecture uses per-tenant scoping through a ClsService
context. The orchestrator must read this from the system context and make it
visible before proceeding.

Sub-phase 3.4 second. With the isolation model established, the orchestrator
applies the OWASP ASVS requirements relevant to multi-tenant JWT: V10
(OAuth/OIDC), V7 (session management), the cross-tenant isolation requirement.
It must produce specific control requirements, not generic advice.

Sub-phase 3.3 third. The API contract changes needed to enforce tenant
isolation — the required headers, the token shape, the error codes for
cross-tenant violations — must be derived from the 3.1 architecture and the
3.4 security controls. The contract cannot be designed independently of both.

Sub-phase 4.1 fourth. The backend service implementation plan must reflect
all three prior decisions. The orchestrator must surface any implementation
approach that would violate the 3.1 architecture or the 3.4 controls.

Sub-phase 5.2 fifth. The integration tests must include cross-tenant isolation
tests — specifically, a test where a valid token for tenant A is used against
tenant B's data and must be rejected. This requirement comes directly from the
3.4 security control for cross-tenant JWT isolation (Rule 8 in the test
integrity skill).

Sub-phase 6.3 last. The security review must verify that all five prior
sub-phases are consistent with each other and with the OWASP ASVS requirements.
If any inconsistency exists, the orchestrator must name it rather than passing.

What the orchestrator must refuse:

Producing the API contract before the architecture is decided.
Producing implementation guidance before the security controls are defined.
Passing the security review if cross-tenant isolation tests are absent.

Oracle gate for O1: the orchestrator produces six sub-phase outputs in the
correct order, each output references the prior sub-phase decisions explicitly,
and the security review correctly identifies one deliberate inconsistency
planted in the scenario (the test suite is missing the cross-tenant rejection
test).

---

### Scenario O2 — New Flow Registration in a Wave-Gated System
Source: XIIGen flow lifecycle management (real project history)
Phases covered: 2.1, 2.3, 3.1, 3.2, 4.1, 4.3, 5.1, 7.3

A developer says: "We need to register a new flow for artifact boundary
management. It must integrate with the existing lifecycle index and respect
the wave gate structure."

What the orchestrator must do in order:

Sub-phase 2.1 first. User stories and acceptance criteria for the new flow,
grounded in what the lifecycle index currently provides and what the wave gate
structure requires.

Sub-phase 2.3 second. Integration point specification — which existing flows
does this new flow depend on, what events does it consume, and what events
does it emit. The orchestrator must map this to the existing gate-event unlock
chain.

Sub-phase 3.1 third. Architecture decision: does this flow run as a new
logical unit, a new logical flow, or an extension of an existing one. The
answer depends on the wave assignment and the dependency graph. The
orchestrator must reason about this from the context, not guess.

Sub-phase 3.2 fourth. Data schema for the flow's index document shape. The
orchestrator must derive this from the existing lifecycle index schema and the
integration points established in 2.3.

Sub-phase 4.1 fifth. Backend service implementation. The orchestrator must
apply the portability checks established in the dev safety rules — no SDK
imports outside provider files, no ClsService violations, correct guard
declarations.

Sub-phase 4.3 sixth. Provider connector design. If the flow touches an
external system, the orchestrator must specify the connector through the
fabric interface, not by direct SDK use.

Sub-phase 5.1 seventh. Unit and contract tests. The orchestrator must require
tests for the gate event chain — specifically, a test that proves the upstream
flow must be ACTIVE before this flow can begin Phase A.

Sub-phase 7.3 last. CI/CD gate configuration. The orchestrator must specify
the gate check that verifies portability and authentication status before
promotion to ACTIVE.

What the orchestrator must refuse:

Proceeding to 3.1 without a defined wave assignment.
Designing the connector in 4.3 with a direct SDK import rather than through
the fabric interface.
Accepting a 5.1 test plan that omits the gate event chain test.

Oracle gate for O2: the orchestrator produces eight sub-phase outputs in
order, the wave assignment is established before architecture design begins,
and the connector specification correctly uses the fabric interface rather
than a direct SDK call.

---

### Scenario O3 — Performance Degradation Investigation
Source: XIIGen load testing and regression work (real project history)
Phases covered: 5.4, 6.2, 3.1 (revisit), 4.1 (revisit), 7.2

A developer says: "Response times on the artifact examination flow have
doubled since the last deployment. We need to find and fix the cause."

This scenario is different from O1 and O2. It starts in Phase 5 and may
require revisiting Phase 3 and Phase 4. The orchestrator must recognize that
a performance investigation is not a forward-only process — it may require
reopening architecture or implementation decisions.

What the orchestrator must do:

Sub-phase 5.4 first. Performance baseline measurement. The orchestrator
must establish what the current numbers are before proposing any fix.

Sub-phase 6.2 second. Regression coverage verification. The orchestrator
must check whether the regression was caught by any existing test and, if
not, name the coverage gap.

Sub-phase 3.1 revisit. If the performance data reveals that the architecture
choice (how many logical unit calls are made per request, how deeply nested
the orchestrator chain is) is the cause, the orchestrator must flag this as
an architecture-level finding rather than an implementation bug.

Sub-phase 4.1 revisit. If the cause is an implementation issue (an unnecessary
database call per iteration, a missing cache), the orchestrator must specify
the fix and verify it against the existing architecture decision — the fix
must not violate the isolation model established in sub-phase 3.1.

Sub-phase 7.2 last. The regression suite must be updated to catch this class
of performance degradation in future deployments. The orchestrator must specify
the new test in terms the CI gate can enforce.

What the orchestrator must refuse:

Proposing an implementation fix before the cause is established.
Proposing an architecture change without first verifying the implementation
is correctly following the existing architecture.
Closing the investigation without a regression test that would have caught
the degradation.

Oracle gate for O3: the orchestrator correctly identifies that the planted
cause is a per-iteration registry call that should be batched, surfaces it
as an implementation issue rather than an architecture issue, and specifies
a regression test that measures per-request registry call count.

---

### Scenario O4 — External Source Integration Under License Constraint
Source: External source pipeline work (real project history)
Phases covered: 2.2, 3.4, 3.3, 4.3, 6.3

A developer says: "We want to integrate OWASP ASVS verification into the
standard development workflow so that security checks run automatically during
the review phase. The OWASP content has a ShareAlike license."

What the orchestrator must handle:

The license constraint is a business rule (sub-phase 2.2) that affects the
API contract design (sub-phase 3.3), the security integration (sub-phase 3.4),
the connector design (sub-phase 4.3), and the compliance review (sub-phase 6.3).
The ShareAlike requirement means that any derivative work that incorporates
OWASP content must itself be shared under compatible terms. The orchestrator
must carry this constraint forward through every sub-phase and refuse to
produce a design that would violate it — for example, a connector that
embeds OWASP requirement text inside a proprietary output format.

Oracle gate for O4: the orchestrator surfaces the ShareAlike constraint at
sub-phase 2.2 and explicitly references it in sub-phases 3.3, 4.3, and 6.3.
It correctly refuses one deliberately non-compliant connector design planted
in the scenario.

---

### Scenario O5 — Full Greenfield Feature Cycle
Source: XIIGen skill advisor flow design (real project history)
Phases covered: All 7 phases, all 31 sub-phases

A developer says: "We are building a new skill advisor feature for the
XIIGen engine. It should recommend which skills to load for a given task,
learn from which recommendations were accepted, and improve over time."

This is the complete orchestration scenario. Every phase and every sub-phase
is relevant. The orchestrator must produce a coherent plan that covers all
seven phases, and each phase output must be consistent with all prior phases.

The oracle for O5 is the most demanding. The planted inconsistencies are:

A UI design in sub-phase 1.2 that assumes a synchronous response, while the
3.1 architecture specifies the recommendation runs as an async flow with a
webhook callback. The orchestrator must detect this conflict when sub-phase
3.1 is produced.

An API contract in sub-phase 3.3 that does not include the learning signal
endpoint, while the 2.1 user stories explicitly require the system to learn
from accepted recommendations. The orchestrator must detect that the contract
is incomplete relative to the requirements.

A test plan in sub-phase 5.3 that has no end-to-end test for the learning
loop — a scenario where a recommendation is accepted and the system produces
a different recommendation for the same input in a subsequent call. The
orchestrator must require this test before accepting sub-phase 5.3 as complete.

Oracle gate for O5: the orchestrator detects all three planted inconsistencies
at the correct phase transition points, halts at each one and surfaces the
conflict rather than continuing, and produces a complete seven-phase plan once
the inconsistencies are resolved.

---

## The Three Orchestration Capabilities Being Measured

Every scenario measures these three capabilities, not classification accuracy.

Phase ordering enforcement. Does the orchestrator refuse to produce sub-phase
N+1 output when sub-phase N is incomplete or contradictory? An orchestrator
that produces API contracts before the architecture is decided is not managing
the development cycle — it is generating text.

Context carry-forward fidelity. Does the output of each sub-phase explicitly
reference the constraints established in prior sub-phases? A sub-phase 4.1
implementation plan that does not mention the 3.1 architecture decisions has
lost context, even if the implementation guidance is individually correct.

Conflict detection and surfacing. When a developer request contradicts an
earlier decision, does the orchestrator name the conflict and halt rather than
continuing? This is the hardest capability to test because it requires the
oracle to plant deliberate inconsistencies and verify the orchestrator finds
them.

---

## Prerequisite Conditions

This plan cannot begin until all of the following are true:

The learning cycle plan is complete. All registered sub-phase flows have
trained units meeting the held-out accuracy thresholds. An orchestrator that
makes routing decisions based on hardcoded rules cannot demonstrate genuine
orchestration because any conflict detection or context carry-forward behavior
would itself be a hardcoded rule, not learned behavior.

All 31 sub-phases have registered flows. Scenario O5 requires all 31 sub-phases
to be operational. Scenarios O1 through O4 require the sub-phases they cover.
The minimum set to begin O1 is sub-phases 3.1, 3.3, 3.4, 4.1, 5.2, and 6.3.

The cross-phase context interface is implemented. A mechanism must exist for
the output of one sub-phase flow to be passed as input context to the next
sub-phase flow. This does not exist yet. It is the first new implementation
work this plan requires.

The phase ordering rules are encoded. The topology established in WDA-00
contains parent-child and transition relationships. Those relationships must
be readable by the orchestrator to enforce ordering. Currently they are stored
in the registry but nothing reads them to plan execution order.

---

## New Implementation Work Required

Three components need to be built before orchestration scenarios can run.
None of these exist yet.

Component 1 — Cross-phase context package.

A `DevelopmentContext` record that accumulates decisions across sub-phases.
When sub-phase 3.1 produces an architecture decision, it is written to the
context package. When sub-phase 3.4 runs, the context package is provided as
input so the security controls can reference the architecture. Each sub-phase
flow receives the context package as a read-only input and contributes its
own decisions to an updated output copy.

This is a new contract in `21.AgentInfrastructure/Logical/`. The record shape
is approximately: a list of phase decisions (sub-phase identifier, decision
type, decision content, timestamp), a list of open constraints (constraints
produced by earlier phases that must be honored by later phases), and a list
of detected conflicts (conflicts found during this orchestration run).

Component 2 — Phase sequencing orchestrator.

A new `ILogicalOrchestrator` implementation that reads the topology registry
to determine what sub-phases are relevant for a given request, what their
ordering constraints are, and which sub-phases can run in parallel versus
which must run in sequence. It calls each sub-phase flow in order, passing
the updated context package forward, and halts the sequence if a sub-phase
returns a conflict rather than a completed decision.

This is not the same as the existing `InMemoryLogicalOrchestrator`. That
orchestrator selects one flow per scenario. The phase sequencing orchestrator
selects and sequences multiple flows for one scenario.

Component 3 — Conflict detector.

A unit that compares a new sub-phase output against the existing context
package and identifies contradictions. A contradiction is defined as: a
decision in the new output that negates a constraint established in a prior
sub-phase. The detector does not resolve contradictions — it names them and
returns a conflict result that halts the orchestration sequence.

---

## Oracle Document Structure for Orchestration Scenarios

Each scenario requires five oracle documents, extending the Stage 1 pattern
to the orchestration level.

Scenario source binding. The XIIGen or external source history that defines
the scenario. Specific flow IDs, commit references, or document names that
establish the scenario as real rather than synthetic.

Multi-phase execution trace. The expected sequence of sub-phase activations,
the expected context package state after each sub-phase, and the expected
output content for each sub-phase. This is more complex than a single-node
trace because it captures how decisions accumulate and constrain later phases.

Conflict injection specification. The deliberate inconsistencies planted in
the scenario, where in the sequence they should be detected, and what the
orchestrator's response should look like when it detects them.

Human result standards. What the complete orchestration output looks like when
everything works correctly — both the individual sub-phase outputs and the
cross-phase coherence visible in the accumulated context package.

Executor contract. Instructions for building the test that proves the
orchestration scenario produces the oracle output. This includes the conflict
injection method — how the planted inconsistencies are introduced into the
test input without being visible in the test code itself.

---

## Execution Order

The implementation work and the scenario development run in this order:

First: implement Component 1 (cross-phase context package) as a new contract
and prove it can be populated and read across two sub-phase calls. Gate B:
a test passes a 3.1 architecture decision through the context package into a
3.4 sub-phase call and the 3.4 unit reads it correctly.

Second: implement Component 3 (conflict detector) as a unit that can be
added to any sub-phase flow. Gate B: the detector correctly identifies a
deliberate contradiction between a context package constraint and a new
sub-phase output.

Third: implement Component 2 (phase sequencing orchestrator) using the
topology registry to determine ordering. Gate B: given a request that spans
sub-phases 3.1 and 3.4, the orchestrator calls 3.1 first, passes the context
forward, calls 3.4 second, and would halt if 3.4 returned a conflict.

Fourth: produce the five oracle documents for Scenario O1 and run the test.
This is the first full orchestration proof.

Fifth: run Scenarios O2, O3, and O4 in order. Each requires its own oracle
documents and its own conflict injection.

Last: run Scenario O5 once all 31 sub-phase flows are trained and the
orchestration components have proven themselves on the prior scenarios.

---

## What Success Looks Like

The orchestration plan is complete when Scenario O5 passes its oracle gate:
the orchestrator detects all three planted inconsistencies at the correct phase
transitions, halts at each one, names the conflict precisely, and produces a
coherent seven-phase development plan when the inconsistencies are resolved.

That result means the system can manage a complete development cycle for a
real feature in a real codebase, recognize when the developer's own decisions
are inconsistent, and produce guidance that is coherent across all phases
rather than correct in each phase independently.

---

# Summary

| Component | What it proves | Depends on |
|---|---|---|
| Cross-phase context package | Decisions carry forward across sub-phases | Learning cycle complete |
| Conflict detector | Contradictions are found and named | Context package |
| Phase sequencing orchestrator | Multiple sub-phases run in correct order | Both above |
| Scenario O1 | Multi-phase orchestration with security | Sequencing orchestrator |
| Scenario O2 | Wave-gated flow registration | Sequencing orchestrator |
| Scenario O3 | Backward investigation across phases | Sequencing orchestrator |
| Scenario O4 | License constraint carry-forward | Sequencing orchestrator |
| Scenario O5 | Full 31-sub-phase development cycle | O1 through O4 proven |

---

# Last updated: 2026-05-05
# Status: awaiting learning cycle completion and three new components before
# first orchestration scenario can run
# Source authority: XIIGen real development history + approved external sources
