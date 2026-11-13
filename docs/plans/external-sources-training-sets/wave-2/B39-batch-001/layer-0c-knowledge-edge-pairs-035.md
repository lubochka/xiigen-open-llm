# Layer 0-C — Knowledge Edge Pairs (035)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E035-001 — OrderTrackingTest: gap vocabulary

A: Which words are gap in order tracking test requests?
B: Gap: "track the order" ("track" is gap), "check delivery status" ("check" is gap), "monitor shipment progress" ("monitor" is gap), "describe order states" ("describe" is gap), "execute order tracking flow" ("execute" is gap). "order" and "delivery" alone are not triggers.

---

## E035-002 — OrderTrackingTest: status badge fires UI

A: "Assert the status badge shows the correct tracking state." Does UI fire?
B: Yes. "status badge" → "badge" → UI fires. TEST fires (assert). Result: [TEST, UI].

---

## E035-003 — OrderTrackingTest: timeline component fires UI

A: "Verify the timeline component renders each fulfillment step." Does UI fire?
B: Yes. "timeline component" → "component" → UI fires. TEST fires (verify). Result: [TEST, UI].

---

## E035-004 — OrderTrackingTest: order tracking page fires UI

A: "Create E2E tests for the order tracking page." Does UI fire?
B: Yes. "order tracking page" → "page" → UI fires. TEST fires (create E2E tests). Result: [TEST, UI].

---

## E035-005 — OrderTrackingTest: accordion fires UI

A: "Assert the order summary accordion expands correctly." Does UI fire?
B: Yes. "accordion" is in the UI lexicon. "order summary accordion" fires UI via "accordion". TEST fires (assert). Result: [TEST, UI].
