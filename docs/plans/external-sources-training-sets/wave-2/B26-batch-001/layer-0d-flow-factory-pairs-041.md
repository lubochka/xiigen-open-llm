# Layer 0-D — Flow Factory Pairs (041) — SSRFAudit Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F041-001 — SSRF audit → SecurityComplianceReviewFlow63

"Audit all URL fetch endpoints for SSRF vulnerability allowing access to internal metadata services."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + SSRF + vulnerability)

---

## F041-002 — Allowlist enforcement audit → SecurityComplianceReviewFlow63

"Audit whether outbound HTTP calls enforce an allowlist to prevent SSRF to internal services."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + enforce + SSRF)

---

## F041-003 — "add HTTP client" → GapHandler

"Add an HTTP client to the payment service for calling the FX (Foreign Exchange) rate API."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
Teaching: Adding an HTTP client feature without SSRF/audit vocabulary is gap.

---

## F041-004 — SSRF + IMPLEMENT → dual flows

"Implement SSRF allowlist validation and audit existing URL fetch paths for metadata endpoint exposure."
FlowFactory.CreateMulti(["IMPLEMENT", "SECURITY"]) → [FeatureImplementationFlow41, SecurityComplianceReviewFlow63]
TransitionType: feature_addition

---

## F041-005 — SSRF write-tests → SecurityTestingFlow52

"Write security tests asserting the webhook service blocks SSRF to 169.254.169.254 and internal RFC 1918 ranges."
FlowFactory.Create("SECURITY", subPhase: "5.2") → SecurityTestingFlow52
Teaching: "write tests asserting" → 5.2.
