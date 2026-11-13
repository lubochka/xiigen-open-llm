# Layer 0-B — Router Pairs (049) — StringParser: routing decisions for TEST and zero-predicate cases
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R049-001 — TEST+IMPLEMENT: "create StringParser and write unit tests" routes to 5.1 + 4.1

Request: "Create the StringParser class and write unit tests for delimiter-based splitting."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  RoutingDecisions: [TEST → 5.1, IMPLEMENT → 4.1] | TransitionType: feature_addition

---

## R049-002 — TEST alone: "assert" on StringParser null-input routes to 5.1, documentation

Request: "Assert that the StringParser throws ArgumentNullException when input is null."
  CompiledPredicateIds: ["TEST"]
  RoutingDecisions: [TEST → 5.1] | TransitionType: documentation | SourceStatus: routable

---

## R049-003 — Zero predicate: "parse the input string" routes to nowhere

Request: "Parse the input string using the configured delimiter in the StringParser."
  "parse" → domain operation verb; zero predicates.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## R049-004 — Zero predicate: "split the token stream" routes to nowhere

Request: "Split the token stream at semicolon delimiters using the StringParser."
  "split" → domain operation verb; zero predicates.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge
