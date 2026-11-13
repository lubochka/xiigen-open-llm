# Layer 0-B — Router Pairs (031) — CSPHeaderAudit Routing
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: router

---

## R031-001 — CSP audit → SECURITY → 6.3

R031-001 test: "Audit CSP headers for unsafe-inline and unsafe-eval directives."
  SECURITY → 6.3 (audit + CSP + unsafe directives) | CompiledPredicateIds: ["SECURITY"]

---

## R031-002 — CSP + XSS → SECURITY → 6.3

R031-002 test: "Review CSP configuration for permissive script-src directives that allow XSS."
  SECURITY → 6.3 (review + CSP + XSS) | CompiledPredicateIds: ["SECURITY"]

---

## R031-003 — HTTP security headers scan → SECURITY → 6.3

R031-003 test: "Scan all HTTP security headers: CSP, HSTS, X-Frame-Options."
  SECURITY → 6.3 (scan + security headers + HSTS/TLS-related) | CompiledPredicateIds: ["SECURITY"]

---

## R031-004 — "add CSP header" → GapHandler

R031-004 test: "Add a Content Security Policy header to the API gateway."
  CompiledPredicateIds: [] → knowledge_edge (configuration task, no audit/vulnerability vocabulary)
  GapHandler: SCHEDULE_CAPABILITY_LEARNING
  Teaching: "add header" without audit verb → gap.

---

## R031-005 — CSP + IMPLEMENT + SECURITY → dual routing

R031-005 test: "Implement strict CSP and audit existing directives for XSS bypass vectors."
  IMPLEMENT → 4.1 | SECURITY → 6.3 | TransitionType: feature_addition (IMPLEMENT present)
  CompiledPredicateIds: ["IMPLEMENT", "SECURITY"]
