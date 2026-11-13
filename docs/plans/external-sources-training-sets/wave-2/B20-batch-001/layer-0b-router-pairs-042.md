# Layer 0-B — Router Pairs (042) — BinarySearch: routing decisions for TEST and zero-predicate cases
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R042-001 — TEST+IMPLEMENT: "create BinarySearch and add test coverage" routes to 5.1 + 4.1

Request: "Create the BinarySearch class and add test coverage for sorted and unsorted input arrays."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  RoutingDecisions: [TEST → 5.1, IMPLEMENT → 4.1] | TransitionType: feature_addition

---

## R042-002 — TEST alone: "spec" on BinarySearch routes to 5.1, documentation

Request: "Spec out the BinarySearch behaviour when the target appears at the first and last positions."
  CompiledPredicateIds: ["TEST"]
  RoutingDecisions: [TEST → 5.1] | TransitionType: documentation | SourceStatus: routable

---

## R042-003 — TEST alone: three TEST verbs deduplicate to one route → 5.1

Request: "Assert, verify, and validate that the BinarySearch returns mid-index for a single-element array."
  Three TEST-firing verbs → one ["TEST"] predicate → one routing decision.
  CompiledPredicateIds: ["TEST"]
  RoutingDecisions: [TEST → 5.1] | TransitionType: documentation | SourceStatus: routable

---

## R042-004 — Zero predicate: "look up element" routes to nowhere

Request: "Look up the target element in the sorted array using binary search."
  "look up" → domain operation phrase; zero predicates.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge
