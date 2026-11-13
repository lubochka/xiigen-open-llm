# Layer 0-D — Flow-Factory Pairs (002)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## F002-001 — SECURITY → SecurityAuditFlow63
Input: "Audit the AuthService for session fixation vulnerabilities."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3
  Factory: FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityAuditFlow63

## F002-002 — SECURITY+QA → [SecurityAuditFlow63, QAGatingFlow73]
Input: "Audit the PaymentModule and QA sign off before the release."
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhases: [6.3, 7.3]
  Factory: FlowFactory.CreateMulti(["SECURITY","QA"]) → [SecurityAuditFlow63, QAGatingFlow73]

## F002-003 — SECURITY+DOCUMENT → [SecurityAuditFlow63, DocumentationFlow62]
Input: "Document the security findings from the BillingService vulnerability audit."
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | SubPhases: [6.3, 6.2]
  Factory: FlowFactory.CreateMulti(["SECURITY","DOCUMENT"]) → [SecurityAuditFlow63, DocumentationFlow62]

## F002-004 — SECURITY+AUTHORIZATION → [SecurityAuditFlow63, AuthorizationTestingFlow52]
Input: "Audit the PermissionService for privilege escalation and broken access control."
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhases: [6.3, 6.3]
  Factory: FlowFactory.CreateMulti(["SECURITY","AUTHORIZATION"]) → [SecurityAuditFlow63, AuthorizationAuditFlow63]
  Note: In audit context, AUTHORIZATION also routes to 6.3 tier

## F002-005 — Zero predicates → GapHandler
Input: "Make the codebase more secure before the launch."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING
  Teaching: "more secure" adjective is gap. Zero predicates.
