# Multi-Phase Orchestration — Design and Experiment Plan
# How to extend the dynamic flow to cover development phases,
# sub-phases, and verified phase transitions
# Version: 1.0 — 2026-05-04

---

## Part 1 — High-Level Design: What Needs to Be Added

---

### The Current State

The nine-node pipeline is a single-phase orchestrator. It takes one source
packet, decomposes it, plans it, processes it, reviews it, and formats it.
It does not know that its output is the input for a downstream phase. It does
not know that its planning decisions create constraints that the next phase
must honor. It treats every run as complete in itself.

This is correct for a single complex problem. It is insufficient for a
development lifecycle where UI/UX definition produces decisions that
functional specs must not contradict, functional specs produce decisions
that technical design must implement, technical design produces constraints
that implementation must respect, and implementation produces evidence that
QA must verify.

Three things need to be added to the architecture to support this. They do
not replace the nine-node pipeline — they sit above it and beside it.

---

### Addition 1 — The Phase Context Packet

The source packet today is hand-authored. It contains the original human
problem, constraints, explicit conflicts, explicit unknowns, and forbidden
assumptions. For a single run this works. For a multi-phase lifecycle it
does not, because the source packet for each downstream phase must be derived
from the outputs of the upstream phase — not authored from scratch.

The phase context packet is a structured artifact that a completed phase
produces alongside its human-readable result. It contains:

**Decisions made.** Every decision the orchestration accepted — named, with
its rationale and its source in the human request. A UI/UX phase that decided
on a component hierarchy produces a named decision: "primary navigation uses
a persistent left sidebar with collapsed mobile behavior." This decision
travels to the functional spec phase as a constraint.

**Constraints inherited.** Every constraint the phase received from its own
source packet that it did not resolve — these pass forward unchanged. A
constraint that said "no external authentication providers" was not resolved
by the UI/UX phase and must appear in every downstream phase's source packet.

**Knowledge edges.** Every domain where the phase named a missing competence.
If the UI/UX phase identified "accessibility compliance for WCAG 2.2 Level AA
is a knowledge edge — proofing rules not available locally," that knowledge
edge travels to the functional spec phase as an explicit open item, not as
an assumption that accessibility is handled.

