# Layer 0-D — Flow-Factory Pairs (003)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## F003-001 — SECURITY+API_CONTRACT → [SecurityAuditFlow63, ContractTestingFlow33]
Input: "Audit the API endpoints for missing authentication headers and broken object-level authorization."
  CompiledPredicateIds: ["SECURITY", "API_CONTRACT", "AUTHORIZATION"] | SubPhases: [6.3, 3.3, 6.3]
  Factory: FlowFactory.CreateMulti(["SECURITY","API_CONTRACT","AUTHORIZATION"])

## F003-002 — SECURITY+IMPLEMENT → [SecurityTestingFlow52, FeatureImplementationFlow41]
Input: "Implement rate limiting on the LoginService to prevent brute-force attacks."
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT"] | SubPhases: [5.2, 4.1]
  Factory: FlowFactory.CreateMulti(["SECURITY","IMPLEMENT"]) → [SecurityTestingFlow52, FeatureImplementationFlow41]
  Note: Implementation context routes SECURITY to 5.2, not 6.3

## F003-003 — SECURITY+QA+DOCUMENT → [SecurityAuditFlow63, QAGatingFlow73, DocumentationFlow62]
Input: "QA the security posture, audit the DataExportService, and document findings."
  CompiledPredicateIds: ["QA", "SECURITY", "DOCUMENT"] | SubPhases: [7.3, 6.3, 6.2]
  Factory: FlowFactory.CreateMulti(["QA","SECURITY","DOCUMENT"]) → 3 flows

## F003-004 — SECURITY+ARCHITECTURE → [SecurityAuditFlow63, ArchitectureFlow35]
Input: "Audit the cloud infrastructure security — review network segmentation and IAM policies."
  CompiledPredicateIds: ["SECURITY", "ARCHITECTURE"] | SubPhases: [6.3, 3.5]
  Factory: FlowFactory.CreateMulti(["SECURITY","ARCHITECTURE"]) → [SecurityAuditFlow63, InfrastructureArchitectureFlow35]

## F003-005 — SECURITY+TEST → [SecurityTestingFlow52, UnitTestingFlow51]
Input: "Write tests asserting the AuthGateway rejects requests without a valid JWT."
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT", "TEST"] | SubPhases: [5.2, 4.1, 5.1]
  Factory: FlowFactory.CreateMulti(["SECURITY","IMPLEMENT","TEST"]) → [SecurityTestingFlow52, FeatureImplementationFlow41, UnitTestingFlow51]
