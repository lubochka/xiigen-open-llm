# Layer 0-C — Knowledge Edge Pairs (039) — InjectionAudit Edge Cases
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E039-001 — Injection vocabulary fires SECURITY

"SQL injection" → SECURITY (attack vector)
"LDAP injection" → SECURITY (attack vector)
"OS command injection" → SECURITY (attack vector)
"NoSQL injection" → SECURITY (attack vector)
"injection" in code-quality context (without attack reference) → gap

---

## E039-002 — "sanitize" fires SECURITY

"sanitize user inputs" → SECURITY (security-action vocabulary: sanitize)
"sanitize HTML output" → SECURITY (sanitize = security action)
"clean the data" → gap (vague action, no security vocabulary)
Teaching: "sanitize" is a direct SECURITY trigger; "clean" is not.

---

## E039-003 — ORM migration boundary

"migrate queries to ORM" → gap (engineering migration task)
"audit whether ORM parameterization prevents SQL injection" → SECURITY (audit + SQL injection)
"update queries to use parameterized statements" → gap (refactoring task, no audit vocabulary)

---

## E039-004 — Multiple injection types stacking

"SQL injection + LDAP injection + command injection" in one audit → SECURITY (three independent triggers)
TrustState: deterministic_generic (three attack-vector triggers in audit context)

---

## E039-005 — "injection" ambiguity

"dependency injection" → gap (design pattern, not attack vector)
"constructor injection" → gap (IoC (Inversion of Control) pattern)
"SQL injection" → SECURITY (always an attack vector)
Teaching: "injection" alone is ambiguous. "SQL/LDAP/command/OS injection" always fires SECURITY.
