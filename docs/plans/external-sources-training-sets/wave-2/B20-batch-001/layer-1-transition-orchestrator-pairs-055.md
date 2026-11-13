# Layer 1 — Transition Orchestrator Pairs (055) — "regression tests" compound fires TEST: utility domain
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T055-001 — "regression tests" fires TEST: TimeZoneConverter example

Request: "Add regression tests for the TimeZoneConverter after extending to support IANA identifiers."
  "add" → IMPLEMENT. "regression tests" → TEST (compound via "tests").
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition

---

## T055-002 — "regression" alone does NOT fire TEST: boundary pair

A-regression-tests: "Write regression tests for the CurrencyFormatter after the GBP (Great British Pound) locale update." → ["TEST", "IMPLEMENT"] | feature_addition
A-regression-alone: "A regression was found in the CurrencyFormatter for the GBP (Great British Pound) locale." → [] | unknown
Teaching: "regression tests" (compound) → TEST fires. "regression" (noun describing a defect) → zero predicates.
The TEST signal requires the compound "regression tests", not the standalone noun "regression".

---

## T055-003 — "integration tests" compound fires TEST: universal across B20

"Write integration tests for the StatisticsCalc and ProbabilityEngine." → ["TEST", "IMPLEMENT"] | feature_addition
"Run integration tests for the StatisticsCalc." → [] | unknown (execution verb governs)
Teaching: "integration tests" is a TEST-firing compound. "run integration tests" → execution verb gap (same as "run unit tests").
