# Layer 0-A — Compiler Pairs (012)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: compiler

---

## C0A-012-001 — UI fires on "pagination" vocabulary

A: "The pagination controls do not update the URL query string."
B: ["UI"] — "pagination controls" → UI predicate (navigation/interaction vocabulary).
CompiledPredicateIds: ["UI"]

---

## C0A-012-002 — TEST+UI: pagination interaction tests

A: "Add interaction tests for the pagination next/previous navigation."
B: ["TEST", "UI"] — "add interaction tests" → TEST (5.3); "pagination next/previous navigation" → UI.
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-012-003 — gap: "load testing"

A: "Run load testing on the product listing endpoint."
B: [] — "load testing" without test-writing verb → observational/operational gap. "run" is not a test-writing verb.
CompiledPredicateIds: []

---

## C0A-012-004 — IMPLEMENT+TEST+UI: build + test

A: "Build the breadcrumb navigation component and write tests for back-navigation."
B: ["IMPLEMENT", "TEST", "UI"] — "build" → IMPLEMENT; "write tests" → TEST (5.3); "breadcrumb navigation component" → UI.
CompiledPredicateIds: ["IMPLEMENT", "TEST", "UI"]

---

## C0A-012-005 — deduplication: three UI triggers → one UI predicate

A: "Fix the button hover state, modal z-index, and checkbox focus outline."
B: ["UI"] — three UI vocabulary triggers (button, modal, checkbox) → one UI predicate. Set not multiset.
CompiledPredicateIds: ["UI"]
