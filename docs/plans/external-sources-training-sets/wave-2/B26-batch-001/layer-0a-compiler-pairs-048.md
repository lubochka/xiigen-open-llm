# Layer 0-A — Compiler Pairs (048) — SECURITY+AUTHORIZATION Combo Reference
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C048-001 — Complete SECURITY+AUTHORIZATION dual-trigger vocabulary table

Vocabulary that fires BOTH predicates simultaneously (any single term sufficient):
  "privilege escalation" → SECURITY + AUTHORIZATION
  "IDOR" (Insecure Direct Object Reference) → SECURITY + AUTHORIZATION
  "wildcard permission" → SECURITY + AUTHORIZATION
  "RBAC bypass" → SECURITY + AUTHORIZATION
  "role claim forgery" → SECURITY + AUTHORIZATION
  "unauthorized resource access" → SECURITY + AUTHORIZATION

A: Reference-only entry. No A/B pair.

---

## C048-002 — AUTHORIZATION solo (no SECURITY co-occurrence)

A: "Define the RBAC permission matrix for the new document management module."
  "RBAC" → AUTHORIZATION (definition task, no audit/attack-vector vocabulary)
  "Define" → design action, not audit
  CompiledPredicateIds: ["AUTHORIZATION"] | SubPhase: 6.3

---

## C048-003 — AUTHORIZATION fires on "access control" in design context

A: "Design the IAM (Identity and Access Management) role hierarchy for the microservices cluster."
  "IAM" → AUTHORIZATION | "access control" design context → AUTHORIZATION
  CompiledPredicateIds: ["AUTHORIZATION"] | SubPhase: 6.3

---

## C048-004 — "authorization header" → API_CONTRACT not AUTHORIZATION

A: "Add the Authorization header to all outbound API calls per the OpenAPI (Open Application Programming Interface) spec."
  "Authorization header" → API_CONTRACT (HTTP header specification context)
  Not AUTHORIZATION — this is an HTTP protocol element, not an access control definition.
  CompiledPredicateIds: ["API_CONTRACT"] | SubPhase: 3.1
  Teaching: "Authorization" as an HTTP header fires API_CONTRACT; AUTHORIZATION fires on RBAC/IAM/permissions vocabulary.

---

## C048-005 — SECURITY+AUTHORIZATION stacking: full audit request

A: "Audit the entire permission model for privilege escalation, IDOR, and wildcard permission grants across all RBAC roles."
  "audit" → SECURITY | "privilege escalation" → SECURITY+AUTHORIZATION | "IDOR" → SECURITY+AUTHORIZATION | "wildcard permission" → SECURITY+AUTHORIZATION | "RBAC" → AUTHORIZATION
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhase: 6.3 | TrustState: deterministic_generic
