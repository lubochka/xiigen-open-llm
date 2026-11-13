# Layer 0-A — Compiler Pairs (037) — CSRFProtection
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C037-001 — CSRF (Cross-Site Request Forgery) token audit → SECURITY

A: "Audit all state-changing endpoints for missing CSRF token validation."
  "audit" (verb) → SECURITY | "CSRF token" → SECURITY (attack vector)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C037-002 — CSRF + SameSite cookie → SECURITY

A: "Review the SameSite cookie attribute configuration to prevent CSRF attacks."
  "review" → SECURITY | "SameSite" → SECURITY (cookie security) | "CSRF" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3 | TrustState: deterministic_generic

---

## C037-003 — "implement CSRF protection" → IMPLEMENT+SECURITY

A: "Implement double-submit cookie CSRF protection on the payment checkout flow."
  "implement" → IMPLEMENT | "CSRF protection" → SECURITY
  CompiledPredicateIds: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition

---

## C037-004 — "CSRF" standalone noun → SECURITY fires

A: "Check whether CSRF tokens are validated on all POST, PUT, and DELETE routes."
  "CSRF" → SECURITY (attack vector solo noun) | "validated" → SECURITY (security action)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C037-005 — "configure CSRF middleware" → gap (no audit vocabulary)

A: "Configure the CSRF middleware in the ASP.NET (Active Server Pages .NET) pipeline."
  "configure" → gap | "CSRF middleware" → gap (configuration action, no audit verb)
  CompiledPredicateIds: [] → knowledge_edge
  Teaching: Configuration tasks without audit/review/scan → always gap.
