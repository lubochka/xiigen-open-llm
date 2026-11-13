# Layer 0-A — Compiler Pairs (015)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-015-001 — TEST+UI+QA: visual regression suite with QA gate

A: "Write visual regression tests for the component library buttons, cards, and modals and QA sign-off on all baseline snapshots."
B: ["TEST", "UI", "QA"] — "write visual regression tests" → TEST; "buttons", "cards", "modals" → UI; "QA sign-off" → QA.
CompiledPredicateIds: ["TEST", "UI", "QA"]

---

## C0A-015-002 — TEST+UI: snapshot baseline for product card

A: "Create snapshot tests that capture the visual baseline for the product card component across light and dark themes."
B: ["TEST", "UI"] — "create snapshot tests" → TEST; "product card component" → UI (component); "themes" → UI (visual).
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-015-003 — gap: "compare screenshots manually"

A: "Compare the before-and-after screenshots of the redesigned header manually."
B: [] — "compare" → gap; "manually" → gap (no predicate). No test-writing verb. "header" → UI? No test-writing verb present means TEST does not fire; "header" alone with gap action → zero predicates.
CompiledPredicateIds: []

---

## C0A-015-004 — TEST+QA: regression test + QA review

A: "Assert that all visual regression test baselines pass QA review before the design token update ships."
B: ["TEST", "QA"] — "assert" → TEST; "visual regression test" → TEST (regression test compound). "QA review" → QA. No UI component vocabulary in this sentence.
CompiledPredicateIds: ["TEST", "QA"]

---

## C0A-015-005 — TEST+UI+QA: full visual regression pipeline

A: "Write visual regression specs for each screen breakpoint, validate the responsive layout components, and QA the full regression suite."
B: ["TEST", "UI", "QA"] — "write visual regression specs" → TEST; "screen", "components", "layout", "responsive" → UI; "QA the full regression suite" → QA.
CompiledPredicateIds: ["TEST", "UI", "QA"]
