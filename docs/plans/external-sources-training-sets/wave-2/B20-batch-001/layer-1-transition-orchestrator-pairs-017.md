# Layer 1 — Transition Orchestrator Pairs (017)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T017-001 — TEST+IMPLEMENT → feature_addition (ShippingCostTests)

A:
  compiled_text: "Write unit tests for the ShippingCostCalculator covering standard, express, and overnight rates by weight tier."
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  flow_id: "UnitTestingFlow51"

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'write' fires IMPLEMENT"
    - "'unit tests' fires TEST — adding new test coverage"
    - "Three rate tiers all require new test methods"
  PhaseId: 5
  SubPhaseId: 5.1
  TransitionConfidence: 0.92

---

## T017-002 — TEST alone → documentation (validate international surcharge)

A:
  compiled_text: "Validate that the ShippingCostCalculator adds the correct international surcharge for orders shipped outside the domestic zone."
  compiled_predicate_ids: ["TEST"]
  flow_id: "UnitTestingFlow51"

B:
  TransitionType: documentation
  TransitionEvidence:
    - "'validate' fires TEST; no IMPLEMENT verb"
    - "Asserting a business rule = documentation classification"
  PhaseId: 5
  SubPhaseId: 5.1
  TransitionConfidence: 0.88

---

## T017-003 — ZERO predicates → no TransitionType (run unit tests)

A:
  compiled_text: "Run unit tests for the ShippingCostCalculator after the weight-tier refactor."
  compiled_predicate_ids: []

B:
  TransitionType: unknown
  TransitionEvidence:
    - "'run' is execution — zero predicates → no TransitionType assigned"
  PhaseId: null
  SubPhaseId: null
  SourceStatus: knowledge_edge
