# Layer 0-A — Compiler Pairs (031) — CSPHeaderAudit
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C031-001 — CSP (Content Security Policy) header audit → SECURITY

A: "Audit the Content Security Policy (CSP) headers for unsafe-inline and unsafe-eval directives."
  "audit" (verb) → SECURITY | "Content Security Policy" → SECURITY (security header vocabulary)
  "unsafe-inline" → SECURITY | "unsafe-eval" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C031-002 — CSP misconfiguration as vulnerability

A: "Review the CSP header configuration and identify any overly permissive script-src directives that allow XSS."
  "Review" + "CSP header" → SECURITY | "XSS" → SECURITY | "overly permissive" → security context
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C031-003 — HTTP security header audit

A: "Scan all HTTP security headers: CSP, HSTS (HTTP Strict Transport Security), X-Frame-Options, and X-Content-Type-Options."
  "Scan" → SECURITY | "HTTP security headers" → SECURITY | "HSTS" → SECURITY (TLS-related)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C031-004 — "add CSP header" configuration gap

A: "Add a Content Security Policy header to the API gateway response."
  "add" → gap action (configuration task, not audit/review) | "Content Security Policy header" → context
  CompiledPredicateIds: [] → knowledge_edge
  Teaching: Configuration tasks ("add", "set", "configure") without vulnerability vocabulary are gap.

---

## C031-005 — CSP + implement + audit → IMPLEMENT+SECURITY

A: "Implement a strict CSP header policy and audit the existing directives for XSS bypass vectors."
  "Implement" → IMPLEMENT | "audit" → SECURITY | "XSS bypass vectors" → SECURITY
  CompiledPredicateIds: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition
