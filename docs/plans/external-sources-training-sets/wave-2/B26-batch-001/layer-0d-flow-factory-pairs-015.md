# Layer 0-D — Flow Factory Pairs (015)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F015-001 — SECURITY+QA → [SecurityComplianceReviewFlow63, QATestingFlow73] (PCI-DSS tokenization)
Input: "Audit the payment module for PAN encryption and QA the tokenization controls."
  CompiledPredicateIds: ["SECURITY", "QA"]
  Factory: FlowFactory.CreateMulti(["SECURITY","QA"]) → [SecurityComplianceReviewFlow63, QATestingFlow73]
  Note: PCI-DSS acronym is gap. "PAN encryption" and "tokenization" provide SECURITY signal.

## F015-002 — Zero predicates → GapHandler (PCI-DSS standalone)
Input: "Ensure PCI-DSS compliance for the card data environment."
  CompiledPredicateIds: [] | GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)

## F015-003 — SECURITY → SecurityComplianceReviewFlow63 (AES-256 cardholder data)
Input: "Review cardholder data storage for AES-256 encryption at rest."
  CompiledPredicateIds: ["SECURITY"]
  Factory: FlowFactory.Create("SECURITY") → SecurityComplianceReviewFlow63

## F015-004 — SECURITY+DOCUMENT → [SecurityComplianceReviewFlow63, DocumentationFlow62] (CVV retention)
Input: "Audit the CardDataService for CVV retention violations and document non-compliant storage patterns."
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"]
  Factory: FlowFactory.CreateMulti(["SECURITY","DOCUMENT"]) → [SecurityComplianceReviewFlow63, DocumentationFlow62]

## F015-005 — SECURITY+QA → [SecurityComplianceReviewFlow63, QATestingFlow73] (TLS + cipher suite)
Input: "Audit TLS certificate configuration for card data channels and QA cipher suite selections."
  CompiledPredicateIds: ["SECURITY", "QA"]
  Factory: FlowFactory.CreateMulti(["SECURITY","QA"]) → [SecurityComplianceReviewFlow63, QATestingFlow73]
