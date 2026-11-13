# Layer 0-D — Flow Factory Pairs (039) — GraphTraversal: factory selection for graph domain
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F039-001 — TEST+IMPLEMENT: "create and add test cases" for GraphTraversal → two instances

Request: "Create the GraphTraversal class and add test cases covering BFS (Breadth-First Search) and DFS (Depth-First Search)."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition.

---

## F039-002 — TEST alone: "assert" on GraphTraversal → one UnitTestingFlow51, documentation

Request: "Assert that the GraphTraversal visits all reachable nodes from the root."
  FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  TransitionType: documentation.

---

## F039-003 — Zero predicate: "traverse the graph" → empty factory

Request: "Traverse the graph in depth-first order."
  FlowFactory.CreateMulti([]) → []
  TransitionType: unknown | SourceStatus: knowledge_edge.
