# Layer 0-B — Router Pairs (040) — TreeTraversal: routing decisions for TEST and zero-predicate cases
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R040-001 — TEST+IMPLEMENT: "implement TreeTraversal and add test suite" routes to 5.1 + 4.1

Request: "Implement the TreeTraversal class and add a test suite covering in-order, pre-order, and post-order traversal."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  RoutingDecisions: [TEST → 5.1, IMPLEMENT → 4.1] | TransitionType: feature_addition

---

## R040-002 — TEST alone: "verify" on TreeTraversal routes to 5.1, documentation

Request: "Verify that the TreeTraversal produces the correct in-order sequence for a balanced binary tree."
  CompiledPredicateIds: ["TEST"]
  RoutingDecisions: [TEST → 5.1] | TransitionType: documentation | SourceStatus: routable

---

## R040-003 — TEST alone: "test harness" on TreeTraversal routes to 5.1, documentation

Request: "Set up a test harness for the TreeTraversal class covering null root and single-node trees."
  "test harness" → TEST (compound). TransitionType: documentation (no construction verb present).
  CompiledPredicateIds: ["TEST"]
  RoutingDecisions: [TEST → 5.1] | TransitionType: documentation | SourceStatus: routable

---

## R040-004 — Zero predicate: "process tree nodes" routes to nowhere

Request: "Process the tree nodes using in-order traversal."
  "process" → domain computation verb; zero predicates.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge
