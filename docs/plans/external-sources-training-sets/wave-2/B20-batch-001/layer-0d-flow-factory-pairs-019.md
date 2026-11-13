# Layer 0-D — Flow Factory Pairs (019)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F019-001 — TEST+IMPLEMENT / 5.1+4.1 → two flows (FraudScoreTests with test doubles)

A:
  sub_phase: [5.1, 4.1]
  predicate: ["TEST", "IMPLEMENT"]
  compiled_text: "Write unit tests for the FraudScoreCalculator and implement the test doubles for the external risk-scoring API."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "UnitTestingFlow51"
      predicate: TEST
      sub_phase: 5.1
    - FlowId: "BackendServiceFlow41"
      predicate: IMPLEMENT
      sub_phase: 4.1
  SelectionReason: "TEST (via 'unit tests' + 'test doubles') + IMPLEMENT (via 'write' + 'implement') → two flows"

---

## F019-002 — ZERO predicates / null → NoFlowCreated (run regression tests)

A:
  sub_phase: null
  predicate: []
  compiled_text: "Run regression tests for the FraudScoreCalculator nightly."

B:
  TrustState: null
  FlowInstances: []
  SelectionReason: "Zero predicates from 'run' → no flow created; knowledge_edge"
  Note: "nightly" scheduling context does not add any predicate. Zero predicates = zero flows.

---

## F019-003 — TEST+IMPLEMENT / 5.1+4.1 → two flows (develop tests with mock)

A:
  sub_phase: [5.1, 4.1]
  predicate: ["TEST", "IMPLEMENT"]
  compiled_text: "Develop unit tests for FraudScoreCalculator with a mock device-fingerprint service."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "UnitTestingFlow51"
      predicate: TEST
      sub_phase: 5.1
    - FlowId: "BackendServiceFlow41"
      predicate: IMPLEMENT
      sub_phase: 4.1
