# Layer 0-D — Flow Factory Pairs (008)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B36-0D-008-001 — IMPLEMENT+DOCUMENT+QA factory: all three; QA last

**Input predicate list:** `["IMPLEMENT", "DOCUMENT", "QA"]`
**Request origin:** "Build the sprint tracking service, document the backlog requirements, and review the velocity reports."

**Factory selection process:**

```
predicates contains IMPLEMENT? YES → BackendServiceFlow41 (Phase 4.1)
predicates contains DOCUMENT? YES → DocumentationFlow (Phase 2.1)
predicates contains QA? YES → QAGatingFlow (QA sub-phase) — ALWAYS LAST
→ Three factories selected
→ Execution: 4.1 parallel with 2.1, then QA gate
→ TransitionType: feature_addition
```

**Factory output:**
- Flow 1: BackendServiceFlow41 (Phase 4.1, primary)
- Flow 2: DocumentationFlow (Phase 2.1, parallel)
- Flow 3: QAGatingFlow (QA sub-phase, last)
- CapabilityGapWarnings: sprint-001, backlog-001, velocity-001

---

## B36-0D-008-002 — three-factory execution sequence

```
Step 1: BackendServiceFlow41 (4.1)  ─┐
Step 2: DocumentationFlow (2.1)      ├─ parallel
                                    ─┘
Step 3: QAGatingFlow (QA)           ← after both complete; validates their output
```

QAGatingFlow runs last because it validates the outputs of the IMPLEMENT and DOCUMENT phases. It cannot validate what has not yet been produced.

---

## B36-0D-008-003 — three-factory result structure

```json
{
  "factoryResults": [
    { "flow": "BackendServiceFlow41", "phase": "4.1", "primary": true },
    { "flow": "DocumentationFlow", "phase": "2.1", "primary": false },
    { "flow": "QAGatingFlow", "phase": "QA", "primary": false, "executionOrder": "last" }
  ],
  "domainId": "software-development",
  "capabilityGapWarnings": [
    "gap-project-planning-sprint-001",
    "gap-project-planning-backlog-001",
    "gap-project-planning-velocity-001"
  ],
  "scheduledSignals": []
}
```

---

## B36-0D-008-004 — IMPLEMENT+DOCUMENT+QA: no SCHEDULE_CAPABILITY_LEARNING

Three predicates fired. SCHEDULE_CAPABILITY_LEARNING is NOT emitted. The scheduledSignals array is empty. CapabilityGapWarnings are present for sprint, backlog, velocity — these flow to AutonomousLearningPipeline via the warning channel, not the SCHEDULE_CAPABILITY_LEARNING channel.

The distinction: SCHEDULE_CAPABILITY_LEARNING means "no routing happened, queue learning." CapabilityGapWarnings mean "routing happened successfully but gap vocabulary was noted." Both reach the AutonomousLearningPipeline but trigger different pipeline behaviors.
