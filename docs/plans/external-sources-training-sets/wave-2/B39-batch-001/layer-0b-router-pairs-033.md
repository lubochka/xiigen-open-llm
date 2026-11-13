# Layer 0-B — Router Pairs (033)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-033-001 — CartFlowTest: routing decision

A: "Write E2E test cases for the cart flow — assert items added/removed, verify the cart badge count." Predicates: [TEST, UI]. Route.
B: TEST → 5.3. UI → 1.5 ("cart badge" → "badge" fires UI → 1.5). TransitionType: documentation. PrimarySubPhaseId: 5.3.

---

## C0B-033-002 — CartFlowTest: quantity selector routing

A: "Create test specs for the cart quantity selector — assert the input accepts numeric values." Predicates: [TEST, UI]. Route.
B: TEST → 5.3. UI → 1.5 ("cart quantity selector", "input field" → "input" fires UI → 1.5). Two sub-phases: [5.3, 1.5].

---

## C0B-033-003 — CartFlowTest: monitor is gap

A: "Monitor the cart page for performance regressions." Predicates: [UI]. Route.
B: UI → 1.5 ("cart page" → "page" fires UI → 1.5). TEST does not fire (monitor is gap; regression alone without test verb is gap). Single sub-phase.

---

## C0B-033-004 — CartFlowTest: full set [IMPLEMENT, TEST, UI, QA]

A: "Implement the cart drawer component, write E2E scenarios, assert the empty cart screen, QA gate." Predicates: [IMPLEMENT, TEST, UI, QA]. Route all.
B: IMPLEMENT → 4.1. TEST → 5.3. UI → 1.5 ("cart drawer component", "empty cart screen", "continue-shopping button" → "component", "screen", "button" fire UI → 1.5). QA → 7.3 (default, "QA gate" without release vocabulary). Execution: IMPLEMENT → TEST+UI (parallel) → QA. TransitionType: feature_addition.

---

## C0B-033-005 — CartFlowTest: snapshot test → 5.3

A: "Add snapshot tests for the cart item card component." Predicates: [TEST, UI]. Is snapshot routed to 5.3 or 5.1?
B: 5.3. Snapshot test in cart/UI context → 5.3 (E2E/visual/interaction testing sub-phase). Snapshot tests are classified as visual regression tests when associated with UI components. UI → 1.5 ("cart item card component" → "component" fires UI). AllSubPhaseIds: [5.3, 1.5].
