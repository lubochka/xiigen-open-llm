# Layer 0-D — Flow-Factory Pairs (009)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## F009-001 — SECURITY → 6.3 (mass assignment audit)
Input: "Audit the UserProfileController for mass assignment vulnerabilities."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

## F009-002 — SECURITY+QA → [6.3, 7.3] (PCI-DSS)
Input: "QA the payment tokenization and audit for PCI-DSS control gaps."
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhases: [6.3, 7.3]

## F009-003 — SECURITY+DOCUMENT → [6.3, 6.2] (threat model)
Input: "Document the threat model for the DataIngestionPipeline including attack surfaces."
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT", "DOCUMENT"] | SubPhases: [6.3, 4.1, 6.2]
  Evidence: "document" → DOCUMENT; "threat model" → SECURITY; "create" implicit in "document" → IMPLEMENT

## F009-004 — SECURITY+AUTHORIZATION → [6.3, 6.3] (IDOR audit)
Input: "Audit the ProductService for IDOR vulnerabilities — customers cannot access other customers' data."
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhases: [6.3, 6.3]

## F009-005 — SECURITY+IMPLEMENT+ARCHITECTURE → [5.2/6.3, 4.1, 3.1] (WAF design)
Input: "Design a WAF integration for the public APIs and implement the firewall rules."
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT", "ARCHITECTURE"] | SubPhases: [5.2, 4.1, 3.1]
  TransitionType: documentation (design)
