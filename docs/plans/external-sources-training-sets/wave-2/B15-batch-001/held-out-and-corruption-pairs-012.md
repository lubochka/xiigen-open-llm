# Held-Out and Corruption Pairs (012)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: held_out_corruption

## HC-012-A — Held-out: full six-predicate B15 request
A: "Build the checkout service, integrate with Stripe, secure payment tokens, write integration tests, document the checkout API, and QA before launch."
B: Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "TEST", "DOCUMENT", "QA"] | feature_addition | QA last

## HC-012-B — Held-out: refactor + extend → refactoring TransitionType
A: "Refactor the order processing pipeline and extend it for multi-currency support."
B: Predicates: ["IMPLEMENT"] | TransitionType: refactoring (refactor verb wins over extend)

## HC-012-C — Corruption v6-03: six predicates collapsed to three
v6 corrupt: Above → ["IMPLEMENT", "SECURITY", "QA"] (API_CONTRACT, TEST, DOCUMENT lost)
Correct: All six predicates → six factories → six flows
Error: v6-03 predicate collapse — three predicates lost.
