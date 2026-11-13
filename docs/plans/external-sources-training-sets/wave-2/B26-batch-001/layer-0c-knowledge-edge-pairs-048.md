# Layer 0-C — Knowledge Edge Pairs (048) — SECURITY+AUTHORIZATION Combo Edge Cases
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E048-001 — AUTHORIZATION fires solo (without SECURITY)

AUTHORIZATION fires alone on:
  "define RBAC roles" → AUTHORIZATION
  "design permission matrix" → AUTHORIZATION
  "configure IAM policies" → AUTHORIZATION (configuration + IAM)
  "role hierarchy design" → AUTHORIZATION
SECURITY does NOT co-fire unless audit/attack-vector vocabulary is present.

---

## E048-002 — SECURITY fires solo (without AUTHORIZATION)

SECURITY fires alone (no AUTHORIZATION) on:
  "audit XSS exposure" → SECURITY only
  "review SSRF vectors" → SECURITY only
  "scan for SQL injection" → SECURITY only
  "check TLS configuration" → SECURITY only
None of these fire AUTHORIZATION because no RBAC/IAM/permission vocabulary is present.

---

## E048-003 — API Authorization header always API_CONTRACT

"Set Authorization: Bearer {token} on all requests" → API_CONTRACT
"Include Authorization header in OpenAPI spec" → API_CONTRACT
"review Authorization header handling" → API_CONTRACT (header spec context)
Never AUTHORIZATION predicate for HTTP header usage.

---

## E048-004 — Gap vocabulary specific to AUTHORIZATION

Always gap for AUTHORIZATION:
  "authentication" alone (without RBAC/permission boundary vocabulary)
  "login" / "logout" (session events, not access-control definitions)
  "user account" (user management, not permission boundary)
  "user roles list" without permission scope definition

---

## E048-005 — Combined request trust state

SECURITY alone → deterministic_generic (with audit verb)
AUTHORIZATION alone → deterministic_generic (with IAM/RBAC vocabulary)
SECURITY + AUTHORIZATION together → deterministic_generic (highest confidence, both predicates confirmed)
