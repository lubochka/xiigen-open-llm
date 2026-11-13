# Layer 0-B — Router Pairs (010)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: router

---

## R010-001 — SECURITY → 6.3 via "broken authentication" audit
Request: "Audit the SessionService for broken authentication patterns — confirm session expiry after 30 minutes of inactivity."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

## R010-002 — SECURITY + QA + DOCUMENT → [6.3, 7.3, 6.2]
Request: "QA the security controls before the patch release, audit the hotfix changes, and document findings."
  CompiledPredicateIds: ["QA", "SECURITY", "DOCUMENT"] | SubPhases: [7.3, 6.3, 6.2]

## R010-003 — SECURITY + AUTHORIZATION + API_CONTRACT → [6.3, 6.3, 3.3]
Request: "Audit the API gateway authentication middleware and verify all protected routes require a valid JWT."
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION", "API_CONTRACT"] | SubPhases: [6.3, 6.3, 3.3]
  Routing: SECURITY+AUTHORIZATION → 6.3 | API_CONTRACT → 3.3

## R010-004 — SECURITY + ARCHITECTURE → [6.3, 3.5]
Request: "Audit the AWS (Amazon Web Services) IAM roles for least-privilege violations in the production environment."
  CompiledPredicateIds: ["SECURITY", "ARCHITECTURE"] | SubPhases: [6.3, 3.5]
  Routing: SECURITY → 6.3 | ARCHITECTURE → 3.5 (cloud infra)

## R010-005 — SECURITY + IMPLEMENT + QA → [5.2/4.1, 7.3]
Request: "Implement input validation for the CustomerSearch endpoint, QA the filter, and verify no injection paths remain."
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT", "QA", "TEST"] | SubPhases: [5.2, 4.1, 7.3, 5.1]
  Routing: SECURITY (enforce) → 5.2 | IMPLEMENT → 4.1 | QA → 7.3 | TEST (verify) → 5.1
