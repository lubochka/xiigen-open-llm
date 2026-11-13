# Layer 0-D — Flow Factory Pairs (020)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F020-001 — TEST+IMPLEMENT+SECURITY / 5.1+4.1+5.2 → three flows (PasswordValidatorTests)

A:
  sub_phase: [5.1, 4.1, 5.2]
  predicate: ["TEST", "IMPLEMENT", "SECURITY"]
  compiled_text: "Write security tests for the PasswordValidator to assert that hashed passwords are never logged and validation timing is constant."

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
  SelectionReason: "Three predicates → three independent flows"

---

## F020-002 — TEST alone / 5.1 → single flow (spec password history)

A:
  sub_phase: 5.1
  predicate: ["TEST"]
  compiled_text: "Spec the password history rule: reject any of the user's last 5 passwords."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "UnitTestingFlow51"
      predicate: TEST
      sub_phase: 5.1
  SelectionReason: "TEST alone via 'spec' → single flow"
