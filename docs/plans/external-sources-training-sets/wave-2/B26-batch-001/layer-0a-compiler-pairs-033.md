# Layer 0-A — Compiler Pairs (033) — CookieSecurityAudit
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C033-001 — Cookie security audit → SECURITY

A: "Audit all cookies for missing Secure, HttpOnly, and SameSite attributes."
  "audit" → SECURITY | "Secure" attribute → SECURITY | "HttpOnly" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C033-002 — Session cookie review

A: "Review the session cookie configuration for SameSite=None without Secure flag vulnerabilities."
  "Review" + "session cookie" → SECURITY | "vulnerabilities" → SECURITY | "session" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C033-003 — Cookie + token vocabulary

A: "Audit the authentication token storage — verify tokens are stored in HttpOnly cookies and not in localStorage."
  "audit" → SECURITY | "authentication token" → SECURITY (token group) | "HttpOnly cookies" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C033-004 — "set cookie" configuration vs audit

A: "Set the SameSite=Strict attribute on all session cookies in the middleware."
  "Set" → gap (configuration instruction, no audit verb) | "session cookies" → weak signal without audit verb
  CompiledPredicateIds: [] → knowledge_edge
  Teaching: Configuration instructions ("set", "configure") without audit/review vocabulary are gap.

---

## C033-005 — Cookie audit + XSS risk

A: "Scan the cookie handling for cookies accessible to JavaScript — identify XSS exfiltration risks."
  "Scan" → SECURITY | "XSS exfiltration" → SECURITY | "cookies accessible to JavaScript" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3
