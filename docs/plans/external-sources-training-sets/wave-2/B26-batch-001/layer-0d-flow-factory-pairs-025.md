# Layer 0-D — Flow Factory Pairs (025) — SOC2Audit Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F025-001 — SOC2 + SECURITY → SecurityComplianceReviewFlow63

"Audit authentication controls against SOC2 CC6."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit verb + authentication = 2 triggers)

---

## F025-002 — SOC2 + SECURITY + DOCUMENT → two flows

"Audit SOC2 access control evidence and document findings."
FlowFactory.CreateMulti(["SECURITY", "DOCUMENT"]) → [SecurityComplianceReviewFlow63, DocumentationFlow62]
TransitionType: documentation (DOCUMENT present, IMPLEMENT absent)

---

## F025-003 — SOC2 gap → no flow

"Ensure SOC2 compliance before year-end."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
No flow instantiated.

---

## F025-004 — SOC2 + IMPLEMENT + SECURITY → two flows, feature_addition

"Implement audit logging for SOC2 CC7 and audit current log coverage."
FlowFactory.CreateMulti(["IMPLEMENT", "SECURITY"]) → [FeatureImplementationFlow41, SecurityComplianceReviewFlow63]
TransitionType: feature_addition (IMPLEMENT present)

---

## F025-005 — SOC2 + SECURITY + AUTHORIZATION → two 6.3 flows

"Audit user privileges and RBAC boundaries per SOC2 CC6.3."
FlowFactory.CreateMulti(["SECURITY", "AUTHORIZATION"]) → [SecurityComplianceReviewFlow63, AuthorizationAuditFlow63]
Both flows at sub-phase 6.3. Flows are not collapsed; both run independently.
