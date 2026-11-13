# Layer 0-D — Flow Factory Pairs (023)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F023-001 — SECURITY+QA → [SecurityComplianceReviewFlow63, QATestingFlow73] (AES/bcrypt/HMAC)
Input: "Audit DataEncryptionService — AES-256 key size, bcrypt iteration count, HMAC integrity — and QA all controls."
  CompiledPredicateIds: ["SECURITY", "QA"]
  Factory: FlowFactory.CreateMulti(["SECURITY","QA"]) → [SecurityComplianceReviewFlow63, QATestingFlow73]

## F023-002 — SECURITY → SecurityComplianceReviewFlow63 (hash algorithm: bcrypt/MD5/SHA-1)
Input: "Review all password hashing configurations — confirm bcrypt cost ≥ 12, reject MD5 and SHA-1."
  CompiledPredicateIds: ["SECURITY"]
  Factory: FlowFactory.Create("SECURITY") → SecurityComplianceReviewFlow63

## F023-003 — SECURITY+DOCUMENT → [SecurityComplianceReviewFlow63, DocumentationFlow62] (encryption-at-rest)
Input: "Audit database encryption-at-rest configuration and document all fields that store PII without encryption."
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"]
  Factory: FlowFactory.CreateMulti(["SECURITY","DOCUMENT"]) → [SecurityComplianceReviewFlow63, DocumentationFlow62]

## F023-004 — SECURITY+IMPLEMENT → [SecurityComplianceReviewFlow63, FeatureImplementationFlow41] (CBC→AES-GCM)
Input: "Audit encryption for CBC mode vulnerabilities and implement AES-GCM as the replacement cipher."
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT"]
  Factory: FlowFactory.CreateMulti(["SECURITY","IMPLEMENT"]) → [SecurityComplianceReviewFlow63, FeatureImplementationFlow41]
  TransitionType: feature_addition (IMPLEMENT present)
  Note: IMPLEMENT changes the TransitionType from documentation to feature_addition.

## F023-005 — SECURITY+QA → [SecurityComplianceReviewFlow63, QATestingFlow73] (TLS 1.3 + certificate rotation)
Input: "Audit TLS configuration — confirm TLS 1.3, reject 1.0/1.1 — and QA certificate rotation automation."
  CompiledPredicateIds: ["SECURITY", "QA"]
  Factory: FlowFactory.CreateMulti(["SECURITY","QA"]) → [SecurityComplianceReviewFlow63, QATestingFlow73]
