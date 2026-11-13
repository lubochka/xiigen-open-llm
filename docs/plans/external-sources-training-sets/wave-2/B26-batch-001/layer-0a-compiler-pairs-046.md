# Layer 0-A — Compiler Pairs (046) — BrokenAuthAudit
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C046-001 — Broken authentication audit → SECURITY

A: "Audit the authentication flow for broken authentication patterns including credential stuffing exposure."
  "audit" → SECURITY | "authentication" → SECURITY (auth group) | "credential stuffing" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3 | TrustState: deterministic_generic

---

## C046-002 — MFA (Multi-Factor Authentication) bypass → SECURITY

A: "Review the MFA implementation for bypass vulnerabilities via fallback authentication paths."
  "review" → SECURITY | "MFA" → SECURITY (auth group solo noun) | "vulnerabilities" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3 | TrustState: deterministic_generic

---

## C046-003 — Session token entropy audit → SECURITY

A: "Audit session token entropy and expiry to prevent brute-force session hijacking."
  "audit" → SECURITY | "session token" → SECURITY (token = auth group) | "brute-force" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3 | TrustState: deterministic_generic

---

## C046-004 — "add login page" → gap (no auth-audit vocabulary)

A: "Add a login page for the new admin section with username and password fields."
  "add" → gap | "login page" → gap (feature addition without auth-audit vocabulary)
  CompiledPredicateIds: [] → knowledge_edge

---

## C046-005 — Broken auth + AUTHORIZATION → SECURITY+AUTHORIZATION

A: "Audit broken authentication paths that allow bypassing RBAC (Role-Based Access Control) authorization gates."
  "audit" → SECURITY | "RBAC" → SECURITY+AUTHORIZATION (dual) | "bypassing" → SECURITY
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhase: 6.3
