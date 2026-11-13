# Layer 0-B — Router Pairs (003)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: router

---

## R003-001 — TEST+DOCUMENT → [5.1, 6.2]

compiled_predicate_ids: ["TEST", "DOCUMENT"]
compiled_text: "Spec out the ReportService data export tests and document the expected output schema."
RoutingDecisions:
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'spec' + 'tests' → TEST → 5.1" }
  - { predicate: DOCUMENT, sub_phase: 6.2, routing_note: "'document' → DOCUMENT → 6.2" }
TransitionType: feature_addition

---

## R003-002 — TEST+SECURITY → [5.1, 5.2]

compiled_predicate_ids: ["TEST", "SECURITY"]
compiled_text: "Test that the PasswordHashingService applies bcrypt with a minimum cost factor of 12 and verify salt uniqueness."
RoutingDecisions:
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'test' + 'verify' → TEST → 5.1" }
  - { predicate: SECURITY, sub_phase: 5.2, routing_note: "'salt' (cryptographic) + 'PasswordHashing' → SECURITY → 5.2" }
TransitionType: feature_addition

---

## R003-003 — TEST+QA → [5.1, 7.3]

compiled_predicate_ids: ["TEST", "QA"]
compiled_text: "QA the PaymentCalculationService test suite and verify that all edge case tests pass."
RoutingDecisions:
  - { predicate: QA, sub_phase: 7.3, routing_note: "'QA' → QA → 7.3" }
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'test suite' + 'verify' + 'tests' → TEST → 5.1" }
TransitionType: feature_addition
Teaching: QA routes to 7.3; TEST routes to 5.1. Two separate routing decisions even when both
predicates appear in the same request.

---

## R003-004 — TEST+IMPLEMENT+DOCUMENT → [5.1, 4.1, 6.2]

compiled_predicate_ids: ["TEST", "IMPLEMENT", "DOCUMENT"]
compiled_text: "Implement test coverage for the PricingEngine, write tests for all formula variants, and document the test matrix."
RoutingDecisions:
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'test coverage' + 'write tests' + 'test matrix' → TEST → 5.1" }
  - { predicate: IMPLEMENT, sub_phase: 4.1, routing_note: "'implement' + 'write' → IMPLEMENT → 4.1" }
  - { predicate: DOCUMENT, sub_phase: 6.2, routing_note: "'document' → DOCUMENT → 6.2" }
TransitionType: feature_addition
