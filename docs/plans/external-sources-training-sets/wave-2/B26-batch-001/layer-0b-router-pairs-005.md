# Layer 0-B — Router Pairs (005)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: router

---

## R005-001 — SECURITY → 6.3 via "audit JWT"
Request: "Audit the JWT validation in the AuthGateway — confirm algorithm pinning is enforced."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3
  Routing: SECURITY + audit → SecurityAuditFlow63

## R005-002 — SECURITY + QA + AUTHORIZATION → [6.3, 7.3, 6.3]
Request: "QA the CustomerPortal access controls, audit for broken object-level authorization."
  CompiledPredicateIds: ["QA", "SECURITY", "AUTHORIZATION"] | SubPhases: [7.3, 6.3, 6.3]
  Routing: QA → 7.3 | SECURITY → 6.3 | AUTHORIZATION → 6.3

## R005-003 — SECURITY + DOCUMENT → [6.3, 6.2]
Request: "Document the threat model for the DataIngestionPipeline — include all attack surfaces."
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | SubPhases: [6.3, 6.2]
  Routing: SECURITY → 6.3 | DOCUMENT → 6.2
  Note: "threat model" fires SECURITY even without "audit" verb — threat modeling is a security-action concept

## R005-004 — SECURITY + IMPLEMENT + API_CONTRACT → [5.2/4.1/3.3]
Request: "Implement rate limiting on the LoginAPI and enforce HTTPS on all endpoints."
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT", "API_CONTRACT"] | SubPhases: [5.2, 4.1, 3.3]
  Routing: SECURITY in implementation context → 5.2 | IMPLEMENT → 4.1 | API_CONTRACT → 3.3

## R005-005 — SECURITY + ARCHITECTURE → [6.3, 3.5]
Request: "Audit the cloud infrastructure security configuration — verify zero-trust network segmentation."
  CompiledPredicateIds: ["SECURITY", "ARCHITECTURE"] | SubPhases: [6.3, 3.5]
  Routing: SECURITY → 6.3 | ARCHITECTURE → 3.5 (infrastructure sub-phase, not app-level 3.1)
