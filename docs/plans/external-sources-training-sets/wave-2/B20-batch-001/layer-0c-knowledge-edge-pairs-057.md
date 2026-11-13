# Layer 0-C — Knowledge Edge Pairs (057) — StatisticsCalc: gap analysis for statistics-domain verbs
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E057-001 — "compute the mean" does NOT fire IMPLEMENT

"Compute the mean of the dataset using StatisticsCalc." → CompiledPredicateIds: []
"compute" → domain computation verb; zero predicates.
Statistics computation verbs: compute, calculate, derive, estimate, measure, aggregate — all zero predicates.

---

## E057-002 — "determine the median" does NOT fire IMPLEMENT

"Determine the median value from the sales data array using StatisticsCalc." → CompiledPredicateIds: []
"determine" → domain computation verb; zero predicates.

---

## E057-003 — "standard deviation" domain term does NOT fire TEST

"Ensure the StatisticsCalc calculates standard deviation correctly." → CompiledPredicateIds: []
"ensure" → informal QA; zero predicates. "standard deviation" → domain statistical measure; does not fire TEST.
"calculate" → also zero predicates (domain computation verb).

---

## E057-004 — "InvalidOperationException" domain term does NOT fire TEST independently

"Assert that the StatisticsCalc throws InvalidOperationException for an empty dataset." → ["TEST"]
  "assert" → TEST fires. Exception type is domain vocabulary; does not fire predicates.
"The StatisticsCalc should throw InvalidOperationException for an empty dataset." → []
  No action verb from any predicate lexicon. "should throw" is a design intention phrase; does not fire TEST.
Teaching: "should throw" (modal + domain verb) does not fire TEST. Only explicit test-lexicon verbs fire TEST.
