# Held-Out and Corruption Pairs (034)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H034-001 — Held-out: WishlistFlowTest — toggle + badge

A: "Write E2E tests for the wishlist page — assert the add-to-wishlist toggle fires, verify the wishlist badge increments."
  Expected: ["TEST", "UI"]
  Held-out: TEST (write, assert, verify). UI (wishlist page → page, add-to-wishlist toggle → toggle, wishlist badge → badge — three triggers → one UI). Result: [TEST, UI]. TransitionType: documentation.

---

## H034-002 — Held-out: WishlistFlowTest — "monitor" gap

A: "Monitor wishlist activity for performance anomalies."
  Expected: []
  Held-out: "monitor" is gap. "wishlist activity", "performance anomalies" — no UI vocabulary. Zero predicates → knowledge_edge_synthetic.

---

## H034-003 — Held-out: WishlistFlowTest — QA before release cut → 7.1

A: "Write wishlist E2E coverage, assert the toggle, and QA acceptance before release cut."
  Expected: ["TEST", "UI", "QA"]
  Held-out: TEST (write, assert). UI (toggle). QA (QA acceptance). QA → 7.1 ("before release cut"). AllSubPhaseIds: [5.3, 1.5, 7.1]. TransitionType: documentation.

---

## C034-001 — Corruption v6-02: "monitor" → TEST

Pattern: v6-02. Corrupt: "Monitor wishlist activity" → predicates = ["TEST"].
Correct: "monitor" is gap. predicates = [] (no UI vocabulary present either).

---

## C034-002 — Corruption v6-01: WISHLIST predicate invented

Pattern: v6-01. Corrupt: predicates = ["TEST", "UI", "WISHLIST"].
Correct: predicates = ["TEST", "UI"]. Domain names never become predicates.
