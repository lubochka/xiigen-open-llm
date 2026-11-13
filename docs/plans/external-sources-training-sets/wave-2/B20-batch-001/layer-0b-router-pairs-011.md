# Layer 0-B — Router Pairs (011)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: router

---

## R011-001 — TEST+API_CONTRACT → [5.1, 3.3] (REST endpoint test)

compiled_predicate_ids: ["TEST", "API_CONTRACT"]
compiled_text: "Test the OrdersController REST endpoints and verify the response contracts match the OpenAPI specification."
RoutingDecisions:
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'test' + 'verify' → TEST → 5.1" }
  - { predicate: API_CONTRACT, sub_phase: 3.3, routing_note: "'REST endpoints' + 'response contracts' → API_CONTRACT → 3.3" }
TransitionType: feature_addition

---

## R011-002 — TEST+SECURITY → [5.1, 5.2] (AES encryption)

compiled_predicate_ids: ["TEST", "IMPLEMENT", "SECURITY"]
compiled_text: "Write unit tests for the DataEncryptionService to verify AES-256 encryption and assert decryption correctness."
RoutingDecisions:
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'unit tests' + 'verify' + 'assert' → TEST → 5.1" }
  - { predicate: IMPLEMENT, sub_phase: 4.1, routing_note: "'write' → IMPLEMENT → 4.1" }
  - { predicate: SECURITY, sub_phase: 5.2, routing_note: "'AES-256 encryption' → SECURITY → 5.2" }
TransitionType: feature_addition

---

## R011-003 — TEST+IMPLEMENT → [5.1, 4.1] (assertion helpers)

compiled_predicate_ids: ["TEST", "IMPLEMENT"]
compiled_text: "Add a factory method helper for creating test instances of the CustomerProfile entity and write assertion helpers."
RoutingDecisions:
  - { predicate: IMPLEMENT, sub_phase: 4.1, routing_note: "'add' + 'write' → IMPLEMENT → 4.1" }
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'test instances' + 'assertion helpers' → TEST → 5.1" }
TransitionType: feature_addition

---

## R011-004 — TEST → 5.1 (spec behaviour)

compiled_predicate_ids: ["TEST"]
compiled_text: "Spec the behaviour of the AuditLogService when a write fails — it must not swallow the error silently."
RoutingDecisions: [{ predicate: TEST, sub_phase: 5.1, routing_note: "'spec' → TEST → 5.1" }]
TransitionType: feature_addition
