# Layer 0-B — Router Pairs (004)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: router

---

## R004-001 — TEST+SECURITY+IMPLEMENT → [5.1, 5.2, 4.1]

compiled_predicate_ids: ["TEST", "SECURITY", "IMPLEMENT"]
compiled_text: "Write tests to verify the SearchService sanitizes query inputs and assert it resists SQL injection attempts."
RoutingDecisions:
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'tests' + 'verify' + 'assert' → TEST → 5.1" }
  - { predicate: SECURITY, sub_phase: 5.2, routing_note: "'sanitizes' + 'SQL injection' → SECURITY → 5.2" }
  - { predicate: IMPLEMENT, sub_phase: 4.1, routing_note: "'write' → IMPLEMENT → 4.1" }
TransitionType: feature_addition

---

## R004-002 — TEST+ARCHITECTURE → [5.1, 3.1]

compiled_predicate_ids: ["TEST", "ARCHITECTURE", "IMPLEMENT"]
compiled_text: "Design the test architecture for the EventBus and write unit tests covering all event handler registrations."
RoutingDecisions:
  - { predicate: ARCHITECTURE, sub_phase: 3.1, routing_note: "'design' + 'test architecture' → ARCHITECTURE → 3.1 (app-level)" }
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'unit tests' → TEST → 5.1" }
  - { predicate: IMPLEMENT, sub_phase: 4.1, routing_note: "'write' → IMPLEMENT → 4.1" }
TransitionType: feature_addition

---

## R004-003 — TEST+API_CONTRACT+IMPLEMENT → [5.1, 3.3, 4.1]

compiled_predicate_ids: ["TEST", "API_CONTRACT", "IMPLEMENT"]
compiled_text: "Build the integration test suite, implement mock API contracts for the shipping provider interface, and verify all error response codes."
RoutingDecisions:
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'integration test suite' + 'verify' → TEST → 5.1" }
  - { predicate: API_CONTRACT, sub_phase: 3.3, routing_note: "'API contracts' + 'interface' → API_CONTRACT → 3.3" }
  - { predicate: IMPLEMENT, sub_phase: 4.1, routing_note: "'build' + 'implement' → IMPLEMENT → 4.1" }
TransitionType: feature_addition

---

## R004-004 — TEST+SECURITY+AUTHORIZATION → [5.1, 5.2, 5.2]

compiled_predicate_ids: ["TEST", "SECURITY", "AUTHORIZATION"]
compiled_text: "Test the complete authentication and authorization flow — verify login, assert token issuance, and validate role enforcement."
RoutingDecisions:
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'test' + 'verify' + 'assert' + 'validate' → TEST → 5.1" }
  - { predicate: SECURITY, sub_phase: 5.2, routing_note: "'authentication' + 'token' → SECURITY → 5.2" }
  - { predicate: AUTHORIZATION, sub_phase: 5.2, routing_note: "'authorization' + 'role enforcement' → AUTHORIZATION → 5.2" }
TransitionType: feature_addition
