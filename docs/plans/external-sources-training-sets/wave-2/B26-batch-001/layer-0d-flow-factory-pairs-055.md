# Layer 0-D — Flow Factory Pairs (055) — MalwareReview Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F055-001 — Malware signature scan → SecurityComplianceReviewFlow63

"Scan all NuGet package artifacts for embedded malware signatures before production deployment."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (scan + malware signatures)

---

## F055-002 — Supply chain malicious code audit → SecurityComplianceReviewFlow63

"Audit the build pipeline for supply chain injection of malicious code in third-party dependencies."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + malicious code + injection)

---

## F055-003 — "update antivirus definitions" → GapHandler

"Update the antivirus and antimalware definitions on all build and production servers."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
Teaching: AV update is a maintenance task; no audit/vulnerability vocabulary → gap.

---

## F055-004 — Backdoor binary audit → SecurityComplianceReviewFlow63

"Audit deployed application images for backdoor binaries and unauthorized outbound connection patterns."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + backdoor + unauthorized)

---

## F055-005 — Malware + DOCUMENT → documentation flow

"Review malware artifacts and document the infection vector and affected credential stores."
FlowFactory.CreateMulti(["SECURITY", "DOCUMENT"]) → [SecurityComplianceReviewFlow63, DocumentationFlow62]
TransitionType: documentation
