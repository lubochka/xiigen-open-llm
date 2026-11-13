# Layer 3 — Teaching Unit Pairs (002)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B36-L3-002-001 — "retrospective" ≠ "review": the sharp lexicon boundary

**Teaching objective:** Model learns to distinguish "retrospective" (gap) from "review" (QA predicate).

**Pair A — correct:**

```
Input: "Schedule the sprint retrospective."
Expected predicates: []
Expected state: KNOWLEDGE_EDGE
```

**Pair B — correct:**

```
Input: "Schedule the sprint review."
Expected predicates: ["QA"]
Expected state: normal routing
```

**Pair C — both in one sentence:**

```
Input: "Schedule the retrospective and review the sprint outcomes."
Expected predicates: ["QA"]  ← "review" fires; "retrospective" is gap
Expected CapabilityGapWarnings: [retrospective-001, sprint-001]
```

---

## B36-L3-002-002 — Why the boundary is sharp

"retrospective" is not a synonym for "review" in the compiler. They are different tokens with different lexicon entries:
- "review" → QA lexicon entry → predicate fires
- "retrospective" → no lexicon entry → gap vocabulary

The compiler does not perform semantic expansion or synonym mapping. If a future iteration adds "retrospective" to a lexicon, that change must be explicit — it cannot happen implicitly from semantic similarity.

---

## B36-L3-002-003 — Teaching test cases

**Test 1:**
Input: "Plan the retrospective agenda."
Correct: predicates: [], KNOWLEDGE_EDGE: true
Wrong: predicates: ["QA"] (treating retrospective as QA)

**Test 2:**
Input: "Review the retrospective outcomes."
Correct: predicates: ["QA"] (review fires), CapabilityGapWarnings: [retrospective-001]
Wrong: predicates: [] (failing to fire QA because context is "retrospective")

**Test 3:**
Input: "Facilitate the retrospective ceremony."
Correct: predicates: [], KNOWLEDGE_EDGE: true
Wrong: predicates: ["QA"] (incorrectly treating facilitation as QA review)

---

## B36-L3-002-004 — Memorization aid

**Retrospective** = looking backward at past work = gap vocabulary (no predicate)
**Review** = examining/approving something = QA predicate

Both involve looking at past work. But the compiler cares about the token, not the intent. "retrospective" is gap. "review" is QA. This distinction must be maintained without exception.
