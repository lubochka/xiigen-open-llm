# Layer 0-B — Router Pairs (010)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: router

---

## B36-0B-010-001 — [IMPLEMENT, DOCUMENT, QA] → 4.1 + 2.1 + QA sub-phase

**Input predicate list:** `["IMPLEMENT", "DOCUMENT", "QA"]`
**Request origin:** "Build the sprint tracking service, document the backlog requirements, and review the velocity reports."

**Router decision tree:**

```
predicates contains IMPLEMENT? YES → Phase 4.1
predicates contains DOCUMENT? YES → Phase 2.1
predicates contains QA? YES → QA sub-phase (LAST)
→ Execution order: 4.1 parallel with 2.1, then QA sub-phase
→ TransitionType: feature_addition (IMPLEMENT is primary)
→ DomainId: "software-development"
→ CapabilityGapWarnings: sprint-001, backlog-001, velocity-001
```

**Router output:**
- Primary path: Phase 4.1 — BackendServiceFlow41
- Secondary path: Phase 2.1 — DocumentationFlow (parallel with 4.1)
- Final gate: QA sub-phase (after 4.1 and 2.1 complete)
- TransitionType: feature_addition
- CapabilityGapWarnings: sprint-001, backlog-001, velocity-001

---

## B36-0B-010-002 — three-predicate routing: QA always executes last

**Execution sequence for [IMPLEMENT, DOCUMENT, QA]:**

```
Step 1: Phase 4.1 (BackendServiceFlow41)    ─┐
Step 2: Phase 2.1 (DocumentationFlow)        ├─ parallel
         ↓                                  ─┘
Step 3: QA sub-phase (gating)               ← always last
```

**Why QA is last:** QA validates the outputs of IMPLEMENT and DOCUMENT. It cannot run before the artifacts it validates exist. This ordering is a fixed invariant — not configurable per request.

---

## B36-0B-010-003 — three-predicate routing with heavy planning vocab

**Input predicate list:** `["IMPLEMENT", "DOCUMENT", "QA"]`
**Request origin:** "Implement the epic features, specify the sprint documentation, and review readiness."

**Gap vocabulary present:** epic, sprint
**CapabilityGapWarnings emitted:** epic-001, sprint-001

**Router decision:** Same routing structure as B36-0B-010-001. Heavy planning vocabulary does not change the three-predicate execution sequence. Gap warnings are attached and forwarded to AutonomousLearningPipeline.

---

## B36-0B-010-004 — three-predicate routing summary

**When all three predicates fire:**
- IMPLEMENT + DOCUMENT + QA present → full multi-phase execution
- DomainId is always "software-development" when any predicate fires
- Gap vocabulary only affects CapabilityGapWarnings, never routing decisions
- SCHEDULE_CAPABILITY_LEARNING is NOT emitted (predicates fired — not knowledge_edge)
- Gap IDs flow to AutonomousLearningPipeline via CapabilityGapWarnings channel

**Anti-pattern to avoid:**
Do NOT emit SCHEDULE_CAPABILITY_LEARNING when predicates fire alongside gap vocabulary. SCHEDULE_CAPABILITY_LEARNING is only for `predicates == []`.
