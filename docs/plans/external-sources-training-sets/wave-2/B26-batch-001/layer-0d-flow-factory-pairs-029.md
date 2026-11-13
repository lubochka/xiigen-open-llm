# Layer 0-D — Flow Factory Pairs (029) — NISTFramework Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F029-001 — NIST CSF management → no flow

"Align security program to NIST CSF."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)

---

## F029-002 — NIST SP 800-53 + privilege escalation → dual 6.3 flows

"Audit NIST SP 800-53 AC-2 controls for privilege escalation."
FlowFactory.CreateMulti(["SECURITY", "AUTHORIZATION"]) → [SecurityComplianceReviewFlow63, AuthorizationAuditFlow63]

---

## F029-003 — NIST FIPS 140-2 + cryptographic review → SecurityComplianceReviewFlow63

"Review cryptographic module implementations against NIST FIPS 140-2."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (review + cryptographic)

---

## F029-004 — NIST risk vocabulary → no flow

"Perform asset inventory and risk assessment per NIST CSF Identify."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)

---

## F029-005 — NIST Detect + authentication scan → SecurityComplianceReviewFlow63

"Scan endpoints for anomalous authentication events per NIST Detect."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (scan + authentication)
