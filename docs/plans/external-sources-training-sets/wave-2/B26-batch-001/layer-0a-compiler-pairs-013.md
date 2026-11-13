# Layer 0-A — Compiler Pairs (013)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: compiler

---

## C013-001 — SECURITY+QA: API security audit request

A: "Audit all REST API endpoints for authentication bypass vulnerabilities and QA the authorization gate."
  "audit" → SECURITY; "REST API endpoints" → API_CONTRACT; "authentication bypass" → SECURITY; "vulnerabilities" → SECURITY; "QA" → QA; "authorization" → AUTHORIZATION
  CompiledPredicateIds: ["SECURITY", "API_CONTRACT", "QA", "AUTHORIZATION"] | SubPhase: 6.3

---

## C013-002 — SECURITY: JWT token validation audit

A: "Scan the token issuance pipeline for JWT algorithm confusion vulnerabilities and certificate pinning gaps."
  "scan" → SECURITY; "JWT algorithm confusion" → SECURITY; "vulnerabilities" → SECURITY; "certificate pinning" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C013-003 — SECURITY+DOCUMENT: API security findings documented

A: "Review the OAuth 2.0 implementation for token leakage and document all security findings."
  "review" (security context) → SECURITY; "OAuth 2.0" → SECURITY (oauth trigger); "token leakage" → SECURITY; "document" → DOCUMENT
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | SubPhase: 6.3/6.2

---

## C013-004 — Gap: "API versioning" is not SECURITY

A: "Review the API versioning strategy and ensure backward compatibility for all endpoints."
  "review" → gap (no security co-occurrence); "API versioning" → gap; "backward compatibility" → gap; "ensure" → gap
  CompiledPredicateIds: [] → knowledge_edge
  Gap: "review" alone does not fire SECURITY. Security co-occurrence (vulnerability, authentication, encryption) is required.

---

## C013-005 — SECURITY+QA: credential exposure audit + QA sign-off

A: "Audit the API gateway for credential exposure in request headers and QA the secret masking controls."
  "audit" → SECURITY; "credential exposure" → SECURITY (credential trigger); "request headers" → API_CONTRACT; "QA" → QA; "secret masking" → SECURITY (secret trigger)
  CompiledPredicateIds: ["SECURITY", "API_CONTRACT", "QA"] | SubPhase: 6.3
  Predicate note: "credential" and "secret" are SECURITY triggers per predicate rules.
