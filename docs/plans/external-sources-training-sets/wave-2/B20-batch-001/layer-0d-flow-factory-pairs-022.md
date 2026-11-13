# Layer 0-D — Flow Factory Pairs (022)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F022-001 — TEST+IMPLEMENT / 5.1+4.1 → two flows (PriceFormatterTests)

A:
  sub_phase: [5.1, 4.1]
  predicate: ["TEST", "IMPLEMENT"]
  compiled_text: "Write unit tests for the PriceFormatter verifying currency symbols, decimal places, and locale-specific separators."

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

## F022-002 — TEST alone / 5.1 → single flow (validate locale)

A:
  sub_phase: 5.1
  predicate: ["TEST"]
  compiled_text: "Validate that the PriceFormatter wraps negative prices in parentheses for accountancy display mode."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "UnitTestingFlow51"
      predicate: TEST
      sub_phase: 5.1
  SelectionReason: "TEST alone via 'validate' (universal rule) → single UnitTestingFlow51"

---

## F022-003 — ZERO predicates / null → NoFlowCreated (run unit tests)

A:
  sub_phase: null
  predicate: []
  compiled_text: "Run unit tests for the PriceFormatter to check the German locale is now fixed."

B:
  TrustState: null
  FlowInstances: []
  SelectionReason: "Zero predicates from 'run' + 'check' (both execution/informal verbs) → no flow"
