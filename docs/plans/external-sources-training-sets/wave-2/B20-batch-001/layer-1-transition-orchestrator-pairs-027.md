# Layer 1 — Transition Orchestrator Pairs (027) — GraphTraversal: TransitionType decisions
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T027-001 — TEST+IMPLEMENT → feature_addition for GraphTraversal

Request: "Create the GraphTraversal class and add test cases covering BFS (Breadth-First Search) and DFS (Depth-First Search)."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition
  Evidence: "create" and "add" → IMPLEMENT construction verbs → feature_addition.

---

## T027-002 — TEST alone → documentation for GraphTraversal

Request: "Assert that the GraphTraversal visits all reachable nodes from the root."
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation
  Evidence: "assert" → TEST. No construction verb present.

---

## T027-003 — Zero predicates → unknown for GraphTraversal

Request: "Apply the traversal algorithm to the dependency tree."
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
  Evidence: "apply" (standalone) → NOT in IMPLEMENT lexicon; zero predicates.
