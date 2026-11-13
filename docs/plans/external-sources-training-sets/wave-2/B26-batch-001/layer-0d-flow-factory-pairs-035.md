# Layer 0-D — Flow Factory Pairs (035) — CSRFProtection Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F035-001 — CSRF audit → SecurityComplianceReviewFlow63

"Audit all state-changing endpoints for missing CSRF token validation."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + CSRF + token validation)

---

## F035-002 — CSRF + SameSite review → SecurityComplianceReviewFlow63

"Review the SameSite cookie attribute and CSRF token implementation for checkout endpoint protection."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (review + SameSite + CSRF = two security triggers)

---

## F035-003 — "configure CSRF middleware" → GapHandler

"Configure the CSRF middleware and set the token expiry to 1 hour."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
Teaching: Middleware configuration without audit vocabulary is gap.

---

## F035-004 — CSRF write-tests → SecurityTestingFlow52

"Write tests asserting CSRF tokens are validated on all POST endpoints."
FlowFactory.Create("SECURITY", subPhase: "5.2") → SecurityTestingFlow52
TrustState: deterministic_generic
Teaching: "write tests" context → 5.2. Audit context → 6.3.

---

## F035-005 — CSRF + IMPLEMENT+SECURITY → dual flows

"Implement double-submit cookie CSRF protection and audit existing endpoints for missing tokens."
FlowFactory.CreateMulti(["IMPLEMENT", "SECURITY"]) → [FeatureImplementationFlow41, SecurityComplianceReviewFlow63]
TransitionType: feature_addition
