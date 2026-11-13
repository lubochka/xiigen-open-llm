# Layer 0-D — Flow Factory Pairs (059) — RandomGenerator: factory with "generate" IMPLEMENT boundary
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F059-001 — IMPLEMENT alone: "generate a random number" → one BackendServiceFlow41, feature_addition

Request: "Generate a random number between 1 and 100 using the RandomGenerator."
  "generate" → IMPLEMENT. No test-infrastructure vocabulary present.
  FlowFactory.CreateMulti(["IMPLEMENT"]) → [BackendServiceFlow41]
  TransitionType: feature_addition.
  Note: this is the only utility operation verb that produces a non-empty factory result without TEST.

---

## F059-002 — TEST+IMPLEMENT: "add RandomGenerator and write unit tests" → two instances

Request: "Add the RandomGenerator class and write unit tests for uniform distribution and seeded reproducibility."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition.

---

## F059-003 — Zero predicate: "produce random samples" → empty factory

Request: "Produce random samples from the distribution using the configured RandomGenerator."
  "produce" → domain operation verb; NOT in IMPLEMENT lexicon.
  FlowFactory.CreateMulti([]) → []
  TransitionType: unknown | SourceStatus: knowledge_edge.
