# Layer 0-D — Flow Factory Pairs (056) — APTHunting Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F056-001 — APT lateral movement audit → SecurityComplianceReviewFlow63

"Audit authentication and network logs for APT lateral movement and credential harvesting indicators."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + lateral movement + credential harvesting)

---

## F056-002 — APT persistence scan → SecurityComplianceReviewFlow63

"Scan for APT persistence mechanisms including scheduled task injection and registry modification."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (scan + persistence mechanisms + injection)

---

## F056-003 — "monitor network for anomalies" → GapHandler

"Monitor all network traffic to identify anomalous connection patterns and baseline deviations."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
Teaching: Network monitoring without attack-vector vocabulary is gap.

---

## F056-004 — APT + AUTHORIZATION (RBAC role injection) → dual flows

"Audit for APT privilege escalation via compromised service account RBAC role injection."
FlowFactory.CreateMulti(["SECURITY", "AUTHORIZATION"]) → [SecurityComplianceReviewFlow63, AuthorizationAuditFlow63]
TrustState: deterministic_generic (privilege escalation + RBAC role injection = dual triggers)

---

## F056-005 — APT + DOCUMENT → documentation flow

"Review APT hunting findings for exfiltration paths and document affected data stores and credential scope."
FlowFactory.CreateMulti(["SECURITY", "DOCUMENT"]) → [SecurityComplianceReviewFlow63, DocumentationFlow62]
TransitionType: documentation
