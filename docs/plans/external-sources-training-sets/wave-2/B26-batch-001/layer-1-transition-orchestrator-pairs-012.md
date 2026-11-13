# Layer 1 — Transition-Orchestrator Pairs (012)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## T012-001 — SECURITY → 6.3 (XXE)
Request: "Scan the DataImportService for XXE injection vulnerabilities."
  TransitionType: feature_addition | SECURITY → SecurityAuditFlow63

## T012-002 — SECURITY+QA → [6.3, 7.3]
Request: "Audit the ConfigService for hardcoded secrets and QA the secret management process."
  TransitionType: feature_addition | SECURITY → SecurityAuditFlow63 | QA → QAGatingFlow73

## T012-003 — SECURITY+IMPLEMENT+DOCUMENT → [5.2, 4.1, 6.2]
Request: "Implement the CVE fix, document the security advisory, verify the patch."
  TransitionType: feature_addition | SECURITY → SecurityTestingFlow52 | IMPLEMENT → FeatureImplementationFlow41 | DOCUMENT → DocumentationFlow62

## T012-004 — SECURITY+AUTHORIZATION+QA+DOCUMENT → 4 flows
Request: "QA the RBAC matrix, audit privilege escalation paths, confirm admin locked, document the controls."
  TransitionType: feature_addition | SECURITY → SecurityAuditFlow63 | AUTHORIZATION → AuthorizationAuditFlow63 | QA → QAGatingFlow73 | DOCUMENT → DocumentationFlow62

## T012-005 — Gap
Request: "Prepare a security roadmap for Q3."
  TransitionType: N/A | CompiledPredicateIds: [] | GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
  Teaching: "security roadmap" is a planning artifact — gap. "prepare" is gap. No security-action vocabulary.
