# Layer 0-B — Router Pairs (017)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: router

---

## R017-001 — TEST+IMPLEMENT → 5.1 + 4.1 (ShippingCostTests)

A:
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  compiled_text: "Write unit tests for the ShippingCostCalculator covering standard, express, and overnight rates by weight tier."

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
  TransitionType: feature_addition
  PrimarySubPhaseId: 5.1
  AllSubPhaseIds: [5.1, 4.1]

---

## R017-002 — TEST alone → 5.1 (verify weight-tier boundary)

A:
  compiled_predicate_ids: ["TEST"]
  compiled_text: "Verify that the ShippingCostCalculator transitions from the 'standard' to 'heavy' rate at exactly 5 kg."

B:
  RoutingDecisions:
    - predicate: TEST
      sub_phase: 5.1
      sub_phase_label: "Unit and Integration Testing"
      confidence: 0.92
      routing_note: "'verify' → TEST → 5.1"
  TransitionType: documentation
  SubPhaseId: 5.1
  Note: "transitions" in the request is a domain state-change term, not the IMPLEMENT verb. No IMPLEMENT fires.
