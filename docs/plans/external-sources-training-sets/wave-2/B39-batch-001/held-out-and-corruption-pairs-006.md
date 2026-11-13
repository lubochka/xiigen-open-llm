# Held-Out and Corruption Pairs (006)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H006-001 — Held-out: IMPLEMENT+UI — "build pagination"

A: "Build the pagination component for the product listing."
  Expected: ["IMPLEMENT", "UI"]
  Held-out: "build" → IMPLEMENT; "pagination component" → UI. Two predicates.

---

## H006-002 — Held-out: TEST+UI+QA — "write + fix + QA"

A: "Write keyboard navigation tests for the tabs, fix Space key handler, QA sign-off."
  Expected: ["TEST", "UI", "QA"]
  Held-out: "write keyboard navigation tests" → TEST; "Space key handler", "tabs" → UI; "QA sign-off" → QA. Three predicates.

---

## H006-003 — Held-out: gap — "performance profiling"

A: "Profile the button render performance in React DevTools."
  Expected: []
  Held-out: "profile" + "React DevTools" → observational/tool gap. No test-writing verb, no QA abbreviation, no implementation verb.

---

## C006-001 — Corruption v6-01: RESPONSIVE invented

A: "Write responsive tests for the product grid at 768px."
  Correct: ["TEST", "UI"]
  Corrupt: ["TEST", "UI", "RESPONSIVE"] ← v6-01 (RESPONSIVE is not a valid predicate)
  Teaching: RESPONSIVE is not a predicate. Responsive vocabulary fires UI predicate. No separate RESPONSIVE predicate exists.

---

## C006-002 — Corruption: TEST alone for "write UI tests" (UI not fired)

A: "Write UI tests for the button component."
  Correct: ["TEST", "UI"]
  Corrupt: ["TEST"] ← wrong (UI predicate suppressed)
  Teaching: "UI tests" contains "UI" vocabulary → UI predicate fires. "button component" reinforces UI. Both TEST and UI must fire.
