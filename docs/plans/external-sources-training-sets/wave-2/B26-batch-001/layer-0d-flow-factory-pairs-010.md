# Layer 0-D — Flow-Factory Pairs (010)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## F010-001 — SECURITY → 6.3 (broken authentication)
Input: "Audit the SessionService for broken authentication — verify expiry and logout invalidation."
  CompiledPredicateIds: ["SECURITY", "TEST"] | SubPhases: [6.3, 5.1]

## F010-002 — SECURITY+QA+DOCUMENT → [6.3, 7.3, 6.2]
Input: "QA the security controls before the patch, audit hotfix changes, document findings."
  CompiledPredicateIds: ["QA", "SECURITY", "DOCUMENT"] | SubPhases: [7.3, 6.3, 6.2]

## F010-003 — SECURITY → 6.3 (token replay)
Input: "Audit the RefreshTokenService for replay vulnerabilities."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

## F010-004 — SECURITY+QA+AUTHORIZATION+DOCUMENT → 4 flows
Input: "QA the RBAC, audit privilege escalation, verify admin locked down, document the matrix."
  CompiledPredicateIds: ["QA", "SECURITY", "AUTHORIZATION", "DOCUMENT", "TEST"] | SubPhases: [7.3, 6.3, 6.3, 6.2, 5.1]

## F010-005 — SECURITY+IMPLEMENT → [5.2, 4.1] (rate limiting)
Input: "Implement rate limiting on the LoginService to mitigate brute-force attacks."
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT"] | SubPhases: [5.2, 4.1]
  Note: Implementation context → SECURITY 5.2
