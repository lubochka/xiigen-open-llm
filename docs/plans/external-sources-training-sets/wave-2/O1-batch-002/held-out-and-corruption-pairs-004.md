# Held-Out and Corruption Pairs (004)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## O1b-HO-004-001 — Held-out: "write tests for scope enforcement"

A: "write tests for the tenant scope enforcement middleware"

B: Predicates: ["TEST", "AUTHORIZATION"] | TransitionType: documentation
   Routing: [{ sub_phase: "3.4", state: "trusted_learned" }, { sub_phase: "5.1", state: "probationary_rejected", fallback: true }]

---

## O1b-HO-004-002 — Held-out: "test the oauth token endpoint"

A: "write unit tests for the OAuth 2.0 token endpoint"

B: Predicates: ["TEST", "SECURITY"] | TransitionType: documentation
   Routing: [{ sub_phase: "3.4", state: "trusted_learned" }, { sub_phase: "5.1", state: "probationary_rejected", fallback: true }]

---

## O1b-HO-004-003 — Held-out: "integration test the client credentials flow"

A: "add integration tests covering the full client credentials grant flow"

B: Predicates: ["TEST", "SECURITY"] | TransitionType: documentation
   Routing: [{ sub_phase: "3.4" }, { sub_phase: "5.1", fallback: true }]

---

## O1b-COR-004-001 — Corruption v6-03: "scope" routed to wrong sub-phase

A: "add per-tenant scope validation to the API middleware"

CORRUPTED B (v6-03): Predicates: ["IMPLEMENT"] | RoutingDecisions: [{ sub_phase: "4.1" }]
   ERROR: "scope" fires AUTHORIZATION → 3.4. "tenant" fires AUTHORIZATION → 3.4. "API middleware" fires API_CONTRACT → 3.3 (optional). Missing all non-IMPLEMENT predicates.

CORRECT B: Predicates: ["IMPLEMENT", "AUTHORIZATION", "API_CONTRACT"] | RoutingDecisions: [{ sub_phase: "3.4" }, { sub_phase: "4.1" }, { sub_phase: "3.3" }]

---

## O1b-COR-004-002 — Corruption v6-03: "scope mismatch" routed to generic (wrong)

A: "handle scope mismatch errors in the authorization middleware"

CORRUPTED B (v6-03): RoutingDecisions: [{ sub_phase: "4.1" }]
   ERROR: "scope" fires AUTHORIZATION → 3.4; "authorization" fires AUTHORIZATION → 3.4 (dedup). Routes to 3.4, not 4.1.

CORRECT B: Predicates: ["AUTHORIZATION", "IMPLEMENT"] | RoutingDecisions: [{ sub_phase: "3.4" }, { sub_phase: "4.1" }]
