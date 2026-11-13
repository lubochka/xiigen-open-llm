# Layer 0-B — Router Pairs (006)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: router

---

## R006-001 — SECURITY → 6.3 via "scan for SSRF"
Request: "Scan the WebhookService for SSRF vulnerabilities in the URL handler."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3
  Routing: SECURITY → SecurityAuditFlow63

## R006-002 — SECURITY + QA → [6.3, 7.3]
Request: "Audit the AuthService session management and QA before the production release."
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhases: [6.3, 7.3]
  Routing: SECURITY → 6.3 | QA → 7.3

## R006-003 — SECURITY + TEST → [5.2, 5.1]
Request: "Write penetration test scripts for the OrderService checkout flow."
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT", "TEST"] | SubPhases: [5.2, 4.1, 5.1]
  Routing: SECURITY in test-writing context → 5.2 | IMPLEMENT via "write" → 4.1 | TEST via "tests" → 5.1

## R006-004 — SECURITY + DOCUMENT + QA → [6.3, 6.2, 7.3]
Request: "QA the security audit findings for the BillingModule and document the remediation plan."
  CompiledPredicateIds: ["QA", "SECURITY", "DOCUMENT"] | SubPhases: [7.3, 6.3, 6.2]
  Routing: QA → 7.3 | SECURITY → 6.3 | DOCUMENT → 6.2

## R006-005 — SECURITY + AUTHORIZATION + ARCHITECTURE → [6.3, 6.3, 3.1]
Request: "Review the AuthorizationService architecture and audit for privilege escalation paths."
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION", "ARCHITECTURE"] | SubPhases: [6.3, 6.3, 3.1]
  Routing: SECURITY → 6.3 | AUTHORIZATION → 6.3 | ARCHITECTURE → 3.1
