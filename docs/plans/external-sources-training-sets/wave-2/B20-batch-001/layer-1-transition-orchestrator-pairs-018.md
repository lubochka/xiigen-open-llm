# Layer 1 — Transition Orchestrator Pairs (018)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T018-001 — TEST+IMPLEMENT → feature_addition (LoyaltyPointsTests)

A:
  compiled_text: "Write unit tests for the LoyaltyPointsService verifying that points are awarded at the correct rate per pound spent."
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  flow_id: "UnitTestingFlow51"

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'write' fires IMPLEMENT — construction action"
    - "'unit tests' + 'verifying' fire TEST — new test suite"
  PhaseId: 5
  SubPhaseId: 5.1
  TransitionConfidence: 0.92

---

## T018-002 — TEST alone → documentation (spec tier multiplier)

A:
  compiled_text: "Spec the loyalty tier multiplier: gold 3x, silver 2x, standard 1x."
  compiled_predicate_ids: ["TEST"]
  flow_id: "UnitTestingFlow51"

B:
  TransitionType: documentation
  TransitionEvidence:
    - "'spec' fires TEST; no IMPLEMENT verb; TEST alone → documentation"
    - "Specifying expected multiplier values = documenting test expectations"
  PhaseId: 5
  SubPhaseId: 5.1
  TransitionConfidence: 0.88

---

## T018-003 — TEST+IMPLEMENT → feature_addition (create test cases)

A:
  compiled_text: "Create unit test cases for LoyaltyPointsService covering point accrual, manual adjustment, and account closure scenarios."
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  flow_id: "UnitTestingFlow51"

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'create' fires IMPLEMENT"
    - "'unit test cases' fires TEST"
    - "Three new test scenarios = net-new test coverage = feature_addition"
  PhaseId: 5
  SubPhaseId: 5.1
  TransitionConfidence: 0.92
