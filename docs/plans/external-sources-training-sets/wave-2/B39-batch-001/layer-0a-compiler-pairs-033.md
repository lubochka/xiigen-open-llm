# Layer 0-A — Compiler Pairs (033)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-033-001 — CartFlowTest: predicate compilation

A: "Write E2E test cases for the cart flow — assert items can be added and removed, verify the cart badge updates the item count."
B: Predicates: TEST ("write E2E test cases", "assert", "verify" → deduplicated); UI ("cart badge" → "badge" fires UI). Result: [TEST, UI]. Routing: [5.3, 1.5]. TransitionType: documentation.

---

## C0A-033-002 — CartFlowTest: quantity selector

A: "Create test specs for the cart quantity selector — assert the input field accepts numeric values and the total price updates."
B: Predicates: TEST ("create test specs", "assert" → deduplicated); UI ("cart quantity selector", "input field" → "input" fires UI). Result: [TEST, UI]. Routing: [5.3, 1.5]. TransitionType: documentation.

---

## C0A-033-003 — CartFlowTest: "monitor" is gap

A: "Monitor the cart page for performance regressions."
B: Predicates: UI ("cart page" → "page" fires UI). TEST does NOT fire — "monitor" is gap. "Regression" alone without a test-writing verb does not fire TEST. Result: [UI]. Routing: [1.5]. TransitionType: documentation.

---

## C0A-033-004 — CartFlowTest: IMPLEMENT+TEST+UI+QA

A: "Implement the cart drawer component, write end-to-end scenarios for add-to-cart and remove interactions, assert the empty cart screen displays the continue-shopping button, and QA gate."
B: Predicates: IMPLEMENT ("implement"); TEST ("write end-to-end scenarios", "assert" → deduplicated); UI ("cart drawer component", "empty cart screen", "continue-shopping button" → deduplicated); QA ("QA gate" → QA). Result: [IMPLEMENT, TEST, UI, QA]. TransitionType: feature_addition. Routing: [4.1, 5.3, 1.5, 7.3].

---

## C0A-033-005 — CartFlowTest: snapshot test fires TEST

A: "Add snapshot tests for the cart item card component."
B: Predicates: TEST ("add snapshot tests" — "add" with test noun → TEST); UI ("cart item card component" → "component" fires UI). Result: [TEST, UI]. Routing: [5.3, 1.5]. TransitionType: documentation.
