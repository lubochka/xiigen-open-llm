# Layer 3 — Teaching Unit Pairs (008)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B36-L3-008-001 — Predicate dominance: project planning vocabulary cannot suppress development predicates

**Teaching objective:** Model learns that gap vocabulary has no suppressive power over predicates.

**The dominance rule (B36 Rule 11):**
When predicates fire alongside gap vocabulary:
- Predicates determine routing (dominate)
- Gap vocabulary produces CapabilityGapWarnings only (subordinate)

Gap vocabulary cannot:
- Suppress a fired predicate
- Change the routing path
- Change DomainId from "software-development" to "project-management"
- Trigger KNOWLEDGE_EDGE when a predicate also fired

---

## B36-L3-008-002 — Teaching pairs for predicate dominance

**Pair A — IMPLEMENT dominates sprint vocabulary:**

```
Input: "Implement the sprint authentication feature."
Gap vocabulary: sprint, feature (feature is also IMPLEMENT — fires predicate)
Expected predicates: ["IMPLEMENT"]
Expected routing: Phase 4.1 (IMPLEMENT dominates)
Wrong output: [] (suppressing IMPLEMENT because "sprint" is planning vocabulary)
```

**Pair B — QA dominates backlog vocabulary:**

```
Input: "Review the backlog grooming outcomes."
Gap vocabulary: backlog, grooming
Expected predicates: ["QA"] (review fires)
Expected routing: QA sub-phase
Wrong output: [] (suppressing QA because context is "backlog grooming")
```

**Pair C — DOCUMENT dominates milestone vocabulary:**

```
Input: "Document the milestone acceptance criteria."
Gap vocabulary: milestone
Expected predicates: ["DOCUMENT"]
Expected routing: Phase 2.1
Wrong output: [] (suppressing DOCUMENT because subject is milestone)
```

---

## B36-L3-008-003 — Suppression is always v6-02 corruption

Any case where a predicate is suppressed because the sentence "feels like planning" is v6-02 corruption. The model must apply this rule without exception:

If token X is in lexicon Y and appears in the request, predicate Y fires. The surrounding vocabulary does not matter. The sentence topic does not matter. The user's apparent intent does not matter.

---

## B36-L3-008-004 — Why dominance is correct behavior

**Practical reasoning:**
When a user says "implement the sprint feature," they want the feature implemented. The sprint context is incidental. Routing to Phase 4.1 (IMPLEMENT) serves the user's primary goal. The sprint gap warning notes that sprint planning assistance is unavailable, but the implementation proceeds.

**System reasoning:**
The orchestrator's job is to route based on what it can do (predicates). When IMPLEMENT fires, the orchestrator can handle the implementation. When sprint fires (gap), it cannot handle sprint planning. Predicate dominance means: do what you can, note what you can't.
