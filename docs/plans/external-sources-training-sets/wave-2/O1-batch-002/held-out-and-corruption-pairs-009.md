# Held-Out and Corruption Pairs (009)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## O1b-HO-009-001 — Held-out: "write API contract for the authorization server"

A: "write the API contract for the authorization server including token, introspect, and register endpoints"

B: Predicates: ["DOCUMENT", "SECURITY", "API_CONTRACT"] | TransitionType: documentation
   Routing: [{ sub_phase: "3.4" }, { sub_phase: "3.3" }, { sub_phase: "2.1" }]

---

## O1b-HO-009-002 — Held-out: "openapi spec for the auth service"

A: "write the OpenAPI spec for the OAuth 2.0 authorization service"

B: Predicates: ["API_CONTRACT", "SECURITY"] | TransitionType: documentation
   Routing: [{ sub_phase: "3.4" }, { sub_phase: "3.3" }]

---

## O1b-HO-009-003 — Held-out: "grpc contract for auth service"

A: "define the gRPC (Google Remote Procedure Call) contract for the internal authorization service"

B: Predicates: ["API_CONTRACT", "AUTHORIZATION"] | TransitionType: documentation
   Routing: [{ sub_phase: "3.4" }, { sub_phase: "3.3" }]
   Reasoning: "grpc" fires API_CONTRACT; "authorization" fires AUTHORIZATION → 3.4.

---

## O1b-COR-009-001 — Corruption v6-02: "scope" not caught in API contract

A: "define the OAuth scope definitions in the OpenAPI spec for the tenant API"

CORRUPTED B (v6-02): Predicates: ["API_CONTRACT", "SECURITY"] | TransitionType: documentation
   ERROR: "scope" fires AUTHORIZATION (suppressed by v6-02). "tenant" fires AUTHORIZATION (suppressed). Missing AUTHORIZATION predicate.

CORRECT B: Predicates: ["API_CONTRACT", "SECURITY", "AUTHORIZATION"] | TransitionType: documentation
   Routing: [{ sub_phase: "3.4" }, { sub_phase: "3.3" }]

---

## O1b-COR-009-002 — Corruption v6-02: "tenant" not caught in contract spec

A: "write the API specification for the tenant authorization model"

CORRUPTED B (v6-02): Predicates: ["API_CONTRACT"] | TransitionType: documentation
   ERROR: "tenant" fires AUTHORIZATION (suppressed). "authorization" fires AUTHORIZATION (suppressed). Missing AUTHORIZATION → missing 3.4 routing.

CORRECT B: Predicates: ["API_CONTRACT", "AUTHORIZATION", "DOCUMENT"] | TransitionType: documentation
   Routing: [{ sub_phase: "3.4" }, { sub_phase: "3.3" }, { sub_phase: "2.1" }]
