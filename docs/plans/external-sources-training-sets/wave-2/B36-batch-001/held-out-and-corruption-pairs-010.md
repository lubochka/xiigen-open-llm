# Held-Out and Corruption Pairs (010)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B36-HO-010-001 — HELD-OUT: mixed IMPLEMENT+QA in sprint context

**Input:** "Build the sprint velocity dashboard and review the burndown metrics."

**Expected output:**
- predicates: ["IMPLEMENT", "QA"] ("build" fires IMPLEMENT; "review" fires QA)
- KNOWLEDGE_EDGE: false
- DomainId: "software-development"
- CapabilityGapWarnings: sprint-001, velocity-001, burndown-001
- TransitionType: feature_addition
- Execution: Phase 4.1 → QA sub-phase (QA last)

**Why this is a held-out case:**
Tests IMPLEMENT+QA compound predicate in heavy planning vocabulary. Three gap terms, two predicates. Tests both predicate firing and QA-last ordering.

---

## B36-HO-010-002 — Generalization test for B36-HO-010-001

The model generalizes correctly if it:
1. Fires IMPLEMENT on "build"
2. Fires QA on "review"
3. Does NOT suppress either because sprint/velocity/burndown are planning vocabulary
4. Orders QA last (Phase 4.1 → QA)
5. Sets DomainId: "software-development"
6. Emits sprint-001, velocity-001, burndown-001 as CapabilityGapWarnings

---

## B36-COR-010-001 — CORRUPTION: PROJECT_PLANNING invented predicate

**Corrupt input/output (DO NOT REPRODUCE):**

```
Input: "Build the sprint velocity dashboard."
Corrupt output: predicates: ["IMPLEMENT", "PROJECT_PLANNING"]
Corrupt reasoning: "IMPLEMENT fires on 'build', and PROJECT_PLANNING captures the sprint/velocity context."
```

**Why this is wrong:** PROJECT_PLANNING is not a registered predicate. Only the nine registered predicates can appear in the predicate list. "sprint" and "velocity" are gap vocabulary — they produce CapabilityGapWarnings, not a PROJECT_PLANNING predicate.

**Correct output:**
- predicates: ["IMPLEMENT"]
- CapabilityGapWarnings: sprint-001, velocity-001

---

## B36-COR-010-002 — CORRUPTION: IMPLEMENT+QA reordered (QA not last)

**Corrupt input/output (DO NOT REPRODUCE):**

```
Input: "Build the sprint velocity dashboard and review the burndown."
Corrupt output: predicates: ["QA", "IMPLEMENT"] (wrong order; QA runs first)
Execution: QA sub-phase → Phase 4.1 (wrong — QA before IMPLEMENT)
```

**Why this is wrong:** QA must always run last. When IMPLEMENT and QA co-occur, the sequence is: Phase 4.1 (IMPLEMENT) → QA sub-phase. QA cannot validate an implementation that has not yet occurred.

**Correct execution order:**
- Phase 4.1 (IMPLEMENT) runs first
- QA sub-phase runs after Phase 4.1 completes
