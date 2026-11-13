# Layer 0-A — Compiler Pairs (058)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-058-001 — IndexedDBTest: predicate compilation

A: "Write E2E test cases for IndexedDB (Index Database API) storage — assert the offline data form saves correctly, verify the sync screen shows pending changes."
B: TEST (write, assert, verify). UI ("offline data form" → form; "sync screen" → screen — two triggers → one UI). Result: [TEST, UI]. TransitionType: documentation. "IndexedDB" is a browser API name — gap.

---

## C0A-058-002 — IndexedDBTest: "IndexedDB" alone is gap

A: "Configure IndexedDB storage for offline support." Predicates?
B: "configure" is gap. "IndexedDB" is a browser API name — gap. "offline support" has no UI vocabulary. Zero predicates → knowledge_edge_synthetic.

---

## C0A-058-003 — IndexedDBTest: sync banner fires UI

A: "Assert the sync status banner updates after IndexedDB write completes." Predicates?
B: TEST (assert). UI (sync status banner → banner). Result: [TEST, UI].

---

## C0A-058-004 — IndexedDBTest: "implement IndexedDB" fires IMPLEMENT

A: "Implement IndexedDB caching for the form page and write E2E tests." Predicates?
B: IMPLEMENT ("implement IndexedDB caching"). TEST ("write E2E tests"). UI ("form page" → both form and page → two UI triggers → one UI). [IMPLEMENT, TEST, UI]. TransitionType: feature_addition.

---

## C0A-058-005 — IndexedDBTest: "monitor IndexedDB" is gap

A: "Monitor IndexedDB transaction errors." Predicates?
B: "monitor" is gap. "IndexedDB transaction errors" has no UI vocabulary. Zero predicates → knowledge_edge_synthetic.
