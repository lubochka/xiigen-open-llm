# Layer 0-B — Router Pairs (003)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: router

---

## R003-001 — SECURITY → 6.3 via "scan for vulnerabilities"
Request: "Scan the ProductSearchService for XSS vulnerabilities."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3
  Routing: SECURITY + scan/audit context → SecurityAuditFlow63

## R003-002 — SECURITY + API_CONTRACT → [6.3, 3.3]
Request: "Audit the API endpoints for missing authentication headers and broken object-level authorization."
  CompiledPredicateIds: ["SECURITY", "API_CONTRACT", "AUTHORIZATION"] | SubPhases: [6.3, 3.3, 6.3]
  Routing: SECURITY → 6.3 | API_CONTRACT → 3.3 | AUTHORIZATION → 6.3

## R003-003 — SECURITY + ARCHITECTURE → [6.3, 3.1]
Request: "Review the security architecture of the microservices mesh for trust boundary violations."
  CompiledPredicateIds: ["SECURITY", "ARCHITECTURE"] | SubPhases: [6.3, 3.1]
  Routing: SECURITY → 6.3 | ARCHITECTURE → ArchitectureFlow31

## R003-004 — SECURITY + QA + DOCUMENT → [6.3, 7.3, 6.2]
Request: "Audit the DataExportService for data leakage, QA the masking rules, and document all findings."
  CompiledPredicateIds: ["SECURITY", "QA", "DOCUMENT"] | SubPhases: [6.3, 7.3, 6.2]
  Routing: SECURITY → 6.3 | QA → 7.3 | DOCUMENT → 6.2

## R003-005 — SECURITY + IMPLEMENT → [6.3/5.2, 4.1]
Request: "Implement CSRF token validation on all POST endpoints and audit the existing implementation."
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT", "API_CONTRACT"] | SubPhases: [5.2, 4.1, 3.3]
  Routing: SECURITY in implement context → 5.2 | IMPLEMENT → 4.1 | API_CONTRACT → 3.3
  Note: "audit the existing implementation" → SECURITY also routes to 6.3 (mixed context → both 5.2 and 6.3)
