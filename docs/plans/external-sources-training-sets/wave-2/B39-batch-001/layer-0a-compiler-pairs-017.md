# Layer 0-A — Compiler Pairs (017)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-017-001 — TEST+UI: snapshot test for component library

A: "Write snapshot tests for the button, input, and dropdown components to detect unintended visual changes."
B: ["TEST", "UI"] — "write snapshot tests" → TEST; "button", "input", "dropdown" → UI (component vocabulary).
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-017-002 — TEST+UI+IMPLEMENT: build snapshot harness + create tests

A: "Build the screenshot test harness and create snapshot assertions for every modal variant in the design system."
B: ["TEST", "UI", "IMPLEMENT"] — "build" → IMPLEMENT; "create snapshot assertions" → TEST; "modal variant" → UI (modal); "design system" → gap (system vocabulary, not UI predicate trigger).
CompiledPredicateIds: ["TEST", "UI", "IMPLEMENT"]

---

## C0A-017-003 — gap: "update snapshots after redesign"

A: "Update the snapshots after the redesign to reflect the new layout."
B: [] — "update" alone is gap (not a test-writing verb; update ≠ write/create/add assertions). "snapshots" → gap without test-writing verb. "layout" → UI fires? Yes — "layout" → UI. But without any test verb or TEST trigger, the UI fires.
Correction: CompiledPredicateIds: ["UI"] — "layout" fires UI. No TEST because "update" is not a test-writing verb.

---

## C0A-017-004 — TEST+UI: assert snapshot matches for pagination component

A: "Assert that the pagination component snapshot matches the approved baseline after the spacing update."
B: ["TEST", "UI"] — "assert" → TEST; "pagination component" → UI (pagination, component).
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-017-005 — TEST+UI+QA: full snapshot review

A: "Create visual snapshot tests for each tooltip variant and QA sign-off that all baselines are approved."
B: ["TEST", "UI", "QA"] — "create visual snapshot tests" → TEST; "tooltip variant" → UI (tooltip); "QA sign-off" → QA.
CompiledPredicateIds: ["TEST", "UI", "QA"]
