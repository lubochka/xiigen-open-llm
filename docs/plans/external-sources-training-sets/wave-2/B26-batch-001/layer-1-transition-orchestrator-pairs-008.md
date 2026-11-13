# Layer 1 — Transition-Orchestrator Pairs (008)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## T008-001 — SECURITY → 6.3 (dependency scan)
Request: "Scan project dependencies for known CVE vulnerabilities."
  TransitionType: feature_addition | SECURITY → SecurityAuditFlow63

## T008-002 — SECURITY+QA → [6.3, 7.3]
Request: "Audit the security hardening changes and QA before the patch release."
  TransitionType: feature_addition | SECURITY → SecurityAuditFlow63 | QA → QAGatingFlow73

## T008-003 — SECURITY+IMPLEMENT+TEST+DOCUMENT → 4 flows
Request: "Implement HSTS headers, write security tests, audit the policy, document the config."
  TransitionType: feature_addition | SECURITY → SecurityAuditFlow63 + SecurityTestingFlow52 | IMPLEMENT → FeatureImplementationFlow41 | TEST → UnitTestingFlow51 | DOCUMENT → DocumentationFlow62

## T008-004 — SECURITY+API_CONTRACT+AUTHORIZATION → [6.3, 3.3, 6.3]
Request: "Audit the API authentication scheme — verify all protected routes require a valid JWT."
  TransitionType: feature_addition | SECURITY → SecurityAuditFlow63 | API_CONTRACT → ContractTestingFlow33 | AUTHORIZATION → AuthorizationAuditFlow63

## T008-005 — Gap
Request: "Review the code before going to production."
  TransitionType: N/A | CompiledPredicateIds: [] | GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
  Teaching: "code review" without security vocabulary is gap. "going to production" is gap.
