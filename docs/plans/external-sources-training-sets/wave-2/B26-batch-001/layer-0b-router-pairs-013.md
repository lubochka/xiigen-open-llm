# Layer 0-B — Router Pairs (013)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: router

---

## R013-001 — SECURITY+QA+API_CONTRACT+AUTHORIZATION → [6.3, 7.3, 3.3, 6.3]
Request: "Audit all REST API endpoints for authentication bypass vulnerabilities and QA the authorization gate."
  CompiledPredicateIds: ["SECURITY", "API_CONTRACT", "QA", "AUTHORIZATION"]
  Routing: SECURITY → 6.3 | API_CONTRACT → 3.3 | QA → 7.3 | AUTHORIZATION → 6.3 (audit context)
  PrimarySubPhaseId: 6.3

## R013-002 — SECURITY → 6.3: JWT algorithm audit
Request: "Scan the token issuance pipeline for JWT algorithm confusion vulnerabilities and certificate pinning gaps."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3
  Routing: SECURITY + scan verb → SecurityComplianceReviewFlow63

## R013-003 — SECURITY+DOCUMENT → [6.3, 6.2]: OAuth findings documented
Request: "Review the OAuth 2.0 implementation for token leakage and document all security findings."
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | SubPhases: [6.3, 6.2]
  Routing: SECURITY → SecurityComplianceReviewFlow63 | DOCUMENT → DocumentationFlow62

## R013-004 — Zero predicates → GapHandler: "API versioning" review
Request: "Review the API versioning strategy and ensure backward compatibility."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING

## R013-005 — SECURITY+API_CONTRACT+QA → [6.3, 3.3, 7.3]: credential exposure + QA
Request: "Audit the API gateway for credential exposure in request headers and QA the secret masking controls."
  CompiledPredicateIds: ["SECURITY", "API_CONTRACT", "QA"] | SubPhases: [6.3, 3.3, 7.3]
  Routing: SECURITY → 6.3 | API_CONTRACT → 3.3 | QA → 7.3
