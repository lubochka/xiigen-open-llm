# Layer 0-A — Compiler Pairs (059) — RandomGenerator: TEST fires on test vocabulary, not random-domain terms
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P059-001 — TEST+IMPLEMENT: "add RandomGenerator and write unit tests"

A: "Add the RandomGenerator class and write unit tests for uniform distribution and seeded reproducibility."
  "add" → IMPLEMENT | "write" → IMPLEMENT (deduplicates) | "unit tests" → TEST
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P059-002 — TEST: "assert" fires TEST for RandomGenerator range enforcement

A: "Assert that the RandomGenerator never produces a value outside the specified min-max range."
  "assert" → TEST.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P059-003 — GAP: "generate a random number" fires zero predicates

A: "Generate a random number between 1 and 100 using the RandomGenerator."
  "generate" — this is a boundary case: "generate" IS in the IMPLEMENT lexicon.
  Correct: "generate" → IMPLEMENT. No test-infrastructure vocabulary present.
  CompiledPredicateIds: ["IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 4.1

---

## P059-004 — TEST: "validate" fires TEST for RandomGenerator seed reproducibility

A: "Validate that the RandomGenerator produces the same sequence when initialised with the same seed."
  "validate" → TEST universally.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P059-005 — GAP: "produce random samples" fires zero predicates

A: "Produce random samples from the distribution using the configured RandomGenerator."
  "produce" → domain operation verb; not in any predicate lexicon.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
