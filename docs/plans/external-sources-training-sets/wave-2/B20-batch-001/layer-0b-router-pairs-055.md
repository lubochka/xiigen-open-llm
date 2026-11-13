# Layer 0-B — Router Pairs (055) — UnitConverter: routing decisions for TEST and zero-predicate cases
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R055-001 — TEST+IMPLEMENT: "implement UnitConverter and write test harness" routes to 5.1 + 4.1

Request: "Implement the UnitConverter and write a test harness for metric to imperial length conversions."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  RoutingDecisions: [TEST → 5.1, IMPLEMENT → 4.1] | TransitionType: feature_addition

---

## R055-002 — TEST alone: "assert" on conversion factor routes to 5.1, documentation

Request: "Assert that the UnitConverter returns 2.54 centimetres per inch."
  CompiledPredicateIds: ["TEST"]
  RoutingDecisions: [TEST → 5.1] | TransitionType: documentation | SourceStatus: routable

---

## R055-003 — Zero predicate: "convert metres to feet" routes to nowhere

Request: "Convert the measurement from metres to feet using the UnitConverter."
  "convert" → domain operation verb; zero predicates.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## R055-004 — Zero predicate: "apply conversion factor" routes to nowhere

Request: "Apply the conversion factor from the UnitConverter lookup table to the input measurement."
  "apply" (standalone) → domain operation verb; not in IMPLEMENT lexicon.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge
