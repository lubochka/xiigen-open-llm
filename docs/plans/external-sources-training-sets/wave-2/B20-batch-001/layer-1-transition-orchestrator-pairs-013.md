# Layer 1 — Transition Orchestrator Pairs (013)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T013-001 — TEST+IMPLEMENT → feature_addition (CartCalculationTests)

A:
  compiled_text: "Write unit tests for the CartCalculationService verifying item totals, quantity adjustments, and empty-cart edge cases."
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  flow_id: "UnitTestingFlow51"

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'write unit tests' — constructing new test artifacts = feature_addition"
    - "'verifying' — adding verification coverage = feature_addition"
    - "No refactoring vocabulary (refactor/extract/restructure)"
    - "No migration vocabulary (migrate/upgrade/port)"
  PhaseId: 5
  SubPhaseId: 5.1
  TransitionConfidence: 0.92

---

## T013-002 — TEST alone → documentation (assert subtotal)

A:
  compiled_text: "Assert that the CartCalculationService returns the correct subtotal when three items with different unit prices are added."
  compiled_predicate_ids: ["TEST"]
  flow_id: "UnitTestingFlow51"

B:
  TransitionType: documentation
  TransitionEvidence:
    - "TEST alone with no IMPLEMENT verb → documentation TransitionType"
    - "'assert' is a test-verification verb, not a construction verb"
    - "No 'write', 'create', 'build', 'develop', 'implement' in the request"
  PhaseId: 5
  SubPhaseId: 5.1
  TransitionConfidence: 0.88
  Note: TransitionType belongs to IMPLEMENT. TEST alone → documentation (specifying expected behaviour, not constructing an artifact).
