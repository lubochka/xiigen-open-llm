# Held-Out and Corruption Pairs (022)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06

---

## HO-022-001 — "assert" locale formatting fires TEST

A: "Assert that the PriceFormatter renders €1.234,56 for German EUR locale."
Expected: TEST fires on "assert". CompiledPredicateIds: ["TEST"]. Routing: 5.1. TransitionType: documentation.
"EUR" — domain currency code — no predicate. "German locale" — domain term — no predicate.

---

## HO-022-002 — "write unit tests" + "verifying" fires TEST+IMPLEMENT

A: "Write unit tests for the PriceFormatter verifying that rounding is always half-up with two decimal places."
Expected: IMPLEMENT fires on "write". TEST fires on "unit tests" + "verifying". CompiledPredicateIds: ["TEST", "IMPLEMENT"].
TransitionType: feature_addition.

---

## COR-022-001 — v6-03: "run tests" promoted to TEST by object noun

CORRUPT: "Run unit tests for the PriceFormatter after the German locale fix." → CompiledPredicateIds: ["TEST"].
Why corrupt: The model promoted "run" to TEST because "unit tests" is the direct object.
Correct: "run" is execution — never TEST. Zero predicates → knowledge_edge.

---

## COR-022-002 — v6-02: IMPLEMENT invented for "price display looks incorrect"

CORRUPT: "The German locale price display looks incorrect in the PriceFormatter." → CompiledPredicateIds: ["IMPLEMENT"].
Why corrupt: The model inferred that "incorrect" implies a bug fix (IMPLEMENT). Observation statements have no predicate.
Correct: Zero predicates → knowledge_edge. "looks incorrect" is an observation, not a construction request.
To fire IMPLEMENT: "Fix the German locale formatting in PriceFormatter." → ["IMPLEMENT"]. TransitionType: refactoring (bug fix).
