# Layer 1 — Transition Orchestrator Pairs (007)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B36-L1-007-001 — IMPLEMENT+DOCUMENT: feature_addition; parallel eligible

**Input:** `predicates: ["IMPLEMENT", "DOCUMENT"]`
**Request origin:** "Write the user stories for the checkout feature and estimate the story points."

**Orchestrator state:**

```json
{
  "transitionType": "feature_addition",
  "contextCarryForward": false,
  "parallelEligible": true,
  "phases": [
    { "phaseId": "4.1", "flow": "BackendServiceFlow41", "order": 1 },
    { "phaseId": "2.1", "flow": "DocumentationFlow", "order": 1 }
  ],
  "domainId": "software-development",
  "knowledgeEdge": false,
  "capabilityGapWarnings": [
    "gap-project-planning-story-point-001",
    "gap-project-planning-estimation-001"
  ],
  "scheduledSignals": []
}
```

---

## B36-L1-007-002 — TransitionType when IMPLEMENT and DOCUMENT both fire

When both IMPLEMENT and DOCUMENT fire:
- TransitionType: feature_addition (IMPLEMENT is primary)
- DOCUMENT contributes to the result but does not change the TransitionType from documentation to feature_addition — it is already feature_addition from IMPLEMENT
- The merged result contains both implementation code and documentation artifacts

---

## B36-L1-007-003 — parallel execution orchestration

The orchestrator schedules BackendServiceFlow41 (4.1) and DocumentationFlow (2.1) as parallel execution units. They run simultaneously. The orchestrator waits for both to complete, then merges their results into a single FinalHumanResult.

No QA gate follows unless QA was in the predicate list. In this case ([IMPLEMENT, DOCUMENT] without QA), no QA gate follows.

---

## B36-L1-007-004 — IMPLEMENT+DOCUMENT orchestration result structure

```json
{
  "orchestrationResult": {
    "transitionType": "feature_addition",
    "contextCarryForward": false,
    "parallelEligible": true,
    "phases": ["4.1", "2.1"],
    "domainId": "software-development",
    "finalHumanResult": "<merged from BackendServiceFlow41 + DocumentationFlow>",
    "knowledgeEdge": false,
    "capabilityGapWarnings": [
      "gap-project-planning-story-point-001",
      "gap-project-planning-estimation-001"
    ],
    "scheduledSignals": []
  }
}
```
