# Layer 0-C — Knowledge Edge Pairs (034)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E034-001 — WishlistFlowTest: gap vocabulary

A: Which words are gap in wishlist test requests?
B: Gap: "monitor wishlist activity" ("monitor" is gap), "check wishlist contents" ("check" is gap), "run wishlist tests" ("run" is gap), "describe the wishlist feature" ("describe" is gap). "wishlist" alone is not a trigger.

---

## E034-002 — WishlistFlowTest: toggle fires UI

A: "Assert the add-to-wishlist toggle fires." Does UI fire?
B: Yes. "toggle" is in the UI lexicon. "add-to-wishlist toggle" fires UI via the "toggle" token. TEST fires (assert). Result: [TEST, UI].

---

## E034-003 — WishlistFlowTest: wishlist page fires UI

A: Does "wishlist page" fire UI?
B: Yes. "page" is in the UI lexicon → fires UI.

---

## E034-004 — WishlistFlowTest: "move-to-cart button" fires UI

A: "Assert the move-to-cart button redirects to cart." Does UI fire?
B: Yes. "button" is in the UI lexicon. "move-to-cart button" fires UI via "button".

---

## E034-005 — WishlistFlowTest: wishlist badge fires UI

A: "Verify the wishlist badge increments." Does UI fire?
B: Yes. "badge" is in the UI lexicon. "wishlist badge" fires UI. TEST fires (verify). Result: [TEST, UI].
