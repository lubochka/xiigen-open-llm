# Layer 0-D — Flow Factory Pairs (035)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F035-001 — OrderTrackingTest: flow selection

A: "Create E2E test scenarios for the order tracking page — assert the status badge updates, verify the timeline component renders each step." Predicates [TEST, UI]. Flows?
B: E2ETestingFlow53 (5.3): TEST ("create E2E test scenarios", "assert", "verify"). UIInteractionFlow15 (1.5): UI ("order tracking page" → page; "status badge" → badge; "timeline component" → component). Flows: [E2ETestingFlow53, UIInteractionFlow15]. TransitionType: documentation.

---

## F035-002 — OrderTrackingTest: "track order" is gap

A: "Track the order and check its status." Predicates? Flows?
B: Zero predicates. "track" is gap. "check" is gap. "order", "status" — no UI vocabulary. Result: [] → knowledge_edge_synthetic. No FlowInstances.

---

## F035-003 — OrderTrackingTest: delivery status modal fires UI

A: "Add test coverage for the delivery status modal on the order tracking page." Flows?
B: TEST ("add test coverage"). UI ("delivery status modal" → modal; "order tracking page" → page). E2ETestingFlow53 (5.3) + UIInteractionFlow15 (1.5). TransitionType: documentation.

---

## F035-004 — OrderTrackingTest: QA before release fires 7.1

A: "Write order tracking E2E tests, assert the status badge, and QA sign-off before release." AllSubPhaseIds?
B: AllSubPhaseIds: [5.3, 1.5, 7.1]. QA → ReleaseGateFlow71 (7.1) — "before release" = release vocabulary. TransitionType: documentation. Order: 5.3 + 1.5 parallel → 7.1.

---

## F035-005 — OrderTrackingTest: Node 1 domains

A: Node 1 domain output for OrderTrackingTest.
B: SelectedLogicalTransition = E2ETestingFlow53. Domains: order tracking page (status badge, timeline component, step indicators), delivery status modal, estimated delivery date display, shipping address section, order summary accordion, tracking map component.
