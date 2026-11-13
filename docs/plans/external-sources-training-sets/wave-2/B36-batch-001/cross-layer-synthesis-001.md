# Cross-Layer Synthesis (001)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## B36-CLS-001-001 — Zero predicate path: full layer trace

**Request:** "Plan the sprint and update the roadmap."

**Layer 0-A — Compiler:**

Token scan:
- plan: no predicate (Rule 17)
- sprint: GAP → sprint-001
- update: no code-level co-occurrence (Rule 12) → no predicate
- roadmap: GAP → roadmap-001

Predicate output: `[]`

---

**Layer 0-B — Router:**

predicates: [] → KNOWLEDGE_EDGE path selected
CapabilityGapWarnings queued: sprint-001, roadmap-001
DomainId: "project-management" (no predicate fired)
TransitionType: unknown
PhaseId: null

---

**Layer 0-C — Knowledge-Edge:**

KNOWLEDGE_EDGE state confirmed
Gap IDs: sprint-001, roadmap-001
Compound ID eligible: gap-project-planning-sprint-roadmap-001 (sprint + roadmap co-occur)
All gap IDs: sprint-001, roadmap-001, sprint-roadmap-001

---

**Layer 0-D — Factory:**

Not called (KNOWLEDGE_EDGE bypasses factory)
Flow: null

---

**Layer 1 — Transition Orchestrator:**

transitionType: unknown
contextCarryForward: false
phaseId: null
domainId: "project-management"
finalHumanResult: null
SCHEDULE_CAPABILITY_LEARNING emitted:
```json
{
  "signal": "SCHEDULE_CAPABILITY_LEARNING",
  "capabilityGapIds": ["gap-project-planning-sprint-001", "gap-project-planning-roadmap-001", "gap-project-planning-sprint-roadmap-001"]
}
```

---

**Layer 2 — Gap Extended:**

Gap report for sprint-001: sprint planning capability not available
Gap report for roadmap-001: roadmap planning capability not available
Gap report for sprint-roadmap-001: integrated sprint-to-roadmap alignment not available

---

**Layer 3 — Teaching:**

Key rules demonstrated:
- Rule 17: "plan" not a predicate
- Rule 12: "update" without code-level entity not a predicate
- Rule 1: sprint, roadmap are gap vocabulary
- Rule 8: TransitionType unknown, PhaseId null for KNOWLEDGE_EDGE
- Rule 9: SCHEDULE_CAPABILITY_LEARNING emitted
- Rule 22: one signal, all gap IDs bundled

---

## B36-CLS-001-002 — Trace summary

| Stage | Output |
|-------|--------|
| Compiler | predicates: [] |
| Router | KNOWLEDGE_EDGE; DomainId: project-management |
| Factory | not called |
| Orchestrator | SCHEDULE_CAPABILITY_LEARNING; FinalHumanResult: null |
| Gap extended | sprint-001, roadmap-001, sprint-roadmap-001 reported |
| Teaching | Rules 17, 12, 1, 8, 9, 22 applied |
