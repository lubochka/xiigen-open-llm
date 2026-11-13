# Layer 0-D — Flow Factory Pairs (022)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F022-001 — SECURITY+QA → [SecurityComplianceReviewFlow63, QATestingFlow73] (RBAC + permission matrix)
Input: "Audit RBAC configuration for all AdminPanel endpoints and QA the permission matrix."
  CompiledPredicateIds: ["SECURITY", "QA"]
  Factory: FlowFactory.CreateMulti(["SECURITY","QA"]) → [SecurityComplianceReviewFlow63, QATestingFlow73]

## F022-002 — SECURITY+AUTHORIZATION → [SecurityComplianceReviewFlow63, AuthorizationAuditFlow63] (privilege escalation)
Input: "Review the AdminService for privilege escalation — can standard user elevate to admin via API parameter manipulation?"
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"]
  Factory: FlowFactory.CreateMulti(["SECURITY","AUTHORIZATION"]) → [SecurityComplianceReviewFlow63, AuthorizationAuditFlow63]
  Note: In audit context, AUTHORIZATION also routes to 6.3.

## F022-003 — SECURITY+AUTHORIZATION+DOCUMENT → three flows (access control matrix)
Input: "Audit access control enforcement points across microservices and document all unenforced authorization boundaries."
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION", "DOCUMENT"]
  Factory: FlowFactory.CreateMulti(["SECURITY","AUTHORIZATION","DOCUMENT"])
    → [SecurityComplianceReviewFlow63, AuthorizationAuditFlow63, DocumentationFlow62]

## F022-004 — Zero predicates → GapHandler (product feature permissions)
Input: "Allow users to manage their own team permissions from the settings page."
  CompiledPredicateIds: [] | GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)

## F022-005 — SECURITY+AUTHORIZATION+QA → three flows (default-deny posture + coverage)
Input: "Audit AuthorizationMiddleware for default-deny posture and QA unauthenticated access paths."
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION", "QA"]
  Factory: FlowFactory.CreateMulti(["SECURITY","AUTHORIZATION","QA"])
    → [SecurityComplianceReviewFlow63, AuthorizationAuditFlow63, QATestingFlow73]
