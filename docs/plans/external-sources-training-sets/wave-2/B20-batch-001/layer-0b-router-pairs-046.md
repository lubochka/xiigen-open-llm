# Layer 0-B — Router Pairs (046) — HashMapImpl: routing decisions for TEST and zero-predicate cases
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R046-001 — TEST+IMPLEMENT: "add HashMapImpl and write unit tests" routes to 5.1 + 4.1

Request: "Add the HashMapImpl class and write unit tests for collision resolution using chaining."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  RoutingDecisions: [TEST → 5.1, IMPLEMENT → 4.1] | TransitionType: feature_addition

---

## R046-002 — TEST alone: "assert" on hash collision handling routes to 5.1, documentation

Request: "Assert that the HashMapImpl handles key collisions without data loss."
  CompiledPredicateIds: ["TEST"]
  RoutingDecisions: [TEST → 5.1] | TransitionType: documentation | SourceStatus: routable

---

## R046-003 — TEST alone: "test double" routes to 5.1, documentation

Request: "Use a test double to simulate hash collision scenarios in the HashMapImpl unit tests."
  "test double" → TEST (compound). TransitionType: documentation (no construction verb).
  CompiledPredicateIds: ["TEST"]
  RoutingDecisions: [TEST → 5.1] | TransitionType: documentation | SourceStatus: routable

---

## R046-004 — Zero predicate: "compute the hash" routes to nowhere

Request: "Compute the hash of the key to locate the correct bucket in the HashMap."
  "compute" → domain computation verb; zero predicates.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge
