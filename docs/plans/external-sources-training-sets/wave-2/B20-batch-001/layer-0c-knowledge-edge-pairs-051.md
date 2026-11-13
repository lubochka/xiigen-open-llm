# Layer 0-C — Knowledge Edge Pairs (051) — DateTimeCalc: gap analysis for datetime-domain verbs
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E051-001 — "calculate date difference" does NOT fire IMPLEMENT

"Calculate the date difference between the start date and end date using DateTimeCalc." → CompiledPredicateIds: []
"calculate" → domain computation verb; zero predicates.
Datetime computation verbs: calculate, compute, determine, derive, measure — all zero predicates.

---

## E051-002 — "leap year" domain term does NOT fire TEST

"Ensure the DateTimeCalc handles leap year boundary dates correctly." → CompiledPredicateIds: []
"ensure" → informal QA; zero predicates. "leap year" → domain calendar term; does not fire TEST.
Gap: instruction using only informal QA + domain terminology.

---

## E051-003 — "elapsed time" does NOT fire any predicate

"Determine the elapsed time between two events using the DateTimeCalc." → CompiledPredicateIds: []
"determine" → domain computation verb; zero predicates. "elapsed time" → domain term; zero predicates.

---

## E051-004 — "February 30" domain boundary; TEST fires from "validate" not from date invalidity

"Validate that the DateTimeCalc rejects February 30 as an invalid date." → CompiledPredicateIds: ["TEST"]
  "validate" → TEST fires. "February 30" → domain example; does not independently fire any predicate.
"The DateTimeCalc should reject February 30 as an invalid date." → CompiledPredicateIds: []
  No action verb from any predicate lexicon. Statement describes expected behaviour without a TEST or IMPLEMENT verb.
Teaching: the invalid date does not fire TEST; the verb "validate" fires TEST.
