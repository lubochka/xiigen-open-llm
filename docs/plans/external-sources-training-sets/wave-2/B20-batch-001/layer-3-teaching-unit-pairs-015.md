# Layer 3 — Teaching Unit Pairs (015)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## Teach-015-001 — "verify" fires TEST for DiscountEngine expiry

A: "Verify that the DiscountEngine rejects expired discount codes." What fires?
B: "verify" fires TEST → 5.1. "expired discount codes" — domain — no predicate. Result: ["TEST"].
   TransitionType: documentation (no IMPLEMENT verb).

---

## Teach-015-002 — "develop test scenarios" fires TEST+IMPLEMENT

A: "Develop test scenarios for the DiscountEngine covering zero-value and maximum-cap cases." What fires?
B: IMPLEMENT fires on "develop". TEST fires on "test scenarios" (via "test"). Two predicates.
   "scenarios" alone is gap; "test scenarios" compound fires TEST via "test".
   TransitionType: feature_addition.

---

## Teach-015-003 — "BOGO" (Buy One Get One) domain term fires no predicate

A: "The DiscountEngine must implement BOGO (Buy One Get One) discounts." What fires?
B: IMPLEMENT fires on "implement". "BOGO" is a domain discount type — no predicate.
   "must" is a modal — no predicate. Result: ["IMPLEMENT"] — TransitionType: feature_addition.
   Note: This is an IMPLEMENT-only request. No TEST fires because there is no test vocabulary.

---

## Teach-015-004 — "mock" alone is gap; "mock" + "test" fires TEST

A: "Mock the discount repository for the DiscountEngine." vs
   "Write tests that mock the discount repository for the DiscountEngine." What fires in each?
B: "Mock the discount repository..." → zero predicates → knowledge_edge. "mock" alone has no predicate.
   "Write tests that mock the discount repository..." → TEST fires on "tests"; IMPLEMENT fires on "write"; "mock" → TEST (test-infrastructure in "tests" context).
   The difference is whether "tests" appears as the primary action object. "mock" alone = gap.
