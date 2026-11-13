# Layer 0-C — Knowledge Edge Pairs (031) — CSPHeaderAudit Knowledge Edge
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E031-001 — CSP vocabulary that fires SECURITY

"Content Security Policy" → SECURITY (security header vocabulary)
"unsafe-inline" / "unsafe-eval" directives → SECURITY (XSS bypass vectors)
"script-src" permissive rules → SECURITY (XSS risk vocabulary)
"HSTS" (HTTP Strict Transport Security) → SECURITY (TLS-related header)
"X-Frame-Options" → SECURITY (clickjacking defense header)
"CSP misconfiguration" → SECURITY

---

## E031-002 — Configuration vs audit distinction for HTTP headers

"Audit CSP headers for unsafe directives" → SECURITY (audit verb + CSP)
"Add a CSP header" → gap (configuration action, no audit)
"Set X-Frame-Options: DENY" → gap (configuration instruction)
Teaching: Header configuration without audit vocabulary is always gap.

---

## E031-003 — CSP + XSS co-occurrence

"CSP" alone → SECURITY (strong security noun)
"CSP" + "XSS" → SECURITY with higher confidence (two independent triggers)
"CSP" + "audit" + "XSS" → deterministic_generic

---

## E031-004 — HTTP security header audit scope

HSTS → fires SECURITY (TLS-redirect header)
Referrer-Policy → borderline; needs "information disclosure" or security co-occurrence
Permissions-Policy → borderline; needs "XSS" or "CSRF" co-occurrence
X-Content-Type-Options → SECURITY (MIME-sniffing attack vector)

---

## E031-005 — Trust state for CSP audits

"Audit CSP headers for unsafe-inline, HSTS, and XSS bypass vectors" → deterministic_generic
"Review CSP configuration" → probationary_accepted (review alone, CSP = moderate signal)
"Add CSP header" → knowledge_edge (zero predicates — configuration)
