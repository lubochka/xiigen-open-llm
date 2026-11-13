# Layer 0-A — Compiler Pairs (035)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-035-001 — OrderTrackingTest: predicate compilation

A: "Write E2E test cases for the order tracking page — assert the status breadcrumb reflects the current shipping stage, verify the tracking timeline component renders all milestones."
B: Predicates: TEST ("write E2E test cases", "assert", "verify" → deduplicated); UI ("order tracking page", "status breadcrumb", "tracking timeline component" → "page", "breadcrumb", "component" fire UI, deduplicated). Result: [TEST, UI]. Routing: [5.3, 1.5]. TransitionType: documentation.

---

## C0A-035-002 — OrderTrackingTest: map component fires UI

A: "Create test specs for the order tracking map component — assert the pin marker updates when the order status changes."
B: Predicates: TEST ("create test specs", "assert" → deduplicated); UI ("order tracking map component" → "component" fires UI). Result: [TEST, UI]. Routing: [5.3, 1.5]. TransitionType: documentation.

---

## C0A-035-003 — OrderTrackingTest: skeleton loader fires UI

A: "Add test coverage for the order tracking skeleton loader — assert it appears within 200ms of page load and is replaced by the tracking component."
B: Predicates: TEST ("add test coverage", "assert" → deduplicated); UI ("skeleton loader", "tracking component" → "skeleton", "component" fire UI, deduplicated). Result: [TEST, UI]. Routing: [5.3, 1.5]. TransitionType: documentation.

---

## C0A-035-004 — OrderTrackingTest: IMPLEMENT co-fires

A: "Implement the order tracking breadcrumb component, write E2E tests for three shipping stages, verify the breadcrumb highlights the active stage."
B: Predicates: IMPLEMENT ("implement"); TEST ("write E2E tests", "verify" → deduplicated); UI ("order tracking breadcrumb component" → "breadcrumb", "component" fire UI, deduplicated). Result: [IMPLEMENT, TEST, UI]. TransitionType: feature_addition. Routing: [4.1, 5.3, 1.5].

---

## C0A-035-005 — OrderTrackingTest: QA co-fires

A: "Write E2E test scenarios for the full order tracking flow, assert the notification toast fires on status change, and QA sign-off before release."
B: Predicates: TEST ("write", "assert" → deduplicated); UI ("notification toast" → "toast" fires UI); QA ("QA sign-off" → QA; "before release" → release vocabulary: QA routes to 7.1). Result: [TEST, UI, QA]. Routing: [5.3, 1.5, 7.1]. TransitionType: documentation.
