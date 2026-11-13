# Held-Out and Corruption Pairs (011)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## O1b-HO-011-001 — Held-out: "validate client credentials against the tenant registry"

A: "validate client credentials against the tenant registry before issuing tokens"

B: Predicates: ["SECURITY", "AUTHORIZATION", "IMPLEMENT"] | TransitionType: feature_addition
   Routing: [{ sub_phase: "3.4" }, { sub_phase: "4.1" }]

---

## O1b-HO-011-002 — Held-out: "check client scope against tenant policy"

A: "check that the requested client scope is within the tenant's policy before token issuance"

B: Predicates: ["SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition
   Routing: [{ sub_phase: "3.4" }]

---

## O1b-HO-011-003 — Held-out: "service account credential validation"

A: "add service account credential validation to the token issuance flow"

B: Predicates: ["SECURITY", "AUTHORIZATION", "IMPLEMENT"] | TransitionType: feature_addition
   Routing: [{ sub_phase: "3.4" }, { sub_phase: "4.1" }]
   Reasoning: "credential" fires SECURITY; "service account" fires AUTHORIZATION; "add" + "flow" fires IMPLEMENT.

---

## O1b-COR-011-001 — Corruption v6-01: invents CLIENT_AUTH predicate

A: "validate client authentication against the authorization server"

CORRUPTED B (v6-01): Predicates: ["CLIENT_AUTH", "AUTHORIZATION"] | TransitionType: feature_addition
   ERROR: "CLIENT_AUTH" invented. "client authentication" = "authentication" → SECURITY + "authorization" → AUTHORIZATION.

CORRECT B: Predicates: ["SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition

---

## O1b-COR-011-002 — Corruption v6-01: invents SERVICE_ACCOUNT_VALIDATOR predicate

A: "build the service account validator for the tenant authorization system"

CORRUPTED B (v6-01): Predicates: ["IMPLEMENT", "SERVICE_ACCOUNT_VALIDATOR"] | TransitionType: feature_addition
   ERROR: "SERVICE_ACCOUNT_VALIDATOR" invented. "service account" fires AUTHORIZATION; "authorization" fires AUTHORIZATION; "build" fires IMPLEMENT.

CORRECT B: Predicates: ["IMPLEMENT", "AUTHORIZATION"] | TransitionType: feature_addition
