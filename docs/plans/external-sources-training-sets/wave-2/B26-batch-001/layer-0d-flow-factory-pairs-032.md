# Layer 0-D — Flow Factory Pairs (032) — SessionFixationAudit Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F032-001 — Session fixation audit → SecurityComplianceReviewFlow63

"Audit the login flow for session fixation vulnerabilities where the session token is not rotated after authentication."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + session token + authentication)

---

## F032-002 — Session token regeneration review → SecurityComplianceReviewFlow63

"Review whether the session token is regenerated after successful authentication to prevent session fixation."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (review + session token + authentication)

---

## F032-003 — "configure session timeout" → GapHandler

"Configure the session timeout to 30 minutes in the application settings."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
Teaching: Session timeout configuration without audit vocabulary is gap.

---

## F032-004 — Session fixation + IMPLEMENT → dual flows

"Implement session token regeneration on login and audit existing sessions for fixation exposure."
FlowFactory.CreateMulti(["IMPLEMENT", "SECURITY"]) → [FeatureImplementationFlow41, SecurityComplianceReviewFlow63]
TransitionType: feature_addition (IMPLEMENT present)

---

## F032-005 — Session fixation + DOCUMENT → documentation flow

"Audit the session fixation exposure and document the session rotation remediation plan."
FlowFactory.CreateMulti(["SECURITY", "DOCUMENT"]) → [SecurityComplianceReviewFlow63, DocumentationFlow62]
TransitionType: documentation (DOCUMENT present, IMPLEMENT absent)
