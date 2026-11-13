# Held-Out and Corruption Pairs (002)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B36-HO-002-001 — HELD-OUT: "release cycle planning" → knowledge_edge

**Input:** "Plan the release cycle for Q3 and align the sprint milestones to it."

**Expected output:**
- predicates: []
- KNOWLEDGE_EDGE: true
- DomainId: "project-management"
- CapabilityGapWarnings: release-cycle-001, sprint-001, milestone-001
- SCHEDULE_CAPABILITY_LEARNING: emitted with all three IDs
- FinalHumanResult: null

**Why this is a held-out case:**
"release cycle" is a compound gap term (Rule 1: "release cycle (planning)" is gap vocabulary). The model must recognize "release cycle" as gap, not fire on "release" alone as a QA trigger (releases are not QA events in planning context).

---

## B36-HO-002-002 — Generalization test for B36-HO-002-001

The model generalizes correctly if it:
1. Recognizes "release cycle" in planning context as gap vocabulary (release-cycle-001)
2. Does NOT fire QA on "release" (release is neutral standalone — not a QA trigger)
3. Does NOT fire IMPLEMENT on any token in the sentence
4. Emits SCHEDULE_CAPABILITY_LEARNING (once, all three gap IDs)

---

## B36-COR-002-001 — CORRUPTION: "release" wrongly fires QA

**Corrupt input/output (DO NOT REPRODUCE):**

```
Input: "Plan the release cycle for Q3."
Corrupt output: predicates: ["QA"]
Corrupt reasoning: "release" triggers QA because releases require QA gates
```

**Why this is wrong:**
"release" is not in the QA lexicon. The QA lexicon tokens include "review", "approve", "test", "verify", "validate" — not "release." A release cycle in planning context is gap vocabulary (release-cycle-001), not a QA trigger.

**Correct output:**
- predicates: []
- KNOWLEDGE_EDGE: true
- CapabilityGapWarnings: release-cycle-001

---

## B36-COR-002-002 — Corruption type classification

**B36-COR-002-001** is a variant of v6-02 corruption: a token is incorrectly treated as firing a predicate based on semantic reasoning ("releases need QA") rather than literal lexicon matching ("release" is not in the QA lexicon).

This corruption causes:
- Incorrect QA routing for planning requests
- DomainId wrongly set to "software-development"
- KNOWLEDGE_EDGE wrongly bypassed
- SCHEDULE_CAPABILITY_LEARNING not emitted
