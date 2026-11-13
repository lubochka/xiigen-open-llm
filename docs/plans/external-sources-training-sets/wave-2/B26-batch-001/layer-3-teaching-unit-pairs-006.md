# Layer 3 — Teaching Unit Pairs (006)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU006-001 — Teach: "key rotation" fires SECURITY
  "Implement key rotation for the encryption service." → ["SECURITY", "IMPLEMENT"]
  "Audit key rotation policies." → ["SECURITY"]
  Rule: Cryptographic key management vocabulary (key rotation, key length, signing key) fires SECURITY.

## TU006-002 — Teach: "timing attack" and "replay attack" fire SECURITY
  "Implement constant-time comparison to prevent timing attacks." → ["SECURITY", "IMPLEMENT"]
  "Audit for token replay vulnerabilities." → ["SECURITY"]
  Rule: Attack vector names (timing attack, replay attack, MITM, side-channel) fire SECURITY.

## TU006-003 — Teach: "AES", "RSA", "bcrypt" fire SECURITY in security context
  "Upgrade to AES-256." → ["SECURITY", "IMPLEMENT"]
  "Audit password hashing — confirm bcrypt cost factor ≥12." → ["SECURITY"]
  Rule: Cryptographic algorithm names fire SECURITY in enforcement/audit contexts.

## TU006-004 — Teach: "2FA"/"MFA"/"two-factor authentication" fire SECURITY
  "Implement 2FA for the admin panel." → ["SECURITY", "IMPLEMENT"]
  "Audit MFA enforcement on privileged accounts." → ["SECURITY"]
  Rule: Multi-factor authentication vocabulary fires SECURITY.

## TU006-005 — Teach: "nonce" and "CSRF token" fire SECURITY
  "Verify CSRF tokens are present on all state-changing requests." → ["TEST", "SECURITY"]
  "Generate a unique nonce for each request." → ["IMPLEMENT", "SECURITY"]
  Rule: Request forgery protection vocabulary (CSRF token, nonce, SameSite cookie) fires SECURITY.
