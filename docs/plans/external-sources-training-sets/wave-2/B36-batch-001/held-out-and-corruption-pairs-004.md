# Held-Out and Corruption Pairs (004)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B36-HO-004-001 — HELD-OUT: "review team velocity" → QA fires on "review"

**Input:** "Review the team velocity data from the last three sprints."

**Expected output:**
- predicates: ["QA"]
- KNOWLEDGE_EDGE: false
- DomainId: "software-development"
- CapabilityGapWarnings: velocity-001, sprint-001
- TransitionType: no_contribution
- PhaseId: QA sub-phase

**Why this is a held-out case:**
Tests the sharp "review → QA" boundary in a planning-heavy sentence. The sentence is entirely about sprint velocity metrics, but "review" fires QA. The model must not suppress QA.

---

## B36-HO-004-002 — Generalization test for B36-HO-004-001

The model generalizes correctly if it:
1. Fires QA on "review"
2. Does NOT suppress QA because "velocity" and "sprints" are planning vocabulary
3. Sets DomainId: "software-development" (QA fired)
4. Emits velocity-001 and sprint-001 as CapabilityGapWarnings (velocity/sprint intent noted)
5. Does NOT emit SCHEDULE_CAPABILITY_LEARNING (QA fired — not knowledge_edge)

---

## B36-COR-004-001 — CORRUPTION: QA suppressed in planning context

**Corrupt input/output (DO NOT REPRODUCE):**

```
Input: "Review the team velocity data."
Corrupt output: predicates: []
Corrupt reasoning: "This is a project planning sentence about velocity metrics. QA is not relevant here."
```

**Why this is wrong:** "review" is in the QA lexicon. It fires QA unconditionally. The subject matter ("velocity data," "planning context") does not suppress QA.

**Correct output:**
- predicates: ["QA"]
- DomainId: "software-development"
- CapabilityGapWarnings: velocity-001

---

## B36-COR-004-002 — Effect of QA suppression

When QA is suppressed (v6-02):
- Request enters KNOWLEDGE_EDGE (wrong state)
- DomainId: "project-management" (wrong)
- SCHEDULE_CAPABILITY_LEARNING emitted (wrong — review is handled by QA, not a gap)
- FinalHumanResult: null (wrong — QA would have produced a result)

The user asking to "review velocity data" does not get a QA result. They get a null response and a learning signal. This is a harmful routing error.
