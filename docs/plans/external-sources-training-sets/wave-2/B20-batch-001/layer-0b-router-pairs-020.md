# Layer 0-B — Router Pairs (020)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: router

---

## R020-001 — TEST+IMPLEMENT+SECURITY → 5.1 + 4.1 + 5.2 (PasswordValidatorTests)

A:
  compiled_predicate_ids: ["TEST", "IMPLEMENT", "SECURITY"]
  compiled_text: "Write security tests for the PasswordValidator to assert that hashed passwords are never logged and that validation timing is constant."

B:
  RoutingDecisions:
    - predicate: TEST
      sub_phase: 5.1
      sub_phase_label: "Unit and Integration Testing"
      confidence: 0.92
      routing_note: "'security tests' (via 'tests') + 'assert' → TEST → 5.1"
    - predicate: IMPLEMENT
      sub_phase: 4.1
      sub_phase_label: "Service and Logic Implementation"
      confidence: 0.92
      routing_note: "'write' → IMPLEMENT → 4.1"
    - predicate: SECURITY
      sub_phase: 5.2
      sub_phase_label: "Security and Compliance Testing"
      confidence: 0.92
      routing_note: "'security tests' (via 'security') + 'hashed passwords' → SECURITY → 5.2"
  TransitionType: feature_addition
  PrimarySubPhaseId: 5.1
  AllSubPhaseIds: [5.1, 4.1, 5.2]
  Note: "security tests" fires both TEST (via "tests") and SECURITY (via "security") simultaneously.
  In a testing context, SECURITY routes to 5.2, not to Phase 1-3 design sub-phases.

---

## R020-002 — TEST alone → 5.1 (spec password history)

A:
  compiled_predicate_ids: ["TEST"]
  compiled_text: "Spec the password history rule: the PasswordValidator must reject any of the user's last 5 passwords."

B:
  RoutingDecisions:
    - predicate: TEST
      sub_phase: 5.1
      sub_phase_label: "Unit and Integration Testing"
      confidence: 0.92
      routing_note: "'spec' → TEST → 5.1"
  TransitionType: documentation
  SubPhaseId: 5.1
