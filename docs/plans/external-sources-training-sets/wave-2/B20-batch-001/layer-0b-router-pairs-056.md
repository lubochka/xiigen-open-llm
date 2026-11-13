# Layer 0-B — Router Pairs (056) — GeoCalculator: routing decisions for TEST and zero-predicate cases
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R056-001 — TEST+IMPLEMENT: "build GeoCalculator and add unit tests" routes to 5.1 + 4.1

Request: "Build the GeoCalculator and add unit tests for Haversine distance between two coordinates."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  RoutingDecisions: [TEST → 5.1, IMPLEMENT → 4.1] | TransitionType: feature_addition

---

## R056-002 — TEST alone: "verify" on GeoCalculator distance routes to 5.1, documentation

Request: "Verify that the GeoCalculator returns a distance within 0.1 km for the London to Paris route."
  CompiledPredicateIds: ["TEST"]
  RoutingDecisions: [TEST → 5.1] | TransitionType: documentation | SourceStatus: routable

---

## R056-003 — Zero predicate: "calculate distance between coordinates" routes to nowhere

Request: "Calculate the distance between two GPS (Global Positioning System) coordinates."
  "calculate" → domain computation verb; GPS (Global Positioning System) → domain abbreviation; neither fires a predicate.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## R056-004 — Zero predicate: "compute bearing angle" routes to nowhere

Request: "Compute the bearing angle from the origin to the destination using the GeoCalculator."
  "compute" → domain computation verb; zero predicates.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge
