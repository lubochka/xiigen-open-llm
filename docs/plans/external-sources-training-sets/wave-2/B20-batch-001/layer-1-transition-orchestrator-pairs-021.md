# Layer 1 — Transition Orchestrator Pairs (021)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T021-001 — TEST+IMPLEMENT+SECURITY → feature_addition (JwtTokenBuilderTests)

A:
  compiled_text: "Write unit tests for the JwtTokenBuilder covering token generation, claims insertion, and expiry enforcement."
  compiled_predicate_ids: ["TEST", "IMPLEMENT", "SECURITY"]
  flow_id: "UnitTestingFlow51"

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'write' fires IMPLEMENT"
    - "'unit tests' fires TEST"
    - "'JWT' + 'token' + 'claims' fire SECURITY in testing context"
    - "New JWT test suite = feature_addition"
  PhaseId: 5
  SubPhaseId: 5.1
  TransitionConfidence: 0.92

---

## T021-002 — TEST+SECURITY → feature_addition (assert token signature)

A:
  compiled_text: "Assert that the JwtTokenBuilder produces tokens signed with RS256 and tampered tokens fail signature verification."
  compiled_predicate_ids: ["TEST", "SECURITY"]
  flow_id: "UnitTestingFlow51"

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'assert' fires TEST — asserting a security property"
    - "'JWT' + 'signed' + 'tampered' + 'signature verification' fire SECURITY"
    - "No IMPLEMENT verb — no construction action; but two predicates → feature_addition"
  PhaseId: 5
  SubPhaseId: 5.1
  TransitionConfidence: 0.92
  Note: TEST alone → documentation. But TEST+SECURITY (two predicates) → feature_addition,
  because combining predicates indicates a multi-dimensional action that goes beyond specification.
