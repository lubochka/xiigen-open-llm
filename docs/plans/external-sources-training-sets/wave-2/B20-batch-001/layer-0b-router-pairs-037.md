# Layer 0-B — Router Pairs (037) — SortingAlgorithm: routing decisions for TEST and zero-predicate cases
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R037-001 — TEST alone: "assert" on SortingAlgorithm routes to 5.1, documentation

Request: "Assert that the SortingAlgorithm returns elements in ascending order."
  CompiledPredicateIds: ["TEST"]
  RoutingDecisions: [TEST → 5.1] | TransitionType: documentation | SourceStatus: routable

---

## R037-002 — TEST+IMPLEMENT: "write tests and implement SortingAlgorithm" routes to 5.1 + 4.1

Request: "Write unit tests for the SortingAlgorithm and implement the insertion sort variant."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  RoutingDecisions: [TEST → 5.1, IMPLEMENT → 4.1] | TransitionType: feature_addition

---

## R037-003 — Zero predicate: "run the sort" routes to nowhere

Request: "Run the SortingAlgorithm on the sample dataset."
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## R037-004 — Zero predicate: "execute sort and check output" routes to nowhere

Request: "Execute the SortingAlgorithm and check that the output array is sorted."
  "execute" → execution verb; "check" → informal QA. Both produce zero predicates.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge
