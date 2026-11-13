# Layer 0-B — Router Pairs (043) — MergeSort: routing decisions for TEST and zero-predicate cases
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R043-001 — TEST+IMPLEMENT: "build MergeSort and write unit tests" routes to 5.1 + 4.1

Request: "Build the MergeSort class and write unit tests for the merge step with overlapping ranges."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  RoutingDecisions: [TEST → 5.1, IMPLEMENT → 4.1] | TransitionType: feature_addition

---

## R043-002 — TEST alone: "assert" on MergeSort stability routes to 5.1, documentation

Request: "Assert that the MergeSort preserves the relative order of equal elements."
  CompiledPredicateIds: ["TEST"]
  RoutingDecisions: [TEST → 5.1] | TransitionType: documentation | SourceStatus: routable

---

## R043-003 — Zero predicate: "merge the sub-arrays" routes to nowhere

Request: "Merge the sub-arrays produced in the divide phase of the MergeSort."
  "merge" → domain operation verb; zero predicates.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## R043-004 — Zero predicate: "start the sort" routes to nowhere

Request: "Start the MergeSort on the input collection before the batch job completes."
  "start" → execution verb; zero predicates.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge
