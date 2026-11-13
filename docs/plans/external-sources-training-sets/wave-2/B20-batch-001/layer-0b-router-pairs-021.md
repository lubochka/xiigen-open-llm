# Layer 0-B — Router Pairs (021)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: router

---

## R021-001 — TEST+IMPLEMENT+SECURITY → 5.1 + 4.1 + 5.2 (JwtTokenBuilderTests)

A:
  compiled_predicate_ids: ["TEST", "IMPLEMENT", "SECURITY"]
  compiled_text: "Write unit tests for the JwtTokenBuilder covering token generation, claims insertion, and expiry enforcement."

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
    - predicate: SECURITY
      sub_phase: 5.2
      sub_phase_label: "Security and Compliance Testing"
      confidence: 0.92
      routing_note: "'JWT' + 'token' + 'claims' → SECURITY → 5.2 (testing context)"
  TransitionType: feature_addition
  PrimarySubPhaseId: 5.1
  AllSubPhaseIds: [5.1, 4.1, 5.2]

---

## R021-002 — TEST+SECURITY → 5.1 + 5.2 (assert token signature)

A:
  compiled_predicate_ids: ["TEST", "SECURITY"]
  compiled_text: "Assert that the JwtTokenBuilder produces tokens signed with the RS256 algorithm and that tampered tokens fail signature verification."

B:
  RoutingDecisions:
    - predicate: TEST
      sub_phase: 5.1
      sub_phase_label: "Unit and Integration Testing"
      confidence: 0.92
      routing_note: "'assert' → TEST → 5.1"
    - predicate: SECURITY
      sub_phase: 5.2
      sub_phase_label: "Security and Compliance Testing"
      confidence: 0.92
      routing_note: "'JWT' + 'signed' + 'tampered' + 'signature verification' → SECURITY → 5.2"
  TransitionType: feature_addition
  PrimarySubPhaseId: 5.1
  AllSubPhaseIds: [5.1, 5.2]
  Note: No IMPLEMENT fires — "assert" is a test verb, not an IMPLEMENT verb.
