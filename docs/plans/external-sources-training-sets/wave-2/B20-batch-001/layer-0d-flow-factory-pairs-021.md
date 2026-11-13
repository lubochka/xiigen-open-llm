# Layer 0-D — Flow Factory Pairs (021)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F021-001 — TEST+IMPLEMENT+SECURITY / 5.1+4.1+5.2 → three flows (JwtTokenBuilderTests)

A:
  sub_phase: [5.1, 4.1, 5.2]
  predicate: ["TEST", "IMPLEMENT", "SECURITY"]
  compiled_text: "Write unit tests for the JwtTokenBuilder covering token generation, claims insertion, and expiry enforcement."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "UnitTestingFlow51"
      predicate: TEST
      sub_phase: 5.1
    - FlowId: "BackendServiceFlow41"
      predicate: IMPLEMENT
      sub_phase: 4.1
    - FlowId: "SecurityComplianceFlow52"
      predicate: SECURITY
      sub_phase: 5.2

---

## F021-002 — TEST+SECURITY / 5.1+5.2 → two flows (assert token signature)

A:
  sub_phase: [5.1, 5.2]
  predicate: ["TEST", "SECURITY"]
  compiled_text: "Assert that JwtTokenBuilder produces tokens signed with RS256 and tampered tokens fail signature verification."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "UnitTestingFlow51"
      predicate: TEST
      sub_phase: 5.1
    - FlowId: "SecurityComplianceFlow52"
      predicate: SECURITY
      sub_phase: 5.2
  SelectionReason: "TEST (via 'assert') + SECURITY (via JWT/signed/tampered/signature) → two flows; no IMPLEMENT"
  Note: No IMPLEMENT fires because "assert" is a test verb, not an IMPLEMENT verb. Two flows, not three.
