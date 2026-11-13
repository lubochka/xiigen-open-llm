# Layer 3 — Teaching Unit Pairs (023)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## Teach-023-001 — "validate coupon" fires TEST; "apply coupon" fires IMPLEMENT

A: "Validate that the CouponApplicator applies the SUMMER20 code correctly." vs
   "Apply the SUMMER20 coupon code to the CartCalculationService." What fires in each?
B: "Validate that..." → "validate" fires TEST (universal) → ["TEST"]. TransitionType: documentation.
   "Apply the SUMMER20 coupon code..." → "apply" fires IMPLEMENT → ["IMPLEMENT"]. TransitionType: feature_addition.
   Teaching: "validate" = TEST. "apply" = IMPLEMENT. Same noun object ("coupon code"), different predicates from the verb.

---

## Teach-023-002 — "run the coupon test suite" does NOT fire TEST

A: "Run the coupon test suite after deploying the new stacking rules." What fires?
B: Zero predicates. "run" — execution verb — NOT TEST. "test suite" is the object of "run" — cannot elevate "run" to TEST.
   "deploying" could fire IMPLEMENT? "deploying" in "after deploying" is a temporal qualifier — not an action verb directed at the system.
   Zero predicates → knowledge_edge.
   Teaching: "after deploying" is temporal context, not a deployment action that fires IMPLEMENT.

---

## Teach-023-003 — "stacking rules" domain term fires no predicate

A: "The CouponApplicator stacking rules allow at most one percentage-off and one fixed-amount coupon." What fires?
B: Zero predicates. This is a description of business rules, not a request with action vocabulary.
   "stacking rules" — domain policy — no predicate. "allow at most" — requirement modal — no predicate.

---

## Teach-023-004 — "create test cases" with "mock" fires TEST+IMPLEMENT

A: "Create test cases for CouponApplicator using a mock coupon repository." What fires?
B: IMPLEMENT fires on "create". TEST fires on "test cases" (via "test") and "mock" (test-infrastructure).
   Deduplication: two TEST-contributing terms → one TEST predicate. Two predicates total: TEST + IMPLEMENT.
   TransitionType: feature_addition. "mock coupon repository" — the mock is a test-infrastructure artifact.
