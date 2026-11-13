# Held-Out and Corruption Pairs (017)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06

---

## HO-017-001 — "assert" free-shipping zero-cost

A: "Assert that the ShippingCostCalculator returns zero when the order qualifies for free shipping."
Expected: TEST fires on "assert". CompiledPredicateIds: ["TEST"]. Routing: 5.1. TransitionType: documentation.

---

## HO-017-002 — "build test fixture" fires TEST+IMPLEMENT for carrier mock

A: "Build a test fixture that stubs the carrier rate API and verifies ShippingCostCalculator timeout handling."
Expected: IMPLEMENT fires on "build". TEST fires on "test fixture" + "stubs" + "verifies". CompiledPredicateIds: ["TEST", "IMPLEMENT"].
TransitionType: feature_addition. Note: "verifies" fires TEST (morphological form of "verify").

---

## COR-017-001 — v6-01: "verify" treated as informal check

CORRUPT: "Verify that the ShippingCostCalculator transitions to the heavy rate at 5 kg." → CompiledPredicateIds: [].
Why corrupt: "verify" fires TEST universally. The corrupt model treated it as an informal confirmation (same family as "check").
Correct: CompiledPredicateIds: ["TEST"]. "verify" is canonical TEST vocabulary — never gap.

---

## COR-017-002 — v6-02: IMPLEMENT invented for "run unit tests"

CORRUPT: "Run unit tests for ShippingCostCalculator." → CompiledPredicateIds: ["IMPLEMENT"].
Why corrupt: The model treated "run" as an IMPLEMENT verb (executing = implementing test runner).
Correct: "run" is an execution verb — fires no predicate. Zero predicates → knowledge_edge.
"run" never fires IMPLEMENT. IMPLEMENT fires only on construction verbs: write, create, build, develop, implement, add, extend.
