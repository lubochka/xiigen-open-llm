# Layer 0-B — Router Pairs (051) — DateTimeCalc: routing decisions for TEST and zero-predicate cases
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R051-001 — TEST+IMPLEMENT: "implement DateTimeCalc and write tests" routes to 5.1 + 4.1

Request: "Implement the DateTimeCalc class and write tests for leap year boundary dates."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  RoutingDecisions: [TEST → 5.1, IMPLEMENT → 4.1] | TransitionType: feature_addition

---

## R051-002 — TEST alone: "assert" on date difference routes to 5.1, documentation

Request: "Assert that the DateTimeCalc returns 365 days between January 1 and December 31 in a non-leap year."
  CompiledPredicateIds: ["TEST"]
  RoutingDecisions: [TEST → 5.1] | TransitionType: documentation | SourceStatus: routable

---

## R051-003 — Zero predicate: "calculate the date difference" routes to nowhere

Request: "Calculate the date difference between the start date and end date using DateTimeCalc."
  "calculate" → domain computation verb; zero predicates.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## R051-004 — Zero predicate: "determine elapsed time" routes to nowhere

Request: "Determine the elapsed time between two events using the DateTimeCalc."
  "determine" → domain computation verb; zero predicates.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge
