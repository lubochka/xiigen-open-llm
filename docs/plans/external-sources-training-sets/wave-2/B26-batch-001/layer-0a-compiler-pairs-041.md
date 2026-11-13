# Layer 0-A — Compiler Pairs (041) — SSRFAudit
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C041-001 — SSRF (Server-Side Request Forgery) audit → SECURITY

A: "Audit all URL fetch endpoints for SSRF vulnerability allowing access to internal services."
  "audit" → SECURITY | "SSRF" → SECURITY (attack vector) | "vulnerability" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3 | TrustState: deterministic_generic

---

## C041-002 — Cloud metadata endpoint SSRF → SECURITY

A: "Review webhook URL parameters for SSRF paths that reach the AWS (Amazon Web Services) metadata endpoint."
  "review" → SECURITY | "SSRF" → SECURITY | "metadata endpoint" → SECURITY context
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C041-003 — Allowlist enforcement audit → SECURITY

A: "Audit whether outbound HTTP (Hypertext Transfer Protocol) calls enforce an allowlist to prevent SSRF."
  "audit" → SECURITY | "enforce" → SECURITY (enforce = security-action verb) | "SSRF" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3 | TrustState: deterministic_generic

---

## C041-004 — "call external API" → gap

A: "Call the external payment gateway API from the checkout service."
  "call" → gap | "external API" → gap (no SSRF/vulnerability vocabulary)
  CompiledPredicateIds: [] → knowledge_edge

---

## C041-005 — SSRF + IMPLEMENT → IMPLEMENT+SECURITY

A: "Implement an outbound request allowlist and audit existing URL fetch paths for SSRF exposure."
  "implement" → IMPLEMENT | "audit" → SECURITY | "SSRF" → SECURITY
  CompiledPredicateIds: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition
