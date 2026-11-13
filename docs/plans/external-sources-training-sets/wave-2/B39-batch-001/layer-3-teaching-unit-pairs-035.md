# Layer 3 — Teaching Unit Pairs (035)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU035-001 — OrderTrackingTest: "track order" → gap

A: "Track the order and check its status." Why zero predicates?
B: "track" is gap. "check" is gap. "order" and "status" are domain terms, not predicate triggers. Zero predicates → knowledge_edge_synthetic. Both gap verbs appear — neither fires any predicate.

---

## TU035-002 — OrderTrackingTest: status badge fires UI

A: Does "status badge" fire UI?
B: Yes. "badge" is in the UI lexicon → fires UI. "status badge", "cart badge", "notification badge", "filter badge" all fire UI via the "badge" token.

---

## TU035-003 — OrderTrackingTest: timeline component fires UI

A: Does "timeline component" fire UI?
B: Yes. "component" is in the UI lexicon → fires UI. Any compound containing "component" fires UI: "timeline component", "form component", "header component".

---

## TU035-004 — OrderTrackingTest: delivery status modal fires UI

A: Does "delivery status modal" fire UI?
B: Yes. "modal" is in the UI lexicon → fires UI. "delivery status modal" fires via "modal".

---

## TU035-005 — OrderTrackingTest: accordion fires UI

A: Does "order summary accordion" fire UI?
B: Yes. "accordion" is in the UI lexicon → fires UI. Any accordion variant ("settings accordion", "FAQ accordion", "order summary accordion") fires UI.
