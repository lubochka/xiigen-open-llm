# Layer 0-D — Flow Factory Pairs (042) — RaceConditionAudit Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F042-001 — Race condition audit → SecurityComplianceReviewFlow63

"Audit the OTP issuance flow for race condition vulnerabilities enabling double-use of one-time tokens."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + vulnerability + token)

---

## F042-002 — Race + privilege escalation → dual flows

"Audit session creation race conditions that allow privilege escalation via concurrent logins."
FlowFactory.CreateMulti(["SECURITY", "AUTHORIZATION"]) → [SecurityComplianceReviewFlow63, AuthorizationAuditFlow63]
TrustState: deterministic_generic (privilege escalation = dual trigger)

---

## F042-003 — "add optimistic locking" → GapHandler

"Add optimistic locking to the checkout order creation flow to prevent double-submit."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
Teaching: Database locking implementation without audit vocabulary is gap.

---

## F042-004 — Race + IMPLEMENT → dual flows

"Implement atomic token redemption and audit the OTP flow for race condition exposure."
FlowFactory.CreateMulti(["IMPLEMENT", "SECURITY"]) → [FeatureImplementationFlow41, SecurityComplianceReviewFlow63]
TransitionType: feature_addition

---

## F042-005 — TOCTOU audit → SecurityComplianceReviewFlow63

"Audit the file permission check sequence for TOCTOU (Time-of-Check to Time-of-Use) race condition exploits."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + TOCTOU + race condition)
