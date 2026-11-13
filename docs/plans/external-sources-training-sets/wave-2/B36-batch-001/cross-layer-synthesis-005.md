# Cross-Layer Synthesis (005)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: cross_layer_synthesis

---

## B36-CLS-005-001 — Pure gap "run our sprint planning": full layer trace

**Request:** "Run our sprint planning ceremony and estimate the backlog for next iteration."

**Layer 0-A — Compiler:**

Token scan:
- run: no predicate (not a registered keyword)
- sprint: GAP → sprint-001
- planning: no predicate (Rule 17: "plan" and variants not predicates)
- ceremony: no predicate
- estimate: no predicate (not a registered keyword)
- backlog: GAP → backlog-001
- iteration: GAP → iteration-001

Predicate output: `[]`
CapabilityGapWarnings: sprint-001, backlog-001, iteration-001

---

**Layer 0-B — Router:**

predicates: [] → KNOWLEDGE_EDGE path selected
CapabilityGapWarnings queued: sprint-001, backlog-001, iteration-001
DomainId: "project-management" (no predicate fired)
TransitionType: unknown
PhaseId: null

---

**Layer 0-C — Knowledge-Edge:**

KNOWLEDGE_EDGE state confirmed
Gap IDs: sprint-001, backlog-001, iteration-001
Compound ID eligible: gap-project-planning-sprint-backlog-001 (sprint + backlog co-occur)
All gap IDs: sprint-001, backlog-001, iteration-001, sprint-backlog-001

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
  "capabilityGapIds": [
    "gap-project-planning-sprint-001",
    "gap-project-planning-backlog-001",
    "gap-project-planning-iteration-001",
    "gap-project-planning-sprint-backlog-001"
  ]
}
```

---

**Layer 2 — Gap Extended:**

Gap report for sprint-001: sprint planning ceremony capability not available
Gap report for backlog-001: backlog estimation capability not available
Gap report for iteration-001: iteration planning capability not available
Gap report for sprint-backlog-001: integrated sprint-backlog planning capability not available

---

**Layer 3 — Teaching:**

Key rules demonstrated:
- Rule 17: "planning" is not a predicate; "run" is not a predicate
- Rule 1: sprint, backlog, iteration are gap vocabulary
- Rule 8: TransitionType unknown, PhaseId null for KNOWLEDGE_EDGE
- Rule 9: SCHEDULE_CAPABILITY_LEARNING emitted
- Rule 22: one signal per request, all gap IDs bundled
- DomainId rule: zero predicates → "project-management" unconditionally

---

## B36-CLS-005-002 — Trace summary

| Stage | Output |
|-------|--------|
| Compiler | predicates: [] |
| Router | KNOWLEDGE_EDGE; DomainId: project-management |
| Factory | not called |
| Orchestrator | SCHEDULE_CAPABILITY_LEARNING; FinalHumanResult: null |
| Gap extended | sprint-001, backlog-001, iteration-001, sprint-backlog-001 reported |
| Teaching | Rules 17, 1, 8, 9, 22 applied; DomainId: project-management confirmed |

---

## B36-CLS-005-003 — Pure gap canonical case

This is the canonical B36 pure-gap case. It demonstrates:
1. A request containing only gap vocabulary and non-predicate verbs
2. Zero predicates firing → knowledge_edge path unconditionally
3. DomainId: "project-management" (predicate absence rule)
4. SCHEDULE_CAPABILITY_LEARNING emitted with all gap IDs bundled
5. No FinalHumanResult produced
6. ContextCarryForward: false

A model that correctly handles this case understands the B36 gap boundary:
no planning ceremony vocabulary maps to a registered predicate.
