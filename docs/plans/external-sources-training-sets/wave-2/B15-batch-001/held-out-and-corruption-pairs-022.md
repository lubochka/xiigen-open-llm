# Held-Out and Corruption Pairs (022)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: held_out_corruption

## HC-022-A — Held-out: PricingEngine refactoring four predicates

A: "Refactor the pricing engine, define pricing API contracts, write regression tests, and document pricing rules."
B: Predicates: ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"] | TransitionType: refactoring | DecisionCount: 4

## HC-022-B — Held-out: "strategy pattern" does not fire ARCHITECTURE

A: "Refactor the pricing engine to use a strategy pattern for discount calculation."
B: Predicates: ["IMPLEMENT"] | TransitionType: refactoring | "strategy pattern" is an implementation design pattern noun — not an ARCHITECTURE trigger.

## HC-022-C — Corruption v6-01: refactoring TransitionType lost

v6 corrupt: "Refactor the pricing engine" → Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Correct: TransitionType: refactoring
Error: v6-01 metadata loss. TransitionType: refactoring must be set when "refactor" verb is detected.

## HC-022-D — Corruption v6-02: "strategy classes" fires ARCHITECTURE (inverse error)

v6 corrupt (inverse): "Refactor the pricing engine to use strategy classes" → Predicates: ["IMPLEMENT", "ARCHITECTURE"]
Correct: Predicates: ["IMPLEMENT"] | TransitionType: refactoring | "strategy classes" is a code pattern noun — not ARCHITECTURE.
Error: v6-02 inverse (spurious ARCHITECTURE from pattern noun).

## HC-022-E — Held-out: refactor + extend → TransitionType: refactoring

A: "Refactor the pricing engine and extend it to support bundle pricing."
B: Predicates: ["IMPLEMENT"] | TransitionType: refactoring | "refactor" wins over "extend" for TransitionType; single IMPLEMENT predicate after deduplication.
