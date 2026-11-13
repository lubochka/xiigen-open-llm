# Layer 0-D — Flow-Factory Pairs (005)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## F005-001 — SECURITY → 6.3 (JWT algorithm audit)
Input: "Audit the AuthService JWT validation to confirm RS256 (RSA with SHA-256) is used instead of HS256."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3
  Factory: SecurityAuditFlow63 only

## F005-002 — SECURITY+QA → [6.3, 7.3]
Input: "Audit the session management and QA the cookie security flags."
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhases: [6.3, 7.3]
  Factory: [SecurityAuditFlow63, QAGatingFlow73]

## F005-003 — SECURITY+IMPLEMENT+API_CONTRACT → 3 flows
Input: "Implement HTTPS enforcement and audit all API endpoints for plaintext exposure."
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT", "API_CONTRACT"] | SubPhases: [5.2/6.3, 4.1, 3.3]
  Factory: [SecurityTestingFlow52, SecurityAuditFlow63, FeatureImplementationFlow41, ContractTestingFlow33]

## F005-004 — Gap → GapHandler
Input: "Improve the overall security of the platform."
  CompiledPredicateIds: []
  Factory: GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
  Teaching: "improve" is gap; "security" as adjective noun without attack/audit vocabulary is gap

## F005-005 — SECURITY+DOCUMENT+ARCHITECTURE → [6.3, 6.2, 3.1]
Input: "Design the security architecture for the new Event Processing system and document the trust boundaries."
  CompiledPredicateIds: ["SECURITY", "ARCHITECTURE", "DOCUMENT"] | SubPhases: [6.3, 3.1, 6.2]
  Factory: [SecurityAuditFlow63, ArchitectureFlow31, DocumentationFlow62]
  TransitionType: documentation (design → documentation)
