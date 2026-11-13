# Layer 0-B — Router Pairs (032)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-032-001 — FilterFlowTest: routing decision

A: "Write E2E test specs for the product filter panel — assert selecting a checkbox updates results, verify the clear-all button resets state." Predicates: [TEST, UI]. Route.
B: TEST → 5.3. UI → 1.5 ("product filter panel", "category checkbox", "clear-all button" → "panel", "checkbox", "button" fire UI → 1.5, deduplicated). TransitionType: documentation.

---

## C0B-032-002 — FilterFlowTest: configure is gap

A: "Configure the filter component to support price range." Predicates: [UI]. Route.
B: UI → 1.5 ("filter component" → "component" fires UI → 1.5). IMPLEMENT does not fire (configure is gap). Single sub-phase: 1.5.

---

## C0B-032-003 — FilterFlowTest: IMPLEMENT routing

A: "Build the filter sidebar with a price range slider component, write test coverage." Predicates: [IMPLEMENT, TEST, UI]. Route.
B: IMPLEMENT → 4.1. TEST → 5.3. UI → 1.5 ("filter sidebar", "price range slider component", "input" → "sidebar", "component", "input" fire UI → 1.5). TransitionType: feature_addition.

---

## C0B-032-004 — FilterFlowTest: multi-select dropdown routing

A: "Create test cases for the multi-select dropdown filter — assert ARIA attributes." Predicates: [TEST, UI]. Route.
B: TEST → 5.3. UI → 1.5 ("multi-select dropdown" → "dropdown" fires UI; "ARIA attributes" → "aria" fires UI; both deduplicated to one UI predicate → 1.5). TransitionType: documentation.

---

## C0B-032-005 — FilterFlowTest: describe is gap

A: "Describe how the filter panel should behave when no results are returned." Predicates: [UI]. Route.
B: UI → 1.5 ("filter panel" → "panel" fires UI → 1.5). TEST does not fire (describe is gap). TrustState: deterministic_generic. Single sub-phase.
