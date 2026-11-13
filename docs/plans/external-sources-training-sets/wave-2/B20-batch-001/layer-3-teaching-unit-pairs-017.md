# Layer 3 — Teaching Unit Pairs (017)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## Teach-017-001 — "fixture" fires TEST (ShippingCost domain)

A: "Build a test fixture for the ShippingCostCalculator with pre-loaded carrier rates." What fires?
B: IMPLEMENT fires on "build". TEST fires on "test fixture" — "fixture" is test-infrastructure vocabulary
   that fires TEST in compound with "test". Two predicates: TEST + IMPLEMENT. TransitionType: feature_addition.

---

## Teach-017-002 — "confirms" does NOT fire TEST

A: "Build a test fixture that stubs the carrier API and confirms ShippingCostCalculator handles timeouts." What fires?
B: IMPLEMENT fires on "build". TEST fires on "test fixture" + "stubs". "confirms" does NOT fire TEST.
   "confirms" is informal verification language — same family as "check", "ensure", "make sure" — gap.
   Two predicates: TEST + IMPLEMENT (from "build" + "test fixture" + "stubs"). "confirms" adds nothing.
   Teaching: Only confirm → not TEST. The TEST fires from "fixture" and "stubs", not from "confirms".

---

## Teach-017-003 — "free shipping" threshold boundary test

A: "Assert that the ShippingCostCalculator returns zero cost when the order total equals the free-shipping threshold exactly." What fires?
B: "assert" fires TEST → 5.1. "free shipping" — domain feature — no predicate. "threshold" — domain term — gap.
   "zero cost" — domain value — no predicate. Result: ["TEST"] — TransitionType: documentation.

---

## Teach-017-004 — "run tests" after a refactor — still does NOT fire TEST

A: "Run the ShippingCostCalculator tests after the weight-tier refactor to check nothing broke." What fires?
B: Zero predicates. "run" is execution — NOT TEST. "refactor" might seem like IMPLEMENT but it is a domain
   qualifier here ("after the refactor"), not an action verb directed at the system. "check" is gap.
   Result: zero predicates → knowledge_edge. The "run ≠ TEST" boundary is absolute — no surrounding
   context (post-refactor, pre-release, nightly) changes this outcome.
