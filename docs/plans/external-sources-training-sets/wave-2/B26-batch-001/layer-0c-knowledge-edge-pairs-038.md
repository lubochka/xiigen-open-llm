# Layer 0-C — Knowledge Edge Pairs (038) — ClickjackingAudit Edge Cases
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E038-001 — Clickjacking vocabulary fires SECURITY

"clickjacking" → SECURITY (attack vector)
"X-Frame-Options" → SECURITY (security header vocabulary)
"frame-ancestors" in CSP context → SECURITY
"allow iframe embedding" → gap (no security-action vocabulary)

---

## E038-002 — X-Frame-Options value boundary

"Set X-Frame-Options to DENY" → gap (configuration instruction, no audit verb)
"audit X-Frame-Options configuration" → SECURITY (audit verb present)
"review X-Frame-Options settings" → SECURITY (review verb with security header)
Teaching: Header name alone does not fire SECURITY. The action verb determines firing.

---

## E038-003 — CSP frame-ancestors vs CSP general

"audit CSP frame-ancestors for clickjacking" → SECURITY (two triggers: CSP + clickjacking)
"review CSP headers" → SECURITY (review + CSP = security action)
"add frame-ancestors to CSP" → gap (add action without audit vocabulary)

---

## E038-004 — Iframe embedding context disambiguation

"embed the widget in an iframe" → gap (integration task)
"audit whether pages can be embedded in iframes by third parties" → SECURITY (audit verb)
Teaching: Embedding a widget is gap; auditing whether embedding is permitted is SECURITY.

---

## E038-005 — Trust state calibration

Clickjacking alone + audit verb → deterministic_generic
X-Frame-Options alone + review → probationary_accepted
Clickjacking + CSP + X-Frame-Options audit → deterministic_generic (high)
