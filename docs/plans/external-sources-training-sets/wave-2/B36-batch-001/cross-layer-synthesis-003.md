# Cross-Layer Synthesis (003)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## B36-CLS-003-001 — False positive DOCUMENT: "Estimate story points for backlog"

**Request:** "Estimate the story points for the backlog items this sprint."

**Layer 0-A — Compiler:**

Token scan:
- estimate: GAP → estimation-001
- story: DOCUMENT fires (Rule 2 — false positive)
- points: no predicate
- backlog: GAP → backlog-001
- items: no predicate
- sprint: GAP → sprint-001

Predicate output: `["DOCUMENT"]`
CapabilityGapWarnings queued: story-point-001, estimation-001, backlog-001, sprint-001

---

**Layer 0-B — Router:**

predicates: ["DOCUMENT"] → Phase 2.1
DomainId: "software-development"
KNOWLEDGE_EDGE: false
CapabilityGapWarnings forwarded

---

**Layer 0-C — Knowledge-Edge:**

Not in KNOWLEDGE_EDGE state (DOCUMENT fired)
Noting: story fires DOCUMENT on estimation-context request — known false positive
Gap warnings: story-point-001 (estimation intent not served by documentation routing)

---

**Layer 0-D — Factory:**

FlowFactory.Select(DOCUMENT) → DocumentationFlow
Phase: 2.1
TransitionType: documentation

---

**Layer 1 — Transition Orchestrator:**

transitionType: documentation
phaseId: "2.1"
domainId: "software-development"
finalHumanResult: populated by DocumentationFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
capabilityGapWarnings: story-point-001, estimation-001, backlog-001, sprint-001

AutonomousLearningPipeline notified of story-point-001 and estimation-001 via CapabilityGapWarnings channel.

---

**Layer 2 — Gap Extended:**

False positive: "story" fires DOCUMENT — user intended estimation
Gap ID story-point-001 records: user wanted story point estimation; DOCUMENT routing does not serve this
Gap ID estimation-001: estimation capability not available
Gap IDs backlog-001 and sprint-001: planning context noted

---

**Layer 3 — Teaching:**

Key rules demonstrated:
- Rule 2: "story" fires DOCUMENT — false positive; model must not suppress
- Rule 7: DomainId "software-development" when DOCUMENT fires
- Rule 11: gap vocabulary (estimation, backlog, sprint) produces warnings only
- No SCHEDULE_CAPABILITY_LEARNING because predicate fired
- CapabilityGapWarnings reach AutonomousLearningPipeline via warning channel

---

## B36-CLS-003-002 — Trace summary

| Stage | Output |
|-------|--------|
| Compiler | predicates: ["DOCUMENT"] (false positive on "story") |
| Router | Phase 2.1; DomainId: software-development |
| Factory | DocumentationFlow |
| Orchestrator | documentation; FinalHumanResult: populated |
| Gap extended | story-point-001, estimation-001, backlog-001, sprint-001 (warnings) |
| Teaching | Rules 2, 7, 11 applied; false positive explained |
