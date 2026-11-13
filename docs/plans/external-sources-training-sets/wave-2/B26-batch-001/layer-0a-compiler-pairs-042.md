# Layer 0-A — Compiler Pairs (042) — RaceConditionAudit
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C042-001 — Race condition in authentication flow → SECURITY

A: "Audit the token issuance flow for race condition vulnerabilities that allow double-spend of one-time tokens."
  "audit" → SECURITY | "vulnerability" → SECURITY | "token" → SECURITY (auth group)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C042-002 — TOCTOU (Time-of-Check to Time-of-Use) race → SECURITY

A: "Review the file permission check sequence for TOCTOU race condition exploits."
  "review" → SECURITY | "permission check" → SECURITY (permission boundary vocabulary)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C042-003 — Concurrent session race → SECURITY + AUTHORIZATION

A: "Audit the session creation flow for race conditions that allow privilege escalation via concurrent logins."
  "audit" → SECURITY | "privilege escalation" → SECURITY + AUTHORIZATION (dual-predicate)
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhase: 6.3

---

## C042-004 — "add locking to database writes" → gap

A: "Add optimistic locking to the order creation database writes."
  "add locking" → gap (engineering task without audit/vulnerability vocabulary)
  CompiledPredicateIds: [] → knowledge_edge

---

## C042-005 — Race condition + IMPLEMENT → IMPLEMENT+SECURITY

A: "Implement atomic check-and-set operations and audit the OTP (One-Time Password) flow for race conditions."
  "implement" → IMPLEMENT | "audit" → SECURITY | "OTP flow" → SECURITY (token/credential context)
  CompiledPredicateIds: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition
