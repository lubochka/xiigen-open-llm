# Layer 1 — Transition Orchestrator Pairs (028) — TreeTraversal: TransitionType with "test harness" boundary
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T028-001 — TEST alone via "test harness" → documentation for TreeTraversal

Request: "Set up a test harness for the TreeTraversal class covering null root and single-node trees."
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation
  Evidence: "test harness" → TEST (compound). "set up" → context verb; does not fire IMPLEMENT.
  No construction verb → documentation.

---

## T028-002 — TEST+IMPLEMENT → feature_addition for TreeTraversal

Request: "Implement the TreeTraversal class and add a test suite covering all three traversal orders."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition
  Evidence: "implement" and "add" → IMPLEMENT construction verbs → feature_addition.

---

## T028-003 — Zero predicates → unknown for TreeTraversal

Request: "Process the tree nodes using in-order traversal."
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
  Evidence: "process" → domain computation verb; zero predicates.
