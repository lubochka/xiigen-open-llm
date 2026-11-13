# Layer 0-A — Compiler Pairs (056) — GeoCalculator: TEST fires on test vocabulary, not geo-domain terms
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P056-001 — TEST+IMPLEMENT: "build GeoCalculator and add unit tests"

A: "Build the GeoCalculator and add unit tests for Haversine distance between two coordinates."
  "build" → IMPLEMENT | "add" → IMPLEMENT (deduplicates) | "unit tests" → TEST
  "Haversine" — domain algorithm name; does not fire any predicate.
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P056-002 — TEST: "verify" fires TEST on GeoCalculator distance accuracy

A: "Verify that the GeoCalculator returns a distance within 0.1 km for the London to Paris route."
  "verify" → TEST.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P056-003 — GAP: "calculate distance between coordinates" fires zero predicates

A: "Calculate the distance between two GPS (Global Positioning System) coordinates using the GeoCalculator."
  "calculate" → domain computation verb; GPS (Global Positioning System) is a domain abbreviation; neither fires a predicate.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P056-004 — TEST: "assert" fires TEST for antipodal points

A: "Assert that the GeoCalculator returns approximately 20,000 km for antipodal coordinate pairs."
  "assert" → TEST.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P056-005 — GAP: "compute bearing angle" fires zero predicates

A: "Compute the bearing angle from the origin coordinate to the destination using the GeoCalculator."
  "compute" → domain computation verb; zero predicates.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
