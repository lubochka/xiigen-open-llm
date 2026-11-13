# Layer 1 — Transition Orchestrator Pairs (044) — GeoCalculator: TransitionType decisions
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T044-001 — TEST alone → documentation for GeoCalculator

Request: "Assert that the GeoCalculator returns approximately 20,000 km for antipodal coordinate pairs."
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation
  Evidence: "assert" → TEST. No construction verb present.

---

## T044-002 — TEST+IMPLEMENT → feature_addition for GeoCalculator

Request: "Build the GeoCalculator and add unit tests for Haversine distance between two coordinates."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition
  Evidence: "build" and "add" → IMPLEMENT construction verbs → feature_addition.

---

## T044-003 — Zero predicates → unknown for GeoCalculator

Request: "Calculate the distance between two GPS (Global Positioning System) coordinates."
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
  Evidence: "calculate" → domain computation verb; GPS (Global Positioning System) → domain abbreviation; both zero predicates.
