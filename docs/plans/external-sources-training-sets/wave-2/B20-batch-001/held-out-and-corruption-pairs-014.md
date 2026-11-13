# Held-Out and Corruption Pairs (014)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06

---

## HO-014-001 — "validate" fires TEST for InventoryAllocation boundary

A: "Validate that the InventoryAllocationService prevents allocation of a quantity greater than 999 units per order."
Expected: TEST fires on "validate". CompiledPredicateIds: ["TEST"]. Routing: 5.1. TransitionType: documentation.
"999 units" — domain constraint — no additional predicate.

---

## HO-014-002 — "write tests" + "mock" fires TEST+IMPLEMENT

A: "Write tests for the InventoryAllocationService that mock the warehouse adapter."
Expected: IMPLEMENT fires on "write". TEST fires on "tests" and "mock". CompiledPredicateIds: ["TEST", "IMPLEMENT"].
TransitionType: feature_addition. "warehouse adapter" — domain term — no additional predicate.

---

## COR-014-001 — v6-01: "assert" suppressed as domain verb

CORRUPT: "Assert that the InventoryAllocationService raises an error for zero-quantity requests." → CompiledPredicateIds: ["IMPLEMENT"].
Why corrupt: The corrupt model treated "assert" as an IMPLEMENT verb (asserting = adding assertion code).
Correct: "assert" fires TEST, not IMPLEMENT. CompiledPredicateIds: ["TEST"]. Routing: 5.1.

---

## COR-014-002 — v6-02: IMPLEMENT invented for "confirm stock reserved"

CORRUPT: "Confirm that the InventoryAllocationService reserves stock for the order." → CompiledPredicateIds: ["IMPLEMENT"].
Why corrupt: "confirm" is gap, not IMPLEMENT. "reserves stock" sounds like implementation but is a domain description.
Correct: Zero predicates → knowledge_edge. "confirm" is not in any predicate lexicon.
