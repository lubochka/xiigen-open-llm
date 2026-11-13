# Held-Out and Corruption Pairs (011)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H011-001 — Held-out: UI — "accordion"

A: "The accordion panel does not maintain expanded state after route change."
  Expected: ["UI"]
  Held-out: "accordion panel" + "expanded state" → UI (component + interaction state vocabulary).

---

## H011-002 — Held-out: TEST+UI — "add test coverage for empty state"

A: "Add test coverage for the search empty state and fix the empty state illustration alignment."
  Expected: ["TEST", "UI"]
  Held-out: "add test coverage" → TEST; "empty state illustration alignment" → UI.

---

## H011-003 — Held-out: gap — "install dependencies"

A: "Install the Playwright testing library and its dependencies."
  Expected: []
  Held-out: "install" is operational gap; "Playwright" is framework name. Gap.

---

## C011-001 — Corruption: "write tests" fires IMPLEMENT (wrong)

A: "Write E2E tests for the date picker."
  Correct: ["TEST", "UI"]
  Corrupt: ["IMPLEMENT", "UI"] ← wrong ("write" incorrectly fires IMPLEMENT instead of TEST)
  Teaching: "write" fires TEST when followed by test vocabulary (tests, test cases, assertions). "write the component" fires IMPLEMENT. Context matters: "write tests" ≠ "write code".

---

## C011-002 — Corruption: UI predicate suppressed in TEST+UI+QA

A: ["TEST", "UI", "QA"]
  Correct: [E2ETestingFlow53, UIInteractionFlow15, QAGatingFlow73] — three flows
  Corrupt: [E2ETestingFlow53, QAGatingFlow73] ← UI predicate suppressed
  Teaching: TEST does not subsume UI. Even in "write UI tests", UI predicate independently fires UIInteractionFlow15.
