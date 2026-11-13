# Layer 3 — Teaching Unit Pairs (006)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B36-L3-006-001 — SCHEDULE_CAPABILITY_LEARNING: complete explanation

**Teaching objective:** Model learns exactly when SCHEDULE_CAPABILITY_LEARNING is emitted, what it contains, and what it triggers.

**Trigger:** predicates: [] AND gap vocabulary present in request

**Contents:**
```json
{
  "signal": "SCHEDULE_CAPABILITY_LEARNING",
  "capabilityGapIds": ["<all gap IDs from request>"],
  "domainId": "project-management"
}
```

**Consumer:** AutonomousLearningPipeline (subproject 16)

---

## B36-L3-006-002 — SCHEDULE_CAPABILITY_LEARNING: what it does NOT do

| Action | Does SCHEDULE_CAPABILITY_LEARNING do this? |
|--------|-------------------------------------------|
| Retry the current request | NO |
| Reroute the current request | NO |
| Fill the capability gap immediately | NO |
| Block the response | NO |
| Change the routing for future requests | NO (AutonomousLearningPipeline may eventually do this) |
| Cause the orchestrator to wait | NO |

SCHEDULE_CAPABILITY_LEARNING is a fire-and-forget signal. The orchestrator emits it, continues, and returns FinalHumanResult: null. The current user gets a null result.

---

## B36-L3-006-003 — SCHEDULE_CAPABILITY_LEARNING: teaching pairs

**Pair A — emitted:**

```
Input: "Plan the sprint backlog."
Predicates: []
SCHEDULE_CAPABILITY_LEARNING: emitted (one signal, capabilityGapIds: [sprint-001, backlog-001])
FinalHumanResult: null
```

**Pair B — NOT emitted:**

```
Input: "Review the sprint backlog."
Predicates: ["QA"]
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
FinalHumanResult: populated by QAGatingFlow
```

**Pair C — emitted once for four gap terms:**

```
Input: "Plan the sprint, groom the backlog, run the standup, track the velocity."
Predicates: []
SCHEDULE_CAPABILITY_LEARNING: ONE signal with capabilityGapIds: [sprint-001, backlog-001, standup-001, velocity-001]
```

---

## B36-L3-006-004 — SCHEDULE_CAPABILITY_LEARNING frequency and accumulation

Each time a B36 knowledge_edge request occurs, one SCHEDULE_CAPABILITY_LEARNING signal fires with the gap IDs from that request. Over time:
- High-frequency gap IDs accumulate more signals
- gap-project-planning-sprint-001 is expected to have the highest signal frequency (sprint appears in most B36 planning requests)
- The AutonomousLearningPipeline uses frequency to prioritize which gaps to develop capabilities for first
- This is why accurate signal emission matters: under-emitting means underestimating gap frequency; over-emitting (multiple per request) means inflating counts
