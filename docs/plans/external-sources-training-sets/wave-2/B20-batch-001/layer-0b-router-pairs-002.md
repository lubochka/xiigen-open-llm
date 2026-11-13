# Layer 0-B — Router Pairs (002)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: router

---

## R002-001 — TEST → 5.1

compiled_predicate_ids: ["TEST"]
compiled_text: "Assert the CartService correctly calculates the subtotal when applying a percentage discount."
RoutingDecisions: [{ predicate: TEST, sub_phase: 5.1, label: "Unit and Integration Testing", confidence: 0.92 }]
TransitionType: feature_addition

---

## R002-002 — TEST+IMPLEMENT → [5.1, 4.1]

compiled_predicate_ids: ["TEST", "IMPLEMENT"]
compiled_text: "Implement parameterized test theories for the TaxCalculationService covering all regional tax rates."
RoutingDecisions:
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'test theories' → TEST → 5.1" }
  - { predicate: IMPLEMENT, sub_phase: 4.1, routing_note: "'implement' → IMPLEMENT → 4.1" }
TransitionType: feature_addition

---

## R002-003 — TEST+API_CONTRACT → [5.1, 3.3]

compiled_predicate_ids: ["TEST", "API_CONTRACT"]
compiled_text: "Write integration tests for the ProductRepository API contract and assert the pagination interface."
RoutingDecisions:
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'integration tests' → TEST → 5.1" }
  - { predicate: API_CONTRACT, sub_phase: 3.3, routing_note: "'API contract' + 'interface' → API_CONTRACT → 3.3" }
  - { predicate: IMPLEMENT, sub_phase: 4.1, routing_note: "'write' → IMPLEMENT → 4.1" }
TransitionType: feature_addition

---

## R002-004 — TEST+AUTHORIZATION+SECURITY → [5.1, 5.2, 5.2]

compiled_predicate_ids: ["TEST", "AUTHORIZATION", "SECURITY"]
compiled_text: "Test the authorization middleware to verify that role-based access rules are enforced correctly."
RoutingDecisions:
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'test' + 'verify' → TEST → 5.1" }
  - { predicate: AUTHORIZATION, sub_phase: 5.2, routing_note: "'authorization' + 'role-based access' → AUTHORIZATION → 5.2" }
  - { predicate: SECURITY, sub_phase: 5.2, routing_note: "'enforced' in access context → SECURITY → 5.2" }
Note: Both AUTHORIZATION and SECURITY route to 5.2 when in a testing-sentence context — each fires independently but both resolve to 5.2.
TransitionType: feature_addition
