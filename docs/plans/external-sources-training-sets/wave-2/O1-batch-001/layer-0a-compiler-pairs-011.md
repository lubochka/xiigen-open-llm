# Layer 0-A — Compiler Pairs (011)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: compiler

---

## O1-compiler-011-001 — v6-02 gap suppression: "login" → no predicate (correct)

A: "The login page keeps showing an error for existing users."
B: Predicates: [] | TransitionType: none

v6-02 error explanation:
  A v6-02 corrupt model would treat "login" as firing SECURITY — this is WRONG.
  "login" is not in the SECURITY lexicon: jwt, authentication, auth, oauth, oidc, token,
    session, asvs, mfa, rbac, security, secret, certificate, tls, harden, secure, encrypt,
    hash, credential, sanitize, validate (security context)
  Correct result: no predicate fires → knowledge_edge
  The gap suppression error is CLAIMING a predicate where none exists

---

## O1-compiler-011-002 — v6-02 gap suppression: "login reset" → no predicate (correct)

A: "Users need a login reset workflow for forgotten passwords."
B: Predicates: [] | TransitionType: none

v6-02 error explanation:
  "login reset" — "login" not in SECURITY lexicon; "reset" not in SECURITY lexicon
  "passwords" — "credential" IS in the SECURITY lexicon; "password" alone is NOT
  Correct result: no predicate fires → knowledge_edge
  A v6-02 corrupt model wrongly fires SECURITY on "login reset" — this is the gap
  suppression error: the model inverts the error (suppresses the correct no-predicate
  result and fires a wrong predicate instead)

---

## O1-compiler-011-003 — v6-02 gap suppression: "tenant" not caught (wrong behavior)

A: "Each tenant gets their own workspace in the admin panel."
B: Predicates: ["AUTHORIZATION"] | TransitionType: feature_addition

v6-02 error explanation:
  "tenant" IS in the AUTHORIZATION lexicon — a v6-02 corrupt model suppresses this
  and returns no predicate. Correct result: AUTHORIZATION fires.
  The gap suppression error here is FAILING to catch a real predicate match.
  IMPLEMENT fires on "gets" (structural addition) — feature_addition TransitionType

---

## O1-compiler-011-004 — v6-02 gap suppression: "access denied" → no predicate (correct)

A: "Some users are getting access denied errors in production."
B: Predicates: [] | TransitionType: none

v6-02 error explanation:
  "access denied" is a runtime error message — it is colloquial, not a predicate match
  "access" alone does not appear in the AUTHORIZATION lexicon (requires "access control"
    as a compound term, or "multi-tenant", "tenant", "role", "permission", "scope", etc.)
  Correct result: no predicate fires → knowledge_edge
  v6-02 error: incorrectly firing AUTHORIZATION on "access" alone

---

## O1-compiler-011-005 — v6-02 gap suppression boundary: "access control" fires (correct)

A: "We need to design the access control policy for the admin dashboard."
B: Predicates: ["AUTHORIZATION"] | TransitionType: feature_addition

v6-02 gap boundary clarification:
  "access control" IS in the AUTHORIZATION lexicon (compound term, not just "access")
  Correct result: AUTHORIZATION fires
  Contrast with pair 011-004: "access denied" ≠ "access control"
  IMPLEMENT fires on "design" (structural creation) — feature_addition TransitionType
