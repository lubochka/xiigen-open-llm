# Held-Out and Corruption Pairs (016)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06

---

## HO-016-001 — "write unit tests" + "stubs" fires TEST+IMPLEMENT

A: "Write unit tests for the TaxCalculationService using stubs for the external tax authority API."
Expected: IMPLEMENT fires on "write". TEST fires on "unit tests" and "stubs". CompiledPredicateIds: ["TEST", "IMPLEMENT"].
TransitionType: feature_addition. "stubs" contributes to TEST (test-infrastructure vocabulary).

---

## HO-016-002 — "spec" for zero-rated goods fires TEST only

A: "Spec the zero-rated goods classification: children's clothing (sizes 0-12), unprocessed food, and prescription medicines are always zero-rated."
Expected: TEST fires on "spec". CompiledPredicateIds: ["TEST"]. Routing: 5.1. TransitionType: documentation.
"zero-rated", "children's clothing" — domain terms — gap.

---

## COR-016-001 — v6-02: SECURITY invented for "tax compliance"

CORRUPT: "Write unit tests for the TaxCalculationService to verify EU tax compliance." → CompiledPredicateIds: ["TEST", "IMPLEMENT", "SECURITY"].
Why corrupt: "compliance" is NOT SECURITY vocabulary. "EU tax compliance" is a domain phrase, not a security context.
Correct: CompiledPredicateIds: ["TEST", "IMPLEMENT"]. Two predicates. SECURITY does not fire here.
SECURITY fires on: security, auth, JWT, token, encryption, authorization, vulnerability, hashing, signing — not "compliance" in a tax context.

---

## COR-016-002 — v6-03: "run tests" promoted to TEST by object noun

CORRUPT: "Run the TaxCalculationService tests after updating the VAT rate tables." → CompiledPredicateIds: ["TEST"].
Why corrupt: The model promoted "run" to TEST because "tests" appeared in the sentence as an object.
Correct: "run" is an execution verb; TEST never fires from "run". Zero predicates → knowledge_edge.
