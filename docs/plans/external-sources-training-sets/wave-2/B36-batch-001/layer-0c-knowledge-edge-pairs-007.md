# Layer 0-C — Knowledge-Edge Pairs (007)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B36-0C-007-001 — SCHEDULE_CAPABILITY_LEARNING: what triggers it

**Trigger condition (B36 Rule 9):**
SCHEDULE_CAPABILITY_LEARNING is emitted when:
1. predicates: [] (zero predicates fired)
2. Gap vocabulary present in the request

**Not triggered when:**
- Any predicate fires (even one), regardless of gap vocabulary
- predicates: [] but no gap vocabulary recognized

---

## B36-0C-007-002 — what SCHEDULE_CAPABILITY_LEARNING means

**SCHEDULE_CAPABILITY_LEARNING signal contents:**

```json
{
  "signal": "SCHEDULE_CAPABILITY_LEARNING",
  "requestId": "<current-request-id>",
  "capabilityGapIds": [
    "gap-project-planning-sprint-001",
    "gap-project-planning-backlog-001"
  ],
  "domainId": "project-management",
  "requestSummary": "<sanitized-request-text>",
  "timestamp": "<ISO-8601>"
}
```

**What the signal triggers (B36 Rule 10):**
- AutonomousLearningPipeline (subproject 16) receives the signal
- The pipeline logs the gap and queues a learning task for future capability expansion
- The current request is NOT retried
- The current response is FinalHumanResult: null (the gap is not filled in real time)

---

## B36-0C-007-003 — SCHEDULE_CAPABILITY_LEARNING: one per request (B36 Rule 22)

**Incorrect behavior:**
- Emitting one SCHEDULE_CAPABILITY_LEARNING per gap term found (e.g., four gap terms → four signals)

**Correct behavior:**
- One SCHEDULE_CAPABILITY_LEARNING per request
- All gap IDs from the request are bundled in the single signal's capabilityGapIds array

**Example:**
Request: "Plan the sprint, groom the backlog, run the standup, and track the velocity."
Gap IDs found: sprint-001, backlog-001, standup-001, velocity-001
SCHEDULE_CAPABILITY_LEARNING signals emitted: **1** (one, with all four IDs in the array)

---

## B36-0C-007-004 — SCHEDULE_CAPABILITY_LEARNING vs CapabilityGapWarnings

| Signal | When emitted | Consumer | Purpose |
|--------|-------------|----------|---------|
| SCHEDULE_CAPABILITY_LEARNING | predicates: [] only | AutonomousLearningPipeline | Queue learning task |
| CapabilityGapWarning (per gap ID) | Any time gap vocab present | RouterResult, AutonomousLearningPipeline | Log gap occurrence |

**Key difference:** CapabilityGapWarnings are always emitted when gap vocabulary is present (whether predicates fired or not). SCHEDULE_CAPABILITY_LEARNING is only emitted when predicates: []. Both signals go to AutonomousLearningPipeline but via different channels and with different semantics.
