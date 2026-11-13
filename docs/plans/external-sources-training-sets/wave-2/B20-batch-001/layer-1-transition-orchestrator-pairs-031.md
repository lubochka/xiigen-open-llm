# Layer 1 — Transition Orchestrator Pairs (031) — MergeSort: TransitionType decisions
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T031-001 — TEST alone via "test fixture" → documentation for MergeSort

Request: "Set up a test fixture with large randomised arrays to benchmark MergeSort performance."
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation
  Evidence: "test fixture" → TEST. "set up" → context verb; does not fire IMPLEMENT.

---

## T031-002 — TEST+IMPLEMENT → feature_addition for MergeSort

Request: "Build the MergeSort class and write unit tests for the merge step with overlapping ranges."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition
  Evidence: "build" and "write" → IMPLEMENT construction verbs → feature_addition.

---

## T031-003 — Zero predicates → unknown for MergeSort

Request: "Merge the sub-arrays produced in the divide phase."
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
  Evidence: "merge" → domain operation verb; zero predicates.
