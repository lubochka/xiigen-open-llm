# Layer 0-B — Router Pairs (016)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: router

---

## R016-001 — TEST+IMPLEMENT → 5.1 + 4.1 (TaxCalculationTests)

A:
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  compiled_text: "Write unit tests for the TaxCalculationService to cover VAT rates for EU countries, US state tax, and zero-rated goods."

B:
  RoutingDecisions:
    - predicate: TEST
      sub_phase: 5.1
      sub_phase_label: "Unit and Integration Testing"
      confidence: 0.92
      routing_note: "'unit tests' → TEST → 5.1"
    - predicate: IMPLEMENT
      sub_phase: 4.1
      sub_phase_label: "Service and Logic Implementation"
      confidence: 0.92
      routing_note: "'write' → IMPLEMENT → 4.1"
  TransitionType: feature_addition
  PrimarySubPhaseId: 5.1
  AllSubPhaseIds: [5.1, 4.1]

---

## R016-002 — TEST+IMPLEMENT → 5.1 + 4.1 (test harness with stubs)

A:
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  compiled_text: "Create a test harness for the TaxCalculationService that stubs the tax-rate repository and validates output per region."

B:
  RoutingDecisions:
    - predicate: TEST
      sub_phase: 5.1
      sub_phase_label: "Unit and Integration Testing"
      confidence: 0.92
      routing_note: "'test harness' + 'stubs' + 'validates' → TEST → 5.1"
    - predicate: IMPLEMENT
      sub_phase: 4.1
      sub_phase_label: "Service and Logic Implementation"
      confidence: 0.92
      routing_note: "'create' → IMPLEMENT → 4.1"
  TransitionType: feature_addition
  PrimarySubPhaseId: 5.1
  AllSubPhaseIds: [5.1, 4.1]
  Note: "stubs" fires TEST (stub is test-infrastructure vocabulary). "create" fires IMPLEMENT independently.
