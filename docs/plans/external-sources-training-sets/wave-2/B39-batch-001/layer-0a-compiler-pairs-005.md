# Layer 0-A — Compiler Pairs (005)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: compiler

---

## C0A-005-001 — IMPLEMENT+TEST+UI: build component + tests

A: "Implement the tooltip component and write interaction tests."
B: ["IMPLEMENT", "TEST", "UI"] — "implement" → IMPLEMENT; "write interaction tests" → TEST (5.3); "tooltip component" → UI.
CompiledPredicateIds: ["IMPLEMENT", "TEST", "UI"]

---

## C0A-005-002 — gap: "run Playwright"

A: "Run the Playwright test suite in CI."
B: [] — "run" is operational gap; "Playwright" is framework name → gap. No test-writing verb.
CompiledPredicateIds: []

---

## C0A-005-003 — TEST: "add assertions for modal"

A: "Add assertions for the modal open and close states."
B: ["TEST", "UI"] — "add assertions" (test-writing verb) → TEST (5.3); "modal" → UI.
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-005-004 — QA fires on "QA team" in UI context

A: "The QA team needs to verify the carousel behavior on mobile."
B: ["UI", "QA"] — "QA team" → QA; "carousel behavior" → UI. "verify" without "write/create/add" context does not fire TEST.
CompiledPredicateIds: ["UI", "QA"]

---

## C0A-005-005 — deduplication: two UI triggers → one UI

A: "The sidebar and the header component need responsive fixes."
B: ["UI"] — "sidebar" and "header component" both fire UI trigger → one UI predicate (set, not multiset).
CompiledPredicateIds: ["UI"]
