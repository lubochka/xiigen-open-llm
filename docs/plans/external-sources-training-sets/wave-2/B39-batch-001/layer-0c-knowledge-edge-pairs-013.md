# Layer 0-C — Knowledge Edge Pairs (013)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E013-001 — "e2e" fires TEST even when "Cypress" is gap

A: "Set up Cypress and write e2e tests for the cart button."
B: ["TEST", "UI"] — "write e2e tests" → TEST; "cart button" → UI. "Set up" → gap; "Cypress" → gap.
Edge: "e2e" is a compound TEST trigger. The framework name (Cypress) does not prevent TEST from firing on the test vocabulary in the same sentence.

---

## E013-002 — "spec" fires TEST in Cypress context

A: "Add specs for the checkout button flow in the Cypress test directory."
B: ["TEST", "UI"] — "add specs" → TEST; "checkout button" → UI. "Cypress test directory" → gap (operational/structural vocabulary).
Edge: "specs" is a test-writing noun compound. "add" is a test-writing verb in context of "specs".

---

## E013-003 — boundary: "set up" + Cypress = zero IMPLEMENT triggers

A: "Set up the Cypress project folder structure."
B: [] — "set up" → gap; "Cypress" → gap; "project folder structure" → gap. Zero predicates.
Edge: "set up" is not in the IMPLEMENT lexicon. IMPLEMENT requires: build, create, write, implement, add, develop, generate, scaffold, extend.

---

## E013-004 — boundary: "run" + "assert" — only "assert" fires TEST

A: "Run the tests and assert that every checkout button is reachable by keyboard."
B: ["TEST", "UI"] — "assert" → TEST; "checkout button" → UI (button); "keyboard" → UI (accessibility). "run" → gap (operational).
Edge: A sentence can contain both a gap verb ("run") and a TEST trigger ("assert"). Only the TEST trigger fires.

---

## E013-005 — gap: "e2e" without test-writing verb

A: "The e2e workflow covers checkout, payment, and confirmation."
B: [] — "e2e" in a descriptive sentence without a test-writing verb → gap. "covers" → gap. Zero predicates.
Edge: "e2e" alone is not a predicate trigger. It must co-occur with a test-writing verb (write, create, add, assert, validate) to fire TEST.
