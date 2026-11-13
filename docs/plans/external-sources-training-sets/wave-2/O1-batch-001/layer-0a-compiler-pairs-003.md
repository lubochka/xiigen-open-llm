# Layer 0-A — Compiler Pairs (003)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: compiler

---

## O1-compiler-003-001 — API_CONTRACT fires on "api"

A: "Define the API contract for the authentication service endpoints."
B: Predicates: ["API_CONTRACT"] | TransitionType: feature_addition

Lexicon evidence:
  API_CONTRACT matched on: "api", "contract", "endpoints"
  IMPLEMENT fires on "define" (structural creation)
  Note: "authentication" also present — SECURITY fires
  Correction — revised: Predicates: ["SECURITY", "API_CONTRACT"]
  SECURITY matched on: "authentication"

---

## O1-compiler-003-002 — API_CONTRACT fires on "endpoint schema"

A: "Specify the request and response schema for the token exchange endpoint."
B: Predicates: ["SECURITY", "API_CONTRACT"] | TransitionType: documentation

Lexicon evidence:
  API_CONTRACT matched on: "request", "response", "schema", "endpoint"
  SECURITY matched on: "token"
  TransitionType: documentation — "specify" without "build/implement/create"
  DOCUMENT fires on "specify" only in DOCUMENT context; here API_CONTRACT dominates

---

## O1-compiler-003-003 — "login" = no predicate (critical boundary)

A: "The login page keeps redirecting users after they submit the form."
B: Predicates: [] | TransitionType: none

Lexicon evidence:
  SECURITY: "login" is NOT in the SECURITY lexicon
  Lexicon items are: jwt, authentication, auth, oauth, oidc, token, session, asvs, mfa,
    rbac, security, secret, certificate, tls, harden, secure, encrypt, hash, credential,
    sanitize, validate (security context)
  "login" is colloquial for the UI interaction; it does not fire any predicate
  Result: knowledge_edge — no predicate, no routing decision

---

## O1-compiler-003-004 — API_CONTRACT fires on "openapi"

A: "Generate the OpenAPI specification for the tenant management REST endpoints."
B: Predicates: ["API_CONTRACT", "AUTHORIZATION"] | TransitionType: feature_addition

Lexicon evidence:
  API_CONTRACT matched on: "openapi", "rest", "endpoints"
  AUTHORIZATION matched on: "tenant"
  IMPLEMENT fires on "generate"

---

## O1-compiler-003-005 — "login" boundary vs "oauth" boundary

A: "Integrate the OAuth2 authorization server for the login workflow."
B: Predicates: ["SECURITY"] | TransitionType: feature_addition

Lexicon evidence:
  SECURITY matched on: "oauth"
  Note: "login" does NOT fire SECURITY — "oauth" is the matching token
  "authorization server" phrase — AUTHORIZATION does not fire on "authorization" in
    this context because it appears as part of "OAuth2 authorization server" (technical
    protocol term); the firing predicate is SECURITY via "oauth"
  Boundary rule: "login" alone = no predicate; "oauth" alone = SECURITY
