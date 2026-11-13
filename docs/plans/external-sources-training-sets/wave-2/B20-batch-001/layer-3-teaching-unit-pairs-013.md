# Layer 3 — Teaching Unit Pairs (013)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## Teach-013-001 — "write unit tests" fires both TEST and IMPLEMENT

A: "Write unit tests for the CartCalculationService." What predicates fire?
B: TEST fires on "unit tests". IMPLEMENT fires on "write". Two predicates fire independently:
   TEST → 5.1 (UnitTestingFlow51), IMPLEMENT → 4.1 (BackendServiceFlow41).
   The noun object "unit tests" fires TEST. The action verb "write" fires IMPLEMENT.
   Both fire without suppressing each other.

---

## Teach-013-002 — "assert" alone fires TEST; TransitionType is documentation

A: "Assert that the CartCalculationService returns zero when the cart is empty." What fires? What is the TransitionType?
B: "assert" fires TEST → sub-phase 5.1. No IMPLEMENT verb is present. One predicate → one routing decision.
   TransitionType: documentation (TEST alone = asserting/specifying expected behaviour, not constructing).
   If "write" were added ("Write a test that asserts..."), IMPLEMENT would also fire → feature_addition.

---

## Teach-013-003 — "validate" always fires TEST (cart domain example)

A: "Validate that discounts are not applied to items marked as non-discountable." Does TEST fire?
B: Yes. "validate" fires TEST universally — in data validation context, business-rule context,
   and test-assertion context. The domain ("items marked as non-discountable") does not affect
   whether TEST fires. "validate" is always TEST. Result: ["TEST"].

---

## Teach-013-004 — "check" does NOT fire TEST (cart domain)

A: "Check that the cart total updates correctly when an item quantity is changed." Does TEST fire?
B: No. "check" is not in the TEST predicate lexicon. TEST fires on: test, verify, assert, validate,
   spec, mock, stub, fixture, scenario, regression (test compound), harness, coverage (test compound).
   "Check" is informal inspection language. Result: zero predicates → knowledge_edge.
   Correction: "Verify that the cart total updates correctly..." → TEST fires.

---

## Teach-013-005 — "run tests" does NOT fire TEST (cart domain)

A: "Run the CartCalculationService unit tests before merging." Does TEST fire?
B: No. "run" is an execution verb. TEST does not fire. "unit tests" is the object of "run" and
   cannot promote "run" to a TEST predicate. Zero predicates → knowledge_edge.
   This is the canonical "run ≠ TEST" boundary: run=execution, write/assert/verify/spec=testing.
