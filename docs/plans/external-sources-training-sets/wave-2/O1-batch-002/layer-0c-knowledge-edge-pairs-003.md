# Layer 0-C — Knowledge Edge Pairs (003)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## O1b-0C-003-001 — "oauth is not working" → SECURITY fires (NOT knowledge_edge)

A: "oauth is not working for our backend service"

B: Result: VALID ROUTING | Predicates: ["SECURITY"]
   Reasoning: "oauth" is explicitly in the SECURITY lexicon. Even though "is not working" is colloquial, the lexicon keyword "oauth" fires SECURITY. This IS routable to 3.4. NOT knowledge_edge.

---

## O1b-0C-003-002 — "oauth keeps failing" → SECURITY fires

A: "oauth keeps failing with a 500 error"

B: Result: VALID ROUTING | Predicates: ["SECURITY"]
   Reasoning: "oauth" fires SECURITY. The error description ("500 error") is incidental. One predicate found → valid routing to 3.4.

---

## O1b-0C-003-003 — "oauth is broken in prod" → SECURITY fires

A: "oauth is broken in production after the last deploy"

B: Result: VALID ROUTING | Predicates: ["SECURITY"]
   Reasoning: "oauth" fires SECURITY. "broken", "production", "deploy" are not lexicon words but they don't negate the SECURITY predicate. Valid routing.

---

## O1b-0C-003-004 — "oauth2 redirect not working" → SECURITY fires

A: "the oauth2 redirect URL is not working for the client app"

B: Result: VALID ROUTING | Predicates: ["SECURITY"]
   Reasoning: "oauth2" fires SECURITY (oauth prefix match). Valid routing to 3.4.

---

## O1b-0C-003-005 — "oidc integration failing" → SECURITY fires

A: "the oidc (OpenID Connect) integration is failing to return user claims"

B: Result: VALID ROUTING | Predicates: ["SECURITY"]
   Reasoning: "oidc" is in the SECURITY lexicon. Even a failure complaint fires SECURITY when a lexicon word is present.
