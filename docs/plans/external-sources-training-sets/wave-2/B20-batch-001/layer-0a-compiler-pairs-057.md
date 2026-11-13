# Layer 0-A — Compiler Pairs (057) — StatisticsCalc: TEST fires on test vocabulary, not stats-domain terms
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P057-001 — TEST+IMPLEMENT: "create StatisticsCalc and write tests"

A: "Create the StatisticsCalc class and write tests for mean, median, and standard deviation."
  "create" → IMPLEMENT | "write" → IMPLEMENT (deduplicates) | "tests" → TEST
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P057-002 — TEST: "assert" fires TEST on statistical calculation boundary

A: "Assert that the StatisticsCalc returns 0.0 standard deviation for a dataset with all identical values."
  "assert" → TEST.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P057-003 — GAP: "compute the mean" fires zero predicates

A: "Compute the mean of the dataset using StatisticsCalc."
  "compute" → domain computation verb; not in any predicate lexicon.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P057-004 — TEST: "validate" fires TEST for StatisticsCalc empty dataset

A: "Validate that the StatisticsCalc throws InvalidOperationException when passed an empty dataset."
  "validate" → TEST universally.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P057-005 — GAP: "determine the median value" fires zero predicates

A: "Determine the median value from the sales data array using StatisticsCalc."
  "determine" → domain computation verb; zero predicates.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
