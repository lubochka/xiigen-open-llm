# Layer 0-B — Router Pairs (035)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-035-001 — OrderTrackingTest: routing decision

A: "Write E2E test cases for the order tracking page — assert the breadcrumb, verify the timeline component." Predicates: [TEST, UI]. Route.
B: TEST → 5.3. UI → 1.5 ("order tracking page", "status breadcrumb", "tracking timeline component" → "page", "breadcrumb", "component" fire UI → 1.5). TransitionType: documentation.

---

## C0B-035-002 — OrderTrackingTest: skeleton loader routing

A: "Add test coverage for the order tracking skeleton loader — assert it appears within 200ms and is replaced by the tracking component." Predicates: [TEST, UI]. Route.
B: TEST → 5.3. UI → 1.5 ("skeleton loader", "tracking component" → "skeleton" and "component" fire UI → 1.5). Skeleton loader vocabulary is explicitly in the UI lexicon.

---

## C0B-035-003 — OrderTrackingTest: IMPLEMENT routing

A: "Implement the order tracking breadcrumb component, write E2E tests for three shipping stages." Predicates: [IMPLEMENT, TEST, UI]. Route.
B: IMPLEMENT → 4.1. TEST → 5.3. UI → 1.5 ("order tracking breadcrumb component" → "breadcrumb", "component" fire UI → 1.5). TransitionType: feature_addition.

---

## C0B-035-004 — OrderTrackingTest: toast fires UI

A: "Write E2E test scenarios for the order tracking flow, assert the notification toast fires on status change." Predicates: [TEST, UI]. Route.
B: TEST → 5.3. UI → 1.5 ("notification toast" → "toast" fires UI → 1.5). Toast is explicitly in the UI lexicon (snackbar/toast).

---

## C0B-035-005 — OrderTrackingTest: QA → 7.1 on release vocabulary

A: "Write E2E test scenarios, assert the toast, and QA sign-off before release." Predicates: [TEST, UI, QA]. Route QA.
B: QA → 7.1 (ReleaseGateFlow71): "before release" is explicit release vocabulary → overrides default 7.3. TEST → 5.3. UI → 1.5. AllSubPhaseIds: [5.3, 1.5, 7.1]. TransitionType: documentation.
