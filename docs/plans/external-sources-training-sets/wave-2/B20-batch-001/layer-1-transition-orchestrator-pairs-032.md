# Layer 1 — Transition Orchestrator Pairs (032) — QuickSort: TransitionType decisions
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T032-001 — TEST+IMPLEMENT → feature_addition for QuickSort (spec + implement)

Request: "Implement the QuickSort class and spec the pivot selection behaviour for worst-case input."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition
  Evidence: "implement" → IMPLEMENT construction verb → feature_addition. "spec" → TEST.

---

## T032-002 — TEST+IMPLEMENT → feature_addition via "create test suite" for QuickSort

Request: "Create a test suite covering QuickSort on arrays of size 0, 1, 2, and 1000."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition
  Evidence: "create" → IMPLEMENT construction verb. "test suite" → TEST.

---

## T032-003 — Zero predicates → unknown for QuickSort

Request: "Partition the array around the chosen pivot element."
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
  Evidence: "partition" → domain operation verb; zero predicates.
