# Layer 0-B — Router Pairs (015)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: router

---

## R015-001 — TEST+IMPLEMENT → 5.1 + 4.1 (DiscountEngineTests)

A:
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  compiled_text: "Write unit tests for the DiscountEngine verifying percentage, fixed-amount, and buy-one-get-one discount logic."

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

## R015-002 — TEST alone → 5.1 (assert discount exclusion)

A:
  compiled_predicate_ids: ["TEST"]
  compiled_text: "Assert that the DiscountEngine does not apply stacked discounts when the product is flagged as non-combinable."

B:
  RoutingDecisions:
    - predicate: TEST
      sub_phase: 5.1
      sub_phase_label: "Unit and Integration Testing"
      confidence: 0.92
      routing_note: "'assert' → TEST → 5.1"
  TransitionType: documentation
  SubPhaseId: 5.1
