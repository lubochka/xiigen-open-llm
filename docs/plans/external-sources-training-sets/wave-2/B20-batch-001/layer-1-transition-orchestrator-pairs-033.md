# Layer 1 — Transition Orchestrator Pairs (033) — HeapSort: TransitionType decisions
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T033-001 — TEST alone → documentation for HeapSort

Request: "Validate that HeapSort correctly handles arrays containing duplicate values."
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation
  Evidence: "validate" → TEST. No construction verb present.

---

## T033-002 — TEST+IMPLEMENT → feature_addition for HeapSort

Request: "Generate the HeapSort class and write tests for the heapify step."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition
  Evidence: "generate" and "write" → IMPLEMENT construction verbs → feature_addition.

---

## T033-003 — Zero predicates → unknown for HeapSort

Request: "Trigger a heap rebuild after inserting a new element."
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
  Evidence: "trigger" → execution verb; zero predicates.
