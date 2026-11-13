# Layer 0-C — Knowledge Edge Pairs (032)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E032-001 — FilterFlowTest: gap vocabulary

A: Which words are gap in filter test requests?
B: Gap: "configure the filter options" ("configure" is gap), "run filter tests" ("run" is gap), "check the filter behavior" ("check" is gap), "describe the filter logic" ("describe" is gap), "update the filter state" ("update" is gap). "filter" alone is not a trigger.

---

## E032-002 — FilterFlowTest: "filter options" is not UI vocabulary

A: "Configure the filter options." Does UI fire?
B: No. "filter options" — "options" is not in the UI lexicon. "configure" is gap. Zero predicates → knowledge_edge_synthetic. Contrast: "filter panel", "filter checkbox", "filter badge" — all fire UI via panel/checkbox/badge tokens.

---

## E032-003 — FilterFlowTest: panel vs. options

A: What is the lexical distinction between "filter panel" (fires UI) and "filter options" (gap)?
B: "panel" is an explicit UI lexicon word. "options" is not. The compound noun's head word determines the UI trigger. Head word "panel" → UI fires. Head word "options" → gap.

---

## E032-004 — FilterFlowTest: badge fires on multiple compound nouns

A: Do "active filter badge", "badge count", and "filter badge" all fire UI?
B: Yes. All three contain "badge", which is in the UI lexicon. Any compound with "badge" as a component fires UI.

---

## E032-005 — FilterFlowTest: clear filters button

A: "Verify the clear filters button resets checkboxes." What fires?
B: TEST (verify). UI: "clear filters button" → button (fires UI), "checkboxes" → checkbox (fires UI) — two triggers, one UI. Result: [TEST, UI].
