# Layer 0-D — Flow Factory Pairs (016)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F016-001 — TEST+IMPLEMENT / 5.1+4.1 → two flows (TaxCalculationTests)

A:
  sub_phase: [5.1, 4.1]
  predicate: ["TEST", "IMPLEMENT"]
  compiled_text: "Write unit tests for the TaxCalculationService to cover VAT rates for EU countries and US state tax."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "UnitTestingFlow51"
      predicate: TEST
      sub_phase: 5.1
    - FlowId: "BackendServiceFlow41"
      predicate: IMPLEMENT
      sub_phase: 4.1

---

## F016-002 — TEST+IMPLEMENT / 5.1+4.1 → two flows (test harness with stubs)

A:
  sub_phase: [5.1, 4.1]
  predicate: ["TEST", "IMPLEMENT"]
  compiled_text: "Create a test harness for the TaxCalculationService that stubs the tax-rate repository."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "UnitTestingFlow51"
      predicate: TEST
      sub_phase: 5.1
      SelectionReason: "'test harness' + 'stubs' → TEST → UnitTestingFlow51"
    - FlowId: "BackendServiceFlow41"
      predicate: IMPLEMENT
      sub_phase: 4.1
      SelectionReason: "'create' → IMPLEMENT → BackendServiceFlow41"
  Note: "test harness" and "stubs" are TEST-infrastructure vocabulary — they select UnitTestingFlow51 alongside the IMPLEMENT flow.
