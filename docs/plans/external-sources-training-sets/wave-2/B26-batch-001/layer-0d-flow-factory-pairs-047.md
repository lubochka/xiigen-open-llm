# Layer 0-D — Flow Factory Pairs (047) — PrivilegeEscalationAudit Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F047-001 — Privilege escalation audit → dual flows

"Audit the role assignment service for privilege escalation paths allowing non-admin users to gain admin access."
FlowFactory.CreateMulti(["SECURITY", "AUTHORIZATION"]) → [SecurityComplianceReviewFlow63, AuthorizationAuditFlow63]
TrustState: deterministic_generic (privilege escalation = dual trigger)

---

## F047-002 — IDOR audit → dual flows

"Scan all user-scoped endpoints for IDOR horizontal privilege escalation."
FlowFactory.CreateMulti(["SECURITY", "AUTHORIZATION"]) → [SecurityComplianceReviewFlow63, AuthorizationAuditFlow63]
TrustState: deterministic_generic (IDOR = dual trigger)

---

## F047-003 — "assign admin role" → GapHandler

"Assign the administrator role to the newly onboarded DevOps engineer in the IAM console."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
Teaching: Role assignment without audit vocabulary is always gap.

---

## F047-004 — Privilege escalation + QA → three flows

"Audit privilege escalation vectors across all RBAC roles and QA the updated permission boundary matrix."
FlowFactory.CreateMulti(["SECURITY", "AUTHORIZATION", "QA"]) → [SecurityComplianceReviewFlow63, AuthorizationAuditFlow63, QATestingFlow73]
TransitionType: feature_addition

---

## F047-005 — Privilege escalation write-tests → SecurityTestingFlow52 + AuthorizationAuditFlow63

"Write tests asserting non-admin users cannot escalate privileges and audit the role boundary checks."
FlowFactory.CreateMulti(["SECURITY(5.2)", "SECURITY(6.3)", "AUTHORIZATION"]) → [SecurityTestingFlow52, SecurityComplianceReviewFlow63, AuthorizationAuditFlow63]
Teaching: "write tests" → 5.2; "audit role boundary checks" → 6.3.
