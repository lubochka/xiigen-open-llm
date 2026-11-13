# Layer 0-B — Router Pairs (012)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: router

---

## R012-001 — TEST+IMPLEMENT+DOCUMENT → [5.1, 4.1, 6.2]

compiled_predicate_ids: ["TEST", "IMPLEMENT", "DOCUMENT"]
compiled_text: "Implement test coverage for the PricingEngine, write tests for all formula variants, and document the test matrix."
RoutingDecisions:
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'test coverage' + 'tests' + 'test matrix' → TEST → 5.1" }
  - { predicate: IMPLEMENT, sub_phase: 4.1, routing_note: "'implement' + 'write' → IMPLEMENT → 4.1" }
  - { predicate: DOCUMENT, sub_phase: 6.2, routing_note: "'document' → DOCUMENT → 6.2" }
TransitionType: feature_addition

---

## R012-002 — TEST+SECURITY+AUTHORIZATION → [5.1, 5.2, 5.2]

compiled_predicate_ids: ["TEST", "SECURITY", "AUTHORIZATION"]
compiled_text: "Test and verify the complete JWT lifecycle — issuance, refresh, revocation — and assert that revoked tokens are rejected by the authorization layer."
RoutingDecisions:
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'test' + 'verify' + 'assert' → TEST → 5.1" }
  - { predicate: SECURITY, sub_phase: 5.2, routing_note: "'JWT' + 'issuance' + 'revocation' → SECURITY → 5.2" }
  - { predicate: AUTHORIZATION, sub_phase: 5.2, routing_note: "'authorization layer' → AUTHORIZATION → 5.2" }
TransitionType: feature_addition

---

## R012-003 — TEST+QA → [5.1, 7.3]

compiled_predicate_ids: ["TEST", "QA"]
compiled_text: "Write unit tests for the ProductService and QA all acceptance criteria before the sprint review."
RoutingDecisions:
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'unit tests' → TEST → 5.1" }
  - { predicate: IMPLEMENT, sub_phase: 4.1, routing_note: "'write' → IMPLEMENT → 4.1" }
  - { predicate: QA, sub_phase: 7.3, routing_note: "'QA' → QA → 7.3" }
TransitionType: feature_addition

---

## R012-004 — TEST+IMPLEMENT → [5.1, 4.1] (test-first)

compiled_predicate_ids: ["TEST", "IMPLEMENT"]
compiled_text: "Write failing tests for the SubscriptionService plan upgrade flow before implementing the service logic."
RoutingDecisions:
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'failing tests' → TEST → 5.1" }
  - { predicate: IMPLEMENT, sub_phase: 4.1, routing_note: "'write' + 'implementing' → IMPLEMENT → 4.1" }
TransitionType: feature_addition
Teaching: "Failing tests" still fires TEST — the test intent is clear regardless of expected test status.
