# Layer 0-B — Router Pairs (023)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: router

---

## R023-001 — TEST+IMPLEMENT → 5.1 + 4.1 (CouponApplicatorTests)

A:
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  compiled_text: "Write unit tests for the CouponApplicator to verify that valid coupons reduce the cart total and expired coupons are rejected."

B:
  RoutingDecisions:
    - predicate: TEST
      sub_phase: 5.1
      sub_phase_label: "Unit and Integration Testing"
      confidence: 0.92
      routing_note: "'unit tests' + 'verify' → TEST → 5.1"
    - predicate: IMPLEMENT
      sub_phase: 4.1
      sub_phase_label: "Service and Logic Implementation"
      confidence: 0.92
      routing_note: "'write' → IMPLEMENT → 4.1"
  TransitionType: feature_addition
  PrimarySubPhaseId: 5.1
  AllSubPhaseIds: [5.1, 4.1]

---

## R023-002 — TEST+IMPLEMENT → 5.1 + 4.1 (create test cases with mock)

A:
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  compiled_text: "Create test cases for CouponApplicator that use mock coupon repositories to isolate applicator logic from database access."

B:
  RoutingDecisions:
    - predicate: TEST
      sub_phase: 5.1
      sub_phase_label: "Unit and Integration Testing"
      confidence: 0.92
      routing_note: "'test cases' + 'mock' → TEST → 5.1"
    - predicate: IMPLEMENT
      sub_phase: 4.1
      sub_phase_label: "Service and Logic Implementation"
      confidence: 0.92
      routing_note: "'create' → IMPLEMENT → 4.1"
  TransitionType: feature_addition
  PrimarySubPhaseId: 5.1
  AllSubPhaseIds: [5.1, 4.1]
  Note: "mock" fires TEST (test-infrastructure vocabulary). "create" fires IMPLEMENT independently.
