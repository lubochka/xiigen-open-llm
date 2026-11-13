# Layer 0-D — Flow Factory Pairs (058) — ZeroTrustAudit Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F058-001 — Zero Trust segmentation audit → dual flows

"Audit network segmentation to verify Zero Trust micro-segmentation enforces least-privilege access controls."
FlowFactory.CreateMulti(["SECURITY", "AUTHORIZATION"]) → [SecurityComplianceReviewFlow63, AuthorizationAuditFlow63]
TrustState: deterministic_generic (least-privilege access = dual trigger)

---

## F058-002 — mTLS review → SecurityComplianceReviewFlow63

"Review mTLS certificate authentication configuration between all microservices."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (review + mTLS = TLS crypto group)

---

## F058-003 — "adopt Zero Trust" → GapHandler

"Adopt Zero Trust architecture principles across the enterprise infrastructure."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
Teaching: Strategic adoption without audit vocabulary is gap.

---

## F058-004 — Unencrypted east-west scan → SecurityComplianceReviewFlow63

"Scan all east-west service calls for missing mutual authentication and unencrypted channels."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (scan + unencrypted + authentication)

---

## F058-005 — Zero Trust + IMPLEMENT → dual flows

"Implement mutual TLS between all services and audit service-to-service tokens for expiry and rotation compliance."
FlowFactory.CreateMulti(["IMPLEMENT", "SECURITY"]) → [FeatureImplementationFlow41, SecurityComplianceReviewFlow63]
TransitionType: feature_addition
