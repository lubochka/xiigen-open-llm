# Layer 0-C — Knowledge Edge Pairs (009)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## O1b-0C-009-001 — "scope mismatch error" → AUTHORIZATION fires on "scope"

A: "getting a scope mismatch error on the token validation call"

B: Result: VALID ROUTING | Predicates: ["SECURITY", "AUTHORIZATION"]
   Reasoning: "scope" fires AUTHORIZATION; "token" fires SECURITY. Both route to 3.4 (dedup). NOT knowledge_edge — "scope" is in the AUTHORIZATION lexicon.

---

## O1b-0C-009-002 — "scope" in technical context

A: "the requested scope exceeds the tenant's allowed scope set"

B: Result: VALID ROUTING | Predicates: ["AUTHORIZATION"]
   Reasoning: "scope" fires AUTHORIZATION; "tenant" fires AUTHORIZATION. Valid routing to 3.4.

---

## O1b-0C-009-003 — "scope" as knowledge_edge (rare, non-technical)

A: "the scope of the project has changed"

B: Result: knowledge_edge | Predicates: []
   Reasoning: BOUNDARY NOTE — "scope" in a project management context (project scope) is colloquial. In the O1b scenario, the predicate compiler uses technical context to disambiguate. "scope of the project" is not a security/authorization technical statement. Zero predicates in this project-management usage.

---

## O1b-0C-009-004 — "scope" in API context → AUTHORIZATION fires

A: "add the read:reports scope to the service account's permission set"

B: Result: VALID ROUTING | Predicates: ["AUTHORIZATION"]
   Reasoning: "scope" fires AUTHORIZATION; "permission" fires AUTHORIZATION; "service account" fires AUTHORIZATION (principal). Multiple AUTHORIZATION signals, one 3.4 routing decision.

---

## O1b-0C-009-005 — "scope" summary

A: When does "scope" fire AUTHORIZATION vs. knowledge_edge?

B: FIRES AUTHORIZATION: "OAuth scope", "API scope", "scope enforcement", "scope mismatch", "scope claims", "assign scope", "scope validation", "tenant scope". DOES NOT FIRE (knowledge_edge): "scope of the project", "scope of work", "scope creep" (project management contexts). Rule: in software/API/OAuth technical contexts, "scope" always fires AUTHORIZATION.
