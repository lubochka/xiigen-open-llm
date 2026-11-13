# Layer 0-D — Flow Factory Pairs (013)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F013-001 — SECURITY+API_CONTRACT+QA+AUTHORIZATION → four flows
Input: "Audit all REST API endpoints for authentication bypass vulnerabilities and QA the authorization gate."
  CompiledPredicateIds: ["SECURITY", "API_CONTRACT", "QA", "AUTHORIZATION"]
  Factory: FlowFactory.CreateMulti(["SECURITY","API_CONTRACT","QA","AUTHORIZATION"])
    → [SecurityComplianceReviewFlow63, ApiContractDesignFlow33, QATestingFlow73, AuthorizationAuditFlow63]
  TrustState: deterministic_generic

## F013-002 — SECURITY → SecurityComplianceReviewFlow63
Input: "Scan the token issuance pipeline for JWT algorithm confusion and certificate pinning gaps."
  CompiledPredicateIds: ["SECURITY"]
  Factory: FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
  TrustState: deterministic_generic

## F013-003 — SECURITY+DOCUMENT → [SecurityComplianceReviewFlow63, DocumentationFlow62]
Input: "Review the OAuth 2.0 implementation for token leakage and document all security findings."
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"]
  Factory: FlowFactory.CreateMulti(["SECURITY","DOCUMENT"]) → [SecurityComplianceReviewFlow63, DocumentationFlow62]

## F013-004 — Zero predicates → GapHandler (no flow instantiated)
Input: "Review the API versioning strategy and ensure backward compatibility."
  CompiledPredicateIds: [] | GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
  No flow instantiated.

## F013-005 — SECURITY+API_CONTRACT+QA → [SecurityComplianceReviewFlow63, ApiContractDesignFlow33, QATestingFlow73]
Input: "Audit the API gateway for credential exposure in request headers and QA the secret masking controls."
  CompiledPredicateIds: ["SECURITY", "API_CONTRACT", "QA"]
  Factory: FlowFactory.CreateMulti(["SECURITY","API_CONTRACT","QA"])
    → [SecurityComplianceReviewFlow63, ApiContractDesignFlow33, QATestingFlow73]
