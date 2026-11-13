# Layer 0-C — Knowledge Edge Pairs (056) — GeoCalculator: gap analysis for geo-domain verbs
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E056-001 — "calculate distance" does NOT fire IMPLEMENT

"Calculate the distance between two GPS (Global Positioning System) coordinates using the GeoCalculator." → CompiledPredicateIds: []
"calculate" → domain computation verb; zero predicates.
GPS (Global Positioning System) → domain abbreviation; zero predicates.

---

## E056-002 — "compute bearing angle" does NOT fire IMPLEMENT

"Compute the bearing angle from the origin coordinate to the destination." → CompiledPredicateIds: []
"compute" → domain computation verb; zero predicates.
Confirmed: bearing, heading, azimuth — all domain geo terms; none fire predicates.

---

## E056-003 — "GPS" and "Haversine" abbreviations do NOT fire any predicate

"Use the Haversine formula for GPS (Global Positioning System) coordinate distance calculation." → CompiledPredicateIds: []
"use" → context verb; zero predicates. "Haversine" → domain algorithm name; zero predicates.
GPS (Global Positioning System) → domain abbreviation; zero predicates.

---

## E056-004 — "antipodal" domain term does NOT fire TEST; TEST fires from "assert"

"Assert that the GeoCalculator returns approximately 20,000 km for antipodal coordinate pairs." → ["TEST"]
  "assert" → TEST fires. "antipodal" → domain geo term; does not independently fire any predicate.
"Calculate the GeoCalculator result for antipodal points." → []
  "calculate" → domain computation verb; zero predicates.
Teaching: the domain geo term (antipodal, equatorial, meridian) does not determine predicate firing.
