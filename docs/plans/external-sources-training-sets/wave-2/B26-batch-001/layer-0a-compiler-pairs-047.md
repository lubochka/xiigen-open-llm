# Layer 0-A — Compiler Pairs (047) — PrivilegeEscalationAudit
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C047-001 — Privilege escalation → SECURITY + AUTHORIZATION (dual)

A: "Audit the role assignment service for privilege escalation paths allowing non-admin users to gain admin access."
  "audit" → SECURITY | "privilege escalation" → SECURITY + AUTHORIZATION (dual-predicate trigger)
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhase: 6.3

---

## C047-002 — Vertical privilege escalation via parameter tampering → SECURITY + AUTHORIZATION

A: "Review the account profile endpoint for vertical privilege escalation via role parameter tampering."
  "review" → SECURITY | "privilege escalation" → SECURITY+AUTHORIZATION | "role" → SECURITY+AUTHORIZATION
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhase: 6.3 | TrustState: deterministic_generic

---

## C047-003 — Horizontal privilege escalation (IDOR) → SECURITY + AUTHORIZATION

A: "Scan all /users/{id} endpoints for IDOR (Insecure Direct Object Reference) horizontal privilege escalation."
  "scan" → SECURITY | "IDOR" → SECURITY+AUTHORIZATION (dual) | "privilege escalation" → SECURITY+AUTHORIZATION
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhase: 6.3 | TrustState: deterministic_generic

---

## C047-004 — "assign admin role to user" → gap (no audit vocabulary)

A: "Assign the admin role to the new onboarded user in the dashboard."
  "assign" → gap | "admin role" → gap (administrative action, no audit/vulnerability vocabulary)
  CompiledPredicateIds: [] → knowledge_edge

---

## C047-005 — Privilege escalation + QA → SECURITY+AUTHORIZATION+QA

A: "Audit privilege escalation vectors across RBAC roles and QA the updated permission boundaries."
  "audit" → SECURITY | "privilege escalation" → SECURITY+AUTHORIZATION | "QA" → QA (literal abbreviation)
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION", "QA"]
