# Layer 3 — Teaching Unit Pairs (018)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU018-001 — "cross-browser tests" is a TEST compound

A: "Write cross-browser tests for the product listing page layout." What predicates fire?
B: TEST fires on "write cross-browser tests" (compound). UI fires on "product listing page" (page) + "layout".
Result: ["TEST", "UI"]. Teaching: "cross-browser tests" is a recognized TEST compound. Browser names themselves (Chrome, Firefox, Safari) are gap.

---

## TU018-002 — "check" is gap for TEST; "validate" fires TEST

A: Compare: "Check the modal behavior on Chrome" vs "Validate the modal behavior on Chrome." What fires in each case?
B: "Check the modal" → ["UI"] only — "check" is gap; "modal" → UI. "Validate the modal" → ["TEST", "UI"] — "validate" → TEST; "modal" → UI. Chrome → gap in both.
Teaching: "check" is operational gap. "validate" is a test-writing verb. This is a critical distinction in B39.

---

## TU018-003 — "compatibility table" is gap; no predicates fire

A: "Check the browser compatibility table for CSS grid support." What predicates fire?
B: Zero predicates. "check" → gap. "browser compatibility table" → gap. "CSS grid support" → gap. No UI component vocabulary present.
Result: []. Teaching: "compatibility" is gap vocabulary. No predicate fires on "browser compatibility" checking.

---

## TU018-004 — "responsive" fires UI; "navigation bar" fires UI

A: "Validate the responsive layout of the navigation bar across Chrome and Firefox." What fires?
B: TEST fires on "validate". UI fires on "responsive layout" (responsive, layout) and "navigation bar" (nav). Chrome, Firefox → gap.
Result: ["TEST", "UI"]. Teaching: "responsive" and "nav/navigation" both fire UI. They fire once each → one UI predicate (deduplicated).

---

## TU018-005 — UI deduplication with three triggers

A: "Write regression test cases for each form input component." How many predicates fire?
B: TEST fires once (from "write regression test cases"). UI fires once (from "form", "input", "component" — three triggers, one predicate). Result: ["TEST", "UI"].
Teaching: "form input component" contains three UI triggers. They all deduplicate to one UI predicate. Deduplication rule is absolute.
