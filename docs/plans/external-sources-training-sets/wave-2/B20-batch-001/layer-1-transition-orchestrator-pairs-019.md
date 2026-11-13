# Layer 1 — Transition Orchestrator Pairs (019)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T019-001 — TEST+IMPLEMENT → feature_addition (FraudScoreTests — TEST+IMPLEMENT)

A:
  compiled_text: "Write unit tests for the FraudScoreCalculator and implement the test doubles for the external risk-scoring API."
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  flow_id: "UnitTestingFlow51"

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'write' fires IMPLEMENT — writing tests"
    - "'implement' fires IMPLEMENT — constructing test doubles"
    - "'unit tests' + 'test doubles' fire TEST"
    - "Both test suite and test infrastructure (doubles) are new artifacts = feature_addition"
  PhaseId: 5
  SubPhaseId: 5.1
  TransitionConfidence: 0.92

---

## T019-002 — TEST+IMPLEMENT → feature_addition (develop tests with mock)

A:
  compiled_text: "Develop unit tests for FraudScoreCalculator with a mock device-fingerprint service to isolate the scoring logic."
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  flow_id: "UnitTestingFlow51"

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'develop' fires IMPLEMENT"
    - "'unit tests' + 'mock' fire TEST"
    - "Mock device-fingerprint service is new test infrastructure"
  PhaseId: 5
  SubPhaseId: 5.1
  TransitionConfidence: 0.92

---

## T019-003 — ZERO predicates → no TransitionType (run regression tests)

A:
  compiled_text: "Run regression tests for the FraudScoreCalculator nightly."
  compiled_predicate_ids: []

B:
  TransitionType: unknown
  PhaseId: null
  SubPhaseId: null
  SourceStatus: knowledge_edge
  Note: "run" produces zero predicates → no TransitionType. The orchestrator does not assign a phase.
