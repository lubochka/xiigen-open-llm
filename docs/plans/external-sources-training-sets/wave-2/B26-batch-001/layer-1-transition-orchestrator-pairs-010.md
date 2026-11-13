# Layer 1 — Transition-Orchestrator Pairs (010)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## T010-001 — SECURITY → 6.3 (mass assignment)
Request: "Audit the UserProfileController for mass assignment vulnerabilities."
  TransitionType: feature_addition | SECURITY → SecurityAuditFlow63

## T010-002 — SECURITY+QA → [6.3, 7.3]
Request: "Audit the session management and QA the cookie security flags (Secure, HttpOnly, SameSite)."
  TransitionType: feature_addition | SECURITY → SecurityAuditFlow63 | QA → QAGatingFlow73

## T010-003 — SECURITY+ARCHITECTURE → [6.3, 3.5]
Request: "Audit the AWS IAM roles for least-privilege violations in production."
  TransitionType: feature_addition | SECURITY → SecurityAuditFlow63 | ARCHITECTURE → InfrastructureArchitectureFlow35

## T010-004 — SECURITY+IMPLEMENT+ARCHITECTURE → [5.2, 4.1, 3.1]
Request: "Design a WAF integration for the public APIs and implement the firewall rules."
  TransitionType: documentation | SECURITY → SecurityTestingFlow52 | IMPLEMENT → FeatureImplementationFlow41 | ARCHITECTURE → ArchitectureFlow31

## T010-005 — SECURITY+QA+DOCUMENT → [6.3, 7.3, 6.2]
Request: "QA the payment processing security controls, audit PCI-DSS gaps, document the findings."
  TransitionType: documentation | SECURITY → SecurityAuditFlow63 | QA → QAGatingFlow73 | DOCUMENT → DocumentationFlow62
