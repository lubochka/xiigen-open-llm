# Layer 0-B — Router Pairs (005)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: router

---

## R005-001 — TEST → 5.1 (validate)

compiled_predicate_ids: ["TEST"]
compiled_text: "Validate the OrderHistoryService query results return only records belonging to the requesting user."
RoutingDecisions: [{ predicate: TEST, sub_phase: 5.1, routing_note: "'validate' → TEST → 5.1" }]
TransitionType: feature_addition

---

## R005-002 — TEST+IMPLEMENT → [5.1, 4.1] (scaffold)

compiled_predicate_ids: ["TEST", "IMPLEMENT"]
compiled_text: "Scaffold the unit test project for the BillingModule and implement test cases for invoice generation and payment processing."
RoutingDecisions:
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'unit test' + 'test cases' → TEST → 5.1" }
  - { predicate: IMPLEMENT, sub_phase: 4.1, routing_note: "'scaffold' + 'implement' → IMPLEMENT → 4.1" }
TransitionType: feature_addition

---

## R005-003 — TEST+DOCUMENT → [5.1, 6.2] (test plan)

compiled_predicate_ids: ["TEST", "IMPLEMENT", "DOCUMENT"]
compiled_text: "Write a test plan for the SubscriptionRenewalService and document the happy path, failure path, and retry scenarios."
RoutingDecisions:
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'test plan' → TEST → 5.1" }
  - { predicate: IMPLEMENT, sub_phase: 4.1, routing_note: "'write' → IMPLEMENT → 4.1" }
  - { predicate: DOCUMENT, sub_phase: 6.2, routing_note: "'document' → DOCUMENT → 6.2" }
TransitionType: feature_addition

---

## R005-004 — TEST+SECURITY → [5.1, 5.2] (injection)

compiled_predicate_ids: ["TEST", "SECURITY", "IMPLEMENT"]
compiled_text: "Implement OAuth token validation tests and write test cases that verify the service rejects tampered JWTs."
RoutingDecisions:
  - { predicate: IMPLEMENT, sub_phase: 4.1, routing_note: "'implement' + 'write' → IMPLEMENT → 4.1" }
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'token validation tests' + 'test cases' + 'verify' → TEST → 5.1" }
  - { predicate: SECURITY, sub_phase: 5.2, routing_note: "'OAuth' + 'tampered JWTs' → SECURITY → 5.2" }
TransitionType: feature_addition
