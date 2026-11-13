# Cross-Layer Synthesis (002)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## B36-CLS-002-001 — Mixed signal IMPLEMENT: "Sprint to implement the checkout feature"

**Request:** "This sprint, implement the checkout feature and close the backlog tickets."

**Layer 0-A — Compiler:**

Token scan:
- sprint: GAP → sprint-001
- implement: IMPLEMENT fires
- checkout: no predicate
- feature: IMPLEMENT fires
- close: no predicate
- backlog: GAP → backlog-001
- tickets: GAP → ticket-001

Predicate output: `["IMPLEMENT"]`
CapabilityGapWarnings queued: sprint-001, backlog-001, ticket-001

---

**Layer 0-B — Router:**

predicates: ["IMPLEMENT"] → Phase 4.1
DomainId: "software-development" (IMPLEMENT fired)
KNOWLEDGE_EDGE: false
CapabilityGapWarnings: sprint-001, backlog-001, ticket-001

---

**Layer 0-C — Knowledge-Edge:**

Not in KNOWLEDGE_EDGE state (IMPLEMENT fired)
Gap warnings forwarded as metadata

---

**Layer 0-D — Factory:**

FlowFactory.Select(IMPLEMENT) → BackendServiceFlow41
Phase: 4.1
TransitionType: feature_addition

---

**Layer 1 — Transition Orchestrator:**

transitionType: feature_addition
phaseId: "4.1"
domainId: "software-development"
finalHumanResult: populated by BackendServiceFlow41
SCHEDULE_CAPABILITY_LEARNING: NOT emitted (predicate fired)
capabilityGapWarnings: sprint-001, backlog-001, ticket-001

---

**Layer 2 — Gap Extended:**

Mixed-signal: IMPLEMENT fired alongside planning vocabulary
sprint-001: sprint context noted but not served (sprint-not-captured mixed-signal form)
backlog-001: backlog context noted
ticket-001: ticket tracking not available

---

**Layer 3 — Teaching:**

Key rules demonstrated:
- Rule 6: "implement" and "feature" fire IMPLEMENT
- Rule 7: DomainId "software-development" when any predicate fires
- Rule 11: predicate dominates; gap vocab produces warnings only
- Rule 20: no KNOWLEDGE_EDGE when predicate fires
- B36 Rule: SCHEDULE_CAPABILITY_LEARNING NOT emitted when predicate fired

---

## B36-CLS-002-002 — Trace summary

| Stage | Output |
|-------|--------|
| Compiler | predicates: ["IMPLEMENT"] |
| Router | Phase 4.1; DomainId: software-development |
| Factory | BackendServiceFlow41 |
| Orchestrator | feature_addition; FinalHumanResult: populated |
| Gap extended | sprint-not-captured, backlog-001, ticket-001 (warnings) |
| Teaching | Rules 6, 7, 11, 20 applied |
