# Layer 0-A — Compiler Pairs (056)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-056-001 — AuditableRelease: document vs. knowledge_edge

Input A: "Document the auditable release requirements for the regulated platform."
- "Document" → DOCUMENT
- "auditable release requirements" → DOCUMENT (requirements document; consolidates)
- PredicateSet: ["DOCUMENT"]
- TransitionType: no_contribution

Input B: "Configure the release pipeline for full auditability."
- "Configure" → not in lexicon
- "release pipeline" → context
- "auditability" → context
- PredicateSet: []
- ResultState: knowledge_edge

---

## B38-0A-056-002 — AuditableRelease: audit and sign-off

Input: "Audit the release process and sign off on the auditable release."
- "Audit" → QA + SECURITY (rule 19: audit always co-fires SECURITY)
- "sign off" → QA (consolidates)
- "auditable release" → context
- PredicateSet: ["QA","SECURITY"]
- TransitionType: no_contribution

---

## B38-0A-056-003 — AuditableRelease: criteria and compliance

Input: "Document the auditable release criteria and verify compliance."
- "Document" → DOCUMENT
- "auditable release criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "verify" → QA
- "compliance" → QA (consolidates)
- PredicateSet: ["QA","DOCUMENT"]
- TransitionType: no_contribution

---

## B38-0A-056-004 — AuditableRelease: build audit service

Input: "Build the audit trail service, document the auditability criteria, and approve the release."
- "Build" → IMPLEMENT (rule 6)
- "audit trail service" → IMPLEMENT (service — rule 2; consolidates)
- "document" → DOCUMENT
- "auditability criteria" → DOCUMENT (criteria — rule 3; consolidates)
- "approve" → QA
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT"]
- TransitionType: feature_addition

---

## B38-0A-056-005 — AuditableRelease: four-predicate

Input: "Review the audit trail service, audit the security controls, document the auditability criteria, and approve the release."
- "Review" → QA
- "audit trail service" → IMPLEMENT (rule 2)
- "audit" → QA (consolidates) + SECURITY (rule 19)
- "security controls" → SECURITY (consolidates)
- "document" → DOCUMENT
- "auditability criteria" → DOCUMENT (criteria; consolidates)
- "approve" → QA (consolidates)
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY"]
- TransitionType: feature_addition
