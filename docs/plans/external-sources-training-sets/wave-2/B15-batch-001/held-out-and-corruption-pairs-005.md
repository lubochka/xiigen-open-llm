# Held-Out and Corruption Pairs (005)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: held_out_corruption

## HC-005-A — Held-out: IMPLEMENT+TEST+DOCUMENT three predicates
A: "Implement the pricing engine, write unit tests for tiered discounts, and document the pricing rules."
B: Predicates: ["IMPLEMENT", "TEST", "DOCUMENT"] | ImplementationFlow41 + UnitTestingFlow51 ‖ DocumentationFlow | feature_addition

## HC-005-B — Held-out: four predicates fire
A: "Build the checkout API, integrate with Stripe, write E2E tests, and document the checkout API."
B: Predicates: ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"] | feature_addition

## HC-005-C — Corruption v6-03: four predicates collapsed to IMPLEMENT
v6 corrupt: Above request → Predicates: ["IMPLEMENT"] → one flow
Correct: ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"] → four flows
Error: v6-03 predicate collapse.
