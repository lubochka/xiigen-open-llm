# Layer 0-D — Flow Factory Pairs (048) — SECURITY+AUTHORIZATION Combo Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F048-001 — Wildcard permission audit → dual flows

"Audit all IAM policies for wildcard permission grants enabling unauthorized access."
FlowFactory.CreateMulti(["SECURITY", "AUTHORIZATION"]) → [SecurityComplianceReviewFlow63, AuthorizationAuditFlow63]
TrustState: deterministic_generic (wildcard permission = dual trigger)

---

## F048-002 — AUTHORIZATION solo → AuthorizationAuditFlow63

"Define the RBAC permission matrix for the document management module."
FlowFactory.Create("AUTHORIZATION", subPhase: "6.3") → AuthorizationAuditFlow63
Teaching: AUTHORIZATION without attack-vector vocabulary → AuthorizationAuditFlow63 only.

---

## F048-003 — "Authorization header" → API_CONTRACT, not authorization flow

"Add Authorization: Bearer header to outbound API calls per the OpenAPI specification."
FlowFactory.Create("API_CONTRACT", subPhase: "3.1") → ContractDefinitionFlow31
Teaching: HTTP Authorization header → ContractDefinitionFlow31, not AuthorizationAuditFlow63.

---

## F048-004 — Role claim forgery + SECURITY → dual flows

"Audit for role claim forgery in JWT tokens allowing unauthorized privilege elevation."
FlowFactory.CreateMulti(["SECURITY", "AUTHORIZATION"]) → [SecurityComplianceReviewFlow63, AuthorizationAuditFlow63]
TrustState: deterministic_generic (role claim forgery = dual trigger + JWT = security token)

---

## F048-005 — Full combo: SECURITY+AUTHORIZATION+QA+DOCUMENT → four flows

"Audit privilege escalation and IDOR vulnerabilities, QA the RBAC boundaries, and document all remediation steps."
FlowFactory.CreateMulti(["SECURITY","AUTHORIZATION","QA","DOCUMENT"])
→ [SecurityComplianceReviewFlow63, AuthorizationAuditFlow63, QATestingFlow73, DocumentationFlow62]
TransitionType: documentation (DOCUMENT present, IMPLEMENT absent)
