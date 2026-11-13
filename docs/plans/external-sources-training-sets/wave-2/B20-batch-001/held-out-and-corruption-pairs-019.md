# Held-Out and Corruption Pairs (019)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06

---

## HO-019-001 — "write tests" + "implement test doubles" fires TEST+IMPLEMENT

A: "Write unit tests for FraudScoreCalculator and implement the test doubles for the risk-scoring API."
Expected: IMPLEMENT fires on "write" + "implement". TEST fires on "unit tests" + "test doubles".
CompiledPredicateIds: ["TEST", "IMPLEMENT"]. TransitionType: feature_addition.
Note: Deduplication keeps one TEST and one IMPLEMENT predicate, regardless of how many tokens fire each.

---

## HO-019-002 — "assert" velocity threshold fires TEST only

A: "Assert that the FraudScoreCalculator increases the risk score for accounts exceeding 5 transactions in 10 minutes."
Expected: TEST fires on "assert". CompiledPredicateIds: ["TEST"]. Routing: 5.1. TransitionType: documentation.
"risk score" — domain term — no SECURITY (risk ≠ security predicate vocabulary).

---

## COR-019-001 — v6-02: SECURITY invented for "fraud score" domain vocabulary

CORRUPT: "Write unit tests for the FraudScoreCalculator." → CompiledPredicateIds: ["TEST", "IMPLEMENT", "SECURITY"].
Why corrupt: "fraud" does not fire SECURITY. SECURITY fires on: security, auth, JWT, token, encryption, authorization,
  vulnerability, hashing, signing, tampered. "fraud" is a domain noun, not SECURITY vocabulary.
Correct: CompiledPredicateIds: ["TEST", "IMPLEMENT"]. Two predicates.

---

## COR-019-002 — v6-03: "run regression tests" promoted to TEST

CORRUPT: "Run regression tests for the FraudScoreCalculator nightly." → CompiledPredicateIds: ["TEST"].
Why corrupt: "regression tests" as an object of "run" was used to promote "run" to TEST.
Correct: "run" never fires TEST. Zero predicates → knowledge_edge.
"regression tests" fires TEST only when the action verb is a TEST/IMPLEMENT verb (create, write, develop, spec...).
