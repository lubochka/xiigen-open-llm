# Layer 0-D — Flow Factory Pairs (056) — GeoCalculator: factory selection for geo domain
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F056-001 — TEST+IMPLEMENT: "build GeoCalculator and add unit tests" → two instances

Request: "Build the GeoCalculator and add unit tests for Haversine distance between two coordinates."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition.

---

## F056-002 — TEST alone: "assert" on GeoCalculator antipodal points → one UnitTestingFlow51, documentation

Request: "Assert that the GeoCalculator returns approximately 20,000 km for antipodal coordinate pairs."
  FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  TransitionType: documentation.

---

## F056-003 — Zero predicate: "calculate distance" → empty factory

Request: "Calculate the distance between two GPS (Global Positioning System) coordinates."
  FlowFactory.CreateMulti([]) → []
  TransitionType: unknown | SourceStatus: knowledge_edge.
