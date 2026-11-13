# Layer 0-D — Flow-Factory Pairs (006)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## F006-001 — SECURITY → 6.3 (dependency CVE scan)
Input: "Scan project dependencies for known CVE vulnerabilities before the release."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

## F006-002 — SECURITY+AUTHORIZATION → [6.3, 6.3]
Input: "Audit the AdminService for wildcard permission grants and broken access control."
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhases: [6.3, 6.3]

## F006-003 — SECURITY+QA+TEST → [6.3, 7.3, 5.2]
Input: "QA the refresh token policy, write security assertions, audit token storage."
  CompiledPredicateIds: ["QA", "SECURITY", "IMPLEMENT", "TEST"] | SubPhases: [7.3, 6.3, 4.1, 5.1]

## F006-004 — SECURITY+IMPLEMENT → [5.2, 4.1] (enforcement path)
Input: "Implement password complexity requirements and enforce minimum 12-character length."
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT"] | SubPhases: [5.2, 4.1]

## F006-005 — SECURITY+DOCUMENT → [6.3, 6.2] (CVE remediation doc)
Input: "Document the CVE remediation status for all critical findings from the dependency audit."
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | SubPhases: [6.3, 6.2]
