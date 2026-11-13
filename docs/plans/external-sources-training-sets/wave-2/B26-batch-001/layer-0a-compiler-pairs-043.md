# Layer 0-A — Compiler Pairs (043) — TimingAttackAudit
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C043-001 — Timing attack in password comparison → SECURITY

A: "Audit the password comparison logic for timing attack vulnerabilities due to early-exit string comparison."
  "audit" → SECURITY | "vulnerability" → SECURITY | "password comparison" → SECURITY (credential vocabulary)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C043-002 — HMAC (Hash-based Message Authentication Code) verification timing → SECURITY

A: "Review HMAC signature verification for constant-time comparison enforcement."
  "review" → SECURITY | "HMAC" → SECURITY (cryptographic group) | "signature verification" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C043-003 — User enumeration via timing → SECURITY

A: "Audit login response timing for user enumeration via differential response latency."
  "audit" → SECURITY | "user enumeration" → SECURITY (vulnerability pattern)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C043-004 — "optimize login latency" → gap (no security vocabulary)

A: "Optimize the login endpoint response latency for better user experience."
  "optimize" → gap | "latency" → gap (performance context, no security-action vocabulary)
  CompiledPredicateIds: [] → knowledge_edge

---

## C043-005 — Constant-time harden → SECURITY

A: "Harden the token comparison function by replacing string equality with a constant-time comparison algorithm."
  "harden" → SECURITY | "token comparison" → SECURITY (auth vocabulary)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3
