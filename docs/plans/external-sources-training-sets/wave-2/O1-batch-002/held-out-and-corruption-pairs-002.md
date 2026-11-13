# Held-Out and Corruption Pairs (002)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## O1b-HO-002-001 — Held-out: "implement tenant-scoped client credentials"

A: "implement tenant-scoped client credentials for the multi-tenant SaaS (Software as a Service) platform"

B: Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition
   Routing: [{ sub_phase: "3.4", state: "trusted_learned" }, { sub_phase: "4.1", state: "deterministic_generic" }]

---

## O1b-HO-002-002 — Held-out: "set up OAuth2 for the gateway"

A: "set up OAuth2 client credentials authentication for the API gateway layer"

B: Predicates: ["SECURITY", "API_CONTRACT", "IMPLEMENT"] | TransitionType: feature_addition
   Routing: [{ sub_phase: "3.4" }, { sub_phase: "3.3" }, { sub_phase: "4.1" }]

---

## O1b-HO-002-003 — Held-out: "define tenant permission model"

A: "define the tenant permission model for the OAuth2 authorization server"

B: Predicates: ["AUTHORIZATION", "SECURITY", "DOCUMENT"] | TransitionType: documentation
   Routing: [{ sub_phase: "3.4" }, { sub_phase: "2.1" }]

---

## O1b-COR-002-001 — Corruption v6-01: invents OAUTH_FLOW predicate

A: "implement OAuth2 client credentials for the service layer"

CORRUPTED B (v6-01): Predicates: ["IMPLEMENT", "OAUTH_FLOW"] | TransitionType: feature_addition
   ERROR: "OAUTH_FLOW" invented. Correct predicate: SECURITY (for "oauth").

CORRECT B: Predicates: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition

---

## O1b-COR-002-002 — Corruption v6-01: invents CLIENT_AUTH predicate

A: "validate client authentication tokens before granting tenant access"

CORRUPTED B (v6-01): Predicates: ["SECURITY", "CLIENT_AUTH", "AUTHORIZATION"] | TransitionType: feature_addition
   ERROR: "CLIENT_AUTH" is invented. "client authentication" fires SECURITY (authentication) + AUTHORIZATION (tenant). No CLIENT_AUTH predicate exists.

CORRECT B: Predicates: ["SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition
