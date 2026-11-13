# Layer 0-D — Flow Factory Pairs (014)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F014-001 — SECURITY+DOCUMENT+QA → three flows (GDPR encryption audit)
Input: "Audit user data for GDPR-related encryption gaps, document findings, and QA data retention controls."
  CompiledPredicateIds: ["SECURITY", "DOCUMENT", "QA"]
  Factory: FlowFactory.CreateMulti(["SECURITY","DOCUMENT","QA"])
    → [SecurityComplianceReviewFlow63, DocumentationFlow62, QATestingFlow73]
  Note: "GDPR" alone is gap. "encryption gaps" provides the SECURITY trigger.

## F014-002 — Zero predicates → GapHandler ("GDPR ensure compliance")
Input: "Ensure the platform is GDPR compliant by the deadline."
  CompiledPredicateIds: [] | GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)

## F014-003 — SECURITY → SecurityComplianceReviewFlow63 (PII + TLS encryption audit)
Input: "Audit the PII storage layer for encryption at rest and TLS in transit."
  CompiledPredicateIds: ["SECURITY"]
  Factory: FlowFactory.Create("SECURITY") → SecurityComplianceReviewFlow63

## F014-004 — SECURITY+DOCUMENT → [SecurityComplianceReviewFlow63, DocumentationFlow62]
Input: "Review the data subject access request flow for security vulnerabilities and document findings."
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"]
  Factory: FlowFactory.CreateMulti(["SECURITY","DOCUMENT"]) → [SecurityComplianceReviewFlow63, DocumentationFlow62]

## F014-005 — SECURITY+QA → [SecurityComplianceReviewFlow63, QATestingFlow73]
Input: "Audit the consent module for cipher strength and QA the key rotation policy."
  CompiledPredicateIds: ["SECURITY", "QA"]
  Factory: FlowFactory.CreateMulti(["SECURITY","QA"]) → [SecurityComplianceReviewFlow63, QATestingFlow73]
