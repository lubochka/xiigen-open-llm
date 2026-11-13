# Layer 0-B — Router Pairs (019)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-019-001 — TEST+UI → 5.3 + 1.5: mobile responsive e2e

A: ["TEST", "UI"] | "Write end-to-end tests for the mobile checkout flow at 320px and 375px covering the sticky footer button"
B: TEST → 5.3; UI → 1.5. "write end-to-end tests" → 5.3; "screen"/"button" → UI → 1.5.

---

## C0B-019-002 — TEST+UI+IMPLEMENT → 5.3 + 1.5 + 4.1: build responsive layout + test

A: ["TEST", "UI", "IMPLEMENT"] | "Implement the responsive grid layout for the product listing and add test coverage for each breakpoint on mobile"
B: TEST → 5.3; UI → 1.5; IMPLEMENT → 4.1. Three flows.
TransitionType: feature_addition (IMPLEMENT present).

---

## C0B-019-003 — UI alone → 1.5: resize browser to see layout

A: ["UI"] | "Resize the browser to 375px to see how the layout looks"
B: UI → 1.5. One routing decision. "layout" → UI → 1.5. "resize" is operational gap → no TEST or IMPLEMENT.

---

## C0B-019-004 — TEST+UI → 5.3 + 1.5: assert hamburger menu

A: ["TEST", "UI"] | "Assert that the hamburger menu navigation is visible and functional at 320px mobile viewport"
B: TEST → 5.3; UI → 1.5. "assert" → 5.3; "navigation" + "visible" → UI → 1.5.

---

## C0B-019-005 — TEST+UI+QA → 5.3 + 1.5 + 7.3: full mobile test + QA

A: ["TEST", "UI", "QA"] | "Create responsive test cases for the mobile product card, verify image scaling and tap targets, and QA sign-off"
B: TEST → 5.3; UI → 1.5; QA → 7.3.
AllSubPhaseIds: [5.3, 1.5, 7.3]
