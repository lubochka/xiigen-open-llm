# Layer 0-D — Flow-Factory Pairs (012)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## F012-001 — SECURITY → 6.3 (XXE scan)
Input: "Scan the DataImportService for XXE injection vulnerabilities in the XML parser."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

## F012-002 — SECURITY+QA → [6.3, 7.3] (code injection)
Input: "Scan the ReportingEngine for code injection and QA before production."
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhases: [6.3, 7.3]

## F012-003 — SECURITY+IMPLEMENT+DOCUMENT → [5.2, 4.1, 6.2] (security advisory)
Input: "Implement the CVE fix for the OrderService, document the security advisory, verify patch."
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT", "DOCUMENT", "TEST"] | SubPhases: [5.2, 4.1, 6.2, 5.1]

## F012-004 — SECURITY+QA+AUTHORIZATION+API_CONTRACT → 4 flows
Input: "QA the API gateway authentication, audit for IDOR, verify bearer token validation on all routes."
  CompiledPredicateIds: ["QA", "SECURITY", "AUTHORIZATION", "API_CONTRACT", "TEST"] | SubPhases: [7.3, 6.3, 6.3, 3.3, 5.1]

## F012-005 — Gap → GapHandler
Input: "Set up a bug bounty program to find security issues."
  CompiledPredicateIds: []
  Factory: GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
  Teaching: "bug bounty program" is organizational/process vocabulary — gap. No security-action vocabulary present.
