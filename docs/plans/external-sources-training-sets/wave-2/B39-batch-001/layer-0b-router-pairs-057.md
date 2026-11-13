# Layer 0-B — Router Pairs (057)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-057-001 — PushNotificationTest: routing

A: "Write E2E tests for push notifications — assert permission modal and verify toast." Predicates [TEST, UI]. Routing?
B: TEST → 5.3. UI → 1.5 (modal, toast). AllSubPhaseIds: [5.3, 1.5].

---

## C0B-057-002 — PushNotificationTest: "subscribe to push" gap

A: "Subscribe to push notifications." Routing?
B: knowledge_edge_synthetic. No routing.

---

## C0B-057-003 — PushNotificationTest: notification badge → 1.5

A: Does a "notification badge" in push notification context route to 1.5?
B: Yes. Badge → 1.5 always regardless of context.

---

## C0B-057-004 — PushNotificationTest: IMPLEMENT → 4.1

A: "Implement push notification support and write E2E tests for the notification panel." AllSubPhaseIds?
B: [4.1, 5.3, 1.5]. TransitionType: feature_addition.

---

## C0B-057-005 — PushNotificationTest: permission modal routing

A: Does a "push notification permission modal" route to 5.3 or 1.5?
B: 1.5. Modal → 1.5 always. The "permission" modifier does not affect the routing — "modal" is the UI trigger token and determines the routing.
