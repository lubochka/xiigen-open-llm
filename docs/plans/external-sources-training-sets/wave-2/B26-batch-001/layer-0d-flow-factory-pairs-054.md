# Layer 0-D — Flow Factory Pairs (054) — ForensicsAnalysis Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F054-001 — Forensic log audit → SecurityComplianceReviewFlow63

"Audit authentication logs for credential stuffing attack signatures over the past 30 days."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + credential stuffing + authentication)

---

## F054-002 — Backdoor review → SecurityComplianceReviewFlow63

"Review the compromised server image for backdoor persistence mechanisms and unauthorized SSH key additions."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (review + backdoor + unauthorized + SSH)

---

## F054-003 — "archive server logs" → GapHandler

"Archive the production server logs to S3 for the quarterly compliance audit."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
Teaching: Archiving logs is a data management task; no attack/audit vocabulary → gap.

---

## F054-004 — Forensic injection scan → SecurityComplianceReviewFlow63

"Scan database query logs for SQL injection artifact signatures from prior exploitation."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (scan + SQL injection + exploitation)

---

## F054-005 — Forensics + DOCUMENT → documentation flow

"Audit breach artifacts and document the attack timeline, affected credentials, and data scope."
FlowFactory.CreateMulti(["SECURITY", "DOCUMENT"]) → [SecurityComplianceReviewFlow63, DocumentationFlow62]
TransitionType: documentation
