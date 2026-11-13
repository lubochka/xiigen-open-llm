# Layer 0-C — Knowledge Edge Pairs (005)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## O1b-0C-005-001 — "users losing session" → SECURITY fires on "session"

A: "users are losing their session after 10 minutes of inactivity"

B: Result: VALID ROUTING | Predicates: ["SECURITY"]
   Reasoning: "session" is in the SECURITY lexicon. This is a technical description of session behavior. SECURITY fires → routes to 3.4. NOT knowledge_edge.

---

## O1b-0C-005-002 — "session keeps expiring" → SECURITY fires

A: "the user session keeps expiring too early"

B: Result: VALID ROUTING | Predicates: ["SECURITY"]
   Reasoning: "session" fires SECURITY. Valid routing to 3.4.

---

## O1b-0C-005-003 — "session timeout configuration" → SECURITY fires

A: "configure the session timeout for the tenant authentication flow"

B: Result: VALID ROUTING | Predicates: ["SECURITY", "AUTHORIZATION"]
   Reasoning: "session" fires SECURITY; "authentication" fires SECURITY; "tenant" fires AUTHORIZATION. Two predicates, both route to 3.4 (dedup).

---

## O1b-0C-005-004 — "users getting logged out" → knowledge_edge ("login" not in lexicon)

A: "users are getting logged out every hour"

B: Result: knowledge_edge | Predicates: []
   Reasoning: "logged out" is not in any predicate lexicon. "login"/"logout" are explicitly NOT in SECURITY. Compare with "session expiring" (SECURITY fires) — the word "session" is the key distinction.

---

## O1b-0C-005-005 — "session invalidated after password reset" → SECURITY fires

A: "the session is being invalidated after a password reset"

B: Result: VALID ROUTING | Predicates: ["SECURITY"]
   Reasoning: "session" fires SECURITY; "password" fires SECURITY. Two SECURITY signals, one 3.4 routing decision.
