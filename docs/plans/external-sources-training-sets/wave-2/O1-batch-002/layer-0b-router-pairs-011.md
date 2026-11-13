# Layer 0-B — Router Pairs (011)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: router

---

## O1b-0B-011-001 — v6-03: "service account" routed to 4.1 (wrong)

A: "create a service account for the billing microservice with API access"

CORRUPTED ROUTING (v6-03): RoutingDecisions: [{ sub_phase: "4.1" }]
   ERROR: "service account" is an AUTHORIZATION principal — it fires AUTHORIZATION → 3.4. Routing to 4.1 alone is incorrect.

CORRECT ROUTING: Predicates: ["AUTHORIZATION", "IMPLEMENT"] | RoutingDecisions: [{ sub_phase: "3.4", state: "trusted_learned" }, { sub_phase: "4.1", state: "deterministic_generic" }]

---

## O1b-0B-011-002 — v6-03: "scope" routed to wrong sub-phase

A: "add per-tenant scope validation to the API middleware"

CORRUPTED ROUTING (v6-03): RoutingDecisions: [{ sub_phase: "4.1" }]
   ERROR: "scope" fires AUTHORIZATION → 3.4. Routing to 4.1 alone misses the 3.4 requirement.

CORRECT ROUTING: Predicates: ["AUTHORIZATION", "IMPLEMENT"] | RoutingDecisions: [{ sub_phase: "3.4", state: "trusted_learned" }, { sub_phase: "4.1", state: "deterministic_generic" }]

---

## O1b-0B-011-003 — v6-03: SECURITY+AUTHORIZATION producing two 3.4 decisions (wrong)

A: Predicates: ["SECURITY", "AUTHORIZATION"]

CORRUPTED ROUTING (v6-03): RoutingDecisions: [{ sub_phase: "3.4", reason: "SECURITY" }, { sub_phase: "3.4", reason: "AUTHORIZATION" }]
   ERROR: Two separate 3.4 entries is wrong. Dedup rule requires exactly one 3.4 entry.

CORRECT ROUTING: RoutingDecisions: [{ sub_phase: "3.4", reason: "SECURITY+AUTHORIZATION dedup" }]

---

## O1b-0B-011-004 — v6-03: tenant scoping routed to 2.1 instead of 3.4

A: "define the tenant scope assignment policy"

CORRUPTED ROUTING (v6-03): RoutingDecisions: [{ sub_phase: "2.1" }]
   ERROR: "scope" fires AUTHORIZATION → 3.4. "tenant" fires AUTHORIZATION → 3.4. Policy definition may also fire DOCUMENT → 2.1, but 3.4 must be present.

CORRECT ROUTING: Predicates: ["AUTHORIZATION", "DOCUMENT"] | RoutingDecisions: [{ sub_phase: "3.4", state: "trusted_learned" }, { sub_phase: "2.1", state: "deterministic_generic" }]

---

## O1b-0B-011-005 — v6-03: correct routing of "service account" confirmed

A: "register a service account principal in the authorization model"

B: Predicates: ["AUTHORIZATION"] | RoutingDecisions: [{ sub_phase: "3.4", flow: "SecurityVerification34FlowAdapter", state: "trusted_learned" }]
   Confirmation: "service account" → AUTHORIZATION (principal/account with scope) → 3.4. This is the correct routing. Never route "service account" to 4.1 alone.
