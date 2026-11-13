# Layer 0-A — Compiler Pairs (051) — DateTimeCalc: TEST fires on test vocabulary, not datetime terms
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P051-001 — TEST+IMPLEMENT: "implement DateTimeCalc and write tests"

A: "Implement the DateTimeCalc class and write tests for leap year boundary dates."
  "implement" → IMPLEMENT | "write" → IMPLEMENT (deduplicates) | "tests" → TEST
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P051-002 — TEST: "assert" fires TEST on date difference calculation

A: "Assert that the DateTimeCalc returns 365 days between January 1 and December 31 in a non-leap year."
  "assert" → TEST.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P051-003 — GAP: "calculate the date difference" fires zero predicates

A: "Calculate the date difference between the start date and end date using DateTimeCalc."
  "calculate" → domain computation verb; not in any predicate lexicon.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P051-004 — TEST: "validate" fires TEST for DateTimeCalc invalid-date handling

A: "Validate that the DateTimeCalc rejects February 30 as an invalid date."
  "validate" → TEST universally.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P051-005 — GAP: "determine the elapsed time" fires zero predicates

A: "Determine the elapsed time between two events using the DateTimeCalc."
  "determine" → domain computation verb; zero predicates.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
