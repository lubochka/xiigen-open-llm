# Layer 0-B — Router Pairs (011)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: router

---

## O1-router-011-001 — v6-03 corruption: "test isolation" routed to 3.4 (wrong)

Source: "Verify isolation between tenant auth contexts in the test suite."
Predicates: ["SECURITY", "AUTHORIZATION", "TEST"] (correct)

v6-03 corrupt routing:
  Wrong: TEST → 3.4 (corrupt model treats "isolation" as SECURITY → routes TEST to 3.4)
  Wrong RoutingDecisions: [3.4 (SECURITY+AUTHORIZATION+TEST collapsed), nothing for 5.1]

Correct routing:
  Decision 1: SECURITY+AUTHORIZATION → 3.4
  Decision 2: TEST → 5.1
Routing decisions: 2 (correct) vs 1 (v6-03 corrupt)
Error: v6-03 suppresses the TEST → 5.1 routing decision and folds TEST into the 3.4 decision

---

## O1-router-011-002 — v6-03 corruption: SECURITY+AUTHORIZATION collapse to wrong sub-phase

Source: "Implement tenant role enforcement for JWT-validated API calls."
Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION", "API_CONTRACT"] (correct)

v6-03 corrupt routing:
  Wrong: Produces TWO separate 3.4 decisions — one for SECURITY alone, one for AUTHORIZATION alone
  Wrong RoutingDecisions: [4.1(IMPL), 3.3(API), 3.4(SECURITY), 3.4(AUTHORIZATION)] — 4 decisions with duplicate 3.4

Correct routing:
  Decision 1: IMPLEMENT → 4.1
  Decision 2: API_CONTRACT → 3.3
  Decision 3: SECURITY+AUTHORIZATION → 3.4 (single deduplicated decision)
Routing decisions: 3 (correct) vs 4 with duplicate (v6-03 corrupt)

---

## O1-router-011-003 — v6-03 corruption: QA not last

Source: "QA the security gates and implement JWT validation."
Predicates: ["QA", "IMPLEMENT", "SECURITY"] (correct)

v6-03 corrupt routing:
  Wrong: [7.x(QA), 4.1(IMPL), 3.4(SEC)] — QA placed first (in predicate discovery order)

Correct routing:
  Decision 1: IMPLEMENT → 4.1
  Decision 2: SECURITY → 3.4
  Decision 3: QA → 7.x (always last)
QA ordering rule: QA is always last in the RoutingDecision[] array, regardless of discovery order.

---

## O1-router-011-004 — v6-03 corruption: API_CONTRACT routes to 3.4 instead of 3.3

Source: "Add an API contract for the authentication service."
Predicates: ["API_CONTRACT", "SECURITY"] (correct)

v6-03 corrupt routing:
  Wrong: API_CONTRACT → 3.4 (corrupt model conflates API+SECURITY into one 3.4 decision)

Correct routing:
  Decision 1: API_CONTRACT → 3.3
  Decision 2: SECURITY → 3.4
Error: API_CONTRACT always routes to 3.3; it never routes to 3.4 regardless of co-present predicates

---

## O1-router-011-005 — v6-03 corruption: DOCUMENT routes to 3.4 instead of 2.1

Source: "Document the JWT security model for the tenant onboarding guide."
Predicates: ["DOCUMENT", "SECURITY", "AUTHORIZATION"] (correct)

v6-03 corrupt routing:
  Wrong: DOCUMENT → 3.4 (corrupt model folds DOCUMENT into SECURITY destination)

Correct routing:
  Decision 1: SECURITY+AUTHORIZATION → 3.4
  Decision 2: DOCUMENT → 2.1
Error: DOCUMENT always routes to 2.1; co-present SECURITY does not redirect DOCUMENT to 3.4
