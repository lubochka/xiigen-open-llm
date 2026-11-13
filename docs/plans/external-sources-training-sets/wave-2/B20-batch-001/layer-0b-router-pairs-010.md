# Layer 0-B — Router Pairs (010)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: router

---

## R010-001 — TEST+DOCUMENT → [5.1, 6.2] (test spec document)

compiled_predicate_ids: ["TEST", "IMPLEMENT", "DOCUMENT"]
compiled_text: "Write the test specification document for the ReturnPolicyService covering all return window rules."
RoutingDecisions:
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'test specification' → TEST → 5.1" }
  - { predicate: IMPLEMENT, sub_phase: 4.1, routing_note: "'write' → IMPLEMENT → 4.1" }
  - { predicate: DOCUMENT, sub_phase: 6.2, routing_note: "'specification document' → DOCUMENT → 6.2" }
TransitionType: feature_addition

---

## R010-002 — TEST+IMPLEMENT+DOCUMENT → [5.1, 4.1, 6.2] (TDD)

compiled_predicate_ids: ["TEST", "IMPLEMENT"]
compiled_text: "Write failing tests for the SubscriptionService plan upgrade flow before implementing the service logic."
RoutingDecisions:
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'failing tests' → TEST → 5.1" }
  - { predicate: IMPLEMENT, sub_phase: 4.1, routing_note: "'write' + 'implementing' → IMPLEMENT → 4.1" }
TransitionType: feature_addition

---

## R010-003 — TEST → 5.1 (performance test)

compiled_predicate_ids: ["TEST"]
compiled_text: "Test that the ProductSearchService returns results within 200 milliseconds for a dataset of 100,000 products."
RoutingDecisions: [{ predicate: TEST, sub_phase: 5.1, routing_note: "'test' → TEST → 5.1; performance thresholds are gap but don't affect routing" }]
TransitionType: feature_addition

---

## R010-004 — TEST+IMPLEMENT → [5.1, 4.1] (spy/fake doubles)

compiled_predicate_ids: ["TEST", "IMPLEMENT"]
compiled_text: "Implement spy and fake objects for the ExternalPaymentGateway and write integration tests for the ChargeService."
RoutingDecisions:
  - { predicate: IMPLEMENT, sub_phase: 4.1, routing_note: "'implement' → IMPLEMENT → 4.1" }
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'integration tests' → TEST → 5.1" }
TransitionType: feature_addition
Teaching: TEST routes to 5.1 for "integration tests" here (service-level integration, not system E2E).
Sub-phase disambiguation: system-level "integration tests" → 5.3; service-level "integration tests" → 5.1.
