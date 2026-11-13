# Layer 0-B — Router Pairs (006)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: router

---

## O1-router-006-001 — SECURITY+AUTHORIZATION+API_CONTRACT+TEST → 3.4+3.3+5.1 (three routing decisions)

Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "TEST"] | Routes to: 3.4, 3.3, 5.1
Routing decisions: 3
Decision 1: SECURITY+AUTHORIZATION → 3.4 | EvidencePredicates: ["SECURITY", "AUTHORIZATION"]
Decision 2: API_CONTRACT → 3.3 | EvidencePredicates: ["API_CONTRACT"]
Decision 3: TEST → 5.1 | EvidencePredicates: ["TEST"]
Key rule: 4 predicates → 3 routing decisions because SECURITY+AUTHORIZATION share 3.4
Source: "We need to add multi-tenant JWT authentication to the API and make sure tenant isolation is tested."

---

## O1-router-006-002 — SECURITY+AUTHORIZATION+API_CONTRACT → 3.4+3.3 (two routing decisions)

Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT"] | Routes to: 3.4, 3.3
Routing decisions: 2
Decision 1: SECURITY+AUTHORIZATION → 3.4 | EvidencePredicates: ["SECURITY", "AUTHORIZATION"]
Decision 2: API_CONTRACT → 3.3 | EvidencePredicates: ["API_CONTRACT"]
Source: "Build the tenant API with JWT authentication and OpenAPI contract."

---

## O1-router-006-003 — SECURITY+AUTHORIZATION+TEST → 3.4+5.1 (two routing decisions)

Predicates: ["SECURITY", "AUTHORIZATION", "TEST"] | Routes to: 3.4, 5.1
Routing decisions: 2
Decision 1: SECURITY+AUTHORIZATION → 3.4
Decision 2: TEST → 5.1
Source: "Build tenant-aware JWT middleware and write unit tests for permission boundary assertions."

---

## O1-router-006-004 — SECURITY+API_CONTRACT+TEST → 3.4+3.3+5.1 (three routing decisions)

Predicates: ["SECURITY", "API_CONTRACT", "TEST"] | Routes to: 3.4, 3.3, 5.1
Routing decisions: 3
Decision 1: SECURITY → 3.4
Decision 2: API_CONTRACT → 3.3
Decision 3: TEST → 5.1
Source: "Implement the token validation endpoint and write unit tests for the schema."

---

## O1-router-006-005 — API_CONTRACT+AUTHORIZATION+TEST → 3.4+3.3+5.1 (three routing decisions)

Predicates: ["API_CONTRACT", "AUTHORIZATION", "TEST"] | Routes to: 3.3, 3.4, 5.1
Routing decisions: 3
Decision 1: API_CONTRACT → 3.3 | EvidencePredicates: ["API_CONTRACT"]
Decision 2: AUTHORIZATION → 3.4 | EvidencePredicates: ["AUTHORIZATION"]
Decision 3: TEST → 5.1 | EvidencePredicates: ["TEST"]
Source: "Design the permission management API and assert all role boundary conditions."
Ordering: canonical order maintained — API_CONTRACT before AUTHORIZATION before TEST
