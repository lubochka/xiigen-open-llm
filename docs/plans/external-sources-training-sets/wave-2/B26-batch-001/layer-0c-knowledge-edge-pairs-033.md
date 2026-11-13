# Layer 0-C — Knowledge Edge Pairs (033) — CookieSecurityAudit Knowledge Edge
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E033-001 — Cookie security vocabulary that fires SECURITY

"Secure flag" on cookies → SECURITY (security attribute)
"HttpOnly" flag → SECURITY (cookie security vocabulary)
"SameSite" attribute → SECURITY (CSRF-prevention vocabulary)
"session cookie" → SECURITY (session fires)
"authentication token in cookie" → SECURITY (token fires)
"cookie exfiltration via XSS" → SECURITY (XSS fires)

---

## E033-002 — Cookie configuration tasks (gap vocabulary)

"Set SameSite=Strict on cookies" → gap (configuration, no audit verb)
"Configure cookie expiry" → gap
"Add HttpOnly flag in middleware" → gap (configuration, no audit)
Teaching: Setting cookie attributes without vulnerability/audit context is always gap.

---

## E033-003 — Cookie + session + XSS risk intersection

"session" alone → SECURITY (fires independently)
"token" alone → SECURITY (fires independently)
"cookies accessible to JavaScript" → SECURITY (XSS exfiltration risk)
"SameSite=None without Secure" → SECURITY (cookie vulnerability pattern)

---

## E033-004 — localStorage vs cookie token storage

"Tokens in localStorage" → SECURITY (token storage vulnerability)
"Tokens in HttpOnly cookies" → SECURITY (correct pattern, but audit context fires SECURITY)
Teaching: Both patterns fire SECURITY when in audit context — auditing token storage is SECURITY regardless of verdict.

---

## E033-005 — Trust state for cookie audits

"Audit cookies for missing Secure, HttpOnly, SameSite attributes" → deterministic_generic (3 triggers + audit)
"Review session cookie configuration" → probationary_accepted
"Set SameSite=Strict" → knowledge_edge (zero predicates)
