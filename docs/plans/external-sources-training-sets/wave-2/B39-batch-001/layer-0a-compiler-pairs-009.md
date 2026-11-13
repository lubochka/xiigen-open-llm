# Layer 0-A — Compiler Pairs (009)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: compiler

---

## C0A-009-001 — UI: "responsive layout" vocabulary

A: "The sidebar layout breaks at 768px viewport width."
B: ["UI"] — "sidebar layout", "viewport width" → UI predicate (responsive/layout vocabulary).
CompiledPredicateIds: ["UI"]

---

## C0A-009-002 — TEST+UI: responsive tests

A: "Write responsive tests for the sidebar at 768px, 1024px, and 1440px breakpoints."
B: ["TEST", "UI"] — "write responsive tests" → TEST (5.3); "sidebar" + "breakpoints" → UI.
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-009-003 — gap: "update theme colors"

A: "Update the theme colors to match the new brand guidelines."
B: ["UI"] — "theme colors" → UI predicate fires on styling/theme vocabulary. Note: styling changes fire UI.
CompiledPredicateIds: ["UI"]

---

## C0A-009-004 — IMPLEMENT+TEST+UI+QA: complete UI feature development cycle

A: "Implement the search autocomplete, write E2E tests, fix dropdown styling, and QA approval."
B: ["IMPLEMENT", "TEST", "UI", "QA"] — four-predicate: implement→IMPLEMENT; write E2E tests→TEST(5.3); dropdown styling→UI; QA approval→QA.
CompiledPredicateIds: ["IMPLEMENT", "TEST", "UI", "QA"]

---

## C0A-009-005 — gap: "Cypress cloud"

A: "Configure Cypress Cloud for test result reporting."
B: [] — "configure Cypress Cloud" → operational gap. Framework name + configuration verb → gap.
CompiledPredicateIds: []
