# Layer 0-D — Flow Factory Pairs (027) — HIPAACompliance Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F027-001 — HIPAA gap → no flow

"Ensure patient portal is HIPAA compliant."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)

---

## F027-002 — HIPAA + encryption audit → SecurityComplianceReviewFlow63

"Audit PHI encryption at rest and in transit."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + encryption = strong signal)

---

## F027-003 — HIPAA + MFA + session → SecurityComplianceReviewFlow63

"Review MFA configuration and session management for HIPAA systems."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (review + MFA + session = 2 auth triggers)

---

## F027-004 — HIPAA + Business Associate Agreement → DocumentationFlow62

"Prepare Business Associate Agreement documentation."
FlowFactory.Create("DOCUMENT", subPhase: "6.2") → DocumentationFlow62
No SecurityComplianceReviewFlow63 — no security action vocabulary in this request.

---

## F027-005 — HIPAA + RBAC audit → dual 6.3 flows

"Audit PHI access controls and verify RBAC boundaries."
FlowFactory.CreateMulti(["SECURITY", "AUTHORIZATION"]) → [SecurityComplianceReviewFlow63, AuthorizationAuditFlow63]
Both run at sub-phase 6.3.
