# Layer 0-B — Router Pairs (047) — BloomFilter: routing decisions for TEST and zero-predicate cases
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R047-001 — TEST+IMPLEMENT: "develop BloomFilter and add test cases" routes to 5.1 + 4.1

Request: "Develop the BloomFilter class and add test cases for false-positive rate verification."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  RoutingDecisions: [TEST → 5.1, IMPLEMENT → 4.1] | TransitionType: feature_addition

---

## R047-002 — TEST alone: "validate" on BloomFilter membership routes to 5.1, documentation

Request: "Validate that the BloomFilter correctly identifies an element as probably-present after insertion."
  CompiledPredicateIds: ["TEST"]
  RoutingDecisions: [TEST → 5.1] | TransitionType: documentation | SourceStatus: routable

---

## R047-003 — Zero predicate: "calculate false-positive rate" routes to nowhere

Request: "Calculate the expected false-positive rate for a BloomFilter with 1000 elements."
  "calculate" → domain computation verb; zero predicates.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## R047-004 — Zero predicate: "apply bit array updates" routes to nowhere

Request: "Apply the bit array updates from the hash functions during element insertion into the BloomFilter."
  "apply" (standalone) → domain operation verb; not in IMPLEMENT lexicon.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge
