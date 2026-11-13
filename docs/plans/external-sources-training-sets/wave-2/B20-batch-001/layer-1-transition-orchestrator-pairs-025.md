# Layer 1 — Transition Orchestrator Pairs (025) — SortingAlgorithm: TransitionType decisions
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T025-001 — TEST alone → documentation for SortingAlgorithm

Request: "Assert that the SortingAlgorithm returns elements in ascending order."
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation
  Evidence: "assert" → TEST. No construction verb present.

---

## T025-002 — TEST+IMPLEMENT → feature_addition for SortingAlgorithm

Request: "Write unit tests for the SortingAlgorithm and implement the insertion sort variant."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition
  Evidence: "write" and "implement" are construction verbs → feature_addition.

---

## T025-003 — Zero predicates → unknown for SortingAlgorithm

Request: "Run the SortingAlgorithm on the sample dataset."
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
  Evidence: "run" → execution verb; zero predicates → no TransitionType determined.
