# Layer 0-A — Compiler Pairs (023)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-023-001 — TEST+UI+QA: three-predicate dense signal

A: "Write e2e test cases for the accessibility-compliant modal, validate every ARIA role, and QA the complete test suite before merge."
B: ["TEST", "UI", "QA"] — "write e2e test cases" → TEST; "modal" → UI (modal); "ARIA role" → UI (accessibility); "validate" → TEST (validation verb in test context, deduplicated); "QA the test suite" → QA.
CompiledPredicateIds: ["TEST", "UI", "QA"]

---

## C0A-023-002 — TEST+UI+QA: three-predicate from three separate clauses

A: "Create test coverage for the checkout button states, update the responsive layout, and QA sign-off on the final build."
B: ["TEST", "UI", "QA", "IMPLEMENT"] — "create test coverage" → TEST; "checkout button" → UI (button); "update the responsive layout" → UI (responsive, layout); "update" → IMPLEMENT? "update" is not explicitly in IMPLEMENT lexicon (build, create, write, implement, add, develop, generate, scaffold, extend). "update" is gap. "QA sign-off" → QA.
Correction: CompiledPredicateIds: ["TEST", "UI", "QA"]

---

## C0A-023-003 — TEST+UI+QA: three-predicate accessibility audit

A: "Write accessibility test scenarios for the form input and button components, assert WCAG 2.1 AA compliance, and QA validate the results."
B: ["TEST", "UI", "QA"] — "write accessibility test scenarios" + "assert" → TEST; "form input", "button components" → UI; "WCAG 2.1 AA compliance" → UI (accessibility); "QA validate" → QA.
CompiledPredicateIds: ["TEST", "UI", "QA"]

---

## C0A-023-004 — boundary: TEST+UI+QA — "verify + screen + QA"

A: "Verify the mobile screen layout for the checkout flow and QA the results for release acceptance."
B: ["TEST", "UI", "QA"] — "verify" → TEST; "screen" → UI (screen); "layout" → UI (layout); "QA the results" → QA. "mobile" → gap alone but screen + layout confirm UI.
CompiledPredicateIds: ["TEST", "UI", "QA"]

---

## C0A-023-005 — TEST+UI+QA: three-predicate: assert + component + QA gate

A: "Assert that every navigation component passes interaction tests and QA sign-off before shipping."
B: ["TEST", "UI", "QA"] — "assert" + "interaction tests" → TEST; "navigation component" → UI (nav, component); "QA sign-off" → QA.
CompiledPredicateIds: ["TEST", "UI", "QA"]
