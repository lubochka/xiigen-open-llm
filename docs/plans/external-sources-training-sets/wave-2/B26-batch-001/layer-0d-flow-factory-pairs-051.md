# Layer 0-D — Flow Factory Pairs (051) — BugBountyTriage Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F051-001 — Bug bounty XSS triage → SecurityComplianceReviewFlow63

"Triage the bug bounty submission reporting stored XSS in the comment input field."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (triage + XSS + stored = attack-vector)

---

## F051-002 — Bug bounty IDOR → dual flows

"Review the bug bounty IDOR report in the invoice download endpoint and assess the privilege scope."
FlowFactory.CreateMulti(["SECURITY", "AUTHORIZATION"]) → [SecurityComplianceReviewFlow63, AuthorizationAuditFlow63]
TrustState: deterministic_generic (IDOR = dual trigger)

---

## F051-003 — "send reward to researcher" → GapHandler

"Send the $2,500 reward to the bug bounty researcher for the valid CSRF report."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
Teaching: Reward payment is an administrative action; no security-action vocabulary present.

---

## F051-004 — Bug bounty CSRF → SecurityComplianceReviewFlow63

"Audit the CSRF vulnerability from the bug bounty report on the password reset endpoint."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + CSRF)

---

## F051-005 — Bug bounty triage + IMPLEMENT fix → dual flows

"Triage the SSRF bug bounty report and implement URL allowlist validation to close the vulnerability."
FlowFactory.CreateMulti(["SECURITY", "IMPLEMENT"]) → [SecurityComplianceReviewFlow63, FeatureImplementationFlow41]
TransitionType: feature_addition
