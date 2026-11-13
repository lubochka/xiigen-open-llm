# Layer 0-A — Compiler Pairs (002)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: compiler

---

## C0A-002-001 — TEST fires on "write E2E tests"

A: "Write end-to-end tests for the checkout form."
B: ["TEST"] — "write" (test-writing verb) + "end-to-end tests" → TEST predicate; sub-phase 5.3 (E2E).
CompiledPredicateIds: ["TEST"]

---

## C0A-002-002 — UI fires on "button click interaction"

A: "Validate button click interactions on the payment page."
B: ["TEST", "UI"] — "validate" in test context → TEST; "button click interactions" → UI.
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-002-003 — gap: "automate the test suite"

A: "Automate the test suite using Playwright."
B: [] — "automate" is gap (not a test-writing verb); "Playwright" is framework name → gap.
CompiledPredicateIds: []

---

## C0A-002-004 — TEST+QA: write tests + QA sign-off

A: "Create UI tests for the registration flow and get QA sign-off."
B: ["TEST", "QA"] — "create UI tests" → TEST (sub-phase 5.3); "QA sign-off" → QA.
CompiledPredicateIds: ["TEST", "QA"]

---

## C0A-002-005 — UI+QA: UI component + QA gate

A: "The modal component needs QA review before release."
B: ["UI", "QA"] — "modal component" → UI; "QA review" → QA. No test-writing verb → TEST does not fire.
CompiledPredicateIds: ["UI", "QA"]
