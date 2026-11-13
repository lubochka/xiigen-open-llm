# Layer 0-B — Router Pairs (059) — RandomGenerator: routing decisions for TEST and boundary-case "generate"
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R059-001 — TEST+IMPLEMENT: "add RandomGenerator and write unit tests" routes to 5.1 + 4.1

Request: "Add the RandomGenerator class and write unit tests for uniform distribution and seeded reproducibility."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  RoutingDecisions: [TEST → 5.1, IMPLEMENT → 4.1] | TransitionType: feature_addition

---

## R059-002 — TEST alone: "assert" on range enforcement routes to 5.1, documentation

Request: "Assert that the RandomGenerator never produces a value outside the specified min-max range."
  CompiledPredicateIds: ["TEST"]
  RoutingDecisions: [TEST → 5.1] | TransitionType: documentation | SourceStatus: routable

---

## R059-003 — IMPLEMENT alone: "generate a random number" routes to 4.1, feature_addition

Request: "Generate a random number between 1 and 100 using the RandomGenerator."
  "generate" → IMPLEMENT (boundary case: "generate" is in the IMPLEMENT lexicon).
  CompiledPredicateIds: ["IMPLEMENT"]
  RoutingDecisions: [IMPLEMENT → 4.1] | TransitionType: feature_addition | SourceStatus: routable

---

## R059-004 — Zero predicate: "produce random samples" routes to nowhere

Request: "Produce random samples from the distribution using the configured RandomGenerator."
  "produce" → domain operation verb; zero predicates.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge
