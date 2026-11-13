# Layer 0-A — Compiler Pairs (Base)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: compiler

---

## O1b-compiler-001 — Full scenario: OAuth2 client credentials with tenant scope

A: "Implement OAuth 2.0 client credentials flow with tenant-scoped API access and service account management."

B: Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "IMPLEMENT"] | TransitionType: feature_addition
   Reasoning: "oauth" fires SECURITY; "tenant-scoped" + "service account" fire AUTHORIZATION; "API access" fires API_CONTRACT; "implement" fires IMPLEMENT. TransitionType is feature_addition because IMPLEMENT is present and no "refactor" verb appears.

---

## O1b-compiler-002 — No-signal prompt: colloquial complaint

A: "the client portal is down"

B: Predicates: [] | TransitionType: knowledge_edge
   Reasoning: "client portal" is colloquial — no lexicon match. "down" is not in any predicate list. Zero predicates detected → knowledge_edge. This is NOT a SECURITY case; "client" alone does not fire any predicate.

---

## O1b-compiler-003 — Single domain: document OAuth2 scopes

A: "document the OAuth2 scopes for our API"

B: Predicates: ["DOCUMENT", "API_CONTRACT", "SECURITY"] | TransitionType: documentation
   Reasoning: "document" fires DOCUMENT; "API" fires API_CONTRACT; "OAuth2" fires SECURITY (oauth is in SECURITY lexicon). TransitionType is documentation because TEST/DOCUMENT present without IMPLEMENT.
