# Layer 0-D — Flow Factory Pairs (026) — ISO27001Review Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F026-001 — ISO 27001 gap → no flow

"Complete ISO 27001 gap analysis."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)

---

## F026-002 — ISO 27001 + TLS/crypto audit → SecurityComplianceReviewFlow63

"Review TLS cipher suites and certificate validity against ISO 27001 A.10."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (review + TLS + certificate = 2 crypto triggers)

---

## F026-003 — ISO 27001 + privilege escalation → dual 6.3 flows

"Audit IAM controls for privilege escalation per ISO 27001 A.9."
FlowFactory.CreateMulti(["SECURITY", "AUTHORIZATION"]) → [SecurityComplianceReviewFlow63, AuthorizationAuditFlow63]

---

## F026-004 — ISO 27001 + SECURITY + DOCUMENT

"Review cryptographic procedures and document deviations for ISMS evidence."
FlowFactory.CreateMulti(["SECURITY", "DOCUMENT"]) → [SecurityComplianceReviewFlow63, DocumentationFlow62]
TransitionType: documentation

---

## F026-005 — "review security posture conceptually" → no flow

"Review overall security posture conceptually for ISO 27001 readiness."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
Teaching: Conceptual posture review is gap → no flow created.
