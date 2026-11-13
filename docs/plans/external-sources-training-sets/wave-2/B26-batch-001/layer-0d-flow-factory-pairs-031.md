# Layer 0-D — Flow Factory Pairs (031) — CSPHeaderAudit Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F031-001 — CSP audit → SecurityComplianceReviewFlow63

"Audit CSP headers for unsafe-inline and unsafe-eval directives."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + CSP + unsafe directives)

---

## F031-002 — CSP + XSS review → SecurityComplianceReviewFlow63

"Review CSP configuration for permissive script-src directives that allow XSS."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (review + CSP + XSS = 2 triggers)

---

## F031-003 — HTTP security header scan → SecurityComplianceReviewFlow63

"Scan all HTTP security headers: CSP, HSTS, X-Frame-Options."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (scan + security headers + HSTS)

---

## F031-004 — "add CSP header" → no SecurityComplianceReviewFlow63

"Add a Content Security Policy header to the API gateway."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
Teaching: Configuration instruction without audit vocabulary → no SecurityComplianceReviewFlow63.

---

## F031-005 — CSP + IMPLEMENT + SECURITY → dual flows

"Implement strict CSP and audit existing directives for XSS bypass vectors."
FlowFactory.CreateMulti(["IMPLEMENT", "SECURITY"]) → [FeatureImplementationFlow41, SecurityComplianceReviewFlow63]
TransitionType: feature_addition (IMPLEMENT present)
