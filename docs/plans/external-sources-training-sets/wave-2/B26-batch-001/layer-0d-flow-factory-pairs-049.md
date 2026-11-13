# Layer 0-D — Flow Factory Pairs (049) — PenTestReport Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F049-001 — Pen test findings triage → SecurityComplianceReviewFlow63

"Audit and triage the external penetration test findings by severity, including SQL injection and SSRF findings."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + triage + SQL injection + SSRF)

---

## F049-002 — "schedule pen test" → GapHandler

"Schedule the quarterly penetration test engagement with the external security firm."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
Teaching: Scheduling is always gap. "penetration test" as event noun is gap.

---

## F049-003 — Pen test + DOCUMENT → documentation flow

"Review pen test findings and document the remediation roadmap for each CRITICAL and HIGH finding."
FlowFactory.CreateMulti(["SECURITY", "DOCUMENT"]) → [SecurityComplianceReviewFlow63, DocumentationFlow62]
TransitionType: documentation

---

## F049-004 — Pen test + IMPLEMENT → implementation flow

"Implement patches for all CRITICAL pen test findings: SSRF allowlist, SQL parameterization, credential rotation."
FlowFactory.CreateMulti(["IMPLEMENT", "SECURITY"]) → [FeatureImplementationFlow41, SecurityComplianceReviewFlow63]
TransitionType: feature_addition (IMPLEMENT present, overrides DOCUMENT)

---

## F049-005 — Multi-vector pen test scope → SecurityComplianceReviewFlow63

"Review pen test scope covering SQL injection, XSS, authentication bypass, and privilege escalation findings."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
Note: privilege escalation also fires AUTHORIZATION; CreateMulti would add AuthorizationAuditFlow63.
TrustState: deterministic_generic (four attack-vector triggers)
