# Layer 1 — Transition Orchestrator Pairs (029) — DijkstraPath: TransitionType decisions
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T029-001 — TEST alone → documentation for DijkstraPath

Request: "Validate that the DijkstraPath handles negative-weight edges by raising an error."
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation
  Evidence: "validate" → TEST. No construction verb present.

---

## T029-002 — TEST+IMPLEMENT → feature_addition for DijkstraPath

Request: "Develop the DijkstraPath class and write tests for graphs with disconnected components."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition
  Evidence: "develop" → IMPLEMENT construction verb → feature_addition.

---

## T029-003 — Zero predicates → unknown for DijkstraPath

Request: "Compute the shortest path between node A and node B."
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
  Evidence: "compute" → domain computation verb; zero predicates.
