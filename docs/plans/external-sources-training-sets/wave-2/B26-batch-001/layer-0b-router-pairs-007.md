# Layer 0-B — Router Pairs (007)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: router

---

## R007-001 — SECURITY → 6.3 via "audit plaintext credentials"
Request: "Audit the ConfigService for hardcoded secrets and plaintext API credentials."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3
  Routing: SECURITY → SecurityAuditFlow63

## R007-002 — SECURITY + IMPLEMENT → [5.2/6.3, 4.1]
Request: "Implement output encoding in the ProductDescriptionRenderer and audit the existing XSS exposure."
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT"] | SubPhases: [5.2/6.3, 4.1]
  Routing: "implement + SECURITY" → 5.2 | "audit + SECURITY" → 6.3 | IMPLEMENT → 4.1
  Note: Mixed context (implement + audit) — factory creates flows for both 5.2 and 6.3

## R007-003 — SECURITY + QA + TEST → [6.3, 7.3, 5.2]
Request: "QA the refresh token rotation policy, write security tests asserting tokens are invalidated, and audit token storage."
  CompiledPredicateIds: ["QA", "SECURITY", "IMPLEMENT", "TEST"] | SubPhases: [7.3, 6.3, 4.1, 5.1]
  Routing: QA → 7.3 | SECURITY → 6.3 | IMPLEMENT via "write" → 4.1 | TEST via "tests" → 5.1

## R007-004 — SECURITY only via "enforce" (no audit verb)
Request: "Enforce minimum password length of 12 characters and disallow common passwords."
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT"]
  Note: "enforce" → SECURITY; no audit verb but SECURITY fires on "enforce"; "implement" implicit in enforcement action
  Routing: SECURITY → 5.2 (enforcement/implementation context) | IMPLEMENT → 4.1

## R007-005 — Zero predicates → gap
Request: "Check that the application is hardened before going to production."
  CompiledPredicateIds: []
  Routing: GapHandler | Teaching: "check" is gap; "hardened" as past participle is gap adjective; "going to production" is gap
