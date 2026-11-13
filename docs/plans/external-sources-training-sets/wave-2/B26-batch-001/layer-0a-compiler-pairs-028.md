# Layer 0-A — Compiler Pairs (028) — CISBenchmark
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C028-001 — CIS (Center for Internet Security) Benchmark: gap label vs SECURITY trigger

A: "Complete the CIS Benchmark Level 2 assessment for the Linux hosts."
  "CIS Benchmark" → gap (compliance framework label) | "assessment" (noun event) → gap
  CompiledPredicateIds: [] → knowledge_edge

---

## C028-002 — CIS Benchmark + audit verb → SECURITY

A: "Audit the server hardening configuration against the CIS Benchmark Level 2 profile."
  "audit" (verb) → SECURITY | "server hardening" → SECURITY (hardening is a security action)
  "CIS Benchmark Level 2" → gap context label
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C028-003 — CIS Benchmark + SSH and TLS audit

A: "Review the SSH key authentication settings and TLS protocol versions against CIS controls."
  "Review" + "SSH key authentication" → SECURITY | "TLS protocol versions" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C028-004 — CIS Benchmark + vulnerability scan

A: "Scan the infrastructure for CIS Benchmark violations and report on open vulnerabilities."
  "Scan" (security context) → SECURITY | "vulnerabilities" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C028-005 — "harden" verb fires SECURITY independently

A: "Harden the API gateway configuration and audit the exposed endpoints for injection vulnerabilities."
  "harden" → SECURITY | "audit" → SECURITY | "injection vulnerabilities" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3
  Teaching: "harden" is a standalone SECURITY trigger equal to "audit" in security-action context.
