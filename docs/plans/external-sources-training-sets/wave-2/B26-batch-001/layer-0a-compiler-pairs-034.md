# Layer 0-A — Compiler Pairs (034) — SessionFixationAudit
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C034-001 — Session fixation audit → SECURITY

A: "Audit the login flow for session fixation vulnerabilities — verify the session ID regenerates after authentication."
  "audit" → SECURITY | "session fixation" → SECURITY (session attack vector) | "vulnerabilities" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C034-002 — Session vocabulary fires SECURITY

A: "Review the session management implementation for predictable session IDs and fixation attack vectors."
  "Review" + "session management" → SECURITY | "session IDs" → SECURITY | "fixation attack" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C034-003 — Session + token + CSRF boundary

A: "Audit the session token lifecycle: creation, rotation on elevation, and invalidation on logout."
  "audit" → SECURITY | "session token" → SECURITY | "rotation on elevation" → SECURITY
  "invalidation on logout" → SECURITY (session security)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C034-004 — Session expiry without security vocabulary

A: "Set the session timeout to 30 minutes for inactive users."
  "Set" → gap | "session timeout" → borderline; without audit/vulnerability vocabulary → gap
  CompiledPredicateIds: [] → knowledge_edge
  Teaching: "set session timeout" is a configuration task. "audit session timeout for security gaps" → SECURITY.

---

## C034-005 — Session + authentication bypass

A: "Audit the session creation path for authentication bypass via pre-authenticated session tokens."
  "audit" → SECURITY | "authentication bypass" → SECURITY | "pre-authenticated session tokens" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3
