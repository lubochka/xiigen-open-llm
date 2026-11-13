# Layer 1 — Transition Orchestrator Pairs (014)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T014-001 — TEST+IMPLEMENT → feature_addition (InventoryAllocationTests)

A:
  compiled_text: "Write unit tests for the InventoryAllocationService to verify that stock is reserved correctly."
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  flow_id: "UnitTestingFlow51"

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'write' fires IMPLEMENT — construction action = feature_addition"
    - "'unit tests' + 'verify' — adding new test coverage"
    - "Net-new test suite: InventoryAllocationService has no prior coverage in scope"
  PhaseId: 5
  SubPhaseId: 5.1
  TransitionConfidence: 0.92

---

## T014-002 — TEST alone → documentation (spec rollback)

A:
  compiled_text: "Spec the rollback behaviour of InventoryAllocationService: when payment fails, reserved stock must be fully released."
  compiled_predicate_ids: ["TEST"]
  flow_id: "UnitTestingFlow51"

B:
  TransitionType: documentation
  TransitionEvidence:
    - "'spec' is a specification verb → TEST fires; no IMPLEMENT verb present"
    - "Specifying expected rollback behaviour = documentation (documenting a test specification)"
    - "No artifact is constructed in this request"
  PhaseId: 5
  SubPhaseId: 5.1
  TransitionConfidence: 0.88
