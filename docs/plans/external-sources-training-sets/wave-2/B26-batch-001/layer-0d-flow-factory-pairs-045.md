# Layer 0-D — Flow Factory Pairs (045) — SensitiveDataExposure Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F045-001 — PII leakage audit → SecurityComplianceReviewFlow63

"Audit application logs for sensitive PII leakage including email addresses and phone numbers."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + PII + sensitive data exposure)

---

## F045-002 — Hardcoded secret scan → SecurityComplianceReviewFlow63

"Scan the codebase for hardcoded secrets, API keys, and database credentials."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (scan + hardcoded secrets + credentials = three triggers)

---

## F045-003 — "mask credit card in UI" → GapHandler

"Mask the last four digits of credit card numbers in the account settings UI."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
Teaching: UI display masking without audit vocabulary is gap.

---

## F045-004 — TLS data-in-transit audit → SecurityComplianceReviewFlow63

"Audit all service-to-service API calls to verify TLS is enforced for data in transit."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + TLS + data in transit)

---

## F045-005 — Secrets + IMPLEMENT → dual flows

"Implement HashiCorp Vault for secrets management and audit current hardcoded credential exposure."
FlowFactory.CreateMulti(["IMPLEMENT", "SECURITY"]) → [FeatureImplementationFlow41, SecurityComplianceReviewFlow63]
TransitionType: feature_addition
