# Layer 0-C — Knowledge-Edge Pairs (007)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## O1-kedge-007-001 — "the token is invalid" → SECURITY fires on "token" (not knowledge_edge)

A: "The token is invalid and the API keeps returning 401 errors."
B: KnowledgeEdge: false | Predicates: ["SECURITY", "API_CONTRACT"] | Action: route to 3.4 and 3.3

Explanation:
  "token" — IS in the SECURITY lexicon (explicit entry)
  "api" — fires API_CONTRACT
  "401" (HTTP status code) — not in any predicate lexicon by itself
  Result: SECURITY + API_CONTRACT fire → routes to 3.4 and 3.3 (2 routing decisions)
  NOT knowledge_edge

---

## O1-kedge-007-002 — "token expired" → SECURITY fires (not knowledge_edge)

A: "Users are getting 'token expired' errors in production."
B: KnowledgeEdge: false | Predicates: ["SECURITY"] | Action: route to 3.4

Explanation:
  "token" — fires SECURITY
  "expired", "production", "errors" — not in any predicate lexicon
  Result: SECURITY fires → routes to 3.4 → NOT knowledge_edge

---

## O1-kedge-007-003 — "JWT not accepted" → SECURITY fires (not knowledge_edge)

A: "JWT tokens from our old issuer are not being accepted by the new service."
B: KnowledgeEdge: false | Predicates: ["SECURITY"] | Action: route to 3.4

Explanation:
  "jwt" — fires SECURITY
  "token" — also fires SECURITY (redundant confirmation)
  Result: SECURITY fires → routes to 3.4

---

## O1-kedge-007-004 — Contrast: "the bearer is rejected" → knowledge_edge ("bearer" not in SECURITY lexicon)

A: "The bearer header is being rejected by the upstream proxy."
B: KnowledgeEdge: ambiguous_no_predicate | Action: route to knowledge_edge synthetic flow

Explanation:
  "bearer" — NOT in the SECURITY lexicon (even though bearer tokens are a security concept)
  Lexicon entries are: jwt, authentication, auth, oauth, oidc, token, session, asvs, mfa,
    rbac, security, secret, certificate, tls, harden, secure, encrypt, hash, credential,
    sanitize, validate (security context)
  "proxy", "rejected", "header", "upstream" — not in any predicate lexicon
  Result: zero predicates → knowledge_edge
  Teaching point: domain knowledge that "bearer" is related to JWT does not override lexicon matching.
    Only exact lexicon tokens fire predicates.
