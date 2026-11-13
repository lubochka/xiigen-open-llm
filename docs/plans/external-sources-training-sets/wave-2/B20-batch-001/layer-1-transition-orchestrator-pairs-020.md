# Layer 1 — Transition Orchestrator Pairs (020)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T020-001 — TEST+IMPLEMENT+SECURITY → feature_addition (PasswordValidatorTests)

A:
  compiled_text: "Write security tests for the PasswordValidator to assert that hashed passwords are never logged and validation timing is constant."
  compiled_predicate_ids: ["TEST", "IMPLEMENT", "SECURITY"]
  flow_id: "UnitTestingFlow51"

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'write' fires IMPLEMENT — construction verb"
    - "'security tests' fires TEST (via 'tests') + SECURITY (via 'security')"
    - "'assert' fires TEST — asserting security properties"
    - "'hashed passwords' fires SECURITY"
    - "Net-new security test suite = feature_addition"
  PhaseId: 5
  SubPhaseId: 5.1
  TransitionConfidence: 0.92
  Multi-predicate TransitionType note: A single TransitionType applies to the request as a whole.
  Even with three predicates (TEST, IMPLEMENT, SECURITY), the TransitionType is feature_addition
  because new artifacts are being constructed. Individual predicates do not carry separate TransitionTypes.

---

## T020-002 — TEST alone → documentation (spec password history)

A:
  compiled_text: "Spec the password history rule: the PasswordValidator must reject any of the user's last 5 passwords."
  compiled_predicate_ids: ["TEST"]
  flow_id: "UnitTestingFlow51"

B:
  TransitionType: documentation
  TransitionEvidence:
    - "'spec' fires TEST; no IMPLEMENT verb → documentation"
    - "Specifying a password history rule = test specification = documentation"
  PhaseId: 5
  SubPhaseId: 5.1
  TransitionConfidence: 0.88
