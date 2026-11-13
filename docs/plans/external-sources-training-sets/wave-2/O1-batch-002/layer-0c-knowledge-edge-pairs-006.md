# Layer 0-C — Knowledge Edge Pairs (006)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## O1b-0C-006-001 — "can't get past the login screen" → knowledge_edge

A: "can't get past the login screen since the last update"

B: Result: knowledge_edge | Predicates: []
   Reasoning: BOUNDARY RULE — "login" is explicitly listed as NOT firing SECURITY. The predicate lexicon states: "login" → no predicate (NOT SECURITY). Therefore zero predicates → knowledge_edge.

---

## O1b-0C-006-002 — "login button broken" → knowledge_edge

A: "the login button is broken on the mobile app"

B: Result: knowledge_edge | Predicates: []
   Reasoning: "login" → no predicate (boundary rule). "button broken" is UI language. Zero predicates.

---

## O1b-0C-006-003 — "login page not loading" → knowledge_edge

A: "the login page is not loading for enterprise customers"

B: Result: knowledge_edge | Predicates: []
   Reasoning: "login" → no predicate. "page not loading" is operational. Zero predicates.

---

## O1b-0C-006-004 — "login flow vs auth flow" — the distinction

A: "the login flow is broken" vs "the auth flow is broken"

B: "login flow is broken" → knowledge_edge (Predicates: []) — "login" explicitly excluded from SECURITY.
   "auth flow is broken" → VALID ROUTING (Predicates: ["SECURITY"]) — "auth" fires SECURITY.
   Key distinction: "login" ≠ "auth". "auth" is in the SECURITY lexicon; "login" is not.

---

## O1b-0C-006-005 — "login" adjacent terms that DO fire SECURITY

A: What terms related to login DO fire SECURITY?

B: These terms DO fire SECURITY: "authentication" (auth), "credential", "password", "session", "token", "oauth", "oidc". These do NOT fire SECURITY: "login", "sign in", "sign up", "logout", "logged out". The boundary is technical protocol terms vs. colloquial UI actions.
