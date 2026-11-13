# Layer 0-A — Compiler Pairs (016)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-016-001 — TEST alone: BDD (Behaviour-Driven Development) scenario for checkout

A: "Write BDD scenarios in Gherkin for the checkout flow: Given-When-Then for each payment method."
B: ["TEST"] — "write BDD scenarios" → TEST (scenario is in TEST lexicon); "Gherkin" → gap (tool name); "checkout flow" — "checkout" is not UI vocabulary; "payment method" — not UI vocabulary.
CompiledPredicateIds: ["TEST"]

---

## C0A-016-002 — TEST+UI: Cucumber scenario for modal interaction

A: "Create Gherkin scenarios that assert the confirmation modal opens, traps focus, and closes on ESC (Escape key)."
B: ["TEST", "UI"] — "create scenarios" + "assert" → TEST; "confirmation modal" → UI (modal); "focus" → UI (accessibility). "Gherkin" → gap. "Cucumber" → gap (tool name).
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-016-003 — gap: "describe the BDD workflow"

A: "Describe the BDD workflow for the checkout acceptance criteria."
B: [] — "describe" → gap (not a test-writing verb); "BDD workflow" → gap; "acceptance criteria" → gap (no predicate fires on this alone).
CompiledPredicateIds: []

---

## C0A-016-004 — TEST+QA: BDD acceptance + QA

A: "Write acceptance test scenarios for the shopping cart and get QA sign-off on the test plan."
B: ["TEST", "QA"] — "write acceptance test scenarios" → TEST (scenario, test); "QA sign-off" → QA. "Shopping cart" — cart is not in UI lexicon; no UI component fires.
CompiledPredicateIds: ["TEST", "QA"]

---

## C0A-016-005 — TEST+UI: Given-When-Then for form validation

A: "Add Given-When-Then test cases for the registration form validation: empty fields, invalid email, and weak password scenarios."
B: ["TEST", "UI"] — "add test cases" → TEST; "registration form" → UI (form); "email" — not UI; "password" — not UI.
CompiledPredicateIds: ["TEST", "UI"]
