# Layer 3 — Teaching Unit Pairs (013)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU013-001 — Cypress is gap; "e2e tests" fires TEST

A: "Set up Cypress and write e2e tests for the cart button." What predicates fire?
B: TEST fires on "write e2e tests". UI fires on "cart button". "Cypress" is gap (framework name). "Set up" is gap (not in IMPLEMENT lexicon).
Result: ["TEST", "UI"]. Routing: TEST → 5.3, UI → 1.5.

---

## TU013-002 — "specs" fires TEST when paired with test verb

A: "Add specs for the checkout button flow in the Cypress test directory." Does TEST fire?
B: Yes. "add specs" → TEST fires on "specs" (test noun) paired with "add" (test-writing verb context). "checkout button" → UI. "Cypress test directory" is structural vocabulary → gap.
Result: ["TEST", "UI"].

---

## TU013-003 — "e2e" alone (descriptive use) does not fire TEST

A: "The e2e workflow covers the checkout, payment, and confirmation steps." Does TEST fire?
B: No. "e2e" in a descriptive sentence without a test-writing verb → gap. "covers" is gap. Zero predicates.
Teaching: TEST requires a test-writing verb (write, create, add, assert, validate) paired with "e2e". "e2e" alone in a description is gap.

---

## TU013-004 — "run" is gap even when "assert" is present

A: "Run the tests and assert that every checkout button is reachable by keyboard." Which predicate fires?
B: TEST fires on "assert" (test assertion verb). "run" is gap (operational). "checkout button" + "keyboard" → UI. Result: ["TEST", "UI"].
Teaching: A sentence can mix gap verbs and test-writing verbs. Only the test-writing verb fires TEST.

---

## TU013-005 — "set up" is consistently gap for IMPLEMENT

A: "Set up the Cypress project folder structure." Does IMPLEMENT fire?
B: No. "set up" is not in the IMPLEMENT lexicon. Zero predicates. IMPLEMENT requires: build, create, write, implement, add, develop, generate, scaffold, extend.
Teaching: "Set up" is operational vocabulary and is consistently gap for both IMPLEMENT and TEST in B39.
