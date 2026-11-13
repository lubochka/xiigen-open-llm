# Layer 0-D — Flow Factory Pairs (030) — OWASPTop10Review Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F030-001 — OWASP Top 10 review → SecurityComplianceReviewFlow63

"Review application against OWASP Top 10."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (OWASP Top 10 co-occurrence elevates trust)

---

## F030-002 — OWASP A01 + IDOR/privilege escalation → dual 6.3 flows

"Audit OWASP A01 — check IDOR and privilege escalation."
FlowFactory.CreateMulti(["SECURITY", "AUTHORIZATION"]) → [SecurityComplianceReviewFlow63, AuthorizationAuditFlow63]
TrustState: deterministic_generic (audit + IDOR + privilege escalation)

---

## F030-003 — OWASP A02 + cipher audit → SecurityComplianceReviewFlow63

"Audit cipher suites and certificate validation for OWASP A02."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63

---

## F030-004 — "make OWASP compliant" → no flow

"Make the application OWASP Top 10 compliant."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)

---

## F030-005 — OWASP + DOCUMENT audit report

"Audit OWASP vulnerabilities and document all findings."
FlowFactory.CreateMulti(["SECURITY", "DOCUMENT"]) → [SecurityComplianceReviewFlow63, DocumentationFlow62]
TransitionType: documentation (DOCUMENT present, IMPLEMENT absent)
