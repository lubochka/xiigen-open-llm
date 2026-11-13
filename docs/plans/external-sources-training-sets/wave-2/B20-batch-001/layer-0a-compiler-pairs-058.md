# Layer 0-A — Compiler Pairs (058) — ProbabilityEngine: TEST fires on test vocabulary, not probability-domain terms
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P058-001 — TEST+IMPLEMENT: "develop ProbabilityEngine and add test cases"

A: "Develop the ProbabilityEngine and add test cases for conditional probability and Bayes theorem."
  "develop" → IMPLEMENT | "add" → IMPLEMENT (deduplicates) | "test cases" → TEST
  "Bayes theorem" — domain term; does not fire any predicate.
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P058-002 — TEST: "verify" fires TEST on probability sum constraint

A: "Verify that all probabilities in the ProbabilityEngine distribution sum to exactly 1.0."
  "verify" → TEST.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P058-003 — GAP: "calculate the probability" fires zero predicates

A: "Calculate the probability of rolling a six on a fair die using the ProbabilityEngine."
  "calculate" → domain computation verb; not in any predicate lexicon.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P058-004 — TEST: "assert" fires TEST for ProbabilityEngine out-of-range input

A: "Assert that the ProbabilityEngine throws ArgumentOutOfRangeException for probability values above 1.0."
  "assert" → TEST.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P058-005 — GAP: "evaluate probability distribution" fires zero predicates

A: "Evaluate the probability distribution across all possible outcomes in the ProbabilityEngine."
  "evaluate" → domain computation verb; zero predicates.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
