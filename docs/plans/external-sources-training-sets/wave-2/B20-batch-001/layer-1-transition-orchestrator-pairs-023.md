# Layer 1 — Transition Orchestrator Pairs (023)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T023-001 — TEST+IMPLEMENT → feature_addition (CouponApplicatorTests)

A:
  compiled_text: "Write unit tests for the CouponApplicator to verify valid coupons reduce cart total and expired coupons are rejected."
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  flow_id: "UnitTestingFlow51"

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'write' fires IMPLEMENT"
    - "'unit tests' + 'verify' fire TEST"
    - "New coupon test suite = feature_addition"
  PhaseId: 5
  SubPhaseId: 5.1
  TransitionConfidence: 0.92

---

## T023-002 — TEST+IMPLEMENT → feature_addition (create test cases with mock)

A:
  compiled_text: "Create test cases for CouponApplicator using mock coupon repositories to isolate applicator logic."
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  flow_id: "UnitTestingFlow51"

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'create' fires IMPLEMENT"
    - "'test cases' + 'mock' fire TEST"
    - "Mock repositories are new test infrastructure = feature_addition"
  PhaseId: 5
  SubPhaseId: 5.1
  TransitionConfidence: 0.92
