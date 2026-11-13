# Layer 0-B — Router Pairs (006)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: router

---

## R006-001 — TEST → 5.1 (assert idempotency)

compiled_predicate_ids: ["TEST"]
compiled_text: "Assert that the CouponService redemption is idempotent — applying the same coupon twice must not double-apply the discount."
RoutingDecisions: [{ predicate: TEST, sub_phase: 5.1, routing_note: "'assert' → TEST → 5.1" }]
TransitionType: feature_addition

---

## R006-002 — TEST+IMPLEMENT → [5.1, 4.1] (test fixtures)

compiled_predicate_ids: ["TEST", "IMPLEMENT"]
compiled_text: "Create test fixtures for the ShippingService and add tests for domestic vs international rate calculation."
RoutingDecisions:
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'test fixtures' + 'tests' → TEST → 5.1" }
  - { predicate: IMPLEMENT, sub_phase: 4.1, routing_note: "'create' + 'add' → IMPLEMENT → 4.1" }
TransitionType: feature_addition

---

## R006-003 — TEST+IMPLEMENT → [5.1, 4.1] (test data builder)

compiled_predicate_ids: ["TEST", "IMPLEMENT"]
compiled_text: "Build a test data builder for the InvoiceService and write tests covering multi-line invoice generation."
RoutingDecisions:
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'test data builder' + 'write tests' → TEST → 5.1" }
  - { predicate: IMPLEMENT, sub_phase: 4.1, routing_note: "'build' + 'write' → IMPLEMENT → 4.1" }
TransitionType: feature_addition

---

## R006-004 — TEST+IMPLEMENT+API_CONTRACT+DOCUMENT → [5.1, 4.1, 3.3, 6.2]

compiled_predicate_ids: ["TEST", "IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
compiled_text: "Implement test coverage for the PricingEngine REST API, write tests verifying all contract endpoints, and document the coverage results."
RoutingDecisions:
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'test coverage' + 'tests' + 'verifying' → TEST → 5.1" }
  - { predicate: IMPLEMENT, sub_phase: 4.1, routing_note: "'implement' + 'write' → IMPLEMENT → 4.1" }
  - { predicate: API_CONTRACT, sub_phase: 3.3, routing_note: "'REST API' + 'contract endpoints' → API_CONTRACT → 3.3" }
  - { predicate: DOCUMENT, sub_phase: 6.2, routing_note: "'document' → DOCUMENT → 6.2" }
TransitionType: feature_addition
Teaching: Four predicates → four independent routing decisions. No predicate is collapsed.
