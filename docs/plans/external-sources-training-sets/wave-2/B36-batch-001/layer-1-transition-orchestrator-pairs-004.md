# Layer 1 — Transition Orchestrator Pairs (004)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B36-L1-004-001 — IMPLEMENT in sprint context: feature_addition; DomainId: software-development

**Input:** `predicates: ["IMPLEMENT"]`
**Request origin:** "Implement the features from Epic E-14 and close the sprint tickets when complete."

**Orchestrator state:**

```json
{
  "transitionType": "feature_addition",
  "contextCarryForward": false,
  "phaseId": "4.1",
  "flowSelected": "BackendServiceFlow41",
  "finalHumanResult": "<from BackendServiceFlow41>",
  "domainId": "software-development",
  "knowledgeEdge": false,
  "capabilityGapWarnings": [
    "gap-project-planning-epic-001",
    "gap-project-planning-sprint-001",
    "gap-project-planning-ticket-001"
  ],
  "scheduledSignals": []
}
```

---

## B36-L1-004-002 — feature_addition TransitionType in sprint context

TransitionType "feature_addition" means the orchestration produced a feature implementation. The sprint/epic/ticket context is noted in gap warnings but does not change the TransitionType. The implementation proceeds as feature_addition.

This is the most common mixed-signal case in B36: a user wants to implement something that is listed in a sprint/epic/backlog. IMPLEMENT fires on the implementation verb; the sprint vocabulary produces warnings. The feature_addition TransitionType reflects the implementation work happening.

---

## B36-L1-004-003 — IMPLEMENT orchestration: nine-node flow executes

When IMPLEMENT fires and BackendServiceFlow41 is selected, the nine-node flow executes. The nine nodes run in their standard sequence. The sprint/epic gap warnings are logged but do not affect the nine-node flow structure.

**Nine-node flow execution in sprint context:**
- All nine nodes execute normally
- CapabilityGapWarnings are attached to the final orchestration result
- The flow does not have special "sprint mode" or "epic mode"

---

## B36-L1-004-004 — IMPLEMENT orchestration result structure

```json
{
  "orchestrationResult": {
    "transitionType": "feature_addition",
    "contextCarryForward": false,
    "phaseId": "4.1",
    "domainId": "software-development",
    "finalHumanResult": "<populated by BackendServiceFlow41>",
    "knowledgeEdge": false,
    "capabilityGapWarnings": [
      "gap-project-planning-epic-001",
      "gap-project-planning-sprint-001",
      "gap-project-planning-ticket-001"
    ],
    "scheduledSignals": [],
    "nineNodeFlowCompleted": true
  }
}
```
