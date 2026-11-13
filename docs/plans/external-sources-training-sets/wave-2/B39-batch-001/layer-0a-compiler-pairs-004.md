# Layer 0-A — Compiler Pairs (004)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: compiler

---

## C0A-004-001 — TEST fires on "verify" in test context

A: "Verify that the navigation menu collapses on mobile viewport."
B: ["TEST", "UI"] — "verify" (test-assertion verb) → TEST (5.3, mobile/browser context); "navigation menu" → UI.
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-004-002 — UI fires on responsive/layout vocabulary

A: "Fix the layout breakpoints for the product listing page."
B: ["UI"] — "layout breakpoints" → UI predicate. No test-writing verb.
CompiledPredicateIds: ["UI"]

---

## C0A-004-003 — IMPLEMENT+UI: build a UI component

A: "Build the date picker component for the booking form."
B: ["IMPLEMENT", "UI"] — "build" → IMPLEMENT; "date picker component" → UI. TransitionType: feature_addition.
CompiledPredicateIds: ["IMPLEMENT", "UI"]

---

## C0A-004-004 — gap: "check the UI"

A: "Check the UI before release."
B: [] — "check" is not a test-writing verb or QA abbreviation; "UI" as noun alone does not fire UI predicate without context. Gap.
CompiledPredicateIds: []

---

## C0A-004-005 — TEST: smoke tests → 5.3

A: "Write smoke tests for the homepage render."
B: ["TEST"] — "write smoke tests" → TEST (sub-phase 5.3, E2E/smoke context). No UI vocabulary beyond "homepage" → only TEST fires.
CompiledPredicateIds: ["TEST"]
