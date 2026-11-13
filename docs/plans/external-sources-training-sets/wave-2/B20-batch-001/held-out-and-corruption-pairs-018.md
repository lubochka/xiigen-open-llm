# Held-Out and Corruption Pairs (018)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06

---

## HO-018-001 — "assert" points expiry fires TEST

A: "Assert that the LoyaltyPointsService marks all points older than 12 months as expired on the nightly run."
Expected: TEST fires on "assert". CompiledPredicateIds: ["TEST"]. Routing: 5.1. TransitionType: documentation.
"nightly run" — temporal qualifier — no predicate. "12 months" — domain value — no predicate.

---

## HO-018-002 — "create test cases" + tier multiplier fires TEST+IMPLEMENT

A: "Create unit test cases for LoyaltyPointsService verifying the gold tier 3x multiplier and silver tier 2x multiplier."
Expected: IMPLEMENT fires on "create". TEST fires on "unit test cases" + "verifying". CompiledPredicateIds: ["TEST", "IMPLEMENT"].
TransitionType: feature_addition. "3x multiplier" — domain value — gap.

---

## COR-018-001 — v6-03: QA and TEST collapsed to TEST only

CORRUPT: "QA the loyalty points flow and assert the accrual rate is correct." → CompiledPredicateIds: ["TEST"], SubPhase: 5.1.
Why corrupt: "QA" fires QA predicate → 7.3. "assert" fires TEST → 5.1. Two predicates → two routing decisions.
Collapsing both to TEST at 5.1 is v6-03 multi-branch result collapse.
Correct: CompiledPredicateIds: ["QA", "TEST"]. Routing: [QA → 7.3, TEST → 5.1]. Two decisions.

---

## COR-018-002 — v6-01: "testable" adjective promoted to TEST

CORRUPT: "Make the LoyaltyPointsService testable by injecting its dependencies." → CompiledPredicateIds: ["TEST", "IMPLEMENT"].
Why corrupt: "testable" is an adjective. Only noun/verb/gerund forms of "test" fire TEST. Adjective form is excluded.
Correct: IMPLEMENT fires on "injecting" (dependency injection = construction action). TEST does NOT fire.
CompiledPredicateIds: ["IMPLEMENT"]. TransitionType: refactoring.
