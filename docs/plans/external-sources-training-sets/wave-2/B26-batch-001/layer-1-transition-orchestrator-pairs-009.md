# Layer 1 — Transition-Orchestrator Pairs (009)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## T009-001 — SECURITY → 6.3 (open redirect)
Request: "Audit the PasswordResetService for open redirect in the redirect URL parameter."
  TransitionType: feature_addition | SECURITY → SecurityAuditFlow63

## T009-002 — SECURITY+QA+AUTHORIZATION → [6.3, 7.3, 6.3]
Request: "QA the CustomerPortal access controls, audit for broken object-level authorization."
  TransitionType: feature_addition | SECURITY → SecurityAuditFlow63 | QA → QAGatingFlow73 | AUTHORIZATION → AuthorizationAuditFlow63

## T009-003 — SECURITY+IMPLEMENT → [5.2, 4.1]
Request: "Implement input validation and allowlist for permitted file types in the DocumentUpload endpoint."
  TransitionType: feature_addition | SECURITY → SecurityTestingFlow52 | IMPLEMENT → FeatureImplementationFlow41

## T009-004 — SECURITY+DOCUMENT → [6.3, 6.2]
Request: "Document the security architecture of the new Payments subdomain — include trust boundaries."
  TransitionType: documentation | SECURITY → SecurityAuditFlow63 | DOCUMENT → DocumentationFlow62

## T009-005 — SECURITY+QA+IMPLEMENT+TEST → 4 flows
Request: "Implement the OWASP mitigations, write regression tests, QA the changes, audit remaining exposure."
  TransitionType: feature_addition | SECURITY → SecurityAuditFlow63 + SecurityTestingFlow52 | IMPLEMENT → FeatureImplementationFlow41 | QA → QAGatingFlow73 | TEST → UnitTestingFlow51
