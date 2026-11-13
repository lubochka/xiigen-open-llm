# Held-Out and Corruption Pairs (005)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## O1b-HO-005-001 — Held-out: "document the client registration API"

A: "document the client registration API for third-party integration partners"

B: Predicates: ["DOCUMENT", "SECURITY", "API_CONTRACT"] | TransitionType: documentation
   Routing: [{ sub_phase: "3.4" }, { sub_phase: "3.3" }, { sub_phase: "2.1" }]

---

## O1b-HO-005-002 — Held-out: "write a spec for the grant endpoint"

A: "write the functional specification for the scope grant endpoint"

B: Predicates: ["DOCUMENT", "AUTHORIZATION", "API_CONTRACT"] | TransitionType: documentation
   Routing: [{ sub_phase: "3.4" }, { sub_phase: "3.3" }, { sub_phase: "2.1" }]

---

## O1b-HO-005-003 — Held-out: "describe the token introspection behavior"

A: "describe the token introspection endpoint behavior for the developer guide"

B: Predicates: ["DOCUMENT", "SECURITY", "API_CONTRACT"] | TransitionType: documentation
   Routing: [{ sub_phase: "3.4" }, { sub_phase: "3.3" }, { sub_phase: "2.1" }]

---

## O1b-COR-005-001 — Corruption v6-01: invents CLIENT_CREDS predicate

A: "set up client credentials grant for the CI/CD (Continuous Integration / Continuous Delivery) pipeline"

CORRUPTED B (v6-01): Predicates: ["CLIENT_CREDS", "IMPLEMENT"] | TransitionType: feature_addition
   ERROR: "CLIENT_CREDS" does not exist. "client credentials" fires SECURITY (credential is in SECURITY lexicon; oauth context).

CORRECT B: Predicates: ["SECURITY", "IMPLEMENT"] | TransitionType: feature_addition

---

## O1b-COR-005-002 — Corruption v6-01: invents OAUTH_GRANT predicate

A: "configure the OAuth grant flow for service-to-service calls"

CORRUPTED B (v6-01): Predicates: ["OAUTH_GRANT", "SECURITY"] | TransitionType: feature_addition
   ERROR: "OAUTH_GRANT" invented. "oauth" fires SECURITY; "grant" fires AUTHORIZATION. No OAUTH_GRANT predicate.

CORRECT B: Predicates: ["SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition
