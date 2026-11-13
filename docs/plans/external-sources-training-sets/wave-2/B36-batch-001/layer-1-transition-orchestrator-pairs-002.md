# Layer 1 — Transition Orchestrator Pairs (002)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B36-L1-002-001 — knowledge_edge path: TransitionType unknown, ContextCarryForward false, no flow

**Input:** `predicates: []`, gap vocabulary present
**Request origin:** "Groom the product backlog and prioritize the tickets for the next iteration."

**Orchestrator state for knowledge_edge:**

```json
{
  "transitionType": "unknown",
  "contextCarryForward": false,
  "phaseId": null,
  "flowSelected": null,
  "finalHumanResult": null,
  "domainId": "project-management",
  "knowledgeEdge": true,
  "scheduledSignals": [
    {
      "signal": "SCHEDULE_CAPABILITY_LEARNING",
      "capabilityGapIds": [
        "gap-project-planning-backlog-001",
        "gap-project-planning-iteration-001",
        "gap-project-planning-ticket-001"
      ]
    }
  ]
}
```

---

## B36-L1-002-002 — ContextCarryForward is false for knowledge_edge

ContextCarryForward false means: the context of this request is not carried forward to the next orchestration step. There is no next step — the request terminates at KNOWLEDGE_EDGE.

In normal (predicate-fired) routing, ContextCarryForward may be true to pass context between phases (e.g., IMPLEMENT context passed to QA gate). In KNOWLEDGE_EDGE, there is no subsequent phase, so ContextCarryForward is always false.

---

## B36-L1-002-003 — SCHEDULE_CAPABILITY_LEARNING emission at orchestrator stage

The orchestrator is the stage that emits SCHEDULE_CAPABILITY_LEARNING. The signal is constructed at orchestration time:

1. Orchestrator receives factory result: null (KNOWLEDGE_EDGE)
2. Orchestrator confirms: predicates: [], knowledgeEdge: true
3. Orchestrator builds SCHEDULE_CAPABILITY_LEARNING signal with all gap IDs from CapabilityGapWarnings
4. Orchestrator emits signal to AutonomousLearningPipeline (subproject 16)
5. Orchestrator sets FinalHumanResult: null
6. Orchestrator returns orchestration result

---

## B36-L1-002-004 — knowledge_edge orchestration result structure

```json
{
  "orchestrationResult": {
    "requestId": "<request-id>",
    "transitionType": "unknown",
    "contextCarryForward": false,
    "phaseId": null,
    "domainId": "project-management",
    "finalHumanResult": null,
    "knowledgeEdge": true,
    "capabilityGapWarnings": [
      "gap-project-planning-backlog-001",
      "gap-project-planning-iteration-001",
      "gap-project-planning-ticket-001"
    ],
    "scheduledSignals": ["SCHEDULE_CAPABILITY_LEARNING"],
    "recommendedAction": "SCHEDULE_CAPABILITY_LEARNING"
  }
}
```
