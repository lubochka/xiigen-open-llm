# Layer 0-C — Knowledge Edge Pairs (031)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E031-001 — SearchFlowTest: gap vocabulary

A: Which words are gap in search test requests?
B: Gap: "scan the search results" ("scan" is gap), "run search tests" ("run" is gap), "monitor search performance" ("monitor" is gap), "execute the search flow" ("execute" is gap), "describe the search behavior" ("describe" is gap). "search" alone is not a trigger.

---

## E031-002 — SearchFlowTest: "results grid" is not UI vocabulary

A: "Verify the search results grid renders." Does UI fire?
B: TEST fires (verify). "results grid" — "grid" is not in the UI lexicon. UI does NOT fire on "grid". If the request said "results screen" or "results page", UI would fire. Result: [TEST] only, not [TEST, UI].

---

## E031-003 — SearchFlowTest: "autocomplete dropdown" fires UI

A: Does "autocomplete dropdown" fire UI?
B: Yes. "dropdown" is in the UI lexicon. "autocomplete dropdown" fires UI via the "dropdown" token. "autocomplete" alone is not a UI lexicon word.

---

## E031-004 — SearchFlowTest: "search page" fires UI

A: Does "search page" fire UI?
B: Yes. "page" is in the UI lexicon. "search page", "results page", "listing page" all fire UI via the "page" token.

---

## E031-005 — SearchFlowTest: no-results screen

A: "Write tests for the no-results screen." What fires?
B: TEST (write tests). UI (no-results screen → screen). Result: [TEST, UI]. "screen" is in the UI lexicon regardless of the "no-results" modifier.
