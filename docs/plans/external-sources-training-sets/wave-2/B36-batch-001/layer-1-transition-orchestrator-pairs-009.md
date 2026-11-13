# Layer 1 — Transition Orchestrator Pairs (009)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B36-L1-009-001 — One SCHEDULE_CAPABILITY_LEARNING per request (not per gap term)

**Scenario:** Request contains four gap terms.

**Input:** "Plan the sprint, groom the backlog, run the standup, and track the velocity."
**Gap terms found:** sprint (sprint-001), backlog (backlog-001), standup (standup-001), velocity (velocity-001)

**Incorrect behavior (DO NOT reproduce):**
```
Emit SCHEDULE_CAPABILITY_LEARNING for sprint-001
Emit SCHEDULE_CAPABILITY_LEARNING for backlog-001
Emit SCHEDULE_CAPABILITY_LEARNING for standup-001
Emit SCHEDULE_CAPABILITY_LEARNING for velocity-001
= Four signals emitted
```

**Correct behavior:**
```
Emit ONE SCHEDULE_CAPABILITY_LEARNING with capabilityGapIds array containing all four IDs
= One signal emitted
```

---

## B36-L1-009-002 — Correct SCHEDULE_CAPABILITY_LEARNING structure for multi-gap request

```json
{
  "signal": "SCHEDULE_CAPABILITY_LEARNING",
  "requestId": "<request-id>",
  "capabilityGapIds": [
    "gap-project-planning-sprint-001",
    "gap-project-planning-backlog-001",
    "gap-project-planning-standup-001",
    "gap-project-planning-velocity-001"
  ],
  "domainId": "project-management",
  "timestamp": "<ISO-8601>"
}
```

One signal. All four gap IDs bundled in the array.

---

## B36-L1-009-003 — Why one signal per request

The AutonomousLearningPipeline (subproject 16) is designed to receive one signal per request and process all gap IDs in that request together. Multiple signals per request would:
1. Create duplicate processing burden
2. Inflate signal counts incorrectly
3. Potentially cause the pipeline to process the same request multiple times

One signal per request maintains accurate frequency tracking for each gap ID across multiple requests.

---

## B36-L1-009-004 — Signal count in orchestration metadata

The orchestration metadata records scheduledSignals as an array. For a multi-gap knowledge_edge request:

```json
{
  "scheduledSignals": ["SCHEDULE_CAPABILITY_LEARNING"],
  "scheduledSignalCount": 1
}
```

The count is always 1 for knowledge_edge, regardless of how many gap terms were present. This count is auditable for correctness testing.
