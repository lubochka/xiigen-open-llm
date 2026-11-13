# Layer 0-A — Compiler Pairs (019)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: compiler | focus: Okta identity connector

---

## B17-0A-019-001 — "build" + "OAuth" + "connector" → IMPLEMENT + API_CONTRACT + SECURITY + AUTHORIZATION

**Input:**
"Build the Okta identity connector, implement the OAuth 2.0 PKCE (Proof Key for Code Exchange) authorization flow, and validate the access token on each request."

**Compiler analysis:**
- "Build" → IMPLEMENT
- "implement" → IMPLEMENT (reinforces)
- "connector" → API_CONTRACT (Rule 4)
- "OAuth" → AUTHORIZATION (Rule 7)
- "token" → SECURITY (Rule 6: "access token" fires SECURITY)
- "validate" — validation of access token → SECURITY (Rule 8: validate signature/token fires SECURITY)
- Integration vocabulary: connector, OAuth, token → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
**SubPhase:** 4.3, 3.3, 3.4
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-019-002 — "integrate" Okta → API_CONTRACT only

**Input:**
"Integrate Okta as the identity provider for the enterprise customer portal."

**Compiler analysis:**
- "Integrate" → API_CONTRACT (Rule 1)
- No build verb → IMPLEMENT does NOT fire
- "identity provider" — no standalone predicate trigger

**CompiledPredicateIds:** ["API_CONTRACT"]
**SubPhase:** 3.3
**TransitionType:** documentation
**DomainId:** software-development

---

## B17-0A-019-003 — "create" + "token" + "authentication" → IMPLEMENT + API_CONTRACT + SECURITY

**Input:**
"Create the Okta OIDC (OpenID Connect) connector and implement the authentication token introspection endpoint."

**Compiler analysis:**
- "Create" → IMPLEMENT
- "implement" → IMPLEMENT (reinforces)
- "connector" → API_CONTRACT
- "OIDC" — acronym; not a standalone predicate trigger (OAuth fires AUTHORIZATION, not OIDC)
- "authentication" → SECURITY (Rule 8: authentication fires SECURITY)
- "token" → SECURITY (reinforces; fires once)
- "endpoint" → API_CONTRACT (reinforces)
- Note: "OIDC" does not fire AUTHORIZATION; only "OAuth" or "oauth" fires AUTHORIZATION (Rule 7)
- Integration vocabulary: connector, token, endpoint → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
**SubPhase:** 4.3, 3.3, 3.4
**TransitionType:** feature_addition
**DomainId:** software-development

**Teaching point:** "OIDC" (OpenID Connect) does NOT fire AUTHORIZATION. Only the word "OAuth" (in any case) fires AUTHORIZATION. "OIDC" fires no predicate by itself.

---

## B17-0A-019-004 — "build" + "OAuth" + "write tests" → IMPLEMENT + API_CONTRACT + SECURITY + AUTHORIZATION + TEST

**Input:**
"Build the Okta connector with OAuth 2.0 device flow, implement the token polling logic, and write unit tests for the authorization callback."

**Compiler analysis:**
- "Build" → IMPLEMENT
- "implement" → IMPLEMENT (reinforces)
- "connector" → API_CONTRACT
- "OAuth" → AUTHORIZATION (Rule 7)
- "token" → SECURITY (Rule 6)
- "write unit tests" → TEST (Rule 13)
- Integration vocabulary: connector, OAuth, token → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "TEST"]
**SubPhase:** 4.3, 3.3, 3.4, 5.1
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-019-005 — "develop" + "document" → IMPLEMENT + API_CONTRACT + DOCUMENT

**Input:**
"Develop the Okta SAML (Security Assertion Markup Language) connector for the enterprise SSO portal and document the authentication flow for IT administrators."

**Compiler analysis:**
- "Develop" → IMPLEMENT (Rule 11)
- "connector" → API_CONTRACT
- "SAML" — acronym; no predicate trigger
- "document" → DOCUMENT (Rule 12)
- "authentication" → SECURITY (Rule 8)
- Integration vocabulary: connector → 4.3 context

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "DOCUMENT"]
**SubPhase:** 4.3, 3.3, 3.4, 2.1
**TransitionType:** feature_addition
**DomainId:** software-development
