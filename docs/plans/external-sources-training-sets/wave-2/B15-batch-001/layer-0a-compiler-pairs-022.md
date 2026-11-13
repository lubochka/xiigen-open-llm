# Layer 0-A — Compiler Pairs (022)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-022-001 — PricingEngineService: "implement" → IMPLEMENT

A: "Implement the pricing engine service that calculates product prices based on base cost, margin rules, and promotional discounts."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition | Sub-phase: 4.1
Evidence: implement → IMPLEMENT; pricing engine service → backend service noun.

---

## B15-compiler-022-002 — PricingEngineService: "refactor" → IMPLEMENT with TransitionType: refactoring

A: "Refactor the pricing engine to separate discount calculation from margin calculation into distinct strategy classes."
B: Predicates: ["IMPLEMENT"] | TransitionType: refactoring
Evidence: refactor → IMPLEMENT; TransitionType: refactoring (refactor verb present → overrides feature_addition).

---

## B15-compiler-022-003 — PricingEngineService: "refactor" + "write tests" → IMPLEMENT+TEST, TransitionType: refactoring

A: "Refactor the pricing engine service and write regression tests to verify pricing parity before and after refactor."
B: Predicates: ["IMPLEMENT", "TEST"] | TransitionType: refactoring
Evidence: refactor → IMPLEMENT (TransitionType: refactoring); "write regression tests" → TEST. TransitionType: refactoring still applies even when TEST also fires.

---

## B15-compiler-022-004 — v6-01 corruption: refactoring TransitionType lost

v6 corrupt: "Refactor the pricing engine service" → Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Correct: Predicates: ["IMPLEMENT"] | TransitionType: refactoring
Error type: v6-01. The corrupt model correctly identified IMPLEMENT but defaulted to feature_addition. "refactor" verb present → TransitionType must be refactoring.

---

## B15-compiler-022-005 — PricingEngineService: four predicates with refactoring

A: "Refactor the pricing engine, define the pricing API contracts, write tests for the new strategy pattern, and document the pricing rules."
B: Predicates: ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"] | TransitionType: refactoring
Evidence: refactor → IMPLEMENT (TransitionType: refactoring); "API contracts" → API_CONTRACT; write tests → TEST; document → DOCUMENT. Four predicates with refactoring TransitionType.
