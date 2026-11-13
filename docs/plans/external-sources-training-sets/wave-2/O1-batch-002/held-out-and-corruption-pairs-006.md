# Held-Out and Corruption Pairs (006)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## O1b-HO-006-001 — Held-out: "build API gateway with tenant isolation"

A: "build the API gateway with per-tenant OAuth scope enforcement and rate limiting"

B: Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION", "API_CONTRACT"] | TransitionType: feature_addition
   Routing: [{ sub_phase: "3.4" }, { sub_phase: "4.1" }, { sub_phase: "3.3" }]

---

## O1b-HO-006-002 — Held-out: "gateway with JWT validation"

A: "configure the API gateway to validate JWT tokens and enforce tenant permissions"

B: Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT"] | TransitionType: feature_addition
   Routing: [{ sub_phase: "3.4" }, { sub_phase: "3.3" }]

---

## O1b-HO-006-003 — Held-out: "tenant-aware routing in the gateway"

A: "add tenant-aware routing to the gateway using OAuth scope claims"

B: Predicates: ["AUTHORIZATION", "SECURITY", "API_CONTRACT"] | TransitionType: feature_addition
   Routing: [{ sub_phase: "3.4" }, { sub_phase: "3.3" }]

---

## O1b-COR-006-001 — Corruption v6-02: AUTHORIZATION suppressed in gateway prompt

A: "add per-tenant rate limiting with OAuth scopes to the gateway"

CORRUPTED B (v6-02): Predicates: ["SECURITY"] | TransitionType: feature_addition
   ERROR: "tenant" fires AUTHORIZATION; "scope" fires AUTHORIZATION. Both suppressed. Only SECURITY detected. Routing misses 3.4 from AUTHORIZATION (though same sub-phase — but the model should still detect AUTHORIZATION as a predicate).

CORRECT B: Predicates: ["SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition

---

## O1b-COR-006-002 — Corruption v6-02: "scope" suppressed in API gateway context

A: "enforce scope-based access control at the API gateway"

CORRUPTED B (v6-02): Predicates: [] | TransitionType: knowledge_edge
   ERROR: "scope" fires AUTHORIZATION; "access control" fires AUTHORIZATION (access-control in lexicon). Both suppressed by v6-02. This is a clear lexicon match, not a colloquial phrase.

CORRECT B: Predicates: ["AUTHORIZATION", "API_CONTRACT"] | TransitionType: feature_addition
