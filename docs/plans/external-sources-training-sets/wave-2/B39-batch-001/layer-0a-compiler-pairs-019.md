# Layer 0-A — Compiler Pairs (019)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-019-001 — TEST+UI: mobile responsive test cases

A: "Write end-to-end tests for the mobile checkout flow at 320px and 375px screen widths covering the sticky footer button."
B: ["TEST", "UI"] — "write end-to-end tests" → TEST; "screen", "button" → UI; "mobile" → not a standalone predicate but "screen" + "button" confirm UI.
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-019-002 — TEST+UI+IMPLEMENT: build responsive layout + add tests

A: "Implement the responsive grid layout for the product listing and add test coverage for each breakpoint on mobile."
B: ["TEST", "UI", "IMPLEMENT"] — "implement" → IMPLEMENT; "add test coverage" → TEST; "responsive grid layout" → UI (responsive, layout); "product listing" — not UI; "mobile" → gap alone, but "responsive" + "layout" confirm UI.
CompiledPredicateIds: ["TEST", "UI", "IMPLEMENT"]

---

## C0A-019-003 — gap: "resize the browser to 375px"

A: "Resize the browser to 375px to see how the layout looks."
B: ["UI"] — "resize" → gap (operational); "layout" → UI fires (layout is in UI lexicon). No test-writing verb → no TEST. "see how" → gap.
CompiledPredicateIds: ["UI"]

---

## C0A-019-004 — TEST+UI: assert hamburger menu on mobile viewport

A: "Assert that the hamburger menu navigation is visible and functional at 320px mobile viewport."
B: ["TEST", "UI"] — "assert" → TEST; "hamburger menu navigation" → UI (nav); "visible" → UI (visual); "mobile viewport" — viewport → gap but "navigation" confirms UI.
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-019-005 — TEST+UI+QA: full mobile test + QA acceptance

A: "Create responsive test cases for the mobile product card, verify image scaling and tap targets, and QA sign-off."
B: ["TEST", "UI", "QA"] — "create test cases" + "verify" → TEST; "product card" → UI (component); "image scaling" → UI (visual); "tap targets" → UI (button/interaction); "QA sign-off" → QA.
CompiledPredicateIds: ["TEST", "UI", "QA"]
