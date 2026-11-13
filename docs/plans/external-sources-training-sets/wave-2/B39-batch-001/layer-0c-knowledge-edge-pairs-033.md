# Layer 0-C — Knowledge Edge Pairs (033)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E033-001 — CartFlowTest: gap vocabulary

A: Which words are gap in cart test requests?
B: Gap: "run cart tests" ("run" is gap), "check cart state" ("check" is gap), "monitor cart additions" ("monitor" is gap), "describe cart behavior" ("describe" is gap), "execute cart E2E" ("execute" is gap). "cart" alone is not a trigger.

---

## E033-002 — CartFlowTest: "add E2E tests" fires TEST not IMPLEMENT

A: "Add E2E tests for the coupon code input." Does IMPLEMENT fire?
B: No. "add" + test artifact noun ("E2E tests") → TEST. IMPLEMENT requires "add" + feature/product noun. "coupon code input" → input → UI. Result: [TEST, UI].

---

## E033-003 — CartFlowTest: "quantity controls" is not UI vocabulary

A: "Assert item quantity controls update totals." Does UI fire?
B: Depends on whether "controls" is in the UI lexicon. "controls" is not a standalone UI lexicon word in B39. However, "remove button" or "quantity input" (if "input" were present) would fire UI. In this exact request with only "quantity controls": TEST fires (assert), UI may not fire. Conservative: [TEST] only if no other UI vocabulary present.

---

## E033-004 — CartFlowTest: cart page fires UI

A: Does "cart page" fire UI?
B: Yes. "page" is in the UI lexicon. "cart page" fires UI via the "page" token.

---

## E033-005 — CartFlowTest: discount badge fires UI

A: "Verify the discount badge appears after coupon code is applied." What fires?
B: TEST (verify). UI (discount badge → badge). Result: [TEST, UI].
