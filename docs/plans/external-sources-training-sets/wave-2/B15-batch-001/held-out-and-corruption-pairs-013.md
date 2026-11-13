# Held-Out and Corruption Pairs (013)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: held_out_corruption

## HC-013-A — Held-out: OrderFulfillmentService with carrier integration

A: "Build the order fulfillment service and integrate with the carrier dispatch API for real-time tracking."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | feature_addition | APIContractFlow33 before ImplementationFlow41

## HC-013-B — Held-out: "dispatch" does not fire API_CONTRACT

A: "Build the order fulfillment service that dispatches completed orders."
B: Predicates: ["IMPLEMENT"] | feature_addition | "dispatch" is not in API_CONTRACT lexicon

## HC-013-C — Corruption v6-01: "fulfillment" treated as gap

v6 corrupt: "Build the order fulfillment service" → CompiledPredicateIds: [] ("fulfillment is domain terminology")
Correct: Predicates: ["IMPLEMENT"] | "build" fires IMPLEMENT unconditionally; domain nouns do not suppress predicate detection.
Error: v6-01 knowledge edge suppression.

## HC-013-D — Corruption v6-02: "packing model" → ARCHITECTURE suppressed

v6 corrupt: "Implement the fulfillment service and design the packing model" → Predicates: ["IMPLEMENT"]
Correct: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | "model" fires ARCHITECTURE.
Error: v6-02.

## HC-013-E — Corruption v6-03: IMPLEMENT+API_CONTRACT collapsed

v6 corrupt: "Build the fulfillment service and integrate with carrier API" → DecisionCount: 1 (only ImplementationFlow41)
Correct: DecisionCount: 2 — ImplementationFlow41 + APIContractFlow33
Error: v6-03 multi-branch collapse.
