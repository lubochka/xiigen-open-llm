# Held-Out and Corruption Pairs (003)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H003-001 — Held-out: UI — "skeleton loader"

A: "The skeleton loader shows for too long before content appears."
  Expected: ["UI"]
  Held-out: "skeleton loader" → UI predicate (loading state vocabulary).

---

## H003-002 — Held-out: IMPLEMENT+TEST+UI+QA — four-predicate

A: "Build the date picker, write calendar interaction tests, fix calendar CSS, and QA sign-off."
  Expected: ["IMPLEMENT", "TEST", "UI", "QA"]
  Held-out: Four distinct clauses → four predicates. TransitionType: feature_addition. QA last.

---

## H003-003 — Held-out: gap — "dashboard layout"

A: "Fix the analytics dashboard layout."
  Expected: []
  Held-out: "dashboard" is confirmed gap vocabulary. Does not fire UI predicate.

---

## C003-001 — Corruption: UI routes to 5.3 instead of 1.5 (wrong)

A: ["UI"] | "Fix the carousel keyboard navigation"
  Correct: UIInteractionFlow15 (sub-phase 1.5)
  Corrupt: E2ETestingFlow53 (sub-phase 5.3) ← wrong (UI routes to 1.5, not 5.3)
  Teaching: UI predicate always routes to 1.5 (UIInteractionFlow15). TEST routes to 5.3 for E2E tests. They are separate flows.

---

## C003-002 — Corruption v6-03: TEST+UI+QA collapsed to TEST only

A: ["TEST", "UI", "QA"] | "Write modal tests, fix modal styling, QA sign-off"
  Correct: [E2ETestingFlow53, UIInteractionFlow15, QAGatingFlow73]
  Corrupt: [E2ETestingFlow53] ← v6-03 (UI and QA flows suppressed)
  Teaching: No cap on flow count. Three predicates → three flows. UI and QA are not subsumed by TEST.
