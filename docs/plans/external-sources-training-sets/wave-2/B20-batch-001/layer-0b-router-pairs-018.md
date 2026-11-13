# Layer 0-B — Router Pairs (018)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: router

---

## R018-001 — TEST+IMPLEMENT → 5.1 + 4.1 (LoyaltyPointsTests)

A:
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  compiled_text: "Write unit tests for the LoyaltyPointsService verifying that points are awarded at the correct rate per pound spent."

B:
  RoutingDecisions:
    - predicate: TEST
      sub_phase: 5.1
      sub_phase_label: "Unit and Integration Testing"
      confidence: 0.92
      routing_note: "'unit tests' + 'verifying' → TEST → 5.1"
    - predicate: IMPLEMENT
      sub_phase: 4.1
      sub_phase_label: "Service and Logic Implementation"
      confidence: 0.92
      routing_note: "'write' → IMPLEMENT → 4.1"
  TransitionType: feature_addition
  PrimarySubPhaseId: 5.1
  AllSubPhaseIds: [5.1, 4.1]

---

## R018-002 — TEST alone → 5.1 (spec tier multiplier)

A:
  compiled_predicate_ids: ["TEST"]
  compiled_text: "Spec the loyalty tier multiplier: gold members earn 3x points, silver members earn 2x, standard members earn 1x."

B:
  RoutingDecisions:
    - predicate: TEST
      sub_phase: 5.1
      sub_phase_label: "Unit and Integration Testing"
      confidence: 0.92
      routing_note: "'spec' → TEST → 5.1"
  TransitionType: documentation
  SubPhaseId: 5.1
  Note: "spec" fires TEST. No IMPLEMENT vocabulary present. TransitionType is documentation.
