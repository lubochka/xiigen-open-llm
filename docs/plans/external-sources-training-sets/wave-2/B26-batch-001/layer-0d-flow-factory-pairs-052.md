# Layer 0-D — Flow Factory Pairs (052) — ThreatIntelReport Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F052-001 — IOC review → SecurityComplianceReviewFlow63

"Review the threat intelligence report for IOCs matching our authentication service access logs."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (review + IOCs + authentication)

---

## F052-002 — IOC scan → SecurityComplianceReviewFlow63

"Scan application logs for indicators of compromise from the latest CISA advisory CVEs."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (scan + IOCs + CVEs)

---

## F052-003 — "read threat brief" → GapHandler

"Read through the latest threat intelligence briefing and share key highlights with the security team."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
Teaching: Reading and sharing information without security-action vocabulary is gap.

---

## F052-004 — Credential exposure + threat intel → SecurityComplianceReviewFlow63

"Audit exposed service account credentials listed in the threat intelligence feed."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + credentials + threat context)

---

## F052-005 — Threat intel + DOCUMENT → documentation flow

"Review IOC matches in perimeter logs and document findings in the security incident register."
FlowFactory.CreateMulti(["SECURITY", "DOCUMENT"]) → [SecurityComplianceReviewFlow63, DocumentationFlow62]
TransitionType: documentation
