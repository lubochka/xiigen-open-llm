# Layer 1 — Transition Orchestrator Pairs (056) — "stub" and "mock" test-adjacent verbs: TransitionType
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T056-001 — "mock" fires TEST (test-adjacent); alone → documentation

Request: "Mock the cost provider and validate the MarkupCalculator produces correct retail price."
  "mock" → TEST (test-adjacent). "validate" → TEST (deduplicates). No construction verb.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation

---

## T056-002 — "stub" fires TEST (test-adjacent); with "build" → feature_addition

Request: "Build the MarkupCalculator class and stub the supplier price service for testing."
  "build" → IMPLEMENT. "stub" → TEST (test-adjacent).
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition

---

## T056-003 — "mock" and "stub" do NOT fire IMPLEMENT: only TEST-adjacent

Request: "Mock the external API and stub the database to isolate the HashMapImpl unit tests."
  "mock" → TEST (test-adjacent). "stub" → TEST (deduplicates).
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation
  Teaching: "mock" and "stub" are TEST-adjacent — they fire TEST but NOT IMPLEMENT.
  They cannot independently produce feature_addition TransitionType without a separate construction verb.
