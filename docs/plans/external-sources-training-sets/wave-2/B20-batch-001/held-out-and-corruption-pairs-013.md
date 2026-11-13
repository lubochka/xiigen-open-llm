# Held-Out and Corruption Pairs (013)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06

---

## HO-013-001 — "spec" fires TEST; TransitionType documentation

A: "Spec the CartCalculationService behaviour when a non-existent product ID is added to the cart."
Expected: TEST fires on "spec". CompiledPredicateIds: ["TEST"]. Routing: 5.1. TransitionType: documentation.
"non-existent product ID" — domain term — gap.

---

## HO-013-002 — "build test fixture" fires TEST+IMPLEMENT

A: "Build a test fixture for CartCalculationService with a pre-seeded product catalogue."
Expected: TEST fires on "test fixture". IMPLEMENT fires on "build". CompiledPredicateIds: ["TEST", "IMPLEMENT"].
Two routing decisions: TEST → 5.1, IMPLEMENT → 4.1. TransitionType: feature_addition.

---

## COR-013-001 — v6-01: TEST suppressed for "verify cart subtotal"

CORRUPT: "Verify the cart subtotal is recalculated after removing an item." → CompiledPredicateIds: [].
Why corrupt: "verify" fires TEST universally. The corrupt model treated "verify" as an informal check and
fired zero predicates. Correct: CompiledPredicateIds: ["TEST"]. Routing: 5.1. TransitionType: documentation.

---

## COR-013-002 — v6-03: "run" promoted to TEST by corrupt model

CORRUPT: "Run unit tests for the CartCalculationService." → CompiledPredicateIds: ["TEST"].
Why corrupt: The corrupt model saw "unit tests" in the sentence and fired TEST, ignoring the action verb "run".
Correct: "run" is an execution verb. Zero predicates → knowledge_edge. The predicate must come from the action verb.
