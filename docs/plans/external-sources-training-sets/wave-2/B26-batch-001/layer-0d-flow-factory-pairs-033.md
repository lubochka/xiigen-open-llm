# Layer 0-D — Flow Factory Pairs (033) — XSSMitigation Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F033-001 — XSS audit → SecurityComplianceReviewFlow63

"Audit all user-controlled output fields for XSS vulnerabilities."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + XSS + vulnerabilities)

---

## F033-002 — XSS + CSP review → SecurityComplianceReviewFlow63

"Review the Content Security Policy configuration and sanitization of user inputs to mitigate XSS."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (review + CSP + sanitize + XSS = four triggers)

---

## F033-003 — "render user comments" → GapHandler

"Render user comments with HTML encoding in the blog view template."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
Teaching: Display feature without XSS audit vocabulary is gap.

---

## F033-004 — XSS + IMPLEMENT → dual flows

"Implement output encoding and audit the comment input fields for stored XSS."
FlowFactory.CreateMulti(["IMPLEMENT", "SECURITY"]) → [FeatureImplementationFlow41, SecurityComplianceReviewFlow63]
TransitionType: feature_addition

---

## F033-005 — Write XSS tests → SecurityTestingFlow52 (not 6.3)

"Write security tests verifying the comment field sanitizes XSS payloads."
FlowFactory.Create("SECURITY", subPhase: "5.2") → SecurityTestingFlow52
TrustState: deterministic_generic
Teaching: Test-writing context → 5.2. Audit context → 6.3.
