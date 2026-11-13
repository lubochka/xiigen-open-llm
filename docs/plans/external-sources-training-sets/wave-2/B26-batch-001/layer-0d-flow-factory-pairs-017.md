# Layer 0-D — Flow Factory Pairs (017)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F017-001 — SECURITY+QA → [SecurityComplianceReviewFlow63, QATestingFlow73] (vulnerability assessment)
Input: "Conduct a vulnerability assessment of the OrderService for SSRF, XXE, and path traversal — and QA findings."
  CompiledPredicateIds: ["SECURITY", "QA"]
  Factory: FlowFactory.CreateMulti(["SECURITY","QA"]) → [SecurityComplianceReviewFlow63, QATestingFlow73]

## F017-002 — SECURITY → SecurityComplianceReviewFlow63 (CVE scan)
Input: "Scan the InvoiceService for known CVE vulnerabilities in its serialization layer."
  CompiledPredicateIds: ["SECURITY"]
  Factory: FlowFactory.Create("SECURITY") → SecurityComplianceReviewFlow63

## F017-003 — SECURITY+DOCUMENT → [SecurityComplianceReviewFlow63, DocumentationFlow62] (timing attack)
Input: "Assess the LoginService for timing attack vulnerabilities and document the remediation steps."
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"]
  Factory: FlowFactory.CreateMulti(["SECURITY","DOCUMENT"]) → [SecurityComplianceReviewFlow63, DocumentationFlow62]

## F017-004 — Zero predicates → GapHandler (performance assessment)
Input: "Assess the OrderService performance under peak load conditions."
  CompiledPredicateIds: [] | GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
  Note: "assess" requires security vocabulary to fire SECURITY. Performance context → zero predicates.

## F017-005 — SECURITY+QA → [SecurityComplianceReviewFlow63, QATestingFlow73] (ASVS Level 2)
Input: "Audit the NotificationService against ASVS Level 2 checklist and QA all failing controls."
  CompiledPredicateIds: ["SECURITY", "QA"]
  Factory: FlowFactory.CreateMulti(["SECURITY","QA"]) → [SecurityComplianceReviewFlow63, QATestingFlow73]
