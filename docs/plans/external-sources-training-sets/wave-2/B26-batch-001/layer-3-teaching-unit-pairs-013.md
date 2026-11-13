# Layer 3 — Teaching Unit Pairs (013)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU013-001 — API security audit: SECURITY fires on authentication + vulnerability vocabulary
A: "Audit all REST API endpoints for authentication bypass vulnerabilities." Does SECURITY fire?
B: Yes. "audit" (verb) → SECURITY. "authentication bypass" → SECURITY. "vulnerabilities" → SECURITY.
   Dense signal: 3 triggers. CompiledPredicateIds: ["SECURITY"]. SubPhase: 6.3. deterministic_generic.

## TU013-002 — JWT algorithm vocabulary fires SECURITY
A: "Review the JWT token signing algorithm — is RS256 preferred over HS256?"
B: Yes. "JWT" → SECURITY. "signing algorithm" → SECURITY (signing trigger). "review" + security vocabulary → SECURITY.
   CompiledPredicateIds: ["SECURITY"]. SubPhase: 6.3.

## TU013-003 — OAuth fires SECURITY; OAuth documentation update is DOCUMENT
A: "Update the OAuth 2.0 documentation to reflect the new redirect URI policy." What predicate fires?
B: DOCUMENT fires on "update... documentation". "OAuth 2.0" is a SECURITY trigger but in a documentation-update context,
   the action shifts the predicate to DOCUMENT. CompiledPredicateIds: ["DOCUMENT"]. Not SECURITY alone.
   Rule: OAuth in a documentation-update action → DOCUMENT; OAuth in an audit action → SECURITY.

## TU013-004 — "QA" abbreviation fires QA; SECURITY+QA produce independent routings
A: "Audit the API gateway for credential exposure and QA the secret masking controls."
B: "audit" + "credential exposure" + "secret" → SECURITY → 6.3. "QA" → QA → 7.3.
   Two independent routing decisions. Two flow instances: SecurityComplianceReviewFlow63 + QATestingFlow73.

## TU013-005 — "authorization header" fires API_CONTRACT not AUTHORIZATION
A: "Ensure all API endpoints declare the Authorization header in their OpenAPI spec." What predicate fires?
B: API_CONTRACT fires on "API endpoints" + "OpenAPI spec". "Authorization header" is an HTTP header specification
   → API_CONTRACT, not the AUTHORIZATION predicate. CompiledPredicateIds: ["API_CONTRACT"].
   Teaching: Context determines which predicate fires for "Authorization" — HTTP spec context → API_CONTRACT.
