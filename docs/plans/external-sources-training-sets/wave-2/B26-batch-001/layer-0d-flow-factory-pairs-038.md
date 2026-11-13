# Layer 0-D — Flow Factory Pairs (038) — ClickjackingAudit Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F038-001 — Clickjacking audit → SecurityComplianceReviewFlow63

"Audit whether X-Frame-Options is set to DENY or SAMEORIGIN on all authenticated pages."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + X-Frame-Options = security header)

---

## F038-002 — CSP frame-ancestors audit → SecurityComplianceReviewFlow63

"Review the CSP frame-ancestors directive for permissive configurations enabling clickjacking."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (review + CSP + clickjacking = two triggers)

---

## F038-003 — "embed widget in iframe" → GapHandler

"Embed the pricing widget in an iframe on the partner landing page."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
Teaching: Widget embedding is a feature task; no audit vocabulary → gap.

---

## F038-004 — Clickjacking + IMPLEMENT → dual flows

"Implement X-Frame-Options DENY on all pages and audit admin portal for clickjacking exposure."
FlowFactory.CreateMulti(["IMPLEMENT", "SECURITY"]) → [FeatureImplementationFlow41, SecurityComplianceReviewFlow63]
TransitionType: feature_addition

---

## F038-005 — Write clickjacking tests → SecurityTestingFlow52

"Write security tests verifying X-Frame-Options DENY is present on all authenticated responses."
FlowFactory.Create("SECURITY", subPhase: "5.2") → SecurityTestingFlow52
Teaching: Test-writing context → 5.2. Not 6.3.
