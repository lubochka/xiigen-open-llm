# Held-Out and Corruption Pairs (003)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## O1b-HO-003-001 — Held-out: "add client certificate authentication"

A: "add client certificate authentication for machine-to-machine service communication"

B: Predicates: ["SECURITY", "IMPLEMENT"] | TransitionType: feature_addition
   Routing: [{ sub_phase: "3.4", state: "trusted_learned" }, { sub_phase: "4.1", state: "deterministic_generic" }]
   Reasoning: "certificate" fires SECURITY; "authentication" fires SECURITY; "add" fires IMPLEMENT.

---

## O1b-HO-003-002 — Held-out: "mutual TLS for services"

A: "configure mutual TLS (mTLS — mutual Transport Layer Security) between internal microservices"

B: Predicates: ["SECURITY"] | TransitionType: feature_addition
   Routing: [{ sub_phase: "3.4", state: "trusted_learned" }]
   Reasoning: "tls" fires SECURITY; "mutual" is not a predicate but the technical context is clear.

---

## O1b-HO-003-003 — Held-out: "PKI for service identity"

A: "design a PKI (Public Key Infrastructure) for service identity in the mesh layer"

B: Predicates: ["SECURITY"] | TransitionType: feature_addition
   Routing: [{ sub_phase: "3.4", state: "trusted_learned" }]
   Reasoning: "key" fires SECURITY; "certificate" implied by PKI. Routes to 3.4.

---

## O1b-COR-003-001 — Corruption v6-02: "client portal" → SECURITY (wrong)

A: "the client portal is timing out on the login page"

CORRUPTED B (v6-02): Predicates: ["SECURITY"] | TransitionType: feature_addition
   ERROR: "client portal" is colloquial; "login" is explicitly NOT in SECURITY lexicon. Zero predicates.

CORRECT B: Predicates: [] | TransitionType: knowledge_edge

---

## O1b-COR-003-002 — Corruption v6-02: "portal oauth issues" — correct detection

A: "the client portal has oauth issues affecting all tenants"

B: Predicates: ["SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition
   Note: This is CORRECT behavior — "oauth" fires SECURITY, "tenant" fires AUTHORIZATION. Despite "client portal" being present, the lexicon keywords override the colloquial phrase.
