# Layer 0-D — Flow Factory Pairs (019)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F019-001 — SECURITY+QA → [SecurityComplianceReviewFlow63, QATestingFlow73] (secret rotation)
Input: "Audit the SecretStore for API keys past rotation deadline and QA the automated rotation pipeline."
  CompiledPredicateIds: ["SECURITY", "QA"]
  Factory: FlowFactory.CreateMulti(["SECURITY","QA"]) → [SecurityComplianceReviewFlow63, QATestingFlow73]

## F019-002 — SECURITY → SecurityComplianceReviewFlow63 (signing key lifecycle)
Input: "Review the signing key lifecycle — confirm rotation cadence, algorithm strength, and revocation procedures."
  CompiledPredicateIds: ["SECURITY"]
  Factory: FlowFactory.Create("SECURITY") → SecurityComplianceReviewFlow63

## F019-003 — SECURITY+DOCUMENT → [SecurityComplianceReviewFlow63, DocumentationFlow62] (hardcoded secrets)
Input: "Audit all repositories for hardcoded secrets and document the remediation plan for each exposure."
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"]
  Factory: FlowFactory.CreateMulti(["SECURITY","DOCUMENT"]) → [SecurityComplianceReviewFlow63, DocumentationFlow62]

## F019-004 — Zero predicates → GapHandler (secrets manager configuration task)
Input: "Configure the secrets manager to store environment variables for the deployment pipeline."
  CompiledPredicateIds: [] | GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)

## F019-005 — SECURITY+QA → [SecurityComplianceReviewFlow63, QATestingFlow73] (key management + HSM + cipher)
Input: "Audit the encryption key management service — cipher algorithm and HSM integration — and QA key lifecycle controls."
  CompiledPredicateIds: ["SECURITY", "QA"]
  Factory: FlowFactory.CreateMulti(["SECURITY","QA"]) → [SecurityComplianceReviewFlow63, QATestingFlow73]
