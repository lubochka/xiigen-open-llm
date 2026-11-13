# Layer 1 — Transition-Orchestrator Pairs (011)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## T011-001 — SECURITY → 6.3 (timing attack)
Request: "Audit the AuthService token comparison for timing attack vulnerabilities."
  TransitionType: feature_addition | SECURITY → SecurityAuditFlow63

## T011-002 — SECURITY+QA+TEST → [6.3, 7.3, 5.2]
Request: "Write penetration test scripts for the OrderService, QA the findings, and audit exposure."
  TransitionType: feature_addition | SECURITY → SecurityAuditFlow63 + SecurityTestingFlow52 | QA → QAGatingFlow73 | TEST → UnitTestingFlow51

## T011-003 — SECURITY+AUTHORIZATION+ARCHITECTURE → [6.3, 6.3, 3.1]
Request: "Review the AuthorizationService architecture — audit privilege escalation and trust boundaries."
  TransitionType: documentation | SECURITY → SecurityAuditFlow63 | AUTHORIZATION → AuthorizationAuditFlow63 | ARCHITECTURE → ArchitectureFlow31

## T011-004 — SECURITY+IMPLEMENT+TEST → [5.2, 4.1, 5.1]
Request: "Implement AES-256 key rotation in the DataVaultService and write security tests."
  TransitionType: feature_addition | SECURITY → SecurityTestingFlow52 | IMPLEMENT → FeatureImplementationFlow41 | TEST → UnitTestingFlow51

## T011-005 — SECURITY+QA+DOCUMENT → [6.3, 7.3, 6.2]
Request: "QA the security hardening checklist, audit the PaymentService, document remediation steps."
  TransitionType: documentation | SECURITY → SecurityAuditFlow63 | QA → QAGatingFlow73 | DOCUMENT → DocumentationFlow62
