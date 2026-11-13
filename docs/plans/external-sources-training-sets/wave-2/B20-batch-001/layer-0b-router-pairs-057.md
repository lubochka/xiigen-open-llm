# Layer 0-B — Router Pairs (057) — StatisticsCalc: routing decisions for TEST and zero-predicate cases
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R057-001 — TEST+IMPLEMENT: "create StatisticsCalc and write tests" routes to 5.1 + 4.1

Request: "Create the StatisticsCalc class and write tests for mean, median, and standard deviation."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  RoutingDecisions: [TEST → 5.1, IMPLEMENT → 4.1] | TransitionType: feature_addition

---

## R057-002 — TEST alone: "assert" on zero standard deviation routes to 5.1, documentation

Request: "Assert that the StatisticsCalc returns 0.0 standard deviation for a dataset with all identical values."
  CompiledPredicateIds: ["TEST"]
  RoutingDecisions: [TEST → 5.1] | TransitionType: documentation | SourceStatus: routable

---

## R057-003 — Zero predicate: "compute the mean" routes to nowhere

Request: "Compute the mean of the dataset using StatisticsCalc."
  "compute" → domain computation verb; zero predicates.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## R057-004 — Zero predicate: "determine the median" routes to nowhere

Request: "Determine the median value from the sales data array using StatisticsCalc."
  "determine" → domain computation verb; zero predicates.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge
