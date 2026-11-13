# Layer 3 — Teaching Unit Pairs (033)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU033-001 — CartFlowTest: "add E2E tests" = TEST not IMPLEMENT

A: "Add E2E tests for the coupon code input on the cart page." Does IMPLEMENT fire?
B: No. "add E2E tests" → TEST (add + test artifact noun). "coupon code input" → UI (input). "cart page" → UI (page). Result: [TEST, UI]. IMPLEMENT fires only when "add" precedes a feature/product noun, not a test noun.

---

## TU033-002 — CartFlowTest: cart badge fires UI

A: "Write test coverage to assert the cart badge updates." What fires?
B: TEST ("write test coverage", "assert"). UI ("cart badge" → badge). [TEST, UI]. TransitionType: documentation.

---

## TU033-003 — CartFlowTest: "implement save-for-later" fires IMPLEMENT

A: "Implement the save-for-later feature and write E2E tests for the cart page." Why does IMPLEMENT fire?
B: "implement ... feature" → IMPLEMENT. "feature" is a feature/product noun. "implement" is a core IMPLEMENT trigger verb. "write E2E tests" → TEST. "cart page" → UI. Three predicates: [IMPLEMENT, TEST, UI]. TransitionType: feature_addition.

---

## TU033-004 — CartFlowTest: empty cart screen fires UI

A: Does "empty cart screen" fire UI?
B: Yes. "screen" fires UI. "empty cart screen" contains the "screen" token → UI fires.

---

## TU033-005 — CartFlowTest: coupon code input fires UI

A: Does "coupon code input" fire UI?
B: Yes. "input" is in the UI lexicon → fires UI. Any compound containing "input" as the head noun fires UI: "coupon code input", "search input", "text input", "quantity input".
