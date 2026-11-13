# Layer 0-A — Compiler Pairs (030) — OWASPTop10Review
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C030-001 — OWASP (Open Web Application Security Project) Top 10 as context label

A: "Review the application against the OWASP Top 10 (Open Web Application Security Project) list."
  "Review" → borderline; "OWASP Top 10" as label → does not fire alone
  "Review" + "OWASP Top 10" → SECURITY fires because OWASP Top 10 is an explicit vulnerability-reference label
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3
  Note: OWASP Top 10 is a strong enough security-vocabulary co-occurrence to push "review" past the gap threshold.

---

## C030-002 — OWASP A01: Broken Access Control + AUTHORIZATION

A: "Audit the application for OWASP A01 (Broken Access Control) — check for IDOR and privilege escalation."
  "audit" → SECURITY | "IDOR" → SECURITY+AUTHORIZATION | "privilege escalation" → SECURITY+AUTHORIZATION
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhase: 6.3

---

## C030-003 — OWASP A02: Cryptographic Failures

A: "Audit the cipher suites and certificate validation for OWASP A02 (Cryptographic Failures)."
  "audit" → SECURITY | "cipher suites" → SECURITY | "certificate validation" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C030-004 — OWASP A03: Injection audit

A: "Scan the query construction code for SQL injection and XSS vulnerabilities per OWASP A03."
  "Scan" (security) → SECURITY | "SQL injection" → SECURITY | "XSS" → SECURITY | "vulnerabilities" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C030-005 — OWASP "make compliant" is gap

A: "Make the application OWASP Top 10 compliant."
  "make compliant" → gap (no specific technical action verb) | "OWASP Top 10 compliant" → gap
  CompiledPredicateIds: [] → knowledge_edge
  Teaching: "make compliant" with any framework label is always gap.
