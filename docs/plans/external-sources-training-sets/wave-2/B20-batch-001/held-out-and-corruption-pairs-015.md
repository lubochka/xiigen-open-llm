# Held-Out and Corruption Pairs (015)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06

---

## HO-015-001 — "assert" fires TEST for DiscountEngine combinability

A: "Assert that the DiscountEngine returns false for CanCombine when both discounts are percentage-off type."
Expected: TEST fires on "assert". CompiledPredicateIds: ["TEST"]. Routing: 5.1. TransitionType: documentation.

---

## HO-015-002 — "develop test scenarios" fires TEST+IMPLEMENT

A: "Develop test scenarios for the DiscountEngine: first-time buyer discount, loyalty bonus, and staff discount."
Expected: IMPLEMENT fires on "develop". TEST fires on "test scenarios" (via "test"). CompiledPredicateIds: ["TEST", "IMPLEMENT"].
TransitionType: feature_addition. "first-time buyer" — domain term — gap.

---

## COR-015-001 — v6-01: TEST suppressed for "validate discount threshold"

CORRUPT: "Validate that the DiscountEngine applies zero discount to carts below the minimum threshold." → CompiledPredicateIds: [].
Why corrupt: The corrupt model treated "validate" as a business-rule validation term outside the TEST lexicon.
Correct: "validate" fires TEST universally in ALL contexts. CompiledPredicateIds: ["TEST"]. Routing: 5.1.
Critical: the "validate fires TEST universally" rule has no domain exception.

---

## COR-015-002 — v6-03: TEST and IMPLEMENT collapsed to IMPLEMENT only

CORRUPT: "Write unit tests for the DiscountEngine." → CompiledPredicateIds: ["IMPLEMENT"].
Why corrupt: The corrupt model fired only IMPLEMENT from "write" and suppressed TEST from "unit tests".
Correct: Both fire independently. CompiledPredicateIds: ["TEST", "IMPLEMENT"]. Two routing decisions.
The object of "write" ("unit tests") fires TEST. The verb "write" fires IMPLEMENT. Neither suppresses the other.
