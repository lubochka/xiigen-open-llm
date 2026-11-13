# Held-Out and Corruption Pairs (018)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H018-001 — Held-out: TEST+UI+QA — cross-browser suite

A: "Write cross-browser tests for the product listing page layout on Chrome, Firefox, Safari, and QA the results."
  Expected: ["TEST", "UI", "QA"]
  Held-out: "write cross-browser tests" → TEST; "product listing page" + "layout" → UI; "QA the results" → QA. Browser names → gap.

---

## H018-002 — Held-out: gap — "check compatibility table"

A: "Check the browser compatibility table for CSS grid support."
  Expected: []
  Held-out: "check" → gap; "browser compatibility table" → gap; "CSS grid" → gap. Zero predicates.

---

## H018-003 — Held-out: TEST+UI — validate responsive layout on browsers

A: "Validate the responsive layout of the navigation bar across Chrome and Firefox."
  Expected: ["TEST", "UI"]
  Held-out: "validate" → TEST; "responsive layout" + "navigation bar" → UI. Chrome, Firefox → gap.

---

## C018-001 — Corruption v6-02: browser name fires BROWSER predicate (wrong)

A: "Write cross-browser tests for the modal on Chrome and Firefox."
  Correct: ["TEST", "UI"]
  Corrupt: ["TEST", "UI", "BROWSER"] ← v6-01 (BROWSER invented as predicate from browser names)
  Teaching: Browser names (Chrome, Firefox, Safari, IE11, Edge) are gap vocabulary. No BROWSER predicate exists.

---

## C018-002 — Corruption v6-02: "check compatibility" → TEST (wrong)

A: "Check the modal behavior on Chrome."
  Correct: ["UI"] (only "modal" fires UI; "check" is gap)
  Corrupt: ["TEST", "UI"] ← v6-02 ("check" incorrectly mapped to TEST as a validation/test verb)
  Teaching: "check" is operational gap. TEST fires on: assert, validate, verify, write tests, create tests. "check" is not equivalent to "validate" for TEST predicate purposes.
