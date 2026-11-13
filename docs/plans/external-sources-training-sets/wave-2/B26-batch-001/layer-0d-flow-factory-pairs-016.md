# Layer 0-D — Flow Factory Pairs (016)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F016-001 — SECURITY+QA → [SecurityComplianceReviewFlow63, QATestingFlow73] (threat model STRIDE)
Input: "Review threat model for authentication subsystem — validate STRIDE coverage — and QA all mitigations."
  CompiledPredicateIds: ["SECURITY", "QA"]
  Factory: FlowFactory.CreateMulti(["SECURITY","QA"]) → [SecurityComplianceReviewFlow63, QATestingFlow73]

## F016-002 — SECURITY → SecurityComplianceReviewFlow63 (attack surface)
Input: "Audit the external-facing attack surface — enumerate all ingress points and authentication mechanisms."
  CompiledPredicateIds: ["SECURITY"]
  Factory: FlowFactory.Create("SECURITY") → SecurityComplianceReviewFlow63

## F016-003 — SECURITY+AUTHORIZATION+DOCUMENT → three flows (threat model + privilege escalation)
Input: "Review threat model for privilege escalation paths and document all newly discovered attack vectors."
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION", "DOCUMENT"]
  Factory: FlowFactory.CreateMulti(["SECURITY","AUTHORIZATION","DOCUMENT"])
    → [SecurityComplianceReviewFlow63, AuthorizationAuditFlow63, DocumentationFlow62]

## F016-004 — Zero predicates → GapHandler (architecture diagram update)
Input: "Update the system architecture diagram to reflect the new microservice boundaries."
  CompiledPredicateIds: [] | GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)

## F016-005 — SECURITY+QA → [SecurityComplianceReviewFlow63, QATestingFlow73] (IDOR + mass assignment)
Input: "Audit threat modeling findings for IDOR and mass assignment vulnerabilities, and QA the remediation plan."
  CompiledPredicateIds: ["SECURITY", "QA"]
  Factory: FlowFactory.CreateMulti(["SECURITY","QA"]) → [SecurityComplianceReviewFlow63, QATestingFlow73]
