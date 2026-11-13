# Held-Out and Corruption Pairs (002)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H002-001 — Held-out: TEST+UI — "write tests for tooltip"

A: "Write interaction tests for the tooltip positioning on mobile."
  Expected: ["TEST", "UI"]
  Held-out: "write interaction tests" → TEST (5.3); "tooltip positioning" → UI. Both fire independently.

---

## H002-002 — Held-out: UI — "drag-and-drop hover state"

A: "The drag-and-drop zone does not show a highlight on drag-over."
  Expected: ["UI"]
  Held-out: "drag-and-drop zone" + "highlight on drag-over" → UI predicate.

---

## H002-003 — Held-out: gap — "run automated tests"

A: "Run the automated E2E test suite against staging."
  Expected: []
  Held-out: "run" is operational gap. "automated" does not fire TEST. Framework execution commands → gap.

---

## C002-001 — Corruption v6-01: TEST_AUTOMATION invented

A: "Write automated UI tests for the checkout flow."
  Correct: ["TEST", "UI"]
  Corrupt: ["TEST", "UI", "TEST_AUTOMATION"] ← v6-01 (TEST_AUTOMATION is not a valid predicate)
  Teaching: "automated" is an adjective. It does not create a separate TEST_AUTOMATION predicate. TEST+UI only.

---

## C002-002 — Corruption v6-02: "automate" → TEST (wrong)

A: "Automate the regression test suite."
  Correct: []
  Corrupt: ["TEST"] ← v6-02 ("automate" incorrectly mapped to TEST)
  Teaching: "automate" is gap vocabulary. TEST fires on test-writing verbs (write, create, add, assert, validate).
