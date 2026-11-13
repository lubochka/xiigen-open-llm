# Layer 0-B — Router Pairs (002)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: router

---

## R002-001 — SECURITY → 6.3 (audit context)
Request: "Audit the AuthService for session fixation vulnerabilities."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3
  Routing: SECURITY + audit verb → SecurityAuditFlow63

## R002-002 — SECURITY + QA → [6.3, 7.3]
Request: "Audit the PaymentService for SQL injection and QA the input validation."
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhases: [6.3, 7.3]
  Routing: SECURITY → SecurityAuditFlow63 | QA → QAGatingFlow73

## R002-003 — SECURITY → 5.2 (test-writing context, not audit)
Request: "Write security tests that assert the LoginService blocks brute-force after 5 attempts."
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT", "TEST"] | SubPhases: [5.2, 4.1, 5.1]
  Routing: SECURITY in test-writing context → SecurityTestingFlow52 (not 6.3)
  Teaching: "audit/review" context → 6.3; "write/assert" context → 5.2

## R002-004 — SECURITY + DOCUMENT → [6.3, 6.2]
Request: "Document the security findings from the BillingService audit."
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | SubPhases: [6.3, 6.2]
  Routing: SECURITY → SecurityAuditFlow63 | DOCUMENT → DocumentationFlow62

## R002-005 — SECURITY + AUTHORIZATION + QA → [6.3, 6.3, 7.3]
Request: "QA the AdminPanel role assignments and audit access control for IDOR vulnerabilities."
  CompiledPredicateIds: ["QA", "SECURITY", "AUTHORIZATION"] | SubPhases: [7.3, 6.3, 6.3]
  Routing: QA → 7.3 | SECURITY → 6.3 | AUTHORIZATION → 6.3 (audit context)
