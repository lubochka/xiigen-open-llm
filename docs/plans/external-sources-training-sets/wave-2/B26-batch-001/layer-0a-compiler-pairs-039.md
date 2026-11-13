# Layer 0-A — Compiler Pairs (039) — InjectionAudit
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C039-001 — SQL (Structured Query Language) injection audit → SECURITY

A: "Audit all database query construction methods for SQL injection vulnerabilities."
  "audit" → SECURITY | "SQL injection" → SECURITY (attack vector) | "vulnerabilities" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3 | TrustState: deterministic_generic

---

## C039-002 — LDAP (Lightweight Directory Access Protocol) injection → SECURITY

A: "Review the directory service integration for LDAP injection attack vectors."
  "review" → SECURITY | "LDAP injection" → SECURITY (attack vector)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C039-003 — Command injection scan → SECURITY

A: "Scan all shell command execution paths for OS command injection vulnerabilities."
  "scan" → SECURITY | "command injection" → SECURITY (attack vector) | "vulnerabilities" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3 | TrustState: deterministic_generic

---

## C039-004 — "sanitize inputs" action → SECURITY

A: "Sanitize all user-supplied inputs before passing them to the database query builder."
  "sanitize" → SECURITY (security-action vocabulary)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C039-005 — "update database queries" without injection context → gap

A: "Update all database queries to use the new ORM (Object-Relational Mapping) abstraction layer."
  "update" → gap | "database queries" → gap (no injection/vulnerability vocabulary)
  CompiledPredicateIds: [] → knowledge_edge
