# Layer 0-D — Flow Factory Pairs (036) — SRI Integrity + Cookie Security Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F036-001 — SRI (Subresource Integrity) audit → SecurityComplianceReviewFlow63

"Audit all CDN script tags for missing SRI integrity hashes."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + SRI + integrity)

---

## F036-002 — Cookie security audit → SecurityComplianceReviewFlow63

"Audit all cookies for missing Secure, HttpOnly, and SameSite attributes."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + security cookie attributes)

---

## F036-003 — "add integrity attribute to script tag" → GapHandler

"Add the integrity attribute to the Bootstrap CDN script tag."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
Teaching: Adding an attribute is a configuration action without audit vocabulary → gap.

---

## F036-004 — Cookie + session security audit → SecurityComplianceReviewFlow63

"Audit session cookie configuration: verify HttpOnly, Secure, SameSite=Strict are enforced."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + session + enforce + cookie security vocabulary)

---

## F036-005 — SRI + IMPLEMENT → dual flows

"Implement SRI hashes for all CDN dependencies and audit existing script tags for missing integrity attributes."
FlowFactory.CreateMulti(["IMPLEMENT", "SECURITY"]) → [FeatureImplementationFlow41, SecurityComplianceReviewFlow63]
TransitionType: feature_addition
