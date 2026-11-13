# Layer 0-A — Compiler Pairs (022)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: compiler

---

## C022-001 — SECURITY+QA: access control audit

A: "Audit the RBAC (Role-Based Access Control) configuration for all AdminPanel endpoints and QA the permission matrix."
  "audit" → SECURITY; "RBAC" → SECURITY (rbac trigger); "access control" → SECURITY; "QA" → QA; "permission matrix" → SECURITY context
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhase: 6.3/7.3

---

## C022-002 — SECURITY+AUTHORIZATION: privilege escalation review

A: "Review the AdminService for privilege escalation paths — can a standard user elevate to admin via API parameter manipulation?"
  "review" (security context) → SECURITY; "privilege escalation" → SECURITY+AUTHORIZATION; "API parameter manipulation" → SECURITY
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhase: 6.3
  Teaching: "privilege escalation" fires both SECURITY and AUTHORIZATION per predicate rules.

---

## C022-003 — SECURITY+DOCUMENT: access control matrix documented

A: "Audit the access control enforcement points across all microservices and document all unenforced authorization boundaries."
  "audit" → SECURITY; "access control enforcement" → SECURITY; "authorization boundaries" → AUTHORIZATION; "document" → DOCUMENT
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION", "DOCUMENT"] | SubPhase: 6.3/6.2

---

## C022-004 — Gap: "user permissions" in product context is gap without audit vocabulary

A: "Allow users to manage their own team permissions from the settings page."
  "allow users" → gap; "manage permissions" → gap (product feature request, no audit vocabulary)
  CompiledPredicateIds: [] → knowledge_edge
  Gap: "permissions" in a product-feature context is not AUTHORIZATION. Audit/enforcement vocabulary is required.

---

## C022-005 — SECURITY+QA: default-deny posture audit + QA

A: "Audit the AuthorizationMiddleware for default-deny posture — confirm that new routes require explicit [Authorize] annotation — and QA the coverage of unauthenticated access paths."
  "audit" → SECURITY; "AuthorizationMiddleware" → AUTHORIZATION; "default-deny" → SECURITY; "unauthenticated access" → SECURITY; "QA" → QA
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION", "QA"] | SubPhase: 6.3/7.3
