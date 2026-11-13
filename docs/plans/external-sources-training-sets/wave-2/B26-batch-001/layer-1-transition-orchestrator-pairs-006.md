# Layer 1 — Transition-Orchestrator Pairs (006)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## T006-001 — SECURITY → 6.3
Request: "Audit the AuthService for broken authentication — confirm session expiry after 30 min inactivity."
  TransitionType: feature_addition | SECURITY → SecurityAuditFlow63

## T006-002 — SECURITY+AUTHORIZATION → [6.3, 6.3]
Request: "Audit the CustomerPortal access control for IDOR vulnerabilities."
  TransitionType: feature_addition | SECURITY → SecurityAuditFlow63 | AUTHORIZATION → AuthorizationAuditFlow63

## T006-003 — SECURITY+QA+TEST → [6.3, 7.3, 5.2]
Request: "QA the security acceptance tests for the CheckoutService, audit for data leakage."
  TransitionType: feature_addition | SECURITY → SecurityAuditFlow63 | QA → QAGatingFlow73 | TEST → SecurityTestingFlow52

## T006-004 — SECURITY+IMPLEMENT → [5.2, 4.1]
Request: "Implement certificate pinning in the MobileApp SDK to prevent MITM attacks."
  TransitionType: feature_addition | SECURITY → SecurityTestingFlow52 | IMPLEMENT → FeatureImplementationFlow41

## T006-005 — SECURITY+DOCUMENT → [6.3, 6.2]
Request: "Document the security advisory for the SQL injection vulnerability in the OrderService."
  TransitionType: documentation | SECURITY → SecurityAuditFlow63 | DOCUMENT → DocumentationFlow62
