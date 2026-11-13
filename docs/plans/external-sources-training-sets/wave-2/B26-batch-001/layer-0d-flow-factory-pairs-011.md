# Layer 0-D — Flow-Factory Pairs (011)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## F011-001 — SECURITY → 6.3 (directory traversal)
Input: "Scan the FileStorageService for directory traversal vulnerabilities."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

## F011-002 — SECURITY+AUTHORIZATION → [6.3, 6.3] (wildcard permissions)
Input: "Audit the AdminService permission matrix for wildcard grants."
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhases: [6.3, 6.3]

## F011-003 — SECURITY+IMPLEMENT+TEST → [5.2, 4.1, 5.1] (AES-256 key rotation)
Input: "Implement AES-256 key rotation in the DataVaultService and write security tests."
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT", "TEST"] | SubPhases: [5.2, 4.1, 5.1]

## F011-004 — SECURITY+QA → [6.3, 7.3] (security QA checklist)
Input: "QA the security hardening checklist for the PaymentService before deployment."
  CompiledPredicateIds: ["QA", "SECURITY"] | SubPhases: [7.3, 6.3]

## F011-005 — SECURITY+ARCHITECTURE+DOCUMENT → [6.3, 3.1, 6.2] (zero-trust)
Input: "Design the zero-trust network architecture and document the security controls."
  CompiledPredicateIds: ["SECURITY", "ARCHITECTURE", "DOCUMENT"] | SubPhases: [6.3, 3.1, 6.2]
  TransitionType: documentation
