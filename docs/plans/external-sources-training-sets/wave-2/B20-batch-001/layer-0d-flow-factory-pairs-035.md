# Layer 0-D — Flow Factory Pairs (035) — MarkupCalculator: factory with "mock" and "stub" test-adjacent
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F035-001 — TEST alone: "mock" fires TEST → one UnitTestingFlow51, documentation

Request: "Mock the cost provider and validate the MarkupCalculator produces correct retail price."
  "mock" → TEST (test-adjacent). "validate" → TEST (deduplicates). "provide" → does not fire IMPLEMENT.
  FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  TransitionType: documentation.

---

## F035-002 — TEST+IMPLEMENT: "build and stub MarkupCalculator" → two instances

Request: "Build the MarkupCalculator class and stub the supplier price service for testing."
  "build" → IMPLEMENT | "stub" → TEST (test-adjacent)
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition.

---

## F035-003 — Zero predicate: "calculate markup percentage" → empty factory

Request: "Calculate the markup percentage applied to each product in the catalogue."
  "calculate" → domain computation verb; zero predicates.
  FlowFactory.CreateMulti([]) → []
  TransitionType: unknown | SourceStatus: knowledge_edge.
