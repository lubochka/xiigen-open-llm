# Layer 0-B — Router Pairs (041)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-041-001 — NotificationTest: routing [TEST, UI]

A: "Write E2E tests for the notification panel — assert badge and toast." Predicates [TEST, UI]. Routing?
B: TEST → 5.3. UI → 1.5 (panel, badge, toast — three triggers → one UI). AllSubPhaseIds: [5.3, 1.5]. TransitionType: documentation.

---

## C0B-041-002 — NotificationTest: "monitor notifications" gap — no routing

A: "Monitor the notification queue." Predicates []. Routing?
B: Zero predicates → knowledge_edge_synthetic. No routing.

---

## C0B-041-003 — NotificationTest: notification dropdown → 1.5

A: "Assert the notification settings dropdown." AllSubPhaseIds?
B: AllSubPhaseIds: [5.3, 1.5]. Dropdown → 1.5.

---

## C0B-041-004 — NotificationTest: IMPLEMENT → 4.1 added

A: [IMPLEMENT, TEST, UI]. AllSubPhaseIds?
B: [4.1, 5.3, 1.5]. TransitionType: feature_addition.

---

## C0B-041-005 — NotificationTest: badge always routes to 1.5

A: Does a "notification badge" ever route to 5.3?
B: No. Badge is UI vocabulary — always routes to 1.5. UIInteractionFlow15 handles all badge interactions in B39.
