# Layer 0-D — Flow Factory Pairs (018)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F018-001 — SECURITY+QA → [SecurityComplianceReviewFlow63, QATestingFlow73] (penetration test review)
Input: "Review penetration test findings for the AccountService and QA all critical vulnerability remediations."
  CompiledPredicateIds: ["SECURITY", "QA"]
  Factory: FlowFactory.CreateMulti(["SECURITY","QA"]) → [SecurityComplianceReviewFlow63, QATestingFlow73]

## F018-002 — SECURITY → SecurityComplianceReviewFlow63 (red team credential harvesting)
Input: "Review the red team engagement report for lateral movement and credential harvesting findings."
  CompiledPredicateIds: ["SECURITY"]
  Factory: FlowFactory.Create("SECURITY") → SecurityComplianceReviewFlow63

## F018-003 — SECURITY+DOCUMENT → [SecurityComplianceReviewFlow63, DocumentationFlow62] (penetration test remediation)
Input: "Audit penetration test remediation status for all CRITICAL findings and document the residual risk."
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"]
  Factory: FlowFactory.CreateMulti(["SECURITY","DOCUMENT"]) → [SecurityComplianceReviewFlow63, DocumentationFlow62]

## F018-004 — Zero predicates → GapHandler (pen test scheduling)
Input: "Schedule the pen test for Q3 and coordinate with the vendor."
  CompiledPredicateIds: [] | GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)

## F018-005 — SECURITY+QA → [SecurityComplianceReviewFlow63, QATestingFlow73] (brute-force + hash + MFA)
Input: "Audit the LoginService brute-force protection — hash algorithm strength and MFA bypass — and QA controls."
  CompiledPredicateIds: ["SECURITY", "QA"]
  Factory: FlowFactory.CreateMulti(["SECURITY","QA"]) → [SecurityComplianceReviewFlow63, QATestingFlow73]
