# Held-Out and Corruption Pairs (008)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## O1b-HO-008-001 — Held-out: "per-tenant rate limiting with OAuth scopes"

A: "add per-tenant rate limiting with OAuth scope-based quotas"

B: Predicates: ["SECURITY", "AUTHORIZATION", "IMPLEMENT"] | TransitionType: feature_addition
   Routing: [{ sub_phase: "3.4" }, { sub_phase: "4.1" }]

---

## O1b-HO-008-002 — Held-out: "scope-based throttling"

A: "implement scope-based throttling where admin:write scope gets higher rate limits"

B: Predicates: ["IMPLEMENT", "AUTHORIZATION"] | TransitionType: feature_addition
   Routing: [{ sub_phase: "3.4" }, { sub_phase: "4.1" }]

---

## O1b-HO-008-003 — Held-out: "tenant quota management API"

A: "build the tenant quota management API with scope-based limits"

B: Predicates: ["IMPLEMENT", "AUTHORIZATION", "API_CONTRACT"] | TransitionType: feature_addition
   Routing: [{ sub_phase: "3.4" }, { sub_phase: "4.1" }, { sub_phase: "3.3" }]

---

## O1b-COR-008-001 — Corruption v6-01: invents TENANT_SCOPE predicate

A: "add per-tenant rate limiting with OAuth scope enforcement"

CORRUPTED B (v6-01): Predicates: ["TENANT_SCOPE", "SECURITY"] | TransitionType: feature_addition
   ERROR: "TENANT_SCOPE" invented. "tenant" fires AUTHORIZATION; "scope" fires AUTHORIZATION.

CORRECT B: Predicates: ["SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition

---

## O1b-COR-008-002 — Corruption v6-01: invents RATE_LIMIT predicate

A: "configure OAuth-based rate limiting per tenant scope"

CORRUPTED B (v6-01): Predicates: ["SECURITY", "AUTHORIZATION", "RATE_LIMIT"] | TransitionType: feature_addition
   ERROR: "RATE_LIMIT" invented. Rate limiting is not a predicate in the O1b lexicon. The correct predicates are SECURITY (oauth) and AUTHORIZATION (tenant, scope).

CORRECT B: Predicates: ["SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition
