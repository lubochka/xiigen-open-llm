# Layer 3 — Teaching Unit Pairs (032)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU032-001 — FilterFlowTest: "configure" is gap

A: "Configure the filter options." Why do zero predicates fire?
B: "configure" is explicitly gap — does not fire IMPLEMENT, TEST, or QA. "filter options" is a configuration concept, not UI vocabulary ("options" is not in the UI lexicon). Zero predicates → knowledge_edge_synthetic.

---

## TU032-002 — FilterFlowTest: panel fires UI

A: Does "filter panel" fire UI?
B: Yes. "panel" is in the UI lexicon → fires UI. "filter panel", "side panel", "settings panel" all fire UI via the "panel" token.

---

## TU032-003 — FilterFlowTest: badge fires UI

A: Does "active filter badge" fire UI?
B: Yes. "badge" is in the UI lexicon → fires UI. "badge count", "filter badge", "cart badge", "notification badge" all fire UI via the "badge" token.

---

## TU032-004 — FilterFlowTest: checkbox fires UI

A: Does "filter checkbox" fire UI?
B: Yes. "checkbox" is in the UI lexicon → fires UI. All checkbox variants ("filter checkboxes", "agreement checkbox") fire UI.

---

## TU032-005 — FilterFlowTest: multiple UI tokens → one UI predicate

A: "Assert filter panel checkboxes narrow results, verify the active filter badge count." How many UI predicates?
B: One. Triggers: "filter panel" (panel), "filter checkboxes" (checkbox), "active filter badge" (badge) = three UI triggers → deduplication → one UI predicate. "assert" + "verify" = two TEST triggers → one TEST predicate. Result: [TEST, UI].
