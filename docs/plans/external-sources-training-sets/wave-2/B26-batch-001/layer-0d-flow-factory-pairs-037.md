# Layer 0-D — Flow Factory Pairs (037) — CSRFProtection Flow Factory (extended)
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F037-001 — CSRF token entropy audit → SecurityComplianceReviewFlow63

"Audit the entropy of CSRF tokens to verify they are cryptographically random."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + CSRF + cryptographically random = crypto group trigger)

---

## F037-002 — CSRF + QA → dual flows

"Audit CSRF protection on all state-changing endpoints and QA the updated token validation middleware."
FlowFactory.CreateMulti(["SECURITY", "QA"]) → [SecurityComplianceReviewFlow63, QATestingFlow73]
TransitionType: feature_addition

---

## F037-003 — CSRF gap: validation library selection → GapHandler

"Select a CSRF protection library for the new API gateway."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
Teaching: Library selection without audit vocabulary is gap.

---

## F037-004 — CSRF + AUTHORIZATION (token bound to role) → dual flows

"Audit CSRF token validation and verify tokens are bound to authenticated user sessions with correct role claims."
FlowFactory.CreateMulti(["SECURITY", "AUTHORIZATION"]) → [SecurityComplianceReviewFlow63, AuthorizationAuditFlow63]
TrustState: deterministic_generic (role claims = AUTHORIZATION)

---

## F037-005 — CSRF + DOCUMENT → documentation flow

"Audit CSRF protection across all authenticated endpoints and document a per-endpoint token validation matrix."
FlowFactory.CreateMulti(["SECURITY", "DOCUMENT"]) → [SecurityComplianceReviewFlow63, DocumentationFlow62]
TransitionType: documentation
