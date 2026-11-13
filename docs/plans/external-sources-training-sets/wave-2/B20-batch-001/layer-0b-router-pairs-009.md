# Layer 0-B — Router Pairs (009)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: router

---

## R009-001 — TEST → 5.1 (concurrency)

compiled_predicate_ids: ["TEST"]
compiled_text: "Verify the InventoryService stock reservation handles concurrent requests without overselling."
RoutingDecisions: [{ predicate: TEST, sub_phase: 5.1, routing_note: "'verify' → TEST → 5.1" }]
TransitionType: feature_addition

---

## R009-002 — TEST → 5.1 (negative path)

compiled_predicate_ids: ["TEST"]
compiled_text: "Test that the PromotionService throws a validation failure when a promotion code has expired."
RoutingDecisions: [{ predicate: TEST, sub_phase: 5.1, routing_note: "'test' → TEST → 5.1" }]
TransitionType: feature_addition

---

## R009-003 — TEST+IMPLEMENT → [5.1, 4.1] (extend)

compiled_predicate_ids: ["TEST", "IMPLEMENT"]
compiled_text: "Extend the existing test suite and write additional tests for the RefundService edge cases."
RoutingDecisions:
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'test suite' + 'tests' → TEST → 5.1" }
  - { predicate: IMPLEMENT, sub_phase: 4.1, routing_note: "'extend' + 'write' → IMPLEMENT → 4.1" }
TransitionType: feature_addition

---

## R009-004 — TEST+IMPLEMENT → [5.1, 4.1] (develop + write)

compiled_predicate_ids: ["TEST", "IMPLEMENT"]
compiled_text: "Develop the WishlistService and write unit tests for add, remove, and merge operations."
RoutingDecisions:
  - { predicate: IMPLEMENT, sub_phase: 4.1, routing_note: "'develop' + 'write' → IMPLEMENT → 4.1" }
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'unit tests' → TEST → 5.1" }
TransitionType: feature_addition
Teaching: "develop" fires IMPLEMENT at 4.1 (no connector/migration vocabulary present).
