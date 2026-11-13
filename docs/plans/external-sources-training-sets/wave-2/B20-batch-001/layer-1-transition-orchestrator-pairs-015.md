# Layer 1 — Transition Orchestrator Pairs (015)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T015-001 — TEST+IMPLEMENT → feature_addition (DiscountEngineTests)

A:
  compiled_text: "Write unit tests for the DiscountEngine verifying percentage, fixed-amount, and buy-one-get-one discount logic."
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  flow_id: "UnitTestingFlow51"

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'write' (IMPLEMENT) + 'unit tests' (TEST) — constructing a new test suite"
    - "Buy-one-get-one (BOGO) logic test coverage is net-new"
    - "No refactoring, migration, or documentation signals"
  PhaseId: 5
  SubPhaseId: 5.1
  TransitionConfidence: 0.92

---

## T015-002 — TEST alone → documentation (validate minimum threshold)

A:
  compiled_text: "Validate that the DiscountEngine applies 15% discount only when cart total exceeds minimum order threshold."
  compiled_predicate_ids: ["TEST"]
  flow_id: "UnitTestingFlow51"

B:
  TransitionType: documentation
  TransitionEvidence:
    - "'validate' is TEST vocabulary (universal rule) — no IMPLEMENT verb"
    - "Validating a rule = asserting a specification = documentation classification"
    - "No 'write', 'create', 'build', 'develop', 'implement' present"
  PhaseId: 5
  SubPhaseId: 5.1
  TransitionConfidence: 0.88
  Teaching: "validate" always fires TEST. When TEST is the only predicate, TransitionType is documentation.
  The "documentation" classification means specifying/asserting expected behaviour, not writing prose documentation.
