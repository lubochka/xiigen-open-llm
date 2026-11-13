# Held-Out and Corruption Pairs (Base)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## HELD-OUT PAIRS

---

## O1b-HO-001 — Novel prompt: tenant-scoped client credentials

A: "implement tenant-scoped client credentials for the multi-tenant SaaS (Software as a Service) platform"

B: Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition
   Routing: [{ sub_phase: "3.4", state: "trusted_learned" }, { sub_phase: "4.1", state: "deterministic_generic" }]
   Reasoning: "implement" fires IMPLEMENT → 4.1; "client credentials" fires SECURITY; "tenant-scoped" fires AUTHORIZATION. SECURITY+AUTHORIZATION dedup to one 3.4 decision. IMPLEMENT adds 4.1. Result: 3.4+4.1 fan-out.

---

## O1b-HO-002 — Novel prompt: client certificate authentication

A: "add client certificate authentication for machine-to-machine service communication"

B: Predicates: ["SECURITY", "IMPLEMENT"] | TransitionType: feature_addition
   Routing: [{ sub_phase: "3.4", state: "trusted_learned" }, { sub_phase: "4.1", state: "deterministic_generic" }]
   Reasoning: "certificate" fires SECURITY (in lexicon); "authentication" fires SECURITY; "add" + "service" fires IMPLEMENT. Result: 3.4+4.1.

---

## O1b-HO-003 — Novel prompt: tenant API gateway with OAuth scopes

A: "configure the API gateway with per-tenant OAuth scope enforcement"

B: Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT"] | TransitionType: feature_addition
   Routing: [{ sub_phase: "3.4", state: "trusted_learned" }, { sub_phase: "3.3", state: "deterministic_generic" }]
   Reasoning: "OAuth" fires SECURITY; "scope enforcement" fires AUTHORIZATION; "API gateway" + "API" fires API_CONTRACT. No IMPLEMENT verb → feature_addition still applies because SECURITY+AUTHORIZATION+API_CONTRACT are present. Result: 3.4+3.3.

---

## CORRUPTION PAIRS

---

## O1b-COR-001 — v6-01: predicate invention (inventing OAUTH_FLOW predicate)

A: "implement OAuth2 client credentials for the service layer"

CORRUPTED B (v6-01): Predicates: ["IMPLEMENT", "OAUTH_FLOW"] | TransitionType: feature_addition
   ERROR: "OAUTH_FLOW" is an invented predicate that does not exist in the predicate lexicon. The correct predicate for "oauth" is SECURITY (it appears explicitly in the SECURITY lexicon list).

CORRECT B: Predicates: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition

---

## O1b-COR-002 — v6-02: gap suppression ("client portal" → SECURITY — wrong)

A: "the client portal is timing out on the login page"

CORRUPTED B (v6-02): Predicates: ["SECURITY"] | TransitionType: feature_addition
   ERROR: "client portal" is colloquial. "login" is explicitly NOT in the SECURITY lexicon (boundary rule: "login" → no predicate). Neither "client portal" nor "timing out" trigger any predicate. This prompt has zero predicates.

CORRECT B: Predicates: [] | TransitionType: knowledge_edge

---

## O1b-COR-003 — v6-03: wrong routing ("service account" routed to 4.1 instead of 3.4)

A: "create a service account with scoped API access for the billing microservice"

CORRUPTED B (v6-03): Predicates: ["SECURITY", "AUTHORIZATION"] | RoutingDecisions: [{ sub_phase: "4.1" }, { sub_phase: "3.4" }]
   ERROR: "service account" is an AUTHORIZATION principal — it routes to 3.4 (Security and Authorization Model), not 4.1 (Implementation). Also, SECURITY+AUTHORIZATION dedup means ONE routing decision to 3.4, not two separate decisions.

CORRECT B: Predicates: ["SECURITY", "AUTHORIZATION", "IMPLEMENT"] | RoutingDecisions: [{ sub_phase: "3.4", state: "trusted_learned" }, { sub_phase: "4.1", state: "deterministic_generic" }]
   Reasoning: "service account" → AUTHORIZATION → 3.4; "scoped" → AUTHORIZATION (confirms 3.4, deduped); "create" → IMPLEMENT → 4.1. Two routing decisions: 3.4 (from SECURITY+AUTH dedup) and 4.1 (from IMPLEMENT).