**Boundary markers.** Every item the phase marked as boundary — present but
not fully specified. A boundary in the UI/UX phase ("exact animation timing
for state transitions is a boundary — needs motion design rules") travels
to the technical design phase as a flagged item, not as a resolved decision.

**Rejected items.** Every request the arbiters rejected, with the rejection
reason. A rejected item from the UI/UX phase ("request to use a custom
non-standard date picker rejected — violates design system consistency rule")
must appear in downstream phases as a known rejection, not as an option that
might be revisited.

The phase context packet is the mechanism that makes phase transitions honest.
Without it, each phase starts from a blank source packet authored by a human
who may not know what the upstream phase decided. With it, each phase starts
with a complete record of what the system has already accepted, rejected,
deferred, and identified as unknown.

---

### Addition 2 — The Phase Transition Gate

Between any two phases, a transition gate runs. The gate has one job: verify
that the downstream phase's source packet is consistent with the upstream
phase's context packet. It does not run the downstream phase. It only checks
whether the downstream phase is allowed to start given what the upstream
phase produced.

The transition gate checks four conditions:

**Constraint preservation.** Every constraint in the upstream context packet
appears in the downstream source packet, unchanged. A constraint that was
in the UI/UX phase but is absent from the functional spec source packet is a
gate failure. The downstream phase cannot silently drop a constraint.

**Decision acknowledgment.** Every decision in the upstream context packet
is either incorporated into the downstream source packet as a given fact, or
explicitly flagged as out of scope for the downstream phase. A downstream
phase that neither uses nor acknowledges an upstream decision is operating
on incomplete context.

**Knowledge edge continuity.** Every knowledge edge in the upstream context
packet either remains a knowledge edge in the downstream source packet, or is
resolved by new source authority that the downstream phase brings in. A
knowledge edge that silently disappears between phases — without a resolution
or a continued edge marker — is a gate failure. The system cannot pretend it
learned something it did not.

**Rejection preservation.** Every rejection in the upstream context packet
appears in the downstream source packet as a known rejection. A downstream
phase that encounters the same rejected item and treats it as an open
question has lost the upstream decision.

The transition gate produces one of three verdicts:

PASS — the downstream phase may start. All four conditions hold.

PASS WITH GAPS — the downstream phase may start, but specific gaps in the
downstream source packet are named. The gaps do not block the phase but must
be resolved before the phase's output is accepted.

BLOCK — the downstream source packet is missing constraints, has dropped
knowledge edges, or has silently reinstated rejected items. The source packet
must be corrected before the downstream phase starts.

---

### Addition 3 — The Phase Registry

The phase registry is a document that maps the development lifecycle phases
to their sub-phases, names the source document types that belong to each,
names the expected context packet structure for each, and names the
transition gate rules between adjacent phases.

The phase registry is the analogue of the flow registry from the architecture
principles. Just as the flow registry stores topology contracts — nodes,
edges, iron rules, quality gates — the phase registry stores phase contracts:
what each phase receives, what it produces, what it must preserve, and what
it is allowed to decide.

The phase registry makes the orchestrator aware of where it sits in the
lifecycle. Without it, each pipeline run is isolated. With it, the pipeline
knows that it is operating in the UI/UX definition phase, that its output
will be consumed by the functional spec phase, and that certain decisions
it makes will create constraints for downstream phases.

---

### The Development Phase Map

The development lifecycle you named maps to this phase structure. Each phase
has sub-phases. Each sub-phase has its own orchestration scope — it is a
separate pipeline run with its own source packet, its own domain splits,
its own planning, and its own context packet output.

```
Phase 1 — UI/UX Definition
  Sub-phase 1.1 — Design system and tokens
  Sub-phase 1.2 — Screen and layout structure
  Sub-phase 1.3 — Interaction model and state transitions
  Sub-phase 1.4 — Accessibility and compliance
  Sub-phase 1.5 — Motion and animation
  Transition gate: 1 → 2

Phase 2 — Functional Specification
  Sub-phase 2.1 — User story and acceptance criteria
  Sub-phase 2.2 — Business rule definition
  Sub-phase 2.3 — Integration point specification
  Sub-phase 2.4 — Data model boundaries
  Transition gate: 2 → 3

Phase 3 — Technical Design
  Sub-phase 3.1 — Architecture and component topology
  Sub-phase 3.2 — Data schema and migration design
  Sub-phase 3.3 — API contract definition
  Sub-phase 3.4 — Security and authorization model
  Sub-phase 3.5 — Infrastructure and deployment topology
  Transition gate: 3 → 4

Phase 4 — Implementation
  Sub-phase 4.1 — Backend services
  Sub-phase 4.2 — Frontend and client rendering
  Sub-phase 4.3 — Integration and provider connectors
  Sub-phase 4.4 — Data migration execution
  Sub-phase 4.5 — Documentation
  Transition gate: 4 → 5

Phase 5 — Testing
  Sub-phase 5.1 — Unit and contract tests
  Sub-phase 5.2 — Integration tests
  Sub-phase 5.3 — End-to-end scenario tests
  Sub-phase 5.4 — Performance and load tests
  Transition gate: 5 → 6

Phase 6 — QA
  Sub-phase 6.1 — Functional review against acceptance criteria
  Sub-phase 6.2 — Regression coverage verification
  Sub-phase 6.3 — Security and compliance review
  Sub-phase 6.4 — Accessibility audit
  Transition gate: 6 → 7

Phase 7 — QA Automation
  Sub-phase 7.1 — Test scenario automation
  Sub-phase 7.2 — Regression suite maintenance
  Sub-phase 7.3 — CI/CD gate configuration
  Sub-phase 7.4 — Coverage and reporting
```

Each transition gate is an experiment opportunity. Each sub-phase is a
pipeline run with its own source document type. Each phase boundary is a
place where the context packet mechanism must be proven to work before
the sub-phases inside that phase can run with confidence.

---

## Part 2 — Document Classification: Which Documents Belong to Which Phase

---

### The Classification Method

A document belongs to a phase if it resolves a problem that the phase must
answer before it can complete its work. The system engineering subject
topology document already organized the XIV knowledge areas by the engineering
problem each resolves. The mapping from those areas to phases is direct.

The classification produces a table with three columns: phase, the knowledge
areas that feed that phase, and the expected context packet output. This table
is the phase registry in its initial form.

---

### Phase-to-Document-Type Mapping

**Phase 1 — UI/UX Definition**

Source document types:
- Design system specifications (tokens, color palettes, typography scales)
- Screen inventory and layout documents
- Interaction model documents (state diagrams, gesture maps)
- Accessibility requirement documents (WCAG level targets, ARIA patterns)
- Motion and animation specifications
- Component library references

Knowledge areas from the topology (Area 7 — Design Intelligence and UI
Understanding): Figma ingestion, DesignIR, ScreenSemanticsIR, module mapping,
system type inference, gap completion.

Context packet output: component hierarchy decisions, design system constraint
set, interaction model decisions, accessibility compliance level, known gaps
in motion and animation specification.

---

**Phase 2 — Functional Specification**

Source document types:
- User story collections
- Acceptance criteria documents
- Business rule catalogs
- Data model boundary documents
- Integration point specifications
- API surface documents (at the level of what is needed, not how)

Knowledge areas: Area 6 (Business Capability Library), Area 9 (Data, Identity,
Security, Tenancy) at the requirements level.

Context packet output: accepted user stories, business rule set, data model
boundary decisions, integration scope decisions, items deferred to technical
design.

---

**Phase 3 — Technical Design**

Source document types:
- Architecture decision records
- Component topology documents
- Data schema and migration plans
- API contract specifications (at the level of what is implemented)
- Security and authorization model documents
- Infrastructure topology documents

Knowledge areas: Area 2 (System Constitution and Non-Negotiable Rules), Area 4
(Flow Runtime and Orchestration), Area 9 (Data, Identity, Security, Tenancy)
at the design level, Area 12 (Deployment, Environment, Operations) at the
design level.

Context packet output: architecture decisions, schema decisions, API contract
commitments, security model decisions, infrastructure decisions, items deferred
to implementation.

---

**Phase 4 — Implementation**

Source document types:
- Session plans (like the continuous learning session plan)
- Implementation guides
- Code review protocols
- Factory and fabric boundary documents

Knowledge areas: Area 8 (Implementation Factories), Area 3 (Dynamic LLM Flow
Pipeline) when implementation involves the pipeline, Area 13 (Platform
Translation) for multi-target implementations.

Context packet output: implemented components inventory, deferred items,
known defects not yet fixed, integration points confirmed and active.

---

**Phase 5 — Testing**

Source document types:
- Test integrity skill documents
- Contract test specifications
- Scenario and acceptance test documents
- Performance baseline documents

Knowledge areas: Area 10 (Quality, Arbiters, Simulation, Gates) at the
test-design level.

Context packet output: test coverage map, failing tests inventory, performance
baseline measurements, untested scenarios.

---

**Phase 6 — QA**

Source document types:
- QA session type documents
- Functional review checklists
- Security and compliance review documents
- Accessibility audit documents

Knowledge areas: Area 10 (Quality, Arbiters, Simulation, Gates) at the
review level, Area 9 (Data, Identity, Security, Tenancy) at the compliance
review level.

Context packet output: QA verdict per acceptance criterion, regression
coverage verdict, compliance status, accessibility audit status.

---

**Phase 7 — QA Automation**

Source document types:
- CI/CD pipeline documents
- Test automation framework documents
- Coverage reporting specifications
- Regression suite maintenance guides

Knowledge areas: Area 12 (Deployment, Environment, Operations), Area 10
(Quality, Arbiters, Simulation, Gates) at the automation level.

Context packet output: automation coverage map, CI gate configuration,
reporting dashboard state, maintenance backlog.

---

## Part 3 — The Transition Experiment

---

### Why One Experiment Covers the Whole Transition Mechanism

All six transitions (1→2, 2→3, 3→4, 4→5, 5→6, 6→7) use the same
transition gate logic. They differ only in the vocabulary of what is being
preserved — UI/UX decisions are different from security model decisions —
but the mechanism is identical: check constraint preservation, decision
acknowledgment, knowledge edge continuity, and rejection preservation.

One well-designed transition experiment proves the mechanism works. If the
mechanism works for one transition, it works for all of them, because the
gate logic is the same and the nine-node pipeline inside each phase is
already proven by E53.

The right transition to test first is Phase 3 → Phase 4 (technical design
to implementation). This transition is chosen because:

The technical design phase produces the most structurally complex context
packet — architecture decisions, schema decisions, API contract commitments,
security model decisions. If the transition gate correctly preserves all of
these into the implementation phase's source packet, it proves the mechanism
handles the most demanding case.

The CONTINUOUS-LEARNING-SESSION-PLAN document already exists as a Phase 4
(implementation) source document. And the architecture documents (DYNAMIC-FLOW-
ARCHITECTURE-PLAN, DYNAMIC-FLOW-HIGH-LEVEL-DESIGN-AMENDMENT) already exist
as Phase 3 (technical design) outputs. The experiment can use real existing
documents rather than synthetic ones.

This transition also already has a known violation ready to inject: the
continuous learning session plan's Principle 2 (BaseService) has no enforcement
mechanism. If this gap was identified in the technical design phase but not
carried into the implementation phase source packet, the transition gate must
catch it. This is a real, documented, known finding from the planning document
binding experiment.

---

### The Transition Experiment Design

**Experiment name:** Phase Transition Gate Examination — Technical Design
to Implementation

**Central question:** Given a technical design phase context packet and an
implementation phase source packet, does the transition gate correctly identify
all four gate conditions — constraint preservation, decision acknowledgment,
knowledge edge continuity, and rejection preservation — or does it miss a
dropped constraint, accept an absent decision acknowledgment, allow a knowledge
edge to disappear silently, or permit a silently reinstated rejected item?

---

**Source document for the technical design phase output (the upstream context
packet):**

The DYNAMIC-FLOW-ARCHITECTURE-PLAN and DYNAMIC-FLOW-HIGH-LEVEL-DESIGN-AMENDMENT
together represent the technical design output for the dynamic flow system.
From these, the upstream context packet contains:

Decisions made:
- The nine-node pipeline is the canonical processing shape
- All units conform to the same external interface (predicate-shape input and output)
- The flow graph is data, not code — stored in a registry, read at runtime
- External AI providers are not part of the architecture (Principle 5)
- Every unit learns from operational feedback (Principle 1)

Constraints inherited (from the locked decisions and CLAUDE.md rules):
- No external ML frameworks (D-11)
- OperationResult-T as the return envelope for all public methods
- BaseService inheritance required for all service classes
- Fabric-first: external dependencies through interfaces only

Knowledge edges:
- Cross-phase context packet mechanism: not yet designed
- Rollback mechanism for trained cells: design complete, implementation not started
- Swappability judge: architecture specified, no training pairs exist

Rejected items:
- Static eight-stage pipeline (replaced by dynamic flow topology)
- Whole-subgraph replacement (cell-level only at this stage)

---

**Source document for the implementation phase source packet (the downstream
source being examined):**

The CONTINUOUS-LEARNING-SESSION-PLAN is the implementation source packet for
the 14.ContinuousLearning component — one part of the dynamic flow
implementation.

---

**The three scenarios:**

Scenario T3→T4-1 — Dropped constraint.

The implementation plan's Principle 3 (Fabric First) correctly preserves the
Fabric-first constraint from the technical design. But Principle 2 (BaseService
inheritance required) has no Gate B enforcement check — the constraint exists
in the plan text but is not enforced. The transition gate must detect that a
technical design constraint (BaseService inheritance is non-negotiable per
CLAUDE.md) is present in the implementation plan as a stated principle but has
no enforcement mechanism. A stated-but-unenforced constraint is a weakened
constraint, and the transition gate must classify it accordingly.

Expected arbiter verdict: CONCERN. The constraint is present in text but not
enforced in the gate checks. The transition gate passes with a named gap.

---

Scenario T3→T4-2 — Absent knowledge edge acknowledgment.

The technical design phase context packet names "cross-phase context packet
mechanism: not yet designed" as a knowledge edge. The implementation plan does
not mention this — it treats the 14.ContinuousLearning component as a complete
component that does not depend on cross-phase coordination. This is correct
for the component's internal scope, but the implementation phase source packet
must acknowledge the knowledge edge and explicitly mark it as out of scope for
this component, not silently absent. The transition gate must detect the
missing acknowledgment.

Expected arbiter verdict: CONCERN. The knowledge edge is not dropped (the
component genuinely does not need it) but the source packet does not
acknowledge that it is out of scope. The correct form is an explicit
out-of-scope marker, not silence.

---

Scenario T3→T4-3 — Reinstated rejected item.

The technical design phase rejected the static eight-stage pipeline. The
implementation plan's component map assigns each component to exactly one
phase in a fixed sequence (Phase 1 through Phase 5). This fixed sequence is
not a dynamic flow topology — it is a static pipeline for this component's
implementation. The transition gate must evaluate whether this static sequence
in the implementation plan contradicts the rejected static pipeline decision
from the technical design phase, or whether it is a different artifact (an
implementation build order rather than a runtime execution topology).

Expected arbiter verdict: PASS. The fixed phase sequence in the implementation
plan is a build-time sequence (what to create in what order), not a runtime
execution topology (how the system runs). The rejected item (static runtime
pipeline) and the implementation build order are different artifact types.
The transition gate must correctly distinguish them rather than flagging the
build order as a rejected item.

---

**Why Scenario T3→T4-3 is the hardest and most important scenario.**

The distinction between a build-time sequence and a runtime execution topology
is exactly the kind of judgment that mechanical checking cannot make. A
mechanical check would see "fixed sequence of phases" and flag it as a
reinstated static pipeline. A careful examination understands that the
implementation plan's phases are a build order — they govern when each C#
class is created, not how the runtime orchestrator will call them. The
transition gate must produce a PASS verdict here, not a false positive CONCERN.
This scenario tests whether the gate can distinguish structural similarity
from semantic equivalence — the same capability that the E53 arbiter
demonstrated when it correctly rejected the duplicate-account request while
correctly accepting the audit traceability request as a legitimate goal.

---

**Oracle documents for Round A:**

Document 1 — Transition Gate Domain Map: defines the vocabulary for phase
transition examination — constraint preservation, decision acknowledgment,
knowledge edge continuity, rejection preservation, stated-but-unenforced
constraint, out-of-scope marker, build-time sequence vs runtime topology.

Document 2 — Transition Gate Failure Taxonomy: defines the failure types —
dropped constraint (BLOCK), stated-but-unenforced constraint (CONCERN),
absent knowledge edge acknowledgment (CONCERN), silent knowledge edge
disappearance (BLOCK), reinstated rejected item (BLOCK if semantic match,
PASS if artifact type differs).

Document 3 — Expected Orchestration Trace: the oracle for the three scenarios.
The most critical section is Scenario T3→T4-3 — the expected PASS verdict
for the build-order-vs-runtime-topology distinction.

Document 4 — Human Result Standards: defines what a passing transition gate
report looks like for a technical lead reviewing whether the implementation
phase is safe to start.

Document 5 — Executor Contract: exact scope for Round B.

---

**Training gap map:**

| Node | Approach | Reason |
|---|---|---|
| 00-01 | Human proxy | Mechanical — phase context packet compilation is deterministic |
| 02 | New pair needed | Transition gate domain vocabulary (constraint preservation, out-of-scope marker, build-time vs runtime) not in prior training |
| 03 | New pair needed | Examination plan with a PASS scenario (Scenario 3) alongside two CONCERN scenarios is a new pattern requiring explicit dependency ordering |
| 04-06 | Human proxy | First pass — content deterministic given the scheme plan |
| 07 | New pair needed | Fire case for Scenario 1 and 2, pass case for Scenario 3 — all new to this domain |
| 08 | New pair needed | PASS verdict for build-order artifact type distinction is the most critical new pair |
| 09 | Human proxy | Existing format sufficient |

---

**Pass gates:**

| Gate | Condition | Failure means |
|---|---|---|
| GATE-T1 | Arbiter output for Scenario 1 names "BaseService" and "no Gate B check" or "enforcement missing" | Stated-but-unenforced constraint not detected. Fix: new Node 8 pair with stated-unenforced-constraint concern verdict |
| GATE-T2 | Arbiter output for Scenario 2 names "out of scope" and "not acknowledged" | Absent acknowledgment not detected. Fix: taxonomy fix — add "absent out-of-scope marker" as named failure type |
| GATE-T3 | Arbiter output for Scenario 3 = PASS with "build-time sequence" reasoning | False positive on reinstated rejection. Fix: Node 8 pair with build-time-vs-runtime-topology pass verdict |
| GATE-T4 | COUNT(BLOCK verdicts) = 0 for all three scenarios | Incorrect severity. Fix: taxonomy content fix — all three scenarios produce CONCERN or PASS, not BLOCK |
| GATE-T5 | Human reviewer confirms the implementation phase is safe to start with two named gaps and one explicit pass | Transition report not actionable. Fix: Node 9 pair for technical lead audience |

---

## Part 4 — How to Extend This to All Transitions

Once the Phase 3→4 transition experiment passes, the same structure scales
to every other transition with minimal additional work.

Each new transition experiment requires:

A new upstream context packet: derived from the existing documents for that
phase. The document classification table in Part 2 names which documents
belong to each phase. The upstream context packet is authored from those
documents before Round A begins.

A new downstream source packet: derived from an existing document that
represents actual work in the downstream phase. The document does not need
to be perfect — the experiment's value comes from finding the gaps between
what the upstream phase decided and what the downstream source packet
acknowledged.

Two new node-02 pairs: the transition gate domain vocabulary differs slightly
per transition because each phase uses different decision vocabulary. The
constraint vocabulary for the UI/UX→functional spec transition involves
design system decisions and interaction model decisions rather than
architecture decisions and API contracts. Two new pairs extend the translator's
vocabulary for each new transition.

One new node-03 pair: the examination plan structure is the same across all
transitions — find constraint preservation failures, find absent
acknowledgments, find knowledge edge continuity breaks, find reinstated
rejections. The node-03 pair for each new transition can reuse the same
plan structure with updated vocabulary.

New node-08 pairs only when a new verdict type appears: the three verdict
types (PASS, CONCERN, BLOCK) are the same across all transitions. New pairs
are needed only when a new failure type appears that the existing pairs do
not cover.

The transition from Phase 1→2 (UI/UX to functional spec) is the second
transition to test after Phase 3→4, because it introduces the design system
vocabulary and the interaction model vocabulary that are the furthest from
the current corpus. If the mechanism works for the most complex vocabulary
(Phase 3→4) and the most novel vocabulary (Phase 1→2), it works for all
intermediate transitions.

---

## Summary

Three additions to the architecture: the phase context packet, the transition
gate, and the phase registry. One experiment to prove the transition mechanism
works. Six subsequent experiments to extend coverage to all phase boundaries.
Each experiment builds on the same structure, the same document classification
table, and the same five oracle documents. The mechanism scales from two
phases to twenty-eight sub-phases without any change to the nine-node
pipeline — only the source packet structure and the transition gate vocabulary
expand.

The corpus expansion required is modest: two new node-02 pairs per transition,
one node-03 pair per transition, new node-08 pairs only when new verdict types
appear. The current examination domain pairs from the five training pair
experiments provide the base vocabulary. The transition experiments extend
that vocabulary to the cross-phase domain.
