# Layer 0-B — Router Pairs (045) — HeapSort: routing decisions for TEST and zero-predicate cases
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R045-001 — TEST+IMPLEMENT: "generate HeapSort and write tests" routes to 5.1 + 4.1

Request: "Generate the HeapSort class and write tests for the heapify step with varied input sizes."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  RoutingDecisions: [TEST → 5.1, IMPLEMENT → 4.1] | TransitionType: feature_addition

---

## R045-002 — TEST alone: "verify" on HeapSort max-heap property routes to 5.1, documentation

Request: "Verify that HeapSort maintains the max-heap property after each heapify call."
  CompiledPredicateIds: ["TEST"]
  RoutingDecisions: [TEST → 5.1] | TransitionType: documentation | SourceStatus: routable

---

## R045-003 — Zero predicate: "heapify the array" routes to nowhere

Request: "Heapify the array before extracting the maximum element."
  "heapify" → domain operation verb; zero predicates.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## R045-004 — Zero predicate: "trigger heap rebuild" routes to nowhere

Request: "Trigger a heap rebuild after inserting a new element into the priority queue."
  "trigger" → execution verb; zero predicates.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge
