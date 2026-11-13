# Held-Out and Corruption Pairs (014)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: held_out_corruption

## HC-014-A — Held-out: RefundService three predicates

A: "Implement the refund service, define the payment reversal contracts, and write integration tests."
B: Predicates: ["IMPLEMENT", "API_CONTRACT", "TEST"] | feature_addition | APIContractFlow33 → ImplementationFlow41 → TestingFlow51

## HC-014-B — Held-out: "chargeback" is not a gap

A: "Build the chargeback dispute management service."
B: Predicates: ["IMPLEMENT"] | feature_addition | "chargeback" is domain noun; "build" fires IMPLEMENT unconditionally.

## HC-014-C — Corruption v6-01: "chargeback" suppresses IMPLEMENT

v6 corrupt: "Implement the chargeback service" → CompiledPredicateIds: [] ("chargeback not in lexicon")
Correct: Predicates: ["IMPLEMENT"] | "implement" fires regardless of domain noun.
Error: v6-01.

## HC-014-D — Corruption v6-02: "define the payment contracts" → API_CONTRACT suppressed

v6 corrupt: "Implement the refund service and define the payment contracts" → Predicates: ["IMPLEMENT", "DOCUMENT"]
Correct: Predicates: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"] | "contract" fires API_CONTRACT; "define" fires DOCUMENT; both additional predicates fire.
Error: v6-02 partial suppression — API_CONTRACT not caught.

## HC-014-E — Held-out: refund four-predicate pipeline

A: "Implement refund service, define payment reversal API contracts, write unit tests, and document the refund API."
B: Predicates: ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"] | feature_addition | DecisionCount: 4
