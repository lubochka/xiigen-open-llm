# Layer 0-B — Router Pairs (007)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: router

---

## R007-001 — TEST+AUTHORIZATION → [5.1, 5.2]

compiled_predicate_ids: ["TEST", "AUTHORIZATION"]
compiled_text: "Test the PermissionEvaluatorService denies access when a user lacks the required role and assert the 403 response code."
RoutingDecisions:
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'test' + 'assert' → TEST → 5.1" }
  - { predicate: AUTHORIZATION, sub_phase: 5.2, routing_note: "'access' + 'role' → AUTHORIZATION → 5.2" }
TransitionType: feature_addition

---

## R007-002 — TEST+IMPLEMENT+SECURITY → [5.1, 4.1, 5.2]

compiled_predicate_ids: ["TEST", "IMPLEMENT", "SECURITY"]
compiled_text: "Write tests for the DataEncryptionService to verify AES-256 encryption and assert decryption correctness with invalid key fails gracefully."
RoutingDecisions:
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'tests' + 'verify' + 'assert' → TEST → 5.1" }
  - { predicate: IMPLEMENT, sub_phase: 4.1, routing_note: "'write' → IMPLEMENT → 4.1" }
  - { predicate: SECURITY, sub_phase: 5.2, routing_note: "'AES-256 encryption' + 'invalid key' → SECURITY → 5.2" }
TransitionType: feature_addition

---

## R007-003 — TEST+IMPLEMENT+SECURITY (full auth) → [5.1, 4.1, 5.2]

compiled_predicate_ids: ["TEST", "SECURITY", "AUTHORIZATION"]
compiled_text: "Test and verify the complete JWT lifecycle — issuance, refresh, revocation — and assert that revoked tokens are rejected by the authorization layer."
RoutingDecisions:
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'test' + 'verify' + 'assert' → TEST → 5.1" }
  - { predicate: SECURITY, sub_phase: 5.2, routing_note: "'JWT' + 'issuance' + 'revocation' → SECURITY → 5.2" }
  - { predicate: AUTHORIZATION, sub_phase: 5.2, routing_note: "'authorization layer' → AUTHORIZATION → 5.2" }
TransitionType: feature_addition

---

## R007-004 — TEST → 5.1 (BDD framing)

compiled_predicate_ids: ["TEST"]
compiled_text: "Test the CheckoutService Given a valid cart When the user places the order Then the order ID must be returned."
RoutingDecisions: [{ predicate: TEST, sub_phase: 5.1, routing_note: "'test' → TEST → 5.1; BDD framing (Given/When/Then) is gap but doesn't affect routing" }]
TransitionType: feature_addition
