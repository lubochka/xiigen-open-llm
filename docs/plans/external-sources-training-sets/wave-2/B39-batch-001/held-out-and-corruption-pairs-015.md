# Held-Out and Corruption Pairs (015)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H015-001 — Held-out: TEST+UI+QA — visual regression with QA gate

A: "Write visual regression tests for buttons, cards, modals and QA sign-off on all baseline snapshots."
  Expected: ["TEST", "UI", "QA"]
  Held-out: "write visual regression tests" → TEST; "buttons/cards/modals" → UI; "QA sign-off" → QA.

---

## H015-002 — Held-out: UI only — "update snapshots after theme change"

A: "Update the visual snapshots after the theme change."
  Expected: ["UI"]
  Held-out: "update" → gap (not TEST, not IMPLEMENT); "visual" → UI. "snapshots" with gap verb → no TEST. Only UI fires.

---

## H015-003 — Held-out: gap — "quality check on visual regression"

A: "Run a quality check on the visual regression baseline approvals."
  Expected: []
  Held-out: "quality check" → gap (not QA); "run" → gap. Zero predicates. The abbreviation "QA" does not appear.

---

## C015-001 — Corruption v6-02: "update snapshots" → TEST (wrong)

A: "Update the visual snapshots after the theme change."
  Correct: ["UI"]
  Corrupt: ["TEST"] ← v6-02 ("update snapshots" incorrectly mapped to TEST — model treats "snapshots" as a TEST noun regardless of verb)
  Teaching: "update" is gap. TEST fires only when a test-writing verb (write, create, add, assert, validate) is present. "update" is not a test-writing verb.

---

## C015-002 — Corruption v6-01: VISUAL_REGRESSION invented as predicate

A: "Write visual regression tests for the button component."
  Correct: ["TEST", "UI"]
  Corrupt: ["TEST", "UI", "VISUAL_REGRESSION"] ← v6-01 (invented VISUAL_REGRESSION predicate)
  Teaching: "visual regression" is a modifier for TEST. It does not create a separate predicate. TEST absorbs the compound "visual regression tests".
