# Layer 0-B — Router Pairs (033) — CookieSecurityAudit Routing
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: router

---

## R033-001 — Cookie security audit → SECURITY → 6.3

R033-001 test: "Audit all cookies for missing Secure, HttpOnly, and SameSite attributes."
  SECURITY → 6.3 (audit + Secure + HttpOnly) | CompiledPredicateIds: ["SECURITY"]

---

## R033-002 — Session cookie + vulnerability → SECURITY → 6.3

R033-002 test: "Review session cookie configuration for SameSite=None without Secure flag vulnerabilities."
  SECURITY → 6.3 (review + session + vulnerabilities) | CompiledPredicateIds: ["SECURITY"]

---

## R033-003 — Authentication token storage → SECURITY → 6.3

R033-003 test: "Audit authentication token storage — verify tokens in HttpOnly cookies, not localStorage."
  SECURITY → 6.3 (audit + authentication token) | CompiledPredicateIds: ["SECURITY"]

---

## R033-004 — "set cookie attribute" configuration → GapHandler

R033-004 test: "Set SameSite=Strict on all session cookies in the middleware."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING
  Teaching: "set attribute" without audit vocabulary → gap.

---

## R033-005 — Cookie + XSS exfiltration → SECURITY → 6.3

R033-005 test: "Scan cookie handling for cookies accessible to JavaScript — identify XSS exfiltration risks."
  SECURITY → 6.3 (scan + XSS exfiltration) | CompiledPredicateIds: ["SECURITY"]
