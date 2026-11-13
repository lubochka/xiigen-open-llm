# Layer 0-D — Flow-Factory Pairs (008)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## F008-001 — SECURITY → 6.3 (open redirect)
Input: "Audit the PasswordResetService for open redirect vulnerabilities."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

## F008-002 — SECURITY+QA → [6.3, 7.3]
Input: "Audit the OAuth2 token flow and QA before production."
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhases: [6.3, 7.3]

## F008-003 — SECURITY+IMPLEMENT+QA → [5.2, 4.1, 7.3]
Input: "Implement input validation for the CustomerSearch endpoint, QA the filter, verify no injection."
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT", "QA", "TEST"] | SubPhases: [5.2, 4.1, 7.3, 5.1]

## F008-004 — SECURITY+API_CONTRACT+AUTHORIZATION → [6.3, 3.3, 6.3]
Input: "Review API authentication scheme — audit for missing bearer token validation on protected routes."
  CompiledPredicateIds: ["SECURITY", "API_CONTRACT", "AUTHORIZATION"] | SubPhases: [6.3, 3.3, 6.3]

## F008-005 — SECURITY+IMPLEMENT+TEST+DOCUMENT → 4 flows
Input: "Implement HSTS headers, write tests asserting HTTPS enforcement, audit policy, document config."
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT", "TEST", "DOCUMENT"] | SubPhases: [5.2/6.3, 4.1, 5.1, 6.2]
