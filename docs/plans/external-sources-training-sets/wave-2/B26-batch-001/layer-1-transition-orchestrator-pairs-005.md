# Layer 1 — Transition-Orchestrator Pairs (005)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## T005-001 — SECURITY+QA → [6.3, 7.3]
Request: "Scan for CVE vulnerabilities and QA the patch before the release."
  TransitionType: feature_addition | SECURITY → SecurityAuditFlow63 | QA → QAGatingFlow73

## T005-002 — SECURITY+IMPLEMENT+TEST → [5.2, 4.1, 5.1]
Request: "Implement 2FA for the AdminService and write tests verifying MFA enforcement."
  TransitionType: feature_addition | SECURITY → SecurityTestingFlow52 | IMPLEMENT → FeatureImplementationFlow41 | TEST → UnitTestingFlow51

## T005-003 — SECURITY+ARCHITECTURE → [6.3, 3.5]
Request: "Audit the cloud infrastructure for least-privilege IAM violations."
  TransitionType: feature_addition | SECURITY → SecurityAuditFlow63 | ARCHITECTURE → InfrastructureArchitectureFlow35

## T005-004 — SECURITY+QA+DOCUMENT → [6.3, 7.3, 6.2]
Request: "QA the security audit findings, audit the BillingModule, document all remediation steps."
  TransitionType: documentation | SECURITY → SecurityAuditFlow63 | QA → QAGatingFlow73 | DOCUMENT → DocumentationFlow62

## T005-005 — Gap
Request: "Make the CustomerPortal safe for users."
  TransitionType: N/A | CompiledPredicateIds: [] | GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
  Teaching: "safe" as adjective is gap; "users" is domain gap. Zero predicates.
