# Layer 0-B — Router Pairs (016)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-016-001 — TEST alone → 5.3: BDD scenario for checkout

A: ["TEST"] | "Write BDD scenarios in Gherkin for the checkout flow: Given-When-Then for each payment method"
B: TEST → 5.3 (E2ETestingFlow53). One flow.
Reason: "BDD scenarios" → TEST → 5.3. "checkout flow" has no UI component vocabulary. Only TEST fires.

---

## C0B-016-002 — TEST+UI → 5.3 + 1.5: Cucumber scenario for modal interaction

A: ["TEST", "UI"] | "Create Gherkin scenarios that assert the confirmation modal opens, traps focus, and closes on ESC"
B: TEST → 5.3; UI → 1.5. Two flows.
Reason: "scenarios" + "assert" → TEST → 5.3; "confirmation modal" + "focus" → UI → 1.5.

---

## C0B-016-003 — zero predicates → GapHandler

A: [] | "Describe the BDD workflow for the checkout acceptance criteria"
B: GapHandler. "describe" → gap; "BDD workflow" → gap; zero predicates.

---

## C0B-016-004 — TEST+QA → 5.3 + 7.3: BDD acceptance + QA

A: ["TEST", "QA"] | "Write acceptance test scenarios for the shopping cart and get QA sign-off"
B: TEST → 5.3; QA → 7.3. "acceptance test scenarios" → 5.3 (E2E/acceptance context); "QA sign-off" → 7.3.

---

## C0B-016-005 — TEST+UI → 5.3 + 1.5: Given-When-Then for form validation

A: ["TEST", "UI"] | "Add Given-When-Then test cases for the registration form validation"
B: TEST → 5.3; UI → 1.5. "test cases" in E2E form-validation context → 5.3; "registration form" → UI → 1.5.
