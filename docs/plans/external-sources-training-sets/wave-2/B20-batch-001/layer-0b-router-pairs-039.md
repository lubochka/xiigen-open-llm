# Layer 0-B — Router Pairs (039) — GraphTraversal: routing decisions for TEST and zero-predicate cases
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R039-001 — TEST+IMPLEMENT: "write test cases and create GraphTraversal" routes to 5.1 + 4.1

Request: "Write test cases covering BFS (Breadth-First Search) and DFS (Depth-First Search) traversal order and create the GraphTraversal class."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  RoutingDecisions: [TEST → 5.1, IMPLEMENT → 4.1] | TransitionType: feature_addition

---

## R039-002 — TEST alone: "assert" on GraphTraversal routes to 5.1, documentation

Request: "Assert that the GraphTraversal visits all reachable nodes from the root."
  CompiledPredicateIds: ["TEST"]
  RoutingDecisions: [TEST → 5.1] | TransitionType: documentation | SourceStatus: routable

---

## R039-003 — Zero predicate: "traverse the graph" routes to nowhere

Request: "Traverse the graph in breadth-first order."
  "traverse" → domain operation verb; zero predicates.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## R039-004 — Zero predicate: "apply traversal to dependency tree" routes to nowhere

Request: "Apply the traversal algorithm to the dependency tree."
  "apply" (standalone) → domain operation verb; not in IMPLEMENT lexicon.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge
