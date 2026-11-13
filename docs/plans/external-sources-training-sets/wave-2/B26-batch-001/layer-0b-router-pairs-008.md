# Layer 0-B — Router Pairs (008)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: router

---

## R008-001 — SECURITY → 6.3 via "OWASP audit"
Request: "Audit the CustomerPortal against the OWASP Top 10 checklist."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3
  Routing: SECURITY → SecurityAuditFlow63

## R008-002 — SECURITY + API_CONTRACT + AUTHORIZATION → [6.3, 3.3, 6.3]
Request: "Review the API authentication scheme — audit for missing bearer token validation on protected endpoints."
  CompiledPredicateIds: ["SECURITY", "API_CONTRACT", "AUTHORIZATION"] | SubPhases: [6.3, 3.3, 6.3]
  Routing: SECURITY → 6.3 | API_CONTRACT → 3.3 | AUTHORIZATION → 6.3

## R008-003 — SECURITY + QA + ARCHITECTURE → [6.3, 7.3, 3.5]
Request: "QA the security posture of the cloud deployment — audit network security groups and IAM (Identity and Access Management) roles."
  CompiledPredicateIds: ["QA", "SECURITY", "ARCHITECTURE"] | SubPhases: [7.3, 6.3, 3.5]
  Routing: QA → 7.3 | SECURITY → 6.3 | ARCHITECTURE (infra) → 3.5

## R008-004 — SECURITY + DOCUMENT → [6.3, 6.2]
Request: "Document the CVE remediation status for all critical vulnerabilities found in the dependency audit."
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | SubPhases: [6.3, 6.2]
  Routing: SECURITY → 6.3 | DOCUMENT → 6.2

## R008-005 — SECURITY + IMPLEMENT + TEST + DOCUMENT → 4 flows
Request: "Implement OWASP Top 10 mitigations, write security tests, document the hardening approach, and audit the remaining exposure."
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT", "TEST", "DOCUMENT"] | SubPhases: [6.3/5.2, 4.1, 5.1, 6.2]
  Routing: SECURITY → 6.3 (audit) + 5.2 (test writing) | IMPLEMENT → 4.1 | TEST → 5.1 | DOCUMENT → 6.2
