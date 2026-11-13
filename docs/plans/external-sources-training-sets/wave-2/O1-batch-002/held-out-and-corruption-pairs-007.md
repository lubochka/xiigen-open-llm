# Held-Out and Corruption Pairs (007)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## O1b-HO-007-001 — Held-out: "secure the client secret rotation endpoint"

A: "secure the client secret rotation endpoint with admin-only scope"

B: Predicates: ["SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition
   Routing: [{ sub_phase: "3.4", state: "trusted_learned" }]
   Reasoning: "secret" fires SECURITY; "rotation" in security context fires SECURITY; "scope" fires AUTHORIZATION. Dedup: one 3.4 decision.

---

## O1b-HO-007-002 — Held-out: "rotate client secrets on schedule"

A: "implement automated client secret rotation every 90 days"

B: Predicates: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition
   Routing: [{ sub_phase: "3.4" }, { sub_phase: "4.1" }]

---

## O1b-HO-007-003 — Held-out: "key rotation for multi-tenant JWT"

A: "implement key rotation for the multi-tenant JWT signing infrastructure"

B: Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition
   Routing: [{ sub_phase: "3.4" }, { sub_phase: "4.1" }]
   Reasoning: "key" fires SECURITY; "jwt" fires SECURITY; "rotation" in security context; "multi-tenant" fires AUTHORIZATION. Dedup: one 3.4.

---

## O1b-COR-007-001 — Corruption v6-03: SECURITY+AUTH → two 3.4 decisions (wrong)

A: Predicates: ["SECURITY", "AUTHORIZATION"]

CORRUPTED B (v6-03): RoutingDecisions: [{ sub_phase: "3.4", reason: "SECURITY" }, { sub_phase: "3.4", reason: "AUTHORIZATION" }]
   ERROR: Two 3.4 decisions produced. Dedup rule violated.

CORRECT B: RoutingDecisions: [{ sub_phase: "3.4", reason: "SECURITY+AUTHORIZATION dedup" }]

---

## O1b-COR-007-002 — Corruption v6-03: SECURITY+AUTH → wrong sub-phase split

A: "validate JWT tokens and enforce tenant authorization scopes"

CORRUPTED B (v6-03): RoutingDecisions: [{ sub_phase: "3.4", reason: "SECURITY" }, { sub_phase: "2.1", reason: "AUTHORIZATION" }]
   ERROR: AUTHORIZATION routes to 3.4, not 2.1. 2.1 is for DOCUMENT. The model incorrectly routed AUTHORIZATION to the documentation sub-phase.

CORRECT B: RoutingDecisions: [{ sub_phase: "3.4", reason: "SECURITY+AUTHORIZATION dedup" }]
