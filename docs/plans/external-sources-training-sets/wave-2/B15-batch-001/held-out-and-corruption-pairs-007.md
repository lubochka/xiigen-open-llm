# Held-Out and Corruption Pairs (007)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: held_out_corruption

## HC-007-A — Held-out: IMPLEMENT+AUTHORIZATION
A: "Build the order management API and add role-based access so only managers can process refunds."
B: Predicates: ["IMPLEMENT", "AUTHORIZATION"] | ImplementationFlow41 + AuthorizationFlow | feature_addition

## HC-007-B — Held-out: "bootstrap" fires IMPLEMENT
A: "Bootstrap the order event consumer with retry logic and dead-letter queue."
B: Predicates: ["IMPLEMENT"] | ImplementationFlow41 | feature_addition

## HC-007-C — Corruption v6-01: ORDER_PROCESSING invented
v6 corrupt: "Implement order processing" → ["ORDER_PROCESSING"] → OrderProcessingFlow
Correct: ["IMPLEMENT"] → ImplementationFlow41
Error: v6-01 — ORDER_PROCESSING predicate does not exist.
