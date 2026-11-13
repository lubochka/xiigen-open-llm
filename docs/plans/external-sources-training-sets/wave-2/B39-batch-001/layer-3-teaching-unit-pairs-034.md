# Layer 3 — Teaching Unit Pairs (034)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU034-001 — WishlistFlowTest: toggle fires UI

A: Does "add-to-wishlist toggle" fire UI?
B: Yes. "toggle" is in the UI lexicon → fires UI. All toggle variants ("dark mode toggle", "feature toggle", "wishlist toggle") fire UI via the "toggle" token.

---

## TU034-002 — WishlistFlowTest: "monitor wishlist activity" → gap

A: "Monitor wishlist activity." Why zero predicates?
B: "monitor" is gap. "wishlist activity" is a behavioral concept, not UI vocabulary ("activity" is not in the UI lexicon). Zero predicates → knowledge_edge_synthetic.

---

## TU034-003 — WishlistFlowTest: badge + toggle → one UI predicate

A: "Assert the wishlist badge increments and the wishlist toggle activates." How many UI predicates?
B: One. Two UI triggers ("wishlist badge" → badge, "wishlist toggle" → toggle) → deduplication → one UI predicate. One TEST trigger ("assert") → one TEST predicate. Result: [TEST, UI].

---

## TU034-004 — WishlistFlowTest: QA before release cut → 7.1

A: "QA acceptance before release cut." QA routing lesson?
B: QA → 7.1. "release cut" = release vocabulary → 7.1 override. The phrase "release cut" signals a release pipeline context. Any of: "release cut", "release branch", "before release", "pre-release" → 7.1.

---

## TU034-005 — WishlistFlowTest: move-to-cart button fires UI

A: "Assert the move-to-cart button redirects correctly." What fires UI?
B: "move-to-cart button" → "button" fires UI. The compound "move-to-cart button" fires UI via the "button" token, regardless of the action described (redirect, transfer, navigation).
