# Layer 0-B — Router Pairs (048) — SECURITY+AUTHORIZATION Combo Routing Reference
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: router

---

## R048-001 — AUTHORIZATION solo → 6.3

R048-001 test: "Define the RBAC permission matrix for the new document management module."
  AUTHORIZATION → 6.3 (design context) | CompiledPredicateIds: ["AUTHORIZATION"]
  SubPhase: 6.3

---

## R048-002 — IAM design → AUTHORIZATION → 6.3

R048-002 test: "Design the IAM role hierarchy for the microservices cluster."
  AUTHORIZATION → 6.3 | CompiledPredicateIds: ["AUTHORIZATION"] | SubPhase: 6.3

---

## R048-003 — "Authorization header" → API_CONTRACT → 3.1

R048-003 test: "Add the Authorization header to all outbound API calls per the OpenAPI spec."
  API_CONTRACT → 3.1 | NOT AUTHORIZATION predicate
  Teaching: HTTP Authorization header → API_CONTRACT, not AUTHORIZATION predicate.

---

## R048-004 — Wildcard permission audit → 6.3 (SECURITY+AUTHORIZATION)

R048-004 test: "Audit wildcard permission grants in the IAM policy for the production account."
  SECURITY → 6.3 | AUTHORIZATION → 6.3 | Dual predicates, same sub-phase
  TrustState: deterministic_generic

---

## R048-005 — Full SECURITY+AUTHORIZATION combo → 6.3 deterministic_generic

R048-005 test: "Audit the entire permission model for privilege escalation, IDOR, and wildcard permission grants."
  SECURITY → 6.3 | AUTHORIZATION → 6.3 | Three dual-predicate triggers
  TrustState: deterministic_generic
