# Held-Out and Corruption Pairs (009)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## H009-001 — Held-out: "mass assignment" fires SECURITY
Request: "Audit the UserProfileController for mass assignment vulnerabilities."
  Correct: ["SECURITY"]
  Evidence: "audit" → SECURITY; "mass assignment vulnerabilities" → SECURITY

## H009-002 — Corruption v6-01: "hardening" alone fires SECURITY (invention)
Corrupt: "Apply configuration hardening to the database server." → ["SECURITY", "IMPLEMENT"]
Correct: ["IMPLEMENT"]
  Fix: "hardening" without "security" co-occurrence is borderline gap for SECURITY. "apply" → IMPLEMENT. Without "security hardening" compound, SECURITY may not fire.
  Conservative ruling: If "security" is not present, "hardening" alone → gap for SECURITY.

## H009-003 — Held-out: "timing attack" fires SECURITY
Request: "Implement constant-time comparison in the TokenValidator to prevent timing attacks."
  Correct: ["SECURITY", "IMPLEMENT"]
  Evidence: "implement" → IMPLEMENT; "constant-time comparison" → SECURITY; "timing attacks" → SECURITY

## H009-004 — Corruption v6-02: QA suppressed in SECURITY+QA request
Corrupt: "QA the security hardening and audit the BillingService." → ["SECURITY"]
Correct: ["QA", "SECURITY"]
  Fix: "QA" abbreviation fires QA. No predicate suppresses it.

## H009-005 — Held-out: "HTTP Authorization header" fires API_CONTRACT not AUTHORIZATION
Request: "Audit the API to confirm all protected endpoints require an Authorization header."
  Correct: ["SECURITY", "API_CONTRACT"]
  Evidence: "audit" → SECURITY; "Authorization header" → API_CONTRACT (HTTP protocol spec)
  Teaching: HTTP header name "Authorization" ≠ AUTHORIZATION predicate.
