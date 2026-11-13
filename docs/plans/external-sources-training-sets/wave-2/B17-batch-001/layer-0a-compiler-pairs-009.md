# Layer 0-A — Compiler Pairs (009)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: compiler

---

## B17-0A-009-001 — Five predicates: IMPLEMENT + API_CONTRACT + SECURITY + AUTHORIZATION + TEST

**Input:**
"Build the Auth0 OAuth connector, implement the token refresh flow, secure the client credentials, and write unit tests."

**Compiler analysis:**
- "Build" → IMPLEMENT (Rule 11)
- "implement" → IMPLEMENT (reinforces)
- "connector" → API_CONTRACT (Rule 4)
- "OAuth" → AUTHORIZATION (Rule 7: "oauth" case-insensitive fires AUTHORIZATION)
- "token" → SECURITY (Rule 6: "token" fires SECURITY)
- "secure" → SECURITY (Rule 8; reinforces)
- "write unit tests" → TEST (Rule 13)
- Integration vocabulary: "connector", "OAuth", "Auth0" → IMPLEMENT routes to 4.3

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "TEST"]
**SubPhase:** 4.3, 3.3, 3.4, 3.4, 5.1
**TransitionType:** feature_addition
**DomainId:** software-development

**Teaching point:** SECURITY and AUTHORIZATION are separate predicates that both route to 3.4. DecisionCount: 5 (one per predicate fired).

---

## B17-0A-009-002 — "OAuth" fires AUTHORIZATION, NOT SECURITY

**Input A:** "Build the OAuth connector for Auth0." → IMPLEMENT + API_CONTRACT + AUTHORIZATION
**Input B:** "Build the token refresh handler." → IMPLEMENT + SECURITY (token → SECURITY)
**Input C:** "Build the OAuth connector and implement token refresh." → IMPLEMENT + API_CONTRACT + AUTHORIZATION + SECURITY

**Key distinction:**
- "oauth" (any case) → AUTHORIZATION (Rule 7)
- "token" → SECURITY (Rule 6)
- "authentication" → SECURITY (Rule 8)
- These are three separate triggers producing up to three separate predicates

---

## B17-0A-009-003 — "secure" and "token" both fire SECURITY (once)

**Input:**
"Build the Auth0 connector, secure the API endpoints, and implement token validation."

**Compiler analysis:**
- "Build" → IMPLEMENT
- "connector" → API_CONTRACT
- "secure" → SECURITY
- "token" → SECURITY (reinforces; SECURITY still fires once)
- No "oauth" → AUTHORIZATION does not fire
- Integration vocabulary → 4.3

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
**SubPhase:** 4.3, 3.3, 3.4
**TransitionType:** feature_addition
**DomainId:** software-development

**Teaching point:** Multiple SECURITY triggers (secure + token) still produce exactly one SECURITY predicate.

---

## B17-0A-009-004 — Full five-predicate predicate ordering

**Rule 20 — Canonical predicate ordering:**
IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION, TEST, DOCUMENT, QA

**Applied to:** Build the Auth0 OAuth connector, implement token refresh, secure credentials, write tests.

**Unordered triggers detected:** IMPLEMENT, TEST, AUTHORIZATION, SECURITY, API_CONTRACT
**Canonical ordered output:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "TEST"]

**Teaching point:** Predicates must always be emitted in canonical order regardless of the order they appear in the input text. The canonical sequence is fixed.
