# Layer 0-B — Router Pairs (044) — QuickSort: routing decisions for TEST and zero-predicate cases
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R044-001 — TEST+IMPLEMENT: "implement QuickSort and spec pivot behaviour" routes to 5.1 + 4.1

Request: "Implement the QuickSort class and spec the pivot selection behaviour for worst-case input."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  RoutingDecisions: [TEST → 5.1, IMPLEMENT → 4.1] | TransitionType: feature_addition

---

## R044-002 — TEST alone: "assert" on QuickSort partition routes to 5.1, documentation

Request: "Assert that the QuickSort partition step places all elements smaller than the pivot to the left."
  CompiledPredicateIds: ["TEST"]
  RoutingDecisions: [TEST → 5.1] | TransitionType: documentation | SourceStatus: routable

---

## R044-003 — TEST+IMPLEMENT: "create a test suite" routes to 5.1 + 4.1

Request: "Create a test suite covering QuickSort on arrays of size 0, 1, 2, and 1000."
  "create" → IMPLEMENT | "test suite" → TEST
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  RoutingDecisions: [TEST → 5.1, IMPLEMENT → 4.1] | TransitionType: feature_addition

---

## R044-004 — Zero predicate: "partition the array" routes to nowhere

Request: "Partition the array around the chosen pivot element using QuickSort logic."
  "partition" → domain operation verb; zero predicates.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge
