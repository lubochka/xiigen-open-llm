# Held-Out and Corruption Pairs (023)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06

---

## HO-023-001 — "assert" single-use coupon fires TEST

A: "Assert that the CouponApplicator marks a single-use coupon as consumed after its first successful application."
Expected: TEST fires on "assert". CompiledPredicateIds: ["TEST"]. Routing: 5.1. TransitionType: documentation.
"single-use coupon" — domain term — no predicate. "consumed" — domain state — no predicate.

---

## HO-023-002 — "create test cases" + "mock" fires TEST+IMPLEMENT

A: "Create test cases for CouponApplicator using a mock coupon repository and verify minimum spend enforcement."
Expected: IMPLEMENT fires on "create". TEST fires on "test cases" + "mock" + "verify". CompiledPredicateIds: ["TEST", "IMPLEMENT"].
TransitionType: feature_addition.

---

## COR-023-001 — v6-03: "run coupon test suite" promoted to TEST

CORRUPT: "Run the coupon test suite after deploying the new stacking rules." → CompiledPredicateIds: ["TEST"].
Why corrupt: "test suite" in the object position was used to fire TEST from "run".
Correct: "run" is execution — never fires TEST. Zero predicates → knowledge_edge.
The canonical rule: TEST fires from the action verb, not from nouns in the object position.

---

## COR-023-002 — v6-01: "validate coupon stacking" treated as business rule (not TEST)

CORRUPT: "Validate that the CouponApplicator allows at most one percentage-off and one fixed-amount coupon simultaneously." → CompiledPredicateIds: [].
Why corrupt: "validate" in a business-rule context was treated as a requirement statement (gap) rather than TEST vocabulary.
Correct: "validate" fires TEST in ALL contexts, including business-rule validation. CompiledPredicateIds: ["TEST"]. Routing: 5.1.
