# Held-Out and Corruption Pairs (002)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: held_out_corruption

## HC-002-A — Held-out: "build" alone
A: "Build the notification service for order confirmations."
B: Predicates: ["IMPLEMENT"] | ImplementationFlow41 | feature_addition

## HC-002-B — Held-out: "scaffold" fires IMPLEMENT
A: "Scaffold the returns microservice with standard middleware."
B: Predicates: ["IMPLEMENT"] | ImplementationFlow41 | feature_addition

## HC-002-C — Corruption v6-01: NOTIFICATION predicate invented
v6 corrupt: "Build order notification service" → ["NOTIFICATION"] → NotificationFlow
Correct: ["IMPLEMENT"] → ImplementationFlow41
Error: v6-01 — NOTIFICATION is not a predicate.
