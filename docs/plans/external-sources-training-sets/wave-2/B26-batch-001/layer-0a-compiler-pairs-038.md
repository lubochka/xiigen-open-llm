# Layer 0-A — Compiler Pairs (038) — ClickjackingAudit
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C038-001 — Clickjacking audit via X-Frame-Options header → SECURITY

A: "Audit whether X-Frame-Options or CSP (Content Security Policy) frame-ancestors are set on all pages."
  "audit" → SECURITY | "X-Frame-Options" → SECURITY (security header) | "CSP" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C038-002 — Frame-ancestors directive review → SECURITY

A: "Review CSP frame-ancestors directive for clickjacking exposure across the admin portal."
  "review" → SECURITY | "CSP frame-ancestors" → SECURITY | "clickjacking" → SECURITY (attack vector)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3 | TrustState: deterministic_generic

---

## C038-003 — "protect against clickjacking" → SECURITY

A: "Harden the login page against clickjacking by enforcing SAMEORIGIN frame policy."
  "harden" → SECURITY | "clickjacking" → SECURITY (attack vector) | "enforcing" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C038-004 — Iframe embedding gap: no audit vocabulary

A: "Allow the dashboard to be embedded in the partner portal iframe."
  "allow" → gap | "embedded" → gap | "iframe" → gap (no audit/security-action vocabulary)
  CompiledPredicateIds: [] → knowledge_edge

---

## C038-005 — Clickjacking + document → SECURITY+DOCUMENT

A: "Audit clickjacking exposure on all authenticated pages and document the X-Frame-Options findings."
  "audit" → SECURITY | "clickjacking" → SECURITY | "document" → DOCUMENT
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | TransitionType: documentation
