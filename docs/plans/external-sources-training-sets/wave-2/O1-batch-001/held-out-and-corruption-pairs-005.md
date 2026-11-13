# Held-Out and Corruption Pairs (005)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: held_out_and_corruption

---

## Held-out pair 005-H001 — document the security handshake protocol

A: "Document the security handshake protocol for the TLS (Transport Layer Security) tenant connection setup."
Expected B: Predicates: ["DOCUMENT", "SECURITY"] | TransitionType: documentation
(not in training set — held out for evaluation)

Lexicon evidence (for evaluator reference only):
  DOCUMENT: "document"
  SECURITY: "security", "tls"
  TransitionType: documentation (DOCUMENT fires; no IMPLEMENT)

---

## Held-out pair 005-H002 — write spec for auth flow

A: "Write the specification for the OAuth2 (Open Authorization 2.0) authorization code flow with PKCE (Proof Key for Code Exchange)."
Expected B: Predicates: ["DOCUMENT", "SECURITY"] | TransitionType: documentation
(not in training set — held out for evaluation)

---

## Corruption 005-C001 — v6-01 invents TENANT_ISOLATION predicate

v6 corrupt: "Document the tenant data isolation policy."
  → Predicates: ["DOCUMENT", "TENANT_ISOLATION"] | TransitionType: documentation
Correct: Predicates: ["DOCUMENT", "AUTHORIZATION"] | TransitionType: documentation
  ("tenant" fires AUTHORIZATION; "document" fires DOCUMENT)
Error type: v6-01 (predicate invention)
TENANT_ISOLATION is not a valid predicate — the correct predicate for "tenant" is AUTHORIZATION

---

## Corruption 005-C002 — v6-01 invents SECURITY_SPEC predicate

v6 corrupt: "Write the security specification for the JWT auth design."
  → Predicates: ["SECURITY_SPEC", "JWT_DOC"] | TransitionType: documentation
Correct: Predicates: ["DOCUMENT", "SECURITY"] | TransitionType: documentation
Error type: v6-01 (predicate invention)
SECURITY_SPEC and JWT_DOC are invented predicates — correct predicates are DOCUMENT and SECURITY
