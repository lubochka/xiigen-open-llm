# Layer 0-C — Knowledge Edge Pairs (007)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## O1b-0C-007-001 — "token expired" → SECURITY fires on "token"

A: "the token expired and the service can't refresh it"

B: Result: VALID ROUTING | Predicates: ["SECURITY"]
   Reasoning: "token" is in the SECURITY lexicon. Fires SECURITY → routes to 3.4. NOT knowledge_edge.

---

## O1b-0C-007-002 — "access token invalid" → SECURITY fires

A: "the access token is invalid after the service restart"

B: Result: VALID ROUTING | Predicates: ["SECURITY"]
   Reasoning: "token" fires SECURITY. "access token" is a technical term, not colloquial. Valid routing.

---

## O1b-0C-007-003 — "token refresh failing" → SECURITY fires

A: "the token refresh endpoint is failing for tenant B"

B: Result: VALID ROUTING | Predicates: ["SECURITY", "AUTHORIZATION"]
   Reasoning: "token" fires SECURITY; "tenant" fires AUTHORIZATION. Both route to 3.4 (dedup to one decision).

---

## O1b-0C-007-004 — "JWT expired" → SECURITY fires

A: "JWT (JSON Web Token) is expired on the introspection call"

B: Result: VALID ROUTING | Predicates: ["SECURITY"]
   Reasoning: "jwt" fires SECURITY; "token" (implied in JWT) also fires. Valid routing to 3.4.

---

## O1b-0C-007-005 — "token" as knowledge_edge boundary (rare)

A: "the train token system is down" (unrelated domain usage)

B: Result: knowledge_edge | Predicates: []
   Reasoning: BOUNDARY NOTE — In practice the predicate compiler uses full-phrase context. "train token system" in a non-software context does not fire SECURITY in the O1b scenario. Context matters when "token" appears in clearly non-software contexts. In all software/API contexts, "token" fires SECURITY.
