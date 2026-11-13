# Layer 3 — Teaching Unit Pairs (010)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU010-001 — Teach: "HTTP Authorization header" fires API_CONTRACT not AUTHORIZATION
  "Confirm all endpoints require an Authorization header." → ["SECURITY", "API_CONTRACT"]
  Rule: The HTTP header name "Authorization" fires API_CONTRACT (protocol spec). AUTHORIZATION fires on role/permission vocabulary.

## TU010-002 — Teach: "information disclosure" fires SECURITY
  "Scan for information disclosure in error responses." → ["SECURITY"]
  Rule: "information disclosure" is an OWASP vulnerability class — fires SECURITY.

## TU010-003 — Teach: "secret" fires SECURITY in cryptographic context; gap in business context
  "Rotate the secret keys." → ["SECURITY"] — cryptographic context
  "This is a trade secret." → [] (gap) — business context
  Rule: Cryptographic "secret" (secret key, secret management, secrets) fires SECURITY. Business secrets are gap.

## TU010-004 — Teach: "phishing awareness training" is gap (organizational, not code security)
  "Run phishing awareness training for the team." → []
  "Implement phishing URL detection in the EmailService." → ["SECURITY", "IMPLEMENT"]
  Rule: Security training/culture activities are gap. Code-level security implementations fire SECURITY.

## TU010-005 — Teach: "security culture" and "security awareness" are gap
  "Build a security-first culture." → [] | "Raise security awareness." → []
  Rule: Organizational/cultural security initiatives are gap. SECURITY fires on technical security-action vocabulary only.
