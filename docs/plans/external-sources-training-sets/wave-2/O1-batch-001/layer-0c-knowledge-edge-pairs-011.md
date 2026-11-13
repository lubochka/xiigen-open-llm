# Layer 0-C — Knowledge-Edge Pairs (011)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## O1-kedge-011-001 — "login" vs "authentication" — the critical boundary (full contrast)

A: "The login page is broken."
B: KnowledgeEdge: true | Predicates: [] | Action: knowledge_edge synthetic flow

Explanation: "login" is not in the SECURITY lexicon. No predicate fires.

---

## O1-kedge-011-002 — "authentication" fires SECURITY (the correct opposite)

A: "The authentication system is broken."
B: KnowledgeEdge: false | Predicates: ["SECURITY"] | Action: route to 3.4

Explanation: "authentication" IS in the SECURITY lexicon. SECURITY fires. Routes to 3.4.

---

## O1-kedge-011-003 — Mixed prompt: "login" + "token" — token fires, login does not

A: "The login page is causing JWT token validation errors."
B: KnowledgeEdge: false | Predicates: ["SECURITY"] | Action: route to 3.4

Explanation:
  "login" → does NOT fire SECURITY
  "jwt" → fires SECURITY
  "token" → also fires SECURITY
  Result: SECURITY fires (from jwt/token); NOT knowledge_edge even though "login" is present
  Teaching point: one predicate-firing token in a prompt is sufficient to prevent knowledge_edge
    The "login" word is simply ignored (produces no predicate signal)

---

## O1-kedge-011-004 — "login" alone vs "auth" alone — comprehensive boundary summary

Reference pair — boundary summary for the login/auth distinction.

Triggers knowledge_edge (no SECURITY predicate):
  "login", "log in", "log-in", "login page", "login screen", "login button"
  "sign in", "sign-in", "sign on", "sign-on"
  "logout", "log out", "sign out"
  "forgot password", "password reset", "reset password"
  "user account", "my account", "account settings"

Triggers SECURITY predicate (NOT knowledge_edge):
  "auth", "authenticate", "authentication", "oauth", "oidc"
  "jwt", "token", "session", "credential"
  "mfa", "rbac" (security context), "asvs", "tls", "certificate"
  "harden", "secure", "security", "encrypt", "hash"
  "sanitize", "validate" (security context), "secret"

Rule: The predicate lexicon is defined by technical security vocabulary, not user experience
  (UX — User Experience) vocabulary. "Login" is a UX term. "Authenticate" is a technical term.
