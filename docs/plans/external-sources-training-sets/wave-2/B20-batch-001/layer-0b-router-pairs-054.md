# Layer 0-B — Router Pairs (054) — NumberFormatter: routing decisions for TEST and zero-predicate cases
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R054-001 — TEST+IMPLEMENT: "develop NumberFormatter and add test cases" routes to 5.1 + 4.1

Request: "Develop the NumberFormatter and add test cases for thousands separators and decimal precision."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  RoutingDecisions: [TEST → 5.1, IMPLEMENT → 4.1] | TransitionType: feature_addition

---

## R054-002 — TEST alone: "verify" on NumberFormatter routes to 5.1, documentation

Request: "Verify that the NumberFormatter inserts a comma every three digits for numbers above 999."
  CompiledPredicateIds: ["TEST"]
  RoutingDecisions: [TEST → 5.1] | TransitionType: documentation | SourceStatus: routable

---

## R054-003 — Zero predicate: "round the number" routes to nowhere

Request: "Round the number to two decimal places using the NumberFormatter precision setting."
  "round" → domain operation verb; zero predicates.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## R054-004 — Zero predicate: "process numeric output" routes to nowhere

Request: "Process the numeric output through the NumberFormatter before sending to the UI layer."
  "process" → domain computation verb; zero predicates.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge
