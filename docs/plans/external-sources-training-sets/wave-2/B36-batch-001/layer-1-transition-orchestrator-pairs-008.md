# Layer 1 — Transition Orchestrator Pairs (008)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B36-L1-008-001 — IMPLEMENT+DOCUMENT+QA: feature_addition; QA last

**Input:** `predicates: ["IMPLEMENT", "DOCUMENT", "QA"]`
**Request origin:** "Build the sprint tracking service, document the backlog requirements, and review the velocity reports."

**Orchestrator state:**

```json
{
  "transitionType": "feature_addition",
  "contextCarryForward": false,
  "phases": [
    { "phaseId": "4.1", "flow": "BackendServiceFlow41", "executionOrder": 1, "parallel": true },
    { "phaseId": "2.1", "flow": "DocumentationFlow", "executionOrder": 1, "parallel": true },
    { "phaseId": "QA", "flow": "QAGatingFlow", "executionOrder": 2, "parallel": false }
  ],
  "domainId": "software-development",
  "knowledgeEdge": false,
  "capabilityGapWarnings": [
    "gap-project-planning-sprint-001",
    "gap-project-planning-backlog-001",
    "gap-project-planning-velocity-001"
  ],
  "scheduledSignals": []
}
```

---

## B36-L1-008-002 — QA last constraint in orchestration

The orchestrator enforces QA-last ordering as a hard constraint. It is not configurable. When QA is in the predicate list alongside IMPLEMENT and/or DOCUMENT:

1. IMPLEMENT (4.1) and DOCUMENT (2.1) run in parallel (executionOrder: 1)
2. QA (QA sub-phase) runs after both complete (executionOrder: 2)
3. QA validates the outputs of both 4.1 and 2.1

If QA ran before 4.1 or 2.1, it would have nothing to validate. The ordering constraint is structural.

---

## B36-L1-008-003 — ContextCarryForward between phases

When QA follows IMPLEMENT and DOCUMENT, ContextCarryForward is true between the phases (4.1 output passes to QA, 2.1 output passes to QA). But ContextCarryForward in the top-level orchestration result refers to whether context is carried to the *next request*, not between internal phases. That is false — the orchestration is self-contained.

---

## B36-L1-008-004 — IMPLEMENT+DOCUMENT+QA orchestration result structure

```json
{
  "orchestrationResult": {
    "transitionType": "feature_addition",
    "contextCarryForward": false,
    "phases": ["4.1", "2.1", "QA"],
    "executionModel": "parallel_4.1_2.1_then_QA",
    "domainId": "software-development",
    "finalHumanResult": "<merged: BackendServiceFlow41 + DocumentationFlow + QAGatingFlow>",
    "knowledgeEdge": false,
    "capabilityGapWarnings": [
      "gap-project-planning-sprint-001",
      "gap-project-planning-backlog-001",
      "gap-project-planning-velocity-001"
    ],
    "scheduledSignals": []
  }
}
```
