# Layer 0-B — Router Pairs (013)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: router

---

## R013-001 — TEST+IMPLEMENT → 5.1 + 4.1 (CartCalculationTests)

A:
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  compiled_text: "Write unit tests for the CartCalculationService verifying item totals, quantity adjustments, and empty-cart edge cases."

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

## R013-002 — TEST alone → 5.1 (assert on subtotal)

A:
  compiled_predicate_ids: ["TEST"]
  compiled_text: "Assert that the CartCalculationService returns the correct subtotal when three items with different unit prices are added."

B:
  RoutingDecisions:
    - predicate: TEST
      sub_phase: 5.1
      sub_phase_label: "Unit and Integration Testing"
      confidence: 0.92
      routing_note: "'assert' → TEST → 5.1"
  TransitionType: documentation
  SubPhaseId: 5.1
  Note: TEST alone → TransitionType documentation. No IMPLEMENT signal in the request.
